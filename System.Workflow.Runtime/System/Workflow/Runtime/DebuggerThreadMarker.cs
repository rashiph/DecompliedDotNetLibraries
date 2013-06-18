namespace System.Workflow.Runtime
{
    using System;

    internal class DebuggerThreadMarker : AmbientEnvironment
    {
        public DebuggerThreadMarker() : base(new object())
        {
        }

        internal static bool IsInDebuggerThread()
        {
            return (AmbientEnvironment.Retrieve() != null);
        }
    }
}

