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
using sones.Library.PropertyHyperGraph;

namespace sones.GraphDS.Services.RemoteAPIService.ServiceConverter
{
    public static class ServiceReturnConverter
    {
        public static IVertexType ConvertOnlyVertexType(IRequestStatistics myRequestStatistics, IVertexType myCreatedVertexType)
        {
            return myCreatedVertexType;
        }

        public static IEdgeType ConvertOnlyEdgeType(IRequestStatistics myRequestStatistics, IEdgeType myCreatedEdgeType)
        {
            return myCreatedEdgeType;
        }

        public static IEnumerable<IVertexType> ConvertOnlyVertexTypes(IRequestStatistics myRequestStatistics, IEnumerable<IVertexType> myCreatedVertexTypes)
        {
            return myCreatedVertexTypes;
        }

        public static IEnumerable<IEdgeType> ConvertOnlyEdgeTypes(IRequestStatistics myRequestStatistics, IEnumerable<IEdgeType> myCreatedEdgeTypes)
        {
            return myCreatedEdgeTypes;
        }

        public static IEnumerable<Int64> ConvertOnlyVertexTypeIDs(IRequestStatistics myRequestStatistics, IEnumerable<long> myDeletetVertexTypes)
        {
            return myDeletetVertexTypes;
        }

        public static UInt64 ConvertOnlyCount(IRequestStatistics myRequestStatistics, UInt64 myVertexCount)
        {
            return myVertexCount;
        }

        public static IVertex ConvertOnlyVertexInstance(IRequestStatistics myRequestStatistics, IVertex myVertex)
        {
            return myVertex;
        }

        public static IEnumerable<IVertex> ConvertOnlyVertices(IRequestStatistics myRequestStatistics, IEnumerable<IVertex> myVertices)
        {
            return myVertices;
        }

        public static IIndexDefinition ConverteOnlyIndexDefinition(IRequestStatistics myRequestStatistics, IIndexDefinition myIndexDefinition)
        {
            return myIndexDefinition;
        }

        public static List<Int64> ConverterOnlyRelevantList(IRequestStatistics myRequestStatistics, IEnumerable<IComparable> myDeletedAttributes, IEnumerable<IComparable> myDeletedVertices)
        {
            if (myDeletedAttributes != null && myDeletedAttributes.Count() != 0)
            {
                return myDeletedAttributes.Select(x => (Int64)x).ToList();
            }
            if (myDeletedVertices != null && myDeletedVertices.Count() != 0)
            {
                return myDeletedVertices.Select(x => (Int64)x).ToList();
            }
            return new List<Int64>();
        }

        public static KeyValuePair<IEnumerable<IComparable>, IEnumerable<IComparable>> ConverteAllLists(IRequestStatistics myRequestStatistics, IEnumerable<IComparable> myDeletedAttributes, IEnumerable<IComparable> myDeletedVertices)
        {
            return new KeyValuePair<IEnumerable<IComparable>, IEnumerable<IComparable>>(myDeletedAttributes, myDeletedVertices);
        }

        public static IEnumerable<IIndexDefinition> ConverteOnlyIndexDefinitions(IRequestStatistics myRequestStatistics, IEnumerable<IIndexDefinition> myIndexDefinitons)
        {
            return myIndexDefinitons;
        }

        public static object ConverteToVoid(IRequestStatistics myRequestStatistics)
        {
            return null;
        }

        public static Dictionary<Int64, String> ConverteOnlyDeletedTypeIDs(IRequestStatistics myRequestStatistics, Dictionary<Int64, String> myDeletedTypeIDs)
        {
            return myDeletedTypeIDs;
        }

        
    

    }
}
