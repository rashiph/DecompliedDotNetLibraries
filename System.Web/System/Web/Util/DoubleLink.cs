namespace System.Web.Util
{
    using System;

    internal class DoubleLink
    {
        internal DoubleLink _next;
        internal DoubleLink _prev;
        internal object Item;

        internal DoubleLink()
        {
            this._next = this._prev = this;
        }

        internal DoubleLink(object item) : this()
        {
            this.Item = item;
        }

        internal void InsertAfter(DoubleLink after)
        {
            this._prev = after;
            this._next = after._next;
            after._next = this;
            this._next._prev = this;
        }

        internal void InsertBefore(DoubleLink before)
        {
            this._prev = before._prev;
            this._next = before;
            before._prev = this;
            this._prev._next = this;
        }

        internal void Remove()
        {
            this._prev._next = this._next;
            this._next._prev = this._prev;
            this._next = this._prev = this;
        }

        internal DoubleLink Next
        {
            get
            {
                return this._next;
            }
        }
    }
}

