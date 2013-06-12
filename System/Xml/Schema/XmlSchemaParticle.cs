namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public abstract class XmlSchemaParticle : XmlSchemaAnnotated
    {
        internal static readonly XmlSchemaParticle Empty = new EmptyParticle();
        private Occurs flags;
        private decimal maxOccurs = 1M;
        private decimal minOccurs = 1M;

        protected XmlSchemaParticle()
        {
        }

        internal XmlQualifiedName GetQualifiedName()
        {
            XmlSchemaElement element = this as XmlSchemaElement;
            if (element != null)
            {
                return element.QualifiedName;
            }
            XmlSchemaAny any = this as XmlSchemaAny;
            if (any == null)
            {
                return XmlQualifiedName.Empty;
            }
            string str = any.Namespace;
            if (str != null)
            {
                str = str.Trim();
            }
            else
            {
                str = string.Empty;
            }
            return new XmlQualifiedName("*", (str.Length == 0) ? "##any" : str);
        }

        internal virtual bool IsEmpty
        {
            get
            {
                return (this.maxOccurs == 0M);
            }
        }

        internal bool IsMultipleOccurrence
        {
            get
            {
                return (this.maxOccurs > 1M);
            }
        }

        [XmlIgnore]
        public decimal MaxOccurs
        {
            get
            {
                return this.maxOccurs;
            }
            set
            {
                if ((value < 0M) || (value != decimal.Truncate(value)))
                {
                    throw new XmlSchemaException("Sch_MaxOccursInvalidXsd", string.Empty);
                }
                this.maxOccurs = value;
                if ((this.maxOccurs == 0M) && ((this.flags & Occurs.Min) == Occurs.None))
                {
                    this.minOccurs = 0M;
                }
                this.flags |= Occurs.Max;
            }
        }

        [XmlAttribute("maxOccurs")]
        public string MaxOccursString
        {
            get
            {
                if ((this.flags & Occurs.Max) == Occurs.None)
                {
                    return null;
                }
                if (!(this.maxOccurs == 79228162514264337593543950335M))
                {
                    return XmlConvert.ToString(this.maxOccurs);
                }
                return "unbounded";
            }
            set
            {
                if (value == null)
                {
                    this.maxOccurs = 1M;
                    this.flags &= ~Occurs.Max;
                }
                else
                {
                    if (value == "unbounded")
                    {
                        this.maxOccurs = 79228162514264337593543950335M;
                    }
                    else
                    {
                        this.maxOccurs = XmlConvert.ToInteger(value);
                        if (this.maxOccurs < 0M)
                        {
                            throw new XmlSchemaException("Sch_MaxOccursInvalidXsd", string.Empty);
                        }
                        if ((this.maxOccurs == 0M) && ((this.flags & Occurs.Min) == Occurs.None))
                        {
                            this.minOccurs = 0M;
                        }
                    }
                    this.flags |= Occurs.Max;
                }
            }
        }

        [XmlIgnore]
        public decimal MinOccurs
        {
            get
            {
                return this.minOccurs;
            }
            set
            {
                if ((value < 0M) || (value != decimal.Truncate(value)))
                {
                    throw new XmlSchemaException("Sch_MinOccursInvalidXsd", string.Empty);
                }
                this.minOccurs = value;
                this.flags |= Occurs.Min;
            }
        }

        [XmlAttribute("minOccurs")]
        public string MinOccursString
        {
            get
            {
                if ((this.flags & Occurs.Min) != Occurs.None)
                {
                    return XmlConvert.ToString(this.minOccurs);
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    this.minOccurs = 1M;
                    this.flags &= ~Occurs.Min;
                }
                else
                {
                    this.minOccurs = XmlConvert.ToInteger(value);
                    if (this.minOccurs < 0M)
                    {
                        throw new XmlSchemaException("Sch_MinOccursInvalidXsd", string.Empty);
                    }
                    this.flags |= Occurs.Min;
                }
            }
        }

        internal virtual string NameString
        {
            get
            {
                return string.Empty;
            }
        }

        private class EmptyParticle : XmlSchemaParticle
        {
            internal override bool IsEmpty
            {
                get
                {
                    return true;
                }
            }
        }

        [Flags]
        private enum Occurs
        {
            None,
            Min,
            Max
        }
    }
}

