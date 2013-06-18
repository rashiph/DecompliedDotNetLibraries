namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design.Data;
    using System.Design;
    using System.Drawing;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;

    internal class SqlDataSourceWizardForm : WizardForm
    {
        private SqlDataSourceConfigureParametersPanel _configureParametersPanel;
        private SqlDataSourceConfigureSelectPanel _configureSelectPanel;
        private SqlDataSourceConnectionPanel _connectionPanel;
        private SqlDataSourceCustomCommandPanel _customCommandPanel;
        private IDataEnvironment _dataEnvironment;
        private System.ComponentModel.Design.Data.DesignerDataConnection _designerDataConnection;
        private SqlDataSourceSaveConfiguredConnectionPanel _saveConfiguredConnectionPanel;
        private SqlDataSource _sqlDataSource;
        private System.Web.UI.Design.WebControls.SqlDataSourceDesigner _sqlDataSourceDesigner;
        private SqlDataSourceSummaryPanel _summaryPanel;

        public SqlDataSourceWizardForm(IServiceProvider serviceProvider, System.Web.UI.Design.WebControls.SqlDataSourceDesigner sqlDataSourceDesigner, IDataEnvironment dataEnvironment) : base(serviceProvider)
        {
            base.Glyph = new Bitmap(typeof(SqlDataSourceWizardForm), "datasourcewizard.bmp");
            this._dataEnvironment = dataEnvironment;
            this._sqlDataSource = (SqlDataSource) sqlDataSourceDesigner.Component;
            this._sqlDataSourceDesigner = sqlDataSourceDesigner;
            this.Text = System.Design.SR.GetString("ConfigureDataSource_Title", new object[] { this._sqlDataSource.ID });
            this._connectionPanel = this.CreateConnectionPanel();
            base.SetPanels(new WizardPanel[] { this._connectionPanel });
            this._saveConfiguredConnectionPanel = new SqlDataSourceSaveConfiguredConnectionPanel(this._sqlDataSourceDesigner, this._dataEnvironment);
            base.RegisterPanel(this._saveConfiguredConnectionPanel);
            this._configureParametersPanel = new SqlDataSourceConfigureParametersPanel(this._sqlDataSourceDesigner);
            base.RegisterPanel(this._configureParametersPanel);
            this._configureSelectPanel = new SqlDataSourceConfigureSelectPanel(this._sqlDataSourceDesigner);
            base.RegisterPanel(this._configureSelectPanel);
            this._customCommandPanel = new SqlDataSourceCustomCommandPanel(this._sqlDataSourceDesigner);
            base.RegisterPanel(this._customCommandPanel);
            this._summaryPanel = new SqlDataSourceSummaryPanel(this._sqlDataSourceDesigner);
            base.RegisterPanel(this._summaryPanel);
            base.Size += new Size(0, 40);
            this.MinimumSize = base.Size;
        }

        protected virtual SqlDataSourceConnectionPanel CreateConnectionPanel()
        {
            return new SqlDataSourceDataConnectionChooserPanel(this.SqlDataSourceDesigner, this.DataEnvironment);
        }

        internal SqlDataSourceConfigureParametersPanel GetConfigureParametersPanel()
        {
            this._configureParametersPanel.ResetUI();
            return this._configureParametersPanel;
        }

        internal SqlDataSourceConfigureSelectPanel GetConfigureSelectPanel()
        {
            this._configureSelectPanel.ResetUI();
            return this._configureSelectPanel;
        }

        internal SqlDataSourceCustomCommandPanel GetCustomCommandPanel()
        {
            this._customCommandPanel.ResetUI();
            return this._customCommandPanel;
        }

        internal SqlDataSourceSaveConfiguredConnectionPanel GetSaveConfiguredConnectionPanel()
        {
            this._saveConfiguredConnectionPanel.ResetUI();
            return this._saveConfiguredConnectionPanel;
        }

        internal SqlDataSourceSummaryPanel GetSummaryPanel()
        {
            this._summaryPanel.ResetUI();
            return this._summaryPanel;
        }

        protected override void OnPanelChanging(WizardPanelChangingEventArgs e)
        {
            base.OnPanelChanging(e);
            if (e.CurrentPanel == this._connectionPanel)
            {
                this._designerDataConnection = this._connectionPanel.DataConnection;
            }
        }

        internal IDataEnvironment DataEnvironment
        {
            get
            {
                return this._dataEnvironment;
            }
        }

        internal System.ComponentModel.Design.Data.DesignerDataConnection DesignerDataConnection
        {
            get
            {
                return this._designerDataConnection;
            }
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.SqlDataSource.ConfigureDataSource";
            }
        }

        internal System.Web.UI.Design.WebControls.SqlDataSourceDesigner SqlDataSourceDesigner
        {
            get
            {
                return this._sqlDataSourceDesigner;
            }
        }
    }
}

