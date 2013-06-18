namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class IntervalBoundary
    {
        private IntervalCollection eqSlot;
        private IntervalCollection gtSlot;
        private IntervalBoundary left;
        private IntervalCollection ltSlot;
        private IntervalBoundary parent;
        private IntervalBoundary right;
        private double val;

        internal IntervalBoundary(double val, IntervalBoundary parent)
        {
            this.val = val;
            this.parent = parent;
        }

        internal void AddToEqSlot(Interval interval)
        {
            this.AddToSlot(ref this.eqSlot, interval);
        }

        internal void AddToGtSlot(Interval interval)
        {
            this.AddToSlot(ref this.gtSlot, interval);
        }

        internal void AddToLtSlot(Interval interval)
        {
            this.AddToSlot(ref this.ltSlot, interval);
        }

        private void AddToSlot(ref IntervalCollection slot, Interval interval)
        {
            if (slot == null)
            {
                slot = new IntervalCollection();
            }
            slot.AddUnique(interval);
        }

        internal IntervalBoundary EnsureLeft(double val)
        {
            if (this.left == null)
            {
                this.left = new IntervalBoundary(val, this);
            }
            return this.left;
        }

        internal IntervalBoundary EnsureRight(double val)
        {
            if (this.right == null)
            {
                this.right = new IntervalBoundary(val, this);
            }
            return this.right;
        }

        internal void RemoveFromEqSlot(Interval interval)
        {
            this.RemoveFromSlot(ref this.eqSlot, interval);
        }

        internal void RemoveFromGtSlot(Interval interval)
        {
            this.RemoveFromSlot(ref this.gtSlot, interval);
        }

        internal void RemoveFromLtSlot(Interval interval)
        {
            this.RemoveFromSlot(ref this.ltSlot, interval);
        }

        private void RemoveFromSlot(ref IntervalCollection slot, Interval interval)
        {
            if (slot != null)
            {
                slot.Remove(interval);
                if (!slot.HasIntervals)
                {
                    slot = null;
                }
            }
        }

        internal void Trim()
        {
            if (this.eqSlot != null)
            {
                this.eqSlot.Trim();
            }
            if (this.gtSlot != null)
            {
                this.gtSlot.Trim();
            }
            if (this.ltSlot != null)
            {
                this.ltSlot.Trim();
            }
            if (this.left != null)
            {
                this.left.Trim();
            }
            if (this.right != null)
            {
                this.right.Trim();
            }
        }

        internal IntervalCollection EqSlot
        {
            get
            {
                return this.eqSlot;
            }
        }

        internal IntervalCollection GtSlot
        {
            get
            {
                return this.gtSlot;
            }
        }

        internal IntervalBoundary Left
        {
            get
            {
                return this.left;
            }
            set
            {
                this.left = value;
            }
        }

        internal IntervalCollection LtSlot
        {
            get
            {
                return this.ltSlot;
            }
        }

        internal IntervalBoundary Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }

        internal IntervalBoundary Right
        {
            get
            {
                return this.right;
            }
            set
            {
                this.right = value;
            }
        }

        internal double Value
        {
            get
            {
                return this.val;
            }
            set
            {
                this.val = value;
            }
        }
    }
}

