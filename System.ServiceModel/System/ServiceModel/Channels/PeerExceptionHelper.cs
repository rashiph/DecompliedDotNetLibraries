namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.PeerResolvers;

    internal class PeerExceptionHelper
    {
        public static Exception GetLastException()
        {
            return new Win32Exception(Marshal.GetLastWin32Error());
        }

        internal static void ThrowArgument_InsufficientCredentials(string property)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InsufficientCredentials", new object[] { property })));
        }

        internal static void ThrowArgument_InsufficientResolverSettings()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InsufficientResolverSettings")));
        }

        internal static void ThrowArgument_InvalidResolverMode(PeerResolverMode mode)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InvalidResolverMode", new object[] { mode })));
        }

        internal static void ThrowArgument_MustOverrideInitialize()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MustOverrideInitialize")));
        }

        internal static void ThrowArgument_PnrpAddressesExceedLimit()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("PnrpAddressesExceedLimit")));
        }

        internal static void ThrowArgumentOutOfRange_InvalidSecurityMode(int value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("Mode", value, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { SecurityMode.None, SecurityMode.TransportWithMessageCredential })));
        }

        internal static void ThrowArgumentOutOfRange_InvalidTransportCredentialType(int value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("CredentialType", value, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { PeerTransportCredentialType.Password, PeerTransportCredentialType.Certificate })));
        }

        internal static void ThrowInvalidOperation_ConflictingHeader(string headerName)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerConflictingHeader", new object[] { headerName, "http://schemas.microsoft.com/net/2006/05/peer" })));
        }

        internal static void ThrowInvalidOperation_DuplicatePeerRegistration(string servicepath)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("DuplicatePeerRegistration", new object[] { servicepath })));
        }

        internal static void ThrowInvalidOperation_InsufficientCryptoSupport(Exception innerException)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InsufficientCryptoSupport"), innerException));
        }

        internal static void ThrowInvalidOperation_NotValidWhenClosed(string operation)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NotValidWhenClosed", new object[] { operation })));
        }

        internal static void ThrowInvalidOperation_NotValidWhenOpen(string operation)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NotValidWhenOpen", new object[] { operation })));
        }

        internal static void ThrowInvalidOperation_PeerCertGenFailure(Exception innerException)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerCertGenFailure"), innerException));
        }

        internal static void ThrowInvalidOperation_PeerConflictingPeerNodeSettings(string propertyName)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerConflictingPeerNodeSettings", new object[] { propertyName })));
        }

        internal static void ThrowInvalidOperation_PnrpAddressesUnsupported()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PnrpAddressesUnsupported")));
        }

        internal static void ThrowInvalidOperation_PnrpNoClouds()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PnrpNoClouds")));
        }

        internal static void ThrowInvalidOperation_UnexpectedSecurityTokensDuringHandshake()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnexpectedSecurityTokensDuringHandshake")));
        }

        internal static void ThrowPnrpError(int errorCode, string cloud)
        {
            ThrowPnrpError(errorCode, cloud, true);
        }

        internal static void ThrowPnrpError(int errorCode, string cloud, bool trace)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PnrpPeerResolver.PnrpException(errorCode, cloud), trace ? TraceEventType.Error : TraceEventType.Information);
        }
    }
}

