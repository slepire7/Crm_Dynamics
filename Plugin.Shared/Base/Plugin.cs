using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Shared.Base
{
    public abstract class Plugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginExecute(new PluginContext(serviceProvider));
        }
        public abstract void PluginExecute(PluginContext pluginContext);
    }
}
