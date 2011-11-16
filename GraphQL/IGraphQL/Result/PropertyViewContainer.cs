/*
* 
* Copyright (C) 2011 Henning 'cosh' Rauch
*
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace sones.GraphQL.Result
{
    /// <summary>
    /// A struct that contains a property view
    /// </summary>
    public struct PropertyViewContainer
    {
        /// <summary>
        /// The property name of the property
        /// </summary>
        public String PropertyName;

        /// <summary>
        /// The property itself
        /// </summary>
        public Object Property;
    }
}
