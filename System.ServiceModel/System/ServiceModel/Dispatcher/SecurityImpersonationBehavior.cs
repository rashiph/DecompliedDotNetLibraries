namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.ComIntegration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Threading;

    internal sealed class SecurityImpersonationBehavior
    {
        private static WindowsPrincipal anonymousWindowsPrincipal;
        private AuditLevel auditLevel;
        private AuditLogLocation auditLogLocation;
        private Dictionary<string, string> domainNameMap;
        private bool impersonateCallerForAllOperations;
        private const int maxDomainNameMapSize = 5;
        private PrincipalPermissionMode principalPermissionMode;
        private Random random;
        private object roleProvider;
        private bool suppressAuditFailure = true;

        private SecurityImpersonationBehavior(DispatchRuntime dispatch)
        {
            this.principalPermissionMode = dispatch.PrincipalPermissionMode;
            this.impersonateCallerForAllOperations = dispatch.ImpersonateCallerForAllOperations;
            this.auditLevel = dispatch.MessageAuthenticationAuditLevel;
            this.auditLogLocation = dispatch.SecurityAuditLogLocation;
            this.suppressAuditFailure = dispatch.SuppressAuditFailure;
            if (dispatch.IsRoleProviderSet)
            {
                this.ApplyRoleProvider(dispatch);
            }
            this.domainNameMap = new Dictionary<string, string>(5, StringComparer.OrdinalIgnoreCase);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ApplyRoleProvider(DispatchRuntime dispatch)
        {
            this.roleProvider = dispatch.RoleProvider;
        }

        public static SecurityImpersonationBehavior CreateIfNecessary(DispatchRuntime dispatch)
        {
            if (IsSecurityBehaviorNeeded(dispatch))
            {
                return new SecurityImpersonationBehavior(dispatch);
            }
            return null;
        }

        private ServiceSecurityContext GetAndCacheSecurityContext(ref MessageRpc rpc)
        {
            ServiceSecurityContext securityContext = rpc.SecurityContext;
            if (!rpc.HasSecurityContext)
            {
                SecurityMessageProperty security = rpc.Request.Properties.Security;
                if (security == null)
                {
                    securityContext = null;
                }
                else
                {
                    securityContext = security.ServiceSecurityContext;
                    if (securityContext == null)
                    {
                        throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityContextMissing", new object[] { rpc.Operation.Name })), rpc.Request);
                    }
                }
                rpc.SecurityContext = securityContext;
                rpc.HasSecurityContext = true;
            }
            return securityContext;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IPrincipal GetCustomPrincipal(ServiceSecurityContext securityContext)
        {
            object obj2;
            if (!securityContext.AuthorizationContext.Properties.TryGetValue("Principal", out obj2) || !(obj2 is IPrincipal))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoPrincipalSpecifiedInAuthorizationContext")));
            }
            return (IPrincipal) obj2;
        }

        private string GetUpnFromDownlevelName(string downlevelName)
        {
            string str3;
            bool flag;
            if (downlevelName == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("downlevelName");
            }
            int index = downlevelName.IndexOf('\\');
            if (((index < 0) || (index == 0)) || (index == (downlevelName.Length - 1)))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.ServiceModel.SR.GetString("DownlevelNameCannotMapToUpn", new object[] { downlevelName })));
            }
            string key = downlevelName.Substring(0, index + 1);
            string str2 = downlevelName.Substring(index + 1);
            lock (this.domainNameMap)
            {
                flag = this.domainNameMap.TryGetValue(key, out str3);
            }
            if (!flag)
            {
                uint size = 50;
                StringBuilder outputString = new StringBuilder((int) size);
                if (!SafeNativeMethods.TranslateName(key, EXTENDED_NAME_FORMAT.NameSamCompatible, EXTENDED_NAME_FORMAT.NameCanonical, outputString, out size))
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != 0x7a)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.ServiceModel.SR.GetString("DownlevelNameCannotMapToUpn", new object[] { downlevelName }), new Win32Exception(error)));
                    }
                    outputString = new StringBuilder((int) size);
                    if (!SafeNativeMethods.TranslateName(key, EXTENDED_NAME_FORMAT.NameSamCompatible, EXTENDED_NAME_FORMAT.NameCanonical, outputString, out size))
                    {
                        error = Marshal.GetLastWin32Error();
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.ServiceModel.SR.GetString("DownlevelNameCannotMapToUpn", new object[] { downlevelName }), new Win32Exception(error)));
                    }
                }
                str3 = outputString.Remove(outputString.Length - 1, 1).ToString();
                lock (this.domainNameMap)
                {
                    if (this.domainNameMap.Count >= 5)
                    {
                        if (this.random == null)
                        {
                            this.random = new Random((int) DateTime.Now.Ticks);
                        }
                        int num4 = this.random.Next() % this.domainNameMap.Count;
                        foreach (string str4 in this.domainNameMap.Keys)
                        {
                            if (num4 <= 0)
                            {
                                this.domainNameMap.Remove(str4);
                                break;
                            }
                            num4--;
                        }
                    }
                    this.domainNameMap[key] = str3;
                }
            }
            return (str2 + "@" + str3);
        }

        private IPrincipal GetWindowsPrincipal(ServiceSecurityContext securityContext)
        {
            WindowsIdentity windowsIdentity = securityContext.WindowsIdentity;
            if (!windowsIdentity.IsAnonymous)
            {
                return new WindowsPrincipal(windowsIdentity);
            }
            WindowsSidIdentity primaryIdentity = securityContext.PrimaryIdentity as WindowsSidIdentity;
            if (primaryIdentity != null)
            {
                return new WindowsSidPrincipal(primaryIdentity, securityContext);
            }
            return AnonymousWindowsPrincipal;
        }

        private static bool IsSecurityBehaviorNeeded(DispatchRuntime dispatch)
        {
            if (AspNetEnvironment.Current.RequiresImpersonation)
            {
                return true;
            }
            if (dispatch.PrincipalPermissionMode != PrincipalPermissionMode.None)
            {
                return true;
            }
            for (int i = 0; i < dispatch.Operations.Count; i++)
            {
                DispatchOperation operation = dispatch.Operations[i];
                if (operation.Impersonation == ImpersonationOption.Required)
                {
                    return true;
                }
                if (operation.Impersonation == ImpersonationOption.NotAllowed)
                {
                    return false;
                }
            }
            return dispatch.ImpersonateCallerForAllOperations;
        }

        private bool IsSecurityContextImpersonationRequired(ref MessageRpc rpc)
        {
            return ((rpc.Operation.Impersonation == ImpersonationOption.Required) || ((rpc.Operation.Impersonation == ImpersonationOption.Allowed) && this.impersonateCallerForAllOperations));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IPrincipal SetCurrentThreadPrincipal(ServiceSecurityContext securityContext, out bool isThreadPrincipalSet)
        {
            IPrincipal currentPrincipal = null;
            IPrincipal windowsPrincipal = null;
            if (this.principalPermissionMode == PrincipalPermissionMode.UseWindowsGroups)
            {
                windowsPrincipal = this.GetWindowsPrincipal(securityContext);
            }
            else if (this.principalPermissionMode == PrincipalPermissionMode.UseAspNetRoles)
            {
                windowsPrincipal = new RoleProviderPrincipal(this.roleProvider, securityContext);
            }
            else if (this.principalPermissionMode == PrincipalPermissionMode.Custom)
            {
                windowsPrincipal = GetCustomPrincipal(securityContext);
            }
            if (windowsPrincipal != null)
            {
                currentPrincipal = Thread.CurrentPrincipal;
                Thread.CurrentPrincipal = windowsPrincipal;
                isThreadPrincipalSet = true;
                return currentPrincipal;
            }
            isThreadPrincipalSet = false;
            return currentPrincipal;
        }

        [SecurityCritical]
        public void StartImpersonation(ref MessageRpc rpc, out IDisposable impersonationContext, out IPrincipal originalPrincipal, out bool isThreadPrincipalSet)
        {
            ServiceSecurityContext andCacheSecurityContext;
            impersonationContext = null;
            originalPrincipal = null;
            isThreadPrincipalSet = false;
            bool flag = this.principalPermissionMode != PrincipalPermissionMode.None;
            bool isSecurityContextImpersonationOn = this.IsSecurityContextImpersonationRequired(ref rpc);
            if (flag || isSecurityContextImpersonationOn)
            {
                andCacheSecurityContext = this.GetAndCacheSecurityContext(ref rpc);
            }
            else
            {
                andCacheSecurityContext = null;
            }
            if (flag && (andCacheSecurityContext != null))
            {
                originalPrincipal = this.SetCurrentThreadPrincipal(andCacheSecurityContext, out isThreadPrincipalSet);
            }
            if (isSecurityContextImpersonationOn || AspNetEnvironment.Current.RequiresImpersonation)
            {
                impersonationContext = this.StartImpersonation2(ref rpc, andCacheSecurityContext, isSecurityContextImpersonationOn);
            }
        }

        [SecurityCritical]
        private IDisposable StartImpersonation2(ref MessageRpc rpc, ServiceSecurityContext securityContext, bool isSecurityContextImpersonationOn)
        {
            IDisposable disposable = null;
            try
            {
                if (isSecurityContextImpersonationOn)
                {
                    if (securityContext == null)
                    {
                        throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxSecurityContextPropertyMissingFromRequestMessage")), rpc.Request);
                    }
                    WindowsIdentity windowsIdentity = securityContext.WindowsIdentity;
                    if (windowsIdentity.User == null)
                    {
                        if (securityContext.PrimaryIdentity is WindowsSidIdentity)
                        {
                            WindowsSidIdentity primaryIdentity = (WindowsSidIdentity) securityContext.PrimaryIdentity;
                            if (primaryIdentity.SecurityIdentifier.IsWellKnown(WellKnownSidType.AnonymousSid))
                            {
                                disposable = new WindowsAnonymousIdentity().Impersonate();
                                goto Label_0103;
                            }
                            using (WindowsIdentity identity3 = new WindowsIdentity(this.GetUpnFromDownlevelName(primaryIdentity.Name), "Kerberos"))
                            {
                                disposable = identity3.Impersonate();
                                goto Label_0103;
                            }
                        }
                        throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityContextDoesNotAllowImpersonation", new object[] { rpc.Operation.Action })), rpc.Request);
                    }
                    disposable = windowsIdentity.Impersonate();
                }
                else if (AspNetEnvironment.Current.RequiresImpersonation && (rpc.HostingProperty != null))
                {
                    disposable = rpc.HostingProperty.Impersonate();
                }
            Label_0103:
                SecurityTraceRecordHelper.TraceImpersonationSucceeded(rpc.Operation);
                if (AuditLevel.Success == (this.auditLevel & AuditLevel.Success))
                {
                    SecurityAuditHelper.WriteImpersonationSuccessEvent(this.auditLogLocation, this.suppressAuditFailure, rpc.Operation.Name, System.ServiceModel.Security.SecurityUtils.GetIdentityNamesFromContext(securityContext.AuthorizationContext));
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                SecurityTraceRecordHelper.TraceImpersonationFailed(rpc.Operation, exception);
                if (AuditLevel.Failure == (this.auditLevel & AuditLevel.Failure))
                {
                    try
                    {
                        string identityNamesFromContext;
                        if (securityContext != null)
                        {
                            identityNamesFromContext = System.ServiceModel.Security.SecurityUtils.GetIdentityNamesFromContext(securityContext.AuthorizationContext);
                        }
                        else
                        {
                            identityNamesFromContext = System.ServiceModel.Security.SecurityUtils.AnonymousIdentity.Name;
                        }
                        SecurityAuditHelper.WriteImpersonationFailureEvent(this.auditLogLocation, this.suppressAuditFailure, rpc.Operation.Name, identityNamesFromContext, exception);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Error);
                    }
                }
                throw;
            }
            return disposable;
        }

        public void StopImpersonation(ref MessageRpc rpc, IDisposable impersonationContext, IPrincipal originalPrincipal, bool isThreadPrincipalSet)
        {
            try
            {
                if ((this.IsSecurityContextImpersonationRequired(ref rpc) || AspNetEnvironment.Current.RequiresImpersonation) && (impersonationContext != null))
                {
                    impersonationContext.Dispose();
                }
                if (isThreadPrincipalSet)
                {
                    Thread.CurrentPrincipal = originalPrincipal;
                }
            }
            catch
            {
                string message = null;
                try
                {
                    message = System.ServiceModel.SR.GetString("SFxRevertImpersonationFailed0");
                }
                finally
                {
                    System.ServiceModel.DiagnosticUtility.FailFast(message);
                }
            }
        }

        private static WindowsPrincipal AnonymousWindowsPrincipal
        {
            get
            {
                if (anonymousWindowsPrincipal == null)
                {
                    anonymousWindowsPrincipal = new WindowsPrincipal(WindowsIdentity.GetAnonymous());
                }
                return anonymousWindowsPrincipal;
            }
        }

        private class WindowsAnonymousIdentity
        {
            public IDisposable Impersonate()
            {
                System.IdentityModel.SafeCloseHandle handle;
                IntPtr currentThread = SafeNativeMethods.GetCurrentThread();
                if (!SafeNativeMethods.OpenCurrentThreadToken(currentThread, TokenAccessLevels.Impersonate, true, out handle))
                {
                    int error = Marshal.GetLastWin32Error();
                    System.ServiceModel.Diagnostics.Utility.CloseInvalidOutSafeHandle(handle);
                    if (error != 0x3f0)
                    {
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                    }
                    handle = new System.IdentityModel.SafeCloseHandle(IntPtr.Zero, false);
                }
                if (!SafeNativeMethods.ImpersonateAnonymousUserOnCurrentThread(currentThread))
                {
                    int num2 = Marshal.GetLastWin32Error();
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(num2));
                }
                return new ImpersonationContext(currentThread, handle);
            }

            private class ImpersonationContext : IDisposable
            {
                private bool disposed;
                private IntPtr threadHandle;
                private System.IdentityModel.SafeCloseHandle tokenHandle;

                public ImpersonationContext(IntPtr threadHandle, System.IdentityModel.SafeCloseHandle tokenHandle)
                {
                    this.threadHandle = threadHandle;
                    this.tokenHandle = tokenHandle;
                }

                public void Dispose()
                {
                    if (!this.disposed)
                    {
                        this.Undo();
                    }
                    this.disposed = true;
                }

                private void Undo()
                {
                    if (!SafeNativeMethods.SetCurrentThreadToken(IntPtr.Zero, this.tokenHandle))
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.ServiceModel.SR.GetString("RevertImpersonationFailure", new object[] { new Win32Exception(error).Message })));
                    }
                    this.tokenHandle.Close();
                }
            }
        }

        private class WindowsSidPrincipal : IPrincipal
        {
            private WindowsSidIdentity identity;
            private ServiceSecurityContext securityContext;

            public WindowsSidPrincipal(WindowsSidIdentity identity, ServiceSecurityContext securityContext)
            {
                this.identity = identity;
                this.securityContext = securityContext;
            }

            public bool IsInRole(string role)
            {
                if (role == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("role");
                }
                NTAccount account = new NTAccount(role);
                Claim claim = Claim.CreateWindowsSidClaim((SecurityIdentifier) account.Translate(typeof(SecurityIdentifier)));
                AuthorizationContext authorizationContext = this.securityContext.AuthorizationContext;
                for (int i = 0; i < authorizationContext.ClaimSets.Count; i++)
                {
                    ClaimSet set = authorizationContext.ClaimSets[i];
                    if (set.ContainsClaim(claim))
                    {
                        return true;
                    }
                }
                return false;
            }

            public IIdentity Identity
            {
                get
                {
                    return this.identity;
                }
            }
        }
    }
}

