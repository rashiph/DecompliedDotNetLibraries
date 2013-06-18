namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices.ComTypes;

    internal class DropTarget : UnsafeNativeMethods.IOleDropTarget
    {
        private System.Windows.Forms.IDataObject lastDataObject;
        private DragDropEffects lastEffect;
        private IDropTarget owner;

        public DropTarget(IDropTarget owner)
        {
            this.owner = owner;
        }

        private DragEventArgs CreateDragEventArgs(object pDataObj, int grfKeyState, NativeMethods.POINTL pt, int pdwEffect)
        {
            System.Windows.Forms.IDataObject data = null;
            if (pDataObj == null)
            {
                data = this.lastDataObject;
            }
            else if (pDataObj is System.Windows.Forms.IDataObject)
            {
                data = (System.Windows.Forms.IDataObject) pDataObj;
            }
            else if (pDataObj is System.Runtime.InteropServices.ComTypes.IDataObject)
            {
                data = new DataObject(pDataObj);
            }
            else
            {
                return null;
            }
            DragEventArgs args = new DragEventArgs(data, grfKeyState, pt.x, pt.y, (DragDropEffects) pdwEffect, this.lastEffect);
            this.lastDataObject = data;
            return args;
        }

        private int GetX(long pt)
        {
            return (int) (((ulong) pt) & 0xffffffffL);
        }

        private int GetY(long pt)
        {
            return (int) (((ulong) (pt >> 0x20)) & 0xffffffffL);
        }

        int UnsafeNativeMethods.IOleDropTarget.OleDragEnter(object pDataObj, int grfKeyState, long pt, ref int pdwEffect)
        {
            NativeMethods.POINTL pointl = new NativeMethods.POINTL {
                x = this.GetX(pt),
                y = this.GetY(pt)
            };
            DragEventArgs e = this.CreateDragEventArgs(pDataObj, grfKeyState, pointl, pdwEffect);
            if (e != null)
            {
                this.owner.OnDragEnter(e);
                pdwEffect = (int) e.Effect;
                this.lastEffect = e.Effect;
            }
            else
            {
                pdwEffect = 0;
            }
            return 0;
        }

        int UnsafeNativeMethods.IOleDropTarget.OleDragLeave()
        {
            this.owner.OnDragLeave(EventArgs.Empty);
            return 0;
        }

        int UnsafeNativeMethods.IOleDropTarget.OleDragOver(int grfKeyState, long pt, ref int pdwEffect)
        {
            NativeMethods.POINTL pointl = new NativeMethods.POINTL {
                x = this.GetX(pt),
                y = this.GetY(pt)
            };
            DragEventArgs e = this.CreateDragEventArgs(null, grfKeyState, pointl, pdwEffect);
            this.owner.OnDragOver(e);
            pdwEffect = (int) e.Effect;
            this.lastEffect = e.Effect;
            return 0;
        }

        int UnsafeNativeMethods.IOleDropTarget.OleDrop(object pDataObj, int grfKeyState, long pt, ref int pdwEffect)
        {
            NativeMethods.POINTL pointl = new NativeMethods.POINTL {
                x = this.GetX(pt),
                y = this.GetY(pt)
            };
            DragEventArgs e = this.CreateDragEventArgs(pDataObj, grfKeyState, pointl, pdwEffect);
            if (e != null)
            {
                this.owner.OnDragDrop(e);
                pdwEffect = (int) e.Effect;
            }
            else
            {
                pdwEffect = 0;
            }
            this.lastEffect = DragDropEffects.None;
            this.lastDataObject = null;
            return 0;
        }
    }
}

