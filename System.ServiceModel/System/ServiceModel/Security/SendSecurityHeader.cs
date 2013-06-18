namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal abstract class SendSecurityHeader : SecurityHeader, IMessageHeaderWithSharedNamespace
    {
        private List<SecurityTokenParameters> basicSupportingTokenParameters;
        private bool basicTokenEncrypted;
        private SecurityProtocolCorrelationState correlationState;
        private SendSecurityHeaderElementContainer elementContainer;
        private SecurityToken encryptingToken;
        private SecurityTokenParameters encryptingTokenParameters;
        private MessagePartSpecification encryptionParts;
        private bool encryptSignature;
        private List<SecurityTokenParameters> endorsingTokenParameters;
        private bool hasEncryptedTokens;
        private bool hasSignedTokens;
        private int idCounter;
        private string idPrefix;
        private static readonly string[] ids = new string[] { "_0", "_1", "_2", "_3", "_4", "_5", "_6", "_7", "_8", "_9" };
        private bool primarySignatureDone;
        private byte[] primarySignatureValue;
        private bool shouldSignToHeader;
        private SignatureConfirmations signatureConfirmationsToSend;
        private MessagePartSpecification signatureParts;
        private SignatureConfirmations signatureValuesGenerated;
        private List<SecurityTokenParameters> signedEndorsingTokenParameters;
        private List<SecurityTokenParameters> signedTokenParameters;
        private SecurityTokenParameters signingTokenParameters;
        private bool signThenEncrypt;
        private bool skipKeyInfoForEncryption;

        protected SendSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay, SecurityStandardsManager standardsManager, SecurityAlgorithmSuite algorithmSuite, MessageDirection transferDirection) : base(message, actor, mustUnderstand, relay, standardsManager, algorithmSuite, transferDirection)
        {
            this.signThenEncrypt = true;
            this.elementContainer = new SendSecurityHeaderElementContainer();
        }

        public void AddBasicSupportingToken(SecurityToken token, SecurityTokenParameters parameters)
        {
            if (token == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            if (parameters == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            base.ThrowIfProcessingStarted();
            SendSecurityHeaderElement tokenElement = new SendSecurityHeaderElement(token.Id, new TokenElement(token, base.StandardsManager)) {
                MarkedForEncryption = true
            };
            this.elementContainer.AddBasicSupportingToken(tokenElement);
            this.hasEncryptedTokens = true;
            this.hasSignedTokens = true;
            this.AddParameters(ref this.basicSupportingTokenParameters, parameters);
        }

        public void AddEndorsingSupportingToken(SecurityToken token, SecurityTokenParameters parameters)
        {
            if (token == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            if (parameters == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            base.ThrowIfProcessingStarted();
            this.elementContainer.AddEndorsingSupportingToken(token);
            if (!(token is ProviderBackedSecurityToken))
            {
                this.shouldSignToHeader |= !base.RequireMessageProtection && (System.ServiceModel.Security.SecurityUtils.GetSecurityKey<AsymmetricSecurityKey>(token) != null);
            }
            this.AddParameters(ref this.endorsingTokenParameters, parameters);
        }

        private void AddGeneratedSignatureValue(byte[] signatureValue, bool wasEncrypted)
        {
            if (base.MaintainSignatureConfirmationState && (this.signatureConfirmationsToSend == null))
            {
                if (this.signatureValuesGenerated == null)
                {
                    this.signatureValuesGenerated = new SignatureConfirmations();
                }
                this.signatureValuesGenerated.AddConfirmation(signatureValue, wasEncrypted);
            }
        }

        private void AddParameters(ref List<SecurityTokenParameters> list, SecurityTokenParameters item)
        {
            if (list == null)
            {
                list = new List<SecurityTokenParameters>();
            }
            list.Add(item);
        }

        public void AddPrerequisiteToken(SecurityToken token)
        {
            base.ThrowIfProcessingStarted();
            if (token == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("token", base.Message);
            }
            this.elementContainer.PrerequisiteToken = token;
        }

        public void AddSignatureConfirmations(SignatureConfirmations confirmations)
        {
            base.ThrowIfProcessingStarted();
            this.signatureConfirmationsToSend = confirmations;
        }

        public void AddSignedEndorsingSupportingToken(SecurityToken token, SecurityTokenParameters parameters)
        {
            if (token == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            if (parameters == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            base.ThrowIfProcessingStarted();
            this.elementContainer.AddSignedEndorsingSupportingToken(token);
            this.hasSignedTokens = true;
            this.shouldSignToHeader |= !base.RequireMessageProtection && (System.ServiceModel.Security.SecurityUtils.GetSecurityKey<AsymmetricSecurityKey>(token) != null);
            this.AddParameters(ref this.signedEndorsingTokenParameters, parameters);
        }

        public void AddSignedSupportingToken(SecurityToken token, SecurityTokenParameters parameters)
        {
            if (token == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            if (parameters == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            base.ThrowIfProcessingStarted();
            this.elementContainer.AddSignedSupportingToken(token);
            this.hasSignedTokens = true;
            this.AddParameters(ref this.signedTokenParameters, parameters);
        }

        public void AddTimestamp(SecurityTimestamp timestamp)
        {
            base.ThrowIfProcessingStarted();
            if (this.elementContainer.Timestamp != null)
            {
                throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TimestampAlreadySetForSecurityHeader")), base.Message);
            }
            if (timestamp == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("timestamp", base.Message);
            }
            this.elementContainer.Timestamp = timestamp;
        }

        public void AddTimestamp(TimeSpan timestampValidityDuration)
        {
            DateTime utcNow = DateTime.UtcNow;
            string id = base.RequireMessageProtection ? System.ServiceModel.Security.SecurityUtils.GenerateId() : this.GenerateId();
            this.AddTimestamp(new SecurityTimestamp(utcNow, utcNow + timestampValidityDuration, id));
        }

        public abstract void ApplyBodySecurity(XmlDictionaryWriter writer, IPrefixGenerator prefixGenerator);
        public abstract void ApplySecurityAndWriteHeaders(MessageHeaders headers, XmlDictionaryWriter writer, IPrefixGenerator prefixGenerator);
        private void CompleteEncryption()
        {
            ISecurityElement element = this.CompleteEncryptionCore(this.elementContainer.PrimarySignature, this.elementContainer.GetBasicSupportingTokens(), this.elementContainer.GetSignatureConfirmations(), this.elementContainer.GetEndorsingSignatures());
            if (element == null)
            {
                this.elementContainer.SourceEncryptionToken = null;
                this.elementContainer.WrappedEncryptionToken = null;
                this.elementContainer.DerivedEncryptionToken = null;
            }
            else
            {
                if (this.skipKeyInfoForEncryption)
                {
                    WrappedKeySecurityToken encryptingToken = this.encryptingToken as WrappedKeySecurityToken;
                    encryptingToken.EnsureEncryptedKeySetUp();
                    encryptingToken.EncryptedKey.ReferenceList = (ReferenceList) element;
                }
                else
                {
                    this.elementContainer.ReferenceList = element;
                }
                this.basicTokenEncrypted = true;
            }
        }

        protected abstract ISecurityElement CompleteEncryptionCore(SendSecurityHeaderElement primarySignature, SendSecurityHeaderElement[] basicTokens, SendSecurityHeaderElement[] signatureConfirmations, SendSecurityHeaderElement[] endorsingSignatures);
        protected abstract ISignatureValueSecurityElement CompletePrimarySignatureCore(SendSecurityHeaderElement[] signatureConfirmations, SecurityToken[] signedEndorsingTokens, SecurityToken[] signedTokens, SendSecurityHeaderElement[] basicTokens);
        internal void CompleteSecurityApplication()
        {
            if (this.SignThenEncrypt)
            {
                this.CompleteSignature();
                this.SignWithSupportingTokens();
                this.CompleteEncryption();
            }
            else
            {
                this.CompleteEncryption();
                this.CompleteSignature();
                this.SignWithSupportingTokens();
            }
            if (this.correlationState != null)
            {
                this.correlationState.SignatureConfirmations = this.GetSignatureValues();
            }
        }

        private void CompleteSignature()
        {
            ISignatureValueSecurityElement item = this.CompletePrimarySignatureCore(this.elementContainer.GetSignatureConfirmations(), this.elementContainer.GetSignedEndorsingSupportingTokens(), this.elementContainer.GetSignedSupportingTokens(), this.elementContainer.GetBasicSupportingTokens());
            if (item != null)
            {
                this.elementContainer.PrimarySignature = new SendSecurityHeaderElement(item.Id, item);
                this.elementContainer.PrimarySignature.MarkedForEncryption = this.encryptSignature;
                this.AddGeneratedSignatureValue(item.GetSignatureValue(), this.EncryptPrimarySignature);
                this.primarySignatureDone = true;
                this.primarySignatureValue = item.GetSignatureValue();
            }
        }

        protected virtual ISignatureValueSecurityElement[] CreateSignatureConfirmationElements(SignatureConfirmations signatureConfirmations)
        {
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SignatureConfirmationNotSupported")));
        }

        protected abstract ISignatureValueSecurityElement CreateSupportingSignature(SecurityToken token, SecurityKeyIdentifier identifier);
        protected abstract ISignatureValueSecurityElement CreateSupportingSignature(SecurityToken token, SecurityKeyIdentifier identifier, ISecurityElement primarySignature);
        public string GenerateId()
        {
            int index = this.idCounter++;
            if (this.idPrefix != null)
            {
                return (this.idPrefix + index);
            }
            if (index < ids.Length)
            {
                return ids[index];
            }
            return ("_" + index);
        }

        private SignatureConfirmations GetSignatureValues()
        {
            return this.signatureValuesGenerated;
        }

        private SecurityTokenReferenceStyle GetTokenReferenceStyle(SecurityTokenParameters parameters)
        {
            if (!ShouldSerializeToken(parameters, base.MessageDirection))
            {
                return SecurityTokenReferenceStyle.External;
            }
            return SecurityTokenReferenceStyle.Internal;
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (((this.basicSupportingTokenParameters != null) && (this.basicSupportingTokenParameters.Count > 0)) && (base.RequireMessageProtection && !this.basicTokenEncrypted))
            {
                throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BasicTokenCannotBeWrittenWithoutEncryption")), base.Message);
            }
            if ((this.elementContainer.Timestamp != null) && (base.Layout != SecurityHeaderLayout.LaxTimestampLast))
            {
                base.StandardsManager.WSUtilitySpecificationVersion.WriteTimestamp(writer, this.elementContainer.Timestamp);
            }
            if (this.elementContainer.PrerequisiteToken != null)
            {
                base.StandardsManager.SecurityTokenSerializer.WriteToken(writer, this.elementContainer.PrerequisiteToken);
            }
            if ((this.elementContainer.SourceSigningToken != null) && ShouldSerializeToken(this.signingTokenParameters, base.MessageDirection))
            {
                base.StandardsManager.SecurityTokenSerializer.WriteToken(writer, this.elementContainer.SourceSigningToken);
            }
            if (this.elementContainer.DerivedSigningToken != null)
            {
                base.StandardsManager.SecurityTokenSerializer.WriteToken(writer, this.elementContainer.DerivedSigningToken);
            }
            if (((this.elementContainer.SourceEncryptionToken != null) && (this.elementContainer.SourceEncryptionToken != this.elementContainer.SourceSigningToken)) && ShouldSerializeToken(this.encryptingTokenParameters, base.MessageDirection))
            {
                base.StandardsManager.SecurityTokenSerializer.WriteToken(writer, this.elementContainer.SourceEncryptionToken);
            }
            if (this.elementContainer.WrappedEncryptionToken != null)
            {
                base.StandardsManager.SecurityTokenSerializer.WriteToken(writer, this.elementContainer.WrappedEncryptionToken);
            }
            if (this.elementContainer.DerivedEncryptionToken != null)
            {
                base.StandardsManager.SecurityTokenSerializer.WriteToken(writer, this.elementContainer.DerivedEncryptionToken);
            }
            if (this.SignThenEncrypt && (this.elementContainer.ReferenceList != null))
            {
                this.elementContainer.ReferenceList.WriteTo(writer, ServiceModelDictionaryManager.Instance);
            }
            SecurityToken[] signedSupportingTokens = this.elementContainer.GetSignedSupportingTokens();
            if (signedSupportingTokens != null)
            {
                for (int i = 0; i < signedSupportingTokens.Length; i++)
                {
                    base.StandardsManager.SecurityTokenSerializer.WriteToken(writer, signedSupportingTokens[i]);
                }
            }
            SendSecurityHeaderElement[] basicSupportingTokens = this.elementContainer.GetBasicSupportingTokens();
            if (basicSupportingTokens != null)
            {
                for (int j = 0; j < basicSupportingTokens.Length; j++)
                {
                    basicSupportingTokens[j].Item.WriteTo(writer, ServiceModelDictionaryManager.Instance);
                }
            }
            SecurityToken[] endorsingSupportingTokens = this.elementContainer.GetEndorsingSupportingTokens();
            if (endorsingSupportingTokens != null)
            {
                for (int k = 0; k < endorsingSupportingTokens.Length; k++)
                {
                    if (ShouldSerializeToken(this.endorsingTokenParameters[k], base.MessageDirection))
                    {
                        base.StandardsManager.SecurityTokenSerializer.WriteToken(writer, endorsingSupportingTokens[k]);
                    }
                }
            }
            SecurityToken[] endorsingDerivedSupportingTokens = this.elementContainer.GetEndorsingDerivedSupportingTokens();
            if (endorsingDerivedSupportingTokens != null)
            {
                for (int m = 0; m < endorsingDerivedSupportingTokens.Length; m++)
                {
                    base.StandardsManager.SecurityTokenSerializer.WriteToken(writer, endorsingDerivedSupportingTokens[m]);
                }
            }
            SecurityToken[] signedEndorsingSupportingTokens = this.elementContainer.GetSignedEndorsingSupportingTokens();
            if (signedEndorsingSupportingTokens != null)
            {
                for (int n = 0; n < signedEndorsingSupportingTokens.Length; n++)
                {
                    base.StandardsManager.SecurityTokenSerializer.WriteToken(writer, signedEndorsingSupportingTokens[n]);
                }
            }
            SecurityToken[] signedEndorsingDerivedSupportingTokens = this.elementContainer.GetSignedEndorsingDerivedSupportingTokens();
            if (signedEndorsingDerivedSupportingTokens != null)
            {
                for (int num6 = 0; num6 < signedEndorsingDerivedSupportingTokens.Length; num6++)
                {
                    base.StandardsManager.SecurityTokenSerializer.WriteToken(writer, signedEndorsingDerivedSupportingTokens[num6]);
                }
            }
            SendSecurityHeaderElement[] signatureConfirmations = this.elementContainer.GetSignatureConfirmations();
            if (signatureConfirmations != null)
            {
                for (int num7 = 0; num7 < signatureConfirmations.Length; num7++)
                {
                    signatureConfirmations[num7].Item.WriteTo(writer, ServiceModelDictionaryManager.Instance);
                }
            }
            if ((this.elementContainer.PrimarySignature != null) && (this.elementContainer.PrimarySignature.Item != null))
            {
                this.elementContainer.PrimarySignature.Item.WriteTo(writer, ServiceModelDictionaryManager.Instance);
            }
            SendSecurityHeaderElement[] endorsingSignatures = this.elementContainer.GetEndorsingSignatures();
            if (endorsingSignatures != null)
            {
                for (int num8 = 0; num8 < endorsingSignatures.Length; num8++)
                {
                    endorsingSignatures[num8].Item.WriteTo(writer, ServiceModelDictionaryManager.Instance);
                }
            }
            if (!this.SignThenEncrypt && (this.elementContainer.ReferenceList != null))
            {
                this.elementContainer.ReferenceList.WriteTo(writer, ServiceModelDictionaryManager.Instance);
            }
            if ((this.elementContainer.Timestamp != null) && (base.Layout == SecurityHeaderLayout.LaxTimestampLast))
            {
                base.StandardsManager.WSUtilitySpecificationVersion.WriteTimestamp(writer, this.elementContainer.Timestamp);
            }
        }

        protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            base.StandardsManager.SecurityVersion.WriteStartHeader(writer);
            base.WriteHeaderAttributes(writer, messageVersion);
        }

        public void RemoveSignatureEncryptionIfAppropriate()
        {
            if (((((this.SignThenEncrypt && this.EncryptPrimarySignature) && (this.SecurityAppliedMessage.BodyProtectionMode != MessagePartProtectionMode.SignThenEncrypt)) && ((this.basicSupportingTokenParameters == null) || (this.basicSupportingTokenParameters.Count == 0))) && (((this.signatureConfirmationsToSend == null) || (this.signatureConfirmationsToSend.Count == 0)) || !this.signatureConfirmationsToSend.IsMarkedForEncryption)) && !this.HasSignedEncryptedMessagePart)
            {
                this.encryptSignature = false;
            }
        }

        public void SetEncryptionToken(SecurityToken token, SecurityTokenParameters tokenParameters)
        {
            base.ThrowIfProcessingStarted();
            if (((token == null) && (tokenParameters != null)) || ((token != null) && (tokenParameters == null)))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("TokenMustBeNullWhenTokenParametersAre")));
            }
            this.elementContainer.SourceEncryptionToken = token;
            this.encryptingTokenParameters = tokenParameters;
        }

        public void SetSigningToken(SecurityToken token, SecurityTokenParameters tokenParameters)
        {
            base.ThrowIfProcessingStarted();
            if (((token == null) && (tokenParameters != null)) || ((token != null) && (tokenParameters == null)))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("TokenMustBeNullWhenTokenParametersAre")));
            }
            this.elementContainer.SourceSigningToken = token;
            this.signingTokenParameters = tokenParameters;
        }

        public Message SetupExecution()
        {
            base.ThrowIfProcessingStarted();
            base.SetProcessingStarted();
            bool signBody = false;
            if (this.elementContainer.SourceSigningToken != null)
            {
                if (this.signatureParts == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("SignatureParts"), base.Message);
                }
                signBody = this.signatureParts.IsBodyIncluded;
            }
            bool encryptBody = false;
            if (this.elementContainer.SourceEncryptionToken != null)
            {
                if (this.encryptionParts == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("EncryptionParts"), base.Message);
                }
                encryptBody = this.encryptionParts.IsBodyIncluded;
            }
            System.ServiceModel.Security.SecurityAppliedMessage message = new System.ServiceModel.Security.SecurityAppliedMessage(base.Message, this, signBody, encryptBody);
            base.Message = message;
            return message;
        }

        internal static bool ShouldSerializeToken(SecurityTokenParameters parameters, MessageDirection transferDirection)
        {
            switch (parameters.InclusionMode)
            {
                case SecurityTokenInclusionMode.AlwaysToRecipient:
                case SecurityTokenInclusionMode.Once:
                    return (transferDirection == MessageDirection.Input);

                case SecurityTokenInclusionMode.Never:
                    return false;

                case SecurityTokenInclusionMode.AlwaysToInitiator:
                    return (transferDirection == MessageDirection.Output);
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UnsupportedTokenInclusionMode", new object[] { parameters.InclusionMode })));
        }

        private void SignWithSupportingToken(SecurityToken token, SecurityKeyIdentifierClause identifierClause)
        {
            ISignatureValueSecurityElement element;
            if (token == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("token", base.Message);
            }
            if (identifierClause == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenManagerCannotCreateTokenReference")), base.Message);
            }
            if (!base.RequireMessageProtection)
            {
                if (this.elementContainer.Timestamp == null)
                {
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SigningWithoutPrimarySignatureRequiresTimestamp")), base.Message);
                }
            }
            else
            {
                if (!this.primarySignatureDone)
                {
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PrimarySignatureMustBeComputedBeforeSupportingTokenSignatures")), base.Message);
                }
                if (this.elementContainer.PrimarySignature.Item == null)
                {
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SupportingTokenSignaturesNotExpected")), base.Message);
                }
            }
            SecurityKeyIdentifier identifier = new SecurityKeyIdentifier(new SecurityKeyIdentifierClause[] { identifierClause });
            if (!base.RequireMessageProtection)
            {
                element = this.CreateSupportingSignature(token, identifier);
            }
            else
            {
                element = this.CreateSupportingSignature(token, identifier, this.elementContainer.PrimarySignature.Item);
            }
            this.AddGeneratedSignatureValue(element.GetSignatureValue(), this.encryptSignature);
            SendSecurityHeaderElement signature = new SendSecurityHeaderElement(element.Id, element) {
                MarkedForEncryption = this.encryptSignature
            };
            this.elementContainer.AddEndorsingSignature(signature);
        }

        private void SignWithSupportingTokens()
        {
            SecurityToken[] endorsingSupportingTokens = this.elementContainer.GetEndorsingSupportingTokens();
            if (endorsingSupportingTokens != null)
            {
                for (int i = 0; i < endorsingSupportingTokens.Length; i++)
                {
                    SecurityToken token2;
                    SecurityKeyIdentifierClause clause2;
                    SecurityToken token = endorsingSupportingTokens[i];
                    SecurityKeyIdentifierClause tokenToDeriveIdentifier = this.endorsingTokenParameters[i].CreateKeyIdentifierClause(token, this.GetTokenReferenceStyle(this.endorsingTokenParameters[i]));
                    if (tokenToDeriveIdentifier == null)
                    {
                        throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenManagerCannotCreateTokenReference")), base.Message);
                    }
                    if (this.endorsingTokenParameters[i].RequireDerivedKeys && !this.endorsingTokenParameters[i].HasAsymmetricKey)
                    {
                        string keyDerivationAlgorithm = System.ServiceModel.Security.SecurityUtils.GetKeyDerivationAlgorithm(base.StandardsManager.MessageSecurityVersion.SecureConversationVersion);
                        DerivedKeySecurityToken token3 = new DerivedKeySecurityToken(-1, 0, base.AlgorithmSuite.GetSignatureKeyDerivationLength(token, base.StandardsManager.MessageSecurityVersion.SecureConversationVersion), null, 0x10, token, tokenToDeriveIdentifier, keyDerivationAlgorithm, this.GenerateId());
                        token2 = token3;
                        clause2 = new LocalIdKeyIdentifierClause(token3.Id, token3.GetType());
                        this.elementContainer.AddEndorsingDerivedSupportingToken(token3);
                    }
                    else
                    {
                        token2 = token;
                        clause2 = tokenToDeriveIdentifier;
                    }
                    this.SignWithSupportingToken(token2, clause2);
                }
            }
            SecurityToken[] signedEndorsingSupportingTokens = this.elementContainer.GetSignedEndorsingSupportingTokens();
            if (signedEndorsingSupportingTokens != null)
            {
                for (int j = 0; j < signedEndorsingSupportingTokens.Length; j++)
                {
                    SecurityToken token5;
                    SecurityKeyIdentifierClause clause4;
                    SecurityToken token4 = signedEndorsingSupportingTokens[j];
                    SecurityKeyIdentifierClause clause3 = this.signedEndorsingTokenParameters[j].CreateKeyIdentifierClause(token4, this.GetTokenReferenceStyle(this.signedEndorsingTokenParameters[j]));
                    if (clause3 == null)
                    {
                        throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenManagerCannotCreateTokenReference")), base.Message);
                    }
                    if (this.signedEndorsingTokenParameters[j].RequireDerivedKeys && !this.signedEndorsingTokenParameters[j].HasAsymmetricKey)
                    {
                        string derivationAlgorithm = System.ServiceModel.Security.SecurityUtils.GetKeyDerivationAlgorithm(base.StandardsManager.MessageSecurityVersion.SecureConversationVersion);
                        DerivedKeySecurityToken token6 = new DerivedKeySecurityToken(-1, 0, base.AlgorithmSuite.GetSignatureKeyDerivationLength(token4, base.StandardsManager.MessageSecurityVersion.SecureConversationVersion), null, 0x10, token4, clause3, derivationAlgorithm, this.GenerateId());
                        token5 = token6;
                        clause4 = new LocalIdKeyIdentifierClause(token6.Id, token6.GetType());
                        this.elementContainer.AddSignedEndorsingDerivedSupportingToken(token6);
                    }
                    else
                    {
                        token5 = token4;
                        clause4 = clause3;
                    }
                    this.SignWithSupportingToken(token5, clause4);
                }
            }
        }

        private void StartEncryption()
        {
            if (this.elementContainer.SourceEncryptionToken != null)
            {
                SecurityToken sourceEncryptionToken;
                SecurityKeyIdentifierClause clause2;
                SecurityKeyIdentifierClause clause3;
                SecurityKeyIdentifier identifier;
                SecurityTokenReferenceStyle tokenReferenceStyle = this.GetTokenReferenceStyle(this.encryptingTokenParameters);
                bool flag = tokenReferenceStyle == SecurityTokenReferenceStyle.Internal;
                SecurityKeyIdentifierClause clause = this.encryptingTokenParameters.CreateKeyIdentifierClause(this.elementContainer.SourceEncryptionToken, tokenReferenceStyle);
                if (clause == null)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenManagerCannotCreateTokenReference")), base.Message);
                }
                if (!System.ServiceModel.Security.SecurityUtils.HasSymmetricSecurityKey(this.elementContainer.SourceEncryptionToken))
                {
                    string str;
                    XmlDictionaryString str2;
                    int keyLength = Math.Max(0x80, base.AlgorithmSuite.DefaultSymmetricKeyLength);
                    System.ServiceModel.Security.CryptoHelper.ValidateSymmetricKeyLength(keyLength, base.AlgorithmSuite);
                    byte[] buffer = new byte[keyLength / 8];
                    System.ServiceModel.Security.CryptoHelper.FillRandomBytes(buffer);
                    base.AlgorithmSuite.GetKeyWrapAlgorithm(this.elementContainer.SourceEncryptionToken, out str, out str2);
                    WrappedKeySecurityToken token2 = new WrappedKeySecurityToken(this.GenerateId(), buffer, str, str2, this.elementContainer.SourceEncryptionToken, new SecurityKeyIdentifier(new SecurityKeyIdentifierClause[] { clause }));
                    this.elementContainer.WrappedEncryptionToken = token2;
                    sourceEncryptionToken = token2;
                    clause2 = new LocalIdKeyIdentifierClause(token2.Id, token2.GetType());
                    flag = true;
                }
                else
                {
                    sourceEncryptionToken = this.elementContainer.SourceEncryptionToken;
                    clause2 = clause;
                }
                if (this.encryptingTokenParameters.RequireDerivedKeys)
                {
                    string encryptionKeyDerivationAlgorithm = base.AlgorithmSuite.GetEncryptionKeyDerivationAlgorithm(sourceEncryptionToken, base.StandardsManager.MessageSecurityVersion.SecureConversationVersion);
                    string keyDerivationAlgorithm = System.ServiceModel.Security.SecurityUtils.GetKeyDerivationAlgorithm(base.StandardsManager.MessageSecurityVersion.SecureConversationVersion);
                    if (encryptionKeyDerivationAlgorithm != keyDerivationAlgorithm)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UnsupportedCryptoAlgorithm", new object[] { encryptionKeyDerivationAlgorithm })));
                    }
                    DerivedKeySecurityToken token3 = new DerivedKeySecurityToken(-1, 0, base.AlgorithmSuite.GetEncryptionKeyDerivationLength(sourceEncryptionToken, base.StandardsManager.MessageSecurityVersion.SecureConversationVersion), null, 0x10, sourceEncryptionToken, clause2, encryptionKeyDerivationAlgorithm, this.GenerateId());
                    this.encryptingToken = this.elementContainer.DerivedEncryptionToken = token3;
                    clause3 = new LocalIdKeyIdentifierClause(token3.Id, token3.GetType());
                }
                else
                {
                    this.encryptingToken = sourceEncryptionToken;
                    clause3 = clause2;
                }
                this.skipKeyInfoForEncryption = ((flag && base.EncryptedKeyContainsReferenceList) && (this.encryptingToken is WrappedKeySecurityToken)) && this.signThenEncrypt;
                if (this.skipKeyInfoForEncryption)
                {
                    identifier = null;
                }
                else
                {
                    identifier = new SecurityKeyIdentifier(new SecurityKeyIdentifierClause[] { clause3 });
                }
                this.StartEncryptionCore(this.encryptingToken, identifier);
            }
        }

        protected abstract void StartEncryptionCore(SecurityToken token, SecurityKeyIdentifier keyIdentifier);
        protected abstract void StartPrimarySignatureCore(SecurityToken token, SecurityKeyIdentifier identifier, MessagePartSpecification signatureParts, bool generateTargettablePrimarySignature);
        internal void StartSecurityApplication()
        {
            if (this.SignThenEncrypt)
            {
                this.StartSignature();
                this.StartEncryption();
            }
            else
            {
                this.StartEncryption();
                this.StartSignature();
            }
        }

        private void StartSignature()
        {
            if (this.elementContainer.SourceSigningToken != null)
            {
                SecurityToken sourceSigningToken;
                SecurityKeyIdentifierClause clause2;
                SecurityTokenReferenceStyle tokenReferenceStyle = this.GetTokenReferenceStyle(this.signingTokenParameters);
                SecurityKeyIdentifierClause tokenToDeriveIdentifier = this.signingTokenParameters.CreateKeyIdentifierClause(this.elementContainer.SourceSigningToken, tokenReferenceStyle);
                if (tokenToDeriveIdentifier == null)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenManagerCannotCreateTokenReference")), base.Message);
                }
                if (this.signingTokenParameters.RequireDerivedKeys && !this.signingTokenParameters.HasAsymmetricKey)
                {
                    string signatureKeyDerivationAlgorithm = base.AlgorithmSuite.GetSignatureKeyDerivationAlgorithm(this.elementContainer.SourceSigningToken, base.StandardsManager.MessageSecurityVersion.SecureConversationVersion);
                    string keyDerivationAlgorithm = System.ServiceModel.Security.SecurityUtils.GetKeyDerivationAlgorithm(base.StandardsManager.MessageSecurityVersion.SecureConversationVersion);
                    if (signatureKeyDerivationAlgorithm != keyDerivationAlgorithm)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UnsupportedCryptoAlgorithm", new object[] { signatureKeyDerivationAlgorithm })));
                    }
                    DerivedKeySecurityToken token2 = new DerivedKeySecurityToken(-1, 0, base.AlgorithmSuite.GetSignatureKeyDerivationLength(this.elementContainer.SourceSigningToken, base.StandardsManager.MessageSecurityVersion.SecureConversationVersion), null, 0x10, this.elementContainer.SourceSigningToken, tokenToDeriveIdentifier, signatureKeyDerivationAlgorithm, this.GenerateId());
                    sourceSigningToken = this.elementContainer.DerivedSigningToken = token2;
                    clause2 = new LocalIdKeyIdentifierClause(sourceSigningToken.Id, sourceSigningToken.GetType());
                }
                else
                {
                    sourceSigningToken = this.elementContainer.SourceSigningToken;
                    clause2 = tokenToDeriveIdentifier;
                }
                SecurityKeyIdentifier identifier = new SecurityKeyIdentifier(new SecurityKeyIdentifierClause[] { clause2 });
                if ((this.signatureConfirmationsToSend != null) && (this.signatureConfirmationsToSend.Count > 0))
                {
                    ISecurityElement[] elementArray = this.CreateSignatureConfirmationElements(this.signatureConfirmationsToSend);
                    for (int i = 0; i < elementArray.Length; i++)
                    {
                        SendSecurityHeaderElement confirmation = new SendSecurityHeaderElement(elementArray[i].Id, elementArray[i]) {
                            MarkedForEncryption = this.signatureConfirmationsToSend.IsMarkedForEncryption
                        };
                        this.elementContainer.AddSignatureConfirmation(confirmation);
                    }
                }
                bool generateTargettablePrimarySignature = (this.endorsingTokenParameters != null) || (this.signedEndorsingTokenParameters != null);
                this.StartPrimarySignatureCore(sourceSigningToken, identifier, this.signatureParts, generateTargettablePrimarySignature);
            }
        }

        public SecurityProtocolCorrelationState CorrelationState
        {
            get
            {
                return this.correlationState;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                this.correlationState = value;
            }
        }

        public SendSecurityHeaderElementContainer ElementContainer
        {
            get
            {
                return this.elementContainer;
            }
        }

        public MessagePartSpecification EncryptionParts
        {
            get
            {
                return this.encryptionParts;
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
                this.encryptionParts = value;
            }
        }

        public bool EncryptPrimarySignature
        {
            get
            {
                return this.encryptSignature;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                this.encryptSignature = value;
            }
        }

        public bool HasEncryptedTokens
        {
            get
            {
                return this.hasEncryptedTokens;
            }
        }

        protected virtual bool HasSignedEncryptedMessagePart
        {
            get
            {
                return false;
            }
        }

        public bool HasSignedTokens
        {
            get
            {
                return this.hasSignedTokens;
            }
        }

        public string IdPrefix
        {
            get
            {
                return this.idPrefix;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                this.idPrefix = (string.IsNullOrEmpty(value) || (value == "_")) ? null : value;
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

        internal byte[] PrimarySignatureValue
        {
            get
            {
                return this.primarySignatureValue;
            }
        }

        protected System.ServiceModel.Security.SecurityAppliedMessage SecurityAppliedMessage
        {
            get
            {
                return (System.ServiceModel.Security.SecurityAppliedMessage) base.Message;
            }
        }

        protected bool ShouldSignToHeader
        {
            get
            {
                return this.shouldSignToHeader;
            }
        }

        public MessagePartSpecification SignatureParts
        {
            get
            {
                return this.signatureParts;
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
                this.signatureParts = value;
            }
        }

        public bool SignThenEncrypt
        {
            get
            {
                return this.signThenEncrypt;
            }
            set
            {
                base.ThrowIfProcessingStarted();
                this.signThenEncrypt = value;
            }
        }

        XmlDictionaryString IMessageHeaderWithSharedNamespace.SharedNamespace
        {
            get
            {
                return System.ServiceModel.XD.UtilityDictionary.Namespace;
            }
        }

        XmlDictionaryString IMessageHeaderWithSharedNamespace.SharedPrefix
        {
            get
            {
                return System.ServiceModel.XD.UtilityDictionary.Prefix;
            }
        }

        public SecurityTimestamp Timestamp
        {
            get
            {
                return this.elementContainer.Timestamp;
            }
        }
    }
}

