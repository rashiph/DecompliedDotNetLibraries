namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Reflection;

    internal class Trie
    {
        private bool hasDescendants = false;
        private TrieSegment root;

        internal Trie()
        {
        }

        internal TrieSegment Add(string newPrefix)
        {
            TrieSegment segment;
            if (newPrefix.Length <= 0)
            {
                return this.Root;
            }
            this.EnsureRoot();
            TrieTraverser traverser = new TrieTraverser(this.root, newPrefix);
        Label_0024:
            segment = traverser.Segment;
            if (traverser.MoveNextByFirstChar())
            {
                int num;
                if ((segment != null) && (-1 != (num = traverser.Segment.FindDivergence(newPrefix, traverser.Offset, traverser.Length))))
                {
                    traverser.Segment = segment.SplitChild(traverser.SegmentIndex, num);
                }
                goto Label_0024;
            }
            if (traverser.Length > 0)
            {
                traverser.Segment = segment.AddChild(new TrieSegment(newPrefix, traverser.Offset, traverser.Length));
                goto Label_0024;
            }
            this.hasDescendants = true;
            return segment;
        }

        private void EnsureRoot()
        {
            if (this.root == null)
            {
                this.root = new TrieSegment();
            }
        }

        private TrieSegment Find(string prefix)
        {
            if (prefix.Length == 0)
            {
                return this.Root;
            }
            if (!this.HasDescendants)
            {
                return null;
            }
            TrieTraverser traverser = new TrieTraverser(this.root, prefix);
            TrieSegment segment = null;
            while (traverser.MoveNext())
            {
                segment = traverser.Segment;
            }
            if (traverser.Length > 0)
            {
                return null;
            }
            return segment;
        }

        private void PruneRoot()
        {
            if ((this.root != null) && this.root.CanPrune)
            {
                this.root = null;
            }
        }

        internal void Remove(string segment)
        {
            TrieSegment segment2 = this[segment];
            if (segment2 != null)
            {
                if (segment2.HasChildren)
                {
                    segment2.Data = null;
                }
                else if (segment2 == this.root)
                {
                    this.root = null;
                    this.hasDescendants = false;
                }
                else
                {
                    segment2.Remove();
                    this.PruneRoot();
                }
            }
        }

        internal void Trim()
        {
            this.root.Trim();
        }

        private bool HasDescendants
        {
            get
            {
                return this.hasDescendants;
            }
        }

        internal TrieSegment this[string prefix]
        {
            get
            {
                return this.Find(prefix);
            }
        }

        internal TrieSegment Root
        {
            get
            {
                this.EnsureRoot();
                return this.root;
            }
        }
    }
}

