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

    public class WsdlEndpointConversionContext
    {
        private readonly WsdlContractConversionContext contractContext;
        private readonly ServiceEndpoint endpoint;
        private readonly Dictionary<FaultBinding, FaultDescription> faultDescriptionBindings;
        private readonly Dictionary<MessageBinding, MessageDescription> messageDescriptionBindings;
        private readonly Dictionary<OperationBinding, OperationDescription> operationDescriptionBindings;
        private readonly Binding wsdlBinding;
        private readonly Dictionary<FaultDescription, FaultBinding> wsdlFaultBindings;
        private readonly Dictionary<MessageDescription, MessageBinding> wsdlMessageBindings;
        private readonly Dictionary<OperationDescription, OperationBinding> wsdlOperationBindings;
        private readonly Port wsdlPort;

        internal WsdlEndpointConversionContext(WsdlEndpointConversionContext bindingContext, ServiceEndpoint endpoint, Port wsdlport)
        {
            this.endpoint = endpoint;
            this.wsdlBinding = bindingContext.WsdlBinding;
            this.wsdlPort = wsdlport;
            this.contractContext = bindingContext.contractContext;
            this.wsdlOperationBindings = bindingContext.wsdlOperationBindings;
            this.operationDescriptionBindings = bindingContext.operationDescriptionBindings;
            this.wsdlMessageBindings = bindingContext.wsdlMessageBindings;
            this.messageDescriptionBindings = bindingContext.messageDescriptionBindings;
            this.wsdlFaultBindings = bindingContext.wsdlFaultBindings;
            this.faultDescriptionBindings = bindingContext.faultDescriptionBindings;
        }

        internal WsdlEndpointConversionContext(WsdlContractConversionContext contractContext, ServiceEndpoint endpoint, Binding wsdlBinding, Port wsdlport)
        {
            this.endpoint = endpoint;
            this.wsdlBinding = wsdlBinding;
            this.wsdlPort = wsdlport;
            this.contractContext = contractContext;
            this.wsdlOperationBindings = new Dictionary<OperationDescription, OperationBinding>();
            this.operationDescriptionBindings = new Dictionary<OperationBinding, OperationDescription>();
            this.wsdlMessageBindings = new Dictionary<MessageDescription, MessageBinding>();
            this.messageDescriptionBindings = new Dictionary<MessageBinding, MessageDescription>();
            this.wsdlFaultBindings = new Dictionary<FaultDescription, FaultBinding>();
            this.faultDescriptionBindings = new Dictionary<FaultBinding, FaultDescription>();
        }

        internal void AddFaultBinding(FaultDescription faultDescription, FaultBinding wsdlFaultBinding)
        {
            this.wsdlFaultBindings.Add(faultDescription, wsdlFaultBinding);
            this.faultDescriptionBindings.Add(wsdlFaultBinding, faultDescription);
        }

        internal void AddMessageBinding(MessageDescription messageDescription, MessageBinding wsdlMessageBinding)
        {
            this.wsdlMessageBindings.Add(messageDescription, wsdlMessageBinding);
            this.messageDescriptionBindings.Add(wsdlMessageBinding, messageDescription);
        }

        internal void AddOperationBinding(OperationDescription operationDescription, OperationBinding wsdlOperationBinding)
        {
            this.wsdlOperationBindings.Add(operationDescription, wsdlOperationBinding);
            this.operationDescriptionBindings.Add(wsdlOperationBinding, operationDescription);
        }

        public FaultBinding GetFaultBinding(FaultDescription fault)
        {
            return this.wsdlFaultBindings[fault];
        }

        public FaultDescription GetFaultDescription(FaultBinding faultBinding)
        {
            return this.faultDescriptionBindings[faultBinding];
        }

        public MessageBinding GetMessageBinding(MessageDescription message)
        {
            return this.wsdlMessageBindings[message];
        }

        public MessageDescription GetMessageDescription(MessageBinding messageBinding)
        {
            return this.messageDescriptionBindings[messageBinding];
        }

        public OperationBinding GetOperationBinding(OperationDescription operation)
        {
            return this.wsdlOperationBindings[operation];
        }

        public OperationDescription GetOperationDescription(OperationBinding operationBinding)
        {
            return this.operationDescriptionBindings[operationBinding];
        }

        public WsdlContractConversionContext ContractConversionContext
        {
            get
            {
                return this.contractContext;
            }
        }

        public ServiceEndpoint Endpoint
        {
            get
            {
                return this.endpoint;
            }
        }

        internal IEnumerable<IWsdlExportExtension> ExportExtensions
        {
            get
            {
                foreach (IWsdlExportExtension iteratorVariable0 in this.endpoint.Behaviors.FindAll<IWsdlExportExtension>())
                {
                    yield return iteratorVariable0;
                }
                foreach (IWsdlExportExtension iteratorVariable1 in this.endpoint.Binding.CreateBindingElements().FindAll<IWsdlExportExtension>())
                {
                    yield return iteratorVariable1;
                }
                foreach (IWsdlExportExtension iteratorVariable2 in this.endpoint.Contract.Behaviors.FindAll<IWsdlExportExtension>())
                {
                    yield return iteratorVariable2;
                }
                foreach (OperationDescription iteratorVariable3 in this.endpoint.Contract.Operations)
                {
                    if (WsdlExporter.OperationIsExportable(iteratorVariable3))
                    {
                        Collection<IWsdlExportExtension> iteratorVariable4 = iteratorVariable3.Behaviors.FindAll<IWsdlExportExtension>();
                        int index = 0;
                        while (index < iteratorVariable4.Count)
                        {
                            if (WsdlExporter.IsBuiltInOperationBehavior(iteratorVariable4[index]))
                            {
                                yield return iteratorVariable4[index];
                                iteratorVariable4.RemoveAt(index);
                            }
                            else
                            {
                                index++;
                            }
                        }
                        foreach (IWsdlExportExtension iteratorVariable6 in iteratorVariable4)
                        {
                            yield return iteratorVariable6;
                        }
                    }
                }
            }
        }

        public Binding WsdlBinding
        {
            get
            {
                return this.wsdlBinding;
            }
        }

        public Port WsdlPort
        {
            get
            {
                return this.wsdlPort;
            }
        }

    }
}

