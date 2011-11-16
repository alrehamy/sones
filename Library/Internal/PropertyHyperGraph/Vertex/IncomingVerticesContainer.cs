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
    /// A struct that contains the incoming edge collection
    /// </summary>
    public struct IncomingVerticesContainer
    {
        /// <summary>
        /// The vertex type id of the vertices that points to this vertex
        /// </summary>
        public Int64 VertexTypeID;

        /// <summary>
        /// The edge property id that points to this vertex
        /// </summary>
        public Int64 EdgePropertyID;

        /// <summary>
        /// The incoming vertices
        /// </summary>
        public IEnumerable<IVertex> IncomingVertices;
    }
}
