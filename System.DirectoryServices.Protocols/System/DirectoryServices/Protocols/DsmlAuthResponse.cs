namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class DsmlAuthResponse : DirectoryResponse
    {
        internal DsmlAuthResponse(XmlNode node) : base(node)
        {
        }
    }
}

