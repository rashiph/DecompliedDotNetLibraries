namespace System.Workflow.Activities.Rules
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing.Design;
    using System.Runtime;
    using System.Workflow.Activities.Common;
    using System.Workflow.Activities.Rules.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;

    [Editor(typeof(RuleSetNameEditor), typeof(UITypeEditor)), TypeConverter(typeof(RuleSetReferenceTypeConverter)), ActivityValidator(typeof(RuleSetReferenceValidator)), DesignerSerializer(typeof(DependencyObjectCodeDomSerializer), typeof(CodeDomSerializer)), DesignerSerializer(typeof(WorkflowMarkupSerializer), typeof(WorkflowMarkupSerializer)), Browsable(true)]
    public sealed class RuleSetReference : DependencyObject
    {
        private string _name;
        private bool _runtimeInitialized;
        [NonSerialized]
        private object syncLock;

        public RuleSetReference()
        {
            this.syncLock = new object();
        }

        public RuleSetReference(string ruleSetName)
        {
            this.syncLock = new object();
            this._name = ruleSetName;
        }

        protected override void InitializeProperties()
        {
            lock (this.syncLock)
            {
                if (!this._runtimeInitialized)
                {
                    Activity parentDependencyObject = base.ParentDependencyObject as Activity;
                    CompositeActivity declaringActivity = Helpers.GetDeclaringActivity(parentDependencyObject);
                    if (declaringActivity == null)
                    {
                        declaringActivity = Helpers.GetRootActivity(parentDependencyObject) as CompositeActivity;
                    }
                    RuleDefinitions definitions = ConditionHelper.Load_Rules_RT(declaringActivity);
                    if (definitions != null)
                    {
                        definitions.OnRuntimeInitialized();
                    }
                    base.InitializeProperties();
                    this._runtimeInitialized = true;
                }
            }
        }

        public string RuleSetName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._name;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._name = value;
            }
        }
    }
}

