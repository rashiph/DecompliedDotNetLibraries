namespace System.Data.Design
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class DataSourceXmlAttributeAttribute : DataSourceXmlSerializationAttribute
    {
        internal DataSourceXmlAttributeAttribute() : this(null)
        {
        }

        internal DataSourceXmlAttributeAttribute(string attributeName)
        {
            base.Name = attributeName;
        }
    }
}

