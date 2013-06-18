namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class ActivityFunc<T, TResult> : ActivityDelegate
    {
        protected internal override DelegateOutArgument GetResultArgument()
        {
            return this.Result;
        }

        internal override IList<RuntimeDelegateArgument> InternalGetRuntimeDelegateArguments()
        {
            return new List<RuntimeDelegateArgument>(2) { new RuntimeDelegateArgument(ActivityDelegate.ArgumentName, typeof(T), 0, this.Argument), new RuntimeDelegateArgument(ActivityDelegate.ResultArgumentName, typeof(TResult), 1, this.Result) };
        }

        [DefaultValue((string) null)]
        public DelegateInArgument<T> Argument { get; set; }

        [DefaultValue((string) null)]
        public DelegateOutArgument<TResult> Result { get; set; }
    }
}

