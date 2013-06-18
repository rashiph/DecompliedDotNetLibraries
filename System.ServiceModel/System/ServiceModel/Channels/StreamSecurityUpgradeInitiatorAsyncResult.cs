namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Authentication;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal abstract class StreamSecurityUpgradeInitiatorAsyncResult : AsyncResult
    {
        private static AsyncCallback onAuthenticateAsClient = Fx.ThunkCallback(new AsyncCallback(StreamSecurityUpgradeInitiatorAsyncResult.OnAuthenticateAsClient));
        private Stream originalStream;
        private SecurityMessageProperty remoteSecurity;
        private Stream upgradedStream;

        public StreamSecurityUpgradeInitiatorAsyncResult(AsyncCallback callback, object state) : base(callback, state)
        {
        }

        public void Begin(Stream stream)
        {
            IAsyncResult result;
            this.originalStream = stream;
            try
            {
                result = this.OnBeginAuthenticateAsClient(this.originalStream, onAuthenticateAsClient);
            }
            catch (AuthenticationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(exception.Message, exception));
            }
            catch (IOException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NegotiationFailedIO", new object[] { exception2.Message }), exception2));
            }
            if (result.CompletedSynchronously)
            {
                this.CompleteAuthenticateAsClient(result);
                base.Complete(true);
            }
        }

        private void CompleteAuthenticateAsClient(IAsyncResult result)
        {
            try
            {
                this.upgradedStream = this.OnCompleteAuthenticateAsClient(result);
            }
            catch (AuthenticationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(exception.Message, exception));
            }
            catch (IOException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("NegotiationFailedIO", new object[] { exception2.Message }), exception2));
            }
            this.remoteSecurity = this.ValidateCreateSecurity();
        }

        public static Stream End(IAsyncResult result, out SecurityMessageProperty remoteSecurity)
        {
            StreamSecurityUpgradeInitiatorAsyncResult result2 = AsyncResult.End<StreamSecurityUpgradeInitiatorAsyncResult>(result);
            remoteSecurity = result2.remoteSecurity;
            return result2.upgradedStream;
        }

        private static void OnAuthenticateAsClient(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                StreamSecurityUpgradeInitiatorAsyncResult asyncState = (StreamSecurityUpgradeInitiatorAsyncResult) result.AsyncState;
                Exception exception = null;
                try
                {
                    asyncState.CompleteAuthenticateAsClient(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                asyncState.Complete(false, exception);
            }
        }

        protected abstract IAsyncResult OnBeginAuthenticateAsClient(Stream stream, AsyncCallback callback);
        protected abstract Stream OnCompleteAuthenticateAsClient(IAsyncResult result);
        protected abstract SecurityMessageProperty ValidateCreateSecurity();
    }
}

