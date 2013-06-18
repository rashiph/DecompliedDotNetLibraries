namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Reflection;

    internal class QueryBranchTable
    {
        private QueryBranch[] branches;
        private int count;

        internal QueryBranchTable() : this(1)
        {
        }

        internal QueryBranchTable(int capacity)
        {
            this.branches = new QueryBranch[capacity];
        }

        internal void AddInOrder(QueryBranch branch)
        {
            int index = 0;
            while (index < this.count)
            {
                if (this.branches[index].ID >= branch.ID)
                {
                    break;
                }
                index++;
            }
            this.InsertAt(index, branch);
        }

        private void Grow()
        {
            QueryBranch[] destinationArray = new QueryBranch[this.branches.Length + 1];
            Array.Copy(this.branches, destinationArray, this.branches.Length);
            this.branches = destinationArray;
        }

        public int IndexOf(Opcode opcode)
        {
            for (int i = 0; i < this.count; i++)
            {
                if (object.ReferenceEquals(opcode, this.branches[i].Branch))
                {
                    return i;
                }
            }
            return -1;
        }

        public int IndexOfID(int id)
        {
            for (int i = 0; i < this.count; i++)
            {
                if (this.branches[i].ID == id)
                {
                    return i;
                }
            }
            return -1;
        }

        internal void InsertAt(int index, QueryBranch branch)
        {
            if (this.count == this.branches.Length)
            {
                this.Grow();
            }
            if (index < this.count)
            {
                Array.Copy(this.branches, index, this.branches, index + 1, this.count - index);
            }
            this.branches[index] = branch;
            this.count++;
        }

        internal bool Remove(Opcode branch)
        {
            int index = this.IndexOf(branch);
            if (index >= 0)
            {
                this.RemoveAt(index);
                return true;
            }
            return false;
        }

        internal void RemoveAt(int index)
        {
            if (index < (this.count - 1))
            {
                Array.Copy(this.branches, index + 1, this.branches, index, (this.count - index) - 1);
            }
            else
            {
                this.branches[index] = null;
            }
            this.count--;
        }

        internal void Trim()
        {
            if (this.count < this.branches.Length)
            {
                QueryBranch[] destinationArray = new QueryBranch[this.count];
                Array.Copy(this.branches, destinationArray, this.count);
                this.branches = destinationArray;
            }
            for (int i = 0; i < this.branches.Length; i++)
            {
                if ((this.branches[i] != null) && (this.branches[i].Branch != null))
                {
                    this.branches[i].Branch.Trim();
                }
            }
        }

        internal int Count
        {
            get
            {
                return this.count;
            }
        }

        internal QueryBranch this[int index]
        {
            get
            {
                return this.branches[index];
            }
        }
    }
}

