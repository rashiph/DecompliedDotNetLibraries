namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public class TreeViewCancelEventArgs : CancelEventArgs
    {
        private TreeViewAction action;
        private TreeNode node;

        public TreeViewCancelEventArgs(TreeNode node, bool cancel, TreeViewAction action) : base(cancel)
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

