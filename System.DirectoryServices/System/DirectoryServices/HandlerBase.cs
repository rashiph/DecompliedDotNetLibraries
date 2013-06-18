namespace System.DirectoryServices
{
    using System;
    using System.Configuration;
    using System.Xml;

    internal class HandlerBase
    {
        private HandlerBase()
        {
        }

        internal static void RemoveBooleanAttribute(XmlNode node, string name, ref bool value)
        {
            value = false;
            XmlNode node2 = node.Attributes.RemoveNamedItem(name);
            if (node2 != null)
            {
                try
                {
                    value = bool.Parse(node2.Value);
                }
                catch (FormatException)
                {
                    throw new ConfigurationErrorsException(System.DirectoryServices.Res.GetString("Invalid_boolean_attribute", new object[] { name }));
                }
            }
        }
    }
}

