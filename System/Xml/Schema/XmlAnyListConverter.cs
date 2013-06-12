namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Xml;

    internal class XmlAnyListConverter : XmlListConverter
    {
        public static readonly XmlValueConverter AnyAtomicList = new XmlAnyListConverter((XmlBaseConverter) XmlAnyConverter.AnyAtomic);
        public static readonly XmlValueConverter ItemList = new XmlAnyListConverter((XmlBaseConverter) XmlAnyConverter.Item);

        protected XmlAnyListConverter(XmlBaseConverter atomicConverter) : base(atomicConverter)
        {
        }

        public override object ChangeType(object value, Type destinationType, IXmlNamespaceResolver nsResolver)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((!(value is IEnumerable) || (value.GetType() == XmlBaseConverter.StringType)) || (value.GetType() == XmlBaseConverter.ByteArrayType))
            {
                value = new object[] { value };
            }
            return this.ChangeListType(value, destinationType, nsResolver);
        }
    }
}

