namespace System.Web.Services.Description
{
    using System;
    using System.Collections;
    using System.Web.Services;
    using System.Xml;

    internal class Soap12ProtocolReflector : SoapProtocolReflector
    {
        private Hashtable actions;
        private Hashtable requestElements;
        private XmlQualifiedName soap11PortType;

        protected override void BeginClass()
        {
            this.requestElements = new Hashtable();
            this.actions = new Hashtable();
            this.soap11PortType = null;
            base.BeginClass();
        }

        private void CheckOperationDuplicates(Soap12OperationBinding operation)
        {
            if (operation.DuplicateByRequestElement != null)
            {
                if (operation.DuplicateBySoapAction != null)
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("TheMethodsAndUseTheSameRequestElementAndSoapActionXmlns6", new object[] { operation.Method.name, operation.DuplicateByRequestElement.Method.name, operation.Method.requestElementName.Name, operation.Method.requestElementName.Namespace, operation.DuplicateBySoapAction.Method.name, operation.Method.action }));
                }
                operation.SoapActionRequired = true;
            }
            else
            {
                operation.SoapActionRequired = false;
            }
        }

        protected override SoapAddressBinding CreateSoapAddressBinding(string serviceUrl)
        {
            return new Soap12AddressBinding { Location = serviceUrl };
        }

        protected override SoapBinding CreateSoapBinding(SoapBindingStyle style)
        {
            return new Soap12Binding { Transport = "http://schemas.xmlsoap.org/soap/http", Style = style };
        }

        protected override SoapBodyBinding CreateSoapBodyBinding(SoapBindingUse use, string ns)
        {
            Soap12BodyBinding binding = new Soap12BodyBinding {
                Use = use
            };
            if (use == SoapBindingUse.Encoded)
            {
                binding.Encoding = "http://www.w3.org/2003/05/soap-encoding";
            }
            binding.Namespace = ns;
            return binding;
        }

        protected override SoapHeaderBinding CreateSoapHeaderBinding(XmlQualifiedName message, string partName, SoapBindingUse use)
        {
            return this.CreateSoapHeaderBinding(message, partName, null, use);
        }

        protected override SoapHeaderBinding CreateSoapHeaderBinding(XmlQualifiedName message, string partName, string ns, SoapBindingUse use)
        {
            Soap12HeaderBinding binding = new Soap12HeaderBinding {
                Message = message,
                Part = partName,
                Namespace = ns,
                Use = use
            };
            if (use == SoapBindingUse.Encoded)
            {
                binding.Encoding = "http://www.w3.org/2003/05/soap-encoding";
            }
            return binding;
        }

        protected override SoapOperationBinding CreateSoapOperationBinding(SoapBindingStyle style, string action)
        {
            Soap12OperationBinding operation = new Soap12OperationBinding {
                SoapAction = action,
                Style = style,
                Method = base.SoapMethod
            };
            this.DealWithAmbiguity(action, base.SoapMethod.requestElementName.ToString(), operation);
            return operation;
        }

        private void DealWithAmbiguity(string action, string requestElement, Soap12OperationBinding operation)
        {
            Soap12OperationBinding binding = (Soap12OperationBinding) this.actions[action];
            if (binding != null)
            {
                operation.DuplicateBySoapAction = binding;
                binding.DuplicateBySoapAction = operation;
                this.CheckOperationDuplicates(binding);
            }
            else
            {
                this.actions[action] = operation;
            }
            Soap12OperationBinding binding2 = (Soap12OperationBinding) this.requestElements[requestElement];
            if (binding2 != null)
            {
                operation.DuplicateByRequestElement = binding2;
                binding2.DuplicateByRequestElement = operation;
                this.CheckOperationDuplicates(binding2);
            }
            else
            {
                this.requestElements[requestElement] = operation;
            }
            this.CheckOperationDuplicates(operation);
        }

        protected override void EndClass()
        {
            if (((base.PortType != null) && (base.Binding != null)) && ((this.soap11PortType != null) && (this.soap11PortType != base.Binding.Type)))
            {
                foreach (Operation operation in base.PortType.Operations)
                {
                    foreach (OperationMessage message in operation.Messages)
                    {
                        ServiceDescription serviceDescription = base.GetServiceDescription(message.Message.Namespace);
                        if (serviceDescription != null)
                        {
                            Message message2 = serviceDescription.Messages[message.Message.Name];
                            if (message2 != null)
                            {
                                serviceDescription.Messages.Remove(message2);
                            }
                        }
                    }
                }
                base.Binding.Type = this.soap11PortType;
                base.PortType.ServiceDescription.PortTypes.Remove(base.PortType);
            }
        }

        protected override bool ReflectMethod()
        {
            if (!base.ReflectMethod())
            {
                return false;
            }
            if (base.Binding != null)
            {
                this.soap11PortType = base.SoapMethod.portType;
                if (this.soap11PortType != base.Binding.Type)
                {
                    base.HeaderMessages.Clear();
                }
            }
            return true;
        }

        internal override WsiProfiles ConformsTo
        {
            get
            {
                return WsiProfiles.None;
            }
        }

        public override string ProtocolName
        {
            get
            {
                return "Soap12";
            }
        }
    }
}

