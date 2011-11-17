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
using System.Collections;

namespace sones.GraphQL.Result
{
    public class QueryResultComparer : IEqualityComparer<IQueryResult>, IEqualityComparer<IVertexView>, IEqualityComparer<IEdgeView>
    {   

        #region IEqualityComparer<QueryResult> Members

        public bool Equals(IQueryResult x, IQueryResult y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            if (x.Vertices.Count() != y.Vertices.Count())
            {
                return false;
            }

            if (x.Error != y.Error)
            {
                return false;
            }

            if (x.NumberOfAffectedVertices != y.NumberOfAffectedVertices)
            {
                return false;
            }

            if (x.Query != y.Query)
            {
                return false;
            }

            if (x.TypeOfResult != y.TypeOfResult)
            {
                return false;
            }

            if (x.NameOfQuerylanguage != y.NameOfQuerylanguage)
            {
                return false;
            }


            
            

            return true;
            
        }

        

        public int GetHashCode(IQueryResult obj)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEqualityComparer<IVertexView> Members

        public bool Equals(IVertexView x, IVertexView y)
        {
            if (x.GetCountOfProperties() != y.GetCountOfProperties())
            {
                return false;
            }

            var xProperties = x.GetAllProperties().ToList();
            var yProperties = y.GetAllProperties().ToList();

            if (xProperties.Count != xProperties.Count)
            {
                return false;
            }
            
            
            for (int i = 0; i <= xProperties.Count; i++)
            {
                if (xProperties[i].PropertyName != yProperties[i].PropertyName)
                {
                    return false;
                }

                if (!xProperties[i].Property.Equals(yProperties[i].Property))
                {
                    return false;
                }
            }

            var xBinaryProperties = x.GetAllBinaryProperties().ToList();
            var yBinaryProperties = y.GetAllBinaryProperties().ToList();

            if (xBinaryProperties.Count != yBinaryProperties.Count)
            {
                return false;
            }

            for (int i = 0; i <= xBinaryProperties.Count; i++)
            {
                if (xBinaryProperties[i].PropertyName != yBinaryProperties[i].PropertyName)
                {
                    return false;
                }

                if (!xBinaryProperties[i].BinaryPropery.Equals(yBinaryProperties[i].BinaryPropery))
                {
                    return false;
                }
            }

            var xEdges = x.GetAllEdges().ToList();
            var yEdges = y.GetAllEdges().ToList();

            if (xEdges.Count != yEdges.Count)
            {
                return false;
            }

            for (int i = 0; i <= xEdges.Count; i++)
            {
                if (xEdges[i].EdgeName != xEdges[i].EdgeName)
                {
                    return false;
                }


                if (!this.Equals(xEdges[i].Edge, yEdges[i].Edge))
                {
                    return false;
                }

            }
            return true;
        }

        public int GetHashCode(IVertexView obj)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEqualityComparer<IEdgeView> Members

        public bool Equals(IEdgeView x, IEdgeView y)
        {
            if (x.GetCountOfProperties() != y.GetCountOfProperties())
            {
                return false;
            }

            var xProperties = x.GetAllProperties().ToList();
            var yProperties = y.GetAllProperties().ToList();

            if (xProperties.Count != yProperties.Count)
            {
                return false;
            }
            
            for (int i = 0; i <= xProperties.Count; i++)
            {
                if (xProperties[i].PropertyName != xProperties[i].PropertyName)
                {
                    return false;
                }

                if (!xProperties[i].Property.Equals(xProperties[i].Property))
                {
                    return false;
                }
                              
            }

            var xTargetVertices = x.GetTargetVertices().ToList();
            var yTargetVertices = y.GetTargetVertices().ToList();

            if (xTargetVertices.Count != xTargetVertices.Count)
            {
                return false;
            }


            for (int i = 0; i <= xTargetVertices.Count; i++)
            {
                if (!this.Equals(xTargetVertices[i], xTargetVertices[i]))
                {
                    return false;
                }
            }


            return true;
        }

        public int GetHashCode(IEdgeView obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}