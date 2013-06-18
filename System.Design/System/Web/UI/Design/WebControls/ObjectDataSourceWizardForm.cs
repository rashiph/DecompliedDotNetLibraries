namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Design;
    using System.Drawing;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;

    internal sealed class ObjectDataSourceWizardForm : WizardForm
    {
        private ObjectDataSource _objectDataSource;
        private ObjectDataSourceDesigner _objectDataSourceDesigner;
        private ObjectDataSourceConfigureParametersPanel _parametersPanel;

        public ObjectDataSourceWizardForm(IServiceProvider serviceProvider, ObjectDataSourceDesigner objectDataSourceDesigner) : base(serviceProvider)
        {
            base.Glyph = new Bitmap(typeof(SqlDataSourceWizardForm), "datasourcewizard.bmp");
            this._objectDataSourceDesigner = objectDataSourceDesigner;
            this._objectDataSource = (ObjectDataSource) this._objectDataSourceDesigner.Component;
            this.Text = System.Design.SR.GetString("ConfigureDataSource_Title", new object[] { this._objectDataSource.ID });
            ObjectDataSourceChooseTypePanel panel = new ObjectDataSourceChooseTypePanel(this._objectDataSourceDesigner);
            ObjectDataSourceChooseMethodsPanel panel2 = new ObjectDataSourceChooseMethodsPanel(this._objectDataSourceDesigner);
            base.SetPanels(new WizardPanel[] { panel, panel2 });
            this._parametersPanel = new ObjectDataSourceConfigureParametersPanel(this._objectDataSourceDesigner);
            base.RegisterPanel(this._parametersPanel);
            base.Size += new Size(0, 40);
            this.MinimumSize = base.Size;
        }

        internal ObjectDataSourceConfigureParametersPanel GetParametersPanel()
        {
            this._parametersPanel.ResetUI();
            return this._parametersPanel;
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.ObjectDataSource.ConfigureDataSource";
            }
        }
    }
}

