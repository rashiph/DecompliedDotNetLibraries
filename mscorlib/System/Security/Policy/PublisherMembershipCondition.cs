namespace System.Security.Policy
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class PublisherMembershipCondition : IConstantMembershipCondition, IReportMatchMembershipCondition, IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
    {
        private X509Certificate m_certificate;
        private SecurityElement m_element;

        internal PublisherMembershipCondition()
        {
            this.m_element = null;
            this.m_certificate = null;
        }

        public PublisherMembershipCondition(X509Certificate certificate)
        {
            CheckCertificate(certificate);
            this.m_certificate = new X509Certificate(certificate);
        }

        public bool Check(Evidence evidence)
        {
            object usedEvidence = null;
            return ((IReportMatchMembershipCondition) this).Check(evidence, out usedEvidence);
        }

        private static void CheckCertificate(X509Certificate certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }
        }

        public IMembershipCondition Copy()
        {
            if ((this.m_certificate == null) && (this.m_element != null))
            {
                this.ParseCertificate();
            }
            return new PublisherMembershipCondition(this.m_certificate);
        }

        public override bool Equals(object o)
        {
            PublisherMembershipCondition condition = o as PublisherMembershipCondition;
            if (condition != null)
            {
                if ((this.m_certificate == null) && (this.m_element != null))
                {
                    this.ParseCertificate();
                }
                if ((condition.m_certificate == null) && (condition.m_element != null))
                {
                    condition.ParseCertificate();
                }
                if (Publisher.PublicKeyEquals(this.m_certificate, condition.m_certificate))
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
                this.m_certificate = null;
            }
        }

        public override int GetHashCode()
        {
            if ((this.m_certificate == null) && (this.m_element != null))
            {
                this.ParseCertificate();
            }
            if (this.m_certificate != null)
            {
                return this.m_certificate.GetHashCode();
            }
            return typeof(PublisherMembershipCondition).GetHashCode();
        }

        private void ParseCertificate()
        {
            lock (this)
            {
                if (this.m_element != null)
                {
                    string hexString = this.m_element.Attribute("X509Certificate");
                    this.m_certificate = (hexString == null) ? null : new X509Certificate(Hex.DecodeHexString(hexString));
                    CheckCertificate(this.m_certificate);
                    this.m_element = null;
                }
            }
        }

        bool IReportMatchMembershipCondition.Check(Evidence evidence, out object usedEvidence)
        {
            usedEvidence = null;
            if (evidence != null)
            {
                Publisher hostEvidence = evidence.GetHostEvidence<Publisher>();
                if (hostEvidence != null)
                {
                    if ((this.m_certificate == null) && (this.m_element != null))
                    {
                        this.ParseCertificate();
                    }
                    if (hostEvidence.Equals(new Publisher(this.m_certificate)))
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
            if ((this.m_certificate == null) && (this.m_element != null))
            {
                this.ParseCertificate();
            }
            if ((this.m_certificate != null) && (this.m_certificate.Subject != null))
            {
                return string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Publisher_ToStringArg"), new object[] { Hex.EncodeHexString(this.m_certificate.GetPublicKey()) });
            }
            return Environment.GetResourceString("Publisher_ToString");
        }

        public SecurityElement ToXml()
        {
            return this.ToXml(null);
        }

        public SecurityElement ToXml(PolicyLevel level)
        {
            if ((this.m_certificate == null) && (this.m_element != null))
            {
                this.ParseCertificate();
            }
            SecurityElement element = new SecurityElement("IMembershipCondition");
            XMLUtil.AddClassAttribute(element, base.GetType(), "System.Security.Policy.PublisherMembershipCondition");
            element.AddAttribute("version", "1");
            if (this.m_certificate != null)
            {
                element.AddAttribute("X509Certificate", this.m_certificate.GetRawCertDataString());
            }
            return element;
        }

        public X509Certificate Certificate
        {
            get
            {
                if ((this.m_certificate == null) && (this.m_element != null))
                {
                    this.ParseCertificate();
                }
                if (this.m_certificate != null)
                {
                    return new X509Certificate(this.m_certificate);
                }
                return null;
            }
            set
            {
                CheckCertificate(value);
                this.m_certificate = new X509Certificate(value);
            }
        }
    }
}

