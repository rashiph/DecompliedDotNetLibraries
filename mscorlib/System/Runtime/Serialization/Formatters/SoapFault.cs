namespace System.Runtime.Serialization.Formatters
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Metadata;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true), SoapType(Embedded=true)]
    public sealed class SoapFault : ISerializable
    {
        [SoapField(Embedded=true)]
        private object detail;
        private string faultActor;
        private string faultCode;
        private string faultString;

        public SoapFault()
        {
        }

        internal SoapFault(SerializationInfo info, StreamingContext context)
        {
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string name = enumerator.Name;
                object obj2 = enumerator.Value;
                if (string.Compare(name, "faultCode", true, CultureInfo.InvariantCulture) == 0)
                {
                    int index = ((string) obj2).IndexOf(':');
                    if (index > -1)
                    {
                        this.faultCode = ((string) obj2).Substring(++index);
                    }
                    else
                    {
                        this.faultCode = (string) obj2;
                    }
                }
                else if (string.Compare(name, "faultString", true, CultureInfo.InvariantCulture) == 0)
                {
                    this.faultString = (string) obj2;
                }
                else
                {
                    if (string.Compare(name, "faultActor", true, CultureInfo.InvariantCulture) == 0)
                    {
                        this.faultActor = (string) obj2;
                        continue;
                    }
                    if (string.Compare(name, "detail", true, CultureInfo.InvariantCulture) == 0)
                    {
                        this.detail = obj2;
                    }
                }
            }
        }

        public SoapFault(string faultCode, string faultString, string faultActor, ServerFault serverFault)
        {
            this.faultCode = faultCode;
            this.faultString = faultString;
            this.faultActor = faultActor;
            this.detail = serverFault;
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("faultcode", "SOAP-ENV:" + this.faultCode);
            info.AddValue("faultstring", this.faultString);
            if (this.faultActor != null)
            {
                info.AddValue("faultactor", this.faultActor);
            }
            info.AddValue("detail", this.detail, typeof(object));
        }

        public object Detail
        {
            get
            {
                return this.detail;
            }
            set
            {
                this.detail = value;
            }
        }

        public string FaultActor
        {
            get
            {
                return this.faultActor;
            }
            set
            {
                this.faultActor = value;
            }
        }

        public string FaultCode
        {
            get
            {
                return this.faultCode;
            }
            set
            {
                this.faultCode = value;
            }
        }

        public string FaultString
        {
            get
            {
                return this.faultString;
            }
            set
            {
                this.faultString = value;
            }
        }
    }
}

