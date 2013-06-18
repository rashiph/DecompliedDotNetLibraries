namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.Xml;

    public class SamlAdvice
    {
        private readonly ImmutableCollection<string> assertionIdReferences;
        private readonly ImmutableCollection<SamlAssertion> assertions;
        private bool isReadOnly;

        public SamlAdvice() : this(null, null)
        {
        }

        public SamlAdvice(IEnumerable<SamlAssertion> assertions) : this(null, assertions)
        {
        }

        public SamlAdvice(IEnumerable<string> references) : this(references, null)
        {
        }

        public SamlAdvice(IEnumerable<string> references, IEnumerable<SamlAssertion> assertions)
        {
            this.assertionIdReferences = new ImmutableCollection<string>();
            this.assertions = new ImmutableCollection<SamlAssertion>();
            if (references != null)
            {
                foreach (string str in references)
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

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.assertionIdReferences.MakeReadOnly();
                foreach (SamlAssertion assertion in this.assertions)
                {
                    assertion.MakeReadOnly();
                }
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
            if (reader.IsEmptyElement)
            {
                reader.MoveToContent();
                reader.Read();
            }
            else
            {
                reader.MoveToContent();
                reader.Read();
                while (reader.IsStartElement())
                {
                    if (!reader.IsStartElement(samlDictionary.AssertionIdReference, samlDictionary.Namespace))
                    {
                        if (!reader.IsStartElement(samlDictionary.Assertion, samlDictionary.Namespace))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLBadSchema", new object[] { samlDictionary.Advice.Value })));
                        }
                        SamlAssertion item = new SamlAssertion();
                        item.ReadXml(reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
                        this.assertions.Add(item);
                    }
                    else
                    {
                        reader.MoveToContent();
                        this.assertionIdReferences.Add(reader.ReadString());
                        reader.MoveToContent();
                        reader.ReadEndElement();
                        continue;
                    }
                }
                reader.MoveToContent();
                reader.ReadEndElement();
            }
        }

        public virtual void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            if (samlSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));
            }
            SamlDictionary samlDictionary = samlSerializer.DictionaryManager.SamlDictionary;
            writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.Advice, samlDictionary.Namespace);
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

