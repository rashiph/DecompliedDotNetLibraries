namespace System.Security.Policy
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class AllMembershipCondition : IConstantMembershipCondition, IReportMatchMembershipCondition, IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
    {
        public bool Check(Evidence evidence)
        {
            object usedEvidence = null;
            return ((IReportMatchMembershipCondition) this).Check(evidence, out usedEvidence);
        }

        public IMembershipCondition Copy()
        {
            return new AllMembershipCondition();
        }

        public override bool Equals(object o)
        {
            return (o is AllMembershipCondition);
        }

        public void FromXml(SecurityElement e)
        {
            this.FromXml(e, null);
        }

        public void FromXml(SecurityElement e, PolicyLevel level)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (!e.Tag.Equals("IMembershipCondition"))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MembershipConditionElement"));
            }
        }

        public override int GetHashCode()
        {
            return typeof(AllMembershipCondition).GetHashCode();
        }

        bool IReportMatchMembershipCondition.Check(Evidence evidence, out object usedEvidence)
        {
            usedEvidence = null;
            return true;
        }

        public override string ToString()
        {
            return Environment.GetResourceString("All_ToString");
        }

        public SecurityElement ToXml()
        {
            return this.ToXml(null);
        }

        public SecurityElement ToXml(PolicyLevel level)
        {
            SecurityElement element = new SecurityElement("IMembershipCondition");
            XMLUtil.AddClassAttribute(element, base.GetType(), "System.Security.Policy.AllMembershipCondition");
            element.AddAttribute("version", "1");
            return element;
        }
    }
}

