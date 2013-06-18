namespace System.IdentityModel.Tokens
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.Xml;

    public class SamlAction
    {
        private string action;
        private bool isReadOnly;
        private string ns;

        public SamlAction()
        {
        }

        public SamlAction(string action) : this(action, null)
        {
        }

        public SamlAction(string action, string ns)
        {
            if (string.IsNullOrEmpty(action))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("action", System.IdentityModel.SR.GetString("SAMLActionNameRequired"));
            }
            this.action = action;
            this.ns = ns;
        }

        private void CheckObjectValidity()
        {
            if (string.IsNullOrEmpty(this.action))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLActionNameRequired")));
            }
        }

        public void MakeReadOnly()
        {
            this.isReadOnly = true;
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
            if (reader.IsStartElement(samlDictionary.Action, samlDictionary.Namespace))
            {
                this.ns = reader.GetAttribute(samlDictionary.ActionNamespaceAttribute, null);
                reader.MoveToContent();
                this.action = reader.ReadString();
                if (string.IsNullOrEmpty(this.action))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLActionNameRequiredOnRead")));
                }
                reader.MoveToContent();
                reader.ReadEndElement();
            }
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
            writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.Action, samlDictionary.Namespace);
            if (this.ns != null)
            {
                writer.WriteStartAttribute(samlDictionary.ActionNamespaceAttribute, null);
                writer.WriteString(this.ns);
                writer.WriteEndAttribute();
            }
            writer.WriteString(this.action);
            writer.WriteEndElement();
        }

        public string Action
        {
            get
            {
                return this.action;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", System.IdentityModel.SR.GetString("SAMLActionNameRequired"));
                }
                this.action = value;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public string Namespace
        {
            get
            {
                return this.ns;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.ns = value;
            }
        }
    }
}

