namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class ClientContextProtocol : ContextProtocol, IContextManager
    {
        private System.Uri callbackAddress;
        private ContextMessageProperty context;
        private bool contextInitialized;
        private bool contextManagementEnabled;
        private CookieContainer cookieContainer;
        private IChannel owner;
        private object thisLock;
        private System.Uri uri;

        public ClientContextProtocol(ContextExchangeMechanism contextExchangeMechanism, System.Uri uri, IChannel owner, System.Uri callbackAddress, bool contextManagementEnabled) : base(contextExchangeMechanism)
        {
            if (contextExchangeMechanism == ContextExchangeMechanism.HttpCookie)
            {
                this.cookieContainer = new CookieContainer();
            }
            this.context = ContextMessageProperty.Empty;
            this.contextManagementEnabled = contextManagementEnabled;
            this.owner = owner;
            this.thisLock = new object();
            this.uri = uri;
            this.callbackAddress = callbackAddress;
        }

        private void EnsureInvariants(bool isServerIssued, ContextMessageProperty newContext)
        {
            if (!this.contextManagementEnabled)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ContextManagementNotEnabled")));
            }
            if ((isServerIssued && !this.contextInitialized) || (this.owner.State == CommunicationState.Created))
            {
                lock (this.thisLock)
                {
                    if ((isServerIssued && !this.contextInitialized) || (this.owner.State == CommunicationState.Created))
                    {
                        this.context = newContext;
                        this.contextInitialized = true;
                        return;
                    }
                }
            }
            if (isServerIssued)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("InvalidContextReceived")));
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CachedContextIsImmutable")));
        }

        public IDictionary<string, string> GetContext()
        {
            if (!this.contextManagementEnabled)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ContextManagementNotEnabled")));
            }
            return new Dictionary<string, string>(this.GetCurrentContext().Context);
        }

        private string GetCookieHeaderFromContext(ContextMessageProperty contextMessageProperty)
        {
            if (contextMessageProperty.Context.Count == 0)
            {
                return "WscContext;Max-Age=0";
            }
            return ContextProtocol.HttpCookieToolbox.EncodeContextAsHttpSetCookieHeader(contextMessageProperty, this.Uri);
        }

        private ContextMessageProperty GetCurrentContext()
        {
            if (this.cookieContainer != null)
            {
                lock (this.cookieContainer)
                {
                    if (this.cookieContainer.GetCookies(this.Uri)["WscContext"] == null)
                    {
                        return ContextMessageProperty.Empty;
                    }
                    return this.context;
                }
            }
            return this.context;
        }

        public override void OnIncomingMessage(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            ContextMessageProperty newContext = null;
            if (base.ContextExchangeMechanism == ContextExchangeMechanism.HttpCookie)
            {
                newContext = this.OnReceiveHttpCookies(message);
            }
            else
            {
                newContext = this.OnReceiveSoapContextHeader(message);
            }
            if (newContext != null)
            {
                if (this.contextManagementEnabled)
                {
                    this.EnsureInvariants(true, newContext);
                }
                else
                {
                    newContext.AddOrReplaceInMessage(message);
                }
            }
            if (message.Headers.FindHeader("CallbackContext", "http://schemas.microsoft.com/ws/2008/02/context") != -1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new ProtocolException(System.ServiceModel.SR.GetString("CallbackContextNotExpectedOnIncomingMessageAtClient", new object[] { message.Headers.Action, "CallbackContext", "http://schemas.microsoft.com/ws/2008/02/context" })));
            }
        }

        public override void OnOutgoingMessage(Message message, System.ServiceModel.Channels.RequestContext requestContext)
        {
            CallbackContextMessageProperty property2;
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            ContextMessageProperty contextMessageProperty = null;
            if (ContextMessageProperty.TryGet(message, out contextMessageProperty) && this.contextManagementEnabled)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidMessageContext")));
            }
            if (base.ContextExchangeMechanism == ContextExchangeMechanism.ContextSoapHeader)
            {
                if (this.contextManagementEnabled)
                {
                    contextMessageProperty = this.GetCurrentContext();
                }
                if (contextMessageProperty != null)
                {
                    base.OnSendSoapContextHeader(message, contextMessageProperty);
                }
            }
            else if (base.ContextExchangeMechanism == ContextExchangeMechanism.HttpCookie)
            {
                if (this.contextManagementEnabled)
                {
                    this.OnSendHttpCookies(message, null);
                }
                else
                {
                    this.OnSendHttpCookies(message, contextMessageProperty);
                }
            }
            if (CallbackContextMessageProperty.TryGet(message, out property2))
            {
                EndpointAddress callbackAddress = property2.CallbackAddress;
                if ((callbackAddress == null) && (this.callbackAddress != null))
                {
                    callbackAddress = property2.CreateCallbackAddress(this.callbackAddress);
                }
                if (callbackAddress != null)
                {
                    if (base.ContextExchangeMechanism != ContextExchangeMechanism.ContextSoapHeader)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CallbackContextOnlySupportedInSoap")));
                    }
                    message.Headers.Add(new CallbackContextMessageHeader(callbackAddress, message.Version.Addressing));
                }
            }
        }

        private ContextMessageProperty OnReceiveHttpCookies(Message message)
        {
            ContextMessageProperty context = null;
            object obj2;
            if (message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out obj2))
            {
                HttpResponseMessageProperty property2 = obj2 as HttpResponseMessageProperty;
                if (property2 == null)
                {
                    return context;
                }
                string str = property2.Headers[HttpResponseHeader.SetCookie];
                if (string.IsNullOrEmpty(str))
                {
                    return context;
                }
                lock (this.cookieContainer)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        this.cookieContainer.SetCookies(this.Uri, str);
                        ContextProtocol.HttpCookieToolbox.TryCreateFromHttpCookieHeader(str, out context);
                    }
                    if (!this.contextManagementEnabled)
                    {
                        this.cookieContainer.SetCookies(this.Uri, "WscContext;Max-Age=0");
                    }
                }
            }
            return context;
        }

        private ContextMessageProperty OnReceiveSoapContextHeader(Message message)
        {
            ContextMessageProperty contextFromHeaderIfExists = ContextMessageHeader.GetContextFromHeaderIfExists(message);
            if ((contextFromHeaderIfExists != null) && DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0xf0006, System.ServiceModel.SR.GetString("TraceCodeContextProtocolContextRetrievedFromMessage"), this);
            }
            return contextFromHeaderIfExists;
        }

        private void OnSendHttpCookies(Message message, ContextMessageProperty context)
        {
            string cookieHeader = null;
            if (this.contextManagementEnabled || (context == null))
            {
                lock (this.cookieContainer)
                {
                    cookieHeader = this.cookieContainer.GetCookieHeader(this.Uri);
                    goto Label_00A2;
                }
            }
            if (context != null)
            {
                string cookieHeaderFromContext = this.GetCookieHeaderFromContext(context);
                lock (this.cookieContainer)
                {
                    this.cookieContainer.SetCookies(this.Uri, cookieHeaderFromContext);
                    cookieHeader = this.cookieContainer.GetCookieHeader(this.Uri);
                    this.cookieContainer.SetCookies(this.Uri, "WscContext;Max-Age=0");
                }
            }
        Label_00A2:
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                object obj2;
                HttpRequestMessageProperty property = null;
                if (message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out obj2))
                {
                    property = obj2 as HttpRequestMessageProperty;
                }
                if (property == null)
                {
                    property = new HttpRequestMessageProperty();
                    message.Properties.Add(HttpRequestMessageProperty.Name, property);
                }
                property.Headers.Add(HttpRequestHeader.Cookie, cookieHeader);
            }
        }

        public void SetContext(IDictionary<string, string> context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            ContextMessageProperty newContext = new ContextMessageProperty(context);
            this.EnsureInvariants(false, newContext);
            if (base.ContextExchangeMechanism == ContextExchangeMechanism.HttpCookie)
            {
                lock (this.cookieContainer)
                {
                    this.cookieContainer.SetCookies(this.Uri, this.GetCookieHeaderFromContext(newContext));
                }
            }
        }

        bool IContextManager.Enabled
        {
            get
            {
                return this.contextManagementEnabled;
            }
            set
            {
                if (this.owner.State != CommunicationState.Created)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ChannelIsOpen")));
                }
                this.contextManagementEnabled = value;
            }
        }

        protected System.Uri Uri
        {
            get
            {
                return this.uri;
            }
        }
    }
}

