namespace System.Web.Util
{
    using System;
    using System.Collections;

    internal class DoubleLinkListEnumerator : IEnumerator
    {
        private DoubleLink _current;
        private DoubleLinkList _list;

        internal DoubleLinkListEnumerator(DoubleLinkList list)
        {
            this._list = list;
            this._current = list;
        }

        internal DoubleLink GetDoubleLink()
        {
            return this._current;
        }

        public bool MoveNext()
        {
            if (this._current.Next == this._list)
            {
                this._current = null;
                return false;
            }
            this._current = this._current.Next;
            return true;
        }

        public void Reset()
        {
            this._current = this._list;
        }

        public object Current
        {
            get
            {
                if ((this._current == null) || (this._current == this._list))
                {
                    throw new InvalidOperationException();
                }
                return this._current.Item;
            }
        }
    }
}

