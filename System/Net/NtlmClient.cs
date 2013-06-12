namespace System.Net
{
    using System;
    using System.Security.Authentication.ExtendedProtection;

    internal class NtlmClient : ISessionAuthenticationModule, IAuthenticationModule
    {
        internal const string AuthType = "NTLM";
        internal const int MaxNtlmCredentialSize = 0x20f;
        internal static string Signature = "NTLM".ToLower(CultureInfo.InvariantCulture);
        internal static int SignatureSize = Signature.Length;

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
            if (!preAuthenticate)
            {
                int index = AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature);
                if (index < 0)
                {
                    return null;
                }
                int startIndex = index + SignatureSize;
                if ((challenge.Length > startIndex) && (challenge[startIndex] != ','))
                {
                    startIndex++;
                }
                else
                {
                    index = -1;
                }
                if ((index >= 0) && (challenge.Length > startIndex))
                {
                    index = challenge.IndexOf(',', startIndex);
                    if (index != -1)
                    {
                        incomingBlob = challenge.Substring(startIndex, index - startIndex);
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
                NetworkCredential credential = credentials.GetCredential(request.ChallengedUri, Signature);
                string str2 = string.Empty;
                if ((credential == null) || (!(credential is SystemNetworkCredential) && ((str2 = credential.InternalGetUserName()).Length == 0)))
                {
                    return null;
                }
                if (((str2.Length + credential.InternalGetPassword().Length) + credential.InternalGetDomain().Length) > 0x20f)
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
                securityContext = new NTAuthentication("NTLM", credential, computeSpn, request, channelBinding);
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
            request.NtlmKeepAlive = incomingBlob == null;
            return AuthenticationManager.GetGroupAuthorization(this, "NTLM " + outgoingBlob, securityContext.IsCompleted, securityContext, unsafeOrProxyAuthenticatedConnectionSharing, false);
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
                this.ClearSession(request);
                if (!request.UnsafeOrProxyAuthenticatedConnectionSharing)
                {
                    request.ServicePoint.ReleaseConnectionGroup(request.GetConnectionGroupLine());
                }
                request.ServicePoint.SetCachedChannelBinding(request.ChallengedUri, securityContext.ChannelBinding);
            }
            return true;
        }

        public string AuthenticationType
        {
            get
            {
                return "NTLM";
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

