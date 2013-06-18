namespace System.Windows.Forms
{
    using System;

    internal class ToolStripItemImageIndexer : System.Windows.Forms.ImageList.Indexer
    {
        private ToolStripItem item;

        public ToolStripItemImageIndexer(ToolStripItem item)
        {
            this.item = item;
        }

        public override System.Windows.Forms.ImageList ImageList
        {
            get
            {
                if ((this.item != null) && (this.item.Owner != null))
                {
                    return this.item.Owner.ImageList;
                }
                return null;
            }
            set
            {
            }
        }
    }
}

