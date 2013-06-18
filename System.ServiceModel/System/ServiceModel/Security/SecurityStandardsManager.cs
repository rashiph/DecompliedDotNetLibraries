namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal class SecurityStandardsManager
    {
        private readonly SignatureTargetIdManager idManager;
        private static SecurityStandardsManager instance;
        private readonly System.ServiceModel.MessageSecurityVersion messageSecurityVersion;
        private readonly System.ServiceModel.Security.SecureConversationDriver secureConversationDriver;
        private readonly System.IdentityModel.Selectors.SecurityTokenSerializer tokenSerializer;
        private readonly System.ServiceModel.Security.TrustDriver trustDriver;
        private System.ServiceModel.Security.WSSecurityTokenSerializer wsSecurityTokenSerializer;
        private readonly System.ServiceModel.Security.WSUtilitySpecificationVersion wsUtilitySpecificationVersion;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public SecurityStandardsManager() : this(System.ServiceModel.Security.WSSecurityTokenSerializer.DefaultInstance)
        {
        }

        public SecurityStandardsManager(System.IdentityModel.Selectors.SecurityTokenSerializer tokenSerializer) : this(System.ServiceModel.MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11, tokenSerializer)
        {
        }

        public SecurityStandardsManager(System.ServiceModel.MessageSecurityVersion messageSecurityVersion, System.IdentityModel.Selectors.SecurityTokenSerializer tokenSerializer)
        {
            if (messageSecurityVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageSecurityVersion"));
            }
            if (tokenSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenSerializer");
            }
            this.messageSecurityVersion = messageSecurityVersion;
            this.tokenSerializer = tokenSerializer;
            if (messageSecurityVersion.SecureConversationVersion == System.ServiceModel.Security.SecureConversationVersion.WSSecureConversation13)
            {
                this.secureConversationDriver = new WSSecureConversationDec2005.DriverDec2005();
            }
            else
            {
                this.secureConversationDriver = new WSSecureConversationFeb2005.DriverFeb2005();
            }
            if ((this.SecurityVersion != System.ServiceModel.Security.SecurityVersion.WSSecurity10) && (this.SecurityVersion != System.ServiceModel.Security.SecurityVersion.WSSecurity11))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("messageSecurityVersion", System.ServiceModel.SR.GetString("MessageSecurityVersionOutOfRange")));
            }
            this.idManager = System.ServiceModel.Security.WSSecurityJan2004.IdManager.Instance;
            this.wsUtilitySpecificationVersion = System.ServiceModel.Security.WSUtilitySpecificationVersion.Default;
            if (messageSecurityVersion.MessageSecurityTokenVersion.TrustVersion == System.ServiceModel.Security.TrustVersion.WSTrust13)
            {
                this.trustDriver = new WSTrustDec2005.DriverDec2005(this);
            }
            else
            {
                this.trustDriver = new WSTrustFeb2005.DriverFeb2005(this);
            }
        }

        internal SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXml(XmlElement element, SecurityTokenReferenceStyle tokenReferenceStyle)
        {
            return this.WSSecurityTokenSerializer.CreateKeyIdentifierClauseFromTokenXml(element, tokenReferenceStyle);
        }

        internal ReceiveSecurityHeader CreateReceiveSecurityHeader(Message message, string actor, SecurityAlgorithmSuite algorithmSuite, MessageDirection direction)
        {
            ReceiveSecurityHeader header = this.TryCreateReceiveSecurityHeader(message, actor, algorithmSuite, direction);
            if (header != null)
            {
                return header;
            }
            if (string.IsNullOrEmpty(actor))
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnableToFindSecurityHeaderInMessageNoActor")), message);
            }
            throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnableToFindSecurityHeaderInMessage", new object[] { actor })), message);
        }

        internal SendSecurityHeader CreateSendSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay, SecurityAlgorithmSuite algorithmSuite, MessageDirection direction)
        {
            return this.SecurityVersion.CreateSendSecurityHeader(message, actor, mustUnderstand, relay, this, algorithmSuite, direction);
        }

        internal bool DoesMessageContainSecurityHeader(Message message)
        {
            return this.SecurityVersion.DoesMessageContainSecurityHeader(message);
        }

        internal bool TryCreateKeyIdentifierClauseFromTokenXml(XmlElement element, SecurityTokenReferenceStyle tokenReferenceStyle, out SecurityKeyIdentifierClause securityKeyIdentifierClause)
        {
            return this.WSSecurityTokenSerializer.TryCreateKeyIdentifierClauseFromTokenXml(element, tokenReferenceStyle, out securityKeyIdentifierClause);
        }

        internal ReceiveSecurityHeader TryCreateReceiveSecurityHeader(Message message, string actor, SecurityAlgorithmSuite algorithmSuite, MessageDirection direction)
        {
            return this.SecurityVersion.TryCreateReceiveSecurityHeader(message, actor, this, algorithmSuite, direction);
        }

        internal bool TryGetSecurityContextIds(Message message, string[] actors, bool isStrictMode, ICollection<UniqueId> results)
        {
            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            System.ServiceModel.Security.SecureConversationDriver secureConversationDriver = this.SecureConversationDriver;
            int headerIndex = this.SecurityVersion.FindIndexOfSecurityHeader(message, actors);
            if (headerIndex < 0)
            {
                return false;
            }
            bool flag = false;
            using (XmlDictionaryReader reader = message.Headers.GetReaderAtHeader(headerIndex))
            {
                if (!reader.IsStartElement())
                {
                    return false;
                }
                if (reader.IsEmptyElement)
                {
                    return false;
                }
                reader.ReadStartElement();
                while (reader.IsStartElement())
                {
                    if (secureConversationDriver.IsAtSecurityContextToken(reader))
                    {
                        results.Add(secureConversationDriver.GetSecurityContextTokenId(reader));
                        flag = true;
                        if (isStrictMode)
                        {
                            return flag;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            return flag;
        }

        public static SecurityStandardsManager DefaultInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SecurityStandardsManager();
                }
                return instance;
            }
        }

        internal SignatureTargetIdManager IdManager
        {
            get
            {
                return this.idManager;
            }
        }

        public System.ServiceModel.MessageSecurityVersion MessageSecurityVersion
        {
            get
            {
                return this.messageSecurityVersion;
            }
        }

        internal System.ServiceModel.Security.SecureConversationDriver SecureConversationDriver
        {
            get
            {
                return this.secureConversationDriver;
            }
        }

        public System.ServiceModel.Security.SecureConversationVersion SecureConversationVersion
        {
            get
            {
                return this.messageSecurityVersion.SecureConversationVersion;
            }
        }

        internal System.IdentityModel.Selectors.SecurityTokenSerializer SecurityTokenSerializer
        {
            get
            {
                return this.tokenSerializer;
            }
        }

        public System.ServiceModel.Security.SecurityVersion SecurityVersion
        {
            get
            {
                if (this.messageSecurityVersion != null)
                {
                    return this.messageSecurityVersion.SecurityVersion;
                }
                return null;
            }
        }

        internal System.ServiceModel.Security.TrustDriver TrustDriver
        {
            get
            {
                return this.trustDriver;
            }
        }

        public System.ServiceModel.Security.TrustVersion TrustVersion
        {
            get
            {
                return this.messageSecurityVersion.TrustVersion;
            }
        }

        private System.ServiceModel.Security.WSSecurityTokenSerializer WSSecurityTokenSerializer
        {
            get
            {
                if (this.wsSecurityTokenSerializer == null)
                {
                    System.ServiceModel.Security.WSSecurityTokenSerializer tokenSerializer = this.tokenSerializer as System.ServiceModel.Security.WSSecurityTokenSerializer;
                    if (tokenSerializer == null)
                    {
                        tokenSerializer = new System.ServiceModel.Security.WSSecurityTokenSerializer(this.SecurityVersion);
                    }
                    this.wsSecurityTokenSerializer = tokenSerializer;
                }
                return this.wsSecurityTokenSerializer;
            }
        }

        internal System.ServiceModel.Security.WSUtilitySpecificationVersion WSUtilitySpecificationVersion
        {
            get
            {
                return this.wsUtilitySpecificationVersion;
            }
        }
    }
}

