namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Designer("System.Windows.Forms.Design.BindingNavigatorDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("RefreshItems"), System.Windows.Forms.SRDescription("DescriptionBindingNavigator"), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultProperty("BindingSource")]
    public class BindingNavigator : ToolStrip, ISupportInitialize
    {
        private ToolStripItem addNewItem;
        private bool addNewItemUserEnabled;
        private System.Windows.Forms.BindingSource bindingSource;
        private ToolStripItem countItem;
        private string countItemFormat;
        private ToolStripItem deleteItem;
        private bool deleteItemUserEnabled;
        private bool initializing;
        private ToolStripItem moveFirstItem;
        private ToolStripItem moveLastItem;
        private ToolStripItem moveNextItem;
        private ToolStripItem movePreviousItem;
        private ToolStripItem positionItem;

        [System.Windows.Forms.SRDescription("BindingNavigatorRefreshItemsEventDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler RefreshItems;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public BindingNavigator() : this(false)
        {
        }

        public BindingNavigator(bool addStandardItems)
        {
            this.countItemFormat = System.Windows.Forms.SR.GetString("BindingNavigatorCountItemFormat");
            this.addNewItemUserEnabled = true;
            this.deleteItemUserEnabled = true;
            if (addStandardItems)
            {
                this.AddStandardItems();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public BindingNavigator(IContainer container) : this(false)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            container.Add(this);
        }

        public BindingNavigator(System.Windows.Forms.BindingSource bindingSource) : this(true)
        {
            this.BindingSource = bindingSource;
        }

        private void AcceptNewPosition()
        {
            if ((this.positionItem != null) && (this.bindingSource != null))
            {
                int position = this.bindingSource.Position;
                try
                {
                    position = Convert.ToInt32(this.positionItem.Text, CultureInfo.CurrentCulture) - 1;
                }
                catch (FormatException)
                {
                }
                catch (OverflowException)
                {
                }
                if (position != this.bindingSource.Position)
                {
                    this.bindingSource.Position = position;
                }
                this.RefreshItemsInternal();
            }
        }

        public virtual void AddStandardItems()
        {
            this.MoveFirstItem = new ToolStripButton();
            this.MovePreviousItem = new ToolStripButton();
            this.MoveNextItem = new ToolStripButton();
            this.MoveLastItem = new ToolStripButton();
            this.PositionItem = new ToolStripTextBox();
            this.CountItem = new ToolStripLabel();
            this.AddNewItem = new ToolStripButton();
            this.DeleteItem = new ToolStripButton();
            ToolStripSeparator separator = new ToolStripSeparator();
            ToolStripSeparator separator2 = new ToolStripSeparator();
            ToolStripSeparator separator3 = new ToolStripSeparator();
            char ch = (string.IsNullOrEmpty(base.Name) || char.IsLower(base.Name[0])) ? 'b' : 'B';
            this.MoveFirstItem.Name = ch + "indingNavigatorMoveFirstItem";
            this.MovePreviousItem.Name = ch + "indingNavigatorMovePreviousItem";
            this.MoveNextItem.Name = ch + "indingNavigatorMoveNextItem";
            this.MoveLastItem.Name = ch + "indingNavigatorMoveLastItem";
            this.PositionItem.Name = ch + "indingNavigatorPositionItem";
            this.CountItem.Name = ch + "indingNavigatorCountItem";
            this.AddNewItem.Name = ch + "indingNavigatorAddNewItem";
            this.DeleteItem.Name = ch + "indingNavigatorDeleteItem";
            separator.Name = ch + "indingNavigatorSeparator";
            separator2.Name = ch + "indingNavigatorSeparator";
            separator3.Name = ch + "indingNavigatorSeparator";
            this.MoveFirstItem.Text = System.Windows.Forms.SR.GetString("BindingNavigatorMoveFirstItemText");
            this.MovePreviousItem.Text = System.Windows.Forms.SR.GetString("BindingNavigatorMovePreviousItemText");
            this.MoveNextItem.Text = System.Windows.Forms.SR.GetString("BindingNavigatorMoveNextItemText");
            this.MoveLastItem.Text = System.Windows.Forms.SR.GetString("BindingNavigatorMoveLastItemText");
            this.AddNewItem.Text = System.Windows.Forms.SR.GetString("BindingNavigatorAddNewItemText");
            this.DeleteItem.Text = System.Windows.Forms.SR.GetString("BindingNavigatorDeleteItemText");
            this.CountItem.ToolTipText = System.Windows.Forms.SR.GetString("BindingNavigatorCountItemTip");
            this.PositionItem.ToolTipText = System.Windows.Forms.SR.GetString("BindingNavigatorPositionItemTip");
            this.CountItem.AutoToolTip = false;
            this.PositionItem.AutoToolTip = false;
            this.PositionItem.AccessibleName = System.Windows.Forms.SR.GetString("BindingNavigatorPositionAccessibleName");
            Bitmap bitmap = new Bitmap(typeof(BindingNavigator), "BindingNavigator.MoveFirst.bmp");
            Bitmap bitmap2 = new Bitmap(typeof(BindingNavigator), "BindingNavigator.MovePrevious.bmp");
            Bitmap bitmap3 = new Bitmap(typeof(BindingNavigator), "BindingNavigator.MoveNext.bmp");
            Bitmap bitmap4 = new Bitmap(typeof(BindingNavigator), "BindingNavigator.MoveLast.bmp");
            Bitmap bitmap5 = new Bitmap(typeof(BindingNavigator), "BindingNavigator.AddNew.bmp");
            Bitmap bitmap6 = new Bitmap(typeof(BindingNavigator), "BindingNavigator.Delete.bmp");
            bitmap.MakeTransparent(Color.Magenta);
            bitmap2.MakeTransparent(Color.Magenta);
            bitmap3.MakeTransparent(Color.Magenta);
            bitmap4.MakeTransparent(Color.Magenta);
            bitmap5.MakeTransparent(Color.Magenta);
            bitmap6.MakeTransparent(Color.Magenta);
            this.MoveFirstItem.Image = bitmap;
            this.MovePreviousItem.Image = bitmap2;
            this.MoveNextItem.Image = bitmap3;
            this.MoveLastItem.Image = bitmap4;
            this.AddNewItem.Image = bitmap5;
            this.DeleteItem.Image = bitmap6;
            this.MoveFirstItem.RightToLeftAutoMirrorImage = true;
            this.MovePreviousItem.RightToLeftAutoMirrorImage = true;
            this.MoveNextItem.RightToLeftAutoMirrorImage = true;
            this.MoveLastItem.RightToLeftAutoMirrorImage = true;
            this.AddNewItem.RightToLeftAutoMirrorImage = true;
            this.DeleteItem.RightToLeftAutoMirrorImage = true;
            this.MoveFirstItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.MovePreviousItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.MoveNextItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.MoveLastItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.AddNewItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.DeleteItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.PositionItem.AutoSize = false;
            this.PositionItem.Width = 50;
            this.Items.AddRange(new ToolStripItem[] { this.MoveFirstItem, this.MovePreviousItem, separator, this.PositionItem, this.CountItem, separator2, this.MoveNextItem, this.MoveLastItem, separator3, this.AddNewItem, this.DeleteItem });
        }

        public void BeginInit()
        {
            this.initializing = true;
        }

        private void CancelNewPosition()
        {
            this.RefreshItemsInternal();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.BindingSource = null;
            }
            base.Dispose(disposing);
        }

        public void EndInit()
        {
            this.initializing = false;
            this.RefreshItemsInternal();
        }

        private void OnAddNew(object sender, EventArgs e)
        {
            if (this.Validate() && (this.bindingSource != null))
            {
                this.bindingSource.AddNew();
                this.RefreshItemsInternal();
            }
        }

        private void OnAddNewItemEnabledChanged(object sender, EventArgs e)
        {
            if (this.AddNewItem != null)
            {
                this.addNewItemUserEnabled = this.addNewItem.Enabled;
            }
        }

        private void OnBindingSourceListChanged(object sender, ListChangedEventArgs e)
        {
            this.RefreshItemsInternal();
        }

        private void OnBindingSourceStateChanged(object sender, EventArgs e)
        {
            this.RefreshItemsInternal();
        }

        private void OnDelete(object sender, EventArgs e)
        {
            if (this.Validate() && (this.bindingSource != null))
            {
                this.bindingSource.RemoveCurrent();
                this.RefreshItemsInternal();
            }
        }

        private void OnDeleteItemEnabledChanged(object sender, EventArgs e)
        {
            if (this.DeleteItem != null)
            {
                this.deleteItemUserEnabled = this.deleteItem.Enabled;
            }
        }

        private void OnMoveFirst(object sender, EventArgs e)
        {
            if (this.Validate() && (this.bindingSource != null))
            {
                this.bindingSource.MoveFirst();
                this.RefreshItemsInternal();
            }
        }

        private void OnMoveLast(object sender, EventArgs e)
        {
            if (this.Validate() && (this.bindingSource != null))
            {
                this.bindingSource.MoveLast();
                this.RefreshItemsInternal();
            }
        }

        private void OnMoveNext(object sender, EventArgs e)
        {
            if (this.Validate() && (this.bindingSource != null))
            {
                this.bindingSource.MoveNext();
                this.RefreshItemsInternal();
            }
        }

        private void OnMovePrevious(object sender, EventArgs e)
        {
            if (this.Validate() && (this.bindingSource != null))
            {
                this.bindingSource.MovePrevious();
                this.RefreshItemsInternal();
            }
        }

        private void OnPositionKey(object sender, KeyEventArgs e)
        {
            Keys keyCode = e.KeyCode;
            if (keyCode != Keys.Enter)
            {
                if (keyCode != Keys.Escape)
                {
                    return;
                }
            }
            else
            {
                this.AcceptNewPosition();
                return;
            }
            this.CancelNewPosition();
        }

        private void OnPositionLostFocus(object sender, EventArgs e)
        {
            this.AcceptNewPosition();
        }

        protected virtual void OnRefreshItems()
        {
            this.RefreshItemsCore();
            if (this.onRefreshItems != null)
            {
                this.onRefreshItems(this, EventArgs.Empty);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void RefreshItemsCore()
        {
            int count;
            int num2;
            bool allowNew;
            bool allowRemove;
            if (this.bindingSource == null)
            {
                count = 0;
                num2 = 0;
                allowNew = false;
                allowRemove = false;
            }
            else
            {
                count = this.bindingSource.Count;
                num2 = this.bindingSource.Position + 1;
                allowNew = this.bindingSource.AllowNew;
                allowRemove = this.bindingSource.AllowRemove;
            }
            if (!base.DesignMode)
            {
                if (this.MoveFirstItem != null)
                {
                    this.moveFirstItem.Enabled = num2 > 1;
                }
                if (this.MovePreviousItem != null)
                {
                    this.movePreviousItem.Enabled = num2 > 1;
                }
                if (this.MoveNextItem != null)
                {
                    this.moveNextItem.Enabled = num2 < count;
                }
                if (this.MoveLastItem != null)
                {
                    this.moveLastItem.Enabled = num2 < count;
                }
                if (this.AddNewItem != null)
                {
                    EventHandler handler = new EventHandler(this.OnAddNewItemEnabledChanged);
                    this.addNewItem.InternalEnabledChanged -= handler;
                    this.addNewItem.Enabled = this.addNewItemUserEnabled && allowNew;
                    this.addNewItem.InternalEnabledChanged += handler;
                }
                if (this.DeleteItem != null)
                {
                    EventHandler handler2 = new EventHandler(this.OnDeleteItemEnabledChanged);
                    this.deleteItem.InternalEnabledChanged -= handler2;
                    this.deleteItem.Enabled = (this.deleteItemUserEnabled && allowRemove) && (count > 0);
                    this.deleteItem.InternalEnabledChanged += handler2;
                }
                if (this.PositionItem != null)
                {
                    this.positionItem.Enabled = (num2 > 0) && (count > 0);
                }
                if (this.CountItem != null)
                {
                    this.countItem.Enabled = count > 0;
                }
            }
            if (this.positionItem != null)
            {
                this.positionItem.Text = num2.ToString(CultureInfo.CurrentCulture);
            }
            if (this.countItem != null)
            {
                this.countItem.Text = base.DesignMode ? this.CountItemFormat : string.Format(CultureInfo.CurrentCulture, this.CountItemFormat, new object[] { count });
            }
        }

        private void RefreshItemsInternal()
        {
            if (!this.initializing)
            {
                this.OnRefreshItems();
            }
        }

        private void ResetCountItemFormat()
        {
            this.countItemFormat = System.Windows.Forms.SR.GetString("BindingNavigatorCountItemFormat");
        }

        private bool ShouldSerializeCountItemFormat()
        {
            return (this.countItemFormat != System.Windows.Forms.SR.GetString("BindingNavigatorCountItemFormat"));
        }

        public bool Validate()
        {
            bool flag;
            return base.ValidateActiveControl(out flag);
        }

        private void WireUpBindingSource(ref System.Windows.Forms.BindingSource oldBindingSource, System.Windows.Forms.BindingSource newBindingSource)
        {
            if (oldBindingSource != newBindingSource)
            {
                if (oldBindingSource != null)
                {
                    oldBindingSource.PositionChanged -= new EventHandler(this.OnBindingSourceStateChanged);
                    oldBindingSource.CurrentChanged -= new EventHandler(this.OnBindingSourceStateChanged);
                    oldBindingSource.CurrentItemChanged -= new EventHandler(this.OnBindingSourceStateChanged);
                    oldBindingSource.DataSourceChanged -= new EventHandler(this.OnBindingSourceStateChanged);
                    oldBindingSource.DataMemberChanged -= new EventHandler(this.OnBindingSourceStateChanged);
                    oldBindingSource.ListChanged -= new ListChangedEventHandler(this.OnBindingSourceListChanged);
                }
                if (newBindingSource != null)
                {
                    newBindingSource.PositionChanged += new EventHandler(this.OnBindingSourceStateChanged);
                    newBindingSource.CurrentChanged += new EventHandler(this.OnBindingSourceStateChanged);
                    newBindingSource.CurrentItemChanged += new EventHandler(this.OnBindingSourceStateChanged);
                    newBindingSource.DataSourceChanged += new EventHandler(this.OnBindingSourceStateChanged);
                    newBindingSource.DataMemberChanged += new EventHandler(this.OnBindingSourceStateChanged);
                    newBindingSource.ListChanged += new ListChangedEventHandler(this.OnBindingSourceListChanged);
                }
                oldBindingSource = newBindingSource;
                this.RefreshItemsInternal();
            }
        }

        private void WireUpButton(ref ToolStripItem oldButton, ToolStripItem newButton, EventHandler clickHandler)
        {
            if (oldButton != newButton)
            {
                if (oldButton != null)
                {
                    oldButton.Click -= clickHandler;
                }
                if (newButton != null)
                {
                    newButton.Click += clickHandler;
                }
                oldButton = newButton;
                this.RefreshItemsInternal();
            }
        }

        private void WireUpLabel(ref ToolStripItem oldLabel, ToolStripItem newLabel)
        {
            if (oldLabel != newLabel)
            {
                oldLabel = newLabel;
                this.RefreshItemsInternal();
            }
        }

        private void WireUpTextBox(ref ToolStripItem oldTextBox, ToolStripItem newTextBox, KeyEventHandler keyUpHandler, EventHandler lostFocusHandler)
        {
            if (oldTextBox != newTextBox)
            {
                ToolStripControlHost host = oldTextBox as ToolStripControlHost;
                ToolStripControlHost host2 = newTextBox as ToolStripControlHost;
                if (host != null)
                {
                    host.KeyUp -= keyUpHandler;
                    host.LostFocus -= lostFocusHandler;
                }
                if (host2 != null)
                {
                    host2.KeyUp += keyUpHandler;
                    host2.LostFocus += lostFocusHandler;
                }
                oldTextBox = newTextBox;
                this.RefreshItemsInternal();
            }
        }

        [System.Windows.Forms.SRDescription("BindingNavigatorAddNewItemPropDescr"), TypeConverter(typeof(ReferenceConverter)), System.Windows.Forms.SRCategory("CatItems")]
        public ToolStripItem AddNewItem
        {
            get
            {
                if ((this.addNewItem != null) && this.addNewItem.IsDisposed)
                {
                    this.addNewItem = null;
                }
                return this.addNewItem;
            }
            set
            {
                if ((this.addNewItem != value) && (value != null))
                {
                    value.InternalEnabledChanged += new EventHandler(this.OnAddNewItemEnabledChanged);
                    this.addNewItemUserEnabled = value.Enabled;
                }
                this.WireUpButton(ref this.addNewItem, value, new EventHandler(this.OnAddNew));
            }
        }

        [TypeConverter(typeof(ReferenceConverter)), System.Windows.Forms.SRCategory("CatData"), System.Windows.Forms.SRDescription("BindingNavigatorBindingSourcePropDescr"), DefaultValue((string) null)]
        public System.Windows.Forms.BindingSource BindingSource
        {
            get
            {
                return this.bindingSource;
            }
            set
            {
                this.WireUpBindingSource(ref this.bindingSource, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatItems"), TypeConverter(typeof(ReferenceConverter)), System.Windows.Forms.SRDescription("BindingNavigatorCountItemPropDescr")]
        public ToolStripItem CountItem
        {
            get
            {
                if ((this.countItem != null) && this.countItem.IsDisposed)
                {
                    this.countItem = null;
                }
                return this.countItem;
            }
            set
            {
                this.WireUpLabel(ref this.countItem, value);
            }
        }

        [System.Windows.Forms.SRDescription("BindingNavigatorCountItemFormatPropDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public string CountItemFormat
        {
            get
            {
                return this.countItemFormat;
            }
            set
            {
                if (this.countItemFormat != value)
                {
                    this.countItemFormat = value;
                    this.RefreshItemsInternal();
                }
            }
        }

        [System.Windows.Forms.SRDescription("BindingNavigatorDeleteItemPropDescr"), TypeConverter(typeof(ReferenceConverter)), System.Windows.Forms.SRCategory("CatItems")]
        public ToolStripItem DeleteItem
        {
            get
            {
                if ((this.deleteItem != null) && this.deleteItem.IsDisposed)
                {
                    this.deleteItem = null;
                }
                return this.deleteItem;
            }
            set
            {
                if ((this.deleteItem != value) && (value != null))
                {
                    value.InternalEnabledChanged += new EventHandler(this.OnDeleteItemEnabledChanged);
                    this.deleteItemUserEnabled = value.Enabled;
                }
                this.WireUpButton(ref this.deleteItem, value, new EventHandler(this.OnDelete));
            }
        }

        [System.Windows.Forms.SRDescription("BindingNavigatorMoveFirstItemPropDescr"), System.Windows.Forms.SRCategory("CatItems"), TypeConverter(typeof(ReferenceConverter))]
        public ToolStripItem MoveFirstItem
        {
            get
            {
                if ((this.moveFirstItem != null) && this.moveFirstItem.IsDisposed)
                {
                    this.moveFirstItem = null;
                }
                return this.moveFirstItem;
            }
            set
            {
                this.WireUpButton(ref this.moveFirstItem, value, new EventHandler(this.OnMoveFirst));
            }
        }

        [TypeConverter(typeof(ReferenceConverter)), System.Windows.Forms.SRDescription("BindingNavigatorMoveLastItemPropDescr"), System.Windows.Forms.SRCategory("CatItems")]
        public ToolStripItem MoveLastItem
        {
            get
            {
                if ((this.moveLastItem != null) && this.moveLastItem.IsDisposed)
                {
                    this.moveLastItem = null;
                }
                return this.moveLastItem;
            }
            set
            {
                this.WireUpButton(ref this.moveLastItem, value, new EventHandler(this.OnMoveLast));
            }
        }

        [TypeConverter(typeof(ReferenceConverter)), System.Windows.Forms.SRDescription("BindingNavigatorMoveNextItemPropDescr"), System.Windows.Forms.SRCategory("CatItems")]
        public ToolStripItem MoveNextItem
        {
            get
            {
                if ((this.moveNextItem != null) && this.moveNextItem.IsDisposed)
                {
                    this.moveNextItem = null;
                }
                return this.moveNextItem;
            }
            set
            {
                this.WireUpButton(ref this.moveNextItem, value, new EventHandler(this.OnMoveNext));
            }
        }

        [System.Windows.Forms.SRCategory("CatItems"), TypeConverter(typeof(ReferenceConverter)), System.Windows.Forms.SRDescription("BindingNavigatorMovePreviousItemPropDescr")]
        public ToolStripItem MovePreviousItem
        {
            get
            {
                if ((this.movePreviousItem != null) && this.movePreviousItem.IsDisposed)
                {
                    this.movePreviousItem = null;
                }
                return this.movePreviousItem;
            }
            set
            {
                this.WireUpButton(ref this.movePreviousItem, value, new EventHandler(this.OnMovePrevious));
            }
        }

        [System.Windows.Forms.SRDescription("BindingNavigatorPositionItemPropDescr"), TypeConverter(typeof(ReferenceConverter)), System.Windows.Forms.SRCategory("CatItems")]
        public ToolStripItem PositionItem
        {
            get
            {
                if ((this.positionItem != null) && this.positionItem.IsDisposed)
                {
                    this.positionItem = null;
                }
                return this.positionItem;
            }
            set
            {
                this.WireUpTextBox(ref this.positionItem, value, new KeyEventHandler(this.OnPositionKey), new EventHandler(this.OnPositionLostFocus));
            }
        }
    }
}

