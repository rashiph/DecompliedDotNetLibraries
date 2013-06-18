namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Runtime;
    using System.Windows.Forms;

    public abstract class WorkflowDesignerMessageFilter : IDisposable, IWorkflowDesignerMessageSink
    {
        private WorkflowView parentView;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected WorkflowDesignerMessageFilter()
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ~WorkflowDesignerMessageFilter()
        {
            this.Dispose(false);
        }

        internal object GetService(System.Type serviceType)
        {
            object service = null;
            if (this.parentView != null)
            {
                service = ((IServiceProvider) this.parentView).GetService(serviceType);
            }
            return service;
        }

        protected virtual void Initialize(WorkflowView parentView)
        {
            this.parentView = parentView;
        }

        protected virtual bool OnDragDrop(DragEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnDragEnter(DragEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnDragLeave()
        {
            return false;
        }

        protected virtual bool OnDragOver(DragEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnGiveFeedback(GiveFeedbackEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnKeyDown(KeyEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnKeyUp(KeyEventArgs eventArgs)
        {
            return false;
        }

        protected virtual void OnLayout(LayoutEventArgs eventArgs)
        {
        }

        protected virtual bool OnMouseCaptureChanged()
        {
            return false;
        }

        protected virtual bool OnMouseDoubleClick(MouseEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnMouseDown(MouseEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnMouseHover(MouseEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnMouseLeave()
        {
            return false;
        }

        protected virtual bool OnMouseMove(MouseEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnMouseUp(MouseEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnMouseWheel(MouseEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnPaint(PaintEventArgs eventArgs, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            return false;
        }

        protected virtual bool OnPaintWorkflowAdornments(PaintEventArgs eventArgs, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            return false;
        }

        protected virtual bool OnQueryContinueDrag(QueryContinueDragEventArgs eventArgs)
        {
            return false;
        }

        protected virtual bool OnScroll(ScrollBar sender, int value)
        {
            return false;
        }

        protected virtual bool OnShowContextMenu(Point screenMenuPoint)
        {
            return false;
        }

        protected virtual void OnThemeChange()
        {
        }

        protected virtual bool ProcessMessage(Message message)
        {
            return false;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal void SetParentView(WorkflowView parentView)
        {
            this.Initialize(parentView);
        }

        void IWorkflowDesignerMessageSink.OnBeginResizing(DesignerEdges sizingEdge)
        {
        }

        bool IWorkflowDesignerMessageSink.OnDragDrop(DragEventArgs eventArgs)
        {
            bool flag = false;
            try
            {
                flag = this.OnDragDrop(eventArgs);
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnDragEnter(DragEventArgs eventArgs)
        {
            bool flag = false;
            try
            {
                flag = this.OnDragEnter(eventArgs);
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnDragLeave()
        {
            bool flag = false;
            try
            {
                flag = this.OnDragLeave();
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnDragOver(DragEventArgs eventArgs)
        {
            bool flag = false;
            try
            {
                flag = this.OnDragOver(eventArgs);
            }
            catch
            {
            }
            return flag;
        }

        void IWorkflowDesignerMessageSink.OnEndResizing()
        {
        }

        bool IWorkflowDesignerMessageSink.OnGiveFeedback(GiveFeedbackEventArgs eventArgs)
        {
            bool flag = false;
            try
            {
                flag = this.OnGiveFeedback(eventArgs);
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnKeyDown(KeyEventArgs eventArgs)
        {
            bool flag = false;
            try
            {
                flag = this.OnKeyDown(eventArgs);
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnKeyUp(KeyEventArgs eventArgs)
        {
            bool flag = false;
            try
            {
                flag = this.OnKeyUp(eventArgs);
            }
            catch
            {
            }
            return flag;
        }

        void IWorkflowDesignerMessageSink.OnLayout(LayoutEventArgs layoutEventArgs)
        {
            try
            {
                this.OnLayout(layoutEventArgs);
            }
            catch
            {
            }
        }

        void IWorkflowDesignerMessageSink.OnLayoutPosition(Graphics graphics)
        {
        }

        void IWorkflowDesignerMessageSink.OnLayoutSize(Graphics graphics)
        {
        }

        bool IWorkflowDesignerMessageSink.OnMouseCaptureChanged()
        {
            bool flag = false;
            try
            {
                flag = this.OnMouseCaptureChanged();
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDoubleClick(MouseEventArgs eventArgs)
        {
            bool flag = false;
            try
            {
                flag = this.OnMouseDoubleClick(eventArgs);
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDown(MouseEventArgs eventArgs)
        {
            bool flag = false;
            try
            {
                flag = this.OnMouseDown(eventArgs);
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDragBegin(Point initialPoint, MouseEventArgs eventArgs)
        {
            return false;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDragEnd()
        {
            return false;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDragMove(MouseEventArgs eventArgs)
        {
            return false;
        }

        bool IWorkflowDesignerMessageSink.OnMouseEnter(MouseEventArgs eventArgs)
        {
            bool flag = false;
            try
            {
                flag = this.OnMouseEnter(eventArgs);
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnMouseHover(MouseEventArgs eventArgs)
        {
            bool flag = false;
            try
            {
                flag = this.OnMouseHover(eventArgs);
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnMouseLeave()
        {
            bool flag = false;
            try
            {
                flag = this.OnMouseLeave();
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnMouseMove(MouseEventArgs eventArgs)
        {
            bool flag = false;
            try
            {
                flag = this.OnMouseMove(eventArgs);
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnMouseUp(MouseEventArgs eventArgs)
        {
            bool flag = false;
            try
            {
                flag = this.OnMouseUp(eventArgs);
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnMouseWheel(MouseEventArgs eventArgs)
        {
            bool flag = false;
            try
            {
                flag = this.OnMouseWheel(eventArgs);
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnPaint(PaintEventArgs eventArgs, Rectangle viewPort)
        {
            bool flag = false;
            try
            {
                flag = this.OnPaint(eventArgs, viewPort, WorkflowTheme.CurrentTheme.AmbientTheme);
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnPaintWorkflowAdornments(PaintEventArgs eventArgs, Rectangle viewPort)
        {
            bool flag = false;
            try
            {
                flag = this.OnPaintWorkflowAdornments(eventArgs, viewPort, WorkflowTheme.CurrentTheme.AmbientTheme);
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnQueryContinueDrag(QueryContinueDragEventArgs eventArgs)
        {
            bool flag = false;
            try
            {
                flag = this.OnQueryContinueDrag(eventArgs);
            }
            catch
            {
            }
            return flag;
        }

        void IWorkflowDesignerMessageSink.OnResizing(DesignerEdges sizingEdge, Rectangle bounds)
        {
        }

        bool IWorkflowDesignerMessageSink.OnScroll(ScrollBar sender, int value)
        {
            bool flag = false;
            try
            {
                flag = this.OnScroll(sender, value);
            }
            catch
            {
            }
            return flag;
        }

        bool IWorkflowDesignerMessageSink.OnShowContextMenu(Point screenMenuPoint)
        {
            bool flag = false;
            try
            {
                flag = this.OnShowContextMenu(screenMenuPoint);
            }
            catch
            {
            }
            return flag;
        }

        void IWorkflowDesignerMessageSink.OnThemeChange()
        {
            try
            {
                this.OnThemeChange();
            }
            catch
            {
            }
        }

        bool IWorkflowDesignerMessageSink.ProcessMessage(Message message)
        {
            bool flag = false;
            try
            {
                flag = this.ProcessMessage(message);
            }
            catch
            {
            }
            return flag;
        }

        protected System.Workflow.ComponentModel.Design.HitTestInfo MessageHitTestContext
        {
            get
            {
                System.Workflow.ComponentModel.Design.HitTestInfo messageHitTestContext = this.ParentView.MessageHitTestContext;
                if (messageHitTestContext == null)
                {
                    messageHitTestContext = System.Workflow.ComponentModel.Design.HitTestInfo.Nowhere;
                }
                return messageHitTestContext;
            }
        }

        protected WorkflowView ParentView
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parentView;
            }
        }
    }
}

