namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class UIPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission
    {
        private UIPermissionClipboard m_clipboardFlag;
        private UIPermissionWindow m_windowFlag;

        public UIPermission(PermissionState state)
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

        public UIPermission(UIPermissionClipboard clipboardFlag)
        {
            VerifyClipboardFlag(clipboardFlag);
            this.m_clipboardFlag = clipboardFlag;
        }

        public UIPermission(UIPermissionWindow windowFlag)
        {
            VerifyWindowFlag(windowFlag);
            this.m_windowFlag = windowFlag;
        }

        public UIPermission(UIPermissionWindow windowFlag, UIPermissionClipboard clipboardFlag)
        {
            VerifyWindowFlag(windowFlag);
            VerifyClipboardFlag(clipboardFlag);
            this.m_windowFlag = windowFlag;
            this.m_clipboardFlag = clipboardFlag;
        }

        public override IPermission Copy()
        {
            return new UIPermission(this.m_windowFlag, this.m_clipboardFlag);
        }

        public override void FromXml(SecurityElement esd)
        {
            CodeAccessPermission.ValidateElement(esd, this);
            if (XMLUtil.IsUnrestricted(esd))
            {
                this.SetUnrestricted(true);
            }
            else
            {
                this.m_windowFlag = UIPermissionWindow.NoWindows;
                this.m_clipboardFlag = UIPermissionClipboard.NoClipboard;
                string str = esd.Attribute("Window");
                if (str != null)
                {
                    this.m_windowFlag = (UIPermissionWindow) Enum.Parse(typeof(UIPermissionWindow), str);
                }
                string str2 = esd.Attribute("Clipboard");
                if (str2 != null)
                {
                    this.m_clipboardFlag = (UIPermissionClipboard) Enum.Parse(typeof(UIPermissionClipboard), str2);
                }
            }
        }

        internal static int GetTokenIndex()
        {
            return 7;
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
            UIPermission permission = (UIPermission) target;
            UIPermissionWindow windowFlag = (this.m_windowFlag < permission.m_windowFlag) ? this.m_windowFlag : permission.m_windowFlag;
            UIPermissionClipboard clipboardFlag = (this.m_clipboardFlag < permission.m_clipboardFlag) ? this.m_clipboardFlag : permission.m_clipboardFlag;
            if ((windowFlag == UIPermissionWindow.NoWindows) && (clipboardFlag == UIPermissionClipboard.NoClipboard))
            {
                return null;
            }
            return new UIPermission(windowFlag, clipboardFlag);
        }

        public override bool IsSubsetOf(IPermission target)
        {
            bool flag;
            if (target == null)
            {
                return ((this.m_windowFlag == UIPermissionWindow.NoWindows) && (this.m_clipboardFlag == UIPermissionClipboard.NoClipboard));
            }
            try
            {
                UIPermission permission = (UIPermission) target;
                if (permission.IsUnrestricted())
                {
                    return true;
                }
                if (this.IsUnrestricted())
                {
                    return false;
                }
                flag = (this.m_windowFlag <= permission.m_windowFlag) && (this.m_clipboardFlag <= permission.m_clipboardFlag);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            return flag;
        }

        public bool IsUnrestricted()
        {
            return ((this.m_windowFlag == UIPermissionWindow.AllWindows) && (this.m_clipboardFlag == UIPermissionClipboard.AllClipboard));
        }

        private void Reset()
        {
            this.m_windowFlag = UIPermissionWindow.NoWindows;
            this.m_clipboardFlag = UIPermissionClipboard.NoClipboard;
        }

        private void SetUnrestricted(bool unrestricted)
        {
            if (unrestricted)
            {
                this.m_windowFlag = UIPermissionWindow.AllWindows;
                this.m_clipboardFlag = UIPermissionClipboard.AllClipboard;
            }
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.UIPermission");
            if (!this.IsUnrestricted())
            {
                if (this.m_windowFlag != UIPermissionWindow.NoWindows)
                {
                    element.AddAttribute("Window", Enum.GetName(typeof(UIPermissionWindow), this.m_windowFlag));
                }
                if (this.m_clipboardFlag != UIPermissionClipboard.NoClipboard)
                {
                    element.AddAttribute("Clipboard", Enum.GetName(typeof(UIPermissionClipboard), this.m_clipboardFlag));
                }
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
            UIPermission permission = (UIPermission) target;
            UIPermissionWindow windowFlag = (this.m_windowFlag > permission.m_windowFlag) ? this.m_windowFlag : permission.m_windowFlag;
            UIPermissionClipboard clipboardFlag = (this.m_clipboardFlag > permission.m_clipboardFlag) ? this.m_clipboardFlag : permission.m_clipboardFlag;
            if ((windowFlag == UIPermissionWindow.NoWindows) && (clipboardFlag == UIPermissionClipboard.NoClipboard))
            {
                return null;
            }
            return new UIPermission(windowFlag, clipboardFlag);
        }

        private static void VerifyClipboardFlag(UIPermissionClipboard flag)
        {
            if ((flag < UIPermissionClipboard.NoClipboard) || (flag > UIPermissionClipboard.AllClipboard))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) flag }));
            }
        }

        private static void VerifyWindowFlag(UIPermissionWindow flag)
        {
            if ((flag < UIPermissionWindow.NoWindows) || (flag > UIPermissionWindow.AllWindows))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) flag }));
            }
        }

        public UIPermissionClipboard Clipboard
        {
            get
            {
                return this.m_clipboardFlag;
            }
            set
            {
                VerifyClipboardFlag(value);
                this.m_clipboardFlag = value;
            }
        }

        public UIPermissionWindow Window
        {
            get
            {
                return this.m_windowFlag;
            }
            set
            {
                VerifyWindowFlag(value);
                this.m_windowFlag = value;
            }
        }
    }
}

