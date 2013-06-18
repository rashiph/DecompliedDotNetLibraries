namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    internal class DragDropManager : WorkflowDesignerMessageFilter
    {
        private const string CF_DESIGNERSTATE = "CF_WINOEDESIGNERCOMPONENTSSTATE";
        private List<Activity> draggedActivities = new List<Activity>();
        private ActivityDesigner draggedDesigner;
        private Image dragImage;
        private Point dragImagePointInClientCoOrd = Point.Empty;
        private bool dragImageSnapped;
        private Point dragInitiationPoint = Point.Empty;
        private bool dragStarted;
        private ActivityDesigner dropTargetDesigner;
        private bool exceptionInDragDrop;
        private List<Activity> existingDraggedActivities = new List<Activity>();
        private bool wasCtrlKeyPressed;

        internal DragDropManager()
        {
        }

        private bool CanInitiateDragDrop()
        {
            ISelectionService service = (ISelectionService) base.GetService(typeof(ISelectionService));
            IDesignerHost host = (IDesignerHost) base.GetService(typeof(IDesignerHost));
            if ((service == null) || (host == null))
            {
                return false;
            }
            ICollection selectedComponents = service.GetSelectedComponents();
            return (((selectedComponents != null) && (selectedComponents.Count >= 1)) && (!service.GetComponentSelected(host.RootComponent) && Helpers.AreAllActivities(selectedComponents)));
        }

        protected virtual void CreateDragFeedbackImages(IList<Activity> draggedActivities)
        {
            Bitmap image = null;
            if (draggedActivities.Count > 0)
            {
                Bitmap bitmap2 = null;
                string s = string.Empty;
                if (draggedActivities.Count > 1)
                {
                    bitmap2 = DR.GetImage("Activities") as Bitmap;
                    s = DR.GetString("ActivitiesDesc", new object[0]);
                }
                else
                {
                    ToolboxBitmapAttribute attribute = (ToolboxBitmapAttribute) TypeDescriptor.GetAttributes(draggedActivities[0].GetType())[typeof(ToolboxBitmapAttribute)];
                    bitmap2 = attribute.GetImage(draggedActivities[0].GetType()) as Bitmap;
                    s = draggedActivities[0].GetType().Name;
                }
                if ((bitmap2 != null) && (s.Length > 0))
                {
                    WorkflowView parentView = base.ParentView;
                    Rectangle rectangle = (bitmap2 != null) ? new Rectangle(Point.Empty, bitmap2.Size) : Rectangle.Empty;
                    Rectangle layoutRectangle = (s.Length > 0) ? new Rectangle(Point.Empty, new Size(AmbientTheme.DragImageTextSize.Width, parentView.Font.Height + 2)) : Rectangle.Empty;
                    if (!rectangle.IsEmpty)
                    {
                        layoutRectangle.Offset(rectangle.Width + AmbientTheme.DragImageMargins.Width, 0);
                    }
                    Size size = parentView.LogicalSizeToClient(new Size(rectangle.Width + layoutRectangle.Width, Math.Max(rectangle.Height, layoutRectangle.Height)));
                    image = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
                    using (Graphics graphics = Graphics.FromImage(image))
                    {
                        using (Brush brush = new SolidBrush(Color.FromArgb(0, 0xff, 0, 0xff)))
                        {
                            graphics.ScaleTransform(this.ScaleZoomFactor, this.ScaleZoomFactor);
                            graphics.FillRectangle(brush, new Rectangle(0, 0, image.Width, image.Height));
                            if (bitmap2 != null)
                            {
                                graphics.DrawImage(bitmap2, new Rectangle(Point.Empty, bitmap2.Size));
                            }
                            if (s.Length > 0)
                            {
                                StringFormat format = new StringFormat {
                                    Alignment = StringAlignment.Near,
                                    Trimming = StringTrimming.EllipsisCharacter,
                                    LineAlignment = StringAlignment.Center
                                };
                                graphics.DrawString(s, parentView.Font, SystemBrushes.WindowText, layoutRectangle, format);
                            }
                        }
                    }
                }
            }
            this.dragImage = image;
        }

        protected virtual void DestroyDragFeedbackImages()
        {
            if (this.dragImage != null)
            {
                this.dragImage.Dispose();
                this.dragImage = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    IServiceContainer service = base.GetService(typeof(IServiceContainer)) as IServiceContainer;
                    if (service != null)
                    {
                        service.RemoveService(typeof(DragDropManager));
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override void Initialize(WorkflowView parentView)
        {
            base.Initialize(parentView);
            IServiceContainer service = base.GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (service != null)
            {
                service.RemoveService(typeof(DragDropManager));
                service.AddService(typeof(DragDropManager), this);
            }
        }

        private void InitiateDragDrop()
        {
            WorkflowView parentView = base.ParentView;
            ISelectionService service = (ISelectionService) base.GetService(typeof(ISelectionService));
            IDesignerHost host = (IDesignerHost) base.GetService(typeof(IDesignerHost));
            if ((service != null) && (host != null))
            {
                ICollection selectedComponents = service.GetSelectedComponents();
                if (((selectedComponents != null) && (selectedComponents.Count >= 1)) && (!service.GetComponentSelected(host.RootComponent) && Helpers.AreAllActivities(selectedComponents)))
                {
                    DragDropEffects none = DragDropEffects.None;
                    try
                    {
                        this.existingDraggedActivities.AddRange(Helpers.GetTopLevelActivities(selectedComponents));
                        DragDropEffects allowedEffects = DesignerHelpers.AreAssociatedDesignersMovable(this.existingDraggedActivities) ? (DragDropEffects.Move | DragDropEffects.Copy) : DragDropEffects.Copy;
                        IDataObject data = CompositeActivityDesigner.SerializeActivitiesToDataObject(base.ParentView, this.existingDraggedActivities.ToArray());
                        none = parentView.DoDragDrop(data, allowedEffects);
                    }
                    catch (Exception exception)
                    {
                        DesignerHelpers.ShowError(base.ParentView, exception.Message);
                    }
                    finally
                    {
                        if ((none == DragDropEffects.Move) && (this.existingDraggedActivities.Count > 0))
                        {
                            string transactionDescription = string.Empty;
                            if (this.existingDraggedActivities.Count > 1)
                            {
                                transactionDescription = SR.GetString("MoveMultipleActivities", new object[] { this.existingDraggedActivities.Count });
                            }
                            else
                            {
                                transactionDescription = SR.GetString("MoveSingleActivity", new object[] { this.existingDraggedActivities[0].GetType() });
                            }
                            CompositeActivityDesigner.RemoveActivities(base.ParentView, this.existingDraggedActivities.AsReadOnly(), transactionDescription);
                        }
                        this.existingDraggedActivities.Clear();
                    }
                }
            }
        }

        private bool IsRecursiveDropOperation(ActivityDesigner dropTargetDesigner)
        {
            if (dropTargetDesigner != null)
            {
                ISelectionService service = (ISelectionService) base.GetService(typeof(ISelectionService));
                CompositeActivity activity = dropTargetDesigner.Activity as CompositeActivity;
                if ((activity == null) || (service == null))
                {
                    return false;
                }
                base.GetService(typeof(WorkflowView));
                base.GetService(typeof(IDesignerHost));
                base.GetService(typeof(WorkflowDesignerLoader));
                if ((this.draggedActivities.Count == 0) || (this.existingDraggedActivities.Count == 0))
                {
                    return false;
                }
                ArrayList list = new ArrayList(Helpers.GetTopLevelActivities(service.GetSelectedComponents()));
                for (CompositeActivity activity2 = activity; activity2 != null; activity2 = activity2.Parent)
                {
                    if (list.Contains(activity2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool IsValidDropContext(System.Workflow.ComponentModel.Design.HitTestInfo dropLocation)
        {
            if (this.draggedActivities.Count == 0)
            {
                return false;
            }
            if ((dropLocation == null) || (dropLocation.AssociatedDesigner == null))
            {
                return false;
            }
            CompositeActivityDesigner associatedDesigner = dropLocation.AssociatedDesigner as CompositeActivityDesigner;
            if (associatedDesigner == null)
            {
                return false;
            }
            if (!associatedDesigner.IsEditable || !associatedDesigner.CanInsertActivities(dropLocation, new List<Activity>(this.draggedActivities).AsReadOnly()))
            {
                return false;
            }
            if (!this.wasCtrlKeyPressed && (this.existingDraggedActivities.Count > 0))
            {
                if (!DesignerHelpers.AreAssociatedDesignersMovable(this.draggedActivities))
                {
                    return false;
                }
                if (this.IsRecursiveDropOperation(dropLocation.AssociatedDesigner))
                {
                    return false;
                }
                foreach (DictionaryEntry entry in Helpers.PairUpCommonParentActivities(this.draggedActivities))
                {
                    CompositeActivityDesigner designer = ActivityDesigner.GetDesigner(entry.Key as Activity) as CompositeActivityDesigner;
                    Activity[] collection = (Activity[]) ((ArrayList) entry.Value).ToArray(typeof(Activity));
                    if ((designer != null) && !designer.CanMoveActivities(dropLocation, new List<Activity>(collection).AsReadOnly()))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        protected override bool OnDragDrop(DragEventArgs eventArgs)
        {
            WorkflowView parentView = base.ParentView;
            parentView.InvalidateClientRectangle(Rectangle.Empty);
            eventArgs.Effect = DragDropEffects.None;
            this.DestroyDragFeedbackImages();
            Point clientPoint = parentView.PointToClient(new Point(eventArgs.X, eventArgs.Y));
            Point point = parentView.ScreenPointToLogical(new Point(eventArgs.X, eventArgs.Y));
            if (!parentView.IsClientPointInActiveLayout(clientPoint))
            {
                if (this.dropTargetDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink) this.dropTargetDesigner).OnDragLeave();
                }
                this.wasCtrlKeyPressed = false;
                this.dropTargetDesigner = null;
                this.draggedActivities.Clear();
                return false;
            }
            this.wasCtrlKeyPressed = (eventArgs.KeyState & 8) == 8;
            ActivityDragEventArgs e = new ActivityDragEventArgs(eventArgs, this.dragInitiationPoint, point, this.draggedActivities);
            System.Workflow.ComponentModel.Design.HitTestInfo messageHitTestContext = base.MessageHitTestContext;
            if (this.dropTargetDesigner != messageHitTestContext.AssociatedDesigner)
            {
                if (this.dropTargetDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink) this.dropTargetDesigner).OnDragLeave();
                    this.dropTargetDesigner = null;
                }
                if (messageHitTestContext.AssociatedDesigner != null)
                {
                    this.dropTargetDesigner = messageHitTestContext.AssociatedDesigner;
                    if (this.dropTargetDesigner != null)
                    {
                        ((IWorkflowDesignerMessageSink) this.dropTargetDesigner).OnDragEnter(e);
                    }
                }
            }
            try
            {
                if (this.dropTargetDesigner != null)
                {
                    if ((!this.wasCtrlKeyPressed && this.IsRecursiveDropOperation(this.dropTargetDesigner)) || ((this.dropTargetDesigner is CompositeActivityDesigner) && !((CompositeActivityDesigner) this.dropTargetDesigner).IsEditable))
                    {
                        ((IWorkflowDesignerMessageSink) this.dropTargetDesigner).OnDragLeave();
                        e.Effect = DragDropEffects.None;
                    }
                    else
                    {
                        List<Activity> draggedActivities = new List<Activity>();
                        string description = SR.GetString("DragDropActivities");
                        if (!this.wasCtrlKeyPressed && (this.existingDraggedActivities.Count > 0))
                        {
                            draggedActivities.AddRange(this.existingDraggedActivities);
                            if (draggedActivities.Count > 1)
                            {
                                description = SR.GetString("MoveMultipleActivities", new object[] { draggedActivities.Count });
                            }
                            else if (draggedActivities.Count == 1)
                            {
                                description = SR.GetString("MoveSingleActivity", new object[] { draggedActivities[0].GetType() });
                            }
                        }
                        else
                        {
                            draggedActivities.AddRange(CompositeActivityDesigner.DeserializeActivitiesFromDataObject(base.ParentView, eventArgs.Data, true));
                            if (draggedActivities.Count > 0)
                            {
                                description = SR.GetString("CreateActivityFromToolbox", new object[] { draggedActivities[0].GetType() });
                            }
                        }
                        IDesignerHost host = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        DesignerTransaction transaction = null;
                        if (draggedActivities.Count > 0)
                        {
                            transaction = host.CreateTransaction(description);
                        }
                        e = new ActivityDragEventArgs(eventArgs, this.dragInitiationPoint, point, draggedActivities);
                        try
                        {
                            ((IWorkflowDesignerMessageSink) this.dropTargetDesigner).OnDragDrop(e);
                            if (e.Effect == DragDropEffects.Move)
                            {
                                this.existingDraggedActivities.Clear();
                            }
                            if (transaction != null)
                            {
                                transaction.Commit();
                            }
                        }
                        catch (Exception exception)
                        {
                            if (transaction != null)
                            {
                                transaction.Cancel();
                            }
                            throw exception;
                        }
                        if (draggedActivities.Count > 0)
                        {
                            Stream data = eventArgs.Data.GetData("CF_WINOEDESIGNERCOMPONENTSSTATE") as Stream;
                            if (data != null)
                            {
                                Helpers.DeserializeDesignersFromStream(draggedActivities, data);
                            }
                            ISelectionService service = (ISelectionService) base.GetService(typeof(ISelectionService));
                            if (service != null)
                            {
                                service.SetSelectedComponents(draggedActivities, SelectionTypes.Replace);
                            }
                        }
                        if (host != null)
                        {
                            host.Activate();
                        }
                    }
                }
            }
            catch (Exception exception2)
            {
                ((IWorkflowDesignerMessageSink) this.dropTargetDesigner).OnDragLeave();
                e.Effect = DragDropEffects.None;
                string message = exception2.Message;
                if ((exception2.InnerException != null) && !string.IsNullOrEmpty(exception2.InnerException.Message))
                {
                    message = exception2.InnerException.Message;
                }
                string str3 = DR.GetString("Error_FailedToDeserializeComponents", new object[0]) + "\r\n" + DR.GetString("Error_Reason", new object[] { message });
                DesignerHelpers.ShowError(base.ParentView, str3);
                if (exception2 != CheckoutException.Canceled)
                {
                    throw new Exception(str3, exception2);
                }
            }
            finally
            {
                this.wasCtrlKeyPressed = false;
                this.draggedActivities.Clear();
                this.dropTargetDesigner = null;
                this.exceptionInDragDrop = false;
                eventArgs.Effect = e.Effect;
            }
            return true;
        }

        protected override bool OnDragEnter(DragEventArgs eventArgs)
        {
            WorkflowView parentView = base.ParentView;
            parentView.InvalidateClientRectangle(Rectangle.Empty);
            eventArgs.Effect = DragDropEffects.None;
            this.wasCtrlKeyPressed = false;
            if (this.existingDraggedActivities.Count > 0)
            {
                this.draggedActivities.AddRange(this.existingDraggedActivities);
            }
            else
            {
                try
                {
                    Activity[] collection = CompositeActivityDesigner.DeserializeActivitiesFromDataObject(base.ParentView, eventArgs.Data);
                    if (collection != null)
                    {
                        this.draggedActivities.AddRange(collection);
                    }
                }
                catch
                {
                    this.exceptionInDragDrop = true;
                }
            }
            Point clientPoint = parentView.PointToClient(new Point(eventArgs.X, eventArgs.Y));
            Point point = parentView.ScreenPointToLogical(new Point(eventArgs.X, eventArgs.Y));
            this.CreateDragFeedbackImages(this.draggedActivities);
            if (this.dragImage != null)
            {
                this.dragImagePointInClientCoOrd = new Point(clientPoint.X + (SystemInformation.CursorSize.Width / 4), clientPoint.Y + (SystemInformation.CursorSize.Height / 4));
            }
            if (!parentView.IsClientPointInActiveLayout(clientPoint))
            {
                return false;
            }
            this.wasCtrlKeyPressed = (eventArgs.KeyState & 8) == 8;
            ActivityDragEventArgs e = new ActivityDragEventArgs(eventArgs, this.dragInitiationPoint, point, this.draggedActivities);
            ActivityDesigner associatedDesigner = base.MessageHitTestContext.AssociatedDesigner;
            if (associatedDesigner == null)
            {
                return false;
            }
            if (!this.wasCtrlKeyPressed && this.IsRecursiveDropOperation(associatedDesigner))
            {
                return false;
            }
            CompositeActivityDesigner designer2 = associatedDesigner as CompositeActivityDesigner;
            if ((designer2 != null) && !designer2.IsEditable)
            {
                return false;
            }
            this.dropTargetDesigner = associatedDesigner;
            ((IWorkflowDesignerMessageSink) this.dropTargetDesigner).OnDragEnter(e);
            if (!e.DragImageSnapPoint.IsEmpty)
            {
                Point point3 = parentView.LogicalPointToClient(e.DragImageSnapPoint);
                Size size = parentView.LogicalSizeToClient(AmbientTheme.DragImageIconSize);
                this.dragImagePointInClientCoOrd = new Point(point3.X - (size.Width / 2), point3.Y - (size.Height / 2));
                this.dragImageSnapped = true;
            }
            eventArgs.Effect = e.Effect;
            if ((eventArgs.Effect == DragDropEffects.None) && this.exceptionInDragDrop)
            {
                eventArgs.Effect = this.wasCtrlKeyPressed ? DragDropEffects.Copy : DragDropEffects.Move;
            }
            return true;
        }

        protected override bool OnDragLeave()
        {
            base.ParentView.InvalidateClientRectangle(Rectangle.Empty);
            this.DestroyDragFeedbackImages();
            this.wasCtrlKeyPressed = false;
            if (this.dropTargetDesigner != null)
            {
                ((IWorkflowDesignerMessageSink) this.dropTargetDesigner).OnDragLeave();
            }
            this.dropTargetDesigner = null;
            this.draggedActivities.Clear();
            this.exceptionInDragDrop = false;
            return true;
        }

        protected override bool OnDragOver(DragEventArgs eventArgs)
        {
            eventArgs.Effect = DragDropEffects.None;
            this.wasCtrlKeyPressed = false;
            this.dragImageSnapped = false;
            WorkflowView parentView = base.ParentView;
            Point clientPoint = parentView.PointToClient(new Point(eventArgs.X, eventArgs.Y));
            Point point = parentView.ScreenPointToLogical(new Point(eventArgs.X, eventArgs.Y));
            Point dragImagePointInClientCoOrd = this.dragImagePointInClientCoOrd;
            this.dragImagePointInClientCoOrd = new Point(clientPoint.X + (SystemInformation.CursorSize.Width / 4), clientPoint.Y + (SystemInformation.CursorSize.Height / 4));
            if (!parentView.IsClientPointInActiveLayout(clientPoint))
            {
                if (this.dropTargetDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink) this.dropTargetDesigner).OnDragLeave();
                }
                this.dropTargetDesigner = null;
            }
            else
            {
                this.wasCtrlKeyPressed = (eventArgs.KeyState & 8) == 8;
                ActivityDragEventArgs e = new ActivityDragEventArgs(eventArgs, this.dragInitiationPoint, point, this.draggedActivities);
                ActivityDesigner associatedDesigner = base.MessageHitTestContext.AssociatedDesigner;
                if (associatedDesigner != null)
                {
                    CompositeActivityDesigner designer2 = associatedDesigner as CompositeActivityDesigner;
                    if ((!this.wasCtrlKeyPressed && this.IsRecursiveDropOperation(associatedDesigner)) || ((designer2 != null) && !designer2.IsEditable))
                    {
                        e.Effect = DragDropEffects.None;
                        associatedDesigner = null;
                    }
                }
                if (this.dropTargetDesigner != associatedDesigner)
                {
                    if (this.dropTargetDesigner != null)
                    {
                        ((IWorkflowDesignerMessageSink) this.dropTargetDesigner).OnDragLeave();
                    }
                    this.dropTargetDesigner = associatedDesigner;
                    if (this.dropTargetDesigner != null)
                    {
                        ((IWorkflowDesignerMessageSink) this.dropTargetDesigner).OnDragEnter(e);
                    }
                }
                else
                {
                    if (this.dropTargetDesigner != null)
                    {
                        ((IWorkflowDesignerMessageSink) this.dropTargetDesigner).OnDragOver(e);
                    }
                    if ((e.Effect != DragDropEffects.None) && !e.DragImageSnapPoint.IsEmpty)
                    {
                        Point point4 = parentView.LogicalPointToClient(e.DragImageSnapPoint);
                        Size size = parentView.LogicalSizeToClient(AmbientTheme.DragImageIconSize);
                        this.dragImagePointInClientCoOrd = new Point(point4.X - (size.Width / 2), point4.Y - (size.Height / 2));
                        this.dragImageSnapped = true;
                    }
                }
                eventArgs.Effect = e.Effect;
            }
            if (this.dragImage != null)
            {
                parentView.InvalidateClientRectangle(new Rectangle(dragImagePointInClientCoOrd, this.dragImage.Size));
                parentView.InvalidateClientRectangle(new Rectangle(this.dragImagePointInClientCoOrd, this.dragImage.Size));
            }
            if ((eventArgs.Effect == DragDropEffects.None) && this.exceptionInDragDrop)
            {
                eventArgs.Effect = this.wasCtrlKeyPressed ? DragDropEffects.Copy : DragDropEffects.Move;
            }
            return true;
        }

        protected override bool OnGiveFeedback(GiveFeedbackEventArgs gfbevent)
        {
            if (this.dropTargetDesigner != null)
            {
                ((IWorkflowDesignerMessageSink) this.dropTargetDesigner).OnGiveFeedback(gfbevent);
            }
            return true;
        }

        protected override bool OnKeyDown(KeyEventArgs eventArgs)
        {
            if (this.draggedDesigner == null)
            {
                return false;
            }
            if (eventArgs.KeyValue == 0x1b)
            {
                ((IWorkflowDesignerMessageSink) this.draggedDesigner).OnMouseDragEnd();
                this.draggedDesigner = null;
                this.dragStarted = false;
                base.ParentView.Capture = false;
            }
            else
            {
                ((IWorkflowDesignerMessageSink) this.draggedDesigner).OnKeyDown(eventArgs);
                eventArgs.Handled = true;
            }
            return true;
        }

        protected override bool OnKeyUp(KeyEventArgs eventArgs)
        {
            if (this.draggedDesigner != null)
            {
                ((IWorkflowDesignerMessageSink) this.draggedDesigner).OnKeyUp(eventArgs);
                eventArgs.Handled = true;
                return true;
            }
            return false;
        }

        protected override bool OnMouseCaptureChanged()
        {
            if (!base.ParentView.Capture)
            {
                if (this.draggedDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink) this.draggedDesigner).OnMouseDragEnd();
                }
                this.draggedDesigner = null;
                this.dragStarted = false;
            }
            return false;
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            WorkflowView parentView = base.ParentView;
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);
            if (parentView.IsClientPointInActiveLayout(clientPoint) && (eventArgs.Button == MouseButtons.Left))
            {
                this.dragInitiationPoint = parentView.ClientPointToLogical(clientPoint);
                this.dragStarted = true;
            }
            return false;
        }

        protected override bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            return (this.draggedDesigner != null);
        }

        protected override bool OnMouseHover(MouseEventArgs eventArgs)
        {
            return (this.draggedDesigner != null);
        }

        protected override bool OnMouseLeave()
        {
            return (this.draggedDesigner != null);
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            WorkflowView parentView = base.ParentView;
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);
            if (!parentView.IsClientPointInActiveLayout(clientPoint))
            {
                return false;
            }
            if (eventArgs.Button == MouseButtons.Left)
            {
                Point point2 = parentView.ClientPointToLogical(clientPoint);
                System.Workflow.ComponentModel.Design.HitTestInfo messageHitTestContext = base.MessageHitTestContext;
                if (this.draggedDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink) this.draggedDesigner).OnMouseDragMove(eventArgs);
                }
                else if ((((parentView.RootDesigner != null) && this.dragStarted) && ((eventArgs.Button & MouseButtons.Left) > MouseButtons.None)) && ((Math.Abs((int) (this.dragInitiationPoint.X - point2.X)) > SystemInformation.DragSize.Width) || (Math.Abs((int) (this.dragInitiationPoint.Y - point2.Y)) > SystemInformation.DragSize.Height)))
                {
                    ActivityDesigner associatedDesigner = messageHitTestContext.AssociatedDesigner;
                    if (associatedDesigner != null)
                    {
                        if (this.CanInitiateDragDrop())
                        {
                            this.InitiateDragDrop();
                            this.dragStarted = false;
                        }
                        else
                        {
                            this.draggedDesigner = associatedDesigner;
                            ((IWorkflowDesignerMessageSink) this.draggedDesigner).OnMouseDragBegin(this.dragInitiationPoint, eventArgs);
                            parentView.Capture = true;
                        }
                    }
                }
            }
            else
            {
                if (this.draggedDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink) this.draggedDesigner).OnMouseDragEnd();
                }
                this.draggedDesigner = null;
            }
            return (this.draggedDesigner != null);
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            if (this.draggedDesigner != null)
            {
                ((IWorkflowDesignerMessageSink) this.draggedDesigner).OnMouseDragEnd();
                this.draggedDesigner = null;
                this.dragStarted = false;
                base.ParentView.Capture = false;
                return true;
            }
            return false;
        }

        protected override bool OnPaintWorkflowAdornments(PaintEventArgs e, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            if (this.dragImage != null)
            {
                ActivityDesignerPaint.DrawImage(e.Graphics, this.dragImage, new Rectangle(this.dragImagePointInClientCoOrd, this.dragImage.Size), new Rectangle(0, 0, this.dragImage.Width, this.dragImage.Height), DesignerContentAlignment.Center, this.dragImageSnapped ? 1f : 0.5f, WorkflowTheme.CurrentTheme.AmbientTheme.DrawGrayscale);
            }
            return false;
        }

        protected override bool OnQueryContinueDrag(QueryContinueDragEventArgs qcdevent)
        {
            if (this.dropTargetDesigner != null)
            {
                ((IWorkflowDesignerMessageSink) this.dropTargetDesigner).OnQueryContinueDrag(qcdevent);
            }
            return true;
        }

        public IList<Activity> DraggedActivities
        {
            get
            {
                return this.draggedActivities.AsReadOnly();
            }
        }

        public ActivityDesigner DraggedDesigner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.draggedDesigner;
            }
        }

        public Point DragInitiationPoint
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dragInitiationPoint;
            }
        }

        public ActivityDesigner DropTargetDesigner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dropTargetDesigner;
            }
        }

        private float ScaleZoomFactor
        {
            get
            {
                WorkflowView parentView = base.ParentView;
                return ((((float) parentView.Zoom) / 100f) * parentView.ActiveLayout.Scaling);
            }
        }
    }
}

