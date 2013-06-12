namespace System.Security
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ISecurityEncodable
    {
        void FromXml(SecurityElement e);
        SecurityElement ToXml();
    }
}

