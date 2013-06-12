namespace System.Configuration
{
    using System;
    using System.Globalization;
    using System.Xml;

    internal class HandlerBase
    {
        private HandlerBase()
        {
        }

        internal static void CheckForChildNodes(XmlNode node)
        {
            if (node.HasChildNodes)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("Config_base_no_child_nodes"), node.FirstChild);
            }
        }

        internal static void CheckForNonElement(XmlNode node)
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("Config_base_elements_only"), node);
            }
        }

        internal static void CheckForUnrecognizedAttributes(XmlNode node)
        {
            if (node.Attributes.Count != 0)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("Config_base_unrecognized_attribute", new object[] { node.Attributes[0].Name }), node);
            }
        }

        private static XmlNode GetAndRemoveAttribute(XmlNode node, string attrib, bool fRequired)
        {
            XmlNode node2 = node.Attributes.RemoveNamedItem(attrib);
            if (fRequired && (node2 == null))
            {
                throw new ConfigurationErrorsException(System.SR.GetString("Config_missing_required_attribute", new object[] { attrib, node.Name }), node);
            }
            return node2;
        }

        internal static XmlNode GetAndRemoveBooleanAttribute(XmlNode node, string attrib, ref bool val)
        {
            return GetAndRemoveBooleanAttributeInternal(node, attrib, false, ref val);
        }

        private static XmlNode GetAndRemoveBooleanAttributeInternal(XmlNode node, string attrib, bool fRequired, ref bool val)
        {
            XmlNode node2 = GetAndRemoveAttribute(node, attrib, fRequired);
            if (node2 != null)
            {
                try
                {
                    val = bool.Parse(node2.Value);
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(System.SR.GetString(System.SR.GetString("Config_invalid_boolean_attribute", new object[] { node2.Name })), exception, node2);
                }
            }
            return node2;
        }

        internal static XmlNode GetAndRemoveIntegerAttribute(XmlNode node, string attrib, ref int val)
        {
            return GetAndRemoveIntegerAttributeInternal(node, attrib, false, ref val);
        }

        private static XmlNode GetAndRemoveIntegerAttributeInternal(XmlNode node, string attrib, bool fRequired, ref int val)
        {
            XmlNode node2 = GetAndRemoveAttribute(node, attrib, fRequired);
            if (node2 != null)
            {
                if (node2.Value.Trim() != node2.Value)
                {
                    throw new ConfigurationErrorsException(System.SR.GetString("Config_invalid_integer_attribute", new object[] { node2.Name }), node2);
                }
                try
                {
                    val = int.Parse(node2.Value, CultureInfo.InvariantCulture);
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(System.SR.GetString("Config_invalid_integer_attribute", new object[] { node2.Name }), exception, node2);
                }
            }
            return node2;
        }

        internal static XmlNode GetAndRemoveStringAttribute(XmlNode node, string attrib, ref string val)
        {
            return GetAndRemoveStringAttributeInternal(node, attrib, false, ref val);
        }

        private static XmlNode GetAndRemoveStringAttributeInternal(XmlNode node, string attrib, bool fRequired, ref string val)
        {
            XmlNode node2 = GetAndRemoveAttribute(node, attrib, fRequired);
            if (node2 != null)
            {
                val = node2.Value;
            }
            return node2;
        }

        internal static bool IsIgnorableAlsoCheckForNonElement(XmlNode node)
        {
            if ((node.NodeType == XmlNodeType.Comment) || (node.NodeType == XmlNodeType.Whitespace))
            {
                return true;
            }
            CheckForNonElement(node);
            return false;
        }

        internal static string RemoveAttribute(XmlNode node, string name)
        {
            XmlNode node2 = node.Attributes.RemoveNamedItem(name);
            if (node2 != null)
            {
                return node2.Value;
            }
            return null;
        }

        internal static string RemoveRequiredAttribute(XmlNode node, string name)
        {
            return RemoveRequiredAttribute(node, name, false);
        }

        internal static string RemoveRequiredAttribute(XmlNode node, string name, bool allowEmpty)
        {
            XmlNode node2 = node.Attributes.RemoveNamedItem(name);
            if (node2 == null)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("Config_base_required_attribute_missing", new object[] { name }), node);
            }
            if (string.IsNullOrEmpty(node2.Value) && !allowEmpty)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("Config_base_required_attribute_empty", new object[] { name }), node);
            }
            return node2.Value;
        }

        internal static void ThrowUnrecognizedElement(XmlNode node)
        {
            throw new ConfigurationErrorsException(System.SR.GetString("Config_base_unrecognized_element"), node);
        }
    }
}

