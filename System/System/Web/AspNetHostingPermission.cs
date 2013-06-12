namespace System.Web
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public sealed class AspNetHostingPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private AspNetHostingPermissionLevel _level;

        public AspNetHostingPermission(PermissionState state)
        {
            switch (state)
            {
                case PermissionState.None:
                    this._level = AspNetHostingPermissionLevel.None;
                    return;

                case PermissionState.Unrestricted:
                    this._level = AspNetHostingPermissionLevel.Unrestricted;
                    return;
            }
            throw new ArgumentException(SR.GetString("InvalidArgument", new object[] { state.ToString(), "state" }));
        }

        public AspNetHostingPermission(AspNetHostingPermissionLevel level)
        {
            VerifyAspNetHostingPermissionLevel(level, "level");
            this._level = level;
        }

        public override IPermission Copy()
        {
            return new AspNetHostingPermission(this._level);
        }

        public override void FromXml(SecurityElement securityElement)
        {
            if (securityElement == null)
            {
                throw new ArgumentNullException(SR.GetString("AspNetHostingPermissionBadXml", new object[] { "securityElement" }));
            }
            if (!securityElement.Tag.Equals("IPermission"))
            {
                throw new ArgumentException(SR.GetString("AspNetHostingPermissionBadXml", new object[] { "securityElement" }));
            }
            string str = securityElement.Attribute("class");
            if (str == null)
            {
                throw new ArgumentException(SR.GetString("AspNetHostingPermissionBadXml", new object[] { "securityElement" }));
            }
            if (str.IndexOf(base.GetType().FullName, StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException(SR.GetString("AspNetHostingPermissionBadXml", new object[] { "securityElement" }));
            }
            if (string.Compare(securityElement.Attribute("version"), "1", StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new ArgumentException(SR.GetString("AspNetHostingPermissionBadXml", new object[] { "version" }));
            }
            string str3 = securityElement.Attribute("Level");
            if (str3 == null)
            {
                this._level = AspNetHostingPermissionLevel.None;
            }
            else
            {
                this._level = (AspNetHostingPermissionLevel) Enum.Parse(typeof(AspNetHostingPermissionLevel), str3);
            }
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            if (target.GetType() != typeof(AspNetHostingPermission))
            {
                throw new ArgumentException(SR.GetString("InvalidArgument", new object[] { (target == null) ? "null" : target.ToString(), "target" }));
            }
            AspNetHostingPermission permission = (AspNetHostingPermission) target;
            if (this.Level <= permission.Level)
            {
                return new AspNetHostingPermission(this.Level);
            }
            return new AspNetHostingPermission(permission.Level);
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return (this._level == AspNetHostingPermissionLevel.None);
            }
            if (target.GetType() != typeof(AspNetHostingPermission))
            {
                throw new ArgumentException(SR.GetString("InvalidArgument", new object[] { (target == null) ? "null" : target.ToString(), "target" }));
            }
            AspNetHostingPermission permission = (AspNetHostingPermission) target;
            return (this.Level <= permission.Level);
        }

        public bool IsUnrestricted()
        {
            return (this._level == AspNetHostingPermissionLevel.Unrestricted);
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("IPermission");
            element.AddAttribute("class", base.GetType().FullName + ", " + base.GetType().Module.Assembly.FullName.Replace('"', '\''));
            element.AddAttribute("version", "1");
            element.AddAttribute("Level", Enum.GetName(typeof(AspNetHostingPermissionLevel), this._level));
            if (this.IsUnrestricted())
            {
                element.AddAttribute("Unrestricted", "true");
            }
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                return this.Copy();
            }
            if (target.GetType() != typeof(AspNetHostingPermission))
            {
                throw new ArgumentException(SR.GetString("InvalidArgument", new object[] { (target == null) ? "null" : target.ToString(), "target" }));
            }
            AspNetHostingPermission permission = (AspNetHostingPermission) target;
            if (this.Level >= permission.Level)
            {
                return new AspNetHostingPermission(this.Level);
            }
            return new AspNetHostingPermission(permission.Level);
        }

        internal static void VerifyAspNetHostingPermissionLevel(AspNetHostingPermissionLevel level, string arg)
        {
            switch (level)
            {
                case AspNetHostingPermissionLevel.None:
                case AspNetHostingPermissionLevel.Minimal:
                case AspNetHostingPermissionLevel.Low:
                case AspNetHostingPermissionLevel.Medium:
                case AspNetHostingPermissionLevel.High:
                case AspNetHostingPermissionLevel.Unrestricted:
                    return;
            }
            throw new ArgumentException(arg);
        }

        public AspNetHostingPermissionLevel Level
        {
            get
            {
                return this._level;
            }
            set
            {
                VerifyAspNetHostingPermissionLevel(value, "Level");
                this._level = value;
            }
        }
    }
}

