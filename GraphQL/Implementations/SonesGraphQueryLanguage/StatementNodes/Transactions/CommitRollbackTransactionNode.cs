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
using Irony.Ast;
using Irony.Parsing;
using sones.GraphDB;
using sones.GraphQL.GQL.Manager.Plugin;
using sones.GraphQL.Result;
using sones.Library.Commons.Security;
using sones.Library.Commons.Transaction;
using sones.Library.ErrorHandling;
using System.Collections.Generic;
using System.Diagnostics;

namespace sones.GraphQL.StatementNodes.Transactions
{
    public enum CommandType
    {
        Commit,
        Rollback
    }

    public sealed class CommitRollbackTransactionNode : AStatement, IAstNodeInit
    {
        #region Properties

        public String Name { get; private set; }
        public Boolean ASync { get; private set; }
        public CommandType Command_Type { get; private set; }

        #endregion

        #region Constructor

        public CommitRollbackTransactionNode()
        { }

        #endregion

        #region IAstNodeInit Members

        public void Init(ParsingContext context, ParseTreeNode parseNode)
        {
            try
            {
                if (parseNode.ChildNodes != null && parseNode.ChildNodes.Count != 0)
                {
                    Command_Type = (CommandType)Enum.Parse(typeof(CommandType), parseNode.ChildNodes[0].ChildNodes[0].Token.Text, true);

                    //in the case we have some optional parameters
                    if (parseNode.ChildNodes[2].ChildNodes != null && parseNode.ChildNodes[2].ChildNodes.Count != 0)
                    {
                        GetAttributes(parseNode.ChildNodes[2], 0);
                    }
                }
            }
            catch (ASonesException e)
            {
                throw e;
            }
        }

        #endregion

        #region AStatement Members

        public override string StatementName
        {
            get { return "CommitRollbackTransaction"; }
        }

        public override TypesOfStatements TypeOfStatement
        {
            get { return TypesOfStatements.Readonly; }
        }

        public override IQueryResult Execute(IGraphDB myGraphDB, IGraphQL myGraphQL, GQLPluginManager myPluginManager, String myQuery, SecurityToken mySecurityToken, Int64 myTransactionToken)
        {
            var sw = Stopwatch.StartNew();

            var _ReturnValues = new Dictionary<String, Object>();

            if (Command_Type == CommandType.Commit)
            {
                myGraphDB.CommitTransaction(mySecurityToken, myTransactionToken);
            }

            else
            {
                myGraphDB.RollbackTransaction(mySecurityToken, myTransactionToken);
            }

            _ReturnValues.Add("TransactionID", myTransactionToken);
            _ReturnValues.Add("ExecutedCommand", Command_Type);
            _ReturnValues.Add("Name", Name == null ? "" : Name);
            _ReturnValues.Add("ASync", ASync);

            return QueryResult.Success(myQuery, SonesGQLConstants.GQL, new List<IVertexView> { new VertexView(_ReturnValues, null) }, Convert.ToUInt64(sw.ElapsedMilliseconds));
        }

        #endregion

        #region private helper methods

        /// <summary>
        /// get values for name and async
        /// </summary>
        /// <param name="myNode">the child node that contain the values</param>
        /// <param name="myCurrentChildNode">the current child node</param>
        private void GetAttributes(ParseTreeNode myNode, Int32 myCurrentChildNode)
        {
            if (myCurrentChildNode < myNode.ChildNodes.Count)
            {
                if (myNode.ChildNodes[myCurrentChildNode].ChildNodes != null && myNode.ChildNodes[myCurrentChildNode].ChildNodes.Count != 0)
                {
                    if (myNode.ChildNodes[myCurrentChildNode].ChildNodes[0].Token.Text.ToUpper() == SonesGQLConstants.TRANSACTION_NAME)
                        Name = myNode.ChildNodes[myCurrentChildNode].ChildNodes[2].Token.ValueString;
                }
                else
                {
                    if (myNode.ChildNodes[myCurrentChildNode].Token.Text.ToUpper() == SonesGQLConstants.TRANSACTION_COMROLLASYNC)
                        ASync = true;
                }

                GetAttributes(myNode, myCurrentChildNode + 1);
            }
        }

        #endregion



    }
}
