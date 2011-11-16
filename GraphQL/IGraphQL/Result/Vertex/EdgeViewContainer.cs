/*
* 
* Copyright (C) 2011 Henning 'cosh' Rauch
*
*/

using System;
using System.Collections.Generic;

namespace sones.GraphQL.Result
{
    /// <summary>
    /// A struct that contains an edge view
    /// </summary>
    public struct EdgeViewContainer
    {
        /// <summary>
        /// The property id of the edge property
        /// </summary>
        public String EdgeName;

        /// <summary>
        /// The edge view itself
        /// </summary>
        public IEdgeView Edge;
    }
}
