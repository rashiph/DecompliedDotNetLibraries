namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct QueryBranchResult
    {
        internal QueryBranch branch;
        private int valIndex;
        internal QueryBranchResult(QueryBranch branch, int valIndex)
        {
            this.branch = branch;
            this.valIndex = valIndex;
        }

        internal QueryBranch Branch
        {
            get
            {
                return this.branch;
            }
        }
        internal int ValIndex
        {
            get
            {
                return this.valIndex;
            }
        }
    }
}

