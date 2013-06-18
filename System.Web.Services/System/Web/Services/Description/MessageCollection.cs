namespace System.Web.Services.Description
{
    using System;
    using System.Reflection;

    public sealed class MessageCollection : ServiceDescriptionBaseCollection
    {
        internal MessageCollection(ServiceDescription serviceDescription) : base(serviceDescription)
        {
        }

        public int Add(Message message)
        {
            return base.List.Add(message);
        }

        public bool Contains(Message message)
        {
            return base.List.Contains(message);
        }

        public void CopyTo(Message[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        protected override string GetKey(object value)
        {
            return ((Message) value).Name;
        }

        public int IndexOf(Message message)
        {
            return base.List.IndexOf(message);
        }

        public void Insert(int index, Message message)
        {
            base.List.Insert(index, message);
        }

        public void Remove(Message message)
        {
            base.List.Remove(message);
        }

        protected override void SetParent(object value, object parent)
        {
            ((Message) value).SetParent((ServiceDescription) parent);
        }

        public Message this[int index]
        {
            get
            {
                return (Message) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        public Message this[string name]
        {
            get
            {
                return (Message) this.Table[name];
            }
        }
    }
}

