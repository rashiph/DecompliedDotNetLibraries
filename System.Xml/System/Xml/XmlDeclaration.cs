namespace System.Xml
{
    using System;
    using System.Text;

    public class XmlDeclaration : XmlLinkedNode
    {
        private string encoding;
        private const string NO = "no";
        private string standalone;
        private string version;
        private const string YES = "yes";

        protected internal XmlDeclaration(string version, string encoding, string standalone, XmlDocument doc) : base(doc)
        {
            if (!this.IsValidXmlVersion(version))
            {
                throw new ArgumentException(Res.GetString("Xdom_Version"));
            }
            if (((standalone != null) && (standalone.Length > 0)) && ((standalone != "yes") && (standalone != "no")))
            {
                throw new ArgumentException(Res.GetString("Xdom_standalone", new object[] { standalone }));
            }
            this.Encoding = encoding;
            this.Standalone = standalone;
            this.Version = version;
        }

        public override XmlNode CloneNode(bool deep)
        {
            return this.OwnerDocument.CreateXmlDeclaration(this.Version, this.Encoding, this.Standalone);
        }

        private bool IsValidXmlVersion(string ver)
        {
            return ((((ver.Length >= 3) && (ver[0] == '1')) && (ver[1] == '.')) && XmlCharType.IsOnlyDigits(ver, 2, ver.Length - 2));
        }

        public override void WriteContentTo(XmlWriter w)
        {
        }

        public override void WriteTo(XmlWriter w)
        {
            w.WriteProcessingInstruction(this.Name, this.InnerText);
        }

        public string Encoding
        {
            get
            {
                return this.encoding;
            }
            set
            {
                this.encoding = (value == null) ? string.Empty : value;
            }
        }

        public override string InnerText
        {
            get
            {
                StringBuilder builder = new StringBuilder("version=\"" + this.Version + "\"");
                if (this.Encoding.Length > 0)
                {
                    builder.Append(" encoding=\"");
                    builder.Append(this.Encoding);
                    builder.Append("\"");
                }
                if (this.Standalone.Length > 0)
                {
                    builder.Append(" standalone=\"");
                    builder.Append(this.Standalone);
                    builder.Append("\"");
                }
                return builder.ToString();
            }
            set
            {
                string version = null;
                string encoding = null;
                string standalone = null;
                string str4 = this.Encoding;
                string str5 = this.Standalone;
                string str6 = this.Version;
                XmlLoader.ParseXmlDeclarationValue(value, out version, out encoding, out standalone);
                try
                {
                    if ((version != null) && !this.IsValidXmlVersion(version))
                    {
                        throw new ArgumentException(Res.GetString("Xdom_Version"));
                    }
                    this.Version = version;
                    if (encoding != null)
                    {
                        this.Encoding = encoding;
                    }
                    if (standalone != null)
                    {
                        this.Standalone = standalone;
                    }
                }
                catch
                {
                    this.Encoding = str4;
                    this.Standalone = str5;
                    this.Version = str6;
                    throw;
                }
            }
        }

        public override string LocalName
        {
            get
            {
                return this.Name;
            }
        }

        public override string Name
        {
            get
            {
                return "xml";
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return XmlNodeType.XmlDeclaration;
            }
        }

        public string Standalone
        {
            get
            {
                return this.standalone;
            }
            set
            {
                if (value == null)
                {
                    this.standalone = string.Empty;
                }
                else
                {
                    if (((value.Length != 0) && (value != "yes")) && (value != "no"))
                    {
                        throw new ArgumentException(Res.GetString("Xdom_standalone", new object[] { value }));
                    }
                    this.standalone = value;
                }
            }
        }

        public override string Value
        {
            get
            {
                return this.InnerText;
            }
            set
            {
                this.InnerText = value;
            }
        }

        public string Version
        {
            get
            {
                return this.version;
            }
            internal set
            {
                this.version = value;
            }
        }
    }
}

