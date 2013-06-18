namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Workflow.ComponentModel.Serialization;

    internal sealed class ColorMarkupSerializer : WorkflowMarkupSerializer
    {
        protected internal override bool CanSerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            return (value is Color);
        }

        protected internal override object DeserializeFromString(WorkflowMarkupSerializationManager serializationManager, Type propertyType, string value)
        {
            if (propertyType.IsAssignableFrom(typeof(Color)))
            {
                string str = value;
                if (!string.IsNullOrEmpty(str))
                {
                    if (str.StartsWith("0X", StringComparison.OrdinalIgnoreCase))
                    {
                        long num = Convert.ToInt64(value, 0x10) & ((long) 0xffffffffL);
                        return Color.FromArgb((byte) (num >> 0x18), (byte) (num >> 0x10), (byte) (num >> 8), (byte) num);
                    }
                    return base.DeserializeFromString(serializationManager, propertyType, value);
                }
            }
            return null;
        }

        protected internal override string SerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            string str = string.Empty;
            if (value is Color)
            {
                Color color = (Color) value;
                str = "0X" + ((long) (((ulong) ((((color.A << 0x18) | (color.R << 0x10)) | (color.G << 8)) | color.B)) & 0xffffffffL)).ToString("X08", CultureInfo.InvariantCulture);
            }
            return str;
        }
    }
}

