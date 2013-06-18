namespace System.ServiceModel.Security
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IdentityModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Xml;

    internal static class SecurityAuditHelper
    {
        private const uint APF_AuditFailure = 0;
        private const uint APF_AuditSuccess = 1;
        private const string ApplicationEventSourceName = "ServiceModel Audit 4.0.0.0";
        private static SafeLoadLibraryHandle authzModule;
        private const uint ImpersonationFailure = 0xc006000a;
        private const uint ImpersonationSuccess = 0x40060009;
        private static bool isSecurityAuditSupported;
        private const int MessageAuthenticationCategory = 2;
        private const uint MessageAuthenticationFailure = 0xc0060004;
        private const uint MessageAuthenticationSuccess = 0x40060003;
        private const string SecurityEventSourceName = "ServiceModel 4.0.0.0";
        private const uint SecurityNegotiationFailure = 0xc0060006;
        private const uint SecurityNegotiationSuccess = 0x40060005;
        private const int ServiceAuthorizationCategory = 1;
        private const uint ServiceAuthorizationFailure = 0xc0060002;
        private const uint ServiceAuthorizationSuccess = 0x40060001;
        private const uint TransportAuthenticationFailure = 0xc0060008;
        private const uint TransportAuthenticationSuccess = 0x40060007;

        private static string ExceptionToString(Exception exception)
        {
            Exception innerException = exception;
            StringBuilder builder = new StringBuilder(0x80);
            while (innerException != null)
            {
                builder.Append(innerException.GetType().Name);
                builder.Append(": ");
                builder.Append(innerException.Message);
                innerException = innerException.InnerException;
                if (innerException != null)
                {
                    builder.Append(" ---> ");
                }
            }
            return builder.ToString();
        }

        private static string GetActivityId()
        {
            Guid activityId = System.ServiceModel.Diagnostics.DiagnosticTrace.ActivityId;
            if (!(activityId == Guid.Empty))
            {
                return activityId.ToString();
            }
            return "<null>";
        }

        private static void WriteAuditEvent(uint auditType, uint auditId, params string[] parameters)
        {
            if (!IsSecurityAuditSupported)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new PlatformNotSupportedException(System.ServiceModel.SR.GetString("SecurityAuditPlatformNotSupported")));
            }
            Privilege privilege = new Privilege("SeAuditPrivilege");
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                try
                {
                    SafeSecurityAuditHandle handle;
                    privilege.Enable();
                    if (!NativeMethods.AuthzRegisterSecurityEventSource(0, "ServiceModel 4.0.0.0", out handle))
                    {
                        int error = Marshal.GetLastWin32Error();
                        Utility.CloseInvalidOutSafeHandle(handle);
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                    }
                    SafeHGlobalHandle handle2 = null;
                    SafeHGlobalHandle[] handleArray = new SafeHGlobalHandle[parameters.Length];
                    try
                    {
                        NativeMethods.AUDIT_PARAM audit_param;
                        NativeMethods.AUDIT_PARAMS audit_params;
                        handle2 = SafeHGlobalHandle.AllocHGlobal((int) (parameters.Length * NativeMethods.AUDIT_PARAM.Size));
                        long num2 = handle2.DangerousGetHandle().ToInt64();
                        audit_param.Type = NativeMethods.AUDIT_PARAM_TYPE.APT_String;
                        audit_param.Length = 0;
                        audit_param.Flags = 0;
                        audit_param.Data1 = IntPtr.Zero;
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(parameters[i]))
                            {
                                string s = System.ServiceModel.Diagnostics.EventLogger.NormalizeEventLogParameter(parameters[i]);
                                handleArray[i] = SafeHGlobalHandle.AllocHGlobal(s);
                                audit_param.Data0 = handleArray[i].DangerousGetHandle();
                            }
                            else
                            {
                                audit_param.Data0 = IntPtr.Zero;
                            }
                            Marshal.StructureToPtr(audit_param, new IntPtr(num2 + (i * NativeMethods.AUDIT_PARAM.Size)), false);
                        }
                        audit_params.Length = 0;
                        audit_params.Flags = auditType;
                        audit_params.Parameters = handle2;
                        audit_params.Count = (ushort) parameters.Length;
                        if (!NativeMethods.AuthzReportSecurityEventFromParams(auditType, handle, auditId, null, ref audit_params))
                        {
                            int num4 = Marshal.GetLastWin32Error();
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(num4));
                        }
                    }
                    finally
                    {
                        for (int j = 0; j < handleArray.Length; j++)
                        {
                            if (handleArray[j] != null)
                            {
                                handleArray[j].Close();
                            }
                        }
                        if (handle2 != null)
                        {
                            handle2.Close();
                        }
                        handle.Close();
                    }
                }
                finally
                {
                    int num6 = -1;
                    string message = null;
                    try
                    {
                        num6 = privilege.Revert();
                        if (num6 != 0)
                        {
                            message = System.ServiceModel.SR.GetString("RevertingPrivilegeFailed", new object[] { new Win32Exception(num6) });
                        }
                    }
                    finally
                    {
                        if (num6 != 0)
                        {
                            System.ServiceModel.DiagnosticUtility.FailFast(message);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private static void WriteEventToApplicationLog(EventInstance instance, params object[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                string param = parameters[i] as string;
                if (param != null)
                {
                    parameters[i] = System.ServiceModel.Diagnostics.EventLogger.NormalizeEventLogParameter(param);
                }
            }
            EventLog.WriteEvent("ServiceModel Audit 4.0.0.0", instance, parameters);
        }

        public static void WriteImpersonationFailureEvent(AuditLogLocation auditLogLocation, bool suppressAuditFailure, string operationName, string clientIdentity, Exception exception)
        {
            try
            {
                if (auditLogLocation == AuditLogLocation.Default)
                {
                    auditLogLocation = IsSecurityAuditSupported ? AuditLogLocation.Security : AuditLogLocation.Application;
                }
                string activityId = GetActivityId();
                if (auditLogLocation == AuditLogLocation.Application)
                {
                    WriteEventToApplicationLog(new EventInstance(0xc006000aL, 2, EventLogEntryType.Error), new object[] { operationName, clientIdentity, activityId, ExceptionToString(exception) });
                }
                else
                {
                    if (auditLogLocation != AuditLogLocation.Security)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("auditLogLocation", System.ServiceModel.SR.GetString("SecurityAuditPlatformNotSupported")));
                    }
                    WriteAuditEvent(0, 0xc006000a, new string[] { operationName, clientIdentity, activityId, ExceptionToString(exception) });
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x70053, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenSuccess"), new SecurityAuditTraceRecord(auditLogLocation, "ImpersonationFailure"), null, null);
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x70054, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenFailure"), new SecurityAuditTraceRecord(auditLogLocation, "ImpersonationFailure"), null, exception2);
                }
                if (!suppressAuditFailure)
                {
                    throw;
                }
            }
        }

        public static void WriteImpersonationSuccessEvent(AuditLogLocation auditLogLocation, bool suppressAuditFailure, string operationName, string clientIdentity)
        {
            try
            {
                if (auditLogLocation == AuditLogLocation.Default)
                {
                    auditLogLocation = IsSecurityAuditSupported ? AuditLogLocation.Security : AuditLogLocation.Application;
                }
                string activityId = GetActivityId();
                if (auditLogLocation == AuditLogLocation.Application)
                {
                    WriteEventToApplicationLog(new EventInstance(0x40060009L, 2, EventLogEntryType.Information), new object[] { operationName, clientIdentity, activityId });
                }
                else
                {
                    if (auditLogLocation != AuditLogLocation.Security)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("auditLogLocation", System.ServiceModel.SR.GetString("SecurityAuditPlatformNotSupported")));
                    }
                    WriteAuditEvent(1, 0x40060009, new string[] { operationName, clientIdentity, activityId });
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x70053, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenSuccess"), new SecurityAuditTraceRecord(auditLogLocation, "ImpersonationSuccess"), null, null);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x70054, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenFailure"), new SecurityAuditTraceRecord(auditLogLocation, "ImpersonationSuccess"), null, exception);
                }
                if (!suppressAuditFailure)
                {
                    throw;
                }
            }
        }

        public static void WriteMessageAuthenticationFailureEvent(AuditLogLocation auditLogLocation, bool suppressAuditFailure, Message message, Uri serviceUri, string action, string clientIdentity, Exception exception)
        {
            try
            {
                if (auditLogLocation == AuditLogLocation.Default)
                {
                    auditLogLocation = IsSecurityAuditSupported ? AuditLogLocation.Security : AuditLogLocation.Application;
                }
                string activityId = GetActivityId();
                if (auditLogLocation == AuditLogLocation.Application)
                {
                    WriteEventToApplicationLog(new EventInstance(0xc0060004L, 2, EventLogEntryType.Error), new object[] { serviceUri.AbsoluteUri, action, clientIdentity, activityId, ExceptionToString(exception) });
                }
                else
                {
                    if (auditLogLocation != AuditLogLocation.Security)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("auditLogLocation", System.ServiceModel.SR.GetString("SecurityAuditPlatformNotSupported")));
                    }
                    WriteAuditEvent(0, 0xc0060004, new string[] { serviceUri.AbsoluteUri, action, clientIdentity, activityId, ExceptionToString(exception) });
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x70053, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenSuccess"), new SecurityAuditTraceRecord(auditLogLocation, "MessageAuthenticationFailure"), null, null, message);
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x70054, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenFailure"), new SecurityAuditTraceRecord(auditLogLocation, "MessageAuthenticationFailure"), null, exception2, message);
                }
                if (!suppressAuditFailure)
                {
                    throw;
                }
            }
        }

        public static void WriteMessageAuthenticationSuccessEvent(AuditLogLocation auditLogLocation, bool suppressAuditFailure, Message message, Uri serviceUri, string action, string clientIdentity)
        {
            try
            {
                if (auditLogLocation == AuditLogLocation.Default)
                {
                    auditLogLocation = IsSecurityAuditSupported ? AuditLogLocation.Security : AuditLogLocation.Application;
                }
                string activityId = GetActivityId();
                if (auditLogLocation == AuditLogLocation.Application)
                {
                    WriteEventToApplicationLog(new EventInstance(0x40060003L, 2, EventLogEntryType.Information), new object[] { serviceUri.AbsoluteUri, action, clientIdentity, activityId });
                }
                else
                {
                    if (auditLogLocation != AuditLogLocation.Security)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("auditLogLocation", System.ServiceModel.SR.GetString("SecurityAuditPlatformNotSupported")));
                    }
                    WriteAuditEvent(1, 0x40060003, new string[] { serviceUri.AbsoluteUri, action, clientIdentity, activityId });
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x70053, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenSuccess"), new SecurityAuditTraceRecord(auditLogLocation, "MessageAuthenticationSuccess"), null, null, message);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x70054, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenFailure"), new SecurityAuditTraceRecord(auditLogLocation, "MessageAuthenticationSuccess"), null, exception, message);
                }
                if (!suppressAuditFailure)
                {
                    throw;
                }
            }
        }

        public static void WriteSecurityNegotiationFailureEvent(AuditLogLocation auditLogLocation, bool suppressAuditFailure, Message message, Uri serviceUri, string action, string clientIdentity, string negotiationType, Exception exception)
        {
            try
            {
                if (auditLogLocation == AuditLogLocation.Default)
                {
                    auditLogLocation = IsSecurityAuditSupported ? AuditLogLocation.Security : AuditLogLocation.Application;
                }
                string activityId = GetActivityId();
                if (auditLogLocation == AuditLogLocation.Application)
                {
                    WriteEventToApplicationLog(new EventInstance(0xc0060006L, 2, EventLogEntryType.Error), new object[] { serviceUri.AbsoluteUri, action, clientIdentity, activityId, negotiationType, ExceptionToString(exception) });
                }
                else
                {
                    if (auditLogLocation != AuditLogLocation.Security)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("auditLogLocation", System.ServiceModel.SR.GetString("SecurityAuditPlatformNotSupported")));
                    }
                    WriteAuditEvent(0, 0xc0060006, new string[] { serviceUri.AbsoluteUri, action, clientIdentity, activityId, negotiationType, ExceptionToString(exception) });
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x70053, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenSuccess"), new SecurityAuditTraceRecord(auditLogLocation, "SecurityNegotiationFailure"), null, null, message);
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x70054, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenFailure"), new SecurityAuditTraceRecord(auditLogLocation, "SecurityNegotiationFailure"), null, exception2, message);
                }
                if (!suppressAuditFailure)
                {
                    throw;
                }
            }
        }

        public static void WriteSecurityNegotiationSuccessEvent(AuditLogLocation auditLogLocation, bool suppressAuditFailure, Message message, Uri serviceUri, string action, string clientIdentity, string negotiationType)
        {
            try
            {
                if (auditLogLocation == AuditLogLocation.Default)
                {
                    auditLogLocation = IsSecurityAuditSupported ? AuditLogLocation.Security : AuditLogLocation.Application;
                }
                string activityId = GetActivityId();
                if (auditLogLocation == AuditLogLocation.Application)
                {
                    WriteEventToApplicationLog(new EventInstance(0x40060005L, 2, EventLogEntryType.Information), new object[] { serviceUri.AbsoluteUri, action, clientIdentity, activityId, negotiationType });
                }
                else
                {
                    if (auditLogLocation != AuditLogLocation.Security)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("auditLogLocation", System.ServiceModel.SR.GetString("SecurityAuditPlatformNotSupported")));
                    }
                    WriteAuditEvent(1, 0x40060005, new string[] { serviceUri.AbsoluteUri, action, clientIdentity, activityId, negotiationType });
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x70053, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenSuccess"), new SecurityAuditTraceRecord(auditLogLocation, "SecurityNegotiationSuccess"), null, null, message);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x70054, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenFailure"), new SecurityAuditTraceRecord(auditLogLocation, "SecurityNegotiationSuccess"), null, exception, message);
                }
                if (!suppressAuditFailure)
                {
                    throw;
                }
            }
        }

        public static void WriteServiceAuthorizationFailureEvent(AuditLogLocation auditLogLocation, bool suppressAuditFailure, Message message, Uri serviceUri, string action, string clientIdentity, string authContextId, string serviceAuthorizationManager, Exception exception)
        {
            try
            {
                if (auditLogLocation == AuditLogLocation.Default)
                {
                    auditLogLocation = IsSecurityAuditSupported ? AuditLogLocation.Security : AuditLogLocation.Application;
                }
                string activityId = GetActivityId();
                if (auditLogLocation == AuditLogLocation.Application)
                {
                    WriteEventToApplicationLog(new EventInstance(0xc0060002L, 1, EventLogEntryType.Error), new object[] { serviceUri.AbsoluteUri, action, clientIdentity, authContextId, activityId, serviceAuthorizationManager, ExceptionToString(exception) });
                }
                else
                {
                    if (auditLogLocation != AuditLogLocation.Security)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("auditLogLocation", System.ServiceModel.SR.GetString("SecurityAuditPlatformNotSupported")));
                    }
                    WriteAuditEvent(0, 0xc0060002, new string[] { serviceUri.AbsoluteUri, action, clientIdentity, authContextId, activityId, serviceAuthorizationManager, ExceptionToString(exception) });
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x70053, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenSuccess"), new SecurityAuditTraceRecord(auditLogLocation, "ServiceAuthorizationFailure"), null, null, message);
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x70054, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenFailure"), new SecurityAuditTraceRecord(auditLogLocation, "ServiceAuthorizationFailure"), null, exception2, message);
                }
                if (!suppressAuditFailure)
                {
                    throw;
                }
            }
        }

        public static void WriteServiceAuthorizationSuccessEvent(AuditLogLocation auditLogLocation, bool suppressAuditFailure, Message message, Uri serviceUri, string action, string clientIdentity, string authContextId, string serviceAuthorizationManager)
        {
            try
            {
                if (auditLogLocation == AuditLogLocation.Default)
                {
                    auditLogLocation = IsSecurityAuditSupported ? AuditLogLocation.Security : AuditLogLocation.Application;
                }
                string activityId = GetActivityId();
                if (auditLogLocation == AuditLogLocation.Application)
                {
                    WriteEventToApplicationLog(new EventInstance(0x40060001L, 1, EventLogEntryType.Information), new object[] { serviceUri.AbsoluteUri, action, clientIdentity, authContextId, activityId, serviceAuthorizationManager });
                }
                else
                {
                    if (auditLogLocation != AuditLogLocation.Security)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("auditLogLocation", System.ServiceModel.SR.GetString("SecurityAuditPlatformNotSupported")));
                    }
                    WriteAuditEvent(1, 0x40060001, new string[] { serviceUri.AbsoluteUri, action, clientIdentity, authContextId, activityId, serviceAuthorizationManager });
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x70053, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenSuccess"), new SecurityAuditTraceRecord(auditLogLocation, "ServiceAuthorizationSuccess"), null, null, message);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x70054, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenFailure"), new SecurityAuditTraceRecord(auditLogLocation, "ServiceAuthorizationSuccess"), null, exception, message);
                }
                if (!suppressAuditFailure)
                {
                    throw;
                }
            }
        }

        public static void WriteTransportAuthenticationFailureEvent(AuditLogLocation auditLogLocation, bool suppressAuditFailure, Message message, Uri serviceUri, string clientIdentity, Exception exception)
        {
            try
            {
                if (auditLogLocation == AuditLogLocation.Default)
                {
                    auditLogLocation = IsSecurityAuditSupported ? AuditLogLocation.Security : AuditLogLocation.Application;
                }
                string activityId = GetActivityId();
                if (auditLogLocation == AuditLogLocation.Application)
                {
                    WriteEventToApplicationLog(new EventInstance(0xc0060008L, 2, EventLogEntryType.Error), new object[] { serviceUri.AbsoluteUri, clientIdentity, activityId, ExceptionToString(exception) });
                }
                else
                {
                    if (auditLogLocation != AuditLogLocation.Security)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("auditLogLocation", System.ServiceModel.SR.GetString("SecurityAuditPlatformNotSupported")));
                    }
                    WriteAuditEvent(0, 0xc0060008, new string[] { serviceUri.AbsoluteUri, clientIdentity, activityId, ExceptionToString(exception) });
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x70053, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenSuccess"), new SecurityAuditTraceRecord(auditLogLocation, "TransportAuthenticationFailure"), null, null, message);
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x70054, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenFailure"), new SecurityAuditTraceRecord(auditLogLocation, "TransportAuthenticationFailure"), null, exception2, message);
                }
                if (!suppressAuditFailure)
                {
                    throw;
                }
            }
        }

        public static void WriteTransportAuthenticationSuccessEvent(AuditLogLocation auditLogLocation, bool suppressAuditFailure, Message message, Uri serviceUri, string clientIdentity)
        {
            try
            {
                if (auditLogLocation == AuditLogLocation.Default)
                {
                    auditLogLocation = IsSecurityAuditSupported ? AuditLogLocation.Security : AuditLogLocation.Application;
                }
                string activityId = GetActivityId();
                if (auditLogLocation == AuditLogLocation.Application)
                {
                    WriteEventToApplicationLog(new EventInstance(0x40060007L, 2, EventLogEntryType.Information), new object[] { serviceUri.AbsoluteUri, clientIdentity, activityId });
                }
                else
                {
                    if (auditLogLocation != AuditLogLocation.Security)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("auditLogLocation", System.ServiceModel.SR.GetString("SecurityAuditPlatformNotSupported")));
                    }
                    WriteAuditEvent(1, 0x40060007, new string[] { serviceUri.AbsoluteUri, clientIdentity, activityId });
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x70053, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenSuccess"), new SecurityAuditTraceRecord(auditLogLocation, "TransportAuthenticationSuccess"), null, null, message);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x70054, System.ServiceModel.SR.GetString("TraceCodeSecurityAuditWrittenFailure"), new SecurityAuditTraceRecord(auditLogLocation, "TransportAuthenticationSuccess"), null, exception, message);
                }
                if (!suppressAuditFailure)
                {
                    throw;
                }
            }
        }

        public static bool IsSecurityAuditSupported
        {
            get
            {
                if (authzModule == null)
                {
                    lock (typeof(SafeLoadLibraryHandle))
                    {
                        SafeLoadLibraryHandle handle = SafeLoadLibraryHandle.LoadLibraryEx(Environment.SystemDirectory + @"\authz.dll");
                        isSecurityAuditSupported = handle.IsProcNameExist("AuthzInstallSecurityEventSource");
                        authzModule = handle;
                    }
                }
                return isSecurityAuditSupported;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            public const string ADVAPI32 = "advapi32.dll";
            public const string AUTHZ = "authz.dll";
            public const string KERNEL32 = "kernel32.dll";

            [DllImport("authz.dll", CharSet=CharSet.Unicode, SetLastError=true)]
            public static extern bool AuthzRegisterSecurityEventSource([In] uint dwFlags, [In] string szEventSourceName, out SecurityAuditHelper.SafeSecurityAuditHandle phEventProvider);
            [DllImport("authz.dll", CharSet=CharSet.Auto, SetLastError=true)]
            public static extern bool AuthzReportSecurityEventFromParams([In] uint dwFlags, [In] SecurityAuditHelper.SafeSecurityAuditHandle providerHandle, [In] uint auditId, [In] byte[] securityIdentifier, [In] ref AUDIT_PARAMS auditParams);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("authz.dll", CharSet=CharSet.Auto, SetLastError=true)]
            public static extern bool AuthzUnregisterSecurityEventSource([In] uint dwFlags, [In, Out] ref IntPtr providerHandle);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true)]
            public static extern bool CloseHandle([In] IntPtr handle);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
            public static extern bool FreeLibrary([In] IntPtr hModule);
            [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
            public static extern IntPtr GetProcAddress([In] SecurityAuditHelper.SafeLoadLibraryHandle hModule, [In, MarshalAs(UnmanagedType.LPStr)] string lpProcName);
            [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            public static extern SecurityAuditHelper.SafeLoadLibraryHandle LoadLibraryExW([In] string lpwLibFileName, [In] IntPtr hFile, [In] uint dwFlags);

            [StructLayout(LayoutKind.Sequential)]
            public struct AUDIT_PARAM
            {
                public SecurityAuditHelper.NativeMethods.AUDIT_PARAM_TYPE Type;
                public uint Length;
                public uint Flags;
                public IntPtr Data0;
                public IntPtr Data1;
                public static readonly int Size;
                static AUDIT_PARAM()
                {
                    Size = Marshal.SizeOf(typeof(SecurityAuditHelper.NativeMethods.AUDIT_PARAM));
                }
            }

            public enum AUDIT_PARAM_TYPE
            {
                APT_String = 2
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct AUDIT_PARAMS
            {
                public uint Length;
                public uint Flags;
                public ushort Count;
                public SecurityAuditHelper.SafeHGlobalHandle Parameters;
            }
        }

        private class SafeHGlobalHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeHGlobalHandle() : base(true)
            {
            }

            public static SecurityAuditHelper.SafeHGlobalHandle AllocHGlobal(byte[] bytes)
            {
                SecurityAuditHelper.SafeHGlobalHandle handle = AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, 0, handle.DangerousGetHandle(), bytes.Length);
                return handle;
            }

            public static SecurityAuditHelper.SafeHGlobalHandle AllocHGlobal(int cb)
            {
                SecurityAuditHelper.SafeHGlobalHandle handle = new SecurityAuditHelper.SafeHGlobalHandle();
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    IntPtr ptr = Marshal.AllocHGlobal(cb);
                    handle.SetHandle(ptr);
                }
                return handle;
            }

            public static SecurityAuditHelper.SafeHGlobalHandle AllocHGlobal(string s)
            {
                byte[] bytes = System.ServiceModel.DiagnosticUtility.Utility.AllocateByteArray((s.Length + 1) * 2);
                Encoding.Unicode.GetBytes(s, 0, s.Length, bytes, 0);
                return AllocHGlobal(bytes);
            }

            protected override bool ReleaseHandle()
            {
                Marshal.FreeHGlobal(base.handle);
                return true;
            }
        }

        private class SafeLoadLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeLoadLibraryHandle() : base(true)
            {
            }

            public bool IsProcNameExist(string procName)
            {
                if (!this.IsInvalid)
                {
                    try
                    {
                        return (IntPtr.Zero != SecurityAuditHelper.NativeMethods.GetProcAddress(this, procName));
                    }
                    catch (ObjectDisposedException exception)
                    {
                        if (System.ServiceModel.DiagnosticUtility.ShouldTraceInformation)
                        {
                            System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                        }
                        return false;
                    }
                }
                return false;
            }

            public static SecurityAuditHelper.SafeLoadLibraryHandle LoadLibraryEx(string library)
            {
                SecurityAuditHelper.SafeLoadLibraryHandle handle = SecurityAuditHelper.NativeMethods.LoadLibraryExW(library, IntPtr.Zero, 0);
                int error = Marshal.GetLastWin32Error();
                if (handle.IsInvalid)
                {
                    handle.SetHandleAsInvalid();
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, System.ServiceModel.SR.GetString("SecurityAuditFailToLoadDll", new object[] { library })));
                }
                return handle;
            }

            protected override bool ReleaseHandle()
            {
                return SecurityAuditHelper.NativeMethods.FreeLibrary(base.handle);
            }
        }

        private class SafeSecurityAuditHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeSecurityAuditHandle() : base(true)
            {
            }

            protected override bool ReleaseHandle()
            {
                return SecurityAuditHelper.NativeMethods.AuthzUnregisterSecurityEventSource(0, ref this.handle);
            }
        }

        private class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeTokenHandle() : base(true)
            {
            }

            protected override bool ReleaseHandle()
            {
                return SecurityAuditHelper.NativeMethods.CloseHandle(base.handle);
            }
        }

        private class SecurityAuditTraceRecord : TraceRecord
        {
            private AuditLogLocation auditLogLocation;
            private string auditType;

            internal SecurityAuditTraceRecord(AuditLogLocation auditLogLocation, string auditType)
            {
                this.auditLogLocation = auditLogLocation;
                this.auditType = auditType;
            }

            internal override void WriteTo(XmlWriter writer)
            {
                writer.WriteElementString("AuditLogLocation", this.auditLogLocation.ToString());
                writer.WriteElementString("AuditType", this.auditType);
            }

            internal override string EventId
            {
                get
                {
                    return base.BuildEventId("SecurityAudit");
                }
            }
        }
    }
}

