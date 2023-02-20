using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Shared.Model
{
    [EntityLogicalName("competitor")]
    public class Competitor : BaseEntity
    {
        public static class Fields
        {
            public const string LogicalName = "competitor";
            public const string Name = "name";
            public const string WebSiteUrl = "websiteurl";
        }
        public Competitor() : base(Fields.LogicalName) { }
        [AttributeLogicalName(Fields.Name)]
        public string Name { get => base.GetValue<string>(); set => base.OnChangeValue(value); }
        [AttributeLogicalName(Fields.WebSiteUrl)]
        public string WebSiteUrl { get => base.GetValue<string>(); set => base.OnChangeValue(value); }

        public Competitor CreateCopy()
        {
            Competitor competitor = this;

            Competitor copy = new Competitor();
            foreach (var field in competitor.Attributes)
            {
                if (field.Value is Guid keyEntity && keyEntity == this.Id) continue;
                copy[field.Key] = field.Value;
            }
            return copy;
        }
    }
}
