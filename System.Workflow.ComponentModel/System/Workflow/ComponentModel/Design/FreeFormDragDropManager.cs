namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    internal sealed class FreeFormDragDropManager : DragDropManager
    {
        private static Cursor DragCopyCursor = new Cursor(typeof(WorkflowView), "Resources.DragCopyCursor.cur");
        private List<Image> draggedDesignerImages;
        private static Cursor DragMoveCursor = new Cursor(typeof(WorkflowView), "Resources.DragMoveCursor.cur");
        private static Cursor MoveCursor = new Cursor(typeof(WorkflowView), "Resources.MoveCursor.cur");
        private Point movedDesignerImagePoint = Point.Empty;
        private Cursor previousCursor = Cursors.Default;

        protected override void CreateDragFeedbackImages(IList<Activity> draggedActivities)
        {
            base.CreateDragFeedbackImages(draggedActivities);
            List<Image> list = new List<Image>();
            using (Graphics graphics = base.ParentView.CreateGraphics())
            {
                foreach (Activity activity in draggedActivities)
                {
                    ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                    if (designer == null)
                    {
                        designer = ActivityDesigner.CreateDesigner(base.ParentView, activity);
                    }
                    list.Add(designer.GetPreviewImage(graphics));
                }
            }
            base.ParentView.InvalidateClientRectangle(Rectangle.Empty);
            this.draggedDesignerImages = list;
        }

        protected override void DestroyDragFeedbackImages()
        {
            base.DestroyDragFeedbackImages();
            if (this.draggedDesignerImages != null)
            {
                foreach (Bitmap bitmap in this.draggedDesignerImages)
                {
                    bitmap.Dispose();
                }
                this.draggedDesignerImages = null;
                base.ParentView.InvalidateClientRectangle(Rectangle.Empty);
            }
        }

        internal static Point[] GetDesignerLocations(Point startPoint, Point endPoint, ICollection<Activity> activitiesToMove)
        {
            List<Point> list = new List<Point>();
            foreach (Activity activity in activitiesToMove)
            {
                Point location = endPoint;
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if ((designer != null) && !startPoint.IsEmpty)
                {
                    Size size = new Size(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
                    location = new Point(designer.Location.X + size.Width, designer.Location.Y + size.Height);
                }
                location = DesignerHelpers.SnapToGrid(location);
                list.Add(location);
            }
            return list.ToArray();
        }

        private void InvalidateDraggedImages(Point[] locations)
        {
            if ((this.draggedDesignerImages != null) && (locations.Length == this.draggedDesignerImages.Count))
            {
                AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                WorkflowView parentView = base.ParentView;
                for (int i = 0; i < this.draggedDesignerImages.Count; i++)
                {
                    Rectangle logicalRectangle = new Rectangle(locations[i], this.draggedDesignerImages[i].Size);
                    logicalRectangle.Inflate(2 * ambientTheme.Margin.Width, 2 * ambientTheme.Margin.Height);
                    parentView.InvalidateLogicalRectangle(logicalRectangle);
                }
            }
        }

        protected override bool OnDragEnter(DragEventArgs eventArgs)
        {
            bool flag = base.OnDragEnter(eventArgs);
            if (this.draggedDesignerImages == null)
            {
                WorkflowView parentView = base.ParentView;
                Point clientPoint = parentView.PointToClient(new Point(eventArgs.X, eventArgs.Y));
                Point point2 = parentView.ScreenPointToLogical(new Point(eventArgs.X, eventArgs.Y));
                if (parentView.IsClientPointInActiveLayout(clientPoint))
                {
                    this.movedDesignerImagePoint = point2;
                    return flag;
                }
                this.movedDesignerImagePoint = base.DragInitiationPoint;
            }
            return flag;
        }

        protected override bool OnDragOver(DragEventArgs eventArgs)
        {
            if (this.draggedDesignerImages != null)
            {
                Point[] locations = GetDesignerLocations(base.DragInitiationPoint, this.movedDesignerImagePoint, base.DraggedActivities);
                this.InvalidateDraggedImages(locations);
            }
            bool flag = base.OnDragOver(eventArgs);
            if (this.draggedDesignerImages != null)
            {
                WorkflowView parentView = base.ParentView;
                Point clientPoint = parentView.PointToClient(new Point(eventArgs.X, eventArgs.Y));
                Point point2 = parentView.ScreenPointToLogical(new Point(eventArgs.X, eventArgs.Y));
                if (parentView.IsClientPointInActiveLayout(clientPoint))
                {
                    this.movedDesignerImagePoint = point2;
                }
                else
                {
                    this.movedDesignerImagePoint = base.DragInitiationPoint;
                }
                Point[] pointArray2 = GetDesignerLocations(base.DragInitiationPoint, this.movedDesignerImagePoint, base.DraggedActivities);
                this.InvalidateDraggedImages(pointArray2);
            }
            return flag;
        }

        protected override bool OnGiveFeedback(GiveFeedbackEventArgs gfbevent)
        {
            base.OnGiveFeedback(gfbevent);
            if (this.draggedDesignerImages == null)
            {
                return false;
            }
            gfbevent.UseDefaultCursors = false;
            if ((gfbevent.Effect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                Cursor.Current = DragMoveCursor;
            }
            else if ((gfbevent.Effect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Cursor.Current = DragCopyCursor;
            }
            else
            {
                Cursor.Current = Cursors.No;
            }
            return true;
        }

        protected override bool OnMouseLeave()
        {
            this.UpdateCursor(false);
            return false;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            bool flag = base.OnMouseMove(eventArgs);
            if (eventArgs.Button == MouseButtons.None)
            {
                bool showMoveCursor = false;
                showMoveCursor |= (((base.MessageHitTestContext != null) && (base.MessageHitTestContext.AssociatedDesigner != null)) && (ActivityDesigner.GetParentDesigner(base.MessageHitTestContext.AssociatedDesigner.Activity) is FreeformActivityDesigner)) && ((base.MessageHitTestContext.HitLocation & HitTestLocations.ActionArea) == HitTestLocations.None);
                this.UpdateCursor(showMoveCursor);
            }
            return flag;
        }

        protected override bool OnPaint(PaintEventArgs eventArgs, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            bool flag = false;
            if ((this.draggedDesignerImages != null) && (base.DropTargetDesigner is FreeformActivityDesigner))
            {
                using (Region region = new Region(ActivityDesignerPaint.GetDesignerPath(base.ParentView.RootDesigner, false)))
                {
                    Region clip = eventArgs.Graphics.Clip;
                    eventArgs.Graphics.Clip = region;
                    Point[] pointArray = GetDesignerLocations(base.DragInitiationPoint, this.movedDesignerImagePoint, base.DraggedActivities);
                    for (int i = 0; i < this.draggedDesignerImages.Count; i++)
                    {
                        Size size = this.draggedDesignerImages[i].Size;
                        ActivityDesignerPaint.DrawImage(eventArgs.Graphics, this.draggedDesignerImages[i], new Rectangle(new Point(pointArray[i].X - (2 * ambientTheme.Margin.Width), pointArray[i].Y - (2 * ambientTheme.Margin.Height)), size), new Rectangle(Point.Empty, size), DesignerContentAlignment.Fill, 0.4f, false);
                    }
                    eventArgs.Graphics.Clip = clip;
                    return flag;
                }
            }
            return base.OnPaint(eventArgs, viewPort, ambientTheme);
        }

        protected override bool OnPaintWorkflowAdornments(PaintEventArgs eventArgs, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            bool flag = false;
            if (((this.draggedDesignerImages != null) && (this.draggedDesignerImages.Count != 0)) && (base.DropTargetDesigner is FreeformActivityDesigner))
            {
                return flag;
            }
            return base.OnPaintWorkflowAdornments(eventArgs, viewPort, ambientTheme);
        }

        protected override bool OnScroll(ScrollBar sender, int value)
        {
            if (this.draggedDesignerImages != null)
            {
                Point[] locations = GetDesignerLocations(base.DragInitiationPoint, this.movedDesignerImagePoint, base.DraggedActivities);
                this.InvalidateDraggedImages(locations);
            }
            bool flag = base.OnScroll(sender, value);
            if (this.draggedDesignerImages != null)
            {
                WorkflowView parentView = base.ParentView;
                Point clientPoint = parentView.PointToClient(Control.MousePosition);
                Point point2 = parentView.ScreenPointToLogical(Control.MousePosition);
                if (parentView.IsClientPointInActiveLayout(clientPoint))
                {
                    this.movedDesignerImagePoint = point2;
                }
                else
                {
                    this.movedDesignerImagePoint = base.DragInitiationPoint;
                }
                Point[] pointArray2 = GetDesignerLocations(base.DragInitiationPoint, this.movedDesignerImagePoint, base.DraggedActivities);
                this.InvalidateDraggedImages(pointArray2);
            }
            return flag;
        }

        private void UpdateCursor(bool showMoveCursor)
        {
            if (showMoveCursor)
            {
                if ((base.ParentView.Cursor != MoveCursor) && (base.ParentView.Cursor == Cursors.Default))
                {
                    this.previousCursor = base.ParentView.Cursor;
                    base.ParentView.Cursor = MoveCursor;
                }
            }
            else
            {
                base.ParentView.Cursor = this.previousCursor;
            }
        }
    }
}

