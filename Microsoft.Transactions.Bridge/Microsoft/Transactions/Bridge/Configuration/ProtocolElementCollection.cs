namespace Microsoft.Transactions.Bridge.Configuration
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(ProtocolElement))]
    internal sealed class ProtocolElementCollection : ConfigurationElementCollection
    {
        private const string wstxProtocolType10 = "Microsoft.Transactions.Wsat.Protocol.PluggableProtocol10, Microsoft.Transactions.Bridge, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
        private const string wstxProtocolType11 = "Microsoft.Transactions.Wsat.Protocol.PluggableProtocol11, Microsoft.Transactions.Bridge, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

        public void Add(ProtocolElement element)
        {
            this.BaseAdd(element);
        }

        public void AssertBothWsatProtocolVersions()
        {
            bool flag = false;
            bool flag2 = false;
            foreach (ProtocolElement element in this)
            {
                if (element.Type == "Microsoft.Transactions.Wsat.Protocol.PluggableProtocol10, Microsoft.Transactions.Bridge, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
                {
                    flag = true;
                }
                else if (element.Type == "Microsoft.Transactions.Wsat.Protocol.PluggableProtocol11, Microsoft.Transactions.Bridge, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
                {
                    flag2 = true;
                }
            }
            if ((!flag & flag2) || (flag & !flag2))
            {
                string str;
                if (flag)
                {
                    str = " Wsat 1.0 ";
                }
                else
                {
                    str = " Wsat 1.1 ";
                }
                DiagnosticUtility.FailFast("Both Wsat protocol versions should be configured to start up. Only" + str + "is started.");
            }
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ProtocolElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProtocolElement) element).Type;
        }

        public int IndexOf(ProtocolElement element)
        {
            return base.BaseIndexOf(element);
        }

        public void Remove(ProtocolElement element)
        {
            base.BaseRemove(element.Type);
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        internal void SetDefaults()
        {
            ProtocolElement element = new ProtocolElement {
                Type = "Microsoft.Transactions.Wsat.Protocol.PluggableProtocol10, Microsoft.Transactions.Bridge, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
            };
            ProtocolElement element2 = new ProtocolElement {
                Type = "Microsoft.Transactions.Wsat.Protocol.PluggableProtocol11, Microsoft.Transactions.Bridge, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
            };
            this.Add(element);
            this.Add(element2);
            this.ResetModified();
        }

        public ProtocolElement this[int index]
        {
            get
            {
                return (ProtocolElement) base.BaseGet(index);
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
    }
}

