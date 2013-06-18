namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    internal class WizardSelectableRegion : DesignerRegion
    {
        private WizardStepBase _wizardStep;

        internal WizardSelectableRegion(WizardDesigner designer, string name, WizardStepBase wizardStep) : base(designer, name, true)
        {
            this._wizardStep = wizardStep;
        }

        internal WizardStepBase Step
        {
            get
            {
                return this._wizardStep;
            }
        }
    }
}

