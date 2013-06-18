namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class ActivityAction<T1, T2, T3> : ActivityDelegate
    {
        internal override IList<RuntimeDelegateArgument> InternalGetRuntimeDelegateArguments()
        {
            return new List<RuntimeDelegateArgument>(3) { new RuntimeDelegateArgument(ActivityDelegate.Argument1Name, typeof(T1), 0, this.Argument1), new RuntimeDelegateArgument(ActivityDelegate.Argument2Name, typeof(T2), 0, this.Argument2), new RuntimeDelegateArgument(ActivityDelegate.Argument3Name, typeof(T3), 0, this.Argument3) };
        }

        [DefaultValue((string) null)]
        public DelegateInArgument<T1> Argument1 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T2> Argument2 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T3> Argument3 { get; set; }
    }
}

