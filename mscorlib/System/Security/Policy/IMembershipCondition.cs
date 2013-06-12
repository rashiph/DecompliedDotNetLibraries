namespace System.Security.Policy
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IMembershipCondition : ISecurityEncodable, ISecurityPolicyEncodable
    {
        bool Check(Evidence evidence);
        IMembershipCondition Copy();
        bool Equals(object obj);
        string ToString();
    }
}

