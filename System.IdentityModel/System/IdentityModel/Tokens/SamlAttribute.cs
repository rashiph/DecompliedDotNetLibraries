namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Selectors;
    using System.Xml;

    public class SamlAttribute
    {
        private readonly ImmutableCollection<string> attributeValues;
        private List<Claim> claims;
        private string claimType;
        private bool isReadOnly;
        private string name;
        private string nameSpace;

        public SamlAttribute()
        {
            this.attributeValues = new ImmutableCollection<string>();
        }

        public SamlAttribute(Claim claim)
        {
            this.attributeValues = new ImmutableCollection<string>();
            if (claim == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claim");
            }
            if (!(claim.Resource is string))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SamlAttributeClaimResourceShouldBeAString"));
            }
            if (claim.Right != Rights.PossessProperty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SamlAttributeClaimRightShouldBePossessProperty"));
            }
            int length = claim.ClaimType.LastIndexOf('/');
            if (((length == -1) || (length == 0)) || (length == (claim.ClaimType.Length - 1)))
            {
                this.nameSpace = string.Empty;
                this.name = claim.ClaimType;
            }
            else
            {
                this.nameSpace = claim.ClaimType.Substring(0, length);
                this.name = claim.ClaimType.Substring(length + 1, claim.ClaimType.Length - (length + 1));
            }
            this.claimType = claim.ClaimType;
            this.attributeValues.Add(claim.Resource as string);
        }

        public SamlAttribute(string attributeNamespace, string attributeName, IEnumerable<string> attributeValues)
        {
            this.attributeValues = new ImmutableCollection<string>();
            if (string.IsNullOrEmpty(attributeName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAttributeNameAttributeRequired"));
            }
            if (string.IsNullOrEmpty(attributeNamespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAttributeNamespaceAttributeRequired"));
            }
            if (attributeValues == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attributeValues");
            }
            this.name = attributeName;
            this.nameSpace = attributeNamespace;
            this.claimType = string.IsNullOrEmpty(this.nameSpace) ? this.name : (this.nameSpace + "/" + this.name);
            foreach (string str in attributeValues)
            {
                if (str == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAttributeValueCannotBeNull"));
                }
                this.attributeValues.Add(str);
            }
            if (this.attributeValues.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAttributeShouldHaveOneValue"));
            }
        }

        private void CheckObjectValidity()
        {
            if (string.IsNullOrEmpty(this.name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAttributeNameAttributeRequired")));
            }
            if (string.IsNullOrEmpty(this.nameSpace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAttributeNamespaceAttributeRequired")));
            }
            if (this.attributeValues.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAttributeShouldHaveOneValue")));
            }
        }

        public virtual ReadOnlyCollection<Claim> ExtractClaims()
        {
            if (this.claims == null)
            {
                List<Claim> list = new List<Claim>(this.attributeValues.Count);
                for (int i = 0; i < this.attributeValues.Count; i++)
                {
                    if (this.attributeValues[i] == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAttributeValueCannotBeNull"));
                    }
                    list.Add(new Claim(this.claimType, this.attributeValues[i], Rights.PossessProperty));
                }
                this.claims = list;
            }
            return this.claims.AsReadOnly();
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.attributeValues.MakeReadOnly();
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
            this.name = reader.GetAttribute(samlDictionary.AttributeName, null);
            if (string.IsNullOrEmpty(this.name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAttributeMissingNameAttributeOnRead")));
            }
            this.nameSpace = reader.GetAttribute(samlDictionary.AttributeNamespace, null);
            if (string.IsNullOrEmpty(this.nameSpace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAttributeMissingNamespaceAttributeOnRead")));
            }
            this.claimType = string.IsNullOrEmpty(this.nameSpace) ? this.name : (this.nameSpace + "/" + this.name);
            reader.MoveToContent();
            reader.Read();
            while (reader.IsStartElement(samlDictionary.AttributeValue, samlDictionary.Namespace))
            {
                string item = reader.ReadString();
                this.attributeValues.Add(item);
                reader.MoveToContent();
                reader.ReadEndElement();
            }
            if (this.attributeValues.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAttributeShouldHaveOneValue")));
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
            writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.Attribute, samlDictionary.Namespace);
            writer.WriteStartAttribute(samlDictionary.AttributeName, null);
            writer.WriteString(this.name);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute(samlDictionary.AttributeNamespace, null);
            writer.WriteString(this.nameSpace);
            writer.WriteEndAttribute();
            for (int i = 0; i < this.attributeValues.Count; i++)
            {
                if (this.attributeValues[i] == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAttributeValueCannotBeNull"));
                }
                writer.WriteElementString(samlDictionary.PreferredPrefix.Value, samlDictionary.AttributeValue, samlDictionary.Namespace, this.attributeValues[i]);
            }
            writer.WriteEndElement();
        }

        public IList<string> AttributeValues
        {
            get
            {
                return this.attributeValues;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAttributeNameAttributeRequired"));
                }
                this.name = value;
            }
        }

        public string Namespace
        {
            get
            {
                return this.nameSpace;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAttributeNamespaceAttributeRequired"));
                }
                this.nameSpace = value;
            }
        }
    }
}

