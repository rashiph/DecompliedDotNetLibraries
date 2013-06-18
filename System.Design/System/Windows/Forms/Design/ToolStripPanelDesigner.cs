namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class ToolStripPanelDesigner : ScrollableControlDesigner
    {
        private static System.Windows.Forms.Padding _defaultPadding = new System.Windows.Forms.Padding(0);
        private ToolStripPanelSelectionBehavior behavior;
        private IComponentChangeService componentChangeSvc;
        private ToolStripPanelSelectionGlyph containerSelectorGlyph;
        private BaseContextMenuStrip contextMenu;
        private IDesignerHost designerHost;
        private MenuCommand designerShortCutCommand;
        private MenuCommand oldShortCutCommand;
        private ToolStripPanel panel;
        private ISelectionService selectionSvc;

        public override bool CanBeParentedTo(IDesigner parentDesigner)
        {
            return ((this.panel != null) && !(this.panel.Parent is ToolStripContainer));
        }

        public override bool CanParent(Control control)
        {
            return (control is ToolStrip);
        }

        private void ComponentChangeSvc_ComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (this.containerSelectorGlyph != null)
            {
                this.containerSelectorGlyph.UpdateGlyph();
            }
        }

        protected override IComponent[] CreateToolCore(ToolboxItem tool, int x, int y, int width, int height, bool hasLocation, bool hasSize)
        {
            if (tool != null)
            {
                System.Type c = tool.GetType(this.designerHost);
                if (!typeof(ToolStrip).IsAssignableFrom(c))
                {
                    ToolStripContainer parent = this.panel.Parent as ToolStripContainer;
                    if (parent != null)
                    {
                        ToolStripContentPanel contentPanel = parent.ContentPanel;
                        if (contentPanel != null)
                        {
                            PanelDesigner toInvoke = this.designerHost.GetDesigner(contentPanel) as PanelDesigner;
                            if (toInvoke != null)
                            {
                                ParentControlDesigner.InvokeCreateTool(toInvoke, tool);
                            }
                        }
                    }
                }
                else
                {
                    base.CreateToolCore(tool, x, y, width, height, hasLocation, hasSize);
                }
            }
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                base.Dispose(disposing);
            }
            finally
            {
                if (disposing && (this.contextMenu != null))
                {
                    this.contextMenu.Dispose();
                }
                if (this.selectionSvc != null)
                {
                    this.selectionSvc.SelectionChanging -= new EventHandler(this.OnSelectionChanging);
                    this.selectionSvc.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
                    this.selectionSvc = null;
                }
                if (this.componentChangeSvc != null)
                {
                    this.componentChangeSvc.ComponentChanged -= new ComponentChangedEventHandler(this.ComponentChangeSvc_ComponentChanged);
                }
                this.panel.ControlAdded -= new ControlEventHandler(this.OnControlAdded);
                this.panel.ControlRemoved -= new ControlEventHandler(this.OnControlRemoved);
            }
        }

        private void DrawBorder(Graphics graphics)
        {
            Pen borderPen = this.BorderPen;
            Rectangle clientRectangle = this.Control.ClientRectangle;
            clientRectangle.Width--;
            clientRectangle.Height--;
            graphics.DrawRectangle(borderPen, clientRectangle);
            borderPen.Dispose();
        }

        internal void ExpandTopPanel()
        {
            if (this.containerSelectorGlyph == null)
            {
                this.behavior = new ToolStripPanelSelectionBehavior(this.panel, base.Component.Site);
                this.containerSelectorGlyph = new ToolStripPanelSelectionGlyph(Rectangle.Empty, Cursors.Default, this.panel, base.Component.Site, this.behavior);
            }
            if ((this.panel != null) && (this.panel.Dock == DockStyle.Top))
            {
                this.panel.Padding = new System.Windows.Forms.Padding(0, 0, 0x19, 0x19);
                this.containerSelectorGlyph.IsExpanded = true;
            }
        }

        internal Glyph GetGlyph()
        {
            if (this.panel != null)
            {
                if (this.containerSelectorGlyph == null)
                {
                    this.behavior = new ToolStripPanelSelectionBehavior(this.panel, base.Component.Site);
                    this.containerSelectorGlyph = new ToolStripPanelSelectionGlyph(Rectangle.Empty, Cursors.Default, this.panel, base.Component.Site, this.behavior);
                }
                if (this.panel.Visible)
                {
                    return this.containerSelectorGlyph;
                }
            }
            return null;
        }

        protected override Control GetParentForComponent(IComponent component)
        {
            System.Type c = component.GetType();
            if (typeof(ToolStrip).IsAssignableFrom(c))
            {
                return this.panel;
            }
            ToolStripContainer parent = this.panel.Parent as ToolStripContainer;
            if (parent != null)
            {
                return parent.ContentPanel;
            }
            return null;
        }

        public override void Initialize(IComponent component)
        {
            this.panel = component as ToolStripPanel;
            base.Initialize(component);
            this.Padding = this.panel.Padding;
            this.designerHost = (IDesignerHost) component.Site.GetService(typeof(IDesignerHost));
            if (this.selectionSvc == null)
            {
                this.selectionSvc = (ISelectionService) this.GetService(typeof(ISelectionService));
                this.selectionSvc.SelectionChanging += new EventHandler(this.OnSelectionChanging);
                this.selectionSvc.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            }
            if (this.designerHost != null)
            {
                this.componentChangeSvc = (IComponentChangeService) this.designerHost.GetService(typeof(IComponentChangeService));
            }
            if (this.componentChangeSvc != null)
            {
                this.componentChangeSvc.ComponentChanged += new ComponentChangedEventHandler(this.ComponentChangeSvc_ComponentChanged);
            }
            this.panel.ControlAdded += new ControlEventHandler(this.OnControlAdded);
            this.panel.ControlRemoved += new ControlEventHandler(this.OnControlRemoved);
        }

        internal void InvalidateGlyph()
        {
            if (this.containerSelectorGlyph != null)
            {
                base.BehaviorService.Invalidate(this.containerSelectorGlyph.Bounds);
            }
        }

        protected override void OnContextMenu(int x, int y)
        {
            if ((this.panel != null) && (this.panel.Parent is ToolStripContainer))
            {
                this.DesignerContextMenu.Show(x, y);
            }
            else
            {
                base.OnContextMenu(x, y);
            }
        }

        private void OnControlAdded(object sender, ControlEventArgs e)
        {
            if (e.Control is ToolStrip)
            {
                this.panel.Padding = new System.Windows.Forms.Padding(0);
                if (this.containerSelectorGlyph != null)
                {
                    this.containerSelectorGlyph.IsExpanded = false;
                }
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(e.Control)["Dock"];
                if (descriptor != null)
                {
                    descriptor.SetValue(e.Control, DockStyle.None);
                }
                if ((this.designerHost != null) && !this.designerHost.Loading)
                {
                    SelectionManager service = (SelectionManager) this.GetService(typeof(SelectionManager));
                    if (service != null)
                    {
                        service.Refresh();
                    }
                }
            }
        }

        private void OnControlRemoved(object sender, ControlEventArgs e)
        {
            if (this.panel.Controls.Count == 0)
            {
                if (this.containerSelectorGlyph != null)
                {
                    this.containerSelectorGlyph.IsExpanded = false;
                }
                if ((this.designerHost != null) && !this.designerHost.Loading)
                {
                    SelectionManager service = (SelectionManager) this.GetService(typeof(SelectionManager));
                    if (service != null)
                    {
                        service.Refresh();
                    }
                }
            }
        }

        private void OnKeyShowDesignerActions(object sender, EventArgs e)
        {
            if (this.containerSelectorGlyph != null)
            {
                this.behavior.OnMouseDown(this.containerSelectorGlyph, MouseButtons.Left, Point.Empty);
            }
        }

        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            if ((!ToolStripDesignerUtils.DisplayInformation.TerminalServer && !ToolStripDesignerUtils.DisplayInformation.HighContrast) && !ToolStripDesignerUtils.DisplayInformation.LowResolution)
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(50, Color.White)))
                {
                    pe.Graphics.FillRectangle(brush, this.panel.ClientRectangle);
                }
            }
            this.DrawBorder(pe.Graphics);
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if (this.selectionSvc.PrimarySelection == this.panel)
            {
                this.designerShortCutCommand = new MenuCommand(new EventHandler(this.OnKeyShowDesignerActions), MenuCommands.KeyInvokeSmartTag);
                IMenuCommandService service = (IMenuCommandService) this.GetService(typeof(IMenuCommandService));
                if (service != null)
                {
                    this.oldShortCutCommand = service.FindCommand(MenuCommands.KeyInvokeSmartTag);
                    if (this.oldShortCutCommand != null)
                    {
                        service.RemoveCommand(this.oldShortCutCommand);
                    }
                    service.AddCommand(this.designerShortCutCommand);
                }
            }
        }

        private void OnSelectionChanging(object sender, EventArgs e)
        {
            if (this.designerShortCutCommand != null)
            {
                IMenuCommandService service = (IMenuCommandService) this.GetService(typeof(IMenuCommandService));
                if (service != null)
                {
                    service.RemoveCommand(this.designerShortCutCommand);
                    if (this.oldShortCutCommand != null)
                    {
                        service.AddCommand(this.oldShortCutCommand);
                    }
                }
                this.designerShortCutCommand = null;
            }
        }

        protected override void PreFilterEvents(IDictionary events)
        {
            base.PreFilterEvents(events);
            if (this.panel.Parent is ToolStripContainer)
            {
                string[] strArray = new string[] { 
                    "AutoSizeChanged", "BindingContextChanged", "CausesValidationChanged", "ChangeUICues", "DockChanged", "DragDrop", "DragEnter", "DragLeave", "DragOver", "EnabledChanged", "FontChanged", "ForeColorChanged", "GiveFeedback", "ImeModeChanged", "KeyDown", "KeyPress", 
                    "KeyUp", "LocationChanged", "MarginChanged", "MouseCaptureChanged", "Move", "QueryAccessibilityHelp", "QueryContinueDrag", "RegionChanged", "Scroll", "Validated", "Validating"
                 };
                for (int i = 0; i < strArray.Length; i++)
                {
                    EventDescriptor oldEventDescriptor = (EventDescriptor) events[strArray[i]];
                    if (oldEventDescriptor != null)
                    {
                        events[strArray[i]] = TypeDescriptor.CreateEvent(oldEventDescriptor.ComponentType, oldEventDescriptor, new Attribute[] { BrowsableAttribute.No });
                    }
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            PropertyDescriptor descriptor;
            base.PreFilterProperties(properties);
            if (this.panel.Parent is ToolStripContainer)
            {
                properties.Remove("Modifiers");
                properties.Remove("Locked");
                properties.Remove("GenerateMember");
                string[] strArray = new string[] { "Anchor", "AutoSize", "Dock", "DockPadding", "Height", "Location", "Name", "Orientation", "Renderer", "RowMargin", "Size", "Visible", "Width" };
                for (int j = 0; j < strArray.Length; j++)
                {
                    descriptor = (PropertyDescriptor) properties[strArray[j]];
                    if (descriptor != null)
                    {
                        properties[strArray[j]] = TypeDescriptor.CreateProperty(descriptor.ComponentType, descriptor, new Attribute[] { BrowsableAttribute.No, DesignerSerializationVisibilityAttribute.Hidden });
                    }
                }
            }
            string[] strArray2 = new string[] { "Padding", "Visible" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray2.Length; i++)
            {
                descriptor = (PropertyDescriptor) properties[strArray2[i]];
                if (descriptor != null)
                {
                    properties[strArray2[i]] = TypeDescriptor.CreateProperty(typeof(ToolStripPanelDesigner), descriptor, attributes);
                }
            }
        }

        private bool ShouldSerializePadding()
        {
            System.Windows.Forms.Padding padding = (System.Windows.Forms.Padding) base.ShadowProperties["Padding"];
            return !padding.Equals(_defaultPadding);
        }

        private bool ShouldSerializeVisible()
        {
            return !this.Visible;
        }

        private Pen BorderPen
        {
            get
            {
                return new Pen((this.Control.BackColor.GetBrightness() < 0.5) ? ControlPaint.Light(this.Control.BackColor) : ControlPaint.Dark(this.Control.BackColor)) { DashStyle = DashStyle.Dash };
            }
        }

        private ContextMenuStrip DesignerContextMenu
        {
            get
            {
                if (this.contextMenu == null)
                {
                    this.contextMenu = new BaseContextMenuStrip(base.Component.Site, base.Component as Component);
                    this.contextMenu.GroupOrdering.Clear();
                    this.contextMenu.GroupOrdering.AddRange(new string[] { "Code", "Verbs", "Custom", "Selection", "Edit", "Properties" });
                    this.contextMenu.Text = "CustomContextMenu";
                }
                return this.contextMenu;
            }
        }

        protected override System.ComponentModel.InheritanceAttribute InheritanceAttribute
        {
            get
            {
                if (((this.panel != null) && (this.panel.Parent is ToolStripContainer)) && (base.InheritanceAttribute == System.ComponentModel.InheritanceAttribute.Inherited))
                {
                    return System.ComponentModel.InheritanceAttribute.InheritedReadOnly;
                }
                return base.InheritanceAttribute;
            }
        }

        private System.Windows.Forms.Padding Padding
        {
            get
            {
                return (System.Windows.Forms.Padding) base.ShadowProperties["Padding"];
            }
            set
            {
                base.ShadowProperties["Padding"] = value;
            }
        }

        public override bool ParticipatesWithSnapLines
        {
            get
            {
                return false;
            }
        }

        public override System.Windows.Forms.Design.SelectionRules SelectionRules
        {
            get
            {
                System.Windows.Forms.Design.SelectionRules selectionRules = base.SelectionRules;
                if ((this.panel != null) && (this.panel.Parent is ToolStripContainer))
                {
                    selectionRules = System.Windows.Forms.Design.SelectionRules.None | System.Windows.Forms.Design.SelectionRules.Locked;
                }
                return selectionRules;
            }
        }

        public ToolStripPanelSelectionGlyph ToolStripPanelSelectorGlyph
        {
            get
            {
                return this.containerSelectorGlyph;
            }
        }

        private bool Visible
        {
            get
            {
                return (bool) base.ShadowProperties["Visible"];
            }
            set
            {
                base.ShadowProperties["Visible"] = value;
                this.panel.Visible = value;
            }
        }
    }
}

