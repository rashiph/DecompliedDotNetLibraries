namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    public class DataContractSerializerOperationBehavior : IOperationBehavior, IWsdlExportExtension
    {
        private readonly bool builtInOperationBehavior;
        private System.ServiceModel.DataContractFormatAttribute dataContractFormatAttribute;
        private System.Runtime.Serialization.DataContractResolver dataContractResolver;
        private IDataContractSurrogate dataContractSurrogate;
        internal bool ignoreExtensionDataObject;
        private bool ignoreExtensionDataObjectSetExplicit;
        internal int maxItemsInObjectGraph;
        private bool maxItemsInObjectGraphSetExplicit;
        private OperationDescription operation;

        public DataContractSerializerOperationBehavior(OperationDescription operation) : this(operation, null)
        {
        }

        public DataContractSerializerOperationBehavior(OperationDescription operation, System.ServiceModel.DataContractFormatAttribute dataContractFormatAttribute)
        {
            this.maxItemsInObjectGraph = 0x10000;
            this.dataContractFormatAttribute = dataContractFormatAttribute ?? new System.ServiceModel.DataContractFormatAttribute();
            this.operation = operation;
        }

        internal DataContractSerializerOperationBehavior(OperationDescription operation, System.ServiceModel.DataContractFormatAttribute dataContractFormatAttribute, bool builtInOperationBehavior) : this(operation, dataContractFormatAttribute)
        {
            this.builtInOperationBehavior = builtInOperationBehavior;
        }

        public virtual XmlObjectSerializer CreateSerializer(System.Type type, string name, string ns, IList<System.Type> knownTypes)
        {
            return new DataContractSerializer(type, name, ns, knownTypes, this.MaxItemsInObjectGraph, this.IgnoreExtensionDataObject, false, this.DataContractSurrogate, this.DataContractResolver);
        }

        public virtual XmlObjectSerializer CreateSerializer(System.Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<System.Type> knownTypes)
        {
            return new DataContractSerializer(type, name, ns, knownTypes, this.MaxItemsInObjectGraph, this.IgnoreExtensionDataObject, false, this.DataContractSurrogate, this.DataContractResolver);
        }

        internal object GetFormatter(OperationDescription operation, out bool formatRequest, out bool formatReply, bool isProxy)
        {
            MessageDescription description = operation.Messages[0];
            MessageDescription description2 = null;
            if (operation.Messages.Count == 2)
            {
                description2 = operation.Messages[1];
            }
            formatRequest = (description != null) && !description.IsUntypedMessage;
            formatReply = (description2 != null) && !description2.IsUntypedMessage;
            if (!formatRequest && !formatReply)
            {
                return null;
            }
            if (PrimitiveOperationFormatter.IsContractSupported(operation))
            {
                return new PrimitiveOperationFormatter(operation, this.dataContractFormatAttribute.Style == OperationFormatStyle.Rpc);
            }
            return new DataContractSerializerOperationFormatter(operation, this.dataContractFormatAttribute, this);
        }

        void IOperationBehavior.AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
        {
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription description, ClientOperation proxy)
        {
            if (description == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            if (proxy == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("proxy");
            }
            if (proxy.Formatter == null)
            {
                bool flag;
                bool flag2;
                proxy.Formatter = (IClientMessageFormatter) this.GetFormatter(description, out flag, out flag2, true);
                proxy.SerializeRequest = flag;
                proxy.DeserializeReply = flag2;
            }
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch)
        {
            if (description == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            if (dispatch == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatch");
            }
            if (dispatch.Formatter == null)
            {
                bool flag;
                bool flag2;
                dispatch.Formatter = (IDispatchMessageFormatter) this.GetFormatter(description, out flag, out flag2, false);
                dispatch.DeserializeRequest = flag;
                dispatch.SerializeReply = flag2;
            }
        }

        void IOperationBehavior.Validate(OperationDescription description)
        {
        }

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext contractContext)
        {
            if (exporter == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }
            if (contractContext == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractContext");
            }
            new DataContractSerializerMessageContractExporter(exporter, contractContext, this.operation, this).ExportMessageContract();
        }

        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext)
        {
            if (exporter == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }
            if (endpointContext == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointContext");
            }
            MessageContractExporter.ExportMessageBinding(exporter, endpointContext, typeof(DataContractSerializerMessageContractExporter), this.operation);
        }

        public System.ServiceModel.DataContractFormatAttribute DataContractFormatAttribute
        {
            get
            {
                return this.dataContractFormatAttribute;
            }
        }

        public System.Runtime.Serialization.DataContractResolver DataContractResolver
        {
            get
            {
                return this.dataContractResolver;
            }
            set
            {
                this.dataContractResolver = value;
            }
        }

        public IDataContractSurrogate DataContractSurrogate
        {
            get
            {
                return this.dataContractSurrogate;
            }
            set
            {
                this.dataContractSurrogate = value;
            }
        }

        public bool IgnoreExtensionDataObject
        {
            get
            {
                return this.ignoreExtensionDataObject;
            }
            set
            {
                this.ignoreExtensionDataObject = value;
                this.ignoreExtensionDataObjectSetExplicit = true;
            }
        }

        internal bool IgnoreExtensionDataObjectSetExplicit
        {
            get
            {
                return this.ignoreExtensionDataObjectSetExplicit;
            }
            set
            {
                this.ignoreExtensionDataObjectSetExplicit = value;
            }
        }

        internal bool IsBuiltInOperationBehavior
        {
            get
            {
                return this.builtInOperationBehavior;
            }
        }

        public int MaxItemsInObjectGraph
        {
            get
            {
                return this.maxItemsInObjectGraph;
            }
            set
            {
                this.maxItemsInObjectGraph = value;
                this.maxItemsInObjectGraphSetExplicit = true;
            }
        }

        internal bool MaxItemsInObjectGraphSetExplicit
        {
            get
            {
                return this.maxItemsInObjectGraphSetExplicit;
            }
            set
            {
                this.maxItemsInObjectGraphSetExplicit = value;
            }
        }
    }
}

