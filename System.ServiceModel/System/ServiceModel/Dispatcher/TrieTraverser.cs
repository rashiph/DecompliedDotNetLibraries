namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TrieTraverser
    {
        private int length;
        private int offset;
        private string prefix;
        private TrieSegment rootSegment;
        private TrieSegment segment;
        private int segmentIndex;
        internal TrieTraverser(TrieSegment root, string prefix)
        {
            this.prefix = prefix;
            this.rootSegment = root;
            this.segment = null;
            this.segmentIndex = -1;
            this.offset = 0;
            this.length = prefix.Length;
        }

        internal int Length
        {
            get
            {
                return this.length;
            }
        }
        internal int Offset
        {
            get
            {
                return this.offset;
            }
        }
        internal TrieSegment Segment
        {
            get
            {
                return this.segment;
            }
            set
            {
                this.segment = value;
            }
        }
        internal int SegmentIndex
        {
            get
            {
                return this.segmentIndex;
            }
        }
        internal bool MoveNext()
        {
            if (this.segment != null)
            {
                int length = this.segment.Length;
                this.offset += length;
                this.length -= length;
                if (this.length > 0)
                {
                    this.segmentIndex = this.segment.GetChildPosition(this.prefix, this.offset, this.length);
                    if (this.segmentIndex > -1)
                    {
                        this.segment = this.segment.GetChild(this.segmentIndex);
                        return true;
                    }
                }
                else
                {
                    this.segmentIndex = -1;
                }
                this.segment = null;
            }
            else if (this.rootSegment != null)
            {
                this.segment = this.rootSegment;
                this.rootSegment = null;
                return true;
            }
            return false;
        }

        internal bool MoveNextByFirstChar()
        {
            if (this.segment != null)
            {
                int length = this.segment.Length;
                this.offset += length;
                this.length -= length;
                if (this.length > 0)
                {
                    this.segmentIndex = this.segment.GetChildPosition(this.prefix[this.offset]);
                    if (this.segmentIndex > -1)
                    {
                        this.segment = this.segment.GetChild(this.segmentIndex);
                        return true;
                    }
                }
                else
                {
                    this.segmentIndex = -1;
                }
                this.segment = null;
            }
            else if (this.rootSegment != null)
            {
                this.segment = this.rootSegment;
                this.rootSegment = null;
                return true;
            }
            return false;
        }
    }
}

