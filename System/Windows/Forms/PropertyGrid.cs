namespace System.Windows.Forms
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.ComponentModel.Com2Interop;
    using System.Windows.Forms.Design;
    using System.Windows.Forms.PropertyGridInternal;

    [ComVisible(true), Designer("System.Windows.Forms.Design.PropertyGridDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), System.Windows.Forms.SRDescription("DescriptionPropertyGrid"), ClassInterface(ClassInterfaceType.AutoDispatch), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class PropertyGrid : ContainerControl, IComPropertyBrowser, System.Windows.Forms.UnsafeNativeMethods.IPropertyNotifySink
    {
        private const int ALPHA = 1;
        private const ushort BatchMode = 0x10;
        private const ushort BatchModeChange = 0x100;
        private Bitmap bmpAlpha;
        private Bitmap bmpCategory;
        private Bitmap bmpPropPage;
        private AttributeCollection browsableAttributes;
        private ToolStripButton btnViewPropertyPages;
        private int buttonType;
        private const int CATEGORIES = 0;
        private System.Drawing.Color categoryForeColor = SystemColors.ControlText;
        private AxHost.ConnectionPointCookie[] connectionPointCookies;
        private object[] currentObjects;
        private GridEntryCollection currentPropEntries;
        private const int CXINDENT = 0;
        private const int CYDIVIDER = 3;
        private const int CYINDENT = 2;
        private int dcSizeRatio = -1;
        private IDesignerEventService designerEventService;
        private IDesignerHost designerHost;
        private Hashtable designerSelections;
        private int dividerMoveY = -1;
        private DocComment doccomment;
        private bool drawFlatToolBar;
        private int dwMsg;
        private static object EventComComponentNameChanged = new object();
        private static object EventPropertySortChanged = new object();
        private static object EventPropertyTabChanged = new object();
        private static object EventPropertyValueChanged = new object();
        private const int EVENTS = 1;
        private static object EventSelectedGridItemChanged = new object();
        private static object EventSelectedObjectsChanged = new object();
        private ushort flags;
        private const ushort FullRefreshAfterBatch = 0x80;
        private const ushort GotDesignerEventService = 2;
        private PropertyGridView gridView;
        private int hcSizeRatio = -1;
        private bool helpVisible = true;
        private HotCommands hotcommands;
        private ImageList[] imageList = new ImageList[2];
        private const ushort InternalChange = 4;
        private const int LARGE_BUTTONS = 1;
        internal Brush lineBrush;
        private System.Drawing.Color lineColor = SystemColors.InactiveBorder;
        private const int MIN_GRID_HEIGHT = 20;
        private const int NO_SORT = 2;
        private const int NORMAL_BUTTONS = 0;
        private readonly ComponentEventHandler onComponentAdd;
        private readonly ComponentChangedEventHandler onComponentChanged;
        private readonly ComponentEventHandler onComponentRemove;
        private int paintFrozen;
        private GridEntry peDefault;
        private GridEntry peMain;
        private const int PROPERTIES = 0;
        private const ushort PropertiesChanged = 1;
        private System.Windows.Forms.PropertySort propertySortValue;
        private string propName;
        private const ushort RefreshingProperties = 0x200;
        private const ushort ReInitTab = 0x20;
        private int selectedViewSort;
        private int selectedViewTab;
        private ToolStripSeparator separator1;
        private ToolStripSeparator separator2;
        private const ushort SysColorChangeRefresh = 0x40;
        private const ushort TabsChanging = 8;
        private SnappableControl targetMove;
        private bool toolbarVisible = true;
        private ToolStrip toolStrip;
        private ToolStripButton[] viewSortButtons;
        private ToolStripButton[] viewTabButtons;
        private Hashtable viewTabProps;
        private PropertyTab[] viewTabs = new PropertyTab[0];
        private PropertyTabScope[] viewTabScopes = new PropertyTabScope[0];
        private bool viewTabsDirty = true;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler BackgroundImageChanged
        {
            add
            {
                base.BackgroundImageChanged += value;
            }
            remove
            {
                base.BackgroundImageChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler BackgroundImageLayoutChanged
        {
            add
            {
                base.BackgroundImageLayoutChanged += value;
            }
            remove
            {
                base.BackgroundImageLayoutChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler ForeColorChanged
        {
            add
            {
                base.ForeColorChanged += value;
            }
            remove
            {
                base.ForeColorChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public event KeyEventHandler KeyDown
        {
            add
            {
                base.KeyDown += value;
            }
            remove
            {
                base.KeyDown -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public event KeyPressEventHandler KeyPress
        {
            add
            {
                base.KeyPress += value;
            }
            remove
            {
                base.KeyPress -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public event KeyEventHandler KeyUp
        {
            add
            {
                base.KeyUp += value;
            }
            remove
            {
                base.KeyUp -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public event MouseEventHandler MouseDown
        {
            add
            {
                base.MouseDown += value;
            }
            remove
            {
                base.MouseDown -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public event EventHandler MouseEnter
        {
            add
            {
                base.MouseEnter += value;
            }
            remove
            {
                base.MouseEnter -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler MouseLeave
        {
            add
            {
                base.MouseLeave += value;
            }
            remove
            {
                base.MouseLeave -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public event MouseEventHandler MouseMove
        {
            add
            {
                base.MouseMove += value;
            }
            remove
            {
                base.MouseMove -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public event MouseEventHandler MouseUp
        {
            add
            {
                base.MouseUp += value;
            }
            remove
            {
                base.MouseUp -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler PaddingChanged
        {
            add
            {
                base.PaddingChanged += value;
            }
            remove
            {
                base.PaddingChanged -= value;
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("PropertyGridPropertySortChangedDescr")]
        public event EventHandler PropertySortChanged
        {
            add
            {
                base.Events.AddHandler(EventPropertySortChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPropertySortChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("PropertyGridPropertyTabchangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event PropertyTabChangedEventHandler PropertyTabChanged
        {
            add
            {
                base.Events.AddHandler(EventPropertyTabChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPropertyTabChanged, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("PropertyGridPropertyValueChangedDescr")]
        public event PropertyValueChangedEventHandler PropertyValueChanged
        {
            add
            {
                base.Events.AddHandler(EventPropertyValueChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPropertyValueChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("PropertyGridSelectedGridItemChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event SelectedGridItemChangedEventHandler SelectedGridItemChanged
        {
            add
            {
                base.Events.AddHandler(EventSelectedGridItemChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSelectedGridItemChanged, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("PropertyGridSelectedObjectsChangedDescr")]
        public event EventHandler SelectedObjectsChanged
        {
            add
            {
                base.Events.AddHandler(EventSelectedObjectsChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSelectedObjectsChanged, value);
            }
        }

        event ComponentRenameEventHandler IComPropertyBrowser.ComComponentNameChanged
        {
            add
            {
                base.Events.AddHandler(EventComComponentNameChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventComComponentNameChanged, value);
            }
        }

        [Browsable(false)]
        public event EventHandler TextChanged
        {
            add
            {
                base.TextChanged += value;
            }
            remove
            {
                base.TextChanged -= value;
            }
        }

        public PropertyGrid()
        {
            this.onComponentAdd = new ComponentEventHandler(this.OnComponentAdd);
            this.onComponentRemove = new ComponentEventHandler(this.OnComponentRemove);
            this.onComponentChanged = new ComponentChangedEventHandler(this.OnComponentChanged);
            base.SuspendLayout();
            base.AutoScaleMode = AutoScaleMode.None;
            try
            {
                this.gridView = this.CreateGridView(null);
                this.gridView.TabStop = true;
                this.gridView.MouseMove += new MouseEventHandler(this.OnChildMouseMove);
                this.gridView.MouseDown += new MouseEventHandler(this.OnChildMouseDown);
                this.gridView.TabIndex = 2;
                this.separator1 = this.CreateSeparatorButton();
                this.separator2 = this.CreateSeparatorButton();
                this.toolStrip = new ToolStrip();
                this.toolStrip.SuspendLayout();
                this.toolStrip.ShowItemToolTips = true;
                this.toolStrip.AccessibleName = System.Windows.Forms.SR.GetString("PropertyGridToolbarAccessibleName");
                this.toolStrip.AccessibleRole = AccessibleRole.ToolBar;
                this.toolStrip.TabStop = true;
                this.toolStrip.AllowMerge = false;
                this.toolStrip.Text = "PropertyGridToolBar";
                this.toolStrip.Dock = DockStyle.None;
                this.toolStrip.AutoSize = false;
                this.toolStrip.TabIndex = 1;
                this.toolStrip.CanOverflow = false;
                this.toolStrip.GripStyle = ToolStripGripStyle.Hidden;
                System.Windows.Forms.Padding padding = this.toolStrip.Padding;
                padding.Left = 2;
                this.toolStrip.Padding = padding;
                this.SetToolStripRenderer();
                this.AddRefTab(this.DefaultTabType, null, PropertyTabScope.Static, true);
                this.doccomment = new DocComment(this);
                this.doccomment.SuspendLayout();
                this.doccomment.TabStop = false;
                this.doccomment.Dock = DockStyle.None;
                this.doccomment.BackColor = SystemColors.Control;
                this.doccomment.ForeColor = SystemColors.ControlText;
                this.doccomment.MouseMove += new MouseEventHandler(this.OnChildMouseMove);
                this.doccomment.MouseDown += new MouseEventHandler(this.OnChildMouseDown);
                this.hotcommands = new HotCommands(this);
                this.hotcommands.SuspendLayout();
                this.hotcommands.TabIndex = 3;
                this.hotcommands.Dock = DockStyle.None;
                this.SetHotCommandColors(false);
                this.hotcommands.Visible = false;
                this.hotcommands.MouseMove += new MouseEventHandler(this.OnChildMouseMove);
                this.hotcommands.MouseDown += new MouseEventHandler(this.OnChildMouseDown);
                this.Controls.AddRange(new Control[] { this.doccomment, this.hotcommands, this.gridView, this.toolStrip });
                base.SetActiveControlInternal(this.gridView);
                this.toolStrip.ResumeLayout(false);
                this.SetupToolbar();
                this.PropertySort = System.Windows.Forms.PropertySort.CategorizedAlphabetical;
                this.Text = "PropertyGrid";
                this.SetSelectState(0);
            }
            catch (Exception)
            {
            }
            finally
            {
                if (this.doccomment != null)
                {
                    this.doccomment.ResumeLayout(false);
                }
                if (this.hotcommands != null)
                {
                    this.hotcommands.ResumeLayout(false);
                }
                base.ResumeLayout(true);
            }
        }

        private int AddImage(Bitmap image)
        {
            image.MakeTransparent();
            int count = this.imageList[0].Images.Count;
            this.imageList[0].Images.Add(image);
            return count;
        }

        internal void AddRefTab(System.Type tabType, object component, PropertyTabScope type, bool setupToolbar)
        {
            PropertyTab tab = null;
            int length = -1;
            if (this.viewTabs != null)
            {
                for (int i = 0; i < this.viewTabs.Length; i++)
                {
                    if (tabType == this.viewTabs[i].GetType())
                    {
                        tab = this.viewTabs[i];
                        length = i;
                        break;
                    }
                }
            }
            else
            {
                length = 0;
            }
            if (tab == null)
            {
                IDesignerHost service = null;
                if (((component != null) && (component is IComponent)) && (((IComponent) component).Site != null))
                {
                    service = (IDesignerHost) ((IComponent) component).Site.GetService(typeof(IDesignerHost));
                }
                try
                {
                    tab = this.CreateTab(tabType, service);
                }
                catch (Exception)
                {
                    return;
                }
                if (this.viewTabs != null)
                {
                    length = this.viewTabs.Length;
                    if (tabType == this.DefaultTabType)
                    {
                        length = 0;
                    }
                    else if (typeof(EventsTab).IsAssignableFrom(tabType))
                    {
                        length = 1;
                    }
                    else
                    {
                        for (int j = 1; j < this.viewTabs.Length; j++)
                        {
                            if (!(this.viewTabs[j] is EventsTab) && (string.Compare(tab.TabName, this.viewTabs[j].TabName, false, CultureInfo.InvariantCulture) < 0))
                            {
                                length = j;
                                break;
                            }
                        }
                    }
                }
                PropertyTab[] destinationArray = new PropertyTab[this.viewTabs.Length + 1];
                Array.Copy(this.viewTabs, 0, destinationArray, 0, length);
                Array.Copy(this.viewTabs, length, destinationArray, length + 1, this.viewTabs.Length - length);
                destinationArray[length] = tab;
                this.viewTabs = destinationArray;
                this.viewTabsDirty = true;
                PropertyTabScope[] scopeArray = new PropertyTabScope[this.viewTabScopes.Length + 1];
                Array.Copy(this.viewTabScopes, 0, scopeArray, 0, length);
                Array.Copy(this.viewTabScopes, length, scopeArray, length + 1, this.viewTabScopes.Length - length);
                scopeArray[length] = type;
                this.viewTabScopes = scopeArray;
            }
            if ((tab != null) && (component != null))
            {
                try
                {
                    object[] components = tab.Components;
                    int num4 = (components == null) ? 0 : components.Length;
                    object[] objArray2 = new object[num4 + 1];
                    if (num4 > 0)
                    {
                        Array.Copy(components, objArray2, num4);
                    }
                    objArray2[num4] = component;
                    tab.Components = objArray2;
                }
                catch (Exception)
                {
                    this.RemoveTab(length, false);
                }
            }
            if (setupToolbar)
            {
                this.SetupToolbar();
                this.ShowEventsButton(false);
            }
        }

        internal void AddTab(System.Type tabType, PropertyTabScope scope)
        {
            this.AddRefTab(tabType, null, scope, true);
        }

        private void ClearCachedProps()
        {
            if (this.viewTabProps != null)
            {
                this.viewTabProps.Clear();
            }
        }

        internal void ClearTabs(PropertyTabScope tabScope)
        {
            if (tabScope < PropertyTabScope.Document)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("PropertyGridTabScope"));
            }
            this.RemoveTabs(tabScope, true);
        }

        internal void ClearValueCaches()
        {
            if (this.peMain != null)
            {
                this.peMain.ClearCachedValues();
            }
        }

        public void CollapseAllGridItems()
        {
            this.gridView.RecursivelyExpand(this.peMain, false, false, -1);
        }

        private PropertyGridView CreateGridView(IServiceProvider sp)
        {
            return new PropertyGridView(sp, this);
        }

        protected virtual PropertyTab CreatePropertyTab(System.Type tabType)
        {
            return null;
        }

        private ToolStripButton CreatePushButton(string toolTipText, int imageIndex, EventHandler eventHandler)
        {
            ToolStripButton button = new ToolStripButton {
                Text = toolTipText,
                AutoToolTip = true,
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                ImageIndex = imageIndex
            };
            button.Click += eventHandler;
            button.ImageScaling = ToolStripItemImageScaling.SizeToFit;
            return button;
        }

        private ToolStripSeparator CreateSeparatorButton()
        {
            return new ToolStripSeparator();
        }

        private PropertyTab CreateTab(System.Type tabType, IDesignerHost host)
        {
            PropertyTab tab = this.CreatePropertyTab(tabType);
            if (tab == null)
            {
                ConstructorInfo constructor = tabType.GetConstructor(new System.Type[] { typeof(IServiceProvider) });
                object site = null;
                if (constructor == null)
                {
                    constructor = tabType.GetConstructor(new System.Type[] { typeof(IDesignerHost) });
                    if (constructor != null)
                    {
                        site = host;
                    }
                }
                else
                {
                    site = this.Site;
                }
                if ((site != null) && (constructor != null))
                {
                    tab = (PropertyTab) constructor.Invoke(new object[] { site });
                }
                else
                {
                    tab = (PropertyTab) Activator.CreateInstance(tabType);
                }
            }
            if (tab != null)
            {
                Bitmap original = tab.Bitmap;
                if (original == null)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("PropertyGridNoBitmap", new object[] { tab.GetType().FullName }));
                }
                Size size = original.Size;
                if ((size.Width != 0x10) || (size.Height != 0x10))
                {
                    original = new Bitmap(original, new Size(0x10, 0x10));
                }
                string tabName = tab.TabName;
                if ((tabName == null) || (tabName.Length == 0))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("PropertyGridTabName", new object[] { tab.GetType().FullName }));
                }
            }
            return tab;
        }

        private void DisplayHotCommands()
        {
            bool visible = this.hotcommands.Visible;
            IComponent component = null;
            DesignerVerb[] array = null;
            if ((this.currentObjects != null) && (this.currentObjects.Length > 0))
            {
                for (int i = 0; i < this.currentObjects.Length; i++)
                {
                    object unwrappedObject = this.GetUnwrappedObject(i);
                    if (unwrappedObject is IComponent)
                    {
                        component = (IComponent) unwrappedObject;
                        break;
                    }
                }
                if (component != null)
                {
                    ISite site = component.Site;
                    if (site != null)
                    {
                        IMenuCommandService service = (IMenuCommandService) site.GetService(typeof(IMenuCommandService));
                        if (service != null)
                        {
                            array = new DesignerVerb[service.Verbs.Count];
                            service.Verbs.CopyTo(array, 0);
                        }
                        else if ((this.currentObjects.Length == 1) && (this.GetUnwrappedObject(0) is IComponent))
                        {
                            IDesignerHost host = (IDesignerHost) site.GetService(typeof(IDesignerHost));
                            if (host != null)
                            {
                                IDesigner designer = host.GetDesigner(component);
                                if (designer != null)
                                {
                                    array = new DesignerVerb[designer.Verbs.Count];
                                    designer.Verbs.CopyTo(array, 0);
                                }
                            }
                        }
                    }
                }
            }
            if (!base.DesignMode)
            {
                if ((array != null) && (array.Length > 0))
                {
                    this.hotcommands.SetVerbs(component, array);
                }
                else
                {
                    this.hotcommands.SetVerbs(null, null);
                }
                if (visible != this.hotcommands.Visible)
                {
                    this.OnLayoutInternal(false);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.GetFlag(2))
                {
                    if (this.designerEventService != null)
                    {
                        this.designerEventService.ActiveDesignerChanged -= new ActiveDesignerEventHandler(this.OnActiveDesignerChanged);
                    }
                    this.designerEventService = null;
                    this.SetFlag(2, false);
                }
                this.ActiveDesigner = null;
                if (this.viewTabs != null)
                {
                    for (int i = 0; i < this.viewTabs.Length; i++)
                    {
                        this.viewTabs[i].Dispose();
                    }
                    this.viewTabs = null;
                }
                if (this.imageList != null)
                {
                    for (int j = 0; j < this.imageList.Length; j++)
                    {
                        if (this.imageList[j] != null)
                        {
                            this.imageList[j].Dispose();
                        }
                    }
                    this.imageList = null;
                }
                if (this.bmpAlpha != null)
                {
                    this.bmpAlpha.Dispose();
                    this.bmpAlpha = null;
                }
                if (this.bmpCategory != null)
                {
                    this.bmpCategory.Dispose();
                    this.bmpCategory = null;
                }
                if (this.bmpPropPage != null)
                {
                    this.bmpPropPage.Dispose();
                    this.bmpPropPage = null;
                }
                if (this.lineBrush != null)
                {
                    this.lineBrush.Dispose();
                    this.lineBrush = null;
                }
                if (this.peMain != null)
                {
                    this.peMain.Dispose();
                    this.peMain = null;
                }
                if (this.currentObjects != null)
                {
                    this.currentObjects = null;
                    this.SinkPropertyNotifyEvents();
                }
                this.ClearCachedProps();
                this.currentPropEntries = null;
            }
            base.Dispose(disposing);
        }

        private void DividerDraw(int y)
        {
            if (y != -1)
            {
                Rectangle bounds = this.gridView.Bounds;
                bounds.Y = y - 3;
                bounds.Height = 3;
                DrawXorBar(this, bounds);
            }
        }

        private SnappableControl DividerInside(int x, int y)
        {
            int num = -1;
            if (this.hotcommands.Visible)
            {
                Point location = this.hotcommands.Location;
                if ((y >= (location.Y - 3)) && (y <= (location.Y + 1)))
                {
                    return this.hotcommands;
                }
                num = 0;
            }
            if (this.doccomment.Visible)
            {
                Point point2 = this.doccomment.Location;
                if ((y >= (point2.Y - 3)) && (y <= (point2.Y + 1)))
                {
                    return this.doccomment;
                }
                if (num == -1)
                {
                    num = 1;
                }
            }
            if (num != -1)
            {
                int num2 = this.gridView.Location.Y;
                int num3 = num2 + this.gridView.Size.Height;
                if ((Math.Abs((int) (num3 - y)) <= 1) && (y > num2))
                {
                    switch (num)
                    {
                        case 0:
                            return this.hotcommands;

                        case 1:
                            return this.doccomment;
                    }
                }
            }
            return null;
        }

        private int DividerLimitHigh(SnappableControl target)
        {
            int num = this.gridView.Location.Y + 20;
            if ((target == this.doccomment) && this.hotcommands.Visible)
            {
                num += this.hotcommands.Size.Height + 2;
            }
            return num;
        }

        private int DividerLimitMove(SnappableControl target, int y)
        {
            Rectangle bounds = target.Bounds;
            int num = y;
            num = Math.Min((bounds.Y + bounds.Height) - 15, num);
            return Math.Max(this.DividerLimitHigh(target), num);
        }

        private static void DrawXorBar(Control ctlDrawTo, Rectangle rcFrame)
        {
            Rectangle rectangle = ctlDrawTo.RectangleToScreen(rcFrame);
            if (rectangle.Width < rectangle.Height)
            {
                for (int i = 0; i < rectangle.Width; i++)
                {
                    ControlPaint.DrawReversibleLine(new Point(rectangle.X + i, rectangle.Y), new Point(rectangle.X + i, rectangle.Y + rectangle.Height), ctlDrawTo.BackColor);
                }
            }
            else
            {
                for (int j = 0; j < rectangle.Height; j++)
                {
                    ControlPaint.DrawReversibleLine(new Point(rectangle.X, rectangle.Y + j), new Point(rectangle.X + rectangle.Width, rectangle.Y + j), ctlDrawTo.BackColor);
                }
            }
        }

        internal void DumpPropsToConsole()
        {
            this.gridView.DumpPropsToConsole(this.peMain, "");
        }

        private bool EnablePropPageButton(object obj)
        {
            if (obj == null)
            {
                this.btnViewPropertyPages.Enabled = false;
                return false;
            }
            IUIService service = (IUIService) this.GetService(typeof(IUIService));
            bool flag = false;
            if (service != null)
            {
                flag = service.CanShowComponentEditor(obj);
            }
            else
            {
                flag = TypeDescriptor.GetEditor(obj, typeof(ComponentEditor)) != null;
            }
            this.btnViewPropertyPages.Enabled = flag;
            return flag;
        }

        private void EnableTabs()
        {
            if (this.currentObjects != null)
            {
                this.SetupToolbar();
                for (int i = 1; i < this.viewTabs.Length; i++)
                {
                    bool flag = true;
                    for (int j = 0; j < this.currentObjects.Length; j++)
                    {
                        try
                        {
                            if (!this.viewTabs[i].CanExtend(this.GetUnwrappedObject(j)))
                            {
                                flag = false;
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag != this.viewTabButtons[i].Visible)
                    {
                        this.viewTabButtons[i].Visible = flag;
                        if (!flag && (i == this.selectedViewTab))
                        {
                            this.SelectViewTabButton(this.viewTabButtons[0], true);
                        }
                    }
                }
            }
        }

        private void EnsureDesignerEventService()
        {
            if (!this.GetFlag(2))
            {
                this.designerEventService = (IDesignerEventService) this.GetService(typeof(IDesignerEventService));
                if (this.designerEventService != null)
                {
                    this.SetFlag(2, true);
                    this.designerEventService.ActiveDesignerChanged += new ActiveDesignerEventHandler(this.OnActiveDesignerChanged);
                    this.OnActiveDesignerChanged(null, new ActiveDesignerEventArgs(null, this.designerEventService.ActiveDesigner));
                }
            }
        }

        private void EnsureLargeButtons()
        {
            if (this.imageList[1] == null)
            {
                this.imageList[1] = new ImageList();
                this.imageList[1].ImageSize = new Size(0x20, 0x20);
                ImageList.ImageCollection images = this.imageList[0].Images;
                for (int i = 0; i < images.Count; i++)
                {
                    if (images[i] is Bitmap)
                    {
                        this.imageList[1].Images.Add(new Bitmap((Bitmap) images[i], 0x20, 0x20));
                    }
                }
            }
        }

        public void ExpandAllGridItems()
        {
            this.gridView.RecursivelyExpand(this.peMain, false, true, 10);
        }

        private static System.Type[] GetCommonTabs(object[] objs, PropertyTabScope tabScope)
        {
            int num2;
            if ((objs == null) || (objs.Length == 0))
            {
                return new System.Type[0];
            }
            System.Type[] sourceArray = new System.Type[5];
            int length = 0;
            PropertyTabAttribute attribute = (PropertyTabAttribute) TypeDescriptor.GetAttributes(objs[0])[typeof(PropertyTabAttribute)];
            if (attribute == null)
            {
                return new System.Type[0];
            }
            for (num2 = 0; num2 < attribute.TabScopes.Length; num2++)
            {
                PropertyTabScope scope = attribute.TabScopes[num2];
                if (scope == tabScope)
                {
                    if (length == sourceArray.Length)
                    {
                        System.Type[] typeArray2 = new System.Type[length * 2];
                        Array.Copy(sourceArray, 0, typeArray2, 0, length);
                        sourceArray = typeArray2;
                    }
                    sourceArray[length++] = attribute.TabClasses[num2];
                }
            }
            if (length == 0)
            {
                return new System.Type[0];
            }
            for (num2 = 1; (num2 < objs.Length) && (length > 0); num2++)
            {
                attribute = (PropertyTabAttribute) TypeDescriptor.GetAttributes(objs[num2])[typeof(PropertyTabAttribute)];
                if (attribute == null)
                {
                    return new System.Type[0];
                }
                for (int i = 0; i < length; i++)
                {
                    bool flag = false;
                    for (int j = 0; j < attribute.TabClasses.Length; j++)
                    {
                        if (attribute.TabClasses[j] == sourceArray[i])
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        sourceArray[i] = sourceArray[length - 1];
                        sourceArray[length - 1] = null;
                        length--;
                        i--;
                    }
                }
            }
            System.Type[] destinationArray = new System.Type[length];
            if (length > 0)
            {
                Array.Copy(sourceArray, 0, destinationArray, 0, length);
            }
            return destinationArray;
        }

        private void GetDataFromCopyData(IntPtr lparam)
        {
            System.Windows.Forms.NativeMethods.COPYDATASTRUCT copydatastruct = (System.Windows.Forms.NativeMethods.COPYDATASTRUCT) System.Windows.Forms.UnsafeNativeMethods.PtrToStructure(lparam, typeof(System.Windows.Forms.NativeMethods.COPYDATASTRUCT));
            if ((copydatastruct != null) && (copydatastruct.lpData != IntPtr.Zero))
            {
                this.propName = Marshal.PtrToStringAuto(copydatastruct.lpData);
                this.dwMsg = copydatastruct.dwData;
            }
        }

        internal GridEntry GetDefaultGridEntry()
        {
            if ((this.peDefault == null) && (this.currentPropEntries != null))
            {
                this.peDefault = (GridEntry) this.currentPropEntries[0];
            }
            return this.peDefault;
        }

        private bool GetFlag(ushort flag)
        {
            return ((this.flags & flag) != 0);
        }

        internal GridEntryCollection GetPropEntries()
        {
            if (this.currentPropEntries == null)
            {
                this.UpdateSelection();
            }
            this.SetFlag(1, false);
            return this.currentPropEntries;
        }

        private PropertyGridView GetPropertyGridView()
        {
            return this.gridView;
        }

        private object GetUnwrappedObject(int index)
        {
            if (((this.currentObjects == null) || (index < 0)) || (index > this.currentObjects.Length))
            {
                return null;
            }
            object propertyOwner = this.currentObjects[index];
            if (propertyOwner is ICustomTypeDescriptor)
            {
                propertyOwner = ((ICustomTypeDescriptor) propertyOwner).GetPropertyOwner(null);
            }
            return propertyOwner;
        }

        internal bool HavePropEntriesChanged()
        {
            return this.GetFlag(1);
        }

        private void OnActiveDesignerChanged(object sender, ActiveDesignerEventArgs e)
        {
            if ((e.OldDesigner != null) && (e.OldDesigner == this.designerHost))
            {
                this.ActiveDesigner = null;
            }
            if ((e.NewDesigner != null) && (e.NewDesigner != this.designerHost))
            {
                this.ActiveDesigner = e.NewDesigner;
            }
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            if (sender != this.btnViewPropertyPages)
            {
                this.gridView.FocusInternal();
            }
        }

        private void OnChildMouseDown(object sender, MouseEventArgs me)
        {
            Point empty = Point.Empty;
            if (this.ShouldForwardChildMouseMessage((Control) sender, me, ref empty))
            {
                this.OnMouseDown(new MouseEventArgs(me.Button, me.Clicks, empty.X, empty.Y, me.Delta));
            }
        }

        private void OnChildMouseMove(object sender, MouseEventArgs me)
        {
            Point empty = Point.Empty;
            if (this.ShouldForwardChildMouseMessage((Control) sender, me, ref empty))
            {
                this.OnMouseMove(new MouseEventArgs(me.Button, me.Clicks, empty.X, empty.Y, me.Delta));
            }
        }

        protected void OnComComponentNameChanged(ComponentRenameEventArgs e)
        {
            ComponentRenameEventHandler handler = (ComponentRenameEventHandler) base.Events[EventComComponentNameChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnComponentAdd(object sender, ComponentEventArgs e)
        {
            PropertyTabAttribute attribute = (PropertyTabAttribute) TypeDescriptor.GetAttributes(e.Component.GetType())[typeof(PropertyTabAttribute)];
            if (attribute != null)
            {
                for (int i = 0; i < attribute.TabClasses.Length; i++)
                {
                    if (attribute.TabScopes[i] == PropertyTabScope.Document)
                    {
                        this.AddRefTab(attribute.TabClasses[i], e.Component, PropertyTabScope.Document, true);
                    }
                }
            }
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            bool flag = this.GetFlag(0x10);
            if ((flag || this.GetFlag(4)) || ((this.gridView.GetInPropertySet() || (this.currentObjects == null)) || (this.currentObjects.Length == 0)))
            {
                if (flag && !this.gridView.GetInPropertySet())
                {
                    this.SetFlag(0x100, true);
                }
            }
            else
            {
                int length = this.currentObjects.Length;
                for (int i = 0; i < length; i++)
                {
                    if (this.currentObjects[i] == e.Component)
                    {
                        this.Refresh(false);
                        return;
                    }
                }
            }
        }

        private void OnComponentRemove(object sender, ComponentEventArgs e)
        {
            PropertyTabAttribute attribute = (PropertyTabAttribute) TypeDescriptor.GetAttributes(e.Component.GetType())[typeof(PropertyTabAttribute)];
            if (attribute != null)
            {
                for (int i = 0; i < attribute.TabClasses.Length; i++)
                {
                    if (attribute.TabScopes[i] == PropertyTabScope.Document)
                    {
                        this.ReleaseTab(attribute.TabClasses[i], e.Component);
                    }
                }
                for (int j = 0; j < this.currentObjects.Length; j++)
                {
                    if (e.Component == this.currentObjects[j])
                    {
                        object[] destinationArray = new object[this.currentObjects.Length - 1];
                        Array.Copy(this.currentObjects, 0, destinationArray, 0, j);
                        if (j < destinationArray.Length)
                        {
                            Array.Copy(this.currentObjects, j + 1, destinationArray, j, destinationArray.Length - j);
                        }
                        if (!this.GetFlag(0x10))
                        {
                            this.SelectedObjects = destinationArray;
                        }
                        else
                        {
                            this.gridView.ClearProps();
                            this.currentObjects = destinationArray;
                            this.SetFlag(0x80, true);
                        }
                    }
                }
                this.SetupToolbar();
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            this.Refresh();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.Refresh();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            if (base.ActiveControl == null)
            {
                base.SetActiveControlInternal(this.gridView);
            }
            else if (!base.ActiveControl.FocusInternal())
            {
                base.SetActiveControlInternal(this.gridView);
            }
        }

        internal void OnGridViewMouseWheel(MouseEventArgs e)
        {
            this.OnMouseWheel(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            this.OnLayoutInternal(false);
            TypeDescriptor.Refreshed += new RefreshEventHandler(this.OnTypeDescriptorRefreshed);
            if ((this.currentObjects != null) && (this.currentObjects.Length > 0))
            {
                this.Refresh(true);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            TypeDescriptor.Refreshed -= new RefreshEventHandler(this.OnTypeDescriptorRefreshed);
            base.OnHandleDestroyed(e);
        }

        private void OnLayoutInternal(bool dividerOnly)
        {
            if (base.IsHandleCreated && base.Visible)
            {
                try
                {
                    this.FreezePainting = true;
                    if (!dividerOnly)
                    {
                        if ((!this.toolStrip.Visible && !this.doccomment.Visible) && !this.hotcommands.Visible)
                        {
                            this.gridView.Location = new Point(0, 0);
                            this.gridView.Size = base.Size;
                            return;
                        }
                        if (this.toolStrip.Visible)
                        {
                            int width = base.Width;
                            int num2 = this.LargeButtons ? 0x29 : 0x19;
                            Rectangle rectangle = new Rectangle(0, 1, width, num2);
                            this.toolStrip.Bounds = rectangle;
                            int y = this.gridView.Location.Y;
                            this.gridView.Location = new Point(0, this.toolStrip.Height + this.toolStrip.Top);
                        }
                        else
                        {
                            this.gridView.Location = new Point(0, 0);
                        }
                    }
                    int height = base.Size.Height;
                    if (height >= 20)
                    {
                        int num5;
                        int num4 = height - (this.gridView.Location.Y + 20);
                        int num6 = 0;
                        int num7 = 0;
                        int optimalHeight = 0;
                        int num9 = 0;
                        if (dividerOnly)
                        {
                            num6 = this.doccomment.Visible ? this.doccomment.Size.Height : 0;
                            num7 = this.hotcommands.Visible ? this.hotcommands.Size.Height : 0;
                        }
                        else
                        {
                            if (this.doccomment.Visible)
                            {
                                optimalHeight = this.doccomment.GetOptimalHeight(base.Size.Width - 3);
                                if (this.doccomment.userSized)
                                {
                                    num6 = this.doccomment.Size.Height;
                                }
                                else if (this.dcSizeRatio != -1)
                                {
                                    num6 = (base.Height * this.dcSizeRatio) / 100;
                                }
                                else
                                {
                                    num6 = optimalHeight;
                                }
                            }
                            if (this.hotcommands.Visible)
                            {
                                num9 = this.hotcommands.GetOptimalHeight(base.Size.Width - 3);
                                if (this.hotcommands.userSized)
                                {
                                    num7 = this.hotcommands.Size.Height;
                                }
                                else if (this.hcSizeRatio != -1)
                                {
                                    num7 = (base.Height * this.hcSizeRatio) / 100;
                                }
                                else
                                {
                                    num7 = num9;
                                }
                            }
                        }
                        if (num6 > 0)
                        {
                            num4 -= 3;
                            if ((num7 == 0) || ((num6 + num7) < num4))
                            {
                                num5 = Math.Min(num6, num4);
                            }
                            else if ((num7 > 0) && (num7 < num4))
                            {
                                num5 = num4 - num7;
                            }
                            else
                            {
                                num5 = Math.Min(num6, (num4 / 2) - 1);
                            }
                            num5 = Math.Max(num5, 6);
                            this.doccomment.SetBounds(0, height - num5, base.Size.Width, num5);
                            if ((num5 <= optimalHeight) && (num5 < num6))
                            {
                                this.doccomment.userSized = false;
                            }
                            else if ((this.dcSizeRatio != -1) || this.doccomment.userSized)
                            {
                                this.dcSizeRatio = (this.doccomment.Height * 100) / base.Height;
                            }
                            this.doccomment.Invalidate();
                            height = this.doccomment.Location.Y - 3;
                            num4 -= num5;
                        }
                        if (num7 > 0)
                        {
                            num4 -= 3;
                            if (num4 > num7)
                            {
                                num5 = Math.Min(num7, num4);
                            }
                            else
                            {
                                num5 = num4;
                            }
                            num5 = Math.Max(num5, 6);
                            if ((num5 <= num9) && (num5 < num7))
                            {
                                this.hotcommands.userSized = false;
                            }
                            else if ((this.hcSizeRatio != -1) || this.hotcommands.userSized)
                            {
                                this.hcSizeRatio = (this.hotcommands.Height * 100) / base.Height;
                            }
                            this.hotcommands.SetBounds(0, height - num5, base.Size.Width, num5);
                            this.hotcommands.Invalidate();
                            height = this.hotcommands.Location.Y - 3;
                        }
                        this.gridView.Size = new Size(base.Size.Width, height - this.gridView.Location.Y);
                    }
                }
                finally
                {
                    this.FreezePainting = false;
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs me)
        {
            SnappableControl control = this.DividerInside(me.X, me.Y);
            if ((control != null) && (me.Button == MouseButtons.Left))
            {
                base.CaptureInternal = true;
                this.targetMove = control;
                this.dividerMoveY = me.Y;
                this.DividerDraw(this.dividerMoveY);
            }
            base.OnMouseDown(me);
        }

        protected override void OnMouseMove(MouseEventArgs me)
        {
            if (this.dividerMoveY == -1)
            {
                if (this.DividerInside(me.X, me.Y) != null)
                {
                    this.Cursor = Cursors.HSplit;
                }
                else
                {
                    this.Cursor = null;
                }
            }
            else
            {
                int num = this.DividerLimitMove(this.targetMove, me.Y);
                if (num != this.dividerMoveY)
                {
                    this.DividerDraw(this.dividerMoveY);
                    this.dividerMoveY = num;
                    this.DividerDraw(this.dividerMoveY);
                }
                base.OnMouseMove(me);
            }
        }

        protected override void OnMouseUp(MouseEventArgs me)
        {
            if (this.dividerMoveY != -1)
            {
                this.Cursor = null;
                this.DividerDraw(this.dividerMoveY);
                this.dividerMoveY = this.DividerLimitMove(this.targetMove, me.Y);
                Rectangle bounds = this.targetMove.Bounds;
                if (this.dividerMoveY != bounds.Y)
                {
                    int num = ((bounds.Height + bounds.Y) - this.dividerMoveY) - 1;
                    Size size = this.targetMove.Size;
                    size.Height = Math.Max(0, num);
                    this.targetMove.Size = size;
                    this.targetMove.userSized = true;
                    this.OnLayoutInternal(true);
                    base.Invalidate(new Rectangle(0, me.Y - 3, base.Size.Width, me.Y + 3));
                    this.gridView.Invalidate(new Rectangle(0, this.gridView.Size.Height - 3, base.Size.Width, 3));
                }
                base.CaptureInternal = false;
                this.dividerMoveY = -1;
                this.targetMove = null;
                base.OnMouseUp(me);
            }
        }

        protected void OnNotifyPropertyValueUIItemsChanged(object sender, EventArgs e)
        {
            this.gridView.LabelPaintMargin = 0;
            this.gridView.Invalidate(true);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Brush brush;
            Point location = this.gridView.Location;
            int width = base.Size.Width;
            if (this.BackColor.IsSystemColor)
            {
                brush = SystemBrushes.FromSystemColor(this.BackColor);
            }
            else
            {
                brush = new SolidBrush(this.BackColor);
            }
            pevent.Graphics.FillRectangle(brush, new Rectangle(0, 0, width, location.Y));
            int y = location.Y + this.gridView.Size.Height;
            if (this.hotcommands.Visible)
            {
                pevent.Graphics.FillRectangle(brush, new Rectangle(0, y, width, this.hotcommands.Location.Y - y));
                y += this.hotcommands.Size.Height;
            }
            if (this.doccomment.Visible)
            {
                pevent.Graphics.FillRectangle(brush, new Rectangle(0, y, width, this.doccomment.Location.Y - y));
                y += this.doccomment.Size.Height;
            }
            pevent.Graphics.FillRectangle(brush, new Rectangle(0, y, width, base.Size.Height - y));
            if (!this.BackColor.IsSystemColor)
            {
                brush.Dispose();
            }
            base.OnPaint(pevent);
            if (this.lineBrush != null)
            {
                this.lineBrush.Dispose();
                this.lineBrush = null;
            }
        }

        protected virtual void OnPropertySortChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventPropertySortChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPropertyTabChanged(PropertyTabChangedEventArgs e)
        {
            PropertyTabChangedEventHandler handler = (PropertyTabChangedEventHandler) base.Events[EventPropertyTabChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPropertyValueChanged(PropertyValueChangedEventArgs e)
        {
            PropertyValueChangedEventHandler handler = (PropertyValueChangedEventHandler) base.Events[EventPropertyValueChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void OnPropertyValueSet(GridItem changedItem, object oldValue)
        {
            this.OnPropertyValueChanged(new PropertyValueChangedEventArgs(changedItem, oldValue));
        }

        protected override void OnResize(EventArgs e)
        {
            if (base.IsHandleCreated && base.Visible)
            {
                this.OnLayoutInternal(false);
            }
            base.OnResize(e);
        }

        protected virtual void OnSelectedGridItemChanged(SelectedGridItemChangedEventArgs e)
        {
            SelectedGridItemChangedEventHandler handler = (SelectedGridItemChangedEventHandler) base.Events[EventSelectedGridItemChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void OnSelectedGridItemChanged(GridEntry oldEntry, GridEntry newEntry)
        {
            this.OnSelectedGridItemChanged(new SelectedGridItemChangedEventArgs(oldEntry, newEntry));
        }

        protected virtual void OnSelectedObjectsChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventSelectedObjectsChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnSystemColorsChanged(EventArgs e)
        {
            this.SetupToolbar(true);
            if (!this.GetFlag(0x40))
            {
                this.SetupToolbar(true);
                this.SetFlag(0x40, true);
            }
            base.OnSystemColorsChanged(e);
        }

        private void OnTransactionClosed(object sender, DesignerTransactionCloseEventArgs e)
        {
            if (e.LastTransaction)
            {
                IComponent selectedObject = this.SelectedObject as IComponent;
                if ((selectedObject != null) && (selectedObject.Site == null))
                {
                    this.SelectedObject = null;
                }
                else
                {
                    this.SetFlag(0x10, false);
                    if (this.GetFlag(0x80))
                    {
                        this.SelectedObjects = this.currentObjects;
                        this.SetFlag(0x80, false);
                    }
                    else if (this.GetFlag(0x100))
                    {
                        this.Refresh(false);
                    }
                    this.SetFlag(0x100, false);
                }
            }
        }

        private void OnTransactionOpened(object sender, EventArgs e)
        {
            this.SetFlag(0x10, true);
        }

        private void OnTypeDescriptorRefreshed(RefreshEventArgs e)
        {
            if (base.InvokeRequired)
            {
                base.BeginInvoke(new RefreshEventHandler(this.OnTypeDescriptorRefreshedInvoke), new object[] { e });
            }
            else
            {
                this.OnTypeDescriptorRefreshedInvoke(e);
            }
        }

        private void OnTypeDescriptorRefreshedInvoke(RefreshEventArgs e)
        {
            if (this.currentObjects != null)
            {
                for (int i = 0; i < this.currentObjects.Length; i++)
                {
                    System.Type typeChanged = e.TypeChanged;
                    if ((this.currentObjects[i] == e.ComponentChanged) || ((typeChanged != null) && typeChanged.IsAssignableFrom(this.currentObjects[i].GetType())))
                    {
                        this.ClearCachedProps();
                        this.Refresh(true);
                        return;
                    }
                }
            }
        }

        private void OnViewButtonClickPP(object sender, EventArgs e)
        {
            if ((this.btnViewPropertyPages.Enabled && (this.currentObjects != null)) && (this.currentObjects.Length > 0))
            {
                object component = this.currentObjects[0];
                object obj3 = component;
                bool flag = false;
                IUIService service = (IUIService) this.GetService(typeof(IUIService));
                try
                {
                    if (service != null)
                    {
                        flag = service.ShowComponentEditor(obj3, this);
                    }
                    else
                    {
                        try
                        {
                            ComponentEditor editor = (ComponentEditor) TypeDescriptor.GetEditor(obj3, typeof(ComponentEditor));
                            if (editor != null)
                            {
                                if (editor is WindowsFormsComponentEditor)
                                {
                                    flag = ((WindowsFormsComponentEditor) editor).EditComponent(null, obj3, this);
                                }
                                else
                                {
                                    flag = editor.EditComponent(obj3);
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                    if (flag)
                    {
                        if ((component is IComponent) && (this.connectionPointCookies[0] == null))
                        {
                            ISite site = ((IComponent) component).Site;
                            if (site != null)
                            {
                                IComponentChangeService service2 = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                                if (service2 != null)
                                {
                                    try
                                    {
                                        service2.OnComponentChanging(component, null);
                                    }
                                    catch (CheckoutException exception)
                                    {
                                        if (exception != CheckoutException.Canceled)
                                        {
                                            throw exception;
                                        }
                                        return;
                                    }
                                    try
                                    {
                                        this.SetFlag(4, true);
                                        service2.OnComponentChanged(component, null, null, null);
                                    }
                                    finally
                                    {
                                        this.SetFlag(4, false);
                                    }
                                }
                            }
                        }
                        this.gridView.Refresh();
                    }
                }
                catch (Exception exception2)
                {
                    string message = System.Windows.Forms.SR.GetString("ErrorPropertyPageFailed");
                    if (service != null)
                    {
                        service.ShowError(exception2, message);
                    }
                    else
                    {
                        RTLAwareMessageBox.Show(null, message, System.Windows.Forms.SR.GetString("PropertyGridTitle"), MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, 0);
                    }
                }
            }
            this.OnButtonClick(sender, e);
        }

        private void OnViewSortButtonClick(object sender, EventArgs e)
        {
            try
            {
                this.FreezePainting = true;
                if (sender == this.viewSortButtons[this.selectedViewSort])
                {
                    this.viewSortButtons[this.selectedViewSort].Checked = true;
                    return;
                }
                this.viewSortButtons[this.selectedViewSort].Checked = false;
                int index = 0;
                index = 0;
                while (index < this.viewSortButtons.Length)
                {
                    if (this.viewSortButtons[index] == sender)
                    {
                        break;
                    }
                    index++;
                }
                this.selectedViewSort = index;
                this.viewSortButtons[this.selectedViewSort].Checked = true;
                switch (this.selectedViewSort)
                {
                    case 0:
                        this.propertySortValue = System.Windows.Forms.PropertySort.CategorizedAlphabetical;
                        break;

                    case 1:
                        this.propertySortValue = System.Windows.Forms.PropertySort.Alphabetical;
                        break;

                    case 2:
                        this.propertySortValue = System.Windows.Forms.PropertySort.NoSort;
                        break;
                }
                this.OnPropertySortChanged(EventArgs.Empty);
                this.Refresh(false);
                this.OnLayoutInternal(false);
            }
            finally
            {
                this.FreezePainting = false;
            }
            this.OnButtonClick(sender, e);
        }

        private void OnViewTabButtonClick(object sender, EventArgs e)
        {
            try
            {
                this.FreezePainting = true;
                this.SelectViewTabButton((ToolStripButton) sender, true);
                this.OnLayoutInternal(false);
                this.SaveTabSelection();
            }
            finally
            {
                this.FreezePainting = false;
            }
            this.OnButtonClick(sender, e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (base.Visible && base.IsHandleCreated)
            {
                this.OnLayoutInternal(false);
                this.SetupToolbar();
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            Keys keys = keyData & Keys.KeyCode;
            if (((keys != Keys.Tab) || ((keyData & Keys.Control) != Keys.None)) || ((keyData & Keys.Alt) != Keys.None))
            {
                return base.ProcessDialogKey(keyData);
            }
            if ((keyData & Keys.Shift) != Keys.None)
            {
                if (this.hotcommands.Visible && this.hotcommands.ContainsFocus)
                {
                    this.gridView.ReverseFocus();
                }
                else if (this.gridView.FocusInside)
                {
                    if (!this.toolStrip.Visible)
                    {
                        return base.ProcessDialogKey(keyData);
                    }
                    this.toolStrip.FocusInternal();
                }
                else
                {
                    if (this.toolStrip.Focused || !this.toolStrip.Visible)
                    {
                        return base.ProcessDialogKey(keyData);
                    }
                    if (this.hotcommands.Visible)
                    {
                        this.hotcommands.Select(false);
                    }
                    else if (this.peMain != null)
                    {
                        this.gridView.ReverseFocus();
                    }
                    else if (this.toolStrip.Visible)
                    {
                        this.toolStrip.FocusInternal();
                    }
                    else
                    {
                        return base.ProcessDialogKey(keyData);
                    }
                }
                return true;
            }
            bool flag = false;
            if (this.toolStrip.Focused)
            {
                if (this.peMain != null)
                {
                    this.gridView.FocusInternal();
                }
                else
                {
                    base.ProcessDialogKey(keyData);
                }
                return true;
            }
            if (this.gridView.FocusInside)
            {
                if (this.hotcommands.Visible)
                {
                    this.hotcommands.Select(true);
                    return true;
                }
                flag = true;
            }
            else if (this.hotcommands.ContainsFocus)
            {
                flag = true;
            }
            else if (this.toolStrip.Visible)
            {
                this.toolStrip.FocusInternal();
            }
            else
            {
                this.gridView.FocusInternal();
            }
            if (!flag)
            {
                return true;
            }
            bool flag2 = base.ProcessDialogKey(keyData);
            if (!flag2 && (base.Parent == null))
            {
                IntPtr parent = System.Windows.Forms.UnsafeNativeMethods.GetParent(new HandleRef(this, base.Handle));
                if (parent != IntPtr.Zero)
                {
                    System.Windows.Forms.UnsafeNativeMethods.SetFocus(new HandleRef(null, parent));
                }
            }
            return flag2;
        }

        public override void Refresh()
        {
            if (!this.GetFlag(0x200))
            {
                this.Refresh(true);
                base.Refresh();
            }
        }

        private void Refresh(bool clearCached)
        {
            if (!base.Disposing && !this.GetFlag(0x200))
            {
                try
                {
                    this.FreezePainting = true;
                    this.SetFlag(0x200, true);
                    if (clearCached)
                    {
                        this.ClearCachedProps();
                    }
                    this.RefreshProperties(clearCached);
                    this.gridView.Refresh();
                    this.DisplayHotCommands();
                }
                finally
                {
                    this.FreezePainting = false;
                    this.SetFlag(0x200, false);
                }
            }
        }

        internal void RefreshProperties(bool clearCached)
        {
            if ((clearCached && (this.selectedViewTab != -1)) && (this.viewTabs != null))
            {
                PropertyTab tab = this.viewTabs[this.selectedViewTab];
                if ((tab != null) && (this.viewTabProps != null))
                {
                    string key = tab.TabName + this.propertySortValue.ToString();
                    this.viewTabProps.Remove(key);
                }
            }
            this.SetFlag(1, true);
            this.UpdateSelection();
        }

        public void RefreshTabs(PropertyTabScope tabScope)
        {
            if (tabScope < PropertyTabScope.Document)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("PropertyGridTabScope"));
            }
            this.RemoveTabs(tabScope, false);
            if (((tabScope <= PropertyTabScope.Component) && (this.currentObjects != null)) && (this.currentObjects.Length > 0))
            {
                System.Type[] commonTabs = GetCommonTabs(this.currentObjects, PropertyTabScope.Component);
                for (int i = 0; i < commonTabs.Length; i++)
                {
                    for (int j = 0; j < this.currentObjects.Length; j++)
                    {
                        this.AddRefTab(commonTabs[i], this.currentObjects[j], PropertyTabScope.Component, false);
                    }
                }
            }
            if ((tabScope <= PropertyTabScope.Document) && (this.designerHost != null))
            {
                IContainer container = this.designerHost.Container;
                if (container != null)
                {
                    ComponentCollection components = container.Components;
                    if (components != null)
                    {
                        foreach (IComponent component in components)
                        {
                            PropertyTabAttribute attribute = (PropertyTabAttribute) TypeDescriptor.GetAttributes(component.GetType())[typeof(PropertyTabAttribute)];
                            if (attribute != null)
                            {
                                for (int k = 0; k < attribute.TabClasses.Length; k++)
                                {
                                    if (attribute.TabScopes[k] == PropertyTabScope.Document)
                                    {
                                        this.AddRefTab(attribute.TabClasses[k], component, PropertyTabScope.Document, false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            this.SetupToolbar();
        }

        internal void ReleaseTab(System.Type tabType, object component)
        {
            PropertyTab tab = null;
            int index = -1;
            for (int i = 0; i < this.viewTabs.Length; i++)
            {
                if (tabType == this.viewTabs[i].GetType())
                {
                    tab = this.viewTabs[i];
                    index = i;
                    break;
                }
            }
            if (tab != null)
            {
                object[] components = tab.Components;
                bool flag = false;
                try
                {
                    int length = -1;
                    if (components != null)
                    {
                        length = Array.IndexOf<object>(components, component);
                    }
                    if (length >= 0)
                    {
                        object[] destinationArray = new object[components.Length - 1];
                        Array.Copy(components, 0, destinationArray, 0, length);
                        Array.Copy(components, length + 1, destinationArray, length, (components.Length - length) - 1);
                        components = destinationArray;
                        tab.Components = components;
                    }
                    flag = components.Length == 0;
                }
                catch (Exception)
                {
                    flag = true;
                }
                if (flag && (this.viewTabScopes[index] > PropertyTabScope.Global))
                {
                    this.RemoveTab(index, false);
                }
            }
        }

        private void RemoveImage(int index)
        {
            this.imageList[0].Images.RemoveAt(index);
            if (this.imageList[1] != null)
            {
                this.imageList[1].Images.RemoveAt(index);
            }
        }

        internal void RemoveTab(System.Type tabType)
        {
            int length = -1;
            for (int i = 0; i < this.viewTabs.Length; i++)
            {
                if (tabType == this.viewTabs[i].GetType())
                {
                    PropertyTab tab1 = this.viewTabs[i];
                    length = i;
                    break;
                }
            }
            if (length != -1)
            {
                PropertyTab[] destinationArray = new PropertyTab[this.viewTabs.Length - 1];
                Array.Copy(this.viewTabs, 0, destinationArray, 0, length);
                Array.Copy(this.viewTabs, length + 1, destinationArray, length, (this.viewTabs.Length - length) - 1);
                this.viewTabs = destinationArray;
                PropertyTabScope[] scopeArray = new PropertyTabScope[this.viewTabScopes.Length - 1];
                Array.Copy(this.viewTabScopes, 0, scopeArray, 0, length);
                Array.Copy(this.viewTabScopes, length + 1, scopeArray, length, (this.viewTabScopes.Length - length) - 1);
                this.viewTabScopes = scopeArray;
                this.viewTabsDirty = true;
                this.SetupToolbar();
            }
        }

        internal void RemoveTab(int tabIndex, bool setupToolbar)
        {
            if ((tabIndex >= this.viewTabs.Length) || (tabIndex < 0))
            {
                throw new ArgumentOutOfRangeException("tabIndex", System.Windows.Forms.SR.GetString("PropertyGridBadTabIndex"));
            }
            if (this.viewTabScopes[tabIndex] == PropertyTabScope.Static)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("PropertyGridRemoveStaticTabs"));
            }
            if (this.selectedViewTab == tabIndex)
            {
                this.selectedViewTab = 0;
            }
            if (!this.GetFlag(0x20) && (this.ActiveDesigner != null))
            {
                int hashCode = this.ActiveDesigner.GetHashCode();
                if (((this.designerSelections != null) && this.designerSelections.ContainsKey(hashCode)) && (((int) this.designerSelections[hashCode]) == tabIndex))
                {
                    this.designerSelections.Remove(hashCode);
                }
            }
            ToolStripButton button = this.viewTabButtons[this.selectedViewTab];
            PropertyTab[] destinationArray = new PropertyTab[this.viewTabs.Length - 1];
            Array.Copy(this.viewTabs, 0, destinationArray, 0, tabIndex);
            Array.Copy(this.viewTabs, tabIndex + 1, destinationArray, tabIndex, (this.viewTabs.Length - tabIndex) - 1);
            this.viewTabs = destinationArray;
            PropertyTabScope[] scopeArray = new PropertyTabScope[this.viewTabScopes.Length - 1];
            Array.Copy(this.viewTabScopes, 0, scopeArray, 0, tabIndex);
            Array.Copy(this.viewTabScopes, tabIndex + 1, scopeArray, tabIndex, (this.viewTabScopes.Length - tabIndex) - 1);
            this.viewTabScopes = scopeArray;
            this.viewTabsDirty = true;
            if (setupToolbar)
            {
                this.SetupToolbar();
                this.selectedViewTab = -1;
                this.SelectViewTabButtonDefault(button);
            }
        }

        internal void RemoveTabs(PropertyTabScope classification, bool setupToolbar)
        {
            if (classification == PropertyTabScope.Static)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("PropertyGridRemoveStaticTabs"));
            }
            if (((this.viewTabButtons != null) && (this.viewTabs != null)) && (this.viewTabScopes != null))
            {
                ToolStripButton button = ((this.selectedViewTab >= 0) && (this.selectedViewTab < this.viewTabButtons.Length)) ? this.viewTabButtons[this.selectedViewTab] : null;
                for (int i = this.viewTabs.Length - 1; i >= 0; i--)
                {
                    if (this.viewTabScopes[i] >= classification)
                    {
                        if (this.selectedViewTab == i)
                        {
                            this.selectedViewTab = -1;
                        }
                        else if (this.selectedViewTab > i)
                        {
                            this.selectedViewTab--;
                        }
                        PropertyTab[] destinationArray = new PropertyTab[this.viewTabs.Length - 1];
                        Array.Copy(this.viewTabs, 0, destinationArray, 0, i);
                        Array.Copy(this.viewTabs, i + 1, destinationArray, i, (this.viewTabs.Length - i) - 1);
                        this.viewTabs = destinationArray;
                        PropertyTabScope[] scopeArray = new PropertyTabScope[this.viewTabScopes.Length - 1];
                        Array.Copy(this.viewTabScopes, 0, scopeArray, 0, i);
                        Array.Copy(this.viewTabScopes, i + 1, scopeArray, i, (this.viewTabScopes.Length - i) - 1);
                        this.viewTabScopes = scopeArray;
                        this.viewTabsDirty = true;
                    }
                }
                if (setupToolbar && this.viewTabsDirty)
                {
                    this.SetupToolbar();
                    this.selectedViewTab = -1;
                    this.SelectViewTabButtonDefault(button);
                    for (int j = 0; j < this.viewTabs.Length; j++)
                    {
                        this.viewTabs[j].Components = new object[0];
                    }
                }
            }
        }

        internal void ReplaceSelectedObject(object oldObject, object newObject)
        {
            for (int i = 0; i < this.currentObjects.Length; i++)
            {
                if (this.currentObjects[i] == oldObject)
                {
                    this.currentObjects[i] = newObject;
                    this.Refresh(true);
                    return;
                }
            }
        }

        private void ResetCommandsActiveLinkColor()
        {
            this.hotcommands.Label.ResetActiveLinkColor();
        }

        private void ResetCommandsBackColor()
        {
            this.hotcommands.ResetBackColor();
        }

        private void ResetCommandsDisabledLinkColor()
        {
            this.hotcommands.Label.ResetDisabledLinkColor();
        }

        private void ResetCommandsForeColor()
        {
            this.hotcommands.ResetForeColor();
        }

        private void ResetCommandsLinkColor()
        {
            this.hotcommands.Label.ResetLinkColor();
        }

        private void ResetHelpBackColor()
        {
            this.doccomment.ResetBackColor();
        }

        private void ResetHelpForeColor()
        {
            this.doccomment.ResetBackColor();
        }

        public void ResetSelectedProperty()
        {
            this.GetPropertyGridView().Reset();
        }

        private void SaveTabSelection()
        {
            if (this.designerHost != null)
            {
                if (this.designerSelections == null)
                {
                    this.designerSelections = new Hashtable();
                }
                this.designerSelections[this.designerHost.GetHashCode()] = this.selectedViewTab;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void ScaleCore(float dx, float dy)
        {
            int x = (int) Math.Round((double) (base.Left * dx));
            int y = (int) Math.Round((double) (base.Top * dy));
            int width = base.Width;
            width = (int) Math.Round((double) (((base.Left + base.Width) * dx) - x));
            int height = base.Height;
            height = (int) Math.Round((double) (((base.Top + base.Height) * dy) - y));
            base.SetBounds(x, y, width, height, BoundsSpecified.All);
        }

        private void SelectViewTabButton(ToolStripButton button, bool updateSelection)
        {
            this.SelectViewTabButtonDefault(button);
            if (updateSelection)
            {
                this.Refresh(false);
            }
        }

        private bool SelectViewTabButtonDefault(ToolStripButton button)
        {
            if ((this.selectedViewTab >= 0) && (this.selectedViewTab >= this.viewTabButtons.Length))
            {
                this.selectedViewTab = -1;
            }
            if (((this.selectedViewTab >= 0) && (this.selectedViewTab < this.viewTabButtons.Length)) && (button == this.viewTabButtons[this.selectedViewTab]))
            {
                this.viewTabButtons[this.selectedViewTab].Checked = true;
                return true;
            }
            PropertyTab oldTab = null;
            if (this.selectedViewTab != -1)
            {
                this.viewTabButtons[this.selectedViewTab].Checked = false;
                oldTab = this.viewTabs[this.selectedViewTab];
            }
            for (int i = 0; i < this.viewTabButtons.Length; i++)
            {
                if (this.viewTabButtons[i] == button)
                {
                    this.selectedViewTab = i;
                    this.viewTabButtons[i].Checked = true;
                    try
                    {
                        this.SetFlag(8, true);
                        this.OnPropertyTabChanged(new PropertyTabChangedEventArgs(oldTab, this.viewTabs[i]));
                    }
                    finally
                    {
                        this.SetFlag(8, false);
                    }
                    return true;
                }
            }
            this.selectedViewTab = 0;
            this.SelectViewTabButton(this.viewTabButtons[0], false);
            return false;
        }

        private void SetFlag(ushort flag, bool value)
        {
            if (value)
            {
                this.flags = (ushort) (this.flags | flag);
            }
            else
            {
                this.flags = (ushort) (this.flags & ~flag);
            }
        }

        private void SetHotCommandColors(bool vscompat)
        {
            if (vscompat)
            {
                this.hotcommands.SetColors(SystemColors.Control, SystemColors.ControlText, SystemColors.ActiveCaption, SystemColors.ActiveCaption, SystemColors.ActiveCaption, SystemColors.ControlDark);
            }
            else
            {
                this.hotcommands.SetColors(SystemColors.Control, SystemColors.ControlText, System.Drawing.Color.Empty, System.Drawing.Color.Empty, System.Drawing.Color.Empty, System.Drawing.Color.Empty);
            }
        }

        private void SetSelectState(int state)
        {
            if (state >= (this.viewTabs.Length * this.viewSortButtons.Length))
            {
                state = 0;
            }
            else if (state < 0)
            {
                state = (this.viewTabs.Length * this.viewSortButtons.Length) - 1;
            }
            int length = this.viewSortButtons.Length;
            if (length > 0)
            {
                int index = state / length;
                int num3 = state % length;
                this.OnViewTabButtonClick(this.viewTabButtons[index], EventArgs.Empty);
                this.OnViewSortButtonClick(this.viewSortButtons[num3], EventArgs.Empty);
            }
        }

        internal void SetStatusBox(string title, string desc)
        {
            this.doccomment.SetComment(title, desc);
        }

        private void SetToolStripRenderer()
        {
            if (this.DrawFlatToolbar)
            {
                ProfessionalColorTable professionalColorTable = new ProfessionalColorTable {
                    UseSystemColors = true
                };
                this.ToolStripRenderer = new ToolStripProfessionalRenderer(professionalColorTable);
            }
            else
            {
                this.ToolStripRenderer = new ToolStripSystemRenderer();
            }
        }

        private void SetupToolbar()
        {
            this.SetupToolbar(false);
        }

        private void SetupToolbar(bool fullRebuild)
        {
            if (this.viewTabsDirty || fullRebuild)
            {
                try
                {
                    int num;
                    ArrayList list;
                    this.FreezePainting = true;
                    if ((this.imageList[0] == null) || fullRebuild)
                    {
                        this.imageList[0] = new ImageList();
                    }
                    EventHandler eventHandler = new EventHandler(this.OnViewTabButtonClick);
                    EventHandler handler2 = new EventHandler(this.OnViewSortButtonClick);
                    EventHandler handler3 = new EventHandler(this.OnViewButtonClickPP);
                    if (fullRebuild)
                    {
                        list = new ArrayList();
                    }
                    else
                    {
                        list = new ArrayList(this.toolStrip.Items);
                    }
                    if ((this.viewSortButtons == null) || fullRebuild)
                    {
                        this.viewSortButtons = new ToolStripButton[3];
                        int num2 = -1;
                        int num3 = -1;
                        try
                        {
                            if (this.bmpAlpha == null)
                            {
                                this.bmpAlpha = new Bitmap(typeof(PropertyGrid), "PBAlpha.bmp");
                            }
                            num2 = this.AddImage(this.bmpAlpha);
                        }
                        catch (Exception)
                        {
                        }
                        try
                        {
                            if (this.bmpCategory == null)
                            {
                                this.bmpCategory = new Bitmap(typeof(PropertyGrid), "PBCatego.bmp");
                            }
                            num3 = this.AddImage(this.bmpCategory);
                        }
                        catch (Exception)
                        {
                        }
                        this.viewSortButtons[1] = this.CreatePushButton(System.Windows.Forms.SR.GetString("PBRSToolTipAlphabetic"), num2, handler2);
                        this.viewSortButtons[0] = this.CreatePushButton(System.Windows.Forms.SR.GetString("PBRSToolTipCategorized"), num3, handler2);
                        this.viewSortButtons[2] = this.CreatePushButton("", 0, handler2);
                        this.viewSortButtons[2].Visible = false;
                        num = 0;
                        while (num < this.viewSortButtons.Length)
                        {
                            list.Add(this.viewSortButtons[num]);
                            num++;
                        }
                    }
                    else
                    {
                        num = list.Count - 1;
                        while (num >= 2)
                        {
                            list.RemoveAt(num);
                            num--;
                        }
                        num = this.imageList[0].Images.Count - 1;
                        while (num >= 2)
                        {
                            this.RemoveImage(num);
                            num--;
                        }
                    }
                    list.Add(this.separator1);
                    this.viewTabButtons = new ToolStripButton[this.viewTabs.Length];
                    bool flag = this.viewTabs.Length > 1;
                    for (num = 0; num < this.viewTabs.Length; num++)
                    {
                        try
                        {
                            Bitmap image = this.viewTabs[num].Bitmap;
                            this.viewTabButtons[num] = this.CreatePushButton(this.viewTabs[num].TabName, this.AddImage(image), eventHandler);
                            if (flag)
                            {
                                list.Add(this.viewTabButtons[num]);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    if (flag)
                    {
                        list.Add(this.separator2);
                    }
                    int imageIndex = 0;
                    try
                    {
                        if (this.bmpPropPage == null)
                        {
                            this.bmpPropPage = new Bitmap(typeof(PropertyGrid), "PBPPage.bmp");
                        }
                        imageIndex = this.AddImage(this.bmpPropPage);
                    }
                    catch (Exception)
                    {
                    }
                    this.btnViewPropertyPages = this.CreatePushButton(System.Windows.Forms.SR.GetString("PBRSToolTipPropertyPages"), imageIndex, handler3);
                    this.btnViewPropertyPages.Enabled = false;
                    list.Add(this.btnViewPropertyPages);
                    if (this.imageList[1] != null)
                    {
                        this.imageList[1].Dispose();
                        this.imageList[1] = null;
                    }
                    if (this.buttonType != 0)
                    {
                        this.EnsureLargeButtons();
                    }
                    this.toolStrip.ImageList = this.imageList[this.buttonType];
                    this.toolStrip.SuspendLayout();
                    this.toolStrip.Items.Clear();
                    for (int i = 0; i < list.Count; i++)
                    {
                        this.toolStrip.Items.Add(list[i] as ToolStripItem);
                    }
                    this.toolStrip.ResumeLayout();
                    if (this.viewTabsDirty)
                    {
                        this.OnLayoutInternal(false);
                    }
                    this.viewTabsDirty = false;
                }
                finally
                {
                    this.FreezePainting = false;
                }
            }
        }

        private bool ShouldForwardChildMouseMessage(Control child, MouseEventArgs me, ref Point pt)
        {
            Size size = child.Size;
            if ((me.Y > 1) && ((size.Height - me.Y) > 1))
            {
                return false;
            }
            System.Windows.Forms.NativeMethods.POINT point = new System.Windows.Forms.NativeMethods.POINT {
                x = me.X,
                y = me.Y
            };
            System.Windows.Forms.UnsafeNativeMethods.MapWindowPoints(new HandleRef(child, child.Handle), new HandleRef(this, base.Handle), point, 1);
            pt.X = point.x;
            pt.Y = point.y;
            return true;
        }

        private bool ShouldSerializeCommandsActiveLinkColor()
        {
            return this.hotcommands.Label.ShouldSerializeActiveLinkColor();
        }

        private bool ShouldSerializeCommandsBackColor()
        {
            return this.hotcommands.ShouldSerializeBackColor();
        }

        private bool ShouldSerializeCommandsDisabledLinkColor()
        {
            return this.hotcommands.Label.ShouldSerializeDisabledLinkColor();
        }

        private bool ShouldSerializeCommandsForeColor()
        {
            return this.hotcommands.ShouldSerializeForeColor();
        }

        private bool ShouldSerializeCommandsLinkColor()
        {
            return this.hotcommands.Label.ShouldSerializeLinkColor();
        }

        protected void ShowEventsButton(bool value)
        {
            if (((this.viewTabs != null) && (this.viewTabs.Length > 1)) && (this.viewTabs[1] is EventsTab))
            {
                this.viewTabButtons[1].Visible = value;
                if (!value && (this.selectedViewTab == 1))
                {
                    this.SelectViewTabButton(this.viewTabButtons[0], true);
                }
            }
            this.UpdatePropertiesViewTabVisibility();
        }

        private void SinkPropertyNotifyEvents()
        {
            for (int i = 0; (this.connectionPointCookies != null) && (i < this.connectionPointCookies.Length); i++)
            {
                if (this.connectionPointCookies[i] != null)
                {
                    this.connectionPointCookies[i].Disconnect();
                    this.connectionPointCookies[i] = null;
                }
            }
            if ((this.currentObjects == null) || (this.currentObjects.Length == 0))
            {
                this.connectionPointCookies = null;
            }
            else
            {
                if ((this.connectionPointCookies == null) || (this.currentObjects.Length > this.connectionPointCookies.Length))
                {
                    this.connectionPointCookies = new AxHost.ConnectionPointCookie[this.currentObjects.Length];
                }
                for (int j = 0; j < this.currentObjects.Length; j++)
                {
                    try
                    {
                        object unwrappedObject = this.GetUnwrappedObject(j);
                        if (Marshal.IsComObject(unwrappedObject))
                        {
                            this.connectionPointCookies[j] = new AxHost.ConnectionPointCookie(unwrappedObject, this, typeof(System.Windows.Forms.UnsafeNativeMethods.IPropertyNotifySink), false);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        void IComPropertyBrowser.DropDownDone()
        {
            this.GetPropertyGridView().DropDownDone();
        }

        bool IComPropertyBrowser.EnsurePendingChangesCommitted()
        {
            bool flag;
            try
            {
                if (this.designerHost != null)
                {
                    this.designerHost.TransactionOpened -= new EventHandler(this.OnTransactionOpened);
                    this.designerHost.TransactionClosed -= new DesignerTransactionCloseEventHandler(this.OnTransactionClosed);
                }
                flag = this.GetPropertyGridView().EnsurePendingChangesCommitted();
            }
            finally
            {
                if (this.designerHost != null)
                {
                    this.designerHost.TransactionOpened += new EventHandler(this.OnTransactionOpened);
                    this.designerHost.TransactionClosed += new DesignerTransactionCloseEventHandler(this.OnTransactionClosed);
                }
            }
            return flag;
        }

        void IComPropertyBrowser.HandleF4()
        {
            if (!this.gridView.ContainsFocus)
            {
                if (base.ActiveControl != this.gridView)
                {
                    base.SetActiveControlInternal(this.gridView);
                }
                this.gridView.FocusInternal();
            }
        }

        void IComPropertyBrowser.LoadState(RegistryKey optRoot)
        {
            if (optRoot != null)
            {
                object obj2 = optRoot.GetValue("PbrsAlpha", "0");
                if ((obj2 != null) && obj2.ToString().Equals("1"))
                {
                    this.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
                }
                else
                {
                    this.PropertySort = System.Windows.Forms.PropertySort.CategorizedAlphabetical;
                }
                obj2 = optRoot.GetValue("PbrsShowDesc", "1");
                this.HelpVisible = (obj2 != null) && obj2.ToString().Equals("1");
                obj2 = optRoot.GetValue("PbrsShowCommands", "0");
                this.CommandsVisibleIfAvailable = (obj2 != null) && obj2.ToString().Equals("1");
                obj2 = optRoot.GetValue("PbrsDescHeightRatio", "-1");
                bool flag = false;
                if (obj2 is string)
                {
                    int num = int.Parse((string) obj2, CultureInfo.InvariantCulture);
                    if (num > 0)
                    {
                        this.dcSizeRatio = num;
                        flag = true;
                    }
                }
                obj2 = optRoot.GetValue("PbrsHotCommandHeightRatio", "-1");
                if (obj2 is string)
                {
                    int num2 = int.Parse((string) obj2, CultureInfo.InvariantCulture);
                    if (num2 > 0)
                    {
                        this.dcSizeRatio = num2;
                        flag = true;
                    }
                }
                if (flag)
                {
                    this.OnLayoutInternal(false);
                }
            }
            else
            {
                this.PropertySort = System.Windows.Forms.PropertySort.CategorizedAlphabetical;
                this.HelpVisible = true;
                this.CommandsVisibleIfAvailable = false;
            }
        }

        void IComPropertyBrowser.SaveState(RegistryKey optRoot)
        {
            if (optRoot != null)
            {
                optRoot.SetValue("PbrsAlpha", (this.PropertySort == System.Windows.Forms.PropertySort.Alphabetical) ? "1" : "0");
                optRoot.SetValue("PbrsShowDesc", this.HelpVisible ? "1" : "0");
                optRoot.SetValue("PbrsShowCommands", this.CommandsVisibleIfAvailable ? "1" : "0");
                optRoot.SetValue("PbrsDescHeightRatio", this.dcSizeRatio.ToString(CultureInfo.InvariantCulture));
                optRoot.SetValue("PbrsHotCommandHeightRatio", this.hcSizeRatio.ToString(CultureInfo.InvariantCulture));
            }
        }

        void System.Windows.Forms.UnsafeNativeMethods.IPropertyNotifySink.OnChanged(int dispID)
        {
            bool clearCached = false;
            PropertyDescriptorGridEntry selectedGridEntry = this.gridView.SelectedGridEntry as PropertyDescriptorGridEntry;
            if (((selectedGridEntry != null) && (selectedGridEntry.PropertyDescriptor != null)) && (selectedGridEntry.PropertyDescriptor.Attributes != null))
            {
                DispIdAttribute attribute = (DispIdAttribute) selectedGridEntry.PropertyDescriptor.Attributes[typeof(DispIdAttribute)];
                if ((attribute != null) && !attribute.IsDefaultAttribute())
                {
                    clearCached = dispID != attribute.Value;
                }
            }
            if (!this.GetFlag(0x200))
            {
                if (!this.gridView.GetInPropertySet() || clearCached)
                {
                    this.Refresh(clearCached);
                }
                object unwrappedObject = this.GetUnwrappedObject(0);
                if (ComNativeDescriptor.Instance.IsNameDispId(unwrappedObject, dispID) || (dispID == -800))
                {
                    this.OnComComponentNameChanged(new ComponentRenameEventArgs(unwrappedObject, null, TypeDescriptor.GetClassName(unwrappedObject)));
                }
            }
        }

        int System.Windows.Forms.UnsafeNativeMethods.IPropertyNotifySink.OnRequestEdit(int dispID)
        {
            return 0;
        }

        private void UpdatePropertiesViewTabVisibility()
        {
            if (this.viewTabButtons != null)
            {
                int num = 0;
                for (int i = 1; i < this.viewTabButtons.Length; i++)
                {
                    if (this.viewTabButtons[i].Visible)
                    {
                        num++;
                    }
                }
                if (num > 0)
                {
                    this.viewTabButtons[0].Visible = true;
                    this.separator2.Visible = true;
                }
                else
                {
                    this.viewTabButtons[0].Visible = false;
                    this.separator2.Visible = false;
                }
            }
        }

        internal void UpdateSelection()
        {
            if (this.GetFlag(1) && (this.viewTabs != null))
            {
                string key = this.viewTabs[this.selectedViewTab].TabName + this.propertySortValue.ToString();
                if ((this.viewTabProps != null) && this.viewTabProps.ContainsKey(key))
                {
                    this.peMain = (GridEntry) this.viewTabProps[key];
                    if (this.peMain != null)
                    {
                        this.peMain.Refresh();
                    }
                }
                else
                {
                    if ((this.currentObjects != null) && (this.currentObjects.Length > 0))
                    {
                        this.peMain = (GridEntry) GridEntry.Create(this.gridView, this.currentObjects, new PropertyGridServiceProvider(this), this.designerHost, this.SelectedTab, this.propertySortValue);
                    }
                    else
                    {
                        this.peMain = null;
                    }
                    if (this.peMain == null)
                    {
                        this.currentPropEntries = new GridEntryCollection(null, new GridEntry[0]);
                        this.gridView.ClearProps();
                        return;
                    }
                    if (this.BrowsableAttributes != null)
                    {
                        this.peMain.BrowsableAttributes = this.BrowsableAttributes;
                    }
                    if (this.viewTabProps == null)
                    {
                        this.viewTabProps = new Hashtable();
                    }
                    this.viewTabProps[key] = this.peMain;
                }
                this.currentPropEntries = this.peMain.Children;
                this.peDefault = this.peMain.DefaultChild;
                this.gridView.Invalidate();
            }
        }

        internal bool WantsTab(bool forward)
        {
            if (forward)
            {
                return (this.toolStrip.Visible && this.toolStrip.Focused);
            }
            return (this.gridView.ContainsFocus && this.toolStrip.Visible);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x300:
                    if (((long) m.LParam) != 0L)
                    {
                        m.Result = this.CanCut ? ((IntPtr) 1) : IntPtr.Zero;
                        return;
                    }
                    this.gridView.DoCutCommand();
                    return;

                case 0x301:
                    if (((long) m.LParam) != 0L)
                    {
                        m.Result = this.CanCopy ? ((IntPtr) 1) : IntPtr.Zero;
                        return;
                    }
                    this.gridView.DoCopyCommand();
                    return;

                case 770:
                    if (((long) m.LParam) != 0L)
                    {
                        m.Result = this.CanPaste ? ((IntPtr) 1) : IntPtr.Zero;
                        return;
                    }
                    this.gridView.DoPasteCommand();
                    return;

                case 0x304:
                    if (((long) m.LParam) != 0L)
                    {
                        m.Result = this.CanUndo ? ((IntPtr) 1) : IntPtr.Zero;
                        return;
                    }
                    this.gridView.DoUndoCommand();
                    return;

                case 0x4a:
                    this.GetDataFromCopyData(m.LParam);
                    m.Result = (IntPtr) 1;
                    return;

                case 0x450:
                    if (this.toolStrip == null)
                    {
                        goto Label_044D;
                    }
                    m.Result = (IntPtr) this.toolStrip.Items.Count;
                    return;

                case 0x451:
                {
                    if (this.toolStrip == null)
                    {
                        goto Label_044D;
                    }
                    int wParam = (int) ((long) m.WParam);
                    if ((wParam >= 0) && (wParam < this.toolStrip.Items.Count))
                    {
                        ToolStripButton sender = this.toolStrip.Items[wParam] as ToolStripButton;
                        if (sender == null)
                        {
                            return;
                        }
                        sender.Checked = !sender.Checked;
                        if (sender != this.btnViewPropertyPages)
                        {
                            switch (((int) ((long) m.WParam)))
                            {
                                case 0:
                                case 1:
                                    this.OnViewSortButtonClick(sender, EventArgs.Empty);
                                    return;
                            }
                            this.SelectViewTabButton(sender, true);
                            return;
                        }
                        this.OnViewButtonClickPP(sender, EventArgs.Empty);
                    }
                    return;
                }
                case 0x452:
                {
                    if (this.toolStrip == null)
                    {
                        goto Label_044D;
                    }
                    int num = (int) ((long) m.WParam);
                    if ((num < 0) || (num >= this.toolStrip.Items.Count))
                    {
                        break;
                    }
                    ToolStripButton button = this.toolStrip.Items[num] as ToolStripButton;
                    if (button == null)
                    {
                        m.Result = IntPtr.Zero;
                        break;
                    }
                    m.Result = button.Checked ? ((IntPtr) 1) : IntPtr.Zero;
                    return;
                }
                case 0x453:
                case 0x454:
                {
                    if (this.toolStrip == null)
                    {
                        goto Label_044D;
                    }
                    int num3 = (int) ((long) m.WParam);
                    if ((num3 >= 0) && (num3 < this.toolStrip.Items.Count))
                    {
                        string text = "";
                        if (m.Msg != 0x453)
                        {
                            text = this.toolStrip.Items[num3].ToolTipText;
                        }
                        else
                        {
                            text = this.toolStrip.Items[num3].Text;
                        }
                        m.Result = AutomationMessages.WriteAutomationText(text);
                    }
                    return;
                }
                case 0x455:
                    if (m.Msg != this.dwMsg)
                    {
                        goto Label_044D;
                    }
                    m.Result = (IntPtr) this.gridView.GetPropertyLocation(this.propName, m.LParam == IntPtr.Zero, m.WParam == IntPtr.Zero);
                    return;

                case 0x456:
                case 0x457:
                    m.Result = this.gridView.SendMessage(m.Msg, m.WParam, m.LParam);
                    return;

                case 0x458:
                    if (m.LParam != IntPtr.Zero)
                    {
                        string str3 = AutomationMessages.ReadAutomationText(m.LParam);
                        for (int i = 0; i < this.viewTabs.Length; i++)
                        {
                            if ((this.viewTabs[i].GetType().FullName == str3) && this.viewTabButtons[i].Visible)
                            {
                                this.SelectViewTabButtonDefault(this.viewTabButtons[i]);
                                m.Result = (IntPtr) 1;
                                break;
                            }
                        }
                    }
                    m.Result = IntPtr.Zero;
                    return;

                case 0x459:
                {
                    string testingInfo = this.gridView.GetTestingInfo((int) ((long) m.WParam));
                    m.Result = AutomationMessages.WriteAutomationText(testingInfo);
                    return;
                }
                default:
                    goto Label_044D;
            }
            return;
        Label_044D:
            base.WndProc(ref m);
        }

        internal IDesignerHost ActiveDesigner
        {
            get
            {
                if (this.designerHost == null)
                {
                    this.designerHost = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                }
                return this.designerHost;
            }
            set
            {
                if (value != this.designerHost)
                {
                    this.SetFlag(0x20, true);
                    if (this.designerHost != null)
                    {
                        IComponentChangeService service = (IComponentChangeService) this.designerHost.GetService(typeof(IComponentChangeService));
                        if (service != null)
                        {
                            service.ComponentAdded -= this.onComponentAdd;
                            service.ComponentRemoved -= this.onComponentRemove;
                            service.ComponentChanged -= this.onComponentChanged;
                        }
                        IPropertyValueUIService service2 = (IPropertyValueUIService) this.designerHost.GetService(typeof(IPropertyValueUIService));
                        if (service2 != null)
                        {
                            service2.PropertyUIValueItemsChanged -= new EventHandler(this.OnNotifyPropertyValueUIItemsChanged);
                        }
                        this.designerHost.TransactionOpened -= new EventHandler(this.OnTransactionOpened);
                        this.designerHost.TransactionClosed -= new DesignerTransactionCloseEventHandler(this.OnTransactionClosed);
                        this.SetFlag(0x10, false);
                        this.RemoveTabs(PropertyTabScope.Document, true);
                        this.designerHost = null;
                    }
                    if (value != null)
                    {
                        IComponentChangeService service3 = (IComponentChangeService) value.GetService(typeof(IComponentChangeService));
                        if (service3 != null)
                        {
                            service3.ComponentAdded += this.onComponentAdd;
                            service3.ComponentRemoved += this.onComponentRemove;
                            service3.ComponentChanged += this.onComponentChanged;
                        }
                        value.TransactionOpened += new EventHandler(this.OnTransactionOpened);
                        value.TransactionClosed += new DesignerTransactionCloseEventHandler(this.OnTransactionClosed);
                        this.SetFlag(0x10, false);
                        IPropertyValueUIService service4 = (IPropertyValueUIService) value.GetService(typeof(IPropertyValueUIService));
                        if (service4 != null)
                        {
                            service4.PropertyUIValueItemsChanged += new EventHandler(this.OnNotifyPropertyValueUIItemsChanged);
                        }
                    }
                    this.designerHost = value;
                    if (this.peMain != null)
                    {
                        this.peMain.DesignerHost = value;
                    }
                    this.RefreshTabs(PropertyTabScope.Document);
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool AutoScroll
        {
            get
            {
                return base.AutoScroll;
            }
            set
            {
                base.AutoScroll = value;
            }
        }

        public override System.Drawing.Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
                this.toolStrip.BackColor = value;
                this.toolStrip.Invalidate(true);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Advanced)]
        public AttributeCollection BrowsableAttributes
        {
            get
            {
                if (this.browsableAttributes == null)
                {
                    this.browsableAttributes = new AttributeCollection(new Attribute[] { new BrowsableAttribute(true) });
                }
                return this.browsableAttributes;
            }
            set
            {
                if ((value == null) || (value == AttributeCollection.Empty))
                {
                    this.browsableAttributes = new AttributeCollection(new Attribute[] { BrowsableAttribute.Yes });
                }
                else
                {
                    Attribute[] array = new Attribute[value.Count];
                    value.CopyTo(array, 0);
                    this.browsableAttributes = new AttributeCollection(array);
                }
                if (((this.currentObjects != null) && (this.currentObjects.Length > 0)) && (this.peMain != null))
                {
                    this.peMain.BrowsableAttributes = this.BrowsableAttributes;
                    this.Refresh(true);
                }
            }
        }

        private bool CanCopy
        {
            get
            {
                return this.gridView.CanCopy;
            }
        }

        private bool CanCut
        {
            get
            {
                return this.gridView.CanCut;
            }
        }

        private bool CanPaste
        {
            get
            {
                return this.gridView.CanPaste;
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("PropertyGridCanShowCommandsDesc"), EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual bool CanShowCommands
        {
            get
            {
                return this.hotcommands.WouldBeVisible;
            }
        }

        private bool CanUndo
        {
            get
            {
                return this.gridView.CanUndo;
            }
        }

        [DefaultValue(typeof(System.Drawing.Color), "ControlText"), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("PropertyGridCategoryForeColorDesc")]
        public System.Drawing.Color CategoryForeColor
        {
            get
            {
                return this.categoryForeColor;
            }
            set
            {
                if (this.categoryForeColor != value)
                {
                    this.categoryForeColor = value;
                    this.gridView.Invalidate();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("PropertyGridCommandsActiveLinkColorDesc")]
        public System.Drawing.Color CommandsActiveLinkColor
        {
            get
            {
                return this.hotcommands.Label.ActiveLinkColor;
            }
            set
            {
                this.hotcommands.Label.ActiveLinkColor = value;
            }
        }

        [System.Windows.Forms.SRDescription("PropertyGridCommandsBackColorDesc"), System.Windows.Forms.SRCategory("CatAppearance")]
        public System.Drawing.Color CommandsBackColor
        {
            get
            {
                return this.hotcommands.BackColor;
            }
            set
            {
                this.hotcommands.BackColor = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("PropertyGridCommandsDisabledLinkColorDesc")]
        public System.Drawing.Color CommandsDisabledLinkColor
        {
            get
            {
                return this.hotcommands.Label.DisabledLinkColor;
            }
            set
            {
                this.hotcommands.Label.DisabledLinkColor = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("PropertyGridCommandsForeColorDesc")]
        public System.Drawing.Color CommandsForeColor
        {
            get
            {
                return this.hotcommands.ForeColor;
            }
            set
            {
                this.hotcommands.ForeColor = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("PropertyGridCommandsLinkColorDesc")]
        public System.Drawing.Color CommandsLinkColor
        {
            get
            {
                return this.hotcommands.Label.LinkColor;
            }
            set
            {
                this.hotcommands.Label.LinkColor = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual bool CommandsVisible
        {
            get
            {
                return this.hotcommands.Visible;
            }
        }

        [System.Windows.Forms.SRDescription("PropertyGridCommandsVisibleIfAvailable"), DefaultValue(true), System.Windows.Forms.SRCategory("CatAppearance")]
        public virtual bool CommandsVisibleIfAvailable
        {
            get
            {
                return this.hotcommands.AllowVisible;
            }
            set
            {
                bool visible = this.hotcommands.Visible;
                this.hotcommands.AllowVisible = value;
                if (visible != this.hotcommands.Visible)
                {
                    this.OnLayoutInternal(false);
                    this.hotcommands.Invalidate();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Point ContextMenuDefaultLocation
        {
            get
            {
                return this.GetPropertyGridView().ContextMenuDefaultLocation;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public Control.ControlCollection Controls
        {
            get
            {
                return base.Controls;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(130, 130);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual System.Type DefaultTabType
        {
            get
            {
                return typeof(PropertiesTab);
            }
        }

        protected bool DrawFlatToolbar
        {
            get
            {
                return this.drawFlatToolBar;
            }
            set
            {
                if (this.drawFlatToolBar != value)
                {
                    this.drawFlatToolBar = value;
                    this.SetToolStripRenderer();
                }
                this.SetHotCommandColors(value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override System.Drawing.Color ForeColor
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

        private bool FreezePainting
        {
            get
            {
                return (this.paintFrozen > 0);
            }
            set
            {
                if ((value && base.IsHandleCreated) && (base.Visible && (this.paintFrozen++ == 0)))
                {
                    base.SendMessage(11, 0, 0);
                }
                if ((!value && (this.paintFrozen != 0)) && (--this.paintFrozen == 0))
                {
                    base.SendMessage(11, 1, 0);
                    base.Invalidate(true);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(typeof(System.Drawing.Color), "Control"), System.Windows.Forms.SRDescription("PropertyGridHelpBackColorDesc")]
        public System.Drawing.Color HelpBackColor
        {
            get
            {
                return this.doccomment.BackColor;
            }
            set
            {
                this.doccomment.BackColor = value;
            }
        }

        [DefaultValue(typeof(System.Drawing.Color), "ControlText"), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("PropertyGridHelpForeColorDesc")]
        public System.Drawing.Color HelpForeColor
        {
            get
            {
                return this.doccomment.ForeColor;
            }
            set
            {
                this.doccomment.ForeColor = value;
            }
        }

        [Localizable(true), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(true), System.Windows.Forms.SRDescription("PropertyGridHelpVisibleDesc")]
        public virtual bool HelpVisible
        {
            get
            {
                return this.helpVisible;
            }
            set
            {
                this.helpVisible = value;
                this.doccomment.Visible = value;
                this.OnLayoutInternal(false);
                base.Invalidate();
                this.doccomment.Invalidate();
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("PropertyGridLargeButtonsDesc"), DefaultValue(false)]
        public bool LargeButtons
        {
            get
            {
                return (this.buttonType == 1);
            }
            set
            {
                if (value != (this.buttonType == 1))
                {
                    this.buttonType = value ? 1 : 0;
                    if (value)
                    {
                        this.EnsureLargeButtons();
                        if ((this.imageList != null) && (this.imageList[1] != null))
                        {
                            this.toolStrip.ImageScalingSize = this.imageList[1].ImageSize;
                        }
                    }
                    else if ((this.imageList != null) && (this.imageList[0] != null))
                    {
                        this.toolStrip.ImageScalingSize = this.imageList[0].ImageSize;
                    }
                    this.toolStrip.ImageList = this.imageList[this.buttonType];
                    this.OnLayoutInternal(false);
                    base.Invalidate();
                    this.toolStrip.Invalidate();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(typeof(System.Drawing.Color), "InactiveBorder"), System.Windows.Forms.SRDescription("PropertyGridLineColorDesc")]
        public System.Drawing.Color LineColor
        {
            get
            {
                return this.lineColor;
            }
            set
            {
                if (this.lineColor != value)
                {
                    this.lineColor = value;
                    if (this.lineBrush != null)
                    {
                        this.lineBrush.Dispose();
                        this.lineBrush = null;
                    }
                    this.gridView.Invalidate();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public System.Windows.Forms.Padding Padding
        {
            get
            {
                return base.Padding;
            }
            set
            {
                base.Padding = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("PropertyGridPropertySortDesc"), DefaultValue(3)]
        public System.Windows.Forms.PropertySort PropertySort
        {
            get
            {
                return this.propertySortValue;
            }
            set
            {
                ToolStripButton button;
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.PropertySort));
                }
                if ((value & System.Windows.Forms.PropertySort.Categorized) != System.Windows.Forms.PropertySort.NoSort)
                {
                    button = this.viewSortButtons[0];
                }
                else if ((value & System.Windows.Forms.PropertySort.Alphabetical) != System.Windows.Forms.PropertySort.NoSort)
                {
                    button = this.viewSortButtons[1];
                }
                else
                {
                    button = this.viewSortButtons[2];
                }
                GridItem selectedGridItem = this.SelectedGridItem;
                this.OnViewSortButtonClick(button, EventArgs.Empty);
                this.propertySortValue = value;
                if (selectedGridItem != null)
                {
                    try
                    {
                        this.SelectedGridItem = selectedGridItem;
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public PropertyTabCollection PropertyTabs
        {
            get
            {
                return new PropertyTabCollection(this);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public GridItem SelectedGridItem
        {
            get
            {
                GridItem selectedGridEntry = this.gridView.SelectedGridEntry;
                if (selectedGridEntry == null)
                {
                    return this.peMain;
                }
                return selectedGridEntry;
            }
            set
            {
                this.gridView.SelectedGridEntry = (GridEntry) value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("PropertyGridSelectedObjectDesc"), TypeConverter(typeof(SelectedObjectConverter)), DefaultValue((string) null)]
        public object SelectedObject
        {
            get
            {
                if ((this.currentObjects != null) && (this.currentObjects.Length != 0))
                {
                    return this.currentObjects[0];
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    this.SelectedObjects = new object[0];
                }
                else
                {
                    this.SelectedObjects = new object[] { value };
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public object[] SelectedObjects
        {
            get
            {
                if (this.currentObjects == null)
                {
                    return new object[0];
                }
                return (object[]) this.currentObjects.Clone();
            }
            set
            {
                try
                {
                    this.FreezePainting = true;
                    this.SetFlag(0x80, false);
                    if (this.GetFlag(0x10))
                    {
                        this.SetFlag(0x100, false);
                    }
                    this.gridView.EnsurePendingChangesCommitted();
                    bool flag = false;
                    bool flag2 = false;
                    bool visible = true;
                    if ((value != null) && (value.Length > 0))
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            if (value[i] == null)
                            {
                                object[] args = new object[] { i.ToString(CultureInfo.CurrentCulture), value.Length.ToString(CultureInfo.CurrentCulture) };
                                throw new ArgumentException(System.Windows.Forms.SR.GetString("PropertyGridSetNull", args));
                            }
                            if (value[i] is IUnimplemented)
                            {
                                throw new NotSupportedException(System.Windows.Forms.SR.GetString("PropertyGridRemotedObject", new object[] { value[i].GetType().FullName }));
                            }
                        }
                    }
                    else
                    {
                        visible = false;
                    }
                    if (((this.currentObjects != null) && (value != null)) && (this.currentObjects.Length == value.Length))
                    {
                        flag = true;
                        flag2 = true;
                        for (int j = 0; (j < value.Length) && (flag || flag2); j++)
                        {
                            if (flag && (this.currentObjects[j] != value[j]))
                            {
                                flag = false;
                            }
                            System.Type type = this.GetUnwrappedObject(j).GetType();
                            object propertyOwner = value[j];
                            if (propertyOwner is ICustomTypeDescriptor)
                            {
                                propertyOwner = ((ICustomTypeDescriptor) propertyOwner).GetPropertyOwner(null);
                            }
                            System.Type type2 = propertyOwner.GetType();
                            if (flag2 && ((type != type2) || (type.IsCOMObject && type2.IsCOMObject)))
                            {
                                flag2 = false;
                            }
                        }
                    }
                    if (!flag)
                    {
                        this.EnsureDesignerEventService();
                        visible = visible && this.GetFlag(2);
                        this.SetStatusBox("", "");
                        this.ClearCachedProps();
                        this.peDefault = null;
                        if (value == null)
                        {
                            this.currentObjects = new object[0];
                        }
                        else
                        {
                            this.currentObjects = (object[]) value.Clone();
                        }
                        this.SinkPropertyNotifyEvents();
                        this.SetFlag(1, true);
                        if (this.gridView != null)
                        {
                            try
                            {
                                this.gridView.RemoveSelectedEntryHelpAttributes();
                            }
                            catch (COMException)
                            {
                            }
                        }
                        if (this.peMain != null)
                        {
                            this.peMain.Dispose();
                        }
                        if ((!flag2 && !this.GetFlag(8)) && (this.selectedViewTab < this.viewTabButtons.Length))
                        {
                            System.Type type3 = (this.selectedViewTab == -1) ? null : this.viewTabs[this.selectedViewTab].GetType();
                            ToolStripButton button = null;
                            this.RefreshTabs(PropertyTabScope.Component);
                            this.EnableTabs();
                            if (type3 != null)
                            {
                                for (int k = 0; k < this.viewTabs.Length; k++)
                                {
                                    if ((this.viewTabs[k].GetType() == type3) && this.viewTabButtons[k].Visible)
                                    {
                                        button = this.viewTabButtons[k];
                                        break;
                                    }
                                }
                            }
                            this.SelectViewTabButtonDefault(button);
                        }
                        if ((visible && (this.viewTabs != null)) && ((this.viewTabs.Length > 1) && (this.viewTabs[1] is EventsTab)))
                        {
                            visible = this.viewTabButtons[1].Visible;
                            Attribute[] array = new Attribute[this.BrowsableAttributes.Count];
                            this.BrowsableAttributes.CopyTo(array, 0);
                            Hashtable hashtable = null;
                            if (this.currentObjects.Length > 10)
                            {
                                hashtable = new Hashtable();
                            }
                            for (int m = 0; (m < this.currentObjects.Length) && visible; m++)
                            {
                                object component = this.currentObjects[m];
                                if (component is ICustomTypeDescriptor)
                                {
                                    component = ((ICustomTypeDescriptor) component).GetPropertyOwner(null);
                                }
                                System.Type key = component.GetType();
                                if ((hashtable == null) || !hashtable.Contains(key))
                                {
                                    visible = visible && ((component is IComponent) && (((IComponent) component).Site != null));
                                    PropertyDescriptorCollection properties = ((EventsTab) this.viewTabs[1]).GetProperties(component, array);
                                    visible = (visible && (properties != null)) && (properties.Count > 0);
                                    if (visible && (hashtable != null))
                                    {
                                        hashtable[key] = key;
                                    }
                                }
                            }
                        }
                        this.ShowEventsButton(visible && (this.currentObjects.Length > 0));
                        this.DisplayHotCommands();
                        if (this.currentObjects.Length == 1)
                        {
                            this.EnablePropPageButton(this.currentObjects[0]);
                        }
                        else
                        {
                            this.EnablePropPageButton(null);
                        }
                        this.OnSelectedObjectsChanged(EventArgs.Empty);
                    }
                    if (!this.GetFlag(8))
                    {
                        if ((this.currentObjects.Length > 0) && this.GetFlag(0x20))
                        {
                            object activeDesigner = this.ActiveDesigner;
                            if (((activeDesigner != null) && (this.designerSelections != null)) && this.designerSelections.ContainsKey(activeDesigner.GetHashCode()))
                            {
                                int index = (int) this.designerSelections[activeDesigner.GetHashCode()];
                                if ((index < this.viewTabs.Length) && ((index == 0) || this.viewTabButtons[index].Visible))
                                {
                                    this.SelectViewTabButton(this.viewTabButtons[index], true);
                                }
                            }
                            else
                            {
                                this.Refresh(false);
                            }
                            this.SetFlag(0x20, false);
                        }
                        else
                        {
                            this.Refresh(true);
                        }
                        if (this.currentObjects.Length > 0)
                        {
                            this.SaveTabSelection();
                        }
                    }
                }
                finally
                {
                    this.FreezePainting = false;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public PropertyTab SelectedTab
        {
            get
            {
                return this.viewTabs[this.selectedViewTab];
            }
        }

        protected internal override bool ShowFocusCues
        {
            get
            {
                return true;
            }
        }

        public override ISite Site
        {
            get
            {
                return base.Site;
            }
            set
            {
                base.SuspendAllLayout(this);
                base.Site = value;
                this.gridView.ServiceProvider = value;
                if (value == null)
                {
                    this.ActiveDesigner = null;
                }
                else
                {
                    this.ActiveDesigner = (IDesignerHost) value.GetService(typeof(IDesignerHost));
                }
                base.ResumeAllLayout(this, true);
            }
        }

        internal override bool SupportsUseCompatibleTextRendering
        {
            get
            {
                return true;
            }
        }

        bool IComPropertyBrowser.InPropertySet
        {
            get
            {
                return this.GetPropertyGridView().GetInPropertySet();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        [System.Windows.Forms.SRDescription("PropertyGridToolbarVisibleDesc"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(true)]
        public virtual bool ToolbarVisible
        {
            get
            {
                return this.toolbarVisible;
            }
            set
            {
                this.toolbarVisible = value;
                this.toolStrip.Visible = value;
                this.OnLayoutInternal(false);
                if (value)
                {
                    this.SetupToolbar(this.viewTabsDirty);
                }
                base.Invalidate();
                this.toolStrip.Invalidate();
            }
        }

        protected System.Windows.Forms.ToolStripRenderer ToolStripRenderer
        {
            get
            {
                if (this.toolStrip != null)
                {
                    return this.toolStrip.Renderer;
                }
                return null;
            }
            set
            {
                if (this.toolStrip != null)
                {
                    this.toolStrip.Renderer = value;
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("UseCompatibleTextRenderingDescr"), DefaultValue(false)]
        public bool UseCompatibleTextRendering
        {
            get
            {
                return base.UseCompatibleTextRenderingInt;
            }
            set
            {
                base.UseCompatibleTextRenderingInt = value;
                this.doccomment.UpdateTextRenderingEngine();
                this.gridView.Invalidate();
            }
        }

        [System.Windows.Forms.SRDescription("PropertyGridViewBackColorDesc"), DefaultValue(typeof(System.Drawing.Color), "Window"), System.Windows.Forms.SRCategory("CatAppearance")]
        public System.Drawing.Color ViewBackColor
        {
            get
            {
                return this.gridView.BackColor;
            }
            set
            {
                this.gridView.BackColor = value;
                this.gridView.Invalidate();
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("PropertyGridViewForeColorDesc"), DefaultValue(typeof(System.Drawing.Color), "WindowText")]
        public System.Drawing.Color ViewForeColor
        {
            get
            {
                return this.gridView.ForeColor;
            }
            set
            {
                this.gridView.ForeColor = value;
                this.gridView.Invalidate();
            }
        }

        private interface IUnimplemented
        {
        }

        internal static class MeasureTextHelper
        {
            public static TextFormatFlags GetTextRendererFlags()
            {
                return (TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping);
            }

            public static SizeF MeasureText(PropertyGrid owner, Graphics g, string text, Font font)
            {
                return MeasureTextSimple(owner, g, text, font, new SizeF(0f, 0f));
            }

            public static SizeF MeasureText(PropertyGrid owner, Graphics g, string text, Font font, SizeF size)
            {
                if (owner.UseCompatibleTextRendering)
                {
                    return g.MeasureString(text, font, size);
                }
                TextFormatFlags flags = ((GetTextRendererFlags() | TextFormatFlags.LeftAndRightPadding) | TextFormatFlags.WordBreak) | TextFormatFlags.NoFullWidthCharacterBreak;
                return (SizeF) TextRenderer.MeasureText(g, text, font, Size.Ceiling(size), flags);
            }

            public static SizeF MeasureText(PropertyGrid owner, Graphics g, string text, Font font, int width)
            {
                return MeasureText(owner, g, text, font, new SizeF((float) width, 999999f));
            }

            public static SizeF MeasureTextSimple(PropertyGrid owner, Graphics g, string text, Font font, SizeF size)
            {
                if (owner.UseCompatibleTextRendering)
                {
                    return g.MeasureString(text, font, size);
                }
                return (SizeF) TextRenderer.MeasureText(g, text, font, Size.Ceiling(size), GetTextRendererFlags());
            }
        }

        private class PropertyGridServiceProvider : IServiceProvider
        {
            private PropertyGrid owner;

            public PropertyGridServiceProvider(PropertyGrid owner)
            {
                this.owner = owner;
            }

            public object GetService(System.Type serviceType)
            {
                object service = null;
                if (this.owner.ActiveDesigner != null)
                {
                    service = this.owner.ActiveDesigner.GetService(serviceType);
                }
                if (service == null)
                {
                    service = this.owner.gridView.GetService(serviceType);
                }
                if ((service == null) && (this.owner.Site != null))
                {
                    service = this.owner.Site.GetService(serviceType);
                }
                return service;
            }
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public class PropertyTabCollection : ICollection, IEnumerable
        {
            internal static PropertyGrid.PropertyTabCollection Empty = new PropertyGrid.PropertyTabCollection(null);
            private PropertyGrid owner;

            internal PropertyTabCollection(PropertyGrid owner)
            {
                this.owner = owner;
            }

            public void AddTabType(System.Type propertyTabType)
            {
                if (this.owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("PropertyGridPropertyTabCollectionReadOnly"));
                }
                this.owner.AddTab(propertyTabType, PropertyTabScope.Global);
            }

            public void AddTabType(System.Type propertyTabType, PropertyTabScope tabScope)
            {
                if (this.owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("PropertyGridPropertyTabCollectionReadOnly"));
                }
                this.owner.AddTab(propertyTabType, tabScope);
            }

            public void Clear(PropertyTabScope tabScope)
            {
                if (this.owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("PropertyGridPropertyTabCollectionReadOnly"));
                }
                this.owner.ClearTabs(tabScope);
            }

            public IEnumerator GetEnumerator()
            {
                if (this.owner == null)
                {
                    return new PropertyTab[0].GetEnumerator();
                }
                return this.owner.viewTabs.GetEnumerator();
            }

            public void RemoveTabType(System.Type propertyTabType)
            {
                if (this.owner == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("PropertyGridPropertyTabCollectionReadOnly"));
                }
                this.owner.RemoveTab(propertyTabType);
            }

            void ICollection.CopyTo(Array dest, int index)
            {
                if ((this.owner != null) && (this.owner.viewTabs.Length > 0))
                {
                    Array.Copy(this.owner.viewTabs, 0, dest, index, this.owner.viewTabs.Length);
                }
            }

            public int Count
            {
                get
                {
                    if (this.owner == null)
                    {
                        return 0;
                    }
                    return this.owner.viewTabs.Length;
                }
            }

            public PropertyTab this[int index]
            {
                get
                {
                    if (this.owner == null)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("PropertyGridPropertyTabCollectionReadOnly"));
                    }
                    return this.owner.viewTabs[index];
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this;
                }
            }
        }

        internal class SelectedObjectConverter : ReferenceConverter
        {
            public SelectedObjectConverter() : base(typeof(IComponent))
            {
            }
        }

        internal abstract class SnappableControl : Control
        {
            protected PropertyGrid ownerGrid;
            internal bool userSized;

            public SnappableControl(PropertyGrid ownerGrid)
            {
                this.ownerGrid = ownerGrid;
                base.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            }

            public abstract int GetOptimalHeight(int width);
            protected override void OnControlAdded(ControlEventArgs ce)
            {
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Rectangle clientRectangle = base.ClientRectangle;
                clientRectangle.Width--;
                clientRectangle.Height--;
                e.Graphics.DrawRectangle(SystemPens.ControlDark, clientRectangle);
            }

            public abstract int SnapHeightRequest(int request);

            public override System.Windows.Forms.Cursor Cursor
            {
                get
                {
                    return Cursors.Default;
                }
                set
                {
                    base.Cursor = value;
                }
            }
        }
    }
}

