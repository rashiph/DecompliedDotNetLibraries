namespace System.Transactions
{
    using System;
    using System.Globalization;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [Serializable]
    public sealed class DistributedTransactionPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private bool unrestricted;

        public DistributedTransactionPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.unrestricted = true;
            }
            else
            {
                this.unrestricted = false;
            }
        }

        public override IPermission Copy()
        {
            DistributedTransactionPermission permission = new DistributedTransactionPermission(PermissionState.None);
            if (this.IsUnrestricted())
            {
                permission.unrestricted = true;
                return permission;
            }
            permission.unrestricted = false;
            return permission;
        }

        public override void FromXml(SecurityElement securityElement)
        {
            if (securityElement == null)
            {
                throw new ArgumentNullException("securityElement");
            }
            if (!securityElement.Tag.Equals("IPermission"))
            {
                throw new ArgumentException(System.Transactions.SR.GetString("ArgumentWrongType"), "securityElement");
            }
            string str = securityElement.Attribute("Unrestricted");
            if (str != null)
            {
                this.unrestricted = Convert.ToBoolean(str, CultureInfo.InvariantCulture);
            }
            else
            {
                this.unrestricted = false;
            }
        }

        public override IPermission Intersect(IPermission target)
        {
            IPermission permission;
            try
            {
                if (target == null)
                {
                    return null;
                }
                DistributedTransactionPermission permission2 = (DistributedTransactionPermission) target;
                if (!permission2.IsUnrestricted())
                {
                    return permission2;
                }
                permission = this.Copy();
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("ArgumentWrongType"), "target");
            }
            return permission;
        }

        public override bool IsSubsetOf(IPermission target)
        {
            bool flag;
            if (target == null)
            {
                return !this.unrestricted;
            }
            try
            {
                DistributedTransactionPermission permission = (DistributedTransactionPermission) target;
                if (!this.unrestricted)
                {
                    return true;
                }
                if (permission.unrestricted)
                {
                    return true;
                }
                flag = false;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("ArgumentWrongType"), "target");
            }
            return flag;
        }

        public bool IsUnrestricted()
        {
            return this.unrestricted;
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("IPermission");
            Type type = base.GetType();
            StringBuilder builder = new StringBuilder(type.Assembly.ToString());
            builder.Replace('"', '\'');
            element.AddAttribute("class", type.FullName + ", " + builder);
            element.AddAttribute("version", "1");
            element.AddAttribute("Unrestricted", this.unrestricted.ToString());
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            IPermission permission;
            try
            {
                if (target == null)
                {
                    return this.Copy();
                }
                DistributedTransactionPermission permission2 = (DistributedTransactionPermission) target;
                if (permission2.IsUnrestricted())
                {
                    return permission2;
                }
                permission = this.Copy();
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("ArgumentWrongType"), "target");
            }
            return permission;
        }
    }
}

