namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Xml;

    internal class OperationSelector : IDispatchOperationSelector
    {
        private IPeerNodeMessageHandling messageHandler;

        public OperationSelector(IPeerNodeMessageHandling messageHandler)
        {
            this.messageHandler = messageHandler;
        }

        public string SelectOperation(ref Message message)
        {
            string action = message.Headers.Action;
            string str2 = null;
            byte[] defaultId = PeerNodeImplementation.DefaultId;
            string str3 = PeerStrings.FindAction(action);
            Uri via = null;
            Uri uri2 = null;
            bool flag = false;
            PeerMessageProperty property = new PeerMessageProperty();
            if (str3 != null)
            {
                return str3;
            }
            try
            {
                str2 = PeerMessageHelpers.GetHeaderString(message.Headers, "FloodMessage", "http://schemas.microsoft.com/net/2006/05/peer");
                via = PeerMessageHelpers.GetHeaderUri(message.Headers, "PeerVia", "http://schemas.microsoft.com/net/2006/05/peer");
                uri2 = PeerMessageHelpers.GetHeaderUri(message.Headers, "PeerTo", "http://schemas.microsoft.com/net/2006/05/peer");
            }
            catch (MessageHeaderException exception)
            {
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                return "Fault";
            }
            catch (SerializationException exception2)
            {
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                return "Fault";
            }
            catch (XmlException exception3)
            {
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Warning);
                return "Fault";
            }
            property.PeerVia = via;
            property.PeerTo = uri2;
            message.Properties.Add("PeerProperty", property);
            if (!(str2 == "PeerFlooder"))
            {
                return null;
            }
            try
            {
                if (!this.messageHandler.ValidateIncomingMessage(ref message, via))
                {
                    property.SkipLocalChannels = true;
                    flag = true;
                    TurnOffSecurityHeader(message);
                }
                if (this.messageHandler.IsNotSeenBefore(message, out defaultId, out property.CacheMiss))
                {
                    property.MessageVerified = true;
                }
                else if (!flag)
                {
                    property.SkipLocalChannels = true;
                }
                if (defaultId == PeerNodeImplementation.DefaultId)
                {
                    return "Fault";
                }
            }
            catch (MessageHeaderException exception4)
            {
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Warning);
                return "Fault";
            }
            catch (SerializationException exception5)
            {
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception5, TraceEventType.Warning);
                return "Fault";
            }
            catch (XmlException exception6)
            {
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception6, TraceEventType.Warning);
                return "Fault";
            }
            catch (MessageSecurityException exception7)
            {
                if (!exception7.ReplayDetected)
                {
                    return "Fault";
                }
                System.ServiceModel.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception7, TraceEventType.Information);
            }
            return "FloodMessage";
        }

        public static void TurnOffSecurityHeader(Message message)
        {
            int i = message.Headers.FindHeader("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            if (i >= 0)
            {
                message.Headers.AddUnderstood(i);
            }
        }
    }
}

