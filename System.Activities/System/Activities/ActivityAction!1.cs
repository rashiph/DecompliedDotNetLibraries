namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class ActivityAction<T> : ActivityDelegate
    {
        internal override IList<RuntimeDelegateArgument> InternalGetRuntimeDelegateArguments()
        {
            return new List<RuntimeDelegateArgument>(1) { new RuntimeDelegateArgument(ActivityDelegate.ArgumentName, typeof(T), 0, this.Argument) };
        }

        [DefaultValue((string) null)]
        public DelegateInArgument<T> Argument { get; set; }
    }
}

