﻿using System;
using System.Collections.Generic;
using sones.Library.Internal.Definitions;
using sones.GraphInfrastructure.Element;

namespace sones.InMemoryNonRevisioned.Element
{
    /// <summary>
    /// A hyper edge is a 1-N relation within the property hypergraph
    /// </summary>
    public sealed class HyperEdge : IHyperEdge
    {

        #region IHyperEdge Members

        public IEnumerable<ISingleEdge> GetEdges(Func<ISingleEdge, bool> myFilterFunction = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEdge Members

        public IVertex GetSourceVertex()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IVertex> GetTargetVertices(Func<IVertex, bool> myFilterFunc = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IGraphElement Members

        public T GetProperty<T>(PropertyID myPropertyID)
        {
            throw new NotImplementedException();
        }

        public bool HasProperty(PropertyID myPropertyID)
        {
            throw new NotImplementedException();
        }

        public ulong GetCountOfProperties()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KeyValuePair<PropertyID, object>> GetAllProperties(Func<PropertyID, object, bool> myFilterFunc = null)
        {
            throw new NotImplementedException();
        }

        public string GetPropertyAsString(PropertyID myPropertyID)
        {
            throw new NotImplementedException();
        }

        public T GetUnstructuredProperty<T>(string myPropertyName)
        {
            throw new NotImplementedException();
        }

        public bool HasUnstructuredProperty(string myPropertyName)
        {
            throw new NotImplementedException();
        }

        public ulong GetCountOfUnstructuredProperties()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KeyValuePair<string, object>> GetAllUnstructuredProperties(Func<string, object, bool> myFilterFunc = null)
        {
            throw new NotImplementedException();
        }

        public string GetUnstructuredPropertyAsString(string myPropertyName)
        {
            throw new NotImplementedException();
        }

        public string Comment
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime CreationDate
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime ModificationDate
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IGraphElementStatistics Members

        public ulong Visits
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IEdgeProperties

        public EdgeID EdgeID
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
        
    }
}