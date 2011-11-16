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
using System.IO;
using sones.GraphFS.Element.Edge;
using sones.GraphFS.ErrorHandling;
using sones.Library.PropertyHyperGraph;
using sones.Library.Commons.VertexStore.Definitions;
using sones.Library.Commons.VertexStore.Definitions.Update;
using sones.Library.BinaryStreamStructure;
using sones.Library.PropertyHyperGraph.ErrorHandling;

namespace sones.GraphFS.Element.Vertex
{
    /// <summary>
    /// The in memory representation of an ivertex
    /// </summary>
    public sealed class InMemoryVertex : AGraphElement, IVertex
    {
        #region data

        private readonly object _lockobject = new object();

        /// <summary>
        /// Used to calculated the hashcode once
        /// </summary>
        private readonly Int32 _hashcode = 0;

        /// <summary>
        /// determines whether this vertex is a bulk vertex
        /// </summary>
        public Boolean IsBulkVertex = true;

        /// <summary>
        /// The binary properties of this vertex
        /// </summary>
        private IDictionary<Int64, Stream> _binaryProperties;

        /// <summary>
        /// The edition of the vertex
        /// </summary>
        private string _edition;

        /// <summary>
        /// The incoming edges of the vertex
        /// (VertexTypeID of the vertex type that points to this vertex, PropertyID of the edge that points to this vertex, Incoming vertices)
        /// </summary>
        public IDictionary<Int64, Dictionary<Int64, IncomingEdgeCollection>> IncomingEdges;

        /// <summary>
        /// The outgoing edges of the vertex
        /// </summary>
        public Dictionary<Int64, IEdge> OutgoingEdges;

        /// <summary>
        /// The id of the vertex
        /// </summary>
        private readonly Int64 _vertexID;

        /// <summary>
        /// The vertex type id
        /// </summary>
        private readonly Int64 _vertexTypeID;

        /// <summary>
        /// The revision id of the vertex
        /// </summary>
        private readonly Int64 _vertexRevisionID;
       
        #endregion

        #region constructor

        /// <summary>
        /// Creates a new in memory vertex
        /// </summary>
        /// <param name="myVertexID">The id of this vertex</param>
        /// <param name="myVertexTypeID">The id of the vertex type</param>
        /// <param name="myVertexRevisionID">The revision id of this vertex</param>
        /// <param name="myEdition">The edition of this vertex</param>
        /// <param name="myBinaryProperties">The binary properties of this vertex</param>
        /// <param name="myOutgoingEdges">The outgoing edges of this vertex</param>
        /// <param name="myComment">The comment on this graph element</param>
        /// <param name="myCreationDate">The creation date of this element</param>
        /// <param name="myModificationDate">The modification date of this element</param>
        /// <param name="myStructuredProperties">The structured properties of this element</param>
        /// <param name="myUnstructuredProperties">The unstructured properties of this element</param>
        public InMemoryVertex(
            Int64 myVertexID,
            Int64 myVertexTypeID,
            Int64 myVertexRevisionID,
            String myEdition,
            IDictionary<long, Stream> myBinaryProperties,
            IDictionary<long, IEdge> myOutgoingEdges,
            String myComment,
            long myCreationDate,
            long myModificationDate,
            IDictionary<Int64, IComparable> myStructuredProperties,
            IDictionary<String, Object> myUnstructuredProperties)
            :base(myComment, myCreationDate, myModificationDate, myStructuredProperties, myUnstructuredProperties)
        {
            _vertexID = myVertexID;
            _vertexTypeID = myVertexTypeID;
            _vertexRevisionID = myVertexRevisionID;
            _edition = myEdition;

            if (myBinaryProperties != null)
            {
                _binaryProperties = new Dictionary<Int64, Stream>();

                foreach (var item in myBinaryProperties)
                {
                    _binaryProperties.Add(item.Key, CopyBinaryStream(item.Value));
                }
            }

            OutgoingEdges = (Dictionary<long, IEdge>)myOutgoingEdges;
            
            IsBulkVertex = false;

            _hashcode = (_vertexID ^ _vertexTypeID).GetHashCode();
        }

        #endregion

        #region private methods

        /// <summary>
        /// Creates a new bulk vertex
        /// </summary>
        /// <param name="myVertexID">The id of the vertex</param>
        /// <param name="myVertexTypeID">The vertex type id</param>
        private InMemoryVertex(
            Int64 myVertexID,
            Int64 myVertexTypeID)
        {
            _vertexID = myVertexID;
            _vertexTypeID = myVertexTypeID;

            IsBulkVertex = true;

            _hashcode = (_vertexID ^ _vertexTypeID).GetHashCode();
        }

        /// <summary>
        /// Copy a binary stream.
        /// </summary>
        /// <param name="myBinStream">The binary stream, which is to copy.</param>
        /// <returns>A copy of the stream.</returns>
        private Stream CopyBinaryStream(Stream myBinStream)
        {
            var tmpStream = new MemoryStream();
            myBinStream.CopyTo(tmpStream);
            tmpStream.Position = 0;
            return tmpStream;            
        }

        #endregion

        #region IVertex Members

        public bool HasIncomingVertices(long myVertexTypeID, long myEdgePropertyID)
        {
            return IncomingEdges != null &&
                   IncomingEdges.ContainsKey(myVertexTypeID) &&
                   IncomingEdges[myVertexTypeID].ContainsKey(myEdgePropertyID);
        }

        public IEnumerable<IncomingVerticesContainer> GetAllIncomingVertices(
            PropertyHyperGraphFilter.IncomingVerticesFilter myFilter = null)
        {
            if (IncomingEdges != null)
            {
                foreach (var aType in IncomingEdges)
                {
                    foreach (var aEdge in aType.Value)
                    {
                        if (myFilter != null)
                        {
                            if (myFilter(aType.Key, aEdge.Key, aEdge.Value))
                            {
                                yield return new IncomingVerticesContainer { VertexTypeID = aType.Key, EdgePropertyID = aEdge.Key, IncomingVertices = aEdge.Value };
                            }
                        }
                        else
                        {
                            yield return new IncomingVerticesContainer { VertexTypeID = aType.Key, EdgePropertyID = aEdge.Key, IncomingVertices = aEdge.Value };
                        }
                    }
                }
            }

            yield break;
        }

        public IEnumerable<IVertex> GetIncomingVertices(Int64 myVertexTypeID, Int64 myEdgePropertyID)
        {
            return HasIncomingVertices(myVertexTypeID, myEdgePropertyID)
                       ? IncomingEdges[myVertexTypeID][myEdgePropertyID]
                       : new IncomingEdgeCollection(0);
        }

        public bool HasOutgoingEdge(long myEdgePropertyID)
        {
            return OutgoingEdges != null &&
                   OutgoingEdges.ContainsKey(myEdgePropertyID);
        }

        public IEnumerable<EdgeContainer> GetAllOutgoingEdges(PropertyHyperGraphFilter.OutgoingEdgeFilter myFilter = null)
        {
            if (OutgoingEdges != null)
            {
                foreach (var aEdge in OutgoingEdges)
                {
                    if (myFilter != null)
                    {
                        if (myFilter(aEdge.Key, aEdge.Value))
                        {
                            yield return new EdgeContainer { PropertyID = aEdge.Key, Edge = aEdge.Value };
                        }
                    }
                    else
                    {
                        yield return new EdgeContainer { PropertyID = aEdge.Key, Edge = aEdge.Value };
                    }
                }
            }

            yield break;
        }

        public IEnumerable<HyperEdgeContainer> GetAllOutgoingHyperEdges(
            PropertyHyperGraphFilter.OutgoingHyperEdgeFilter myFilter = null)
        {
            if (OutgoingEdges != null)
            {
                foreach (var aEdge in OutgoingEdges)
                {
                    var interestingEdge = aEdge.Value as IHyperEdge;

                    if (interestingEdge == null) continue;

                    if (myFilter != null)
                    {
                        if (myFilter(aEdge.Key, interestingEdge))
                        {
                            yield return new HyperEdgeContainer { PropertyID = aEdge.Key, Edge = interestingEdge };
                        }
                    }
                    else
                    {
                        yield return new HyperEdgeContainer { PropertyID = aEdge.Key, Edge = interestingEdge };
                    }
                }
            }

            yield break;
        }

        public IEnumerable<SingleEdgeContainer> GetAllOutgoingSingleEdges(
            PropertyHyperGraphFilter.OutgoingSingleEdgeFilter myFilter = null)
        {
            if (OutgoingEdges != null)
            {
                foreach (var aEdge in OutgoingEdges)
                {
                    var interestingEdge = aEdge.Value as ISingleEdge;

                    if (interestingEdge == null) continue;

                    if (myFilter != null)
                    {
                        if (myFilter(aEdge.Key, interestingEdge))
                        {
                            yield return new SingleEdgeContainer { PropertyID = aEdge.Key, Edge = interestingEdge };
                        }
                    }
                    else
                    {
                        yield return new SingleEdgeContainer { PropertyID = aEdge.Key, Edge = interestingEdge };
                    }
                }
            }

            yield break;
        }

        public IEdge GetOutgoingEdge(long myEdgePropertyID)
        {
            return HasOutgoingEdge(myEdgePropertyID) ? OutgoingEdges[myEdgePropertyID] : null;
        }

        public IHyperEdge GetOutgoingHyperEdge(long myEdgePropertyID)
        {
            var edge = GetOutgoingEdge(myEdgePropertyID);

            return edge as IHyperEdge;
        }

        public ISingleEdge GetOutgoingSingleEdge(long myEdgePropertyID)
        {
            var edge = GetOutgoingEdge(myEdgePropertyID);

            return edge as ISingleEdge;
        }

        public Stream GetBinaryProperty(long myPropertyID)
        {
            if(_binaryProperties != null)
            {
               if(_binaryProperties.ContainsKey(myPropertyID))
               {
                   var prop = _binaryProperties[myPropertyID];

                   return new StreamProxy(prop);
               }
               else
               {
                   throw new BinaryNotExistentException(myPropertyID);
               }
            }
            else
            {
                throw new BinaryNotExistentException(myPropertyID);
            }
        }

        public IEnumerable<BinaryPropertyContainer> GetAllBinaryProperties(PropertyHyperGraphFilter.BinaryPropertyFilter myFilter = null)
        {
            if (_binaryProperties != null)
            {
                foreach (var aBinary in _binaryProperties)
                {
                    if (myFilter != null)
                    {
                        if (myFilter(aBinary.Key, aBinary.Value))
                        {
                            yield return new BinaryPropertyContainer { PropertyID = aBinary.Key, BinaryPropery = aBinary.Value };
                        }
                    }
                    else
                    {
                        yield return new BinaryPropertyContainer { PropertyID = aBinary.Key, BinaryPropery = aBinary.Value };
                    }
                }
            }

            yield break;
        }

        public T GetProperty<T>(long myPropertyID)
        {
            if (HasProperty(myPropertyID))
            {
                return (T) _structuredProperties[myPropertyID];
            }
            
            throw new CouldNotFindStructuredVertexPropertyException(_vertexTypeID,
                                                                    _vertexID, myPropertyID);
        }

        public IComparable GetProperty(long myPropertyID)
        {
            if (HasProperty(myPropertyID))
            {
                return _structuredProperties[myPropertyID];
            }

            throw new CouldNotFindStructuredVertexPropertyException(_vertexTypeID,
                                                                    _vertexID, myPropertyID);
        }

        public bool HasProperty(long myPropertyID)
        {
            return _structuredProperties != null &&
                   _structuredProperties.ContainsKey(myPropertyID);
        }

        public int GetCountOfProperties()
        {
            return _structuredProperties == null ? 0 : _structuredProperties.Count;
        }

        public IEnumerable<PropertyContainer> GetAllProperties(PropertyHyperGraphFilter.GraphElementStructuredPropertyFilter myFilter = null)
        {
            return GetAllPropertiesProtected(myFilter);
        }

        public string GetPropertyAsString(long myPropertyID)
        {
            if (HasProperty(myPropertyID))
            {
                return _structuredProperties[myPropertyID].ToString();
            }
            
            throw new CouldNotFindStructuredVertexPropertyException(_vertexTypeID,
                                                                    _vertexID, myPropertyID);
        }

        public T GetUnstructuredProperty<T>(string myPropertyName)
        {
            if (HasUnstructuredProperty(myPropertyName))
            {
                return (T) _unstructuredProperties[myPropertyName];
            }
            
            throw new CouldNotFindUnStructuredVertexPropertyException(_vertexTypeID,
                                                                      _vertexID, myPropertyName);
        }

        public bool HasUnstructuredProperty(string myPropertyName)
        {
            return _unstructuredProperties != null &&
                   _unstructuredProperties.ContainsKey(myPropertyName);
        }

        public int GetCountOfUnstructuredProperties()
        {
            return _unstructuredProperties == null ? 0 : _unstructuredProperties.Count;
        }

        public IEnumerable<UnstructuredPropertyContainer> GetAllUnstructuredProperties(
            PropertyHyperGraphFilter.GraphElementUnStructuredPropertyFilter myFilter = null)
        {
            return GetAllUnstructuredPropertiesProtected(myFilter);
        }

        public string GetUnstructuredPropertyAsString(string myPropertyName)
        {
            if (HasUnstructuredProperty(myPropertyName))
            {
                return _unstructuredProperties[myPropertyName].ToString();
            }
            
            throw new CouldNotFindUnStructuredVertexPropertyException(_vertexTypeID,
                                                                      _vertexID, myPropertyName);
        }

        public string Comment
        {
            get { return _comment; }
        }

        public long CreationDate
        {
            get { return _creationDate; }
        }

        public long ModificationDate
        {
            get { return _modificationDate; }
        }

        public long VertexTypeID
        {
            get { return _vertexTypeID; }
        }

        public long VertexID
        {
            get { return _vertexID; }
        }

        public Int64 VertexRevisionID
        {
            get { return _vertexRevisionID; }
        }

        public string EditionName
        {
            get { return _edition; }
        }

        public IVertexStatistics Statistics
        {
            get { return null; }
        }

        public IGraphPartitionInformation PartitionInformation
        {
            get { return null; }
        }

        #endregion

        #region static methods

        /// <summary>
        /// Creates a new InMemoryVertex from an IVertex
        /// </summary>
        /// <param name="aVertex">The vertex template</param>
        /// <returns>A new InMemoryVertex</returns>
        internal static InMemoryVertex CopyFromIVertex(IVertex aVertex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new bulk vertex
        /// </summary>
        /// <param name="myVertexID">The id of the vertex</param>
        /// <param name="myVertexTypeID">The vertex type id</param>
        /// <returns>A new bulk InMemoryVertex</returns>
        internal static InMemoryVertex CreateNewBulkVertex(
            Int64 myVertexID,
            Int64 myVertexTypeID)
        {
            return new InMemoryVertex(myVertexID, myVertexTypeID);
        }

        #endregion

        #region public update methods

        public void UpdateComment(String myComment)
        {
            lock (_lockobject)
            {

                _comment = myComment;
            }
        }

        public void UpdateBinaryProperties(IDictionary<Int64, StreamAddDefinition> myBinaryUpdatedProperties, IEnumerable<Int64> myDeletedBinaryProperties)
        {
            lock (_lockobject)
            {
                if (myDeletedBinaryProperties != null)
                {
                    if (_binaryProperties != null)
                    {
                        foreach (var item in myDeletedBinaryProperties)
                        {
                            _binaryProperties[item].Close();
                            _binaryProperties.Remove(item);
                        }
                    }
                }

                if (myBinaryUpdatedProperties != null)
                {
                    if (_binaryProperties != null)
                    {
                        foreach (var item in myBinaryUpdatedProperties)
                        {
                            if (_binaryProperties.ContainsKey(item.Value.PropertyID))
                            {
                                var streamCopy = CopyBinaryStream(item.Value.Stream);
                                _binaryProperties[item.Value.PropertyID].Close();
                                _binaryProperties[item.Value.PropertyID] = streamCopy;
                            }
                            else
                            {
                                _binaryProperties.Add(item.Value.PropertyID, CopyBinaryStream(item.Value.Stream));
                            }
                        }
                    }
                    else 
                    {
                        _binaryProperties = new Dictionary<Int64, Stream>();

                        foreach (var item in myBinaryUpdatedProperties)
                        {
                            _binaryProperties.Add(item.Value.PropertyID, CopyBinaryStream(item.Value.Stream));
                        }
                    }
                }
            }
        }

        public void UpdateStructuredProperties(StructuredPropertiesUpdate myStructuredUpdates)
        {
            lock (_lockobject)
            {
                if (myStructuredUpdates != null)
                {

                    if (myStructuredUpdates.Deleted != null)
                    {
                        if (_structuredProperties != null)
                        {
                            foreach (var item in myStructuredUpdates.Deleted)
                            {
                                _structuredProperties.Remove(item);
                            }
                        }
                    }

                    if (myStructuredUpdates.Updated != null)
                    {
                        if (_structuredProperties != null)
                        {
                            foreach (var item in myStructuredUpdates.Updated)
                            {
                                if (_structuredProperties.ContainsKey(item.Key))
                                {
                                    _structuredProperties[item.Key] = item.Value;
                                }
                                else
                                {
                                    _structuredProperties.Add(item.Key, item.Value);
                                }
                            }
                        }
                        else
                        {
                            _structuredProperties = new Dictionary<Int64, IComparable>();

                            foreach (var item in myStructuredUpdates.Updated)
                            {
                                _structuredProperties.Add(item.Key, item.Value);
                            }
                        }
                    }
                }

            }
        }

        public void UpdateUnstructuredProperties(UnstructuredPropertiesUpdate myUnstructuredUpdates)
        {
            lock (_lockobject)
            {
                if (myUnstructuredUpdates != null)
                {
                    if (myUnstructuredUpdates.Deleted != null)
                    {
                        if (_unstructuredProperties != null)
                        {
                            foreach (var item in myUnstructuredUpdates.Deleted)
                            {
                                if (_unstructuredProperties != null)
                                    _unstructuredProperties.Remove(item);
                            }
                        }
                    }

                    if (myUnstructuredUpdates.Updated != null)
                    {
                        if (_unstructuredProperties != null)
                        {
                            foreach (var item in myUnstructuredUpdates.Updated)
                            {
                                if (_unstructuredProperties.ContainsKey(item.Key))
                                {
                                    _unstructuredProperties[item.Key] = item.Value;
                                }
                                else
                                {
                                    _unstructuredProperties.Add(item.Key, item.Value);
                                }
                            }
                        }
                        else
                        {
                            _unstructuredProperties = new Dictionary<String, Object>();

                            foreach (var item in myUnstructuredUpdates.Updated)
                            {
                                _unstructuredProperties.Add(item.Key, item.Value);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Equals Overrides

        public override Boolean Equals(Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            var p = obj as InMemoryVertex;

            return p != null && Equals(p);
        }

        public Boolean Equals(InMemoryVertex p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            return _vertexID == p._vertexID
                   && (_vertexTypeID == p._vertexTypeID);
        }

        public static Boolean operator ==(InMemoryVertex a, InMemoryVertex b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Equals(b);
        }

        public static Boolean operator !=(InMemoryVertex a, InMemoryVertex b)
        {
            return !(a == b);
        }

		/// <summary>
		/// Returns the hash code for the current instance.
		/// </summary>
		/// 
		/// <returns>
		/// A hash code for the current object.
		/// </returns>
        public override int GetHashCode()
        {
            return _hashcode;
        }

        #endregion

        /// <summary>
        /// Activates a bulk vertex type
        /// </summary>
        /// <param name="binaryProperties"></param>
        /// <param name="edges"></param>
        /// <param name="Comment"></param>
        /// <param name="CreationDate"></param>
        /// <param name="ModificationDate"></param>
        /// <param name="StructuredProperties"></param>
        /// <param name="UnstructuredProperties"></param>
        internal void Activate(Dictionary<long, Stream> binaryProperties, Dictionary<long, IEdge> edges, string Comment, long CreationDate, long ModificationDate, IDictionary<long, IComparable> StructuredProperties, IDictionary<string, object> UnstructuredProperties)
        {
            lock (_lockobject)
            {
                if (binaryProperties != null)
                {
                    _binaryProperties = new Dictionary<Int64, Stream>();

                    //copy the values
                    foreach (var item in binaryProperties)
                    {
                        _binaryProperties.Add(item.Key, CopyBinaryStream(item.Value));
                    }
                }

                _comment = Comment;
                _creationDate = CreationDate;
                _modificationDate = ModificationDate;
                _structuredProperties = StructuredProperties;
                _unstructuredProperties = UnstructuredProperties;
                OutgoingEdges = edges;

                //so this is no bulk-vertex anymore
                IsBulkVertex = false;
            }
        }
    }
}
