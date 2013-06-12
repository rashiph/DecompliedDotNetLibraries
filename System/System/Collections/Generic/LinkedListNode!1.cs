namespace System.Collections.Generic
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public sealed class LinkedListNode<T>
    {
        internal T item;
        internal LinkedList<T> list;
        internal LinkedListNode<T> next;
        internal LinkedListNode<T> prev;

        public LinkedListNode(T value)
        {
            this.item = value;
        }

        internal LinkedListNode(LinkedList<T> list, T value)
        {
            this.list = list;
            this.item = value;
        }

        internal void Invalidate()
        {
            this.list = null;
            this.next = null;
            this.prev = null;
        }

        public LinkedList<T> List
        {
            get
            {
                return this.list;
            }
        }

        public LinkedListNode<T> Next
        {
            get
            {
                if ((this.next != null) && (this.next != this.list.head))
                {
                    return this.next;
                }
                return null;
            }
        }

        public LinkedListNode<T> Previous
        {
            get
            {
                if ((this.prev != null) && (this != this.list.head))
                {
                    return this.prev;
                }
                return null;
            }
        }

        public T Value
        {
            get
            {
                return this.item;
            }
            set
            {
                this.item = value;
            }
        }
    }
}

