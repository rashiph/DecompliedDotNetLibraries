namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class SplitContainerDesigner : ParentControlDesigner
    {
        private IDesignerHost designerHost;
        private bool disabledGlyphs;
        private bool disableDrawGrid;
        private int initialSplitterDist;
        private static int numberOfSplitterPanels = 2;
        private const string panel1Name = "Panel1";
        private const string panel2Name = "Panel2";
        private SplitterPanel selectedPanel;
        private SplitContainer splitContainer;
        private bool splitContainerSelected;
        private bool splitterDistanceException;
        private SplitterPanel splitterPanel1;
        private SplitterPanel splitterPanel2;

        public override bool CanParent(Control control)
        {
            return false;
        }

        private static SplitterPanel CheckIfPanelSelected(object comp)
        {
            return (comp as SplitterPanel);
        }

        protected override IComponent[] CreateToolCore(ToolboxItem tool, int x, int y, int width, int height, bool hasLocation, bool hasSize)
        {
            if (this.Selected == null)
            {
                this.Selected = this.splitterPanel1;
            }
            SplitterPanelDesigner toInvoke = (SplitterPanelDesigner) this.designerHost.GetDesigner(this.Selected);
            ParentControlDesigner.InvokeCreateTool(toInvoke, tool);
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
            if (service != null)
            {
                service.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
            }
            this.splitContainer.MouseDown -= new MouseEventHandler(this.OnSplitContainer);
            this.splitContainer.SplitterMoved -= new SplitterEventHandler(this.OnSplitterMoved);
            this.splitContainer.SplitterMoving -= new SplitterCancelEventHandler(this.OnSplitterMoving);
            this.splitContainer.DoubleClick -= new EventHandler(this.OnSplitContainerDoubleClick);
            base.Dispose(disposing);
        }

        protected override ControlBodyGlyph GetControlGlyph(GlyphSelectionType selectionType)
        {
            ControlBodyGlyph glyph = null;
            SelectionManager service = (SelectionManager) this.GetService(typeof(SelectionManager));
            if (service != null)
            {
                Rectangle bounds = base.BehaviorService.ControlRectInAdornerWindow(this.splitterPanel1);
                SplitterPanelDesigner designer = this.designerHost.GetDesigner(this.splitterPanel1) as SplitterPanelDesigner;
                this.OnSetCursor();
                if (designer != null)
                {
                    glyph = new ControlBodyGlyph(bounds, Cursor.Current, this.splitterPanel1, designer);
                    service.BodyGlyphAdorner.Glyphs.Add(glyph);
                }
                bounds = base.BehaviorService.ControlRectInAdornerWindow(this.splitterPanel2);
                designer = this.designerHost.GetDesigner(this.splitterPanel2) as SplitterPanelDesigner;
                if (designer != null)
                {
                    glyph = new ControlBodyGlyph(bounds, Cursor.Current, this.splitterPanel2, designer);
                    service.BodyGlyphAdorner.Glyphs.Add(glyph);
                }
            }
            return base.GetControlGlyph(selectionType);
        }

        protected override bool GetHitTest(Point point)
        {
            return ((this.InheritanceAttribute != InheritanceAttribute.InheritedReadOnly) && this.splitContainerSelected);
        }

        protected override Control GetParentForComponent(IComponent component)
        {
            return this.splitterPanel1;
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            base.AutoResizeHandles = true;
            this.splitContainer = component as SplitContainer;
            this.splitterPanel1 = this.splitContainer.Panel1;
            this.splitterPanel2 = this.splitContainer.Panel2;
            base.EnableDesignMode(this.splitContainer.Panel1, "Panel1");
            base.EnableDesignMode(this.splitContainer.Panel2, "Panel2");
            this.designerHost = (IDesignerHost) component.Site.GetService(typeof(IDesignerHost));
            if (this.selectedPanel == null)
            {
                this.Selected = this.splitterPanel1;
            }
            this.splitContainer.MouseDown += new MouseEventHandler(this.OnSplitContainer);
            this.splitContainer.SplitterMoved += new SplitterEventHandler(this.OnSplitterMoved);
            this.splitContainer.SplitterMoving += new SplitterCancelEventHandler(this.OnSplitterMoving);
            this.splitContainer.DoubleClick += new EventHandler(this.OnSplitContainerDoubleClick);
            ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
            if (service != null)
            {
                service.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            }
        }

        public override ControlDesigner InternalControlDesigner(int internalControlIndex)
        {
            SplitterPanel panel;
            switch (internalControlIndex)
            {
                case 0:
                    panel = this.splitterPanel1;
                    break;

                case 1:
                    panel = this.splitterPanel2;
                    break;

                default:
                    return null;
            }
            return (this.designerHost.GetDesigner(panel) as ControlDesigner);
        }

        public override int NumberOfInternalControlDesigners()
        {
            return numberOfSplitterPanels;
        }

        protected override void OnDragEnter(DragEventArgs de)
        {
            de.Effect = DragDropEffects.None;
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

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
            this.splitContainerSelected = false;
            if (service != null)
            {
                foreach (object obj2 in service.GetSelectedComponents())
                {
                    SplitterPanel panel = CheckIfPanelSelected(obj2);
                    if ((panel != null) && (panel.Parent == this.splitContainer))
                    {
                        this.splitContainerSelected = false;
                        this.Selected = panel;
                        break;
                    }
                    this.Selected = null;
                    if (obj2 == this.splitContainer)
                    {
                        this.splitContainerSelected = true;
                        break;
                    }
                }
            }
        }

        private void OnSplitContainer(object sender, MouseEventArgs e)
        {
            ((ISelectionService) this.GetService(typeof(ISelectionService))).SetSelectedComponents(new object[] { this.Control });
        }

        private void OnSplitContainerDoubleClick(object sender, EventArgs e)
        {
            if (this.splitContainerSelected)
            {
                try
                {
                    this.DoDefaultAction();
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                    base.DisplayError(exception);
                }
            }
        }

        private void OnSplitterMoved(object sender, SplitterEventArgs e)
        {
            if ((this.InheritanceAttribute != InheritanceAttribute.InheritedReadOnly) && !this.splitterDistanceException)
            {
                try
                {
                    base.RaiseComponentChanging(TypeDescriptor.GetProperties(this.splitContainer)["SplitterDistance"]);
                    base.RaiseComponentChanged(TypeDescriptor.GetProperties(this.splitContainer)["SplitterDistance"], null, null);
                    if (this.disabledGlyphs)
                    {
                        base.BehaviorService.EnableAllAdorners(true);
                        SelectionManager service = (SelectionManager) this.GetService(typeof(SelectionManager));
                        if (service != null)
                        {
                            service.Refresh();
                        }
                        this.disabledGlyphs = false;
                    }
                }
                catch (InvalidOperationException exception)
                {
                    ((IUIService) base.Component.Site.GetService(typeof(IUIService))).ShowError(exception.Message);
                }
                catch (CheckoutException exception2)
                {
                    if (exception2 == CheckoutException.Canceled)
                    {
                        try
                        {
                            this.splitterDistanceException = true;
                            this.splitContainer.SplitterDistance = this.initialSplitterDist;
                            return;
                        }
                        finally
                        {
                            this.splitterDistanceException = false;
                        }
                    }
                    throw;
                }
            }
        }

        private void OnSplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            this.initialSplitterDist = this.splitContainer.SplitterDistance;
            if (this.InheritanceAttribute != InheritanceAttribute.InheritedReadOnly)
            {
                this.disabledGlyphs = true;
                Adorner bodyGlyphAdorner = null;
                SelectionManager service = (SelectionManager) this.GetService(typeof(SelectionManager));
                if (service != null)
                {
                    bodyGlyphAdorner = service.BodyGlyphAdorner;
                }
                foreach (Adorner adorner2 in base.BehaviorService.Adorners)
                {
                    if ((bodyGlyphAdorner == null) || !adorner2.Equals(bodyGlyphAdorner))
                    {
                        adorner2.EnabledInternal = false;
                    }
                }
                base.BehaviorService.Invalidate();
                ArrayList list = new ArrayList();
                foreach (ControlBodyGlyph glyph in bodyGlyphAdorner.Glyphs)
                {
                    if (!(glyph.RelatedComponent is SplitterPanel))
                    {
                        list.Add(glyph);
                    }
                }
                foreach (Glyph glyph2 in list)
                {
                    bodyGlyphAdorner.Glyphs.Remove(glyph2);
                }
            }
        }

        internal void SplitterPanelHover()
        {
            this.OnMouseHover();
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                OrientationActionList list = new OrientationActionList(this);
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
                foreach (SplitterPanel panel in this.splitContainer.Controls)
                {
                    foreach (Control control in panel.Controls)
                    {
                        list.Add(control);
                    }
                }
                return list;
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

        internal SplitterPanel Selected
        {
            get
            {
                return this.selectedPanel;
            }
            set
            {
                if (this.selectedPanel != null)
                {
                    SplitterPanelDesigner designer = (SplitterPanelDesigner) this.designerHost.GetDesigner(this.selectedPanel);
                    designer.Selected = false;
                }
                if (value != null)
                {
                    SplitterPanelDesigner designer2 = (SplitterPanelDesigner) this.designerHost.GetDesigner(value);
                    this.selectedPanel = value;
                    designer2.Selected = true;
                }
                else if (this.selectedPanel != null)
                {
                    SplitterPanelDesigner designer3 = (SplitterPanelDesigner) this.designerHost.GetDesigner(this.selectedPanel);
                    this.selectedPanel = null;
                    designer3.Selected = false;
                }
            }
        }

        public override IList SnapLines
        {
            get
            {
                return (base.SnapLinesInternal() as ArrayList);
            }
        }

        private class OrientationActionList : DesignerActionList
        {
            private string actionName;
            private SplitContainerDesigner owner;
            private Component ownerComponent;

            public OrientationActionList(SplitContainerDesigner owner) : base(owner.Component)
            {
                this.owner = owner;
                this.ownerComponent = owner.Component as Component;
                if (this.ownerComponent != null)
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.ownerComponent)["Orientation"];
                    if (descriptor != null)
                    {
                        bool flag = ((Orientation) descriptor.GetValue(this.ownerComponent)) == Orientation.Horizontal;
                        this.actionName = flag ? System.Design.SR.GetString("DesignerShortcutVerticalOrientation") : System.Design.SR.GetString("DesignerShortcutHorizontalOrientation");
                    }
                }
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                items.Add(new DesignerActionVerbItem(new DesignerVerb(this.actionName, new EventHandler(this.OnOrientationActionClick))));
                return items;
            }

            private void OnOrientationActionClick(object sender, EventArgs e)
            {
                DesignerVerb verb = sender as DesignerVerb;
                if (verb != null)
                {
                    Orientation orientation = verb.Text.Equals(System.Design.SR.GetString("DesignerShortcutHorizontalOrientation")) ? Orientation.Horizontal : Orientation.Vertical;
                    this.actionName = (orientation == Orientation.Horizontal) ? System.Design.SR.GetString("DesignerShortcutVerticalOrientation") : System.Design.SR.GetString("DesignerShortcutHorizontalOrientation");
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.ownerComponent)["Orientation"];
                    if ((descriptor != null) && (((Orientation) descriptor.GetValue(this.ownerComponent)) != orientation))
                    {
                        descriptor.SetValue(this.ownerComponent, orientation);
                    }
                    DesignerActionUIService service = (DesignerActionUIService) this.owner.GetService(typeof(DesignerActionUIService));
                    if (service != null)
                    {
                        service.Refresh(this.ownerComponent);
                    }
                }
            }
        }
    }
}

