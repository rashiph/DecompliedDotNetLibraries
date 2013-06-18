namespace System.ServiceModel.Activation
{
    using System;
    using System.Globalization;
    using System.Resources;

    internal class SR
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceManager;

        private SR()
        {
        }

        internal static string CannotResolveConstructorStringToWorkflowType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CannotResolveConstructorStringToWorkflowType", Culture), new object[] { param0 });
        }

        internal static string ExtendedProtectionPolicyEnforcementMismatch(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ExtendedProtectionPolicyEnforcementMismatch", Culture), new object[] { param0, param1 });
        }

        internal static string ExtendedProtectionPolicyScenarioMismatch(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ExtendedProtectionPolicyScenarioMismatch", Culture), new object[] { param0, param1 });
        }

        internal static string Hosting_AddressIsAbsoluteUri(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_AddressIsAbsoluteUri", Culture), new object[] { param0 });
        }

        internal static string Hosting_AddressPointsOutsideTheVirtualDirectory(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_AddressPointsOutsideTheVirtualDirectory", Culture), new object[] { param0, param1 });
        }

        internal static string Hosting_AuthSchemesRequireOtherAuth(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_AuthSchemesRequireOtherAuth", Culture), new object[] { param0 });
        }

        internal static string Hosting_BadMetabaseSettingsIis7Type(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_BadMetabaseSettingsIis7Type", Culture), new object[] { param0 });
        }

        internal static string Hosting_BuildProviderAmbiguousType(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_BuildProviderAmbiguousType", Culture), new object[] { param0, param1, param2 });
        }

        internal static string Hosting_BuildProviderAttributeEmpty(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_BuildProviderAttributeEmpty", Culture), new object[] { param0 });
        }

        internal static string Hosting_BuildProviderAttributeMissing(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_BuildProviderAttributeMissing", Culture), new object[] { param0 });
        }

        internal static string Hosting_BuildProviderCouldNotCreateType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_BuildProviderCouldNotCreateType", Culture), new object[] { param0 });
        }

        internal static string Hosting_BuildProviderDirectiveEndBracketMissing(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_BuildProviderDirectiveEndBracketMissing", Culture), new object[] { param0 });
        }

        internal static string Hosting_BuildProviderDirectiveMissing(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_BuildProviderDirectiveMissing", Culture), new object[] { param0 });
        }

        internal static string Hosting_BuildProviderDuplicateAttribute(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_BuildProviderDuplicateAttribute", Culture), new object[] { param0 });
        }

        internal static string Hosting_BuildProviderDuplicateDirective(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_BuildProviderDuplicateDirective", Culture), new object[] { param0 });
        }

        internal static string Hosting_BuildProviderInvalidValueForBooleanAttribute(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_BuildProviderInvalidValueForBooleanAttribute", Culture), new object[] { param0, param1 });
        }

        internal static string Hosting_BuildProviderInvalidValueForNonNegativeIntegerAttribute(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_BuildProviderInvalidValueForNonNegativeIntegerAttribute", Culture), new object[] { param0, param1 });
        }

        internal static string Hosting_BuildProviderMutualExclusiveAttributes(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_BuildProviderMutualExclusiveAttributes", Culture), new object[] { param0, param1 });
        }

        internal static string Hosting_BuildProviderRequiredAttributesMissing(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_BuildProviderRequiredAttributesMissing", Culture), new object[] { param0, param1 });
        }

        internal static string Hosting_BuildProviderUnknownAttribute(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_BuildProviderUnknownAttribute", Culture), new object[] { param0 });
        }

        internal static string Hosting_BuildProviderUnknownDirective(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_BuildProviderUnknownDirective", Culture), new object[] { param0 });
        }

        internal static string Hosting_CompilationResultInvalid(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_CompilationResultInvalid", Culture), new object[] { param0 });
        }

        internal static string Hosting_CurlyBracketFoundInRoutePrefix(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_CurlyBracketFoundInRoutePrefix", Culture), new object[] { param0, param1 });
        }

        internal static string Hosting_EnvironmentShuttingDown(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_EnvironmentShuttingDown", Culture), new object[] { param0, param1 });
        }

        internal static string Hosting_ExtendedProtectionDotlessSpnNotEnabled(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_ExtendedProtectionDotlessSpnNotEnabled", Culture), new object[] { param0 });
        }

        internal static string Hosting_ExtendedProtectionFlagsNotSupport(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_ExtendedProtectionFlagsNotSupport", Culture), new object[] { param0 });
        }

        internal static string Hosting_ExtendedProtectionPoliciesMustMatch(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_ExtendedProtectionPoliciesMustMatch", Culture), new object[] { param0 });
        }

        internal static string Hosting_ExtendedProtectionSpnFormatError(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_ExtendedProtectionSpnFormatError", Culture), new object[] { param0 });
        }

        internal static string Hosting_FactoryTypeNotResolved(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_FactoryTypeNotResolved", Culture), new object[] { param0 });
        }

        internal static string Hosting_InvalidHandlerForWorkflowService(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_InvalidHandlerForWorkflowService", Culture), new object[] { param0, param1, param2 });
        }

        internal static string Hosting_IServiceHostNotImplemented(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_IServiceHostNotImplemented", Culture), new object[] { param0 });
        }

        internal static string Hosting_ListenerNotFoundForActivation(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_ListenerNotFoundForActivation", Culture), new object[] { param0 });
        }

        internal static string Hosting_ListenerNotFoundForActivationInRecycling(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_ListenerNotFoundForActivationInRecycling", Culture), new object[] { param0 });
        }

        internal static string Hosting_MemoryGatesCheckFailed(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_MemoryGatesCheckFailed", Culture), new object[] { param0, param1 });
        }

        internal static string Hosting_MetabaseDataStringsTerminate(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_MetabaseDataStringsTerminate", Culture), new object[] { param0 });
        }

        internal static string Hosting_MetabaseDataTypeUnsupported(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_MetabaseDataTypeUnsupported", Culture), new object[] { param0, param1 });
        }

        internal static string Hosting_MetabaseSettingsIis7TypeNotFound(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_MetabaseSettingsIis7TypeNotFound", Culture), new object[] { param0, param1 });
        }

        internal static string Hosting_NoDefaultCtor(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_NoDefaultCtor", Culture), new object[] { param0 });
        }

        internal static string Hosting_NonHTTPInCompatibilityMode(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_NonHTTPInCompatibilityMode", Culture), new object[] { param0 });
        }

        internal static string Hosting_NoServiceAndFactorySpecifiedForFilelessService(object param0, object param1, object param2, object param3)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_NoServiceAndFactorySpecifiedForFilelessService", Culture), new object[] { param0, param1, param2, param3 });
        }

        internal static string Hosting_NotSupportedAuthScheme(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_NotSupportedAuthScheme", Culture), new object[] { param0 });
        }

        internal static string Hosting_NotSupportedProtocol(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_NotSupportedProtocol", Culture), new object[] { param0 });
        }

        internal static string Hosting_NoValidExtensionFoundForRegistedFilelessService(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_NoValidExtensionFoundForRegistedFilelessService", Culture), new object[] { param0, param1 });
        }

        internal static string Hosting_ProcessNotExecutingUnderHostedContext(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_ProcessNotExecutingUnderHostedContext", Culture), new object[] { param0 });
        }

        internal static string Hosting_ProtocolNoConfiguration(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_ProtocolNoConfiguration", Culture), new object[] { param0 });
        }

        internal static string Hosting_RelativeAddressExtensionNotSupportError(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_RelativeAddressExtensionNotSupportError", Culture), new object[] { param0, param1, param2 });
        }

        internal static string Hosting_RelativeAddressFormatError(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_RelativeAddressFormatError", Culture), new object[] { param0 });
        }

        internal static string Hosting_RelativeAddressHasBeenAdded(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_RelativeAddressHasBeenAdded", Culture), new object[] { param0, param1 });
        }

        internal static string Hosting_RouteHasAlreadyBeenAdded(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_RouteHasAlreadyBeenAdded", Culture), new object[] { param0 });
        }

        internal static string Hosting_ServiceCannotBeActivated(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_ServiceCannotBeActivated", Culture), new object[] { param0, param1 });
        }

        internal static string Hosting_ServiceHostBaseIsNull(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_ServiceHostBaseIsNull", Culture), new object[] { param0 });
        }

        internal static string Hosting_ServiceNotExist(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_ServiceNotExist", Culture), new object[] { param0 });
        }

        internal static string Hosting_ServiceTypeNotResolved(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_ServiceTypeNotResolved", Culture), new object[] { param0 });
        }

        internal static string Hosting_SharedEndpointRequiresRelativeEndpoint(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_SharedEndpointRequiresRelativeEndpoint", Culture), new object[] { param0 });
        }

        internal static string Hosting_SslSettingsMisconfigured(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_SslSettingsMisconfigured", Culture), new object[] { param0, param1 });
        }

        internal static string Hosting_TransportBindingNotFound(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("Hosting_TransportBindingNotFound", Culture), new object[] { param0 });
        }

        internal static string InvalidCompiledString(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidCompiledString", Culture), new object[] { param0 });
        }

        internal static string PartialTrustNonHttpActivation(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("PartialTrustNonHttpActivation", Culture), new object[] { param0, param1 });
        }

        internal static string TypeNotActivity(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TypeNotActivity", Culture), new object[] { param0 });
        }

        internal static string BaseAddressesNotProvided
        {
            get
            {
                return ResourceManager.GetString("BaseAddressesNotProvided", Culture);
            }
        }

        internal static CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        internal static string DefaultBusyCountSource
        {
            get
            {
                return ResourceManager.GetString("DefaultBusyCountSource", Culture);
            }
        }

        internal static string ExtendedProtectionNotSupported
        {
            get
            {
                return ResourceManager.GetString("ExtendedProtectionNotSupported", Culture);
            }
        }

        internal static string ExtendedProtectionPolicyCustomChannelBindingMismatch
        {
            get
            {
                return ResourceManager.GetString("ExtendedProtectionPolicyCustomChannelBindingMismatch", Culture);
            }
        }

        internal static string Hosting_AuthSchemesRequireWindowsAuth
        {
            get
            {
                return ResourceManager.GetString("Hosting_AuthSchemesRequireWindowsAuth", Culture);
            }
        }

        internal static string Hosting_BuildProviderDirectiveNameMissing
        {
            get
            {
                return ResourceManager.GetString("Hosting_BuildProviderDirectiveNameMissing", Culture);
            }
        }

        internal static string Hosting_BuildProviderMainAttributeMissing
        {
            get
            {
                return ResourceManager.GetString("Hosting_BuildProviderMainAttributeMissing", Culture);
            }
        }

        internal static string Hosting_ExtendedProtectionSPNListNotSubset
        {
            get
            {
                return ResourceManager.GetString("Hosting_ExtendedProtectionSPNListNotSubset", Culture);
            }
        }

        internal static string Hosting_GetGlobalMemoryFailed
        {
            get
            {
                return ResourceManager.GetString("Hosting_GetGlobalMemoryFailed", Culture);
            }
        }

        internal static string Hosting_ImpersonationFailed
        {
            get
            {
                return ResourceManager.GetString("Hosting_ImpersonationFailed", Culture);
            }
        }

        internal static string Hosting_MemoryGatesCheckFailedUnderPartialTrust
        {
            get
            {
                return ResourceManager.GetString("Hosting_MemoryGatesCheckFailedUnderPartialTrust", Culture);
            }
        }

        internal static string Hosting_MetabaseAccessError
        {
            get
            {
                return ResourceManager.GetString("Hosting_MetabaseAccessError", Culture);
            }
        }

        internal static string Hosting_RouteServiceRequiresCompatibilityMode
        {
            get
            {
                return ResourceManager.GetString("Hosting_RouteServiceRequiresCompatibilityMode", Culture);
            }
        }

        internal static string Hosting_ServiceCompatibilityNotAllowed
        {
            get
            {
                return ResourceManager.GetString("Hosting_ServiceCompatibilityNotAllowed", Culture);
            }
        }

        internal static string Hosting_ServiceCompatibilityRequire
        {
            get
            {
                return ResourceManager.GetString("Hosting_ServiceCompatibilityRequire", Culture);
            }
        }

        internal static string Hosting_ServiceTypeNotProvided
        {
            get
            {
                return ResourceManager.GetString("Hosting_ServiceTypeNotProvided", Culture);
            }
        }

        internal static string Hosting_UnrecognizedTokenCheckingValue
        {
            get
            {
                return ResourceManager.GetString("Hosting_UnrecognizedTokenCheckingValue", Culture);
            }
        }

        internal static string PipeListenerProxyStopped
        {
            get
            {
                return ResourceManager.GetString("PipeListenerProxyStopped", Culture);
            }
        }

        internal static string RequestContextAborted
        {
            get
            {
                return ResourceManager.GetString("RequestContextAborted", Culture);
            }
        }

        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceManager, null))
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("System.ServiceModel.Activation.SR", typeof(System.ServiceModel.Activation.SR).Assembly);
                    resourceManager = manager;
                }
                return resourceManager;
            }
        }

        internal static string ServiceTypeUnknown
        {
            get
            {
                return ResourceManager.GetString("ServiceTypeUnknown", Culture);
            }
        }

        internal static string TraceCodeHttpChannelMessageReceiveFailed
        {
            get
            {
                return ResourceManager.GetString("TraceCodeHttpChannelMessageReceiveFailed", Culture);
            }
        }

        internal static string TraceCodeRequestContextAbort
        {
            get
            {
                return ResourceManager.GetString("TraceCodeRequestContextAbort", Culture);
            }
        }

        internal static string TraceCodeWebHostCompilation
        {
            get
            {
                return ResourceManager.GetString("TraceCodeWebHostCompilation", Culture);
            }
        }

        internal static string TraceCodeWebHostDebugRequest
        {
            get
            {
                return ResourceManager.GetString("TraceCodeWebHostDebugRequest", Culture);
            }
        }

        internal static string TraceCodeWebHostNoCBTSupport
        {
            get
            {
                return ResourceManager.GetString("TraceCodeWebHostNoCBTSupport", Culture);
            }
        }

        internal static string TraceCodeWebHostProtocolMisconfigured
        {
            get
            {
                return ResourceManager.GetString("TraceCodeWebHostProtocolMisconfigured", Culture);
            }
        }

        internal static string TraceCodeWebHostServiceActivated
        {
            get
            {
                return ResourceManager.GetString("TraceCodeWebHostServiceActivated", Culture);
            }
        }

        internal static string TraceCodeWebHostServiceCloseFailed
        {
            get
            {
                return ResourceManager.GetString("TraceCodeWebHostServiceCloseFailed", Culture);
            }
        }

        internal static string ValueMustBeNonNegative
        {
            get
            {
                return ResourceManager.GetString("ValueMustBeNonNegative", Culture);
            }
        }

        internal static string WorkflowServiceHostFactoryConstructorStringNotProvided
        {
            get
            {
                return ResourceManager.GetString("WorkflowServiceHostFactoryConstructorStringNotProvided", Culture);
            }
        }
    }
}

