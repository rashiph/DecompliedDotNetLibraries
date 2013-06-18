namespace System.Web.Services.Protocols
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public sealed class SoapUnknownHeader : SoapHeader
    {
        private XmlElement element;

        private string GetElementAttribute(string name, string ns, XmlElement element)
        {
            if (element != null)
            {
                if ((element.Prefix.Length == 0) && (element.NamespaceURI == ns))
                {
                    if (element.HasAttribute(name))
                    {
                        return element.GetAttribute(name);
                    }
                    return null;
                }
                if (element.HasAttribute(name, ns))
                {
                    return element.GetAttribute(name, ns);
                }
            }
            return null;
        }

        [XmlIgnore]
        public XmlElement Element
        {
            get
            {
                if (this.element == null)
                {
                    return null;
                }
                if (base.version == SoapProtocolVersion.Soap12)
                {
                    if (this.InternalMustUnderstand)
                    {
                        this.element.SetAttribute("mustUnderstand", "http://www.w3.org/2003/05/soap-envelope", "1");
                    }
                    this.element.RemoveAttribute("mustUnderstand", "http://schemas.xmlsoap.org/soap/envelope/");
                    string internalActor = this.InternalActor;
                    if ((internalActor != null) && (internalActor.Length != 0))
                    {
                        this.element.SetAttribute("role", "http://www.w3.org/2003/05/soap-envelope", internalActor);
                    }
                    this.element.RemoveAttribute("actor", "http://schemas.xmlsoap.org/soap/envelope/");
                }
                else if (base.version == SoapProtocolVersion.Soap11)
                {
                    if (this.InternalMustUnderstand)
                    {
                        this.element.SetAttribute("mustUnderstand", "http://schemas.xmlsoap.org/soap/envelope/", "1");
                    }
                    this.element.RemoveAttribute("mustUnderstand", "http://www.w3.org/2003/05/soap-envelope");
                    string str2 = this.InternalActor;
                    if ((str2 != null) && (str2.Length != 0))
                    {
                        this.element.SetAttribute("actor", "http://schemas.xmlsoap.org/soap/envelope/", str2);
                    }
                    this.element.RemoveAttribute("role", "http://www.w3.org/2003/05/soap-envelope");
                    this.element.RemoveAttribute("relay", "http://www.w3.org/2003/05/soap-envelope");
                }
                return this.element;
            }
            set
            {
                if ((value == null) && (this.element != null))
                {
                    base.InternalMustUnderstand = this.InternalMustUnderstand;
                    base.InternalActor = this.InternalActor;
                }
                this.element = value;
            }
        }

        internal override string InternalActor
        {
            get
            {
                if (this.element == null)
                {
                    return base.InternalActor;
                }
                string str = this.GetElementAttribute("actor", "http://schemas.xmlsoap.org/soap/envelope/", this.element);
                if (str == null)
                {
                    str = this.GetElementAttribute("role", "http://www.w3.org/2003/05/soap-envelope", this.element);
                    if (str == null)
                    {
                        return "";
                    }
                }
                return str;
            }
            set
            {
                base.InternalActor = value;
                if (this.element != null)
                {
                    if ((value == null) || (value.Length == 0))
                    {
                        this.element.RemoveAttribute("actor", "http://schemas.xmlsoap.org/soap/envelope/");
                    }
                    else
                    {
                        this.element.SetAttribute("actor", "http://schemas.xmlsoap.org/soap/envelope/", value);
                    }
                    this.element.RemoveAttribute("role", "http://www.w3.org/2003/05/soap-envelope");
                }
            }
        }

        internal override bool InternalMustUnderstand
        {
            get
            {
                if (this.element == null)
                {
                    return base.InternalMustUnderstand;
                }
                string str = this.GetElementAttribute("mustUnderstand", "http://schemas.xmlsoap.org/soap/envelope/", this.element);
                if (str == null)
                {
                    str = this.GetElementAttribute("mustUnderstand", "http://www.w3.org/2003/05/soap-envelope", this.element);
                    if (str == null)
                    {
                        return false;
                    }
                }
                switch (str)
                {
                    case "false":
                    case "0":
                        return false;

                    case "true":
                    case "1":
                        return true;
                }
                return false;
            }
            set
            {
                base.InternalMustUnderstand = value;
                if (this.element != null)
                {
                    if (value)
                    {
                        this.element.SetAttribute("mustUnderstand", "http://schemas.xmlsoap.org/soap/envelope/", "1");
                    }
                    else
                    {
                        this.element.RemoveAttribute("mustUnderstand", "http://schemas.xmlsoap.org/soap/envelope/");
                    }
                    this.element.RemoveAttribute("mustUnderstand", "http://www.w3.org/2003/05/soap-envelope");
                }
            }
        }

        internal override bool InternalRelay
        {
            get
            {
                if (this.element == null)
                {
                    return base.InternalRelay;
                }
                switch (this.GetElementAttribute("relay", "http://www.w3.org/2003/05/soap-envelope", this.element))
                {
                    case "false":
                    case "0":
                        return false;

                    case "true":
                    case "1":
                        return true;

                    case null:
                        return false;
                }
                return false;
            }
            set
            {
                base.InternalRelay = value;
                if (this.element != null)
                {
                    if (value)
                    {
                        this.element.SetAttribute("relay", "http://www.w3.org/2003/05/soap-envelope", "1");
                    }
                    else
                    {
                        this.element.RemoveAttribute("relay", "http://www.w3.org/2003/05/soap-envelope");
                    }
                }
            }
        }
    }
}

