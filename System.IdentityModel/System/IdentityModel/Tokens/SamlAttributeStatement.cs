namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.Xml;

    public class SamlAttributeStatement : SamlSubjectStatement
    {
        private readonly ImmutableCollection<SamlAttribute> attributes;
        private bool isReadOnly;

        public SamlAttributeStatement()
        {
            this.attributes = new ImmutableCollection<SamlAttribute>();
        }

        public SamlAttributeStatement(SamlSubject samlSubject, IEnumerable<SamlAttribute> attributes) : base(samlSubject)
        {
            this.attributes = new ImmutableCollection<SamlAttribute>();
            if (attributes == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("attributes"));
            }
            foreach (SamlAttribute attribute in attributes)
            {
                if (attribute == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLEntityCannotBeNullOrEmpty", new object[] { XD.SamlDictionary.Attribute.Value }));
                }
                this.attributes.Add(attribute);
            }
            this.CheckObjectValidity();
        }

        protected override void AddClaimsToList(IList<Claim> claims)
        {
            if (claims == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claims");
            }
            for (int i = 0; i < this.attributes.Count; i++)
            {
                if (this.attributes[i] != null)
                {
                    ReadOnlyCollection<Claim> onlys = this.attributes[i].ExtractClaims();
                    if (onlys != null)
                    {
                        for (int j = 0; j < onlys.Count; j++)
                        {
                            if (onlys[j] != null)
                            {
                                claims.Add(onlys[j]);
                            }
                        }
                    }
                }
            }
        }

        private void CheckObjectValidity()
        {
            if (base.SamlSubject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLSubjectStatementRequiresSubject")));
            }
            if (this.attributes.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAttributeShouldHaveOneValue")));
            }
        }

        public override void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                foreach (SamlAttribute attribute in this.attributes)
                {
                    attribute.MakeReadOnly();
                }
                this.attributes.MakeReadOnly();
                this.isReadOnly = true;
            }
        }

        public override void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
            }
            if (samlSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));
            }
            SamlDictionary samlDictionary = samlSerializer.DictionaryManager.SamlDictionary;
            reader.MoveToContent();
            reader.Read();
            if (!reader.IsStartElement(samlDictionary.Subject, samlDictionary.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAttributeStatementMissingSubjectOnRead")));
            }
            SamlSubject subject = new SamlSubject();
            subject.ReadXml(reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
            base.SamlSubject = subject;
            while (reader.IsStartElement())
            {
                if (!reader.IsStartElement(samlDictionary.Attribute, samlDictionary.Namespace))
                {
                    break;
                }
                SamlAttribute item = samlSerializer.LoadAttribute(reader, keyInfoSerializer, outOfBandTokenResolver);
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLUnableToLoadAttribute")));
                }
                this.attributes.Add(item);
            }
            if (this.attributes.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAttributeStatementMissingAttributeOnRead")));
            }
            reader.MoveToContent();
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            this.CheckObjectValidity();
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            if (samlSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));
            }
            SamlDictionary samlDictionary = samlSerializer.DictionaryManager.SamlDictionary;
            writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.AttributeStatement, samlDictionary.Namespace);
            base.SamlSubject.WriteXml(writer, samlSerializer, keyInfoSerializer);
            for (int i = 0; i < this.attributes.Count; i++)
            {
                this.attributes[i].WriteXml(writer, samlSerializer, keyInfoSerializer);
            }
            writer.WriteEndElement();
        }

        public IList<SamlAttribute> Attributes
        {
            get
            {
                return this.attributes;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }
    }
}

