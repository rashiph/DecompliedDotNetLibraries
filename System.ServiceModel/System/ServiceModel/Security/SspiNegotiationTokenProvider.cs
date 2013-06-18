namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal abstract class SspiNegotiationTokenProvider : NegotiationTokenProvider<SspiNegotiationTokenProviderState>
    {
        private bool negotiateTokenOnOpen;
        private SecurityBindingElement securityBindingElement;

        protected SspiNegotiationTokenProvider() : this(null)
        {
        }

        protected SspiNegotiationTokenProvider(SecurityBindingElement securityBindingElement)
        {
            this.securityBindingElement = securityBindingElement;
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

        private static void AddToDigest(SspiNegotiationTokenProviderState sspiState, RequestSecurityToken rst)
        {
            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);
            rst.WriteTo(writer);
            writer.Flush();
            AddToDigest(sspiState.NegotiationDigest, stream);
        }

        private void AddToDigest(SspiNegotiationTokenProviderState sspiState, RequestSecurityTokenResponse rstr, bool wasReceived, bool isFinalRstr)
        {
            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);
            if (!wasReceived)
            {
                rstr.WriteTo(writer);
            }
            else if (!isFinalRstr)
            {
                rstr.RequestSecurityTokenResponseXml.WriteTo(writer);
            }
            else
            {
                XmlElement element = (XmlElement) rstr.RequestSecurityTokenResponseXml.CloneNode(true);
                List<System.Xml.XmlNode> list = new List<System.Xml.XmlNode>(2);
                for (int i = 0; i < element.ChildNodes.Count; i++)
                {
                    System.Xml.XmlNode item = element.ChildNodes[i];
                    if (base.StandardsManager.TrustDriver.IsRequestedSecurityTokenElement(item.LocalName, item.NamespaceURI))
                    {
                        list.Add(item);
                    }
                    else if (base.StandardsManager.TrustDriver.IsRequestedProofTokenElement(item.LocalName, item.NamespaceURI))
                    {
                        list.Add(item);
                    }
                }
                for (int j = 0; j < list.Count; j++)
                {
                    element.RemoveChild(list[j]);
                }
                element.WriteTo(writer);
            }
            writer.Flush();
            AddToDigest(sspiState.NegotiationDigest, stream);
        }

        protected override IRequestChannel CreateClientChannel(EndpointAddress target, Uri via)
        {
            IRequestChannel channel = base.CreateClientChannel(target, via);
            if (!System.ServiceModel.Security.SecurityUtils.IsChannelBindingDisabled && (this.securityBindingElement is TransportSecurityBindingElement))
            {
                IChannelBindingProvider property = channel.GetProperty<IChannelBindingProvider>();
                if (property != null)
                {
                    property.EnableChannelBindingSupport();
                }
            }
            return channel;
        }

        protected override BodyWriter GetFirstOutgoingMessageBody(SspiNegotiationTokenProviderState sspiState, out MessageProperties messageProperties)
        {
            RequestSecurityToken token;
            messageProperties = null;
            return new RequestSecurityToken(base.StandardsManager, false) { Context = sspiState.Context, TokenType = base.StandardsManager.SecureConversationDriver.TokenTypeUri, KeySize = base.SecurityAlgorithmSuite.DefaultSymmetricKeyLength, OnGetBinaryNegotiation = new RequestSecurityToken.OnGetBinaryNegotiationCallback(new GetOutgoingBlobProxy(sspiState, this, token).GetOutgoingBlob) };
        }

        protected override IChannelFactory<IRequestChannel> GetNegotiationChannelFactory(IChannelFactory<IRequestChannel> transportChannelFactory, ChannelBuilder channelBuilder)
        {
            return transportChannelFactory;
        }

        protected override BodyWriter GetNextOutgoingMessageBody(Message incomingMessage, SspiNegotiationTokenProviderState sspiState)
        {
            byte[] negotiationData;
            try
            {
                IssuanceTokenProviderBase<SspiNegotiationTokenProviderState>.ThrowIfFault(incomingMessage, base.TargetAddress);
            }
            catch (FaultException exception)
            {
                if (!exception.Code.IsSenderFault)
                {
                    throw;
                }
                if (!(exception.Code.SubCode.Name == "FailedAuthentication") && !(exception.Code.SubCode.Name == "FailedAuthentication"))
                {
                    throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("FailedSspiNegotiation"), exception), incomingMessage);
                }
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("AuthenticationOfClientFailed"), exception), incomingMessage);
            }
            RequestSecurityTokenResponse rstr = null;
            RequestSecurityTokenResponse authenticatorRstr = null;
            XmlDictionaryReader readerAtBodyContents = incomingMessage.GetReaderAtBodyContents();
            using (readerAtBodyContents)
            {
                if (base.StandardsManager.TrustDriver.IsAtRequestSecurityTokenResponseCollection(readerAtBodyContents))
                {
                    using (IEnumerator<RequestSecurityTokenResponse> enumerator = base.StandardsManager.TrustDriver.CreateRequestSecurityTokenResponseCollection(readerAtBodyContents).RstrCollection.GetEnumerator())
                    {
                        enumerator.MoveNext();
                        rstr = enumerator.Current;
                        if (enumerator.MoveNext())
                        {
                            authenticatorRstr = enumerator.Current;
                        }
                    }
                    if (authenticatorRstr == null)
                    {
                        throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("AuthenticatorNotPresentInRSTRCollection")), incomingMessage);
                    }
                    if (authenticatorRstr.Context != rstr.Context)
                    {
                        throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("RSTRAuthenticatorHasBadContext")), incomingMessage);
                    }
                    this.AddToDigest(sspiState, rstr, true, true);
                }
                else if (base.StandardsManager.TrustDriver.IsAtRequestSecurityTokenResponse(readerAtBodyContents))
                {
                    rstr = RequestSecurityTokenResponse.CreateFrom(base.StandardsManager, readerAtBodyContents);
                    this.AddToDigest(sspiState, rstr, true, false);
                }
                else
                {
                    base.StandardsManager.TrustDriver.OnRSTRorRSTRCMissingException();
                }
                incomingMessage.ReadFromBodyContentsToEnd(readerAtBodyContents);
            }
            if (rstr.Context != sspiState.Context)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("BadSecurityNegotiationContext")), incomingMessage);
            }
            BinaryNegotiation binaryNegotiation = rstr.GetBinaryNegotiation();
            if (binaryNegotiation != null)
            {
                this.ValidateIncomingBinaryNegotiation(binaryNegotiation);
                negotiationData = binaryNegotiation.GetNegotiationData();
            }
            else
            {
                negotiationData = null;
            }
            if ((negotiationData == null) && !sspiState.SspiNegotiation.IsCompleted)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NoBinaryNegoToReceive")), incomingMessage);
            }
            if ((negotiationData == null) && sspiState.SspiNegotiation.IsCompleted)
            {
                this.OnNegotiationComplete(sspiState, rstr, authenticatorRstr);
                return null;
            }
            byte[] outgoingBlob = sspiState.SspiNegotiation.GetOutgoingBlob(negotiationData, System.ServiceModel.Security.SecurityUtils.GetChannelBindingFromMessage(incomingMessage), null);
            if ((outgoingBlob == null) && !sspiState.SspiNegotiation.IsCompleted)
            {
                throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NoBinaryNegoToSend")), incomingMessage);
            }
            if ((outgoingBlob == null) && sspiState.SspiNegotiation.IsCompleted)
            {
                this.OnNegotiationComplete(sspiState, rstr, authenticatorRstr);
                return null;
            }
            return this.PrepareRstr(sspiState, outgoingBlob);
        }

        private static bool IsCorrectAuthenticator(SspiNegotiationTokenProviderState sspiState, byte[] proofKey, byte[] serverAuthenticator)
        {
            byte[] hash;
            lock (sspiState.NegotiationDigest)
            {
                sspiState.NegotiationDigest.TransformFinalBlock(System.ServiceModel.Security.CryptoHelper.EmptyBuffer, 0, 0);
                hash = sspiState.NegotiationDigest.Hash;
            }
            byte[] buffer2 = new Psha1DerivedKeyGenerator(proofKey).GenerateDerivedKey(System.ServiceModel.Security.SecurityUtils.CombinedHashLabel, hash, 0x100, 0);
            if (buffer2.Length != serverAuthenticator.Length)
            {
                return false;
            }
            for (int i = 0; i < buffer2.Length; i++)
            {
                if (buffer2[i] != serverAuthenticator[i])
                {
                    return false;
                }
            }
            return true;
        }

        private void OnNegotiationComplete(SspiNegotiationTokenProviderState sspiState, RequestSecurityTokenResponse negotiationRstr, RequestSecurityTokenResponse authenticatorRstr)
        {
            ISspiNegotiation sspiNegotiation = sspiState.SspiNegotiation;
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = this.ValidateSspiNegotiation(sspiNegotiation);
            SecurityTokenResolver resolver = new SspiSecurityTokenResolver(sspiNegotiation);
            GenericXmlSecurityToken serviceToken = negotiationRstr.GetIssuedToken(resolver, System.ServiceModel.Security.EmptyReadOnlyCollection<SecurityTokenAuthenticator>.Instance, SecurityKeyEntropyMode.ServerEntropy, null, base.SecurityContextTokenUri, authorizationPolicies, 0, false);
            if (serviceToken == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NoServiceTokenReceived")));
            }
            WrappedKeySecurityToken proofToken = serviceToken.ProofToken as WrappedKeySecurityToken;
            if ((proofToken == null) || (proofToken.WrappingAlgorithm != sspiNegotiation.KeyEncryptionAlgorithm))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("ProofTokenWasNotWrappedCorrectly")));
            }
            byte[] wrappedKey = proofToken.GetWrappedKey();
            if (authenticatorRstr == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("RSTRAuthenticatorNotPresent")));
            }
            byte[] authenticator = authenticatorRstr.GetAuthenticator();
            if (authenticator == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("RSTRAuthenticatorNotPresent")));
            }
            if (!IsCorrectAuthenticator(sspiState, wrappedKey, authenticator))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("RSTRAuthenticatorIncorrect")));
            }
            sspiState.SetServiceToken(serviceToken);
        }

        public override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.EnsureEndpointAddressDoesNotRequireEncryption(base.TargetAddress);
            base.OnOpen(helper.RemainingTime());
            if (this.negotiateTokenOnOpen)
            {
                base.DoNegotiation(helper.RemainingTime());
            }
        }

        private BodyWriter PrepareRstr(SspiNegotiationTokenProviderState sspiState, byte[] outgoingBlob)
        {
            RequestSecurityTokenResponse rstr = new RequestSecurityTokenResponse(base.StandardsManager) {
                Context = sspiState.Context
            };
            rstr.SetBinaryNegotiation(new BinaryNegotiation(this.NegotiationValueType, outgoingBlob));
            rstr.MakeReadOnly();
            this.AddToDigest(sspiState, rstr, false, false);
            return rstr;
        }

        private void ValidateIncomingBinaryNegotiation(BinaryNegotiation incomingNego)
        {
            incomingNego.Validate(this.NegotiationValueType);
        }

        protected abstract ReadOnlyCollection<IAuthorizationPolicy> ValidateSspiNegotiation(ISspiNegotiation sspiNegotiation);

        public bool NegotiateTokenOnOpen
        {
            get
            {
                return this.negotiateTokenOnOpen;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.negotiateTokenOnOpen = value;
            }
        }

        public abstract XmlDictionaryString NegotiationValueType { get; }

        private class GetOutgoingBlobProxy
        {
            private RequestSecurityToken _rst;
            private SspiNegotiationTokenProvider _sspiProvider;
            private SspiNegotiationTokenProviderState _sspiState;

            public GetOutgoingBlobProxy(SspiNegotiationTokenProviderState sspiState, SspiNegotiationTokenProvider sspiProvider, RequestSecurityToken rst)
            {
                this._sspiState = sspiState;
                this._sspiProvider = sspiProvider;
                this._rst = rst;
            }

            public void GetOutgoingBlob(ChannelBinding channelBinding)
            {
                byte[] negotiationData = this._sspiState.SspiNegotiation.GetOutgoingBlob(null, channelBinding, null);
                if ((negotiationData == null) && !this._sspiState.SspiNegotiation.IsCompleted)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NoBinaryNegoToSend")));
                }
                this._rst.SetBinaryNegotiation(new BinaryNegotiation(this._sspiProvider.NegotiationValueType, negotiationData));
                SspiNegotiationTokenProvider.AddToDigest(this._sspiState, this._rst);
                this._rst.MakeReadOnly();
            }
        }

        private class SspiSecurityTokenResolver : SecurityTokenResolver, ISspiNegotiationInfo
        {
            private ISspiNegotiation sspiNegotiation;

            public SspiSecurityTokenResolver(ISspiNegotiation sspiNegotiation)
            {
                this.sspiNegotiation = sspiNegotiation;
            }

            protected override bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
            {
                key = null;
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            protected override bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
            {
                token = null;
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            protected override bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
            {
                token = null;
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

            public ISspiNegotiation SspiNegotiation
            {
                get
                {
                    return this.sspiNegotiation;
                }
            }
        }
    }
}

