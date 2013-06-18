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
    using System.Security.Permissions;
    using System.Windows.Forms.VisualStyles;

    [ComVisible(true), System.Windows.Forms.SRDescription("DescriptionCheckedListBox"), ClassInterface(ClassInterfaceType.AutoDispatch), LookupBindingProperties]
    public class CheckedListBox : ListBox
    {
        private CheckedIndexCollection checkedIndexCollection;
        private CheckedItemCollection checkedItemCollection;
        private bool checkOnClick;
        private bool flat = true;
        private int idealCheckSize = 13;
        private bool killnextselect;
        private int lastSelected = -1;
        private const int LB_CHECKED = 1;
        private const int LB_ERROR = -1;
        private const int LB_UNCHECKED = 0;
        private static int LBC_GETCHECKSTATE = System.Windows.Forms.SafeNativeMethods.RegisterWindowMessage("LBC_GETCHECKSTATE");
        private static int LBC_SETCHECKSTATE = System.Windows.Forms.SafeNativeMethods.RegisterWindowMessage("LBC_SETCHECKSTATE");

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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler DataSourceChanged
        {
            add
            {
                base.DataSourceChanged += value;
            }
            remove
            {
                base.DataSourceChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler DisplayMemberChanged
        {
            add
            {
                base.DisplayMemberChanged += value;
            }
            remove
            {
                base.DisplayMemberChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event DrawItemEventHandler DrawItem
        {
            add
            {
                base.DrawItem += value;
            }
            remove
            {
                base.DrawItem -= value;
            }
        }

        [System.Windows.Forms.SRDescription("CheckedListBoxItemCheckDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event ItemCheckEventHandler ItemCheck;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event MeasureItemEventHandler MeasureItem
        {
            add
            {
                base.MeasureItem += value;
            }
            remove
            {
                base.MeasureItem -= value;
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
        public event EventHandler ValueMemberChanged
        {
            add
            {
                base.ValueMemberChanged += value;
            }
            remove
            {
                base.ValueMemberChanged -= value;
            }
        }

        public CheckedListBox()
        {
            base.SetStyle(ControlStyles.ResizeRedraw, true);
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new CheckedListBoxAccessibleObject(this);
        }

        protected override ListBox.ObjectCollection CreateItemCollection()
        {
            return new ObjectCollection(this);
        }

        public bool GetItemChecked(int index)
        {
            return (this.GetItemCheckState(index) != CheckState.Unchecked);
        }

        public CheckState GetItemCheckState(int index)
        {
            if ((index < 0) || (index >= this.Items.Count))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            return this.CheckedItems.GetCheckedState(index);
        }

        private void InvalidateItem(int index)
        {
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.NativeMethods.RECT lparam = new System.Windows.Forms.NativeMethods.RECT();
                base.SendMessage(0x198, index, ref lparam);
                System.Windows.Forms.SafeNativeMethods.InvalidateRect(new HandleRef(this, base.Handle), ref lparam, false);
            }
        }

        private void LbnSelChange()
        {
            int selectedIndex = this.SelectedIndex;
            if ((selectedIndex >= 0) && (selectedIndex < this.Items.Count))
            {
                base.AccessibilityNotifyClients(AccessibleEvents.Focus, selectedIndex);
                base.AccessibilityNotifyClients(AccessibleEvents.Selection, selectedIndex);
                if (!this.killnextselect && ((selectedIndex == this.lastSelected) || this.checkOnClick))
                {
                    CheckState checkedState = this.CheckedItems.GetCheckedState(selectedIndex);
                    CheckState newCheckValue = (checkedState != CheckState.Unchecked) ? CheckState.Unchecked : CheckState.Checked;
                    ItemCheckEventArgs ice = new ItemCheckEventArgs(selectedIndex, newCheckValue, checkedState);
                    this.OnItemCheck(ice);
                    this.CheckedItems.SetCheckedState(selectedIndex, ice.NewValue);
                }
                this.lastSelected = selectedIndex;
                this.InvalidateItem(selectedIndex);
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.SafeNativeMethods.InvalidateRect(new HandleRef(this, base.Handle), (System.Windows.Forms.NativeMethods.COMRECT) null, true);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            this.killnextselect = false;
            base.OnClick(e);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            object obj2;
            if (this.Font.Height < 0)
            {
                this.Font = Control.DefaultFont;
            }
            if (e.Index < 0)
            {
                return;
            }
            if (e.Index < this.Items.Count)
            {
                obj2 = this.Items[e.Index];
            }
            else
            {
                obj2 = base.NativeGetItemText(e.Index);
            }
            Rectangle bounds = e.Bounds;
            int num = 1;
            int num2 = this.Font.Height + (2 * num);
            ButtonState normal = ButtonState.Normal;
            if (this.flat)
            {
                normal |= ButtonState.Flat;
            }
            if (e.Index < this.Items.Count)
            {
                switch (this.CheckedItems.GetCheckedState(e.Index))
                {
                    case CheckState.Checked:
                        normal |= ButtonState.Checked;
                        break;

                    case CheckState.Indeterminate:
                        normal |= ButtonState.Checked | ButtonState.Inactive;
                        break;
                }
            }
            if (Application.RenderWithVisualStyles)
            {
                CheckBoxState state = CheckBoxRenderer.ConvertFromButtonState(normal, false, (e.State & DrawItemState.HotLight) == DrawItemState.HotLight);
                this.idealCheckSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, state).Width;
            }
            int num3 = Math.Max((num2 - this.idealCheckSize) / 2, 0);
            if ((num3 + this.idealCheckSize) > bounds.Height)
            {
                num3 = bounds.Height - this.idealCheckSize;
            }
            Rectangle rectangle = new Rectangle(bounds.X + num, bounds.Y + num3, this.idealCheckSize, this.idealCheckSize);
            if (this.RightToLeft == RightToLeft.Yes)
            {
                rectangle.X = ((bounds.X + bounds.Width) - this.idealCheckSize) - num;
            }
            if (Application.RenderWithVisualStyles)
            {
                CheckBoxState state3 = CheckBoxRenderer.ConvertFromButtonState(normal, false, (e.State & DrawItemState.HotLight) == DrawItemState.HotLight);
                CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(rectangle.X, rectangle.Y), state3);
            }
            else
            {
                ControlPaint.DrawCheckBox(e.Graphics, rectangle, normal);
            }
            Rectangle rect = new Rectangle((bounds.X + this.idealCheckSize) + (num * 2), bounds.Y, bounds.Width - (this.idealCheckSize + (num * 2)), bounds.Height);
            if (this.RightToLeft == RightToLeft.Yes)
            {
                rect.X = bounds.X;
            }
            string s = "";
            Color highlight = (this.SelectionMode != System.Windows.Forms.SelectionMode.None) ? e.BackColor : this.BackColor;
            Color grayText = (this.SelectionMode != System.Windows.Forms.SelectionMode.None) ? e.ForeColor : this.ForeColor;
            if (!base.Enabled)
            {
                grayText = SystemColors.GrayText;
            }
            Font font = this.Font;
            s = base.GetItemText(obj2);
            if ((this.SelectionMode != System.Windows.Forms.SelectionMode.None) && ((e.State & DrawItemState.Selected) == DrawItemState.Selected))
            {
                if (base.Enabled)
                {
                    highlight = SystemColors.Highlight;
                    grayText = SystemColors.HighlightText;
                }
                else
                {
                    highlight = SystemColors.InactiveBorder;
                    grayText = SystemColors.GrayText;
                }
            }
            using (Brush brush = new SolidBrush(highlight))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
            Rectangle layoutRectangle = new Rectangle(rect.X + 1, rect.Y, rect.Width - 1, rect.Height - (num * 2));
            if (this.UseCompatibleTextRendering)
            {
                using (StringFormat format = new StringFormat())
                {
                    if (base.UseTabStops)
                    {
                        float num4 = 3.6f * this.Font.Height;
                        float[] tabStops = new float[15];
                        float num5 = -(this.idealCheckSize + (num * 2));
                        for (int i = 1; i < tabStops.Length; i++)
                        {
                            tabStops[i] = num4;
                        }
                        if (Math.Abs(num5) < num4)
                        {
                            tabStops[0] = num4 + num5;
                        }
                        else
                        {
                            tabStops[0] = num4;
                        }
                        format.SetTabStops(0f, tabStops);
                    }
                    else if (base.UseCustomTabOffsets)
                    {
                        float[] destination = new float[base.CustomTabOffsets.Count];
                        base.CustomTabOffsets.CopyTo(destination, 0);
                        format.SetTabStops(0f, destination);
                    }
                    if (this.RightToLeft == RightToLeft.Yes)
                    {
                        format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                    }
                    format.FormatFlags |= StringFormatFlags.NoWrap;
                    format.Trimming = StringTrimming.None;
                    using (SolidBrush brush2 = new SolidBrush(grayText))
                    {
                        e.Graphics.DrawString(s, font, brush2, layoutRectangle, format);
                    }
                    goto Label_049B;
                }
            }
            TextFormatFlags flags = TextFormatFlags.Default;
            flags |= TextFormatFlags.NoPrefix;
            if (base.UseTabStops || base.UseCustomTabOffsets)
            {
                flags |= TextFormatFlags.ExpandTabs;
            }
            if (this.RightToLeft == RightToLeft.Yes)
            {
                flags |= TextFormatFlags.RightToLeft;
                flags |= TextFormatFlags.Right;
            }
            TextRenderer.DrawText(e.Graphics, s, font, layoutRectangle, grayText, flags);
        Label_049B:
            if (((e.State & DrawItemState.Focus) == DrawItemState.Focus) && ((e.State & DrawItemState.NoFocusRect) != DrawItemState.NoFocusRect))
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, rect, grayText, highlight);
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            if (base.IsHandleCreated)
            {
                base.SendMessage(0x1a0, 0, this.ItemHeight);
            }
            base.OnFontChanged(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            base.SendMessage(0x1a0, 0, this.ItemHeight);
        }

        protected virtual void OnItemCheck(ItemCheckEventArgs ice)
        {
            if (this.onItemCheck != null)
            {
                this.onItemCheck(this, ice);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if ((e.KeyChar == ' ') && (this.SelectionMode != System.Windows.Forms.SelectionMode.None))
            {
                this.LbnSelChange();
            }
            if (base.FormattingEnabled)
            {
                base.OnKeyPress(e);
            }
        }

        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            base.OnMeasureItem(e);
            if (e.ItemHeight < (this.idealCheckSize + 2))
            {
                e.ItemHeight = this.idealCheckSize + 2;
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            this.lastSelected = this.SelectedIndex;
        }

        protected override void RefreshItems()
        {
            Hashtable hashtable = new Hashtable();
            for (int i = 0; i < this.Items.Count; i++)
            {
                hashtable[i] = this.CheckedItems.GetCheckedState(i);
            }
            base.RefreshItems();
            for (int j = 0; j < this.Items.Count; j++)
            {
                this.CheckedItems.SetCheckedState(j, (CheckState) hashtable[j]);
            }
        }

        public void SetItemChecked(int index, bool value)
        {
            this.SetItemCheckState(index, value ? CheckState.Checked : CheckState.Unchecked);
        }

        public void SetItemCheckState(int index, CheckState value)
        {
            if ((index < 0) || (index >= this.Items.Count))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
            {
                throw new InvalidEnumArgumentException("value", (int) value, typeof(CheckState));
            }
            CheckState checkedState = this.CheckedItems.GetCheckedState(index);
            if (value != checkedState)
            {
                ItemCheckEventArgs ice = new ItemCheckEventArgs(index, value, checkedState);
                this.OnItemCheck(ice);
                if (ice.NewValue != checkedState)
                {
                    this.CheckedItems.SetCheckedState(index, ice.NewValue);
                    this.InvalidateItem(index);
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WmReflectCommand(ref Message m)
        {
            switch (System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam))
            {
                case 1:
                    this.LbnSelChange();
                    base.WmReflectCommand(ref m);
                    return;

                case 2:
                    this.LbnSelChange();
                    base.WmReflectCommand(ref m);
                    return;
            }
            base.WmReflectCommand(ref m);
        }

        private void WmReflectVKeyToItem(ref Message m)
        {
            switch (System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam))
            {
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x27:
                case 40:
                    this.killnextselect = true;
                    break;

                default:
                    this.killnextselect = false;
                    break;
            }
            m.Result = System.Windows.Forms.NativeMethods.InvalidIntPtr;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x202e:
                    this.WmReflectVKeyToItem(ref m);
                    return;

                case 0x202f:
                    m.Result = System.Windows.Forms.NativeMethods.InvalidIntPtr;
                    return;
            }
            if (m.Msg == LBC_GETCHECKSTATE)
            {
                int wParam = (int) ((long) m.WParam);
                if ((wParam < 0) || (wParam >= this.Items.Count))
                {
                    m.Result = (IntPtr) (-1);
                }
                else
                {
                    m.Result = this.GetItemChecked(wParam) ? ((IntPtr) 1) : IntPtr.Zero;
                }
            }
            else if (m.Msg == LBC_SETCHECKSTATE)
            {
                int index = (int) ((long) m.WParam);
                int lParam = (int) ((long) m.LParam);
                if (((index < 0) || (index >= this.Items.Count)) || ((lParam != 1) && (lParam != 0)))
                {
                    m.Result = IntPtr.Zero;
                }
                else
                {
                    this.SetItemChecked(index, lParam == 1);
                    m.Result = (IntPtr) 1;
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public CheckedIndexCollection CheckedIndices
        {
            get
            {
                if (this.checkedIndexCollection == null)
                {
                    this.checkedIndexCollection = new CheckedIndexCollection(this);
                }
                return this.checkedIndexCollection;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public CheckedItemCollection CheckedItems
        {
            get
            {
                if (this.checkedItemCollection == null)
                {
                    this.checkedItemCollection = new CheckedItemCollection(this);
                }
                return this.checkedItemCollection;
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("CheckedListBoxCheckOnClickDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool CheckOnClick
        {
            get
            {
                return this.checkOnClick;
            }
            set
            {
                this.checkOnClick = value;
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.Style |= 0x410;
                return createParams;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public string DisplayMember
        {
            get
            {
                return base.DisplayMember;
            }
            set
            {
                base.DisplayMember = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override System.Windows.Forms.DrawMode DrawMode
        {
            get
            {
                return System.Windows.Forms.DrawMode.Normal;
            }
            set
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override int ItemHeight
        {
            get
            {
                return (this.Font.Height + 2);
            }
            set
            {
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("ListBoxItemsDescr"), Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), System.Windows.Forms.SRCategory("CatData"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ObjectCollection Items
        {
            get
            {
                return (ObjectCollection) base.Items;
            }
        }

        internal override int MaxItemWidth
        {
            get
            {
                return ((base.MaxItemWidth + this.idealCheckSize) + 3);
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

        public override System.Windows.Forms.SelectionMode SelectionMode
        {
            get
            {
                return base.SelectionMode;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.SelectionMode));
                }
                if ((value != System.Windows.Forms.SelectionMode.One) && (value != System.Windows.Forms.SelectionMode.None))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("CheckedListBoxInvalidSelectionMode"));
                }
                if (value != this.SelectionMode)
                {
                    base.SelectionMode = value;
                    base.RecreateHandle();
                }
            }
        }

        internal override bool SupportsUseCompatibleTextRendering
        {
            get
            {
                return true;
            }
        }

        [System.Windows.Forms.SRDescription("CheckedListBoxThreeDCheckBoxesDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(false)]
        public bool ThreeDCheckBoxes
        {
            get
            {
                return !this.flat;
            }
            set
            {
                if (this.flat == value)
                {
                    this.flat = !value;
                    ObjectCollection items = this.Items;
                    if ((items != null) && (items.Count > 0))
                    {
                        base.Invalidate();
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false), System.Windows.Forms.SRDescription("UseCompatibleTextRenderingDescr")]
        public bool UseCompatibleTextRendering
        {
            get
            {
                return base.UseCompatibleTextRenderingInt;
            }
            set
            {
                base.UseCompatibleTextRenderingInt = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public string ValueMember
        {
            get
            {
                return base.ValueMember;
            }
            set
            {
                base.ValueMember = value;
            }
        }

        public class CheckedIndexCollection : IList, ICollection, IEnumerable
        {
            private CheckedListBox owner;

            internal CheckedIndexCollection(CheckedListBox owner)
            {
                this.owner = owner;
            }

            public bool Contains(int index)
            {
                return (this.IndexOf(index) != -1);
            }

            public void CopyTo(Array dest, int index)
            {
                int count = this.owner.CheckedItems.Count;
                for (int i = 0; i < count; i++)
                {
                    dest.SetValue(this[i], (int) (i + index));
                }
            }

            public IEnumerator GetEnumerator()
            {
                int[] dest = new int[this.Count];
                this.CopyTo(dest, 0);
                return dest.GetEnumerator();
            }

            public int IndexOf(int index)
            {
                if ((index >= 0) && (index < this.owner.Items.Count))
                {
                    object entryObject = this.InnerArray.GetEntryObject(index, 0);
                    return this.owner.CheckedItems.IndexOfIdentifier(entryObject);
                }
                return -1;
            }

            int IList.Add(object value)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("CheckedListBoxCheckedIndexCollectionIsReadOnly"));
            }

            void IList.Clear()
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("CheckedListBoxCheckedIndexCollectionIsReadOnly"));
            }

            bool IList.Contains(object index)
            {
                return ((index is int) && this.Contains((int) index));
            }

            int IList.IndexOf(object index)
            {
                if (index is int)
                {
                    return this.IndexOf((int) index);
                }
                return -1;
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("CheckedListBoxCheckedIndexCollectionIsReadOnly"));
            }

            void IList.Remove(object value)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("CheckedListBoxCheckedIndexCollectionIsReadOnly"));
            }

            void IList.RemoveAt(int index)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("CheckedListBoxCheckedIndexCollectionIsReadOnly"));
            }

            public int Count
            {
                get
                {
                    return this.owner.CheckedItems.Count;
                }
            }

            private ListBox.ItemArray InnerArray
            {
                get
                {
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

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
            public int this[int index]
            {
                get
                {
                    object entryObject = this.InnerArray.GetEntryObject(index, CheckedListBox.CheckedItemCollection.AnyMask);
                    return this.InnerArray.IndexOfIdentifier(entryObject, 0);
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

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("CheckedListBoxCheckedIndexCollectionIsReadOnly"));
                }
            }
        }

        public class CheckedItemCollection : IList, ICollection, IEnumerable
        {
            internal static int AnyMask = (CheckedItemMask | IndeterminateItemMask);
            internal static int CheckedItemMask = ListBox.ItemArray.CreateMask();
            internal static int IndeterminateItemMask = ListBox.ItemArray.CreateMask();
            private CheckedListBox owner;

            internal CheckedItemCollection(CheckedListBox owner)
            {
                this.owner = owner;
            }

            public bool Contains(object item)
            {
                return (this.IndexOf(item) != -1);
            }

            public void CopyTo(Array dest, int index)
            {
                int count = this.InnerArray.GetCount(AnyMask);
                for (int i = 0; i < count; i++)
                {
                    dest.SetValue(this.InnerArray.GetItem(i, AnyMask), (int) (i + index));
                }
            }

            internal CheckState GetCheckedState(int index)
            {
                bool state = this.InnerArray.GetState(index, CheckedItemMask);
                if (this.InnerArray.GetState(index, IndeterminateItemMask))
                {
                    return CheckState.Indeterminate;
                }
                if (state)
                {
                    return CheckState.Checked;
                }
                return CheckState.Unchecked;
            }

            public IEnumerator GetEnumerator()
            {
                return this.InnerArray.GetEnumerator(AnyMask, true);
            }

            public int IndexOf(object item)
            {
                return this.InnerArray.IndexOf(item, AnyMask);
            }

            internal int IndexOfIdentifier(object item)
            {
                return this.InnerArray.IndexOfIdentifier(item, AnyMask);
            }

            internal void SetCheckedState(int index, CheckState value)
            {
                bool flag;
                bool flag2;
                switch (value)
                {
                    case CheckState.Checked:
                        flag = true;
                        flag2 = false;
                        break;

                    case CheckState.Indeterminate:
                        flag = false;
                        flag2 = true;
                        break;

                    default:
                        flag = false;
                        flag2 = false;
                        break;
                }
                bool state = this.InnerArray.GetState(index, CheckedItemMask);
                bool flag4 = this.InnerArray.GetState(index, IndeterminateItemMask);
                this.InnerArray.SetState(index, CheckedItemMask, flag);
                this.InnerArray.SetState(index, IndeterminateItemMask, flag2);
                if ((state != flag) || (flag4 != flag2))
                {
                    this.owner.AccessibilityNotifyClients(AccessibleEvents.StateChange, index);
                }
            }

            int IList.Add(object value)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("CheckedListBoxCheckedItemCollectionIsReadOnly"));
            }

            void IList.Clear()
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("CheckedListBoxCheckedItemCollectionIsReadOnly"));
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("CheckedListBoxCheckedItemCollectionIsReadOnly"));
            }

            void IList.Remove(object value)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("CheckedListBoxCheckedItemCollectionIsReadOnly"));
            }

            void IList.RemoveAt(int index)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("CheckedListBoxCheckedItemCollectionIsReadOnly"));
            }

            public int Count
            {
                get
                {
                    return this.InnerArray.GetCount(AnyMask);
                }
            }

            private ListBox.ItemArray InnerArray
            {
                get
                {
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

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
            public object this[int index]
            {
                get
                {
                    return this.InnerArray.GetItem(index, AnyMask);
                }
                set
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("CheckedListBoxCheckedItemCollectionIsReadOnly"));
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

        [ComVisible(true)]
        internal class CheckedListBoxAccessibleObject : Control.ControlAccessibleObject
        {
            public CheckedListBoxAccessibleObject(System.Windows.Forms.CheckedListBox owner) : base(owner)
            {
            }

            public override AccessibleObject GetChild(int index)
            {
                if ((index >= 0) && (index < this.CheckedListBox.Items.Count))
                {
                    return new System.Windows.Forms.CheckedListBox.CheckedListBoxItemAccessibleObject(this.CheckedListBox.GetItemText(this.CheckedListBox.Items[index]), index, this);
                }
                return null;
            }

            public override int GetChildCount()
            {
                return this.CheckedListBox.Items.Count;
            }

            public override AccessibleObject GetFocused()
            {
                int focusedIndex = this.CheckedListBox.FocusedIndex;
                if (focusedIndex >= 0)
                {
                    return this.GetChild(focusedIndex);
                }
                return null;
            }

            public override AccessibleObject GetSelected()
            {
                int selectedIndex = this.CheckedListBox.SelectedIndex;
                if (selectedIndex >= 0)
                {
                    return this.GetChild(selectedIndex);
                }
                return null;
            }

            public override AccessibleObject HitTest(int x, int y)
            {
                int childCount = this.GetChildCount();
                for (int i = 0; i < childCount; i++)
                {
                    AccessibleObject child = this.GetChild(i);
                    if (child.Bounds.Contains(x, y))
                    {
                        return child;
                    }
                }
                if (this.Bounds.Contains(x, y))
                {
                    return this;
                }
                return null;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation direction)
            {
                if (this.GetChildCount() > 0)
                {
                    if (direction == AccessibleNavigation.FirstChild)
                    {
                        return this.GetChild(0);
                    }
                    if (direction == AccessibleNavigation.LastChild)
                    {
                        return this.GetChild(this.GetChildCount() - 1);
                    }
                }
                return base.Navigate(direction);
            }

            private System.Windows.Forms.CheckedListBox CheckedListBox
            {
                get
                {
                    return (System.Windows.Forms.CheckedListBox) base.Owner;
                }
            }
        }

        [ComVisible(true)]
        internal class CheckedListBoxItemAccessibleObject : AccessibleObject
        {
            private int index;
            private string name;
            private CheckedListBox.CheckedListBoxAccessibleObject parent;

            public CheckedListBoxItemAccessibleObject(string name, int index, CheckedListBox.CheckedListBoxAccessibleObject parent)
            {
                this.name = name;
                this.parent = parent;
                this.index = index;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                this.ParentCheckedListBox.SetItemChecked(this.index, !this.ParentCheckedListBox.GetItemChecked(this.index));
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation direction)
            {
                if (((direction == AccessibleNavigation.Down) || (direction == AccessibleNavigation.Next)) && (this.index < (this.parent.GetChildCount() - 1)))
                {
                    return this.parent.GetChild(this.index + 1);
                }
                if (((direction == AccessibleNavigation.Up) || (direction == AccessibleNavigation.Previous)) && (this.index > 0))
                {
                    return this.parent.GetChild(this.index - 1);
                }
                return base.Navigate(direction);
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void Select(AccessibleSelection flags)
            {
                try
                {
                    this.ParentCheckedListBox.AccessibilityObject.GetSystemIAccessibleInternal().accSelect((int) flags, this.index + 1);
                }
                catch (ArgumentException)
                {
                }
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle itemRectangle = this.ParentCheckedListBox.GetItemRectangle(this.index);
                    System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT(itemRectangle.X, itemRectangle.Y);
                    System.Windows.Forms.UnsafeNativeMethods.ClientToScreen(new HandleRef(this.ParentCheckedListBox, this.ParentCheckedListBox.Handle), pt);
                    return new Rectangle(pt.x, pt.y, itemRectangle.Width, itemRectangle.Height);
                }
            }

            public override string DefaultAction
            {
                get
                {
                    if (this.ParentCheckedListBox.GetItemChecked(this.index))
                    {
                        return System.Windows.Forms.SR.GetString("AccessibleActionUncheck");
                    }
                    return System.Windows.Forms.SR.GetString("AccessibleActionCheck");
                }
            }

            public override string Name
            {
                get
                {
                    return this.name;
                }
                set
                {
                    this.name = value;
                }
            }

            public override AccessibleObject Parent
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.parent;
                }
            }

            private CheckedListBox ParentCheckedListBox
            {
                get
                {
                    return (CheckedListBox) this.parent.Owner;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.CheckButton;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates states = AccessibleStates.Selectable | AccessibleStates.Focusable;
                    switch (this.ParentCheckedListBox.GetItemCheckState(this.index))
                    {
                        case CheckState.Checked:
                            states |= AccessibleStates.Checked;
                            break;

                        case CheckState.Indeterminate:
                            states |= AccessibleStates.Indeterminate;
                            break;
                    }
                    if (this.ParentCheckedListBox.SelectedIndex == this.index)
                    {
                        states |= AccessibleStates.Focused | AccessibleStates.Selected;
                    }
                    return states;
                }
            }

            public override string Value
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.ParentCheckedListBox.GetItemChecked(this.index).ToString();
                }
            }
        }

        public class ObjectCollection : ListBox.ObjectCollection
        {
            private CheckedListBox owner;

            public ObjectCollection(CheckedListBox owner) : base(owner)
            {
                this.owner = owner;
            }

            public int Add(object item, bool isChecked)
            {
                return this.Add(item, isChecked ? CheckState.Checked : CheckState.Unchecked);
            }

            public int Add(object item, CheckState check)
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(check, (int) check, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) check, typeof(CheckState));
                }
                int index = base.Add(item);
                this.owner.SetItemCheckState(index, check);
                return index;
            }
        }
    }
}

