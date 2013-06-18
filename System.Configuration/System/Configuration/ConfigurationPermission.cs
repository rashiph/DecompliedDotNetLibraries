namespace System.Configuration
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public sealed class ConfigurationPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private PermissionState _permissionState;

        public ConfigurationPermission(PermissionState state)
        {
            switch (state)
            {
                case PermissionState.None:
                case PermissionState.Unrestricted:
                    this._permissionState = state;
                    return;
            }
            throw ExceptionUtil.ParameterInvalid("state");
        }

        public override IPermission Copy()
        {
            return new ConfigurationPermission(this._permissionState);
        }

        public override void FromXml(SecurityElement securityElement)
        {
            if (securityElement == null)
            {
                throw new ArgumentNullException(System.Configuration.SR.GetString("ConfigurationPermissionBadXml", new object[] { "securityElement" }));
            }
            if (!securityElement.Tag.Equals("IPermission"))
            {
                throw new ArgumentException(System.Configuration.SR.GetString("ConfigurationPermissionBadXml", new object[] { "securityElement" }));
            }
            string str = securityElement.Attribute("class");
            if (str == null)
            {
                throw new ArgumentException(System.Configuration.SR.GetString("ConfigurationPermissionBadXml", new object[] { "securityElement" }));
            }
            if (str.IndexOf(base.GetType().FullName, StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException(System.Configuration.SR.GetString("ConfigurationPermissionBadXml", new object[] { "securityElement" }));
            }
            if (securityElement.Attribute("version") != "1")
            {
                throw new ArgumentException(System.Configuration.SR.GetString("ConfigurationPermissionBadXml", new object[] { "version" }));
            }
            string str3 = securityElement.Attribute("Unrestricted");
            if (str3 == null)
            {
                this._permissionState = PermissionState.None;
            }
            else
            {
                switch (str3)
                {
                    case "true":
                        this._permissionState = PermissionState.Unrestricted;
                        return;

                    case "false":
                        this._permissionState = PermissionState.None;
                        return;
                }
                throw new ArgumentException(System.Configuration.SR.GetString("ConfigurationPermissionBadXml", new object[] { "Unrestricted" }));
            }
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            if (target.GetType() != typeof(ConfigurationPermission))
            {
                throw ExceptionUtil.ParameterInvalid("target");
            }
            if (this._permissionState == PermissionState.None)
            {
                return new ConfigurationPermission(PermissionState.None);
            }
            ConfigurationPermission permission = (ConfigurationPermission) target;
            return new ConfigurationPermission(permission._permissionState);
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return (this._permissionState == PermissionState.None);
            }
            if (target.GetType() != typeof(ConfigurationPermission))
            {
                throw ExceptionUtil.ParameterInvalid("target");
            }
            ConfigurationPermission permission = (ConfigurationPermission) target;
            if (this._permissionState != PermissionState.None)
            {
                return (permission._permissionState == PermissionState.Unrestricted);
            }
            return true;
        }

        public bool IsUnrestricted()
        {
            return (this._permissionState == PermissionState.Unrestricted);
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("IPermission");
            element.AddAttribute("class", base.GetType().FullName + ", " + base.GetType().Module.Assembly.FullName.Replace('"', '\''));
            element.AddAttribute("version", "1");
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
            if (target.GetType() != typeof(ConfigurationPermission))
            {
                throw ExceptionUtil.ParameterInvalid("target");
            }
            if (this._permissionState == PermissionState.Unrestricted)
            {
                return new ConfigurationPermission(PermissionState.Unrestricted);
            }
            ConfigurationPermission permission = (ConfigurationPermission) target;
            return new ConfigurationPermission(permission._permissionState);
        }
    }
}

