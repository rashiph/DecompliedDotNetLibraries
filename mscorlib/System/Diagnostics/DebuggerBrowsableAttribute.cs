namespace System.Diagnostics
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false)]
    public sealed class DebuggerBrowsableAttribute : Attribute
    {
        private DebuggerBrowsableState state;

        public DebuggerBrowsableAttribute(DebuggerBrowsableState state)
        {
            if ((state < DebuggerBrowsableState.Never) || (state > DebuggerBrowsableState.RootHidden))
            {
                throw new ArgumentOutOfRangeException("state");
            }
            this.state = state;
        }

        public DebuggerBrowsableState State
        {
            get
            {
                return this.state;
            }
        }
    }
}

