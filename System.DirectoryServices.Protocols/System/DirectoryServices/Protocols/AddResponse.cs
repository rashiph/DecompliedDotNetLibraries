namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class AddResponse : DirectoryResponse
    {
        internal AddResponse(XmlNode node) : base(node)
        {
        }

        internal AddResponse(string dn, DirectoryControl[] controls, ResultCode result, string message, Uri[] referral) : base(dn, controls, result, message, referral)
        {
        }
    }
}

