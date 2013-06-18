namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class IntervalTree
    {
        private IntervalCollection intervals;
        private IntervalBoundary root;

        internal IntervalTree()
        {
        }

        internal void Add(Interval interval)
        {
            this.AddIntervalToTree(interval);
            this.EnsureIntervals();
            this.intervals.Add(interval);
        }

        private void AddIntervalToTree(Interval interval)
        {
            this.EditLeft(interval, true);
            this.EditRight(interval, true);
        }

        private void EditLeft(Interval interval, bool add)
        {
            double num;
            if (add)
            {
                this.EnsureRoot(interval.LowerBound);
            }
            IntervalBoundary root = this.root;
            IntervalBoundary boundary2 = null;
        Label_0018:
            num = root.Value;
            if (num < interval.LowerBound)
            {
                root = add ? root.EnsureRight(interval.LowerBound) : root.Right;
                goto Label_0018;
            }
            if ((boundary2 != null) && (boundary2.Value <= interval.UpperBound))
            {
                if (add)
                {
                    root.AddToGtSlot(interval);
                }
                else
                {
                    root.RemoveFromGtSlot(interval);
                }
            }
            if (num > interval.LowerBound)
            {
                if (num < interval.UpperBound)
                {
                    if (add)
                    {
                        root.AddToEqSlot(interval);
                    }
                    else
                    {
                        root.RemoveFromEqSlot(interval);
                    }
                }
                boundary2 = root;
                root = add ? root.EnsureLeft(interval.LowerBound) : root.Left;
                goto Label_0018;
            }
            if (IntervalOp.LessThanEquals == interval.LowerOp)
            {
                if (add)
                {
                    root.AddToEqSlot(interval);
                }
                else
                {
                    root.RemoveFromEqSlot(interval);
                }
            }
        }

        private void EditRight(Interval interval, bool add)
        {
            double num;
            if (add)
            {
                this.EnsureRoot(interval.UpperBound);
            }
            IntervalBoundary root = this.root;
            IntervalBoundary boundary2 = null;
        Label_0018:
            num = root.Value;
            if (num > interval.UpperBound)
            {
                root = add ? root.EnsureLeft(interval.UpperBound) : root.Left;
                goto Label_0018;
            }
            if ((boundary2 != null) && (boundary2.Value >= interval.LowerBound))
            {
                if (add)
                {
                    root.AddToLtSlot(interval);
                }
                else
                {
                    root.RemoveFromLtSlot(interval);
                }
            }
            if (num < interval.UpperBound)
            {
                if (num > interval.LowerBound)
                {
                    if (add)
                    {
                        root.AddToEqSlot(interval);
                    }
                    else
                    {
                        root.RemoveFromEqSlot(interval);
                    }
                }
                boundary2 = root;
                root = add ? root.EnsureRight(interval.UpperBound) : root.Right;
                goto Label_0018;
            }
            if (IntervalOp.LessThanEquals == interval.UpperOp)
            {
                if (add)
                {
                    root.AddToEqSlot(interval);
                }
                else
                {
                    root.RemoveFromEqSlot(interval);
                }
            }
        }

        private void EnsureIntervals()
        {
            if (this.intervals == null)
            {
                this.intervals = new IntervalCollection();
            }
        }

        private void EnsureRoot(double val)
        {
            if (this.root == null)
            {
                this.root = new IntervalBoundary(val, null);
            }
        }

        internal IntervalBoundary FindBoundaryNode(double val)
        {
            return this.FindBoundaryNode(this.root, val);
        }

        internal IntervalBoundary FindBoundaryNode(IntervalBoundary root, double val)
        {
            IntervalBoundary boundary = null;
            if (root != null)
            {
                if (root.Value == val)
                {
                    return root;
                }
                boundary = this.FindBoundaryNode(root.Left, val);
                if (boundary == null)
                {
                    boundary = this.FindBoundaryNode(root.Right, val);
                }
            }
            return boundary;
        }

        internal Interval FindInterval(Interval interval)
        {
            return this.FindInterval(interval.LowerBound, interval.LowerOp, interval.UpperBound, interval.UpperOp);
        }

        internal Interval FindInterval(double lowerBound, IntervalOp lowerOp, double upperBound, IntervalOp upperOp)
        {
            int num;
            if ((this.intervals != null) && (-1 != (num = this.intervals.IndexOf(lowerBound, lowerOp, upperBound, upperOp))))
            {
                return this.intervals[num];
            }
            return null;
        }

        private void PruneTree(Interval intervalRemoved)
        {
            if (-1 == this.intervals.IndexOf(intervalRemoved.LowerBound))
            {
                this.RemoveBoundary(this.FindBoundaryNode(intervalRemoved.LowerBound));
            }
            if ((intervalRemoved.LowerBound != intervalRemoved.UpperBound) && (-1 == this.intervals.IndexOf(intervalRemoved.UpperBound)))
            {
                this.RemoveBoundary(this.FindBoundaryNode(intervalRemoved.UpperBound));
            }
        }

        internal void Remove(Interval interval)
        {
            this.RemoveIntervalFromTree(interval);
            this.intervals.Remove(interval);
            this.PruneTree(interval);
        }

        private void RemoveBoundary(IntervalBoundary boundary)
        {
            IntervalCollection intervalsWithEndPoint = null;
            int count = 0;
            if ((boundary.Left != null) && (boundary.Right != null))
            {
                IntervalBoundary left = boundary.Left;
                while (left.Right != null)
                {
                    left = left.Right;
                }
                intervalsWithEndPoint = this.intervals.GetIntervalsWithEndPoint(left.Value);
                count = intervalsWithEndPoint.Count;
                for (int j = 0; j < count; j++)
                {
                    this.RemoveIntervalFromTree(intervalsWithEndPoint[j]);
                }
                double num3 = boundary.Value;
                boundary.Value = left.Value;
                left.Value = num3;
                boundary = left;
            }
            if (boundary.Left != null)
            {
                this.Replace(boundary, boundary.Left);
            }
            else
            {
                this.Replace(boundary, boundary.Right);
            }
            boundary.Parent = null;
            boundary.Left = null;
            boundary.Right = null;
            for (int i = 0; i < count; i++)
            {
                this.AddIntervalToTree(intervalsWithEndPoint[i]);
            }
        }

        private void RemoveIntervalFromTree(Interval interval)
        {
            this.EditLeft(interval, false);
            this.EditRight(interval, false);
        }

        private void Replace(IntervalBoundary replace, IntervalBoundary with)
        {
            IntervalBoundary parent = replace.Parent;
            if (parent != null)
            {
                if (replace == parent.Left)
                {
                    parent.Left = with;
                }
                else if (replace == parent.Right)
                {
                    parent.Right = with;
                }
            }
            else
            {
                this.root = with;
            }
            if (with != null)
            {
                with.Parent = parent;
            }
        }

        internal void Trim()
        {
            this.intervals.Trim();
            this.root.Trim();
        }

        internal int Count
        {
            get
            {
                if (this.intervals == null)
                {
                    return 0;
                }
                return this.intervals.Count;
            }
        }

        internal IntervalCollection Intervals
        {
            get
            {
                if (this.intervals == null)
                {
                    return new IntervalCollection();
                }
                return this.intervals;
            }
        }

        internal IntervalBoundary Root
        {
            get
            {
                return this.root;
            }
        }
    }
}

