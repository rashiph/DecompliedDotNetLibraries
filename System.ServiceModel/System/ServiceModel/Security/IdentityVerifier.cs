namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security.Tokens;

    public abstract class IdentityVerifier
    {
        protected IdentityVerifier()
        {
        }

        private static void AdjustAddress(ref EndpointAddress reference, Uri via)
        {
            if ((reference.Identity == null) && (reference.Uri != via))
            {
                reference = new EndpointAddress(via, new AddressHeader[0]);
            }
        }

        internal bool CheckAccess(EndpointAddress reference, Message message)
        {
            EndpointIdentity identity;
            if (reference == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reference");
            }
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (!this.TryGetIdentity(reference, out identity))
            {
                return false;
            }
            SecurityMessageProperty security = null;
            if (message.Properties != null)
            {
                security = message.Properties.Security;
            }
            return (((security != null) && (security.ServiceSecurityContext != null)) && this.CheckAccess(identity, security.ServiceSecurityContext.AuthorizationContext));
        }

        public abstract bool CheckAccess(EndpointIdentity identity, AuthorizationContext authContext);
        public static IdentityVerifier CreateDefault()
        {
            return DefaultIdentityVerifier.Instance;
        }

        private Exception CreateIdentityCheckException(EndpointIdentity identity, AuthorizationContext authorizationContext, string errorString, EndpointAddress serviceReference)
        {
            if (((identity.IdentityClaim == null) || !(identity.IdentityClaim.ClaimType == ClaimTypes.Dns)) || (!(identity.IdentityClaim.Right == Rights.PossessProperty) || !(identity.IdentityClaim.Resource is string)))
            {
                return new MessageSecurityException(System.ServiceModel.SR.GetString(errorString, new object[] { identity, serviceReference }));
            }
            string resource = (string) identity.IdentityClaim.Resource;
            string str2 = null;
            for (int i = 0; i < authorizationContext.ClaimSets.Count; i++)
            {
                ClaimSet set = authorizationContext.ClaimSets[i];
                foreach (Claim claim in set.FindClaims(ClaimTypes.Dns, Rights.PossessProperty))
                {
                    if (claim.Resource is string)
                    {
                        str2 = (string) claim.Resource;
                        break;
                    }
                }
                if (str2 != null)
                {
                    break;
                }
            }
            if ("IdentityCheckFailedForIncomingMessage".Equals(errorString))
            {
                if (str2 == null)
                {
                    return new MessageSecurityException(System.ServiceModel.SR.GetString("DnsIdentityCheckFailedForIncomingMessageLackOfDnsClaim", new object[] { resource }));
                }
                return new MessageSecurityException(System.ServiceModel.SR.GetString("DnsIdentityCheckFailedForIncomingMessage", new object[] { resource, str2 }));
            }
            if ("IdentityCheckFailedForOutgoingMessage".Equals(errorString))
            {
                if (str2 == null)
                {
                    return new MessageSecurityException(System.ServiceModel.SR.GetString("DnsIdentityCheckFailedForOutgoingMessageLackOfDnsClaim", new object[] { resource }));
                }
                return new MessageSecurityException(System.ServiceModel.SR.GetString("DnsIdentityCheckFailedForOutgoingMessage", new object[] { resource, str2 }));
            }
            return new MessageSecurityException(System.ServiceModel.SR.GetString(errorString, new object[] { identity, serviceReference }));
        }

        private void EnsureIdentity(EndpointAddress serviceReference, AuthorizationContext authorizationContext, string errorString)
        {
            EndpointIdentity identity;
            if (authorizationContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authorizationContext");
            }
            if (!this.TryGetIdentity(serviceReference, out identity))
            {
                SecurityTraceRecordHelper.TraceIdentityVerificationFailure(identity, authorizationContext, base.GetType());
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString(errorString, new object[] { identity, serviceReference })));
            }
            if (!this.CheckAccess(identity, authorizationContext))
            {
                Exception exception = this.CreateIdentityCheckException(identity, authorizationContext, errorString, serviceReference);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(exception);
            }
        }

        internal void EnsureIncomingIdentity(EndpointAddress serviceReference, AuthorizationContext authorizationContext)
        {
            this.EnsureIdentity(serviceReference, authorizationContext, "IdentityCheckFailedForIncomingMessage");
        }

        internal void EnsureOutgoingIdentity(EndpointAddress serviceReference, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            if (authorizationPolicies == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authorizationPolicies");
            }
            AuthorizationContext authorizationContext = AuthorizationContext.CreateDefaultAuthorizationContext(authorizationPolicies);
            this.EnsureIdentity(serviceReference, authorizationContext, "IdentityCheckFailedForOutgoingMessage");
        }

        internal void EnsureOutgoingIdentity(EndpointAddress serviceReference, Uri via, AuthorizationContext authorizationContext)
        {
            AdjustAddress(ref serviceReference, via);
            this.EnsureIdentity(serviceReference, authorizationContext, "IdentityCheckFailedForOutgoingMessage");
        }

        public abstract bool TryGetIdentity(EndpointAddress reference, out EndpointIdentity identity);
        internal bool TryGetIdentity(EndpointAddress reference, Uri via, out EndpointIdentity identity)
        {
            AdjustAddress(ref reference, via);
            return this.TryGetIdentity(reference, out identity);
        }

        private class DefaultIdentityVerifier : IdentityVerifier
        {
            private static readonly IdentityVerifier.DefaultIdentityVerifier instance = new IdentityVerifier.DefaultIdentityVerifier();

            public override bool CheckAccess(EndpointIdentity identity, AuthorizationContext authContext)
            {
                if (identity == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identity");
                }
                if (authContext == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authContext");
                }
                for (int i = 0; i < authContext.ClaimSets.Count; i++)
                {
                    ClaimSet claimSet = authContext.ClaimSets[i];
                    if (claimSet.ContainsClaim(identity.IdentityClaim))
                    {
                        SecurityTraceRecordHelper.TraceIdentityVerificationSuccess(identity, identity.IdentityClaim, base.GetType());
                        return true;
                    }
                    string expectedSpn = null;
                    if (ClaimTypes.Dns.Equals(identity.IdentityClaim.ClaimType))
                    {
                        expectedSpn = string.Format(CultureInfo.InvariantCulture, "host/{0}", new object[] { (string) identity.IdentityClaim.Resource });
                        Claim claim = this.CheckDnsEquivalence(claimSet, expectedSpn);
                        if (claim != null)
                        {
                            SecurityTraceRecordHelper.TraceIdentityVerificationSuccess(identity, claim, base.GetType());
                            return true;
                        }
                    }
                    SecurityIdentifier identitySid = null;
                    if (ClaimTypes.Sid.Equals(identity.IdentityClaim.ClaimType))
                    {
                        identitySid = this.GetSecurityIdentifier(identity.IdentityClaim);
                    }
                    else if (ClaimTypes.Upn.Equals(identity.IdentityClaim.ClaimType))
                    {
                        identitySid = ((UpnEndpointIdentity) identity).GetUpnSid();
                    }
                    else if (ClaimTypes.Spn.Equals(identity.IdentityClaim.ClaimType))
                    {
                        identitySid = ((SpnEndpointIdentity) identity).GetSpnSid();
                    }
                    else if (ClaimTypes.Dns.Equals(identity.IdentityClaim.ClaimType))
                    {
                        identitySid = new SpnEndpointIdentity(expectedSpn).GetSpnSid();
                    }
                    if (identitySid != null)
                    {
                        Claim claim2 = this.CheckSidEquivalence(identitySid, claimSet);
                        if (claim2 != null)
                        {
                            SecurityTraceRecordHelper.TraceIdentityVerificationSuccess(identity, claim2, base.GetType());
                            return true;
                        }
                    }
                }
                SecurityTraceRecordHelper.TraceIdentityVerificationFailure(identity, authContext, base.GetType());
                return false;
            }

            private Claim CheckDnsEquivalence(ClaimSet claimSet, string expectedSpn)
            {
                foreach (Claim claim in claimSet.FindClaims(ClaimTypes.Spn, Rights.PossessProperty))
                {
                    if (expectedSpn.Equals((string) claim.Resource, StringComparison.OrdinalIgnoreCase))
                    {
                        return claim;
                    }
                }
                return null;
            }

            private Claim CheckSidEquivalence(SecurityIdentifier identitySid, ClaimSet claimSet)
            {
                foreach (Claim claim in claimSet)
                {
                    SecurityIdentifier securityIdentifier = this.GetSecurityIdentifier(claim);
                    if ((securityIdentifier != null) && identitySid.Equals(securityIdentifier))
                    {
                        return claim;
                    }
                }
                return null;
            }

            private SecurityIdentifier GetSecurityIdentifier(Claim claim)
            {
                if (claim.Resource is WindowsIdentity)
                {
                    return ((WindowsIdentity) claim.Resource).User;
                }
                if (claim.Resource is WindowsSidIdentity)
                {
                    return ((WindowsSidIdentity) claim.Resource).SecurityIdentifier;
                }
                return (claim.Resource as SecurityIdentifier);
            }

            private EndpointIdentity TryCreateDnsIdentity(EndpointAddress reference)
            {
                Uri uri = reference.Uri;
                if (!uri.IsAbsoluteUri)
                {
                    return null;
                }
                return EndpointIdentity.CreateDnsIdentity(uri.DnsSafeHost);
            }

            public override bool TryGetIdentity(EndpointAddress reference, out EndpointIdentity identity)
            {
                if (reference == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reference");
                }
                identity = reference.Identity;
                if (identity == null)
                {
                    identity = this.TryCreateDnsIdentity(reference);
                }
                if (identity == null)
                {
                    SecurityTraceRecordHelper.TraceIdentityDeterminationFailure(reference, typeof(IdentityVerifier.DefaultIdentityVerifier));
                    return false;
                }
                SecurityTraceRecordHelper.TraceIdentityDeterminationSuccess(reference, identity, typeof(IdentityVerifier.DefaultIdentityVerifier));
                return true;
            }

            public static IdentityVerifier.DefaultIdentityVerifier Instance
            {
                get
                {
                    return instance;
                }
            }
        }
    }
}

