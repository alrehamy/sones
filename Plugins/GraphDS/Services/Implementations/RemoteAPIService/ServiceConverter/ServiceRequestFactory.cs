﻿/*
* sones GraphDB - Community Edition - http://www.sones.com
* Copyright (C) 2007-2011 sones GmbH
*
* This file is part of sones GraphDB Community Edition.
*
* sones GraphDB is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published by
* the Free Software Foundation, version 3 of the License.
* 
* sones GraphDB is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with sones GraphDB. If not, see <http://www.gnu.org/licenses/>.
* 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.TypeSystem;
using sones.GraphDB.Request;
using sones.GraphDS.Services.RemoteAPIService.DataContracts.ServiceTypeManagement;
using sones.GraphDS.Services.RemoteAPIService.DataContracts.ServiceRequests;
using sones.GraphDS.Services.RemoteAPIService.DataContracts.ChangesetObjects;
using sones.GraphDS.Services.RemoteAPIService.DataContracts.InsertPayload;
using sones.GraphDS.Services.RemoteAPIService.DataContracts.PayloadObjects;
using sones.GraphDS.Services.RemoteAPIService.DataContracts.InstanceObjects;
using sones.GraphDB.Expression;
using sones.GraphDS.Services.RemoteAPIService.DataContracts.ServiceRequests.Expression;
using sones.GraphDB;
using System.IO;

namespace sones.GraphDS.Services.RemoteAPIService.ServiceConverter
{
    public static class ServiceRequestFactory
    {
        public static RequestAlterVertexType MakeRequestAlterVertexType(ServiceVertexType myVertexType, ServiceAlterVertexChangeset myChangeset)
        {
            var Request = new RequestAlterVertexType(myVertexType.Name);

            #region Add Attributes

            if (myChangeset.ToBeAddedProperties != null)
            {
                foreach (var toAdd in myChangeset.ToBeAddedProperties)
                {
                    Request.AddProperty(toAdd.ToPropertyPredefinition());
                }
            }

            if (myChangeset.ToBeAddedIncomingEdges != null)
            {
                foreach (var toAdd in myChangeset.ToBeAddedIncomingEdges)
                {
                    Request.AddIncomingEdge(toAdd.ToIncomingEdgePredefinition());
                }
            }

            if (myChangeset.ToBeAddedOutgoingEdges != null)
            {
                foreach (var toAdd in myChangeset.ToBeAddedOutgoingEdges)
                {
                    Request.AddOutgoingEdge(toAdd.ToOutgoingEdgePredefinition());
                }
            }

            if (myChangeset.ToBeAddedUniques != null)
            {
                foreach (var toAdd in myChangeset.ToBeAddedUniques)
                {
                    Request.AddUnique(toAdd.ToUniquePredefinition());
                }
            }

            if (myChangeset.ToBeAddedMandatories != null)
            {
                foreach (var toAdd in myChangeset.ToBeAddedMandatories)
                {
                    Request.AddMandatory(toAdd.ToMandatoryPredefinition());
                }
            }

            if (myChangeset.ToBeAddedIndices != null)
            {
                foreach (var toAdd in myChangeset.ToBeAddedIndices)
                {
                    Request.AddIndex(toAdd.ToIndexPredefinition());
                }
            }


            #endregion

            #region Remove Attributes

            if (myChangeset.ToBeRemovedProperties != null)
            {
                foreach (var toDel in myChangeset.ToBeRemovedProperties)
                {
                    Request.RemoveProperty(toDel);
                }
            }

            if (myChangeset.ToBeRemovedIncomingEdges != null)
            {
                foreach (var toDel in myChangeset.ToBeRemovedIncomingEdges)
                {
                    Request.RemoveIncomingEdge(toDel);
                }
            }

            if (myChangeset.ToBeRemovedOutgoingEdges != null)
            {
                foreach (var toDel in myChangeset.ToBeRemovedOutgoingEdges)
                {
                    Request.RemoveOutgoingEdge(toDel);
                }
            }

            if (myChangeset.ToBeRemovedUniques != null)
            {
                foreach (var toDel in myChangeset.ToBeRemovedUniques)
                {
                    Request.RemoveUnique(toDel);
                }
            }

            if (myChangeset.ToBeRemovedMandatories != null)
            {
                foreach (var toDel in myChangeset.ToBeRemovedMandatories)
                {
                    Request.RemoveMandatory(toDel);
                }
            }

            if (myChangeset.ToBeRemovedIndices != null)
            {
                foreach (var toDel in myChangeset.ToBeRemovedIndices)
                {
                    Request.RemoveIndex(toDel.Key, toDel.Value);
                }
            }

            #endregion

            #region Rename Task

            if (myChangeset.ToBeRenamedProperties != null)
            {
                myChangeset.ToBeRenamedProperties.Select((tuple, key) => Request.RenameAttribute(tuple.Key, tuple.Value));
            }
            
            #endregion


            if (myChangeset.Comment != null)
                Request.SetComment(myChangeset.Comment);

            if (myChangeset.NewTypeName != null)
                Request.RenameType(myChangeset.Comment);
            
            //todo add unknown attribute

            return Request;
        }

        public static RequestAlterEdgeType MakeRequestAlterEdgeType(ServiceEdgeType myEdgeType, ServiceAlterEdgeChangeset myChangeset)
        {
            var Request = new RequestAlterEdgeType(myEdgeType.Name);

            #region Add Attributes

            if (myChangeset.ToBeAddedProperties != null)
            {
                foreach (var toAdd in myChangeset.ToBeAddedProperties)
                {
                    Request.AddProperty(toAdd.ToPropertyPredefinition());
                }
            }
            
            #endregion

            #region Remove Attributes

            if (myChangeset.ToBeRemovedProperties != null)
            {
                foreach (var toDel in myChangeset.ToBeRemovedProperties)
                {
                    Request.RemoveProperty(toDel);
                }
            }

            #endregion

            #region Rename Task

            if (myChangeset.ToBeRenamedProperties != null)
            {
                foreach(var item in myChangeset.ToBeRenamedProperties)
                {
                    Request.RenameAttribute(item.Key, item.Value);
                }
            }

            #endregion

            
            if (myChangeset.Comment != null)
                Request.SetComment(myChangeset.Comment);

            if (myChangeset.NewTypeName != null)
                Request.RenameType(myChangeset.NewTypeName);
                        
            //todo add unknown attribute

            return Request;
        }

        public static RequestCreateVertexType MakeRequestCreateVertexType(ServiceVertexTypePredefinition myVertexTypePredefinition)
        {
            return new RequestCreateVertexType(myVertexTypePredefinition.ToVertexTypePredefinition());
        }

        public static RequestCreateVertexTypes MakeRequestCreateVertexTypes(IEnumerable<ServiceVertexTypePredefinition> myVertexTypePredefinition)
        {
            return new RequestCreateVertexTypes(myVertexTypePredefinition.Select(x => x.ToVertexTypePredefinition()));
        }

        public static RequestCreateEdgeType MakeRequestCreateEdgeType(ServiceEdgeTypePredefinition myEdgeTypePredefinition)
        {
            return new RequestCreateEdgeType(myEdgeTypePredefinition.ToEdgeTypePredefinition());
        }

        public static RequestCreateEdgeTypes MakeRequestCreateEdgeTypes(IEnumerable<ServiceEdgeTypePredefinition> myEdgeTypePredefinition)
        {
            return new RequestCreateEdgeTypes(myEdgeTypePredefinition.Select(x => (ATypePredefinition)x.ToEdgeTypePredefinition()));
        }
        
        public static RequestCreateIndex MakeRequestCreateIndex(ServiceIndexPredefinition myIndexPreDef)
        {
            return new RequestCreateIndex(myIndexPreDef.ToIndexPredefinition());
        }

        public static RequestDelete MakeRequestDelete(ServiceVertexType myVertexType, IEnumerable<Int64> myVertexIDs = null, ServiceDeletePayload myDeletePayload = null)
        {
            RequestGetVertices PreRequest = null;
            if (myVertexIDs != null)
            {
                PreRequest = new RequestGetVertices(myVertexType.Name, myVertexIDs);
            }
            else
            {
                PreRequest = new RequestGetVertices(myVertexType.Name);
            }

            RequestDelete Request = new RequestDelete(PreRequest);
            if (myDeletePayload != null)
            {
                foreach (var toDel in myDeletePayload.ToBeDeletedAttributes)
                {
                    Request.AddAttribute(toDel);

                }
            }
            return Request;
        }

        public static RequestDescribeIndex MakeRequestDescribeIndex(String myVertexTypeName, String myIndexName)
        {
            if (String.IsNullOrEmpty(myVertexTypeName))
                return new RequestDescribeIndex(myVertexTypeName, myIndexName);
            return null;
        }

        public static RequestDropEdgeType MakeRequestDropEdgeType(ServiceEdgeType myEdgeType)
        {
            RequestDropEdgeType Request = new RequestDropEdgeType(myEdgeType.Name);
            return Request;
        }

        public static RequestDropIndex MakeRequestDropIndex(ServiceVertexType myVertexType, String myIndexName,String myEdition)
        {
            RequestDropIndex Request = new RequestDropIndex(myVertexType.Name, myIndexName, myEdition);
            return Request;
        }

        public static RequestDropVertexType MakeRequestDropVertexType(ServiceVertexType myVertexType)
        {
            RequestDropVertexType Request = new RequestDropVertexType(myVertexType.Name);
            return Request;
        }

        public static RequestGetAllEdgeTypes MakeRequestGetAllEdgeTypes(String myEdition = null)
        {
            RequestGetAllEdgeTypes Request = new RequestGetAllEdgeTypes(myEdition);
            return Request;
        }

        public static RequestGetAllVertexTypes MakeRequestGetAllVertexTypes(String myEdition = null)
        {
            RequestGetAllVertexTypes Request = new RequestGetAllVertexTypes(myEdition);
            return Request;
        }

        public static RequestGetEdgeType MakeRequestGetEdgeType(Int64 myEdgeTypeID, String myEdition = null)
        {
            RequestGetEdgeType Request = new RequestGetEdgeType(myEdgeTypeID,myEdition);
            return Request;
        }

        public static RequestGetEdgeType MakeRequestGetEdgeType(String myEdgeTypeName, String myEdition = null)
        {
            RequestGetEdgeType Request = new RequestGetEdgeType(myEdgeTypeName, myEdition);
            return Request;
        }

        public static RequestGetVertex MakeRequestGetVertex(ServiceVertexType myVertexType, Int64 myVertexID, String myEdition = null)
        {
            return new RequestGetVertex(myVertexType.Name, myVertexID, myEdition);
        }

        public static RequestGetVertex MakeRequestGetVertex(Int64 myVertexTypeID, Int64 myVertexID, String myEdition = null)
        {
            return new RequestGetVertex(myVertexTypeID, myVertexID, myEdition);
        }

        public static RequestGetVertexCount MakeRequestGetVertexCount(ServiceVertexType myVertexTypeName)
        {
            return new RequestGetVertexCount(myVertexTypeName.Name);
        }
        
        public static RequestGetVertexType MakeRequestGetVertexType(String myVertexTypeName)
        {
            RequestGetVertexType Request = new RequestGetVertexType(myVertexTypeName);
            return Request;
        }

        public static RequestGetVertexType MakeRequestGetVertexType(Int64 myVertexTypeID)
        {
            RequestGetVertexType Request = new RequestGetVertexType(myVertexTypeID);
            return Request;
        }

        public static RequestGetVertices MakeRequestGetVertices(String myVertexTypeName, IEnumerable<Int64> myVertexIDs = null)
        {
            if (myVertexIDs == null)
            {
                return new RequestGetVertices(myVertexTypeName);
            }
            else
            {
                return new RequestGetVertices(myVertexTypeName, myVertexIDs);
            }
        }

        public static RequestGetVertices MakeRequestGetVertices(Int64 myVertexTypeID, IEnumerable<Int64> myVertexIDs = null)
        {
            if (myVertexIDs == null)
            {
                return new RequestGetVertices(myVertexTypeID);
            }
            else
            {
                return new RequestGetVertices(myVertexTypeID, myVertexIDs);
            }
        }

        public static RequestGetVertices MakeRequestGetVertices(ServiceBaseExpression myExpression)
        {
            return new RequestGetVertices(ServiceExpressionConverter.ConvertExpression(myExpression));
        }

        public static RequestUpdate MakeRequestUpdateBinary(Int64 myVertexTypeID, Int64 myVertexID, String myPropertyName, Stream myStream)
        {
            var idList = new List<Int64>();
            idList.Add(myVertexID);
            var Request = new RequestUpdate(new RequestGetVertices(myVertexTypeID, idList));
            Request.UpdateBinaryProperty(myPropertyName, myStream);
            return Request;
        }

        public static RequestUpdate MakeRequestUpdate(ServiceUpdateChangeset myUpdateChangeset)
        {
            #region PreRequest

            RequestGetVertices PreRequest = null;
            if (myUpdateChangeset.Expression == null)
            {
                if (myUpdateChangeset.VertexTypeName != null)
                {
                    PreRequest = MakeRequestGetVertices(myUpdateChangeset.VertexTypeName, myUpdateChangeset.VertexIDs);
                }
                else
                {
                    PreRequest = MakeRequestGetVertices(myUpdateChangeset.VertexTypeID, myUpdateChangeset.VertexIDs);
                }
            }
            else
            {
                PreRequest = MakeRequestGetVertices(myUpdateChangeset.Expression);
            }

            RequestUpdate Request = new RequestUpdate(PreRequest);

            if (!String.IsNullOrEmpty(myUpdateChangeset.Comment))
                Request.UpdateComment(myUpdateChangeset.Comment);

            if (!String.IsNullOrEmpty(myUpdateChangeset.Edition))
                Request.UpdateEdition(myUpdateChangeset.Edition);

            #endregion

            
            #region element collection

            if (myUpdateChangeset.AddedElementsToCollectionProperties != null)
            {
                foreach (var element in myUpdateChangeset.AddedElementsToCollectionProperties)
                {
                    Request.AddElementsToCollection(element.Key, element.Value);
                }
            }

            if (myUpdateChangeset.RemovedElementsFromCollectionProperties != null)
            {
                foreach (var element in myUpdateChangeset.RemovedElementsFromCollectionProperties)
                {
                    Request.RemoveElementsFromCollection(element.Key, element.Value);
                }
            }

            if (myUpdateChangeset.AddedElementsToCollectionEdges != null)
            {
                foreach (var element in myUpdateChangeset.AddedElementsToCollectionEdges)
                {
                    Request.AddElementsToCollection(element.Key, element.Value.ToEdgePredefinition());
                }
            }

            if (myUpdateChangeset.RemovedElementsFromCollectionEdges != null)
            {
                foreach (var element in myUpdateChangeset.RemovedElementsFromCollectionEdges)
                {
                    Request.RemoveElementsFromCollection(element.Key, element.Value.ToEdgePredefinition());
                }
            }

            #endregion


            #region Properties

            if (myUpdateChangeset.UpdatedStructuredProperties != null)
            {
                foreach (var item in myUpdateChangeset.UpdatedStructuredProperties)
                {
                    Request.UpdateStructuredProperty(item.Key, item.Value);
                }
            }

            if (myUpdateChangeset.UpdatedUnstructuredProperties != null)
            { 
                foreach (var item in myUpdateChangeset.UpdatedUnstructuredProperties)
                {
                    Request.UpdateUnstructuredProperty(item.Key, item.Value);
                }
            }

            if (myUpdateChangeset.UpdatedUnknownProperties != null)
            {
                foreach (var item in myUpdateChangeset.UpdatedUnknownProperties)
                {
                    Request.UpdateUnknownProperty(item.Key, item.Value);
                }
            }

            #endregion


            #region Update Edges

            if (myUpdateChangeset.UpdatedOutgoingEdges != null)
            {
                foreach (var Edge in myUpdateChangeset.UpdatedOutgoingEdges)
                {
                    Request.UpdateEdge(Edge.ToEdgePredefinition());
                }
            }

            if (myUpdateChangeset.UpdateOutgoingEdgesProperties != null)
            {
                foreach (var Edge in myUpdateChangeset.UpdateOutgoingEdgesProperties)
                {
                    Request.UpdateEdge(Edge.ToSingleEdgeUpdateDefinition());
                }
            }
            
            #endregion          

            if (myUpdateChangeset.RemovedAttributes != null)
            {
                foreach (var item in myUpdateChangeset.RemovedAttributes)
                {
                    Request.RemoveAttribute(item);
                }
            }

            return Request;
        }

        public static RequestInsertVertex MakeRequestInsertVertex(String myVertexTypeName, ServiceInsertPayload myPayload)
        {
            RequestInsertVertex Request = new RequestInsertVertex(myVertexTypeName);

            if(!String.IsNullOrEmpty(myPayload.Comment))
                Request.SetComment(myPayload.Comment);

            if(!String.IsNullOrEmpty(myPayload.Edition))
                Request.SetEdition(myPayload.Edition);

            if (myPayload.UUID != null)
                Request.SetUUID(myPayload.UUID.Value);

            if (myPayload.StructuredProperties != null)
            {
                foreach (var toInsert in myPayload.StructuredProperties)
                {
                    Request.AddStructuredProperty(toInsert.PropertyName, toInsert.PropertyValue as IComparable);
                }
            }

            if (myPayload.UnstructuredProperties != null)
            {
                foreach (var toInsert in myPayload.UnstructuredProperties)
                {
                    Request.AddUnstructuredProperty(toInsert.PropertyName, toInsert.PropertyValue);
                }
            }

            if (myPayload.Edges != null)
            {
                foreach (var Edge in myPayload.Edges)
                {
                    Request.AddEdge(Edge.ToEdgePredefinition());
                }
            }
            
            
            return Request;
        }

        public static RequestRebuildIndices MakeRequestRebuildIndices(IEnumerable<string> myTypeNames)
        {
            RequestRebuildIndices Request = new RequestRebuildIndices(myTypeNames);
            return Request;
        }

        public static RequestStatistics MakeRequestStatistics(Int64 myMilliseconds)
        {
            RequestStatistics Request = new RequestStatistics(TimeSpan.FromMilliseconds(myMilliseconds));
            return Request;
        }

        public static RequestTraverseVertex MakeRequestTraverseVertex()
        {
            return null; //This Request is currently unused, because of the fact there is no way to deliver the logic for graph traversals;
        }

        public static RequestTruncate MakeRequestTruncate(String myVertexTypeName)
        {
            RequestTruncate Request = new RequestTruncate(myVertexTypeName);
            return Request;
        }

        public static RequestClear MakeRequestClear()
        {
           return new RequestClear();
        }

        
    }
}
