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
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class LoginDesigner : CompositeControlDesigner
    {
        private static DesignerAutoFormatCollection _autoFormats;
        private const string _failureTextID = "FailureText";
        private Login _login;
        private static readonly string[] _nonTemplateProperties = new string[] { 
            "BorderPadding", "CheckBoxStyle", "CreateUserIconUrl", "CreateUserText", "CreateUserUrl", "DisplayRememberMe", "FailureTextStyle", "HelpPageIconUrl", "HelpPageText", "HelpPageUrl", "HyperLinkStyle", "InstructionText", "InstructionTextStyle", "LabelStyle", "Orientation", "PasswordLabelText", 
            "PasswordRecoveryIconUrl", "PasswordRecoveryText", "PasswordRecoveryUrl", "PasswordRequiredErrorMessage", "RememberMeText", "LoginButtonImageUrl", "LoginButtonStyle", "LoginButtonText", "LoginButtonType", "TextBoxStyle", "TextLayout", "TitleText", "TitleTextStyle", "UserNameLabelText", "UserNameRequiredErrorMessage", "ValidatorTextStyle"
         };
        private const string _templateName = "LayoutTemplate";

        private void ConvertToTemplate()
        {
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ConvertToTemplateChangeCallback), null, System.Design.SR.GetString("WebControls_ConvertToTemplate"), this.TemplateDescriptor);
        }

        private bool ConvertToTemplateChangeCallback(object context)
        {
            try
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                ITemplate template = new ConvertToTemplateHelper(this, service).ConvertToTemplate();
                this.TemplateDescriptor.SetValue(this._login, template);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override string GetDesignTimeHtml(DesignerRegionCollection regions)
        {
            if (base.UseRegions(regions, this._login.LayoutTemplate))
            {
                ((WebControl) base.ViewControl).Enabled = true;
                IDictionary data = new HybridDictionary(1);
                data.Add("RegionEditing", true);
                ((IControlDesignerAccessor) base.ViewControl).SetDesignModeState(data);
                EditableDesignerRegion region = new TemplatedEditableDesignerRegion(this.TemplateDefinition) {
                    Description = System.Design.SR.GetString("ContainerControlDesigner_RegionWatermark")
                };
                regions.Add(region);
            }
            return this.GetDesignTimeHtml();
        }

        public override string GetEditableDesignerRegionContent(EditableDesignerRegion region)
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            return ControlPersister.PersistTemplate(this._login.LayoutTemplate, service);
        }

        protected override string GetErrorDesignTimeHtml(Exception e)
        {
            return base.CreatePlaceHolderDesignTimeHtml(System.Design.SR.GetString("Control_ErrorRenderingShort") + "<br />" + e.Message);
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(Login));
            this._login = (Login) component;
            base.Initialize(component);
        }

        private void LaunchWebAdmin()
        {
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (host != null)
            {
                IWebAdministrationService service = (IWebAdministrationService) host.GetService(typeof(IWebAdministrationService));
                if (service != null)
                {
                    service.Start(null);
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            if (this.Templated)
            {
                foreach (string str in _nonTemplateProperties)
                {
                    PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[str];
                    if (oldPropertyDescriptor != null)
                    {
                        properties[str] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { BrowsableAttribute.No });
                    }
                }
            }
            RenderOuterTableHelper.SetupRenderOuterTable(properties, base.Component, false, base.GetType());
        }

        private void Reset()
        {
            this.UpdateDesignTimeHtml();
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ResetChangeCallback), null, System.Design.SR.GetString("WebControls_Reset"), this.TemplateDescriptor);
        }

        private bool ResetChangeCallback(object context)
        {
            this.TemplateDescriptor.SetValue(this._login, null);
            return true;
        }

        public override void SetEditableDesignerRegionContent(EditableDesignerRegion region, string content)
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            ITemplate template = ControlParser.ParseTemplate(service, content);
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component)[region.Name];
            using (DesignerTransaction transaction = service.CreateTransaction("SetEditableDesignerRegionContent"))
            {
                descriptor.SetValue(base.Component, template);
                transaction.Commit();
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                lists.Add(new LoginDesignerActionList(this));
                return lists;
            }
        }

        public override bool AllowResize
        {
            get
            {
                if (!this.RenderOuterTable)
                {
                    return false;
                }
                return base.AllowResize;
            }
        }

        public override DesignerAutoFormatCollection AutoFormats
        {
            get
            {
                if (_autoFormats == null)
                {
                    _autoFormats = ControlDesigner.CreateAutoFormats(AutoFormatSchemes.LOGIN_SCHEME_NAMES, schemeName => new LoginAutoFormat(schemeName, "<Schemes>\r\n<xsd:schema id=\"Schemes\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n  <xsd:element name=\"Scheme\">\r\n     <xsd:complexType>\r\n       <xsd:all>\r\n        <xsd:element name=\"SchemeName\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderPadding\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"FontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"FontName\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TextLayout\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TitleTextBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TitleTextForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TitleTextFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TitleTextFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"InstructionTextForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"InstructionTextFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TextboxFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SubmitButtonBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SubmitButtonForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SubmitButtonFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SubmitButtonFontName\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SubmitButtonBorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SubmitButtonBorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SubmitButtonBorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"RenderOuterTable\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n      </xsd:all>\r\n    </xsd:complexType>\r\n  </xsd:element>\r\n  <xsd:element name=\"Schemes\" msdata:IsDataSet=\"true\">\r\n    <xsd:complexType>\r\n      <xsd:choice maxOccurs=\"unbounded\">\r\n        <xsd:element ref=\"Scheme\"/>\r\n      </xsd:choice>\r\n    </xsd:complexType>\r\n  </xsd:element>\r\n</xsd:schema>\r\n<Scheme>\r\n  <SchemeName>LoginScheme_Empty</SchemeName>\r\n  <RenderOuterTable>True</RenderOuterTable>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>LoginScheme_Elegant</SchemeName>\r\n  <BackColor>#F7F7DE</BackColor>\r\n  <BorderColor>#CCCC99</BorderColor>\r\n  <BorderWidth>1</BorderWidth>\r\n  <BorderStyle>4</BorderStyle>\r\n  <FontSize>10</FontSize>\r\n  <FontName>Verdana</FontName>\r\n  <TitleTextBackColor>#6B696B</TitleTextBackColor>\r\n  <TitleTextForeColor>#FFFFFF</TitleTextForeColor>\r\n  <TitleTextFont>1</TitleTextFont>\r\n  <RenderOuterTable>True</RenderOuterTable>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>LoginScheme_Professional</SchemeName>\r\n  <BackColor>#F7F6F3</BackColor>\r\n  <ForeColor>#333333</ForeColor>\r\n  <BorderColor>#E6E2D8</BorderColor>\r\n  <BorderWidth>1</BorderWidth>\r\n  <BorderStyle>4</BorderStyle>\r\n  <BorderPadding>4</BorderPadding>\r\n  <FontSize>0.8em</FontSize>\r\n  <FontName>Verdana</FontName>\r\n  <TitleTextBackColor>#5D7B9D</TitleTextBackColor>\r\n  <TitleTextForeColor>White</TitleTextForeColor>\r\n  <TitleTextFont>1</TitleTextFont>\r\n  <TitleTextFontSize>0.9em</TitleTextFontSize>\r\n  <InstructionTextForeColor>Black</InstructionTextForeColor>\r\n  <InstructionTextFont>2</InstructionTextFont>\r\n  <TextboxFontSize>0.8em</TextboxFontSize>\r\n  <SubmitButtonBackColor>#FFFBFF</SubmitButtonBackColor>\r\n  <SubmitButtonForeColor>#284775</SubmitButtonForeColor>\r\n  <SubmitButtonFontSize>0.8em</SubmitButtonFontSize>\r\n  <SubmitButtonFontName>Verdana</SubmitButtonFontName>\r\n  <SubmitButtonBorderColor>#CCCCCC</SubmitButtonBorderColor>\r\n  <SubmitButtonBorderWidth>1</SubmitButtonBorderWidth>\r\n  <SubmitButtonBorderStyle>4</SubmitButtonBorderStyle>\r\n  <RenderOuterTable>True</RenderOuterTable>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>LoginScheme_Simple</SchemeName>\r\n  <BackColor>#E3EAEB</BackColor>\r\n  <ForeColor>#333333</ForeColor>\r\n  <BorderColor>#E6E2D8</BorderColor>\r\n  <BorderWidth>1</BorderWidth>\r\n  <BorderStyle>4</BorderStyle>\r\n  <BorderPadding>4</BorderPadding>\r\n  <FontSize>0.8em</FontSize>\r\n  <FontName>Verdana</FontName>\r\n  <TextLayout>1</TextLayout>\r\n  <TitleTextBackColor>#1C5E55</TitleTextBackColor>\r\n  <TitleTextForeColor>White</TitleTextForeColor>\r\n  <TitleTextFont>1</TitleTextFont>\r\n  <TitleTextFontSize>0.9em</TitleTextFontSize>\r\n  <InstructionTextForeColor>Black</InstructionTextForeColor>\r\n  <InstructionTextFont>2</InstructionTextFont>\r\n  <TextboxFontSize>0.8em</TextboxFontSize>\r\n  <SubmitButtonBackColor>White</SubmitButtonBackColor>\r\n  <SubmitButtonForeColor>#1C5E55</SubmitButtonForeColor>\r\n  <SubmitButtonFontSize>0.8em</SubmitButtonFontSize>\r\n  <SubmitButtonFontName>Verdana</SubmitButtonFontName>\r\n  <SubmitButtonBorderColor>#C5BBAF</SubmitButtonBorderColor>\r\n  <SubmitButtonBorderWidth>1</SubmitButtonBorderWidth>\r\n  <SubmitButtonBorderStyle>4</SubmitButtonBorderStyle>\r\n  <RenderOuterTable>True</RenderOuterTable>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>LoginScheme_Classic</SchemeName>\r\n  <BackColor>#EFF3FB</BackColor>\r\n  <ForeColor>#333333</ForeColor>\r\n  <BorderColor>#B5C7DE</BorderColor>\r\n  <BorderWidth>1</BorderWidth>\r\n  <BorderStyle>4</BorderStyle>\r\n  <BorderPadding>4</BorderPadding>\r\n  <FontSize>0.8em</FontSize>\r\n  <FontName>Verdana</FontName>\r\n  <TitleTextBackColor>#507CD1</TitleTextBackColor>\r\n  <TitleTextForeColor>White</TitleTextForeColor>\r\n  <TitleTextFont>1</TitleTextFont>\r\n  <TitleTextFontSize>0.9em</TitleTextFontSize>\r\n  <InstructionTextForeColor>Black</InstructionTextForeColor>\r\n  <InstructionTextFont>2</InstructionTextFont>\r\n  <TextboxFontSize>0.8em</TextboxFontSize>\r\n  <SubmitButtonBackColor>White</SubmitButtonBackColor>\r\n  <SubmitButtonForeColor>#284E98</SubmitButtonForeColor>\r\n  <SubmitButtonFontSize>0.8em</SubmitButtonFontSize>\r\n  <SubmitButtonFontName>Verdana</SubmitButtonFontName>\r\n  <SubmitButtonBorderColor>#507CD1</SubmitButtonBorderColor>\r\n  <SubmitButtonBorderWidth>1</SubmitButtonBorderWidth>\r\n  <SubmitButtonBorderStyle>4</SubmitButtonBorderStyle>\r\n  <RenderOuterTable>True</RenderOuterTable>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>LoginScheme_Colorful</SchemeName>\r\n  <BackColor>#FFFBD6</BackColor>\r\n  <ForeColor>#333333</ForeColor>\r\n  <BorderColor>#FFDFAD</BorderColor>\r\n  <BorderWidth>1</BorderWidth>\r\n  <BorderStyle>4</BorderStyle>\r\n  <BorderPadding>4</BorderPadding>\r\n  <FontSize>0.8em</FontSize>\r\n  <FontName>Verdana</FontName>\r\n  <TextLayout>1</TextLayout>\r\n  <TitleTextBackColor>#990000</TitleTextBackColor>\r\n  <TitleTextForeColor>White</TitleTextForeColor>\r\n  <TitleTextFont>1</TitleTextFont>\r\n  <TitleTextFontSize>0.9em</TitleTextFontSize>\r\n  <InstructionTextForeColor>Black</InstructionTextForeColor>\r\n  <InstructionTextFont>2</InstructionTextFont>\r\n  <TextboxFontSize>0.8em</TextboxFontSize>\r\n  <SubmitButtonBackColor>White</SubmitButtonBackColor>\r\n  <SubmitButtonForeColor>#990000</SubmitButtonForeColor>\r\n  <SubmitButtonFontSize>0.8em</SubmitButtonFontSize>\r\n  <SubmitButtonFontName>Verdana</SubmitButtonFontName>\r\n  <SubmitButtonBorderColor>#CC9966</SubmitButtonBorderColor>\r\n  <SubmitButtonBorderWidth>1</SubmitButtonBorderWidth>\r\n  <SubmitButtonBorderStyle>4</SubmitButtonBorderStyle>\r\n  <RenderOuterTable>True</RenderOuterTable>\r\n</Scheme>\r\n</Schemes>\r\n"));
                }
                return _autoFormats;
            }
        }

        public bool RenderOuterTable
        {
            get
            {
                return ((Login) base.Component).RenderOuterTable;
            }
            set
            {
                RenderOuterTableHelper.SetRenderOuterTable(value, this, false);
            }
        }

        private bool Templated
        {
            get
            {
                return (this._login.LayoutTemplate != null);
            }
        }

        private System.Web.UI.Design.TemplateDefinition TemplateDefinition
        {
            get
            {
                return new System.Web.UI.Design.TemplateDefinition(this, "LayoutTemplate", this._login, "LayoutTemplate", ((WebControl) base.ViewControl).ControlStyle);
            }
        }

        private PropertyDescriptor TemplateDescriptor
        {
            get
            {
                return TypeDescriptor.GetProperties(base.Component).Find("LayoutTemplate", false);
            }
        }

        public override TemplateGroupCollection TemplateGroups
        {
            get
            {
                TemplateGroupCollection templateGroups = base.TemplateGroups;
                TemplateGroup group = new TemplateGroup("LayoutTemplate", ((WebControl) base.ViewControl).ControlStyle);
                group.AddTemplateDefinition(new System.Web.UI.Design.TemplateDefinition(this, "LayoutTemplate", this._login, "LayoutTemplate", ((WebControl) base.ViewControl).ControlStyle));
                templateGroups.Add(group);
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

        private sealed class ConvertToTemplateHelper : LoginDesignerUtil.GenericConvertToTemplateHelper<Login, LoginDesigner>
        {
            private static readonly string[] _persistedControlIDs = new string[] { "UserName", "UserNameRequired", "Password", "PasswordRequired", "RememberMe", "LoginButton", "LoginImageButton", "LoginLinkButton", "FailureText", "CreateUserLink", "PasswordRecoveryLink", "HelpLink" };
            private static readonly string[] _persistedIfNotVisibleControlIDs = new string[] { "FailureText" };

            public ConvertToTemplateHelper(LoginDesigner designer, IDesignerHost designerHost) : base(designer, designerHost)
            {
            }

            protected override System.Web.UI.Control GetDefaultTemplateContents()
            {
                System.Web.UI.Control control = base.Designer.ViewControl.Controls[0];
                return (Table) control.Controls[0];
            }

            protected override Style GetFailureTextStyle(Login control)
            {
                return control.FailureTextStyle;
            }

            protected override ITemplate GetTemplate(Login control)
            {
                return control.LayoutTemplate;
            }

            protected override string[] PersistedControlIDs
            {
                get
                {
                    return _persistedControlIDs;
                }
            }

            protected override string[] PersistedIfNotVisibleControlIDs
            {
                get
                {
                    return _persistedIfNotVisibleControlIDs;
                }
            }
        }

        private class LoginDesignerActionList : DesignerActionList
        {
            private LoginDesigner _parent;

            public LoginDesignerActionList(LoginDesigner parent) : base(parent.Component)
            {
                this._parent = parent;
            }

            public void ConvertToTemplate()
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    this._parent.ConvertToTemplate();
                }
                finally
                {
                    Cursor.Current = current;
                }
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                if (this._parent.InTemplateMode)
                {
                    return new DesignerActionItemCollection();
                }
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                if (!this._parent.Templated)
                {
                    items.Add(new DesignerActionMethodItem(this, "ConvertToTemplate", System.Design.SR.GetString("WebControls_ConvertToTemplate"), string.Empty, System.Design.SR.GetString("WebControls_ConvertToTemplateDescription"), true));
                }
                else
                {
                    items.Add(new DesignerActionMethodItem(this, "Reset", System.Design.SR.GetString("WebControls_Reset"), string.Empty, System.Design.SR.GetString("WebControls_ResetDescription"), true));
                }
                items.Add(new DesignerActionMethodItem(this, "LaunchWebAdmin", System.Design.SR.GetString("Login_LaunchWebAdmin"), string.Empty, System.Design.SR.GetString("Login_LaunchWebAdminDescription"), true));
                return items;
            }

            public void LaunchWebAdmin()
            {
                this._parent.LaunchWebAdmin();
            }

            public void Reset()
            {
                this._parent.Reset();
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
        }
    }
}

