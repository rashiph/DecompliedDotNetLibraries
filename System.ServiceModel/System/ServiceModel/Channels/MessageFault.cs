namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    public abstract class MessageFault
    {
        protected MessageFault()
        {
        }

        public static MessageFault CreateFault(Message message, int maxBufferSize)
        {
            MessageFault fault2;
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("message"));
            }
            XmlDictionaryReader readerAtBodyContents = message.GetReaderAtBodyContents();
            XmlDictionaryReader reader2 = readerAtBodyContents;
            try
            {
                MessageFault fault;
                EnvelopeVersion envelope = message.Version.Envelope;
                if (envelope == EnvelopeVersion.Soap12)
                {
                    fault = ReceivedFault.CreateFault12(readerAtBodyContents, maxBufferSize);
                }
                else if (envelope == EnvelopeVersion.Soap11)
                {
                    fault = ReceivedFault.CreateFault11(readerAtBodyContents, maxBufferSize);
                }
                else
                {
                    if (envelope != EnvelopeVersion.None)
                    {
                        throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EnvelopeVersionUnknown", new object[] { envelope.ToString() })), message);
                    }
                    fault = ReceivedFault.CreateFaultNone(readerAtBodyContents, maxBufferSize);
                }
                message.ReadFromBodyContentsToEnd(readerAtBodyContents);
                fault2 = fault;
            }
            catch (InvalidOperationException exception)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorDeserializingFault"), exception));
            }
            catch (FormatException exception2)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorDeserializingFault"), exception2));
            }
            catch (XmlException exception3)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("SFxErrorDeserializingFault"), exception3));
            }
            finally
            {
                if (reader2 != null)
                {
                    reader2.Dispose();
                }
            }
            return fault2;
        }

        public static MessageFault CreateFault(FaultCode code, FaultReason reason)
        {
            return CreateFault(code, reason, null, null, "", "");
        }

        public static MessageFault CreateFault(FaultCode code, string reason)
        {
            return CreateFault(code, new FaultReason(reason));
        }

        public static MessageFault CreateFault(FaultCode code, FaultReason reason, object detail)
        {
            return CreateFault(code, reason, detail, DataContractSerializerDefaults.CreateSerializer((detail == null) ? typeof(object) : detail.GetType(), 0x7fffffff), "", "");
        }

        public static MessageFault CreateFault(FaultCode code, FaultReason reason, object detail, XmlObjectSerializer serializer)
        {
            return CreateFault(code, reason, detail, serializer, "", "");
        }

        public static MessageFault CreateFault(FaultCode code, FaultReason reason, object detail, XmlObjectSerializer serializer, string actor)
        {
            if (serializer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            }
            return CreateFault(code, reason, detail, serializer, actor, actor);
        }

        public static MessageFault CreateFault(FaultCode code, FaultReason reason, object detail, XmlObjectSerializer serializer, string actor, string node)
        {
            if (code == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("code"));
            }
            if (reason == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reason"));
            }
            if (actor == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("actor"));
            }
            if (node == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("node"));
            }
            return new XmlObjectSerializerFault(code, reason, detail, serializer, actor, node);
        }

        public T GetDetail<T>()
        {
            return this.GetDetail<T>(DataContractSerializerDefaults.CreateSerializer(typeof(T), 0x7fffffff));
        }

        public T GetDetail<T>(XmlObjectSerializer serializer)
        {
            if (serializer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            }
            XmlDictionaryReader readerAtDetailContents = this.GetReaderAtDetailContents();
            T local = (T) serializer.ReadObject(readerAtDetailContents);
            if (!readerAtDetailContents.EOF)
            {
                readerAtDetailContents.MoveToContent();
                if ((readerAtDetailContents.NodeType != XmlNodeType.EndElement) && !readerAtDetailContents.EOF)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("ExtraContentIsPresentInFaultDetail")));
                }
            }
            return local;
        }

        public XmlDictionaryReader GetReaderAtDetailContents()
        {
            if (!this.HasDetail)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FaultDoesNotHaveAnyDetail")));
            }
            return this.OnGetReaderAtDetailContents();
        }

        protected virtual XmlDictionaryReader OnGetReaderAtDetailContents()
        {
            XmlBuffer buffer = new XmlBuffer(0x7fffffff);
            XmlDictionaryWriter writer = buffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            this.OnWriteDetail(writer, EnvelopeVersion.Soap12);
            buffer.CloseSection();
            buffer.Close();
            XmlDictionaryReader reader = buffer.GetReader(0);
            reader.Read();
            return reader;
        }

        protected virtual void OnWriteDetail(XmlDictionaryWriter writer, EnvelopeVersion version)
        {
            this.OnWriteStartDetail(writer, version);
            this.OnWriteDetailContents(writer);
            writer.WriteEndElement();
        }

        protected abstract void OnWriteDetailContents(XmlDictionaryWriter writer);
        protected virtual void OnWriteStartDetail(XmlDictionaryWriter writer, EnvelopeVersion version)
        {
            if (version == EnvelopeVersion.Soap12)
            {
                writer.WriteStartElement(XD.Message12Dictionary.FaultDetail, XD.Message12Dictionary.Namespace);
            }
            else if (version == EnvelopeVersion.Soap11)
            {
                writer.WriteStartElement(XD.Message11Dictionary.FaultDetail, XD.Message11Dictionary.FaultNamespace);
            }
            else
            {
                writer.WriteStartElement(XD.Message12Dictionary.FaultDetail, XD.MessageDictionary.Namespace);
            }
        }

        public static bool WasHeaderNotUnderstood(MessageHeaders headers, string name, string ns)
        {
            if (headers == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("headers");
            }
            for (int i = 0; i < headers.Count; i++)
            {
                MessageHeaderInfo info = headers[i];
                if ((string.Compare(info.Name, "NotUnderstood", StringComparison.Ordinal) == 0) && (string.Compare(info.Namespace, "http://www.w3.org/2003/05/soap-envelope", StringComparison.Ordinal) == 0))
                {
                    using (XmlDictionaryReader reader = headers.GetReaderAtHeader(i))
                    {
                        string str;
                        string str2;
                        reader.MoveToAttribute("qname", "http://www.w3.org/2003/05/soap-envelope");
                        reader.ReadContentAsQualifiedName(out str, out str2);
                        if (((str != null) && (str2 != null)) && ((string.Compare(name, str, StringComparison.Ordinal) == 0) && (string.Compare(ns, str2, StringComparison.Ordinal) == 0)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void WriteFaultCode12Driver(XmlDictionaryWriter writer, FaultCode faultCode, EnvelopeVersion version)
        {
            string senderFaultName;
            string str2;
            writer.WriteStartElement(XD.Message12Dictionary.FaultValue, version.DictionaryNamespace);
            if (faultCode.IsSenderFault)
            {
                senderFaultName = version.SenderFaultName;
            }
            else if (faultCode.IsReceiverFault)
            {
                senderFaultName = version.ReceiverFaultName;
            }
            else
            {
                senderFaultName = faultCode.Name;
            }
            if (faultCode.IsPredefinedFault)
            {
                str2 = version.Namespace;
            }
            else
            {
                str2 = faultCode.Namespace;
            }
            if (writer.LookupPrefix(str2) == null)
            {
                writer.WriteAttributeString("xmlns", "a", "http://www.w3.org/2000/xmlns/", str2);
            }
            writer.WriteQualifiedName(senderFaultName, str2);
            writer.WriteEndElement();
            if (faultCode.SubCode != null)
            {
                writer.WriteStartElement(XD.Message12Dictionary.FaultSubcode, version.DictionaryNamespace);
                this.WriteFaultCode12Driver(writer, faultCode.SubCode, version);
                writer.WriteEndElement();
            }
        }

        public void WriteTo(XmlDictionaryWriter writer, EnvelopeVersion version)
        {
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (version == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            }
            if (version == EnvelopeVersion.Soap12)
            {
                this.WriteTo12(writer);
            }
            else if (version == EnvelopeVersion.Soap11)
            {
                this.WriteTo11(writer);
            }
            else
            {
                if (version != EnvelopeVersion.None)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("EnvelopeVersionUnknown", new object[] { version.ToString() })));
                }
                this.WriteToNone(writer);
            }
        }

        public void WriteTo(XmlWriter writer, EnvelopeVersion version)
        {
            this.WriteTo(XmlDictionaryWriter.CreateDictionaryWriter(writer), version);
        }

        private void WriteTo11(XmlDictionaryWriter writer)
        {
            string name;
            string str2;
            writer.WriteStartElement(XD.MessageDictionary.Fault, XD.Message11Dictionary.Namespace);
            writer.WriteStartElement(XD.Message11Dictionary.FaultCode, XD.Message11Dictionary.FaultNamespace);
            FaultCode subCode = this.Code;
            if (subCode.SubCode != null)
            {
                subCode = subCode.SubCode;
            }
            if (subCode.IsSenderFault)
            {
                name = "Client";
            }
            else if (subCode.IsReceiverFault)
            {
                name = "Server";
            }
            else
            {
                name = subCode.Name;
            }
            if (subCode.IsPredefinedFault)
            {
                str2 = "http://schemas.xmlsoap.org/soap/envelope/";
            }
            else
            {
                str2 = subCode.Namespace;
            }
            if (writer.LookupPrefix(str2) == null)
            {
                writer.WriteAttributeString("xmlns", "a", "http://www.w3.org/2000/xmlns/", str2);
            }
            writer.WriteQualifiedName(name, str2);
            writer.WriteEndElement();
            FaultReasonText text = this.Reason.Translations[0];
            writer.WriteStartElement(XD.Message11Dictionary.FaultString, XD.Message11Dictionary.FaultNamespace);
            if (text.XmlLang.Length > 0)
            {
                writer.WriteAttributeString("xml", "lang", "http://www.w3.org/XML/1998/namespace", text.XmlLang);
            }
            writer.WriteString(text.Text);
            writer.WriteEndElement();
            if (this.Actor.Length > 0)
            {
                writer.WriteElementString(XD.Message11Dictionary.FaultActor, XD.Message11Dictionary.FaultNamespace, this.Actor);
            }
            if (this.HasDetail)
            {
                this.OnWriteDetail(writer, EnvelopeVersion.Soap11);
            }
            writer.WriteEndElement();
        }

        private void WriteTo12(XmlDictionaryWriter writer)
        {
            this.WriteTo12Driver(writer, EnvelopeVersion.Soap12);
        }

        private void WriteTo12Driver(XmlDictionaryWriter writer, EnvelopeVersion version)
        {
            writer.WriteStartElement(XD.MessageDictionary.Fault, version.DictionaryNamespace);
            writer.WriteStartElement(XD.Message12Dictionary.FaultCode, version.DictionaryNamespace);
            this.WriteFaultCode12Driver(writer, this.Code, version);
            writer.WriteEndElement();
            writer.WriteStartElement(XD.Message12Dictionary.FaultReason, version.DictionaryNamespace);
            FaultReason reason = this.Reason;
            for (int i = 0; i < reason.Translations.Count; i++)
            {
                FaultReasonText text = reason.Translations[i];
                writer.WriteStartElement(XD.Message12Dictionary.FaultText, version.DictionaryNamespace);
                writer.WriteAttributeString("xml", "lang", "http://www.w3.org/XML/1998/namespace", text.XmlLang);
                writer.WriteString(text.Text);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            if (this.Node.Length > 0)
            {
                writer.WriteElementString(XD.Message12Dictionary.FaultNode, version.DictionaryNamespace, this.Node);
            }
            if (this.Actor.Length > 0)
            {
                writer.WriteElementString(XD.Message12Dictionary.FaultRole, version.DictionaryNamespace, this.Actor);
            }
            if (this.HasDetail)
            {
                this.OnWriteDetail(writer, version);
            }
            writer.WriteEndElement();
        }

        private void WriteToNone(XmlDictionaryWriter writer)
        {
            this.WriteTo12Driver(writer, EnvelopeVersion.None);
        }

        public virtual string Actor
        {
            get
            {
                return "";
            }
        }

        public abstract FaultCode Code { get; }

        public abstract bool HasDetail { get; }

        public bool IsMustUnderstandFault
        {
            get
            {
                FaultCode code = this.Code;
                if (string.Compare(code.Name, "MustUnderstand", StringComparison.Ordinal) != 0)
                {
                    return false;
                }
                if ((string.Compare(code.Namespace, EnvelopeVersion.Soap11.Namespace, StringComparison.Ordinal) != 0) && (string.Compare(code.Namespace, EnvelopeVersion.Soap12.Namespace, StringComparison.Ordinal) != 0))
                {
                    return false;
                }
                return true;
            }
        }

        public virtual string Node
        {
            get
            {
                return "";
            }
        }

        public abstract FaultReason Reason { get; }
    }
}

