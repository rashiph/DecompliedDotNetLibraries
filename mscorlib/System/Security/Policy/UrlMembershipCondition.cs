namespace System.Security.Policy
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class UrlMembershipCondition : IConstantMembershipCondition, IReportMatchMembershipCondition, IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
    {
        private SecurityElement m_element;
        private URLString m_url;

        internal UrlMembershipCondition()
        {
            this.m_url = null;
        }

        public UrlMembershipCondition(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            this.m_url = new URLString(url, false, true);
            if (this.m_url.IsRelativeFileUrl)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_RelativeUrlMembershipCondition"), "url");
            }
        }

        public bool Check(Evidence evidence)
        {
            object usedEvidence = null;
            return ((IReportMatchMembershipCondition) this).Check(evidence, out usedEvidence);
        }

        public IMembershipCondition Copy()
        {
            if ((this.m_url == null) && (this.m_element != null))
            {
                this.ParseURL();
            }
            return new UrlMembershipCondition { m_url = new URLString(this.m_url.ToString()) };
        }

        public override bool Equals(object o)
        {
            UrlMembershipCondition condition = o as UrlMembershipCondition;
            if (condition != null)
            {
                if ((this.m_url == null) && (this.m_element != null))
                {
                    this.ParseURL();
                }
                if ((condition.m_url == null) && (condition.m_element != null))
                {
                    condition.ParseURL();
                }
                if (object.Equals(this.m_url, condition.m_url))
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
                this.m_element = e;
                this.m_url = null;
            }
        }

        public override int GetHashCode()
        {
            if ((this.m_url == null) && (this.m_element != null))
            {
                this.ParseURL();
            }
            if (this.m_url != null)
            {
                return this.m_url.GetHashCode();
            }
            return typeof(UrlMembershipCondition).GetHashCode();
        }

        private void ParseURL()
        {
            lock (this)
            {
                if (this.m_element != null)
                {
                    string url = this.m_element.Attribute("Url");
                    if (url == null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_UrlCannotBeNull"));
                    }
                    URLString str2 = new URLString(url);
                    if (str2.IsRelativeFileUrl)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_RelativeUrlMembershipCondition"));
                    }
                    this.m_url = str2;
                    this.m_element = null;
                }
            }
        }

        bool IReportMatchMembershipCondition.Check(Evidence evidence, out object usedEvidence)
        {
            usedEvidence = null;
            if (evidence != null)
            {
                System.Security.Policy.Url hostEvidence = evidence.GetHostEvidence<System.Security.Policy.Url>();
                if (hostEvidence != null)
                {
                    if ((this.m_url == null) && (this.m_element != null))
                    {
                        this.ParseURL();
                    }
                    if (hostEvidence.GetURLString().IsSubsetOf(this.m_url))
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
            if ((this.m_url == null) && (this.m_element != null))
            {
                this.ParseURL();
            }
            if (this.m_url != null)
            {
                return string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Url_ToStringArg"), new object[] { this.m_url.ToString() });
            }
            return Environment.GetResourceString("Url_ToString");
        }

        public SecurityElement ToXml()
        {
            return this.ToXml(null);
        }

        public SecurityElement ToXml(PolicyLevel level)
        {
            if ((this.m_url == null) && (this.m_element != null))
            {
                this.ParseURL();
            }
            SecurityElement element = new SecurityElement("IMembershipCondition");
            XMLUtil.AddClassAttribute(element, base.GetType(), "System.Security.Policy.UrlMembershipCondition");
            element.AddAttribute("version", "1");
            if (this.m_url != null)
            {
                element.AddAttribute("Url", this.m_url.ToString());
            }
            return element;
        }

        public string Url
        {
            get
            {
                if ((this.m_url == null) && (this.m_element != null))
                {
                    this.ParseURL();
                }
                return this.m_url.ToString();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                URLString str = new URLString(value);
                if (str.IsRelativeFileUrl)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_RelativeUrlMembershipCondition"), "value");
                }
                this.m_url = str;
            }
        }
    }
}

