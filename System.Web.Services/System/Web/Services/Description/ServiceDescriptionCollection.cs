namespace System.Web.Services.Description
{
    using System;
    using System.Reflection;
    using System.Web.Services;
    using System.Xml;

    public sealed class ServiceDescriptionCollection : ServiceDescriptionBaseCollection
    {
        public ServiceDescriptionCollection() : base(null)
        {
        }

        public int Add(ServiceDescription serviceDescription)
        {
            return base.List.Add(serviceDescription);
        }

        public bool Contains(ServiceDescription serviceDescription)
        {
            return base.List.Contains(serviceDescription);
        }

        public void CopyTo(ServiceDescription[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public Binding GetBinding(XmlQualifiedName name)
        {
            ServiceDescription serviceDescription = this.GetServiceDescription(name);
            Binding binding = null;
            while ((binding == null) && (serviceDescription != null))
            {
                binding = serviceDescription.Bindings[name.Name];
                serviceDescription = serviceDescription.Next;
            }
            if (binding == null)
            {
                throw this.ItemNotFound(name, "binding");
            }
            return binding;
        }

        protected override string GetKey(object value)
        {
            string targetNamespace = ((ServiceDescription) value).TargetNamespace;
            if (targetNamespace == null)
            {
                return string.Empty;
            }
            return targetNamespace;
        }

        public Message GetMessage(XmlQualifiedName name)
        {
            ServiceDescription serviceDescription = this.GetServiceDescription(name);
            Message message = null;
            while ((message == null) && (serviceDescription != null))
            {
                message = serviceDescription.Messages[name.Name];
                serviceDescription = serviceDescription.Next;
            }
            if (message == null)
            {
                throw this.ItemNotFound(name, "message");
            }
            return message;
        }

        public PortType GetPortType(XmlQualifiedName name)
        {
            ServiceDescription serviceDescription = this.GetServiceDescription(name);
            PortType type = null;
            while ((type == null) && (serviceDescription != null))
            {
                type = serviceDescription.PortTypes[name.Name];
                serviceDescription = serviceDescription.Next;
            }
            if (type == null)
            {
                throw this.ItemNotFound(name, "message");
            }
            return type;
        }

        public Service GetService(XmlQualifiedName name)
        {
            ServiceDescription serviceDescription = this.GetServiceDescription(name);
            Service service = null;
            while ((service == null) && (serviceDescription != null))
            {
                service = serviceDescription.Services[name.Name];
                serviceDescription = serviceDescription.Next;
            }
            if (service == null)
            {
                throw this.ItemNotFound(name, "service");
            }
            return service;
        }

        private ServiceDescription GetServiceDescription(XmlQualifiedName name)
        {
            ServiceDescription description = this[name.Namespace];
            if (description == null)
            {
                throw new ArgumentException(System.Web.Services.Res.GetString("WebDescriptionMissing", new object[] { name.ToString(), name.Namespace }), "name");
            }
            return description;
        }

        public int IndexOf(ServiceDescription serviceDescription)
        {
            return base.List.IndexOf(serviceDescription);
        }

        public void Insert(int index, ServiceDescription serviceDescription)
        {
            base.List.Insert(index, serviceDescription);
        }

        private Exception ItemNotFound(XmlQualifiedName name, string type)
        {
            return new Exception(System.Web.Services.Res.GetString("WebDescriptionMissingItem", new object[] { type, name.Name, name.Namespace }));
        }

        protected override void OnInsertComplete(int index, object value)
        {
            string key = this.GetKey(value);
            if (key != null)
            {
                ServiceDescription description1 = (ServiceDescription) this.Table[key];
                ((ServiceDescription) value).Next = (ServiceDescription) this.Table[key];
                this.Table[key] = value;
            }
            this.SetParent(value, this);
        }

        public void Remove(ServiceDescription serviceDescription)
        {
            base.List.Remove(serviceDescription);
        }

        protected override void SetParent(object value, object parent)
        {
            ((ServiceDescription) value).SetParent((ServiceDescriptionCollection) parent);
        }

        public ServiceDescription this[int index]
        {
            get
            {
                return (ServiceDescription) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        public ServiceDescription this[string ns]
        {
            get
            {
                return (ServiceDescription) this.Table[ns];
            }
        }
    }
}

