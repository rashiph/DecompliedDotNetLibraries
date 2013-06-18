namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.Xml;

    public class SamlEvidence
    {
        private readonly ImmutableCollection<string> assertionIdReferences;
        private readonly ImmutableCollection<SamlAssertion> assertions;
        private bool isReadOnly;

        public SamlEvidence()
        {
            this.assertionIdReferences = new ImmutableCollection<string>();
            this.assertions = new ImmutableCollection<SamlAssertion>();
        }

        public SamlEvidence(IEnumerable<SamlAssertion> assertions) : this(null, assertions)
        {
        }

        public SamlEvidence(IEnumerable<string> assertionIdReferences) : this(assertionIdReferences, null)
        {
        }

        public SamlEvidence(IEnumerable<string> assertionIdReferences, IEnumerable<SamlAssertion> assertions)
        {
            this.assertionIdReferences = new ImmutableCollection<string>();
            this.assertions = new ImmutableCollection<SamlAssertion>();
            if ((assertionIdReferences == null) && (assertions == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLEvidenceShouldHaveOneAssertion"));
            }
            if (assertionIdReferences != null)
            {
                foreach (string str in assertionIdReferences)
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLEntityCannotBeNullOrEmpty", new object[] { XD.SamlDictionary.AssertionIdReference.Value }));
                    }
                    this.assertionIdReferences.Add(str);
                }
            }
            if (assertions != null)
            {
                foreach (SamlAssertion assertion in assertions)
                {
                    if (assertion == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLEntityCannotBeNullOrEmpty", new object[] { XD.SamlDictionary.Assertion.Value }));
                    }
                    this.assertions.Add(assertion);
                }
            }
        }

        private void CheckObjectValidity()
        {
            if ((this.assertions.Count == 0) && (this.assertionIdReferences.Count == 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLEvidenceShouldHaveOneAssertion")));
            }
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                foreach (SamlAssertion assertion in this.assertions)
                {
                    assertion.MakeReadOnly();
                }
                this.assertionIdReferences.MakeReadOnly();
                this.assertions.MakeReadOnly();
                this.isReadOnly = true;
            }
        }

        public virtual void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
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
                if (!reader.IsStartElement(samlDictionary.AssertionIdReference, samlDictionary.Namespace))
                {
                    if (!reader.IsStartElement(samlDictionary.Assertion, samlDictionary.Namespace))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLBadSchema", new object[] { samlDictionary.Evidence.Value })));
                    }
                    SamlAssertion item = new SamlAssertion();
                    item.ReadXml(reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
                    this.assertions.Add(item);
                }
                else
                {
                    reader.MoveToContent();
                    this.assertionIdReferences.Add(reader.ReadString());
                    reader.ReadEndElement();
                    continue;
                }
            }
            if ((this.assertionIdReferences.Count == 0) && (this.assertions.Count == 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLEvidenceShouldHaveOneAssertionOnRead")));
            }
            reader.MoveToContent();
            reader.ReadEndElement();
        }

        public virtual void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
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
            writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.Evidence.Value, samlDictionary.Namespace.Value);
            for (int i = 0; i < this.assertionIdReferences.Count; i++)
            {
                writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.AssertionIdReference, samlDictionary.Namespace);
                writer.WriteString(this.assertionIdReferences[i]);
                writer.WriteEndElement();
            }
            for (int j = 0; j < this.assertions.Count; j++)
            {
                this.assertions[j].WriteXml(writer, samlSerializer, keyInfoSerializer);
            }
            writer.WriteEndElement();
        }

        public IList<string> AssertionIdReferences
        {
            get
            {
                return this.assertionIdReferences;
            }
        }

        public IList<SamlAssertion> Assertions
        {
            get
            {
                return this.assertions;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }
    }
}

