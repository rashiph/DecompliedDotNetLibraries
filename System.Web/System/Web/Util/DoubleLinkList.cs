namespace System.Web.Util
{
    using System;

    internal class DoubleLinkList : DoubleLink
    {
        internal DoubleLinkList()
        {
        }

        internal DoubleLinkListEnumerator GetEnumerator()
        {
            return new DoubleLinkListEnumerator(this);
        }

        internal virtual void InsertHead(DoubleLink entry)
        {
            entry.InsertAfter(this);
        }

        internal virtual void InsertTail(DoubleLink entry)
        {
            entry.InsertBefore(this);
        }

        internal bool IsEmpty()
        {
            return (base._next == this);
        }

        internal int Length
        {
            get
            {
                int num = 0;
                DoubleLinkListEnumerator enumerator = this.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    num++;
                }
                return num;
            }
        }
    }
}

