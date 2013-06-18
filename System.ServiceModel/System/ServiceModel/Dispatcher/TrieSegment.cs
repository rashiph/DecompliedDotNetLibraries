namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class TrieSegment
    {
        private SortedBuffer<TrieSegment, TrieSegmentComparer> children;
        private QueryBranch data;
        private TrieSegment parent;
        private static readonly TrieSegmentComparer SegComparer = new TrieSegmentComparer();
        private static readonly TrieSegmentKeyComparer SegKeyComparer = new TrieSegmentKeyComparer();
        private char segmentFirstChar;
        private int segmentLength;
        private string segmentTail;

        internal TrieSegment() : this('\0')
        {
        }

        internal TrieSegment(char firstChar) : this(firstChar, string.Empty)
        {
        }

        internal TrieSegment(char firstChar, string segmentTail)
        {
            this.SetSegment(firstChar, segmentTail);
            this.children = new SortedBuffer<TrieSegment, TrieSegmentComparer>(SegComparer);
        }

        internal TrieSegment(string sourceSegment, int offset, int length)
        {
            this.SetSegmentString(sourceSegment, offset, length);
            this.children = new SortedBuffer<TrieSegment, TrieSegmentComparer>(SegComparer);
        }

        internal TrieSegment AddChild(TrieSegment segment)
        {
            this.children.Insert(segment);
            segment.parent = this;
            return segment;
        }

        internal void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            if (this.data != null)
            {
                this.data.Branch.CollectXPathFilters(filters);
            }
            for (int i = 0; i < this.children.Count; i++)
            {
                this.children[i].CollectXPathFilters(filters);
            }
        }

        internal int FindDivergence(string compareString, int offset, int length)
        {
            if (compareString[offset] != this.segmentFirstChar)
            {
                return 0;
            }
            length--;
            offset++;
            int num = (length <= this.segmentTail.Length) ? length : this.segmentTail.Length;
            int num2 = 0;
            for (int i = offset; num2 < num; i++)
            {
                if (compareString[i] != this.segmentTail[num2])
                {
                    return (num2 + 1);
                }
                num2++;
            }
            if (length < this.segmentTail.Length)
            {
                return (length + 1);
            }
            return -1;
        }

        internal TrieSegment GetChild(int index)
        {
            return this.children[index];
        }

        internal int GetChildPosition(char ch)
        {
            return this.children.IndexOfKey<char>(ch, SegKeyComparer);
        }

        internal int GetChildPosition(string matchString, int offset, int length)
        {
            if (this.HasChildren)
            {
                char key = matchString[offset];
                int num = length - 1;
                int indexA = offset + 1;
                int num3 = this.children.IndexOfKey<char>(key, SegKeyComparer);
                if (num3 >= 0)
                {
                    TrieSegment segment = this.children[num3];
                    if ((num >= segment.segmentTail.Length) && ((segment.segmentTail.Length == 0) || (string.CompareOrdinal(matchString, indexA, segment.segmentTail, 0, segment.segmentTail.Length) == 0)))
                    {
                        return num3;
                    }
                }
            }
            return -1;
        }

        internal int IndexOf(TrieSegment segment)
        {
            return this.children.IndexOf(segment);
        }

        internal void MergeChild(int childIndex)
        {
            TrieSegment old = this.children[childIndex];
            if (old.CanMerge)
            {
                TrieSegment replace = old.children[0];
                StringBuilder builder = new StringBuilder();
                builder.Append(old.segmentTail);
                builder.Append(replace.segmentFirstChar);
                builder.Append(replace.segmentTail);
                replace.SetSegment(old.segmentFirstChar, builder.ToString());
                replace.parent = this;
                this.children.Exchange(old, replace);
                old.parent = null;
            }
        }

        internal void MergeChild(TrieSegment segment)
        {
            int index = this.IndexOf(segment);
            if (index > -1)
            {
                this.MergeChild(index);
            }
        }

        internal void Remove()
        {
            if (this.parent != null)
            {
                this.parent.RemoveChild(this);
            }
        }

        private void RemoveChild(TrieSegment segment)
        {
            int index = this.IndexOf(segment);
            if (index >= 0)
            {
                this.RemoveChild(index, true);
            }
        }

        internal void RemoveChild(int childIndex, bool fixupTree)
        {
            TrieSegment segment = this.children[childIndex];
            segment.parent = null;
            this.children.RemoveAt(childIndex);
            if (this.children.Count == 0)
            {
                if (fixupTree && this.CanPrune)
                {
                    this.Remove();
                }
            }
            else if ((fixupTree && this.CanMerge) && (this.parent != null))
            {
                this.parent.MergeChild(this);
            }
        }

        private void SetSegment(char firstChar, string segmentTail)
        {
            this.segmentFirstChar = firstChar;
            this.segmentTail = segmentTail;
            this.segmentLength = (firstChar == '\0') ? 0 : (1 + segmentTail.Length);
        }

        private void SetSegmentString(string segmentString, int offset, int length)
        {
            this.segmentFirstChar = segmentString[offset];
            if (length > 1)
            {
                this.segmentTail = segmentString.Substring(offset + 1, length - 1);
            }
            else
            {
                this.segmentTail = string.Empty;
            }
            this.segmentLength = length;
        }

        private TrieSegment SplitAt(int charIndex)
        {
            TrieSegment segment;
            if (1 == charIndex)
            {
                segment = new TrieSegment(this.segmentFirstChar);
            }
            else
            {
                segment = new TrieSegment(this.segmentFirstChar, this.segmentTail.Substring(0, charIndex - 1));
            }
            charIndex--;
            this.SetSegmentString(this.segmentTail, charIndex, this.segmentTail.Length - charIndex);
            segment.AddChild(this);
            return segment;
        }

        internal TrieSegment SplitChild(int childIndex, int charIndex)
        {
            TrieSegment item = this.children[childIndex];
            this.children.Remove(item);
            TrieSegment segment2 = item.SplitAt(charIndex);
            this.children.Insert(segment2);
            segment2.parent = this;
            return segment2;
        }

        internal void Trim()
        {
            this.children.Trim();
            for (int i = 0; i < this.children.Count; i++)
            {
                this.children[i].Trim();
            }
        }

        internal bool CanMerge
        {
            get
            {
                return ((this.data == null) && (1 == this.children.Count));
            }
        }

        internal bool CanPrune
        {
            get
            {
                return ((this.data == null) && (0 == this.children.Count));
            }
        }

        internal QueryBranch Data
        {
            get
            {
                return this.data;
            }
            set
            {
                this.data = value;
            }
        }

        internal char FirstChar
        {
            get
            {
                return this.segmentFirstChar;
            }
        }

        internal bool HasChildren
        {
            get
            {
                return (this.children.Count > 0);
            }
        }

        internal int Length
        {
            get
            {
                return this.segmentLength;
            }
        }
    }
}

