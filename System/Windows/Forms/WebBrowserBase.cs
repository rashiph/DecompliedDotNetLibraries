namespace System.Windows.Forms
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    [ComVisible(true), DefaultProperty("Name"), Designer("System.Windows.Forms.Design.AxDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultEvent("Enter"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class WebBrowserBase : Control
    {
        internal object activeXInstance;
        private WebBrowserHelper.AXEditMode axEditMode;
        private BitVector32 axHostState = new BitVector32();
        private System.Windows.Forms.UnsafeNativeMethods.IOleControl axOleControl;
        private System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject axOleInPlaceActiveObject;
        private System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject axOleInPlaceObject;
        private System.Windows.Forms.UnsafeNativeMethods.IOleObject axOleObject;
        private WebBrowserHelper.AXState axReloadingState;
        private WebBrowserSiteBase axSite;
        private WebBrowserHelper.AXState axState;
        private WebBrowserBaseNativeWindow axWindow;
        private Guid clsid;
        internal WebBrowserContainer container;
        private ContainerControl containingControl;
        private IntPtr hwndFocus = IntPtr.Zero;
        private bool ignoreDialogKeys;
        private bool inRtlRecreate;
        private int noComponentChange;
        private EventHandler selectionChangeHandler;
        private WebBrowserHelper.SelectionStyle selectionStyle;
        private WebBrowserContainer wbContainer;
        private Size webBrowserBaseChangingSize = Size.Empty;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler BackColorChanged
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "BackColorChanged" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler BackgroundImageChanged
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "BackgroundImageChanged" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler BackgroundImageLayoutChanged
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "BackgroundImageLayoutChanged" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler BindingContextChanged
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "BindingContextChanged" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event UICuesEventHandler ChangeUICues
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "ChangeUICues" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler Click
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "Click" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler CursorChanged
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "CursorChanged" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler DoubleClick
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "DoubleClick" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event DragEventHandler DragDrop
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "DragDrop" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event DragEventHandler DragEnter
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "DragEnter" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler DragLeave
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "DragLeave" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event DragEventHandler DragOver
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "DragOver" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler EnabledChanged
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "EnabledChanged" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler Enter
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "Enter" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler FontChanged
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "FontChanged" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler ForeColorChanged
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "ForeColorChanged" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event GiveFeedbackEventHandler GiveFeedback
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "GiveFeedback" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event HelpEventHandler HelpRequested
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "HelpRequested" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler ImeModeChanged
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "ImeModeChanged" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event KeyEventHandler KeyDown
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "KeyDown" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event KeyPressEventHandler KeyPress
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "KeyPress" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event KeyEventHandler KeyUp
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "KeyUp" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event LayoutEventHandler Layout
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "Layout" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler Leave
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "Leave" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler MouseCaptureChanged
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "MouseCaptureChanged" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event MouseEventHandler MouseClick
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "MouseClick" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event MouseEventHandler MouseDoubleClick
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "MouseDoubleClick" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event MouseEventHandler MouseDown
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "MouseDown" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler MouseEnter
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "MouseEnter" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler MouseHover
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "MouseHover" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler MouseLeave
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "MouseLeave" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event MouseEventHandler MouseMove
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "MouseMove" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event MouseEventHandler MouseUp
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "MouseUp" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event MouseEventHandler MouseWheel
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "MouseWheel" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event PaintEventHandler Paint
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "Paint" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "QueryAccessibilityHelp" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event QueryContinueDragEventHandler QueryContinueDrag
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "QueryContinueDrag" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler RightToLeftChanged
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "RightToLeftChanged" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler StyleChanged
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "StyleChanged" }));
            }
            remove
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler TextChanged
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "TextChanged" }));
            }
            remove
            {
            }
        }

        internal WebBrowserBase(string clsidString)
        {
            if (Application.OleRequired() != ApartmentState.STA)
            {
                throw new ThreadStateException(System.Windows.Forms.SR.GetString("AXMTAThread", new object[] { clsidString }));
            }
            base.SetStyle(ControlStyles.UserPaint, false);
            this.clsid = new Guid(clsidString);
            this.webBrowserBaseChangingSize.Width = -1;
            this.SetAXHostState(WebBrowserHelper.isMaskEdit, this.clsid.Equals(WebBrowserHelper.maskEdit_Clsid));
        }

        internal void AddSelectionHandler()
        {
            if (!this.GetAXHostState(WebBrowserHelper.addedSelectionHandler))
            {
                this.SetAXHostState(WebBrowserHelper.addedSelectionHandler, true);
                ISelectionService selectionService = WebBrowserHelper.GetSelectionService(this);
                if (selectionService != null)
                {
                    selectionService.SelectionChanging += this.SelectionChangeHandler;
                }
            }
        }

        private void AmbientChanged(int dispid)
        {
            if (this.activeXInstance != null)
            {
                try
                {
                    base.Invalidate();
                    this.axOleControl.OnAmbientPropertyChange(dispid);
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                }
            }
        }

        protected virtual void AttachInterfaces(object nativeActiveXObject)
        {
        }

        private void AttachInterfacesInternal()
        {
            this.axOleObject = (System.Windows.Forms.UnsafeNativeMethods.IOleObject) this.activeXInstance;
            this.axOleInPlaceObject = (System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject) this.activeXInstance;
            this.axOleInPlaceActiveObject = (System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject) this.activeXInstance;
            this.axOleControl = (System.Windows.Forms.UnsafeNativeMethods.IOleControl) this.activeXInstance;
            this.AttachInterfaces(this.activeXInstance);
        }

        internal void AttachWindow(IntPtr hwnd)
        {
            System.Windows.Forms.UnsafeNativeMethods.SetParent(new HandleRef(null, hwnd), new HandleRef(this, base.Handle));
            if (this.axWindow != null)
            {
                this.axWindow.ReleaseHandle();
            }
            this.axWindow = new WebBrowserBaseNativeWindow(this);
            this.axWindow.AssignHandle(hwnd, false);
            base.UpdateZOrder();
            base.UpdateBounds();
            Size size = base.Size;
            size = this.SetExtent(size.Width, size.Height);
            Point location = base.Location;
            base.Bounds = new Rectangle(location.X, location.Y, size.Width, size.Height);
        }

        internal override bool CanSelectCore()
        {
            if (this.ActiveXState < WebBrowserHelper.AXState.InPlaceActive)
            {
                return false;
            }
            return base.CanSelectCore();
        }

        protected virtual void CreateSink()
        {
        }

        internal WebBrowserContainer CreateWebBrowserContainer()
        {
            if (this.wbContainer == null)
            {
                this.wbContainer = new WebBrowserContainer(this);
            }
            return this.wbContainer;
        }

        protected virtual WebBrowserSiteBase CreateWebBrowserSiteBase()
        {
            return new WebBrowserSiteBase(this);
        }

        protected virtual void DetachInterfaces()
        {
        }

        private void DetachInterfacesInternal()
        {
            this.axOleObject = null;
            this.axOleInPlaceObject = null;
            this.axOleInPlaceActiveObject = null;
            this.axOleControl = null;
            this.DetachInterfaces();
        }

        protected virtual void DetachSink()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.TransitionDownTo(WebBrowserHelper.AXState.Passive);
            }
            base.Dispose(disposing);
        }

        internal bool DoVerb(int verb)
        {
            return (this.axOleObject.DoVerb(verb, IntPtr.Zero, this.ActiveXSite, 0, base.Handle, new System.Windows.Forms.NativeMethods.COMRECT(base.Bounds)) == 0);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DrawToBitmap(Bitmap bitmap, Rectangle targetBounds)
        {
            base.DrawToBitmap(bitmap, targetBounds);
        }

        internal ContainerControl FindContainerControlInternal()
        {
            if (this.Site != null)
            {
                IDesignerHost service = (IDesignerHost) this.Site.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    IComponent rootComponent = service.RootComponent;
                    if ((rootComponent != null) && (rootComponent is ContainerControl))
                    {
                        return (ContainerControl) rootComponent;
                    }
                }
            }
            ContainerControl control = null;
            for (Control control2 = this; control2 != null; control2 = control2.ParentInternal)
            {
                ContainerControl control3 = control2 as ContainerControl;
                if (control3 != null)
                {
                    control = control3;
                }
            }
            if (control == null)
            {
                control = Control.FromHandle(System.Windows.Forms.UnsafeNativeMethods.GetParent(new HandleRef(this, base.Handle))) as ContainerControl;
            }
            if (control is Application.ParkingWindow)
            {
                control = null;
            }
            this.SetAXHostState(WebBrowserHelper.recomputeContainingControl, control == null);
            return control;
        }

        internal bool GetAXHostState(int mask)
        {
            return this.axHostState[mask];
        }

        private Size GetExtent()
        {
            System.Windows.Forms.NativeMethods.tagSIZEL pSizel = new System.Windows.Forms.NativeMethods.tagSIZEL();
            this.axOleObject.GetExtent(1, pSizel);
            this.HiMetric2Pixel(pSizel, pSizel);
            return new Size(pSizel.cx, pSizel.cy);
        }

        internal IntPtr GetHandleNoCreate()
        {
            if (!base.IsHandleCreated)
            {
                return IntPtr.Zero;
            }
            return base.Handle;
        }

        internal WebBrowserContainer GetParentContainer()
        {
            if (this.container == null)
            {
                this.container = WebBrowserContainer.FindContainerForControl(this);
            }
            if (this.container == null)
            {
                this.container = this.CreateWebBrowserContainer();
                this.container.AddControl(this);
            }
            return this.container;
        }

        private void HiMetric2Pixel(System.Windows.Forms.NativeMethods.tagSIZEL sz, System.Windows.Forms.NativeMethods.tagSIZEL szout)
        {
            System.Windows.Forms.NativeMethods._POINTL pPtlHimetric = new System.Windows.Forms.NativeMethods._POINTL {
                x = sz.cx,
                y = sz.cy
            };
            System.Windows.Forms.NativeMethods.tagPOINTF pPtfContainer = new System.Windows.Forms.NativeMethods.tagPOINTF();
            ((System.Windows.Forms.UnsafeNativeMethods.IOleControlSite) this.ActiveXSite).TransformCoords(pPtlHimetric, pPtfContainer, 6);
            szout.cx = (int) pPtfContainer.x;
            szout.cy = (int) pPtfContainer.y;
        }

        protected override bool IsInputChar(char charCode)
        {
            return true;
        }

        internal void MakeDirty()
        {
            ISite site = this.Site;
            if (site != null)
            {
                IComponentChangeService service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.OnComponentChanging(this, null);
                    service.OnComponentChanged(this, null, null, null);
                }
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            this.AmbientChanged(-701);
        }

        internal override void OnBoundsUpdate(int x, int y, int width, int height)
        {
            if (this.ActiveXState >= WebBrowserHelper.AXState.InPlaceActive)
            {
                try
                {
                    this.webBrowserBaseChangingSize.Width = width;
                    this.webBrowserBaseChangingSize.Height = height;
                    this.AXInPlaceObject.SetObjectRects(new System.Windows.Forms.NativeMethods.COMRECT(new Rectangle(0, 0, width, height)), WebBrowserHelper.GetClipRect());
                }
                finally
                {
                    this.webBrowserBaseChangingSize.Width = -1;
                }
            }
            base.OnBoundsUpdate(x, y, width, height);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.AmbientChanged(-703);
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            this.AmbientChanged(-704);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            if (this.ActiveXState < WebBrowserHelper.AXState.UIActive)
            {
                this.TransitionUpTo(WebBrowserHelper.AXState.UIActive);
            }
            base.OnGotFocus(e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnHandleCreated(EventArgs e)
        {
            if (Application.OleRequired() != ApartmentState.STA)
            {
                throw new ThreadStateException(System.Windows.Forms.SR.GetString("ThreadMustBeSTA"));
            }
            base.OnHandleCreated(e);
            if ((this.axReloadingState != WebBrowserHelper.AXState.Passive) && (this.axReloadingState != this.axState))
            {
                if (this.axState < this.axReloadingState)
                {
                    this.TransitionUpTo(this.axReloadingState);
                }
                else
                {
                    this.TransitionDownTo(this.axReloadingState);
                }
                this.axReloadingState = WebBrowserHelper.AXState.Passive;
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (!base.ContainsFocus)
            {
                this.TransitionDownTo(WebBrowserHelper.AXState.InPlaceActive);
            }
        }

        private void OnNewSelection(object sender, EventArgs e)
        {
            if (base.DesignMode)
            {
                ISelectionService selectionService = WebBrowserHelper.GetSelectionService(this);
                if (selectionService != null)
                {
                    if (!selectionService.GetComponentSelected(this))
                    {
                        if (this.EditMode)
                        {
                            this.GetParentContainer().OnExitEditMode(this);
                            this.SetEditMode(WebBrowserHelper.AXEditMode.None);
                        }
                        this.SetSelectionStyle(WebBrowserHelper.SelectionStyle.Selected);
                        this.RemoveSelectionHandler();
                    }
                    else
                    {
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this)["SelectionStyle"];
                        if ((descriptor != null) && (descriptor.PropertyType == typeof(int)))
                        {
                            int num = (int) descriptor.GetValue(this);
                            if (num != this.selectionStyle)
                            {
                                descriptor.SetValue(this, this.selectionStyle);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            Control parentInternal = this.ParentInternal;
            if (((base.Visible && (parentInternal != null)) && parentInternal.Visible) || base.IsHandleCreated)
            {
                this.TransitionUpTo(WebBrowserHelper.AXState.InPlaceActive);
            }
            base.OnParentChanged(e);
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if ((base.Visible && !base.Disposing) && !base.IsDisposed)
            {
                this.TransitionUpTo(WebBrowserHelper.AXState.InPlaceActive);
            }
            base.OnVisibleChanged(e);
        }

        private void Pixel2hiMetric(System.Windows.Forms.NativeMethods.tagSIZEL sz, System.Windows.Forms.NativeMethods.tagSIZEL szout)
        {
            System.Windows.Forms.NativeMethods.tagPOINTF pPtfContainer = new System.Windows.Forms.NativeMethods.tagPOINTF {
                x = sz.cx,
                y = sz.cy
            };
            System.Windows.Forms.NativeMethods._POINTL pPtlHimetric = new System.Windows.Forms.NativeMethods._POINTL();
            ((System.Windows.Forms.UnsafeNativeMethods.IOleControlSite) this.ActiveXSite).TransformCoords(pPtlHimetric, pPtfContainer, 10);
            szout.cx = pPtlHimetric.x;
            szout.cy = pPtlHimetric.y;
        }

        public override bool PreProcessMessage(ref Message msg)
        {
            if (this.IsUserMode)
            {
                if (this.GetAXHostState(WebBrowserHelper.siteProcessedInputKey))
                {
                    return base.PreProcessMessage(ref msg);
                }
                System.Windows.Forms.NativeMethods.MSG lpmsg = new System.Windows.Forms.NativeMethods.MSG {
                    message = msg.Msg,
                    wParam = msg.WParam,
                    lParam = msg.LParam,
                    hwnd = msg.HWnd
                };
                this.SetAXHostState(WebBrowserHelper.siteProcessedInputKey, false);
                try
                {
                    if (this.axOleInPlaceObject != null)
                    {
                        int num = this.axOleInPlaceActiveObject.TranslateAccelerator(ref lpmsg);
                        if (num == 0)
                        {
                            return true;
                        }
                        msg.Msg = lpmsg.message;
                        msg.WParam = lpmsg.wParam;
                        msg.LParam = lpmsg.lParam;
                        msg.HWnd = lpmsg.hwnd;
                        if (num == 1)
                        {
                            bool flag = false;
                            this.ignoreDialogKeys = true;
                            try
                            {
                                flag = base.PreProcessMessage(ref msg);
                            }
                            finally
                            {
                                this.ignoreDialogKeys = false;
                            }
                            return flag;
                        }
                        return (this.GetAXHostState(WebBrowserHelper.siteProcessedInputKey) && base.PreProcessMessage(ref msg));
                    }
                }
                finally
                {
                    this.SetAXHostState(WebBrowserHelper.siteProcessedInputKey, false);
                }
            }
            return false;
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            return (!this.ignoreDialogKeys && base.ProcessDialogKey(keyData));
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
            bool flag = false;
            if (base.CanSelect)
            {
                try
                {
                    System.Windows.Forms.NativeMethods.tagCONTROLINFO pCI = new System.Windows.Forms.NativeMethods.tagCONTROLINFO();
                    if (System.Windows.Forms.NativeMethods.Succeeded(this.axOleControl.GetControlInfo(pCI)))
                    {
                        System.Windows.Forms.NativeMethods.MSG lpMsg = new System.Windows.Forms.NativeMethods.MSG {
                            hwnd = IntPtr.Zero,
                            message = 260,
                            wParam = (IntPtr) char.ToUpper(charCode, CultureInfo.CurrentCulture),
                            lParam = (IntPtr) 0x20180001,
                            time = System.Windows.Forms.SafeNativeMethods.GetTickCount()
                        };
                        System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT();
                        System.Windows.Forms.UnsafeNativeMethods.GetCursorPos(pt);
                        lpMsg.pt_x = pt.x;
                        lpMsg.pt_y = pt.y;
                        if (System.Windows.Forms.SafeNativeMethods.IsAccelerator(new HandleRef(pCI, pCI.hAccel), pCI.cAccel, ref lpMsg, null))
                        {
                            this.axOleControl.OnMnemonic(ref lpMsg);
                            this.FocusInternal();
                            flag = true;
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                }
            }
            return flag;
        }

        internal override void RecreateHandleCore()
        {
            if (!this.inRtlRecreate)
            {
                base.RecreateHandleCore();
            }
        }

        internal bool RemoveSelectionHandler()
        {
            bool aXHostState = this.GetAXHostState(WebBrowserHelper.addedSelectionHandler);
            if (aXHostState)
            {
                this.SetAXHostState(WebBrowserHelper.addedSelectionHandler, false);
                ISelectionService selectionService = WebBrowserHelper.GetSelectionService(this);
                if (selectionService != null)
                {
                    selectionService.SelectionChanging -= this.SelectionChangeHandler;
                }
            }
            return aXHostState;
        }

        internal void SetAXHostState(int mask, bool value)
        {
            this.axHostState[mask] = value;
        }

        internal void SetEditMode(WebBrowserHelper.AXEditMode em)
        {
            this.axEditMode = em;
        }

        private Size SetExtent(int width, int height)
        {
            System.Windows.Forms.NativeMethods.tagSIZEL sz = new System.Windows.Forms.NativeMethods.tagSIZEL {
                cx = width,
                cy = height
            };
            bool designMode = base.DesignMode;
            try
            {
                this.Pixel2hiMetric(sz, sz);
                this.axOleObject.SetExtent(1, sz);
            }
            catch (COMException)
            {
                designMode = true;
            }
            if (designMode)
            {
                this.axOleObject.GetExtent(1, sz);
                try
                {
                    this.axOleObject.SetExtent(1, sz);
                }
                catch (COMException)
                {
                }
            }
            return this.GetExtent();
        }

        internal void SetSelectionStyle(WebBrowserHelper.SelectionStyle selectionStyle)
        {
            if (base.DesignMode)
            {
                ISelectionService selectionService = WebBrowserHelper.GetSelectionService(this);
                this.selectionStyle = selectionStyle;
                if ((selectionService != null) && selectionService.GetComponentSelected(this))
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this)["SelectionStyle"];
                    if ((descriptor != null) && (descriptor.PropertyType == typeof(int)))
                    {
                        descriptor.SetValue(this, (int) selectionStyle);
                    }
                }
            }
        }

        private void StartEvents()
        {
            if (!this.GetAXHostState(WebBrowserHelper.sinkAttached))
            {
                this.SetAXHostState(WebBrowserHelper.sinkAttached, true);
                this.CreateSink();
            }
            this.ActiveXSite.StartEvents();
        }

        private void StopEvents()
        {
            if (this.GetAXHostState(WebBrowserHelper.sinkAttached))
            {
                this.SetAXHostState(WebBrowserHelper.sinkAttached, false);
                this.DetachSink();
            }
            this.ActiveXSite.StopEvents();
        }

        internal void TransitionDownTo(WebBrowserHelper.AXState state)
        {
            if (!this.GetAXHostState(WebBrowserHelper.inTransition))
            {
                this.SetAXHostState(WebBrowserHelper.inTransition, true);
                try
                {
                    while (state < this.ActiveXState)
                    {
                        switch (this.ActiveXState)
                        {
                            case WebBrowserHelper.AXState.Loaded:
                            {
                                this.TransitionFromLoadedToPassive();
                                continue;
                            }
                            case WebBrowserHelper.AXState.Running:
                            {
                                this.TransitionFromRunningToLoaded();
                                continue;
                            }
                            case WebBrowserHelper.AXState.InPlaceActive:
                            {
                                this.TransitionFromInPlaceActiveToRunning();
                                continue;
                            }
                            case WebBrowserHelper.AXState.UIActive:
                            {
                                this.TransitionFromUIActiveToInPlaceActive();
                                continue;
                            }
                        }
                        this.ActiveXState -= 1;
                    }
                }
                finally
                {
                    this.SetAXHostState(WebBrowserHelper.inTransition, false);
                }
            }
        }

        private void TransitionFromInPlaceActiveToRunning()
        {
            if (this.ActiveXState == WebBrowserHelper.AXState.InPlaceActive)
            {
                ContainerControl containingControl = this.ContainingControl;
                if ((containingControl != null) && (containingControl.ActiveControl == this))
                {
                    containingControl.SetActiveControlInternal(null);
                }
                this.AXInPlaceObject.InPlaceDeactivate();
                this.ActiveXState = WebBrowserHelper.AXState.Running;
            }
        }

        private void TransitionFromInPlaceActiveToUIActive()
        {
            if (this.ActiveXState == WebBrowserHelper.AXState.InPlaceActive)
            {
                try
                {
                    this.DoVerb(-4);
                }
                catch (Exception exception)
                {
                    throw new TargetInvocationException(System.Windows.Forms.SR.GetString("AXNohWnd", new object[] { base.GetType().Name }), exception);
                }
                this.ActiveXState = WebBrowserHelper.AXState.UIActive;
            }
        }

        private void TransitionFromLoadedToPassive()
        {
            if (this.ActiveXState == WebBrowserHelper.AXState.Loaded)
            {
                this.NoComponentChangeEvents++;
                try
                {
                    if (this.activeXInstance != null)
                    {
                        this.DetachInterfacesInternal();
                        Marshal.FinalReleaseComObject(this.activeXInstance);
                        this.activeXInstance = null;
                    }
                }
                finally
                {
                    this.NoComponentChangeEvents--;
                }
                this.ActiveXState = WebBrowserHelper.AXState.Passive;
            }
        }

        private void TransitionFromLoadedToRunning()
        {
            if (this.ActiveXState == WebBrowserHelper.AXState.Loaded)
            {
                int misc = 0;
                if (System.Windows.Forms.NativeMethods.Succeeded(this.axOleObject.GetMiscStatus(1, out misc)) && ((misc & 0x20000) != 0))
                {
                    this.axOleObject.SetClientSite(this.ActiveXSite);
                }
                if (!base.DesignMode)
                {
                    this.StartEvents();
                }
                this.ActiveXState = WebBrowserHelper.AXState.Running;
            }
        }

        private void TransitionFromPassiveToLoaded()
        {
            if (this.ActiveXState == WebBrowserHelper.AXState.Passive)
            {
                this.activeXInstance = System.Windows.Forms.UnsafeNativeMethods.CoCreateInstance(ref this.clsid, null, 1, ref System.Windows.Forms.NativeMethods.ActiveX.IID_IUnknown);
                this.ActiveXState = WebBrowserHelper.AXState.Loaded;
                this.AttachInterfacesInternal();
            }
        }

        private void TransitionFromRunningToInPlaceActive()
        {
            if (this.ActiveXState == WebBrowserHelper.AXState.Running)
            {
                try
                {
                    this.DoVerb(-5);
                }
                catch (Exception exception)
                {
                    throw new TargetInvocationException(System.Windows.Forms.SR.GetString("AXNohWnd", new object[] { base.GetType().Name }), exception);
                }
                base.CreateControl(true);
                this.ActiveXState = WebBrowserHelper.AXState.InPlaceActive;
            }
        }

        private void TransitionFromRunningToLoaded()
        {
            if (this.ActiveXState == WebBrowserHelper.AXState.Running)
            {
                this.StopEvents();
                WebBrowserContainer parentContainer = this.GetParentContainer();
                if (parentContainer != null)
                {
                    parentContainer.RemoveControl(this);
                }
                this.axOleObject.SetClientSite(null);
                this.ActiveXState = WebBrowserHelper.AXState.Loaded;
            }
        }

        private void TransitionFromUIActiveToInPlaceActive()
        {
            if (this.ActiveXState == WebBrowserHelper.AXState.UIActive)
            {
                this.AXInPlaceObject.UIDeactivate();
                this.ActiveXState = WebBrowserHelper.AXState.InPlaceActive;
            }
        }

        internal void TransitionUpTo(WebBrowserHelper.AXState state)
        {
            if (!this.GetAXHostState(WebBrowserHelper.inTransition))
            {
                this.SetAXHostState(WebBrowserHelper.inTransition, true);
                try
                {
                    while (state > this.ActiveXState)
                    {
                        switch (this.ActiveXState)
                        {
                            case WebBrowserHelper.AXState.Passive:
                            {
                                this.TransitionFromPassiveToLoaded();
                                continue;
                            }
                            case WebBrowserHelper.AXState.Loaded:
                            {
                                this.TransitionFromLoadedToRunning();
                                continue;
                            }
                            case WebBrowserHelper.AXState.Running:
                            {
                                this.TransitionFromRunningToInPlaceActive();
                                continue;
                            }
                            case WebBrowserHelper.AXState.InPlaceActive:
                            {
                                this.TransitionFromInPlaceActiveToUIActive();
                                continue;
                            }
                        }
                        this.ActiveXState += 1;
                    }
                }
                finally
                {
                    this.SetAXHostState(WebBrowserHelper.inTransition, false);
                }
            }
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            IntPtr ptr;
            switch (m.Msg)
            {
                case 2:
                    break;

                case 8:
                    this.hwndFocus = m.WParam;
                    try
                    {
                        base.WndProc(ref m);
                        return;
                    }
                    finally
                    {
                        this.hwndFocus = IntPtr.Zero;
                    }
                    break;

                case 20:
                case 0x15:
                case 0x20:
                case 0x2b:
                case 0x7b:
                case 0x202:
                case 0x203:
                case 0x205:
                case 0x206:
                case 520:
                case 0x209:
                case 0x2055:
                    this.DefWndProc(ref m);
                    return;

                case 0x21:
                case 0x201:
                case 0x204:
                case 0x207:
                    if ((!base.DesignMode && (this.containingControl != null)) && (this.containingControl.ActiveControl != this))
                    {
                        this.FocusInternal();
                    }
                    this.DefWndProc(ref m);
                    return;

                case 0x53:
                    base.WndProc(ref m);
                    this.DefWndProc(ref m);
                    return;

                case 0x111:
                    if (!Control.ReflectMessageInternal(m.LParam, ref m))
                    {
                        this.DefWndProc(ref m);
                    }
                    return;

                default:
                    if (m.Msg == WebBrowserHelper.REGMSG_MSG)
                    {
                        m.Result = (IntPtr) 0x7b;
                        return;
                    }
                    base.WndProc(ref m);
                    return;
            }
            if ((this.ActiveXState >= WebBrowserHelper.AXState.InPlaceActive) && System.Windows.Forms.NativeMethods.Succeeded(this.AXInPlaceObject.GetWindow(out ptr)))
            {
                Application.ParkHandle(new HandleRef(this.AXInPlaceObject, ptr));
            }
            if (base.RecreatingHandle)
            {
                this.axReloadingState = this.axState;
            }
            this.TransitionDownTo(WebBrowserHelper.AXState.Running);
            if (this.axWindow != null)
            {
                this.axWindow.ReleaseHandle();
            }
            this.OnHandleDestroyed(EventArgs.Empty);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public object ActiveXInstance
        {
            get
            {
                return this.activeXInstance;
            }
        }

        internal WebBrowserSiteBase ActiveXSite
        {
            get
            {
                if (this.axSite == null)
                {
                    this.axSite = this.CreateWebBrowserSiteBase();
                }
                return this.axSite;
            }
        }

        internal WebBrowserHelper.AXState ActiveXState
        {
            get
            {
                return this.axState;
            }
            set
            {
                this.axState = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override bool AllowDrop
        {
            get
            {
                return base.AllowDrop;
            }
            set
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("WebBrowserAllowDropNotSupported"));
            }
        }

        internal System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject AXInPlaceObject
        {
            get
            {
                return this.axOleInPlaceObject;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public override Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("WebBrowserBackgroundImageNotSupported"));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override ImageLayout BackgroundImageLayout
        {
            get
            {
                return base.BackgroundImageLayout;
            }
            set
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("WebBrowserBackgroundImageLayoutNotSupported"));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        internal ContainerControl ContainingControl
        {
            get
            {
                if ((this.containingControl == null) || this.GetAXHostState(WebBrowserHelper.recomputeContainingControl))
                {
                    this.containingControl = this.FindContainerControlInternal();
                }
                return this.containingControl;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override System.Windows.Forms.Cursor Cursor
        {
            get
            {
                return base.Cursor;
            }
            set
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("WebBrowserCursorNotSupported"));
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(0x4b, 0x17);
            }
        }

        private bool EditMode
        {
            get
            {
                return (this.axEditMode != WebBrowserHelper.AXEditMode.None);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("WebBrowserEnabledNotSupported"));
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public override System.Drawing.Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public System.Windows.Forms.ImeMode ImeMode
        {
            get
            {
                return base.ImeMode;
            }
            set
            {
                base.ImeMode = value;
            }
        }

        internal bool IsUserMode
        {
            get
            {
                if (this.Site != null)
                {
                    return !base.DesignMode;
                }
                return true;
            }
        }

        internal int NoComponentChangeEvents
        {
            get
            {
                return this.noComponentChange;
            }
            set
            {
                this.noComponentChange = value;
            }
        }

        [Browsable(false), Localizable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override System.Windows.Forms.RightToLeft RightToLeft
        {
            get
            {
                return System.Windows.Forms.RightToLeft.No;
            }
            set
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("WebBrowserRightToLeftNotSupported"));
            }
        }

        private EventHandler SelectionChangeHandler
        {
            get
            {
                if (this.selectionChangeHandler == null)
                {
                    this.selectionChangeHandler = new EventHandler(this.OnNewSelection);
                }
                return this.selectionChangeHandler;
            }
        }

        public override ISite Site
        {
            set
            {
                bool flag = this.RemoveSelectionHandler();
                base.Site = value;
                if (flag)
                {
                    this.AddSelectionHandler();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), Bindable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get
            {
                return "";
            }
            set
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("WebBrowserTextNotSupported"));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool UseWaitCursor
        {
            get
            {
                return base.UseWaitCursor;
            }
            set
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("WebBrowserUseWaitCursorNotSupported"));
            }
        }

        private class WebBrowserBaseNativeWindow : NativeWindow
        {
            private System.Windows.Forms.WebBrowserBase WebBrowserBase;

            public WebBrowserBaseNativeWindow(System.Windows.Forms.WebBrowserBase ax)
            {
                this.WebBrowserBase = ax;
            }

            private unsafe void WmWindowPosChanging(ref Message m)
            {
                System.Windows.Forms.NativeMethods.WINDOWPOS* lParam = (System.Windows.Forms.NativeMethods.WINDOWPOS*) m.LParam;
                lParam->x = 0;
                lParam->y = 0;
                Size webBrowserBaseChangingSize = this.WebBrowserBase.webBrowserBaseChangingSize;
                if (webBrowserBaseChangingSize.Width == -1)
                {
                    lParam->cx = this.WebBrowserBase.Width;
                    lParam->cy = this.WebBrowserBase.Height;
                }
                else
                {
                    lParam->cx = webBrowserBaseChangingSize.Width;
                    lParam->cy = webBrowserBaseChangingSize.Height;
                }
                m.Result = IntPtr.Zero;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 70)
                {
                    this.WmWindowPosChanging(ref m);
                }
                else
                {
                    base.WndProc(ref m);
                }
            }
        }
    }
}

