namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class ModifyDNResponse : DirectoryResponse
    {
        internal ModifyDNResponse(XmlNode node) : base(node)
        {
        }

        internal ModifyDNResponse(string dn, DirectoryControl[] controls, ResultCode result, string message, Uri[] referral) : base(dn, controls, result, message, referral)
        {
        }
    }
}

