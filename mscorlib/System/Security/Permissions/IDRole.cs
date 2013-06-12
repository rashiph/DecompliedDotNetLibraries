namespace System.Security.Permissions
{
    using System;
    using System.Security;
    using System.Security.Principal;

    [Serializable]
    internal class IDRole
    {
        internal bool m_authenticated;
        internal string m_id;
        internal string m_role;
        [NonSerialized]
        private SecurityIdentifier m_sid;

        internal void FromXml(SecurityElement e)
        {
            string strA = e.Attribute("Authenticated");
            if (strA != null)
            {
                this.m_authenticated = string.Compare(strA, "true", StringComparison.OrdinalIgnoreCase) == 0;
            }
            else
            {
                this.m_authenticated = false;
            }
            string str2 = e.Attribute("ID");
            if (str2 != null)
            {
                this.m_id = str2;
            }
            else
            {
                this.m_id = null;
            }
            string str3 = e.Attribute("Role");
            if (str3 != null)
            {
                this.m_role = str3;
            }
            else
            {
                this.m_role = null;
            }
        }

        public override int GetHashCode()
        {
            return (((this.m_authenticated ? 0 : 0x65) + ((this.m_id == null) ? 0 : this.m_id.GetHashCode())) + ((this.m_role == null) ? 0 : this.m_role.GetHashCode()));
        }

        internal SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("Identity");
            if (this.m_authenticated)
            {
                element.AddAttribute("Authenticated", "true");
            }
            if (this.m_id != null)
            {
                element.AddAttribute("ID", SecurityElement.Escape(this.m_id));
            }
            if (this.m_role != null)
            {
                element.AddAttribute("Role", SecurityElement.Escape(this.m_role));
            }
            return element;
        }

        internal SecurityIdentifier Sid
        {
            [SecurityCritical]
            get
            {
                if (string.IsNullOrEmpty(this.m_role))
                {
                    return null;
                }
                if (this.m_sid == null)
                {
                    NTAccount account = new NTAccount(this.m_role);
                    IdentityReferenceCollection references2 = NTAccount.Translate(new IdentityReferenceCollection(1) { account }, typeof(SecurityIdentifier), false);
                    this.m_sid = references2[0] as SecurityIdentifier;
                }
                return this.m_sid;
            }
        }
    }
}

