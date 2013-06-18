namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal class IntervalBranchIndex : QueryBranchIndex
    {
        private IntervalTree intervalTree = new IntervalTree();

        internal IntervalBranchIndex()
        {
        }

        internal override void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            for (int i = 0; i < this.intervalTree.Intervals.Count; i++)
            {
                this.intervalTree.Intervals[i].Branch.Branch.CollectXPathFilters(filters);
            }
        }

        private void Match(int valIndex, double point, QueryBranchResultSet results)
        {
            IntervalTreeTraverser traverser = new IntervalTreeTraverser(point, this.intervalTree.Root);
            while (traverser.MoveNext())
            {
                IntervalCollection slot = traverser.Slot;
                int num = 0;
                int count = slot.Count;
                while (num < count)
                {
                    QueryBranch branch = slot[num].Branch;
                    if (branch != null)
                    {
                        results.Add(branch, valIndex);
                    }
                    num++;
                }
            }
        }

        internal override void Match(int valIndex, ref Value val, QueryBranchResultSet results)
        {
            if (ValueDataType.Sequence == val.Type)
            {
                NodeSequence sequence = val.Sequence;
                for (int i = 0; i < sequence.Count; i++)
                {
                    this.Match(valIndex, sequence.Items[i].NumberValue(), results);
                }
            }
            else
            {
                this.Match(valIndex, val.ToDouble(), results);
            }
        }

        internal override void Remove(object key)
        {
            this.intervalTree.Remove((Interval) key);
        }

        internal override void Trim()
        {
            this.intervalTree.Trim();
        }

        internal override int Count
        {
            get
            {
                return this.intervalTree.Count;
            }
        }

        internal override QueryBranch this[object key]
        {
            get
            {
                Interval interval = this.intervalTree.FindInterval((Interval) key);
                if (interval != null)
                {
                    return interval.Branch;
                }
                return null;
            }
            set
            {
                Interval interval = (Interval) key;
                interval.Branch = value;
                this.intervalTree.Add(interval);
            }
        }
    }
}

