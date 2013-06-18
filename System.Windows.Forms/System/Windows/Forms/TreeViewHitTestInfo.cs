namespace System.Windows.Forms
{
    using System;

    public class TreeViewHitTestInfo
    {
        private TreeViewHitTestLocations loc;
        private TreeNode node;

        public TreeViewHitTestInfo(TreeNode hitNode, TreeViewHitTestLocations hitLocation)
        {
            this.node = hitNode;
            this.loc = hitLocation;
        }

        public TreeViewHitTestLocations Location
        {
            get
            {
                return this.loc;
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

