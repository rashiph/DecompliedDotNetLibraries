namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.Activities.Common;
    using System.Workflow.Activities.Rules;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [SRDescription("PolicyActivityDescription"), ToolboxBitmap(typeof(PolicyActivity), "Resources.Rule.png"), ToolboxItem(typeof(ActivityToolboxItem)), Designer(typeof(PolicyDesigner), typeof(IDesigner)), SRCategory("Standard")]
    public sealed class PolicyActivity : Activity
    {
        public static readonly DependencyProperty RuleSetReferenceProperty = DependencyProperty.Register("RuleSetReference", typeof(System.Workflow.Activities.Rules.RuleSetReference), typeof(PolicyActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new ValidationOptionAttribute(ValidationOption.Required) }));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PolicyActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PolicyActivity(string name) : base(name)
        {
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            Activity declaringActivity = System.Workflow.Activities.Common.Helpers.GetDeclaringActivity(this);
            if (declaringActivity == null)
            {
                declaringActivity = System.Workflow.Activities.Common.Helpers.GetRootActivity(this);
            }
            RuleDefinitions definitions = (RuleDefinitions) declaringActivity.GetValue(RuleDefinitions.RuleDefinitionsProperty);
            if (definitions != null)
            {
                RuleSet set = definitions.RuleSets[this.RuleSetReference.RuleSetName];
                if (set != null)
                {
                    set.Execute(declaringActivity, executionContext);
                }
            }
            return ActivityExecutionStatus.Closed;
        }

        protected override void Initialize(IServiceProvider provider)
        {
            if (base.Parent == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_MustHaveParent"));
            }
            base.Initialize(provider);
        }

        [SRDescription("RuleSetDescription"), MergableProperty(false)]
        public System.Workflow.Activities.Rules.RuleSetReference RuleSetReference
        {
            get
            {
                return (System.Workflow.Activities.Rules.RuleSetReference) base.GetValue(RuleSetReferenceProperty);
            }
            set
            {
                base.SetValue(RuleSetReferenceProperty, value);
            }
        }
    }
}

