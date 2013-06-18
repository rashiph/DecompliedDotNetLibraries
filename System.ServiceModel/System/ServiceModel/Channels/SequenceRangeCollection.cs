namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.ServiceModel;
    using System.Text;

    internal abstract class SequenceRangeCollection
    {
        private static EmptyRangeCollection empty = new EmptyRangeCollection();
        private static LowerComparer lowerComparer = new LowerComparer();
        private static UpperComparer upperComparer = new UpperComparer();

        protected SequenceRangeCollection()
        {
        }

        public abstract bool Contains(long number);
        private static SequenceRangeCollection GeneralCreate(SequenceRange[] sortedRanges)
        {
            if (sortedRanges.Length == 0)
            {
                return empty;
            }
            if (sortedRanges.Length == 1)
            {
                return new SingleItemRangeCollection(sortedRanges[0]);
            }
            return new MultiItemRangeCollection(sortedRanges);
        }

        private static SequenceRangeCollection GeneralMerge(SequenceRange[] sortedRanges, SequenceRange range)
        {
            int num;
            int num2;
            if (sortedRanges.Length == 0)
            {
                return new SingleItemRangeCollection(range);
            }
            if (sortedRanges.Length == 1)
            {
                if (range.Lower == sortedRanges[0].Upper)
                {
                    num = 0;
                }
                else if (range.Lower < sortedRanges[0].Upper)
                {
                    num = -1;
                }
                else
                {
                    num = -2;
                }
            }
            else
            {
                num = Array.BinarySearch<SequenceRange>(sortedRanges, new SequenceRange(range.Lower), upperComparer);
            }
            if (num < 0)
            {
                num = ~num;
                if ((num > 0) && (sortedRanges[num - 1].Upper == (range.Lower - 1L)))
                {
                    num--;
                }
                if (num == sortedRanges.Length)
                {
                    SequenceRange[] rangeArray = new SequenceRange[sortedRanges.Length + 1];
                    Array.Copy(sortedRanges, rangeArray, sortedRanges.Length);
                    rangeArray[sortedRanges.Length] = range;
                    return GeneralCreate(rangeArray);
                }
            }
            if (sortedRanges.Length == 1)
            {
                if (range.Upper == sortedRanges[0].Lower)
                {
                    num2 = 0;
                }
                else if (range.Upper < sortedRanges[0].Lower)
                {
                    num2 = -1;
                }
                else
                {
                    num2 = -2;
                }
            }
            else
            {
                num2 = Array.BinarySearch<SequenceRange>(sortedRanges, new SequenceRange(range.Upper), lowerComparer);
            }
            if (num2 < 0)
            {
                num2 = ~num2;
                if (num2 > 0)
                {
                    if ((num2 == sortedRanges.Length) || (sortedRanges[num2].Lower != (range.Upper + 1L)))
                    {
                        num2--;
                    }
                }
                else if (sortedRanges[0].Lower > (range.Upper + 1L))
                {
                    SequenceRange[] rangeArray2 = new SequenceRange[sortedRanges.Length + 1];
                    Array.Copy(sortedRanges, 0, rangeArray2, 1, sortedRanges.Length);
                    rangeArray2[0] = range;
                    return GeneralCreate(rangeArray2);
                }
            }
            long lower = (range.Lower < sortedRanges[num].Lower) ? range.Lower : sortedRanges[num].Lower;
            long upper = (range.Upper > sortedRanges[num2].Upper) ? range.Upper : sortedRanges[num2].Upper;
            int num5 = (num2 - num) + 1;
            int num6 = (sortedRanges.Length - num5) + 1;
            if (num6 == 1)
            {
                return new SingleItemRangeCollection(lower, upper);
            }
            SequenceRange[] destinationArray = new SequenceRange[num6];
            Array.Copy(sortedRanges, destinationArray, num);
            destinationArray[num] = new SequenceRange(lower, upper);
            Array.Copy(sortedRanges, (int) (num2 + 1), destinationArray, (int) (num + 1), (int) ((sortedRanges.Length - num2) - 1));
            return GeneralCreate(destinationArray);
        }

        public abstract SequenceRangeCollection MergeWith(long number);
        public abstract SequenceRangeCollection MergeWith(SequenceRange range);
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.Count; i++)
            {
                SequenceRange range = this[i];
                if (i > 0)
                {
                    builder.Append(',');
                }
                builder.Append(range.Lower);
                builder.Append('-');
                builder.Append(range.Upper);
            }
            return builder.ToString();
        }

        public abstract int Count { get; }

        public static SequenceRangeCollection Empty
        {
            get
            {
                return empty;
            }
        }

        public abstract SequenceRange this[int index] { get; }

        private class EmptyRangeCollection : SequenceRangeCollection
        {
            public override bool Contains(long number)
            {
                return false;
            }

            public override SequenceRangeCollection MergeWith(long number)
            {
                return new SequenceRangeCollection.SingleItemRangeCollection(number, number);
            }

            public override SequenceRangeCollection MergeWith(SequenceRange range)
            {
                return new SequenceRangeCollection.SingleItemRangeCollection(range);
            }

            public override int Count
            {
                get
                {
                    return 0;
                }
            }

            public override SequenceRange this[int index]
            {
                get
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index"));
                }
            }
        }

        private class LowerComparer : IComparer<SequenceRange>
        {
            public int Compare(SequenceRange x, SequenceRange y)
            {
                if (x.Lower < y.Lower)
                {
                    return -1;
                }
                if (x.Lower > y.Lower)
                {
                    return 1;
                }
                return 0;
            }
        }

        private class MultiItemRangeCollection : SequenceRangeCollection
        {
            private SequenceRange[] ranges;

            public MultiItemRangeCollection(SequenceRange[] sortedRanges)
            {
                this.ranges = sortedRanges;
            }

            public override bool Contains(long number)
            {
                if (this.ranges.Length == 0)
                {
                    return false;
                }
                if (this.ranges.Length == 1)
                {
                    return this.ranges[0].Contains(number);
                }
                SequenceRange range = new SequenceRange(number);
                int num = Array.BinarySearch<SequenceRange>(this.ranges, range, SequenceRangeCollection.lowerComparer);
                if (num >= 0)
                {
                    return true;
                }
                num = ~num;
                if (num == 0)
                {
                    return false;
                }
                return (this.ranges[num - 1].Upper >= number);
            }

            public override SequenceRangeCollection MergeWith(long number)
            {
                return this.MergeWith(new SequenceRange(number));
            }

            public override SequenceRangeCollection MergeWith(SequenceRange newRange)
            {
                return SequenceRangeCollection.GeneralMerge(this.ranges, newRange);
            }

            public override int Count
            {
                get
                {
                    return this.ranges.Length;
                }
            }

            public override SequenceRange this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.ranges.Length))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", index, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, this.ranges.Length - 1 })));
                    }
                    return this.ranges[index];
                }
            }
        }

        private class SingleItemRangeCollection : SequenceRangeCollection
        {
            private SequenceRange range;

            public SingleItemRangeCollection(SequenceRange range)
            {
                this.range = range;
            }

            public SingleItemRangeCollection(long lower, long upper)
            {
                this.range = new SequenceRange(lower, upper);
            }

            public override bool Contains(long number)
            {
                return this.range.Contains(number);
            }

            public override SequenceRangeCollection MergeWith(long number)
            {
                if (number == (this.range.Upper + 1L))
                {
                    return new SequenceRangeCollection.SingleItemRangeCollection(this.range.Lower, number);
                }
                return this.MergeWith(new SequenceRange(number));
            }

            public override SequenceRangeCollection MergeWith(SequenceRange newRange)
            {
                if (newRange.Lower == (this.range.Upper + 1L))
                {
                    return new SequenceRangeCollection.SingleItemRangeCollection(this.range.Lower, newRange.Upper);
                }
                if (this.range.Contains(newRange))
                {
                    return this;
                }
                if (newRange.Contains(this.range))
                {
                    return new SequenceRangeCollection.SingleItemRangeCollection(newRange);
                }
                if (newRange.Upper == (this.range.Lower - 1L))
                {
                    return new SequenceRangeCollection.SingleItemRangeCollection(newRange.Lower, this.range.Upper);
                }
                SequenceRange[] sortedRanges = new SequenceRange[] { this.range };
                return SequenceRangeCollection.GeneralMerge(sortedRanges, newRange);
            }

            public override int Count
            {
                get
                {
                    return 1;
                }
            }

            public override SequenceRange this[int index]
            {
                get
                {
                    if (index != 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index"));
                    }
                    return this.range;
                }
            }
        }

        private class UpperComparer : IComparer<SequenceRange>
        {
            public int Compare(SequenceRange x, SequenceRange y)
            {
                if (x.Upper < y.Upper)
                {
                    return -1;
                }
                if (x.Upper > y.Upper)
                {
                    return 1;
                }
                return 0;
            }
        }
    }
}

