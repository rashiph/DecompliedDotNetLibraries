namespace System.Workflow.ComponentModel
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;

    [ActivityValidator(typeof(MethodBindValidator))]
    internal sealed class MethodBind : MemberBind
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MethodBind()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MethodBind(string name) : base(name)
        {
        }

        public override object GetRuntimeValue(Activity activity)
        {
            throw new Exception(SR.GetString("Error_NoTargetTypeForMethod"));
        }

        public override object GetRuntimeValue(Activity activity, Type targetType)
        {
            throw new NotImplementedException();
        }

        public override void SetRuntimeValue(Activity activity, object value)
        {
            throw new Exception(SR.GetString("Error_MethodDataSourceIsReadOnly"));
        }
    }
}

