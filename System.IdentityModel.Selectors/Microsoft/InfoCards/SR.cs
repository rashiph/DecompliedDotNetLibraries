namespace Microsoft.InfoCards
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class SR
    {
        internal const string AppliesToMustOnlyHaveEndpointAddress = "AppliesToMustOnlyHaveEndpointAddress";
        internal const string CallStackTraceFormat = "CallStackTraceFormat";
        internal const string CannotOpenImportFile = "CannotOpenImportFile";
        internal const string CardDoesNotMatchRequiredAuthType = "CardDoesNotMatchRequiredAuthType";
        internal const string ClaimIdNull = "ClaimIdNull";
        internal const string ClientAPICannotImport = "ClientAPICannotImport";
        internal const string ClientAPIDetailedExceptionMessage = "ClientAPIDetailedExceptionMessage";
        internal const string ClientAPIInfocardError = "ClientAPIInfocardError";
        internal const string ClientAPIInvalidIdentity = "ClientAPIInvalidIdentity";
        internal const string ClientAPIInvalidPolicy = "ClientAPIInvalidPolicy";
        internal const string ClientAPIServiceBusy = "ClientAPIServiceBusy";
        internal const string ClientAPIServiceNotInstalledError = "ClientAPIServiceNotInstalledError";
        internal const string ClientAPIServiceNotStartedError = "ClientAPIServiceNotStartedError";
        internal const string ClientAPIUIInitializationFailed = "ClientAPIUIInitializationFailed";
        internal const string ClientAPIUnsupportedPolicyOptions = "ClientAPIUnsupportedPolicyOptions";
        internal const string ClientAPIUntrustedRecipientError = "ClientAPIUntrustedRecipientError";
        internal const string ClientAPIUserCancellationError = "ClientAPIUserCancellationError";
        internal const string ClientCryptoSessionDisposed = "ClientCryptoSessionDisposed";
        internal const string ClientStsCommunicationException = "ClientStsCommunicationException";
        internal const string ClientUnsupportedCryptoAlgorithm = "ClientUnsupportedCryptoAlgorithm";
        internal const string CouldNotGenerateStrong3DesKey = "CouldNotGenerateStrong3DesKey";
        internal const string CountryDescription = "CountryDescription";
        internal const string CountryText = "CountryText";
        internal const string CreateProcessFailed = "CreateProcessFailed";
        internal const string DateOfBirthDescription = "DateOfBirthDescription";
        internal const string DateOfBirthText = "DateOfBirthText";
        internal const string EmailAddressDescription = "EmailAddressDescription";
        internal const string EmailAddressText = "EmailAddressText";
        internal const string EndpointNotFound = "EndpointNotFound";
        internal const string EntropyModeCannotHaveComputedKey = "EntropyModeCannotHaveComputedKey";
        internal const string EntropyModeRequiresProofToken = "EntropyModeRequiresProofToken";
        internal const string EventLogMessage = "EventLogMessage";
        internal const string ExtendedInfoInSelfIssued = "ExtendedInfoInSelfIssued";
        internal const string FailedReadingIPSTSPolicy = "FailedReadingIPSTSPolicy";
        internal const string FailedToBindToService = "FailedToBindToService";
        internal const string FailedToCreateProcessMutex = "FailedToCreateProcessMutex";
        internal const string FailedToSerializeObject = "FailedToSerializeObject";
        internal const string FailedToVerifySignature = "FailedToVerifySignature";
        internal const string FaultMessageFormat = "FaultMessageFormat";
        internal const string GenderDescription = "GenderDescription";
        internal const string GenderText = "GenderText";
        internal const string GeneralExceptionMessage = "GeneralExceptionMessage";
        internal const string GeneralInformation = "GeneralInformation";
        internal const string GeneralTraceMessage = "GeneralTraceMessage";
        internal const string GivenNameDescription = "GivenNameDescription";
        internal const string GivenNameText = "GivenNameText";
        internal const string HomePhoneDescription = "HomePhoneDescription";
        internal const string HomePhoneText = "HomePhoneText";
        internal const string IdentityProviderRequestedUnsupportedAuthType = "IdentityProviderRequestedUnsupportedAuthType";
        internal const string ImportFileNotFound = "ImportFileNotFound";
        internal const string ImportInaccesibleFile = "ImportInaccesibleFile";
        internal const string IndigoNoSuitableEndpointsForAddress = "IndigoNoSuitableEndpointsForAddress";
        internal const string InnerExceptionTraceFormat = "InnerExceptionTraceFormat";
        internal const string Invalid3DesKeySize = "Invalid3DesKeySize";
        internal const string InvalidAppliesToInPolicy = "InvalidAppliesToInPolicy";
        internal const string InvalidDisplayClaimType = "InvalidDisplayClaimType";
        internal const string InvalidDisplayToken = "InvalidDisplayToken";
        internal const string InvalidEntropyContents = "InvalidEntropyContents";
        internal const string InvalidFlagsSpecified = "InvalidFlagsSpecified";
        internal const string InvalidHACertificateStructure = "InvalidHACertificateStructure";
        internal const string InvalidImportFile = "InvalidImportFile";
        internal const string InvalidImportFileName = "InvalidImportFileName";
        internal const string InvalidIPSTSPolicy = "InvalidIPSTSPolicy";
        internal const string InvalidIssuerForIssuedToken = "InvalidIssuerForIssuedToken";
        internal const string InvalidKeyOption = "InvalidKeyOption";
        internal const string InvalidOrCorruptArgumentStream = "InvalidOrCorruptArgumentStream";
        internal const string InvalidPolicyLength = "InvalidPolicyLength";
        internal const string InvalidPolicySpecified = "InvalidPolicySpecified";
        internal const string InvalidRecipientSpecified = "InvalidRecipientSpecified";
        internal const string InvalidSelfIssuedUri = "InvalidSelfIssuedUri";
        internal const string InvalidServiceUri = "InvalidServiceUri";
        internal const string InvalidUriFormat = "InvalidUriFormat";
        internal const string IPSTSClientInvalidTokenReference = "IPSTSClientInvalidTokenReference";
        internal const string IPStsPolicyRequestingNonPpidClaims = "IPStsPolicyRequestingNonPpidClaims";
        internal const string KeySizeMustBeGreaterThanZero = "KeySizeMustBeGreaterThanZero";
        internal const string KeyTypeNotRecognized = "KeyTypeNotRecognized";
        internal const string LedgerEntryIncorrectType = "LedgerEntryIncorrectType";
        private static Microsoft.InfoCards.SR loader;
        internal const string LocalityDescription = "LocalityDescription";
        internal const string LocalityText = "LocalityText";
        internal const string LogoCouldNotCreateHashAlgorithm = "LogoCouldNotCreateHashAlgorithm";
        internal const string LogoHashValidationFailed = "LogoHashValidationFailed";
        internal const string LogoInvalidAsnLength = "LogoInvalidAsnLength";
        internal const string LogoInvalidCertificateLength = "LogoInvalidCertificateLength";
        internal const string LogoInvalidLogoType = "LogoInvalidLogoType";
        internal const string LogosPresentButNoHashes = "LogosPresentButNoHashes";
        internal const string LogoUnsupportedAudio = "LogoUnsupportedAudio";
        internal const string LogoUnsupportedIndirectReferences = "LogoUnsupportedIndirectReferences";
        internal const string LogoUnsupportedType = "LogoUnsupportedType";
        internal const string MobilePhoneDescription = "MobilePhoneDescription";
        internal const string MobilePhoneText = "MobilePhoneText";
        internal const string MoreThanOneEndPointFoundWhenNoIssuerIsSpecified = "MoreThanOneEndPointFoundWhenNoIssuerIsSpecified";
        internal const string MultipleEntropyElementsFound = "MultipleEntropyElementsFound";
        internal const string MultipleIssuerInformation = "MultipleIssuerInformation";
        internal const string MultipleKeySizeElementsFound = "MultipleKeySizeElementsFound";
        internal const string MultipleLifetimeElementsFound = "MultipleLifetimeElementsFound";
        internal const string MultiplePolicyElementsWithSameID = "MultiplePolicyElementsWithSameID";
        internal const string MultipleRequestedAttachedReferenceElementsFound = "MultipleRequestedAttachedReferenceElementsFound";
        internal const string MultipleRequestedDisplayTokenElementsFound = "MultipleRequestedDisplayTokenElementsFound";
        internal const string MultipleRequestedProofTokenElementsFound = "MultipleRequestedProofTokenElementsFound";
        internal const string MultipleRequestedSecurityTokenElementsFound = "MultipleRequestedSecurityTokenElementsFound";
        internal const string MultipleRequestedUnattachedReferenceElementsFound = "MultipleRequestedUnattachedReferenceElementsFound";
        internal const string MultipleRequestTypeElementsFound = "MultipleRequestTypeElementsFound";
        internal const string MultipleTokenElementsFoundInPolicy = "MultipleTokenElementsFoundInPolicy";
        internal const string MultipleTokenTypeElementsFound = "MultipleTokenTypeElementsFound";
        internal const string NoAppropriateEndPointFound = "NoAppropriateEndPointFound";
        internal const string NoAuthenticationServicesInCard = "NoAuthenticationServicesInCard";
        internal const string NoCachedCertificateForRecipient = "NoCachedCertificateForRecipient";
        internal const string NoCardNameSpecified = "NoCardNameSpecified";
        internal const string NoCertificateFoundInSignature = "NoCertificateFoundInSignature";
        internal const string NoCertificateInEndPoint = "NoCertificateInEndPoint";
        internal const string NoClaimsFoundInPolicy = "NoClaimsFoundInPolicy";
        internal const string NoIssuedTokenXml = "NoIssuedTokenXml";
        internal const string NoIssuerSpecifiedWhenMexIsSpecified = "NoIssuerSpecifiedWhenMexIsSpecified";
        internal const string NonApprovedlistedElementFound = "NonApprovedlistedElementFound";
        internal const string NonceLengthTooShort = "NonceLengthTooShort";
        internal const string NonHttpsURIFound = "NonHttpsURIFound";
        internal const string NoPolicyElementFound = "NoPolicyElementFound";
        internal const string NoProofKeyOnlyAllowedInBrowser = "NoProofKeyOnlyAllowedInBrowser";
        internal const string NoRecipientCertificateFound = "NoRecipientCertificateFound";
        internal const string NoSymmetricKeyFound = "NoSymmetricKeyFound";
        internal const string NoTokenReturned = "NoTokenReturned";
        internal const string NoValidPolicyElementFound = "NoValidPolicyElementFound";
        internal const string OnlyIssueRequestTypeSupported = "OnlyIssueRequestTypeSupported";
        internal const string OnlyPSha1SupportedCurrently = "OnlyPSha1SupportedCurrently";
        internal const string OtherPhoneDescription = "OtherPhoneDescription";
        internal const string OtherPhoneText = "OtherPhoneText";
        internal const string PostalCodeDescription = "PostalCodeDescription";
        internal const string PostalCodeText = "PostalCodeText";
        internal const string PPIDDescription = "PPIDDescription";
        internal const string PPIDText = "PPIDText";
        internal const string ProblemRetrievingTokenFromIdentityProvider = "ProblemRetrievingTokenFromIdentityProvider";
        internal const string ProofKeyTypeMismatch = "ProofKeyTypeMismatch";
        internal const string ProofTokenXmlUnexpectedInRstr = "ProofTokenXmlUnexpectedInRstr";
        internal const string RecipientCertificateNotValid = "RecipientCertificateNotValid";
        internal const string RecipientNotFromSameSecurityDomain = "RecipientNotFromSameSecurityDomain";
        internal const string RemoteCryptoSessionUnavailable = "RemoteCryptoSessionUnavailable";
        private ResourceManager resources;
        internal const string RPStsWithNoSSLFailure = "RPStsWithNoSSLFailure";
        internal const string SchemaValidationError = "SchemaValidationError";
        internal const string SchemaValidationFailed = "SchemaValidationFailed";
        internal const string SelfIssuedIssuerName = "SelfIssuedIssuerName";
        internal const string SelfIssuedUriUsed = "SelfIssuedUriUsed";
        internal const string SelfOrAnonIssuerNotAllowedWhenMexSpecified = "SelfOrAnonIssuerNotAllowedWhenMexSpecified";
        internal const string ServiceAsyncOpGeneratedException = "ServiceAsyncOpGeneratedException";
        internal const string ServiceBadKeySizeInPolicy = "ServiceBadKeySizeInPolicy";
        internal const string ServiceCanNotExportCertIdentityPrivateKey = "ServiceCanNotExportCertIdentityPrivateKey";
        internal const string ServiceCantGetRowWithNullID = "ServiceCantGetRowWithNullID";
        internal const string ServiceCantSerializeIncompleteInfoCard = "ServiceCantSerializeIncompleteInfoCard";
        internal const string ServiceCardDecryptionFailed = "ServiceCardDecryptionFailed";
        internal const string ServiceCardEncryptionFailed = "ServiceCardEncryptionFailed";
        internal const string ServiceCardWrongVersion = "ServiceCardWrongVersion";
        internal const string ServiceClientProcessExited = "ServiceClientProcessExited";
        internal const string ServiceCouldNotRetrieveCertNameString = "ServiceCouldNotRetrieveCertNameString";
        internal const string ServiceCrashedWithoutException = "ServiceCrashedWithoutException";
        internal const string ServiceDisplayTokenNoClaimName = "ServiceDisplayTokenNoClaimName";
        internal const string ServiceDoesNotSupportThisClaim = "ServiceDoesNotSupportThisClaim";
        internal const string ServiceDoesNotSupportThisTokenType = "ServiceDoesNotSupportThisTokenType";
        internal const string ServiceEncounteredFatalError = "ServiceEncounteredFatalError";
        internal const string ServiceEprDoesNotHaveValidMetadata = "ServiceEprDoesNotHaveValidMetadata";
        internal const string ServiceErrorGettingClientPid = "ServiceErrorGettingClientPid";
        internal const string ServiceErrorGettingClientTSSession = "ServiceErrorGettingClientTSSession";
        internal const string ServiceFailedToWriteToken = "ServiceFailedToWriteToken";
        internal const string ServiceInaccessibleFile = "ServiceInaccessibleFile";
        internal const string ServiceInformation = "ServiceInformation";
        internal const string ServiceInUseOnAnotherSession = "ServiceInUseOnAnotherSession";
        internal const string ServiceInvalidArgument = "ServiceInvalidArgument";
        internal const string ServiceInvalidArguments = "ServiceInvalidArguments";
        internal const string ServiceInvalidAsymmetricKeySize = "ServiceInvalidAsymmetricKeySize";
        internal const string ServiceInvalidAsyncHandle = "ServiceInvalidAsyncHandle";
        internal const string ServiceInvalidCallerToken = "ServiceInvalidCallerToken";
        internal const string ServiceInvalidClaimUri = "ServiceInvalidClaimUri";
        internal const string ServiceInvalidCredentialSelector = "ServiceInvalidCredentialSelector";
        internal const string ServiceInvalidDataInRequest = "ServiceInvalidDataInRequest";
        internal const string ServiceInvalidEncryptedClaimValues = "ServiceInvalidEncryptedClaimValues";
        internal const string ServiceInvalidEprInPolicy = "ServiceInvalidEprInPolicy";
        internal const string ServiceInvalidPrivacyNoticeVersion = "ServiceInvalidPrivacyNoticeVersion";
        internal const string ServiceInvalidSerialNumber = "ServiceInvalidSerialNumber";
        internal const string ServiceInvalidThumbPrintValue = "ServiceInvalidThumbPrintValue";
        internal const string ServiceInvalidTokenService = "ServiceInvalidTokenService";
        internal const string ServiceInvalidUri = "ServiceInvalidUri";
        internal const string ServiceObjectIsNotOfExpectedType = "ServiceObjectIsNotOfExpectedType";
        internal const string ServicePopulateIdBeforeInfoCardGet = "ServicePopulateIdBeforeInfoCardGet";
        internal const string ServicePopulateIdBeforeInfoCardGetClaims = "ServicePopulateIdBeforeInfoCardGetClaims";
        internal const string ServicePopulateIdBeforeInfoCardGetLedger = "ServicePopulateIdBeforeInfoCardGetLedger";
        internal const string ServiceProcessHasExited = "ServiceProcessHasExited";
        internal const string ServiceRequestBufferLengthInvalid = "ServiceRequestBufferLengthInvalid";
        internal const string ServiceSTSCommunicationFailed = "ServiceSTSCommunicationFailed";
        internal const string ServiceTokenEncryptionFailed = "ServiceTokenEncryptionFailed";
        internal const string ServiceTooManyAsyncOperations = "ServiceTooManyAsyncOperations";
        internal const string ServiceTooManyCryptoSessions = "ServiceTooManyCryptoSessions";
        internal const string ServiceUnableToDeserializeInfoCardStream = "ServiceUnableToDeserializeInfoCardStream";
        internal const string ServiceUnableToReadUIAgentSleepTime = "ServiceUnableToReadUIAgentSleepTime";
        internal const string ServiceUnableToValidateCallerToken = "ServiceUnableToValidateCallerToken";
        internal const string ServiceUnknownCryptoSessionId = "ServiceUnknownCryptoSessionId";
        internal const string ServiceUnsupportedFileSystem = "ServiceUnsupportedFileSystem";
        internal const string ServiceUnsupportedKeyDerivationAlgorithm = "ServiceUnsupportedKeyDerivationAlgorithm";
        internal const string ServiceUnsupportedKeyIdentifierType = "ServiceUnsupportedKeyIdentifierType";
        internal const string ServiceUnsupportedPolicyElementFound = "ServiceUnsupportedPolicyElementFound";
        internal const string SignatureNotVerified = "SignatureNotVerified";
        internal const string StateOrProvinceDescription = "StateOrProvinceDescription";
        internal const string StateOrProvinceText = "StateOrProvinceText";
        internal const string StoreAclsTamperedWith = "StoreAclsTamperedWith";
        internal const string StoreBeginTransaction = "StoreBeginTransaction";
        internal const string StoreCanNotUnmountSystemStorage = "StoreCanNotUnmountSystemStorage";
        internal const string StoreClosing = "StoreClosing";
        internal const string StoreCommitTransaction = "StoreCommitTransaction";
        internal const string StoreCryptProtectDataAsSystemFailed = "StoreCryptProtectDataAsSystemFailed";
        internal const string StoreCryptProtectDataFailed = "StoreCryptProtectDataFailed";
        internal const string StoreCryptUnprotectDataAsSystemFailed = "StoreCryptUnprotectDataAsSystemFailed";
        internal const string StoreCryptUnprotectDataFailed = "StoreCryptUnprotectDataFailed";
        internal const string StoreDataSourceCanNotImportToSelf = "StoreDataSourceCanNotImportToSelf";
        internal const string StoreDataSourceIdOutOfRange = "StoreDataSourceIdOutOfRange";
        internal const string StoreDataSourceInvalidIndexName = "StoreDataSourceInvalidIndexName";
        internal const string StoreDataSourceRowNotOwned = "StoreDataSourceRowNotOwned";
        internal const string StoreDataSourceWriteLockNotHeld = "StoreDataSourceWriteLockNotHeld";
        internal const string StoreDecryptedKeyIsNotValid = "StoreDecryptedKeyIsNotValid";
        internal const string StoreDeleting = "StoreDeleting";
        internal const string StoreFailedToOpenStore = "StoreFailedToOpenStore";
        internal const string StoreFileInUse = "StoreFileInUse";
        internal const string StoreFileNotProtectedByPassphrase = "StoreFileNotProtectedByPassphrase";
        internal const string StoreFileNotProtectedWithDPAPI = "StoreFileNotProtectedWithDPAPI";
        internal const string StoreFreeListSizeOutOfRange = "StoreFreeListSizeOutOfRange";
        internal const string StoreFreeListValueOutOfRange = "StoreFreeListValueOutOfRange";
        internal const string StoreHashUtilityDataOutOfRange = "StoreHashUtilityDataOutOfRange";
        internal const string StoreHashUtilityDataToHashOutOfRange = "StoreHashUtilityDataToHashOutOfRange";
        internal const string StoreHighValueOutOfRange = "StoreHighValueOutOfRange";
        internal const string StoreImpersonateLoggedOnUserFailed = "StoreImpersonateLoggedOnUserFailed";
        internal const string StoreIndexDataBufferDataLengthOutOfRange = "StoreIndexDataBufferDataLengthOutOfRange";
        internal const string StoreIndexDataBufferIndexOutOfRange = "StoreIndexDataBufferIndexOutOfRange";
        internal const string StoreIndexedDataBufferNullOrEmptyDataIndexBuffer = "StoreIndexedDataBufferNullOrEmptyDataIndexBuffer";
        internal const string StoreIndexedDataBufferNullOrEmptyMasterIndexBuffer = "StoreIndexedDataBufferNullOrEmptyMasterIndexBuffer";
        internal const string StoreIndexGrowthFactorInvalid = "StoreIndexGrowthFactorInvalid";
        internal const string StoreIndexInitialSizeInvalid = "StoreIndexInitialSizeInvalid";
        internal const string StoreIndexNameInvalid = "StoreIndexNameInvalid";
        internal const string StoreIndexObjectBufferOverflow = "StoreIndexObjectBufferOverflow";
        internal const string StoreIndexObjectCanNotBeCanonicalized = "StoreIndexObjectCanNotBeCanonicalized";
        internal const string StoreIndexObjectCanNotBeCompiled = "StoreIndexObjectCanNotBeCompiled";
        internal const string StoreIndexValueCanNotBeNull = "StoreIndexValueCanNotBeNull";
        internal const string StoreInvalidDataFilePath = "StoreInvalidDataFilePath";
        internal const string StoreIsAlreadyLoaded = "StoreIsAlreadyLoaded";
        internal const string StoreKeyAlreadyProtected = "StoreKeyAlreadyProtected";
        internal const string StoreKeyNotAlreadyProtected = "StoreKeyNotAlreadyProtected";
        internal const string StoreLastIndexOutOfRange = "StoreLastIndexOutOfRange";
        internal const string StoreLoading = "StoreLoading";
        internal const string StoreLocalIdOutOfRange = "StoreLocalIdOutOfRange";
        internal const string StoreLowValueOutOfRange = "StoreLowValueOutOfRange";
        internal const string StoreMoreThanOneRowReturnedInSingleMatchQuery = "StoreMoreThanOneRowReturnedInSingleMatchQuery";
        internal const string StoreNoReparsePointAllowed = "StoreNoReparsePointAllowed";
        internal const string StoreNullIndexValueNotPermitted = "StoreNullIndexValueNotPermitted";
        internal const string StorePassphraseKeyAlreadyProtected = "StorePassphraseKeyAlreadyProtected";
        internal const string StorePassphraseNotAlreadyProtected = "StorePassphraseNotAlreadyProtected";
        internal const string StoreProcessingTransaction = "StoreProcessingTransaction";
        internal const string StoreRollbackTransaction = "StoreRollbackTransaction";
        internal const string StoreRowOwnedByOtherDataSource = "StoreRowOwnedByOtherDataSource";
        internal const string StoreSignatureNotValid = "StoreSignatureNotValid";
        internal const string StoreSourceAlreadyMounted = "StoreSourceAlreadyMounted";
        internal const string StoreSourceIdOutOfRange = "StoreSourceIdOutOfRange";
        internal const string StoreUnableToGetStoreKeyFromDPAPI = "StoreUnableToGetStoreKeyFromDPAPI";
        internal const string StoreUniqueIndexConstraintBroken = "StoreUniqueIndexConstraintBroken";
        internal const string StoreVersionNotSupported = "StoreVersionNotSupported";
        internal const string StreetAddressDescription = "StreetAddressDescription";
        internal const string StreetAddressText = "StreetAddressText";
        internal const string StsCommunicationException = "StsCommunicationException";
        internal const string SurnameDescription = "SurnameDescription";
        internal const string SurnameText = "SurnameText";
        internal const string SymmetricProofKeyLengthMismatch = "SymmetricProofKeyLengthMismatch";
        internal const string TooLongClaimValue = "TooLongClaimValue";
        internal const string TooManyClientRequests = "TooManyClientRequests";
        internal const string TooManyClientUIConnections = "TooManyClientUIConnections";
        internal const string TooManyIssuedSecurityTokenParameters = "TooManyIssuedSecurityTokenParameters";
        internal const string UIAgentCrash = "UIAgentCrash";
        internal const string UnableToAuthenticateUIAgent = "UnableToAuthenticateUIAgent";
        internal const string UnableToBuildChainForNonHARecipient = "UnableToBuildChainForNonHARecipient";
        internal const string UnableToQueueThreadpool = "UnableToQueueThreadpool";
        internal const string UnexpectedElement = "UnexpectedElement";
        internal const string UnsupportedEncryptionAlgorithm = "UnsupportedEncryptionAlgorithm";
        internal const string UnsupportedEncryptWithAlgorithm = "UnsupportedEncryptWithAlgorithm";
        internal const string UnsupportedIdentityType = "UnsupportedIdentityType";
        internal const string UnsupportedSignatureAlgorithm = "UnsupportedSignatureAlgorithm";
        internal const string UnsupportedSignWithAlgorithm = "UnsupportedSignWithAlgorithm";
        internal const string UserIdentityEqualSystemNotSupported = "UserIdentityEqualSystemNotSupported";
        internal const string ValueMustBeInRange = "ValueMustBeInRange";
        internal const string ValueMustBeNonNegative = "ValueMustBeNonNegative";
        internal const string ValueMustBePositive = "ValueMustBePositive";
        internal const string WebPageDescription = "WebPageDescription";
        internal const string WebPageText = "WebPageText";
        internal const string X509ChainBuildFail = "X509ChainBuildFail";
        internal const string X509ChainFailAndPeerTrustFail = "X509ChainFailAndPeerTrustFail";

        internal SR()
        {
            this.resources = new ResourceManager("infocard", base.GetType().Assembly);
        }

        private static Microsoft.InfoCards.SR GetLoader()
        {
            if (loader == null)
            {
                Microsoft.InfoCards.SR sr = new Microsoft.InfoCards.SR();
                Interlocked.CompareExchange<Microsoft.InfoCards.SR>(ref loader, sr, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            Microsoft.InfoCards.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            Microsoft.InfoCards.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            Microsoft.InfoCards.SR loader = GetLoader();
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

