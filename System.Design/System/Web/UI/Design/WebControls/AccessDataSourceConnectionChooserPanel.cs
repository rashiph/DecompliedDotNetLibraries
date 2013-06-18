namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Data;
    using System.Design;
    using System.Drawing;
    using System.IO;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal class AccessDataSourceConnectionChooserPanel : SqlDataSourceConnectionPanel
    {
        private AccessDataSource _accessDataSource;
        private AccessDataSourceDesigner _accessDataSourceDesigner;
        private System.Windows.Forms.Label _dataFileLabel;
        private System.Windows.Forms.TextBox _dataFileTextBox;
        private System.Windows.Forms.Label _helpLabel;
        private System.Windows.Forms.Button _selectFileButton;

        public AccessDataSourceConnectionChooserPanel(AccessDataSourceDesigner accessDataSourceDesigner, AccessDataSource accessDataSource) : base(accessDataSourceDesigner)
        {
            this._accessDataSource = accessDataSource;
            this._accessDataSourceDesigner = accessDataSourceDesigner;
            this.InitializeComponent();
            this.InitializeUI();
            this.DataFile = this._accessDataSource.DataFile;
        }

        private void InitializeComponent()
        {
            this._dataFileLabel = new System.Windows.Forms.Label();
            this._dataFileTextBox = new System.Windows.Forms.TextBox();
            this._selectFileButton = new System.Windows.Forms.Button();
            this._helpLabel = new System.Windows.Forms.Label();
            base.SuspendLayout();
            this._dataFileLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._dataFileLabel.Location = new Point(0, 0);
            this._dataFileLabel.Name = "_dataFileLabel";
            this._dataFileLabel.Size = new Size(0x1cf, 0x10);
            this._dataFileLabel.TabIndex = 10;
            this._dataFileTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._dataFileTextBox.Location = new Point(0, 0x12);
            this._dataFileTextBox.Name = "_dataFileTextBox";
            this._dataFileTextBox.Size = new Size(0x1cf, 20);
            this._dataFileTextBox.TabIndex = 20;
            this._dataFileTextBox.TextChanged += new EventHandler(this.OnDataFileTextBoxTextChanged);
            this._selectFileButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this._selectFileButton.Location = new Point(0x1d5, 0x11);
            this._selectFileButton.Name = "_selectFileButton";
            this._selectFileButton.Size = new Size(0x4b, 0x17);
            this._selectFileButton.TabIndex = 30;
            this._selectFileButton.Click += new EventHandler(this.OnSelectFileButtonClick);
            this._helpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._helpLabel.Location = new Point(0, 0x2c);
            this._helpLabel.Name = "_helpLabel";
            this._helpLabel.Size = new Size(0x1cf, 0x20);
            this._helpLabel.TabIndex = 40;
            base.Controls.Add(this._helpLabel);
            base.Controls.Add(this._selectFileButton);
            base.Controls.Add(this._dataFileTextBox);
            base.Controls.Add(this._dataFileLabel);
            base.Name = "AccessDataSourceConnectionChooserPanel";
            base.Size = new Size(0x220, 0x112);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeUI()
        {
            this._dataFileLabel.Text = System.Design.SR.GetString("AccessDataSourceConnectionChooserPanel_DataFileLabel");
            this._selectFileButton.Text = System.Design.SR.GetString("AccessDataSourceConnectionChooserPanel_BrowseButton");
            this._helpLabel.Text = System.Design.SR.GetString("AccessDataSourceConnectionChooserPanel_HelpLabel");
            base.Caption = System.Design.SR.GetString("AccessDataSourceConnectionChooserPanel_PanelCaption");
        }

        protected internal override void OnComplete()
        {
            if (this._accessDataSource.DataFile != this.DataFile)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this._accessDataSource)["DataFile"];
                descriptor.ResetValue(this._accessDataSource);
                descriptor.SetValue(this._accessDataSource, this.DataFile);
            }
        }

        private void OnDataFileTextBoxTextChanged(object sender, EventArgs e)
        {
            this.SetEnabledState();
        }

        public override bool OnNext()
        {
            if (!File.Exists(UrlPath.MapPath(base.ServiceProvider, this.DataFile)))
            {
                UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("AccessDataSourceConnectionChooserPanel_FileNotFound", new object[] { this.DataFile }));
                return false;
            }
            return base.OnNext();
        }

        private void OnSelectFileButtonClick(object sender, EventArgs e)
        {
            string str = UrlBuilder.BuildUrl(this._accessDataSource, this, this.DataFile, System.Design.SR.GetString("MdbDataFileEditor_Caption"), System.Design.SR.GetString("MdbDataFileEditor_Filter"));
            if (str != null)
            {
                this.DataFile = str;
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            base.ParentWizard.FinishButton.Enabled = false;
            if (base.Visible)
            {
                this.SetEnabledState();
            }
            else
            {
                base.ParentWizard.NextButton.Enabled = true;
            }
        }

        private void SetEnabledState()
        {
            if (base.ParentWizard != null)
            {
                base.ParentWizard.NextButton.Enabled = this._dataFileTextBox.Text.Length > 0;
            }
        }

        public override DesignerDataConnection DataConnection
        {
            get
            {
                AccessDataSource dataSource = new AccessDataSource {
                    DataFile = this.DataFile
                };
                return new DesignerDataConnection("AccessDataSource", dataSource.ProviderName, AccessDataSourceDesigner.GetConnectionString(base.ServiceProvider, dataSource));
            }
        }

        private string DataFile
        {
            get
            {
                return this._dataFileTextBox.Text;
            }
            set
            {
                this._dataFileTextBox.Text = value;
                this._dataFileTextBox.Select(0, 0);
            }
        }
    }
}

