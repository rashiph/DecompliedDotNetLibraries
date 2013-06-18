namespace System.Web.Services.Protocols
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HttpMethodAttribute : Attribute
    {
        private Type parameterFormatter;
        private Type returnFormatter;

        public HttpMethodAttribute()
        {
            this.returnFormatter = null;
            this.parameterFormatter = null;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public HttpMethodAttribute(Type returnFormatter, Type parameterFormatter)
        {
            this.returnFormatter = returnFormatter;
            this.parameterFormatter = parameterFormatter;
        }

        public Type ParameterFormatter
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parameterFormatter;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.parameterFormatter = value;
            }
        }

        public Type ReturnFormatter
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.returnFormatter;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.returnFormatter = value;
            }
        }
    }
}

