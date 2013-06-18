namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.EnterpriseServices;
    using System.IdentityModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;

    internal class ComPlusInstanceProvider : IInstanceProvider
    {
        private static readonly Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
        private ServiceInfo info;
        private static bool platformSupportsBitness;
        private static bool platformSupportsBitnessSet;

        public ComPlusInstanceProvider(ServiceInfo info)
        {
            this.info = info;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ComPlusInstanceProviderRequiresMessage0")));
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            object obj2 = null;
            Guid empty = Guid.Empty;
            if (ContextUtil.IsInTransaction)
            {
                empty = ContextUtil.TransactionId;
            }
            ComPlusInstanceCreationTrace.Trace(TraceEventType.Verbose, 0x50012, "TraceCodeComIntegrationInstanceCreationRequest", this.info, message, empty);
            WindowsIdentity messageIdentity = null;
            messageIdentity = MessageUtil.GetMessageIdentity(message);
            WindowsImpersonationContext context = null;
            try
            {
                try
                {
                    if (this.info.HostingMode == HostingMode.WebHostOutOfProcess)
                    {
                        if (!System.ServiceModel.ComIntegration.SecurityUtils.IsAtleastImpersonationToken(new SafeCloseHandle(messageIdentity.Token, false)))
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("BadImpersonationLevelForOutOfProcWas"), HR.ERROR_BAD_IMPERSONATION_LEVEL));
                        }
                        context = messageIdentity.Impersonate();
                    }
                    CLSCTX sERVER = CLSCTX.SERVER;
                    if (PlatformSupportsBitness && (this.info.HostingMode == HostingMode.WebHostOutOfProcess))
                    {
                        if (this.info.Bitness == Bitness.Bitness32)
                        {
                            sERVER |= CLSCTX.ACTIVATE_32_BIT_SERVER;
                        }
                        else
                        {
                            sERVER |= CLSCTX.ACTIVATE_64_BIT_SERVER;
                        }
                    }
                    obj2 = System.ServiceModel.ComIntegration.SafeNativeMethods.CoCreateInstance(this.info.Clsid, null, sERVER, IID_IUnknown);
                }
                finally
                {
                    if (context != null)
                    {
                        context.Undo();
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                Uri uri = null;
                if (message.Headers.From != null)
                {
                    uri = message.Headers.From.Uri;
                }
                System.ServiceModel.DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.ComPlus, (EventLogEventId) (-1073610726), new string[] { (uri == null) ? string.Empty : uri.ToString(), this.info.AppID.ToString(), this.info.Clsid.ToString(), empty.ToString(), messageIdentity.Name, exception.ToString() });
                throw TraceUtility.ThrowHelperError(exception, message);
            }
            System.ServiceModel.ComIntegration.TransactionProxy proxy = instanceContext.Extensions.Find<System.ServiceModel.ComIntegration.TransactionProxy>();
            if (proxy != null)
            {
                proxy.InstanceID = obj2.GetHashCode();
            }
            ComPlusInstanceCreationTrace.Trace(TraceEventType.Verbose, 0x50013, "TraceCodeComIntegrationInstanceCreationSuccess", this.info, message, obj2.GetHashCode(), empty);
            return obj2;
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            int hashCode = instance.GetHashCode();
            IDisposable disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
            else
            {
                Marshal.ReleaseComObject(instance);
            }
            ComPlusInstanceCreationTrace.Trace(TraceEventType.Verbose, 0x50014, "TraceCodeComIntegrationInstanceReleased", this.info, instanceContext, hashCode);
        }

        private static bool PlatformSupportsBitness
        {
            get
            {
                if (!platformSupportsBitnessSet)
                {
                    if (Environment.OSVersion.Version.Major > 5)
                    {
                        platformSupportsBitness = true;
                    }
                    else if (Environment.OSVersion.Version.Major == 5)
                    {
                        if (Environment.OSVersion.Version.Minor > 2)
                        {
                            platformSupportsBitness = true;
                        }
                        else if ((Environment.OSVersion.Version.Minor == 2) && !string.IsNullOrEmpty(Environment.OSVersion.ServicePack))
                        {
                            platformSupportsBitness = true;
                        }
                    }
                    platformSupportsBitnessSet = true;
                }
                return platformSupportsBitness;
            }
        }
    }
}

