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
    using System.Windows.Forms.Layout;
    using System.Windows.Forms.VisualStyles;

    [DefaultEvent("SelectedIndexChanged"), Designer("System.Windows.Forms.Design.ListBoxDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ComVisible(true), System.Windows.Forms.SRDescription("DescriptionListBox"), DefaultBindingProperty("SelectedValue"), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultProperty("Items")]
    public class ListBox : ListControl
    {
        private System.Windows.Forms.BorderStyle borderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        private System.Windows.Forms.SelectionMode cachedSelectionMode = System.Windows.Forms.SelectionMode.One;
        private static bool checkedOS = false;
        private int columnWidth;
        private IntegerCollection customTabOffsets;
        public const int DefaultItemHeight = 13;
        private bool doubleClickFired;
        private System.Windows.Forms.DrawMode drawMode;
        private static readonly object EVENT_DRAWITEM = new object();
        private static readonly object EVENT_MEASUREITEM = new object();
        private static readonly object EVENT_SELECTEDINDEXCHANGED = new object();
        private bool fontIsChanged;
        private int horizontalExtent;
        private bool horizontalScrollbar;
        private bool integralHeight = true;
        private bool integralHeightAdjust;
        private int itemHeight = 13;
        private ObjectCollection itemsCollection;
        private int maxWidth = -1;
        private const int maxWin9xHeight = 0x7fff;
        private bool multiColumn;
        public const int NoMatches = -1;
        private int requestedHeight;
        private static bool runningOnWin2K = true;
        private bool scrollAlwaysVisible;
        private SelectedIndexCollection selectedIndices;
        private SelectedObjectCollection selectedItems;
        private bool selectedValueChangedFired;
        private System.Windows.Forms.SelectionMode selectionMode = System.Windows.Forms.SelectionMode.One;
        private bool selectionModeChanging;
        private bool sorted;
        private int topIndex;
        private int updateCount;
        private bool useCustomTabOffsets;
        private bool useTabStops = true;

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

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        public event EventHandler Click
        {
            add
            {
                base.Click += value;
            }
            remove
            {
                base.Click -= value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("drawItemEventDescr")]
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

        [System.Windows.Forms.SRDescription("measureItemEventDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event MeasureItemEventHandler MeasureItem
        {
            add
            {
                base.Events.AddHandler(EVENT_MEASUREITEM, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MEASUREITEM, value);
            }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public event MouseEventHandler MouseClick
        {
            add
            {
                base.MouseClick += value;
            }
            remove
            {
                base.MouseClick -= value;
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

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("selectedIndexChangedEventDescr")]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
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

        public ListBox()
        {
            base.SetStyle(ControlStyles.UseTextForAccessibility | ControlStyles.StandardClick | ControlStyles.UserPaint, false);
            base.SetState2(0x800, true);
            base.SetBounds(0, 0, 120, 0x60);
            this.requestedHeight = base.Height;
        }

        [Obsolete("This method has been deprecated.  There is no replacement.  http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual void AddItemsCore(object[] value)
        {
            if (((value == null) ? 0 : value.Length) != 0)
            {
                this.Items.AddRangeInternal(value);
            }
        }

        public void BeginUpdate()
        {
            base.BeginUpdateInternal();
            this.updateCount++;
        }

        private void CheckIndex(int index)
        {
            if ((index < 0) || (index >= this.Items.Count))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("IndexOutOfRange", new object[] { index.ToString(CultureInfo.CurrentCulture) }));
            }
        }

        private void CheckNoDataSource()
        {
            if (base.DataSource != null)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataSourceLocksItems"));
            }
        }

        public void ClearSelected()
        {
            bool flag = false;
            int num = (this.itemsCollection == null) ? 0 : this.itemsCollection.Count;
            for (int i = 0; i < num; i++)
            {
                if (this.SelectedItems.GetSelected(i))
                {
                    flag = true;
                    this.SelectedItems.SetSelected(i, false);
                    if (base.IsHandleCreated)
                    {
                        this.NativeSetSelected(i, false);
                    }
                }
            }
            if (flag)
            {
                this.OnSelectedIndexChanged(EventArgs.Empty);
            }
        }

        internal virtual int ComputeMaxItemWidth(int oldMax)
        {
            string[] objects = new string[this.Items.Count];
            for (int i = 0; i < this.Items.Count; i++)
            {
                objects[i] = base.GetItemText(this.Items[i]);
            }
            Size size = LayoutUtils.OldGetLargestStringSizeInCollection(this.Font, objects);
            return Math.Max(oldMax, size.Width);
        }

        protected virtual ObjectCollection CreateItemCollection()
        {
            return new ObjectCollection(this);
        }

        public void EndUpdate()
        {
            base.EndUpdateInternal();
            this.updateCount--;
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
            int num = (this.itemsCollection == null) ? 0 : this.itemsCollection.Count;
            if (num == 0)
            {
                return -1;
            }
            if ((startIndex < -1) || (startIndex >= num))
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            return base.FindStringInternal(s, this.Items, startIndex, false);
        }

        public int FindStringExact(string s)
        {
            return this.FindStringExact(s, -1);
        }

        public int FindStringExact(string s, int startIndex)
        {
            if (s == null)
            {
                return -1;
            }
            int num = (this.itemsCollection == null) ? 0 : this.itemsCollection.Count;
            if (num == 0)
            {
                return -1;
            }
            if ((startIndex < -1) || (startIndex >= num))
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            return base.FindStringInternal(s, this.Items, startIndex, true);
        }

        public int GetItemHeight(int index)
        {
            int num = (this.itemsCollection == null) ? 0 : this.itemsCollection.Count;
            if ((index < 0) || ((index > 0) && (index >= num)))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            if (this.drawMode != System.Windows.Forms.DrawMode.OwnerDrawVariable)
            {
                index = 0;
            }
            if (!base.IsHandleCreated)
            {
                return this.itemHeight;
            }
            int num2 = (int) ((long) base.SendMessage(0x1a1, index, 0));
            if (num2 == -1)
            {
                throw new Win32Exception();
            }
            return num2;
        }

        public Rectangle GetItemRectangle(int index)
        {
            this.CheckIndex(index);
            System.Windows.Forms.NativeMethods.RECT lparam = new System.Windows.Forms.NativeMethods.RECT();
            base.SendMessage(0x198, index, ref lparam);
            return Rectangle.FromLTRB(lparam.left, lparam.top, lparam.right, lparam.bottom);
        }

        internal override Size GetPreferredSizeCore(Size proposedConstraints)
        {
            int preferredHeight = this.PreferredHeight;
            if (base.IsHandleCreated)
            {
                int width = this.SizeFromClientSize(new Size(this.MaxItemWidth, preferredHeight)).Width + (SystemInformation.VerticalScrollBarWidth + 4);
                return (new Size(width, preferredHeight) + this.Padding.Size);
            }
            return this.DefaultSize;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, BoundsSpecified specified)
        {
            bounds.Height = this.requestedHeight;
            return base.GetScaledBounds(bounds, factor, specified);
        }

        public bool GetSelected(int index)
        {
            this.CheckIndex(index);
            return this.GetSelectedInternal(index);
        }

        private bool GetSelectedInternal(int index)
        {
            if (base.IsHandleCreated)
            {
                int num = (int) ((long) base.SendMessage(0x187, index, 0));
                if (num == -1)
                {
                    throw new Win32Exception();
                }
                return (num > 0);
            }
            return ((this.itemsCollection != null) && this.SelectedItems.GetSelected(index));
        }

        public int IndexFromPoint(Point p)
        {
            return this.IndexFromPoint(p.X, p.Y);
        }

        public int IndexFromPoint(int x, int y)
        {
            System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
            System.Windows.Forms.UnsafeNativeMethods.GetClientRect(new HandleRef(this, base.Handle), ref rect);
            if (((rect.left <= x) && (x < rect.right)) && ((rect.top <= y) && (y < rect.bottom)))
            {
                int n = (int) ((long) base.SendMessage(0x1a9, 0, (int) ((long) System.Windows.Forms.NativeMethods.Util.MAKELPARAM(x, y))));
                if (System.Windows.Forms.NativeMethods.Util.HIWORD(n) == 0)
                {
                    return System.Windows.Forms.NativeMethods.Util.LOWORD(n);
                }
            }
            return -1;
        }

        private int NativeAdd(object item)
        {
            int num = (int) ((long) base.SendMessage(0x180, 0, base.GetItemText(item)));
            switch (num)
            {
                case -2:
                    throw new OutOfMemoryException();

                case -1:
                    throw new OutOfMemoryException(System.Windows.Forms.SR.GetString("ListBoxItemOverflow"));
            }
            return num;
        }

        private void NativeClear()
        {
            base.SendMessage(0x184, 0, 0);
        }

        internal string NativeGetItemText(int index)
        {
            int num = (int) ((long) base.SendMessage(0x18a, index, 0));
            StringBuilder lParam = new StringBuilder(num + 1);
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x189, index, lParam);
            return lParam.ToString();
        }

        private int NativeInsert(int index, object item)
        {
            int num = (int) ((long) base.SendMessage(0x181, index, base.GetItemText(item)));
            switch (num)
            {
                case -2:
                    throw new OutOfMemoryException();

                case -1:
                    throw new OutOfMemoryException(System.Windows.Forms.SR.GetString("ListBoxItemOverflow"));
            }
            return num;
        }

        private void NativeRemoveAt(int index)
        {
            bool flag = ((int) ((long) base.SendMessage(0x187, (IntPtr) index, IntPtr.Zero))) > 0;
            base.SendMessage(0x182, index, 0);
            if (flag)
            {
                this.OnSelectedIndexChanged(EventArgs.Empty);
            }
        }

        private void NativeSetSelected(int index, bool value)
        {
            if (this.selectionMode == System.Windows.Forms.SelectionMode.One)
            {
                base.SendMessage(390, value ? index : -1, 0);
            }
            else
            {
                base.SendMessage(0x185, value ? -1 : 0, index);
            }
        }

        private void NativeUpdateSelection()
        {
            int count = this.Items.Count;
            for (int i = 0; i < count; i++)
            {
                this.SelectedItems.SetSelected(i, false);
            }
            int[] lParam = null;
            switch (this.selectionMode)
            {
                case System.Windows.Forms.SelectionMode.One:
                {
                    int num3 = (int) ((long) base.SendMessage(0x188, 0, 0));
                    if (num3 >= 0)
                    {
                        lParam = new int[] { num3 };
                    }
                    break;
                }
                case System.Windows.Forms.SelectionMode.MultiSimple:
                case System.Windows.Forms.SelectionMode.MultiExtended:
                {
                    int wParam = (int) ((long) base.SendMessage(400, 0, 0));
                    if (wParam > 0)
                    {
                        lParam = new int[wParam];
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x191, wParam, lParam);
                    }
                    break;
                }
            }
            if (lParam != null)
            {
                foreach (int num5 in lParam)
                {
                    this.SelectedItems.SetSelected(num5, true);
                }
            }
        }

        protected override void OnChangeUICues(UICuesEventArgs e)
        {
            base.Invalidate();
            base.OnChangeUICues(e);
        }

        protected override void OnDataSourceChanged(EventArgs e)
        {
            if (base.DataSource == null)
            {
                this.BeginUpdate();
                this.SelectedIndex = -1;
                this.Items.ClearInternal();
                this.EndUpdate();
            }
            base.OnDataSourceChanged(e);
            this.RefreshItems();
        }

        protected override void OnDisplayMemberChanged(EventArgs e)
        {
            base.OnDisplayMemberChanged(e);
            this.RefreshItems();
            if ((this.SelectionMode != System.Windows.Forms.SelectionMode.None) && (base.DataManager != null))
            {
                this.SelectedIndex = base.DataManager.Position;
            }
        }

        protected virtual void OnDrawItem(DrawItemEventArgs e)
        {
            DrawItemEventHandler handler = (DrawItemEventHandler) base.Events[EVENT_DRAWITEM];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.UpdateFontCache();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            base.SendMessage(0x1a5, CultureInfo.CurrentCulture.LCID, 0);
            if (this.columnWidth != 0)
            {
                base.SendMessage(0x195, this.columnWidth, 0);
            }
            if (this.drawMode == System.Windows.Forms.DrawMode.OwnerDrawFixed)
            {
                base.SendMessage(0x1a0, 0, this.ItemHeight);
            }
            if (this.topIndex != 0)
            {
                base.SendMessage(0x197, this.topIndex, 0);
            }
            if (this.UseCustomTabOffsets && (this.CustomTabOffsets != null))
            {
                int count = this.CustomTabOffsets.Count;
                int[] destination = new int[count];
                this.CustomTabOffsets.CopyTo(destination, 0);
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x192, count, destination);
            }
            if (this.itemsCollection != null)
            {
                int num2 = this.itemsCollection.Count;
                for (int i = 0; i < num2; i++)
                {
                    this.NativeAdd(this.itemsCollection[i]);
                    if ((this.selectionMode != System.Windows.Forms.SelectionMode.None) && (this.selectedItems != null))
                    {
                        this.selectedItems.PushSelectionIntoNativeListBox(i);
                    }
                }
            }
            if (((this.selectedItems != null) && (this.selectedItems.Count > 0)) && (this.selectionMode == System.Windows.Forms.SelectionMode.One))
            {
                this.SelectedItems.Dirty();
                this.SelectedItems.EnsureUpToDate();
            }
            this.UpdateHorizontalExtent();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            this.SelectedItems.EnsureUpToDate();
            if (base.Disposing)
            {
                this.itemsCollection = null;
            }
            base.OnHandleDestroyed(e);
        }

        protected virtual void OnMeasureItem(MeasureItemEventArgs e)
        {
            MeasureItemEventHandler handler = (MeasureItemEventHandler) base.Events[EVENT_MEASUREITEM];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (this.ParentInternal != null)
            {
                base.RecreateHandle();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if ((this.RightToLeft == System.Windows.Forms.RightToLeft.Yes) || this.HorizontalScrollbar)
            {
                base.Invalidate();
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            if (((base.DataManager != null) && (base.DataManager.Position != this.SelectedIndex)) && (!base.FormattingEnabled || (this.SelectedIndex != -1)))
            {
                base.DataManager.Position = this.SelectedIndex;
            }
            EventHandler handler = (EventHandler) base.Events[EVENT_SELECTEDINDEXCHANGED];
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

        public override void Refresh()
        {
            if (this.drawMode == System.Windows.Forms.DrawMode.OwnerDrawVariable)
            {
                int count = this.Items.Count;
                using (Graphics graphics = base.CreateGraphicsInternal())
                {
                    for (int i = 0; i < count; i++)
                    {
                        MeasureItemEventArgs e = new MeasureItemEventArgs(graphics, i, this.ItemHeight);
                        this.OnMeasureItem(e);
                    }
                }
            }
            base.Refresh();
        }

        protected override void RefreshItem(int index)
        {
            this.Items.SetItemInternal(index, this.Items[index]);
        }

        protected override void RefreshItems()
        {
            ObjectCollection itemsCollection = this.itemsCollection;
            this.itemsCollection = null;
            this.selectedIndices = null;
            if (base.IsHandleCreated)
            {
                this.NativeClear();
            }
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
            if (destination != null)
            {
                this.Items.AddRangeInternal(destination);
            }
            if (this.SelectionMode != System.Windows.Forms.SelectionMode.None)
            {
                if (base.DataManager != null)
                {
                    this.SelectedIndex = base.DataManager.Position;
                }
                else if (itemsCollection != null)
                {
                    int count = itemsCollection.Count;
                    for (int j = 0; j < count; j++)
                    {
                        if (itemsCollection.InnerArray.GetState(j, SelectedObjectCollection.SelectedObjectMask))
                        {
                            this.SelectedItem = itemsCollection[j];
                        }
                    }
                }
            }
        }

        public override void ResetBackColor()
        {
            base.ResetBackColor();
        }

        public override void ResetForeColor()
        {
            base.ResetForeColor();
        }

        private void ResetItemHeight()
        {
            this.itemHeight = 13;
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            if ((factor.Width != 1f) && (factor.Height != 1f))
            {
                this.UpdateFontCache();
            }
            base.ScaleControl(factor, specified);
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (!this.integralHeightAdjust && (height != base.Height))
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
            this.SelectedItems.Dirty();
            if (base.DataManager != null)
            {
                if (base.DataSource is ICurrencyManagerProvider)
                {
                    this.selectedValueChangedFired = false;
                }
                if (base.IsHandleCreated)
                {
                    base.SendMessage(390, base.DataManager.Position, 0);
                }
                if (!this.selectedValueChangedFired)
                {
                    this.OnSelectedValueChanged(EventArgs.Empty);
                    this.selectedValueChangedFired = false;
                }
            }
            this.EndUpdate();
        }

        public void SetSelected(int index, bool value)
        {
            int num = (this.itemsCollection == null) ? 0 : this.itemsCollection.Count;
            if ((index < 0) || (index >= num))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            if (this.selectionMode == System.Windows.Forms.SelectionMode.None)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListBoxInvalidSelectionMode"));
            }
            this.SelectedItems.SetSelected(index, value);
            if (base.IsHandleCreated)
            {
                this.NativeSetSelected(index, value);
            }
            this.SelectedItems.Dirty();
            this.OnSelectedIndexChanged(EventArgs.Empty);
        }

        protected virtual void Sort()
        {
            this.CheckNoDataSource();
            SelectedObjectCollection selectedItems = this.SelectedItems;
            selectedItems.EnsureUpToDate();
            if (this.sorted && (this.itemsCollection != null))
            {
                this.itemsCollection.InnerArray.Sort();
                if (base.IsHandleCreated)
                {
                    this.NativeClear();
                    int count = this.itemsCollection.Count;
                    for (int i = 0; i < count; i++)
                    {
                        this.NativeAdd(this.itemsCollection[i]);
                        if (selectedItems.GetSelected(i))
                        {
                            this.NativeSetSelected(i, true);
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            string str = base.ToString();
            if (this.itemsCollection != null)
            {
                str = str + ", Items.Count: " + this.Items.Count.ToString(CultureInfo.CurrentCulture);
                if (this.Items.Count > 0)
                {
                    string itemText = base.GetItemText(this.Items[0]);
                    string str3 = (itemText.Length > 40) ? itemText.Substring(0, 40) : itemText;
                    str = str + ", Items[0]: " + str3;
                }
            }
            return str;
        }

        private void UpdateCustomTabOffsets()
        {
            if ((base.IsHandleCreated && this.UseCustomTabOffsets) && (this.CustomTabOffsets != null))
            {
                int count = this.CustomTabOffsets.Count;
                int[] destination = new int[count];
                this.CustomTabOffsets.CopyTo(destination, 0);
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x192, count, destination);
                base.Invalidate();
            }
        }

        private void UpdateFontCache()
        {
            this.fontIsChanged = true;
            this.integralHeightAdjust = true;
            try
            {
                base.Height = this.requestedHeight;
            }
            finally
            {
                this.integralHeightAdjust = false;
            }
            this.maxWidth = -1;
            this.UpdateHorizontalExtent();
            CommonProperties.xClearPreferredSizeCache(this);
        }

        private void UpdateHorizontalExtent()
        {
            if ((!this.multiColumn && this.horizontalScrollbar) && base.IsHandleCreated)
            {
                int horizontalExtent = this.horizontalExtent;
                if (horizontalExtent == 0)
                {
                    horizontalExtent = this.MaxItemWidth;
                }
                base.SendMessage(0x194, horizontalExtent, 0);
            }
        }

        private void UpdateMaxItemWidth(object item, bool removing)
        {
            if (!this.horizontalScrollbar || (this.horizontalExtent > 0))
            {
                this.maxWidth = -1;
            }
            else if (this.maxWidth > -1)
            {
                int num;
                using (Graphics graphics = base.CreateGraphicsInternal())
                {
                    num = (int) Math.Ceiling((double) graphics.MeasureString(base.GetItemText(item), this.Font).Width);
                }
                if (removing)
                {
                    if (num >= this.maxWidth)
                    {
                        this.maxWidth = -1;
                    }
                }
                else if (num > this.maxWidth)
                {
                    this.maxWidth = num;
                }
            }
        }

        private void WmPrint(ref Message m)
        {
            base.WndProc(ref m);
            if ((((2 & ((int) m.LParam)) != 0) && Application.RenderWithVisualStyles) && (this.BorderStyle == System.Windows.Forms.BorderStyle.Fixed3D))
            {
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    using (Graphics graphics = Graphics.FromHdc(m.WParam))
                    {
                        Rectangle rect = new Rectangle(0, 0, base.Size.Width - 1, base.Size.Height - 1);
                        using (Pen pen = new Pen(VisualStyleInformation.TextControlBorder))
                        {
                            graphics.DrawRectangle(pen, rect);
                        }
                        rect.Inflate(-1, -1);
                        graphics.DrawRectangle(SystemPens.Window, rect);
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected virtual void WmReflectCommand(ref Message m)
        {
            switch (System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam))
            {
                case 1:
                    if (this.selectedItems != null)
                    {
                        this.selectedItems.Dirty();
                    }
                    this.OnSelectedIndexChanged(EventArgs.Empty);
                    break;

                case 2:
                    break;

                default:
                    return;
            }
        }

        private void WmReflectDrawItem(ref Message m)
        {
            System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT lParam = (System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT));
            IntPtr hDC = lParam.hDC;
            IntPtr handle = Control.SetUpPalette(hDC, false, false);
            try
            {
                using (Graphics graphics = Graphics.FromHdcInternal(hDC))
                {
                    Rectangle rect = Rectangle.FromLTRB(lParam.rcItem.left, lParam.rcItem.top, lParam.rcItem.right, lParam.rcItem.bottom);
                    if (this.HorizontalScrollbar)
                    {
                        if (this.MultiColumn)
                        {
                            rect.Width = Math.Max(this.ColumnWidth, rect.Width);
                        }
                        else
                        {
                            rect.Width = Math.Max(this.MaxItemWidth, rect.Width);
                        }
                    }
                    this.OnDrawItem(new DrawItemEventArgs(graphics, this.Font, rect, lParam.itemID, (DrawItemState) lParam.itemState, this.ForeColor, this.BackColor));
                }
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    System.Windows.Forms.SafeNativeMethods.SelectPalette(new HandleRef(null, hDC), new HandleRef(null, handle), 0);
                }
            }
            m.Result = (IntPtr) 1;
        }

        private void WmReflectMeasureItem(ref Message m)
        {
            System.Windows.Forms.NativeMethods.MEASUREITEMSTRUCT lParam = (System.Windows.Forms.NativeMethods.MEASUREITEMSTRUCT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.MEASUREITEMSTRUCT));
            if ((this.drawMode == System.Windows.Forms.DrawMode.OwnerDrawVariable) && (lParam.itemID >= 0))
            {
                Graphics graphics = base.CreateGraphicsInternal();
                MeasureItemEventArgs e = new MeasureItemEventArgs(graphics, lParam.itemID, this.ItemHeight);
                try
                {
                    this.OnMeasureItem(e);
                    lParam.itemHeight = e.ItemHeight;
                }
                finally
                {
                    graphics.Dispose();
                }
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
            int msg = m.Msg;
            if (msg <= 0x205)
            {
                switch (msg)
                {
                    case 0x201:
                        if (this.selectedItems != null)
                        {
                            this.selectedItems.Dirty();
                        }
                        base.WndProc(ref m);
                        return;

                    case 0x202:
                    {
                        int x = System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam);
                        int y = System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam);
                        Point p = new Point(x, y);
                        p = base.PointToScreen(p);
                        if (base.Capture && (System.Windows.Forms.UnsafeNativeMethods.WindowFromPoint(p.X, p.Y) == base.Handle))
                        {
                            if (this.doubleClickFired || base.ValidationCancelled)
                            {
                                this.doubleClickFired = false;
                                if (!base.ValidationCancelled)
                                {
                                    this.OnDoubleClick(new MouseEventArgs(MouseButtons.Left, 2, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                                    this.OnMouseDoubleClick(new MouseEventArgs(MouseButtons.Left, 2, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                                }
                            }
                            else
                            {
                                this.OnClick(new MouseEventArgs(MouseButtons.Left, 1, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                                this.OnMouseClick(new MouseEventArgs(MouseButtons.Left, 1, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                            }
                        }
                        goto Label_01BF;
                    }
                    case 0x203:
                        this.doubleClickFired = true;
                        base.WndProc(ref m);
                        return;

                    case 0x205:
                    {
                        int num3 = System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam);
                        int num4 = System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam);
                        Point point2 = new Point(num3, num4);
                        point2 = base.PointToScreen(point2);
                        if ((base.Capture && (System.Windows.Forms.UnsafeNativeMethods.WindowFromPoint(point2.X, point2.Y) == base.Handle)) && (this.selectedItems != null))
                        {
                            this.selectedItems.Dirty();
                        }
                        base.WndProc(ref m);
                        return;
                    }
                    case 0x47:
                        base.WndProc(ref m);
                        if (this.integralHeight && this.fontIsChanged)
                        {
                            base.Height = Math.Max(base.Height, this.ItemHeight);
                            this.fontIsChanged = false;
                        }
                        return;
                }
                goto Label_029F;
            }
            switch (msg)
            {
                case 0x202b:
                    this.WmReflectDrawItem(ref m);
                    return;

                case 0x202c:
                    this.WmReflectMeasureItem(ref m);
                    return;

                case 0x317:
                    this.WmPrint(ref m);
                    return;

                default:
                    if (msg != 0x2111)
                    {
                        goto Label_029F;
                    }
                    this.WmReflectCommand(ref m);
                    return;
            }
        Label_01BF:
            if (base.GetState(0x800))
            {
                base.DefWndProc(ref m);
            }
            else
            {
                base.WndProc(ref m);
            }
            this.doubleClickFired = false;
            return;
        Label_029F:
            base.WndProc(ref m);
        }

        protected override bool AllowSelection
        {
            get
            {
                return (this.selectionMode != System.Windows.Forms.SelectionMode.None);
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

        [DefaultValue(2), System.Windows.Forms.SRDescription("ListBoxBorderDescr"), DispId(-504), System.Windows.Forms.SRCategory("CatAppearance")]
        public System.Windows.Forms.BorderStyle BorderStyle
        {
            get
            {
                return this.borderStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.BorderStyle));
                }
                if (value != this.borderStyle)
                {
                    this.borderStyle = value;
                    base.RecreateHandle();
                    this.integralHeightAdjust = true;
                    try
                    {
                        base.Height = this.requestedHeight;
                    }
                    finally
                    {
                        this.integralHeightAdjust = false;
                    }
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("ListBoxColumnWidthDescr"), DefaultValue(0), System.Windows.Forms.SRCategory("CatBehavior")]
        public int ColumnWidth
        {
            get
            {
                return this.columnWidth;
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "value", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                if (this.columnWidth != value)
                {
                    this.columnWidth = value;
                    if (this.columnWidth == 0)
                    {
                        base.RecreateHandle();
                    }
                    else if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x195, this.columnWidth, 0);
                    }
                }
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ClassName = "LISTBOX";
                createParams.Style |= 0x200041;
                if (this.scrollAlwaysVisible)
                {
                    createParams.Style |= 0x1000;
                }
                if (!this.integralHeight)
                {
                    createParams.Style |= 0x100;
                }
                if (this.useTabStops)
                {
                    createParams.Style |= 0x80;
                }
                switch (this.borderStyle)
                {
                    case System.Windows.Forms.BorderStyle.FixedSingle:
                        createParams.Style |= 0x800000;
                        break;

                    case System.Windows.Forms.BorderStyle.Fixed3D:
                        createParams.ExStyle |= 0x200;
                        break;
                }
                if (this.multiColumn)
                {
                    createParams.Style |= 0x100200;
                }
                else if (this.horizontalScrollbar)
                {
                    createParams.Style |= 0x100000;
                }
                switch (this.selectionMode)
                {
                    case System.Windows.Forms.SelectionMode.None:
                        createParams.Style |= 0x4000;
                        break;

                    case System.Windows.Forms.SelectionMode.MultiSimple:
                        createParams.Style |= 8;
                        break;

                    case System.Windows.Forms.SelectionMode.MultiExtended:
                        createParams.Style |= 0x800;
                        break;
                }
                switch (this.drawMode)
                {
                    case System.Windows.Forms.DrawMode.Normal:
                        return createParams;

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

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ListBoxCustomTabOffsetsDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Browsable(false)]
        public IntegerCollection CustomTabOffsets
        {
            get
            {
                if (this.customTabOffsets == null)
                {
                    this.customTabOffsets = new IntegerCollection(this);
                }
                return this.customTabOffsets;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(120, 0x60);
            }
        }

        [System.Windows.Forms.SRDescription("ListBoxDrawModeDescr"), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(0)]
        public virtual System.Windows.Forms.DrawMode DrawMode
        {
            get
            {
                return this.drawMode;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.DrawMode));
                }
                if (this.drawMode != value)
                {
                    if (this.MultiColumn && (value == System.Windows.Forms.DrawMode.OwnerDrawVariable))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("ListBoxVarHeightMultiCol"), "value");
                    }
                    this.drawMode = value;
                    base.RecreateHandle();
                    if (this.drawMode == System.Windows.Forms.DrawMode.OwnerDrawVariable)
                    {
                        LayoutTransaction.DoLayoutIf(this.AutoSize, this.ParentInternal, this, PropertyNames.DrawMode);
                    }
                }
            }
        }

        internal int FocusedIndex
        {
            get
            {
                if (base.IsHandleCreated)
                {
                    return (int) ((long) base.SendMessage(0x19f, 0, 0));
                }
                return -1;
            }
        }

        public override System.Drawing.Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                if (!this.integralHeight)
                {
                    this.RefreshItems();
                }
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

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ListBoxHorizontalExtentDescr"), DefaultValue(0), Localizable(true)]
        public int HorizontalExtent
        {
            get
            {
                return this.horizontalExtent;
            }
            set
            {
                if (value != this.horizontalExtent)
                {
                    this.horizontalExtent = value;
                    this.UpdateHorizontalExtent();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ListBoxHorizontalScrollbarDescr"), DefaultValue(false), Localizable(true)]
        public bool HorizontalScrollbar
        {
            get
            {
                return this.horizontalScrollbar;
            }
            set
            {
                if (value != this.horizontalScrollbar)
                {
                    this.horizontalScrollbar = value;
                    this.RefreshItems();
                    if (!this.MultiColumn)
                    {
                        base.RecreateHandle();
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true), DefaultValue(true), System.Windows.Forms.SRDescription("ListBoxIntegralHeightDescr"), RefreshProperties(RefreshProperties.Repaint)]
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
                    this.integralHeightAdjust = true;
                    try
                    {
                        base.Height = this.requestedHeight;
                    }
                    finally
                    {
                        this.integralHeightAdjust = false;
                    }
                }
            }
        }

        [RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRDescription("ListBoxItemHeightDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(13)]
        public virtual int ItemHeight
        {
            get
            {
                if ((this.drawMode != System.Windows.Forms.DrawMode.OwnerDrawFixed) && (this.drawMode != System.Windows.Forms.DrawMode.OwnerDrawVariable))
                {
                    return this.GetItemHeight(0);
                }
                return this.itemHeight;
            }
            set
            {
                if ((value < 1) || (value > 0xff))
                {
                    object[] args = new object[] { "ItemHeight", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture), "256" };
                    throw new ArgumentOutOfRangeException("ItemHeight", System.Windows.Forms.SR.GetString("InvalidExBoundArgument", args));
                }
                if (this.itemHeight != value)
                {
                    this.itemHeight = value;
                    if ((this.drawMode == System.Windows.Forms.DrawMode.OwnerDrawFixed) && base.IsHandleCreated)
                    {
                        this.BeginUpdate();
                        base.SendMessage(0x1a0, 0, value);
                        if (this.IntegralHeight)
                        {
                            Size size = base.Size;
                            base.Size = new Size(size.Width + 1, size.Height);
                            base.Size = size;
                        }
                        this.EndUpdate();
                    }
                }
            }
        }

        [MergableProperty(false), System.Windows.Forms.SRDescription("ListBoxItemsDescr"), Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), System.Windows.Forms.SRCategory("CatData"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Localizable(true)]
        public ObjectCollection Items
        {
            get
            {
                if (this.itemsCollection == null)
                {
                    this.itemsCollection = this.CreateItemCollection();
                }
                return this.itemsCollection;
            }
        }

        internal virtual int MaxItemWidth
        {
            get
            {
                if (this.horizontalExtent > 0)
                {
                    return this.horizontalExtent;
                }
                if (this.DrawMode != System.Windows.Forms.DrawMode.Normal)
                {
                    return -1;
                }
                if (this.maxWidth <= -1)
                {
                    this.maxWidth = this.ComputeMaxItemWidth(this.maxWidth);
                }
                return this.maxWidth;
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("ListBoxMultiColumnDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool MultiColumn
        {
            get
            {
                return this.multiColumn;
            }
            set
            {
                if (this.multiColumn != value)
                {
                    if (value && (this.drawMode == System.Windows.Forms.DrawMode.OwnerDrawVariable))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("ListBoxVarHeightMultiCol"), "value");
                    }
                    this.multiColumn = value;
                    base.RecreateHandle();
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ListBoxPreferredHeightDescr"), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public int PreferredHeight
        {
            get
            {
                int height = 0;
                if (this.drawMode == System.Windows.Forms.DrawMode.OwnerDrawVariable)
                {
                    if (base.RecreatingHandle || base.GetState(0x40000))
                    {
                        height = base.Height;
                    }
                    else if (this.itemsCollection != null)
                    {
                        int count = this.itemsCollection.Count;
                        for (int i = 0; i < count; i++)
                        {
                            height += this.GetItemHeight(i);
                        }
                    }
                }
                else
                {
                    int num4 = ((this.itemsCollection == null) || (this.itemsCollection.Count == 0)) ? 1 : this.itemsCollection.Count;
                    height = this.GetItemHeight(0) * num4;
                }
                if (this.borderStyle != System.Windows.Forms.BorderStyle.None)
                {
                    height += (SystemInformation.BorderSize.Height * 4) + 3;
                }
                return height;
            }
        }

        public override System.Windows.Forms.RightToLeft RightToLeft
        {
            get
            {
                if (!RunningOnWin2K)
                {
                    return System.Windows.Forms.RightToLeft.No;
                }
                return base.RightToLeft;
            }
            set
            {
                base.RightToLeft = value;
            }
        }

        private static bool RunningOnWin2K
        {
            get
            {
                if (!checkedOS && ((Environment.OSVersion.Platform != PlatformID.Win32NT) || (Environment.OSVersion.Version.Major < 5)))
                {
                    runningOnWin2K = false;
                    checkedOS = true;
                }
                return runningOnWin2K;
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("ListBoxScrollIsVisibleDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool ScrollAlwaysVisible
        {
            get
            {
                return this.scrollAlwaysVisible;
            }
            set
            {
                if (this.scrollAlwaysVisible != value)
                {
                    this.scrollAlwaysVisible = value;
                    base.RecreateHandle();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ListBoxSelectedIndexDescr"), Browsable(false), Bindable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override int SelectedIndex
        {
            get
            {
                System.Windows.Forms.SelectionMode mode = this.selectionModeChanging ? this.cachedSelectionMode : this.selectionMode;
                if (mode != System.Windows.Forms.SelectionMode.None)
                {
                    if ((mode == System.Windows.Forms.SelectionMode.One) && base.IsHandleCreated)
                    {
                        return (int) ((long) base.SendMessage(0x188, 0, 0));
                    }
                    if ((this.itemsCollection != null) && (this.SelectedItems.Count > 0))
                    {
                        return this.Items.IndexOfIdentifier(this.SelectedItems.GetObjectAt(0));
                    }
                }
                return -1;
            }
            set
            {
                int num = (this.itemsCollection == null) ? 0 : this.itemsCollection.Count;
                if ((value < -1) || (value >= num))
                {
                    throw new ArgumentOutOfRangeException("SelectedIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "SelectedIndex", value.ToString(CultureInfo.CurrentCulture) }));
                }
                if (this.selectionMode == System.Windows.Forms.SelectionMode.None)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ListBoxInvalidSelectionMode"), "SelectedIndex");
                }
                if ((this.selectionMode == System.Windows.Forms.SelectionMode.One) && (value != -1))
                {
                    int selectedIndex = this.SelectedIndex;
                    if (selectedIndex != value)
                    {
                        if (selectedIndex != -1)
                        {
                            this.SelectedItems.SetSelected(selectedIndex, false);
                        }
                        this.SelectedItems.SetSelected(value, true);
                        if (base.IsHandleCreated)
                        {
                            this.NativeSetSelected(value, true);
                        }
                        this.OnSelectedIndexChanged(EventArgs.Empty);
                    }
                }
                else if (value == -1)
                {
                    if (this.SelectedIndex != -1)
                    {
                        this.ClearSelected();
                    }
                }
                else if (!this.SelectedItems.GetSelected(value))
                {
                    this.SelectedItems.SetSelected(value, true);
                    if (base.IsHandleCreated)
                    {
                        this.NativeSetSelected(value, true);
                    }
                    this.OnSelectedIndexChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRDescription("ListBoxSelectedIndicesDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SelectedIndexCollection SelectedIndices
        {
            get
            {
                if (this.selectedIndices == null)
                {
                    this.selectedIndices = new SelectedIndexCollection(this);
                }
                return this.selectedIndices;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ListBoxSelectedItemDescr"), Browsable(false), Bindable(true)]
        public object SelectedItem
        {
            get
            {
                if (this.SelectedItems.Count > 0)
                {
                    return this.SelectedItems[0];
                }
                return null;
            }
            set
            {
                if (this.itemsCollection != null)
                {
                    if (value != null)
                    {
                        int index = this.itemsCollection.IndexOf(value);
                        if (index != -1)
                        {
                            this.SelectedIndex = index;
                        }
                    }
                    else
                    {
                        this.SelectedIndex = -1;
                    }
                }
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("ListBoxSelectedItemsDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SelectedObjectCollection SelectedItems
        {
            get
            {
                if (this.selectedItems == null)
                {
                    this.selectedItems = new SelectedObjectCollection(this);
                }
                return this.selectedItems;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ListBoxSelectionModeDescr"), DefaultValue(1)]
        public virtual System.Windows.Forms.SelectionMode SelectionMode
        {
            get
            {
                return this.selectionMode;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.SelectionMode));
                }
                if (this.selectionMode != value)
                {
                    this.SelectedItems.EnsureUpToDate();
                    this.selectionMode = value;
                    try
                    {
                        this.selectionModeChanging = true;
                        base.RecreateHandle();
                    }
                    finally
                    {
                        this.selectionModeChanging = false;
                        this.cachedSelectionMode = this.selectionMode;
                        if (base.IsHandleCreated)
                        {
                            this.NativeUpdateSelection();
                        }
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false), System.Windows.Forms.SRDescription("ListBoxSortedDescr")]
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
                    this.sorted = value;
                    if ((this.sorted && (this.itemsCollection != null)) && (this.itemsCollection.Count >= 1))
                    {
                        this.Sort();
                    }
                }
            }
        }

        [Bindable(false), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get
            {
                if ((this.SelectionMode == System.Windows.Forms.SelectionMode.None) || (this.SelectedItem == null))
                {
                    return base.Text;
                }
                if (base.FormattingEnabled)
                {
                    return base.GetItemText(this.SelectedItem);
                }
                return base.FilterItemOnProperty(this.SelectedItem).ToString();
            }
            set
            {
                base.Text = value;
                if (((this.SelectionMode != System.Windows.Forms.SelectionMode.None) && (value != null)) && ((this.SelectedItem == null) || !value.Equals(base.GetItemText(this.SelectedItem))))
                {
                    int count = this.Items.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (string.Compare(value, base.GetItemText(this.Items[i]), true, CultureInfo.CurrentCulture) == 0)
                        {
                            this.SelectedIndex = i;
                            return;
                        }
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ListBoxTopIndexDescr"), Browsable(false)]
        public int TopIndex
        {
            get
            {
                if (base.IsHandleCreated)
                {
                    return (int) ((long) base.SendMessage(0x18e, 0, 0));
                }
                return this.topIndex;
            }
            set
            {
                if (base.IsHandleCreated)
                {
                    base.SendMessage(0x197, value, 0);
                }
                else
                {
                    this.topIndex = value;
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Browsable(false), DefaultValue(false)]
        public bool UseCustomTabOffsets
        {
            get
            {
                return this.useCustomTabOffsets;
            }
            set
            {
                if (this.useCustomTabOffsets != value)
                {
                    this.useCustomTabOffsets = value;
                    base.RecreateHandle();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ListBoxUseTabStopsDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true)]
        public bool UseTabStops
        {
            get
            {
                return this.useTabStops;
            }
            set
            {
                if (this.useTabStops != value)
                {
                    this.useTabStops = value;
                    base.RecreateHandle();
                }
            }
        }

        public class IntegerCollection : IList, ICollection, IEnumerable
        {
            private int count;
            private int[] innerArray;
            private ListBox owner;

            public IntegerCollection(ListBox owner)
            {
                this.owner = owner;
            }

            public int Add(int item)
            {
                int num = this.AddInternal(item);
                this.owner.UpdateCustomTabOffsets();
                return num;
            }

            private int AddInternal(int item)
            {
                this.EnsureSpace(1);
                int index = this.IndexOf(item);
                if (index == -1)
                {
                    this.innerArray[this.count++] = item;
                    Array.Sort<int>(this.innerArray, 0, this.count);
                    index = this.IndexOf(item);
                }
                return index;
            }

            public void AddRange(int[] items)
            {
                this.AddRangeInternal(items);
            }

            public void AddRange(ListBox.IntegerCollection value)
            {
                this.AddRangeInternal(value);
            }

            private void AddRangeInternal(ICollection items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                this.owner.BeginUpdate();
                try
                {
                    this.EnsureSpace(items.Count);
                    foreach (object obj2 in items)
                    {
                        if (!(obj2 is int))
                        {
                            throw new ArgumentException("item");
                        }
                        this.AddInternal((int) obj2);
                    }
                    this.owner.UpdateCustomTabOffsets();
                }
                finally
                {
                    this.owner.EndUpdate();
                }
            }

            public void Clear()
            {
                this.count = 0;
                this.innerArray = null;
            }

            public bool Contains(int item)
            {
                return (this.IndexOf(item) != -1);
            }

            public void CopyTo(Array destination, int index)
            {
                int count = this.Count;
                for (int i = 0; i < count; i++)
                {
                    destination.SetValue(this[i], (int) (i + index));
                }
            }

            private void EnsureSpace(int elements)
            {
                if (this.innerArray == null)
                {
                    this.innerArray = new int[Math.Max(elements, 4)];
                }
                else if ((this.count + elements) >= this.innerArray.Length)
                {
                    int[] array = new int[Math.Max((int) (this.innerArray.Length * 2), (int) (this.innerArray.Length + elements))];
                    this.innerArray.CopyTo(array, 0);
                    this.innerArray = array;
                }
            }

            public int IndexOf(int item)
            {
                int index = -1;
                if (this.innerArray != null)
                {
                    index = Array.IndexOf<int>(this.innerArray, item);
                    if (index >= this.count)
                    {
                        index = -1;
                    }
                }
                return index;
            }

            public void Remove(int item)
            {
                int index = this.IndexOf(item);
                if (index != -1)
                {
                    this.RemoveAt(index);
                }
            }

            public void RemoveAt(int index)
            {
                if ((index < 0) || (index >= this.count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                this.count--;
                for (int i = index; i < this.count; i++)
                {
                    this.innerArray[i] = this.innerArray[i + 1];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new CustomTabOffsetsEnumerator(this);
            }

            int IList.Add(object item)
            {
                if (!(item is int))
                {
                    throw new ArgumentException("item");
                }
                return this.Add((int) item);
            }

            void IList.Clear()
            {
                this.Clear();
            }

            bool IList.Contains(object item)
            {
                return ((item is int) && this.Contains((int) item));
            }

            int IList.IndexOf(object item)
            {
                if (item is int)
                {
                    return this.IndexOf((int) item);
                }
                return -1;
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListBoxCantInsertIntoIntegerCollection"));
            }

            void IList.Remove(object value)
            {
                if (!(value is int))
                {
                    throw new ArgumentException("value");
                }
                this.Remove((int) value);
            }

            void IList.RemoveAt(int index)
            {
                this.RemoveAt(index);
            }

            [Browsable(false)]
            public int Count
            {
                get
                {
                    return this.count;
                }
            }

            public int this[int index]
            {
                get
                {
                    return this.innerArray[index];
                }
                set
                {
                    if ((index < 0) || (index >= this.count))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    this.innerArray[index] = value;
                    this.owner.UpdateCustomTabOffsets();
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return true;
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

            bool IList.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    if (!(value is int))
                    {
                        throw new ArgumentException("value");
                    }
                    this[index] = (int) value;
                }
            }

            private class CustomTabOffsetsEnumerator : IEnumerator
            {
                private int current;
                private ListBox.IntegerCollection items;

                public CustomTabOffsetsEnumerator(ListBox.IntegerCollection items)
                {
                    this.items = items;
                    this.current = -1;
                }

                bool IEnumerator.MoveNext()
                {
                    if (this.current < (this.items.Count - 1))
                    {
                        this.current++;
                        return true;
                    }
                    this.current = this.items.Count;
                    return false;
                }

                void IEnumerator.Reset()
                {
                    this.current = -1;
                }

                object IEnumerator.Current
                {
                    get
                    {
                        if ((this.current == -1) || (this.current == this.items.Count))
                        {
                            throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListEnumCurrentOutOfRange"));
                        }
                        return this.items[this.current];
                    }
                }
            }
        }

        internal class ItemArray : IComparer
        {
            private int count;
            private Entry[] entries;
            private static int lastMask = 1;
            private ListControl listControl;
            private int version;

            public ItemArray(ListControl listControl)
            {
                this.listControl = listControl;
            }

            public object Add(object item)
            {
                this.EnsureSpace(1);
                this.version++;
                this.entries[this.count] = new Entry(item);
                return this.entries[this.count++];
            }

            public void AddRange(ICollection items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                this.EnsureSpace(items.Count);
                foreach (object obj2 in items)
                {
                    this.entries[this.count++] = new Entry(obj2);
                }
                this.version++;
            }

            public int BinarySearch(object element)
            {
                return Array.BinarySearch(this.entries, 0, this.count, element, this);
            }

            public void Clear()
            {
                this.count = 0;
                this.version++;
            }

            public static int CreateMask()
            {
                int lastMask = ListBox.ItemArray.lastMask;
                ListBox.ItemArray.lastMask = ListBox.ItemArray.lastMask << 1;
                return lastMask;
            }

            private void EnsureSpace(int elements)
            {
                if (this.entries == null)
                {
                    this.entries = new Entry[Math.Max(elements, 4)];
                }
                else if ((this.count + elements) >= this.entries.Length)
                {
                    Entry[] array = new Entry[Math.Max((int) (this.entries.Length * 2), (int) (this.entries.Length + elements))];
                    this.entries.CopyTo(array, 0);
                    this.entries = array;
                }
            }

            public int GetActualIndex(int virtualIndex, int stateMask)
            {
                if (stateMask == 0)
                {
                    return virtualIndex;
                }
                int num = -1;
                for (int i = 0; i < this.count; i++)
                {
                    if ((this.entries[i].state & stateMask) != 0)
                    {
                        num++;
                        if (num == virtualIndex)
                        {
                            return i;
                        }
                    }
                }
                return -1;
            }

            public int GetCount(int stateMask)
            {
                if (stateMask == 0)
                {
                    return this.count;
                }
                int num = 0;
                for (int i = 0; i < this.count; i++)
                {
                    if ((this.entries[i].state & stateMask) != 0)
                    {
                        num++;
                    }
                }
                return num;
            }

            internal object GetEntryObject(int virtualIndex, int stateMask)
            {
                int actualIndex = this.GetActualIndex(virtualIndex, stateMask);
                if (actualIndex == -1)
                {
                    throw new IndexOutOfRangeException();
                }
                return this.entries[actualIndex];
            }

            public IEnumerator GetEnumerator(int stateMask)
            {
                return this.GetEnumerator(stateMask, false);
            }

            public IEnumerator GetEnumerator(int stateMask, bool anyBit)
            {
                return new EntryEnumerator(this, stateMask, anyBit);
            }

            public object GetItem(int virtualIndex, int stateMask)
            {
                int actualIndex = this.GetActualIndex(virtualIndex, stateMask);
                if (actualIndex == -1)
                {
                    throw new IndexOutOfRangeException();
                }
                return this.entries[actualIndex].item;
            }

            public bool GetState(int index, int stateMask)
            {
                return ((this.entries[index].state & stateMask) == stateMask);
            }

            public int IndexOf(object item, int stateMask)
            {
                int num = -1;
                for (int i = 0; i < this.count; i++)
                {
                    if ((stateMask == 0) || ((this.entries[i].state & stateMask) != 0))
                    {
                        num++;
                        if (this.entries[i].item.Equals(item))
                        {
                            return num;
                        }
                    }
                }
                return -1;
            }

            public int IndexOfIdentifier(object identifier, int stateMask)
            {
                int num = -1;
                for (int i = 0; i < this.count; i++)
                {
                    if ((stateMask == 0) || ((this.entries[i].state & stateMask) != 0))
                    {
                        num++;
                        if (this.entries[i] == identifier)
                        {
                            return num;
                        }
                    }
                }
                return -1;
            }

            public void Insert(int index, object item)
            {
                this.EnsureSpace(1);
                if (index < this.count)
                {
                    Array.Copy(this.entries, index, this.entries, index + 1, this.count - index);
                }
                this.entries[index] = new Entry(item);
                this.count++;
                this.version++;
            }

            public void Remove(object item)
            {
                int index = this.IndexOf(item, 0);
                if (index != -1)
                {
                    this.RemoveAt(index);
                }
            }

            public void RemoveAt(int index)
            {
                this.count--;
                for (int i = index; i < this.count; i++)
                {
                    this.entries[i] = this.entries[i + 1];
                }
                this.entries[this.count] = null;
                this.version++;
            }

            public void SetItem(int index, object item)
            {
                this.entries[index].item = item;
            }

            public void SetState(int index, int stateMask, bool value)
            {
                if (value)
                {
                    Entry entry1 = this.entries[index];
                    entry1.state |= stateMask;
                }
                else
                {
                    Entry entry2 = this.entries[index];
                    entry2.state &= ~stateMask;
                }
                this.version++;
            }

            public void Sort()
            {
                Array.Sort(this.entries, 0, this.count, this);
            }

            public void Sort(Array externalArray)
            {
                Array.Sort(externalArray, this);
            }

            int IComparer.Compare(object item1, object item2)
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
                if (item1 is Entry)
                {
                    item1 = ((Entry) item1).item;
                }
                if (item2 is Entry)
                {
                    item2 = ((Entry) item2).item;
                }
                string itemText = this.listControl.GetItemText(item1);
                string str2 = this.listControl.GetItemText(item2);
                return Application.CurrentCulture.CompareInfo.Compare(itemText, str2, CompareOptions.StringSort);
            }

            public int Version
            {
                get
                {
                    return this.version;
                }
            }

            private class Entry
            {
                public object item;
                public int state;

                public Entry(object item)
                {
                    this.item = item;
                    this.state = 0;
                }
            }

            private class EntryEnumerator : IEnumerator
            {
                private bool anyBit;
                private int current;
                private ListBox.ItemArray items;
                private int state;
                private int version;

                public EntryEnumerator(ListBox.ItemArray items, int state, bool anyBit)
                {
                    this.items = items;
                    this.state = state;
                    this.anyBit = anyBit;
                    this.version = items.version;
                    this.current = -1;
                }

                bool IEnumerator.MoveNext()
                {
                    if (this.version != this.items.version)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListEnumVersionMismatch"));
                    }
                    while (this.current < (this.items.count - 1))
                    {
                        this.current++;
                        if (!this.anyBit)
                        {
                            if ((this.items.entries[this.current].state & this.state) == this.state)
                            {
                                return true;
                            }
                        }
                        else if ((this.items.entries[this.current].state & this.state) != 0)
                        {
                            return true;
                        }
                    }
                    this.current = this.items.count;
                    return false;
                }

                void IEnumerator.Reset()
                {
                    if (this.version != this.items.version)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListEnumVersionMismatch"));
                    }
                    this.current = -1;
                }

                object IEnumerator.Current
                {
                    get
                    {
                        if ((this.current == -1) || (this.current == this.items.count))
                        {
                            throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListEnumCurrentOutOfRange"));
                        }
                        return this.items.entries[this.current].item;
                    }
                }
            }
        }

        [ListBindable(false)]
        public class ObjectCollection : IList, ICollection, IEnumerable
        {
            private ListBox.ItemArray items;
            private ListBox owner;

            public ObjectCollection(ListBox owner)
            {
                this.owner = owner;
            }

            public ObjectCollection(ListBox owner, ListBox.ObjectCollection value)
            {
                this.owner = owner;
                this.AddRange(value);
            }

            public ObjectCollection(ListBox owner, object[] value)
            {
                this.owner = owner;
                this.AddRange(value);
            }

            public int Add(object item)
            {
                this.owner.CheckNoDataSource();
                int num = this.AddInternal(item);
                this.owner.UpdateHorizontalExtent();
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
                    this.InnerArray.Add(item);
                }
                else
                {
                    if (this.Count > 0)
                    {
                        index = this.InnerArray.BinarySearch(item);
                        if (index < 0)
                        {
                            index = ~index;
                        }
                    }
                    else
                    {
                        index = 0;
                    }
                    this.InnerArray.Insert(index, item);
                }
                bool flag = false;
                try
                {
                    if (this.owner.sorted)
                    {
                        if (this.owner.IsHandleCreated)
                        {
                            this.owner.NativeInsert(index, item);
                            this.owner.UpdateMaxItemWidth(item, false);
                            if (this.owner.selectedItems != null)
                            {
                                this.owner.selectedItems.Dirty();
                            }
                        }
                    }
                    else
                    {
                        index = this.Count - 1;
                        if (this.owner.IsHandleCreated)
                        {
                            this.owner.NativeAdd(item);
                            this.owner.UpdateMaxItemWidth(item, false);
                        }
                    }
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        this.InnerArray.Remove(item);
                    }
                }
                return index;
            }

            public void AddRange(ListBox.ObjectCollection value)
            {
                this.owner.CheckNoDataSource();
                this.AddRangeInternal(value);
            }

            public void AddRange(object[] items)
            {
                this.owner.CheckNoDataSource();
                this.AddRangeInternal(items);
            }

            internal void AddRangeInternal(ICollection items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException("items");
                }
                this.owner.BeginUpdate();
                try
                {
                    foreach (object obj2 in items)
                    {
                        this.AddInternal(obj2);
                    }
                }
                finally
                {
                    this.owner.UpdateHorizontalExtent();
                    this.owner.EndUpdate();
                }
            }

            public virtual void Clear()
            {
                this.owner.CheckNoDataSource();
                this.ClearInternal();
            }

            internal void ClearInternal()
            {
                int count = this.owner.Items.Count;
                for (int i = 0; i < count; i++)
                {
                    this.owner.UpdateMaxItemWidth(this.InnerArray.GetItem(i, 0), true);
                }
                if (this.owner.IsHandleCreated)
                {
                    this.owner.NativeClear();
                }
                this.InnerArray.Clear();
                this.owner.maxWidth = -1;
                this.owner.UpdateHorizontalExtent();
            }

            public bool Contains(object value)
            {
                return (this.IndexOf(value) != -1);
            }

            public void CopyTo(object[] destination, int arrayIndex)
            {
                int count = this.InnerArray.GetCount(0);
                for (int i = 0; i < count; i++)
                {
                    destination[i + arrayIndex] = this.InnerArray.GetItem(i, 0);
                }
            }

            public IEnumerator GetEnumerator()
            {
                return this.InnerArray.GetEnumerator(0);
            }

            public int IndexOf(object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                return this.InnerArray.IndexOf(value, 0);
            }

            internal int IndexOfIdentifier(object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                return this.InnerArray.IndexOfIdentifier(value, 0);
            }

            public void Insert(int index, object item)
            {
                this.owner.CheckNoDataSource();
                if ((index < 0) || (index > this.InnerArray.GetCount(0)))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }
                if (this.owner.sorted)
                {
                    this.Add(item);
                }
                else
                {
                    this.InnerArray.Insert(index, item);
                    if (this.owner.IsHandleCreated)
                    {
                        bool flag = false;
                        try
                        {
                            this.owner.NativeInsert(index, item);
                            this.owner.UpdateMaxItemWidth(item, false);
                            flag = true;
                        }
                        finally
                        {
                            if (!flag)
                            {
                                this.InnerArray.RemoveAt(index);
                            }
                        }
                    }
                }
                this.owner.UpdateHorizontalExtent();
            }

            public void Remove(object value)
            {
                int index = this.InnerArray.IndexOf(value, 0);
                if (index != -1)
                {
                    this.RemoveAt(index);
                }
            }

            public void RemoveAt(int index)
            {
                this.owner.CheckNoDataSource();
                if ((index < 0) || (index >= this.InnerArray.GetCount(0)))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                this.owner.UpdateMaxItemWidth(this.InnerArray.GetItem(index, 0), true);
                this.InnerArray.RemoveAt(index);
                if (this.owner.IsHandleCreated)
                {
                    this.owner.NativeRemoveAt(index);
                }
                this.owner.UpdateHorizontalExtent();
            }

            internal void SetItemInternal(int index, object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if ((index < 0) || (index >= this.InnerArray.GetCount(0)))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                this.owner.UpdateMaxItemWidth(this.InnerArray.GetItem(index, 0), true);
                this.InnerArray.SetItem(index, value);
                if (this.owner.IsHandleCreated)
                {
                    bool flag = this.owner.SelectedIndex == index;
                    if (string.Compare(this.owner.GetItemText(value), this.owner.NativeGetItemText(index), true, CultureInfo.CurrentCulture) != 0)
                    {
                        this.owner.NativeRemoveAt(index);
                        this.owner.SelectedItems.SetSelected(index, false);
                        this.owner.NativeInsert(index, value);
                        this.owner.UpdateMaxItemWidth(value, false);
                        if (flag)
                        {
                            this.owner.SelectedIndex = index;
                        }
                    }
                    else if (flag)
                    {
                        this.owner.OnSelectedIndexChanged(EventArgs.Empty);
                    }
                }
                this.owner.UpdateHorizontalExtent();
            }

            void ICollection.CopyTo(Array destination, int index)
            {
                int count = this.InnerArray.GetCount(0);
                for (int i = 0; i < count; i++)
                {
                    destination.SetValue(this.InnerArray.GetItem(i, 0), (int) (i + index));
                }
            }

            int IList.Add(object item)
            {
                return this.Add(item);
            }

            public int Count
            {
                get
                {
                    return this.InnerArray.GetCount(0);
                }
            }

            internal ListBox.ItemArray InnerArray
            {
                get
                {
                    if (this.items == null)
                    {
                        this.items = new ListBox.ItemArray(this.owner);
                    }
                    return this.items;
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
                    if ((index < 0) || (index >= this.InnerArray.GetCount(0)))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    return this.InnerArray.GetItem(index, 0);
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

        public class SelectedIndexCollection : IList, ICollection, IEnumerable
        {
            private ListBox owner;

            public SelectedIndexCollection(ListBox owner)
            {
                this.owner = owner;
            }

            public void Add(int index)
            {
                if (((this.owner != null) && (this.owner.Items != null)) && ((index != -1) && !this.Contains(index)))
                {
                    this.owner.SetSelected(index, true);
                }
            }

            public void Clear()
            {
                if (this.owner != null)
                {
                    this.owner.ClearSelected();
                }
            }

            public bool Contains(int selectedIndex)
            {
                return (this.IndexOf(selectedIndex) != -1);
            }

            public void CopyTo(Array destination, int index)
            {
                int count = this.Count;
                for (int i = 0; i < count; i++)
                {
                    destination.SetValue(this[i], (int) (i + index));
                }
            }

            public IEnumerator GetEnumerator()
            {
                return new SelectedIndexEnumerator(this);
            }

            public int IndexOf(int selectedIndex)
            {
                if (((selectedIndex >= 0) && (selectedIndex < this.InnerArray.GetCount(0))) && this.InnerArray.GetState(selectedIndex, ListBox.SelectedObjectCollection.SelectedObjectMask))
                {
                    return this.InnerArray.IndexOf(this.InnerArray.GetItem(selectedIndex, 0), ListBox.SelectedObjectCollection.SelectedObjectMask);
                }
                return -1;
            }

            public void Remove(int index)
            {
                if (((this.owner != null) && (this.owner.Items != null)) && ((index != -1) && this.Contains(index)))
                {
                    this.owner.SetSelected(index, false);
                }
            }

            int IList.Add(object value)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListBoxSelectedIndexCollectionIsReadOnly"));
            }

            void IList.Clear()
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListBoxSelectedIndexCollectionIsReadOnly"));
            }

            bool IList.Contains(object selectedIndex)
            {
                return ((selectedIndex is int) && this.Contains((int) selectedIndex));
            }

            int IList.IndexOf(object selectedIndex)
            {
                if (selectedIndex is int)
                {
                    return this.IndexOf((int) selectedIndex);
                }
                return -1;
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListBoxSelectedIndexCollectionIsReadOnly"));
            }

            void IList.Remove(object value)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListBoxSelectedIndexCollectionIsReadOnly"));
            }

            void IList.RemoveAt(int index)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListBoxSelectedIndexCollectionIsReadOnly"));
            }

            [Browsable(false)]
            public int Count
            {
                get
                {
                    return this.owner.SelectedItems.Count;
                }
            }

            private ListBox.ItemArray InnerArray
            {
                get
                {
                    this.owner.SelectedItems.EnsureUpToDate();
                    return this.owner.Items.InnerArray;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public int this[int index]
            {
                get
                {
                    object entryObject = this.InnerArray.GetEntryObject(index, ListBox.SelectedObjectCollection.SelectedObjectMask);
                    return this.InnerArray.IndexOfIdentifier(entryObject, 0);
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return true;
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
                    return true;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListBoxSelectedIndexCollectionIsReadOnly"));
                }
            }

            private class SelectedIndexEnumerator : IEnumerator
            {
                private int current;
                private ListBox.SelectedIndexCollection items;

                public SelectedIndexEnumerator(ListBox.SelectedIndexCollection items)
                {
                    this.items = items;
                    this.current = -1;
                }

                bool IEnumerator.MoveNext()
                {
                    if (this.current < (this.items.Count - 1))
                    {
                        this.current++;
                        return true;
                    }
                    this.current = this.items.Count;
                    return false;
                }

                void IEnumerator.Reset()
                {
                    this.current = -1;
                }

                object IEnumerator.Current
                {
                    get
                    {
                        if ((this.current == -1) || (this.current == this.items.Count))
                        {
                            throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListEnumCurrentOutOfRange"));
                        }
                        return this.items[this.current];
                    }
                }
            }
        }

        public class SelectedObjectCollection : IList, ICollection, IEnumerable
        {
            private int count;
            private int lastVersion;
            private ListBox owner;
            internal static int SelectedObjectMask = ListBox.ItemArray.CreateMask();
            private bool stateDirty;

            public SelectedObjectCollection(ListBox owner)
            {
                this.owner = owner;
                this.stateDirty = true;
                this.lastVersion = -1;
            }

            public void Add(object value)
            {
                if (this.owner != null)
                {
                    ListBox.ObjectCollection items = this.owner.Items;
                    if ((items != null) && (value != null))
                    {
                        int index = items.IndexOf(value);
                        if ((index != -1) && !this.GetSelected(index))
                        {
                            this.owner.SelectedIndex = index;
                        }
                    }
                }
            }

            public void Clear()
            {
                if (this.owner != null)
                {
                    this.owner.ClearSelected();
                }
            }

            public bool Contains(object selectedObject)
            {
                return (this.IndexOf(selectedObject) != -1);
            }

            public void CopyTo(Array destination, int index)
            {
                int count = this.InnerArray.GetCount(SelectedObjectMask);
                for (int i = 0; i < count; i++)
                {
                    destination.SetValue(this.InnerArray.GetItem(i, SelectedObjectMask), (int) (i + index));
                }
            }

            internal void Dirty()
            {
                this.stateDirty = true;
            }

            internal void EnsureUpToDate()
            {
                if (this.stateDirty)
                {
                    this.stateDirty = false;
                    if (this.owner.IsHandleCreated)
                    {
                        this.owner.NativeUpdateSelection();
                    }
                }
            }

            public IEnumerator GetEnumerator()
            {
                return this.InnerArray.GetEnumerator(SelectedObjectMask);
            }

            internal object GetObjectAt(int index)
            {
                return this.InnerArray.GetEntryObject(index, SelectedObjectMask);
            }

            internal bool GetSelected(int index)
            {
                return this.InnerArray.GetState(index, SelectedObjectMask);
            }

            public int IndexOf(object selectedObject)
            {
                return this.InnerArray.IndexOf(selectedObject, SelectedObjectMask);
            }

            internal void PushSelectionIntoNativeListBox(int index)
            {
                if (this.owner.Items.InnerArray.GetState(index, SelectedObjectMask))
                {
                    this.owner.NativeSetSelected(index, true);
                }
            }

            public void Remove(object value)
            {
                if (this.owner != null)
                {
                    ListBox.ObjectCollection items = this.owner.Items;
                    if ((items != null) & (value != null))
                    {
                        int index = items.IndexOf(value);
                        if ((index != -1) && this.GetSelected(index))
                        {
                            this.owner.SetSelected(index, false);
                        }
                    }
                }
            }

            internal void SetSelected(int index, bool value)
            {
                this.InnerArray.SetState(index, SelectedObjectMask, value);
            }

            int IList.Add(object value)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListBoxSelectedObjectCollectionIsReadOnly"));
            }

            void IList.Clear()
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListBoxSelectedObjectCollectionIsReadOnly"));
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListBoxSelectedObjectCollectionIsReadOnly"));
            }

            void IList.Remove(object value)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListBoxSelectedObjectCollectionIsReadOnly"));
            }

            void IList.RemoveAt(int index)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListBoxSelectedObjectCollectionIsReadOnly"));
            }

            public int Count
            {
                get
                {
                    if (this.owner.IsHandleCreated)
                    {
                        switch ((this.owner.selectionModeChanging ? this.owner.cachedSelectionMode : this.owner.selectionMode))
                        {
                            case SelectionMode.None:
                                return 0;

                            case SelectionMode.One:
                                if (this.owner.SelectedIndex < 0)
                                {
                                    return 0;
                                }
                                return 1;

                            case SelectionMode.MultiSimple:
                            case SelectionMode.MultiExtended:
                                return (int) ((long) this.owner.SendMessage(400, 0, 0));
                        }
                        return 0;
                    }
                    if (this.lastVersion != this.InnerArray.Version)
                    {
                        this.lastVersion = this.InnerArray.Version;
                        this.count = this.InnerArray.GetCount(SelectedObjectMask);
                    }
                    return this.count;
                }
            }

            private ListBox.ItemArray InnerArray
            {
                get
                {
                    this.EnsureUpToDate();
                    return this.owner.Items.InnerArray;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public object this[int index]
            {
                get
                {
                    return this.InnerArray.GetItem(index, SelectedObjectMask);
                }
                set
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ListBoxSelectedObjectCollectionIsReadOnly"));
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
                    return true;
                }
            }
        }
    }
}

