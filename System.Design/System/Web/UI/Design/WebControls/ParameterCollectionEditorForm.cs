namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Design;
    using System.Drawing;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal class ParameterCollectionEditorForm : DesignerForm
    {
        private System.Windows.Forms.Button _cancelButton;
        private System.Web.UI.Control _control;
        private System.Windows.Forms.Button _okButton;
        private ParameterEditorUserControl _parameterEditorUserControl;
        private ParameterCollection _parameters;

        public ParameterCollectionEditorForm(IServiceProvider serviceProvider, ParameterCollection parameters, ControlDesigner designer) : base(serviceProvider)
        {
            this._parameters = parameters;
            if (designer != null)
            {
                this._control = designer.Component as System.Web.UI.Control;
            }
            this.InitializeComponent();
            this.InitializeUI();
            ArrayList list = new ArrayList();
            foreach (ICloneable cloneable in parameters)
            {
                object clone = cloneable.Clone();
                if (designer != null)
                {
                    designer.RegisterClone(cloneable, clone);
                }
                list.Add(clone);
            }
            this._parameterEditorUserControl.AddParameters((Parameter[]) list.ToArray(typeof(Parameter)));
        }

        private void InitializeComponent()
        {
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._parameterEditorUserControl = new ParameterEditorUserControl(base.ServiceProvider, this._control);
            base.SuspendLayout();
            this._parameterEditorUserControl.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._parameterEditorUserControl.Location = new Point(12, 12);
            this._parameterEditorUserControl.Size = new Size(560, 0x116);
            this._parameterEditorUserControl.TabIndex = 10;
            this._okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._okButton.Location = new Point(0x1a0, 0x12b);
            this._okButton.TabIndex = 20;
            this._okButton.Click += new EventHandler(this.OnOkButtonClick);
            this._cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._cancelButton.Location = new Point(0x1f1, 0x12b);
            this._cancelButton.TabIndex = 30;
            this._cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            base.AcceptButton = this._okButton;
            base.CancelButton = this._cancelButton;
            base.ClientSize = new Size(0x248, 0x14e);
            base.Controls.Add(this._parameterEditorUserControl);
            base.Controls.Add(this._cancelButton);
            base.Controls.Add(this._okButton);
            this.MinimumSize = new Size(0x1e4, 0x110);
            base.InitializeForm();
            base.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            this._okButton.Text = System.Design.SR.GetString("OK");
            this._cancelButton.Text = System.Design.SR.GetString("Cancel");
            this.Text = System.Design.SR.GetString("ParameterCollectionEditorForm_Caption");
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            Parameter[] parameters = this._parameterEditorUserControl.GetParameters();
            this._parameters.Clear();
            foreach (Parameter parameter in parameters)
            {
                this._parameters.Add(parameter);
            }
            base.DialogResult = DialogResult.OK;
            base.Close();
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.Parameter.CollectionEditor";
            }
        }
    }
}

