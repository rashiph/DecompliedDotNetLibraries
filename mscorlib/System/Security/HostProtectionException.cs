namespace System.Security
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    [Serializable, ComVisible(true)]
    public class HostProtectionException : SystemException
    {
        private const string DemandedResourcesName = "DemandedResources";
        private HostProtectionResource m_demanded;
        private HostProtectionResource m_protected;
        private const string ProtectedResourcesName = "ProtectedResources";

        public HostProtectionException()
        {
            this.m_protected = HostProtectionResource.None;
            this.m_demanded = HostProtectionResource.None;
        }

        public HostProtectionException(string message) : base(message)
        {
            this.m_protected = HostProtectionResource.None;
            this.m_demanded = HostProtectionResource.None;
        }

        [SecuritySafeCritical]
        protected HostProtectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.m_protected = (HostProtectionResource) info.GetValue("ProtectedResources", typeof(HostProtectionResource));
            this.m_demanded = (HostProtectionResource) info.GetValue("DemandedResources", typeof(HostProtectionResource));
        }

        private HostProtectionException(HostProtectionResource protectedResources, HostProtectionResource demandedResources) : base(SecurityException.GetResString("HostProtection_HostProtection"))
        {
            base.SetErrorCode(-2146232768);
            this.m_protected = protectedResources;
            this.m_demanded = demandedResources;
        }

        public HostProtectionException(string message, Exception e) : base(message, e)
        {
            this.m_protected = HostProtectionResource.None;
            this.m_demanded = HostProtectionResource.None;
        }

        public HostProtectionException(string message, HostProtectionResource protectedResources, HostProtectionResource demandedResources) : base(message)
        {
            base.SetErrorCode(-2146232768);
            this.m_protected = protectedResources;
            this.m_demanded = demandedResources;
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("ProtectedResources", this.ProtectedResources, typeof(HostProtectionResource));
            info.AddValue("DemandedResources", this.DemandedResources, typeof(HostProtectionResource));
        }

        public override string ToString()
        {
            string str = this.ToStringHelper("HostProtection_ProtectedResources", this.ProtectedResources);
            StringBuilder builder = new StringBuilder();
            builder.Append(base.ToString());
            builder.Append(str);
            builder.Append(this.ToStringHelper("HostProtection_DemandedResources", this.DemandedResources));
            return builder.ToString();
        }

        private string ToStringHelper(string resourceString, object attr)
        {
            if (attr == null)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(Environment.NewLine);
            builder.Append(Environment.NewLine);
            builder.Append(Environment.GetResourceString(resourceString));
            builder.Append(Environment.NewLine);
            builder.Append(attr);
            return builder.ToString();
        }

        public HostProtectionResource DemandedResources
        {
            get
            {
                return this.m_demanded;
            }
        }

        public HostProtectionResource ProtectedResources
        {
            get
            {
                return this.m_protected;
            }
        }
    }
}

