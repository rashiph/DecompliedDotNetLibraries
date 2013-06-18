namespace System.ServiceModel.Description
{
    using System;

    internal static class MetadataStrings
    {
        public static class Addressing10
        {
            public const string NamespaceUri = "http://www.w3.org/2005/08/addressing";
            public const string Prefix = "wsa10";

            public static class MetadataPolicy
            {
                public const string Addressing = "Addressing";
                public const string AnonymousResponses = "AnonymousResponses";
                public const string NamespaceUri = "http://www.w3.org/2007/05/addressing/metadata";
                public const string NonAnonymousResponses = "NonAnonymousResponses";
                public const string Prefix = "wsam";
            }

            public static class WsdlBindingPolicy
            {
                public const string NamespaceUri = "http://www.w3.org/2006/05/addressing/wsdl";
                public const string Prefix = "wsaw";
                public const string UsingAddressing = "UsingAddressing";
            }
        }

        public static class Addressing200408
        {
            public const string NamespaceUri = "http://schemas.xmlsoap.org/ws/2004/08/addressing";
            public const string Prefix = "wsa";

            public static class Policy
            {
                public const string NamespaceUri = "http://schemas.xmlsoap.org/ws/2004/08/addressing/policy";
                public const string Prefix = "wsap";
                public const string UsingAddressing = "UsingAddressing";
            }
        }

        public static class AddressingMetadata
        {
            public const string Action = "Action";
            public const string NamespaceUri = "http://www.w3.org/2007/05/addressing/metadata";
            public const string Prefix = "wsam";
        }

        public static class AddressingWsdl
        {
            public const string Action = "Action";
            public const string NamespaceUri = "http://www.w3.org/2006/05/addressing/wsdl";
            public const string Prefix = "wsaw";
        }

        public static class MetadataExchangeStrings
        {
            public const string BindingNamespace = "http://schemas.microsoft.com/ws/2005/02/mex/bindings";
            public const string Dialect = "Dialect";
            public const string HttpBindingName = "MetadataExchangeHttpBinding";
            public const string HttpsBindingName = "MetadataExchangeHttpsBinding";
            public const string Identifier = "Identifier";
            public const string Location = "Location";
            public const string Metadata = "Metadata";
            public const string MetadataReference = "MetadataReference";
            public const string MetadataSection = "MetadataSection";
            public const string Name = "WS-MetadataExchange";
            public const string NamedPipeBindingName = "MetadataExchangeNamedPipeBinding";
            public const string Namespace = "http://schemas.xmlsoap.org/ws/2004/09/mex";
            public const string Prefix = "wsx";
            public const string TcpBindingName = "MetadataExchangeTcpBinding";
        }

        public static class ServiceDescription
        {
            public const string ArrayType = "arrayType";
            public const string Definitions = "definitions";
        }

        public static class WSPolicy
        {
            public const string NamespaceUri = "http://schemas.xmlsoap.org/ws/2004/09/policy";
            public const string NamespaceUri15 = "http://www.w3.org/ns/ws-policy";
            public const string Prefix = "wsp";

            public static class Attributes
            {
                public const string Optional = "Optional";
                public const string PolicyURIs = "PolicyURIs";
                public const string TargetNamespace = "TargetNamespace";
                public const string URI = "URI";
            }

            public static class Elements
            {
                public const string All = "All";
                public const string ExactlyOne = "ExactlyOne";
                public const string Policy = "Policy";
                public const string PolicyReference = "PolicyReference";
            }
        }

        public static class WSTransfer
        {
            public const string GetAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/Get";
            public const string GetResponseAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse";
            public const string Name = "WS-Transfer";
            public const string Namespace = "http://schemas.xmlsoap.org/ws/2004/09/transfer";
            public const string Prefix = "wxf";
        }

        public static class Wsu
        {
            public const string NamespaceUri = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
            public const string Prefix = "wsu";

            public static class Attributes
            {
                public const string Id = "Id";
            }
        }

        public static class Xml
        {
            public const string NamespaceUri = "http://www.w3.org/XML/1998/namespace";
            public const string Prefix = "xml";

            public static class Attributes
            {
                public const string Id = "id";
            }
        }

        public static class XmlSchema
        {
            public const string Schema = "schema";
        }
    }
}

