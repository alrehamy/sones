using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.TypeSystem;
using sones.GraphDS.Services.RemoteAPIService.ServiceContracts.VertexInstanceService;
using sones.GraphDS.Services.RemoteAPIService.DataContracts.InstanceObjects;
using sones.GraphDS.Services.RemoteAPIService.ServiceConverter;
using sones.Library.PropertyHyperGraph;
using sones.Library.Commons.Security;
using sones.GraphDS.Services.RemoteAPIService.DataContracts;
using sones.GraphDS.Services.RemoteAPIService.ErrorHandling;
using sones.GraphDS.Services.RemoteAPIService.DataContracts.PayloadObjects;

namespace sones.GraphDS.Services.RemoteAPIService.ServiceContractImplementation
{
    public partial class RPCServiceContract : IVertexService
    {
        public bool HasIncomingVertices(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex, long myVertexTypeID, long myEdgePropertyID)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.HasIncomingVertices(myVertexTypeID, myEdgePropertyID);
        }

        public List<ServiceIncomingVerticesContainer> GetAllIncomingVertices(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.GetAllIncomingVertices().Select(
                x => new ServiceIncomingVerticesContainer 
                    { 
                        VertexTypeID = x.VertexTypeID, 
                        EdgePropertyID = x.EdgePropertyID, 
                        IncomingVertices = x.IncomingVertices.Select(y => new ServiceVertexInstance(y)).ToList()
                    } ).ToList();
        }

        public List<ServiceVertexInstance> GetIncomingVertices(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex, long myVertexTypeID, long myEdgePropertyID)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.GetIncomingVertices(myVertexTypeID, myEdgePropertyID).Select(x => new ServiceVertexInstance(x)).ToList();
        }

        public bool HasOutgoingEdge(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex, long myEdgePropertyID)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.HasOutgoingEdge(myEdgePropertyID);
        }

        public List<ServiceEdgeInstance> GetAllOutgoingEdges(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.GetAllOutgoingEdges().Select<EdgeContainer, ServiceEdgeInstance>(x =>
            {
                if (x.Edge is ISingleEdge)
                    return new ServiceSingleEdgeInstance(x.Edge as ISingleEdge, x.PropertyID);
                else
                    return new ServiceHyperEdgeInstance(x.Edge as IHyperEdge, x.PropertyID);
            }).ToList();
        }

        public List<ServiceHyperEdgeInstance> GetAllOutgoingHyperEdges(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.GetAllOutgoingHyperEdges().Select(x => new ServiceHyperEdgeInstance(x.Edge, x.PropertyID)).ToList();
        }

        public List<ServiceSingleEdgeInstance> GetAllOutgoingSingleEdges(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.GetAllOutgoingSingleEdges().Select(x => new ServiceSingleEdgeInstance(x.Edge, x.PropertyID)).ToList();
        }

        public ServiceEdgeInstance GetOutgoingEdge(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex, long myEdgePropertyID)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            var Edge = Response.GetOutgoingEdge(myEdgePropertyID);
            if(Edge is ISingleEdge)
                return new ServiceSingleEdgeInstance(Edge as ISingleEdge, myEdgePropertyID);
            else
                return new ServiceHyperEdgeInstance(Edge as IHyperEdge, myEdgePropertyID);
        }

        public ServiceHyperEdgeInstance GetOutgoingHyperEdge(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex, long myEdgePropertyID)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            var value = Response.GetOutgoingHyperEdge(myEdgePropertyID);
            if (value != null)
            {
                return new ServiceHyperEdgeInstance(Response.GetOutgoingHyperEdge(myEdgePropertyID), myEdgePropertyID);
            }
            return null;
            
        }

        public ServiceSingleEdgeInstance GetOutgoingSingleEdge(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex, long myEdgePropertyID)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            var value = Response.GetOutgoingSingleEdge(myEdgePropertyID);
            if (value != null)
            {
                return new ServiceSingleEdgeInstance(value, myEdgePropertyID);
            }
            return null;                            
        }

        public object GetProperty(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex, long myPropertyID)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.GetProperty(myPropertyID);
        }

        public bool HasProperty(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex, long myPropertyID)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.HasProperty(myPropertyID);
        }

        public int GetCountOfProperties(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.GetCountOfProperties();
        }

        public List<ServicePropertyContainer> GetAllProperties(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.GetAllProperties().Select(x => new ServicePropertyContainer { PropertyID = x.PropertyID, Property = (object)x.Property }).ToList();
        }

        public string GetPropertyAsString(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex, long myPropertyID)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.GetPropertyAsString(myPropertyID);
        }

        public object GetUnstructuredProperty(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex, string myPropertyName)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.GetUnstructuredProperty<object>(myPropertyName);
        }

        public bool HasUnstructuredProperty(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex, string myPropertyName)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.HasUnstructuredProperty(myPropertyName);
        }

        public int GetCountOfUnstructuredProperties(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.GetCountOfUnstructuredProperties();
        }

        public List<ServiceUnstructuredPropertyContainer> GetAllUnstructuredProperties(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.GetAllUnstructuredProperties().Select(_ => new ServiceUnstructuredPropertyContainer { PropertyName = _.PropertyName, Property = _.Property }).ToList();
        }

        public string GetUnstructuredPropertyAsString(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex, string myPropertyName)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.GetUnstructuredPropertyAsString(myPropertyName);
        }

        public string Comment(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.Comment;
        }

        public long CreationDate(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.CreationDate;
        }

        public long ModificationDate(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.ModificationDate;
        }

        public ServiceVertexStatistics VertexStatistics(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return new ServiceVertexStatistics(Response.Statistics);
        }

        public Int64 PartitionID(SecurityToken mySecurityToken, Int64 myTransToken, ServiceVertexInstance myVertex)
        {
            var Request = ServiceRequestFactory.MakeRequestGetVertex(myVertex.TypeID, myVertex.VertexID);
            var Response = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
            return Response.PartitionInformation.PartitionID;
        }
    }
}
