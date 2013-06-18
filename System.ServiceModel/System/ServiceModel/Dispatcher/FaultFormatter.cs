namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class FaultFormatter : IClientFaultFormatter, IDispatchFaultFormatter
    {
        private FaultContractInfo[] faultContractInfos;

        internal FaultFormatter(System.Type[] detailTypes)
        {
            List<FaultContractInfo> faultContractInfos = new List<FaultContractInfo>();
            for (int i = 0; i < detailTypes.Length; i++)
            {
                faultContractInfos.Add(new FaultContractInfo("*", detailTypes[i]));
            }
            AddInfrastructureFaults(faultContractInfos);
            this.faultContractInfos = GetSortedArray(faultContractInfos);
        }

        internal FaultFormatter(SynchronizedCollection<FaultContractInfo> faultContractInfoCollection)
        {
            List<FaultContractInfo> list;
            lock (faultContractInfoCollection.SyncRoot)
            {
                list = new List<FaultContractInfo>(faultContractInfoCollection);
            }
            AddInfrastructureFaults(list);
            this.faultContractInfos = GetSortedArray(list);
        }

        private static void AddInfrastructureFaults(List<FaultContractInfo> faultContractInfos)
        {
            faultContractInfos.Add(new FaultContractInfo("http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher/fault", typeof(ExceptionDetail)));
        }

        protected virtual FaultException CreateFaultException(MessageFault messageFault, string action)
        {
            IList<FaultContractInfo> faultContractInfos;
            if (action != null)
            {
                faultContractInfos = new List<FaultContractInfo>();
                for (int j = 0; j < this.faultContractInfos.Length; j++)
                {
                    if ((this.faultContractInfos[j].Action == action) || (this.faultContractInfos[j].Action == "*"))
                    {
                        faultContractInfos.Add(this.faultContractInfos[j]);
                    }
                }
            }
            else
            {
                faultContractInfos = this.faultContractInfos;
            }
            System.Type detailType = null;
            object detailObj = null;
            for (int i = 0; i < faultContractInfos.Count; i++)
            {
                FaultContractInfo info = faultContractInfos[i];
                XmlDictionaryReader readerAtDetailContents = messageFault.GetReaderAtDetailContents();
                XmlObjectSerializer serializer = info.Serializer;
                if (serializer.IsStartObject(readerAtDetailContents))
                {
                    detailType = info.Detail;
                    try
                    {
                        detailObj = serializer.ReadObject(readerAtDetailContents);
                        FaultException exception = this.CreateFaultException(messageFault, action, detailObj, detailType, readerAtDetailContents);
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

        protected FaultException CreateFaultException(MessageFault messageFault, string action, object detailObj, System.Type detailType, XmlDictionaryReader detailReader)
        {
            if (!detailReader.EOF)
            {
                detailReader.MoveToContent();
                if ((detailReader.NodeType != XmlNodeType.EndElement) && !detailReader.EOF)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("ExtraContentIsPresentInFaultDetail")));
                }
            }
            bool flag = true;
            if (detailObj == null)
            {
                flag = !detailType.IsValueType;
            }
            else
            {
                flag = detailType.IsAssignableFrom(detailObj.GetType());
            }
            if (flag)
            {
                return (FaultException) Activator.CreateInstance(typeof(FaultException<>).MakeGenericType(new System.Type[] { detailType }), new object[] { detailObj, messageFault.Reason, messageFault.Code, action });
            }
            return null;
        }

        private static MessageFault CreateMessageFault(XmlObjectSerializer serializer, FaultException faultException, System.Type detailType)
        {
            if (detailType == null)
            {
                if (faultException.Fault != null)
                {
                    return faultException.Fault;
                }
                return MessageFault.CreateFault(faultException.Code, faultException.Reason);
            }
            return (MessageFault) Activator.CreateInstance(typeof(OperationFault).MakeGenericType(new System.Type[] { detailType }), new object[] { serializer, faultException });
        }

        public FaultException Deserialize(MessageFault messageFault, string action)
        {
            if (!messageFault.HasDetail)
            {
                return new FaultException(messageFault, action);
            }
            return this.CreateFaultException(messageFault, action);
        }

        protected virtual XmlObjectSerializer GetSerializer(System.Type detailType, string faultExceptionAction, out string action)
        {
            action = faultExceptionAction;
            FaultContractInfo info = null;
            for (int i = 0; i < this.faultContractInfos.Length; i++)
            {
                if (this.faultContractInfos[i].Detail == detailType)
                {
                    info = this.faultContractInfos[i];
                    break;
                }
            }
            if (info == null)
            {
                return DataContractSerializerDefaults.CreateSerializer(detailType, 0x7fffffff);
            }
            if (action == null)
            {
                action = info.Action;
            }
            return info.Serializer;
        }

        private static FaultContractInfo[] GetSortedArray(List<FaultContractInfo> faultContractInfoList)
        {
            FaultContractInfo[] array = faultContractInfoList.ToArray();
            Array.Sort<FaultContractInfo>(array, (Comparison<FaultContractInfo>) ((x, y) => string.CompareOrdinal(x.Action, y.Action)));
            return array;
        }

        public MessageFault Serialize(FaultException faultException, out string action)
        {
            XmlObjectSerializer serializer = null;
            System.Type detailType = null;
            string faultExceptionAction = action = faultException.Action;
            System.Type type2 = null;
            for (System.Type type3 = faultException.GetType(); type3 != typeof(FaultException); type3 = type3.BaseType)
            {
                if (type3.IsGenericType && (type3.GetGenericTypeDefinition() == typeof(FaultException<>)))
                {
                    type2 = type3;
                    break;
                }
            }
            if (type2 != null)
            {
                detailType = type2.GetGenericArguments()[0];
                serializer = this.GetSerializer(detailType, faultExceptionAction, out action);
            }
            return CreateMessageFault(serializer, faultException, detailType);
        }

        internal class OperationFault<T> : XmlObjectSerializerFault
        {
            public OperationFault(XmlObjectSerializer serializer, FaultException<T> faultException) : base(faultException.Code, faultException.Reason, faultException.Detail, serializer, string.Empty, string.Empty)
            {
            }
        }
    }
}

