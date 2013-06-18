namespace System.Web.Services.Protocols
{
    using System;
    using System.Net;
    using System.Web;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Xml;

    internal abstract class SoapServerProtocolHelper
    {
        private SoapServerProtocol protocol;
        private string requestNamespace;

        protected SoapServerProtocolHelper(SoapServerProtocol protocol)
        {
            this.protocol = protocol;
        }

        protected SoapServerProtocolHelper(SoapServerProtocol protocol, string requestNamespace)
        {
            this.protocol = protocol;
            this.requestNamespace = requestNamespace;
        }

        internal static SoapServerProtocolHelper GetHelper(SoapServerProtocol protocol, string envelopeNs)
        {
            if (envelopeNs == "http://schemas.xmlsoap.org/soap/envelope/")
            {
                return new Soap11ServerProtocolHelper(protocol, envelopeNs);
            }
            if (envelopeNs == "http://www.w3.org/2003/05/soap-envelope")
            {
                return new Soap12ServerProtocolHelper(protocol, envelopeNs);
            }
            return new Soap11ServerProtocolHelper(protocol, envelopeNs);
        }

        protected XmlQualifiedName GetRequestElement()
        {
            XmlQualifiedName empty;
            SoapServerMessage message = this.ServerProtocol.Message;
            long position = message.Stream.Position;
            XmlReader xmlReader = this.protocol.GetXmlReader();
            xmlReader.MoveToContent();
            this.requestNamespace = xmlReader.NamespaceURI;
            if (!xmlReader.IsStartElement("Envelope", this.requestNamespace))
            {
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebMissingEnvelopeElement"));
            }
            if (xmlReader.IsEmptyElement)
            {
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebMissingBodyElement"));
            }
            xmlReader.ReadStartElement("Envelope", this.requestNamespace);
            xmlReader.MoveToContent();
            while (!xmlReader.EOF && !xmlReader.IsStartElement("Body", this.requestNamespace))
            {
                xmlReader.Skip();
            }
            if (xmlReader.EOF)
            {
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebMissingBodyElement"));
            }
            if (xmlReader.IsEmptyElement)
            {
                empty = XmlQualifiedName.Empty;
            }
            else
            {
                xmlReader.ReadStartElement("Body", this.requestNamespace);
                xmlReader.MoveToContent();
                empty = new XmlQualifiedName(xmlReader.LocalName, xmlReader.NamespaceURI);
            }
            message.Stream.Position = position;
            return empty;
        }

        internal abstract SoapServerMethod RouteRequest();
        internal HttpStatusCode SetResponseErrorCode(HttpResponse response, SoapException soapException)
        {
            if ((soapException.SubCode != null) && (soapException.SubCode.Code == Soap12FaultCodes.UnsupportedMediaTypeFaultCode))
            {
                response.StatusCode = 0x19f;
                soapException.ClearSubCode();
            }
            else if (SoapException.IsClientFaultCode(soapException.Code))
            {
                System.Web.Services.Protocols.ServerProtocol.SetHttpResponseStatusCode(response, 500);
                for (Exception exception = soapException; exception != null; exception = exception.InnerException)
                {
                    if (exception is XmlException)
                    {
                        response.StatusCode = 400;
                    }
                }
            }
            else
            {
                System.Web.Services.Protocols.ServerProtocol.SetHttpResponseStatusCode(response, 500);
            }
            response.StatusDescription = HttpWorkerRequest.GetStatusDescription(response.StatusCode);
            return (HttpStatusCode) response.StatusCode;
        }

        internal abstract void WriteFault(XmlWriter writer, SoapException soapException, HttpStatusCode statusCode);

        internal abstract string EncodingNs { get; }

        internal abstract string EnvelopeNs { get; }

        internal abstract string HttpContentType { get; }

        internal abstract WebServiceProtocols Protocol { get; }

        internal string RequestNamespace
        {
            get
            {
                return this.requestNamespace;
            }
        }

        protected SoapServerProtocol ServerProtocol
        {
            get
            {
                return this.protocol;
            }
        }

        protected SoapServerType ServerType
        {
            get
            {
                return (SoapServerType) this.protocol.ServerType;
            }
        }

        internal abstract SoapProtocolVersion Version { get; }
    }
}

