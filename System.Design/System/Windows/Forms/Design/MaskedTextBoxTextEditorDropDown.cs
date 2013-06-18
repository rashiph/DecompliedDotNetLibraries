namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class MaskedTextBoxTextEditorDropDown : UserControl
    {
        private bool cancel;
        private MaskedTextBox cloneMtb;
        private ErrorProvider errorProvider;

        public MaskedTextBoxTextEditorDropDown(MaskedTextBox maskedTextBox)
        {
            this.cloneMtb = MaskedTextBoxDesigner.GetDesignMaskedTextBox(maskedTextBox);
            this.errorProvider = new ErrorProvider();
            ((ISupportInitialize) this.errorProvider).BeginInit();
            base.SuspendLayout();
            this.cloneMtb.Dock = DockStyle.Fill;
            this.cloneMtb.TextMaskFormat = MaskFormat.IncludePromptAndLiterals;
            this.cloneMtb.ResetOnPrompt = true;
            this.cloneMtb.SkipLiterals = true;
            this.cloneMtb.ResetOnSpace = true;
            this.cloneMtb.Name = "MaskedTextBoxClone";
            this.cloneMtb.TabIndex = 0;
            this.cloneMtb.MaskInputRejected += new MaskInputRejectedEventHandler(this.maskedTextBox_MaskInputRejected);
            this.cloneMtb.KeyDown += new KeyEventHandler(this.maskedTextBox_KeyDown);
            this.errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            base.Controls.Add(this.cloneMtb);
            this.BackColor = SystemColors.Control;
            base.BorderStyle = BorderStyle.FixedSingle;
            base.Name = "MaskedTextBoxTextEditorDropDown";
            base.Padding = new Padding(0x10);
            base.Size = new Size(100, 0x34);
            ((ISupportInitialize) this.errorProvider).EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void maskedTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            this.errorProvider.Clear();
        }

        private void maskedTextBox_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            this.errorProvider.SetError(this.cloneMtb, MaskedTextBoxDesigner.GetMaskInputRejectedErrorMessage(e));
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.cancel = true;
            }
            return base.ProcessDialogKey(keyData);
        }

        public string Value
        {
            get
            {
                if (this.cancel)
                {
                    return null;
                }
                return this.cloneMtb.Text;
            }
        }
    }
}

