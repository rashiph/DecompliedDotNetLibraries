namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;

    [ConfigurationCollection(typeof(XPathMessageFilterElement))]
    public sealed class XPathMessageFilterElementCollection : ServiceModelConfigurationElementCollection<XPathMessageFilterElement>
    {
        public XPathMessageFilterElementCollection() : base(ConfigurationElementCollectionType.AddRemoveClearMap, null, new XPathMessageFilterElementComparer())
        {
        }

        public override bool ContainsKey(object key)
        {
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }
            string str = string.Empty;
            if (key.GetType().IsAssignableFrom(typeof(XPathMessageFilter)))
            {
                str = XPathMessageFilterElementComparer.ParseXPathString((XPathMessageFilter) key);
            }
            else
            {
                if (!key.GetType().IsAssignableFrom(typeof(string)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ConfigInvalidKeyType", new object[] { "XPathMessageFilterElement", typeof(XPathMessageFilter).AssemblyQualifiedName, key.GetType().AssemblyQualifiedName })));
                }
                str = (string) key;
            }
            return base.ContainsKey(str);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            XPathMessageFilterElement element2 = (XPathMessageFilterElement) element;
            if (element2.Filter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("element", System.ServiceModel.SR.GetString("ConfigXPathFilterIsNull"));
            }
            return XPathMessageFilterElementComparer.ParseXPathString(element2.Filter);
        }

        public override XPathMessageFilterElement this[object key]
        {
            get
            {
                if (key == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
                }
                if (!key.GetType().IsAssignableFrom(typeof(XPathMessageFilter)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ConfigInvalidKeyType", new object[] { "XPathMessageFilterElement", typeof(XPathMessageFilter).AssemblyQualifiedName, key.GetType().AssemblyQualifiedName })));
                }
                XPathMessageFilterElement element = (XPathMessageFilterElement) base.BaseGet(key);
                if (element == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new KeyNotFoundException(System.ServiceModel.SR.GetString("ConfigKeyNotFoundInElementCollection", new object[] { key.ToString() })));
                }
                return element;
            }
            set
            {
                if (this.IsReadOnly())
                {
                    base.Add(value);
                }
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (key == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
                }
                if (!key.GetType().IsAssignableFrom(typeof(XPathMessageFilter)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ConfigInvalidKeyType", new object[] { "XPathMessageFilterElement", typeof(XPathMessageFilter).AssemblyQualifiedName, key.GetType().AssemblyQualifiedName })));
                }
                string a = XPathMessageFilterElementComparer.ParseXPathString((XPathMessageFilter) key);
                string elementKey = (string) this.GetElementKey(value);
                if (!string.Equals(a, elementKey, StringComparison.Ordinal))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("ConfigKeysDoNotMatch", new object[] { this.GetElementKey(value).ToString(), key.ToString() }));
                }
                if (base.BaseGet(key) != null)
                {
                    base.BaseRemove(key);
                }
                base.Add(value);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return new ConfigurationPropertyCollection();
            }
        }
    }
}

