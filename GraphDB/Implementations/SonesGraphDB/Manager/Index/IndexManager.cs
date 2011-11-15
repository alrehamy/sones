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
using System.Linq;
using sones.GraphDB.ErrorHandling;
using sones.GraphDB.Extensions;
using sones.GraphDB.Manager.BaseGraph;
using sones.GraphDB.Manager.Plugin;
using sones.GraphDB.Manager.TypeManagement;
using sones.GraphDB.Request;
using sones.GraphDB.Settings;
using sones.GraphDB.TypeManagement.Base;
using sones.GraphDB.TypeSystem;
using sones.Library.CollectionWrapper;
using sones.Library.Commons.Security;
using sones.Library.Commons.VertexStore;
using sones.Library.Commons.VertexStore.Definitions;
using sones.Library.LanguageExtensions;
using sones.Library.PropertyHyperGraph;
using sones.Library.Settings;
using sones.Library.VersionedPluginManager;
using sones.Plugins.Index;
using sones.Plugins.Index.Persistent;
using sones.Plugins.Index.Range;
using sones.Plugins.Index.Versioned;
using sones.Plugins.Index.Helper;


namespace sones.GraphDB.Manager.Index
{
    /// <summary>
    /// This class represents an index manager.
    /// </summary>
    /// The responsibilities of the index manager are creating, removing und retrieving of indices.
    /// Each database has one index manager.
    public sealed class IndexManager : IIndexManager, IManager
    {
        #region data

        /// <summary>
        /// The plugin manager that is used to generate new instances of indices
        /// </summary>
        private readonly GraphDBPluginManager _pluginManager;

        /// <summary>
        /// The potential parameters for plugin-indices
        /// </summary>
        private readonly Dictionary<string, PluginDefinition> _indexPluginParameter;

        private IVertexStore _vertexStore;

        private IManagerOf<ITypeHandler<IVertexType>> _vertexTypeManager;

        private BaseGraphStorageManager _baseStorageManager;

        private Dictionary<long, ISonesIndex> _indices = new Dictionary<long, ISonesIndex>();
        private IDManager _idManager;
        private ISonesIndex _ownIndex;

        private GraphApplicationSettings _applicationSettings;

        #endregion

        #region constructor

        /// <summary>
        /// Create a new index manager
        /// </summary>
        /// <param name="myVertexStore">The vertex store of the graphDB</param>
        /// <param name="myPluginManager">The sones graphDB plugin manager</param>
        /// <param name="myPluginDefinitions">The parameters for plugin-indices</param>
        public IndexManager(IDManager myIDManager, GraphDBPluginManager myPluginManager, GraphApplicationSettings myApplicationSettings, List<PluginDefinition> myPluginDefinitions = null)
        {
            _idManager = myIDManager;
            _pluginManager = myPluginManager;
            _applicationSettings = myApplicationSettings;

            _indexPluginParameter = myPluginDefinitions != null 
                ? myPluginDefinitions.ToDictionary(key => key.NameOfPlugin, value => value) 
                : new Dictionary<string, PluginDefinition>();
        }

        #endregion

        #region IIndexManager Members

        public IIndexDefinition CreateIndex(IndexPredefinition myIndexDefinition, SecurityToken mySecurity, Int64 myTransaction, bool myIsUserDefined = true)
        {
            myIndexDefinition.CheckNull("myIndexDefinition");

            if (myIndexDefinition.Name != null && myIndexDefinition.Name.StartsWith("sones"))
                throw new Exception("It is not allowed to add an index with a name, that starts with 'sones'.");

            var vertexType = _vertexTypeManager.ExecuteManager.GetType(myIndexDefinition.VertexTypeName, myTransaction, mySecurity);
            
            var indexName = myIndexDefinition.Name ?? CreateIndexName(myIndexDefinition, vertexType);

            if (_ownIndex.ContainsKey(indexName))
                //TODO a better exception here.
                throw new Exception("An index with that name already exists.");

            if (myIndexDefinition.Properties == null)
                throw new Exception("Index without properties is not allowed.");

            foreach (var prop in myIndexDefinition.Properties)
            {
                var propDef = vertexType.GetPropertyDefinition(prop);
                if (!vertexType.HasProperty(prop) || (propDef.RelatedType.ID != vertexType.ID && !HasIndex(propDef, mySecurity, myTransaction)))
                    //TODO a better exception here.
                    throw new AttributeDoesNotExistException("The property is not defined on the vertex type " + vertexType.Name + ", it is defined on a parent type.");
            }

            var indexID = _idManager.GetVertexTypeUniqeID((long)BaseTypes.Index).GetNextID();
            var info = new VertexInformation((long)BaseTypes.Index, indexID, 0, myIndexDefinition.Edition);

            var typeClass = myIndexDefinition.TypeName ?? GetBestMatchingIndexName(false, false);
            var parameter = (_indexPluginParameter.ContainsKey(typeClass))
                    ? _indexPluginParameter[typeClass].PluginParameter
                    : new Dictionary<string, object>();
            var options = ValidateOptions(myIndexDefinition.IndexOptions, typeClass);

            // load propertyIDs for indexed properties
            var propertyIDs = myIndexDefinition.Properties.Select(prop => vertexType.GetPropertyDefinition(prop).ID).ToList();
            // add propertyIDs for indexing
            parameter.Add(IndexConstants.PROPERTY_IDS_OPTIONS_KEY, propertyIDs);
            
            parameter = FillOptions(parameter, options);
            // HACK: manually inject eventuall existing PersistenceLocation (btk, 23.09.2011)
            #region Hack
            if (_applicationSettings.Get<PersistenceLocation>() != null)
            {
                // if not already, initialize
                if (parameter == null)
                    parameter = new Dictionary<string, object>();

                if (!parameter.ContainsKey("Path")) // only add when not already in there...
                    parameter.Add("Path", _applicationSettings.Get<PersistenceLocation>());
            }
            #endregion


            var index = _pluginManager.GetAndInitializePlugin<ISonesIndex>(typeClass, parameter, indexID);
            var props = myIndexDefinition.Properties.Select(prop => new VertexInformation((long)BaseTypes.Property, vertexType.GetPropertyDefinition(prop).ID)).ToList();

            var date = DateTime.UtcNow.ToBinary();


            var indexVertex = _baseStorageManager.StoreIndex(
                                _vertexStore,
                                info,
                                indexName,
                                myIndexDefinition.Comment,
                                date,
                                myIndexDefinition.TypeName,
                                //GetIsSingleValue(index),
                                GetIsRangeValue(index),
                                GetIsVersionedValue(index),
                                true,
                                myIndexDefinition.IndexOptions,
                                new VertexInformation((long)BaseTypes.VertexType, vertexType.ID),
                                null,
                                props,
                                mySecurity,
                                myTransaction);

            _ownIndex.Add(indexName, indexID);
            _indices.Add(indexID, index);

            foreach (var childType in vertexType.GetDescendantVertexTypes())
            {
                var childID = _idManager.GetVertexTypeUniqeID((long)BaseTypes.Index).GetNextID();
                var childName = CreateIndexName(myIndexDefinition, childType);

    
                var childIndex = _pluginManager.GetAndInitializePlugin<ISonesIndex>(typeClass, parameter, childID);

                _baseStorageManager.StoreIndex(
                                _vertexStore,
                                new VertexInformation((long)BaseTypes.Index, childID),
                                childName,
                                indexName, //we store the source index name as comment
                                date,
                                myIndexDefinition.TypeName,
                                //GetIsSingleValue(index),
                                GetIsRangeValue(index),
                                GetIsVersionedValue(index),
                                false,
                                myIndexDefinition.IndexOptions,
                                new VertexInformation((long)BaseTypes.VertexType, childType.ID),
                                info,
                                props,
                                mySecurity,
                                myTransaction);

                _ownIndex.Add(childName, childID);
                _indices.Add(childID, childIndex);

            }

            

            var indexDefinition = _baseStorageManager.CreateIndexDefinition(indexVertex, vertexType);

            _vertexTypeManager.ExecuteManager.CleanUpTypes();

            var reloadedVertexType = _vertexTypeManager.ExecuteManager.GetType(vertexType.Name, myTransaction, mySecurity);

            foreach(var type in reloadedVertexType.GetDescendantVertexTypesAndSelf())
            {
                RebuildIndices(type, myTransaction, mySecurity);
            }

            return indexDefinition;
        }

        private Dictionary<String, object> ValidateOptions(IDictionary<String, object> myOptions, string myIndexType)
        {
            if (myOptions == null)
                return null;

            var parameters = _pluginManager.GetPluginParameter<ISonesIndex>(myIndexType);

            var result = new Dictionary<String, object>();

            foreach (var option in myOptions)
            {
                if (!parameters.ContainsKey(option.Key))
                    throw new UnknownOptionException(option.Key, parameters);

                var value = option.Value;

                if (value != null && !parameters[option.Key].IsAssignableFrom(value.GetType()))
                {
                    try
                    {
                        value = Convert.ChangeType(option.Value, parameters[option.Key]);
                    }
                    catch (Exception ex)
                    {
                        throw new IllegalOptionException(option.Key, parameters[option.Key], ex);
                    }
                }

                result[option.Key] = value;
            }

            return result;
        }

        private string CreateIndexName(IndexPredefinition myIndexDefinition, IVertexType vertexType)
        {
            var propNames = string.Join("AND", myIndexDefinition.Properties);

            int count = 0;
            string result;
            do
            {
                result = string.Join("_", "sones", propNames, vertexType.Name, count++);
            } while (_ownIndex.ContainsKey(result));

            return result;
        }

        public bool HasIndex(IPropertyDefinition myPropertyDefinition, SecurityToken mySecurityToken, Int64 myTransactionToken)
        {
            return myPropertyDefinition.InIndices.CountIsGreater(0);
        }

        public bool HasIndex(String myIndexName)
        {
            return _ownIndex.ContainsKey(myIndexName);
        }

        public IEnumerable<ISonesIndex> GetIndices(IPropertyDefinition myPropertyDefinition, SecurityToken mySecurityToken, Int64 myTransactionToken)
        {
            return myPropertyDefinition.InIndices.Select(_ => _indices[_.ID]);
        }

        public IEnumerable<ISonesIndex> GetIndices(IVertexType myVertexType, IList<IPropertyDefinition> myPropertyDefinition, SecurityToken mySecurityToken, Int64 myTransactionToken)
        {
            myVertexType.CheckNull("myVertexType");
            myPropertyDefinition.CheckNull("myPropertyDefinition");
            
            if (myPropertyDefinition.Count == 0)
                throw new ArgumentOutOfRangeException("myPropertyDefinition", "At least one property must be given.");

            var propertyTypes = myPropertyDefinition.GroupBy(_ => _.RelatedType);
            foreach (var group in propertyTypes)
            {
                if (!myVertexType.IsDescendantOrSelf(group.Key))
                {
                    throw new ArgumentException(string.Format("The properties ({0}) defined on type {1} is not part of inheritance hierarchy of {2}.", 
                        string.Join(",", group.Select(_ => _.Name)), 
                        group.Key.Name, 
                        myVertexType.Name));
                }
            }

            var result = myVertexType.GetIndexDefinitions(false).Where(_ => myPropertyDefinition.SequenceEqual(_.IndexedProperties)).Select(_=>_indices[_.ID]).ToArray();

            return result;

        }

        public string GetBestMatchingIndexName(bool myIsRange, bool myIsVersioned)
        {
            if (myIsRange || myIsVersioned)
            {
                throw new NotImplementedException("It's currently not supported to use ranged or versioned indices");
            }

            return _applicationSettings.Get<DefaultIndexImplementation>();
        }

        public IEnumerable<IIndexDefinition> DescribeIndices(Int64 myTransactionToken, SecurityToken mySecurityToken)
        {   
            var vertexTypes = _vertexTypeManager.ExecuteManager.GetAllTypes(myTransactionToken, mySecurityToken);

            foreach (var type in vertexTypes)
            {
                foreach (var idx in type.GetIndexDefinitions(false))
                {
                    yield return idx;
                }
            }

            yield break;
        }

        public IEnumerable<IIndexDefinition> DescribeIndex(String myTypeName, String myIndexName, String myEdition, Int64 myTransaction, SecurityToken mySecurity)
        {
            var vertextype = _vertexTypeManager.ExecuteManager.GetType(myTypeName, myTransaction, mySecurity);

            var indices = vertextype.GetIndexDefinitions(true);

            if (!string.IsNullOrWhiteSpace(myIndexName))
            {
                indices = indices.Where(x => myIndexName.Equals(x.Name));
            }
            
            return indices;
        }

        public void RemoveIndexInstance(long myIndexID, Int64 myTransaction, SecurityToken mySecurity)
        {
            var vertex = _vertexStore.GetVertex(mySecurity, myTransaction, myIndexID, (long)BaseTypes.Index, String.Empty);
            if (_vertexStore.RemoveVertex(mySecurity, myTransaction, myIndexID, (long)BaseTypes.Index))
            {
                var def = _baseStorageManager.CreateIndexDefinition(vertex);
                _ownIndex.Remove(def.Name);
                _indices.Remove(myIndexID);
            }
            else
            {
                throw new ArgumentOutOfRangeException("myIndexID", "No index available with that id.");
            }
        }


        public void RebuildIndices(long myVertexTypeID, Int64 myTransactionToken, SecurityToken mySecurityToken)
        {
            var vertexType = _vertexTypeManager.ExecuteManager.GetType(myVertexTypeID, myTransactionToken, mySecurityToken);

            RebuildIndices(vertexType, myTransactionToken, mySecurityToken, false);
        }

        public void RebuildIndices(IVertexType myVertexType, Int64 myTransactionToken, SecurityToken mySecurityToken)
        {
            RebuildIndices(myVertexType, myTransactionToken, mySecurityToken, false);
        }

        private void RebuildIndices(IVertexType myVertexType, Int64 myTransaction, SecurityToken mySecurity, bool myOnlyNonPersistent )
        {
            Dictionary<IList<IPropertyDefinition>, IEnumerable<ISonesIndex>> toRebuild = new Dictionary<IList<IPropertyDefinition>, IEnumerable<ISonesIndex>>();
            foreach (var indexDef in myVertexType.GetIndexDefinitions(false))
            {                
                var indices = GetIndices(myVertexType, indexDef.IndexedProperties, mySecurity, myTransaction).Where(_=>!myOnlyNonPersistent || !GetIsPersistentIndex(_));
                toRebuild.Add(indexDef.IndexedProperties, indices);
            }

            if (toRebuild.Count > 0)
            {
                foreach (var aIdxCollection in toRebuild.Values)
                {
                    foreach (var aIdx in aIdxCollection)
                    {
                        aIdx.Clear();
                    }
                }

                var vertices = _vertexStore.GetVerticesByTypeID(mySecurity, myTransaction, myVertexType.ID);

                foreach (var vertex in vertices)
                {
                    foreach (var indexGroup in toRebuild)
                    {
                        foreach (var index in indexGroup.Value)
                        {
                            index.Add(CreateIndexKey(indexGroup.Key, vertex), vertex.VertexID);
                        }
                    }
                }
            }

        }

        private IComparable CreateIndexKey(IList<IPropertyDefinition> myIndexProps, IVertex vertex)
        {
            if (myIndexProps.Count > 1)
            {
                List<IComparable> values = new List<IComparable>(myIndexProps.Count);
                for (int i = 0; i < myIndexProps.Count; i++)
                {
                    values[i] = myIndexProps[i].GetValue(vertex);
                }

                //using ListCollectionWrapper from Expressions, maybe this class should go to Lib
                return new ListCollectionWrapper(values);
            }
            else if (myIndexProps.Count == 1)
            {
                return myIndexProps[0].GetValue(vertex);
            }
            throw new ArgumentOutOfRangeException("myIndexProps", "At least one property must be indexed.");
        }


        #endregion

        #region IManager Members

        public void Initialize(IMetaManager myMetaManager)
        {
            _vertexTypeManager = myMetaManager.VertexTypeManager;
            _vertexStore = myMetaManager.VertexStore;
            _baseStorageManager = myMetaManager.BaseGraphStorageManager;
        }

        public void Load(Int64 myTransaction, SecurityToken mySecurity)
        {
            var maxID = Int64.MinValue;

            var vertices = _vertexStore.GetVerticesByTypeID(mySecurity, myTransaction, (long)BaseTypes.Index);
            foreach (var indexVertex in vertices)
            {
                var def = _baseStorageManager.CreateIndexDefinition(indexVertex);
                var vertexType = def.VertexType;
                var indexID = def.ID;
                maxID = Math.Max(maxID, def.ID);
                var typeClass = def.IndexTypeName ?? GetBestMatchingIndexName(def.IsRange, def.IsVersioned);
                var parameter = (_indexPluginParameter.ContainsKey(typeClass))
                        ? _indexPluginParameter[typeClass].PluginParameter
                        : new Dictionary<string, object>();
                
                var options = (indexVertex.GetAllUnstructuredProperties() == null)
                                ? null
                                : indexVertex.GetAllUnstructuredProperties().Select(_ => new KeyValuePair<String, object>(_.Item1, _.Item2));

                // add propertyIDs for indexing
                parameter.Add(IndexConstants.PROPERTY_IDS_OPTIONS_KEY, def.IndexedProperties.Select(propDef => propDef.ID).ToList());
                parameter = FillOptions(parameter, options);

                // HACK: manually inject eventuall existing PersistenceLocation (btk, 23.09.2011)
                #region Hack
                if (_applicationSettings.Get<PersistenceLocation>() != null)
                {
                    // if not already, initialize
                    if (parameter == null)
                        parameter = new Dictionary<string, object>();

                    if (!parameter.ContainsKey("Path")) // only add when not already in there...
                        parameter.Add("Path", _applicationSettings.Get<PersistenceLocation>());
                }
                #endregion

                var index = _pluginManager.GetAndInitializePlugin<ISonesIndex>(typeClass, parameter, indexID);

                _indices.Add(indexID, index);
                if (def.Name == "IndexDotName")
                    _ownIndex = index;
            }

            _idManager.GetVertexTypeUniqeID((long)BaseTypes.Index).SetToMaxID(maxID);

            RebuildIndices(_vertexTypeManager.ExecuteManager.GetType((long)BaseTypes.BaseType, 
                                                                        myTransaction, 
                                                                        mySecurity), 
                            myTransaction, mySecurity, true);
            RebuildIndices(_vertexTypeManager.ExecuteManager.GetType((long)BaseTypes.VertexType, 
                                                                        myTransaction, 
                                                                        mySecurity), 
                            myTransaction, mySecurity, true);
            RebuildIndices(_vertexTypeManager.ExecuteManager.GetType((long)BaseTypes.EdgeType, 
                                                                        myTransaction, 
                                                                        mySecurity), 
                            myTransaction, mySecurity, true);
            RebuildIndices(_vertexTypeManager.ExecuteManager.GetType((long)BaseTypes.Index, 
                                                                        myTransaction, 
                                                                        mySecurity), 
                            myTransaction, mySecurity, true);
        }

        private Dictionary<string, object> FillOptions(IDictionary<string, object> myParameters, IEnumerable<KeyValuePair<string, object>> myOptions)
        {
            if (myParameters == null && myOptions == null)
                return null;

            var result = (myParameters != null)
                            ? new Dictionary<String, object>(myParameters)
                            : new Dictionary<String, object>();

            if (myOptions != null)
                foreach (var option in myOptions)
                    result[option.Key] = option.Value;

            return result;
        }

        #endregion

        private static bool GetIsVersionedValue(ISonesIndex myIndex)
        {
            return myIndex is ISonesVersionedIndex;
        }

        private static bool GetIsRangeValue(ISonesIndex myIndex)
        {
            return myIndex is ISonesRangeIndex;
        }

        private static bool GetIsPersistentIndex(ISonesIndex myIndex)
        {
            return myIndex is ISonesPersistentIndex;
        }

        //private bool GetIsSingleValue(IIndex<IComparable, long> index)
        //{
        //    return index is ISingleValueIndex<IComparable, Int64>;
        //}

        #region IIndexManager Members


        public IEnumerable<ISonesIndex> GetIndices(IVertexType myVertexType, IPropertyDefinition myPropertyDefinition, SecurityToken mySecurityToken, Int64 myTransactionToken)
        {
            myVertexType.CheckNull("myVertexType");
            return myVertexType.GetIndexDefinitions(false).Where(_=>_.IndexedProperties.Count == 1 && _.IndexedProperties.Contains(myPropertyDefinition)).Select(_ => _indices[_.ID]);
            //return myPropertyDefinition.InIndices.Where(_ => myVertexType.Equals(_.VertexType)).Select(_ => _indices[_.ID]);
            
        }

        public void DropIndex(RequestDropIndex myDropIndexRequest, Int64 myTransactionToken, SecurityToken mySecurityToken)
        {
            var vertexType = _vertexTypeManager.ExecuteManager.GetType(myDropIndexRequest.TypeName, myTransactionToken, mySecurityToken);

            if (!String.IsNullOrEmpty(myDropIndexRequest.IndexName))
            {
                //so there is an index name

                var indexDefinitions = vertexType.GetIndexDefinitions(false);

                if (indexDefinitions != null && indexDefinitions.Count() > 0)
                {
                    if (!String.IsNullOrEmpty(myDropIndexRequest.Edition))
                    {
                        //so there is also an edition
                        ProcessDropIndex(indexDefinitions.Where(_ => _.SourceIndex == null && _.IndexTypeName == myDropIndexRequest.IndexName && _.Edition == myDropIndexRequest.Edition), vertexType, mySecurityToken, myTransactionToken);
                    }
                    else
                    {
                        //no edition
                        ProcessDropIndex(indexDefinitions.Where(_ => _.SourceIndex == null && _.Name == myDropIndexRequest.IndexName), vertexType, mySecurityToken, myTransactionToken);
                    }

                    _vertexTypeManager.ExecuteManager.CleanUpTypes();
                }
            }
        }

        private void ProcessDropIndex(IEnumerable<IIndexDefinition> myToBeDroppedIndices, IVertexType vertexType, SecurityToken mySecurityToken, Int64 myTransactionToken)
        {
            foreach (var aVertexType in vertexType.GetDescendantVertexTypesAndSelf())
            {
                foreach (var aIndexDefinition in myToBeDroppedIndices)
                {
                    RemoveIndexInstance(aIndexDefinition.ID, myTransactionToken, mySecurityToken);
                }
            }
        }

        public ISonesIndex GetIndex(BaseUniqueIndex myIndex)
        {
            return _indices[(long)myIndex];
        }

        #endregion


        public ISonesIndex GetIndex(string myIndexName, SecurityToken mySecurity, Int64 myTransaction)
        {
            IEnumerable<long> values;

            _ownIndex.TryGetValues(myIndexName, out values);

            return (values.Count() > 0) ? _indices[values.First()] : null;
        }

        public void Shutdown()
        {
            //TODO
        }
    }
}
