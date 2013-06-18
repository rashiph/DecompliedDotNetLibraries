namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    [SRDescription("StateMachineWorkflowActivityDescription"), ActivityValidator(typeof(StateActivityValidator)), ToolboxItem(false), Designer(typeof(StateMachineWorkflowDesigner), typeof(IRootDesigner)), Designer(typeof(StateMachineWorkflowDesigner), typeof(IDesigner)), SRCategory("Standard"), ComVisible(false), ToolboxBitmap(typeof(StateMachineWorkflowActivity), "Resources.StateMachineWorkflowActivity.png"), SRDisplayName("StateMachineWorkflow")]
    public class StateMachineWorkflowActivity : StateActivity
    {
        public static readonly DependencyProperty CompletedStateNameProperty = DependencyProperty.Register("CompletedStateName", typeof(string), typeof(StateMachineWorkflowActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        internal const string CompletedStateNamePropertyName = "CompletedStateName";
        public static readonly DependencyProperty InitialStateNameProperty = DependencyProperty.Register("InitialStateName", typeof(string), typeof(StateMachineWorkflowActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        internal const string InitialStateNamePropertyName = "InitialStateName";
        public const string SetStateQueueName = "SetStateQueue";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public StateMachineWorkflowActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public StateMachineWorkflowActivity(string name) : base(name)
        {
        }

        [SRDescription("CompletedStateDescription"), Editor(typeof(StateDropDownEditor), typeof(UITypeEditor)), DefaultValue("")]
        public string CompletedStateName
        {
            get
            {
                return (string) base.GetValue(CompletedStateNameProperty);
            }
            set
            {
                base.SetValue(CompletedStateNameProperty, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string CurrentStateName
        {
            get
            {
                StateMachineExecutionState executionState = this.ExecutionState;
                if (executionState == null)
                {
                    return null;
                }
                return executionState.CurrentStateName;
            }
        }

        [SRCategory("Conditions"), SRDescription("DynamicUpdateConditionDescr")]
        public ActivityCondition DynamicUpdateCondition
        {
            get
            {
                return (WorkflowChanges.GetCondition(this) as ActivityCondition);
            }
            set
            {
                WorkflowChanges.SetCondition(this, value);
            }
        }

        internal StateMachineExecutionState ExecutionState
        {
            get
            {
                return (StateMachineExecutionState) base.GetValue(StateActivity.StateMachineExecutionStateProperty);
            }
        }

        [Editor(typeof(StateDropDownEditor), typeof(UITypeEditor)), DefaultValue(""), ValidationOption(ValidationOption.Optional), SRDescription("InitialStateDescription")]
        public string InitialStateName
        {
            get
            {
                return (string) base.GetValue(InitialStateNameProperty);
            }
            set
            {
                base.SetValue(InitialStateNameProperty, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string PreviousStateName
        {
            get
            {
                StateMachineExecutionState executionState = this.ExecutionState;
                if (executionState == null)
                {
                    return null;
                }
                return executionState.PreviousStateName;
            }
        }
    }
}

