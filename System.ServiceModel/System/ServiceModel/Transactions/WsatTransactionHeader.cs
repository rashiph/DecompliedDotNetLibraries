namespace System.ServiceModel.Transactions
{
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Transactions;
    using System.Xml;

    internal class WsatTransactionHeader : MessageHeader
    {
        private CoordinationContext context;
        private string wsatHeaderElement;
        private string wsatNamespace;

        public WsatTransactionHeader(CoordinationContext context, ProtocolVersion protocolVersion)
        {
            this.context = context;
            CoordinationStrings strings = CoordinationStrings.Version(protocolVersion);
            this.wsatHeaderElement = strings.CoordinationContext;
            this.wsatNamespace = strings.Namespace;
        }

        public static CoordinationContext GetCoordinationContext(Message message, ProtocolVersion protocolVersion)
        {
            int num;
            CoordinationContext context;
            CoordinationStrings strings = CoordinationStrings.Version(protocolVersion);
            string coordinationContext = strings.CoordinationContext;
            string ns = strings.Namespace;
            try
            {
                num = message.Headers.FindHeader(coordinationContext, ns);
            }
            catch (MessageHeaderException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                return null;
            }
            if (num < 0)
            {
                return null;
            }
            XmlDictionaryReader readerAtHeader = message.Headers.GetReaderAtHeader(num);
            using (readerAtHeader)
            {
                context = GetCoordinationContext(readerAtHeader, protocolVersion);
            }
            MessageHeaderInfo headerInfo = message.Headers[num];
            if (!message.Headers.UnderstoodHeaders.Contains(headerInfo))
            {
                message.Headers.UnderstoodHeaders.Add(headerInfo);
            }
            return context;
        }

        public static CoordinationContext GetCoordinationContext(XmlDictionaryReader reader, ProtocolVersion protocolVersion)
        {
            CoordinationContext context;
            CoordinationXmlDictionaryStrings strings = CoordinationXmlDictionaryStrings.Version(protocolVersion);
            try
            {
                context = CoordinationContext.ReadFrom(reader, strings.CoordinationContext, strings.Namespace, protocolVersion);
            }
            catch (InvalidCoordinationContextException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException(System.ServiceModel.SR.GetString("WsatHeaderCorrupt"), exception));
            }
            return context;
        }

        public static bool IsStartElement(XmlDictionaryReader reader, ProtocolVersion protocolVersion)
        {
            CoordinationXmlDictionaryStrings strings = CoordinationXmlDictionaryStrings.Version(protocolVersion);
            return reader.IsStartElement(strings.CoordinationContext, strings.Namespace);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            this.context.WriteContent(writer);
        }

        public static void WriteElement(XmlDictionaryWriter writer, CoordinationContext context, ProtocolVersion protocolVersion)
        {
            CoordinationXmlDictionaryStrings strings = CoordinationXmlDictionaryStrings.Version(protocolVersion);
            context.WriteTo(writer, strings.CoordinationContext, strings.Namespace);
        }

        public override bool MustUnderstand
        {
            get
            {
                return true;
            }
        }

        public override string Name
        {
            get
            {
                return this.wsatHeaderElement;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.wsatNamespace;
            }
        }
    }
}

