namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [DesignTimeVisible(false), ToolboxItem(false), TypeConverter(typeof(ColumnHeaderConverter)), DefaultProperty("Text")]
    public class ColumnHeader : Component, ICloneable
    {
        private int displayIndexInternal;
        private ColumnHeaderImageListIndexer imageIndexer;
        internal int index;
        private System.Windows.Forms.ListView listview;
        internal string name;
        internal string text;
        private HorizontalAlignment textAlign;
        private bool textAlignInitialized;
        private object userData;
        internal int width;

        public ColumnHeader()
        {
            this.index = -1;
            this.width = 60;
            this.displayIndexInternal = -1;
            this.imageIndexer = new ColumnHeaderImageListIndexer(this);
        }

        public ColumnHeader(int imageIndex) : this()
        {
            this.ImageIndex = imageIndex;
        }

        public ColumnHeader(string imageKey) : this()
        {
            this.ImageKey = imageKey;
        }

        public void AutoResize(ColumnHeaderAutoResizeStyle headerAutoResize)
        {
            if ((headerAutoResize < ColumnHeaderAutoResizeStyle.None) || (headerAutoResize > ColumnHeaderAutoResizeStyle.ColumnContent))
            {
                throw new InvalidEnumArgumentException("headerAutoResize", (int) headerAutoResize, typeof(ColumnHeaderAutoResizeStyle));
            }
            if (this.listview != null)
            {
                this.listview.AutoResizeColumn(this.Index, headerAutoResize);
            }
        }

        public object Clone()
        {
            System.Type type = base.GetType();
            ColumnHeader header = null;
            if (type == typeof(ColumnHeader))
            {
                header = new ColumnHeader();
            }
            else
            {
                header = (ColumnHeader) Activator.CreateInstance(type);
            }
            header.text = this.text;
            header.Width = this.width;
            header.textAlign = this.TextAlign;
            return header;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.listview != null))
            {
                int index = this.Index;
                if (index != -1)
                {
                    this.listview.Columns.RemoveAt(index);
                }
            }
            base.Dispose(disposing);
        }

        private void ResetText()
        {
            this.Text = null;
        }

        private void SetDisplayIndices(int[] cols)
        {
            if (this.listview.IsHandleCreated && !this.listview.Disposing)
            {
                UnsafeNativeMethods.SendMessage(new HandleRef(this.listview, this.listview.Handle), 0x103a, cols.Length, cols);
            }
        }

        private bool ShouldSerializeDisplayIndex()
        {
            return (this.DisplayIndex != this.Index);
        }

        private bool ShouldSerializeName()
        {
            return !string.IsNullOrEmpty(this.name);
        }

        internal bool ShouldSerializeText()
        {
            return (this.text != null);
        }

        public override string ToString()
        {
            return ("ColumnHeader: Text: " + this.Text);
        }

        internal int ActualImageIndex_Internal
        {
            get
            {
                int actualIndex = this.imageIndexer.ActualIndex;
                if (((this.ImageList != null) && (this.ImageList.Images != null)) && (actualIndex < this.ImageList.Images.Count))
                {
                    return actualIndex;
                }
                return -1;
            }
        }

        [RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRDescription("ColumnHeaderDisplayIndexDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatBehavior")]
        public int DisplayIndex
        {
            get
            {
                return this.DisplayIndexInternal;
            }
            set
            {
                if (this.listview == null)
                {
                    this.DisplayIndexInternal = value;
                }
                else
                {
                    if ((value < 0) || (value > (this.listview.Columns.Count - 1)))
                    {
                        throw new ArgumentOutOfRangeException("DisplayIndex", System.Windows.Forms.SR.GetString("ColumnHeaderBadDisplayIndex"));
                    }
                    int num = Math.Min(this.DisplayIndexInternal, value);
                    int num2 = Math.Max(this.DisplayIndexInternal, value);
                    int[] cols = new int[this.listview.Columns.Count];
                    bool flag = value > this.DisplayIndexInternal;
                    ColumnHeader header = null;
                    for (int i = 0; i < this.listview.Columns.Count; i++)
                    {
                        ColumnHeader header2 = this.listview.Columns[i];
                        if (header2.DisplayIndex == this.DisplayIndexInternal)
                        {
                            header = header2;
                        }
                        else if ((header2.DisplayIndex >= num) && (header2.DisplayIndex <= num2))
                        {
                            header2.DisplayIndexInternal -= flag ? 1 : -1;
                        }
                        if (i != this.Index)
                        {
                            cols[header2.DisplayIndexInternal] = i;
                        }
                    }
                    header.DisplayIndexInternal = value;
                    cols[header.DisplayIndexInternal] = header.Index;
                    this.SetDisplayIndices(cols);
                }
            }
        }

        internal int DisplayIndexInternal
        {
            get
            {
                return this.displayIndexInternal;
            }
            set
            {
                this.displayIndexInternal = value;
            }
        }

        [TypeConverter(typeof(ImageIndexConverter)), DefaultValue(-1), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), RefreshProperties(RefreshProperties.Repaint), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ImageIndex
        {
            get
            {
                if (((this.imageIndexer.Index != -1) && (this.ImageList != null)) && (this.imageIndexer.Index >= this.ImageList.Images.Count))
                {
                    return (this.ImageList.Images.Count - 1);
                }
                return this.imageIndexer.Index;
            }
            set
            {
                if (value < -1)
                {
                    object[] args = new object[] { "ImageIndex", value.ToString(CultureInfo.CurrentCulture), -1.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("ImageIndex", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                if (this.imageIndexer.Index != value)
                {
                    this.imageIndexer.Index = value;
                    if ((this.ListView != null) && this.ListView.IsHandleCreated)
                    {
                        this.ListView.SetColumnInfo(0x10, this);
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), RefreshProperties(RefreshProperties.Repaint), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), TypeConverter(typeof(ImageKeyConverter)), DefaultValue("")]
        public string ImageKey
        {
            get
            {
                return this.imageIndexer.Key;
            }
            set
            {
                if (value != this.imageIndexer.Key)
                {
                    this.imageIndexer.Key = value;
                    if ((this.ListView != null) && this.ListView.IsHandleCreated)
                    {
                        this.ListView.SetColumnInfo(0x10, this);
                    }
                }
            }
        }

        [Browsable(false)]
        public System.Windows.Forms.ImageList ImageList
        {
            get
            {
                return this.imageIndexer.ImageList;
            }
        }

        [Browsable(false)]
        public int Index
        {
            get
            {
                if (this.listview != null)
                {
                    return this.listview.GetColumnIndex(this);
                }
                return -1;
            }
        }

        [Browsable(false)]
        public System.Windows.Forms.ListView ListView
        {
            get
            {
                return this.listview;
            }
        }

        [System.Windows.Forms.SRDescription("ColumnHeaderNameDescr"), Browsable(false)]
        public string Name
        {
            get
            {
                return WindowsFormsUtils.GetComponentName(this, this.name);
            }
            set
            {
                if (value == null)
                {
                    this.name = "";
                }
                else
                {
                    this.name = value;
                }
                if (this.Site != null)
                {
                    this.Site.Name = value;
                }
            }
        }

        internal System.Windows.Forms.ListView OwnerListview
        {
            get
            {
                return this.listview;
            }
            set
            {
                int width = this.Width;
                this.listview = value;
                this.Width = width;
            }
        }

        [Bindable(true), System.Windows.Forms.SRDescription("ControlTagDescr"), DefaultValue((string) null), TypeConverter(typeof(StringConverter)), System.Windows.Forms.SRCategory("CatData"), Localizable(false)]
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

        [System.Windows.Forms.SRDescription("ColumnCaption"), Localizable(true)]
        public string Text
        {
            get
            {
                if (this.text == null)
                {
                    return "ColumnHeader";
                }
                return this.text;
            }
            set
            {
                if (value == null)
                {
                    this.text = "";
                }
                else
                {
                    this.text = value;
                }
                if (this.listview != null)
                {
                    this.listview.SetColumnInfo(4, this);
                }
            }
        }

        [System.Windows.Forms.SRDescription("ColumnAlignment"), DefaultValue(0), Localizable(true)]
        public HorizontalAlignment TextAlign
        {
            get
            {
                if (!this.textAlignInitialized && (this.listview != null))
                {
                    this.textAlignInitialized = true;
                    if (((this.Index != 0) && (this.listview.RightToLeft == RightToLeft.Yes)) && !this.listview.IsMirrored)
                    {
                        this.textAlign = HorizontalAlignment.Right;
                    }
                }
                return this.textAlign;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(HorizontalAlignment));
                }
                this.textAlign = value;
                if ((this.Index == 0) && (this.textAlign != HorizontalAlignment.Left))
                {
                    this.textAlign = HorizontalAlignment.Left;
                }
                if (this.listview != null)
                {
                    this.listview.SetColumnInfo(1, this);
                    this.listview.Invalidate();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ColumnWidth"), Localizable(true), DefaultValue(60)]
        public int Width
        {
            get
            {
                if (((this.listview != null) && this.listview.IsHandleCreated) && (!this.listview.Disposing && (this.listview.View == View.Details)))
                {
                    IntPtr handle = UnsafeNativeMethods.SendMessage(new HandleRef(this.listview, this.listview.Handle), 0x101f, 0, 0);
                    if (handle != IntPtr.Zero)
                    {
                        int num = (int) UnsafeNativeMethods.SendMessage(new HandleRef(this.listview, handle), 0x1200, 0, 0);
                        if (this.Index < num)
                        {
                            this.width = (int) UnsafeNativeMethods.SendMessage(new HandleRef(this.listview, this.listview.Handle), 0x101d, this.Index, 0);
                        }
                    }
                }
                return this.width;
            }
            set
            {
                this.width = value;
                if (this.listview != null)
                {
                    this.listview.SetColumnWidth(this.Index, ColumnHeaderAutoResizeStyle.None);
                }
            }
        }

        internal int WidthInternal
        {
            get
            {
                return this.width;
            }
        }

        internal class ColumnHeaderImageListIndexer : System.Windows.Forms.ImageList.Indexer
        {
            private ColumnHeader owner;

            public ColumnHeaderImageListIndexer(ColumnHeader ch)
            {
                this.owner = ch;
            }

            public override System.Windows.Forms.ImageList ImageList
            {
                get
                {
                    if ((this.owner != null) && (this.owner.ListView != null))
                    {
                        return this.owner.ListView.SmallImageList;
                    }
                    return null;
                }
                set
                {
                }
            }
        }
    }
}

