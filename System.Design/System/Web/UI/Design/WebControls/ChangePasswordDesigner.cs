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
    public class ChangePasswordDesigner : ControlDesigner
    {
        private static DesignerAutoFormatCollection _autoFormats;
        private ChangePassword _changePassword;
        private static readonly string[] _changePasswordViewRegionToPropertyMap = new string[] { "ChangePasswordTitleText", "UserNameLabelText", "PasswordLabelText", "InstructionText", "PasswordHintText", "NewPasswordLabelText", "ConfirmNewPasswordLabelText" };
        private const string _failureTextID = "FailureText";
        private static readonly string[] _nonTemplateProperties = new string[] { 
            "BorderPadding", "CancelButtonImageUrl", "CancelButtonStyle", "CancelButtonText", "CancelButtonType", "ChangePasswordButtonImageUrl", "ChangePasswordButtonStyle", "ChangePasswordButtonText", "ChangePasswordButtonType", "ChangePasswordTitleText", "ConfirmNewPasswordLabelText", "ConfirmPasswordCompareErrorMessage", "ConfirmPasswordRequiredErrorMessage", "ContinueButtonImageUrl", "ContinueButtonStyle", "ContinueButtonText", 
            "ContinueButtonType", "CreateUserIconUrl", "CreateUserText", "CreateUserUrl", "DisplayUserName", "EditProfileText", "EditProfileIconUrl", "EditProfileUrl", "FailureTextStyle", "HelpPageIconUrl", "HelpPageText", "HelpPageUrl", "HyperLinkStyle", "InstructionText", "InstructionTextStyle", "LabelStyle", 
            "NewPasswordLabelText", "NewPasswordRequiredErrorMessage", "NewPasswordRegularExpression", "NewPasswordRegularExpressionErrorMessage", "PasswordHintText", "PasswordHintStyle", "PasswordLabelText", "PasswordRecoveryText", "PasswordRecoveryUrl", "PasswordRecoveryIconUrl", "PasswordRequiredErrorMessage", "SuccessTitleText", "SuccessText", "SuccessTextStyle", "TextBoxStyle", "TitleTextStyle", 
            "UserNameLabelText", "UserNameRequiredErrorMessage", "ValidatorTextStyle"
         };
        private static readonly string[] _successViewRegionToPropertyMap = new string[] { "SuccessText", "SuccessTitleText" };
        private static readonly string[] _templateNames = new string[] { "ChangePasswordTemplate", "SuccessTemplate" };

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
                this.TemplateDescriptor.SetValue(this._changePassword, template);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override string GetDesignTimeHtml()
        {
            return this.GetDesignTimeHtml(null);
        }

        public override string GetDesignTimeHtml(DesignerRegionCollection regions)
        {
            IDictionary data = new HybridDictionary(2);
            data["CurrentView"] = this.CurrentView;
            if (base.UseRegions(regions, this.GetTemplate(this._changePassword)))
            {
                ((WebControl) base.ViewControl).Enabled = true;
                data.Add("RegionEditing", true);
                EditableDesignerRegion region = new TemplatedEditableDesignerRegion(this.TemplateDefinition) {
                    Description = System.Design.SR.GetString("ContainerControlDesigner_RegionWatermark")
                };
                regions.Add(region);
            }
            try
            {
                ((IControlDesignerAccessor) base.ViewControl).SetDesignModeState(data);
                ((ICompositeControlDesignerAccessor) base.ViewControl).RecreateChildControls();
                return base.GetDesignTimeHtml();
            }
            catch (Exception exception)
            {
                return this.GetErrorDesignTimeHtml(exception);
            }
        }

        public override string GetEditableDesignerRegionContent(EditableDesignerRegion region)
        {
            ITemplate template = this.GetTemplate(this._changePassword);
            if (template == null)
            {
                return this.GetEmptyDesignTimeHtml();
            }
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            return ControlPersister.PersistTemplate(template, service);
        }

        protected override string GetErrorDesignTimeHtml(Exception e)
        {
            return base.CreatePlaceHolderDesignTimeHtml(System.Design.SR.GetString("Control_ErrorRenderingShort") + "<br />" + e.Message);
        }

        private ITemplate GetTemplate(ChangePassword changePassword)
        {
            switch (this.CurrentView)
            {
                case ViewType.ChangePassword:
                    return changePassword.ChangePasswordTemplate;

                case ViewType.Success:
                    return changePassword.SuccessTemplate;
            }
            return null;
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(ChangePassword));
            this._changePassword = (ChangePassword) component;
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
            this.TemplateDescriptor.SetValue(base.Component, null);
            return true;
        }

        public override void SetEditableDesignerRegionContent(EditableDesignerRegion region, string content)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component)[region.Name];
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            ITemplate template = ControlParser.ParseTemplate(service, content);
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
                lists.Add(new ChangePasswordDesignerActionList(this));
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
                    _autoFormats = ControlDesigner.CreateAutoFormats(AutoFormatSchemes.CHANGEPASSWORD_SCHEME_NAMES, schemeName => new ChangePasswordAutoFormat(schemeName, "<Schemes>\r\n<xsd:schema id=\"Schemes\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n  <xsd:element name=\"Scheme\">\r\n     <xsd:complexType>\r\n       <xsd:all>\r\n        <xsd:element name=\"SchemeName\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderPadding\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"FontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"FontName\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TitleTextBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TitleTextForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TitleTextFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TitleTextFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"PasswordHintForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"PasswordHintFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"InstructionTextForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"InstructionTextFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TextboxFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"ButtonBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"ButtonForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"ButtonFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"ButtonFontName\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"ButtonBorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"ButtonBorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"ButtonBorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"RenderOuterTable\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n      </xsd:all>\r\n    </xsd:complexType>\r\n  </xsd:element>\r\n  <xsd:element name=\"Schemes\" msdata:IsDataSet=\"true\">\r\n    <xsd:complexType>\r\n      <xsd:choice maxOccurs=\"unbounded\">\r\n        <xsd:element ref=\"Scheme\"/>\r\n      </xsd:choice>\r\n    </xsd:complexType>\r\n  </xsd:element>\r\n</xsd:schema>\r\n<Scheme>\r\n  <SchemeName>ChangePasswordScheme_Empty</SchemeName>\r\n  <RenderOuterTable>True</RenderOuterTable>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>ChangePasswordScheme_Elegant</SchemeName>\r\n  <BackColor>#F7F7DE</BackColor>\r\n  <BorderColor>#CCCC99</BorderColor>\r\n  <BorderWidth>1</BorderWidth>\r\n  <BorderStyle>4</BorderStyle>\r\n  <FontSize>10</FontSize>\r\n  <FontName>Verdana</FontName>\r\n  <TitleTextBackColor>#6B696B</TitleTextBackColor>\r\n  <TitleTextForeColor>#FFFFFF</TitleTextForeColor>\r\n  <TitleTextFont>1</TitleTextFont>\r\n  <RenderOuterTable>True</RenderOuterTable>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>ChangePasswordScheme_Professional</SchemeName>\r\n  <BackColor>#F7F6F3</BackColor>\r\n  <ForeColor>#333333</ForeColor>\r\n  <BorderColor>#E6E2D8</BorderColor>\r\n  <BorderWidth>1</BorderWidth>\r\n  <BorderStyle>4</BorderStyle>\r\n  <BorderPadding>4</BorderPadding>\r\n  <FontSize>0.8em</FontSize>\r\n  <FontName>Verdana</FontName>\r\n  <TitleTextBackColor>#5D7B9D</TitleTextBackColor>\r\n  <TitleTextForeColor>White</TitleTextForeColor>\r\n  <TitleTextFont>1</TitleTextFont>\r\n  <TitleTextFontSize>0.9em</TitleTextFontSize>\r\n  <InstructionTextForeColor>Black</InstructionTextForeColor>\r\n  <InstructionTextFont>2</InstructionTextFont>\r\n  <PasswordHintForeColor>#888888</PasswordHintForeColor>\r\n  <PasswordHintFont>2</PasswordHintFont>\r\n  <TextboxFontSize>0.8em</TextboxFontSize>\r\n  <ButtonBackColor>#FFFBFF</ButtonBackColor>\r\n  <ButtonForeColor>#284775</ButtonForeColor>\r\n  <ButtonFontSize>0.8em</ButtonFontSize>\r\n  <ButtonFontName>Verdana</ButtonFontName>\r\n  <ButtonBorderColor>#CCCCCC</ButtonBorderColor>\r\n  <ButtonBorderWidth>1</ButtonBorderWidth>\r\n  <ButtonBorderStyle>4</ButtonBorderStyle>\r\n  <RenderOuterTable>True</RenderOuterTable>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>ChangePasswordScheme_Simple</SchemeName>\r\n  <BackColor>#E3EAEB</BackColor>\r\n  <ForeColor>#333333</ForeColor>\r\n  <BorderColor>#E6E2D8</BorderColor>\r\n  <BorderWidth>1</BorderWidth>\r\n  <BorderStyle>4</BorderStyle>\r\n  <BorderPadding>4</BorderPadding>\r\n  <FontSize>0.8em</FontSize>\r\n  <FontName>Verdana</FontName>\r\n  <TitleTextBackColor>#1C5E55</TitleTextBackColor>\r\n  <TitleTextForeColor>White</TitleTextForeColor>\r\n  <TitleTextFont>1</TitleTextFont>\r\n  <TitleTextFontSize>0.9em</TitleTextFontSize>\r\n  <InstructionTextForeColor>Black</InstructionTextForeColor>\r\n  <InstructionTextFont>2</InstructionTextFont>\r\n  <TextboxFontSize>0.8em</TextboxFontSize>\r\n  <ButtonBackColor>White</ButtonBackColor>\r\n  <ButtonForeColor>#1C5E55</ButtonForeColor>\r\n  <ButtonFontSize>0.8em</ButtonFontSize>\r\n  <ButtonFontName>Verdana</ButtonFontName>\r\n  <ButtonBorderColor>#C5BBAF</ButtonBorderColor>\r\n  <ButtonBorderWidth>1</ButtonBorderWidth>\r\n  <ButtonBorderStyle>4</ButtonBorderStyle>\r\n  <PasswordHintForeColor>#1C5E55</PasswordHintForeColor>\r\n  <PasswordHintFont>2</PasswordHintFont>\r\n  <RenderOuterTable>True</RenderOuterTable>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>ChangePasswordScheme_Classic</SchemeName>\r\n  <BackColor>#EFF3FB</BackColor>\r\n  <ForeColor>#333333</ForeColor>\r\n  <BorderColor>#B5C7DE</BorderColor>\r\n  <BorderWidth>1</BorderWidth>\r\n  <BorderStyle>4</BorderStyle>\r\n  <BorderPadding>4</BorderPadding>\r\n  <FontSize>0.8em</FontSize>\r\n  <FontName>Verdana</FontName>\r\n  <TitleTextBackColor>#507CD1</TitleTextBackColor>\r\n  <TitleTextForeColor>White</TitleTextForeColor>\r\n  <TitleTextFont>1</TitleTextFont>\r\n  <TitleTextFontSize>0.9em</TitleTextFontSize>\r\n  <InstructionTextForeColor>Black</InstructionTextForeColor>\r\n  <InstructionTextFont>2</InstructionTextFont>\r\n  <TextboxFontSize>0.8em</TextboxFontSize>\r\n  <ButtonBackColor>White</ButtonBackColor>\r\n  <ButtonForeColor>#284E98</ButtonForeColor>\r\n  <ButtonFontSize>0.8em</ButtonFontSize>\r\n  <ButtonFontName>Verdana</ButtonFontName>\r\n  <ButtonBorderColor>#507CD1</ButtonBorderColor>\r\n  <ButtonBorderWidth>1</ButtonBorderWidth>\r\n  <ButtonBorderStyle>4</ButtonBorderStyle>\r\n  <PasswordHintForeColor>#507CD1</PasswordHintForeColor>\r\n  <PasswordHintFont>2</PasswordHintFont>\r\n  <RenderOuterTable>True</RenderOuterTable>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>ChangePasswordScheme_Colorful</SchemeName>\r\n  <BackColor>#FFFBD6</BackColor>\r\n  <ForeColor>#333333</ForeColor>\r\n  <BorderColor>#FFDFAD</BorderColor>\r\n  <BorderWidth>1</BorderWidth>\r\n  <BorderStyle>4</BorderStyle>\r\n  <BorderPadding>4</BorderPadding>\r\n  <FontSize>0.8em</FontSize>\r\n  <FontName>Verdana</FontName>\r\n  <TitleTextBackColor>#990000</TitleTextBackColor>\r\n  <TitleTextForeColor>White</TitleTextForeColor>\r\n  <TitleTextFont>1</TitleTextFont>\r\n  <TitleTextFontSize>0.9em</TitleTextFontSize>\r\n  <InstructionTextForeColor>Black</InstructionTextForeColor>\r\n  <InstructionTextFont>2</InstructionTextFont>\r\n  <TextboxFontSize>0.8em</TextboxFontSize>\r\n  <ButtonBackColor>White</ButtonBackColor>\r\n  <ButtonForeColor>#990000</ButtonForeColor>\r\n  <ButtonFontSize>0.8em</ButtonFontSize>\r\n  <ButtonFontName>Verdana</ButtonFontName>\r\n  <ButtonBorderColor>#CC9966</ButtonBorderColor>\r\n  <ButtonBorderWidth>1</ButtonBorderWidth>\r\n  <ButtonBorderStyle>4</ButtonBorderStyle>\r\n  <PasswordHintForeColor>#888888</PasswordHintForeColor>\r\n  <PasswordHintFont>2</PasswordHintFont>\r\n  <RenderOuterTable>True</RenderOuterTable>\r\n</Scheme>\r\n</Schemes>\r\n"));
                }
                return _autoFormats;
            }
        }

        private ViewType CurrentView
        {
            get
            {
                object obj2 = base.DesignerState["CurrentView"];
                if (obj2 != null)
                {
                    return (ViewType) obj2;
                }
                return ViewType.ChangePassword;
            }
            set
            {
                base.DesignerState["CurrentView"] = value;
            }
        }

        public bool RenderOuterTable
        {
            get
            {
                return ((ChangePassword) base.Component).RenderOuterTable;
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
                return (this.GetTemplate(this._changePassword) != null);
            }
        }

        private System.Web.UI.Design.TemplateDefinition TemplateDefinition
        {
            get
            {
                string name = _templateNames[(int) this.CurrentView];
                return new System.Web.UI.Design.TemplateDefinition(this, name, this._changePassword, name, ((WebControl) base.ViewControl).ControlStyle);
            }
        }

        private PropertyDescriptor TemplateDescriptor
        {
            get
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(base.Component);
                string name = _templateNames[(int) this.CurrentView];
                return properties.Find(name, false);
            }
        }

        public override TemplateGroupCollection TemplateGroups
        {
            get
            {
                TemplateGroupCollection templateGroups = base.TemplateGroups;
                TemplateGroupCollection groups = new TemplateGroupCollection();
                for (int i = 0; i < _templateNames.Length; i++)
                {
                    string groupName = _templateNames[i];
                    TemplateGroup group = new TemplateGroup(groupName, ((WebControl) base.ViewControl).ControlStyle);
                    group.AddTemplateDefinition(new System.Web.UI.Design.TemplateDefinition(this, groupName, this._changePassword, groupName, ((WebControl) base.ViewControl).ControlStyle));
                    groups.Add(group);
                }
                templateGroups.AddRange(groups);
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

        private class ChangePasswordDesignerActionList : DesignerActionList
        {
            private ChangePasswordDesigner _designer;

            public ChangePasswordDesignerActionList(ChangePasswordDesigner designer) : base(designer.Component)
            {
                this._designer = designer;
            }

            public void ConvertToTemplate()
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    this._designer.ConvertToTemplate();
                }
                finally
                {
                    Cursor.Current = current;
                }
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                items.Add(new DesignerActionPropertyItem("View", System.Design.SR.GetString("WebControls_Views"), string.Empty, System.Design.SR.GetString("WebControls_ViewsDescription")));
                if (this._designer.Templated)
                {
                    items.Add(new DesignerActionMethodItem(this, "Reset", System.Design.SR.GetString("WebControls_Reset"), string.Empty, System.Design.SR.GetString("WebControls_ResetDescriptionViews"), true));
                }
                else
                {
                    items.Add(new DesignerActionMethodItem(this, "ConvertToTemplate", System.Design.SR.GetString("WebControls_ConvertToTemplate"), string.Empty, System.Design.SR.GetString("WebControls_ConvertToTemplateDescriptionViews"), true));
                }
                items.Add(new DesignerActionMethodItem(this, "LaunchWebAdmin", System.Design.SR.GetString("Login_LaunchWebAdmin"), string.Empty, System.Design.SR.GetString("Login_LaunchWebAdminDescription"), true));
                return items;
            }

            public void LaunchWebAdmin()
            {
                this._designer.LaunchWebAdmin();
            }

            public void Reset()
            {
                this._designer.Reset();
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

            [TypeConverter(typeof(ChangePasswordViewTypeConverter))]
            public string View
            {
                get
                {
                    if (this._designer.CurrentView == ChangePasswordDesigner.ViewType.ChangePassword)
                    {
                        return System.Design.SR.GetString("ChangePassword_ChangePasswordView");
                    }
                    return System.Design.SR.GetString("ChangePassword_SuccessView");
                }
                set
                {
                    if (string.Compare(value, System.Design.SR.GetString("ChangePassword_ChangePasswordView"), StringComparison.Ordinal) == 0)
                    {
                        this._designer.CurrentView = ChangePasswordDesigner.ViewType.ChangePassword;
                    }
                    else if (string.Compare(value, System.Design.SR.GetString("ChangePassword_SuccessView"), StringComparison.Ordinal) == 0)
                    {
                        this._designer.CurrentView = ChangePasswordDesigner.ViewType.Success;
                    }
                    TypeDescriptor.Refresh(this._designer.Component);
                    this._designer.UpdateDesignTimeHtml();
                }
            }

            private class ChangePasswordViewTypeConverter : TypeConverter
            {
                public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
                {
                    return new TypeConverter.StandardValuesCollection(new string[] { System.Design.SR.GetString("ChangePassword_ChangePasswordView"), System.Design.SR.GetString("ChangePassword_SuccessView") });
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

        private sealed class ConvertToTemplateHelper : LoginDesignerUtil.GenericConvertToTemplateHelper<ChangePassword, ChangePasswordDesigner>
        {
            private static readonly string[] _persistedControlIDs = new string[] { 
                "UserName", "UserNameRequired", "CurrentPassword", "CurrentPasswordRequired", "NewPassword", "NewPasswordRequired", "NewPasswordRegExp", "ConfirmNewPassword", "ConfirmNewPasswordRequired", "NewPasswordCompare", "ChangePasswordPushButton", "ChangePasswordImageButton", "ChangePasswordLinkButton", "CancelPushButton", "CancelImageButton", "CancelLinkButton", 
                "ContinuePushButton", "ContinueImageButton", "ContinueLinkButton", "FailureText", "HelpLink", "CreateUserLink", "PasswordRecoveryLink", "EditProfileLink", "EditProfileLinkSuccess"
             };
            private static readonly string[] _persistedIfNotVisibleControlIDs = new string[] { "FailureText" };

            public ConvertToTemplateHelper(ChangePasswordDesigner designer, IDesignerHost designerHost) : base(designer, designerHost)
            {
            }

            protected override System.Web.UI.Control GetDefaultTemplateContents()
            {
                System.Web.UI.Control control = null;
                switch (base.Designer.CurrentView)
                {
                    case ChangePasswordDesigner.ViewType.ChangePassword:
                        control = base.Designer.ViewControl.Controls[0];
                        break;

                    case ChangePasswordDesigner.ViewType.Success:
                        control = base.Designer.ViewControl.Controls[1];
                        break;
                }
                return (Table) control.Controls[0];
            }

            protected override Style GetFailureTextStyle(ChangePassword control)
            {
                return control.FailureTextStyle;
            }

            protected override ITemplate GetTemplate(ChangePassword control)
            {
                return base.Designer.GetTemplate(control);
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

        private enum ViewType
        {
            ChangePassword,
            Success
        }
    }
}

