namespace System.Workflow.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;

    [DesignerSerializer(typeof(DependencyObjectCodeDomSerializer), typeof(CodeDomSerializer)), DesignerSerializer(typeof(WorkflowMarkupSerializer), typeof(WorkflowMarkupSerializer)), TypeConverter(typeof(ConditionTypeConverter)), Browsable(true), ActivityValidator(typeof(ConditionValidator)), MergableProperty(false)]
    public abstract class ActivityCondition : DependencyObject
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ActivityCondition()
        {
        }

        public abstract bool Evaluate(Activity activity, IServiceProvider provider);
    }
}

