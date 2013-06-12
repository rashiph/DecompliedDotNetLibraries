namespace System.Xml.XPath
{
    using System;
    using System.Xml;
    using System.Xml.Schema;

    public abstract class XPathItem
    {
        protected XPathItem()
        {
        }

        public virtual object ValueAs(Type returnType)
        {
            return this.ValueAs(returnType, null);
        }

        public abstract object ValueAs(Type returnType, IXmlNamespaceResolver nsResolver);

        public abstract bool IsNode { get; }

        public abstract object TypedValue { get; }

        public abstract string Value { get; }

        public abstract bool ValueAsBoolean { get; }

        public abstract DateTime ValueAsDateTime { get; }

        public abstract double ValueAsDouble { get; }

        public abstract int ValueAsInt { get; }

        public abstract long ValueAsLong { get; }

        public abstract Type ValueType { get; }

        public abstract XmlSchemaType XmlType { get; }
    }
}

