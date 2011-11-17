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
using sones.GraphQL.Result;
using sones.GraphDB;
using sones.Library.Commons.Security;
using sones.Library.Commons.Transaction;
using sones.GraphQL.GQL.Manager.Plugin;
using System.Collections.Generic;
using sones.GraphQL.Structure.Nodes.Misc;
using sones.Library.ErrorHandling;
using sones.GraphDB.TypeSystem;
using sones.GraphDB.Request;

namespace sones.GraphQL.StatementNodes.DML
{
    public sealed class RebuildIndicesNode : AStatement, IAstNodeInit
    {
        #region Data

        private HashSet<String> _Types;

        #endregion

        #region constructors

        public RebuildIndicesNode()
        { }

        #endregion
        
        #region IAstNodeInit Members

        public void Init(ParsingContext context, ParseTreeNode parseNode)
        {
            _Types = new HashSet<string>();

            if (HasChildNodes(parseNode.ChildNodes[2]))
            {
                parseNode.ChildNodes[2].ChildNodes[0].ChildNodes.ForEach(item => _Types.Add(((ATypeNode)item.AstNode).ReferenceAndType.TypeName));
            }
        }

        #endregion

        #region AStatement Members

        public override string StatementName
        {
            get { return "REBUILD INDICES"; }
        }

        public override TypesOfStatements TypeOfStatement
        {
            get { return TypesOfStatements.ReadWrite; }
        }

        public override IQueryResult Execute(IGraphDB myGraphDB, IGraphQL myGraphQL, GQLPluginManager myPluginManager, String myQuery, SecurityToken mySecurityToken, Int64 myTransactionToken)
        {
            try
            {
                var request = new RequestRebuildIndices(_Types);

                return myGraphDB.RebuildIndices<IQueryResult>(mySecurityToken, myTransactionToken, request, _=> GenerateOutput(myQuery, _));
            }
            catch (ASonesException ex)
            {
                return QueryResult.Failure(myQuery, SonesGQLConstants.GQL, ex);
            }
        }

        #endregion

        private IQueryResult GenerateOutput(String myQuery, IRequestStatistics myStats)
        {
            return QueryResult.Success(myQuery,
                                    SonesGQLConstants.GQL,
                                    new IVertexView[0],
                                    Convert.ToUInt64(myStats.ExecutionTime.TotalMilliseconds));
        }
    }
}
