namespace System.ServiceModel.Transactions
{
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class WsatRegistrationHeader : AddressHeader
    {
        private string contextId;
        private const string HeaderName = "RegisterInfo";
        private const string HeaderNamespace = "http://schemas.microsoft.com/ws/2006/02/transactions";
        private string tokenId;
        private Guid transactionId;

        public WsatRegistrationHeader(Guid transactionId, string contextId, string tokenId)
        {
            this.transactionId = transactionId;
            this.contextId = contextId;
            this.tokenId = tokenId;
        }

        protected override void OnWriteAddressHeaderContents(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(XD.DotNetAtomicTransactionExternalDictionary.LocalTransactionId, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
            writer.WriteValue(this.transactionId);
            writer.WriteEndElement();
            if (this.contextId != null)
            {
                writer.WriteStartElement(XD.DotNetAtomicTransactionExternalDictionary.ContextId, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
                writer.WriteValue(this.contextId);
                writer.WriteEndElement();
            }
            if (this.tokenId != null)
            {
                writer.WriteStartElement(XD.DotNetAtomicTransactionExternalDictionary.TokenId, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
                writer.WriteValue(this.tokenId);
                writer.WriteEndElement();
            }
        }

        protected override void OnWriteStartAddressHeader(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement("mstx", XD.DotNetAtomicTransactionExternalDictionary.RegisterInfo, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
        }

        public static WsatRegistrationHeader ReadFrom(Message message)
        {
            int num;
            WsatRegistrationHeader header;
            try
            {
                num = message.Headers.FindHeader("RegisterInfo", "http://schemas.microsoft.com/ws/2006/02/transactions");
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
            XmlDictionaryReader reader2 = readerAtHeader;
            try
            {
                header = ReadFrom(readerAtHeader);
            }
            catch (XmlException exception2)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnlistmentHeaderException(exception2.Message, exception2));
            }
            finally
            {
                if (reader2 != null)
                {
                    reader2.Dispose();
                }
            }
            MessageHeaderInfo headerInfo = message.Headers[num];
            if (!message.Headers.UnderstoodHeaders.Contains(headerInfo))
            {
                message.Headers.UnderstoodHeaders.Add(headerInfo);
            }
            return header;
        }

        private static WsatRegistrationHeader ReadFrom(XmlDictionaryReader reader)
        {
            string str;
            string str2;
            reader.ReadFullStartElement(XD.DotNetAtomicTransactionExternalDictionary.RegisterInfo, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
            reader.MoveToStartElement(XD.DotNetAtomicTransactionExternalDictionary.LocalTransactionId, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
            Guid transactionId = reader.ReadElementContentAsGuid();
            if (transactionId == Guid.Empty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("InvalidRegistrationHeaderTransactionId")));
            }
            if (reader.IsStartElement(XD.DotNetAtomicTransactionExternalDictionary.ContextId, XD.DotNetAtomicTransactionExternalDictionary.Namespace))
            {
                Uri uri;
                str = reader.ReadElementContentAsString().Trim();
                if (((str.Length == 0) || (str.Length > 0x100)) || !Uri.TryCreate(str, UriKind.Absolute, out uri))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("InvalidRegistrationHeaderIdentifier")));
                }
            }
            else
            {
                str = null;
            }
            if (reader.IsStartElement(XD.DotNetAtomicTransactionExternalDictionary.TokenId, XD.DotNetAtomicTransactionExternalDictionary.Namespace))
            {
                str2 = reader.ReadElementContentAsString().Trim();
                if (str2.Length == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("InvalidRegistrationHeaderTokenId")));
                }
            }
            else
            {
                str2 = null;
            }
            while (reader.IsStartElement())
            {
                reader.Skip();
            }
            reader.ReadEndElement();
            return new WsatRegistrationHeader(transactionId, str, str2);
        }

        public string ContextId
        {
            get
            {
                return this.contextId;
            }
        }

        public override string Name
        {
            get
            {
                return "RegisterInfo";
            }
        }

        public override string Namespace
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/02/transactions";
            }
        }

        public string TokenId
        {
            get
            {
                return this.tokenId;
            }
        }

        public Guid TransactionId
        {
            get
            {
                return this.transactionId;
            }
        }
    }
}

