namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    [SRCategory("Standard"), Designer(typeof(IfElseBranchDesigner), typeof(IDesigner)), ToolboxItem(false), ActivityValidator(typeof(IfElseBranchValidator)), ToolboxBitmap(typeof(IfElseBranchActivity), "Resources.DecisionBranch.bmp")]
    public sealed class IfElseBranchActivity : SequenceActivity
    {
        public static readonly DependencyProperty ConditionProperty = DependencyProperty.Register("Condition", typeof(ActivityCondition), typeof(IfElseBranchActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IfElseBranchActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IfElseBranchActivity(string name) : base(name)
        {
        }

        [RefreshProperties(RefreshProperties.Repaint), DefaultValue((string) null), SRCategory("Conditions"), SRDescription("ConditionDescr")]
        public ActivityCondition Condition
        {
            get
            {
                return (base.GetValue(ConditionProperty) as ActivityCondition);
            }
            set
            {
                base.SetValue(ConditionProperty, value);
            }
        }
    }
}

