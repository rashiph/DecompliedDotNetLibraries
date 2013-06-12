namespace System.Threading
{
    using System;
    using System.Security;

    internal class ThreadHelper
    {
        internal static ContextCallback _ccb = new ContextCallback(ThreadHelper.ThreadStart_Context);
        private ExecutionContext _executionContext;
        private Delegate _start;
        private object _startArg;

        internal ThreadHelper(Delegate start)
        {
            this._start = start;
        }

        internal void SetExecutionContextHelper(ExecutionContext ec)
        {
            this._executionContext = ec;
        }

        [SecurityCritical]
        internal void ThreadStart()
        {
            if (this._executionContext != null)
            {
                ExecutionContext.Run(this._executionContext, _ccb, this);
            }
            else
            {
                ((System.Threading.ThreadStart) this._start)();
            }
        }

        [SecurityCritical]
        internal void ThreadStart(object obj)
        {
            this._startArg = obj;
            if (this._executionContext != null)
            {
                ExecutionContext.Run(this._executionContext, _ccb, this);
            }
            else
            {
                ((ParameterizedThreadStart) this._start)(obj);
            }
        }

        internal static void ThreadStart_Context(object state)
        {
            ThreadHelper helper = (ThreadHelper) state;
            if (helper._start is System.Threading.ThreadStart)
            {
                ((System.Threading.ThreadStart) helper._start)();
            }
            else
            {
                ((ParameterizedThreadStart) helper._start)(helper._startArg);
            }
        }
    }
}

