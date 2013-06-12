namespace System.Xml.Serialization
{
    using System;
    using System.Xml;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=true)]
    public class XmlAnyElementAttribute : Attribute
    {
        private string name;
        private string ns;
        private bool nsSpecified;
        private int order;

        public XmlAnyElementAttribute()
        {
            this.order = -1;
        }

        public XmlAnyElementAttribute(string name)
        {
            this.order = -1;
            this.name = name;
        }

        public XmlAnyElementAttribute(string name, string ns)
        {
            this.order = -1;
            this.name = name;
            this.ns = ns;
            this.nsSpecified = true;
        }

        public string Name
        {
            get
            {
                if (this.name != null)
                {
                    return this.name;
                }
                return string.Empty;
            }
            set
            {
                this.name = value;
            }
        }

        public string Namespace
        {
            get
            {
                return this.ns;
            }
            set
            {
                this.ns = value;
                this.nsSpecified = true;
            }
        }

        internal bool NamespaceSpecified
        {
            get
            {
                return this.nsSpecified;
            }
        }

        public int Order
        {
            get
            {
                return this.order;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("XmlDisallowNegativeValues"), "Order");
                }
                this.order = value;
            }
        }
    }
}

