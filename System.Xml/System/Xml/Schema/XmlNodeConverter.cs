namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.XPath;

    internal class XmlNodeConverter : XmlBaseConverter
    {
        public static readonly XmlValueConverter Node = new XmlNodeConverter();

        protected XmlNodeConverter() : base(XmlTypeCode.Node)
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
            Type derivedType = value.GetType();
            if (destinationType == XmlBaseConverter.ObjectType)
            {
                destinationType = base.DefaultClrType;
            }
            if ((destinationType == XmlBaseConverter.XPathNavigatorType) && XmlBaseConverter.IsDerivedFrom(derivedType, XmlBaseConverter.XPathNavigatorType))
            {
                return (XPathNavigator) value;
            }
            if ((destinationType == XmlBaseConverter.XPathItemType) && XmlBaseConverter.IsDerivedFrom(derivedType, XmlBaseConverter.XPathNavigatorType))
            {
                return (XPathItem) value;
            }
            return this.ChangeListType(value, destinationType, nsResolver);
        }
    }
}

