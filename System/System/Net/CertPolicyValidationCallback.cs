namespace System.Net
{
    using System;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;

    internal class CertPolicyValidationCallback
    {
        private ICertificatePolicy m_CertificatePolicy;
        private ExecutionContext m_Context;

        internal CertPolicyValidationCallback()
        {
            this.m_CertificatePolicy = new DefaultCertPolicy();
            this.m_Context = null;
        }

        internal CertPolicyValidationCallback(ICertificatePolicy certificatePolicy)
        {
            this.m_CertificatePolicy = certificatePolicy;
            this.m_Context = ExecutionContext.Capture();
        }

        internal void Callback(object state)
        {
            CallbackContext context = (CallbackContext) state;
            context.result = context.policyWrapper.CheckErrors(context.hostName, context.certificate, context.chain, context.sslPolicyErrors);
        }

        internal bool Invoke(string hostName, ServicePoint servicePoint, X509Certificate certificate, WebRequest request, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            PolicyWrapper policyWrapper = new PolicyWrapper(this.m_CertificatePolicy, servicePoint, request);
            if (this.m_Context == null)
            {
                return policyWrapper.CheckErrors(hostName, certificate, chain, sslPolicyErrors);
            }
            ExecutionContext executionContext = this.m_Context.CreateCopy();
            CallbackContext state = new CallbackContext(policyWrapper, hostName, certificate, chain, sslPolicyErrors);
            ExecutionContext.Run(executionContext, new ContextCallback(this.Callback), state);
            return state.result;
        }

        internal ICertificatePolicy CertificatePolicy
        {
            get
            {
                return this.m_CertificatePolicy;
            }
        }

        internal bool UsesDefault
        {
            get
            {
                return (this.m_Context == null);
            }
        }

        private class CallbackContext
        {
            internal readonly X509Certificate certificate;
            internal readonly X509Chain chain;
            internal readonly string hostName;
            internal readonly PolicyWrapper policyWrapper;
            internal bool result;
            internal readonly SslPolicyErrors sslPolicyErrors;

            internal CallbackContext(PolicyWrapper policyWrapper, string hostName, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                this.policyWrapper = policyWrapper;
                this.hostName = hostName;
                this.certificate = certificate;
                this.chain = chain;
                this.sslPolicyErrors = sslPolicyErrors;
            }
        }
    }
}

