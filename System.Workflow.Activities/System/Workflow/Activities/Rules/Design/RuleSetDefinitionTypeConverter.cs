namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Workflow.Activities.Rules;

    internal class RuleSetDefinitionTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            RuleSet ruleSet = value as RuleSet;
            if ((destinationType == typeof(string)) && (ruleSet != null))
            {
                return DesignerHelpers.GetRuleSetPreview(ruleSet);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

