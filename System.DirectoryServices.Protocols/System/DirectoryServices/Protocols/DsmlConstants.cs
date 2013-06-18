namespace System.DirectoryServices.Protocols
{
    using System;

    internal class DsmlConstants
    {
        public const string ADSessionUri = "urn:schema-microsoft-com:activedirectory:dsmlv2";
        public const string AttrBinaryTypePrefixedValue = "xsd:base64Binary";
        public const string AttrDsmlAttrName = "name";
        public const string AttrSearchReqFilterExtenmatchDnattr = "dnAttributes";
        public const string AttrSearchReqFilterExtenmatchMatchrule = "matchingRule";
        public const string AttrSearchReqFilterExtenmatchName = "name";
        public const string AttrSearchReqFilterPresentName = "name";
        public const string AttrSearchReqFilterSubstrName = "name";
        public const string AttrTypePrefixedName = "xsi:type";
        public const string DefaultSearchFilter = "<present name='objectClass' xmlns=\"urn:oasis:names:tc:DSML:2:0:core\"/>";
        public const string DsmlAddResponse = "addResponse";
        public const string DsmlAuthResponse = "authResponse";
        public const string DsmlCompareResponse = "compareResponse";
        public const string DsmlDelResponse = "delResponse";
        public const string DsmlErrorResponse = "errorResponse";
        public const string DsmlExtendedResponse = "extendedResponse";
        public const string DsmlModDNResponse = "modDNResponse";
        public const string DsmlModifyResponse = "modifyResponse";
        public const string DsmlSearchResponse = "searchResponse";
        public const string DsmlUri = "urn:oasis:names:tc:DSML:2:0:core";
        public const string ElementDsmlAttrValue = "value";
        public const string ElementSearchReqFilter = "filter";
        public const string ElementSearchReqFilterAnd = "and";
        public const string ElementSearchReqFilterApprox = "approxMatch";
        public const string ElementSearchReqFilterEqual = "equalityMatch";
        public const string ElementSearchReqFilterExtenmatch = "extensibleMatch";
        public const string ElementSearchReqFilterExtenmatchValue = "value";
        public const string ElementSearchReqFilterGrteq = "greaterOrEqual";
        public const string ElementSearchReqFilterLesseq = "lessOrEqual";
        public const string ElementSearchReqFilterNot = "not";
        public const string ElementSearchReqFilterOr = "or";
        public const string ElementSearchReqFilterPresent = "present";
        public const string ElementSearchReqFilterSubstr = "substrings";
        public const string ElementSearchReqFilterSubstrAny = "any";
        public const string ElementSearchReqFilterSubstrFinal = "final";
        public const string ElementSearchReqFilterSubstrInit = "initial";
        public const string HttpPostMethod = "POST";
        public const string SOAPBeginSession = "<ad:BeginSession xmlns:ad=\"urn:schema-microsoft-com:activedirectory:dsmlv2\" se:mustUnderstand=\"1\"/>";
        public const string SOAPBodyBegin = "<se:Body xmlns=\"urn:oasis:names:tc:DSML:2:0:core\">";
        public const string SOAPBodyEnd = "</se:Body>";
        public const string SOAPEndSession1 = "<ad:EndSession xmlns:ad=\"urn:schema-microsoft-com:activedirectory:dsmlv2\" ad:SessionID=\"";
        public const string SOAPEndSession2 = "\" se:mustUnderstand=\"1\"/>";
        public const string SOAPEnvelopeBegin = "<se:Envelope xmlns:se=\"http://schemas.xmlsoap.org/soap/envelope/\">";
        public const string SOAPEnvelopeEnd = "</se:Envelope>";
        public const string SOAPHeaderBegin = "<se:Header>";
        public const string SOAPHeaderEnd = "</se:Header>";
        public const string SOAPSession1 = "<ad:Session xmlns:ad=\"urn:schema-microsoft-com:activedirectory:dsmlv2\" ad:SessionID=\"";
        public const string SOAPSession2 = "\" se:mustUnderstand=\"1\"/>";
        public const string SoapUri = "http://schemas.xmlsoap.org/soap/envelope/";
        public const string XsdUri = "http://www.w3.org/2001/XMLSchema";
        public const string XsiUri = "http://www.w3.org/2001/XMLSchema-instance";

        private DsmlConstants()
        {
        }
    }
}

