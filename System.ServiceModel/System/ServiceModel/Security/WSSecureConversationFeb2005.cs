namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal class WSSecureConversationFeb2005 : WSSecureConversation
    {
        private IList<Type> knownClaimTypes;
        private SecurityStateEncoder securityStateEncoder;

        public WSSecureConversationFeb2005(WSSecurityTokenSerializer tokenSerializer, SecurityStateEncoder securityStateEncoder, IEnumerable<Type> knownTypes, int maxKeyDerivationOffset, int maxKeyDerivationLabelLength, int maxKeyDerivationNonceLength) : base(tokenSerializer, maxKeyDerivationOffset, maxKeyDerivationLabelLength, maxKeyDerivationNonceLength)
        {
            if (securityStateEncoder != null)
            {
                this.securityStateEncoder = securityStateEncoder;
            }
            else
            {
                this.securityStateEncoder = new DataProtectionSecurityStateEncoder();
            }
            this.knownClaimTypes = new List<Type>();
            if (knownTypes != null)
            {
                foreach (Type type in knownTypes)
                {
                    this.knownClaimTypes.Add(type);
                }
            }
        }

        public override void PopulateStrEntries(IList<WSSecurityTokenSerializer.StrEntry> strEntries)
        {
            strEntries.Add(new SctStrEntryFeb2005(this));
        }

        public override void PopulateTokenEntries(IList<WSSecurityTokenSerializer.TokenEntry> tokenEntryList)
        {
            base.PopulateTokenEntries(tokenEntryList);
            tokenEntryList.Add(new SecurityContextTokenEntryFeb2005(this, this.securityStateEncoder, this.knownClaimTypes));
        }

        public override SecureConversationDictionary SerializerDictionary
        {
            get
            {
                return XD.SecureConversationFeb2005Dictionary;
            }
        }

        public class DriverFeb2005 : WSSecureConversation.Driver
        {
            public override XmlDictionaryString CloseAction
            {
                get
                {
                    return XD.SecureConversationFeb2005Dictionary.RequestSecurityContextClose;
                }
            }

            public override XmlDictionaryString CloseResponseAction
            {
                get
                {
                    return XD.SecureConversationFeb2005Dictionary.RequestSecurityContextCloseResponse;
                }
            }

            protected override SecureConversationDictionary DriverDictionary
            {
                get
                {
                    return XD.SecureConversationFeb2005Dictionary;
                }
            }

            public override bool IsSessionSupported
            {
                get
                {
                    return true;
                }
            }

            public override XmlDictionaryString Namespace
            {
                get
                {
                    return XD.SecureConversationFeb2005Dictionary.Namespace;
                }
            }

            public override XmlDictionaryString RenewAction
            {
                get
                {
                    return XD.SecureConversationFeb2005Dictionary.RequestSecurityContextRenew;
                }
            }

            public override XmlDictionaryString RenewResponseAction
            {
                get
                {
                    return XD.SecureConversationFeb2005Dictionary.RequestSecurityContextRenewResponse;
                }
            }

            public override string TokenTypeUri
            {
                get
                {
                    return XD.SecureConversationFeb2005Dictionary.SecurityContextTokenType.Value;
                }
            }
        }

        private class SctStrEntryFeb2005 : WSSecureConversation.SctStrEntry
        {
            public SctStrEntryFeb2005(WSSecureConversationFeb2005 parent) : base(parent)
            {
            }

            protected override UniqueId ReadGeneration(XmlDictionaryReader reader)
            {
                return XmlHelper.GetAttributeAsUniqueId(reader, DXD.SecureConversationDec2005Dictionary.Instance, XD.SecureConversationFeb2005Dictionary.Namespace);
            }

            protected override void WriteGeneration(XmlDictionaryWriter writer, SecurityContextKeyIdentifierClause clause)
            {
                if (clause.Generation != null)
                {
                    XmlHelper.WriteAttributeStringAsUniqueId(writer, XD.SecureConversationFeb2005Dictionary.Prefix.Value, DXD.SecureConversationDec2005Dictionary.Instance, XD.SecureConversationFeb2005Dictionary.Namespace, clause.Generation);
                }
            }
        }

        private class SecurityContextTokenEntryFeb2005 : WSSecureConversation.SecurityContextTokenEntry
        {
            public SecurityContextTokenEntryFeb2005(WSSecureConversationFeb2005 parent, SecurityStateEncoder securityStateEncoder, IList<Type> knownClaimTypes) : base(parent, securityStateEncoder, knownClaimTypes)
            {
            }

            protected override bool CanReadGeneration(XmlDictionaryReader reader)
            {
                return reader.IsStartElement(DXD.SecureConversationDec2005Dictionary.Instance, XD.SecureConversationFeb2005Dictionary.Namespace);
            }

            protected override bool CanReadGeneration(XmlElement element)
            {
                return ((element.LocalName == DXD.SecureConversationDec2005Dictionary.Instance.Value) && (element.NamespaceURI == XD.SecureConversationFeb2005Dictionary.Namespace.Value));
            }

            protected override UniqueId ReadGeneration(XmlDictionaryReader reader)
            {
                return reader.ReadElementContentAsUniqueId();
            }

            protected override UniqueId ReadGeneration(XmlElement element)
            {
                return XmlHelper.ReadTextElementAsUniqueId(element);
            }

            protected override void WriteGeneration(XmlDictionaryWriter writer, SecurityContextSecurityToken sct)
            {
                if (sct.KeyGeneration != null)
                {
                    writer.WriteStartElement(XD.SecureConversationFeb2005Dictionary.Prefix.Value, DXD.SecureConversationDec2005Dictionary.Instance, XD.SecureConversationFeb2005Dictionary.Namespace);
                    XmlHelper.WriteStringAsUniqueId(writer, sct.KeyGeneration);
                    writer.WriteEndElement();
                }
            }
        }
    }
}

