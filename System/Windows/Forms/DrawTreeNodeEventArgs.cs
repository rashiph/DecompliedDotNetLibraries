namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class DrawTreeNodeEventArgs : EventArgs
    {
        private readonly Rectangle bounds;
        private bool drawDefault;
        private readonly System.Drawing.Graphics graphics;
        private readonly TreeNode node;
        private readonly TreeNodeStates state;

        public DrawTreeNodeEventArgs(System.Drawing.Graphics graphics, TreeNode node, Rectangle bounds, TreeNodeStates state)
        {
            this.graphics = graphics;
            this.node = node;
            this.bounds = bounds;
            this.state = state;
            this.drawDefault = false;
        }

        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        public bool DrawDefault
        {
            get
            {
                return this.drawDefault;
            }
            set
            {
                this.drawDefault = value;
            }
        }

        public System.Drawing.Graphics Graphics
        {
            get
            {
                return this.graphics;
            }
        }

        public TreeNode Node
        {
            get
            {
                return this.node;
            }
        }

        public TreeNodeStates State
        {
            get
            {
                return this.state;
            }
        }
    }
}

