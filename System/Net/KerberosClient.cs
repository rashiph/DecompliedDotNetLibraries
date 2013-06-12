namespace System.Net
{
    using System;
    using System.Security.Authentication.ExtendedProtection;

    internal class KerberosClient : ISessionAuthenticationModule, IAuthenticationModule
    {
        internal const string AuthType = "Kerberos";
        internal static string Signature = "Kerberos".ToLower(CultureInfo.InvariantCulture);
        internal static int SignatureSize = Signature.Length;

        internal KerberosClient()
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
                securityContext = new NTAuthentication("Kerberos", credential, computeSpn, request, channelBinding);
                request.CurrentAuthenticationState.SetSecurityContext(securityContext, this);
            }
            string outgoingBlob = securityContext.GetOutgoingBlob(incomingBlob);
            if (outgoingBlob == null)
            {
                return null;
            }
            return new Authorization("Kerberos " + outgoingBlob, securityContext.IsCompleted, string.Empty, securityContext.IsMutualAuthFlag);
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
                if (request.CurrentAuthenticationState.StatusCodeMatch == request.ResponseStatusCode)
                {
                    return false;
                }
                int num = (challenge == null) ? -1 : AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature);
                if (num >= 0)
                {
                    int startIndex = num + SignatureSize;
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
                return "Kerberos";
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

