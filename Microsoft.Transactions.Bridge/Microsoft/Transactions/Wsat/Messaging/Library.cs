namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal static class Library
    {
        public static Message CreateFaultMessage(UniqueId messageID, MessageVersion messageVersion, Fault fault)
        {
            MessageFault fault2 = MessageFault.CreateFault(FaultCode.CreateSenderFaultCode(fault.Code), fault.Reason);
            Message message = Message.CreateMessage(messageVersion, fault2, messageVersion.Addressing.FaultAction);
            message.Headers.Action = fault.Action;
            if (messageID != null)
            {
                message.Headers.RelatesTo = messageID;
            }
            return message;
        }

        public static FaultCode GetBaseFaultCode(MessageFault fault)
        {
            FaultCode subCode = fault.Code;
            if (subCode != null)
            {
                while (subCode.SubCode != null)
                {
                    subCode = subCode.SubCode;
                }
            }
            return subCode;
        }

        public static string GetFaultCodeName(MessageFault fault)
        {
            FaultCode subCode = fault.Code;
            if (subCode == null)
            {
                return "unknown";
            }
            if (subCode.SubCode != null)
            {
                subCode = subCode.SubCode;
                if (subCode == null)
                {
                    return "unknown";
                }
            }
            return subCode.Name;
        }

        public static string GetFaultCodeReason(MessageFault fault)
        {
            FaultReasonText matchingTranslation;
            FaultReason reason = fault.Reason;
            if (reason == null)
            {
                return "unknown";
            }
            try
            {
                matchingTranslation = reason.GetMatchingTranslation(CultureInfo.CurrentCulture);
            }
            catch (ArgumentException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                return "unknown";
            }
            return matchingTranslation.Text;
        }

        public static EndpointAddress GetFaultToHeader(MessageHeaders headers, ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(Library), "GetFaultToHeader");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return GetFaultToHeader10(headers);

                case ProtocolVersion.Version11:
                    return GetFaultToHeader11(headers);
            }
            return null;
        }

        private static EndpointAddress GetFaultToHeader10(MessageHeaders headers)
        {
            EndpointAddress faultTo = headers.FaultTo;
            if (faultTo == null)
            {
                faultTo = headers.ReplyTo;
                if (faultTo == null)
                {
                    faultTo = headers.From;
                }
            }
            return faultTo;
        }

        private static EndpointAddress GetFaultToHeader11(MessageHeaders headers)
        {
            EndpointAddress from = headers.From;
            if (((from != null) && !from.IsNone) && !from.IsAnonymous)
            {
                return from;
            }
            return null;
        }

        public static EndpointAddress GetReplyToHeader(MessageHeaders headers)
        {
            EndpointAddress replyTo = headers.ReplyTo;
            if (((replyTo != null) && !replyTo.IsNone) && !replyTo.IsAnonymous)
            {
                return replyTo;
            }
            return headers.From;
        }

        public static void SendFaultResponse(Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result, Fault fault)
        {
            Message reply = CreateFaultMessage(result.MessageId, result.MessageVersion, fault);
            result.Finished(reply);
        }
    }
}

