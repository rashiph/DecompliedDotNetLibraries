namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Design;
    using System.Drawing;
    using System.Reflection;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal sealed class ObjectDataSourceConfigureParametersPanel : WizardPanel
    {
        private System.Windows.Forms.Label _helpLabel;
        private ObjectDataSource _objectDataSource;
        private ObjectDataSourceDesigner _objectDataSourceDesigner;
        private ParameterEditorUserControl _parameterEditorUserControl;
        private System.Windows.Forms.Label _signatureLabel;
        private System.Windows.Forms.TextBox _signatureTextBox;

        public ObjectDataSourceConfigureParametersPanel(ObjectDataSourceDesigner objectDataSourceDesigner)
        {
            this._objectDataSourceDesigner = objectDataSourceDesigner;
            this._objectDataSource = (ObjectDataSource) this._objectDataSourceDesigner.Component;
            this.InitializeComponent();
            this.InitializeUI();
            this._parameterEditorUserControl.SetAllowCollectionChanges(false);
        }

        private void InitializeComponent()
        {
            this._helpLabel = new System.Windows.Forms.Label();
            this._parameterEditorUserControl = new ParameterEditorUserControl(this._objectDataSource.Site, this._objectDataSource);
            this._signatureLabel = new System.Windows.Forms.Label();
            this._signatureTextBox = new System.Windows.Forms.TextBox();
            base.SuspendLayout();
            this._helpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._helpLabel.Location = new Point(0, 0);
            this._helpLabel.Name = "_helpLabel";
            this._helpLabel.Size = new Size(0x220, 0x2d);
            this._helpLabel.TabIndex = 10;
            this._parameterEditorUserControl.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._parameterEditorUserControl.Location = new Point(0, 0x26);
            this._parameterEditorUserControl.Name = "_parameterEditorUserControl";
            this._parameterEditorUserControl.Size = new Size(0x220, 0x98);
            this._parameterEditorUserControl.TabIndex = 20;
            this._parameterEditorUserControl.ParametersChanged += new EventHandler(this.OnParameterEditorUserControlParametersChanged);
            this._signatureLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this._signatureLabel.Location = new Point(0, 0xd6);
            this._signatureLabel.Name = "_signatureLabel";
            this._signatureLabel.Size = new Size(0x220, 0x10);
            this._signatureLabel.TabIndex = 30;
            this._signatureTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this._signatureTextBox.BackColor = SystemColors.Control;
            this._signatureTextBox.Location = new Point(0, 0xe8);
            this._signatureTextBox.Multiline = true;
            this._signatureTextBox.Name = "_signatureTextBox";
            this._signatureTextBox.ReadOnly = true;
            this._signatureTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._signatureTextBox.Size = new Size(0x220, 0x2a);
            this._signatureTextBox.TabIndex = 40;
            this._signatureTextBox.Text = "";
            base.Controls.Add(this._signatureTextBox);
            base.Controls.Add(this._signatureLabel);
            base.Controls.Add(this._parameterEditorUserControl);
            base.Controls.Add(this._helpLabel);
            base.Name = "ObjectDataSourceConfigureParametersPanel";
            base.Size = new Size(0x220, 0x112);
            base.ResumeLayout(false);
        }

        public void InitializeParameters(ParameterCollection selectParameters)
        {
            Parameter[] parameterArray = new Parameter[selectParameters.Count];
            selectParameters.CopyTo(parameterArray, 0);
            this._parameterEditorUserControl.AddParameters(parameterArray);
        }

        private void InitializeUI()
        {
            base.Caption = System.Design.SR.GetString("ObjectDataSourceConfigureParametersPanel_PanelCaption");
            this._helpLabel.Text = System.Design.SR.GetString("ObjectDataSourceConfigureParametersPanel_HelpLabel");
            this._signatureLabel.Text = System.Design.SR.GetString("ObjectDataSource_General_MethodSignatureLabel");
        }

        protected internal override void OnComplete()
        {
            this._objectDataSource.SelectParameters.Clear();
            foreach (Parameter parameter in this._parameterEditorUserControl.GetParameters())
            {
                this._objectDataSource.SelectParameters.Add(parameter);
            }
        }

        public override bool OnNext()
        {
            return true;
        }

        private void OnParameterEditorUserControlParametersChanged(object sender, EventArgs e)
        {
            this.UpdateUI();
        }

        public override void OnPrevious()
        {
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (base.Visible)
            {
                base.ParentWizard.NextButton.Enabled = false;
                this.UpdateUI();
            }
        }

        public void ResetUI()
        {
            this._parameterEditorUserControl.ClearParameters();
        }

        public void SetMethod(MethodInfo selectMethodInfo)
        {
            this._signatureTextBox.Text = ObjectDataSourceMethodEditor.GetMethodSignature(selectMethodInfo);
            Parameter[] parameters = ObjectDataSourceDesigner.MergeParameters(this._parameterEditorUserControl.GetParameters(), selectMethodInfo);
            this._parameterEditorUserControl.ClearParameters();
            this._parameterEditorUserControl.AddParameters(parameters);
        }

        private void UpdateUI()
        {
            base.ParentWizard.FinishButton.Enabled = this._parameterEditorUserControl.ParametersConfigured;
        }
    }
}

