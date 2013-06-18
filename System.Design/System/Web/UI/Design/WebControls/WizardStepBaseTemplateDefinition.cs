namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    internal class WizardStepBaseTemplateDefinition : TemplateDefinition
    {
        private WizardStepBase _step;

        public WizardStepBaseTemplateDefinition(WizardDesigner designer, WizardStepBase step, string name, Style style) : base(designer, name, step, name, style)
        {
            this._step = step;
        }

        public override string Content
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach (Control control in this._step.Controls)
                {
                    builder.Append(ControlPersister.PersistControl(control));
                }
                return builder.ToString();
            }
            set
            {
                this._step.Controls.Clear();
                if (value != null)
                {
                    IDesignerHost service = (IDesignerHost) base.GetService(typeof(IDesignerHost));
                    foreach (Control control in ControlParser.ParseControls(service, value))
                    {
                        this._step.Controls.Add(control);
                    }
                }
            }
        }
    }
}

