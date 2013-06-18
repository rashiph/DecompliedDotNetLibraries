namespace System.Data.Common
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Xml;

    public class DbProviderConfigurationHandler : IConfigurationSectionHandler
    {
        internal const string settings = "settings";

        internal static NameValueCollection CloneParent(NameValueCollection parentConfig)
        {
            if (parentConfig == null)
            {
                parentConfig = new NameValueCollection();
                return parentConfig;
            }
            parentConfig = new NameValueCollection(parentConfig);
            return parentConfig;
        }

        public virtual object Create(object parent, object configContext, XmlNode section)
        {
            return CreateStatic(parent, configContext, section);
        }

        internal static object CreateStatic(object parent, object configContext, XmlNode section)
        {
            object obj2 = parent;
            if (section != null)
            {
                obj2 = CloneParent(parent as NameValueCollection);
                bool flag = false;
                System.Data.Common.HandlerBase.CheckForUnrecognizedAttributes(section);
                foreach (XmlNode node in section.ChildNodes)
                {
                    if (!System.Data.Common.HandlerBase.IsIgnorableAlsoCheckForNonElement(node))
                    {
                        string str;
                        if (((str = node.Name) == null) || !(str == "settings"))
                        {
                            throw ADP.ConfigUnrecognizedElement(node);
                        }
                        if (flag)
                        {
                            throw ADP.ConfigSectionsUnique("settings");
                        }
                        flag = true;
                        DbProviderDictionarySectionHandler.CreateStatic(obj2 as NameValueCollection, configContext, node);
                    }
                }
            }
            return obj2;
        }

        internal static string RemoveAttribute(XmlNode node, string name)
        {
            XmlNode node2 = node.Attributes.RemoveNamedItem(name);
            if (node2 == null)
            {
                throw ADP.ConfigRequiredAttributeMissing(name, node);
            }
            string str = node2.Value;
            if (str.Length == 0)
            {
                throw ADP.ConfigRequiredAttributeEmpty(name, node);
            }
            return str;
        }

        private sealed class DbProviderDictionarySectionHandler
        {
            internal static NameValueCollection CreateStatic(NameValueCollection config, object context, XmlNode section)
            {
                if (section != null)
                {
                    System.Data.Common.HandlerBase.CheckForUnrecognizedAttributes(section);
                }
                foreach (XmlNode node in section.ChildNodes)
                {
                    if (!System.Data.Common.HandlerBase.IsIgnorableAlsoCheckForNonElement(node))
                    {
                        string name = node.Name;
                        if (name == null)
                        {
                            goto Label_0079;
                        }
                        if (!(name == "add"))
                        {
                            if (name == "remove")
                            {
                                goto Label_0067;
                            }
                            if (name == "clear")
                            {
                                goto Label_0070;
                            }
                            goto Label_0079;
                        }
                        HandleAdd(node, config);
                    }
                    continue;
                Label_0067:
                    HandleRemove(node, config);
                    continue;
                Label_0070:
                    HandleClear(node, config);
                    continue;
                Label_0079:
                    throw ADP.ConfigUnrecognizedElement(node);
                }
                return config;
            }

            private static void HandleAdd(XmlNode child, NameValueCollection config)
            {
                System.Data.Common.HandlerBase.CheckForChildNodes(child);
                string name = DbProviderConfigurationHandler.RemoveAttribute(child, "name");
                string str = DbProviderConfigurationHandler.RemoveAttribute(child, "value");
                System.Data.Common.HandlerBase.CheckForUnrecognizedAttributes(child);
                config.Add(name, str);
            }

            private static void HandleClear(XmlNode child, NameValueCollection config)
            {
                System.Data.Common.HandlerBase.CheckForChildNodes(child);
                System.Data.Common.HandlerBase.CheckForUnrecognizedAttributes(child);
                config.Clear();
            }

            private static void HandleRemove(XmlNode child, NameValueCollection config)
            {
                System.Data.Common.HandlerBase.CheckForChildNodes(child);
                string name = DbProviderConfigurationHandler.RemoveAttribute(child, "name");
                System.Data.Common.HandlerBase.CheckForUnrecognizedAttributes(child);
                config.Remove(name);
            }
        }
    }
}

