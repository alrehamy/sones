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
            this.ToBeAddedProperties = (myRequestAlterVertexType.ToBeAddedProperties == null)
                ? null : myRequestAlterVertexType.ToBeAddedProperties.Select(x => new ServicePropertyPredefinition(x)).ToArray();
            this.ToBeAddedIncomingEdges = (myRequestAlterVertexType.ToBeAddedIncomingEdges == null)
                ? null : myRequestAlterVertexType.ToBeAddedIncomingEdges.Select(x => new ServiceIncomingEdgePredefinition(x)).ToArray();
            this.ToBeAddedOutgoingEdges = (myRequestAlterVertexType.ToBeAddedOutgoingEdges == null)
                ? null : myRequestAlterVertexType.ToBeAddedOutgoingEdges.Select(x => new ServiceOutgoingEdgePredefinition(x)).ToArray();
            this.ToBeAddedIndices = (myRequestAlterVertexType.ToBeAddedIndices == null)
                ? null : myRequestAlterVertexType.ToBeAddedIndices.Select(x => new ServiceIndexPredefinition(x)).ToArray();
            this.ToBeAddedUniques = (myRequestAlterVertexType.ToBeAddedUniques == null)
                ? null : myRequestAlterVertexType.ToBeAddedUniques.Select(x => new ServiceUniquePredefinition(x)).ToArray();
            this.ToBeAddedMandatories = (myRequestAlterVertexType.ToBeAddedMandatories == null)
                ? null : myRequestAlterVertexType.ToBeAddedMandatories.Select(x => new ServiceMandatoryPredefinition(x)).ToArray();
            this.ToBeRemovedProperties = (myRequestAlterVertexType.ToBeRemovedProperties == null)
                ? null : myRequestAlterVertexType.ToBeRemovedProperties.ToArray();
            this.ToBeRemovedIncomingEdges = (myRequestAlterVertexType.ToBeRemovedIncomingEdges == null)
                ? null : myRequestAlterVertexType.ToBeRemovedIncomingEdges.ToArray();
            this.ToBeRemovedOutgoingEdges = (myRequestAlterVertexType.ToBeRemovedOutgoingEdges == null)
                ? null : myRequestAlterVertexType.ToBeRemovedOutgoingEdges.ToArray();
            this.ToBeRemovedIndices = myRequestAlterVertexType.ToBeRemovedIndices;
            this.ToBeRemovedUniques = (myRequestAlterVertexType.ToBeRemovedUniques == null)
                ? null : myRequestAlterVertexType.ToBeRemovedUniques.ToArray();
            this.ToBeRemovedMandatories = (myRequestAlterVertexType.ToBeRemovedMandatories == null)
                ? null : myRequestAlterVertexType.ToBeRemovedMandatories.ToArray();
            this.ToBeRenamedProperties = myRequestAlterVertexType.ToBeRenamedProperties;
            this.ToBeDefinedAttributes = (myRequestAlterVertexType.ToBeDefinedAttributes == null)
                ? null : myRequestAlterVertexType.ToBeDefinedAttributes.Select(x => new ServiceUnknownAttributePredefinition(x)).ToArray();
            this.ToBeUndefinedAttributes = (myRequestAlterVertexType.ToBeUndefinedAttributes == null)
                ? null : myRequestAlterVertexType.ToBeUndefinedAttributes.ToArray();
        }
    }
}
