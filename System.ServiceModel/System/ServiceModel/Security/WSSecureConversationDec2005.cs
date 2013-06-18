namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal class WSSecureConversationDec2005 : WSSecureConversation
    {
        private IList<Type> knownClaimTypes;
        private SecurityStateEncoder securityStateEncoder;

        public WSSecureConversationDec2005(WSSecurityTokenSerializer tokenSerializer, SecurityStateEncoder securityStateEncoder, IEnumerable<Type> knownTypes, int maxKeyDerivationOffset, int maxKeyDerivationLabelLength, int maxKeyDerivationNonceLength) : base(tokenSerializer, maxKeyDerivationOffset, maxKeyDerivationLabelLength, maxKeyDerivationNonceLength)
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
            strEntries.Add(new SctStrEntryDec2005(this));
        }

        public override void PopulateTokenEntries(IList<WSSecurityTokenSerializer.TokenEntry> tokenEntryList)
        {
            base.PopulateTokenEntries(tokenEntryList);
            tokenEntryList.Add(new SecurityContextTokenEntryDec2005(this, this.securityStateEncoder, this.knownClaimTypes));
        }

        public override string DerivationAlgorithm
        {
            get
            {
                return "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/dk/p_sha1";
            }
        }

        public override SecureConversationDictionary SerializerDictionary
        {
            get
            {
                return DXD.SecureConversationDec2005Dictionary;
            }
        }

        public class DriverDec2005 : WSSecureConversation.Driver
        {
            public override XmlDictionaryString CloseAction
            {
                get
                {
                    return DXD.SecureConversationDec2005Dictionary.RequestSecurityContextClose;
                }
            }

            public override XmlDictionaryString CloseResponseAction
            {
                get
                {
                    return DXD.SecureConversationDec2005Dictionary.RequestSecurityContextCloseResponse;
                }
            }

            protected override SecureConversationDictionary DriverDictionary
            {
                get
                {
                    return DXD.SecureConversationDec2005Dictionary;
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
                    return DXD.SecureConversationDec2005Dictionary.Namespace;
                }
            }

            public override XmlDictionaryString RenewAction
            {
                get
                {
                    return DXD.SecureConversationDec2005Dictionary.RequestSecurityContextRenew;
                }
            }

            public override XmlDictionaryString RenewResponseAction
            {
                get
                {
                    return DXD.SecureConversationDec2005Dictionary.RequestSecurityContextRenewResponse;
                }
            }

            public override string TokenTypeUri
            {
                get
                {
                    return DXD.SecureConversationDec2005Dictionary.SecurityContextTokenType.Value;
                }
            }
        }

        private class SctStrEntryDec2005 : WSSecureConversation.SctStrEntry
        {
            public SctStrEntryDec2005(WSSecureConversationDec2005 parent) : base(parent)
            {
            }

            protected override UniqueId ReadGeneration(XmlDictionaryReader reader)
            {
                return XmlHelper.GetAttributeAsUniqueId(reader, DXD.SecureConversationDec2005Dictionary.Instance, DXD.SecureConversationDec2005Dictionary.Namespace);
            }

            protected override void WriteGeneration(XmlDictionaryWriter writer, SecurityContextKeyIdentifierClause clause)
            {
                if (clause.Generation != null)
                {
                    XmlHelper.WriteAttributeStringAsUniqueId(writer, DXD.SecureConversationDec2005Dictionary.Prefix.Value, DXD.SecureConversationDec2005Dictionary.Instance, DXD.SecureConversationDec2005Dictionary.Namespace, clause.Generation);
                }
            }
        }

        private class SecurityContextTokenEntryDec2005 : WSSecureConversation.SecurityContextTokenEntry
        {
            public SecurityContextTokenEntryDec2005(WSSecureConversationDec2005 parent, SecurityStateEncoder securityStateEncoder, IList<Type> knownClaimTypes) : base(parent, securityStateEncoder, knownClaimTypes)
            {
            }

            protected override bool CanReadGeneration(XmlDictionaryReader reader)
            {
                return reader.IsStartElement(DXD.SecureConversationDec2005Dictionary.Instance, DXD.SecureConversationDec2005Dictionary.Namespace);
            }

            protected override bool CanReadGeneration(XmlElement element)
            {
                return ((element.LocalName == DXD.SecureConversationDec2005Dictionary.Instance.Value) && (element.NamespaceURI == DXD.SecureConversationDec2005Dictionary.Namespace.Value));
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
                    writer.WriteStartElement(DXD.SecureConversationDec2005Dictionary.Prefix.Value, DXD.SecureConversationDec2005Dictionary.Instance, DXD.SecureConversationDec2005Dictionary.Namespace);
                    XmlHelper.WriteStringAsUniqueId(writer, sct.KeyGeneration);
                    writer.WriteEndElement();
                }
            }
        }
    }
}

