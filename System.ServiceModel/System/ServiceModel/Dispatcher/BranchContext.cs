namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct BranchContext
    {
        private ProcessingContext branchContext;
        private ProcessingContext sourceContext;
        internal BranchContext(ProcessingContext context)
        {
            this.sourceContext = context;
            this.branchContext = null;
        }

        internal ProcessingContext Create()
        {
            if (this.branchContext == null)
            {
                this.branchContext = this.sourceContext.Clone();
            }
            else
            {
                this.branchContext.CopyFrom(this.sourceContext);
            }
            return this.branchContext;
        }

        internal void Release()
        {
            if (this.branchContext != null)
            {
                this.branchContext.Release();
            }
        }
    }
}

