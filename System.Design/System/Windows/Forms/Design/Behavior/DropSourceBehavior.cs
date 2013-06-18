namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal sealed class DropSourceBehavior : System.Windows.Forms.Design.Behavior.Behavior, IComparer
    {
        private DragDropEffects allowedEffects;
        private BehaviorService behaviorServiceSource;
        private BehaviorService behaviorServiceTarget;
        private bool cleanedUpDrag;
        private Rectangle clearDragImageRect;
        private bool currentShowState = true;
        private BehaviorDataObject data;
        private IDesignerHost destHost;
        private DragAssistanceManager dragAssistanceManager;
        private DragComponent[] dragComponents;
        private Image dragImage;
        private Rectangle dragImageRect;
        private Region dragImageRegion;
        private ArrayList dragObjects;
        private Graphics graphicsTarget;
        private Point initialMouseLoc;
        private IComponent lastDropTarget;
        private DragDropEffects lastEffect;
        private Point lastFeedbackLocation;
        private Point lastSnapOffset;
        private Point originalDragImageLocation;
        private Size parentGridSize;
        private Point parentLocation;
        private int primaryComponentIndex = -1;
        private IServiceProvider serviceProviderSource;
        private IServiceProvider serviceProviderTarget;
        private bool shareParent = true;
        private IDesignerHost srcHost;
        private StatusCommandUI statusCommandUITarget;
        private Control suspendedParent;
        private bool targetAllowsSnapLines;

        internal DropSourceBehavior(ICollection dragComponents, Control source, Point initialMouseLocation)
        {
            this.serviceProviderSource = source.Site;
            if (this.serviceProviderSource != null)
            {
                this.behaviorServiceSource = (BehaviorService) this.serviceProviderSource.GetService(typeof(BehaviorService));
                if ((this.behaviorServiceSource != null) && ((dragComponents != null) && (dragComponents.Count > 0)))
                {
                    this.srcHost = (IDesignerHost) this.serviceProviderSource.GetService(typeof(IDesignerHost));
                    if (this.srcHost != null)
                    {
                        this.data = new BehaviorDataObject(dragComponents, source, this);
                        this.allowedEffects = DragDropEffects.Move | DragDropEffects.Copy;
                        this.dragComponents = new DragComponent[dragComponents.Count];
                        this.parentGridSize = Size.Empty;
                        this.lastEffect = DragDropEffects.None;
                        this.lastFeedbackLocation = new Point(-1, -1);
                        this.lastSnapOffset = Point.Empty;
                        this.dragImageRect = Rectangle.Empty;
                        this.clearDragImageRect = Rectangle.Empty;
                        this.InitiateDrag(initialMouseLocation, dragComponents);
                    }
                }
            }
        }

        private Point AdjustToGrid(Point dragLoc)
        {
            Point point = new Point(dragLoc.X - this.parentLocation.X, dragLoc.Y - this.parentLocation.Y);
            Point empty = Point.Empty;
            int num = point.X % this.parentGridSize.Width;
            int num2 = point.Y % this.parentGridSize.Height;
            if (num > (this.parentGridSize.Width / 2))
            {
                empty.X = this.parentGridSize.Width - num;
            }
            else
            {
                empty.X = -num;
            }
            if (num2 > (this.parentGridSize.Height / 2))
            {
                empty.Y = this.parentGridSize.Height - num2;
                return empty;
            }
            empty.Y = -num2;
            return empty;
        }

        internal void CleanupDrag()
        {
            this.CleanupDrag(true);
        }

        internal void CleanupDrag(bool clearImages)
        {
            if (!this.cleanedUpDrag)
            {
                if (clearImages)
                {
                    this.ClearAllDragImages();
                }
                this.ShowHideDragControls(true);
                try
                {
                    if (this.suspendedParent != null)
                    {
                        this.suspendedParent.ResumeLayout();
                    }
                }
                finally
                {
                    this.suspendedParent = null;
                    this.behaviorServiceSource.EnableAllAdorners(true);
                    if ((this.destHost != this.srcHost) && (this.destHost != null))
                    {
                        this.behaviorServiceTarget.EnableAllAdorners(true);
                        this.behaviorServiceTarget.SyncSelection();
                    }
                    if (this.behaviorServiceSource != null)
                    {
                        this.behaviorServiceSource.SyncSelection();
                    }
                    if (this.dragImageRegion != null)
                    {
                        this.dragImageRegion.Dispose();
                        this.dragImageRegion = null;
                    }
                    if (this.dragImage != null)
                    {
                        this.dragImage.Dispose();
                        this.dragImage = null;
                    }
                    if (this.dragComponents != null)
                    {
                        for (int i = 0; i < this.dragComponents.Length; i++)
                        {
                            if (this.dragComponents[i].dragImage != null)
                            {
                                this.dragComponents[i].dragImage.Dispose();
                                this.dragComponents[i].dragImage = null;
                            }
                        }
                    }
                    if (this.graphicsTarget != null)
                    {
                        this.graphicsTarget.Dispose();
                        this.graphicsTarget = null;
                    }
                    this.cleanedUpDrag = true;
                }
            }
        }

        private void ClearAllDragImages()
        {
            if (this.dragImageRect != Rectangle.Empty)
            {
                Rectangle dragImageRect = this.dragImageRect;
                dragImageRect.Location = this.MapPointFromSourceToTarget(dragImageRect.Location);
                if (this.graphicsTarget != null)
                {
                    this.graphicsTarget.SetClip(dragImageRect);
                }
                if (this.behaviorServiceTarget != null)
                {
                    this.behaviorServiceTarget.Invalidate(dragImageRect);
                }
                if (this.graphicsTarget != null)
                {
                    this.graphicsTarget.ResetClip();
                }
            }
        }

        private void DisableAdorners(IServiceProvider serviceProvider, BehaviorService behaviorService, bool hostChange)
        {
            Adorner bodyGlyphAdorner = null;
            SelectionManager service = (SelectionManager) serviceProvider.GetService(typeof(SelectionManager));
            if (service != null)
            {
                bodyGlyphAdorner = service.BodyGlyphAdorner;
            }
            foreach (Adorner adorner2 in behaviorService.Adorners)
            {
                if ((bodyGlyphAdorner == null) || !adorner2.Equals(bodyGlyphAdorner))
                {
                    adorner2.EnabledInternal = false;
                }
            }
            behaviorService.Invalidate();
            if (hostChange)
            {
                service.OnBeginDrag(new BehaviorDragDropEventArgs(this.dragObjects));
            }
        }

        private void DropControl(int dragComponentIndex, Control dragTarget, Control dragSource, bool localDrag)
        {
            Control dragComponent = this.dragComponents[dragComponentIndex].dragComponent as Control;
            if ((this.lastEffect == DragDropEffects.Copy) || ((this.srcHost != this.destHost) && (this.destHost != null)))
            {
                dragComponent.Visible = true;
                bool flag = true;
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(dragComponent)["Visible"];
                if (descriptor != null)
                {
                    flag = (bool) descriptor.GetValue(dragComponent);
                }
                this.SetDesignerHost(dragComponent);
                dragComponent.Parent = dragTarget;
                if (descriptor != null)
                {
                    descriptor.SetValue(dragComponent, flag);
                }
            }
            else if (!localDrag && dragComponent.Parent.Equals(dragSource))
            {
                dragSource.Controls.Remove(dragComponent);
                dragComponent.Visible = true;
                dragTarget.Controls.Add(dragComponent);
            }
        }

        private void EndDragDrop(bool allowSetChildIndexOnDrop)
        {
            Control target = this.data.Target as Control;
            if (target != null)
            {
                if (this.serviceProviderTarget == null)
                {
                    this.serviceProviderTarget = target.Site;
                    if (this.serviceProviderTarget == null)
                    {
                        return;
                    }
                }
                if (this.destHost == null)
                {
                    this.destHost = (IDesignerHost) this.serviceProviderTarget.GetService(typeof(IDesignerHost));
                    if (this.destHost == null)
                    {
                        return;
                    }
                }
                if (this.behaviorServiceTarget == null)
                {
                    this.behaviorServiceTarget = (BehaviorService) this.serviceProviderTarget.GetService(typeof(BehaviorService));
                    if (this.behaviorServiceTarget == null)
                    {
                        return;
                    }
                }
                ArrayList list = null;
                bool flag = this.lastEffect == DragDropEffects.Copy;
                Control source = this.data.Source;
                bool localDrag = source.Equals(target);
                PropertyDescriptor member = TypeDescriptor.GetProperties(target)["Controls"];
                PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(source)["Controls"];
                IComponentChangeService service = (IComponentChangeService) this.serviceProviderSource.GetService(typeof(IComponentChangeService));
                IComponentChangeService service2 = (IComponentChangeService) this.serviceProviderTarget.GetService(typeof(IComponentChangeService));
                if (this.dragAssistanceManager != null)
                {
                    this.dragAssistanceManager.OnMouseUp();
                }
                ISelectionService service3 = null;
                if (flag || ((this.srcHost != this.destHost) && (this.destHost != null)))
                {
                    service3 = (ISelectionService) this.serviceProviderTarget.GetService(typeof(ISelectionService));
                }
                try
                {
                    if ((this.dragComponents != null) && (this.dragComponents.Length > 0))
                    {
                        string str;
                        DesignerTransaction transaction = null;
                        DesignerTransaction transaction2 = null;
                        if (this.dragComponents.Length == 1)
                        {
                            string componentName = TypeDescriptor.GetComponentName(this.dragComponents[0].dragComponent);
                            if ((componentName == null) || (componentName.Length == 0))
                            {
                                componentName = this.dragComponents[0].dragComponent.GetType().Name;
                            }
                            str = System.Design.SR.GetString(flag ? "BehaviorServiceCopyControl" : "BehaviorServiceMoveControl", new object[] { componentName });
                        }
                        else
                        {
                            str = System.Design.SR.GetString(flag ? "BehaviorServiceCopyControls" : "BehaviorServiceMoveControls", new object[] { this.dragComponents.Length });
                        }
                        if ((this.srcHost != null) && (((this.srcHost == this.destHost) || (this.destHost == null)) || !flag))
                        {
                            transaction = this.srcHost.CreateTransaction(str);
                        }
                        if ((this.srcHost != this.destHost) && (this.destHost != null))
                        {
                            transaction2 = this.destHost.CreateTransaction(str);
                        }
                        try
                        {
                            ComponentTray tray = null;
                            int num = 0;
                            if (flag)
                            {
                                tray = this.serviceProviderTarget.GetService(typeof(ComponentTray)) as ComponentTray;
                                num = (tray != null) ? tray.Controls.Count : 0;
                                ArrayList objects = new ArrayList();
                                for (int j = 0; j < this.dragComponents.Length; j++)
                                {
                                    objects.Add(this.dragComponents[j].dragComponent);
                                }
                                objects = DesignerUtils.CopyDragObjects(objects, this.serviceProviderTarget) as ArrayList;
                                if (objects == null)
                                {
                                    return;
                                }
                                list = new ArrayList();
                                for (int k = 0; k < objects.Count; k++)
                                {
                                    list.Add(this.dragComponents[k].dragComponent);
                                    this.dragComponents[k].dragComponent = objects[k];
                                }
                            }
                            if ((!localDrag || flag) && ((service != null) && (service2 != null)))
                            {
                                service2.OnComponentChanging(target, member);
                                if (!flag)
                                {
                                    service.OnComponentChanging(source, descriptor2);
                                }
                            }
                            int num4 = ParentControlDesigner.DetermineTopChildIndex(target);
                            this.DropControl(this.primaryComponentIndex, target, source, localDrag);
                            Point p = this.behaviorServiceSource.AdornerWindowPointToScreen(this.dragComponents[this.primaryComponentIndex].draggedLocation);
                            p = ((Control) this.dragComponents[this.primaryComponentIndex].dragComponent).Parent.PointToClient(p);
                            if (((Control) this.dragComponents[this.primaryComponentIndex].dragComponent).Parent.IsMirrored)
                            {
                                p.Offset(-((Control) this.dragComponents[this.primaryComponentIndex].dragComponent).Width, 0);
                            }
                            Control dragComponent = this.dragComponents[this.primaryComponentIndex].dragComponent as Control;
                            PropertyDescriptor descriptor3 = TypeDescriptor.GetProperties(dragComponent)["Location"];
                            if ((dragComponent != null) && (descriptor3 != null))
                            {
                                try
                                {
                                    service2.OnComponentChanging(dragComponent, descriptor3);
                                }
                                catch (CheckoutException exception)
                                {
                                    if (exception != CheckoutException.Canceled)
                                    {
                                        throw;
                                    }
                                    return;
                                }
                            }
                            this.SetLocationPropertyAndChildIndex(this.primaryComponentIndex, target, p, this.shareParent ? (num4 + this.dragComponents[this.primaryComponentIndex].zorderIndex) : num4, allowSetChildIndexOnDrop);
                            if (service3 != null)
                            {
                                service3.SetSelectedComponents(new object[] { this.dragComponents[this.primaryComponentIndex].dragComponent }, SelectionTypes.Click | SelectionTypes.Replace);
                            }
                            for (int i = 0; i < this.dragComponents.Length; i++)
                            {
                                if (i != this.primaryComponentIndex)
                                {
                                    this.DropControl(i, target, source, localDrag);
                                    Point dropPoint = new Point(p.X + this.dragComponents[i].positionOffset.X, p.Y + this.dragComponents[i].positionOffset.Y);
                                    this.SetLocationPropertyAndChildIndex(i, target, dropPoint, this.shareParent ? (num4 + this.dragComponents[i].zorderIndex) : num4, allowSetChildIndexOnDrop);
                                    if (service3 != null)
                                    {
                                        service3.SetSelectedComponents(new object[] { this.dragComponents[i].dragComponent }, SelectionTypes.Add);
                                    }
                                }
                            }
                            if ((!localDrag || flag) && ((service != null) && (service2 != null)))
                            {
                                service2.OnComponentChanged(target, member, target.Controls, target.Controls);
                                if (!flag)
                                {
                                    service.OnComponentChanged(source, descriptor2, source.Controls, source.Controls);
                                }
                            }
                            if (list != null)
                            {
                                for (int m = 0; m < list.Count; m++)
                                {
                                    this.dragComponents[m].dragComponent = list[m];
                                }
                                list = null;
                            }
                            if (flag)
                            {
                                if (tray == null)
                                {
                                    tray = this.serviceProviderTarget.GetService(typeof(ComponentTray)) as ComponentTray;
                                }
                                if (tray != null)
                                {
                                    int num7 = tray.Controls.Count - num;
                                    if (num7 > 0)
                                    {
                                        ArrayList components = new ArrayList();
                                        for (int n = 0; n < num7; n++)
                                        {
                                            components.Add(tray.Controls[num + n]);
                                        }
                                        tray.UpdatePastePositions(components);
                                    }
                                }
                            }
                            this.CleanupDrag(false);
                            if (transaction != null)
                            {
                                transaction.Commit();
                                transaction = null;
                            }
                            if (transaction2 != null)
                            {
                                transaction2.Commit();
                                transaction2 = null;
                            }
                        }
                        finally
                        {
                            if (transaction != null)
                            {
                                transaction.Cancel();
                            }
                            if (transaction2 != null)
                            {
                                transaction2.Cancel();
                            }
                        }
                    }
                }
                finally
                {
                    if (list != null)
                    {
                        for (int num9 = 0; num9 < list.Count; num9++)
                        {
                            this.dragComponents[num9].dragComponent = list[num9];
                        }
                    }
                    this.CleanupDrag(false);
                    if (this.statusCommandUITarget != null)
                    {
                        this.statusCommandUITarget.SetStatusInformation((service3 == null) ? (this.dragComponents[this.primaryComponentIndex].dragComponent as Component) : (service3.PrimarySelection as Component));
                    }
                }
                this.lastFeedbackLocation = new Point(-1, -1);
            }
        }

        private void GetParentSnapInfo(Control parentControl, BehaviorService bhvSvc)
        {
            this.parentGridSize = Size.Empty;
            if ((bhvSvc != null) && !bhvSvc.UseSnapLines)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(parentControl)["SnapToGrid"];
                if ((descriptor != null) && ((bool) descriptor.GetValue(parentControl)))
                {
                    PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(parentControl)["GridSize"];
                    if ((descriptor2 != null) && (this.dragComponents[this.primaryComponentIndex].dragComponent is Control))
                    {
                        this.parentGridSize = (Size) descriptor2.GetValue(parentControl);
                        this.parentLocation = bhvSvc.MapAdornerWindowPoint(parentControl.Handle, Point.Empty);
                        if ((parentControl.Parent != null) && parentControl.Parent.IsMirrored)
                        {
                            this.parentLocation.Offset(-parentControl.Width, 0);
                        }
                    }
                }
            }
        }

        internal ArrayList GetSortedDragControls(ref int primaryControlIndex)
        {
            ArrayList list = new ArrayList();
            primaryControlIndex = -1;
            if ((this.dragComponents != null) && (this.dragComponents.Length > 0))
            {
                primaryControlIndex = this.primaryComponentIndex;
                for (int i = 0; i < this.dragComponents.Length; i++)
                {
                    list.Add(this.dragComponents[i].dragComponent);
                }
            }
            return list;
        }

        internal void GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            this.lastEffect = e.Effect;
            if ((this.data.Target == null) || (e.Effect == DragDropEffects.None))
            {
                if (this.clearDragImageRect != this.dragImageRect)
                {
                    this.ClearAllDragImages();
                    this.clearDragImageRect = this.dragImageRect;
                }
                if (this.dragAssistanceManager != null)
                {
                    this.dragAssistanceManager.EraseSnapLines();
                }
            }
            else
            {
                bool flag = false;
                Point mousePosition = Control.MousePosition;
                bool flag2 = Control.ModifierKeys == Keys.Alt;
                if (flag2 && (this.dragAssistanceManager != null))
                {
                    this.dragAssistanceManager.EraseSnapLines();
                }
                if (this.data.Target.Equals(this.data.Source) && (this.lastEffect != DragDropEffects.Copy))
                {
                    e.UseDefaultCursors = false;
                    Cursor.Current = Cursors.Default;
                }
                else
                {
                    e.UseDefaultCursors = true;
                }
                Control target = this.data.Target as Control;
                if ((mousePosition != this.lastFeedbackLocation) || (flag2 && (this.dragAssistanceManager != null)))
                {
                    if (!this.data.Target.Equals(this.lastDropTarget))
                    {
                        this.serviceProviderTarget = target.Site;
                        if (this.serviceProviderTarget == null)
                        {
                            return;
                        }
                        IDesignerHost service = (IDesignerHost) this.serviceProviderTarget.GetService(typeof(IDesignerHost));
                        if (service == null)
                        {
                            return;
                        }
                        this.targetAllowsSnapLines = true;
                        ControlDesigner designer = service.GetDesigner(target) as ControlDesigner;
                        if ((designer != null) && !designer.ParticipatesWithSnapLines)
                        {
                            this.targetAllowsSnapLines = false;
                        }
                        this.statusCommandUITarget = new StatusCommandUI(this.serviceProviderTarget);
                        if ((this.lastDropTarget == null) || (service != this.destHost))
                        {
                            if ((this.destHost != null) && (this.destHost != this.srcHost))
                            {
                                this.behaviorServiceTarget.EnableAllAdorners(true);
                            }
                            this.behaviorServiceTarget = (BehaviorService) this.serviceProviderTarget.GetService(typeof(BehaviorService));
                            if (this.behaviorServiceTarget == null)
                            {
                                return;
                            }
                            this.GetParentSnapInfo(target, this.behaviorServiceTarget);
                            if (service != this.srcHost)
                            {
                                this.DisableAdorners(this.serviceProviderTarget, this.behaviorServiceTarget, true);
                            }
                            this.ClearAllDragImages();
                            if (this.lastDropTarget != null)
                            {
                                for (int i = 0; i < this.dragObjects.Count; i++)
                                {
                                    Control c = (Control) this.dragObjects[i];
                                    Rectangle rect = this.behaviorServiceSource.ControlRectInAdornerWindow(c);
                                    rect.Location = this.behaviorServiceSource.AdornerWindowPointToScreen(rect.Location);
                                    rect.Location = this.behaviorServiceTarget.MapAdornerWindowPoint(IntPtr.Zero, rect.Location);
                                    if (i == 0)
                                    {
                                        if (this.dragImageRegion != null)
                                        {
                                            this.dragImageRegion.Dispose();
                                        }
                                        this.dragImageRegion = new Region(rect);
                                    }
                                    else
                                    {
                                        this.dragImageRegion.Union(rect);
                                    }
                                }
                            }
                            if (this.graphicsTarget != null)
                            {
                                this.graphicsTarget.Dispose();
                            }
                            this.graphicsTarget = this.behaviorServiceTarget.AdornerWindowGraphics;
                            flag = true;
                            this.destHost = service;
                        }
                        this.lastDropTarget = this.data.Target;
                    }
                    if (this.ShowHideDragControls(this.lastEffect == DragDropEffects.Copy) && !flag)
                    {
                        flag = true;
                    }
                    if (flag && this.behaviorServiceTarget.UseSnapLines)
                    {
                        if (this.dragAssistanceManager != null)
                        {
                            this.dragAssistanceManager.EraseSnapLines();
                        }
                        this.dragAssistanceManager = new DragAssistanceManager(this.serviceProviderTarget, this.graphicsTarget, this.dragObjects, null, this.lastEffect == DragDropEffects.Copy);
                    }
                    Point pt = new Point((mousePosition.X - this.initialMouseLoc.X) + this.dragComponents[this.primaryComponentIndex].originalControlLocation.X, (mousePosition.Y - this.initialMouseLoc.Y) + this.dragComponents[this.primaryComponentIndex].originalControlLocation.Y);
                    pt = this.MapPointFromSourceToTarget(pt);
                    Rectangle dragBounds = new Rectangle(pt.X, pt.Y, this.dragComponents[this.primaryComponentIndex].dragImage.Width, this.dragComponents[this.primaryComponentIndex].dragImage.Height);
                    if (this.dragAssistanceManager != null)
                    {
                        if (this.targetAllowsSnapLines && !flag2)
                        {
                            this.lastSnapOffset = this.dragAssistanceManager.OnMouseMove(dragBounds);
                        }
                        else
                        {
                            this.dragAssistanceManager.OnMouseMove(new Rectangle(-100, -100, 0, 0));
                        }
                    }
                    else if (!this.parentGridSize.IsEmpty)
                    {
                        this.lastSnapOffset = this.AdjustToGrid(pt);
                    }
                    pt.X += this.lastSnapOffset.X;
                    pt.Y += this.lastSnapOffset.Y;
                    this.dragComponents[this.primaryComponentIndex].draggedLocation = this.MapPointFromTargetToSource(pt);
                    Rectangle dragImageRect = this.dragImageRect;
                    pt = new Point((mousePosition.X - this.initialMouseLoc.X) + this.originalDragImageLocation.X, (mousePosition.Y - this.initialMouseLoc.Y) + this.originalDragImageLocation.Y) {
                        X = pt.X + this.lastSnapOffset.X,
                        Y = pt.Y + this.lastSnapOffset.Y
                    };
                    this.dragImageRect.Location = pt;
                    dragImageRect.Location = this.MapPointFromSourceToTarget(dragImageRect.Location);
                    Rectangle a = this.dragImageRect;
                    a.Location = this.MapPointFromSourceToTarget(a.Location);
                    Region region = new Region(Rectangle.Union(a, dragImageRect));
                    region.Exclude(a);
                    using (Region region2 = this.dragImageRegion.Clone())
                    {
                        region2.Translate((int) ((mousePosition.X - this.initialMouseLoc.X) + this.lastSnapOffset.X), (int) ((mousePosition.Y - this.initialMouseLoc.Y) + this.lastSnapOffset.Y));
                        region2.Complement(a);
                        region2.Union(region);
                        this.behaviorServiceTarget.Invalidate(region2);
                    }
                    region.Dispose();
                    if (this.graphicsTarget != null)
                    {
                        this.graphicsTarget.SetClip(a);
                        this.graphicsTarget.DrawImage(this.dragImage, a.X, a.Y);
                        this.graphicsTarget.ResetClip();
                    }
                    Control dragComponent = this.dragComponents[this.primaryComponentIndex].dragComponent as Control;
                    if (dragComponent != null)
                    {
                        Point p = this.behaviorServiceSource.AdornerWindowPointToScreen(this.dragComponents[this.primaryComponentIndex].draggedLocation);
                        p = target.PointToClient(p);
                        if (target.IsMirrored && dragComponent.IsMirrored)
                        {
                            p.Offset(-dragComponent.Width, 0);
                        }
                        if (this.statusCommandUITarget != null)
                        {
                            this.statusCommandUITarget.SetStatusInformation(dragComponent, p);
                        }
                    }
                    if (((this.dragAssistanceManager != null) && !flag2) && this.targetAllowsSnapLines)
                    {
                        this.dragAssistanceManager.RenderSnapLinesInternal();
                    }
                    this.lastFeedbackLocation = mousePosition;
                }
                this.data.Target = null;
            }
        }

        private void InitiateDrag(Point initialMouseLocation, ICollection dragComps)
        {
            this.dragObjects = new ArrayList(dragComps);
            this.DisableAdorners(this.serviceProviderSource, this.behaviorServiceSource, false);
            Control control = this.dragObjects[0] as Control;
            Control control2 = (control != null) ? control.Parent : null;
            Color backColor = (control2 != null) ? control2.BackColor : Color.Empty;
            this.dragImageRect = Rectangle.Empty;
            this.clearDragImageRect = Rectangle.Empty;
            this.initialMouseLoc = initialMouseLocation;
            for (int i = 0; i < this.dragObjects.Count; i++)
            {
                Control c = (Control) this.dragObjects[i];
                this.dragComponents[i].dragComponent = this.dragObjects[i];
                this.dragComponents[i].positionOffset = new Point(c.Location.X - control.Location.X, c.Location.Y - control.Location.Y);
                Rectangle rect = this.behaviorServiceSource.ControlRectInAdornerWindow(c);
                if (this.dragImageRect.IsEmpty)
                {
                    this.dragImageRect = rect;
                    this.dragImageRegion = new Region(rect);
                }
                else
                {
                    this.dragImageRect = Rectangle.Union(this.dragImageRect, rect);
                    this.dragImageRegion.Union(rect);
                }
                this.dragComponents[i].draggedLocation = rect.Location;
                this.dragComponents[i].originalControlLocation = this.dragComponents[i].draggedLocation;
                DesignerUtils.GenerateSnapShot(c, ref this.dragComponents[i].dragImage, (i == 0) ? 2 : 1, 1.0, backColor);
                if ((control2 != null) && this.shareParent)
                {
                    this.dragComponents[i].zorderIndex = control2.Controls.GetChildIndex(c, false);
                    if (this.dragComponents[i].zorderIndex == -1)
                    {
                        this.shareParent = false;
                    }
                }
            }
            if (this.shareParent)
            {
                Array.Sort(this.dragComponents, this);
            }
            for (int j = 0; j < this.dragComponents.Length; j++)
            {
                if (control.Equals(this.dragComponents[j].dragComponent as Control))
                {
                    this.primaryComponentIndex = j;
                    break;
                }
            }
            if (control2 != null)
            {
                this.suspendedParent = control2;
                this.suspendedParent.SuspendLayout();
                this.GetParentSnapInfo(this.suspendedParent, this.behaviorServiceSource);
            }
            int width = this.dragImageRect.Width;
            if (width == 0)
            {
                width = 1;
            }
            int height = this.dragImageRect.Height;
            if (height == 0)
            {
                height = 1;
            }
            this.dragImage = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            using (Graphics graphics = Graphics.FromImage(this.dragImage))
            {
                graphics.Clear(Color.Chartreuse);
            }
            ((Bitmap) this.dragImage).MakeTransparent(Color.Chartreuse);
            using (Graphics graphics2 = Graphics.FromImage(this.dragImage))
            {
                using (SolidBrush brush = new SolidBrush(control.BackColor))
                {
                    for (int k = 0; k < this.dragComponents.Length; k++)
                    {
                        Rectangle rectangle2 = new Rectangle(this.dragComponents[k].draggedLocation.X - this.dragImageRect.X, this.dragComponents[k].draggedLocation.Y - this.dragImageRect.Y, this.dragComponents[k].dragImage.Width, this.dragComponents[k].dragImage.Height);
                        graphics2.FillRectangle(brush, rectangle2);
                        graphics2.DrawImage(this.dragComponents[k].dragImage, rectangle2, new Rectangle(0, 0, this.dragComponents[k].dragImage.Width, this.dragComponents[k].dragImage.Height), GraphicsUnit.Pixel);
                    }
                }
            }
            this.originalDragImageLocation = new Point(this.dragImageRect.X, this.dragImageRect.Y);
            this.ShowHideDragControls(false);
            this.cleanedUpDrag = false;
        }

        private Point MapPointFromSourceToTarget(Point pt)
        {
            if ((this.srcHost != this.destHost) && (this.destHost != null))
            {
                pt = this.behaviorServiceSource.AdornerWindowPointToScreen(pt);
                return this.behaviorServiceTarget.MapAdornerWindowPoint(IntPtr.Zero, pt);
            }
            return pt;
        }

        private Point MapPointFromTargetToSource(Point pt)
        {
            if ((this.srcHost != this.destHost) && (this.destHost != null))
            {
                pt = this.behaviorServiceTarget.AdornerWindowPointToScreen(pt);
                return this.behaviorServiceSource.MapAdornerWindowPoint(IntPtr.Zero, pt);
            }
            return pt;
        }

        internal void QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if ((this.behaviorServiceSource != null) && this.behaviorServiceSource.CancelDrag)
            {
                e.Action = DragAction.Cancel;
                this.CleanupDrag(true);
            }
            else if ((e.Action != DragAction.Continue) && ((e.Action == DragAction.Cancel) || (this.lastEffect == DragDropEffects.None)))
            {
                this.CleanupDrag(true);
                e.Action = DragAction.Cancel;
            }
        }

        private void SetDesignerHost(Control c)
        {
            foreach (Control control in c.Controls)
            {
                this.SetDesignerHost(control);
            }
            if (((c.Site != null) && !(c.Site is INestedSite)) && (this.destHost != null))
            {
                this.destHost.Container.Add(c);
            }
        }

        private void SetLocationPropertyAndChildIndex(int dragComponentIndex, Control dragTarget, Point dropPoint, int newIndex, bool allowSetChildIndexOnDrop)
        {
            Control dragComponent = this.dragComponents[dragComponentIndex].dragComponent as Control;
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.dragComponents[dragComponentIndex].dragComponent)["Location"];
            if ((descriptor != null) && (dragComponent != null))
            {
                Point point = new Point(dropPoint.X, dropPoint.Y);
                ScrollableControl parent = dragComponent.Parent as ScrollableControl;
                if (parent != null)
                {
                    Point autoScrollPosition = parent.AutoScrollPosition;
                    point.Offset(-autoScrollPosition.X, -autoScrollPosition.Y);
                }
                descriptor.SetValue(dragComponent, point);
                if (allowSetChildIndexOnDrop)
                {
                    dragTarget.Controls.SetChildIndex(dragComponent, newIndex);
                }
            }
        }

        internal bool ShowHideDragControls(bool show)
        {
            if (this.currentShowState == show)
            {
                return false;
            }
            this.currentShowState = show;
            if (this.dragComponents != null)
            {
                for (int i = 0; i < this.dragComponents.Length; i++)
                {
                    Control dragComponent = this.dragComponents[i].dragComponent as Control;
                    if (dragComponent != null)
                    {
                        dragComponent.Visible = show;
                    }
                }
            }
            return true;
        }

        int IComparer.Compare(object x, object y)
        {
            DragComponent component = (DragComponent) x;
            DragComponent component2 = (DragComponent) y;
            if (component.zorderIndex > component2.zorderIndex)
            {
                return -1;
            }
            if (component.zorderIndex < component2.zorderIndex)
            {
                return 1;
            }
            return 0;
        }

        internal DragDropEffects AllowedEffects
        {
            get
            {
                return this.allowedEffects;
            }
        }

        internal System.Windows.Forms.DataObject DataObject
        {
            get
            {
                return this.data;
            }
        }

        internal class BehaviorDataObject : DataObject
        {
            private ICollection dragComponents;
            private Control source;
            private DropSourceBehavior sourceBehavior;
            private IComponent target;

            public BehaviorDataObject(ICollection dragComponents, Control source, DropSourceBehavior sourceBehavior)
            {
                this.dragComponents = dragComponents;
                this.source = source;
                this.sourceBehavior = sourceBehavior;
                this.target = null;
            }

            internal void CleanupDrag()
            {
                this.sourceBehavior.CleanupDrag();
            }

            internal void EndDragDrop(bool allowSetChildIndexOnDrop)
            {
                this.sourceBehavior.EndDragDrop(allowSetChildIndexOnDrop);
            }

            internal ArrayList GetSortedDragControls(ref int primaryControlIndex)
            {
                return this.sourceBehavior.GetSortedDragControls(ref primaryControlIndex);
            }

            public ICollection DragComponents
            {
                get
                {
                    return this.dragComponents;
                }
            }

            public Control Source
            {
                get
                {
                    return this.source;
                }
            }

            public IComponent Target
            {
                get
                {
                    return this.target;
                }
                set
                {
                    this.target = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DragComponent
        {
            public object dragComponent;
            public int zorderIndex;
            public Point originalControlLocation;
            public Point draggedLocation;
            public Image dragImage;
            public Point positionOffset;
        }
    }
}

