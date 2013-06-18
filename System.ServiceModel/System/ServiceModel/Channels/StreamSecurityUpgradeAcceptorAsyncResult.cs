namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Authentication;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;

    internal abstract class StreamSecurityUpgradeAcceptorAsyncResult : TraceAsyncResult
    {
        private static AsyncCallback onAuthenticateAsServer = Fx.ThunkCallback(new AsyncCallback(StreamSecurityUpgradeAcceptorAsyncResult.OnAuthenticateAsServer));
        private SecurityMessageProperty remoteSecurity;
        private Stream upgradedStream;

        protected StreamSecurityUpgradeAcceptorAsyncResult(AsyncCallback callback, object state) : base(callback, state)
        {
        }

        public void Begin(Stream stream)
        {
            IAsyncResult result;
            try
            {
                result = this.OnBegin(stream, onAuthenticateAsServer);
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
                this.CompleteAuthenticateAsServer(result);
                base.Complete(true);
            }
        }

        private void CompleteAuthenticateAsServer(IAsyncResult result)
        {
            try
            {
                this.upgradedStream = this.OnCompleteAuthenticateAsServer(result);
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
            StreamSecurityUpgradeAcceptorAsyncResult result2 = AsyncResult.End<StreamSecurityUpgradeAcceptorAsyncResult>(result);
            remoteSecurity = result2.remoteSecurity;
            return result2.upgradedStream;
        }

        private static void OnAuthenticateAsServer(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                StreamSecurityUpgradeAcceptorAsyncResult asyncState = (StreamSecurityUpgradeAcceptorAsyncResult) result.AsyncState;
                Exception exception = null;
                try
                {
                    asyncState.CompleteAuthenticateAsServer(result);
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

        protected abstract IAsyncResult OnBegin(Stream stream, AsyncCallback callback);
        protected abstract Stream OnCompleteAuthenticateAsServer(IAsyncResult result);
        protected abstract SecurityMessageProperty ValidateCreateSecurity();
    }
}

