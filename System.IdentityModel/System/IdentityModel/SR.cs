namespace System.IdentityModel
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class SR
    {
        internal const string AESCipherModeNotSupported = "AESCipherModeNotSupported";
        internal const string AESCryptAcquireContextFailed = "AESCryptAcquireContextFailed";
        internal const string AESCryptDecryptFailed = "AESCryptDecryptFailed";
        internal const string AESCryptEncryptFailed = "AESCryptEncryptFailed";
        internal const string AESCryptGetKeyParamFailed = "AESCryptGetKeyParamFailed";
        internal const string AESCryptImportKeyFailed = "AESCryptImportKeyFailed";
        internal const string AESCryptSetKeyParamFailed = "AESCryptSetKeyParamFailed";
        internal const string AESInsufficientOutputBuffer = "AESInsufficientOutputBuffer";
        internal const string AESInvalidInputBlockSize = "AESInvalidInputBlockSize";
        internal const string AESIVLengthNotSupported = "AESIVLengthNotSupported";
        internal const string AESKeyLengthNotSupported = "AESKeyLengthNotSupported";
        internal const string AESPaddingModeNotSupported = "AESPaddingModeNotSupported";
        internal const string AlgorithmAndKeyMisMatch = "AlgorithmAndKeyMisMatch";
        internal const string AlgorithmAndPrivateKeyMisMatch = "AlgorithmAndPrivateKeyMisMatch";
        internal const string AlgorithmAndPublicKeyMisMatch = "AlgorithmAndPublicKeyMisMatch";
        internal const string AlgorithmMismatchForTransform = "AlgorithmMismatchForTransform";
        internal const string ArgumentCannotBeEmptyString = "ArgumentCannotBeEmptyString";
        internal const string ArgumentInvalidCertificate = "ArgumentInvalidCertificate";
        internal const string AsyncCallbackException = "AsyncCallbackException";
        internal const string AsyncObjectAlreadyEnded = "AsyncObjectAlreadyEnded";
        internal const string AtLeastOneReferenceRequired = "AtLeastOneReferenceRequired";
        internal const string AtLeastOneTransformRequired = "AtLeastOneTransformRequired";
        internal const string AuthorizationContextCreated = "AuthorizationContextCreated";
        internal const string AuthorizationPolicyEvaluated = "AuthorizationPolicyEvaluated";
        internal const string CannotFindCert = "CannotFindCert";
        internal const string CannotFindDocumentRoot = "CannotFindDocumentRoot";
        internal const string CannotValidateSecurityTokenType = "CannotValidateSecurityTokenType";
        internal const string ChildNodeTypeMissing = "ChildNodeTypeMissing";
        internal const string CouldNotFindNamespaceForPrefix = "CouldNotFindNamespaceForPrefix";
        internal const string CreateSequenceRefused = "CreateSequenceRefused";
        internal const string CryptoAlgorithmIsNotFipsCompliant = "CryptoAlgorithmIsNotFipsCompliant";
        internal const string CustomCryptoAlgorithmIsNotValidHashAlgorithm = "CustomCryptoAlgorithmIsNotValidHashAlgorithm";
        internal const string CustomCryptoAlgorithmIsNotValidKeyedHashAlgorithm = "CustomCryptoAlgorithmIsNotValidKeyedHashAlgorithm";
        internal const string CustomCryptoAlgorithmIsNotValidSymmetricAlgorithm = "CustomCryptoAlgorithmIsNotValidSymmetricAlgorithm";
        internal const string Default = "Default";
        internal const string DigestVerificationFailedForReference = "DigestVerificationFailedForReference";
        internal const string EmptyBase64Attribute = "EmptyBase64Attribute";
        internal const string EmptyOrNullArgumentString = "EmptyOrNullArgumentString";
        internal const string EmptyTransformChainNotSupported = "EmptyTransformChainNotSupported";
        internal const string EmptyXmlElementError = "EmptyXmlElementError";
        internal const string ExpectedElementMissing = "ExpectedElementMissing";
        internal const string ExternalDictionaryDoesNotContainAllRequiredStrings = "ExternalDictionaryDoesNotContainAllRequiredStrings";
        internal const string FailAcceptSecurityContext = "FailAcceptSecurityContext";
        internal const string FailedToDeleteKeyContainerFile = "FailedToDeleteKeyContainerFile";
        internal const string FailInitializeSecurityContext = "FailInitializeSecurityContext";
        internal const string FailLogonUser = "FailLogonUser";
        internal const string FoundMultipleCerts = "FoundMultipleCerts";
        internal const string ImpersonationLevelNotSupported = "ImpersonationLevelNotSupported";
        internal const string InclusiveNamespacePrefixRequiresSignatureReader = "InclusiveNamespacePrefixRequiresSignatureReader";
        internal const string IncorrectUserNameFormat = "IncorrectUserNameFormat";
        internal const string InnerReaderMustBeAtElement = "InnerReaderMustBeAtElement";
        internal const string InvalidAsyncResult = "InvalidAsyncResult";
        internal const string InvalidCustomCryptoAlgorithm = "InvalidCustomCryptoAlgorithm";
        internal const string InvalidCustomKeyWrapAlgorithm = "InvalidCustomKeyWrapAlgorithm";
        internal const string InvalidHexString = "InvalidHexString";
        internal const string InvalidNamespaceForEmptyPrefix = "InvalidNamespaceForEmptyPrefix";
        internal const string InvalidNtMapping = "InvalidNtMapping";
        internal const string InvalidOperationForWriterState = "InvalidOperationForWriterState";
        internal const string InvalidReaderState = "InvalidReaderState";
        internal const string InvalidServiceBindingInSspiNegotiationNoServiceBinding = "InvalidServiceBindingInSspiNegotiationNoServiceBinding";
        internal const string InvalidServiceBindingInSspiNegotiationServiceBindingNotMatched = "InvalidServiceBindingInSspiNegotiationServiceBindingNotMatched";
        internal const string InvalidXmlQualifiedName = "InvalidXmlQualifiedName";
        internal const string KerberosApReqInvalidOrOutOfMemory = "KerberosApReqInvalidOrOutOfMemory";
        internal const string KerberosMultilegsNotSupported = "KerberosMultilegsNotSupported";
        internal const string KeyIdentifierCannotCreateKey = "KeyIdentifierCannotCreateKey";
        internal const string KeyIdentifierClauseDoesNotSupportKeyCreation = "KeyIdentifierClauseDoesNotSupportKeyCreation";
        internal const string LastMessageNumberExceeded = "LastMessageNumberExceeded";
        internal const string LengthMustBeGreaterThanZero = "LengthMustBeGreaterThanZero";
        internal const string LengthOfArrayToConvertMustGreaterThanZero = "LengthOfArrayToConvertMustGreaterThanZero";
        private static System.IdentityModel.SR loader;
        internal const string LocalIdCannotBeEmpty = "LocalIdCannotBeEmpty";
        internal const string MessageNumberRollover = "MessageNumberRollover";
        internal const string MissingPrivateKey = "MissingPrivateKey";
        internal const string NoInputIsSetForCanonicalization = "NoInputIsSetForCanonicalization";
        internal const string NoKeyIdentifierClauseFound = "NoKeyIdentifierClauseFound";
        internal const string NoPrivateKeyAvailable = "NoPrivateKeyAvailable";
        internal const string ObjectIsReadOnly = "ObjectIsReadOnly";
        internal const string PrefixNotDefinedForNamespace = "PrefixNotDefinedForNamespace";
        internal const string PrivateKeyExchangeNotSupported = "PrivateKeyExchangeNotSupported";
        internal const string PrivateKeyNotDSA = "PrivateKeyNotDSA";
        internal const string PrivateKeyNotRSA = "PrivateKeyNotRSA";
        internal const string ProvidedNetworkCredentialsForKerberosHasInvalidUserName = "ProvidedNetworkCredentialsForKerberosHasInvalidUserName";
        internal const string Psha1KeyLengthInvalid = "Psha1KeyLengthInvalid";
        internal const string PublicKeyNotDSA = "PublicKeyNotDSA";
        internal const string PublicKeyNotRSA = "PublicKeyNotRSA";
        internal const string RequiredAttributeMissing = "RequiredAttributeMissing";
        internal const string RequiredTargetNotSigned = "RequiredTargetNotSigned";
        private ResourceManager resources;
        internal const string RevertingPrivilegeFailed = "RevertingPrivilegeFailed";
        internal const string SAMLActionNameRequired = "SAMLActionNameRequired";
        internal const string SAMLActionNameRequiredOnRead = "SAMLActionNameRequiredOnRead";
        internal const string SAMLAssertionIDIsInvalid = "SAMLAssertionIDIsInvalid";
        internal const string SAMLAssertionIdRequired = "SAMLAssertionIdRequired";
        internal const string SAMLAssertionIssuerRequired = "SAMLAssertionIssuerRequired";
        internal const string SAMLAssertionMissingIssuerAttributeOnRead = "SAMLAssertionMissingIssuerAttributeOnRead";
        internal const string SAMLAssertionMissingMajorVersionAttributeOnRead = "SAMLAssertionMissingMajorVersionAttributeOnRead";
        internal const string SAMLAssertionMissingMinorVersionAttributeOnRead = "SAMLAssertionMissingMinorVersionAttributeOnRead";
        internal const string SamlAssertionMissingSigningCredentials = "SamlAssertionMissingSigningCredentials";
        internal const string SAMLAssertionRequireOneStatement = "SAMLAssertionRequireOneStatement";
        internal const string SAMLAssertionRequireOneStatementOnRead = "SAMLAssertionRequireOneStatementOnRead";
        internal const string SamlAttributeClaimResourceShouldBeAString = "SamlAttributeClaimResourceShouldBeAString";
        internal const string SamlAttributeClaimRightShouldBePossessProperty = "SamlAttributeClaimRightShouldBePossessProperty";
        internal const string SAMLAttributeMissingNameAttributeOnRead = "SAMLAttributeMissingNameAttributeOnRead";
        internal const string SAMLAttributeMissingNamespaceAttributeOnRead = "SAMLAttributeMissingNamespaceAttributeOnRead";
        internal const string SAMLAttributeNameAttributeRequired = "SAMLAttributeNameAttributeRequired";
        internal const string SAMLAttributeNamespaceAttributeRequired = "SAMLAttributeNamespaceAttributeRequired";
        internal const string SAMLAttributeShouldHaveOneValue = "SAMLAttributeShouldHaveOneValue";
        internal const string SAMLAttributeStatementMissingAttributeOnRead = "SAMLAttributeStatementMissingAttributeOnRead";
        internal const string SAMLAttributeStatementMissingSubjectOnRead = "SAMLAttributeStatementMissingSubjectOnRead";
        internal const string SAMLAttributeValueCannotBeNull = "SAMLAttributeValueCannotBeNull";
        internal const string SAMLAudienceRestrictionInvalidAudienceValueOnRead = "SAMLAudienceRestrictionInvalidAudienceValueOnRead";
        internal const string SAMLAudienceRestrictionShouldHaveOneAudience = "SAMLAudienceRestrictionShouldHaveOneAudience";
        internal const string SAMLAudienceRestrictionShouldHaveOneAudienceOnRead = "SAMLAudienceRestrictionShouldHaveOneAudienceOnRead";
        internal const string SAMLAudienceUrisNotFound = "SAMLAudienceUrisNotFound";
        internal const string SAMLAudienceUriValidationFailed = "SAMLAudienceUriValidationFailed";
        internal const string SAMLAuthenticationStatementMissingAuthenticationInstanceOnRead = "SAMLAuthenticationStatementMissingAuthenticationInstanceOnRead";
        internal const string SAMLAuthenticationStatementMissingAuthenticationMethod = "SAMLAuthenticationStatementMissingAuthenticationMethod";
        internal const string SAMLAuthenticationStatementMissingAuthenticationMethodOnRead = "SAMLAuthenticationStatementMissingAuthenticationMethodOnRead";
        internal const string SAMLAuthenticationStatementMissingSubject = "SAMLAuthenticationStatementMissingSubject";
        internal const string SAMLAuthorityBindingInvalidAuthorityKind = "SAMLAuthorityBindingInvalidAuthorityKind";
        internal const string SAMLAuthorityBindingMissingAuthorityKind = "SAMLAuthorityBindingMissingAuthorityKind";
        internal const string SAMLAuthorityBindingMissingAuthorityKindOnRead = "SAMLAuthorityBindingMissingAuthorityKindOnRead";
        internal const string SAMLAuthorityBindingMissingBindingOnRead = "SAMLAuthorityBindingMissingBindingOnRead";
        internal const string SAMLAuthorityBindingMissingLocationOnRead = "SAMLAuthorityBindingMissingLocationOnRead";
        internal const string SAMLAuthorityBindingRequiresBinding = "SAMLAuthorityBindingRequiresBinding";
        internal const string SAMLAuthorityBindingRequiresLocation = "SAMLAuthorityBindingRequiresLocation";
        internal const string SAMLAuthorityKindMissingName = "SAMLAuthorityKindMissingName";
        internal const string SAMLAuthorizationDecisionHasMoreThanOneEvidence = "SAMLAuthorizationDecisionHasMoreThanOneEvidence";
        internal const string SAMLAuthorizationDecisionResourceRequired = "SAMLAuthorizationDecisionResourceRequired";
        internal const string SAMLAuthorizationDecisionShouldHaveOneAction = "SAMLAuthorizationDecisionShouldHaveOneAction";
        internal const string SAMLAuthorizationDecisionShouldHaveOneActionOnRead = "SAMLAuthorizationDecisionShouldHaveOneActionOnRead";
        internal const string SAMLAuthorizationDecisionStatementMissingDecisionAttributeOnRead = "SAMLAuthorizationDecisionStatementMissingDecisionAttributeOnRead";
        internal const string SAMLAuthorizationDecisionStatementMissingResourceAttributeOnRead = "SAMLAuthorizationDecisionStatementMissingResourceAttributeOnRead";
        internal const string SAMLAuthorizationDecisionStatementMissingSubjectOnRead = "SAMLAuthorizationDecisionStatementMissingSubjectOnRead";
        internal const string SAMLBadSchema = "SAMLBadSchema";
        internal const string SAMLElementNotRecognized = "SAMLElementNotRecognized";
        internal const string SAMLEntityCannotBeNullOrEmpty = "SAMLEntityCannotBeNullOrEmpty";
        internal const string SAMLEvidenceShouldHaveOneAssertion = "SAMLEvidenceShouldHaveOneAssertion";
        internal const string SAMLEvidenceShouldHaveOneAssertionOnRead = "SAMLEvidenceShouldHaveOneAssertionOnRead";
        internal const string SamlInvalidSigningToken = "SamlInvalidSigningToken";
        internal const string SAMLNameIdentifierMissingIdentifierValueOnRead = "SAMLNameIdentifierMissingIdentifierValueOnRead";
        internal const string SamlSerializerRequiresExternalSerializers = "SamlSerializerRequiresExternalSerializers";
        internal const string SamlSerializerUnableToReadSecurityKeyIdentifier = "SamlSerializerUnableToReadSecurityKeyIdentifier";
        internal const string SamlSerializerUnableToWriteSecurityKeyIdentifier = "SamlSerializerUnableToWriteSecurityKeyIdentifier";
        internal const string SAMLSignatureAlreadyRead = "SAMLSignatureAlreadyRead";
        internal const string SamlSigningTokenMissing = "SamlSigningTokenMissing";
        internal const string SamlSigningTokenNotFound = "SamlSigningTokenNotFound";
        internal const string SAMLSubjectConfirmationClauseMissingConfirmationMethodOnRead = "SAMLSubjectConfirmationClauseMissingConfirmationMethodOnRead";
        internal const string SAMLSubjectNameIdentifierRequiresNameValue = "SAMLSubjectNameIdentifierRequiresNameValue";
        internal const string SAMLSubjectRequiresConfirmationMethodWhenConfirmationDataOrKeyInfoIsSpecified = "SAMLSubjectRequiresConfirmationMethodWhenConfirmationDataOrKeyInfoIsSpecified";
        internal const string SAMLSubjectRequiresNameIdentifierOrConfirmationMethod = "SAMLSubjectRequiresNameIdentifierOrConfirmationMethod";
        internal const string SAMLSubjectRequiresNameIdentifierOrConfirmationMethodOnRead = "SAMLSubjectRequiresNameIdentifierOrConfirmationMethodOnRead";
        internal const string SAMLSubjectStatementRequiresSubject = "SAMLSubjectStatementRequiresSubject";
        internal const string SamlTokenAuthenticatorCanOnlyProcessSamlTokens = "SamlTokenAuthenticatorCanOnlyProcessSamlTokens";
        internal const string SamlTokenMissingSignature = "SamlTokenMissingSignature";
        internal const string SAMLTokenNotSerialized = "SAMLTokenNotSerialized";
        internal const string SAMLTokenTimeInvalid = "SAMLTokenTimeInvalid";
        internal const string SAMLTokenVersionNotSupported = "SAMLTokenVersionNotSupported";
        internal const string SamlUnableToExtractSubjectKey = "SamlUnableToExtractSubjectKey";
        internal const string SAMLUnableToLoadAdvice = "SAMLUnableToLoadAdvice";
        internal const string SAMLUnableToLoadAssertion = "SAMLUnableToLoadAssertion";
        internal const string SAMLUnableToLoadAttribute = "SAMLUnableToLoadAttribute";
        internal const string SAMLUnableToLoadCondtion = "SAMLUnableToLoadCondtion";
        internal const string SAMLUnableToLoadCondtions = "SAMLUnableToLoadCondtions";
        internal const string SAMLUnableToLoadStatement = "SAMLUnableToLoadStatement";
        internal const string SAMLUnableToLoadUnknownElement = "SAMLUnableToLoadUnknownElement";
        internal const string SAMLUnableToResolveSignatureKey = "SAMLUnableToResolveSignatureKey";
        internal const string SecurityChannelBindingMissing = "SecurityChannelBindingMissing";
        internal const string SecurityTokenManagerCannotCreateAuthenticatorForRequirement = "SecurityTokenManagerCannotCreateAuthenticatorForRequirement";
        internal const string SecurityTokenRequirementDoesNotContainProperty = "SecurityTokenRequirementDoesNotContainProperty";
        internal const string SecurityTokenRequirementHasInvalidTypeForProperty = "SecurityTokenRequirementHasInvalidTypeForProperty";
        internal const string SettingdMayBeModifiedOnlyWhenTheWriterIsInStartState = "SettingdMayBeModifiedOnlyWhenTheWriterIsInStartState";
        internal const string SignatureVerificationFailed = "SignatureVerificationFailed";
        internal const string SigningTokenHasNoKeys = "SigningTokenHasNoKeys";
        internal const string SigningTokenHasNoKeysSupportingTheAlgorithmSuite = "SigningTokenHasNoKeysSupportingTheAlgorithmSuite";
        internal const string SspiLoginPromptHeaderMessage = "SspiLoginPromptHeaderMessage";
        internal const string SSPIPackageNotSupported = "SSPIPackageNotSupported";
        internal const string SspiPayloadNotEncrypted = "SspiPayloadNotEncrypted";
        internal const string SspiWrapperEncryptDecryptAssert1 = "SspiWrapperEncryptDecryptAssert1";
        internal const string SspiWrapperEncryptDecryptAssert2 = "SspiWrapperEncryptDecryptAssert2";
        internal const string SuiteDoesNotAcceptAlgorithm = "SuiteDoesNotAcceptAlgorithm";
        internal const string SymmetricKeyLengthTooShort = "SymmetricKeyLengthTooShort";
        internal const string TokenCancellationNotSupported = "TokenCancellationNotSupported";
        internal const string TokenDoesNotMeetKeySizeRequirements = "TokenDoesNotMeetKeySizeRequirements";
        internal const string TokenDoesNotSupportKeyIdentifierClauseCreation = "TokenDoesNotSupportKeyIdentifierClauseCreation";
        internal const string TokenProviderUnableToGetToken = "TokenProviderUnableToGetToken";
        internal const string TokenProviderUnableToRenewToken = "TokenProviderUnableToRenewToken";
        internal const string TokenRenewalNotSupported = "TokenRenewalNotSupported";
        internal const string TraceCodeIdentityModel = "TraceCodeIdentityModel";
        internal const string UnableToCreateHashAlgorithmFromAsymmetricCrypto = "UnableToCreateHashAlgorithmFromAsymmetricCrypto";
        internal const string UnableToCreateKerberosCredentials = "UnableToCreateKerberosCredentials";
        internal const string UnableToCreateKeyedHashAlgorithm = "UnableToCreateKeyedHashAlgorithm";
        internal const string UnableToCreateKeyedHashAlgorithmFromSymmetricCrypto = "UnableToCreateKeyedHashAlgorithmFromSymmetricCrypto";
        internal const string UnableToCreateSignatureDeformatterFromAsymmetricCrypto = "UnableToCreateSignatureDeformatterFromAsymmetricCrypto";
        internal const string UnableToCreateSignatureFormatterFromAsymmetricCrypto = "UnableToCreateSignatureFormatterFromAsymmetricCrypto";
        internal const string UnableToCreateTokenReference = "UnableToCreateTokenReference";
        internal const string UnableToFindPrefix = "UnableToFindPrefix";
        internal const string UnableToResolveKeyReference = "UnableToResolveKeyReference";
        internal const string UnableToResolveReferenceInSamlSignature = "UnableToResolveReferenceInSamlSignature";
        internal const string UnableToResolveReferenceUriForSignature = "UnableToResolveReferenceUriForSignature";
        internal const string UnableToResolveTokenReference = "UnableToResolveTokenReference";
        internal const string UnboundPrefixInQName = "UnboundPrefixInQName";
        internal const string UndefinedUseOfPrefixAtAttribute = "UndefinedUseOfPrefixAtAttribute";
        internal const string UndefinedUseOfPrefixAtElement = "UndefinedUseOfPrefixAtElement";
        internal const string UnexpectedEndOfFile = "UnexpectedEndOfFile";
        internal const string UnexpectedEOFFromReader = "UnexpectedEOFFromReader";
        internal const string UnexpectedXmlChildNode = "UnexpectedXmlChildNode";
        internal const string UnknownICryptoType = "UnknownICryptoType";
        internal const string UnsupportedAlgorithmForCryptoOperation = "UnsupportedAlgorithmForCryptoOperation";
        internal const string UnsupportedCryptoAlgorithm = "UnsupportedCryptoAlgorithm";
        internal const string UnsupportedEncryptionAlgorithm = "UnsupportedEncryptionAlgorithm";
        internal const string UnsupportedInputTypeForTransform = "UnsupportedInputTypeForTransform";
        internal const string UnsupportedKeyDerivationAlgorithm = "UnsupportedKeyDerivationAlgorithm";
        internal const string UnsupportedKeyWrapAlgorithm = "UnsupportedKeyWrapAlgorithm";
        internal const string UnsupportedLastTransform = "UnsupportedLastTransform";
        internal const string UnsupportedNodeTypeInReader = "UnsupportedNodeTypeInReader";
        internal const string UnsupportedTransformAlgorithm = "UnsupportedTransformAlgorithm";
        internal const string UserNameAuthenticationFailed = "UserNameAuthenticationFailed";
        internal const string UserNameCannotBeEmpty = "UserNameCannotBeEmpty";
        internal const string ValueMustBeGreaterThanZero = "ValueMustBeGreaterThanZero";
        internal const string ValueMustBeInRange = "ValueMustBeInRange";
        internal const string ValueMustBeNonNegative = "ValueMustBeNonNegative";
        internal const string ValueMustBeOf2Types = "ValueMustBeOf2Types";
        internal const string ValueMustBeOne = "ValueMustBeOne";
        internal const string ValueMustBePositive = "ValueMustBePositive";
        internal const string ValueMustBeZero = "ValueMustBeZero";
        internal const string X509CertStoreLocationNotValid = "X509CertStoreLocationNotValid";
        internal const string X509ChainBuildFail = "X509ChainBuildFail";
        internal const string X509ChainBuildFailedWhenMappingCertToWindowsAccount = "X509ChainBuildFailedWhenMappingCertToWindowsAccount";
        internal const string X509FindValueMismatch = "X509FindValueMismatch";
        internal const string X509FindValueMismatchMulti = "X509FindValueMismatchMulti";
        internal const string X509InvalidUsageTime = "X509InvalidUsageTime";
        internal const string X509IsInUntrustedStore = "X509IsInUntrustedStore";
        internal const string X509IsNotInTrustedStore = "X509IsNotInTrustedStore";
        internal const string X509ValidationFail = "X509ValidationFail";
        internal const string XDCannotFindValueInDictionaryString = "XDCannotFindValueInDictionaryString";
        internal const string XmlBufferQuotaExceeded = "XmlBufferQuotaExceeded";
        internal const string XmlLangAttributeMissing = "XmlLangAttributeMissing";
        internal const string XmlTokenBufferIsEmpty = "XmlTokenBufferIsEmpty";

        internal SR()
        {
            this.resources = new ResourceManager("System.IdentityModel", base.GetType().Assembly);
        }

        private static System.IdentityModel.SR GetLoader()
        {
            if (loader == null)
            {
                System.IdentityModel.SR sr = new System.IdentityModel.SR();
                Interlocked.CompareExchange<System.IdentityModel.SR>(ref loader, sr, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            System.IdentityModel.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            System.IdentityModel.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            System.IdentityModel.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader.resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                {
                    args[i] = str2.Substring(0, 0x3fd) + "...";
                }
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return GetString(name);
        }

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }
    }
}

