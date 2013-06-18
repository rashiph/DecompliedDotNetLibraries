namespace System.Workflow.ComponentModel
{
    using System;
    using System.Reflection;
    using System.Runtime;

    internal sealed class WalkerEventArgs : EventArgs
    {
        private WalkerAction action;
        private Activity currentActivity;
        private PropertyInfo currentProperty;
        private object currentPropertyOwner;
        private object currentValue;

        internal WalkerEventArgs(Activity currentActivity)
        {
            this.currentActivity = currentActivity;
            this.currentPropertyOwner = null;
            this.currentProperty = null;
            this.currentValue = null;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal WalkerEventArgs(Activity currentActivity, object currentValue, PropertyInfo currentProperty, object currentPropertyOwner) : this(currentActivity)
        {
            this.currentPropertyOwner = currentPropertyOwner;
            this.currentProperty = currentProperty;
            this.currentValue = currentValue;
        }

        public WalkerAction Action
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.action;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.action = value;
            }
        }

        public Activity CurrentActivity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.currentActivity;
            }
        }

        public PropertyInfo CurrentProperty
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.currentProperty;
            }
        }

        public object CurrentPropertyOwner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.currentPropertyOwner;
            }
        }

        public object CurrentValue
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.currentValue;
            }
        }
    }
}

