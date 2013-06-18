namespace System.Web.Services.Description
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime;
    using System.Web.Services;
    using System.Xml;

    public sealed class ServiceDescriptionFormatExtensionCollection : ServiceDescriptionBaseCollection
    {
        private ArrayList handledElements;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ServiceDescriptionFormatExtensionCollection(object parent) : base(parent)
        {
        }

        public int Add(object extension)
        {
            return base.List.Add(extension);
        }

        public bool Contains(object extension)
        {
            return base.List.Contains(extension);
        }

        public void CopyTo(object[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public object Find(Type type)
        {
            for (int i = 0; i < base.List.Count; i++)
            {
                object obj2 = base.List[i];
                if (type.IsAssignableFrom(obj2.GetType()))
                {
                    ((ServiceDescriptionFormatExtension) obj2).Handled = true;
                    return obj2;
                }
            }
            return null;
        }

        public XmlElement Find(string name, string ns)
        {
            for (int i = 0; i < base.List.Count; i++)
            {
                XmlElement element = base.List[i] as XmlElement;
                if (((element != null) && (element.LocalName == name)) && (element.NamespaceURI == ns))
                {
                    this.SetHandled(element);
                    return element;
                }
            }
            return null;
        }

        public object[] FindAll(Type type)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < base.List.Count; i++)
            {
                object obj2 = base.List[i];
                if (type.IsAssignableFrom(obj2.GetType()))
                {
                    ((ServiceDescriptionFormatExtension) obj2).Handled = true;
                    list.Add(obj2);
                }
            }
            return (object[]) list.ToArray(type);
        }

        public XmlElement[] FindAll(string name, string ns)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < base.List.Count; i++)
            {
                XmlElement element = base.List[i] as XmlElement;
                if (((element != null) && (element.LocalName == name)) && (element.NamespaceURI == ns))
                {
                    this.SetHandled(element);
                    list.Add(element);
                }
            }
            return (XmlElement[]) list.ToArray(typeof(XmlElement));
        }

        public int IndexOf(object extension)
        {
            return base.List.IndexOf(extension);
        }

        public void Insert(int index, object extension)
        {
            base.List.Insert(index, extension);
        }

        public bool IsHandled(object item)
        {
            if (item is XmlElement)
            {
                return this.IsHandled((XmlElement) item);
            }
            return ((ServiceDescriptionFormatExtension) item).Handled;
        }

        private bool IsHandled(XmlElement element)
        {
            if (this.handledElements == null)
            {
                return false;
            }
            return this.handledElements.Contains(element);
        }

        public bool IsRequired(object item)
        {
            if (item is XmlElement)
            {
                return this.IsRequired((XmlElement) item);
            }
            return ((ServiceDescriptionFormatExtension) item).Required;
        }

        private bool IsRequired(XmlElement element)
        {
            XmlAttribute attribute = element.Attributes["required", "http://schemas.xmlsoap.org/wsdl/"];
            if ((attribute == null) || (attribute.Value == null))
            {
                attribute = element.Attributes["required"];
                if ((attribute == null) || (attribute.Value == null))
                {
                    return false;
                }
            }
            return XmlConvert.ToBoolean(attribute.Value);
        }

        protected override void OnValidate(object value)
        {
            if (!(value is XmlElement) && !(value is ServiceDescriptionFormatExtension))
            {
                throw new ArgumentException(System.Web.Services.Res.GetString("OnlyXmlElementsOrTypesDerivingFromServiceDescriptionFormatExtension0"), "value");
            }
            base.OnValidate(value);
        }

        public void Remove(object extension)
        {
            base.List.Remove(extension);
        }

        private void SetHandled(XmlElement element)
        {
            if (this.handledElements == null)
            {
                this.handledElements = new ArrayList();
            }
            if (!this.handledElements.Contains(element))
            {
                this.handledElements.Add(element);
            }
        }

        protected override void SetParent(object value, object parent)
        {
            if (value is ServiceDescriptionFormatExtension)
            {
                ((ServiceDescriptionFormatExtension) value).SetParent(parent);
            }
        }

        public object this[int index]
        {
            get
            {
                return base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

