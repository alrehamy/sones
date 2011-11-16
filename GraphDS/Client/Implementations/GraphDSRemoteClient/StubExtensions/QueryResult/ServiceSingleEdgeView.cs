using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphQL.Result;

namespace sones.GraphDS.GraphDSRemoteClient.sonesGraphDSRemoteAPI
{
    public partial class ServiceSingleEdgeView
    {
        internal ISingleEdgeView ToSingleEdgeView(IServiceToken myServiceToken)
        {
            Dictionary<String, Object> properties = PropertyList;
            return new SingleEdgeView(properties, this.TargetVertex.ToVertexView(myServiceToken));
        }
    }
}
