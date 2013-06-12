namespace System.Net
{
    using System;
    using System.Security.Authentication.ExtendedProtection;

    internal class DigestClient : ISessionAuthenticationModule, IAuthenticationModule
    {
        private static bool _WDigestAvailable = (SSPIWrapper.GetVerifyPackageInfo(GlobalSSPI.SSPIAuth, "WDigest") != null);
        internal const string AuthType = "Digest";
        private static PrefixLookup challengeCache = new PrefixLookup();
        internal static string Signature = "Digest".ToLower(CultureInfo.InvariantCulture);
        internal static int SignatureSize = Signature.Length;
        private static readonly char[] singleSpaceArray = new char[] { ' ' };

        public Authorization Authenticate(string challenge, WebRequest webRequest, ICredentials credentials)
        {
            return this.DoAuthenticate(challenge, webRequest, credentials, false);
        }

        internal static bool CheckQOP(HttpDigestChallenge challenge)
        {
            if (challenge.QopPresent)
            {
                for (int i = 0; i >= 0; i += "auth".Length)
                {
                    i = challenge.QualityOfProtection.IndexOf("auth", i);
                    if (i < 0)
                    {
                        return false;
                    }
                    if (((i == 0) || (", \"'\t\r\n".IndexOf(challenge.QualityOfProtection[i - 1]) >= 0)) && (((i + "auth".Length) == challenge.QualityOfProtection.Length) || (", \"'\t\r\n".IndexOf(challenge.QualityOfProtection[i + "auth".Length]) >= 0)))
                    {
                        break;
                    }
                }
            }
            return true;
        }

        public void ClearSession(WebRequest webRequest)
        {
            HttpWebRequest request = webRequest as HttpWebRequest;
            request.CurrentAuthenticationState.ClearSession();
        }

        private Authorization DoAuthenticate(string challenge, WebRequest webRequest, ICredentials credentials, bool preAuthenticate)
        {
            HttpDigestChallenge challenge2;
            if (credentials == null)
            {
                return null;
            }
            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;
            NetworkCredential credential = credentials.GetCredential(httpWebRequest.ChallengedUri, Signature);
            if (credential is SystemNetworkCredential)
            {
                if (WDigestAvailable)
                {
                    return this.XPDoAuthenticate(challenge, httpWebRequest, credentials, preAuthenticate);
                }
                return null;
            }
            if (!preAuthenticate)
            {
                int startingPoint = AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature);
                if (startingPoint < 0)
                {
                    return null;
                }
                challenge2 = HttpDigest.Interpret(challenge, startingPoint, httpWebRequest);
            }
            else
            {
                challenge2 = challengeCache.Lookup(httpWebRequest.ChallengedUri.AbsoluteUri) as HttpDigestChallenge;
            }
            if (challenge2 == null)
            {
                return null;
            }
            if (!CheckQOP(challenge2))
            {
                if (Logging.On)
                {
                    Logging.PrintError(Logging.Web, SR.GetString("net_log_digest_qop_not_supported", new object[] { challenge2.QualityOfProtection }));
                }
                return null;
            }
            if (preAuthenticate)
            {
                challenge2 = challenge2.CopyAndIncrementNonce();
                challenge2.SetFromRequest(httpWebRequest);
            }
            if (credential == null)
            {
                return null;
            }
            ICredentialPolicy credentialPolicy = AuthenticationManager.CredentialPolicy;
            if ((credentialPolicy != null) && !credentialPolicy.ShouldSendCredential(httpWebRequest.ChallengedUri, httpWebRequest, credential, this))
            {
                return null;
            }
            string computeSpn = httpWebRequest.CurrentAuthenticationState.GetComputeSpn(httpWebRequest);
            ChannelBinding channelBinding = null;
            if (httpWebRequest.CurrentAuthenticationState.TransportContext != null)
            {
                channelBinding = httpWebRequest.CurrentAuthenticationState.TransportContext.GetChannelBinding(ChannelBindingKind.Endpoint);
            }
            Authorization authorization = HttpDigest.Authenticate(challenge2, credential, computeSpn, channelBinding);
            if ((!preAuthenticate && webRequest.PreAuthenticate) && (authorization != null))
            {
                string[] strArray = (challenge2.Domain == null) ? new string[] { httpWebRequest.ChallengedUri.GetParts(UriComponents.SchemeAndServer, UriFormat.UriEscaped) } : challenge2.Domain.Split(singleSpaceArray);
                authorization.ProtectionRealm = (challenge2.Domain == null) ? null : strArray;
                for (int i = 0; i < strArray.Length; i++)
                {
                    challengeCache.Add(strArray[i], challenge2);
                }
            }
            return authorization;
        }

        public Authorization PreAuthenticate(WebRequest webRequest, ICredentials credentials)
        {
            return this.DoAuthenticate(null, webRequest, credentials, true);
        }

        private static string RefineDigestChallenge(string challenge, int index)
        {
            string str = null;
            int num4;
            if ((challenge == null) || (index >= challenge.Length))
            {
                throw new ArgumentOutOfRangeException("challenge", challenge);
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
            if ((index < 0) || (challenge.Length <= startIndex))
            {
                throw new ArgumentOutOfRangeException("challenge", challenge);
            }
            str = challenge.Substring(startIndex);
            int num3 = 0;
            bool flag = true;
            HttpDigestChallenge challenge2 = new HttpDigestChallenge();
        Label_0070:
            num4 = num3;
            index = AuthenticationManager.SplitNoQuotes(str, ref num4);
            if (num4 >= 0)
            {
                string str3;
                string name = str.Substring(num3, num4 - num3);
                if (index < 0)
                {
                    str3 = HttpDigest.unquote(str.Substring(num4 + 1));
                }
                else
                {
                    str3 = HttpDigest.unquote(str.Substring(num4 + 1, (index - num4) - 1));
                }
                flag = challenge2.defineAttribute(name, str3);
                if ((index >= 0) && flag)
                {
                    num3 = ++index;
                    goto Label_0070;
                }
            }
            if ((!flag || (num4 < 0)) && (num3 < str.Length))
            {
                str = (num3 > 0) ? str.Substring(0, num3 - 1) : "";
            }
            return str;
        }

        public bool Update(string challenge, WebRequest webRequest)
        {
            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;
            if (httpWebRequest.CurrentAuthenticationState.GetSecurityContext(this) != null)
            {
                return this.XPUpdate(challenge, httpWebRequest);
            }
            if (httpWebRequest.ResponseStatusCode != httpWebRequest.CurrentAuthenticationState.StatusCodeMatch)
            {
                ChannelBinding channelBinding = null;
                if (httpWebRequest.CurrentAuthenticationState.TransportContext != null)
                {
                    channelBinding = httpWebRequest.CurrentAuthenticationState.TransportContext.GetChannelBinding(ChannelBindingKind.Endpoint);
                }
                httpWebRequest.ServicePoint.SetCachedChannelBinding(httpWebRequest.ChallengedUri, channelBinding);
                return true;
            }
            int startingPoint = (challenge == null) ? -1 : AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature);
            if (startingPoint < 0)
            {
                return true;
            }
            int startIndex = startingPoint + SignatureSize;
            if ((challenge.Length > startIndex) && (challenge[startIndex] != ','))
            {
                startIndex++;
            }
            else
            {
                startingPoint = -1;
            }
            if ((startingPoint >= 0) && (challenge.Length > startIndex))
            {
                challenge.Substring(startIndex);
            }
            HttpDigestChallenge challenge2 = HttpDigest.Interpret(challenge, startingPoint, httpWebRequest);
            return ((challenge2 == null) || !challenge2.Stale);
        }

        private Authorization XPDoAuthenticate(string challenge, HttpWebRequest httpWebRequest, ICredentials credentials, bool preAuthenticate)
        {
            NTAuthentication securityContext = null;
            string incomingBlob = null;
            SecurityStatus status;
            if (!preAuthenticate)
            {
                int index = AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature);
                if (index < 0)
                {
                    return null;
                }
                securityContext = httpWebRequest.CurrentAuthenticationState.GetSecurityContext(this);
                incomingBlob = RefineDigestChallenge(challenge, index);
            }
            else
            {
                HttpDigestChallenge challenge2 = challengeCache.Lookup(httpWebRequest.ChallengedUri.AbsoluteUri) as HttpDigestChallenge;
                if (challenge2 == null)
                {
                    return null;
                }
                challenge2 = challenge2.CopyAndIncrementNonce();
                challenge2.SetFromRequest(httpWebRequest);
                incomingBlob = challenge2.ToBlob();
            }
            UriComponents uriParts = 0;
            if (httpWebRequest.CurrentMethod.ConnectRequest)
            {
                uriParts = UriComponents.HostAndPort;
            }
            else if (httpWebRequest.UsesProxySemantics)
            {
                uriParts = UriComponents.HttpRequestUrl;
            }
            else
            {
                uriParts = UriComponents.PathAndQuery;
            }
            string parts = httpWebRequest.GetRemoteResourceUri().GetParts(uriParts, UriFormat.UriEscaped);
            if (securityContext == null)
            {
                NetworkCredential credential = credentials.GetCredential(httpWebRequest.ChallengedUri, Signature);
                if ((credential == null) || (!(credential is SystemNetworkCredential) && (credential.InternalGetUserName().Length == 0)))
                {
                    return null;
                }
                ICredentialPolicy credentialPolicy = AuthenticationManager.CredentialPolicy;
                if ((credentialPolicy != null) && !credentialPolicy.ShouldSendCredential(httpWebRequest.ChallengedUri, httpWebRequest, credential, this))
                {
                    return null;
                }
                string computeSpn = httpWebRequest.CurrentAuthenticationState.GetComputeSpn(httpWebRequest);
                ChannelBinding channelBinding = null;
                if (httpWebRequest.CurrentAuthenticationState.TransportContext != null)
                {
                    channelBinding = httpWebRequest.CurrentAuthenticationState.TransportContext.GetChannelBinding(ChannelBindingKind.Endpoint);
                }
                securityContext = new NTAuthentication("WDigest", credential, computeSpn, httpWebRequest, channelBinding);
                httpWebRequest.CurrentAuthenticationState.SetSecurityContext(securityContext, this);
            }
            string str4 = securityContext.GetOutgoingDigestBlob(incomingBlob, httpWebRequest.CurrentMethod.Name, parts, null, false, false, out status);
            if (str4 == null)
            {
                return null;
            }
            Authorization authorization = new Authorization("Digest " + str4, securityContext.IsCompleted, string.Empty, securityContext.IsMutualAuthFlag);
            if (!preAuthenticate && httpWebRequest.PreAuthenticate)
            {
                HttpDigestChallenge challenge3 = HttpDigest.Interpret(incomingBlob, -1, httpWebRequest);
                string[] strArray = (challenge3.Domain == null) ? new string[] { httpWebRequest.ChallengedUri.GetParts(UriComponents.SchemeAndServer, UriFormat.UriEscaped) } : challenge3.Domain.Split(singleSpaceArray);
                authorization.ProtectionRealm = (challenge3.Domain == null) ? null : strArray;
                for (int i = 0; i < strArray.Length; i++)
                {
                    challengeCache.Add(strArray[i], challenge3);
                }
            }
            return authorization;
        }

        private bool XPUpdate(string challenge, HttpWebRequest httpWebRequest)
        {
            SecurityStatus status;
            NTAuthentication securityContext = httpWebRequest.CurrentAuthenticationState.GetSecurityContext(this);
            if (securityContext == null)
            {
                return false;
            }
            int index = (challenge == null) ? -1 : AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature);
            if (index < 0)
            {
                httpWebRequest.ServicePoint.SetCachedChannelBinding(httpWebRequest.ChallengedUri, securityContext.ChannelBinding);
                this.ClearSession(httpWebRequest);
                return true;
            }
            if (httpWebRequest.ResponseStatusCode != httpWebRequest.CurrentAuthenticationState.StatusCodeMatch)
            {
                httpWebRequest.ServicePoint.SetCachedChannelBinding(httpWebRequest.ChallengedUri, securityContext.ChannelBinding);
                this.ClearSession(httpWebRequest);
                return true;
            }
            string incomingBlob = RefineDigestChallenge(challenge, index);
            securityContext.GetOutgoingDigestBlob(incomingBlob, httpWebRequest.CurrentMethod.Name, null, null, false, true, out status);
            httpWebRequest.CurrentAuthenticationState.Authorization.MutuallyAuthenticated = securityContext.IsMutualAuthFlag;
            return securityContext.IsCompleted;
        }

        public string AuthenticationType
        {
            get
            {
                return "Digest";
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
                return WDigestAvailable;
            }
        }

        internal static bool WDigestAvailable
        {
            get
            {
                return _WDigestAvailable;
            }
        }
    }
}

