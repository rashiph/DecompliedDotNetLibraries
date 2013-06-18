namespace System.Windows.Forms
{
    using System;

    public interface IDropTarget
    {
        void OnDragDrop(DragEventArgs e);
        void OnDragEnter(DragEventArgs e);
        void OnDragLeave(EventArgs e);
        void OnDragOver(DragEventArgs e);
    }
}

