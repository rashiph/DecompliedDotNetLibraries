namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom;
    using System.Collections.ObjectModel;
    using System.ServiceModel;

    public class ServiceContractGenerationContext
    {
        private CodeTypeDeclaration channelType;
        private CodeTypeReference channelTypeReference;
        private CodeTypeDeclaration clientType;
        private CodeTypeReference clientTypeReference;
        private CodeNamespace codeNamespace;
        private readonly ContractDescription contract;
        private readonly CodeTypeDeclaration contractType;
        private CodeTypeReference contractTypeReference;
        private readonly CodeTypeDeclaration duplexCallbackType;
        private CodeTypeReference duplexCallbackTypeReference;
        private readonly Collection<OperationContractGenerationContext> operations;
        private readonly System.ServiceModel.Description.ServiceContractGenerator serviceContractGenerator;
        private System.ServiceModel.Description.ServiceContractGenerator.CodeTypeFactory typeFactory;

        public ServiceContractGenerationContext(System.ServiceModel.Description.ServiceContractGenerator serviceContractGenerator, ContractDescription contract, CodeTypeDeclaration contractType)
        {
            this.operations = new Collection<OperationContractGenerationContext>();
            if (serviceContractGenerator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serviceContractGenerator"));
            }
            if (contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contract"));
            }
            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contractType"));
            }
            this.serviceContractGenerator = serviceContractGenerator;
            this.contract = contract;
            this.contractType = contractType;
        }

        public ServiceContractGenerationContext(System.ServiceModel.Description.ServiceContractGenerator serviceContractGenerator, ContractDescription contract, CodeTypeDeclaration contractType, CodeTypeDeclaration duplexCallbackType) : this(serviceContractGenerator, contract, contractType)
        {
            this.duplexCallbackType = duplexCallbackType;
        }

        internal CodeTypeDeclaration ChannelType
        {
            get
            {
                return this.channelType;
            }
            set
            {
                this.channelType = value;
            }
        }

        internal CodeTypeReference ChannelTypeReference
        {
            get
            {
                return this.channelTypeReference;
            }
            set
            {
                this.channelTypeReference = value;
            }
        }

        internal CodeTypeDeclaration ClientType
        {
            get
            {
                return this.clientType;
            }
            set
            {
                this.clientType = value;
            }
        }

        internal CodeTypeReference ClientTypeReference
        {
            get
            {
                return this.clientTypeReference;
            }
            set
            {
                this.clientTypeReference = value;
            }
        }

        public ContractDescription Contract
        {
            get
            {
                return this.contract;
            }
        }

        public CodeTypeDeclaration ContractType
        {
            get
            {
                return this.contractType;
            }
        }

        internal CodeTypeReference ContractTypeReference
        {
            get
            {
                return this.contractTypeReference;
            }
            set
            {
                this.contractTypeReference = value;
            }
        }

        public CodeTypeDeclaration DuplexCallbackType
        {
            get
            {
                return this.duplexCallbackType;
            }
        }

        internal CodeTypeReference DuplexCallbackTypeReference
        {
            get
            {
                return this.duplexCallbackTypeReference;
            }
            set
            {
                this.duplexCallbackTypeReference = value;
            }
        }

        internal CodeNamespace Namespace
        {
            get
            {
                return this.codeNamespace;
            }
            set
            {
                this.codeNamespace = value;
            }
        }

        public Collection<OperationContractGenerationContext> Operations
        {
            get
            {
                return this.operations;
            }
        }

        public System.ServiceModel.Description.ServiceContractGenerator ServiceContractGenerator
        {
            get
            {
                return this.serviceContractGenerator;
            }
        }

        internal System.ServiceModel.Description.ServiceContractGenerator.CodeTypeFactory TypeFactory
        {
            get
            {
                return this.typeFactory;
            }
            set
            {
                this.typeFactory = value;
            }
        }
    }
}

