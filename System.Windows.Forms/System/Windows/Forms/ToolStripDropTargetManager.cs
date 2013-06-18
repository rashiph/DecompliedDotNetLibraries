namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    internal class ToolStripDropTargetManager : IDropTarget
    {
        internal static readonly TraceSwitch DragDropDebug;
        private IDropTarget lastDropTarget;
        private ToolStrip owner;

        public ToolStripDropTargetManager(ToolStrip owner)
        {
            this.owner = owner;
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
        }

        public void EnsureRegistered(IDropTarget dropTarget)
        {
            this.SetAcceptDrops(true);
        }

        public void EnsureUnRegistered(IDropTarget dropTarget)
        {
            for (int i = 0; i < this.owner.Items.Count; i++)
            {
                if (this.owner.Items[i].AllowDrop)
                {
                    return;
                }
            }
            if (!this.owner.AllowDrop && !this.owner.AllowItemReorder)
            {
                this.SetAcceptDrops(false);
                this.owner.DropTargetManager = null;
            }
        }

        private ToolStripItem FindItemAtPoint(int x, int y)
        {
            return this.owner.GetItemAt(this.owner.PointToClient(new Point(x, y)));
        }

        public void OnDragDrop(DragEventArgs e)
        {
            if (this.lastDropTarget != null)
            {
                this.lastDropTarget.OnDragDrop(e);
            }
            this.lastDropTarget = null;
        }

        public void OnDragEnter(DragEventArgs e)
        {
            if (this.owner.AllowItemReorder && e.Data.GetDataPresent(typeof(ToolStripItem)))
            {
                this.lastDropTarget = this.owner.ItemReorderDropTarget;
            }
            else
            {
                ToolStripItem item = this.FindItemAtPoint(e.X, e.Y);
                if ((item != null) && item.AllowDrop)
                {
                    this.lastDropTarget = item;
                }
                else if (this.owner.AllowDrop)
                {
                    this.lastDropTarget = this.owner;
                }
                else
                {
                    this.lastDropTarget = null;
                }
            }
            if (this.lastDropTarget != null)
            {
                this.lastDropTarget.OnDragEnter(e);
            }
        }

        public void OnDragLeave(EventArgs e)
        {
            if (this.lastDropTarget != null)
            {
                this.lastDropTarget.OnDragLeave(e);
            }
            this.lastDropTarget = null;
        }

        public void OnDragOver(DragEventArgs e)
        {
            IDropTarget newTarget = null;
            if (this.owner.AllowItemReorder && e.Data.GetDataPresent(typeof(ToolStripItem)))
            {
                newTarget = this.owner.ItemReorderDropTarget;
            }
            else
            {
                ToolStripItem item = this.FindItemAtPoint(e.X, e.Y);
                if ((item != null) && item.AllowDrop)
                {
                    newTarget = item;
                }
                else if (this.owner.AllowDrop)
                {
                    newTarget = this.owner;
                }
                else
                {
                    newTarget = null;
                }
            }
            if (newTarget != this.lastDropTarget)
            {
                this.UpdateDropTarget(newTarget, e);
            }
            if (this.lastDropTarget != null)
            {
                this.lastDropTarget.OnDragOver(e);
            }
        }

        private void SetAcceptDrops(bool accept)
        {
            if (this.owner.AllowDrop && accept)
            {
                System.Windows.Forms.IntSecurity.ClipboardRead.Demand();
            }
            if (accept && this.owner.IsHandleCreated)
            {
                try
                {
                    if (Application.OleRequired() != ApartmentState.STA)
                    {
                        throw new ThreadStateException(System.Windows.Forms.SR.GetString("ThreadMustBeSTA"));
                    }
                    if (accept)
                    {
                        int error = System.Windows.Forms.UnsafeNativeMethods.RegisterDragDrop(new HandleRef(this.owner, this.owner.Handle), new DropTarget(this));
                        if ((error != 0) && (error != -2147221247))
                        {
                            throw new Win32Exception(error);
                        }
                    }
                    else
                    {
                        System.Windows.Forms.IntSecurity.ClipboardRead.Assert();
                        try
                        {
                            int num2 = System.Windows.Forms.UnsafeNativeMethods.RevokeDragDrop(new HandleRef(this.owner, this.owner.Handle));
                            if ((num2 != 0) && (num2 != -2147221248))
                            {
                                throw new Win32Exception(num2);
                            }
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DragDropRegFailed"), exception);
                }
            }
        }

        private void UpdateDropTarget(IDropTarget newTarget, DragEventArgs e)
        {
            if (newTarget != this.lastDropTarget)
            {
                if (this.lastDropTarget != null)
                {
                    this.OnDragLeave(new EventArgs());
                }
                this.lastDropTarget = newTarget;
                if (newTarget != null)
                {
                    DragEventArgs args = new DragEventArgs(e.Data, e.KeyState, e.X, e.Y, e.AllowedEffect, e.Effect) {
                        Effect = DragDropEffects.None
                    };
                    this.OnDragEnter(args);
                }
            }
        }
    }
}

