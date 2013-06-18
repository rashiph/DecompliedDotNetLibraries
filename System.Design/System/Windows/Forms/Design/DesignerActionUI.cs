namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class DesignerActionUI : IDisposable
    {
        private BehaviorService behaviorService;
        private bool cancelClose;
        private MenuCommand cmdShowDesignerActions;
        private Hashtable componentToGlyph;
        private DesignerActionKeyboardBehavior dapkb;
        private static TraceSwitch DesigneActionPanelTraceSwitch = new TraceSwitch("DesigneActionPanelTrace", "DesignerActionPanel tracing");
        private Adorner designerActionAdorner;
        internal DesignerActionToolStripDropDown designerActionHost;
        private DesignerActionService designerActionService;
        private DesignerActionUIService designerActionUIService;
        private bool disposeActionService;
        private bool disposeActionUIService;
        internal static readonly TraceSwitch DropDownVisibilityDebug;
        private bool inTransaction;
        private IComponent lastPanelComponent;
        private IWin32Window mainParentWindow;
        private Control marshalingControl;
        private IMenuCommandService menuCommandService;
        private IComponent relatedComponentTransaction;
        private DesignerActionGlyph relatedGlyphTransaction;
        private ISelectionService selSvc;
        private IServiceProvider serviceProvider;
        private IUIService uiService;

        public DesignerActionUI(IServiceProvider serviceProvider, Adorner containerAdorner)
        {
            this.serviceProvider = serviceProvider;
            this.designerActionAdorner = containerAdorner;
            this.behaviorService = (BehaviorService) serviceProvider.GetService(typeof(BehaviorService));
            this.menuCommandService = (IMenuCommandService) serviceProvider.GetService(typeof(IMenuCommandService));
            this.selSvc = (ISelectionService) serviceProvider.GetService(typeof(ISelectionService));
            if ((this.behaviorService != null) && (this.selSvc != null))
            {
                this.designerActionService = (DesignerActionService) serviceProvider.GetService(typeof(DesignerActionService));
                if (this.designerActionService == null)
                {
                    this.designerActionService = new DesignerActionService(serviceProvider);
                    this.disposeActionService = true;
                }
                this.designerActionUIService = (DesignerActionUIService) serviceProvider.GetService(typeof(DesignerActionUIService));
                if (this.designerActionUIService == null)
                {
                    this.designerActionUIService = new DesignerActionUIService(serviceProvider);
                    this.disposeActionUIService = true;
                }
                this.designerActionUIService.DesignerActionUIStateChange += new DesignerActionUIStateChangeEventHandler(this.OnDesignerActionUIStateChange);
                this.designerActionService.DesignerActionListsChanged += new DesignerActionListsChangedEventHandler(this.OnDesignerActionsChanged);
                this.lastPanelComponent = null;
                IComponentChangeService service = (IComponentChangeService) serviceProvider.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
                }
                if (this.menuCommandService != null)
                {
                    this.cmdShowDesignerActions = new MenuCommand(new EventHandler(this.OnKeyShowDesignerActions), MenuCommands.KeyInvokeSmartTag);
                    this.menuCommandService.AddCommand(this.cmdShowDesignerActions);
                }
                this.uiService = (IUIService) serviceProvider.GetService(typeof(IUIService));
                if (this.uiService != null)
                {
                    this.mainParentWindow = this.uiService.GetDialogOwnerWindow();
                }
                this.componentToGlyph = new Hashtable();
                this.marshalingControl = new Control();
                this.marshalingControl.CreateControl();
            }
        }

        private void DesignerTransactionClosed(object sender, DesignerTransactionCloseEventArgs e)
        {
            if (e.LastTransaction && (this.relatedComponentTransaction != null))
            {
                this.inTransaction = false;
                IDesignerHost service = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
                service.TransactionClosed -= new DesignerTransactionCloseEventHandler(this.DesignerTransactionClosed);
                this.RecreateInternal(this.relatedComponentTransaction);
                this.relatedComponentTransaction = null;
            }
        }

        public void Dispose()
        {
            if (this.marshalingControl != null)
            {
                this.marshalingControl.Dispose();
                this.marshalingControl = null;
            }
            if (this.serviceProvider != null)
            {
                IComponentChangeService service = (IComponentChangeService) this.serviceProvider.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                }
                if (this.cmdShowDesignerActions != null)
                {
                    IMenuCommandService service2 = (IMenuCommandService) this.serviceProvider.GetService(typeof(IMenuCommandService));
                    if (service2 != null)
                    {
                        service2.RemoveCommand(this.cmdShowDesignerActions);
                    }
                }
            }
            this.serviceProvider = null;
            this.behaviorService = null;
            this.selSvc = null;
            if (this.designerActionService != null)
            {
                this.designerActionService.DesignerActionListsChanged -= new DesignerActionListsChangedEventHandler(this.OnDesignerActionsChanged);
                if (this.disposeActionService)
                {
                    this.designerActionService.Dispose();
                }
            }
            this.designerActionService = null;
            if (this.designerActionUIService != null)
            {
                this.designerActionUIService.DesignerActionUIStateChange -= new DesignerActionUIStateChangeEventHandler(this.OnDesignerActionUIStateChange);
                if (this.disposeActionUIService)
                {
                    this.designerActionUIService.Dispose();
                }
            }
            this.designerActionUIService = null;
            this.designerActionAdorner = null;
        }

        public DesignerActionGlyph GetDesignerActionGlyph(IComponent comp)
        {
            return this.GetDesignerActionGlyph(comp, null);
        }

        internal DesignerActionGlyph GetDesignerActionGlyph(IComponent comp, DesignerActionListCollection dalColl)
        {
            InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(comp)[typeof(InheritanceAttribute)];
            if (attribute != InheritanceAttribute.InheritedReadOnly)
            {
                if (dalColl == null)
                {
                    dalColl = this.designerActionService.GetComponentActions(comp);
                }
                if ((dalColl != null) && (dalColl.Count > 0))
                {
                    DesignerActionGlyph glyph = null;
                    if (this.componentToGlyph[comp] == null)
                    {
                        DesignerActionBehavior behavior = new DesignerActionBehavior(this.serviceProvider, comp, dalColl, this);
                        if (!(comp is Control) || (comp is ToolStripDropDown))
                        {
                            ComponentTray service = this.serviceProvider.GetService(typeof(ComponentTray)) as ComponentTray;
                            if (service != null)
                            {
                                ComponentTray.TrayControl trayControlFromComponent = service.GetTrayControlFromComponent(comp);
                                if (trayControlFromComponent != null)
                                {
                                    Rectangle bounds = trayControlFromComponent.Bounds;
                                    glyph = new DesignerActionGlyph(behavior, bounds, service);
                                }
                            }
                        }
                        if (glyph == null)
                        {
                            glyph = new DesignerActionGlyph(behavior, this.designerActionAdorner);
                        }
                        if (glyph != null)
                        {
                            this.componentToGlyph.Add(comp, glyph);
                        }
                        return glyph;
                    }
                    glyph = this.componentToGlyph[comp] as DesignerActionGlyph;
                    if (glyph != null)
                    {
                        DesignerActionBehavior behavior2 = glyph.Behavior as DesignerActionBehavior;
                        if (behavior2 != null)
                        {
                            behavior2.ActionLists = dalColl;
                        }
                        glyph.Invalidate();
                    }
                    return glyph;
                }
                this.RemoveActionGlyph(comp);
            }
            return null;
        }

        private Point GetGlyphLocationScreenCoord(IComponent relatedComponent, Glyph glyph)
        {
            Point point = new Point(0, 0);
            if ((relatedComponent is Control) && !(relatedComponent is ToolStripDropDown))
            {
                return this.behaviorService.AdornerWindowPointToScreen(glyph.Bounds.Location);
            }
            if (relatedComponent is ToolStripItem)
            {
                ToolStripItem item = relatedComponent as ToolStripItem;
                if ((item != null) && (item.Owner != null))
                {
                    point = this.behaviorService.AdornerWindowPointToScreen(glyph.Bounds.Location);
                }
                return point;
            }
            if (relatedComponent != null)
            {
                ComponentTray service = this.serviceProvider.GetService(typeof(ComponentTray)) as ComponentTray;
                if (service != null)
                {
                    point = service.PointToScreen(glyph.Bounds.Location);
                }
            }
            return point;
        }

        internal void HideDesignerActionPanel()
        {
            if (this.IsDesignerActionPanelVisible)
            {
                this.designerActionHost.Close();
            }
        }

        private void InvalidateGlyphOnLastTransaction(object sender, DesignerTransactionCloseEventArgs e)
        {
            if (e.LastTransaction)
            {
                IDesignerHost host = (this.serviceProvider != null) ? (this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost) : null;
                if (host != null)
                {
                    host.TransactionClosed -= new DesignerTransactionCloseEventHandler(this.InvalidateGlyphOnLastTransaction);
                }
                if (this.relatedGlyphTransaction != null)
                {
                    this.relatedGlyphTransaction.InvalidateOwnerLocation();
                }
                this.relatedGlyphTransaction = null;
            }
        }

        private void OnComponentChanged(object source, ComponentChangedEventArgs ce)
        {
            if ((((ce.Component != null) && (ce.Member != null)) && this.IsDesignerActionPanelVisible) && ((this.lastPanelComponent == null) || this.lastPanelComponent.Equals(ce.Component)))
            {
                DesignerActionGlyph glyph = this.componentToGlyph[ce.Component] as DesignerActionGlyph;
                if (glyph != null)
                {
                    glyph.Invalidate();
                    if (ce.Member.Name.Equals("Dock"))
                    {
                        this.RecreatePanel(ce.Component as IComponent);
                    }
                    if ((ce.Member.Name.Equals("Location") || ce.Member.Name.Equals("Width")) || ce.Member.Name.Equals("Height"))
                    {
                        this.UpdateDAPLocation(ce.Component as IComponent, glyph);
                    }
                }
            }
        }

        private void OnDesignerActionsChanged(object sender, DesignerActionListsChangedEventArgs e)
        {
            if ((this.marshalingControl != null) && this.marshalingControl.IsHandleCreated)
            {
                this.marshalingControl.BeginInvoke(new ActionChangedEventHandler(this.OnInvokedDesignerActionChanged), new object[] { sender, e });
            }
        }

        private void OnDesignerActionUIStateChange(object sender, DesignerActionUIStateChangeEventArgs e)
        {
            IComponent relatedObject = e.RelatedObject as IComponent;
            if (relatedObject != null)
            {
                DesignerActionGlyph designerActionGlyph = this.GetDesignerActionGlyph(relatedObject);
                if (designerActionGlyph != null)
                {
                    if (e.ChangeType != DesignerActionUIStateChangeType.Show)
                    {
                        if (e.ChangeType != DesignerActionUIStateChangeType.Hide)
                        {
                            if (e.ChangeType == DesignerActionUIStateChangeType.Refresh)
                            {
                                designerActionGlyph.Invalidate();
                                this.RecreatePanel((IComponent) e.RelatedObject);
                            }
                        }
                        else
                        {
                            DesignerActionBehavior behavior2 = designerActionGlyph.Behavior as DesignerActionBehavior;
                            if (behavior2 != null)
                            {
                                behavior2.HideUI();
                            }
                        }
                    }
                    else
                    {
                        DesignerActionBehavior behavior = designerActionGlyph.Behavior as DesignerActionBehavior;
                        if (behavior != null)
                        {
                            behavior.ShowUI(designerActionGlyph);
                        }
                    }
                }
            }
            else if (e.ChangeType == DesignerActionUIStateChangeType.Hide)
            {
                this.HideDesignerActionPanel();
            }
        }

        private void OnInvokedDesignerActionChanged(object sender, DesignerActionListsChangedEventArgs e)
        {
            IComponent relatedObject = e.RelatedObject as IComponent;
            DesignerActionGlyph designerActionGlyph = null;
            if (e.ChangeType == DesignerActionListsChangedType.ActionListsAdded)
            {
                if (relatedObject == null)
                {
                    return;
                }
                IComponent primarySelection = this.selSvc.PrimarySelection as IComponent;
                if (primarySelection == e.RelatedObject)
                {
                    designerActionGlyph = this.GetDesignerActionGlyph(relatedObject, e.ActionLists);
                    if (designerActionGlyph != null)
                    {
                        this.VerifyGlyphIsInAdorner(designerActionGlyph);
                    }
                    else
                    {
                        this.RemoveActionGlyph(e.RelatedObject);
                    }
                }
            }
            if ((e.ChangeType == DesignerActionListsChangedType.ActionListsRemoved) && (e.ActionLists.Count == 0))
            {
                this.RemoveActionGlyph(e.RelatedObject);
            }
            else if (designerActionGlyph != null)
            {
                this.RecreatePanel(relatedObject);
            }
        }

        private void OnKeyShowDesignerActions(object sender, EventArgs e)
        {
            this.ShowDesignerActionPanelForPrimarySelection();
        }

        private void OnShowComplete(object sender, EventArgs e)
        {
            this.cancelClose = false;
            if (((this.designerActionHost != null) && (this.designerActionHost.Handle != IntPtr.Zero)) && this.designerActionHost.Visible)
            {
                System.Design.UnsafeNativeMethods.SetActiveWindow(new HandleRef(this, this.designerActionHost.Handle));
                this.designerActionHost.CheckFocusIsRight();
            }
        }

        private void RecreateInternal(IComponent comp)
        {
            DesignerActionGlyph designerActionGlyph = this.GetDesignerActionGlyph(comp);
            if (designerActionGlyph != null)
            {
                this.VerifyGlyphIsInAdorner(designerActionGlyph);
                this.RecreatePanel(designerActionGlyph);
                this.UpdateDAPLocation(comp, designerActionGlyph);
            }
        }

        private void RecreatePanel(IComponent comp)
        {
            if (!this.inTransaction && (comp == this.selSvc.PrimarySelection))
            {
                IDesignerHost service = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (service != null)
                {
                    bool isClosingTransaction = false;
                    IDesignerHostTransactionState state = service as IDesignerHostTransactionState;
                    if (state != null)
                    {
                        isClosingTransaction = state.IsClosingTransaction;
                    }
                    if (service.InTransaction && !isClosingTransaction)
                    {
                        service.TransactionClosed += new DesignerTransactionCloseEventHandler(this.DesignerTransactionClosed);
                        this.inTransaction = true;
                        this.relatedComponentTransaction = comp;
                        return;
                    }
                }
                this.RecreateInternal(comp);
            }
        }

        private void RecreatePanel(Glyph glyphWithPanelToRegen)
        {
            if (this.IsDesignerActionPanelVisible && (glyphWithPanelToRegen != null))
            {
                DesignerActionBehavior behavior = glyphWithPanelToRegen.Behavior as DesignerActionBehavior;
                if (behavior != null)
                {
                    this.designerActionHost.CurrentPanel.UpdateTasks(behavior.ActionLists, new DesignerActionListCollection(), System.Design.SR.GetString("DesignerActionPanel_DefaultPanelTitle", new object[] { behavior.RelatedComponent.GetType().Name }), null);
                    this.designerActionHost.UpdateContainerSize();
                }
            }
        }

        internal void RemoveActionGlyph(object relatedObject)
        {
            if (relatedObject != null)
            {
                if (this.IsDesignerActionPanelVisible && (relatedObject == this.lastPanelComponent))
                {
                    this.HideDesignerActionPanel();
                }
                DesignerActionGlyph glyph = (DesignerActionGlyph) this.componentToGlyph[relatedObject];
                if (glyph != null)
                {
                    ComponentTray service = this.serviceProvider.GetService(typeof(ComponentTray)) as ComponentTray;
                    if (((service != null) && (service.SelectionGlyphs != null)) && ((service != null) && service.SelectionGlyphs.Contains(glyph)))
                    {
                        service.SelectionGlyphs.Remove(glyph);
                    }
                    if (this.designerActionAdorner.Glyphs.Contains(glyph))
                    {
                        this.designerActionAdorner.Glyphs.Remove(glyph);
                    }
                    this.componentToGlyph.Remove(relatedObject);
                    IDesignerHost host = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if ((host != null) && host.InTransaction)
                    {
                        host.TransactionClosed += new DesignerTransactionCloseEventHandler(this.InvalidateGlyphOnLastTransaction);
                        this.relatedGlyphTransaction = glyph;
                    }
                }
            }
        }

        internal void ShowDesignerActionPanel(IComponent relatedComponent, DesignerActionPanel panel, DesignerActionGlyph glyph)
        {
            if (this.designerActionHost == null)
            {
                this.designerActionHost = new DesignerActionToolStripDropDown(this, this.mainParentWindow);
                this.designerActionHost.AutoSize = false;
                this.designerActionHost.Padding = Padding.Empty;
                this.designerActionHost.Renderer = new NoBorderRenderer();
                this.designerActionHost.Text = "DesignerActionTopLevelForm";
                this.designerActionHost.Closing += new ToolStripDropDownClosingEventHandler(this.toolStripDropDown_Closing);
            }
            this.designerActionHost.AccessibleName = System.Design.SR.GetString("DesignerActionPanel_DefaultPanelTitle", new object[] { relatedComponent.GetType().Name });
            panel.AccessibleName = System.Design.SR.GetString("DesignerActionPanel_DefaultPanelTitle", new object[] { relatedComponent.GetType().Name });
            this.designerActionHost.SetDesignerActionPanel(panel, glyph);
            Point screenLocation = this.UpdateDAPLocation(relatedComponent, glyph);
            if ((this.behaviorService != null) && this.behaviorService.AdornerWindowControl.DisplayRectangle.IntersectsWith(glyph.Bounds))
            {
                if ((this.mainParentWindow != null) && (this.mainParentWindow.Handle != IntPtr.Zero))
                {
                    System.Design.UnsafeNativeMethods.SetWindowLong(new HandleRef(this.designerActionHost, this.designerActionHost.Handle), -8, new HandleRef(this.mainParentWindow, this.mainParentWindow.Handle));
                }
                this.cancelClose = true;
                this.designerActionHost.Show(screenLocation);
                this.designerActionHost.Focus();
                this.designerActionHost.BeginInvoke(new EventHandler(this.OnShowComplete));
                glyph.InvalidateOwnerLocation();
                this.lastPanelComponent = relatedComponent;
                this.dapkb = new DesignerActionKeyboardBehavior(this.designerActionHost.CurrentPanel, this.serviceProvider, this.behaviorService);
                this.behaviorService.PushBehavior(this.dapkb);
            }
        }

        internal bool ShowDesignerActionPanelForPrimarySelection()
        {
            if (this.selSvc != null)
            {
                object primarySelection = this.selSvc.PrimarySelection;
                if ((primarySelection == null) || !this.componentToGlyph.Contains(primarySelection))
                {
                    return false;
                }
                DesignerActionGlyph g = (DesignerActionGlyph) this.componentToGlyph[primarySelection];
                if ((g != null) && (g.Behavior is DesignerActionBehavior))
                {
                    DesignerActionBehavior behavior = g.Behavior as DesignerActionBehavior;
                    if (behavior != null)
                    {
                        if (!this.IsDesignerActionPanelVisible)
                        {
                            behavior.ShowUI(g);
                            return true;
                        }
                        behavior.HideUI();
                        return false;
                    }
                }
            }
            return false;
        }

        private void toolStripDropDown_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (this.cancelClose || e.Cancel)
            {
                e.Cancel = true;
            }
            else
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                {
                    e.Cancel = true;
                }
                if (e.CloseReason == ToolStripDropDownCloseReason.Keyboard)
                {
                    e.Cancel = false;
                }
                if (!e.Cancel && (this.lastPanelComponent != null))
                {
                    Point lastCursorPoint = DesignerUtils.LastCursorPoint;
                    DesignerActionGlyph glyph = this.componentToGlyph[this.lastPanelComponent] as DesignerActionGlyph;
                    if (glyph != null)
                    {
                        Rectangle rectangle3 = new Rectangle(this.GetGlyphLocationScreenCoord(this.lastPanelComponent, glyph), new Size(glyph.Bounds.Width, glyph.Bounds.Height));
                        if (rectangle3.Contains(lastCursorPoint))
                        {
                            DesignerActionBehavior behavior = glyph.Behavior as DesignerActionBehavior;
                            behavior.IgnoreNextMouseUp = true;
                        }
                        glyph.InvalidateOwnerLocation();
                    }
                    this.lastPanelComponent = null;
                    this.behaviorService.PopBehavior(this.dapkb);
                }
            }
        }

        internal Point UpdateDAPLocation(IComponent component, DesignerActionGlyph glyph)
        {
            DockStyle style;
            if (component == null)
            {
                component = this.lastPanelComponent;
            }
            if (this.designerActionHost == null)
            {
                return Point.Empty;
            }
            if ((component == null) || (glyph == null))
            {
                return this.designerActionHost.Location;
            }
            if ((this.behaviorService != null) && !this.behaviorService.AdornerWindowControl.DisplayRectangle.IntersectsWith(glyph.Bounds))
            {
                this.HideDesignerActionPanel();
                return this.designerActionHost.Location;
            }
            Point glyphLocationScreenCoord = this.GetGlyphLocationScreenCoord(component, glyph);
            Rectangle rectangleAnchor = new Rectangle(glyphLocationScreenCoord, glyph.Bounds.Size);
            Point point2 = DesignerActionPanel.ComputePreferredDesktopLocation(rectangleAnchor, this.designerActionHost.Size, out style);
            glyph.DockEdge = style;
            this.designerActionHost.Location = point2;
            return point2;
        }

        private void VerifyGlyphIsInAdorner(DesignerActionGlyph glyph)
        {
            if (glyph.IsInComponentTray)
            {
                ComponentTray service = this.serviceProvider.GetService(typeof(ComponentTray)) as ComponentTray;
                if ((service.SelectionGlyphs != null) && !service.SelectionGlyphs.Contains(glyph))
                {
                    service.SelectionGlyphs.Insert(0, glyph);
                }
            }
            else if (((this.designerActionAdorner != null) && (this.designerActionAdorner.Glyphs != null)) && !this.designerActionAdorner.Glyphs.Contains(glyph))
            {
                this.designerActionAdorner.Glyphs.Insert(0, glyph);
            }
            glyph.InvalidateOwnerLocation();
        }

        internal bool IsDesignerActionPanelVisible
        {
            get
            {
                return ((this.designerActionHost != null) && this.designerActionHost.Visible);
            }
        }

        internal IComponent LastPanelComponent
        {
            get
            {
                if (!this.IsDesignerActionPanelVisible)
                {
                    return null;
                }
                return this.lastPanelComponent;
            }
        }

        private delegate void ActionChangedEventHandler(object sender, DesignerActionListsChangedEventArgs e);
    }
}

