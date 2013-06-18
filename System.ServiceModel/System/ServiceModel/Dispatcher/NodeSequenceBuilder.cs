namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NodeSequenceBuilder
    {
        private ProcessingContext context;
        private NodeSequence sequence;
        internal NodeSequenceBuilder(ProcessingContext context, NodeSequence sequence)
        {
            this.context = context;
            this.sequence = sequence;
        }

        internal NodeSequenceBuilder(ProcessingContext context) : this(context, null)
        {
        }

        internal NodeSequence Sequence
        {
            get
            {
                if (this.sequence == null)
                {
                    return NodeSequence.Empty;
                }
                return this.sequence;
            }
            set
            {
                this.sequence = value;
            }
        }
        internal void Add(ref NodeSequenceItem item)
        {
            if (this.sequence == null)
            {
                this.sequence = this.context.CreateSequence();
                this.sequence.StartNodeset();
            }
            this.sequence.Add(ref item);
        }

        internal void EndNodeset()
        {
            if (this.sequence != null)
            {
                this.sequence.StopNodeset();
            }
        }

        internal void StartNodeset()
        {
            if (this.sequence != null)
            {
                this.sequence.StartNodeset();
            }
        }
    }
}

