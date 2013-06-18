namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    internal sealed class WindowManager : WorkflowDesignerMessageFilter
    {
        private ActivityDesigner currentActiveDesigner;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal WindowManager()
        {
        }

        private ActivityDesigner GetDesignerWithFocus()
        {
            ActivityDesigner designer = null;
            ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
            if (service == null)
            {
                return designer;
            }
            object primarySelection = service.PrimarySelection;
            if (primarySelection is Activity)
            {
                return ActivityDesigner.GetDesigner(primarySelection as Activity);
            }
            return ActivityDesigner.GetParentDesigner(primarySelection);
        }

        protected override bool OnDragEnter(DragEventArgs eventArgs)
        {
            if (this.currentActiveDesigner != null)
            {
                ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseLeave();
                this.currentActiveDesigner = null;
            }
            return false;
        }

        protected override bool OnKeyDown(KeyEventArgs eventArgs)
        {
            if ((eventArgs != null) && ((eventArgs.KeyCode == Keys.PageUp) || (eventArgs.KeyCode == Keys.Next)))
            {
                this.UpdateViewOnPageUpDown(eventArgs.KeyCode == Keys.PageUp);
            }
            ISelectionService service = ((IServiceProvider) base.ParentView).GetService(typeof(ISelectionService)) as ISelectionService;
            if (eventArgs.KeyCode == Keys.Enter)
            {
                IDesigner designer = ActivityDesigner.GetDesigner(service.PrimarySelection as Activity);
                if (designer != null)
                {
                    designer.DoDefaultAction();
                    eventArgs.Handled = true;
                }
            }
            else if (eventArgs.KeyCode == Keys.Escape)
            {
                if (!eventArgs.Handled)
                {
                    CompositeActivityDesigner parentDesigner = ActivityDesigner.GetParentDesigner(service.PrimarySelection);
                    if (parentDesigner != null)
                    {
                        service.SetSelectedComponents(new object[] { parentDesigner.Activity }, SelectionTypes.Replace);
                    }
                    eventArgs.Handled = true;
                }
            }
            else if (eventArgs.KeyCode == Keys.Delete)
            {
                IDesignerHost host = ((IServiceProvider) base.ParentView).GetService(typeof(IDesignerHost)) as IDesignerHost;
                if ((host != null) && !service.GetComponentSelected(host.RootComponent))
                {
                    ICollection selectedComponents = service.GetSelectedComponents();
                    if (DesignerHelpers.AreComponentsRemovable(selectedComponents))
                    {
                        List<Activity> activities = new List<Activity>(Helpers.GetTopLevelActivities(service.GetSelectedComponents()));
                        bool flag = activities.Count > 0;
                        foreach (DictionaryEntry entry in Helpers.PairUpCommonParentActivities(activities))
                        {
                            CompositeActivityDesigner designer3 = ActivityDesigner.GetDesigner(entry.Key as Activity) as CompositeActivityDesigner;
                            if ((designer3 != null) && !designer3.CanRemoveActivities(new List<Activity>((Activity[]) ((ArrayList) entry.Value).ToArray(typeof(Activity))).AsReadOnly()))
                            {
                                flag = false;
                            }
                        }
                        if (flag)
                        {
                            List<ConnectorHitTestInfo> components = new List<ConnectorHitTestInfo>();
                            foreach (object obj2 in selectedComponents)
                            {
                                ConnectorHitTestInfo item = obj2 as ConnectorHitTestInfo;
                                if (item != null)
                                {
                                    components.Add(item);
                                }
                            }
                            CompositeActivityDesigner.RemoveActivities(base.ParentView, activities.AsReadOnly(), SR.GetString("DeletingActivities"));
                            if ((service != null) && (components.Count > 0))
                            {
                                service.SetSelectedComponents(components, SelectionTypes.Add);
                            }
                            eventArgs.Handled = true;
                        }
                    }
                }
            }
            else if (((eventArgs.KeyCode == Keys.Left) || (eventArgs.KeyCode == Keys.Right)) || (((eventArgs.KeyCode == Keys.Up) || (eventArgs.KeyCode == Keys.Down)) || (eventArgs.KeyCode == Keys.Tab)))
            {
                ActivityDesigner designer4 = ActivityDesigner.GetDesigner(service.PrimarySelection as Activity);
                if ((designer4 != null) && (designer4.ParentDesigner != null))
                {
                    ((IWorkflowDesignerMessageSink) designer4.ParentDesigner).OnKeyDown(eventArgs);
                    eventArgs.Handled = true;
                }
            }
            if (!eventArgs.Handled)
            {
                ActivityDesigner designerWithFocus = this.GetDesignerWithFocus();
                if (designerWithFocus != null)
                {
                    ((IWorkflowDesignerMessageSink) designerWithFocus).OnKeyDown(eventArgs);
                }
            }
            return eventArgs.Handled;
        }

        protected override bool OnKeyUp(KeyEventArgs eventArgs)
        {
            ActivityDesigner designerWithFocus = this.GetDesignerWithFocus();
            if (designerWithFocus != null)
            {
                ((IWorkflowDesignerMessageSink) designerWithFocus).OnKeyUp(eventArgs);
            }
            return false;
        }

        protected override void OnLayout(LayoutEventArgs eventArgs)
        {
            WorkflowView parentView = base.ParentView;
            using (Graphics graphics = parentView.CreateGraphics())
            {
                if (parentView.RootDesigner != null)
                {
                    try
                    {
                        ((IWorkflowDesignerMessageSink) parentView.RootDesigner).OnLayoutSize(graphics);
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        ((IWorkflowDesignerMessageSink) parentView.RootDesigner).OnLayoutPosition(graphics);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        protected override bool OnMouseCaptureChanged()
        {
            if (this.currentActiveDesigner != null)
            {
                ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseCaptureChanged();
            }
            return false;
        }

        protected override bool OnMouseDoubleClick(MouseEventArgs eventArgs)
        {
            ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
            if (service != null)
            {
                ArrayList list = new ArrayList(service.GetSelectedComponents());
                for (int i = 0; i < list.Count; i++)
                {
                    Activity activity = list[i] as Activity;
                    if (activity != null)
                    {
                        IDesigner designer = ActivityDesigner.GetDesigner(activity);
                        if (designer != null)
                        {
                            designer.DoDefaultAction();
                            ((IWorkflowDesignerMessageSink) designer).OnMouseDoubleClick(eventArgs);
                            break;
                        }
                    }
                }
            }
            return false;
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);
            WorkflowView parentView = base.ParentView;
            if (!parentView.IsClientPointInActiveLayout(clientPoint))
            {
                return true;
            }
            object activity = null;
            System.Workflow.ComponentModel.Design.HitTestInfo messageHitTestContext = base.MessageHitTestContext;
            if (messageHitTestContext == System.Workflow.ComponentModel.Design.HitTestInfo.Nowhere)
            {
                activity = parentView.RootDesigner.Activity;
            }
            else
            {
                activity = messageHitTestContext.SelectableObject;
            }
            if (activity != null)
            {
                ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
                if (service != null)
                {
                    service.SetSelectedComponents(new object[] { activity }, SelectionTypes.Click);
                }
            }
            if (this.currentActiveDesigner != messageHitTestContext.AssociatedDesigner)
            {
                if (this.currentActiveDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseLeave();
                }
                this.currentActiveDesigner = messageHitTestContext.AssociatedDesigner;
                if (this.currentActiveDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseEnter(eventArgs);
                }
            }
            if ((this.currentActiveDesigner != null) && ((Control.ModifierKeys & (Keys.Control | Keys.Shift)) == Keys.None))
            {
                ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseDown(eventArgs);
            }
            return false;
        }

        protected override bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            if (this.currentActiveDesigner != null)
            {
                ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseEnter(eventArgs);
            }
            return false;
        }

        protected override bool OnMouseHover(MouseEventArgs eventArgs)
        {
            if (this.currentActiveDesigner != null)
            {
                ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseHover(eventArgs);
            }
            return false;
        }

        protected override bool OnMouseLeave()
        {
            if (this.currentActiveDesigner != null)
            {
                ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseLeave();
                this.currentActiveDesigner = null;
            }
            return false;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);
            if (!base.ParentView.IsClientPointInActiveLayout(clientPoint))
            {
                if (this.currentActiveDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseLeave();
                }
                this.currentActiveDesigner = null;
                return true;
            }
            System.Workflow.ComponentModel.Design.HitTestInfo messageHitTestContext = base.MessageHitTestContext;
            if (this.currentActiveDesigner != messageHitTestContext.AssociatedDesigner)
            {
                if (this.currentActiveDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseLeave();
                }
                this.currentActiveDesigner = messageHitTestContext.AssociatedDesigner;
                if (this.currentActiveDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseEnter(eventArgs);
                }
            }
            else if (this.currentActiveDesigner != null)
            {
                ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseMove(eventArgs);
            }
            return false;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);
            if (!base.ParentView.IsClientPointInActiveLayout(clientPoint))
            {
                if (this.currentActiveDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseLeave();
                }
                this.currentActiveDesigner = null;
                return true;
            }
            System.Workflow.ComponentModel.Design.HitTestInfo messageHitTestContext = base.MessageHitTestContext;
            if (this.currentActiveDesigner != messageHitTestContext.AssociatedDesigner)
            {
                if (this.currentActiveDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseLeave();
                }
                this.currentActiveDesigner = messageHitTestContext.AssociatedDesigner;
                if (this.currentActiveDesigner != null)
                {
                    ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseEnter(eventArgs);
                }
            }
            if (this.currentActiveDesigner != null)
            {
                ((IWorkflowDesignerMessageSink) this.currentActiveDesigner).OnMouseUp(eventArgs);
            }
            return false;
        }

        protected override bool OnMouseWheel(MouseEventArgs eventArgs)
        {
            this.UpdateViewOnMouseWheel(eventArgs, Control.ModifierKeys);
            return true;
        }

        protected override bool OnScroll(ScrollBar sender, int value)
        {
            ActivityDesigner designerWithFocus = this.GetDesignerWithFocus();
            if (designerWithFocus != null)
            {
                ((IWorkflowDesignerMessageSink) designerWithFocus).OnScroll(sender, value);
            }
            return false;
        }

        protected override bool OnShowContextMenu(Point screenMenuPoint)
        {
            IMenuCommandService service = base.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (service != null)
            {
                service.ShowContextMenu(WorkflowMenuCommands.SelectionMenu, screenMenuPoint.X, screenMenuPoint.Y);
            }
            return true;
        }

        protected override void OnThemeChange()
        {
            WorkflowView parentView = base.ParentView;
            if (parentView.RootDesigner != null)
            {
                ((IWorkflowDesignerMessageSink) parentView.RootDesigner).OnThemeChange();
            }
        }

        protected override bool ProcessMessage(Message message)
        {
            ActivityDesigner designerWithFocus = this.GetDesignerWithFocus();
            if (designerWithFocus != null)
            {
                ((IWorkflowDesignerMessageSink) designerWithFocus).ProcessMessage(message);
            }
            return false;
        }

        private void UpdateViewOnMouseWheel(MouseEventArgs eventArgs, Keys modifierKeys)
        {
            WorkflowView parentView = base.ParentView;
            if (Control.ModifierKeys == Keys.Control)
            {
                int num = parentView.Zoom + ((eventArgs.Delta / 120) * 10);
                num = Math.Min(Math.Max(num, 10), 400);
                parentView.Zoom = num;
            }
            else
            {
                int num2 = -eventArgs.Delta / 120;
                int smallChange = parentView.VScrollBar.SmallChange;
                Point scrollPosition = parentView.ScrollPosition;
                scrollPosition.Y += num2 * smallChange;
                parentView.ScrollPosition = scrollPosition;
            }
        }

        private void UpdateViewOnPageUpDown(bool pageUp)
        {
            WorkflowView parentView = base.ParentView;
            Point scrollPosition = parentView.ScrollPosition;
            scrollPosition.Y += (pageUp ? -1 : 1) * parentView.VScrollBar.LargeChange;
            parentView.ScrollPosition = scrollPosition;
        }
    }
}

