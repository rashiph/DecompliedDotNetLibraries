namespace System.Runtime.Serialization
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class SR
    {
        internal const string AbstractElementNotSupported = "AbstractElementNotSupported";
        internal const string AbstractTypeNotSupported = "AbstractTypeNotSupported";
        internal const string AmbiguousReferencedCollectionTypes1 = "AmbiguousReferencedCollectionTypes1";
        internal const string AmbiguousReferencedCollectionTypes3 = "AmbiguousReferencedCollectionTypes3";
        internal const string AmbiguousReferencedTypes1 = "AmbiguousReferencedTypes1";
        internal const string AmbiguousReferencedTypes3 = "AmbiguousReferencedTypes3";
        internal const string AnnotationAttributeNotFound = "AnnotationAttributeNotFound";
        internal const string AnonymousTypeNotSupported = "AnonymousTypeNotSupported";
        internal const string AnyAttributeNotSupported = "AnyAttributeNotSupported";
        internal const string ArrayExceededSize = "ArrayExceededSize";
        internal const string ArrayExceededSizeAttribute = "ArrayExceededSizeAttribute";
        internal const string ArrayItemFormMustBe = "ArrayItemFormMustBe";
        internal const string ArraySizeAttributeIncorrect = "ArraySizeAttributeIncorrect";
        internal const string ArraySizeXmlMismatch = "ArraySizeXmlMismatch";
        internal const string ArrayTypeCannotBeImported = "ArrayTypeCannotBeImported";
        internal const string ArrayTypeIsNotSupported = "ArrayTypeIsNotSupported";
        internal const string AssemblyNotFound = "AssemblyNotFound";
        internal const string AttributedTypesCannotInheritFromNonAttributedSerializableTypes = "AttributedTypesCannotInheritFromNonAttributedSerializableTypes";
        internal const string AttributeNotFound = "AttributeNotFound";
        internal const string BaseTypeNotISerializable = "BaseTypeNotISerializable";
        internal const string CallbackMustReturnVoid = "CallbackMustReturnVoid";
        internal const string CallbackParameterInvalid = "CallbackParameterInvalid";
        internal const string CallbacksCannotBeVirtualMethods = "CallbacksCannotBeVirtualMethods";
        internal const string CannotComputeUniqueName = "CannotComputeUniqueName";
        internal const string CannotDeriveFromSealedReferenceType = "CannotDeriveFromSealedReferenceType";
        internal const string CannotDeserializeRefAtTopLevel = "CannotDeserializeRefAtTopLevel";
        internal const string CannotExportNullAssembly = "CannotExportNullAssembly";
        internal const string CannotExportNullKnownType = "CannotExportNullKnownType";
        internal const string CannotExportNullType = "CannotExportNullType";
        internal const string CannotHaveDuplicateAttributeNames = "CannotHaveDuplicateAttributeNames";
        internal const string CannotHaveDuplicateElementNames = "CannotHaveDuplicateElementNames";
        internal const string CannotImportInvalidSchemas = "CannotImportInvalidSchemas";
        internal const string CannotImportNullDataContractName = "CannotImportNullDataContractName";
        internal const string CannotImportNullSchema = "CannotImportNullSchema";
        internal const string CannotLoadMemberType = "CannotLoadMemberType";
        internal const string CannotSerializeObjectWithCycles = "CannotSerializeObjectWithCycles";
        internal const string CannotSetMembersForReferencedType = "CannotSetMembersForReferencedType";
        internal const string CannotSetNamespaceForReferencedType = "CannotSetNamespaceForReferencedType";
        internal const string CannotUseGenericTypeAsBase = "CannotUseGenericTypeAsBase";
        internal const string CanOnlyStoreIntoArgOrLocGot0 = "CanOnlyStoreIntoArgOrLocGot0";
        internal const string ChangingFullTypeNameNotSupported = "ChangingFullTypeNameNotSupported";
        internal const string CharIsInvalidPrimitive = "CharIsInvalidPrimitive";
        internal const string CircularTypeReference = "CircularTypeReference";
        internal const string ClassDataContractReturnedForGetOnlyCollection = "ClassDataContractReturnedForGetOnlyCollection";
        internal const string CLRNamespaceMappedMultipleTimes = "CLRNamespaceMappedMultipleTimes";
        internal const string ClrTypeNotFound = "ClrTypeNotFound";
        internal const string CollectionAssignedToIncompatibleInterface = "CollectionAssignedToIncompatibleInterface";
        internal const string CollectionMustHaveAddMethod = "CollectionMustHaveAddMethod";
        internal const string CollectionMustHaveGetEnumeratorMethod = "CollectionMustHaveGetEnumeratorMethod";
        internal const string CollectionMustHaveItemType = "CollectionMustHaveItemType";
        internal const string CollectionTypeCannotBeBuiltIn = "CollectionTypeCannotBeBuiltIn";
        internal const string CollectionTypeCannotHaveDataContract = "CollectionTypeCannotHaveDataContract";
        internal const string CollectionTypeDoesNotHaveAddMethod = "CollectionTypeDoesNotHaveAddMethod";
        internal const string CollectionTypeDoesNotHaveDefaultCtor = "CollectionTypeDoesNotHaveDefaultCtor";
        internal const string CollectionTypeHasMultipleDefinitionsOfInterface = "CollectionTypeHasMultipleDefinitionsOfInterface";
        internal const string CollectionTypeIsNotIEnumerable = "CollectionTypeIsNotIEnumerable";
        internal const string CombinedPrefixNSLength = "CombinedPrefixNSLength";
        internal const string ComplexTypeRestrictionNotSupported = "ComplexTypeRestrictionNotSupported";
        internal const string ConfigDataContractSerializerSectionLoadError = "ConfigDataContractSerializerSectionLoadError";
        internal const string ConfigIndexOutOfRange = "ConfigIndexOutOfRange";
        internal const string ConfigMustOnlyAddParamsWithType = "ConfigMustOnlyAddParamsWithType";
        internal const string ConfigMustOnlySetTypeOrIndex = "ConfigMustOnlySetTypeOrIndex";
        internal const string ConfigMustSetTypeOrIndex = "ConfigMustSetTypeOrIndex";
        internal const string CouldNotReadSerializationSchema = "CouldNotReadSerializationSchema";
        internal const string DataContractCacheOverflow = "DataContractCacheOverflow";
        internal const string DataContractNamespaceAlreadySet = "DataContractNamespaceAlreadySet";
        internal const string DataContractNamespaceIsNotValid = "DataContractNamespaceIsNotValid";
        internal const string DataContractNamespaceReserved = "DataContractNamespaceReserved";
        internal const string DataMemberOnEnumField = "DataMemberOnEnumField";
        internal const string DcTypeNotFoundOnDeserialize = "DcTypeNotFoundOnDeserialize";
        internal const string DcTypeNotFoundOnSerialize = "DcTypeNotFoundOnSerialize";
        internal const string DcTypeNotResolvedOnDeserialize = "DcTypeNotResolvedOnDeserialize";
        internal const string DefaultOnElementNotSupported = "DefaultOnElementNotSupported";
        internal const string DerivedTypeNotISerializable = "DerivedTypeNotISerializable";
        internal const string DeserializedObjectWithIdNotFound = "DeserializedObjectWithIdNotFound";
        internal const string DupContractInDataContractSet = "DupContractInDataContractSet";
        internal const string DupContractInKnownTypes = "DupContractInKnownTypes";
        internal const string DupEnumMemberValue = "DupEnumMemberValue";
        internal const string DupKeyValueName = "DupKeyValueName";
        internal const string DuplicateAttribute = "DuplicateAttribute";
        internal const string DuplicateCallback = "DuplicateCallback";
        internal const string DuplicateExtensionDataSetMethod = "DuplicateExtensionDataSetMethod";
        internal const string DupMemberName = "DupMemberName";
        internal const string DupTypeContractInDataContractSet = "DupTypeContractInDataContractSet";
        internal const string ElementMaxOccursMustBe = "ElementMaxOccursMustBe";
        internal const string ElementMinOccursMustBe = "ElementMinOccursMustBe";
        internal const string ElementRefOnLocalElementNotSupported = "ElementRefOnLocalElementNotSupported";
        internal const string EncounteredWithNameNamespace = "EncounteredWithNameNamespace";
        internal const string EnumEnumerationFacetsMustHaveValue = "EnumEnumerationFacetsMustHaveValue";
        internal const string EnumListInAnonymousTypeNotSupported = "EnumListInAnonymousTypeNotSupported";
        internal const string EnumListMustContainAnonymousType = "EnumListMustContainAnonymousType";
        internal const string EnumOnlyEnumerationFacetsSupported = "EnumOnlyEnumerationFacetsSupported";
        internal const string EnumRestrictionInvalid = "EnumRestrictionInvalid";
        internal const string EnumTypeCannotBeImported = "EnumTypeCannotBeImported";
        internal const string EnumTypeCannotHaveIsReference = "EnumTypeCannotHaveIsReference";
        internal const string EnumTypeNotSupportedByDataContractJsonSerializer = "EnumTypeNotSupportedByDataContractJsonSerializer";
        internal const string EnumUnionInAnonymousTypeNotSupported = "EnumUnionInAnonymousTypeNotSupported";
        internal const string ErrorDeserializing = "ErrorDeserializing";
        internal const string ErrorInLine = "ErrorInLine";
        internal const string ErrorIsStartObject = "ErrorIsStartObject";
        internal const string ErrorSerializing = "ErrorSerializing";
        internal const string ErrorTypeInfo = "ErrorTypeInfo";
        internal const string ErrorWriteEndObject = "ErrorWriteEndObject";
        internal const string ErrorWriteStartObject = "ErrorWriteStartObject";
        internal const string ExceededMaxItemsQuota = "ExceededMaxItemsQuota";
        internal const string ExpectingElement = "ExpectingElement";
        internal const string ExpectingElementAtDeserialize = "ExpectingElementAtDeserialize";
        internal const string ExpectingEnd = "ExpectingEnd";
        internal const string ExpectingState = "ExpectingState";
        internal const string ExtensionDataSetMustReturnVoid = "ExtensionDataSetMustReturnVoid";
        internal const string ExtensionDataSetParameterInvalid = "ExtensionDataSetParameterInvalid";
        internal const string FactoryObjectContainsSelfReference = "FactoryObjectContainsSelfReference";
        internal const string FactoryTypeNotISerializable = "FactoryTypeNotISerializable";
        internal const string FixedOnElementNotSupported = "FixedOnElementNotSupported";
        internal const string FormMustBeQualified = "FormMustBeQualified";
        internal const string GenericAnnotationAttributeNotFound = "GenericAnnotationAttributeNotFound";
        internal const string GenericAnnotationForNestedLevelMustBeIncreasing = "GenericAnnotationForNestedLevelMustBeIncreasing";
        internal const string GenericAnnotationHasInvalidAttributeValue = "GenericAnnotationHasInvalidAttributeValue";
        internal const string GenericAnnotationHasInvalidElement = "GenericAnnotationHasInvalidElement";
        internal const string GenericNameBraceMismatch = "GenericNameBraceMismatch";
        internal const string GenericParameterNotValid = "GenericParameterNotValid";
        internal const string GenericTypeNameMismatch = "GenericTypeNameMismatch";
        internal const string GenericTypeNotExportable = "GenericTypeNotExportable";
        internal const string GetOnlyCollectionMustHaveAddMethod = "GetOnlyCollectionMustHaveAddMethod";
        internal const string GetRealObjectReturnedNull = "GetRealObjectReturnedNull";
        internal const string InconsistentIsReference = "InconsistentIsReference";
        internal const string IndexedPropertyCannotBeSerialized = "IndexedPropertyCannotBeSerialized";
        internal const string InterfaceTypeCannotBeCreated = "InterfaceTypeCannotBeCreated";
        internal const string InvalidAnnotationExpectingText = "InvalidAnnotationExpectingText";
        internal const string InvalidAssemblyFormat = "InvalidAssemblyFormat";
        internal const string InvalidCharacterEncountered = "InvalidCharacterEncountered";
        internal const string InvalidClassDerivation = "InvalidClassDerivation";
        internal const string InvalidClrNameGeneratedForISerializable = "InvalidClrNameGeneratedForISerializable";
        internal const string InvalidClrNamespaceGeneratedForISerializable = "InvalidClrNamespaceGeneratedForISerializable";
        internal const string InvalidCollectionContractItemName = "InvalidCollectionContractItemName";
        internal const string InvalidCollectionContractKeyName = "InvalidCollectionContractKeyName";
        internal const string InvalidCollectionContractKeyNoDictionary = "InvalidCollectionContractKeyNoDictionary";
        internal const string InvalidCollectionContractName = "InvalidCollectionContractName";
        internal const string InvalidCollectionContractNamespace = "InvalidCollectionContractNamespace";
        internal const string InvalidCollectionContractValueName = "InvalidCollectionContractValueName";
        internal const string InvalidCollectionContractValueNoDictionary = "InvalidCollectionContractValueNoDictionary";
        internal const string InvalidCollectionDataContract = "InvalidCollectionDataContract";
        internal const string InvalidCollectionType = "InvalidCollectionType";
        internal const string InvalidDataContractName = "InvalidDataContractName";
        internal const string InvalidDataContractNamespace = "InvalidDataContractNamespace";
        internal const string InvalidDataMemberName = "InvalidDataMemberName";
        internal const string InvalidDataNode = "InvalidDataNode";
        internal const string InvalidEmitDefaultAnnotation = "InvalidEmitDefaultAnnotation";
        internal const string InvalidEnumBaseType = "InvalidEnumBaseType";
        internal const string InvalidEnumMemberValue = "InvalidEnumMemberValue";
        internal const string InvalidEnumValueOnRead = "InvalidEnumValueOnRead";
        internal const string InvalidEnumValueOnWrite = "InvalidEnumValueOnWrite";
        internal const string InvalidGetSchemaMethod = "InvalidGetSchemaMethod";
        internal const string InvalidGlobalDataContractNamespace = "InvalidGlobalDataContractNamespace";
        internal const string InvalidIdDefinition = "InvalidIdDefinition";
        internal const string InvalidInclusivePrefixListCollection = "InvalidInclusivePrefixListCollection";
        internal const string InvalidISerializableDerivation = "InvalidISerializableDerivation";
        internal const string InvalidKeyValueType = "InvalidKeyValueType";
        internal const string InvalidKeyValueTypeNamespace = "InvalidKeyValueTypeNamespace";
        internal const string InvalidLocalNameEmpty = "InvalidLocalNameEmpty";
        internal const string InvalidMember = "InvalidMember";
        internal const string InvalidNodeType = "InvalidNodeType";
        internal const string InvalidNonNullReturnValueByIsAny = "InvalidNonNullReturnValueByIsAny";
        internal const string InvalidPrimitiveType = "InvalidPrimitiveType";
        internal const string InvalidRefDefinition = "InvalidRefDefinition";
        internal const string InvalidReturnSchemaOnGetSchemaMethod = "InvalidReturnSchemaOnGetSchemaMethod";
        internal const string InvalidReturnTypeOnGetSchemaMethod = "InvalidReturnTypeOnGetSchemaMethod";
        internal const string InvalidSizeDefinition = "InvalidSizeDefinition";
        internal const string InvalidStateInExtensionDataReader = "InvalidStateInExtensionDataReader";
        internal const string InvalidXmlDataContractName = "InvalidXmlDataContractName";
        internal const string InvalidXmlDeserializingExtensionData = "InvalidXmlDeserializingExtensionData";
        internal const string InvalidXmlQualifiedNameValue = "InvalidXmlQualifiedNameValue";
        internal const string InvalidXsIdDefinition = "InvalidXsIdDefinition";
        internal const string InvalidXsRefDefinition = "InvalidXsRefDefinition";
        internal const string IsAnyCannotBeNull = "IsAnyCannotBeNull";
        internal const string IsAnyCannotBeSerializedAsDerivedType = "IsAnyCannotBeSerializedAsDerivedType";
        internal const string IsAnyCannotHaveXmlRoot = "IsAnyCannotHaveXmlRoot";
        internal const string IsAnyNotSupportedByNetDataContractSerializer = "IsAnyNotSupportedByNetDataContractSerializer";
        internal const string IsDictionaryFormattedIncorrectly = "IsDictionaryFormattedIncorrectly";
        internal const string ISerializableCannotHaveDataContract = "ISerializableCannotHaveDataContract";
        internal const string ISerializableContainsMoreThanOneItems = "ISerializableContainsMoreThanOneItems";
        internal const string ISerializableDerivedContainsOneOrMoreItems = "ISerializableDerivedContainsOneOrMoreItems";
        internal const string ISerializableDoesNotContainAny = "ISerializableDoesNotContainAny";
        internal const string ISerializableMustRefFactoryTypeAttribute = "ISerializableMustRefFactoryTypeAttribute";
        internal const string ISerializableTypeCannotBeImported = "ISerializableTypeCannotBeImported";
        internal const string ISerializableWildcardMaxOccursMustBe = "ISerializableWildcardMaxOccursMustBe";
        internal const string ISerializableWildcardMinOccursMustBe = "ISerializableWildcardMinOccursMustBe";
        internal const string ISerializableWildcardNamespaceInvalid = "ISerializableWildcardNamespaceInvalid";
        internal const string ISerializableWildcardProcessContentsInvalid = "ISerializableWildcardProcessContentsInvalid";
        internal const string IsNotAssignableFrom = "IsNotAssignableFrom";
        internal const string IsReferenceGetOnlyCollectionsNotSupported = "IsReferenceGetOnlyCollectionsNotSupported";
        internal const string IsRequiredDataMemberOnIsReferenceDataContractType = "IsRequiredDataMemberOnIsReferenceDataContractType";
        internal const string IsValueTypeFormattedIncorrectly = "IsValueTypeFormattedIncorrectly";
        internal const string IXmlSerializableCannotHaveCollectionDataContract = "IXmlSerializableCannotHaveCollectionDataContract";
        internal const string IXmlSerializableCannotHaveDataContract = "IXmlSerializableCannotHaveDataContract";
        internal const string IXmlSerializableIllegalOperation = "IXmlSerializableIllegalOperation";
        internal const string IXmlSerializableMissingEndElements = "IXmlSerializableMissingEndElements";
        internal const string IXmlSerializableMustHaveDefaultConstructor = "IXmlSerializableMustHaveDefaultConstructor";
        internal const string IXmlSerializableWritePastSubTree = "IXmlSerializableWritePastSubTree";
        internal const string JsonAttributeAlreadyWritten = "JsonAttributeAlreadyWritten";
        internal const string JsonAttributeMustHaveElement = "JsonAttributeMustHaveElement";
        internal const string JsonCannotWriteStandaloneTextAfterQuotedText = "JsonCannotWriteStandaloneTextAfterQuotedText";
        internal const string JsonCannotWriteTextAfterNonTextAttribute = "JsonCannotWriteTextAfterNonTextAttribute";
        internal const string JsonDateTimeOutOfRange = "JsonDateTimeOutOfRange";
        internal const string JsonDuplicateMemberInInput = "JsonDuplicateMemberInInput";
        internal const string JsonDuplicateMemberNames = "JsonDuplicateMemberNames";
        internal const string JsonEncodingNotSupported = "JsonEncodingNotSupported";
        internal const string JsonEncounteredUnexpectedCharacter = "JsonEncounteredUnexpectedCharacter";
        internal const string JsonEndElementNoOpenNodes = "JsonEndElementNoOpenNodes";
        internal const string JsonExpectedEncoding = "JsonExpectedEncoding";
        internal const string JsonInvalidBytes = "JsonInvalidBytes";
        internal const string JsonInvalidDataTypeSpecifiedForServerType = "JsonInvalidDataTypeSpecifiedForServerType";
        internal const string JsonInvalidDateTimeString = "JsonInvalidDateTimeString";
        internal const string JsonInvalidFFFE = "JsonInvalidFFFE";
        internal const string JsonInvalidItemNameForArrayElement = "JsonInvalidItemNameForArrayElement";
        internal const string JsonInvalidLocalNameEmpty = "JsonInvalidLocalNameEmpty";
        internal const string JsonInvalidMethodBetweenStartEndAttribute = "JsonInvalidMethodBetweenStartEndAttribute";
        internal const string JsonInvalidRootElementName = "JsonInvalidRootElementName";
        internal const string JsonInvalidStartElementCall = "JsonInvalidStartElementCall";
        internal const string JsonInvalidWriteStat = "JsonInvalidWriteStat";
        internal const string JsonInvalidWriteState = "JsonInvalidWriteState";
        internal const string JsonMethodNotSupported = "JsonMethodNotSupported";
        internal const string JsonMultipleRootElementsNotAllowedOnWriter = "JsonMultipleRootElementsNotAllowedOnWriter";
        internal const string JsonMustSpecifyDataType = "JsonMustSpecifyDataType";
        internal const string JsonMustUseWriteStringForWritingAttributeValues = "JsonMustUseWriteStringForWritingAttributeValues";
        internal const string JsonNamespaceMustBeEmpty = "JsonNamespaceMustBeEmpty";
        internal const string JsonNestedArraysNotSupported = "JsonNestedArraysNotSupported";
        internal const string JsonNodeTypeArrayOrObjectNotSpecified = "JsonNodeTypeArrayOrObjectNotSpecified";
        internal const string JsonNoMatchingStartAttribute = "JsonNoMatchingStartAttribute";
        internal const string JsonOffsetExceedsBufferSize = "JsonOffsetExceedsBufferSize";
        internal const string JsonOneRequiredMemberNotFound = "JsonOneRequiredMemberNotFound";
        internal const string JsonOnlyWhitespace = "JsonOnlyWhitespace";
        internal const string JsonOpenAttributeMustBeClosedFirst = "JsonOpenAttributeMustBeClosedFirst";
        internal const string JsonPrefixMustBeNullOrEmpty = "JsonPrefixMustBeNullOrEmpty";
        internal const string JsonRequiredMembersNotFound = "JsonRequiredMembersNotFound";
        internal const string JsonServerTypeSpecifiedForInvalidDataType = "JsonServerTypeSpecifiedForInvalidDataType";
        internal const string JsonSizeExceedsRemainingBufferSpace = "JsonSizeExceedsRemainingBufferSpace";
        internal const string JsonTypeNotSupportedByDataContractJsonSerializer = "JsonTypeNotSupportedByDataContractJsonSerializer";
        internal const string JsonUnexpectedAttributeLocalName = "JsonUnexpectedAttributeLocalName";
        internal const string JsonUnexpectedAttributeValue = "JsonUnexpectedAttributeValue";
        internal const string JsonUnexpectedEndOfFile = "JsonUnexpectedEndOfFile";
        internal const string JsonUnsupportedForIsReference = "JsonUnsupportedForIsReference";
        internal const string JsonValueMustBeInRange = "JsonValueMustBeInRange";
        internal const string JsonWriteArrayNotSupported = "JsonWriteArrayNotSupported";
        internal const string JsonWriterClosed = "JsonWriterClosed";
        internal const string JsonXmlInvalidDeclaration = "JsonXmlInvalidDeclaration";
        internal const string JsonXmlProcessingInstructionNotSupported = "JsonXmlProcessingInstructionNotSupported";
        internal const string KnownTypeAttributeEmptyString = "KnownTypeAttributeEmptyString";
        internal const string KnownTypeAttributeMethodNull = "KnownTypeAttributeMethodNull";
        internal const string KnownTypeAttributeNoData = "KnownTypeAttributeNoData";
        internal const string KnownTypeAttributeOneScheme = "KnownTypeAttributeOneScheme";
        internal const string KnownTypeAttributeReturnType = "KnownTypeAttributeReturnType";
        internal const string KnownTypeAttributeUnknownMethod = "KnownTypeAttributeUnknownMethod";
        internal const string KnownTypeAttributeValidMethodTypes = "KnownTypeAttributeValidMethodTypes";
        internal const string KnownTypeConfigClosedGenericDeclared = "KnownTypeConfigClosedGenericDeclared";
        internal const string KnownTypeConfigGenericParamMismatch = "KnownTypeConfigGenericParamMismatch";
        internal const string KnownTypeConfigIndexOutOfBounds = "KnownTypeConfigIndexOutOfBounds";
        internal const string KnownTypeConfigIndexOutOfBoundsZero = "KnownTypeConfigIndexOutOfBoundsZero";
        internal const string KnownTypeConfigObject = "KnownTypeConfigObject";
        private static System.Runtime.Serialization.SR loader;
        internal const string MaxArrayLengthExceeded = "MaxArrayLengthExceeded";
        internal const string MimeContentTypeHeaderInvalid = "MimeContentTypeHeaderInvalid";
        internal const string MimeHeaderInvalidCharacter = "MimeHeaderInvalidCharacter";
        internal const string MimeMessageGetContentStreamCalledAlready = "MimeMessageGetContentStreamCalledAlready";
        internal const string MimeReaderHeaderAlreadyExists = "MimeReaderHeaderAlreadyExists";
        internal const string MimeReaderMalformedHeader = "MimeReaderMalformedHeader";
        internal const string MimeReaderResetCalledBeforeEOF = "MimeReaderResetCalledBeforeEOF";
        internal const string MimeReaderTruncated = "MimeReaderTruncated";
        internal const string MimeVersionHeaderInvalid = "MimeVersionHeaderInvalid";
        internal const string MimeWriterInvalidStateForClose = "MimeWriterInvalidStateForClose";
        internal const string MimeWriterInvalidStateForContent = "MimeWriterInvalidStateForContent";
        internal const string MimeWriterInvalidStateForHeader = "MimeWriterInvalidStateForHeader";
        internal const string MimeWriterInvalidStateForStartPart = "MimeWriterInvalidStateForStartPart";
        internal const string MimeWriterInvalidStateForStartPreface = "MimeWriterInvalidStateForStartPreface";
        internal const string MissingGetSchemaMethod = "MissingGetSchemaMethod";
        internal const string MissingSchemaType = "MissingSchemaType";
        internal const string MixedContentNotSupported = "MixedContentNotSupported";
        internal const string MtomBoundaryInvalid = "MtomBoundaryInvalid";
        internal const string MtomBufferQuotaExceeded = "MtomBufferQuotaExceeded";
        internal const string MtomContentTransferEncodingNotPresent = "MtomContentTransferEncodingNotPresent";
        internal const string MtomContentTransferEncodingNotSupported = "MtomContentTransferEncodingNotSupported";
        internal const string MtomContentTypeInvalid = "MtomContentTypeInvalid";
        internal const string MtomDataMustNotContainXopInclude = "MtomDataMustNotContainXopInclude";
        internal const string MtomExceededMaxSizeInBytes = "MtomExceededMaxSizeInBytes";
        internal const string MtomInvalidCIDUri = "MtomInvalidCIDUri";
        internal const string MtomInvalidEmptyURI = "MtomInvalidEmptyURI";
        internal const string MtomInvalidStartUri = "MtomInvalidStartUri";
        internal const string MtomInvalidTransferEncodingForMimePart = "MtomInvalidTransferEncodingForMimePart";
        internal const string MtomMessageContentTypeNotFound = "MtomMessageContentTypeNotFound";
        internal const string MtomMessageInvalidContent = "MtomMessageInvalidContent";
        internal const string MtomMessageInvalidContentInMimePart = "MtomMessageInvalidContentInMimePart";
        internal const string MtomMessageInvalidMimeVersion = "MtomMessageInvalidMimeVersion";
        internal const string MtomMessageNotApplicationXopXml = "MtomMessageNotApplicationXopXml";
        internal const string MtomMessageNotMultipart = "MtomMessageNotMultipart";
        internal const string MtomMessageRequiredParamNotSpecified = "MtomMessageRequiredParamNotSpecified";
        internal const string MtomMimePartReferencedMoreThanOnce = "MtomMimePartReferencedMoreThanOnce";
        internal const string MtomPartNotFound = "MtomPartNotFound";
        internal const string MtomRootContentTypeNotFound = "MtomRootContentTypeNotFound";
        internal const string MtomRootNotApplicationXopXml = "MtomRootNotApplicationXopXml";
        internal const string MtomRootPartNotFound = "MtomRootPartNotFound";
        internal const string MtomRootRequiredParamNotSpecified = "MtomRootRequiredParamNotSpecified";
        internal const string MtomRootUnexpectedCharset = "MtomRootUnexpectedCharset";
        internal const string MtomRootUnexpectedType = "MtomRootUnexpectedType";
        internal const string MtomXopIncludeHrefNotSpecified = "MtomXopIncludeHrefNotSpecified";
        internal const string MtomXopIncludeInvalidXopAttributes = "MtomXopIncludeInvalidXopAttributes";
        internal const string MtomXopIncludeInvalidXopElement = "MtomXopIncludeInvalidXopElement";
        internal const string MultipleIdDefinition = "MultipleIdDefinition";
        internal const string MustContainOnlyLocalElements = "MustContainOnlyLocalElements";
        internal const string NameCannotBeNullOrEmpty = "NameCannotBeNullOrEmpty";
        internal const string NoConversionPossibleTo = "NoConversionPossibleTo";
        internal const string NoGetMethodForProperty = "NoGetMethodForProperty";
        internal const string NonAttributedSerializableTypesMustHaveDefaultConstructor = "NonAttributedSerializableTypesMustHaveDefaultConstructor";
        internal const string NonOptionalFieldMemberOnIsReferenceSerializableType = "NonOptionalFieldMemberOnIsReferenceSerializableType";
        internal const string NoSetMethodForProperty = "NoSetMethodForProperty";
        internal const string NullKnownType = "NullKnownType";
        internal const string NullValueReturnedForGetOnlyCollection = "NullValueReturnedForGetOnlyCollection";
        internal const string ObjectTableOverflow = "ObjectTableOverflow";
        internal const string OffsetExceedsBufferSize = "OffsetExceedsBufferSize";
        internal const string OnlyDataContractTypesCanHaveExtensionData = "OnlyDataContractTypesCanHaveExtensionData";
        internal const string OrderCannotBeNegative = "OrderCannotBeNegative";
        internal const string OutParametersMustBeByRefTypeReceived = "OutParametersMustBeByRefTypeReceived";
        internal const string ParameterCountMismatch = "ParameterCountMismatch";
        internal const string PartialTrustCollectionContractAddMethodNotPublic = "PartialTrustCollectionContractAddMethodNotPublic";
        internal const string PartialTrustCollectionContractNoPublicConstructor = "PartialTrustCollectionContractNoPublicConstructor";
        internal const string PartialTrustCollectionContractTypeNotPublic = "PartialTrustCollectionContractTypeNotPublic";
        internal const string PartialTrustDataContractFieldGetNotPublic = "PartialTrustDataContractFieldGetNotPublic";
        internal const string PartialTrustDataContractFieldSetNotPublic = "PartialTrustDataContractFieldSetNotPublic";
        internal const string PartialTrustDataContractOnDeserializedNotPublic = "PartialTrustDataContractOnDeserializedNotPublic";
        internal const string PartialTrustDataContractOnDeserializingNotPublic = "PartialTrustDataContractOnDeserializingNotPublic";
        internal const string PartialTrustDataContractOnSerializedNotPublic = "PartialTrustDataContractOnSerializedNotPublic";
        internal const string PartialTrustDataContractOnSerializingNotPublic = "PartialTrustDataContractOnSerializingNotPublic";
        internal const string PartialTrustDataContractPropertyGetNotPublic = "PartialTrustDataContractPropertyGetNotPublic";
        internal const string PartialTrustDataContractPropertySetNotPublic = "PartialTrustDataContractPropertySetNotPublic";
        internal const string PartialTrustDataContractTypeNotPublic = "PartialTrustDataContractTypeNotPublic";
        internal const string PartialTrustISerializableNoPublicConstructor = "PartialTrustISerializableNoPublicConstructor";
        internal const string PartialTrustIXmlSerializableTypeNotPublic = "PartialTrustIXmlSerializableTypeNotPublic";
        internal const string PartialTrustIXmlSerialzableNoPublicConstructor = "PartialTrustIXmlSerialzableNoPublicConstructor";
        internal const string PartialTrustNonAttributedSerializableTypeNoPublicConstructor = "PartialTrustNonAttributedSerializableTypeNoPublicConstructor";
        internal const string QueryGeneratorPathToMemberNotFound = "QueryGeneratorPathToMemberNotFound";
        internal const string QuotaCopyReadOnly = "QuotaCopyReadOnly";
        internal const string QuotaIsReadOnly = "QuotaIsReadOnly";
        internal const string QuotaMustBePositive = "QuotaMustBePositive";
        internal const string ReadNotSupportedOnStream = "ReadNotSupportedOnStream";
        internal const string RecursiveCollectionType = "RecursiveCollectionType";
        internal const string RedefineNotSupported = "RedefineNotSupported";
        internal const string ReferencedBaseTypeDoesNotExist = "ReferencedBaseTypeDoesNotExist";
        internal const string ReferencedCollectionTypesCannotContainNull = "ReferencedCollectionTypesCannotContainNull";
        internal const string ReferencedTypeDoesNotMatch = "ReferencedTypeDoesNotMatch";
        internal const string ReferencedTypeMatchingMessage = "ReferencedTypeMatchingMessage";
        internal const string ReferencedTypeNotMatchingMessage = "ReferencedTypeNotMatchingMessage";
        internal const string ReferencedTypesCannotContainNull = "ReferencedTypesCannotContainNull";
        internal const string RequiredMemberMustBeEmitted = "RequiredMemberMustBeEmitted";
        internal const string RequiresClassDataContractToSetIsISerializable = "RequiresClassDataContractToSetIsISerializable";
        internal const string ResolveTypeReturnedFalse = "ResolveTypeReturnedFalse";
        internal const string ResolveTypeReturnedNull = "ResolveTypeReturnedNull";
        private ResourceManager resources;
        internal const string RootParticleMustBeSequence = "RootParticleMustBeSequence";
        internal const string RootSequenceMaxOccursMustBe = "RootSequenceMaxOccursMustBe";
        internal const string RootSequenceMustBeRequired = "RootSequenceMustBeRequired";
        internal const string SeekNotSupportedOnStream = "SeekNotSupportedOnStream";
        internal const string SerializationInfo_ConstructorNotFound = "SerializationInfo_ConstructorNotFound";
        internal const string SimpleContentNotSupported = "SimpleContentNotSupported";
        internal const string SimpleTypeRestrictionDoesNotSpecifyBase = "SimpleTypeRestrictionDoesNotSpecifyBase";
        internal const string SimpleTypeUnionNotSupported = "SimpleTypeUnionNotSupported";
        internal const string SizeExceedsRemainingBufferSpace = "SizeExceedsRemainingBufferSpace";
        internal const string SpecifiedTypeNotFoundInSchema = "SpecifiedTypeNotFoundInSchema";
        internal const string SubstitutionGroupOnElementNotSupported = "SubstitutionGroupOnElementNotSupported";
        internal const string SupportForMultidimensionalArraysNotPresent = "SupportForMultidimensionalArraysNotPresent";
        internal const string SurrogatesWithGetOnlyCollectionsNotSupported = "SurrogatesWithGetOnlyCollectionsNotSupported";
        internal const string SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser = "SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser";
        internal const string TooManyCollectionContracts = "TooManyCollectionContracts";
        internal const string TooManyDataContracts = "TooManyDataContracts";
        internal const string TooManyDataMembers = "TooManyDataMembers";
        internal const string TooManyEnumMembers = "TooManyEnumMembers";
        internal const string TooManyIgnoreDataMemberAttributes = "TooManyIgnoreDataMemberAttributes";
        internal const string TopLevelElementRepresentsDifferentType = "TopLevelElementRepresentsDifferentType";
        internal const string TraceCodeElementIgnored = "TraceCodeElementIgnored";
        internal const string TraceCodeFactoryTypeNotFound = "TraceCodeFactoryTypeNotFound";
        internal const string TraceCodeObjectWithLargeDepth = "TraceCodeObjectWithLargeDepth";
        internal const string TraceCodeReadObjectBegin = "TraceCodeReadObjectBegin";
        internal const string TraceCodeReadObjectEnd = "TraceCodeReadObjectEnd";
        internal const string TraceCodeWriteObjectBegin = "TraceCodeWriteObjectBegin";
        internal const string TraceCodeWriteObjectContentBegin = "TraceCodeWriteObjectContentBegin";
        internal const string TraceCodeWriteObjectContentEnd = "TraceCodeWriteObjectContentEnd";
        internal const string TraceCodeWriteObjectEnd = "TraceCodeWriteObjectEnd";
        internal const string TraceCodeXsdExportAnnotationFailed = "TraceCodeXsdExportAnnotationFailed";
        internal const string TraceCodeXsdExportBegin = "TraceCodeXsdExportBegin";
        internal const string TraceCodeXsdExportDupItems = "TraceCodeXsdExportDupItems";
        internal const string TraceCodeXsdExportEnd = "TraceCodeXsdExportEnd";
        internal const string TraceCodeXsdExportError = "TraceCodeXsdExportError";
        internal const string TraceCodeXsdImportAnnotationFailed = "TraceCodeXsdImportAnnotationFailed";
        internal const string TraceCodeXsdImportBegin = "TraceCodeXsdImportBegin";
        internal const string TraceCodeXsdImportEnd = "TraceCodeXsdImportEnd";
        internal const string TraceCodeXsdImportError = "TraceCodeXsdImportError";
        internal const string TypeCannotBeImported = "TypeCannotBeImported";
        internal const string TypeCannotBeImportedHowToFix = "TypeCannotBeImportedHowToFix";
        internal const string TypeHasNotBeenImported = "TypeHasNotBeenImported";
        internal const string TypeMustBeConcrete = "TypeMustBeConcrete";
        internal const string TypeMustBeIXmlSerializable = "TypeMustBeIXmlSerializable";
        internal const string TypeMustNotBeOpenGeneric = "TypeMustNotBeOpenGeneric";
        internal const string TypeNotSerializable = "TypeNotSerializable";
        internal const string TypeShouldNotContainAttributes = "TypeShouldNotContainAttributes";
        internal const string UnexpectedContractType = "UnexpectedContractType";
        internal const string UnexpectedElementExpectingElements = "UnexpectedElementExpectingElements";
        internal const string UnexpectedEndOfFile = "UnexpectedEndOfFile";
        internal const string UnknownConstantType = "UnknownConstantType";
        internal const string UnknownXmlType = "UnknownXmlType";
        internal const string ValueMustBeInRange = "ValueMustBeInRange";
        internal const string ValueMustBeNonNegative = "ValueMustBeNonNegative";
        internal const string ValueTypeCannotBeNull = "ValueTypeCannotBeNull";
        internal const string ValueTypeCannotHaveBaseType = "ValueTypeCannotHaveBaseType";
        internal const string ValueTypeCannotHaveId = "ValueTypeCannotHaveId";
        internal const string ValueTypeCannotHaveIsReference = "ValueTypeCannotHaveIsReference";
        internal const string ValueTypeCannotHaveRef = "ValueTypeCannotHaveRef";
        internal const string WriteBufferOverflow = "WriteBufferOverflow";
        internal const string WriteNotSupportedOnStream = "WriteNotSupportedOnStream";
        internal const string XmlArrayTooSmall = "XmlArrayTooSmall";
        internal const string XmlArrayTooSmallInput = "XmlArrayTooSmallInput";
        internal const string XmlArrayTooSmallOutput = "XmlArrayTooSmallOutput";
        internal const string XmlBadBOM = "XmlBadBOM";
        internal const string XmlBase64DataExpected = "XmlBase64DataExpected";
        internal const string XmlCanonicalizationNotStarted = "XmlCanonicalizationNotStarted";
        internal const string XmlCanonicalizationStarted = "XmlCanonicalizationStarted";
        internal const string XmlCDATAInvalidAtTopLevel = "XmlCDATAInvalidAtTopLevel";
        internal const string XmlCloseCData = "XmlCloseCData";
        internal const string XmlConversionOverflow = "XmlConversionOverflow";
        internal const string XmlDeclarationRequired = "XmlDeclarationRequired";
        internal const string XmlDeclMissing = "XmlDeclMissing";
        internal const string XmlDeclMissingVersion = "XmlDeclMissingVersion";
        internal const string XmlDeclNotFirst = "XmlDeclNotFirst";
        internal const string XmlDictionaryStringIDRange = "XmlDictionaryStringIDRange";
        internal const string XmlDictionaryStringIDUndefinedSession = "XmlDictionaryStringIDUndefinedSession";
        internal const string XmlDictionaryStringIDUndefinedStatic = "XmlDictionaryStringIDUndefinedStatic";
        internal const string XmlDuplicateAttribute = "XmlDuplicateAttribute";
        internal const string XmlElementAttributes = "XmlElementAttributes";
        internal const string XmlEmptyNamespaceRequiresNullPrefix = "XmlEmptyNamespaceRequiresNullPrefix";
        internal const string XmlEncodingMismatch = "XmlEncodingMismatch";
        internal const string XmlEncodingNotSupported = "XmlEncodingNotSupported";
        internal const string XmlEndElementExpected = "XmlEndElementExpected";
        internal const string XmlEndElementNoOpenNodes = "XmlEndElementNoOpenNodes";
        internal const string XmlExpectedEncoding = "XmlExpectedEncoding";
        internal const string XmlForObjectCannotHaveContent = "XmlForObjectCannotHaveContent";
        internal const string XmlFoundCData = "XmlFoundCData";
        internal const string XmlFoundComment = "XmlFoundComment";
        internal const string XmlFoundElement = "XmlFoundElement";
        internal const string XmlFoundEndElement = "XmlFoundEndElement";
        internal const string XmlFoundEndOfFile = "XmlFoundEndOfFile";
        internal const string XmlFoundNodeType = "XmlFoundNodeType";
        internal const string XmlFoundText = "XmlFoundText";
        internal const string XmlFullStartElementExpected = "XmlFullStartElementExpected";
        internal const string XmlFullStartElementLocalNameNsExpected = "XmlFullStartElementLocalNameNsExpected";
        internal const string XmlFullStartElementNameExpected = "XmlFullStartElementNameExpected";
        internal const string XmlIDDefined = "XmlIDDefined";
        internal const string XmlIllegalOutsideRoot = "XmlIllegalOutsideRoot";
        internal const string XmlInvalidBase64Length = "XmlInvalidBase64Length";
        internal const string XmlInvalidBase64Sequence = "XmlInvalidBase64Sequence";
        internal const string XmlInvalidBinHexLength = "XmlInvalidBinHexLength";
        internal const string XmlInvalidBinHexSequence = "XmlInvalidBinHexSequence";
        internal const string XmlInvalidBytes = "XmlInvalidBytes";
        internal const string XmlInvalidCharRef = "XmlInvalidCharRef";
        internal const string XmlInvalidCommentChars = "XmlInvalidCommentChars";
        internal const string XmlInvalidConversion = "XmlInvalidConversion";
        internal const string XmlInvalidDeclaration = "XmlInvalidDeclaration";
        internal const string XmlInvalidDepth = "XmlInvalidDepth";
        internal const string XmlInvalidEncoding = "XmlInvalidEncoding";
        internal const string XmlInvalidFFFE = "XmlInvalidFFFE";
        internal const string XmlInvalidFormat = "XmlInvalidFormat";
        internal const string XmlInvalidHighSurrogate = "XmlInvalidHighSurrogate";
        internal const string XmlInvalidID = "XmlInvalidID";
        internal const string XmlInvalidLowSurrogate = "XmlInvalidLowSurrogate";
        internal const string XmlInvalidOperation = "XmlInvalidOperation";
        internal const string XmlInvalidPrefixState = "XmlInvalidPrefixState";
        internal const string XmlInvalidQualifiedName = "XmlInvalidQualifiedName";
        internal const string XmlInvalidRootData = "XmlInvalidRootData";
        internal const string XmlInvalidStandalone = "XmlInvalidStandalone";
        internal const string XmlInvalidStream = "XmlInvalidStream";
        internal const string XmlInvalidSurrogate = "XmlInvalidSurrogate";
        internal const string XmlInvalidUniqueId = "XmlInvalidUniqueId";
        internal const string XmlInvalidUTF8Bytes = "XmlInvalidUTF8Bytes";
        internal const string XmlInvalidVersion = "XmlInvalidVersion";
        internal const string XmlInvalidWriteState = "XmlInvalidWriteState";
        internal const string XmlInvalidXmlByte = "XmlInvalidXmlByte";
        internal const string XmlInvalidXmlSpace = "XmlInvalidXmlSpace";
        internal const string XmlKeyAlreadyExists = "XmlKeyAlreadyExists";
        internal const string XmlLineInfo = "XmlLineInfo";
        internal const string XmlMalformedDecl = "XmlMalformedDecl";
        internal const string XmlMaxArrayLengthExceeded = "XmlMaxArrayLengthExceeded";
        internal const string XmlMaxArrayLengthOrMaxItemsQuotaExceeded = "XmlMaxArrayLengthOrMaxItemsQuotaExceeded";
        internal const string XmlMaxBytesPerReadExceeded = "XmlMaxBytesPerReadExceeded";
        internal const string XmlMaxDepthExceeded = "XmlMaxDepthExceeded";
        internal const string XmlMaxNameTableCharCountExceeded = "XmlMaxNameTableCharCountExceeded";
        internal const string XmlMaxStringContentLengthExceeded = "XmlMaxStringContentLengthExceeded";
        internal const string XmlMethodNotSupported = "XmlMethodNotSupported";
        internal const string XmlMissingLowSurrogate = "XmlMissingLowSurrogate";
        internal const string XmlMultipleRootElements = "XmlMultipleRootElements";
        internal const string XmlNamespaceNotFound = "XmlNamespaceNotFound";
        internal const string XmlNestedArraysNotSupported = "XmlNestedArraysNotSupported";
        internal const string XmlNoRootElement = "XmlNoRootElement";
        internal const string XmlObjectAssignedToIncompatibleInterface = "XmlObjectAssignedToIncompatibleInterface";
        internal const string XmlOnlyOneRoot = "XmlOnlyOneRoot";
        internal const string XmlOnlySingleValue = "XmlOnlySingleValue";
        internal const string XmlOnlyWhitespace = "XmlOnlyWhitespace";
        internal const string XmlPrefixBoundToNamespace = "XmlPrefixBoundToNamespace";
        internal const string XmlProcessingInstructionNotSupported = "XmlProcessingInstructionNotSupported";
        internal const string XmlReservedPrefix = "XmlReservedPrefix";
        internal const string XmlSpaceBetweenAttributes = "XmlSpaceBetweenAttributes";
        internal const string XmlSpecificBindingNamespace = "XmlSpecificBindingNamespace";
        internal const string XmlSpecificBindingPrefix = "XmlSpecificBindingPrefix";
        internal const string XmlStartElementExpected = "XmlStartElementExpected";
        internal const string XmlStartElementLocalNameNsExpected = "XmlStartElementLocalNameNsExpected";
        internal const string XmlStartElementNameExpected = "XmlStartElementNameExpected";
        internal const string XmlTagMismatch = "XmlTagMismatch";
        internal const string XmlTokenExpected = "XmlTokenExpected";
        internal const string XmlUndefinedPrefix = "XmlUndefinedPrefix";
        internal const string XmlUnexpectedEndElement = "XmlUnexpectedEndElement";
        internal const string XmlUnexpectedEndOfFile = "XmlUnexpectedEndOfFile";
        internal const string XmlWriteFunctionInvalid = "XmlWriteFunctionInvalid";
        internal const string XmlWriterClosed = "XmlWriterClosed";
        internal const string XmlWriterMustBeInElement = "XmlWriterMustBeInElement";

        internal SR()
        {
            this.resources = new ResourceManager("System.Runtime.Serialization", base.GetType().Assembly);
        }

        private static System.Runtime.Serialization.SR GetLoader()
        {
            if (loader == null)
            {
                System.Runtime.Serialization.SR sr = new System.Runtime.Serialization.SR();
                Interlocked.CompareExchange<System.Runtime.Serialization.SR>(ref loader, sr, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            System.Runtime.Serialization.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            System.Runtime.Serialization.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            System.Runtime.Serialization.SR loader = GetLoader();
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

