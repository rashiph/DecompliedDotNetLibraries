namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.ServiceModel.Activities;

    public class TrackingConfigurationCollection<TConfigurationElement> : ConfigurationElementCollection where TConfigurationElement: TrackingConfigurationElement, new()
    {
        public void Add(TConfigurationElement element)
        {
            if (!this.IsReadOnly() && (element == null))
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }
            base.BaseAdd(element);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return Activator.CreateInstance<TConfigurationElement>();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TrackingConfigurationElement) element).ElementKey;
        }

        public int IndexOf(TConfigurationElement element)
        {
            if (element == null)
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }
            return base.BaseIndexOf(element);
        }

        public void Remove(TConfigurationElement element)
        {
            if (!this.IsReadOnly() && (element == null))
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }
            base.BaseRemove(this.GetElementKey(element));
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        public TConfigurationElement this[int index]
        {
            get
            {
                return (TConfigurationElement) base.BaseGet(index);
            }
            set
            {
                if (!this.IsReadOnly())
                {
                    if (value == null)
                    {
                        throw FxTrace.Exception.ArgumentNull("value");
                    }
                    if (base.BaseGet(index) != null)
                    {
                        base.BaseRemoveAt(index);
                    }
                }
                base.BaseAdd(index, value);
            }
        }
    }
}

