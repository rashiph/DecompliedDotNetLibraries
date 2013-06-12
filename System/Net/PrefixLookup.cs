namespace System.Net
{
    using System;
    using System.Collections.Generic;

    internal class PrefixLookup
    {
        private int capacity = 100;
        private const int defaultCapacity = 100;
        private LinkedList<PrefixValuePair> lruList = new LinkedList<PrefixValuePair>();

        internal void Add(string prefix, object value)
        {
            if (((this.capacity != 0) && (prefix != null)) && ((prefix.Length != 0) && (value != null)))
            {
                lock (this.lruList)
                {
                    if ((this.lruList.First != null) && this.lruList.First.Value.prefix.Equals(prefix))
                    {
                        this.lruList.First.Value.value = value;
                    }
                    else
                    {
                        this.lruList.AddFirst(new PrefixValuePair(prefix, value));
                        while (this.lruList.Count > this.capacity)
                        {
                            this.lruList.RemoveLast();
                        }
                    }
                }
            }
        }

        internal object Lookup(string lookupKey)
        {
            if (((lookupKey == null) || (lookupKey.Length == 0)) || (this.lruList.Count == 0))
            {
                return null;
            }
            LinkedListNode<PrefixValuePair> node = null;
            lock (this.lruList)
            {
                int length = 0;
                for (LinkedListNode<PrefixValuePair> node2 = this.lruList.First; node2 != null; node2 = node2.Next)
                {
                    string prefix = node2.Value.prefix;
                    if ((prefix.Length > length) && lookupKey.StartsWith(prefix))
                    {
                        length = prefix.Length;
                        node = node2;
                        if (length == lookupKey.Length)
                        {
                            break;
                        }
                    }
                }
                if ((node != null) && (node != this.lruList.First))
                {
                    this.lruList.Remove(node);
                    this.lruList.AddFirst(node);
                }
            }
            if (node == null)
            {
                return null;
            }
            return node.Value.value;
        }

        private class PrefixValuePair
        {
            public string prefix;
            public object value;

            public PrefixValuePair(string pre, object val)
            {
                this.prefix = pre;
                this.value = val;
            }
        }
    }
}

