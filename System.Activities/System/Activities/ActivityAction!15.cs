namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : ActivityDelegate
    {
        internal override IList<RuntimeDelegateArgument> InternalGetRuntimeDelegateArguments()
        {
            return new List<RuntimeDelegateArgument>(15) { new RuntimeDelegateArgument(ActivityDelegate.Argument1Name, typeof(T1), 0, this.Argument1), new RuntimeDelegateArgument(ActivityDelegate.Argument2Name, typeof(T2), 0, this.Argument2), new RuntimeDelegateArgument(ActivityDelegate.Argument3Name, typeof(T3), 0, this.Argument3), new RuntimeDelegateArgument(ActivityDelegate.Argument4Name, typeof(T4), 0, this.Argument4), new RuntimeDelegateArgument(ActivityDelegate.Argument5Name, typeof(T5), 0, this.Argument5), new RuntimeDelegateArgument(ActivityDelegate.Argument6Name, typeof(T6), 0, this.Argument6), new RuntimeDelegateArgument(ActivityDelegate.Argument7Name, typeof(T7), 0, this.Argument7), new RuntimeDelegateArgument(ActivityDelegate.Argument8Name, typeof(T8), 0, this.Argument8), new RuntimeDelegateArgument(ActivityDelegate.Argument9Name, typeof(T9), 0, this.Argument9), new RuntimeDelegateArgument(ActivityDelegate.Argument10Name, typeof(T10), 0, this.Argument10), new RuntimeDelegateArgument(ActivityDelegate.Argument11Name, typeof(T11), 0, this.Argument11), new RuntimeDelegateArgument(ActivityDelegate.Argument12Name, typeof(T12), 0, this.Argument12), new RuntimeDelegateArgument(ActivityDelegate.Argument13Name, typeof(T13), 0, this.Argument13), new RuntimeDelegateArgument(ActivityDelegate.Argument14Name, typeof(T14), 0, this.Argument14), new RuntimeDelegateArgument(ActivityDelegate.Argument15Name, typeof(T15), 0, this.Argument15) };
        }

        [DefaultValue((string) null)]
        public DelegateInArgument<T1> Argument1 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T10> Argument10 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T11> Argument11 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T12> Argument12 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T13> Argument13 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T14> Argument14 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T15> Argument15 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T2> Argument2 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T3> Argument3 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T4> Argument4 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T5> Argument5 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T6> Argument6 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T7> Argument7 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T8> Argument8 { get; set; }

        [DefaultValue((string) null)]
        public DelegateInArgument<T9> Argument9 { get; set; }
    }
}

