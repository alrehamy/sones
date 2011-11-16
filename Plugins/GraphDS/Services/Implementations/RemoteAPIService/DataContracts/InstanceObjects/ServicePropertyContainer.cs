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
    public struct ServicePropertyContainer
    {
        /// <summary>
        /// The property id
        /// </summary>
        [DataMember]
        public Int64 PropertyID;

        /// <summary>
        /// The property itself
        /// </summary>
        [DataMember]
        public Object Property;
    }
}
