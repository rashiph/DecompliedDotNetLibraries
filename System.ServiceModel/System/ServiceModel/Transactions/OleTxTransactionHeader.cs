namespace System.ServiceModel.Transactions
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Transactions;
    using System.Xml;

    internal class OleTxTransactionHeader : MessageHeader
    {
        private static readonly XmlDictionaryString CoordinationNamespace = XD.CoordinationExternal10Dictionary.Namespace;
        private const string OleTxHeaderElement = "OleTxTransaction";
        private const string OleTxNamespace = "http://schemas.microsoft.com/ws/2006/02/tx/oletx";
        private byte[] propagationToken;
        private System.ServiceModel.Transactions.WsatExtendedInformation wsatInfo;

        public OleTxTransactionHeader(byte[] propagationToken, System.ServiceModel.Transactions.WsatExtendedInformation wsatInfo)
        {
            this.propagationToken = propagationToken;
            this.wsatInfo = wsatInfo;
        }

        public static bool IsStartPropagationTokenElement(XmlDictionaryReader reader)
        {
            return reader.IsStartElement(XD.OleTxTransactionExternalDictionary.PropagationToken, XD.OleTxTransactionExternalDictionary.Namespace);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (this.wsatInfo != null)
            {
                if (this.wsatInfo.Timeout != 0)
                {
                    writer.WriteAttributeString(XD.CoordinationExternalDictionary.Expires, CoordinationNamespace, XmlConvert.ToString(this.wsatInfo.Timeout));
                }
                if (!string.IsNullOrEmpty(this.wsatInfo.Identifier))
                {
                    writer.WriteAttributeString(XD.CoordinationExternalDictionary.Identifier, CoordinationNamespace, this.wsatInfo.Identifier);
                }
            }
            WritePropagationTokenElement(writer, this.propagationToken);
        }

        public static OleTxTransactionHeader ReadFrom(Message message)
        {
            int num;
            OleTxTransactionHeader header;
            try
            {
                num = message.Headers.FindHeader("OleTxTransaction", "http://schemas.microsoft.com/ws/2006/02/tx/oletx");
            }
            catch (MessageHeaderException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException(System.ServiceModel.SR.GetString("OleTxHeaderCorrupt"), exception));
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
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException(System.ServiceModel.SR.GetString("OleTxHeaderCorrupt"), exception2));
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

        private static OleTxTransactionHeader ReadFrom(XmlDictionaryReader reader)
        {
            System.ServiceModel.Transactions.WsatExtendedInformation wsatInfo = null;
            if (reader.IsStartElement(XD.OleTxTransactionExternalDictionary.OleTxTransaction, XD.OleTxTransactionExternalDictionary.Namespace))
            {
                Uri uri;
                string attribute = reader.GetAttribute(XD.CoordinationExternalDictionary.Identifier, CoordinationNamespace);
                if (!string.IsNullOrEmpty(attribute) && !Uri.TryCreate(attribute, UriKind.Absolute, out uri))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("InvalidWsatExtendedInfo")));
                }
                string str2 = reader.GetAttribute(XD.CoordinationExternalDictionary.Expires, CoordinationNamespace);
                uint timeout = 0;
                if (!string.IsNullOrEmpty(str2))
                {
                    try
                    {
                        timeout = XmlConvert.ToUInt32(str2);
                    }
                    catch (FormatException exception)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("InvalidWsatExtendedInfo"), exception));
                    }
                    catch (OverflowException exception2)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Error);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("InvalidWsatExtendedInfo"), exception2));
                    }
                }
                if (!string.IsNullOrEmpty(attribute) || (timeout != 0))
                {
                    wsatInfo = new System.ServiceModel.Transactions.WsatExtendedInformation(attribute, timeout);
                }
            }
            reader.ReadFullStartElement(XD.OleTxTransactionExternalDictionary.OleTxTransaction, XD.OleTxTransactionExternalDictionary.Namespace);
            byte[] propagationToken = ReadPropagationTokenElement(reader);
            while (reader.IsStartElement())
            {
                reader.Skip();
            }
            reader.ReadEndElement();
            return new OleTxTransactionHeader(propagationToken, wsatInfo);
        }

        public static byte[] ReadPropagationTokenElement(XmlDictionaryReader reader)
        {
            reader.ReadFullStartElement(XD.OleTxTransactionExternalDictionary.PropagationToken, XD.OleTxTransactionExternalDictionary.Namespace);
            byte[] buffer = reader.ReadContentAsBase64();
            if (buffer.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("InvalidPropagationToken")));
            }
            reader.ReadEndElement();
            return buffer;
        }

        public static void WritePropagationTokenElement(XmlDictionaryWriter writer, byte[] propagationToken)
        {
            writer.WriteStartElement(XD.OleTxTransactionExternalDictionary.PropagationToken, XD.OleTxTransactionExternalDictionary.Namespace);
            writer.WriteBase64(propagationToken, 0, propagationToken.Length);
            writer.WriteEndElement();
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
                return "OleTxTransaction";
            }
        }

        public override string Namespace
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/02/tx/oletx";
            }
        }

        public byte[] PropagationToken
        {
            get
            {
                return this.propagationToken;
            }
        }

        public System.ServiceModel.Transactions.WsatExtendedInformation WsatExtendedInformation
        {
            get
            {
                return this.wsatInfo;
            }
        }
    }
}

