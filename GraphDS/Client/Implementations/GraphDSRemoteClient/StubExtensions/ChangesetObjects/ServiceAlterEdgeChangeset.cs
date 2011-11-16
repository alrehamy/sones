using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.Request;

namespace sones.GraphDS.GraphDSRemoteClient.sonesGraphDSRemoteAPI
{
    public partial class ServiceAlterEdgeChangeset
    {
        internal ServiceAlterEdgeChangeset(RequestAlterEdgeType myRequestAlterEdgeType)
        {
            this.NewTypeName = myRequestAlterEdgeType.AlteredTypeName;
            this.Comment = myRequestAlterEdgeType.AlteredComment;
            this.ToBeAddedProperties = myRequestAlterEdgeType.ToBeAddedProperties.Select(x => new ServicePropertyPredefinition(x)).ToArray();
            this.ToBeRemovedProperties = myRequestAlterEdgeType.ToBeRemovedProperties.ToArray();
            this.ToBeRenamedProperties = myRequestAlterEdgeType.ToBeRenamedProperties;
        }
    }
}
