namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Reflection;
    using System.Runtime;

    internal class SimpleParameterInfo : ParameterInfo
    {
        private Type parameterType;

        public SimpleParameterInfo(ParameterInfo parameter)
        {
            this.parameterType = typeof(Nullable<>).MakeGenericType(new Type[] { parameter.ParameterType });
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SimpleParameterInfo(Type parameter)
        {
            this.parameterType = parameter;
        }

        public override Type ParameterType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parameterType;
            }
        }
    }
}

