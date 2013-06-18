namespace System.EnterpriseServices.Internal
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Xml;

    internal class DiscoFile
    {
        public void AddElement(string FilePath, string SoapPageRef)
        {
            try
            {
                new SecurityPermission(SecurityPermissionFlag.RemotingConfiguration).Demand();
                XmlDocument document = new XmlDocument();
                document.Load(FilePath);
                XmlNode documentElement = document.DocumentElement;
                while (documentElement.Name != "discovery")
                {
                    documentElement = documentElement.NextSibling;
                }
                XmlNodeList list = documentElement.SelectNodes("descendant::*[attribute::ref='" + SoapPageRef + "']");
                while ((list != null) && (list.Count > 0))
                {
                    XmlNode oldChild = list.Item(0);
                    if (oldChild.ParentNode != null)
                    {
                        oldChild.ParentNode.RemoveChild(oldChild);
                        list = documentElement.SelectNodes("descendant::*[attribute::ref='" + SoapPageRef + "']");
                    }
                }
                list = documentElement.SelectNodes("descendant::*[attribute::address='" + SoapPageRef + "']");
                while ((list != null) && (list.Count > 0))
                {
                    XmlNode node3 = list.Item(0);
                    if (node3.ParentNode != null)
                    {
                        node3.ParentNode.RemoveChild(node3);
                        list = documentElement.SelectNodes("descendant::*[attribute::address='" + SoapPageRef + "']");
                    }
                }
                XmlElement newChild = document.CreateElement("", "contractRef", "");
                newChild.SetAttribute("ref", SoapPageRef);
                newChild.SetAttribute("docRef", SoapPageRef);
                newChild.SetAttribute("xmlns", "http://schemas.xmlsoap.org/disco/scl/");
                documentElement.AppendChild(newChild);
                XmlElement element2 = document.CreateElement("", "soap", "");
                element2.SetAttribute("address", SoapPageRef);
                element2.SetAttribute("xmlns", "http://schemas.xmlsoap.org/disco/soap/");
                documentElement.AppendChild(element2);
                document.Save(FilePath);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception.ToString());
            }
        }

        public void Create(string FilePath, string DiscoRef)
        {
            try
            {
                if (!FilePath.EndsWith("/", StringComparison.Ordinal) && !FilePath.EndsWith(@"\", StringComparison.Ordinal))
                {
                    FilePath = FilePath + @"\";
                }
                if (!File.Exists(FilePath + DiscoRef))
                {
                    new SecurityPermission(SecurityPermissionFlag.RemotingConfiguration).Demand();
                    string str = "<?xml version=\"1.0\" ?>\n";
                    str = str + "<discovery xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://schemas.xmlsoap.org/disco/\">\n" + "</discovery>\n";
                    FileStream stream = new FileStream(FilePath + DiscoRef, FileMode.Create);
                    StreamWriter writer = new StreamWriter(stream);
                    writer.Write(str);
                    writer.Close();
                    stream.Close();
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception.ToString());
            }
        }

        internal void DeleteElement(string FilePath, string SoapPageRef)
        {
            try
            {
                new SecurityPermission(SecurityPermissionFlag.RemotingConfiguration).Demand();
                XmlDocument document = new XmlDocument();
                document.Load(FilePath);
                XmlNode documentElement = document.DocumentElement;
                while (documentElement.Name != "discovery")
                {
                    documentElement = documentElement.NextSibling;
                }
                XmlNodeList list = documentElement.SelectNodes("descendant::*[attribute::ref='" + SoapPageRef + "']");
                while ((list != null) && (list.Count > 0))
                {
                    XmlNode oldChild = list.Item(0);
                    if (oldChild.ParentNode != null)
                    {
                        oldChild.ParentNode.RemoveChild(oldChild);
                        list = documentElement.SelectNodes("descendant::*[attribute::ref='" + SoapPageRef + "']");
                    }
                }
                list = documentElement.SelectNodes("descendant::*[attribute::address='" + SoapPageRef + "']");
                while ((list != null) && (list.Count > 0))
                {
                    XmlNode node3 = list.Item(0);
                    if (node3.ParentNode != null)
                    {
                        node3.ParentNode.RemoveChild(node3);
                        list = documentElement.SelectNodes("descendant::*[attribute::address='" + SoapPageRef + "']");
                    }
                }
                document.Save(FilePath);
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception.ToString());
            }
        }
    }
}

