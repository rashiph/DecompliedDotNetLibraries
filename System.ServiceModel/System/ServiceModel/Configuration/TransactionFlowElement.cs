namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class TransactionFlowElement : BindingElementExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            TransactionFlowBindingElement element = (TransactionFlowBindingElement) bindingElement;
            element.Transactions = true;
            element.TransactionProtocol = this.TransactionProtocol;
            element.AllowWildcardAction = this.AllowWildcardAction;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            TransactionFlowElement element = (TransactionFlowElement) from;
            this.TransactionProtocol = element.TransactionProtocol;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            return new TransactionFlowBindingElement(true, this.TransactionProtocol) { AllowWildcardAction = this.AllowWildcardAction };
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            TransactionFlowBindingElement element = (TransactionFlowBindingElement) bindingElement;
            this.TransactionProtocol = element.TransactionProtocol;
        }

        [ConfigurationProperty("allowWildcardAction", DefaultValue=false)]
        public bool AllowWildcardAction
        {
            get
            {
                return (bool) base["allowWildcardAction"];
            }
            set
            {
                base["allowWildcardAction"] = value;
            }
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(TransactionFlowBindingElement);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("transactionProtocol", typeof(System.ServiceModel.TransactionProtocol), "OleTransactions", new TransactionProtocolConverter(), null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("allowWildcardAction", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("transactionProtocol", DefaultValue="OleTransactions"), TypeConverter(typeof(TransactionProtocolConverter))]
        public System.ServiceModel.TransactionProtocol TransactionProtocol
        {
            get
            {
                return (System.ServiceModel.TransactionProtocol) base["transactionProtocol"];
            }
            set
            {
                base["transactionProtocol"] = value;
            }
        }
    }
}

