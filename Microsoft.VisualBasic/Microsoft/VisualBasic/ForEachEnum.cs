namespace Microsoft.VisualBasic
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class ForEachEnum : IEnumerator, IDisposable
    {
        private bool mAtBeginning;
        private Collection mCollectionObject;
        private Collection.Node mCurrent;
        private bool mDisposed = false;
        private Collection.Node mNext;
        internal WeakReference WeakRef;

        public ForEachEnum(Collection coll)
        {
            this.mCollectionObject = coll;
            this.Reset();
        }

        public void Adjust(Collection.Node Node, AdjustIndexType Type)
        {
            if ((Node != null) && !this.mDisposed)
            {
                switch (Type)
                {
                    case AdjustIndexType.Insert:
                        if ((this.mCurrent != null) && (Node == this.mCurrent.m_Next))
                        {
                            this.mNext = Node;
                        }
                        break;

                    case AdjustIndexType.Remove:
                        if ((Node != this.mCurrent) && (Node == this.mNext))
                        {
                            this.mNext = this.mNext.m_Next;
                        }
                        break;
                }
            }
        }

        internal void AdjustOnListCleared()
        {
            this.mNext = null;
        }

        private void Dispose()
        {
            if (!this.mDisposed)
            {
                this.mCollectionObject.RemoveIterator(this.WeakRef);
                this.mDisposed = true;
            }
            this.mCurrent = null;
            this.mNext = null;
        }

        public bool MoveNext()
        {
            if (!this.mDisposed)
            {
                if (this.mAtBeginning)
                {
                    this.mAtBeginning = false;
                    this.mNext = this.mCollectionObject.GetFirstListNode();
                }
                if (this.mNext == null)
                {
                    this.Dispose();
                    return false;
                }
                this.mCurrent = this.mNext;
                if (this.mCurrent != null)
                {
                    this.mNext = this.mCurrent.m_Next;
                    return true;
                }
                this.Dispose();
            }
            return false;
        }

        public void Reset()
        {
            if (this.mDisposed)
            {
                this.mCollectionObject.AddIterator(this.WeakRef);
                this.mDisposed = false;
            }
            this.mCurrent = null;
            this.mNext = null;
            this.mAtBeginning = true;
        }

        public object Current
        {
            get
            {
                if (this.mCurrent == null)
                {
                    return null;
                }
                return this.mCurrent.m_Value;
            }
        }

        public object System.Collections.IEnumerator.Current
        {
            get
            {
                if (this.mCurrent == null)
                {
                    return null;
                }
                return this.mCurrent.m_Value;
            }
        }

        internal enum AdjustIndexType
        {
            Insert,
            Remove
        }
    }
}

