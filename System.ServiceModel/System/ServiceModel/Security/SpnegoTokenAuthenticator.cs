namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Net;
    using System.Runtime;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal sealed class SpnegoTokenAuthenticator : SspiNegotiationTokenAuthenticator
    {
        private bool allowUnauthenticatedCallers;
        private System.IdentityModel.SafeFreeCredentials credentialsHandle;
        private bool extractGroupsForWindowsAccounts;
        private NetworkCredential serverCredential;

        protected override SspiNegotiationTokenAuthenticatorState CreateSspiState(byte[] incomingBlob, string incomingValueTypeUri)
        {
            return new SspiNegotiationTokenAuthenticatorState(new WindowsSspiNegotiation("Negotiate", this.credentialsHandle, base.DefaultServiceBinding));
        }

        private void FreeCredentialsHandle()
        {
            if (this.credentialsHandle != null)
            {
                this.credentialsHandle.Close();
                this.credentialsHandle = null;
            }
        }

        public override void OnAbort()
        {
            base.OnAbort();
            this.FreeCredentialsHandle();
        }

        public override void OnClose(TimeSpan timeout)
        {
            base.OnClose(timeout);
            this.FreeCredentialsHandle();
        }

        public override void OnOpening()
        {
            base.OnOpening();
            if (this.credentialsHandle == null)
            {
                this.credentialsHandle = System.ServiceModel.Security.SecurityUtils.GetCredentialsHandle("Negotiate", this.serverCredential, true, new string[0]);
            }
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateSspiNegotiation(ISspiNegotiation sspiNegotiation)
        {
            WindowsSspiNegotiation windowsNegotiation = (WindowsSspiNegotiation) sspiNegotiation;
            if (!windowsNegotiation.IsValidContext)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(System.ServiceModel.SR.GetString("InvalidSspiNegotiation")));
            }
            SecurityTraceRecordHelper.TraceServiceSpnego(windowsNegotiation);
            if (base.IsClientAnonymous)
            {
                return System.ServiceModel.Security.EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }
            using (System.IdentityModel.SafeCloseHandle handle = windowsNegotiation.GetContextToken())
            {
                WindowsIdentity identity = new WindowsIdentity(handle.DangerousGetHandle(), windowsNegotiation.ProtocolName);
                System.ServiceModel.Security.SecurityUtils.ValidateAnonymityConstraint(identity, this.AllowUnauthenticatedCallers);
                List<IAuthorizationPolicy> list = new List<IAuthorizationPolicy>(1);
                WindowsClaimSet issuance = new WindowsClaimSet(identity, windowsNegotiation.ProtocolName, this.extractGroupsForWindowsAccounts, false);
                list.Add(new UnconditionalPolicy(issuance, TimeoutHelper.Add(DateTime.UtcNow, base.ServiceTokenLifetime)));
                return list.AsReadOnly();
            }
        }

        public bool AllowUnauthenticatedCallers
        {
            get
            {
                return this.allowUnauthenticatedCallers;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.allowUnauthenticatedCallers = value;
            }
        }

        public bool ExtractGroupsForWindowsAccounts
        {
            get
            {
                return this.extractGroupsForWindowsAccounts;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.extractGroupsForWindowsAccounts = value;
            }
        }

        public override XmlDictionaryString NegotiationValueType
        {
            get
            {
                return System.ServiceModel.XD.TrustApr2004Dictionary.SpnegoValueTypeUri;
            }
        }

        public NetworkCredential ServerCredential
        {
            get
            {
                return this.serverCredential;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.serverCredential = value;
            }
        }
    }
}

