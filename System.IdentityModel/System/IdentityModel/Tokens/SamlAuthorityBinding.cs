namespace System.IdentityModel.Tokens
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract]
    public class SamlAuthorityBinding
    {
        private XmlQualifiedName authorityKind;
        private string binding;
        [DataMember]
        private bool isReadOnly;
        private string location;

        public SamlAuthorityBinding()
        {
        }

        public SamlAuthorityBinding(XmlQualifiedName authorityKind, string binding, string location)
        {
            this.AuthorityKind = authorityKind;
            this.Binding = binding;
            this.Location = location;
            this.CheckObjectValidity();
        }

        private void CheckObjectValidity()
        {
            if (this.authorityKind == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthorityBindingMissingAuthorityKind")));
            }
            if (string.IsNullOrEmpty(this.authorityKind.Name))
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthorityKindMissingName")));
            }
            if (string.IsNullOrEmpty(this.binding))
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthorityBindingRequiresBinding")));
            }
            if (string.IsNullOrEmpty(this.location))
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthorityBindingRequiresLocation")));
            }
        }

        public void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        public virtual void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            string str2;
            string str3;
            if (reader == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
            }
            if (samlSerializer == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));
            }
            SamlDictionary samlDictionary = samlSerializer.DictionaryManager.SamlDictionary;
            string attribute = reader.GetAttribute(samlDictionary.AuthorityKind, null);
            if (string.IsNullOrEmpty(attribute))
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthorityBindingMissingAuthorityKindOnRead")));
            }
            string[] strArray = attribute.Split(new char[] { ':' });
            if (strArray.Length > 2)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthorityBindingInvalidAuthorityKind")));
            }
            if (strArray.Length == 2)
            {
                str3 = strArray[0];
                str2 = strArray[1];
            }
            else
            {
                str3 = string.Empty;
                str2 = strArray[0];
            }
            string ns = reader.LookupNamespace(str3);
            this.authorityKind = new XmlQualifiedName(str2, ns);
            this.binding = reader.GetAttribute(samlDictionary.Binding, null);
            if (string.IsNullOrEmpty(this.binding))
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthorityBindingMissingBindingOnRead")));
            }
            this.location = reader.GetAttribute(samlDictionary.Location, null);
            if (string.IsNullOrEmpty(this.location))
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthorityBindingMissingLocationOnRead")));
            }
            if (reader.IsEmptyElement)
            {
                reader.MoveToContent();
                reader.Read();
            }
            else
            {
                reader.MoveToContent();
                reader.Read();
                reader.ReadEndElement();
            }
        }

        public virtual void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            this.CheckObjectValidity();
            if (writer == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            if (samlSerializer == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));
            }
            SamlDictionary samlDictionary = samlSerializer.DictionaryManager.SamlDictionary;
            writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.AuthorityBinding, samlDictionary.Namespace);
            string prefix = null;
            if (!string.IsNullOrEmpty(this.authorityKind.Namespace))
            {
                writer.WriteAttributeString(string.Empty, samlDictionary.NamespaceAttributePrefix.Value, null, this.authorityKind.Namespace);
                prefix = writer.LookupPrefix(this.authorityKind.Namespace);
            }
            writer.WriteStartAttribute(samlDictionary.AuthorityKind, null);
            if (string.IsNullOrEmpty(prefix))
            {
                writer.WriteString(this.authorityKind.Name);
            }
            else
            {
                writer.WriteString(prefix + ":" + this.authorityKind.Name);
            }
            writer.WriteEndAttribute();
            writer.WriteStartAttribute(samlDictionary.Location, null);
            writer.WriteString(this.location);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute(samlDictionary.Binding, null);
            writer.WriteString(this.binding);
            writer.WriteEndAttribute();
            writer.WriteEndElement();
        }

        [DataMember]
        public XmlQualifiedName AuthorityKind
        {
            get
            {
                return this.authorityKind;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (value == null)
                {
                    throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                if (string.IsNullOrEmpty(value.Name))
                {
                    throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAuthorityKindMissingName"));
                }
                this.authorityKind = value;
            }
        }

        [DataMember]
        public string Binding
        {
            get
            {
                return this.binding;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (string.IsNullOrEmpty(value))
                {
                    throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAuthorityBindingRequiresBinding"));
                }
                this.binding = value;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        [DataMember]
        public string Location
        {
            get
            {
                return this.location;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (string.IsNullOrEmpty(value))
                {
                    throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAuthorityBindingRequiresLocation"));
                }
                this.location = value;
            }
        }
    }
}

