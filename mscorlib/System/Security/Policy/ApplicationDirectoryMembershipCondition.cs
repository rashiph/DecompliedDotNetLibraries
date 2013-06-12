namespace System.Security.Policy
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class ApplicationDirectoryMembershipCondition : IConstantMembershipCondition, IReportMatchMembershipCondition, IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
    {
        public bool Check(Evidence evidence)
        {
            object usedEvidence = null;
            return ((IReportMatchMembershipCondition) this).Check(evidence, out usedEvidence);
        }

        public IMembershipCondition Copy()
        {
            return new ApplicationDirectoryMembershipCondition();
        }

        public override bool Equals(object o)
        {
            return (o is ApplicationDirectoryMembershipCondition);
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
            return typeof(ApplicationDirectoryMembershipCondition).GetHashCode();
        }

        bool IReportMatchMembershipCondition.Check(Evidence evidence, out object usedEvidence)
        {
            usedEvidence = null;
            if (evidence != null)
            {
                ApplicationDirectory hostEvidence = evidence.GetHostEvidence<ApplicationDirectory>();
                Url url = evidence.GetHostEvidence<Url>();
                if ((hostEvidence != null) && (url != null))
                {
                    string directory = hostEvidence.Directory;
                    if ((directory != null) && (directory.Length > 1))
                    {
                        if (directory[directory.Length - 1] == '/')
                        {
                            directory = directory + "*";
                        }
                        else
                        {
                            directory = directory + "/*";
                        }
                        URLString operand = new URLString(directory);
                        if (url.GetURLString().IsSubsetOf(operand))
                        {
                            usedEvidence = hostEvidence;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            return Environment.GetResourceString("ApplicationDirectory_ToString");
        }

        public SecurityElement ToXml()
        {
            return this.ToXml(null);
        }

        public SecurityElement ToXml(PolicyLevel level)
        {
            SecurityElement element = new SecurityElement("IMembershipCondition");
            XMLUtil.AddClassAttribute(element, base.GetType(), "System.Security.Policy.ApplicationDirectoryMembershipCondition");
            element.AddAttribute("version", "1");
            return element;
        }
    }
}

