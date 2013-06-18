namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Xml;

    public sealed class TransactionFlowBindingElement : BindingElement, IPolicyExportExtension
    {
        private TransactionFlowOption issuedTokens;
        private System.ServiceModel.TransactionProtocol transactionProtocol;
        private bool transactions;

        public TransactionFlowBindingElement() : this(true, TransactionFlowDefaults.TransactionProtocol)
        {
        }

        internal TransactionFlowBindingElement(bool transactions) : this(transactions, TransactionFlowDefaults.TransactionProtocol)
        {
        }

        private TransactionFlowBindingElement(TransactionFlowBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.transactions = elementToBeCloned.transactions;
            this.issuedTokens = elementToBeCloned.issuedTokens;
            if (!System.ServiceModel.TransactionProtocol.IsDefined(elementToBeCloned.transactionProtocol))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("ConfigInvalidTransactionFlowProtocolValue", new object[] { elementToBeCloned.transactionProtocol.ToString() }));
            }
            this.transactionProtocol = elementToBeCloned.transactionProtocol;
            this.AllowWildcardAction = elementToBeCloned.AllowWildcardAction;
        }

        public TransactionFlowBindingElement(System.ServiceModel.TransactionProtocol transactionProtocol) : this(true, transactionProtocol)
        {
        }

        internal TransactionFlowBindingElement(bool transactions, System.ServiceModel.TransactionProtocol transactionProtocol)
        {
            this.transactions = transactions;
            this.issuedTokens = transactions ? TransactionFlowOption.Allowed : TransactionFlowOption.NotAllowed;
            if (!System.ServiceModel.TransactionProtocol.IsDefined(transactionProtocol))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("ConfigInvalidTransactionFlowProtocolValue", new object[] { transactionProtocol.ToString() }));
            }
            this.transactionProtocol = transactionProtocol;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
            }
            Dictionary<DirectionalAction, TransactionFlowOption> dictionary = this.GetDictionary(context);
            if (!this.IsFlowEnabled(dictionary))
            {
                return context.BuildInnerChannelFactory<TChannel>();
            }
            if (this.issuedTokens == TransactionFlowOption.NotAllowed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TransactionFlowRequiredIssuedTokens")));
            }
            return new TransactionChannelFactory<TChannel>(this.transactionProtocol, context, dictionary, this.AllowWildcardAction) { FlowIssuedTokens = this.IssuedTokens };
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            if (!context.CanBuildInnerChannelListener<TChannel>())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
            }
            Dictionary<DirectionalAction, TransactionFlowOption> dictionary = this.GetDictionary(context);
            if (!this.IsFlowEnabled(dictionary))
            {
                return context.BuildInnerChannelListener<TChannel>();
            }
            if (this.issuedTokens == TransactionFlowOption.NotAllowed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TransactionFlowRequiredIssuedTokens")));
            }
            return new TransactionChannelListener<TChannel>(this.transactionProtocol, context.Binding, dictionary, context.BuildInnerChannelListener<TChannel>()) { FlowIssuedTokens = this.IssuedTokens };
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("context"));
            }
            if (((!(typeof(TChannel) == typeof(IOutputChannel)) && !(typeof(TChannel) == typeof(IDuplexChannel))) && (!(typeof(TChannel) == typeof(IRequestChannel)) && !(typeof(TChannel) == typeof(IOutputSessionChannel)))) && (!(typeof(TChannel) == typeof(IRequestSessionChannel)) && !(typeof(TChannel) == typeof(IDuplexSessionChannel))))
            {
                return false;
            }
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (!context.CanBuildInnerChannelListener<TChannel>())
            {
                return false;
            }
            if (((!(typeof(TChannel) == typeof(IInputChannel)) && !(typeof(TChannel) == typeof(IReplyChannel))) && (!(typeof(TChannel) == typeof(IDuplexChannel)) && !(typeof(TChannel) == typeof(IInputSessionChannel)))) && !(typeof(TChannel) == typeof(IReplySessionChannel)))
            {
                return (typeof(TChannel) == typeof(IDuplexSessionChannel));
            }
            return true;
        }

        public override BindingElement Clone()
        {
            return new TransactionFlowBindingElement(this);
        }

        private XmlElement GetAssertion(XmlDocument doc, TransactionFlowOption option, string prefix, string name, string ns, string policyNs)
        {
            if (doc == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("doc");
            }
            XmlElement element = null;
            switch (option)
            {
                case TransactionFlowOption.NotAllowed:
                    return element;

                case TransactionFlowOption.Allowed:
                {
                    element = doc.CreateElement(prefix, name, ns);
                    System.Xml.XmlAttribute node = doc.CreateAttribute("wsp", "Optional", policyNs);
                    node.Value = "true";
                    element.Attributes.Append(node);
                    if ((this.transactionProtocol == System.ServiceModel.TransactionProtocol.OleTransactions) || (this.transactionProtocol == System.ServiceModel.TransactionProtocol.WSAtomicTransactionOctober2004))
                    {
                        System.Xml.XmlAttribute attribute2 = doc.CreateAttribute("wsp1", "Optional", "http://schemas.xmlsoap.org/ws/2002/12/policy");
                        attribute2.Value = "true";
                        element.Attributes.Append(attribute2);
                    }
                    return element;
                }
                case TransactionFlowOption.Mandatory:
                    return doc.CreateElement(prefix, name, ns);
            }
            return element;
        }

        private Dictionary<DirectionalAction, TransactionFlowOption> GetDictionary(BindingContext context)
        {
            Dictionary<DirectionalAction, TransactionFlowOption> dictionary = context.BindingParameters.Find<Dictionary<DirectionalAction, TransactionFlowOption>>();
            if (dictionary == null)
            {
                dictionary = new Dictionary<DirectionalAction, TransactionFlowOption>();
            }
            return dictionary;
        }

        internal static MessagePartSpecification GetIssuedTokenHeaderSpecification(SecurityStandardsManager standardsManager)
        {
            if (!standardsManager.TrustDriver.IsIssuedTokensSupported)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TrustDriverVersionDoesNotSupportIssuedTokens")));
            }
            return new MessagePartSpecification(new XmlQualifiedName[] { new XmlQualifiedName(standardsManager.TrustDriver.IssuedTokensHeaderName, standardsManager.TrustDriver.IssuedTokensHeaderNamespace) });
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (!(typeof(T) == typeof(ChannelProtectionRequirements)))
            {
                return context.GetInnerProperty<T>();
            }
            ChannelProtectionRequirements protectionRequirements = this.GetProtectionRequirements();
            if (protectionRequirements == null)
            {
                return context.GetInnerProperty<ChannelProtectionRequirements>();
            }
            protectionRequirements.Add(context.GetInnerProperty<ChannelProtectionRequirements>() ?? new ChannelProtectionRequirements());
            return (T) protectionRequirements;
        }

        private ChannelProtectionRequirements GetProtectionRequirements()
        {
            if (!this.Transactions && (this.IssuedTokens == TransactionFlowOption.NotAllowed))
            {
                return null;
            }
            ChannelProtectionRequirements requirements = new ChannelProtectionRequirements();
            if (this.Transactions)
            {
                MessagePartSpecification specification = new MessagePartSpecification(new XmlQualifiedName[] { new XmlQualifiedName("CoordinationContext", "http://schemas.xmlsoap.org/ws/2004/10/wscoor"), new XmlQualifiedName("CoordinationContext", "http://docs.oasis-open.org/ws-tx/wscoor/2006/06"), new XmlQualifiedName("OleTxTransaction", "http://schemas.microsoft.com/ws/2006/02/tx/oletx") });
                specification.MakeReadOnly();
                requirements.IncomingSignatureParts.AddParts(specification);
                requirements.OutgoingSignatureParts.AddParts(specification);
                requirements.IncomingEncryptionParts.AddParts(specification);
                requirements.OutgoingEncryptionParts.AddParts(specification);
            }
            if (this.IssuedTokens != TransactionFlowOption.NotAllowed)
            {
                MessagePartSpecification issuedTokenHeaderSpecification = GetIssuedTokenHeaderSpecification(SecurityStandardsManager.DefaultInstance);
                issuedTokenHeaderSpecification.MakeReadOnly();
                requirements.IncomingSignatureParts.AddParts(issuedTokenHeaderSpecification);
                requirements.IncomingEncryptionParts.AddParts(issuedTokenHeaderSpecification);
                requirements.OutgoingSignatureParts.AddParts(issuedTokenHeaderSpecification);
                requirements.OutgoingEncryptionParts.AddParts(issuedTokenHeaderSpecification);
            }
            MessagePartSpecification parts = new MessagePartSpecification(true);
            parts.MakeReadOnly();
            requirements.OutgoingSignatureParts.AddParts(parts, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/transactions/fault");
            requirements.OutgoingEncryptionParts.AddParts(parts, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/transactions/fault");
            return requirements;
        }

        private bool IsFlowEnabled(Dictionary<DirectionalAction, TransactionFlowOption> dictionary)
        {
            if (this.issuedTokens != TransactionFlowOption.NotAllowed)
            {
                return true;
            }
            if (this.transactions)
            {
                using (Dictionary<DirectionalAction, TransactionFlowOption>.ValueCollection.Enumerator enumerator = dictionary.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (((TransactionFlowOption) enumerator.Current) != TransactionFlowOption.NotAllowed)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal bool IsFlowEnabled(ContractDescription contract)
        {
            if (this.issuedTokens != TransactionFlowOption.NotAllowed)
            {
                return true;
            }
            if (this.transactions)
            {
                foreach (OperationDescription description in contract.Operations)
                {
                    TransactionFlowAttribute attribute = description.Behaviors.Find<TransactionFlowAttribute>();
                    if ((attribute != null) && (attribute.Transactions != TransactionFlowOption.NotAllowed))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }
            TransactionFlowBindingElement element = b as TransactionFlowBindingElement;
            if (element == null)
            {
                return false;
            }
            if (this.transactions != element.transactions)
            {
                return false;
            }
            if (this.issuedTokens != element.issuedTokens)
            {
                return false;
            }
            if (this.transactionProtocol != element.transactionProtocol)
            {
                return false;
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransactionProtocol()
        {
            return (this.TransactionProtocol != System.ServiceModel.TransactionProtocol.Default);
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            TransactionFlowBindingElement element = context.BindingElements.Find<TransactionFlowBindingElement>();
            if ((element != null) && element.Transactions)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement item = null;
                foreach (OperationDescription description in context.Contract.Operations)
                {
                    TransactionFlowAttribute attribute = description.Behaviors.Find<TransactionFlowAttribute>();
                    TransactionFlowOption option = (attribute == null) ? TransactionFlowOption.NotAllowed : attribute.Transactions;
                    if (element.TransactionProtocol == System.ServiceModel.TransactionProtocol.OleTransactions)
                    {
                        item = this.GetAssertion(doc, option, "oletx", "OleTxAssertion", "http://schemas.microsoft.com/ws/2006/02/tx/oletx", exporter.PolicyVersion.Namespace);
                    }
                    else if (element.TransactionProtocol == System.ServiceModel.TransactionProtocol.WSAtomicTransactionOctober2004)
                    {
                        item = this.GetAssertion(doc, option, "wsat", "ATAssertion", "http://schemas.xmlsoap.org/ws/2004/10/wsat", exporter.PolicyVersion.Namespace);
                    }
                    else if (element.TransactionProtocol == System.ServiceModel.TransactionProtocol.WSAtomicTransaction11)
                    {
                        item = this.GetAssertion(doc, option, "wsat", "ATAssertion", "http://docs.oasis-open.org/ws-tx/wsat/2006/06", exporter.PolicyVersion.Namespace);
                    }
                    if (item != null)
                    {
                        context.GetOperationBindingAssertions(description).Add(item);
                    }
                }
            }
        }

        internal static void ValidateOption(TransactionFlowOption opt)
        {
            if (!TransactionFlowOptionHelper.IsDefined(opt))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("TransactionFlowBadOption")));
            }
        }

        [DefaultValue(false)]
        public bool AllowWildcardAction { get; set; }

        internal TransactionFlowOption IssuedTokens
        {
            get
            {
                return this.issuedTokens;
            }
            set
            {
                ValidateOption(value);
                this.issuedTokens = value;
            }
        }

        public System.ServiceModel.TransactionProtocol TransactionProtocol
        {
            get
            {
                return this.transactionProtocol;
            }
            set
            {
                if (!System.ServiceModel.TransactionProtocol.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.transactionProtocol = value;
            }
        }

        internal bool Transactions
        {
            get
            {
                return this.transactions;
            }
            set
            {
                this.transactions = value;
                this.issuedTokens = value ? TransactionFlowOption.Allowed : TransactionFlowOption.NotAllowed;
            }
        }
    }
}

