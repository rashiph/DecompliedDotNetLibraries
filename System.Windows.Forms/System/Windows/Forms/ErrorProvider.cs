namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Internal;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.Internal;

    [ProvideProperty("IconAlignment", typeof(Control)), ToolboxItemFilter("System.Windows.Forms"), ComplexBindingProperties("DataSource", "DataMember"), ProvideProperty("IconPadding", typeof(Control)), ProvideProperty("Error", typeof(Control)), System.Windows.Forms.SRDescription("DescriptionErrorProvider")]
    public class ErrorProvider : Component, IExtenderProvider, ISupportInitialize
    {
        private int blinkRate;
        private ErrorBlinkStyle blinkStyle;
        private EventHandler currentChanged;
        private string dataMember;
        private object dataSource;
        private const int defaultBlinkRate = 250;
        private const ErrorBlinkStyle defaultBlinkStyle = ErrorBlinkStyle.BlinkIfDifferentError;
        [ThreadStatic]
        private static System.Drawing.Icon defaultIcon;
        private const ErrorIconAlignment defaultIconAlignment = ErrorIconAlignment.MiddleRight;
        private BindingManagerBase errorManager;
        private System.Drawing.Icon icon;
        private bool initializing;
        private bool inSetErrorManager;
        private int itemIdCounter;
        private Hashtable items;
        private System.Windows.Forms.ContainerControl parentControl;
        private EventHandler propChangedEvent;
        private IconRegion region;
        private bool rightToLeft;
        private bool setErrorManagerOnEndInit;
        private bool showIcon;
        private object userData;
        private Hashtable windows;

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnRightToLeftChangedDescr")]
        public event EventHandler RightToLeftChanged;

        public ErrorProvider()
        {
            this.items = new Hashtable();
            this.windows = new Hashtable();
            this.icon = DefaultIcon;
            this.showIcon = true;
            this.icon = DefaultIcon;
            this.blinkRate = 250;
            this.blinkStyle = ErrorBlinkStyle.BlinkIfDifferentError;
            this.currentChanged = new EventHandler(this.ErrorManager_CurrentChanged);
        }

        public ErrorProvider(IContainer container) : this()
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            container.Add(this);
        }

        public ErrorProvider(System.Windows.Forms.ContainerControl parentControl) : this()
        {
            this.parentControl = parentControl;
            this.propChangedEvent = new EventHandler(this.ParentControl_BindingContextChanged);
            parentControl.BindingContextChanged += this.propChangedEvent;
        }

        public void BindToDataAndErrors(object newDataSource, string newDataMember)
        {
            this.Set_ErrorManager(newDataSource, newDataMember, false);
        }

        public bool CanExtend(object extendee)
        {
            return (((extendee is Control) && !(extendee is Form)) && !(extendee is ToolBar));
        }

        public void Clear()
        {
            ErrorWindow[] array = new ErrorWindow[this.windows.Values.Count];
            this.windows.Values.CopyTo(array, 0);
            for (int i = 0; i < array.Length; i++)
            {
                array[i].Dispose();
            }
            this.windows.Clear();
            foreach (ControlItem item in this.items.Values)
            {
                if (item != null)
                {
                    item.Dispose();
                }
            }
            this.items.Clear();
        }

        private void DataSource_Initialized(object sender, EventArgs e)
        {
            ISupportInitializeNotification dataSource = this.DataSource as ISupportInitializeNotification;
            if (dataSource != null)
            {
                dataSource.Initialized -= new EventHandler(this.DataSource_Initialized);
            }
            this.EndInitCore();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Clear();
                this.DisposeRegion();
                this.UnwireEvents(this.errorManager);
            }
            base.Dispose(disposing);
        }

        private void DisposeRegion()
        {
            if (this.region != null)
            {
                this.region.Dispose();
                this.region = null;
            }
        }

        private void EndInitCore()
        {
            this.initializing = false;
            if (this.setErrorManagerOnEndInit)
            {
                this.setErrorManagerOnEndInit = false;
                this.Set_ErrorManager(this.DataSource, this.DataMember, true);
            }
        }

        private ControlItem EnsureControlItem(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            ControlItem item = (ControlItem) this.items[control];
            if (item == null)
            {
                item = new ControlItem(this, control, (IntPtr) (++this.itemIdCounter));
                this.items[control] = item;
            }
            return item;
        }

        internal ErrorWindow EnsureErrorWindow(Control parent)
        {
            ErrorWindow window = (ErrorWindow) this.windows[parent];
            if (window == null)
            {
                window = new ErrorWindow(this, parent);
                this.windows[parent] = window;
            }
            return window;
        }

        private void ErrorManager_BindingComplete(object sender, BindingCompleteEventArgs e)
        {
            Binding binding = e.Binding;
            if ((binding != null) && (binding.Control != null))
            {
                this.SetError(binding.Control, (e.ErrorText == null) ? string.Empty : e.ErrorText);
            }
        }

        private void ErrorManager_BindingsChanged(object sender, CollectionChangeEventArgs e)
        {
            this.ErrorManager_CurrentChanged(this.errorManager, e);
        }

        private void ErrorManager_CurrentChanged(object sender, EventArgs e)
        {
            if (this.errorManager.Count != 0)
            {
                object current = this.errorManager.Current;
                if (current is IDataErrorInfo)
                {
                    BindingsCollection bindings = this.errorManager.Bindings;
                    int count = bindings.Count;
                    foreach (ControlItem item in this.items.Values)
                    {
                        item.BlinkPhase = 0;
                    }
                    Hashtable hashtable = new Hashtable(count);
                    for (int i = 0; i < count; i++)
                    {
                        if (bindings[i].Control != null)
                        {
                            BindToObject bindToObject = bindings[i].BindToObject;
                            string str = ((IDataErrorInfo) current)[bindToObject.BindingMemberInfo.BindingField];
                            if (str == null)
                            {
                                str = "";
                            }
                            string str2 = "";
                            if (hashtable.Contains(bindings[i].Control))
                            {
                                str2 = (string) hashtable[bindings[i].Control];
                            }
                            if (string.IsNullOrEmpty(str2))
                            {
                                str2 = str;
                            }
                            else
                            {
                                str2 = str2 + "\r\n" + str;
                            }
                            hashtable[bindings[i].Control] = str2;
                        }
                    }
                    IEnumerator enumerator = hashtable.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        DictionaryEntry entry = (DictionaryEntry) enumerator.Current;
                        this.SetError((Control) entry.Key, (string) entry.Value);
                    }
                }
            }
        }

        private void ErrorManager_ItemChanged(object sender, ItemChangedEventArgs e)
        {
            BindingsCollection bindings = this.errorManager.Bindings;
            int count = bindings.Count;
            if ((e.Index == -1) && (this.errorManager.Count == 0))
            {
                for (int i = 0; i < count; i++)
                {
                    if (bindings[i].Control != null)
                    {
                        this.SetError(bindings[i].Control, "");
                    }
                }
            }
            else
            {
                this.ErrorManager_CurrentChanged(sender, e);
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(""), System.Windows.Forms.SRDescription("ErrorProviderErrorDescr"), Localizable(true)]
        public string GetError(Control control)
        {
            return this.EnsureControlItem(control).Error;
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ErrorProviderIconAlignmentDescr"), Localizable(true), DefaultValue(3)]
        public ErrorIconAlignment GetIconAlignment(Control control)
        {
            return this.EnsureControlItem(control).IconAlignment;
        }

        [System.Windows.Forms.SRDescription("ErrorProviderIconPaddingDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(0)]
        public int GetIconPadding(Control control)
        {
            return this.EnsureControlItem(control).IconPadding;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnRightToLeftChanged(EventArgs e)
        {
            foreach (ErrorWindow window in this.windows.Values)
            {
                window.Update(false);
            }
            if (this.onRightToLeftChanged != null)
            {
                this.onRightToLeftChanged(this, e);
            }
        }

        private void ParentControl_BindingContextChanged(object sender, EventArgs e)
        {
            this.Set_ErrorManager(this.DataSource, this.DataMember, true);
        }

        private void ResetIcon()
        {
            this.Icon = DefaultIcon;
        }

        private void Set_ErrorManager(object newDataSource, string newDataMember, bool force)
        {
            if (!this.inSetErrorManager)
            {
                this.inSetErrorManager = true;
                try
                {
                    bool flag = this.DataSource != newDataSource;
                    bool flag2 = this.DataMember != newDataMember;
                    if ((flag || flag2) || force)
                    {
                        this.dataSource = newDataSource;
                        this.dataMember = newDataMember;
                        if (this.initializing)
                        {
                            this.setErrorManagerOnEndInit = true;
                        }
                        else
                        {
                            this.UnwireEvents(this.errorManager);
                            if (((this.parentControl != null) && (this.dataSource != null)) && (this.parentControl.BindingContext != null))
                            {
                                this.errorManager = this.parentControl.BindingContext[this.dataSource, this.dataMember];
                            }
                            else
                            {
                                this.errorManager = null;
                            }
                            this.WireEvents(this.errorManager);
                            if (this.errorManager != null)
                            {
                                this.UpdateBinding();
                            }
                        }
                    }
                }
                finally
                {
                    this.inSetErrorManager = false;
                }
            }
        }

        public void SetError(Control control, string value)
        {
            this.EnsureControlItem(control).Error = value;
        }

        public void SetIconAlignment(Control control, ErrorIconAlignment value)
        {
            this.EnsureControlItem(control).IconAlignment = value;
        }

        public void SetIconPadding(Control control, int padding)
        {
            this.EnsureControlItem(control).IconPadding = padding;
        }

        private bool ShouldSerializeDataMember()
        {
            return ((this.dataMember != null) && (this.dataMember.Length != 0));
        }

        private bool ShouldSerializeDataSource()
        {
            return (this.dataSource != null);
        }

        private bool ShouldSerializeIcon()
        {
            return (this.Icon != DefaultIcon);
        }

        void ISupportInitialize.BeginInit()
        {
            this.initializing = true;
        }

        void ISupportInitialize.EndInit()
        {
            ISupportInitializeNotification dataSource = this.DataSource as ISupportInitializeNotification;
            if ((dataSource != null) && !dataSource.IsInitialized)
            {
                dataSource.Initialized += new EventHandler(this.DataSource_Initialized);
            }
            else
            {
                this.EndInitCore();
            }
        }

        private void UnwireEvents(BindingManagerBase listManager)
        {
            if (listManager != null)
            {
                listManager.CurrentChanged -= this.currentChanged;
                listManager.BindingComplete -= new BindingCompleteEventHandler(this.ErrorManager_BindingComplete);
                CurrencyManager manager = listManager as CurrencyManager;
                if (manager != null)
                {
                    manager.ItemChanged -= new ItemChangedEventHandler(this.ErrorManager_ItemChanged);
                    manager.Bindings.CollectionChanged -= new CollectionChangeEventHandler(this.ErrorManager_BindingsChanged);
                }
            }
        }

        public void UpdateBinding()
        {
            this.ErrorManager_CurrentChanged(this.errorManager, EventArgs.Empty);
        }

        private void WireEvents(BindingManagerBase listManager)
        {
            if (listManager != null)
            {
                listManager.CurrentChanged += this.currentChanged;
                listManager.BindingComplete += new BindingCompleteEventHandler(this.ErrorManager_BindingComplete);
                CurrencyManager manager = listManager as CurrencyManager;
                if (manager != null)
                {
                    manager.ItemChanged += new ItemChangedEventHandler(this.ErrorManager_ItemChanged);
                    manager.Bindings.CollectionChanged += new CollectionChangeEventHandler(this.ErrorManager_BindingsChanged);
                }
            }
        }

        [System.Windows.Forms.SRDescription("ErrorProviderBlinkRateDescr"), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(250)]
        public int BlinkRate
        {
            get
            {
                return this.blinkRate;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("BlinkRate", value, System.Windows.Forms.SR.GetString("BlinkRateMustBeZeroOrMore"));
                }
                this.blinkRate = value;
                if (this.blinkRate == 0)
                {
                    this.BlinkStyle = ErrorBlinkStyle.NeverBlink;
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(0), System.Windows.Forms.SRDescription("ErrorProviderBlinkStyleDescr")]
        public ErrorBlinkStyle BlinkStyle
        {
            get
            {
                if (this.blinkRate == 0)
                {
                    return ErrorBlinkStyle.NeverBlink;
                }
                return this.blinkStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ErrorBlinkStyle));
                }
                if (this.blinkRate == 0)
                {
                    value = ErrorBlinkStyle.NeverBlink;
                }
                if (this.blinkStyle != value)
                {
                    if (value == ErrorBlinkStyle.AlwaysBlink)
                    {
                        this.showIcon = true;
                        this.blinkStyle = ErrorBlinkStyle.AlwaysBlink;
                        foreach (ErrorWindow window in this.windows.Values)
                        {
                            window.StartBlinking();
                        }
                    }
                    else if (this.blinkStyle == ErrorBlinkStyle.AlwaysBlink)
                    {
                        this.blinkStyle = value;
                        foreach (ErrorWindow window2 in this.windows.Values)
                        {
                            window2.StopBlinking();
                        }
                    }
                    else
                    {
                        this.blinkStyle = value;
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("ErrorProviderContainerControlDescr"), DefaultValue((string) null), System.Windows.Forms.SRCategory("CatData")]
        public System.Windows.Forms.ContainerControl ContainerControl
        {
            [UIPermission(SecurityAction.InheritanceDemand, Window=UIPermissionWindow.AllWindows), UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
            get
            {
                return this.parentControl;
            }
            set
            {
                if (this.parentControl != value)
                {
                    if (this.parentControl != null)
                    {
                        this.parentControl.BindingContextChanged -= this.propChangedEvent;
                    }
                    this.parentControl = value;
                    if (this.parentControl != null)
                    {
                        this.parentControl.BindingContextChanged += this.propChangedEvent;
                    }
                    this.Set_ErrorManager(this.DataSource, this.DataMember, true);
                }
            }
        }

        [Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), System.Windows.Forms.SRDescription("ErrorProviderDataMemberDescr"), DefaultValue((string) null), System.Windows.Forms.SRCategory("CatData")]
        public string DataMember
        {
            get
            {
                return this.dataMember;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                this.Set_ErrorManager(this.DataSource, value, false);
            }
        }

        [System.Windows.Forms.SRCategory("CatData"), AttributeProvider(typeof(IListSource)), DefaultValue((string) null), System.Windows.Forms.SRDescription("ErrorProviderDataSourceDescr")]
        public object DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                if (((this.parentControl != null) && (value != null)) && !string.IsNullOrEmpty(this.dataMember))
                {
                    try
                    {
                        this.errorManager = this.parentControl.BindingContext[value, this.dataMember];
                    }
                    catch (ArgumentException)
                    {
                        this.dataMember = "";
                    }
                }
                this.Set_ErrorManager(value, this.DataMember, false);
            }
        }

        private static System.Drawing.Icon DefaultIcon
        {
            get
            {
                if (defaultIcon == null)
                {
                    lock (typeof(ErrorProvider))
                    {
                        if (defaultIcon == null)
                        {
                            defaultIcon = new System.Drawing.Icon(typeof(ErrorProvider), "Error.ico");
                        }
                    }
                }
                return defaultIcon;
            }
        }

        [System.Windows.Forms.SRDescription("ErrorProviderIconDescr"), System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true)]
        public System.Drawing.Icon Icon
        {
            get
            {
                return this.icon;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.icon = value;
                this.DisposeRegion();
                ErrorWindow[] array = new ErrorWindow[this.windows.Values.Count];
                this.windows.Values.CopyTo(array, 0);
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].Update(false);
                }
            }
        }

        internal IconRegion Region
        {
            get
            {
                if (this.region == null)
                {
                    this.region = new IconRegion(this.Icon);
                }
                return this.region;
            }
        }

        [DefaultValue(false), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ControlRightToLeftDescr")]
        public virtual bool RightToLeft
        {
            get
            {
                return this.rightToLeft;
            }
            set
            {
                if (value != this.rightToLeft)
                {
                    this.rightToLeft = value;
                    this.OnRightToLeftChanged(EventArgs.Empty);
                }
            }
        }

        public override ISite Site
        {
            set
            {
                base.Site = value;
                if (value != null)
                {
                    IDesignerHost service = value.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (service != null)
                    {
                        IComponent rootComponent = service.RootComponent;
                        if (rootComponent is System.Windows.Forms.ContainerControl)
                        {
                            this.ContainerControl = (System.Windows.Forms.ContainerControl) rootComponent;
                        }
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatData"), Localizable(false), DefaultValue((string) null), Bindable(true), TypeConverter(typeof(StringConverter)), System.Windows.Forms.SRDescription("ControlTagDescr")]
        public object Tag
        {
            get
            {
                return this.userData;
            }
            set
            {
                this.userData = value;
            }
        }

        internal class ControlItem
        {
            private int blinkPhase;
            private Control control;
            private string error = string.Empty;
            private ErrorIconAlignment iconAlignment = ErrorIconAlignment.MiddleRight;
            private int iconPadding;
            private IntPtr id;
            private ErrorProvider provider;
            private const int startingBlinkPhase = 10;
            private bool toolTipShown = false;
            private ErrorProvider.ErrorWindow window;

            public ControlItem(ErrorProvider provider, Control control, IntPtr id)
            {
                this.id = id;
                this.control = control;
                this.provider = provider;
                this.control.HandleCreated += new EventHandler(this.OnCreateHandle);
                this.control.HandleDestroyed += new EventHandler(this.OnDestroyHandle);
                this.control.LocationChanged += new EventHandler(this.OnBoundsChanged);
                this.control.SizeChanged += new EventHandler(this.OnBoundsChanged);
                this.control.VisibleChanged += new EventHandler(this.OnParentVisibleChanged);
                this.control.ParentChanged += new EventHandler(this.OnParentVisibleChanged);
            }

            private void AddToWindow()
            {
                if (((this.window == null) && (this.control.Created || this.control.RecreatingHandle)) && ((this.control.Visible && (this.control.ParentInternal != null)) && (this.error.Length > 0)))
                {
                    this.window = this.provider.EnsureErrorWindow(this.control.ParentInternal);
                    this.window.Add(this);
                    if (this.provider.BlinkStyle != ErrorBlinkStyle.NeverBlink)
                    {
                        this.StartBlinking();
                    }
                }
            }

            public void Dispose()
            {
                if (this.control != null)
                {
                    this.control.HandleCreated -= new EventHandler(this.OnCreateHandle);
                    this.control.HandleDestroyed -= new EventHandler(this.OnDestroyHandle);
                    this.control.LocationChanged -= new EventHandler(this.OnBoundsChanged);
                    this.control.SizeChanged -= new EventHandler(this.OnBoundsChanged);
                    this.control.VisibleChanged -= new EventHandler(this.OnParentVisibleChanged);
                    this.control.ParentChanged -= new EventHandler(this.OnParentVisibleChanged);
                }
                this.error = string.Empty;
            }

            internal Rectangle GetIconBounds(Size size)
            {
                int x = 0;
                int y = 0;
                switch (this.RTLTranslateIconAlignment(this.IconAlignment))
                {
                    case ErrorIconAlignment.TopLeft:
                    case ErrorIconAlignment.MiddleLeft:
                    case ErrorIconAlignment.BottomLeft:
                        x = (this.control.Left - size.Width) - this.iconPadding;
                        break;

                    case ErrorIconAlignment.TopRight:
                    case ErrorIconAlignment.MiddleRight:
                    case ErrorIconAlignment.BottomRight:
                        x = this.control.Right + this.iconPadding;
                        break;
                }
                switch (this.IconAlignment)
                {
                    case ErrorIconAlignment.TopLeft:
                    case ErrorIconAlignment.TopRight:
                        y = this.control.Top;
                        break;

                    case ErrorIconAlignment.MiddleLeft:
                    case ErrorIconAlignment.MiddleRight:
                        y = this.control.Top + ((this.control.Height - size.Height) / 2);
                        break;

                    case ErrorIconAlignment.BottomLeft:
                    case ErrorIconAlignment.BottomRight:
                        y = this.control.Bottom - size.Height;
                        break;
                }
                return new Rectangle(x, y, size.Width, size.Height);
            }

            private void OnBoundsChanged(object sender, EventArgs e)
            {
                this.UpdateWindow();
            }

            private void OnCreateHandle(object sender, EventArgs e)
            {
                this.AddToWindow();
            }

            private void OnDestroyHandle(object sender, EventArgs e)
            {
                this.RemoveFromWindow();
            }

            private void OnParentVisibleChanged(object sender, EventArgs e)
            {
                this.BlinkPhase = 0;
                this.RemoveFromWindow();
                this.AddToWindow();
            }

            private void RemoveFromWindow()
            {
                if (this.window != null)
                {
                    this.window.Remove(this);
                    this.window = null;
                }
            }

            internal ErrorIconAlignment RTLTranslateIconAlignment(ErrorIconAlignment align)
            {
                if (this.provider.RightToLeft)
                {
                    switch (align)
                    {
                        case ErrorIconAlignment.TopLeft:
                            return ErrorIconAlignment.TopRight;

                        case ErrorIconAlignment.TopRight:
                            return ErrorIconAlignment.TopLeft;

                        case ErrorIconAlignment.MiddleLeft:
                            return ErrorIconAlignment.MiddleRight;

                        case ErrorIconAlignment.MiddleRight:
                            return ErrorIconAlignment.MiddleLeft;

                        case ErrorIconAlignment.BottomLeft:
                            return ErrorIconAlignment.BottomRight;

                        case ErrorIconAlignment.BottomRight:
                            return ErrorIconAlignment.BottomLeft;
                    }
                }
                return align;
            }

            private void StartBlinking()
            {
                if (this.window != null)
                {
                    this.BlinkPhase = 10;
                    this.window.StartBlinking();
                }
            }

            private void UpdateWindow()
            {
                if (this.window != null)
                {
                    this.window.Update(false);
                }
            }

            public int BlinkPhase
            {
                get
                {
                    return this.blinkPhase;
                }
                set
                {
                    this.blinkPhase = value;
                }
            }

            public string Error
            {
                get
                {
                    return this.error;
                }
                set
                {
                    if (value == null)
                    {
                        value = "";
                    }
                    if (!this.error.Equals(value) || (this.provider.BlinkStyle == ErrorBlinkStyle.AlwaysBlink))
                    {
                        bool flag = this.error.Length == 0;
                        this.error = value;
                        if (value.Length == 0)
                        {
                            this.RemoveFromWindow();
                        }
                        else if (flag)
                        {
                            this.AddToWindow();
                        }
                        else if (this.provider.BlinkStyle != ErrorBlinkStyle.NeverBlink)
                        {
                            this.StartBlinking();
                        }
                        else
                        {
                            this.UpdateWindow();
                        }
                    }
                }
            }

            public ErrorIconAlignment IconAlignment
            {
                get
                {
                    return this.iconAlignment;
                }
                set
                {
                    if (this.iconAlignment != value)
                    {
                        if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 5))
                        {
                            throw new InvalidEnumArgumentException("value", (int) value, typeof(ErrorIconAlignment));
                        }
                        this.iconAlignment = value;
                        this.UpdateWindow();
                    }
                }
            }

            public int IconPadding
            {
                get
                {
                    return this.iconPadding;
                }
                set
                {
                    if (this.iconPadding != value)
                    {
                        this.iconPadding = value;
                        this.UpdateWindow();
                    }
                }
            }

            public IntPtr Id
            {
                get
                {
                    return this.id;
                }
            }

            public bool ToolTipShown
            {
                get
                {
                    return this.toolTipShown;
                }
                set
                {
                    this.toolTipShown = value;
                }
            }
        }

        internal class ErrorWindow : NativeWindow
        {
            private ArrayList items = new ArrayList();
            private DeviceContext mirrordc;
            private Size mirrordcExtent = Size.Empty;
            private DeviceContextMapMode mirrordcMode = DeviceContextMapMode.Text;
            private Point mirrordcOrigin = Point.Empty;
            private Control parent;
            private ErrorProvider provider;
            private Timer timer;
            private NativeWindow tipWindow;
            private Rectangle windowBounds = Rectangle.Empty;

            public ErrorWindow(ErrorProvider provider, Control parent)
            {
                this.provider = provider;
                this.parent = parent;
            }

            public void Add(ErrorProvider.ControlItem item)
            {
                this.items.Add(item);
                if (this.EnsureCreated())
                {
                    System.Windows.Forms.NativeMethods.TOOLINFO_T toolinfo_t;
                    toolinfo_t = new System.Windows.Forms.NativeMethods.TOOLINFO_T {
                        cbSize = Marshal.SizeOf(toolinfo_t),
                        hwnd = base.Handle,
                        uId = item.Id,
                        lpszText = item.Error,
                        uFlags = 0x10
                    };
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.tipWindow, this.tipWindow.Handle), System.Windows.Forms.NativeMethods.TTM_ADDTOOL, 0, toolinfo_t);
                    this.Update(false);
                }
            }

            private void CreateMirrorDC(IntPtr hdc, int originOffset)
            {
                this.mirrordc = DeviceContext.FromHdc(hdc);
                if (this.parent.IsMirrored && (this.mirrordc != null))
                {
                    this.mirrordc.SaveHdc();
                    this.mirrordcExtent = this.mirrordc.ViewportExtent;
                    this.mirrordcOrigin = this.mirrordc.ViewportOrigin;
                    this.mirrordcMode = this.mirrordc.SetMapMode(DeviceContextMapMode.Anisotropic);
                    this.mirrordc.ViewportExtent = new Size(-this.mirrordcExtent.Width, this.mirrordcExtent.Height);
                    this.mirrordc.ViewportOrigin = new Point(this.mirrordcOrigin.X + originOffset, this.mirrordcOrigin.Y);
                }
            }

            public void Dispose()
            {
                this.EnsureDestroyed();
            }

            private bool EnsureCreated()
            {
                if (base.Handle == IntPtr.Zero)
                {
                    System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX initcommoncontrolsex;
                    if (!this.parent.IsHandleCreated)
                    {
                        return false;
                    }
                    CreateParams cp = new CreateParams {
                        Caption = string.Empty,
                        Style = 0x50000000,
                        ClassStyle = 8,
                        X = 0,
                        Y = 0,
                        Width = 0,
                        Height = 0,
                        Parent = this.parent.Handle
                    };
                    this.CreateHandle(cp);
                    initcommoncontrolsex = new System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX {
                        dwICC = 8,
                        dwSize = Marshal.SizeOf(initcommoncontrolsex)
                    };
                    System.Windows.Forms.SafeNativeMethods.InitCommonControlsEx(initcommoncontrolsex);
                    cp = new CreateParams {
                        Parent = base.Handle,
                        ClassName = "tooltips_class32",
                        Style = 1
                    };
                    this.tipWindow = new NativeWindow();
                    this.tipWindow.CreateHandle(cp);
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.tipWindow, this.tipWindow.Handle), 0x418, 0, SystemInformation.MaxWindowTrackSize.Width);
                    System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this.tipWindow, this.tipWindow.Handle), System.Windows.Forms.NativeMethods.HWND_TOP, 0, 0, 0, 0, 0x13);
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.tipWindow, this.tipWindow.Handle), 0x403, 3, 0);
                }
                return true;
            }

            private void EnsureDestroyed()
            {
                if (this.timer != null)
                {
                    this.timer.Dispose();
                    this.timer = null;
                }
                if (this.tipWindow != null)
                {
                    this.tipWindow.DestroyHandle();
                    this.tipWindow = null;
                }
                System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.HWND_TOP, this.windowBounds.X, this.windowBounds.Y, this.windowBounds.Width, this.windowBounds.Height, 0x83);
                if (this.parent != null)
                {
                    this.parent.Invalidate(true);
                }
                this.DestroyHandle();
                if (this.mirrordc != null)
                {
                    this.mirrordc.Dispose();
                }
            }

            private void OnPaint(ref Message m)
            {
                System.Windows.Forms.NativeMethods.PAINTSTRUCT lpPaint = new System.Windows.Forms.NativeMethods.PAINTSTRUCT();
                IntPtr hdc = System.Windows.Forms.UnsafeNativeMethods.BeginPaint(new HandleRef(this, base.Handle), ref lpPaint);
                this.CreateMirrorDC(hdc, this.windowBounds.Width - 1);
                try
                {
                    for (int i = 0; i < this.items.Count; i++)
                    {
                        Rectangle iconBounds = ((ErrorProvider.ControlItem) this.items[i]).GetIconBounds(this.provider.Region.Size);
                        System.Windows.Forms.SafeNativeMethods.DrawIconEx(new HandleRef(this, this.mirrordc.Hdc), iconBounds.X - this.windowBounds.X, iconBounds.Y - this.windowBounds.Y, new HandleRef(this.provider.Region, this.provider.Region.IconHandle), iconBounds.Width, iconBounds.Height, 0, System.Windows.Forms.NativeMethods.NullHandleRef, 3);
                    }
                }
                finally
                {
                    this.RestoreMirrorDC();
                }
                System.Windows.Forms.UnsafeNativeMethods.EndPaint(new HandleRef(this, base.Handle), ref lpPaint);
            }

            protected override void OnThreadException(Exception e)
            {
                Application.OnThreadException(e);
            }

            private void OnTimer(object sender, EventArgs e)
            {
                int num = 0;
                for (int i = 0; i < this.items.Count; i++)
                {
                    num += ((ErrorProvider.ControlItem) this.items[i]).BlinkPhase;
                }
                if ((num == 0) && (this.provider.BlinkStyle != ErrorBlinkStyle.AlwaysBlink))
                {
                    this.timer.Stop();
                }
                this.Update(true);
            }

            private void OnToolTipVisibilityChanging(IntPtr id, bool toolTipShown)
            {
                for (int i = 0; i < this.items.Count; i++)
                {
                    if (((ErrorProvider.ControlItem) this.items[i]).Id == id)
                    {
                        ((ErrorProvider.ControlItem) this.items[i]).ToolTipShown = toolTipShown;
                    }
                }
            }

            public void Remove(ErrorProvider.ControlItem item)
            {
                this.items.Remove(item);
                if (this.tipWindow != null)
                {
                    System.Windows.Forms.NativeMethods.TOOLINFO_T toolinfo_t;
                    toolinfo_t = new System.Windows.Forms.NativeMethods.TOOLINFO_T {
                        cbSize = Marshal.SizeOf(toolinfo_t),
                        hwnd = base.Handle,
                        uId = item.Id
                    };
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.tipWindow, this.tipWindow.Handle), System.Windows.Forms.NativeMethods.TTM_DELTOOL, 0, toolinfo_t);
                }
                if (this.items.Count == 0)
                {
                    this.EnsureDestroyed();
                }
                else
                {
                    this.Update(false);
                }
            }

            private void RestoreMirrorDC()
            {
                if (this.parent.IsMirrored && (this.mirrordc != null))
                {
                    this.mirrordc.ViewportExtent = this.mirrordcExtent;
                    this.mirrordc.ViewportOrigin = this.mirrordcOrigin;
                    this.mirrordc.SetMapMode(this.mirrordcMode);
                    this.mirrordc.RestoreHdc();
                    this.mirrordc.Dispose();
                }
                this.mirrordc = null;
                this.mirrordcExtent = Size.Empty;
                this.mirrordcOrigin = Point.Empty;
                this.mirrordcMode = DeviceContextMapMode.Text;
            }

            internal void StartBlinking()
            {
                if (this.timer == null)
                {
                    this.timer = new Timer();
                    this.timer.Tick += new EventHandler(this.OnTimer);
                }
                this.timer.Interval = this.provider.BlinkRate;
                this.timer.Start();
                this.Update(false);
            }

            internal void StopBlinking()
            {
                if (this.timer != null)
                {
                    this.timer.Stop();
                }
                this.Update(false);
            }

            public void Update(bool timerCaused)
            {
                ErrorProvider.IconRegion region = this.provider.Region;
                Size size = region.Size;
                this.windowBounds = Rectangle.Empty;
                for (int i = 0; i < this.items.Count; i++)
                {
                    Rectangle iconBounds = ((ErrorProvider.ControlItem) this.items[i]).GetIconBounds(size);
                    if (this.windowBounds.IsEmpty)
                    {
                        this.windowBounds = iconBounds;
                    }
                    else
                    {
                        this.windowBounds = Rectangle.Union(this.windowBounds, iconBounds);
                    }
                }
                Region wrapper = new Region(new Rectangle(0, 0, 0, 0));
                IntPtr zero = IntPtr.Zero;
                try
                {
                    for (int j = 0; j < this.items.Count; j++)
                    {
                        ErrorProvider.ControlItem item2 = (ErrorProvider.ControlItem) this.items[j];
                        Rectangle rectangle2 = item2.GetIconBounds(size);
                        rectangle2.X -= this.windowBounds.X;
                        rectangle2.Y -= this.windowBounds.Y;
                        bool flag = true;
                        if (!item2.ToolTipShown)
                        {
                            switch (this.provider.BlinkStyle)
                            {
                                case ErrorBlinkStyle.BlinkIfDifferentError:
                                    flag = (item2.BlinkPhase == 0) || ((item2.BlinkPhase > 0) && ((item2.BlinkPhase & 1) == (j & 1)));
                                    break;

                                case ErrorBlinkStyle.AlwaysBlink:
                                    flag = ((j & 1) == 0) == this.provider.showIcon;
                                    break;
                            }
                        }
                        if (flag)
                        {
                            region.Region.Translate(rectangle2.X, rectangle2.Y);
                            wrapper.Union(region.Region);
                            region.Region.Translate(-rectangle2.X, -rectangle2.Y);
                        }
                        if (this.tipWindow != null)
                        {
                            System.Windows.Forms.NativeMethods.TOOLINFO_T toolinfo_t;
                            toolinfo_t = new System.Windows.Forms.NativeMethods.TOOLINFO_T {
                                cbSize = Marshal.SizeOf(toolinfo_t),
                                hwnd = base.Handle,
                                uId = item2.Id,
                                lpszText = item2.Error,
                                rect = System.Windows.Forms.NativeMethods.RECT.FromXYWH(rectangle2.X, rectangle2.Y, rectangle2.Width, rectangle2.Height),
                                uFlags = 0x10
                            };
                            if (this.provider.RightToLeft)
                            {
                                toolinfo_t.uFlags |= 4;
                            }
                            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.tipWindow, this.tipWindow.Handle), System.Windows.Forms.NativeMethods.TTM_SETTOOLINFO, 0, toolinfo_t);
                        }
                        if (timerCaused && (item2.BlinkPhase > 0))
                        {
                            item2.BlinkPhase--;
                        }
                    }
                    if (timerCaused)
                    {
                        this.provider.showIcon = !this.provider.showIcon;
                    }
                    DeviceContext context = null;
                    using (context = DeviceContext.FromHwnd(base.Handle))
                    {
                        this.CreateMirrorDC(context.Hdc, this.windowBounds.Width);
                        Graphics g = Graphics.FromHdcInternal(this.mirrordc.Hdc);
                        try
                        {
                            zero = wrapper.GetHrgn(g);
                            System.Internal.HandleCollector.Add(zero, System.Windows.Forms.NativeMethods.CommonHandles.GDI);
                        }
                        finally
                        {
                            g.Dispose();
                            this.RestoreMirrorDC();
                        }
                        if (System.Windows.Forms.UnsafeNativeMethods.SetWindowRgn(new HandleRef(this, base.Handle), new HandleRef(wrapper, zero), true) != 0)
                        {
                            zero = IntPtr.Zero;
                        }
                    }
                }
                finally
                {
                    wrapper.Dispose();
                    if (zero != IntPtr.Zero)
                    {
                        System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, zero));
                    }
                }
                System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.HWND_TOP, this.windowBounds.X, this.windowBounds.Y, this.windowBounds.Width, this.windowBounds.Height, 0x10);
                System.Windows.Forms.SafeNativeMethods.InvalidateRect(new HandleRef(this, base.Handle), (System.Windows.Forms.NativeMethods.COMRECT) null, false);
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case 15:
                        this.OnPaint(ref m);
                        return;

                    case 20:
                        break;

                    case 0x4e:
                    {
                        System.Windows.Forms.NativeMethods.NMHDR lParam = (System.Windows.Forms.NativeMethods.NMHDR) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHDR));
                        if ((lParam.code == -521) || (lParam.code == -522))
                        {
                            this.OnToolTipVisibilityChanging(lParam.idFrom, lParam.code == -521);
                            return;
                        }
                        break;
                    }
                    default:
                        base.WndProc(ref m);
                        break;
                }
            }
        }

        internal class IconRegion
        {
            private Icon icon;
            private System.Drawing.Region region;

            public IconRegion(Icon icon)
            {
                this.icon = new Icon(icon, 0x10, 0x10);
            }

            public void Dispose()
            {
                if (this.region != null)
                {
                    this.region.Dispose();
                    this.region = null;
                }
                this.icon.Dispose();
            }

            public IntPtr IconHandle
            {
                get
                {
                    return this.icon.Handle;
                }
            }

            public System.Drawing.Region Region
            {
                get
                {
                    if (this.region == null)
                    {
                        this.region = new System.Drawing.Region(new Rectangle(0, 0, 0, 0));
                        IntPtr zero = IntPtr.Zero;
                        try
                        {
                            System.Drawing.Size size = this.icon.Size;
                            Bitmap bitmap = this.icon.ToBitmap();
                            bitmap.MakeTransparent();
                            zero = ControlPaint.CreateHBitmapTransparencyMask(bitmap);
                            bitmap.Dispose();
                            int num = size.Width / 8;
                            byte[] lpvBits = new byte[num * size.Height];
                            System.Windows.Forms.SafeNativeMethods.GetBitmapBits(new HandleRef(null, zero), lpvBits.Length, lpvBits);
                            for (int i = 0; i < size.Height; i++)
                            {
                                for (int j = 0; j < size.Width; j++)
                                {
                                    if ((lpvBits[(i * num) + (j / 8)] & (((int) 1) << (7 - (j % 8)))) == 0)
                                    {
                                        this.region.Union(new Rectangle(j, i, 1, 1));
                                    }
                                }
                            }
                            this.region.Intersect(new Rectangle(0, 0, size.Width, size.Height));
                        }
                        finally
                        {
                            if (zero != IntPtr.Zero)
                            {
                                System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, zero));
                            }
                        }
                    }
                    return this.region;
                }
            }

            public System.Drawing.Size Size
            {
                get
                {
                    return this.icon.Size;
                }
            }
        }
    }
}

