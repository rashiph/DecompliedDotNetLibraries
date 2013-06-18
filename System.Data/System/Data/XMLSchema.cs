namespace System.Data
{
    using System;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Globalization;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    internal class XMLSchema
    {
        internal static bool FEqualIdentity(XmlNode node, string name, string ns)
        {
            return (((node != null) && (node.LocalName == name)) && (node.NamespaceURI == ns));
        }

        internal static string GenUniqueColumnName(string proposedName, DataTable table)
        {
            if (table.Columns.IndexOf(proposedName) >= 0)
            {
                for (int i = 0; i <= table.Columns.Count; i++)
                {
                    string columnName = proposedName + "_" + i.ToString(CultureInfo.InvariantCulture);
                    if (table.Columns.IndexOf(columnName) < 0)
                    {
                        return columnName;
                    }
                }
            }
            return proposedName;
        }

        internal static bool GetBooleanAttribute(XmlElement element, string attrName, string attrNS, bool defVal)
        {
            string attribute = element.GetAttribute(attrName, attrNS);
            if ((attribute == null) || (attribute.Length == 0))
            {
                return defVal;
            }
            if ((attribute == "true") || (attribute == "1"))
            {
                return true;
            }
            if (!(attribute == "false") && !(attribute == "0"))
            {
                throw ExceptionBuilder.InvalidAttributeValue(attrName, attribute);
            }
            return false;
        }

        internal static TypeConverter GetConverter(Type type)
        {
            TypeConverter converter;
            HostProtectionAttribute attribute = new HostProtectionAttribute {
                SharedState = true
            };
            ((CodeAccessPermission) attribute.CreatePermission()).Assert();
            try
            {
                converter = TypeDescriptor.GetConverter(type);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return converter;
        }

        internal static void SetProperties(object instance, XmlAttributeCollection attrs)
        {
            for (int i = 0; i < attrs.Count; i++)
            {
                if (attrs[i].NamespaceURI == "urn:schemas-microsoft-com:xml-msdata")
                {
                    string localName = attrs[i].LocalName;
                    string text = attrs[i].Value;
                    if (((localName != "DefaultValue") && (localName != "RemotingFormat")) && ((localName != "Expression") || !(instance is DataColumn)))
                    {
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(instance)[localName];
                        if (descriptor != null)
                        {
                            object type;
                            Type propertyType = descriptor.PropertyType;
                            TypeConverter converter = GetConverter(propertyType);
                            if (converter.CanConvertFrom(typeof(string)))
                            {
                                type = converter.ConvertFromString(text);
                            }
                            else if (propertyType == typeof(Type))
                            {
                                type = DataStorage.GetType(text);
                            }
                            else
                            {
                                if (propertyType != typeof(CultureInfo))
                                {
                                    throw ExceptionBuilder.CannotConvert(text, propertyType.FullName);
                                }
                                type = new CultureInfo(text);
                            }
                            descriptor.SetValue(instance, type);
                        }
                    }
                }
            }
        }
    }
}

