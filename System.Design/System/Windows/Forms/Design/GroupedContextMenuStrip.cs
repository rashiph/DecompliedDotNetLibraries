namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows.Forms;

    internal class GroupedContextMenuStrip : ContextMenuStrip
    {
        private StringCollection groupOrdering;
        private ContextMenuStripGroupCollection groups;
        private bool populated;

        protected override void OnOpening(CancelEventArgs e)
        {
            base.SuspendLayout();
            if (!this.populated)
            {
                this.Populate();
            }
            this.RefreshItems();
            base.ResumeLayout(true);
            base.PerformLayout();
            e.Cancel = this.Items.Count == 0;
            base.OnOpening(e);
        }

        public void Populate()
        {
            this.Items.Clear();
            foreach (string str in this.GroupOrdering)
            {
                if (this.groups.ContainsKey(str))
                {
                    List<ToolStripItem> items = this.groups[str].Items;
                    if ((this.Items.Count > 0) && (items.Count > 0))
                    {
                        this.Items.Add(new ToolStripSeparator());
                    }
                    foreach (ToolStripItem item in items)
                    {
                        this.Items.Add(item);
                    }
                }
            }
            this.populated = true;
        }

        public virtual void RefreshItems()
        {
        }

        public StringCollection GroupOrdering
        {
            get
            {
                if (this.groupOrdering == null)
                {
                    this.groupOrdering = new StringCollection();
                }
                return this.groupOrdering;
            }
        }

        public ContextMenuStripGroupCollection Groups
        {
            get
            {
                if (this.groups == null)
                {
                    this.groups = new ContextMenuStripGroupCollection();
                }
                return this.groups;
            }
        }

        public bool Populated
        {
            set
            {
                this.populated = value;
            }
        }
    }
}

