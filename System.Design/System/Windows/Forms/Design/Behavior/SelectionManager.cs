namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal sealed class SelectionManager : IDisposable
    {
        private BehaviorService behaviorService;
        private Adorner bodyAdorner;
        private Hashtable componentToDesigner;
        private int curCompIndex;
        private Rectangle[] curSelectionBounds;
        private DesignerActionUI designerActionUI;
        private IDesignerHost designerHost;
        private bool needRefresh;
        private object prevPrimarySelection = null;
        private Rectangle[] prevSelectionBounds = null;
        private Control rootComponent;
        private Adorner selectionAdorner;
        private bool selectionChanging;
        private ISelectionService selSvc;
        private IServiceProvider serviceProvider;

        public SelectionManager(IServiceProvider serviceProvider, BehaviorService behaviorService)
        {
            this.behaviorService = behaviorService;
            this.serviceProvider = serviceProvider;
            this.selSvc = (ISelectionService) serviceProvider.GetService(typeof(ISelectionService));
            this.designerHost = (IDesignerHost) serviceProvider.GetService(typeof(IDesignerHost));
            if (this.designerHost != null)
            {
                ISelectionService selSvc = this.selSvc;
            }
            behaviorService.BeginDrag += new BehaviorDragDropEventHandler(this.OnBeginDrag);
            behaviorService.Synchronize += new EventHandler(this.OnSynchronize);
            this.selSvc.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            this.rootComponent = (Control) this.designerHost.RootComponent;
            this.selectionAdorner = new Adorner();
            this.bodyAdorner = new Adorner();
            behaviorService.Adorners.Add(this.bodyAdorner);
            behaviorService.Adorners.Add(this.selectionAdorner);
            this.componentToDesigner = new Hashtable();
            IComponentChangeService service = (IComponentChangeService) serviceProvider.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.ComponentAdded += new ComponentEventHandler(this.OnComponentAdded);
                service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                service.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
            }
            this.designerHost.TransactionClosed += new DesignerTransactionCloseEventHandler(this.OnTransactionClosed);
            DesignerOptionService service2 = this.designerHost.GetService(typeof(DesignerOptionService)) as DesignerOptionService;
            if (service2 != null)
            {
                PropertyDescriptor descriptor = service2.Options.Properties["UseSmartTags"];
                if (((descriptor != null) && (descriptor.PropertyType == typeof(bool))) && ((bool) descriptor.GetValue(null)))
                {
                    this.designerActionUI = new DesignerActionUI(serviceProvider, this.selectionAdorner);
                    behaviorService.DesignerActionUI = this.designerActionUI;
                }
            }
        }

        private void AddAllControlGlyphs(Control parent, ArrayList selComps, object primarySelection)
        {
            foreach (Control control in parent.Controls)
            {
                this.AddAllControlGlyphs(control, selComps, primarySelection);
            }
            GlyphSelectionType notSelected = GlyphSelectionType.NotSelected;
            if (selComps.Contains(parent))
            {
                if (parent.Equals(primarySelection))
                {
                    notSelected = GlyphSelectionType.SelectedPrimary;
                }
                else
                {
                    notSelected = GlyphSelectionType.Selected;
                }
            }
            this.AddControlGlyphs(parent, notSelected);
        }

        private void AddControlGlyphs(Control c, GlyphSelectionType selType)
        {
            ControlDesigner designer = (ControlDesigner) this.componentToDesigner[c];
            if (designer != null)
            {
                ControlBodyGlyph controlGlyphInternal = designer.GetControlGlyphInternal(selType);
                if (controlGlyphInternal != null)
                {
                    this.bodyAdorner.Glyphs.Add(controlGlyphInternal);
                    if ((selType == GlyphSelectionType.SelectedPrimary) || (selType == GlyphSelectionType.Selected))
                    {
                        if (this.curSelectionBounds[this.curCompIndex] == Rectangle.Empty)
                        {
                            this.curSelectionBounds[this.curCompIndex] = controlGlyphInternal.Bounds;
                        }
                        else
                        {
                            this.curSelectionBounds[this.curCompIndex] = Rectangle.Union(this.curSelectionBounds[this.curCompIndex], controlGlyphInternal.Bounds);
                        }
                    }
                }
                GlyphCollection glyphs = designer.GetGlyphs(selType);
                if (glyphs != null)
                {
                    this.selectionAdorner.Glyphs.AddRange(glyphs);
                    if ((selType == GlyphSelectionType.SelectedPrimary) || (selType == GlyphSelectionType.Selected))
                    {
                        foreach (Glyph glyph2 in glyphs)
                        {
                            this.curSelectionBounds[this.curCompIndex] = Rectangle.Union(this.curSelectionBounds[this.curCompIndex], glyph2.Bounds);
                        }
                    }
                }
            }
            if ((selType == GlyphSelectionType.SelectedPrimary) || (selType == GlyphSelectionType.Selected))
            {
                this.curCompIndex++;
            }
        }

        private Region DetermineRegionToRefresh(object primarySelection)
        {
            Rectangle[] curSelectionBounds;
            Rectangle[] prevSelectionBounds;
            Region region = new Region(Rectangle.Empty);
            if (this.curSelectionBounds.Length >= this.prevSelectionBounds.Length)
            {
                curSelectionBounds = this.curSelectionBounds;
                prevSelectionBounds = this.prevSelectionBounds;
            }
            else
            {
                curSelectionBounds = this.prevSelectionBounds;
                prevSelectionBounds = this.curSelectionBounds;
            }
            bool[] flagArray = new bool[prevSelectionBounds.Length];
            for (int i = 0; i < prevSelectionBounds.Length; i++)
            {
                flagArray[i] = false;
            }
            for (int j = 0; j < curSelectionBounds.Length; j++)
            {
                bool flag = false;
                Rectangle rect = curSelectionBounds[j];
                for (int m = 0; m < prevSelectionBounds.Length; m++)
                {
                    if (rect.IntersectsWith(prevSelectionBounds[m]))
                    {
                        Rectangle rectangle2 = prevSelectionBounds[m];
                        flag = true;
                        if (rect != rectangle2)
                        {
                            region.Union(rect);
                            region.Union(rectangle2);
                        }
                        flagArray[m] = true;
                        break;
                    }
                }
                if (!flag)
                {
                    region.Union(rect);
                }
            }
            for (int k = 0; k < flagArray.Length; k++)
            {
                if (!flagArray[k])
                {
                    region.Union(prevSelectionBounds[k]);
                }
            }
            using (Graphics graphics = this.behaviorService.AdornerWindowGraphics)
            {
                if ((!region.IsEmpty(graphics) || (primarySelection == null)) || primarySelection.Equals(this.prevPrimarySelection))
                {
                    return region;
                }
                for (int n = 0; n < this.curSelectionBounds.Length; n++)
                {
                    region.Union(this.curSelectionBounds[n]);
                }
            }
            return region;
        }

        public void Dispose()
        {
            if (this.designerHost != null)
            {
                this.designerHost.TransactionClosed -= new DesignerTransactionCloseEventHandler(this.OnTransactionClosed);
                this.designerHost = null;
            }
            if (this.serviceProvider != null)
            {
                IComponentChangeService service = (IComponentChangeService) this.serviceProvider.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentAdded -= new ComponentEventHandler(this.OnComponentAdded);
                    service.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                    service.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                }
                if (this.selSvc != null)
                {
                    this.selSvc.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
                    this.selSvc = null;
                }
                this.serviceProvider = null;
            }
            if (this.behaviorService != null)
            {
                this.behaviorService.Adorners.Remove(this.bodyAdorner);
                this.behaviorService.Adorners.Remove(this.selectionAdorner);
                this.behaviorService.BeginDrag -= new BehaviorDragDropEventHandler(this.OnBeginDrag);
                this.behaviorService.Synchronize -= new EventHandler(this.OnSynchronize);
                this.behaviorService = null;
            }
            if (this.selectionAdorner != null)
            {
                this.selectionAdorner.Glyphs.Clear();
                this.selectionAdorner = null;
            }
            if (this.bodyAdorner != null)
            {
                this.bodyAdorner.Glyphs.Clear();
                this.bodyAdorner = null;
            }
            if (this.designerActionUI != null)
            {
                this.designerActionUI.Dispose();
                this.designerActionUI = null;
            }
        }

        internal void OnBeginDrag(BehaviorDragDropEventArgs e)
        {
            this.OnBeginDrag(null, e);
        }

        private void OnBeginDrag(object source, BehaviorDragDropEventArgs e)
        {
            ArrayList list = new ArrayList(e.DragComponents);
            ArrayList list2 = new ArrayList();
            foreach (ControlBodyGlyph glyph in this.bodyAdorner.Glyphs)
            {
                if ((glyph.RelatedComponent is Control) && (list.Contains(glyph.RelatedComponent) || !((Control) glyph.RelatedComponent).AllowDrop))
                {
                    list2.Add(glyph);
                }
            }
            foreach (Glyph glyph2 in list2)
            {
                this.bodyAdorner.Glyphs.Remove(glyph2);
            }
        }

        private void OnComponentAdded(object source, ComponentEventArgs ce)
        {
            IComponent component = ce.Component;
            IDesigner designer = this.designerHost.GetDesigner(component);
            if (designer is ControlDesigner)
            {
                this.componentToDesigner.Add(component, designer);
            }
        }

        private void OnComponentChanged(object source, ComponentChangedEventArgs ce)
        {
            if (this.selSvc.GetComponentSelected(ce.Component))
            {
                if (!this.designerHost.InTransaction)
                {
                    this.Refresh();
                }
                else
                {
                    this.NeedRefresh = true;
                }
            }
        }

        private void OnComponentRemoved(object source, ComponentEventArgs ce)
        {
            if (this.componentToDesigner.Contains(ce.Component))
            {
                this.componentToDesigner.Remove(ce.Component);
            }
            if (this.designerActionUI != null)
            {
                this.designerActionUI.RemoveActionGlyph(ce.Component);
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if (this.selectionChanging)
            {
                return;
            }
            this.selectionChanging = true;
            this.selectionAdorner.Glyphs.Clear();
            this.bodyAdorner.Glyphs.Clear();
            ArrayList selComps = new ArrayList(this.selSvc.GetSelectedComponents());
            object primarySelection = this.selSvc.PrimarySelection;
            this.curCompIndex = 0;
            this.curSelectionBounds = new Rectangle[selComps.Count];
            this.AddAllControlGlyphs(this.rootComponent, selComps, primarySelection);
            if (this.prevSelectionBounds != null)
            {
                Region region = this.DetermineRegionToRefresh(primarySelection);
                using (Graphics graphics = this.behaviorService.AdornerWindowGraphics)
                {
                    if (!region.IsEmpty(graphics))
                    {
                        this.selectionAdorner.Invalidate(region);
                    }
                    goto Label_012D;
                }
            }
            if (this.curSelectionBounds.Length > 0)
            {
                Rectangle a = this.curSelectionBounds[0];
                for (int i = 1; i < this.curSelectionBounds.Length; i++)
                {
                    a = Rectangle.Union(a, this.curSelectionBounds[i]);
                }
                if (a != Rectangle.Empty)
                {
                    this.selectionAdorner.Invalidate(a);
                }
            }
            else
            {
                this.selectionAdorner.Invalidate();
            }
        Label_012D:
            this.prevPrimarySelection = primarySelection;
            if (this.curSelectionBounds.Length > 0)
            {
                this.prevSelectionBounds = new Rectangle[this.curSelectionBounds.Length];
                Array.Copy(this.curSelectionBounds, this.prevSelectionBounds, this.curSelectionBounds.Length);
            }
            else
            {
                this.prevSelectionBounds = null;
            }
            this.selectionChanging = false;
        }

        private void OnSynchronize(object sender, EventArgs e)
        {
            this.Refresh();
        }

        private void OnTransactionClosed(object sender, DesignerTransactionCloseEventArgs e)
        {
            if (e.LastTransaction && this.NeedRefresh)
            {
                this.Refresh();
            }
        }

        public void Refresh()
        {
            this.NeedRefresh = false;
            this.OnSelectionChanged(this, null);
        }

        internal Adorner BodyGlyphAdorner
        {
            get
            {
                return this.bodyAdorner;
            }
        }

        internal bool NeedRefresh
        {
            get
            {
                return this.needRefresh;
            }
            set
            {
                this.needRefresh = value;
            }
        }

        internal Adorner SelectionGlyphAdorner
        {
            get
            {
                return this.selectionAdorner;
            }
        }
    }
}

