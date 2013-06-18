namespace System.Windows.Forms.Design
{
    using System;
    using System.Drawing;

    internal interface ISelectionUIService
    {
        event ContainerSelectorActiveEventHandler ContainerSelectorActive;

        void AssignSelectionUIHandler(object component, ISelectionUIHandler handler);
        bool BeginDrag(SelectionRules rules, int initialX, int initialY);
        void ClearSelectionUIHandler(object component, ISelectionUIHandler handler);
        void DragMoved(Rectangle offset);
        void EndDrag(bool cancel);
        object[] FilterSelection(object[] components, SelectionRules selectionRules);
        Size GetAdornmentDimensions(AdornmentType adornmentType);
        bool GetAdornmentHitTest(object component, Point pt);
        bool GetContainerSelected(object component);
        SelectionRules GetSelectionRules(object component);
        SelectionStyles GetSelectionStyle(object component);
        void SetContainerSelected(object component, bool selected);
        void SetSelectionStyle(object component, SelectionStyles style);
        void SyncComponent(object component);
        void SyncSelection();

        bool Dragging { get; }

        bool Visible { get; set; }
    }
}

