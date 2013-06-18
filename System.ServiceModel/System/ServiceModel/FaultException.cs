namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    [Serializable, KnownType(typeof(FaultException.FaultCodeData)), KnownType(typeof(FaultException.FaultCodeData[])), KnownType(typeof(FaultException.FaultReasonData)), KnownType(typeof(FaultException.FaultReasonData[]))]
    public class FaultException : CommunicationException
    {
        private string action;
        private FaultCode code;
        private MessageFault fault;
        internal const string Namespace = "http://schemas.xmlsoap.org/Microsoft/WindowsCommunicationFoundation/2005/08/Faults/";
        private FaultReason reason;

        public FaultException() : base(System.ServiceModel.SR.GetString("SFxFaultReason"))
        {
            this.code = DefaultCode;
            this.reason = DefaultReason;
        }

        public FaultException(MessageFault fault) : base(GetSafeReasonText(GetReason(fault)))
        {
            if (fault == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("fault");
            }
            this.code = EnsureCode(fault.Code);
            this.reason = EnsureReason(fault.Reason);
            this.fault = fault;
        }

        public FaultException(FaultReason reason) : base(GetSafeReasonText(reason))
        {
            this.code = DefaultCode;
            this.reason = EnsureReason(reason);
        }

        public FaultException(string reason) : base(reason)
        {
            this.code = DefaultCode;
            this.reason = CreateReason(reason);
        }

        protected FaultException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.code = this.ReconstructFaultCode(info, "code");
            this.reason = this.ReconstructFaultReason(info, "reason");
            this.fault = (MessageFault) info.GetValue("messageFault", typeof(MessageFault));
            this.action = info.GetString("action");
        }

        public FaultException(MessageFault fault, string action) : base(GetSafeReasonText(GetReason(fault)))
        {
            if (fault == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("fault");
            }
            this.code = fault.Code;
            this.reason = fault.Reason;
            this.fault = fault;
            this.action = action;
        }

        public FaultException(FaultReason reason, FaultCode code) : base(GetSafeReasonText(reason))
        {
            this.code = EnsureCode(code);
            this.reason = EnsureReason(reason);
        }

        public FaultException(string reason, FaultCode code) : base(reason)
        {
            this.code = EnsureCode(code);
            this.reason = CreateReason(reason);
        }

        public FaultException(FaultReason reason, FaultCode code, string action) : base(GetSafeReasonText(reason))
        {
            this.code = EnsureCode(code);
            this.reason = EnsureReason(reason);
            this.action = action;
        }

        public FaultException(string reason, FaultCode code, string action) : base(reason)
        {
            this.code = EnsureCode(code);
            this.reason = CreateReason(reason);
            this.action = action;
        }

        internal FaultException(FaultReason reason, FaultCode code, string action, Exception innerException) : base(GetSafeReasonText(reason), innerException)
        {
            this.code = EnsureCode(code);
            this.reason = EnsureReason(reason);
            this.action = action;
        }

        internal FaultException(string reason, FaultCode code, string action, Exception innerException) : base(reason, innerException)
        {
            this.code = EnsureCode(code);
            this.reason = CreateReason(reason);
            this.action = action;
        }

        internal void AddFaultCodeObjectData(SerializationInfo info, string key, FaultCode code)
        {
            info.AddValue(key, FaultCodeData.GetObjectData(code));
        }

        internal void AddFaultReasonObjectData(SerializationInfo info, string key, FaultReason reason)
        {
            info.AddValue(key, FaultReasonData.GetObjectData(reason));
        }

        private static FaultCode CreateCode(string code)
        {
            if (code == null)
            {
                return DefaultCode;
            }
            return new FaultCode(code);
        }

        public static FaultException CreateFault(MessageFault messageFault, params System.Type[] faultDetailTypes)
        {
            return CreateFault(messageFault, null, faultDetailTypes);
        }

        public static FaultException CreateFault(MessageFault messageFault, string action, params System.Type[] faultDetailTypes)
        {
            if (messageFault == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageFault");
            }
            if (faultDetailTypes == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("faultDetailTypes");
            }
            DataContractSerializerFaultFormatter formatter = new DataContractSerializerFaultFormatter(faultDetailTypes);
            return formatter.Deserialize(messageFault, action);
        }

        public virtual MessageFault CreateMessageFault()
        {
            if (this.fault != null)
            {
                return this.fault;
            }
            return MessageFault.CreateFault(this.code, this.reason);
        }

        private static FaultReason CreateReason(string reason)
        {
            if (reason == null)
            {
                return DefaultReason;
            }
            return new FaultReason(reason);
        }

        private static FaultCode EnsureCode(FaultCode code)
        {
            if (code == null)
            {
                return DefaultCode;
            }
            return code;
        }

        private static FaultReason EnsureReason(FaultReason reason)
        {
            if (reason == null)
            {
                return DefaultReason;
            }
            return reason;
        }

        [SecurityCritical, SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            this.AddFaultCodeObjectData(info, "code", this.code);
            this.AddFaultReasonObjectData(info, "reason", this.reason);
            info.AddValue("messageFault", this.fault);
            info.AddValue("action", this.action);
        }

        private static FaultReason GetReason(MessageFault fault)
        {
            if (fault == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("fault");
            }
            return fault.Reason;
        }

        internal static string GetSafeReasonText(MessageFault messageFault)
        {
            if (messageFault == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageFault");
            }
            return GetSafeReasonText(messageFault.Reason);
        }

        internal static string GetSafeReasonText(FaultReason reason)
        {
            if (reason == null)
            {
                return System.ServiceModel.SR.GetString("SFxUnknownFaultNullReason0");
            }
            try
            {
                return reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text;
            }
            catch (ArgumentException)
            {
                if (reason.Translations.Count == 0)
                {
                    return System.ServiceModel.SR.GetString("SFxUnknownFaultZeroReasons0");
                }
                return System.ServiceModel.SR.GetString("SFxUnknownFaultNoMatchingTranslation1", new object[] { reason.Translations[0].Text });
            }
        }

        internal FaultCode ReconstructFaultCode(SerializationInfo info, string key)
        {
            FaultCodeData[] nodes = (FaultCodeData[]) info.GetValue(key, typeof(FaultCodeData[]));
            return FaultCodeData.Construct(nodes);
        }

        internal FaultReason ReconstructFaultReason(SerializationInfo info, string key)
        {
            FaultReasonData[] nodes = (FaultReasonData[]) info.GetValue(key, typeof(FaultReasonData[]));
            return FaultReasonData.Construct(nodes);
        }

        public string Action
        {
            get
            {
                return this.action;
            }
        }

        public FaultCode Code
        {
            get
            {
                return this.code;
            }
        }

        private static FaultCode DefaultCode
        {
            get
            {
                return new FaultCode("Sender");
            }
        }

        private static FaultReason DefaultReason
        {
            get
            {
                return new FaultReason(System.ServiceModel.SR.GetString("SFxFaultReason"));
            }
        }

        internal MessageFault Fault
        {
            get
            {
                return this.fault;
            }
        }

        public override string Message
        {
            get
            {
                return GetSafeReasonText(this.Reason);
            }
        }

        public FaultReason Reason
        {
            get
            {
                return this.reason;
            }
        }

        [Serializable]
        internal class FaultCodeData
        {
            private string name;
            private string ns;

            internal static FaultCode Construct(FaultException.FaultCodeData[] nodes)
            {
                FaultCode subCode = null;
                for (int i = nodes.Length - 1; i >= 0; i--)
                {
                    subCode = new FaultCode(nodes[i].name, nodes[i].ns, subCode);
                }
                return subCode;
            }

            private static int GetDepth(FaultCode code)
            {
                int num = 0;
                while (code != null)
                {
                    num++;
                    code = code.SubCode;
                }
                return num;
            }

            internal static FaultException.FaultCodeData[] GetObjectData(FaultCode code)
            {
                FaultException.FaultCodeData[] dataArray = new FaultException.FaultCodeData[GetDepth(code)];
                for (int i = 0; i < dataArray.Length; i++)
                {
                    dataArray[i] = new FaultException.FaultCodeData();
                    dataArray[i].name = code.Name;
                    dataArray[i].ns = code.Namespace;
                    code = code.SubCode;
                }
                return dataArray;
            }
        }

        [Serializable]
        internal class FaultReasonData
        {
            private string text;
            private string xmlLang;

            internal static FaultReason Construct(FaultException.FaultReasonData[] nodes)
            {
                FaultReasonText[] translations = new FaultReasonText[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    translations[i] = new FaultReasonText(nodes[i].text, nodes[i].xmlLang);
                }
                return new FaultReason(translations);
            }

            internal static FaultException.FaultReasonData[] GetObjectData(FaultReason reason)
            {
                SynchronizedReadOnlyCollection<FaultReasonText> translations = reason.Translations;
                FaultException.FaultReasonData[] dataArray = new FaultException.FaultReasonData[translations.Count];
                for (int i = 0; i < translations.Count; i++)
                {
                    dataArray[i] = new FaultException.FaultReasonData();
                    dataArray[i].xmlLang = translations[i].XmlLang;
                    dataArray[i].text = translations[i].Text;
                }
                return dataArray;
            }
        }
    }
}

