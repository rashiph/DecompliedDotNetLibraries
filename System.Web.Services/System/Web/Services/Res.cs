namespace System.Web.Services
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class Res
    {
        internal const string AsyncDuplicateUserState = "AsyncDuplicateUserState";
        internal const string Binding = "Binding";
        internal const string BindingInvalidAttribute = "BindingInvalidAttribute";
        internal const string BindingMissingAttribute = "BindingMissingAttribute";
        internal const string BindingMultipleParts = "BindingMultipleParts";
        internal const string BindingOperationMissing = "BindingOperationMissing";
        internal const string BothAndUseTheMessageNameUseTheMessageName3 = "BothAndUseTheMessageNameUseTheMessageName3";
        internal const string BPConformanceHeaderFault = "BPConformanceHeaderFault";
        internal const string BPConformanceSoapEncodedMethod = "BPConformanceSoapEncodedMethod";
        internal const string CanTCallTheEndMethodOfAnAsyncCallMoreThan = "CanTCallTheEndMethodOfAnAsyncCallMoreThan";
        internal const string CanTMergeBinding = "CanTMergeBinding";
        internal const string CanTMergeMessage = "CanTMergeMessage";
        internal const string CanTMergePortType = "CanTMergePortType";
        internal const string CanTMergeService = "CanTMergeService";
        internal const string CanTMergeTypes = "CanTMergeTypes";
        internal const string CanTSpecifyElementOnEncodedMessagePartsPart = "CanTSpecifyElementOnEncodedMessagePartsPart";
        internal const string ClientProtocolAllowAutoRedirect = "ClientProtocolAllowAutoRedirect";
        internal const string ClientProtocolClientCertificates = "ClientProtocolClientCertificates";
        internal const string ClientProtocolCookieContainer = "ClientProtocolCookieContainer";
        internal const string ClientProtocolDomain = "ClientProtocolDomain";
        internal const string ClientProtocolEnableDecompression = "ClientProtocolEnableDecompression";
        internal const string ClientProtocolEncoding = "ClientProtocolEncoding";
        internal const string ClientProtocolPassword = "ClientProtocolPassword";
        internal const string ClientProtocolPreAuthenticate = "ClientProtocolPreAuthenticate";
        internal const string ClientProtocolProxyName = "ClientProtocolProxyName";
        internal const string ClientProtocolProxyPort = "ClientProtocolProxyPort";
        internal const string ClientProtocolSoapVersion = "ClientProtocolSoapVersion";
        internal const string ClientProtocolTimeout = "ClientProtocolTimeout";
        internal const string ClientProtocolUrl = "ClientProtocolUrl";
        internal const string ClientProtocolUserAgent = "ClientProtocolUserAgent";
        internal const string ClientProtocolUsername = "ClientProtocolUsername";
        internal const string CodeGenSupportParameterAttributes = "CodeGenSupportParameterAttributes";
        internal const string CodeGenSupportReferenceParameters = "CodeGenSupportReferenceParameters";
        internal const string CodeGenSupportReturnTypeAttributes = "CodeGenSupportReturnTypeAttributes";
        internal const string CodegenWarningDetails = "CodegenWarningDetails";
        internal const string CodeRemarks = "CodeRemarks";
        internal const string ConfigKeyNotFoundInElementCollection = "ConfigKeyNotFoundInElementCollection";
        internal const string ConfigKeysDoNotMatch = "ConfigKeysDoNotMatch";
        internal const string ContractOverride = "ContractOverride";
        internal const string Description = "Description";
        internal const string DiscoveryIsNotPossibleBecauseTypeIsMissing1 = "DiscoveryIsNotPossibleBecauseTypeIsMissing1";
        internal const string DuplicateInputOutputNames0 = "DuplicateInputOutputNames0";
        internal const string EachMessagePartInAnUseEncodedMessageMustSpecify0 = "EachMessagePartInAnUseEncodedMessageMustSpecify0";
        internal const string EachMessagePartInAUseLiteralMessageMustSpecify0 = "EachMessagePartInAUseLiteralMessageMustSpecify0";
        internal const string EachMessagePartInRpcUseLiteralMessageMustSpecify0 = "EachMessagePartInRpcUseLiteralMessageMustSpecify0";
        internal const string Element = "Element";
        internal const string ElementEncodedExtension = "ElementEncodedExtension";
        internal const string ElementTypeMustBeObjectOrSoapExtensionOrSoapReflectedException = "ElementTypeMustBeObjectOrSoapExtensionOrSoapReflectedException";
        internal const string ElementTypeMustBeObjectOrSoapReflectedException = "ElementTypeMustBeObjectOrSoapReflectedException";
        internal const string FailedToHandleRequest0 = "FailedToHandleRequest0";
        internal const string Fault = "Fault";
        internal const string FaultBinding = "FaultBinding";
        internal const string HeaderFault = "HeaderFault";
        internal const string HelpGeneratorDefaultNamespaceHelp1 = "HelpGeneratorDefaultNamespaceHelp1";
        internal const string HelpGeneratorDefaultNamespaceHelp2 = "HelpGeneratorDefaultNamespaceHelp2";
        internal const string HelpGeneratorDefaultNamespaceHelp3 = "HelpGeneratorDefaultNamespaceHelp3";
        internal const string HelpGeneratorDefaultNamespaceHelp4 = "HelpGeneratorDefaultNamespaceHelp4";
        internal const string HelpGeneratorDefaultNamespaceHelp5 = "HelpGeneratorDefaultNamespaceHelp5";
        internal const string HelpGeneratorDefaultNamespaceHelp6 = "HelpGeneratorDefaultNamespaceHelp6";
        internal const string HelpGeneratorDefaultNamespaceWarning1 = "HelpGeneratorDefaultNamespaceWarning1";
        internal const string HelpGeneratorDefaultNamespaceWarning2 = "HelpGeneratorDefaultNamespaceWarning2";
        internal const string HelpGeneratorEnableHttpPostHeader = "HelpGeneratorEnableHttpPostHeader";
        internal const string HelpGeneratorEnableHttpPostInstructions = "HelpGeneratorEnableHttpPostInstructions";
        internal const string HelpGeneratorHttpGetText = "HelpGeneratorHttpGetText";
        internal const string HelpGeneratorHttpGetTitle = "HelpGeneratorHttpGetTitle";
        internal const string HelpGeneratorHttpPostText = "HelpGeneratorHttpPostText";
        internal const string HelpGeneratorHttpPostTitle = "HelpGeneratorHttpPostTitle";
        internal const string HelpGeneratorImplementation = "HelpGeneratorImplementation";
        internal const string HelpGeneratorInternalError = "HelpGeneratorInternalError";
        internal const string HelpGeneratorInvokeButton = "HelpGeneratorInvokeButton";
        internal const string HelpGeneratorLanguageConfig = "HelpGeneratorLanguageConfig";
        internal const string HelpGeneratorLinkBack = "HelpGeneratorLinkBack";
        internal const string HelpGeneratorMethodNotFound = "HelpGeneratorMethodNotFound";
        internal const string HelpGeneratorMethodNotFoundText = "HelpGeneratorMethodNotFoundText";
        internal const string HelpGeneratorNoHttpGetTest = "HelpGeneratorNoHttpGetTest";
        internal const string HelpGeneratorNoHttpPostTest = "HelpGeneratorNoHttpPostTest";
        internal const string HelpGeneratorNoTestFormRemote = "HelpGeneratorNoTestFormRemote";
        internal const string HelpGeneratorNoTestNonPrimitive = "HelpGeneratorNoTestNonPrimitive";
        internal const string HelpGeneratorOperationsIntro = "HelpGeneratorOperationsIntro";
        internal const string HelpGeneratorParameter = "HelpGeneratorParameter";
        internal const string HelpGeneratorRecommendation = "HelpGeneratorRecommendation";
        internal const string HelpGeneratorServiceConformance = "HelpGeneratorServiceConformance";
        internal const string HelpGeneratorServiceConformanceConfig = "HelpGeneratorServiceConformanceConfig";
        internal const string HelpGeneratorServiceConformanceDetails = "HelpGeneratorServiceConformanceDetails";
        internal const string HelpGeneratorServiceConformanceHelp = "HelpGeneratorServiceConformanceHelp";
        internal const string HelpGeneratorServiceConformanceR1014 = "HelpGeneratorServiceConformanceR1014";
        internal const string HelpGeneratorServiceConformanceR1014_r = "HelpGeneratorServiceConformanceR1014_r";
        internal const string HelpGeneratorServiceConformanceR2007 = "HelpGeneratorServiceConformanceR2007";
        internal const string HelpGeneratorServiceConformanceR2007_r = "HelpGeneratorServiceConformanceR2007_r";
        internal const string HelpGeneratorServiceConformanceR2026 = "HelpGeneratorServiceConformanceR2026";
        internal const string HelpGeneratorServiceConformanceR2028 = "HelpGeneratorServiceConformanceR2028";
        internal const string HelpGeneratorServiceConformanceR2105 = "HelpGeneratorServiceConformanceR2105";
        internal const string HelpGeneratorServiceConformanceR2105_r = "HelpGeneratorServiceConformanceR2105_r";
        internal const string HelpGeneratorServiceConformanceR2201 = "HelpGeneratorServiceConformanceR2201";
        internal const string HelpGeneratorServiceConformanceR2203 = "HelpGeneratorServiceConformanceR2203";
        internal const string HelpGeneratorServiceConformanceR2204 = "HelpGeneratorServiceConformanceR2204";
        internal const string HelpGeneratorServiceConformanceR2205 = "HelpGeneratorServiceConformanceR2205";
        internal const string HelpGeneratorServiceConformanceR2210 = "HelpGeneratorServiceConformanceR2210";
        internal const string HelpGeneratorServiceConformanceR2210_r = "HelpGeneratorServiceConformanceR2210_r";
        internal const string HelpGeneratorServiceConformanceR2303 = "HelpGeneratorServiceConformanceR2303";
        internal const string HelpGeneratorServiceConformanceR2304 = "HelpGeneratorServiceConformanceR2304";
        internal const string HelpGeneratorServiceConformanceR2304_r = "HelpGeneratorServiceConformanceR2304_r";
        internal const string HelpGeneratorServiceConformanceR2306 = "HelpGeneratorServiceConformanceR2306";
        internal const string HelpGeneratorServiceConformanceR2701 = "HelpGeneratorServiceConformanceR2701";
        internal const string HelpGeneratorServiceConformanceR2702 = "HelpGeneratorServiceConformanceR2702";
        internal const string HelpGeneratorServiceConformanceR2705 = "HelpGeneratorServiceConformanceR2705";
        internal const string HelpGeneratorServiceConformanceR2705_r = "HelpGeneratorServiceConformanceR2705_r";
        internal const string HelpGeneratorServiceConformanceR2706 = "HelpGeneratorServiceConformanceR2706";
        internal const string HelpGeneratorServiceConformanceR2706_r = "HelpGeneratorServiceConformanceR2706_r";
        internal const string HelpGeneratorServiceConformanceR2710 = "HelpGeneratorServiceConformanceR2710";
        internal const string HelpGeneratorServiceConformanceR2710_r = "HelpGeneratorServiceConformanceR2710_r";
        internal const string HelpGeneratorServiceConformanceR2716 = "HelpGeneratorServiceConformanceR2716";
        internal const string HelpGeneratorServiceConformanceR2717 = "HelpGeneratorServiceConformanceR2717";
        internal const string HelpGeneratorServiceConformanceR2718 = "HelpGeneratorServiceConformanceR2718";
        internal const string HelpGeneratorServiceConformanceR2720 = "HelpGeneratorServiceConformanceR2720";
        internal const string HelpGeneratorServiceConformanceR2721 = "HelpGeneratorServiceConformanceR2721";
        internal const string HelpGeneratorServiceConformanceR2726 = "HelpGeneratorServiceConformanceR2726";
        internal const string HelpGeneratorServiceConformanceR2749 = "HelpGeneratorServiceConformanceR2749";
        internal const string HelpGeneratorServiceConformanceR2754 = "HelpGeneratorServiceConformanceR2754";
        internal const string HelpGeneratorServiceConformanceR2803 = "HelpGeneratorServiceConformanceR2803";
        internal const string HelpGeneratorServiceConformanceR2803_r = "HelpGeneratorServiceConformanceR2803_r";
        internal const string HelpGeneratorServiceConformanceRxxxx = "HelpGeneratorServiceConformanceRxxxx";
        internal const string HelpGeneratorServiceConformanceRxxxx_r = "HelpGeneratorServiceConformanceRxxxx_r";
        internal const string HelpGeneratorSoap1_2Text = "HelpGeneratorSoap1_2Text";
        internal const string HelpGeneratorSoap1_2Title = "HelpGeneratorSoap1_2Title";
        internal const string HelpGeneratorSoapText = "HelpGeneratorSoapText";
        internal const string HelpGeneratorSoapTitle = "HelpGeneratorSoapTitle";
        internal const string HelpGeneratorStyleAactive = "HelpGeneratorStyleAactive";
        internal const string HelpGeneratorStyleAhover = "HelpGeneratorStyleAhover";
        internal const string HelpGeneratorStyleAlink = "HelpGeneratorStyleAlink";
        internal const string HelpGeneratorStyleAvisited = "HelpGeneratorStyleAvisited";
        internal const string HelpGeneratorStyleBODY = "HelpGeneratorStyleBODY";
        internal const string HelpGeneratorStylebutton = "HelpGeneratorStylebutton";
        internal const string HelpGeneratorStylecontent = "HelpGeneratorStylecontent";
        internal const string HelpGeneratorStylefontError = "HelpGeneratorStylefontError";
        internal const string HelpGeneratorStylefontkey = "HelpGeneratorStylefontkey";
        internal const string HelpGeneratorStylefontvalue = "HelpGeneratorStylefontvalue";
        internal const string HelpGeneratorStylefrmheader = "HelpGeneratorStylefrmheader";
        internal const string HelpGeneratorStylefrmInput = "HelpGeneratorStylefrmInput";
        internal const string HelpGeneratorStylefrmtext = "HelpGeneratorStylefrmtext";
        internal const string HelpGeneratorStyleh2 = "HelpGeneratorStyleh2";
        internal const string HelpGeneratorStyleh3 = "HelpGeneratorStyleh3";
        internal const string HelpGeneratorStyleheading1 = "HelpGeneratorStyleheading1";
        internal const string HelpGeneratorStyleintro = "HelpGeneratorStyleintro";
        internal const string HelpGeneratorStyleli = "HelpGeneratorStyleli";
        internal const string HelpGeneratorStyleol = "HelpGeneratorStyleol";
        internal const string HelpGeneratorStyleP = "HelpGeneratorStyleP";
        internal const string HelpGeneratorStylepre = "HelpGeneratorStylepre";
        internal const string HelpGeneratorStyletd = "HelpGeneratorStyletd";
        internal const string HelpGeneratorStyleul = "HelpGeneratorStyleul";
        internal const string HelpGeneratorTestHeader = "HelpGeneratorTestHeader";
        internal const string HelpGeneratorTestText = "HelpGeneratorTestText";
        internal const string HelpGeneratorValue = "HelpGeneratorValue";
        internal const string HelpGeneratorWebService = "HelpGeneratorWebService";
        internal const string IfAppSettingBaseUrlArgumentIsSpecifiedThen0 = "IfAppSettingBaseUrlArgumentIsSpecifiedThen0";
        internal const string indexMustBeBetweenAnd0Inclusive = "indexMustBeBetweenAnd0Inclusive";
        internal const string InitFailed = "InitFailed";
        internal const string InputElement = "InputElement";
        internal const string InternalConfigurationError0 = "InternalConfigurationError0";
        internal const string internalError0 = "internalError0";
        internal const string Invalid_priority_group_value = "Invalid_priority_group_value";
        private static Res loader;
        internal const string Message = "Message";
        internal const string MessageHasNoParts1 = "MessageHasNoParts1";
        internal const string Missing2 = "Missing2";
        internal const string MissingBinding0 = "MissingBinding0";
        internal const string MissingHttpOperationElement0 = "MissingHttpOperationElement0";
        internal const string MissingInputBinding0 = "MissingInputBinding0";
        internal const string MissingMatchElement0 = "MissingMatchElement0";
        internal const string MissingMessage2 = "MissingMessage2";
        internal const string MissingMessagePartForMessageFromNamespace3 = "MissingMessagePartForMessageFromNamespace3";
        internal const string MissingOutputBinding0 = "MissingOutputBinding0";
        internal const string MissingSoapBodyInputBinding0 = "MissingSoapBodyInputBinding0";
        internal const string MissingSoapBodyOutputBinding0 = "MissingSoapBodyOutputBinding0";
        internal const string MissingSoapOperationBinding0 = "MissingSoapOperationBinding0";
        internal const string MultipleBindingsWithSameName2 = "MultipleBindingsWithSameName2";
        internal const string NeedConcreteType = "NeedConcreteType";
        internal const string NoInputHTTPFormatsWereRecognized0 = "NoInputHTTPFormatsWereRecognized0";
        internal const string NoInputMIMEFormatsWereRecognized0 = "NoInputMIMEFormatsWereRecognized0";
        internal const string NoMethodsWereFoundInTheWSDLForThisProtocol = "NoMethodsWereFoundInTheWSDLForThisProtocol";
        internal const string NonClsCompliantException = "NonClsCompliantException";
        internal const string NoOutputMIMEFormatsWereRecognized0 = "NoOutputMIMEFormatsWereRecognized0";
        internal const string NotificationIsNotSupported0 = "NotificationIsNotSupported0";
        internal const string OneWayIsNotSupported0 = "OneWayIsNotSupported0";
        internal const string OnlyOneWebServiceBindingAttributeMayBeSpecified1 = "OnlyOneWebServiceBindingAttributeMayBeSpecified1";
        internal const string OnlyOperationInputOrOperationOutputTypes = "OnlyOperationInputOrOperationOutputTypes";
        internal const string OnlyXmlElementsOrTypesDerivingFromServiceDescriptionFormatExtension0 = "OnlyXmlElementsOrTypesDerivingFromServiceDescriptionFormatExtension0";
        internal const string Operation = "Operation";
        internal const string OperationBinding = "OperationBinding";
        internal const string OperationFlowNotification = "OperationFlowNotification";
        internal const string OperationFlowSolicitResponse = "OperationFlowSolicitResponse";
        internal const string OperationMissingBinding = "OperationMissingBinding";
        internal const string OperationOverload = "OperationOverload";
        internal const string OutputElement = "OutputElement";
        internal const string Part = "Part";
        internal const string Port = "Port";
        internal const string PortTypeOperationMissing = "PortTypeOperationMissing";
        internal const string ProtocolDoesNotAsyncSerialize = "ProtocolDoesNotAsyncSerialize";
        internal const string ProtocolWithNameIsNotRecognized1 = "ProtocolWithNameIsNotRecognized1";
        internal const string RequestResponseIsNotSupported0 = "RequestResponseIsNotSupported0";
        internal const string RequiredXmlFormatExtensionAttributeIsMissing1 = "RequiredXmlFormatExtensionAttributeIsMissing1";
        private ResourceManager resources;
        internal const string Rxxxx = "Rxxxx";
        internal const string SchemaSyntaxErrorDetails = "SchemaSyntaxErrorDetails";
        internal const string SchemaSyntaxErrorItemDetails = "SchemaSyntaxErrorItemDetails";
        internal const string SchemaValidationError = "SchemaValidationError";
        internal const string SchemaValidationWarning = "SchemaValidationWarning";
        internal const string ServiceDescriptionWasNotFound0 = "ServiceDescriptionWasNotFound0";
        internal const string SolicitResponseIsNotSupported0 = "SolicitResponseIsNotSupported0";
        internal const string SpecifyingAnElementForUseEncodedMessageParts0 = "SpecifyingAnElementForUseEncodedMessageParts0";
        internal const string SpecifyingATypeForUseLiteralMessagesIs0 = "SpecifyingATypeForUseLiteralMessagesIs0";
        internal const string SpecifyingATypeForUseLiteralMessagesIsAny = "SpecifyingATypeForUseLiteralMessagesIsAny";
        internal const string StackTraceEnd = "StackTraceEnd";
        internal const string StreamDoesNotRead = "StreamDoesNotRead";
        internal const string StreamDoesNotSeek = "StreamDoesNotSeek";
        internal const string SyntaxErrorInWSDLDocumentMessageDoesNotHave1 = "SyntaxErrorInWSDLDocumentMessageDoesNotHave1";
        internal const string TheBinding0FromNamespace1WasIgnored2 = "TheBinding0FromNamespace1WasIgnored2";
        internal const string TheBindingNamedFromNamespaceWasNotFoundIn3 = "TheBindingNamedFromNamespaceWasNotFoundIn3";
        internal const string TheCombinationOfStyleRpcWithUseLiteralIsNot0 = "TheCombinationOfStyleRpcWithUseLiteralIsNot0";
        internal const string TheDocumentWasNotRecognizedAsAKnownDocumentType = "TheDocumentWasNotRecognizedAsAKnownDocumentType";
        internal const string TheDocumentWasUnderstoodButContainsErrors = "TheDocumentWasUnderstoodButContainsErrors";
        internal const string TheEncodingIsNotSupported1 = "TheEncodingIsNotSupported1";
        internal const string TheHTMLDocumentDoesNotContainDiscoveryInformation = "TheHTMLDocumentDoesNotContainDiscoveryInformation";
        internal const string TheMethodDoesNotHaveARequestElementEither1 = "TheMethodDoesNotHaveARequestElementEither1";
        internal const string TheMethodsAndUseTheSameRequestElementAndSoapActionXmlns6 = "TheMethodsAndUseTheSameRequestElementAndSoapActionXmlns6";
        internal const string TheMethodsAndUseTheSameRequestElementXmlns4 = "TheMethodsAndUseTheSameRequestElementXmlns4";
        internal const string TheMethodsAndUseTheSameSoapActionWhenTheService3 = "TheMethodsAndUseTheSameSoapActionWhenTheService3";
        internal const string TheOperation0FromNamespace1WasIgnored2 = "TheOperation0FromNamespace1WasIgnored2";
        internal const string TheOperationBinding0FromNamespace1WasIgnored = "TheOperationBinding0FromNamespace1WasIgnored";
        internal const string TheOperationBindingFromNamespaceHadInvalid3 = "TheOperationBindingFromNamespaceHadInvalid3";
        internal const string TheOperationFromNamespaceHadInvalidSyntax3 = "TheOperationFromNamespaceHadInvalidSyntax3";
        internal const string TheOperationStyleRpcButBothMessagesAreNot0 = "TheOperationStyleRpcButBothMessagesAreNot0";
        internal const string ThereIsNoSoapTransportImporterThatUnderstands1 = "ThereIsNoSoapTransportImporterThatUnderstands1";
        internal const string TheRequestElementXmlnsWasNotRecognized2 = "TheRequestElementXmlnsWasNotRecognized2";
        internal const string ThereWasAnErrorDownloading0 = "ThereWasAnErrorDownloading0";
        internal const string ThereWasAnErrorDuringAsyncProcessing = "ThereWasAnErrorDuringAsyncProcessing";
        internal const string TheRootElementForTheRequestCouldNotBeDetermined0 = "TheRootElementForTheRequestCouldNotBeDetermined0";
        internal const string TheSchemaDocumentContainsLinksThatCouldNotBeResolved = "TheSchemaDocumentContainsLinksThatCouldNotBeResolved";
        internal const string TheSyntaxOfTypeMayNotBeExtended1 = "TheSyntaxOfTypeMayNotBeExtended1";
        internal const string TheWSDLDocumentContainsLinksThatCouldNotBeResolved = "TheWSDLDocumentContainsLinksThatCouldNotBeResolved";
        internal const string TraceCallEnter = "TraceCallEnter";
        internal const string TraceCallEnterDetails = "TraceCallEnterDetails";
        internal const string TraceCallExit = "TraceCallExit";
        internal const string TraceCreateSerializer = "TraceCreateSerializer";
        internal const string TraceExceptionCought = "TraceExceptionCought";
        internal const string TraceExceptionDetails = "TraceExceptionDetails";
        internal const string TraceExceptionIgnored = "TraceExceptionIgnored";
        internal const string TraceExceptionThrown = "TraceExceptionThrown";
        internal const string TracePostWorkItemIn = "TracePostWorkItemIn";
        internal const string TracePostWorkItemOut = "TracePostWorkItemOut";
        internal const string TraceReadHeaders = "TraceReadHeaders";
        internal const string TraceReadRequest = "TraceReadRequest";
        internal const string TraceReadResponse = "TraceReadResponse";
        internal const string TraceUrl = "TraceUrl";
        internal const string TraceUrlReferrer = "TraceUrlReferrer";
        internal const string TraceUserHostAddress = "TraceUserHostAddress";
        internal const string TraceUserHostName = "TraceUserHostName";
        internal const string TraceWriteHeaders = "TraceWriteHeaders";
        internal const string TraceWriteRequest = "TraceWriteRequest";
        internal const string TraceWriteResponse = "TraceWriteResponse";
        internal const string TypeIsMissingWebServiceBindingAttributeThat2 = "TypeIsMissingWebServiceBindingAttributeThat2";
        internal const string UnableToHandleRequest0 = "UnableToHandleRequest0";
        internal const string UnableToHandleRequestActionNotRecognized1 = "UnableToHandleRequestActionNotRecognized1";
        internal const string UnableToHandleRequestActionRequired0 = "UnableToHandleRequestActionRequired0";
        internal const string UnableToImportBindingFromNamespace2 = "UnableToImportBindingFromNamespace2";
        internal const string UnableToImportOperation1 = "UnableToImportOperation1";
        internal const string UnexpectedFlush = "UnexpectedFlush";
        internal const string UnknownWebServicesProtocolInConfigFile1 = "UnknownWebServicesProtocolInConfigFile1";
        internal const string UnsupportedMessageStyle1 = "UnsupportedMessageStyle1";
        internal const string UriValueRelative = "UriValueRelative";
        internal const string ValidationError = "ValidationError";
        internal const string WebAsyncMissingEnd = "WebAsyncMissingEnd";
        internal const string WebAsyncTransaction = "WebAsyncTransaction";
        internal const string WebBadEnum = "WebBadEnum";
        internal const string WebBadHex = "WebBadHex";
        internal const string WebBadOutParameter = "WebBadOutParameter";
        internal const string WebBadStreamState = "WebBadStreamState";
        internal const string WebBothMethodAttrs = "WebBothMethodAttrs";
        internal const string WebBothServiceAttrs = "WebBothServiceAttrs";
        internal const string WebCannotAccessValue = "WebCannotAccessValue";
        internal const string WebCannotAccessValueStage = "WebCannotAccessValueStage";
        internal const string WebCannotUnderstandHeader = "WebCannotUnderstandHeader";
        internal const string WebChangeTypeFailed = "WebChangeTypeFailed";
        internal const string WebClientBindingAttributeRequired = "WebClientBindingAttributeRequired";
        internal const string WebConfigExtensionError = "WebConfigExtensionError";
        internal const string WebConfigInvalidExtensionPriority = "WebConfigInvalidExtensionPriority";
        internal const string WebContractReferenceName = "WebContractReferenceName";
        internal const string WebDescriptionHeaderAndBodyUseMismatch = "WebDescriptionHeaderAndBodyUseMismatch";
        internal const string WebDescriptionMissing = "WebDescriptionMissing";
        internal const string WebDescriptionMissingBodyUseAttribute = "WebDescriptionMissingBodyUseAttribute";
        internal const string WebDescriptionMissingItem = "WebDescriptionMissingItem";
        internal const string WebDescriptionPartElementRequired = "WebDescriptionPartElementRequired";
        internal const string WebDescriptionPartElementWarning = "WebDescriptionPartElementWarning";
        internal const string WebDescriptionPartTypeRequired = "WebDescriptionPartTypeRequired";
        internal const string WebDescriptionPartTypeWarning = "WebDescriptionPartTypeWarning";
        internal const string WebDescriptionTooManyMessages = "WebDescriptionTooManyMessages";
        internal const string WebDiscoRefReport = "WebDiscoRefReport";
        internal const string WebDiscoveryDocumentReferenceName = "WebDiscoveryDocumentReferenceName";
        internal const string WebDuplicateBinding = "WebDuplicateBinding";
        internal const string WebDuplicateFaultBinding = "WebDuplicateFaultBinding";
        internal const string WebDuplicateFormatExtension = "WebDuplicateFormatExtension";
        internal const string WebDuplicateImport = "WebDuplicateImport";
        internal const string WebDuplicateMessage = "WebDuplicateMessage";
        internal const string WebDuplicateMessagePart = "WebDuplicateMessagePart";
        internal const string WebDuplicateOperation = "WebDuplicateOperation";
        internal const string WebDuplicateOperationBinding = "WebDuplicateOperationBinding";
        internal const string WebDuplicateOperationFault = "WebDuplicateOperationFault";
        internal const string WebDuplicateOperationMessage = "WebDuplicateOperationMessage";
        internal const string WebDuplicatePort = "WebDuplicatePort";
        internal const string WebDuplicatePortType = "WebDuplicatePortType";
        internal const string WebDuplicateService = "WebDuplicateService";
        internal const string WebDuplicateServiceDescription = "WebDuplicateServiceDescription";
        internal const string WebDuplicateUnknownElement = "WebDuplicateUnknownElement";
        internal const string WebEmptyRef = "WebEmptyRef";
        internal const string WebExtensionError = "WebExtensionError";
        internal const string WebHeaderInvalidMustUnderstand = "WebHeaderInvalidMustUnderstand";
        internal const string WebHeaderInvalidRelay = "WebHeaderInvalidRelay";
        internal const string WebHeaderMissing = "WebHeaderMissing";
        internal const string WebHeaderOneWayOut = "WebHeaderOneWayOut";
        internal const string WebHeaderRead = "WebHeaderRead";
        internal const string WebHeaderStatic = "WebHeaderStatic";
        internal const string WebHeaderType = "WebHeaderType";
        internal const string WebHeaderWrite = "WebHeaderWrite";
        internal const string WebHttpHeader = "WebHttpHeader";
        internal const string WebInOutParameter = "WebInOutParameter";
        internal const string WebInvalidBindingName = "WebInvalidBindingName";
        internal const string WebInvalidBindingPlacement = "WebInvalidBindingPlacement";
        internal const string WebInvalidContentType = "WebInvalidContentType";
        internal const string WebInvalidDocType = "WebInvalidDocType";
        internal const string WebInvalidEnvelopeNamespace = "WebInvalidEnvelopeNamespace";
        internal const string WebInvalidFormat = "WebInvalidFormat";
        internal const string WebInvalidMethodName = "WebInvalidMethodName";
        internal const string WebInvalidMethodNameCase = "WebInvalidMethodNameCase";
        internal const string WebInvalidRequestFormat = "WebInvalidRequestFormat";
        internal const string WebInvalidRequestFormatDetails = "WebInvalidRequestFormatDetails";
        internal const string WebMethodMissingParams = "WebMethodMissingParams";
        internal const string WebMethodStatic = "WebMethodStatic";
        internal const string WebMissingBodyElement = "WebMissingBodyElement";
        internal const string WebMissingClientProtocol = "WebMissingClientProtocol";
        internal const string WebMissingCustomAttribute = "WebMissingCustomAttribute";
        internal const string WebMissingDocument = "WebMissingDocument";
        internal const string WebMissingEnvelopeElement = "WebMissingEnvelopeElement";
        internal const string WebMissingHeader = "WebMissingHeader";
        internal const string WebMissingHelpContext = "WebMissingHelpContext";
        internal const string WebMissingParameter = "WebMissingParameter";
        internal const string WebMissingPath = "WebMissingPath";
        internal const string WebMissingResource = "WebMissingResource";
        internal const string WebMultiDimArray = "WebMultiDimArray";
        internal const string WebMultiplyDeclaredHeaderTypes = "WebMultiplyDeclaredHeaderTypes";
        internal const string WebNegativeValue = "WebNegativeValue";
        internal const string WebNoReturnValue = "WebNoReturnValue";
        internal const string WebNullAsyncResultInBegin = "WebNullAsyncResultInBegin";
        internal const string WebNullAsyncResultInEnd = "WebNullAsyncResultInEnd";
        internal const string WebNullReaderForMessage = "WebNullReaderForMessage";
        internal const string WebNullRef = "WebNullRef";
        internal const string WebNullWriterForMessage = "WebNullWriterForMessage";
        internal const string WebOneWayOutParameters = "WebOneWayOutParameters";
        internal const string WebOneWayReturnValue = "WebOneWayReturnValue";
        internal const string WebPathNotFound = "WebPathNotFound";
        internal const string WebQNamePrefixUndefined = "WebQNamePrefixUndefined";
        internal const string WebRefDuplicateSchema = "WebRefDuplicateSchema";
        internal const string WebRefDuplicateService = "WebRefDuplicateService";
        internal const string WebRefInvalidAttribute = "WebRefInvalidAttribute";
        internal const string WebRefInvalidAttribute2 = "WebRefInvalidAttribute2";
        internal const string WebReflectionError = "WebReflectionError";
        internal const string WebReflectionErrorMethod = "WebReflectionErrorMethod";
        internal const string WebRequestContent = "WebRequestContent";
        internal const string WebRequestUnableToProcess = "WebRequestUnableToProcess";
        internal const string WebRequestUnableToRead = "WebRequestUnableToRead";
        internal const string WebResolveMissingClientProtocol = "WebResolveMissingClientProtocol";
        internal const string WebResponseBadXml = "WebResponseBadXml";
        internal const string WebResponseContent = "WebResponseContent";
        internal const string WebResponseKnownError = "WebResponseKnownError";
        internal const string WebResponseUnknownError = "WebResponseUnknownError";
        internal const string WebResponseUnknownErrorEmptyBody = "WebResponseUnknownErrorEmptyBody";
        internal const string WebResultNotXml = "WebResultNotXml";
        internal const string WebSchemaNotFound = "WebSchemaNotFound";
        internal const string WebServiceContext = "WebServiceContext";
        internal const string WebServiceDescriptionIgnoredOptional = "WebServiceDescriptionIgnoredOptional";
        internal const string WebServiceDescriptionIgnoredRequired = "WebServiceDescriptionIgnoredRequired";
        internal const string WebServiceServer = "WebServiceServer";
        internal const string WebServiceSession = "WebServiceSession";
        internal const string WebServiceSoapVersion = "WebServiceSoapVersion";
        internal const string WebServiceUser = "WebServiceUser";
        internal const string WebShemaReferenceName = "WebShemaReferenceName";
        internal const string WebSoap11EncodingStyleNotSupported1 = "WebSoap11EncodingStyleNotSupported1";
        internal const string WebSuppressedExceptionMessage = "WebSuppressedExceptionMessage";
        internal const string WebTextMatchBadCaptureIndex = "WebTextMatchBadCaptureIndex";
        internal const string WebTextMatchBadGroupIndex = "WebTextMatchBadGroupIndex";
        internal const string WebTextMatchIgnoredTypeWarning = "WebTextMatchIgnoredTypeWarning";
        internal const string WebTextMatchMissingPattern = "WebTextMatchMissingPattern";
        internal const string WebTimeout = "WebTimeout";
        internal const string WebUnknownAttribute = "WebUnknownAttribute";
        internal const string WebUnknownAttribute2 = "WebUnknownAttribute2";
        internal const string WebUnknownAttribute3 = "WebUnknownAttribute3";
        internal const string WebUnknownElement = "WebUnknownElement";
        internal const string WebUnknownElement1 = "WebUnknownElement1";
        internal const string WebUnknownElement2 = "WebUnknownElement2";
        internal const string WebUnknownEncodingStyle = "WebUnknownEncodingStyle";
        internal const string WebUnrecognizedRequestFormat = "WebUnrecognizedRequestFormat";
        internal const string WebUnrecognizedRequestFormatUrl = "WebUnrecognizedRequestFormatUrl";
        internal const string WebUnreferencedObject = "WebUnreferencedObject";
        internal const string WebVirtualDisoRoot = "WebVirtualDisoRoot";
        internal const string WebWsiContentTypeEncoding = "WebWsiContentTypeEncoding";
        internal const string WebWsiViolation = "WebWsiViolation";
        internal const string WhenUsingAMessageStyleOfParametersAsDocument0 = "WhenUsingAMessageStyleOfParametersAsDocument0";
        internal const string WireSignature = "WireSignature";
        internal const string WireSignatureEmpty = "WireSignatureEmpty";
        internal const string WsdlGenRpcLitAccessorNamespace = "WsdlGenRpcLitAccessorNamespace";
        internal const string WsdlGenRpcLitAnonimousType = "WsdlGenRpcLitAnonimousType";
        internal const string WsdlInstanceValidation = "WsdlInstanceValidation";
        internal const string WsdlInstanceValidationDetails = "WsdlInstanceValidationDetails";
        internal const string XmlLang = "XmlLang";
        internal const string XmlSchema = "XmlSchema";
        internal const string XmlSchemaAttributeReference = "XmlSchemaAttributeReference";
        internal const string XmlSchemaContentDef = "XmlSchemaContentDef";
        internal const string XmlSchemaElementReference = "XmlSchemaElementReference";
        internal const string XmlSchemaItem = "XmlSchemaItem";
        internal const string XmlSchemaNamedItem = "XmlSchemaNamedItem";

        internal Res()
        {
            this.resources = new ResourceManager("System.Web.Services", base.GetType().Assembly);
        }

        private static Res GetLoader()
        {
            if (loader == null)
            {
                Res res = new Res();
                Interlocked.CompareExchange<Res>(ref loader, res, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            Res loader = GetLoader();
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

