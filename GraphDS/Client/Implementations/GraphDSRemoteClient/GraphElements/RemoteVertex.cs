using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDS.GraphDSRemoteClient.sonesGraphDSRemoteAPI;
using sones.Library.PropertyHyperGraph;
using System.IO;

namespace sones.GraphDS.GraphDSRemoteClient.GraphElements
{
    internal class RemoteVertex : ARemoteGraphElement, IVertex
    {
        #region Data

        /// <summary>
        /// The edition of the vertex
        /// </summary>
        private string _edition;

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


        #region Constructor

        internal RemoteVertex(ServiceVertexInstance myVertex, IServiceToken myServiceToken) : base(myServiceToken)
        {
            this._edition = myVertex.Edition;
            this._vertexID = myVertex.VertexID;
            this._vertexTypeID = myVertex.TypeID;
            this._vertexRevisionID = myVertex.VertexRevisionID;
        }

        #endregion


        #region ARemoteGraphElement
        
        public override string Comment
        {
            get
            {
                return _ServiceToken.VertexService.CommentByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this));
            }
        }

        public override long CreationDate
        {
            get
            {
                return _ServiceToken.VertexService.CreationDateByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this));
            }
        }

        public override long ModificationDate
        {
            get
            {
                return _ServiceToken.VertexService.ModificationDateByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this));
            }
        }

        public IDictionary<long, IComparable> StructuredProperties
        {
            get
            {
                return _ServiceToken.VertexService.GetAllPropertiesByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this))
                    .ToDictionary(k => k.PropertyID, v => (IComparable)v.Property);
            }
        }

        public IDictionary<string, object> UnstructuredProperties
        {
            get
            {
                return _ServiceToken.VertexService.GetAllUnstructuredPropertiesByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this))
                    .ToDictionary(k => k.PropertyName, v => v.Property);
            }
        }

        #endregion


        #region IVertex

        public bool HasIncomingVertices(long myVertexTypeID, long myEdgePropertyID)
        {
            return _ServiceToken.VertexService.HasIncomingVertices(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this), myVertexTypeID, myEdgePropertyID);
        }

        public IEnumerable<IncomingVerticesContainer> GetAllIncomingVertices(PropertyHyperGraphFilter.IncomingVerticesFilter myFilter = null)
        {
            return _ServiceToken.VertexService.GetAllIncomingVertices(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this))
                .Select(x => new IncomingVerticesContainer { VertexTypeID = x.VertexTypeID, EdgePropertyID = x.EdgePropertyID, IncomingVertices = x.IncomingVertices.Select(y => (IVertex)new RemoteVertex(y, _ServiceToken)) });
        }

        public IEnumerable<IVertex> GetIncomingVertices(long myVertexTypeID, long myEdgePropertyID)
        {
            return _ServiceToken.VertexService.GetIncomingVertices(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this), myVertexTypeID, myEdgePropertyID)
                .Select(x => (IVertex)new RemoteVertex(x, _ServiceToken));
        }

        public bool HasOutgoingEdge(long myEdgePropertyID)
        {
            return _ServiceToken.VertexService.HasOutgoingEdgeByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this), myEdgePropertyID);
        }

        public IEnumerable<EdgeContainer> GetAllOutgoingEdges(PropertyHyperGraphFilter.OutgoingEdgeFilter myFilter = null)
        {
            return _ServiceToken.VertexService.GetAllOutgoingEdges(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this))
                .Select(x =>
                    {
                        if (x is ServiceSingleEdgeInstance)
                            return new EdgeContainer { PropertyID = x.EdgePropertyID.Value, Edge = new RemoteSingleEdge((ServiceSingleEdgeInstance)x, _ServiceToken) };
                        else
                            return new EdgeContainer { PropertyID = x.EdgePropertyID.Value, Edge = new RemoteHyperEdge((ServiceHyperEdgeInstance)x, _ServiceToken) };
                    });
        }

        public IEnumerable<HyperEdgeContainer> GetAllOutgoingHyperEdges(PropertyHyperGraphFilter.OutgoingHyperEdgeFilter myFilter = null)
        {
            return _ServiceToken.VertexService.GetAllOutgoingHyperEdges(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this))
                .Select(x => new HyperEdgeContainer {  PropertyID = x.EdgePropertyID.Value, Edge = new RemoteHyperEdge(x, _ServiceToken) });
        }

        public IEnumerable<SingleEdgeContainer> GetAllOutgoingSingleEdges(PropertyHyperGraphFilter.OutgoingSingleEdgeFilter myFilter = null)
        {
            return _ServiceToken.VertexService.GetAllOutgoingSingleEdges(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this))
                .Select(x => new SingleEdgeContainer { PropertyID = x.EdgePropertyID.Value, Edge = new RemoteSingleEdge(x, _ServiceToken) } );
        }

        public IEdge GetOutgoingEdge(long myEdgePropertyID)
        {
            var svcEdge = _ServiceToken.VertexService.GetOutgoingEdge(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this), myEdgePropertyID);
            if (svcEdge is ServiceSingleEdgeInstance)
                return new RemoteSingleEdge((ServiceSingleEdgeInstance)svcEdge, _ServiceToken);
            else
                return new RemoteHyperEdge((ServiceHyperEdgeInstance)svcEdge, _ServiceToken);
        }

        public IHyperEdge GetOutgoingHyperEdge(long myEdgePropertyID)
        {
            return new RemoteHyperEdge(
                _ServiceToken.VertexService.GetOutgoingHyperEdge(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this), myEdgePropertyID),
                _ServiceToken);
        }

        public ISingleEdge GetOutgoingSingleEdge(long myEdgePropertyID)
        {
            return new RemoteSingleEdge(
                _ServiceToken.VertexService.GetOutgoingSingleEdge(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this), myEdgePropertyID),
                _ServiceToken);
        }

        public Stream GetBinaryProperty(long myPropertyID)
        {
            return _ServiceToken.StreamedService.GetBinaryProperty(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this), myPropertyID);
        }

        public IEnumerable<BinaryPropertyContainer> GetAllBinaryProperties(PropertyHyperGraphFilter.BinaryPropertyFilter myFilter = null)
        {
            return _ServiceToken.StreamedService.GetAllBinaryProperties(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this)).Select(_ => new BinaryPropertyContainer { PropertyID = _.PropertyID, BinaryPropery = _.BinaryPropery}).ToList();
        }

        public T GetProperty<T>(long myPropertyID)
        {
            return (T)_ServiceToken.VertexService.GetPropertyByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this), myPropertyID);
        }

        public IComparable GetProperty(long myPropertyID)
        {
            return (IComparable)_ServiceToken.VertexService.GetPropertyByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this), myPropertyID);
        }

        public bool HasProperty(long myPropertyID)
        {
            return _ServiceToken.VertexService.HasPropertyByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this), myPropertyID);
        }

        public int GetCountOfProperties()
        {
            return _ServiceToken.VertexService.GetCountOfPropertiesByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this));
        }

        public IEnumerable<PropertyContainer> GetAllProperties(PropertyHyperGraphFilter.GraphElementStructuredPropertyFilter myFilter = null)
        {
            return _ServiceToken.VertexService.GetAllPropertiesByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this))
                .Select(x => new PropertyContainer {  PropertyID = x.PropertyID,  Property = (IComparable)x.Property });
        }

        public string GetPropertyAsString(long myPropertyID)
        {
            return _ServiceToken.VertexService.GetPropertyAsStringByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this), myPropertyID);
        }

        public T GetUnstructuredProperty<T>(string myPropertyName)
        {
            return (T)_ServiceToken.VertexService.GetUnstructuredPropertyByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this), myPropertyName);
        }

        public bool HasUnstructuredProperty(string myPropertyName)
        {
            return _ServiceToken.VertexService.HasUnstructuredPropertyByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this), myPropertyName);
        }

        public int GetCountOfUnstructuredProperties()
        {
            return _ServiceToken.VertexService.GetCountOfUnstructuredPropertiesByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this));
        }

        public IEnumerable<UnstructuredPropertyContainer> GetAllUnstructuredProperties(PropertyHyperGraphFilter.GraphElementUnStructuredPropertyFilter myFilter = null)
        {
            return _ServiceToken.VertexService.GetAllUnstructuredPropertiesByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this)).Select(_ => new UnstructuredPropertyContainer { PropertyName = _.PropertyName, Property = _.Property}).ToList();
        }

        public string GetUnstructuredPropertyAsString(string myPropertyName)
        {
            return _ServiceToken.VertexService.GetUnstructuredPropertyAsStringByVertexInstance(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this), myPropertyName);
        }

        public long VertexTypeID
        {
            get { return this._vertexTypeID; }
        }

        public long VertexID
        {
            get { return this._vertexID; }
        }

        public long VertexRevisionID
        {
            get { return this._vertexRevisionID; }
        }

        public string EditionName
        {
            get { return this._edition; }
        }

        public IVertexStatistics Statistics
        {
            get
            {
                return new RemoteVertexStatistics(_ServiceToken.VertexService.VertexStatistics(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this)));
            }
        }

        public IGraphPartitionInformation PartitionInformation
        {
            get
            {
                return new RemotePartitionInformation(_ServiceToken.VertexService.PartitionID(_ServiceToken.SecurityToken, _ServiceToken.TransactionToken, new ServiceVertexInstance(this)));
            }
        }

        #endregion


        #region inner classes

        internal class RemoteVertexStatistics : IVertexStatistics
        {
            private ulong _OutDegree;
            private ulong _InDegree;
            private long _Visits;

            internal RemoteVertexStatistics(ServiceVertexStatistics myStatistics)
            {
                _OutDegree = myStatistics.OutDegree;
                _InDegree = myStatistics.InDegree;
                _Visits = myStatistics.Visits;
            }

            public ulong InDegree { get { return _InDegree; } }
            public ulong OutDegree { get { return _OutDegree; } }
            public ulong Degree { get { return _InDegree + _OutDegree; } }
            public long Visits { get { return _Visits; } }
        }

        internal class RemotePartitionInformation : IGraphPartitionInformation
        {
            private long _PartitionID;

            internal RemotePartitionInformation(long myPartitionID)
            {
                _PartitionID = myPartitionID;
            }

            public long PartitionID { get { return _PartitionID; } }
        }

        #endregion
    }
}
