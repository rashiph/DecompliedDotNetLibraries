namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal class FormatStringDialog : Form
    {
        private Button cancelButton;
        private ITypeDescriptorContext context;
        private System.Windows.Forms.DataGridViewCellStyle dgvCellStyle;
        private bool dirty;
        private FormatControl formatControl1;
        private System.Windows.Forms.ListControl listControl;
        private Button okButton;

        public FormatStringDialog(ITypeDescriptorContext context)
        {
            this.context = context;
            this.InitializeComponent();
            if (System.Design.SR.GetString("RTL").Equals("RTL_False"))
            {
                this.RightToLeft = RightToLeft.No;
                this.RightToLeftLayout = false;
            }
            else
            {
                this.RightToLeft = RightToLeft.Yes;
                this.RightToLeftLayout = true;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.dirty = false;
        }

        public void End()
        {
        }

        internal void FormatControlFinishedLoading()
        {
            this.okButton.Top = this.formatControl1.Bottom + 5;
            this.cancelButton.Top = this.formatControl1.Bottom + 5;
            int rightSideOffset = GetRightSideOffset(this.formatControl1);
            int num2 = GetRightSideOffset(this.cancelButton);
            this.okButton.Left += rightSideOffset - num2;
            this.cancelButton.Left += rightSideOffset - num2;
        }

        private void FormatStringDialog_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            this.FormatStringDialog_HelpRequestHandled();
            e.Cancel = true;
        }

        private void FormatStringDialog_HelpRequested(object sender, HelpEventArgs e)
        {
            this.FormatStringDialog_HelpRequestHandled();
            e.Handled = true;
        }

        private void FormatStringDialog_HelpRequestHandled()
        {
            IHelpService service = this.context.GetService(typeof(IHelpService)) as IHelpService;
            if (service != null)
            {
                service.ShowHelpFromKeyword("vs.FormatStringDialog");
            }
        }

        private void FormatStringDialog_Load(object sender, EventArgs e)
        {
            string str = (this.dgvCellStyle != null) ? this.dgvCellStyle.Format : this.listControl.FormatString;
            object obj2 = (this.dgvCellStyle != null) ? this.dgvCellStyle.NullValue : null;
            string str2 = string.Empty;
            if (!string.IsNullOrEmpty(str))
            {
                str2 = FormatControl.FormatTypeStringFromFormatString(str);
            }
            if (this.dgvCellStyle != null)
            {
                this.formatControl1.NullValueTextBoxEnabled = true;
            }
            else
            {
                this.formatControl1.NullValueTextBoxEnabled = false;
            }
            this.formatControl1.FormatType = str2;
            FormatControl.FormatTypeClass formatTypeItem = this.formatControl1.FormatTypeItem;
            if (formatTypeItem != null)
            {
                formatTypeItem.PushFormatStringIntoFormatType(str);
            }
            else
            {
                this.formatControl1.FormatType = System.Design.SR.GetString("BindingFormattingDialogFormatTypeNoFormatting");
            }
            this.formatControl1.NullValue = (obj2 != null) ? obj2.ToString() : "";
        }

        private static int GetRightSideOffset(Control ctl)
        {
            int width = ctl.Width;
            while (ctl != null)
            {
                width += ctl.Left;
                ctl = ctl.Parent;
            }
            return width;
        }

        private void InitializeComponent()
        {
            this.cancelButton = new Button();
            this.okButton = new Button();
            this.formatControl1 = new FormatControl();
            base.SuspendLayout();
            this.formatControl1.Location = new Point(10, 10);
            this.formatControl1.Margin = new Padding(0);
            this.formatControl1.Name = "formatControl1";
            this.formatControl1.Size = new Size(0x178, 0x10c);
            this.formatControl1.TabIndex = 0;
            this.cancelButton.Location = new Point(0x12b, 0x120);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(0x57, 0x17);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = System.Design.SR.GetString("DataGridView_Cancel");
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Click += new EventHandler(this.cancelButton_Click);
            this.okButton.Location = new Point(0xcb, 0x120);
            this.okButton.Name = "okButton";
            this.okButton.Size = new Size(0x57, 0x17);
            this.okButton.TabIndex = 1;
            this.okButton.Text = System.Design.SR.GetString("DataGridView_OK");
            this.okButton.DialogResult = DialogResult.OK;
            this.okButton.Click += new EventHandler(this.okButton_Click);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.ClientSize = new Size(0x18c, 0x127);
            this.AutoSize = true;
            base.HelpButton = true;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.StartPosition = FormStartPosition.CenterScreen;
            base.ShowInTaskbar = false;
            base.Icon = null;
            base.Name = "Form1";
            base.Controls.Add(this.okButton);
            base.Controls.Add(this.formatControl1);
            base.Controls.Add(this.cancelButton);
            base.Padding = new Padding(0);
            this.Text = System.Design.SR.GetString("FormatStringDialogTitle");
            base.HelpButtonClicked += new CancelEventHandler(this.FormatStringDialog_HelpButtonClicked);
            base.HelpRequested += new HelpEventHandler(this.FormatStringDialog_HelpRequested);
            base.Load += new EventHandler(this.FormatStringDialog_Load);
            base.ResumeLayout(false);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.PushChanges();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if ((keyData & ~Keys.KeyCode) != Keys.None)
            {
                return base.ProcessDialogKey(keyData);
            }
            Keys keys = keyData & Keys.KeyCode;
            if (keys != Keys.Enter)
            {
                if (keys != Keys.Escape)
                {
                    return base.ProcessDialogKey(keyData);
                }
            }
            else
            {
                base.DialogResult = DialogResult.OK;
                this.PushChanges();
                base.Close();
                return true;
            }
            this.dirty = false;
            base.DialogResult = DialogResult.Cancel;
            base.Close();
            return true;
        }

        private void PushChanges()
        {
            FormatControl.FormatTypeClass formatTypeItem = this.formatControl1.FormatTypeItem;
            if (formatTypeItem != null)
            {
                if (this.dgvCellStyle != null)
                {
                    this.dgvCellStyle.Format = formatTypeItem.FormatString;
                    this.dgvCellStyle.NullValue = this.formatControl1.NullValue;
                }
                else
                {
                    this.listControl.FormatString = formatTypeItem.FormatString;
                }
                this.dirty = true;
            }
        }

        public System.Windows.Forms.DataGridViewCellStyle DataGridViewCellStyle
        {
            set
            {
                this.dgvCellStyle = value;
                this.listControl = null;
            }
        }

        public bool Dirty
        {
            get
            {
                if (!this.dirty)
                {
                    return this.formatControl1.Dirty;
                }
                return true;
            }
        }

        public System.Windows.Forms.ListControl ListControl
        {
            set
            {
                this.listControl = value;
                this.dgvCellStyle = null;
            }
        }
    }
}

