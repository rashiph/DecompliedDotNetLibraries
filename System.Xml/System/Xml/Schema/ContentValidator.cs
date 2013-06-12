namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class ContentValidator
    {
        public static readonly ContentValidator Any = new ContentValidator(XmlSchemaContentType.Mixed, true, true);
        private XmlSchemaContentType contentType;
        public static readonly ContentValidator Empty = new ContentValidator(XmlSchemaContentType.Empty);
        private bool isEmptiable;
        private bool isOpen;
        public static readonly ContentValidator Mixed = new ContentValidator(XmlSchemaContentType.Mixed);
        public static readonly ContentValidator TextOnly = new ContentValidator(XmlSchemaContentType.TextOnly, false, false);

        public ContentValidator(XmlSchemaContentType contentType)
        {
            this.contentType = contentType;
            this.isEmptiable = true;
        }

        protected ContentValidator(XmlSchemaContentType contentType, bool isOpen, bool isEmptiable)
        {
            this.contentType = contentType;
            this.isOpen = isOpen;
            this.isEmptiable = isEmptiable;
        }

        public static void AddParticleToExpected(XmlSchemaParticle p, XmlSchemaSet schemaSet, ArrayList particles)
        {
            AddParticleToExpected(p, schemaSet, particles, false);
        }

        public static void AddParticleToExpected(XmlSchemaParticle p, XmlSchemaSet schemaSet, ArrayList particles, bool global)
        {
            if (!particles.Contains(p))
            {
                particles.Add(p);
            }
            XmlSchemaElement element = p as XmlSchemaElement;
            if ((element != null) && (global || !element.RefName.IsEmpty))
            {
                XmlSchemaSubstitutionGroup group = (XmlSchemaSubstitutionGroup) schemaSet.SubstitutionGroups[element.QualifiedName];
                if (group != null)
                {
                    for (int i = 0; i < group.Members.Count; i++)
                    {
                        XmlSchemaElement item = (XmlSchemaElement) group.Members[i];
                        if (!element.QualifiedName.Equals(item.QualifiedName) && !particles.Contains(item))
                        {
                            particles.Add(item);
                        }
                    }
                }
            }
        }

        public virtual bool CompleteValidation(ValidationState context)
        {
            return true;
        }

        public virtual ArrayList ExpectedElements(ValidationState context, bool isRequiredOnly)
        {
            return null;
        }

        public virtual ArrayList ExpectedParticles(ValidationState context, bool isRequiredOnly, XmlSchemaSet schemaSet)
        {
            return null;
        }

        public virtual void InitValidation(ValidationState context)
        {
        }

        public virtual object ValidateElement(XmlQualifiedName name, ValidationState context, out int errorCode)
        {
            if ((this.contentType == XmlSchemaContentType.TextOnly) || (this.contentType == XmlSchemaContentType.Empty))
            {
                context.NeedValidateChildren = false;
            }
            errorCode = -1;
            return null;
        }

        public XmlSchemaContentType ContentType
        {
            get
            {
                return this.contentType;
            }
        }

        public virtual bool IsEmptiable
        {
            get
            {
                return this.isEmptiable;
            }
        }

        public bool IsOpen
        {
            get
            {
                return (((this.contentType != XmlSchemaContentType.TextOnly) && (this.contentType != XmlSchemaContentType.Empty)) && this.isOpen);
            }
            set
            {
                this.isOpen = value;
            }
        }

        public bool PreserveWhitespace
        {
            get
            {
                if (this.contentType != XmlSchemaContentType.TextOnly)
                {
                    return (this.contentType == XmlSchemaContentType.Mixed);
                }
                return true;
            }
        }
    }
}

