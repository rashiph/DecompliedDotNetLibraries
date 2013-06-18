namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal interface IWorkflowDesignerMessageSink
    {
        void OnBeginResizing(DesignerEdges sizingEdge);
        bool OnDragDrop(DragEventArgs e);
        bool OnDragEnter(DragEventArgs e);
        bool OnDragLeave();
        bool OnDragOver(DragEventArgs e);
        void OnEndResizing();
        bool OnGiveFeedback(GiveFeedbackEventArgs e);
        bool OnKeyDown(KeyEventArgs e);
        bool OnKeyUp(KeyEventArgs e);
        void OnLayout(LayoutEventArgs layoutEventArgs);
        void OnLayoutPosition(Graphics graphics);
        void OnLayoutSize(Graphics graphics);
        bool OnMouseCaptureChanged();
        bool OnMouseDoubleClick(MouseEventArgs e);
        bool OnMouseDown(MouseEventArgs e);
        bool OnMouseDragBegin(Point initialPoint, MouseEventArgs e);
        bool OnMouseDragEnd();
        bool OnMouseDragMove(MouseEventArgs e);
        bool OnMouseEnter(MouseEventArgs e);
        bool OnMouseHover(MouseEventArgs e);
        bool OnMouseLeave();
        bool OnMouseMove(MouseEventArgs e);
        bool OnMouseUp(MouseEventArgs e);
        bool OnMouseWheel(MouseEventArgs e);
        bool OnPaint(PaintEventArgs e, Rectangle viewPort);
        bool OnPaintWorkflowAdornments(PaintEventArgs e, Rectangle viewPort);
        bool OnQueryContinueDrag(QueryContinueDragEventArgs e);
        void OnResizing(DesignerEdges sizingEdge, Rectangle bounds);
        bool OnScroll(ScrollBar sender, int value);
        bool OnShowContextMenu(Point screenMenuPoint);
        void OnThemeChange();
        bool ProcessMessage(Message message);
    }
}

