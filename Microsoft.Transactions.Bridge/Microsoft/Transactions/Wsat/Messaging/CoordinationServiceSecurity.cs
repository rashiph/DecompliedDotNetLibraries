namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Xml;

    internal class CoordinationServiceSecurity
    {
        private static DataContractSerializer identifierElementSerializer10;
        private static DataContractSerializer identifierElementSerializer11;
        private IdentityVerifier identityVerifier = IdentityVerifier.CreateDefault();
        private static byte[] Label = Encoding.UTF8.GetBytes("WS-AT Supporting Token");
        private static System.ServiceModel.Security.Tokens.SecurityContextSecurityTokenParameters securityContextSecurityTokenParameters;
        private static SecurityStandardsManager SecurityStandardsManager2007 = CreateStandardsManager(MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12);

        public static void AddIssuedToken(Message message, RequestSecurityTokenResponse rstr)
        {
            TransactionFlowProperty.Ensure(message).IssuedTokens.Add(rstr);
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Added issued token to message");
            }
        }

        public static void AddSupportingToken(Message message, RequestSecurityTokenResponse rstr)
        {
            GenericXmlSecurityToken token = rstr.GetIssuedToken(null, null, SecurityKeyEntropyMode.ServerEntropy, null, null, null);
            SecurityMessageProperty property = new SecurityMessageProperty();
            SupportingTokenSpecification item = new SupportingTokenSpecification(token, new List<IAuthorizationPolicy>().AsReadOnly(), SecurityTokenAttachmentMode.Endorsing, SecurityContextSecurityTokenParameters);
            property.OutgoingSupportingTokens.Add(item);
            message.Properties.Security = property;
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Attached supporting token {0} to register message", rstr.Context);
            }
        }

        public bool CheckIdentity(Proxy proxy, Message message)
        {
            EndpointAddress to = proxy.To;
            bool access = this.identityVerifier.CheckAccess(to, message);
            this.TraceCheckIdentityResult(access, to, message);
            return access;
        }

        public static void CreateIssuedToken(Guid transactionId, string coordinationContextId, ProtocolVersion protocolVersion, out RequestSecurityTokenResponse issuedToken, out string sctId)
        {
            sctId = CoordinationContext.CreateNativeIdentifier(Guid.NewGuid());
            byte[] key = DeriveIssuedTokenKey(transactionId, sctId);
            DateTime utcNow = DateTime.UtcNow;
            SecurityContextSecurityToken token = new SecurityContextSecurityToken(new UniqueId(sctId), key, utcNow, utcNow + TimeSpan.FromDays(36500.0));
            BinarySecretSecurityToken token2 = new BinarySecretSecurityToken(key);
            SecurityStandardsManager standardsManager = CreateStandardsManager(protocolVersion);
            RequestSecurityTokenResponse response = new RequestSecurityTokenResponse(standardsManager) {
                TokenType = standardsManager.SecureConversationDriver.TokenTypeUri,
                RequestedUnattachedReference = SecurityContextSecurityTokenParameters.CreateKeyIdentifierClause(token, SecurityTokenReferenceStyle.External),
                RequestedAttachedReference = SecurityContextSecurityTokenParameters.CreateKeyIdentifierClause(token, SecurityTokenReferenceStyle.Internal),
                RequestedSecurityToken = token,
                RequestedProofToken = token2
            };
            DataContractSerializer serializer = IdentifierElementSerializer(protocolVersion);
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(CoordinationServiceSecurity), "CreateIssuedToken");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    response.SetAppliesTo<IdentifierElement10>(new IdentifierElement10(coordinationContextId), serializer);
                    break;

                case ProtocolVersion.Version11:
                    response.SetAppliesTo<IdentifierElement11>(new IdentifierElement11(coordinationContextId), serializer);
                    break;
            }
            response.MakeReadOnly();
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Created issued token with id {0} for transaction {1}", sctId, transactionId);
            }
            issuedToken = response;
        }

        private static SecurityStandardsManager CreateStandardsManager(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(CoordinationServiceSecurity), "CreateStandardsManager");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return SecurityStandardsManager.DefaultInstance;

                case ProtocolVersion.Version11:
                    return SecurityStandardsManager2007;
            }
            return null;
        }

        private static SecurityStandardsManager CreateStandardsManager(MessageSecurityVersion securityVersion)
        {
            return new SecurityStandardsManager(securityVersion, new WSSecurityTokenSerializer(securityVersion.SecurityVersion, securityVersion.TrustVersion, securityVersion.SecureConversationVersion, false, null, null, null));
        }

        public static byte[] DeriveIssuedTokenKey(Guid transactionId, string sctId)
        {
            return Psha1DerivedKeyGeneratorHelper.GenerateDerivedKey(transactionId.ToByteArray(), Label, Encoding.UTF8.GetBytes(sctId), SecurityAlgorithmSuite.Default.DefaultSymmetricKeyLength, 0);
        }

        public static RequestSecurityTokenResponse GetIssuedToken(Message message, string identifier, ProtocolVersion protocolVersion)
        {
            ICollection<RequestSecurityTokenResponse> is2 = TransactionFlowProperty.TryGetIssuedTokens(message);
            if (is2 == null)
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "No issued tokens found in message");
                }
                return null;
            }
            string str = CoordinationStrings.Version(protocolVersion).Identifier;
            string str2 = CoordinationStrings.Version(protocolVersion).Namespace;
            foreach (RequestSecurityTokenResponse response in is2)
            {
                string str3;
                string str4;
                response.GetAppliesToQName(out str3, out str4);
                if ((str3 == str) && (str4 == str2))
                {
                    if (DebugTrace.Verbose)
                    {
                        DebugTrace.Trace(TraceLevel.Verbose, "Found issued token in message");
                    }
                    try
                    {
                        IdentifierElement appliesTo = null;
                        DataContractSerializer serializer = IdentifierElementSerializer(protocolVersion);
                        ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(CoordinationServiceSecurity), "GetIssuedToken");
                        switch (protocolVersion)
                        {
                            case ProtocolVersion.Version10:
                                appliesTo = response.GetAppliesTo<IdentifierElement10>(serializer);
                                break;

                            case ProtocolVersion.Version11:
                                appliesTo = response.GetAppliesTo<IdentifierElement11>(serializer);
                                break;
                        }
                        if (!(appliesTo.Identifier != identifier))
                        {
                            return response;
                        }
                        if (DebugTrace.Error)
                        {
                            DebugTrace.Trace(TraceLevel.Error, "Issued token identifier does not match expected {0}", identifier);
                        }
                        throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(Microsoft.Transactions.SR.GetString("IssuedTokenIdentifierMismatch")));
                    }
                    catch (SerializationException exception)
                    {
                        if (DebugTrace.Error)
                        {
                            DebugTrace.Trace(TraceLevel.Error, "Issued token AppliesTo element could not be deserialized: {0}", exception.Message);
                        }
                        throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(exception.Message, exception));
                    }
                    return response;
                }
            }
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "No matching issued token found in message");
            }
            return null;
        }

        public static string GetSenderName(Message message)
        {
            return GetSenderName(message.Properties);
        }

        public static string GetSenderName(MessageProperties messageProperties)
        {
            SecurityMessageProperty security = messageProperties.Security;
            if (security == null)
            {
                return "anonymous";
            }
            ServiceSecurityContext serviceSecurityContext = security.ServiceSecurityContext;
            if ((serviceSecurityContext == null) || serviceSecurityContext.IsAnonymous)
            {
                return "anonymous";
            }
            IIdentity primaryIdentity = serviceSecurityContext.PrimaryIdentity;
            if (primaryIdentity != null)
            {
                return primaryIdentity.Name;
            }
            return serviceSecurityContext.PrimaryIdentity.ToString();
        }

        private static DataContractSerializer IdentifierElementSerializer(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(CoordinationServiceSecurity), "IdentifierElementSerializer");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    if (identifierElementSerializer10 == null)
                    {
                        identifierElementSerializer10 = new DataContractSerializer(typeof(IdentifierElement10));
                    }
                    return identifierElementSerializer10;

                case ProtocolVersion.Version11:
                    if (identifierElementSerializer11 == null)
                    {
                        identifierElementSerializer11 = new DataContractSerializer(typeof(IdentifierElement11));
                    }
                    return identifierElementSerializer11;
            }
            return null;
        }

        private void TraceCheckIdentityResult(bool access, EndpointAddress service, Message message)
        {
            if (DebugTrace.Verbose)
            {
                if (DebugTrace.Pii)
                {
                    string senderName = GetSenderName(message);
                    if (access)
                    {
                        DebugTrace.Trace(TraceLevel.Verbose, "Access granted by identity verifier to {0}", senderName);
                    }
                    else
                    {
                        DebugTrace.Trace(TraceLevel.Verbose, "Access denied by identity verifier to {0}, expected {1}", senderName, (service.Identity == null) ? service.Uri.AbsoluteUri : service.Identity.ToString());
                    }
                }
                else if (access)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Access granted by identity verifier");
                }
                else
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Access denied by identity verifier");
                }
            }
        }

        public static System.ServiceModel.Security.Tokens.SecurityContextSecurityTokenParameters SecurityContextSecurityTokenParameters
        {
            get
            {
                if (securityContextSecurityTokenParameters == null)
                {
                    securityContextSecurityTokenParameters = new System.ServiceModel.Security.Tokens.SecurityContextSecurityTokenParameters();
                }
                return securityContextSecurityTokenParameters;
            }
        }
    }
}

