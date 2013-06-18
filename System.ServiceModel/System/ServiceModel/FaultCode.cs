namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Description;

    public class FaultCode
    {
        private string name;
        private string ns;
        private FaultCode subCode;
        private EnvelopeVersion version;

        public FaultCode(string name) : this(name, "", null)
        {
        }

        public FaultCode(string name, FaultCode subCode) : this(name, "", subCode)
        {
        }

        public FaultCode(string name, string ns) : this(name, ns, null)
        {
        }

        public FaultCode(string name, string ns, FaultCode subCode)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("name"));
            }
            if (name.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("name"));
            }
            if (!string.IsNullOrEmpty(ns))
            {
                NamingHelper.CheckUriParameter(ns, "ns");
            }
            this.name = name;
            this.ns = ns;
            this.subCode = subCode;
            if (ns == "http://www.w3.org/2003/05/soap-envelope")
            {
                this.version = EnvelopeVersion.Soap12;
            }
            else if (ns == "http://schemas.xmlsoap.org/soap/envelope/")
            {
                this.version = EnvelopeVersion.Soap11;
            }
            else if (ns == "http://schemas.microsoft.com/ws/2005/05/envelope/none")
            {
                this.version = EnvelopeVersion.None;
            }
            else
            {
                this.version = null;
            }
        }

        public static FaultCode CreateReceiverFaultCode(FaultCode subCode)
        {
            if (subCode == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("subCode"));
            }
            return new FaultCode("Receiver", subCode);
        }

        public static FaultCode CreateReceiverFaultCode(string name, string ns)
        {
            return CreateReceiverFaultCode(new FaultCode(name, ns));
        }

        public static FaultCode CreateSenderFaultCode(FaultCode subCode)
        {
            return new FaultCode("Sender", subCode);
        }

        public static FaultCode CreateSenderFaultCode(string name, string ns)
        {
            return CreateSenderFaultCode(new FaultCode(name, ns));
        }

        public bool IsPredefinedFault
        {
            get
            {
                if (this.ns.Length != 0)
                {
                    return (this.version != null);
                }
                return true;
            }
        }

        public bool IsReceiverFault
        {
            get
            {
                if (!this.IsPredefinedFault)
                {
                    return false;
                }
                return (this.name == (this.version ?? EnvelopeVersion.Soap12).ReceiverFaultName);
            }
        }

        public bool IsSenderFault
        {
            get
            {
                if (!this.IsPredefinedFault)
                {
                    return false;
                }
                return (this.name == (this.version ?? EnvelopeVersion.Soap12).SenderFaultName);
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public string Namespace
        {
            get
            {
                return this.ns;
            }
        }

        public FaultCode SubCode
        {
            get
            {
                return this.subCode;
            }
        }
    }
}

