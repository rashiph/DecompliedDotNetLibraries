namespace System.Security.Policy
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, ComVisible(true)]
    public sealed class GacInstalled : EvidenceBase, IIdentityPermissionFactory
    {
        public override EvidenceBase Clone()
        {
            return new GacInstalled();
        }

        public object Copy()
        {
            return this.Clone();
        }

        public IPermission CreateIdentityPermission(Evidence evidence)
        {
            return new GacIdentityPermission();
        }

        public override bool Equals(object o)
        {
            return (o is GacInstalled);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return this.ToXml().ToString();
        }

        internal SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement(base.GetType().FullName);
            element.AddAttribute("version", "1");
            return element;
        }
    }
}

