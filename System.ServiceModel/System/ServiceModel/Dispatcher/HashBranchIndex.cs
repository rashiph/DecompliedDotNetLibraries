namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal abstract class HashBranchIndex : QueryBranchIndex
    {
        private Dictionary<object, QueryBranch> literals = new Dictionary<object, QueryBranch>();

        internal HashBranchIndex()
        {
        }

        internal override void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            foreach (QueryBranch branch in this.literals.Values)
            {
                branch.Branch.CollectXPathFilters(filters);
            }
        }

        internal override void Remove(object key)
        {
            this.literals.Remove(key);
        }

        internal override void Trim()
        {
        }

        internal override int Count
        {
            get
            {
                return this.literals.Count;
            }
        }

        internal override QueryBranch this[object literal]
        {
            get
            {
                QueryBranch branch;
                if (this.literals.TryGetValue(literal, out branch))
                {
                    return branch;
                }
                return null;
            }
            set
            {
                this.literals[literal] = value;
            }
        }
    }
}

