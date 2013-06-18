namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class TreeNodeMouseHoverEventArgs : EventArgs
    {
        private readonly TreeNode node;

        public TreeNodeMouseHoverEventArgs(TreeNode node)
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

