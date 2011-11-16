/*
* 
* Copyright (C) 2011 Henning 'cosh' Rauch
*
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sones.Library.VersionedPluginManager
{
    /// <summary>
    /// Informations about instances of a certain type
    /// </summary>
    public class InstanceContainer
    {
        public ActivatorInfo Info {get; set;}
        public List<Object> Objects { get; set; }
    }
}
