namespace System
{
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class SR
    {
        internal const string Arg_ArrayPlusOffTooSmall = "Arg_ArrayPlusOffTooSmall";
        internal const string Arg_HSCapacityOverflow = "Arg_HSCapacityOverflow";
        internal const string Argument_EmptyFile = "Argument_EmptyFile";
        internal const string Argument_EmptyServerName = "Argument_EmptyServerName";
        internal const string Argument_InvalidHandle = "Argument_InvalidHandle";
        internal const string Argument_InvalidOffLen = "Argument_InvalidOffLen";
        internal const string Argument_MapNameEmptyString = "Argument_MapNameEmptyString";
        internal const string Argument_NeedNonemptyDelimiter = "Argument_NeedNonemptyDelimiter";
        internal const string Argument_NeedNonemptyPipeName = "Argument_NeedNonemptyPipeName";
        internal const string Argument_NewMMFAppendModeNotAllowed = "Argument_NewMMFAppendModeNotAllowed";
        internal const string Argument_NewMMFWriteAccessNotAllowed = "Argument_NewMMFWriteAccessNotAllowed";
        internal const string Argument_NonContainerInvalidAnyFlag = "Argument_NonContainerInvalidAnyFlag";
        internal const string Argument_ReadAccessWithLargeCapacity = "Argument_ReadAccessWithLargeCapacity";
        internal const string Argument_WrongAsyncResult = "Argument_WrongAsyncResult";
        internal const string ArgumentException_CountMaxLengthSmallerThanMinLength = "ArgumentException_CountMaxLengthSmallerThanMinLength";
        internal const string ArgumentException_DuplicateName = "ArgumentException_DuplicateName";
        internal const string ArgumentException_DuplicateParameterAttribute = "ArgumentException_DuplicateParameterAttribute";
        internal const string ArgumentException_DuplicatePosition = "ArgumentException_DuplicatePosition";
        internal const string ArgumentException_DuplicateRemainingArgumets = "ArgumentException_DuplicateRemainingArgumets";
        internal const string ArgumentException_HelpMessageBaseNameNullOrEmpty = "ArgumentException_HelpMessageBaseNameNullOrEmpty";
        internal const string ArgumentException_HelpMessageNullOrEmpty = "ArgumentException_HelpMessageNullOrEmpty";
        internal const string ArgumentException_HelpMessageResourceIdNullOrEmpty = "ArgumentException_HelpMessageResourceIdNullOrEmpty";
        internal const string ArgumentException_InvalidParameterName = "ArgumentException_InvalidParameterName";
        internal const string ArgumentException_LengthMaxLengthSmallerThanMinLength = "ArgumentException_LengthMaxLengthSmallerThanMinLength";
        internal const string ArgumentException_MissingBaseNameOrResourceId = "ArgumentException_MissingBaseNameOrResourceId";
        internal const string ArgumentException_NoParametersFound = "ArgumentException_NoParametersFound";
        internal const string ArgumentException_ParserBuiltWithValueType = "ArgumentException_ParserBuiltWithValueType";
        internal const string ArgumentException_RangeMaxRangeSmallerThanMinRange = "ArgumentException_RangeMaxRangeSmallerThanMinRange";
        internal const string ArgumentException_RangeMinRangeMaxRangeType = "ArgumentException_RangeMinRangeMaxRangeType";
        internal const string ArgumentException_RangeNotIComparable = "ArgumentException_RangeNotIComparable";
        internal const string ArgumentException_RegexPatternNullOrEmpty = "ArgumentException_RegexPatternNullOrEmpty";
        internal const string ArgumentException_RequiredPositionalAfterOptionalPositional = "ArgumentException_RequiredPositionalAfterOptionalPositional";
        internal const string ArgumentException_TypeMismatchForRemainingArguments = "ArgumentException_TypeMismatchForRemainingArguments";
        internal const string ArgumentException_UnregisteredParameterName = "ArgumentException_UnregisteredParameterName";
        internal const string ArgumentException_ValidationParameterTypeMismatch = "ArgumentException_ValidationParameterTypeMismatch";
        internal const string ArgumentNull_Buffer = "ArgumentNull_Buffer";
        internal const string ArgumentNull_FileStream = "ArgumentNull_FileStream";
        internal const string ArgumentNull_MapName = "ArgumentNull_MapName";
        internal const string ArgumentNull_ServerName = "ArgumentNull_ServerName";
        internal const string ArgumentOutOfRange_AdditionalAccessLimited = "ArgumentOutOfRange_AdditionalAccessLimited";
        internal const string ArgumentOutOfRange_AnonymousReserved = "ArgumentOutOfRange_AnonymousReserved";
        internal const string ArgumentOutOfRange_CapacityGEFileSizeRequired = "ArgumentOutOfRange_CapacityGEFileSizeRequired";
        internal const string ArgumentOutOfRange_CapacityLargerThanLogicalAddressSpaceNotAllowed = "ArgumentOutOfRange_CapacityLargerThanLogicalAddressSpaceNotAllowed";
        internal const string ArgumentOutOfRange_DirectionModeInOrOut = "ArgumentOutOfRange_DirectionModeInOrOut";
        internal const string ArgumentOutOfRange_DirectionModeInOutOrInOut = "ArgumentOutOfRange_DirectionModeInOutOrInOut";
        internal const string ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable = "ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable";
        internal const string ArgumentOutOfRange_ImpersonationInvalid = "ArgumentOutOfRange_ImpersonationInvalid";
        internal const string ArgumentOutOfRange_ImpersonationOptionsInvalid = "ArgumentOutOfRange_ImpersonationOptionsInvalid";
        internal const string ArgumentOutOfRange_InvalidPipeAccessRights = "ArgumentOutOfRange_InvalidPipeAccessRights";
        internal const string ArgumentOutOfRange_InvalidTimeout = "ArgumentOutOfRange_InvalidTimeout";
        internal const string ArgumentOutOfRange_MaxArgExceeded = "ArgumentOutOfRange_MaxArgExceeded";
        internal const string ArgumentOutOfRange_MaxNumServerInstances = "ArgumentOutOfRange_MaxNumServerInstances";
        internal const string ArgumentOutOfRange_MaxStringsExceeded = "ArgumentOutOfRange_MaxStringsExceeded";
        internal const string ArgumentOutOfRange_NeedMaxFileSizeGEBufferSize = "ArgumentOutOfRange_NeedMaxFileSizeGEBufferSize";
        internal const string ArgumentOutOfRange_NeedNonNegNum = "ArgumentOutOfRange_NeedNonNegNum";
        internal const string ArgumentOutOfRange_NeedPositiveNumber = "ArgumentOutOfRange_NeedPositiveNumber";
        internal const string ArgumentOutOfRange_NeedValidId = "ArgumentOutOfRange_NeedValidId";
        internal const string ArgumentOutOfRange_NeedValidLogRetention = "ArgumentOutOfRange_NeedValidLogRetention";
        internal const string ArgumentOutOfRange_NeedValidMaxNumFiles = "ArgumentOutOfRange_NeedValidMaxNumFiles";
        internal const string ArgumentOutOfRange_NeedValidPipeAccessRights = "ArgumentOutOfRange_NeedValidPipeAccessRights";
        internal const string ArgumentOutOfRange_OptionsInvalid = "ArgumentOutOfRange_OptionsInvalid";
        internal const string ArgumentOutOfRange_PositionLessThanCapacityRequired = "ArgumentOutOfRange_PositionLessThanCapacityRequired";
        internal const string ArgumentOutOfRange_PositiveOrDefaultCapacityRequired = "ArgumentOutOfRange_PositiveOrDefaultCapacityRequired";
        internal const string ArgumentOutOfRange_PositiveOrDefaultSizeRequired = "ArgumentOutOfRange_PositiveOrDefaultSizeRequired";
        internal const string ArgumentOutOfRange_TransmissionModeByteOrMsg = "ArgumentOutOfRange_TransmissionModeByteOrMsg";
        internal const string CommandLineParser_Aliases = "CommandLineParser_Aliases";
        internal const string CommandLineParser_ErrorMessagePrefix = "CommandLineParser_ErrorMessagePrefix";
        internal const string CommandLineParser_HelpMessagePrefix = "CommandLineParser_HelpMessagePrefix";
        internal const string Cryptography_ArgECDHKeySizeMismatch = "Cryptography_ArgECDHKeySizeMismatch";
        internal const string Cryptography_ArgECDHRequiresECDHKey = "Cryptography_ArgECDHRequiresECDHKey";
        internal const string Cryptography_ArgECDsaRequiresECDsaKey = "Cryptography_ArgECDsaRequiresECDsaKey";
        internal const string Cryptography_ArgExpectedECDiffieHellmanCngPublicKey = "Cryptography_ArgExpectedECDiffieHellmanCngPublicKey";
        internal const string Cryptography_ArgMustBeCngAlgorithm = "Cryptography_ArgMustBeCngAlgorithm";
        internal const string Cryptography_ArgMustBeCngAlgorithmGroup = "Cryptography_ArgMustBeCngAlgorithmGroup";
        internal const string Cryptography_ArgMustBeCngKeyBlobFormat = "Cryptography_ArgMustBeCngKeyBlobFormat";
        internal const string Cryptography_ArgMustBeCngProvider = "Cryptography_ArgMustBeCngProvider";
        internal const string Cryptography_DecryptWithNoKey = "Cryptography_DecryptWithNoKey";
        internal const string Cryptography_ECXmlSerializationFormatRequired = "Cryptography_ECXmlSerializationFormatRequired";
        internal const string Cryptography_InvalidAlgorithmGroup = "Cryptography_InvalidAlgorithmGroup";
        internal const string Cryptography_InvalidAlgorithmName = "Cryptography_InvalidAlgorithmName";
        internal const string Cryptography_InvalidCipherMode = "Cryptography_InvalidCipherMode";
        internal const string Cryptography_InvalidIVSize = "Cryptography_InvalidIVSize";
        internal const string Cryptography_InvalidKeyBlobFormat = "Cryptography_InvalidKeyBlobFormat";
        internal const string Cryptography_InvalidKeySize = "Cryptography_InvalidKeySize";
        internal const string Cryptography_InvalidPadding = "Cryptography_InvalidPadding";
        internal const string Cryptography_InvalidProviderName = "Cryptography_InvalidProviderName";
        internal const string Cryptography_MissingDomainParameters = "Cryptography_MissingDomainParameters";
        internal const string Cryptography_MissingIV = "Cryptography_MissingIV";
        internal const string Cryptography_MissingPublicKey = "Cryptography_MissingPublicKey";
        internal const string Cryptography_MustTransformWholeBlock = "Cryptography_MustTransformWholeBlock";
        internal const string Cryptography_NonCompliantFIPSAlgorithm = "Cryptography_NonCompliantFIPSAlgorithm";
        internal const string Cryptography_OpenEphemeralKeyHandleWithoutEphemeralFlag = "Cryptography_OpenEphemeralKeyHandleWithoutEphemeralFlag";
        internal const string Cryptography_OpenInvalidHandle = "Cryptography_OpenInvalidHandle";
        internal const string Cryptography_PartialBlock = "Cryptography_PartialBlock";
        internal const string Cryptography_PlatformNotSupported = "Cryptography_PlatformNotSupported";
        internal const string Cryptography_TlsRequiresLabelAndSeed = "Cryptography_TlsRequiresLabelAndSeed";
        internal const string Cryptography_TransformBeyondEndOfBuffer = "Cryptography_TransformBeyondEndOfBuffer";
        internal const string Cryptography_UnexpectedXmlNamespace = "Cryptography_UnexpectedXmlNamespace";
        internal const string Cryptography_UnknownEllipticCurve = "Cryptography_UnknownEllipticCurve";
        internal const string Cryptography_UnknownEllipticCurveAlgorithm = "Cryptography_UnknownEllipticCurveAlgorithm";
        internal const string Cryptography_UnknownPaddingMode = "Cryptography_UnknownPaddingMode";
        internal const string IndexOutOfRange_IORaceCondition = "IndexOutOfRange_IORaceCondition";
        internal const string InvalidOperation_CalledTwice = "InvalidOperation_CalledTwice";
        internal const string InvalidOperation_CantCreateFileMapping = "InvalidOperation_CantCreateFileMapping";
        internal const string InvalidOperation_EndReadCalledMultiple = "InvalidOperation_EndReadCalledMultiple";
        internal const string InvalidOperation_EndWaitForConnectionCalledMultiple = "InvalidOperation_EndWaitForConnectionCalledMultiple";
        internal const string InvalidOperation_EndWriteCalledMultiple = "InvalidOperation_EndWriteCalledMultiple";
        internal const string InvalidOperation_EnumFailedVersion = "InvalidOperation_EnumFailedVersion";
        internal const string InvalidOperation_EnumOpCantHappen = "InvalidOperation_EnumOpCantHappen";
        internal const string InvalidOperation_PipeAlreadyConnected = "InvalidOperation_PipeAlreadyConnected";
        internal const string InvalidOperation_PipeAlreadyDisconnected = "InvalidOperation_PipeAlreadyDisconnected";
        internal const string InvalidOperation_PipeClosed = "InvalidOperation_PipeClosed";
        internal const string InvalidOperation_PipeDisconnected = "InvalidOperation_PipeDisconnected";
        internal const string InvalidOperation_PipeHandleNotSet = "InvalidOperation_PipeHandleNotSet";
        internal const string InvalidOperation_PipeMessageTypeNotSupported = "InvalidOperation_PipeMessageTypeNotSupported";
        internal const string InvalidOperation_PipeNotAsync = "InvalidOperation_PipeNotAsync";
        internal const string InvalidOperation_PipeNotYetConnected = "InvalidOperation_PipeNotYetConnected";
        internal const string InvalidOperation_PipeReadModeNotMessage = "InvalidOperation_PipeReadModeNotMessage";
        internal const string InvalidOperationException_AddParameterAfterParse = "InvalidOperationException_AddParameterAfterParse";
        internal const string InvalidOperationException_BindAfterBind = "InvalidOperationException_BindAfterBind";
        internal const string InvalidOperationException_GetParameterTypeMismatch = "InvalidOperationException_GetParameterTypeMismatch";
        internal const string InvalidOperationException_GetParameterValueBeforeParse = "InvalidOperationException_GetParameterValueBeforeParse";
        internal const string InvalidOperationException_GetRemainingArgumentsNotAllowed = "InvalidOperationException_GetRemainingArgumentsNotAllowed";
        internal const string InvalidOperationException_ParameterSetBeforeParse = "InvalidOperationException_ParameterSetBeforeParse";
        internal const string InvalidOperationException_SetRemainingArgumentsParameterAfterParse = "InvalidOperationException_SetRemainingArgumentsParameterAfterParse";
        internal const string IO_DriveNotFound_Drive = "IO_DriveNotFound_Drive";
        internal const string IO_EOF_ReadBeyondEOF = "IO_EOF_ReadBeyondEOF";
        internal const string IO_FileNotFound = "IO_FileNotFound";
        internal const string IO_FileNotFound_FileName = "IO_FileNotFound_FileName";
        internal const string IO_FileTooLongOrHandleNotSync = "IO_FileTooLongOrHandleNotSync";
        internal const string IO_IO_AlreadyExists_Name = "IO_IO_AlreadyExists_Name";
        internal const string IO_IO_BindHandleFailed = "IO_IO_BindHandleFailed";
        internal const string IO_IO_FileExists_Name = "IO_IO_FileExists_Name";
        internal const string IO_IO_InvalidPipeHandle = "IO_IO_InvalidPipeHandle";
        internal const string IO_IO_NoPermissionToDirectoryName = "IO_IO_NoPermissionToDirectoryName";
        internal const string IO_IO_PipeBroken = "IO_IO_PipeBroken";
        internal const string IO_IO_SharingViolation_File = "IO_IO_SharingViolation_File";
        internal const string IO_IO_SharingViolation_NoFileName = "IO_IO_SharingViolation_NoFileName";
        internal const string IO_NotEnoughMemory = "IO_NotEnoughMemory";
        internal const string IO_PathNotFound_NoPathName = "IO_PathNotFound_NoPathName";
        internal const string IO_PathNotFound_Path = "IO_PathNotFound_Path";
        internal const string IO_PathTooLong = "IO_PathTooLong";
        private static System.SR loader;
        internal const string LockRecursionException_ReadAfterWriteNotAllowed = "LockRecursionException_ReadAfterWriteNotAllowed";
        internal const string LockRecursionException_RecursiveReadNotAllowed = "LockRecursionException_RecursiveReadNotAllowed";
        internal const string LockRecursionException_RecursiveUpgradeNotAllowed = "LockRecursionException_RecursiveUpgradeNotAllowed";
        internal const string LockRecursionException_RecursiveWriteNotAllowed = "LockRecursionException_RecursiveWriteNotAllowed";
        internal const string LockRecursionException_UpgradeAfterReadNotAllowed = "LockRecursionException_UpgradeAfterReadNotAllowed";
        internal const string LockRecursionException_UpgradeAfterWriteNotAllowed = "LockRecursionException_UpgradeAfterWriteNotAllowed";
        internal const string LockRecursionException_WriteAfterReadNotAllowed = "LockRecursionException_WriteAfterReadNotAllowed";
        internal const string NotSupported_AnonymousPipeMessagesNotSupported = "NotSupported_AnonymousPipeMessagesNotSupported";
        internal const string NotSupported_AnonymousPipeUnidirectional = "NotSupported_AnonymousPipeUnidirectional";
        internal const string NotSupported_DelayAllocateFileBackedNotAllowed = "NotSupported_DelayAllocateFileBackedNotAllowed";
        internal const string NotSupported_DownLevelVista = "NotSupported_DownLevelVista";
        internal const string NotSupported_IONonFileDevices = "NotSupported_IONonFileDevices";
        internal const string NotSupported_MemStreamNotExpandable = "NotSupported_MemStreamNotExpandable";
        internal const string NotSupported_MMViewStreamsFixedLength = "NotSupported_MMViewStreamsFixedLength";
        internal const string NotSupported_SetTextWriter = "NotSupported_SetTextWriter";
        internal const string NotSupported_UnreadableStream = "NotSupported_UnreadableStream";
        internal const string NotSupported_UnseekableStream = "NotSupported_UnseekableStream";
        internal const string NotSupported_UnwritableStream = "NotSupported_UnwritableStream";
        internal const string ObjectDisposed_FileClosed = "ObjectDisposed_FileClosed";
        internal const string ObjectDisposed_PipeClosed = "ObjectDisposed_PipeClosed";
        internal const string ObjectDisposed_ReaderClosed = "ObjectDisposed_ReaderClosed";
        internal const string ObjectDisposed_StreamClosed = "ObjectDisposed_StreamClosed";
        internal const string ObjectDisposed_StreamIsClosed = "ObjectDisposed_StreamIsClosed";
        internal const string ObjectDisposed_ViewAccessorClosed = "ObjectDisposed_ViewAccessorClosed";
        internal const string ObjectDisposed_WriterClosed = "ObjectDisposed_WriterClosed";
        internal const string ParameterBindingException_AmbiguousParameterName = "ParameterBindingException_AmbiguousParameterName";
        internal const string ParameterBindingException_AmbiguousParameterSet = "ParameterBindingException_AmbiguousParameterSet";
        internal const string ParameterBindingException_NestedResponseFiles = "ParameterBindingException_NestedResponseFiles";
        internal const string ParameterBindingException_ParameterValueAlreadySpecified = "ParameterBindingException_ParameterValueAlreadySpecified";
        internal const string ParameterBindingException_RequiredParameterMissingCommandLineValue = "ParameterBindingException_RequiredParameterMissingCommandLineValue";
        internal const string ParameterBindingException_ResponseFileException = "ParameterBindingException_ResponseFileException";
        internal const string ParameterBindingException_TransformationError = "ParameterBindingException_TransformationError";
        internal const string ParameterBindingException_UnboundCommandLineArguments = "ParameterBindingException_UnboundCommandLineArguments";
        internal const string ParameterBindingException_UnboundMandatoryParameter = "ParameterBindingException_UnboundMandatoryParameter";
        internal const string ParameterBindingException_UnknownParameteName = "ParameterBindingException_UnknownParameteName";
        internal const string ParameterBindingException_UnknownParameterSet = "ParameterBindingException_UnknownParameterSet";
        internal const string ParameterBindingException_ValididationError = "ParameterBindingException_ValididationError";
        internal const string Perflib_Argument_CounterAlreadyExists = "Perflib_Argument_CounterAlreadyExists";
        internal const string Perflib_Argument_CounterNameAlreadyExists = "Perflib_Argument_CounterNameAlreadyExists";
        internal const string Perflib_Argument_CounterSetAlreadyRegister = "Perflib_Argument_CounterSetAlreadyRegister";
        internal const string Perflib_Argument_EmptyCounterName = "Perflib_Argument_EmptyCounterName";
        internal const string Perflib_Argument_EmptyInstanceName = "Perflib_Argument_EmptyInstanceName";
        internal const string Perflib_Argument_InstanceAlreadyExists = "Perflib_Argument_InstanceAlreadyExists";
        internal const string Perflib_Argument_InvalidCounterSetInstanceType = "Perflib_Argument_InvalidCounterSetInstanceType";
        internal const string Perflib_Argument_InvalidCounterType = "Perflib_Argument_InvalidCounterType";
        internal const string Perflib_Argument_InvalidInstance = "Perflib_Argument_InvalidInstance";
        internal const string Perflib_Argument_ProviderNotFound = "Perflib_Argument_ProviderNotFound";
        internal const string Perflib_InsufficientMemory_CounterSetTemplate = "Perflib_InsufficientMemory_CounterSetTemplate";
        internal const string Perflib_InsufficientMemory_InstanceCounterBlock = "Perflib_InsufficientMemory_InstanceCounterBlock";
        internal const string Perflib_InvalidOperation_AddCounterAfterInstance = "Perflib_InvalidOperation_AddCounterAfterInstance";
        internal const string Perflib_InvalidOperation_CounterRefValue = "Perflib_InvalidOperation_CounterRefValue";
        internal const string Perflib_InvalidOperation_CounterSetContainsNoCounter = "Perflib_InvalidOperation_CounterSetContainsNoCounter";
        internal const string Perflib_InvalidOperation_CounterSetNotInstalled = "Perflib_InvalidOperation_CounterSetNotInstalled";
        internal const string Perflib_InvalidOperation_InstanceNotFound = "Perflib_InvalidOperation_InstanceNotFound";
        internal const string Perflib_InvalidOperation_NoActiveProvider = "Perflib_InvalidOperation_NoActiveProvider";
        internal const string Perflib_PlatformNotSupported = "Perflib_PlatformNotSupported";
        internal const string PlatformNotSupported_NamedPipeServers = "PlatformNotSupported_NamedPipeServers";
        private ResourceManager resources;
        internal const string Serialization_MissingKeys = "Serialization_MissingKeys";
        internal const string SynchronizationLockException_IncorrectDispose = "SynchronizationLockException_IncorrectDispose";
        internal const string SynchronizationLockException_MisMatchedRead = "SynchronizationLockException_MisMatchedRead";
        internal const string SynchronizationLockException_MisMatchedUpgrade = "SynchronizationLockException_MisMatchedUpgrade";
        internal const string SynchronizationLockException_MisMatchedWrite = "SynchronizationLockException_MisMatchedWrite";
        internal const string TraceAsTraceSource = "TraceAsTraceSource";
        internal const string UnauthorizedAccess_IODenied_NoPathName = "UnauthorizedAccess_IODenied_NoPathName";
        internal const string UnauthorizedAccess_IODenied_Path = "UnauthorizedAccess_IODenied_Path";
        internal const string ValidateMetadataException_CountMaxLengthFailure = "ValidateMetadataException_CountMaxLengthFailure";
        internal const string ValidateMetadataException_CountMinLengthFailure = "ValidateMetadataException_CountMinLengthFailure";
        internal const string ValidateMetadataException_LengthMaxLengthFailure = "ValidateMetadataException_LengthMaxLengthFailure";
        internal const string ValidateMetadataException_LengthMinLengthFailure = "ValidateMetadataException_LengthMinLengthFailure";
        internal const string ValidateMetadataException_PatternFailure = "ValidateMetadataException_PatternFailure";
        internal const string ValidateMetadataException_RangeGreaterThanMaxRangeFailure = "ValidateMetadataException_RangeGreaterThanMaxRangeFailure";
        internal const string ValidateMetadataException_RangeSmallerThanMinRangeFailure = "ValidateMetadataException_RangeSmallerThanMinRangeFailure";

        internal SR()
        {
            this.resources = new ResourceManager("System.Core", base.GetType().Assembly);
        }

        private static System.SR GetLoader()
        {
            if (loader == null)
            {
                System.SR sr = new System.SR();
                Interlocked.CompareExchange<System.SR>(ref loader, sr, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            System.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            System.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            System.SR loader = GetLoader();
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

