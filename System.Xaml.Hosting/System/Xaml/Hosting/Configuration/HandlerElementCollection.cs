namespace System.Xaml.Hosting.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Xaml.Hosting;

    [ConfigurationCollection(typeof(HandlerElement), CollectionType=ConfigurationElementCollectionType.AddRemoveClearMapAlternate)]
    public sealed class HandlerElementCollection : ConfigurationElementCollection
    {
        public HandlerElementCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public void Add(HandlerElement handlerElement)
        {
            if (!this.IsReadOnly() && (handlerElement == null))
            {
                throw FxTrace.Exception.ArgumentNull("handlerElement");
            }
            base.BaseAdd(handlerElement, false);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new HandlerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }
            return ((HandlerElement) element).Key;
        }

        public void Remove(string xamlRootElementType)
        {
            if (!this.IsReadOnly() && (xamlRootElementType == null))
            {
                throw FxTrace.Exception.ArgumentNull("xamlRootElementType");
            }
            base.BaseRemove(xamlRootElementType);
        }

        public void Remove(HandlerElement handlerElement)
        {
            if (!this.IsReadOnly() && (handlerElement == null))
            {
                throw FxTrace.Exception.ArgumentNull("handlerElement");
            }
            base.BaseRemove(this.GetElementKey(handlerElement));
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        internal bool TryGetHttpHandlerType(Type hostedXamlType, out Type httpHandlerType)
        {
            httpHandlerType = null;
            foreach (HandlerElement element in this)
            {
                if (element.LoadXamlRootElementType().IsAssignableFrom(hostedXamlType))
                {
                    httpHandlerType = element.LoadHttpHandlerType();
                    return true;
                }
            }
            return false;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMapAlternate;
            }
        }

        public HandlerElement this[int index]
        {
            get
            {
                return (HandlerElement) base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return false;
            }
        }
    }
}

