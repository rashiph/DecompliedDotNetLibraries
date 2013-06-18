namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.Activities.Common;
    using System.Workflow.Activities.Rules;
    using System.Workflow.ComponentModel;

    internal abstract class RuleDefinitionDynamicPropertyDescriptor : DynamicPropertyDescriptor
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleDefinitionDynamicPropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor descriptor) : base(serviceProvider, descriptor)
        {
        }

        protected RuleDefinitions GetRuleDefinitions(object component)
        {
            IReferenceService service = (IReferenceService) base.ServiceProvider.GetService(typeof(IReferenceService));
            if (service == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.MissingService, new object[] { typeof(IReferenceService).FullName }));
            }
            Activity activity = service.GetComponent(component) as Activity;
            if (activity == null)
            {
                return null;
            }
            Activity rootActivity = Helpers.GetRootActivity(activity);
            if (rootActivity == null)
            {
                return null;
            }
            Activity declaringActivity = Helpers.GetDeclaringActivity(activity);
            if ((declaringActivity != rootActivity) && (declaringActivity != null))
            {
                return ConditionHelper.GetRuleDefinitionsFromManifest(declaringActivity.GetType());
            }
            return ConditionHelper.Load_Rules_DT(base.ServiceProvider, rootActivity);
        }
    }
}

