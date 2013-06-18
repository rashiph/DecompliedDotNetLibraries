namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    public class WizardStepEditableRegion : EditableDesignerRegion, IWizardStepEditableRegion
    {
        private WizardStepBase _wizardStep;

        public WizardStepEditableRegion(WizardDesigner designer, WizardStepBase wizardStep) : base(designer, designer.GetRegionName(wizardStep), false)
        {
            this._wizardStep = wizardStep;
            base.EnsureSize = true;
        }

        public WizardStepBase Step
        {
            get
            {
                return this._wizardStep;
            }
        }
    }
}

