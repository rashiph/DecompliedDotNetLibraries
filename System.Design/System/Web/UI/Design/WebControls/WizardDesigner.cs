namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class WizardDesigner : CompositeControlDesigner
    {
        private const string _activeStepIndexPropName = "ActiveStepIndex";
        private const string _activeStepIndexTransactionDescription = "Update ActiveStepIndex";
        private DesignerAutoFormatCollection _autoFormats;
        private const string _cancelButtonID = "CancelButton";
        internal const string _contentTemplateName = "ContentTemplate";
        private static string[] _controlTemplateNames = new string[] { "HeaderTemplate", "SideBarTemplate", "StartNavigationTemplate", "StepNavigationTemplate", "FinishNavigationTemplate" };
        internal const string _customNavigationControls = "CustomNavigationControls";
        internal const string _customNavigationTemplateName = "CustomNavigationTemplate";
        private const string _dataListID = "SideBarList";
        private const string _displaySideBarPropName = "DisplaySideBar";
        private const string _finishButtonID = "FinishButton";
        private static string[] _finishButtonIDs = new string[] { "FinishPreviousButton", "FinishButton", "CancelButton" };
        private const string _finishNavigationTemplateName = "FinishNavigationTemplate";
        private static readonly string[] _finishNavigationTemplateProperties = new string[] { "FinishCompleteButtonText", "FinishCompleteButtonType", "FinishCompleteButtonImageUrl", "FinishPreviousButtonText", "FinishPreviousButtonType", "FinishPreviousButtonImageUrl", "FinishCompleteButtonStyle", "FinishPreviousButtonStyle" };
        private const string _finishPreviousButtonID = "FinishPreviousButton";
        private static readonly string[] _generalNavigationButtonProperties = new string[] { "CancelButtonImageUrl", "CancelButtonText", "CancelButtonType", "DisplayCancelButton", "CancelButtonStyle", "NavigationButtonStyle" };
        private static readonly string[] _headerProperties = new string[] { "HeaderText" };
        private const string _headerTemplateName = "HeaderTemplate";
        internal const int _navigationStyleLength = 6;
        private const string _navigationTemplateName = "CustomNavigationTemplate";
        private const string _sideBarButtonID = "SideBarButton";
        private static readonly string[] _sideBarProperties = new string[] { "SideBarButtonStyle" };
        private const string _sideBarTemplateName = "SideBarTemplate";
        private static string[] _startButtonIDs = new string[] { "StartNextButton", "CancelButton" };
        private const string _startNavigationTemplateName = "StartNavigationTemplate";
        private static readonly string[] _startNavigationTemplateProperties = new string[] { "StartNextButtonText", "StartNextButtonType", "StartNextButtonImageUrl", "StartNextButtonStyle" };
        private const string _startNextButtonID = "StartNextButton";
        private static string[] _stepButtonIDs = new string[] { "StepPreviousButton", "StepNextButton", "CancelButton" };
        private const string _stepNavigationTemplateName = "StepNavigationTemplate";
        private static readonly string[] _stepNavigationTemplateProperties = new string[] { "StepNextButtonText", "StepNextButtonType", "StepNextButtonImageUrl", "StepPreviousButtonText", "StepPreviousButtonType", "StepPreviousButtonImageUrl", "StepPreviousButtonStyle", "StepNextButtonStyle" };
        private const string _stepNextButtonID = "StepNextButton";
        private const string _stepPreviousButtonID = "StepPreviousButton";
        private const string _stepTableCellID = "StepTableCell";
        private static string[] _stepTemplateNames = new string[] { "ContentTemplate", "CustomNavigationTemplate" };
        private bool _supportsDesignerRegion;
        private bool _supportsDesignerRegionQueried;
        private Wizard _wizard;
        private const string _wizardStepsPropertyName = "WizardSteps";

        protected virtual void AddDesignerRegions(DesignerRegionCollection regions)
        {
            if (this.SupportsDesignerRegions)
            {
                foreach (WizardStepBase base2 in this._wizard.WizardSteps)
                {
                    if (base2 is TemplatedWizardStep)
                    {
                        TemplateDefinition templateDefinition = new TemplateDefinition(this, "ContentTemplate", this._wizard, "ContentTemplate", this.TemplateStyleArray[5]);
                        DesignerRegion region = new WizardStepTemplatedEditableRegion(templateDefinition, base2) {
                            Description = System.Design.SR.GetString("ContainerControlDesigner_RegionWatermark")
                        };
                        regions.Add(region);
                    }
                    else
                    {
                        DesignerRegion region2 = new WizardStepEditableRegion(this, base2) {
                            Description = System.Design.SR.GetString("ContainerControlDesigner_RegionWatermark")
                        };
                        regions.Add(region2);
                    }
                }
                foreach (WizardStepBase base3 in this._wizard.WizardSteps)
                {
                    regions.Add(new WizardSelectableRegion(this, "Move to " + this.GetRegionName(base3), base3));
                }
            }
        }

        protected virtual void ConvertToCustomNavigationTemplate()
        {
            try
            {
                ITemplate context = null;
                string description = System.Design.SR.GetString("Wizard_ConvertToCustomNavigationTemplate");
                TemplatedWizardStep activeStep = this.ActiveStep as TemplatedWizardStep;
                if (activeStep != null)
                {
                    TemplatedWizardStep step2 = ((Wizard) base.ViewControl).ActiveStep as TemplatedWizardStep;
                    if ((step2 != null) && (step2.CustomNavigationTemplate != null))
                    {
                        context = step2.CustomNavigationTemplate;
                    }
                    else
                    {
                        switch (this._wizard.GetStepType(activeStep, this.ActiveStepIndex))
                        {
                            case WizardStepType.Finish:
                                context = this.GetTemplateFromDesignModeState(_finishButtonIDs);
                                break;

                            case WizardStepType.Start:
                                context = this.GetTemplateFromDesignModeState(_startButtonIDs);
                                break;

                            case WizardStepType.Step:
                                context = this.GetTemplateFromDesignModeState(_stepButtonIDs);
                                break;
                        }
                    }
                    ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ConvertToCustomNavigationTemplateCallBack), context, description);
                }
            }
            catch (Exception)
            {
            }
        }

        internal bool ConvertToCustomNavigationTemplateCallBack(object context)
        {
            ITemplate template = (ITemplate) context;
            TemplatedWizardStep activeStep = this.ActiveStep as TemplatedWizardStep;
            activeStep.CustomNavigationTemplate = template;
            return true;
        }

        private void ConvertToFinishNavigationTemplate()
        {
            this.ConvertToTemplate(System.Design.SR.GetString("Wizard_ConvertToFinishNavigationTemplate"), base.Component, "FinishNavigationTemplate", _finishButtonIDs);
        }

        private void ConvertToSideBarTemplate()
        {
            this.ConvertToTemplate(System.Design.SR.GetString("Wizard_ConvertToSideBarTemplate"), base.Component, "SideBarTemplate", new string[] { "SideBarList" });
        }

        private void ConvertToStartNavigationTemplate()
        {
            this.ConvertToTemplate(System.Design.SR.GetString("Wizard_ConvertToStartNavigationTemplate"), base.Component, "StartNavigationTemplate", _startButtonIDs);
        }

        private void ConvertToStepNavigationTemplate()
        {
            this.ConvertToTemplate(System.Design.SR.GetString("Wizard_ConvertToStepNavigationTemplate"), base.Component, "StepNavigationTemplate", _stepButtonIDs);
        }

        protected void ConvertToTemplate(string description, IComponent component, string templateName, string[] keys)
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ConvertToTemplateCallBack), new Triplet(component, templateName, keys), description);
            this.UpdateDesignTimeHtml();
        }

        private bool ConvertToTemplateCallBack(object context)
        {
            Triplet triplet = (Triplet) context;
            IComponent first = (IComponent) triplet.First;
            string second = (string) triplet.Second;
            string[] third = (string[]) triplet.Third;
            TypeDescriptor.GetProperties(first)[second].SetValue(first, this.GetTemplateFromDesignModeState(third));
            return true;
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Wizard viewControl = (Wizard) base.ViewControl;
            if ((viewControl.ActiveStepIndex == -1) && (viewControl.WizardSteps.Count > 0))
            {
                viewControl.ActiveStepIndex = 0;
            }
            IControlDesignerAccessor accessor = viewControl;
            IDictionary designModeState = accessor.GetDesignModeState();
            TemplatedWizardStep activeStep = viewControl.ActiveStep as TemplatedWizardStep;
            if (((activeStep == null) || (activeStep.ContentTemplate == null)) || (((TemplatedWizardStep) this._wizard.WizardSteps[viewControl.ActiveStepIndex]).ContentTemplate != null))
            {
                TableCell cell = designModeState["StepTableCell"] as TableCell;
                if ((cell != null) && (viewControl.ActiveStepIndex != -1))
                {
                    cell.Attributes["_designerRegion"] = viewControl.ActiveStepIndex.ToString(NumberFormatInfo.InvariantInfo);
                }
            }
        }

        private void DataListItemDataBound(object sender, WizardSideBarListControlItemEventArgs e)
        {
            WizardSideBarListControlItem item = e.Item;
            WebControl control = item.FindControl("SideBarButton") as WebControl;
            if (control != null)
            {
                control.Attributes["_designerRegion"] = (item.ItemIndex + ((Wizard) base.ViewControl).WizardSteps.Count).ToString(NumberFormatInfo.InvariantInfo);
            }
        }

        public override string GetDesignTimeHtml()
        {
            string designTimeHtml = null;
            if (this.ActiveStepIndex != -1)
            {
                Wizard viewControl = (Wizard) base.ViewControl;
                IControlDesignerAccessor accessor = viewControl;
                IWizardSideBarListControl control = accessor.GetDesignModeState()["SideBarList"] as IWizardSideBarListControl;
                if (control != null)
                {
                    control.ItemDataBound += new EventHandler<WizardSideBarListControlItemEventArgs>(this.DataListItemDataBound);
                    ICompositeControlDesignerAccessor accessor2 = viewControl;
                    accessor2.RecreateChildControls();
                }
                ArrayList list = new ArrayList(viewControl.WizardSteps.Count);
                foreach (WizardStepBase base2 in viewControl.WizardSteps)
                {
                    list.Add(base2.Title);
                    if (((base2.Title == null) || (base2.Title.Length == 0)) && ((base2.ID == null) || (base2.ID.Length == 0)))
                    {
                        base2.Title = this.GetRegionName(base2);
                    }
                }
                if (!this.InRegionEditingMode(viewControl))
                {
                    viewControl.Enabled = true;
                }
                designTimeHtml = base.GetDesignTimeHtml();
                if ((designTimeHtml != null) && (designTimeHtml.Length != 0))
                {
                    return designTimeHtml;
                }
            }
            return this.GetEmptyDesignTimeHtml();
        }

        public override string GetDesignTimeHtml(DesignerRegionCollection regions)
        {
            this.AddDesignerRegions(regions);
            IControlDesignerAccessor accessor = this._wizard;
            IDictionary dictionary = null;
            try
            {
                dictionary = accessor.GetDesignModeState();
            }
            catch (Exception exception)
            {
                return this.GetErrorDesignTimeHtml(exception);
            }
            IWizardSideBarListControl control = dictionary["SideBarList"] as IWizardSideBarListControl;
            if (control != null)
            {
                control.ItemDataBound += new EventHandler<WizardSideBarListControlItemEventArgs>(this.DataListItemDataBound);
            }
            Wizard viewControl = (Wizard) base.ViewControl;
            IControlDesignerAccessor accessor2 = viewControl;
            IDictionary designModeState = accessor2.GetDesignModeState();
            if (designModeState != null)
            {
                designModeState["ShouldRenderWizardSteps"] = this.InRegionEditingMode(viewControl);
            }
            return this.GetDesignTimeHtml();
        }

        public override string GetEditableDesignerRegionContent(EditableDesignerRegion region)
        {
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            IWizardStepEditableRegion region2 = region as IWizardStepEditableRegion;
            if (region2 == null)
            {
                throw new ArgumentException(System.Design.SR.GetString("Wizard_InvalidRegion"));
            }
            return this.GetEditableDesignerRegionContent(region2);
        }

        internal virtual string GetEditableDesignerRegionContent(IWizardStepEditableRegion region)
        {
            StringBuilder builder = new StringBuilder();
            ControlCollection controls = region.Step.Controls;
            IDesignerHost service = (IDesignerHost) base.Component.Site.GetService(typeof(IDesignerHost));
            if (region.Step is TemplatedWizardStep)
            {
                TemplatedWizardStep step = (TemplatedWizardStep) region.Step;
                return ControlPersister.PersistTemplate(step.ContentTemplate, service);
            }
            if ((controls.Count == 1) && (controls[0] is LiteralControl))
            {
                string text = ((LiteralControl) controls[0]).Text;
                if ((text == null) || (text.Trim().Length == 0))
                {
                    return string.Empty;
                }
            }
            foreach (Control control in controls)
            {
                builder.Append(ControlPersister.PersistControl(control, service));
            }
            return builder.ToString();
        }

        internal string GetRegionName(WizardStepBase step)
        {
            if ((step.Title != null) && (step.Title.Length > 0))
            {
                return step.Title;
            }
            if ((step.ID != null) && (step.ID.Length > 0))
            {
                return step.ID;
            }
            int num = step.Wizard.WizardSteps.IndexOf(step) + 1;
            return ("[step (" + num + ")]");
        }

        private ITemplate GetTemplateFromDesignModeState(string[] keys)
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            IDictionary designModeState = ((IControlDesignerAccessor) this._wizard).GetDesignModeState();
            this.ResetInternalControls(designModeState);
            string templateText = string.Empty;
            foreach (string str2 in keys)
            {
                Control control = designModeState[str2] as Control;
                if ((control != null) && control.Visible)
                {
                    control.ID = str2;
                    templateText = templateText + ControlPersister.PersistControl(control, service);
                }
            }
            return ControlParser.ParseTemplate(service, templateText);
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(Wizard));
            this._wizard = (Wizard) component;
            base.Initialize(component);
            base.SetViewFlags(ViewFlags.TemplateEditing, true);
        }

        internal virtual bool InRegionEditingMode(Wizard viewControl)
        {
            if (!this.SupportsDesignerRegions)
            {
                return true;
            }
            TemplatedWizardStep activeStep = this.ActiveStep as TemplatedWizardStep;
            if ((activeStep != null) && (activeStep.ContentTemplate == null))
            {
                TemplatedWizardStep step2 = viewControl.WizardSteps[this.ActiveStepIndex] as TemplatedWizardStep;
                if ((step2 != null) && (step2.ContentTemplate != null))
                {
                    return true;
                }
            }
            return false;
        }

        private void MarkPropertyNonBrowsable(IDictionary properties, string propName)
        {
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[propName];
            if (oldPropertyDescriptor != null)
            {
                properties[propName] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { BrowsableAttribute.No });
            }
        }

        protected override void OnClick(DesignerRegionMouseEventArgs e)
        {
            base.OnClick(e);
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            WizardSelectableRegion region = e.Region as WizardSelectableRegion;
            if (region != null)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this._wizard)["ActiveStepIndex"];
                int index = this._wizard.WizardSteps.IndexOf(region.Step);
                if (this.ActiveStepIndex != index)
                {
                    using (DesignerTransaction transaction = service.CreateTransaction("Update ActiveStepIndex"))
                    {
                        descriptor.SetValue(base.Component, index);
                        transaction.Commit();
                    }
                }
            }
        }

        public override void OnComponentChanged(object sender, ComponentChangedEventArgs ce)
        {
            if (((ce != null) && (ce.Member != null)) && ((ce.Member.Name == "WizardSteps") && (this._wizard.ActiveStepIndex >= this._wizard.WizardSteps.Count)))
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                using (DesignerTransaction transaction = service.CreateTransaction("Update ActiveStepIndex"))
                {
                    TypeDescriptor.GetProperties(this._wizard)["ActiveStepIndex"].SetValue(base.Component, this._wizard.WizardSteps.Count - 1);
                    transaction.Commit();
                }
            }
            base.OnComponentChanged(sender, ce);
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["DisplaySideBar"];
            if (oldPropertyDescriptor != null)
            {
                properties["DisplaySideBar"] = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, null);
            }
            if (base.InTemplateMode)
            {
                this.MarkPropertyNonBrowsable(properties, "WizardSteps");
            }
            if (this._wizard.StartNavigationTemplate != null)
            {
                foreach (string str in _startNavigationTemplateProperties)
                {
                    this.MarkPropertyNonBrowsable(properties, str);
                }
            }
            if (this._wizard.StepNavigationTemplate != null)
            {
                foreach (string str2 in _stepNavigationTemplateProperties)
                {
                    this.MarkPropertyNonBrowsable(properties, str2);
                }
            }
            if (this._wizard.FinishNavigationTemplate != null)
            {
                foreach (string str3 in _finishNavigationTemplateProperties)
                {
                    this.MarkPropertyNonBrowsable(properties, str3);
                }
            }
            if (((this._wizard.StartNavigationTemplate != null) && (this._wizard.StepNavigationTemplate != null)) && (this._wizard.FinishNavigationTemplate != null))
            {
                foreach (string str4 in _generalNavigationButtonProperties)
                {
                    this.MarkPropertyNonBrowsable(properties, str4);
                }
            }
            if (this._wizard.HeaderTemplate != null)
            {
                foreach (string str5 in _headerProperties)
                {
                    this.MarkPropertyNonBrowsable(properties, str5);
                }
            }
            if (this._wizard.SideBarTemplate != null)
            {
                foreach (string str6 in _sideBarProperties)
                {
                    this.MarkPropertyNonBrowsable(properties, str6);
                }
            }
        }

        private void ResetCustomNavigationTemplate()
        {
            WizardStepBase activeStep = this.ActiveStep;
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ResetCustomNavigationTemplateCallBack), null, System.Design.SR.GetString("Wizard_ResetCustomNavigationTemplate"));
        }

        private bool ResetCustomNavigationTemplateCallBack(object context)
        {
            WizardStepBase activeStep = this.ActiveStep;
            TypeDescriptor.GetProperties(activeStep)["CustomNavigationTemplate"].ResetValue(activeStep);
            return true;
        }

        private void ResetFinishNavigationTemplate()
        {
            this.ResetTemplate(System.Design.SR.GetString("Wizard_ResetFinishNavigationTemplate"), base.Component, "FinishNavigationTemplate");
        }

        private void ResetInternalControls(IDictionary dictionary)
        {
            IWizardSideBarListControl control = (IWizardSideBarListControl) dictionary["SideBarList"];
            if (control != null)
            {
                control.SelectedIndex = -1;
            }
        }

        private void ResetSideBarTemplate()
        {
            this.ResetTemplate(System.Design.SR.GetString("Wizard_ResetSideBarTemplate"), base.Component, "SideBarTemplate");
        }

        private void ResetStartNavigationTemplate()
        {
            this.ResetTemplate(System.Design.SR.GetString("Wizard_ResetStartNavigationTemplate"), base.Component, "StartNavigationTemplate");
        }

        private void ResetStepNavigationTemplate()
        {
            this.ResetTemplate(System.Design.SR.GetString("Wizard_ResetStepNavigationTemplate"), base.Component, "StepNavigationTemplate");
        }

        protected void ResetTemplate(string description, IComponent component, string templateName)
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ResetTemplateCallBack), new Pair(component, templateName), description);
            this.UpdateDesignTimeHtml();
        }

        private bool ResetTemplateCallBack(object context)
        {
            Pair pair = (Pair) context;
            IComponent first = (IComponent) pair.First;
            string second = (string) pair.Second;
            TypeDescriptor.GetProperties(first)[second].ResetValue(first);
            return true;
        }

        public override void SetEditableDesignerRegionContent(EditableDesignerRegion region, string content)
        {
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            IWizardStepEditableRegion region2 = region as IWizardStepEditableRegion;
            if (region2 == null)
            {
                throw new ArgumentException(System.Design.SR.GetString("Wizard_InvalidRegion"));
            }
            IDesignerHost service = (IDesignerHost) base.Component.Site.GetService(typeof(IDesignerHost));
            if (region2.Step is TemplatedWizardStep)
            {
                IComponent step = region2.Step;
                ITemplate template = ControlParser.ParseTemplate(service, content);
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(step)["ContentTemplate"];
                using (DesignerTransaction transaction = service.CreateTransaction("SetEditableDesignerRegionContent"))
                {
                    descriptor.SetValue(step, template);
                    transaction.Commit();
                }
                this.ViewControlCreated = false;
            }
            else
            {
                this.SetWizardStepContent(region2.Step, content, service);
            }
        }

        private void SetWizardStepContent(WizardStepBase step, string content, IDesignerHost host)
        {
            Control[] controlArray = null;
            if ((content != null) && (content.Length > 0))
            {
                controlArray = ControlParser.ParseControls(host, content);
            }
            step.Controls.Clear();
            if (controlArray != null)
            {
                foreach (Control control in controlArray)
                {
                    step.Controls.Add(control);
                }
            }
        }

        private void StartWizardStepCollectionEditor()
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            PropertyDescriptor propDesc = TypeDescriptor.GetProperties(base.Component)["WizardSteps"];
            using (DesignerTransaction transaction = service.CreateTransaction(System.Design.SR.GetString("Wizard_StartWizardStepCollectionEditor")))
            {
                UITypeEditor editor = (UITypeEditor) propDesc.GetEditor(typeof(UITypeEditor));
                if (editor.EditValue(new System.Web.UI.Design.WebControls.TypeDescriptorContext(service, propDesc, base.Component), new WindowsFormsEditorServiceHelper(this), propDesc.GetValue(base.Component)) != null)
                {
                    transaction.Commit();
                }
            }
            if ((this._wizard.ActiveStepIndex >= -1) && (this._wizard.ActiveStepIndex < this._wizard.WizardSteps.Count))
            {
                try
                {
                    this.ViewControlCreated = false;
                    this.CreateChildControls();
                }
                catch
                {
                }
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                lists.Add(new WizardDesignerActionList(this));
                return lists;
            }
        }

        internal WizardStepBase ActiveStep
        {
            get
            {
                if (this.ActiveStepIndex != -1)
                {
                    return this._wizard.WizardSteps[this.ActiveStepIndex];
                }
                return null;
            }
        }

        internal int ActiveStepIndex
        {
            get
            {
                int activeStepIndex = this._wizard.ActiveStepIndex;
                if ((activeStepIndex == -1) && (this._wizard.WizardSteps.Count > 0))
                {
                    return 0;
                }
                return activeStepIndex;
            }
        }

        public override DesignerAutoFormatCollection AutoFormats
        {
            get
            {
                if (this._autoFormats == null)
                {
                    this._autoFormats = ControlDesigner.CreateAutoFormats(AutoFormatSchemes.WIZARD_SCHEME_NAMES, schemeName => new WizardAutoFormat(schemeName, "<Schemes>\r\n        <xsd:schema id=\"Schemes\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n          <xsd:element name=\"Scheme\">\r\n            <xsd:complexType>\r\n              <xsd:all>\r\n                <xsd:element name=\"SchemeName\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FontName\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"NavigationButtonStyleBorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"NavigationButtonStyleFontName\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"NavigationButtonStyleFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"NavigationButtonStyleBorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"NavigationButtonStyleBorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"NavigationButtonStyleForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"NavigationButtonStyleBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"StepStyleBorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"StepStyleBorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"StepStyleBorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"StepStyleForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"StepStyleBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"StepStyleFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SideBarButtonStyleFontUnderline\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SideBarButtonStyleFontName\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SideBarButtonStyleForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SideBarButtonStyleBorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SideBarButtonStyleBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderStyleForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderStyleBorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderStyleBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderStyleFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderStyleFontBold\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderStyleBorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderStyleHorizontalAlign\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderStyleBorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SideBarStyleBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SideBarStyleVerticalAlign\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SideBarStyleFontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SideBarStyleFontUnderline\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SideBarStyleFontStrikeout\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SideBarStyleBorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n              </xsd:all>\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n          <xsd:element name=\"Schemes\" msdata:IsDataSet=\"true\">\r\n            <xsd:complexType>\r\n              <xsd:choice maxOccurs=\"unbounded\">\r\n                <xsd:element ref=\"Scheme\"/>\r\n              </xsd:choice>\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n        </xsd:schema>\r\n        <Scheme>\r\n          <SchemeName>WizardAFmt_Scheme_Default</SchemeName>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>WizardAFmt_Scheme_Colorful</SchemeName>\r\n          <FontName>Verdana</FontName>\r\n          <FontSize>0.8em</FontSize>\r\n          <BackColor>#FFFBD6</BackColor>\r\n          <BorderColor>#FFDFAD</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <NavigationButtonStyleBorderWidth>1px</NavigationButtonStyleBorderWidth>\r\n          <NavigationButtonStyleFontName>Verdana</NavigationButtonStyleFontName>\r\n          <NavigationButtonStyleFontSize>0.8em</NavigationButtonStyleFontSize>\r\n          <NavigationButtonStyleBorderStyle>4</NavigationButtonStyleBorderStyle>\r\n          <NavigationButtonStyleBorderColor>#CC9966</NavigationButtonStyleBorderColor>\r\n          <NavigationButtonStyleForeColor>#990000</NavigationButtonStyleForeColor>\r\n          <NavigationButtonStyleBackColor>White</NavigationButtonStyleBackColor>\r\n          <SideBarButtonStyleFontUnderline>False</SideBarButtonStyleFontUnderline>\r\n          <SideBarButtonStyleForeColor>White</SideBarButtonStyleForeColor>\r\n          <HeaderStyleForeColor>#333333</HeaderStyleForeColor>\r\n          <HeaderStyleBorderColor>#FFFBD6</HeaderStyleBorderColor>\r\n          <HeaderStyleBackColor>#FFCC66</HeaderStyleBackColor>\r\n          <HeaderStyleFontSize>0.9em</HeaderStyleFontSize>\r\n          <HeaderStyleFontBold>True</HeaderStyleFontBold>\r\n          <HeaderStyleBorderWidth>2px</HeaderStyleBorderWidth>\r\n          <HeaderStyleHorizontalAlign>2</HeaderStyleHorizontalAlign>\r\n          <HeaderStyleBorderStyle>4</HeaderStyleBorderStyle>\r\n          <SideBarStyleBackColor>#990000</SideBarStyleBackColor>\r\n          <SideBarStyleVerticalAlign>1</SideBarStyleVerticalAlign>\r\n          <SideBarStyleFontSize>0.9em</SideBarStyleFontSize>\r\n          <SideBarStyleFontUnderline>False</SideBarStyleFontUnderline>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>WizardAFmt_Scheme_Professional</SchemeName>\r\n          <FontName>Verdana</FontName>\r\n          <FontSize>0.8em</FontSize>\r\n          <BackColor>#F7F6F3</BackColor>\r\n          <BorderColor>#CCCCCC</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>4</BorderStyle>\r\n          <StepStyleForeColor>#5D7B9D</StepStyleForeColor>\r\n          <StepStyleBorderWidth>0px</StepStyleBorderWidth>\r\n          <NavigationButtonStyleBorderWidth>1px</NavigationButtonStyleBorderWidth>\r\n          <NavigationButtonStyleFontName>Verdana</NavigationButtonStyleFontName>\r\n          <NavigationButtonStyleFontSize>0.8em</NavigationButtonStyleFontSize>\r\n          <NavigationButtonStyleBorderStyle>4</NavigationButtonStyleBorderStyle>\r\n          <NavigationButtonStyleBorderColor>#CCCCCC</NavigationButtonStyleBorderColor>\r\n          <NavigationButtonStyleForeColor>#284775</NavigationButtonStyleForeColor>\r\n          <NavigationButtonStyleBackColor>#FFFBFF</NavigationButtonStyleBackColor>\r\n          <SideBarButtonStyleFontUnderline>False</SideBarButtonStyleFontUnderline>\r\n          <SideBarButtonStyleFontName>Verdana</SideBarButtonStyleFontName>\r\n          <SideBarButtonStyleForeColor>White</SideBarButtonStyleForeColor>\r\n          <SideBarButtonStyleBorderWidth>0px</SideBarButtonStyleBorderWidth>\r\n          <HeaderStyleForeColor>White</HeaderStyleForeColor>\r\n          <HeaderStyleBackColor>#5D7B9D</HeaderStyleBackColor>\r\n          <HeaderStyleFontSize>0.9em</HeaderStyleFontSize>\r\n          <HeaderStyleFontBold>True</HeaderStyleFontBold>\r\n          <HeaderStyleHorizontalAlign>1</HeaderStyleHorizontalAlign>\r\n          <HeaderStyleBorderStyle>4</HeaderStyleBorderStyle>\r\n          <SideBarStyleBackColor>#7C6F57</SideBarStyleBackColor>\r\n          <SideBarStyleVerticalAlign>1</SideBarStyleVerticalAlign>\r\n          <SideBarStyleFontSize>0.9em</SideBarStyleFontSize>\r\n          <SideBarStyleBorderWidth>0px</SideBarStyleBorderWidth>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>WizardAFmt_Scheme_Classic</SchemeName>\r\n          <FontName>Verdana</FontName>\r\n          <FontSize>0.8em</FontSize>\r\n          <BackColor>#EFF3FB</BackColor>\r\n          <BorderColor>#B5C7DE</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <StepStyleForeColor>#333333</StepStyleForeColor>\r\n          <StepStyleFontSize>0.8em</StepStyleFontSize>\r\n          <NavigationButtonStyleBorderWidth>1px</NavigationButtonStyleBorderWidth>\r\n          <NavigationButtonStyleFontName>Verdana</NavigationButtonStyleFontName>\r\n          <NavigationButtonStyleFontSize>0.8em</NavigationButtonStyleFontSize>\r\n          <NavigationButtonStyleBorderStyle>4</NavigationButtonStyleBorderStyle>\r\n          <NavigationButtonStyleBorderColor>#507CD1</NavigationButtonStyleBorderColor>\r\n          <NavigationButtonStyleForeColor>#284E98</NavigationButtonStyleForeColor>\r\n          <NavigationButtonStyleBackColor>White</NavigationButtonStyleBackColor>\r\n          <SideBarButtonStyleFontUnderline>False</SideBarButtonStyleFontUnderline>\r\n          <SideBarButtonStyleFontName>Verdana</SideBarButtonStyleFontName>\r\n          <SideBarButtonStyleForeColor>White</SideBarButtonStyleForeColor>\r\n          <SideBarButtonStyleBackColor>#507CD1</SideBarButtonStyleBackColor>\r\n          <HeaderStyleForeColor>White</HeaderStyleForeColor>\r\n          <HeaderStyleBorderColor>#EFF3FB</HeaderStyleBorderColor>\r\n          <HeaderStyleBackColor>#284E98</HeaderStyleBackColor>\r\n          <HeaderStyleFontSize>0.9em</HeaderStyleFontSize>\r\n          <HeaderStyleFontBold>True</HeaderStyleFontBold>\r\n          <HeaderStyleBorderWidth>2px</HeaderStyleBorderWidth>\r\n          <HeaderStyleHorizontalAlign>2</HeaderStyleHorizontalAlign>\r\n          <HeaderStyleBorderStyle>4</HeaderStyleBorderStyle>\r\n          <SideBarStyleBackColor>#507CD1</SideBarStyleBackColor>\r\n          <SideBarStyleVerticalAlign>1</SideBarStyleVerticalAlign>\r\n          <SideBarStyleFontSize>0.9em</SideBarStyleFontSize>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>WizardAFmt_Scheme_Simple</SchemeName>\r\n          <FontName>Verdana</FontName>\r\n          <FontSize>0.8em</FontSize>\r\n          <BackColor>#E6E2D8</BackColor>\r\n          <BorderColor>#999999</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>4</BorderStyle>\r\n          <StepStyleBorderStyle>4</StepStyleBorderStyle>\r\n          <StepStyleBorderColor>#E6E2D8</StepStyleBorderColor>\r\n          <StepStyleBackColor>#F7F6F3</StepStyleBackColor>\r\n          <StepStyleBorderWidth>2px</StepStyleBorderWidth>\r\n          <NavigationButtonStyleBorderWidth>1px</NavigationButtonStyleBorderWidth>\r\n          <NavigationButtonStyleFontName>Verdana</NavigationButtonStyleFontName>\r\n          <NavigationButtonStyleFontSize>0.8em</NavigationButtonStyleFontSize>\r\n          <NavigationButtonStyleBorderStyle>4</NavigationButtonStyleBorderStyle>\r\n          <NavigationButtonStyleBorderColor>#C5BBAF</NavigationButtonStyleBorderColor>\r\n          <NavigationButtonStyleForeColor>#1C5E55</NavigationButtonStyleForeColor>\r\n          <NavigationButtonStyleBackColor>White</NavigationButtonStyleBackColor>\r\n          <SideBarButtonStyleFontUnderline>False</SideBarButtonStyleFontUnderline>\r\n          <SideBarButtonStyleForeColor>White</SideBarButtonStyleForeColor>\r\n          <HeaderStyleForeColor>White</HeaderStyleForeColor>\r\n          <HeaderStyleBackColor>#666666</HeaderStyleBackColor>\r\n          <HeaderStyleBorderColor>#E6E2D8</HeaderStyleBorderColor>\r\n          <HeaderStyleFontSize>0.9em</HeaderStyleFontSize>\r\n          <HeaderStyleFontBold>True</HeaderStyleFontBold>\r\n          <HeaderStyleHorizontalAlign>2</HeaderStyleHorizontalAlign>\r\n          <HeaderStyleBorderStyle>4</HeaderStyleBorderStyle>\r\n          <HeaderStyleBorderWidth>2px</HeaderStyleBorderWidth>\r\n          <SideBarStyleBackColor>#1C5E55</SideBarStyleBackColor>\r\n          <SideBarStyleVerticalAlign>1</SideBarStyleVerticalAlign>\r\n          <SideBarStyleFontSize>0.9em</SideBarStyleFontSize>\r\n        </Scheme>\r\n      </Schemes>"));
                }
                return this._autoFormats;
            }
        }

        protected bool DisplaySideBar
        {
            get
            {
                return ((Wizard) base.Component).DisplaySideBar;
            }
            set
            {
                TypeDescriptor.Refresh(base.Component);
                ((Wizard) base.Component).DisplaySideBar = value;
                TypeDescriptor.Refresh(base.Component);
            }
        }

        private Style[] StepTemplateStyleArray
        {
            get
            {
                Style style = new Style();
                Wizard viewControl = (Wizard) base.ViewControl;
                style.CopyFrom(viewControl.ControlStyle);
                style.CopyFrom(viewControl.StepStyle);
                Style style2 = new Style();
                style2.CopyFrom(viewControl.ControlStyle);
                style2.CopyFrom(viewControl.NavigationStyle);
                return new Style[] { style, style2 };
            }
        }

        internal bool SupportsDesignerRegions
        {
            get
            {
                if (!this._supportsDesignerRegionQueried)
                {
                    if (base.View != null)
                    {
                        this._supportsDesignerRegion = base.View.SupportsRegions;
                    }
                    this._supportsDesignerRegionQueried = true;
                }
                return (this._supportsDesignerRegion && (this._wizard.LayoutTemplate == null));
            }
        }

        public override TemplateGroupCollection TemplateGroups
        {
            get
            {
                TemplateGroupCollection templateGroups = base.TemplateGroups;
                for (int i = 0; i < _controlTemplateNames.Length; i++)
                {
                    string groupName = _controlTemplateNames[i];
                    TemplateGroup group = new TemplateGroup(groupName);
                    group.AddTemplateDefinition(new TemplateDefinition(this, groupName, this._wizard, groupName, this.TemplateStyleArray[i]));
                    templateGroups.Add(group);
                }
                foreach (WizardStepBase base2 in this._wizard.WizardSteps)
                {
                    string regionName = this.GetRegionName(base2);
                    TemplateGroup group2 = new TemplateGroup(regionName);
                    if (base2 is TemplatedWizardStep)
                    {
                        for (int j = 0; j < _stepTemplateNames.Length; j++)
                        {
                            group2.AddTemplateDefinition(new TemplateDefinition(this, _stepTemplateNames[j], base2, _stepTemplateNames[j], this.StepTemplateStyleArray[j]));
                        }
                    }
                    else if (!this.SupportsDesignerRegions)
                    {
                        group2.AddTemplateDefinition(new WizardStepBaseTemplateDefinition(this, base2, regionName, this.StepTemplateStyleArray[0]));
                    }
                    if (!group2.IsEmpty)
                    {
                        templateGroups.Add(group2);
                    }
                }
                return templateGroups;
            }
        }

        internal Style[] TemplateStyleArray
        {
            get
            {
                Style style = new Style();
                Wizard viewControl = (Wizard) base.ViewControl;
                style.CopyFrom(viewControl.ControlStyle);
                style.CopyFrom(viewControl.HeaderStyle);
                Style style2 = new Style();
                style2.CopyFrom(viewControl.ControlStyle);
                style2.CopyFrom(viewControl.SideBarStyle);
                Style style3 = new Style();
                style3.CopyFrom(viewControl.ControlStyle);
                style3.CopyFrom(viewControl.NavigationStyle);
                return new Style[] { style, style2, style3, style3, style3, style3 };
            }
        }

        protected override bool UsePreviewControl
        {
            get
            {
                return true;
            }
        }

        private class WizardDesignerActionList : DesignerActionList
        {
            private WizardDesigner _designer;

            public WizardDesignerActionList(WizardDesigner designer) : base(designer.Component)
            {
                this._designer = designer;
            }

            public void ConvertToCustomNavigationTemplate()
            {
                this._designer.ConvertToCustomNavigationTemplate();
            }

            public void ConvertToFinishNavigationTemplate()
            {
                this._designer.ConvertToFinishNavigationTemplate();
            }

            public void ConvertToSideBarTemplate()
            {
                this._designer.ConvertToSideBarTemplate();
            }

            public void ConvertToStartNavigationTemplate()
            {
                this._designer.ConvertToStartNavigationTemplate();
            }

            public void ConvertToStepNavigationTemplate()
            {
                this._designer.ConvertToStepNavigationTemplate();
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                if (!this._designer.InTemplateMode)
                {
                    if (this._designer._wizard.WizardSteps.Count > 0)
                    {
                        items.Add(new DesignerActionPropertyItem("View", System.Design.SR.GetString("Wizard_StepsView"), string.Empty, System.Design.SR.GetString("Wizard_StepsViewDescription")));
                    }
                    items.Add(new DesignerActionMethodItem(this, "StartWizardStepCollectionEditor", System.Design.SR.GetString("Wizard_StartWizardStepCollectionEditor"), string.Empty, System.Design.SR.GetString("Wizard_StartWizardStepCollectionEditorDescription"), true));
                    Wizard wizard = this._designer._wizard;
                    int activeStepIndex = this._designer.ActiveStepIndex;
                    if ((activeStepIndex < 0) || (activeStepIndex >= wizard.WizardSteps.Count))
                    {
                        return items;
                    }
                    if (wizard.StartNavigationTemplate != null)
                    {
                        items.Add(new DesignerActionMethodItem(this, "ResetStartNavigationTemplate", System.Design.SR.GetString("Wizard_ResetStartNavigationTemplate"), string.Empty, System.Design.SR.GetString("Wizard_ResetDescription", new object[] { "StartNavigation" }), true));
                    }
                    else
                    {
                        items.Add(new DesignerActionMethodItem(this, "ConvertToStartNavigationTemplate", System.Design.SR.GetString("Wizard_ConvertToStartNavigationTemplate"), string.Empty, System.Design.SR.GetString("Wizard_ConvertToTemplateDescription", new object[] { "StartNavigation" }), true));
                    }
                    if (wizard.StepNavigationTemplate != null)
                    {
                        items.Add(new DesignerActionMethodItem(this, "ResetStepNavigationTemplate", System.Design.SR.GetString("Wizard_ResetStepNavigationTemplate"), string.Empty, System.Design.SR.GetString("Wizard_ResetDescription", new object[] { "StepNavigation" }), true));
                    }
                    else
                    {
                        items.Add(new DesignerActionMethodItem(this, "ConvertToStepNavigationTemplate", System.Design.SR.GetString("Wizard_ConvertToStepNavigationTemplate"), string.Empty, System.Design.SR.GetString("Wizard_ConvertToTemplateDescription", new object[] { "StepNavigation" }), true));
                    }
                    if (wizard.FinishNavigationTemplate != null)
                    {
                        items.Add(new DesignerActionMethodItem(this, "ResetFinishNavigationTemplate", System.Design.SR.GetString("Wizard_ResetFinishNavigationTemplate"), string.Empty, System.Design.SR.GetString("Wizard_ResetDescription", new object[] { "FinishNavigation" }), true));
                    }
                    else
                    {
                        items.Add(new DesignerActionMethodItem(this, "ConvertToFinishNavigationTemplate", System.Design.SR.GetString("Wizard_ConvertToFinishNavigationTemplate"), string.Empty, System.Design.SR.GetString("Wizard_ConvertToTemplateDescription", new object[] { "FinishNavigation" }), true));
                    }
                    if (wizard.DisplaySideBar)
                    {
                        if (wizard.SideBarTemplate != null)
                        {
                            items.Add(new DesignerActionMethodItem(this, "ResetSideBarTemplate", System.Design.SR.GetString("Wizard_ResetSideBarTemplate"), string.Empty, System.Design.SR.GetString("Wizard_ResetDescription", new object[] { "SideBar" }), true));
                        }
                        else
                        {
                            items.Add(new DesignerActionMethodItem(this, "ConvertToSideBarTemplate", System.Design.SR.GetString("Wizard_ConvertToSideBarTemplate"), string.Empty, System.Design.SR.GetString("Wizard_ConvertToTemplateDescription", new object[] { "SideBar" }), true));
                        }
                    }
                    TemplatedWizardStep activeStep = this._designer.ActiveStep as TemplatedWizardStep;
                    if ((activeStep == null) || (activeStep.StepType == WizardStepType.Complete))
                    {
                        return items;
                    }
                    if (activeStep.CustomNavigationTemplate != null)
                    {
                        items.Add(new DesignerActionMethodItem(this, "ResetCustomNavigationTemplate", System.Design.SR.GetString("Wizard_ResetCustomNavigationTemplate"), string.Empty, System.Design.SR.GetString("Wizard_ResetDescription", new object[] { "CustomNavigation" }), true));
                        return items;
                    }
                    items.Add(new DesignerActionMethodItem(this, "ConvertToCustomNavigationTemplate", System.Design.SR.GetString("Wizard_ConvertToCustomNavigationTemplate"), string.Empty, System.Design.SR.GetString("Wizard_ConvertToTemplateDescription", new object[] { "CustomNavigation" }), true));
                }
                return items;
            }

            public void ResetCustomNavigationTemplate()
            {
                this._designer.ResetCustomNavigationTemplate();
            }

            public void ResetFinishNavigationTemplate()
            {
                this._designer.ResetFinishNavigationTemplate();
            }

            public void ResetSideBarTemplate()
            {
                this._designer.ResetSideBarTemplate();
            }

            public void ResetStartNavigationTemplate()
            {
                this._designer.ResetStartNavigationTemplate();
            }

            public void ResetStepNavigationTemplate()
            {
                this._designer.ResetStepNavigationTemplate();
            }

            public void StartWizardStepCollectionEditor()
            {
                this._designer.StartWizardStepCollectionEditor();
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

            [TypeConverter(typeof(WizardStepTypeConverter))]
            public int View
            {
                get
                {
                    return this._designer.ActiveStepIndex;
                }
                set
                {
                    if (value != this._designer.ActiveStepIndex)
                    {
                        IDesignerHost service = (IDesignerHost) this._designer.GetService(typeof(IDesignerHost));
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this._designer.Component)["ActiveStepIndex"];
                        using (DesignerTransaction transaction = service.CreateTransaction(System.Design.SR.GetString("Wizard_OnViewChanged")))
                        {
                            descriptor.SetValue(this._designer.Component, value);
                            transaction.Commit();
                        }
                        this._designer.UpdateDesignTimeHtml();
                        TypeDescriptor.Refresh(this._designer.Component);
                    }
                }
            }

            private class WizardStepTypeConverter : TypeConverter
            {
                public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                {
                    return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
                }

                public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
                {
                    return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
                }

                public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
                {
                    if (value is string)
                    {
                        WizardDesigner.WizardDesignerActionList instance = (WizardDesigner.WizardDesignerActionList) context.Instance;
                        WizardDesigner designer = instance._designer;
                        WizardStepCollection wizardSteps = designer._wizard.WizardSteps;
                        for (int i = 0; i < wizardSteps.Count; i++)
                        {
                            if (string.Compare(designer.GetRegionName(wizardSteps[i]), (string) value, StringComparison.Ordinal) == 0)
                            {
                                return i;
                            }
                        }
                    }
                    return base.ConvertFrom(context, culture, value);
                }

                public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
                {
                    if (destinationType == typeof(string))
                    {
                        if (value is string)
                        {
                            return value;
                        }
                        WizardDesigner.WizardDesignerActionList instance = (WizardDesigner.WizardDesignerActionList) context.Instance;
                        WizardDesigner designer = instance._designer;
                        WizardStepCollection wizardSteps = designer._wizard.WizardSteps;
                        if (value is int)
                        {
                            int num = (int) value;
                            if ((num == -1) && (wizardSteps.Count > 0))
                            {
                                num = 0;
                            }
                            if (num >= wizardSteps.Count)
                            {
                                return null;
                            }
                            return designer.GetRegionName(wizardSteps[num]);
                        }
                    }
                    return base.ConvertTo(context, culture, value, destinationType);
                }

                public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
                {
                    int[] values = null;
                    if (context != null)
                    {
                        WizardDesigner.WizardDesignerActionList instance = (WizardDesigner.WizardDesignerActionList) context.Instance;
                        WizardStepCollection wizardSteps = instance._designer._wizard.WizardSteps;
                        values = new int[wizardSteps.Count];
                        for (int i = 0; i < wizardSteps.Count; i++)
                        {
                            values[i] = i;
                        }
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
    }
}

