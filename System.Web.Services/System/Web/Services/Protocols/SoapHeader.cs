namespace System.Web.Services.Protocols
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web.Services;
    using System.Xml.Serialization;

    [XmlType(IncludeInSchema=false), SoapType(IncludeInSchema=false)]
    public abstract class SoapHeader
    {
        private string actor;
        private bool didUnderstand;
        private bool mustUnderstand;
        private bool relay;
        internal SoapProtocolVersion version;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected SoapHeader()
        {
        }

        [SoapAttribute("actor", Namespace="http://schemas.xmlsoap.org/soap/envelope/"), DefaultValue(""), XmlAttribute("actor", Namespace="http://schemas.xmlsoap.org/soap/envelope/")]
        public string Actor
        {
            get
            {
                if (this.version == SoapProtocolVersion.Soap12)
                {
                    return "";
                }
                return this.InternalActor;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.InternalActor = value;
            }
        }

        [XmlIgnore, SoapIgnore]
        public bool DidUnderstand
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.didUnderstand;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.didUnderstand = value;
            }
        }

        [DefaultValue("0"), XmlAttribute("mustUnderstand", Namespace="http://schemas.xmlsoap.org/soap/envelope/"), SoapAttribute("mustUnderstand", Namespace="http://schemas.xmlsoap.org/soap/envelope/")]
        public string EncodedMustUnderstand
        {
            get
            {
                if ((this.version != SoapProtocolVersion.Soap12) && this.MustUnderstand)
                {
                    return "1";
                }
                return "0";
            }
            set
            {
                switch (value)
                {
                    case "false":
                    case "0":
                        this.MustUnderstand = false;
                        return;

                    case "true":
                    case "1":
                        this.MustUnderstand = true;
                        return;
                }
                throw new ArgumentException(Res.GetString("WebHeaderInvalidMustUnderstand", new object[] { value }));
            }
        }

        [XmlAttribute("mustUnderstand", Namespace="http://www.w3.org/2003/05/soap-envelope"), ComVisible(false), SoapAttribute("mustUnderstand", Namespace="http://www.w3.org/2003/05/soap-envelope"), DefaultValue("0")]
        public string EncodedMustUnderstand12
        {
            get
            {
                if ((this.version != SoapProtocolVersion.Soap11) && this.MustUnderstand)
                {
                    return "1";
                }
                return "0";
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.EncodedMustUnderstand = value;
            }
        }

        [SoapAttribute("relay", Namespace="http://www.w3.org/2003/05/soap-envelope"), XmlAttribute("relay", Namespace="http://www.w3.org/2003/05/soap-envelope"), ComVisible(false), DefaultValue("0")]
        public string EncodedRelay
        {
            get
            {
                if ((this.version != SoapProtocolVersion.Soap11) && this.Relay)
                {
                    return "1";
                }
                return "0";
            }
            set
            {
                switch (value)
                {
                    case "false":
                    case "0":
                        this.Relay = false;
                        return;

                    case "true":
                    case "1":
                        this.Relay = true;
                        return;
                }
                throw new ArgumentException(Res.GetString("WebHeaderInvalidRelay", new object[] { value }));
            }
        }

        internal virtual string InternalActor
        {
            [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
            get
            {
                if (this.actor != null)
                {
                    return this.actor;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
            set
            {
                this.actor = value;
            }
        }

        internal virtual bool InternalMustUnderstand
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
            get
            {
                return this.mustUnderstand;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
            set
            {
                this.mustUnderstand = value;
            }
        }

        internal virtual bool InternalRelay
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
            get
            {
                return this.relay;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
            set
            {
                this.relay = value;
            }
        }

        [SoapIgnore, XmlIgnore]
        public bool MustUnderstand
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.InternalMustUnderstand;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.InternalMustUnderstand = value;
            }
        }

        [XmlIgnore, SoapIgnore, ComVisible(false)]
        public bool Relay
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.InternalRelay;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.InternalRelay = value;
            }
        }

        [ComVisible(false), DefaultValue(""), XmlAttribute("role", Namespace="http://www.w3.org/2003/05/soap-envelope"), SoapAttribute("role", Namespace="http://www.w3.org/2003/05/soap-envelope")]
        public string Role
        {
            get
            {
                if (this.version == SoapProtocolVersion.Soap11)
                {
                    return "";
                }
                return this.InternalActor;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.InternalActor = value;
            }
        }
    }
}

