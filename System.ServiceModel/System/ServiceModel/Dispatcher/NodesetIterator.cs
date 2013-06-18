namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NodesetIterator
    {
        private int index;
        private int indexStart;
        private NodeSequence sequence;
        private NodeSequenceItem[] items;
        internal NodesetIterator(NodeSequence sequence)
        {
            this.sequence = sequence;
            this.items = sequence.Items;
            this.index = -1;
            this.indexStart = -1;
        }

        internal int Index
        {
            get
            {
                return this.index;
            }
        }
        internal bool NextItem()
        {
            if (-1 == this.index)
            {
                this.index = this.indexStart;
                return true;
            }
            if (this.items[this.index].Last)
            {
                return false;
            }
            this.index++;
            return true;
        }

        internal bool NextNodeset()
        {
            this.indexStart = this.index + 1;
            this.index = -1;
            return (this.indexStart < this.sequence.Count);
        }
    }
}

