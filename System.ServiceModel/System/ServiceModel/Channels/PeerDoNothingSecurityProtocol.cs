namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    internal class PeerDoNothingSecurityProtocol : SecurityProtocol
    {
        public PeerDoNothingSecurityProtocol(SecurityProtocolFactory factory) : base(factory, null, null)
        {
        }

        public override void OnAbort()
        {
        }

        public override void OnClose(TimeSpan timeout)
        {
        }

        public override void OnOpen(TimeSpan timeout)
        {
        }

        public override void SecureOutgoingMessage(ref Message message, TimeSpan timeout)
        {
        }

        public override void VerifyIncomingMessage(ref Message request, TimeSpan timeout)
        {
            try
            {
                int i = request.Headers.FindHeader("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
                if (i >= 0)
                {
                    request.Headers.AddUnderstood(i);
                }
            }
            catch (MessageHeaderException exception)
            {
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
            }
            catch (XmlException exception2)
            {
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
            }
            catch (SerializationException exception3)
            {
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
            }
        }
    }
}

