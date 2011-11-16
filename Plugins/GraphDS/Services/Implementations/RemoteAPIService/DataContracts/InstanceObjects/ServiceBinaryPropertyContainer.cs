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
using System.IO;

namespace sones.GraphDS.Services.RemoteAPIService.DataContracts.InstanceObjects
{
    [DataContract(Namespace = sonesRPCServer.Namespace)]
    public struct ServiceBinaryPropertyContainer
    {
        /// <summary>
        /// The id of the binary property id
        /// </summary>
        [DataMember]
        public Int64 PropertyID;

        /// <summary>
        /// The binary property itself
        /// </summary>
        [DataMember]
        public Stream BinaryPropery;
    }
}
