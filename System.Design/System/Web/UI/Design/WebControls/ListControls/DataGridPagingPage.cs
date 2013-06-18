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
    internal sealed class DataGridPagingPage : BaseDataListPage
    {
        private System.Windows.Forms.CheckBox allowCustomPagingCheck;
        private System.Windows.Forms.CheckBox allowPagingCheck;
        private const int IDX_MODE_PAGEBUTTONS = 0;
        private const int IDX_MODE_PAGENUMBERS = 1;
        private const int IDX_POS_BOTTOM = 1;
        private const int IDX_POS_TOP = 0;
        private const int IDX_POS_TOPANDBOTTOM = 2;
        private ComboBox modeCombo;
        private System.Windows.Forms.TextBox nextPageTextEdit;
        private NumberEdit pageButtonCountEdit;
        private NumberEdit pageSizeEdit;
        private ComboBox posCombo;
        private System.Windows.Forms.TextBox prevPageTextEdit;
        private System.Windows.Forms.CheckBox visibleCheck;

        private void InitForm()
        {
            GroupLabel label = new GroupLabel();
            this.allowPagingCheck = new System.Windows.Forms.CheckBox();
            this.allowCustomPagingCheck = new System.Windows.Forms.CheckBox();
            System.Windows.Forms.Label label2 = new System.Windows.Forms.Label();
            this.pageSizeEdit = new NumberEdit();
            System.Windows.Forms.Label label3 = new System.Windows.Forms.Label();
            GroupLabel label4 = new GroupLabel();
            this.visibleCheck = new System.Windows.Forms.CheckBox();
            System.Windows.Forms.Label label5 = new System.Windows.Forms.Label();
            this.posCombo = new ComboBox();
            System.Windows.Forms.Label label6 = new System.Windows.Forms.Label();
            this.modeCombo = new ComboBox();
            System.Windows.Forms.Label label7 = new System.Windows.Forms.Label();
            this.nextPageTextEdit = new System.Windows.Forms.TextBox();
            System.Windows.Forms.Label label8 = new System.Windows.Forms.Label();
            this.prevPageTextEdit = new System.Windows.Forms.TextBox();
            System.Windows.Forms.Label label9 = new System.Windows.Forms.Label();
            this.pageButtonCountEdit = new NumberEdit();
            label.SetBounds(4, 4, 0x1af, 0x10);
            label.Text = System.Design.SR.GetString("DGPg_PagingGroup");
            label.TabStop = false;
            label.TabIndex = 0;
            this.allowPagingCheck.SetBounds(12, 0x18, 180, 0x10);
            this.allowPagingCheck.Text = System.Design.SR.GetString("DGPg_AllowPaging");
            this.allowPagingCheck.TextAlign = ContentAlignment.MiddleLeft;
            this.allowPagingCheck.TabIndex = 1;
            this.allowPagingCheck.FlatStyle = FlatStyle.System;
            this.allowPagingCheck.CheckedChanged += new EventHandler(this.OnCheckChangedAllowPaging);
            this.allowCustomPagingCheck.SetBounds(220, 0x18, 180, 0x10);
            this.allowCustomPagingCheck.Text = System.Design.SR.GetString("DGPg_AllowCustomPaging");
            this.allowCustomPagingCheck.TextAlign = ContentAlignment.MiddleLeft;
            this.allowCustomPagingCheck.TabIndex = 2;
            this.allowCustomPagingCheck.FlatStyle = FlatStyle.System;
            this.allowCustomPagingCheck.CheckedChanged += new EventHandler(this.OnCheckChangedAllowCustomPaging);
            label2.SetBounds(12, 50, 100, 14);
            label2.Text = System.Design.SR.GetString("DGPg_PageSize");
            label2.TabStop = false;
            label2.TabIndex = 3;
            this.pageSizeEdit.SetBounds(0x70, 0x2e, 40, 0x18);
            this.pageSizeEdit.TabIndex = 4;
            this.pageSizeEdit.AllowDecimal = false;
            this.pageSizeEdit.AllowNegative = false;
            this.pageSizeEdit.TextChanged += new EventHandler(this.OnTextChangedPageSize);
            label3.SetBounds(0x9e, 50, 80, 14);
            label3.Text = System.Design.SR.GetString("DGPg_Rows");
            label3.TabStop = false;
            label3.TabIndex = 5;
            label4.SetBounds(4, 0x4e, 0x1af, 14);
            label4.Text = System.Design.SR.GetString("DGPg_NavigationGroup");
            label4.TabStop = false;
            label4.TabIndex = 6;
            this.visibleCheck.SetBounds(12, 100, 260, 0x10);
            this.visibleCheck.Text = System.Design.SR.GetString("DGPg_Visible");
            this.visibleCheck.TextAlign = ContentAlignment.MiddleLeft;
            this.visibleCheck.TabIndex = 7;
            this.visibleCheck.FlatStyle = FlatStyle.System;
            this.visibleCheck.CheckedChanged += new EventHandler(this.OnCheckChangedVisible);
            label5.SetBounds(12, 0x7a, 150, 14);
            label5.Text = System.Design.SR.GetString("DGPg_Position");
            label5.TabStop = false;
            label5.TabIndex = 8;
            this.posCombo.SetBounds(12, 0x8a, 0x90, 0x15);
            this.posCombo.TabIndex = 9;
            this.posCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.posCombo.Items.AddRange(new object[] { System.Design.SR.GetString("DGPg_Pos_Top"), System.Design.SR.GetString("DGPg_Pos_Bottom"), System.Design.SR.GetString("DGPg_Pos_TopBottom") });
            this.posCombo.SelectedIndexChanged += new EventHandler(this.OnPagerChanged);
            label6.SetBounds(12, 0xa6, 150, 14);
            label6.Text = System.Design.SR.GetString("DGPg_Mode");
            label6.TabStop = false;
            label6.TabIndex = 10;
            this.modeCombo.SetBounds(12, 0xb6, 0x90, 0x40);
            this.modeCombo.TabIndex = 11;
            this.modeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.modeCombo.Items.AddRange(new object[] { System.Design.SR.GetString("DGPg_Mode_Buttons"), System.Design.SR.GetString("DGPg_Mode_Numbers") });
            this.modeCombo.SelectedIndexChanged += new EventHandler(this.OnPagerChanged);
            label7.SetBounds(12, 210, 200, 14);
            label7.Text = System.Design.SR.GetString("DGPg_NextPage");
            label7.TabStop = false;
            label7.TabIndex = 12;
            this.nextPageTextEdit.SetBounds(12, 0xe2, 0x90, 0x18);
            this.nextPageTextEdit.TabIndex = 13;
            this.nextPageTextEdit.TextChanged += new EventHandler(this.OnPagerChanged);
            label8.SetBounds(220, 210, 200, 14);
            label8.Text = System.Design.SR.GetString("DGPg_PrevPage");
            label8.TabStop = false;
            label8.TabIndex = 14;
            this.prevPageTextEdit.SetBounds(220, 0xe2, 140, 0x18);
            this.prevPageTextEdit.TabIndex = 15;
            this.prevPageTextEdit.TextChanged += new EventHandler(this.OnPagerChanged);
            label9.SetBounds(12, 0xfe, 200, 14);
            label9.Text = System.Design.SR.GetString("DGPg_ButtonCount");
            label9.TabStop = false;
            label9.TabIndex = 0x10;
            this.pageButtonCountEdit.SetBounds(12, 270, 40, 0x18);
            this.pageButtonCountEdit.TabIndex = 0x11;
            this.pageButtonCountEdit.AllowDecimal = false;
            this.pageButtonCountEdit.AllowNegative = false;
            this.pageButtonCountEdit.TextChanged += new EventHandler(this.OnPagerChanged);
            this.Text = System.Design.SR.GetString("DGPg_Text");
            base.AccessibleDescription = System.Design.SR.GetString("DGPg_Desc");
            base.Size = new Size(0x1d0, 300);
            base.CommitOnDeactivate = true;
            base.Icon = new Icon(base.GetType(), "DataGridPagingPage.ico");
            base.Controls.Clear();
            base.Controls.AddRange(new Control[] { 
                this.pageButtonCountEdit, label9, this.prevPageTextEdit, label8, this.nextPageTextEdit, label7, this.modeCombo, label6, this.posCombo, label5, this.visibleCheck, label4, label3, this.pageSizeEdit, label2, this.allowCustomPagingCheck, 
                this.allowPagingCheck, label
             });
        }

        private void InitPage()
        {
            this.pageSizeEdit.Clear();
            this.visibleCheck.Checked = false;
            this.posCombo.SelectedIndex = -1;
            this.modeCombo.SelectedIndex = -1;
            this.nextPageTextEdit.Clear();
            this.prevPageTextEdit.Clear();
        }

        protected override void LoadComponent()
        {
            this.InitPage();
            System.Web.UI.WebControls.DataGrid baseControl = (System.Web.UI.WebControls.DataGrid) base.GetBaseControl();
            DataGridPagerStyle pagerStyle = baseControl.PagerStyle;
            this.allowPagingCheck.Checked = baseControl.AllowPaging;
            this.allowCustomPagingCheck.Checked = baseControl.AllowCustomPaging;
            this.pageSizeEdit.Text = baseControl.PageSize.ToString(NumberFormatInfo.CurrentInfo);
            this.visibleCheck.Checked = pagerStyle.Visible;
            switch (pagerStyle.Mode)
            {
                case PagerMode.NextPrev:
                    this.modeCombo.SelectedIndex = 0;
                    break;

                case PagerMode.NumericPages:
                    this.modeCombo.SelectedIndex = 1;
                    break;
            }
            switch (pagerStyle.Position)
            {
                case PagerPosition.Bottom:
                    this.posCombo.SelectedIndex = 1;
                    break;

                case PagerPosition.Top:
                    this.posCombo.SelectedIndex = 0;
                    break;

                case PagerPosition.TopAndBottom:
                    this.posCombo.SelectedIndex = 2;
                    break;
            }
            this.nextPageTextEdit.Text = pagerStyle.NextPageText;
            this.prevPageTextEdit.Text = pagerStyle.PrevPageText;
            this.pageButtonCountEdit.Text = pagerStyle.PageButtonCount.ToString(NumberFormatInfo.CurrentInfo);
            this.UpdateEnabledVisibleState();
        }

        private void OnCheckChangedAllowCustomPaging(object source, EventArgs e)
        {
            if (!base.IsLoading())
            {
                this.SetDirty();
            }
        }

        private void OnCheckChangedAllowPaging(object source, EventArgs e)
        {
            if (!base.IsLoading())
            {
                this.SetDirty();
                this.UpdateEnabledVisibleState();
            }
        }

        private void OnCheckChangedVisible(object source, EventArgs e)
        {
            if (!base.IsLoading())
            {
                this.SetDirty();
                this.UpdateEnabledVisibleState();
            }
        }

        private void OnPagerChanged(object source, EventArgs e)
        {
            if (!base.IsLoading())
            {
                this.SetDirty();
                if (source == this.modeCombo)
                {
                    this.UpdateEnabledVisibleState();
                }
            }
        }

        private void OnTextChangedPageSize(object source, EventArgs e)
        {
            if (!base.IsLoading())
            {
                this.SetDirty();
                this.UpdateEnabledVisibleState();
            }
        }

        protected override void SaveComponent()
        {
            System.Web.UI.WebControls.DataGrid baseControl = (System.Web.UI.WebControls.DataGrid) base.GetBaseControl();
            DataGridPagerStyle pagerStyle = baseControl.PagerStyle;
            baseControl.AllowPaging = this.allowPagingCheck.Checked;
            baseControl.AllowCustomPaging = this.allowCustomPagingCheck.Checked;
            string s = this.pageSizeEdit.Text.Trim();
            if (s.Length != 0)
            {
                try
                {
                    baseControl.PageSize = int.Parse(s, CultureInfo.InvariantCulture);
                }
                catch
                {
                    this.pageSizeEdit.Text = baseControl.PageSize.ToString(NumberFormatInfo.CurrentInfo);
                }
            }
            pagerStyle.Visible = this.visibleCheck.Checked;
            switch (this.modeCombo.SelectedIndex)
            {
                case 0:
                    pagerStyle.Mode = PagerMode.NextPrev;
                    break;

                case 1:
                    pagerStyle.Mode = PagerMode.NumericPages;
                    break;
            }
            switch (this.posCombo.SelectedIndex)
            {
                case 0:
                    pagerStyle.Position = PagerPosition.Top;
                    break;

                case 1:
                    pagerStyle.Position = PagerPosition.Bottom;
                    break;

                case 2:
                    pagerStyle.Position = PagerPosition.TopAndBottom;
                    break;
            }
            pagerStyle.NextPageText = this.nextPageTextEdit.Text;
            pagerStyle.PrevPageText = this.prevPageTextEdit.Text;
            string str2 = this.pageButtonCountEdit.Text.Trim();
            if (str2.Length != 0)
            {
                try
                {
                    pagerStyle.PageButtonCount = int.Parse(str2, CultureInfo.InvariantCulture);
                }
                catch
                {
                    this.pageButtonCountEdit.Text = pagerStyle.PageButtonCount.ToString(NumberFormatInfo.CurrentInfo);
                }
            }
        }

        public override void SetComponent(IComponent component)
        {
            base.SetComponent(component);
            this.InitForm();
        }

        private void UpdateEnabledVisibleState()
        {
            int result = 0;
            string s = this.pageSizeEdit.Text.Trim();
            if (s.Length != 0)
            {
                int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
            }
            bool flag = this.allowPagingCheck.Checked;
            bool flag2 = flag && (result != 0);
            bool flag3 = this.visibleCheck.Checked;
            bool flag4 = this.modeCombo.SelectedIndex == 0;
            this.allowCustomPagingCheck.Enabled = flag;
            this.pageSizeEdit.Enabled = flag;
            this.visibleCheck.Enabled = flag2;
            this.posCombo.Enabled = flag2 && flag3;
            this.modeCombo.Enabled = flag2 && flag3;
            this.nextPageTextEdit.Enabled = (flag2 && flag3) && flag4;
            this.prevPageTextEdit.Enabled = (flag2 && flag3) && flag4;
            this.pageButtonCountEdit.Enabled = (flag2 && flag3) && !flag4;
        }

        protected override string HelpKeyword
        {
            get
            {
                return "net.Asp.DataGridProperties.Paging";
            }
        }
    }
}

