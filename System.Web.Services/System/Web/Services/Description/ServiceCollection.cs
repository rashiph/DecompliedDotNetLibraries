namespace System.Web.Services.Description
{
    using System;
    using System.Reflection;

    public sealed class ServiceCollection : ServiceDescriptionBaseCollection
    {
        internal ServiceCollection(ServiceDescription serviceDescription) : base(serviceDescription)
        {
        }

        public int Add(Service service)
        {
            return base.List.Add(service);
        }

        public bool Contains(Service service)
        {
            return base.List.Contains(service);
        }

        public void CopyTo(Service[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        protected override string GetKey(object value)
        {
            return ((Service) value).Name;
        }

        public int IndexOf(Service service)
        {
            return base.List.IndexOf(service);
        }

        public void Insert(int index, Service service)
        {
            base.List.Insert(index, service);
        }

        public void Remove(Service service)
        {
            base.List.Remove(service);
        }

        protected override void SetParent(object value, object parent)
        {
            ((Service) value).SetParent((ServiceDescription) parent);
        }

        public Service this[int index]
        {
            get
            {
                return (Service) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        public Service this[string name]
        {
            get
            {
                return (Service) this.Table[name];
            }
        }
    }
}

