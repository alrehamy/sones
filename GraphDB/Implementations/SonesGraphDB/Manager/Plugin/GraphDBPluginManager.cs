/*
* sones GraphDB - Community Edition - http://www.sones.com
* Copyright (C) 2007-2011 sones GmbH
*
* This file is part of sones GraphDB Community Edition.
*
* sones GraphDB is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published by
* the Free Software Foundation, version 3 of the License.
* 
* sones GraphDB is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with sones GraphDB. If not, see <http://www.gnu.org/licenses/>.
* 
*/

using System;
using sones.GraphFS;
using sones.Library.Commons.Security;
using sones.Library.Commons.Transaction;
using sones.Library.UserdefinedDataType;
using sones.Library.VersionedPluginManager;
using sones.Plugins.Index;
using sones.Plugins.Index.Versioned;

namespace sones.GraphDB.Manager.Plugin
{
    /// <summary>
    /// A plugin manager for the sones GraphDB component
    /// </summary>
    public sealed class GraphDBPluginManager : AComponentPluginManager
    {
        #region constructor

        /// <summary>
        /// Create a new sones graphDB plugin manager
        /// </summary>
        public GraphDBPluginManager()
        {
            #region Register & Discover

            // Change the version if there are ANY changes which will prevent loading the plugin.
            // As long as there are still some plugins which does not have their own assembly you need to change the compatibility of ALL plugins of the GraphDB and GraphFSInterface assembly.
            // So, if any plugin in the GraphDB changes you need to change the AssemblyVersion of the GraphDB AND modify the compatibility version of the other plugins.
            _pluginManager
                .Register<IGraphFS>(IGraphFSVersionCompatibility.MinVersion, IGraphFSVersionCompatibility.MaxVersion)
                .Register<ITransactionManager>(ITransactionManagerVersionCompatibility.MinVersion, ITransactionManagerVersionCompatibility.MaxVersion)
                .Register<ISecurityManager>(ISecurityManagerVersionCompatibility.MinVersion, ISecurityManagerVersionCompatibility.MaxVersion)
                .Register<ILogicExpressionOptimizer>(ILogicExpressionOptimizerVersionCompatibility.MinVersion, ILogicExpressionOptimizerVersionCompatibility.MaxVersion)
                .Register<ISonesIndex>(ISonesIndexVersionCompatibility.MinVersion, ISonesIndexVersionCompatibility.MaxVersion)
                .Register<ISonesVersionedIndex>(ISonesIndexVersionCompatibility.MinVersion, ISonesIndexVersionCompatibility.MaxVersion)
                .Register<AUserdefinedDataType>(ISonesIndexVersionCompatibility.MinVersion, ISonesIndexVersionCompatibility.MaxVersion);

            _pluginManager.DiscoverAllPlugins();

            #endregion

            #region Get all plugins and fill the lookup dictionaries

            var componentName = this.GetType().Assembly.GetName().Name;

            FillLookup<IGraphFS>(componentName, _ => _.PluginName);
            FillLookup<ITransactionManager>(componentName, _ => _.PluginName);
            FillLookup<ISecurityManager>(componentName, _ => _.PluginName);
            FillLookup<ILogicExpressionOptimizer>(componentName, _ => _.PluginName);
            FillLookup<ISonesIndex>(componentName, _ => _.IndexName);
            FillLookup<AUserdefinedDataType>(componentName, _ => _.TypeName);

            #endregion   
        }

        #endregion
    }
}
