namespace System.Security
{
    using System;
    using System.Collections;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class ReadOnlyPermissionSet : PermissionSet
    {
        [NonSerialized]
        private bool m_deserializing;
        private SecurityElement m_originXml;

        public ReadOnlyPermissionSet(SecurityElement permissionSetXml)
        {
            if (permissionSetXml == null)
            {
                throw new ArgumentNullException("permissionSetXml");
            }
            this.m_originXml = permissionSetXml.Copy();
            base.FromXml(this.m_originXml);
        }

        protected override IPermission AddPermissionImpl(IPermission perm)
        {
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ModifyROPermSet"));
        }

        public override PermissionSet Copy()
        {
            return new ReadOnlyPermissionSet(this.m_originXml);
        }

        public override void FromXml(SecurityElement et)
        {
            if (!this.m_deserializing)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ModifyROPermSet"));
            }
            base.FromXml(et);
        }

        protected override IEnumerator GetEnumeratorImpl()
        {
            return new ReadOnlyPermissionSetEnumerator(base.GetEnumeratorImpl());
        }

        protected override IPermission GetPermissionImpl(Type permClass)
        {
            IPermission permissionImpl = base.GetPermissionImpl(permClass);
            if (permissionImpl == null)
            {
                return null;
            }
            return permissionImpl.Copy();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            this.m_deserializing = false;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext ctx)
        {
            this.m_deserializing = true;
        }

        protected override IPermission RemovePermissionImpl(Type permClass)
        {
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ModifyROPermSet"));
        }

        protected override IPermission SetPermissionImpl(IPermission perm)
        {
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ModifyROPermSet"));
        }

        public override SecurityElement ToXml()
        {
            return this.m_originXml.Copy();
        }

        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }
    }
}

