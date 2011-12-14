/*
* sones GraphDB - Community Edition - http://www.sones.com
* Copyright (C) 2007-2011 sones GmbH
*
* This file is part of sones GraphDB Community Edition.
*
* sones GraphDB is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published by
* the Free Software Foundation, version 3 of the License.
* 
* sones GraphDB is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with sones GraphDB. If not, see <http://www.gnu.org/licenses/>.
* 
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using sones.GraphDB;
using sones.GraphQL;
using sones.GraphQL.Result;
using sones.Library.Commons.Security;
using sones.Library.Commons.Transaction;
using sones.Library.ErrorHandling;
using sones.Library.VersionedPluginManager;
using sones.Plugins.SonesGQL.DBImport.ErrorHandling;
using sones.Library.DataStructures;
using sones.Plugins.SonesGQL.DBImport;
using System.Threading.Tasks;
using System.Threading;
using sones.Library.LanguageExtensions;
using sones.Library.SlimLogFramework;
using sones.Library.SlimLogFramework.Logger;

namespace sones.Plugins.SonesGQL
{
    public sealed class GraphDBImport_GQL : IGraphDBImport
    {
        private long _numberOfStatements = 0;
		
		 /// <summary>
		/// The ILogger instance, that can be used to log information.
		/// </summary>
		private ILogger _logger;

		/// <summary>
		/// The path to the log location.
		/// </summary>
		private string _logpath;
		
        #region constructor

        public GraphDBImport_GQL ()
		{ 
			_logpath = Path.Combine (".gqlImport_" + Guid.NewGuid().ToString() + ".log");
		}

        #endregion

        #region IGraphDBImport Members

        public IEnumerable<IVertexView> Import(String myLocation,
            IGraphDB myGraphDB,
            IGraphQL myGraphQL,
            SecurityToken mySecurityToken,
            Int64 myTransactionToken,
            UInt32 myParallelTasks = 1U,
            IEnumerable<string> myComments = null,
            UInt64? myOffset = null,
            UInt64? myLimit = null,
            VerbosityTypes myVerbosityType = VerbosityTypes.Silent,
            Dictionary<string, string> myOptions = null)
        {
            Stream stream = null;

            #region Read querie lines from location

            try
            {
				//if verbositiy is silent, we do not configure the logger, so it is an empty logger.
				if (myVerbosityType != VerbosityTypes.Silent) {
					Level logLevel = (myVerbosityType == VerbosityTypes.Full)
                        ? Level.FINE
                        : Level.INFO;
					LogManager.Instance.ConfigureLogger ("GqlImport", new FileLogger (_logpath, logLevel));
				}

				//store some arguments as fields, because there is at most one execution at any time.
				_logger = LogManager.Instance.GetLogger ("GqlImport");
				
                #region file
                if (myLocation.ToLower().StartsWith(@"file:\\"))
                {
                    //lines = ReadFile(location.Substring(@"file:\\".Length));
                    stream = GetStreamFromFile(myLocation.Substring(@"file:\\".Length));
                }
                #endregion
                #region http
                else if (myLocation.ToLower().StartsWith("http://"))
                {
                    stream = GetStreamFromHttp(myLocation);
                }
                #endregion
                else
                {
                    throw new InvalidImportLocationException(myLocation, @"file:\\", "http://");
                }

                #region Start import using the AGraphDBImport implementation and return the result

                Import(stream, myGraphDB, myGraphQL, mySecurityToken, myTransactionToken, myParallelTasks, myComments, myOffset, myLimit, myVerbosityType);

                return new VertexView(new Dictionary<string, object> { { "Number of import statements:", _numberOfStatements } }, null).SingleEnumerable();

                #endregion
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }

            #endregion
        }

        private void Import (Stream myInputStream, IGraphDB myIGraphDB, IGraphQL myGraphQL, SecurityToken mySecurityToken, Int64 myTransactionToken, UInt32 myParallelTasks = 1U, IEnumerable<string> myComments = null, ulong? myOffset = null, ulong? myLimit = null, VerbosityTypes myVerbosityType = VerbosityTypes.Silent)
		{
			var lines = ReadLinesFromStream (myInputStream);

            #region Evaluate Limit and Offset

			if (myOffset != null) {
				if (lines != null)
					lines = lines.Skip<String> (Convert.ToInt32 (myOffset.Value));
			}
			if (myLimit != null) {
				if (lines != null)
					lines = lines.Take<String> (Convert.ToInt32 (myLimit.Value));
			}

            #endregion

            #region Import queries

			ExecuteAsSingleThread (lines, myGraphQL, mySecurityToken, myTransactionToken, myVerbosityType, myComments);

            #endregion
        }

        private void ExecuteAsSingleThread (IEnumerable<String> myLines,
                                                    IGraphQL myIGraphQL,
                                                    SecurityToken mySecurityToken,
                                                    Int64 myTransactionToken,
                                                    VerbosityTypes myVerbosityType,
                                                    IEnumerable<String> comments = null)
		{
			
			_logger.Log (Level.INFO, "Started GQL import.");
			
            #region data

			Int64 numberOfLine = 0;

            #endregion

            #region check lines and execute query
			
			Stopwatch sw = new Stopwatch ();
			
			foreach (var _Line in myLines) {
				numberOfLine++;

				if (String.IsNullOrWhiteSpace (_Line)) {
					continue;
				}

                #region Skip comments

				if (IsComment (_Line, comments))
					continue;

                #endregion

                #region execute query
				_logger.Log (Level.INFO, String.Format ("Executing query {0}: {1}", _numberOfStatements, _Line));
				
				sw.Reset ();
				sw.Start ();
				
				var tempResult = myIGraphQL.Query (mySecurityToken, myTransactionToken, _Line);
				Interlocked.Increment (ref _numberOfStatements);
				
				sw.Stop ();
				
				_logger.Log (Level.INFO, String.Format ("Took {0}ms.", sw.Elapsed.TotalMilliseconds));
				
                #endregion

                #region Add errors and break execution

				if (tempResult.TypeOfResult == ResultType.Failed) {
					if (tempResult.Error.Message.Equals ("Mal-formed  string literal - cannot find termination symbol.")) {
						
						string error = "Query at line [" + numberOfLine + "] [" + _Line + "] failed with " + tempResult.Error.ToString () + " add next line...";
						Debug.WriteLine (error);
						_logger.Log (Level.WARNING, error);
					}
					if (myVerbosityType == VerbosityTypes.Errors) {
						if (tempResult.Error != null) {
							_logger.Log (Level.SEVERE, tempResult.Error.ToString ());
						} else {
							_logger.Log (Level.SEVERE, "Import failed because of unknown exception");
						}
					}
				}

                #endregion
			}


            #endregion
			
			_logger.Log (Level.INFO, String.Format ("Finished GQL import and executed {0} statements", _numberOfStatements));
		}

        private Boolean IsComment(String myQuery, IEnumerable<String> comments = null)
        {
            if (comments == null || comments.Count() == 0)
                return false;

            return comments.Any(c => myQuery.StartsWith(c));
        }

        private IQueryResult ExecuteQuery(String myQuery, IGraphQL myGraphQL, SecurityToken mySecurityToken, Int64 myTransactionToken)
        {
            return myGraphQL.Query(mySecurityToken, myTransactionToken, myQuery);
        }

        #endregion

        #region Get streams

        /// <summary>
        /// Reads a file, just let all exceptions thrown, they are too much to pack them into a graphDBException.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private Stream GetStreamFromFile(String location)
        {
            return File.OpenRead(location);
        }

        /// <summary>
        /// Reads a http ressource, just let all exceptions thrown, they are too much to pack them into a graphDBException.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private Stream GetStreamFromHttp(String location)
        {
            var request = (HttpWebRequest)WebRequest.Create(location);
            var response = request.GetResponse();
            return response.GetResponseStream();
        }

        #endregion

        #region protected methods

        /// <summary>
        /// Will read all lines of the <paramref name="myInputStream"/>. 
        /// Keep in mind, that this can be done ony 1 time. You need to seek the stream to 0 to reread all lines again.
        /// </summary>
        /// <param name="myInputStream"></param>
        /// <returns></returns>
        private IEnumerable<String> ReadLinesFromStream(Stream myInputStream)
        {
            var streamReader = new StreamReader(myInputStream);
            while (!streamReader.EndOfStream)
            {
                yield return streamReader.ReadLine();
            }
        }

        #endregion

        #region IPluginable Members

        public string PluginName
        {
            get { return "sones.gqlimport"; }
        }

        public string PluginShortName
        {
            get { return "GQL"; }
        }

        public string PluginDescription
        {
            get { return "This class realizes GQL code import from a file."; }
        }

        public PluginParameters<Type> SetableParameters
        {
            get { return new PluginParameters<Type>(); }
        }

        public IPluginable InitializePlugin(String myUniqueString, Dictionary<string, object> myParameters = null)
        {
            return new GraphDBImport_GQL();
        }

        public void Dispose()
        { }

        #endregion
    }
}
