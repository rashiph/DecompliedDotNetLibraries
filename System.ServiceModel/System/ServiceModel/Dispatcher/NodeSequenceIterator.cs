namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.ServiceModel;
    using System.Xml.XPath;

    internal class NodeSequenceIterator : XPathNodeIterator
    {
        private NodeSequenceIterator data;
        private int index;
        private SeekableXPathNavigator nav;
        private NodeSequence seq;

        internal NodeSequenceIterator(NodeSequence seq)
        {
            this.data = this;
            this.seq = seq;
        }

        internal NodeSequenceIterator(NodeSequenceIterator iter)
        {
            this.data = iter.data;
            this.index = iter.index;
        }

        internal void Clear()
        {
            this.data.seq = null;
            this.nav = null;
        }

        public override XPathNodeIterator Clone()
        {
            return new NodeSequenceIterator(this);
        }

        public override IEnumerator GetEnumerator()
        {
            return new NodeSequenceEnumerator(this);
        }

        public override bool MoveNext()
        {
            if (this.data.seq == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("QueryIteratorOutOfScope")));
            }
            if (this.index < this.data.seq.Count)
            {
                if (this.nav == null)
                {
                    NodeSequenceItem item = this.data.seq[this.index];
                    this.nav = (SeekableXPathNavigator) item.GetNavigator().Clone();
                }
                else
                {
                    this.nav.CurrentPosition = this.data.seq[this.index].GetNavigatorPosition();
                }
                this.index++;
                return true;
            }
            this.index++;
            this.nav = null;
            return false;
        }

        public void Reset()
        {
            this.nav = null;
            this.index = 0;
        }

        public override int Count
        {
            get
            {
                return this.data.seq.Count;
            }
        }

        public override XPathNavigator Current
        {
            get
            {
                if (this.index == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.Unexpected, System.ServiceModel.SR.GetString("QueryContextNotSupportedInSequences")));
                }
                if (this.index > this.data.seq.Count)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("QueryAfterNodes")));
                }
                return this.nav;
            }
        }

        public override int CurrentPosition
        {
            get
            {
                return this.index;
            }
        }
    }
}

