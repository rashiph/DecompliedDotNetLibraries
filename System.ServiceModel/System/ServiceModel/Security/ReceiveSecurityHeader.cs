namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal abstract class ReceiveSecurityHeader : SecurityHeader
    {
        private List<SecurityTokenAuthenticator> allowedAuthenticators;
        private bool allowFirstTokenMismatch;
        private const int AppendPosition = -1;
        private Collection<SecurityToken> basicTokens;
        private ChannelBinding channelBinding;
        private TimeSpan clockSkew;
        private SecurityTokenResolver combinedPrimaryTokenResolver;
        private SecurityTokenResolver combinedUniversalTokenResolver;
        private SecurityTokenAuthenticator derivedTokenAuthenticator;
        private ReceiveSecurityHeaderElementManager elementManager;
        private OperationTracker encryptionTracker;
        private Collection<SecurityToken> endorsingTokens;
        private bool enforceDerivedKeyRequirement;
        private bool expectBasicTokens;
        private SecurityToken expectedEncryptionToken;
        private SecurityTokenParameters expectedEncryptionTokenParameters;
        private bool expectEncryption;
        private bool expectEndorsingTokens;
        private bool expectSignature;
        private bool expectSignatureConfirmation;
        private bool expectSignedTokens;
        private ExtendedProtectionPolicy extendedProtectionPolicy;
        private bool hasAtLeastOneItemInsideSecurityHeaderEncrypted;
        private bool hasAtLeastOneSupportingTokenExpectedToBeSigned;
        private bool hasEndorsingOrSignedEndorsingSupportingTokens;
        private readonly int headerIndex;
        private int maxDerivedKeyLength;
        private int maxDerivedKeys;
        private long maxReceivedMessageSize;
        private NonceCache nonceCache;
        private int numDerivedKeys;
        private OrderTracker orderTracker;
        private SecurityToken outOfBandPrimaryToken;
        private IList<SecurityToken> outOfBandPrimaryTokenCollection;
        private ReadOnlyCollection<SecurityTokenResolver> outOfBandTokenResolver;
        private SecurityTokenAuthenticator pendingSupportingTokenAuthenticator;
        private byte[] primarySignatureValue;
        private SecurityTokenAuthenticator primaryTokenAuthenticator;
        private SecurityTokenParameters primaryTokenParameters;
        private SecurityHeaderTokenResolver primaryTokenResolver;
        private TokenTracker primaryTokenTracker;
        private MessageProtectionOrder protectionOrder;
        private XmlDictionaryReaderQuotas readerQuotas;
        private SignatureConfirmations receivedSignatureConfirmations;
        private SignatureConfirmations receivedSignatureValues;
        private TimeSpan replayWindow;
        private SignatureResourcePool resourcePool;
        private System.ServiceModel.Channels.XmlAttributeHolder[] securityElementAttributes;
        private System.ServiceModel.Security.SecurityVerifiedMessage securityVerifiedMessage;
        private OperationTracker signatureTracker;
        private Collection<SecurityToken> signedEndorsingTokens;
        private Collection<SecurityToken> signedTokens;
        private IList<SupportingTokenAuthenticatorSpecification> supportingTokenAuthenticators;
        private List<TokenTracker> supportingTokenTrackers;
        private TimeoutHelper timeoutHelper;
        private SecurityTimestamp timestamp;
        private Dictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>> tokenPoliciesMapping;
        private SecurityHeaderTokenResolver universalTokenResolver;
        private List<SecurityTokenAuthenticator> wrappedKeyAuthenticator;
        private WrappedKeySecurityToken wrappedKeyToken;
        private SecurityToken wrappingToken;
        private SecurityTokenParameters wrappingTokenParameters;

        protected ReceiveSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay, SecurityStandardsManager standardsManager, SecurityAlgorithmSuite algorithmSuite, int headerIndex, MessageDirection direction) : base(message, actor, mustUnderstand, relay, standardsManager, algorithmSuite, direction)
        {
            this.expectEncryption = true;
            this.expectSignature = true;
            this.orderTracker = new OrderTracker();
            this.signatureTracker = new OperationTracker();
            this.encryptionTracker = new OperationTracker();
            this.enforceDerivedKeyRequirement = true;
            this.maxReceivedMessageSize = 0x10000L;
            this.headerIndex = headerIndex;
            this.elementManager = new ReceiveSecurityHeaderElementManager(this);
        }

        private void AddDerivedKeyTokenToResolvers(SecurityToken token)
        {
            this.universalTokenResolver.Add(token);
            SecurityToken rootToken = this.GetRootToken(token);
            if (this.IsPrimaryToken(rootToken))
            {
                this.primaryTokenResolver.Add(token);
            }
        }

        private void AddIncomingSignatureConfirmation(byte[] signatureValue, bool isFromDecryptedSource)
        {
            if (base.MaintainSignatureConfirmationState)
            {
                if (this.receivedSignatureConfirmations == null)
                {
                    this.receivedSignatureConfirmations = new SignatureConfirmations();
                }
                this.receivedSignatureConfirmations.AddConfirmation(signatureValue, isFromDecryptedSource);
            }
        }

        private void AddIncomingSignatureValue(byte[] signatureValue, bool isFromDecryptedSource)
        {
            if (base.MaintainSignatureConfirmationState && !this.ExpectSignatureConfirmation)
            {
                if (this.receivedSignatureValues == null)
                {
                    this.receivedSignatureValues = new SignatureConfirmations();
                }
                this.receivedSignatureValues.AddConfirmation(signatureValue, isFromDecryptedSource);
            }
        }

        private static void AddNonce(NonceCache cache, byte[] nonce)
        {
            if (!cache.TryAddNonce(nonce))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("InvalidOrReplayedNonce"), true));
            }
        }

        private static void CheckNonce(NonceCache cache, byte[] nonce)
        {
            if (cache.CheckNonce(nonce))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("InvalidOrReplayedNonce"), true));
            }
        }

        public void ConfigureAsymmetricBindingClientReceiveHeader(SecurityToken primaryToken, SecurityTokenParameters primaryTokenParameters, SecurityToken encryptionToken, SecurityTokenParameters encryptionTokenParameters, SecurityTokenAuthenticator primaryTokenAuthenticator)
        {
            this.outOfBandPrimaryToken = primaryToken;
            this.primaryTokenParameters = primaryTokenParameters;
            this.primaryTokenAuthenticator = primaryTokenAuthenticator;
            this.allowFirstTokenMismatch = primaryTokenAuthenticator != null;
            if ((encryptionToken != null) && !System.ServiceModel.Security.SecurityUtils.HasSymmetricSecurityKey(encryptionToken))
            {
                this.wrappingToken = encryptionToken;
                this.wrappingTokenParameters = encryptionTokenParameters;
            }
            else
            {
                this.expectedEncryptionToken = encryptionToken;
                this.expectedEncryptionTokenParameters = encryptionTokenParameters;
            }
        }

        public void ConfigureAsymmetricBindingServerReceiveHeader(SecurityTokenAuthenticator primaryTokenAuthenticator, SecurityTokenParameters primaryTokenParameters, SecurityToken wrappingToken, SecurityTokenParameters wrappingTokenParameters, IList<SupportingTokenAuthenticatorSpecification> supportingTokenAuthenticators)
        {
            this.primaryTokenAuthenticator = primaryTokenAuthenticator;
            this.primaryTokenParameters = primaryTokenParameters;
            this.wrappingToken = wrappingToken;
            this.wrappingTokenParameters = wrappingTokenParameters;
            this.supportingTokenAuthenticators = supportingTokenAuthenticators;
        }

        public void ConfigureOutOfBandTokenResolver(ReadOnlyCollection<SecurityTokenResolver> outOfBandResolvers)
        {
            if (outOfBandResolvers == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("outOfBandResolvers");
            }
            if (outOfBandResolvers.Count != 0)
            {
                this.outOfBandTokenResolver = outOfBandResolvers;
            }
        }

        public void ConfigureSymmetricBindingClientReceiveHeader(IList<SecurityToken> primaryTokens, SecurityTokenParameters primaryTokenParameters)
        {
            this.outOfBandPrimaryTokenCollection = primaryTokens;
            this.primaryTokenParameters = primaryTokenParameters;
        }

        public void ConfigureSymmetricBindingClientReceiveHeader(SecurityToken primaryToken, SecurityTokenParameters primaryTokenParameters)
        {
            this.outOfBandPrimaryToken = primaryToken;
            this.primaryTokenParameters = primaryTokenParameters;
        }

        public void ConfigureSymmetricBindingServerReceiveHeader(SecurityTokenAuthenticator primaryTokenAuthenticator, SecurityTokenParameters primaryTokenParameters, IList<SupportingTokenAuthenticatorSpecification> supportingTokenAuthenticators)
        {
            this.primaryTokenAuthenticator = primaryTokenAuthenticator;
            this.primaryTokenParameters = primaryTokenParameters;
            this.supportingTokenAuthenticators = supportingTokenAuthenticators;
        }

        public void ConfigureSymmetricBindingServerReceiveHeader(SecurityToken wrappingToken, SecurityTokenParameters wrappingTokenParameters, IList<SupportingTokenAuthenticatorSpecification> supportingTokenAuthenticators)
        {
            this.wrappingToken = wrappingToken;
            this.wrappingTokenParameters = wrappingTokenParameters;
            this.supportingTokenAuthenticators = supportingTokenAuthenticators;
        }

        public void ConfigureTransportBindingServerReceiveHeader(IList<SupportingTokenAuthenticatorSpecification> supportingTokenAuthenticators)
        {
            this.supportingTokenAuthenticators = supportingTokenAuthenticators;
        }

        internal XmlDictionaryReader CreateDecryptedReader(byte[] decryptedBuffer)
        {
            return ContextImportHelper.CreateSplicedReader(decryptedBuffer, this.SecurityVerifiedMessage.GetEnvelopeAttributes(), this.SecurityVerifiedMessage.GetHeaderAttributes(), this.securityElementAttributes, this.ReaderQuotas);
        }

        internal XmlDictionaryReader CreateSecurityHeaderReader()
        {
            return this.securityVerifiedMessage.GetReaderAtSecurityHeader();
        }

        protected abstract byte[] DecryptSecurityHeaderElement(EncryptedData encryptedData, WrappedKeySecurityToken wrappedKeyToken, out SecurityToken encryptionToken);
        protected abstract WrappedKeySecurityToken DecryptWrappedKey(XmlDictionaryReader reader);
        protected abstract void EnsureDecryptionComplete();
        internal void EnsureDerivedKeyLimitNotReached()
        {
            this.numDerivedKeys++;
            if (this.numDerivedKeys > this.maxDerivedKeys)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("DerivedKeyLimitExceeded", new object[] { this.maxDerivedKeys })));
            }
        }

        private Collection<SecurityToken> EnsureSupportingTokens(ref Collection<SecurityToken> list)
        {
            if (list == null)
            {
                list = new Collection<SecurityToken>();
            }
            return list;
        }

        internal void ExecuteDerivedKeyTokenStubPass(bool isFinalPass)
        {
            for (int i = 0; i < this.elementManager.Count; i++)
            {
                if (this.elementManager.GetElementCategory(i) == ReceiveSecurityHeaderElementCategory.Token)
                {
                    DerivedKeySecurityTokenStub element = this.elementManager.GetElement(i) as DerivedKeySecurityTokenStub;
                    if (element != null)
                    {
                        SecurityToken token = null;
                        this.universalTokenResolver.TryResolveToken(element.TokenToDeriveIdentifier, out token);
                        if (token != null)
                        {
                            this.EnsureDerivedKeyLimitNotReached();
                            DerivedKeySecurityToken token2 = element.CreateToken(token, this.maxDerivedKeyLength);
                            this.elementManager.SetElement(i, token2);
                            this.AddDerivedKeyTokenToResolvers(token2);
                        }
                        else if (isFinalPass)
                        {
                            throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnableToResolveKeyInfoClauseInDerivedKeyToken", new object[] { element.TokenToDeriveIdentifier })), base.Message);
                        }
                    }
                }
            }
        }

        internal void ExecuteFullPass(XmlDictionaryReader reader)
        {
            bool primarySignatureFound = !base.RequireMessageProtection;
            for (int i = 0; reader.IsStartElement(); i++)
            {
                if (this.IsReaderAtSignature(reader))
                {
                    SignedXml signedXml = this.ReadSignature(reader, -1, null);
                    if (primarySignatureFound)
                    {
                        this.elementManager.SetBindingMode(i, ReceiveSecurityHeaderBindingModes.Endorsing);
                        this.ProcessSupportingSignature(signedXml, false);
                    }
                    else
                    {
                        primarySignatureFound = true;
                        this.elementManager.SetBindingMode(i, ReceiveSecurityHeaderBindingModes.Primary);
                        this.ProcessPrimarySignature(signedXml, false);
                    }
                }
                else if (this.IsReaderAtReferenceList(reader))
                {
                    ReferenceList referenceList = this.ReadReferenceList(reader);
                    this.ProcessReferenceList(referenceList);
                }
                else if (base.StandardsManager.WSUtilitySpecificationVersion.IsReaderAtTimestamp(reader))
                {
                    this.ReadTimestamp(reader);
                }
                else if (this.IsReaderAtEncryptedKey(reader))
                {
                    this.ReadEncryptedKey(reader, true);
                }
                else if (this.IsReaderAtEncryptedData(reader))
                {
                    EncryptedData encryptedData = this.ReadEncryptedData(reader);
                    this.ProcessEncryptedData(encryptedData, this.timeoutHelper.RemainingTime(), i, true, ref primarySignatureFound);
                }
                else if (base.StandardsManager.SecurityVersion.IsReaderAtSignatureConfirmation(reader))
                {
                    this.ReadSignatureConfirmation(reader, -1, null);
                }
                else
                {
                    this.ReadToken(reader, -1, null, null, null, this.timeoutHelper.RemainingTime());
                }
            }
            reader.ReadEndElement();
            reader.Close();
        }

        protected abstract void ExecuteMessageProtectionPass(bool hasAtLeastOneSupportingTokenExpectedToBeSigned);
        internal void ExecuteReadingPass(XmlDictionaryReader reader)
        {
            for (int i = 0; reader.IsStartElement(); i++)
            {
                if (this.IsReaderAtSignature(reader))
                {
                    this.ReadSignature(reader, -1, null);
                }
                else if (this.IsReaderAtReferenceList(reader))
                {
                    this.ReadReferenceList(reader);
                }
                else if (base.StandardsManager.WSUtilitySpecificationVersion.IsReaderAtTimestamp(reader))
                {
                    this.ReadTimestamp(reader);
                }
                else if (this.IsReaderAtEncryptedKey(reader))
                {
                    this.ReadEncryptedKey(reader, false);
                }
                else if (this.IsReaderAtEncryptedData(reader))
                {
                    this.ReadEncryptedData(reader);
                }
                else if (base.StandardsManager.SecurityVersion.IsReaderAtSignatureConfirmation(reader))
                {
                    this.ReadSignatureConfirmation(reader, -1, null);
                }
                else
                {
                    this.ReadToken(reader, -1, null, null, null, this.timeoutHelper.RemainingTime());
                }
            }
            reader.ReadEndElement();
            reader.Close();
        }

        internal void ExecuteSignatureEncryptionProcessingPass()
        {
            for (int i = 0; i < this.elementManager.Count; i++)
            {
                ReceiveSecurityHeaderEntry entry;
                this.elementManager.GetElementEntry(i, out entry);
                switch (entry.elementCategory)
                {
                    case ReceiveSecurityHeaderElementCategory.Signature:
                    {
                        if (entry.bindingMode != ReceiveSecurityHeaderBindingModes.Primary)
                        {
                            break;
                        }
                        this.ProcessPrimarySignature((SignedXml) entry.element, entry.encrypted);
                        continue;
                    }
                    case ReceiveSecurityHeaderElementCategory.EncryptedData:
                    case ReceiveSecurityHeaderElementCategory.EncryptedKey:
                    case ReceiveSecurityHeaderElementCategory.SignatureConfirmation:
                    case ReceiveSecurityHeaderElementCategory.Timestamp:
                    {
                        continue;
                    }
                    case ReceiveSecurityHeaderElementCategory.ReferenceList:
                    {
                        this.ProcessReferenceList((ReferenceList) entry.element);
                        continue;
                    }
                    case ReceiveSecurityHeaderElementCategory.Token:
                    {
                        WrappedKeySecurityToken element = entry.element as WrappedKeySecurityToken;
                        if ((element != null) && (element.ReferenceList != null))
                        {
                            this.ProcessReferenceList(element.ReferenceList, element);
                        }
                        continue;
                    }
                    default:
                    {
                        continue;
                    }
                }
                this.ProcessSupportingSignature((SignedXml) entry.element, entry.encrypted);
            }
        }

        internal void ExecuteSubheaderDecryptionPass()
        {
            for (int i = 0; i < this.elementManager.Count; i++)
            {
                if (this.elementManager.GetElementCategory(i) == ReceiveSecurityHeaderElementCategory.EncryptedData)
                {
                    EncryptedData element = this.elementManager.GetElement<EncryptedData>(i);
                    bool primarySignatureFound = false;
                    this.ProcessEncryptedData(element, this.timeoutHelper.RemainingTime(), i, false, ref primarySignatureFound);
                }
            }
        }

        protected TAuthenticator FindAllowedAuthenticator<TAuthenticator>(bool removeIfPresent) where TAuthenticator: SecurityTokenAuthenticator
        {
            if (this.allowedAuthenticators != null)
            {
                for (int i = 0; i < this.allowedAuthenticators.Count; i++)
                {
                    if (this.allowedAuthenticators[i] is TAuthenticator)
                    {
                        TAuthenticator local = this.allowedAuthenticators[i];
                        if (removeIfPresent)
                        {
                            this.allowedAuthenticators.RemoveAt(i);
                        }
                        return local;
                    }
                }
            }
            return default(TAuthenticator);
        }

        private XmlDictionaryReader GetReaderAtSecurityHeader()
        {
            XmlDictionaryReader readerAtFirstHeader = this.SecurityVerifiedMessage.GetReaderAtFirstHeader();
            for (int i = 0; i < this.HeaderIndex; i++)
            {
                readerAtFirstHeader.Skip();
            }
            return readerAtFirstHeader;
        }

        private SecurityToken GetRootToken(SecurityToken token)
        {
            if (token is DerivedKeySecurityToken)
            {
                return ((DerivedKeySecurityToken) token).TokenToDerive;
            }
            return token;
        }

        public SignatureConfirmations GetSentSignatureConfirmations()
        {
            return this.receivedSignatureConfirmations;
        }

        public SignatureConfirmations GetSentSignatureValues()
        {
            return this.receivedSignatureValues;
        }

        private TokenTracker GetSupportingTokenTracker(SecurityToken token)
        {
            if (this.supportingTokenTrackers != null)
            {
                for (int i = 0; i < this.supportingTokenTrackers.Count; i++)
                {
                    if (this.supportingTokenTrackers[i].token == token)
                    {
                        return this.supportingTokenTrackers[i];
                    }
                }
            }
            return null;
        }

        protected TokenTracker GetSupportingTokenTracker(SecurityTokenAuthenticator tokenAuthenticator, out SupportingTokenAuthenticatorSpecification spec)
        {
            spec = null;
            if (this.supportingTokenAuthenticators != null)
            {
                for (int i = 0; i < this.supportingTokenAuthenticators.Count; i++)
                {
                    if (this.supportingTokenAuthenticators[i].TokenAuthenticator == tokenAuthenticator)
                    {
                        spec = this.supportingTokenAuthenticators[i];
                        return this.supportingTokenTrackers[i];
                    }
                }
            }
            return null;
        }

        private bool IsPrimaryToken(SecurityToken token)
        {
            bool flag = (((token == this.outOfBandPrimaryToken) || ((this.primaryTokenTracker != null) && (token == this.primaryTokenTracker.token))) || (token == this.expectedEncryptionToken)) || ((token is WrappedKeySecurityToken) && (((WrappedKeySecurityToken) token).WrappingToken == this.wrappingToken));
            if (!flag && (this.outOfBandPrimaryTokenCollection != null))
            {
                for (int i = 0; i < this.outOfBandPrimaryTokenCollection.Count; i++)
                {
                    if (this.outOfBandPrimaryTokenCollection[i] == token)
                    {
                        return true;
                    }
                }
            }
            return flag;
        }

        protected abstract bool IsReaderAtEncryptedData(XmlDictionaryReader reader);
        protected abstract bool IsReaderAtEncryptedKey(XmlDictionaryReader reader);
        protected abstract bool IsReaderAtReferenceList(XmlDictionaryReader reader);
        protected abstract bool IsReaderAtSignature(XmlDictionaryReader reader);
        private void MarkHeaderAsUnderstood()
        {
            MessageHeaderInfo headerInfo = base.Message.Headers[this.headerIndex];
            base.Message.Headers.UnderstoodHeaders.Add(headerInfo);
        }

        protected abstract void OnDecryptionOfSecurityHeaderItemRequiringReferenceListEntry(string id);
        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            XmlDictionaryReader readerAtSecurityHeader = this.GetReaderAtSecurityHeader();
            readerAtSecurityHeader.ReadStartElement();
            for (int i = 0; i < this.ElementManager.Count; i++)
            {
                ReceiveSecurityHeaderEntry entry;
                this.ElementManager.GetElementEntry(i, out entry);
                XmlDictionaryReader reader = null;
                if (entry.encrypted)
                {
                    reader = this.ElementManager.GetReader(i, false);
                    writer.WriteNode(reader, false);
                    reader.Close();
                    readerAtSecurityHeader.Skip();
                }
                else
                {
                    writer.WriteNode(readerAtSecurityHeader, false);
                }
            }
            readerAtSecurityHeader.Close();
        }

        protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            base.StandardsManager.SecurityVersion.WriteStartHeader(writer);
            System.ServiceModel.Channels.XmlAttributeHolder[] securityElementAttributes = this.securityElementAttributes;
            for (int i = 0; i < securityElementAttributes.Length; i++)
            {
                writer.WriteAttributeString(securityElementAttributes[i].Prefix, securityElementAttributes[i].LocalName, securityElementAttributes[i].NamespaceUri, securityElementAttributes[i].Value);
            }
        }

        public void Process(TimeSpan timeout, ChannelBinding channelBinding, ExtendedProtectionPolicy extendedProtectionPolicy)
        {
            MessageProtectionOrder protectionOrder = this.protectionOrder;
            bool flag = false;
            if ((this.protectionOrder == MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature) && ((this.RequiredEncryptionParts == null) || !this.RequiredEncryptionParts.IsBodyIncluded))
            {
                protectionOrder = MessageProtectionOrder.SignBeforeEncrypt;
                flag = true;
            }
            this.channelBinding = channelBinding;
            this.extendedProtectionPolicy = extendedProtectionPolicy;
            this.orderTracker.SetRequiredProtectionOrder(protectionOrder);
            base.SetProcessingStarted();
            this.timeoutHelper = new TimeoutHelper(timeout);
            base.Message = this.securityVerifiedMessage = new System.ServiceModel.Security.SecurityVerifiedMessage(base.Message, this);
            XmlDictionaryReader reader = this.CreateSecurityHeaderReader();
            reader.MoveToStartElement();
            if (reader.IsEmptyElement)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SecurityHeaderIsEmpty")), base.Message);
            }
            if (base.RequireMessageProtection)
            {
                this.securityElementAttributes = System.ServiceModel.Channels.XmlAttributeHolder.ReadAttributes(reader);
            }
            else
            {
                this.securityElementAttributes = System.ServiceModel.Channels.XmlAttributeHolder.emptyArray;
            }
            reader.ReadStartElement();
            if (this.primaryTokenParameters != null)
            {
                this.primaryTokenTracker = new TokenTracker(null, this.outOfBandPrimaryToken, this.allowFirstTokenMismatch);
            }
            this.universalTokenResolver = new SecurityHeaderTokenResolver(this);
            this.primaryTokenResolver = new SecurityHeaderTokenResolver(this);
            if (this.outOfBandPrimaryToken != null)
            {
                this.universalTokenResolver.Add(this.outOfBandPrimaryToken, SecurityTokenReferenceStyle.External, this.primaryTokenParameters);
                this.primaryTokenResolver.Add(this.outOfBandPrimaryToken, SecurityTokenReferenceStyle.External, this.primaryTokenParameters);
            }
            else if (this.outOfBandPrimaryTokenCollection != null)
            {
                for (int i = 0; i < this.outOfBandPrimaryTokenCollection.Count; i++)
                {
                    this.universalTokenResolver.Add(this.outOfBandPrimaryTokenCollection[i], SecurityTokenReferenceStyle.External, this.primaryTokenParameters);
                    this.primaryTokenResolver.Add(this.outOfBandPrimaryTokenCollection[i], SecurityTokenReferenceStyle.External, this.primaryTokenParameters);
                }
            }
            if (this.wrappingToken != null)
            {
                this.universalTokenResolver.ExpectedWrapper = this.wrappingToken;
                this.universalTokenResolver.ExpectedWrapperTokenParameters = this.wrappingTokenParameters;
                this.primaryTokenResolver.ExpectedWrapper = this.wrappingToken;
                this.primaryTokenResolver.ExpectedWrapperTokenParameters = this.wrappingTokenParameters;
            }
            else if (this.expectedEncryptionToken != null)
            {
                this.universalTokenResolver.Add(this.expectedEncryptionToken, SecurityTokenReferenceStyle.External, this.expectedEncryptionTokenParameters);
                this.primaryTokenResolver.Add(this.expectedEncryptionToken, SecurityTokenReferenceStyle.External, this.expectedEncryptionTokenParameters);
            }
            if (this.outOfBandTokenResolver == null)
            {
                this.combinedUniversalTokenResolver = this.universalTokenResolver;
                this.combinedPrimaryTokenResolver = this.primaryTokenResolver;
            }
            else
            {
                this.combinedUniversalTokenResolver = new AggregateTokenResolver(this.universalTokenResolver, this.outOfBandTokenResolver);
                this.combinedPrimaryTokenResolver = new AggregateTokenResolver(this.primaryTokenResolver, this.outOfBandTokenResolver);
            }
            this.allowedAuthenticators = new List<SecurityTokenAuthenticator>();
            if (this.primaryTokenAuthenticator != null)
            {
                this.allowedAuthenticators.Add(this.primaryTokenAuthenticator);
            }
            if (this.DerivedTokenAuthenticator != null)
            {
                this.allowedAuthenticators.Add(this.DerivedTokenAuthenticator);
            }
            this.pendingSupportingTokenAuthenticator = null;
            int num2 = 0;
            if ((this.supportingTokenAuthenticators != null) && (this.supportingTokenAuthenticators.Count > 0))
            {
                this.supportingTokenTrackers = new List<TokenTracker>(this.supportingTokenAuthenticators.Count);
                for (int j = 0; j < this.supportingTokenAuthenticators.Count; j++)
                {
                    SupportingTokenAuthenticatorSpecification spec = this.supportingTokenAuthenticators[j];
                    switch (spec.SecurityTokenAttachmentMode)
                    {
                        case SecurityTokenAttachmentMode.Signed:
                            this.hasAtLeastOneSupportingTokenExpectedToBeSigned = true;
                            break;

                        case SecurityTokenAttachmentMode.Endorsing:
                            this.hasEndorsingOrSignedEndorsingSupportingTokens = true;
                            break;

                        case SecurityTokenAttachmentMode.SignedEndorsing:
                            this.hasEndorsingOrSignedEndorsingSupportingTokens = true;
                            this.hasAtLeastOneSupportingTokenExpectedToBeSigned = true;
                            break;

                        case SecurityTokenAttachmentMode.SignedEncrypted:
                            this.hasAtLeastOneSupportingTokenExpectedToBeSigned = true;
                            break;
                    }
                    if ((this.primaryTokenAuthenticator != null) && this.primaryTokenAuthenticator.GetType().Equals(spec.TokenAuthenticator.GetType()))
                    {
                        this.pendingSupportingTokenAuthenticator = spec.TokenAuthenticator;
                    }
                    else
                    {
                        this.allowedAuthenticators.Add(spec.TokenAuthenticator);
                    }
                    if ((spec.TokenParameters.RequireDerivedKeys && !spec.TokenParameters.HasAsymmetricKey) && ((spec.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.Endorsing) || (spec.SecurityTokenAttachmentMode == SecurityTokenAttachmentMode.SignedEndorsing)))
                    {
                        num2++;
                    }
                    this.supportingTokenTrackers.Add(new TokenTracker(spec));
                }
            }
            if (this.DerivedTokenAuthenticator != null)
            {
                int num4 = (base.AlgorithmSuite.DefaultEncryptionKeyDerivationLength >= base.AlgorithmSuite.DefaultSignatureKeyDerivationLength) ? base.AlgorithmSuite.DefaultEncryptionKeyDerivationLength : base.AlgorithmSuite.DefaultSignatureKeyDerivationLength;
                this.maxDerivedKeyLength = num4 / 8;
                this.maxDerivedKeys = (2 + num2) * 2;
            }
            SecurityHeaderElementInferenceEngine.GetInferenceEngine(base.Layout).ExecuteProcessingPasses(this, reader);
            if (base.RequireMessageProtection)
            {
                this.ElementManager.EnsureAllRequiredSecurityHeaderTargetsWereProtected();
                this.ExecuteMessageProtectionPass(this.hasAtLeastOneSupportingTokenExpectedToBeSigned);
                if ((this.RequiredSignatureParts != null) && (this.SignatureToken == null))
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("RequiredSignatureMissing")), base.Message);
                }
            }
            this.EnsureDecryptionComplete();
            this.signatureTracker.SetDerivationSourceIfRequired();
            this.encryptionTracker.SetDerivationSourceIfRequired();
            if (this.EncryptionToken != null)
            {
                if (this.wrappingToken != null)
                {
                    if (!(this.EncryptionToken is WrappedKeySecurityToken) || (((WrappedKeySecurityToken) this.EncryptionToken).WrappingToken != this.wrappingToken))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("EncryptedKeyWasNotEncryptedWithTheRequiredEncryptingToken", new object[] { this.wrappingToken })));
                    }
                }
                else if (this.expectedEncryptionToken != null)
                {
                    if (this.EncryptionToken != this.expectedEncryptionToken)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("MessageWasNotEncryptedWithTheRequiredEncryptingToken")));
                    }
                }
                else if ((this.SignatureToken != null) && (this.EncryptionToken != this.SignatureToken))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("SignatureAndEncryptionTokenMismatch", new object[] { this.SignatureToken, this.EncryptionToken })));
                }
            }
            if (this.EnforceDerivedKeyRequirement)
            {
                if (this.SignatureToken != null)
                {
                    if (this.primaryTokenParameters != null)
                    {
                        if ((this.primaryTokenParameters.RequireDerivedKeys && !this.primaryTokenParameters.HasAsymmetricKey) && !this.primaryTokenTracker.IsDerivedFrom)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("PrimarySignatureWasNotSignedByDerivedKey", new object[] { this.primaryTokenParameters })));
                        }
                    }
                    else if (((this.wrappingTokenParameters != null) && this.wrappingTokenParameters.RequireDerivedKeys) && !this.signatureTracker.IsDerivedToken)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("PrimarySignatureWasNotSignedByDerivedWrappedKey", new object[] { this.wrappingTokenParameters })));
                    }
                }
                if (this.EncryptionToken != null)
                {
                    if (this.wrappingTokenParameters != null)
                    {
                        if (this.wrappingTokenParameters.RequireDerivedKeys && !this.encryptionTracker.IsDerivedToken)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("MessageWasNotEncryptedByDerivedWrappedKey", new object[] { this.wrappingTokenParameters })));
                        }
                    }
                    else if (this.expectedEncryptionTokenParameters != null)
                    {
                        if (this.expectedEncryptionTokenParameters.RequireDerivedKeys && !this.encryptionTracker.IsDerivedToken)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("MessageWasNotEncryptedByDerivedEncryptionToken", new object[] { this.expectedEncryptionTokenParameters })));
                        }
                    }
                    else if (((this.primaryTokenParameters != null) && !this.primaryTokenParameters.HasAsymmetricKey) && (this.primaryTokenParameters.RequireDerivedKeys && !this.encryptionTracker.IsDerivedToken))
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("MessageWasNotEncryptedByDerivedEncryptionToken", new object[] { this.primaryTokenParameters })));
                    }
                }
            }
            if ((flag && (this.BasicSupportingTokens != null)) && (this.BasicSupportingTokens.Count > 0))
            {
                this.VerifySignatureEncryption();
            }
            if (this.supportingTokenTrackers != null)
            {
                for (int k = 0; k < this.supportingTokenTrackers.Count; k++)
                {
                    this.VerifySupportingToken(this.supportingTokenTrackers[k]);
                }
            }
            if (this.nonceCache != null)
            {
                if (this.timestamp == null)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("NoTimestampAvailableInSecurityHeaderToDoReplayDetection")), base.Message);
                }
                if (this.primarySignatureValue == null)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("NoSignatureAvailableInSecurityHeaderToDoReplayDetection")), base.Message);
                }
                AddNonce(this.nonceCache, this.primarySignatureValue);
                this.timestamp.ValidateFreshness(this.replayWindow, this.clockSkew);
            }
            if (this.ExpectSignatureConfirmation)
            {
                this.ElementManager.VerifySignatureConfirmationWasFound();
            }
            this.MarkHeaderAsUnderstood();
        }

        private void ProcessEncryptedData(EncryptedData encryptedData, TimeSpan timeout, int position, bool eagerMode, ref bool primarySignatureFound)
        {
            SecurityToken token;
            string id = encryptedData.Id;
            byte[] decryptedBuffer = this.DecryptSecurityHeaderElement(encryptedData, this.wrappedKeyToken, out token);
            XmlDictionaryReader reader = this.CreateDecryptedReader(decryptedBuffer);
            if (this.IsReaderAtSignature(reader))
            {
                this.RecordEncryptionTokenAndRemoveReferenceListEntry(id, token);
                SignedXml signedXml = this.ReadSignature(reader, position, decryptedBuffer);
                if (eagerMode)
                {
                    if (primarySignatureFound)
                    {
                        this.elementManager.SetBindingMode(position, ReceiveSecurityHeaderBindingModes.Endorsing);
                        this.ProcessSupportingSignature(signedXml, true);
                    }
                    else
                    {
                        primarySignatureFound = true;
                        this.elementManager.SetBindingMode(position, ReceiveSecurityHeaderBindingModes.Primary);
                        this.ProcessPrimarySignature(signedXml, true);
                    }
                }
            }
            else if (base.StandardsManager.SecurityVersion.IsReaderAtSignatureConfirmation(reader))
            {
                this.RecordEncryptionTokenAndRemoveReferenceListEntry(id, token);
                this.ReadSignatureConfirmation(reader, position, decryptedBuffer);
            }
            else if (this.IsReaderAtEncryptedData(reader))
            {
                SecurityToken token2;
                ReceiveSecurityHeaderEntry entry;
                EncryptedData data = this.ReadSecurityHeaderEncryptedItem(reader);
                byte[] buffer2 = this.DecryptSecurityHeaderElement(data, this.wrappedKeyToken, out token2);
                XmlDictionaryReader reader2 = this.CreateDecryptedReader(buffer2);
                this.ReadToken(reader2, position, buffer2, token, id, timeout);
                this.ElementManager.GetElementEntry(position, out entry);
                if (this.EncryptBeforeSignMode)
                {
                    entry.encryptedFormId = encryptedData.Id;
                    entry.encryptedFormWsuId = encryptedData.WsuId;
                }
                else
                {
                    entry.encryptedFormId = data.Id;
                    entry.encryptedFormWsuId = data.WsuId;
                }
                entry.decryptedBuffer = decryptedBuffer;
                entry.doubleEncrypted = true;
                this.ElementManager.ReplaceHeaderEntry(position, entry);
            }
            else
            {
                this.ReadToken(reader, position, decryptedBuffer, token, id, timeout);
            }
        }

        private void ProcessPrimarySignature(SignedXml signedXml, bool isFromDecryptedSource)
        {
            this.orderTracker.OnProcessSignature(isFromDecryptedSource);
            this.primarySignatureValue = signedXml.GetSignatureValue();
            if (this.nonceCache != null)
            {
                CheckNonce(this.nonceCache, this.primarySignatureValue);
            }
            SecurityToken token = this.VerifySignature(signedXml, true, this.primaryTokenResolver, null, null);
            SecurityToken rootToken = this.GetRootToken(token);
            bool flag = token is DerivedKeySecurityToken;
            if (this.primaryTokenTracker != null)
            {
                this.primaryTokenTracker.RecordToken(rootToken);
                this.primaryTokenTracker.IsDerivedFrom = flag;
            }
            this.AddIncomingSignatureValue(signedXml.GetSignatureValue(), isFromDecryptedSource);
        }

        private void ProcessReferenceList(ReferenceList referenceList)
        {
            this.ProcessReferenceList(referenceList, null);
        }

        private void ProcessReferenceList(ReferenceList referenceList, WrappedKeySecurityToken wrappedKeyToken)
        {
            this.orderTracker.OnProcessReferenceList();
            this.ProcessReferenceListCore(referenceList, wrappedKeyToken);
        }

        protected abstract void ProcessReferenceListCore(ReferenceList referenceList, WrappedKeySecurityToken wrappedKeyToken);
        private void ProcessSupportingSignature(SignedXml signedXml, bool isFromDecryptedSource)
        {
            string id;
            XmlDictionaryReader reader;
            object obj2;
            if (!this.ExpectEndorsingTokens)
            {
                throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SupportingTokenSignaturesNotExpected")), base.Message);
            }
            if (!base.RequireMessageProtection)
            {
                if (this.timestamp == null)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SigningWithoutPrimarySignatureRequiresTimestamp")), base.Message);
                }
                reader = null;
                id = this.timestamp.Id;
                obj2 = null;
            }
            else
            {
                this.elementManager.GetPrimarySignature(out reader, out id);
                if (reader == null)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("NoPrimarySignatureAvailableForSupportingTokenSignatureVerification")), base.Message);
                }
                obj2 = reader;
            }
            SecurityToken token = this.VerifySignature(signedXml, false, this.universalTokenResolver, obj2, id);
            if (reader != null)
            {
                reader.Close();
            }
            if (token == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SignatureVerificationFailed")), base.Message);
            }
            SecurityToken rootToken = this.GetRootToken(token);
            TokenTracker supportingTokenTracker = this.GetSupportingTokenTracker(rootToken);
            if (supportingTokenTracker == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("UnknownSupportingToken", new object[] { token })));
            }
            if (supportingTokenTracker.AlreadyReadEndorsingSignature)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MoreThanOneSupportingSignature", new object[] { token })));
            }
            supportingTokenTracker.IsEndorsing = true;
            supportingTokenTracker.AlreadyReadEndorsingSignature = true;
            supportingTokenTracker.IsDerivedFrom = token is DerivedKeySecurityToken;
            this.AddIncomingSignatureValue(signedXml.GetSignatureValue(), isFromDecryptedSource);
        }

        private EncryptedData ReadEncryptedData(XmlDictionaryReader reader)
        {
            EncryptedData encryptedData = this.ReadSecurityHeaderEncryptedItem(reader);
            this.elementManager.AppendEncryptedData(encryptedData);
            return encryptedData;
        }

        private void ReadEncryptedKey(XmlDictionaryReader reader, bool processReferenceListIfPresent)
        {
            this.orderTracker.OnEncryptedKey();
            WrappedKeySecurityToken token = this.DecryptWrappedKey(reader);
            if (token.WrappingToken != this.wrappingToken)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("EncryptedKeyWasNotEncryptedWithTheRequiredEncryptingToken", new object[] { this.wrappingToken })));
            }
            this.universalTokenResolver.Add(token);
            this.primaryTokenResolver.Add(token);
            if (token.ReferenceList != null)
            {
                if (!base.EncryptedKeyContainsReferenceList)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("EncryptedKeyWithReferenceListNotAllowed")));
                }
                if (!this.ExpectEncryption)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("EncryptionNotExpected")), base.Message);
                }
                if (processReferenceListIfPresent)
                {
                    this.ProcessReferenceList(token.ReferenceList, token);
                }
                this.wrappedKeyToken = token;
            }
            this.elementManager.AppendToken(token, ReceiveSecurityHeaderBindingModes.Primary, null);
        }

        private ReferenceList ReadReferenceList(XmlDictionaryReader reader)
        {
            if (!this.ExpectEncryption)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("EncryptionNotExpected")), base.Message);
            }
            ReferenceList referenceList = this.ReadReferenceListCore(reader);
            this.elementManager.AppendReferenceList(referenceList);
            return referenceList;
        }

        protected abstract ReferenceList ReadReferenceListCore(XmlDictionaryReader reader);
        protected abstract EncryptedData ReadSecurityHeaderEncryptedItem(XmlDictionaryReader reader);
        private SignedXml ReadSignature(XmlDictionaryReader reader, int position, byte[] decryptedBuffer)
        {
            int num;
            if (!this.ExpectSignature)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SignatureNotExpected")), base.Message);
            }
            SignedXml signedXml = this.ReadSignatureCore(reader);
            signedXml.Signature.SignedInfo.ReaderProvider = this.ElementManager;
            if (decryptedBuffer == null)
            {
                this.elementManager.AppendSignature(signedXml);
                num = this.elementManager.Count - 1;
            }
            else
            {
                this.elementManager.SetSignatureAfterDecryption(position, signedXml, decryptedBuffer);
                num = position;
            }
            signedXml.Signature.SignedInfo.SignatureReaderProviderCallbackContext = num;
            return signedXml;
        }

        private void ReadSignatureConfirmation(XmlDictionaryReader reader, int position, byte[] decryptedBuffer)
        {
            if (!this.ExpectSignatureConfirmation)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SignatureConfirmationsNotExpected")), base.Message);
            }
            if (this.orderTracker.PrimarySignatureDone)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SignatureConfirmationsOccursAfterPrimarySignature")), base.Message);
            }
            ISignatureValueSecurityElement signatureConfirmationElement = base.StandardsManager.SecurityVersion.ReadSignatureConfirmation(reader);
            if (decryptedBuffer == null)
            {
                this.AddIncomingSignatureConfirmation(signatureConfirmationElement.GetSignatureValue(), false);
                this.elementManager.AppendSignatureConfirmation(signatureConfirmationElement);
            }
            else
            {
                this.AddIncomingSignatureConfirmation(signatureConfirmationElement.GetSignatureValue(), true);
                this.elementManager.SetSignatureConfirmationAfterDecryption(position, signatureConfirmationElement, decryptedBuffer);
            }
        }

        protected abstract SignedXml ReadSignatureCore(XmlDictionaryReader signatureReader);
        private void ReadTimestamp(XmlDictionaryReader reader)
        {
            if (this.timestamp != null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("DuplicateTimestampInSecurityHeader")), base.Message);
            }
            bool flag = base.RequireMessageProtection || this.hasEndorsingOrSignedEndorsingSupportingTokens;
            string digestAlgorithm = flag ? base.AlgorithmSuite.DefaultDigestAlgorithm : null;
            SignatureResourcePool resourcePool = flag ? this.ResourcePool : null;
            this.timestamp = base.StandardsManager.WSUtilitySpecificationVersion.ReadTimestamp(reader, digestAlgorithm, resourcePool);
            this.timestamp.ValidateRangeAndFreshness(this.replayWindow, this.clockSkew);
            this.elementManager.AppendTimestamp(this.timestamp);
        }

        private SecurityToken ReadToken(XmlReader reader, SecurityTokenResolver tokenResolver, IList<SecurityTokenAuthenticator> allowedTokenAuthenticators, out SecurityTokenAuthenticator usedTokenAuthenticator)
        {
            SecurityToken token = base.StandardsManager.SecurityTokenSerializer.ReadToken(reader, tokenResolver);
            if (token is DerivedKeySecurityTokenStub)
            {
                if (this.DerivedTokenAuthenticator == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnableToFindTokenAuthenticator", new object[] { typeof(DerivedKeySecurityToken) })));
                }
                usedTokenAuthenticator = this.DerivedTokenAuthenticator;
                return token;
            }
            for (int i = 0; i < allowedTokenAuthenticators.Count; i++)
            {
                SecurityTokenAuthenticator authenticator = allowedTokenAuthenticators[i];
                if (authenticator.CanValidateToken(token))
                {
                    ReadOnlyCollection<IAuthorizationPolicy> onlys;
                    ServiceCredentialsSecurityTokenManager.KerberosSecurityTokenAuthenticatorWrapper wrapper = authenticator as ServiceCredentialsSecurityTokenManager.KerberosSecurityTokenAuthenticatorWrapper;
                    if (wrapper != null)
                    {
                        onlys = wrapper.ValidateToken(token, this.channelBinding, this.extendedProtectionPolicy);
                    }
                    else
                    {
                        onlys = authenticator.ValidateToken(token);
                    }
                    this.SecurityTokenAuthorizationPoliciesMapping.Add(token, onlys);
                    usedTokenAuthenticator = authenticator;
                    return token;
                }
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnableToFindTokenAuthenticator", new object[] { token.GetType() })));
        }

        private void ReadToken(XmlDictionaryReader reader, int position, byte[] decryptedBuffer, SecurityToken encryptionToken, string idInEncryptedForm, TimeSpan timeout)
        {
            SecurityTokenAuthenticator authenticator;
            ReceiveSecurityHeaderBindingModes primary;
            string localName = reader.LocalName;
            string namespaceURI = reader.NamespaceURI;
            string attribute = reader.GetAttribute(System.ServiceModel.XD.SecurityJan2004Dictionary.ValueType, null);
            SecurityToken token = this.ReadToken(reader, this.CombinedUniversalTokenResolver, this.allowedAuthenticators, out authenticator);
            if (token == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenManagerCouldNotReadToken", new object[] { localName, namespaceURI, attribute })), base.Message);
            }
            DerivedKeySecurityToken token2 = token as DerivedKeySecurityToken;
            if (token2 != null)
            {
                this.EnsureDerivedKeyLimitNotReached();
                token2.InitializeDerivedKey(this.maxDerivedKeyLength);
            }
            if ((authenticator is SspiNegotiationTokenAuthenticator) || (authenticator == this.primaryTokenAuthenticator))
            {
                this.allowedAuthenticators.Remove(authenticator);
            }
            TokenTracker item = null;
            if (authenticator == this.primaryTokenAuthenticator)
            {
                this.universalTokenResolver.Add(token, SecurityTokenReferenceStyle.Internal, this.primaryTokenParameters);
                this.primaryTokenResolver.Add(token, SecurityTokenReferenceStyle.Internal, this.primaryTokenParameters);
                if (this.pendingSupportingTokenAuthenticator != null)
                {
                    this.allowedAuthenticators.Add(this.pendingSupportingTokenAuthenticator);
                    this.pendingSupportingTokenAuthenticator = null;
                }
                this.primaryTokenTracker.RecordToken(token);
                primary = ReceiveSecurityHeaderBindingModes.Primary;
            }
            else if (authenticator == this.DerivedTokenAuthenticator)
            {
                if (token is DerivedKeySecurityTokenStub)
                {
                    if (base.Layout == SecurityHeaderLayout.Strict)
                    {
                        DerivedKeySecurityTokenStub stub = (DerivedKeySecurityTokenStub) token;
                        throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnableToResolveKeyInfoClauseInDerivedKeyToken", new object[] { stub.TokenToDeriveIdentifier })), base.Message);
                    }
                }
                else
                {
                    this.AddDerivedKeyTokenToResolvers(token);
                }
                primary = ReceiveSecurityHeaderBindingModes.Unknown;
            }
            else
            {
                SupportingTokenAuthenticatorSpecification specification;
                bool flag;
                bool flag2;
                item = this.GetSupportingTokenTracker(authenticator, out specification);
                if (item == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("UnknownTokenAuthenticatorUsedInTokenProcessing", new object[] { authenticator })));
                }
                if (item.token != null)
                {
                    item = new TokenTracker(specification);
                    this.supportingTokenTrackers.Add(item);
                }
                item.RecordToken(token);
                if (encryptionToken != null)
                {
                    item.IsEncrypted = true;
                }
                SecurityTokenAttachmentModeHelper.Categorize(specification.SecurityTokenAttachmentMode, out flag, out flag2, out primary);
                if (flag)
                {
                    if (!this.ExpectBasicTokens)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("BasicTokenNotExpected")));
                    }
                    if (base.RequireMessageProtection && (encryptionToken != null))
                    {
                        this.RecordEncryptionTokenAndRemoveReferenceListEntry(idInEncryptedForm, encryptionToken);
                    }
                }
                if (flag2 && !this.ExpectSignedTokens)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("SignedSupportingTokenNotExpected")));
                }
                this.universalTokenResolver.Add(token, SecurityTokenReferenceStyle.Internal, specification.TokenParameters);
            }
            if (position == -1)
            {
                this.elementManager.AppendToken(token, primary, item);
            }
            else
            {
                this.elementManager.SetTokenAfterDecryption(position, token, primary, decryptedBuffer, item);
            }
        }

        protected void RecordEncryptionToken(SecurityToken token)
        {
            this.encryptionTracker.RecordToken(token);
        }

        private void RecordEncryptionTokenAndRemoveReferenceListEntry(string id, SecurityToken encryptionToken)
        {
            if (id == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MissingIdInEncryptedElement")), base.Message);
            }
            this.OnDecryptionOfSecurityHeaderItemRequiringReferenceListEntry(id);
            this.RecordEncryptionToken(encryptionToken);
        }

        protected void RecordSignatureToken(SecurityToken token)
        {
            this.signatureTracker.RecordToken(token);
        }

        public void SetRequiredProtectionOrder(MessageProtectionOrder protectionOrder)
        {
            base.ThrowIfProcessingStarted();
            this.protectionOrder = protectionOrder;
        }

        public void SetTimeParameters(NonceCache nonceCache, TimeSpan replayWindow, TimeSpan clockSkew)
        {
            this.nonceCache = nonceCache;
            this.replayWindow = replayWindow;
            this.clockSkew = clockSkew;
        }

        protected abstract bool TryDeleteReferenceListEntry(string id);
        protected abstract SecurityToken VerifySignature(SignedXml signedXml, bool isPrimarySignature, SecurityHeaderTokenResolver resolver, object signatureTarget, string id);
        protected void VerifySignatureEncryption()
        {
            if ((this.protectionOrder == MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature) && !this.orderTracker.AllSignaturesEncrypted)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("PrimarySignatureIsRequiredToBeEncrypted")));
            }
        }

        private void VerifySupportingToken(TokenTracker tracker)
        {
            if (tracker == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tracker");
            }
            SupportingTokenAuthenticatorSpecification spec = tracker.spec;
            if (tracker.token == null)
            {
                if (!spec.IsTokenOptional)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("SupportingTokenNotProvided", new object[] { spec.TokenParameters, spec.SecurityTokenAttachmentMode })));
                }
            }
            else
            {
                switch (spec.SecurityTokenAttachmentMode)
                {
                    case SecurityTokenAttachmentMode.Signed:
                        if (!tracker.IsSigned && base.RequireMessageProtection)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("SupportingTokenIsNotSigned", new object[] { spec.TokenParameters })));
                        }
                        this.EnsureSupportingTokens(ref this.signedTokens).Add(tracker.token);
                        return;

                    case SecurityTokenAttachmentMode.Endorsing:
                        if (!tracker.IsEndorsing)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("SupportingTokenIsNotEndorsing", new object[] { spec.TokenParameters })));
                        }
                        if ((this.EnforceDerivedKeyRequirement && spec.TokenParameters.RequireDerivedKeys) && (!spec.TokenParameters.HasAsymmetricKey && !tracker.IsDerivedFrom))
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("SupportingSignatureIsNotDerivedFrom", new object[] { spec.TokenParameters })));
                        }
                        this.EnsureSupportingTokens(ref this.endorsingTokens).Add(tracker.token);
                        return;

                    case SecurityTokenAttachmentMode.SignedEndorsing:
                        if (!tracker.IsSigned && base.RequireMessageProtection)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("SupportingTokenIsNotSigned", new object[] { spec.TokenParameters })));
                        }
                        if (!tracker.IsEndorsing)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("SupportingTokenIsNotEndorsing", new object[] { spec.TokenParameters })));
                        }
                        if ((this.EnforceDerivedKeyRequirement && spec.TokenParameters.RequireDerivedKeys) && (!spec.TokenParameters.HasAsymmetricKey && !tracker.IsDerivedFrom))
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("SupportingSignatureIsNotDerivedFrom", new object[] { spec.TokenParameters })));
                        }
                        this.EnsureSupportingTokens(ref this.signedEndorsingTokens).Add(tracker.token);
                        return;

                    case SecurityTokenAttachmentMode.SignedEncrypted:
                        if (!tracker.IsSigned && base.RequireMessageProtection)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("SupportingTokenIsNotSigned", new object[] { spec.TokenParameters })));
                        }
                        if (!tracker.IsEncrypted && base.RequireMessageProtection)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("SupportingTokenIsNotEncrypted", new object[] { spec.TokenParameters })));
                        }
                        this.EnsureSupportingTokens(ref this.basicTokens).Add(tracker.token);
                        return;
                }
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UnknownTokenAttachmentMode", new object[] { spec.SecurityTokenAttachmentMode })));
            }
        }

        public Collection<SecurityToken> BasicSupportingTokens
        {
            get
            {
                return this.basicTokens;
            }
        }

        public SecurityTokenResolver CombinedPrimaryTokenResolver
        {
            get
            {
                return this.combinedPrimaryTokenResolver;
            }
        }

        public SecurityTokenResolver CombinedUniversalTokenResolver
        {
            get
            {
                return this.combinedUniversalTokenResolver;
            }
        }

        public SecurityTokenAuthenticator DerivedTokenAuthenticator
        {
            get
            {
                return this.derivedTokenAuthenticator;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                this.derivedTokenAuthenticator = value;
            }
        }

        public ReceiveSecurityHeaderElementManager ElementManager
        {
            get
            {
                return this.elementManager;
            }
        }

        public bool EncryptBeforeSignMode
        {
            get
            {
                return this.orderTracker.EncryptBeforeSignMode;
            }
        }

        public SecurityToken EncryptionToken
        {
            get
            {
                return this.encryptionTracker.Token;
            }
        }

        public Collection<SecurityToken> EndorsingSupportingTokens
        {
            get
            {
                return this.endorsingTokens;
            }
        }

        public bool EnforceDerivedKeyRequirement
        {
            get
            {
                return this.enforceDerivedKeyRequirement;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                this.enforceDerivedKeyRequirement = value;
            }
        }

        public bool ExpectBasicTokens
        {
            get
            {
                return this.expectBasicTokens;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                this.expectBasicTokens = value;
            }
        }

        public bool ExpectEncryption
        {
            get
            {
                return this.expectEncryption;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                this.expectEncryption = value;
            }
        }

        public bool ExpectEndorsingTokens
        {
            get
            {
                return this.expectEndorsingTokens;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                this.expectEndorsingTokens = value;
            }
        }

        public bool ExpectSignature
        {
            get
            {
                return this.expectSignature;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                this.expectSignature = value;
            }
        }

        public bool ExpectSignatureConfirmation
        {
            get
            {
                return this.expectSignatureConfirmation;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                this.expectSignatureConfirmation = value;
            }
        }

        public bool ExpectSignedTokens
        {
            get
            {
                return this.expectSignedTokens;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                this.expectSignedTokens = value;
            }
        }

        public bool HasAtLeastOneItemInsideSecurityHeaderEncrypted
        {
            get
            {
                return this.hasAtLeastOneItemInsideSecurityHeaderEncrypted;
            }
            set
            {
                this.hasAtLeastOneItemInsideSecurityHeaderEncrypted = value;
            }
        }

        internal int HeaderIndex
        {
            get
            {
                return this.headerIndex;
            }
        }

        public int MaxDerivedKeyLength
        {
            get
            {
                return this.maxDerivedKeyLength;
            }
        }

        internal long MaxReceivedMessageSize
        {
            get
            {
                return this.maxReceivedMessageSize;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                this.maxReceivedMessageSize = value;
            }
        }

        public override string Name
        {
            get
            {
                return base.StandardsManager.SecurityVersion.HeaderName.Value;
            }
        }

        public override string Namespace
        {
            get
            {
                return base.StandardsManager.SecurityVersion.HeaderNamespace.Value;
            }
        }

        public byte[] PrimarySignatureValue
        {
            get
            {
                return this.primarySignatureValue;
            }
        }

        public SecurityHeaderTokenResolver PrimaryTokenResolver
        {
            get
            {
                return this.primaryTokenResolver;
            }
        }

        public Message ProcessedMessage
        {
            get
            {
                return base.Message;
            }
        }

        internal XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.readerQuotas;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                if (value == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.readerQuotas = value;
            }
        }

        public MessagePartSpecification RequiredEncryptionParts
        {
            get
            {
                return this.encryptionTracker.Parts;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                if (value == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("value"), base.Message);
                }
                if (!value.IsReadOnly)
                {
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessagePartSpecificationMustBeImmutable")), base.Message);
                }
                this.encryptionTracker.Parts = value;
            }
        }

        public MessagePartSpecification RequiredSignatureParts
        {
            get
            {
                return this.signatureTracker.Parts;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                if (value == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("value"), base.Message);
                }
                if (!value.IsReadOnly)
                {
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessagePartSpecificationMustBeImmutable")), base.Message);
                }
                this.signatureTracker.Parts = value;
            }
        }

        protected SignatureResourcePool ResourcePool
        {
            get
            {
                if (this.resourcePool == null)
                {
                    this.resourcePool = new SignatureResourcePool();
                }
                return this.resourcePool;
            }
        }

        public Dictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>> SecurityTokenAuthorizationPoliciesMapping
        {
            get
            {
                if (this.tokenPoliciesMapping == null)
                {
                    this.tokenPoliciesMapping = new Dictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>>();
                }
                return this.tokenPoliciesMapping;
            }
        }

        internal System.ServiceModel.Security.SecurityVerifiedMessage SecurityVerifiedMessage
        {
            get
            {
                return this.securityVerifiedMessage;
            }
        }

        public SecurityToken SignatureToken
        {
            get
            {
                return this.signatureTracker.Token;
            }
        }

        public Collection<SecurityToken> SignedEndorsingSupportingTokens
        {
            get
            {
                return this.signedEndorsingTokens;
            }
        }

        public Collection<SecurityToken> SignedSupportingTokens
        {
            get
            {
                return this.signedTokens;
            }
        }

        public SecurityTimestamp Timestamp
        {
            get
            {
                return this.timestamp;
            }
        }

        public List<SecurityTokenAuthenticator> WrappedKeySecurityTokenAuthenticator
        {
            get
            {
                return this.wrappedKeyAuthenticator;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                this.wrappedKeyAuthenticator = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OperationTracker
        {
            private MessagePartSpecification parts;
            private SecurityToken token;
            private bool isDerivedToken;
            public MessagePartSpecification Parts
            {
                get
                {
                    return this.parts;
                }
                set
                {
                    this.parts = value;
                }
            }
            public SecurityToken Token
            {
                get
                {
                    return this.token;
                }
            }
            public bool IsDerivedToken
            {
                get
                {
                    return this.isDerivedToken;
                }
            }
            public void RecordToken(SecurityToken token)
            {
                if (this.token == null)
                {
                    this.token = token;
                }
                else if (!object.ReferenceEquals(this.token, token))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MismatchInSecurityOperationToken")));
                }
            }

            public void SetDerivationSourceIfRequired()
            {
                DerivedKeySecurityToken token = this.token as DerivedKeySecurityToken;
                if (token != null)
                {
                    this.token = token.TokenToDerive;
                    this.isDerivedToken = true;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OrderTracker
        {
            private const int MaxAllowedWrappedKeys = 1;
            private static readonly ReceiverProcessingOrder[] stateTransitionTableOnDecrypt;
            private static readonly ReceiverProcessingOrder[] stateTransitionTableOnVerify;
            private int referenceListCount;
            private ReceiverProcessingOrder state;
            private int signatureCount;
            private int unencryptedSignatureCount;
            private int numWrappedKeys;
            private MessageProtectionOrder protectionOrder;
            private bool enforce;
            public bool AllSignaturesEncrypted
            {
                get
                {
                    return (this.unencryptedSignatureCount == 0);
                }
            }
            public bool EncryptBeforeSignMode
            {
                get
                {
                    return (this.enforce && (this.protectionOrder == MessageProtectionOrder.EncryptBeforeSign));
                }
            }
            public bool EncryptBeforeSignOrderRequirementMet
            {
                get
                {
                    return ((this.state != ReceiverProcessingOrder.DecryptVerify) && (this.state != ReceiverProcessingOrder.Mixed));
                }
            }
            public bool PrimarySignatureDone
            {
                get
                {
                    return (this.signatureCount > 0);
                }
            }
            public bool SignBeforeEncryptOrderRequirementMet
            {
                get
                {
                    return ((this.state != ReceiverProcessingOrder.VerifyDecrypt) && (this.state != ReceiverProcessingOrder.Mixed));
                }
            }
            private void EnforceProtectionOrder()
            {
                switch (this.protectionOrder)
                {
                    case MessageProtectionOrder.SignBeforeEncrypt:
                        break;

                    case MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature:
                        if (!this.AllSignaturesEncrypted)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("PrimarySignatureIsRequiredToBeEncrypted")));
                        }
                        break;

                    case MessageProtectionOrder.EncryptBeforeSign:
                        if (!this.EncryptBeforeSignOrderRequirementMet)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MessageProtectionOrderMismatch", new object[] { this.protectionOrder })));
                        }
                        return;

                    default:
                        return;
                }
                if (!this.SignBeforeEncryptOrderRequirementMet)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MessageProtectionOrderMismatch", new object[] { this.protectionOrder })));
                }
            }

            public void OnProcessReferenceList()
            {
                if (this.referenceListCount > 0)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("AtMostOneReferenceListIsSupportedWithDefaultPolicyCheck")));
                }
                this.referenceListCount++;
                this.state = stateTransitionTableOnDecrypt[(int) this.state];
                if (this.enforce)
                {
                    this.EnforceProtectionOrder();
                }
            }

            public void OnProcessSignature(bool isEncrypted)
            {
                if (this.signatureCount > 0)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("AtMostOneSignatureIsSupportedWithDefaultPolicyCheck")));
                }
                this.signatureCount++;
                if (!isEncrypted)
                {
                    this.unencryptedSignatureCount++;
                }
                this.state = stateTransitionTableOnVerify[(int) this.state];
                if (this.enforce)
                {
                    this.EnforceProtectionOrder();
                }
            }

            public void OnEncryptedKey()
            {
                if (this.numWrappedKeys == 1)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("WrappedKeyLimitExceeded", new object[] { this.numWrappedKeys })));
                }
                this.numWrappedKeys++;
            }

            public void SetRequiredProtectionOrder(MessageProtectionOrder protectionOrder)
            {
                this.protectionOrder = protectionOrder;
                this.enforce = true;
            }

            static OrderTracker()
            {
                stateTransitionTableOnDecrypt = new ReceiverProcessingOrder[] { ReceiverProcessingOrder.Decrypt, ReceiverProcessingOrder.VerifyDecrypt, ReceiverProcessingOrder.Decrypt, ReceiverProcessingOrder.Mixed, ReceiverProcessingOrder.VerifyDecrypt, ReceiverProcessingOrder.Mixed };
                stateTransitionTableOnVerify = new ReceiverProcessingOrder[] { ReceiverProcessingOrder.Verify, ReceiverProcessingOrder.Verify, ReceiverProcessingOrder.DecryptVerify, ReceiverProcessingOrder.DecryptVerify, ReceiverProcessingOrder.Mixed, ReceiverProcessingOrder.Mixed };
            }
            private enum ReceiverProcessingOrder
            {
                None,
                Verify,
                Decrypt,
                DecryptVerify,
                VerifyDecrypt,
                Mixed
            }
        }
    }
}

