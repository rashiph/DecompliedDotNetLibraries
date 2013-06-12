namespace System.Drawing.Printing
{
    using System;
    using System.Drawing;
    using System.Security;
    using System.Security.Permissions;

    [AttributeUsage(AttributeTargets.All, AllowMultiple=true)]
    public sealed class PrintingPermissionAttribute : CodeAccessSecurityAttribute
    {
        private PrintingPermissionLevel level;

        public PrintingPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            if (base.Unrestricted)
            {
                return new PrintingPermission(PermissionState.Unrestricted);
            }
            return new PrintingPermission(this.level);
        }

        public PrintingPermissionLevel Level
        {
            get
            {
                return this.level;
            }
            set
            {
                if ((value < PrintingPermissionLevel.NoPrinting) || (value > PrintingPermissionLevel.AllPrinting))
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("PrintingPermissionAttributeInvalidPermissionLevel"), "value");
                }
                this.level = value;
            }
        }
    }
}

