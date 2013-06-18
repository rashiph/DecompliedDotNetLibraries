namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    [ComVisible(false)]
    public sealed class TrustInfo
    {
        private System.Security.PermissionSet _inputPermissionSet;
        private XmlDocument _inputTrustInfoDocument;
        private bool _isFullTrust = true;
        private System.Security.PermissionSet _outputPermissionSet;
        private bool _preserveFullTrustPermissionSet;
        private bool sameSiteChanged;
        private string sameSiteSetting = "site";

        private void AddSameSiteAttribute(XmlElement permissionSetElement)
        {
            XmlAttribute namedItem = (XmlAttribute) permissionSetElement.Attributes.GetNamedItem(XmlUtil.TrimPrefix("asmv2:SameSite"));
            if (namedItem == null)
            {
                namedItem = permissionSetElement.OwnerDocument.CreateAttribute(XmlUtil.TrimPrefix("asmv2:SameSite"));
                permissionSetElement.Attributes.Append(namedItem);
            }
            namedItem.Value = this.sameSiteSetting;
        }

        public void Clear()
        {
            this._inputPermissionSet = null;
            this._inputTrustInfoDocument = null;
            this._isFullTrust = true;
            this._outputPermissionSet = null;
        }

        private void FixupPermissionSetElement(XmlElement permissionSetElement)
        {
            XmlDocument ownerDocument = permissionSetElement.OwnerDocument;
            XmlNamespaceManager namespaceManager = XmlNamespaces.GetNamespaceManager(ownerDocument.NameTable);
            if (this._preserveFullTrustPermissionSet)
            {
                XmlAttribute node = (XmlAttribute) permissionSetElement.Attributes.GetNamedItem(XmlUtil.TrimPrefix("asmv2:Unrestricted"));
                if (this._isFullTrust)
                {
                    if (node == null)
                    {
                        node = ownerDocument.CreateAttribute(XmlUtil.TrimPrefix("asmv2:Unrestricted"));
                        permissionSetElement.Attributes.Append(node);
                    }
                    node.Value = "true";
                }
                else if (node != null)
                {
                    permissionSetElement.Attributes.RemoveNamedItem(XmlUtil.TrimPrefix("asmv2:Unrestricted"));
                }
            }
            else if (this._isFullTrust)
            {
                XmlAttribute attribute2 = (XmlAttribute) permissionSetElement.Attributes.GetNamedItem(XmlUtil.TrimPrefix("asmv2:Unrestricted"));
                if (attribute2 == null)
                {
                    attribute2 = ownerDocument.CreateAttribute(XmlUtil.TrimPrefix("asmv2:Unrestricted"));
                    permissionSetElement.Attributes.Append(attribute2);
                }
                attribute2.Value = "true";
                while (permissionSetElement.FirstChild != null)
                {
                    permissionSetElement.RemoveChild(permissionSetElement.FirstChild);
                }
            }
            XmlAttribute namedItem = (XmlAttribute) permissionSetElement.Attributes.GetNamedItem(XmlUtil.TrimPrefix("asmv2:ID"));
            if (namedItem == null)
            {
                namedItem = ownerDocument.CreateAttribute(XmlUtil.TrimPrefix("asmv2:ID"));
                permissionSetElement.Attributes.Append(namedItem);
            }
            if (string.IsNullOrEmpty(namedItem.Value))
            {
                namedItem.Value = "Custom";
            }
            this.AddSameSiteAttribute(permissionSetElement);
            if ((permissionSetElement.ParentNode != null) && (permissionSetElement.ParentNode.NodeType != XmlNodeType.Document))
            {
                XmlAttribute attribute4 = null;
                XmlElement newChild = (XmlElement) permissionSetElement.ParentNode.SelectSingleNode("asmv2:defaultAssemblyRequest", namespaceManager);
                if (newChild == null)
                {
                    newChild = ownerDocument.CreateElement(XmlUtil.TrimPrefix("asmv2:defaultAssemblyRequest"), "urn:schemas-microsoft-com:asm.v2");
                    permissionSetElement.ParentNode.AppendChild(newChild);
                }
                attribute4 = (XmlAttribute) permissionSetElement.Attributes.GetNamedItem(XmlUtil.TrimPrefix("asmv2:permissionSetReference"));
                if (attribute4 == null)
                {
                    attribute4 = ownerDocument.CreateAttribute(XmlUtil.TrimPrefix("asmv2:permissionSetReference"));
                    newChild.Attributes.Append(attribute4);
                }
                if (string.Compare(namedItem.Value, attribute4.Value, StringComparison.Ordinal) != 0)
                {
                    attribute4.Value = namedItem.Value;
                }
            }
        }

        private System.Security.PermissionSet GetInputPermissionSet()
        {
            if (this._inputPermissionSet == null)
            {
                XmlElement inputPermissionSetElement = this.GetInputPermissionSetElement();
                if (this._preserveFullTrustPermissionSet)
                {
                    XmlAttribute namedItem = (XmlAttribute) inputPermissionSetElement.Attributes.GetNamedItem(XmlUtil.TrimPrefix("asmv2:Unrestricted"));
                    this._isFullTrust = (namedItem != null) && bool.Parse(namedItem.Value);
                    if (this._isFullTrust)
                    {
                        XmlDocument document = new XmlDocument();
                        document.AppendChild(document.ImportNode(inputPermissionSetElement, true));
                        document.DocumentElement.Attributes.RemoveNamedItem(XmlUtil.TrimPrefix("asmv2:Unrestricted"));
                        inputPermissionSetElement = document.DocumentElement;
                    }
                    this._inputPermissionSet = SecurityUtilities.XmlToPermissionSet(inputPermissionSetElement);
                }
                else
                {
                    this._inputPermissionSet = SecurityUtilities.XmlToPermissionSet(inputPermissionSetElement);
                    this._isFullTrust = this._inputPermissionSet.IsUnrestricted();
                }
            }
            return this._inputPermissionSet;
        }

        private XmlElement GetInputPermissionSetElement()
        {
            if (this._inputTrustInfoDocument == null)
            {
                this._inputTrustInfoDocument = new XmlDocument();
                XmlNamespaces.GetNamespaceManager(this._inputTrustInfoDocument.NameTable);
                XmlElement newChild = this._inputTrustInfoDocument.CreateElement(XmlUtil.TrimPrefix("asmv2:trustInfo"), "urn:schemas-microsoft-com:asm.v2");
                this._inputTrustInfoDocument.AppendChild(newChild);
            }
            return this.GetPermissionSetElement(this._inputTrustInfoDocument);
        }

        private XmlElement GetInputRequestedPrivilegeElement()
        {
            if (this._inputTrustInfoDocument == null)
            {
                return null;
            }
            XmlNamespaceManager namespaceManager = XmlNamespaces.GetNamespaceManager(this._inputTrustInfoDocument.NameTable);
            XmlElement documentElement = this._inputTrustInfoDocument.DocumentElement;
            if (documentElement == null)
            {
                return null;
            }
            XmlElement element2 = (XmlElement) documentElement.SelectSingleNode("asmv2:security", namespaceManager);
            if (element2 == null)
            {
                return null;
            }
            XmlElement element3 = (XmlElement) element2.SelectSingleNode("asmv3:requestedPrivileges", namespaceManager);
            if (element3 == null)
            {
                return null;
            }
            return element3;
        }

        private System.Security.PermissionSet GetOutputPermissionSet()
        {
            if (this._outputPermissionSet == null)
            {
                this._outputPermissionSet = this.GetInputPermissionSet();
            }
            return this._outputPermissionSet;
        }

        private XmlDocument GetOutputPermissionSetDocument()
        {
            return SecurityUtilities.PermissionSetToXml(this.GetOutputPermissionSet());
        }

        private XmlElement GetPermissionSetElement(XmlDocument document)
        {
            XmlNamespaceManager namespaceManager = XmlNamespaces.GetNamespaceManager(document.NameTable);
            XmlElement documentElement = document.DocumentElement;
            XmlElement newChild = (XmlElement) documentElement.SelectSingleNode("asmv2:security", namespaceManager);
            if (newChild == null)
            {
                newChild = document.CreateElement(XmlUtil.TrimPrefix("asmv2:security"), "urn:schemas-microsoft-com:asm.v2");
                documentElement.AppendChild(newChild);
            }
            XmlElement element3 = (XmlElement) newChild.SelectSingleNode("asmv2:applicationRequestMinimum", namespaceManager);
            if (element3 == null)
            {
                element3 = document.CreateElement(XmlUtil.TrimPrefix("asmv2:applicationRequestMinimum"), "urn:schemas-microsoft-com:asm.v2");
                newChild.AppendChild(element3);
            }
            XmlElement element4 = (XmlElement) element3.SelectSingleNode("asmv2:PermissionSet", namespaceManager);
            if (element4 == null)
            {
                element4 = document.CreateElement(XmlUtil.TrimPrefix("asmv2:PermissionSet"), "urn:schemas-microsoft-com:asm.v2");
                element3.AppendChild(element4);
                XmlAttribute node = document.CreateAttribute(XmlUtil.TrimPrefix("asmv2:Unrestricted"), "urn:schemas-microsoft-com:asm.v2");
                node.Value = this._isFullTrust.ToString().ToLowerInvariant();
                element4.Attributes.Append(node);
            }
            return element4;
        }

        private XmlElement GetRequestedPrivilegeElement(XmlElement inputRequestedPrivilegeElement, XmlDocument document)
        {
            XmlElement newChild = document.CreateElement(XmlUtil.TrimPrefix("asmv3:requestedPrivileges"), "urn:schemas-microsoft-com:asm.v3");
            document.AppendChild(newChild);
            string str = null;
            string str2 = null;
            string data = null;
            if (inputRequestedPrivilegeElement == null)
            {
                str = "asInvoker";
                str2 = "false";
                data = new ResourceManager("Microsoft.Build.Tasks.Deployment.ManifestUtilities.Strings", typeof(SecurityUtilities).Module.Assembly).GetString("TrustInfo.RequestedExecutionLevelComment");
            }
            else
            {
                XmlNamespaceManager namespaceManager = XmlNamespaces.GetNamespaceManager(document.NameTable);
                XmlElement element2 = (XmlElement) inputRequestedPrivilegeElement.SelectSingleNode("asmv3:requestedExecutionLevel", namespaceManager);
                if (element2 != null)
                {
                    XmlNode previousSibling = element2.PreviousSibling;
                    if ((previousSibling != null) && (previousSibling.NodeType == XmlNodeType.Comment))
                    {
                        data = ((XmlComment) previousSibling).Data;
                    }
                    if (element2.HasAttribute("level"))
                    {
                        str = element2.GetAttribute("level");
                    }
                    if (element2.HasAttribute("uiAccess"))
                    {
                        str2 = element2.GetAttribute("uiAccess");
                    }
                }
            }
            if (data != null)
            {
                XmlComment comment = document.CreateComment(data);
                newChild.AppendChild(comment);
            }
            if (str != null)
            {
                XmlElement element3 = document.CreateElement(XmlUtil.TrimPrefix("asmv3:requestedExecutionLevel"), "urn:schemas-microsoft-com:asm.v3");
                newChild.AppendChild(element3);
                XmlAttribute node = document.CreateAttribute("level");
                node.Value = str;
                element3.Attributes.Append(node);
                if (str2 != null)
                {
                    XmlAttribute attribute2 = document.CreateAttribute("uiAccess");
                    attribute2.Value = str2;
                    element3.Attributes.Append(attribute2);
                }
            }
            return newChild;
        }

        public void Read(Stream input)
        {
            this.Read(input, "/asmv2:trustInfo");
        }

        public void Read(string path)
        {
            using (Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                this.Read(stream);
            }
        }

        private void Read(Stream s, string xpath)
        {
            this.Clear();
            XmlDocument document = new XmlDocument();
            document.Load(s);
            XmlNamespaceManager namespaceManager = XmlNamespaces.GetNamespaceManager(document.NameTable);
            XmlElement element = (XmlElement) document.SelectSingleNode(xpath, namespaceManager);
            if (element != null)
            {
                this.ReadTrustInfo(element.OuterXml);
            }
        }

        public void ReadManifest(Stream input)
        {
            this.Read(input, "/asmv1:assembly/asmv2:trustInfo");
        }

        public void ReadManifest(string path)
        {
            using (Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                this.ReadManifest(stream);
            }
        }

        private void ReadTrustInfo(string xml)
        {
            this._inputTrustInfoDocument = new XmlDocument();
            this._inputTrustInfoDocument.LoadXml(xml);
            XmlElement inputPermissionSetElement = this.GetInputPermissionSetElement();
            XmlAttribute namedItem = (XmlAttribute) inputPermissionSetElement.Attributes.GetNamedItem(XmlUtil.TrimPrefix("asmv2:Unrestricted"));
            this._isFullTrust = (namedItem != null) && bool.Parse(namedItem.Value);
            XmlAttribute attribute2 = (XmlAttribute) inputPermissionSetElement.Attributes.GetNamedItem(XmlUtil.TrimPrefix("asmv2:SameSite"));
            if (attribute2 != null)
            {
                this.sameSiteSetting = attribute2.Value;
            }
        }

        public override string ToString()
        {
            MemoryStream output = new MemoryStream();
            this.Write(output);
            output.Position = 0L;
            StreamReader reader = new StreamReader(output);
            return reader.ReadToEnd();
        }

        public void Write(Stream output)
        {
            XmlDocument document = new XmlDocument();
            XmlElement newChild = XmlUtil.CloneElementToDocument(this.GetInputPermissionSetElement(), document, "urn:schemas-microsoft-com:asm.v2");
            document.AppendChild(newChild);
            string str = null;
            XmlDocument document2 = new XmlDocument();
            XmlElement inputRequestedPrivilegeElement = this.GetInputRequestedPrivilegeElement();
            XmlElement requestedPrivilegeElement = null;
            requestedPrivilegeElement = this.GetRequestedPrivilegeElement(inputRequestedPrivilegeElement, document2);
            if (requestedPrivilegeElement != null)
            {
                document2.AppendChild(requestedPrivilegeElement);
                MemoryStream outStream = new MemoryStream();
                document2.Save(outStream);
                outStream.Position = 0L;
                str = Util.WriteTempFile(outStream);
            }
            try
            {
                string resource = "trustinfo2.xsl";
                MemoryStream stream2 = new MemoryStream();
                if ((this._outputPermissionSet == null) && !this.sameSiteChanged)
                {
                    XmlElement documentElement = document.DocumentElement;
                    this.FixupPermissionSetElement(documentElement);
                    document.Save(stream2);
                    stream2.Position = 0L;
                }
                else
                {
                    XmlDocument outputPermissionSetDocument = this.GetOutputPermissionSetDocument();
                    XmlElement permissionSetElement = outputPermissionSetDocument.DocumentElement;
                    this.FixupPermissionSetElement(permissionSetElement);
                    if (document.DocumentElement == null)
                    {
                        outputPermissionSetDocument.Save(stream2);
                        stream2.Position = 0L;
                    }
                    else
                    {
                        XmlElement oldChild = document.DocumentElement;
                        XmlElement element8 = (XmlElement) document.ImportNode(permissionSetElement, true);
                        oldChild.ParentNode.ReplaceChild(element8, oldChild);
                        document.Save(stream2);
                        stream2.Position = 0L;
                    }
                }
                Stream input = null;
                if (str != null)
                {
                    DictionaryEntry[] entries = new DictionaryEntry[] { new DictionaryEntry("defaultRequestedPrivileges", str) };
                    input = XmlUtil.XslTransform(resource, stream2, entries);
                }
                else
                {
                    input = XmlUtil.XslTransform(resource, stream2, new DictionaryEntry[0]);
                }
                Util.CopyStream(input, output);
            }
            finally
            {
                if (str != null)
                {
                    File.Delete(str);
                }
            }
        }

        public void Write(string path)
        {
            using (Stream stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                this.Write(stream);
                stream.Flush();
            }
        }

        public void WriteManifest(Stream output)
        {
            string name = "manifest.xml";
            Stream embeddedResourceStream = Util.GetEmbeddedResourceStream(name);
            this.WriteManifest(embeddedResourceStream, output);
        }

        public void WriteManifest(string path)
        {
            Stream input = null;
            try
            {
                if (File.Exists(path))
                {
                    input = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                else
                {
                    input = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
                }
                if (input.Length > 0L)
                {
                    this.WriteManifest(input, input);
                }
                else
                {
                    this.WriteManifest(input);
                }
            }
            finally
            {
                if (input != null)
                {
                    input.Flush();
                    input.Close();
                }
            }
        }

        public void WriteManifest(Stream input, Stream output)
        {
            int tickCount = Environment.TickCount;
            XmlDocument document = new XmlDocument();
            document.Load(input);
            XmlNamespaceManager namespaceManager = XmlNamespaces.GetNamespaceManager(document.NameTable);
            XmlElement element = (XmlElement) document.SelectSingleNode("asmv1:assembly", namespaceManager);
            if (element == null)
            {
                throw new BadImageFormatException();
            }
            XmlElement newChild = (XmlElement) element.SelectSingleNode("asmv2:trustInfo", namespaceManager);
            if (newChild == null)
            {
                newChild = document.CreateElement(XmlUtil.TrimPrefix("asmv2:trustInfo"), "urn:schemas-microsoft-com:asm.v2");
                element.AppendChild(newChild);
            }
            if (((this._inputTrustInfoDocument != null) && (this._outputPermissionSet == null)) && !this.sameSiteChanged)
            {
                XmlElement element3 = (XmlElement) document.ImportNode(this._inputTrustInfoDocument.DocumentElement, true);
                newChild.ParentNode.ReplaceChild(element3, newChild);
            }
            else
            {
                XmlElement element4 = (XmlElement) newChild.SelectSingleNode("asmv2:security", namespaceManager);
                if (element4 == null)
                {
                    element4 = document.CreateElement(XmlUtil.TrimPrefix("asmv2:security"), "urn:schemas-microsoft-com:asm.v2");
                    newChild.AppendChild(element4);
                }
                XmlElement element5 = (XmlElement) element4.SelectSingleNode("asmv2:applicationRequestMinimum", namespaceManager);
                if (element5 == null)
                {
                    element5 = document.CreateElement(XmlUtil.TrimPrefix("asmv2:applicationRequestMinimum"), "urn:schemas-microsoft-com:asm.v2");
                    element4.AppendChild(element5);
                }
                foreach (XmlNode node in element5.SelectNodes("asmv2:PermissionSet", namespaceManager))
                {
                    element5.RemoveChild(node);
                }
                XmlDocument outputPermissionSetDocument = this.GetOutputPermissionSetDocument();
                XmlElement element6 = (XmlElement) document.ImportNode(outputPermissionSetDocument.DocumentElement, true);
                element5.AppendChild(element6);
                this.FixupPermissionSetElement(element6);
            }
            if (output.Length > 0L)
            {
                output.SetLength(0L);
                output.Flush();
            }
            document.Save(output);
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "ManifestWriter.WriteTrustInfo t={0}", new object[] { Environment.TickCount - tickCount }));
        }

        public bool HasUnmanagedCodePermission
        {
            get
            {
                System.Security.PermissionSet outputPermissionSet = this.GetOutputPermissionSet();
                if (outputPermissionSet == null)
                {
                    return false;
                }
                System.Security.PermissionSet other = new System.Security.PermissionSet(PermissionState.None);
                other.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
                return (outputPermissionSet.Intersect(other) != null);
            }
        }

        public bool IsFullTrust
        {
            get
            {
                this.GetInputPermissionSet();
                return this._isFullTrust;
            }
            set
            {
                this._isFullTrust = value;
            }
        }

        public System.Security.PermissionSet PermissionSet
        {
            get
            {
                return this.GetOutputPermissionSet();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("PermissionSet cannot be set to null.");
                }
                this._outputPermissionSet = value;
            }
        }

        public bool PreserveFullTrustPermissionSet
        {
            get
            {
                return this._preserveFullTrustPermissionSet;
            }
            set
            {
                this._preserveFullTrustPermissionSet = value;
            }
        }

        public string SameSiteAccess
        {
            get
            {
                return this.sameSiteSetting;
            }
            set
            {
                this.sameSiteSetting = value;
                this.sameSiteChanged = true;
            }
        }
    }
}

