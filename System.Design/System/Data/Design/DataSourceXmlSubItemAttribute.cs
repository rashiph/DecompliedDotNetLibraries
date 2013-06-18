namespace System.Data.Design
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class DataSourceXmlSubItemAttribute : DataSourceXmlSerializationAttribute
    {
        internal DataSourceXmlSubItemAttribute()
        {
        }

        internal DataSourceXmlSubItemAttribute(Type itemType)
        {
            base.ItemType = itemType;
        }
    }
}

