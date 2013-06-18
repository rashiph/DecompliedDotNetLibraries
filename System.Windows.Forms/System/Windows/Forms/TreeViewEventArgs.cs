namespace System.Windows.Forms
{
    using System;

    public class TreeViewEventArgs : EventArgs
    {
        private TreeViewAction action;
        private TreeNode node;

        public TreeViewEventArgs(TreeNode node)
        {
            this.node = node;
        }

        public TreeViewEventArgs(TreeNode node, TreeViewAction action)
        {
            this.node = node;
            this.action = action;
        }

        public TreeViewAction Action
        {
            get
            {
                return this.action;
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

