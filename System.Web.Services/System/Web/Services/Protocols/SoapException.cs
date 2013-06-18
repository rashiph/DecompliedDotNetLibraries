namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Xml;

    [Serializable]
    public class SoapException : SystemException
    {
        private string actor;
        public static readonly XmlQualifiedName ClientFaultCode = new XmlQualifiedName("Client", "http://schemas.xmlsoap.org/soap/envelope/");
        private XmlQualifiedName code;
        private XmlNode detail;
        public static readonly XmlQualifiedName DetailElementName = new XmlQualifiedName("detail", "");
        private string lang;
        public static readonly XmlQualifiedName MustUnderstandFaultCode = new XmlQualifiedName("MustUnderstand", "http://schemas.xmlsoap.org/soap/envelope/");
        private string role;
        public static readonly XmlQualifiedName ServerFaultCode = new XmlQualifiedName("Server", "http://schemas.xmlsoap.org/soap/envelope/");
        private SoapFaultSubCode subCode;
        public static readonly XmlQualifiedName VersionMismatchFaultCode = new XmlQualifiedName("VersionMismatch", "http://schemas.xmlsoap.org/soap/envelope/");

        public SoapException() : base(null)
        {
            this.code = XmlQualifiedName.Empty;
        }

        protected SoapException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.code = XmlQualifiedName.Empty;
            IDictionary data = base.Data;
            this.code = (XmlQualifiedName) data["code"];
            this.actor = (string) data["actor"];
            this.role = (string) data["role"];
            this.subCode = (SoapFaultSubCode) data["subCode"];
            this.lang = (string) data["lang"];
        }

        public SoapException(string message, XmlQualifiedName code) : base(message)
        {
            this.code = XmlQualifiedName.Empty;
            this.code = code;
        }

        public SoapException(string message, XmlQualifiedName code, Exception innerException) : base(message, innerException)
        {
            this.code = XmlQualifiedName.Empty;
            this.code = code;
        }

        public SoapException(string message, XmlQualifiedName code, string actor) : base(message)
        {
            this.code = XmlQualifiedName.Empty;
            this.code = code;
            this.actor = actor;
        }

        public SoapException(string message, XmlQualifiedName code, SoapFaultSubCode subCode) : base(message)
        {
            this.code = XmlQualifiedName.Empty;
            this.code = code;
            this.subCode = subCode;
        }

        public SoapException(string message, XmlQualifiedName code, string actor, Exception innerException) : base(message, innerException)
        {
            this.code = XmlQualifiedName.Empty;
            this.code = code;
            this.actor = actor;
        }

        public SoapException(string message, XmlQualifiedName code, string actor, XmlNode detail) : base(message)
        {
            this.code = XmlQualifiedName.Empty;
            this.code = code;
            this.actor = actor;
            this.detail = detail;
        }

        public SoapException(string message, XmlQualifiedName code, string actor, XmlNode detail, Exception innerException) : base(message, innerException)
        {
            this.code = XmlQualifiedName.Empty;
            this.code = code;
            this.actor = actor;
            this.detail = detail;
        }

        public SoapException(string message, XmlQualifiedName code, string actor, string role, XmlNode detail, SoapFaultSubCode subCode, Exception innerException) : base(message, innerException)
        {
            this.code = XmlQualifiedName.Empty;
            this.code = code;
            this.actor = actor;
            this.role = role;
            this.detail = detail;
            this.subCode = subCode;
        }

        public SoapException(string message, XmlQualifiedName code, string actor, string role, string lang, XmlNode detail, SoapFaultSubCode subCode, Exception innerException) : base(message, innerException)
        {
            this.code = XmlQualifiedName.Empty;
            this.code = code;
            this.actor = actor;
            this.role = role;
            this.detail = detail;
            this.lang = lang;
            this.subCode = subCode;
        }

        internal void ClearSubCode()
        {
            if (this.subCode != null)
            {
                this.subCode = this.subCode.SubCode;
            }
        }

        internal static SoapException Create(SoapProtocolVersion soapVersion, string message, XmlQualifiedName code, Exception innerException)
        {
            if (WebServicesSection.Current.Diagnostics.SuppressReturningExceptions)
            {
                return CreateSuppressedException(soapVersion, System.Web.Services.Res.GetString("WebSuppressedExceptionMessage"), innerException);
            }
            return new SoapException(message, code, innerException);
        }

        internal static SoapException Create(SoapProtocolVersion soapVersion, string message, XmlQualifiedName code, string actor, string role, XmlNode detail, SoapFaultSubCode subCode, Exception innerException)
        {
            if (WebServicesSection.Current.Diagnostics.SuppressReturningExceptions)
            {
                return CreateSuppressedException(soapVersion, System.Web.Services.Res.GetString("WebSuppressedExceptionMessage"), innerException);
            }
            return new SoapException(message, code, actor, role, detail, subCode, innerException);
        }

        private static SoapException CreateSuppressedException(SoapProtocolVersion soapVersion, string message, Exception innerException)
        {
            return new SoapException(System.Web.Services.Res.GetString("WebSuppressedExceptionMessage"), (soapVersion == SoapProtocolVersion.Soap12) ? new XmlQualifiedName("Receiver", "http://www.w3.org/2003/05/soap-envelope") : new XmlQualifiedName("Server", "http://schemas.xmlsoap.org/soap/envelope/"));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            IDictionary data = this.Data;
            data["code"] = this.Code;
            data["actor"] = this.Actor;
            data["role"] = this.Role;
            data["subCode"] = this.SubCode;
            data["lang"] = this.Lang;
            base.GetObjectData(info, context);
        }

        public static bool IsClientFaultCode(XmlQualifiedName code)
        {
            if (!(code == ClientFaultCode))
            {
                return (code == Soap12FaultCodes.SenderFaultCode);
            }
            return true;
        }

        public static bool IsMustUnderstandFaultCode(XmlQualifiedName code)
        {
            if (!(code == MustUnderstandFaultCode))
            {
                return (code == Soap12FaultCodes.MustUnderstandFaultCode);
            }
            return true;
        }

        public static bool IsServerFaultCode(XmlQualifiedName code)
        {
            if (!(code == ServerFaultCode))
            {
                return (code == Soap12FaultCodes.ReceiverFaultCode);
            }
            return true;
        }

        public static bool IsVersionMismatchFaultCode(XmlQualifiedName code)
        {
            if (!(code == VersionMismatchFaultCode))
            {
                return (code == Soap12FaultCodes.VersionMismatchFaultCode);
            }
            return true;
        }

        public string Actor
        {
            get
            {
                if (this.actor != null)
                {
                    return this.actor;
                }
                return string.Empty;
            }
        }

        public XmlQualifiedName Code
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.code;
            }
        }

        public XmlNode Detail
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.detail;
            }
        }

        [ComVisible(false)]
        public string Lang
        {
            get
            {
                if (this.lang != null)
                {
                    return this.lang;
                }
                return string.Empty;
            }
        }

        [ComVisible(false)]
        public string Node
        {
            get
            {
                if (this.actor != null)
                {
                    return this.actor;
                }
                return string.Empty;
            }
        }

        [ComVisible(false)]
        public string Role
        {
            get
            {
                if (this.role != null)
                {
                    return this.role;
                }
                return string.Empty;
            }
        }

        [ComVisible(false)]
        public SoapFaultSubCode SubCode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.subCode;
            }
        }
    }
}

