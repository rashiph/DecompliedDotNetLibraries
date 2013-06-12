namespace System.Security.Principal
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public class GenericIdentity : IIdentity
    {
        private string m_name;
        private string m_type;

        public GenericIdentity(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.m_name = name;
            this.m_type = "";
        }

        public GenericIdentity(string name, string type)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.m_name = name;
            this.m_type = type;
        }

        public virtual string AuthenticationType
        {
            get
            {
                return this.m_type;
            }
        }

        public virtual bool IsAuthenticated
        {
            get
            {
                return !this.m_name.Equals("");
            }
        }

        public virtual string Name
        {
            get
            {
                return this.m_name;
            }
        }
    }
}

