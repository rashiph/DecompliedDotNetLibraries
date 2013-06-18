namespace System.IdentityModel.Selectors
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Threading;

    public abstract class SecurityTokenProvider
    {
        protected SecurityTokenProvider()
        {
        }

        public IAsyncResult BeginCancelToken(TimeSpan timeout, SecurityToken token, AsyncCallback callback, object state)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            return this.BeginCancelTokenCore(timeout, token, callback, state);
        }

        protected virtual IAsyncResult BeginCancelTokenCore(TimeSpan timeout, SecurityToken token, AsyncCallback callback, object state)
        {
            this.CancelToken(timeout, token);
            return new SecurityTokenAsyncResult(null, callback, state);
        }

        public IAsyncResult BeginGetToken(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginGetTokenCore(timeout, callback, state);
        }

        protected virtual IAsyncResult BeginGetTokenCore(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new SecurityTokenAsyncResult(this.GetToken(timeout), callback, state);
        }

        public IAsyncResult BeginRenewToken(TimeSpan timeout, SecurityToken tokenToBeRenewed, AsyncCallback callback, object state)
        {
            if (tokenToBeRenewed == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenToBeRenewed");
            }
            return this.BeginRenewTokenCore(timeout, tokenToBeRenewed, callback, state);
        }

        protected virtual IAsyncResult BeginRenewTokenCore(TimeSpan timeout, SecurityToken tokenToBeRenewed, AsyncCallback callback, object state)
        {
            return new SecurityTokenAsyncResult(this.RenewTokenCore(timeout, tokenToBeRenewed), callback, state);
        }

        public void CancelToken(TimeSpan timeout, SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            this.CancelTokenCore(timeout, token);
        }

        protected virtual void CancelTokenCore(TimeSpan timeout, SecurityToken token)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("TokenCancellationNotSupported", new object[] { this })));
        }

        public void EndCancelToken(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            this.EndCancelTokenCore(result);
        }

        protected virtual void EndCancelTokenCore(IAsyncResult result)
        {
            SecurityTokenAsyncResult.End(result);
        }

        public SecurityToken EndGetToken(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            SecurityToken token = this.EndGetTokenCore(result);
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("TokenProviderUnableToGetToken", new object[] { this })));
            }
            return token;
        }

        protected virtual SecurityToken EndGetTokenCore(IAsyncResult result)
        {
            return SecurityTokenAsyncResult.End(result);
        }

        public SecurityToken EndRenewToken(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            SecurityToken token = this.EndRenewTokenCore(result);
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("TokenProviderUnableToRenewToken", new object[] { this })));
            }
            return token;
        }

        protected virtual SecurityToken EndRenewTokenCore(IAsyncResult result)
        {
            return SecurityTokenAsyncResult.End(result);
        }

        public SecurityToken GetToken(TimeSpan timeout)
        {
            SecurityToken tokenCore = this.GetTokenCore(timeout);
            if (tokenCore == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("TokenProviderUnableToGetToken", new object[] { this })));
            }
            return tokenCore;
        }

        protected abstract SecurityToken GetTokenCore(TimeSpan timeout);
        public SecurityToken RenewToken(TimeSpan timeout, SecurityToken tokenToBeRenewed)
        {
            if (tokenToBeRenewed == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenToBeRenewed");
            }
            SecurityToken token = this.RenewTokenCore(timeout, tokenToBeRenewed);
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("TokenProviderUnableToRenewToken", new object[] { this })));
            }
            return token;
        }

        protected virtual SecurityToken RenewTokenCore(TimeSpan timeout, SecurityToken tokenToBeRenewed)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("TokenRenewalNotSupported", new object[] { this })));
        }

        public virtual bool SupportsTokenCancellation
        {
            get
            {
                return false;
            }
        }

        public virtual bool SupportsTokenRenewal
        {
            get
            {
                return false;
            }
        }

        internal protected class SecurityTokenAsyncResult : IAsyncResult
        {
            private ManualResetEvent manualResetEvent;
            private object state;
            private object thisLock = new object();
            private SecurityToken token;

            public SecurityTokenAsyncResult(SecurityToken token, AsyncCallback callback, object state)
            {
                this.token = token;
                this.state = state;
                if (callback != null)
                {
                    try
                    {
                        callback(this);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(System.IdentityModel.SR.GetString("AsyncCallbackException"), exception);
                    }
                }
            }

            public static SecurityToken End(IAsyncResult result)
            {
                if (result == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
                }
                SecurityTokenProvider.SecurityTokenAsyncResult result2 = result as SecurityTokenProvider.SecurityTokenAsyncResult;
                if (result2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.IdentityModel.SR.GetString("InvalidAsyncResult"), "result"));
                }
                return result2.token;
            }

            public object AsyncState
            {
                get
                {
                    return this.state;
                }
            }

            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    if (this.manualResetEvent == null)
                    {
                        lock (this.thisLock)
                        {
                            if (this.manualResetEvent == null)
                            {
                                this.manualResetEvent = new ManualResetEvent(true);
                            }
                        }
                    }
                    return this.manualResetEvent;
                }
            }

            public bool CompletedSynchronously
            {
                get
                {
                    return true;
                }
            }

            public bool IsCompleted
            {
                get
                {
                    return true;
                }
            }
        }
    }
}

