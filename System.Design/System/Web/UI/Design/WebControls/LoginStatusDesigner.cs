namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class LoginStatusDesigner : CompositeControlDesigner
    {
        private bool _loggedIn;
        private LoginStatus _loginStatus;

        public override string GetDesignTimeHtml()
        {
            string logoutText;
            IDictionary data = new HybridDictionary(2);
            data["LoggedIn"] = this._loggedIn;
            LoginStatus viewControl = (LoginStatus) base.ViewControl;
            ((IControlDesignerAccessor) viewControl).SetDesignModeState(data);
            if (this._loggedIn)
            {
                logoutText = viewControl.LogoutText;
                if (((logoutText == null) || (logoutText.Length == 0)) || (logoutText == " "))
                {
                    viewControl.LogoutText = "[" + viewControl.ID + "]";
                }
            }
            else
            {
                logoutText = viewControl.LoginText;
                if (((logoutText == null) || (logoutText.Length == 0)) || (logoutText == " "))
                {
                    viewControl.LoginText = "[" + viewControl.ID + "]";
                }
            }
            return base.GetDesignTimeHtml();
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(LoginStatus));
            this._loginStatus = (LoginStatus) component;
            base.Initialize(component);
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                lists.Add(new LoginStatusDesignerActionList(this));
                return lists;
            }
        }

        protected override bool UsePreviewControl
        {
            get
            {
                return true;
            }
        }

        private class LoginStatusDesignerActionList : DesignerActionList
        {
            private LoginStatusDesigner _designer;

            public LoginStatusDesignerActionList(LoginStatusDesigner designer) : base(designer.Component)
            {
                this._designer = designer;
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                items.Add(new DesignerActionPropertyItem("View", System.Design.SR.GetString("WebControls_Views"), string.Empty, System.Design.SR.GetString("WebControls_ViewsDescription")));
                return items;
            }

            public override bool AutoShow
            {
                get
                {
                    return true;
                }
                set
                {
                }
            }

            [TypeConverter(typeof(LoginStatusViewTypeConverter))]
            public string View
            {
                get
                {
                    if (this._designer._loggedIn)
                    {
                        return System.Design.SR.GetString("LoginStatus_LoggedInView");
                    }
                    return System.Design.SR.GetString("LoginStatus_LoggedOutView");
                }
                set
                {
                    if (string.Compare(value, System.Design.SR.GetString("LoginStatus_LoggedInView"), StringComparison.Ordinal) == 0)
                    {
                        this._designer._loggedIn = true;
                    }
                    else if (string.Compare(value, System.Design.SR.GetString("LoginStatus_LoggedOutView"), StringComparison.Ordinal) == 0)
                    {
                        this._designer._loggedIn = false;
                    }
                    this._designer.UpdateDesignTimeHtml();
                }
            }

            private class LoginStatusViewTypeConverter : TypeConverter
            {
                public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
                {
                    return new TypeConverter.StandardValuesCollection(new string[] { System.Design.SR.GetString("LoginStatus_LoggedOutView"), System.Design.SR.GetString("LoginStatus_LoggedInView") });
                }

                public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
                {
                    return true;
                }

                public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
                {
                    return true;
                }
            }
        }
    }
}

