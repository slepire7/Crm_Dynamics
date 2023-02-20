using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Plugin.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins
{
    public class OnCreateCompetitor_PreOperation : Plugin.Shared.Base.Plugin
    {
        public const int DEPTH = 2;
        public override void PluginExecute(PluginContext pluginContext)
        {
            if (pluginContext.Context.Depth > DEPTH) return;
            var target = pluginContext.GetTarget();
            if (HasExist(ref target, pluginContext)) return;



            var copy = new Entity(target.LogicalName);
            foreach (var field in target.Attributes)
            {
                copy[field.Key] = field.Value;
            }
        }

        private bool HasExist(ref Entity Competitor, PluginContext _pluginCtx)
        {

            var queryExpression = new QueryExpression(Competitor.LogicalName);
            queryExpression.Criteria.AddCondition("name", ConditionOperator.Equal, Competitor.GetAttributeValue<string>("name"));
            queryExpression.Criteria.AddCondition("websiteurl", ConditionOperator.Equal, Competitor.GetAttributeValue<string>("websiteurl"));
            return _pluginCtx.OrganizationService.RetrieveMultiple(queryExpression).Entities.Any();

        }
    }
}
