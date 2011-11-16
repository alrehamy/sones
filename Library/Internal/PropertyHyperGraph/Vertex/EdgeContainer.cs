/*
* 
* Copyright (C) 2011 Henning 'cosh' Rauch
*
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sones.Library.PropertyHyperGraph
{
    /// <summary>
    /// A struct that contains a edge
    /// </summary>
    public struct EdgeContainer
    {
        /// <summary>
        /// The property id of the edge property
        /// </summary>
        public Int64 PropertyID;

        /// <summary>
        /// The edge itself
        /// </summary>
        public IEdge Edge;
    }
}
