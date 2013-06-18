namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.Activities.Rules;

    internal class RuleConditionReferenceExpressionTypeConverter : TypeConverter
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal RuleConditionReferenceExpressionTypeConverter()
        {
        }

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
            if (destinationType != typeof(string))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            CodeExpression expression = value as CodeExpression;
            if (expression == null)
            {
                return Messages.ConditionExpression;
            }
            return new RuleExpressionCondition(expression).ToString();
        }
    }
}

