namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class DsmlErrorResponse : DirectoryResponse
    {
        private ErrorResponseCategory category;
        private string detail;
        private string message;

        internal DsmlErrorResponse(XmlNode node) : base(node)
        {
            this.category = ~ErrorResponseCategory.NotAttempted;
        }

        public override DirectoryControl[] Controls
        {
            get
            {
                throw new NotSupportedException(System.DirectoryServices.Protocols.Res.GetString("NotSupportOnDsmlErrRes"));
            }
        }

        public string Detail
        {
            get
            {
                if (this.detail == null)
                {
                    XmlElement element = (XmlElement) base.dsmlNode.SelectSingleNode("dsml:detail", base.dsmlNS);
                    if (element != null)
                    {
                        this.detail = element.InnerXml;
                    }
                }
                return this.detail;
            }
        }

        public override string ErrorMessage
        {
            get
            {
                throw new NotSupportedException(System.DirectoryServices.Protocols.Res.GetString("NotSupportOnDsmlErrRes"));
            }
        }

        public override string MatchedDN
        {
            get
            {
                throw new NotSupportedException(System.DirectoryServices.Protocols.Res.GetString("NotSupportOnDsmlErrRes"));
            }
        }

        public string Message
        {
            get
            {
                if (this.message == null)
                {
                    XmlElement element = (XmlElement) base.dsmlNode.SelectSingleNode("dsml:message", base.dsmlNS);
                    if (element != null)
                    {
                        this.message = element.InnerText;
                    }
                }
                return this.message;
            }
        }

        public override Uri[] Referral
        {
            get
            {
                throw new NotSupportedException(System.DirectoryServices.Protocols.Res.GetString("NotSupportOnDsmlErrRes"));
            }
        }

        public override System.DirectoryServices.Protocols.ResultCode ResultCode
        {
            get
            {
                throw new NotSupportedException(System.DirectoryServices.Protocols.Res.GetString("NotSupportOnDsmlErrRes"));
            }
        }

        public ErrorResponseCategory Type
        {
            get
            {
                if (this.category == ~ErrorResponseCategory.NotAttempted)
                {
                    XmlAttribute attribute = (XmlAttribute) base.dsmlNode.SelectSingleNode("@dsml:type", base.dsmlNS);
                    if (attribute == null)
                    {
                        attribute = (XmlAttribute) base.dsmlNode.SelectSingleNode("@type", base.dsmlNS);
                    }
                    if (attribute == null)
                    {
                        throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("MissingErrorResponseType"));
                    }
                    switch (attribute.Value)
                    {
                        case "notAttempted":
                            this.category = ErrorResponseCategory.NotAttempted;
                            goto Label_017F;

                        case "couldNotConnect":
                            this.category = ErrorResponseCategory.CouldNotConnect;
                            goto Label_017F;

                        case "connectionClosed":
                            this.category = ErrorResponseCategory.ConnectionClosed;
                            goto Label_017F;

                        case "malformedRequest":
                            this.category = ErrorResponseCategory.MalformedRequest;
                            goto Label_017F;

                        case "gatewayInternalError":
                            this.category = ErrorResponseCategory.GatewayInternalError;
                            goto Label_017F;

                        case "authenticationFailed":
                            this.category = ErrorResponseCategory.AuthenticationFailed;
                            goto Label_017F;

                        case "unresolvableURI":
                            this.category = ErrorResponseCategory.UnresolvableUri;
                            goto Label_017F;

                        case "other":
                            this.category = ErrorResponseCategory.Other;
                            goto Label_017F;
                    }
                    throw new DsmlInvalidDocumentException(System.DirectoryServices.Protocols.Res.GetString("ErrorResponseInvalidValue", new object[] { attribute.Value }));
                }
            Label_017F:
                return this.category;
            }
        }
    }
}

