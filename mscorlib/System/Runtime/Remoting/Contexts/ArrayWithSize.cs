namespace System.Runtime.Remoting.Contexts
{
    using System;

    internal class ArrayWithSize
    {
        internal int Count;
        internal IDynamicMessageSink[] Sinks;

        internal ArrayWithSize(IDynamicMessageSink[] sinks, int count)
        {
            this.Sinks = sinks;
            this.Count = count;
        }
    }
}

