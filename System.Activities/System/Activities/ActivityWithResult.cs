namespace System.Activities
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    public abstract class ActivityWithResult : Activity
    {
        internal ActivityWithResult()
        {
        }

        internal abstract Type InternalResultType { get; }

        [IgnoreDataMember]
        public OutArgument Result
        {
            get
            {
                return this.ResultCore;
            }
            set
            {
                this.ResultCore = value;
            }
        }

        internal abstract OutArgument ResultCore { get; set; }

        internal RuntimeArgument ResultRuntimeArgument { get; set; }

        public Type ResultType
        {
            get
            {
                return this.InternalResultType;
            }
        }
    }
}

