namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;

    internal class NegotiateClient : ISessionAuthenticationModule, IAuthenticationModule
    {
        internal const string AuthType = "Negotiate";
        private const string nego2Header = "Nego2";
        private const string nego2Signature = "nego2";
        private const string negotiateHeader = "Negotiate";
        private const string negotiateSignature = "negotiate";

        public NegotiateClient()
        {
            if (!ComNetOS.IsWin2K)
            {
                throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
            }
        }

        public Authorization Authenticate(string challenge, WebRequest webRequest, ICredentials credentials)
        {
            return this.DoAuthenticate(challenge, webRequest, credentials, false);
        }

        public void ClearSession(WebRequest webRequest)
        {
            HttpWebRequest request = webRequest as HttpWebRequest;
            request.CurrentAuthenticationState.ClearSession();
        }

        private Authorization DoAuthenticate(string challenge, WebRequest webRequest, ICredentials credentials, bool preAuthenticate)
        {
            if (credentials == null)
            {
                return null;
            }
            HttpWebRequest request = webRequest as HttpWebRequest;
            NTAuthentication securityContext = null;
            string incomingBlob = null;
            bool flag = false;
            if (!preAuthenticate)
            {
                int signatureIndex = GetSignatureIndex(challenge, out flag);
                if (signatureIndex < 0)
                {
                    return null;
                }
                int startIndex = signatureIndex + (flag ? "nego2".Length : "negotiate".Length);
                if ((challenge.Length > startIndex) && (challenge[startIndex] != ','))
                {
                    startIndex++;
                }
                else
                {
                    signatureIndex = -1;
                }
                if ((signatureIndex >= 0) && (challenge.Length > startIndex))
                {
                    signatureIndex = challenge.IndexOf(',', startIndex);
                    if (signatureIndex != -1)
                    {
                        incomingBlob = challenge.Substring(startIndex, signatureIndex - startIndex);
                    }
                    else
                    {
                        incomingBlob = challenge.Substring(startIndex);
                    }
                }
                securityContext = request.CurrentAuthenticationState.GetSecurityContext(this);
            }
            if (securityContext == null)
            {
                NetworkCredential credential = credentials.GetCredential(request.ChallengedUri, "negotiate");
                if ((credential == null) || (!(credential is SystemNetworkCredential) && (credential.InternalGetUserName().Length == 0)))
                {
                    return null;
                }
                ICredentialPolicy credentialPolicy = AuthenticationManager.CredentialPolicy;
                if ((credentialPolicy != null) && !credentialPolicy.ShouldSendCredential(request.ChallengedUri, request, credential, this))
                {
                    return null;
                }
                string computeSpn = request.CurrentAuthenticationState.GetComputeSpn(request);
                ChannelBinding channelBinding = null;
                if (request.CurrentAuthenticationState.TransportContext != null)
                {
                    channelBinding = request.CurrentAuthenticationState.TransportContext.GetChannelBinding(ChannelBindingKind.Endpoint);
                }
                securityContext = new NTAuthentication("Negotiate", credential, computeSpn, request, channelBinding);
                request.CurrentAuthenticationState.SetSecurityContext(securityContext, this);
            }
            string outgoingBlob = securityContext.GetOutgoingBlob(incomingBlob);
            if (outgoingBlob == null)
            {
                return null;
            }
            bool unsafeOrProxyAuthenticatedConnectionSharing = request.UnsafeOrProxyAuthenticatedConnectionSharing;
            if (unsafeOrProxyAuthenticatedConnectionSharing)
            {
                request.LockConnection = true;
            }
            request.NtlmKeepAlive = ((incomingBlob == null) && securityContext.IsValidContext) && !securityContext.IsKerberos;
            return AuthenticationManager.GetGroupAuthorization(this, (flag ? "Nego2" : "Negotiate") + " " + outgoingBlob, securityContext.IsCompleted, securityContext, unsafeOrProxyAuthenticatedConnectionSharing, securityContext.IsKerberos);
        }

        private static int GetSignatureIndex(string challenge, out bool useNego2)
        {
            useNego2 = true;
            int num = -1;
            if (ComNetOS.IsWin7)
            {
                num = AuthenticationManager.FindSubstringNotInQuotes(challenge, "nego2");
            }
            if (num < 0)
            {
                useNego2 = false;
                num = AuthenticationManager.FindSubstringNotInQuotes(challenge, "negotiate");
            }
            return num;
        }

        public Authorization PreAuthenticate(WebRequest webRequest, ICredentials credentials)
        {
            return this.DoAuthenticate(null, webRequest, credentials, true);
        }

        public bool Update(string challenge, WebRequest webRequest)
        {
            HttpWebRequest request = webRequest as HttpWebRequest;
            NTAuthentication securityContext = request.CurrentAuthenticationState.GetSecurityContext(this);
            if (securityContext != null)
            {
                if (!securityContext.IsCompleted && (request.CurrentAuthenticationState.StatusCodeMatch == request.ResponseStatusCode))
                {
                    return false;
                }
                if (!request.UnsafeOrProxyAuthenticatedConnectionSharing)
                {
                    request.ServicePoint.ReleaseConnectionGroup(request.GetConnectionGroupLine());
                }
                bool flag = true;
                int num = (challenge == null) ? -1 : GetSignatureIndex(challenge, out flag);
                if (num >= 0)
                {
                    int startIndex = num + (flag ? "nego2".Length : "negotiate".Length);
                    string incomingBlob = null;
                    if ((challenge.Length > startIndex) && (challenge[startIndex] != ','))
                    {
                        startIndex++;
                    }
                    else
                    {
                        num = -1;
                    }
                    if ((num >= 0) && (challenge.Length > startIndex))
                    {
                        incomingBlob = challenge.Substring(startIndex);
                    }
                    securityContext.GetOutgoingBlob(incomingBlob);
                    request.CurrentAuthenticationState.Authorization.MutuallyAuthenticated = securityContext.IsMutualAuthFlag;
                }
                request.ServicePoint.SetCachedChannelBinding(request.ChallengedUri, securityContext.ChannelBinding);
                this.ClearSession(request);
            }
            return true;
        }

        public string AuthenticationType
        {
            get
            {
                return "Negotiate";
            }
        }

        public bool CanPreAuthenticate
        {
            get
            {
                return true;
            }
        }

        public bool CanUseDefaultCredentials
        {
            get
            {
                return true;
            }
        }
    }
}

