namespace System.Net
{
    using System;
    using System.Net.Security;

    internal class AuthenticationState
    {
        private System.Net.TransportContext _TransportContext;
        internal System.Net.Authorization Authorization;
        private string ChallengedSpn;
        internal Uri ChallengedUri;
        private bool IsProxyAuth;
        internal IAuthenticationModule Module;
        private NTAuthentication SecurityContext;
        private bool TriedPreAuth;
        internal string UniqueGroupId;

        internal AuthenticationState(bool isProxyAuth)
        {
            this.IsProxyAuth = isProxyAuth;
        }

        internal bool AttemptAuthenticate(HttpWebRequest httpWebRequest, ICredentials authInfo)
        {
            if ((this.Authorization != null) && this.Authorization.Complete)
            {
                if (this.IsProxyAuth)
                {
                    this.ClearAuthReq(httpWebRequest);
                }
                return false;
            }
            if (authInfo == null)
            {
                return false;
            }
            string challenge = httpWebRequest.AuthHeader(this.AuthenticateHeader);
            if (challenge == null)
            {
                if ((!this.IsProxyAuth && (this.Authorization != null)) && (httpWebRequest.ProxyAuthenticationState.Authorization != null))
                {
                    httpWebRequest.Headers.Set(this.AuthorizationHeader, this.Authorization.Message);
                }
                return false;
            }
            this.PrepareState(httpWebRequest);
            try
            {
                this.Authorization = AuthenticationManager.Authenticate(challenge, httpWebRequest, authInfo);
            }
            catch (Exception)
            {
                this.Authorization = null;
                this.ClearSession(httpWebRequest);
                throw;
            }
            if (this.Authorization == null)
            {
                return false;
            }
            if (this.Authorization.Message == null)
            {
                this.Authorization = null;
                return false;
            }
            this.UniqueGroupId = this.Authorization.ConnectionGroupId;
            try
            {
                httpWebRequest.Headers.Set(this.AuthorizationHeader, this.Authorization.Message);
            }
            catch
            {
                this.Authorization = null;
                this.ClearSession(httpWebRequest);
                throw;
            }
            return true;
        }

        internal void ClearAuthReq(HttpWebRequest httpWebRequest)
        {
            this.TriedPreAuth = false;
            this.Authorization = null;
            this.UniqueGroupId = null;
            httpWebRequest.Headers.Remove(this.AuthorizationHeader);
        }

        internal void ClearSession()
        {
            if (this.SecurityContext != null)
            {
                this.SecurityContext.CloseContext();
                this.SecurityContext = null;
            }
        }

        internal void ClearSession(HttpWebRequest httpWebRequest)
        {
            this.PrepareState(httpWebRequest);
            ISessionAuthenticationModule module = this.Module as ISessionAuthenticationModule;
            this.Module = null;
            if (module != null)
            {
                try
                {
                    module.ClearSession(httpWebRequest);
                }
                catch (Exception exception)
                {
                    if (NclUtilities.IsFatal(exception))
                    {
                        throw;
                    }
                }
            }
        }

        internal string GetComputeSpn(HttpWebRequest httpWebRequest)
        {
            if (this.ChallengedSpn != null)
            {
                return this.ChallengedSpn;
            }
            string parts = httpWebRequest.ChallengedUri.GetParts(UriComponents.Path | UriComponents.SchemeAndServer, UriFormat.SafeUnescaped);
            string hostName = AuthenticationManager.SpnDictionary.InternalGet(parts);
            if (hostName == null)
            {
                if (!this.IsProxyAuth && (httpWebRequest.ServicePoint.InternalProxyServicePoint || httpWebRequest.UseCustomHost))
                {
                    hostName = httpWebRequest.ChallengedUri.Host;
                    if (((httpWebRequest.ChallengedUri.HostNameType != UriHostNameType.IPv6) && (httpWebRequest.ChallengedUri.HostNameType != UriHostNameType.IPv4)) && (hostName.IndexOf('.') == -1))
                    {
                        try
                        {
                            hostName = Dns.InternalGetHostByName(hostName).HostName;
                        }
                        catch (Exception exception)
                        {
                            if (NclUtilities.IsFatal(exception))
                            {
                                throw;
                            }
                        }
                    }
                }
                else
                {
                    hostName = httpWebRequest.ServicePoint.Hostname;
                }
                hostName = "HTTP/" + hostName;
                parts = httpWebRequest.ChallengedUri.GetParts(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped) + "/";
                AuthenticationManager.SpnDictionary.InternalSet(parts, hostName);
            }
            return (this.ChallengedSpn = hostName);
        }

        internal NTAuthentication GetSecurityContext(IAuthenticationModule module)
        {
            if (module != this.Module)
            {
                return null;
            }
            return this.SecurityContext;
        }

        internal void PreAuthIfNeeded(HttpWebRequest httpWebRequest, ICredentials authInfo)
        {
            if (!this.TriedPreAuth)
            {
                this.TriedPreAuth = true;
                if (authInfo != null)
                {
                    this.PrepareState(httpWebRequest);
                    System.Net.Authorization authorization = null;
                    try
                    {
                        authorization = AuthenticationManager.PreAuthenticate(httpWebRequest, authInfo);
                        if ((authorization != null) && (authorization.Message != null))
                        {
                            this.UniqueGroupId = authorization.ConnectionGroupId;
                            httpWebRequest.Headers.Set(this.AuthorizationHeader, authorization.Message);
                        }
                    }
                    catch (Exception)
                    {
                        this.ClearSession(httpWebRequest);
                    }
                }
            }
        }

        private void PrepareState(HttpWebRequest httpWebRequest)
        {
            Uri uri = this.IsProxyAuth ? httpWebRequest.ServicePoint.InternalAddress : httpWebRequest.GetRemoteResourceUri();
            if (this.ChallengedUri != uri)
            {
                if (((this.ChallengedUri == null) || (this.ChallengedUri.Scheme != uri.Scheme)) || ((this.ChallengedUri.Host != uri.Host) || (this.ChallengedUri.Port != uri.Port)))
                {
                    this.ChallengedSpn = null;
                }
                this.ChallengedUri = uri;
            }
            httpWebRequest.CurrentAuthenticationState = this;
        }

        internal void SetSecurityContext(NTAuthentication securityContext, IAuthenticationModule module)
        {
            this.SecurityContext = securityContext;
        }

        internal void Update(HttpWebRequest httpWebRequest)
        {
            if (this.Authorization != null)
            {
                this.PrepareState(httpWebRequest);
                ISessionAuthenticationModule module = this.Module as ISessionAuthenticationModule;
                if (module != null)
                {
                    string challenge = httpWebRequest.AuthHeader(this.AuthenticateHeader);
                    if (this.IsProxyAuth || (httpWebRequest.ResponseStatusCode != HttpStatusCode.ProxyAuthenticationRequired))
                    {
                        bool complete = true;
                        try
                        {
                            complete = module.Update(challenge, httpWebRequest);
                        }
                        catch (Exception)
                        {
                            this.ClearSession(httpWebRequest);
                            if ((httpWebRequest.AuthenticationLevel == AuthenticationLevel.MutualAuthRequired) && (((httpWebRequest.CurrentAuthenticationState == null) || (httpWebRequest.CurrentAuthenticationState.Authorization == null)) || !httpWebRequest.CurrentAuthenticationState.Authorization.MutuallyAuthenticated))
                            {
                                throw;
                            }
                        }
                        this.Authorization.SetComplete(complete);
                    }
                }
                if (((httpWebRequest.PreAuthenticate && (this.Module != null)) && (this.Authorization.Complete && this.Module.CanPreAuthenticate)) && (httpWebRequest.ResponseStatusCode != this.StatusCodeMatch))
                {
                    AuthenticationManager.BindModule(this.ChallengedUri, this.Authorization, this.Module);
                }
            }
        }

        internal HttpResponseHeader AuthenticateHeader
        {
            get
            {
                if (!this.IsProxyAuth)
                {
                    return HttpResponseHeader.WwwAuthenticate;
                }
                return HttpResponseHeader.ProxyAuthenticate;
            }
        }

        internal string AuthorizationHeader
        {
            get
            {
                if (!this.IsProxyAuth)
                {
                    return "Authorization";
                }
                return "Proxy-Authorization";
            }
        }

        internal HttpStatusCode StatusCodeMatch
        {
            get
            {
                if (!this.IsProxyAuth)
                {
                    return HttpStatusCode.Unauthorized;
                }
                return HttpStatusCode.ProxyAuthenticationRequired;
            }
        }

        internal System.Net.TransportContext TransportContext
        {
            get
            {
                return this._TransportContext;
            }
            set
            {
                this._TransportContext = value;
            }
        }
    }
}

