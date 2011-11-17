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
using sones.GraphQL.GQL.Structure.Helper.Definition;
using sones.GraphQL.Structure.Nodes.DDL;
using sones.GraphDB.TypeSystem;
using sones.GraphDB.Request;
using sones.Library.ErrorHandling;

namespace sones.GraphQL.StatementNodes.DDL
{
    /// <summary>
    /// This node is requested in case of an Create Types statement.
    /// </summary>
    public sealed class CreateVertexTypesNode : AStatement, IAstNodeInit
    {
        #region Data

        private List<GraphDBTypeDefinition> _TypeDefinitions = new List<GraphDBTypeDefinition>();
        private String _query;

        #endregion

        #region constructor

        public CreateVertexTypesNode()
        {

        }

        #endregion

        #region IAstNodeInit Members

        public void Init(ParsingContext context, ParseTreeNode myParseTreeNode)
        {
            //createTypesStmt.Rule =      S_CREATE + S_VERTEX + S_TYPES + bulkVertexTypeList
            //                        |   S_CREATE + S_ABSTRACT + S_VERTEX + S_TYPE + bulkVertexType
            //                        |   S_CREATE + S_VERTEX + S_TYPE + bulkVertexType;

            if (myParseTreeNode.ChildNodes[1].Token.KeyTerm == ((SonesGQLGrammar)context.Language.Grammar).S_ABSTRACT)
            {
                #region Abstract & Single VertexType

                BulkVertexTypeNode aTempNode = (BulkVertexTypeNode)myParseTreeNode.ChildNodes[4].AstNode;

                Boolean isAbstract = true;

                if (HasChildNodes(myParseTreeNode.ChildNodes[1]))
                {
                    isAbstract = true;
                }
                
                _TypeDefinitions.Add(new GraphDBTypeDefinition(aTempNode.TypeName, aTempNode.Extends, isAbstract, aTempNode.Attributes, aTempNode.BackwardEdges, aTempNode.Indices, aTempNode.Comment));
                
                #endregion
            }
            else
            {
                if (myParseTreeNode.ChildNodes[2].Token.KeyTerm == ((SonesGQLGrammar)context.Language.Grammar).S_TYPES)
                {
                    #region multiple VertexTypes

                    foreach (var _ParseTreeNode in myParseTreeNode.ChildNodes[3].ChildNodes)
                    {
                        if (_ParseTreeNode.AstNode != null)
                        {
                            BulkVertexTypeListMemberNode aTempNode = (BulkVertexTypeListMemberNode)_ParseTreeNode.AstNode;
                            _TypeDefinitions.Add(new GraphDBTypeDefinition(aTempNode.TypeName, aTempNode.Extends, aTempNode.IsAbstract, aTempNode.Attributes, aTempNode.BackwardEdges, aTempNode.Indices, aTempNode.Comment));
                        }
                    }

                    #endregion
                }
                else
                {
                    #region single vertex type

                    BulkVertexTypeNode aTempNode = (BulkVertexTypeNode)myParseTreeNode.ChildNodes[3].AstNode;

                    _TypeDefinitions.Add(new GraphDBTypeDefinition(aTempNode.TypeName, aTempNode.Extends, false, aTempNode.Attributes, aTempNode.BackwardEdges, aTempNode.Indices, aTempNode.Comment));

                    #endregion
                }
            }

        }

        #endregion

        #region AStatement Members

        public override string StatementName
        {
            get { return "CreateTypes"; }
        }

        public override TypesOfStatements TypeOfStatement
        {
            get { return TypesOfStatements.ReadWrite; }
        }

        public override IQueryResult Execute(IGraphDB myGraphDB, IGraphQL myGraphQL, GQLPluginManager myPluginManager, String myQuery, SecurityToken mySecurityToken, Int64 myTransactionToken)
        {
            _query = myQuery;

            IQueryResult result;

            try
            {
                result = myGraphDB.CreateVertexTypes<IQueryResult>(
                        mySecurityToken,
                        myTransactionToken,
                        new RequestCreateVertexTypes(GenerateVertexTypePredefinitions()),
                        CreateQueryResult);
            }
            catch (ASonesException e)
            {
                result = new QueryResult(_query, SonesGQLConstants.GQL, 0, ResultType.Failed, null, e);
            }

            return result;
        }

        #endregion

        #region private helper

        /// <summary>
        /// Generates a query result using some statistics and the created vertices
        /// </summary>
        /// <param name="myStats">The statistics of the request</param>
        /// <param name="myCreatedVertexTypes">The created vertex types</param>
        /// <returns>A IQueryResult</returns>
        private IQueryResult CreateQueryResult(IRequestStatistics myStats, IEnumerable<IVertexType> myCreatedVertexTypes)
        {
            return QueryResult.Success(_query, SonesGQLConstants.GQL, CreateVertexViews(myCreatedVertexTypes), Convert.ToUInt64(myStats.ExecutionTime.Milliseconds));
        }

        /// <summary>
        /// Creates vertex views corresponding to the created vertex types
        /// </summary>
        /// <param name="myCreatedVertexTypes">The vertex types that have been created</param>
        /// <returns>An enumerable of vertex views</returns>
        private IEnumerable<IVertexView> CreateVertexViews(IEnumerable<IVertexType> myCreatedVertexTypes)
        {
            List<IVertexView> result = new List<IVertexView>();

            foreach (var aCreatedVertes in myCreatedVertexTypes)
            {
                result.Add(GenerateAVertexView(aCreatedVertes));
            }

            return result;
        }

        /// <summary>
        /// Generates a single vertex view corresponding to a created vertex type
        /// </summary>
        /// <param name="aCreatedVertes">The vertex type that has been created</param>
        /// <returns>The resulting vertex view</returns>
        private IVertexView GenerateAVertexView(IVertexType aCreatedVertes)
        {
            return new VertexView(new Dictionary<string,object>
                                                         {
                                                             {SonesGQLConstants.VertexType, aCreatedVertes.Name},
                                                             {"VertexTypeID", aCreatedVertes.ID}
                                                         }, null);
        }

        /// <summary>
        /// Generates the vertex type predefinitions that are necessary to create the vertex types
        /// </summary>
        /// <returns>An enumerable of those pre definitions</returns>
        private IEnumerable<VertexTypePredefinition> GenerateVertexTypePredefinitions()
        {
            List<VertexTypePredefinition> result = new List<VertexTypePredefinition>();

            foreach (var aDefinition in _TypeDefinitions)
            {
                result.Add(GenerateAVertexTypePredefinition(aDefinition));
            }

            return result;
        }

        /// <summary>
        /// Generates a single vertex type predefinition
        /// </summary>
        /// <param name="aDefinition">The definition that has been created by the gql</param>
        /// <returns>The corresponding vertex type predefinition</returns>
        private VertexTypePredefinition GenerateAVertexTypePredefinition(GraphDBTypeDefinition aDefinition)
        {
            var result = new VertexTypePredefinition(aDefinition.Name);

            #region extends

            if (aDefinition.ParentType != null && aDefinition.ParentType.Length > 0)
            {
                result.SetSuperTypeName(aDefinition.ParentType);
            }

            #endregion

            #region abstract

            if (aDefinition.IsAbstract)
            {
                result.MarkAsAbstract();
            }

            #endregion

            #region comment

            if (aDefinition.Comment != null && aDefinition.Comment.Length > 0)
            {
                result.SetComment(aDefinition.Comment);
            }

            #endregion

            #region attributes

            if (aDefinition.Attributes != null)
            {
                foreach (var aAttribute in aDefinition.Attributes)
                {
                    result.AddUnknownAttribute(GenerateUnknownAttribute(aAttribute));
                }
            }

            #endregion

            #region incoming edges

            if (aDefinition.BackwardEdgeNodes != null)
            {
                foreach (var aIncomingEdge in aDefinition.BackwardEdgeNodes)
                {
                    result.AddIncomingEdge(GenerateAIncomingEdge(aIncomingEdge));
                }
            }

            #endregion

            #region indices

            if (aDefinition.Indices != null)
            {
                foreach (var aIndex in aDefinition.Indices)
                {
                    result.AddIndex(GenerateIndex(aIndex, aDefinition.Name));
                }
            }

            #endregion

            return result;
        }

        /// <summary>
        /// Generates a index predefinition
        /// </summary>
        /// <param name="aIndex">The index definition by the gql</param>
        /// <returns>An IndexPredefinition</returns>
        private IndexPredefinition GenerateIndex(IndexDefinition aIndex, String myVertexType)
        {
            IndexPredefinition result;

            if (String.IsNullOrEmpty(aIndex.IndexName))
            {
                result = new IndexPredefinition(myVertexType);
            }
            else
            {
                result = new IndexPredefinition(aIndex.IndexName, myVertexType);
            }
            result.SetVertexType(myVertexType);
            if (!String.IsNullOrEmpty(aIndex.IndexType))
            {
                result.SetIndexType(aIndex.IndexType);
            }

            foreach (var aIndexProperty in aIndex.IndexAttributeDefinitions)
            {
                result.AddProperty(aIndexProperty.IndexAttribute.ContentString);
            }

            if (aIndex.Options != null)
            {
                foreach (var aIndexOption in aIndex.Options)
                {
                    result.AddOption(aIndexOption.Key, aIndexOption.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Generates an incoming edge attribute
        /// </summary>
        /// <param name="aIncomingEdge">The incoming edge definition by the gql</param>
        /// <returns>An incoming edge predefinition</returns>
        private IncomingEdgePredefinition GenerateAIncomingEdge(IncomingEdgeDefinition aIncomingEdge)
        {
            IncomingEdgePredefinition result = new IncomingEdgePredefinition(aIncomingEdge.AttributeName,
                                                                                aIncomingEdge.TypeName, 
                                                                                aIncomingEdge.TypeAttributeName);

            return result;
        }

        /// <summary>
        /// Generates a attribute definition
        /// </summary>
        /// <param name="aAttribute">The attribute that is going to be transfered</param>
        /// <returns>A attribute predefinition</returns>
        private UnknownAttributePredefinition GenerateUnknownAttribute(KeyValuePair<AttributeDefinition, string> aAttribute)
        {
            UnknownAttributePredefinition result = new UnknownAttributePredefinition(aAttribute.Key.AttributeName, aAttribute.Value);
            
            if (aAttribute.Key.AttributeType.EdgeType != null)
            {
                result.SetInnerEdgeType(aAttribute.Key.AttributeType.EdgeType);
            }

            if (aAttribute.Key.DefaultValue != null)
            {
                result.SetDefaultValue(aAttribute.Key.DefaultValue.ToString());
            }

            switch (aAttribute.Key.AttributeType.Type)
            {
                case SonesGQLGrammar.TERMINAL_SET:

                    result.SetMultiplicityAsSet();

                    break;

                case SonesGQLGrammar.TERMINAL_LIST:

                    result.SetMultiplicityAsList();

                    break;
            }

            if (aAttribute.Key.DefaultValue != null)
                result.SetDefaultValue(aAttribute.Key.DefaultValue.ToString());


            if (aAttribute.Key.AttributeType.TypeCharacteristics.IsMandatory)
                result.SetAsMandatory();

            if (aAttribute.Key.AttributeType.TypeCharacteristics.IsUnique)
                result.SetAsUnique();



            return result;
        }

        #endregion

    }
}
