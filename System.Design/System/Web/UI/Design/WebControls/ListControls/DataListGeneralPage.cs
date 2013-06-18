namespace System.Web.UI.Design.WebControls.ListControls
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class DataListGeneralPage : BaseDataListPage
    {
        private System.Windows.Forms.CheckBox extractRowsCheck;
        private const int IDX_DIR_HORIZONTAL = 0;
        private const int IDX_DIR_VERTICAL = 1;
        private const int IDX_MODE_FLOW = 1;
        private const int IDX_MODE_TABLE = 0;
        private NumberEdit repeatColumnsEdit;
        private ComboBox repeatDirectionCombo;
        private ComboBox repeatLayoutCombo;
        private System.Windows.Forms.CheckBox showFooterCheck;
        private System.Windows.Forms.CheckBox showHeaderCheck;

        private void InitForm()
        {
            GroupLabel label = new GroupLabel();
            this.showHeaderCheck = new System.Windows.Forms.CheckBox();
            this.showFooterCheck = new System.Windows.Forms.CheckBox();
            GroupLabel label2 = new GroupLabel();
            System.Windows.Forms.Label label3 = new System.Windows.Forms.Label();
            this.repeatColumnsEdit = new NumberEdit();
            System.Windows.Forms.Label label4 = new System.Windows.Forms.Label();
            this.repeatDirectionCombo = new ComboBox();
            System.Windows.Forms.Label label5 = new System.Windows.Forms.Label();
            this.repeatLayoutCombo = new ComboBox();
            GroupLabel label6 = new GroupLabel();
            this.extractRowsCheck = new System.Windows.Forms.CheckBox();
            label.SetBounds(4, 4, 360, 0x10);
            label.Text = System.Design.SR.GetString("DLGen_HeaderFooterGroup");
            label.TabIndex = 7;
            label.TabStop = false;
            this.showHeaderCheck.SetBounds(8, 0x18, 170, 0x10);
            this.showHeaderCheck.TabIndex = 8;
            this.showHeaderCheck.Text = System.Design.SR.GetString("DLGen_ShowHeader");
            this.showHeaderCheck.TextAlign = ContentAlignment.MiddleLeft;
            this.showHeaderCheck.FlatStyle = FlatStyle.System;
            this.showHeaderCheck.CheckedChanged += new EventHandler(this.OnCheckChangedShowHeader);
            this.showFooterCheck.SetBounds(8, 0x2a, 170, 0x10);
            this.showFooterCheck.TabIndex = 9;
            this.showFooterCheck.Text = System.Design.SR.GetString("DLGen_ShowFooter");
            this.showFooterCheck.TextAlign = ContentAlignment.MiddleLeft;
            this.showFooterCheck.FlatStyle = FlatStyle.System;
            this.showFooterCheck.CheckedChanged += new EventHandler(this.OnCheckChangedShowFooter);
            label2.SetBounds(4, 0x44, 360, 0x10);
            label2.Text = System.Design.SR.GetString("DLGen_RepeatLayoutGroup");
            label2.TabIndex = 10;
            label2.TabStop = false;
            label3.SetBounds(8, 0x58, 0x6a, 0x10);
            label3.Text = System.Design.SR.GetString("DLGen_RepeatColumns");
            label3.TabStop = false;
            label3.TabIndex = 11;
            this.repeatColumnsEdit.SetBounds(0x70, 0x54, 40, 0x15);
            this.repeatColumnsEdit.AllowDecimal = false;
            this.repeatColumnsEdit.AllowNegative = false;
            this.repeatColumnsEdit.TabIndex = 12;
            this.repeatColumnsEdit.TextChanged += new EventHandler(this.OnChangedRepeatProps);
            label4.SetBounds(8, 0x71, 0x6a, 0x10);
            label4.Text = System.Design.SR.GetString("DLGen_RepeatDirection");
            label4.TabStop = false;
            label4.TabIndex = 13;
            this.repeatDirectionCombo.SetBounds(0x70, 0x6d, 140, 0x38);
            this.repeatDirectionCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.repeatDirectionCombo.Items.AddRange(new object[] { System.Design.SR.GetString("DLGen_RD_Horz"), System.Design.SR.GetString("DLGen_RD_Vert") });
            this.repeatDirectionCombo.TabIndex = 14;
            this.repeatDirectionCombo.SelectedIndexChanged += new EventHandler(this.OnChangedRepeatProps);
            label5.SetBounds(8, 0x8a, 0x6a, 0x10);
            label5.Text = System.Design.SR.GetString("DLGen_RepeatLayout");
            label5.TabStop = false;
            label5.TabIndex = 15;
            this.repeatLayoutCombo.SetBounds(0x70, 0x86, 140, 0x15);
            this.repeatLayoutCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.repeatLayoutCombo.Items.AddRange(new object[] { System.Design.SR.GetString("DLGen_RL_Table"), System.Design.SR.GetString("DLGen_RL_Flow") });
            this.repeatLayoutCombo.TabIndex = 0x10;
            this.repeatLayoutCombo.SelectedIndexChanged += new EventHandler(this.OnChangedRepeatProps);
            label6.SetBounds(4, 0xa2, 360, 0x10);
            label6.Text = System.Design.SR.GetString("DLGen_Templates");
            label6.TabIndex = 0x11;
            label6.TabStop = false;
            label6.Visible = false;
            this.extractRowsCheck.SetBounds(8, 0xb6, 260, 0x10);
            this.extractRowsCheck.Text = System.Design.SR.GetString("DLGen_ExtractRows");
            this.extractRowsCheck.TabIndex = 0x12;
            this.extractRowsCheck.Visible = false;
            this.extractRowsCheck.FlatStyle = FlatStyle.System;
            this.extractRowsCheck.CheckedChanged += new EventHandler(this.OnCheckChangedExtractRows);
            this.Text = System.Design.SR.GetString("DLGen_Text");
            base.AccessibleDescription = System.Design.SR.GetString("DLGen_Desc");
            base.Size = new Size(0x170, 280);
            base.CommitOnDeactivate = true;
            base.Icon = new Icon(base.GetType(), "DataListGeneralPage.ico");
            base.Controls.Clear();
            base.Controls.AddRange(new Control[] { this.extractRowsCheck, label6, this.repeatLayoutCombo, label5, this.repeatDirectionCombo, label4, this.repeatColumnsEdit, label3, label2, this.showFooterCheck, this.showHeaderCheck, label });
        }

        private void InitPage()
        {
            this.showHeaderCheck.Checked = false;
            this.showFooterCheck.Checked = false;
            this.repeatColumnsEdit.Clear();
            this.repeatDirectionCombo.SelectedIndex = -1;
            this.repeatLayoutCombo.SelectedIndex = -1;
            this.extractRowsCheck.Checked = false;
        }

        protected override void LoadComponent()
        {
            this.InitPage();
            DataList baseControl = (DataList) base.GetBaseControl();
            this.showHeaderCheck.Checked = baseControl.ShowHeader;
            this.showFooterCheck.Checked = baseControl.ShowFooter;
            this.repeatColumnsEdit.Text = baseControl.RepeatColumns.ToString(NumberFormatInfo.CurrentInfo);
            switch (baseControl.RepeatDirection)
            {
                case RepeatDirection.Horizontal:
                    this.repeatDirectionCombo.SelectedIndex = 0;
                    break;

                case RepeatDirection.Vertical:
                    this.repeatDirectionCombo.SelectedIndex = 1;
                    break;
            }
            switch (baseControl.RepeatLayout)
            {
                case RepeatLayout.Table:
                    this.repeatLayoutCombo.SelectedIndex = 0;
                    break;

                case RepeatLayout.Flow:
                    this.repeatLayoutCombo.SelectedIndex = 1;
                    break;
            }
            this.extractRowsCheck.Checked = baseControl.ExtractTemplateRows;
        }

        private void OnChangedRepeatProps(object source, EventArgs e)
        {
            if (!base.IsLoading())
            {
                this.SetDirty();
            }
        }

        private void OnCheckChangedExtractRows(object source, EventArgs e)
        {
            if (!base.IsLoading())
            {
                this.SetDirty();
            }
        }

        private void OnCheckChangedShowFooter(object source, EventArgs e)
        {
            if (!base.IsLoading())
            {
                this.SetDirty();
            }
        }

        private void OnCheckChangedShowHeader(object source, EventArgs e)
        {
            if (!base.IsLoading())
            {
                this.SetDirty();
            }
        }

        protected override void SaveComponent()
        {
            DataList baseControl = (DataList) base.GetBaseControl();
            baseControl.ShowHeader = this.showHeaderCheck.Checked;
            baseControl.ShowFooter = this.showFooterCheck.Checked;
            string s = this.repeatColumnsEdit.Text.Trim();
            if (s.Length != 0)
            {
                try
                {
                    baseControl.RepeatColumns = int.Parse(s, CultureInfo.CurrentCulture);
                }
                catch
                {
                    this.repeatColumnsEdit.Text = baseControl.RepeatColumns.ToString(CultureInfo.CurrentCulture);
                }
            }
            switch (this.repeatDirectionCombo.SelectedIndex)
            {
                case 0:
                    baseControl.RepeatDirection = RepeatDirection.Horizontal;
                    break;

                case 1:
                    baseControl.RepeatDirection = RepeatDirection.Vertical;
                    break;
            }
            switch (this.repeatLayoutCombo.SelectedIndex)
            {
                case 0:
                    baseControl.RepeatLayout = RepeatLayout.Table;
                    break;

                case 1:
                    baseControl.RepeatLayout = RepeatLayout.Flow;
                    break;
            }
            baseControl.ExtractTemplateRows = this.extractRowsCheck.Checked;
        }

        public override void SetComponent(IComponent component)
        {
            base.SetComponent(component);
            this.InitForm();
        }

        protected override string HelpKeyword
        {
            get
            {
                return "net.Asp.DataListProperties.General";
            }
        }
    }
}

