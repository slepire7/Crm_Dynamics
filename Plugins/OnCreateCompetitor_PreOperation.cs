using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Plugin.Shared;
using Plugin.Shared.Model;
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
            var Competidor_Target = pluginContext.GetTarget<Competitor>();

            if (HasExist(ref Competidor_Target, pluginContext)) return;

            var Copy_Competitor = Competidor_Target.CreateCopy();
            pluginContext.OrganizationService.Create(Copy_Competitor);
        }

        private bool HasExist(ref Competitor Competitor, PluginContext _pluginCtx)
        {

            var queryExpression = new QueryExpression(Competitor.LogicalName);
            queryExpression.Criteria.AddCondition("name", ConditionOperator.Equal, Competitor.Name);
            queryExpression.Criteria.AddCondition("websiteurl", ConditionOperator.Equal, Competitor.WebSiteUrl);
            var Query_Expression_Result = _pluginCtx.OrganizationService.RetrieveMultiple(queryExpression).Entities;
            return Query_Expression_Result.Any();

        }
    }
}
