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

    internal class RuleConditionReferenceNamePropertyDescriptor : DynamicPropertyDescriptor
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleConditionReferenceNamePropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor descriptor) : base(serviceProvider, descriptor)
        {
        }

        public override object GetEditor(Type editorBaseType)
        {
            new SecurityPermission(PermissionState.Unrestricted).Demand();
            return new ConditionNameEditor();
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
                return reference.ConditionName;
            }
            return null;
        }

        public override void SetValue(object component, object value)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (!(component is RuleConditionReference))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Messages.NotARuleConditionReference, new object[] { "component" }), "component");
            }
            string key = value as string;
            if ((key == null) || (key.TrimEnd(new char[0]).Length == 0))
            {
                key = string.Empty;
            }
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
            if (((conditions != null) && (key.Length != 0)) && !conditions.Contains(key))
            {
                RuleExpressionCondition item = new RuleExpressionCondition {
                    Name = key
                };
                conditions.Add(item);
                ConditionHelper.Flush_Rules_DT(serviceProvider, Helpers.GetRootActivity(serviceProvider.Component as Activity));
            }
            PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(component)["ConditionName"];
            if (propertyDescriptor != null)
            {
                PropertyDescriptorUtils.SetPropertyValue(serviceProvider, propertyDescriptor, component, key);
            }
        }

        public override string Description
        {
            get
            {
                return Messages.NamePropertyDescription;
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

