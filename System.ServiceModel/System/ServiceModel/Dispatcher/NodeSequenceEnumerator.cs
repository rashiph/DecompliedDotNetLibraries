namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.ServiceModel;

    internal class NodeSequenceEnumerator : IEnumerator
    {
        private NodeSequenceIterator iter;

        internal NodeSequenceEnumerator(NodeSequenceIterator iter)
        {
            this.iter = new NodeSequenceIterator(iter);
            this.Reset();
        }

        public bool MoveNext()
        {
            return this.iter.MoveNext();
        }

        public void Reset()
        {
            this.iter.Reset();
        }

        public object Current
        {
            get
            {
                if (this.iter.CurrentPosition == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("QueryBeforeNodes")));
                }
                if (this.iter.CurrentPosition > this.iter.Count)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("QueryAfterNodes")));
                }
                return this.iter.Current;
            }
        }
    }
}

