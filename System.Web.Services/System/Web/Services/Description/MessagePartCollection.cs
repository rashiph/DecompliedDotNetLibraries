namespace System.Web.Services.Description
{
    using System;
    using System.Reflection;

    public sealed class MessagePartCollection : ServiceDescriptionBaseCollection
    {
        internal MessagePartCollection(Message message) : base(message)
        {
        }

        public int Add(MessagePart messagePart)
        {
            return base.List.Add(messagePart);
        }

        public bool Contains(MessagePart messagePart)
        {
            return base.List.Contains(messagePart);
        }

        public void CopyTo(MessagePart[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        protected override string GetKey(object value)
        {
            return ((MessagePart) value).Name;
        }

        public int IndexOf(MessagePart messagePart)
        {
            return base.List.IndexOf(messagePart);
        }

        public void Insert(int index, MessagePart messagePart)
        {
            base.List.Insert(index, messagePart);
        }

        public void Remove(MessagePart messagePart)
        {
            base.List.Remove(messagePart);
        }

        protected override void SetParent(object value, object parent)
        {
            ((MessagePart) value).SetParent((Message) parent);
        }

        public MessagePart this[int index]
        {
            get
            {
                return (MessagePart) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        public MessagePart this[string name]
        {
            get
            {
                return (MessagePart) this.Table[name];
            }
        }
    }
}

