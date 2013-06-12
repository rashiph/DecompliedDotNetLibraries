namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections.Generic;

    internal sealed class ClonableStack<T> : List<T>
    {
        public ClonableStack()
        {
        }

        public ClonableStack(int capacity) : base(capacity)
        {
        }

        private ClonableStack(IEnumerable<T> collection) : base(collection)
        {
        }

        public ClonableStack<T> Clone()
        {
            return new ClonableStack<T>(this);
        }

        public T Peek()
        {
            return base[base.Count - 1];
        }

        public T Pop()
        {
            int index = base.Count - 1;
            T local = base[index];
            base.RemoveAt(index);
            return local;
        }

        public void Push(T value)
        {
            base.Add(value);
        }
    }
}

