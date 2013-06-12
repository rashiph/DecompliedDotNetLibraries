namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaGroup : XmlSchemaAnnotated
    {
        private XmlSchemaParticle canonicalParticle;
        private string name;
        private XmlSchemaGroupBase particle;
        private XmlQualifiedName qname = XmlQualifiedName.Empty;
        private XmlSchemaGroup redefined;
        private int selfReferenceCount;

        internal override XmlSchemaObject Clone()
        {
            return this.Clone(null);
        }

        internal XmlSchemaObject Clone(XmlSchema parentSchema)
        {
            XmlSchemaGroup group = (XmlSchemaGroup) base.MemberwiseClone();
            if (XmlSchemaComplexType.HasParticleRef(this.particle, parentSchema))
            {
                group.particle = XmlSchemaComplexType.CloneParticle(this.particle, parentSchema) as XmlSchemaGroupBase;
            }
            group.canonicalParticle = XmlSchemaParticle.Empty;
            return group;
        }

        internal void SetQualifiedName(XmlQualifiedName value)
        {
            this.qname = value;
        }

        [XmlIgnore]
        internal XmlSchemaParticle CanonicalParticle
        {
            get
            {
                return this.canonicalParticle;
            }
            set
            {
                this.canonicalParticle = value;
            }
        }

        [XmlAttribute("name")]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        [XmlIgnore]
        internal override string NameAttribute
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }

        [XmlElement("sequence", typeof(XmlSchemaSequence)), XmlElement("choice", typeof(XmlSchemaChoice)), XmlElement("all", typeof(XmlSchemaAll))]
        public XmlSchemaGroupBase Particle
        {
            get
            {
                return this.particle;
            }
            set
            {
                this.particle = value;
            }
        }

        [XmlIgnore]
        public XmlQualifiedName QualifiedName
        {
            get
            {
                return this.qname;
            }
        }

        [XmlIgnore]
        internal XmlSchemaGroup Redefined
        {
            get
            {
                return this.redefined;
            }
            set
            {
                this.redefined = value;
            }
        }

        [XmlIgnore]
        internal int SelfReferenceCount
        {
            get
            {
                return this.selfReferenceCount;
            }
            set
            {
                this.selfReferenceCount = value;
            }
        }
    }
}

