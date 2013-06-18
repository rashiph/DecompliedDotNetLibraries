namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public abstract class MetadataExporter
    {
        private readonly Collection<MetadataConversionError> errors = new Collection<MetadataConversionError>();
        private System.ServiceModel.Description.PolicyVersion policyVersion = System.ServiceModel.Description.PolicyVersion.Policy12;
        private readonly Dictionary<object, object> state = new Dictionary<object, object>();

        internal MetadataExporter()
        {
        }

        private Exception CreateExtensionException(IPolicyExportExtension exporter, Exception e)
        {
            return new InvalidOperationException(System.ServiceModel.SR.GetString("PolicyExtensionExportError", new object[] { exporter.GetType(), e.Message }), e);
        }

        public abstract void ExportContract(ContractDescription contract);
        public abstract void ExportEndpoint(ServiceEndpoint endpoint);
        protected internal PolicyConversionContext ExportPolicy(ServiceEndpoint endpoint)
        {
            PolicyConversionContext context = new ExportedPolicyConversionContext(endpoint);
            foreach (IPolicyExportExtension extension in endpoint.Binding.CreateBindingElements().FindAll<IPolicyExportExtension>())
            {
                try
                {
                    extension.ExportPolicy(this, context);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateExtensionException(extension, exception));
                }
            }
            return context;
        }

        public abstract MetadataSet GetGeneratedMetadata();

        public Collection<MetadataConversionError> Errors
        {
            get
            {
                return this.errors;
            }
        }

        public System.ServiceModel.Description.PolicyVersion PolicyVersion
        {
            get
            {
                return this.policyVersion;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.policyVersion = value;
            }
        }

        public Dictionary<object, object> State
        {
            get
            {
                return this.state;
            }
        }

        private sealed class ExportedPolicyConversionContext : PolicyConversionContext
        {
            private PolicyAssertionCollection bindingAssertions;
            private readonly BindingElementCollection bindingElements;
            private Dictionary<FaultDescription, PolicyAssertionCollection> faultBindingAssertions;
            private Dictionary<MessageDescription, PolicyAssertionCollection> messageBindingAssertions;
            private Dictionary<OperationDescription, PolicyAssertionCollection> operationBindingAssertions;

            internal ExportedPolicyConversionContext(ServiceEndpoint endpoint) : base(endpoint)
            {
                this.bindingElements = endpoint.Binding.CreateBindingElements();
                this.bindingAssertions = new PolicyAssertionCollection();
                this.operationBindingAssertions = new Dictionary<OperationDescription, PolicyAssertionCollection>();
                this.messageBindingAssertions = new Dictionary<MessageDescription, PolicyAssertionCollection>();
                this.faultBindingAssertions = new Dictionary<FaultDescription, PolicyAssertionCollection>();
            }

            public override PolicyAssertionCollection GetBindingAssertions()
            {
                return this.bindingAssertions;
            }

            public override PolicyAssertionCollection GetFaultBindingAssertions(FaultDescription fault)
            {
                lock (this.faultBindingAssertions)
                {
                    if (!this.faultBindingAssertions.ContainsKey(fault))
                    {
                        this.faultBindingAssertions.Add(fault, new PolicyAssertionCollection());
                    }
                }
                return this.faultBindingAssertions[fault];
            }

            public override PolicyAssertionCollection GetMessageBindingAssertions(MessageDescription message)
            {
                lock (this.messageBindingAssertions)
                {
                    if (!this.messageBindingAssertions.ContainsKey(message))
                    {
                        this.messageBindingAssertions.Add(message, new PolicyAssertionCollection());
                    }
                }
                return this.messageBindingAssertions[message];
            }

            public override PolicyAssertionCollection GetOperationBindingAssertions(OperationDescription operation)
            {
                lock (this.operationBindingAssertions)
                {
                    if (!this.operationBindingAssertions.ContainsKey(operation))
                    {
                        this.operationBindingAssertions.Add(operation, new PolicyAssertionCollection());
                    }
                }
                return this.operationBindingAssertions[operation];
            }

            public override BindingElementCollection BindingElements
            {
                get
                {
                    return this.bindingElements;
                }
            }
        }
    }
}

