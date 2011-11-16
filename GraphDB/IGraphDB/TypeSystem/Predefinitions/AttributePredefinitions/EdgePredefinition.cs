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
using sones.GraphDB.Request;
using sones.GraphDB.Request.Insert;

namespace sones.GraphDB.TypeSystem

{
    /// <summary>
    /// The definition of an edge.
    /// </summary>
    public sealed class EdgePredefinition: IPropertyProvider
    {
        #region data

        /// <summary>
        /// </summary>
        public String EdgeName { get; private set; }

        /// <summary>
        /// The comment for this edge definition.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The IDs of the vertices where to connect to.
        /// </summary>
        public IDictionary<String, HashSet<long>> VertexIDsByVertexTypeName { get { return _vertexIDsByVertexTypeName; } }
        private Dictionary<String, HashSet<long>> _vertexIDsByVertexTypeName;

        /// <summary>
        /// The IDs of the vertices where to connect to.
        /// </summary>
        public IDictionary<Int64, HashSet<long>> VertexIDsByVertexTypeID { get { return _vertexIDsByVertexTypeID; } }
        private Dictionary<Int64, HashSet<long>> _vertexIDsByVertexTypeID;

        /// <summary>
        /// The well defined properties of a vertex.
        /// </summary>
        public IDictionary<String, IComparable> StructuredProperties { get { return _structured; } }
        private Dictionary<String, IComparable> _structured;

        /// <summary>
        /// The unstructured part of a vertex.
        /// </summary>
        public IDictionary<String, Object> UnstructuredProperties { get { return _unstructured; } }
        private Dictionary<String, Object> _unstructured;

        /// <summary>
        /// The properties where the user does not know if it is structured or not.
        /// </summary>
        public IDictionary<String, Object> UnknownProperties { get { return _unknown; } }
        private Dictionary<String, Object> _unknown;

        /// <summary>
        /// The edges contained by this hyper edge.
        /// </summary>
        public IEnumerable<EdgePredefinition> ContainedEdges { get { return _edges; } }
        private List<EdgePredefinition> _edges;

        #endregion

        #region constructor

        /// <summary>
        /// Creates a new instance of EdgeDefinition.
        /// </summary>
        /// <param name="myEdgeName">The name of the edge.</param>
        public EdgePredefinition(String myEdgeName)
        {
            EdgeName = myEdgeName;

            Comment = String.Empty;
        }

        /// <summary>
        /// Creates a new instance of EdgeDefinition.
        /// </summary>
        /// <remarks>Use this constructor, if a contained edge is to be created.</remarks>
        public EdgePredefinition() 
        {
            EdgeName = String.Empty;

            Comment = String.Empty;
        }

        #endregion

        #region fluent

        /// <summary>
        /// Adds a new structured property
        /// </summary>
        /// <param name="myPropertyName">The name of the property</param>
        /// <param name="myProperty">The value of the property</param>
        /// <returns>The reference of the current object. (fluent interface).</returns>
        public EdgePredefinition AddStructuredProperty(String myPropertyName, IComparable myProperty)
        {
            _structured = _structured ?? new Dictionary<String, IComparable>();
            _structured.Add(myPropertyName, myProperty);

            return this;
        }

        /// <summary>
        /// Adds a new unstructured property
        /// </summary>
        /// <param name="myPropertyName">The name of the property</param>
        /// <param name="myProperty">The value of the property</param>
        /// <returns>The reference of the current object. (fluent interface).</returns>
        public EdgePredefinition AddUnstructuredProperty(String myPropertyName, Object myProperty)
        {
            _unstructured = _unstructured ?? new Dictionary<String, Object>();
            _unstructured.Add(myPropertyName, myProperty);

            return this;
        }

        /// <summary>
        /// Adds a new unknown property
        /// </summary>
        /// <param name="myPropertyName">The name of the property</param>
        /// <param name="myProperty">The value of the property</param>
        /// <returns>The reference of the current object. (fluent interface).</returns>
        public EdgePredefinition AddUnknownProperty(String myPropertyName, Object myProperty)
        {
            _unknown = _unknown ?? new Dictionary<String, Object>();
            _unknown.Add(myPropertyName, myProperty);

            return this;
        }

        /// <summary>
        /// Adds an edge to this edge.
        /// </summary>
        /// <param name="myContainedEdge">The edges that will be contained by this hyper edge.</param>
        /// <returns>The reference of the current object. (fluent interface).</returns>
        public EdgePredefinition AddEdge(EdgePredefinition myContainedEdge)
        {
            _edges = _edges ?? new List<EdgePredefinition>();
            _edges.Add(myContainedEdge);

            return this;

        }

        /// <summary>
        /// Adds a vertex ID to this edge definition..
        /// </summary>
        /// <param name="myVertexID">The vertex ID where to connect to.</param>
        /// <returns>The reference of the current object. (fluent interface).</returns>
        public EdgePredefinition AddVertexID(String myVertexType, long myVertexID)
        {
            var set = EnsureHashSet(myVertexType);
            set.Add(myVertexID);

            return this;
        }

        /// <summary>
        /// Adds a vertex ID to this edge definition..
        /// </summary>
        /// <param name="myVertexTypeID">The vertextype ID where to connect to.</param>
        /// <param name="myVertexID">The vertex ID where to connect to.</param>
        /// <returns>The reference of the current object. (fluent interface).</returns>
        public EdgePredefinition AddVertexID(long myVertexTypeID, long myVertexID)
        {
            var set = EnsureHashSet(myVertexTypeID);
            set.Add(myVertexID);

            return this;
        }

        /// <summary>
        /// Adds verex IDs to this edge definition..
        /// </summary>
        /// <param name="myVertexIDs">The vertex IDs where to connect to.</param>
        /// <returns>The reference of the current object. (fluent interface).</returns>
        public EdgePredefinition AddVertexID(String myVertexType, IEnumerable<long> myVertexIDs)
        {
            var set = EnsureHashSet(myVertexType);
            set.UnionWith(myVertexIDs);

            return this;
        }

        #endregion

        #region IPropertyProvider Members

        IPropertyProvider IPropertyProvider.AddStructuredProperty(string myPropertyName, IComparable myProperty)
        {
            return AddStructuredProperty(myPropertyName, myProperty);
        }

        IPropertyProvider IPropertyProvider.AddUnstructuredProperty(string myPropertyName, object myProperty)
        {
            return AddUnstructuredProperty(myPropertyName, myProperty);
        }

        IPropertyProvider IPropertyProvider.AddUnknownProperty(string myPropertyName, object myProperty)
        {
            return AddUnknownProperty(myPropertyName, myProperty);
        }

        #endregion

        #region IUnknownProvider Members

        void IUnknownProvider.ClearUnknown()
        {
            _unknown = null;
        }

        #endregion


        private HashSet<long> EnsureHashSet(String myVertexType)
        {
            _vertexIDsByVertexTypeName = _vertexIDsByVertexTypeName ?? new Dictionary<String, HashSet<long>>();
            if (!_vertexIDsByVertexTypeName.ContainsKey(myVertexType))
                _vertexIDsByVertexTypeName.Add(myVertexType, new HashSet<long>());

            return _vertexIDsByVertexTypeName[myVertexType];
        }

        private HashSet<long> EnsureHashSet(long myVertexTypeID)
        {
            _vertexIDsByVertexTypeID = _vertexIDsByVertexTypeID ?? new Dictionary<long, HashSet<long>>();
            if (!_vertexIDsByVertexTypeID.ContainsKey(myVertexTypeID))
                _vertexIDsByVertexTypeID.Add(myVertexTypeID, new HashSet<long>());

            return _vertexIDsByVertexTypeID[myVertexTypeID];
        }

    }
    
}