namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public class ActivityChangedEventArgs : EventArgs
    {
        private System.Workflow.ComponentModel.Activity activity;
        private MemberDescriptor member;
        private object newValue;
        private object oldValue;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityChangedEventArgs(System.Workflow.ComponentModel.Activity activity, MemberDescriptor member, object oldValue, object newValue)
        {
            this.activity = activity;
            this.member = member;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public System.Workflow.ComponentModel.Activity Activity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activity;
            }
        }

        public MemberDescriptor Member
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.member;
            }
        }

        public object NewValue
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.newValue;
            }
        }

        public object OldValue
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.oldValue;
            }
        }
    }
}

