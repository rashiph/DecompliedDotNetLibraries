namespace System.ServiceModel.Description
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Web.Services.Description;

    public class WsdlContractConversionContext
    {
        private readonly ContractDescription contract;
        private readonly Dictionary<OperationFault, FaultDescription> faultDescriptions;
        private readonly Dictionary<OperationMessage, MessageDescription> messageDescriptions;
        private readonly Dictionary<Operation, Collection<OperationBinding>> operationBindings;
        private readonly Dictionary<Operation, OperationDescription> operationDescriptions;
        private readonly Dictionary<FaultDescription, OperationFault> wsdlOperationFaults;
        private readonly Dictionary<MessageDescription, OperationMessage> wsdlOperationMessages;
        private readonly Dictionary<OperationDescription, Operation> wsdlOperations;
        private readonly PortType wsdlPortType;

        internal WsdlContractConversionContext(ContractDescription contract, PortType wsdlPortType)
        {
            this.contract = contract;
            this.wsdlPortType = wsdlPortType;
            this.wsdlOperations = new Dictionary<OperationDescription, Operation>();
            this.operationDescriptions = new Dictionary<Operation, OperationDescription>();
            this.wsdlOperationMessages = new Dictionary<MessageDescription, OperationMessage>();
            this.messageDescriptions = new Dictionary<OperationMessage, MessageDescription>();
            this.wsdlOperationFaults = new Dictionary<FaultDescription, OperationFault>();
            this.faultDescriptions = new Dictionary<OperationFault, FaultDescription>();
            this.operationBindings = new Dictionary<Operation, Collection<OperationBinding>>();
        }

        internal void AddFault(FaultDescription faultDescription, OperationFault wsdlOperationFault)
        {
            this.wsdlOperationFaults.Add(faultDescription, wsdlOperationFault);
            this.faultDescriptions.Add(wsdlOperationFault, faultDescription);
        }

        internal void AddMessage(MessageDescription messageDescription, OperationMessage wsdlOperationMessage)
        {
            this.wsdlOperationMessages.Add(messageDescription, wsdlOperationMessage);
            this.messageDescriptions.Add(wsdlOperationMessage, messageDescription);
        }

        internal void AddOperation(OperationDescription operationDescription, Operation wsdlOperation)
        {
            this.wsdlOperations.Add(operationDescription, wsdlOperation);
            this.operationDescriptions.Add(wsdlOperation, operationDescription);
        }

        public FaultDescription GetFaultDescription(OperationFault operationFault)
        {
            return this.faultDescriptions[operationFault];
        }

        public MessageDescription GetMessageDescription(OperationMessage operationMessage)
        {
            return this.messageDescriptions[operationMessage];
        }

        public Operation GetOperation(OperationDescription operation)
        {
            return this.wsdlOperations[operation];
        }

        internal Collection<OperationBinding> GetOperationBindings(Operation operation)
        {
            Collection<OperationBinding> collection;
            if (!this.operationBindings.TryGetValue(operation, out collection))
            {
                collection = new Collection<OperationBinding>();
                foreach (System.Web.Services.Description.ServiceDescription description in this.WsdlPortType.ServiceDescription.ServiceDescriptions)
                {
                    foreach (Binding binding in description.Bindings)
                    {
                        if ((binding.Type.Name == this.WsdlPortType.Name) && (binding.Type.Namespace == this.WsdlPortType.ServiceDescription.TargetNamespace))
                        {
                            foreach (OperationBinding binding2 in binding.Operations)
                            {
                                if ((binding2.Name == operation.Name) && WsdlImporter.Binding2DescriptionHelper.IsOperationBoundBy(binding2, operation))
                                {
                                    collection.Add(binding2);
                                    break;
                                }
                            }
                        }
                    }
                }
                this.operationBindings.Add(operation, collection);
            }
            return collection;
        }

        public OperationDescription GetOperationDescription(Operation operation)
        {
            return this.operationDescriptions[operation];
        }

        public OperationFault GetOperationFault(FaultDescription fault)
        {
            return this.wsdlOperationFaults[fault];
        }

        public OperationMessage GetOperationMessage(MessageDescription message)
        {
            return this.wsdlOperationMessages[message];
        }

        public ContractDescription Contract
        {
            get
            {
                return this.contract;
            }
        }

        internal IEnumerable<IWsdlExportExtension> ExportExtensions
        {
            get
            {
                foreach (IWsdlExportExtension iteratorVariable0 in this.contract.Behaviors.FindAll<IWsdlExportExtension>())
                {
                    yield return iteratorVariable0;
                }
                foreach (OperationDescription iteratorVariable1 in this.contract.Operations)
                {
                    if (WsdlExporter.OperationIsExportable(iteratorVariable1))
                    {
                        Collection<IWsdlExportExtension> iteratorVariable2 = iteratorVariable1.Behaviors.FindAll<IWsdlExportExtension>();
                        int index = 0;
                        while (index < iteratorVariable2.Count)
                        {
                            if (WsdlExporter.IsBuiltInOperationBehavior(iteratorVariable2[index]))
                            {
                                yield return iteratorVariable2[index];
                                iteratorVariable2.RemoveAt(index);
                            }
                            else
                            {
                                index++;
                            }
                        }
                        foreach (IWsdlExportExtension iteratorVariable4 in iteratorVariable2)
                        {
                            yield return iteratorVariable4;
                        }
                    }
                }
            }
        }

        public PortType WsdlPortType
        {
            get
            {
                return this.wsdlPortType;
            }
        }

    }
}

