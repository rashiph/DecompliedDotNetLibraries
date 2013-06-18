namespace System.Xml.Linq
{
    using System;
    using System.Runtime;
    using System.Text;
    using System.Xml;

    public class XDeclaration
    {
        private string encoding;
        private string standalone;
        private string version;

        public XDeclaration(XDeclaration other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            this.version = other.version;
            this.encoding = other.encoding;
            this.standalone = other.standalone;
        }

        internal XDeclaration(XmlReader r)
        {
            this.version = r.GetAttribute("version");
            this.encoding = r.GetAttribute("encoding");
            this.standalone = r.GetAttribute("standalone");
            r.Read();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XDeclaration(string version, string encoding, string standalone)
        {
            this.version = version;
            this.encoding = encoding;
            this.standalone = standalone;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("<?xml");
            if (this.version != null)
            {
                builder.Append(" version=\"");
                builder.Append(this.version);
                builder.Append("\"");
            }
            if (this.encoding != null)
            {
                builder.Append(" encoding=\"");
                builder.Append(this.encoding);
                builder.Append("\"");
            }
            if (this.standalone != null)
            {
                builder.Append(" standalone=\"");
                builder.Append(this.standalone);
                builder.Append("\"");
            }
            builder.Append("?>");
            return builder.ToString();
        }

        public string Encoding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.encoding;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.encoding = value;
            }
        }

        public string Standalone
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.standalone;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.standalone = value;
            }
        }

        public string Version
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.version;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.version = value;
            }
        }
    }
}

