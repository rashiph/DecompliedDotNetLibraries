namespace System.Data.Design
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class DataSourceXmlElementAttribute : DataSourceXmlSerializationAttribute
    {
        internal DataSourceXmlElementAttribute() : this(null)
        {
        }

        internal DataSourceXmlElementAttribute(string elementName)
        {
            base.Name = elementName;
        }
    }
}

