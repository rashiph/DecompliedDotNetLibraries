namespace System.Security.Policy
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    internal interface IReportMatchMembershipCondition : IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
    {
        bool Check(Evidence evidence, out object usedEvidence);
    }
}

