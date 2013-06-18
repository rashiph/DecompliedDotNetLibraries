namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime;

    internal class ArgBuilder
    {
        internal Type ArgType;
        internal int Index;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ArgBuilder(int index, Type argType)
        {
            this.Index = index;
            this.ArgType = argType;
        }
    }
}

