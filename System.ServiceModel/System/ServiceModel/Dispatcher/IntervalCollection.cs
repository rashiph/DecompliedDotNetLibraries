namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class IntervalCollection : ArrayList
    {
        internal IntervalCollection() : base(1)
        {
        }

        internal int Add(Interval interval)
        {
            this.Capacity = this.Count + 1;
            return base.Add(interval);
        }

        internal int AddUnique(Interval interval)
        {
            int index = this.IndexOf(interval);
            if (-1 == index)
            {
                return this.Add(interval);
            }
            return index;
        }

        internal IntervalCollection GetIntervalsWithEndPoint(double endPoint)
        {
            IntervalCollection intervals = new IntervalCollection();
            int count = this.Count;
            for (int i = 0; i < count; i++)
            {
                Interval interval = this[i];
                if (interval.HasMatchingEndPoint(endPoint))
                {
                    intervals.Add(interval);
                }
            }
            return intervals;
        }

        internal int IndexOf(double endPoint)
        {
            int count = this.Count;
            for (int i = 0; i < count; i++)
            {
                Interval interval = this[i];
                if (interval.HasMatchingEndPoint(endPoint))
                {
                    return i;
                }
            }
            return -1;
        }

        internal int IndexOf(Interval interval)
        {
            return base.IndexOf(interval);
        }

        internal int IndexOf(double lowerBound, IntervalOp lowerOp, double upperBound, IntervalOp upperOp)
        {
            int count = this.Count;
            for (int i = 0; i < count; i++)
            {
                if (this[i].Equals(lowerBound, lowerOp, upperBound, upperOp))
                {
                    return i;
                }
            }
            return -1;
        }

        internal void Remove(Interval interval)
        {
            base.Remove(interval);
            this.TrimToSize();
        }

        internal void Trim()
        {
            this.TrimToSize();
        }

        internal bool HasIntervals
        {
            get
            {
                return (this.Count > 0);
            }
        }

        internal Interval this[int index]
        {
            get
            {
                return (Interval) base[index];
            }
        }
    }
}

