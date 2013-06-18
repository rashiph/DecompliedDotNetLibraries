namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Globalization;
    using System.ServiceModel;

    internal static class Error
    {
        private const string FaultNamespace = "http://schemas.xmlsoap.org/Microsoft/WindowsCommunicationFoundation/2005/08/Faults/";

        public static Exception ActivationAccessDenied()
        {
            return CreateFault("ComActivationAccessDenied", System.ServiceModel.SR.GetString("ComActivationAccessDenied"));
        }

        public static Exception ActivationFailure()
        {
            return CreateFault("ComActivationFailure", System.ServiceModel.SR.GetString("ComActivationFailure"));
        }

        public static Exception CallAccessDenied()
        {
            return CreateFault("ComAccessDenied", System.ServiceModel.SR.GetString("ComMessageAccessDenied"));
        }

        public static Exception CannotAccessDirectory(string directory)
        {
            return CreateFault("CannotAccessDirectory", System.ServiceModel.SR.GetString("CannotAccessDirectory", new object[] { directory }));
        }

        private static Exception CreateFault(string code, string reason)
        {
            FaultCode code2 = FaultCode.CreateSenderFaultCode(code, "http://schemas.xmlsoap.org/Microsoft/WindowsCommunicationFoundation/2005/08/Faults/");
            return new FaultException(new FaultReason(reason, CultureInfo.CurrentCulture), code2);
        }

        public static Exception DirectoryNotFound(string directory)
        {
            return CreateFault("DirectoryNotFound", System.ServiceModel.SR.GetString("TempDirectoryNotFound", new object[] { directory }));
        }

        public static Exception DllHostInitializerFoundNoServices()
        {
            return CreateFault("DllHostInitializerFoundNoServices", System.ServiceModel.SR.GetString("ComDllHostInitializerFoundNoServices"));
        }

        public static Exception DuplicateOperation()
        {
            return CreateFault("DuplicateOperation", System.ServiceModel.SR.GetString("ComDuplicateOperation"));
        }

        public static Exception InconsistentSessionRequirements()
        {
            return CreateFault("ComInconsistentSessionRequirements", System.ServiceModel.SR.GetString("ComInconsistentSessionRequirements"));
        }

        public static Exception ListenerInitFailed(string message)
        {
            return new ComPlusListenerInitializationException(message);
        }

        public static Exception ListenerInitFailed(string message, Exception inner)
        {
            return new ComPlusListenerInitializationException(message, inner);
        }

        public static Exception ManifestCreationFailed(string file, string error)
        {
            return CreateFault("ManifestCreationFailed", System.ServiceModel.SR.GetString("ComIntegrationManifestCreationFailed", new object[] { file, error }));
        }

        public static Exception NoAsyncOperationsAllowed()
        {
            return CreateFault("NoAsyncOperationsAllowed", System.ServiceModel.SR.GetString("ComNoAsyncOperationsAllowed"));
        }

        public static Exception QFENotPresent()
        {
            return CreateFault("ServiceHostStartingServiceErrorNoQFE", System.ServiceModel.SR.GetString("ComPlusServiceHostStartingServiceErrorNoQFE"));
        }

        public static Exception RequiresWindowsSecurity()
        {
            return CreateFault("ComWindowsIdentityRequired", System.ServiceModel.SR.GetString("ComRequiresWindowsSecurity"));
        }

        public static Exception ServiceMonikerSupportLoadFailed(string dllname)
        {
            return CreateFault("UnableToLoadServiceMonikerSupportDll", System.ServiceModel.SR.GetString("UnableToLoadDll", new object[] { dllname }));
        }

        public static Exception TransactionMismatch()
        {
            return CreateFault("Transactions", System.ServiceModel.SR.GetString("SFxTransactionsNotSupported"));
        }

        public static Exception UnexpectedThreadingModel()
        {
            return CreateFault("UnexpectedThreadingModel", System.ServiceModel.SR.GetString("UnexpectedThreadingModel"));
        }
    }
}

