namespace System.Diagnostics
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Security.Permissions;

    [ConfigurationCollection(typeof(ListenerElement))]
    internal class ListenerElementsCollection : ConfigurationElementCollection
    {
        protected override void BaseAdd(ConfigurationElement element)
        {
            ListenerElement element2 = element as ListenerElement;
            if (element2.Name.Equals("Default") && element2.TypeName.Equals(typeof(DefaultTraceListener).FullName))
            {
                base.BaseAdd(element2, false);
            }
            else
            {
                base.BaseAdd(element2, this.ThrowOnDuplicate);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ListenerElement(true);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ListenerElement) element).Name;
        }

        public TraceListenerCollection GetRuntimeObject()
        {
            TraceListenerCollection listeners = new TraceListenerCollection();
            bool flag = false;
            foreach (ListenerElement element in this)
            {
                if (!flag && !element._isAddedByDefault)
                {
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                    flag = true;
                }
                listeners.Add(element.GetRuntimeObject());
            }
            return listeners;
        }

        protected override void InitializeDefault()
        {
            this.InitializeDefaultInternal();
        }

        internal void InitializeDefaultInternal()
        {
            ListenerElement element = new ListenerElement(false) {
                Name = "Default",
                TypeName = typeof(DefaultTraceListener).FullName,
                _isAddedByDefault = true
            };
            this.BaseAdd(element);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        public ListenerElement this[string name]
        {
            get
            {
                return (ListenerElement) base.BaseGet(name);
            }
        }
    }
}

