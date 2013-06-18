namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Threading;
    using System.Xml.XPath;

    internal class SafeNodeSequenceIterator : NodeSequenceIterator, IDisposable
    {
        private ProcessingContext context;
        private int disposed;
        private NodeSequence seq;

        public SafeNodeSequenceIterator(NodeSequence seq, ProcessingContext context) : base(seq)
        {
            this.context = context;
            this.seq = seq;
            Interlocked.Increment(ref this.seq.refCount);
            this.context.Processor.AddRef();
        }

        public override XPathNodeIterator Clone()
        {
            return new SafeNodeSequenceIterator(this.seq, this.context);
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref this.disposed, 1, 0) == 0)
            {
                QueryProcessor processor = this.context.Processor;
                this.context.ReleaseSequence(this.seq);
                this.context.Processor.Matcher.ReleaseProcessor(processor);
            }
        }
    }
}

