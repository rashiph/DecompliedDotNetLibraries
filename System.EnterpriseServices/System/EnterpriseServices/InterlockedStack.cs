namespace System.EnterpriseServices
{
    using System;
    using System.Threading;

    internal class InterlockedStack
    {
        private int _count;
        private object _head = null;

        public object Pop()
        {
            object obj2;
            object next;
            do
            {
                obj2 = this._head;
                if (obj2 == null)
                {
                    return null;
                }
                next = ((Node) obj2).Next;
            }
            while (Interlocked.CompareExchange(ref this._head, next, obj2) != obj2);
            Interlocked.Decrement(ref this._count);
            return ((Node) obj2).Object;
        }

        public void Push(object o)
        {
            object obj2;
            Node node = new Node(o);
            do
            {
                obj2 = this._head;
                node.Next = (Node) obj2;
            }
            while (Interlocked.CompareExchange(ref this._head, node, obj2) != obj2);
            Interlocked.Increment(ref this._count);
        }

        public int Count
        {
            get
            {
                return this._count;
            }
        }

        private class Node
        {
            public InterlockedStack.Node Next;
            public object Object;

            public Node(object o)
            {
                this.Object = o;
                this.Next = null;
            }
        }
    }
}

