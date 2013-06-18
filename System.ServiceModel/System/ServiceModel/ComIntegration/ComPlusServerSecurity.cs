namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;
    using System.ServiceModel;

    internal sealed class ComPlusServerSecurity : IContextSecurityPerimeter, IServerSecurity, IDisposable
    {
        private WindowsIdentity clientIdentity;
        private WindowsImpersonationContext impersonateContext;
        private bool isImpersonating;
        private IntPtr oldSecurityObject = IntPtr.Zero;
        private const uint RPC_C_AUTHN_DEFAULT = uint.MaxValue;
        private const uint RPC_C_AUTHN_GSS_KERBEROS = 0x10;
        private const uint RPC_C_AUTHN_GSS_NEGOTIATE = 9;
        private const uint RPC_C_AUTHN_LEVEL_CALL = 3;
        private const uint RPC_C_AUTHN_LEVEL_CONNECT = 2;
        private const uint RPC_C_AUTHN_LEVEL_DEFAULT = 0;
        private const uint RPC_C_AUTHN_LEVEL_NONE = 1;
        private const uint RPC_C_AUTHN_LEVEL_PKT = 4;
        private const uint RPC_C_AUTHN_LEVEL_PKT_INTEGRITY = 5;
        private const uint RPC_C_AUTHN_LEVEL_PKT_PRIVACY = 6;
        private const uint RPC_C_AUTHN_WINNT = 10;
        private const uint RPC_C_AUTHZ_NONE = 0;
        private bool shouldUseCallContext;

        public ComPlusServerSecurity(WindowsIdentity clientIdentity, bool shouldUseCallContext)
        {
            if (clientIdentity == null)
            {
                throw Fx.AssertAndThrow("NULL Identity");
            }
            if (IntPtr.Zero == clientIdentity.Token)
            {
                throw Fx.AssertAndThrow("Token handle cannot be zero");
            }
            this.shouldUseCallContext = shouldUseCallContext;
            this.clientIdentity = clientIdentity;
            IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(this);
            try
            {
                this.oldSecurityObject = SafeNativeMethods.CoSwitchCallContext(iUnknownForObject);
            }
            catch
            {
                Marshal.Release(iUnknownForObject);
                throw;
            }
        }

        public void Dispose(bool disposing)
        {
            this.RevertToSelf();
            IntPtr pUnk = SafeNativeMethods.CoSwitchCallContext(this.oldSecurityObject);
            if (IntPtr.Zero == pUnk)
            {
                DiagnosticUtility.FailFast("Security Context was should not be null");
            }
            if (Marshal.GetObjectForIUnknown(pUnk) != this)
            {
                DiagnosticUtility.FailFast("Security Context was modified from underneath us");
            }
            Marshal.Release(pUnk);
            if (disposing)
            {
                this.clientIdentity = null;
                if (this.impersonateContext != null)
                {
                    this.impersonateContext.Dispose();
                }
            }
        }

        ~ComPlusServerSecurity()
        {
            this.Dispose(false);
        }

        public bool GetPerimeterFlag()
        {
            return this.shouldUseCallContext;
        }

        public int ImpersonateClient()
        {
            int num = HR.E_FAIL;
            try
            {
                this.impersonateContext = WindowsIdentity.Impersonate(this.clientIdentity.Token);
                this.isImpersonating = true;
                num = HR.S_OK;
            }
            catch (SecurityException)
            {
                num = HR.RPC_NT_BINDING_HAS_NO_AUTH;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
            }
            return num;
        }

        public bool IsImpersonating()
        {
            return this.isImpersonating;
        }

        public void QueryBlanket(IntPtr authnSvc, IntPtr authzSvc, IntPtr serverPrincipalName, IntPtr authnLevel, IntPtr impLevel, IntPtr clientPrincipalName, IntPtr Capabilities)
        {
            if (authnSvc != IntPtr.Zero)
            {
                uint maxValue = uint.MaxValue;
                string authenticationType = this.clientIdentity.AuthenticationType;
                if (authenticationType.ToUpperInvariant() == "NTLM")
                {
                    maxValue = 10;
                }
                else if (authenticationType.ToUpperInvariant() == "KERBEROS")
                {
                    maxValue = 0x10;
                }
                else if (authenticationType.ToUpperInvariant() == "NEGOTIATE")
                {
                    maxValue = 9;
                }
                Marshal.WriteInt32(authnSvc, (int) maxValue);
            }
            if (authzSvc != IntPtr.Zero)
            {
                Marshal.WriteInt32(authzSvc, 0);
            }
            if (serverPrincipalName != IntPtr.Zero)
            {
                IntPtr val = Marshal.StringToCoTaskMemUni(System.ServiceModel.ComIntegration.SecurityUtils.GetProcessIdentity().Name);
                Marshal.WriteIntPtr(serverPrincipalName, val);
            }
            if (authnLevel != IntPtr.Zero)
            {
                Marshal.WriteInt32(authnLevel, 0);
            }
            if (impLevel != IntPtr.Zero)
            {
                Marshal.WriteInt32(impLevel, 0);
            }
            if (clientPrincipalName != IntPtr.Zero)
            {
                IntPtr ptr2 = Marshal.StringToCoTaskMemUni(this.clientIdentity.Name);
                Marshal.WriteIntPtr(clientPrincipalName, ptr2);
            }
            if (Capabilities != IntPtr.Zero)
            {
                Marshal.WriteInt32(Capabilities, 0);
            }
        }

        public int RevertToSelf()
        {
            int num = HR.E_FAIL;
            if (this.isImpersonating)
            {
                try
                {
                    this.impersonateContext.Undo();
                    this.isImpersonating = false;
                    num = HR.S_OK;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                }
            }
            return num;
        }

        public void SetPerimeterFlag(bool flag)
        {
            this.shouldUseCallContext = flag;
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

