namespace System.Web.Services.Description
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Text;
    using System.Web.Services.Configuration;
    using System.Xml.Serialization;

    [XmlFormatExtensionPoint("Extensions")]
    public sealed class Operation : NamedItem
    {
        private ServiceDescriptionFormatExtensionCollection extensions;
        private OperationFaultCollection faults;
        private OperationMessageCollection messages;
        private string[] parameters;
        private System.Web.Services.Description.PortType parent;

        private string GetMessageName(string operationName, string messageName, bool isInput)
        {
            if ((messageName != null) && (messageName.Length > 0))
            {
                return messageName;
            }
            switch (this.Messages.Flow)
            {
                case OperationFlow.OneWay:
                    if (!isInput)
                    {
                        return null;
                    }
                    return operationName;

                case OperationFlow.RequestResponse:
                    if (!isInput)
                    {
                        return (operationName + "Response");
                    }
                    return (operationName + "Request");
            }
            return null;
        }

        public bool IsBoundBy(OperationBinding operationBinding)
        {
            if (operationBinding.Name != base.Name)
            {
                return false;
            }
            OperationMessage input = this.Messages.Input;
            if (input != null)
            {
                if (operationBinding.Input == null)
                {
                    return false;
                }
                string str = this.GetMessageName(base.Name, input.Name, true);
                if (this.GetMessageName(operationBinding.Name, operationBinding.Input.Name, true) != str)
                {
                    return false;
                }
            }
            else if (operationBinding.Input != null)
            {
                return false;
            }
            OperationMessage output = this.Messages.Output;
            if (output != null)
            {
                if (operationBinding.Output == null)
                {
                    return false;
                }
                string str3 = this.GetMessageName(base.Name, output.Name, false);
                if (this.GetMessageName(operationBinding.Name, operationBinding.Output.Name, false) != str3)
                {
                    return false;
                }
            }
            else if (operationBinding.Output != null)
            {
                return false;
            }
            return true;
        }

        internal void SetParent(System.Web.Services.Description.PortType parent)
        {
            this.parent = parent;
        }

        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new ServiceDescriptionFormatExtensionCollection(this);
                }
                return this.extensions;
            }
        }

        [XmlElement("fault")]
        public OperationFaultCollection Faults
        {
            get
            {
                if (this.faults == null)
                {
                    this.faults = new OperationFaultCollection(this);
                }
                return this.faults;
            }
        }

        [XmlElement("output", typeof(OperationOutput)), XmlElement("input", typeof(OperationInput))]
        public OperationMessageCollection Messages
        {
            get
            {
                if (this.messages == null)
                {
                    this.messages = new OperationMessageCollection(this);
                }
                return this.messages;
            }
        }

        [XmlIgnore]
        public string[] ParameterOrder
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parameters;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.parameters = value;
            }
        }

        [XmlAttribute("parameterOrder"), DefaultValue("")]
        public string ParameterOrderString
        {
            get
            {
                if (this.parameters == null)
                {
                    return string.Empty;
                }
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < this.parameters.Length; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(' ');
                    }
                    builder.Append(this.parameters[i]);
                }
                return builder.ToString();
            }
            set
            {
                if (value == null)
                {
                    this.parameters = null;
                }
                else
                {
                    this.parameters = value.Split(new char[] { ' ' });
                }
            }
        }

        public System.Web.Services.Description.PortType PortType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parent;
            }
        }
    }
}

