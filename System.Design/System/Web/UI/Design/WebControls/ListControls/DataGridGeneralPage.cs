namespace System.Web.UI.Design.WebControls.ListControls
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class DataGridGeneralPage : BaseDataListPage
    {
        private System.Windows.Forms.CheckBox allowSortingCheck;
        private System.Windows.Forms.CheckBox showFooterCheck;
        private System.Windows.Forms.CheckBox showHeaderCheck;

        private void InitForm()
        {
            GroupLabel label = new GroupLabel();
            this.showHeaderCheck = new System.Windows.Forms.CheckBox();
            this.showFooterCheck = new System.Windows.Forms.CheckBox();
            GroupLabel label2 = new GroupLabel();
            this.allowSortingCheck = new System.Windows.Forms.CheckBox();
            label.SetBounds(4, 4, 0x1af, 0x10);
            label.Text = System.Design.SR.GetString("DGGen_HeaderFooterGroup");
            label.TabIndex = 8;
            label.TabStop = false;
            this.showHeaderCheck.SetBounds(12, 0x18, 160, 0x10);
            this.showHeaderCheck.TabIndex = 9;
            this.showHeaderCheck.Text = System.Design.SR.GetString("DGGen_ShowHeader");
            this.showHeaderCheck.TextAlign = ContentAlignment.MiddleLeft;
            this.showHeaderCheck.FlatStyle = FlatStyle.System;
            this.showHeaderCheck.CheckedChanged += new EventHandler(this.OnCheckChangedShowHeader);
            this.showFooterCheck.SetBounds(12, 0x2c, 160, 0x10);
            this.showFooterCheck.TabIndex = 10;
            this.showFooterCheck.Text = System.Design.SR.GetString("DGGen_ShowFooter");
            this.showFooterCheck.TextAlign = ContentAlignment.MiddleLeft;
            this.showFooterCheck.FlatStyle = FlatStyle.System;
            this.showFooterCheck.CheckedChanged += new EventHandler(this.OnCheckChangedShowFooter);
            label2.SetBounds(4, 70, 0x1af, 0x10);
            label2.Text = System.Design.SR.GetString("DGGen_BehaviorGroup");
            label2.TabIndex = 11;
            label2.TabStop = false;
            this.allowSortingCheck.SetBounds(12, 0x58, 160, 0x10);
            this.allowSortingCheck.Text = System.Design.SR.GetString("DGGen_AllowSorting");
            this.allowSortingCheck.TabIndex = 12;
            this.allowSortingCheck.TextAlign = ContentAlignment.MiddleLeft;
            this.allowSortingCheck.FlatStyle = FlatStyle.System;
            this.allowSortingCheck.CheckedChanged += new EventHandler(this.OnCheckChangedAllowSorting);
            this.Text = System.Design.SR.GetString("DGGen_Text");
            base.AccessibleDescription = System.Design.SR.GetString("DGGen_Desc");
            base.Size = new Size(0x1d0, 0x110);
            base.CommitOnDeactivate = true;
            base.Icon = new Icon(base.GetType(), "DataGridGeneralPage.ico");
            base.Controls.Clear();
            base.Controls.AddRange(new Control[] { this.allowSortingCheck, label2, this.showFooterCheck, this.showHeaderCheck, label });
        }

        private void InitPage()
        {
            this.showHeaderCheck.Checked = false;
            this.showFooterCheck.Checked = false;
            this.allowSortingCheck.Checked = false;
        }

        protected override void LoadComponent()
        {
            this.InitPage();
            System.Web.UI.WebControls.DataGrid baseControl = (System.Web.UI.WebControls.DataGrid) base.GetBaseControl();
            this.showHeaderCheck.Checked = baseControl.ShowHeader;
            this.showFooterCheck.Checked = baseControl.ShowFooter;
            this.allowSortingCheck.Checked = baseControl.AllowSorting;
        }

        private void OnCheckChangedAllowSorting(object source, EventArgs e)
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
            System.Web.UI.WebControls.DataGrid baseControl = (System.Web.UI.WebControls.DataGrid) base.GetBaseControl();
            baseControl.ShowHeader = this.showHeaderCheck.Checked;
            baseControl.ShowFooter = this.showFooterCheck.Checked;
            baseControl.AllowSorting = this.allowSortingCheck.Checked;
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
                return "net.Asp.DataGridProperties.General";
            }
        }
    }
}

