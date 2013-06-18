namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections.Generic;

    internal class ContextMenuStripGroup
    {
        private List<ToolStripItem> items;
        private string name;

        public ContextMenuStripGroup(string name)
        {
            this.name = name;
        }

        public List<ToolStripItem> Items
        {
            get
            {
                if (this.items == null)
                {
                    this.items = new List<ToolStripItem>();
                }
                return this.items;
            }
        }
    }
}

