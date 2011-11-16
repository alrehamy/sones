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
    /// Just a wrapper to hold some information about the plugin which is going to be activated.
    /// </summary>
    public struct ActivatorInfo
    {
        public Type Type { get; set; }
        public Version MinVersion { get; set; }
        public Version MaxVersion { get; set; }
        public Object[] CtorArgs { get; set; }
        public Func<Type, Object> ActivateDelegate { get; set; }
    }
}