namespace System.Xml.Serialization
{
    using System;
    using System.Xml;
    using System.Xml.Schema;

    internal abstract class Accessor
    {
        private bool any;
        private string anyNs;
        private object defaultValue;
        private XmlSchemaForm form;
        private bool isFixed;
        private bool isOptional;
        private TypeMapping mapping;
        private string name;
        private string ns;
        private bool topLevelInSchema;

        internal Accessor()
        {
        }

        internal static string EscapeName(string name)
        {
            if ((name != null) && (name.Length != 0))
            {
                return XmlConvert.EncodeLocalName(name);
            }
            return name;
        }

        internal static string EscapeQName(string name)
        {
            if ((name == null) || (name.Length == 0))
            {
                return name;
            }
            int length = name.LastIndexOf(':');
            if (length < 0)
            {
                return XmlConvert.EncodeLocalName(name);
            }
            if ((length == 0) || (length == (name.Length - 1)))
            {
                throw new ArgumentException(Res.GetString("Xml_InvalidNameChars", new object[] { name }), "name");
            }
            return new XmlQualifiedName(XmlConvert.EncodeLocalName(name.Substring(length + 1)), XmlConvert.EncodeLocalName(name.Substring(0, length))).ToString();
        }

        internal string ToString(string defaultNs)
        {
            if (this.Any)
            {
                return (((this.Namespace == null) ? "##any" : this.Namespace) + ":" + this.Name);
            }
            if (!(this.Namespace == defaultNs))
            {
                return (this.Namespace + ":" + this.Name);
            }
            return this.Name;
        }

        internal static string UnescapeName(string name)
        {
            return XmlConvert.DecodeName(name);
        }

        internal bool Any
        {
            get
            {
                return this.any;
            }
            set
            {
                this.any = value;
            }
        }

        internal string AnyNamespaces
        {
            get
            {
                return this.anyNs;
            }
            set
            {
                this.anyNs = value;
            }
        }

        internal object Default
        {
            get
            {
                return this.defaultValue;
            }
            set
            {
                this.defaultValue = value;
            }
        }

        internal XmlSchemaForm Form
        {
            get
            {
                return this.form;
            }
            set
            {
                this.form = value;
            }
        }

        internal bool HasDefault
        {
            get
            {
                return ((this.defaultValue != null) && (this.defaultValue != DBNull.Value));
            }
        }

        internal bool IsFixed
        {
            get
            {
                return this.isFixed;
            }
            set
            {
                this.isFixed = value;
            }
        }

        internal bool IsOptional
        {
            get
            {
                return this.isOptional;
            }
            set
            {
                this.isOptional = value;
            }
        }

        internal bool IsTopLevelInSchema
        {
            get
            {
                return this.topLevelInSchema;
            }
            set
            {
                this.topLevelInSchema = value;
            }
        }

        internal TypeMapping Mapping
        {
            get
            {
                return this.mapping;
            }
            set
            {
                this.mapping = value;
            }
        }

        internal virtual string Name
        {
            get
            {
                if (this.name != null)
                {
                    return this.name;
                }
                return string.Empty;
            }
            set
            {
                this.name = value;
            }
        }

        internal string Namespace
        {
            get
            {
                return this.ns;
            }
            set
            {
                this.ns = value;
            }
        }
    }
}

