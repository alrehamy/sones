using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.TypeSystem;

namespace sones.GraphDS.GraphDSRemoteClient.sonesGraphDSRemoteAPI
{
    public partial class ServiceEdgePredefinition
    {
        internal ServiceEdgePredefinition(EdgePredefinition myEdgePredefinition)
        {
            this.EdgeName = myEdgePredefinition.EdgeName;
            this.Comment = myEdgePredefinition.Comment;

            this.ContainedEdges = (myEdgePredefinition.ContainedEdges == null)
                ? null : myEdgePredefinition.ContainedEdges.Select(x => new ServiceEdgePredefinition(x)).ToArray();

            this.VertexIDsByID = (myEdgePredefinition.VertexIDsByVertexTypeID == null)
                ? null : myEdgePredefinition.VertexIDsByVertexTypeID.ToDictionary(_ => _.Key, _ =>_.Value.ToArray());

            this.StructuredProperties = (myEdgePredefinition.StructuredProperties == null)
                ? null : myEdgePredefinition.StructuredProperties.Select(x => new StructuredProperty(x.Key, x.Value)).ToArray();

            this.UnstructuredProperties = (myEdgePredefinition.UnstructuredProperties == null)
                ? null : myEdgePredefinition.UnstructuredProperties.Select(x => new UnstructuredProperty(x.Key, x.Value)).ToArray();
        }
    }
}
