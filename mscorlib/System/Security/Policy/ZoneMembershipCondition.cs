namespace System.Security.Policy
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class ZoneMembershipCondition : IConstantMembershipCondition, IReportMatchMembershipCondition, IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
    {
        private SecurityElement m_element;
        private System.Security.SecurityZone m_zone;
        private static readonly string[] s_names = new string[] { "MyComputer", "Intranet", "Trusted", "Internet", "Untrusted" };

        internal ZoneMembershipCondition()
        {
            this.m_zone = System.Security.SecurityZone.NoZone;
        }

        public ZoneMembershipCondition(System.Security.SecurityZone zone)
        {
            VerifyZone(zone);
            this.SecurityZone = zone;
        }

        public bool Check(Evidence evidence)
        {
            object usedEvidence = null;
            return ((IReportMatchMembershipCondition) this).Check(evidence, out usedEvidence);
        }

        public IMembershipCondition Copy()
        {
            if ((this.m_zone == System.Security.SecurityZone.NoZone) && (this.m_element != null))
            {
                this.ParseZone();
            }
            return new ZoneMembershipCondition(this.m_zone);
        }

        public override bool Equals(object o)
        {
            ZoneMembershipCondition condition = o as ZoneMembershipCondition;
            if (condition != null)
            {
                if ((this.m_zone == System.Security.SecurityZone.NoZone) && (this.m_element != null))
                {
                    this.ParseZone();
                }
                if ((condition.m_zone == System.Security.SecurityZone.NoZone) && (condition.m_element != null))
                {
                    condition.ParseZone();
                }
                if (this.m_zone == condition.m_zone)
                {
                    return true;
                }
            }
            return false;
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
            lock (this)
            {
                this.m_zone = System.Security.SecurityZone.NoZone;
                this.m_element = e;
            }
        }

        public override int GetHashCode()
        {
            if ((this.m_zone == System.Security.SecurityZone.NoZone) && (this.m_element != null))
            {
                this.ParseZone();
            }
            return (int) this.m_zone;
        }

        private void ParseZone()
        {
            lock (this)
            {
                if (this.m_element != null)
                {
                    string str = this.m_element.Attribute("Zone");
                    this.m_zone = System.Security.SecurityZone.NoZone;
                    if (str == null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_ZoneCannotBeNull"));
                    }
                    this.m_zone = (System.Security.SecurityZone) Enum.Parse(typeof(System.Security.SecurityZone), str);
                    VerifyZone(this.m_zone);
                    this.m_element = null;
                }
            }
        }

        bool IReportMatchMembershipCondition.Check(Evidence evidence, out object usedEvidence)
        {
            usedEvidence = null;
            if (evidence != null)
            {
                Zone hostEvidence = evidence.GetHostEvidence<Zone>();
                if (hostEvidence != null)
                {
                    if ((this.m_zone == System.Security.SecurityZone.NoZone) && (this.m_element != null))
                    {
                        this.ParseZone();
                    }
                    if (hostEvidence.SecurityZone == this.m_zone)
                    {
                        usedEvidence = hostEvidence;
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            if ((this.m_zone == System.Security.SecurityZone.NoZone) && (this.m_element != null))
            {
                this.ParseZone();
            }
            return string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Zone_ToString"), new object[] { s_names[(int) this.m_zone] });
        }

        public SecurityElement ToXml()
        {
            return this.ToXml(null);
        }

        public SecurityElement ToXml(PolicyLevel level)
        {
            if ((this.m_zone == System.Security.SecurityZone.NoZone) && (this.m_element != null))
            {
                this.ParseZone();
            }
            SecurityElement element = new SecurityElement("IMembershipCondition");
            XMLUtil.AddClassAttribute(element, base.GetType(), "System.Security.Policy.ZoneMembershipCondition");
            element.AddAttribute("version", "1");
            if (this.m_zone != System.Security.SecurityZone.NoZone)
            {
                element.AddAttribute("Zone", Enum.GetName(typeof(System.Security.SecurityZone), this.m_zone));
            }
            return element;
        }

        private static void VerifyZone(System.Security.SecurityZone zone)
        {
            if ((zone < System.Security.SecurityZone.MyComputer) || (zone > System.Security.SecurityZone.Untrusted))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IllegalZone"));
            }
        }

        public System.Security.SecurityZone SecurityZone
        {
            get
            {
                if ((this.m_zone == System.Security.SecurityZone.NoZone) && (this.m_element != null))
                {
                    this.ParseZone();
                }
                return this.m_zone;
            }
            set
            {
                VerifyZone(value);
                this.m_zone = value;
            }
        }
    }
}

