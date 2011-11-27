﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.Request;

namespace sones.GraphDS.GraphDSRemoteClient.sonesGraphDSRemoteAPI
{
    public partial class ServiceUpdateChangeset
    {
        internal ServiceUpdateChangeset(RequestUpdate myRequestUpdate)
        {
            this.VertexTypeName = myRequestUpdate.GetVerticesRequest.VertexTypeName;
            this.VertexTypeID = myRequestUpdate.GetVerticesRequest.VertexTypeID;
            this.VertexIDs = myRequestUpdate.GetVerticesRequest.VertexIDs.ToList();
            this.Expression = (myRequestUpdate.GetVerticesRequest.Expression == null) ? null : ConvertHelper.ToServiceExpression(myRequestUpdate.GetVerticesRequest.Expression);

            this.Comment = myRequestUpdate.UpdatedComment;
            this.Edition = myRequestUpdate.UpdatedEdition;

            if (myRequestUpdate.AddedElementsToCollectionProperties != null)
            {
                this.AddedElementsToCollectionProperties = new Dictionary<string, List<object>>();
                foreach (var item in myRequestUpdate.AddedElementsToCollectionProperties)
                    this.AddedElementsToCollectionProperties.Add(item.Key, item.Value.Select(x => (object)x).ToList());
            }

            if (myRequestUpdate.RemovedElementsFromCollectionProperties != null)
            {
                this.RemovedElementsFromCollectionProperties = new Dictionary<string, List<object>>();
                foreach (var item in myRequestUpdate.RemovedElementsFromCollectionProperties)
                    this.RemovedElementsFromCollectionProperties.Add(item.Key, item.Value.Select(x => (object)x).ToList());
            }

            if (myRequestUpdate.AddedElementsToCollectionEdges != null)
            {
                this.AddedElementsToCollectionEdges = new Dictionary<string, ServiceEdgePredefinition>();
                foreach (var item in myRequestUpdate.AddedElementsToCollectionEdges)
                    this.AddedElementsToCollectionEdges.Add(item.Key, new ServiceEdgePredefinition(item.Value));
            }

            if (myRequestUpdate.RemovedElementsFromCollectionEdges != null)
            {
                this.RemovedElementsFromCollectionEdges = new Dictionary<string, ServiceEdgePredefinition>();
                foreach (var item in myRequestUpdate.RemovedElementsFromCollectionEdges)
                    this.RemovedElementsFromCollectionEdges.Add(item.Key, new ServiceEdgePredefinition(item.Value));
            }
            

            this.UpdatedUnstructuredProperties = (myRequestUpdate.UpdatedUnstructuredProperties == null)
                ? null : myRequestUpdate.UpdatedUnstructuredProperties.ToDictionary(k => k.Key, v => v.Value);
            this.UpdatedStructuredProperties = (myRequestUpdate.UpdatedStructuredProperties == null)
                ? null : myRequestUpdate.UpdatedStructuredProperties.ToDictionary(k => k.Key, v => (object)v.Value);

            this.UpdatedOutgoingEdges = (myRequestUpdate.UpdateOutgoingEdges == null)
                ? null : myRequestUpdate.UpdateOutgoingEdges.Select(x => new ServiceEdgePredefinition(x)).ToList();
            this.UpdateOutgoingEdgesProperties = (myRequestUpdate.UpdateOutgoingEdgesProperties == null)
                ? null : myRequestUpdate.UpdateOutgoingEdgesProperties.Select(x => new ServiceSingleEdgeUpdateDefinition(x)).ToList();

            this.UpdatedUnknownProperties = (myRequestUpdate.UpdatedUnknownProperties == null)
                ? null : myRequestUpdate.UpdatedUnknownProperties.ToDictionary(k => k.Key, v => v.Value);
            this.RemovedAttributes = myRequestUpdate.RemovedAttributes.ToArray();
        }
    }
}