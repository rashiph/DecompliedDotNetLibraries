namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class ModifyResponse : DirectoryResponse
    {
        internal ModifyResponse(XmlNode node) : base(node)
        {
        }

        internal ModifyResponse(string dn, DirectoryControl[] controls, ResultCode result, string message, Uri[] referral) : base(dn, controls, result, message, referral)
        {
        }
    }
}

