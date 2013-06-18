namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class CompareResponse : DirectoryResponse
    {
        internal CompareResponse(XmlNode node) : base(node)
        {
        }

        internal CompareResponse(string dn, DirectoryControl[] controls, ResultCode result, string message, Uri[] referral) : base(dn, controls, result, message, referral)
        {
        }
    }
}

