namespace System.Security.Policy
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class SiteMembershipCondition : IConstantMembershipCondition, IReportMatchMembershipCondition, IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
    {
        private SecurityElement m_element;
        private SiteString m_site;

        internal SiteMembershipCondition()
        {
            this.m_site = null;
        }

        public SiteMembershipCondition(string site)
        {
            if (site == null)
            {
                throw new ArgumentNullException("site");
            }
            this.m_site = new SiteString(site);
        }

        public bool Check(Evidence evidence)
        {
            object usedEvidence = null;
            return ((IReportMatchMembershipCondition) this).Check(evidence, out usedEvidence);
        }

        public IMembershipCondition Copy()
        {
            if ((this.m_site == null) && (this.m_element != null))
            {
                this.ParseSite();
            }
            return new SiteMembershipCondition(this.m_site.ToString());
        }

        public override bool Equals(object o)
        {
            SiteMembershipCondition condition = o as SiteMembershipCondition;
            if (condition != null)
            {
                if ((this.m_site == null) && (this.m_element != null))
                {
                    this.ParseSite();
                }
                if ((condition.m_site == null) && (condition.m_element != null))
                {
                    condition.ParseSite();
                }
                if (object.Equals(this.m_site, condition.m_site))
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
                this.m_site = null;
                this.m_element = e;
            }
        }

        public override int GetHashCode()
        {
            if ((this.m_site == null) && (this.m_element != null))
            {
                this.ParseSite();
            }
            if (this.m_site != null)
            {
                return this.m_site.GetHashCode();
            }
            return typeof(SiteMembershipCondition).GetHashCode();
        }

        private void ParseSite()
        {
            lock (this)
            {
                if (this.m_element != null)
                {
                    string site = this.m_element.Attribute("Site");
                    if (site == null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_SiteCannotBeNull"));
                    }
                    this.m_site = new SiteString(site);
                    this.m_element = null;
                }
            }
        }

        bool IReportMatchMembershipCondition.Check(Evidence evidence, out object usedEvidence)
        {
            usedEvidence = null;
            if (evidence != null)
            {
                System.Security.Policy.Site hostEvidence = evidence.GetHostEvidence<System.Security.Policy.Site>();
                if (hostEvidence != null)
                {
                    if ((this.m_site == null) && (this.m_element != null))
                    {
                        this.ParseSite();
                    }
                    if (hostEvidence.GetSiteString().IsSubsetOf(this.m_site))
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
            if ((this.m_site == null) && (this.m_element != null))
            {
                this.ParseSite();
            }
            if (this.m_site != null)
            {
                return string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Site_ToStringArg"), new object[] { this.m_site });
            }
            return Environment.GetResourceString("Site_ToString");
        }

        public SecurityElement ToXml()
        {
            return this.ToXml(null);
        }

        public SecurityElement ToXml(PolicyLevel level)
        {
            if ((this.m_site == null) && (this.m_element != null))
            {
                this.ParseSite();
            }
            SecurityElement element = new SecurityElement("IMembershipCondition");
            XMLUtil.AddClassAttribute(element, base.GetType(), "System.Security.Policy.SiteMembershipCondition");
            element.AddAttribute("version", "1");
            if (this.m_site != null)
            {
                element.AddAttribute("Site", this.m_site.ToString());
            }
            return element;
        }

        public string Site
        {
            get
            {
                if ((this.m_site == null) && (this.m_element != null))
                {
                    this.ParseSite();
                }
                if (this.m_site != null)
                {
                    return this.m_site.ToString();
                }
                return "";
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_site = new SiteString(value);
            }
        }
    }
}

