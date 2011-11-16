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
    /// A struct that contains the property information
    /// </summary>
    public struct PropertyContainer
    {
        /// <summary>
        /// The property id
        /// </summary>
        public Int64 PropertyID;

        /// <summary>
        /// The property itself
        /// </summary>
        public IComparable Property;
    }
}
