namespace System.ServiceModel.Channels
{
    using System;
    using System.Net.Security;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    internal static class MsmqVerifier
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void EnsureSecurityTokenManagerPresent<TChannel>(MsmqChannelFactoryBase<TChannel> factory)
        {
            if (factory.SecurityTokenManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqTokenProviderNeededForCertificates")));
            }
        }

        internal static void VerifyReceiver(MsmqReceiveParameters receiveParameters, Uri listenUri)
        {
            MsmqException exception;
            bool flag2;
            if (!receiveParameters.Durable && receiveParameters.ExactlyOnce)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqNoAssurancesForVolatile")));
            }
            if (receiveParameters.ReceiveContextSettings.Enabled && !receiveParameters.ExactlyOnce)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqExactlyOnceNeededForReceiveContext")));
            }
            VerifySecurity(receiveParameters.TransportSecurity, null);
            string formatName = receiveParameters.AddressTranslator.UriToFormatName(listenUri);
            if (receiveParameters.ReceiveContextSettings.Enabled && formatName.Contains(";"))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqReceiveContextSubqueuesNotSupported")));
            }
            if (!MsmqQueue.IsReadable(formatName, out exception))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqQueueNotReadable"), exception));
            }
            bool flag = false;
            flag = MsmqQueue.TryGetIsTransactional(formatName, out flag2);
            try
            {
                if (!flag && (receiveParameters is MsmqTransportReceiveParameters))
                {
                    flag = MsmqQueue.TryGetIsTransactional(MsmqUri.ActiveDirectoryAddressTranslator.UriToFormatName(listenUri), out flag2);
                }
            }
            catch (MsmqException exception2)
            {
                MsmqDiagnostics.ExpectedException(exception2);
            }
            if (flag)
            {
                if (!receiveParameters.ExactlyOnce && flag2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqNonTransactionalQueueNeeded")));
                }
                if (receiveParameters.ExactlyOnce && !flag2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqTransactionalQueueNeeded")));
                }
            }
            if (receiveParameters.ExactlyOnce)
            {
                if (Msmq.IsAdvancedPoisonHandlingSupported)
                {
                    if (!formatName.Contains(";"))
                    {
                        if (!MsmqQueue.IsMoveable(formatName + ";retry"))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqDirectFormatNameRequiredForPoison")));
                        }
                    }
                    else if (ReceiveErrorHandling.Move == receiveParameters.ReceiveErrorHandling)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqNoMoveForSubqueues")));
                    }
                }
                else if ((ReceiveErrorHandling.Reject == receiveParameters.ReceiveErrorHandling) || (ReceiveErrorHandling.Move == receiveParameters.ReceiveErrorHandling))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqAdvancedPoisonHandlingRequired")));
                }
            }
        }

        private static void VerifySecurity(MsmqTransportSecurity security, bool? useActiveDirectory)
        {
            if ((security.MsmqAuthenticationMode == MsmqAuthenticationMode.WindowsDomain) && !Msmq.ActiveDirectoryEnabled)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqWindowsAuthnRequiresAD")));
            }
            if ((security.MsmqAuthenticationMode == MsmqAuthenticationMode.None) && (security.MsmqProtectionLevel != ProtectionLevel.None))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqAuthNoneRequiresProtectionNone")));
            }
            if ((security.MsmqAuthenticationMode == MsmqAuthenticationMode.Certificate) && (security.MsmqProtectionLevel == ProtectionLevel.None))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqAuthCertificateRequiresProtectionSign")));
            }
            if ((security.MsmqAuthenticationMode == MsmqAuthenticationMode.WindowsDomain) && (security.MsmqProtectionLevel == ProtectionLevel.None))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqAuthWindowsRequiresProtectionNotNone")));
            }
            if (((security.MsmqProtectionLevel == ProtectionLevel.EncryptAndSign) && useActiveDirectory.HasValue) && !useActiveDirectory.Value)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqEncryptRequiresUseAD")));
            }
        }

        internal static void VerifySender<TChannel>(MsmqChannelFactoryBase<TChannel> factory)
        {
            if (!factory.Durable && factory.ExactlyOnce)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqNoAssurancesForVolatile")));
            }
            MsmqChannelFactory<TChannel> factory2 = factory as MsmqChannelFactory<TChannel>;
            if (((factory2 != null) && factory2.UseActiveDirectory) && (factory2.QueueTransferProtocol != QueueTransferProtocol.Native))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqActiveDirectoryRequiresNativeTransfer")));
            }
            bool? useActiveDirectory = null;
            if (factory2 != null)
            {
                useActiveDirectory = new bool?(factory2.UseActiveDirectory);
            }
            VerifySecurity(factory.MsmqTransportSecurity, useActiveDirectory);
            if (null != factory.CustomDeadLetterQueue)
            {
                bool flag;
                if (DeadLetterQueue.Custom != factory.DeadLetterQueue)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqPerAppDLQRequiresCustom")));
                }
                if (!Msmq.IsPerAppDeadLetterQueueSupported)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqPerAppDLQRequiresMsmq4")));
                }
                if (!factory.ExactlyOnce)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqPerAppDLQRequiresExactlyOnce")));
                }
                string formatName = MsmqUri.NetMsmqAddressTranslator.UriToFormatName(factory.CustomDeadLetterQueue);
                if (!MsmqQueue.IsWriteable(formatName))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqDLQNotWriteable")));
                }
                if (!MsmqQueue.TryGetIsTransactional(formatName, out flag) || !flag)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqTransactedDLQExpected")));
                }
            }
            if ((null == factory.CustomDeadLetterQueue) && (DeadLetterQueue.Custom == factory.DeadLetterQueue))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqCustomRequiresPerAppDLQ")));
            }
            if (MsmqAuthenticationMode.Certificate == factory.MsmqTransportSecurity.MsmqAuthenticationMode)
            {
                EnsureSecurityTokenManagerPresent<TChannel>(factory);
            }
        }
    }
}

