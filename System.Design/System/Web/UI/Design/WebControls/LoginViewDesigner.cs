namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class LoginViewDesigner : ControlDesigner
    {
        private const int _anonymousTemplateIndex = 0;
        private const string _anonymousTemplateName = "AnonymousTemplate";
        private const string _contentTemplateName = "ContentTemplate";
        private const string _designtimeHTML = "<table cellspacing=0 cellpadding=0 border=0 style=\"display:inline-block\">\r\n                <tr>\r\n                    <td nowrap align=center valign=middle style=\"color:{0}; background-color:{1}; \">{2}</td>\r\n                </tr>\r\n                <tr>\r\n                    <td style=\"vertical-align:top;\" {3}='0'>{4}</td>\r\n                </tr>\r\n          </table>";
        private const int _loggedInTemplateIndex = 1;
        private const string _loggedInTemplateName = "LoggedInTemplate";
        private LoginView _loginView;
        private const string _roleGroupsPropertyName = "RoleGroups";
        private const int _roleGroupStartingIndex = 2;
        private TemplateGroupCollection _templateGroups;
        private static readonly string[] _templateNames = new string[] { "AnonymousTemplate", "LoggedInTemplate" };

        private EditableDesignerRegion BuildRegion()
        {
            return new LoginViewDesignerRegion(this, this.CurrentObject, this.CurrentTemplate, this.CurrentTemplateDescriptor, this.TemplateDefinition) { Description = System.Design.SR.GetString("ContainerControlDesigner_RegionWatermark") };
        }

        private static string CreateRoleGroupCaption(int roleGroupIndex, RoleGroupCollection roleGroups)
        {
            string str = roleGroups[roleGroupIndex].ToString();
            string str2 = "RoleGroup[" + roleGroupIndex.ToString(CultureInfo.InvariantCulture) + "]";
            if ((str != null) && (str.Length > 0))
            {
                str2 = str2 + " - " + str;
            }
            return str2;
        }

        private void EditRoleGroups()
        {
            PropertyDescriptor context = TypeDescriptor.GetProperties(base.Component)["RoleGroups"];
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.EditRoleGroupsChangeCallback), context, System.Design.SR.GetString("LoginView_EditRoleGroupsTransactionDescription"), context);
            int num = this._loginView.RoleGroups.Count + 2;
            if (this.CurrentView >= num)
            {
                this.CurrentView = num - 1;
            }
            if (this.CurrentView < 0)
            {
                this.CurrentView = 0;
            }
            this._templateGroups = null;
        }

        private bool EditRoleGroupsChangeCallback(object context)
        {
            PropertyDescriptor propDesc = (PropertyDescriptor) context;
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            UITypeEditor editor = (UITypeEditor) propDesc.GetEditor(typeof(UITypeEditor));
            return (editor.EditValue(new System.Web.UI.Design.WebControls.TypeDescriptorContext(service, propDesc, base.Component), new WindowsFormsEditorServiceHelper(this), propDesc.GetValue(base.Component)) != null);
        }

        public override string GetDesignTimeHtml()
        {
            string designTimeHtml = string.Empty;
            if (this.CurrentViewControlTemplate != null)
            {
                LoginView viewControl = (LoginView) base.ViewControl;
                IDictionary data = new HybridDictionary(1);
                data["TemplateIndex"] = this.CurrentView;
                ((IControlDesignerAccessor) viewControl).SetDesignModeState(data);
                viewControl.DataBind();
                designTimeHtml = base.GetDesignTimeHtml();
            }
            return designTimeHtml;
        }

        public override string GetDesignTimeHtml(DesignerRegionCollection regions)
        {
            string designTimeHtml = string.Empty;
            if (base.UseRegions(regions, this.CurrentTemplate, this.CurrentViewControlTemplate))
            {
                regions.Add(this.BuildRegion());
            }
            else
            {
                designTimeHtml = this.GetDesignTimeHtml();
            }
            StringBuilder builder = new StringBuilder(0x400);
            builder.Append(string.Format(CultureInfo.InvariantCulture, "<table cellspacing=0 cellpadding=0 border=0 style=\"display:inline-block\">\r\n                <tr>\r\n                    <td nowrap align=center valign=middle style=\"color:{0}; background-color:{1}; \">{2}</td>\r\n                </tr>\r\n                <tr>\r\n                    <td style=\"vertical-align:top;\" {3}='0'>{4}</td>\r\n                </tr>\r\n          </table>", new object[] { ColorTranslator.ToHtml(SystemColors.ControlText), ColorTranslator.ToHtml(SystemColors.Control), this._loginView.ID, DesignerRegion.DesignerRegionAttributeName, designTimeHtml }));
            return builder.ToString();
        }

        public override string GetEditableDesignerRegionContent(EditableDesignerRegion region)
        {
            if (region is LoginViewDesignerRegion)
            {
                ITemplate template = ((LoginViewDesignerRegion) region).Template;
                if (template != null)
                {
                    IDesignerHost service = (IDesignerHost) base.Component.Site.GetService(typeof(IDesignerHost));
                    return ControlPersister.PersistTemplate(template, service);
                }
            }
            return base.GetEditableDesignerRegionContent(region);
        }

        protected override string GetEmptyDesignTimeHtml()
        {
            string str = string.Empty;
            switch (this.CurrentView)
            {
                case 0:
                    str = System.Design.SR.GetString("LoginView_AnonymousTemplateEmpty");
                    break;

                case 1:
                    str = System.Design.SR.GetString("LoginView_LoggedInTemplateEmpty");
                    break;

                default:
                {
                    int roleGroupIndex = this.CurrentView - 2;
                    string str2 = CreateRoleGroupCaption(roleGroupIndex, this._loginView.RoleGroups);
                    str = System.Design.SR.GetString("LoginView_RoleGroupTemplateEmpty", new object[] { str2 });
                    break;
                }
            }
            return base.CreatePlaceHolderDesignTimeHtml(str + "<br>" + System.Design.SR.GetString("LoginView_NoTemplateInst"));
        }

        protected override string GetErrorDesignTimeHtml(Exception e)
        {
            return base.CreatePlaceHolderDesignTimeHtml(System.Design.SR.GetString("LoginView_ErrorRendering") + "<br />" + e.Message);
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(LoginView));
            this._loginView = (LoginView) component;
            base.Initialize(component);
        }

        private void LaunchWebAdmin()
        {
            if (base.Component.Site != null)
            {
                IDesignerHost host = (IDesignerHost) base.Component.Site.GetService(typeof(IDesignerHost));
                if (host != null)
                {
                    IWebAdministrationService service = (IWebAdministrationService) host.GetService(typeof(IWebAdministrationService));
                    if (service != null)
                    {
                        service.Start(null);
                    }
                }
            }
        }

        public override void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if ((e.Member == null) || e.Member.Name.Equals("RoleGroups"))
            {
                int num = this._loginView.RoleGroups.Count + 2;
                if (this.CurrentView >= num)
                {
                    this.CurrentView = num - 1;
                }
                this._templateGroups = null;
            }
            base.OnComponentChanged(sender, e);
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            if (base.InTemplateMode)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["RoleGroups"];
                properties["RoleGroups"] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { BrowsableAttribute.No });
            }
        }

        public override void SetEditableDesignerRegionContent(EditableDesignerRegion region, string content)
        {
            LoginViewDesignerRegion region2 = region as LoginViewDesignerRegion;
            if (region2 != null)
            {
                IDesignerHost service = (IDesignerHost) base.Component.Site.GetService(typeof(IDesignerHost));
                ITemplate template = ControlParser.ParseTemplate(service, content);
                using (DesignerTransaction transaction = service.CreateTransaction("SetEditableDesignerRegionContent"))
                {
                    region2.PropertyDescriptor.SetValue(region2.Object, template);
                    transaction.Commit();
                }
                region2.Template = template;
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                lists.Add(new LoginViewDesignerActionList(this));
                return lists;
            }
        }

        private object CurrentObject
        {
            get
            {
                if (this.CurrentView == 0)
                {
                    return base.Component;
                }
                if (this.CurrentView == 1)
                {
                    return base.Component;
                }
                return this._loginView.RoleGroups[this.CurrentView - 2];
            }
        }

        private ITemplate CurrentTemplate
        {
            get
            {
                if (this.CurrentView == 0)
                {
                    return this._loginView.AnonymousTemplate;
                }
                if (this.CurrentView == 1)
                {
                    return this._loginView.LoggedInTemplate;
                }
                RoleGroup group = this._loginView.RoleGroups[this.CurrentView - 2];
                return group.ContentTemplate;
            }
        }

        private PropertyDescriptor CurrentTemplateDescriptor
        {
            get
            {
                if (this.CurrentView == 0)
                {
                    return TypeDescriptor.GetProperties(base.Component)["AnonymousTemplate"];
                }
                if (this.CurrentView == 1)
                {
                    return TypeDescriptor.GetProperties(base.Component)["LoggedInTemplate"];
                }
                RoleGroup component = this._loginView.RoleGroups[this.CurrentView - 2];
                return TypeDescriptor.GetProperties(component)["ContentTemplate"];
            }
        }

        private int CurrentView
        {
            get
            {
                object obj2 = base.DesignerState["CurrentView"];
                int num = (obj2 == null) ? 0 : ((int) obj2);
                if (num <= ((2 + this._loginView.RoleGroups.Count) - 1))
                {
                    return num;
                }
                return 0;
            }
            set
            {
                base.DesignerState["CurrentView"] = value;
            }
        }

        private ITemplate CurrentViewControlTemplate
        {
            get
            {
                if (this.CurrentView == 0)
                {
                    return ((LoginView) base.ViewControl).AnonymousTemplate;
                }
                if (this.CurrentView == 1)
                {
                    return ((LoginView) base.ViewControl).LoggedInTemplate;
                }
                RoleGroup group = ((LoginView) base.ViewControl).RoleGroups[this.CurrentView - 2];
                return group.ContentTemplate;
            }
        }

        private System.Web.UI.Design.TemplateDefinition TemplateDefinition
        {
            get
            {
                int currentView = this.CurrentView;
                if (currentView == 0)
                {
                    return new System.Web.UI.Design.TemplateDefinition(this, "AnonymousTemplate", this._loginView, "AnonymousTemplate");
                }
                if (this.CurrentView == 1)
                {
                    return new System.Web.UI.Design.TemplateDefinition(this, "LoggedInTemplate", this._loginView, "LoggedInTemplate");
                }
                return new System.Web.UI.Design.TemplateDefinition(this, "ContentTemplate", this._loginView.RoleGroups[currentView - 2], "ContentTemplate");
            }
        }

        public override TemplateGroupCollection TemplateGroups
        {
            get
            {
                TemplateGroupCollection templateGroups = base.TemplateGroups;
                if (this._templateGroups == null)
                {
                    this._templateGroups = new TemplateGroupCollection();
                    TemplateGroup group = new TemplateGroup("AnonymousTemplate");
                    group.AddTemplateDefinition(new System.Web.UI.Design.TemplateDefinition(this, "AnonymousTemplate", this._loginView, "AnonymousTemplate"));
                    this._templateGroups.Add(group);
                    group = new TemplateGroup("LoggedInTemplate");
                    group.AddTemplateDefinition(new System.Web.UI.Design.TemplateDefinition(this, "LoggedInTemplate", this._loginView, "LoggedInTemplate"));
                    this._templateGroups.Add(group);
                    RoleGroupCollection roleGroups = this._loginView.RoleGroups;
                    for (int i = 0; i < roleGroups.Count; i++)
                    {
                        string groupName = CreateRoleGroupCaption(i, roleGroups);
                        group = new TemplateGroup(groupName);
                        group.AddTemplateDefinition(new System.Web.UI.Design.TemplateDefinition(this, groupName, this._loginView.RoleGroups[i], "ContentTemplate"));
                        this._templateGroups.Add(group);
                    }
                }
                templateGroups.AddRange(this._templateGroups);
                return templateGroups;
            }
        }

        protected override bool UsePreviewControl
        {
            get
            {
                return true;
            }
        }

        private class LoginViewDesignerActionList : DesignerActionList
        {
            private LoginViewDesigner _designer;

            public LoginViewDesignerActionList(LoginViewDesigner designer) : base(designer.Component)
            {
                this._designer = designer;
            }

            public void EditRoleGroups()
            {
                this._designer.EditRoleGroups();
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                items.Add(new DesignerActionMethodItem(this, "EditRoleGroups", System.Design.SR.GetString("LoginView_EditRoleGroups"), string.Empty, System.Design.SR.GetString("LoginView_EditRoleGroupsDescription"), true));
                items.Add(new DesignerActionPropertyItem("View", System.Design.SR.GetString("WebControls_Views"), string.Empty, System.Design.SR.GetString("WebControls_ViewsDescription")));
                items.Add(new DesignerActionMethodItem(this, "LaunchWebAdmin", System.Design.SR.GetString("Login_LaunchWebAdmin"), string.Empty, System.Design.SR.GetString("Login_LaunchWebAdminDescription"), true));
                return items;
            }

            public void LaunchWebAdmin()
            {
                this._designer.LaunchWebAdmin();
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

            [TypeConverter(typeof(LoginViewViewTypeConverter))]
            public string View
            {
                get
                {
                    int currentView = this._designer.CurrentView;
                    if ((currentView - 2) >= this._designer._loginView.RoleGroups.Count)
                    {
                        currentView = this._designer._loginView.RoleGroups.Count + 1;
                        this._designer.CurrentView = currentView;
                    }
                    if (currentView == 0)
                    {
                        return "AnonymousTemplate";
                    }
                    if (currentView == 1)
                    {
                        return "LoggedInTemplate";
                    }
                    return LoginViewDesigner.CreateRoleGroupCaption(currentView - 2, this._designer._loginView.RoleGroups);
                }
                set
                {
                    if (string.Compare(value, "AnonymousTemplate", StringComparison.Ordinal) == 0)
                    {
                        this._designer.CurrentView = 0;
                    }
                    else if (string.Compare(value, "LoggedInTemplate", StringComparison.Ordinal) == 0)
                    {
                        this._designer.CurrentView = 1;
                    }
                    else
                    {
                        RoleGroupCollection roleGroups = this._designer._loginView.RoleGroups;
                        for (int i = 0; i < roleGroups.Count; i++)
                        {
                            string strB = LoginViewDesigner.CreateRoleGroupCaption(i, roleGroups);
                            if (string.Compare(value, strB, StringComparison.Ordinal) == 0)
                            {
                                this._designer.CurrentView = i + 2;
                            }
                        }
                    }
                    this._designer.UpdateDesignTimeHtml();
                }
            }

            private class LoginViewViewTypeConverter : TypeConverter
            {
                public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
                {
                    LoginViewDesigner.LoginViewDesignerActionList instance = (LoginViewDesigner.LoginViewDesignerActionList) context.Instance;
                    RoleGroupCollection roleGroups = instance._designer._loginView.RoleGroups;
                    string[] values = new string[roleGroups.Count + 2];
                    values[0] = "AnonymousTemplate";
                    values[1] = "LoggedInTemplate";
                    for (int i = 0; i < roleGroups.Count; i++)
                    {
                        values[i + 2] = LoginViewDesigner.CreateRoleGroupCaption(i, roleGroups);
                    }
                    return new TypeConverter.StandardValuesCollection(values);
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

        private class LoginViewDesignerRegion : TemplatedEditableDesignerRegion
        {
            private object _object;
            private System.ComponentModel.PropertyDescriptor _prop;
            private ITemplate _template;

            public LoginViewDesignerRegion(ControlDesigner owner, object obj, ITemplate template, System.ComponentModel.PropertyDescriptor descriptor, TemplateDefinition definition) : base(definition)
            {
                this._template = template;
                this._object = obj;
                this._prop = descriptor;
                base.EnsureSize = true;
            }

            public object Object
            {
                get
                {
                    return this._object;
                }
            }

            public System.ComponentModel.PropertyDescriptor PropertyDescriptor
            {
                get
                {
                    return this._prop;
                }
            }

            public ITemplate Template
            {
                get
                {
                    return this._template;
                }
                set
                {
                    this._template = value;
                }
            }
        }
    }
}

