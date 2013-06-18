namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class MdiWindowDialog : Form
    {
        private Form active;
        private Button cancelButton;
        private ListBox itemList;
        private Button okButton;
        private TableLayoutPanel okCancelTableLayoutPanel;

        public MdiWindowDialog()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(MdiWindowDialog));
            this.itemList = new ListBox();
            this.okButton = new Button();
            this.cancelButton = new Button();
            this.okCancelTableLayoutPanel = new TableLayoutPanel();
            this.okCancelTableLayoutPanel.SuspendLayout();
            this.itemList.DoubleClick += new EventHandler(this.ItemList_doubleClick);
            this.itemList.SelectedIndexChanged += new EventHandler(this.ItemList_selectedIndexChanged);
            base.SuspendLayout();
            manager.ApplyResources(this.itemList, "itemList");
            this.itemList.FormattingEnabled = true;
            this.itemList.Name = "itemList";
            manager.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = DialogResult.OK;
            this.okButton.Margin = new Padding(0, 0, 3, 0);
            this.okButton.Name = "okButton";
            manager.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Margin = new Padding(3, 0, 0, 0);
            this.cancelButton.Name = "cancelButton";
            manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.ColumnCount = 2;
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okCancelTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            this.okCancelTableLayoutPanel.RowCount = 1;
            this.okCancelTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this, "$this");
            base.AutoScaleMode = AutoScaleMode.Font;
            base.Controls.Add(this.okCancelTableLayoutPanel);
            base.Controls.Add(this.itemList);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "MdiWindowDialog";
            base.ShowIcon = false;
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            base.AcceptButton = this.okButton;
            base.CancelButton = this.cancelButton;
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void ItemList_doubleClick(object source, EventArgs e)
        {
            this.okButton.PerformClick();
        }

        private void ItemList_selectedIndexChanged(object source, EventArgs e)
        {
            ListItem selectedItem = (ListItem) this.itemList.SelectedItem;
            if (selectedItem != null)
            {
                this.active = selectedItem.form;
            }
        }

        public void SetItems(Form active, Form[] all)
        {
            int num = 0;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].Visible)
                {
                    int num3 = this.itemList.Items.Add(new ListItem(all[i]));
                    if (all[i].Equals(active))
                    {
                        num = num3;
                    }
                }
            }
            this.active = active;
            this.itemList.SelectedIndex = num;
        }

        public Form ActiveChildForm
        {
            get
            {
                return this.active;
            }
        }

        private class ListItem
        {
            public Form form;

            public ListItem(Form f)
            {
                this.form = f;
            }

            public override string ToString()
            {
                return this.form.Text;
            }
        }
    }
}

