namespace System.CodeDom.Compiler
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Xml;

    internal static class HandlerBase
    {
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
                throw new ConfigurationErrorsException(System.SR.GetString("Config_base_unrecognized_attribute", new object[] { node.Attributes[0].Name }), node.Attributes[0]);
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

        private static XmlNode GetAndRemoveNonEmptyStringAttributeInternal(XmlNode node, string attrib, bool fRequired, ref string val)
        {
            XmlNode node2 = GetAndRemoveStringAttributeInternal(node, attrib, fRequired, ref val);
            if ((node2 != null) && (val.Length == 0))
            {
                throw new ConfigurationErrorsException(System.SR.GetString("Empty_attribute", new object[] { attrib }), node2);
            }
            return node2;
        }

        private static XmlNode GetAndRemoveNonNegativeAttributeInternal(XmlNode node, string attrib, bool fRequired, ref int val)
        {
            XmlNode node2 = GetAndRemoveIntegerAttributeInternal(node, attrib, fRequired, ref val);
            if ((node2 != null) && (val < 0))
            {
                throw new ConfigurationErrorsException(System.SR.GetString("Invalid_nonnegative_integer_attribute", new object[] { attrib }), node2);
            }
            return node2;
        }

        internal static XmlNode GetAndRemoveNonNegativeIntegerAttribute(XmlNode node, string attrib, ref int val)
        {
            return GetAndRemoveNonNegativeAttributeInternal(node, attrib, false, ref val);
        }

        internal static XmlNode GetAndRemoveRequiredNonEmptyStringAttribute(XmlNode node, string attrib, ref string val)
        {
            return GetAndRemoveNonEmptyStringAttributeInternal(node, attrib, true, ref val);
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

        internal static void ThrowUnrecognizedElement(XmlNode node)
        {
            throw new ConfigurationErrorsException(System.SR.GetString("Config_base_unrecognized_element"), node);
        }
    }
}

