namespace System.Net
{
    using System;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;

    internal class ServerCertValidationCallback
    {
        private ExecutionContext m_Context;
        private RemoteCertificateValidationCallback m_ValidationCallback;

        internal ServerCertValidationCallback(RemoteCertificateValidationCallback validationCallback)
        {
            this.m_ValidationCallback = validationCallback;
            this.m_Context = ExecutionContext.Capture();
        }

        internal void Callback(object state)
        {
            CallbackContext context = (CallbackContext) state;
            context.result = this.m_ValidationCallback(context.request, context.certificate, context.chain, context.sslPolicyErrors);
        }

        internal bool Invoke(object request, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (this.m_Context == null)
            {
                return this.m_ValidationCallback(request, certificate, chain, sslPolicyErrors);
            }
            ExecutionContext executionContext = this.m_Context.CreateCopy();
            CallbackContext state = new CallbackContext(request, certificate, chain, sslPolicyErrors);
            ExecutionContext.Run(executionContext, new ContextCallback(this.Callback), state);
            return state.result;
        }

        internal RemoteCertificateValidationCallback ValidationCallback
        {
            get
            {
                return this.m_ValidationCallback;
            }
        }

        private class CallbackContext
        {
            internal readonly X509Certificate certificate;
            internal readonly X509Chain chain;
            internal readonly object request;
            internal bool result;
            internal readonly SslPolicyErrors sslPolicyErrors;

            internal CallbackContext(object request, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                this.request = request;
                this.certificate = certificate;
                this.chain = chain;
                this.sslPolicyErrors = sslPolicyErrors;
            }
        }
    }
}

