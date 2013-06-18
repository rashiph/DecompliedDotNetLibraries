namespace System.Drawing.Printing
{
    using System;
    using System.Drawing;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public sealed class PrintingPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private PrintingPermissionLevel printingLevel;

        public PrintingPermission(PrintingPermissionLevel printingLevel)
        {
            VerifyPrintingLevel(printingLevel);
            this.printingLevel = printingLevel;
        }

        public PrintingPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.printingLevel = PrintingPermissionLevel.AllPrinting;
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(System.Drawing.SR.GetString("InvalidPermissionState"));
                }
                this.printingLevel = PrintingPermissionLevel.NoPrinting;
            }
        }

        public override IPermission Copy()
        {
            return new PrintingPermission(this.printingLevel);
        }

        public override void FromXml(SecurityElement esd)
        {
            if (esd == null)
            {
                throw new ArgumentNullException("esd");
            }
            string str = esd.Attribute("class");
            if ((str == null) || (str.IndexOf(base.GetType().FullName) == -1))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidClassName"));
            }
            string a = esd.Attribute("Unrestricted");
            if ((a != null) && string.Equals(a, "true", StringComparison.OrdinalIgnoreCase))
            {
                this.printingLevel = PrintingPermissionLevel.AllPrinting;
            }
            else
            {
                this.printingLevel = PrintingPermissionLevel.NoPrinting;
                string str3 = esd.Attribute("Level");
                if (str3 != null)
                {
                    this.printingLevel = (PrintingPermissionLevel) Enum.Parse(typeof(PrintingPermissionLevel), str3);
                }
            }
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            PrintingPermission permission = target as PrintingPermission;
            if (permission == null)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("TargetNotPrintingPermission"));
            }
            PrintingPermissionLevel printingLevel = (this.printingLevel < permission.printingLevel) ? this.printingLevel : permission.printingLevel;
            if (printingLevel == PrintingPermissionLevel.NoPrinting)
            {
                return null;
            }
            return new PrintingPermission(printingLevel);
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return (this.printingLevel == PrintingPermissionLevel.NoPrinting);
            }
            PrintingPermission permission = target as PrintingPermission;
            if (permission == null)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("TargetNotPrintingPermission"));
            }
            return (this.printingLevel <= permission.printingLevel);
        }

        public bool IsUnrestricted()
        {
            return (this.printingLevel == PrintingPermissionLevel.AllPrinting);
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("IPermission");
            element.AddAttribute("class", base.GetType().FullName + ", " + base.GetType().Module.Assembly.FullName.Replace('"', '\''));
            element.AddAttribute("version", "1");
            if (!this.IsUnrestricted())
            {
                element.AddAttribute("Level", Enum.GetName(typeof(PrintingPermissionLevel), this.printingLevel));
                return element;
            }
            element.AddAttribute("Unrestricted", "true");
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                return this.Copy();
            }
            PrintingPermission permission = target as PrintingPermission;
            if (permission == null)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("TargetNotPrintingPermission"));
            }
            PrintingPermissionLevel printingLevel = (this.printingLevel > permission.printingLevel) ? this.printingLevel : permission.printingLevel;
            if (printingLevel == PrintingPermissionLevel.NoPrinting)
            {
                return null;
            }
            return new PrintingPermission(printingLevel);
        }

        private static void VerifyPrintingLevel(PrintingPermissionLevel level)
        {
            if ((level < PrintingPermissionLevel.NoPrinting) || (level > PrintingPermissionLevel.AllPrinting))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidPermissionLevel"));
            }
        }

        public PrintingPermissionLevel Level
        {
            get
            {
                return this.printingLevel;
            }
            set
            {
                VerifyPrintingLevel(value);
                this.printingLevel = value;
            }
        }
    }
}

