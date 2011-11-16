﻿/*
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
using System.Linq;
using System.Text;
using sones.GraphDB.TypeSystem;
using sones.Library.Commons.Transaction;
using sones.Library.Commons.Security;
using sones.GraphDB.TypeManagement.Base;
using sones.GraphDB.TypeManagement;
using sones.GraphDB.ErrorHandling;
using sones.Library.PropertyHyperGraph;
using sones.GraphDB.Request;
using sones.Library.Commons.VertexStore.Definitions;
using sones.GraphDB.Expression;
using sones.Library.ErrorHandling;
using sones.Library.LanguageExtensions;
using sones.Library.Commons.VertexStore.Definitions.Update;
using sones.GraphFS.ErrorHandling;

namespace sones.GraphDB.Manager.TypeManagement
{
    internal class ExecuteEdgeTypeManager: AExecuteTypeManager<IEdgeType>
    {
        #region data

        #endregion

        #region constructor

        public ExecuteEdgeTypeManager(IDManager myIDManager)
            :base(myIDManager)
        {
            //_idManager = myIDManager;

            _baseTypes = new Dictionary<long, IBaseType>();
            _nameIndex = new Dictionary<String, long>();
        }

        #endregion

        #region ACheckTypeManager member

        public override IEnumerable<IEdgeType> GetAllTypes(Int64 myTransaction,
                                                            SecurityToken mySecurity)
        {
            var egdes = _vertexManager.ExecuteManager.GetVertices(BaseTypes.EdgeType.ToString(),
                                                                        myTransaction,
                                                                        mySecurity,
                                                                        false);

            return egdes == null ? Enumerable.Empty<IEdgeType>()
                                    : egdes.Select(x => new EdgeType(x, _baseStorageManager));
        }

        public override void TruncateType(long myTypeID,
                                            Int64 myTransactionToken,
                                            SecurityToken mySecurityToken)
        {
            GetType(myTypeID, myTransactionToken, mySecurityToken);

            if (IsTypeBaseType(myTypeID))
                throw new InvalidTypeException("[BaseType] " + myTypeID.ToString(), "userdefined type");

            return;
        }

        public override void TruncateType(string myTypeName,
                                            Int64 myTransactionToken,
                                            SecurityToken mySecurityToken)
        {
            GetType(myTypeName, myTransactionToken, mySecurityToken);

            if (IsTypeBaseType(myTypeName))
                throw new InvalidTypeException("[BaseType] " + myTypeName, "userdefined type");

            return;
        }

        public override void CleanUpTypes()
        {
            var help = new Dictionary<long, IBaseType>(_baseTypes);

            _baseTypes.Clear();
            _nameIndex.Clear();

            foreach (var type in new[] { BaseTypes.Edge, 
                                            BaseTypes.Orderable, 
                                            BaseTypes.Weighted })
            {
                _baseTypes.Add((long)type, help[(long)type]);
                _nameIndex.Add(type.ToString(), (long)type);
            }
        }

        public override void Initialize(IMetaManager myMetaManager)
        {
            _indexManager       = myMetaManager.IndexManager;
            _vertexManager      = myMetaManager.VertexManager;
            _vertexTypeManager =  myMetaManager.VertexTypeManager;
            _baseTypeManager    = myMetaManager.BaseTypeManager;
            _baseStorageManager = myMetaManager.BaseGraphStorageManager;
        }

        public override void Load(Int64 myTransaction,
                                    SecurityToken mySecurity)
        {
            LoadBaseType(
                myTransaction,
                mySecurity,
                BaseTypes.Edge,
                BaseTypes.Weighted,
                BaseTypes.Orderable);
        }

        protected override IEdgeType CreateType(IVertex myVertex)
        {
            var result = new EdgeType(myVertex, _baseStorageManager);

            _baseTypes.Add(result.ID, result);
            _nameIndex.Add(result.Name, result.ID);

            return result;
        }

        #endregion

        #region private helper

        #region abstract helper

        /// <summary>
        /// Loads the specified base types.
        /// </summary>
        /// <param name="myTransaction">The Int64.</param>
        /// <param name="mySecurity">The SecurityToken.</param>
        /// <param name="myBaseTypes">The to be loaded types.</param>
        private void LoadBaseType(Int64 myTransaction, 
                                    SecurityToken mySecurity, 
                                    params BaseTypes[] myBaseTypes)
        {
            foreach (var baseType in myBaseTypes)
            {
                var vertex = _vertexManager.ExecuteManager.VertexStore.GetVertex(mySecurity, 
                                                                                    myTransaction, 
                                                                                    (long)baseType, 
                                                                                    (long)BaseTypes.EdgeType, 
                                                                                    String.Empty);
                
                if (vertex == null)
                    throw new BaseEdgeTypeNotExistException(baseType.ToString(), "Could not load base edge type.");

                _baseTypes.Add((long)baseType, new EdgeType(vertex, _baseStorageManager));
            }
        }

        /// <summary>
        /// Checks if the given type is a base type
        /// </summary>
        protected override bool IsTypeBaseType(long myTypeID)
        {
            return ((long)BaseTypes.Edge).Equals(myTypeID) ||
                        ((long)BaseTypes.Orderable).Equals(myTypeID) ||
                        ((long)BaseTypes.Weighted).Equals(myTypeID);
        }

        /// <summary>
        /// Checks if the given type is a base type
        /// </summary>
        protected override bool IsTypeBaseType(String myTypeName)
        {
            BaseTypes type;
            if (!Enum.TryParse(myTypeName, out type))
                return false;

            return true;
        }

        /// <summary>
        /// Calls the needed store methods depending on the typemanger.
        /// </summary>
        /// <param name="myDefsTopologically">The topologically sorted type predefinitions.</param>
        /// <param name="myTypeInfos">The created type infos.</param>
        /// <param name="myCreationDate">The creation date.</param>
        /// <param name="myResultPos">The result position.</param>
        /// <param name="myTransactionToken">The Int64.</param>
        /// <param name="mySecurityToken">The SecurityToken.</param>
        /// <param name="myResult">Ref on result array.</param>
        protected override IEnumerable<IEdgeType> StoreTypeAndAttributes(LinkedList<ATypePredefinition> myDefsTopologically,
                                                                            Dictionary<String, TypeInfo> myTypeInfos,
                                                                            long myCreationDate,
                                                                            int myResultPos,
                                                                            Int64 myTransactionToken,
                                                                            SecurityToken mySecurityToken,
                                                                            ref IVertex[] myResult)
        {
            #region store vertex type

            StoreEdgeType(myDefsTopologically,
                            myTypeInfos,
                            myCreationDate,
                            myResultPos,
                            myTransactionToken,
                            mySecurityToken,
                            ref myResult);

            #endregion

            #region Store Attributes

            //The order of adds is important. First property, then outgoing edges (that might point to properties) and finally incoming edges (that might point to outgoing edges)
            //Do not try to merge it into one for block.

            #region Store properties

            StoreProperties(myDefsTopologically,
                            myTypeInfos,
                            myCreationDate,
                            myTransactionToken,
                            mySecurityToken);

            #endregion

            var resultTypes = new EdgeType[myResult.Length];

            #region reload the stored types

            //reload the IVertex objects, that represents the type.
            for (int i = 0; i < myResult.Length; i++)
            {
                myResult[i] = _vertexManager.ExecuteManager.VertexStore.GetVertex(mySecurityToken, myTransactionToken,
                                                                                myResult[i].VertexID,
                                                                                myResult[i].VertexTypeID, String.Empty);

                var newEdgeType = new EdgeType(myResult[i], _baseStorageManager);

                resultTypes[i] = newEdgeType;

                _baseTypes.Add(myTypeInfos[newEdgeType.Name].VertexInfo.VertexID, newEdgeType);
            }

            #endregion

            #endregion

            return resultTypes.AsEnumerable<IEdgeType>();
        }

        /// <summary>
        /// Checks if the attribute names on vertex type definitions are unique, containing parent myAttributes.
        /// </summary>
        /// <param name="myTopologicallySortedPointer">A pointer to a vertex type predefinitions in a topologically sorted linked list.</param>
        /// <param name="myAttributes">A dictionary vertex type name to attribute names, that is build up during the process of CanAddCheckWithFS.</param>
        /// <param name="myTransaction">A transaction token for this operation.</param>
        /// <param name="mySecurity">A security token for this operation.</param>
        protected override void CanAddCheckAttributeNameUniquenessWithFS(LinkedListNode<ATypePredefinition> myTopologicallySortedPointer,
                                                                            IDictionary<string, HashSet<string>> myAttributes,
                                                                            Int64 myTransaction,
                                                                            SecurityToken mySecurity)
        {
            var parentPredef = GetParentPredefinitionOnTopologicallySortedList(myTopologicallySortedPointer);

            if (parentPredef == null)
            {
                //Get the parent type from FS.
                var parent = Get(myTopologicallySortedPointer.Value.SuperTypeName, myTransaction, mySecurity);

                if (parent == null)
                    //No parent type was found.
                    throw new TypeDoesNotExistException<IEdgeType>(myTopologicallySortedPointer.Value.SuperTypeName);

                if (parent.GetProperty<bool>((long)AttributeDefinitions.BaseTypeDotIsSealed))
                    //The parent type is sealed.
                    throw new SealedBaseTypeException(myTopologicallySortedPointer.Value.TypeName, 
                                                        parent.GetPropertyAsString((long)AttributeDefinitions.AttributeDotName));

                var parentType = new EdgeType(parent, _baseStorageManager);
                var attributeNames = parentType.GetAttributeDefinitions(true).Select(_ => _.Name);

                myAttributes[myTopologicallySortedPointer.Value.TypeName] = new HashSet<string>(attributeNames);
            }
            else
            {
                myAttributes[myTopologicallySortedPointer.Value.TypeName] = new HashSet<string>(myAttributes[parentPredef.Value.TypeName]);
            }

            var attributeNamesSet = myAttributes[myTopologicallySortedPointer.Value.TypeName];

            CheckPropertiesUniqueName(myTopologicallySortedPointer.Value, attributeNamesSet);
        }

        /// <summary>
        /// Does the necessary checks for can add with the use of the FS.
        /// </summary>
        /// <param name="myDefsTopologically">A topologically sorted list of vertex type predefinitions. 
        ///     <remarks><c>NULL</c> is not allowed, but not checked.</remarks></param>
        /// <param name="myDefsByName">The same vertex type predefinitions as in 
        ///     <paramref name="myDefsTpologically"/>, but indexed by their name. 
        ///     <remarks><c>NULL</c> is not allowed, but not checked.</remarks></param>
        /// <param name="myTransaction">A transaction token for this operation.</param>
        /// <param name="mySecurity">A security token for this operation.</param>
        ///     <remarks><paramref name="myDefsTopologically"/> and 
        ///     <paramref name="myDefsByName"/> must contain the same vertex type predefinitions. This is never checked.</remarks>
        /// The predefinitions are checked one by one in topologically order. 
        protected override void CanAddCheckWithFS(LinkedList<ATypePredefinition> myDefsTopologically,
                                                    IDictionary<string, ATypePredefinition> myDefsByName,
                                                    Int64 myTransaction, SecurityToken mySecurity)
        {

            //Contains the vertex type name to the attribute names of the vertex type.
            var attributes = new Dictionary<String, HashSet<String>>(myDefsTopologically.Count);

            for (var current = myDefsTopologically.First; current != null; current = current.Next)
            {
                CanAddCheckVertexNameUniqueWithFS(current.Value, myTransaction, mySecurity);
                CanAddCheckAttributeNameUniquenessWithFS(current, attributes, myTransaction, mySecurity);
            }
        }

        protected override Dictionary<String, TypeInfo> GenerateTypeInfos(
                                                            LinkedList<ATypePredefinition> myDefsSortedTopologically,
                                                            IDictionary<string, ATypePredefinition> myDefsByName,
                                                            long myFirstID,
                                                            Int64 myTransaction,
                                                            SecurityToken mySecurity)
        {
            var neededTypes = new HashSet<string>();

            foreach (var def in myDefsByName)
            {
                neededTypes.Add(def.Value.TypeName);
                neededTypes.Add(def.Value.SuperTypeName);
            }

            //At most all vertex types are needed.
            var result = new Dictionary<String, TypeInfo>((int)myFirstID + myDefsByName.Count);

            foreach (var type in neededTypes)
            {
                if (myDefsByName.ContainsKey(type))
                {
                    result.Add(type, new TypeInfo
                    {
                        AttributeCountWithParents = myDefsByName[type].AttributeCount,
                        VertexInfo = new VertexInformation((long)BaseTypes.EdgeType, myFirstID++)
                    });
                }
                else
                {
                    var vertex = _vertexManager.ExecuteManager.GetSingleVertex(
                                    new BinaryExpression(new SingleLiteralExpression(type),
                                                            BinaryOperator.Equals,
                                                            _EdgeTypeNameExpression),
                                    myTransaction, mySecurity);

                    IEdgeType neededType = new EdgeType(vertex, _baseStorageManager);

                    result.Add(type, new TypeInfo
                    {
                        AttributeCountWithParents = neededType.GetAttributeDefinitions(true).LongCount(),
                        VertexInfo = new VertexInformation((long)BaseTypes.EdgeType, _baseStorageManager.GetUUID(vertex))
                    });
                }
            }

            //accumulate attribute counts
            for (var current = myDefsSortedTopologically.First; current != null; current = current.Next)
            {
                if (!result.ContainsKey(current.Value.TypeName))
                    continue;

                var info = result[current.Value.TypeName];

                info.AttributeCountWithParents = info.AttributeCountWithParents + result[current.Value.SuperTypeName].AttributeCountWithParents;

                result[current.Value.TypeName] = info;
            }

            return result;
        }

        /// <summary>
        /// Gets an IVertex representing the vertex type given by <paramref name="myTypeID"/>.
        /// </summary>
        /// <param name="myTypeId"></param>
        /// <param name="myTransaction">A transaction token for this operation.</param>
        /// <param name="mySecurity">A security token for this operation.</param>
        /// <returns>An IVertex instance, that represents the vertex type with the given ID or <c>NULL</c>, if not present.</returns>
        protected override IVertex Get(long myTypeId,
                                        Int64 myTransaction,
                                        SecurityToken mySecurity)
        {
            #region get the type from fs

            return _vertexManager.ExecuteManager
                    .GetSingleVertex(new BinaryExpression(_EdgeDotEdgeTypeIDExpression,
                                                            BinaryOperator.Equals,
                                                            new SingleLiteralExpression(myTypeId)),
                                        myTransaction, mySecurity);

            #endregion
        }

        /// <summary>
        /// Gets an IVertex representing the vertex type given by <paramref name="myTypeName"/>.
        /// </summary>
        /// <param name="myTypeName">The vertex type name.</param>
        /// <param name="myTransaction">A transaction token for this operation.</param>
        /// <param name="mySecurity">A security token for this operation.</param>
        /// <returns>An IVertex instance, that represents the vertex type with the given name or <c>NULL</c>, if not present.</returns>
        protected override IVertex Get(string myTypeName,
                                        Int64 myTransaction,
                                        SecurityToken mySecurity)
        {
            #region get the type from fs

            return _vertexManager.ExecuteManager
                    .GetSingleVertex(new BinaryExpression(_EdgeTypeNameExpression,
                                                            BinaryOperator.Equals,
                                                            new SingleLiteralExpression(myTypeName)),
                                        myTransaction, mySecurity);

            #endregion
        }

        /// <summary>
        /// Reservs myCountOfNeededIDs type ids in id manager depending on type and gets the first reserved id.
        /// </summary>
        /// <param name="myCountOfNeededIDs">Count of to be reserved ids.</param>
        /// <returns>The first reserved id.</returns>
        protected override long GetFirstTypeID(int myCountOfNeededIDs)
        {
            return _idManager.EdgeTypeID.ReserveIDs(myCountOfNeededIDs);
        }

        /// <summary>
        /// Removes the given types from the graphDB.
        /// </summary>
        /// <param name="myVertexTypes">The types to delete.</param>
        /// <param name="myTransaction">Transaction token.</param>
        /// <param name="mySecurity">Security Token.</param>
        /// <param name="myIgnoreReprimands">True means, that reprimands (IncomingEdges) on the types wich should be removed are ignored.</param>
        /// <returns>Set of deleted type IDs.</returns>
        protected override Dictionary<Int64, String> Remove(IEnumerable<IEdgeType> myTypes,
                                                            Int64 myTransaction,
                                                            SecurityToken mySecurity,
                                                            bool myIgnoreReprimands = false)
        {
            //the attribute types on delete types which have to be removed
            var toDeleteAttributeDefinitions = new List<IAttributeDefinition>();

            #region get propertydefinitions

            //get child vertex types
            foreach (var delType in myTypes)
            {
                try
                {
                    //check if type exists
                    var temp = GetType(delType.ID, myTransaction, mySecurity);
                }
                catch (Exception exception)
                {
                    throw new TypeRemoveException<IEdgeType>(delType.Name, exception); 
                }

                if (!delType.HasParentType)
                    //type must be base type because there is no parent type, Exception that base type cannot be deleted
                    throw new TypeRemoveException<IEdgeType>(delType.Name, "A BaseType connot be removed.");

                if (IsTypeBaseType(delType.ID))
                    //Exception that base type cannot be deleted
                    throw new TypeRemoveException<IEdgeType>(delType.Name, "A BaseType connot be removed.");

                if (!myIgnoreReprimands)
                {
                    #region check that existing child types are specified

                    if (!delType.GetDescendantTypes().All(child => myTypes.Contains(child)))
                        throw new TypeRemoveException<IEdgeType>(delType.Name, "The given type has child types and cannot be removed.");

                    #endregion

                    #region check if there is a vertex type with an outgoing edge of type - delType

                    var vertex = Get(delType.ID, myTransaction, mySecurity);
                    List<long> incomingVertices = new List<long>();
                    foreach (var collection in vertex.GetAllIncomingVertices().Select(_ => _.IncomingVertices.Select(x => x.VertexID)))
                        incomingVertices.AddRange(collection);

                    var attributes = delType.GetAttributeDefinitions(true).Select(_ => _.ID);

                    if(!incomingVertices.All(_ => attributes.Contains(_)))
                        throw new TypeRemoveException<IEdgeType>(delType.Name, 
                                    @"There are reprimands on the type which shoul be removed.
                                        There exist an outgoing edge of type " + delType.Name);

                    #endregion
                }

                toDeleteAttributeDefinitions.AddRange(delType.GetAttributeDefinitions(false));
            }

            #endregion

            //the IDs of the deleted vertices
            var deletedTypeIDs = new Dictionary<Int64, String>(myTypes.ToDictionary(key => key.ID, item => item.Name));

            #region remove attribute types on delete types

            //delete attribute vertices
            foreach (var attr in toDeleteAttributeDefinitions)
            {
                switch (attr.Kind)
                {
                    case (AttributeType.Property):
                        if (!_vertexManager.ExecuteManager.VertexStore.RemoveVertex(mySecurity, 
                                                                                    myTransaction, 
                                                                                    attr.ID, 
                                                                                    (long)BaseTypes.Property))
                            throw new TypeRemoveException<IEdgeType>(attr.RelatedType.Name, 
                                                                        "The Property " + attr.Name + " could not be removed.");
                        break;

                    default:
                        throw new TypeRemoveException<IEdgeType>(attr.RelatedType.Name, "invalid attribute type.");
                }
            }

            #endregion

            #region remove vertex type names from index

            foreach (var type in myTypes)
            {
                var result = _indexManager.GetIndex(BaseUniqueIndex.EdgeTypeDotName);

                if (result != null)
                    result.Remove(type.Name);
                    //if (!result.Remove(type.Name))
                    //    throw new TypeRemoveException<IEdgeType>(type.Name, "Error during delete the Index on type.");
            }

            #endregion

            #region remove vertices

            //delete the vertices
            foreach (var type in myTypes)
            {
                //removes the instances of the VertexType
                //_vertexManager.ExecuteManager.VertexStore.RemoveVertices(mySecurity, myTransaction, (long)BaseTypes.Edge, new List<long>{ type.ID });

                //removes the vertexType
                if (!_vertexManager.ExecuteManager.VertexStore.RemoveVertex(mySecurity, myTransaction, type.ID, (long)BaseTypes.EdgeType))
                    if (_vertexManager.ExecuteManager.VertexStore.VertexExists(mySecurity, myTransaction, type.ID, (long)BaseTypes.EdgeType))
                        throw new TypeRemoveException<IEdgeType>(type.Name, "Could not remove the vertex representing the type.");
            }

            #endregion

            toDeleteAttributeDefinitions.Clear();

            CleanUpTypes();

            return deletedTypeIDs;
        }

        /// <summary>
        /// Checks if the given parameter type is valid.
        /// </summary>
        /// <param name="myRequest">The parameter to be checked.</param>
        protected override void CheckRequestType(IRequestAlterType myRequest)
        {
            if (!(myRequest is RequestAlterEdgeType))
                throw new InvalidParameterTypeException("AlterTypeRequest", myRequest.GetType().Name, typeof(RequestAlterEdgeType).Name);
        }

        /// <summary>
        /// All to be removed things of the alter type request are going to be removed inside this method,
        /// the related operations will be executed inside here.
        /// </summary>
        /// <param name="myAlterTypeRequest">The alter type request.</param>
        /// <param name="myType">The to be altered type.</param>
        /// <param name="myTransactionToken">The Int64.</param>
        /// <param name="mySecurityToken">The SecurityToken.</param>
        /// <param name="myUpdateRequest">A reference to an update request to update relevant vertices.</param>
        protected override void AlterType_Remove(IRequestAlterType myAlterTypeRequest,
                                                    IEdgeType myType,
                                                    Int64 myTransactionToken,
                                                    SecurityToken mySecurityToken,
                                                    ref RequestUpdate myUpdateRequest)
        {
            List<long> removedProps = (List<long>)RemoveAttributes(myAlterTypeRequest.ToBeRemovedProperties,
                                                myType,
                                                myTransactionToken,
                                                mySecurityToken);

            List<long> undefProps = (List<long>)UndefineAttributes(myAlterTypeRequest.ToBeUndefinedAttributes,
                                                myType,
                                                myTransactionToken,
                                                mySecurityToken);

            myUpdateRequest
                .UpdateEdge(new SingleEdgeUpdateDefinition(
                                    new VertexInformation(), 
                                    new VertexInformation(), 
                                    myType.ID, 
                                    null, 
                                    new StructuredPropertiesUpdate(null,
                                                                    removedProps)));
        }

        /// <summary>
        /// All to be added things of the alter type request are going to be added inside this method,
        /// the related operations will be executed inside here.
        /// </summary>
        /// <param name="myAlterTypeRequest">The alter type request.</param>
        /// <param name="myType">The to be altered type.</param>
        /// <param name="myTransactionToken">The Int64.</param>
        /// <param name="myUpdateRequest">A reference to an update request to update relevant vertices.</param>
        protected override void AlterType_Add(IRequestAlterType myAlterTypeRequest,
                                                IEdgeType myType,
                                                Int64 myTransactionToken,
                                                SecurityToken mySecurityToken,
                                                ref RequestUpdate myUpdateRequest)
        {
            var addedProps = AddAttributes(myAlterTypeRequest.ToBeAddedProperties,
                                              myType, 
                                              myTransactionToken, 
                                              mySecurityToken);

            var definedProps = DefineAttributes(myAlterTypeRequest.ToBeDefinedAttributes,
                                              myType,
                                              myTransactionToken,
                                              mySecurityToken);

            myUpdateRequest
                .UpdateEdge(new SingleEdgeUpdateDefinition(
                                    new VertexInformation(),
                                    new VertexInformation(),
                                    myType.ID,
                                    null,
                                    new StructuredPropertiesUpdate(addedProps)));
        }

        /// <summary>
        /// Adds the specified properties to the given type and stores them.
        /// </summary>
        /// <param name="myToBeAddedProperties">The to be added properties.</param>
        /// <param name="myTransactionToken">The Int64.</param>
        /// <param name="mySecurityToken">The SecurityToken.</param>
        /// <param name="myType">The to be altered type.</param>
        /// <returns>A dictionary with to be added attributes and default value</returns>returns>
        protected override Dictionary<long, IComparable> ProcessAddPropery(
            IEnumerable<PropertyPredefinition> myToBeAddedProperties,
            Int64 myTransactionToken,
            SecurityToken mySecurityToken,
            IEdgeType myType)
        {
            Dictionary<long, IComparable> dict = null;

            foreach (var aProperty in myToBeAddedProperties)
            {
                var id = _idManager
                            .GetVertexTypeUniqeID((long)BaseTypes.Attribute)
                            .GetNextID();

                if (aProperty.DefaultValue != null)
                {
                    dict = dict ?? new Dictionary<long, IComparable>();

                    dict.Add(id, aProperty.DefaultValue);
                }
                
                _baseStorageManager.StoreProperty(
                    _vertexManager.ExecuteManager.VertexStore,
                    new VertexInformation(
                        (long)BaseTypes.Property,
                        id),
                    aProperty.AttributeName,
                    aProperty.Comment,
                    DateTime.UtcNow.ToBinary(),
                    aProperty.IsMandatory,
                    aProperty.Multiplicity,
                    aProperty.DefaultValue,
                    true,
                    new VertexInformation(
                        (long)BaseTypes.EdgeType,
                        myType.ID),
                    ConvertBasicType(aProperty.AttributeType),
                    mySecurityToken,
                    myTransactionToken);
            }

            return dict;
        }

        private void DefinePropertyOnSingleEdge(
            Int64 myTransactionToken,
            SecurityToken mySecurityToken,
            ISingleEdge edge, 
            UnknownAttributePredefinition aAttribute, 
            long PropertyId, 
            long EdgeId,
            IVertex vertex,
            IEdgeType myType,
            bool bHyperEdge = false
        )
        {
            bool bFound = false;

            #region cast existing unstructured properties on vertices to structured

            foreach (var property in edge.GetAllUnstructuredProperties())
            {
                if (property.PropertyName.CompareTo(aAttribute.AttributeName) == 0)
                {
                    bFound = true;

                    // Found unstructured property with same name on the vertex
                    var targettype = _baseStorageManager.GetBaseType(aAttribute.AttributeType);
                    IComparable value = null;

                    // Try Convert Type
                    try
                    {
                        value = property.Property.ConvertToIComparable(targettype);
                    }
                    catch (InvalidCastException)
                    {
                        throw new VertexAttributeCastException(aAttribute.AttributeName, property.Property.GetType(), targettype, false);
                    }
                    catch (FormatException)
                    {
                        throw new VertexAttributeCastException(aAttribute.AttributeName, property.Property.GetType(), targettype, true);
                    }

                    // add list with only one item -> unstructured property to delete
                    var unstructdel = new List<string>();
                    unstructdel.Add(property.PropertyName);
                    UnstructuredPropertiesUpdate propdelete = new UnstructuredPropertiesUpdate(null, unstructdel);

                    // add list with only one item -> structured property to add
                    Dictionary<long, IComparable> properties2add = new Dictionary<long, IComparable>();
                    properties2add.Add(PropertyId, value);
                    StructuredPropertiesUpdate propadd = new StructuredPropertiesUpdate(properties2add, null);

                    var sourcevertex = edge.GetSourceVertex();
                    var targetvertex = edge.GetTargetVertex();

                    VertexUpdateDefinition upddef;

                    if (bHyperEdge)
                    {
                        // add list with only one item -> edges to update
                        List<SingleEdgeUpdateDefinition> edges2update = new List<SingleEdgeUpdateDefinition>();
                        edges2update.Add(
                            new SingleEdgeUpdateDefinition(
                                new VertexInformation(sourcevertex.VertexTypeID, sourcevertex.VertexID),
                                new VertexInformation(targetvertex.VertexTypeID, targetvertex.VertexID),
                                edge.EdgeTypeID, null, propadd, propdelete
                            )
                        );

                        // add list with only one item -> edges to update
                        Dictionary<long, HyperEdgeUpdateDefinition> hedges2update = new Dictionary<long, HyperEdgeUpdateDefinition>();
                        hedges2update.Add(
                            EdgeId,
                            new HyperEdgeUpdateDefinition(
                               edge.EdgeTypeID, null, null, null, null, edges2update
                            )
                        );

                        HyperEdgeUpdate edgeupd = new HyperEdgeUpdate(hedges2update);
                        upddef = new VertexUpdateDefinition(null, null, null, null, null, edgeupd, null);
                    }
                    else
                    {
                        // add list with only one item -> edges to update
                        Dictionary<long, SingleEdgeUpdateDefinition> edges2update = new Dictionary<long, SingleEdgeUpdateDefinition>();
                        edges2update.Add(
                            EdgeId,
                            new SingleEdgeUpdateDefinition(
                                new VertexInformation(sourcevertex.VertexTypeID, sourcevertex.VertexID),
                                new VertexInformation(targetvertex.VertexTypeID, targetvertex.VertexID),
                                edge.EdgeTypeID, null, propadd, propdelete
                            )
                        );
                        
                        SingleEdgeUpdate edgeupd = new SingleEdgeUpdate(edges2update);
                        upddef = new VertexUpdateDefinition(null, null, null, null, edgeupd, null, null);
                    }

                    // Update the vertex (delete unstructured and add structured)
                    
                    _vertexManager.ExecuteManager.VertexStore.UpdateVertex(mySecurityToken, myTransactionToken, vertex.VertexID, vertex.VertexTypeID, upddef);

                    // we can break as we already found the property on the vertex
                    break;
                }
            }

            #endregion

            #region add mandatory attribute if not existing on vertex

            if ((!bFound) && (aAttribute.IsMandatory))
            {
                if (aAttribute.DefaultValue == null) throw new DefineMandatoryWithoutDefaultException(aAttribute.AttributeName, myType.Name);

                // Found unstructured property with same name on the vertex
                var targettype = _baseStorageManager.GetBaseType(aAttribute.AttributeType);
                IComparable value = null;

                // Try Convert Type
                try
                {
                    value = aAttribute.DefaultValue.ConvertToIComparable(targettype);
                }
                catch (InvalidCastException)
                {
                    throw new VertexAttributeCastException(aAttribute.AttributeName, aAttribute.DefaultValue.GetType(), targettype, false);
                }
                catch (FormatException)
                {
                    throw new VertexAttributeCastException(aAttribute.AttributeName, aAttribute.DefaultValue.GetType(), targettype, true);
                }

                // add list with only one item -> structured property to add
                Dictionary<long, IComparable> properties2add = new Dictionary<long, IComparable>();
                properties2add.Add(PropertyId, value);
                StructuredPropertiesUpdate propadd = new StructuredPropertiesUpdate(properties2add, null);

                var sourcevertex = edge.GetSourceVertex();
                var targetvertex = edge.GetTargetVertex();

                VertexUpdateDefinition upddef;

                if (bHyperEdge)
                {
                    // add list with only one item -> edges to update
                    List<SingleEdgeUpdateDefinition> edges2update = new List<SingleEdgeUpdateDefinition>();
                    edges2update.Add(
                        new SingleEdgeUpdateDefinition(
                            new VertexInformation(sourcevertex.VertexTypeID, sourcevertex.VertexID),
                            new VertexInformation(targetvertex.VertexTypeID, targetvertex.VertexID),
                            edge.EdgeTypeID, null, propadd, null
                        )
                    );

                    // add list with only one item -> edges to update
                    Dictionary<long, HyperEdgeUpdateDefinition> hedges2update = new Dictionary<long, HyperEdgeUpdateDefinition>();
                    hedges2update.Add(
                        EdgeId,
                        new HyperEdgeUpdateDefinition(
                           edge.EdgeTypeID, null, null, null, null, edges2update
                        )
                    );

                    HyperEdgeUpdate edgeupd = new HyperEdgeUpdate(hedges2update);
                    upddef = new VertexUpdateDefinition(null, null, null, null, null, edgeupd, null);
                }
                else
                {
                    // add list with only one item -> edges to update
                    Dictionary<long, SingleEdgeUpdateDefinition> edges2update = new Dictionary<long, SingleEdgeUpdateDefinition>();
                    edges2update.Add(
                        EdgeId,
                        new SingleEdgeUpdateDefinition(
                            new VertexInformation(sourcevertex.VertexTypeID, sourcevertex.VertexID),
                            new VertexInformation(targetvertex.VertexTypeID, targetvertex.VertexID),
                            edge.EdgeTypeID, null, propadd, null
                        )
                    );

                    SingleEdgeUpdate edgeupd = new SingleEdgeUpdate(edges2update);
                    upddef = new VertexUpdateDefinition(null, null, null, null, edgeupd, null, null);
                }

                // Update the vertex (delete unstructured and add structured)

                _vertexManager.ExecuteManager.VertexStore.UpdateVertex(mySecurityToken, myTransactionToken, vertex.VertexID, vertex.VertexTypeID, upddef);
            }

            #endregion
        }

        private void DefinePropertyOnHyperEdge(
            Int64 myTransactionToken,
            SecurityToken mySecurityToken,
            IHyperEdge edge, 
            UnknownAttributePredefinition aAttribute, 
            long PropertyId, 
            long EdgeId,
            IVertex vertex,
            IEdgeType myType
        )
        {
            bool bFound = false;

            #region cast existing unstructured properties on vertices to structured

            foreach (var property in edge.GetAllUnstructuredProperties())
            {
                if (property.PropertyName.CompareTo(aAttribute.AttributeName) == 0)
                {
                    bFound = true;

                    // Found unstructured property with same name on the vertex
                    var targettype = _baseStorageManager.GetBaseType(aAttribute.AttributeType);
                    IComparable value = null;

                    // Try Convert Type
                    try
                    {
                        value = property.Property.ConvertToIComparable(targettype);
                    }
                    catch (InvalidCastException)
                    {
                        throw new VertexAttributeCastException(aAttribute.AttributeName, property.Property.GetType(), targettype, false);
                    }
                    catch (FormatException)
                    {
                        throw new VertexAttributeCastException(aAttribute.AttributeName, property.Property.GetType(), targettype, true);
                    }

                    // add list with only one item -> unstructured property to delete
                    var unstructdel = new List<string>();
                    unstructdel.Add(property.PropertyName);
                    UnstructuredPropertiesUpdate propdelete = new UnstructuredPropertiesUpdate(null, unstructdel);

                    // add list with only one item -> structured property to add
                    Dictionary<long, IComparable> properties2add = new Dictionary<long, IComparable>();
                    properties2add.Add(PropertyId, value);
                    StructuredPropertiesUpdate propadd = new StructuredPropertiesUpdate(properties2add, null);

                    // add list with only one item -> edges to update
                    Dictionary<long, HyperEdgeUpdateDefinition> edges2update = new Dictionary<long, HyperEdgeUpdateDefinition>();
                    edges2update.Add(
                        EdgeId,
                        new HyperEdgeUpdateDefinition(
                            edge.EdgeTypeID,
                            null, propadd, propdelete
                        )
                    );

                    HyperEdgeUpdate edgeupd = new HyperEdgeUpdate(edges2update);

                    // Update the vertex (delete unstructured and add structured)
                    VertexUpdateDefinition upddef = new VertexUpdateDefinition(null, null, null, null, null, edgeupd, null);
                    _vertexManager.ExecuteManager.VertexStore.UpdateVertex(mySecurityToken, myTransactionToken, vertex.VertexID, vertex.VertexTypeID, upddef);

                    // we can break as we already found the property on the vertex
                    break;
                }
            }

            #endregion

            #region add mandatory attribute if not existing on vertex

            if ((!bFound) && (aAttribute.IsMandatory))
            {
                if (aAttribute.DefaultValue == null) throw new DefineMandatoryWithoutDefaultException(aAttribute.AttributeName, myType.Name);

                // Found unstructured property with same name on the vertex
                var targettype = _baseStorageManager.GetBaseType(aAttribute.AttributeType);
                IComparable value = null;

                // Try Convert Type
                try
                {
                    value = aAttribute.DefaultValue.ConvertToIComparable(targettype);
                }
                catch (InvalidCastException)
                {
                    throw new VertexAttributeCastException(aAttribute.AttributeName, aAttribute.DefaultValue.GetType(), targettype, false);
                }
                catch (FormatException)
                {
                    throw new VertexAttributeCastException(aAttribute.AttributeName, aAttribute.DefaultValue.GetType(), targettype, true);
                }

                // add list with only one item -> structured property to add
                Dictionary<long, IComparable> properties2add = new Dictionary<long, IComparable>();
                properties2add.Add(PropertyId, value);
                StructuredPropertiesUpdate propadd = new StructuredPropertiesUpdate(properties2add, null);

                // add list with only one item -> edges to update
                Dictionary<long, HyperEdgeUpdateDefinition> edges2update = new Dictionary<long, HyperEdgeUpdateDefinition>();
                edges2update.Add(
                    EdgeId,
                    new HyperEdgeUpdateDefinition(
                        edge.EdgeTypeID,
                        null, propadd, null
                    )
                );

                HyperEdgeUpdate edgeupd = new HyperEdgeUpdate(edges2update);

                // Update the vertex (delete unstructured and add structured)
                VertexUpdateDefinition upddef = new VertexUpdateDefinition(null, null, null, null, null, edgeupd, null);
                _vertexManager.ExecuteManager.VertexStore.UpdateVertex(mySecurityToken, myTransactionToken, vertex.VertexID, vertex.VertexTypeID, upddef);
            }

            #endregion
        }

        /// <summary>
        /// Defines specified attributes in the given type and stores them.
        /// </summary>
        /// <param name="myToBeDefinedAttributes">The attributes to be defined</param>
        /// <param name="myTransactionToken">The Int64.</param>
        /// <param name="mySecurityToken">The SecurityToken.</param>
        /// <param name="myType">The type to be altered.</param>
        /// <returns>A dictionary with to be defined attributes and default value</returns>returns>
        protected override Dictionary<long, IComparable> ProcessDefineAttributes(
            IEnumerable<UnknownAttributePredefinition> myToBeDefinedAttributes,
            Int64 myTransactionToken,
            SecurityToken mySecurityToken,
            IEdgeType myType)
        {
            Dictionary<long, IComparable> dict = null;

            foreach (var aAttribute in myToBeDefinedAttributes)
            {
                dict = dict ?? new Dictionary<long, IComparable>();

                var id = _idManager
                            .GetVertexTypeUniqeID((long)BaseTypes.Attribute)
                            .GetNextID();

                dict.Add(id, aAttribute.DefaultValue);

                foreach (var vertextype in _vertexTypeManager.ExecuteManager.GetAllTypes(myTransactionToken, mySecurityToken))
                {
                    foreach (var vertex in _vertexManager.ExecuteManager.VertexStore.GetVerticesByTypeID(mySecurityToken, myTransactionToken, vertextype.ID))
                    {
                        foreach (var outedge in vertex.GetAllOutgoingSingleEdges(new PropertyHyperGraphFilter.OutgoingSingleEdgeFilter((ID, edge) => (edge.EdgeTypeID == myType.ID))))
                        {
                            ISingleEdge edge = outedge.Edge;
                            if (edge == null) continue;

                            DefinePropertyOnSingleEdge(myTransactionToken, mySecurityToken, edge, aAttribute, id, outedge.PropertyID, vertex, myType);
                        }

                        foreach (var outedge in vertex.GetAllOutgoingHyperEdges(new PropertyHyperGraphFilter.OutgoingHyperEdgeFilter((ID, edge) => (edge.EdgeTypeID == myType.ID))))
                        {
                            IHyperEdge edge = outedge.Edge;

                            if (edge == null) continue;

                            DefinePropertyOnHyperEdge(myTransactionToken, mySecurityToken, edge, aAttribute, id, outedge.PropertyID, vertex, myType);

                            foreach (var containededge in edge.GetAllEdges(new PropertyHyperGraphFilter.SingleEdgeFilter((cedge) => (cedge.EdgeTypeID == myType.ID))))
                            {
                                DefinePropertyOnSingleEdge(myTransactionToken, mySecurityToken, containededge, aAttribute, id, outedge.PropertyID, vertex, myType, true);
                            }
                        }
                    }
                }

                #region add property definition to vertex type

                PropertyMultiplicity multiplicity = PropertyMultiplicity.Single;
                switch (aAttribute.Multiplicity)
                {
                    case UnknownAttributePredefinition.LISTMultiplicity: multiplicity = PropertyMultiplicity.List; break;
                    case UnknownAttributePredefinition.SETMultiplicity: multiplicity = PropertyMultiplicity.Set; break;
                    default: multiplicity = PropertyMultiplicity.Single; break;
                }

                _baseStorageManager.StoreProperty(
                    _vertexManager.ExecuteManager.VertexStore,
                    new VertexInformation(
                        (long)BaseTypes.Property,
                        id),
                    aAttribute.AttributeName,
                    aAttribute.Comment,
                    DateTime.UtcNow.ToBinary(),
                    aAttribute.IsMandatory,
                    multiplicity,
                    aAttribute.DefaultValue,
                    true,
                    new VertexInformation(
                        (long)BaseTypes.EdgeType,
                        myType.ID),
                    ConvertBasicType(aAttribute.AttributeType),
                    mySecurityToken,
                    myTransactionToken);

                #endregion
            }

            return dict;
        }

        private void UndefinePropertyOnSingleEdge(
            Int64 myTransactionToken,
            SecurityToken mySecurityToken,
            ISingleEdge edge,
            string AttributeName,
            long PropertyId,
            long EdgeId,
            IVertex vertex,
            IEdgeType myType,
            bool bHyperEdge = false
        )
        {
            IComparable property;

            try
            {
                property = edge.GetProperty(PropertyId);
            }
            catch (CouldNotFindStructuredEdgePropertyException)
            {
                return;
            }
          
            // The property is set on this vertex
            string value = null;

            // Try Convert Type
            try
            {
                value = Convert.ToString(property);
            }
            catch (InvalidCastException)
            {
                throw new VertexAttributeCastException(AttributeName, property.GetType(), typeof(String), false);
            }
            catch (FormatException)
            {
                throw new VertexAttributeCastException(AttributeName, property.GetType(), typeof(String), true);
            }

            // add list with only one item -> structured property to delete
            var structdel = new List<long>();
            structdel.Add(PropertyId);
            StructuredPropertiesUpdate propdelete = new StructuredPropertiesUpdate(null, structdel);

            // add list with only one item -> unstructured property to add
            Dictionary<string, Object> properties2add = new Dictionary<string, Object>();
            properties2add.Add(AttributeName, value);
            UnstructuredPropertiesUpdate propadd = new UnstructuredPropertiesUpdate(properties2add, null);

            var sourcevertex = edge.GetSourceVertex();
            var targetvertex = edge.GetTargetVertex();

            VertexUpdateDefinition upddef;

            if (bHyperEdge)
            {
                // add list with only one item -> edges to update
                List<SingleEdgeUpdateDefinition> edges2update = new List<SingleEdgeUpdateDefinition>();
                edges2update.Add(
                    new SingleEdgeUpdateDefinition(
                        new VertexInformation(sourcevertex.VertexTypeID, sourcevertex.VertexID),
                        new VertexInformation(targetvertex.VertexTypeID, targetvertex.VertexID),
                        edge.EdgeTypeID, null, propdelete, propadd
                    )
                );

                // add list with only one item -> edges to update
                Dictionary<long, HyperEdgeUpdateDefinition> hedges2update = new Dictionary<long, HyperEdgeUpdateDefinition>();
                hedges2update.Add(
                    EdgeId,
                    new HyperEdgeUpdateDefinition(
                        edge.EdgeTypeID, null, null, null, null, edges2update
                    )
                );

                HyperEdgeUpdate edgeupd = new HyperEdgeUpdate(hedges2update);
                upddef = new VertexUpdateDefinition(null, null, null, null, null, edgeupd, null);
            }
            else
            {
                // add list with only one item -> edges to update
                Dictionary<long, SingleEdgeUpdateDefinition> edges2update = new Dictionary<long, SingleEdgeUpdateDefinition>();
                edges2update.Add(
                    EdgeId,
                    new SingleEdgeUpdateDefinition(
                        new VertexInformation(sourcevertex.VertexTypeID, sourcevertex.VertexID),
                        new VertexInformation(targetvertex.VertexTypeID, targetvertex.VertexID),
                        edge.EdgeTypeID, null, propdelete, propadd
                    )
                );

                SingleEdgeUpdate edgeupd = new SingleEdgeUpdate(edges2update);
                upddef = new VertexUpdateDefinition(null, null, null, null, edgeupd, null, null);
            }

            // Update the vertex (delete unstructured and add structured)

            _vertexManager.ExecuteManager.VertexStore.UpdateVertex(mySecurityToken, myTransactionToken, vertex.VertexID, vertex.VertexTypeID, upddef);
        }

        private void UndefinePropertyOnHyperEdge(
            Int64 myTransactionToken,
            SecurityToken mySecurityToken,
            IHyperEdge edge,
            string AttributeName,
            long PropertyId,
            long EdgeId,
            IVertex vertex,
            IEdgeType myType
        )
        {
            IComparable property;

            try
            {
                property = edge.GetProperty(PropertyId);
            }
            catch (CouldNotFindStructuredEdgePropertyException)
            {
                return;
            }
            
            // The property is set on this vertex
            string value = null;

            // Try Convert Type
            try
            {
                value = Convert.ToString(property);
            }
            catch (InvalidCastException)
            {
                throw new VertexAttributeCastException(AttributeName, property.GetType(), typeof(String), false);
            }
            catch (FormatException)
            {
                throw new VertexAttributeCastException(AttributeName, property.GetType(), typeof(String), true);
            }

            // add list with only one item -> structured property to delete
            var structdel = new List<long>();
            structdel.Add(PropertyId);
            StructuredPropertiesUpdate propdelete = new StructuredPropertiesUpdate(null, structdel);

            // add list with only one item -> unstructured property to add
            Dictionary<string, Object> properties2add = new Dictionary<string, Object>();
            properties2add.Add(AttributeName, value);
            UnstructuredPropertiesUpdate propadd = new UnstructuredPropertiesUpdate(properties2add, null);

            // add list with only one item -> edges to update
            Dictionary<long, HyperEdgeUpdateDefinition> edges2update = new Dictionary<long, HyperEdgeUpdateDefinition>();
            edges2update.Add(
                EdgeId,
                new HyperEdgeUpdateDefinition(
                    edge.EdgeTypeID,
                    null, propdelete, propadd
                )
            );

            HyperEdgeUpdate edgeupd = new HyperEdgeUpdate(edges2update);

            // Update the vertex (delete unstructured and add structured)
            VertexUpdateDefinition upddef = new VertexUpdateDefinition(null, null, null, null, null, edgeupd, null);
            _vertexManager.ExecuteManager.VertexStore.UpdateVertex(mySecurityToken, myTransactionToken, vertex.VertexID, vertex.VertexTypeID, upddef);

        }

        /// <summary>
        /// Undefines specified attributes in the given type.
        /// </summary>
        /// <param name="myToBeDefinedAttributes">The attributes to be undefined</param>
        /// <param name="myTransactionToken">The Int64.</param>
        /// <param name="mySecurityToken">The SecurityToken.</param>
        /// <param name="myType">The type to be altered.</param>
        /// <returns>A list containing IDs of undefined attributes</returns>returns>
        protected override IEnumerable<long> ProcessUndefineAttributes(
            IEnumerable<String> myToBeUndefinedAttributes,
            Int64 myTransactionToken,
            SecurityToken mySecurityToken,
            IEdgeType myType)
        {
            List<long> undefined = null;

            foreach (var aAttribute in myToBeUndefinedAttributes)
            {
                #region remove related indices
                var propertyDefinition = myType.GetPropertyDefinition(aAttribute);

                foreach (var aIndexDefinition in propertyDefinition.InIndices)
                {
                    _indexManager.RemoveIndexInstance(aIndexDefinition.ID,
                                                        myTransactionToken,
                                                        mySecurityToken);
                }

                #endregion

                undefined = undefined ?? new List<long>();
                undefined.Add(propertyDefinition.ID);

                foreach (var vertextype in _vertexTypeManager.ExecuteManager.GetAllTypes(myTransactionToken, mySecurityToken))
                {
                    foreach (var vertex in _vertexManager.ExecuteManager.VertexStore.GetVerticesByTypeID(mySecurityToken, myTransactionToken, vertextype.ID))
                    {
                        foreach (var outedge in vertex.GetAllOutgoingSingleEdges(new PropertyHyperGraphFilter.OutgoingSingleEdgeFilter((ID, edge) => (edge.EdgeTypeID == myType.ID))))
                        {
                            ISingleEdge edge = outedge.Edge;
                            if (edge == null) continue;

                            UndefinePropertyOnSingleEdge(myTransactionToken, mySecurityToken, edge, aAttribute, propertyDefinition.ID, outedge.PropertyID, vertex, myType);
                        }

                        foreach (var outedge in vertex.GetAllOutgoingHyperEdges(new PropertyHyperGraphFilter.OutgoingHyperEdgeFilter((ID, edge) => (edge.EdgeTypeID == myType.ID))))
                        {
                            IHyperEdge edge = outedge.Edge;

                            if (edge == null) continue;

                            UndefinePropertyOnHyperEdge(myTransactionToken, mySecurityToken, edge, aAttribute, propertyDefinition.ID, outedge.PropertyID, vertex, myType);

                            foreach (var containededge in edge.GetAllEdges(new PropertyHyperGraphFilter.SingleEdgeFilter((cedge) => (cedge.EdgeTypeID == myType.ID))))
                            {
                                UndefinePropertyOnSingleEdge(myTransactionToken, mySecurityToken, containededge, aAttribute, propertyDefinition.ID, outedge.PropertyID, vertex, myType, true);
                            }
                        }
                    }
                }

                // remove the property definition from the vertex type
                _vertexManager.ExecuteManager.VertexStore.RemoveVertex(mySecurityToken,
                                                                        myTransactionToken,
                                                                        propertyDefinition.ID,
                                                                        (long)BaseTypes.Property);

            }

            return undefined;
        }

        /// <summary>
        /// Renames attributes.
        /// </summary>
        /// <param name="myToBeRenamedAttributes">The to be renamed properties.</param>
        /// <param name="myType">The to be altered type.</param>
        /// <param name="myTransactionToken">The Int64.</param>
        /// <param name="mySecurityToken">The SecurityToken.</param>
        protected override void RenameAttributes(Dictionary<string, string> myToBeRenamedAttributes, 
                                                    IEdgeType myType, 
                                                    Int64 myTransactionToken, 
                                                    SecurityToken mySecurityToken)
        {
            if (!myToBeRenamedAttributes.IsNotNullOrEmpty())
                return;

            foreach (var aToBeRenamedAttribute in myToBeRenamedAttributes)
            {
                VertexUpdateDefinition update = new VertexUpdateDefinition(null, 
                                                                            new StructuredPropertiesUpdate(
                                                                                new Dictionary<long, IComparable> 
                                                                                    { { (long)AttributeDefinitions.AttributeDotName, 
                                                                                          aToBeRenamedAttribute.Value } }));

                var attribute = myType.GetAttributeDefinition(aToBeRenamedAttribute.Key);

                switch (attribute.Kind)
                {
                    case AttributeType.Property:
                        _vertexManager.ExecuteManager.VertexStore.UpdateVertex(mySecurityToken, 
                                                                                myTransactionToken, 
                                                                                attribute.ID, 
                                                                                (long)BaseTypes.Property, 
                                                                                update);
                        break;

                    case AttributeType.IncomingEdge:
                        throw new InvalidTypeException(AttributeType.IncomingEdge.ToString(), AttributeType.Property.ToString());

                    case AttributeType.OutgoingEdge:
                        throw new InvalidTypeException(AttributeType.OutgoingEdge.ToString(), AttributeType.Property.ToString());

                    case AttributeType.BinaryProperty:
                        throw new InvalidTypeException(AttributeType.BinaryProperty.ToString(), AttributeType.Property.ToString());

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Change the comment on the type.
        /// </summary>
        /// <param name="myType">The to be altered type.</param>
        /// <param name="myNewComment">The new comment.</param>
        /// <param name="myTransactionToken">The Int64.</param>
        /// <param name="mySecurityToken">The SecurityToken.</param>
        protected override void ChangeCommentOnType(IEdgeType myType,
                                                    string myNewComment,
                                                    Int64 myTransactionToken,
                                                    SecurityToken mySecurityToken)
        {
            if (!String.IsNullOrWhiteSpace(myNewComment))
            {
                var update = new VertexUpdateDefinition(myNewComment);

                _vertexManager.ExecuteManager.VertexStore.UpdateVertex(mySecurityToken,    
                                                                        myTransactionToken, 
                                                                        myType.ID, 
                                                                        (long)BaseTypes.EdgeType, 
                                                                        update);
            }
        }

        /// <summary>
        /// Renames a type.
        /// </summary>
        /// <param name="myType">The to be altered type.</param>
        /// <param name="myNewTypeName">The new type name.</param>
        /// <param name="myTransactionToken">The Int64.</param>
        /// <param name="mySecurityToken">The SecurityToken.</param>
        protected override void RenameType(IEdgeType myType, 
                                            string myNewTypeName, 
                                            Int64 myTransactionToken, 
                                            SecurityToken mySecurityToken)
        {
            if (!String.IsNullOrWhiteSpace(myNewTypeName))
            {
                var update = new VertexUpdateDefinition(null, 
                                                        new StructuredPropertiesUpdate(
                                                            new Dictionary<long, IComparable> 
                                                                { { (long)AttributeDefinitions.BaseTypeDotName, 
                                                                      myNewTypeName } }));

                _vertexManager.ExecuteManager.VertexStore.UpdateVertex(mySecurityToken, 
                                                                        myTransactionToken, 
                                                                        myType.ID, 
                                                                        (long)BaseTypes.EdgeType, 
                                                                        update);
            }
        }

        /// <summary>
        /// Calls the RebuildIndices method of the index manager.
        /// </summary>
        /// <param name="myTransactionToken">The Int64.</param>
        /// <param name="mySecurityToken">The SecurityToken.</param>
        protected override void CallRebuildIndices(Int64 myTransactionToken,
                                                    SecurityToken mySecurityToken)
        {
            _indexManager.RebuildIndices((long)BaseTypes.EdgeType,
                                            myTransactionToken,
                                            mySecurityToken);
        }

        #endregion
        
        private void StoreEdgeType(LinkedList<ATypePredefinition> myDefsTopologically,
                                        Dictionary<String, TypeInfo> myTypeInfos,
                                        long myCreationDate,
                                        int myResultPos,
                                        Int64 myTransactionToken,
                                        SecurityToken mySecurityToken,
                                        ref IVertex[] myResult)
        {
            //now we store each vertex type
            for (var current = myDefsTopologically.First; current != null; current = current.Next)
            {
                var newEdgeType = _baseStorageManager.StoreEdgeType(
                     _vertexManager.ExecuteManager.VertexStore,
                     myTypeInfos[current.Value.TypeName].VertexInfo,
                     current.Value.TypeName,
                     current.Value.Comment,
                     myCreationDate,
                     false,
                     current.Value.IsSealed,
                     true,
                     myTypeInfos[current.Value.SuperTypeName].VertexInfo,
                     mySecurityToken,
                     myTransactionToken);

                myResult[myResultPos++] = newEdgeType;

                _indexManager.GetIndex(BaseUniqueIndex.EdgeTypeDotName)
                                .Add(current.Value.TypeName, myTypeInfos[current.Value.TypeName].VertexInfo.VertexID);

                _nameIndex.Add(current.Value.TypeName, myTypeInfos[current.Value.TypeName].VertexInfo.VertexID);
            }
        }

        /// <summary>
        /// Removes attributes.
        /// </summary>
        /// <param name="myToBeRemovedProperties">To be removed Proerties.</param>
        /// <param name="myType">The to be altered type.</param>
        /// <param name="myTransactionToken">The Int64.</param>
        /// <param name="mySecurityToken">The SecurityToken.</param>
        /// <returns>A list with the deleted property id's.</returns>
        private IEnumerable<long> RemoveAttributes(IEnumerable<string> myToBeRemovedProperties,
                                                    IEdgeType myType,
                                                    Int64 myTransactionToken,
                                                    SecurityToken mySecurityToken)
        {
            if (myToBeRemovedProperties.IsNotNullOrEmpty())
                return ProcessPropertyRemoval(myToBeRemovedProperties, 
                                                myType, 
                                                myTransactionToken, 
                                                mySecurityToken);

            return null;
        }

        /// <summary>
        /// Undefines attributes.
        /// </summary>
        /// <param name="myToBeUndefinedAttributes">To be undefined Attributes.</param>
        /// <param name="myType">The to be altered type.</param>
        /// <param name="myTransactionToken">The Int64.</param>
        /// <param name="mySecurityToken">The SecurityToken.</param>
        /// <returns>A list with the undefined attribute id's.</returns>
        private IEnumerable<long> UndefineAttributes(IEnumerable<string> myToBeUndefinedAttributes,
                                                    IEdgeType myType,
                                                    Int64 myTransactionToken,
                                                    SecurityToken mySecurityToken)
        {
            if (myToBeUndefinedAttributes.IsNotNullOrEmpty())
                return ProcessUndefineAttributes(myToBeUndefinedAttributes,
                                                myTransactionToken,
                                                mySecurityToken,
                                                myType);

            return null;
        }

        /// <summary>
        /// Adds attributes.
        /// </summary>
        /// <param name="myToBeAddedProperties">The to be added properties.</param>
        /// <param name="myType">The to be altered type.</param>
        /// <param name="myTransactionToken">The Int64.</param>
        /// <param name="mySecurityToken">The SecurityToken.</param>
        /// <returns>A dictionary with to be added attributes and default value.</returns>
        private Dictionary<long, IComparable> AddAttributes(
            IEnumerable<PropertyPredefinition> myToBeAddedProperties,
            IEdgeType myType,
            Int64 myTransactionToken,
            SecurityToken mySecurityToken)
        {

            if (myToBeAddedProperties.IsNotNullOrEmpty())
                return ProcessAddPropery(myToBeAddedProperties,
                                            myTransactionToken,
                                            mySecurityToken,
                                            myType);

            return null;
        }

        /// <summary>
        /// Defines attributes.
        /// </summary>
        /// <param name="myToBeDefinedAttributes">The to be defined attributes.</param>
        /// <param name="myType">The to be altered type.</param>
        /// <param name="myTransactionToken">The Int64.</param>
        /// <param name="mySecurityToken">The SecurityToken.</param>
        /// <returns>A dictionary with to be defined attributes and default value.</returns>
        private Dictionary<long, IComparable> DefineAttributes(
            IEnumerable<UnknownAttributePredefinition> myToBeDefinedAttributes,
            IEdgeType myType,
            Int64 myTransactionToken,
            SecurityToken mySecurityToken)
        {

            if (myToBeDefinedAttributes.IsNotNullOrEmpty())
                return ProcessDefineAttributes(myToBeDefinedAttributes,
                                            myTransactionToken,
                                            mySecurityToken,
                                            myType);

            return null;
        }

        #endregion

    }
}
