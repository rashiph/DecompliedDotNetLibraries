namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    public class CorrelationDataMessageProperty : IMessageProperty
    {
        private Dictionary<string, DataProviderEntry> dataProviders;
        private const string PropertyName = "CorrelationDataMessageProperty";

        public CorrelationDataMessageProperty()
        {
        }

        private CorrelationDataMessageProperty(IDictionary<string, DataProviderEntry> dataProviders)
        {
            if ((dataProviders != null) && (dataProviders.Count > 0))
            {
                this.dataProviders = new Dictionary<string, DataProviderEntry>(dataProviders);
            }
        }

        public void Add(string name, Func<string> dataProvider)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            if (dataProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dataProvider");
            }
            if (this.dataProviders == null)
            {
                this.dataProviders = new Dictionary<string, DataProviderEntry>();
            }
            this.dataProviders.Add(name, new DataProviderEntry(dataProvider));
        }

        public static void AddData(Message message, string name, Func<string> dataProvider)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            if (dataProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dataProvider");
            }
            CorrelationDataMessageProperty property = null;
            object obj2 = null;
            if (message.Properties.TryGetValue("CorrelationDataMessageProperty", out obj2))
            {
                property = obj2 as CorrelationDataMessageProperty;
            }
            bool flag = false;
            if (property == null)
            {
                property = new CorrelationDataMessageProperty();
                flag = true;
            }
            property.Add(name, dataProvider);
            if (flag)
            {
                message.Properties["CorrelationDataMessageProperty"] = property;
            }
        }

        public IMessageProperty CreateCopy()
        {
            return new CorrelationDataMessageProperty(this.dataProviders);
        }

        public bool Remove(string name)
        {
            return ((this.dataProviders != null) && this.dataProviders.Remove(name));
        }

        public static bool TryGet(Message message, out CorrelationDataMessageProperty property)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            return TryGet(message.Properties, out property);
        }

        public static bool TryGet(MessageProperties properties, out CorrelationDataMessageProperty property)
        {
            object obj2 = null;
            if (properties.TryGetValue("CorrelationDataMessageProperty", out obj2))
            {
                property = obj2 as CorrelationDataMessageProperty;
            }
            else
            {
                property = null;
            }
            return (property != null);
        }

        public bool TryGetValue(string name, out string value)
        {
            DataProviderEntry entry;
            if ((this.dataProviders != null) && this.dataProviders.TryGetValue(name, out entry))
            {
                value = entry.Data;
                return true;
            }
            value = null;
            return false;
        }

        public static string Name
        {
            get
            {
                return "CorrelationDataMessageProperty";
            }
        }

        private class DataProviderEntry
        {
            private Func<string> dataProvider;
            private string resolvedData;

            public DataProviderEntry(Func<string> dataProvider)
            {
                this.dataProvider = dataProvider;
                this.resolvedData = null;
            }

            public string Data
            {
                get
                {
                    if (this.dataProvider != null)
                    {
                        this.resolvedData = this.dataProvider();
                        this.dataProvider = null;
                    }
                    return this.resolvedData;
                }
            }
        }
    }
}

