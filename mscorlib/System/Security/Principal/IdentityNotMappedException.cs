namespace System.Security.Principal
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(false)]
    public sealed class IdentityNotMappedException : SystemException
    {
        private IdentityReferenceCollection unmappedIdentities;

        public IdentityNotMappedException() : base(Environment.GetResourceString("IdentityReference_IdentityNotMapped"))
        {
        }

        public IdentityNotMappedException(string message) : base(message)
        {
        }

        internal IdentityNotMappedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IdentityNotMappedException(string message, Exception inner) : base(message, inner)
        {
        }

        internal IdentityNotMappedException(string message, IdentityReferenceCollection unmappedIdentities) : this(message)
        {
            this.unmappedIdentities = unmappedIdentities;
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
        }

        public IdentityReferenceCollection UnmappedIdentities
        {
            get
            {
                if (this.unmappedIdentities == null)
                {
                    this.unmappedIdentities = new IdentityReferenceCollection();
                }
                return this.unmappedIdentities;
            }
        }
    }
}

