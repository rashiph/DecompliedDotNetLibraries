namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;

    internal class TrieSegmentComparer : IComparer<TrieSegment>
    {
        public int Compare(TrieSegment t1, TrieSegment t2)
        {
            return (t1.FirstChar - t2.FirstChar);
        }

        public bool Equals(TrieSegment t1, TrieSegment t2)
        {
            return (t1.FirstChar == t2.FirstChar);
        }

        public int GetHashCode(TrieSegment t)
        {
            return t.GetHashCode();
        }
    }
}

