namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal sealed class ToolStripPanelSelectionBehavior : System.Windows.Forms.Design.Behavior.Behavior
    {
        private BehaviorService behaviorService;
        private const int defaultBounds = 0x19;
        private ToolStripPanel relatedControl;
        private IServiceProvider serviceProvider;

        internal ToolStripPanelSelectionBehavior(ToolStripPanel containerControl, IServiceProvider serviceProvider)
        {
            this.behaviorService = (BehaviorService) serviceProvider.GetService(typeof(BehaviorService));
            if (this.behaviorService != null)
            {
                this.relatedControl = containerControl;
                this.serviceProvider = serviceProvider;
            }
        }

        private static bool DragComponentContainsToolStrip(DropSourceBehavior.BehaviorDataObject data)
        {
            if (data != null)
            {
                ArrayList list = new ArrayList(data.DragComponents);
                foreach (Component component in list)
                {
                    if (component is ToolStrip)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ExpandPanel(bool setSelection)
        {
            switch (this.relatedControl.Dock)
            {
                case DockStyle.Top:
                    this.relatedControl.Padding = new Padding(0, 0, 0, 0x19);
                    break;

                case DockStyle.Bottom:
                    this.relatedControl.Padding = new Padding(0, 0x19, 0, 0);
                    break;

                case DockStyle.Left:
                    this.relatedControl.Padding = new Padding(0, 0, 0x19, 0);
                    break;

                case DockStyle.Right:
                    this.relatedControl.Padding = new Padding(0x19, 0, 0, 0);
                    break;
            }
            if (setSelection)
            {
                ISelectionService service = (ISelectionService) this.serviceProvider.GetService(typeof(ISelectionService));
                if (service != null)
                {
                    service.SetSelectedComponents(new object[] { this.relatedControl }, SelectionTypes.Replace);
                }
            }
        }

        public override void OnDragDrop(Glyph g, DragEventArgs e)
        {
            ToolStripPanelSelectionGlyph glyph = g as ToolStripPanelSelectionGlyph;
            bool flag = false;
            ArrayList controls = null;
            DropSourceBehavior.BehaviorDataObject data = e.Data as DropSourceBehavior.BehaviorDataObject;
            if (data == null)
            {
                if ((e.Data is DataObject) && (controls == null))
                {
                    IToolboxService service = (IToolboxService) this.serviceProvider.GetService(typeof(IToolboxService));
                    IDesignerHost host = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if ((service != null) && (host != null))
                    {
                        ToolboxItem tool = service.DeserializeToolboxItem(e.Data, host);
                        if (((tool.GetType(host) == typeof(ToolStrip)) || (tool.GetType(host) == typeof(MenuStrip))) || (tool.GetType(host) == typeof(StatusStrip)))
                        {
                            ToolStripPanelDesigner designer = host.GetDesigner(this.relatedControl) as ToolStripPanelDesigner;
                            if (designer != null)
                            {
                                OleDragDropHandler oleDragHandler = designer.GetOleDragHandler();
                                if (oleDragHandler != null)
                                {
                                    oleDragHandler.CreateTool(tool, this.relatedControl, 0, 0, 0, 0, false, false);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                controls = new ArrayList(data.DragComponents);
                foreach (Component component in controls)
                {
                    ToolStrip strip = component as ToolStrip;
                    if ((strip != null) && (strip.Parent != this.relatedControl))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    Control parent = this.relatedControl.Parent;
                    if (parent != null)
                    {
                        try
                        {
                            parent.SuspendLayout();
                            this.ExpandPanel(false);
                            Rectangle bounds = glyph.Bounds;
                            glyph.IsExpanded = true;
                            this.behaviorService.Invalidate(bounds);
                            this.behaviorService.Invalidate(glyph.Bounds);
                            this.ReParentControls(controls, e.Effect == DragDropEffects.Copy);
                        }
                        finally
                        {
                            parent.ResumeLayout(true);
                        }
                    }
                }
                data.CleanupDrag();
            }
        }

        public override void OnDragEnter(Glyph g, DragEventArgs e)
        {
            e.Effect = DragComponentContainsToolStrip(e.Data as DropSourceBehavior.BehaviorDataObject) ? ((Control.ModifierKeys == Keys.Control) ? DragDropEffects.Copy : DragDropEffects.Move) : DragDropEffects.None;
        }

        public override void OnDragOver(Glyph g, DragEventArgs e)
        {
            e.Effect = DragComponentContainsToolStrip(e.Data as DropSourceBehavior.BehaviorDataObject) ? ((Control.ModifierKeys == Keys.Control) ? DragDropEffects.Copy : DragDropEffects.Move) : DragDropEffects.None;
        }

        public override bool OnMouseDown(Glyph g, MouseButtons button, Point mouseLoc)
        {
            ToolStripPanelSelectionGlyph glyph = g as ToolStripPanelSelectionGlyph;
            if ((button == MouseButtons.Left) && (glyph != null))
            {
                if (!glyph.IsExpanded)
                {
                    this.ExpandPanel(true);
                    Rectangle bounds = glyph.Bounds;
                    glyph.IsExpanded = true;
                    this.behaviorService.Invalidate(bounds);
                    this.behaviorService.Invalidate(glyph.Bounds);
                }
                else
                {
                    this.relatedControl.Padding = new Padding(0);
                    Rectangle rect = glyph.Bounds;
                    glyph.IsExpanded = false;
                    this.behaviorService.Invalidate(rect);
                    this.behaviorService.Invalidate(glyph.Bounds);
                    ISelectionService service = (ISelectionService) this.serviceProvider.GetService(typeof(ISelectionService));
                    Component primarySelection = service.PrimarySelection as Component;
                    if (primarySelection != this.relatedControl.Parent)
                    {
                        if (service != null)
                        {
                            service.SetSelectedComponents(new object[] { this.relatedControl.Parent }, SelectionTypes.Replace);
                        }
                    }
                    else
                    {
                        Control parent = this.relatedControl.Parent;
                        parent.PerformLayout();
                        ((SelectionManager) this.serviceProvider.GetService(typeof(SelectionManager))).Refresh();
                        Point location = this.behaviorService.ControlToAdornerWindow(parent);
                        Rectangle rectangle3 = new Rectangle(location, parent.Size);
                        this.behaviorService.Invalidate(rectangle3);
                    }
                }
            }
            return false;
        }

        private void ReParentControls(ArrayList controls, bool copy)
        {
            IDesignerHost host = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if ((host != null) && (controls.Count > 0))
            {
                string str;
                if ((controls.Count == 1) && (controls[0] is ToolStrip))
                {
                    string componentName = TypeDescriptor.GetComponentName(controls[0]);
                    if ((componentName == null) || (componentName.Length == 0))
                    {
                        componentName = controls[0].GetType().Name;
                    }
                    str = System.Design.SR.GetString(copy ? "BehaviorServiceCopyControl" : "BehaviorServiceMoveControl", new object[] { componentName });
                }
                else
                {
                    str = System.Design.SR.GetString(copy ? "BehaviorServiceCopyControls" : "BehaviorServiceMoveControls", new object[] { controls.Count });
                }
                DesignerTransaction transaction = host.CreateTransaction(str);
                try
                {
                    ArrayList objects = null;
                    ISelectionService service = (ISelectionService) this.serviceProvider.GetService(typeof(ISelectionService));
                    if (copy)
                    {
                        objects = new ArrayList();
                        service = (ISelectionService) this.serviceProvider.GetService(typeof(ISelectionService));
                    }
                    for (int i = 0; i < controls.Count; i++)
                    {
                        Control control = controls[i] as Control;
                        if (control is ToolStrip)
                        {
                            if (copy)
                            {
                                objects.Clear();
                                objects.Add(control);
                                objects = DesignerUtils.CopyDragObjects(objects, this.serviceProvider) as ArrayList;
                                if (objects != null)
                                {
                                    control = objects[0] as Control;
                                    control.Visible = true;
                                }
                            }
                            Control relatedControl = this.relatedControl;
                            IComponentChangeService service2 = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                            PropertyDescriptor member = TypeDescriptor.GetProperties(relatedControl)["Controls"];
                            Control parent = control.Parent;
                            if ((parent != null) && !copy)
                            {
                                if (service2 != null)
                                {
                                    service2.OnComponentChanging(parent, member);
                                }
                                parent.Controls.Remove(control);
                            }
                            if (service2 != null)
                            {
                                service2.OnComponentChanging(relatedControl, member);
                            }
                            relatedControl.Controls.Add(control);
                            if (((service2 != null) && (parent != null)) && !copy)
                            {
                                service2.OnComponentChanged(parent, member, null, null);
                            }
                            if (service2 != null)
                            {
                                service2.OnComponentChanged(relatedControl, member, null, null);
                            }
                            if (service != null)
                            {
                                service.SetSelectedComponents(new object[] { control }, (i == 0) ? (SelectionTypes.Click | SelectionTypes.Replace) : SelectionTypes.Add);
                            }
                        }
                    }
                }
                catch
                {
                    if (transaction != null)
                    {
                        transaction.Cancel();
                        transaction = null;
                    }
                }
                finally
                {
                    if (transaction != null)
                    {
                        transaction.Commit();
                        transaction = null;
                    }
                }
            }
        }
    }
}

