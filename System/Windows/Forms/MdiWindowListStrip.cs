namespace System.Windows.Forms
{
    using System;
    using System.Globalization;
    using System.Security;

    internal class MdiWindowListStrip : MenuStrip
    {
        private Form mdiParent;
        private MenuStrip mergedMenu;
        private ToolStripMenuItem mergeItem;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.mdiParent = null;
            }
            base.Dispose(disposing);
        }

        private void OnMoreWindowsMenuItemClick(object sender, EventArgs e)
        {
            Form[] mdiChildren = this.mdiParent.MdiChildren;
            if (mdiChildren != null)
            {
                IntSecurity.AllWindows.Assert();
                try
                {
                    using (MdiWindowDialog dialog = new MdiWindowDialog())
                    {
                        dialog.SetItems(this.mdiParent.ActiveMdiChild, mdiChildren);
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            dialog.ActiveChildForm.Activate();
                            if ((dialog.ActiveChildForm.ActiveControl != null) && !dialog.ActiveChildForm.ActiveControl.Focused)
                            {
                                dialog.ActiveChildForm.ActiveControl.Focus();
                            }
                        }
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
        }

        private void OnWindowListItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
                Form mdiForm = item.MdiForm;
                if (mdiForm != null)
                {
                    IntSecurity.ModifyFocus.Assert();
                    try
                    {
                        mdiForm.Activate();
                        if ((mdiForm.ActiveControl != null) && !mdiForm.ActiveControl.Focused)
                        {
                            mdiForm.ActiveControl.Focus();
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
        }

        public void PopulateItems(Form mdiParent, ToolStripMenuItem mdiMergeItem, bool includeSeparator)
        {
            this.mdiParent = mdiParent;
            base.SuspendLayout();
            this.MergeItem.DropDown.SuspendLayout();
            try
            {
                ToolStripMenuItem mergeItem = this.MergeItem;
                mergeItem.DropDownItems.Clear();
                mergeItem.Text = mdiMergeItem.Text;
                Form[] mdiChildren = mdiParent.MdiChildren;
                if ((mdiChildren != null) && (mdiChildren.Length != 0))
                {
                    if (includeSeparator)
                    {
                        ToolStripSeparator separator = new ToolStripSeparator {
                            MergeAction = MergeAction.Append,
                            MergeIndex = -1
                        };
                        mergeItem.DropDownItems.Add(separator);
                    }
                    Form activeMdiChild = mdiParent.ActiveMdiChild;
                    int num = 0;
                    int num2 = 1;
                    int num3 = 0;
                    bool flag = false;
                    for (int i = 0; i < mdiChildren.Length; i++)
                    {
                        if (mdiChildren[i].Visible && (mdiChildren[i].CloseReason == CloseReason.None))
                        {
                            num++;
                            if (((flag && (num3 < 9)) || (!flag && (num3 < 8))) || mdiChildren[i].Equals(activeMdiChild))
                            {
                                string str = WindowsFormsUtils.EscapeTextWithAmpersands(mdiParent.MdiChildren[i].Text);
                                str = (str == null) ? string.Empty : str;
                                ToolStripMenuItem item2 = new ToolStripMenuItem(mdiParent.MdiChildren[i]) {
                                    Text = string.Format(CultureInfo.CurrentCulture, "&{0} {1}", new object[] { num2, str }),
                                    MergeAction = MergeAction.Append,
                                    MergeIndex = num2
                                };
                                item2.Click += new EventHandler(this.OnWindowListItemClick);
                                if (mdiChildren[i].Equals(activeMdiChild))
                                {
                                    item2.Checked = true;
                                    flag = true;
                                }
                                num2++;
                                num3++;
                                mergeItem.DropDownItems.Add(item2);
                            }
                        }
                    }
                    if (num > 9)
                    {
                        ToolStripMenuItem item3 = new ToolStripMenuItem {
                            Text = System.Windows.Forms.SR.GetString("MDIMenuMoreWindows")
                        };
                        item3.Click += new EventHandler(this.OnMoreWindowsMenuItemClick);
                        item3.MergeAction = MergeAction.Append;
                        mergeItem.DropDownItems.Add(item3);
                    }
                }
            }
            finally
            {
                base.ResumeLayout(false);
                this.MergeItem.DropDown.ResumeLayout(false);
            }
        }

        internal MenuStrip MergedMenu
        {
            get
            {
                return this.mergedMenu;
            }
            set
            {
                this.mergedMenu = value;
            }
        }

        internal ToolStripMenuItem MergeItem
        {
            get
            {
                if (this.mergeItem == null)
                {
                    this.mergeItem = new ToolStripMenuItem();
                    this.mergeItem.MergeAction = MergeAction.MatchOnly;
                }
                if (this.mergeItem.Owner == null)
                {
                    this.Items.Add(this.mergeItem);
                }
                return this.mergeItem;
            }
        }
    }
}

