namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class DirectoryControl
    {
        private bool directoryControlCriticality;
        private bool directoryControlServerSide;
        private string directoryControlType;
        internal byte[] directoryControlValue;

        internal DirectoryControl(XmlElement el)
        {
            XmlAttribute attribute2;
            this.directoryControlType = "";
            this.directoryControlCriticality = true;
            this.directoryControlServerSide = true;
            XmlNamespaceManager dsmlNamespaceManager = NamespaceUtils.GetDsmlNamespaceManager();
            XmlAttribute attribute = (XmlAttribute) el.SelectSingleNode("@dsml:criticality", dsmlNamespaceManager);
            if (attribute == null)
            {
                attribute = (XmlAttribute) el.SelectSingleNode("@criticality", dsmlNamespaceManager);
            }
            if (attribute == null)
            {
                this.directoryControlCriticality = false;
            }
            else
            {
                string str = attribute.Value;
                switch (str)
                {
                    case "true":
                    case "1":
                        this.directoryControlCriticality = true;
                        goto Label_00B5;
                }
                if (!(str == "false") && !(str == "0"))
                {
                    throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("BadControl"));
                }
                this.directoryControlCriticality = false;
            }
        Label_00B5:
            attribute2 = (XmlAttribute) el.SelectSingleNode("@dsml:type", dsmlNamespaceManager);
            if (attribute2 == null)
            {
                attribute2 = (XmlAttribute) el.SelectSingleNode("@type", dsmlNamespaceManager);
            }
            if (attribute2 == null)
            {
                throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("BadControl"));
            }
            this.directoryControlType = attribute2.Value;
            XmlElement element = (XmlElement) el.SelectSingleNode("dsml:controlValue", dsmlNamespaceManager);
            if (element != null)
            {
                try
                {
                    this.directoryControlValue = Convert.FromBase64String(element.InnerText);
                }
                catch (FormatException)
                {
                    throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("BadControl"));
                }
            }
        }

        public DirectoryControl(string type, byte[] value, bool isCritical, bool serverSide)
        {
            this.directoryControlType = "";
            this.directoryControlCriticality = true;
            this.directoryControlServerSide = true;
            Utility.CheckOSVersion();
            this.directoryControlType = type;
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (value != null)
            {
                this.directoryControlValue = new byte[value.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    this.directoryControlValue[i] = value[i];
                }
            }
            this.directoryControlCriticality = isCritical;
            this.directoryControlServerSide = serverSide;
        }

        public virtual byte[] GetValue()
        {
            if (this.directoryControlValue == null)
            {
                return new byte[0];
            }
            byte[] buffer = new byte[this.directoryControlValue.Length];
            for (int i = 0; i < this.directoryControlValue.Length; i++)
            {
                buffer[i] = this.directoryControlValue[i];
            }
            return buffer;
        }

        internal XmlElement ToXmlNode(XmlDocument doc)
        {
            XmlElement element = doc.CreateElement("control", "urn:oasis:names:tc:DSML:2:0:core");
            XmlAttribute node = doc.CreateAttribute("type", null);
            node.InnerText = this.Type;
            element.Attributes.Append(node);
            XmlAttribute attribute2 = doc.CreateAttribute("criticality", null);
            attribute2.InnerText = this.IsCritical ? "true" : "false";
            element.Attributes.Append(attribute2);
            byte[] inArray = this.GetValue();
            if (inArray.Length != 0)
            {
                XmlElement newChild = doc.CreateElement("controlValue", "urn:oasis:names:tc:DSML:2:0:core");
                XmlAttribute attribute3 = doc.CreateAttribute("xsi:type", "http://www.w3.org/2001/XMLSchema-instance");
                attribute3.InnerText = "xsd:base64Binary";
                newChild.Attributes.Append(attribute3);
                string str = Convert.ToBase64String(inArray);
                newChild.InnerText = str;
                element.AppendChild(newChild);
            }
            return element;
        }

        internal static void TransformControls(DirectoryControl[] controls)
        {
            for (int i = 0; i < controls.Length; i++)
            {
                byte[] buffer = controls[i].GetValue();
                if (controls[i].Type == "1.2.840.113556.1.4.319")
                {
                    object[] objArray = BerConverter.Decode("{iO}", buffer);
                    int count = (int) objArray[0];
                    byte[] cookie = (byte[]) objArray[1];
                    if (cookie == null)
                    {
                        cookie = new byte[0];
                    }
                    controls[i] = new PageResultResponseControl(count, cookie, controls[i].IsCritical, controls[i].GetValue());
                }
                else if (controls[i].Type == "1.2.840.113556.1.4.1504")
                {
                    object[] objArray2 = null;
                    if (Utility.IsWin2kOS)
                    {
                        objArray2 = BerConverter.Decode("{i}", buffer);
                    }
                    else
                    {
                        objArray2 = BerConverter.Decode("{e}", buffer);
                    }
                    int result = (int) objArray2[0];
                    controls[i] = new AsqResponseControl(result, controls[i].IsCritical, controls[i].GetValue());
                }
                else if (controls[i].Type == "1.2.840.113556.1.4.841")
                {
                    object[] objArray3 = BerConverter.Decode("{iiO}", buffer);
                    int num4 = (int) objArray3[0];
                    int resultSize = (int) objArray3[1];
                    byte[] buffer3 = (byte[]) objArray3[2];
                    controls[i] = new DirSyncResponseControl(buffer3, num4 != 0, resultSize, controls[i].IsCritical, controls[i].GetValue());
                }
                else if (controls[i].Type == "1.2.840.113556.1.4.474")
                {
                    bool flag;
                    object[] objArray4 = null;
                    int num6 = 0;
                    string attributeName = null;
                    if (Utility.IsWin2kOS)
                    {
                        objArray4 = BerConverter.TryDecode("{ia}", buffer, out flag);
                    }
                    else
                    {
                        objArray4 = BerConverter.TryDecode("{ea}", buffer, out flag);
                    }
                    if (flag)
                    {
                        num6 = (int) objArray4[0];
                        attributeName = (string) objArray4[1];
                    }
                    else
                    {
                        if (Utility.IsWin2kOS)
                        {
                            objArray4 = BerConverter.Decode("{i}", buffer);
                        }
                        else
                        {
                            objArray4 = BerConverter.Decode("{e}", buffer);
                        }
                        num6 = (int) objArray4[0];
                    }
                    controls[i] = new SortResponseControl((ResultCode) num6, attributeName, controls[i].IsCritical, controls[i].GetValue());
                }
                else if (controls[i].Type == "2.16.840.1.113730.3.4.10")
                {
                    int num7;
                    int num8;
                    int num9;
                    byte[] context = null;
                    object[] objArray5 = null;
                    bool decodeSucceeded = false;
                    if (Utility.IsWin2kOS)
                    {
                        objArray5 = BerConverter.TryDecode("{iiiO}", buffer, out decodeSucceeded);
                    }
                    else
                    {
                        objArray5 = BerConverter.TryDecode("{iieO}", buffer, out decodeSucceeded);
                    }
                    if (decodeSucceeded)
                    {
                        num7 = (int) objArray5[0];
                        num8 = (int) objArray5[1];
                        num9 = (int) objArray5[2];
                        context = (byte[]) objArray5[3];
                    }
                    else
                    {
                        if (Utility.IsWin2kOS)
                        {
                            objArray5 = BerConverter.Decode("{iii}", buffer);
                        }
                        else
                        {
                            objArray5 = BerConverter.Decode("{iie}", buffer);
                        }
                        num7 = (int) objArray5[0];
                        num8 = (int) objArray5[1];
                        num9 = (int) objArray5[2];
                    }
                    controls[i] = new VlvResponseControl(num7, num8, context, (ResultCode) num9, controls[i].IsCritical, controls[i].GetValue());
                }
            }
        }

        public bool IsCritical
        {
            get
            {
                return this.directoryControlCriticality;
            }
            set
            {
                this.directoryControlCriticality = value;
            }
        }

        public bool ServerSide
        {
            get
            {
                return this.directoryControlServerSide;
            }
            set
            {
                this.directoryControlServerSide = value;
            }
        }

        public string Type
        {
            get
            {
                return this.directoryControlType;
            }
        }
    }
}

