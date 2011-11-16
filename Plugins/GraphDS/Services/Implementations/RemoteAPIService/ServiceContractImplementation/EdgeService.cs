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
using sones.GraphDS.Services.RemoteAPIService.ServiceContracts.VertexInstanceService;
using sones.GraphDS.Services.RemoteAPIService.DataContracts.InstanceObjects;
using sones.GraphDS.Services.RemoteAPIService.ServiceConverter;
using sones.Library.PropertyHyperGraph;
using sones.Library.Commons.Security;
using sones.GraphDS.Services.RemoteAPIService.DataContracts;
using sones.GraphDS.Services.RemoteAPIService.ServiceContracts.EdgeInstanceService;
using sones.GraphDS.Services.RemoteAPIService.ErrorHandling;

namespace sones.GraphDS.Services.RemoteAPIService.ServiceContractImplementation
{
    public partial class RPCServiceContract : IEdgeService
    {
        
        #region IGraphElementService

        public object GetProperty(SecurityToken mySecurityToken, Int64 myTransToken, ServiceEdgeInstance myEdge, long myPropertyID)
        {
            if(myEdge.EdgePropertyID != null)
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(myEdge.SourceVertex.TypeID, myEdge.SourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                return SourceVertex.GetOutgoingEdge((Int64)myEdge.EdgePropertyID).GetProperty(myPropertyID);
            }
            else
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.TypeID,
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                IHyperEdge HyperEdge = SourceVertex.GetOutgoingHyperEdge((Int64)myEdge.EdgePropertyID);
                return HyperEdge.GetAllEdges(delegate(ISingleEdge mySingleEdge){
                    return (mySingleEdge.GetSourceVertex().VertexID == myEdge.SourceVertex.VertexID && mySingleEdge.GetTargetVertex().VertexID == (myEdge as ServiceSingleEdgeInstance).TargetVertex.VertexID);
                }).First<ISingleEdge>().GetProperty(myPropertyID);
            }
        }

        public bool HasProperty(SecurityToken mySecurityToken, Int64 myTransToken, ServiceEdgeInstance myEdge, long myPropertyID)
        {
            if(myEdge.EdgePropertyID != null)
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(myEdge.SourceVertex.TypeID, myEdge.SourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                return SourceVertex.GetOutgoingEdge((Int64)myEdge.EdgePropertyID).HasProperty(myPropertyID);
            }
            else
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.TypeID,
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                IHyperEdge HyperEdge = SourceVertex.GetOutgoingHyperEdge((Int64)myEdge.EdgePropertyID);
                return HyperEdge.GetAllEdges(delegate(ISingleEdge mySingleEdge){
                    return (mySingleEdge.GetSourceVertex().VertexID == myEdge.SourceVertex.VertexID && mySingleEdge.GetTargetVertex().VertexID == (myEdge as ServiceSingleEdgeInstance).TargetVertex.VertexID);
                }).First<ISingleEdge>().HasProperty(myPropertyID);
            }
        }

        public int GetCountOfProperties(SecurityToken mySecurityToken, Int64 myTransToken, ServiceEdgeInstance myEdge)
        {
            if(myEdge.EdgePropertyID != null)
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(myEdge.SourceVertex.TypeID, myEdge.SourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                return SourceVertex.GetOutgoingEdge((Int64)myEdge.EdgePropertyID).GetCountOfProperties();
            }
            else
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.TypeID,
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                IHyperEdge HyperEdge = SourceVertex.GetOutgoingHyperEdge((Int64)myEdge.EdgePropertyID);
                return HyperEdge.GetAllEdges(delegate(ISingleEdge mySingleEdge){
                    return (mySingleEdge.GetSourceVertex().VertexID == myEdge.SourceVertex.VertexID && mySingleEdge.GetTargetVertex().VertexID == (myEdge as ServiceSingleEdgeInstance).TargetVertex.VertexID);
                }).First<ISingleEdge>().GetCountOfProperties();
            }
        }

        public List<ServicePropertyContainer> GetAllProperties(SecurityToken mySecurityToken, Int64 myTransToken, ServiceEdgeInstance myEdge)
        {
            List<ServicePropertyContainer> PropertyCollection;
            if(myEdge.EdgePropertyID != null)
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(myEdge.SourceVertex.TypeID, myEdge.SourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                PropertyCollection = SourceVertex.GetOutgoingEdge((Int64)myEdge.EdgePropertyID).GetAllProperties().Select(_ => new ServicePropertyContainer { PropertyID = _.PropertyID, Property = _.Property }).ToList();
            }
            else
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.TypeID,
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                IHyperEdge HyperEdge = SourceVertex.GetOutgoingHyperEdge((Int64)myEdge.EdgePropertyID);
                PropertyCollection = HyperEdge.GetAllEdges(delegate(ISingleEdge mySingleEdge){
                    return (mySingleEdge.GetSourceVertex().VertexID == myEdge.SourceVertex.VertexID && mySingleEdge.GetTargetVertex().VertexID == (myEdge as ServiceSingleEdgeInstance).TargetVertex.VertexID);
                }).First<ISingleEdge>().GetAllProperties().Select(_ => new ServicePropertyContainer { PropertyID = _.PropertyID, Property = _.Property }).ToList();
            }
            return PropertyCollection;
        }

        public string GetPropertyAsString(SecurityToken mySecurityToken, Int64 myTransToken, ServiceEdgeInstance myEdge, long myPropertyID)
        {
            if(myEdge.EdgePropertyID != null)
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(myEdge.SourceVertex.TypeID, myEdge.SourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                return SourceVertex.GetOutgoingEdge((Int64)myEdge.EdgePropertyID).GetPropertyAsString(myPropertyID);
            }
            else
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.TypeID,
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                IHyperEdge HyperEdge = SourceVertex.GetOutgoingHyperEdge((Int64)myEdge.EdgePropertyID);
                return HyperEdge.GetAllEdges(delegate(ISingleEdge mySingleEdge){
                    return (mySingleEdge.GetSourceVertex().VertexID == myEdge.SourceVertex.VertexID && mySingleEdge.GetTargetVertex().VertexID == (myEdge as ServiceSingleEdgeInstance).TargetVertex.VertexID);
                }).First<ISingleEdge>().GetPropertyAsString(myPropertyID);
            }
        }

        public object GetUnstructuredProperty(SecurityToken mySecurityToken, Int64 myTransToken, ServiceEdgeInstance myEdge, string myPropertyName)
        {
            if(myEdge.EdgePropertyID != null)
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(myEdge.SourceVertex.TypeID, myEdge.SourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                return SourceVertex.GetOutgoingEdge((Int64)myEdge.EdgePropertyID).GetUnstructuredProperty<object>(myPropertyName);
            }
            else
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.TypeID,
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                IHyperEdge HyperEdge = SourceVertex.GetOutgoingHyperEdge((Int64)myEdge.EdgePropertyID);
                return HyperEdge.GetAllEdges(delegate(ISingleEdge mySingleEdge){
                    return (mySingleEdge.GetSourceVertex().VertexID == myEdge.SourceVertex.VertexID && mySingleEdge.GetTargetVertex().VertexID == (myEdge as ServiceSingleEdgeInstance).TargetVertex.VertexID);
                }).First<ISingleEdge>().GetUnstructuredProperty<object>(myPropertyName);
            }
        }

        public bool HasUnstructuredProperty(SecurityToken mySecurityToken, Int64 myTransToken, ServiceEdgeInstance myEdge, string myPropertyName)
        {
            if(myEdge.EdgePropertyID != null)
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(myEdge.SourceVertex.TypeID, myEdge.SourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                return SourceVertex.GetOutgoingEdge((Int64)myEdge.EdgePropertyID).HasUnstructuredProperty(myPropertyName);
            }
            else
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.TypeID,
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                IHyperEdge HyperEdge = SourceVertex.GetOutgoingHyperEdge((Int64)myEdge.EdgePropertyID);
                return HyperEdge.GetAllEdges(delegate(ISingleEdge mySingleEdge){
                    return (mySingleEdge.GetSourceVertex().VertexID == myEdge.SourceVertex.VertexID && mySingleEdge.GetTargetVertex().VertexID == (myEdge as ServiceSingleEdgeInstance).TargetVertex.VertexID);
                }).First<ISingleEdge>().HasUnstructuredProperty(myPropertyName);
            }
        }

        public int GetCountOfUnstructuredProperties(SecurityToken mySecurityToken, Int64 myTransToken, ServiceEdgeInstance myEdge)
        {
            if(myEdge.EdgePropertyID != null)
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(myEdge.SourceVertex.TypeID, myEdge.SourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                return SourceVertex.GetOutgoingEdge((Int64)myEdge.EdgePropertyID).GetCountOfUnstructuredProperties();
            }
            else
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.TypeID,
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                IHyperEdge HyperEdge = SourceVertex.GetOutgoingHyperEdge((Int64)myEdge.EdgePropertyID);
                return HyperEdge.GetAllEdges(delegate(ISingleEdge mySingleEdge){
                    return (mySingleEdge.GetSourceVertex().VertexID == myEdge.SourceVertex.VertexID && mySingleEdge.GetTargetVertex().VertexID == (myEdge as ServiceSingleEdgeInstance).TargetVertex.VertexID);
                }).First<ISingleEdge>().GetCountOfUnstructuredProperties();
            }
        }

        public List<ServiceUnstructuredPropertyContainer> GetAllUnstructuredProperties(SecurityToken mySecurityToken, Int64 myTransToken, ServiceEdgeInstance myEdge)
        {
            if(myEdge.EdgePropertyID != null)
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(myEdge.SourceVertex.TypeID, myEdge.SourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                return SourceVertex.GetOutgoingEdge((Int64)myEdge.EdgePropertyID).GetAllUnstructuredProperties().Select(_ => new ServiceUnstructuredPropertyContainer { PropertyName = _.PropertyName, Property = _.Property }).ToList();
            }
            else
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.TypeID,
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                IHyperEdge HyperEdge = SourceVertex.GetOutgoingHyperEdge((Int64)myEdge.EdgePropertyID);
                return HyperEdge.GetAllEdges(delegate(ISingleEdge mySingleEdge){
                    return (mySingleEdge.GetSourceVertex().VertexID == myEdge.SourceVertex.VertexID && mySingleEdge.GetTargetVertex().VertexID == (myEdge as ServiceSingleEdgeInstance).TargetVertex.VertexID);
                }).First<ISingleEdge>().GetAllUnstructuredProperties().Select(_ => new ServiceUnstructuredPropertyContainer { PropertyName = _.PropertyName, Property = _.Property }).ToList();
            }
        }

        public string GetUnstructuredPropertyAsString(SecurityToken mySecurityToken, Int64 myTransToken, ServiceEdgeInstance myEdge, string myPropertyName)
        {
            if(myEdge.EdgePropertyID != null)
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(myEdge.SourceVertex.TypeID, myEdge.SourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                return SourceVertex.GetOutgoingEdge((Int64)myEdge.EdgePropertyID).GetUnstructuredPropertyAsString(myPropertyName);
            }
            else
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.TypeID,
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                IHyperEdge HyperEdge = SourceVertex.GetOutgoingHyperEdge((Int64)myEdge.EdgePropertyID);
                return HyperEdge.GetAllEdges(delegate(ISingleEdge mySingleEdge){
                    return (mySingleEdge.GetSourceVertex().VertexID == myEdge.SourceVertex.VertexID && mySingleEdge.GetTargetVertex().VertexID == (myEdge as ServiceSingleEdgeInstance).TargetVertex.VertexID);
                }).First<ISingleEdge>().GetUnstructuredPropertyAsString(myPropertyName);
            }
        }

        public string Comment(SecurityToken mySecurityToken, Int64 myTransToken, ServiceEdgeInstance myEdge)
        {
            if (myEdge.EdgePropertyID != null)
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(myEdge.SourceVertex.TypeID, myEdge.SourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                return SourceVertex.GetOutgoingEdge((Int64)myEdge.EdgePropertyID).Comment;
            }
            else
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.TypeID,
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                IHyperEdge HyperEdge = SourceVertex.GetOutgoingHyperEdge((Int64)myEdge.EdgePropertyID);
                return HyperEdge.GetAllEdges(delegate(ISingleEdge mySingleEdge)
                {
                    return (mySingleEdge.GetSourceVertex().VertexID == myEdge.SourceVertex.VertexID && mySingleEdge.GetTargetVertex().VertexID == (myEdge as ServiceSingleEdgeInstance).TargetVertex.VertexID);
                }).First<ISingleEdge>().Comment;
            }
        }

        public long CreationDate(SecurityToken mySecurityToken, Int64 myTransToken, ServiceEdgeInstance myEdge)
        {
            if (myEdge.EdgePropertyID != null)
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(myEdge.SourceVertex.TypeID, myEdge.SourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                return SourceVertex.GetOutgoingEdge((Int64)myEdge.EdgePropertyID).CreationDate;
            }
            else
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.TypeID,
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                IHyperEdge HyperEdge = SourceVertex.GetOutgoingHyperEdge((Int64)myEdge.EdgePropertyID);
                return HyperEdge.GetAllEdges(delegate(ISingleEdge mySingleEdge)
                {
                    return (mySingleEdge.GetSourceVertex().VertexID == myEdge.SourceVertex.VertexID && mySingleEdge.GetTargetVertex().VertexID == (myEdge as ServiceSingleEdgeInstance).TargetVertex.VertexID);
                }).First<ISingleEdge>().CreationDate;
            }
        }

        public long ModificationDate(SecurityToken mySecurityToken, Int64 myTransToken, ServiceEdgeInstance myEdge)
        {
            if (myEdge.EdgePropertyID != null)
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(myEdge.SourceVertex.TypeID, myEdge.SourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                return SourceVertex.GetOutgoingEdge((Int64)myEdge.EdgePropertyID).ModificationDate;
            }
            else
            {
                var Request = ServiceRequestFactory.MakeRequestGetVertex(
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.TypeID,
                    (myEdge as ServiceSingleEdgeInstance).HyperEdgeSourceVertex.VertexID);
                var SourceVertex = this.GraphDS.GetVertex<IVertex>(mySecurityToken, myTransToken, Request, ServiceReturnConverter.ConvertOnlyVertexInstance);
                IHyperEdge HyperEdge = SourceVertex.GetOutgoingHyperEdge((Int64)myEdge.EdgePropertyID);
                return HyperEdge.GetAllEdges(delegate(ISingleEdge mySingleEdge)
                {
                    return (mySingleEdge.GetSourceVertex().VertexID == myEdge.SourceVertex.VertexID && mySingleEdge.GetTargetVertex().VertexID == (myEdge as ServiceSingleEdgeInstance).TargetVertex.VertexID);
                }).First<ISingleEdge>().ModificationDate;
            }
        }

        #endregion
    }
}
