namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Xml;

    public abstract class MessageHeader : MessageHeaderInfo
    {
        private const string DefaultActorValue = "";
        private const bool DefaultMustUnderstandValue = false;
        private const bool DefaultRelayValue = false;

        protected MessageHeader()
        {
        }

        public static MessageHeader CreateHeader(string name, string ns, object value)
        {
            return CreateHeader(name, ns, value, false, "", false);
        }

        public static MessageHeader CreateHeader(string name, string ns, object value, bool mustUnderstand)
        {
            return CreateHeader(name, ns, value, mustUnderstand, "", false);
        }

        public static MessageHeader CreateHeader(string name, string ns, object value, XmlObjectSerializer serializer)
        {
            return CreateHeader(name, ns, value, serializer, false, "", false);
        }

        public static MessageHeader CreateHeader(string name, string ns, object value, bool mustUnderstand, string actor)
        {
            return CreateHeader(name, ns, value, mustUnderstand, actor, false);
        }

        public static MessageHeader CreateHeader(string name, string ns, object value, XmlObjectSerializer serializer, bool mustUnderstand)
        {
            return CreateHeader(name, ns, value, serializer, mustUnderstand, "", false);
        }

        public static MessageHeader CreateHeader(string name, string ns, object value, bool mustUnderstand, string actor, bool relay)
        {
            return new XmlObjectSerializerHeader(name, ns, value, null, mustUnderstand, actor, relay);
        }

        public static MessageHeader CreateHeader(string name, string ns, object value, XmlObjectSerializer serializer, bool mustUnderstand, string actor)
        {
            return CreateHeader(name, ns, value, serializer, mustUnderstand, actor, false);
        }

        public static MessageHeader CreateHeader(string name, string ns, object value, XmlObjectSerializer serializer, bool mustUnderstand, string actor, bool relay)
        {
            if (serializer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serializer"));
            }
            return new XmlObjectSerializerHeader(name, ns, value, serializer, mustUnderstand, actor, relay);
        }

        internal static void GetHeaderAttributes(XmlDictionaryReader reader, MessageVersion version, out string actor, out bool mustUnderstand, out bool relay, out bool isReferenceParameter)
        {
            int attributeCount = reader.AttributeCount;
            if (attributeCount == 0)
            {
                mustUnderstand = false;
                actor = string.Empty;
                relay = false;
                isReferenceParameter = false;
            }
            else
            {
                string attribute = reader.GetAttribute(XD.MessageDictionary.MustUnderstand, version.Envelope.DictionaryNamespace);
                if ((attribute != null) && ToBoolean(attribute))
                {
                    mustUnderstand = true;
                }
                else
                {
                    mustUnderstand = false;
                }
                if (mustUnderstand && (attributeCount == 1))
                {
                    actor = string.Empty;
                    relay = false;
                }
                else
                {
                    actor = reader.GetAttribute(version.Envelope.DictionaryActor, version.Envelope.DictionaryNamespace);
                    if (actor == null)
                    {
                        actor = "";
                    }
                    if (version.Envelope == EnvelopeVersion.Soap12)
                    {
                        string str2 = reader.GetAttribute(XD.Message12Dictionary.Relay, version.Envelope.DictionaryNamespace);
                        if ((str2 != null) && ToBoolean(str2))
                        {
                            relay = true;
                        }
                        else
                        {
                            relay = false;
                        }
                    }
                    else
                    {
                        relay = false;
                    }
                }
                isReferenceParameter = false;
                if (version.Addressing == AddressingVersion.WSAddressing10)
                {
                    string str3 = reader.GetAttribute(XD.AddressingDictionary.IsReferenceParameter, version.Addressing.DictionaryNamespace);
                    if (str3 != null)
                    {
                        isReferenceParameter = ToBoolean(str3);
                    }
                }
            }
        }

        public virtual bool IsMessageVersionSupported(MessageVersion messageVersion)
        {
            if (messageVersion == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageVersion");
            }
            return true;
        }

        protected abstract void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion);
        protected virtual void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteStartElement(this.Name, this.Namespace);
            this.WriteHeaderAttributes(writer, messageVersion);
        }

        private static bool ToBoolean(string value)
        {
            bool flag;
            if (value.Length == 1)
            {
                switch (value[0])
                {
                    case '1':
                        return true;

                    case '0':
                        return false;
                }
            }
            else
            {
                if (value == "true")
                {
                    return true;
                }
                if (value == "false")
                {
                    return false;
                }
            }
            try
            {
                flag = XmlConvert.ToBoolean(value);
            }
            catch (FormatException exception)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(exception.Message, null));
            }
            return flag;
        }

        public override string ToString()
        {
            StringWriter w = new StringWriter(CultureInfo.InvariantCulture);
            XmlTextWriter writer = new XmlTextWriter(w) {
                Formatting = Formatting.Indented
            };
            XmlDictionaryWriter writer3 = XmlDictionaryWriter.CreateDictionaryWriter(writer);
            if (this.IsMessageVersionSupported(MessageVersion.Soap12WSAddressing10))
            {
                this.WriteHeader(writer3, MessageVersion.Soap12WSAddressing10);
            }
            else if (this.IsMessageVersionSupported(MessageVersion.Soap12WSAddressingAugust2004))
            {
                this.WriteHeader(writer3, MessageVersion.Soap12WSAddressingAugust2004);
            }
            else if (this.IsMessageVersionSupported(MessageVersion.Soap11WSAddressing10))
            {
                this.WriteHeader(writer3, MessageVersion.Soap11WSAddressing10);
            }
            else if (this.IsMessageVersionSupported(MessageVersion.Soap11WSAddressingAugust2004))
            {
                this.WriteHeader(writer3, MessageVersion.Soap11WSAddressingAugust2004);
            }
            else if (this.IsMessageVersionSupported(MessageVersion.Soap12))
            {
                this.WriteHeader(writer3, MessageVersion.Soap12);
            }
            else if (this.IsMessageVersionSupported(MessageVersion.Soap11))
            {
                this.WriteHeader(writer3, MessageVersion.Soap11);
            }
            else
            {
                this.WriteHeader(writer3, MessageVersion.None);
            }
            writer3.Flush();
            return w.ToString();
        }

        public void WriteHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            if (messageVersion == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageVersion"));
            }
            this.OnWriteStartHeader(writer, messageVersion);
            this.OnWriteHeaderContents(writer, messageVersion);
            writer.WriteEndElement();
        }

        public void WriteHeader(XmlWriter writer, MessageVersion messageVersion)
        {
            this.WriteHeader(XmlDictionaryWriter.CreateDictionaryWriter(writer), messageVersion);
        }

        protected void WriteHeaderAttributes(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            string actor = this.Actor;
            if (actor.Length > 0)
            {
                writer.WriteAttributeString(messageVersion.Envelope.DictionaryActor, messageVersion.Envelope.DictionaryNamespace, actor);
            }
            if (this.MustUnderstand)
            {
                writer.WriteAttributeString(XD.MessageDictionary.MustUnderstand, messageVersion.Envelope.DictionaryNamespace, "1");
            }
            if (this.Relay && (messageVersion.Envelope == EnvelopeVersion.Soap12))
            {
                writer.WriteAttributeString(XD.Message12Dictionary.Relay, XD.Message12Dictionary.Namespace, "1");
            }
        }

        public void WriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            if (messageVersion == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageVersion"));
            }
            this.OnWriteHeaderContents(writer, messageVersion);
        }

        public void WriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            if (messageVersion == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageVersion"));
            }
            this.OnWriteStartHeader(writer, messageVersion);
        }

        public override string Actor
        {
            get
            {
                return "";
            }
        }

        public override bool IsReferenceParameter
        {
            get
            {
                return false;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return false;
            }
        }

        public override bool Relay
        {
            get
            {
                return false;
            }
        }
    }
}

