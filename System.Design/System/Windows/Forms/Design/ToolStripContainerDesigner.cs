namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class ToolStripContainerDesigner : ParentControlDesigner
    {
        private ToolStripPanel bottomToolStripPanel;
        private const string bottomToolStripPanelName = "BottomToolStripPanel";
        private ToolStripContentPanel contentToolStripPanel;
        private const string contentToolStripPanelName = "ContentPanel";
        private IDesignerHost designerHost;
        private bool disableDrawGrid;
        private ToolStripPanel leftToolStripPanel;
        private const string leftToolStripPanelName = "LeftToolStripPanel";
        private Control[] panels;
        private ToolStripPanel rightToolStripPanel;
        private const string rightToolStripPanelName = "RightToolStripPanel";
        private ISelectionService selectionSvc;
        private ToolStripContainer toolStripContainer;
        private ToolStripPanel topToolStripPanel;
        private const string topToolStripPanelName = "TopToolStripPanel";

        private void AddPanelSelectionGlyph(ToolStripPanelDesigner designer, SelectionManager selMgr)
        {
            if (designer != null)
            {
                Glyph childGlyph = designer.GetGlyph();
                if (childGlyph != null)
                {
                    foreach (object obj2 in this.selectionSvc.GetSelectedComponents())
                    {
                        Component c = obj2 as Component;
                        if ((c != null) && !this.CheckAssociatedControl(c, childGlyph, selMgr.BodyGlyphAdorner.Glyphs))
                        {
                            selMgr.BodyGlyphAdorner.Glyphs.Insert(0, childGlyph);
                        }
                    }
                }
            }
        }

        public override bool CanParent(Control control)
        {
            return false;
        }

        private bool CheckAssociatedControl(Component c, Glyph childGlyph, GlyphCollection glyphs)
        {
            bool flag = false;
            ToolStripDropDownItem dropDownItem = c as ToolStripDropDownItem;
            if (dropDownItem != null)
            {
                flag = this.CheckDropDownBounds(dropDownItem, childGlyph, glyphs);
            }
            if (flag)
            {
                return flag;
            }
            Control associatedControl = this.GetAssociatedControl(c);
            if (((associatedControl == null) || (associatedControl == this.toolStripContainer)) || System.Design.UnsafeNativeMethods.IsChild(new HandleRef(this.toolStripContainer, this.toolStripContainer.Handle), new HandleRef(associatedControl, associatedControl.Handle)))
            {
                return flag;
            }
            Rectangle bounds = childGlyph.Bounds;
            Rectangle rect = base.BehaviorService.ControlRectInAdornerWindow(associatedControl);
            if ((c == this.designerHost.RootComponent) || !bounds.IntersectsWith(rect))
            {
                glyphs.Insert(0, childGlyph);
            }
            return true;
        }

        private bool CheckDropDownBounds(ToolStripDropDownItem dropDownItem, Glyph childGlyph, GlyphCollection glyphs)
        {
            if (dropDownItem == null)
            {
                return false;
            }
            Rectangle bounds = childGlyph.Bounds;
            Rectangle rect = base.BehaviorService.ControlRectInAdornerWindow(dropDownItem.DropDown);
            if (!bounds.IntersectsWith(rect))
            {
                glyphs.Insert(0, childGlyph);
            }
            return true;
        }

        private ToolStripContainer ContainerParent(Control c)
        {
            if ((c != null) && !(c is ToolStripContainer))
            {
                while (c.Parent != null)
                {
                    if (c.Parent is ToolStripContainer)
                    {
                        return (c.Parent as ToolStripContainer);
                    }
                    c = c.Parent;
                }
            }
            return null;
        }

        protected override IComponent[] CreateToolCore(ToolboxItem tool, int x, int y, int width, int height, bool hasLocation, bool hasSize)
        {
            if (tool != null)
            {
                System.Type c = tool.GetType(this.designerHost);
                if (typeof(StatusStrip).IsAssignableFrom(c))
                {
                    ParentControlDesigner.InvokeCreateTool(this.GetDesigner(this.bottomToolStripPanel), tool);
                }
                else if (typeof(ToolStrip).IsAssignableFrom(c))
                {
                    ParentControlDesigner.InvokeCreateTool(this.GetDesigner(this.topToolStripPanel), tool);
                }
                else
                {
                    ParentControlDesigner.InvokeCreateTool(this.GetDesigner(this.contentToolStripPanel), tool);
                }
            }
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.selectionSvc != null)
            {
                this.selectionSvc = null;
            }
        }

        private Control GetAssociatedControl(Component c)
        {
            if (c is Control)
            {
                return (c as Control);
            }
            if (!(c is ToolStripItem))
            {
                return null;
            }
            ToolStripItem item = c as ToolStripItem;
            Control currentParent = item.GetCurrentParent();
            if (currentParent == null)
            {
                currentParent = item.Owner;
            }
            return currentParent;
        }

        protected override ControlBodyGlyph GetControlGlyph(GlyphSelectionType selectionType)
        {
            SelectionManager service = (SelectionManager) this.GetService(typeof(SelectionManager));
            if (service != null)
            {
                for (int i = 0; i <= 4; i++)
                {
                    Control c = this.panels[i];
                    Rectangle bounds = base.BehaviorService.ControlRectInAdornerWindow(c);
                    ControlDesigner designer = this.InternalControlDesigner(i);
                    this.OnSetCursor();
                    if (designer != null)
                    {
                        ControlBodyGlyph glyph = new ControlBodyGlyph(bounds, Cursor.Current, c, designer);
                        service.BodyGlyphAdorner.Glyphs.Add(glyph);
                        bool flag = true;
                        ICollection selectedComponents = this.selectionSvc.GetSelectedComponents();
                        if (!this.selectionSvc.GetComponentSelected(this.toolStripContainer))
                        {
                            foreach (object obj2 in selectedComponents)
                            {
                                if (this.ContainerParent(obj2 as Control) == this.toolStripContainer)
                                {
                                    flag = true;
                                }
                                else
                                {
                                    flag = false;
                                }
                            }
                        }
                        if (flag)
                        {
                            ToolStripPanelDesigner designer2 = designer as ToolStripPanelDesigner;
                            if (designer2 != null)
                            {
                                this.AddPanelSelectionGlyph(designer2, service);
                            }
                        }
                    }
                }
            }
            return base.GetControlGlyph(selectionType);
        }

        private PanelDesigner GetDesigner(ToolStripContentPanel panel)
        {
            return (this.designerHost.GetDesigner(panel) as PanelDesigner);
        }

        private ToolStripPanelDesigner GetDesigner(ToolStripPanel panel)
        {
            return (this.designerHost.GetDesigner(panel) as ToolStripPanelDesigner);
        }

        protected override Control GetParentForComponent(IComponent component)
        {
            System.Type c = component.GetType();
            if (typeof(StatusStrip).IsAssignableFrom(c))
            {
                return this.bottomToolStripPanel;
            }
            if (typeof(ToolStrip).IsAssignableFrom(c))
            {
                return this.topToolStripPanel;
            }
            return this.contentToolStripPanel;
        }

        public override void Initialize(IComponent component)
        {
            this.toolStripContainer = (ToolStripContainer) component;
            base.Initialize(component);
            base.AutoResizeHandles = true;
            this.topToolStripPanel = this.toolStripContainer.TopToolStripPanel;
            this.bottomToolStripPanel = this.toolStripContainer.BottomToolStripPanel;
            this.leftToolStripPanel = this.toolStripContainer.LeftToolStripPanel;
            this.rightToolStripPanel = this.toolStripContainer.RightToolStripPanel;
            this.contentToolStripPanel = this.toolStripContainer.ContentPanel;
            this.panels = new Control[] { this.contentToolStripPanel, this.leftToolStripPanel, this.rightToolStripPanel, this.topToolStripPanel, this.bottomToolStripPanel };
            ToolboxBitmapAttribute attribute = new ToolboxBitmapAttribute(typeof(ToolStripPanel), "ToolStripContainer_BottomToolStripPanel.bmp");
            ToolboxBitmapAttribute attribute2 = new ToolboxBitmapAttribute(typeof(ToolStripPanel), "ToolStripContainer_RightToolStripPanel.bmp");
            ToolboxBitmapAttribute attribute3 = new ToolboxBitmapAttribute(typeof(ToolStripPanel), "ToolStripContainer_TopToolStripPanel.bmp");
            ToolboxBitmapAttribute attribute4 = new ToolboxBitmapAttribute(typeof(ToolStripPanel), "ToolStripContainer_LeftToolStripPanel.bmp");
            TypeDescriptor.AddAttributes(this.bottomToolStripPanel, new Attribute[] { attribute, new DescriptionAttribute("bottom") });
            TypeDescriptor.AddAttributes(this.rightToolStripPanel, new Attribute[] { attribute2, new DescriptionAttribute("right") });
            TypeDescriptor.AddAttributes(this.leftToolStripPanel, new Attribute[] { attribute4, new DescriptionAttribute("left") });
            TypeDescriptor.AddAttributes(this.topToolStripPanel, new Attribute[] { attribute3, new DescriptionAttribute("top") });
            base.EnableDesignMode(this.topToolStripPanel, "TopToolStripPanel");
            base.EnableDesignMode(this.bottomToolStripPanel, "BottomToolStripPanel");
            base.EnableDesignMode(this.leftToolStripPanel, "LeftToolStripPanel");
            base.EnableDesignMode(this.rightToolStripPanel, "RightToolStripPanel");
            base.EnableDesignMode(this.contentToolStripPanel, "ContentPanel");
            this.designerHost = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (this.selectionSvc == null)
            {
                this.selectionSvc = (ISelectionService) this.GetService(typeof(ISelectionService));
            }
            if (this.topToolStripPanel != null)
            {
                (this.designerHost.GetDesigner(this.topToolStripPanel) as ToolStripPanelDesigner).ExpandTopPanel();
            }
            this.TopToolStripPanelVisible = this.toolStripContainer.TopToolStripPanelVisible;
            this.LeftToolStripPanelVisible = this.toolStripContainer.LeftToolStripPanelVisible;
            this.RightToolStripPanelVisible = this.toolStripContainer.RightToolStripPanelVisible;
            this.BottomToolStripPanelVisible = this.toolStripContainer.BottomToolStripPanelVisible;
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);
        }

        public override ControlDesigner InternalControlDesigner(int internalControlIndex)
        {
            if ((internalControlIndex < this.panels.Length) && (internalControlIndex >= 0))
            {
                Control component = this.panels[internalControlIndex];
                return (this.designerHost.GetDesigner(component) as ControlDesigner);
            }
            return null;
        }

        public override int NumberOfInternalControlDesigners()
        {
            return this.panels.Length;
        }

        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            try
            {
                this.disableDrawGrid = true;
                base.OnPaintAdornments(pe);
            }
            finally
            {
                this.disableDrawGrid = false;
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "TopToolStripPanelVisible", "LeftToolStripPanelVisible", "RightToolStripPanelVisible", "BottomToolStripPanelVisible" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(ToolStripContainerDesigner), oldPropertyDescriptor, attributes);
                }
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                ToolStripContainerActionList list = new ToolStripContainerActionList(this.toolStripContainer) {
                    AutoShow = true
                };
                lists.Add(list);
                return lists;
            }
        }

        protected override bool AllowControlLasso
        {
            get
            {
                return false;
            }
        }

        public override ICollection AssociatedComponents
        {
            get
            {
                ArrayList list = new ArrayList();
                foreach (Control control in this.toolStripContainer.Controls)
                {
                    foreach (Control control2 in control.Controls)
                    {
                        list.Add(control2);
                    }
                }
                return list;
            }
        }

        private bool BottomToolStripPanelVisible
        {
            get
            {
                return (bool) base.ShadowProperties["BottomToolStripPanelVisible"];
            }
            set
            {
                base.ShadowProperties["BottomToolStripPanelVisible"] = value;
                ((ToolStripContainer) base.Component).BottomToolStripPanelVisible = value;
            }
        }

        protected override bool DrawGrid
        {
            get
            {
                if (this.disableDrawGrid)
                {
                    return false;
                }
                return base.DrawGrid;
            }
        }

        private bool LeftToolStripPanelVisible
        {
            get
            {
                return (bool) base.ShadowProperties["LeftToolStripPanelVisible"];
            }
            set
            {
                base.ShadowProperties["LeftToolStripPanelVisible"] = value;
                ((ToolStripContainer) base.Component).LeftToolStripPanelVisible = value;
            }
        }

        private bool RightToolStripPanelVisible
        {
            get
            {
                return (bool) base.ShadowProperties["RightToolStripPanelVisible"];
            }
            set
            {
                base.ShadowProperties["RightToolStripPanelVisible"] = value;
                ((ToolStripContainer) base.Component).RightToolStripPanelVisible = value;
            }
        }

        public override IList SnapLines
        {
            get
            {
                return (base.SnapLinesInternal() as ArrayList);
            }
        }

        private bool TopToolStripPanelVisible
        {
            get
            {
                return (bool) base.ShadowProperties["TopToolStripPanelVisible"];
            }
            set
            {
                base.ShadowProperties["TopToolStripPanelVisible"] = value;
                ((ToolStripContainer) base.Component).TopToolStripPanelVisible = value;
            }
        }
    }
}

