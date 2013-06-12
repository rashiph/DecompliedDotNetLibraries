namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.Util;
    using System.Xml;

    internal static class HandlerBase
    {
        internal static bool CheckAndReadConnectionString(ref string connectionString, bool throwIfError)
        {
            ConnectionStringSettings settings = RuntimeConfig.GetConfig().ConnectionStrings.ConnectionStrings[connectionString];
            if (((settings != null) && (settings.ConnectionString != null)) && (settings.ConnectionString.Length > 0))
            {
                connectionString = settings.ConnectionString;
            }
            return CheckAndReadRegistryValue(ref connectionString, throwIfError);
        }

        internal static bool CheckAndReadRegistryValue(ref string value, bool throwIfError)
        {
            if (value == null)
            {
                return true;
            }
            if (!System.Web.Util.StringUtil.StringStartsWithIgnoreCase(value, "registry:"))
            {
                return true;
            }
            StringBuilder buffer = new StringBuilder(0x400);
            if (UnsafeNativeMethods.GetCredentialFromRegistry(value, buffer, 0x400) == 0)
            {
                value = buffer.ToString();
                return true;
            }
            if (throwIfError)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_registry_config"));
            }
            return false;
        }

        internal static void CheckAssignableType(XmlNode node, Type baseType, Type type)
        {
            if (!baseType.IsAssignableFrom(type))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Type_doesnt_inherit_from_type", new object[] { type.FullName, baseType.FullName }), node);
            }
        }

        internal static void CheckAssignableType(string filename, int lineNumber, Type baseType, Type type)
        {
            if (!baseType.IsAssignableFrom(type))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Type_doesnt_inherit_from_type", new object[] { type.FullName, baseType.FullName }), filename, lineNumber);
            }
        }

        internal static void CheckForbiddenAttribute(XmlNode node, string attrib)
        {
            XmlAttribute attribute = node.Attributes[attrib];
            if (attribute != null)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_base_unrecognized_attribute", new object[] { attrib }), attribute);
            }
        }

        internal static void CheckForNonCommentChildNodes(XmlNode node)
        {
            foreach (XmlNode node2 in node.ChildNodes)
            {
                if (node2.NodeType != XmlNodeType.Comment)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_base_no_child_nodes"), node2);
                }
            }
        }

        internal static void CheckForUnrecognizedAttributes(XmlNode node)
        {
            if (node.Attributes.Count != 0)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_base_unrecognized_attribute", new object[] { node.Attributes[0].Name }), node.Attributes[0]);
            }
        }

        private static XmlNode GetAndRemoveAttribute(XmlNode node, string attrib, bool fRequired)
        {
            XmlNode node2 = node.Attributes.RemoveNamedItem(attrib);
            if (fRequired && (node2 == null))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Missing_required_attribute", new object[] { attrib, node.Name }), node);
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
                if (node2.Value == "true")
                {
                    val = true;
                    return node2;
                }
                if (node2.Value != "false")
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_boolean_attribute", new object[] { node2.Name }), node2);
                }
                val = false;
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
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_integer_attribute", new object[] { node2.Name }), node2);
                }
                try
                {
                    val = int.Parse(node2.Value, CultureInfo.InvariantCulture);
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_integer_attribute", new object[] { node2.Name }), exception, node2);
                }
            }
            return node2;
        }

        internal static XmlNode GetAndRemoveNonEmptyStringAttribute(XmlNode node, string attrib, ref string val)
        {
            return GetAndRemoveNonEmptyStringAttributeInternal(node, attrib, false, ref val);
        }

        private static XmlNode GetAndRemoveNonEmptyStringAttributeInternal(XmlNode node, string attrib, bool fRequired, ref string val)
        {
            XmlNode node2 = GetAndRemoveStringAttributeInternal(node, attrib, fRequired, ref val);
            if ((node2 != null) && (val.Length == 0))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Empty_attribute", new object[] { attrib }), node2);
            }
            return node2;
        }

        private static XmlNode GetAndRemovePositiveAttributeInternal(XmlNode node, string attrib, bool fRequired, ref int val)
        {
            XmlNode node2 = GetAndRemoveIntegerAttributeInternal(node, attrib, fRequired, ref val);
            if ((node2 != null) && (val <= 0))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_positive_integer_attribute", new object[] { attrib }), node2);
            }
            return node2;
        }

        internal static XmlNode GetAndRemovePositiveIntegerAttribute(XmlNode node, string attrib, ref int val)
        {
            return GetAndRemovePositiveAttributeInternal(node, attrib, false, ref val);
        }

        internal static XmlNode GetAndRemoveRequiredNonEmptyStringAttribute(XmlNode node, string attrib, ref string val)
        {
            return GetAndRemoveNonEmptyStringAttributeInternal(node, attrib, true, ref val);
        }

        internal static XmlNode GetAndRemoveRequiredStringAttribute(XmlNode node, string attrib, ref string val)
        {
            return GetAndRemoveStringAttributeInternal(node, attrib, true, ref val);
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

        internal static XmlNode GetAndRemoveTypeAttribute(XmlNode node, string attrib, ref Type val)
        {
            return GetAndRemoveTypeAttributeInternal(node, attrib, false, ref val);
        }

        private static XmlNode GetAndRemoveTypeAttributeInternal(XmlNode node, string attrib, bool fRequired, ref Type val)
        {
            XmlNode node2 = GetAndRemoveAttribute(node, attrib, fRequired);
            if (node2 != null)
            {
                val = ConfigUtil.GetType(node2.Value, node2);
            }
            return node2;
        }

        internal static bool IsServerConfiguration(object context)
        {
            return (context is HttpConfigurationContext);
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
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_base_required_attribute_missing", new object[] { name }), node);
            }
            if ((node2.Value.Length == 0) && !allowEmpty)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_base_required_attribute_empty", new object[] { name }), node);
            }
            return node2.Value;
        }

        internal static void ThrowUnrecognizedElement(XmlNode node)
        {
            throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_base_unrecognized_element"), node);
        }
    }
}

