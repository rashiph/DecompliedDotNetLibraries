namespace System.ComponentModel
{
    using System;
    using System.Reflection;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public sealed class EventHandlerList : IDisposable
    {
        private ListEntry head;
        private Component parent;

        public EventHandlerList()
        {
        }

        internal EventHandlerList(Component parent)
        {
            this.parent = parent;
        }

        public void AddHandler(object key, Delegate value)
        {
            ListEntry entry = this.Find(key);
            if (entry != null)
            {
                entry.handler = Delegate.Combine(entry.handler, value);
            }
            else
            {
                this.head = new ListEntry(key, value, this.head);
            }
        }

        public void AddHandlers(EventHandlerList listToAddFrom)
        {
            for (ListEntry entry = listToAddFrom.head; entry != null; entry = entry.next)
            {
                this.AddHandler(entry.key, entry.handler);
            }
        }

        public void Dispose()
        {
            this.head = null;
        }

        private ListEntry Find(object key)
        {
            ListEntry head = this.head;
            while (head != null)
            {
                if (head.key == key)
                {
                    return head;
                }
                head = head.next;
            }
            return head;
        }

        public void RemoveHandler(object key, Delegate value)
        {
            ListEntry entry = this.Find(key);
            if (entry != null)
            {
                entry.handler = Delegate.Remove(entry.handler, value);
            }
        }

        public Delegate this[object key]
        {
            get
            {
                ListEntry entry = null;
                if ((this.parent == null) || this.parent.CanRaiseEventsInternal)
                {
                    entry = this.Find(key);
                }
                if (entry != null)
                {
                    return entry.handler;
                }
                return null;
            }
            set
            {
                ListEntry entry = this.Find(key);
                if (entry != null)
                {
                    entry.handler = value;
                }
                else
                {
                    this.head = new ListEntry(key, value, this.head);
                }
            }
        }

        private sealed class ListEntry
        {
            internal Delegate handler;
            internal object key;
            internal EventHandlerList.ListEntry next;

            public ListEntry(object key, Delegate handler, EventHandlerList.ListEntry next)
            {
                this.next = next;
                this.key = key;
                this.handler = handler;
            }
        }
    }
}

