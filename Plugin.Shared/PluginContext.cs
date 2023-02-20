using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Shared
{
    public class PluginContext
    {
        ITracingService _Logger;
        public IPluginExecutionContext Context;
        public IOrganizationService OrganizationService;
        public PluginContext(IServiceProvider serviceProvider)
        {
            _Logger = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            OrganizationService = serviceFactory.CreateOrganizationService(Context.InitiatingUserId);
        }

        public void Trace(string message)
        {
            this._Logger.Trace(message);
        }

        public Entity GetTarget()
        {
            const string Key = "Target";
            return Context.InputParameters.ContainsKey(Key) ? (Entity)Context.InputParameters[Key] : default;
        }
    }
}
