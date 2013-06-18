namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public class RuleExecution
    {
        private System.Workflow.ComponentModel.Activity activity;
        private System.Workflow.ComponentModel.ActivityExecutionContext activityExecutionContext;
        private bool halted;
        private RuleLiteralResult thisLiteralResult;
        private object thisObject;
        private RuleValidation validation;

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId="1#")]
        public RuleExecution(RuleValidation validation, object thisObject)
        {
            if (validation == null)
            {
                throw new ArgumentNullException("validation");
            }
            if (thisObject == null)
            {
                throw new ArgumentNullException("thisObject");
            }
            if (validation.ThisType != thisObject.GetType())
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.ValidationMismatch, new object[] { RuleDecompiler.DecompileType(validation.ThisType), RuleDecompiler.DecompileType(thisObject.GetType()) }));
            }
            this.validation = validation;
            this.activity = thisObject as System.Workflow.ComponentModel.Activity;
            this.thisObject = thisObject;
            this.thisLiteralResult = new RuleLiteralResult(thisObject);
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId="1#"), TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleExecution(RuleValidation validation, object thisObject, System.Workflow.ComponentModel.ActivityExecutionContext activityExecutionContext) : this(validation, thisObject)
        {
            this.activityExecutionContext = activityExecutionContext;
        }

        public System.Workflow.ComponentModel.Activity Activity
        {
            get
            {
                if (this.activity == null)
                {
                    throw new InvalidOperationException(Messages.NoActivity);
                }
                return this.activity;
            }
        }

        public System.Workflow.ComponentModel.ActivityExecutionContext ActivityExecutionContext
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activityExecutionContext;
            }
        }

        public bool Halted
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.halted;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.halted = value;
            }
        }

        internal RuleLiteralResult ThisLiteralResult
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.thisLiteralResult;
            }
        }

        public object ThisObject
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.thisObject;
            }
        }

        public RuleValidation Validation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.validation;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.validation = value;
            }
        }
    }
}

