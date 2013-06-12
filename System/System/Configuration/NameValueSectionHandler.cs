namespace System.Configuration
{
    using System;
    using System.Xml;

    public class NameValueSectionHandler : IConfigurationSectionHandler
    {
        private const string defaultKeyAttribute = "key";
        private const string defaultValueAttribute = "value";

        public object Create(object parent, object context, XmlNode section)
        {
            return CreateStatic(parent, section, this.KeyAttributeName, this.ValueAttributeName);
        }

        internal static object CreateStatic(object parent, XmlNode section)
        {
            return CreateStatic(parent, section, "key", "value");
        }

        internal static object CreateStatic(object parent, XmlNode section, string keyAttriuteName, string valueAttributeName)
        {
            ReadOnlyNameValueCollection values;
            if (parent == null)
            {
                values = new ReadOnlyNameValueCollection(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                ReadOnlyNameValueCollection values2 = (ReadOnlyNameValueCollection) parent;
                values = new ReadOnlyNameValueCollection(values2);
            }
            HandlerBase.CheckForUnrecognizedAttributes(section);
            foreach (XmlNode node in section.ChildNodes)
            {
                if (!HandlerBase.IsIgnorableAlsoCheckForNonElement(node))
                {
                    if (node.Name == "add")
                    {
                        string str = HandlerBase.RemoveRequiredAttribute(node, keyAttriuteName);
                        string str2 = HandlerBase.RemoveRequiredAttribute(node, valueAttributeName, true);
                        HandlerBase.CheckForUnrecognizedAttributes(node);
                        values[str] = str2;
                    }
                    else if (node.Name == "remove")
                    {
                        string name = HandlerBase.RemoveRequiredAttribute(node, keyAttriuteName);
                        HandlerBase.CheckForUnrecognizedAttributes(node);
                        values.Remove(name);
                    }
                    else if (node.Name.Equals("clear"))
                    {
                        HandlerBase.CheckForUnrecognizedAttributes(node);
                        values.Clear();
                    }
                    else
                    {
                        HandlerBase.ThrowUnrecognizedElement(node);
                    }
                }
            }
            values.SetReadOnly();
            return values;
        }

        protected virtual string KeyAttributeName
        {
            get
            {
                return "key";
            }
        }

        protected virtual string ValueAttributeName
        {
            get
            {
                return "value";
            }
        }
    }
}

