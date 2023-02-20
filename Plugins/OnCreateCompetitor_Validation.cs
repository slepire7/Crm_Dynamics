using Microsoft.Xrm.Sdk;
using Plugin.Shared;
using System;
using System.Linq;

namespace Plugins
{
    public class OnCreateCompetitor_Validation : Plugin.Shared.Base.Plugin
    {
        public const int DEPTH = 2;
        public override void PluginExecute(PluginContext pluginContext)
        {
            try
            {
                if (pluginContext.Context.Depth > DEPTH) return;
                Entity Competitor = pluginContext.GetTarget();
                Validation_WebSite(ref Competitor);

            }
            catch (InvalidPluginExecutionException ex) when (ex.InnerException != null)
            {
                pluginContext.Trace(ex.InnerException.Message);
                throw ex;
            }
            catch (InvalidPluginExecutionException ex)
            {
                pluginContext.Trace(ex.Message);
                throw ex;
            }
        }
        public void Validation_WebSite(ref Entity Competitor)
        {
            string Url_Competitor = Competitor.GetAttributeValue<string>("websiteurl");
            if (string.IsNullOrEmpty(Url_Competitor) == false)
            {
                Uri[] BLACK_LIST_WEBSITES = new Uri[2] {
                     new Uri("https://www.google.com")
                    ,new Uri("https://www.bing.com")
                };

                if (BLACK_LIST_WEBSITES.Any(uri => uri.Host == new Uri(Url_Competitor).Host))
                    throw new InvalidPluginExecutionException($"o site: {Url_Competitor} não pode ser utilizado", PluginHttpStatusCode.BadRequest);
            }
        }
    }
}
