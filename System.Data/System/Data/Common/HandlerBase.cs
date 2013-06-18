namespace System.Data.Common
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Xml;

    internal static class HandlerBase
    {
        internal static void CheckForChildNodes(XmlNode node)
        {
            if (node.HasChildNodes)
            {
                throw ADP.ConfigBaseNoChildNodes(node.FirstChild);
            }
        }

        private static void CheckForNonElement(XmlNode node)
        {
            if (XmlNodeType.Element != node.NodeType)
            {
                throw ADP.ConfigBaseElementsOnly(node);
            }
        }

        internal static void CheckForUnrecognizedAttributes(XmlNode node)
        {
            if (node.Attributes.Count != 0)
            {
                throw ADP.ConfigUnrecognizedAttributes(node);
            }
        }

        internal static DataSet CloneParent(DataSet parentConfig, bool insenstive)
        {
            if (parentConfig == null)
            {
                parentConfig = new DataSet("system.data");
                parentConfig.CaseSensitive = !insenstive;
                parentConfig.Locale = CultureInfo.InvariantCulture;
                return parentConfig;
            }
            parentConfig = parentConfig.Copy();
            return parentConfig;
        }

        internal static bool IsIgnorableAlsoCheckForNonElement(XmlNode node)
        {
            if ((XmlNodeType.Comment == node.NodeType) || (XmlNodeType.Whitespace == node.NodeType))
            {
                return true;
            }
            CheckForNonElement(node);
            return false;
        }

        internal static string RemoveAttribute(XmlNode node, string name, bool required, bool allowEmpty)
        {
            XmlNode node2 = node.Attributes.RemoveNamedItem(name);
            if (node2 == null)
            {
                if (required)
                {
                    throw ADP.ConfigRequiredAttributeMissing(name, node);
                }
                return null;
            }
            string str = node2.Value;
            if (!allowEmpty && (str.Length == 0))
            {
                throw ADP.ConfigRequiredAttributeEmpty(name, node);
            }
            return str;
        }
    }
}

