namespace System.Windows.Forms
{
    using System;

    public class NodeLabelEditEventArgs : EventArgs
    {
        private bool cancelEdit;
        private readonly string label;
        private readonly TreeNode node;

        public NodeLabelEditEventArgs(TreeNode node)
        {
            this.node = node;
            this.label = null;
        }

        public NodeLabelEditEventArgs(TreeNode node, string label)
        {
            this.node = node;
            this.label = label;
        }

        public bool CancelEdit
        {
            get
            {
                return this.cancelEdit;
            }
            set
            {
                this.cancelEdit = value;
            }
        }

        public string Label
        {
            get
            {
                return this.label;
            }
        }

        public TreeNode Node
        {
            get
            {
                return this.node;
            }
        }
    }
}

