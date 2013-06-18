namespace System.Transactions
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [AttributeUsage(AttributeTargets.All, AllowMultiple=true)]
    public sealed class DistributedTransactionPermissionAttribute : CodeAccessSecurityAttribute
    {
        private bool unrestricted;

        public DistributedTransactionPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            if (this.Unrestricted)
            {
                return new DistributedTransactionPermission(PermissionState.Unrestricted);
            }
            return new DistributedTransactionPermission(PermissionState.None);
        }

        public bool Unrestricted
        {
            get
            {
                return this.unrestricted;
            }
            set
            {
                this.unrestricted = value;
            }
        }
    }
}

