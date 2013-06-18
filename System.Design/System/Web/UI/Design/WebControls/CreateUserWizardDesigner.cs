namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class CreateUserWizardDesigner : WizardDesigner
    {
        private const string _answerID = "Answer";
        private const string _answerRequiredID = "AnswerRequired";
        private static DesignerAutoFormatCollection _autoFormats;
        private const string _cancelButtonButtonID = "CancelButtonButton";
        private const string _cancelButtonID = "CancelButton";
        private const string _cancelButtonImageButtonID = "CancelButtonImageButton";
        private const string _cancelButtonLinkButtonID = "CancelButtonLinkButton";
        private static readonly Hashtable _completeStepConverter;
        private const string _confirmPasswordID = "ConfirmPassword";
        private const string _confirmPasswordRequiredID = "ConfirmPasswordRequired";
        private const string _continueButtonButtonID = "ContinueButtonButton";
        private const string _continueButtonID = "ContinueButton";
        private const string _continueButtonImageButtonID = "ContinueButtonImageButton";
        private const string _continueButtonLinkButtonID = "ContinueButtonLinkButton";
        private const string _createUserButtonButtonID = "StepNextButtonButton";
        private const string _createUserButtonID = "StepNextButton";
        private const string _createUserButtonImageButtonID = "StepNextButtonImageButton";
        private const string _createUserButtonLinkButtonID = "StepNextButtonLinkButton";
        private const string _createUserNavigationTemplateName = "CreateUserNavigationTemplate";
        private CreateUserWizard _createUserWizard;
        private static readonly string[] _defaultCompleteStepProperties = new string[] { "CompleteSuccessText", "CompleteSuccessTextStyle", "ContinueButtonStyle", "ContinueButtonText", "ContinueButtonType", "ContinueButtonImageUrl", "EditProfileText", "EditProfileIconUrl", "EditProfileUrl" };
        private static readonly string[] _defaultCreateStepProperties = new string[] { 
            "AnswerLabelText", "ConfirmPasswordLabelText", "ConfirmPasswordCompareErrorMessage", "ConfirmPasswordRequiredErrorMessage", "EmailLabelText", "ErrorMessageStyle", "HelpPageIconUrl", "HelpPageText", "HelpPageUrl", "HyperLinkStyle", "InstructionText", "InstructionTextStyle", "LabelStyle", "PasswordHintText", "PasswordHintStyle", "PasswordLabelText", 
            "PasswordRequiredErrorMessage", "QuestionLabelText", "TextBoxStyle", "UserNameLabelText", "UserNameRequiredErrorMessage", "AnswerRequiredErrorMessage", "EmailRegularExpression", "EmailRegularExpressionErrorMessage", "EmailRequiredErrorMessage", "PasswordRegularExpression", "PasswordRegularExpressionErrorMessage", "QuestionRequiredErrorMessage", "ValidatorTextStyle"
         };
        private static readonly string[] _defaultCreateUserNavProperties = new string[] { "CancelButtonImageUrl", "CancelButtonType", "CancelButtonStyle", "CancelButtonText", "CreateUserButtonImageUrl", "CreateUserButtonType", "CreateUserButtonStyle", "CreateUserButtonText" };
        private const string _editProfileLinkID = "EditProfileLink";
        private const string _emailID = "Email";
        private const string _emailRegExpID = "EmailRegExp";
        private const string _emailRequiredID = "EmailRequired";
        private const string _helpLinkID = "HelpLink";
        private const string _passwordCompareID = "PasswordCompare";
        private const string _passwordID = "Password";
        private const string _passwordRegExpID = "PasswordRegExp";
        private const string _passwordRequiredID = "PasswordRequired";
        private static readonly string[] _persistedControlIDs = new string[] { 
            "UserName", "UserNameRequired", "Password", "PasswordRequired", "ConfirmPassword", "Email", "Question", "Answer", "ConfirmPasswordRequired", "PasswordRegExp", "EmailRegExp", "EmailRequired", "QuestionRequired", "AnswerRequired", "PasswordCompare", "CancelButton", 
            "ContinueButton", "StepNextButton", "ErrorMessage", "HelpLink", "EditProfileLink"
         };
        private static readonly Hashtable _persistedIDConverter = new Hashtable();
        private static readonly string[] _persistedIfNotVisibleControlIDs = new string[] { "ErrorMessage" };
        private const string _previousButtonButtonID = "StepPreviousButton";
        private const string _previousButtonID = "StepNextButton";
        private const string _previousButtonImageButtonID = "StepPreviousButtonImageButton";
        private const string _previousButtonLinkButtonID = "StepPreviousButtonLinkButton";
        private const string _questionID = "Question";
        private const string _questionRequiredID = "QuestionRequired";
        private const string _unknownErrorMessageID = "ErrorMessage";
        private const string _userNameID = "UserName";
        private const string _userNameRequiredID = "UserNameRequired";

        static CreateUserWizardDesigner()
        {
            _persistedIDConverter.Add("CancelButtonImageButton", "CancelButton");
            _persistedIDConverter.Add("CancelButtonButton", "CancelButton");
            _persistedIDConverter.Add("CancelButtonLinkButton", "CancelButton");
            _persistedIDConverter.Add("StepNextButtonImageButton", "StepNextButton");
            _persistedIDConverter.Add("StepNextButtonButton", "StepNextButton");
            _persistedIDConverter.Add("StepNextButtonLinkButton", "StepNextButton");
            _persistedIDConverter.Add("StepPreviousButtonImageButton", "StepNextButton");
            _persistedIDConverter.Add("StepPreviousButton", "StepNextButton");
            _persistedIDConverter.Add("StepPreviousButtonLinkButton", "StepNextButton");
            _completeStepConverter = new Hashtable();
            _completeStepConverter.Add("ContinueButtonImageButton", "ContinueButton");
            _completeStepConverter.Add("ContinueButtonButton", "ContinueButton");
            _completeStepConverter.Add("ContinueButtonLinkButton", "ContinueButton");
        }

        protected override void AddDesignerRegions(DesignerRegionCollection regions)
        {
            if (base.SupportsDesignerRegions)
            {
                if (this._createUserWizard.CreateUserStep == null)
                {
                    this.CreateChildControls();
                    if (this._createUserWizard.CreateUserStep == null)
                    {
                        return;
                    }
                }
                bool flag = this._createUserWizard.CreateUserStep.ContentTemplate == null;
                bool flag2 = this._createUserWizard.CompleteStep.ContentTemplate == null;
                foreach (WizardStepBase base2 in this._createUserWizard.WizardSteps)
                {
                    bool flag3 = (flag && (base2 is CreateUserWizardStep)) || (flag2 && (base2 is CompleteWizardStep));
                    DesignerRegion region = null;
                    if (!flag3)
                    {
                        if (base2 is TemplatedWizardStep)
                        {
                            TemplateDefinition templateDefinition = new TemplateDefinition(this, "ContentTemplate", this._createUserWizard, "ContentTemplate", base.TemplateStyleArray[5]);
                            region = new WizardStepTemplatedEditableRegion(templateDefinition, base2) {
                                EnsureSize = false
                            };
                        }
                        else
                        {
                            region = new WizardStepEditableRegion(this, base2);
                        }
                        region.Description = System.Design.SR.GetString("ContainerControlDesigner_RegionWatermark");
                    }
                    else
                    {
                        region = new WizardSelectableRegion(this, base.GetRegionName(base2), base2);
                    }
                    regions.Add(region);
                }
                foreach (WizardStepBase base3 in this._createUserWizard.WizardSteps)
                {
                    WizardSelectableRegion region2 = new WizardSelectableRegion(this, "Move to " + base.GetRegionName(base3), base3);
                    if (this._createUserWizard.ActiveStep == base3)
                    {
                        region2.Selected = true;
                    }
                    regions.Add(region2);
                }
            }
        }

        private void ApplyStylesToCustomizedStep(CreateUserWizard createUserWizard, Table table)
        {
            if (createUserWizard.ControlStyleCreated)
            {
                Style controlStyle = createUserWizard.ControlStyle;
                table.ForeColor = controlStyle.ForeColor;
                table.BackColor = controlStyle.BackColor;
                table.Font.CopyFrom(controlStyle.Font);
                table.Font.Size = new FontUnit(Unit.Percentage(100.0));
            }
            Style stepStyle = createUserWizard.StepStyle;
            if (!stepStyle.IsEmpty)
            {
                table.ForeColor = stepStyle.ForeColor;
                table.BackColor = stepStyle.BackColor;
                table.Font.CopyFrom(stepStyle.Font);
                table.Font.Size = new FontUnit(Unit.Percentage(100.0));
            }
        }

        private string ConvertNavigationTableToHtmlTable(Table table)
        {
            ((IControlDesignerAccessor) this._createUserWizard).GetDesignModeState();
            StringWriter writer = new StringWriter(CultureInfo.CurrentCulture);
            HtmlTextWriter writer2 = new HtmlTextWriter(writer);
            if (table.Width != Unit.Empty)
            {
                writer2.AddStyleAttribute(HtmlTextWriterStyle.Width, table.Width.ToString(CultureInfo.CurrentCulture));
            }
            if (table.Height != Unit.Empty)
            {
                writer2.AddStyleAttribute(HtmlTextWriterStyle.Height, table.Height.ToString(CultureInfo.CurrentCulture));
            }
            if (table.CellSpacing != 0)
            {
                writer2.AddAttribute(HtmlTextWriterAttribute.Cellspacing, table.CellSpacing.ToString(CultureInfo.CurrentCulture));
            }
            string str = "0";
            if (table.BorderWidth != Unit.Empty)
            {
                str = table.BorderWidth.ToString(CultureInfo.CurrentCulture);
            }
            writer2.AddAttribute(HtmlTextWriterAttribute.Border, str);
            writer2.RenderBeginTag(HtmlTextWriterTag.Table);
            ArrayList list = new ArrayList(table.Rows.Count);
            foreach (TableRow row in table.Rows)
            {
                if (row.Visible)
                {
                    ArrayList list2 = new ArrayList(row.Cells.Count);
                    foreach (TableCell cell in row.Cells)
                    {
                        if (cell.Visible && cell.HasControls())
                        {
                            ArrayList list3 = new ArrayList(cell.Controls.Count);
                            foreach (System.Web.UI.Control control in cell.Controls)
                            {
                                if (((control.Visible && ((!(control is Literal) || (control.ID == "ErrorMessage")) || (((Literal) control).Text.Length != 0))) && (!(control is HyperLink) || (((HyperLink) control).Text.Length != 0))) && (!(control is System.Web.UI.WebControls.Image) || (((System.Web.UI.WebControls.Image) control).ImageUrl.Length != 0)))
                                {
                                    list3.Add(control);
                                }
                            }
                            if (list3.Count > 0)
                            {
                                list2.Add(new CellControls(cell, list3));
                            }
                        }
                    }
                    if (list2.Count > 0)
                    {
                        list.Add(new RowCells(row, list2));
                    }
                }
            }
            foreach (RowCells cells in list)
            {
                switch (cells._row.HorizontalAlign)
                {
                    case HorizontalAlign.Center:
                        writer2.AddAttribute(HtmlTextWriterAttribute.Align, "center");
                        break;

                    case HorizontalAlign.Right:
                        writer2.AddAttribute(HtmlTextWriterAttribute.Align, "right");
                        break;
                }
                writer2.RenderBeginTag(HtmlTextWriterTag.Tr);
                foreach (CellControls controls in cells._cells)
                {
                    switch (controls._cell.HorizontalAlign)
                    {
                        case HorizontalAlign.Center:
                            writer2.AddAttribute(HtmlTextWriterAttribute.Align, "center");
                            break;

                        case HorizontalAlign.Right:
                            writer2.AddAttribute(HtmlTextWriterAttribute.Align, "right");
                            break;
                    }
                    writer2.AddAttribute(HtmlTextWriterAttribute.Colspan, controls._cell.ColumnSpan.ToString(CultureInfo.CurrentCulture));
                    StringBuilder builder = new StringBuilder();
                    foreach (System.Web.UI.Control control2 in controls._controls)
                    {
                        bool flag = control2.ID == "ErrorMessage";
                        if ((control2 is Literal) && !flag)
                        {
                            builder.Append(((Literal) control2).Text);
                        }
                        else
                        {
                            if (flag)
                            {
                                writer2.AddStyleAttribute(HtmlTextWriterStyle.Color, "Red");
                                control2.EnableViewState = false;
                            }
                            builder.Append(ControlPersister.PersistControl(control2));
                        }
                    }
                    writer2.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer2.Write(builder.ToString());
                    writer2.RenderEndTag();
                }
                writer2.RenderEndTag();
            }
            writer2.RenderEndTag();
            return writer.ToString();
        }

        private string ConvertTableToHtmlTable(Table originalTable, System.Web.UI.Control container)
        {
            return this.ConvertTableToHtmlTable(originalTable, container, null);
        }

        private string ConvertTableToHtmlTable(Table originalTable, System.Web.UI.Control container, IDictionary persistMap)
        {
            IList list = new ArrayList();
            foreach (System.Web.UI.Control control in originalTable.Controls)
            {
                list.Add(control);
            }
            Table child = new Table();
            foreach (System.Web.UI.Control control2 in list)
            {
                child.Controls.Add(control2);
            }
            if (originalTable.ControlStyleCreated)
            {
                child.ApplyStyle(originalTable.ControlStyle);
            }
            child.Width = ((WebControl) base.ViewControl).Width;
            child.Height = ((WebControl) base.ViewControl).Height;
            if (container != null)
            {
                container.Controls.Add(child);
                container.Controls.Remove(originalTable);
            }
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (persistMap != null)
            {
                foreach (string str in persistMap.Keys)
                {
                    System.Web.UI.Control control3 = child.FindControl(str);
                    if ((control3 != null) && control3.Visible)
                    {
                        control3.ID = (string) persistMap[str];
                        LiteralControl control4 = new LiteralControl(ControlPersister.PersistControl(control3, service));
                        control3.Parent.Controls.Add(control4);
                        control3.Parent.Controls.Remove(control3);
                    }
                }
            }
            foreach (string str3 in _persistedControlIDs)
            {
                System.Web.UI.Control control5 = child.FindControl(str3);
                if (control5 != null)
                {
                    if (Array.IndexOf<string>(_persistedIfNotVisibleControlIDs, str3) >= 0)
                    {
                        control5.Visible = true;
                        control5.Parent.Visible = true;
                        control5.Parent.Parent.Visible = true;
                    }
                    if (str3 == "ErrorMessage")
                    {
                        TableCell parent = (TableCell) control5.Parent;
                        parent.ForeColor = Color.Red;
                        parent.ApplyStyle(this._createUserWizard.ErrorMessageStyle);
                        control5.EnableViewState = false;
                    }
                    if (control5.Visible)
                    {
                        LiteralControl control6 = new LiteralControl(ControlPersister.PersistControl(control5, service));
                        control5.Parent.Controls.Add(control6);
                        control5.Parent.Controls.Remove(control5);
                    }
                }
            }
            StringWriter writer = new StringWriter(CultureInfo.CurrentCulture);
            HtmlTextWriter writer2 = new HtmlTextWriter(writer);
            child.RenderControl(writer2);
            return writer.ToString();
        }

        protected override void ConvertToCustomNavigationTemplate()
        {
            try
            {
                if (this._createUserWizard.ActiveStep == this._createUserWizard.CreateUserStep)
                {
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    ITemplate customNavigationTemplate = ((CreateUserWizard) base.ViewControl).CreateUserStep.CustomNavigationTemplate;
                    if (customNavigationTemplate == null)
                    {
                        System.Web.UI.ControlCollection controls = ((IControlDesignerAccessor) this._createUserWizard).GetDesignModeState()["CustomNavigationControls"] as System.Web.UI.ControlCollection;
                        if (controls != null)
                        {
                            string templateText = string.Empty;
                            foreach (System.Web.UI.Control control in controls)
                            {
                                if ((control != null) && control.Visible)
                                {
                                    foreach (string str2 in _persistedIDConverter.Keys)
                                    {
                                        System.Web.UI.Control control2 = control.FindControl(str2);
                                        if ((control2 != null) && control2.Visible)
                                        {
                                            control2.ID = (string) _persistedIDConverter[str2];
                                        }
                                    }
                                    if (control is Table)
                                    {
                                        templateText = templateText + this.ConvertNavigationTableToHtmlTable((Table) control);
                                    }
                                    else
                                    {
                                        StringWriter writer = new StringWriter(CultureInfo.CurrentCulture);
                                        HtmlTextWriter writer2 = new HtmlTextWriter(writer);
                                        control.RenderControl(writer2);
                                        templateText = templateText + writer.ToString();
                                    }
                                }
                            }
                            customNavigationTemplate = ControlParser.ParseTemplate(service, templateText);
                        }
                    }
                    ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ConvertToCustomNavigationTemplateCallBack), customNavigationTemplate, System.Design.SR.GetString("Wizard_ConvertToCustomNavigationTemplate"));
                    this.UpdateDesignTimeHtml();
                }
                else
                {
                    base.ConvertToCustomNavigationTemplate();
                }
            }
            catch (Exception)
            {
            }
        }

        private void CustomizeCompleteStep()
        {
            IComponent completeStep = this._createUserWizard.CompleteStep;
            PropertyDescriptor member = TypeDescriptor.GetProperties(base.Component)["ActiveStepIndex"];
            int index = this._createUserWizard.WizardSteps.IndexOf(this._createUserWizard.CompleteStep);
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.NavigateToStep), index, System.Design.SR.GetString("CreateUserWizard_NavigateToStep", new object[] { index }), member);
            PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(completeStep)["ContentTemplate"];
            ControlDesigner.InvokeTransactedChange(base.Component.Site, completeStep, new TransactedChangeCallback(this.CustomizeCompleteStepCallback), null, System.Design.SR.GetString("CreateUserWizard_CustomizeCompleteStep"), descriptor2);
        }

        private bool CustomizeCompleteStepCallback(object context)
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            CreateUserWizard viewControl = (CreateUserWizard) base.ViewControl;
            ITemplate contentTemplate = viewControl.CompleteStep.ContentTemplate;
            if (contentTemplate == null)
            {
                try
                {
                    this.SetConvertToTemplateDesignModeState(true);
                    this.ViewControlCreated = false;
                    this.GetDesignTimeHtml();
                    viewControl = (CreateUserWizard) base.ViewControl;
                    IControlDesignerAccessor accessor = viewControl;
                    accessor.GetDesignModeState();
                    StringBuilder builder = new StringBuilder();
                    TemplatedWizardStep step = viewControl.CompleteStep;
                    Table styleTableForCustomizedStep = this.GetStyleTableForCustomizedStep(viewControl, step);
                    this.ApplyStylesToCustomizedStep(viewControl, styleTableForCustomizedStep);
                    builder.Append(this.ConvertTableToHtmlTable(styleTableForCustomizedStep, step.ContentTemplateContainer, _completeStepConverter));
                    contentTemplate = ControlParser.ParseTemplate(service, builder.ToString());
                    this.SetConvertToTemplateDesignModeState(false);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            IComponent completeStep = this._createUserWizard.CompleteStep;
            TypeDescriptor.GetProperties(completeStep)["ContentTemplate"].SetValue(completeStep, contentTemplate);
            this.UpdateDesignTimeHtml();
            return true;
        }

        private void CustomizeCreateUserStep()
        {
            IComponent createUserStep = this._createUserWizard.CreateUserStep;
            PropertyDescriptor member = TypeDescriptor.GetProperties(base.Component)["ActiveStepIndex"];
            int index = this._createUserWizard.WizardSteps.IndexOf(this._createUserWizard.CreateUserStep);
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.NavigateToStep), index, System.Design.SR.GetString("CreateUserWizard_NavigateToStep", new object[] { index }), member);
            PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(createUserStep)["ContentTemplate"];
            ControlDesigner.InvokeTransactedChange(base.Component.Site, createUserStep, new TransactedChangeCallback(this.CustomizeCreateUserStepCallback), null, System.Design.SR.GetString("CreateUserWizard_CustomizeCreateUserStep"), descriptor2);
        }

        private bool CustomizeCreateUserStepCallback(object context)
        {
            try
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                CreateUserWizard viewControl = (CreateUserWizard) base.ViewControl;
                ITemplate contentTemplate = viewControl.CreateUserStep.ContentTemplate;
                if (contentTemplate == null)
                {
                    this.ViewControlCreated = false;
                    this.SetConvertToTemplateDesignModeState(true);
                    this.GetDesignTimeHtml();
                    viewControl = (CreateUserWizard) base.ViewControl;
                    IControlDesignerAccessor accessor = viewControl;
                    accessor.GetDesignModeState();
                    StringBuilder builder = new StringBuilder();
                    TemplatedWizardStep step = viewControl.CreateUserStep;
                    Table styleTableForCustomizedStep = this.GetStyleTableForCustomizedStep(viewControl, step);
                    this.ApplyStylesToCustomizedStep(viewControl, styleTableForCustomizedStep);
                    builder.Append(this.ConvertTableToHtmlTable(styleTableForCustomizedStep, step.ContentTemplateContainer));
                    contentTemplate = ControlParser.ParseTemplate(service, builder.ToString());
                    this.SetConvertToTemplateDesignModeState(false);
                }
                IComponent createUserStep = this._createUserWizard.CreateUserStep;
                TypeDescriptor.GetProperties(createUserStep)["ContentTemplate"].SetValue(createUserStep, contentTemplate);
                this.UpdateDesignTimeHtml();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal override string GetEditableDesignerRegionContent(IWizardStepEditableRegion region)
        {
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            StringBuilder builder = new StringBuilder();
            if (((region.Step == this._createUserWizard.CreateUserStep) && (((CreateUserWizardStep) region.Step).ContentTemplate == null)) && (region.Step.Controls[0] is Table))
            {
                Table originalTable = (Table) ((Table) region.Step.Controls[0]).Rows[0].Cells[0].Controls[0];
                builder.Append(this.ConvertTableToHtmlTable(originalTable, ((TemplatedWizardStep) region.Step).ContentTemplateContainer));
                return builder.ToString();
            }
            if (((region.Step == this._createUserWizard.CompleteStep) && (((CompleteWizardStep) region.Step).ContentTemplate == null)) && (region.Step.Controls[0] is Table))
            {
                Table table2 = (Table) ((Table) region.Step.Controls[0]).Rows[0].Cells[0].Controls[0];
                builder.Append(this.ConvertTableToHtmlTable(table2, ((TemplatedWizardStep) region.Step).ContentTemplateContainer));
                return builder.ToString();
            }
            return base.GetEditableDesignerRegionContent(region);
        }

        protected override string GetErrorDesignTimeHtml(Exception e)
        {
            return base.CreatePlaceHolderDesignTimeHtml(System.Design.SR.GetString("Control_ErrorRenderingShort") + "<br />" + e.Message);
        }

        private Table GetStyleTableForCustomizedStep(CreateUserWizard createUserWizard, TemplatedWizardStep step)
        {
            if (createUserWizard.LayoutTemplate == null)
            {
                return (Table) ((Table) step.Controls[0].Controls[0]).Rows[0].Cells[0].Controls[0];
            }
            return (Table) step.Controls[0].Controls[0];
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(CreateUserWizard));
            this._createUserWizard = (CreateUserWizard) component;
            base.Initialize(component);
        }

        internal override bool InRegionEditingMode(Wizard viewControl)
        {
            return (!base.SupportsDesignerRegions || IsStepEmpty(this._createUserWizard.ActiveStep));
        }

        private static bool IsStepEmpty(WizardStepBase step)
        {
            if (!(step is CreateUserWizardStep) && !(step is CompleteWizardStep))
            {
                return false;
            }
            TemplatedWizardStep step2 = (TemplatedWizardStep) step;
            return (step2.ContentTemplate == null);
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

        private bool NavigateToStep(object context)
        {
            try
            {
                int num = (int) context;
                this._createUserWizard.ActiveStepIndex = num;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            TemplatedWizardStep createUserStep = this._createUserWizard.CreateUserStep;
            bool flag = (createUserStep != null) && (createUserStep.ContentTemplate != null);
            if (flag)
            {
                foreach (string str in _defaultCreateStepProperties)
                {
                    PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[str];
                    if (oldPropertyDescriptor != null)
                    {
                        properties[str] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { BrowsableAttribute.No });
                    }
                }
            }
            TemplatedWizardStep completeStep = this._createUserWizard.CompleteStep;
            bool flag2 = (completeStep != null) && (completeStep.ContentTemplate != null);
            if (flag2)
            {
                foreach (string str2 in _defaultCompleteStepProperties)
                {
                    PropertyDescriptor descriptor2 = (PropertyDescriptor) properties[str2];
                    if (descriptor2 != null)
                    {
                        properties[str2] = TypeDescriptor.CreateProperty(descriptor2.ComponentType, descriptor2, new Attribute[] { BrowsableAttribute.No });
                    }
                }
            }
            if ((createUserStep != null) && (createUserStep.CustomNavigationTemplate != null))
            {
                foreach (string str3 in _defaultCreateUserNavProperties)
                {
                    PropertyDescriptor descriptor3 = (PropertyDescriptor) properties[str3];
                    if (descriptor3 != null)
                    {
                        properties[str3] = TypeDescriptor.CreateProperty(descriptor3.ComponentType, descriptor3, new Attribute[] { BrowsableAttribute.No });
                    }
                }
            }
            if (flag2 && flag)
            {
                PropertyDescriptor descriptor4 = (PropertyDescriptor) properties["TitleTextStyle"];
                if (descriptor4 != null)
                {
                    properties["TitleTextStyle"] = TypeDescriptor.CreateProperty(descriptor4.ComponentType, descriptor4, new Attribute[] { BrowsableAttribute.No });
                }
            }
        }

        private bool ResetCallback(object context)
        {
            try
            {
                IComponent component = (IComponent) context;
                TypeDescriptor.GetProperties(component)["ContentTemplate"].SetValue(component, null);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ResetCompleteStep()
        {
            this.UpdateDesignTimeHtml();
            PropertyDescriptor member = TypeDescriptor.GetProperties(base.Component)["WizardSteps"];
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ResetCallback), this._createUserWizard.CompleteStep, System.Design.SR.GetString("CreateUserWizard_ResetCompleteStepVerb"), member);
        }

        private void ResetCreateUserStep()
        {
            this.UpdateDesignTimeHtml();
            PropertyDescriptor member = TypeDescriptor.GetProperties(base.Component)["WizardSteps"];
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ResetCallback), this._createUserWizard.CreateUserStep, System.Design.SR.GetString("CreateUserWizard_ResetCreateUserStepVerb"), member);
        }

        private void SetConvertToTemplateDesignModeState(bool value)
        {
            Hashtable data = new Hashtable(1);
            data.Add("ConvertToTemplate", value);
            ((IControlDesignerAccessor) base.ViewControl).SetDesignModeState(data);
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                lists.Add(new CreateUserWizardDesignerActionList(this));
                return lists;
            }
        }

        public override DesignerAutoFormatCollection AutoFormats
        {
            get
            {
                if (_autoFormats == null)
                {
                    _autoFormats = ControlDesigner.CreateAutoFormats(AutoFormatSchemes.CREATEUSERWIZARD_SCHEME_NAMES, schemeName => new CreateUserWizardAutoFormat(schemeName, "<Schemes>\r\n<xsd:schema id=\"Schemes\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n  <xsd:element name=\"Scheme\">\r\n     <xsd:complexType>\r\n       <xsd:all>\r\n        <xsd:element name=\"SchemeName\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderPadding\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"FontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"FontName\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TextLayout\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TitleTextBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TitleTextForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TitleTextFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TitleTextFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"InstructionTextForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"InstructionTextFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"TextboxFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"NavigationButtonStyleBorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"NavigationButtonStyleFontName\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"NavigationButtonStyleFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"NavigationButtonStyleBorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"NavigationButtonStyleBorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"NavigationButtonStyleForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"NavigationButtonStyleBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"StepStyleBorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"StepStyleBorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"StepStyleBorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"StepStyleForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"StepStyleBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"StepStyleFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SideBarButtonStyleFontUnderline\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SideBarButtonStyleFontName\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SideBarButtonStyleForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SideBarButtonStyleBorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SideBarButtonStyleBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderStyleForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderStyleBorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderStyleBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderStyleFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderStyleFontBold\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderStyleBorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderStyleHorizontalAlign\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderStyleBorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SideBarStyleBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SideBarStyleVerticalAlign\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SideBarStyleFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SideBarStyleFontUnderline\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SideBarStyleFontStrikeout\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SideBarStyleBorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n      </xsd:all>\r\n    </xsd:complexType>\r\n  </xsd:element>\r\n  <xsd:element name=\"Schemes\" msdata:IsDataSet=\"true\">\r\n    <xsd:complexType>\r\n      <xsd:choice maxOccurs=\"unbounded\">\r\n        <xsd:element ref=\"Scheme\"/>\r\n      </xsd:choice>\r\n    </xsd:complexType>\r\n  </xsd:element>\r\n</xsd:schema>\r\n<Scheme>\r\n  <SchemeName>CreateUserWizardScheme_Empty</SchemeName>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>CreateUserWizardScheme_Elegant</SchemeName>\r\n  <BackColor>#F7F7DE</BackColor>\r\n  <BorderColor>#CCCC99</BorderColor>\r\n  <BorderWidth>1</BorderWidth>\r\n  <BorderStyle>4</BorderStyle>\r\n  <FontSize>10</FontSize>\r\n  <FontName>Verdana</FontName>\r\n  <TitleTextBackColor>#6B696B</TitleTextBackColor>\r\n  <TitleTextForeColor>#FFFFFF</TitleTextForeColor>\r\n  <TitleTextFont>1</TitleTextFont>\r\n  <StepStyleBorderWidth>0px</StepStyleBorderWidth>\r\n  <NavigationButtonStyleBorderWidth>1px</NavigationButtonStyleBorderWidth>\r\n  <NavigationButtonStyleFontName>Verdana</NavigationButtonStyleFontName>\r\n  <NavigationButtonStyleBorderStyle>4</NavigationButtonStyleBorderStyle>\r\n  <NavigationButtonStyleBorderColor>#CCCCCC</NavigationButtonStyleBorderColor>\r\n  <NavigationButtonStyleForeColor>#284775</NavigationButtonStyleForeColor>\r\n  <NavigationButtonStyleBackColor>#FFFBFF</NavigationButtonStyleBackColor>\r\n  <SideBarButtonStyleFontUnderline>False</SideBarButtonStyleFontUnderline>\r\n  <SideBarButtonStyleFontName>Verdana</SideBarButtonStyleFontName>\r\n  <SideBarButtonStyleForeColor>#FFFFFF</SideBarButtonStyleForeColor>\r\n  <SideBarButtonStyleBorderWidth>0px</SideBarButtonStyleBorderWidth>\r\n  <HeaderStyleForeColor>#FFFFFF</HeaderStyleForeColor>\r\n  <HeaderStyleBackColor>#6B696B</HeaderStyleBackColor>\r\n  <HeaderStyleFontBold>True</HeaderStyleFontBold>\r\n  <HeaderStyleHorizontalAlign>2</HeaderStyleHorizontalAlign>\r\n  <SideBarStyleBackColor>#7C6F57</SideBarStyleBackColor>\r\n  <SideBarStyleVerticalAlign>1</SideBarStyleVerticalAlign>\r\n  <SideBarStyleFontSize>0.9em</SideBarStyleFontSize>\r\n  <SideBarStyleBorderWidth>0px</SideBarStyleBorderWidth>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>CreateUserWizardScheme_Professional</SchemeName>\r\n  <BackColor>#F7F6F3</BackColor>\r\n  <ForeColor>#333333</ForeColor>\r\n  <BorderColor>#E6E2D8</BorderColor>\r\n  <BorderWidth>1</BorderWidth>\r\n  <BorderStyle>4</BorderStyle>\r\n  <BorderPadding>4</BorderPadding>\r\n  <FontSize>0.8em</FontSize>\r\n  <FontName>Verdana</FontName>\r\n  <TitleTextBackColor>#5D7B9D</TitleTextBackColor>\r\n  <TitleTextForeColor>White</TitleTextForeColor>\r\n  <TitleTextFont>1</TitleTextFont>\r\n  <TitleTextFontSize>0.9em</TitleTextFontSize>\r\n  <InstructionTextForeColor>Black</InstructionTextForeColor>\r\n  <InstructionTextFont>2</InstructionTextFont>\r\n  <TextboxFontSize>0.8em</TextboxFontSize>\r\n  <StepStyleBorderWidth>0px</StepStyleBorderWidth>\r\n  <NavigationButtonStyleBorderWidth>1px</NavigationButtonStyleBorderWidth>\r\n  <NavigationButtonStyleFontName>Verdana</NavigationButtonStyleFontName>\r\n  <NavigationButtonStyleBorderStyle>4</NavigationButtonStyleBorderStyle>\r\n  <NavigationButtonStyleBorderColor>#CCCCCC</NavigationButtonStyleBorderColor>\r\n  <NavigationButtonStyleForeColor>#284775</NavigationButtonStyleForeColor>\r\n  <NavigationButtonStyleBackColor>#FFFBFF</NavigationButtonStyleBackColor>\r\n  <SideBarButtonStyleFontUnderline>False</SideBarButtonStyleFontUnderline>\r\n  <SideBarButtonStyleFontName>Verdana</SideBarButtonStyleFontName>\r\n  <SideBarButtonStyleForeColor>White</SideBarButtonStyleForeColor>\r\n  <SideBarButtonStyleBorderWidth>0px</SideBarButtonStyleBorderWidth>\r\n  <HeaderStyleForeColor>White</HeaderStyleForeColor>\r\n  <HeaderStyleBackColor>#5D7B9D</HeaderStyleBackColor>\r\n  <HeaderStyleFontSize>0.9em</HeaderStyleFontSize>\r\n  <HeaderStyleFontBold>True</HeaderStyleFontBold>\r\n  <HeaderStyleHorizontalAlign>2</HeaderStyleHorizontalAlign>\r\n  <HeaderStyleBorderStyle>4</HeaderStyleBorderStyle>\r\n  <SideBarStyleBackColor>#5D7B9D</SideBarStyleBackColor>\r\n  <SideBarStyleVerticalAlign>1</SideBarStyleVerticalAlign>\r\n  <SideBarStyleFontSize>0.9em</SideBarStyleFontSize>\r\n  <SideBarStyleBorderWidth>0px</SideBarStyleBorderWidth>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>CreateUserWizardScheme_Simple</SchemeName>\r\n  <BackColor>#E3EAEB</BackColor>\r\n  <ForeColor>#333333</ForeColor>\r\n  <BorderColor>#E6E2D8</BorderColor>\r\n  <BorderWidth>1</BorderWidth>\r\n  <BorderStyle>4</BorderStyle>\r\n  <BorderPadding>4</BorderPadding>\r\n  <FontSize>0.8em</FontSize>\r\n  <FontName>Verdana</FontName>\r\n  <TextLayout>1</TextLayout>\r\n  <TitleTextBackColor>#1C5E55</TitleTextBackColor>\r\n  <TitleTextForeColor>White</TitleTextForeColor>\r\n  <TitleTextFont>1</TitleTextFont>\r\n  <TitleTextFontSize>0.9em</TitleTextFontSize>\r\n  <InstructionTextForeColor>Black</InstructionTextForeColor>\r\n  <InstructionTextFont>2</InstructionTextFont>\r\n  <TextboxFontSize>0.8em</TextboxFontSize>\r\n  <StepStyleBorderWidth>0px</StepStyleBorderWidth>\r\n  <NavigationButtonStyleBorderWidth>1px</NavigationButtonStyleBorderWidth>\r\n  <NavigationButtonStyleFontName>Verdana</NavigationButtonStyleFontName>\r\n  <NavigationButtonStyleBorderStyle>4</NavigationButtonStyleBorderStyle>\r\n  <NavigationButtonStyleBorderColor>#C5BBAF</NavigationButtonStyleBorderColor>\r\n  <NavigationButtonStyleForeColor>#1C5E55</NavigationButtonStyleForeColor>\r\n  <NavigationButtonStyleBackColor>White</NavigationButtonStyleBackColor>\r\n  <SideBarButtonStyleFontUnderline>False</SideBarButtonStyleFontUnderline>\r\n  <SideBarButtonStyleForeColor>White</SideBarButtonStyleForeColor>\r\n  <HeaderStyleForeColor>White</HeaderStyleForeColor>\r\n  <HeaderStyleBackColor>#666666</HeaderStyleBackColor>\r\n  <HeaderStyleBorderColor>#E6E2D8</HeaderStyleBorderColor>\r\n  <HeaderStyleFontSize>0.9em</HeaderStyleFontSize>\r\n  <HeaderStyleFontBold>True</HeaderStyleFontBold>\r\n  <HeaderStyleHorizontalAlign>2</HeaderStyleHorizontalAlign>\r\n  <HeaderStyleBorderStyle>4</HeaderStyleBorderStyle>\r\n  <HeaderStyleBorderWidth>2px</HeaderStyleBorderWidth>\r\n  <SideBarStyleBackColor>#1C5E55</SideBarStyleBackColor>\r\n  <SideBarStyleVerticalAlign>1</SideBarStyleVerticalAlign>\r\n  <SideBarStyleFontSize>0.9em</SideBarStyleFontSize>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>CreateUserWizardScheme_Classic</SchemeName>\r\n  <BackColor>#EFF3FB</BackColor>\r\n  <ForeColor>#333333</ForeColor>\r\n  <BorderColor>#B5C7DE</BorderColor>\r\n  <BorderWidth>1</BorderWidth>\r\n  <BorderStyle>4</BorderStyle>\r\n  <BorderPadding>4</BorderPadding>\r\n  <FontSize>0.8em</FontSize>\r\n  <FontName>Verdana</FontName>\r\n  <TitleTextBackColor>#507CD1</TitleTextBackColor>\r\n  <TitleTextForeColor>White</TitleTextForeColor>\r\n  <TitleTextFont>1</TitleTextFont>\r\n  <TitleTextFontSize>0.9em</TitleTextFontSize>\r\n  <InstructionTextForeColor>Black</InstructionTextForeColor>\r\n  <InstructionTextFont>2</InstructionTextFont>\r\n  <TextboxFontSize>0.8em</TextboxFontSize>\r\n  <StepStyleFontSize>0.8em</StepStyleFontSize>\r\n  <NavigationButtonStyleBorderWidth>1px</NavigationButtonStyleBorderWidth>\r\n  <NavigationButtonStyleFontName>Verdana</NavigationButtonStyleFontName>\r\n  <NavigationButtonStyleBorderStyle>4</NavigationButtonStyleBorderStyle>\r\n  <NavigationButtonStyleBorderColor>#507CD1</NavigationButtonStyleBorderColor>\r\n  <NavigationButtonStyleForeColor>#284E98</NavigationButtonStyleForeColor>\r\n  <NavigationButtonStyleBackColor>White</NavigationButtonStyleBackColor>\r\n  <SideBarButtonStyleFontUnderline>False</SideBarButtonStyleFontUnderline>\r\n  <SideBarButtonStyleFontName>Verdana</SideBarButtonStyleFontName>\r\n  <SideBarButtonStyleForeColor>White</SideBarButtonStyleForeColor>\r\n  <SideBarButtonStyleBackColor>#507CD1</SideBarButtonStyleBackColor>\r\n  <HeaderStyleForeColor>White</HeaderStyleForeColor>\r\n  <HeaderStyleBorderColor>#EFF3FB</HeaderStyleBorderColor>\r\n  <HeaderStyleBackColor>#284E98</HeaderStyleBackColor>\r\n  <HeaderStyleFontSize>0.9em</HeaderStyleFontSize>\r\n  <HeaderStyleFontBold>True</HeaderStyleFontBold>\r\n  <HeaderStyleBorderWidth>2px</HeaderStyleBorderWidth>\r\n  <HeaderStyleHorizontalAlign>2</HeaderStyleHorizontalAlign>\r\n  <HeaderStyleBorderStyle>4</HeaderStyleBorderStyle>\r\n  <SideBarStyleBackColor>#507CD1</SideBarStyleBackColor>\r\n  <SideBarStyleVerticalAlign>1</SideBarStyleVerticalAlign>\r\n  <SideBarStyleFontSize>0.9em</SideBarStyleFontSize>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>CreateUserWizardScheme_Colorful</SchemeName>\r\n  <BackColor>#FFFBD6</BackColor>\r\n  <ForeColor>#333333</ForeColor>\r\n  <BorderColor>#FFDFAD</BorderColor>\r\n  <BorderWidth>1</BorderWidth>\r\n  <BorderStyle>4</BorderStyle>\r\n  <BorderPadding>4</BorderPadding>\r\n  <FontSize>0.8em</FontSize>\r\n  <FontName>Verdana</FontName>\r\n  <TextLayout>1</TextLayout>\r\n  <TitleTextBackColor>#990000</TitleTextBackColor>\r\n  <TitleTextForeColor>White</TitleTextForeColor>\r\n  <TitleTextFont>1</TitleTextFont>\r\n  <TitleTextFontSize>0.9em</TitleTextFontSize>\r\n  <InstructionTextForeColor>Black</InstructionTextForeColor>\r\n  <InstructionTextFont>2</InstructionTextFont>\r\n  <TextboxFontSize>0.8em</TextboxFontSize>\r\n  <NavigationButtonStyleBorderWidth>1px</NavigationButtonStyleBorderWidth>\r\n  <NavigationButtonStyleFontName>Verdana</NavigationButtonStyleFontName>\r\n  <NavigationButtonStyleBorderStyle>4</NavigationButtonStyleBorderStyle>\r\n  <NavigationButtonStyleBorderColor>#CC9966</NavigationButtonStyleBorderColor>\r\n  <NavigationButtonStyleForeColor>#990000</NavigationButtonStyleForeColor>\r\n  <NavigationButtonStyleBackColor>White</NavigationButtonStyleBackColor>\r\n  <SideBarButtonStyleFontUnderline>False</SideBarButtonStyleFontUnderline>\r\n  <SideBarButtonStyleForeColor>White</SideBarButtonStyleForeColor>\r\n  <HeaderStyleForeColor>#333333</HeaderStyleForeColor>\r\n  <HeaderStyleBorderColor>#FFFBD6</HeaderStyleBorderColor>\r\n  <HeaderStyleBackColor>#FFCC66</HeaderStyleBackColor>\r\n  <HeaderStyleFontSize>0.9em</HeaderStyleFontSize>\r\n  <HeaderStyleFontBold>True</HeaderStyleFontBold>\r\n  <HeaderStyleBorderWidth>2px</HeaderStyleBorderWidth>\r\n  <HeaderStyleHorizontalAlign>2</HeaderStyleHorizontalAlign>\r\n  <HeaderStyleBorderStyle>4</HeaderStyleBorderStyle>\r\n  <SideBarStyleBackColor>#990000</SideBarStyleBackColor>\r\n  <SideBarStyleVerticalAlign>1</SideBarStyleVerticalAlign>\r\n  <SideBarStyleFontSize>0.9em</SideBarStyleFontSize>\r\n  <SideBarStyleFontUnderline>False</SideBarStyleFontUnderline>\r\n</Scheme>\r\n</Schemes>\r\n"));
                }
                return _autoFormats;
            }
        }

        protected override bool UsePreviewControl
        {
            get
            {
                return true;
            }
        }

        private class CellControls
        {
            internal TableCell _cell;
            internal ArrayList _controls;

            internal CellControls(TableCell cell, ArrayList controls)
            {
                this._cell = cell;
                this._controls = controls;
            }
        }

        private class CreateUserWizardDesignerActionList : DesignerActionList
        {
            private CreateUserWizardDesigner _parent;

            public CreateUserWizardDesignerActionList(CreateUserWizardDesigner parent) : base(parent.Component)
            {
                this._parent = parent;
            }

            public void CustomizeCompleteStep()
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    this._parent.CustomizeCompleteStep();
                }
                finally
                {
                    Cursor.Current = current;
                }
            }

            public void CustomizeCreateUserStep()
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    this._parent.CustomizeCreateUserStep();
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
                if (this._parent._createUserWizard.CreateUserStep.ContentTemplate == null)
                {
                    items.Add(new DesignerActionMethodItem(this, "CustomizeCreateUserStep", System.Design.SR.GetString("CreateUserWizard_CustomizeCreateUserStep"), string.Empty, System.Design.SR.GetString("CreateUserWizard_CustomizeCreateUserStepDescription"), true));
                }
                else
                {
                    items.Add(new DesignerActionMethodItem(this, "ResetCreateUserStep", System.Design.SR.GetString("CreateUserWizard_ResetCreateUserStepVerb"), string.Empty, System.Design.SR.GetString("CreateUserWizard_ResetCreateUserStepVerbDescription"), true));
                }
                if (this._parent._createUserWizard.CompleteStep.ContentTemplate == null)
                {
                    items.Add(new DesignerActionMethodItem(this, "CustomizeCompleteStep", System.Design.SR.GetString("CreateUserWizard_CustomizeCompleteStep"), string.Empty, System.Design.SR.GetString("CreateUserWizard_CustomizeCompleteStepDescription"), true));
                }
                else
                {
                    items.Add(new DesignerActionMethodItem(this, "ResetCompleteStep", System.Design.SR.GetString("CreateUserWizard_ResetCompleteStepVerb"), string.Empty, System.Design.SR.GetString("CreateUserWizard_ResetCompleteStepVerbDescription"), true));
                }
                items.Add(new DesignerActionMethodItem(this, "LaunchWebAdmin", System.Design.SR.GetString("Login_LaunchWebAdmin"), string.Empty, System.Design.SR.GetString("Login_LaunchWebAdminDescription"), true));
                return items;
            }

            public void LaunchWebAdmin()
            {
                this._parent.LaunchWebAdmin();
            }

            public void ResetCompleteStep()
            {
                this._parent.ResetCompleteStep();
            }

            public void ResetCreateUserStep()
            {
                this._parent.ResetCreateUserStep();
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

        private class RowCells
        {
            internal ArrayList _cells;
            internal TableRow _row;

            internal RowCells(TableRow row, ArrayList cells)
            {
                this._row = row;
                this._cells = cells;
            }
        }
    }
}

