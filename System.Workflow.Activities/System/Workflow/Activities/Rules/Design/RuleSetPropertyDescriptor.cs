namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Workflow.Activities.Common;
    using System.Workflow.Activities.Rules;
    using System.Workflow.ComponentModel;

    internal class RuleSetPropertyDescriptor : RuleDefinitionDynamicPropertyDescriptor
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleSetPropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor descriptor) : base(serviceProvider, descriptor)
        {
        }

        public override object GetEditor(Type editorBaseType)
        {
            new SecurityPermission(PermissionState.Unrestricted).Demand();
            return new RuleSetDefinitionEditor();
        }

        public override object GetValue(object component)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            RuleSetReference reference = component as RuleSetReference;
            if (!string.IsNullOrEmpty(reference.RuleSetName))
            {
                RuleDefinitions ruleDefinitions = base.GetRuleDefinitions(component);
                if (ruleDefinitions != null)
                {
                    RuleSetCollection ruleSets = ruleDefinitions.RuleSets;
                    if ((ruleSets != null) && ruleSets.Contains(reference.RuleSetName))
                    {
                        return ruleSets[reference.RuleSetName];
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
            RuleSetReference reference = component as RuleSetReference;
            if (reference == null)
            {
                throw new ArgumentNullException("component");
            }
            RuleSet item = value as RuleSet;
            if (!string.IsNullOrEmpty(reference.RuleSetName))
            {
                ISite serviceProvider = PropertyDescriptorUtils.GetSite(base.ServiceProvider, component);
                if (serviceProvider == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.MissingService, new object[] { typeof(ISite).FullName }));
                }
                RuleSetCollection ruleSets = null;
                RuleDefinitions definitions = ConditionHelper.Load_Rules_DT(serviceProvider, Helpers.GetRootActivity(serviceProvider.Component as Activity));
                if (definitions != null)
                {
                    ruleSets = definitions.RuleSets;
                }
                if ((ruleSets != null) && ruleSets.Contains(reference.RuleSetName))
                {
                    ruleSets.Remove(reference.RuleSetName);
                    ruleSets.Add(item);
                    ConditionHelper.Flush_Rules_DT(serviceProvider, Helpers.GetRootActivity(serviceProvider.Component as Activity));
                }
            }
        }

        public override TypeConverter Converter
        {
            get
            {
                return new RuleSetDefinitionTypeConverter();
            }
        }

        public override string Description
        {
            get
            {
                return SR.GetString("RuleSetDefinitionDescription");
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

