namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Workflow.Activities.Common;
    using System.Workflow.Activities.Rules;
    using System.Workflow.ComponentModel;

    internal class RuleConditionReferencePropertyDescriptor : RuleDefinitionDynamicPropertyDescriptor
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleConditionReferencePropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor descriptor) : base(serviceProvider, descriptor)
        {
        }

        public override object GetEditor(Type editorBaseType)
        {
            new SecurityPermission(PermissionState.Unrestricted).Demand();
            return new LogicalExpressionEditor();
        }

        public override object GetValue(object component)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            RuleConditionReference reference = component as RuleConditionReference;
            if (reference == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Messages.NotARuleConditionReference, new object[] { "component" }), "component");
            }
            if (reference.ConditionName != null)
            {
                RuleDefinitions ruleDefinitions = base.GetRuleDefinitions(component);
                if (ruleDefinitions != null)
                {
                    RuleConditionCollection conditions = ruleDefinitions.Conditions;
                    if ((conditions != null) && conditions.Contains(reference.ConditionName))
                    {
                        RuleExpressionCondition condition = (RuleExpressionCondition) conditions[reference.ConditionName];
                        return condition.Expression;
                    }
                }
            }
            return null;
        }

        public override void SetValue(object component, object value)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            RuleConditionReference reference = component as RuleConditionReference;
            if (reference == null)
            {
                throw new ArgumentNullException("component");
            }
            CodeExpression expression = value as CodeExpression;
            if (reference.ConditionName != null)
            {
                ISite serviceProvider = PropertyDescriptorUtils.GetSite(base.ServiceProvider, component);
                if (serviceProvider == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.MissingService, new object[] { typeof(ISite).FullName }));
                }
                RuleConditionCollection conditions = null;
                RuleDefinitions definitions = ConditionHelper.Load_Rules_DT(serviceProvider, Helpers.GetRootActivity(serviceProvider.Component as Activity));
                if (definitions != null)
                {
                    conditions = definitions.Conditions;
                }
                if ((conditions != null) && conditions.Contains(reference.ConditionName))
                {
                    RuleExpressionCondition condition = (RuleExpressionCondition) conditions[reference.ConditionName];
                    condition.Expression = expression;
                    ConditionHelper.Flush_Rules_DT(serviceProvider, Helpers.GetRootActivity(serviceProvider.Component as Activity));
                }
            }
        }

        public override TypeConverter Converter
        {
            get
            {
                return new RuleConditionReferenceExpressionTypeConverter();
            }
        }

        public override string Description
        {
            get
            {
                return Messages.ExpressionPropertyDescription;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
    }
}

