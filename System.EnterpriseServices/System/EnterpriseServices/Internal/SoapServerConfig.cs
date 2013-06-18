namespace System.EnterpriseServices.Internal
{
    using System;
    using System.EnterpriseServices;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    internal static class SoapServerConfig
    {
        internal static void AddComponent(string filePath, string assemblyName, string typeName, string progId, string assemblyFile, string wkoMode, bool wellKnown, bool clientActivated)
        {
            try
            {
                AssemblyManager manager = new AssemblyManager();
                string str = typeName + ", " + manager.GetFullName(assemblyFile);
                string str2 = typeName + ", " + assemblyName;
                XmlDocument configXml = new XmlDocument();
                configXml.Load(filePath);
                XmlNode documentElement = configXml.DocumentElement;
                documentElement = FindOrCreateElement(configXml, documentElement, "system.runtime.remoting");
                documentElement = FindOrCreateElement(configXml, documentElement, "application");
                documentElement = FindOrCreateElement(configXml, documentElement, "service");
                XmlNodeList list = documentElement.SelectNodes("descendant::*[attribute::type='" + str2 + "']");
                while ((list != null) && (list.Count > 0))
                {
                    XmlNode oldChild = list.Item(0);
                    if (oldChild.ParentNode != null)
                    {
                        oldChild.ParentNode.RemoveChild(oldChild);
                        list = documentElement.SelectNodes("descendant::*[attribute::type='" + str2 + "']");
                    }
                }
                list = documentElement.SelectNodes("descendant::*[attribute::type='" + str + "']");
                while ((list != null) && (list.Count > 0))
                {
                    XmlNode node3 = list.Item(0);
                    if (node3.ParentNode != null)
                    {
                        node3.ParentNode.RemoveChild(node3);
                        list = documentElement.SelectNodes("descendant::*[attribute::type='" + str + "']");
                    }
                }
                if (wellKnown)
                {
                    XmlElement newChild = configXml.CreateElement("wellknown");
                    newChild.SetAttribute("mode", wkoMode);
                    newChild.SetAttribute("type", str);
                    newChild.SetAttribute("objectUri", progId + ".soap");
                    documentElement.AppendChild(newChild);
                }
                if (clientActivated)
                {
                    XmlElement element2 = configXml.CreateElement("activated");
                    element2.SetAttribute("type", str2);
                    documentElement.AppendChild(element2);
                }
                configXml.Save(filePath);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(Resource.FormatString("Soap_ConfigAdditionFailure") + " " + exception.Message);
                throw;
            }
        }

        internal static bool ChangeSecuritySettings(string fileName, bool impersonate, bool authentication)
        {
            if (!File.Exists(fileName))
            {
                return false;
            }
            XmlDocument configXml = new XmlDocument();
            configXml.Load(fileName);
            bool flag = UpdateChannels(configXml);
            if (flag)
            {
                flag = UpdateSystemWeb(configXml, impersonate, authentication);
                try
                {
                    if (flag)
                    {
                        configXml.Save(fileName);
                    }
                }
                catch (Exception exception)
                {
                    if ((exception is NullReferenceException) || (exception is SEHException))
                    {
                        throw;
                    }
                    ComSoapPublishError.Report(Resource.FormatString("Soap_WebConfigFailed"));
                    throw;
                }
            }
            if (!flag)
            {
                ComSoapPublishError.Report(Resource.FormatString("Soap_WebConfigFailed"));
            }
            return flag;
        }

        internal static bool Create(string inFilePath, bool impersonate, bool windowsAuth)
        {
            string str = inFilePath;
            if (str.Length <= 0)
            {
                return false;
            }
            if (!str.EndsWith("/", StringComparison.Ordinal) && !str.EndsWith(@"\", StringComparison.Ordinal))
            {
                str = str + @"\";
            }
            string path = str + "web.config";
            if (!File.Exists(path))
            {
                XmlTextWriter writer = new XmlTextWriter(path, new UTF8Encoding()) {
                    Formatting = Formatting.Indented
                };
                writer.WriteStartDocument();
                writer.WriteStartElement("configuration");
                writer.Flush();
                writer.Close();
            }
            return ChangeSecuritySettings(path, impersonate, windowsAuth);
        }

        internal static void DeleteComponent(string filePath, string assemblyName, string typeName, string progId, string assemblyFile)
        {
            try
            {
                AssemblyManager manager = new AssemblyManager();
                string str = typeName + ", " + manager.GetFullName(assemblyFile);
                string str2 = typeName + ", " + assemblyName;
                XmlDocument configXml = new XmlDocument();
                configXml.Load(filePath);
                XmlNode documentElement = configXml.DocumentElement;
                documentElement = FindOrCreateElement(configXml, documentElement, "system.runtime.remoting");
                documentElement = FindOrCreateElement(configXml, documentElement, "application");
                documentElement = FindOrCreateElement(configXml, documentElement, "service");
                XmlNodeList list = documentElement.SelectNodes("descendant::*[attribute::type='" + str2 + "']");
                while ((list != null) && (list.Count > 0))
                {
                    XmlNode oldChild = list.Item(0);
                    if (oldChild.ParentNode != null)
                    {
                        oldChild.ParentNode.RemoveChild(oldChild);
                        list = documentElement.SelectNodes("descendant::*[attribute::type='" + str2 + "']");
                    }
                }
                list = documentElement.SelectNodes("descendant::*[attribute::type='" + str + "']");
                while ((list != null) && (list.Count > 0))
                {
                    XmlNode node3 = list.Item(0);
                    if (node3.ParentNode != null)
                    {
                        node3.ParentNode.RemoveChild(node3);
                        list = documentElement.SelectNodes("descendant::*[attribute::type='" + str + "']");
                    }
                }
                configXml.Save(filePath);
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch (RegistrationException)
            {
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(Resource.FormatString("Soap_ConfigDeletionFailure") + " " + exception.Message);
                throw;
            }
        }

        internal static XmlElement FindOrCreateElement(XmlDocument configXml, XmlNode node, string elemName)
        {
            XmlNodeList list = node.SelectNodes(elemName);
            if (list.Count == 0)
            {
                XmlElement newChild = configXml.CreateElement(elemName);
                node.AppendChild(newChild);
                return newChild;
            }
            return (XmlElement) list[0];
        }

        internal static bool UpdateChannels(XmlDocument configXml)
        {
            XmlNode documentElement = configXml.DocumentElement;
            XmlElement node = FindOrCreateElement(configXml, documentElement, "system.runtime.remoting");
            node = FindOrCreateElement(configXml, node, "application");
            node = FindOrCreateElement(configXml, node, "channels");
            FindOrCreateElement(configXml, node, "channel").SetAttribute("ref", "http server");
            return true;
        }

        internal static bool UpdateSystemWeb(XmlDocument configXml, bool impersonate, bool authentication)
        {
            XmlNode documentElement = configXml.DocumentElement;
            XmlElement node = FindOrCreateElement(configXml, documentElement, "system.web");
            if (impersonate)
            {
                FindOrCreateElement(configXml, node, "identity").SetAttribute("impersonate", "true");
            }
            if (authentication)
            {
                FindOrCreateElement(configXml, node, "authentication").SetAttribute("mode", "Windows");
            }
            return true;
        }
    }
}

