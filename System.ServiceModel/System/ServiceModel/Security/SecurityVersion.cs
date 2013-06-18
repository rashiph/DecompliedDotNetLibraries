namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Xml;

    public abstract class SecurityVersion
    {
        private readonly XmlDictionaryString headerName;
        private readonly XmlDictionaryString headerNamespace;
        private readonly XmlDictionaryString headerPrefix;

        internal SecurityVersion(XmlDictionaryString headerName, XmlDictionaryString headerNamespace, XmlDictionaryString headerPrefix)
        {
            this.headerName = headerName;
            this.headerNamespace = headerNamespace;
            this.headerPrefix = headerPrefix;
        }

        internal abstract ReceiveSecurityHeader CreateReceiveSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay, SecurityStandardsManager standardsManager, SecurityAlgorithmSuite algorithmSuite, MessageDirection direction, int headerIndex);
        internal abstract SendSecurityHeader CreateSendSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay, SecurityStandardsManager standardsManager, SecurityAlgorithmSuite algorithmSuite, MessageDirection direction);
        internal bool DoesMessageContainSecurityHeader(Message message)
        {
            return (message.Headers.FindHeader(this.HeaderName.Value, this.HeaderNamespace.Value) >= 0);
        }

        internal int FindIndexOfSecurityHeader(Message message, string[] actors)
        {
            return message.Headers.FindHeader(this.HeaderName.Value, this.HeaderNamespace.Value, actors);
        }

        internal virtual bool IsReaderAtSignatureConfirmation(XmlDictionaryReader reader)
        {
            return false;
        }

        internal virtual ISignatureValueSecurityElement ReadSignatureConfirmation(XmlDictionaryReader reader)
        {
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SignatureConfirmationNotSupported")));
        }

        internal ReceiveSecurityHeader TryCreateReceiveSecurityHeader(Message message, string actor, SecurityStandardsManager standardsManager, SecurityAlgorithmSuite algorithmSuite, MessageDirection direction)
        {
            int headerIndex = message.Headers.FindHeader(this.HeaderName.Value, this.HeaderNamespace.Value, new string[] { actor });
            if ((headerIndex < 0) && string.IsNullOrEmpty(actor))
            {
                headerIndex = message.Headers.FindHeader(this.HeaderName.Value, this.HeaderNamespace.Value, message.Version.Envelope.UltimateDestinationActorValues);
            }
            if (headerIndex < 0)
            {
                return null;
            }
            MessageHeaderInfo info = message.Headers[headerIndex];
            return this.CreateReceiveSecurityHeader(message, info.Actor, info.MustUnderstand, info.Relay, standardsManager, algorithmSuite, direction, headerIndex);
        }

        internal virtual void WriteSignatureConfirmation(XmlDictionaryWriter writer, string id, byte[] signatureConfirmation)
        {
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SignatureConfirmationNotSupported")));
        }

        internal void WriteStartHeader(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(this.HeaderPrefix.Value, this.HeaderName, this.HeaderNamespace);
        }

        internal static SecurityVersion Default
        {
            get
            {
                return WSSecurity11;
            }
        }

        internal abstract XmlDictionaryString FailedAuthenticationFaultCode { get; }

        internal XmlDictionaryString HeaderName
        {
            get
            {
                return this.headerName;
            }
        }

        internal XmlDictionaryString HeaderNamespace
        {
            get
            {
                return this.headerNamespace;
            }
        }

        internal XmlDictionaryString HeaderPrefix
        {
            get
            {
                return this.headerPrefix;
            }
        }

        internal abstract XmlDictionaryString InvalidSecurityFaultCode { get; }

        internal abstract XmlDictionaryString InvalidSecurityTokenFaultCode { get; }

        internal virtual bool SupportsSignatureConfirmation
        {
            get
            {
                return false;
            }
        }

        public static SecurityVersion WSSecurity10
        {
            get
            {
                return SecurityVersion10.Instance;
            }
        }

        public static SecurityVersion WSSecurity11
        {
            get
            {
                return SecurityVersion11.Instance;
            }
        }

        private class SecurityVersion10 : SecurityVersion
        {
            private static readonly SecurityVersion.SecurityVersion10 instance = new SecurityVersion.SecurityVersion10();

            protected SecurityVersion10() : base(System.ServiceModel.XD.SecurityJan2004Dictionary.Security, System.ServiceModel.XD.SecurityJan2004Dictionary.Namespace, System.ServiceModel.XD.SecurityJan2004Dictionary.Prefix)
            {
            }

            internal override ReceiveSecurityHeader CreateReceiveSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay, SecurityStandardsManager standardsManager, SecurityAlgorithmSuite algorithmSuite, MessageDirection direction, int headerIndex)
            {
                return new WSSecurityOneDotZeroReceiveSecurityHeader(message, actor, mustUnderstand, relay, standardsManager, algorithmSuite, headerIndex, direction);
            }

            internal override SendSecurityHeader CreateSendSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay, SecurityStandardsManager standardsManager, SecurityAlgorithmSuite algorithmSuite, MessageDirection direction)
            {
                return new WSSecurityOneDotZeroSendSecurityHeader(message, actor, mustUnderstand, relay, standardsManager, algorithmSuite, direction);
            }

            public override string ToString()
            {
                return "WSSecurity10";
            }

            internal override XmlDictionaryString FailedAuthenticationFaultCode
            {
                get
                {
                    return System.ServiceModel.XD.SecurityJan2004Dictionary.FailedAuthenticationFaultCode;
                }
            }

            public static SecurityVersion.SecurityVersion10 Instance
            {
                get
                {
                    return instance;
                }
            }

            internal override XmlDictionaryString InvalidSecurityFaultCode
            {
                get
                {
                    return System.ServiceModel.XD.SecurityJan2004Dictionary.InvalidSecurityFaultCode;
                }
            }

            internal override XmlDictionaryString InvalidSecurityTokenFaultCode
            {
                get
                {
                    return System.ServiceModel.XD.SecurityJan2004Dictionary.InvalidSecurityTokenFaultCode;
                }
            }
        }

        private sealed class SecurityVersion11 : SecurityVersion.SecurityVersion10
        {
            private static readonly SecurityVersion.SecurityVersion11 instance = new SecurityVersion.SecurityVersion11();

            private SecurityVersion11()
            {
            }

            internal override ReceiveSecurityHeader CreateReceiveSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay, SecurityStandardsManager standardsManager, SecurityAlgorithmSuite algorithmSuite, MessageDirection direction, int headerIndex)
            {
                return new WSSecurityOneDotOneReceiveSecurityHeader(message, actor, mustUnderstand, relay, standardsManager, algorithmSuite, headerIndex, direction);
            }

            internal override SendSecurityHeader CreateSendSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay, SecurityStandardsManager standardsManager, SecurityAlgorithmSuite algorithmSuite, MessageDirection direction)
            {
                return new WSSecurityOneDotOneSendSecurityHeader(message, actor, mustUnderstand, relay, standardsManager, algorithmSuite, direction);
            }

            internal override bool IsReaderAtSignatureConfirmation(XmlDictionaryReader reader)
            {
                return reader.IsStartElement(System.ServiceModel.XD.SecurityXXX2005Dictionary.SignatureConfirmation, System.ServiceModel.XD.SecurityXXX2005Dictionary.Namespace);
            }

            internal override ISignatureValueSecurityElement ReadSignatureConfirmation(XmlDictionaryReader reader)
            {
                reader.MoveToStartElement(System.ServiceModel.XD.SecurityXXX2005Dictionary.SignatureConfirmation, System.ServiceModel.XD.SecurityXXX2005Dictionary.Namespace);
                bool isEmptyElement = reader.IsEmptyElement;
                string id = System.ServiceModel.Security.XmlHelper.GetRequiredNonEmptyAttribute(reader, System.ServiceModel.XD.UtilityDictionary.IdAttribute, System.ServiceModel.XD.UtilityDictionary.Namespace);
                byte[] signatureValue = System.ServiceModel.Security.XmlHelper.GetRequiredBase64Attribute(reader, System.ServiceModel.XD.SecurityXXX2005Dictionary.ValueAttribute, null);
                reader.ReadStartElement();
                if (!isEmptyElement)
                {
                    reader.ReadEndElement();
                }
                return new SignatureConfirmationElement(id, signatureValue, this);
            }

            public override string ToString()
            {
                return "WSSecurity11";
            }

            internal override void WriteSignatureConfirmation(XmlDictionaryWriter writer, string id, byte[] signature)
            {
                if (id == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("id");
                }
                if (signature == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signature");
                }
                writer.WriteStartElement(System.ServiceModel.XD.SecurityXXX2005Dictionary.Prefix.Value, System.ServiceModel.XD.SecurityXXX2005Dictionary.SignatureConfirmation, System.ServiceModel.XD.SecurityXXX2005Dictionary.Namespace);
                writer.WriteAttributeString(System.ServiceModel.XD.UtilityDictionary.Prefix.Value, System.ServiceModel.XD.UtilityDictionary.IdAttribute, System.ServiceModel.XD.UtilityDictionary.Namespace, id);
                writer.WriteStartAttribute(System.ServiceModel.XD.SecurityXXX2005Dictionary.ValueAttribute, null);
                writer.WriteBase64(signature, 0, signature.Length);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }

            public static SecurityVersion.SecurityVersion11 Instance
            {
                get
                {
                    return instance;
                }
            }

            internal override bool SupportsSignatureConfirmation
            {
                get
                {
                    return true;
                }
            }
        }
    }
}

