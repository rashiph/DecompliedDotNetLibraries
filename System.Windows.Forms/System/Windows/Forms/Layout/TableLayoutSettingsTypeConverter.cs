namespace System.Windows.Forms.Layout
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Text;
    using System.Windows.Forms;
    using System.Xml;

    public class TableLayoutSettingsTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (!(destinationType == typeof(InstanceDescriptor)) && !(destinationType == typeof(string)))
            {
                return base.CanConvertTo(context, destinationType);
            }
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(value as string);
                TableLayoutSettings settings = new TableLayoutSettings();
                this.ParseControls(settings, document.GetElementsByTagName("Control"));
                this.ParseStyles(settings, document.GetElementsByTagName("Columns"), true);
                this.ParseStyles(settings, document.GetElementsByTagName("Rows"), false);
                return settings;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (!(value is TableLayoutSettings) || !(destinationType == typeof(string)))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            TableLayoutSettings settings = value as TableLayoutSettings;
            StringBuilder output = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(output);
            writer.WriteStartElement("TableLayoutSettings");
            writer.WriteStartElement("Controls");
            foreach (TableLayoutSettings.ControlInformation information in settings.GetControlsInformation())
            {
                writer.WriteStartElement("Control");
                writer.WriteAttributeString("Name", information.Name.ToString());
                writer.WriteAttributeString("Row", information.Row.ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("RowSpan", information.RowSpan.ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("Column", information.Column.ToString(CultureInfo.CurrentCulture));
                writer.WriteAttributeString("ColumnSpan", information.ColumnSpan.ToString(CultureInfo.CurrentCulture));
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteStartElement("Columns");
            StringBuilder builder2 = new StringBuilder();
            foreach (ColumnStyle style in (IEnumerable) settings.ColumnStyles)
            {
                builder2.AppendFormat("{0},{1},", style.SizeType, style.Width);
            }
            if (builder2.Length > 0)
            {
                builder2.Remove(builder2.Length - 1, 1);
            }
            writer.WriteAttributeString("Styles", builder2.ToString());
            writer.WriteEndElement();
            writer.WriteStartElement("Rows");
            StringBuilder builder3 = new StringBuilder();
            foreach (RowStyle style2 in (IEnumerable) settings.RowStyles)
            {
                builder3.AppendFormat("{0},{1},", style2.SizeType, style2.Height);
            }
            if (builder3.Length > 0)
            {
                builder3.Remove(builder3.Length - 1, 1);
            }
            writer.WriteAttributeString("Styles", builder3.ToString());
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Close();
            return output.ToString();
        }

        private string GetAttributeValue(XmlNode node, string attribute)
        {
            XmlAttribute attribute2 = node.Attributes[attribute];
            if (attribute2 != null)
            {
                return attribute2.Value;
            }
            return null;
        }

        private int GetAttributeValue(XmlNode node, string attribute, int valueIfNotFound)
        {
            int num;
            string attributeValue = this.GetAttributeValue(node, attribute);
            if (!string.IsNullOrEmpty(attributeValue) && int.TryParse(attributeValue, out num))
            {
                return num;
            }
            return valueIfNotFound;
        }

        private void ParseControls(TableLayoutSettings settings, XmlNodeList controlXmlFragments)
        {
            foreach (XmlNode node in controlXmlFragments)
            {
                string attributeValue = this.GetAttributeValue(node, "Name");
                if (!string.IsNullOrEmpty(attributeValue))
                {
                    int row = this.GetAttributeValue(node, "Row", -1);
                    int num2 = this.GetAttributeValue(node, "RowSpan", 1);
                    int column = this.GetAttributeValue(node, "Column", -1);
                    int num4 = this.GetAttributeValue(node, "ColumnSpan", 1);
                    settings.SetRow(attributeValue, row);
                    settings.SetColumn(attributeValue, column);
                    settings.SetRowSpan(attributeValue, num2);
                    settings.SetColumnSpan(attributeValue, num4);
                }
            }
        }

        private void ParseStyles(TableLayoutSettings settings, XmlNodeList controlXmlFragments, bool columns)
        {
            foreach (XmlNode node in controlXmlFragments)
            {
                string attributeValue = this.GetAttributeValue(node, "Styles");
                System.Type enumType = typeof(SizeType);
                if (!string.IsNullOrEmpty(attributeValue))
                {
                    int num2;
                    for (int i = 0; i < attributeValue.Length; i = num2)
                    {
                        float num3;
                        num2 = i;
                        while (char.IsLetter(attributeValue[num2]))
                        {
                            num2++;
                        }
                        SizeType sizeType = (SizeType) Enum.Parse(enumType, attributeValue.Substring(i, num2 - i), true);
                        while (!char.IsDigit(attributeValue[num2]))
                        {
                            num2++;
                        }
                        StringBuilder builder = new StringBuilder();
                        while ((num2 < attributeValue.Length) && char.IsDigit(attributeValue[num2]))
                        {
                            builder.Append(attributeValue[num2]);
                            num2++;
                        }
                        builder.Append('.');
                        while ((num2 < attributeValue.Length) && !char.IsLetter(attributeValue[num2]))
                        {
                            if (char.IsDigit(attributeValue[num2]))
                            {
                                builder.Append(attributeValue[num2]);
                            }
                            num2++;
                        }
                        if (!float.TryParse(builder.ToString(), NumberStyles.Float, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat, out num3))
                        {
                            num3 = 0f;
                        }
                        if (columns)
                        {
                            settings.ColumnStyles.Add(new ColumnStyle(sizeType, num3));
                        }
                        else
                        {
                            settings.RowStyles.Add(new RowStyle(sizeType, num3));
                        }
                    }
                }
            }
        }
    }
}

