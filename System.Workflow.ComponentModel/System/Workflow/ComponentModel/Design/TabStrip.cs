namespace System.Workflow.ComponentModel.Design
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    [ToolboxItem(false)]
    internal sealed class TabStrip : Control
    {
        private ToolTip buttonTips;
        private DrawTabItemStruct[] drawItems;
        private const int MinSize = 0x12;
        private Orientation orientation;
        private int reqTabItemSize;
        private int selectedTab = -1;
        private ItemList<System.Workflow.ComponentModel.Design.ItemInfo> tabItemList;
        private const int TabMargin = 1;

        public event SelectionChangeEventHandler<TabSelectionChangeEventArgs> TabChange;

        public TabStrip(Orientation orientation, int tabSize)
        {
            base.SuspendLayout();
            this.orientation = orientation;
            this.reqTabItemSize = Math.Max(tabSize, 0x12);
            this.Font = new Font(this.Font.FontFamily, (float) ((this.reqTabItemSize * 2) / 3), GraphicsUnit.Pixel);
            this.tabItemList = new ItemList<System.Workflow.ComponentModel.Design.ItemInfo>(this);
            this.tabItemList.ListChanging += new ItemListChangeEventHandler<System.Workflow.ComponentModel.Design.ItemInfo>(this.OnItemsChanging);
            this.tabItemList.ListChanged += new ItemListChangeEventHandler<System.Workflow.ComponentModel.Design.ItemInfo>(this.OnItemsChanged);
            this.buttonTips = new ToolTip();
            this.buttonTips.ShowAlways = true;
            this.buttonTips.SetToolTip(this, string.Empty);
            this.BackColor = SystemColors.Control;
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.Selectable | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            base.ResumeLayout();
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.SystemEvents_UserPreferenceChanged);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.SystemEvents_UserPreferenceChanged);
            }
            base.Dispose(disposing);
        }

        private Rectangle GetTabImageRectangle(System.Workflow.ComponentModel.Design.ItemInfo tabItemInfo)
        {
            int index = this.tabItemList.IndexOf(tabItemInfo);
            if (index < 0)
            {
                throw new ArgumentException(DR.GetString("ButtonInformationMissing", new object[0]));
            }
            Rectangle empty = Rectangle.Empty;
            if ((tabItemInfo.Image != null) && (this.drawItems.Length == this.tabItemList.Count))
            {
                empty = this.drawItems[index].TabItemRectangle;
                empty.Inflate(-1, -1);
                empty.Size = new Size(this.reqTabItemSize - 2, this.reqTabItemSize - 2);
            }
            return empty;
        }

        private Rectangle GetTabItemRectangle(System.Workflow.ComponentModel.Design.ItemInfo tabItemInfo)
        {
            int index = this.tabItemList.IndexOf(tabItemInfo);
            if (index < 0)
            {
                throw new ArgumentException(DR.GetString("ButtonInformationMissing", new object[0]));
            }
            if (this.drawItems.Length == this.tabItemList.Count)
            {
                return this.drawItems[index].TabItemRectangle;
            }
            return Rectangle.Empty;
        }

        private Rectangle GetTabTextRectangle(System.Workflow.ComponentModel.Design.ItemInfo tabItemInfo)
        {
            int index = this.tabItemList.IndexOf(tabItemInfo);
            if (index < 0)
            {
                throw new ArgumentException(DR.GetString("ButtonInformationMissing", new object[0]));
            }
            Rectangle empty = Rectangle.Empty;
            if ((tabItemInfo.Text == null) || (this.drawItems.Length != this.tabItemList.Count))
            {
                return empty;
            }
            empty = this.drawItems[index].TabItemRectangle;
            empty.Inflate(-1, -1);
            Rectangle tabImageRectangle = this.GetTabImageRectangle(tabItemInfo);
            if (!tabImageRectangle.IsEmpty)
            {
                if (this.orientation == Orientation.Horizontal)
                {
                    empty.X += tabImageRectangle.Width + 1;
                    empty.Width -= tabImageRectangle.Width + 1;
                }
                else
                {
                    empty.Y += tabImageRectangle.Height + 1;
                    empty.Height -= tabImageRectangle.Height + 1;
                }
            }
            if ((empty.Width > 0) && (empty.Height > 0))
            {
                return empty;
            }
            return Rectangle.Empty;
        }

        private void OnItemsChanged(object sender, ItemListChangeEventArgs<System.Workflow.ComponentModel.Design.ItemInfo> e)
        {
            if (this.tabItemList.Count == 0)
            {
                this.selectedTab = -1;
            }
            else if (this.selectedTab > (this.tabItemList.Count - 1))
            {
                this.SelectedTab = this.tabItemList.Count - 1;
            }
            if (base.Parent != null)
            {
                base.Parent.PerformLayout();
            }
        }

        private void OnItemsChanging(object sender, ItemListChangeEventArgs<System.Workflow.ComponentModel.Design.ItemInfo> e)
        {
            if (e.Action == ItemListChangeAction.Add)
            {
                foreach (System.Workflow.ComponentModel.Design.ItemInfo info in e.AddedItems)
                {
                    if (this.tabItemList.Contains(info))
                    {
                        throw new ArgumentException(DR.GetString("Error_TabExistsWithSameId", new object[0]));
                    }
                }
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            using (Graphics graphics = base.CreateGraphics())
            {
                this.drawItems = new DrawTabItemStruct[this.tabItemList.Count];
                int num = (this.orientation == Orientation.Horizontal) ? base.Width : base.Height;
                bool flag = false;
                if (num <= this.MinimumRequiredSize)
                {
                    flag = true;
                }
                int num2 = 0;
                for (int i = 0; i < this.tabItemList.Count; i++)
                {
                    int num4 = 0;
                    System.Workflow.ComponentModel.Design.ItemInfo info = this.tabItemList[i];
                    if (info.Image != null)
                    {
                        num4++;
                        num4 += this.reqTabItemSize - 2;
                    }
                    if ((info.Text != null) && (info.Text.Length > 0))
                    {
                        SizeF ef = graphics.MeasureString(info.Text, this.Font);
                        int width = Convert.ToInt32(Math.Ceiling((double) ef.Width));
                        this.drawItems[i].TextSize = new Size(width, Convert.ToInt32(Math.Ceiling((double) ef.Height)));
                        if (!flag)
                        {
                            num4 += this.drawItems[i].TextSize.Width + 1;
                        }
                    }
                    num4 += (num4 == 0) ? this.reqTabItemSize : 1;
                    this.drawItems[i].TabItemRectangle = Rectangle.Empty;
                    if (this.orientation == Orientation.Horizontal)
                    {
                        this.drawItems[i].TabItemRectangle.X = num2;
                        this.drawItems[i].TabItemRectangle.Y = 0;
                        this.drawItems[i].TabItemRectangle.Width = num4;
                        this.drawItems[i].TabItemRectangle.Height = this.reqTabItemSize;
                    }
                    else
                    {
                        this.drawItems[i].TabItemRectangle.X = 0;
                        this.drawItems[i].TabItemRectangle.Y = num2;
                        this.drawItems[i].TabItemRectangle.Width = this.reqTabItemSize;
                        this.drawItems[i].TabItemRectangle.Height = num4;
                    }
                    num2 += num4 + 1;
                }
                num2--;
                if (num2 > num)
                {
                    int num5 = (int) Math.Ceiling((double) (((double) (num2 - num)) / ((double) Math.Max(1, this.tabItemList.Count))));
                    num2 = 0;
                    DrawTabItemStruct struct2 = this.drawItems[this.tabItemList.Count - 1];
                    int num6 = (this.orientation == Orientation.Horizontal) ? (struct2.TabItemRectangle.Width - num5) : (struct2.TabItemRectangle.Height - num5);
                    if (num6 < this.reqTabItemSize)
                    {
                        num5 += (int) Math.Ceiling((double) (((double) (this.reqTabItemSize - num6)) / ((double) Math.Max(1, this.tabItemList.Count))));
                    }
                    for (int j = 0; j < this.tabItemList.Count; j++)
                    {
                        if (this.orientation == Orientation.Horizontal)
                        {
                            this.drawItems[j].TabItemRectangle.X -= num2;
                            this.drawItems[j].TabItemRectangle.Width -= num5;
                            if ((j == (this.tabItemList.Count - 1)) && (this.drawItems[j].TabItemRectangle.Width < this.reqTabItemSize))
                            {
                                this.drawItems[j].TabItemRectangle.Width = this.reqTabItemSize;
                            }
                        }
                        else
                        {
                            this.drawItems[j].TabItemRectangle.Y -= num2;
                            this.drawItems[j].TabItemRectangle.Height -= num5;
                            if ((j == (this.tabItemList.Count - 1)) && (this.drawItems[j].TabItemRectangle.Height < this.reqTabItemSize))
                            {
                                this.drawItems[j].TabItemRectangle.Height = this.reqTabItemSize;
                            }
                        }
                        num2 += num5;
                    }
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int num = 0;
            foreach (System.Workflow.ComponentModel.Design.ItemInfo info in this.tabItemList)
            {
                if (this.GetTabItemRectangle(info).Contains(new Point(e.X, e.Y)))
                {
                    this.SelectedTab = num;
                    break;
                }
                num++;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.buttonTips.SetToolTip(this, string.Empty);
            base.Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            foreach (System.Workflow.ComponentModel.Design.ItemInfo info in this.tabItemList)
            {
                if (this.GetTabItemRectangle(info).Contains(new Point(e.X, e.Y)) && (info.Text != this.buttonTips.GetToolTip(this)))
                {
                    this.buttonTips.Active = false;
                    this.buttonTips.SetToolTip(this, info.Text);
                    this.buttonTips.Active = true;
                    break;
                }
            }
            base.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (this.drawItems.Length == this.tabItemList.Count)
            {
                e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                System.Drawing.Color color = System.Drawing.Color.FromArgb(0xff, 0xee, 0xc2);
                System.Drawing.Color color2 = System.Drawing.Color.FromArgb(0xff, 0xc0, 0x6f);
                if (SystemInformation.HighContrast)
                {
                    color = System.Drawing.Color.FromArgb(0xff - color.R, 0xff - color.G, 0xff - color.B);
                    color2 = System.Drawing.Color.FromArgb(0xff - color2.R, 0xff - color2.G, 0xff - color2.B);
                }
                using (Brush brush = new SolidBrush(color))
                {
                    using (Brush brush2 = new SolidBrush(color2))
                    {
                        for (int i = 0; i < this.drawItems.Length; i++)
                        {
                            System.Workflow.ComponentModel.Design.ItemInfo tabItemInfo = this.tabItemList[i];
                            DrawTabItemStruct struct2 = this.drawItems[i];
                            Brush control = SystemBrushes.Control;
                            Rectangle tabItemRectangle = struct2.TabItemRectangle;
                            if (this.selectedTab == i)
                            {
                                control = brush2;
                                e.Graphics.FillRectangle(control, tabItemRectangle);
                                e.Graphics.DrawRectangle(SystemPens.Highlight, tabItemRectangle);
                            }
                            else
                            {
                                Point pt = base.PointToClient(Control.MousePosition);
                                if (tabItemRectangle.Contains(pt))
                                {
                                    control = brush;
                                    e.Graphics.FillRectangle(control, tabItemRectangle);
                                    e.Graphics.DrawRectangle(SystemPens.ControlDarkDark, tabItemRectangle);
                                }
                            }
                            Rectangle tabImageRectangle = this.GetTabImageRectangle(tabItemInfo);
                            if (!tabImageRectangle.IsEmpty)
                            {
                                e.Graphics.DrawImage(tabItemInfo.Image, tabImageRectangle);
                            }
                            Rectangle tabTextRectangle = this.GetTabTextRectangle(tabItemInfo);
                            if (!tabTextRectangle.IsEmpty)
                            {
                                StringFormat format = new StringFormat {
                                    Alignment = StringAlignment.Center,
                                    LineAlignment = StringAlignment.Center,
                                    Trimming = StringTrimming.EllipsisCharacter
                                };
                                if (this.orientation == Orientation.Horizontal)
                                {
                                    RectangleF layoutRectangle = new RectangleF((float) tabTextRectangle.X, (float) tabTextRectangle.Y, (float) tabTextRectangle.Width, (float) tabTextRectangle.Height);
                                    e.Graphics.DrawString(tabItemInfo.Text, this.Font, SystemBrushes.ControlText, layoutRectangle, format);
                                }
                                else
                                {
                                    using (Bitmap bitmap = new Bitmap(tabTextRectangle.Height, tabTextRectangle.Width, e.Graphics))
                                    {
                                        using (Graphics graphics = Graphics.FromImage(bitmap))
                                        {
                                            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                                            graphics.FillRectangle(control, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                                            graphics.DrawString(this.tabItemList[i].Text, this.Font, SystemBrushes.ControlText, new Rectangle(0, 0, bitmap.Width, bitmap.Height), format);
                                            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                            e.Graphics.DrawImage(bitmap, tabTextRectangle);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            this.buttonTips.BackColor = SystemColors.Info;
            this.buttonTips.ForeColor = SystemColors.InfoText;
        }

        public int MaximumRequiredSize
        {
            get
            {
                int num = 0;
                if (this.tabItemList.Count == this.drawItems.Length)
                {
                    for (int i = 0; i < this.tabItemList.Count; i++)
                    {
                        System.Workflow.ComponentModel.Design.ItemInfo info = this.tabItemList[i];
                        int num3 = 0;
                        if (info.Image != null)
                        {
                            num3++;
                            num3 += this.reqTabItemSize;
                        }
                        if ((info.Text != null) && (info.Text.Length > 0))
                        {
                            num3++;
                            num3 += this.drawItems[i].TextSize.Width;
                        }
                        num3 += (num3 == 0) ? this.reqTabItemSize : 1;
                        num += num3;
                    }
                }
                return num;
            }
        }

        public int MinimumRequiredSize
        {
            get
            {
                int num = 0;
                for (int i = 0; i < this.tabItemList.Count; i++)
                {
                    num += 1 + this.reqTabItemSize;
                }
                return num;
            }
        }

        public int SelectedTab
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.selectedTab;
            }
            set
            {
                if ((value >= 0) && (value <= this.tabItemList.Count))
                {
                    System.Workflow.ComponentModel.Design.ItemInfo previousItem = ((this.selectedTab >= 0) && (this.selectedTab < this.tabItemList.Count)) ? this.tabItemList[this.selectedTab] : null;
                    System.Workflow.ComponentModel.Design.ItemInfo tabItemInfo = this.tabItemList[value];
                    this.selectedTab = value;
                    base.Invalidate();
                    if (this.TabChange != null)
                    {
                        Rectangle tabItemRectangle = this.GetTabItemRectangle(tabItemInfo);
                        Point location = base.PointToScreen(tabItemRectangle.Location);
                        this.TabChange(this, new TabSelectionChangeEventArgs(previousItem, tabItemInfo, new Rectangle(location, tabItemRectangle.Size)));
                    }
                }
            }
        }

        public IList<System.Workflow.ComponentModel.Design.ItemInfo> Tabs
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.tabItemList;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DrawTabItemStruct
        {
            public Rectangle TabItemRectangle;
            public Size TextSize;
        }
    }
}

