namespace System.Runtime.Remoting.Metadata
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Field), ComVisible(true)]
    public sealed class SoapFieldAttribute : SoapAttribute
    {
        private ExplicitlySet _explicitlySet;
        private int _order;
        private string _xmlElementName;

        public bool IsInteropXmlElement()
        {
            return ((this._explicitlySet & ExplicitlySet.XmlElementName) != ExplicitlySet.None);
        }

        public int Order
        {
            get
            {
                return this._order;
            }
            set
            {
                this._order = value;
            }
        }

        public string XmlElementName
        {
            get
            {
                if ((this._xmlElementName == null) && (base.ReflectInfo != null))
                {
                    this._xmlElementName = ((FieldInfo) base.ReflectInfo).Name;
                }
                return this._xmlElementName;
            }
            set
            {
                this._xmlElementName = value;
                this._explicitlySet |= ExplicitlySet.XmlElementName;
            }
        }

        [Serializable, Flags]
        private enum ExplicitlySet
        {
            None,
            XmlElementName
        }
    }
}

