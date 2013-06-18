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
    using System.Windows.Forms.Layout;

    [System.Windows.Forms.SRDescription("DescriptionDomainUpDown"), ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true), DefaultBindingProperty("SelectedItem"), DefaultProperty("Items"), DefaultEvent("SelectedItemChanged")]
    public class DomainUpDown : UpDownBase
    {
        private static readonly string DefaultValue = "";
        private static readonly bool DefaultWrap = false;
        private int domainIndex = -1;
        private DomainUpDownItemCollection domainItems;
        private bool inSort;
        private bool sorted;
        private string stringValue = DefaultValue;
        private bool wrap = DefaultWrap;

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

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("DomainUpDownOnSelectedItemChangedDescr")]
        public event EventHandler SelectedItemChanged;

        public DomainUpDown()
        {
            base.SetState2(0x800, true);
            this.Text = string.Empty;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new DomainUpDownAccessibleObject(this);
        }

        public override void DownButton()
        {
            if ((this.domainItems != null) && (this.domainItems.Count > 0))
            {
                int index = -1;
                if (base.UserEdit)
                {
                    index = this.MatchIndex(this.Text, false, this.domainIndex);
                }
                if (index != -1)
                {
                    this.SelectIndex(index);
                }
                else if (this.domainIndex < (this.domainItems.Count - 1))
                {
                    this.SelectIndex(this.domainIndex + 1);
                }
                else if (this.Wrap)
                {
                    this.SelectIndex(0);
                }
            }
        }

        internal override Size GetPreferredSizeCore(Size proposedConstraints)
        {
            int preferredHeight = base.PreferredHeight;
            int width = LayoutUtils.OldGetLargestStringSizeInCollection(this.Font, this.Items).Width;
            width = base.SizeFromClientSize(width, preferredHeight).Width + base.upDownButtons.Width;
            return (new Size(width, preferredHeight) + this.Padding.Size);
        }

        internal int MatchIndex(string text, bool complete)
        {
            return this.MatchIndex(text, complete, this.domainIndex);
        }

        internal int MatchIndex(string text, bool complete, int startPosition)
        {
            if (this.domainItems == null)
            {
                return -1;
            }
            if (text.Length < 1)
            {
                return -1;
            }
            if (this.domainItems.Count <= 0)
            {
                return -1;
            }
            if (startPosition < 0)
            {
                startPosition = this.domainItems.Count - 1;
            }
            if (startPosition >= this.domainItems.Count)
            {
                startPosition = 0;
            }
            int num = startPosition;
            int num2 = -1;
            bool flag = false;
            if (!complete)
            {
                text = text.ToUpper(CultureInfo.InvariantCulture);
            }
            do
            {
                if (complete)
                {
                    flag = this.Items[num].ToString().Equals(text);
                }
                else
                {
                    flag = this.Items[num].ToString().ToUpper(CultureInfo.InvariantCulture).StartsWith(text);
                }
                if (flag)
                {
                    num2 = num;
                }
                num++;
                if (num >= this.domainItems.Count)
                {
                    num = 0;
                }
            }
            while (!flag && (num != startPosition));
            return num2;
        }

        protected override void OnChanged(object source, EventArgs e)
        {
            this.OnSelectedItemChanged(source, e);
        }

        protected void OnSelectedItemChanged(object source, EventArgs e)
        {
            if (this.onSelectedItemChanged != null)
            {
                this.onSelectedItemChanged(this, e);
            }
        }

        protected override void OnTextBoxKeyPress(object source, KeyPressEventArgs e)
        {
            if (base.ReadOnly)
            {
                char[] chArray = new char[] { e.KeyChar };
                switch (char.GetUnicodeCategory(chArray[0]))
                {
                    case UnicodeCategory.LetterNumber:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.MathSymbol:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.OtherNumber:
                    case UnicodeCategory.UppercaseLetter:
                    {
                        int index = this.MatchIndex(new string(chArray), false, this.domainIndex + 1);
                        if (index != -1)
                        {
                            this.SelectIndex(index);
                        }
                        e.Handled = true;
                        break;
                    }
                }
            }
            base.OnTextBoxKeyPress(source, e);
        }

        private void SelectIndex(int index)
        {
            if (((this.domainItems == null) || (index < -1)) || (index >= this.domainItems.Count))
            {
                index = -1;
            }
            else
            {
                this.domainIndex = index;
                if (this.domainIndex >= 0)
                {
                    this.stringValue = this.domainItems[this.domainIndex].ToString();
                    base.UserEdit = false;
                    this.UpdateEditText();
                }
                else
                {
                    base.UserEdit = true;
                }
            }
        }

        private void SortDomainItems()
        {
            if (!this.inSort)
            {
                this.inSort = true;
                try
                {
                    if (this.sorted && (this.domainItems != null))
                    {
                        ArrayList.Adapter(this.domainItems).Sort(new DomainUpDownItemCompare());
                        if (!base.UserEdit)
                        {
                            int index = this.MatchIndex(this.stringValue, true);
                            if (index != -1)
                            {
                                this.SelectIndex(index);
                            }
                        }
                    }
                }
                finally
                {
                    this.inSort = false;
                }
            }
        }

        public override string ToString()
        {
            string str = base.ToString();
            if (this.Items != null)
            {
                str = (str + ", Items.Count: " + this.Items.Count.ToString(CultureInfo.CurrentCulture)) + ", SelectedIndex: " + this.SelectedIndex.ToString(CultureInfo.CurrentCulture);
            }
            return str;
        }

        public override void UpButton()
        {
            if (((this.domainItems != null) && (this.domainItems.Count > 0)) && (this.domainIndex != -1))
            {
                int index = -1;
                if (base.UserEdit)
                {
                    index = this.MatchIndex(this.Text, false, this.domainIndex);
                }
                if (index != -1)
                {
                    this.SelectIndex(index);
                }
                else if (this.domainIndex > 0)
                {
                    this.SelectIndex(this.domainIndex - 1);
                }
                else if (this.Wrap)
                {
                    this.SelectIndex(this.domainItems.Count - 1);
                }
            }
        }

        protected override void UpdateEditText()
        {
            base.UserEdit = false;
            base.ChangingText = true;
            this.Text = this.stringValue;
        }

        [System.Windows.Forms.SRCategory("CatData"), Localizable(true), Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.Windows.Forms.SRDescription("DomainUpDownItemsDescr")]
        public DomainUpDownItemCollection Items
        {
            get
            {
                if (this.domainItems == null)
                {
                    this.domainItems = new DomainUpDownItemCollection(this);
                }
                return this.domainItems;
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

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("DomainUpDownSelectedIndexDescr"), DefaultValue(-1), Browsable(false)]
        public int SelectedIndex
        {
            get
            {
                if (base.UserEdit)
                {
                    return -1;
                }
                return this.domainIndex;
            }
            set
            {
                if ((value < -1) || (value >= this.Items.Count))
                {
                    throw new ArgumentOutOfRangeException("SelectedIndex", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "SelectedIndex", value.ToString(CultureInfo.CurrentCulture) }));
                }
                if (value != this.SelectedIndex)
                {
                    this.SelectIndex(value);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("DomainUpDownSelectedItemDescr"), Browsable(false)]
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
                if (value == null)
                {
                    this.SelectedIndex = -1;
                }
                else
                {
                    for (int i = 0; i < this.Items.Count; i++)
                    {
                        if ((value != null) && value.Equals(this.Items[i]))
                        {
                            this.SelectedIndex = i;
                            return;
                        }
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("DomainUpDownSortedDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool Sorted
        {
            get
            {
                return this.sorted;
            }
            set
            {
                this.sorted = value;
                if (this.sorted)
                {
                    this.SortDomainItems();
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("DomainUpDownWrapDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool Wrap
        {
            get
            {
                return this.wrap;
            }
            set
            {
                this.wrap = value;
            }
        }

        [ComVisible(true)]
        public class DomainItemAccessibleObject : AccessibleObject
        {
            private string name;
            private DomainUpDown.DomainItemListAccessibleObject parent;

            public DomainItemAccessibleObject(string name, AccessibleObject parent)
            {
                this.name = name;
                this.parent = (DomainUpDown.DomainItemListAccessibleObject) parent;
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

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.ListItem;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    return AccessibleStates.Selectable;
                }
            }

            public override string Value
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.name;
                }
            }
        }

        internal class DomainItemListAccessibleObject : AccessibleObject
        {
            private DomainUpDown.DomainUpDownAccessibleObject parent;

            public DomainItemListAccessibleObject(DomainUpDown.DomainUpDownAccessibleObject parent)
            {
                this.parent = parent;
            }

            public override AccessibleObject GetChild(int index)
            {
                if ((index >= 0) && (index < this.GetChildCount()))
                {
                    return new DomainUpDown.DomainItemAccessibleObject(((DomainUpDown) this.parent.Owner).Items[index].ToString(), this);
                }
                return null;
            }

            public override int GetChildCount()
            {
                return ((DomainUpDown) this.parent.Owner).Items.Count;
            }

            public override string Name
            {
                get
                {
                    string name = base.Name;
                    if ((name != null) && (name.Length != 0))
                    {
                        return name;
                    }
                    return "Items";
                }
                set
                {
                    base.Name = value;
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

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.List;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    return (AccessibleStates.Offscreen | AccessibleStates.Invisible);
                }
            }
        }

        [ComVisible(true)]
        public class DomainUpDownAccessibleObject : Control.ControlAccessibleObject
        {
            private DomainUpDown.DomainItemListAccessibleObject itemList;

            public DomainUpDownAccessibleObject(Control owner) : base(owner)
            {
            }

            public override AccessibleObject GetChild(int index)
            {
                switch (index)
                {
                    case 0:
                        return ((UpDownBase) base.Owner).TextBox.AccessibilityObject.Parent;

                    case 1:
                        return ((UpDownBase) base.Owner).UpDownButtonsInternal.AccessibilityObject.Parent;

                    case 2:
                        return this.ItemList;
                }
                return null;
            }

            public override int GetChildCount()
            {
                return 3;
            }

            private DomainUpDown.DomainItemListAccessibleObject ItemList
            {
                get
                {
                    if (this.itemList == null)
                    {
                        this.itemList = new DomainUpDown.DomainItemListAccessibleObject(this);
                    }
                    return this.itemList;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    AccessibleRole accessibleRole = base.Owner.AccessibleRole;
                    if (accessibleRole != AccessibleRole.Default)
                    {
                        return accessibleRole;
                    }
                    return AccessibleRole.ComboBox;
                }
            }
        }

        public class DomainUpDownItemCollection : ArrayList
        {
            private DomainUpDown owner;

            internal DomainUpDownItemCollection(DomainUpDown owner)
            {
                this.owner = owner;
            }

            public override int Add(object item)
            {
                int num = base.Add(item);
                if (this.owner.Sorted)
                {
                    this.owner.SortDomainItems();
                }
                return num;
            }

            public override void Insert(int index, object item)
            {
                base.Insert(index, item);
                if (this.owner.Sorted)
                {
                    this.owner.SortDomainItems();
                }
            }

            public override void Remove(object item)
            {
                int index = this.IndexOf(item);
                if (index == -1)
                {
                    throw new ArgumentOutOfRangeException("item", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "item", item.ToString() }));
                }
                this.RemoveAt(index);
            }

            public override void RemoveAt(int item)
            {
                base.RemoveAt(item);
                if (item < this.owner.domainIndex)
                {
                    this.owner.SelectIndex(this.owner.domainIndex - 1);
                }
                else if (item == this.owner.domainIndex)
                {
                    this.owner.SelectIndex(-1);
                }
            }

            [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public override object this[int index]
            {
                get
                {
                    return base[index];
                }
                set
                {
                    base[index] = value;
                    if (this.owner.SelectedIndex == index)
                    {
                        this.owner.SelectIndex(index);
                    }
                    if (this.owner.Sorted)
                    {
                        this.owner.SortDomainItems();
                    }
                }
            }
        }

        private sealed class DomainUpDownItemCompare : IComparer
        {
            public int Compare(object p, object q)
            {
                if ((p != q) && ((p != null) && (q != null)))
                {
                    return string.Compare(p.ToString(), q.ToString(), false, CultureInfo.CurrentCulture);
                }
                return 0;
            }
        }
    }
}

