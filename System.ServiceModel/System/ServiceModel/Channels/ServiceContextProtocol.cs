namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class ServiceContextProtocol : ContextProtocol
    {
        public ServiceContextProtocol(ContextExchangeMechanism contextExchangeMechanism) : base(contextExchangeMechanism)
        {
        }

        public override void OnIncomingMessage(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (base.ContextExchangeMechanism == ContextExchangeMechanism.HttpCookie)
            {
                this.OnReceiveHttpCookies(message);
            }
            else
            {
                this.OnReceiveSoapContextHeader(message);
            }
            int headerIndex = message.Headers.FindHeader("CallbackContext", "http://schemas.microsoft.com/ws/2008/02/context");
            if (headerIndex > 0)
            {
                CallbackContextMessageProperty property = CallbackContextMessageHeader.ParseCallbackContextHeader(message.Headers.GetReaderAtHeader(headerIndex), message.Version.Addressing);
                message.Properties.Add(CallbackContextMessageProperty.Name, property);
            }
            ContextExchangeCorrelationHelper.AddIncomingContextCorrelationData(message);
        }

        public override void OnOutgoingMessage(Message message, System.ServiceModel.Channels.RequestContext requestContext)
        {
            ContextMessageProperty property;
            CallbackContextMessageProperty property2;
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (ContextMessageProperty.TryGet(message, out property))
            {
                if (base.ContextExchangeMechanism == ContextExchangeMechanism.HttpCookie)
                {
                    Uri requestUri = null;
                    if (requestContext.RequestMessage.Properties != null)
                    {
                        requestUri = requestContext.RequestMessage.Properties.Via;
                    }
                    if (requestUri == null)
                    {
                        requestUri = requestContext.RequestMessage.Headers.To;
                    }
                    this.OnSendHttpCookies(message, property, requestUri);
                }
                else
                {
                    base.OnSendSoapContextHeader(message, property);
                }
            }
            if (CallbackContextMessageProperty.TryGet(message, out property2))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.ServiceModel.SR.GetString("CallbackContextNotExpectedOnOutgoingMessageAtServer", new object[] { message.Headers.Action })));
            }
        }

        private void OnReceiveHttpCookies(Message message)
        {
            object obj2;
            if (message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out obj2))
            {
                HttpRequestMessageProperty property = obj2 as HttpRequestMessageProperty;
                if (property != null)
                {
                    ContextMessageProperty property2;
                    string str = property.Headers[HttpRequestHeader.Cookie];
                    if (!string.IsNullOrEmpty(str) && ContextProtocol.HttpCookieToolbox.TryCreateFromHttpCookieHeader(str, out property2))
                    {
                        property2.AddOrReplaceInMessage(message);
                    }
                }
            }
        }

        private void OnReceiveSoapContextHeader(Message message)
        {
            ContextMessageProperty contextFromHeaderIfExists = ContextMessageHeader.GetContextFromHeaderIfExists(message);
            if (contextFromHeaderIfExists != null)
            {
                contextFromHeaderIfExists.AddOrReplaceInMessage(message);
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0xf0006, System.ServiceModel.SR.GetString("TraceCodeContextProtocolContextRetrievedFromMessage"), this);
                }
            }
        }

        private void OnSendHttpCookies(Message message, ContextMessageProperty context, Uri requestUri)
        {
            object obj2;
            HttpResponseMessageProperty property = null;
            if (message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out obj2))
            {
                property = obj2 as HttpResponseMessageProperty;
            }
            if (property == null)
            {
                property = new HttpResponseMessageProperty();
                message.Properties.Add(HttpResponseMessageProperty.Name, property);
            }
            string str = ContextProtocol.HttpCookieToolbox.EncodeContextAsHttpSetCookieHeader(context, requestUri);
            property.Headers.Add(HttpResponseHeader.SetCookie, str);
        }
    }
}

