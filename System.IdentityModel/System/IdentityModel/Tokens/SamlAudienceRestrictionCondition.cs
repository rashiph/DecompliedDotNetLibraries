namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.Xml;

    public class SamlAudienceRestrictionCondition : SamlCondition
    {
        private readonly ImmutableCollection<Uri> audiences;
        private bool isReadOnly;

        public SamlAudienceRestrictionCondition()
        {
            this.audiences = new ImmutableCollection<Uri>();
        }

        public SamlAudienceRestrictionCondition(IEnumerable<Uri> audiences)
        {
            this.audiences = new ImmutableCollection<Uri>();
            if (audiences == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("audiences"));
            }
            foreach (Uri uri in audiences)
            {
                if (uri == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLEntityCannotBeNullOrEmpty", new object[] { XD.SamlDictionary.Audience.Value }));
                }
                this.audiences.Add(uri);
            }
            this.CheckObjectValidity();
        }

        private void CheckObjectValidity()
        {
            if (this.audiences.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAudienceRestrictionShouldHaveOneAudience")));
            }
        }

        public override void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.audiences.MakeReadOnly();
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
            while (reader.IsStartElement())
            {
                if (!reader.IsStartElement(samlDictionary.Audience, samlDictionary.Namespace))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLBadSchema", new object[] { samlDictionary.AudienceRestrictionCondition.Value })));
                }
                reader.MoveToContent();
                string str = reader.ReadString();
                if (string.IsNullOrEmpty(str))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAudienceRestrictionInvalidAudienceValueOnRead")));
                }
                this.audiences.Add(new Uri(str));
                reader.MoveToContent();
                reader.ReadEndElement();
            }
            if (this.audiences.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAudienceRestrictionShouldHaveOneAudienceOnRead")));
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
            writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.AudienceRestrictionCondition, samlDictionary.Namespace);
            for (int i = 0; i < this.audiences.Count; i++)
            {
                writer.WriteElementString(samlDictionary.Audience, samlDictionary.Namespace, this.audiences[i].AbsoluteUri);
            }
            writer.WriteEndElement();
        }

        public IList<Uri> Audiences
        {
            get
            {
                return this.audiences;
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

