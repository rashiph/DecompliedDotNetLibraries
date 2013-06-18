namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal sealed class XmlDataSourceConfigureDataSourceForm : DesignerForm
    {
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _chooseDataFileButton;
        private System.Windows.Forms.Button _chooseTransformFileButton;
        private System.Windows.Forms.Label _dataFileLabel;
        private System.Windows.Forms.TextBox _dataFileTextBox;
        private System.Windows.Forms.Label _helpLabel;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Label _transformFileHelpLabel;
        private System.Windows.Forms.Label _transformFileLabel;
        private System.Windows.Forms.TextBox _transformFileTextBox;
        private XmlDataSource _xmlDataSource;
        private System.Windows.Forms.Label _xpathExpressionHelpLabel;
        private System.Windows.Forms.Label _xpathExpressionLabel;
        private System.Windows.Forms.TextBox _xpathExpressionTextBox;

        public XmlDataSourceConfigureDataSourceForm(IServiceProvider serviceProvider, XmlDataSource xmlDataSource) : base(serviceProvider)
        {
            this._xmlDataSource = xmlDataSource;
            this.InitializeComponent();
            this.InitializeUI();
            this.DataFile = this._xmlDataSource.DataFile;
            this.TransformFile = this._xmlDataSource.TransformFile;
            this.XPath = this._xmlDataSource.XPath;
        }

        private void InitializeComponent()
        {
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._dataFileLabel = new System.Windows.Forms.Label();
            this._dataFileTextBox = new System.Windows.Forms.TextBox();
            this._chooseDataFileButton = new System.Windows.Forms.Button();
            this._helpLabel = new System.Windows.Forms.Label();
            this._chooseTransformFileButton = new System.Windows.Forms.Button();
            this._transformFileTextBox = new System.Windows.Forms.TextBox();
            this._transformFileLabel = new System.Windows.Forms.Label();
            this._transformFileHelpLabel = new System.Windows.Forms.Label();
            this._xpathExpressionTextBox = new System.Windows.Forms.TextBox();
            this._xpathExpressionLabel = new System.Windows.Forms.Label();
            this._xpathExpressionHelpLabel = new System.Windows.Forms.Label();
            base.SuspendLayout();
            this._helpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._helpLabel.Location = new Point(12, 12);
            this._helpLabel.Name = "_helpLabel";
            this._helpLabel.Size = new Size(0x1f5, 40);
            this._helpLabel.TabIndex = 10;
            this._dataFileLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._dataFileLabel.Location = new Point(12, 60);
            this._dataFileLabel.Name = "_dataFileLabel";
            this._dataFileLabel.Size = new Size(410, 0x10);
            this._dataFileLabel.TabIndex = 20;
            this._dataFileTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._dataFileTextBox.Location = new Point(12, 0x4e);
            this._dataFileTextBox.Name = "_dataFileTextBox";
            this._dataFileTextBox.Size = new Size(0x1a9, 20);
            this._dataFileTextBox.TabIndex = 30;
            this._chooseDataFileButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this._chooseDataFileButton.Location = new Point(0x1bc, 0x4e);
            this._chooseDataFileButton.Name = "_chooseDataFileButton";
            this._chooseDataFileButton.Size = new Size(0x4b, 0x17);
            this._chooseDataFileButton.TabIndex = 40;
            this._chooseDataFileButton.Click += new EventHandler(this.OnChooseDataFileButtonClick);
            this._transformFileLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._transformFileLabel.Location = new Point(12, 110);
            this._transformFileLabel.Name = "_transformFileLabel";
            this._transformFileLabel.Size = new Size(410, 0x10);
            this._transformFileLabel.TabIndex = 80;
            this._transformFileTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._transformFileTextBox.Location = new Point(12, 0x80);
            this._transformFileTextBox.Name = "_transformFileTextBox";
            this._transformFileTextBox.Size = new Size(0x1a9, 20);
            this._transformFileTextBox.TabIndex = 90;
            this._chooseTransformFileButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this._chooseTransformFileButton.Location = new Point(0x1bc, 0x80);
            this._chooseTransformFileButton.Name = "_chooseTransformFileButton";
            this._chooseTransformFileButton.TabIndex = 100;
            this._chooseTransformFileButton.Size = new Size(0x4b, 0x17);
            this._chooseTransformFileButton.Click += new EventHandler(this.OnChooseTransformFileButtonClick);
            this._transformFileHelpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._transformFileHelpLabel.Location = new Point(12, 0x98);
            this._transformFileHelpLabel.Name = "_transformFileHelpLabel";
            this._transformFileHelpLabel.Size = new Size(0x1f8, 0x20);
            this._transformFileHelpLabel.TabIndex = 0x69;
            this._xpathExpressionLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._xpathExpressionLabel.Location = new Point(12, 0xc2);
            this._xpathExpressionLabel.Name = "_xpathExpressionLabel";
            this._xpathExpressionLabel.Size = new Size(410, 0x10);
            this._xpathExpressionLabel.TabIndex = 110;
            this._xpathExpressionTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._xpathExpressionTextBox.Location = new Point(12, 0xd4);
            this._xpathExpressionTextBox.Name = "_xpathExpressionTextBox";
            this._xpathExpressionTextBox.Size = new Size(0x1a9, 20);
            this._xpathExpressionTextBox.TabIndex = 120;
            this._xpathExpressionHelpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._xpathExpressionHelpLabel.Location = new Point(12, 0xee);
            this._xpathExpressionHelpLabel.Name = "_xpathExpressionHelpLabel";
            this._xpathExpressionHelpLabel.Size = new Size(0x1f8, 30);
            this._xpathExpressionHelpLabel.TabIndex = 0x7d;
            this._okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._okButton.Location = new Point(0x16e, 0x121);
            this._okButton.Name = "_okButton";
            this._okButton.TabIndex = 130;
            this._okButton.Click += new EventHandler(this.OnOkButtonClick);
            this._cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._cancelButton.DialogResult = DialogResult.Cancel;
            this._cancelButton.Location = new Point(0x1bf, 0x121);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.TabIndex = 140;
            this._cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            base.AcceptButton = this._okButton;
            base.CancelButton = this._cancelButton;
            base.ClientSize = new Size(0x216, 0x144);
            base.Controls.Add(this._xpathExpressionTextBox);
            base.Controls.Add(this._xpathExpressionLabel);
            base.Controls.Add(this._xpathExpressionHelpLabel);
            base.Controls.Add(this._chooseTransformFileButton);
            base.Controls.Add(this._transformFileTextBox);
            base.Controls.Add(this._transformFileLabel);
            base.Controls.Add(this._transformFileHelpLabel);
            base.Controls.Add(this._helpLabel);
            base.Controls.Add(this._chooseDataFileButton);
            base.Controls.Add(this._dataFileTextBox);
            base.Controls.Add(this._dataFileLabel);
            base.Controls.Add(this._okButton);
            base.Controls.Add(this._cancelButton);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Name = "XmlDataSourceConfigureDataSourceForm";
            base.InitializeForm();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeUI()
        {
            this._dataFileLabel.Text = System.Design.SR.GetString("XmlDataSourceConfigureDataSourceForm_DataFileLabel");
            this._transformFileLabel.Text = System.Design.SR.GetString("XmlDataSourceConfigureDataSourceForm_TransformFileLabel");
            this._xpathExpressionLabel.Text = System.Design.SR.GetString("XmlDataSourceConfigureDataSourceForm_XPathExpressionLabel");
            this._transformFileHelpLabel.Text = System.Design.SR.GetString("XmlDataSourceConfigureDataSourceForm_TransformFileHelpLabel");
            this._xpathExpressionHelpLabel.Text = System.Design.SR.GetString("XmlDataSourceConfigureDataSourceForm_XPathExpressionHelpLabel");
            this._chooseDataFileButton.Text = System.Design.SR.GetString("XmlDataSourceConfigureDataSourceForm_Browse");
            this._chooseTransformFileButton.Text = System.Design.SR.GetString("XmlDataSourceConfigureDataSourceForm_Browse");
            this._helpLabel.Text = System.Design.SR.GetString("XmlDataSourceConfigureDataSourceForm_HelpLabel");
            this._okButton.Text = System.Design.SR.GetString("OK");
            this._cancelButton.Text = System.Design.SR.GetString("Cancel");
            this._chooseDataFileButton.AccessibleDescription = System.Design.SR.GetString("XmlDataFileEditor_Ellipses");
            this._chooseTransformFileButton.AccessibleDescription = System.Design.SR.GetString("XslTransformFileEditor_Ellipses");
            this.Text = System.Design.SR.GetString("ConfigureDataSource_Title", new object[] { this._xmlDataSource.ID });
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void OnChooseDataFileButtonClick(object sender, EventArgs e)
        {
            string str = UrlBuilder.BuildUrl(this._xmlDataSource, this, this.DataFile, System.Design.SR.GetString("XmlDataFileEditor_Caption"), System.Design.SR.GetString("XmlDataFileEditor_Filter"));
            if (str != null)
            {
                this.DataFile = str;
            }
        }

        private void OnChooseTransformFileButtonClick(object sender, EventArgs e)
        {
            string str = UrlBuilder.BuildUrl(this._xmlDataSource, this, this.TransformFile, System.Design.SR.GetString("XslTransformFileEditor_Caption"), System.Design.SR.GetString("XslTransformFileEditor_Filter"));
            if (str != null)
            {
                this.TransformFile = str;
            }
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            PropertyDescriptor descriptor;
            if (this._xmlDataSource.DataFile != this.DataFile)
            {
                descriptor = TypeDescriptor.GetProperties(this._xmlDataSource)["DataFile"];
                descriptor.ResetValue(this._xmlDataSource);
                descriptor.SetValue(this._xmlDataSource, this.DataFile);
            }
            if (this._xmlDataSource.TransformFile != this.TransformFile)
            {
                descriptor = TypeDescriptor.GetProperties(this._xmlDataSource)["TransformFile"];
                descriptor.ResetValue(this._xmlDataSource);
                descriptor.SetValue(this._xmlDataSource, this.TransformFile);
            }
            if (this._xmlDataSource.XPath != this.XPath)
            {
                descriptor = TypeDescriptor.GetProperties(this._xmlDataSource)["XPath"];
                descriptor.ResetValue(this._xmlDataSource);
                descriptor.SetValue(this._xmlDataSource, this.XPath);
            }
            base.DialogResult = DialogResult.OK;
            base.Close();
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

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.XmlDataSource.ConfigureDataSource";
            }
        }

        private string TransformFile
        {
            get
            {
                return this._transformFileTextBox.Text;
            }
            set
            {
                this._transformFileTextBox.Text = value;
                this._transformFileTextBox.Select(0, 0);
            }
        }

        private string XPath
        {
            get
            {
                return this._xpathExpressionTextBox.Text;
            }
            set
            {
                this._xpathExpressionTextBox.Text = value;
                this._xpathExpressionTextBox.Select(0, 0);
            }
        }
    }
}

