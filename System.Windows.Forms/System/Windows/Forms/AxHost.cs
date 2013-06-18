namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Threading;
    using System.Windows.Forms.ComponentModel.Com2Interop;
    using System.Windows.Forms.Design;

    [DesignTimeVisible(false), ComVisible(true), ToolboxItem(false), DefaultEvent("Enter"), Designer("System.Windows.Forms.Design.AxHostDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ClassInterface(ClassInterfaceType.AutoDispatch), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public abstract class AxHost : Control, ISupportInitialize, ICustomTypeDescriptor
    {
        private AboutBoxDelegate aboutBoxDelegate;
        private static readonly int addedSelectionHandler = BitVector32.CreateMask(manualUpdate);
        private static readonly int assignUniqueID = BitVector32.CreateMask(processingKeyUp);
        private Attribute[] attribsStash;
        private static BooleanSwitch AxAlwaysSaveSwitch = new BooleanSwitch("AxAlwaysSave", "ActiveX to save all controls regardless of their IsDirty function return value");
        private AxContainer axContainer;
        private static TraceSwitch AxHostSwitch = new TraceSwitch("AxHost", "ActiveX host creation");
        private static TraceSwitch AxHTraceSwitch = new TraceSwitch("AxHTrace", "ActiveX handle tracing");
        private static BooleanSwitch AxIgnoreTMSwitch = new BooleanSwitch("AxIgnoreTM", "ActiveX switch to ignore thread models");
        private static TraceSwitch AxPropTraceSwitch = new TraceSwitch("AxPropTrace", "ActiveX property tracing");
        private BitVector32 axState;
        private static CategoryAttribute[] categoryNames;
        private static readonly int checkedCP = BitVector32.CreateMask(checkedIppb);
        private static readonly int checkedIppb = BitVector32.CreateMask(refreshProperties);
        private Guid clsid;
        private static Guid comctlImageCombo_Clsid = new Guid("{a98a24c0-b06f-3684-8c12-c52ae341e0bc}");
        private AxContainer container;
        private ContainerControl containingControl;
        private static Guid dataSource_Guid = new Guid("{7C0FFAB3-CD84-11D0-949A-00A0C91110ED}");
        private static readonly int disposed = BitVector32.CreateMask(sinkAttached);
        private static COMException E_FAIL = new COMException(System.Windows.Forms.SR.GetString("AXUnknownError"), -2147467259);
        private static COMException E_INVALIDARG = new COMException(System.Windows.Forms.SR.GetString("AXInvalidArgument"), -2147024809);
        private static COMException E_NOINTERFACE = new COMException(System.Windows.Forms.SR.GetString("AxInterfaceNotSupported"), -2147467262);
        private static COMException E_NOTIMPL = new COMException(System.Windows.Forms.SR.GetString("AXNotImplemented"), -2147483647);
        private const int EDITM_HOST = 2;
        private const int EDITM_NONE = 0;
        private const int EDITM_OBJECT = 1;
        private int editMode;
        private AxComponentEditor editor;
        private static readonly int editorRefresh = BitVector32.CreateMask(ocxStateSet);
        private static readonly int fFakingWindow = BitVector32.CreateMask(fSimpleFrame);
        private int flags;
        private static readonly int fNeedOwnWindow = BitVector32.CreateMask(checkedCP);
        private static Hashtable fontTable;
        private static readonly int fOwnWindow = BitVector32.CreateMask(fNeedOwnWindow);
        private int freezeCount;
        private static readonly int fSimpleFrame = BitVector32.CreateMask(fOwnWindow);
        private static readonly int handlePosRectChanged = BitVector32.CreateMask(valueChanged);
        private const int HMperInch = 0x9ec;
        private IntPtr hwndFocus;
        private System.Windows.Forms.NativeMethods.ICategorizeProperties iCategorizeProperties;
        private static Guid icf2_Guid = typeof(System.Windows.Forms.UnsafeNativeMethods.IClassFactory2).GUID;
        private static Guid ifont_Guid = typeof(System.Windows.Forms.UnsafeNativeMethods.IFont).GUID;
        private static Guid ifontDisp_Guid = typeof(System.Windows.Forms.SafeNativeMethods.IFontDisp).GUID;
        private bool ignoreDialogKeys;
        private const int INPROC_SERVER = 1;
        private object instance;
        private static readonly int inTransition = BitVector32.CreateMask(needLicenseKey);
        private System.Windows.Forms.UnsafeNativeMethods.IOleControl iOleControl;
        private System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject iOleInPlaceActiveObject;
        private System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject iOleInPlaceActiveObjectExternal;
        private System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject iOleInPlaceObject;
        private System.Windows.Forms.UnsafeNativeMethods.IOleObject iOleObject;
        private static Guid ioleobject_Guid = typeof(System.Windows.Forms.UnsafeNativeMethods.IOleObject).GUID;
        private System.Windows.Forms.NativeMethods.IPerPropertyBrowsing iPerPropertyBrowsing;
        private System.Windows.Forms.UnsafeNativeMethods.IPersistPropertyBag iPersistPropBag;
        private System.Windows.Forms.UnsafeNativeMethods.IPersistStorage iPersistStorage;
        private System.Windows.Forms.UnsafeNativeMethods.IPersistStream iPersistStream;
        private System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit iPersistStreamInit;
        private static Guid ipicture_Guid = typeof(System.Windows.Forms.UnsafeNativeMethods.IPicture).GUID;
        private static Guid ipictureDisp_Guid = typeof(System.Windows.Forms.UnsafeNativeMethods.IPictureDisp).GUID;
        private bool isMaskEdit;
        private static Guid ivbformat_Guid = typeof(System.Windows.Forms.UnsafeNativeMethods.IVBFormat).GUID;
        private string licenseKey;
        private static readonly int listeningToIdle = BitVector32.CreateMask(editorRefresh);
        private static int logPixelsX = -1;
        private static int logPixelsY = -1;
        private static readonly int manualUpdate = BitVector32.CreateMask(disposed);
        private static Guid maskEdit_Clsid = new Guid("{c932ba85-4374-101b-a56c-00aa003668dc}");
        private int miscStatusBits;
        private static readonly int needLicenseKey = BitVector32.CreateMask(siteProcessedInputKey);
        private ContainerControl newParent;
        private int noComponentChange;
        private Hashtable objectDefinedCategoryNames;
        private const int OC_INPLACE = 4;
        private const int OC_LOADED = 1;
        private const int OC_OPEN = 0x10;
        private const int OC_PASSIVE = 0;
        private const int OC_RUNNING = 2;
        private const int OC_UIACTIVE = 8;
        private int ocState;
        private State ocxState;
        private static readonly int ocxStateSet = BitVector32.CreateMask();
        private const int OLEIVERB_HIDE = -3;
        private const int OLEIVERB_INPLACEACTIVATE = -5;
        private const int OLEIVERB_PRIMARY = 0;
        private const int OLEIVERB_PROPERTIES = -7;
        private const int OLEIVERB_SHOW = -1;
        private const int OLEIVERB_UIACTIVATE = -4;
        private readonly OleInterfaces oleSite;
        private EventHandler onContainerVisibleChanged;
        private static readonly int ownDisposing = BitVector32.CreateMask(rejectSelection);
        private static readonly int processingKeyUp = BitVector32.CreateMask(inTransition);
        private Hashtable properties;
        private Hashtable propertyInfos;
        private PropertyDescriptorCollection propsStash;
        private static readonly int refreshProperties = BitVector32.CreateMask(listeningToIdle);
        private readonly int REGMSG_MSG;
        private const int REGMSG_RETVAL = 0x7b;
        private static readonly int rejectSelection = BitVector32.CreateMask(fFakingWindow);
        private static readonly int renameEventHooked = BitVector32.CreateMask(assignUniqueID);
        private EventHandler selectionChangeHandler;
        private int selectionStyle;
        private static readonly int sinkAttached = BitVector32.CreateMask(ownDisposing);
        private static readonly int siteProcessedInputKey = BitVector32.CreateMask(handlePosRectChanged);
        private const int STG_STORAGE = 2;
        private const int STG_STREAM = 0;
        private const int STG_STREAMINIT = 1;
        private const int STG_UNKNOWN = -1;
        private int storageType;
        private string text;
        private static readonly int valueChanged = BitVector32.CreateMask(addedSelectionHandler);
        private static Guid windowsMediaPlayer_Clsid = new Guid("{22d6f312-b0f6-11d0-94ab-0080c74c7e95}");
        private IntPtr wndprocAddr;

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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler ContextMenuChanged
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "ContextMenuChanged" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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
        public event EventHandler MouseClick
        {
            add
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXAddInvalidEvent", new object[] { "MouseClick" }));
            }
            remove
            {
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler MouseDoubleClick
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        static AxHost()
        {
            CategoryAttribute[] attributeArray = new CategoryAttribute[12];
            attributeArray[1] = new WinCategoryAttribute("Default");
            attributeArray[2] = new WinCategoryAttribute("Default");
            attributeArray[3] = new WinCategoryAttribute("Font");
            attributeArray[4] = new WinCategoryAttribute("Layout");
            attributeArray[5] = new WinCategoryAttribute("Appearance");
            attributeArray[6] = new WinCategoryAttribute("Behavior");
            attributeArray[7] = new WinCategoryAttribute("Data");
            attributeArray[8] = new WinCategoryAttribute("List");
            attributeArray[9] = new WinCategoryAttribute("Text");
            attributeArray[10] = new WinCategoryAttribute("Scale");
            attributeArray[11] = new WinCategoryAttribute("DDE");
            categoryNames = attributeArray;
        }

        protected AxHost(string clsid) : this(clsid, 0)
        {
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        protected AxHost(string clsid, int flags)
        {
            this.REGMSG_MSG = System.Windows.Forms.SafeNativeMethods.RegisterWindowMessage(Application.WindowMessagesVersion + "_subclassCheck");
            this.axState = new BitVector32();
            this.storageType = -1;
            this.wndprocAddr = IntPtr.Zero;
            this.text = "";
            this.hwndFocus = IntPtr.Zero;
            if (Application.OleRequired() != ApartmentState.STA)
            {
                throw new ThreadStateException(System.Windows.Forms.SR.GetString("AXMTAThread", new object[] { clsid }));
            }
            this.oleSite = new OleInterfaces(this);
            this.selectionChangeHandler = new EventHandler(this.OnNewSelection);
            this.clsid = new Guid(clsid);
            this.flags = flags;
            this.axState[assignUniqueID] = !base.GetType().GUID.Equals(comctlImageCombo_Clsid);
            this.axState[needLicenseKey] = true;
            this.axState[rejectSelection] = true;
            this.isMaskEdit = this.clsid.Equals(maskEdit_Clsid);
            this.onContainerVisibleChanged = new EventHandler(this.OnContainerVisibleChanged);
        }

        private void ActivateAxControl()
        {
            if (this.QuickActivate())
            {
                this.DepersistControl();
            }
            else
            {
                this.SlowActivate();
            }
            this.SetOcState(2);
        }

        private void AddSelectionHandler()
        {
            if (!this.axState[addedSelectionHandler])
            {
                ISelectionService selectionService = this.GetSelectionService();
                if (selectionService != null)
                {
                    selectionService.SelectionChanging += this.selectionChangeHandler;
                }
                this.axState[addedSelectionHandler] = true;
            }
        }

        private void AmbientChanged(int dispid)
        {
            if (this.GetOcx() != null)
            {
                try
                {
                    base.Invalidate();
                    this.GetOleControl().OnAmbientPropertyChange(dispid);
                }
                catch (Exception)
                {
                }
            }
        }

        protected virtual void AttachInterfaces()
        {
        }

        private void AttachWindow(IntPtr hwnd)
        {
            if (!this.axState[fFakingWindow])
            {
                base.WindowAssignHandle(hwnd, this.axState[assignUniqueID]);
            }
            base.UpdateZOrder();
            Size size = base.Size;
            base.UpdateBounds();
            Size extent = this.GetExtent();
            Point location = base.Location;
            if ((size.Width < extent.Width) || (size.Height < extent.Height))
            {
                base.Bounds = new Rectangle(location.X, location.Y, extent.Width, extent.Height);
            }
            else
            {
                Size size3 = this.SetExtent(size.Width, size.Height);
                if (!size3.Equals(size))
                {
                    base.Bounds = new Rectangle(location.X, location.Y, size3.Width, size3.Height);
                }
            }
            this.OnHandleCreated(EventArgs.Empty);
            this.InformOfNewHandle();
        }

        private bool AwaitingDefreezing()
        {
            return (this.freezeCount > 0);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void BeginInit()
        {
        }

        internal override bool CanSelectCore()
        {
            return ((this.GetControlEnabled() && !this.axState[rejectSelection]) && base.CanSelectCore());
        }

        private bool CanShowPropertyPages()
        {
            if (this.GetOcState() < 2)
            {
                return false;
            }
            return (this.GetOcx() is System.Windows.Forms.NativeMethods.ISpecifyPropertyPages);
        }

        private bool CheckSubclassing()
        {
            if (!base.IsHandleCreated || (this.wndprocAddr == IntPtr.Zero))
            {
                return true;
            }
            IntPtr handle = base.Handle;
            IntPtr windowLong = System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, handle), -4);
            if (windowLong == this.wndprocAddr)
            {
                return true;
            }
            if (((int) ((long) base.SendMessage(this.REGMSG_MSG, 0, 0))) == 0x7b)
            {
                this.wndprocAddr = windowLong;
                return true;
            }
            base.WindowReleaseHandle();
            System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, handle), -4, new HandleRef(this, windowLong));
            base.WindowAssignHandle(handle, this.axState[assignUniqueID]);
            this.InformOfNewHandle();
            this.axState[manualUpdate] = true;
            return false;
        }

        private int Convert2int(object o, bool xDirection)
        {
            o = ((Array) o).GetValue(0);
            if (o.GetType() == typeof(float))
            {
                return Twip2Pixel(Convert.ToDouble(o, CultureInfo.InvariantCulture), xDirection);
            }
            return Convert.ToInt32(o, CultureInfo.InvariantCulture);
        }

        private short Convert2short(object o)
        {
            o = ((Array) o).GetValue(0);
            return Convert.ToInt16(o, CultureInfo.InvariantCulture);
        }

        protected override void CreateHandle()
        {
            if (!base.IsHandleCreated)
            {
                this.TransitionUpTo(2);
                if (!this.axState[fOwnWindow])
                {
                    if (!this.axState[fNeedOwnWindow])
                    {
                        this.TransitionUpTo(4);
                        if (this.axState[fNeedOwnWindow])
                        {
                            this.CreateHandle();
                            return;
                        }
                    }
                    else
                    {
                        this.axState[fNeedOwnWindow] = false;
                        this.axState[fFakingWindow] = true;
                        base.CreateHandle();
                    }
                }
                else
                {
                    base.SetState(2, false);
                    base.CreateHandle();
                }
                this.GetParentContainer().ControlCreated(this);
            }
        }

        private void CreateInstance()
        {
            try
            {
                this.instance = this.CreateInstanceCore(this.clsid);
            }
            catch (ExternalException exception)
            {
                if (exception.ErrorCode == -2147221230)
                {
                    throw new LicenseException(base.GetType(), this, System.Windows.Forms.SR.GetString("AXNoLicenseToUse"));
                }
                throw;
            }
            this.SetOcState(1);
        }

        protected virtual object CreateInstanceCore(Guid clsid)
        {
            if (this.IsUserMode())
            {
                this.CreateWithLicense(this.licenseKey, clsid);
            }
            else
            {
                this.CreateWithoutLicense(clsid);
            }
            return this.instance;
        }

        private State CreateNewOcxState(State oldOcxState)
        {
            this.NoComponentChangeEvents++;
            try
            {
                if (this.GetOcState() < 2)
                {
                    return null;
                }
                try
                {
                    PropertyBagStream pPropBag = null;
                    if (this.iPersistPropBag != null)
                    {
                        pPropBag = new PropertyBagStream();
                        this.iPersistPropBag.Save(pPropBag, true, true);
                    }
                    MemoryStream dataStream = null;
                    switch (this.storageType)
                    {
                        case 0:
                        case 1:
                            dataStream = new MemoryStream();
                            if (this.storageType != 0)
                            {
                                break;
                            }
                            this.iPersistStream.Save(new System.Windows.Forms.UnsafeNativeMethods.ComStreamFromDataStream(dataStream), true);
                            goto Label_00A9;

                        case 2:
                            if (oldOcxState == null)
                            {
                                return null;
                            }
                            return oldOcxState.RefreshStorage(this.iPersistStorage);

                        default:
                            return null;
                    }
                    this.iPersistStreamInit.Save(new System.Windows.Forms.UnsafeNativeMethods.ComStreamFromDataStream(dataStream), true);
                Label_00A9:
                    if (dataStream != null)
                    {
                        return new State(dataStream, this.storageType, this, pPropBag);
                    }
                    if (pPropBag != null)
                    {
                        return new State(pPropBag);
                    }
                }
                catch (Exception)
                {
                }
            }
            finally
            {
                this.NoComponentChangeEvents--;
            }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void CreateSink()
        {
        }

        private void CreateWithLicense(string license, Guid clsid)
        {
            if (license != null)
            {
                try
                {
                    System.Windows.Forms.UnsafeNativeMethods.IClassFactory2 factory = System.Windows.Forms.UnsafeNativeMethods.CoGetClassObject(ref clsid, 1, 0, ref icf2_Guid);
                    if (factory != null)
                    {
                        factory.CreateInstanceLic(null, null, ref System.Windows.Forms.NativeMethods.ActiveX.IID_IUnknown, license, out this.instance);
                    }
                }
                catch (Exception)
                {
                }
            }
            if (this.instance == null)
            {
                this.CreateWithoutLicense(clsid);
            }
        }

        private void CreateWithoutLicense(Guid clsid)
        {
            object obj2 = null;
            obj2 = System.Windows.Forms.UnsafeNativeMethods.CoCreateInstance(ref clsid, null, 1, ref System.Windows.Forms.NativeMethods.ActiveX.IID_IUnknown);
            this.instance = obj2;
        }

        private void DepersistControl()
        {
            this.Freeze(true);
            if (this.ocxState == null)
            {
                if (this.instance is System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit)
                {
                    this.iPersistStreamInit = (System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit) this.instance;
                    try
                    {
                        this.storageType = 1;
                        this.iPersistStreamInit.InitNew();
                    }
                    catch (Exception)
                    {
                    }
                    return;
                }
                if (this.instance is System.Windows.Forms.UnsafeNativeMethods.IPersistStream)
                {
                    this.storageType = 0;
                    this.iPersistStream = (System.Windows.Forms.UnsafeNativeMethods.IPersistStream) this.instance;
                    return;
                }
                if (this.instance is System.Windows.Forms.UnsafeNativeMethods.IPersistStorage)
                {
                    this.storageType = 2;
                    this.ocxState = new State(this);
                    this.iPersistStorage = (System.Windows.Forms.UnsafeNativeMethods.IPersistStorage) this.instance;
                    try
                    {
                        this.iPersistStorage.InitNew(this.ocxState.GetStorage());
                    }
                    catch (Exception)
                    {
                    }
                    return;
                }
                if (this.instance is System.Windows.Forms.UnsafeNativeMethods.IPersistPropertyBag)
                {
                    this.iPersistPropBag = (System.Windows.Forms.UnsafeNativeMethods.IPersistPropertyBag) this.instance;
                    try
                    {
                        this.iPersistPropBag.InitNew();
                    }
                    catch (Exception)
                    {
                    }
                }
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("UnableToInitComponent"));
            }
            switch (this.ocxState.Type)
            {
                case 0:
                    try
                    {
                        this.iPersistStream = (System.Windows.Forms.UnsafeNativeMethods.IPersistStream) this.instance;
                        this.DepersistFromIStream(this.ocxState.GetStream());
                    }
                    catch (Exception)
                    {
                    }
                    break;

                case 1:
                    if (this.instance is System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit)
                    {
                        try
                        {
                            this.iPersistStreamInit = (System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit) this.instance;
                            this.DepersistFromIStreamInit(this.ocxState.GetStream());
                        }
                        catch (Exception)
                        {
                        }
                        this.GetControlEnabled();
                        break;
                    }
                    this.ocxState.Type = 0;
                    this.DepersistControl();
                    return;

                case 2:
                    try
                    {
                        this.iPersistStorage = (System.Windows.Forms.UnsafeNativeMethods.IPersistStorage) this.instance;
                        this.DepersistFromIStorage(this.ocxState.GetStorage());
                    }
                    catch (Exception)
                    {
                    }
                    break;

                default:
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("UnableToInitComponent"));
            }
            if (this.ocxState.GetPropBag() != null)
            {
                try
                {
                    this.iPersistPropBag = (System.Windows.Forms.UnsafeNativeMethods.IPersistPropertyBag) this.instance;
                    this.DepersistFromIPropertyBag(this.ocxState.GetPropBag());
                }
                catch (Exception)
                {
                }
            }
        }

        private void DepersistFromIPropertyBag(System.Windows.Forms.UnsafeNativeMethods.IPropertyBag propBag)
        {
            this.iPersistPropBag.Load(propBag, null);
        }

        private void DepersistFromIStorage(System.Windows.Forms.UnsafeNativeMethods.IStorage storage)
        {
            this.storageType = 2;
            if (storage != null)
            {
                this.iPersistStorage.Load(storage);
            }
        }

        private void DepersistFromIStream(System.Windows.Forms.UnsafeNativeMethods.IStream istream)
        {
            this.storageType = 0;
            this.iPersistStream.Load(istream);
        }

        private void DepersistFromIStreamInit(System.Windows.Forms.UnsafeNativeMethods.IStream istream)
        {
            this.storageType = 1;
            this.iPersistStreamInit.Load(istream);
        }

        private void DestroyFakeWindow()
        {
            this.axState[fFakingWindow] = false;
            base.DestroyHandle();
        }

        protected override void DestroyHandle()
        {
            if (this.axState[fOwnWindow])
            {
                base.DestroyHandle();
            }
            else if (base.IsHandleCreated)
            {
                this.TransitionDownTo(2);
            }
        }

        private void DetachAndForward(ref Message m)
        {
            IntPtr handleNoCreate = this.GetHandleNoCreate();
            this.DetachWindow();
            if (handleNoCreate != IntPtr.Zero)
            {
                IntPtr windowLong = System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, handleNoCreate), -4);
                m.Result = System.Windows.Forms.UnsafeNativeMethods.CallWindowProc(windowLong, handleNoCreate, m.Msg, m.WParam, m.LParam);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void DetachSink()
        {
        }

        private void DetachWindow()
        {
            if (base.IsHandleCreated)
            {
                this.OnHandleDestroyed(EventArgs.Empty);
                for (Control control = this; control != null; control = control.ParentInternal)
                {
                }
                base.WindowReleaseHandle();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.TransitionDownTo(0);
                if (this.newParent != null)
                {
                    this.newParent.Dispose();
                }
                if (this.oleSite != null)
                {
                    this.oleSite.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void DisposeAxControl()
        {
            if (this.GetParentContainer() != null)
            {
                this.GetParentContainer().RemoveControl(this);
            }
            this.TransitionDownTo(2);
            if (this.GetOcState() == 2)
            {
                this.GetOleObject().SetClientSite(null);
                this.SetOcState(1);
            }
        }

        internal override void DisposeAxControls()
        {
            this.axState[rejectSelection] = true;
            base.DisposeAxControls();
            this.TransitionDownTo(0);
        }

        public void DoVerb(int verb)
        {
            Control parentInternal = this.ParentInternal;
            this.GetOleObject().DoVerb(verb, IntPtr.Zero, this.oleSite, -1, (parentInternal != null) ? parentInternal.Handle : IntPtr.Zero, FillInRect(new System.Windows.Forms.NativeMethods.COMRECT(), base.Bounds));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DrawToBitmap(Bitmap bitmap, Rectangle targetBounds)
        {
            base.DrawToBitmap(bitmap, targetBounds);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void EndInit()
        {
            if (this.ParentInternal != null)
            {
                this.ParentInternal.CreateControl(true);
                ContainerControl containingControl = this.ContainingControl;
                if (containingControl != null)
                {
                    containingControl.VisibleChanged += this.onContainerVisibleChanged;
                }
            }
        }

        private void EnsureWindowPresent()
        {
            if (!base.IsHandleCreated)
            {
                try
                {
                    ((System.Windows.Forms.UnsafeNativeMethods.IOleClientSite) this.oleSite).ShowObject();
                }
                catch
                {
                }
            }
            if (!base.IsHandleCreated && (this.ParentInternal != null))
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXNohWnd", new object[] { base.GetType().Name }));
            }
        }

        private static System.Windows.Forms.NativeMethods.COMRECT FillInRect(System.Windows.Forms.NativeMethods.COMRECT dest, Rectangle source)
        {
            dest.left = source.X;
            dest.top = source.Y;
            dest.right = source.Width + source.X;
            dest.bottom = source.Height + source.Y;
            return dest;
        }

        private PropertyDescriptorCollection FillProperties(Attribute[] attributes)
        {
            if (this.RefreshAllProperties)
            {
                this.RefreshAllProperties = false;
                this.propsStash = null;
                this.attribsStash = null;
            }
            else if (this.propsStash != null)
            {
                if ((attributes == null) && (this.attribsStash == null))
                {
                    return this.propsStash;
                }
                if (((attributes != null) && (this.attribsStash != null)) && (attributes.Length == this.attribsStash.Length))
                {
                    bool flag = true;
                    int num = 0;
                    foreach (Attribute attribute in attributes)
                    {
                        if (!attribute.Equals(this.attribsStash[num++]))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        return this.propsStash;
                    }
                }
            }
            ArrayList list = new ArrayList();
            if (this.properties == null)
            {
                this.properties = new Hashtable();
            }
            if (this.propertyInfos == null)
            {
                this.propertyInfos = new Hashtable();
                foreach (PropertyInfo info in base.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    this.propertyInfos.Add(info.Name, info);
                }
            }
            PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(this, null, true);
            if (descriptors != null)
            {
                for (int i = 0; i < descriptors.Count; i++)
                {
                    if (descriptors[i].DesignTimeOnly)
                    {
                        list.Add(descriptors[i]);
                    }
                    else
                    {
                        string name = descriptors[i].Name;
                        PropertyDescriptor descriptor = null;
                        PropertyInfo info2 = (PropertyInfo) this.propertyInfos[name];
                        if ((info2 == null) || info2.CanRead)
                        {
                            if (!this.properties.ContainsKey(name))
                            {
                                if (info2 != null)
                                {
                                    descriptor = new AxPropertyDescriptor(descriptors[i], this);
                                    ((AxPropertyDescriptor) descriptor).UpdateAttributes();
                                }
                                else
                                {
                                    descriptor = descriptors[i];
                                }
                                this.properties.Add(name, descriptor);
                                list.Add(descriptor);
                            }
                            else
                            {
                                PropertyDescriptor descriptor2 = (PropertyDescriptor) this.properties[name];
                                AxPropertyDescriptor descriptor3 = descriptor2 as AxPropertyDescriptor;
                                if (((info2 != null) || (descriptor3 == null)) && ((info2 == null) || (descriptor3 != null)))
                                {
                                    if (descriptor3 != null)
                                    {
                                        descriptor3.UpdateAttributes();
                                    }
                                    list.Add(descriptor2);
                                }
                            }
                        }
                    }
                }
                if (attributes != null)
                {
                    Attribute attribute2 = null;
                    foreach (Attribute attribute3 in attributes)
                    {
                        if (attribute3 is BrowsableAttribute)
                        {
                            attribute2 = attribute3;
                        }
                    }
                    if (attribute2 != null)
                    {
                        ArrayList list2 = null;
                        foreach (PropertyDescriptor descriptor4 in list)
                        {
                            if (descriptor4 is AxPropertyDescriptor)
                            {
                                Attribute attribute4 = descriptor4.Attributes[typeof(BrowsableAttribute)];
                                if ((attribute4 != null) && !attribute4.Equals(attribute2))
                                {
                                    if (list2 == null)
                                    {
                                        list2 = new ArrayList();
                                    }
                                    list2.Add(descriptor4);
                                }
                            }
                        }
                        if (list2 != null)
                        {
                            foreach (object obj2 in list2)
                            {
                                list.Remove(obj2);
                            }
                        }
                    }
                }
            }
            PropertyDescriptor[] array = new PropertyDescriptor[list.Count];
            list.CopyTo(array, 0);
            this.propsStash = new PropertyDescriptorCollection(array);
            this.attribsStash = attributes;
            return this.propsStash;
        }

        private ContainerControl FindContainerControlInternal()
        {
            if (this.Site != null)
            {
                IDesignerHost service = (IDesignerHost) this.Site.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    ContainerControl rootComponent = service.RootComponent as ContainerControl;
                    if (rootComponent != null)
                    {
                        return rootComponent;
                    }
                }
            }
            for (Control control3 = this; control3 != null; control3 = control3.ParentInternal)
            {
                ContainerControl control4 = control3 as ContainerControl;
                if (control4 != null)
                {
                    return control4;
                }
            }
            return null;
        }

        private void Freeze(bool v)
        {
            if (v)
            {
                try
                {
                    this.GetOleControl().FreezeEvents(-1);
                }
                catch (COMException)
                {
                }
                this.freezeCount++;
            }
            else
            {
                try
                {
                    this.GetOleControl().FreezeEvents(0);
                }
                catch (COMException)
                {
                }
                this.freezeCount--;
            }
        }

        private object GetAmbientProperty(int dispid)
        {
            Control parentInternal = this.ParentInternal;
            switch (dispid)
            {
                case -715:
                    return true;

                case -713:
                    return false;

                case -712:
                    return false;

                case -711:
                    return false;

                case -710:
                    return false;

                case -709:
                    return this.IsUserMode();

                case -706:
                    return true;

                case -705:
                    return Thread.CurrentThread.CurrentCulture.LCID;

                case -704:
                    if (parentInternal == null)
                    {
                        return null;
                    }
                    return GetOleColorFromColor(parentInternal.ForeColor);

                case -703:
                    if (parentInternal == null)
                    {
                        return null;
                    }
                    return GetIFontFromFont(parentInternal.Font);

                case -702:
                {
                    string nameForControl = this.GetParentContainer().GetNameForControl(this);
                    if (nameForControl == null)
                    {
                        nameForControl = "";
                    }
                    return nameForControl;
                }
                case -701:
                    if (parentInternal == null)
                    {
                        return null;
                    }
                    return GetOleColorFromColor(parentInternal.BackColor);

                case -732:
                {
                    Control parent = this;
                    while (parent != null)
                    {
                        if (parent.RightToLeft == System.Windows.Forms.RightToLeft.No)
                        {
                            return false;
                        }
                        if (parent.RightToLeft == System.Windows.Forms.RightToLeft.Yes)
                        {
                            return true;
                        }
                        if (parent.RightToLeft == System.Windows.Forms.RightToLeft.Inherit)
                        {
                            parent = parent.Parent;
                        }
                    }
                    return null;
                }
            }
            return null;
        }

        private bool GetAxState(int mask)
        {
            return this.axState[mask];
        }

        private System.Windows.Forms.NativeMethods.ICategorizeProperties GetCategorizeProperties()
        {
            if (((this.iCategorizeProperties == null) && !this.axState[checkedCP]) && (this.instance != null))
            {
                this.axState[checkedCP] = true;
                if (this.instance is System.Windows.Forms.NativeMethods.ICategorizeProperties)
                {
                    this.iCategorizeProperties = (System.Windows.Forms.NativeMethods.ICategorizeProperties) this.instance;
                }
            }
            return this.iCategorizeProperties;
        }

        private CategoryAttribute GetCategoryForDispid(int dispid)
        {
            System.Windows.Forms.NativeMethods.ICategorizeProperties categorizeProperties = this.GetCategorizeProperties();
            if (categorizeProperties != null)
            {
                CategoryAttribute attribute = null;
                int categoryID = 0;
                try
                {
                    categorizeProperties.MapPropertyToCategory(dispid, ref categoryID);
                    if (categoryID != 0)
                    {
                        int index = -categoryID;
                        if (((index > 0) && (index < categoryNames.Length)) && (categoryNames[index] != null))
                        {
                            return categoryNames[index];
                        }
                        index = -index;
                        int key = index;
                        if (this.objectDefinedCategoryNames != null)
                        {
                            attribute = (CategoryAttribute) this.objectDefinedCategoryNames[key];
                            if (attribute != null)
                            {
                                return attribute;
                            }
                        }
                        string categoryName = null;
                        if ((categorizeProperties.GetCategoryName(index, CultureInfo.CurrentCulture.LCID, out categoryName) == 0) && (categoryName != null))
                        {
                            attribute = new CategoryAttribute(categoryName);
                            if (this.objectDefinedCategoryNames == null)
                            {
                                this.objectDefinedCategoryNames = new Hashtable();
                            }
                            this.objectDefinedCategoryNames.Add(key, attribute);
                            return attribute;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            return null;
        }

        private System.Windows.Forms.NativeMethods.COMRECT GetClipRect(System.Windows.Forms.NativeMethods.COMRECT clipRect)
        {
            if (clipRect != null)
            {
                FillInRect(clipRect, new Rectangle(0, 0, 0x7d00, 0x7d00));
            }
            return clipRect;
        }

        [CLSCompliant(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        protected static Color GetColorFromOleColor(uint color)
        {
            return ColorTranslator.FromOle((int) color);
        }

        private bool GetControlEnabled()
        {
            try
            {
                return base.IsHandleCreated;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private Size GetExtent()
        {
            System.Windows.Forms.NativeMethods.tagSIZEL pSizel = new System.Windows.Forms.NativeMethods.tagSIZEL();
            this.GetOleObject().GetExtent(1, pSizel);
            this.HiMetric2Pixel(pSizel, pSizel);
            return new Size(pSizel.cx, pSizel.cy);
        }

        private static System.Windows.Forms.NativeMethods.FONTDESC GetFONTDESCFromFont(System.Drawing.Font font)
        {
            System.Windows.Forms.NativeMethods.FONTDESC fontdesc = null;
            if (fontTable == null)
            {
                fontTable = new Hashtable();
            }
            else
            {
                fontdesc = (System.Windows.Forms.NativeMethods.FONTDESC) fontTable[font];
            }
            if (fontdesc == null)
            {
                fontdesc = new System.Windows.Forms.NativeMethods.FONTDESC {
                    lpstrName = font.Name,
                    cySize = (long) (font.SizeInPoints * 10000f)
                };
                System.Windows.Forms.NativeMethods.LOGFONT logFont = new System.Windows.Forms.NativeMethods.LOGFONT();
                font.ToLogFont(logFont);
                fontdesc.sWeight = (short) logFont.lfWeight;
                fontdesc.sCharset = logFont.lfCharSet;
                fontdesc.fItalic = font.Italic;
                fontdesc.fUnderline = font.Underline;
                fontdesc.fStrikethrough = font.Strikeout;
                fontTable[font] = fontdesc;
            }
            return fontdesc;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected static System.Drawing.Font GetFontFromIFont(object font)
        {
            if (font == null)
            {
                return null;
            }
            System.Windows.Forms.UnsafeNativeMethods.IFont font2 = (System.Windows.Forms.UnsafeNativeMethods.IFont) font;
            try
            {
                System.Drawing.Font font3 = System.Drawing.Font.FromHfont(font2.GetHFont());
                if (font3.Unit != GraphicsUnit.Point)
                {
                    font3 = new System.Drawing.Font(font3.Name, font3.SizeInPoints, font3.Style, GraphicsUnit.Point, font3.GdiCharSet, font3.GdiVerticalFont);
                }
                return font3;
            }
            catch (Exception)
            {
                return Control.DefaultFont;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected static System.Drawing.Font GetFontFromIFontDisp(object font)
        {
            if (font == null)
            {
                return null;
            }
            System.Windows.Forms.UnsafeNativeMethods.IFont font2 = font as System.Windows.Forms.UnsafeNativeMethods.IFont;
            if (font2 != null)
            {
                return GetFontFromIFont(font2);
            }
            System.Windows.Forms.SafeNativeMethods.IFontDisp disp = (System.Windows.Forms.SafeNativeMethods.IFontDisp) font;
            FontStyle regular = FontStyle.Regular;
            try
            {
                if (disp.Bold)
                {
                    regular |= FontStyle.Bold;
                }
                if (disp.Italic)
                {
                    regular |= FontStyle.Italic;
                }
                if (disp.Underline)
                {
                    regular |= FontStyle.Underline;
                }
                if (disp.Strikethrough)
                {
                    regular |= FontStyle.Strikeout;
                }
                if (disp.Weight >= 700)
                {
                    regular |= FontStyle.Bold;
                }
                return new System.Drawing.Font(disp.Name, ((float) disp.Size) / 10000f, regular, GraphicsUnit.Point, (byte) disp.Charset);
            }
            catch (Exception)
            {
                return Control.DefaultFont;
            }
        }

        private IntPtr GetHandleNoCreate()
        {
            if (base.IsHandleCreated)
            {
                return base.Handle;
            }
            return IntPtr.Zero;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected static object GetIFontDispFromFont(System.Drawing.Font font)
        {
            if (font == null)
            {
                return null;
            }
            if (font.Unit != GraphicsUnit.Point)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("AXFontUnitNotPoint"), "font");
            }
            return System.Windows.Forms.SafeNativeMethods.OleCreateIFontDispIndirect(GetFONTDESCFromFont(font), ref ifontDisp_Guid);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected static object GetIFontFromFont(System.Drawing.Font font)
        {
            if (font == null)
            {
                return null;
            }
            if (font.Unit != GraphicsUnit.Point)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("AXFontUnitNotPoint"), "font");
            }
            try
            {
                return System.Windows.Forms.UnsafeNativeMethods.OleCreateIFontIndirect(GetFONTDESCFromFont(font), ref ifont_Guid);
            }
            catch
            {
                return null;
            }
        }

        private System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject GetInPlaceActiveObject()
        {
            if (this.iOleInPlaceActiveObjectExternal != null)
            {
                return this.iOleInPlaceActiveObjectExternal;
            }
            if (this.iOleInPlaceActiveObject == null)
            {
                try
                {
                    this.iOleInPlaceActiveObject = (System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject) this.instance;
                }
                catch (InvalidCastException)
                {
                }
            }
            return this.iOleInPlaceActiveObject;
        }

        private System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject GetInPlaceObject()
        {
            if (this.iOleInPlaceObject == null)
            {
                this.iOleInPlaceObject = (System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject) this.instance;
            }
            return this.iOleInPlaceObject;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected static object GetIPictureDispFromPicture(Image image)
        {
            if (image == null)
            {
                return null;
            }
            return System.Windows.Forms.UnsafeNativeMethods.OleCreateIPictureDispIndirect(GetPICTDESCFromPicture(image), ref ipictureDisp_Guid, true);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected static object GetIPictureFromCursor(System.Windows.Forms.Cursor cursor)
        {
            if (cursor == null)
            {
                return null;
            }
            System.Windows.Forms.NativeMethods.PICTDESCicon pictdesc = new System.Windows.Forms.NativeMethods.PICTDESCicon(Icon.FromHandle(cursor.Handle));
            return System.Windows.Forms.UnsafeNativeMethods.OleCreateIPictureIndirect(pictdesc, ref ipicture_Guid, true);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected static object GetIPictureFromPicture(Image image)
        {
            if (image == null)
            {
                return null;
            }
            return System.Windows.Forms.UnsafeNativeMethods.OleCreateIPictureIndirect(GetPICTDESCFromPicture(image), ref ipicture_Guid, true);
        }

        private string GetLicenseKey()
        {
            return this.GetLicenseKey(this.clsid);
        }

        private string GetLicenseKey(Guid clsid)
        {
            if ((this.licenseKey != null) || !this.axState[needLicenseKey])
            {
                return this.licenseKey;
            }
            try
            {
                System.Windows.Forms.UnsafeNativeMethods.IClassFactory2 factory = System.Windows.Forms.UnsafeNativeMethods.CoGetClassObject(ref clsid, 1, 0, ref icf2_Guid);
                System.Windows.Forms.NativeMethods.tagLICINFO licInfo = new System.Windows.Forms.NativeMethods.tagLICINFO();
                factory.GetLicInfo(licInfo);
                if (licInfo.fRuntimeAvailable != 0)
                {
                    string[] pBstrKey = new string[1];
                    factory.RequestLicKey(0, pBstrKey);
                    this.licenseKey = pBstrKey[0];
                    return this.licenseKey;
                }
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == E_NOINTERFACE.ErrorCode)
                {
                    return null;
                }
                this.axState[needLicenseKey] = false;
            }
            catch (Exception)
            {
                this.axState[needLicenseKey] = false;
            }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected static double GetOADateFromTime(DateTime time)
        {
            return time.ToOADate();
        }

        private int GetOcState()
        {
            return this.ocState;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public object GetOcx()
        {
            return this.instance;
        }

        private object GetOcxCreate()
        {
            if (this.instance == null)
            {
                this.CreateInstance();
                this.RealizeStyles();
                this.AttachInterfaces();
                this.oleSite.OnOcxCreate();
            }
            return this.instance;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), CLSCompliant(false)]
        protected static uint GetOleColorFromColor(Color color)
        {
            return (uint) ColorTranslator.ToOle(color);
        }

        private System.Windows.Forms.UnsafeNativeMethods.IOleControl GetOleControl()
        {
            if (this.iOleControl == null)
            {
                this.iOleControl = (System.Windows.Forms.UnsafeNativeMethods.IOleControl) this.instance;
            }
            return this.iOleControl;
        }

        private System.Windows.Forms.UnsafeNativeMethods.IOleObject GetOleObject()
        {
            if (this.iOleObject == null)
            {
                this.iOleObject = (System.Windows.Forms.UnsafeNativeMethods.IOleObject) this.instance;
            }
            return this.iOleObject;
        }

        private AxContainer GetParentContainer()
        {
            System.Windows.Forms.IntSecurity.GetParent.Demand();
            if (this.container == null)
            {
                this.container = AxContainer.FindContainerForControl(this);
            }
            if (this.container == null)
            {
                ContainerControl containingControl = this.ContainingControl;
                if (containingControl == null)
                {
                    if (this.newParent == null)
                    {
                        this.newParent = new ContainerControl();
                        this.axContainer = this.newParent.CreateAxContainer();
                        this.axContainer.AddControl(this);
                    }
                    return this.axContainer;
                }
                this.container = containingControl.CreateAxContainer();
                this.container.AddControl(this);
                this.containingControl = containingControl;
            }
            return this.container;
        }

        private System.Windows.Forms.NativeMethods.IPerPropertyBrowsing GetPerPropertyBrowsing()
        {
            if (((this.iPerPropertyBrowsing == null) && !this.axState[checkedIppb]) && (this.instance != null))
            {
                this.axState[checkedIppb] = true;
                if (this.instance is System.Windows.Forms.NativeMethods.IPerPropertyBrowsing)
                {
                    this.iPerPropertyBrowsing = (System.Windows.Forms.NativeMethods.IPerPropertyBrowsing) this.instance;
                }
            }
            return this.iPerPropertyBrowsing;
        }

        private static object GetPICTDESCFromPicture(Image image)
        {
            Bitmap bitmap = image as Bitmap;
            if (bitmap != null)
            {
                return new System.Windows.Forms.NativeMethods.PICTDESCbmp(bitmap);
            }
            Metafile metafile = image as Metafile;
            if (metafile == null)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("AXUnknownImage"), "image");
            }
            return new System.Windows.Forms.NativeMethods.PICTDESCemf(metafile);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected static Image GetPictureFromIPicture(object picture)
        {
            if (picture == null)
            {
                return null;
            }
            IntPtr zero = IntPtr.Zero;
            System.Windows.Forms.UnsafeNativeMethods.IPicture pict = (System.Windows.Forms.UnsafeNativeMethods.IPicture) picture;
            int pictureType = pict.GetPictureType();
            if (pictureType == 1)
            {
                try
                {
                    zero = pict.GetHPal();
                }
                catch (COMException)
                {
                }
            }
            return GetPictureFromParams(pict, pict.GetHandle(), pictureType, zero, pict.GetWidth(), pict.GetHeight());
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected static Image GetPictureFromIPictureDisp(object picture)
        {
            if (picture == null)
            {
                return null;
            }
            IntPtr zero = IntPtr.Zero;
            System.Windows.Forms.UnsafeNativeMethods.IPictureDisp pict = (System.Windows.Forms.UnsafeNativeMethods.IPictureDisp) picture;
            int pictureType = pict.PictureType;
            if (pictureType == 1)
            {
                try
                {
                    zero = pict.HPal;
                }
                catch (COMException)
                {
                }
            }
            return GetPictureFromParams(pict, pict.Handle, pictureType, zero, pict.Width, pict.Height);
        }

        private static Image GetPictureFromParams(object pict, IntPtr handle, int type, IntPtr paletteHandle, int width, int height)
        {
            switch (type)
            {
                case -1:
                    return null;

                case 0:
                    return null;

                case 1:
                    return Image.FromHbitmap(handle, paletteHandle);

                case 2:
                {
                    WmfPlaceableFileHeader wmfHeader = new WmfPlaceableFileHeader {
                        BboxRight = (short) width,
                        BboxBottom = (short) height
                    };
                    return (Image) new Metafile(handle, wmfHeader, false).Clone();
                }
                case 3:
                    return (Image) Icon.FromHandle(handle).Clone();

                case 4:
                    return (Image) new Metafile(handle, false).Clone();
            }
            throw new ArgumentException(System.Windows.Forms.SR.GetString("AXUnknownImage"), "type");
        }

        private AxPropertyDescriptor GetPropertyDescriptorFromDispid(int dispid)
        {
            foreach (PropertyDescriptor descriptor in this.FillProperties(null))
            {
                AxPropertyDescriptor descriptor2 = descriptor as AxPropertyDescriptor;
                if ((descriptor2 != null) && (descriptor2.Dispid == dispid))
                {
                    return descriptor2;
                }
            }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, BoundsSpecified specified)
        {
            return bounds;
        }

        private ISelectionService GetSelectionService()
        {
            return GetSelectionService(this);
        }

        private static ISelectionService GetSelectionService(Control ctl)
        {
            ISite site = ctl.Site;
            if (site != null)
            {
                return (site.GetService(typeof(ISelectionService)) as ISelectionService);
            }
            return null;
        }

        private bool GetSiteOwnsDeactivation()
        {
            return this.axState[ownDisposing];
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected static DateTime GetTimeFromOADate(double date)
        {
            return DateTime.FromOADate(date);
        }

        public bool HasPropertyPages()
        {
            if (this.CanShowPropertyPages())
            {
                System.Windows.Forms.NativeMethods.ISpecifyPropertyPages ocx = (System.Windows.Forms.NativeMethods.ISpecifyPropertyPages) this.GetOcx();
                try
                {
                    System.Windows.Forms.NativeMethods.tagCAUUID pPages = new System.Windows.Forms.NativeMethods.tagCAUUID();
                    try
                    {
                        ocx.GetPages(pPages);
                        if (pPages.cElems > 0)
                        {
                            return true;
                        }
                    }
                    finally
                    {
                        if (pPages.pElems != IntPtr.Zero)
                        {
                            Marshal.FreeCoTaskMem(pPages.pElems);
                        }
                    }
                }
                catch
                {
                }
            }
            return false;
        }

        private void HideAxControl()
        {
            this.DoVerb(-3);
            if (this.GetOcState() < 4)
            {
                this.axState[fNeedOwnWindow] = true;
                this.SetOcState(4);
            }
        }

        private void HiMetric2Pixel(System.Windows.Forms.NativeMethods.tagSIZEL sz, System.Windows.Forms.NativeMethods.tagSIZEL szout)
        {
            System.Windows.Forms.NativeMethods._POINTL pPtlHimetric = new System.Windows.Forms.NativeMethods._POINTL {
                x = sz.cx,
                y = sz.cy
            };
            System.Windows.Forms.NativeMethods.tagPOINTF pPtfContainer = new System.Windows.Forms.NativeMethods.tagPOINTF();
            ((System.Windows.Forms.UnsafeNativeMethods.IOleControlSite) this.oleSite).TransformCoords(pPtlHimetric, pPtfContainer, 6);
            szout.cx = (int) pPtfContainer.x;
            szout.cy = (int) pPtfContainer.y;
        }

        private int HM2Pix(int hm, int logP)
        {
            return (((logP * hm) + 0x4f6) / 0x9ec);
        }

        private void InformOfNewHandle()
        {
            for (Control control = this; control != null; control = control.ParentInternal)
            {
            }
            this.wndprocAddr = System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, base.Handle), -4);
        }

        internal override IntPtr InitializeDCForWmCtlColor(IntPtr dc, int msg)
        {
            if (this.isMaskEdit)
            {
                return base.InitializeDCForWmCtlColor(dc, msg);
            }
            return IntPtr.Zero;
        }

        private void InPlaceActivate()
        {
            try
            {
                this.DoVerb(-5);
            }
            catch (Exception exception)
            {
                throw new TargetInvocationException(System.Windows.Forms.SR.GetString("AXNohWnd", new object[] { base.GetType().Name }), exception);
            }
            this.EnsureWindowPresent();
        }

        private void InPlaceDeactivate()
        {
            this.axState[ownDisposing] = true;
            ContainerControl containingControl = this.ContainingControl;
            if ((containingControl != null) && (containingControl.ActiveControl == this))
            {
                containingControl.ActiveControl = null;
            }
            try
            {
                this.GetInPlaceObject().InPlaceDeactivate();
            }
            catch (Exception)
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void InvokeEditMode()
        {
            if (this.editMode == 0)
            {
                this.AddSelectionHandler();
                this.editMode = 2;
                this.SetSelectionStyle(2);
                System.Windows.Forms.UnsafeNativeMethods.GetFocus();
                try
                {
                    this.UiActivate();
                }
                catch (Exception)
                {
                }
            }
        }

        private bool IsDirty()
        {
            if (this.GetOcState() < 2)
            {
                return false;
            }
            if (this.axState[valueChanged])
            {
                this.axState[valueChanged] = false;
                return true;
            }
            int hr = -2147467259;
            switch (this.storageType)
            {
                case 0:
                    hr = this.iPersistStream.IsDirty();
                    break;

                case 1:
                    hr = this.iPersistStreamInit.IsDirty();
                    break;

                case 2:
                    hr = this.iPersistStorage.IsDirty();
                    break;

                default:
                    return true;
            }
            if (hr == 1)
            {
                return false;
            }
            return (System.Windows.Forms.NativeMethods.Failed(hr) || true);
        }

        [UIPermission(SecurityAction.InheritanceDemand, Window=UIPermissionWindow.AllWindows)]
        protected override bool IsInputChar(char charCode)
        {
            return true;
        }

        internal bool IsUserMode()
        {
            ISite site = this.Site;
            if (site != null)
            {
                return !site.DesignMode;
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void MakeDirty()
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

        private void MakeVisibleWithShow()
        {
            ContainerControl containingControl = this.ContainingControl;
            Control control2 = (containingControl == null) ? null : containingControl.ActiveControl;
            try
            {
                this.DoVerb(-1);
            }
            catch (Exception exception)
            {
                throw new TargetInvocationException(System.Windows.Forms.SR.GetString("AXNohWnd", new object[] { base.GetType().Name }), exception);
            }
            this.EnsureWindowPresent();
            base.CreateControl(true);
            if ((containingControl != null) && (containingControl.ActiveControl != control2))
            {
                containingControl.ActiveControl = control2;
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            this.AmbientChanged(-701);
        }

        private void OnComponentRename(object sender, ComponentRenameEventArgs e)
        {
            if (e.Component == this)
            {
                System.Windows.Forms.UnsafeNativeMethods.IOleControl ocx = this.GetOcx() as System.Windows.Forms.UnsafeNativeMethods.IOleControl;
                if (ocx != null)
                {
                    ocx.OnAmbientPropertyChange(-702);
                }
            }
        }

        private void OnContainerVisibleChanged(object sender, EventArgs e)
        {
            ContainerControl containingControl = this.ContainingControl;
            if (containingControl != null)
            {
                if ((containingControl.Visible && base.Visible) && !this.axState[fOwnWindow])
                {
                    this.MakeVisibleWithShow();
                }
                else if ((!containingControl.Visible && base.Visible) && (base.IsHandleCreated && (this.GetOcState() >= 4)))
                {
                    this.HideAxControl();
                }
                else if ((containingControl.Visible && !base.GetState(2)) && (base.IsHandleCreated && (this.GetOcState() >= 4)))
                {
                    this.HideAxControl();
                }
            }
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

        protected override void OnHandleCreated(EventArgs e)
        {
            if (Application.OleRequired() != ApartmentState.STA)
            {
                throw new ThreadStateException(System.Windows.Forms.SR.GetString("ThreadMustBeSTA"));
            }
            base.SetAcceptDrops(this.AllowDrop);
            base.RaiseCreateHandleEvent(e);
        }

        private void OnIdle(object sender, EventArgs e)
        {
            if (this.axState[refreshProperties])
            {
                TypeDescriptor.Refresh(base.GetType());
            }
        }

        protected virtual void OnInPlaceActive()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnLostFocus(EventArgs e)
        {
            bool flag = this.GetHandleNoCreate() != this.hwndFocus;
            if (flag && base.IsHandleCreated)
            {
                flag = !System.Windows.Forms.UnsafeNativeMethods.IsChild(new HandleRef(this, this.GetHandleNoCreate()), new HandleRef(null, this.hwndFocus));
            }
            base.OnLostFocus(e);
            if (flag)
            {
                this.UiDeactivate();
            }
        }

        private void OnNewSelection(object sender, EventArgs e)
        {
            if (!this.IsUserMode())
            {
                ISelectionService selectionService = this.GetSelectionService();
                if (selectionService != null)
                {
                    if ((this.GetOcState() >= 8) && !selectionService.GetComponentSelected(this))
                    {
                        System.Windows.Forms.NativeMethods.Failed(this.UiDeactivate());
                    }
                    if (!selectionService.GetComponentSelected(this))
                    {
                        if (this.editMode != 0)
                        {
                            this.GetParentContainer().OnExitEditMode(this);
                            this.editMode = 0;
                        }
                        this.SetSelectionStyle(1);
                        this.RemoveSelectionHandler();
                    }
                    else
                    {
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this)["SelectionStyle"];
                        if ((descriptor != null) && (descriptor.PropertyType == typeof(int)))
                        {
                            int num2 = (int) descriptor.GetValue(this);
                            if (num2 != this.selectionStyle)
                            {
                                descriptor.SetValue(this, this.selectionStyle);
                            }
                        }
                    }
                }
            }
        }

        private bool OwnWindow()
        {
            if (!this.axState[fOwnWindow])
            {
                return this.axState[fFakingWindow];
            }
            return true;
        }

        private void ParseMiscBits(int bits)
        {
            this.axState[fOwnWindow] = ((bits & 0x400) != 0) && this.IsUserMode();
            this.axState[fSimpleFrame] = (bits & 0x10000) != 0;
        }

        private int Pix2HM(int pix, int logP)
        {
            return (((0x9ec * pix) + (logP >> 1)) / logP);
        }

        private void Pixel2hiMetric(System.Windows.Forms.NativeMethods.tagSIZEL sz, System.Windows.Forms.NativeMethods.tagSIZEL szout)
        {
            System.Windows.Forms.NativeMethods.tagPOINTF pPtfContainer = new System.Windows.Forms.NativeMethods.tagPOINTF {
                x = sz.cx,
                y = sz.cy
            };
            System.Windows.Forms.NativeMethods._POINTL pPtlHimetric = new System.Windows.Forms.NativeMethods._POINTL();
            ((System.Windows.Forms.UnsafeNativeMethods.IOleControlSite) this.oleSite).TransformCoords(pPtlHimetric, pPtfContainer, 10);
            szout.cx = pPtlHimetric.x;
            szout.cy = pPtlHimetric.y;
        }

        private static int Pixel2Twip(int v, bool xDirection)
        {
            SetupLogPixels(false);
            int num = xDirection ? logPixelsX : logPixelsY;
            return (int) (((((double) v) / ((double) num)) * 72.0) * 20.0);
        }

        public override bool PreProcessMessage(ref Message msg)
        {
            if (this.IsUserMode())
            {
                if (this.axState[siteProcessedInputKey])
                {
                    return base.PreProcessMessage(ref msg);
                }
                System.Windows.Forms.NativeMethods.MSG lpmsg = new System.Windows.Forms.NativeMethods.MSG {
                    message = msg.Msg,
                    wParam = msg.WParam,
                    lParam = msg.LParam,
                    hwnd = msg.HWnd
                };
                this.axState[siteProcessedInputKey] = false;
                try
                {
                    System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject inPlaceActiveObject = this.GetInPlaceActiveObject();
                    if (inPlaceActiveObject != null)
                    {
                        int num = inPlaceActiveObject.TranslateAccelerator(ref lpmsg);
                        msg.Msg = lpmsg.message;
                        msg.WParam = lpmsg.wParam;
                        msg.LParam = lpmsg.lParam;
                        msg.HWnd = lpmsg.hwnd;
                        switch (num)
                        {
                            case 0:
                                return true;

                            case 1:
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
                        }
                        return (this.axState[siteProcessedInputKey] && base.PreProcessMessage(ref msg));
                    }
                }
                finally
                {
                    this.axState[siteProcessedInputKey] = false;
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
            if (base.CanSelect)
            {
                try
                {
                    System.Windows.Forms.NativeMethods.tagCONTROLINFO pCI = new System.Windows.Forms.NativeMethods.tagCONTROLINFO();
                    if (System.Windows.Forms.NativeMethods.Failed(this.GetOleControl().GetControlInfo(pCI)))
                    {
                        return false;
                    }
                    System.Windows.Forms.NativeMethods.MSG lpMsg = new System.Windows.Forms.NativeMethods.MSG {
                        hwnd = (this.ContainingControl == null) ? IntPtr.Zero : this.ContainingControl.Handle,
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
                        this.GetOleControl().OnMnemonic(ref lpMsg);
                        base.Focus();
                        return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected bool PropsValid()
        {
            return this.CanAccessProperties;
        }

        private bool QuickActivate()
        {
            if (!(this.instance is System.Windows.Forms.UnsafeNativeMethods.IQuickActivate))
            {
                return false;
            }
            System.Windows.Forms.UnsafeNativeMethods.IQuickActivate instance = (System.Windows.Forms.UnsafeNativeMethods.IQuickActivate) this.instance;
            System.Windows.Forms.UnsafeNativeMethods.tagQACONTAINER pQaContainer = new System.Windows.Forms.UnsafeNativeMethods.tagQACONTAINER();
            System.Windows.Forms.UnsafeNativeMethods.tagQACONTROL pQaControl = new System.Windows.Forms.UnsafeNativeMethods.tagQACONTROL();
            pQaContainer.pClientSite = this.oleSite;
            pQaContainer.pPropertyNotifySink = this.oleSite;
            pQaContainer.pFont = GetIFontFromFont(this.GetParentContainer().parent.Font);
            pQaContainer.dwAppearance = 0;
            pQaContainer.lcid = Application.CurrentCulture.LCID;
            Control parentInternal = this.ParentInternal;
            if (parentInternal != null)
            {
                pQaContainer.colorFore = GetOleColorFromColor(parentInternal.ForeColor);
                pQaContainer.colorBack = GetOleColorFromColor(parentInternal.BackColor);
            }
            else
            {
                pQaContainer.colorFore = GetOleColorFromColor(SystemColors.WindowText);
                pQaContainer.colorBack = GetOleColorFromColor(SystemColors.Window);
            }
            pQaContainer.dwAmbientFlags = 0xe0;
            if (this.IsUserMode())
            {
                pQaContainer.dwAmbientFlags |= 4;
            }
            try
            {
                instance.QuickActivate(pQaContainer, pQaControl);
            }
            catch (Exception)
            {
                this.DisposeAxControl();
                return false;
            }
            this.miscStatusBits = pQaControl.dwMiscStatus;
            this.ParseMiscBits(this.miscStatusBits);
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void RaiseOnMouseDown(short button, short shift, int x, int y)
        {
            base.OnMouseDown(new MouseEventArgs((MouseButtons) (button << 20), 1, x, y, 0));
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void RaiseOnMouseDown(short button, short shift, float x, float y)
        {
            this.RaiseOnMouseDown(button, shift, Twip2Pixel((int) x, true), Twip2Pixel((int) y, false));
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void RaiseOnMouseDown(object o1, object o2, object o3, object o4)
        {
            this.RaiseOnMouseDown(this.Convert2short(o1), this.Convert2short(o2), this.Convert2int(o3, true), this.Convert2int(o4, false));
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void RaiseOnMouseMove(short button, short shift, int x, int y)
        {
            base.OnMouseMove(new MouseEventArgs((MouseButtons) (button << 20), 1, x, y, 0));
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void RaiseOnMouseMove(short button, short shift, float x, float y)
        {
            this.RaiseOnMouseMove(button, shift, Twip2Pixel((int) x, true), Twip2Pixel((int) y, false));
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void RaiseOnMouseMove(object o1, object o2, object o3, object o4)
        {
            this.RaiseOnMouseMove(this.Convert2short(o1), this.Convert2short(o2), this.Convert2int(o3, true), this.Convert2int(o4, false));
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void RaiseOnMouseUp(short button, short shift, int x, int y)
        {
            base.OnMouseUp(new MouseEventArgs((MouseButtons) (button << 20), 1, x, y, 0));
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void RaiseOnMouseUp(short button, short shift, float x, float y)
        {
            this.RaiseOnMouseUp(button, shift, Twip2Pixel((int) x, true), Twip2Pixel((int) y, false));
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void RaiseOnMouseUp(object o1, object o2, object o3, object o4)
        {
            this.RaiseOnMouseUp(this.Convert2short(o1), this.Convert2short(o2), this.Convert2int(o3, true), this.Convert2int(o4, false));
        }

        private void RealizeStyles()
        {
            base.SetStyle(ControlStyles.UserPaint, false);
            int misc = 0;
            if (!System.Windows.Forms.NativeMethods.Failed(this.GetOleObject().GetMiscStatus(1, out misc)))
            {
                this.miscStatusBits = misc;
                this.ParseMiscBits(this.miscStatusBits);
            }
        }

        private void ReleaseAxControl()
        {
            this.NoComponentChangeEvents++;
            ContainerControl containingControl = this.ContainingControl;
            if (containingControl != null)
            {
                containingControl.VisibleChanged -= this.onContainerVisibleChanged;
            }
            try
            {
                if (this.instance != null)
                {
                    Marshal.FinalReleaseComObject(this.instance);
                    this.instance = null;
                    this.iOleInPlaceObject = null;
                    this.iOleObject = null;
                    this.iOleControl = null;
                    this.iOleInPlaceActiveObject = null;
                    this.iOleInPlaceActiveObjectExternal = null;
                    this.iPerPropertyBrowsing = null;
                    this.iCategorizeProperties = null;
                    this.iPersistStream = null;
                    this.iPersistStreamInit = null;
                    this.iPersistStorage = null;
                }
                this.axState[checkedIppb] = false;
                this.axState[checkedCP] = false;
                this.axState[disposed] = true;
                this.freezeCount = 0;
                this.axState[sinkAttached] = false;
                this.wndprocAddr = IntPtr.Zero;
                this.SetOcState(0);
            }
            finally
            {
                this.NoComponentChangeEvents--;
            }
        }

        private bool RemoveSelectionHandler()
        {
            if (!this.axState[addedSelectionHandler])
            {
                return false;
            }
            ISelectionService selectionService = this.GetSelectionService();
            if (selectionService != null)
            {
                selectionService.SelectionChanging -= this.selectionChangeHandler;
            }
            this.axState[addedSelectionHandler] = false;
            return true;
        }

        protected void SetAboutBoxDelegate(AboutBoxDelegate d)
        {
            this.aboutBoxDelegate = (AboutBoxDelegate) Delegate.Combine(this.aboutBoxDelegate, d);
        }

        private void SetAxState(int mask, bool value)
        {
            this.axState[mask] = value;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (!this.GetAxState(handlePosRectChanged))
            {
                this.axState[handlePosRectChanged] = true;
                Size size = base.ApplySizeConstraints(width, height);
                width = size.Width;
                height = size.Height;
                try
                {
                    if (this.axState[fFakingWindow])
                    {
                        base.SetBoundsCore(x, y, width, height, specified);
                    }
                    else
                    {
                        Rectangle bounds = base.Bounds;
                        if (((bounds.X != x) || (bounds.Y != y)) || ((bounds.Width != width) || (bounds.Height != height)))
                        {
                            if (!base.IsHandleCreated)
                            {
                                base.UpdateBounds(x, y, width, height);
                            }
                            else
                            {
                                if (this.GetOcState() > 2)
                                {
                                    this.CheckSubclassing();
                                    if ((width != bounds.Width) || (height != bounds.Height))
                                    {
                                        Size size2 = this.SetExtent(width, height);
                                        width = size2.Width;
                                        height = size2.Height;
                                    }
                                }
                                if (this.axState[manualUpdate])
                                {
                                    this.SetObjectRects(new Rectangle(x, y, width, height));
                                    this.CheckSubclassing();
                                    base.UpdateBounds();
                                }
                                else
                                {
                                    this.SetObjectRects(new Rectangle(x, y, width, height));
                                    base.SetBoundsCore(x, y, width, height, specified);
                                    base.Invalidate();
                                }
                            }
                        }
                    }
                }
                finally
                {
                    this.axState[handlePosRectChanged] = false;
                }
            }
        }

        private Size SetExtent(int width, int height)
        {
            System.Windows.Forms.NativeMethods.tagSIZEL sz = new System.Windows.Forms.NativeMethods.tagSIZEL {
                cx = width,
                cy = height
            };
            bool flag = !this.IsUserMode();
            try
            {
                this.Pixel2hiMetric(sz, sz);
                this.GetOleObject().SetExtent(1, sz);
            }
            catch (COMException)
            {
                flag = true;
            }
            if (flag)
            {
                this.GetOleObject().GetExtent(1, sz);
                try
                {
                    this.GetOleObject().SetExtent(1, sz);
                }
                catch (COMException)
                {
                }
            }
            return this.GetExtent();
        }

        private void SetObjectRects(Rectangle bounds)
        {
            if (this.GetOcState() >= 4)
            {
                this.GetInPlaceObject().SetObjectRects(FillInRect(new System.Windows.Forms.NativeMethods.COMRECT(), bounds), this.GetClipRect(new System.Windows.Forms.NativeMethods.COMRECT()));
            }
        }

        private void SetOcState(int nv)
        {
            this.ocState = nv;
        }

        private void SetSelectionStyle(int selectionStyle)
        {
            if (!this.IsUserMode())
            {
                ISelectionService selectionService = this.GetSelectionService();
                this.selectionStyle = selectionStyle;
                if ((selectionService != null) && selectionService.GetComponentSelected(this))
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this)["SelectionStyle"];
                    if ((descriptor != null) && (descriptor.PropertyType == typeof(int)))
                    {
                        descriptor.SetValue(this, selectionStyle);
                    }
                }
            }
        }

        private static int SetupLogPixels(bool force)
        {
            if ((logPixelsX == -1) || force)
            {
                IntPtr dC = System.Windows.Forms.UnsafeNativeMethods.GetDC(System.Windows.Forms.NativeMethods.NullHandleRef);
                if (dC == IntPtr.Zero)
                {
                    return -2147467259;
                }
                logPixelsX = System.Windows.Forms.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, dC), 0x58);
                logPixelsY = System.Windows.Forms.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, dC), 90);
                System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(System.Windows.Forms.NativeMethods.NullHandleRef, new HandleRef(null, dC));
            }
            return 0;
        }

        protected override void SetVisibleCore(bool value)
        {
            if (base.GetState(2) != value)
            {
                bool visible = base.Visible;
                if ((base.IsHandleCreated || value) && (((this.ParentInternal != null) && this.ParentInternal.Created) && !this.axState[fOwnWindow]))
                {
                    this.TransitionUpTo(2);
                    if (value)
                    {
                        if (this.axState[fFakingWindow])
                        {
                            this.DestroyFakeWindow();
                        }
                        if (!base.IsHandleCreated)
                        {
                            try
                            {
                                this.SetExtent(base.Width, base.Height);
                                this.InPlaceActivate();
                                base.CreateControl(true);
                            }
                            catch
                            {
                                this.MakeVisibleWithShow();
                            }
                        }
                        else
                        {
                            this.MakeVisibleWithShow();
                        }
                    }
                    else
                    {
                        this.HideAxControl();
                    }
                }
                if (!value)
                {
                    this.axState[fNeedOwnWindow] = false;
                }
                if (!this.axState[fOwnWindow])
                {
                    base.SetState(2, value);
                    if (base.Visible != visible)
                    {
                        this.OnVisibleChanged(EventArgs.Empty);
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private bool ShouldSerializeContainingControl()
        {
            return (this.ContainingControl != this.ParentInternal);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal override bool ShouldSerializeText()
        {
            bool flag = false;
            try
            {
                flag = this.Text.Length != 0;
            }
            catch (COMException)
            {
            }
            return flag;
        }

        public void ShowAboutBox()
        {
            if (this.aboutBoxDelegate != null)
            {
                this.aboutBoxDelegate();
            }
        }

        private unsafe void ShowPropertyPageForDispid(int dispid, Guid guid)
        {
            try
            {
                IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(this.GetOcx());
                System.Windows.Forms.NativeMethods.OCPFIPARAMS p = new System.Windows.Forms.NativeMethods.OCPFIPARAMS {
                    hwndOwner = (this.ContainingControl == null) ? IntPtr.Zero : this.ContainingControl.Handle,
                    lpszCaption = base.Name,
                    ppUnk = (IntPtr) ((ulong) ((IntPtr) &iUnknownForObject)),
                    uuid = (IntPtr) ((ulong) ((IntPtr) &guid)),
                    dispidInitial = dispid
                };
                System.Windows.Forms.UnsafeNativeMethods.OleCreatePropertyFrameIndirect(p);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void ShowPropertyPages()
        {
            if ((this.ParentInternal != null) && this.ParentInternal.IsHandleCreated)
            {
                this.ShowPropertyPages(this.ParentInternal);
            }
        }

        public void ShowPropertyPages(Control control)
        {
            try
            {
                if (this.CanShowPropertyPages())
                {
                    System.Windows.Forms.NativeMethods.ISpecifyPropertyPages ocx = (System.Windows.Forms.NativeMethods.ISpecifyPropertyPages) this.GetOcx();
                    System.Windows.Forms.NativeMethods.tagCAUUID pPages = new System.Windows.Forms.NativeMethods.tagCAUUID();
                    try
                    {
                        ocx.GetPages(pPages);
                        if (pPages.cElems <= 0)
                        {
                            return;
                        }
                    }
                    catch
                    {
                        return;
                    }
                    IDesignerHost service = null;
                    if (this.Site != null)
                    {
                        service = (IDesignerHost) this.Site.GetService(typeof(IDesignerHost));
                    }
                    DesignerTransaction transaction = null;
                    try
                    {
                        if (service != null)
                        {
                            transaction = service.CreateTransaction(System.Windows.Forms.SR.GetString("AXEditProperties"));
                        }
                        string caption = null;
                        object pobjs = this.GetOcx();
                        IntPtr handle = (this.ContainingControl == null) ? IntPtr.Zero : this.ContainingControl.Handle;
                        System.Windows.Forms.SafeNativeMethods.OleCreatePropertyFrame(new HandleRef(this, handle), 0, 0, caption, 1, ref pobjs, pPages.cElems, new HandleRef(null, pPages.pElems), Application.CurrentCulture.LCID, 0, IntPtr.Zero);
                    }
                    finally
                    {
                        if (this.oleSite != null)
                        {
                            ((System.Windows.Forms.UnsafeNativeMethods.IPropertyNotifySink) this.oleSite).OnChanged(-1);
                        }
                        if (transaction != null)
                        {
                            transaction.Commit();
                        }
                        if (pPages.pElems != IntPtr.Zero)
                        {
                            Marshal.FreeCoTaskMem(pPages.pElems);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        private void SlowActivate()
        {
            bool flag = false;
            if ((this.miscStatusBits & 0x20000) != 0)
            {
                this.GetOleObject().SetClientSite(this.oleSite);
                flag = true;
            }
            this.DepersistControl();
            if (!flag)
            {
                this.GetOleObject().SetClientSite(this.oleSite);
            }
        }

        private void StartEvents()
        {
            if (!this.axState[sinkAttached])
            {
                try
                {
                    this.CreateSink();
                    this.oleSite.StartEvents();
                }
                catch (Exception)
                {
                }
                this.axState[sinkAttached] = true;
            }
        }

        private void StopEvents()
        {
            if (this.axState[sinkAttached])
            {
                try
                {
                    this.DetachSink();
                }
                catch (Exception)
                {
                }
                this.axState[sinkAttached] = false;
            }
            this.oleSite.StopEvents();
        }

        private void SyncRenameNotification(bool hook)
        {
            if (base.DesignMode && (hook != this.axState[renameEventHooked]))
            {
                IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    if (hook)
                    {
                        service.ComponentRename += new ComponentRenameEventHandler(this.OnComponentRename);
                    }
                    else
                    {
                        service.ComponentRename -= new ComponentRenameEventHandler(this.OnComponentRename);
                    }
                    this.axState[renameEventHooked] = hook;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            if (!this.axState[editorRefresh] && this.HasPropertyPages())
            {
                this.axState[editorRefresh] = true;
                TypeDescriptor.Refresh(base.GetType());
            }
            return TypeDescriptor.GetAttributes(this, true);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string ICustomTypeDescriptor.GetClassName()
        {
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string ICustomTypeDescriptor.GetComponentName()
        {
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        object ICustomTypeDescriptor.GetEditor(System.Type editorBaseType)
        {
            if (editorBaseType != typeof(ComponentEditor))
            {
                return null;
            }
            if ((this.editor == null) && ((this.editor == null) && this.HasPropertyPages()))
            {
                this.editor = new AxComponentEditor();
            }
            return this.editor;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return this.FillProperties(null);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return this.FillProperties(attributes);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        private void TransitionDownTo(int state)
        {
            if (!this.axState[inTransition])
            {
                try
                {
                    this.axState[inTransition] = true;
                    while (state < this.GetOcState())
                    {
                        int ocState = this.GetOcState();
                        switch (ocState)
                        {
                            case 1:
                            {
                                this.ReleaseAxControl();
                                this.SetOcState(0);
                                continue;
                            }
                            case 2:
                            {
                                this.StopEvents();
                                this.DisposeAxControl();
                                this.SetOcState(1);
                                continue;
                            }
                            case 3:
                                goto Label_00BE;

                            case 4:
                                if (!this.axState[fFakingWindow])
                                {
                                    break;
                                }
                                this.DestroyFakeWindow();
                                this.SetOcState(2);
                                goto Label_0091;

                            case 8:
                            {
                                this.UiDeactivate();
                                this.SetOcState(4);
                                continue;
                            }
                            default:
                            {
                                if (ocState != 0x10)
                                {
                                    goto Label_00BE;
                                }
                                this.SetOcState(8);
                                continue;
                            }
                        }
                        this.InPlaceDeactivate();
                    Label_0091:
                        this.SetOcState(2);
                        continue;
                    Label_00BE:
                        this.SetOcState(this.GetOcState() - 1);
                    }
                }
                finally
                {
                    this.axState[inTransition] = false;
                }
            }
        }

        private void TransitionUpTo(int state)
        {
            if (!this.axState[inTransition])
            {
                try
                {
                    this.axState[inTransition] = true;
                    while (state > this.GetOcState())
                    {
                        switch (this.GetOcState())
                        {
                            case 0:
                            {
                                this.axState[disposed] = false;
                                this.GetOcxCreate();
                                this.SetOcState(1);
                                continue;
                            }
                            case 1:
                            {
                                this.ActivateAxControl();
                                this.SetOcState(2);
                                if (this.IsUserMode())
                                {
                                    this.StartEvents();
                                }
                                continue;
                            }
                            case 2:
                                this.axState[ownDisposing] = false;
                                if (!this.axState[fOwnWindow])
                                {
                                    this.InPlaceActivate();
                                    if ((base.Visible || (this.ContainingControl == null)) || !this.ContainingControl.Visible)
                                    {
                                        break;
                                    }
                                    this.HideAxControl();
                                }
                                goto Label_017F;

                            case 4:
                            {
                                this.DoVerb(-1);
                                this.SetOcState(8);
                                continue;
                            }
                            default:
                                goto Label_01A7;
                        }
                        base.CreateControl(true);
                        if (!this.IsUserMode() && !this.axState[ocxStateSet])
                        {
                            Size extent = this.GetExtent();
                            Rectangle bounds = base.Bounds;
                            if (bounds.Size.Equals(this.DefaultSize) && !bounds.Size.Equals(extent))
                            {
                                bounds.Width = extent.Width;
                                bounds.Height = extent.Height;
                                base.Bounds = bounds;
                            }
                        }
                    Label_017F:
                        if (this.GetOcState() < 4)
                        {
                            this.SetOcState(4);
                        }
                        this.OnInPlaceActive();
                        continue;
                    Label_01A7:
                        this.SetOcState(this.GetOcState() + 1);
                    }
                }
                finally
                {
                    this.axState[inTransition] = false;
                }
            }
        }

        private static int Twip2Pixel(double v, bool xDirection)
        {
            SetupLogPixels(false);
            int num = xDirection ? logPixelsX : logPixelsY;
            return (int) (((v / 20.0) / 72.0) * num);
        }

        private static int Twip2Pixel(int v, bool xDirection)
        {
            SetupLogPixels(false);
            int num = xDirection ? logPixelsX : logPixelsY;
            return (int) (((((double) v) / 20.0) / 72.0) * num);
        }

        private void UiActivate()
        {
            if (this.CanUIActivate)
            {
                this.DoVerb(-4);
            }
        }

        private int UiDeactivate()
        {
            bool flag = this.axState[ownDisposing];
            this.axState[ownDisposing] = true;
            int num = 0;
            try
            {
                num = this.GetInPlaceObject().UIDeactivate();
            }
            finally
            {
                this.axState[ownDisposing] = flag;
            }
            return num;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 20:
                case 0x15:
                case 0x20:
                case 0x2b:
                case 0x202:
                case 0x203:
                case 0x205:
                case 0x206:
                case 520:
                case 0x209:
                case 0x2055:
                    this.DefWndProc(ref m);
                    return;

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

                case 2:
                {
                    if (this.GetOcState() >= 4)
                    {
                        IntPtr ptr;
                        System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject inPlaceObject = this.GetInPlaceObject();
                        if (System.Windows.Forms.NativeMethods.Succeeded(inPlaceObject.GetWindow(out ptr)))
                        {
                            Application.ParkHandle(new HandleRef(inPlaceObject, ptr));
                        }
                    }
                    bool state = base.GetState(2);
                    this.TransitionDownTo(2);
                    this.DetachAndForward(ref m);
                    if (state != base.GetState(2))
                    {
                        base.SetState(2, state);
                    }
                    return;
                }
                case 0x53:
                    base.WndProc(ref m);
                    this.DefWndProc(ref m);
                    return;

                case 0x7b:
                    this.DefWndProc(ref m);
                    return;

                case 130:
                    goto Label_01CF;

                case 0x101:
                    if (this.axState[processingKeyUp])
                    {
                        return;
                    }
                    this.axState[processingKeyUp] = true;
                    try
                    {
                        if (base.PreProcessControlMessage(ref m) != PreProcessControlState.MessageProcessed)
                        {
                            this.DefWndProc(ref m);
                        }
                        return;
                    }
                    finally
                    {
                        this.axState[processingKeyUp] = false;
                    }
                    goto Label_01CF;

                case 0x201:
                case 0x204:
                case 0x207:
                    if (this.IsUserMode())
                    {
                        base.Focus();
                    }
                    this.DefWndProc(ref m);
                    return;

                case 0x111:
                    break;

                default:
                    if (m.Msg == this.REGMSG_MSG)
                    {
                        m.Result = (IntPtr) 0x7b;
                        return;
                    }
                    base.WndProc(ref m);
                    return;
            }
            if (!Control.ReflectMessageInternal(m.LParam, ref m))
            {
                this.DefWndProc(ref m);
            }
            return;
        Label_01CF:
            this.DetachAndForward(ref m);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                base.BackgroundImage = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ImageLayout BackgroundImageLayout
        {
            get
            {
                return base.BackgroundImageLayout;
            }
            set
            {
                base.BackgroundImageLayout = value;
            }
        }

        internal override bool CanAccessProperties
        {
            get
            {
                int ocState = this.GetOcState();
                return ((this.axState[fOwnWindow] && ((ocState > 2) || (this.IsUserMode() && (ocState >= 2)))) || (ocState >= 4));
            }
        }

        private bool CanUIActivate
        {
            get
            {
                if (!this.IsUserMode())
                {
                    return (this.editMode != 0);
                }
                return true;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public ContainerControl ContainingControl
        {
            get
            {
                System.Windows.Forms.IntSecurity.GetParent.Demand();
                if (this.containingControl == null)
                {
                    this.containingControl = this.FindContainerControlInternal();
                }
                return this.containingControl;
            }
            set
            {
                this.containingControl = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override System.Windows.Forms.ContextMenu ContextMenu
        {
            get
            {
                return base.ContextMenu;
            }
            set
            {
                base.ContextMenu = value;
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                if (this.axState[fOwnWindow] && this.IsUserMode())
                {
                    createParams.Style &= -268435457;
                }
                return createParams;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override System.Windows.Forms.Cursor Cursor
        {
            get
            {
                return base.Cursor;
            }
            set
            {
                base.Cursor = value;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(0x4b, 0x17);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool EditMode
        {
            get
            {
                return (this.editMode != 0);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public virtual bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool HasAboutBox
        {
            get
            {
                return (this.aboutBoxDelegate != null);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
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

        private int NoComponentChangeEvents
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

        [DefaultValue((string) null), EditorBrowsable(EditorBrowsableState.Advanced), RefreshProperties(RefreshProperties.All), Browsable(false)]
        public State OcxState
        {
            get
            {
                if (this.IsDirty() || (this.ocxState == null))
                {
                    this.ocxState = this.CreateNewOcxState(this.ocxState);
                }
                return this.ocxState;
            }
            set
            {
                this.axState[ocxStateSet] = true;
                if (value != null)
                {
                    if ((this.storageType != -1) && (this.storageType != value.type))
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("AXOcxStateLoaded"));
                    }
                    if (this.ocxState != value)
                    {
                        this.ocxState = value;
                        if (this.ocxState != null)
                        {
                            this.axState[manualUpdate] = this.ocxState._GetManualUpdate();
                            this.licenseKey = this.ocxState._GetLicenseKey();
                        }
                        else
                        {
                            this.axState[manualUpdate] = false;
                            this.licenseKey = null;
                        }
                        if ((this.ocxState != null) && (this.GetOcState() >= 2))
                        {
                            this.DepersistControl();
                        }
                    }
                }
            }
        }

        private bool RefreshAllProperties
        {
            get
            {
                return this.axState[refreshProperties];
            }
            set
            {
                this.axState[refreshProperties] = value;
                if (value && !this.axState[listeningToIdle])
                {
                    Application.Idle += new EventHandler(this.OnIdle);
                    this.axState[listeningToIdle] = true;
                }
                else if (!value && this.axState[listeningToIdle])
                {
                    Application.Idle -= new EventHandler(this.OnIdle);
                    this.axState[listeningToIdle] = false;
                }
            }
        }

        [Localizable(true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool RightToLeft
        {
            get
            {
                return (base.RightToLeft == System.Windows.Forms.RightToLeft.Yes);
            }
            set
            {
                base.RightToLeft = value ? System.Windows.Forms.RightToLeft.Yes : System.Windows.Forms.RightToLeft.No;
            }
        }

        public override ISite Site
        {
            set
            {
                if (!this.axState[disposed])
                {
                    bool flag = this.RemoveSelectionHandler();
                    bool flag2 = this.IsUserMode();
                    this.SyncRenameNotification(false);
                    base.Site = value;
                    bool flag3 = this.IsUserMode();
                    if (!flag3)
                    {
                        this.GetOcxCreate();
                    }
                    if (flag)
                    {
                        this.AddSelectionHandler();
                    }
                    this.SyncRenameNotification(value != null);
                    if (((value != null) && !flag3) && ((flag2 != flag3) && (this.GetOcState() > 1)))
                    {
                        this.TransitionDownTo(1);
                        this.TransitionUpTo(4);
                        ContainerControl containingControl = this.ContainingControl;
                        if (((containingControl != null) && containingControl.Visible) && base.Visible)
                        {
                            this.MakeVisibleWithShow();
                        }
                    }
                    if (((flag2 != flag3) && !base.IsHandleCreated) && (!this.axState[disposed] && (this.GetOcx() != null)))
                    {
                        this.RealizeStyles();
                    }
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
            }
        }

        protected delegate void AboutBoxDelegate();

        public enum ActiveXInvokeKind
        {
            MethodInvoke,
            PropertyGet,
            PropertySet
        }

        [ComVisible(false), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        public class AxComponentEditor : WindowsFormsComponentEditor
        {
            public override bool EditComponent(ITypeDescriptorContext context, object obj, IWin32Window parent)
            {
                AxHost host = obj as AxHost;
                if (host != null)
                {
                    try
                    {
                        ((System.Windows.Forms.UnsafeNativeMethods.IOleControlSite) host.oleSite).ShowPropertyFrame();
                        return true;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                return false;
            }
        }

        internal class AxContainer : System.Windows.Forms.UnsafeNativeMethods.IOleContainer, System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame, IReflect
        {
            private IContainer assocContainer;
            private Hashtable components;
            private Hashtable containerCache = new Hashtable();
            private AxHost ctlInEditMode;
            private bool formAlreadyCreated;
            private const int GC_CHILD = 1;
            private const int GC_CONTAINER = 0x20;
            private const int GC_FIRSTSIBLING = 4;
            private const int GC_LASTSIBLING = 2;
            private const int GC_NEXTSIBLING = 0x80;
            private const int GC_PREVSIBLING = 0x40;
            private int lockCount;
            internal ContainerControl parent;
            private Hashtable proxyCache;
            private AxHost siteActive;
            private AxHost siteUIActive;

            internal AxContainer(ContainerControl parent)
            {
                this.parent = parent;
                if (parent.Created)
                {
                    this.FormCreated();
                }
            }

            internal void AddControl(Control ctl)
            {
                lock (this)
                {
                    if (this.containerCache.Contains(ctl))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("AXDuplicateControl", new object[] { this.GetNameForControl(ctl) }), "ctl");
                    }
                    this.containerCache.Add(ctl, ctl);
                    if (this.assocContainer == null)
                    {
                        ISite site = ctl.Site;
                        if (site != null)
                        {
                            this.assocContainer = site.Container;
                            IComponentChangeService service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                            if (service != null)
                            {
                                service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                            }
                        }
                    }
                }
            }

            internal void ControlCreated(AxHost invoker)
            {
                if (this.formAlreadyCreated)
                {
                    if (invoker.IsUserMode() && invoker.AwaitingDefreezing())
                    {
                        invoker.Freeze(false);
                    }
                }
                else
                {
                    this.parent.CreateAxContainer();
                }
            }

            internal System.Windows.Forms.UnsafeNativeMethods.IEnumUnknown EnumControls(Control ctl, int dwOleContF, int dwWhich)
            {
                System.Windows.Forms.UnsafeNativeMethods.IEnumUnknown unknown;
                this.GetComponents();
                this.LockComponents();
                try
                {
                    AxHost.AxContainer container;
                    ArrayList l = null;
                    bool selected = (dwWhich & 0x40000000) != 0;
                    bool flag2 = (dwWhich & 0x8000000) != 0;
                    bool flag3 = (dwWhich & 0x10000000) != 0;
                    bool flag4 = (dwWhich & 0x20000000) != 0;
                    dwWhich &= -2013265921;
                    if (flag3 && flag4)
                    {
                        throw AxHost.E_INVALIDARG;
                    }
                    if (((dwWhich == 2) || (dwWhich == 3)) && (flag3 || flag4))
                    {
                        throw AxHost.E_INVALIDARG;
                    }
                    int num = 0;
                    int tabIndex = -1;
                    Control[] array = null;
                    switch (dwWhich)
                    {
                        case 1:
                        {
                            Control parentInternal = ctl.ParentInternal;
                            if (parentInternal == null)
                            {
                                goto Label_00E2;
                            }
                            array = parentInternal.GetChildControlsInTabOrder(false);
                            if (!flag4)
                            {
                                break;
                            }
                            tabIndex = ctl.TabIndex;
                            goto Label_00EA;
                        }
                        case 2:
                            l = new ArrayList();
                            this.MaybeAdd(l, ctl, selected, dwOleContF, false);
                            goto Label_0128;

                        case 3:
                            array = ctl.GetChildControlsInTabOrder(false);
                            ctl = null;
                            goto Label_015F;

                        case 4:
                        {
                            Hashtable components = this.GetComponents();
                            array = new Control[components.Keys.Count];
                            components.Keys.CopyTo(array, 0);
                            ctl = this.parent;
                            goto Label_015F;
                        }
                        default:
                            throw AxHost.E_INVALIDARG;
                    }
                    if (flag3)
                    {
                        num = ctl.TabIndex + 1;
                    }
                    goto Label_00EA;
                Label_00E2:
                    array = new Control[0];
                Label_00EA:
                    ctl = null;
                    goto Label_015F;
                Label_0102:
                    container = FindContainerForControl(ctl);
                    if (container == null)
                    {
                        goto Label_015F;
                    }
                    this.MaybeAdd(l, container.parent, selected, dwOleContF, true);
                    ctl = container.parent;
                Label_0128:
                    if (ctl != null)
                    {
                        goto Label_0102;
                    }
                Label_015F:
                    if (l == null)
                    {
                        l = new ArrayList();
                        if ((tabIndex == -1) && (array != null))
                        {
                            tabIndex = array.Length;
                        }
                        if (ctl != null)
                        {
                            this.MaybeAdd(l, ctl, selected, dwOleContF, false);
                        }
                        for (int i = num; i < tabIndex; i++)
                        {
                            this.MaybeAdd(l, array[i], selected, dwOleContF, false);
                        }
                    }
                    object[] objArray = new object[l.Count];
                    l.CopyTo(objArray, 0);
                    if (flag2)
                    {
                        int index = 0;
                        for (int j = objArray.Length - 1; index < j; j--)
                        {
                            object obj2 = objArray[index];
                            objArray[index] = objArray[j];
                            objArray[j] = obj2;
                            index++;
                        }
                    }
                    unknown = new AxHost.EnumUnknown(objArray);
                }
                finally
                {
                    this.UnlockComponents();
                }
                return unknown;
            }

            private void FillComponentsTable(IContainer container)
            {
                if (container != null)
                {
                    ComponentCollection components = container.Components;
                    if (components != null)
                    {
                        this.components = new Hashtable();
                        foreach (IComponent component in components)
                        {
                            if (((component is Control) && (component != this.parent)) && (component.Site != null))
                            {
                                this.components.Add(component, component);
                            }
                        }
                        return;
                    }
                }
                bool flag = true;
                Control[] array = new Control[this.containerCache.Values.Count];
                this.containerCache.Values.CopyTo(array, 0);
                if (array != null)
                {
                    if ((array.Length > 0) && (this.components == null))
                    {
                        this.components = new Hashtable();
                        flag = false;
                    }
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (flag && !this.components.Contains(array[i]))
                        {
                            this.components.Add(array[i], array[i]);
                        }
                    }
                }
                this.GetAllChildren(this.parent);
            }

            internal static AxHost.AxContainer FindContainerForControl(Control ctl)
            {
                AxHost host = ctl as AxHost;
                if (host != null)
                {
                    if (host.container != null)
                    {
                        return host.container;
                    }
                    ContainerControl containingControl = host.ContainingControl;
                    if (containingControl != null)
                    {
                        AxHost.AxContainer container = containingControl.CreateAxContainer();
                        if (container.RegisterControl(host))
                        {
                            container.AddControl(host);
                            return container;
                        }
                    }
                }
                return null;
            }

            internal void FormCreated()
            {
                if (!this.formAlreadyCreated)
                {
                    this.formAlreadyCreated = true;
                    ArrayList list = new ArrayList();
                    this.ListAxControls(list, false);
                    AxHost[] array = new AxHost[list.Count];
                    list.CopyTo(array, 0);
                    for (int i = 0; i < array.Length; i++)
                    {
                        AxHost host = array[i];
                        if (((host.GetOcState() >= 2) && host.IsUserMode()) && host.AwaitingDefreezing())
                        {
                            host.Freeze(false);
                        }
                    }
                }
            }

            private void GetAllChildren(Control ctl)
            {
                if (ctl != null)
                {
                    if (this.components == null)
                    {
                        this.components = new Hashtable();
                    }
                    if ((ctl != this.parent) && !this.components.Contains(ctl))
                    {
                        this.components.Add(ctl, ctl);
                    }
                    foreach (Control control in ctl.Controls)
                    {
                        this.GetAllChildren(control);
                    }
                }
            }

            private Hashtable GetComponents()
            {
                return this.GetComponents(this.GetParentsContainer());
            }

            private Hashtable GetComponents(IContainer cont)
            {
                if (this.lockCount == 0)
                {
                    this.FillComponentsTable(cont);
                }
                return this.components;
            }

            private bool GetControlBelongs(Control ctl)
            {
                return (this.GetComponents()[ctl] != null);
            }

            internal string GetNameForControl(Control ctl)
            {
                string str = (ctl.Site != null) ? ctl.Site.Name : ctl.Name;
                if (str != null)
                {
                    return str;
                }
                return "";
            }

            private IContainer GetParentIsDesigned()
            {
                ISite site = this.parent.Site;
                if ((site != null) && site.DesignMode)
                {
                    return site.Container;
                }
                return null;
            }

            private IContainer GetParentsContainer()
            {
                IContainer parentIsDesigned = this.GetParentIsDesigned();
                if (parentIsDesigned != null)
                {
                    return parentIsDesigned;
                }
                return this.assocContainer;
            }

            internal object GetProxyForContainer()
            {
                return this;
            }

            internal System.Windows.Forms.UnsafeNativeMethods.IExtender GetProxyForControl(Control ctl)
            {
                System.Windows.Forms.UnsafeNativeMethods.IExtender extender = null;
                if (this.proxyCache == null)
                {
                    this.proxyCache = new Hashtable();
                }
                else
                {
                    extender = (System.Windows.Forms.UnsafeNativeMethods.IExtender) this.proxyCache[ctl];
                }
                if (extender == null)
                {
                    if ((ctl != this.parent) && !this.GetControlBelongs(ctl))
                    {
                        AxHost.AxContainer container = FindContainerForControl(ctl);
                        if (container == null)
                        {
                            return null;
                        }
                        extender = new ExtenderProxy(ctl, container);
                    }
                    else
                    {
                        extender = new ExtenderProxy(ctl, this);
                    }
                    this.proxyCache.Add(ctl, extender);
                }
                return extender;
            }

            private void ListAxControls(ArrayList list, bool fuseOcx)
            {
                Hashtable components = this.GetComponents();
                if (components != null)
                {
                    Control[] array = new Control[components.Keys.Count];
                    components.Keys.CopyTo(array, 0);
                    if (array != null)
                    {
                        for (int i = 0; i < array.Length; i++)
                        {
                            Control control = array[i];
                            AxHost host = control as AxHost;
                            if (host != null)
                            {
                                if (fuseOcx)
                                {
                                    list.Add(host.GetOcx());
                                }
                                else
                                {
                                    list.Add(control);
                                }
                            }
                        }
                    }
                }
            }

            private void LockComponents()
            {
                this.lockCount++;
            }

            private void MaybeAdd(ArrayList l, Control ctl, bool selected, int dwOleContF, bool ignoreBelong)
            {
                if ((ignoreBelong || (ctl == this.parent)) || this.GetControlBelongs(ctl))
                {
                    if (selected)
                    {
                        ISelectionService selectionService = AxHost.GetSelectionService(ctl);
                        if ((selectionService == null) || !selectionService.GetComponentSelected(this))
                        {
                            return;
                        }
                    }
                    AxHost host = ctl as AxHost;
                    if ((host != null) && ((dwOleContF & 1) != 0))
                    {
                        l.Add(host.GetOcx());
                    }
                    else if ((dwOleContF & 4) != 0)
                    {
                        object proxyForControl = this.GetProxyForControl(ctl);
                        if (proxyForControl != null)
                        {
                            l.Add(proxyForControl);
                        }
                    }
                }
            }

            private void OnComponentRemoved(object sender, ComponentEventArgs e)
            {
                Control component = e.Component as Control;
                if ((sender == this.assocContainer) && (component != null))
                {
                    this.RemoveControl(component);
                }
            }

            internal void OnExitEditMode(AxHost ctl)
            {
                if ((this.ctlInEditMode != null) && (this.ctlInEditMode == ctl))
                {
                    this.ctlInEditMode = null;
                }
            }

            internal void OnInPlaceDeactivate(AxHost site)
            {
                if (this.siteActive == site)
                {
                    this.siteActive = null;
                    if (site.GetSiteOwnsDeactivation())
                    {
                        this.parent.ActiveControl = null;
                    }
                }
            }

            internal void OnUIActivate(AxHost site)
            {
                if (this.siteUIActive != site)
                {
                    if ((this.siteUIActive != null) && (this.siteUIActive != site))
                    {
                        AxHost siteUIActive = this.siteUIActive;
                        bool axState = siteUIActive.GetAxState(AxHost.ownDisposing);
                        try
                        {
                            siteUIActive.SetAxState(AxHost.ownDisposing, true);
                            siteUIActive.GetInPlaceObject().UIDeactivate();
                        }
                        finally
                        {
                            siteUIActive.SetAxState(AxHost.ownDisposing, axState);
                        }
                    }
                    site.AddSelectionHandler();
                    this.siteUIActive = site;
                    ContainerControl containingControl = site.ContainingControl;
                    if (containingControl != null)
                    {
                        containingControl.ActiveControl = site;
                    }
                }
            }

            internal void OnUIDeactivate(AxHost site)
            {
                this.siteUIActive = null;
                site.RemoveSelectionHandler();
                site.SetSelectionStyle(1);
                site.editMode = 0;
                if (site.GetSiteOwnsDeactivation())
                {
                    ContainerControl containingControl = site.ContainingControl;
                }
            }

            private bool RegisterControl(AxHost ctl)
            {
                ISite site = ctl.Site;
                if (site != null)
                {
                    IContainer container = site.Container;
                    if (container != null)
                    {
                        if (this.assocContainer != null)
                        {
                            return (container == this.assocContainer);
                        }
                        this.assocContainer = container;
                        IComponentChangeService service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                        if (service != null)
                        {
                            service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                        }
                        return true;
                    }
                }
                return false;
            }

            internal void RemoveControl(Control ctl)
            {
                lock (this)
                {
                    if (this.containerCache.Contains(ctl))
                    {
                        this.containerCache.Remove(ctl);
                    }
                }
            }

            FieldInfo IReflect.GetField(string name, BindingFlags bindingAttr)
            {
                return null;
            }

            FieldInfo[] IReflect.GetFields(BindingFlags bindingAttr)
            {
                return new FieldInfo[0];
            }

            MemberInfo[] IReflect.GetMember(string name, BindingFlags bindingAttr)
            {
                return new MemberInfo[0];
            }

            MemberInfo[] IReflect.GetMembers(BindingFlags bindingAttr)
            {
                return new MemberInfo[0];
            }

            MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr)
            {
                return null;
            }

            MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr, Binder binder, System.Type[] types, ParameterModifier[] modifiers)
            {
                return null;
            }

            MethodInfo[] IReflect.GetMethods(BindingFlags bindingAttr)
            {
                return new MethodInfo[0];
            }

            PropertyInfo[] IReflect.GetProperties(BindingFlags bindingAttr)
            {
                return new PropertyInfo[0];
            }

            PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr)
            {
                return null;
            }

            PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr, Binder binder, System.Type returnType, System.Type[] types, ParameterModifier[] modifiers)
            {
                return null;
            }

            object IReflect.InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
            {
                foreach (DictionaryEntry entry in this.containerCache)
                {
                    if (this.GetNameForControl((Control) entry.Key).Equals(name))
                    {
                        return this.GetProxyForControl((Control) entry.Value);
                    }
                }
                throw AxHost.E_FAIL;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleContainer.EnumObjects(int grfFlags, out System.Windows.Forms.UnsafeNativeMethods.IEnumUnknown ppenum)
            {
                ppenum = null;
                if ((grfFlags & 1) != 0)
                {
                    ArrayList list = new ArrayList();
                    this.ListAxControls(list, true);
                    if (list.Count > 0)
                    {
                        object[] array = new object[list.Count];
                        list.CopyTo(array, 0);
                        ppenum = new AxHost.EnumUnknown(array);
                        return 0;
                    }
                }
                ppenum = new AxHost.EnumUnknown(null);
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleContainer.LockContainer(bool fLock)
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleContainer.ParseDisplayName(object pbc, string pszDisplayName, int[] pchEaten, object[] ppmkOut)
            {
                if (ppmkOut != null)
                {
                    ppmkOut[0] = null;
                }
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame.ContextSensitiveHelp(int fEnterMode)
            {
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame.EnableModeless(bool fEnable)
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame.GetBorder(System.Windows.Forms.NativeMethods.COMRECT lprectBorder)
            {
                return -2147467263;
            }

            IntPtr System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame.GetWindow()
            {
                return this.parent.Handle;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame.InsertMenus(IntPtr hmenuShared, System.Windows.Forms.NativeMethods.tagOleMenuGroupWidths lpMenuWidths)
            {
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame.RemoveMenus(IntPtr hmenuShared)
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame.RequestBorderSpace(System.Windows.Forms.NativeMethods.COMRECT pborderwidths)
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame.SetActiveObject(System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject pActiveObject, string pszObjName)
            {
                if ((this.siteUIActive != null) && (this.siteUIActive.iOleInPlaceActiveObjectExternal != pActiveObject))
                {
                    if (this.siteUIActive.iOleInPlaceActiveObjectExternal != null)
                    {
                        Marshal.ReleaseComObject(this.siteUIActive.iOleInPlaceActiveObjectExternal);
                    }
                    this.siteUIActive.iOleInPlaceActiveObjectExternal = pActiveObject;
                }
                if (pActiveObject == null)
                {
                    if (this.ctlInEditMode != null)
                    {
                        this.ctlInEditMode.editMode = 0;
                        this.ctlInEditMode = null;
                    }
                    return 0;
                }
                AxHost axHost = null;
                if (pActiveObject is System.Windows.Forms.UnsafeNativeMethods.IOleObject)
                {
                    System.Windows.Forms.UnsafeNativeMethods.IOleObject obj2 = (System.Windows.Forms.UnsafeNativeMethods.IOleObject) pActiveObject;
                    System.Windows.Forms.UnsafeNativeMethods.IOleClientSite clientSite = null;
                    try
                    {
                        clientSite = obj2.GetClientSite();
                        if (clientSite is AxHost.OleInterfaces)
                        {
                            axHost = ((AxHost.OleInterfaces) clientSite).GetAxHost();
                        }
                    }
                    catch (COMException)
                    {
                    }
                    if (this.ctlInEditMode != null)
                    {
                        this.ctlInEditMode.SetSelectionStyle(1);
                        this.ctlInEditMode.editMode = 0;
                    }
                    if (axHost == null)
                    {
                        this.ctlInEditMode = null;
                    }
                    else if (!axHost.IsUserMode())
                    {
                        this.ctlInEditMode = axHost;
                        axHost.editMode = 1;
                        axHost.AddSelectionHandler();
                        axHost.SetSelectionStyle(2);
                    }
                }
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame.SetBorderSpace(System.Windows.Forms.NativeMethods.COMRECT pborderwidths)
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame.SetMenu(IntPtr hmenuShared, IntPtr holemenu, IntPtr hwndActiveObject)
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame.SetStatusText(string pszStatusText)
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame.TranslateAccelerator(ref System.Windows.Forms.NativeMethods.MSG lpmsg, short wID)
            {
                return 1;
            }

            private void UnlockComponents()
            {
                this.lockCount--;
                if (this.lockCount == 0)
                {
                    this.components = null;
                }
            }

            System.Type IReflect.UnderlyingSystemType
            {
                get
                {
                    return null;
                }
            }

            private class ExtenderProxy : System.Windows.Forms.UnsafeNativeMethods.IExtender, System.Windows.Forms.UnsafeNativeMethods.IVBGetControl, System.Windows.Forms.UnsafeNativeMethods.IGetVBAObject, System.Windows.Forms.UnsafeNativeMethods.IGetOleObject, IReflect
            {
                private WeakReference pContainer;
                private WeakReference pRef;

                internal ExtenderProxy(Control principal, AxHost.AxContainer container)
                {
                    this.pRef = new WeakReference(principal);
                    this.pContainer = new WeakReference(container);
                }

                private AxHost.AxContainer GetC()
                {
                    return (AxHost.AxContainer) this.pContainer.Target;
                }

                private Control GetP()
                {
                    return (Control) this.pRef.Target;
                }

                public void Move(object left, object top, object width, object height)
                {
                }

                FieldInfo IReflect.GetField(string name, BindingFlags bindingAttr)
                {
                    return null;
                }

                FieldInfo[] IReflect.GetFields(BindingFlags bindingAttr)
                {
                    return new FieldInfo[0];
                }

                MemberInfo[] IReflect.GetMember(string name, BindingFlags bindingAttr)
                {
                    MemberInfo[] member = this.GetP().GetType().GetMember(name, bindingAttr);
                    if (member == null)
                    {
                        member = base.GetType().GetMember(name, bindingAttr);
                    }
                    return member;
                }

                MemberInfo[] IReflect.GetMembers(BindingFlags bindingAttr)
                {
                    MemberInfo[] members = base.GetType().GetMembers(bindingAttr);
                    MemberInfo[] sourceArray = this.GetP().GetType().GetMembers(bindingAttr);
                    if (members == null)
                    {
                        return sourceArray;
                    }
                    if (sourceArray == null)
                    {
                        return members;
                    }
                    MemberInfo[] destinationArray = new MemberInfo[members.Length + sourceArray.Length];
                    Array.Copy(members, 0, destinationArray, 0, members.Length);
                    Array.Copy(sourceArray, 0, destinationArray, members.Length, sourceArray.Length);
                    return destinationArray;
                }

                MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr)
                {
                    return null;
                }

                MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr, Binder binder, System.Type[] types, ParameterModifier[] modifiers)
                {
                    return null;
                }

                MethodInfo[] IReflect.GetMethods(BindingFlags bindingAttr)
                {
                    return new MethodInfo[] { base.GetType().GetMethod("Move") };
                }

                PropertyInfo[] IReflect.GetProperties(BindingFlags bindingAttr)
                {
                    PropertyInfo[] properties = base.GetType().GetProperties(bindingAttr);
                    PropertyInfo[] infoArray2 = this.GetP().GetType().GetProperties(bindingAttr);
                    if (properties == null)
                    {
                        return infoArray2;
                    }
                    if (infoArray2 == null)
                    {
                        return properties;
                    }
                    int num = 0;
                    PropertyInfo[] infoArray3 = new PropertyInfo[properties.Length + infoArray2.Length];
                    foreach (PropertyInfo info in properties)
                    {
                        infoArray3[num++] = info;
                    }
                    foreach (PropertyInfo info2 in infoArray2)
                    {
                        infoArray3[num++] = info2;
                    }
                    return infoArray3;
                }

                PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr)
                {
                    PropertyInfo property = this.GetP().GetType().GetProperty(name, bindingAttr);
                    if (property == null)
                    {
                        property = base.GetType().GetProperty(name, bindingAttr);
                    }
                    return property;
                }

                PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr, Binder binder, System.Type returnType, System.Type[] types, ParameterModifier[] modifiers)
                {
                    PropertyInfo info = this.GetP().GetType().GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
                    if (info == null)
                    {
                        info = base.GetType().GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
                    }
                    return info;
                }

                object IReflect.InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
                {
                    try
                    {
                        return base.GetType().InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
                    }
                    catch (MissingMethodException)
                    {
                        return this.GetP().GetType().InvokeMember(name, invokeAttr, binder, this.GetP(), args, modifiers, culture, namedParameters);
                    }
                }

                object System.Windows.Forms.UnsafeNativeMethods.IGetOleObject.GetOleObject(ref Guid riid)
                {
                    if (!riid.Equals(AxHost.ioleobject_Guid))
                    {
                        throw AxHost.E_INVALIDARG;
                    }
                    Control p = this.GetP();
                    if ((p == null) || !(p is AxHost))
                    {
                        throw AxHost.E_FAIL;
                    }
                    return ((AxHost) p).GetOcx();
                }

                int System.Windows.Forms.UnsafeNativeMethods.IGetVBAObject.GetObject(ref Guid riid, System.Windows.Forms.UnsafeNativeMethods.IVBFormat[] rval, int dwReserved)
                {
                    if ((rval == null) || riid.Equals(Guid.Empty))
                    {
                        return -2147024809;
                    }
                    if (riid.Equals(AxHost.ivbformat_Guid))
                    {
                        rval[0] = new AxHost.VBFormat();
                        return 0;
                    }
                    rval[0] = null;
                    return -2147467262;
                }

                int System.Windows.Forms.UnsafeNativeMethods.IVBGetControl.EnumControls(int dwOleContF, int dwWhich, out System.Windows.Forms.UnsafeNativeMethods.IEnumUnknown ppenum)
                {
                    ppenum = this.GetC().EnumControls(this.GetP(), dwOleContF, dwWhich);
                    return 0;
                }

                public int Align
                {
                    get
                    {
                        int dock = (int) this.GetP().Dock;
                        if ((dock >= 0) && (dock <= 4))
                        {
                            return dock;
                        }
                        return 0;
                    }
                    set
                    {
                        this.GetP().Dock = (DockStyle) value;
                    }
                }

                public uint BackColor
                {
                    get
                    {
                        return AxHost.GetOleColorFromColor(this.GetP().BackColor);
                    }
                    set
                    {
                        this.GetP().BackColor = AxHost.GetColorFromOleColor(value);
                    }
                }

                public object Container
                {
                    get
                    {
                        return this.GetC().GetProxyForContainer();
                    }
                }

                public bool Enabled
                {
                    get
                    {
                        return this.GetP().Enabled;
                    }
                    set
                    {
                        this.GetP().Enabled = value;
                    }
                }

                public uint ForeColor
                {
                    get
                    {
                        return AxHost.GetOleColorFromColor(this.GetP().ForeColor);
                    }
                    set
                    {
                        this.GetP().ForeColor = AxHost.GetColorFromOleColor(value);
                    }
                }

                public int Height
                {
                    get
                    {
                        return AxHost.Pixel2Twip(this.GetP().Height, false);
                    }
                    set
                    {
                        this.GetP().Height = AxHost.Twip2Pixel(value, false);
                    }
                }

                public IntPtr Hwnd
                {
                    get
                    {
                        return this.GetP().Handle;
                    }
                }

                public int Left
                {
                    get
                    {
                        return AxHost.Pixel2Twip(this.GetP().Left, true);
                    }
                    set
                    {
                        this.GetP().Left = AxHost.Twip2Pixel(value, true);
                    }
                }

                public string Name
                {
                    get
                    {
                        return this.GetC().GetNameForControl(this.GetP());
                    }
                }

                public object Parent
                {
                    get
                    {
                        return this.GetC().GetProxyForControl(this.GetC().parent);
                    }
                }

                System.Type IReflect.UnderlyingSystemType
                {
                    get
                    {
                        return null;
                    }
                }

                public short TabIndex
                {
                    get
                    {
                        return (short) this.GetP().TabIndex;
                    }
                    set
                    {
                        this.GetP().TabIndex = value;
                    }
                }

                public bool TabStop
                {
                    get
                    {
                        return this.GetP().TabStop;
                    }
                    set
                    {
                        this.GetP().TabStop = value;
                    }
                }

                public string Text
                {
                    get
                    {
                        return this.GetP().Text;
                    }
                    set
                    {
                        this.GetP().Text = value;
                    }
                }

                public int Top
                {
                    get
                    {
                        return AxHost.Pixel2Twip(this.GetP().Top, false);
                    }
                    set
                    {
                        this.GetP().Top = AxHost.Twip2Pixel(value, false);
                    }
                }

                public bool Visible
                {
                    get
                    {
                        return this.GetP().Visible;
                    }
                    set
                    {
                        this.GetP().Visible = value;
                    }
                }

                public int Width
                {
                    get
                    {
                        return AxHost.Pixel2Twip(this.GetP().Width, true);
                    }
                    set
                    {
                        this.GetP().Width = AxHost.Twip2Pixel(value, true);
                    }
                }
            }
        }

        private class AxEnumConverter : Com2EnumConverter
        {
            private AxHost.AxPropertyDescriptor target;

            public AxEnumConverter(AxHost.AxPropertyDescriptor target, Com2Enum com2Enum) : base(com2Enum)
            {
                this.target = target;
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                TypeConverter converter = this.target.Converter;
                return base.GetStandardValues(context);
            }
        }

        internal class AxFlags
        {
            internal const int IgnoreThreadModel = 0x10000000;
            internal const int IncludePropertiesVerb = 2;
            internal const int PreventEditMode = 1;
        }

        private class AxPerPropertyBrowsingEnum : Com2Enum
        {
            private bool arraysFetched;
            private OleStrCAMarshaler nameMarshaller;
            private AxHost owner;
            private AxHost.AxPropertyDescriptor target;
            private Int32CAMarshaler valueMarshaller;

            public AxPerPropertyBrowsingEnum(AxHost.AxPropertyDescriptor targetObject, AxHost owner, OleStrCAMarshaler names, Int32CAMarshaler values, bool allowUnknowns) : base(new string[0], new object[0], allowUnknowns)
            {
                this.target = targetObject;
                this.nameMarshaller = names;
                this.valueMarshaller = values;
                this.owner = owner;
                this.arraysFetched = false;
            }

            private void EnsureArrays()
            {
                if (!this.arraysFetched)
                {
                    this.arraysFetched = true;
                    try
                    {
                        object[] items = this.nameMarshaller.Items;
                        object[] objArray2 = this.valueMarshaller.Items;
                        System.Windows.Forms.NativeMethods.IPerPropertyBrowsing perPropertyBrowsing = this.owner.GetPerPropertyBrowsing();
                        int length = 0;
                        if (items.Length > 0)
                        {
                            object[] values = new object[objArray2.Length];
                            System.Windows.Forms.NativeMethods.VARIANT pVarOut = new System.Windows.Forms.NativeMethods.VARIANT();
                            for (int i = 0; i < items.Length; i++)
                            {
                                int dwCookie = (int) objArray2[i];
                                if ((items[i] != null) && (items[i] is string))
                                {
                                    pVarOut.vt = 0;
                                    if ((perPropertyBrowsing.GetPredefinedValue(this.target.Dispid, dwCookie, pVarOut) == 0) && (pVarOut.vt != 0))
                                    {
                                        values[i] = pVarOut.ToObject();
                                    }
                                    pVarOut.Clear();
                                    length++;
                                }
                            }
                            if (length > 0)
                            {
                                string[] destinationArray = new string[length];
                                Array.Copy(items, 0, destinationArray, 0, length);
                                base.PopulateArrays(destinationArray, values);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            public override object FromString(string s)
            {
                this.EnsureArrays();
                return base.FromString(s);
            }

            protected override void PopulateArrays(string[] names, object[] values)
            {
            }

            internal void RefreshArrays(OleStrCAMarshaler names, Int32CAMarshaler values)
            {
                this.nameMarshaller = names;
                this.valueMarshaller = values;
                this.arraysFetched = false;
            }

            public override string ToString(object v)
            {
                this.EnsureArrays();
                return base.ToString(v);
            }

            public override string[] Names
            {
                get
                {
                    this.EnsureArrays();
                    return base.Names;
                }
            }

            public override object[] Values
            {
                get
                {
                    this.EnsureArrays();
                    return base.Values;
                }
            }
        }

        internal class AxPropertyDescriptor : PropertyDescriptor
        {
            private PropertyDescriptor baseProp;
            private TypeConverter converter;
            private DispIdAttribute dispid;
            private UITypeEditor editor;
            private const int FlagCheckGetter = 2;
            private const int FlagGettterThrew = 4;
            private const int FlagIgnoreCanAccessProperties = 8;
            private int flags;
            private const int FlagSettingValue = 0x10;
            private const int FlagUpdatedEditorAndConverter = 1;
            internal AxHost owner;
            private ArrayList updateAttrs;

            internal AxPropertyDescriptor(PropertyDescriptor baseProp, AxHost owner) : base(baseProp)
            {
                this.updateAttrs = new ArrayList();
                this.baseProp = baseProp;
                this.owner = owner;
                this.dispid = (DispIdAttribute) baseProp.Attributes[typeof(DispIdAttribute)];
                if (this.dispid != null)
                {
                    if (!this.IsBrowsable && !this.IsReadOnly)
                    {
                        Guid propertyPage = this.GetPropertyPage(this.dispid.Value);
                        if (!Guid.Empty.Equals(propertyPage))
                        {
                            this.AddAttribute(new BrowsableAttribute(true));
                        }
                    }
                    CategoryAttribute categoryForDispid = owner.GetCategoryForDispid(this.dispid.Value);
                    if (categoryForDispid != null)
                    {
                        this.AddAttribute(categoryForDispid);
                    }
                    if (this.PropertyType.GUID.Equals(AxHost.dataSource_Guid))
                    {
                        this.SetFlag(8, true);
                    }
                }
            }

            private void AddAttribute(Attribute attr)
            {
                this.updateAttrs.Add(attr);
            }

            public override bool CanResetValue(object o)
            {
                return this.baseProp.CanResetValue(o);
            }

            public override object GetEditor(System.Type editorBaseType)
            {
                this.UpdateTypeConverterAndTypeEditorInternal(false, this.dispid.Value);
                if (editorBaseType.Equals(typeof(UITypeEditor)) && (this.editor != null))
                {
                    return this.editor;
                }
                return base.GetEditor(editorBaseType);
            }

            private bool GetFlag(int flagValue)
            {
                return ((this.flags & flagValue) == flagValue);
            }

            private Guid GetPropertyPage(int dispid)
            {
                try
                {
                    Guid guid;
                    System.Windows.Forms.NativeMethods.IPerPropertyBrowsing perPropertyBrowsing = this.owner.GetPerPropertyBrowsing();
                    if (perPropertyBrowsing == null)
                    {
                        return Guid.Empty;
                    }
                    if (System.Windows.Forms.NativeMethods.Succeeded(perPropertyBrowsing.MapPropertyToPage(dispid, out guid)))
                    {
                        return guid;
                    }
                }
                catch (COMException)
                {
                }
                catch (Exception)
                {
                }
                return Guid.Empty;
            }

            public override object GetValue(object component)
            {
                object obj2;
                if ((!this.GetFlag(8) && !this.owner.CanAccessProperties) || this.GetFlag(4))
                {
                    return null;
                }
                try
                {
                    this.owner.NoComponentChangeEvents++;
                    obj2 = this.baseProp.GetValue(component);
                }
                catch (Exception exception)
                {
                    if (!this.GetFlag(2))
                    {
                        this.SetFlag(2, true);
                        this.AddAttribute(new BrowsableAttribute(false));
                        this.owner.RefreshAllProperties = true;
                        this.SetFlag(4, true);
                    }
                    throw exception;
                }
                finally
                {
                    this.owner.NoComponentChangeEvents--;
                }
                return obj2;
            }

            public void OnValueChanged(object component)
            {
                this.OnValueChanged(component, EventArgs.Empty);
            }

            public override void ResetValue(object o)
            {
                this.baseProp.ResetValue(o);
            }

            private void SetFlag(int flagValue, bool value)
            {
                if (value)
                {
                    this.flags |= flagValue;
                }
                else
                {
                    this.flags &= ~flagValue;
                }
            }

            public override void SetValue(object component, object value)
            {
                if (this.GetFlag(8) || this.owner.CanAccessProperties)
                {
                    try
                    {
                        this.SetFlag(0x10, true);
                        if (this.PropertyType.IsEnum && (value.GetType() != this.PropertyType))
                        {
                            this.baseProp.SetValue(component, Enum.ToObject(this.PropertyType, value));
                        }
                        else
                        {
                            this.baseProp.SetValue(component, value);
                        }
                    }
                    finally
                    {
                        this.SetFlag(0x10, false);
                    }
                    this.OnValueChanged(component);
                    if (this.owner == component)
                    {
                        this.owner.SetAxState(AxHost.valueChanged, true);
                    }
                }
            }

            public override bool ShouldSerializeValue(object o)
            {
                return this.baseProp.ShouldSerializeValue(o);
            }

            internal void UpdateAttributes()
            {
                if (this.updateAttrs.Count != 0)
                {
                    ArrayList list = new ArrayList(this.AttributeArray);
                    foreach (Attribute attribute in this.updateAttrs)
                    {
                        list.Add(attribute);
                    }
                    Attribute[] array = new Attribute[list.Count];
                    list.CopyTo(array, 0);
                    this.AttributeArray = array;
                    this.updateAttrs.Clear();
                }
            }

            internal void UpdateTypeConverterAndTypeEditor(bool force)
            {
                if (this.GetFlag(1) && force)
                {
                    this.SetFlag(1, false);
                }
            }

            internal void UpdateTypeConverterAndTypeEditorInternal(bool force, int dispid)
            {
                if ((!this.GetFlag(1) || force) && (this.owner.GetOcx() != null))
                {
                    try
                    {
                        System.Windows.Forms.NativeMethods.IPerPropertyBrowsing perPropertyBrowsing = this.owner.GetPerPropertyBrowsing();
                        if (perPropertyBrowsing != null)
                        {
                            bool flag = false;
                            System.Windows.Forms.NativeMethods.CA_STRUCT pCaStringsOut = new System.Windows.Forms.NativeMethods.CA_STRUCT();
                            System.Windows.Forms.NativeMethods.CA_STRUCT pCaCookiesOut = new System.Windows.Forms.NativeMethods.CA_STRUCT();
                            int errorCode = 0;
                            try
                            {
                                errorCode = perPropertyBrowsing.GetPredefinedStrings(dispid, pCaStringsOut, pCaCookiesOut);
                            }
                            catch (ExternalException exception)
                            {
                                errorCode = exception.ErrorCode;
                            }
                            if (errorCode != 0)
                            {
                                flag = false;
                                if (this.converter is Com2EnumConverter)
                                {
                                    this.converter = null;
                                }
                            }
                            else
                            {
                                flag = true;
                            }
                            if (flag)
                            {
                                OleStrCAMarshaler names = new OleStrCAMarshaler(pCaStringsOut);
                                Int32CAMarshaler values = new Int32CAMarshaler(pCaCookiesOut);
                                if ((names.Count > 0) && (values.Count > 0))
                                {
                                    if (this.converter == null)
                                    {
                                        this.converter = new AxHost.AxEnumConverter(this, new AxHost.AxPerPropertyBrowsingEnum(this, this.owner, names, values, true));
                                    }
                                    else if (this.converter is AxHost.AxEnumConverter)
                                    {
                                        ((AxHost.AxEnumConverter) this.converter).RefreshValues();
                                        AxHost.AxPerPropertyBrowsingEnum enum2 = ((AxHost.AxEnumConverter) this.converter).com2Enum as AxHost.AxPerPropertyBrowsingEnum;
                                        if (enum2 != null)
                                        {
                                            enum2.RefreshArrays(names, values);
                                        }
                                    }
                                }
                            }
                            else if (((ComAliasNameAttribute) this.baseProp.Attributes[typeof(ComAliasNameAttribute)]) == null)
                            {
                                Guid propertyPage = this.GetPropertyPage(dispid);
                                if (!Guid.Empty.Equals(propertyPage))
                                {
                                    this.editor = new AxHost.AxPropertyTypeEditor(this, propertyPage);
                                    if (!this.IsBrowsable)
                                    {
                                        this.AddAttribute(new BrowsableAttribute(true));
                                    }
                                }
                            }
                        }
                        this.SetFlag(1, true);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            public override System.Type ComponentType
            {
                get
                {
                    return this.baseProp.ComponentType;
                }
            }

            public override TypeConverter Converter
            {
                get
                {
                    if (this.dispid != null)
                    {
                        this.UpdateTypeConverterAndTypeEditorInternal(false, this.Dispid);
                    }
                    if (this.converter == null)
                    {
                        return base.Converter;
                    }
                    return this.converter;
                }
            }

            internal int Dispid
            {
                get
                {
                    DispIdAttribute attribute = (DispIdAttribute) this.baseProp.Attributes[typeof(DispIdAttribute)];
                    if (attribute != null)
                    {
                        return attribute.Value;
                    }
                    return -1;
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return this.baseProp.IsReadOnly;
                }
            }

            public override System.Type PropertyType
            {
                get
                {
                    return this.baseProp.PropertyType;
                }
            }

            internal bool SettingValue
            {
                get
                {
                    return this.GetFlag(0x10);
                }
            }
        }

        private class AxPropertyTypeEditor : UITypeEditor
        {
            private Guid guid;
            private AxHost.AxPropertyDescriptor propDesc;

            public AxPropertyTypeEditor(AxHost.AxPropertyDescriptor pd, Guid guid)
            {
                this.propDesc = pd;
                this.guid = guid;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                try
                {
                    object instance = context.Instance;
                    this.propDesc.owner.ShowPropertyPageForDispid(this.propDesc.Dispid, this.guid);
                }
                catch (Exception exception)
                {
                    if (provider != null)
                    {
                        IUIService service = (IUIService) provider.GetService(typeof(IUIService));
                        if (service != null)
                        {
                            service.ShowError(exception, System.Windows.Forms.SR.GetString("ErrorTypeConverterFailed"));
                        }
                    }
                }
                return value;
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }
        }

        [AttributeUsage(AttributeTargets.Class, Inherited=false)]
        public sealed class ClsidAttribute : Attribute
        {
            private string val;

            public ClsidAttribute(string clsid)
            {
                this.val = clsid;
            }

            public string Value
            {
                get
                {
                    return this.val;
                }
            }
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public class ConnectionPointCookie
        {
            private System.Windows.Forms.UnsafeNativeMethods.IConnectionPoint connectionPoint;
            private int cookie;
            internal int threadId;

            public ConnectionPointCookie(object source, object sink, System.Type eventInterface) : this(source, sink, eventInterface, true)
            {
            }

            internal ConnectionPointCookie(object source, object sink, System.Type eventInterface, bool throwException)
            {
                if (source is System.Windows.Forms.UnsafeNativeMethods.IConnectionPointContainer)
                {
                    System.Windows.Forms.UnsafeNativeMethods.IConnectionPointContainer container = (System.Windows.Forms.UnsafeNativeMethods.IConnectionPointContainer) source;
                    try
                    {
                        Guid gUID = eventInterface.GUID;
                        if (container.FindConnectionPoint(ref gUID, out this.connectionPoint) != 0)
                        {
                            this.connectionPoint = null;
                        }
                    }
                    catch
                    {
                        this.connectionPoint = null;
                    }
                    if (this.connectionPoint != null)
                    {
                        if ((sink != null) && eventInterface.IsInstanceOfType(sink))
                        {
                            int num = this.connectionPoint.Advise(sink, ref this.cookie);
                            if (num != 0)
                            {
                                this.cookie = 0;
                                Marshal.ReleaseComObject(this.connectionPoint);
                                this.connectionPoint = null;
                                if (throwException)
                                {
                                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, System.Windows.Forms.SR.GetString("AXNoSinkAdvise", new object[] { eventInterface.Name }), new object[] { num }));
                                }
                            }
                            else
                            {
                                this.threadId = Thread.CurrentThread.ManagedThreadId;
                            }
                        }
                        else if (throwException)
                        {
                            throw new InvalidCastException(System.Windows.Forms.SR.GetString("AXNoSinkImplementation", new object[] { eventInterface.Name }));
                        }
                    }
                    else if (throwException)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("AXNoEventInterface", new object[] { eventInterface.Name }));
                    }
                }
                else if (throwException)
                {
                    throw new InvalidCastException(System.Windows.Forms.SR.GetString("AXNoConnectionPointContainer"));
                }
                if ((this.connectionPoint == null) || (this.cookie == 0))
                {
                    if (this.connectionPoint != null)
                    {
                        Marshal.ReleaseComObject(this.connectionPoint);
                    }
                    if (throwException)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("AXNoConnectionPoint", new object[] { eventInterface.Name }));
                    }
                }
            }

            private void AttemptDisconnect(object trash)
            {
                if (this.threadId == Thread.CurrentThread.ManagedThreadId)
                {
                    this.Disconnect();
                }
            }

            public void Disconnect()
            {
                if ((this.connectionPoint != null) && (this.cookie != 0))
                {
                    try
                    {
                        this.connectionPoint.Unadvise(this.cookie);
                    }
                    catch (Exception exception)
                    {
                        if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        this.cookie = 0;
                    }
                    try
                    {
                        Marshal.ReleaseComObject(this.connectionPoint);
                    }
                    catch (Exception exception2)
                    {
                        if (System.Windows.Forms.ClientUtils.IsCriticalException(exception2))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        this.connectionPoint = null;
                    }
                }
            }

            ~ConnectionPointCookie()
            {
                if (((this.connectionPoint != null) && (this.cookie != 0)) && !AppDomain.CurrentDomain.IsFinalizingForUnload())
                {
                    SynchronizationContext current = SynchronizationContext.Current;
                    if (current != null)
                    {
                        current.Post(new SendOrPostCallback(this.AttemptDisconnect), null);
                    }
                }
            }

            internal bool Connected
            {
                get
                {
                    return ((this.connectionPoint != null) && (this.cookie != 0));
                }
            }
        }

        internal class EnumUnknown : System.Windows.Forms.UnsafeNativeMethods.IEnumUnknown
        {
            private object[] arr;
            private int loc;
            private int size;

            internal EnumUnknown(object[] arr)
            {
                this.arr = arr;
                this.loc = 0;
                this.size = (arr == null) ? 0 : arr.Length;
            }

            private EnumUnknown(object[] arr, int loc) : this(arr)
            {
                this.loc = loc;
            }

            void System.Windows.Forms.UnsafeNativeMethods.IEnumUnknown.Clone(out System.Windows.Forms.UnsafeNativeMethods.IEnumUnknown ppenum)
            {
                ppenum = new AxHost.EnumUnknown(this.arr, this.loc);
            }

            int System.Windows.Forms.UnsafeNativeMethods.IEnumUnknown.Next(int celt, IntPtr rgelt, IntPtr pceltFetched)
            {
                if (pceltFetched != IntPtr.Zero)
                {
                    Marshal.WriteInt32(pceltFetched, 0, 0);
                }
                if (celt < 0)
                {
                    return -2147024809;
                }
                int val = 0;
                if (this.loc < this.size)
                {
                    while ((this.loc < this.size) && (val < celt))
                    {
                        if (this.arr[this.loc] != null)
                        {
                            Marshal.WriteIntPtr(rgelt, Marshal.GetIUnknownForObject(this.arr[this.loc]));
                            rgelt = (IntPtr) (((long) rgelt) + sizeof(IntPtr));
                            val++;
                        }
                        this.loc++;
                    }
                }
                else
                {
                    val = 0;
                }
                if (pceltFetched != IntPtr.Zero)
                {
                    Marshal.WriteInt32(pceltFetched, 0, val);
                }
                if (val != celt)
                {
                    return 1;
                }
                return 0;
            }

            void System.Windows.Forms.UnsafeNativeMethods.IEnumUnknown.Reset()
            {
                this.loc = 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IEnumUnknown.Skip(int celt)
            {
                this.loc += celt;
                if (this.loc >= this.size)
                {
                    return 1;
                }
                return 0;
            }
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public class InvalidActiveXStateException : Exception
        {
            private AxHost.ActiveXInvokeKind kind;
            private string name;

            public InvalidActiveXStateException()
            {
            }

            public InvalidActiveXStateException(string name, AxHost.ActiveXInvokeKind kind)
            {
                this.name = name;
                this.kind = kind;
            }

            public override string ToString()
            {
                switch (this.kind)
                {
                    case AxHost.ActiveXInvokeKind.MethodInvoke:
                        return System.Windows.Forms.SR.GetString("AXInvalidMethodInvoke", new object[] { this.name });

                    case AxHost.ActiveXInvokeKind.PropertyGet:
                        return System.Windows.Forms.SR.GetString("AXInvalidPropertyGet", new object[] { this.name });

                    case AxHost.ActiveXInvokeKind.PropertySet:
                        return System.Windows.Forms.SR.GetString("AXInvalidPropertySet", new object[] { this.name });
                }
                return base.ToString();
            }
        }

        private class OleInterfaces : System.Windows.Forms.UnsafeNativeMethods.IOleControlSite, System.Windows.Forms.UnsafeNativeMethods.IOleClientSite, System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite, System.Windows.Forms.UnsafeNativeMethods.ISimpleFrameSite, System.Windows.Forms.UnsafeNativeMethods.IVBGetControl, System.Windows.Forms.UnsafeNativeMethods.IGetVBAObject, System.Windows.Forms.UnsafeNativeMethods.IPropertyNotifySink, IReflect, IDisposable
        {
            private AxHost.ConnectionPointCookie connectionPoint;
            private AxHost host;

            internal OleInterfaces(AxHost host)
            {
                if (host == null)
                {
                    throw new ArgumentNullException("host");
                }
                this.host = host;
            }

            private void AttemptStopEvents(object trash)
            {
                if ((this.connectionPoint != null) && (this.connectionPoint.threadId == Thread.CurrentThread.ManagedThreadId))
                {
                    this.StopEvents();
                }
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposing && !AppDomain.CurrentDomain.IsFinalizingForUnload())
                {
                    SynchronizationContext current = SynchronizationContext.Current;
                    if (current != null)
                    {
                        current.Post(new SendOrPostCallback(this.AttemptStopEvents), null);
                    }
                }
            }

            internal AxHost GetAxHost()
            {
                return this.host;
            }

            internal void OnOcxCreate()
            {
                this.StartEvents();
            }

            internal void StartEvents()
            {
                if (this.connectionPoint == null)
                {
                    object ocx = this.host.GetOcx();
                    try
                    {
                        this.connectionPoint = new AxHost.ConnectionPointCookie(ocx, this, typeof(System.Windows.Forms.UnsafeNativeMethods.IPropertyNotifySink));
                    }
                    catch
                    {
                    }
                }
            }

            internal void StopEvents()
            {
                if (this.connectionPoint != null)
                {
                    this.connectionPoint.Disconnect();
                    this.connectionPoint = null;
                }
            }

            FieldInfo IReflect.GetField(string name, BindingFlags bindingAttr)
            {
                return null;
            }

            FieldInfo[] IReflect.GetFields(BindingFlags bindingAttr)
            {
                return new FieldInfo[0];
            }

            MemberInfo[] IReflect.GetMember(string name, BindingFlags bindingAttr)
            {
                return new MemberInfo[0];
            }

            MemberInfo[] IReflect.GetMembers(BindingFlags bindingAttr)
            {
                return new MemberInfo[0];
            }

            MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr)
            {
                return null;
            }

            MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr, Binder binder, System.Type[] types, ParameterModifier[] modifiers)
            {
                return null;
            }

            MethodInfo[] IReflect.GetMethods(BindingFlags bindingAttr)
            {
                return new MethodInfo[0];
            }

            PropertyInfo[] IReflect.GetProperties(BindingFlags bindingAttr)
            {
                return new PropertyInfo[0];
            }

            PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr)
            {
                return null;
            }

            PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr, Binder binder, System.Type returnType, System.Type[] types, ParameterModifier[] modifiers)
            {
                return null;
            }

            object IReflect.InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
            {
                if (name.StartsWith("[DISPID="))
                {
                    int index = name.IndexOf("]");
                    int dispid = int.Parse(name.Substring(8, index - 8), CultureInfo.InvariantCulture);
                    object ambientProperty = this.host.GetAmbientProperty(dispid);
                    if (ambientProperty != null)
                    {
                        return ambientProperty;
                    }
                }
                throw AxHost.E_FAIL;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IGetVBAObject.GetObject(ref Guid riid, System.Windows.Forms.UnsafeNativeMethods.IVBFormat[] rval, int dwReserved)
            {
                if ((rval == null) || riid.Equals(Guid.Empty))
                {
                    return -2147024809;
                }
                if (riid.Equals(AxHost.ivbformat_Guid))
                {
                    rval[0] = new AxHost.VBFormat();
                    return 0;
                }
                rval[0] = null;
                return -2147467262;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleClientSite.GetContainer(out System.Windows.Forms.UnsafeNativeMethods.IOleContainer container)
            {
                container = this.host.GetParentContainer();
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleClientSite.GetMoniker(int dwAssign, int dwWhichMoniker, out object moniker)
            {
                moniker = null;
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleClientSite.OnShowWindow(int fShow)
            {
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleClientSite.RequestNewObjectLayout()
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleClientSite.SaveObject()
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleClientSite.ShowObject()
            {
                if (!this.host.GetAxState(AxHost.fOwnWindow))
                {
                    IntPtr ptr;
                    if (this.host.GetAxState(AxHost.fFakingWindow))
                    {
                        this.host.DestroyFakeWindow();
                        this.host.TransitionDownTo(1);
                        this.host.TransitionUpTo(4);
                    }
                    if (this.host.GetOcState() < 4)
                    {
                        return 0;
                    }
                    if (System.Windows.Forms.NativeMethods.Succeeded(this.host.GetInPlaceObject().GetWindow(out ptr)))
                    {
                        if (this.host.GetHandleNoCreate() != ptr)
                        {
                            this.host.DetachWindow();
                            if (ptr != IntPtr.Zero)
                            {
                                this.host.AttachWindow(ptr);
                            }
                        }
                    }
                    else if (this.host.GetInPlaceObject() is System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObjectWindowless)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("AXWindowlessControl"));
                    }
                }
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleControlSite.GetExtendedControl(out object ppDisp)
            {
                ppDisp = this.host.GetParentContainer().GetProxyForControl(this.host);
                if (ppDisp == null)
                {
                    return -2147467263;
                }
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleControlSite.LockInPlaceActive(int fLock)
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleControlSite.OnControlInfoChanged()
            {
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleControlSite.OnFocus(int fGotFocus)
            {
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleControlSite.ShowPropertyFrame()
            {
                if (this.host.CanShowPropertyPages())
                {
                    this.host.ShowPropertyPages();
                    return 0;
                }
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleControlSite.TransformCoords(System.Windows.Forms.NativeMethods._POINTL pPtlHimetric, System.Windows.Forms.NativeMethods.tagPOINTF pPtfContainer, int dwFlags)
            {
                int hr = AxHost.SetupLogPixels(false);
                if (System.Windows.Forms.NativeMethods.Failed(hr))
                {
                    return hr;
                }
                if ((dwFlags & 4) != 0)
                {
                    if ((dwFlags & 2) == 0)
                    {
                        if ((dwFlags & 1) == 0)
                        {
                            return -2147024809;
                        }
                        pPtfContainer.x = this.host.HM2Pix(pPtlHimetric.x, AxHost.logPixelsX);
                        pPtfContainer.y = this.host.HM2Pix(pPtlHimetric.y, AxHost.logPixelsY);
                    }
                    else
                    {
                        pPtfContainer.x = this.host.HM2Pix(pPtlHimetric.x, AxHost.logPixelsX);
                        pPtfContainer.y = this.host.HM2Pix(pPtlHimetric.y, AxHost.logPixelsY);
                    }
                }
                else
                {
                    if ((dwFlags & 8) != 0)
                    {
                        if ((dwFlags & 2) != 0)
                        {
                            pPtlHimetric.x = this.host.Pix2HM((int) pPtfContainer.x, AxHost.logPixelsX);
                            pPtlHimetric.y = this.host.Pix2HM((int) pPtfContainer.y, AxHost.logPixelsY);
                            goto Label_013D;
                        }
                        if ((dwFlags & 1) != 0)
                        {
                            pPtlHimetric.x = this.host.Pix2HM((int) pPtfContainer.x, AxHost.logPixelsX);
                            pPtlHimetric.y = this.host.Pix2HM((int) pPtfContainer.y, AxHost.logPixelsY);
                            goto Label_013D;
                        }
                    }
                    return -2147024809;
                }
            Label_013D:
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleControlSite.TranslateAccelerator(ref System.Windows.Forms.NativeMethods.MSG pMsg, int grfModifiers)
            {
                int num;
                this.host.SetAxState(AxHost.siteProcessedInputKey, true);
                Message msg = new Message {
                    Msg = pMsg.message,
                    WParam = pMsg.wParam,
                    LParam = pMsg.lParam,
                    HWnd = pMsg.hwnd
                };
                try
                {
                    num = this.host.PreProcessMessage(ref msg) ? 0 : 1;
                }
                finally
                {
                    this.host.SetAxState(AxHost.siteProcessedInputKey, false);
                }
                return num;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite.CanInPlaceActivate()
            {
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite.ContextSensitiveHelp(int fEnterMode)
            {
                return -2147467263;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite.DeactivateAndUndo()
            {
                return this.host.GetInPlaceObject().UIDeactivate();
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite.DiscardUndoState()
            {
                return 0;
            }

            IntPtr System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite.GetWindow()
            {
                IntPtr ptr;
                try
                {
                    Control parentInternal = this.host.ParentInternal;
                    ptr = (parentInternal != null) ? parentInternal.Handle : IntPtr.Zero;
                }
                catch (Exception exception)
                {
                    throw exception;
                }
                return ptr;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite.GetWindowContext(out System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame ppFrame, out System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceUIWindow ppDoc, System.Windows.Forms.NativeMethods.COMRECT lprcPosRect, System.Windows.Forms.NativeMethods.COMRECT lprcClipRect, System.Windows.Forms.NativeMethods.tagOIFI lpFrameInfo)
            {
                ppDoc = null;
                ppFrame = this.host.GetParentContainer();
                AxHost.FillInRect(lprcPosRect, this.host.Bounds);
                this.host.GetClipRect(lprcClipRect);
                if (lpFrameInfo != null)
                {
                    lpFrameInfo.cb = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.tagOIFI));
                    lpFrameInfo.fMDIApp = false;
                    lpFrameInfo.hAccel = IntPtr.Zero;
                    lpFrameInfo.cAccelEntries = 0;
                    lpFrameInfo.hwndFrame = this.host.ParentInternal.Handle;
                }
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite.OnInPlaceActivate()
            {
                this.host.SetAxState(AxHost.ownDisposing, false);
                this.host.SetAxState(AxHost.rejectSelection, false);
                this.host.SetOcState(4);
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite.OnInPlaceDeactivate()
            {
                if (this.host.GetOcState() == 8)
                {
                    ((System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite) this).OnUIDeactivate(0);
                }
                this.host.GetParentContainer().OnInPlaceDeactivate(this.host);
                this.host.DetachWindow();
                this.host.SetOcState(2);
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite.OnPosRectChange(System.Windows.Forms.NativeMethods.COMRECT lprcPosRect)
            {
                bool axState = true;
                if (AxHost.windowsMediaPlayer_Clsid.Equals(this.host.clsid))
                {
                    axState = this.host.GetAxState(AxHost.handlePosRectChanged);
                }
                if (axState)
                {
                    this.host.GetInPlaceObject().SetObjectRects(lprcPosRect, this.host.GetClipRect(new System.Windows.Forms.NativeMethods.COMRECT()));
                    this.host.MakeDirty();
                }
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite.OnUIActivate()
            {
                this.host.SetOcState(8);
                this.host.GetParentContainer().OnUIActivate(this.host);
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite.OnUIDeactivate(int fUndoable)
            {
                this.host.GetParentContainer().OnUIDeactivate(this.host);
                if (this.host.GetOcState() > 4)
                {
                    this.host.SetOcState(4);
                }
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite.Scroll(System.Windows.Forms.NativeMethods.tagSIZE scrollExtant)
            {
                return 1;
            }

            void System.Windows.Forms.UnsafeNativeMethods.IPropertyNotifySink.OnChanged(int dispid)
            {
                if (this.host.NoComponentChangeEvents == 0)
                {
                    this.host.NoComponentChangeEvents++;
                    try
                    {
                        AxHost.AxPropertyDescriptor member = null;
                        if (dispid != -1)
                        {
                            member = this.host.GetPropertyDescriptorFromDispid(dispid);
                            if (member != null)
                            {
                                member.OnValueChanged(this.host);
                                if (!member.SettingValue)
                                {
                                    member.UpdateTypeConverterAndTypeEditor(true);
                                }
                            }
                        }
                        else
                        {
                            foreach (PropertyDescriptor descriptor2 in ((ICustomTypeDescriptor) this.host).GetProperties())
                            {
                                member = descriptor2 as AxHost.AxPropertyDescriptor;
                                if ((member != null) && !member.SettingValue)
                                {
                                    member.UpdateTypeConverterAndTypeEditor(true);
                                }
                            }
                        }
                        ISite site = this.host.Site;
                        if (site != null)
                        {
                            IComponentChangeService service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                            if (service != null)
                            {
                                try
                                {
                                    service.OnComponentChanging(this.host, member);
                                }
                                catch (CheckoutException exception)
                                {
                                    if (exception != CheckoutException.Canceled)
                                    {
                                        throw exception;
                                    }
                                    return;
                                }
                                service.OnComponentChanged(this.host, member, null, (member != null) ? member.GetValue(this.host) : null);
                            }
                        }
                    }
                    catch (Exception exception2)
                    {
                        throw exception2;
                    }
                    finally
                    {
                        this.host.NoComponentChangeEvents--;
                    }
                }
            }

            int System.Windows.Forms.UnsafeNativeMethods.IPropertyNotifySink.OnRequestEdit(int dispid)
            {
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.ISimpleFrameSite.PostMessageFilter(IntPtr hwnd, int msg, IntPtr wp, IntPtr lp, ref IntPtr plResult, int dwCookie)
            {
                return 1;
            }

            int System.Windows.Forms.UnsafeNativeMethods.ISimpleFrameSite.PreMessageFilter(IntPtr hwnd, int msg, IntPtr wp, IntPtr lp, ref IntPtr plResult, ref int pdwCookie)
            {
                return 0;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IVBGetControl.EnumControls(int dwOleContF, int dwWhich, out System.Windows.Forms.UnsafeNativeMethods.IEnumUnknown ppenum)
            {
                ppenum = null;
                ppenum = this.host.GetParentContainer().EnumControls(this.host, dwOleContF, dwWhich);
                return 0;
            }

            System.Type IReflect.UnderlyingSystemType
            {
                get
                {
                    return null;
                }
            }
        }

        internal class PropertyBagStream : System.Windows.Forms.UnsafeNativeMethods.IPropertyBag
        {
            private Hashtable bag = new Hashtable();

            internal void Read(Stream stream)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    this.bag = (Hashtable) formatter.Deserialize(stream);
                }
                catch
                {
                    this.bag = new Hashtable();
                }
            }

            int System.Windows.Forms.UnsafeNativeMethods.IPropertyBag.Read(string pszPropName, ref object pVar, System.Windows.Forms.UnsafeNativeMethods.IErrorLog pErrorLog)
            {
                if (this.bag.Contains(pszPropName))
                {
                    pVar = this.bag[pszPropName];
                    if (pVar != null)
                    {
                        return 0;
                    }
                }
                return -2147024809;
            }

            int System.Windows.Forms.UnsafeNativeMethods.IPropertyBag.Write(string pszPropName, ref object pVar)
            {
                if ((pVar == null) || pVar.GetType().IsSerializable)
                {
                    this.bag[pszPropName] = pVar;
                }
                return 0;
            }

            internal void Write(Stream stream)
            {
                new BinaryFormatter().Serialize(stream, this.bag);
            }
        }

        [Serializable, TypeConverter(typeof(TypeConverter)), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        public class State : ISerializable
        {
            private byte[] buffer;
            private System.Windows.Forms.UnsafeNativeMethods.ILockBytes iLockBytes;
            private int length;
            private string licenseKey;
            private bool manualUpdate;
            private MemoryStream ms;
            private AxHost.PropertyBagStream propBag;
            private System.Windows.Forms.UnsafeNativeMethods.IStorage storage;
            internal int type;
            private int VERSION;

            internal State(MemoryStream ms)
            {
                this.VERSION = 1;
                this.ms = ms;
                this.length = (int) ms.Length;
                this.InitializeFromStream(ms);
            }

            internal State(AxHost ctl)
            {
                this.VERSION = 1;
                this.CreateStorage();
                this.manualUpdate = ctl.GetAxState(AxHost.manualUpdate);
                this.licenseKey = ctl.GetLicenseKey();
                this.type = 2;
            }

            internal State(AxHost.PropertyBagStream propBag)
            {
                this.VERSION = 1;
                this.propBag = propBag;
            }

            protected State(SerializationInfo info, StreamingContext context)
            {
                this.VERSION = 1;
                SerializationInfoEnumerator enumerator = info.GetEnumerator();
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        if (string.Compare(enumerator.Name, "Data", true, CultureInfo.InvariantCulture) == 0)
                        {
                            try
                            {
                                byte[] buffer = (byte[]) enumerator.Value;
                                if (buffer != null)
                                {
                                    this.InitializeFromStream(new MemoryStream(buffer));
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        else if (string.Compare(enumerator.Name, "PropertyBagBinary", true, CultureInfo.InvariantCulture) == 0)
                        {
                            try
                            {
                                byte[] buffer2 = (byte[]) enumerator.Value;
                                if (buffer2 != null)
                                {
                                    this.propBag = new AxHost.PropertyBagStream();
                                    this.propBag.Read(new MemoryStream(buffer2));
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }

            internal State(MemoryStream ms, int storageType, AxHost ctl, AxHost.PropertyBagStream propBag)
            {
                this.VERSION = 1;
                this.type = storageType;
                this.propBag = propBag;
                this.length = (int) ms.Length;
                this.ms = ms;
                this.manualUpdate = ctl.GetAxState(AxHost.manualUpdate);
                this.licenseKey = ctl.GetLicenseKey();
            }

            public State(Stream ms, int storageType, bool manualUpdate, string licKey)
            {
                this.VERSION = 1;
                this.type = storageType;
                this.length = (int) ms.Length;
                this.manualUpdate = manualUpdate;
                this.licenseKey = licKey;
                this.InitializeBufferFromStream(ms);
            }

            internal string _GetLicenseKey()
            {
                return this.licenseKey;
            }

            internal bool _GetManualUpdate()
            {
                return this.manualUpdate;
            }

            private void CreateStorage()
            {
                IntPtr zero = IntPtr.Zero;
                if (this.buffer != null)
                {
                    zero = System.Windows.Forms.UnsafeNativeMethods.GlobalAlloc(2, this.length);
                    IntPtr destination = System.Windows.Forms.UnsafeNativeMethods.GlobalLock(new HandleRef(null, zero));
                    try
                    {
                        if (destination != IntPtr.Zero)
                        {
                            Marshal.Copy(this.buffer, 0, destination, this.length);
                        }
                    }
                    finally
                    {
                        System.Windows.Forms.UnsafeNativeMethods.GlobalUnlock(new HandleRef(null, zero));
                    }
                }
                bool flag = false;
                try
                {
                    this.iLockBytes = System.Windows.Forms.UnsafeNativeMethods.CreateILockBytesOnHGlobal(new HandleRef(null, zero), true);
                    if (this.buffer == null)
                    {
                        this.storage = System.Windows.Forms.UnsafeNativeMethods.StgCreateDocfileOnILockBytes(this.iLockBytes, 0x1012, 0);
                    }
                    else
                    {
                        this.storage = System.Windows.Forms.UnsafeNativeMethods.StgOpenStorageOnILockBytes(this.iLockBytes, null, 0x12, 0, 0);
                    }
                }
                catch (Exception)
                {
                    flag = true;
                }
                if (flag)
                {
                    if ((this.iLockBytes == null) && (zero != IntPtr.Zero))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.GlobalFree(new HandleRef(null, zero));
                    }
                    else
                    {
                        this.iLockBytes = null;
                    }
                    this.storage = null;
                }
            }

            internal System.Windows.Forms.UnsafeNativeMethods.IPropertyBag GetPropBag()
            {
                return this.propBag;
            }

            internal System.Windows.Forms.UnsafeNativeMethods.IStorage GetStorage()
            {
                if (this.storage == null)
                {
                    this.CreateStorage();
                }
                return this.storage;
            }

            internal System.Windows.Forms.UnsafeNativeMethods.IStream GetStream()
            {
                if (this.ms == null)
                {
                    if (this.buffer == null)
                    {
                        return null;
                    }
                    this.ms = new MemoryStream(this.buffer);
                }
                else
                {
                    this.ms.Seek(0L, SeekOrigin.Begin);
                }
                return new System.Windows.Forms.UnsafeNativeMethods.ComStreamFromDataStream(this.ms);
            }

            private void InitializeBufferFromStream(Stream ids)
            {
                BinaryReader reader = new BinaryReader(ids);
                this.length = reader.ReadInt32();
                if (this.length > 0)
                {
                    this.buffer = reader.ReadBytes(this.length);
                }
            }

            private void InitializeFromStream(Stream ids)
            {
                BinaryReader reader = new BinaryReader(ids);
                this.type = reader.ReadInt32();
                reader.ReadInt32();
                this.manualUpdate = reader.ReadBoolean();
                int count = reader.ReadInt32();
                if (count != 0)
                {
                    this.licenseKey = new string(reader.ReadChars(count));
                }
                for (int i = reader.ReadInt32(); i > 0; i--)
                {
                    int num3 = reader.ReadInt32();
                    ids.Position += num3;
                }
                this.length = reader.ReadInt32();
                if (this.length > 0)
                {
                    this.buffer = reader.ReadBytes(this.length);
                }
            }

            internal AxHost.State RefreshStorage(System.Windows.Forms.UnsafeNativeMethods.IPersistStorage iPersistStorage)
            {
                if ((this.storage == null) || (this.iLockBytes == null))
                {
                    return null;
                }
                iPersistStorage.Save(this.storage, true);
                this.storage.Commit(0);
                iPersistStorage.HandsOffStorage();
                try
                {
                    this.buffer = null;
                    this.ms = null;
                    System.Windows.Forms.NativeMethods.STATSTG pstatstg = new System.Windows.Forms.NativeMethods.STATSTG();
                    this.iLockBytes.Stat(pstatstg, 1);
                    this.length = (int) pstatstg.cbSize;
                    this.buffer = new byte[this.length];
                    IntPtr hGlobalFromILockBytes = System.Windows.Forms.UnsafeNativeMethods.GetHGlobalFromILockBytes(this.iLockBytes);
                    IntPtr source = System.Windows.Forms.UnsafeNativeMethods.GlobalLock(new HandleRef(null, hGlobalFromILockBytes));
                    try
                    {
                        if (source != IntPtr.Zero)
                        {
                            Marshal.Copy(source, this.buffer, 0, this.length);
                        }
                        else
                        {
                            this.length = 0;
                            this.buffer = null;
                        }
                    }
                    finally
                    {
                        System.Windows.Forms.UnsafeNativeMethods.GlobalUnlock(new HandleRef(null, hGlobalFromILockBytes));
                    }
                }
                finally
                {
                    iPersistStorage.SaveCompleted(this.storage);
                }
                return this;
            }

            internal void Save(MemoryStream stream)
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(this.type);
                writer.Write(this.VERSION);
                writer.Write(this.manualUpdate);
                if (this.licenseKey != null)
                {
                    writer.Write(this.licenseKey.Length);
                    writer.Write(this.licenseKey.ToCharArray());
                }
                else
                {
                    writer.Write(0);
                }
                writer.Write(0);
                writer.Write(this.length);
                if (this.buffer != null)
                {
                    writer.Write(this.buffer);
                }
                else if (this.ms != null)
                {
                    this.ms.Position = 0L;
                    this.ms.WriteTo(stream);
                }
            }

            void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
            {
                System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
                MemoryStream stream = new MemoryStream();
                this.Save(stream);
                si.AddValue("Data", stream.ToArray());
                if (this.propBag != null)
                {
                    try
                    {
                        stream = new MemoryStream();
                        this.propBag.Write(stream);
                        si.AddValue("PropertyBagBinary", stream.ToArray());
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            internal int Type
            {
                get
                {
                    return this.type;
                }
                set
                {
                    this.type = value;
                }
            }
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public class StateConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
            {
                return ((sourceType == typeof(byte[])) || base.CanConvertFrom(context, sourceType));
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
            {
                return ((destinationType == typeof(byte[])) || base.CanConvertTo(context, destinationType));
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is byte[])
                {
                    return new AxHost.State(new MemoryStream((byte[]) value));
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
            {
                if (destinationType == null)
                {
                    throw new ArgumentNullException("destinationType");
                }
                if (!(destinationType == typeof(byte[])))
                {
                    return base.ConvertTo(context, culture, value, destinationType);
                }
                if (value != null)
                {
                    MemoryStream stream = new MemoryStream();
                    ((AxHost.State) value).Save(stream);
                    stream.Close();
                    return stream.ToArray();
                }
                return new byte[0];
            }
        }

        [AttributeUsage(AttributeTargets.Assembly, Inherited=false)]
        public sealed class TypeLibraryTimeStampAttribute : Attribute
        {
            private DateTime val;

            public TypeLibraryTimeStampAttribute(string timestamp)
            {
                this.val = DateTime.Parse(timestamp, CultureInfo.InvariantCulture);
            }

            public DateTime Value
            {
                get
                {
                    return this.val;
                }
            }
        }

        private class VBFormat : System.Windows.Forms.UnsafeNativeMethods.IVBFormat
        {
            int System.Windows.Forms.UnsafeNativeMethods.IVBFormat.Format(ref object var, IntPtr pszFormat, IntPtr lpBuffer, short cpBuffer, int lcid, short firstD, short firstW, short[] result)
            {
                if (result == null)
                {
                    return -2147024809;
                }
                result[0] = 0;
                if ((lpBuffer == IntPtr.Zero) || (cpBuffer < 2))
                {
                    return -2147024809;
                }
                IntPtr zero = IntPtr.Zero;
                System.Windows.Forms.UnsafeNativeMethods.VarFormat(ref var, new HandleRef(null, pszFormat), firstD, firstW, 0x20, ref zero);
                try
                {
                    int num = 0;
                    if (zero != IntPtr.Zero)
                    {
                        short val = 0;
                        cpBuffer = (short) (cpBuffer - 1);
                        while ((num < cpBuffer) && ((val = Marshal.ReadInt16(zero, num * 2)) != 0))
                        {
                            Marshal.WriteInt16(lpBuffer, num * 2, val);
                            num++;
                        }
                    }
                    Marshal.WriteInt16(lpBuffer, num * 2, (short) 0);
                    result[0] = (short) num;
                }
                finally
                {
                    System.Windows.Forms.SafeNativeMethods.SysFreeString(new HandleRef(null, zero));
                }
                return 0;
            }
        }
    }
}

