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
    /// A struct that contains an binary property view
    /// </summary>
    public struct BinaryPropertyViewContainer
    {
        /// <summary>
        /// The property id of the binary property
        /// </summary>
        public String PropertyName;

        /// <summary>
        /// The binary property itself
        /// </summary>
        public Stream BinaryPropery;
    }
}
