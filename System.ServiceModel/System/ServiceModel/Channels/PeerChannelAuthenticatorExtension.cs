namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class PeerChannelAuthenticatorExtension : IExtension<IPeerNeighbor>
    {
        private IPeerNeighbor host;
        private string meshId;
        private EventHandler onSucceeded;
        private EventArgs originalArgs;
        private PeerSecurityManager securityManager;
        private PeerAuthState state;
        private object thisLock = new object();
        private static TimeSpan Timeout = new TimeSpan(0, 2, 0);
        private IOThreadTimer timer;

        public PeerChannelAuthenticatorExtension(PeerSecurityManager securityManager, EventHandler onSucceeded, EventArgs args, string meshId)
        {
            this.securityManager = securityManager;
            this.state = PeerAuthState.Created;
            this.originalArgs = args;
            this.onSucceeded = onSucceeded;
            this.meshId = meshId;
        }

        public void Attach(IPeerNeighbor host)
        {
            Fx.AssertAndThrow(this.securityManager.AuthenticationMode == PeerAuthenticationMode.Password, "Invalid AuthenticationMode!");
            Fx.AssertAndThrow(host != null, "unrecognized host!");
            this.host = host;
            this.timer = new IOThreadTimer(new Action<object>(this.OnTimeout), null, true);
            this.timer.Set(Timeout);
        }

        public void Detach(IPeerNeighbor host)
        {
            if (host.State < PeerNeighborState.Authenticated)
            {
                this.OnFailed(host);
            }
            this.host = null;
            this.timer.Cancel();
        }

        public void InitiateHandShake()
        {
            IPeerNeighbor host = this.host;
            Message message = null;
            using (new OperationContextScope(new OperationContext(null)))
            {
                PeerHashToken selfToken = this.securityManager.GetSelfToken();
                Message request = Message.CreateMessage(MessageVersion.Soap12WSAddressing10, "RequestSecurityToken", (BodyWriter) new PeerRequestSecurityToken(selfToken));
                bool flag = false;
                try
                {
                    message = host.RequestSecurityToken(request);
                    if (message == null)
                    {
                        throw Fx.AssertAndThrow("SecurityHandshake return empty message!");
                    }
                    this.ProcessRstr(host, message, PeerSecurityManager.FindClaim(ServiceSecurityContext.Current));
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        flag = true;
                        throw;
                    }
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    this.state = PeerAuthState.Failed;
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        ServiceSecurityContext current = ServiceSecurityContext.Current;
                        ClaimSet claimSet = null;
                        if (((current != null) && (current.AuthorizationContext != null)) && ((current.AuthorizationContext.ClaimSets != null) && (current.AuthorizationContext.ClaimSets.Count > 0)))
                        {
                            claimSet = current.AuthorizationContext.ClaimSets[0];
                        }
                        PeerAuthenticationFailureTraceRecord extendedData = new PeerAuthenticationFailureTraceRecord(this.meshId, host.ListenAddress.EndpointAddress.ToString(), claimSet, exception);
                        TraceUtility.TraceEvent(TraceEventType.Error, 0x4004d, System.ServiceModel.SR.GetString("TraceCodePeerNodeAuthenticationFailure"), extendedData, this, null);
                    }
                    host.Abort(PeerCloseReason.AuthenticationFailure, PeerCloseInitiator.LocalNode);
                }
                finally
                {
                    if (!flag)
                    {
                        request.Close();
                    }
                }
            }
        }

        public void OnAuthenticated()
        {
            IPeerNeighbor sender = null;
            lock (this.ThisLock)
            {
                this.timer.Cancel();
                sender = this.host;
                this.state = PeerAuthState.Authenticated;
            }
            if (sender != null)
            {
                sender.TrySetState(PeerNeighborState.Authenticated);
                this.onSucceeded(sender, this.originalArgs);
            }
        }

        private void OnFailed(IPeerNeighbor neighbor)
        {
            lock (this.ThisLock)
            {
                this.state = PeerAuthState.Failed;
                this.timer.Cancel();
                this.host = null;
            }
            if (DiagnosticUtility.ShouldTraceError)
            {
                PeerAuthenticationFailureTraceRecord extendedData = null;
                string remoteAddress = "";
                PeerNodeAddress listenAddress = neighbor.ListenAddress;
                if (listenAddress != null)
                {
                    remoteAddress = listenAddress.EndpointAddress.ToString();
                }
                OperationContext current = OperationContext.Current;
                if (current != null)
                {
                    remoteAddress = current.IncomingMessageProperties.Via.ToString();
                    ServiceSecurityContext serviceSecurityContext = current.ServiceSecurityContext;
                    if (serviceSecurityContext != null)
                    {
                        extendedData = new PeerAuthenticationFailureTraceRecord(this.meshId, remoteAddress, serviceSecurityContext.AuthorizationContext.ClaimSets[0], null);
                        if (DiagnosticUtility.ShouldTraceError)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Error, 0x4004d, System.ServiceModel.SR.GetString("TraceCodePeerNodeAuthenticationFailure"), extendedData, this, null);
                        }
                    }
                }
                else
                {
                    extendedData = new PeerAuthenticationFailureTraceRecord(this.meshId, remoteAddress);
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, 0x4004e, System.ServiceModel.SR.GetString("TraceCodePeerNodeAuthenticationTimeout"), extendedData, this, null);
                    }
                }
            }
            neighbor.Abort(PeerCloseReason.AuthenticationFailure, PeerCloseInitiator.LocalNode);
        }

        public static void OnNeighborClosed(IPeerNeighbor neighbor)
        {
            PeerChannelAuthenticatorExtension item = neighbor.Extensions.Find<PeerChannelAuthenticatorExtension>();
            if (item != null)
            {
                neighbor.Extensions.Remove(item);
            }
        }

        private void OnTimeout(object state)
        {
            IPeerNeighbor host = this.host;
            if ((host != null) && (host.State < PeerNeighborState.Authenticated))
            {
                this.OnFailed(host);
            }
        }

        public Message ProcessRst(Message message, Claim claim)
        {
            IPeerNeighbor host = this.host;
            PeerRequestSecurityTokenResponse response = null;
            Message message2 = null;
            lock (this.ThisLock)
            {
                if (((this.state != PeerAuthState.Created) || (host == null)) || (host.IsInitiator || (host.State != PeerNeighborState.Opened)))
                {
                    this.OnFailed(host);
                    return null;
                }
            }
            try
            {
                PeerHashToken token = PeerRequestSecurityToken.CreateHashTokenFrom(message);
                if (!this.securityManager.GetExpectedTokenForClaim(claim).Equals(token))
                {
                    this.OnFailed(host);
                    return message2;
                }
                this.state = PeerAuthState.Authenticated;
                response = new PeerRequestSecurityTokenResponse(this.securityManager.GetSelfToken());
                message2 = Message.CreateMessage(MessageVersion.Soap12WSAddressing10, "RequestSecurityTokenResponse", (BodyWriter) response);
                this.OnAuthenticated();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                this.OnFailed(host);
            }
            return message2;
        }

        public void ProcessRstr(IPeerNeighbor neighbor, Message message, Claim claim)
        {
            PeerHashToken token = PeerRequestSecurityTokenResponse.CreateHashTokenFrom(message);
            if (!token.IsValid)
            {
                this.OnFailed(neighbor);
            }
            else if (!this.securityManager.GetExpectedTokenForClaim(claim).Equals(token))
            {
                this.OnFailed(neighbor);
            }
            else
            {
                this.OnAuthenticated();
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        private enum PeerAuthState
        {
            Created,
            Authenticated,
            Failed
        }
    }
}

