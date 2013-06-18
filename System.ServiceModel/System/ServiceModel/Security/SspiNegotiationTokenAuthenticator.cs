namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal abstract class SspiNegotiationTokenAuthenticator : NegotiationTokenAuthenticator<SspiNegotiationTokenAuthenticatorState>
    {
        private string defaultServiceBinding;
        private System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy extendedProtectionPolicy;
        private object thisLock = new object();

        protected SspiNegotiationTokenAuthenticator()
        {
        }

        private static void AddToDigest(HashAlgorithm negotiationDigest, Stream stream)
        {
            stream.Flush();
            stream.Seek(0L, SeekOrigin.Begin);
            CanonicalizationDriver driver = new CanonicalizationDriver();
            driver.SetInput(stream);
            byte[] bytes = driver.GetBytes();
            lock (negotiationDigest)
            {
                negotiationDigest.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
            }
        }

        private static void AddToDigest(SspiNegotiationTokenAuthenticatorState sspiState, RequestSecurityToken rst)
        {
            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter w = XmlDictionaryWriter.CreateTextWriter(stream);
            rst.RequestSecurityTokenXml.WriteTo(w);
            w.Flush();
            AddToDigest(sspiState.NegotiationDigest, stream);
        }

        private static void AddToDigest(SspiNegotiationTokenAuthenticatorState sspiState, RequestSecurityTokenResponse rstr, bool wasReceived)
        {
            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter w = XmlDictionaryWriter.CreateTextWriter(stream);
            if (wasReceived)
            {
                rstr.RequestSecurityTokenResponseXml.WriteTo(w);
            }
            else
            {
                rstr.WriteTo(w);
            }
            w.Flush();
            AddToDigest(sspiState.NegotiationDigest, stream);
        }

        private static byte[] ComputeAuthenticator(SspiNegotiationTokenAuthenticatorState sspiState, byte[] key)
        {
            byte[] hash;
            lock (sspiState.NegotiationDigest)
            {
                sspiState.NegotiationDigest.TransformFinalBlock(System.ServiceModel.Security.CryptoHelper.EmptyBuffer, 0, 0);
                hash = sspiState.NegotiationDigest.Hash;
            }
            Psha1DerivedKeyGenerator generator = new Psha1DerivedKeyGenerator(key);
            return generator.GenerateDerivedKey(System.ServiceModel.Security.SecurityUtils.CombinedHashLabel, hash, 0x100, 0);
        }

        protected abstract SspiNegotiationTokenAuthenticatorState CreateSspiState(byte[] incomingBlob, string incomingValueTypeUri);
        protected override MessageFilter GetListenerFilter()
        {
            return new SspiNegotiationFilter(this);
        }

        protected override Binding GetNegotiationBinding(Binding binding)
        {
            return binding;
        }

        protected virtual BinaryNegotiation GetOutgoingBinaryNegotiation(ISspiNegotiation sspiNegotiation, byte[] outgoingBlob)
        {
            return new BinaryNegotiation(this.NegotiationValueType, outgoingBlob);
        }

        protected virtual void IssueServiceToken(SspiNegotiationTokenAuthenticatorState sspiState, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, out SecurityContextSecurityToken serviceToken, out WrappedKeySecurityToken proofToken, out int issuedKeySize)
        {
            UniqueId contextId = System.ServiceModel.Security.SecurityUtils.GenerateUniqueId();
            string id = System.ServiceModel.Security.SecurityUtils.GenerateId();
            if (sspiState.RequestedKeySize == 0)
            {
                issuedKeySize = base.SecurityAlgorithmSuite.DefaultSymmetricKeyLength;
            }
            else
            {
                issuedKeySize = sspiState.RequestedKeySize;
            }
            byte[] buffer = new byte[issuedKeySize / 8];
            System.ServiceModel.Security.CryptoHelper.FillRandomBytes(buffer);
            DateTime utcNow = DateTime.UtcNow;
            DateTime tokenExpirationTime = TimeoutHelper.Add(utcNow, base.ServiceTokenLifetime);
            serviceToken = base.IssueSecurityContextToken(contextId, id, buffer, utcNow, tokenExpirationTime, authorizationPolicies, base.EncryptStateInServiceToken);
            proofToken = new WrappedKeySecurityToken(string.Empty, buffer, sspiState.SspiNegotiation);
        }

        private BodyWriter ProcessNegotiation(SspiNegotiationTokenAuthenticatorState negotiationState, Message incomingMessage, BinaryNegotiation incomingNego)
        {
            BinaryNegotiation outgoingBinaryNegotiation;
            ISspiNegotiation sspiNegotiation = negotiationState.SspiNegotiation;
            byte[] outgoingBlob = sspiNegotiation.GetOutgoingBlob(incomingNego.GetNegotiationData(), System.ServiceModel.Security.SecurityUtils.GetChannelBindingFromMessage(incomingMessage), this.extendedProtectionPolicy);
            if (!sspiNegotiation.IsValidContext)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidSspiNegotiation")), incomingMessage);
            }
            if ((outgoingBlob == null) && !sspiNegotiation.IsCompleted)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NoBinaryNegoToSend")), incomingMessage);
            }
            if (outgoingBlob != null)
            {
                outgoingBinaryNegotiation = this.GetOutgoingBinaryNegotiation(sspiNegotiation, outgoingBlob);
            }
            else
            {
                outgoingBinaryNegotiation = null;
            }
            if (sspiNegotiation.IsCompleted)
            {
                SecurityContextSecurityToken token;
                WrappedKeySecurityToken token2;
                int num;
                ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = this.ValidateSspiNegotiation(sspiNegotiation);
                this.IssueServiceToken(negotiationState, authorizationPolicies, out token, out token2, out num);
                negotiationState.SetServiceToken(token);
                SecurityKeyIdentifierClause clause = base.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(token, SecurityTokenReferenceStyle.External);
                SecurityKeyIdentifierClause clause2 = base.IssuedSecurityTokenParameters.CreateKeyIdentifierClause(token, SecurityTokenReferenceStyle.Internal);
                RequestSecurityTokenResponse response = new RequestSecurityTokenResponse(base.StandardsManager) {
                    Context = negotiationState.Context,
                    KeySize = num,
                    TokenType = base.SecurityContextTokenUri
                };
                if (outgoingBinaryNegotiation != null)
                {
                    response.SetBinaryNegotiation(outgoingBinaryNegotiation);
                }
                response.RequestedUnattachedReference = clause;
                response.RequestedAttachedReference = clause2;
                response.SetLifetime(token.ValidFrom, token.ValidTo);
                if (negotiationState.AppliesTo != null)
                {
                    if (incomingMessage.Version.Addressing != AddressingVersion.WSAddressing10)
                    {
                        if (incomingMessage.Version.Addressing != AddressingVersion.WSAddressingAugust2004)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { incomingMessage.Version.Addressing })));
                        }
                        response.SetAppliesTo<EndpointAddressAugust2004>(EndpointAddressAugust2004.FromEndpointAddress(negotiationState.AppliesTo), negotiationState.AppliesToSerializer);
                    }
                    else
                    {
                        response.SetAppliesTo<EndpointAddress10>(EndpointAddress10.FromEndpointAddress(negotiationState.AppliesTo), negotiationState.AppliesToSerializer);
                    }
                }
                response.MakeReadOnly();
                AddToDigest(negotiationState, response, false);
                RequestSecurityTokenResponse response2 = new RequestSecurityTokenResponse(base.StandardsManager) {
                    RequestedSecurityToken = token,
                    RequestedProofToken = token2,
                    Context = negotiationState.Context,
                    KeySize = num,
                    TokenType = base.SecurityContextTokenUri
                };
                if (outgoingBinaryNegotiation != null)
                {
                    response2.SetBinaryNegotiation(outgoingBinaryNegotiation);
                }
                response2.RequestedAttachedReference = clause2;
                response2.RequestedUnattachedReference = clause;
                if (negotiationState.AppliesTo != null)
                {
                    if (incomingMessage.Version.Addressing != AddressingVersion.WSAddressing10)
                    {
                        if (incomingMessage.Version.Addressing != AddressingVersion.WSAddressingAugust2004)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { incomingMessage.Version.Addressing })));
                        }
                        response2.SetAppliesTo<EndpointAddressAugust2004>(EndpointAddressAugust2004.FromEndpointAddress(negotiationState.AppliesTo), negotiationState.AppliesToSerializer);
                    }
                    else
                    {
                        response2.SetAppliesTo<EndpointAddress10>(EndpointAddress10.FromEndpointAddress(negotiationState.AppliesTo), negotiationState.AppliesToSerializer);
                    }
                }
                response2.MakeReadOnly();
                byte[] authenticator = ComputeAuthenticator(negotiationState, token.GetKeyBytes());
                RequestSecurityTokenResponse response3 = new RequestSecurityTokenResponse(base.StandardsManager) {
                    Context = negotiationState.Context
                };
                response3.SetAuthenticator(authenticator);
                response3.MakeReadOnly();
                return new RequestSecurityTokenResponseCollection(new List<RequestSecurityTokenResponse>(2) { response2, response3 }, base.StandardsManager);
            }
            RequestSecurityTokenResponse rstr = new RequestSecurityTokenResponse(base.StandardsManager) {
                Context = negotiationState.Context
            };
            rstr.SetBinaryNegotiation(outgoingBinaryNegotiation);
            rstr.MakeReadOnly();
            AddToDigest(negotiationState, rstr, false);
            return rstr;
        }

        protected override BodyWriter ProcessRequestSecurityToken(Message request, RequestSecurityToken requestSecurityToken, out SspiNegotiationTokenAuthenticatorState negotiationState)
        {
            string str;
            string str2;
            if (request == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }
            if (requestSecurityToken == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("requestSecurityToken", request);
            }
            if ((requestSecurityToken.RequestType != null) && (requestSecurityToken.RequestType != base.StandardsManager.TrustDriver.RequestTypeIssue))
            {
                throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidRstRequestType", new object[] { requestSecurityToken.RequestType })), request);
            }
            BinaryNegotiation binaryNegotiation = requestSecurityToken.GetBinaryNegotiation();
            this.ValidateIncomingBinaryNegotiation(binaryNegotiation);
            negotiationState = this.CreateSspiState(binaryNegotiation.GetNegotiationData(), binaryNegotiation.ValueTypeUri);
            AddToDigest(negotiationState, requestSecurityToken);
            negotiationState.Context = requestSecurityToken.Context;
            if (requestSecurityToken.KeySize != 0)
            {
                WSTrust.Driver.ValidateRequestedKeySize(requestSecurityToken.KeySize, base.SecurityAlgorithmSuite);
            }
            negotiationState.RequestedKeySize = requestSecurityToken.KeySize;
            requestSecurityToken.GetAppliesToQName(out str, out str2);
            if ((str == "EndpointReference") && (str2 == request.Version.Addressing.Namespace))
            {
                DataContractSerializer serializer;
                if (request.Version.Addressing == AddressingVersion.WSAddressing10)
                {
                    serializer = DataContractSerializerDefaults.CreateSerializer(typeof(EndpointAddress10), 0x10000);
                    negotiationState.AppliesTo = requestSecurityToken.GetAppliesTo<EndpointAddress10>(serializer).ToEndpointAddress();
                }
                else
                {
                    if (request.Version.Addressing != AddressingVersion.WSAddressingAugust2004)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { request.Version.Addressing })));
                    }
                    serializer = DataContractSerializerDefaults.CreateSerializer(typeof(EndpointAddressAugust2004), 0x10000);
                    negotiationState.AppliesTo = requestSecurityToken.GetAppliesTo<EndpointAddressAugust2004>(serializer).ToEndpointAddress();
                }
                negotiationState.AppliesToSerializer = serializer;
            }
            return this.ProcessNegotiation(negotiationState, request, binaryNegotiation);
        }

        protected override BodyWriter ProcessRequestSecurityTokenResponse(SspiNegotiationTokenAuthenticatorState negotiationState, Message request, RequestSecurityTokenResponse requestSecurityTokenResponse)
        {
            if (request == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }
            if (requestSecurityTokenResponse == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("requestSecurityTokenResponse", request);
            }
            if (requestSecurityTokenResponse.Context != negotiationState.Context)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("BadSecurityNegotiationContext")), request);
            }
            AddToDigest(negotiationState, requestSecurityTokenResponse, true);
            BinaryNegotiation binaryNegotiation = requestSecurityTokenResponse.GetBinaryNegotiation();
            this.ValidateIncomingBinaryNegotiation(binaryNegotiation);
            return this.ProcessNegotiation(negotiationState, request, binaryNegotiation);
        }

        protected virtual void ValidateIncomingBinaryNegotiation(BinaryNegotiation incomingNego)
        {
            if (incomingNego == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NoBinaryNegoToReceive")));
            }
            incomingNego.Validate(this.NegotiationValueType);
        }

        protected abstract ReadOnlyCollection<IAuthorizationPolicy> ValidateSspiNegotiation(ISspiNegotiation sspiNegotiation);

        public string DefaultServiceBinding
        {
            get
            {
                if (this.defaultServiceBinding == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.defaultServiceBinding == null)
                        {
                            this.defaultServiceBinding = System.ServiceModel.Security.SecurityUtils.GetSpnFromIdentity(System.ServiceModel.Security.SecurityUtils.CreateWindowsIdentity(), new EndpointAddress(base.ListenUri, new AddressHeader[0]));
                        }
                    }
                }
                return this.defaultServiceBinding;
            }
            set
            {
                this.defaultServiceBinding = value;
            }
        }

        public System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy ExtendedProtectionPolicy
        {
            get
            {
                return this.extendedProtectionPolicy;
            }
            set
            {
                this.extendedProtectionPolicy = value;
            }
        }

        protected override bool IsMultiLegNegotiation
        {
            get
            {
                return true;
            }
        }

        public abstract XmlDictionaryString NegotiationValueType { get; }

        protected object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        private class SspiNegotiationFilter : HeaderFilter
        {
            private SspiNegotiationTokenAuthenticator authenticator;

            public SspiNegotiationFilter(SspiNegotiationTokenAuthenticator authenticator)
            {
                this.authenticator = authenticator;
            }

            public override bool Match(Message message)
            {
                if (!(message.Headers.Action == this.authenticator.RequestSecurityTokenAction.Value) && !(message.Headers.Action == this.authenticator.RequestSecurityTokenResponseAction.Value))
                {
                    return false;
                }
                return !SecurityVersion.Default.DoesMessageContainSecurityHeader(message);
            }
        }
    }
}

