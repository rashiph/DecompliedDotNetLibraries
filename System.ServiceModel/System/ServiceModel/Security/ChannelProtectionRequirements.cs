namespace System.ServiceModel.Security
{
    using System;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Xml;

    public class ChannelProtectionRequirements
    {
        private ScopedMessagePartSpecification incomingEncryptionParts;
        private ScopedMessagePartSpecification incomingSignatureParts;
        private bool isReadOnly;
        private ScopedMessagePartSpecification outgoingEncryptionParts;
        private ScopedMessagePartSpecification outgoingSignatureParts;

        public ChannelProtectionRequirements()
        {
            this.incomingSignatureParts = new ScopedMessagePartSpecification();
            this.incomingEncryptionParts = new ScopedMessagePartSpecification();
            this.outgoingSignatureParts = new ScopedMessagePartSpecification();
            this.outgoingEncryptionParts = new ScopedMessagePartSpecification();
        }

        public ChannelProtectionRequirements(ChannelProtectionRequirements other)
        {
            if (other == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("other"));
            }
            this.incomingSignatureParts = new ScopedMessagePartSpecification(other.incomingSignatureParts);
            this.incomingEncryptionParts = new ScopedMessagePartSpecification(other.incomingEncryptionParts);
            this.outgoingSignatureParts = new ScopedMessagePartSpecification(other.outgoingSignatureParts);
            this.outgoingEncryptionParts = new ScopedMessagePartSpecification(other.outgoingEncryptionParts);
        }

        internal ChannelProtectionRequirements(ChannelProtectionRequirements other, ProtectionLevel newBodyProtectionLevel)
        {
            if (other == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("other"));
            }
            this.incomingSignatureParts = new ScopedMessagePartSpecification(other.incomingSignatureParts, newBodyProtectionLevel != ProtectionLevel.None);
            this.incomingEncryptionParts = new ScopedMessagePartSpecification(other.incomingEncryptionParts, newBodyProtectionLevel == ProtectionLevel.EncryptAndSign);
            this.outgoingSignatureParts = new ScopedMessagePartSpecification(other.outgoingSignatureParts, newBodyProtectionLevel != ProtectionLevel.None);
            this.outgoingEncryptionParts = new ScopedMessagePartSpecification(other.outgoingEncryptionParts, newBodyProtectionLevel == ProtectionLevel.EncryptAndSign);
        }

        public void Add(ChannelProtectionRequirements protectionRequirements)
        {
            this.Add(protectionRequirements, false);
        }

        public void Add(ChannelProtectionRequirements protectionRequirements, bool channelScopeOnly)
        {
            if (protectionRequirements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("protectionRequirements"));
            }
            if (protectionRequirements.incomingSignatureParts != null)
            {
                this.incomingSignatureParts.AddParts(protectionRequirements.incomingSignatureParts.ChannelParts);
            }
            if (protectionRequirements.incomingEncryptionParts != null)
            {
                this.incomingEncryptionParts.AddParts(protectionRequirements.incomingEncryptionParts.ChannelParts);
            }
            if (protectionRequirements.outgoingSignatureParts != null)
            {
                this.outgoingSignatureParts.AddParts(protectionRequirements.outgoingSignatureParts.ChannelParts);
            }
            if (protectionRequirements.outgoingEncryptionParts != null)
            {
                this.outgoingEncryptionParts.AddParts(protectionRequirements.outgoingEncryptionParts.ChannelParts);
            }
            if (!channelScopeOnly)
            {
                AddActionParts(this.incomingSignatureParts, protectionRequirements.incomingSignatureParts);
                AddActionParts(this.incomingEncryptionParts, protectionRequirements.incomingEncryptionParts);
                AddActionParts(this.outgoingSignatureParts, protectionRequirements.outgoingSignatureParts);
                AddActionParts(this.outgoingEncryptionParts, protectionRequirements.outgoingEncryptionParts);
            }
        }

        private static void AddActionParts(ScopedMessagePartSpecification to, ScopedMessagePartSpecification from)
        {
            foreach (string str in from.Actions)
            {
                MessagePartSpecification specification;
                if (from.TryGetParts(str, true, out specification))
                {
                    to.AddParts(specification, str);
                }
            }
        }

        private static void AddFaultProtectionRequirements(FaultDescriptionCollection faults, ChannelProtectionRequirements requirements, ProtectionLevel defaultProtectionLevel, bool addToIncoming)
        {
            if (faults == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("faults"));
            }
            if (requirements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("requirements"));
            }
            foreach (FaultDescription description in faults)
            {
                MessagePartSpecification parts = new MessagePartSpecification();
                MessagePartSpecification specification2 = new MessagePartSpecification();
                ProtectionLevel level = description.HasProtectionLevel ? description.ProtectionLevel : defaultProtectionLevel;
                if (level != ProtectionLevel.None)
                {
                    parts.IsBodyIncluded = true;
                    if (level == ProtectionLevel.EncryptAndSign)
                    {
                        specification2.IsBodyIncluded = true;
                    }
                }
                if (addToIncoming)
                {
                    requirements.IncomingSignatureParts.AddParts(parts, description.Action);
                    requirements.IncomingEncryptionParts.AddParts(specification2, description.Action);
                }
                else
                {
                    requirements.OutgoingSignatureParts.AddParts(parts, description.Action);
                    requirements.OutgoingEncryptionParts.AddParts(specification2, description.Action);
                }
            }
        }

        private static void AddHeaderProtectionRequirements(MessageHeaderDescription header, MessagePartSpecification signedParts, MessagePartSpecification encryptedParts, ProtectionLevel defaultProtectionLevel)
        {
            ProtectionLevel level = header.HasProtectionLevel ? header.ProtectionLevel : defaultProtectionLevel;
            if (level != ProtectionLevel.None)
            {
                XmlQualifiedName item = new XmlQualifiedName(header.Name, header.Namespace);
                signedParts.HeaderTypes.Add(item);
                if (level == ProtectionLevel.EncryptAndSign)
                {
                    encryptedParts.HeaderTypes.Add(item);
                }
            }
        }

        internal static ChannelProtectionRequirements CreateFromContract(ContractDescription contract, ISecurityCapabilities bindingElement, bool isForClient)
        {
            return CreateFromContract(contract, bindingElement.SupportedRequestProtectionLevel, bindingElement.SupportedResponseProtectionLevel, isForClient);
        }

        internal static ChannelProtectionRequirements CreateFromContract(ContractDescription contract, ProtectionLevel defaultRequestProtectionLevel, ProtectionLevel defaultResponseProtectionLevel, bool isForClient)
        {
            ProtectionLevel protectionLevel;
            ProtectionLevel level2;
            if (contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contract"));
            }
            ChannelProtectionRequirements requirements = new ChannelProtectionRequirements();
            if (contract.HasProtectionLevel)
            {
                protectionLevel = contract.ProtectionLevel;
                level2 = contract.ProtectionLevel;
            }
            else
            {
                protectionLevel = defaultRequestProtectionLevel;
                level2 = defaultResponseProtectionLevel;
            }
            foreach (OperationDescription description in contract.Operations)
            {
                ProtectionLevel level3;
                ProtectionLevel level4;
                if (description.HasProtectionLevel)
                {
                    level3 = description.ProtectionLevel;
                    level4 = description.ProtectionLevel;
                }
                else
                {
                    level3 = protectionLevel;
                    level4 = level2;
                }
                foreach (MessageDescription description2 in description.Messages)
                {
                    ProtectionLevel level5;
                    ProtectionLevel none;
                    if (description2.HasProtectionLevel)
                    {
                        level5 = description2.ProtectionLevel;
                    }
                    else if (description2.Direction == MessageDirection.Input)
                    {
                        level5 = level3;
                    }
                    else
                    {
                        level5 = level4;
                    }
                    MessagePartSpecification signedParts = new MessagePartSpecification();
                    MessagePartSpecification encryptedParts = new MessagePartSpecification();
                    foreach (MessageHeaderDescription description3 in description2.Headers)
                    {
                        AddHeaderProtectionRequirements(description3, signedParts, encryptedParts, level5);
                    }
                    if (description2.Body.Parts.Count > 0)
                    {
                        none = ProtectionLevel.None;
                    }
                    else if (description2.Body.ReturnValue != null)
                    {
                        if (!description2.Body.ReturnValue.GetType().Equals(typeof(MessagePartDescription)))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("OnlyBodyReturnValuesSupported")));
                        }
                        MessagePartDescription returnValue = description2.Body.ReturnValue;
                        none = returnValue.HasProtectionLevel ? returnValue.ProtectionLevel : level5;
                    }
                    else
                    {
                        none = level5;
                    }
                    if (description2.Body.Parts.Count > 0)
                    {
                        foreach (MessagePartDescription description5 in description2.Body.Parts)
                        {
                            ProtectionLevel level7 = description5.HasProtectionLevel ? description5.ProtectionLevel : level5;
                            none = ProtectionLevelHelper.Max(none, level7);
                            if (none == ProtectionLevel.EncryptAndSign)
                            {
                                break;
                            }
                        }
                    }
                    if (none != ProtectionLevel.None)
                    {
                        signedParts.IsBodyIncluded = true;
                        if (none == ProtectionLevel.EncryptAndSign)
                        {
                            encryptedParts.IsBodyIncluded = true;
                        }
                    }
                    if (description2.Direction == MessageDirection.Input)
                    {
                        requirements.IncomingSignatureParts.AddParts(signedParts, description2.Action);
                        requirements.IncomingEncryptionParts.AddParts(encryptedParts, description2.Action);
                    }
                    else
                    {
                        requirements.OutgoingSignatureParts.AddParts(signedParts, description2.Action);
                        requirements.OutgoingEncryptionParts.AddParts(encryptedParts, description2.Action);
                    }
                }
                if (description.Faults != null)
                {
                    if (description.IsServerInitiated())
                    {
                        AddFaultProtectionRequirements(description.Faults, requirements, level3, true);
                    }
                    else
                    {
                        AddFaultProtectionRequirements(description.Faults, requirements, level4, false);
                    }
                }
            }
            return requirements;
        }

        internal static ChannelProtectionRequirements CreateFromContractAndUnionResponseProtectionRequirements(ContractDescription contract, ISecurityCapabilities bindingElement, bool isForClient)
        {
            ChannelProtectionRequirements requirements = CreateFromContract(contract, bindingElement.SupportedRequestProtectionLevel, bindingElement.SupportedResponseProtectionLevel, isForClient);
            ChannelProtectionRequirements requirements2 = new ChannelProtectionRequirements();
            requirements2.OutgoingEncryptionParts.AddParts(UnionMessagePartSpecifications(requirements.OutgoingEncryptionParts), "*");
            requirements2.OutgoingSignatureParts.AddParts(UnionMessagePartSpecifications(requirements.OutgoingSignatureParts), "*");
            requirements.IncomingEncryptionParts.CopyTo(requirements2.IncomingEncryptionParts);
            requirements.IncomingSignatureParts.CopyTo(requirements2.IncomingSignatureParts);
            return requirements2;
        }

        public ChannelProtectionRequirements CreateInverse()
        {
            ChannelProtectionRequirements requirements = new ChannelProtectionRequirements();
            requirements.Add(this, true);
            requirements.incomingSignatureParts = new ScopedMessagePartSpecification(this.OutgoingSignatureParts);
            requirements.outgoingSignatureParts = new ScopedMessagePartSpecification(this.IncomingSignatureParts);
            requirements.incomingEncryptionParts = new ScopedMessagePartSpecification(this.OutgoingEncryptionParts);
            requirements.outgoingEncryptionParts = new ScopedMessagePartSpecification(this.IncomingEncryptionParts);
            return requirements;
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.incomingSignatureParts.MakeReadOnly();
                this.incomingEncryptionParts.MakeReadOnly();
                this.outgoingSignatureParts.MakeReadOnly();
                this.outgoingEncryptionParts.MakeReadOnly();
                this.isReadOnly = true;
            }
        }

        private static MessagePartSpecification UnionMessagePartSpecifications(ScopedMessagePartSpecification actionParts)
        {
            MessagePartSpecification specification = new MessagePartSpecification(false);
            foreach (string str in actionParts.Actions)
            {
                MessagePartSpecification specification2;
                if (actionParts.TryGetParts(str, out specification2))
                {
                    if (specification2.IsBodyIncluded)
                    {
                        specification.IsBodyIncluded = true;
                    }
                    foreach (XmlQualifiedName name in specification2.HeaderTypes)
                    {
                        if (!specification.IsHeaderIncluded(name.Name, name.Namespace))
                        {
                            specification.HeaderTypes.Add(name);
                        }
                    }
                }
            }
            return specification;
        }

        public ScopedMessagePartSpecification IncomingEncryptionParts
        {
            get
            {
                return this.incomingEncryptionParts;
            }
        }

        public ScopedMessagePartSpecification IncomingSignatureParts
        {
            get
            {
                return this.incomingSignatureParts;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public ScopedMessagePartSpecification OutgoingEncryptionParts
        {
            get
            {
                return this.outgoingEncryptionParts;
            }
        }

        public ScopedMessagePartSpecification OutgoingSignatureParts
        {
            get
            {
                return this.outgoingSignatureParts;
            }
        }
    }
}

