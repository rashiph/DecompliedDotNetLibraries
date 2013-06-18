namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Xml;

    internal class XmlSerializerFaultFormatter : FaultFormatter
    {
        private SynchronizedCollection<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo> xmlSerializerFaultContractInfos;

        internal XmlSerializerFaultFormatter(System.Type[] detailTypes, SynchronizedCollection<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo> xmlSerializerFaultContractInfos) : base(detailTypes)
        {
            this.Initialize(xmlSerializerFaultContractInfos);
        }

        internal XmlSerializerFaultFormatter(SynchronizedCollection<FaultContractInfo> faultContractInfoCollection, SynchronizedCollection<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo> xmlSerializerFaultContractInfos) : base(faultContractInfoCollection)
        {
            this.Initialize(xmlSerializerFaultContractInfos);
        }

        protected override FaultException CreateFaultException(MessageFault messageFault, string action)
        {
            IList<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo> xmlSerializerFaultContractInfos;
            if (action != null)
            {
                xmlSerializerFaultContractInfos = new List<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo>();
                for (int j = 0; j < this.xmlSerializerFaultContractInfos.Count; j++)
                {
                    if ((this.xmlSerializerFaultContractInfos[j].FaultContractInfo.Action == action) || (this.xmlSerializerFaultContractInfos[j].FaultContractInfo.Action == "*"))
                    {
                        xmlSerializerFaultContractInfos.Add(this.xmlSerializerFaultContractInfos[j]);
                    }
                }
            }
            else
            {
                xmlSerializerFaultContractInfos = this.xmlSerializerFaultContractInfos;
            }
            System.Type detailType = null;
            object detailObj = null;
            for (int i = 0; i < xmlSerializerFaultContractInfos.Count; i++)
            {
                XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo info = xmlSerializerFaultContractInfos[i];
                XmlDictionaryReader readerAtDetailContents = messageFault.GetReaderAtDetailContents();
                XmlObjectSerializer serializer = info.Serializer;
                if (serializer.IsStartObject(readerAtDetailContents))
                {
                    detailType = info.FaultContractInfo.Detail;
                    try
                    {
                        detailObj = serializer.ReadObject(readerAtDetailContents);
                        FaultException exception = base.CreateFaultException(messageFault, action, detailObj, detailType, readerAtDetailContents);
                        if (exception != null)
                        {
                            return exception;
                        }
                    }
                    catch (SerializationException)
                    {
                    }
                }
            }
            return new FaultException(messageFault, action);
        }

        protected override XmlObjectSerializer GetSerializer(System.Type detailType, string faultExceptionAction, out string action)
        {
            action = faultExceptionAction;
            XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo info = null;
            for (int i = 0; i < this.xmlSerializerFaultContractInfos.Count; i++)
            {
                if (this.xmlSerializerFaultContractInfos[i].FaultContractInfo.Detail == detailType)
                {
                    info = this.xmlSerializerFaultContractInfos[i];
                    break;
                }
            }
            if (info == null)
            {
                return new XmlSerializerObjectSerializer(detailType);
            }
            if (action == null)
            {
                action = info.FaultContractInfo.Action;
            }
            return info.Serializer;
        }

        private void Initialize(SynchronizedCollection<XmlSerializerOperationBehavior.Reflector.XmlSerializerFaultContractInfo> xmlSerializerFaultContractInfos)
        {
            if (xmlSerializerFaultContractInfos == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlSerializerFaultContractInfos");
            }
            this.xmlSerializerFaultContractInfos = xmlSerializerFaultContractInfos;
        }
    }
}

