namespace System.Security.Principal
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public class GenericPrincipal : IPrincipal
    {
        private IIdentity m_identity;
        private string[] m_roles;

        public GenericPrincipal(IIdentity identity, string[] roles)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            this.m_identity = identity;
            if (roles != null)
            {
                this.m_roles = new string[roles.Length];
                for (int i = 0; i < roles.Length; i++)
                {
                    this.m_roles[i] = roles[i];
                }
            }
            else
            {
                this.m_roles = null;
            }
        }

        public virtual bool IsInRole(string role)
        {
            if ((role != null) && (this.m_roles != null))
            {
                for (int i = 0; i < this.m_roles.Length; i++)
                {
                    if ((this.m_roles[i] != null) && (string.Compare(this.m_roles[i], role, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual IIdentity Identity
        {
            get
            {
                return this.m_identity;
            }
        }
    }
}

