﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.TypeSystem;

namespace sones.GraphDS.GraphDSRemoteClient.sonesGraphDSRemoteAPI
{
    public partial class ServiceVertexType
    {
        internal ServiceVertexType(String myName) : base(myName)
        {
        }

        internal ServiceVertexType(Int64 myID)
        {
            this.ID = myID;
        }

        internal ServiceVertexType(IVertexType myVertexType) : base(myVertexType.Name)
        {
        }
    }
}
