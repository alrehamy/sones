/*
* 
* Copyright (C) 2011 Henning 'cosh' Rauch
*
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using sones.Library.PropertyHyperGraph;

namespace sones.GraphDS.Services.RemoteAPIService.DataContracts.InstanceObjects
{
    [DataContract(Namespace = sonesRPCServer.Namespace)]
    public struct ServiceIncomingVerticesContainer
    {
        /// <summary>
        /// The vertex type id of the vertices that points to this vertex
        /// </summary>
        [DataMember]
        public Int64 VertexTypeID;

        /// <summary>
        /// The edge property id that points to this vertex
        /// </summary>
        [DataMember]
        public Int64 EdgePropertyID;

        /// <summary>
        /// The incoming vertices
        /// </summary>
        [DataMember]
        public List<ServiceVertexInstance> IncomingVertices;
    }
}
