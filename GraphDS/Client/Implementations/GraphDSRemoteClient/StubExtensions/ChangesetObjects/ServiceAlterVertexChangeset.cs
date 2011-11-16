using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.Request;

namespace sones.GraphDS.GraphDSRemoteClient.sonesGraphDSRemoteAPI
{
    public partial class ServiceAlterVertexChangeset
    {
        internal ServiceAlterVertexChangeset(RequestAlterVertexType myRequestAlterVertexType)
        {
            this.NewTypeName = myRequestAlterVertexType.AlteredTypeName;
            this.Comment = myRequestAlterVertexType.AlteredComment;
            this.ToBeAddedProperties = myRequestAlterVertexType.ToBeAddedProperties.Select(x => new ServicePropertyPredefinition(x)).ToArray();
            this.ToBeAddedIncomingEdges = myRequestAlterVertexType.ToBeAddedIncomingEdges.Select(x => new ServiceIncomingEdgePredefinition(x)).ToArray();
            this.ToBeAddedOutgoingEdges = myRequestAlterVertexType.ToBeAddedOutgoingEdges.Select(x => new ServiceOutgoingEdgePredefinition(x)).ToArray();
            this.ToBeAddedIndices = myRequestAlterVertexType.ToBeAddedIndices.Select(x => new ServiceIndexPredefinition(x)).ToArray();
            this.ToBeAddedUniques = myRequestAlterVertexType.ToBeAddedUniques.Select(x => new ServiceUniquePredefinition(x)).ToArray();
            this.ToBeAddedMandatories = myRequestAlterVertexType.ToBeAddedMandatories.Select(x => new ServiceMandatoryPredefinition(x)).ToArray();
            this.ToBeRemovedProperties = myRequestAlterVertexType.ToBeRemovedProperties.ToArray();
            this.ToBeRemovedIncomingEdges = myRequestAlterVertexType.ToBeRemovedIncomingEdges.ToArray();
            this.ToBeRemovedOutgoingEdges = myRequestAlterVertexType.ToBeRemovedOutgoingEdges.ToArray();
            this.ToBeRemovedIndices = myRequestAlterVertexType.ToBeRemovedIndices;
            this.ToBeRemovedUniques = myRequestAlterVertexType.ToBeRemovedUniques.ToArray();
            this.ToBeRemovedMandatories = myRequestAlterVertexType.ToBeRemovedMandatories.ToArray();
            this.ToBeRenamedProperties = myRequestAlterVertexType.ToBeRenamedProperties;
        }
    }
}
