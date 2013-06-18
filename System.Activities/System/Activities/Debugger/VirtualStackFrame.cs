namespace System.Activities.Debugger
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    [DebuggerNonUserCode]
    public class VirtualStackFrame
    {
        private IDictionary<string, object> locals;
        private System.Activities.Debugger.State state;

        public VirtualStackFrame(System.Activities.Debugger.State state) : this(state, null)
        {
        }

        public VirtualStackFrame(System.Activities.Debugger.State state, IDictionary<string, object> locals)
        {
            this.state = state;
            this.locals = locals;
        }

        public override string ToString()
        {
            return this.state.ToString();
        }

        public IDictionary<string, object> Locals
        {
            get
            {
                return this.locals;
            }
        }

        public System.Activities.Debugger.State State
        {
            get
            {
                return this.state;
            }
        }
    }
}

