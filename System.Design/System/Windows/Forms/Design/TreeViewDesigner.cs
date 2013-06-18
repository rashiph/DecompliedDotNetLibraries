namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal class TreeViewDesigner : ControlDesigner
    {
        private DesignerActionListCollection _actionLists;
        private TreeView treeView;
        private System.Design.NativeMethods.TV_HITTESTINFO tvhit = new System.Design.NativeMethods.TV_HITTESTINFO();

        public TreeViewDesigner()
        {
            base.AutoResizeHandles = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.treeView != null))
            {
                this.treeView.AfterExpand -= new TreeViewEventHandler(this.TreeViewInvalidate);
                this.treeView.AfterCollapse -= new TreeViewEventHandler(this.TreeViewInvalidate);
                this.treeView = null;
            }
            base.Dispose(disposing);
        }

        protected override bool GetHitTest(Point point)
        {
            point = this.Control.PointToClient(point);
            this.tvhit.pt_x = point.X;
            this.tvhit.pt_y = point.Y;
            System.Design.NativeMethods.SendMessage(this.Control.Handle, 0x1111, 0, this.tvhit);
            return (this.tvhit.flags == 0x10);
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            this.treeView = component as TreeView;
            if (this.treeView != null)
            {
                this.treeView.AfterExpand += new TreeViewEventHandler(this.TreeViewInvalidate);
                this.treeView.AfterCollapse += new TreeViewEventHandler(this.TreeViewInvalidate);
            }
        }

        private void TreeViewInvalidate(object sender, TreeViewEventArgs e)
        {
            if (this.treeView != null)
            {
                this.treeView.Invalidate();
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (this._actionLists == null)
                {
                    this._actionLists = new DesignerActionListCollection();
                    this._actionLists.Add(new TreeViewActionList(this));
                }
                return this._actionLists;
            }
        }
    }
}

