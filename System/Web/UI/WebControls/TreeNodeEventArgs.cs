namespace System.Web.UI.WebControls
{
    using System;

    public sealed class TreeNodeEventArgs : EventArgs
    {
        private TreeNode _node;

        public TreeNodeEventArgs(TreeNode node)
        {
            this._node = node;
        }

        public TreeNode Node
        {
            get
            {
                return this._node;
            }
        }
    }
}

