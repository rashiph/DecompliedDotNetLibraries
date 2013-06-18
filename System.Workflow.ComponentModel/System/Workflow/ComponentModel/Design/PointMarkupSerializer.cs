namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Reflection;
    using System.Workflow.ComponentModel.Serialization;

    internal sealed class PointMarkupSerializer : WorkflowMarkupSerializer
    {
        protected internal override bool CanSerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            return (value is Point);
        }

        protected internal override object DeserializeFromString(WorkflowMarkupSerializationManager serializationManager, Type propertyType, string value)
        {
            object empty = Point.Empty;
            string str = value;
            if (string.IsNullOrEmpty(str))
            {
                return empty;
            }
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(Point));
            if (((converter != null) && converter.CanConvertFrom(typeof(string))) && !base.IsValidCompactAttributeFormat(str))
            {
                return converter.ConvertFrom(value);
            }
            return base.SerializeToString(serializationManager, value);
        }

        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();
            if (obj is Point)
            {
                list.Add(typeof(Point).GetProperty("X"));
                list.Add(typeof(Point).GetProperty("Y"));
            }
            return list.ToArray();
        }

        protected internal override string SerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(value);
            if ((converter != null) && converter.CanConvertTo(typeof(string)))
            {
                return (converter.ConvertTo(value, typeof(string)) as string);
            }
            return base.SerializeToString(serializationManager, value);
        }
    }
}

