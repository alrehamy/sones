using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphQL.Result;

namespace sones.GraphDS.GraphDSRemoteClient.sonesGraphDSRemoteAPI
{
    public partial class ServiceHyperEdgeView
    {
        internal IHyperEdgeView ToHyperEdgeView(IServiceToken myServiceToken)
        {
            return new HyperEdgeView(this.PropertyList, this.Edges.Select(x => x.ToSingleEdgeView(myServiceToken)));
        }
    }
}
