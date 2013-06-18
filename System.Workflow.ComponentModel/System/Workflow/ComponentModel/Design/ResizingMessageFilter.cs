namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    internal sealed class ResizingMessageFilter : WorkflowDesignerMessageFilter
    {
        private DesignerEdges designerSizingEdge;
        private ActivityDesigner designerToResize;
        private DesignerTransaction designerTransaction;

        private bool CanResizeDesigner(ActivityDesigner designer)
        {
            if (!designer.EnableVisualResizing)
            {
                return false;
            }
            if (designer.ParentDesigner == null)
            {
                return true;
            }
            FreeformActivityDesigner parentDesigner = designer.ParentDesigner as FreeformActivityDesigner;
            return ((parentDesigner != null) && parentDesigner.CanResizeContainedDesigner(designer));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private ActivityDesigner GetDesignerToResize(Point point, out DesignerEdges sizingEdge)
        {
            ActivityDesigner designer = null;
            sizingEdge = DesignerEdges.None;
            ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
            if (service != null)
            {
                ArrayList list = new ArrayList(service.GetSelectedComponents());
                for (int i = 0; (i < list.Count) && (designer == null); i++)
                {
                    Activity activity = list[i] as Activity;
                    if (activity != null)
                    {
                        ActivityDesigner designer2 = ActivityDesigner.GetDesigner(activity);
                        if (designer2 != null)
                        {
                            SelectionGlyph glyph = designer2.Glyphs[typeof(SelectionGlyph)] as SelectionGlyph;
                            if (glyph != null)
                            {
                                foreach (Rectangle rectangle in glyph.GetGrabHandles(designer2))
                                {
                                    if (rectangle.Contains(point))
                                    {
                                        designer = designer2;
                                        sizingEdge = this.GetSizingEdge(designer, point);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return designer;
        }

        private DesignerEdges GetSizingEdge(ActivityDesigner designer, Point point)
        {
            DesignerEdges none = DesignerEdges.None;
            Size selectionSize = WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize;
            Rectangle bounds = designer.Bounds;
            Point[] line = new Point[] { new Point(bounds.Left, bounds.Top), new Point(bounds.Left, bounds.Bottom) };
            if (Math.Floor(DesignerGeometryHelper.DistanceFromPointToLineSegment(point, line)) <= (selectionSize.Width + 1))
            {
                none |= DesignerEdges.Left;
            }
            Point[] pointArray2 = new Point[] { new Point(bounds.Left, bounds.Top), new Point(bounds.Right, bounds.Top) };
            if (Math.Floor(DesignerGeometryHelper.DistanceFromPointToLineSegment(point, pointArray2)) <= (selectionSize.Height + 1))
            {
                none |= DesignerEdges.Top;
            }
            Point[] pointArray3 = new Point[] { new Point(bounds.Right, bounds.Top), new Point(bounds.Right, bounds.Bottom) };
            if (Math.Floor(DesignerGeometryHelper.DistanceFromPointToLineSegment(point, pointArray3)) <= (selectionSize.Width + 1))
            {
                none |= DesignerEdges.Right;
            }
            Point[] pointArray4 = new Point[] { new Point(bounds.Left, bounds.Bottom), new Point(bounds.Right, bounds.Bottom) };
            if (Math.Floor(DesignerGeometryHelper.DistanceFromPointToLineSegment(point, pointArray4)) <= (selectionSize.Height + 1))
            {
                none |= DesignerEdges.Bottom;
            }
            return none;
        }

        protected override bool OnKeyDown(KeyEventArgs eventArgs)
        {
            if ((eventArgs.KeyValue == 0x1b) && (this.designerToResize != null))
            {
                this.SetResizableDesigner(null, DesignerEdges.None);
                return true;
            }
            return false;
        }

        protected override bool OnMouseCaptureChanged()
        {
            if (this.designerToResize != null)
            {
                this.SetResizableDesigner(null, DesignerEdges.None);
            }
            return false;
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            if (eventArgs.Button == MouseButtons.Left)
            {
                WorkflowView parentView = base.ParentView;
                if (parentView == null)
                {
                    throw new InvalidOperationException(DR.GetString("WorkflowViewNull", new object[0]));
                }
                Point clientPoint = new Point(eventArgs.X, eventArgs.Y);
                Point point = parentView.ClientPointToLogical(clientPoint);
                DesignerEdges none = DesignerEdges.None;
                ActivityDesigner designerToResize = this.GetDesignerToResize(point, out none);
                if (((designerToResize != null) && (none != DesignerEdges.None)) && this.CanResizeDesigner(designerToResize))
                {
                    this.SetResizableDesigner(designerToResize, none);
                }
            }
            return (this.designerToResize != null);
        }

        protected override bool OnMouseLeave()
        {
            if (this.designerToResize != null)
            {
                this.SetResizableDesigner(null, DesignerEdges.None);
            }
            else
            {
                this.UpdateCursor(DesignerEdges.None);
            }
            return false;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            WorkflowView parentView = base.ParentView;
            if (parentView == null)
            {
                throw new InvalidOperationException(DR.GetString("WorkflowViewNull", new object[0]));
            }
            bool flag = false;
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);
            Point point = parentView.ClientPointToLogical(clientPoint);
            DesignerEdges none = DesignerEdges.None;
            if (this.designerToResize != null)
            {
                none = this.designerSizingEdge;
                this.UpdateDesignerSize(point, this.designerToResize, this.designerSizingEdge);
                flag = true;
            }
            else if (eventArgs.Button == MouseButtons.None)
            {
                ActivityDesigner designerToResize = this.GetDesignerToResize(point, out none);
                if (((designerToResize != null) && (none != DesignerEdges.None)) && this.CanResizeDesigner(designerToResize))
                {
                    flag = true;
                }
            }
            this.UpdateCursor(none);
            return flag;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            if ((this.designerToResize != null) && (eventArgs.Button == MouseButtons.Left))
            {
                WorkflowView parentView = base.ParentView;
                if (parentView == null)
                {
                    throw new InvalidOperationException(DR.GetString("WorkflowViewNull", new object[0]));
                }
                this.UpdateDesignerSize(parentView.ClientPointToLogical(new Point(eventArgs.X, eventArgs.Y)), this.designerToResize, this.designerSizingEdge);
            }
            this.SetResizableDesigner(null, DesignerEdges.None);
            return false;
        }

        private void SetResizableDesigner(ActivityDesigner designer, DesignerEdges sizingEdge)
        {
            if ((this.designerToResize != designer) && ((designer == null) || this.CanResizeDesigner(designer)))
            {
                WorkflowView parentView = base.ParentView;
                if (parentView == null)
                {
                    throw new InvalidOperationException(DR.GetString("WorkflowViewNull", new object[0]));
                }
                if (designer != null)
                {
                    if (this.designerTransaction != null)
                    {
                        this.designerTransaction.Cancel();
                    }
                    IDesignerHost service = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (service != null)
                    {
                        this.designerTransaction = service.CreateTransaction(DR.GetString("ResizeUndoDescription", new object[] { designer.Text }));
                    }
                    ((IWorkflowDesignerMessageSink) designer).OnBeginResizing(sizingEdge);
                }
                else
                {
                    if (this.designerTransaction != null)
                    {
                        this.designerTransaction.Commit();
                        this.designerTransaction = null;
                    }
                    ((IWorkflowDesignerMessageSink) this.designerToResize).OnEndResizing();
                }
                this.designerToResize = designer;
                this.designerSizingEdge = sizingEdge;
                parentView.Capture = this.designerToResize != null;
                this.UpdateCursor(this.designerSizingEdge);
            }
        }

        private void UpdateCursor(DesignerEdges sizingEdge)
        {
            WorkflowView parentView = base.ParentView;
            if (parentView == null)
            {
                throw new InvalidOperationException(DR.GetString("WorkflowViewNull", new object[0]));
            }
            Cursor sizeNWSE = parentView.Cursor;
            if ((((sizingEdge & DesignerEdges.Left) > DesignerEdges.None) && ((sizingEdge & DesignerEdges.Top) > DesignerEdges.None)) || (((sizingEdge & DesignerEdges.Right) > DesignerEdges.None) && ((sizingEdge & DesignerEdges.Bottom) > DesignerEdges.None)))
            {
                sizeNWSE = Cursors.SizeNWSE;
            }
            else if ((((sizingEdge & DesignerEdges.Right) > DesignerEdges.None) && ((sizingEdge & DesignerEdges.Top) > DesignerEdges.None)) || (((sizingEdge & DesignerEdges.Left) > DesignerEdges.None) && ((sizingEdge & DesignerEdges.Bottom) > DesignerEdges.None)))
            {
                sizeNWSE = Cursors.SizeNESW;
            }
            else if (((sizingEdge & DesignerEdges.Top) > DesignerEdges.None) || ((sizingEdge & DesignerEdges.Bottom) > DesignerEdges.None))
            {
                sizeNWSE = Cursors.SizeNS;
            }
            else if (((sizingEdge & DesignerEdges.Left) > DesignerEdges.None) || ((sizingEdge & DesignerEdges.Right) > DesignerEdges.None))
            {
                sizeNWSE = Cursors.SizeWE;
            }
            else if ((sizingEdge == DesignerEdges.None) && (((parentView.Cursor == Cursors.SizeNWSE) || (parentView.Cursor == Cursors.SizeNESW)) || ((parentView.Cursor == Cursors.SizeNS) || (parentView.Cursor == Cursors.SizeWE))))
            {
                sizeNWSE = Cursors.Default;
            }
            if (parentView.Cursor != sizeNWSE)
            {
                parentView.Cursor = sizeNWSE;
            }
        }

        private void UpdateDesignerSize(Point point, ActivityDesigner designerToSize, DesignerEdges sizingEdge)
        {
            if (base.ParentView == null)
            {
                throw new InvalidOperationException(DR.GetString("WorkflowViewNull", new object[0]));
            }
            Rectangle empty = Rectangle.Empty;
            if (designerToSize.ParentDesigner != null)
            {
                empty = designerToSize.ParentDesigner.Bounds;
                Size selectionSize = WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize;
                empty.Inflate(-2 * selectionSize.Width, -2 * selectionSize.Height);
            }
            Rectangle bounds = designerToSize.Bounds;
            if ((sizingEdge & DesignerEdges.Left) > DesignerEdges.None)
            {
                int x = point.X;
                if (!empty.IsEmpty)
                {
                    x = Math.Max(x, empty.X);
                }
                x = DesignerHelpers.SnapToGrid(new Point(x, 0)).X;
                bounds.Width += bounds.Left - x;
                int num2 = (bounds.Width < designerToSize.MinimumSize.Width) ? (bounds.Width - designerToSize.MinimumSize.Width) : 0;
                bounds.X = x + num2;
            }
            if ((sizingEdge & DesignerEdges.Top) > DesignerEdges.None)
            {
                int y = point.Y;
                if (!empty.IsEmpty)
                {
                    y = Math.Max(y, empty.Y);
                }
                y = DesignerHelpers.SnapToGrid(new Point(0, y)).Y;
                bounds.Height += bounds.Top - y;
                int num4 = (bounds.Height < designerToSize.MinimumSize.Height) ? (bounds.Height - designerToSize.MinimumSize.Height) : 0;
                bounds.Y = y + num4;
            }
            if ((sizingEdge & DesignerEdges.Right) > DesignerEdges.None)
            {
                bounds.Width += point.X - bounds.Right;
            }
            if ((sizingEdge & DesignerEdges.Bottom) > DesignerEdges.None)
            {
                bounds.Height += point.Y - bounds.Bottom;
            }
            bounds.Width = Math.Max(bounds.Width, designerToSize.MinimumSize.Width);
            bounds.Height = Math.Max(bounds.Height, designerToSize.MinimumSize.Height);
            if (!empty.IsEmpty)
            {
                bounds = Rectangle.Intersect(bounds, empty);
            }
            ((IWorkflowDesignerMessageSink) designerToSize).OnResizing(sizingEdge, bounds);
        }
    }
}

