namespace System
{
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(false)]
    public sealed class ApplicationIdentity : ISerializable
    {
        private IDefinitionAppId _appId;

        private ApplicationIdentity()
        {
        }

        [SecurityCritical]
        internal ApplicationIdentity(IDefinitionAppId applicationIdentity)
        {
            this._appId = applicationIdentity;
        }

        [SecuritySafeCritical]
        public ApplicationIdentity(string applicationIdentityFullName)
        {
            if (applicationIdentityFullName == null)
            {
                throw new ArgumentNullException("applicationIdentityFullName");
            }
            this._appId = IsolationInterop.AppIdAuthority.TextToDefinition(0, applicationIdentityFullName);
        }

        [SecurityCritical]
        private ApplicationIdentity(SerializationInfo info, StreamingContext context)
        {
            string identity = (string) info.GetValue("FullName", typeof(string));
            if (identity == null)
            {
                throw new ArgumentNullException("fullName");
            }
            this._appId = IsolationInterop.AppIdAuthority.TextToDefinition(0, identity);
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FullName", this.FullName, typeof(string));
        }

        public override string ToString()
        {
            return this.FullName;
        }

        public string CodeBase
        {
            [SecuritySafeCritical]
            get
            {
                return this._appId.get_Codebase();
            }
        }

        public string FullName
        {
            [SecuritySafeCritical]
            get
            {
                return IsolationInterop.AppIdAuthority.DefinitionToText(0, this._appId);
            }
        }

        internal IDefinitionAppId Identity
        {
            [SecurityCritical]
            get
            {
                return this._appId;
            }
        }
    }
}

