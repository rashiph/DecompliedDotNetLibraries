namespace System.Windows.Forms
{
    using System;

    public class TreeNodeMouseClickEventArgs : MouseEventArgs
    {
        private TreeNode node;

        public TreeNodeMouseClickEventArgs(TreeNode node, MouseButtons button, int clicks, int x, int y) : base(button, clicks, x, y, 0)
        {
            this.node = node;
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

