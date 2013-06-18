namespace System.Workflow.ComponentModel
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;

    [ActivityValidator(typeof(FieldBindValidator))]
    internal sealed class FieldBind : MemberBind
    {
        private string path;

        public FieldBind()
        {
            this.path = string.Empty;
        }

        public FieldBind(string name) : base(name)
        {
            this.path = string.Empty;
        }

        public FieldBind(string name, string path) : base(name)
        {
            this.path = string.Empty;
            this.path = path;
        }

        public override object GetRuntimeValue(Activity activity)
        {
            throw new NotImplementedException();
        }

        public override object GetRuntimeValue(Activity activity, Type targetType)
        {
            throw new NotImplementedException();
        }

        protected override void OnRuntimeInitialized(Activity activity)
        {
            throw new NotImplementedException();
        }

        public override void SetRuntimeValue(Activity activity, object value)
        {
            throw new NotImplementedException();
        }

        public string Path
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.path;
            }
            set
            {
                if (!base.designMode)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
                }
                this.path = value;
            }
        }
    }
}

