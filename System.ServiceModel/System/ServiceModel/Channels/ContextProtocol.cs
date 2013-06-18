namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal abstract class ContextProtocol
    {
        private System.ServiceModel.Channels.ContextExchangeMechanism contextExchangeMechanism;

        protected ContextProtocol(System.ServiceModel.Channels.ContextExchangeMechanism contextExchangeMechanism)
        {
            if (!ContextExchangeMechanismHelper.IsDefined(contextExchangeMechanism))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("contextExchangeMechanism"));
            }
            this.contextExchangeMechanism = contextExchangeMechanism;
        }

        public abstract void OnIncomingMessage(Message message);
        public abstract void OnOutgoingMessage(Message message, RequestContext requestContext);
        protected void OnSendSoapContextHeader(Message message, ContextMessageProperty context)
        {
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (context == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (context.Context.Count > 0)
            {
                message.Headers.Add(new ContextMessageHeader(context.Context));
            }
            if (System.ServiceModel.DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0xf0005, System.ServiceModel.SR.GetString("TraceCodeContextProtocolContextAddedToMessage"), this);
            }
        }

        protected System.ServiceModel.Channels.ContextExchangeMechanism ContextExchangeMechanism
        {
            get
            {
                return this.contextExchangeMechanism;
            }
        }

        internal static class HttpCookieToolbox
        {
            public const string ContextHttpCookieName = "WscContext";
            public const string RemoveContextHttpCookieHeader = "WscContext;Max-Age=0";

            public static string EncodeContextAsHttpSetCookieHeader(ContextMessageProperty context, Uri uri)
            {
                if (uri == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("uri");
                }
                if (context == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
                }
                MemoryStream output = new MemoryStream();
                XmlWriterSettings settings = new XmlWriterSettings {
                    OmitXmlDeclaration = true
                };
                XmlWriter writer = XmlWriter.Create(output, settings);
                new ContextMessageHeader(context.Context).WriteHeader(writer, MessageVersion.Default);
                writer.Flush();
                return string.Format(CultureInfo.InvariantCulture, "{0}=\"{1}\";Path={2}", new object[] { "WscContext", Convert.ToBase64String(output.GetBuffer(), 0, (int) output.Length), uri.AbsolutePath });
            }

            public static bool TryCreateFromHttpCookieHeader(string httpCookieHeader, out ContextMessageProperty context)
            {
                if (httpCookieHeader == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("httpCookieHeader");
                }
                context = null;
                foreach (string str in httpCookieHeader.Split(new char[] { ';' }))
                {
                    string str2 = str.Trim();
                    if (str2.StartsWith("WscContext", StringComparison.Ordinal))
                    {
                        int index = str2.IndexOf('=');
                        if (index < 0)
                        {
                            context = new ContextMessageProperty();
                            break;
                        }
                        if (index < (str2.Length - 1))
                        {
                            string s = str2.Substring(index + 1).Trim();
                            if (((s.Length > 1) && (s[0] == '"')) && (s[s.Length - 1] == '"'))
                            {
                                s = s.Substring(1, s.Length - 2);
                            }
                            try
                            {
                                context = ContextMessageHeader.ParseContextHeader(XmlReader.Create(new MemoryStream(Convert.FromBase64String(s))));
                                break;
                            }
                            catch (SerializationException exception)
                            {
                                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                            }
                            catch (ProtocolException exception2)
                            {
                                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                            }
                        }
                    }
                }
                return (context != null);
            }
        }
    }
}

