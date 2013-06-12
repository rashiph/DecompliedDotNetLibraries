namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal abstract class ResetableIterator : XPathNodeIterator
    {
        public ResetableIterator()
        {
            base.count = -1;
        }

        protected ResetableIterator(ResetableIterator other)
        {
            base.count = other.count;
        }

        public virtual bool MoveToPosition(int pos)
        {
            this.Reset();
            for (int i = this.CurrentPosition; i < pos; i++)
            {
                if (!this.MoveNext())
                {
                    return false;
                }
            }
            return true;
        }

        public abstract void Reset();
        protected void ResetCount()
        {
            base.count = -1;
        }

        public abstract override int CurrentPosition { get; }
    }
}

