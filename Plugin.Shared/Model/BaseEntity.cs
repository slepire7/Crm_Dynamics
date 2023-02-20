using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Plugin.Shared.Model
{
    public class BaseEntity : Entity
    {
        protected BaseEntity(string logicalName) : base(logicalName) { }

        private string TransformFieldName(string prop)
        {
            return prop.ToLowerInvariant();
        }
        protected T GetValue<T>([CallerMemberName] string field = null)
        {
            return base.GetAttributeValue<T>(TransformFieldName(field));
        }
        protected void OnChangeValue(object value, [CallerMemberName] string field = null)
        {
            base.SetAttributeValue(TransformFieldName(field), value);
        }
        protected void OnChangeValue(Enum value, [CallerMemberName] string field = null)
        {
            int intValue = Convert.ToInt32(value);
            var opt_value = new OptionSetValue(intValue);
            base.SetAttributeValue(TransformFieldName(field), opt_value);
        }
    }
}
