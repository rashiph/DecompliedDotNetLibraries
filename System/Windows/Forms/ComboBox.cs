namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms.Internal;
    using System.Windows.Forms.Layout;

    [ClassInterface(ClassInterfaceType.AutoDispatch), DefaultProperty("Items"), DefaultBindingProperty("Text"), System.Windows.Forms.SRDescription("DescriptionComboBox"), DefaultEvent("SelectedIndexChanged"), ComVisible(true), Designer("System.Windows.Forms.Design.ComboBoxDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class ComboBox : ListControl
    {
        private bool allowCommit = true;
        private AutoCompleteStringCollection autoCompleteCustomSource;
        private bool autoCompleteDroppedDown;
        private System.Windows.Forms.AutoCompleteMode autoCompleteMode;
        private System.Windows.Forms.AutoCompleteSource autoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
        private const int AutoCompleteTimeout = 0x989680;
        private long autoCompleteTimeStamp;
        private bool canFireLostFocus;
        private ComboBoxChildNativeWindow childEdit;
        private ComboBoxChildNativeWindow childListBox;
        private string currentText = "";
        private const int DefaultDropDownHeight = 0x6a;
        private const int DefaultSimpleStyleHeight = 150;
        private bool dropDown;
        private IntPtr dropDownHandle;
        private static readonly object EVENT_DRAWITEM = new object();
        private static readonly object EVENT_DROPDOWN = new object();
        private static readonly object EVENT_DROPDOWNCLOSED = new object();
        private static readonly object EVENT_DROPDOWNSTYLE = new object();
        private static readonly object EVENT_MEASUREITEM = new object();
        private static readonly object EVENT_SELECTEDINDEXCHANGED = new object();
        private static readonly object EVENT_SELECTEDITEMCHANGED = new object();
        private static readonly object EVENT_SELECTIONCHANGECOMMITTED = new object();
        private static readonly object EVENT_TEXTUPDATE = new object();
        private AutoCompleteDropDownFinder finder = new AutoCompleteDropDownFinder();
        private bool fireLostFocus = true;
        private bool fireSetFocus = true;
        private System.Windows.Forms.FlatStyle flatStyle = System.Windows.Forms.FlatStyle.Standard;
        private bool fromHandleCreate;
        private bool integralHeight = true;
        private ObjectCollection itemsCollection;
        private string lastTextChangedValue;
        private short maxDropDownItems = 8;
        private bool mouseEvents;
        private bool mouseInEdit;
        private bool mouseOver;
        private bool mousePressed;
        private short prefHeightCache = -1;
        private static readonly int PropDrawMode = PropertyStore.CreateKey();
        private static readonly int PropDropDownHeight = PropertyStore.CreateKey();
        private static readonly int PropDropDownWidth = PropertyStore.CreateKey();
        private static readonly int PropFlatComboAdapter = PropertyStore.CreateKey();
        private static readonly int PropItemHeight = PropertyStore.CreateKey();
        private static readonly int PropMatchingText = PropertyStore.CreateKey();
        private static readonly int PropMaxLength = PropertyStore.CreateKey();
        private static readonly int PropStyle = PropertyStore.CreateKey();
        private int requestedHeight;
        private int selectedIndex = -1;
        private bool selectedValueChangedFired;
        private bool sorted;
        private StringSource stringSource;
        private bool suppressNextWindosPos;
        private int updateCount;

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
        public event EventHandler DoubleClick
        {
            add
            {
                base.DoubleClick += value;
            }
            remove
            {
                base.DoubleClick -= value;
            }
        }

        [System.Windows.Forms.SRDescription("drawItemEventDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event DrawItemEventHandler DrawItem
        {
            add
            {
                base.Events.AddHandler(EVENT_DRAWITEM, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DRAWITEM, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ComboBoxOnDropDownDescr")]
        public event EventHandler DropDown
        {
            add
            {
                base.Events.AddHandler(EVENT_DROPDOWN, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DROPDOWN, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ComboBoxOnDropDownClosedDescr")]
        public event EventHandler DropDownClosed
        {
            add
            {
                base.Events.AddHandler(EVENT_DROPDOWNCLOSED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DROPDOWNCLOSED, value);
            }
        }

        [System.Windows.Forms.SRDescription("ComboBoxDropDownStyleChangedDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler DropDownStyleChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_DROPDOWNSTYLE, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DROPDOWNSTYLE, value);
            }
        }

        [System.Windows.Forms.SRDescription("measureItemEventDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event MeasureItemEventHandler MeasureItem
        {
            add
            {
                base.Events.AddHandler(EVENT_MEASUREITEM, value);
                this.UpdateItemHeight();
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MEASUREITEM, value);
                this.UpdateItemHeight();
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event PaintEventHandler Paint
        {
            add
            {
                base.Paint += value;
            }
            remove
            {
                base.Paint -= value;
            }
        }

        [System.Windows.Forms.SRDescription("selectedIndexChangedEventDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler SelectedIndexChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_SELECTEDINDEXCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_SELECTEDINDEXCHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("selectionChangeCommittedEventDescr")]
        public event EventHandler SelectionChangeCommitted
        {
            add
            {
                base.Events.AddHandler(EVENT_SELECTIONCHANGECOMMITTED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_SELECTIONCHANGECOMMITTED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ComboBoxOnTextUpdateDescr")]
        public event EventHandler TextUpdate
        {
            add
            {
                base.Events.AddHandler(EVENT_TEXTUPDATE, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_TEXTUPDATE, value);
            }
        }

        public ComboBox()
        {
            base.SetStyle(ControlStyles.UseTextForAccessibility | ControlStyles.StandardClick | ControlStyles.UserPaint, false);
            this.requestedHeight = 150;
            base.SetState2(0x800, true);
        }

        [Obsolete("This method has been deprecated.  There is no replacement.  http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual void AddItemsCore(object[] value)
        {
            if (((value == null) ? 0 : value.Length) != 0)
            {
                this.BeginUpdate();
                try
                {
                    this.Items.AddRangeInternal(value);
                }
                finally
                {
                    this.EndUpdate();
                }
            }
        }

        internal override Rectangle ApplyBoundsConstraints(int suggestedX, int suggestedY, int proposedWidth, int proposedHeight)
        {
            if ((this.DropDownStyle == ComboBoxStyle.DropDown) || (this.DropDownStyle == ComboBoxStyle.DropDownList))
            {
                proposedHeight = this.PreferredHeight;
            }
            return base.ApplyBoundsConstraints(suggestedX, suggestedY, proposedWidth, proposedHeight);
        }

        public void BeginUpdate()
        {
            this.updateCount++;
            base.BeginUpdateInternal();
        }

        private void CheckNoDataSource()
        {
            if (this.DataSource != null)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataSourceLocksItems"));
            }
        }

        private void ChildWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x30:
                    this.DefChildWndProc(ref m);
                    if ((this.childEdit != null) && (m.HWnd == this.childEdit.Handle))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.childEdit.Handle), 0xd3, 3, 0);
                    }
                    return;

                case 0x51:
                    this.DefChildWndProc(ref m);
                    return;

                case 7:
                    if (!base.DesignMode)
                    {
                        ImeContext.SetImeStatus(base.CachedImeMode, m.HWnd);
                    }
                    if (!base.HostedInWin32DialogManager)
                    {
                        IContainerControl containerControlInternal = base.GetContainerControlInternal();
                        if (containerControlInternal != null)
                        {
                            ContainerControl control2 = containerControlInternal as ContainerControl;
                            if ((control2 != null) && !control2.ActivateControlInternal(this, false))
                            {
                                return;
                            }
                        }
                    }
                    this.DefChildWndProc(ref m);
                    if (this.fireSetFocus)
                    {
                        this.OnGotFocus(EventArgs.Empty);
                    }
                    if (this.FlatStyle == System.Windows.Forms.FlatStyle.Popup)
                    {
                        base.Invalidate();
                    }
                    return;

                case 8:
                    if (!base.DesignMode)
                    {
                        base.OnImeContextStatusChanged(m.HWnd);
                    }
                    this.DefChildWndProc(ref m);
                    if (this.fireLostFocus)
                    {
                        this.OnLostFocus(EventArgs.Empty);
                    }
                    if (this.FlatStyle == System.Windows.Forms.FlatStyle.Popup)
                    {
                        base.Invalidate();
                    }
                    return;

                case 0x20:
                    if (((this.Cursor != this.DefaultCursor) && (this.childEdit != null)) && ((m.HWnd == this.childEdit.Handle) && (System.Windows.Forms.NativeMethods.Util.LOWORD(m.LParam) == 1)))
                    {
                        Cursor.CurrentInternal = this.Cursor;
                        return;
                    }
                    this.DefChildWndProc(ref m);
                    return;

                case 0x100:
                case 260:
                    if (this.SystemAutoCompleteEnabled && !ACNativeWindow.AutoCompleteActive)
                    {
                        this.finder.FindDropDowns(false);
                    }
                    if (this.AutoCompleteMode != System.Windows.Forms.AutoCompleteMode.None)
                    {
                        char wParam = (char) ((ushort) ((long) m.WParam));
                        if (wParam == '\x001b')
                        {
                            this.DroppedDown = false;
                        }
                        else if ((wParam == '\r') && this.DroppedDown)
                        {
                            this.UpdateText();
                            this.OnSelectionChangeCommittedInternal(EventArgs.Empty);
                            this.DroppedDown = false;
                        }
                    }
                    if ((this.DropDownStyle == ComboBoxStyle.Simple) && (m.HWnd == this.childListBox.Handle))
                    {
                        this.DefChildWndProc(ref m);
                        return;
                    }
                    if ((base.PreProcessControlMessage(ref m) != PreProcessControlState.MessageProcessed) && !this.ProcessKeyMessage(ref m))
                    {
                        this.DefChildWndProc(ref m);
                    }
                    return;

                case 0x101:
                case 0x105:
                    if ((this.DropDownStyle != ComboBoxStyle.Simple) || !(m.HWnd == this.childListBox.Handle))
                    {
                        if (base.PreProcessControlMessage(ref m) != PreProcessControlState.MessageProcessed)
                        {
                            if (this.ProcessKeyMessage(ref m))
                            {
                                return;
                            }
                            this.DefChildWndProc(ref m);
                        }
                        break;
                    }
                    this.DefChildWndProc(ref m);
                    break;

                case 0x102:
                    if ((this.DropDownStyle != ComboBoxStyle.Simple) || !(m.HWnd == this.childListBox.Handle))
                    {
                        if ((base.PreProcessControlMessage(ref m) != PreProcessControlState.MessageProcessed) && !this.ProcessKeyMessage(ref m))
                        {
                            this.DefChildWndProc(ref m);
                        }
                        return;
                    }
                    this.DefChildWndProc(ref m);
                    return;

                case 0x106:
                    if ((this.DropDownStyle != ComboBoxStyle.Simple) || !(m.HWnd == this.childListBox.Handle))
                    {
                        if ((base.PreProcessControlMessage(ref m) != PreProcessControlState.MessageProcessed) && !this.ProcessKeyEventArgs(ref m))
                        {
                            this.DefChildWndProc(ref m);
                        }
                        return;
                    }
                    this.DefChildWndProc(ref m);
                    return;

                case 0x7b:
                    if ((this.ContextMenu != null) || (this.ContextMenuStrip != null))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x7b, m.WParam, m.LParam);
                        return;
                    }
                    this.DefChildWndProc(ref m);
                    return;

                case 0x200:
                {
                    Point point9 = this.EditToComboboxMapping(m);
                    this.DefChildWndProc(ref m);
                    this.OnMouseEnterInternal(EventArgs.Empty);
                    this.OnMouseMove(new MouseEventArgs(Control.MouseButtons, 0, point9.X, point9.Y, 0));
                    return;
                }
                case 0x201:
                {
                    this.mousePressed = true;
                    this.mouseEvents = true;
                    base.CaptureInternal = true;
                    this.DefChildWndProc(ref m);
                    Point point4 = this.EditToComboboxMapping(m);
                    this.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, point4.X, point4.Y, 0));
                    return;
                }
                case 0x202:
                {
                    System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                    System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(this, base.Handle), ref rect);
                    Rectangle rectangle = new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
                    int x = System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam);
                    int y = System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam);
                    Point p = new Point(x, y);
                    p = base.PointToScreen(p);
                    if (this.mouseEvents && !base.ValidationCancelled)
                    {
                        this.mouseEvents = false;
                        if (this.mousePressed)
                        {
                            if (!rectangle.Contains(p))
                            {
                                this.mousePressed = false;
                                this.mouseInEdit = false;
                                this.OnMouseLeave(EventArgs.Empty);
                            }
                            else
                            {
                                this.mousePressed = false;
                                this.OnClick(new MouseEventArgs(MouseButtons.Left, 1, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                                this.OnMouseClick(new MouseEventArgs(MouseButtons.Left, 1, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                            }
                        }
                    }
                    this.DefChildWndProc(ref m);
                    base.CaptureInternal = false;
                    p = this.EditToComboboxMapping(m);
                    this.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, p.X, p.Y, 0));
                    return;
                }
                case 0x203:
                {
                    this.mousePressed = true;
                    this.mouseEvents = true;
                    base.CaptureInternal = true;
                    this.DefChildWndProc(ref m);
                    Point point = this.EditToComboboxMapping(m);
                    this.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, point.X, point.Y, 0));
                    return;
                }
                case 0x204:
                {
                    this.mousePressed = true;
                    this.mouseEvents = true;
                    if ((this.ContextMenu != null) || (this.ContextMenuStrip != null))
                    {
                        base.CaptureInternal = true;
                    }
                    this.DefChildWndProc(ref m);
                    Point point7 = this.EditToComboboxMapping(m);
                    this.OnMouseDown(new MouseEventArgs(MouseButtons.Right, 1, point7.X, point7.Y, 0));
                    return;
                }
                case 0x205:
                {
                    this.mousePressed = false;
                    this.mouseEvents = false;
                    if (this.ContextMenu != null)
                    {
                        base.CaptureInternal = false;
                    }
                    this.DefChildWndProc(ref m);
                    Point point8 = this.EditToComboboxMapping(m);
                    this.OnMouseUp(new MouseEventArgs(MouseButtons.Right, 1, point8.X, point8.Y, 0));
                    return;
                }
                case 0x206:
                {
                    this.mousePressed = true;
                    this.mouseEvents = true;
                    base.CaptureInternal = true;
                    this.DefChildWndProc(ref m);
                    Point point3 = this.EditToComboboxMapping(m);
                    this.OnMouseDown(new MouseEventArgs(MouseButtons.Right, 1, point3.X, point3.Y, 0));
                    return;
                }
                case 0x207:
                {
                    this.mousePressed = true;
                    this.mouseEvents = true;
                    base.CaptureInternal = true;
                    this.DefChildWndProc(ref m);
                    Point point6 = this.EditToComboboxMapping(m);
                    this.OnMouseDown(new MouseEventArgs(MouseButtons.Middle, 1, point6.X, point6.Y, 0));
                    return;
                }
                case 520:
                    this.mousePressed = false;
                    this.mouseEvents = false;
                    base.CaptureInternal = false;
                    this.DefChildWndProc(ref m);
                    this.OnMouseUp(new MouseEventArgs(MouseButtons.Middle, 1, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                    return;

                case 0x209:
                {
                    this.mousePressed = true;
                    this.mouseEvents = true;
                    base.CaptureInternal = true;
                    this.DefChildWndProc(ref m);
                    Point point2 = this.EditToComboboxMapping(m);
                    this.OnMouseDown(new MouseEventArgs(MouseButtons.Middle, 1, point2.X, point2.Y, 0));
                    return;
                }
                case 0x2a3:
                    this.DefChildWndProc(ref m);
                    this.OnMouseLeaveInternal(EventArgs.Empty);
                    return;

                default:
                    this.DefChildWndProc(ref m);
                    return;
            }
            if (this.SystemAutoCompleteEnabled && !ACNativeWindow.AutoCompleteActive)
            {
                this.finder.FindDropDowns();
            }
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new ComboBoxAccessibleObject(this);
        }

        internal virtual FlatComboAdapter CreateFlatComboAdapterInstance()
        {
            return new FlatComboAdapter(this, false);
        }

        protected override void CreateHandle()
        {
            using (new LayoutTransaction(this.ParentInternal, this, PropertyNames.Bounds))
            {
                base.CreateHandle();
            }
        }

        private void DefChildWndProc(ref Message m)
        {
            if (this.childEdit != null)
            {
                NativeWindow window = (m.HWnd == this.childEdit.Handle) ? this.childEdit : this.childListBox;
                if (window != null)
                {
                    window.DefWndProc(ref m);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.autoCompleteCustomSource != null)
                {
                    this.autoCompleteCustomSource.CollectionChanged -= new CollectionChangeEventHandler(this.OnAutoCompleteCustomSourceChanged);
                }
                if (this.stringSource != null)
                {
                    this.stringSource.ReleaseAutoComplete();
                    this.stringSource = null;
                }
            }
            base.Dispose(disposing);
        }

        internal Point EditToComboboxMapping(Message m)
        {
            if (this.childEdit == null)
            {
                return new Point(0, 0);
            }
            System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
            System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(this, base.Handle), ref rect);
            System.Windows.Forms.NativeMethods.RECT rect2 = new System.Windows.Forms.NativeMethods.RECT();
            System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(this, this.childEdit.Handle), ref rect2);
            int x = System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam) + (rect2.left - rect.left);
            return new Point(x, System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam) + (rect2.top - rect.top));
        }

        public void EndUpdate()
        {
            this.updateCount--;
            if ((this.updateCount == 0) && (this.AutoCompleteSource == System.Windows.Forms.AutoCompleteSource.ListItems))
            {
                this.SetAutoComplete(false, false);
            }
            if (base.EndUpdateInternal())
            {
                if ((this.childEdit != null) && (this.childEdit.Handle != IntPtr.Zero))
                {
                    System.Windows.Forms.SafeNativeMethods.InvalidateRect(new HandleRef(this, this.childEdit.Handle), (System.Windows.Forms.NativeMethods.COMRECT) null, false);
                }
                if ((this.childListBox != null) && (this.childListBox.Handle != IntPtr.Zero))
                {
                    System.Windows.Forms.SafeNativeMethods.InvalidateRect(new HandleRef(this, this.childListBox.Handle), (System.Windows.Forms.NativeMethods.COMRECT) null, false);
                }
            }
        }

        public int FindString(string s)
        {
            return this.FindString(s, -1);
        }

        public int FindString(string s, int startIndex)
        {
            if (s == null)
            {
                return -1;
            }
            if ((this.itemsCollection == null) || (this.itemsCollection.Count == 0))
            {
                return -1;
            }
            if ((startIndex < -1) || (startIndex >= this.itemsCollection.Count))
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            return base.FindStringInternal(s, this.Items, startIndex, false);
        }

        public int FindStringExact(string s)
        {
            return this.FindStringExact(s, -1, true);
        }

        public int FindStringExact(string s, int startIndex)
        {
            return this.FindStringExact(s, startIndex, true);
        }

        internal int FindStringExact(string s, int startIndex, bool ignorecase)
        {
            if (s == null)
            {
                return -1;
            }
            if ((this.itemsCollection == null) || (this.itemsCollection.Count == 0))
            {
                return -1;
            }
            if ((startIndex < -1) || (startIndex >= this.itemsCollection.Count))
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            return base.FindStringInternal(s, this.Items, startIndex, true, ignorecase);
        }

        private int FindStringIgnoreCase(string value)
        {
            int num = this.FindStringExact(value, -1, false);
            if (num == -1)
            {
                num = this.FindStringExact(value, -1, true);
            }
            return num;
        }

        private int GetComboHeight()
        {
            Size empty = Size.Empty;
            using (WindowsFont font = WindowsFont.FromFont(this.Font))
            {
                empty = WindowsGraphicsCacheManager.MeasurementGraphics.GetTextExtent("0", font);
            }
            int itemHeight = empty.Height + SystemInformation.Border3DSize.Height;
            if (this.DrawMode != System.Windows.Forms.DrawMode.Normal)
            {
                itemHeight = this.ItemHeight;
            }
            Size fixedFrameBorderSize = SystemInformation.FixedFrameBorderSize;
            return ((2 * fixedFrameBorderSize.Height) + itemHeight);
        }

        public int GetItemHeight(int index)
        {
            if (this.DrawMode != System.Windows.Forms.DrawMode.OwnerDrawVariable)
            {
                return this.ItemHeight;
            }
            if (((index < 0) || (this.itemsCollection == null)) || (index >= this.itemsCollection.Count))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            if (!base.IsHandleCreated)
            {
                return this.ItemHeight;
            }
            int num = (int) ((long) base.SendMessage(340, index, 0));
            if (num == -1)
            {
                throw new Win32Exception();
            }
            return num;
        }

        private string[] GetStringsForAutoComplete(IList collection)
        {
            if (collection is AutoCompleteStringCollection)
            {
                string[] strArray = new string[this.AutoCompleteCustomSource.Count];
                for (int j = 0; j < this.AutoCompleteCustomSource.Count; j++)
                {
                    strArray[j] = this.AutoCompleteCustomSource[j];
                }
                return strArray;
            }
            if (!(collection is ObjectCollection))
            {
                return new string[0];
            }
            string[] strArray2 = new string[this.itemsCollection.Count];
            for (int i = 0; i < this.itemsCollection.Count; i++)
            {
                strArray2[i] = base.GetItemText(this.itemsCollection[i]);
            }
            return strArray2;
        }

        internal override IntPtr InitializeDCForWmCtlColor(IntPtr dc, int msg)
        {
            if ((msg == 0x138) && !this.ShouldSerializeBackColor())
            {
                return IntPtr.Zero;
            }
            if ((msg == 0x134) && base.GetStyle(ControlStyles.UserPaint))
            {
                System.Windows.Forms.SafeNativeMethods.SetTextColor(new HandleRef(null, dc), ColorTranslator.ToWin32(this.ForeColor));
                System.Windows.Forms.SafeNativeMethods.SetBkColor(new HandleRef(null, dc), ColorTranslator.ToWin32(this.BackColor));
                return base.BackColorBrush;
            }
            return base.InitializeDCForWmCtlColor(dc, msg);
        }

        private bool InterceptAutoCompleteKeystroke(Message m)
        {
            if (m.Msg == 0x100)
            {
                if (((int) ((long) m.WParam)) == 0x2e)
                {
                    this.MatchingText = "";
                    this.autoCompleteTimeStamp = DateTime.Now.Ticks;
                    if (this.Items.Count > 0)
                    {
                        this.SelectedIndex = 0;
                    }
                    return false;
                }
            }
            else if (m.Msg == 0x102)
            {
                string str;
                char wParam = (char) ((ushort) ((long) m.WParam));
                switch (wParam)
                {
                    case '\b':
                        if (((DateTime.Now.Ticks - this.autoCompleteTimeStamp) > 0x989680L) || (this.MatchingText.Length <= 1))
                        {
                            this.MatchingText = "";
                            if (this.Items.Count > 0)
                            {
                                this.SelectedIndex = 0;
                            }
                        }
                        else
                        {
                            this.MatchingText = this.MatchingText.Remove(this.MatchingText.Length - 1);
                            this.SelectedIndex = this.FindString(this.MatchingText);
                        }
                        this.autoCompleteTimeStamp = DateTime.Now.Ticks;
                        return false;

                    case '\x001b':
                        this.MatchingText = "";
                        break;
                }
                if (((wParam != '\x001b') && (wParam != '\r')) && (!this.DroppedDown && (this.AutoCompleteMode != System.Windows.Forms.AutoCompleteMode.Append)))
                {
                    this.DroppedDown = true;
                }
                if ((DateTime.Now.Ticks - this.autoCompleteTimeStamp) > 0x989680L)
                {
                    str = new string(wParam, 1);
                    if (this.FindString(str) != -1)
                    {
                        this.MatchingText = str;
                    }
                    this.autoCompleteTimeStamp = DateTime.Now.Ticks;
                    return false;
                }
                str = this.MatchingText + wParam;
                int num = this.FindString(str);
                if (num != -1)
                {
                    this.MatchingText = str;
                    if (num != this.SelectedIndex)
                    {
                        this.SelectedIndex = num;
                    }
                }
                this.autoCompleteTimeStamp = DateTime.Now.Ticks;
                return true;
            }
            return false;
        }

        private void InvalidateEverything()
        {
            System.Windows.Forms.SafeNativeMethods.RedrawWindow(new HandleRef(this, base.Handle), (System.Windows.Forms.NativeMethods.COMRECT) null, System.Windows.Forms.NativeMethods.NullHandleRef, 0x485);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch ((keyData & (Keys.Alt | Keys.KeyCode)))
            {
                case Keys.Enter:
                case Keys.Escape:
                    if (this.DroppedDown || this.autoCompleteDroppedDown)
                    {
                        return true;
                    }
                    if (this.SystemAutoCompleteEnabled && ACNativeWindow.AutoCompleteActive)
                    {
                        this.autoCompleteDroppedDown = true;
                        return true;
                    }
                    break;
            }
            return base.IsInputKey(keyData);
        }

        private int NativeAdd(object item)
        {
            int num = (int) ((long) base.SendMessage(0x143, 0, base.GetItemText(item)));
            if (num < 0)
            {
                throw new OutOfMemoryException(System.Windows.Forms.SR.GetString("ComboBoxItemOverflow"));
            }
            return num;
        }

        private void NativeClear()
        {
            string windowText = null;
            if (this.DropDownStyle != ComboBoxStyle.DropDownList)
            {
                windowText = this.WindowText;
            }
            base.SendMessage(0x14b, 0, 0);
            if (windowText != null)
            {
                this.WindowText = windowText;
            }
        }

        private string NativeGetItemText(int index)
        {
            int num = (int) ((long) base.SendMessage(0x149, index, 0));
            StringBuilder lParam = new StringBuilder(num + 1);
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x148, index, lParam);
            return lParam.ToString();
        }

        private int NativeInsert(int index, object item)
        {
            int num = (int) ((long) base.SendMessage(330, index, base.GetItemText(item)));
            if (num < 0)
            {
                throw new OutOfMemoryException(System.Windows.Forms.SR.GetString("ComboBoxItemOverflow"));
            }
            return num;
        }

        private void NativeRemoveAt(int index)
        {
            if ((this.DropDownStyle == ComboBoxStyle.DropDownList) && (this.SelectedIndex == index))
            {
                base.Invalidate();
            }
            base.SendMessage(0x144, index, 0);
        }

        private void NotifyAutoComplete()
        {
            this.NotifyAutoComplete(true);
        }

        private void NotifyAutoComplete(bool setSelectedIndex)
        {
            string text = this.Text;
            bool flag = text != this.lastTextChangedValue;
            bool flag2 = false;
            if (setSelectedIndex)
            {
                int num = this.FindStringIgnoreCase(text);
                if ((num != -1) && (num != this.SelectedIndex))
                {
                    this.SelectedIndex = num;
                    this.SelectionStart = 0;
                    this.SelectionLength = text.Length;
                    flag2 = true;
                }
            }
            if (flag && !flag2)
            {
                this.OnTextChanged(EventArgs.Empty);
            }
            this.lastTextChangedValue = text;
        }

        private void OnAutoCompleteCustomSourceChanged(object sender, CollectionChangeEventArgs e)
        {
            if (this.AutoCompleteSource == System.Windows.Forms.AutoCompleteSource.CustomSource)
            {
                if (this.AutoCompleteCustomSource.Count == 0)
                {
                    this.SetAutoComplete(true, true);
                }
                else
                {
                    this.SetAutoComplete(true, false);
                }
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            this.UpdateControl(false);
        }

        protected override void OnDataSourceChanged(EventArgs e)
        {
            if ((this.Sorted && (this.DataSource != null)) && base.Created)
            {
                this.DataSource = null;
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ComboBoxDataSourceWithSort"));
            }
            if (this.DataSource == null)
            {
                this.BeginUpdate();
                this.SelectedIndex = -1;
                this.Items.ClearInternal();
                this.EndUpdate();
            }
            if (!this.Sorted && base.Created)
            {
                base.OnDataSourceChanged(e);
            }
            this.RefreshItems();
        }

        protected override void OnDisplayMemberChanged(EventArgs e)
        {
            base.OnDisplayMemberChanged(e);
            this.RefreshItems();
        }

        protected virtual void OnDrawItem(DrawItemEventArgs e)
        {
            DrawItemEventHandler handler = (DrawItemEventHandler) base.Events[EVENT_DRAWITEM];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDropDown(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_DROPDOWN];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDropDownClosed(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_DROPDOWNCLOSED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDropDownStyleChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_DROPDOWNSTYLE];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.ResetHeightCache();
            if (this.AutoCompleteMode == System.Windows.Forms.AutoCompleteMode.None)
            {
                this.UpdateControl(true);
            }
            else
            {
                base.RecreateHandle();
            }
            CommonProperties.xClearPreferredSizeCache(this);
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            this.UpdateControl(false);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnGotFocus(EventArgs e)
        {
            if (!this.canFireLostFocus)
            {
                base.OnGotFocus(e);
                this.canFireLostFocus = true;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            bool flag2;
            base.OnHandleCreated(e);
            if (this.MaxLength > 0)
            {
                base.SendMessage(0x141, this.MaxLength, 0);
            }
            if (((this.childEdit == null) && (this.childListBox == null)) && (this.DropDownStyle != ComboBoxStyle.DropDownList))
            {
                IntPtr window = System.Windows.Forms.UnsafeNativeMethods.GetWindow(new HandleRef(this, base.Handle), 5);
                if (window != IntPtr.Zero)
                {
                    if (this.DropDownStyle == ComboBoxStyle.Simple)
                    {
                        this.childListBox = new ComboBoxChildNativeWindow(this);
                        this.childListBox.AssignHandle(window);
                        window = System.Windows.Forms.UnsafeNativeMethods.GetWindow(new HandleRef(this, window), 2);
                    }
                    this.childEdit = new ComboBoxChildNativeWindow(this);
                    this.childEdit.AssignHandle(window);
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.childEdit.Handle), 0xd3, 3, 0);
                }
            }
            int integer = base.Properties.GetInteger(PropDropDownWidth, out flag2);
            if (flag2)
            {
                base.SendMessage(0x160, integer, 0);
            }
            flag2 = false;
            base.Properties.GetInteger(PropItemHeight, out flag2);
            if (flag2)
            {
                this.UpdateItemHeight();
            }
            if (this.DropDownStyle == ComboBoxStyle.Simple)
            {
                base.Height = this.requestedHeight;
            }
            try
            {
                this.fromHandleCreate = true;
                this.SetAutoComplete(false, false);
            }
            finally
            {
                this.fromHandleCreate = false;
            }
            if (this.itemsCollection != null)
            {
                foreach (object obj2 in this.itemsCollection)
                {
                    this.NativeAdd(obj2);
                }
                if (this.selectedIndex >= 0)
                {
                    base.SendMessage(0x14e, this.selectedIndex, 0);
                    this.UpdateText();
                    this.selectedIndex = -1;
                }
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            this.dropDownHandle = IntPtr.Zero;
            if (base.Disposing)
            {
                this.itemsCollection = null;
                this.selectedIndex = -1;
            }
            else
            {
                this.selectedIndex = this.SelectedIndex;
            }
            if (this.stringSource != null)
            {
                this.stringSource.ReleaseAutoComplete();
                this.stringSource = null;
            }
            base.OnHandleDestroyed(e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (this.SystemAutoCompleteEnabled)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    this.NotifyAutoComplete(true);
                }
                else if ((e.KeyCode == Keys.Escape) && this.autoCompleteDroppedDown)
                {
                    this.NotifyAutoComplete(false);
                }
                this.autoCompleteDroppedDown = false;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if ((!e.Handled && ((e.KeyChar == '\r') || (e.KeyChar == '\x001b'))) && this.DroppedDown)
            {
                this.dropDown = false;
                if (base.FormattingEnabled)
                {
                    this.Text = this.WindowText;
                    this.SelectAll();
                    e.Handled = false;
                }
                else
                {
                    this.DroppedDown = false;
                    e.Handled = true;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnLostFocus(EventArgs e)
        {
            if (this.canFireLostFocus)
            {
                if (((this.AutoCompleteMode != System.Windows.Forms.AutoCompleteMode.None) && (this.AutoCompleteSource == System.Windows.Forms.AutoCompleteSource.ListItems)) && (this.DropDownStyle == ComboBoxStyle.DropDownList))
                {
                    this.MatchingText = "";
                }
                base.OnLostFocus(e);
                this.canFireLostFocus = false;
            }
        }

        protected virtual void OnMeasureItem(MeasureItemEventArgs e)
        {
            MeasureItemEventHandler handler = (MeasureItemEventHandler) base.Events[EVENT_MEASUREITEM];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this.MouseIsOver = true;
        }

        private void OnMouseEnterInternal(EventArgs args)
        {
            if (!this.mouseInEdit)
            {
                this.OnMouseEnter(args);
                this.mouseInEdit = true;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.MouseIsOver = false;
        }

        private void OnMouseLeaveInternal(EventArgs args)
        {
            System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
            System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(this, base.Handle), ref rect);
            Rectangle rectangle = new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
            Point mousePosition = Control.MousePosition;
            if (!rectangle.Contains(mousePosition))
            {
                this.OnMouseLeave(args);
                this.mouseInEdit = false;
            }
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            if (this.DropDownStyle == ComboBoxStyle.Simple)
            {
                base.Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (this.DropDownStyle == ComboBoxStyle.Simple)
            {
                this.InvalidateEverything();
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            EventHandler handler = (EventHandler) base.Events[EVENT_SELECTEDINDEXCHANGED];
            if (handler != null)
            {
                handler(this, e);
            }
            if (((base.DataManager != null) && (base.DataManager.Position != this.SelectedIndex)) && (!base.FormattingEnabled || (this.SelectedIndex != -1)))
            {
                base.DataManager.Position = this.SelectedIndex;
            }
        }

        protected virtual void OnSelectedItemChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_SELECTEDITEMCHANGED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnSelectedValueChanged(EventArgs e)
        {
            base.OnSelectedValueChanged(e);
            this.selectedValueChangedFired = true;
        }

        protected virtual void OnSelectionChangeCommitted(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_SELECTIONCHANGECOMMITTED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnSelectionChangeCommittedInternal(EventArgs e)
        {
            if (this.allowCommit)
            {
                try
                {
                    this.allowCommit = false;
                    this.OnSelectionChangeCommitted(e);
                }
                finally
                {
                    this.allowCommit = true;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnTextChanged(EventArgs e)
        {
            if (this.SystemAutoCompleteEnabled)
            {
                string text = this.Text;
                if (text != this.lastTextChangedValue)
                {
                    base.OnTextChanged(e);
                    this.lastTextChangedValue = text;
                }
            }
            else
            {
                base.OnTextChanged(e);
            }
        }

        protected virtual void OnTextUpdate(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_TEXTUPDATE];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnValidating(CancelEventArgs e)
        {
            if (this.SystemAutoCompleteEnabled)
            {
                this.NotifyAutoComplete();
            }
            base.OnValidating(e);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessKeyEventArgs(ref Message m)
        {
            return ((((this.AutoCompleteMode != System.Windows.Forms.AutoCompleteMode.None) && (this.AutoCompleteSource == System.Windows.Forms.AutoCompleteSource.ListItems)) && ((this.DropDownStyle == ComboBoxStyle.DropDownList) && this.InterceptAutoCompleteKeystroke(m))) || base.ProcessKeyEventArgs(ref m));
        }

        internal override void RecreateHandleCore()
        {
            string windowText = this.WindowText;
            base.RecreateHandleCore();
            if (!string.IsNullOrEmpty(windowText) && string.IsNullOrEmpty(this.WindowText))
            {
                this.WindowText = windowText;
            }
        }

        protected override void RefreshItem(int index)
        {
            this.Items.SetItemInternal(index, this.Items[index]);
        }

        protected override void RefreshItems()
        {
            int selectedIndex = this.SelectedIndex;
            ObjectCollection itemsCollection = this.itemsCollection;
            this.itemsCollection = null;
            object[] destination = null;
            if ((base.DataManager != null) && (base.DataManager.Count != -1))
            {
                destination = new object[base.DataManager.Count];
                for (int i = 0; i < destination.Length; i++)
                {
                    destination[i] = base.DataManager[i];
                }
            }
            else if (itemsCollection != null)
            {
                destination = new object[itemsCollection.Count];
                itemsCollection.CopyTo(destination, 0);
            }
            this.BeginUpdate();
            try
            {
                if (base.IsHandleCreated)
                {
                    this.NativeClear();
                }
                if (destination != null)
                {
                    this.Items.AddRangeInternal(destination);
                }
                if (base.DataManager != null)
                {
                    this.SelectedIndex = base.DataManager.Position;
                }
                else
                {
                    this.SelectedIndex = selectedIndex;
                }
            }
            finally
            {
                this.EndUpdate();
            }
        }

        private void ReleaseChildWindow()
        {
            if (this.childEdit != null)
            {
                this.childEdit.ReleaseHandle();
                this.childEdit = null;
            }
            if (this.childListBox != null)
            {
                this.childListBox.ReleaseHandle();
                this.childListBox = null;
            }
        }

        private void ResetAutoCompleteCustomSource()
        {
            this.AutoCompleteCustomSource = null;
        }

        private void ResetDropDownWidth()
        {
            base.Properties.RemoveInteger(PropDropDownWidth);
        }

        private void ResetHeightCache()
        {
            this.prefHeightCache = -1;
        }

        private void ResetItemHeight()
        {
            base.Properties.RemoveInteger(PropItemHeight);
        }

        public override void ResetText()
        {
            base.ResetText();
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            if ((factor.Width != 1f) && (factor.Height != 1f))
            {
                this.ResetHeightCache();
            }
            base.ScaleControl(factor, specified);
        }

        public void Select(int start, int length)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException("start", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "start", start.ToString(CultureInfo.CurrentCulture) }));
            }
            int high = start + length;
            if (high < 0)
            {
                throw new ArgumentOutOfRangeException("length", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "length", length.ToString(CultureInfo.CurrentCulture) }));
            }
            base.SendMessage(0x142, 0, System.Windows.Forms.NativeMethods.Util.MAKELPARAM(start, high));
        }

        public void SelectAll()
        {
            this.Select(0, 0x7fffffff);
        }

        private void SetAutoComplete(bool reset, bool recreate)
        {
            if (base.IsHandleCreated && (this.childEdit != null))
            {
                if (this.AutoCompleteMode != System.Windows.Forms.AutoCompleteMode.None)
                {
                    if ((!this.fromHandleCreate && recreate) && base.IsHandleCreated)
                    {
                        System.Windows.Forms.AutoCompleteMode autoCompleteMode = this.AutoCompleteMode;
                        this.autoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
                        base.RecreateHandle();
                        this.autoCompleteMode = autoCompleteMode;
                    }
                    if (this.AutoCompleteSource == System.Windows.Forms.AutoCompleteSource.CustomSource)
                    {
                        if (this.AutoCompleteCustomSource != null)
                        {
                            if (this.AutoCompleteCustomSource.Count == 0)
                            {
                                int flags = -1610612736;
                                System.Windows.Forms.SafeNativeMethods.SHAutoComplete(new HandleRef(this, this.childEdit.Handle), flags);
                            }
                            else if (this.stringSource != null)
                            {
                                this.stringSource.RefreshList(this.GetStringsForAutoComplete(this.AutoCompleteCustomSource));
                            }
                            else
                            {
                                this.stringSource = new StringSource(this.GetStringsForAutoComplete(this.AutoCompleteCustomSource));
                                if (!this.stringSource.Bind(new HandleRef(this, this.childEdit.Handle), (int) this.AutoCompleteMode))
                                {
                                    throw new ArgumentException(System.Windows.Forms.SR.GetString("AutoCompleteFailure"));
                                }
                            }
                        }
                    }
                    else if (this.AutoCompleteSource == System.Windows.Forms.AutoCompleteSource.ListItems)
                    {
                        if (this.DropDownStyle == ComboBoxStyle.DropDownList)
                        {
                            int num3 = -1610612736;
                            System.Windows.Forms.SafeNativeMethods.SHAutoComplete(new HandleRef(this, this.childEdit.Handle), num3);
                        }
                        else if (this.itemsCollection != null)
                        {
                            if (this.itemsCollection.Count == 0)
                            {
                                int num2 = -1610612736;
                                System.Windows.Forms.SafeNativeMethods.SHAutoComplete(new HandleRef(this, this.childEdit.Handle), num2);
                            }
                            else if (this.stringSource != null)
                            {
                                this.stringSource.RefreshList(this.GetStringsForAutoComplete(this.Items));
                            }
                            else
                            {
                                this.stringSource = new StringSource(this.GetStringsForAutoComplete(this.Items));
                                if (!this.stringSource.Bind(new HandleRef(this, this.childEdit.Handle), (int) this.AutoCompleteMode))
                                {
                                    throw new ArgumentException(System.Windows.Forms.SR.GetString("AutoCompleteFailureListItems"));
                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            int num4 = 0;
                            if (this.AutoCompleteMode == System.Windows.Forms.AutoCompleteMode.Suggest)
                            {
                                num4 |= -1879048192;
                            }
                            if (this.AutoCompleteMode == System.Windows.Forms.AutoCompleteMode.Append)
                            {
                                num4 |= 0x60000000;
                            }
                            if (this.AutoCompleteMode == System.Windows.Forms.AutoCompleteMode.SuggestAppend)
                            {
                                num4 |= 0x10000000;
                                num4 |= 0x40000000;
                            }
                            System.Windows.Forms.SafeNativeMethods.SHAutoComplete(new HandleRef(this, this.childEdit.Handle), ((int) this.AutoCompleteSource) | num4);
                        }
                        catch (SecurityException)
                        {
                        }
                    }
                }
                else if (reset)
                {
                    int num5 = -1610612736;
                    System.Windows.Forms.SafeNativeMethods.SHAutoComplete(new HandleRef(this, this.childEdit.Handle), num5);
                }
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if ((specified & BoundsSpecified.Height) != BoundsSpecified.None)
            {
                this.requestedHeight = height;
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        protected override void SetItemCore(int index, object value)
        {
            this.Items.SetItemInternal(index, value);
        }

        protected override void SetItemsCore(IList value)
        {
            this.BeginUpdate();
            this.Items.ClearInternal();
            this.Items.AddRangeInternal(value);
            if (base.DataManager != null)
            {
                if (this.DataSource is ICurrencyManagerProvider)
                {
                    this.selectedValueChangedFired = false;
                }
                if (base.IsHandleCreated)
                {
                    base.SendMessage(0x14e, base.DataManager.Position, 0);
                }
                else
                {
                    this.selectedIndex = base.DataManager.Position;
                }
                if (!this.selectedValueChangedFired)
                {
                    this.OnSelectedValueChanged(EventArgs.Empty);
                    this.selectedValueChangedFired = false;
                }
            }
            this.EndUpdate();
        }

        private bool ShouldSerializeAutoCompleteCustomSource()
        {
            return ((this.autoCompleteCustomSource != null) && (this.autoCompleteCustomSource.Count > 0));
        }

        internal bool ShouldSerializeDropDownWidth()
        {
            return base.Properties.ContainsInteger(PropDropDownWidth);
        }

        internal bool ShouldSerializeItemHeight()
        {
            return base.Properties.ContainsInteger(PropItemHeight);
        }

        internal override bool ShouldSerializeText()
        {
            return ((this.SelectedIndex == -1) && base.ShouldSerializeText());
        }

        public override string ToString()
        {
            return (base.ToString() + ", Items.Count: " + ((this.itemsCollection == null) ? 0.ToString(CultureInfo.CurrentCulture) : this.itemsCollection.Count.ToString(CultureInfo.CurrentCulture)));
        }

        private void UpdateControl(bool recreate)
        {
            this.ResetHeightCache();
            if (base.IsHandleCreated)
            {
                if ((this.DropDownStyle == ComboBoxStyle.Simple) && recreate)
                {
                    base.RecreateHandle();
                }
                else
                {
                    this.UpdateItemHeight();
                    this.InvalidateEverything();
                }
            }
        }

        private void UpdateDropDownHeight()
        {
            if (this.dropDownHandle != IntPtr.Zero)
            {
                int dropDownHeight = this.DropDownHeight;
                if (dropDownHeight == 0x6a)
                {
                    int num2 = (this.itemsCollection == null) ? 0 : this.itemsCollection.Count;
                    int num3 = Math.Min(Math.Max(num2, 1), this.maxDropDownItems);
                    dropDownHeight = (this.ItemHeight * num3) + 2;
                }
                System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, this.dropDownHandle), System.Windows.Forms.NativeMethods.NullHandleRef, 0, 0, this.DropDownWidth, dropDownHeight, 6);
            }
        }

        private void UpdateItemHeight()
        {
            if (!base.IsHandleCreated)
            {
                base.CreateControl();
            }
            if (this.DrawMode == System.Windows.Forms.DrawMode.OwnerDrawFixed)
            {
                base.SendMessage(0x153, -1, this.ItemHeight);
                base.SendMessage(0x153, 0, this.ItemHeight);
            }
            else if (this.DrawMode == System.Windows.Forms.DrawMode.OwnerDrawVariable)
            {
                base.SendMessage(0x153, -1, this.ItemHeight);
                Graphics graphics = base.CreateGraphicsInternal();
                for (int i = 0; i < this.Items.Count; i++)
                {
                    int itemHeight = (int) ((long) base.SendMessage(340, i, 0));
                    MeasureItemEventArgs e = new MeasureItemEventArgs(graphics, i, itemHeight);
                    this.OnMeasureItem(e);
                    if (e.ItemHeight != itemHeight)
                    {
                        base.SendMessage(0x153, i, e.ItemHeight);
                    }
                }
                graphics.Dispose();
            }
        }

        internal bool UpdateNeeded()
        {
            return (this.updateCount == 0);
        }

        private void UpdateText()
        {
            string lParam = null;
            if (this.SelectedIndex != -1)
            {
                object item = this.Items[this.SelectedIndex];
                if (item != null)
                {
                    lParam = base.GetItemText(item);
                }
            }
            this.Text = lParam;
            if (((this.DropDownStyle == ComboBoxStyle.DropDown) && (this.childEdit != null)) && (this.childEdit.Handle != IntPtr.Zero))
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.childEdit.Handle), 12, IntPtr.Zero, lParam);
            }
        }

        private void WmEraseBkgnd(ref Message m)
        {
            if ((this.DropDownStyle == ComboBoxStyle.Simple) && (this.ParentInternal != null))
            {
                System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                System.Windows.Forms.SafeNativeMethods.GetClientRect(new HandleRef(this, base.Handle), ref rect);
                Control parentInternal = this.ParentInternal;
                Graphics graphics = Graphics.FromHdcInternal(m.WParam);
                if (parentInternal != null)
                {
                    Brush brush = new SolidBrush(parentInternal.BackColor);
                    graphics.FillRectangle(brush, rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
                    brush.Dispose();
                }
                else
                {
                    graphics.FillRectangle(SystemBrushes.Control, rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
                }
                graphics.Dispose();
                m.Result = (IntPtr) 1;
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        private void WmParentNotify(ref Message m)
        {
            base.WndProc(ref m);
            if (((int) ((long) m.WParam)) == 0x3e80001)
            {
                this.dropDownHandle = m.LParam;
            }
        }

        private void WmReflectCommand(ref Message m)
        {
            switch (System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam))
            {
                case 1:
                    this.UpdateText();
                    this.OnSelectedIndexChanged(EventArgs.Empty);
                    return;

                case 2:
                case 3:
                case 4:
                    break;

                case 5:
                    this.OnTextChanged(EventArgs.Empty);
                    return;

                case 6:
                    this.OnTextUpdate(EventArgs.Empty);
                    return;

                case 7:
                    this.currentText = this.Text;
                    this.dropDown = true;
                    this.OnDropDown(EventArgs.Empty);
                    this.UpdateDropDownHeight();
                    return;

                case 8:
                    this.OnDropDownClosed(EventArgs.Empty);
                    if ((base.FormattingEnabled && (this.Text != this.currentText)) && this.dropDown)
                    {
                        this.OnTextChanged(EventArgs.Empty);
                    }
                    this.dropDown = false;
                    return;

                case 9:
                    this.OnSelectionChangeCommittedInternal(EventArgs.Empty);
                    break;

                default:
                    return;
            }
        }

        private void WmReflectDrawItem(ref Message m)
        {
            System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT lParam = (System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT));
            IntPtr handle = Control.SetUpPalette(lParam.hDC, false, false);
            try
            {
                using (Graphics graphics = Graphics.FromHdcInternal(lParam.hDC))
                {
                    this.OnDrawItem(new DrawItemEventArgs(graphics, this.Font, Rectangle.FromLTRB(lParam.rcItem.left, lParam.rcItem.top, lParam.rcItem.right, lParam.rcItem.bottom), lParam.itemID, (DrawItemState) lParam.itemState, this.ForeColor, this.BackColor));
                }
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    System.Windows.Forms.SafeNativeMethods.SelectPalette(new HandleRef(this, lParam.hDC), new HandleRef(null, handle), 0);
                }
            }
            m.Result = (IntPtr) 1;
        }

        private void WmReflectMeasureItem(ref Message m)
        {
            System.Windows.Forms.NativeMethods.MEASUREITEMSTRUCT lParam = (System.Windows.Forms.NativeMethods.MEASUREITEMSTRUCT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.MEASUREITEMSTRUCT));
            if ((this.DrawMode == System.Windows.Forms.DrawMode.OwnerDrawVariable) && (lParam.itemID >= 0))
            {
                Graphics graphics = base.CreateGraphicsInternal();
                MeasureItemEventArgs e = new MeasureItemEventArgs(graphics, lParam.itemID, this.ItemHeight);
                this.OnMeasureItem(e);
                lParam.itemHeight = e.ItemHeight;
                graphics.Dispose();
            }
            else
            {
                lParam.itemHeight = this.ItemHeight;
            }
            Marshal.StructureToPtr(lParam, m.LParam, false);
            m.Result = (IntPtr) 1;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 7:
                    try
                    {
                        this.fireSetFocus = false;
                        base.WndProc(ref m);
                        return;
                    }
                    finally
                    {
                        this.fireSetFocus = true;
                    }
                    break;

                case 8:
                    break;

                case 15:
                    if (!base.GetStyle(ControlStyles.UserPaint) && ((this.FlatStyle == System.Windows.Forms.FlatStyle.Flat) || (this.FlatStyle == System.Windows.Forms.FlatStyle.Popup)))
                    {
                        using (WindowsRegion region = new WindowsRegion(this.FlatComboBoxAdapter.dropDownRect))
                        {
                            using (WindowsRegion region2 = new WindowsRegion(base.Bounds))
                            {
                                IntPtr wParam;
                                System.Windows.Forms.NativeMethods.RegionFlags flags = (System.Windows.Forms.NativeMethods.RegionFlags) System.Windows.Forms.SafeNativeMethods.GetUpdateRgn(new HandleRef(this, base.Handle), new HandleRef(this, region2.HRegion), true);
                                region.CombineRegion(region2, region, RegionCombineMode.DIFF);
                                Rectangle updateRegionBox = region2.ToRectangle();
                                this.FlatComboBoxAdapter.ValidateOwnerDrawRegions(this, updateRegionBox);
                                System.Windows.Forms.NativeMethods.PAINTSTRUCT lpPaint = new System.Windows.Forms.NativeMethods.PAINTSTRUCT();
                                bool flag2 = false;
                                if (m.WParam == IntPtr.Zero)
                                {
                                    wParam = System.Windows.Forms.UnsafeNativeMethods.BeginPaint(new HandleRef(this, base.Handle), ref lpPaint);
                                    flag2 = true;
                                }
                                else
                                {
                                    wParam = m.WParam;
                                }
                                using (DeviceContext context = DeviceContext.FromHdc(wParam))
                                {
                                    using (WindowsGraphics graphics = new WindowsGraphics(context))
                                    {
                                        if (flags != System.Windows.Forms.NativeMethods.RegionFlags.ERROR)
                                        {
                                            graphics.DeviceContext.SetClip(region);
                                        }
                                        m.WParam = wParam;
                                        this.DefWndProc(ref m);
                                        if (flags != System.Windows.Forms.NativeMethods.RegionFlags.ERROR)
                                        {
                                            graphics.DeviceContext.SetClip(region2);
                                        }
                                        using (Graphics graphics2 = Graphics.FromHdcInternal(wParam))
                                        {
                                            this.FlatComboBoxAdapter.DrawFlatCombo(this, graphics2);
                                        }
                                    }
                                }
                                if (flag2)
                                {
                                    System.Windows.Forms.UnsafeNativeMethods.EndPaint(new HandleRef(this, base.Handle), ref lpPaint);
                                }
                            }
                            return;
                        }
                    }
                    base.WndProc(ref m);
                    return;

                case 20:
                    this.WmEraseBkgnd(ref m);
                    return;

                case 0x47:
                    if (!this.suppressNextWindosPos)
                    {
                        base.WndProc(ref m);
                    }
                    this.suppressNextWindosPos = false;
                    return;

                case 130:
                    base.WndProc(ref m);
                    this.ReleaseChildWindow();
                    return;

                case 0x20:
                    base.WndProc(ref m);
                    return;

                case 0x30:
                    if (base.Width == 0)
                    {
                        this.suppressNextWindosPos = true;
                    }
                    base.WndProc(ref m);
                    return;

                case 0x133:
                case 0x134:
                    goto Label_017F;

                case 0x201:
                    this.mouseEvents = true;
                    base.WndProc(ref m);
                    return;

                case 0x202:
                {
                    System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                    System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(this, base.Handle), ref rect);
                    Rectangle rectangle = new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
                    int x = System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam);
                    int y = System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam);
                    Point p = new Point(x, y);
                    p = base.PointToScreen(p);
                    if (this.mouseEvents && !base.ValidationCancelled)
                    {
                        this.mouseEvents = false;
                        if (base.Capture && rectangle.Contains(p))
                        {
                            this.OnClick(new MouseEventArgs(MouseButtons.Left, 1, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                            this.OnMouseClick(new MouseEventArgs(MouseButtons.Left, 1, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                        }
                        base.WndProc(ref m);
                        return;
                    }
                    base.CaptureInternal = false;
                    this.DefWndProc(ref m);
                    return;
                }
                case 0x210:
                    this.WmParentNotify(ref m);
                    return;

                case 0x202b:
                    this.WmReflectDrawItem(ref m);
                    return;

                case 0x202c:
                    this.WmReflectMeasureItem(ref m);
                    return;

                case 0x2111:
                    this.WmReflectCommand(ref m);
                    return;

                case 0x2a3:
                    this.DefWndProc(ref m);
                    this.OnMouseLeaveInternal(EventArgs.Empty);
                    return;

                case 0x318:
                    if ((base.GetStyle(ControlStyles.UserPaint) || (this.FlatStyle != System.Windows.Forms.FlatStyle.Flat)) && (this.FlatStyle != System.Windows.Forms.FlatStyle.Popup))
                    {
                        goto Label_04EF;
                    }
                    this.DefWndProc(ref m);
                    if ((((int) ((long) m.LParam)) & 4) != 4)
                    {
                        goto Label_04EF;
                    }
                    if ((!base.GetStyle(ControlStyles.UserPaint) && (this.FlatStyle == System.Windows.Forms.FlatStyle.Flat)) || (this.FlatStyle == System.Windows.Forms.FlatStyle.Popup))
                    {
                        using (Graphics graphics3 = Graphics.FromHdcInternal(m.WParam))
                        {
                            this.FlatComboBoxAdapter.DrawFlatCombo(this, graphics3);
                        }
                    }
                    return;

                default:
                    if (m.Msg == System.Windows.Forms.NativeMethods.WM_MOUSEENTER)
                    {
                        this.DefWndProc(ref m);
                        this.OnMouseEnterInternal(EventArgs.Empty);
                        return;
                    }
                    base.WndProc(ref m);
                    return;
            }
            try
            {
                this.fireLostFocus = false;
                base.WndProc(ref m);
                if (((!Application.RenderWithVisualStyles && !base.GetStyle(ControlStyles.UserPaint)) && (this.DropDownStyle == ComboBoxStyle.DropDownList)) && ((this.FlatStyle == System.Windows.Forms.FlatStyle.Flat) || (this.FlatStyle == System.Windows.Forms.FlatStyle.Popup)))
                {
                    System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, base.Handle), 0x2a3, 0, 0);
                }
                return;
            }
            finally
            {
                this.fireLostFocus = true;
            }
        Label_017F:
            m.Result = this.InitializeDCForWmCtlColor(m.WParam, m.Msg);
            return;
        Label_04EF:
            base.WndProc(ref m);
        }

        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRDescription("ComboBoxAutoCompleteCustomSourceDescr"), Browsable(true), Localizable(true)]
        public AutoCompleteStringCollection AutoCompleteCustomSource
        {
            get
            {
                if (this.autoCompleteCustomSource == null)
                {
                    this.autoCompleteCustomSource = new AutoCompleteStringCollection();
                    this.autoCompleteCustomSource.CollectionChanged += new CollectionChangeEventHandler(this.OnAutoCompleteCustomSourceChanged);
                }
                return this.autoCompleteCustomSource;
            }
            set
            {
                if (this.autoCompleteCustomSource != value)
                {
                    if (this.autoCompleteCustomSource != null)
                    {
                        this.autoCompleteCustomSource.CollectionChanged -= new CollectionChangeEventHandler(this.OnAutoCompleteCustomSourceChanged);
                    }
                    this.autoCompleteCustomSource = value;
                    if (this.autoCompleteCustomSource != null)
                    {
                        this.autoCompleteCustomSource.CollectionChanged += new CollectionChangeEventHandler(this.OnAutoCompleteCustomSourceChanged);
                    }
                    this.SetAutoComplete(false, true);
                }
            }
        }

        [Browsable(true), DefaultValue(0), System.Windows.Forms.SRDescription("ComboBoxAutoCompleteModeDescr"), EditorBrowsable(EditorBrowsableState.Always)]
        public System.Windows.Forms.AutoCompleteMode AutoCompleteMode
        {
            get
            {
                return this.autoCompleteMode;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.AutoCompleteMode));
                }
                if (((this.DropDownStyle == ComboBoxStyle.DropDownList) && (this.AutoCompleteSource != System.Windows.Forms.AutoCompleteSource.ListItems)) && (value != System.Windows.Forms.AutoCompleteMode.None))
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ComboBoxAutoCompleteModeOnlyNoneAllowed"));
                }
                if (Application.OleRequired() != ApartmentState.STA)
                {
                    throw new ThreadStateException(System.Windows.Forms.SR.GetString("ThreadMustBeSTA"));
                }
                bool reset = false;
                if ((this.autoCompleteMode != System.Windows.Forms.AutoCompleteMode.None) && (value == System.Windows.Forms.AutoCompleteMode.None))
                {
                    reset = true;
                }
                this.autoCompleteMode = value;
                this.SetAutoComplete(reset, true);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), DefaultValue(0x80), System.Windows.Forms.SRDescription("ComboBoxAutoCompleteSourceDescr"), Browsable(true)]
        public System.Windows.Forms.AutoCompleteSource AutoCompleteSource
        {
            get
            {
                return this.autoCompleteSource;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid_NotSequential(value, (int) value, new int[] { 0x80, 7, 6, 0x40, 1, 0x20, 2, 0x100, 4 }))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.AutoCompleteSource));
                }
                if (((this.DropDownStyle == ComboBoxStyle.DropDownList) && (this.AutoCompleteMode != System.Windows.Forms.AutoCompleteMode.None)) && (value != System.Windows.Forms.AutoCompleteSource.ListItems))
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ComboBoxAutoCompleteSourceOnlyListItemsAllowed"));
                }
                if (Application.OleRequired() != ApartmentState.STA)
                {
                    throw new ThreadStateException(System.Windows.Forms.SR.GetString("ThreadMustBeSTA"));
                }
                if (((value != System.Windows.Forms.AutoCompleteSource.None) && (value != System.Windows.Forms.AutoCompleteSource.CustomSource)) && (value != System.Windows.Forms.AutoCompleteSource.ListItems))
                {
                    new FileIOPermission(PermissionState.Unrestricted) { AllFiles = FileIOPermissionAccess.PathDiscovery }.Demand();
                }
                this.autoCompleteSource = value;
                this.SetAutoComplete(false, true);
            }
        }

        public override Color BackColor
        {
            get
            {
                if (this.ShouldSerializeBackColor())
                {
                    return base.BackColor;
                }
                return SystemColors.Window;
            }
            set
            {
                base.BackColor = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ClassName = "COMBOBOX";
                createParams.Style |= 0x200240;
                createParams.ExStyle |= 0x200;
                if (!this.integralHeight)
                {
                    createParams.Style |= 0x400;
                }
                switch (this.DropDownStyle)
                {
                    case ComboBoxStyle.Simple:
                        createParams.Style |= 1;
                        break;

                    case ComboBoxStyle.DropDown:
                        createParams.Style |= 2;
                        createParams.Height = this.PreferredHeight;
                        break;

                    case ComboBoxStyle.DropDownList:
                        createParams.Style |= 3;
                        createParams.Height = this.PreferredHeight;
                        break;
                }
                switch (this.DrawMode)
                {
                    case System.Windows.Forms.DrawMode.OwnerDrawFixed:
                        createParams.Style |= 0x10;
                        return createParams;

                    case System.Windows.Forms.DrawMode.OwnerDrawVariable:
                        createParams.Style |= 0x20;
                        return createParams;
                }
                return createParams;
            }
        }

        [RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRCategory("CatData"), AttributeProvider(typeof(IListSource)), System.Windows.Forms.SRDescription("ListControlDataSourceDescr"), DefaultValue((string) null)]
        public object DataSource
        {
            get
            {
                return base.DataSource;
            }
            set
            {
                base.DataSource = value;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(0x79, this.PreferredHeight);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(0), System.Windows.Forms.SRDescription("ComboBoxDrawModeDescr"), RefreshProperties(RefreshProperties.Repaint)]
        public System.Windows.Forms.DrawMode DrawMode
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropDrawMode, out flag);
                if (flag)
                {
                    return (System.Windows.Forms.DrawMode) integer;
                }
                return System.Windows.Forms.DrawMode.Normal;
            }
            set
            {
                if (this.DrawMode != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.DrawMode));
                    }
                    this.ResetHeightCache();
                    base.Properties.SetInteger(PropDrawMode, (int) value);
                    base.RecreateHandle();
                }
            }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DefaultValue(0x6a), System.Windows.Forms.SRDescription("ComboBoxDropDownHeightDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public int DropDownHeight
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropDropDownHeight, out flag);
                if (flag)
                {
                    return integer;
                }
                return 0x6a;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("DropDownHeight", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "DropDownHeight", value.ToString(CultureInfo.CurrentCulture) }));
                }
                if (base.Properties.GetInteger(PropDropDownHeight) != value)
                {
                    base.Properties.SetInteger(PropDropDownHeight, value);
                    this.IntegralHeight = false;
                }
            }
        }

        [System.Windows.Forms.SRDescription("ComboBoxStyleDescr"), System.Windows.Forms.SRCategory("CatAppearance"), RefreshProperties(RefreshProperties.Repaint), DefaultValue(1)]
        public ComboBoxStyle DropDownStyle
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropStyle, out flag);
                if (flag)
                {
                    return (ComboBoxStyle) integer;
                }
                return ComboBoxStyle.DropDown;
            }
            set
            {
                if (this.DropDownStyle != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(ComboBoxStyle));
                    }
                    if (((value == ComboBoxStyle.DropDownList) && (this.AutoCompleteSource != System.Windows.Forms.AutoCompleteSource.ListItems)) && (this.AutoCompleteMode != System.Windows.Forms.AutoCompleteMode.None))
                    {
                        this.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
                    }
                    this.ResetHeightCache();
                    base.Properties.SetInteger(PropStyle, (int) value);
                    if (base.IsHandleCreated)
                    {
                        base.RecreateHandle();
                    }
                    this.OnDropDownStyleChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ComboBoxDropDownWidthDescr")]
        public int DropDownWidth
        {
            get
            {
                bool flag;
                int integer = base.Properties.GetInteger(PropDropDownWidth, out flag);
                if (flag)
                {
                    return integer;
                }
                return base.Width;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("DropDownWidth", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "DropDownWidth", value.ToString(CultureInfo.CurrentCulture) }));
                }
                if (base.Properties.GetInteger(PropDropDownWidth) != value)
                {
                    base.Properties.SetInteger(PropDropDownWidth, value);
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x160, value, 0);
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ComboBoxDroppedDownDescr"), Browsable(false)]
        public bool DroppedDown
        {
            get
            {
                return (base.IsHandleCreated && (((int) ((long) base.SendMessage(0x157, 0, 0))) != 0));
            }
            set
            {
                if (!base.IsHandleCreated)
                {
                    this.CreateHandle();
                }
                base.SendMessage(0x14f, value ? -1 : 0, 0);
            }
        }

        private FlatComboAdapter FlatComboBoxAdapter
        {
            get
            {
                FlatComboAdapter adapter = base.Properties.GetObject(PropFlatComboAdapter) as FlatComboAdapter;
                if ((adapter == null) || !adapter.IsValid(this))
                {
                    adapter = this.CreateFlatComboAdapterInstance();
                    base.Properties.SetObject(PropFlatComboAdapter, adapter);
                }
                return adapter;
            }
        }

        [System.Windows.Forms.SRDescription("ComboBoxFlatStyleDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(2), Localizable(true)]
        public System.Windows.Forms.FlatStyle FlatStyle
        {
            get
            {
                return this.flatStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.FlatStyle));
                }
                this.flatStyle = value;
                base.Invalidate();
            }
        }

        public override bool Focused
        {
            get
            {
                if (base.Focused)
                {
                    return true;
                }
                IntPtr focus = System.Windows.Forms.UnsafeNativeMethods.GetFocus();
                if (!(focus != IntPtr.Zero))
                {
                    return false;
                }
                return (((this.childEdit != null) && (focus == this.childEdit.Handle)) || ((this.childListBox != null) && (focus == this.childListBox.Handle)));
            }
        }

        public override Color ForeColor
        {
            get
            {
                if (this.ShouldSerializeForeColor())
                {
                    return base.ForeColor;
                }
                return SystemColors.WindowText;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRDescription("ComboBoxIntegralHeightDescr"), System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true)]
        public bool IntegralHeight
        {
            get
            {
                return this.integralHeight;
            }
            set
            {
                if (this.integralHeight != value)
                {
                    this.integralHeight = value;
                    base.RecreateHandle();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ComboBoxItemHeightDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatBehavior")]
        public int ItemHeight
        {
            get
            {
                System.Windows.Forms.DrawMode drawMode = this.DrawMode;
                if (((drawMode == System.Windows.Forms.DrawMode.OwnerDrawFixed) || (drawMode == System.Windows.Forms.DrawMode.OwnerDrawVariable)) || !base.IsHandleCreated)
                {
                    bool flag;
                    int integer = base.Properties.GetInteger(PropItemHeight, out flag);
                    if (flag)
                    {
                        return integer;
                    }
                    return (base.FontHeight + 2);
                }
                int num2 = (int) ((long) base.SendMessage(340, 0, 0));
                if (num2 == -1)
                {
                    throw new Win32Exception();
                }
                return num2;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("ItemHeight", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "ItemHeight", value.ToString(CultureInfo.CurrentCulture) }));
                }
                this.ResetHeightCache();
                if (base.Properties.GetInteger(PropItemHeight) != value)
                {
                    base.Properties.SetInteger(PropItemHeight, value);
                    if (this.DrawMode != System.Windows.Forms.DrawMode.Normal)
                    {
                        this.UpdateItemHeight();
                    }
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRCategory("CatData"), System.Windows.Forms.SRDescription("ComboBoxItemsDescr"), Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ObjectCollection Items
        {
            get
            {
                if (this.itemsCollection == null)
                {
                    this.itemsCollection = new ObjectCollection(this);
                }
                return this.itemsCollection;
            }
        }

        private string MatchingText
        {
            get
            {
                string str = (string) base.Properties.GetObject(PropMatchingText);
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                if ((value != null) || base.Properties.ContainsObject(PropMatchingText))
                {
                    base.Properties.SetObject(PropMatchingText, value);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(8), System.Windows.Forms.SRDescription("ComboBoxMaxDropDownItemsDescr"), Localizable(true)]
        public int MaxDropDownItems
        {
            get
            {
                return this.maxDropDownItems;
            }
            set
            {
                if ((value < 1) || (value > 100))
                {
                    object[] args = new object[] { "MaxDropDownItems", value.ToString(CultureInfo.CurrentCulture), 1.ToString(CultureInfo.CurrentCulture), 100.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("MaxDropDownItems", System.Windows.Forms.SR.GetString("InvalidBoundArgument", args));
                }
                this.maxDropDownItems = (short) value;
            }
        }

        public override Size MaximumSize
        {
            get
            {
                return base.MaximumSize;
            }
            set
            {
                base.MaximumSize = new Size(value.Width, 0);
            }
        }

        [DefaultValue(0), Localizable(true), System.Windows.Forms.SRDescription("ComboBoxMaxLengthDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public int MaxLength
        {
            get
            {
                return base.Properties.GetInteger(PropMaxLength);
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (this.MaxLength != value)
                {
                    base.Properties.SetInteger(PropMaxLength, value);
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x141, value, 0);
                    }
                }
            }
        }

        public override Size MinimumSize
        {
            get
            {
                return base.MinimumSize;
            }
            set
            {
                base.MinimumSize = new Size(value.Width, 0);
            }
        }

        internal bool MouseIsOver
        {
            get
            {
                return this.mouseOver;
            }
            set
            {
                if (this.mouseOver != value)
                {
                    this.mouseOver = value;
                    if ((!base.ContainsFocus || !Application.RenderWithVisualStyles) && (this.FlatStyle == System.Windows.Forms.FlatStyle.Popup))
                    {
                        base.Invalidate();
                        base.Update();
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRDescription("ComboBoxPreferredHeightDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PreferredHeight
        {
            get
            {
                if (!base.FormattingEnabled)
                {
                    Size size = TextRenderer.MeasureText(LayoutUtils.TestString, this.Font, new Size(0x7fff, (int) (base.FontHeight * 1.25)), TextFormatFlags.SingleLine);
                    this.prefHeightCache = (short) ((size.Height + (SystemInformation.BorderSize.Height * 8)) + this.Padding.Size.Height);
                    return this.prefHeightCache;
                }
                if (this.prefHeightCache < 0)
                {
                    Size size2 = TextRenderer.MeasureText(LayoutUtils.TestString, this.Font, new Size(0x7fff, (int) (base.FontHeight * 1.25)), TextFormatFlags.SingleLine);
                    if (this.DropDownStyle == ComboBoxStyle.Simple)
                    {
                        int num = this.Items.Count + 1;
                        this.prefHeightCache = (short) (((size2.Height * num) + (SystemInformation.BorderSize.Height * 0x10)) + this.Padding.Size.Height);
                    }
                    else
                    {
                        this.prefHeightCache = (short) this.GetComboHeight();
                    }
                }
                return this.prefHeightCache;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), System.Windows.Forms.SRDescription("ComboBoxSelectedIndexDescr")]
        public override int SelectedIndex
        {
            get
            {
                if (base.IsHandleCreated)
                {
                    return (int) ((long) base.SendMessage(0x147, 0, 0));
                }
                return this.selectedIndex;
            }
            set
            {
                if (this.SelectedIndex != value)
                {
                    int count = 0;
                    if (this.itemsCollection != null)
                    {
                        count = this.itemsCollection.Count;
                    }
                    if ((value < -1) || (value >= count))
                    {
                        throw new ArgumentOutOfRangeException("SelectedIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "SelectedIndex", value.ToString(CultureInfo.CurrentCulture) }));
                    }
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x14e, value, 0);
                    }
                    else
                    {
                        this.selectedIndex = value;
                    }
                    this.UpdateText();
                    if (base.IsHandleCreated)
                    {
                        this.OnTextChanged(EventArgs.Empty);
                    }
                    this.OnSelectedItemChanged(EventArgs.Empty);
                    this.OnSelectedIndexChanged(EventArgs.Empty);
                }
            }
        }

        [Bindable(true), Browsable(false), System.Windows.Forms.SRDescription("ComboBoxSelectedItemDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem
        {
            get
            {
                int selectedIndex = this.SelectedIndex;
                if (selectedIndex != -1)
                {
                    return this.Items[selectedIndex];
                }
                return null;
            }
            set
            {
                int index = -1;
                if (this.itemsCollection != null)
                {
                    if (value != null)
                    {
                        index = this.itemsCollection.IndexOf(value);
                    }
                    else
                    {
                        this.SelectedIndex = -1;
                    }
                }
                if (index != -1)
                {
                    this.SelectedIndex = index;
                }
            }
        }

        [System.Windows.Forms.SRDescription("ComboBoxSelectedTextDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText
        {
            get
            {
                if (this.DropDownStyle == ComboBoxStyle.DropDownList)
                {
                    return "";
                }
                return this.Text.Substring(this.SelectionStart, this.SelectionLength);
            }
            set
            {
                if (this.DropDownStyle != ComboBoxStyle.DropDownList)
                {
                    string lParam = (value == null) ? "" : value;
                    base.CreateControl();
                    if (base.IsHandleCreated && (this.childEdit != null))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.childEdit.Handle), 0xc2, System.Windows.Forms.NativeMethods.InvalidIntPtr, lParam);
                    }
                }
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("ComboBoxSelectionLengthDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionLength
        {
            get
            {
                int[] lParam = new int[1];
                int[] wParam = new int[1];
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 320, wParam, lParam);
                return (lParam[0] - wParam[0]);
            }
            set
            {
                this.Select(this.SelectionStart, value);
            }
        }

        [System.Windows.Forms.SRDescription("ComboBoxSelectionStartDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionStart
        {
            get
            {
                int[] wParam = new int[1];
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 320, wParam, null);
                return wParam[0];
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("SelectionStart", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "SelectionStart", value.ToString(CultureInfo.CurrentCulture) }));
                }
                this.Select(value, this.SelectionLength);
            }
        }

        [System.Windows.Forms.SRDescription("ComboBoxSortedDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool Sorted
        {
            get
            {
                return this.sorted;
            }
            set
            {
                if (this.sorted != value)
                {
                    if ((this.DataSource != null) && value)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("ComboBoxSortWithDataSource"));
                    }
                    this.sorted = value;
                    this.RefreshItems();
                    this.SelectedIndex = -1;
                }
            }
        }

        private bool SystemAutoCompleteEnabled
        {
            get
            {
                return ((this.autoCompleteMode != System.Windows.Forms.AutoCompleteMode.None) && (this.DropDownStyle != ComboBoxStyle.DropDownList));
            }
        }

        [Bindable(true), Localizable(true)]
        public override string Text
        {
            get
            {
                if ((this.SelectedItem != null) && !base.BindingFieldEmpty)
                {
                    if (!base.FormattingEnabled)
                    {
                        return base.FilterItemOnProperty(this.SelectedItem).ToString();
                    }
                    string itemText = base.GetItemText(this.SelectedItem);
                    if (!string.IsNullOrEmpty(itemText) && (string.Compare(itemText, base.Text, true, CultureInfo.CurrentCulture) == 0))
                    {
                        return itemText;
                    }
                }
                return base.Text;
            }
            set
            {
                if (((this.DropDownStyle != ComboBoxStyle.DropDownList) || base.IsHandleCreated) || (string.IsNullOrEmpty(value) || (this.FindStringExact(value) != -1)))
                {
                    base.Text = value;
                    object item = null;
                    item = this.SelectedItem;
                    if (!base.DesignMode)
                    {
                        if (value == null)
                        {
                            this.SelectedIndex = -1;
                        }
                        else if ((value != null) && ((item == null) || (string.Compare(value, base.GetItemText(item), false, CultureInfo.CurrentCulture) != 0)))
                        {
                            int num = this.FindStringIgnoreCase(value);
                            if (num != -1)
                            {
                                this.SelectedIndex = num;
                            }
                        }
                    }
                }
            }
        }

        private sealed class ACNativeWindow : NativeWindow
        {
            private static Hashtable ACWindows = new Hashtable();
            internal static int inWndProcCnt;

            internal ACNativeWindow(IntPtr acHandle)
            {
                base.AssignHandle(acHandle);
                ACWindows.Add(acHandle, this);
                System.Windows.Forms.UnsafeNativeMethods.EnumChildWindows(new HandleRef(this, acHandle), new System.Windows.Forms.NativeMethods.EnumChildrenCallback(ComboBox.ACNativeWindow.RegisterACWindowRecursive), System.Windows.Forms.NativeMethods.NullHandleRef);
            }

            internal static void ClearNullACWindows()
            {
                ArrayList list = new ArrayList();
                foreach (DictionaryEntry entry in ACWindows)
                {
                    if (entry.Value == null)
                    {
                        list.Add(entry.Key);
                    }
                }
                foreach (IntPtr ptr in list)
                {
                    ACWindows.Remove(ptr);
                }
            }

            internal static void RegisterACWindow(IntPtr acHandle, bool subclass)
            {
                if ((subclass && ACWindows.ContainsKey(acHandle)) && (ACWindows[acHandle] == null))
                {
                    ACWindows.Remove(acHandle);
                }
                if (!ACWindows.ContainsKey(acHandle))
                {
                    if (subclass)
                    {
                        new ComboBox.ACNativeWindow(acHandle);
                    }
                    else
                    {
                        ACWindows.Add(acHandle, null);
                    }
                }
            }

            private static bool RegisterACWindowRecursive(IntPtr handle, IntPtr lparam)
            {
                if (!ACWindows.ContainsKey(handle))
                {
                    new ComboBox.ACNativeWindow(handle);
                }
                return true;
            }

            protected override void WndProc(ref Message m)
            {
                inWndProcCnt++;
                try
                {
                    base.WndProc(ref m);
                }
                finally
                {
                    inWndProcCnt--;
                }
                if (m.Msg == 130)
                {
                    ACWindows.Remove(base.Handle);
                }
            }

            internal static bool AutoCompleteActive
            {
                get
                {
                    if (inWndProcCnt > 0)
                    {
                        return true;
                    }
                    foreach (object obj2 in ACWindows.Values)
                    {
                        ComboBox.ACNativeWindow window = obj2 as ComboBox.ACNativeWindow;
                        if ((window != null) && window.Visible)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            internal bool Visible
            {
                get
                {
                    return System.Windows.Forms.SafeNativeMethods.IsWindowVisible(new HandleRef(this, base.Handle));
                }
            }
        }

        private class AutoCompleteDropDownFinder
        {
            private const string AutoCompleteClassName = "Auto-Suggest Dropdown";
            private const int MaxClassName = 0x100;
            private bool shouldSubClass;

            private bool Callback(IntPtr hWnd, IntPtr lParam)
            {
                HandleRef hRef = new HandleRef(null, hWnd);
                if (GetClassName(hRef) == "Auto-Suggest Dropdown")
                {
                    ComboBox.ACNativeWindow.RegisterACWindow(hRef.Handle, this.shouldSubClass);
                }
                return true;
            }

            internal void FindDropDowns()
            {
                this.FindDropDowns(true);
            }

            internal void FindDropDowns(bool subclass)
            {
                if (!subclass)
                {
                    ComboBox.ACNativeWindow.ClearNullACWindows();
                }
                this.shouldSubClass = subclass;
                System.Windows.Forms.UnsafeNativeMethods.EnumThreadWindows(System.Windows.Forms.SafeNativeMethods.GetCurrentThreadId(), new System.Windows.Forms.NativeMethods.EnumThreadWindowsCallback(this.Callback), new HandleRef(null, IntPtr.Zero));
            }

            private static string GetClassName(HandleRef hRef)
            {
                StringBuilder lpClassName = new StringBuilder(0x100);
                System.Windows.Forms.UnsafeNativeMethods.GetClassName(hRef, lpClassName, 0x100);
                return lpClassName.ToString();
            }
        }

        [ComVisible(true)]
        public class ChildAccessibleObject : AccessibleObject
        {
            private ComboBox owner;

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public ChildAccessibleObject(ComboBox owner, IntPtr handle)
            {
                this.owner = owner;
                base.UseStdAccessibleObjects(handle);
            }

            public override string Name
            {
                get
                {
                    return this.owner.AccessibilityObject.Name;
                }
            }
        }

        [ComVisible(true)]
        internal class ComboBoxAccessibleObject : Control.ControlAccessibleObject
        {
            private const int COMBOBOX_ACC_ITEM_INDEX = 1;

            public ComboBoxAccessibleObject(Control ownerControl) : base(ownerControl)
            {
            }

            internal override string get_accKeyboardShortcutInternal(object childID)
            {
                base.ValidateChildID(ref childID);
                if ((childID != null) && (((int) childID) == 1))
                {
                    return this.KeyboardShortcut;
                }
                return base.get_accKeyboardShortcutInternal(childID);
            }

            internal override string get_accNameInternal(object childID)
            {
                base.ValidateChildID(ref childID);
                if ((childID != null) && (((int) childID) == 1))
                {
                    return this.Name;
                }
                return base.get_accNameInternal(childID);
            }
        }

        private class ComboBoxChildNativeWindow : NativeWindow
        {
            private InternalAccessibleObject _accessibilityObject;
            private ComboBox _owner;

            internal ComboBoxChildNativeWindow(ComboBox comboBox)
            {
                this._owner = comboBox;
            }

            private void WmGetObject(ref Message m)
            {
                if (-4 == ((int) ((long) m.LParam)))
                {
                    Guid refiid = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");
                    try
                    {
                        AccessibleObject accessibleImplemention = null;
                        if (this._accessibilityObject == null)
                        {
                            System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                            try
                            {
                                accessibleImplemention = new ComboBox.ChildAccessibleObject(this._owner, base.Handle);
                                this._accessibilityObject = new InternalAccessibleObject(accessibleImplemention);
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                            }
                        }
                        IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(this._accessibilityObject);
                        System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                        try
                        {
                            m.Result = System.Windows.Forms.UnsafeNativeMethods.LresultFromObject(ref refiid, m.WParam, new HandleRef(this, iUnknownForObject));
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                            Marshal.Release(iUnknownForObject);
                        }
                        return;
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("RichControlLresult"), exception);
                    }
                }
                base.DefWndProc(ref m);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x3d)
                {
                    this.WmGetObject(ref m);
                }
                else
                {
                    this._owner.ChildWndProc(ref m);
                }
            }
        }

        internal class FlatComboAdapter
        {
            private Rectangle clientRect;
            internal Rectangle dropDownRect;
            private Rectangle innerBorder;
            private Rectangle innerInnerBorder;
            private RightToLeft origRightToLeft;
            private Rectangle outerBorder;
            private Rectangle whiteFillRect;
            private const int WhiteFillRectWidth = 5;

            public FlatComboAdapter(ComboBox comboBox, bool smallButton)
            {
                this.clientRect = comboBox.ClientRectangle;
                int horizontalScrollBarArrowWidth = SystemInformation.HorizontalScrollBarArrowWidth;
                this.outerBorder = new Rectangle(this.clientRect.Location, new Size(this.clientRect.Width - 1, this.clientRect.Height - 1));
                this.innerBorder = new Rectangle(this.outerBorder.X + 1, this.outerBorder.Y + 1, (this.outerBorder.Width - horizontalScrollBarArrowWidth) - 2, this.outerBorder.Height - 2);
                this.innerInnerBorder = new Rectangle(this.innerBorder.X + 1, this.innerBorder.Y + 1, this.innerBorder.Width - 2, this.innerBorder.Height - 2);
                this.dropDownRect = new Rectangle(this.innerBorder.Right + 1, this.innerBorder.Y, horizontalScrollBarArrowWidth, this.innerBorder.Height + 1);
                if (smallButton)
                {
                    this.whiteFillRect = this.dropDownRect;
                    this.whiteFillRect.Width = 5;
                    this.dropDownRect.X += 5;
                    this.dropDownRect.Width -= 5;
                }
                this.origRightToLeft = comboBox.RightToLeft;
                if (this.origRightToLeft == RightToLeft.Yes)
                {
                    this.innerBorder.X = this.clientRect.Width - this.innerBorder.Right;
                    this.innerInnerBorder.X = this.clientRect.Width - this.innerInnerBorder.Right;
                    this.dropDownRect.X = this.clientRect.Width - this.dropDownRect.Right;
                    this.whiteFillRect.X = (this.clientRect.Width - this.whiteFillRect.Right) + 1;
                }
            }

            public virtual void DrawFlatCombo(ComboBox comboBox, Graphics g)
            {
                if (comboBox.DropDownStyle != ComboBoxStyle.Simple)
                {
                    Color outerBorderColor = this.GetOuterBorderColor(comboBox);
                    Color innerBorderColor = this.GetInnerBorderColor(comboBox);
                    bool flag = comboBox.RightToLeft == RightToLeft.Yes;
                    this.DrawFlatComboDropDown(comboBox, g, this.dropDownRect);
                    if (!LayoutUtils.IsZeroWidthOrHeight(this.whiteFillRect))
                    {
                        using (Brush brush = new SolidBrush(innerBorderColor))
                        {
                            g.FillRectangle(brush, this.whiteFillRect);
                        }
                    }
                    if (outerBorderColor.IsSystemColor)
                    {
                        Pen pen = SystemPens.FromSystemColor(outerBorderColor);
                        g.DrawRectangle(pen, this.outerBorder);
                        if (flag)
                        {
                            g.DrawRectangle(pen, new Rectangle(this.outerBorder.X, this.outerBorder.Y, this.dropDownRect.Width + 1, this.outerBorder.Height));
                        }
                        else
                        {
                            g.DrawRectangle(pen, new Rectangle(this.dropDownRect.X, this.outerBorder.Y, this.outerBorder.Right - this.dropDownRect.X, this.outerBorder.Height));
                        }
                    }
                    else
                    {
                        using (Pen pen2 = new Pen(outerBorderColor))
                        {
                            g.DrawRectangle(pen2, this.outerBorder);
                            if (flag)
                            {
                                g.DrawRectangle(pen2, new Rectangle(this.outerBorder.X, this.outerBorder.Y, this.dropDownRect.Width + 1, this.outerBorder.Height));
                            }
                            else
                            {
                                g.DrawRectangle(pen2, new Rectangle(this.dropDownRect.X, this.outerBorder.Y, this.outerBorder.Right - this.dropDownRect.X, this.outerBorder.Height));
                            }
                        }
                    }
                    if (innerBorderColor.IsSystemColor)
                    {
                        Pen pen3 = SystemPens.FromSystemColor(innerBorderColor);
                        g.DrawRectangle(pen3, this.innerBorder);
                        g.DrawRectangle(pen3, this.innerInnerBorder);
                    }
                    else
                    {
                        using (Pen pen4 = new Pen(innerBorderColor))
                        {
                            g.DrawRectangle(pen4, this.innerBorder);
                            g.DrawRectangle(pen4, this.innerInnerBorder);
                        }
                    }
                    if (!comboBox.Enabled || (comboBox.FlatStyle == FlatStyle.Popup))
                    {
                        bool focused = comboBox.ContainsFocus || comboBox.MouseIsOver;
                        using (Pen pen5 = new Pen(this.GetPopupOuterBorderColor(comboBox, focused)))
                        {
                            Pen pen6 = comboBox.Enabled ? pen5 : SystemPens.Control;
                            if (flag)
                            {
                                g.DrawRectangle(pen6, new Rectangle(this.outerBorder.X, this.outerBorder.Y, this.dropDownRect.Width + 1, this.outerBorder.Height));
                            }
                            else
                            {
                                g.DrawRectangle(pen6, new Rectangle(this.dropDownRect.X, this.outerBorder.Y, this.outerBorder.Right - this.dropDownRect.X, this.outerBorder.Height));
                            }
                            g.DrawRectangle(pen5, this.outerBorder);
                        }
                    }
                }
            }

            protected virtual void DrawFlatComboDropDown(ComboBox comboBox, Graphics g, Rectangle dropDownRect)
            {
                g.FillRectangle(SystemBrushes.Control, dropDownRect);
                Brush brush = comboBox.Enabled ? SystemBrushes.ControlText : SystemBrushes.ControlDark;
                Point point = new Point(dropDownRect.Left + (dropDownRect.Width / 2), dropDownRect.Top + (dropDownRect.Height / 2));
                if (this.origRightToLeft == RightToLeft.Yes)
                {
                    point.X -= dropDownRect.Width % 2;
                }
                else
                {
                    point.X += dropDownRect.Width % 2;
                }
                Point[] points = new Point[] { new Point(point.X - 2, point.Y - 1), new Point(point.X + 3, point.Y - 1), new Point(point.X, point.Y + 2) };
                g.FillPolygon(brush, points);
            }

            protected virtual Color GetInnerBorderColor(ComboBox comboBox)
            {
                if (!comboBox.Enabled)
                {
                    return SystemColors.Control;
                }
                return comboBox.BackColor;
            }

            protected virtual Color GetOuterBorderColor(ComboBox comboBox)
            {
                if (!comboBox.Enabled)
                {
                    return SystemColors.ControlDark;
                }
                return SystemColors.Window;
            }

            protected virtual Color GetPopupOuterBorderColor(ComboBox comboBox, bool focused)
            {
                if (comboBox.Enabled && !focused)
                {
                    return SystemColors.Window;
                }
                return SystemColors.ControlDark;
            }

            public bool IsValid(ComboBox combo)
            {
                return ((combo.ClientRectangle == this.clientRect) && (combo.RightToLeft == this.origRightToLeft));
            }

            public void ValidateOwnerDrawRegions(ComboBox comboBox, Rectangle updateRegionBox)
            {
                if (comboBox == null)
                {
                    System.Windows.Forms.NativeMethods.RECT rect;
                    Rectangle r = new Rectangle(0, 0, comboBox.Width, this.innerBorder.Top);
                    Rectangle rectangle2 = new Rectangle(0, this.innerBorder.Bottom, comboBox.Width, comboBox.Height - this.innerBorder.Bottom);
                    Rectangle rectangle3 = new Rectangle(0, 0, this.innerBorder.Left, comboBox.Height);
                    Rectangle rectangle4 = new Rectangle(this.innerBorder.Right, 0, comboBox.Width - this.innerBorder.Right, comboBox.Height);
                    if (r.IntersectsWith(updateRegionBox))
                    {
                        rect = new System.Windows.Forms.NativeMethods.RECT(r);
                        System.Windows.Forms.SafeNativeMethods.ValidateRect(new HandleRef(comboBox, comboBox.Handle), ref rect);
                    }
                    if (rectangle2.IntersectsWith(updateRegionBox))
                    {
                        rect = new System.Windows.Forms.NativeMethods.RECT(rectangle2);
                        System.Windows.Forms.SafeNativeMethods.ValidateRect(new HandleRef(comboBox, comboBox.Handle), ref rect);
                    }
                    if (rectangle3.IntersectsWith(updateRegionBox))
                    {
                        rect = new System.Windows.Forms.NativeMethods.RECT(rectangle3);
                        System.Windows.Forms.SafeNativeMethods.ValidateRect(new HandleRef(comboBox, comboBox.Handle), ref rect);
                    }
                    if (rectangle4.IntersectsWith(updateRegionBox))
                    {
                        rect = new System.Windows.Forms.NativeMethods.RECT(rectangle4);
                        System.Windows.Forms.SafeNativeMethods.ValidateRect(new HandleRef(comboBox, comboBox.Handle), ref rect);
                    }
                }
            }
        }

        private sealed class ItemComparer : IComparer
        {
            private ComboBox comboBox;

            public ItemComparer(ComboBox comboBox)
            {
                this.comboBox = comboBox;
            }

            public int Compare(object item1, object item2)
            {
                if (item1 == null)
                {
                    if (item2 == null)
                    {
                        return 0;
                    }
                    return -1;
                }
                if (item2 == null)
                {
                    return 1;
                }
                string itemText = this.comboBox.GetItemText(item1);
                string str2 = this.comboBox.GetItemText(item2);
                return Application.CurrentCulture.CompareInfo.Compare(itemText, str2, CompareOptions.StringSort);
            }
        }

        [ListBindable(false)]
        public class ObjectCollection : IList, ICollection, IEnumerable
        {
            private IComparer comparer;
            private ArrayList innerList;
            private ComboBox owner;

            public ObjectCollection(ComboBox owner)
            {
                this.owner = owner;
            }

            public int Add(object item)
            {
                this.owner.CheckNoDataSource();
                int num = this.AddInternal(item);
                if (this.owner.UpdateNeeded() && (this.owner.AutoCompleteSource == AutoCompleteSource.ListItems))
                {
                    this.owner.SetAutoComplete(false, false);
                }
                return num;
            }

            private int AddInternal(object item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }
                int index = -1;
                if (!this.owner.sorted)
                {
                    this.InnerList.Add(item);
                }
                else
                {
                    index = this.InnerList.BinarySearch(item, this.Comparer);
                    if (index < 0)
                    {
                        index = ~index;
                    }
                    this.InnerList.Insert(index, item);
                }
                bool flag = false;
                try
                {
                    if (this.owner.sorted)
                    {
                        if (this.owner.IsHandleCreated)
                        {
                            this.owner.NativeInsert(index, item);
                        }
                    }
                    else
                    {
                        index = this.InnerList.Count - 1;
                        if (this.owner.IsHandleCreated)
                        {
                            this.owner.NativeAdd(item);
                        }
                    }
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        this.InnerList.Remove(item);
                    }
                }
                return index;
            }

            public void AddRange(object[] items)
            {
                this.owner.CheckNoDataSource();
                this.owner.BeginUpdate();
                try
                {
                    this.AddRangeInternal(items);
                }
                finally
                {
                    this.owner.EndUpdate();
                }
            }

            internal void AddRangeInternal(IList items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                foreach (object obj2 in items)
                {
                    this.AddInternal(obj2);
                }
                if (this.owner.AutoCompleteSource == AutoCompleteSource.ListItems)
                {
                    this.owner.SetAutoComplete(false, false);
                }
            }

            public void Clear()
            {
                this.owner.CheckNoDataSource();
                this.ClearInternal();
            }

            internal void ClearInternal()
            {
                if (this.owner.IsHandleCreated)
                {
                    this.owner.NativeClear();
                }
                this.InnerList.Clear();
                this.owner.selectedIndex = -1;
                if (this.owner.AutoCompleteSource == AutoCompleteSource.ListItems)
                {
                    this.owner.SetAutoComplete(false, true);
                }
            }

            public bool Contains(object value)
            {
                return (this.IndexOf(value) != -1);
            }

            public void CopyTo(object[] destination, int arrayIndex)
            {
                this.InnerList.CopyTo(destination, arrayIndex);
            }

            public IEnumerator GetEnumerator()
            {
                return this.InnerList.GetEnumerator();
            }

            public int IndexOf(object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                return this.InnerList.IndexOf(value);
            }

            public void Insert(int index, object item)
            {
                this.owner.CheckNoDataSource();
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }
                if ((index < 0) || (index > this.InnerList.Count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                if (this.owner.sorted)
                {
                    this.Add(item);
                }
                else
                {
                    this.InnerList.Insert(index, item);
                    if (this.owner.IsHandleCreated)
                    {
                        bool flag = false;
                        try
                        {
                            this.owner.NativeInsert(index, item);
                            flag = true;
                        }
                        finally
                        {
                            if (flag)
                            {
                                if (this.owner.AutoCompleteSource == AutoCompleteSource.ListItems)
                                {
                                    this.owner.SetAutoComplete(false, false);
                                }
                            }
                            else
                            {
                                this.InnerList.RemoveAt(index);
                            }
                        }
                    }
                }
            }

            public void Remove(object value)
            {
                int index = this.InnerList.IndexOf(value);
                if (index != -1)
                {
                    this.RemoveAt(index);
                }
            }

            public void RemoveAt(int index)
            {
                this.owner.CheckNoDataSource();
                if ((index < 0) || (index >= this.InnerList.Count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                if (this.owner.IsHandleCreated)
                {
                    this.owner.NativeRemoveAt(index);
                }
                this.InnerList.RemoveAt(index);
                if (!this.owner.IsHandleCreated && (index < this.owner.selectedIndex))
                {
                    this.owner.selectedIndex--;
                }
                if (this.owner.AutoCompleteSource == AutoCompleteSource.ListItems)
                {
                    this.owner.SetAutoComplete(false, false);
                }
            }

            internal void SetItemInternal(int index, object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if ((index < 0) || (index >= this.InnerList.Count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                this.InnerList[index] = value;
                if (this.owner.IsHandleCreated)
                {
                    bool flag = index == this.owner.SelectedIndex;
                    if (string.Compare(this.owner.GetItemText(value), this.owner.NativeGetItemText(index), true, CultureInfo.CurrentCulture) != 0)
                    {
                        this.owner.NativeRemoveAt(index);
                        this.owner.NativeInsert(index, value);
                        if (flag)
                        {
                            this.owner.SelectedIndex = index;
                            this.owner.UpdateText();
                        }
                        if (this.owner.AutoCompleteSource == AutoCompleteSource.ListItems)
                        {
                            this.owner.SetAutoComplete(false, false);
                        }
                    }
                    else if (flag)
                    {
                        this.owner.OnSelectedItemChanged(EventArgs.Empty);
                        this.owner.OnSelectedIndexChanged(EventArgs.Empty);
                    }
                }
            }

            void ICollection.CopyTo(Array destination, int index)
            {
                this.InnerList.CopyTo(destination, index);
            }

            int IList.Add(object item)
            {
                return this.Add(item);
            }

            private IComparer Comparer
            {
                get
                {
                    if (this.comparer == null)
                    {
                        this.comparer = new ComboBox.ItemComparer(this.owner);
                    }
                    return this.comparer;
                }
            }

            public int Count
            {
                get
                {
                    return this.InnerList.Count;
                }
            }

            private ArrayList InnerList
            {
                get
                {
                    if (this.innerList == null)
                    {
                        this.innerList = new ArrayList();
                    }
                    return this.innerList;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
            public virtual object this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.InnerList.Count))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    return this.InnerList[index];
                }
                set
                {
                    this.owner.CheckNoDataSource();
                    this.SetItemInternal(index, value);
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

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }
        }
    }
}

