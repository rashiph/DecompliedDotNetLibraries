namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class EnlistmentHeader : AddressHeader
    {
        private Guid enlistment;
        public const string HeaderName = "Enlistment";
        public const string HeaderNamespace = "http://schemas.microsoft.com/ws/2006/02/transactions";
        private ControlProtocol protocol;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EnlistmentHeader(Guid enlistment) : this(enlistment, ControlProtocol.None)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EnlistmentHeader(Guid enlistment, ControlProtocol protocol)
        {
            this.enlistment = enlistment;
            this.protocol = protocol;
        }

        protected override void OnWriteAddressHeaderContents(XmlDictionaryWriter writer)
        {
            if (this.protocol != ControlProtocol.None)
            {
                writer.WriteStartAttribute(XD.DotNetAtomicTransactionExternalDictionary.Protocol, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
                writer.WriteValue((int) this.protocol);
                writer.WriteEndAttribute();
            }
            writer.WriteValue(this.enlistment);
        }

        protected override void OnWriteStartAddressHeader(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement("mstx", XD.DotNetAtomicTransactionExternalDictionary.Enlistment, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
        }

        public static bool ReadFrom(Message message, out Guid enlistmentId, out ControlProtocol protocol)
        {
            int num;
            enlistmentId = Guid.Empty;
            protocol = ControlProtocol.None;
            try
            {
                num = message.Headers.FindHeader("Enlistment", "http://schemas.microsoft.com/ws/2006/02/transactions", new string[] { string.Empty });
            }
            catch (MessageHeaderException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                return false;
            }
            if (num < 0)
            {
                return false;
            }
            XmlDictionaryReader readerAtHeader = message.Headers.GetReaderAtHeader(num);
            using (readerAtHeader)
            {
                ReadFrom(readerAtHeader, out enlistmentId, out protocol);
            }
            MessageHeaderInfo headerInfo = message.Headers[num];
            if (!message.Headers.UnderstoodHeaders.Contains(headerInfo))
            {
                message.Headers.UnderstoodHeaders.Add(headerInfo);
            }
            return true;
        }

        public static void ReadFrom(XmlDictionaryReader reader, out Guid enlistment, out ControlProtocol protocol)
        {
            try
            {
                if (reader.IsEmptyElement || !reader.IsStartElement(XD.DotNetAtomicTransactionExternalDictionary.Enlistment, XD.DotNetAtomicTransactionExternalDictionary.Namespace))
                {
                    throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnlistmentHeaderException(Microsoft.Transactions.SR.GetString("InvalidEnlistmentHeader")));
                }
                string attribute = reader.GetAttribute(XD.DotNetAtomicTransactionExternalDictionary.Protocol, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
                if (attribute == null)
                {
                    protocol = ControlProtocol.None;
                }
                else
                {
                    protocol = (ControlProtocol) XmlConvert.ToInt32(attribute.Trim());
                    if ((protocol != ControlProtocol.Durable2PC) && (protocol != ControlProtocol.Volatile2PC))
                    {
                        throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnlistmentHeaderException(Microsoft.Transactions.SR.GetString("InvalidEnlistmentHeader")));
                    }
                }
                enlistment = reader.ReadElementContentAsGuid();
            }
            catch (FormatException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnlistmentHeaderException(Microsoft.Transactions.SR.GetString("InvalidEnlistmentHeader"), exception));
            }
            catch (OverflowException exception2)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnlistmentHeaderException(Microsoft.Transactions.SR.GetString("InvalidEnlistmentHeader"), exception2));
            }
            catch (XmlException exception3)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Warning);
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnlistmentHeaderException(Microsoft.Transactions.SR.GetString("InvalidEnlistmentHeader"), exception3));
            }
        }

        public override string Name
        {
            get
            {
                return "Enlistment";
            }
        }

        public override string Namespace
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/02/transactions";
            }
        }
    }
}

