namespace System.Security.Policy
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), ComVisible(true)]
    public sealed class PermissionRequestEvidence : EvidenceBase
    {
        private PermissionSet m_denied;
        private PermissionSet m_optional;
        private PermissionSet m_request;
        private string m_strDenied;
        private string m_strOptional;
        private string m_strRequest;

        public PermissionRequestEvidence(PermissionSet request, PermissionSet optional, PermissionSet denied)
        {
            if (request == null)
            {
                this.m_request = null;
            }
            else
            {
                this.m_request = request.Copy();
            }
            if (optional == null)
            {
                this.m_optional = null;
            }
            else
            {
                this.m_optional = optional.Copy();
            }
            if (denied == null)
            {
                this.m_denied = null;
            }
            else
            {
                this.m_denied = denied.Copy();
            }
        }

        public override EvidenceBase Clone()
        {
            return this.Copy();
        }

        public PermissionRequestEvidence Copy()
        {
            return new PermissionRequestEvidence(this.m_request, this.m_optional, this.m_denied);
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            return this.ToXml().ToString();
        }

        internal SecurityElement ToXml()
        {
            SecurityElement element2;
            SecurityElement element = new SecurityElement("System.Security.Policy.PermissionRequestEvidence");
            element.AddAttribute("version", "1");
            if (this.m_request != null)
            {
                element2 = new SecurityElement("Request");
                element2.AddChild(this.m_request.ToXml());
                element.AddChild(element2);
            }
            if (this.m_optional != null)
            {
                element2 = new SecurityElement("Optional");
                element2.AddChild(this.m_optional.ToXml());
                element.AddChild(element2);
            }
            if (this.m_denied != null)
            {
                element2 = new SecurityElement("Denied");
                element2.AddChild(this.m_denied.ToXml());
                element.AddChild(element2);
            }
            return element;
        }

        public PermissionSet DeniedPermissions
        {
            get
            {
                return this.m_denied;
            }
        }

        public PermissionSet OptionalPermissions
        {
            get
            {
                return this.m_optional;
            }
        }

        public PermissionSet RequestedPermissions
        {
            get
            {
                return this.m_request;
            }
        }
    }
}

