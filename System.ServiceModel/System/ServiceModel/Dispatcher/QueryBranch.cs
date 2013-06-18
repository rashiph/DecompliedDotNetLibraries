namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class QueryBranch
    {
        internal Opcode branch;
        internal int id;

        internal QueryBranch(Opcode branch, int id)
        {
            this.branch = branch;
            this.id = id;
        }

        internal Opcode Branch
        {
            get
            {
                return this.branch;
            }
        }

        internal int ID
        {
            get
            {
                return this.id;
            }
        }
    }
}

