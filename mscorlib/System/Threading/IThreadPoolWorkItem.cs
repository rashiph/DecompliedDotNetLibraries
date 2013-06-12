namespace System.Threading
{
    using System;
    using System.Security;

    internal interface IThreadPoolWorkItem
    {
        [SecurityCritical]
        void ExecuteWorkItem();
        [SecurityCritical]
        void MarkAborted(ThreadAbortException tae);
    }
}

