namespace System.Security.Permissions
{
    using System;
    using System.Security.Policy;

    [Serializable]
    internal sealed class StrongName2
    {
        public string m_name;
        public StrongNamePublicKeyBlob m_publicKeyBlob;
        public Version m_version;

        public StrongName2(StrongNamePublicKeyBlob publicKeyBlob, string name, Version version)
        {
            this.m_publicKeyBlob = publicKeyBlob;
            this.m_name = name;
            this.m_version = version;
        }

        public StrongName2 Copy()
        {
            return new StrongName2(this.m_publicKeyBlob, this.m_name, this.m_version);
        }

        public bool Equals(StrongName2 target)
        {
            if (!target.IsSubsetOf(this))
            {
                return false;
            }
            if (!this.IsSubsetOf(target))
            {
                return false;
            }
            return true;
        }

        public StrongName2 Intersect(StrongName2 target)
        {
            if (target.IsSubsetOf(this))
            {
                return target.Copy();
            }
            if (this.IsSubsetOf(target))
            {
                return this.Copy();
            }
            return null;
        }

        public bool IsSubsetOf(StrongName2 target)
        {
            if (this.m_publicKeyBlob == null)
            {
                return true;
            }
            if (!this.m_publicKeyBlob.Equals(target.m_publicKeyBlob))
            {
                return false;
            }
            if ((this.m_name != null) && ((target.m_name == null) || !StrongName.CompareNames(target.m_name, this.m_name)))
            {
                return false;
            }
            return ((this.m_version == null) || ((target.m_version != null) && (target.m_version.CompareTo(this.m_version) == 0)));
        }
    }
}

