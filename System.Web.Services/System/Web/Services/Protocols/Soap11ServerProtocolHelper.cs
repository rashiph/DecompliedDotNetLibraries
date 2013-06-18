namespace System.Web.Services.Protocols
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Web;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Web.Services.Description;
    using System.Web.Services.Diagnostics;
    using System.Xml;

    internal class Soap11ServerProtocolHelper : SoapServerProtocolHelper
    {
        internal Soap11ServerProtocolHelper(SoapServerProtocol protocol) : base(protocol)
        {
        }

        internal Soap11ServerProtocolHelper(SoapServerProtocol protocol, string requestNamespace) : base(protocol, requestNamespace)
        {
        }

        internal override SoapServerMethod RouteRequest()
        {
            object requestElement;
            string str = base.ServerProtocol.Request.Headers["SOAPAction"];
            if (str == null)
            {
                throw new SoapException(System.Web.Services.Res.GetString("UnableToHandleRequestActionRequired0"), new XmlQualifiedName("Client", "http://schemas.xmlsoap.org/soap/envelope/"));
            }
            if (base.ServerType.routingOnSoapAction)
            {
                if (str.StartsWith("\"", StringComparison.Ordinal) && str.EndsWith("\"", StringComparison.Ordinal))
                {
                    str = str.Substring(1, str.Length - 2);
                }
                requestElement = System.Web.HttpUtility.UrlDecode(str);
            }
            else
            {
                try
                {
                    requestElement = base.GetRequestElement();
                }
                catch (SoapException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                    {
                        throw;
                    }
                    throw new SoapException(System.Web.Services.Res.GetString("TheRootElementForTheRequestCouldNotBeDetermined0"), new XmlQualifiedName("Server", "http://schemas.xmlsoap.org/soap/envelope/"), exception);
                }
            }
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "RouteRequest", new object[0]) : null;
            if (Tracing.On)
            {
                Tracing.Enter("RouteRequest", caller, new TraceMethod(base.ServerType, "GetMethod", new object[] { requestElement }), Tracing.Details(base.ServerProtocol.Request));
            }
            SoapServerMethod method = base.ServerType.GetMethod(requestElement);
            if (Tracing.On)
            {
                Tracing.Exit("RouteRequest", caller);
            }
            if (method != null)
            {
                return method;
            }
            if (base.ServerType.routingOnSoapAction)
            {
                throw new SoapException(System.Web.Services.Res.GetString("WebHttpHeader", new object[] { "SOAPAction", (string) requestElement }), new XmlQualifiedName("Client", "http://schemas.xmlsoap.org/soap/envelope/"));
            }
            throw new SoapException(System.Web.Services.Res.GetString("TheRequestElementXmlnsWasNotRecognized2", new object[] { ((XmlQualifiedName) requestElement).Name, ((XmlQualifiedName) requestElement).Namespace }), new XmlQualifiedName("Client", "http://schemas.xmlsoap.org/soap/envelope/"));
        }

        private static XmlQualifiedName TranslateFaultCode(XmlQualifiedName code)
        {
            if (code.Namespace != "http://schemas.xmlsoap.org/soap/envelope/")
            {
                if (!(code.Namespace == "http://www.w3.org/2003/05/soap-envelope"))
                {
                    return code;
                }
                if (code.Name == "Receiver")
                {
                    return SoapException.ServerFaultCode;
                }
                if (code.Name == "Sender")
                {
                    return SoapException.ClientFaultCode;
                }
                if (code.Name == "MustUnderstand")
                {
                    return SoapException.MustUnderstandFaultCode;
                }
                if (code.Name == "VersionMismatch")
                {
                    return SoapException.VersionMismatchFaultCode;
                }
            }
            return code;
        }

        internal override void WriteFault(XmlWriter writer, SoapException soapException, HttpStatusCode statusCode)
        {
            if ((statusCode == HttpStatusCode.InternalServerError) && (soapException != null))
            {
                SoapServerMessage message = base.ServerProtocol.Message;
                writer.WriteStartDocument();
                writer.WriteStartElement("soap", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
                writer.WriteAttributeString("xmlns", "soap", null, "http://schemas.xmlsoap.org/soap/envelope/");
                writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
                if (base.ServerProtocol.ServerMethod != null)
                {
                    SoapHeaderHandling.WriteHeaders(writer, base.ServerProtocol.ServerMethod.outHeaderSerializer, message.Headers, base.ServerProtocol.ServerMethod.outHeaderMappings, SoapHeaderDirection.Fault, base.ServerProtocol.ServerMethod.use == SoapBindingUse.Encoded, base.ServerType.serviceNamespace, base.ServerType.serviceDefaultIsEncoded, "http://schemas.xmlsoap.org/soap/envelope/");
                }
                else
                {
                    SoapHeaderHandling.WriteUnknownHeaders(writer, message.Headers, "http://schemas.xmlsoap.org/soap/envelope/");
                }
                writer.WriteStartElement("Body", "http://schemas.xmlsoap.org/soap/envelope/");
                writer.WriteStartElement("Fault", "http://schemas.xmlsoap.org/soap/envelope/");
                writer.WriteStartElement("faultcode", "");
                XmlQualifiedName name = TranslateFaultCode(soapException.Code);
                if (((name.Namespace != null) && (name.Namespace.Length > 0)) && (writer.LookupPrefix(name.Namespace) == null))
                {
                    writer.WriteAttributeString("xmlns", "q0", null, name.Namespace);
                }
                writer.WriteQualifiedName(name.Name, name.Namespace);
                writer.WriteEndElement();
                writer.WriteStartElement("faultstring", "");
                if ((soapException.Lang != null) && (soapException.Lang.Length != 0))
                {
                    writer.WriteAttributeString("xml", "lang", "http://www.w3.org/XML/1998/namespace", soapException.Lang);
                }
                writer.WriteString(base.ServerProtocol.GenerateFaultString(soapException));
                writer.WriteEndElement();
                string actor = soapException.Actor;
                if (actor.Length > 0)
                {
                    writer.WriteElementString("faultactor", "", actor);
                }
                if (!(soapException is SoapHeaderException))
                {
                    if (soapException.Detail == null)
                    {
                        writer.WriteStartElement("detail", "");
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

        internal override string EncodingNs
        {
            get
            {
                return "http://schemas.xmlsoap.org/soap/encoding/";
            }
        }

        internal override string EnvelopeNs
        {
            get
            {
                return "http://schemas.xmlsoap.org/soap/envelope/";
            }
        }

        internal override string HttpContentType
        {
            get
            {
                return "text/xml";
            }
        }

        internal override WebServiceProtocols Protocol
        {
            get
            {
                return WebServiceProtocols.HttpSoap;
            }
        }

        internal override SoapProtocolVersion Version
        {
            get
            {
                return SoapProtocolVersion.Soap11;
            }
        }
    }
}

