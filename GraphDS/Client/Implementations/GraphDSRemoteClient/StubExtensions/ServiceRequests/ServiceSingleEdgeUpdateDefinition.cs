﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.Library.Commons.VertexStore.Definitions.Update;
using sones.Library.Commons.VertexStore.Definitions;

namespace sones.GraphDS.GraphDSRemoteClient.sonesGraphDSRemoteAPI
{
    public partial class ServiceSingleEdgeUpdateDefinition
    {
        internal ServiceSingleEdgeUpdateDefinition(SingleEdgeUpdateDefinition myUpdateDefinition)
        {
            this.CommentUpdate = myUpdateDefinition.CommentUpdate;
            this.EdgeTypeID = myUpdateDefinition.EdgeTypeID;

            this.UpdatedStructuredProperties = (myUpdateDefinition.UpdatedStructuredProperties.Updated == null)
                ? null : myUpdateDefinition.UpdatedStructuredProperties.Updated.ToDictionary(k => k.Key, v => (object)v.Value);

            this.DeletedStructuredProperties = (myUpdateDefinition.UpdatedStructuredProperties.Deleted == null)
                ? null : myUpdateDefinition.UpdatedStructuredProperties.Deleted.ToArray();

            this.UpdatedUnstructuredProperties = (myUpdateDefinition.UpdatedUnstructuredProperties.Updated == null)
                ? null : myUpdateDefinition.UpdatedUnstructuredProperties.Updated.ToDictionary(k => k.Key, v => (object)v.Value);

            this.DeletedUnstructuredProperties = (myUpdateDefinition.UpdatedUnstructuredProperties.Deleted == null)
                ? null : myUpdateDefinition.UpdatedUnstructuredProperties.Deleted.ToArray();

            this.SourceVertex = new ServiceVertexInformation(myUpdateDefinition.SourceVertex);
            this.TargetVertex = new ServiceVertexInformation(myUpdateDefinition.TargetVertex);
        }

        public partial class ServiceVertexInformation
        {
            internal ServiceVertexInformation(VertexInformation myVertexInformation)
            {
                VertexTypeIDField = myVertexInformation.VertexTypeID;
                VertexIDField = myVertexInformation.VertexID;
                VertexRevisionIDField = myVertexInformation.VertexRevisionID;
                VertexEditionNameField = myVertexInformation.VertexEditionName ?? String.Empty;
            }
        }
    }
}
