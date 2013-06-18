namespace System.Windows.Forms
{
    using Accessibility;
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.Internal;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms.Internal;
    using System.Windows.Forms.Layout;

    [ToolboxItemFilter("System.Windows.Forms"), Designer("System.Windows.Forms.Design.ControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DesignerSerializer("System.Windows.Forms.Design.ControlCodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("Text"), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultEvent("Click")]
    public class Control : Component, System.Windows.Forms.UnsafeNativeMethods.IOleControl, System.Windows.Forms.UnsafeNativeMethods.IOleObject, System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject, System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject, System.Windows.Forms.UnsafeNativeMethods.IOleWindow, System.Windows.Forms.UnsafeNativeMethods.IViewObject, System.Windows.Forms.UnsafeNativeMethods.IViewObject2, System.Windows.Forms.UnsafeNativeMethods.IPersist, System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit, System.Windows.Forms.UnsafeNativeMethods.IPersistPropertyBag, System.Windows.Forms.UnsafeNativeMethods.IPersistStorage, System.Windows.Forms.UnsafeNativeMethods.IQuickActivate, ISupportOleDropSource, IDropTarget, ISynchronizeInvoke, IWin32Window, IArrangedElement, IBindableComponent, IComponent, IDisposable
    {
        internal static readonly BooleanSwitch BufferPinkRect;
        private LayoutEventArgs cachedLayoutEventArgs;
        private static bool checkForIllegalCrossThreadCalls = Debugger.IsAttached;
        private int clientHeight;
        private int clientWidth;
        internal static readonly TraceSwitch ControlKeyboardRouting;
        private ControlStyles controlStyle;
        private System.Windows.Forms.CreateParams createParams;
        [ThreadStatic]
        internal static HelpInfo currentHelpInfo = null;
        private static System.Drawing.Font defaultFont;
        private static FontHandleWrapper defaultFontHandleWrapper;
        private static readonly object EventAutoSizeChanged = new object();
        private static readonly object EventBackColor = new object();
        private static readonly object EventBackgroundImage = new object();
        private static readonly object EventBackgroundImageLayout = new object();
        private static readonly object EventBindingContext = new object();
        private static readonly object EventCausesValidation = new object();
        private static readonly object EventChangeUICues = new object();
        private static readonly object EventClick = new object();
        private static readonly object EventClientSize = new object();
        private static readonly object EventContextMenu = new object();
        private static readonly object EventContextMenuStrip = new object();
        private static readonly object EventControlAdded = new object();
        private static readonly object EventControlRemoved = new object();
        private static readonly object EventCursor = new object();
        private static readonly object EventDock = new object();
        private static readonly object EventDoubleClick = new object();
        private static readonly object EventDragDrop = new object();
        private static readonly object EventDragEnter = new object();
        private static readonly object EventDragLeave = new object();
        private static readonly object EventDragOver = new object();
        private static readonly object EventEnabled = new object();
        private static readonly object EventEnabledChanged = new object();
        private static readonly object EventEnter = new object();
        private static readonly object EventFont = new object();
        private static readonly object EventForeColor = new object();
        private static readonly object EventGiveFeedback = new object();
        private static readonly object EventGotFocus = new object();
        private static readonly object EventHandleCreated = new object();
        private static readonly object EventHandleDestroyed = new object();
        private static readonly object EventHelpRequested = new object();
        private static readonly object EventImeModeChanged = new object();
        private static readonly object EventInvalidated = new object();
        private static readonly object EventKeyDown = new object();
        private static readonly object EventKeyPress = new object();
        private static readonly object EventKeyUp = new object();
        private static readonly object EventLayout = new object();
        private static readonly object EventLeave = new object();
        private static readonly object EventLocation = new object();
        private static readonly object EventLostFocus = new object();
        private static readonly object EventMarginChanged = new object();
        private static readonly object EventMouseCaptureChanged = new object();
        private static readonly object EventMouseClick = new object();
        private static readonly object EventMouseDoubleClick = new object();
        private static readonly object EventMouseDown = new object();
        private static readonly object EventMouseEnter = new object();
        private static readonly object EventMouseHover = new object();
        private static readonly object EventMouseLeave = new object();
        private static readonly object EventMouseMove = new object();
        private static readonly object EventMouseUp = new object();
        private static readonly object EventMouseWheel = new object();
        private static readonly object EventMove = new object();
        internal static readonly object EventPaddingChanged = new object();
        private static readonly object EventPaint = new object();
        private static readonly object EventParent = new object();
        private static readonly object EventPreviewKeyDown = new object();
        private static readonly object EventQueryAccessibilityHelp = new object();
        private static readonly object EventQueryContinueDrag = new object();
        private static readonly object EventRegionChanged = new object();
        private static readonly object EventResize = new object();
        private static readonly object EventRightToLeft = new object();
        private static readonly object EventSize = new object();
        private static readonly object EventStyleChanged = new object();
        private static readonly object EventSystemColorsChanged = new object();
        private static readonly object EventTabIndex = new object();
        private static readonly object EventTabStop = new object();
        private static readonly object EventText = new object();
        private static readonly object EventValidated = new object();
        private static readonly object EventValidating = new object();
        private static readonly object EventVisible = new object();
        private static readonly object EventVisibleChanged = new object();
        internal static readonly TraceSwitch FocusTracing;
        private int height;
        private static bool ignoreWmImeNotify;
        private const int ImeCharsToIgnoreDisabled = -1;
        private const int ImeCharsToIgnoreEnabled = 0;
        [ThreadStatic]
        private static bool inCrossThreadSafeCall = false;
        private static ContextCallback invokeMarshaledCallbackHelperDelegate;
        private byte layoutSuspendCount;
        private static bool mouseWheelInit;
        private static int mouseWheelMessage = 0x20a;
        private static bool mouseWheelRoutingNeeded;
        private const short PaintLayerBackground = 1;
        private const short PaintLayerForeground = 2;
        internal static readonly TraceSwitch PaletteTracing;
        private Control parent;
        private static readonly int PropAccessibility = PropertyStore.CreateKey();
        private static readonly int PropAccessibleDefaultActionDescription = PropertyStore.CreateKey();
        private static readonly int PropAccessibleDescription = PropertyStore.CreateKey();
        private static readonly int PropAccessibleHelpProvider = PropertyStore.CreateKey();
        private static readonly int PropAccessibleName = PropertyStore.CreateKey();
        private static readonly int PropAccessibleRole = PropertyStore.CreateKey();
        private static readonly int PropActiveXImpl = PropertyStore.CreateKey();
        private static System.Windows.Forms.ImeMode propagatingImeMode = System.Windows.Forms.ImeMode.Inherit;
        private static readonly int PropAmbientPropertiesService = PropertyStore.CreateKey();
        private static readonly int PropAutoScrollOffset = PropertyStore.CreateKey();
        private static readonly int PropBackBrush = PropertyStore.CreateKey();
        private static readonly int PropBackColor = PropertyStore.CreateKey();
        private static readonly int PropBackgroundImage = PropertyStore.CreateKey();
        private static readonly int PropBackgroundImageLayout = PropertyStore.CreateKey();
        private static readonly int PropBindingManager = PropertyStore.CreateKey();
        private static readonly int PropBindings = PropertyStore.CreateKey();
        private static readonly int PropCacheTextCount = PropertyStore.CreateKey();
        private static readonly int PropCacheTextField = PropertyStore.CreateKey();
        private static readonly int PropContextMenu = PropertyStore.CreateKey();
        private static readonly int PropContextMenuStrip = PropertyStore.CreateKey();
        private static readonly int PropControlsCollection = PropertyStore.CreateKey();
        private static readonly int PropControlVersionInfo = PropertyStore.CreateKey();
        private static readonly int PropCurrentAmbientFont = PropertyStore.CreateKey();
        private static readonly int PropCursor = PropertyStore.CreateKey();
        private static readonly int PropDisableImeModeChangedCount = PropertyStore.CreateKey();
        private PropertyStore propertyStore;
        private static readonly int PropFont = PropertyStore.CreateKey();
        private static readonly int PropFontHandleWrapper = PropertyStore.CreateKey();
        private static readonly int PropFontHeight = PropertyStore.CreateKey();
        private static readonly int PropForeColor = PropertyStore.CreateKey();
        private static readonly int PropImeMode = PropertyStore.CreateKey();
        private static readonly int PropImeWmCharsToIgnore = PropertyStore.CreateKey();
        private static readonly int PropLastCanEnableIme = PropertyStore.CreateKey();
        private static readonly int PropName = PropertyStore.CreateKey();
        private static readonly int PropNcAccessibility = PropertyStore.CreateKey();
        private static readonly int PropPaintingException = PropertyStore.CreateKey();
        private static readonly int PropRegion = PropertyStore.CreateKey();
        private static readonly int PropRightToLeft = PropertyStore.CreateKey();
        private static readonly int PropUseCompatibleTextRendering = PropertyStore.CreateKey();
        private static readonly int PropUserData = PropertyStore.CreateKey();
        private Control reflectParent;
        private byte requiredScaling;
        private const byte RequiredScalingEnabledMask = 0x10;
        private const byte RequiredScalingMask = 15;
        private int state;
        internal const int STATE_ALLOWDROP = 0x40;
        internal const int STATE_CAUSESVALIDATION = 0x20000;
        internal const int STATE_CHECKEDHOST = 0x1000000;
        internal const int STATE_CREATED = 1;
        internal const int STATE_CREATINGHANDLE = 0x40000;
        internal const int STATE_DISPOSED = 0x800;
        internal const int STATE_DISPOSING = 0x1000;
        internal const int STATE_DOUBLECLICKFIRED = 0x4000000;
        internal const int STATE_DROPTARGET = 0x80;
        internal const int STATE_ENABLED = 4;
        internal const int STATE_EXCEPTIONWHILEPAINTING = 0x400000;
        internal const int STATE_HOSTEDINDIALOG = 0x2000000;
        internal const int STATE_ISACCESSIBLE = 0x100000;
        internal const int STATE_LAYOUTDEFERRED = 0x200;
        internal const int STATE_LAYOUTISDIRTY = 0x800000;
        internal const int STATE_MIRRORED = 0x40000000;
        internal const int STATE_MODAL = 0x20;
        internal const int STATE_MOUSEENTERPENDING = 0x2000;
        internal const int STATE_MOUSEPRESSED = 0x8000000;
        internal const int STATE_NOZORDER = 0x100;
        internal const int STATE_OWNCTLBRUSH = 0x200000;
        internal const int STATE_PARENTRECREATING = 0x20000000;
        internal const int STATE_RECREATE = 0x10;
        internal const int STATE_SIZELOCKEDBYOS = 0x10000;
        internal const int STATE_TABSTOP = 8;
        internal const int STATE_THREADMARSHALLPENDING = 0x8000;
        internal const int STATE_TOPLEVEL = 0x80000;
        internal const int STATE_TRACKINGMOUSEEVENT = 0x4000;
        internal const int STATE_USEWAITCURSOR = 0x400;
        internal const int STATE_VALIDATIONCANCELLED = 0x10000000;
        internal const int STATE_VISIBLE = 2;
        private int state2;
        private const int STATE2_BECOMINGACTIVECONTROL = 0x20;
        private const int STATE2_CLEARLAYOUTARGS = 0x40;
        private const int STATE2_HAVEINVOKED = 1;
        private const int STATE2_INPUTCHAR = 0x100;
        private const int STATE2_INPUTKEY = 0x80;
        internal const int STATE2_INTERESTEDINUSERPREFERENCECHANGED = 8;
        private const int STATE2_ISACTIVEX = 0x400;
        private const int STATE2_LISTENINGTOUSERPREFERENCECHANGED = 4;
        internal const int STATE2_MAINTAINSOWNCAPTUREMODE = 0x10;
        private const int STATE2_SETSCROLLPOS = 2;
        private const int STATE2_UICUES = 0x200;
        internal const int STATE2_USEPREFERREDSIZECACHE = 0x800;
        private int tabIndex;
        private string text;
        private Queue threadCallbackList;
        private static int threadCallbackMessage;
        private System.Windows.Forms.NativeMethods.TRACKMOUSEEVENT trackMouseEvent;
        private int uiCuesState;
        private const int UISTATE_FOCUS_CUES_HIDDEN = 1;
        private const int UISTATE_FOCUS_CUES_MASK = 15;
        private const int UISTATE_FOCUS_CUES_SHOW = 2;
        private const int UISTATE_KEYBOARD_CUES_HIDDEN = 0x10;
        private const int UISTATE_KEYBOARD_CUES_MASK = 240;
        private const int UISTATE_KEYBOARD_CUES_SHOW = 0x20;
        private short updateCount;
        internal static bool UseCompatibleTextRenderingDefault = true;
        private int width;
        private ControlNativeWindow window;
        private static int WM_GETCONTROLNAME = System.Windows.Forms.SafeNativeMethods.RegisterWindowMessage("WM_GETCONTROLNAME");
        private static int WM_GETCONTROLTYPE = System.Windows.Forms.SafeNativeMethods.RegisterWindowMessage("WM_GETCONTROLTYPE");
        private int x;
        private int y;

        [System.Windows.Forms.SRDescription("ControlOnAutoSizeChangedDescr"), Browsable(false), System.Windows.Forms.SRCategory("CatPropertyChanged"), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler AutoSizeChanged
        {
            add
            {
                base.Events.AddHandler(EventAutoSizeChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventAutoSizeChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnBackColorChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler BackColorChanged
        {
            add
            {
                base.Events.AddHandler(EventBackColor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventBackColor, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnBackgroundImageChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler BackgroundImageChanged
        {
            add
            {
                base.Events.AddHandler(EventBackgroundImage, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventBackgroundImage, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnBackgroundImageLayoutChangedDescr")]
        public event EventHandler BackgroundImageLayoutChanged
        {
            add
            {
                base.Events.AddHandler(EventBackgroundImageLayout, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventBackgroundImageLayout, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnBindingContextChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler BindingContextChanged
        {
            add
            {
                base.Events.AddHandler(EventBindingContext, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventBindingContext, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnCausesValidationChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler CausesValidationChanged
        {
            add
            {
                base.Events.AddHandler(EventCausesValidation, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCausesValidation, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ControlOnChangeUICuesDescr")]
        public event UICuesEventHandler ChangeUICues
        {
            add
            {
                base.Events.AddHandler(EventChangeUICues, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventChangeUICues, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("ControlOnClickDescr")]
        public event EventHandler Click
        {
            add
            {
                base.Events.AddHandler(EventClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventClick, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnClientSizeChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler ClientSizeChanged
        {
            add
            {
                base.Events.AddHandler(EventClientSize, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventClientSize, value);
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("ControlOnContextMenuChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler ContextMenuChanged
        {
            add
            {
                base.Events.AddHandler(EventContextMenu, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventContextMenu, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlContextMenuStripChangedDescr")]
        public event EventHandler ContextMenuStripChanged
        {
            add
            {
                base.Events.AddHandler(EventContextMenuStrip, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventContextMenuStrip, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Browsable(true), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRDescription("ControlOnControlAddedDescr")]
        public event ControlEventHandler ControlAdded
        {
            add
            {
                base.Events.AddHandler(EventControlAdded, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventControlAdded, value);
            }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRDescription("ControlOnControlRemovedDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event ControlEventHandler ControlRemoved
        {
            add
            {
                base.Events.AddHandler(EventControlRemoved, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventControlRemoved, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnCursorChangedDescr")]
        public event EventHandler CursorChanged
        {
            add
            {
                base.Events.AddHandler(EventCursor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCursor, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnDockChangedDescr")]
        public event EventHandler DockChanged
        {
            add
            {
                base.Events.AddHandler(EventDock, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDock, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnDoubleClickDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event EventHandler DoubleClick
        {
            add
            {
                base.Events.AddHandler(EventDoubleClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDoubleClick, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnDragDropDescr"), System.Windows.Forms.SRCategory("CatDragDrop")]
        public event DragEventHandler DragDrop
        {
            add
            {
                base.Events.AddHandler(EventDragDrop, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDragDrop, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnDragEnterDescr"), System.Windows.Forms.SRCategory("CatDragDrop")]
        public event DragEventHandler DragEnter
        {
            add
            {
                base.Events.AddHandler(EventDragEnter, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDragEnter, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatDragDrop"), System.Windows.Forms.SRDescription("ControlOnDragLeaveDescr")]
        public event EventHandler DragLeave
        {
            add
            {
                base.Events.AddHandler(EventDragLeave, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDragLeave, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatDragDrop"), System.Windows.Forms.SRDescription("ControlOnDragOverDescr")]
        public event DragEventHandler DragOver
        {
            add
            {
                base.Events.AddHandler(EventDragOver, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDragOver, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnEnabledChangedDescr")]
        public event EventHandler EnabledChanged
        {
            add
            {
                base.Events.AddHandler(EventEnabled, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventEnabled, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatFocus"), System.Windows.Forms.SRDescription("ControlOnEnterDescr")]
        public event EventHandler Enter
        {
            add
            {
                base.Events.AddHandler(EventEnter, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventEnter, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnFontChangedDescr")]
        public event EventHandler FontChanged
        {
            add
            {
                base.Events.AddHandler(EventFont, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventFont, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnForeColorChangedDescr")]
        public event EventHandler ForeColorChanged
        {
            add
            {
                base.Events.AddHandler(EventForeColor, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventForeColor, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnGiveFeedbackDescr"), System.Windows.Forms.SRCategory("CatDragDrop")]
        public event GiveFeedbackEventHandler GiveFeedback
        {
            add
            {
                base.Events.AddHandler(EventGiveFeedback, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventGiveFeedback, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatFocus"), System.Windows.Forms.SRDescription("ControlOnGotFocusDescr"), EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public event EventHandler GotFocus
        {
            add
            {
                base.Events.AddHandler(EventGotFocus, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventGotFocus, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPrivate"), System.Windows.Forms.SRDescription("ControlOnCreateHandleDescr"), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler HandleCreated
        {
            add
            {
                base.Events.AddHandler(EventHandleCreated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventHandleCreated, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), System.Windows.Forms.SRDescription("ControlOnDestroyHandleDescr"), System.Windows.Forms.SRCategory("CatPrivate")]
        public event EventHandler HandleDestroyed
        {
            add
            {
                base.Events.AddHandler(EventHandleDestroyed, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventHandleDestroyed, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnHelpDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event HelpEventHandler HelpRequested
        {
            add
            {
                base.Events.AddHandler(EventHelpRequested, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventHelpRequested, value);
            }
        }

        [WinCategory("Behavior"), System.Windows.Forms.SRDescription("ControlOnImeModeChangedDescr")]
        public event EventHandler ImeModeChanged
        {
            add
            {
                base.Events.AddHandler(EventImeModeChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventImeModeChanged, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ControlOnInvalidateDescr"), Browsable(false)]
        public event InvalidateEventHandler Invalidated
        {
            add
            {
                base.Events.AddHandler(EventInvalidated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventInvalidated, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnKeyDownDescr"), System.Windows.Forms.SRCategory("CatKey")]
        public event KeyEventHandler KeyDown
        {
            add
            {
                base.Events.AddHandler(EventKeyDown, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventKeyDown, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatKey"), System.Windows.Forms.SRDescription("ControlOnKeyPressDescr")]
        public event KeyPressEventHandler KeyPress
        {
            add
            {
                base.Events.AddHandler(EventKeyPress, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventKeyPress, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnKeyUpDescr"), System.Windows.Forms.SRCategory("CatKey")]
        public event KeyEventHandler KeyUp
        {
            add
            {
                base.Events.AddHandler(EventKeyUp, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventKeyUp, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnLayoutDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public event LayoutEventHandler Layout
        {
            add
            {
                base.Events.AddHandler(EventLayout, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLayout, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatFocus"), System.Windows.Forms.SRDescription("ControlOnLeaveDescr")]
        public event EventHandler Leave
        {
            add
            {
                base.Events.AddHandler(EventLeave, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLeave, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnLocationChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler LocationChanged
        {
            add
            {
                base.Events.AddHandler(EventLocation, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLocation, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRDescription("ControlOnLostFocusDescr"), Browsable(false), System.Windows.Forms.SRCategory("CatFocus")]
        public event EventHandler LostFocus
        {
            add
            {
                base.Events.AddHandler(EventLostFocus, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLostFocus, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnMarginChangedDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public event EventHandler MarginChanged
        {
            add
            {
                base.Events.AddHandler(EventMarginChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMarginChanged, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("ControlOnMouseCaptureChangedDescr")]
        public event EventHandler MouseCaptureChanged
        {
            add
            {
                base.Events.AddHandler(EventMouseCaptureChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseCaptureChanged, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("ControlOnMouseClickDescr")]
        public event MouseEventHandler MouseClick
        {
            add
            {
                base.Events.AddHandler(EventMouseClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseClick, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("ControlOnMouseDoubleClickDescr")]
        public event MouseEventHandler MouseDoubleClick
        {
            add
            {
                base.Events.AddHandler(EventMouseDoubleClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseDoubleClick, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatMouse"), System.Windows.Forms.SRDescription("ControlOnMouseDownDescr")]
        public event MouseEventHandler MouseDown
        {
            add
            {
                base.Events.AddHandler(EventMouseDown, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseDown, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatMouse"), System.Windows.Forms.SRDescription("ControlOnMouseEnterDescr")]
        public event EventHandler MouseEnter
        {
            add
            {
                base.Events.AddHandler(EventMouseEnter, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseEnter, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatMouse"), System.Windows.Forms.SRDescription("ControlOnMouseHoverDescr")]
        public event EventHandler MouseHover
        {
            add
            {
                base.Events.AddHandler(EventMouseHover, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseHover, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatMouse"), System.Windows.Forms.SRDescription("ControlOnMouseLeaveDescr")]
        public event EventHandler MouseLeave
        {
            add
            {
                base.Events.AddHandler(EventMouseLeave, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseLeave, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnMouseMoveDescr"), System.Windows.Forms.SRCategory("CatMouse")]
        public event MouseEventHandler MouseMove
        {
            add
            {
                base.Events.AddHandler(EventMouseMove, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseMove, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnMouseUpDescr"), System.Windows.Forms.SRCategory("CatMouse")]
        public event MouseEventHandler MouseUp
        {
            add
            {
                base.Events.AddHandler(EventMouseUp, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseUp, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRDescription("ControlOnMouseWheelDescr"), Browsable(false), System.Windows.Forms.SRCategory("CatMouse")]
        public event MouseEventHandler MouseWheel
        {
            add
            {
                base.Events.AddHandler(EventMouseWheel, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseWheel, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnMoveDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public event EventHandler Move
        {
            add
            {
                base.Events.AddHandler(EventMove, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMove, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ControlOnPaddingChangedDescr")]
        public event EventHandler PaddingChanged
        {
            add
            {
                base.Events.AddHandler(EventPaddingChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPaddingChanged, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ControlOnPaintDescr")]
        public event PaintEventHandler Paint
        {
            add
            {
                base.Events.AddHandler(EventPaint, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPaint, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnParentChangedDescr")]
        public event EventHandler ParentChanged
        {
            add
            {
                base.Events.AddHandler(EventParent, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventParent, value);
            }
        }

        [System.Windows.Forms.SRDescription("PreviewKeyDownDescr"), System.Windows.Forms.SRCategory("CatKey")]
        public event PreviewKeyDownEventHandler PreviewKeyDown
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)] add
            {
                base.Events.AddHandler(EventPreviewKeyDown, value);
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)] remove
            {
                base.Events.RemoveHandler(EventPreviewKeyDown, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ControlOnQueryAccessibilityHelpDescr")]
        public event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp
        {
            add
            {
                base.Events.AddHandler(EventQueryAccessibilityHelp, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventQueryAccessibilityHelp, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatDragDrop"), System.Windows.Forms.SRDescription("ControlOnQueryContinueDragDescr")]
        public event QueryContinueDragEventHandler QueryContinueDrag
        {
            add
            {
                base.Events.AddHandler(EventQueryContinueDrag, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventQueryContinueDrag, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlRegionChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler RegionChanged
        {
            add
            {
                base.Events.AddHandler(EventRegionChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRegionChanged, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ControlOnResizeDescr")]
        public event EventHandler Resize
        {
            add
            {
                base.Events.AddHandler(EventResize, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventResize, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnRightToLeftChangedDescr")]
        public event EventHandler RightToLeftChanged
        {
            add
            {
                base.Events.AddHandler(EventRightToLeft, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRightToLeft, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnSizeChangedDescr")]
        public event EventHandler SizeChanged
        {
            add
            {
                base.Events.AddHandler(EventSize, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSize, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ControlOnStyleChangedDescr")]
        public event EventHandler StyleChanged
        {
            add
            {
                base.Events.AddHandler(EventStyleChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventStyleChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnSystemColorsChangedDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler SystemColorsChanged
        {
            add
            {
                base.Events.AddHandler(EventSystemColorsChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSystemColorsChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnTabIndexChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler TabIndexChanged
        {
            add
            {
                base.Events.AddHandler(EventTabIndex, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventTabIndex, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnTabStopChangedDescr")]
        public event EventHandler TabStopChanged
        {
            add
            {
                base.Events.AddHandler(EventTabStop, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventTabStop, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnTextChangedDescr")]
        public event EventHandler TextChanged
        {
            add
            {
                base.Events.AddHandler(EventText, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventText, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatFocus"), System.Windows.Forms.SRDescription("ControlOnValidatedDescr")]
        public event EventHandler Validated
        {
            add
            {
                base.Events.AddHandler(EventValidated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventValidated, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnValidatingDescr"), System.Windows.Forms.SRCategory("CatFocus")]
        public event CancelEventHandler Validating
        {
            add
            {
                base.Events.AddHandler(EventValidating, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventValidating, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnVisibleChangedDescr")]
        public event EventHandler VisibleChanged
        {
            add
            {
                base.Events.AddHandler(EventVisible, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventVisible, value);
            }
        }

        public Control() : this(true)
        {
        }

        internal Control(bool autoInstallSyncContext)
        {
            this.propertyStore = new PropertyStore();
            this.window = new ControlNativeWindow(this);
            this.RequiredScalingEnabled = true;
            this.RequiredScaling = BoundsSpecified.All;
            this.tabIndex = -1;
            this.state = 0x2000e;
            this.state2 = 8;
            this.SetStyle(ControlStyles.UseTextForAccessibility | ControlStyles.AllPaintingInWmPaint | ControlStyles.StandardDoubleClick | ControlStyles.Selectable | ControlStyles.StandardClick | ControlStyles.UserPaint, true);
            this.InitMouseWheelSupport();
            if (this.DefaultMargin != CommonProperties.DefaultMargin)
            {
                this.Margin = this.DefaultMargin;
            }
            if (this.DefaultMinimumSize != CommonProperties.DefaultMinimumSize)
            {
                this.MinimumSize = this.DefaultMinimumSize;
            }
            if (this.DefaultMaximumSize != CommonProperties.DefaultMaximumSize)
            {
                this.MaximumSize = this.DefaultMaximumSize;
            }
            System.Drawing.Size defaultSize = this.DefaultSize;
            this.width = defaultSize.Width;
            this.height = defaultSize.Height;
            CommonProperties.xClearPreferredSizeCache(this);
            if ((this.width != 0) && (this.height != 0))
            {
                System.Windows.Forms.NativeMethods.RECT rect;
                rect = new System.Windows.Forms.NativeMethods.RECT {
                    left = rect.right = rect.top = rect.bottom = 0
                };
                System.Windows.Forms.CreateParams createParams = this.CreateParams;
                System.Windows.Forms.SafeNativeMethods.AdjustWindowRectEx(ref rect, createParams.Style, false, createParams.ExStyle);
                this.clientWidth = this.width - (rect.right - rect.left);
                this.clientHeight = this.height - (rect.bottom - rect.top);
            }
            if (autoInstallSyncContext)
            {
                WindowsFormsSynchronizationContext.InstallIfNeeded();
            }
        }

        public Control(string text) : this(null, text)
        {
        }

        public Control(Control parent, string text) : this()
        {
            this.Parent = parent;
            this.Text = text;
        }

        public Control(string text, int left, int top, int width, int height) : this(null, text, left, top, width, height)
        {
        }

        public Control(Control parent, string text, int left, int top, int width, int height) : this(parent, text)
        {
            this.Location = new Point(left, top);
            this.Size = new System.Drawing.Size(width, height);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal void AccessibilityNotifyClients(AccessibleEvents accEvent, int childID)
        {
            this.AccessibilityNotifyClients(accEvent, -4, childID);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void AccessibilityNotifyClients(AccessibleEvents accEvent, int objectID, int childID)
        {
            if (this.IsHandleCreated)
            {
                System.Windows.Forms.UnsafeNativeMethods.NotifyWinEvent((int) accEvent, new HandleRef(this, this.Handle), objectID, childID + 1);
            }
        }

        private IntPtr ActiveXMergeRegion(IntPtr region)
        {
            return this.ActiveXInstance.MergeRegion(region);
        }

        private void ActiveXOnFocus(bool focus)
        {
            this.ActiveXInstance.OnFocus(focus);
        }

        private void ActiveXUpdateBounds(ref int x, ref int y, ref int width, ref int height, int flags)
        {
            this.ActiveXInstance.UpdateBounds(ref x, ref y, ref width, ref height, flags);
        }

        private void ActiveXViewChanged()
        {
            this.ActiveXInstance.ViewChangedInternal();
        }

        internal virtual void AddReflectChild()
        {
        }

        internal virtual Rectangle ApplyBoundsConstraints(int suggestedX, int suggestedY, int proposedWidth, int proposedHeight)
        {
            Rectangle rectangle;
            if (!(this.MaximumSize != System.Drawing.Size.Empty) && !(this.MinimumSize != System.Drawing.Size.Empty))
            {
                return new Rectangle(suggestedX, suggestedY, proposedWidth, proposedHeight);
            }
            System.Drawing.Size b = LayoutUtils.ConvertZeroToUnbounded(this.MaximumSize);
            return new Rectangle(suggestedX, suggestedY, 0, 0) { Size = LayoutUtils.IntersectSizes(new System.Drawing.Size(proposedWidth, proposedHeight), b), Size = LayoutUtils.UnionSizes(rectangle.Size, this.MinimumSize) };
        }

        internal System.Drawing.Size ApplySizeConstraints(System.Drawing.Size proposedSize)
        {
            return this.ApplyBoundsConstraints(0, 0, proposedSize.Width, proposedSize.Height).Size;
        }

        internal System.Drawing.Size ApplySizeConstraints(int width, int height)
        {
            return this.ApplyBoundsConstraints(0, 0, width, height).Size;
        }

        internal virtual void AssignParent(Control value)
        {
            if (value != null)
            {
                this.RequiredScalingEnabled = value.RequiredScalingEnabled;
            }
            if (this.CanAccessProperties)
            {
                System.Drawing.Font font = this.Font;
                System.Drawing.Color foreColor = this.ForeColor;
                System.Drawing.Color backColor = this.BackColor;
                System.Windows.Forms.RightToLeft rightToLeft = this.RightToLeft;
                bool enabled = this.Enabled;
                bool visible = this.Visible;
                this.parent = value;
                this.OnParentChanged(EventArgs.Empty);
                if (this.GetAnyDisposingInHierarchy())
                {
                    return;
                }
                if (enabled != this.Enabled)
                {
                    this.OnEnabledChanged(EventArgs.Empty);
                }
                bool flag3 = this.Visible;
                if ((visible != flag3) && ((visible || !flag3) || ((this.parent != null) || this.GetTopLevel())))
                {
                    this.OnVisibleChanged(EventArgs.Empty);
                }
                if (!font.Equals(this.Font))
                {
                    this.OnFontChanged(EventArgs.Empty);
                }
                if (!foreColor.Equals(this.ForeColor))
                {
                    this.OnForeColorChanged(EventArgs.Empty);
                }
                if (!backColor.Equals(this.BackColor))
                {
                    this.OnBackColorChanged(EventArgs.Empty);
                }
                if (rightToLeft != this.RightToLeft)
                {
                    this.OnRightToLeftChanged(EventArgs.Empty);
                }
                if ((this.Properties.GetObject(PropBindingManager) == null) && this.Created)
                {
                    this.OnBindingContextChanged(EventArgs.Empty);
                }
            }
            else
            {
                this.parent = value;
                this.OnParentChanged(EventArgs.Empty);
            }
            this.SetState(0x1000000, false);
            if (this.ParentInternal != null)
            {
                this.ParentInternal.LayoutEngine.InitLayout(this, BoundsSpecified.All);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IAsyncResult BeginInvoke(Delegate method)
        {
            return this.BeginInvoke(method, null);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IAsyncResult BeginInvoke(Delegate method, params object[] args)
        {
            using (new MultithreadSafeCallScope())
            {
                return (IAsyncResult) this.FindMarshalingControl().MarshaledInvoke(this, method, args, false);
            }
        }

        internal void BeginUpdateInternal()
        {
            if (this.IsHandleCreated)
            {
                if (this.updateCount == 0)
                {
                    this.SendMessage(11, 0, 0);
                }
                this.updateCount = (short) (this.updateCount + 1);
            }
        }

        public void BringToFront()
        {
            if (this.parent != null)
            {
                this.parent.Controls.SetChildIndex(this, 0);
            }
            else if ((this.IsHandleCreated && this.GetTopLevel()) && System.Windows.Forms.SafeNativeMethods.IsWindowEnabled(new HandleRef(this.window, this.Handle)))
            {
                System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this.window, this.Handle), System.Windows.Forms.NativeMethods.HWND_TOP, 0, 0, 0, 0, 3);
            }
        }

        internal virtual bool CanProcessMnemonic()
        {
            if (!this.Enabled || !this.Visible)
            {
                return false;
            }
            if (this.parent != null)
            {
                return this.parent.CanProcessMnemonic();
            }
            return true;
        }

        internal virtual bool CanSelectCore()
        {
            if ((this.controlStyle & ControlStyles.Selectable) != ControlStyles.Selectable)
            {
                return false;
            }
            for (Control control = this; control != null; control = control.parent)
            {
                if (!control.Enabled || !control.Visible)
                {
                    return false;
                }
            }
            return true;
        }

        internal static void CheckParentingCycle(Control bottom, Control toFind)
        {
            Form form = null;
            Control control = null;
            for (Control control2 = bottom; control2 != null; control2 = control2.ParentInternal)
            {
                control = control2;
                if (control2 == toFind)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("CircularOwner"));
                }
            }
            if ((control != null) && (control is Form))
            {
                Form form2 = (Form) control;
                for (Form form3 = form2; form3 != null; form3 = form3.OwnerInternal)
                {
                    form = form3;
                    if (form3 == toFind)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("CircularOwner"));
                    }
                }
            }
            if ((form != null) && (form.ParentInternal != null))
            {
                CheckParentingCycle(form.ParentInternal, toFind);
            }
        }

        private void ChildGotFocus(Control child)
        {
            if (this.IsActiveX)
            {
                this.ActiveXOnFocus(true);
            }
            if (this.parent != null)
            {
                this.parent.ChildGotFocus(child);
            }
        }

        public bool Contains(Control ctl)
        {
            while (ctl != null)
            {
                ctl = ctl.ParentInternal;
                if (ctl == null)
                {
                    return false;
                }
                if (ctl == this)
                {
                    return true;
                }
            }
            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual AccessibleObject CreateAccessibilityInstance()
        {
            return new ControlAccessibleObject(this);
        }

        public void CreateControl()
        {
            bool created = this.Created;
            this.CreateControl(false);
            if (((this.Properties.GetObject(PropBindingManager) == null) && (this.ParentInternal != null)) && !created)
            {
                this.OnBindingContextChanged(EventArgs.Empty);
            }
        }

        internal void CreateControl(bool fIgnoreVisible)
        {
            if ((((this.state & 1) == 0) && this.Visible) || fIgnoreVisible)
            {
                this.state |= 1;
                bool flag2 = false;
                try
                {
                    if (!this.IsHandleCreated)
                    {
                        this.CreateHandle();
                    }
                    ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                    if (controls != null)
                    {
                        Control[] array = new Control[controls.Count];
                        controls.CopyTo(array, 0);
                        foreach (Control control in array)
                        {
                            if (control.IsHandleCreated)
                            {
                                control.SetParentHandle(this.Handle);
                            }
                            control.CreateControl(fIgnoreVisible);
                        }
                    }
                    flag2 = true;
                }
                finally
                {
                    if (!flag2)
                    {
                        this.state &= -2;
                    }
                }
                this.OnCreateControl();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual ControlCollection CreateControlsInstance()
        {
            return new ControlCollection(this);
        }

        public Graphics CreateGraphics()
        {
            using (new MultithreadSafeCallScope())
            {
                System.Windows.Forms.IntSecurity.CreateGraphicsForControl.Demand();
                return this.CreateGraphicsInternal();
            }
        }

        internal Graphics CreateGraphicsInternal()
        {
            return Graphics.FromHwndInternal(this.Handle);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), UIPermission(SecurityAction.InheritanceDemand, Window=UIPermissionWindow.AllWindows)]
        protected virtual void CreateHandle()
        {
            IntPtr zero = IntPtr.Zero;
            if (this.GetState(0x800))
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (!this.GetState(0x40000))
            {
                Rectangle bounds;
                try
                {
                    this.SetState(0x40000, true);
                    bounds = this.Bounds;
                    if (Application.UseVisualStyles)
                    {
                        zero = System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Activate();
                    }
                    System.Windows.Forms.CreateParams createParams = this.CreateParams;
                    this.SetState(0x40000000, (createParams.ExStyle & 0x400000) != 0);
                    if (this.parent != null)
                    {
                        Rectangle clientRectangle = this.parent.ClientRectangle;
                        if (!clientRectangle.IsEmpty)
                        {
                            if (createParams.X != -2147483648)
                            {
                                createParams.X -= clientRectangle.X;
                            }
                            if (createParams.Y != -2147483648)
                            {
                                createParams.Y -= clientRectangle.Y;
                            }
                        }
                    }
                    if ((createParams.Parent == IntPtr.Zero) && ((createParams.Style & 0x40000000) != 0))
                    {
                        Application.ParkHandle(createParams);
                    }
                    this.window.CreateHandle(createParams);
                    this.UpdateReflectParent(true);
                }
                finally
                {
                    this.SetState(0x40000, false);
                    System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Deactivate(zero);
                }
                if (this.Bounds != bounds)
                {
                    LayoutTransaction.DoLayout(this.ParentInternal, this, PropertyNames.Bounds);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected virtual void DefWndProc(ref Message m)
        {
            this.window.DefWndProc(ref m);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode), UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected virtual void DestroyHandle()
        {
            if (this.RecreatingHandle && (this.threadCallbackList != null))
            {
                lock (this.threadCallbackList)
                {
                    if (threadCallbackMessage != 0)
                    {
                        System.Windows.Forms.NativeMethods.MSG msg = new System.Windows.Forms.NativeMethods.MSG();
                        if (System.Windows.Forms.UnsafeNativeMethods.PeekMessage(ref msg, new HandleRef(this, this.Handle), threadCallbackMessage, threadCallbackMessage, 0))
                        {
                            this.SetState(0x8000, true);
                        }
                    }
                }
            }
            if (!this.RecreatingHandle && (this.threadCallbackList != null))
            {
                lock (this.threadCallbackList)
                {
                    Exception exception = new ObjectDisposedException(base.GetType().Name);
                    while (this.threadCallbackList.Count > 0)
                    {
                        ThreadMethodEntry entry = (ThreadMethodEntry) this.threadCallbackList.Dequeue();
                        entry.exception = exception;
                        entry.Complete();
                    }
                }
            }
            if ((0x40 & ((int) ((long) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this.window, this.InternalHandle), -20)))) != 0)
            {
                System.Windows.Forms.UnsafeNativeMethods.DefMDIChildProc(this.InternalHandle, 0x10, IntPtr.Zero, IntPtr.Zero);
            }
            else
            {
                this.window.DestroyHandle();
            }
            this.trackMouseEvent = null;
        }

        private void DetachContextMenu(object sender, EventArgs e)
        {
            this.ContextMenu = null;
        }

        private void DetachContextMenuStrip(object sender, EventArgs e)
        {
            this.ContextMenuStrip = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (this.GetState(0x200000))
            {
                object obj2 = this.Properties.GetObject(PropBackBrush);
                if (obj2 != null)
                {
                    IntPtr handle = (IntPtr) obj2;
                    if (handle != IntPtr.Zero)
                    {
                        System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(this, handle));
                    }
                    this.Properties.SetObject(PropBackBrush, null);
                }
            }
            this.UpdateReflectParent(false);
            if (disposing)
            {
                if (!this.GetState(0x1000))
                {
                    if (this.GetState(0x40000))
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ClosingWhileCreatingHandle", new object[] { "Dispose" }));
                    }
                    this.SetState(0x1000, true);
                    this.SuspendLayout();
                    try
                    {
                        this.DisposeAxControls();
                        System.Windows.Forms.ContextMenu menu = (System.Windows.Forms.ContextMenu) this.Properties.GetObject(PropContextMenu);
                        if (menu != null)
                        {
                            menu.Disposed -= new EventHandler(this.DetachContextMenu);
                        }
                        this.ResetBindings();
                        if (this.IsHandleCreated)
                        {
                            this.DestroyHandle();
                        }
                        if (this.parent != null)
                        {
                            this.parent.Controls.Remove(this);
                        }
                        ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                        if (controls != null)
                        {
                            for (int i = 0; i < controls.Count; i++)
                            {
                                Control control = controls[i];
                                control.parent = null;
                                control.Dispose();
                            }
                            this.Properties.SetObject(PropControlsCollection, null);
                        }
                        base.Dispose(disposing);
                    }
                    finally
                    {
                        this.ResumeLayout(false);
                        this.SetState(0x1000, false);
                        this.SetState(0x800, true);
                    }
                }
            }
            else
            {
                if (this.window != null)
                {
                    this.window.ForceExitMessageLoop();
                }
                base.Dispose(disposing);
            }
        }

        internal virtual void DisposeAxControls()
        {
            ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
            if (controls != null)
            {
                for (int i = 0; i < controls.Count; i++)
                {
                    controls[i].DisposeAxControls();
                }
            }
        }

        private void DisposeFontHandle()
        {
            if (this.Properties.ContainsObject(PropFontHandleWrapper))
            {
                FontHandleWrapper wrapper = this.Properties.GetObject(PropFontHandleWrapper) as FontHandleWrapper;
                if (wrapper != null)
                {
                    wrapper.Dispose();
                }
                this.Properties.SetObject(PropFontHandleWrapper, null);
            }
        }

        [UIPermission(SecurityAction.Demand, Clipboard=UIPermissionClipboard.OwnClipboard)]
        public DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects)
        {
            int[] finalEffect = new int[1];
            System.Windows.Forms.UnsafeNativeMethods.IOleDropSource dropSource = new DropSource(this);
            System.Runtime.InteropServices.ComTypes.IDataObject dataObject = null;
            if (data is System.Runtime.InteropServices.ComTypes.IDataObject)
            {
                dataObject = (System.Runtime.InteropServices.ComTypes.IDataObject) data;
            }
            else
            {
                DataObject obj3 = null;
                if (data is System.Windows.Forms.IDataObject)
                {
                    obj3 = new DataObject((System.Windows.Forms.IDataObject) data);
                }
                else
                {
                    obj3 = new DataObject();
                    obj3.SetData(data);
                }
                dataObject = obj3;
            }
            try
            {
                System.Windows.Forms.SafeNativeMethods.DoDragDrop(dataObject, dropSource, (int) allowedEffects, finalEffect);
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
            return (DragDropEffects) finalEffect[0];
        }

        [UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.AllWindows)]
        public void DrawToBitmap(Bitmap bitmap, Rectangle targetBounds)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException("bitmap");
            }
            if (((targetBounds.Width <= 0) || (targetBounds.Height <= 0)) || ((targetBounds.X < 0) || (targetBounds.Y < 0)))
            {
                throw new ArgumentException("targetBounds");
            }
            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
            }
            int width = Math.Min(this.Width, targetBounds.Width);
            int height = Math.Min(this.Height, targetBounds.Height);
            Bitmap image = new Bitmap(width, height, bitmap.PixelFormat);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                IntPtr hdc = graphics.GetHdc();
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x317, hdc, (IntPtr) 30);
                using (Graphics graphics2 = Graphics.FromImage(bitmap))
                {
                    IntPtr handle = graphics2.GetHdc();
                    System.Windows.Forms.SafeNativeMethods.BitBlt(new HandleRef(graphics2, handle), targetBounds.X, targetBounds.Y, width, height, new HandleRef(graphics, hdc), 0, 0, 0xcc0020);
                    graphics2.ReleaseHdcInternal(handle);
                }
                graphics.ReleaseHdcInternal(hdc);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public object EndInvoke(IAsyncResult asyncResult)
        {
            using (new MultithreadSafeCallScope())
            {
                if (asyncResult == null)
                {
                    throw new ArgumentNullException("asyncResult");
                }
                ThreadMethodEntry entry = asyncResult as ThreadMethodEntry;
                if (entry == null)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ControlBadAsyncResult"), "asyncResult");
                }
                if (!asyncResult.IsCompleted)
                {
                    int num;
                    Control wrapper = this.FindMarshalingControl();
                    if (System.Windows.Forms.SafeNativeMethods.GetWindowThreadProcessId(new HandleRef(wrapper, wrapper.Handle), out num) == System.Windows.Forms.SafeNativeMethods.GetCurrentThreadId())
                    {
                        wrapper.InvokeMarshaledCallbacks();
                    }
                    else
                    {
                        entry.marshaler.WaitForWaitHandle(asyncResult.AsyncWaitHandle);
                    }
                }
                if (entry.exception != null)
                {
                    throw entry.exception;
                }
                return entry.retVal;
            }
        }

        internal bool EndUpdateInternal()
        {
            return this.EndUpdateInternal(true);
        }

        internal bool EndUpdateInternal(bool invalidate)
        {
            if (this.updateCount <= 0)
            {
                return false;
            }
            this.updateCount = (short) (this.updateCount - 1);
            if (this.updateCount == 0)
            {
                this.SendMessage(11, -1, 0);
                if (invalidate)
                {
                    this.Invalidate();
                }
            }
            return true;
        }

        [UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.AllWindows)]
        public Form FindForm()
        {
            return this.FindFormInternal();
        }

        internal Form FindFormInternal()
        {
            Control parentInternal = this;
            while ((parentInternal != null) && !(parentInternal is Form))
            {
                parentInternal = parentInternal.ParentInternal;
            }
            return (Form) parentInternal;
        }

        private Control FindMarshalingControl()
        {
            lock (this)
            {
                Control parentInternal = this;
                while ((parentInternal != null) && !parentInternal.IsHandleCreated)
                {
                    parentInternal = parentInternal.ParentInternal;
                }
                if (parentInternal == null)
                {
                    parentInternal = this;
                }
                return parentInternal;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool Focus()
        {
            System.Windows.Forms.IntSecurity.ModifyFocus.Demand();
            return this.FocusInternal();
        }

        internal virtual bool FocusInternal()
        {
            if (this.CanFocus)
            {
                System.Windows.Forms.UnsafeNativeMethods.SetFocus(new HandleRef(this, this.Handle));
            }
            if (this.Focused && (this.ParentInternal != null))
            {
                IContainerControl containerControlInternal = this.ParentInternal.GetContainerControlInternal();
                if (containerControlInternal != null)
                {
                    if (containerControlInternal is ContainerControl)
                    {
                        ((ContainerControl) containerControlInternal).SetActiveControlInternal(this);
                    }
                    else
                    {
                        containerControlInternal.ActiveControl = this;
                    }
                }
            }
            return this.Focused;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Control FromChildHandle(IntPtr handle)
        {
            System.Windows.Forms.IntSecurity.ControlFromHandleOrLocation.Demand();
            return FromChildHandleInternal(handle);
        }

        internal static Control FromChildHandleInternal(IntPtr handle)
        {
            while (handle != IntPtr.Zero)
            {
                Control control = FromHandleInternal(handle);
                if (control != null)
                {
                    return control;
                }
                handle = System.Windows.Forms.UnsafeNativeMethods.GetAncestor(new HandleRef(null, handle), 1);
            }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Control FromHandle(IntPtr handle)
        {
            System.Windows.Forms.IntSecurity.ControlFromHandleOrLocation.Demand();
            return FromHandleInternal(handle);
        }

        internal static Control FromHandleInternal(IntPtr handle)
        {
            NativeWindow previousWindow = NativeWindow.FromHandle(handle);
            while ((previousWindow != null) && !(previousWindow is ControlNativeWindow))
            {
                previousWindow = previousWindow.PreviousWindow;
            }
            if (previousWindow is ControlNativeWindow)
            {
                return ((ControlNativeWindow) previousWindow).GetControl();
            }
            return null;
        }

        private AccessibleObject GetAccessibilityObject(int accObjId)
        {
            switch (accObjId)
            {
                case -4:
                    return this.AccessibilityObject;

                case 0:
                    return this.NcAccessibilityObject;
            }
            if (accObjId > 0)
            {
                return this.GetAccessibilityObjectById(accObjId);
            }
            return null;
        }

        protected virtual AccessibleObject GetAccessibilityObjectById(int objectId)
        {
            return null;
        }

        internal bool GetAnyDisposingInHierarchy()
        {
            Control parent = this;
            while (parent != null)
            {
                if (parent.Disposing)
                {
                    return true;
                }
                parent = parent.parent;
            }
            return false;
        }

        protected AutoSizeMode GetAutoSizeMode()
        {
            return CommonProperties.GetAutoSizeMode(this);
        }

        internal static AutoValidate GetAutoValidateForControl(Control control)
        {
            ContainerControl parentContainerControl = control.ParentContainerControl;
            if (parentContainerControl == null)
            {
                return AutoValidate.EnablePreventFocusChange;
            }
            return parentContainerControl.AutoValidate;
        }

        public Control GetChildAtPoint(Point pt)
        {
            return this.GetChildAtPoint(pt, GetChildAtPointSkip.None);
        }

        public Control GetChildAtPoint(Point pt, GetChildAtPointSkip skipValue)
        {
            int invalidValue = (int) skipValue;
            if ((invalidValue < 0) || (invalidValue > 7))
            {
                throw new InvalidEnumArgumentException("skipValue", invalidValue, typeof(GetChildAtPointSkip));
            }
            Control descendant = FromChildHandleInternal(System.Windows.Forms.UnsafeNativeMethods.ChildWindowFromPointEx(new HandleRef(null, this.Handle), pt.X, pt.Y, invalidValue));
            if ((descendant != null) && !this.IsDescendant(descendant))
            {
                System.Windows.Forms.IntSecurity.ControlFromHandleOrLocation.Demand();
            }
            if (descendant != this)
            {
                return descendant;
            }
            return null;
        }

        internal Control[] GetChildControlsInTabOrder(bool handleCreatedOnly)
        {
            ArrayList childControlsTabOrderList = this.GetChildControlsTabOrderList(handleCreatedOnly);
            Control[] controlArray = new Control[childControlsTabOrderList.Count];
            for (int i = 0; i < childControlsTabOrderList.Count; i++)
            {
                controlArray[i] = ((ControlTabOrderHolder) childControlsTabOrderList[i]).control;
            }
            return controlArray;
        }

        private ArrayList GetChildControlsTabOrderList(bool handleCreatedOnly)
        {
            ArrayList list = new ArrayList();
            foreach (Control control in this.Controls)
            {
                if (!handleCreatedOnly || control.IsHandleCreated)
                {
                    list.Add(new ControlTabOrderHolder(list.Count, control.TabIndex, control));
                }
            }
            list.Sort(new ControlTabOrderComparer());
            return list;
        }

        private static ArrayList GetChildWindows(IntPtr hWndParent)
        {
            ArrayList list = new ArrayList();
            for (IntPtr ptr = System.Windows.Forms.UnsafeNativeMethods.GetWindow(new HandleRef(null, hWndParent), 5); ptr != IntPtr.Zero; ptr = System.Windows.Forms.UnsafeNativeMethods.GetWindow(new HandleRef(null, ptr), 2))
            {
                list.Add(ptr);
            }
            return list;
        }

        private int[] GetChildWindowsInTabOrder()
        {
            ArrayList childWindowsTabOrderList = this.GetChildWindowsTabOrderList();
            int[] numArray = new int[childWindowsTabOrderList.Count];
            for (int i = 0; i < childWindowsTabOrderList.Count; i++)
            {
                numArray[i] = ((ControlTabOrderHolder) childWindowsTabOrderList[i]).oldOrder;
            }
            return numArray;
        }

        private ArrayList GetChildWindowsTabOrderList()
        {
            ArrayList list = new ArrayList();
            foreach (IntPtr ptr in GetChildWindows(this.Handle))
            {
                Control control = FromHandleInternal(ptr);
                int newOrder = (control == null) ? -1 : control.TabIndex;
                list.Add(new ControlTabOrderHolder(list.Count, newOrder, control));
            }
            list.Sort(new ControlTabOrderComparer());
            return list;
        }

        public IContainerControl GetContainerControl()
        {
            System.Windows.Forms.IntSecurity.GetParent.Demand();
            return this.GetContainerControlInternal();
        }

        internal IContainerControl GetContainerControlInternal()
        {
            Control ctl = this;
            if ((ctl != null) && this.IsContainerControl)
            {
                ctl = ctl.ParentInternal;
            }
            while ((ctl != null) && !IsFocusManagingContainerControl(ctl))
            {
                ctl = ctl.ParentInternal;
            }
            return (IContainerControl) ctl;
        }

        private static FontHandleWrapper GetDefaultFontHandleWrapper()
        {
            if (defaultFontHandleWrapper == null)
            {
                defaultFontHandleWrapper = new FontHandleWrapper(DefaultFont);
            }
            return defaultFontHandleWrapper;
        }

        internal virtual Control GetFirstChildControlInTabOrder(bool forward)
        {
            ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
            Control control = null;
            if (controls != null)
            {
                if (forward)
                {
                    for (int j = 0; j < controls.Count; j++)
                    {
                        if ((control == null) || (control.tabIndex > controls[j].tabIndex))
                        {
                            control = controls[j];
                        }
                    }
                    return control;
                }
                for (int i = controls.Count - 1; i >= 0; i--)
                {
                    if ((control == null) || (control.tabIndex < controls[i].tabIndex))
                    {
                        control = controls[i];
                    }
                }
            }
            return control;
        }

        internal IntPtr GetHRgn(System.Drawing.Region region)
        {
            Graphics g = this.CreateGraphicsInternal();
            IntPtr hrgn = region.GetHrgn(g);
            System.Internal.HandleCollector.Add(hrgn, System.Windows.Forms.NativeMethods.CommonHandles.GDI);
            g.Dispose();
            return hrgn;
        }

        private MenuItem GetMenuItemFromHandleId(IntPtr hmenu, int item)
        {
            MenuItem baseItem = null;
            int menuItemID = System.Windows.Forms.UnsafeNativeMethods.GetMenuItemID(new HandleRef(null, hmenu), item);
            if (menuItemID == -1)
            {
                IntPtr zero = IntPtr.Zero;
                zero = System.Windows.Forms.UnsafeNativeMethods.GetSubMenu(new HandleRef(null, hmenu), item);
                int menuItemCount = System.Windows.Forms.UnsafeNativeMethods.GetMenuItemCount(new HandleRef(null, zero));
                MenuItem menuItemFromHandleId = null;
                for (int i = 0; i < menuItemCount; i++)
                {
                    menuItemFromHandleId = this.GetMenuItemFromHandleId(zero, i);
                    if (menuItemFromHandleId != null)
                    {
                        Menu parent = menuItemFromHandleId.Parent;
                        if ((parent != null) && (parent is MenuItem))
                        {
                            return (MenuItem) parent;
                        }
                        menuItemFromHandleId = null;
                    }
                }
                return menuItemFromHandleId;
            }
            Command commandFromID = Command.GetCommandFromID(menuItemID);
            if (commandFromID != null)
            {
                object target = commandFromID.Target;
                if ((target != null) && (target is MenuItem.MenuItemData))
                {
                    baseItem = ((MenuItem.MenuItemData) target).baseItem;
                }
            }
            return baseItem;
        }

        public Control GetNextControl(Control ctl, bool forward)
        {
            if (!this.Contains(ctl))
            {
                ctl = this;
            }
            if (!forward)
            {
                if (ctl != this)
                {
                    int tabIndex = ctl.tabIndex;
                    bool flag2 = false;
                    Control control4 = null;
                    Control parent = ctl.parent;
                    int count = 0;
                    ControlCollection controls3 = (ControlCollection) parent.Properties.GetObject(PropControlsCollection);
                    if (controls3 != null)
                    {
                        count = controls3.Count;
                    }
                    for (int i = count - 1; i >= 0; i--)
                    {
                        if (controls3[i] != ctl)
                        {
                            if (((controls3[i].tabIndex <= tabIndex) && ((control4 == null) || (control4.tabIndex < controls3[i].tabIndex))) && ((controls3[i].tabIndex != tabIndex) || flag2))
                            {
                                control4 = controls3[i];
                            }
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    if (control4 == null)
                    {
                        if (parent == this)
                        {
                            return null;
                        }
                        return parent;
                    }
                    ctl = control4;
                }
                for (ControlCollection controls4 = (ControlCollection) ctl.Properties.GetObject(PropControlsCollection); ((controls4 != null) && (controls4.Count > 0)) && ((ctl == this) || !IsFocusManagingContainerControl(ctl)); controls4 = (ControlCollection) ctl.Properties.GetObject(PropControlsCollection))
                {
                    Control firstChildControlInTabOrder = ctl.GetFirstChildControlInTabOrder(false);
                    if (firstChildControlInTabOrder == null)
                    {
                        break;
                    }
                    ctl = firstChildControlInTabOrder;
                }
            }
            else
            {
                ControlCollection controls = (ControlCollection) ctl.Properties.GetObject(PropControlsCollection);
                if (((controls != null) && (controls.Count > 0)) && ((ctl == this) || !IsFocusManagingContainerControl(ctl)))
                {
                    Control control = ctl.GetFirstChildControlInTabOrder(true);
                    if (control != null)
                    {
                        return control;
                    }
                }
                while (ctl != this)
                {
                    int num = ctl.tabIndex;
                    bool flag = false;
                    Control control2 = null;
                    Control control3 = ctl.parent;
                    int num2 = 0;
                    ControlCollection controls2 = (ControlCollection) control3.Properties.GetObject(PropControlsCollection);
                    if (controls2 != null)
                    {
                        num2 = controls2.Count;
                    }
                    for (int j = 0; j < num2; j++)
                    {
                        if (controls2[j] != ctl)
                        {
                            if (((controls2[j].tabIndex >= num) && ((control2 == null) || (control2.tabIndex > controls2[j].tabIndex))) && ((controls2[j].tabIndex != num) || flag))
                            {
                                control2 = controls2[j];
                            }
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    if (control2 != null)
                    {
                        return control2;
                    }
                    ctl = ctl.parent;
                }
            }
            if (ctl != this)
            {
                return ctl;
            }
            return null;
        }

        private System.Drawing.Font GetParentFont()
        {
            if ((this.ParentInternal != null) && this.ParentInternal.CanAccessProperties)
            {
                return this.ParentInternal.Font;
            }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual System.Drawing.Size GetPreferredSize(System.Drawing.Size proposedSize)
        {
            System.Drawing.Size preferredSizeCore;
            if (this.GetState(0x1800))
            {
                return CommonProperties.xGetPreferredSizeCache(this);
            }
            proposedSize = LayoutUtils.ConvertZeroToUnbounded(proposedSize);
            proposedSize = this.ApplySizeConstraints(proposedSize);
            if (this.GetState2(0x800))
            {
                System.Drawing.Size size2 = CommonProperties.xGetPreferredSizeCache(this);
                if (!size2.IsEmpty && (proposedSize == LayoutUtils.MaxSize))
                {
                    return size2;
                }
            }
            this.CacheTextInternal = true;
            try
            {
                preferredSizeCore = this.GetPreferredSizeCore(proposedSize);
            }
            finally
            {
                this.CacheTextInternal = false;
            }
            preferredSizeCore = this.ApplySizeConstraints(preferredSizeCore);
            if (this.GetState2(0x800) && (proposedSize == LayoutUtils.MaxSize))
            {
                CommonProperties.xSetPreferredSizeCache(this, preferredSizeCore);
            }
            return preferredSizeCore;
        }

        internal virtual System.Drawing.Size GetPreferredSizeCore(System.Drawing.Size proposedSize)
        {
            return CommonProperties.GetSpecifiedBounds(this).Size;
        }

        internal static IntPtr GetSafeHandle(IWin32Window window)
        {
            IntPtr zero = IntPtr.Zero;
            Control control = window as Control;
            if (control != null)
            {
                return control.Handle;
            }
            System.Windows.Forms.IntSecurity.AllWindows.Demand();
            zero = window.Handle;
            if (!(zero == IntPtr.Zero) && !System.Windows.Forms.UnsafeNativeMethods.IsWindow(new HandleRef(null, zero)))
            {
                throw new Win32Exception(6);
            }
            return zero;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, BoundsSpecified specified)
        {
            System.Windows.Forms.NativeMethods.RECT lpRect = new System.Windows.Forms.NativeMethods.RECT(0, 0, 0, 0);
            System.Windows.Forms.CreateParams createParams = this.CreateParams;
            System.Windows.Forms.SafeNativeMethods.AdjustWindowRectEx(ref lpRect, createParams.Style, this.HasMenu, createParams.ExStyle);
            float width = factor.Width;
            float height = factor.Height;
            int x = bounds.X;
            int y = bounds.Y;
            bool flag = !this.GetState(0x80000);
            if (flag)
            {
                ISite site = this.Site;
                if ((site != null) && site.DesignMode)
                {
                    IDesignerHost service = site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if ((service != null) && (service.RootComponent == this))
                    {
                        flag = false;
                    }
                }
            }
            if (flag)
            {
                if ((specified & BoundsSpecified.X) != BoundsSpecified.None)
                {
                    x = (int) Math.Round((double) (bounds.X * width));
                }
                if ((specified & BoundsSpecified.Y) != BoundsSpecified.None)
                {
                    y = (int) Math.Round((double) (bounds.Y * height));
                }
            }
            int num5 = bounds.Width;
            int num6 = bounds.Height;
            if (((this.controlStyle & ControlStyles.FixedWidth) != ControlStyles.FixedWidth) && ((specified & BoundsSpecified.Width) != BoundsSpecified.None))
            {
                int num7 = lpRect.right - lpRect.left;
                int num8 = bounds.Width - num7;
                num5 = ((int) Math.Round((double) (num8 * width))) + num7;
            }
            if (((this.controlStyle & ControlStyles.FixedHeight) != ControlStyles.FixedHeight) && ((specified & BoundsSpecified.Height) != BoundsSpecified.None))
            {
                int num9 = lpRect.bottom - lpRect.top;
                int num10 = bounds.Height - num9;
                num6 = ((int) Math.Round((double) (num10 * height))) + num9;
            }
            return new Rectangle(x, y, num5, num6);
        }

        internal bool GetState(int flag)
        {
            return ((this.state & flag) != 0);
        }

        private bool GetState2(int flag)
        {
            return ((this.state2 & flag) != 0);
        }

        protected bool GetStyle(ControlStyles flag)
        {
            return ((this.controlStyle & flag) == flag);
        }

        protected bool GetTopLevel()
        {
            return ((this.state & 0x80000) != 0);
        }

        internal virtual bool GetVisibleCore()
        {
            if (!this.GetState(2))
            {
                return false;
            }
            return ((this.ParentInternal == null) || this.ParentInternal.GetVisibleCore());
        }

        private System.Windows.Forms.MouseButtons GetXButton(int wparam)
        {
            switch (wparam)
            {
                case 1:
                    return System.Windows.Forms.MouseButtons.XButton1;

                case 2:
                    return System.Windows.Forms.MouseButtons.XButton2;
            }
            return System.Windows.Forms.MouseButtons.None;
        }

        public void Hide()
        {
            this.Visible = false;
        }

        private void HookMouseEvent()
        {
            if (!this.GetState(0x4000))
            {
                this.SetState(0x4000, true);
                if (this.trackMouseEvent == null)
                {
                    this.trackMouseEvent = new System.Windows.Forms.NativeMethods.TRACKMOUSEEVENT();
                    this.trackMouseEvent.dwFlags = 3;
                    this.trackMouseEvent.hwndTrack = this.Handle;
                }
                System.Windows.Forms.SafeNativeMethods.TrackMouseEvent(this.trackMouseEvent);
            }
        }

        internal virtual IntPtr InitializeDCForWmCtlColor(IntPtr dc, int msg)
        {
            if (!this.GetStyle(ControlStyles.UserPaint))
            {
                System.Windows.Forms.SafeNativeMethods.SetTextColor(new HandleRef(null, dc), ColorTranslator.ToWin32(this.ForeColor));
                System.Windows.Forms.SafeNativeMethods.SetBkColor(new HandleRef(null, dc), ColorTranslator.ToWin32(this.BackColor));
                return this.BackColorBrush;
            }
            return System.Windows.Forms.UnsafeNativeMethods.GetStockObject(5);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void InitLayout()
        {
            this.LayoutEngine.InitLayout(this, BoundsSpecified.All);
        }

        private void InitMouseWheelSupport()
        {
            if (!mouseWheelInit)
            {
                mouseWheelRoutingNeeded = !SystemInformation.NativeMouseWheelSupport;
                if (mouseWheelRoutingNeeded)
                {
                    if (System.Windows.Forms.UnsafeNativeMethods.FindWindow("MouseZ", "Magellan MSWHEEL") != IntPtr.Zero)
                    {
                        int num = System.Windows.Forms.SafeNativeMethods.RegisterWindowMessage("MSWHEEL_ROLLMSG");
                        if (num != 0)
                        {
                            mouseWheelMessage = num;
                        }
                    }
                }
                mouseWheelInit = true;
            }
        }

        private void InitScaling(BoundsSpecified specified)
        {
            this.requiredScaling = (byte) (this.requiredScaling | ((byte) (specified & BoundsSpecified.All)));
        }

        public void Invalidate()
        {
            this.Invalidate(false);
        }

        public void Invalidate(bool invalidateChildren)
        {
            if (this.IsHandleCreated)
            {
                if (invalidateChildren)
                {
                    System.Windows.Forms.SafeNativeMethods.RedrawWindow(new HandleRef(this.window, this.Handle), (System.Windows.Forms.NativeMethods.COMRECT) null, System.Windows.Forms.NativeMethods.NullHandleRef, 0x85);
                }
                else
                {
                    using (new MultithreadSafeCallScope())
                    {
                        System.Windows.Forms.SafeNativeMethods.InvalidateRect(new HandleRef(this.window, this.Handle), (System.Windows.Forms.NativeMethods.COMRECT) null, (this.controlStyle & ControlStyles.Opaque) != ControlStyles.Opaque);
                    }
                }
                this.NotifyInvalidate(this.ClientRectangle);
            }
        }

        public void Invalidate(Rectangle rc)
        {
            this.Invalidate(rc, false);
        }

        public void Invalidate(System.Drawing.Region region)
        {
            this.Invalidate(region, false);
        }

        public void Invalidate(Rectangle rc, bool invalidateChildren)
        {
            if (rc.IsEmpty)
            {
                this.Invalidate(invalidateChildren);
            }
            else if (this.IsHandleCreated)
            {
                if (invalidateChildren)
                {
                    System.Windows.Forms.NativeMethods.RECT rcUpdate = System.Windows.Forms.NativeMethods.RECT.FromXYWH(rc.X, rc.Y, rc.Width, rc.Height);
                    System.Windows.Forms.SafeNativeMethods.RedrawWindow(new HandleRef(this.window, this.Handle), ref rcUpdate, System.Windows.Forms.NativeMethods.NullHandleRef, 0x85);
                }
                else
                {
                    System.Windows.Forms.NativeMethods.RECT rect = System.Windows.Forms.NativeMethods.RECT.FromXYWH(rc.X, rc.Y, rc.Width, rc.Height);
                    using (new MultithreadSafeCallScope())
                    {
                        System.Windows.Forms.SafeNativeMethods.InvalidateRect(new HandleRef(this.window, this.Handle), ref rect, (this.controlStyle & ControlStyles.Opaque) != ControlStyles.Opaque);
                    }
                }
                this.NotifyInvalidate(rc);
            }
        }

        public void Invalidate(System.Drawing.Region region, bool invalidateChildren)
        {
            if (region == null)
            {
                this.Invalidate(invalidateChildren);
            }
            else if (this.IsHandleCreated)
            {
                IntPtr hRgn = this.GetHRgn(region);
                try
                {
                    if (invalidateChildren)
                    {
                        System.Windows.Forms.SafeNativeMethods.RedrawWindow(new HandleRef(this, this.Handle), (System.Windows.Forms.NativeMethods.COMRECT) null, new HandleRef(region, hRgn), 0x85);
                    }
                    else
                    {
                        using (new MultithreadSafeCallScope())
                        {
                            System.Windows.Forms.SafeNativeMethods.InvalidateRgn(new HandleRef(this, this.Handle), new HandleRef(region, hRgn), !this.GetStyle(ControlStyles.Opaque));
                        }
                    }
                }
                finally
                {
                    System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(region, hRgn));
                }
                Rectangle empty = Rectangle.Empty;
                using (Graphics graphics = this.CreateGraphicsInternal())
                {
                    empty = Rectangle.Ceiling(region.GetBounds(graphics));
                }
                this.OnInvalidated(new InvalidateEventArgs(empty));
            }
        }

        public object Invoke(Delegate method)
        {
            return this.Invoke(method, null);
        }

        public object Invoke(Delegate method, params object[] args)
        {
            using (new MultithreadSafeCallScope())
            {
                return this.FindMarshalingControl().MarshaledInvoke(this, method, args, true);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void InvokeGotFocus(Control toInvoke, EventArgs e)
        {
            if (toInvoke != null)
            {
                toInvoke.OnGotFocus(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void InvokeLostFocus(Control toInvoke, EventArgs e)
        {
            if (toInvoke != null)
            {
                toInvoke.OnLostFocus(e);
            }
        }

        private void InvokeMarshaledCallback(ThreadMethodEntry tme)
        {
            if (tme.executionContext != null)
            {
                if (invokeMarshaledCallbackHelperDelegate == null)
                {
                    invokeMarshaledCallbackHelperDelegate = new ContextCallback(Control.InvokeMarshaledCallbackHelper);
                }
                if (SynchronizationContext.Current == null)
                {
                    WindowsFormsSynchronizationContext.InstallIfNeeded();
                }
                tme.syncContext = SynchronizationContext.Current;
                ExecutionContext.Run(tme.executionContext, invokeMarshaledCallbackHelperDelegate, tme);
            }
            else
            {
                InvokeMarshaledCallbackHelper(tme);
            }
        }

        private static void InvokeMarshaledCallbackDo(ThreadMethodEntry tme)
        {
            if (tme.method is EventHandler)
            {
                if ((tme.args == null) || (tme.args.Length < 1))
                {
                    ((EventHandler) tme.method)(tme.caller, EventArgs.Empty);
                }
                else if (tme.args.Length < 2)
                {
                    ((EventHandler) tme.method)(tme.args[0], EventArgs.Empty);
                }
                else
                {
                    ((EventHandler) tme.method)(tme.args[0], (EventArgs) tme.args[1]);
                }
            }
            else if (tme.method is MethodInvoker)
            {
                ((MethodInvoker) tme.method)();
            }
            else if (tme.method is WaitCallback)
            {
                ((WaitCallback) tme.method)(tme.args[0]);
            }
            else
            {
                tme.retVal = tme.method.DynamicInvoke(tme.args);
            }
        }

        private static void InvokeMarshaledCallbackHelper(object obj)
        {
            ThreadMethodEntry tme = (ThreadMethodEntry) obj;
            if (tme.syncContext != null)
            {
                SynchronizationContext current = SynchronizationContext.Current;
                try
                {
                    SynchronizationContext.SetSynchronizationContext(tme.syncContext);
                    InvokeMarshaledCallbackDo(tme);
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(current);
                }
            }
            else
            {
                InvokeMarshaledCallbackDo(tme);
            }
        }

        private void InvokeMarshaledCallbacks()
        {
            ThreadMethodEntry tme = null;
            lock (this.threadCallbackList)
            {
                if (this.threadCallbackList.Count > 0)
                {
                    tme = (ThreadMethodEntry) this.threadCallbackList.Dequeue();
                }
                goto Label_00E8;
            }
        Label_0043:
            if (tme.method != null)
            {
                try
                {
                    if (NativeWindow.WndProcShouldBeDebuggable && !tme.synchronous)
                    {
                        this.InvokeMarshaledCallback(tme);
                    }
                    else
                    {
                        try
                        {
                            this.InvokeMarshaledCallback(tme);
                        }
                        catch (Exception exception)
                        {
                            tme.exception = exception.GetBaseException();
                        }
                    }
                }
                finally
                {
                    tme.Complete();
                    if ((!NativeWindow.WndProcShouldBeDebuggable && (tme.exception != null)) && !tme.synchronous)
                    {
                        Application.OnThreadException(tme.exception);
                    }
                }
            }
            lock (this.threadCallbackList)
            {
                if (this.threadCallbackList.Count > 0)
                {
                    tme = (ThreadMethodEntry) this.threadCallbackList.Dequeue();
                }
                else
                {
                    tme = null;
                }
            }
        Label_00E8:
            if (tme != null)
            {
                goto Label_0043;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void InvokeOnClick(Control toInvoke, EventArgs e)
        {
            if (toInvoke != null)
            {
                toInvoke.OnClick(e);
            }
        }

        protected void InvokePaint(Control c, PaintEventArgs e)
        {
            c.OnPaint(e);
        }

        protected void InvokePaintBackground(Control c, PaintEventArgs e)
        {
            c.OnPaintBackground(e);
        }

        internal bool IsDescendant(Control descendant)
        {
            for (Control control = descendant; control != null; control = control.ParentInternal)
            {
                if (control == this)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsFocusManagingContainerControl(Control ctl)
        {
            return (((ctl.controlStyle & ControlStyles.ContainerControl) == ControlStyles.ContainerControl) && (ctl is IContainerControl));
        }

        internal bool IsFontSet()
        {
            return (((System.Drawing.Font) this.Properties.GetObject(PropFont)) != null);
        }

        [UIPermission(SecurityAction.InheritanceDemand, Window=UIPermissionWindow.AllWindows)]
        protected virtual bool IsInputChar(char charCode)
        {
            int num = 0;
            if (charCode == '\t')
            {
                num = 0x86;
            }
            else
            {
                num = 0x84;
            }
            return ((((int) ((long) this.SendMessage(0x87, 0, 0))) & num) != 0);
        }

        [UIPermission(SecurityAction.InheritanceDemand, Window=UIPermissionWindow.AllWindows)]
        protected virtual bool IsInputKey(Keys keyData)
        {
            if ((keyData & Keys.Alt) == Keys.Alt)
            {
                return false;
            }
            int num = 4;
            switch ((keyData & Keys.KeyCode))
            {
                case Keys.Left:
                case Keys.Up:
                case Keys.Right:
                case Keys.Down:
                    num = 5;
                    break;

                case Keys.Tab:
                    num = 6;
                    break;
            }
            return (this.IsHandleCreated && ((((int) ((long) this.SendMessage(0x87, 0, 0))) & num) != 0));
        }

        public static bool IsKeyLocked(Keys keyVal)
        {
            if (((keyVal != Keys.Insert) && (keyVal != Keys.NumLock)) && ((keyVal != Keys.Capital) && (keyVal != Keys.Scroll)))
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ControlIsKeyLockedNumCapsScrollLockKeysSupportedOnly"));
            }
            int keyState = System.Windows.Forms.UnsafeNativeMethods.GetKeyState((int) keyVal);
            if ((keyVal != Keys.Insert) && (keyVal != Keys.Capital))
            {
                return ((keyState & 0x8001) != 0);
            }
            return ((keyState & 1) != 0);
        }

        public static bool IsMnemonic(char charCode, string text)
        {
            if (charCode == '&')
            {
                return false;
            }
            if (text != null)
            {
                char ch2;
                int num = -1;
                char c = char.ToUpper(charCode, CultureInfo.CurrentCulture);
                do
                {
                    if ((num + 1) >= text.Length)
                    {
                        goto Label_006E;
                    }
                    num = text.IndexOf('&', num + 1) + 1;
                    if ((num <= 0) || (num >= text.Length))
                    {
                        goto Label_006E;
                    }
                    ch2 = char.ToUpper(text[num], CultureInfo.CurrentCulture);
                }
                while ((ch2 != c) && (char.ToLower(ch2, CultureInfo.CurrentCulture) != char.ToLower(c, CultureInfo.CurrentCulture)));
                return true;
            }
        Label_006E:
            return false;
        }

        internal bool IsUpdating()
        {
            return (this.updateCount > 0);
        }

        private bool IsValidBackColor(System.Drawing.Color c)
        {
            if ((!c.IsEmpty && !this.GetStyle(ControlStyles.SupportsTransparentBackColor)) && (c.A < 0xff))
            {
                return false;
            }
            return true;
        }

        private void ListenToUserPreferenceChanged(bool listen)
        {
            if (this.GetState2(4))
            {
                if (!listen)
                {
                    this.SetState2(4, false);
                    SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.UserPreferenceChanged);
                }
            }
            else if (listen)
            {
                this.SetState2(4, true);
                SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.UserPreferenceChanged);
            }
        }

        private object MarshaledInvoke(Control caller, Delegate method, object[] args, bool synchronous)
        {
            int num;
            if (!this.IsHandleCreated)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ErrorNoMarshalingThread"));
            }
            if (((ActiveXImpl) this.Properties.GetObject(PropActiveXImpl)) != null)
            {
                System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
            }
            bool flag = false;
            if ((System.Windows.Forms.SafeNativeMethods.GetWindowThreadProcessId(new HandleRef(this, this.Handle), out num) == System.Windows.Forms.SafeNativeMethods.GetCurrentThreadId()) && synchronous)
            {
                flag = true;
            }
            ExecutionContext executionContext = null;
            if (!flag)
            {
                executionContext = ExecutionContext.Capture();
            }
            ThreadMethodEntry entry = new ThreadMethodEntry(caller, this, method, args, synchronous, executionContext);
            lock (this)
            {
                if (this.threadCallbackList == null)
                {
                    this.threadCallbackList = new Queue();
                }
            }
            lock (this.threadCallbackList)
            {
                if (threadCallbackMessage == 0)
                {
                    threadCallbackMessage = System.Windows.Forms.SafeNativeMethods.RegisterWindowMessage(Application.WindowMessagesVersion + "_ThreadCallbackMessage");
                }
                this.threadCallbackList.Enqueue(entry);
            }
            if (flag)
            {
                this.InvokeMarshaledCallbacks();
            }
            else
            {
                System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, this.Handle), threadCallbackMessage, IntPtr.Zero, IntPtr.Zero);
            }
            if (!synchronous)
            {
                return entry;
            }
            if (!entry.IsCompleted)
            {
                this.WaitForWaitHandle(entry.AsyncWaitHandle);
            }
            if (entry.exception != null)
            {
                throw entry.exception;
            }
            return entry.retVal;
        }

        private void MarshalStringToMessage(string value, ref Message m)
        {
            if (m.LParam == IntPtr.Zero)
            {
                m.Result = (IntPtr) ((value.Length + 1) * Marshal.SystemDefaultCharSize);
            }
            else if (((int) ((long) m.WParam)) < (value.Length + 1))
            {
                m.Result = (IntPtr) (-1);
            }
            else
            {
                byte[] buffer;
                byte[] bytes;
                char[] chars = new char[1];
                if (Marshal.SystemDefaultCharSize == 1)
                {
                    bytes = Encoding.Default.GetBytes(value);
                    buffer = Encoding.Default.GetBytes(chars);
                }
                else
                {
                    bytes = Encoding.Unicode.GetBytes(value);
                    buffer = Encoding.Unicode.GetBytes(chars);
                }
                Marshal.Copy(bytes, 0, m.LParam, bytes.Length);
                Marshal.Copy(buffer, 0, (IntPtr) (((long) m.LParam) + bytes.Length), buffer.Length);
                m.Result = (IntPtr) ((bytes.Length + buffer.Length) / Marshal.SystemDefaultCharSize);
            }
        }

        internal void NotifyEnter()
        {
            this.OnEnter(EventArgs.Empty);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void NotifyInvalidate(Rectangle invalidatedArea)
        {
            this.OnInvalidated(new InvalidateEventArgs(invalidatedArea));
        }

        internal void NotifyLeave()
        {
            this.OnLeave(EventArgs.Empty);
        }

        private void NotifyValidated()
        {
            this.OnValidated(EventArgs.Empty);
        }

        private bool NotifyValidating()
        {
            CancelEventArgs e = new CancelEventArgs();
            this.OnValidating(e);
            return e.Cancel;
        }

        internal virtual void NotifyValidationResult(object sender, CancelEventArgs ev)
        {
            this.ValidationCancelled = ev.Cancel;
        }

        protected virtual void OnAutoSizeChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventAutoSizeChanged] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnBackColorChanged(EventArgs e)
        {
            if (!this.GetAnyDisposingInHierarchy())
            {
                object obj2 = this.Properties.GetObject(PropBackBrush);
                if (obj2 != null)
                {
                    if (this.GetState(0x200000))
                    {
                        IntPtr handle = (IntPtr) obj2;
                        if (handle != IntPtr.Zero)
                        {
                            System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(this, handle));
                        }
                    }
                    this.Properties.SetObject(PropBackBrush, null);
                }
                this.Invalidate();
                EventHandler handler = base.Events[EventBackColor] as EventHandler;
                if (handler != null)
                {
                    handler(this, e);
                }
                ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                if (controls != null)
                {
                    for (int i = 0; i < controls.Count; i++)
                    {
                        controls[i].OnParentBackColorChanged(e);
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnBackgroundImageChanged(EventArgs e)
        {
            if (!this.GetAnyDisposingInHierarchy())
            {
                this.Invalidate();
                EventHandler handler = base.Events[EventBackgroundImage] as EventHandler;
                if (handler != null)
                {
                    handler(this, e);
                }
                ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                if (controls != null)
                {
                    for (int i = 0; i < controls.Count; i++)
                    {
                        controls[i].OnParentBackgroundImageChanged(e);
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnBackgroundImageLayoutChanged(EventArgs e)
        {
            if (!this.GetAnyDisposingInHierarchy())
            {
                this.Invalidate();
                EventHandler handler = base.Events[EventBackgroundImageLayout] as EventHandler;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnBindingContextChanged(EventArgs e)
        {
            if (this.Properties.GetObject(PropBindings) != null)
            {
                this.UpdateBindings();
            }
            EventHandler handler = base.Events[EventBindingContext] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
            ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
            if (controls != null)
            {
                for (int i = 0; i < controls.Count; i++)
                {
                    controls[i].OnParentBindingContextChanged(e);
                }
            }
        }

        internal virtual void OnBoundsUpdate(int x, int y, int width, int height)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnCausesValidationChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventCausesValidation] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnChangeUICues(UICuesEventArgs e)
        {
            UICuesEventHandler handler = (UICuesEventHandler) base.Events[EventChangeUICues];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal virtual void OnChildLayoutResuming(Control child, bool performLayout)
        {
            if (this.ParentInternal != null)
            {
                this.ParentInternal.OnChildLayoutResuming(child, performLayout);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnClick(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventClick];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnClientSizeChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventClientSize] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnContextMenuChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventContextMenu] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnContextMenuStripChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventContextMenuStrip] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnControlAdded(ControlEventArgs e)
        {
            ControlEventHandler handler = (ControlEventHandler) base.Events[EventControlAdded];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnControlRemoved(ControlEventArgs e)
        {
            ControlEventHandler handler = (ControlEventHandler) base.Events[EventControlRemoved];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnCreateControl()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnCursorChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventCursor] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
            ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
            if (controls != null)
            {
                for (int i = 0; i < controls.Count; i++)
                {
                    controls[i].OnParentCursorChanged(e);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnDockChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventDock] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnDoubleClick(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventDoubleClick];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnDragDrop(DragEventArgs drgevent)
        {
            DragEventHandler handler = (DragEventHandler) base.Events[EventDragDrop];
            if (handler != null)
            {
                handler(this, drgevent);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnDragEnter(DragEventArgs drgevent)
        {
            DragEventHandler handler = (DragEventHandler) base.Events[EventDragEnter];
            if (handler != null)
            {
                handler(this, drgevent);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnDragLeave(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventDragLeave];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnDragOver(DragEventArgs drgevent)
        {
            DragEventHandler handler = (DragEventHandler) base.Events[EventDragOver];
            if (handler != null)
            {
                handler(this, drgevent);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnEnabledChanged(EventArgs e)
        {
            if (!this.GetAnyDisposingInHierarchy())
            {
                if (this.IsHandleCreated)
                {
                    System.Windows.Forms.SafeNativeMethods.EnableWindow(new HandleRef(this, this.Handle), this.Enabled);
                    if (this.GetStyle(ControlStyles.UserPaint))
                    {
                        this.Invalidate();
                        this.Update();
                    }
                }
                EventHandler handler = base.Events[EventEnabled] as EventHandler;
                if (handler != null)
                {
                    handler(this, e);
                }
                ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                if (controls != null)
                {
                    for (int i = 0; i < controls.Count; i++)
                    {
                        controls[i].OnParentEnabledChanged(e);
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnEnter(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventEnter];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnFontChanged(EventArgs e)
        {
            if (!this.GetAnyDisposingInHierarchy())
            {
                this.Invalidate();
                if (this.Properties.ContainsInteger(PropFontHeight))
                {
                    this.Properties.SetInteger(PropFontHeight, -1);
                }
                this.DisposeFontHandle();
                if (this.IsHandleCreated && !this.GetStyle(ControlStyles.UserPaint))
                {
                    this.SetWindowFont();
                }
                EventHandler handler = base.Events[EventFont] as EventHandler;
                if (handler != null)
                {
                    handler(this, e);
                }
                ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                using (new LayoutTransaction(this, this, PropertyNames.Font, false))
                {
                    if (controls != null)
                    {
                        for (int i = 0; i < controls.Count; i++)
                        {
                            controls[i].OnParentFontChanged(e);
                        }
                    }
                }
                LayoutTransaction.DoLayout(this, this, PropertyNames.Font);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnForeColorChanged(EventArgs e)
        {
            if (!this.GetAnyDisposingInHierarchy())
            {
                this.Invalidate();
                EventHandler handler = base.Events[EventForeColor] as EventHandler;
                if (handler != null)
                {
                    handler(this, e);
                }
                ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                if (controls != null)
                {
                    for (int i = 0; i < controls.Count; i++)
                    {
                        controls[i].OnParentForeColorChanged(e);
                    }
                }
            }
        }

        internal virtual void OnFrameWindowActivate(bool fActivate)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnGiveFeedback(GiveFeedbackEventArgs gfbevent)
        {
            GiveFeedbackEventHandler handler = (GiveFeedbackEventHandler) base.Events[EventGiveFeedback];
            if (handler != null)
            {
                handler(this, gfbevent);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnGotFocus(EventArgs e)
        {
            if (this.IsActiveX)
            {
                this.ActiveXOnFocus(true);
            }
            if (this.parent != null)
            {
                this.parent.ChildGotFocus(this);
            }
            EventHandler handler = (EventHandler) base.Events[EventGotFocus];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnHandleCreated(EventArgs e)
        {
            if (this.IsHandleCreated)
            {
                if (!this.GetStyle(ControlStyles.UserPaint))
                {
                    this.SetWindowFont();
                }
                this.SetAcceptDrops(this.AllowDrop);
                System.Drawing.Region region = (System.Drawing.Region) this.Properties.GetObject(PropRegion);
                if (region != null)
                {
                    IntPtr hRgn = this.GetHRgn(region);
                    try
                    {
                        if (this.IsActiveX)
                        {
                            hRgn = this.ActiveXMergeRegion(hRgn);
                        }
                        if (System.Windows.Forms.UnsafeNativeMethods.SetWindowRgn(new HandleRef(this, this.Handle), new HandleRef(this, hRgn), System.Windows.Forms.SafeNativeMethods.IsWindowVisible(new HandleRef(this, this.Handle))) != 0)
                        {
                            hRgn = IntPtr.Zero;
                        }
                    }
                    finally
                    {
                        if (hRgn != IntPtr.Zero)
                        {
                            System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, hRgn));
                        }
                    }
                }
                ControlAccessibleObject obj2 = this.Properties.GetObject(PropAccessibility) as ControlAccessibleObject;
                ControlAccessibleObject obj3 = this.Properties.GetObject(PropNcAccessibility) as ControlAccessibleObject;
                IntPtr handle = this.Handle;
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    if (obj2 != null)
                    {
                        obj2.Handle = handle;
                    }
                    if (obj3 != null)
                    {
                        obj3.Handle = handle;
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                if ((this.text != null) && (this.text.Length != 0))
                {
                    System.Windows.Forms.UnsafeNativeMethods.SetWindowText(new HandleRef(this, this.Handle), this.text);
                }
                if ((!(this is ScrollableControl) && !this.IsMirrored) && (this.GetState2(2) && !this.GetState2(1)))
                {
                    this.BeginInvoke(new EventHandler(this.OnSetScrollPosition));
                    this.SetState2(1, true);
                    this.SetState2(2, false);
                }
                if (this.GetState2(8))
                {
                    this.ListenToUserPreferenceChanged(this.GetTopLevel());
                }
            }
            EventHandler handler = (EventHandler) base.Events[EventHandleCreated];
            if (handler != null)
            {
                handler(this, e);
            }
            if (this.IsHandleCreated && this.GetState(0x8000))
            {
                System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, this.Handle), threadCallbackMessage, IntPtr.Zero, IntPtr.Zero);
                this.SetState(0x8000, false);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnHandleDestroyed(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventHandleDestroyed];
            if (handler != null)
            {
                handler(this, e);
            }
            this.UpdateReflectParent(false);
            if (!this.RecreatingHandle)
            {
                if (this.GetState(0x200000))
                {
                    object obj2 = this.Properties.GetObject(PropBackBrush);
                    if (obj2 != null)
                    {
                        IntPtr handle = (IntPtr) obj2;
                        if (handle != IntPtr.Zero)
                        {
                            System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(this, handle));
                        }
                        this.Properties.SetObject(PropBackBrush, null);
                    }
                }
                this.ListenToUserPreferenceChanged(false);
            }
            try
            {
                if (!this.GetAnyDisposingInHierarchy())
                {
                    this.text = this.Text;
                    if ((this.text != null) && (this.text.Length == 0))
                    {
                        this.text = null;
                    }
                }
                this.SetAcceptDrops(false);
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnHelpRequested(HelpEventArgs hevent)
        {
            HelpEventHandler handler = (HelpEventHandler) base.Events[EventHelpRequested];
            if (handler != null)
            {
                handler(this, hevent);
                hevent.Handled = true;
            }
            if (!hevent.Handled && (this.ParentInternal != null))
            {
                this.ParentInternal.OnHelpRequested(hevent);
            }
        }

        internal void OnImeContextStatusChanged(IntPtr handle)
        {
            System.Windows.Forms.ImeMode imeMode = ImeContext.GetImeMode(handle);
            if (imeMode != System.Windows.Forms.ImeMode.Inherit)
            {
                System.Windows.Forms.ImeMode cachedImeMode = this.CachedImeMode;
                if (this.CanEnableIme)
                {
                    if (cachedImeMode != System.Windows.Forms.ImeMode.NoControl)
                    {
                        this.CachedImeMode = imeMode;
                        this.VerifyImeModeChanged(cachedImeMode, this.CachedImeMode);
                    }
                    else
                    {
                        PropagatingImeMode = imeMode;
                    }
                }
            }
        }

        protected virtual void OnImeModeChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventImeModeChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnInvalidated(InvalidateEventArgs e)
        {
            if (this.IsActiveX)
            {
                this.ActiveXViewChanged();
            }
            ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
            if (controls != null)
            {
                for (int i = 0; i < controls.Count; i++)
                {
                    controls[i].OnParentInvalidated(e);
                }
            }
            InvalidateEventHandler handler = (InvalidateEventHandler) base.Events[EventInvalidated];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal virtual void OnInvokedSetScrollPosition(object sender, EventArgs e)
        {
            if (!(this is ScrollableControl) && !this.IsMirrored)
            {
                System.Windows.Forms.NativeMethods.SCROLLINFO si = new System.Windows.Forms.NativeMethods.SCROLLINFO {
                    cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.SCROLLINFO)),
                    fMask = 1
                };
                if (System.Windows.Forms.UnsafeNativeMethods.GetScrollInfo(new HandleRef(this, this.Handle), 0, si))
                {
                    si.nPos = (this.RightToLeft == System.Windows.Forms.RightToLeft.Yes) ? si.nMax : si.nMin;
                    this.SendMessage(0x114, System.Windows.Forms.NativeMethods.Util.MAKELPARAM(4, si.nPos), 0);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            KeyEventHandler handler = (KeyEventHandler) base.Events[EventKeyDown];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnKeyPress(KeyPressEventArgs e)
        {
            KeyPressEventHandler handler = (KeyPressEventHandler) base.Events[EventKeyPress];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            KeyEventHandler handler = (KeyEventHandler) base.Events[EventKeyUp];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnLayout(LayoutEventArgs levent)
        {
            if (this.IsActiveX)
            {
                this.ActiveXViewChanged();
            }
            LayoutEventHandler handler = (LayoutEventHandler) base.Events[EventLayout];
            if (handler != null)
            {
                handler(this, levent);
            }
            if (this.LayoutEngine.Layout(this, levent) && (this.ParentInternal != null))
            {
                this.ParentInternal.SetState(0x800000, true);
            }
        }

        internal virtual void OnLayoutResuming(bool performLayout)
        {
            if (this.ParentInternal != null)
            {
                this.ParentInternal.OnChildLayoutResuming(this, performLayout);
            }
        }

        internal virtual void OnLayoutSuspended()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnLeave(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventLeave];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnLocationChanged(EventArgs e)
        {
            this.OnMove(EventArgs.Empty);
            EventHandler handler = base.Events[EventLocation] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnLostFocus(EventArgs e)
        {
            if (this.IsActiveX)
            {
                this.ActiveXOnFocus(false);
            }
            EventHandler handler = (EventHandler) base.Events[EventLostFocus];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnMarginChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventMarginChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMouseCaptureChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventMouseCaptureChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMouseClick(MouseEventArgs e)
        {
            MouseEventHandler handler = (MouseEventHandler) base.Events[EventMouseClick];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMouseDoubleClick(MouseEventArgs e)
        {
            MouseEventHandler handler = (MouseEventHandler) base.Events[EventMouseDoubleClick];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMouseDown(MouseEventArgs e)
        {
            MouseEventHandler handler = (MouseEventHandler) base.Events[EventMouseDown];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMouseEnter(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventMouseEnter];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMouseHover(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventMouseHover];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMouseLeave(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventMouseLeave];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMouseMove(MouseEventArgs e)
        {
            MouseEventHandler handler = (MouseEventHandler) base.Events[EventMouseMove];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            MouseEventHandler handler = (MouseEventHandler) base.Events[EventMouseUp];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMouseWheel(MouseEventArgs e)
        {
            MouseEventHandler handler = (MouseEventHandler) base.Events[EventMouseWheel];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMove(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventMove];
            if (handler != null)
            {
                handler(this, e);
            }
            if (this.RenderTransparent)
            {
                this.Invalidate();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnNotifyMessage(Message m)
        {
        }

        protected virtual void OnPaddingChanged(EventArgs e)
        {
            if (this.GetStyle(ControlStyles.ResizeRedraw))
            {
                this.Invalidate();
            }
            EventHandler handler = (EventHandler) base.Events[EventPaddingChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnPaint(PaintEventArgs e)
        {
            PaintEventHandler handler = (PaintEventHandler) base.Events[EventPaint];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnPaintBackground(PaintEventArgs pevent)
        {
            System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
            System.Windows.Forms.UnsafeNativeMethods.GetClientRect(new HandleRef(this.window, this.InternalHandle), ref rect);
            this.PaintBackground(pevent, new Rectangle(rect.left, rect.top, rect.right, rect.bottom));
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnParentBackColorChanged(EventArgs e)
        {
            if (this.Properties.GetColor(PropBackColor).IsEmpty)
            {
                this.OnBackColorChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnParentBackgroundImageChanged(EventArgs e)
        {
            this.OnBackgroundImageChanged(e);
        }

        internal virtual void OnParentBecameInvisible()
        {
            if (this.GetState(2))
            {
                ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                if (controls != null)
                {
                    for (int i = 0; i < controls.Count; i++)
                    {
                        controls[i].OnParentBecameInvisible();
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnParentBindingContextChanged(EventArgs e)
        {
            if (this.Properties.GetObject(PropBindingManager) == null)
            {
                this.OnBindingContextChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnParentChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventParent] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
            if (this.TopMostParent.IsActiveX)
            {
                this.OnTopMostActiveXParentChanged(EventArgs.Empty);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnParentCursorChanged(EventArgs e)
        {
            if (this.Properties.GetObject(PropCursor) == null)
            {
                this.OnCursorChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnParentEnabledChanged(EventArgs e)
        {
            if (this.GetState(4))
            {
                this.OnEnabledChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnParentFontChanged(EventArgs e)
        {
            if (this.Properties.GetObject(PropFont) == null)
            {
                this.OnFontChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnParentForeColorChanged(EventArgs e)
        {
            if (this.Properties.GetColor(PropForeColor).IsEmpty)
            {
                this.OnForeColorChanged(e);
            }
        }

        internal virtual void OnParentHandleRecreated()
        {
            Control parentInternal = this.ParentInternal;
            if ((parentInternal != null) && this.IsHandleCreated)
            {
                System.Windows.Forms.UnsafeNativeMethods.SetParent(new HandleRef(this, this.Handle), new HandleRef(parentInternal, parentInternal.Handle));
                this.UpdateZOrder();
            }
            this.SetState(0x20000000, false);
            if (this.ReflectParent == this.ParentInternal)
            {
                this.RecreateHandle();
            }
        }

        internal virtual void OnParentHandleRecreating()
        {
            this.SetState(0x20000000, true);
            if (this.IsHandleCreated)
            {
                Application.ParkHandle(new HandleRef(this, this.Handle));
            }
        }

        private void OnParentInvalidated(InvalidateEventArgs e)
        {
            if (this.RenderTransparent && this.IsHandleCreated)
            {
                Rectangle invalidRect = e.InvalidRect;
                Point location = this.Location;
                invalidRect.Offset(-location.X, -location.Y);
                invalidRect = Rectangle.Intersect(this.ClientRectangle, invalidRect);
                if (!invalidRect.IsEmpty)
                {
                    this.Invalidate(invalidRect);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnParentRightToLeftChanged(EventArgs e)
        {
            if (!this.Properties.ContainsInteger(PropRightToLeft) || (this.Properties.GetInteger(PropRightToLeft) == 2))
            {
                this.OnRightToLeftChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnParentVisibleChanged(EventArgs e)
        {
            if (this.GetState(2))
            {
                this.OnVisibleChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected virtual void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            PreviewKeyDownEventHandler handler = (PreviewKeyDownEventHandler) base.Events[EventPreviewKeyDown];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnPrint(PaintEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (this.GetStyle(ControlStyles.UserPaint))
            {
                this.PaintWithErrorHandling(e, 1);
                e.ResetGraphics();
                this.PaintWithErrorHandling(e, 2);
            }
            else
            {
                Message message;
                PrintPaintEventArgs args = e as PrintPaintEventArgs;
                bool flag = false;
                IntPtr zero = IntPtr.Zero;
                if (args == null)
                {
                    IntPtr lparam = (IntPtr) 30;
                    zero = e.HDC;
                    if (zero == IntPtr.Zero)
                    {
                        zero = e.Graphics.GetHdc();
                        flag = true;
                    }
                    message = Message.Create(this.Handle, 0x318, zero, lparam);
                }
                else
                {
                    message = args.Message;
                }
                try
                {
                    this.DefWndProc(ref message);
                }
                finally
                {
                    if (flag)
                    {
                        e.Graphics.ReleaseHdcInternal(zero);
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnQueryContinueDrag(QueryContinueDragEventArgs qcdevent)
        {
            QueryContinueDragEventHandler handler = (QueryContinueDragEventHandler) base.Events[EventQueryContinueDrag];
            if (handler != null)
            {
                handler(this, qcdevent);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnRegionChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventRegionChanged] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnResize(EventArgs e)
        {
            if (((this.controlStyle & ControlStyles.ResizeRedraw) == ControlStyles.ResizeRedraw) || this.GetState(0x400000))
            {
                this.Invalidate();
            }
            LayoutTransaction.DoLayout(this, this, PropertyNames.Bounds);
            EventHandler handler = (EventHandler) base.Events[EventResize];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnRightToLeftChanged(EventArgs e)
        {
            if (!this.GetAnyDisposingInHierarchy())
            {
                this.SetState2(2, true);
                this.RecreateHandle();
                EventHandler handler = base.Events[EventRightToLeft] as EventHandler;
                if (handler != null)
                {
                    handler(this, e);
                }
                ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                if (controls != null)
                {
                    for (int i = 0; i < controls.Count; i++)
                    {
                        controls[i].OnParentRightToLeftChanged(e);
                    }
                }
            }
        }

        private void OnSetScrollPosition(object sender, EventArgs e)
        {
            this.SetState2(1, false);
            this.OnInvokedSetScrollPosition(sender, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnSizeChanged(EventArgs e)
        {
            this.OnResize(EventArgs.Empty);
            EventHandler handler = base.Events[EventSize] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnStyleChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventStyleChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnSystemColorsChanged(EventArgs e)
        {
            ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
            if (controls != null)
            {
                for (int i = 0; i < controls.Count; i++)
                {
                    controls[i].OnSystemColorsChanged(EventArgs.Empty);
                }
            }
            this.Invalidate();
            EventHandler handler = (EventHandler) base.Events[EventSystemColorsChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnTabIndexChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventTabIndex] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnTabStopChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventTabStop] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnTextChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventText] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal virtual void OnTopMostActiveXParentChanged(EventArgs e)
        {
            ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
            if (controls != null)
            {
                for (int i = 0; i < controls.Count; i++)
                {
                    controls[i].OnTopMostActiveXParentChanged(e);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnValidated(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventValidated];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnValidating(CancelEventArgs e)
        {
            CancelEventHandler handler = (CancelEventHandler) base.Events[EventValidating];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnVisibleChanged(EventArgs e)
        {
            bool visible = this.Visible;
            if (visible)
            {
                this.UnhookMouseEvent();
                this.trackMouseEvent = null;
            }
            if (((this.parent != null) && visible) && (!this.Created && !this.GetAnyDisposingInHierarchy()))
            {
                this.CreateControl();
            }
            EventHandler handler = base.Events[EventVisible] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
            ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
            if (controls != null)
            {
                for (int i = 0; i < controls.Count; i++)
                {
                    Control control = controls[i];
                    if (control.Visible)
                    {
                        control.OnParentVisibleChanged(e);
                    }
                    if (!visible)
                    {
                        control.OnParentBecameInvisible();
                    }
                }
            }
        }

        private static void PaintBackColor(PaintEventArgs e, Rectangle rectangle, System.Drawing.Color backColor)
        {
            System.Drawing.Color nearestColor = backColor;
            if (((nearestColor.A == 0xff) && (e.HDC != IntPtr.Zero)) && (DisplayInformation.BitsPerPixel > 8))
            {
                using (WindowsGraphics graphics = WindowsGraphics.FromHdc(e.HDC))
                {
                    nearestColor = graphics.GetNearestColor(nearestColor);
                    using (WindowsBrush brush = new WindowsSolidBrush(graphics.DeviceContext, nearestColor))
                    {
                        graphics.FillRectangle(brush, rectangle);
                    }
                    return;
                }
            }
            if (nearestColor.A > 0)
            {
                if (nearestColor.A == 0xff)
                {
                    nearestColor = e.Graphics.GetNearestColor(nearestColor);
                }
                using (Brush brush2 = new SolidBrush(nearestColor))
                {
                    e.Graphics.FillRectangle(brush2, rectangle);
                }
            }
        }

        internal void PaintBackground(PaintEventArgs e, Rectangle rectangle)
        {
            this.PaintBackground(e, rectangle, this.BackColor, Point.Empty);
        }

        internal void PaintBackground(PaintEventArgs e, Rectangle rectangle, System.Drawing.Color backColor)
        {
            this.PaintBackground(e, rectangle, backColor, Point.Empty);
        }

        internal void PaintBackground(PaintEventArgs e, Rectangle rectangle, System.Drawing.Color backColor, Point scrollOffset)
        {
            if (this.RenderColorTransparent(backColor))
            {
                this.PaintTransparentBackground(e, rectangle);
            }
            bool flag = ((this is Form) || (this is MdiClient)) && this.IsMirrored;
            if (((this.BackgroundImage != null) && !DisplayInformation.HighContrast) && !flag)
            {
                if ((this.BackgroundImageLayout == ImageLayout.Tile) && ControlPaint.IsImageTransparent(this.BackgroundImage))
                {
                    this.PaintTransparentBackground(e, rectangle);
                }
                Point autoScrollPosition = scrollOffset;
                if ((this is ScrollableControl) && (autoScrollPosition != Point.Empty))
                {
                    autoScrollPosition = ((ScrollableControl) this).AutoScrollPosition;
                }
                if (ControlPaint.IsImageTransparent(this.BackgroundImage))
                {
                    PaintBackColor(e, rectangle, backColor);
                }
                ControlPaint.DrawBackgroundImage(e.Graphics, this.BackgroundImage, backColor, this.BackgroundImageLayout, this.ClientRectangle, rectangle, autoScrollPosition, this.RightToLeft);
            }
            else
            {
                PaintBackColor(e, rectangle, backColor);
            }
        }

        private void PaintException(PaintEventArgs e)
        {
            int num = 2;
            using (Pen pen = new Pen(System.Drawing.Color.Red, (float) num))
            {
                Rectangle clientRectangle = this.ClientRectangle;
                Rectangle rect = clientRectangle;
                rect.X++;
                rect.Y++;
                rect.Width--;
                rect.Height--;
                e.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                rect.Inflate(-1, -1);
                e.Graphics.FillRectangle(Brushes.White, rect);
                e.Graphics.DrawLine(pen, clientRectangle.Left, clientRectangle.Top, clientRectangle.Right, clientRectangle.Bottom);
                e.Graphics.DrawLine(pen, clientRectangle.Left, clientRectangle.Bottom, clientRectangle.Right, clientRectangle.Top);
            }
        }

        internal void PaintTransparentBackground(PaintEventArgs e, Rectangle rectangle)
        {
            this.PaintTransparentBackground(e, rectangle, null);
        }

        internal void PaintTransparentBackground(PaintEventArgs e, Rectangle rectangle, System.Drawing.Region transparentRegion)
        {
            Graphics g = e.Graphics;
            Control parentInternal = this.ParentInternal;
            if (parentInternal != null)
            {
                if (Application.RenderWithVisualStyles && parentInternal.RenderTransparencyWithVisualStyles)
                {
                    System.Drawing.Drawing2D.GraphicsState gstate = null;
                    if (transparentRegion != null)
                    {
                        gstate = g.Save();
                    }
                    try
                    {
                        if (transparentRegion != null)
                        {
                            g.Clip = transparentRegion;
                        }
                        ButtonRenderer.DrawParentBackground(g, rectangle, this);
                        return;
                    }
                    finally
                    {
                        if (gstate != null)
                        {
                            g.Restore(gstate);
                        }
                    }
                }
                Rectangle rectangle2 = new Rectangle(-this.Left, -this.Top, parentInternal.Width, parentInternal.Height);
                Rectangle clipRect = new Rectangle(rectangle.Left + this.Left, rectangle.Top + this.Top, rectangle.Width, rectangle.Height);
                using (WindowsGraphics graphics2 = WindowsGraphics.FromGraphics(g))
                {
                    graphics2.DeviceContext.TranslateTransform(-this.Left, -this.Top);
                    using (PaintEventArgs args = new PaintEventArgs(graphics2.GetHdc(), clipRect))
                    {
                        if (transparentRegion != null)
                        {
                            args.Graphics.Clip = transparentRegion;
                            args.Graphics.TranslateClip(-rectangle2.X, -rectangle2.Y);
                        }
                        try
                        {
                            this.InvokePaintBackground(parentInternal, args);
                            this.InvokePaint(parentInternal, args);
                        }
                        finally
                        {
                            if (transparentRegion != null)
                            {
                                args.Graphics.TranslateClip(rectangle2.X, rectangle2.Y);
                            }
                        }
                    }
                    return;
                }
            }
            g.FillRectangle(SystemBrushes.Control, rectangle);
        }

        private void PaintWithErrorHandling(PaintEventArgs e, short layer)
        {
            try
            {
                this.CacheTextInternal = true;
                if (this.GetState(0x400000))
                {
                    if (layer == 1)
                    {
                        this.PaintException(e);
                    }
                }
                else
                {
                    bool flag = true;
                    try
                    {
                        switch (layer)
                        {
                            case 1:
                                if (!this.GetStyle(ControlStyles.Opaque))
                                {
                                    this.OnPaintBackground(e);
                                }
                                break;

                            case 2:
                                this.OnPaint(e);
                                break;
                        }
                        flag = false;
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.SetState(0x400000, true);
                            this.Invalidate();
                        }
                    }
                }
            }
            finally
            {
                this.CacheTextInternal = false;
            }
        }

        internal bool PerformContainerValidation(ValidationConstraints validationConstraints)
        {
            bool flag = false;
            foreach (Control control in this.Controls)
            {
                if ((((validationConstraints & ValidationConstraints.ImmediateChildren) != ValidationConstraints.ImmediateChildren) && control.ShouldPerformContainerValidation()) && control.PerformContainerValidation(validationConstraints))
                {
                    flag = true;
                }
                if ((((((validationConstraints & ValidationConstraints.Selectable) != ValidationConstraints.Selectable) || control.GetStyle(ControlStyles.Selectable)) && (((validationConstraints & ValidationConstraints.Enabled) != ValidationConstraints.Enabled) || control.Enabled)) && ((((validationConstraints & ValidationConstraints.Visible) != ValidationConstraints.Visible) || control.Visible) && (((validationConstraints & ValidationConstraints.TabStop) != ValidationConstraints.TabStop) || control.TabStop))) && control.PerformControlValidation(true))
                {
                    flag = true;
                }
            }
            return flag;
        }

        internal bool PerformControlValidation(bool bulkValidation)
        {
            if (this.CausesValidation)
            {
                if (this.NotifyValidating())
                {
                    return true;
                }
                if (bulkValidation || NativeWindow.WndProcShouldBeDebuggable)
                {
                    this.NotifyValidated();
                }
                else
                {
                    try
                    {
                        this.NotifyValidated();
                    }
                    catch (Exception exception)
                    {
                        Application.OnThreadException(exception);
                    }
                }
            }
            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void PerformLayout()
        {
            if (this.cachedLayoutEventArgs != null)
            {
                this.PerformLayout(this.cachedLayoutEventArgs);
                this.cachedLayoutEventArgs = null;
                this.SetState2(0x40, false);
            }
            else
            {
                this.PerformLayout(null, null);
            }
        }

        internal void PerformLayout(LayoutEventArgs args)
        {
            if (!this.GetAnyDisposingInHierarchy())
            {
                if (this.layoutSuspendCount > 0)
                {
                    this.SetState(0x200, true);
                    if ((this.cachedLayoutEventArgs == null) || (this.GetState2(0x40) && (args != null)))
                    {
                        this.cachedLayoutEventArgs = args;
                        if (this.GetState2(0x40))
                        {
                            this.SetState2(0x40, false);
                        }
                    }
                    this.LayoutEngine.ProcessSuspendedLayoutEventArgs(this, args);
                }
                else
                {
                    this.layoutSuspendCount = 1;
                    try
                    {
                        this.CacheTextInternal = true;
                        this.OnLayout(args);
                    }
                    finally
                    {
                        this.CacheTextInternal = false;
                        this.SetState(0x800200, false);
                        this.layoutSuspendCount = 0;
                        if ((this.ParentInternal != null) && this.ParentInternal.GetState(0x800000))
                        {
                            LayoutTransaction.DoLayout(this.ParentInternal, this, PropertyNames.PreferredSize);
                        }
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void PerformLayout(Control affectedControl, string affectedProperty)
        {
            this.PerformLayout(new LayoutEventArgs(affectedControl, affectedProperty));
        }

        public Point PointToClient(Point p)
        {
            return this.PointToClientInternal(p);
        }

        internal Point PointToClientInternal(Point p)
        {
            System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT(p.X, p.Y);
            System.Windows.Forms.UnsafeNativeMethods.MapWindowPoints(System.Windows.Forms.NativeMethods.NullHandleRef, new HandleRef(this, this.Handle), pt, 1);
            return new Point(pt.x, pt.y);
        }

        public Point PointToScreen(Point p)
        {
            System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT(p.X, p.Y);
            System.Windows.Forms.UnsafeNativeMethods.MapWindowPoints(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.NullHandleRef, pt, 1);
            return new Point(pt.x, pt.y);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public PreProcessControlState PreProcessControlMessage(ref Message msg)
        {
            return PreProcessControlMessageInternal(null, ref msg);
        }

        internal static PreProcessControlState PreProcessControlMessageInternal(Control target, ref Message msg)
        {
            PreProcessControlState state2;
            if (target == null)
            {
                target = FromChildHandleInternal(msg.HWnd);
            }
            if (target == null)
            {
                return PreProcessControlState.MessageNotNeeded;
            }
            target.SetState2(0x80, false);
            target.SetState2(0x100, false);
            target.SetState2(0x200, true);
            try
            {
                Keys keyData = ((Keys) ((int) ((long) msg.WParam))) | ModifierKeys;
                if ((msg.Msg == 0x100) || (msg.Msg == 260))
                {
                    target.ProcessUICues(ref msg);
                    PreviewKeyDownEventArgs e = new PreviewKeyDownEventArgs(keyData);
                    target.OnPreviewKeyDown(e);
                    if (e.IsInputKey)
                    {
                        return PreProcessControlState.MessageNeeded;
                    }
                }
                PreProcessControlState messageNotNeeded = PreProcessControlState.MessageNotNeeded;
                if (!target.PreProcessMessage(ref msg))
                {
                    if ((msg.Msg == 0x100) || (msg.Msg == 260))
                    {
                        if (target.GetState2(0x80) || target.IsInputKey(keyData))
                        {
                            messageNotNeeded = PreProcessControlState.MessageNeeded;
                        }
                    }
                    else if (((msg.Msg == 0x102) || (msg.Msg == 0x106)) && (target.GetState2(0x100) || target.IsInputChar((char) ((int) msg.WParam))))
                    {
                        messageNotNeeded = PreProcessControlState.MessageNeeded;
                    }
                }
                else
                {
                    messageNotNeeded = PreProcessControlState.MessageProcessed;
                }
                state2 = messageNotNeeded;
            }
            finally
            {
                target.SetState2(0x200, false);
            }
            return state2;
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public virtual bool PreProcessMessage(ref Message msg)
        {
            if ((msg.Msg == 0x100) || (msg.Msg == 260))
            {
                if (!this.GetState2(0x200))
                {
                    this.ProcessUICues(ref msg);
                }
                Keys keyData = ((Keys) ((int) ((long) msg.WParam))) | ModifierKeys;
                if (this.ProcessCmdKey(ref msg, keyData))
                {
                    return true;
                }
                if (this.IsInputKey(keyData))
                {
                    this.SetState2(0x80, true);
                    return false;
                }
                System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                try
                {
                    return this.ProcessDialogKey(keyData);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            if ((msg.Msg == 0x102) || (msg.Msg == 0x106))
            {
                if ((msg.Msg == 0x102) && this.IsInputChar((char) ((int) msg.WParam)))
                {
                    this.SetState2(0x100, true);
                    return false;
                }
                return this.ProcessDialogChar((char) ((int) msg.WParam));
            }
            return false;
        }

        private void PrintToMetaFile(HandleRef hDC, IntPtr lParam)
        {
            lParam = (IntPtr) (((long) lParam) & -17L);
            System.Windows.Forms.NativeMethods.POINT point = new System.Windows.Forms.NativeMethods.POINT();
            System.Windows.Forms.SafeNativeMethods.GetViewportOrgEx(hDC, point);
            HandleRef hRgn = new HandleRef(null, System.Windows.Forms.SafeNativeMethods.CreateRectRgn(point.x, point.y, point.x + this.Width, point.y + this.Height));
            try
            {
                System.Windows.Forms.SafeNativeMethods.SelectClipRgn(hDC, hRgn);
                this.PrintToMetaFileRecursive(hDC, lParam, new Rectangle(Point.Empty, this.Size));
            }
            finally
            {
                System.Windows.Forms.SafeNativeMethods.DeleteObject(hRgn);
            }
        }

        private void PrintToMetaFile_SendPrintMessage(HandleRef hDC, IntPtr lParam)
        {
            if (this.GetStyle(ControlStyles.UserPaint))
            {
                this.SendMessage(0x317, hDC.Handle, lParam);
            }
            else
            {
                if (this.Controls.Count == 0)
                {
                    lParam = (IntPtr) (((long) lParam) | 0x10L);
                }
                using (MetafileDCWrapper wrapper = new MetafileDCWrapper(hDC, this.Size))
                {
                    this.SendMessage(0x317, wrapper.HDC, lParam);
                }
            }
        }

        internal virtual void PrintToMetaFileRecursive(HandleRef hDC, IntPtr lParam, Rectangle bounds)
        {
            using (new WindowsFormsUtils.DCMapping(hDC, bounds))
            {
                this.PrintToMetaFile_SendPrintMessage(hDC, (IntPtr) (((long) lParam) & -5L));
                System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(null, this.Handle), ref rect);
                Point location = this.PointToScreen(Point.Empty);
                location = new Point(location.X - rect.left, location.Y - rect.top);
                Rectangle rectangle = new Rectangle(location, this.ClientSize);
                using (new WindowsFormsUtils.DCMapping(hDC, rectangle))
                {
                    this.PrintToMetaFile_SendPrintMessage(hDC, (IntPtr) (((long) lParam) & -3L));
                    for (int i = this.Controls.Count - 1; i >= 0; i--)
                    {
                        Control control = this.Controls[i];
                        if (control.Visible)
                        {
                            control.PrintToMetaFileRecursive(hDC, lParam, control.Bounds);
                        }
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected virtual bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            System.Windows.Forms.ContextMenu menu = (System.Windows.Forms.ContextMenu) this.Properties.GetObject(PropContextMenu);
            return (((menu != null) && menu.ProcessCmdKey(ref msg, keyData, this)) || ((this.parent != null) && this.parent.ProcessCmdKey(ref msg, keyData)));
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows), UIPermission(SecurityAction.InheritanceDemand, Window=UIPermissionWindow.AllWindows)]
        protected virtual bool ProcessDialogChar(char charCode)
        {
            return ((this.parent != null) && this.parent.ProcessDialogChar(charCode));
        }

        [UIPermission(SecurityAction.InheritanceDemand, Window=UIPermissionWindow.AllWindows), UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected virtual bool ProcessDialogKey(Keys keyData)
        {
            return ((this.parent != null) && this.parent.ProcessDialogKey(keyData));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected virtual bool ProcessKeyEventArgs(ref Message m)
        {
            KeyEventArgs e = null;
            KeyPressEventArgs args2 = null;
            IntPtr zero = IntPtr.Zero;
            if ((m.Msg == 0x102) || (m.Msg == 0x106))
            {
                int imeWmCharsToIgnore = this.ImeWmCharsToIgnore;
                if (imeWmCharsToIgnore > 0)
                {
                    imeWmCharsToIgnore--;
                    this.ImeWmCharsToIgnore = imeWmCharsToIgnore;
                    return false;
                }
                args2 = new KeyPressEventArgs((char) ((ushort) ((long) m.WParam)));
                this.OnKeyPress(args2);
                zero = (IntPtr) args2.KeyChar;
            }
            else if (m.Msg == 0x286)
            {
                int num2 = this.ImeWmCharsToIgnore;
                if (Marshal.SystemDefaultCharSize == 1)
                {
                    char ch = '\0';
                    byte[] lpMultiByteStr = new byte[] { (byte) (((int) ((long) m.WParam)) >> 8), (byte) ((long) m.WParam) };
                    char[] lpWideCharStr = new char[1];
                    int num3 = System.Windows.Forms.UnsafeNativeMethods.MultiByteToWideChar(0, 1, lpMultiByteStr, lpMultiByteStr.Length, lpWideCharStr, 0);
                    if (num3 <= 0)
                    {
                        throw new Win32Exception();
                    }
                    lpWideCharStr = new char[num3];
                    System.Windows.Forms.UnsafeNativeMethods.MultiByteToWideChar(0, 1, lpMultiByteStr, lpMultiByteStr.Length, lpWideCharStr, lpWideCharStr.Length);
                    if (lpWideCharStr[0] != '\0')
                    {
                        ch = lpWideCharStr[0];
                        num2 += 2;
                    }
                    else if ((lpWideCharStr[0] == '\0') && (lpWideCharStr.Length >= 2))
                    {
                        ch = lpWideCharStr[1];
                        num2++;
                    }
                    this.ImeWmCharsToIgnore = num2;
                    args2 = new KeyPressEventArgs(ch);
                }
                else
                {
                    num2 += 3 - Marshal.SystemDefaultCharSize;
                    this.ImeWmCharsToIgnore = num2;
                    args2 = new KeyPressEventArgs((char) ((ushort) ((long) m.WParam)));
                }
                char keyChar = args2.KeyChar;
                this.OnKeyPress(args2);
                if (args2.KeyChar == keyChar)
                {
                    zero = m.WParam;
                }
                else if (Marshal.SystemDefaultCharSize == 1)
                {
                    string wideStr = new string(new char[] { args2.KeyChar });
                    byte[] pOutBytes = null;
                    int num4 = System.Windows.Forms.UnsafeNativeMethods.WideCharToMultiByte(0, 0, wideStr, wideStr.Length, null, 0, IntPtr.Zero, IntPtr.Zero);
                    if (num4 >= 2)
                    {
                        pOutBytes = new byte[num4];
                        System.Windows.Forms.UnsafeNativeMethods.WideCharToMultiByte(0, 0, wideStr, wideStr.Length, pOutBytes, pOutBytes.Length, IntPtr.Zero, IntPtr.Zero);
                        int num5 = Marshal.SizeOf(typeof(IntPtr));
                        if (num4 > num5)
                        {
                            num4 = num5;
                        }
                        long num6 = 0L;
                        for (int i = 0; i < num4; i++)
                        {
                            num6 = num6 << 8;
                            num6 |= pOutBytes[i];
                        }
                        zero = (IntPtr) num6;
                    }
                    else if (num4 == 1)
                    {
                        pOutBytes = new byte[num4];
                        System.Windows.Forms.UnsafeNativeMethods.WideCharToMultiByte(0, 0, wideStr, wideStr.Length, pOutBytes, pOutBytes.Length, IntPtr.Zero, IntPtr.Zero);
                        zero = (IntPtr) pOutBytes[0];
                    }
                    else
                    {
                        zero = m.WParam;
                    }
                }
                else
                {
                    zero = (IntPtr) args2.KeyChar;
                }
            }
            else
            {
                e = new KeyEventArgs(((Keys) ((int) ((long) m.WParam))) | ModifierKeys);
                if ((m.Msg == 0x100) || (m.Msg == 260))
                {
                    this.OnKeyDown(e);
                }
                else
                {
                    this.OnKeyUp(e);
                }
            }
            if (args2 != null)
            {
                m.WParam = zero;
                return args2.Handled;
            }
            if (e.SuppressKeyPress)
            {
                this.RemovePendingMessages(0x102, 0x102);
                this.RemovePendingMessages(0x106, 0x106);
                this.RemovePendingMessages(0x286, 0x286);
            }
            return e.Handled;
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected internal virtual bool ProcessKeyMessage(ref Message m)
        {
            return (((this.parent != null) && this.parent.ProcessKeyPreview(ref m)) || this.ProcessKeyEventArgs(ref m));
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected virtual bool ProcessKeyPreview(ref Message m)
        {
            return ((this.parent != null) && this.parent.ProcessKeyPreview(ref m));
        }

        [UIPermission(SecurityAction.InheritanceDemand, Window=UIPermissionWindow.AllWindows), UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal virtual bool ProcessMnemonic(char charCode)
        {
            return false;
        }

        internal void ProcessUICues(ref Message msg)
        {
            Keys keys = ((Keys) ((int) msg.WParam)) & Keys.KeyCode;
            switch (keys)
            {
                case Keys.F10:
                case Keys.Menu:
                case Keys.Tab:
                {
                    Control wrapper = null;
                    int num = (int) ((long) this.SendMessage(0x129, 0, 0));
                    if (num == 0)
                    {
                        wrapper = this.TopMostParent;
                        num = (int) wrapper.SendMessage(0x129, 0, 0);
                    }
                    int num2 = 0;
                    if (((keys == Keys.F10) || (keys == Keys.Menu)) && ((num & 2) != 0))
                    {
                        num2 |= 2;
                    }
                    if ((keys == Keys.Tab) && ((num & 1) != 0))
                    {
                        num2 |= 1;
                    }
                    if (num2 != 0)
                    {
                        if (wrapper == null)
                        {
                            wrapper = this.TopMostParent;
                        }
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(wrapper, wrapper.Handle), (System.Windows.Forms.UnsafeNativeMethods.GetParent(new HandleRef(null, wrapper.Handle)) == IntPtr.Zero) ? 0x127 : 0x128, (IntPtr) (2 | (num2 << 0x10)), IntPtr.Zero);
                    }
                    break;
                }
            }
        }

        internal void RaiseCreateHandleEvent(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventHandleCreated];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void RaiseDragEvent(object key, DragEventArgs e)
        {
            DragEventHandler handler = (DragEventHandler) base.Events[key];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void RaiseKeyEvent(object key, KeyEventArgs e)
        {
            KeyEventHandler handler = (KeyEventHandler) base.Events[key];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void RaiseMouseEvent(object key, MouseEventArgs e)
        {
            MouseEventHandler handler = (MouseEventHandler) base.Events[key];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void RaisePaintEvent(object key, PaintEventArgs e)
        {
            PaintEventHandler handler = (PaintEventHandler) base.Events[EventPaint];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void RecreateHandle()
        {
            this.RecreateHandleCore();
        }

        internal virtual void RecreateHandleCore()
        {
            lock (this)
            {
                if (this.IsHandleCreated)
                {
                    bool containsFocus = this.ContainsFocus;
                    bool flag2 = (this.state & 1) != 0;
                    if (this.GetState(0x4000))
                    {
                        this.SetState(0x2000, true);
                        this.UnhookMouseEvent();
                    }
                    HandleRef hWnd = new HandleRef(this, System.Windows.Forms.UnsafeNativeMethods.GetParent(new HandleRef(this, this.Handle)));
                    try
                    {
                        Control[] controlArray = null;
                        this.state |= 0x10;
                        try
                        {
                            ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                            if ((controls != null) && (controls.Count > 0))
                            {
                                controlArray = new Control[controls.Count];
                                for (int i = 0; i < controls.Count; i++)
                                {
                                    Control control = controls[i];
                                    if ((control != null) && control.IsHandleCreated)
                                    {
                                        control.OnParentHandleRecreating();
                                        controlArray[i] = control;
                                    }
                                    else
                                    {
                                        controlArray[i] = null;
                                    }
                                }
                            }
                            this.DestroyHandle();
                            this.CreateHandle();
                        }
                        finally
                        {
                            this.state &= -17;
                            if (controlArray != null)
                            {
                                for (int j = 0; j < controlArray.Length; j++)
                                {
                                    Control control2 = controlArray[j];
                                    if ((control2 != null) && control2.IsHandleCreated)
                                    {
                                        control2.OnParentHandleRecreated();
                                    }
                                }
                            }
                        }
                        if (flag2)
                        {
                            this.CreateControl();
                        }
                    }
                    finally
                    {
                        if (((hWnd.Handle != IntPtr.Zero) && ((FromHandleInternal(hWnd.Handle) == null) || (this.parent == null))) && System.Windows.Forms.UnsafeNativeMethods.IsWindow(hWnd))
                        {
                            System.Windows.Forms.UnsafeNativeMethods.SetParent(new HandleRef(this, this.Handle), hWnd);
                        }
                    }
                    if (containsFocus)
                    {
                        this.FocusInternal();
                    }
                }
            }
        }

        public Rectangle RectangleToClient(Rectangle r)
        {
            System.Windows.Forms.NativeMethods.RECT rect = System.Windows.Forms.NativeMethods.RECT.FromXYWH(r.X, r.Y, r.Width, r.Height);
            System.Windows.Forms.UnsafeNativeMethods.MapWindowPoints(System.Windows.Forms.NativeMethods.NullHandleRef, new HandleRef(this, this.Handle), ref rect, 2);
            return Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
        }

        public Rectangle RectangleToScreen(Rectangle r)
        {
            System.Windows.Forms.NativeMethods.RECT rect = System.Windows.Forms.NativeMethods.RECT.FromXYWH(r.X, r.Y, r.Width, r.Height);
            System.Windows.Forms.UnsafeNativeMethods.MapWindowPoints(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.NullHandleRef, ref rect, 2);
            return Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected static bool ReflectMessage(IntPtr hWnd, ref Message m)
        {
            System.Windows.Forms.IntSecurity.SendMessages.Demand();
            return ReflectMessageInternal(hWnd, ref m);
        }

        internal static bool ReflectMessageInternal(IntPtr hWnd, ref Message m)
        {
            Control control = FromHandleInternal(hWnd);
            if (control == null)
            {
                return false;
            }
            m.Result = control.SendMessage(0x2000 + m.Msg, m.WParam, m.LParam);
            return true;
        }

        public virtual void Refresh()
        {
            this.Invalidate(true);
            this.Update();
        }

        private void RemovePendingMessages(int msgMin, int msgMax)
        {
            if (!this.IsDisposed)
            {
                System.Windows.Forms.NativeMethods.MSG msg = new System.Windows.Forms.NativeMethods.MSG();
                IntPtr handle = this.Handle;
                while (System.Windows.Forms.UnsafeNativeMethods.PeekMessage(ref msg, new HandleRef(this, handle), msgMin, msgMax, 1))
                {
                }
            }
        }

        internal virtual void RemoveReflectChild()
        {
        }

        private bool RenderColorTransparent(System.Drawing.Color c)
        {
            return (this.GetStyle(ControlStyles.SupportsTransparentBackColor) && (c.A < 0xff));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ResetBackColor()
        {
            this.BackColor = System.Drawing.Color.Empty;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ResetBindings()
        {
            ControlBindingsCollection bindingss = (ControlBindingsCollection) this.Properties.GetObject(PropBindings);
            if (bindingss != null)
            {
                bindingss.Clear();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ResetCursor()
        {
            this.Cursor = null;
        }

        private void ResetEnabled()
        {
            this.Enabled = true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ResetFont()
        {
            this.Font = null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ResetForeColor()
        {
            this.ForeColor = System.Drawing.Color.Empty;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ResetImeMode()
        {
            this.ImeMode = this.DefaultImeMode;
        }

        private void ResetLocation()
        {
            this.Location = new Point(0, 0);
        }

        private void ResetMargin()
        {
            this.Margin = this.DefaultMargin;
        }

        private void ResetMinimumSize()
        {
            this.MinimumSize = this.DefaultMinimumSize;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void ResetMouseEventArgs()
        {
            if (this.GetState(0x4000))
            {
                this.UnhookMouseEvent();
                this.HookMouseEvent();
            }
        }

        private void ResetPadding()
        {
            CommonProperties.ResetPadding(this);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ResetRightToLeft()
        {
            this.RightToLeft = System.Windows.Forms.RightToLeft.Inherit;
        }

        private void ResetSize()
        {
            this.Size = this.DefaultSize;
        }

        public virtual void ResetText()
        {
            this.Text = string.Empty;
        }

        private void ResetVisible()
        {
            this.Visible = true;
        }

        public void ResumeLayout()
        {
            this.ResumeLayout(true);
        }

        public void ResumeLayout(bool performLayout)
        {
            bool flag = false;
            if (this.layoutSuspendCount > 0)
            {
                if (this.layoutSuspendCount == 1)
                {
                    this.layoutSuspendCount = (byte) (this.layoutSuspendCount + 1);
                    try
                    {
                        this.OnLayoutResuming(performLayout);
                    }
                    finally
                    {
                        this.layoutSuspendCount = (byte) (this.layoutSuspendCount - 1);
                    }
                }
                this.layoutSuspendCount = (byte) (this.layoutSuspendCount - 1);
                if (((this.layoutSuspendCount == 0) && this.GetState(0x200)) && performLayout)
                {
                    this.PerformLayout();
                    flag = true;
                }
            }
            if (!flag)
            {
                this.SetState2(0x40, true);
            }
            if (!performLayout)
            {
                CommonProperties.xClearPreferredSizeCache(this);
                ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                if (controls != null)
                {
                    for (int i = 0; i < controls.Count; i++)
                    {
                        this.LayoutEngine.InitLayout(controls[i], BoundsSpecified.All);
                        CommonProperties.xClearPreferredSizeCache(controls[i]);
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected ContentAlignment RtlTranslateAlignment(ContentAlignment align)
        {
            return this.RtlTranslateContent(align);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected HorizontalAlignment RtlTranslateAlignment(HorizontalAlignment align)
        {
            return this.RtlTranslateHorizontal(align);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected LeftRightAlignment RtlTranslateAlignment(LeftRightAlignment align)
        {
            return this.RtlTranslateLeftRight(align);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal ContentAlignment RtlTranslateContent(ContentAlignment align)
        {
            if (System.Windows.Forms.RightToLeft.Yes != this.RightToLeft)
            {
                return align;
            }
            if ((align & WindowsFormsUtils.AnyTopAlign) != ((ContentAlignment) 0))
            {
                switch (align)
                {
                    case ContentAlignment.TopLeft:
                        return ContentAlignment.TopRight;

                    case ContentAlignment.TopRight:
                        return ContentAlignment.TopLeft;
                }
            }
            if ((align & WindowsFormsUtils.AnyMiddleAlign) != ((ContentAlignment) 0))
            {
                switch (align)
                {
                    case ContentAlignment.MiddleLeft:
                        return ContentAlignment.MiddleRight;

                    case ContentAlignment.MiddleRight:
                        return ContentAlignment.MiddleLeft;
                }
            }
            if ((align & WindowsFormsUtils.AnyBottomAlign) == ((ContentAlignment) 0))
            {
                return align;
            }
            ContentAlignment alignment3 = align;
            if (alignment3 != ContentAlignment.BottomLeft)
            {
                if (alignment3 == ContentAlignment.BottomRight)
                {
                    return ContentAlignment.BottomLeft;
                }
                return align;
            }
            return ContentAlignment.BottomRight;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected HorizontalAlignment RtlTranslateHorizontal(HorizontalAlignment align)
        {
            if (System.Windows.Forms.RightToLeft.Yes == this.RightToLeft)
            {
                if (align == HorizontalAlignment.Left)
                {
                    return HorizontalAlignment.Right;
                }
                if (HorizontalAlignment.Right == align)
                {
                    return HorizontalAlignment.Left;
                }
            }
            return align;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected LeftRightAlignment RtlTranslateLeftRight(LeftRightAlignment align)
        {
            if (System.Windows.Forms.RightToLeft.Yes == this.RightToLeft)
            {
                if (align == LeftRightAlignment.Left)
                {
                    return LeftRightAlignment.Right;
                }
                if (LeftRightAlignment.Right == align)
                {
                    return LeftRightAlignment.Left;
                }
            }
            return align;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void Scale(SizeF factor)
        {
            using (new LayoutTransaction(this, this, PropertyNames.Bounds, false))
            {
                this.ScaleControl(factor, factor, this);
                if (this.ScaleChildren)
                {
                    ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                    if (controls != null)
                    {
                        for (int i = 0; i < controls.Count; i++)
                        {
                            controls[i].Scale(factor);
                        }
                    }
                }
            }
            LayoutTransaction.DoLayout(this, this, PropertyNames.Bounds);
        }

        [Obsolete("This method has been deprecated. Use the Scale(SizeF ratio) method instead. http://go.microsoft.com/fwlink/?linkid=14202"), EditorBrowsable(EditorBrowsableState.Never)]
        public void Scale(float ratio)
        {
            this.ScaleCore(ratio, ratio);
        }

        [Obsolete("This method has been deprecated. Use the Scale(SizeF ratio) method instead. http://go.microsoft.com/fwlink/?linkid=14202"), EditorBrowsable(EditorBrowsableState.Never)]
        public void Scale(float dx, float dy)
        {
            this.SuspendLayout();
            try
            {
                this.ScaleCore(dx, dy);
            }
            finally
            {
                this.ResumeLayout();
            }
        }

        internal virtual void Scale(SizeF includedFactor, SizeF excludedFactor, Control requestingControl)
        {
            using (new LayoutTransaction(this, this, PropertyNames.Bounds, false))
            {
                this.ScaleControl(includedFactor, excludedFactor, requestingControl);
                this.ScaleChildControls(includedFactor, excludedFactor, requestingControl);
            }
            LayoutTransaction.DoLayout(this, this, PropertyNames.Bounds);
        }

        internal void ScaleChildControls(SizeF includedFactor, SizeF excludedFactor, Control requestingControl)
        {
            if (this.ScaleChildren)
            {
                ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                if (controls != null)
                {
                    for (int i = 0; i < controls.Count; i++)
                    {
                        controls[i].Scale(includedFactor, excludedFactor, requestingControl);
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            System.Windows.Forms.CreateParams createParams = this.CreateParams;
            System.Windows.Forms.NativeMethods.RECT lpRect = new System.Windows.Forms.NativeMethods.RECT(0, 0, 0, 0);
            System.Windows.Forms.SafeNativeMethods.AdjustWindowRectEx(ref lpRect, createParams.Style, this.HasMenu, createParams.ExStyle);
            System.Drawing.Size minimumSize = this.MinimumSize;
            System.Drawing.Size maximumSize = this.MaximumSize;
            this.MinimumSize = System.Drawing.Size.Empty;
            this.MaximumSize = System.Drawing.Size.Empty;
            Rectangle rectangle = this.GetScaledBounds(this.Bounds, factor, specified);
            float width = factor.Width;
            float height = factor.Height;
            System.Windows.Forms.Padding padding = this.Padding;
            System.Windows.Forms.Padding margin = this.Margin;
            if (width == 1f)
            {
                specified &= ~(BoundsSpecified.Width | BoundsSpecified.X);
            }
            if (height == 1f)
            {
                specified &= ~(BoundsSpecified.Height | BoundsSpecified.Y);
            }
            if (width != 1f)
            {
                padding.Left = (int) Math.Round((double) (padding.Left * width));
                padding.Right = (int) Math.Round((double) (padding.Right * width));
                margin.Left = (int) Math.Round((double) (margin.Left * width));
                margin.Right = (int) Math.Round((double) (margin.Right * width));
            }
            if (height != 1f)
            {
                padding.Top = (int) Math.Round((double) (padding.Top * height));
                padding.Bottom = (int) Math.Round((double) (padding.Bottom * height));
                margin.Top = (int) Math.Round((double) (margin.Top * height));
                margin.Bottom = (int) Math.Round((double) (margin.Bottom * height));
            }
            this.Padding = padding;
            this.Margin = margin;
            System.Drawing.Size size = lpRect.Size;
            if (!minimumSize.IsEmpty)
            {
                minimumSize -= size;
                minimumSize = this.ScaleSize(LayoutUtils.UnionSizes(System.Drawing.Size.Empty, minimumSize), factor.Width, factor.Height) + size;
            }
            if (!maximumSize.IsEmpty)
            {
                maximumSize -= size;
                maximumSize = this.ScaleSize(LayoutUtils.UnionSizes(System.Drawing.Size.Empty, maximumSize), factor.Width, factor.Height) + size;
            }
            System.Drawing.Size b = LayoutUtils.ConvertZeroToUnbounded(maximumSize);
            System.Drawing.Size size5 = LayoutUtils.UnionSizes(LayoutUtils.IntersectSizes(rectangle.Size, b), minimumSize);
            this.SetBoundsCore(rectangle.X, rectangle.Y, size5.Width, size5.Height, BoundsSpecified.All);
            this.MaximumSize = maximumSize;
            this.MinimumSize = minimumSize;
        }

        internal void ScaleControl(SizeF includedFactor, SizeF excludedFactor, Control requestingControl)
        {
            BoundsSpecified none = BoundsSpecified.None;
            BoundsSpecified specified = BoundsSpecified.None;
            if (!includedFactor.IsEmpty)
            {
                none = this.RequiredScaling;
            }
            if (!excludedFactor.IsEmpty)
            {
                specified |= ~this.RequiredScaling & BoundsSpecified.All;
            }
            if (none != BoundsSpecified.None)
            {
                this.ScaleControl(includedFactor, none);
            }
            if (specified != BoundsSpecified.None)
            {
                this.ScaleControl(excludedFactor, specified);
            }
            if (!includedFactor.IsEmpty)
            {
                this.RequiredScaling = BoundsSpecified.None;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void ScaleCore(float dx, float dy)
        {
            this.SuspendLayout();
            try
            {
                int x = (int) Math.Round((double) (this.x * dx));
                int y = (int) Math.Round((double) (this.y * dy));
                int width = this.width;
                if ((this.controlStyle & ControlStyles.FixedWidth) != ControlStyles.FixedWidth)
                {
                    width = ((int) Math.Round((double) ((this.x + this.width) * dx))) - x;
                }
                int height = this.height;
                if ((this.controlStyle & ControlStyles.FixedHeight) != ControlStyles.FixedHeight)
                {
                    height = ((int) Math.Round((double) ((this.y + this.height) * dy))) - y;
                }
                this.SetBounds(x, y, width, height, BoundsSpecified.All);
                ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                if (controls != null)
                {
                    for (int i = 0; i < controls.Count; i++)
                    {
                        controls[i].Scale(dx, dy);
                    }
                }
            }
            finally
            {
                this.ResumeLayout();
            }
        }

        internal System.Drawing.Size ScaleSize(System.Drawing.Size startSize, float x, float y)
        {
            System.Drawing.Size size = startSize;
            if (!this.GetStyle(ControlStyles.FixedWidth))
            {
                size.Width = (int) Math.Round((double) (size.Width * x));
            }
            if (!this.GetStyle(ControlStyles.FixedHeight))
            {
                size.Height = (int) Math.Round((double) (size.Height * y));
            }
            return size;
        }

        public void Select()
        {
            this.Select(false, false);
        }

        protected virtual void Select(bool directed, bool forward)
        {
            IContainerControl containerControlInternal = this.GetContainerControlInternal();
            if (containerControlInternal != null)
            {
                containerControlInternal.ActiveControl = this;
            }
        }

        public bool SelectNextControl(Control ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
        {
            if (!this.Contains(ctl) || (!nested && (ctl.parent != this)))
            {
                ctl = null;
            }
            bool flag = false;
            Control control = ctl;
            do
            {
                ctl = this.GetNextControl(ctl, forward);
                if (ctl == null)
                {
                    if (!wrap)
                    {
                        break;
                    }
                    if (flag)
                    {
                        return false;
                    }
                    flag = true;
                }
                else if ((ctl.CanSelect && (!tabStopOnly || ctl.TabStop)) && (nested || (ctl.parent == this)))
                {
                    ctl.Select(true, forward);
                    return true;
                }
            }
            while (ctl != control);
            return false;
        }

        internal bool SelectNextControlInternal(Control ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
        {
            return this.SelectNextControl(ctl, forward, tabStopOnly, nested, wrap);
        }

        private void SelectNextIfFocused()
        {
            if (this.ContainsFocus && (this.ParentInternal != null))
            {
                IContainerControl containerControlInternal = this.ParentInternal.GetContainerControlInternal();
                if (containerControlInternal != null)
                {
                    ((Control) containerControlInternal).SelectNextControlInternal(this, true, true, true, true);
                }
            }
        }

        internal IntPtr SendMessage(int msg, ref int wparam, ref int lparam)
        {
            return System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), msg, ref wparam, ref lparam);
        }

        internal IntPtr SendMessage(int msg, bool wparam, int lparam)
        {
            return System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), msg, wparam, lparam);
        }

        internal IntPtr SendMessage(int msg, int wparam, int lparam)
        {
            return System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), msg, wparam, lparam);
        }

        internal IntPtr SendMessage(int msg, int wparam, IntPtr lparam)
        {
            return System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), msg, (IntPtr) wparam, lparam);
        }

        internal IntPtr SendMessage(int msg, int wparam, ref System.Windows.Forms.NativeMethods.RECT lparam)
        {
            return System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), msg, wparam, ref lparam);
        }

        internal IntPtr SendMessage(int msg, int wparam, string lparam)
        {
            return System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), msg, wparam, lparam);
        }

        internal IntPtr SendMessage(int msg, IntPtr wparam, int lparam)
        {
            return System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), msg, wparam, (IntPtr) lparam);
        }

        internal IntPtr SendMessage(int msg, IntPtr wparam, IntPtr lparam)
        {
            return System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), msg, wparam, lparam);
        }

        public void SendToBack()
        {
            if (this.parent != null)
            {
                this.parent.Controls.SetChildIndex(this, -1);
            }
            else if (this.IsHandleCreated && this.GetTopLevel())
            {
                System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this.window, this.Handle), System.Windows.Forms.NativeMethods.HWND_BOTTOM, 0, 0, 0, 0, 3);
            }
        }

        internal void SetAcceptDrops(bool accept)
        {
            if ((accept != this.GetState(0x80)) && this.IsHandleCreated)
            {
                try
                {
                    if (Application.OleRequired() != ApartmentState.STA)
                    {
                        throw new ThreadStateException(System.Windows.Forms.SR.GetString("ThreadMustBeSTA"));
                    }
                    if (accept)
                    {
                        System.Windows.Forms.IntSecurity.ClipboardRead.Demand();
                        int error = System.Windows.Forms.UnsafeNativeMethods.RegisterDragDrop(new HandleRef(this, this.Handle), new DropTarget(this));
                        if ((error != 0) && (error != -2147221247))
                        {
                            throw new Win32Exception(error);
                        }
                    }
                    else
                    {
                        int num2 = System.Windows.Forms.UnsafeNativeMethods.RevokeDragDrop(new HandleRef(this, this.Handle));
                        if ((num2 != 0) && (num2 != -2147221248))
                        {
                            throw new Win32Exception(num2);
                        }
                    }
                    this.SetState(0x80, accept);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DragDropRegFailed"), exception);
                }
            }
        }

        protected void SetAutoSizeMode(AutoSizeMode mode)
        {
            CommonProperties.SetAutoSizeMode(this, mode);
        }

        public void SetBounds(int x, int y, int width, int height)
        {
            if (((this.x != x) || (this.y != y)) || ((this.width != width) || (this.height != height)))
            {
                this.SetBoundsCore(x, y, width, height, BoundsSpecified.All);
                LayoutTransaction.DoLayout(this.ParentInternal, this, PropertyNames.Bounds);
            }
            else
            {
                this.InitScaling(BoundsSpecified.All);
            }
        }

        public void SetBounds(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if ((specified & BoundsSpecified.X) == BoundsSpecified.None)
            {
                x = this.x;
            }
            if ((specified & BoundsSpecified.Y) == BoundsSpecified.None)
            {
                y = this.y;
            }
            if ((specified & BoundsSpecified.Width) == BoundsSpecified.None)
            {
                width = this.width;
            }
            if ((specified & BoundsSpecified.Height) == BoundsSpecified.None)
            {
                height = this.height;
            }
            if (((this.x != x) || (this.y != y)) || ((this.width != width) || (this.height != height)))
            {
                this.SetBoundsCore(x, y, width, height, specified);
                LayoutTransaction.DoLayout(this.ParentInternal, this, PropertyNames.Bounds);
            }
            else
            {
                this.InitScaling(specified);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (this.ParentInternal != null)
            {
                this.ParentInternal.SuspendLayout();
            }
            try
            {
                if (((this.x != x) || (this.y != y)) || ((this.width != width) || (this.height != height)))
                {
                    CommonProperties.UpdateSpecifiedBounds(this, x, y, width, height, specified);
                    Rectangle rectangle = this.ApplyBoundsConstraints(x, y, width, height);
                    width = rectangle.Width;
                    height = rectangle.Height;
                    x = rectangle.X;
                    y = rectangle.Y;
                    if (!this.IsHandleCreated)
                    {
                        this.UpdateBounds(x, y, width, height);
                    }
                    else if (!this.GetState(0x10000))
                    {
                        int flags = 20;
                        if ((this.x == x) && (this.y == y))
                        {
                            flags |= 2;
                        }
                        if ((this.width == width) && (this.height == height))
                        {
                            flags |= 1;
                        }
                        this.OnBoundsUpdate(x, y, width, height);
                        System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this.window, this.Handle), System.Windows.Forms.NativeMethods.NullHandleRef, x, y, width, height, flags);
                    }
                }
            }
            finally
            {
                this.InitScaling(specified);
                if (this.ParentInternal != null)
                {
                    CommonProperties.xClearPreferredSizeCache(this.ParentInternal);
                    this.ParentInternal.LayoutEngine.InitLayout(this, specified);
                    this.ParentInternal.ResumeLayout(true);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void SetClientSizeCore(int x, int y)
        {
            this.Size = this.SizeFromClientSize(x, y);
            this.clientWidth = x;
            this.clientHeight = y;
            this.OnClientSizeChanged(EventArgs.Empty);
        }

        private void SetHandle(IntPtr value)
        {
            if (value == IntPtr.Zero)
            {
                this.SetState(1, false);
            }
            this.UpdateRoot();
        }

        private void SetParentHandle(IntPtr value)
        {
            if (this.IsHandleCreated)
            {
                IntPtr parent = System.Windows.Forms.UnsafeNativeMethods.GetParent(new HandleRef(this.window, this.Handle));
                bool topLevel = this.GetTopLevel();
                if ((parent != value) || ((parent == IntPtr.Zero) && !topLevel))
                {
                    bool flag2 = ((parent == IntPtr.Zero) && !topLevel) || ((value == IntPtr.Zero) && topLevel);
                    if (flag2)
                    {
                        Form form = this as Form;
                        if ((form != null) && !form.CanRecreateHandle())
                        {
                            flag2 = false;
                            this.UpdateStyles();
                        }
                    }
                    if (flag2)
                    {
                        this.RecreateHandle();
                    }
                    if (!this.GetTopLevel())
                    {
                        if (value == IntPtr.Zero)
                        {
                            Application.ParkHandle(new HandleRef(this.window, this.Handle));
                            this.UpdateRoot();
                        }
                        else
                        {
                            System.Windows.Forms.UnsafeNativeMethods.SetParent(new HandleRef(this.window, this.Handle), new HandleRef(null, value));
                            if (this.parent != null)
                            {
                                this.parent.UpdateChildZOrder(this);
                            }
                            Application.UnparkHandle(new HandleRef(this.window, this.Handle));
                        }
                    }
                }
                else if (((value == IntPtr.Zero) && (parent == IntPtr.Zero)) && topLevel)
                {
                    System.Windows.Forms.UnsafeNativeMethods.SetParent(new HandleRef(this.window, this.Handle), new HandleRef(null, IntPtr.Zero));
                    Application.UnparkHandle(new HandleRef(this.window, this.Handle));
                }
            }
        }

        internal void SetState(int flag, bool value)
        {
            this.state = value ? (this.state | flag) : (this.state & ~flag);
        }

        internal void SetState2(int flag, bool value)
        {
            this.state2 = value ? (this.state2 | flag) : (this.state2 & ~flag);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void SetStyle(ControlStyles flag, bool value)
        {
            if (((flag & ControlStyles.EnableNotifyMessage) != 0) && value)
            {
                System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
            }
            this.controlStyle = value ? (this.controlStyle | flag) : (this.controlStyle & ~flag);
        }

        protected void SetTopLevel(bool value)
        {
            if (value && this.IsActiveX)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("TopLevelNotAllowedIfActiveX"));
            }
            if (value)
            {
                if (this is Form)
                {
                    System.Windows.Forms.IntSecurity.TopLevelWindow.Demand();
                }
                else
                {
                    System.Windows.Forms.IntSecurity.UnrestrictedWindows.Demand();
                }
            }
            this.SetTopLevelInternal(value);
        }

        internal void SetTopLevelInternal(bool value)
        {
            if (this.GetTopLevel() != value)
            {
                if (this.parent != null)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("TopLevelParentedControl"), "value");
                }
                this.SetState(0x80000, value);
                if (this.IsHandleCreated && this.GetState2(8))
                {
                    this.ListenToUserPreferenceChanged(value);
                }
                this.UpdateStyles();
                this.SetParentHandle(IntPtr.Zero);
                if (value && this.Visible)
                {
                    this.CreateControl();
                }
                this.UpdateRoot();
            }
        }

        internal static IntPtr SetUpPalette(IntPtr dc, bool force, bool realizePalette)
        {
            IntPtr halftonePalette = Graphics.GetHalftonePalette();
            IntPtr ptr2 = System.Windows.Forms.SafeNativeMethods.SelectPalette(new HandleRef(null, dc), new HandleRef(null, halftonePalette), force ? 0 : 1);
            if ((ptr2 != IntPtr.Zero) && realizePalette)
            {
                System.Windows.Forms.SafeNativeMethods.RealizePalette(new HandleRef(null, dc));
            }
            return ptr2;
        }

        protected virtual void SetVisibleCore(bool value)
        {
            try
            {
                System.Internal.HandleCollector.SuspendCollect();
                if (this.GetVisibleCore() != value)
                {
                    if (!value)
                    {
                        this.SelectNextIfFocused();
                    }
                    bool flag = false;
                    if (this.GetTopLevel())
                    {
                        if (this.IsHandleCreated || value)
                        {
                            System.Windows.Forms.SafeNativeMethods.ShowWindow(new HandleRef(this, this.Handle), value ? this.ShowParams : 0);
                        }
                    }
                    else if (this.IsHandleCreated || ((value && (this.parent != null)) && this.parent.Created))
                    {
                        this.SetState(2, value);
                        flag = true;
                        try
                        {
                            if (value)
                            {
                                this.CreateControl();
                            }
                            System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this.window, this.Handle), System.Windows.Forms.NativeMethods.NullHandleRef, 0, 0, 0, 0, 0x17 | (value ? 0x40 : 0x80));
                        }
                        catch
                        {
                            this.SetState(2, !value);
                            throw;
                        }
                    }
                    if (this.GetVisibleCore() != value)
                    {
                        this.SetState(2, value);
                        flag = true;
                    }
                    if (flag)
                    {
                        using (new LayoutTransaction(this.parent, this, PropertyNames.Visible))
                        {
                            this.OnVisibleChanged(EventArgs.Empty);
                        }
                    }
                    this.UpdateRoot();
                }
                else if ((this.GetState(2) || value) || (!this.IsHandleCreated || System.Windows.Forms.SafeNativeMethods.IsWindowVisible(new HandleRef(this, this.Handle))))
                {
                    this.SetState(2, value);
                    if (this.IsHandleCreated)
                    {
                        System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this.window, this.Handle), System.Windows.Forms.NativeMethods.NullHandleRef, 0, 0, 0, 0, 0x17 | (value ? 0x40 : 0x80));
                    }
                }
            }
            finally
            {
                System.Internal.HandleCollector.ResumeCollect();
            }
        }

        private void SetWindowFont()
        {
            this.SendMessage(0x30, this.FontHandle, 0);
        }

        private void SetWindowStyle(int flag, bool value)
        {
            int windowLong = (int) ((long) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, this.Handle), -16));
            System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, this.Handle), -16, new HandleRef(null, value ? ((IntPtr) (windowLong | flag)) : ((IntPtr) (windowLong & ~flag))));
        }

        internal virtual bool ShouldPerformContainerValidation()
        {
            return this.GetStyle(ControlStyles.ContainerControl);
        }

        private bool ShouldSerializeAccessibleName()
        {
            string accessibleName = this.AccessibleName;
            return ((accessibleName != null) && (accessibleName.Length > 0));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeBackColor()
        {
            return !this.Properties.GetColor(PropBackColor).IsEmpty;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeCursor()
        {
            bool flag;
            object obj2 = this.Properties.GetObject(PropCursor, out flag);
            return (flag && (obj2 != null));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private bool ShouldSerializeEnabled()
        {
            return !this.GetState(4);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeFont()
        {
            bool flag;
            object obj2 = this.Properties.GetObject(PropFont, out flag);
            return (flag && (obj2 != null));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeForeColor()
        {
            return !this.Properties.GetColor(PropForeColor).IsEmpty;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeImeMode()
        {
            bool flag;
            int integer = this.Properties.GetInteger(PropImeMode, out flag);
            return (flag && (integer != this.DefaultImeMode));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal bool ShouldSerializeMargin()
        {
            return !this.Margin.Equals(this.DefaultMargin);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeMaximumSize()
        {
            return (this.MaximumSize != this.DefaultMaximumSize);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeMinimumSize()
        {
            return (this.MinimumSize != this.DefaultMinimumSize);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal bool ShouldSerializePadding()
        {
            return !this.Padding.Equals(this.DefaultPadding);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeRightToLeft()
        {
            bool flag;
            int integer = this.Properties.GetInteger(PropRightToLeft, out flag);
            return (flag && (integer != 2));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeSize()
        {
            System.Drawing.Size defaultSize = this.DefaultSize;
            if (this.width == defaultSize.Width)
            {
                return (this.height != defaultSize.Height);
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeText()
        {
            return (this.Text.Length != 0);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private bool ShouldSerializeVisible()
        {
            return !this.GetState(2);
        }

        public void Show()
        {
            this.Visible = true;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual System.Drawing.Size SizeFromClientSize(System.Drawing.Size clientSize)
        {
            return this.SizeFromClientSize(clientSize.Width, clientSize.Height);
        }

        internal System.Drawing.Size SizeFromClientSize(int width, int height)
        {
            System.Windows.Forms.NativeMethods.RECT lpRect = new System.Windows.Forms.NativeMethods.RECT(0, 0, width, height);
            System.Windows.Forms.CreateParams createParams = this.CreateParams;
            System.Windows.Forms.SafeNativeMethods.AdjustWindowRectEx(ref lpRect, createParams.Style, this.HasMenu, createParams.ExStyle);
            return lpRect.Size;
        }

        public void SuspendLayout()
        {
            this.layoutSuspendCount = (byte) (this.layoutSuspendCount + 1);
            if (this.layoutSuspendCount == 1)
            {
                this.OnLayoutSuspended();
            }
        }

        void IDropTarget.OnDragDrop(DragEventArgs drgEvent)
        {
            this.OnDragDrop(drgEvent);
        }

        void IDropTarget.OnDragEnter(DragEventArgs drgEvent)
        {
            this.OnDragEnter(drgEvent);
        }

        void IDropTarget.OnDragLeave(EventArgs e)
        {
            this.OnDragLeave(e);
        }

        void IDropTarget.OnDragOver(DragEventArgs drgEvent)
        {
            this.OnDragOver(drgEvent);
        }

        void ISupportOleDropSource.OnGiveFeedback(GiveFeedbackEventArgs giveFeedbackEventArgs)
        {
            this.OnGiveFeedback(giveFeedbackEventArgs);
        }

        void ISupportOleDropSource.OnQueryContinueDrag(QueryContinueDragEventArgs queryContinueDragEventArgs)
        {
            this.OnQueryContinueDrag(queryContinueDragEventArgs);
        }

        void IArrangedElement.PerformLayout(IArrangedElement affectedElement, string affectedProperty)
        {
            this.PerformLayout(new LayoutEventArgs(affectedElement, affectedProperty));
        }

        void IArrangedElement.SetBounds(Rectangle bounds, BoundsSpecified specified)
        {
            ISite site = this.Site;
            IComponentChangeService service = null;
            PropertyDescriptor member = null;
            PropertyDescriptor descriptor2 = null;
            bool flag = false;
            bool flag2 = false;
            if ((site != null) && site.DesignMode)
            {
                service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    member = TypeDescriptor.GetProperties(this)[PropertyNames.Size];
                    descriptor2 = TypeDescriptor.GetProperties(this)[PropertyNames.Location];
                    try
                    {
                        if (((member != null) && !member.IsReadOnly) && ((bounds.Width != this.Width) || (bounds.Height != this.Height)))
                        {
                            if (!(site is INestedSite))
                            {
                                service.OnComponentChanging(this, member);
                            }
                            flag = true;
                        }
                        if (((descriptor2 != null) && !descriptor2.IsReadOnly) && ((bounds.X != this.x) || (bounds.Y != this.y)))
                        {
                            if (!(site is INestedSite))
                            {
                                service.OnComponentChanging(this, descriptor2);
                            }
                            flag2 = true;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }
            this.SetBoundsCore(bounds.X, bounds.Y, bounds.Width, bounds.Height, specified);
            if ((site != null) && (service != null))
            {
                try
                {
                    if (flag)
                    {
                        service.OnComponentChanged(this, member, null, null);
                    }
                    if (flag2)
                    {
                        service.OnComponentChanged(this, descriptor2, null, null);
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleControl.FreezeEvents(int bFreeze)
        {
            this.ActiveXInstance.EventsFrozen = bFreeze != 0;
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleControl.GetControlInfo(System.Windows.Forms.NativeMethods.tagCONTROLINFO pCI)
        {
            pCI.cb = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.tagCONTROLINFO));
            pCI.hAccel = IntPtr.Zero;
            pCI.cAccel = 0;
            pCI.dwFlags = 0;
            if (this.IsInputKey(Keys.Enter))
            {
                pCI.dwFlags |= 1;
            }
            if (this.IsInputKey(Keys.Escape))
            {
                pCI.dwFlags |= 2;
            }
            this.ActiveXInstance.GetControlInfo(pCI);
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleControl.OnAmbientPropertyChange(int dispID)
        {
            this.ActiveXInstance.OnAmbientPropertyChange(dispID);
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleControl.OnMnemonic(ref System.Windows.Forms.NativeMethods.MSG pMsg)
        {
            this.ProcessMnemonic((char) ((int) pMsg.wParam));
            return 0;
        }

        void System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject.ContextSensitiveHelp(int fEnterMode)
        {
            ((System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject) this).ContextSensitiveHelp(fEnterMode);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject.EnableModeless(int fEnable)
        {
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject.GetWindow(out IntPtr hwnd)
        {
            return ((System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject) this).GetWindow(out hwnd);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject.OnDocWindowActivate(int fActivate)
        {
            this.ActiveXInstance.OnDocWindowActivate(fActivate);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject.OnFrameWindowActivate(bool fActivate)
        {
            this.OnFrameWindowActivate(fActivate);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject.ResizeBorder(System.Windows.Forms.NativeMethods.COMRECT prcBorder, System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceUIWindow pUIWindow, bool fFrameWindow)
        {
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject.TranslateAccelerator(ref System.Windows.Forms.NativeMethods.MSG lpmsg)
        {
            return this.ActiveXInstance.TranslateAccelerator(ref lpmsg);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject.ContextSensitiveHelp(int fEnterMode)
        {
            if (fEnterMode != 0)
            {
                this.OnHelpRequested(new HelpEventArgs(MousePosition));
            }
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject.GetWindow(out IntPtr hwnd)
        {
            return this.ActiveXInstance.GetWindow(out hwnd);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject.InPlaceDeactivate()
        {
            this.ActiveXInstance.InPlaceDeactivate();
        }

        void System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject.ReactivateAndUndo()
        {
        }

        void System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject.SetObjectRects(System.Windows.Forms.NativeMethods.COMRECT lprcPosRect, System.Windows.Forms.NativeMethods.COMRECT lprcClipRect)
        {
            this.ActiveXInstance.SetObjectRects(lprcPosRect, lprcClipRect);
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject.UIDeactivate()
        {
            return this.ActiveXInstance.UIDeactivate();
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.Advise(IAdviseSink pAdvSink, out int cookie)
        {
            cookie = this.ActiveXInstance.Advise(pAdvSink);
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.Close(int dwSaveOption)
        {
            this.ActiveXInstance.Close(dwSaveOption);
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.DoVerb(int iVerb, IntPtr lpmsg, System.Windows.Forms.UnsafeNativeMethods.IOleClientSite pActiveSite, int lindex, IntPtr hwndParent, System.Windows.Forms.NativeMethods.COMRECT lprcPosRect)
        {
            short num = (short) iVerb;
            iVerb = num;
            try
            {
                this.ActiveXInstance.DoVerb(iVerb, lpmsg, pActiveSite, lindex, hwndParent, lprcPosRect);
            }
            catch (Exception)
            {
                throw;
            }
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.EnumAdvise(out IEnumSTATDATA e)
        {
            e = null;
            return -2147467263;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.EnumVerbs(out System.Windows.Forms.UnsafeNativeMethods.IEnumOLEVERB e)
        {
            return ActiveXImpl.EnumVerbs(out e);
        }

        System.Windows.Forms.UnsafeNativeMethods.IOleClientSite System.Windows.Forms.UnsafeNativeMethods.IOleObject.GetClientSite()
        {
            return this.ActiveXInstance.GetClientSite();
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.GetClipboardData(int dwReserved, out System.Runtime.InteropServices.ComTypes.IDataObject data)
        {
            data = null;
            return -2147467263;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.GetExtent(int dwDrawAspect, System.Windows.Forms.NativeMethods.tagSIZEL pSizel)
        {
            this.ActiveXInstance.GetExtent(dwDrawAspect, pSizel);
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.GetMiscStatus(int dwAspect, out int cookie)
        {
            if ((dwAspect & 1) != 0)
            {
                int num = 0x20180;
                if (this.GetStyle(ControlStyles.ResizeRedraw))
                {
                    num |= 1;
                }
                if (this is IButtonControl)
                {
                    num |= 0x1000;
                }
                cookie = num;
                return 0;
            }
            cookie = 0;
            return -2147221397;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.GetMoniker(int dwAssign, int dwWhichMoniker, out object moniker)
        {
            moniker = null;
            return -2147467263;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.GetUserClassID(ref Guid pClsid)
        {
            pClsid = base.GetType().GUID;
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.GetUserType(int dwFormOfType, out string userType)
        {
            if (dwFormOfType == 1)
            {
                userType = base.GetType().FullName;
            }
            else
            {
                userType = base.GetType().Name;
            }
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.InitFromData(System.Runtime.InteropServices.ComTypes.IDataObject pDataObject, int fCreation, int dwReserved)
        {
            return -2147467263;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.IsUpToDate()
        {
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.OleUpdate()
        {
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.SetClientSite(System.Windows.Forms.UnsafeNativeMethods.IOleClientSite pClientSite)
        {
            this.ActiveXInstance.SetClientSite(pClientSite);
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.SetColorScheme(System.Windows.Forms.NativeMethods.tagLOGPALETTE pLogpal)
        {
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.SetExtent(int dwDrawAspect, System.Windows.Forms.NativeMethods.tagSIZEL pSizel)
        {
            this.ActiveXInstance.SetExtent(dwDrawAspect, pSizel);
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.SetHostNames(string szContainerApp, string szContainerObj)
        {
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.SetMoniker(int dwWhichMoniker, object pmk)
        {
            return -2147467263;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleObject.Unadvise(int dwConnection)
        {
            this.ActiveXInstance.Unadvise(dwConnection);
            return 0;
        }

        void System.Windows.Forms.UnsafeNativeMethods.IOleWindow.ContextSensitiveHelp(int fEnterMode)
        {
            ((System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject) this).ContextSensitiveHelp(fEnterMode);
        }

        int System.Windows.Forms.UnsafeNativeMethods.IOleWindow.GetWindow(out IntPtr hwnd)
        {
            return ((System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceObject) this).GetWindow(out hwnd);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPersist.GetClassID(out Guid pClassID)
        {
            pClassID = base.GetType().GUID;
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPersistPropertyBag.GetClassID(out Guid pClassID)
        {
            pClassID = base.GetType().GUID;
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPersistPropertyBag.InitNew()
        {
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPersistPropertyBag.Load(System.Windows.Forms.UnsafeNativeMethods.IPropertyBag pPropBag, System.Windows.Forms.UnsafeNativeMethods.IErrorLog pErrorLog)
        {
            this.ActiveXInstance.Load(pPropBag, pErrorLog);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPersistPropertyBag.Save(System.Windows.Forms.UnsafeNativeMethods.IPropertyBag pPropBag, bool fClearDirty, bool fSaveAllProperties)
        {
            this.ActiveXInstance.Save(pPropBag, fClearDirty, fSaveAllProperties);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPersistStorage.GetClassID(out Guid pClassID)
        {
            pClassID = base.GetType().GUID;
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPersistStorage.HandsOffStorage()
        {
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPersistStorage.InitNew(System.Windows.Forms.UnsafeNativeMethods.IStorage pstg)
        {
        }

        int System.Windows.Forms.UnsafeNativeMethods.IPersistStorage.IsDirty()
        {
            return this.ActiveXInstance.IsDirty();
        }

        int System.Windows.Forms.UnsafeNativeMethods.IPersistStorage.Load(System.Windows.Forms.UnsafeNativeMethods.IStorage pstg)
        {
            this.ActiveXInstance.Load(pstg);
            return 0;
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPersistStorage.Save(System.Windows.Forms.UnsafeNativeMethods.IStorage pstg, bool fSameAsLoad)
        {
            this.ActiveXInstance.Save(pstg, fSameAsLoad);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPersistStorage.SaveCompleted(System.Windows.Forms.UnsafeNativeMethods.IStorage pStgNew)
        {
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit.GetClassID(out Guid pClassID)
        {
            pClassID = base.GetType().GUID;
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit.GetSizeMax(long pcbSize)
        {
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit.InitNew()
        {
        }

        int System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit.IsDirty()
        {
            return this.ActiveXInstance.IsDirty();
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit.Load(System.Windows.Forms.UnsafeNativeMethods.IStream pstm)
        {
            this.ActiveXInstance.Load(pstm);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPersistStreamInit.Save(System.Windows.Forms.UnsafeNativeMethods.IStream pstm, bool fClearDirty)
        {
            this.ActiveXInstance.Save(pstm, fClearDirty);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IQuickActivate.GetContentExtent(System.Windows.Forms.NativeMethods.tagSIZEL pSizel)
        {
            this.ActiveXInstance.GetExtent(1, pSizel);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IQuickActivate.QuickActivate(System.Windows.Forms.UnsafeNativeMethods.tagQACONTAINER pQaContainer, System.Windows.Forms.UnsafeNativeMethods.tagQACONTROL pQaControl)
        {
            this.ActiveXInstance.QuickActivate(pQaContainer, pQaControl);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IQuickActivate.SetContentExtent(System.Windows.Forms.NativeMethods.tagSIZEL pSizel)
        {
            this.ActiveXInstance.SetExtent(1, pSizel);
        }

        int System.Windows.Forms.UnsafeNativeMethods.IViewObject.Draw(int dwDrawAspect, int lindex, IntPtr pvAspect, System.Windows.Forms.NativeMethods.tagDVTARGETDEVICE ptd, IntPtr hdcTargetDev, IntPtr hdcDraw, System.Windows.Forms.NativeMethods.COMRECT lprcBounds, System.Windows.Forms.NativeMethods.COMRECT lprcWBounds, IntPtr pfnContinue, int dwContinue)
        {
            try
            {
                this.ActiveXInstance.Draw(dwDrawAspect, lindex, pvAspect, ptd, hdcTargetDev, hdcDraw, lprcBounds, lprcWBounds, pfnContinue, dwContinue);
            }
            catch (ExternalException exception)
            {
                return exception.ErrorCode;
            }
            return 0;
        }

        int System.Windows.Forms.UnsafeNativeMethods.IViewObject.Freeze(int dwDrawAspect, int lindex, IntPtr pvAspect, IntPtr pdwFreeze)
        {
            return -2147467263;
        }

        void System.Windows.Forms.UnsafeNativeMethods.IViewObject.GetAdvise(int[] paspects, int[] padvf, IAdviseSink[] pAdvSink)
        {
            this.ActiveXInstance.GetAdvise(paspects, padvf, pAdvSink);
        }

        int System.Windows.Forms.UnsafeNativeMethods.IViewObject.GetColorSet(int dwDrawAspect, int lindex, IntPtr pvAspect, System.Windows.Forms.NativeMethods.tagDVTARGETDEVICE ptd, IntPtr hicTargetDev, System.Windows.Forms.NativeMethods.tagLOGPALETTE ppColorSet)
        {
            return -2147467263;
        }

        void System.Windows.Forms.UnsafeNativeMethods.IViewObject.SetAdvise(int aspects, int advf, IAdviseSink pAdvSink)
        {
            this.ActiveXInstance.SetAdvise(aspects, advf, pAdvSink);
        }

        int System.Windows.Forms.UnsafeNativeMethods.IViewObject.Unfreeze(int dwFreeze)
        {
            return -2147467263;
        }

        void System.Windows.Forms.UnsafeNativeMethods.IViewObject2.Draw(int dwDrawAspect, int lindex, IntPtr pvAspect, System.Windows.Forms.NativeMethods.tagDVTARGETDEVICE ptd, IntPtr hdcTargetDev, IntPtr hdcDraw, System.Windows.Forms.NativeMethods.COMRECT lprcBounds, System.Windows.Forms.NativeMethods.COMRECT lprcWBounds, IntPtr pfnContinue, int dwContinue)
        {
            this.ActiveXInstance.Draw(dwDrawAspect, lindex, pvAspect, ptd, hdcTargetDev, hdcDraw, lprcBounds, lprcWBounds, pfnContinue, dwContinue);
        }

        int System.Windows.Forms.UnsafeNativeMethods.IViewObject2.Freeze(int dwDrawAspect, int lindex, IntPtr pvAspect, IntPtr pdwFreeze)
        {
            return -2147467263;
        }

        void System.Windows.Forms.UnsafeNativeMethods.IViewObject2.GetAdvise(int[] paspects, int[] padvf, IAdviseSink[] pAdvSink)
        {
            this.ActiveXInstance.GetAdvise(paspects, padvf, pAdvSink);
        }

        int System.Windows.Forms.UnsafeNativeMethods.IViewObject2.GetColorSet(int dwDrawAspect, int lindex, IntPtr pvAspect, System.Windows.Forms.NativeMethods.tagDVTARGETDEVICE ptd, IntPtr hicTargetDev, System.Windows.Forms.NativeMethods.tagLOGPALETTE ppColorSet)
        {
            return -2147467263;
        }

        void System.Windows.Forms.UnsafeNativeMethods.IViewObject2.GetExtent(int dwDrawAspect, int lindex, System.Windows.Forms.NativeMethods.tagDVTARGETDEVICE ptd, System.Windows.Forms.NativeMethods.tagSIZEL lpsizel)
        {
            ((System.Windows.Forms.UnsafeNativeMethods.IOleObject) this).GetExtent(dwDrawAspect, lpsizel);
        }

        void System.Windows.Forms.UnsafeNativeMethods.IViewObject2.SetAdvise(int aspects, int advf, IAdviseSink pAdvSink)
        {
            this.ActiveXInstance.SetAdvise(aspects, advf, pAdvSink);
        }

        int System.Windows.Forms.UnsafeNativeMethods.IViewObject2.Unfreeze(int dwFreeze)
        {
            return -2147467263;
        }

        private void UnhookMouseEvent()
        {
            this.SetState(0x4000, false);
        }

        public void Update()
        {
            System.Windows.Forms.SafeNativeMethods.UpdateWindow(new HandleRef(this.window, this.InternalHandle));
        }

        private void UpdateBindings()
        {
            for (int i = 0; i < this.DataBindings.Count; i++)
            {
                System.Windows.Forms.BindingContext.UpdateBinding(this.BindingContext, this.DataBindings[i]);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal void UpdateBounds()
        {
            System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
            System.Windows.Forms.UnsafeNativeMethods.GetClientRect(new HandleRef(this.window, this.InternalHandle), ref rect);
            int right = rect.right;
            int bottom = rect.bottom;
            System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(this.window, this.InternalHandle), ref rect);
            if (!this.GetTopLevel())
            {
                System.Windows.Forms.UnsafeNativeMethods.MapWindowPoints(System.Windows.Forms.NativeMethods.NullHandleRef, new HandleRef(null, System.Windows.Forms.UnsafeNativeMethods.GetParent(new HandleRef(this.window, this.InternalHandle))), ref rect, 2);
            }
            this.UpdateBounds(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top, right, bottom);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void UpdateBounds(int x, int y, int width, int height)
        {
            System.Windows.Forms.NativeMethods.RECT rect;
            rect = new System.Windows.Forms.NativeMethods.RECT {
                left = rect.right = rect.top = rect.bottom = 0
            };
            System.Windows.Forms.CreateParams createParams = this.CreateParams;
            System.Windows.Forms.SafeNativeMethods.AdjustWindowRectEx(ref rect, createParams.Style, false, createParams.ExStyle);
            int clientWidth = width - (rect.right - rect.left);
            int clientHeight = height - (rect.bottom - rect.top);
            this.UpdateBounds(x, y, width, height, clientWidth, clientHeight);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void UpdateBounds(int x, int y, int width, int height, int clientWidth, int clientHeight)
        {
            bool flag = (this.x != x) || (this.y != y);
            bool flag2 = (((this.Width != width) || (this.Height != height)) || (this.clientWidth != clientWidth)) || (this.clientHeight != clientHeight);
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.clientWidth = clientWidth;
            this.clientHeight = clientHeight;
            if (flag)
            {
                this.OnLocationChanged(EventArgs.Empty);
            }
            if (flag2)
            {
                this.OnSizeChanged(EventArgs.Empty);
                this.OnClientSizeChanged(EventArgs.Empty);
                CommonProperties.xClearPreferredSizeCache(this);
                LayoutTransaction.DoLayout(this.ParentInternal, this, PropertyNames.Bounds);
            }
        }

        private void UpdateChildControlIndex(Control ctl)
        {
            int newIndex = 0;
            int childIndex = this.Controls.GetChildIndex(ctl);
            IntPtr internalHandle = ctl.InternalHandle;
            while ((internalHandle = System.Windows.Forms.UnsafeNativeMethods.GetWindow(new HandleRef(null, internalHandle), 3)) != IntPtr.Zero)
            {
                Control child = FromHandleInternal(internalHandle);
                if (child != null)
                {
                    newIndex = this.Controls.GetChildIndex(child, false) + 1;
                    break;
                }
            }
            if (newIndex > childIndex)
            {
                newIndex--;
            }
            if (newIndex != childIndex)
            {
                this.Controls.SetChildIndex(ctl, newIndex);
            }
        }

        private void UpdateChildZOrder(Control ctl)
        {
            if ((this.IsHandleCreated && ctl.IsHandleCreated) && (ctl.parent == this))
            {
                IntPtr handle = (IntPtr) System.Windows.Forms.NativeMethods.HWND_TOP;
                int childIndex = this.Controls.GetChildIndex(ctl);
                while (--childIndex >= 0)
                {
                    Control control = this.Controls[childIndex];
                    if (control.IsHandleCreated && (control.parent == this))
                    {
                        handle = control.Handle;
                        break;
                    }
                }
                if (System.Windows.Forms.UnsafeNativeMethods.GetWindow(new HandleRef(ctl.window, ctl.Handle), 3) != handle)
                {
                    this.state |= 0x100;
                    try
                    {
                        System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(ctl.window, ctl.Handle), new HandleRef(null, handle), 0, 0, 0, 0, 3);
                    }
                    finally
                    {
                        this.state &= -257;
                    }
                }
            }
        }

        internal void UpdateImeContextMode()
        {
            System.Windows.Forms.ImeMode[] inputLanguageTable = ImeModeConversion.InputLanguageTable;
            if ((!base.DesignMode && (inputLanguageTable != ImeModeConversion.UnsupportedTable)) && this.Focused)
            {
                System.Windows.Forms.ImeMode disable = System.Windows.Forms.ImeMode.Disable;
                System.Windows.Forms.ImeMode cachedImeMode = this.CachedImeMode;
                if (this.ImeSupported && this.CanEnableIme)
                {
                    disable = (cachedImeMode == System.Windows.Forms.ImeMode.NoControl) ? PropagatingImeMode : cachedImeMode;
                }
                if ((this.CurrentImeContextMode != disable) && (disable != System.Windows.Forms.ImeMode.Inherit))
                {
                    this.DisableImeModeChangedCount++;
                    System.Windows.Forms.ImeMode propagatingImeMode = PropagatingImeMode;
                    try
                    {
                        ImeContext.SetImeStatus(disable, this.Handle);
                    }
                    finally
                    {
                        this.DisableImeModeChangedCount--;
                        if ((disable == System.Windows.Forms.ImeMode.Disable) && (inputLanguageTable == ImeModeConversion.ChineseTable))
                        {
                            PropagatingImeMode = propagatingImeMode;
                        }
                    }
                    if (cachedImeMode == System.Windows.Forms.ImeMode.NoControl)
                    {
                        if (this.CanEnableIme)
                        {
                            PropagatingImeMode = this.CurrentImeContextMode;
                        }
                    }
                    else
                    {
                        if (this.CanEnableIme)
                        {
                            this.CachedImeMode = this.CurrentImeContextMode;
                        }
                        this.VerifyImeModeChanged(disable, this.CachedImeMode);
                    }
                }
            }
        }

        private void UpdateReflectParent(bool findNewParent)
        {
            if ((!this.Disposing && findNewParent) && this.IsHandleCreated)
            {
                IntPtr parent = System.Windows.Forms.UnsafeNativeMethods.GetParent(new HandleRef(this, this.Handle));
                if (parent != IntPtr.Zero)
                {
                    this.ReflectParent = FromHandleInternal(parent);
                    return;
                }
            }
            this.ReflectParent = null;
        }

        private void UpdateRoot()
        {
            this.window.LockReference(this.GetTopLevel() && this.Visible);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void UpdateStyles()
        {
            this.UpdateStylesCore();
            this.OnStyleChanged(EventArgs.Empty);
        }

        internal virtual void UpdateStylesCore()
        {
            if (this.IsHandleCreated)
            {
                System.Windows.Forms.CreateParams createParams = this.CreateParams;
                int windowStyle = this.WindowStyle;
                int windowExStyle = this.WindowExStyle;
                if ((this.state & 2) != 0)
                {
                    createParams.Style |= 0x10000000;
                }
                if (windowStyle != createParams.Style)
                {
                    this.WindowStyle = createParams.Style;
                }
                if (windowExStyle != createParams.ExStyle)
                {
                    this.WindowExStyle = createParams.ExStyle;
                    this.SetState(0x40000000, (createParams.ExStyle & 0x400000) != 0);
                }
                System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.NullHandleRef, 0, 0, 0, 0, 0x37);
                this.Invalidate(true);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void UpdateZOrder()
        {
            if (this.parent != null)
            {
                this.parent.UpdateChildZOrder(this);
            }
        }

        private void UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs pref)
        {
            if (pref.Category == UserPreferenceCategory.Color)
            {
                defaultFont = null;
                this.OnSystemColorsChanged(EventArgs.Empty);
            }
        }

        internal bool ValidateActiveControl(out bool validatedControlAllowsFocusChange)
        {
            bool flag = true;
            validatedControlAllowsFocusChange = false;
            IContainerControl containerControlInternal = this.GetContainerControlInternal();
            if (containerControlInternal == null)
            {
                return flag;
            }
            ContainerControl control2 = containerControlInternal as ContainerControl;
            if (control2 == null)
            {
                return flag;
            }
            while (control2.ActiveControl == null)
            {
                Control parentInternal = control2.ParentInternal;
                if (parentInternal == null)
                {
                    break;
                }
                ContainerControl control3 = parentInternal.GetContainerControlInternal() as ContainerControl;
                if (control3 == null)
                {
                    break;
                }
                control2 = control3;
            }
            return control2.ValidateInternal(true, out validatedControlAllowsFocusChange);
        }

        private void VerifyImeModeChanged(System.Windows.Forms.ImeMode oldMode, System.Windows.Forms.ImeMode newMode)
        {
            if ((this.ImeSupported && (this.DisableImeModeChangedCount == 0)) && ((newMode != System.Windows.Forms.ImeMode.NoControl) && (oldMode != newMode)))
            {
                this.OnImeModeChanged(EventArgs.Empty);
            }
        }

        internal void VerifyImeRestrictedModeChanged()
        {
            bool canEnableIme = this.CanEnableIme;
            if (this.LastCanEnableIme != canEnableIme)
            {
                if (this.Focused)
                {
                    this.DisableImeModeChangedCount++;
                    try
                    {
                        this.UpdateImeContextMode();
                    }
                    finally
                    {
                        this.DisableImeModeChangedCount--;
                    }
                }
                System.Windows.Forms.ImeMode cachedImeMode = this.CachedImeMode;
                System.Windows.Forms.ImeMode disable = System.Windows.Forms.ImeMode.Disable;
                if (canEnableIme)
                {
                    disable = cachedImeMode;
                    cachedImeMode = System.Windows.Forms.ImeMode.Disable;
                }
                this.VerifyImeModeChanged(cachedImeMode, disable);
                this.LastCanEnableIme = canEnableIme;
            }
        }

        private void WaitForWaitHandle(WaitHandle waitHandle)
        {
            Application.ThreadContext context = Application.ThreadContext.FromId(this.CreateThreadId);
            if (context != null)
            {
                IntPtr handle = context.GetHandle();
                bool flag = false;
                uint lpExitCode = 0;
                while (!flag)
                {
                    if ((System.Windows.Forms.UnsafeNativeMethods.GetExitCodeThread(handle, out lpExitCode) && (lpExitCode != 0x103)) || AppDomain.CurrentDomain.IsFinalizingForUnload())
                    {
                        if (!waitHandle.WaitOne(1, false))
                        {
                            throw new InvalidAsynchronousStateException(System.Windows.Forms.SR.GetString("ThreadNoLongerValid"));
                        }
                        return;
                    }
                    if (this.IsDisposed)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ErrorNoMarshalingThread"));
                    }
                    flag = waitHandle.WaitOne(0x3e8, false);
                }
            }
        }

        internal void WindowAssignHandle(IntPtr handle, bool value)
        {
            this.window.AssignHandle(handle, value);
        }

        internal void WindowReleaseHandle()
        {
            this.window.ReleaseHandle();
        }

        private void WmCaptureChanged(ref Message m)
        {
            this.OnMouseCaptureChanged(EventArgs.Empty);
            this.DefWndProc(ref m);
        }

        private void WmClose(ref Message m)
        {
            if (this.ParentInternal != null)
            {
                IntPtr handle = this.Handle;
                IntPtr ptr2 = handle;
                while (handle != IntPtr.Zero)
                {
                    ptr2 = handle;
                    handle = System.Windows.Forms.UnsafeNativeMethods.GetParent(new HandleRef(null, handle));
                    int windowLong = (int) ((long) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(null, ptr2), -16));
                    if ((windowLong & 0x40000000) == 0)
                    {
                        break;
                    }
                }
                if (ptr2 != IntPtr.Zero)
                {
                    System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(null, ptr2), 0x10, IntPtr.Zero, IntPtr.Zero);
                }
            }
            this.DefWndProc(ref m);
        }

        private void WmCommand(ref Message m)
        {
            if (IntPtr.Zero == m.LParam)
            {
                if (Command.DispatchID(System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam)))
                {
                    return;
                }
            }
            else if (ReflectMessageInternal(m.LParam, ref m))
            {
                return;
            }
            this.DefWndProc(ref m);
        }

        internal virtual void WmContextMenu(ref Message m)
        {
            this.WmContextMenu(ref m, this);
        }

        internal void WmContextMenu(ref Message m, Control sourceControl)
        {
            System.Windows.Forms.ContextMenu menu = this.Properties.GetObject(PropContextMenu) as System.Windows.Forms.ContextMenu;
            System.Windows.Forms.ContextMenuStrip strip = (menu != null) ? null : (this.Properties.GetObject(PropContextMenuStrip) as System.Windows.Forms.ContextMenuStrip);
            if ((menu != null) || (strip != null))
            {
                Point point;
                int x = System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam);
                int y = System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam);
                bool isKeyboardActivated = false;
                if (((int) ((long) m.LParam)) == -1)
                {
                    isKeyboardActivated = true;
                    point = new Point(this.Width / 2, this.Height / 2);
                }
                else
                {
                    point = this.PointToClientInternal(new Point(x, y));
                }
                if (this.ClientRectangle.Contains(point))
                {
                    if (menu != null)
                    {
                        menu.Show(sourceControl, point);
                    }
                    else if (strip != null)
                    {
                        strip.ShowInternal(sourceControl, point, isKeyboardActivated);
                    }
                    else
                    {
                        this.DefWndProc(ref m);
                    }
                }
                else
                {
                    this.DefWndProc(ref m);
                }
            }
            else
            {
                this.DefWndProc(ref m);
            }
        }

        private void WmCreate(ref Message m)
        {
            this.DefWndProc(ref m);
            if (this.parent != null)
            {
                this.parent.UpdateChildZOrder(this);
            }
            this.UpdateBounds();
            this.OnHandleCreated(EventArgs.Empty);
            if (!this.GetStyle(ControlStyles.CacheText))
            {
                this.text = null;
            }
        }

        private void WmCtlColorControl(ref Message m)
        {
            Control control = FromHandleInternal(m.LParam);
            if (control != null)
            {
                m.Result = control.InitializeDCForWmCtlColor(m.WParam, m.Msg);
                if (m.Result != IntPtr.Zero)
                {
                    return;
                }
            }
            this.DefWndProc(ref m);
        }

        private void WmDestroy(ref Message m)
        {
            if ((!this.RecreatingHandle && !this.Disposing) && (!this.IsDisposed && this.GetState(0x4000)))
            {
                this.OnMouseLeave(EventArgs.Empty);
                this.UnhookMouseEvent();
            }
            this.OnHandleDestroyed(EventArgs.Empty);
            if (!this.Disposing)
            {
                if (!this.RecreatingHandle)
                {
                    this.SetState(1, false);
                }
            }
            else
            {
                this.SetState(2, false);
            }
            this.DefWndProc(ref m);
        }

        private void WmDisplayChange(ref Message m)
        {
            BufferedGraphicsManager.Current.Invalidate();
            this.DefWndProc(ref m);
        }

        private void WmDrawItem(ref Message m)
        {
            if (m.WParam == IntPtr.Zero)
            {
                this.WmDrawItemMenuItem(ref m);
            }
            else
            {
                this.WmOwnerDraw(ref m);
            }
        }

        private void WmDrawItemMenuItem(ref Message m)
        {
            System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT lParam = (System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT));
            MenuItem menuItemFromItemData = MenuItem.GetMenuItemFromItemData(lParam.itemData);
            if (menuItemFromItemData != null)
            {
                menuItemFromItemData.WmDrawItem(ref m);
            }
        }

        private void WmEraseBkgnd(ref Message m)
        {
            if (this.GetStyle(ControlStyles.UserPaint))
            {
                if (!this.GetStyle(ControlStyles.AllPaintingInWmPaint))
                {
                    IntPtr wParam = m.WParam;
                    if (wParam == IntPtr.Zero)
                    {
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                    System.Windows.Forms.UnsafeNativeMethods.GetClientRect(new HandleRef(this, this.Handle), ref rect);
                    using (PaintEventArgs args = new PaintEventArgs(wParam, Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom)))
                    {
                        this.PaintWithErrorHandling(args, 1);
                    }
                }
                m.Result = (IntPtr) 1;
            }
            else
            {
                this.DefWndProc(ref m);
            }
        }

        private void WmExitMenuLoop(ref Message m)
        {
            if (((((int) ((long) m.WParam)) == 0) ? 0 : 1) != 0)
            {
                System.Windows.Forms.ContextMenu menu = (System.Windows.Forms.ContextMenu) this.Properties.GetObject(PropContextMenu);
                if (menu != null)
                {
                    menu.OnCollapse(EventArgs.Empty);
                }
            }
            this.DefWndProc(ref m);
        }

        private void WmGetControlName(ref Message m)
        {
            string name;
            if (this.Site != null)
            {
                name = this.Site.Name;
            }
            else
            {
                name = this.Name;
            }
            if (name == null)
            {
                name = "";
            }
            this.MarshalStringToMessage(name, ref m);
        }

        private void WmGetControlType(ref Message m)
        {
            string assemblyQualifiedName = base.GetType().AssemblyQualifiedName;
            this.MarshalStringToMessage(assemblyQualifiedName, ref m);
        }

        private void WmGetObject(ref Message m)
        {
            InternalAccessibleObject obj2 = null;
            AccessibleObject accessibilityObject = this.GetAccessibilityObject((int) ((long) m.LParam));
            if (accessibilityObject != null)
            {
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    obj2 = new InternalAccessibleObject(accessibilityObject);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            if (obj2 != null)
            {
                Guid refiid = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");
                try
                {
                    object obj4 = obj2;
                    if (obj4 is IAccessible)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ControlAccessibileObjectInvalid"));
                    }
                    System.Windows.Forms.UnsafeNativeMethods.IAccessibleInternal o = obj2;
                    if (o == null)
                    {
                        m.Result = IntPtr.Zero;
                    }
                    else
                    {
                        IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(o);
                        System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                        try
                        {
                            m.Result = System.Windows.Forms.UnsafeNativeMethods.LresultFromObject(ref refiid, m.WParam, new HandleRef(accessibilityObject, iUnknownForObject));
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                            Marshal.Release(iUnknownForObject);
                        }
                    }
                    return;
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("RichControlLresult"), exception);
                }
            }
            this.DefWndProc(ref m);
        }

        private void WmHelp(ref Message m)
        {
            HelpInfo helpInfo = MessageBox.HelpInfo;
            if (helpInfo != null)
            {
                switch (helpInfo.Option)
                {
                    case 1:
                        Help.ShowHelp(this, helpInfo.HelpFilePath);
                        break;

                    case 2:
                        Help.ShowHelp(this, helpInfo.HelpFilePath, helpInfo.Keyword);
                        break;

                    case 3:
                        Help.ShowHelp(this, helpInfo.HelpFilePath, helpInfo.Navigator);
                        break;

                    case 4:
                        Help.ShowHelp(this, helpInfo.HelpFilePath, helpInfo.Navigator, helpInfo.Param);
                        break;
                }
            }
            System.Windows.Forms.NativeMethods.HELPINFO lParam = (System.Windows.Forms.NativeMethods.HELPINFO) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.HELPINFO));
            HelpEventArgs hevent = new HelpEventArgs(new Point(lParam.MousePos.x, lParam.MousePos.y));
            this.OnHelpRequested(hevent);
            if (!hevent.Handled)
            {
                this.DefWndProc(ref m);
            }
        }

        private void WmImeChar(ref Message m)
        {
            if (!this.ProcessKeyEventArgs(ref m))
            {
                this.DefWndProc(ref m);
            }
        }

        private void WmImeEndComposition(ref Message m)
        {
            this.ImeWmCharsToIgnore = -1;
            this.DefWndProc(ref m);
        }

        private void WmImeKillFocus()
        {
            Control topMostParent = this.TopMostParent;
            Form form = topMostParent as Form;
            if (((form == null) || form.Modal) && (!topMostParent.ContainsFocus && (propagatingImeMode != System.Windows.Forms.ImeMode.Inherit)))
            {
                IgnoreWmImeNotify = true;
                try
                {
                    ImeContext.SetImeStatus(PropagatingImeMode, topMostParent.Handle);
                    PropagatingImeMode = System.Windows.Forms.ImeMode.Inherit;
                }
                finally
                {
                    IgnoreWmImeNotify = false;
                }
            }
        }

        private void WmImeNotify(ref Message m)
        {
            if ((this.ImeSupported && (ImeModeConversion.InputLanguageTable != ImeModeConversion.UnsupportedTable)) && !IgnoreWmImeNotify)
            {
                switch (((int) m.WParam))
                {
                    case 6:
                    case 8:
                        this.OnImeContextStatusChanged(this.Handle);
                        break;
                }
            }
            this.DefWndProc(ref m);
        }

        internal void WmImeSetFocus()
        {
            if (ImeModeConversion.InputLanguageTable != ImeModeConversion.UnsupportedTable)
            {
                this.UpdateImeContextMode();
            }
        }

        private void WmImeStartComposition(ref Message m)
        {
            this.Properties.SetInteger(PropImeWmCharsToIgnore, 0);
            this.DefWndProc(ref m);
        }

        private void WmInitMenuPopup(ref Message m)
        {
            System.Windows.Forms.ContextMenu menu = (System.Windows.Forms.ContextMenu) this.Properties.GetObject(PropContextMenu);
            if ((menu == null) || !menu.ProcessInitMenuPopup(m.WParam))
            {
                this.DefWndProc(ref m);
            }
        }

        private void WmInputLangChange(ref Message m)
        {
            this.UpdateImeContextMode();
            if (ImeModeConversion.InputLanguageTable == ImeModeConversion.UnsupportedTable)
            {
                PropagatingImeMode = System.Windows.Forms.ImeMode.Off;
            }
            Form form = this.FindFormInternal();
            if (form != null)
            {
                InputLanguageChangedEventArgs iplevent = InputLanguage.CreateInputLanguageChangedEventArgs(m);
                form.PerformOnInputLanguageChanged(iplevent);
            }
            this.DefWndProc(ref m);
        }

        private void WmInputLangChangeRequest(ref Message m)
        {
            InputLanguageChangingEventArgs iplcevent = InputLanguage.CreateInputLanguageChangingEventArgs(m);
            Form form = this.FindFormInternal();
            if (form != null)
            {
                form.PerformOnInputLanguageChanging(iplcevent);
            }
            if (!iplcevent.Cancel)
            {
                this.DefWndProc(ref m);
            }
            else
            {
                m.Result = IntPtr.Zero;
            }
        }

        private void WmKeyChar(ref Message m)
        {
            if (!this.ProcessKeyMessage(ref m))
            {
                this.DefWndProc(ref m);
            }
        }

        private void WmKillFocus(ref Message m)
        {
            this.WmImeKillFocus();
            this.DefWndProc(ref m);
            this.OnLostFocus(EventArgs.Empty);
        }

        private void WmMeasureItem(ref Message m)
        {
            if (m.WParam == IntPtr.Zero)
            {
                System.Windows.Forms.NativeMethods.MEASUREITEMSTRUCT lParam = (System.Windows.Forms.NativeMethods.MEASUREITEMSTRUCT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.MEASUREITEMSTRUCT));
                MenuItem menuItemFromItemData = MenuItem.GetMenuItemFromItemData(lParam.itemData);
                if (menuItemFromItemData != null)
                {
                    menuItemFromItemData.WmMeasureItem(ref m);
                }
            }
            else
            {
                this.WmOwnerDraw(ref m);
            }
        }

        private void WmMenuChar(ref Message m)
        {
            Menu contextMenu = this.ContextMenu;
            if (contextMenu != null)
            {
                contextMenu.WmMenuChar(ref m);
                bool flag1 = m.Result != IntPtr.Zero;
            }
        }

        private void WmMenuSelect(ref Message m)
        {
            int id = System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam);
            int num2 = System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam);
            IntPtr lParam = m.LParam;
            MenuItem baseItem = null;
            if ((num2 & 0x2000) == 0)
            {
                if ((num2 & 0x10) == 0)
                {
                    Command commandFromID = Command.GetCommandFromID(id);
                    if (commandFromID != null)
                    {
                        object target = commandFromID.Target;
                        if ((target != null) && (target is MenuItem.MenuItemData))
                        {
                            baseItem = ((MenuItem.MenuItemData) target).baseItem;
                        }
                    }
                }
                else
                {
                    baseItem = this.GetMenuItemFromHandleId(lParam, id);
                }
            }
            if (baseItem != null)
            {
                baseItem.PerformSelect();
            }
            this.DefWndProc(ref m);
        }

        private void WmMouseDown(ref Message m, System.Windows.Forms.MouseButtons button, int clicks)
        {
            System.Windows.Forms.MouseButtons mouseButtons = MouseButtons;
            this.SetState(0x8000000, true);
            if (!this.GetStyle(ControlStyles.UserMouse))
            {
                this.DefWndProc(ref m);
            }
            else if ((button == System.Windows.Forms.MouseButtons.Left) && this.GetStyle(ControlStyles.Selectable))
            {
                this.FocusInternal();
            }
            if (mouseButtons == MouseButtons)
            {
                if (!this.GetState2(0x10))
                {
                    this.CaptureInternal = true;
                }
                if ((mouseButtons == MouseButtons) && this.Enabled)
                {
                    this.OnMouseDown(new MouseEventArgs(button, clicks, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                }
            }
        }

        private void WmMouseEnter(ref Message m)
        {
            this.DefWndProc(ref m);
            this.OnMouseEnter(EventArgs.Empty);
        }

        private void WmMouseHover(ref Message m)
        {
            this.DefWndProc(ref m);
            this.OnMouseHover(EventArgs.Empty);
        }

        private void WmMouseLeave(ref Message m)
        {
            this.DefWndProc(ref m);
            this.OnMouseLeave(EventArgs.Empty);
        }

        private void WmMouseMove(ref Message m)
        {
            if (!this.GetStyle(ControlStyles.UserMouse))
            {
                this.DefWndProc(ref m);
            }
            this.OnMouseMove(new MouseEventArgs(MouseButtons, 0, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
        }

        private void WmMouseUp(ref Message m, System.Windows.Forms.MouseButtons button, int clicks)
        {
            try
            {
                int x = System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam);
                int y = System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam);
                Point p = new Point(x, y);
                p = this.PointToScreen(p);
                if (!this.GetStyle(ControlStyles.UserMouse))
                {
                    this.DefWndProc(ref m);
                }
                else if (button == System.Windows.Forms.MouseButtons.Right)
                {
                    this.SendMessage(0x7b, this.Handle, System.Windows.Forms.NativeMethods.Util.MAKELPARAM(p.X, p.Y));
                }
                bool flag = false;
                if ((((this.controlStyle & ControlStyles.StandardClick) == ControlStyles.StandardClick) && this.GetState(0x8000000)) && (!this.IsDisposed && (System.Windows.Forms.UnsafeNativeMethods.WindowFromPoint(p.X, p.Y) == this.Handle)))
                {
                    flag = true;
                }
                if (flag && !this.ValidationCancelled)
                {
                    if (!this.GetState(0x4000000))
                    {
                        this.OnClick(new MouseEventArgs(button, clicks, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                        this.OnMouseClick(new MouseEventArgs(button, clicks, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                    }
                    else
                    {
                        this.OnDoubleClick(new MouseEventArgs(button, 2, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                        this.OnMouseDoubleClick(new MouseEventArgs(button, 2, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                    }
                }
                this.OnMouseUp(new MouseEventArgs(button, clicks, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
            }
            finally
            {
                this.SetState(0x4000000, false);
                this.SetState(0x8000000, false);
                this.SetState(0x10000000, false);
                this.CaptureInternal = false;
            }
        }

        private void WmMouseWheel(ref Message m)
        {
            Point p = new Point(System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam));
            p = this.PointToClient(p);
            HandledMouseEventArgs e = new HandledMouseEventArgs(System.Windows.Forms.MouseButtons.None, 0, p.X, p.Y, System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.WParam));
            this.OnMouseWheel(e);
            m.Result = e.Handled ? IntPtr.Zero : ((IntPtr) 1);
            if (!e.Handled)
            {
                this.DefWndProc(ref m);
            }
        }

        private void WmMove(ref Message m)
        {
            this.DefWndProc(ref m);
            this.UpdateBounds();
        }

        private unsafe void WmNotify(ref Message m)
        {
            System.Windows.Forms.NativeMethods.NMHDR* lParam = (System.Windows.Forms.NativeMethods.NMHDR*) m.LParam;
            if (!ReflectMessageInternal(lParam->hwndFrom, ref m))
            {
                if (lParam->code == -521)
                {
                    m.Result = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(null, lParam->hwndFrom), 0x2000 + m.Msg, m.WParam, m.LParam);
                }
                else
                {
                    if (lParam->code == -522)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(null, lParam->hwndFrom), 0x2000 + m.Msg, m.WParam, m.LParam);
                    }
                    this.DefWndProc(ref m);
                }
            }
        }

        private void WmNotifyFormat(ref Message m)
        {
            if (!ReflectMessageInternal(m.WParam, ref m))
            {
                this.DefWndProc(ref m);
            }
        }

        private void WmOwnerDraw(ref Message m)
        {
            bool flag = false;
            if (!ReflectMessageInternal(m.WParam, ref m))
            {
                IntPtr handleFromID = this.window.GetHandleFromID((short) System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam));
                if (handleFromID != IntPtr.Zero)
                {
                    Control control = FromHandleInternal(handleFromID);
                    if (control != null)
                    {
                        m.Result = control.SendMessage(0x2000 + m.Msg, handleFromID, m.LParam);
                        flag = true;
                    }
                }
            }
            else
            {
                flag = true;
            }
            if (!flag)
            {
                this.DefWndProc(ref m);
            }
        }

        private void WmPaint(ref Message m)
        {
            bool flag = this.DoubleBuffered || (this.GetStyle(ControlStyles.AllPaintingInWmPaint) && this.DoubleBufferingEnabled);
            IntPtr zero = IntPtr.Zero;
            System.Windows.Forms.NativeMethods.PAINTSTRUCT lpPaint = new System.Windows.Forms.NativeMethods.PAINTSTRUCT();
            bool flag2 = false;
            try
            {
                IntPtr wParam;
                Rectangle clientRectangle;
                if (m.WParam == IntPtr.Zero)
                {
                    zero = this.Handle;
                    wParam = System.Windows.Forms.UnsafeNativeMethods.BeginPaint(new HandleRef(this, zero), ref lpPaint);
                    flag2 = true;
                    clientRectangle = new Rectangle(lpPaint.rcPaint_left, lpPaint.rcPaint_top, lpPaint.rcPaint_right - lpPaint.rcPaint_left, lpPaint.rcPaint_bottom - lpPaint.rcPaint_top);
                }
                else
                {
                    wParam = m.WParam;
                    clientRectangle = this.ClientRectangle;
                }
                if (!flag || ((clientRectangle.Width > 0) && (clientRectangle.Height > 0)))
                {
                    IntPtr handle = IntPtr.Zero;
                    BufferedGraphics graphics = null;
                    PaintEventArgs e = null;
                    System.Drawing.Drawing2D.GraphicsState gstate = null;
                    try
                    {
                        if (flag || (m.WParam == IntPtr.Zero))
                        {
                            handle = SetUpPalette(wParam, false, false);
                        }
                        if (flag)
                        {
                            try
                            {
                                graphics = this.BufferContext.Allocate(wParam, this.ClientRectangle);
                            }
                            catch (Exception exception)
                            {
                                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                                {
                                    throw;
                                }
                                flag = false;
                            }
                        }
                        if (graphics != null)
                        {
                            graphics.Graphics.SetClip(clientRectangle);
                            e = new PaintEventArgs(graphics.Graphics, clientRectangle);
                            gstate = e.Graphics.Save();
                        }
                        else
                        {
                            e = new PaintEventArgs(wParam, clientRectangle);
                        }
                        using (e)
                        {
                            if (((m.WParam == IntPtr.Zero) && this.GetStyle(ControlStyles.AllPaintingInWmPaint)) || flag)
                            {
                                this.PaintWithErrorHandling(e, 1);
                                if (gstate != null)
                                {
                                    e.Graphics.Restore(gstate);
                                }
                                else
                                {
                                    e.ResetGraphics();
                                }
                            }
                            this.PaintWithErrorHandling(e, 2);
                            if (graphics != null)
                            {
                                graphics.Render();
                            }
                        }
                    }
                    finally
                    {
                        if (graphics != null)
                        {
                            graphics.Dispose();
                        }
                        if (handle != IntPtr.Zero)
                        {
                            System.Windows.Forms.SafeNativeMethods.SelectPalette(new HandleRef(null, wParam), new HandleRef(null, handle), 0);
                        }
                    }
                }
            }
            finally
            {
                if (flag2)
                {
                    System.Windows.Forms.UnsafeNativeMethods.EndPaint(new HandleRef(this, zero), ref lpPaint);
                }
            }
        }

        private void WmParentNotify(ref Message m)
        {
            int num = System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam);
            IntPtr zero = IntPtr.Zero;
            switch (num)
            {
                case 1:
                    zero = m.LParam;
                    break;

                case 2:
                    break;

                default:
                    zero = System.Windows.Forms.UnsafeNativeMethods.GetDlgItem(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam));
                    break;
            }
            if ((zero == IntPtr.Zero) || !ReflectMessageInternal(zero, ref m))
            {
                this.DefWndProc(ref m);
            }
        }

        private void WmPrintClient(ref Message m)
        {
            using (PaintEventArgs args = new PrintPaintEventArgs(m, m.WParam, this.ClientRectangle))
            {
                this.OnPrint(args);
            }
        }

        private void WmQueryNewPalette(ref Message m)
        {
            IntPtr dC = System.Windows.Forms.UnsafeNativeMethods.GetDC(new HandleRef(this, this.Handle));
            try
            {
                SetUpPalette(dC, true, true);
            }
            finally
            {
                System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(new HandleRef(this, this.Handle), new HandleRef(null, dC));
            }
            this.Invalidate(true);
            m.Result = (IntPtr) 1;
            this.DefWndProc(ref m);
        }

        private void WmSetCursor(ref Message m)
        {
            if ((m.WParam == this.InternalHandle) && (System.Windows.Forms.NativeMethods.Util.LOWORD(m.LParam) == 1))
            {
                System.Windows.Forms.Cursor.CurrentInternal = this.Cursor;
            }
            else
            {
                this.DefWndProc(ref m);
            }
        }

        private void WmSetFocus(ref Message m)
        {
            this.WmImeSetFocus();
            if (!this.HostedInWin32DialogManager)
            {
                IContainerControl containerControlInternal = this.GetContainerControlInternal();
                if (containerControlInternal != null)
                {
                    bool flag;
                    ContainerControl control2 = containerControlInternal as ContainerControl;
                    if (control2 != null)
                    {
                        flag = control2.ActivateControlInternal(this);
                    }
                    else
                    {
                        System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                        try
                        {
                            flag = containerControlInternal.ActivateControl(this);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                    if (!flag)
                    {
                        return;
                    }
                }
            }
            this.DefWndProc(ref m);
            this.OnGotFocus(EventArgs.Empty);
        }

        private void WmShowWindow(ref Message m)
        {
            this.DefWndProc(ref m);
            if ((this.state & 0x10) == 0)
            {
                bool flag = m.WParam != IntPtr.Zero;
                bool visible = this.Visible;
                if (flag)
                {
                    bool state = this.GetState(2);
                    this.SetState(2, true);
                    bool flag4 = false;
                    try
                    {
                        this.CreateControl();
                        flag4 = true;
                    }
                    finally
                    {
                        if (!flag4)
                        {
                            this.SetState(2, state);
                        }
                    }
                }
                else
                {
                    bool topLevel = this.GetTopLevel();
                    if (this.ParentInternal != null)
                    {
                        topLevel = this.ParentInternal.Visible;
                    }
                    if (topLevel)
                    {
                        this.SetState(2, false);
                    }
                }
                if (!this.GetState(0x20000000) && (visible != flag))
                {
                    this.OnVisibleChanged(EventArgs.Empty);
                }
            }
        }

        private void WmUpdateUIState(ref Message m)
        {
            bool showKeyboardCues = false;
            bool showFocusCues = false;
            bool flag3 = (this.uiCuesState & 240) != 0;
            bool flag4 = (this.uiCuesState & 15) != 0;
            if (flag3)
            {
                showKeyboardCues = this.ShowKeyboardCues;
            }
            if (flag4)
            {
                showFocusCues = this.ShowFocusCues;
            }
            this.DefWndProc(ref m);
            int num = System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam);
            if (num != 3)
            {
                UICues none = UICues.None;
                if ((System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam) & 2) != 0)
                {
                    bool flag5 = num == 2;
                    if ((flag5 != showKeyboardCues) || !flag3)
                    {
                        none |= UICues.ChangeKeyboard;
                        this.uiCuesState &= -241;
                        this.uiCuesState |= flag5 ? 0x20 : 0x10;
                    }
                    if (flag5)
                    {
                        none |= UICues.ShowKeyboard;
                    }
                }
                if ((System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam) & 1) != 0)
                {
                    bool flag6 = num == 2;
                    if ((flag6 != showFocusCues) || !flag4)
                    {
                        none |= UICues.ChangeFocus;
                        this.uiCuesState &= -16;
                        this.uiCuesState |= flag6 ? 2 : 1;
                    }
                    if (flag6)
                    {
                        none |= UICues.ShowFocus;
                    }
                }
                if ((none & UICues.Changed) != UICues.None)
                {
                    this.OnChangeUICues(new UICuesEventArgs(none));
                    this.Invalidate(true);
                }
            }
        }

        private unsafe void WmWindowPosChanged(ref Message m)
        {
            this.DefWndProc(ref m);
            this.UpdateBounds();
            if (((this.parent != null) && (System.Windows.Forms.UnsafeNativeMethods.GetParent(new HandleRef(this.window, this.InternalHandle)) == this.parent.InternalHandle)) && ((this.state & 0x100) == 0))
            {
                System.Windows.Forms.NativeMethods.WINDOWPOS* lParam = (System.Windows.Forms.NativeMethods.WINDOWPOS*) m.LParam;
                if ((lParam->flags & 4) == 0)
                {
                    this.parent.UpdateChildControlIndex(this);
                }
            }
        }

        private unsafe void WmWindowPosChanging(ref Message m)
        {
            if (this.IsActiveX)
            {
                System.Windows.Forms.NativeMethods.WINDOWPOS* lParam = (System.Windows.Forms.NativeMethods.WINDOWPOS*) m.LParam;
                bool flag = false;
                if (((lParam->flags & 2) == 0) && ((lParam->x != this.Left) || (lParam->y != this.Top)))
                {
                    flag = true;
                }
                if (((lParam->flags & 1) == 0) && ((lParam->cx != this.Width) || (lParam->cy != this.Height)))
                {
                    flag = true;
                }
                if (flag)
                {
                    this.ActiveXUpdateBounds(ref lParam->x, ref lParam->y, ref lParam->cx, ref lParam->cy, lParam->flags);
                }
            }
            this.DefWndProc(ref m);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected virtual void WndProc(ref Message m)
        {
            if ((this.controlStyle & ControlStyles.EnableNotifyMessage) == ControlStyles.EnableNotifyMessage)
            {
                this.OnNotifyMessage(m);
            }
            switch (m.Msg)
            {
                case 1:
                    this.WmCreate(ref m);
                    return;

                case 2:
                    this.WmDestroy(ref m);
                    return;

                case 3:
                    this.WmMove(ref m);
                    return;

                case 7:
                    this.WmSetFocus(ref m);
                    return;

                case 8:
                    this.WmKillFocus(ref m);
                    return;

                case 15:
                    if (!this.GetStyle(ControlStyles.UserPaint))
                    {
                        this.DefWndProc(ref m);
                        return;
                    }
                    this.WmPaint(ref m);
                    return;

                case 0x10:
                    this.WmClose(ref m);
                    return;

                case 20:
                    this.WmEraseBkgnd(ref m);
                    return;

                case 0x18:
                    this.WmShowWindow(ref m);
                    return;

                case 0x19:
                case 0x132:
                case 0x133:
                case 0x134:
                case 0x135:
                case 310:
                case 0x137:
                case 0x138:
                case 0x2132:
                case 0x2133:
                case 0x2134:
                case 0x2135:
                case 0x2136:
                case 0x2137:
                case 0x2138:
                case 0x2019:
                    this.WmCtlColorControl(ref m);
                    return;

                case 0x20:
                    this.WmSetCursor(ref m);
                    return;

                case 0x2b:
                    this.WmDrawItem(ref m);
                    return;

                case 0x2c:
                    this.WmMeasureItem(ref m);
                    return;

                case 0x2d:
                case 0x2e:
                case 0x2f:
                case 0x39:
                case 0x114:
                case 0x115:
                    if (!ReflectMessageInternal(m.LParam, ref m))
                    {
                        this.DefWndProc(ref m);
                    }
                    return;

                case 70:
                    this.WmWindowPosChanging(ref m);
                    return;

                case 0x47:
                    this.WmWindowPosChanged(ref m);
                    return;

                case 0x3d:
                    this.WmGetObject(ref m);
                    return;

                case 0x100:
                case 0x101:
                case 0x102:
                case 260:
                case 0x105:
                    this.WmKeyChar(ref m);
                    return;

                case 0x7e:
                    this.WmDisplayChange(ref m);
                    return;

                case 0x4e:
                    this.WmNotify(ref m);
                    return;

                case 80:
                    this.WmInputLangChangeRequest(ref m);
                    return;

                case 0x51:
                    this.WmInputLangChange(ref m);
                    return;

                case 0x53:
                    this.WmHelp(ref m);
                    return;

                case 0x55:
                    this.WmNotifyFormat(ref m);
                    return;

                case 0x7b:
                    this.WmContextMenu(ref m);
                    return;

                case 0x10d:
                    this.WmImeStartComposition(ref m);
                    return;

                case 270:
                    this.WmImeEndComposition(ref m);
                    return;

                case 0x111:
                    this.WmCommand(ref m);
                    return;

                case 0x112:
                    if (((((int) ((long) m.WParam)) & 0xfff0) != 0xf100) || !ToolStripManager.ProcessMenuKey(ref m))
                    {
                        this.DefWndProc(ref m);
                        return;
                    }
                    m.Result = IntPtr.Zero;
                    return;

                case 0x117:
                    this.WmInitMenuPopup(ref m);
                    return;

                case 0x11f:
                    this.WmMenuSelect(ref m);
                    return;

                case 0x120:
                    this.WmMenuChar(ref m);
                    return;

                case 0x128:
                    this.WmUpdateUIState(ref m);
                    return;

                case 0x200:
                    this.WmMouseMove(ref m);
                    return;

                case 0x201:
                    this.WmMouseDown(ref m, System.Windows.Forms.MouseButtons.Left, 1);
                    return;

                case 0x202:
                    this.WmMouseUp(ref m, System.Windows.Forms.MouseButtons.Left, 1);
                    return;

                case 0x203:
                    this.WmMouseDown(ref m, System.Windows.Forms.MouseButtons.Left, 2);
                    if (this.GetStyle(ControlStyles.StandardDoubleClick))
                    {
                        this.SetState(0x4000000, true);
                    }
                    return;

                case 0x204:
                    this.WmMouseDown(ref m, System.Windows.Forms.MouseButtons.Right, 1);
                    return;

                case 0x205:
                    this.WmMouseUp(ref m, System.Windows.Forms.MouseButtons.Right, 1);
                    return;

                case 0x206:
                    this.WmMouseDown(ref m, System.Windows.Forms.MouseButtons.Right, 2);
                    if (this.GetStyle(ControlStyles.StandardDoubleClick))
                    {
                        this.SetState(0x4000000, true);
                    }
                    return;

                case 0x207:
                    this.WmMouseDown(ref m, System.Windows.Forms.MouseButtons.Middle, 1);
                    return;

                case 520:
                    this.WmMouseUp(ref m, System.Windows.Forms.MouseButtons.Middle, 1);
                    return;

                case 0x209:
                    this.WmMouseDown(ref m, System.Windows.Forms.MouseButtons.Middle, 2);
                    if (this.GetStyle(ControlStyles.StandardDoubleClick))
                    {
                        this.SetState(0x4000000, true);
                    }
                    return;

                case 0x20a:
                    this.WmMouseWheel(ref m);
                    return;

                case 0x20b:
                    this.WmMouseDown(ref m, this.GetXButton(System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam)), 1);
                    return;

                case 0x20c:
                    this.WmMouseUp(ref m, this.GetXButton(System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam)), 1);
                    return;

                case 0x20d:
                    this.WmMouseDown(ref m, this.GetXButton(System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam)), 2);
                    if (this.GetStyle(ControlStyles.StandardDoubleClick))
                    {
                        this.SetState(0x4000000, true);
                    }
                    return;

                case 0x210:
                    this.WmParentNotify(ref m);
                    return;

                case 530:
                    this.WmExitMenuLoop(ref m);
                    return;

                case 0x215:
                    this.WmCaptureChanged(ref m);
                    return;

                case 0x282:
                    this.WmImeNotify(ref m);
                    return;

                case 0x2a1:
                    this.WmMouseHover(ref m);
                    return;

                case 0x2a3:
                    this.WmMouseLeave(ref m);
                    return;

                case 0x30f:
                    this.WmQueryNewPalette(ref m);
                    return;

                case 0x286:
                    this.WmImeChar(ref m);
                    return;

                case 0x2055:
                    m.Result = (Marshal.SystemDefaultCharSize == 1) ? ((IntPtr) 1) : ((IntPtr) 2);
                    return;

                case 0x318:
                    if (this.GetStyle(ControlStyles.UserPaint))
                    {
                        this.WmPrintClient(ref m);
                        return;
                    }
                    this.DefWndProc(ref m);
                    return;
            }
            if ((m.Msg == threadCallbackMessage) && (m.Msg != 0))
            {
                this.InvokeMarshaledCallbacks();
            }
            else if (m.Msg == WM_GETCONTROLNAME)
            {
                this.WmGetControlName(ref m);
            }
            else if (m.Msg == WM_GETCONTROLTYPE)
            {
                this.WmGetControlType(ref m);
            }
            else
            {
                if (mouseWheelRoutingNeeded && (m.Msg == mouseWheelMessage))
                {
                    Keys none = Keys.None;
                    none |= (System.Windows.Forms.UnsafeNativeMethods.GetKeyState(0x11) < 0) ? Keys.Back : Keys.None;
                    none |= (System.Windows.Forms.UnsafeNativeMethods.GetKeyState(0x10) < 0) ? Keys.MButton : Keys.None;
                    IntPtr focus = System.Windows.Forms.UnsafeNativeMethods.GetFocus();
                    if (focus == IntPtr.Zero)
                    {
                        this.SendMessage(m.Msg, (IntPtr) ((((int) ((long) m.WParam)) << 0x10) | none), m.LParam);
                    }
                    else
                    {
                        IntPtr zero = IntPtr.Zero;
                        IntPtr desktopWindow = System.Windows.Forms.UnsafeNativeMethods.GetDesktopWindow();
                        while (((zero == IntPtr.Zero) && (focus != IntPtr.Zero)) && (focus != desktopWindow))
                        {
                            zero = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(null, focus), 0x20a, (int) ((((int) ((long) m.WParam)) << 0x10) | none), m.LParam);
                            focus = System.Windows.Forms.UnsafeNativeMethods.GetParent(new HandleRef(null, focus));
                        }
                    }
                }
                if (m.Msg == System.Windows.Forms.NativeMethods.WM_MOUSEENTER)
                {
                    this.WmMouseEnter(ref m);
                }
                else
                {
                    this.DefWndProc(ref m);
                }
            }
        }

        private void WndProcException(Exception e)
        {
            Application.OnThreadException(e);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), System.Windows.Forms.SRDescription("ControlAccessibilityObjectDescr")]
        public AccessibleObject AccessibilityObject
        {
            get
            {
                AccessibleObject obj2 = (AccessibleObject) this.Properties.GetObject(PropAccessibility);
                if (obj2 == null)
                {
                    obj2 = this.CreateAccessibilityInstance();
                    if (!(obj2 is ControlAccessibleObject))
                    {
                        return null;
                    }
                    this.Properties.SetObject(PropAccessibility, obj2);
                }
                return obj2;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlAccessibleDefaultActionDescr"), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRCategory("CatAccessibility")]
        public string AccessibleDefaultActionDescription
        {
            get
            {
                return (string) this.Properties.GetObject(PropAccessibleDefaultActionDescription);
            }
            set
            {
                this.Properties.SetObject(PropAccessibleDefaultActionDescription, value);
            }
        }

        [DefaultValue((string) null), System.Windows.Forms.SRCategory("CatAccessibility"), Localizable(true), System.Windows.Forms.SRDescription("ControlAccessibleDescriptionDescr")]
        public string AccessibleDescription
        {
            get
            {
                return (string) this.Properties.GetObject(PropAccessibleDescription);
            }
            set
            {
                this.Properties.SetObject(PropAccessibleDescription, value);
            }
        }

        [DefaultValue((string) null), Localizable(true), System.Windows.Forms.SRDescription("ControlAccessibleNameDescr"), System.Windows.Forms.SRCategory("CatAccessibility")]
        public string AccessibleName
        {
            get
            {
                return (string) this.Properties.GetObject(PropAccessibleName);
            }
            set
            {
                this.Properties.SetObject(PropAccessibleName, value);
            }
        }

        [DefaultValue(-1), System.Windows.Forms.SRCategory("CatAccessibility"), System.Windows.Forms.SRDescription("ControlAccessibleRoleDescr")]
        public System.Windows.Forms.AccessibleRole AccessibleRole
        {
            get
            {
                bool flag;
                int integer = this.Properties.GetInteger(PropAccessibleRole, out flag);
                if (flag)
                {
                    return (System.Windows.Forms.AccessibleRole) integer;
                }
                return System.Windows.Forms.AccessibleRole.Default;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, -1, 0x40))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.AccessibleRole));
                }
                this.Properties.SetInteger(PropAccessibleRole, (int) value);
            }
        }

        private System.Drawing.Color ActiveXAmbientBackColor
        {
            get
            {
                return this.ActiveXInstance.AmbientBackColor;
            }
        }

        private System.Drawing.Font ActiveXAmbientFont
        {
            get
            {
                return this.ActiveXInstance.AmbientFont;
            }
        }

        private System.Drawing.Color ActiveXAmbientForeColor
        {
            get
            {
                return this.ActiveXInstance.AmbientForeColor;
            }
        }

        private bool ActiveXEventsFrozen
        {
            get
            {
                return this.ActiveXInstance.EventsFrozen;
            }
        }

        private IntPtr ActiveXHWNDParent
        {
            get
            {
                return this.ActiveXInstance.HWNDParent;
            }
        }

        private ActiveXImpl ActiveXInstance
        {
            get
            {
                ActiveXImpl impl = (ActiveXImpl) this.Properties.GetObject(PropActiveXImpl);
                if (impl == null)
                {
                    if (this.GetState(0x80000))
                    {
                        throw new NotSupportedException(System.Windows.Forms.SR.GetString("AXTopLevelSource"));
                    }
                    impl = new ActiveXImpl(this);
                    this.SetState2(0x400, true);
                    this.Properties.SetObject(PropActiveXImpl, impl);
                }
                return impl;
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ControlAllowDropDescr")]
        public virtual bool AllowDrop
        {
            get
            {
                return this.GetState(0x40);
            }
            set
            {
                if (this.GetState(0x40) != value)
                {
                    if (value && !this.IsHandleCreated)
                    {
                        System.Windows.Forms.IntSecurity.ClipboardRead.Demand();
                    }
                    this.SetState(0x40, value);
                    if (this.IsHandleCreated)
                    {
                        try
                        {
                            this.SetAcceptDrops(value);
                        }
                        catch
                        {
                            this.SetState(0x40, !value);
                            throw;
                        }
                    }
                }
            }
        }

        private AmbientProperties AmbientPropertiesService
        {
            get
            {
                bool flag;
                AmbientProperties service = (AmbientProperties) this.Properties.GetObject(PropAmbientPropertiesService, out flag);
                if (!flag)
                {
                    if (this.Site != null)
                    {
                        service = (AmbientProperties) this.Site.GetService(typeof(AmbientProperties));
                    }
                    else
                    {
                        service = (AmbientProperties) this.GetService(typeof(AmbientProperties));
                    }
                    if (service != null)
                    {
                        this.Properties.SetObject(PropAmbientPropertiesService, service);
                    }
                }
                return service;
            }
        }

        [Localizable(true), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRCategory("CatLayout"), DefaultValue(5), System.Windows.Forms.SRDescription("ControlAnchorDescr")]
        public virtual AnchorStyles Anchor
        {
            get
            {
                return DefaultLayout.GetAnchor(this);
            }
            set
            {
                DefaultLayout.SetAnchor(this.ParentInternal, this, value);
            }
        }

        [DefaultValue(typeof(Point), "0, 0"), EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public virtual Point AutoScrollOffset
        {
            get
            {
                if (this.Properties.ContainsObject(PropAutoScrollOffset))
                {
                    return (Point) this.Properties.GetObject(PropAutoScrollOffset);
                }
                return Point.Empty;
            }
            set
            {
                if (this.AutoScrollOffset != value)
                {
                    this.Properties.SetObject(PropAutoScrollOffset, value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(false), System.Windows.Forms.SRDescription("ControlAutoSizeDescr"), EditorBrowsable(EditorBrowsableState.Never), Localizable(true), Browsable(false), RefreshProperties(RefreshProperties.All), System.Windows.Forms.SRCategory("CatLayout")]
        public virtual bool AutoSize
        {
            get
            {
                return CommonProperties.GetAutoSize(this);
            }
            set
            {
                if (value != this.AutoSize)
                {
                    CommonProperties.SetAutoSize(this, value);
                    if (this.ParentInternal != null)
                    {
                        if (value && (this.ParentInternal.LayoutEngine == DefaultLayout.Instance))
                        {
                            this.ParentInternal.LayoutEngine.InitLayout(this, BoundsSpecified.Size);
                        }
                        LayoutTransaction.DoLayout(this.ParentInternal, this, PropertyNames.AutoSize);
                    }
                    this.OnAutoSizeChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DispId(-501), System.Windows.Forms.SRDescription("ControlBackColorDescr")]
        public virtual System.Drawing.Color BackColor
        {
            get
            {
                System.Drawing.Color rawBackColor = this.RawBackColor;
                if (!rawBackColor.IsEmpty)
                {
                    return rawBackColor;
                }
                Control parentInternal = this.ParentInternal;
                if ((parentInternal != null) && parentInternal.CanAccessProperties)
                {
                    rawBackColor = parentInternal.BackColor;
                    if (this.IsValidBackColor(rawBackColor))
                    {
                        return rawBackColor;
                    }
                }
                if (this.IsActiveX)
                {
                    rawBackColor = this.ActiveXAmbientBackColor;
                }
                if (rawBackColor.IsEmpty)
                {
                    AmbientProperties ambientPropertiesService = this.AmbientPropertiesService;
                    if (ambientPropertiesService != null)
                    {
                        rawBackColor = ambientPropertiesService.BackColor;
                    }
                }
                if (!rawBackColor.IsEmpty && this.IsValidBackColor(rawBackColor))
                {
                    return rawBackColor;
                }
                return DefaultBackColor;
            }
            set
            {
                if ((!value.Equals(System.Drawing.Color.Empty) && !this.GetStyle(ControlStyles.SupportsTransparentBackColor)) && (value.A < 0xff))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("TransparentBackColorNotAllowed"));
                }
                System.Drawing.Color backColor = this.BackColor;
                if (!value.IsEmpty || this.Properties.ContainsObject(PropBackColor))
                {
                    this.Properties.SetColor(PropBackColor, value);
                }
                if (!backColor.Equals(this.BackColor))
                {
                    this.OnBackColorChanged(EventArgs.Empty);
                }
            }
        }

        internal IntPtr BackColorBrush
        {
            get
            {
                IntPtr sysColorBrush;
                object obj2 = this.Properties.GetObject(PropBackBrush);
                if (obj2 != null)
                {
                    return (IntPtr) obj2;
                }
                if ((!this.Properties.ContainsObject(PropBackColor) && (this.parent != null)) && (this.parent.BackColor == this.BackColor))
                {
                    return this.parent.BackColorBrush;
                }
                System.Drawing.Color backColor = this.BackColor;
                if (ColorTranslator.ToOle(backColor) < 0)
                {
                    sysColorBrush = System.Windows.Forms.SafeNativeMethods.GetSysColorBrush(ColorTranslator.ToOle(backColor) & 0xff);
                    this.SetState(0x200000, false);
                }
                else
                {
                    sysColorBrush = System.Windows.Forms.SafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(backColor));
                    this.SetState(0x200000, true);
                }
                this.Properties.SetObject(PropBackBrush, sysColorBrush);
                return sysColorBrush;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue((string) null), System.Windows.Forms.SRDescription("ControlBackgroundImageDescr"), Localizable(true)]
        public virtual Image BackgroundImage
        {
            get
            {
                return (Image) this.Properties.GetObject(PropBackgroundImage);
            }
            set
            {
                if (this.BackgroundImage != value)
                {
                    this.Properties.SetObject(PropBackgroundImage, value);
                    this.OnBackgroundImageChanged(EventArgs.Empty);
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(1), System.Windows.Forms.SRDescription("ControlBackgroundImageLayoutDescr")]
        public virtual ImageLayout BackgroundImageLayout
        {
            get
            {
                if (!this.Properties.ContainsObject(PropBackgroundImageLayout))
                {
                    return ImageLayout.Tile;
                }
                return (ImageLayout) this.Properties.GetObject(PropBackgroundImageLayout);
            }
            set
            {
                if (this.BackgroundImageLayout != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 4))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(ImageLayout));
                    }
                    if (((value == ImageLayout.Center) || (value == ImageLayout.Zoom)) || (value == ImageLayout.Stretch))
                    {
                        this.SetStyle(ControlStyles.ResizeRedraw, true);
                        if (ControlPaint.IsImageTransparent(this.BackgroundImage))
                        {
                            this.DoubleBuffered = true;
                        }
                    }
                    this.Properties.SetObject(PropBackgroundImageLayout, value);
                    this.OnBackgroundImageLayoutChanged(EventArgs.Empty);
                }
            }
        }

        internal bool BecomingActiveControl
        {
            get
            {
                return this.GetState2(0x20);
            }
            set
            {
                if (value != this.BecomingActiveControl)
                {
                    Application.ThreadContext.FromCurrent().ActivatingControl = value ? this : null;
                    this.SetState2(0x20, value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), System.Windows.Forms.SRDescription("ControlBindingContextDescr")]
        public virtual System.Windows.Forms.BindingContext BindingContext
        {
            get
            {
                return this.BindingContextInternal;
            }
            set
            {
                this.BindingContextInternal = value;
            }
        }

        internal System.Windows.Forms.BindingContext BindingContextInternal
        {
            get
            {
                System.Windows.Forms.BindingContext context = (System.Windows.Forms.BindingContext) this.Properties.GetObject(PropBindingManager);
                if (context != null)
                {
                    return context;
                }
                Control parentInternal = this.ParentInternal;
                if ((parentInternal != null) && parentInternal.CanAccessProperties)
                {
                    return parentInternal.BindingContext;
                }
                return null;
            }
            set
            {
                System.Windows.Forms.BindingContext context = (System.Windows.Forms.BindingContext) this.Properties.GetObject(PropBindingManager);
                System.Windows.Forms.BindingContext context2 = value;
                if (context != context2)
                {
                    this.Properties.SetObject(PropBindingManager, context2);
                    this.OnBindingContextChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRDescription("ControlBottomDescr"), EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatLayout")]
        public int Bottom
        {
            get
            {
                return (this.y + this.height);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), System.Windows.Forms.SRDescription("ControlBoundsDescr"), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRCategory("CatLayout")]
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(this.x, this.y, this.width, this.height);
            }
            set
            {
                this.SetBounds(value.X, value.Y, value.Width, value.Height, BoundsSpecified.All);
            }
        }

        private BufferedGraphicsContext BufferContext
        {
            get
            {
                return BufferedGraphicsManager.Current;
            }
        }

        internal System.Windows.Forms.ImeMode CachedImeMode
        {
            get
            {
                bool flag;
                System.Windows.Forms.ImeMode integer = (System.Windows.Forms.ImeMode) this.Properties.GetInteger(PropImeMode, out flag);
                if (!flag)
                {
                    integer = this.DefaultImeMode;
                }
                if (integer != System.Windows.Forms.ImeMode.Inherit)
                {
                    return integer;
                }
                Control parentInternal = this.ParentInternal;
                if (parentInternal != null)
                {
                    return parentInternal.CachedImeMode;
                }
                return System.Windows.Forms.ImeMode.NoControl;
            }
            set
            {
                this.Properties.SetInteger(PropImeMode, (int) value);
            }
        }

        internal bool CacheTextInternal
        {
            get
            {
                bool flag;
                if (this.Properties.GetInteger(PropCacheTextCount, out flag) <= 0)
                {
                    return this.GetStyle(ControlStyles.CacheText);
                }
                return true;
            }
            set
            {
                if (!this.GetStyle(ControlStyles.CacheText) && this.IsHandleCreated)
                {
                    bool flag;
                    int integer = this.Properties.GetInteger(PropCacheTextCount, out flag);
                    if (value)
                    {
                        if (integer == 0)
                        {
                            this.Properties.SetObject(PropCacheTextField, this.text);
                            if (this.text == null)
                            {
                                this.text = this.WindowText;
                            }
                        }
                        integer++;
                    }
                    else
                    {
                        integer--;
                        if (integer == 0)
                        {
                            this.text = (string) this.Properties.GetObject(PropCacheTextField, out flag);
                        }
                    }
                    this.Properties.SetInteger(PropCacheTextCount, integer);
                }
            }
        }

        internal virtual bool CanAccessProperties
        {
            get
            {
                return true;
            }
        }

        protected virtual bool CanEnableIme
        {
            get
            {
                return this.ImeSupported;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatFocus"), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRDescription("ControlCanFocusDescr"), Browsable(false)]
        public bool CanFocus
        {
            get
            {
                if (!this.IsHandleCreated)
                {
                    return false;
                }
                bool flag = System.Windows.Forms.SafeNativeMethods.IsWindowVisible(new HandleRef(this.window, this.Handle));
                bool flag2 = System.Windows.Forms.SafeNativeMethods.IsWindowEnabled(new HandleRef(this.window, this.Handle));
                return (flag && flag2);
            }
        }

        protected override bool CanRaiseEvents
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (this.IsActiveX)
                {
                    return !this.ActiveXEventsFrozen;
                }
                return true;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatFocus"), System.Windows.Forms.SRDescription("ControlCanSelectDescr")]
        public bool CanSelect
        {
            get
            {
                return this.CanSelectCore();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRDescription("ControlCaptureDescr"), Browsable(false), System.Windows.Forms.SRCategory("CatFocus")]
        public bool Capture
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.CaptureInternal;
            }
            set
            {
                if (value)
                {
                    System.Windows.Forms.IntSecurity.GetCapture.Demand();
                }
                this.CaptureInternal = value;
            }
        }

        internal bool CaptureInternal
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (this.IsHandleCreated && (System.Windows.Forms.UnsafeNativeMethods.GetCapture() == this.Handle));
            }
            set
            {
                if (this.CaptureInternal != value)
                {
                    if (value)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetCapture(new HandleRef(this, this.Handle));
                    }
                    else
                    {
                        System.Windows.Forms.SafeNativeMethods.ReleaseCapture();
                    }
                }
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRCategory("CatFocus"), System.Windows.Forms.SRDescription("ControlCausesValidationDescr")]
        public bool CausesValidation
        {
            get
            {
                return this.GetState(0x20000);
            }
            set
            {
                if (value != this.CausesValidation)
                {
                    this.SetState(0x20000, value);
                    this.OnCausesValidationChanged(EventArgs.Empty);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlCheckForIllegalCrossThreadCalls"), Browsable(false)]
        public static bool CheckForIllegalCrossThreadCalls
        {
            get
            {
                return checkForIllegalCrossThreadCalls;
            }
            set
            {
                checkForIllegalCrossThreadCalls = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlClientRectangleDescr"), System.Windows.Forms.SRCategory("CatLayout"), Browsable(false)]
        public Rectangle ClientRectangle
        {
            get
            {
                return new Rectangle(0, 0, this.clientWidth, this.clientHeight);
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("ControlClientSizeDescr"), System.Windows.Forms.SRCategory("CatLayout"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Advanced)]
        public System.Drawing.Size ClientSize
        {
            get
            {
                return new System.Drawing.Size(this.clientWidth, this.clientHeight);
            }
            set
            {
                this.SetClientSizeCore(value.Width, value.Height);
            }
        }

        [Browsable(false), Description("ControlCompanyNameDescr"), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CompanyName
        {
            get
            {
                return this.VersionInfo.CompanyName;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlContainsFocusDescr"), Browsable(false)]
        public bool ContainsFocus
        {
            get
            {
                if (!this.IsHandleCreated)
                {
                    return false;
                }
                IntPtr focus = System.Windows.Forms.UnsafeNativeMethods.GetFocus();
                if (focus == IntPtr.Zero)
                {
                    return false;
                }
                return ((focus == this.Handle) || System.Windows.Forms.UnsafeNativeMethods.IsChild(new HandleRef(this, this.Handle), new HandleRef(this, focus)));
            }
        }

        [Browsable(false), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ControlContextMenuDescr"), DefaultValue((string) null)]
        public virtual System.Windows.Forms.ContextMenu ContextMenu
        {
            get
            {
                return (System.Windows.Forms.ContextMenu) this.Properties.GetObject(PropContextMenu);
            }
            set
            {
                System.Windows.Forms.ContextMenu menu = (System.Windows.Forms.ContextMenu) this.Properties.GetObject(PropContextMenu);
                if (menu != value)
                {
                    EventHandler handler = new EventHandler(this.DetachContextMenu);
                    if (menu != null)
                    {
                        menu.Disposed -= handler;
                    }
                    this.Properties.SetObject(PropContextMenu, value);
                    if (value != null)
                    {
                        value.Disposed += handler;
                    }
                    this.OnContextMenuChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue((string) null), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ControlContextMenuDescr")]
        public virtual System.Windows.Forms.ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return (System.Windows.Forms.ContextMenuStrip) this.Properties.GetObject(PropContextMenuStrip);
            }
            set
            {
                System.Windows.Forms.ContextMenuStrip strip = this.Properties.GetObject(PropContextMenuStrip) as System.Windows.Forms.ContextMenuStrip;
                if (strip != value)
                {
                    EventHandler handler = new EventHandler(this.DetachContextMenuStrip);
                    if (strip != null)
                    {
                        strip.Disposed -= handler;
                    }
                    this.Properties.SetObject(PropContextMenuStrip, value);
                    if (value != null)
                    {
                        value.Disposed += handler;
                    }
                    this.OnContextMenuStripChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRDescription("ControlControlsDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ControlCollection Controls
        {
            get
            {
                ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                if (controls == null)
                {
                    controls = this.CreateControlsInstance();
                    this.Properties.SetObject(PropControlsCollection, controls);
                }
                return controls;
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("ControlCreatedDescr"), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Created
        {
            get
            {
                return ((this.state & 1) != 0);
            }
        }

        protected virtual System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                if ((System.Windows.Forms.UnsafeNativeMethods.GetModuleHandle("comctl32.dll") == IntPtr.Zero) && (System.Windows.Forms.UnsafeNativeMethods.LoadLibrary("comctl32.dll") == IntPtr.Zero))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), System.Windows.Forms.SR.GetString("LoadDLLError", new object[] { "comctl32.dll" }));
                }
                if (this.createParams == null)
                {
                    this.createParams = new System.Windows.Forms.CreateParams();
                }
                System.Windows.Forms.CreateParams createParams = this.createParams;
                createParams.Style = 0;
                createParams.ExStyle = 0;
                createParams.ClassStyle = 0;
                createParams.Caption = this.text;
                createParams.X = this.x;
                createParams.Y = this.y;
                createParams.Width = this.width;
                createParams.Height = this.height;
                createParams.Style = 0x2000000;
                if (this.GetStyle(ControlStyles.ContainerControl))
                {
                    createParams.ExStyle |= 0x10000;
                }
                createParams.ClassStyle = 8;
                if ((this.state & 0x80000) == 0)
                {
                    createParams.Parent = (this.parent == null) ? IntPtr.Zero : this.parent.InternalHandle;
                    createParams.Style |= 0x44000000;
                }
                else
                {
                    createParams.Parent = IntPtr.Zero;
                }
                if ((this.state & 8) != 0)
                {
                    createParams.Style |= 0x10000;
                }
                if ((this.state & 2) != 0)
                {
                    createParams.Style |= 0x10000000;
                }
                if (!this.Enabled)
                {
                    createParams.Style |= 0x8000000;
                }
                if ((createParams.Parent == IntPtr.Zero) && this.IsActiveX)
                {
                    createParams.Parent = this.ActiveXHWNDParent;
                }
                if (this.RightToLeft == System.Windows.Forms.RightToLeft.Yes)
                {
                    createParams.ExStyle |= 0x2000;
                    createParams.ExStyle |= 0x1000;
                    createParams.ExStyle |= 0x4000;
                }
                return createParams;
            }
        }

        internal int CreateThreadId
        {
            get
            {
                if (this.IsHandleCreated)
                {
                    int num;
                    return System.Windows.Forms.SafeNativeMethods.GetWindowThreadProcessId(new HandleRef(this, this.Handle), out num);
                }
                return System.Windows.Forms.SafeNativeMethods.GetCurrentThreadId();
            }
        }

        internal System.Windows.Forms.ImeMode CurrentImeContextMode
        {
            get
            {
                if (this.IsHandleCreated)
                {
                    return ImeContext.GetImeMode(this.Handle);
                }
                return System.Windows.Forms.ImeMode.Inherit;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ControlCursorDescr"), AmbientValue((string) null)]
        public virtual System.Windows.Forms.Cursor Cursor
        {
            get
            {
                if (this.GetState(0x400))
                {
                    return Cursors.WaitCursor;
                }
                System.Windows.Forms.Cursor cursor = (System.Windows.Forms.Cursor) this.Properties.GetObject(PropCursor);
                if (cursor != null)
                {
                    return cursor;
                }
                System.Windows.Forms.Cursor defaultCursor = this.DefaultCursor;
                if (defaultCursor == Cursors.Default)
                {
                    Control parentInternal = this.ParentInternal;
                    if (parentInternal != null)
                    {
                        return parentInternal.Cursor;
                    }
                    AmbientProperties ambientPropertiesService = this.AmbientPropertiesService;
                    if ((ambientPropertiesService != null) && (ambientPropertiesService.Cursor != null))
                    {
                        return ambientPropertiesService.Cursor;
                    }
                }
                return defaultCursor;
            }
            set
            {
                System.Windows.Forms.Cursor cursor = (System.Windows.Forms.Cursor) this.Properties.GetObject(PropCursor);
                System.Windows.Forms.Cursor cursor2 = this.Cursor;
                if (cursor != value)
                {
                    System.Windows.Forms.IntSecurity.ModifyCursor.Demand();
                    this.Properties.SetObject(PropCursor, value);
                }
                if (this.IsHandleCreated)
                {
                    System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT();
                    System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                    System.Windows.Forms.UnsafeNativeMethods.GetCursorPos(pt);
                    System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(this, this.Handle), ref rect);
                    if ((((rect.left <= pt.x) && (pt.x < rect.right)) && ((rect.top <= pt.y) && (pt.y < rect.bottom))) || (System.Windows.Forms.UnsafeNativeMethods.GetCapture() == this.Handle))
                    {
                        this.SendMessage(0x20, this.Handle, (IntPtr) 1);
                    }
                }
                if (!cursor2.Equals(value))
                {
                    this.OnCursorChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatData"), System.Windows.Forms.SRDescription("ControlBindingsDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), RefreshProperties(RefreshProperties.All), ParenthesizePropertyName(true)]
        public ControlBindingsCollection DataBindings
        {
            get
            {
                ControlBindingsCollection bindingss = (ControlBindingsCollection) this.Properties.GetObject(PropBindings);
                if (bindingss == null)
                {
                    bindingss = new ControlBindingsCollection(this);
                    this.Properties.SetObject(PropBindings, bindingss);
                }
                return bindingss;
            }
        }

        public static System.Drawing.Color DefaultBackColor
        {
            get
            {
                return SystemColors.Control;
            }
        }

        protected virtual System.Windows.Forms.Cursor DefaultCursor
        {
            get
            {
                return Cursors.Default;
            }
        }

        public static System.Drawing.Font DefaultFont
        {
            get
            {
                if (defaultFont == null)
                {
                    defaultFont = SystemFonts.DefaultFont;
                }
                return defaultFont;
            }
        }

        public static System.Drawing.Color DefaultForeColor
        {
            get
            {
                return SystemColors.ControlText;
            }
        }

        protected virtual System.Windows.Forms.ImeMode DefaultImeMode
        {
            get
            {
                return System.Windows.Forms.ImeMode.Inherit;
            }
        }

        protected virtual System.Windows.Forms.Padding DefaultMargin
        {
            get
            {
                return CommonProperties.DefaultMargin;
            }
        }

        protected virtual System.Drawing.Size DefaultMaximumSize
        {
            get
            {
                return CommonProperties.DefaultMaximumSize;
            }
        }

        protected virtual System.Drawing.Size DefaultMinimumSize
        {
            get
            {
                return CommonProperties.DefaultMinimumSize;
            }
        }

        protected virtual System.Windows.Forms.Padding DefaultPadding
        {
            get
            {
                return System.Windows.Forms.Padding.Empty;
            }
        }

        private System.Windows.Forms.RightToLeft DefaultRightToLeft
        {
            get
            {
                return System.Windows.Forms.RightToLeft.No;
            }
        }

        protected virtual System.Drawing.Size DefaultSize
        {
            get
            {
                return System.Drawing.Size.Empty;
            }
        }

        internal System.Drawing.Color DisabledColor
        {
            get
            {
                System.Drawing.Color backColor = this.BackColor;
                if (backColor.A == 0)
                {
                    for (Control control = this.ParentInternal; backColor.A == 0; control = control.ParentInternal)
                    {
                        if (control == null)
                        {
                            return SystemColors.Control;
                        }
                        backColor = control.BackColor;
                    }
                }
                return backColor;
            }
        }

        internal int DisableImeModeChangedCount
        {
            get
            {
                bool flag;
                return this.Properties.GetInteger(PropDisableImeModeChangedCount, out flag);
            }
            set
            {
                this.Properties.SetInteger(PropDisableImeModeChangedCount, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlDisplayRectangleDescr"), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Rectangle DisplayRectangle
        {
            get
            {
                return new Rectangle(0, 0, this.clientWidth, this.clientHeight);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRDescription("ControlDisposingDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Disposing
        {
            get
            {
                return this.GetState(0x1000);
            }
        }

        [RefreshProperties(RefreshProperties.Repaint), DefaultValue(0), System.Windows.Forms.SRDescription("ControlDockDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatLayout")]
        public virtual DockStyle Dock
        {
            get
            {
                return DefaultLayout.GetDock(this);
            }
            set
            {
                if (value != this.Dock)
                {
                    this.SuspendLayout();
                    try
                    {
                        DefaultLayout.SetDock(this, value);
                        this.OnDockChanged(EventArgs.Empty);
                    }
                    finally
                    {
                        this.ResumeLayout();
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ControlDoubleBufferedDescr")]
        protected virtual bool DoubleBuffered
        {
            get
            {
                return this.GetStyle(ControlStyles.OptimizedDoubleBuffer);
            }
            set
            {
                if (value != this.DoubleBuffered)
                {
                    if (value)
                    {
                        this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, value);
                    }
                    else
                    {
                        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, value);
                    }
                }
            }
        }

        private bool DoubleBufferingEnabled
        {
            get
            {
                return this.GetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint);
            }
        }

        [System.Windows.Forms.SRDescription("ControlEnabledDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DispId(-514), Localizable(true)]
        public bool Enabled
        {
            get
            {
                if (!this.GetState(4))
                {
                    return false;
                }
                return ((this.ParentInternal == null) || this.ParentInternal.Enabled);
            }
            set
            {
                bool enabled = this.Enabled;
                this.SetState(4, value);
                if (enabled != value)
                {
                    if (!value)
                    {
                        this.SelectNextIfFocused();
                    }
                    this.OnEnabledChanged(EventArgs.Empty);
                }
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("ControlFocusedDescr"), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool Focused
        {
            get
            {
                return (this.IsHandleCreated && (System.Windows.Forms.UnsafeNativeMethods.GetFocus() == this.Handle));
            }
        }

        [AmbientValue((string) null), DispId(-512), System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true), System.Windows.Forms.SRDescription("ControlFontDescr")]
        public virtual System.Drawing.Font Font
        {
            [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(ActiveXFontMarshaler), MarshalCookie="")]
            get
            {
                System.Drawing.Font font = (System.Drawing.Font) this.Properties.GetObject(PropFont);
                if (font != null)
                {
                    return font;
                }
                System.Drawing.Font parentFont = this.GetParentFont();
                if (parentFont != null)
                {
                    return parentFont;
                }
                if (this.IsActiveX)
                {
                    parentFont = this.ActiveXAmbientFont;
                    if (parentFont != null)
                    {
                        return parentFont;
                    }
                }
                AmbientProperties ambientPropertiesService = this.AmbientPropertiesService;
                if ((ambientPropertiesService != null) && (ambientPropertiesService.Font != null))
                {
                    return ambientPropertiesService.Font;
                }
                return DefaultFont;
            }
            [param: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(ActiveXFontMarshaler), MarshalCookie="")]
            set
            {
                System.Drawing.Font font = (System.Drawing.Font) this.Properties.GetObject(PropFont);
                System.Drawing.Font font2 = this.Font;
                bool flag = false;
                if (value == null)
                {
                    if (font != null)
                    {
                        flag = true;
                    }
                }
                else if (font == null)
                {
                    flag = true;
                }
                else
                {
                    flag = !value.Equals(font);
                }
                if (flag)
                {
                    this.Properties.SetObject(PropFont, value);
                    if (!font2.Equals(value))
                    {
                        this.DisposeFontHandle();
                        if (this.Properties.ContainsInteger(PropFontHeight))
                        {
                            this.Properties.SetInteger(PropFontHeight, (value == null) ? -1 : value.Height);
                        }
                        using (new LayoutTransaction(this.ParentInternal, this, PropertyNames.Font))
                        {
                            this.OnFontChanged(EventArgs.Empty);
                            return;
                        }
                    }
                    if (this.IsHandleCreated && !this.GetStyle(ControlStyles.UserPaint))
                    {
                        this.DisposeFontHandle();
                        this.SetWindowFont();
                    }
                }
            }
        }

        internal IntPtr FontHandle
        {
            get
            {
                System.Drawing.Font font = (System.Drawing.Font) this.Properties.GetObject(PropFont);
                if (font != null)
                {
                    FontHandleWrapper wrapper = (FontHandleWrapper) this.Properties.GetObject(PropFontHandleWrapper);
                    if (wrapper == null)
                    {
                        wrapper = new FontHandleWrapper(font);
                        this.Properties.SetObject(PropFontHandleWrapper, wrapper);
                    }
                    return wrapper.Handle;
                }
                if (this.parent != null)
                {
                    return this.parent.FontHandle;
                }
                AmbientProperties ambientPropertiesService = this.AmbientPropertiesService;
                if ((ambientPropertiesService == null) || (ambientPropertiesService.Font == null))
                {
                    return GetDefaultFontHandleWrapper().Handle;
                }
                FontHandleWrapper wrapper2 = null;
                System.Drawing.Font font2 = (System.Drawing.Font) this.Properties.GetObject(PropCurrentAmbientFont);
                if ((font2 != null) && (font2 == ambientPropertiesService.Font))
                {
                    wrapper2 = (FontHandleWrapper) this.Properties.GetObject(PropFontHandleWrapper);
                }
                else
                {
                    this.Properties.SetObject(PropCurrentAmbientFont, ambientPropertiesService.Font);
                }
                if (wrapper2 == null)
                {
                    wrapper2 = new FontHandleWrapper(ambientPropertiesService.Font);
                    this.Properties.SetObject(PropFontHandleWrapper, wrapper2);
                }
                return wrapper2.Handle;
            }
        }

        protected int FontHeight
        {
            get
            {
                bool flag;
                int integer = this.Properties.GetInteger(PropFontHeight, out flag);
                if (flag && (integer != -1))
                {
                    return integer;
                }
                System.Drawing.Font font = (System.Drawing.Font) this.Properties.GetObject(PropFont);
                if (font != null)
                {
                    integer = font.Height;
                    this.Properties.SetInteger(PropFontHeight, integer);
                    return integer;
                }
                int fontHeight = -1;
                if ((this.ParentInternal != null) && this.ParentInternal.CanAccessProperties)
                {
                    fontHeight = this.ParentInternal.FontHeight;
                }
                if (fontHeight == -1)
                {
                    fontHeight = this.Font.Height;
                    this.Properties.SetInteger(PropFontHeight, fontHeight);
                }
                return fontHeight;
            }
            set
            {
                this.Properties.SetInteger(PropFontHeight, value);
            }
        }

        [DispId(-513), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ControlForeColorDescr")]
        public virtual System.Drawing.Color ForeColor
        {
            get
            {
                System.Drawing.Color color = this.Properties.GetColor(PropForeColor);
                if (!color.IsEmpty)
                {
                    return color;
                }
                Control parentInternal = this.ParentInternal;
                if ((parentInternal != null) && parentInternal.CanAccessProperties)
                {
                    return parentInternal.ForeColor;
                }
                System.Drawing.Color empty = System.Drawing.Color.Empty;
                if (this.IsActiveX)
                {
                    empty = this.ActiveXAmbientForeColor;
                }
                if (empty.IsEmpty)
                {
                    AmbientProperties ambientPropertiesService = this.AmbientPropertiesService;
                    if (ambientPropertiesService != null)
                    {
                        empty = ambientPropertiesService.ForeColor;
                    }
                }
                if (!empty.IsEmpty)
                {
                    return empty;
                }
                return DefaultForeColor;
            }
            set
            {
                System.Drawing.Color foreColor = this.ForeColor;
                if (!value.IsEmpty || this.Properties.ContainsObject(PropForeColor))
                {
                    this.Properties.SetColor(PropForeColor, value);
                }
                if (!foreColor.Equals(this.ForeColor))
                {
                    this.OnForeColorChanged(EventArgs.Empty);
                }
            }
        }

        [DispId(-515), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlHandleDescr"), Browsable(false)]
        public IntPtr Handle
        {
            get
            {
                if ((checkForIllegalCrossThreadCalls && !inCrossThreadSafeCall) && this.InvokeRequired)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("IllegalCrossThreadCall", new object[] { this.Name }));
                }
                if (!this.IsHandleCreated)
                {
                    this.CreateHandle();
                }
                return this.HandleInternal;
            }
        }

        internal IntPtr HandleInternal
        {
            get
            {
                return this.window.Handle;
            }
        }

        [System.Windows.Forms.SRDescription("ControlHasChildrenDescr"), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasChildren
        {
            get
            {
                ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                return ((controls != null) && (controls.Count > 0));
            }
        }

        internal virtual bool HasMenu
        {
            get
            {
                return false;
            }
        }

        [System.Windows.Forms.SRDescription("ControlHeightDescr"), EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatLayout"), Browsable(false)]
        public int Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.SetBounds(this.x, this.y, this.width, value, BoundsSpecified.Height);
            }
        }

        internal bool HostedInWin32DialogManager
        {
            get
            {
                if (!this.GetState(0x1000000))
                {
                    Control topMostParent = this.TopMostParent;
                    if (this != topMostParent)
                    {
                        this.SetState(0x2000000, topMostParent.HostedInWin32DialogManager);
                    }
                    else
                    {
                        IntPtr parent = System.Windows.Forms.UnsafeNativeMethods.GetParent(new HandleRef(this, this.Handle));
                        IntPtr handle = parent;
                        StringBuilder lpClassName = new StringBuilder(0x20);
                        this.SetState(0x2000000, false);
                        while (parent != IntPtr.Zero)
                        {
                            int num = System.Windows.Forms.UnsafeNativeMethods.GetClassName(new HandleRef(null, handle), null, 0);
                            if (num > lpClassName.Capacity)
                            {
                                lpClassName.Capacity = num + 5;
                            }
                            System.Windows.Forms.UnsafeNativeMethods.GetClassName(new HandleRef(null, handle), lpClassName, lpClassName.Capacity);
                            if (lpClassName.ToString() == "#32770")
                            {
                                this.SetState(0x2000000, true);
                                break;
                            }
                            handle = parent;
                            parent = System.Windows.Forms.UnsafeNativeMethods.GetParent(new HandleRef(null, parent));
                        }
                    }
                    this.SetState(0x1000000, true);
                }
                return this.GetState(0x2000000);
            }
        }

        private static bool IgnoreWmImeNotify
        {
            get
            {
                return ignoreWmImeNotify;
            }
            set
            {
                ignoreWmImeNotify = value;
            }
        }

        [AmbientValue(-1), System.Windows.Forms.SRDescription("ControlIMEModeDescr"), System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true)]
        public System.Windows.Forms.ImeMode ImeMode
        {
            get
            {
                System.Windows.Forms.ImeMode imeModeBase = this.ImeModeBase;
                if (imeModeBase == System.Windows.Forms.ImeMode.OnHalf)
                {
                    imeModeBase = System.Windows.Forms.ImeMode.On;
                }
                return imeModeBase;
            }
            set
            {
                this.ImeModeBase = value;
            }
        }

        protected virtual System.Windows.Forms.ImeMode ImeModeBase
        {
            get
            {
                return this.CachedImeMode;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, -1, 12))
                {
                    throw new InvalidEnumArgumentException("ImeMode", (int) value, typeof(System.Windows.Forms.ImeMode));
                }
                System.Windows.Forms.ImeMode cachedImeMode = this.CachedImeMode;
                this.CachedImeMode = value;
                if (cachedImeMode != value)
                {
                    Control control = null;
                    if (!base.DesignMode && (ImeModeConversion.InputLanguageTable != ImeModeConversion.UnsupportedTable))
                    {
                        if (this.Focused)
                        {
                            control = this;
                        }
                        else if (this.ContainsFocus)
                        {
                            control = FromChildHandleInternal(System.Windows.Forms.UnsafeNativeMethods.GetFocus());
                        }
                        if ((control != null) && control.CanEnableIme)
                        {
                            this.DisableImeModeChangedCount++;
                            try
                            {
                                control.UpdateImeContextMode();
                            }
                            finally
                            {
                                this.DisableImeModeChangedCount--;
                            }
                        }
                    }
                    this.VerifyImeModeChanged(cachedImeMode, this.CachedImeMode);
                }
            }
        }

        private bool ImeSupported
        {
            get
            {
                return (this.DefaultImeMode != System.Windows.Forms.ImeMode.Disable);
            }
        }

        internal int ImeWmCharsToIgnore
        {
            get
            {
                return this.Properties.GetInteger(PropImeWmCharsToIgnore);
            }
            set
            {
                if (this.ImeWmCharsToIgnore != -1)
                {
                    this.Properties.SetInteger(PropImeWmCharsToIgnore, value);
                }
            }
        }

        internal IntPtr InternalHandle
        {
            get
            {
                if (!this.IsHandleCreated)
                {
                    return IntPtr.Zero;
                }
                return this.Handle;
            }
        }

        [System.Windows.Forms.SRDescription("ControlInvokeRequiredDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool InvokeRequired
        {
            get
            {
                using (new MultithreadSafeCallScope())
                {
                    HandleRef ref2;
                    int num;
                    if (this.IsHandleCreated)
                    {
                        ref2 = new HandleRef(this, this.Handle);
                    }
                    else
                    {
                        Control wrapper = this.FindMarshalingControl();
                        if (!wrapper.IsHandleCreated)
                        {
                            return false;
                        }
                        ref2 = new HandleRef(wrapper, wrapper.Handle);
                    }
                    int windowThreadProcessId = System.Windows.Forms.SafeNativeMethods.GetWindowThreadProcessId(ref2, out num);
                    int currentThreadId = System.Windows.Forms.SafeNativeMethods.GetCurrentThreadId();
                    return (windowThreadProcessId != currentThreadId);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ControlIsAccessibleDescr")]
        public bool IsAccessible
        {
            get
            {
                return this.GetState(0x100000);
            }
            set
            {
                this.SetState(0x100000, value);
            }
        }

        internal bool IsActiveX
        {
            get
            {
                return this.GetState2(0x400);
            }
        }

        internal virtual bool IsContainerControl
        {
            get
            {
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlDisposedDescr")]
        public bool IsDisposed
        {
            get
            {
                return this.GetState(0x800);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlHandleCreatedDescr"), EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool IsHandleCreated
        {
            get
            {
                return (this.window.Handle != IntPtr.Zero);
            }
        }

        internal bool IsIEParent
        {
            get
            {
                if (!this.IsActiveX)
                {
                    return false;
                }
                return this.ActiveXInstance.IsIE;
            }
        }

        internal bool IsLayoutSuspended
        {
            get
            {
                return (this.layoutSuspendCount > 0);
            }
        }

        [System.Windows.Forms.SRDescription("IsMirroredDescr"), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatLayout")]
        public bool IsMirrored
        {
            get
            {
                if (!this.IsHandleCreated)
                {
                    System.Windows.Forms.CreateParams createParams = this.CreateParams;
                    this.SetState(0x40000000, (createParams.ExStyle & 0x400000) != 0);
                }
                return this.GetState(0x40000000);
            }
        }

        internal virtual bool IsMnemonicsListenerAxSourced
        {
            get
            {
                return false;
            }
        }

        internal bool IsWindowObscured
        {
            get
            {
                if (!this.IsHandleCreated || !this.Visible)
                {
                    return false;
                }
                bool flag = false;
                System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                Control parentInternal = this.ParentInternal;
                if (parentInternal != null)
                {
                    while (parentInternal.ParentInternal != null)
                    {
                        parentInternal = parentInternal.ParentInternal;
                    }
                }
                System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(this, this.Handle), ref rect);
                using (System.Drawing.Region region = new System.Drawing.Region(Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom)))
                {
                    IntPtr ptr2;
                    IntPtr handle;
                    if (parentInternal != null)
                    {
                        handle = parentInternal.Handle;
                    }
                    else
                    {
                        handle = this.Handle;
                    }
                    for (IntPtr ptr = handle; (ptr2 = System.Windows.Forms.UnsafeNativeMethods.GetWindow(new HandleRef(null, ptr), 3)) != IntPtr.Zero; ptr = ptr2)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(null, ptr2), ref rect);
                        Rectangle rectangle = Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
                        if (System.Windows.Forms.SafeNativeMethods.IsWindowVisible(new HandleRef(null, ptr2)))
                        {
                            region.Exclude(rectangle);
                        }
                    }
                    using (Graphics graphics = this.CreateGraphics())
                    {
                        flag = region.IsEmpty(graphics);
                    }
                }
                return flag;
            }
        }

        private bool LastCanEnableIme
        {
            get
            {
                bool flag;
                int integer = this.Properties.GetInteger(PropLastCanEnableIme, out flag);
                if (flag)
                {
                    return (integer == 1);
                }
                return true;
            }
            set
            {
                this.Properties.SetInteger(PropLastCanEnableIme, value ? 1 : 0);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual System.Windows.Forms.Layout.LayoutEngine LayoutEngine
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return DefaultLayout.Instance;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatLayout"), EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRDescription("ControlLeftDescr")]
        public int Left
        {
            get
            {
                return this.x;
            }
            set
            {
                this.SetBounds(value, this.y, this.width, this.height, BoundsSpecified.X);
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ControlLocationDescr"), Localizable(true)]
        public Point Location
        {
            get
            {
                return new Point(this.x, this.y);
            }
            set
            {
                this.SetBounds(value.X, value.Y, this.width, this.height, BoundsSpecified.Location);
            }
        }

        [System.Windows.Forms.SRDescription("ControlMarginDescr"), System.Windows.Forms.SRCategory("CatLayout"), Localizable(true)]
        public System.Windows.Forms.Padding Margin
        {
            get
            {
                return CommonProperties.GetMargin(this);
            }
            set
            {
                value = LayoutUtils.ClampNegativePaddingToZero(value);
                if (value != this.Margin)
                {
                    CommonProperties.SetMargin(this, value);
                    this.OnMarginChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ControlMaximumSizeDescr"), AmbientValue(typeof(System.Drawing.Size), "0, 0")]
        public virtual System.Drawing.Size MaximumSize
        {
            get
            {
                return CommonProperties.GetMaximumSize(this, this.DefaultMaximumSize);
            }
            set
            {
                if (value == System.Drawing.Size.Empty)
                {
                    CommonProperties.ClearMaximumSize(this);
                }
                else if (value != this.MaximumSize)
                {
                    CommonProperties.SetMaximumSize(this, value);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ControlMinimumSizeDescr")]
        public virtual System.Drawing.Size MinimumSize
        {
            get
            {
                return CommonProperties.GetMinimumSize(this, this.DefaultMinimumSize);
            }
            set
            {
                if (value != this.MinimumSize)
                {
                    CommonProperties.SetMinimumSize(this, value);
                }
            }
        }

        public static Keys ModifierKeys
        {
            get
            {
                Keys none = Keys.None;
                if (System.Windows.Forms.UnsafeNativeMethods.GetKeyState(0x10) < 0)
                {
                    none |= Keys.Shift;
                }
                if (System.Windows.Forms.UnsafeNativeMethods.GetKeyState(0x11) < 0)
                {
                    none |= Keys.Control;
                }
                if (System.Windows.Forms.UnsafeNativeMethods.GetKeyState(0x12) < 0)
                {
                    none |= Keys.Alt;
                }
                return none;
            }
        }

        public static System.Windows.Forms.MouseButtons MouseButtons
        {
            get
            {
                System.Windows.Forms.MouseButtons none = System.Windows.Forms.MouseButtons.None;
                if (System.Windows.Forms.UnsafeNativeMethods.GetKeyState(1) < 0)
                {
                    none |= System.Windows.Forms.MouseButtons.Left;
                }
                if (System.Windows.Forms.UnsafeNativeMethods.GetKeyState(2) < 0)
                {
                    none |= System.Windows.Forms.MouseButtons.Right;
                }
                if (System.Windows.Forms.UnsafeNativeMethods.GetKeyState(4) < 0)
                {
                    none |= System.Windows.Forms.MouseButtons.Middle;
                }
                if (System.Windows.Forms.UnsafeNativeMethods.GetKeyState(5) < 0)
                {
                    none |= System.Windows.Forms.MouseButtons.XButton1;
                }
                if (System.Windows.Forms.UnsafeNativeMethods.GetKeyState(6) < 0)
                {
                    none |= System.Windows.Forms.MouseButtons.XButton2;
                }
                return none;
            }
        }

        public static Point MousePosition
        {
            get
            {
                System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT();
                System.Windows.Forms.UnsafeNativeMethods.GetCursorPos(pt);
                return new Point(pt.x, pt.y);
            }
        }

        [Browsable(false)]
        public string Name
        {
            get
            {
                string name = (string) this.Properties.GetObject(PropName);
                if (string.IsNullOrEmpty(name))
                {
                    if (this.Site != null)
                    {
                        name = this.Site.Name;
                    }
                    if (name == null)
                    {
                        name = "";
                    }
                }
                return name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.Properties.SetObject(PropName, null);
                }
                else
                {
                    this.Properties.SetObject(PropName, value);
                }
            }
        }

        private AccessibleObject NcAccessibilityObject
        {
            get
            {
                AccessibleObject obj2 = (AccessibleObject) this.Properties.GetObject(PropNcAccessibility);
                if (obj2 == null)
                {
                    obj2 = new ControlAccessibleObject(this, 0);
                    this.Properties.SetObject(PropNcAccessibility, obj2);
                }
                return obj2;
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), Localizable(true), System.Windows.Forms.SRDescription("ControlPaddingDescr")]
        public System.Windows.Forms.Padding Padding
        {
            get
            {
                return CommonProperties.GetPadding(this, this.DefaultPadding);
            }
            set
            {
                if (value != this.Padding)
                {
                    CommonProperties.SetPadding(this, value);
                    this.SetState(0x800000, true);
                    using (new LayoutTransaction(this.ParentInternal, this, PropertyNames.Padding))
                    {
                        this.OnPaddingChanged(EventArgs.Empty);
                    }
                    if (this.GetState(0x800000))
                    {
                        LayoutTransaction.DoLayout(this, this, PropertyNames.Padding);
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("ControlParentDescr"), System.Windows.Forms.SRCategory("CatBehavior"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control Parent
        {
            get
            {
                System.Windows.Forms.IntSecurity.GetParent.Demand();
                return this.ParentInternal;
            }
            set
            {
                this.ParentInternal = value;
            }
        }

        internal ContainerControl ParentContainerControl
        {
            get
            {
                for (Control control = this.ParentInternal; control != null; control = control.ParentInternal)
                {
                    if (control is ContainerControl)
                    {
                        return (control as ContainerControl);
                    }
                }
                return null;
            }
        }

        internal virtual Control ParentInternal
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.parent;
            }
            set
            {
                if (this.parent != value)
                {
                    if (value != null)
                    {
                        value.Controls.Add(this);
                    }
                    else
                    {
                        this.parent.Controls.Remove(this);
                    }
                }
            }
        }

        [Browsable(false)]
        public System.Drawing.Size PreferredSize
        {
            get
            {
                return this.GetPreferredSize(System.Drawing.Size.Empty);
            }
        }

        [System.Windows.Forms.SRDescription("ControlProductNameDescr"), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ProductName
        {
            get
            {
                return this.VersionInfo.ProductName;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRDescription("ControlProductVersionDescr")]
        public string ProductVersion
        {
            get
            {
                return this.VersionInfo.ProductVersion;
            }
        }

        protected static System.Windows.Forms.ImeMode PropagatingImeMode
        {
            get
            {
                if (propagatingImeMode == System.Windows.Forms.ImeMode.Inherit)
                {
                    System.Windows.Forms.ImeMode inherit = System.Windows.Forms.ImeMode.Inherit;
                    IntPtr focus = System.Windows.Forms.UnsafeNativeMethods.GetFocus();
                    if (focus != IntPtr.Zero)
                    {
                        inherit = ImeContext.GetImeMode(focus);
                        if (inherit == System.Windows.Forms.ImeMode.Disable)
                        {
                            focus = System.Windows.Forms.UnsafeNativeMethods.GetAncestor(new HandleRef(null, focus), 2);
                            if (focus != IntPtr.Zero)
                            {
                                inherit = ImeContext.GetImeMode(focus);
                            }
                        }
                    }
                    PropagatingImeMode = inherit;
                }
                return propagatingImeMode;
            }
            private set
            {
                if (propagatingImeMode != value)
                {
                    switch (value)
                    {
                        case System.Windows.Forms.ImeMode.NoControl:
                        case System.Windows.Forms.ImeMode.Disable:
                            return;
                    }
                    propagatingImeMode = value;
                }
            }
        }

        internal PropertyStore Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.propertyStore;
            }
        }

        internal System.Drawing.Color RawBackColor
        {
            get
            {
                return this.Properties.GetColor(PropBackColor);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRCategory("CatBehavior"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlRecreatingHandleDescr")]
        public bool RecreatingHandle
        {
            get
            {
                return ((this.state & 0x10) != 0);
            }
        }

        private Control ReflectParent
        {
            get
            {
                return this.reflectParent;
            }
            set
            {
                if (value != null)
                {
                    value.AddReflectChild();
                }
                Control reflectParent = this.ReflectParent;
                this.reflectParent = value;
                if (reflectParent != null)
                {
                    reflectParent.RemoveReflectChild();
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRCategory("CatLayout"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlRegionDescr")]
        public System.Drawing.Region Region
        {
            get
            {
                return (System.Drawing.Region) this.Properties.GetObject(PropRegion);
            }
            set
            {
                if (this.GetState(0x80000))
                {
                    System.Windows.Forms.IntSecurity.ChangeWindowRegionForTopLevel.Demand();
                }
                System.Drawing.Region region = this.Region;
                if (region != value)
                {
                    this.Properties.SetObject(PropRegion, value);
                    if (region != null)
                    {
                        region.Dispose();
                    }
                    if (this.IsHandleCreated)
                    {
                        IntPtr zero = IntPtr.Zero;
                        try
                        {
                            if (value != null)
                            {
                                zero = this.GetHRgn(value);
                            }
                            if (this.IsActiveX)
                            {
                                zero = this.ActiveXMergeRegion(zero);
                            }
                            if (System.Windows.Forms.UnsafeNativeMethods.SetWindowRgn(new HandleRef(this, this.Handle), new HandleRef(this, zero), System.Windows.Forms.SafeNativeMethods.IsWindowVisible(new HandleRef(this, this.Handle))) != 0)
                            {
                                zero = IntPtr.Zero;
                            }
                        }
                        finally
                        {
                            if (zero != IntPtr.Zero)
                            {
                                System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, zero));
                            }
                        }
                    }
                    this.OnRegionChanged(EventArgs.Empty);
                }
            }
        }

        [Obsolete("This property has been deprecated. Please use RightToLeft instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected internal bool RenderRightToLeft
        {
            get
            {
                return true;
            }
        }

        internal virtual bool RenderTransparencyWithVisualStyles
        {
            get
            {
                return false;
            }
        }

        internal bool RenderTransparent
        {
            get
            {
                return (this.GetStyle(ControlStyles.SupportsTransparentBackColor) && (this.BackColor.A < 0xff));
            }
        }

        internal BoundsSpecified RequiredScaling
        {
            get
            {
                if ((this.requiredScaling & 0x10) != 0)
                {
                    return (((BoundsSpecified) this.requiredScaling) & BoundsSpecified.All);
                }
                return BoundsSpecified.None;
            }
            set
            {
                byte num = (byte) (this.requiredScaling & 0x10);
                this.requiredScaling = (byte) ((value & BoundsSpecified.All) | ((BoundsSpecified) num));
            }
        }

        internal bool RequiredScalingEnabled
        {
            get
            {
                return ((this.requiredScaling & 0x10) != 0);
            }
            set
            {
                byte num = (byte) (this.requiredScaling & 15);
                this.requiredScaling = num;
                if (value)
                {
                    this.requiredScaling = (byte) (this.requiredScaling | 0x10);
                }
            }
        }

        [System.Windows.Forms.SRDescription("ControlResizeRedrawDescr")]
        protected bool ResizeRedraw
        {
            get
            {
                return this.GetStyle(ControlStyles.ResizeRedraw);
            }
            set
            {
                this.SetStyle(ControlStyles.ResizeRedraw, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlRightDescr"), System.Windows.Forms.SRCategory("CatLayout"), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Right
        {
            get
            {
                return (this.x + this.width);
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("ControlRightToLeftDescr"), AmbientValue(2), System.Windows.Forms.SRCategory("CatAppearance")]
        public virtual System.Windows.Forms.RightToLeft RightToLeft
        {
            get
            {
                bool flag;
                int integer = this.Properties.GetInteger(PropRightToLeft, out flag);
                if (!flag)
                {
                    integer = 2;
                }
                if (integer == 2)
                {
                    Control parentInternal = this.ParentInternal;
                    if (parentInternal != null)
                    {
                        integer = (int) parentInternal.RightToLeft;
                    }
                    else
                    {
                        integer = (int) this.DefaultRightToLeft;
                    }
                }
                return (System.Windows.Forms.RightToLeft) integer;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("RightToLeft", (int) value, typeof(System.Windows.Forms.RightToLeft));
                }
                System.Windows.Forms.RightToLeft rightToLeft = this.RightToLeft;
                if (this.Properties.ContainsInteger(PropRightToLeft) || (value != System.Windows.Forms.RightToLeft.Inherit))
                {
                    this.Properties.SetInteger(PropRightToLeft, (int) value);
                }
                if (rightToLeft != this.RightToLeft)
                {
                    using (new LayoutTransaction(this, this, PropertyNames.RightToLeft))
                    {
                        this.OnRightToLeftChanged(EventArgs.Empty);
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual bool ScaleChildren
        {
            get
            {
                return true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal bool ShouldAutoValidate
        {
            get
            {
                return (GetAutoValidateForControl(this) != AutoValidate.Disable);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal virtual bool ShowFocusCues
        {
            get
            {
                if (!this.IsHandleCreated)
                {
                    return true;
                }
                if ((this.uiCuesState & 15) == 0)
                {
                    if (SystemInformation.MenuAccessKeysUnderlined)
                    {
                        this.uiCuesState |= 2;
                    }
                    else
                    {
                        this.uiCuesState |= 1;
                        int num = 0x30000;
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.TopMostParent, this.TopMostParent.Handle), 0x127, (IntPtr) (num | 1), IntPtr.Zero);
                    }
                }
                return ((this.uiCuesState & 15) == 2);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected internal virtual bool ShowKeyboardCues
        {
            get
            {
                if (!this.IsHandleCreated || base.DesignMode)
                {
                    return true;
                }
                if ((this.uiCuesState & 240) == 0)
                {
                    if (SystemInformation.MenuAccessKeysUnderlined)
                    {
                        this.uiCuesState |= 0x20;
                    }
                    else
                    {
                        int num = 0x30000;
                        this.uiCuesState |= 0x10;
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.TopMostParent, this.TopMostParent.Handle), 0x127, (IntPtr) (num | 1), IntPtr.Zero);
                    }
                }
                return ((this.uiCuesState & 240) == 0x20);
            }
        }

        internal virtual int ShowParams
        {
            get
            {
                return 5;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override ISite Site
        {
            get
            {
                return base.Site;
            }
            set
            {
                AmbientProperties ambientPropertiesService = this.AmbientPropertiesService;
                AmbientProperties service = null;
                if (value != null)
                {
                    service = (AmbientProperties) value.GetService(typeof(AmbientProperties));
                }
                if (ambientPropertiesService != service)
                {
                    bool flag = !this.Properties.ContainsObject(PropFont);
                    bool flag2 = !this.Properties.ContainsObject(PropBackColor);
                    bool flag3 = !this.Properties.ContainsObject(PropForeColor);
                    bool flag4 = !this.Properties.ContainsObject(PropCursor);
                    System.Drawing.Font font = null;
                    System.Drawing.Color empty = System.Drawing.Color.Empty;
                    System.Drawing.Color foreColor = System.Drawing.Color.Empty;
                    System.Windows.Forms.Cursor cursor = null;
                    if (flag)
                    {
                        font = this.Font;
                    }
                    if (flag2)
                    {
                        empty = this.BackColor;
                    }
                    if (flag3)
                    {
                        foreColor = this.ForeColor;
                    }
                    if (flag4)
                    {
                        cursor = this.Cursor;
                    }
                    this.Properties.SetObject(PropAmbientPropertiesService, service);
                    base.Site = value;
                    if (flag && !font.Equals(this.Font))
                    {
                        this.OnFontChanged(EventArgs.Empty);
                    }
                    if (flag3 && !foreColor.Equals(this.ForeColor))
                    {
                        this.OnForeColorChanged(EventArgs.Empty);
                    }
                    if (flag2 && !empty.Equals(this.BackColor))
                    {
                        this.OnBackColorChanged(EventArgs.Empty);
                    }
                    if (flag4 && cursor.Equals(this.Cursor))
                    {
                        this.OnCursorChanged(EventArgs.Empty);
                    }
                }
                else
                {
                    base.Site = value;
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), Localizable(true), System.Windows.Forms.SRDescription("ControlSizeDescr")]
        public System.Drawing.Size Size
        {
            get
            {
                return new System.Drawing.Size(this.width, this.height);
            }
            set
            {
                this.SetBounds(this.x, this.y, value.Width, value.Height, BoundsSpecified.Size);
            }
        }

        internal virtual bool SupportsUseCompatibleTextRendering
        {
            get
            {
                return false;
            }
        }

        ArrangedElementCollection IArrangedElement.Children
        {
            get
            {
                ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                if (controls == null)
                {
                    return ArrangedElementCollection.Empty;
                }
                return controls;
            }
        }

        IArrangedElement IArrangedElement.Container
        {
            get
            {
                return this.ParentInternal;
            }
        }

        bool IArrangedElement.ParticipatesInLayout
        {
            get
            {
                return this.GetState(2);
            }
        }

        PropertyStore IArrangedElement.Properties
        {
            get
            {
                return this.Properties;
            }
        }

        [MergableProperty(false), Localizable(true), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ControlTabIndexDescr")]
        public int TabIndex
        {
            get
            {
                if (this.tabIndex != -1)
                {
                    return this.tabIndex;
                }
                return 0;
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "TabIndex", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("TabIndex", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                if (this.tabIndex != value)
                {
                    this.tabIndex = value;
                    this.OnTabIndexChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(true), DispId(-516), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ControlTabStopDescr")]
        public bool TabStop
        {
            get
            {
                return this.TabStopInternal;
            }
            set
            {
                if (this.TabStop != value)
                {
                    this.TabStopInternal = value;
                    if (this.IsHandleCreated)
                    {
                        this.SetWindowStyle(0x10000, value);
                    }
                    this.OnTabStopChanged(EventArgs.Empty);
                }
            }
        }

        internal bool TabStopInternal
        {
            get
            {
                return ((this.state & 8) != 0);
            }
            set
            {
                if (this.TabStopInternal != value)
                {
                    this.SetState(8, value);
                }
            }
        }

        [Localizable(false), Bindable(true), TypeConverter(typeof(StringConverter)), System.Windows.Forms.SRCategory("CatData"), System.Windows.Forms.SRDescription("ControlTagDescr"), DefaultValue((string) null)]
        public object Tag
        {
            get
            {
                return this.Properties.GetObject(PropUserData);
            }
            set
            {
                this.Properties.SetObject(PropUserData, value);
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("ControlTextDescr"), Bindable(true), System.Windows.Forms.SRCategory("CatAppearance"), DispId(-517)]
        public virtual string Text
        {
            get
            {
                if (!this.CacheTextInternal)
                {
                    return this.WindowText;
                }
                if (this.text != null)
                {
                    return this.text;
                }
                return "";
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if (value != this.Text)
                {
                    if (this.CacheTextInternal)
                    {
                        this.text = value;
                    }
                    this.WindowText = value;
                    this.OnTextChanged(EventArgs.Empty);
                    if (this.IsMnemonicsListenerAxSourced)
                    {
                        for (Control control = this; control != null; control = control.ParentInternal)
                        {
                            ActiveXImpl impl = (ActiveXImpl) control.Properties.GetObject(PropActiveXImpl);
                            if (impl != null)
                            {
                                impl.UpdateAccelTable();
                                return;
                            }
                        }
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlTopDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public int Top
        {
            get
            {
                return this.y;
            }
            set
            {
                this.SetBounds(this.x, value, this.width, this.height, BoundsSpecified.Y);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlTopLevelControlDescr"), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public Control TopLevelControl
        {
            get
            {
                System.Windows.Forms.IntSecurity.GetParent.Demand();
                return this.TopLevelControlInternal;
            }
        }

        internal Control TopLevelControlInternal
        {
            get
            {
                Control parentInternal = this;
                while ((parentInternal != null) && !parentInternal.GetTopLevel())
                {
                    parentInternal = parentInternal.ParentInternal;
                }
                return parentInternal;
            }
        }

        internal Control TopMostParent
        {
            get
            {
                Control parentInternal = this;
                while (parentInternal.ParentInternal != null)
                {
                    parentInternal = parentInternal.ParentInternal;
                }
                return parentInternal;
            }
        }

        internal bool UseCompatibleTextRenderingInt
        {
            get
            {
                if (this.Properties.ContainsInteger(PropUseCompatibleTextRendering))
                {
                    bool flag;
                    int integer = this.Properties.GetInteger(PropUseCompatibleTextRendering, out flag);
                    if (flag)
                    {
                        return (integer == 1);
                    }
                }
                return UseCompatibleTextRenderingDefault;
            }
            set
            {
                if (this.SupportsUseCompatibleTextRendering && (this.UseCompatibleTextRenderingInt != value))
                {
                    this.Properties.SetInteger(PropUseCompatibleTextRendering, value ? 1 : 0);
                    LayoutTransaction.DoLayoutIf(this.AutoSize, this.ParentInternal, this, PropertyNames.UseCompatibleTextRendering);
                    this.Invalidate();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ControlUseWaitCursorDescr"), DefaultValue(false), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public bool UseWaitCursor
        {
            get
            {
                return this.GetState(0x400);
            }
            set
            {
                if (this.GetState(0x400) != value)
                {
                    this.SetState(0x400, value);
                    ControlCollection controls = (ControlCollection) this.Properties.GetObject(PropControlsCollection);
                    if (controls != null)
                    {
                        for (int i = 0; i < controls.Count; i++)
                        {
                            controls[i].UseWaitCursor = value;
                        }
                    }
                }
            }
        }

        internal bool ValidationCancelled
        {
            get
            {
                if (this.GetState(0x10000000))
                {
                    return true;
                }
                Control parentInternal = this.ParentInternal;
                return ((parentInternal != null) && parentInternal.ValidationCancelled);
            }
            set
            {
                this.SetState(0x10000000, value);
            }
        }

        private ControlVersionInfo VersionInfo
        {
            get
            {
                ControlVersionInfo info = (ControlVersionInfo) this.Properties.GetObject(PropControlVersionInfo);
                if (info == null)
                {
                    info = new ControlVersionInfo(this);
                    this.Properties.SetObject(PropControlVersionInfo, info);
                }
                return info;
            }
        }

        [Localizable(true), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ControlVisibleDescr")]
        public bool Visible
        {
            get
            {
                return this.GetVisibleCore();
            }
            set
            {
                this.SetVisibleCore(value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlWidthDescr"), Browsable(false), EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRCategory("CatLayout"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.SetBounds(this.x, this.y, value, this.height, BoundsSpecified.Width);
            }
        }

        private int WindowExStyle
        {
            get
            {
                return (int) ((long) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, this.Handle), -20));
            }
            set
            {
                System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, this.Handle), -20, new HandleRef(null, (IntPtr) value));
            }
        }

        internal int WindowStyle
        {
            get
            {
                return (int) ((long) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, this.Handle), -16));
            }
            set
            {
                System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, this.Handle), -16, new HandleRef(null, (IntPtr) value));
            }
        }

        [Browsable(false), System.Windows.Forms.SRCategory("CatBehavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlWindowTargetDescr"), EditorBrowsable(EditorBrowsableState.Never)]
        public IWindowTarget WindowTarget
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                return this.window.WindowTarget;
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            set
            {
                this.window.WindowTarget = value;
            }
        }

        internal virtual string WindowText
        {
            get
            {
                if (!this.IsHandleCreated)
                {
                    if (this.text == null)
                    {
                        return "";
                    }
                    return this.text;
                }
                using (new MultithreadSafeCallScope())
                {
                    int windowTextLength = System.Windows.Forms.SafeNativeMethods.GetWindowTextLength(new HandleRef(this.window, this.Handle));
                    if (SystemInformation.DbcsEnabled)
                    {
                        windowTextLength = (windowTextLength * 2) + 1;
                    }
                    StringBuilder lpString = new StringBuilder(windowTextLength + 1);
                    System.Windows.Forms.UnsafeNativeMethods.GetWindowText(new HandleRef(this.window, this.Handle), lpString, lpString.Capacity);
                    return lpString.ToString();
                }
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if (!this.WindowText.Equals(value))
                {
                    if (this.IsHandleCreated)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetWindowText(new HandleRef(this.window, this.Handle), value);
                    }
                    else if (value.Length == 0)
                    {
                        this.text = null;
                    }
                    else
                    {
                        this.text = value;
                    }
                }
            }
        }

        private class ActiveXFontMarshaler : ICustomMarshaler
        {
            private static Control.ActiveXFontMarshaler instance;

            public void CleanUpManagedData(object obj)
            {
            }

            public void CleanUpNativeData(IntPtr pObj)
            {
                Marshal.Release(pObj);
            }

            internal static ICustomMarshaler GetInstance(string cookie)
            {
                if (instance == null)
                {
                    instance = new Control.ActiveXFontMarshaler();
                }
                return instance;
            }

            public int GetNativeDataSize()
            {
                return -1;
            }

            public IntPtr MarshalManagedToNative(object obj)
            {
                IntPtr ptr2;
                Font font = (Font) obj;
                System.Windows.Forms.NativeMethods.tagFONTDESC fontdesc = new System.Windows.Forms.NativeMethods.tagFONTDESC();
                System.Windows.Forms.NativeMethods.LOGFONT logFont = new System.Windows.Forms.NativeMethods.LOGFONT();
                System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
                try
                {
                    font.ToLogFont(logFont);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                fontdesc.lpstrName = font.Name;
                fontdesc.cySize = (long) (font.SizeInPoints * 10000f);
                fontdesc.sWeight = (short) logFont.lfWeight;
                fontdesc.sCharset = logFont.lfCharSet;
                fontdesc.fItalic = font.Italic;
                fontdesc.fUnderline = font.Underline;
                fontdesc.fStrikethrough = font.Strikeout;
                Guid gUID = typeof(System.Windows.Forms.UnsafeNativeMethods.IFont).GUID;
                IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(System.Windows.Forms.UnsafeNativeMethods.OleCreateFontIndirect(fontdesc, ref gUID));
                int hr = Marshal.QueryInterface(iUnknownForObject, ref gUID, out ptr2);
                Marshal.Release(iUnknownForObject);
                if (System.Windows.Forms.NativeMethods.Failed(hr))
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
                return ptr2;
            }

            public object MarshalNativeToManaged(IntPtr pObj)
            {
                System.Windows.Forms.UnsafeNativeMethods.IFont objectForIUnknown = (System.Windows.Forms.UnsafeNativeMethods.IFont) Marshal.GetObjectForIUnknown(pObj);
                IntPtr zero = IntPtr.Zero;
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    zero = objectForIUnknown.GetHFont();
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                Font defaultFont = null;
                System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
                try
                {
                    defaultFont = Font.FromHfont(zero);
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                    defaultFont = Control.DefaultFont;
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                return defaultFont;
            }
        }

        private class ActiveXImpl : MarshalByRefObject, IWindowTarget
        {
            private short accelCount = -1;
            private IntPtr accelTable;
            private BitVector32 activeXState;
            private static readonly int adjustingRect = BitVector32.CreateMask(uiDead);
            private System.Windows.Forms.NativeMethods.COMRECT adjustRect;
            private ArrayList adviseList;
            private Control.AmbientProperty[] ambientProperties;
            private static System.Windows.Forms.NativeMethods.tagOLEVERB[] axVerbs;
            private static readonly int changingExtents = BitVector32.CreateMask(eventsFrozen);
            private static bool checkedIE;
            private System.Windows.Forms.UnsafeNativeMethods.IOleClientSite clientSite;
            private IntPtr clipRegion;
            private Control control;
            private IWindowTarget controlWindowTarget;
            private static readonly int eventsFrozen = BitVector32.CreateMask(viewAdvisePrimeFirst);
            private static int globalActiveXCount = 0;
            private static readonly int hiMetricPerInch = 0x9ec;
            private IntPtr hwndParent;
            private static readonly int inPlaceActive = BitVector32.CreateMask(isDirty);
            private System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame inPlaceFrame;
            private System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceUIWindow inPlaceUiWindow;
            private static readonly int inPlaceVisible = BitVector32.CreateMask(inPlaceActive);
            private static readonly int isDirty = BitVector32.CreateMask(saving);
            private static bool isIE;
            private static Point logPixels = Point.Empty;
            private static readonly int saving = BitVector32.CreateMask(changingExtents);
            private static readonly int uiActive = BitVector32.CreateMask(inPlaceVisible);
            private static readonly int uiDead = BitVector32.CreateMask(uiActive);
            private static readonly int viewAdviseOnlyOnce = BitVector32.CreateMask();
            private static readonly int viewAdvisePrimeFirst = BitVector32.CreateMask(viewAdviseOnlyOnce);
            private IAdviseSink viewAdviseSink;

            internal ActiveXImpl(Control control)
            {
                this.control = control;
                this.controlWindowTarget = control.WindowTarget;
                control.WindowTarget = this;
                this.adviseList = new ArrayList();
                this.activeXState = new BitVector32();
                this.ambientProperties = new Control.AmbientProperty[] { new Control.AmbientProperty("Font", -703), new Control.AmbientProperty("BackColor", -701), new Control.AmbientProperty("ForeColor", -704) };
            }

            internal int Advise(IAdviseSink pAdvSink)
            {
                this.adviseList.Add(pAdvSink);
                return this.adviseList.Count;
            }

            private void CallParentPropertyChanged(Control control, string propName)
            {
                switch (propName)
                {
                    case "BackColor":
                        control.OnParentBackColorChanged(EventArgs.Empty);
                        return;

                    case "BackgroundImage":
                        control.OnParentBackgroundImageChanged(EventArgs.Empty);
                        return;

                    case "BindingContext":
                        control.OnParentBindingContextChanged(EventArgs.Empty);
                        return;

                    case "Enabled":
                        control.OnParentEnabledChanged(EventArgs.Empty);
                        return;

                    case "Font":
                        control.OnParentFontChanged(EventArgs.Empty);
                        return;

                    case "ForeColor":
                        control.OnParentForeColorChanged(EventArgs.Empty);
                        return;

                    case "RightToLeft":
                        control.OnParentRightToLeftChanged(EventArgs.Empty);
                        return;

                    case "Visible":
                        control.OnParentVisibleChanged(EventArgs.Empty);
                        return;
                }
            }

            internal void Close(int dwSaveOption)
            {
                if (this.activeXState[inPlaceActive])
                {
                    this.InPlaceDeactivate();
                }
                if (((dwSaveOption == 0) || (dwSaveOption == 2)) && this.activeXState[isDirty])
                {
                    if (this.clientSite != null)
                    {
                        this.clientSite.SaveObject();
                    }
                    this.SendOnSave();
                }
            }

            internal void DoVerb(int iVerb, IntPtr lpmsg, System.Windows.Forms.UnsafeNativeMethods.IOleClientSite pActiveSite, int lindex, IntPtr hwndParent, System.Windows.Forms.NativeMethods.COMRECT lprcPosRect)
            {
                switch (iVerb)
                {
                    case -5:
                    case -4:
                    case -1:
                    case 0:
                        this.InPlaceActivate(iVerb);
                        if (lpmsg != IntPtr.Zero)
                        {
                            System.Windows.Forms.NativeMethods.MSG msg = (System.Windows.Forms.NativeMethods.MSG) System.Windows.Forms.UnsafeNativeMethods.PtrToStructure(lpmsg, typeof(System.Windows.Forms.NativeMethods.MSG));
                            Control wrapper = this.control;
                            if (((msg.hwnd != this.control.Handle) && (msg.message >= 0x200)) && (msg.message <= 0x20a))
                            {
                                IntPtr handle = (msg.hwnd == IntPtr.Zero) ? hwndParent : msg.hwnd;
                                System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT {
                                    x = System.Windows.Forms.NativeMethods.Util.LOWORD(msg.lParam),
                                    y = System.Windows.Forms.NativeMethods.Util.HIWORD(msg.lParam)
                                };
                                System.Windows.Forms.UnsafeNativeMethods.MapWindowPoints(new HandleRef(null, handle), new HandleRef(this.control, this.control.Handle), pt, 1);
                                Control childAtPoint = wrapper.GetChildAtPoint(new Point(pt.x, pt.y));
                                if ((childAtPoint != null) && (childAtPoint != wrapper))
                                {
                                    System.Windows.Forms.UnsafeNativeMethods.MapWindowPoints(new HandleRef(wrapper, wrapper.Handle), new HandleRef(childAtPoint, childAtPoint.Handle), pt, 1);
                                    wrapper = childAtPoint;
                                }
                                msg.lParam = System.Windows.Forms.NativeMethods.Util.MAKELPARAM(pt.x, pt.y);
                            }
                            if ((msg.message == 0x100) && (msg.wParam == ((IntPtr) 9)))
                            {
                                wrapper.SelectNextControl(null, Control.ModifierKeys != Keys.Shift, true, true, true);
                                return;
                            }
                            wrapper.SendMessage(msg.message, msg.wParam, msg.lParam);
                        }
                        return;

                    case -3:
                        this.UIDeactivate();
                        this.InPlaceDeactivate();
                        if (this.activeXState[inPlaceVisible])
                        {
                            this.SetInPlaceVisible(false);
                        }
                        return;
                }
                ThrowHr(-2147467263);
            }

            internal void Draw(int dwDrawAspect, int lindex, IntPtr pvAspect, System.Windows.Forms.NativeMethods.tagDVTARGETDEVICE ptd, IntPtr hdcTargetDev, IntPtr hdcDraw, System.Windows.Forms.NativeMethods.COMRECT prcBounds, System.Windows.Forms.NativeMethods.COMRECT lprcWBounds, IntPtr pfnContinue, int dwContinue)
            {
                int num3 = dwDrawAspect;
                if (((num3 != 1) && (num3 != 0x10)) && (num3 != 0x20))
                {
                    ThrowHr(-2147221397);
                }
                int objectType = System.Windows.Forms.UnsafeNativeMethods.GetObjectType(new HandleRef(null, hdcDraw));
                if (objectType == 4)
                {
                    ThrowHr(-2147221184);
                }
                System.Windows.Forms.NativeMethods.POINT point = new System.Windows.Forms.NativeMethods.POINT();
                System.Windows.Forms.NativeMethods.POINT point2 = new System.Windows.Forms.NativeMethods.POINT();
                System.Windows.Forms.NativeMethods.SIZE size = new System.Windows.Forms.NativeMethods.SIZE();
                System.Windows.Forms.NativeMethods.SIZE size2 = new System.Windows.Forms.NativeMethods.SIZE();
                int nMapMode = 1;
                if (!this.control.IsHandleCreated)
                {
                    this.control.CreateHandle();
                }
                if (prcBounds != null)
                {
                    System.Windows.Forms.NativeMethods.RECT lpRect = new System.Windows.Forms.NativeMethods.RECT(prcBounds.left, prcBounds.top, prcBounds.right, prcBounds.bottom);
                    System.Windows.Forms.SafeNativeMethods.LPtoDP(new HandleRef(null, hdcDraw), ref lpRect, 2);
                    nMapMode = System.Windows.Forms.SafeNativeMethods.SetMapMode(new HandleRef(null, hdcDraw), 8);
                    System.Windows.Forms.SafeNativeMethods.SetWindowOrgEx(new HandleRef(null, hdcDraw), 0, 0, point2);
                    System.Windows.Forms.SafeNativeMethods.SetWindowExtEx(new HandleRef(null, hdcDraw), this.control.Width, this.control.Height, size);
                    System.Windows.Forms.SafeNativeMethods.SetViewportOrgEx(new HandleRef(null, hdcDraw), lpRect.left, lpRect.top, point);
                    System.Windows.Forms.SafeNativeMethods.SetViewportExtEx(new HandleRef(null, hdcDraw), lpRect.right - lpRect.left, lpRect.bottom - lpRect.top, size2);
                }
                try
                {
                    IntPtr lparam = (IntPtr) 30;
                    if (objectType != 12)
                    {
                        this.control.SendMessage(0x317, hdcDraw, lparam);
                    }
                    else
                    {
                        this.control.PrintToMetaFile(new HandleRef(null, hdcDraw), lparam);
                    }
                }
                finally
                {
                    if (prcBounds != null)
                    {
                        System.Windows.Forms.SafeNativeMethods.SetWindowOrgEx(new HandleRef(null, hdcDraw), point2.x, point2.y, null);
                        System.Windows.Forms.SafeNativeMethods.SetWindowExtEx(new HandleRef(null, hdcDraw), size.cx, size.cy, null);
                        System.Windows.Forms.SafeNativeMethods.SetViewportOrgEx(new HandleRef(null, hdcDraw), point.x, point.y, null);
                        System.Windows.Forms.SafeNativeMethods.SetViewportExtEx(new HandleRef(null, hdcDraw), size2.cx, size2.cy, null);
                        System.Windows.Forms.SafeNativeMethods.SetMapMode(new HandleRef(null, hdcDraw), nMapMode);
                    }
                }
            }

            internal static int EnumVerbs(out System.Windows.Forms.UnsafeNativeMethods.IEnumOLEVERB e)
            {
                if (axVerbs == null)
                {
                    System.Windows.Forms.NativeMethods.tagOLEVERB goleverb = new System.Windows.Forms.NativeMethods.tagOLEVERB();
                    System.Windows.Forms.NativeMethods.tagOLEVERB goleverb2 = new System.Windows.Forms.NativeMethods.tagOLEVERB();
                    System.Windows.Forms.NativeMethods.tagOLEVERB goleverb3 = new System.Windows.Forms.NativeMethods.tagOLEVERB();
                    System.Windows.Forms.NativeMethods.tagOLEVERB goleverb4 = new System.Windows.Forms.NativeMethods.tagOLEVERB();
                    System.Windows.Forms.NativeMethods.tagOLEVERB goleverb5 = new System.Windows.Forms.NativeMethods.tagOLEVERB();
                    System.Windows.Forms.NativeMethods.tagOLEVERB goleverb6 = new System.Windows.Forms.NativeMethods.tagOLEVERB();
                    goleverb.lVerb = -1;
                    goleverb2.lVerb = -5;
                    goleverb3.lVerb = -4;
                    goleverb4.lVerb = -3;
                    goleverb5.lVerb = 0;
                    goleverb6.lVerb = -7;
                    goleverb6.lpszVerbName = System.Windows.Forms.SR.GetString("AXProperties");
                    goleverb6.grfAttribs = 2;
                    axVerbs = new System.Windows.Forms.NativeMethods.tagOLEVERB[] { goleverb, goleverb2, goleverb3, goleverb4, goleverb5 };
                }
                e = new Control.ActiveXVerbEnum(axVerbs);
                return 0;
            }

            private static byte[] FromBase64WrappedString(string text)
            {
                if (text.IndexOfAny(new char[] { ' ', '\r', '\n' }) == -1)
                {
                    return Convert.FromBase64String(text);
                }
                StringBuilder builder = new StringBuilder(text.Length);
                for (int i = 0; i < text.Length; i++)
                {
                    char ch = text[i];
                    if (((ch != '\n') && (ch != '\r')) && (ch != ' '))
                    {
                        builder.Append(text[i]);
                    }
                }
                return Convert.FromBase64String(builder.ToString());
            }

            internal void GetAdvise(int[] paspects, int[] padvf, IAdviseSink[] pAdvSink)
            {
                if (paspects != null)
                {
                    paspects[0] = 1;
                }
                if (padvf != null)
                {
                    padvf[0] = 0;
                    if (this.activeXState[viewAdviseOnlyOnce])
                    {
                        padvf[0] |= 4;
                    }
                    if (this.activeXState[viewAdvisePrimeFirst])
                    {
                        padvf[0] |= 2;
                    }
                }
                if (pAdvSink != null)
                {
                    pAdvSink[0] = this.viewAdviseSink;
                }
            }

            private bool GetAmbientProperty(int dispid, ref object obj)
            {
                if (this.clientSite is System.Windows.Forms.UnsafeNativeMethods.IDispatch)
                {
                    System.Windows.Forms.UnsafeNativeMethods.IDispatch clientSite = (System.Windows.Forms.UnsafeNativeMethods.IDispatch) this.clientSite;
                    object[] pVarResult = new object[1];
                    Guid empty = Guid.Empty;
                    int hr = -2147467259;
                    System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                    try
                    {
                        hr = clientSite.Invoke(dispid, ref empty, System.Windows.Forms.NativeMethods.LOCALE_USER_DEFAULT, 2, new System.Windows.Forms.NativeMethods.tagDISPPARAMS(), pVarResult, null, null);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    if (System.Windows.Forms.NativeMethods.Succeeded(hr))
                    {
                        obj = pVarResult[0];
                        return true;
                    }
                }
                return false;
            }

            internal System.Windows.Forms.UnsafeNativeMethods.IOleClientSite GetClientSite()
            {
                return this.clientSite;
            }

            [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
            internal int GetControlInfo(System.Windows.Forms.NativeMethods.tagCONTROLINFO pCI)
            {
                if (this.accelCount == -1)
                {
                    ArrayList mnemonicList = new ArrayList();
                    this.GetMnemonicList(this.control, mnemonicList);
                    this.accelCount = (short) mnemonicList.Count;
                    if (this.accelCount > 0)
                    {
                        int num = System.Windows.Forms.UnsafeNativeMethods.SizeOf(typeof(System.Windows.Forms.NativeMethods.ACCEL));
                        IntPtr handle = Marshal.AllocHGlobal((int) ((num * this.accelCount) * 2));
                        try
                        {
                            System.Windows.Forms.NativeMethods.ACCEL structure = new System.Windows.Forms.NativeMethods.ACCEL {
                                cmd = 0
                            };
                            this.accelCount = 0;
                            foreach (char ch in mnemonicList)
                            {
                                IntPtr ptr = (IntPtr) (((long) handle) + (this.accelCount * num));
                                if ((ch >= 'A') && (ch <= 'Z'))
                                {
                                    structure.fVirt = 0x11;
                                    structure.key = (short) (System.Windows.Forms.UnsafeNativeMethods.VkKeyScan(ch) & 0xff);
                                    Marshal.StructureToPtr(structure, ptr, false);
                                    this.accelCount = (short) (this.accelCount + 1);
                                    ptr = (IntPtr) (((long) ptr) + num);
                                    structure.fVirt = 0x15;
                                    Marshal.StructureToPtr(structure, ptr, false);
                                }
                                else
                                {
                                    structure.fVirt = 0x11;
                                    short num2 = System.Windows.Forms.UnsafeNativeMethods.VkKeyScan(ch);
                                    if ((num2 & 0x100) != 0)
                                    {
                                        structure.fVirt = (byte) (structure.fVirt | 4);
                                    }
                                    structure.key = (short) (num2 & 0xff);
                                    Marshal.StructureToPtr(structure, ptr, false);
                                }
                                structure.cmd = (short) (structure.cmd + 1);
                                this.accelCount = (short) (this.accelCount + 1);
                            }
                            if (this.accelTable != IntPtr.Zero)
                            {
                                System.Windows.Forms.UnsafeNativeMethods.DestroyAcceleratorTable(new HandleRef(this, this.accelTable));
                                this.accelTable = IntPtr.Zero;
                            }
                            this.accelTable = System.Windows.Forms.UnsafeNativeMethods.CreateAcceleratorTable(new HandleRef(null, handle), this.accelCount);
                        }
                        finally
                        {
                            if (handle != IntPtr.Zero)
                            {
                                Marshal.FreeHGlobal(handle);
                            }
                        }
                    }
                }
                pCI.cAccel = this.accelCount;
                pCI.hAccel = this.accelTable;
                return 0;
            }

            private static System.Type GetDefaultEventsInterface(System.Type controlType)
            {
                System.Type type = null;
                object[] customAttributes = controlType.GetCustomAttributes(typeof(ComSourceInterfacesAttribute), false);
                if (customAttributes.Length > 0)
                {
                    ComSourceInterfacesAttribute attribute = (ComSourceInterfacesAttribute) customAttributes[0];
                    string name = attribute.Value.Split(new char[1])[0];
                    type = controlType.Module.Assembly.GetType(name, false);
                    if (type == null)
                    {
                        type = System.Type.GetType(name, false);
                    }
                }
                return type;
            }

            internal void GetExtent(int dwDrawAspect, System.Windows.Forms.NativeMethods.tagSIZEL pSizel)
            {
                if ((dwDrawAspect & 1) != 0)
                {
                    Size size = this.control.Size;
                    Point point = this.PixelToHiMetric(size.Width, size.Height);
                    pSizel.cx = point.X;
                    pSizel.cy = point.Y;
                }
                else
                {
                    ThrowHr(-2147221397);
                }
            }

            private void GetMnemonicList(Control control, ArrayList mnemonicList)
            {
                char mnemonic = WindowsFormsUtils.GetMnemonic(control.Text, true);
                if (mnemonic != '\0')
                {
                    mnemonicList.Add(mnemonic);
                }
                foreach (Control control2 in control.Controls)
                {
                    if (control2 != null)
                    {
                        this.GetMnemonicList(control2, mnemonicList);
                    }
                }
            }

            private string GetStreamName()
            {
                string fullName = this.control.GetType().FullName;
                int length = fullName.Length;
                if (length > 0x1f)
                {
                    fullName = fullName.Substring(length - 0x1f);
                }
                return fullName;
            }

            internal int GetWindow(out IntPtr hwnd)
            {
                if (!this.activeXState[inPlaceActive])
                {
                    hwnd = IntPtr.Zero;
                    return -2147467259;
                }
                hwnd = this.control.Handle;
                return 0;
            }

            private Point HiMetricToPixel(int x, int y)
            {
                return new Point { X = ((this.LogPixels.X * x) + (hiMetricPerInch / 2)) / hiMetricPerInch, Y = ((this.LogPixels.Y * y) + (hiMetricPerInch / 2)) / hiMetricPerInch };
            }

            [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
            internal void InPlaceActivate(int verb)
            {
                System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite clientSite = this.clientSite as System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite;
                if (clientSite != null)
                {
                    if (!this.activeXState[inPlaceActive])
                    {
                        int hr = clientSite.CanInPlaceActivate();
                        if (hr != 0)
                        {
                            if (System.Windows.Forms.NativeMethods.Succeeded(hr))
                            {
                                hr = -2147467259;
                            }
                            ThrowHr(hr);
                        }
                        clientSite.OnInPlaceActivate();
                        this.activeXState[inPlaceActive] = true;
                    }
                    if (!this.activeXState[inPlaceVisible])
                    {
                        System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame frame;
                        System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceUIWindow window;
                        System.Windows.Forms.NativeMethods.tagOIFI lpFrameInfo = new System.Windows.Forms.NativeMethods.tagOIFI {
                            cb = System.Windows.Forms.UnsafeNativeMethods.SizeOf(typeof(System.Windows.Forms.NativeMethods.tagOIFI))
                        };
                        IntPtr zero = IntPtr.Zero;
                        zero = clientSite.GetWindow();
                        System.Windows.Forms.NativeMethods.COMRECT lprcPosRect = new System.Windows.Forms.NativeMethods.COMRECT();
                        System.Windows.Forms.NativeMethods.COMRECT lprcClipRect = new System.Windows.Forms.NativeMethods.COMRECT();
                        if ((this.inPlaceUiWindow != null) && System.Windows.Forms.UnsafeNativeMethods.IsComObject(this.inPlaceUiWindow))
                        {
                            System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(this.inPlaceUiWindow);
                            this.inPlaceUiWindow = null;
                        }
                        if ((this.inPlaceFrame != null) && System.Windows.Forms.UnsafeNativeMethods.IsComObject(this.inPlaceFrame))
                        {
                            System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(this.inPlaceFrame);
                            this.inPlaceFrame = null;
                        }
                        clientSite.GetWindowContext(out frame, out window, lprcPosRect, lprcClipRect, lpFrameInfo);
                        this.SetObjectRects(lprcPosRect, lprcClipRect);
                        this.inPlaceFrame = frame;
                        this.inPlaceUiWindow = window;
                        this.hwndParent = zero;
                        System.Windows.Forms.UnsafeNativeMethods.SetParent(new HandleRef(this.control, this.control.Handle), new HandleRef(null, zero));
                        this.control.CreateControl();
                        this.clientSite.ShowObject();
                        this.SetInPlaceVisible(true);
                    }
                    if (((verb == 0) || (verb == -4)) && !this.activeXState[uiActive])
                    {
                        this.activeXState[uiActive] = true;
                        clientSite.OnUIActivate();
                        if (!this.control.ContainsFocus)
                        {
                            this.control.FocusInternal();
                        }
                        this.inPlaceFrame.SetActiveObject(this.control, null);
                        if (this.inPlaceUiWindow != null)
                        {
                            this.inPlaceUiWindow.SetActiveObject(this.control, null);
                        }
                        int num2 = this.inPlaceFrame.SetBorderSpace(null);
                        if ((System.Windows.Forms.NativeMethods.Failed(num2) && (num2 != -2147221491)) && ((num2 != -2147221087) && (num2 != -2147467263)))
                        {
                            System.Windows.Forms.UnsafeNativeMethods.ThrowExceptionForHR(num2);
                        }
                        if (this.inPlaceUiWindow != null)
                        {
                            num2 = this.inPlaceFrame.SetBorderSpace(null);
                            if ((System.Windows.Forms.NativeMethods.Failed(num2) && (num2 != -2147221491)) && ((num2 != -2147221087) && (num2 != -2147467263)))
                            {
                                System.Windows.Forms.UnsafeNativeMethods.ThrowExceptionForHR(num2);
                            }
                        }
                    }
                }
            }

            internal void InPlaceDeactivate()
            {
                if (this.activeXState[inPlaceActive])
                {
                    if (this.activeXState[uiActive])
                    {
                        this.UIDeactivate();
                    }
                    this.activeXState[inPlaceActive] = false;
                    this.activeXState[inPlaceVisible] = false;
                    System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite clientSite = this.clientSite as System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite;
                    if (clientSite != null)
                    {
                        System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                        try
                        {
                            clientSite.OnInPlaceDeactivate();
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                    this.control.Visible = false;
                    this.hwndParent = IntPtr.Zero;
                    if ((this.inPlaceUiWindow != null) && System.Windows.Forms.UnsafeNativeMethods.IsComObject(this.inPlaceUiWindow))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(this.inPlaceUiWindow);
                        this.inPlaceUiWindow = null;
                    }
                    if ((this.inPlaceFrame != null) && System.Windows.Forms.UnsafeNativeMethods.IsComObject(this.inPlaceFrame))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(this.inPlaceFrame);
                        this.inPlaceFrame = null;
                    }
                }
            }

            internal int IsDirty()
            {
                if (this.activeXState[isDirty])
                {
                    return 0;
                }
                return 1;
            }

            private bool IsResourceProp(PropertyDescriptor prop)
            {
                TypeConverter converter = prop.Converter;
                System.Type[] typeArray = new System.Type[] { typeof(string), typeof(byte[]) };
                foreach (System.Type type in typeArray)
                {
                    if (converter.CanConvertTo(type) && converter.CanConvertFrom(type))
                    {
                        return false;
                    }
                }
                return (prop.GetValue(this.control) is ISerializable);
            }

            internal void Load(System.Windows.Forms.UnsafeNativeMethods.IStorage stg)
            {
                System.Windows.Forms.UnsafeNativeMethods.IStream stream = null;
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    stream = stg.OpenStream(this.GetStreamName(), IntPtr.Zero, 0x10, 0);
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147287038)
                    {
                        throw;
                    }
                    stream = stg.OpenStream(base.GetType().FullName, IntPtr.Zero, 0x10, 0);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                this.Load(stream);
                stream = null;
                if (System.Windows.Forms.UnsafeNativeMethods.IsComObject(stg))
                {
                    System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(stg);
                }
            }

            internal void Load(System.Windows.Forms.UnsafeNativeMethods.IStream stream)
            {
                PropertyBagStream pPropBag = new PropertyBagStream();
                pPropBag.Read(stream);
                this.Load(pPropBag, null);
                if (System.Windows.Forms.UnsafeNativeMethods.IsComObject(stream))
                {
                    System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(stream);
                }
            }

            internal void Load(System.Windows.Forms.UnsafeNativeMethods.IPropertyBag pPropBag, System.Windows.Forms.UnsafeNativeMethods.IErrorLog pErrorLog)
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this.control, new Attribute[] { DesignerSerializationVisibilityAttribute.Visible });
                for (int i = 0; i < properties.Count; i++)
                {
                    try
                    {
                        object pVar = null;
                        int hr = -2147467259;
                        System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                        try
                        {
                            hr = pPropBag.Read(properties[i].Name, ref pVar, pErrorLog);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                        if (System.Windows.Forms.NativeMethods.Succeeded(hr) && (pVar != null))
                        {
                            string str = null;
                            int errorCode = 0;
                            try
                            {
                                if (pVar.GetType() != typeof(string))
                                {
                                    pVar = Convert.ToString(pVar, CultureInfo.InvariantCulture);
                                }
                                if (this.IsResourceProp(properties[i]))
                                {
                                    MemoryStream serializationStream = new MemoryStream(Convert.FromBase64String(pVar.ToString()));
                                    BinaryFormatter formatter = new BinaryFormatter();
                                    properties[i].SetValue(this.control, formatter.Deserialize(serializationStream));
                                }
                                else
                                {
                                    TypeConverter converter = properties[i].Converter;
                                    object obj3 = null;
                                    if (converter.CanConvertFrom(typeof(string)))
                                    {
                                        obj3 = converter.ConvertFromInvariantString(pVar.ToString());
                                    }
                                    else if (converter.CanConvertFrom(typeof(byte[])))
                                    {
                                        string text = pVar.ToString();
                                        obj3 = converter.ConvertFrom(null, CultureInfo.InvariantCulture, FromBase64WrappedString(text));
                                    }
                                    properties[i].SetValue(this.control, obj3);
                                }
                            }
                            catch (Exception exception)
                            {
                                str = exception.ToString();
                                if (exception is ExternalException)
                                {
                                    errorCode = ((ExternalException) exception).ErrorCode;
                                }
                                else
                                {
                                    errorCode = -2147467259;
                                }
                            }
                            if ((str != null) && (pErrorLog != null))
                            {
                                System.Windows.Forms.NativeMethods.tagEXCEPINFO gexcepinfo = new System.Windows.Forms.NativeMethods.tagEXCEPINFO {
                                    bstrSource = this.control.GetType().FullName,
                                    bstrDescription = str,
                                    scode = errorCode
                                };
                                pErrorLog.AddError(properties[i].Name, gexcepinfo);
                            }
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception2))
                        {
                            throw;
                        }
                    }
                }
                if (System.Windows.Forms.UnsafeNativeMethods.IsComObject(pPropBag))
                {
                    System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(pPropBag);
                }
            }

            private Control.AmbientProperty LookupAmbient(int dispid)
            {
                for (int i = 0; i < this.ambientProperties.Length; i++)
                {
                    if (this.ambientProperties[i].DispID == dispid)
                    {
                        return this.ambientProperties[i];
                    }
                }
                return this.ambientProperties[0];
            }

            internal IntPtr MergeRegion(IntPtr region)
            {
                if (this.clipRegion == IntPtr.Zero)
                {
                    return region;
                }
                if (region == IntPtr.Zero)
                {
                    return this.clipRegion;
                }
                try
                {
                    IntPtr handle = System.Windows.Forms.SafeNativeMethods.CreateRectRgn(0, 0, 0, 0);
                    try
                    {
                        System.Windows.Forms.SafeNativeMethods.CombineRgn(new HandleRef(null, handle), new HandleRef(null, region), new HandleRef(this, this.clipRegion), 4);
                        System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, region));
                    }
                    catch
                    {
                        System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, handle));
                        throw;
                    }
                    return handle;
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                    return region;
                }
            }

            internal void OnAmbientPropertyChange(int dispID)
            {
                if (dispID != -1)
                {
                    for (int i = 0; i < this.ambientProperties.Length; i++)
                    {
                        if (this.ambientProperties[i].DispID == dispID)
                        {
                            this.ambientProperties[i].ResetValue();
                            this.CallParentPropertyChanged(this.control, this.ambientProperties[i].Name);
                            return;
                        }
                    }
                    object obj2 = new object();
                    int num3 = dispID;
                    if (num3 == -713)
                    {
                        IButtonControl control = this.control as IButtonControl;
                        if ((control != null) && this.GetAmbientProperty(-713, ref obj2))
                        {
                            control.NotifyDefault((bool) obj2);
                        }
                    }
                    else if ((num3 == -710) && this.GetAmbientProperty(-710, ref obj2))
                    {
                        this.activeXState[uiDead] = (bool) obj2;
                    }
                }
                else
                {
                    for (int j = 0; j < this.ambientProperties.Length; j++)
                    {
                        this.ambientProperties[j].ResetValue();
                        this.CallParentPropertyChanged(this.control, this.ambientProperties[j].Name);
                    }
                }
            }

            internal void OnDocWindowActivate(int fActivate)
            {
                if ((this.activeXState[uiActive] && (fActivate != 0)) && (this.inPlaceFrame != null))
                {
                    int num;
                    System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                    try
                    {
                        num = this.inPlaceFrame.SetBorderSpace(null);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    if ((System.Windows.Forms.NativeMethods.Failed(num) && (num != -2147221087)) && (num != -2147467263))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.ThrowExceptionForHR(num);
                    }
                }
            }

            internal void OnFocus(bool focus)
            {
                if (this.activeXState[inPlaceActive] && (this.clientSite is System.Windows.Forms.UnsafeNativeMethods.IOleControlSite))
                {
                    System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                    try
                    {
                        ((System.Windows.Forms.UnsafeNativeMethods.IOleControlSite) this.clientSite).OnFocus(focus ? 1 : 0);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
                if ((focus && this.activeXState[inPlaceActive]) && !this.activeXState[uiActive])
                {
                    this.InPlaceActivate(-4);
                }
            }

            private Point PixelToHiMetric(int x, int y)
            {
                return new Point { X = ((hiMetricPerInch * x) + (this.LogPixels.X >> 1)) / this.LogPixels.X, Y = ((hiMetricPerInch * y) + (this.LogPixels.Y >> 1)) / this.LogPixels.Y };
            }

            internal void QuickActivate(System.Windows.Forms.UnsafeNativeMethods.tagQACONTAINER pQaContainer, System.Windows.Forms.UnsafeNativeMethods.tagQACONTROL pQaControl)
            {
                int num;
                this.LookupAmbient(-701).Value = ColorTranslator.FromOle((int) pQaContainer.colorBack);
                this.LookupAmbient(-704).Value = ColorTranslator.FromOle((int) pQaContainer.colorFore);
                if (pQaContainer.pFont != null)
                {
                    Control.AmbientProperty ambient = this.LookupAmbient(-703);
                    System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                    try
                    {
                        Font font2 = Font.FromHfont(((System.Windows.Forms.UnsafeNativeMethods.IFont) pQaContainer.pFont).GetHFont());
                        ambient.Value = font2;
                    }
                    catch (Exception exception)
                    {
                        if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                        {
                            throw;
                        }
                        ambient.Value = null;
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
                pQaControl.cbSize = System.Windows.Forms.UnsafeNativeMethods.SizeOf(typeof(System.Windows.Forms.UnsafeNativeMethods.tagQACONTROL));
                this.SetClientSite(pQaContainer.pClientSite);
                if (pQaContainer.pAdviseSink != null)
                {
                    this.SetAdvise(1, 0, (IAdviseSink) pQaContainer.pAdviseSink);
                }
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    ((System.Windows.Forms.UnsafeNativeMethods.IOleObject) this.control).GetMiscStatus(1, out num);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                pQaControl.dwMiscStatus = num;
                if ((pQaContainer.pUnkEventSink != null) && (this.control is UserControl))
                {
                    System.Type defaultEventsInterface = GetDefaultEventsInterface(this.control.GetType());
                    if (defaultEventsInterface != null)
                    {
                        System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                        try
                        {
                            AdviseHelper.AdviseConnectionPoint(this.control, pQaContainer.pUnkEventSink, defaultEventsInterface, out pQaControl.dwEventCookie);
                        }
                        catch (Exception exception2)
                        {
                            if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception2))
                            {
                                throw;
                            }
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                }
                if ((pQaContainer.pPropertyNotifySink != null) && System.Windows.Forms.UnsafeNativeMethods.IsComObject(pQaContainer.pPropertyNotifySink))
                {
                    System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(pQaContainer.pPropertyNotifySink);
                }
                if ((pQaContainer.pUnkEventSink != null) && System.Windows.Forms.UnsafeNativeMethods.IsComObject(pQaContainer.pUnkEventSink))
                {
                    System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(pQaContainer.pUnkEventSink);
                }
            }

            internal void Save(System.Windows.Forms.UnsafeNativeMethods.IStorage stg, bool fSameAsLoad)
            {
                System.Windows.Forms.UnsafeNativeMethods.IStream stream = null;
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    stream = stg.CreateStream(this.GetStreamName(), 0x1011, 0, 0);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                this.Save(stream, true);
                System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(stream);
            }

            internal void Save(System.Windows.Forms.UnsafeNativeMethods.IStream stream, bool fClearDirty)
            {
                PropertyBagStream pPropBag = new PropertyBagStream();
                this.Save(pPropBag, fClearDirty, false);
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    pPropBag.Write(stream);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                if (System.Windows.Forms.UnsafeNativeMethods.IsComObject(stream))
                {
                    System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(stream);
                }
            }

            [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
            internal void Save(System.Windows.Forms.UnsafeNativeMethods.IPropertyBag pPropBag, bool fClearDirty, bool fSaveAllProperties)
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this.control, new Attribute[] { DesignerSerializationVisibilityAttribute.Visible });
                for (int i = 0; i < properties.Count; i++)
                {
                    if (fSaveAllProperties || properties[i].ShouldSerializeValue(this.control))
                    {
                        object obj2;
                        if (this.IsResourceProp(properties[i]))
                        {
                            MemoryStream serializationStream = new MemoryStream();
                            new BinaryFormatter().Serialize(serializationStream, properties[i].GetValue(this.control));
                            byte[] buffer = new byte[(int) serializationStream.Length];
                            serializationStream.Position = 0L;
                            serializationStream.Read(buffer, 0, buffer.Length);
                            obj2 = Convert.ToBase64String(buffer);
                            pPropBag.Write(properties[i].Name, ref obj2);
                        }
                        else
                        {
                            TypeConverter converter = properties[i].Converter;
                            if (converter.CanConvertFrom(typeof(string)))
                            {
                                obj2 = converter.ConvertToInvariantString(properties[i].GetValue(this.control));
                                pPropBag.Write(properties[i].Name, ref obj2);
                            }
                            else if (converter.CanConvertFrom(typeof(byte[])))
                            {
                                byte[] inArray = (byte[]) converter.ConvertTo(null, CultureInfo.InvariantCulture, properties[i].GetValue(this.control), typeof(byte[]));
                                obj2 = Convert.ToBase64String(inArray);
                                pPropBag.Write(properties[i].Name, ref obj2);
                            }
                        }
                    }
                }
                if (System.Windows.Forms.UnsafeNativeMethods.IsComObject(pPropBag))
                {
                    System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(pPropBag);
                }
                if (fClearDirty)
                {
                    this.activeXState[isDirty] = false;
                }
            }

            private void SendOnSave()
            {
                int count = this.adviseList.Count;
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                for (int i = 0; i < count; i++)
                {
                    ((IAdviseSink) this.adviseList[i]).OnSave();
                }
            }

            internal void SetAdvise(int aspects, int advf, IAdviseSink pAdvSink)
            {
                if ((aspects & 1) == 0)
                {
                    ThrowHr(-2147221397);
                }
                this.activeXState[viewAdvisePrimeFirst] = (advf & 2) != 0;
                this.activeXState[viewAdviseOnlyOnce] = (advf & 4) != 0;
                if ((this.viewAdviseSink != null) && System.Windows.Forms.UnsafeNativeMethods.IsComObject(this.viewAdviseSink))
                {
                    System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(this.viewAdviseSink);
                }
                this.viewAdviseSink = pAdvSink;
                if (this.activeXState[viewAdvisePrimeFirst])
                {
                    this.ViewChanged();
                }
            }

            internal void SetClientSite(System.Windows.Forms.UnsafeNativeMethods.IOleClientSite value)
            {
                if (this.clientSite != null)
                {
                    if (value == null)
                    {
                        globalActiveXCount--;
                        if ((globalActiveXCount == 0) && this.IsIE)
                        {
                            new PermissionSet(PermissionState.Unrestricted).Assert();
                            try
                            {
                                MethodInfo info = typeof(SystemEvents).GetMethod("Shutdown", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, new System.Type[0], new ParameterModifier[0]);
                                if (info != null)
                                {
                                    info.Invoke(null, null);
                                }
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                            }
                        }
                    }
                    if (System.Windows.Forms.UnsafeNativeMethods.IsComObject(this.clientSite))
                    {
                        System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                        try
                        {
                            Marshal.FinalReleaseComObject(this.clientSite);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                }
                this.clientSite = value;
                if (this.clientSite != null)
                {
                    this.control.Site = new Control.AxSourcingSite(this.control, this.clientSite, "ControlAxSourcingSite");
                }
                else
                {
                    this.control.Site = null;
                }
                object obj2 = new object();
                if (this.GetAmbientProperty(-710, ref obj2))
                {
                    this.activeXState[uiDead] = (bool) obj2;
                }
                if ((this.control is IButtonControl) && this.GetAmbientProperty(-710, ref obj2))
                {
                    ((IButtonControl) this.control).NotifyDefault((bool) obj2);
                }
                if (this.clientSite == null)
                {
                    if (this.accelTable != IntPtr.Zero)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.DestroyAcceleratorTable(new HandleRef(this, this.accelTable));
                        this.accelTable = IntPtr.Zero;
                        this.accelCount = -1;
                    }
                    if (this.IsIE)
                    {
                        this.control.Dispose();
                    }
                }
                else
                {
                    globalActiveXCount++;
                    if ((globalActiveXCount == 1) && this.IsIE)
                    {
                        new PermissionSet(PermissionState.Unrestricted).Assert();
                        try
                        {
                            MethodInfo info2 = typeof(SystemEvents).GetMethod("Startup", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, new System.Type[0], new ParameterModifier[0]);
                            if (info2 != null)
                            {
                                info2.Invoke(null, null);
                            }
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                }
                this.control.OnTopMostActiveXParentChanged(EventArgs.Empty);
            }

            [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
            internal void SetExtent(int dwDrawAspect, System.Windows.Forms.NativeMethods.tagSIZEL pSizel)
            {
                if ((dwDrawAspect & 1) != 0)
                {
                    if (!this.activeXState[changingExtents])
                    {
                        this.activeXState[changingExtents] = true;
                        try
                        {
                            Size size = new Size(this.HiMetricToPixel(pSizel.cx, pSizel.cy));
                            if (this.activeXState[inPlaceActive])
                            {
                                System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite clientSite = this.clientSite as System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite;
                                if (clientSite != null)
                                {
                                    Rectangle bounds = this.control.Bounds;
                                    bounds.Location = new Point(bounds.X, bounds.Y);
                                    Size size2 = new Size(size.Width, size.Height);
                                    bounds.Width = size2.Width;
                                    bounds.Height = size2.Height;
                                    clientSite.OnPosRectChange(System.Windows.Forms.NativeMethods.COMRECT.FromXYWH(bounds.X, bounds.Y, bounds.Width, bounds.Height));
                                }
                            }
                            this.control.Size = size;
                            if (!this.control.Size.Equals(size))
                            {
                                this.activeXState[isDirty] = true;
                                if (!this.activeXState[inPlaceActive])
                                {
                                    this.ViewChanged();
                                }
                                if (!this.activeXState[inPlaceActive] && (this.clientSite != null))
                                {
                                    this.clientSite.RequestNewObjectLayout();
                                }
                            }
                        }
                        finally
                        {
                            this.activeXState[changingExtents] = false;
                        }
                    }
                }
                else
                {
                    ThrowHr(-2147221397);
                }
            }

            private void SetInPlaceVisible(bool visible)
            {
                this.activeXState[inPlaceVisible] = visible;
                this.control.Visible = visible;
            }

            internal void SetObjectRects(System.Windows.Forms.NativeMethods.COMRECT lprcPosRect, System.Windows.Forms.NativeMethods.COMRECT lprcClipRect)
            {
                Rectangle a = Rectangle.FromLTRB(lprcPosRect.left, lprcPosRect.top, lprcPosRect.right, lprcPosRect.bottom);
                if (this.activeXState[adjustingRect])
                {
                    this.adjustRect.left = a.X;
                    this.adjustRect.top = a.Y;
                    this.adjustRect.right = a.Width + a.X;
                    this.adjustRect.bottom = a.Height + a.Y;
                }
                else
                {
                    this.activeXState[adjustingRect] = true;
                    try
                    {
                        this.control.Bounds = a;
                    }
                    finally
                    {
                        this.activeXState[adjustingRect] = false;
                    }
                }
                bool flag = false;
                if (this.clipRegion != IntPtr.Zero)
                {
                    this.clipRegion = IntPtr.Zero;
                    flag = true;
                }
                if (lprcClipRect != null)
                {
                    Rectangle rectangle3;
                    Rectangle b = Rectangle.FromLTRB(lprcClipRect.left, lprcClipRect.top, lprcClipRect.right, lprcClipRect.bottom);
                    if (!b.IsEmpty)
                    {
                        rectangle3 = Rectangle.Intersect(a, b);
                    }
                    else
                    {
                        rectangle3 = a;
                    }
                    if (!rectangle3.Equals(a))
                    {
                        System.Windows.Forms.NativeMethods.RECT rect = System.Windows.Forms.NativeMethods.RECT.FromXYWH(rectangle3.X, rectangle3.Y, rectangle3.Width, rectangle3.Height);
                        IntPtr parent = System.Windows.Forms.UnsafeNativeMethods.GetParent(new HandleRef(this.control, this.control.Handle));
                        System.Windows.Forms.UnsafeNativeMethods.MapWindowPoints(new HandleRef(null, parent), new HandleRef(this.control, this.control.Handle), ref rect, 2);
                        this.clipRegion = System.Windows.Forms.SafeNativeMethods.CreateRectRgn(rect.left, rect.top, rect.right, rect.bottom);
                        flag = true;
                    }
                }
                if (flag && this.control.IsHandleCreated)
                {
                    IntPtr clipRegion = this.clipRegion;
                    Region region = this.control.Region;
                    if (region != null)
                    {
                        IntPtr hRgn = this.control.GetHRgn(region);
                        clipRegion = this.MergeRegion(hRgn);
                    }
                    System.Windows.Forms.UnsafeNativeMethods.SetWindowRgn(new HandleRef(this.control, this.control.Handle), new HandleRef(this, clipRegion), System.Windows.Forms.SafeNativeMethods.IsWindowVisible(new HandleRef(this.control, this.control.Handle)));
                }
                this.control.Invalidate();
            }

            void IWindowTarget.OnHandleChange(IntPtr newHandle)
            {
                this.controlWindowTarget.OnHandleChange(newHandle);
            }

            void IWindowTarget.OnMessage(ref Message m)
            {
                if (this.activeXState[uiDead])
                {
                    if ((m.Msg >= 0x200) && (m.Msg <= 0x20a))
                    {
                        return;
                    }
                    if ((m.Msg >= 0xa1) && (m.Msg <= 0xa9))
                    {
                        return;
                    }
                    if ((m.Msg >= 0x100) && (m.Msg <= 0x108))
                    {
                        return;
                    }
                }
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                this.controlWindowTarget.OnMessage(ref m);
            }

            internal static void ThrowHr(int hr)
            {
                ExternalException exception = new ExternalException(System.Windows.Forms.SR.GetString("ExternalException"), hr);
                throw exception;
            }

            internal int TranslateAccelerator(ref System.Windows.Forms.NativeMethods.MSG lpmsg)
            {
                bool flag = false;
                switch (lpmsg.message)
                {
                    case 0x100:
                    case 0x102:
                    case 260:
                    case 0x106:
                        flag = true;
                        break;
                }
                Message msg = Message.Create(lpmsg.hwnd, lpmsg.message, lpmsg.wParam, lpmsg.lParam);
                if (flag)
                {
                    Control ctl = Control.FromChildHandleInternal(lpmsg.hwnd);
                    if ((ctl != null) && ((this.control == ctl) || this.control.Contains(ctl)))
                    {
                        switch (Control.PreProcessControlMessageInternal(ctl, ref msg))
                        {
                            case PreProcessControlState.MessageProcessed:
                                lpmsg.message = msg.Msg;
                                lpmsg.wParam = msg.WParam;
                                lpmsg.lParam = msg.LParam;
                                return 0;

                            case PreProcessControlState.MessageNeeded:
                                System.Windows.Forms.UnsafeNativeMethods.TranslateMessage(ref lpmsg);
                                if (!System.Windows.Forms.SafeNativeMethods.IsWindowUnicode(new HandleRef(null, lpmsg.hwnd)))
                                {
                                    System.Windows.Forms.UnsafeNativeMethods.DispatchMessageA(ref lpmsg);
                                }
                                else
                                {
                                    System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(ref lpmsg);
                                }
                                return 0;
                        }
                    }
                }
                int num = 1;
                System.Windows.Forms.UnsafeNativeMethods.IOleControlSite clientSite = this.clientSite as System.Windows.Forms.UnsafeNativeMethods.IOleControlSite;
                if (clientSite != null)
                {
                    int grfModifiers = 0;
                    if (System.Windows.Forms.UnsafeNativeMethods.GetKeyState(0x10) < 0)
                    {
                        grfModifiers |= 1;
                    }
                    if (System.Windows.Forms.UnsafeNativeMethods.GetKeyState(0x11) < 0)
                    {
                        grfModifiers |= 2;
                    }
                    if (System.Windows.Forms.UnsafeNativeMethods.GetKeyState(0x12) < 0)
                    {
                        grfModifiers |= 4;
                    }
                    System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                    try
                    {
                        num = clientSite.TranslateAccelerator(ref lpmsg, grfModifiers);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
                return num;
            }

            internal int UIDeactivate()
            {
                if (this.activeXState[uiActive])
                {
                    this.activeXState[uiActive] = false;
                    if (this.inPlaceUiWindow != null)
                    {
                        this.inPlaceUiWindow.SetActiveObject(null, null);
                    }
                    System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                    this.inPlaceFrame.SetActiveObject(null, null);
                    System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite clientSite = this.clientSite as System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite;
                    if (clientSite != null)
                    {
                        clientSite.OnUIDeactivate(0);
                    }
                }
                return 0;
            }

            internal void Unadvise(int dwConnection)
            {
                if ((dwConnection > this.adviseList.Count) || (this.adviseList[dwConnection - 1] == null))
                {
                    ThrowHr(-2147221500);
                }
                IAdviseSink o = (IAdviseSink) this.adviseList[dwConnection - 1];
                this.adviseList.RemoveAt(dwConnection - 1);
                if ((o != null) && System.Windows.Forms.UnsafeNativeMethods.IsComObject(o))
                {
                    System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(o);
                }
            }

            internal void UpdateAccelTable()
            {
                this.accelCount = -1;
                System.Windows.Forms.UnsafeNativeMethods.IOleControlSite clientSite = this.clientSite as System.Windows.Forms.UnsafeNativeMethods.IOleControlSite;
                if (clientSite != null)
                {
                    System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                    clientSite.OnControlInfoChanged();
                }
            }

            internal void UpdateBounds(ref int x, ref int y, ref int width, ref int height, int flags)
            {
                if (!this.activeXState[adjustingRect] && this.activeXState[inPlaceVisible])
                {
                    System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite clientSite = this.clientSite as System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite;
                    if (clientSite != null)
                    {
                        System.Windows.Forms.NativeMethods.COMRECT lprcPosRect = new System.Windows.Forms.NativeMethods.COMRECT();
                        if ((flags & 2) != 0)
                        {
                            lprcPosRect.left = this.control.Left;
                            lprcPosRect.top = this.control.Top;
                        }
                        else
                        {
                            lprcPosRect.left = x;
                            lprcPosRect.top = y;
                        }
                        if ((flags & 1) != 0)
                        {
                            lprcPosRect.right = lprcPosRect.left + this.control.Width;
                            lprcPosRect.bottom = lprcPosRect.top + this.control.Height;
                        }
                        else
                        {
                            lprcPosRect.right = lprcPosRect.left + width;
                            lprcPosRect.bottom = lprcPosRect.top + height;
                        }
                        this.adjustRect = lprcPosRect;
                        this.activeXState[adjustingRect] = true;
                        System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                        try
                        {
                            clientSite.OnPosRectChange(lprcPosRect);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                            this.adjustRect = null;
                            this.activeXState[adjustingRect] = false;
                        }
                        if ((flags & 2) == 0)
                        {
                            x = lprcPosRect.left;
                            y = lprcPosRect.top;
                        }
                        if ((flags & 1) == 0)
                        {
                            width = lprcPosRect.right - lprcPosRect.left;
                            height = lprcPosRect.bottom - lprcPosRect.top;
                        }
                    }
                }
            }

            private void ViewChanged()
            {
                if ((this.viewAdviseSink != null) && !this.activeXState[saving])
                {
                    System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                    try
                    {
                        this.viewAdviseSink.OnViewChange(1, -1);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    if (this.activeXState[viewAdviseOnlyOnce])
                    {
                        if (System.Windows.Forms.UnsafeNativeMethods.IsComObject(this.viewAdviseSink))
                        {
                            System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(this.viewAdviseSink);
                        }
                        this.viewAdviseSink = null;
                    }
                }
            }

            internal void ViewChangedInternal()
            {
                this.ViewChanged();
            }

            [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            internal System.Drawing.Color AmbientBackColor
            {
                get
                {
                    Control.AmbientProperty ambient = this.LookupAmbient(-701);
                    if (ambient.Empty)
                    {
                        object obj2 = null;
                        if (this.GetAmbientProperty(-701, ref obj2) && (obj2 != null))
                        {
                            try
                            {
                                ambient.Value = ColorTranslator.FromOle(Convert.ToInt32(obj2, CultureInfo.InvariantCulture));
                            }
                            catch (Exception exception)
                            {
                                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                                {
                                    throw;
                                }
                            }
                        }
                    }
                    if (ambient.Value == null)
                    {
                        return System.Drawing.Color.Empty;
                    }
                    return (System.Drawing.Color) ambient.Value;
                }
            }

            [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            internal Font AmbientFont
            {
                get
                {
                    Control.AmbientProperty ambient = this.LookupAmbient(-703);
                    if (ambient.Empty)
                    {
                        object obj2 = null;
                        if (this.GetAmbientProperty(-703, ref obj2))
                        {
                            try
                            {
                                System.Windows.Forms.UnsafeNativeMethods.IFont font = (System.Windows.Forms.UnsafeNativeMethods.IFont) obj2;
                                System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
                                Font font2 = null;
                                try
                                {
                                    font2 = Font.FromHfont(font.GetHFont());
                                }
                                finally
                                {
                                    CodeAccessPermission.RevertAssert();
                                }
                                ambient.Value = font2;
                            }
                            catch (Exception exception)
                            {
                                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                                {
                                    throw;
                                }
                                ambient.Value = null;
                            }
                        }
                    }
                    return (Font) ambient.Value;
                }
            }

            [EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
            internal System.Drawing.Color AmbientForeColor
            {
                get
                {
                    Control.AmbientProperty ambient = this.LookupAmbient(-704);
                    if (ambient.Empty)
                    {
                        object obj2 = null;
                        if (this.GetAmbientProperty(-704, ref obj2) && (obj2 != null))
                        {
                            try
                            {
                                ambient.Value = ColorTranslator.FromOle(Convert.ToInt32(obj2, CultureInfo.InvariantCulture));
                            }
                            catch (Exception exception)
                            {
                                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                                {
                                    throw;
                                }
                            }
                        }
                    }
                    if (ambient.Value == null)
                    {
                        return System.Drawing.Color.Empty;
                    }
                    return (System.Drawing.Color) ambient.Value;
                }
            }

            [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            internal bool EventsFrozen
            {
                get
                {
                    return this.activeXState[eventsFrozen];
                }
                set
                {
                    this.activeXState[eventsFrozen] = value;
                }
            }

            internal IntPtr HWNDParent
            {
                get
                {
                    return this.hwndParent;
                }
            }

            internal bool IsIE
            {
                [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    if (!checkedIE)
                    {
                        if (this.clientSite == null)
                        {
                            return false;
                        }
                        if (Assembly.GetEntryAssembly() == null)
                        {
                            System.Windows.Forms.UnsafeNativeMethods.IOleContainer container;
                            if (System.Windows.Forms.NativeMethods.Succeeded(this.clientSite.GetContainer(out container)) && (container is System.Windows.Forms.NativeMethods.IHTMLDocument))
                            {
                                isIE = true;
                            }
                            if ((container != null) && System.Windows.Forms.UnsafeNativeMethods.IsComObject(container))
                            {
                                System.Windows.Forms.UnsafeNativeMethods.ReleaseComObject(container);
                            }
                        }
                        checkedIE = true;
                    }
                    return isIE;
                }
            }

            private Point LogPixels
            {
                get
                {
                    if (logPixels.IsEmpty)
                    {
                        logPixels = new Point();
                        IntPtr dC = System.Windows.Forms.UnsafeNativeMethods.GetDC(System.Windows.Forms.NativeMethods.NullHandleRef);
                        logPixels.X = System.Windows.Forms.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, dC), 0x58);
                        logPixels.Y = System.Windows.Forms.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, dC), 90);
                        System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(System.Windows.Forms.NativeMethods.NullHandleRef, new HandleRef(null, dC));
                    }
                    return logPixels;
                }
            }

            internal static class AdviseHelper
            {
                public static bool AdviseConnectionPoint(object connectionPoint, object sink, System.Type eventInterface, out int cookie)
                {
                    using (ComConnectionPointContainer container = new ComConnectionPointContainer(connectionPoint, true))
                    {
                        return AdviseConnectionPoint(container, sink, eventInterface, out cookie);
                    }
                }

                internal static bool AdviseConnectionPoint(ComConnectionPointContainer cpc, object sink, System.Type eventInterface, out int cookie)
                {
                    bool flag;
                    using (ComConnectionPoint point = cpc.FindConnectionPoint(eventInterface))
                    {
                        using (SafeIUnknown unknown = new SafeIUnknown(sink, true))
                        {
                            flag = point.Advise(unknown.DangerousGetHandle(), out cookie);
                        }
                    }
                    return flag;
                }

                internal sealed class ComConnectionPoint : Control.ActiveXImpl.AdviseHelper.SafeIUnknown
                {
                    private VTABLE vtbl;

                    public ComConnectionPoint(object obj, bool addRefIntPtr) : base(obj, addRefIntPtr, typeof(System.Runtime.InteropServices.ComTypes.IConnectionPoint).GUID)
                    {
                        this.vtbl = base.LoadVtable<VTABLE>();
                    }

                    public bool Advise(IntPtr punkEventSink, out int cookie)
                    {
                        AdviseD delegateForFunctionPointer = (AdviseD) Marshal.GetDelegateForFunctionPointer(this.vtbl.AdvisePtr, typeof(AdviseD));
                        return (delegateForFunctionPointer(base.handle, punkEventSink, out cookie) == 0);
                    }

                    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
                    private delegate int AdviseD(IntPtr This, IntPtr punkEventSink, out int cookie);

                    [StructLayout(LayoutKind.Sequential)]
                    private class VTABLE
                    {
                        public IntPtr QueryInterfacePtr;
                        public IntPtr AddRefPtr;
                        public IntPtr ReleasePtr;
                        public IntPtr GetConnectionInterfacePtr;
                        public IntPtr GetConnectionPointContainterPtr;
                        public IntPtr AdvisePtr;
                        public IntPtr UnadvisePtr;
                        public IntPtr EnumConnectionsPtr;
                    }
                }

                internal sealed class ComConnectionPointContainer : Control.ActiveXImpl.AdviseHelper.SafeIUnknown
                {
                    private VTABLE vtbl;

                    public ComConnectionPointContainer(object obj, bool addRefIntPtr) : base(obj, addRefIntPtr, typeof(System.Runtime.InteropServices.ComTypes.IConnectionPointContainer).GUID)
                    {
                        this.vtbl = base.LoadVtable<VTABLE>();
                    }

                    public Control.ActiveXImpl.AdviseHelper.ComConnectionPoint FindConnectionPoint(System.Type eventInterface)
                    {
                        FindConnectionPointD delegateForFunctionPointer = (FindConnectionPointD) Marshal.GetDelegateForFunctionPointer(this.vtbl.FindConnectionPointPtr, typeof(FindConnectionPointD));
                        IntPtr zero = IntPtr.Zero;
                        Guid gUID = eventInterface.GUID;
                        if ((delegateForFunctionPointer(base.handle, ref gUID, out zero) != 0) || (zero == IntPtr.Zero))
                        {
                            throw new ArgumentException(System.Windows.Forms.SR.GetString("AXNoConnectionPoint", new object[] { eventInterface.Name }));
                        }
                        return new Control.ActiveXImpl.AdviseHelper.ComConnectionPoint(zero, false);
                    }

                    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
                    private delegate int FindConnectionPointD(IntPtr This, ref Guid iid, out IntPtr ppv);

                    [StructLayout(LayoutKind.Sequential)]
                    private class VTABLE
                    {
                        public IntPtr QueryInterfacePtr;
                        public IntPtr AddRefPtr;
                        public IntPtr ReleasePtr;
                        public IntPtr EnumConnectionPointsPtr;
                        public IntPtr FindConnectionPointPtr;
                    }
                }

                internal class SafeIUnknown : SafeHandle
                {
                    public SafeIUnknown(object obj, bool addRefIntPtr) : this(obj, addRefIntPtr, Guid.Empty)
                    {
                    }

                    public SafeIUnknown(object obj, bool addRefIntPtr, Guid iid) : base(IntPtr.Zero, true)
                    {
                        RuntimeHelpers.PrepareConstrainedRegions();
                        try
                        {
                        }
                        finally
                        {
                            IntPtr iUnknownForObject;
                            if (obj is IntPtr)
                            {
                                iUnknownForObject = (IntPtr) obj;
                                if (addRefIntPtr)
                                {
                                    Marshal.AddRef(iUnknownForObject);
                                }
                            }
                            else
                            {
                                iUnknownForObject = Marshal.GetIUnknownForObject(obj);
                            }
                            if (iid != Guid.Empty)
                            {
                                IntPtr pUnk = iUnknownForObject;
                                try
                                {
                                    iUnknownForObject = InternalQueryInterface(iUnknownForObject, ref iid);
                                }
                                finally
                                {
                                    Marshal.Release(pUnk);
                                }
                            }
                            base.handle = iUnknownForObject;
                        }
                    }

                    private static IntPtr InternalQueryInterface(IntPtr pUnk, ref Guid iid)
                    {
                        IntPtr ptr;
                        if ((Marshal.QueryInterface(pUnk, ref iid, out ptr) != 0) || (ptr == IntPtr.Zero))
                        {
                            throw new InvalidCastException(System.Windows.Forms.SR.GetString("AxInterfaceNotSupported"));
                        }
                        return ptr;
                    }

                    protected V LoadVtable<V>()
                    {
                        return (V) Marshal.PtrToStructure(Marshal.ReadIntPtr(base.handle, 0), typeof(V));
                    }

                    protected sealed override bool ReleaseHandle()
                    {
                        IntPtr handle = base.handle;
                        base.handle = IntPtr.Zero;
                        if (IntPtr.Zero != handle)
                        {
                            Marshal.Release(handle);
                        }
                        return true;
                    }

                    public sealed override bool IsInvalid
                    {
                        get
                        {
                            if (!base.IsClosed)
                            {
                                return (IntPtr.Zero == base.handle);
                            }
                            return true;
                        }
                    }
                }
            }

            private class PropertyBagStream : System.Windows.Forms.UnsafeNativeMethods.IPropertyBag
            {
                private Hashtable bag = new Hashtable();

                [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
                internal void Read(System.Windows.Forms.UnsafeNativeMethods.IStream istream)
                {
                    Stream serializationStream = new DataStreamFromComStream(istream);
                    byte[] buffer = new byte[0x1000];
                    int offset = 0;
                    int num2 = serializationStream.Read(buffer, offset, 0x1000);
                    for (int i = num2; num2 == 0x1000; i += num2)
                    {
                        byte[] destinationArray = new byte[buffer.Length + 0x1000];
                        Array.Copy(buffer, destinationArray, buffer.Length);
                        buffer = destinationArray;
                        offset += 0x1000;
                        num2 = serializationStream.Read(buffer, offset, 0x1000);
                    }
                    serializationStream = new MemoryStream(buffer);
                    BinaryFormatter formatter = new BinaryFormatter();
                    try
                    {
                        this.bag = (Hashtable) formatter.Deserialize(serializationStream);
                    }
                    catch (Exception exception)
                    {
                        if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                        {
                            throw;
                        }
                        this.bag = new Hashtable();
                    }
                }

                [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
                int System.Windows.Forms.UnsafeNativeMethods.IPropertyBag.Read(string pszPropName, ref object pVar, System.Windows.Forms.UnsafeNativeMethods.IErrorLog pErrorLog)
                {
                    if (!this.bag.Contains(pszPropName))
                    {
                        return -2147024809;
                    }
                    pVar = this.bag[pszPropName];
                    return 0;
                }

                [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
                int System.Windows.Forms.UnsafeNativeMethods.IPropertyBag.Write(string pszPropName, ref object pVar)
                {
                    this.bag[pszPropName] = pVar;
                    return 0;
                }

                [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
                internal void Write(System.Windows.Forms.UnsafeNativeMethods.IStream istream)
                {
                    Stream serializationStream = new DataStreamFromComStream(istream);
                    new BinaryFormatter().Serialize(serializationStream, this.bag);
                }
            }
        }

        private class ActiveXVerbEnum : System.Windows.Forms.UnsafeNativeMethods.IEnumOLEVERB
        {
            private int current;
            private System.Windows.Forms.NativeMethods.tagOLEVERB[] verbs;

            internal ActiveXVerbEnum(System.Windows.Forms.NativeMethods.tagOLEVERB[] verbs)
            {
                this.verbs = verbs;
                this.current = 0;
            }

            public void Clone(out System.Windows.Forms.UnsafeNativeMethods.IEnumOLEVERB ppenum)
            {
                ppenum = new Control.ActiveXVerbEnum(this.verbs);
            }

            public int Next(int celt, System.Windows.Forms.NativeMethods.tagOLEVERB rgelt, int[] pceltFetched)
            {
                int num = 0;
                if (celt != 1)
                {
                    celt = 1;
                }
                while ((celt > 0) && (this.current < this.verbs.Length))
                {
                    rgelt.lVerb = this.verbs[this.current].lVerb;
                    rgelt.lpszVerbName = this.verbs[this.current].lpszVerbName;
                    rgelt.fuFlags = this.verbs[this.current].fuFlags;
                    rgelt.grfAttribs = this.verbs[this.current].grfAttribs;
                    celt--;
                    this.current++;
                    num++;
                }
                if (pceltFetched != null)
                {
                    pceltFetched[0] = num;
                }
                if (celt != 0)
                {
                    return 1;
                }
                return 0;
            }

            public void Reset()
            {
                this.current = 0;
            }

            public int Skip(int celt)
            {
                if ((this.current + celt) < this.verbs.Length)
                {
                    this.current += celt;
                    return 0;
                }
                this.current = this.verbs.Length;
                return 1;
            }
        }

        private class AmbientProperty
        {
            private int dispID;
            private bool empty;
            private string name;
            private object value;

            internal AmbientProperty(string name, int dispID)
            {
                this.name = name;
                this.dispID = dispID;
                this.value = null;
                this.empty = true;
            }

            internal void ResetValue()
            {
                this.empty = true;
                this.value = null;
            }

            internal int DispID
            {
                get
                {
                    return this.dispID;
                }
            }

            internal bool Empty
            {
                get
                {
                    return this.empty;
                }
            }

            internal string Name
            {
                get
                {
                    return this.name;
                }
            }

            internal object Value
            {
                get
                {
                    return this.value;
                }
                set
                {
                    this.value = value;
                    this.empty = false;
                }
            }
        }

        private class AxSourcingSite : ISite, IServiceProvider
        {
            private System.Windows.Forms.UnsafeNativeMethods.IOleClientSite clientSite;
            private IComponent component;
            private string name;
            private HtmlShimManager shimManager;

            internal AxSourcingSite(IComponent component, System.Windows.Forms.UnsafeNativeMethods.IOleClientSite clientSite, string name)
            {
                this.component = component;
                this.clientSite = clientSite;
                this.name = name;
            }

            public object GetService(System.Type service)
            {
                object clientSite = null;
                if (service == typeof(HtmlDocument))
                {
                    System.Windows.Forms.UnsafeNativeMethods.IOleContainer container;
                    int num;
                    try
                    {
                        System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                        num = this.clientSite.GetContainer(out container);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    if (!System.Windows.Forms.NativeMethods.Succeeded(num) || !(container is System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument))
                    {
                        return clientSite;
                    }
                    if (this.shimManager == null)
                    {
                        this.shimManager = new HtmlShimManager();
                    }
                    return new HtmlDocument(this.shimManager, container as System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument);
                }
                if (this.clientSite.GetType().IsAssignableFrom(service))
                {
                    System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
                    clientSite = this.clientSite;
                }
                return clientSite;
            }

            public IComponent Component
            {
                get
                {
                    return this.component;
                }
            }

            public IContainer Container
            {
                get
                {
                    return null;
                }
            }

            public bool DesignMode
            {
                get
                {
                    return false;
                }
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
                set
                {
                    if ((value == null) || (this.name == null))
                    {
                        this.name = value;
                    }
                }
            }
        }

        [ComVisible(true)]
        public class ControlAccessibleObject : AccessibleObject
        {
            private IntPtr handle;
            private static IntPtr oleAccAvailable = System.Windows.Forms.NativeMethods.InvalidIntPtr;
            private Control ownerControl;

            public ControlAccessibleObject(Control ownerControl)
            {
                this.handle = IntPtr.Zero;
                if (ownerControl == null)
                {
                    throw new ArgumentNullException("ownerControl");
                }
                this.ownerControl = ownerControl;
                IntPtr handle = ownerControl.Handle;
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    this.Handle = handle;
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }

            internal ControlAccessibleObject(Control ownerControl, int accObjId)
            {
                this.handle = IntPtr.Zero;
                if (ownerControl == null)
                {
                    throw new ArgumentNullException("ownerControl");
                }
                base.AccessibleObjectId = accObjId;
                this.ownerControl = ownerControl;
                IntPtr handle = ownerControl.Handle;
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    this.Handle = handle;
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }

            public override int GetHelpTopic(out string fileName)
            {
                int num = 0;
                QueryAccessibilityHelpEventHandler handler = (QueryAccessibilityHelpEventHandler) this.Owner.Events[Control.EventQueryAccessibilityHelp];
                if (handler == null)
                {
                    return base.GetHelpTopic(out fileName);
                }
                QueryAccessibilityHelpEventArgs e = new QueryAccessibilityHelpEventArgs();
                handler(this.Owner, e);
                fileName = e.HelpNamespace;
                if (!string.IsNullOrEmpty(fileName))
                {
                    System.Windows.Forms.IntSecurity.DemandFileIO(FileIOPermissionAccess.PathDiscovery, fileName);
                }
                try
                {
                    num = int.Parse(e.HelpKeyword, CultureInfo.InvariantCulture);
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                }
                return num;
            }

            internal override bool GetSysChild(AccessibleNavigation navdir, out AccessibleObject accessibleObject)
            {
                accessibleObject = null;
                Control parentInternal = this.ownerControl.ParentInternal;
                int index = -1;
                Control[] array = null;
                switch (navdir)
                {
                    case AccessibleNavigation.Next:
                        if (base.IsNonClientObject && (parentInternal != null))
                        {
                            array = parentInternal.GetChildControlsInTabOrder(true);
                            index = Array.IndexOf<Control>(array, this.ownerControl);
                            if (index != -1)
                            {
                                index++;
                            }
                        }
                        break;

                    case AccessibleNavigation.Previous:
                        if (base.IsNonClientObject && (parentInternal != null))
                        {
                            array = parentInternal.GetChildControlsInTabOrder(true);
                            index = Array.IndexOf<Control>(array, this.ownerControl);
                            if (index != -1)
                            {
                                index--;
                            }
                        }
                        break;

                    case AccessibleNavigation.FirstChild:
                        if (base.IsClientObject)
                        {
                            array = this.ownerControl.GetChildControlsInTabOrder(true);
                            index = 0;
                        }
                        break;

                    case AccessibleNavigation.LastChild:
                        if (base.IsClientObject)
                        {
                            array = this.ownerControl.GetChildControlsInTabOrder(true);
                            index = array.Length - 1;
                        }
                        break;
                }
                if ((array == null) || (array.Length == 0))
                {
                    return false;
                }
                if ((index >= 0) && (index < array.Length))
                {
                    accessibleObject = array[index].NcAccessibilityObject;
                }
                return true;
            }

            internal override int[] GetSysChildOrder()
            {
                if (this.ownerControl.GetStyle(ControlStyles.ContainerControl))
                {
                    return this.ownerControl.GetChildWindowsInTabOrder();
                }
                return base.GetSysChildOrder();
            }

            public void NotifyClients(AccessibleEvents accEvent)
            {
                System.Windows.Forms.UnsafeNativeMethods.NotifyWinEvent((int) accEvent, new HandleRef(this, this.Handle), -4, 0);
            }

            public void NotifyClients(AccessibleEvents accEvent, int childID)
            {
                System.Windows.Forms.UnsafeNativeMethods.NotifyWinEvent((int) accEvent, new HandleRef(this, this.Handle), -4, childID + 1);
            }

            public void NotifyClients(AccessibleEvents accEvent, int objectID, int childID)
            {
                System.Windows.Forms.UnsafeNativeMethods.NotifyWinEvent((int) accEvent, new HandleRef(this, this.Handle), objectID, childID + 1);
            }

            public override string ToString()
            {
                if (this.Owner != null)
                {
                    return ("ControlAccessibleObject: Owner = " + this.Owner.ToString());
                }
                return "ControlAccessibleObject: Owner = null";
            }

            public override string DefaultAction
            {
                get
                {
                    string accessibleDefaultActionDescription = this.ownerControl.AccessibleDefaultActionDescription;
                    if (accessibleDefaultActionDescription != null)
                    {
                        return accessibleDefaultActionDescription;
                    }
                    return base.DefaultAction;
                }
            }

            public override string Description
            {
                get
                {
                    string accessibleDescription = this.ownerControl.AccessibleDescription;
                    if (accessibleDescription != null)
                    {
                        return accessibleDescription;
                    }
                    return base.Description;
                }
            }

            public IntPtr Handle
            {
                get
                {
                    return this.handle;
                }
                set
                {
                    System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
                    if (this.handle != value)
                    {
                        this.handle = value;
                        if (oleAccAvailable != IntPtr.Zero)
                        {
                            bool flag = false;
                            if (oleAccAvailable == System.Windows.Forms.NativeMethods.InvalidIntPtr)
                            {
                                oleAccAvailable = System.Windows.Forms.UnsafeNativeMethods.LoadLibrary("oleacc.dll");
                                flag = oleAccAvailable != IntPtr.Zero;
                            }
                            if ((this.handle != IntPtr.Zero) && (oleAccAvailable != IntPtr.Zero))
                            {
                                base.UseStdAccessibleObjects(this.handle);
                            }
                            if (flag)
                            {
                                System.Windows.Forms.UnsafeNativeMethods.FreeLibrary(new HandleRef(null, oleAccAvailable));
                            }
                        }
                    }
                }
            }

            public override string Help
            {
                get
                {
                    QueryAccessibilityHelpEventHandler handler = (QueryAccessibilityHelpEventHandler) this.Owner.Events[Control.EventQueryAccessibilityHelp];
                    if (handler != null)
                    {
                        QueryAccessibilityHelpEventArgs e = new QueryAccessibilityHelpEventArgs();
                        handler(this.Owner, e);
                        return e.HelpString;
                    }
                    return base.Help;
                }
            }

            public override string KeyboardShortcut
            {
                get
                {
                    char mnemonic = WindowsFormsUtils.GetMnemonic(this.TextLabel, false);
                    if (mnemonic != '\0')
                    {
                        return ("Alt+" + mnemonic);
                    }
                    return null;
                }
            }

            public override string Name
            {
                get
                {
                    string accessibleName = this.ownerControl.AccessibleName;
                    if (accessibleName != null)
                    {
                        return accessibleName;
                    }
                    return WindowsFormsUtils.TextWithoutMnemonics(this.TextLabel);
                }
                set
                {
                    this.ownerControl.AccessibleName = value;
                }
            }

            public Control Owner
            {
                get
                {
                    return this.ownerControl;
                }
            }

            public override AccessibleObject Parent
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return base.Parent;
                }
            }

            internal Label PreviousLabel
            {
                get
                {
                    Control parentInternal = this.Owner.ParentInternal;
                    if (parentInternal != null)
                    {
                        ContainerControl containerControlInternal = parentInternal.GetContainerControlInternal() as ContainerControl;
                        if (containerControlInternal == null)
                        {
                            return null;
                        }
                        for (Control control3 = containerControlInternal.GetNextControl(this.Owner, false); control3 != null; control3 = containerControlInternal.GetNextControl(control3, false))
                        {
                            if (control3 is Label)
                            {
                                return (control3 as Label);
                            }
                            if (control3.Visible && control3.TabStop)
                            {
                                break;
                            }
                        }
                    }
                    return null;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    AccessibleRole accessibleRole = this.ownerControl.AccessibleRole;
                    if (accessibleRole != AccessibleRole.Default)
                    {
                        return accessibleRole;
                    }
                    return base.Role;
                }
            }

            internal string TextLabel
            {
                get
                {
                    if (this.ownerControl.GetStyle(ControlStyles.UseTextForAccessibility))
                    {
                        string text = this.ownerControl.Text;
                        if (!string.IsNullOrEmpty(text))
                        {
                            return text;
                        }
                    }
                    Label previousLabel = this.PreviousLabel;
                    if (previousLabel != null)
                    {
                        string str2 = previousLabel.Text;
                        if (!string.IsNullOrEmpty(str2))
                        {
                            return str2;
                        }
                    }
                    return null;
                }
            }
        }

        [ListBindable(false), ComVisible(false)]
        public class ControlCollection : ArrangedElementCollection, IList, ICollection, System.Collections.IEnumerable, ICloneable
        {
            private int lastAccessedIndex = -1;
            private Control owner;

            public ControlCollection(Control owner)
            {
                this.owner = owner;
            }

            public virtual void Add(Control value)
            {
                if (value != null)
                {
                    if (value.GetTopLevel())
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("TopLevelControlAdd"));
                    }
                    if (this.owner.CreateThreadId != value.CreateThreadId)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("AddDifferentThreads"));
                    }
                    Control.CheckParentingCycle(this.owner, value);
                    if (value.parent == this.owner)
                    {
                        value.SendToBack();
                    }
                    else
                    {
                        if (value.parent != null)
                        {
                            value.parent.Controls.Remove(value);
                        }
                        base.InnerList.Add(value);
                        if (value.tabIndex == -1)
                        {
                            int num = 0;
                            for (int i = 0; i < (this.Count - 1); i++)
                            {
                                int tabIndex = this[i].TabIndex;
                                if (num <= tabIndex)
                                {
                                    num = tabIndex + 1;
                                }
                            }
                            value.tabIndex = num;
                        }
                        this.owner.SuspendLayout();
                        try
                        {
                            Control parent = value.parent;
                            try
                            {
                                value.AssignParent(this.owner);
                            }
                            finally
                            {
                                if ((parent != value.parent) && ((this.owner.state & 1) != 0))
                                {
                                    value.SetParentHandle(this.owner.InternalHandle);
                                    if (value.Visible)
                                    {
                                        value.CreateControl();
                                    }
                                }
                            }
                            value.InitLayout();
                        }
                        finally
                        {
                            this.owner.ResumeLayout(false);
                        }
                        LayoutTransaction.DoLayout(this.owner, value, PropertyNames.Parent);
                        this.owner.OnControlAdded(new ControlEventArgs(value));
                    }
                }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public virtual void AddRange(Control[] controls)
            {
                if (controls == null)
                {
                    throw new ArgumentNullException("controls");
                }
                if (controls.Length > 0)
                {
                    this.owner.SuspendLayout();
                    try
                    {
                        for (int i = 0; i < controls.Length; i++)
                        {
                            this.Add(controls[i]);
                        }
                    }
                    finally
                    {
                        this.owner.ResumeLayout(true);
                    }
                }
            }

            public virtual void Clear()
            {
                this.owner.SuspendLayout();
                CommonProperties.xClearAllPreferredSizeCaches(this.owner);
                try
                {
                    while (this.Count != 0)
                    {
                        this.RemoveAt(this.Count - 1);
                    }
                }
                finally
                {
                    this.owner.ResumeLayout();
                }
            }

            public bool Contains(Control control)
            {
                return base.InnerList.Contains(control);
            }

            public virtual bool ContainsKey(string key)
            {
                return this.IsValidIndex(this.IndexOfKey(key));
            }

            public Control[] Find(string key, bool searchAllChildren)
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentNullException("key", System.Windows.Forms.SR.GetString("FindKeyMayNotBeEmptyOrNull"));
                }
                ArrayList list = this.FindInternal(key, searchAllChildren, this, new ArrayList());
                Control[] array = new Control[list.Count];
                list.CopyTo(array, 0);
                return array;
            }

            private ArrayList FindInternal(string key, bool searchAllChildren, Control.ControlCollection controlsToLookIn, ArrayList foundControls)
            {
                if ((controlsToLookIn == null) || (foundControls == null))
                {
                    return null;
                }
                try
                {
                    for (int i = 0; i < controlsToLookIn.Count; i++)
                    {
                        if ((controlsToLookIn[i] != null) && WindowsFormsUtils.SafeCompareStrings(controlsToLookIn[i].Name, key, true))
                        {
                            foundControls.Add(controlsToLookIn[i]);
                        }
                    }
                    if (!searchAllChildren)
                    {
                        return foundControls;
                    }
                    for (int j = 0; j < controlsToLookIn.Count; j++)
                    {
                        if (((controlsToLookIn[j] != null) && (controlsToLookIn[j].Controls != null)) && (controlsToLookIn[j].Controls.Count > 0))
                        {
                            foundControls = this.FindInternal(key, searchAllChildren, controlsToLookIn[j].Controls, foundControls);
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                }
                return foundControls;
            }

            public int GetChildIndex(Control child)
            {
                return this.GetChildIndex(child, true);
            }

            public virtual int GetChildIndex(Control child, bool throwException)
            {
                int index = this.IndexOf(child);
                if ((index == -1) && throwException)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ControlNotChild"));
                }
                return index;
            }

            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            public override System.Collections.IEnumerator GetEnumerator()
            {
                return new ControlCollectionEnumerator(this);
            }

            public int IndexOf(Control control)
            {
                return base.InnerList.IndexOf(control);
            }

            public virtual int IndexOfKey(string key)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    if (this.IsValidIndex(this.lastAccessedIndex) && WindowsFormsUtils.SafeCompareStrings(this[this.lastAccessedIndex].Name, key, true))
                    {
                        return this.lastAccessedIndex;
                    }
                    for (int i = 0; i < this.Count; i++)
                    {
                        if (WindowsFormsUtils.SafeCompareStrings(this[i].Name, key, true))
                        {
                            this.lastAccessedIndex = i;
                            return i;
                        }
                    }
                    this.lastAccessedIndex = -1;
                }
                return -1;
            }

            private bool IsValidIndex(int index)
            {
                return ((index >= 0) && (index < this.Count));
            }

            public virtual void Remove(Control value)
            {
                if ((value != null) && (value.ParentInternal == this.owner))
                {
                    value.SetParentHandle(IntPtr.Zero);
                    base.InnerList.Remove(value);
                    value.AssignParent(null);
                    LayoutTransaction.DoLayout(this.owner, value, PropertyNames.Parent);
                    this.owner.OnControlRemoved(new ControlEventArgs(value));
                    ContainerControl containerControlInternal = this.owner.GetContainerControlInternal() as ContainerControl;
                    if (containerControlInternal != null)
                    {
                        containerControlInternal.AfterControlRemoved(value, this.owner);
                    }
                }
            }

            public void RemoveAt(int index)
            {
                this.Remove(this[index]);
            }

            public virtual void RemoveByKey(string key)
            {
                int index = this.IndexOfKey(key);
                if (this.IsValidIndex(index))
                {
                    this.RemoveAt(index);
                }
            }

            public virtual void SetChildIndex(Control child, int newIndex)
            {
                this.SetChildIndexInternal(child, newIndex);
            }

            internal virtual void SetChildIndexInternal(Control child, int newIndex)
            {
                if (child == null)
                {
                    throw new ArgumentNullException("child");
                }
                int childIndex = this.GetChildIndex(child);
                if (childIndex != newIndex)
                {
                    if ((newIndex >= this.Count) || (newIndex == -1))
                    {
                        newIndex = this.Count - 1;
                    }
                    base.MoveElement(child, childIndex, newIndex);
                    child.UpdateZOrder();
                    LayoutTransaction.DoLayout(this.owner, child, PropertyNames.ChildIndex);
                }
            }

            int IList.Add(object control)
            {
                if (!(control is Control))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ControlBadControl"), "control");
                }
                this.Add((Control) control);
                return this.IndexOf((Control) control);
            }

            void IList.Remove(object control)
            {
                if (control is Control)
                {
                    this.Remove((Control) control);
                }
            }

            object ICloneable.Clone()
            {
                Control.ControlCollection controls = this.owner.CreateControlsInstance();
                controls.InnerList.AddRange(this);
                return controls;
            }

            public virtual Control this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.Count))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("IndexOutOfRange", new object[] { index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    return (Control) base.InnerList[index];
                }
            }

            public virtual Control this[string key]
            {
                get
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        int index = this.IndexOfKey(key);
                        if (this.IsValidIndex(index))
                        {
                            return this[index];
                        }
                    }
                    return null;
                }
            }

            public Control Owner
            {
                get
                {
                    return this.owner;
                }
            }

            private class ControlCollectionEnumerator : System.Collections.IEnumerator
            {
                private Control.ControlCollection controls;
                private int current;
                private int originalCount;

                public ControlCollectionEnumerator(Control.ControlCollection controls)
                {
                    this.controls = controls;
                    this.originalCount = controls.Count;
                    this.current = -1;
                }

                public bool MoveNext()
                {
                    if ((this.current < (this.controls.Count - 1)) && (this.current < (this.originalCount - 1)))
                    {
                        this.current++;
                        return true;
                    }
                    return false;
                }

                public void Reset()
                {
                    this.current = -1;
                }

                public object Current
                {
                    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
                    get
                    {
                        if (this.current == -1)
                        {
                            return null;
                        }
                        return this.controls[this.current];
                    }
                }
            }
        }

        internal sealed class ControlNativeWindow : NativeWindow, IWindowTarget
        {
            private Control control;
            private GCHandle rootRef;
            internal IWindowTarget target;

            internal ControlNativeWindow(Control control)
            {
                this.control = control;
                this.target = this;
            }

            internal Control GetControl()
            {
                return this.control;
            }

            internal void LockReference(bool locked)
            {
                if (locked)
                {
                    if (!this.rootRef.IsAllocated)
                    {
                        this.rootRef = GCHandle.Alloc(this.GetControl(), GCHandleType.Normal);
                    }
                }
                else if (this.rootRef.IsAllocated)
                {
                    this.rootRef.Free();
                }
            }

            protected override void OnHandleChange()
            {
                this.target.OnHandleChange(base.Handle);
            }

            public void OnHandleChange(IntPtr newHandle)
            {
                this.control.SetHandle(newHandle);
            }

            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            public void OnMessage(ref Message m)
            {
                this.control.WndProc(ref m);
            }

            protected override void OnThreadException(Exception e)
            {
                this.control.WndProcException(e);
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case 0x200:
                        if (!this.control.GetState(0x4000))
                        {
                            this.control.HookMouseEvent();
                            if (!this.control.GetState(0x2000))
                            {
                                this.control.SendMessage(System.Windows.Forms.NativeMethods.WM_MOUSEENTER, 0, 0);
                            }
                            else
                            {
                                this.control.SetState(0x2000, false);
                            }
                        }
                        break;

                    case 0x20a:
                        this.control.ResetMouseEventArgs();
                        break;

                    case 0x2a3:
                        this.control.UnhookMouseEvent();
                        break;
                }
                this.target.OnMessage(ref m);
            }

            internal IWindowTarget WindowTarget
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

        private class ControlTabOrderComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                Control.ControlTabOrderHolder holder = (Control.ControlTabOrderHolder) x;
                Control.ControlTabOrderHolder holder2 = (Control.ControlTabOrderHolder) y;
                int num = holder.newOrder - holder2.newOrder;
                if (num == 0)
                {
                    num = holder.oldOrder - holder2.oldOrder;
                }
                return num;
            }
        }

        private class ControlTabOrderHolder
        {
            internal readonly Control control;
            internal readonly int newOrder;
            internal readonly int oldOrder;

            internal ControlTabOrderHolder(int oldOrder, int newOrder, Control control)
            {
                this.oldOrder = oldOrder;
                this.newOrder = newOrder;
                this.control = control;
            }
        }

        private class ControlVersionInfo
        {
            private string companyName;
            private Control owner;
            private string productName;
            private string productVersion;
            private FileVersionInfo versionInfo;

            internal ControlVersionInfo(Control owner)
            {
                this.owner = owner;
            }

            private FileVersionInfo GetFileVersionInfo()
            {
                if (this.versionInfo == null)
                {
                    string fullyQualifiedName;
                    new FileIOPermission(PermissionState.None) { AllFiles = FileIOPermissionAccess.PathDiscovery }.Assert();
                    try
                    {
                        fullyQualifiedName = this.owner.GetType().Module.FullyQualifiedName;
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    new FileIOPermission(FileIOPermissionAccess.Read, fullyQualifiedName).Assert();
                    try
                    {
                        this.versionInfo = FileVersionInfo.GetVersionInfo(fullyQualifiedName);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
                return this.versionInfo;
            }

            internal string CompanyName
            {
                get
                {
                    if (this.companyName == null)
                    {
                        object[] customAttributes = this.owner.GetType().Module.Assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                        if ((customAttributes != null) && (customAttributes.Length > 0))
                        {
                            this.companyName = ((AssemblyCompanyAttribute) customAttributes[0]).Company;
                        }
                        if ((this.companyName == null) || (this.companyName.Length == 0))
                        {
                            this.companyName = this.GetFileVersionInfo().CompanyName;
                            if (this.companyName != null)
                            {
                                this.companyName = this.companyName.Trim();
                            }
                        }
                        if ((this.companyName == null) || (this.companyName.Length == 0))
                        {
                            string str = this.owner.GetType().Namespace;
                            if (str == null)
                            {
                                str = string.Empty;
                            }
                            int index = str.IndexOf("/");
                            if (index != -1)
                            {
                                this.companyName = str.Substring(0, index);
                            }
                            else
                            {
                                this.companyName = str;
                            }
                        }
                    }
                    return this.companyName;
                }
            }

            internal string ProductName
            {
                get
                {
                    if (this.productName == null)
                    {
                        object[] customAttributes = this.owner.GetType().Module.Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                        if ((customAttributes != null) && (customAttributes.Length > 0))
                        {
                            this.productName = ((AssemblyProductAttribute) customAttributes[0]).Product;
                        }
                        if ((this.productName == null) || (this.productName.Length == 0))
                        {
                            this.productName = this.GetFileVersionInfo().ProductName;
                            if (this.productName != null)
                            {
                                this.productName = this.productName.Trim();
                            }
                        }
                        if ((this.productName == null) || (this.productName.Length == 0))
                        {
                            string str = this.owner.GetType().Namespace;
                            if (str == null)
                            {
                                str = string.Empty;
                            }
                            int index = str.IndexOf(".");
                            if (index != -1)
                            {
                                this.productName = str.Substring(index + 1);
                            }
                            else
                            {
                                this.productName = str;
                            }
                        }
                    }
                    return this.productName;
                }
            }

            internal string ProductVersion
            {
                get
                {
                    if (this.productVersion == null)
                    {
                        object[] customAttributes = this.owner.GetType().Module.Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
                        if ((customAttributes != null) && (customAttributes.Length > 0))
                        {
                            this.productVersion = ((AssemblyInformationalVersionAttribute) customAttributes[0]).InformationalVersion;
                        }
                        if ((this.productVersion == null) || (this.productVersion.Length == 0))
                        {
                            this.productVersion = this.GetFileVersionInfo().ProductVersion;
                            if (this.productVersion != null)
                            {
                                this.productVersion = this.productVersion.Trim();
                            }
                        }
                        if (this.productVersion.Length == 0)
                        {
                            this.productVersion = "1.0.0.0";
                        }
                    }
                    return this.productVersion;
                }
            }
        }

        internal sealed class FontHandleWrapper : MarshalByRefObject, IDisposable
        {
            private IntPtr handle;

            internal FontHandleWrapper(Font font)
            {
                this.handle = font.ToHfont();
                System.Internal.HandleCollector.Add(this.handle, System.Windows.Forms.NativeMethods.CommonHandles.GDI);
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (this.handle != IntPtr.Zero)
                {
                    System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(this, this.handle));
                    this.handle = IntPtr.Zero;
                }
            }

            ~FontHandleWrapper()
            {
                this.Dispose(false);
            }

            internal IntPtr Handle
            {
                get
                {
                    return this.handle;
                }
            }
        }

        private class MetafileDCWrapper : IDisposable
        {
            private System.Windows.Forms.NativeMethods.RECT destRect;
            private HandleRef hBitmap = System.Windows.Forms.NativeMethods.NullHandleRef;
            private HandleRef hBitmapDC = System.Windows.Forms.NativeMethods.NullHandleRef;
            private HandleRef hMetafileDC = System.Windows.Forms.NativeMethods.NullHandleRef;
            private HandleRef hOriginalBmp = System.Windows.Forms.NativeMethods.NullHandleRef;

            internal MetafileDCWrapper(HandleRef hOriginalDC, Size size)
            {
                if ((size.Width < 0) || (size.Height < 0))
                {
                    throw new ArgumentException("size", System.Windows.Forms.SR.GetString("ControlMetaFileDCWrapperSizeInvalid"));
                }
                this.hMetafileDC = hOriginalDC;
                this.destRect = new System.Windows.Forms.NativeMethods.RECT(0, 0, size.Width, size.Height);
                this.hBitmapDC = new HandleRef(this, System.Windows.Forms.UnsafeNativeMethods.CreateCompatibleDC(System.Windows.Forms.NativeMethods.NullHandleRef));
                int deviceCaps = System.Windows.Forms.UnsafeNativeMethods.GetDeviceCaps(this.hBitmapDC, 14);
                int nBitsPerPixel = System.Windows.Forms.UnsafeNativeMethods.GetDeviceCaps(this.hBitmapDC, 12);
                this.hBitmap = new HandleRef(this, System.Windows.Forms.SafeNativeMethods.CreateBitmap(size.Width, size.Height, deviceCaps, nBitsPerPixel, IntPtr.Zero));
                this.hOriginalBmp = new HandleRef(this, System.Windows.Forms.SafeNativeMethods.SelectObject(this.hBitmapDC, this.hBitmap));
            }

            private unsafe bool DICopy(HandleRef hdcDest, HandleRef hdcSrc, System.Windows.Forms.NativeMethods.RECT rect, bool bStretch)
            {
                bool flag = false;
                HandleRef hObject = new HandleRef(this, System.Windows.Forms.SafeNativeMethods.CreateBitmap(1, 1, 1, 1, IntPtr.Zero));
                if (hObject.Handle != IntPtr.Zero)
                {
                    try
                    {
                        int left;
                        int top;
                        int bmWidth;
                        int bmHeight;
                        HandleRef ref3 = new HandleRef(this, System.Windows.Forms.SafeNativeMethods.SelectObject(hdcSrc, hObject));
                        if (ref3.Handle == IntPtr.Zero)
                        {
                            return flag;
                        }
                        System.Windows.Forms.SafeNativeMethods.SelectObject(hdcSrc, ref3);
                        System.Windows.Forms.NativeMethods.BITMAP bm = new System.Windows.Forms.NativeMethods.BITMAP();
                        if (System.Windows.Forms.UnsafeNativeMethods.GetObject(ref3, Marshal.SizeOf(bm), bm) == 0)
                        {
                            return flag;
                        }
                        System.Windows.Forms.NativeMethods.BITMAPINFO_FLAT bmi = new System.Windows.Forms.NativeMethods.BITMAPINFO_FLAT {
                            bmiHeader_biSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.BITMAPINFOHEADER)),
                            bmiHeader_biWidth = bm.bmWidth,
                            bmiHeader_biHeight = bm.bmHeight,
                            bmiHeader_biPlanes = 1,
                            bmiHeader_biBitCount = bm.bmBitsPixel,
                            bmiHeader_biCompression = 0,
                            bmiHeader_biSizeImage = 0,
                            bmiHeader_biXPelsPerMeter = 0,
                            bmiHeader_biYPelsPerMeter = 0,
                            bmiHeader_biClrUsed = 0,
                            bmiHeader_biClrImportant = 0,
                            bmiColors = new byte[0x400]
                        };
                        long num2 = ((int) 1) << (bm.bmBitsPixel * bm.bmPlanes);
                        if (num2 <= 0x100L)
                        {
                            byte[] lppe = new byte[Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.PALETTEENTRY)) * 0x100];
                            System.Windows.Forms.SafeNativeMethods.GetSystemPaletteEntries(hdcSrc, 0, (int) num2, lppe);
                            try
                            {
                                fixed (byte* numRef = bmi.bmiColors)
                                {
                                    try
                                    {
                                        fixed (byte* numRef2 = lppe)
                                        {
                                            System.Windows.Forms.NativeMethods.RGBQUAD* rgbquadPtr = (System.Windows.Forms.NativeMethods.RGBQUAD*) numRef;
                                            System.Windows.Forms.NativeMethods.PALETTEENTRY* paletteentryPtr = (System.Windows.Forms.NativeMethods.PALETTEENTRY*) numRef2;
                                            for (long i = 0L; i < ((int) num2); i += 1L)
                                            {
                                                rgbquadPtr[(int) (i * sizeof(System.Windows.Forms.NativeMethods.RGBQUAD))].rgbRed = paletteentryPtr[(int) (i * sizeof(System.Windows.Forms.NativeMethods.PALETTEENTRY))].peRed;
                                                rgbquadPtr[(int) (i * sizeof(System.Windows.Forms.NativeMethods.RGBQUAD))].rgbBlue = paletteentryPtr[(int) (i * sizeof(System.Windows.Forms.NativeMethods.PALETTEENTRY))].peBlue;
                                                rgbquadPtr[(int) (i * sizeof(System.Windows.Forms.NativeMethods.RGBQUAD))].rgbGreen = paletteentryPtr[(int) (i * sizeof(System.Windows.Forms.NativeMethods.PALETTEENTRY))].peGreen;
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        numRef2 = null;
                                    }
                                }
                            }
                            finally
                            {
                                numRef = null;
                            }
                        }
                        long num3 = bm.bmBitsPixel * bm.bmWidth;
                        long num4 = (num3 + 7L) / 8L;
                        long num5 = num4 * bm.bmHeight;
                        byte[] lpvBits = new byte[num5];
                        if (System.Windows.Forms.SafeNativeMethods.GetDIBits(hdcSrc, ref3, 0, bm.bmHeight, lpvBits, ref bmi, 0) == 0)
                        {
                            return flag;
                        }
                        if (bStretch)
                        {
                            left = rect.left;
                            top = rect.top;
                            bmWidth = rect.right - rect.left;
                            bmHeight = rect.bottom - rect.top;
                        }
                        else
                        {
                            left = rect.left;
                            top = rect.top;
                            bmWidth = bm.bmWidth;
                            bmHeight = bm.bmHeight;
                        }
                        if (System.Windows.Forms.SafeNativeMethods.StretchDIBits(hdcDest, left, top, bmWidth, bmHeight, 0, 0, bm.bmWidth, bm.bmHeight, lpvBits, ref bmi, 0, 0xcc0020) == -1)
                        {
                            return flag;
                        }
                        flag = true;
                    }
                    finally
                    {
                        System.Windows.Forms.SafeNativeMethods.DeleteObject(hObject);
                    }
                }
                return flag;
            }

            ~MetafileDCWrapper()
            {
                ((IDisposable) this).Dispose();
            }

            void IDisposable.Dispose()
            {
                if (((this.hBitmapDC.Handle != IntPtr.Zero) && (this.hMetafileDC.Handle != IntPtr.Zero)) && (this.hBitmap.Handle != IntPtr.Zero))
                {
                    try
                    {
                        this.DICopy(this.hMetafileDC, this.hBitmapDC, this.destRect, true);
                        System.Windows.Forms.SafeNativeMethods.SelectObject(this.hBitmapDC, this.hOriginalBmp);
                        System.Windows.Forms.SafeNativeMethods.DeleteObject(this.hBitmap);
                        System.Windows.Forms.UnsafeNativeMethods.DeleteCompatibleDC(this.hBitmapDC);
                    }
                    finally
                    {
                        this.hBitmapDC = System.Windows.Forms.NativeMethods.NullHandleRef;
                        this.hBitmap = System.Windows.Forms.NativeMethods.NullHandleRef;
                        this.hOriginalBmp = System.Windows.Forms.NativeMethods.NullHandleRef;
                        GC.SuppressFinalize(this);
                    }
                }
            }

            internal IntPtr HDC
            {
                get
                {
                    return this.hBitmapDC.Handle;
                }
            }
        }

        private sealed class MultithreadSafeCallScope : IDisposable
        {
            private bool resultedInSet;

            internal MultithreadSafeCallScope()
            {
                if (Control.checkForIllegalCrossThreadCalls && !Control.inCrossThreadSafeCall)
                {
                    Control.inCrossThreadSafeCall = true;
                    this.resultedInSet = true;
                }
                else
                {
                    this.resultedInSet = false;
                }
            }

            void IDisposable.Dispose()
            {
                if (this.resultedInSet)
                {
                    Control.inCrossThreadSafeCall = false;
                }
            }
        }

        private sealed class PrintPaintEventArgs : PaintEventArgs
        {
            private System.Windows.Forms.Message m;

            internal PrintPaintEventArgs(System.Windows.Forms.Message m, IntPtr dc, Rectangle clipRect) : base(dc, clipRect)
            {
                this.m = m;
            }

            internal System.Windows.Forms.Message Message
            {
                get
                {
                    return this.m;
                }
            }
        }

        private class ThreadMethodEntry : IAsyncResult
        {
            internal object[] args;
            internal Control caller;
            internal Exception exception;
            internal ExecutionContext executionContext;
            private object invokeSyncObject = new object();
            private bool isCompleted;
            internal Control marshaler;
            internal Delegate method;
            private ManualResetEvent resetEvent;
            internal object retVal;
            internal SynchronizationContext syncContext;
            internal bool synchronous;

            internal ThreadMethodEntry(Control caller, Control marshaler, Delegate method, object[] args, bool synchronous, ExecutionContext executionContext)
            {
                this.caller = caller;
                this.marshaler = marshaler;
                this.method = method;
                this.args = args;
                this.exception = null;
                this.retVal = null;
                this.synchronous = synchronous;
                this.isCompleted = false;
                this.resetEvent = null;
                this.executionContext = executionContext;
            }

            internal void Complete()
            {
                lock (this.invokeSyncObject)
                {
                    this.isCompleted = true;
                    if (this.resetEvent != null)
                    {
                        this.resetEvent.Set();
                    }
                }
            }

            ~ThreadMethodEntry()
            {
                if (this.resetEvent != null)
                {
                    this.resetEvent.Close();
                }
            }

            public object AsyncState
            {
                get
                {
                    return null;
                }
            }

            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    if (this.resetEvent == null)
                    {
                        lock (this.invokeSyncObject)
                        {
                            if (this.resetEvent == null)
                            {
                                this.resetEvent = new ManualResetEvent(false);
                                if (this.isCompleted)
                                {
                                    this.resetEvent.Set();
                                }
                            }
                        }
                    }
                    return this.resetEvent;
                }
            }

            public bool CompletedSynchronously
            {
                get
                {
                    return (this.isCompleted && this.synchronous);
                }
            }

            public bool IsCompleted
            {
                get
                {
                    return this.isCompleted;
                }
            }
        }
    }
}

