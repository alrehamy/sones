/*
* 
* Copyright (C) 2011 Henning 'cosh' Rauch
*
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace sones.Library.PropertyHyperGraph
{
    /// <summary>
    /// A struct that contains a binary property
    /// </summary>
    public struct BinaryPropertyContainer
    {
        /// <summary>
        /// The property id of the binary property
        /// </summary>
        public Int64 PropertyID;

        /// <summary>
        /// The binary property itself
        /// </summary>
        public Stream BinaryPropery;
    }
}
