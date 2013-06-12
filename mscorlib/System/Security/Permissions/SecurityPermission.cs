namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class SecurityPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission
    {
        private const string _strHeaderAssertion = "Assertion";
        private const string _strHeaderControlAppDomain = "ControlAppDomain";
        private const string _strHeaderControlDomainPolicy = "ControlDomainPolicy";
        private const string _strHeaderControlEvidence = "ControlEvidence";
        private const string _strHeaderControlPolicy = "ControlPolicy";
        private const string _strHeaderControlPrincipal = "ControlPrincipal";
        private const string _strHeaderControlThread = "ControlThread";
        private const string _strHeaderExecution = "Execution";
        private const string _strHeaderSerializationFormatter = "SerializationFormatter";
        private const string _strHeaderSkipVerification = "SkipVerification";
        private const string _strHeaderUnmanagedCode = "UnmanagedCode";
        private SecurityPermissionFlag m_flags;

        public SecurityPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.SetUnrestricted(true);
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPermissionState"));
                }
                this.SetUnrestricted(false);
                this.Reset();
            }
        }

        public SecurityPermission(SecurityPermissionFlag flag)
        {
            this.VerifyAccess(flag);
            this.SetUnrestricted(false);
            this.m_flags = flag;
        }

        public override IPermission Copy()
        {
            if (this.IsUnrestricted())
            {
                return new SecurityPermission(PermissionState.Unrestricted);
            }
            return new SecurityPermission(this.m_flags);
        }

        public override void FromXml(SecurityElement esd)
        {
            CodeAccessPermission.ValidateElement(esd, this);
            if (XMLUtil.IsUnrestricted(esd))
            {
                this.m_flags = SecurityPermissionFlag.AllFlags;
            }
            else
            {
                this.Reset();
                this.SetUnrestricted(false);
                string str = esd.Attribute("Flags");
                if (str != null)
                {
                    this.m_flags = (SecurityPermissionFlag) Enum.Parse(typeof(SecurityPermissionFlag), str);
                }
            }
        }

        internal static int GetTokenIndex()
        {
            return 6;
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            if (!base.VerifyType(target))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            SecurityPermission permission = (SecurityPermission) target;
            SecurityPermissionFlag noFlags = SecurityPermissionFlag.NoFlags;
            if (permission.IsUnrestricted())
            {
                if (this.IsUnrestricted())
                {
                    return new SecurityPermission(PermissionState.Unrestricted);
                }
                noFlags = this.m_flags;
            }
            else if (this.IsUnrestricted())
            {
                noFlags = permission.m_flags;
            }
            else
            {
                noFlags = this.m_flags & permission.m_flags;
            }
            if (noFlags == SecurityPermissionFlag.NoFlags)
            {
                return null;
            }
            return new SecurityPermission(noFlags);
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return (this.m_flags == SecurityPermissionFlag.NoFlags);
            }
            SecurityPermission permission = target as SecurityPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            return ((this.m_flags & ~permission.m_flags) == SecurityPermissionFlag.NoFlags);
        }

        public bool IsUnrestricted()
        {
            return (this.m_flags == SecurityPermissionFlag.AllFlags);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.LinkDemand, SkipVerification=true)]
        internal static void MethodWithSkipVerificationLinkDemand()
        {
        }

        private void Reset()
        {
            this.m_flags = SecurityPermissionFlag.NoFlags;
        }

        private void SetUnrestricted(bool unrestricted)
        {
            if (unrestricted)
            {
                this.m_flags = SecurityPermissionFlag.AllFlags;
            }
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.SecurityPermission");
            if (!this.IsUnrestricted())
            {
                element.AddAttribute("Flags", XMLUtil.BitFieldEnumToString(typeof(SecurityPermissionFlag), this.m_flags));
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
            if (!base.VerifyType(target))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            SecurityPermission permission = (SecurityPermission) target;
            if (permission.IsUnrestricted() || this.IsUnrestricted())
            {
                return new SecurityPermission(PermissionState.Unrestricted);
            }
            return new SecurityPermission(this.m_flags | permission.m_flags);
        }

        private void VerifyAccess(SecurityPermissionFlag type)
        {
            if ((type & ~SecurityPermissionFlag.AllFlags) != SecurityPermissionFlag.NoFlags)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) type }));
            }
        }

        public SecurityPermissionFlag Flags
        {
            get
            {
                return this.m_flags;
            }
            set
            {
                this.VerifyAccess(value);
                this.m_flags = value;
            }
        }
    }
}

