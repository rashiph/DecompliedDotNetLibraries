namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml;

    internal class Utils
    {
        private Utils()
        {
        }

        internal static void AddNamespaces(XmlElement elem, Hashtable namespaces)
        {
            if (namespaces != null)
            {
                foreach (string str in namespaces.Keys)
                {
                    if (!elem.HasAttribute(str))
                    {
                        XmlAttribute newAttr = elem.OwnerDocument.CreateAttribute(str);
                        newAttr.Value = namespaces[str] as string;
                        elem.SetAttributeNode(newAttr);
                    }
                }
            }
        }

        internal static void AddNamespaces(XmlElement elem, CanonicalXmlNodeList namespaces)
        {
            if (namespaces != null)
            {
                foreach (XmlNode node in namespaces)
                {
                    string name = (node.Prefix.Length > 0) ? (node.Prefix + ":" + node.LocalName) : node.LocalName;
                    if (!elem.HasAttribute(name) && (!name.Equals("xmlns") || (elem.Prefix.Length != 0)))
                    {
                        XmlAttribute newAttr = elem.OwnerDocument.CreateAttribute(name);
                        newAttr.Value = node.Value;
                        elem.SetAttributeNode(newAttr);
                    }
                }
            }
        }

        internal static XmlNodeList AllDescendantNodes(XmlNode node, bool includeComments)
        {
            CanonicalXmlNodeList list = new CanonicalXmlNodeList();
            CanonicalXmlNodeList list2 = new CanonicalXmlNodeList();
            CanonicalXmlNodeList list3 = new CanonicalXmlNodeList();
            CanonicalXmlNodeList list4 = new CanonicalXmlNodeList();
            int num = 0;
            list2.Add(node);
            do
            {
                XmlNode node2 = list2[num];
                XmlNodeList childNodes = node2.ChildNodes;
                if (childNodes != null)
                {
                    foreach (XmlNode node3 in childNodes)
                    {
                        if (includeComments || !(node3 is XmlComment))
                        {
                            list2.Add(node3);
                        }
                    }
                }
                if (node2.Attributes != null)
                {
                    foreach (XmlNode node4 in node2.Attributes)
                    {
                        if ((node4.LocalName == "xmlns") || (node4.Prefix == "xmlns"))
                        {
                            list4.Add(node4);
                        }
                        else
                        {
                            list3.Add(node4);
                        }
                    }
                }
                num++;
            }
            while (num < list2.Count);
            foreach (XmlNode node5 in list2)
            {
                list.Add(node5);
            }
            foreach (XmlNode node6 in list3)
            {
                list.Add(node6);
            }
            foreach (XmlNode node7 in list4)
            {
                list.Add(node7);
            }
            return list;
        }

        [SecuritySafeCritical]
        internal static X509Certificate2Collection BuildBagOfCerts(KeyInfoX509Data keyInfoX509Data, CertUsageType certUsageType)
        {
            X509Certificate2Collection certificates = new X509Certificate2Collection();
            ArrayList list = (certUsageType == CertUsageType.Decryption) ? new ArrayList() : null;
            if (keyInfoX509Data.Certificates != null)
            {
                foreach (X509Certificate2 certificate in keyInfoX509Data.Certificates)
                {
                    switch (certUsageType)
                    {
                        case CertUsageType.Verification:
                            certificates.Add(certificate);
                            break;

                        case CertUsageType.Decryption:
                            list.Add(new X509IssuerSerial(certificate.IssuerName.Name, certificate.SerialNumber));
                            break;
                    }
                }
            }
            if (((keyInfoX509Data.SubjectNames != null) || (keyInfoX509Data.IssuerSerials != null)) || ((keyInfoX509Data.SubjectKeyIds != null) || (list != null)))
            {
                new StorePermission(StorePermissionFlags.OpenStore).Assert();
                X509Store[] storeArray = new X509Store[2];
                string storeName = (certUsageType == CertUsageType.Verification) ? "AddressBook" : "My";
                storeArray[0] = new X509Store(storeName, StoreLocation.CurrentUser);
                storeArray[1] = new X509Store(storeName, StoreLocation.LocalMachine);
                for (int i = 0; i < storeArray.Length; i++)
                {
                    if (storeArray[i] != null)
                    {
                        X509Certificate2Collection certificates2 = null;
                        try
                        {
                            storeArray[i].Open(OpenFlags.OpenExistingOnly);
                            certificates2 = storeArray[i].Certificates;
                            storeArray[i].Close();
                            if (keyInfoX509Data.SubjectNames != null)
                            {
                                foreach (string str2 in keyInfoX509Data.SubjectNames)
                                {
                                    certificates2 = certificates2.Find(X509FindType.FindBySubjectDistinguishedName, str2, false);
                                }
                            }
                            if (keyInfoX509Data.IssuerSerials != null)
                            {
                                foreach (X509IssuerSerial serial in keyInfoX509Data.IssuerSerials)
                                {
                                    certificates2 = certificates2.Find(X509FindType.FindByIssuerDistinguishedName, serial.IssuerName, false);
                                    certificates2 = certificates2.Find(X509FindType.FindBySerialNumber, serial.SerialNumber, false);
                                }
                            }
                            if (keyInfoX509Data.SubjectKeyIds != null)
                            {
                                foreach (byte[] buffer in keyInfoX509Data.SubjectKeyIds)
                                {
                                    string findValue = System.Security.Cryptography.X509Certificates.X509Utils.EncodeHexString(buffer);
                                    certificates2 = certificates2.Find(X509FindType.FindBySubjectKeyIdentifier, findValue, false);
                                }
                            }
                            if (list != null)
                            {
                                foreach (X509IssuerSerial serial2 in list)
                                {
                                    certificates2 = certificates2.Find(X509FindType.FindByIssuerDistinguishedName, serial2.IssuerName, false);
                                    certificates2 = certificates2.Find(X509FindType.FindBySerialNumber, serial2.SerialNumber, false);
                                }
                            }
                        }
                        catch (CryptographicException)
                        {
                        }
                        if (certificates2 != null)
                        {
                            certificates.AddRange(certificates2);
                        }
                    }
                }
            }
            return certificates;
        }

        internal static byte[] ConvertIntToByteArray(int dwInput)
        {
            byte[] buffer = new byte[8];
            int index = 0;
            if (dwInput == 0)
            {
                return new byte[1];
            }
            int num = dwInput;
            while (num > 0)
            {
                int num2 = num % 0x100;
                buffer[index] = (byte) num2;
                num = (num - num2) / 0x100;
                index++;
            }
            byte[] buffer2 = new byte[index];
            for (int i = 0; i < index; i++)
            {
                buffer2[i] = buffer[(index - i) - 1];
            }
            return buffer2;
        }

        internal static XmlDocument DiscardComments(XmlDocument document)
        {
            XmlNodeList list = document.SelectNodes("//comment()");
            if (list != null)
            {
                foreach (XmlNode node in list)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }
            return document;
        }

        internal static string DiscardWhiteSpaces(string inputBuffer)
        {
            return DiscardWhiteSpaces(inputBuffer, 0, inputBuffer.Length);
        }

        internal static string DiscardWhiteSpaces(string inputBuffer, int inputOffset, int inputCount)
        {
            int num;
            int num2 = 0;
            for (num = 0; num < inputCount; num++)
            {
                if (char.IsWhiteSpace(inputBuffer[inputOffset + num]))
                {
                    num2++;
                }
            }
            char[] chArray = new char[inputCount - num2];
            num2 = 0;
            for (num = 0; num < inputCount; num++)
            {
                if (!char.IsWhiteSpace(inputBuffer[inputOffset + num]))
                {
                    chArray[num2++] = inputBuffer[inputOffset + num];
                }
            }
            return new string(chArray);
        }

        internal static string EscapeAttributeValue(string value)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(value);
            sb.Replace("&", "&amp;");
            sb.Replace("<", "&lt;");
            sb.Replace("\"", "&quot;");
            SBReplaceCharWithString(sb, '\t', "&#x9;");
            SBReplaceCharWithString(sb, '\n', "&#xA;");
            SBReplaceCharWithString(sb, '\r', "&#xD;");
            return sb.ToString();
        }

        internal static string EscapeCData(string data)
        {
            return EscapeTextData(data);
        }

        internal static string EscapeTextData(string data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(data);
            sb.Replace("&", "&amp;");
            sb.Replace("<", "&lt;");
            sb.Replace(">", "&gt;");
            SBReplaceCharWithString(sb, '\r', "&#xD;");
            return sb.ToString();
        }

        internal static string EscapeWhitespaceData(string data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(data);
            SBReplaceCharWithString(sb, '\r', "&#xD;");
            return sb.ToString();
        }

        internal static string ExtractIdFromLocalUri(string uri)
        {
            string str = uri.Substring(1);
            if (!str.StartsWith("xpointer(id(", StringComparison.Ordinal))
            {
                return str;
            }
            int index = str.IndexOf("id(", StringComparison.Ordinal);
            int num2 = str.IndexOf(")", StringComparison.Ordinal);
            if ((num2 < 0) || (num2 < (index + 3)))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidReference"));
            }
            return str.Substring(index + 3, (num2 - index) - 3).Replace("'", "").Replace("\"", "");
        }

        internal static string GetAttribute(XmlElement element, string localName, string namespaceURI)
        {
            string attribute = element.HasAttribute(localName) ? element.GetAttribute(localName) : null;
            if ((attribute == null) && element.HasAttribute(localName, namespaceURI))
            {
                attribute = element.GetAttribute(localName, namespaceURI);
            }
            return attribute;
        }

        internal static int GetHexArraySize(byte[] hex)
        {
            int length = hex.Length;
            while (length-- > 0)
            {
                if (hex[length] != 0)
                {
                    break;
                }
            }
            return (length + 1);
        }

        internal static string GetIdFromLocalUri(string uri, out bool discardComments)
        {
            string str = uri.Substring(1);
            discardComments = true;
            if (str.StartsWith("xpointer(id(", StringComparison.Ordinal))
            {
                int index = str.IndexOf("id(", StringComparison.Ordinal);
                int num2 = str.IndexOf(")", StringComparison.Ordinal);
                if ((num2 < 0) || (num2 < (index + 3)))
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidReference"));
                }
                str = str.Substring(index + 3, (num2 - index) - 3).Replace("'", "").Replace("\"", "");
                discardComments = false;
            }
            return str;
        }

        internal static string GetNamespacePrefix(XmlAttribute a)
        {
            if (a.Prefix.Length != 0)
            {
                return a.LocalName;
            }
            return string.Empty;
        }

        internal static XmlDocument GetOwnerDocument(XmlNodeList nodeList)
        {
            foreach (XmlNode node in nodeList)
            {
                if (node.OwnerDocument != null)
                {
                    return node.OwnerDocument;
                }
            }
            return null;
        }

        internal static CanonicalXmlNodeList GetPropagatedAttributes(XmlElement elem)
        {
            if (elem == null)
            {
                return null;
            }
            CanonicalXmlNodeList list = new CanonicalXmlNodeList();
            XmlNode parentNode = elem;
            if (parentNode == null)
            {
                return null;
            }
            bool flag = true;
            while (parentNode != null)
            {
                XmlElement element = parentNode as XmlElement;
                if (element == null)
                {
                    parentNode = parentNode.ParentNode;
                }
                else
                {
                    if (!IsCommittedNamespace(element, element.Prefix, element.NamespaceURI) && !IsRedundantNamespace(element, element.Prefix, element.NamespaceURI))
                    {
                        string name = (element.Prefix.Length > 0) ? ("xmlns:" + element.Prefix) : "xmlns";
                        XmlAttribute attribute = elem.OwnerDocument.CreateAttribute(name);
                        attribute.Value = element.NamespaceURI;
                        list.Add(attribute);
                    }
                    if (element.HasAttributes)
                    {
                        foreach (XmlAttribute attribute2 in element.Attributes)
                        {
                            if (flag && (attribute2.LocalName == "xmlns"))
                            {
                                XmlAttribute attribute3 = elem.OwnerDocument.CreateAttribute("xmlns");
                                attribute3.Value = attribute2.Value;
                                list.Add(attribute3);
                                flag = false;
                            }
                            else if ((attribute2.Prefix == "xmlns") || (attribute2.Prefix == "xml"))
                            {
                                list.Add(attribute2);
                            }
                            else if (((attribute2.NamespaceURI.Length > 0) && !IsCommittedNamespace(element, attribute2.Prefix, attribute2.NamespaceURI)) && !IsRedundantNamespace(element, attribute2.Prefix, attribute2.NamespaceURI))
                            {
                                string str2 = (attribute2.Prefix.Length > 0) ? ("xmlns:" + attribute2.Prefix) : "xmlns";
                                XmlAttribute attribute4 = elem.OwnerDocument.CreateAttribute(str2);
                                attribute4.Value = attribute2.NamespaceURI;
                                list.Add(attribute4);
                            }
                        }
                    }
                    parentNode = parentNode.ParentNode;
                }
            }
            return list;
        }

        internal static bool HasAttribute(XmlElement element, string localName, string namespaceURI)
        {
            if (!element.HasAttribute(localName))
            {
                return element.HasAttribute(localName, namespaceURI);
            }
            return true;
        }

        private static bool HasNamespace(XmlElement element, string prefix, string value)
        {
            return (IsCommittedNamespace(element, prefix, value) || ((element.Prefix == prefix) && (element.NamespaceURI == value)));
        }

        internal static bool HasNamespacePrefix(XmlAttribute a, string nsPrefix)
        {
            return GetNamespacePrefix(a).Equals(nsPrefix);
        }

        internal static bool IsCommittedNamespace(XmlElement element, string prefix, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            string name = (prefix.Length > 0) ? ("xmlns:" + prefix) : "xmlns";
            return (element.HasAttribute(name) && (element.GetAttribute(name) == value));
        }

        internal static bool IsDefaultNamespaceNode(XmlNode n)
        {
            bool flag = ((n.NodeType == XmlNodeType.Attribute) && (n.Prefix.Length == 0)) && n.LocalName.Equals("xmlns");
            bool flag2 = IsXmlNamespaceNode(n);
            if (!flag)
            {
                return flag2;
            }
            return true;
        }

        internal static bool IsEmptyDefaultNamespaceNode(XmlNode n)
        {
            return (IsDefaultNamespaceNode(n) && (n.Value.Length == 0));
        }

        internal static bool IsNamespaceNode(XmlNode n)
        {
            if (n.NodeType != XmlNodeType.Attribute)
            {
                return false;
            }
            return (n.Prefix.Equals("xmlns") || ((n.Prefix.Length == 0) && n.LocalName.Equals("xmlns")));
        }

        internal static bool IsNonRedundantNamespaceDecl(XmlAttribute a, XmlAttribute nearestAncestorWithSamePrefix)
        {
            if (nearestAncestorWithSamePrefix == null)
            {
                return !IsEmptyDefaultNamespaceNode(a);
            }
            return !nearestAncestorWithSamePrefix.Value.Equals(a.Value);
        }

        internal static bool IsRedundantNamespace(XmlElement element, string prefix, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            for (XmlNode node = element.ParentNode; node != null; node = node.ParentNode)
            {
                XmlElement element2 = node as XmlElement;
                if ((element2 != null) && HasNamespace(element2, prefix, value))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsXmlNamespaceNode(XmlNode n)
        {
            return ((n.NodeType == XmlNodeType.Attribute) && n.Prefix.Equals("xml"));
        }

        internal static bool IsXmlPrefixDefinitionNode(XmlAttribute a)
        {
            return false;
        }

        internal static bool NodeInList(XmlNode node, XmlNodeList nodeList)
        {
            foreach (XmlNode node2 in nodeList)
            {
                if (node2 == node)
                {
                    return true;
                }
            }
            return false;
        }

        internal static XmlDocument PreProcessDocumentInput(XmlDocument document, XmlResolver xmlResolver, string baseUri)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            MyXmlDocument document2 = new MyXmlDocument {
                PreserveWhitespace = document.PreserveWhitespace
            };
            using (TextReader reader = new StringReader(document.OuterXml))
            {
                XmlReaderSettings settings = new XmlReaderSettings {
                    XmlResolver = xmlResolver,
                    DtdProcessing = DtdProcessing.Parse
                };
                XmlReader reader2 = XmlReader.Create(reader, settings, baseUri);
                document2.Load(reader2);
            }
            return document2;
        }

        internal static XmlDocument PreProcessElementInput(XmlElement elem, XmlResolver xmlResolver, string baseUri)
        {
            if (elem == null)
            {
                throw new ArgumentNullException("elem");
            }
            MyXmlDocument document = new MyXmlDocument {
                PreserveWhitespace = true
            };
            using (TextReader reader = new StringReader(elem.OuterXml))
            {
                XmlReaderSettings settings = new XmlReaderSettings {
                    XmlResolver = xmlResolver,
                    DtdProcessing = DtdProcessing.Parse
                };
                XmlReader reader2 = XmlReader.Create(reader, settings, baseUri);
                document.Load(reader2);
            }
            return document;
        }

        internal static XmlReader PreProcessStreamInput(Stream inputStream, XmlResolver xmlResolver, string baseUri)
        {
            XmlReaderSettings settings = new XmlReaderSettings {
                XmlResolver = xmlResolver,
                DtdProcessing = DtdProcessing.Parse
            };
            return XmlReader.Create(inputStream, settings, baseUri);
        }

        internal static long Pump(Stream input, Stream output)
        {
            int num;
            MemoryStream stream = input as MemoryStream;
            if ((stream != null) && (stream.Position == 0L))
            {
                stream.WriteTo(output);
                return stream.Length;
            }
            byte[] buffer = new byte[0x1000];
            long num2 = 0L;
            while ((num = input.Read(buffer, 0, 0x1000)) > 0)
            {
                output.Write(buffer, 0, num);
                num2 += num;
            }
            return num2;
        }

        internal static void RemoveAllChildren(XmlElement inputElement)
        {
            XmlNode firstChild = inputElement.FirstChild;
            XmlNode nextSibling = null;
            while (firstChild != null)
            {
                nextSibling = firstChild.NextSibling;
                inputElement.RemoveChild(firstChild);
                firstChild = nextSibling;
            }
        }

        internal static void SBReplaceCharWithString(StringBuilder sb, char oldChar, string newString)
        {
            int startIndex = 0;
            int length = newString.Length;
            while (startIndex < sb.Length)
            {
                if (sb[startIndex] == oldChar)
                {
                    sb.Remove(startIndex, 1);
                    sb.Insert(startIndex, newString);
                    startIndex += length;
                }
                else
                {
                    startIndex++;
                }
            }
        }

        internal static Hashtable TokenizePrefixListString(string s)
        {
            Hashtable hashtable = new Hashtable();
            if (s != null)
            {
                foreach (string str in s.Split(null))
                {
                    if (str.Equals("#default"))
                    {
                        hashtable.Add(string.Empty, true);
                    }
                    else if (str.Length > 0)
                    {
                        hashtable.Add(str, true);
                    }
                }
            }
            return hashtable;
        }
    }
}

