namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal interface ISelectionUIHandler
    {
        bool BeginDrag(object[] components, SelectionRules rules, int initialX, int initialY);
        void DragMoved(object[] components, Rectangle offset);
        void EndDrag(object[] components, bool cancel);
        Rectangle GetComponentBounds(object component);
        SelectionRules GetComponentRules(object component);
        Rectangle GetSelectionClipRect(object component);
        void OleDragDrop(DragEventArgs de);
        void OleDragEnter(DragEventArgs de);
        void OleDragLeave();
        void OleDragOver(DragEventArgs de);
        void OnSelectionDoubleClick(IComponent component);
        bool QueryBeginDrag(object[] components, SelectionRules rules, int initialX, int initialY);
        void ShowContextMenu(IComponent component);
    }
}

