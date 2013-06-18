namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class TrieSegmentKeyComparer : IItemComparer<char, TrieSegment>
    {
        public int Compare(char c, TrieSegment t)
        {
            return (c - t.FirstChar);
        }
    }
}

