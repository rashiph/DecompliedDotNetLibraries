namespace System.Web.Services.Protocols
{
    using System;
    using System.Net;
    using System.Web;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Web.Services.Description;
    using System.Web.Services.Diagnostics;
    using System.Xml;

    internal class Soap12ServerProtocolHelper : SoapServerProtocolHelper
    {
        internal Soap12ServerProtocolHelper(SoapServerProtocol protocol) : base(protocol)
        {
        }

        internal Soap12ServerProtocolHelper(SoapServerProtocol protocol, string requestNamespace) : base(protocol, requestNamespace)
        {
        }

        internal override SoapServerMethod RouteRequest()
        {
            string action = ContentType.GetAction(base.ServerProtocol.Request.ContentType);
            SoapServerMethod method = null;
            bool flag = false;
            bool flag2 = false;
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "RouteRequest", new object[0]) : null;
            if ((action != null) && (action.Length > 0))
            {
                action = System.Web.HttpUtility.UrlDecode(action);
                if (Tracing.On)
                {
                    Tracing.Enter("RouteRequest", caller, new TraceMethod(base.ServerType, "GetMethod", new object[] { action }), Tracing.Details(base.ServerProtocol.Request));
                }
                method = base.ServerType.GetMethod(action);
                if (Tracing.On)
                {
                    Tracing.Exit("RouteRequest", caller);
                }
                if ((method != null) && (base.ServerType.GetDuplicateMethod(action) != null))
                {
                    method = null;
                    flag = true;
                }
            }
            XmlQualifiedName empty = XmlQualifiedName.Empty;
            if (method == null)
            {
                empty = base.GetRequestElement();
                if (Tracing.On)
                {
                    Tracing.Enter("RouteRequest", caller, new TraceMethod(base.ServerType, "GetMethod", new object[] { empty }), Tracing.Details(base.ServerProtocol.Request));
                }
                method = base.ServerType.GetMethod(empty);
                if (Tracing.On)
                {
                    Tracing.Exit("RouteRequest", caller);
                }
                if ((method != null) && (base.ServerType.GetDuplicateMethod(empty) != null))
                {
                    method = null;
                    flag2 = true;
                }
            }
            if (method != null)
            {
                return method;
            }
            if ((action == null) || (action.Length == 0))
            {
                throw new SoapException(System.Web.Services.Res.GetString("UnableToHandleRequestActionRequired0"), Soap12FaultCodes.SenderFaultCode);
            }
            if (flag)
            {
                if (flag2)
                {
                    throw new SoapException(System.Web.Services.Res.GetString("UnableToHandleRequest0"), Soap12FaultCodes.ReceiverFaultCode);
                }
                throw new SoapException(System.Web.Services.Res.GetString("TheRequestElementXmlnsWasNotRecognized2", new object[] { empty.Name, empty.Namespace }), Soap12FaultCodes.SenderFaultCode);
            }
            throw new SoapException(System.Web.Services.Res.GetString("UnableToHandleRequestActionNotRecognized1", new object[] { action }), Soap12FaultCodes.SenderFaultCode);
        }

        private static XmlQualifiedName TranslateFaultCode(XmlQualifiedName code)
        {
            if (code.Namespace == "http://schemas.xmlsoap.org/soap/envelope/")
            {
                if (code.Name == "Server")
                {
                    return Soap12FaultCodes.ReceiverFaultCode;
                }
                if (code.Name == "Client")
                {
                    return Soap12FaultCodes.SenderFaultCode;
                }
                if (code.Name == "MustUnderstand")
                {
                    return Soap12FaultCodes.MustUnderstandFaultCode;
                }
                if (code.Name == "VersionMismatch")
                {
                    return Soap12FaultCodes.VersionMismatchFaultCode;
                }
            }
            return code;
        }

        internal override void WriteFault(XmlWriter writer, SoapException soapException, HttpStatusCode statusCode)
        {
            if ((statusCode == HttpStatusCode.InternalServerError) && (soapException != null))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("soap", "Envelope", "http://www.w3.org/2003/05/soap-envelope");
                writer.WriteAttributeString("xmlns", "soap", null, "http://www.w3.org/2003/05/soap-envelope");
                writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
                if (base.ServerProtocol.ServerMethod != null)
                {
                    SoapHeaderHandling.WriteHeaders(writer, base.ServerProtocol.ServerMethod.outHeaderSerializer, base.ServerProtocol.Message.Headers, base.ServerProtocol.ServerMethod.outHeaderMappings, SoapHeaderDirection.Fault, base.ServerProtocol.ServerMethod.use == SoapBindingUse.Encoded, base.ServerType.serviceNamespace, base.ServerType.serviceDefaultIsEncoded, "http://www.w3.org/2003/05/soap-envelope");
                }
                else
                {
                    SoapHeaderHandling.WriteUnknownHeaders(writer, base.ServerProtocol.Message.Headers, "http://www.w3.org/2003/05/soap-envelope");
                }
                writer.WriteStartElement("Body", "http://www.w3.org/2003/05/soap-envelope");
                writer.WriteStartElement("Fault", "http://www.w3.org/2003/05/soap-envelope");
                writer.WriteStartElement("Code", "http://www.w3.org/2003/05/soap-envelope");
                WriteFaultCodeValue(writer, TranslateFaultCode(soapException.Code), soapException.SubCode);
                writer.WriteEndElement();
                writer.WriteStartElement("Reason", "http://www.w3.org/2003/05/soap-envelope");
                writer.WriteStartElement("Text", "http://www.w3.org/2003/05/soap-envelope");
                writer.WriteAttributeString("xml", "lang", "http://www.w3.org/XML/1998/namespace", System.Web.Services.Res.GetString("XmlLang"));
                writer.WriteString(base.ServerProtocol.GenerateFaultString(soapException));
                writer.WriteEndElement();
                writer.WriteEndElement();
                string actor = soapException.Actor;
                if (actor.Length > 0)
                {
                    writer.WriteElementString("Node", "http://www.w3.org/2003/05/soap-envelope", actor);
                }
                string role = soapException.Role;
                if (role.Length > 0)
                {
                    writer.WriteElementString("Role", "http://www.w3.org/2003/05/soap-envelope", role);
                }
                if (!(soapException is SoapHeaderException))
                {
                    if (soapException.Detail == null)
                    {
                        writer.WriteStartElement("Detail", "http://www.w3.org/2003/05/soap-envelope");
                        writer.WriteEndElement();
                    }
                    else
                    {
                        soapException.Detail.WriteTo(writer);
                    }
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.Flush();
            }
        }

        private static void WriteFaultCodeValue(XmlWriter writer, XmlQualifiedName code, SoapFaultSubCode subcode)
        {
            if (code != null)
            {
                writer.WriteStartElement("Value", "http://www.w3.org/2003/05/soap-envelope");
                if (((code.Namespace != null) && (code.Namespace.Length > 0)) && (writer.LookupPrefix(code.Namespace) == null))
                {
                    writer.WriteAttributeString("xmlns", "q0", null, code.Namespace);
                }
                writer.WriteQualifiedName(code.Name, code.Namespace);
                writer.WriteEndElement();
                if (subcode != null)
                {
                    writer.WriteStartElement("Subcode", "http://www.w3.org/2003/05/soap-envelope");
                    WriteFaultCodeValue(writer, subcode.Code, subcode.SubCode);
                    writer.WriteEndElement();
                }
            }
        }

        internal override string EncodingNs
        {
            get
            {
                return "http://www.w3.org/2003/05/soap-encoding";
            }
        }

        internal override string EnvelopeNs
        {
            get
            {
                return "http://www.w3.org/2003/05/soap-envelope";
            }
        }

        internal override string HttpContentType
        {
            get
            {
                return "application/soap+xml";
            }
        }

        internal override WebServiceProtocols Protocol
        {
            get
            {
                return WebServiceProtocols.HttpSoap12;
            }
        }

        internal override SoapProtocolVersion Version
        {
            get
            {
                return SoapProtocolVersion.Soap12;
            }
        }
    }
}

