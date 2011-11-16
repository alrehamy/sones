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
    public struct ServiceUnstructuredPropertyContainer
    {
        /// <summary>
        /// The property name
        /// </summary>
        [DataMember]
        public String PropertyName;

        /// <summary>
        /// The property itself
        /// </summary>
        [DataMember]
        public Object Property;
    }
}
