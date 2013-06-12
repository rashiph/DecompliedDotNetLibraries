namespace System.Security
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Policy;

    [ComVisible(true)]
    public interface ISecurityPolicyEncodable
    {
        void FromXml(SecurityElement e, PolicyLevel level);
        SecurityElement ToXml(PolicyLevel level);
    }
}

