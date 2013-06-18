namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal class TrieBranchIndex : QueryBranchIndex
    {
        private int count = 0;
        private Trie trie = new Trie();

        internal TrieBranchIndex()
        {
        }

        internal override void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            this.trie.Root.CollectXPathFilters(filters);
        }

        private void Match(int valIndex, string segment, QueryBranchResultSet results)
        {
            TrieTraverser traverser = new TrieTraverser(this.trie.Root, segment);
            while (traverser.MoveNext())
            {
                object data = traverser.Segment.Data;
                if (data != null)
                {
                    results.Add((QueryBranch) data, valIndex);
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
                    this.Match(valIndex, sequence.Items[i].StringValue(), results);
                }
            }
            else
            {
                this.Match(valIndex, val.String, results);
            }
        }

        internal override void Remove(object key)
        {
            this.trie.Remove((string) key);
            this.count--;
        }

        internal override void Trim()
        {
            this.trie.Trim();
        }

        internal override int Count
        {
            get
            {
                return this.count;
            }
        }

        internal override QueryBranch this[object key]
        {
            get
            {
                TrieSegment segment = this.trie[(string) key];
                if (segment != null)
                {
                    return segment.Data;
                }
                return null;
            }
            set
            {
                TrieSegment segment = this.trie.Add((string) key);
                this.count++;
                segment.Data = value;
            }
        }
    }
}

