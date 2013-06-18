namespace System.Drawing.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ColorEditor : UITypeEditor
    {
        private ColorUI colorUI;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc == null)
                {
                    return value;
                }
                if (this.colorUI == null)
                {
                    this.colorUI = new ColorUI(this);
                }
                this.colorUI.Start(edSvc, value);
                edSvc.DropDownControl(this.colorUI);
                if ((this.colorUI.Value != null) && (((Color) this.colorUI.Value) != Color.Empty))
                {
                    value = this.colorUI.Value;
                }
                this.colorUI.End();
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Value is Color)
            {
                Color color = (Color) e.Value;
                SolidBrush brush = new SolidBrush(color);
                e.Graphics.FillRectangle(brush, e.Bounds);
                brush.Dispose();
            }
        }

        private class ColorPalette : Control
        {
            public const int CELL_SIZE = 0x10;
            public const int CELLS = 0x40;
            public const int CELLS_ACROSS = 8;
            public const int CELLS_CUSTOM = 0x10;
            public const int CELLS_DOWN = 8;
            private ColorEditor.ColorUI colorUI;
            private Color[] customColors;
            private Point focus = new Point(0, 0);
            public const int MARGIN = 8;
            private Color selectedColor;
            private static readonly int[] staticCells = new int[] { 
                0xffffff, 0xc0c0ff, 0xc0e0ff, 0xc0ffff, 0xc0ffc0, 0xffffc0, 0xffc0c0, 0xffc0ff, 0xe0e0e0, 0x8080ff, 0x80c0ff, 0x80ffff, 0x80ff80, 0xffff80, 0xff8080, 0xff80ff, 
                0xc0c0c0, 0xff, 0x80ff, 0xffff, 0xff00, 0xffff00, 0xff0000, 0xff00ff, 0x808080, 0xc0, 0x40c0, 0xc0c0, 0xc000, 0xc0c000, 0xc00000, 0xc000c0, 
                0x404040, 0x80, 0x4080, 0x8080, 0x8000, 0x808000, 0x800000, 0x800080, 0, 0x40, 0x404080, 0x4040, 0x4000, 0x404000, 0x400000, 0x400040
             };
            private Color[] staticColors;

            public event EventHandler Picked;

            public ColorPalette(ColorEditor.ColorUI colorUI, Color[] customColors)
            {
                this.colorUI = colorUI;
                base.SetStyle(ControlStyles.Opaque, true);
                this.BackColor = SystemColors.Control;
                base.Size = new Size(0xca, 0xca);
                this.staticColors = new Color[0x30];
                for (int i = 0; i < staticCells.Length; i++)
                {
                    this.staticColors[i] = ColorTranslator.FromOle(staticCells[i]);
                }
                this.customColors = customColors;
            }

            protected override AccessibleObject CreateAccessibilityInstance()
            {
                return new ColorPaletteAccessibleObject(this);
            }

            private static void FillRectWithCellBounds(int across, int down, ref Rectangle rect)
            {
                rect.X = 8 + (across * 0x18);
                rect.Y = 8 + (down * 0x18);
                rect.Width = 0x10;
                rect.Height = 0x10;
            }

            private static int Get1DFrom2D(Point pt)
            {
                return Get1DFrom2D(pt.X, pt.Y);
            }

            private static int Get1DFrom2D(int x, int y)
            {
                if ((x != -1) && (y != -1))
                {
                    return (x + (8 * y));
                }
                return -1;
            }

            internal static Point Get2DFrom1D(int cell)
            {
                int x = cell % 8;
                return new Point(x, cell / 8);
            }

            private static Point GetCell2DFromLocationMouse(int x, int y)
            {
                int num = x / 0x18;
                int num2 = y / 0x18;
                if ((((num >= 0) && (num2 >= 0)) && ((num < 8) && (num2 < 8))) && (((x - (0x18 * num)) >= 8) && ((y - (0x18 * num2)) >= 8)))
                {
                    return new Point(num, num2);
                }
                return new Point(-1, -1);
            }

            private Point GetCellFromColor(Color c)
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (this.GetColorFromCell(j, i).Equals(c))
                        {
                            return new Point(j, i);
                        }
                    }
                }
                return Point.Empty;
            }

            private static int GetCellFromLocationMouse(int x, int y)
            {
                return Get1DFrom2D(GetCell2DFromLocationMouse(x, y));
            }

            private Color GetColorFromCell(int index)
            {
                if (index < 0x30)
                {
                    return this.staticColors[index];
                }
                return this.customColors[(index - 0x40) + 0x10];
            }

            private Color GetColorFromCell(int across, int down)
            {
                return this.GetColorFromCell(Get1DFrom2D(across, down));
            }

            private void InvalidateFocus()
            {
                Rectangle rect = new Rectangle();
                FillRectWithCellBounds(this.focus.X, this.focus.Y, ref rect);
                base.Invalidate(Rectangle.Inflate(rect, 5, 5));
                System.Drawing.Design.UnsafeNativeMethods.NotifyWinEvent(0x8005, new HandleRef(this, base.Handle), -4, 1 + Get1DFrom2D(this.focus.X, this.focus.Y));
            }

            private void InvalidateSelection()
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (this.SelectedColor.Equals(this.GetColorFromCell(j, i)))
                        {
                            Rectangle rect = new Rectangle();
                            FillRectWithCellBounds(j, i, ref rect);
                            base.Invalidate(Rectangle.Inflate(rect, 5, 5));
                            break;
                        }
                    }
                }
            }

            protected override bool IsInputKey(Keys keyData)
            {
                switch (keyData)
                {
                    case Keys.Left:
                    case Keys.Up:
                    case Keys.Right:
                    case Keys.Down:
                    case Keys.Enter:
                        return true;

                    case Keys.F2:
                        return false;
                }
                return base.IsInputKey(keyData);
            }

            protected virtual void LaunchDialog(int customIndex)
            {
                base.Invalidate();
                this.colorUI.EditorService.CloseDropDown();
                ColorEditor.CustomColorDialog dialog = new ColorEditor.CustomColorDialog();
                IntPtr focus = System.Drawing.Design.UnsafeNativeMethods.GetFocus();
                try
                {
                    if (dialog.ShowDialog() != DialogResult.Cancel)
                    {
                        Color color = dialog.Color;
                        this.customColors[customIndex] = dialog.Color;
                        this.SelectedColor = this.customColors[customIndex];
                        this.OnPicked(EventArgs.Empty);
                    }
                    dialog.Dispose();
                }
                finally
                {
                    if (focus != IntPtr.Zero)
                    {
                        System.Drawing.Design.UnsafeNativeMethods.SetFocus(new HandleRef(null, focus));
                    }
                }
            }

            protected override void OnGotFocus(EventArgs e)
            {
                base.OnGotFocus(e);
                this.InvalidateFocus();
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                base.OnKeyDown(e);
                switch (e.KeyCode)
                {
                    case Keys.Space:
                        this.SelectedColor = this.GetColorFromCell(this.focus.X, this.focus.Y);
                        this.InvalidateFocus();
                        return;

                    case Keys.PageUp:
                    case Keys.Next:
                    case Keys.End:
                    case Keys.Home:
                        break;

                    case Keys.Left:
                        this.SetFocus(new Point(this.focus.X - 1, this.focus.Y));
                        return;

                    case Keys.Up:
                        this.SetFocus(new Point(this.focus.X, this.focus.Y - 1));
                        return;

                    case Keys.Right:
                        this.SetFocus(new Point(this.focus.X + 1, this.focus.Y));
                        return;

                    case Keys.Down:
                        this.SetFocus(new Point(this.focus.X, this.focus.Y + 1));
                        break;

                    case Keys.Enter:
                        this.SelectedColor = this.GetColorFromCell(this.focus.X, this.focus.Y);
                        this.InvalidateFocus();
                        this.OnPicked(EventArgs.Empty);
                        return;

                    default:
                        return;
                }
            }

            protected override void OnLostFocus(EventArgs e)
            {
                base.OnLostFocus(e);
                this.InvalidateFocus();
            }

            protected override void OnMouseDown(MouseEventArgs me)
            {
                base.OnMouseDown(me);
                if (me.Button == MouseButtons.Left)
                {
                    Point newFocus = GetCell2DFromLocationMouse(me.X, me.Y);
                    if (((newFocus.X != -1) && (newFocus.Y != -1)) && (newFocus != this.focus))
                    {
                        this.SetFocus(newFocus);
                    }
                }
            }

            protected override void OnMouseMove(MouseEventArgs me)
            {
                base.OnMouseMove(me);
                if ((me.Button == MouseButtons.Left) && base.Bounds.Contains(me.X, me.Y))
                {
                    Point newFocus = GetCell2DFromLocationMouse(me.X, me.Y);
                    if (((newFocus.X != -1) && (newFocus.Y != -1)) && (newFocus != this.focus))
                    {
                        this.SetFocus(newFocus);
                    }
                }
            }

            protected override void OnMouseUp(MouseEventArgs me)
            {
                base.OnMouseUp(me);
                if (me.Button == MouseButtons.Left)
                {
                    Point newFocus = GetCell2DFromLocationMouse(me.X, me.Y);
                    if ((newFocus.X != -1) && (newFocus.Y != -1))
                    {
                        this.SetFocus(newFocus);
                        this.SelectedColor = this.GetColorFromCell(this.focus.X, this.focus.Y);
                        this.OnPicked(EventArgs.Empty);
                    }
                }
                else if (me.Button == MouseButtons.Right)
                {
                    int cellFromLocationMouse = GetCellFromLocationMouse(me.X, me.Y);
                    if (((cellFromLocationMouse != -1) && (cellFromLocationMouse >= 0x30)) && (cellFromLocationMouse < 0x40))
                    {
                        this.LaunchDialog((cellFromLocationMouse - 0x40) + 0x10);
                    }
                }
            }

            protected override void OnPaint(PaintEventArgs pe)
            {
                Graphics graphics = pe.Graphics;
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    graphics.FillRectangle(brush, base.ClientRectangle);
                }
                Rectangle rect = new Rectangle {
                    Width = 0x10,
                    Height = 0x10,
                    X = 8,
                    Y = 8
                };
                bool flag = false;
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        Color colorFromCell = this.GetColorFromCell(Get1DFrom2D(j, i));
                        FillRectWithCellBounds(j, i, ref rect);
                        if (colorFromCell.Equals(this.SelectedColor) && !flag)
                        {
                            ControlPaint.DrawBorder(graphics, Rectangle.Inflate(rect, 3, 3), SystemColors.ControlText, ButtonBorderStyle.Solid);
                            flag = true;
                        }
                        if (((this.focus.X == j) && (this.focus.Y == i)) && this.Focused)
                        {
                            ControlPaint.DrawFocusRectangle(graphics, Rectangle.Inflate(rect, 5, 5), SystemColors.ControlText, SystemColors.Control);
                        }
                        ControlPaint.DrawBorder(graphics, Rectangle.Inflate(rect, 2, 2), SystemColors.Control, 2, ButtonBorderStyle.Inset, SystemColors.Control, 2, ButtonBorderStyle.Inset, SystemColors.Control, 2, ButtonBorderStyle.Inset, SystemColors.Control, 2, ButtonBorderStyle.Inset);
                        PaintValue(colorFromCell, graphics, rect);
                    }
                }
            }

            protected void OnPicked(EventArgs e)
            {
                if (this.onPicked != null)
                {
                    this.onPicked(this, e);
                }
            }

            private static void PaintValue(Color color, Graphics g, Rectangle rect)
            {
                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, rect);
                }
            }

            protected override bool ProcessDialogKey(Keys keyData)
            {
                if (keyData == Keys.F2)
                {
                    int num = Get1DFrom2D(this.focus.X, this.focus.Y);
                    if ((num >= 0x30) && (num < 0x40))
                    {
                        this.LaunchDialog((num - 0x40) + 0x10);
                        return true;
                    }
                }
                return base.ProcessDialogKey(keyData);
            }

            private void SetFocus(Point newFocus)
            {
                if (newFocus.X < 0)
                {
                    newFocus.X = 0;
                }
                if (newFocus.Y < 0)
                {
                    newFocus.Y = 0;
                }
                if (newFocus.X >= 8)
                {
                    newFocus.X = 7;
                }
                if (newFocus.Y >= 8)
                {
                    newFocus.Y = 7;
                }
                if (this.focus != newFocus)
                {
                    this.InvalidateFocus();
                    this.focus = newFocus;
                    this.InvalidateFocus();
                }
            }

            public Color[] CustomColors
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.customColors;
                }
            }

            internal int FocusedCell
            {
                get
                {
                    return Get1DFrom2D(this.focus);
                }
            }

            public Color SelectedColor
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.selectedColor;
                }
                set
                {
                    if (!value.Equals(this.selectedColor))
                    {
                        this.InvalidateSelection();
                        this.selectedColor = value;
                        this.SetFocus(this.GetCellFromColor(value));
                        this.InvalidateSelection();
                    }
                }
            }

            [ComVisible(true)]
            public class ColorPaletteAccessibleObject : Control.ControlAccessibleObject
            {
                private ColorCellAccessibleObject[] cells;

                public ColorPaletteAccessibleObject(System.Drawing.Design.ColorEditor.ColorPalette owner) : base(owner)
                {
                    this.cells = new ColorCellAccessibleObject[0x40];
                }

                public override AccessibleObject GetChild(int id)
                {
                    if ((id < 0) || (id >= 0x40))
                    {
                        return null;
                    }
                    if (this.cells[id] == null)
                    {
                        this.cells[id] = new ColorCellAccessibleObject(this, this.ColorPalette.GetColorFromCell(id), id);
                    }
                    return this.cells[id];
                }

                public override int GetChildCount()
                {
                    return 0x40;
                }

                public override AccessibleObject HitTest(int x, int y)
                {
                    System.Drawing.Design.NativeMethods.POINT pt = new System.Drawing.Design.NativeMethods.POINT(x, y);
                    System.Drawing.Design.UnsafeNativeMethods.ScreenToClient(new HandleRef(this.ColorPalette, this.ColorPalette.Handle), pt);
                    int cellFromLocationMouse = System.Drawing.Design.ColorEditor.ColorPalette.GetCellFromLocationMouse(pt.x, pt.y);
                    if (cellFromLocationMouse != -1)
                    {
                        return this.GetChild(cellFromLocationMouse);
                    }
                    return base.HitTest(x, y);
                }

                internal System.Drawing.Design.ColorEditor.ColorPalette ColorPalette
                {
                    get
                    {
                        return (System.Drawing.Design.ColorEditor.ColorPalette) base.Owner;
                    }
                }

                [ComVisible(true)]
                public class ColorCellAccessibleObject : AccessibleObject
                {
                    private int cell;
                    private Color color;
                    private ColorEditor.ColorPalette.ColorPaletteAccessibleObject parent;

                    public ColorCellAccessibleObject(ColorEditor.ColorPalette.ColorPaletteAccessibleObject parent, Color color, int cell)
                    {
                        this.color = color;
                        this.parent = parent;
                        this.cell = cell;
                    }

                    public override Rectangle Bounds
                    {
                        get
                        {
                            Point point = ColorEditor.ColorPalette.Get2DFrom1D(this.cell);
                            Rectangle rect = new Rectangle();
                            ColorEditor.ColorPalette.FillRectWithCellBounds(point.X, point.Y, ref rect);
                            System.Drawing.Design.NativeMethods.POINT pt = new System.Drawing.Design.NativeMethods.POINT(rect.X, rect.Y);
                            System.Drawing.Design.UnsafeNativeMethods.ClientToScreen(new HandleRef(this.parent.ColorPalette, this.parent.ColorPalette.Handle), pt);
                            return new Rectangle(pt.x, pt.y, rect.Width, rect.Height);
                        }
                    }

                    public override string Name
                    {
                        get
                        {
                            return this.color.ToString();
                        }
                    }

                    public override AccessibleObject Parent
                    {
                        get
                        {
                            return this.parent;
                        }
                    }

                    public override AccessibleRole Role
                    {
                        get
                        {
                            return AccessibleRole.Cell;
                        }
                    }

                    public override AccessibleStates State
                    {
                        get
                        {
                            AccessibleStates state = base.State;
                            if (this.cell == this.parent.ColorPalette.FocusedCell)
                            {
                                state |= AccessibleStates.Focused;
                            }
                            return state;
                        }
                    }

                    public override string Value
                    {
                        get
                        {
                            return this.color.ToString();
                        }
                    }
                }
            }
        }

        private class ColorUI : Control
        {
            private object[] colorConstants;
            private bool commonHeightSet;
            private TabPage commonTabPage;
            private Color[] customColors;
            private ColorEditor editor;
            private IWindowsFormsEditorService edSvc;
            private ListBox lbCommon;
            private ListBox lbSystem;
            private ColorEditor.ColorPalette pal;
            private TabPage paletteTabPage;
            private object[] systemColorConstants;
            private bool systemHeightSet;
            private TabPage systemTabPage;
            private ColorEditorTabControl tabControl;
            private object value;

            public ColorUI(ColorEditor editor)
            {
                this.editor = editor;
                this.InitializeComponent();
                this.AdjustListBoxItemHeight();
            }

            private void AdjustColorUIHeight()
            {
                Size size = this.pal.Size;
                Rectangle tabRect = this.tabControl.GetTabRect(0);
                int num = 0;
                base.Size = new Size(size.Width + (2 * num), (size.Height + (2 * num)) + tabRect.Height);
                this.tabControl.Size = base.Size;
            }

            private void AdjustListBoxItemHeight()
            {
                this.lbSystem.ItemHeight = this.Font.Height + 2;
                this.lbCommon.ItemHeight = this.Font.Height + 2;
            }

            public void End()
            {
                this.edSvc = null;
                this.value = null;
            }

            private Color GetBestColor(Color color)
            {
                object[] colorValues = this.ColorValues;
                int num = color.ToArgb();
                for (int i = 0; i < colorValues.Length; i++)
                {
                    Color color2 = (Color) colorValues[i];
                    if (color2.ToArgb() == num)
                    {
                        return (Color) colorValues[i];
                    }
                }
                return color;
            }

            private static object[] GetConstants(System.Type enumType)
            {
                MethodAttributes attributes = MethodAttributes.Static | MethodAttributes.Public;
                PropertyInfo[] properties = enumType.GetProperties();
                ArrayList list = new ArrayList();
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo info = properties[i];
                    if (info.PropertyType == typeof(Color))
                    {
                        MethodInfo getMethod = info.GetGetMethod();
                        if ((getMethod != null) && ((getMethod.Attributes & attributes) == attributes))
                        {
                            object[] index = null;
                            list.Add(info.GetValue(null, index));
                        }
                    }
                }
                return list.ToArray();
            }

            private void InitializeComponent()
            {
                this.paletteTabPage = new TabPage(System.Drawing.Design.SR.GetString("ColorEditorPaletteTab"));
                this.commonTabPage = new TabPage(System.Drawing.Design.SR.GetString("ColorEditorStandardTab"));
                this.systemTabPage = new TabPage(System.Drawing.Design.SR.GetString("ColorEditorSystemTab"));
                base.AccessibleName = System.Drawing.Design.SR.GetString("ColorEditorAccName");
                this.tabControl = new ColorEditorTabControl();
                this.tabControl.TabPages.Add(this.paletteTabPage);
                this.tabControl.TabPages.Add(this.commonTabPage);
                this.tabControl.TabPages.Add(this.systemTabPage);
                this.tabControl.TabStop = false;
                this.tabControl.SelectedTab = this.systemTabPage;
                this.tabControl.SelectedIndexChanged += new EventHandler(this.OnTabControlSelChange);
                this.tabControl.Dock = DockStyle.Fill;
                this.tabControl.Resize += new EventHandler(this.OnTabControlResize);
                this.lbSystem = new ColorEditorListBox();
                this.lbSystem.DrawMode = DrawMode.OwnerDrawFixed;
                this.lbSystem.BorderStyle = BorderStyle.FixedSingle;
                this.lbSystem.IntegralHeight = false;
                this.lbSystem.Sorted = false;
                this.lbSystem.Click += new EventHandler(this.OnListClick);
                this.lbSystem.DrawItem += new DrawItemEventHandler(this.OnListDrawItem);
                this.lbSystem.KeyDown += new KeyEventHandler(this.OnListKeyDown);
                this.lbSystem.Dock = DockStyle.Fill;
                this.lbSystem.FontChanged += new EventHandler(this.OnFontChanged);
                this.lbCommon = new ColorEditorListBox();
                this.lbCommon.DrawMode = DrawMode.OwnerDrawFixed;
                this.lbCommon.BorderStyle = BorderStyle.FixedSingle;
                this.lbCommon.IntegralHeight = false;
                this.lbCommon.Sorted = false;
                this.lbCommon.Click += new EventHandler(this.OnListClick);
                this.lbCommon.DrawItem += new DrawItemEventHandler(this.OnListDrawItem);
                this.lbCommon.KeyDown += new KeyEventHandler(this.OnListKeyDown);
                this.lbCommon.Dock = DockStyle.Fill;
                Array.Sort(this.ColorValues, new ColorEditor.StandardColorComparer());
                Array.Sort(this.SystemColorValues, new ColorEditor.SystemColorComparer());
                this.lbCommon.Items.Clear();
                foreach (object obj2 in this.ColorValues)
                {
                    this.lbCommon.Items.Add(obj2);
                }
                this.lbSystem.Items.Clear();
                foreach (object obj3 in this.SystemColorValues)
                {
                    this.lbSystem.Items.Add(obj3);
                }
                this.pal = new ColorEditor.ColorPalette(this, this.CustomColors);
                this.pal.Picked += new EventHandler(this.OnPalettePick);
                this.paletteTabPage.Controls.Add(this.pal);
                this.systemTabPage.Controls.Add(this.lbSystem);
                this.commonTabPage.Controls.Add(this.lbCommon);
                base.Controls.Add(this.tabControl);
            }

            protected override void OnFontChanged(EventArgs e)
            {
                base.OnFontChanged(e);
                this.AdjustListBoxItemHeight();
                this.AdjustColorUIHeight();
            }

            private void OnFontChanged(object sender, EventArgs e)
            {
                this.commonHeightSet = this.systemHeightSet = false;
            }

            protected override void OnGotFocus(EventArgs e)
            {
                base.OnGotFocus(e);
                this.OnTabControlSelChange(this, EventArgs.Empty);
            }

            private void OnListClick(object sender, EventArgs e)
            {
                ListBox box = (ListBox) sender;
                if (box.SelectedItem != null)
                {
                    this.value = (Color) box.SelectedItem;
                }
                this.edSvc.CloseDropDown();
            }

            private void OnListDrawItem(object sender, DrawItemEventArgs die)
            {
                ListBox box = (ListBox) sender;
                object obj2 = box.Items[die.Index];
                Font font = this.Font;
                if ((box == this.lbCommon) && !this.commonHeightSet)
                {
                    box.ItemHeight = box.Font.Height;
                    this.commonHeightSet = true;
                }
                else if ((box == this.lbSystem) && !this.systemHeightSet)
                {
                    box.ItemHeight = box.Font.Height;
                    this.systemHeightSet = true;
                }
                Graphics canvas = die.Graphics;
                die.DrawBackground();
                this.editor.PaintValue(obj2, canvas, new Rectangle(die.Bounds.X + 2, die.Bounds.Y + 2, 0x16, die.Bounds.Height - 4));
                canvas.DrawRectangle(SystemPens.WindowText, new Rectangle(die.Bounds.X + 2, die.Bounds.Y + 2, 0x15, (die.Bounds.Height - 4) - 1));
                Brush brush = new SolidBrush(die.ForeColor);
                Color color = (Color) obj2;
                canvas.DrawString(color.Name, font, brush, (float) (die.Bounds.X + 0x1a), (float) die.Bounds.Y);
                brush.Dispose();
            }

            private void OnListKeyDown(object sender, KeyEventArgs ke)
            {
                if (ke.KeyCode == Keys.Enter)
                {
                    this.OnListClick(sender, EventArgs.Empty);
                }
            }

            private void OnPalettePick(object sender, EventArgs e)
            {
                ColorEditor.ColorPalette palette = (ColorEditor.ColorPalette) sender;
                this.value = this.GetBestColor(palette.SelectedColor);
                this.edSvc.CloseDropDown();
            }

            private void OnTabControlResize(object sender, EventArgs e)
            {
                Rectangle clientRectangle = this.tabControl.TabPages[0].ClientRectangle;
                Rectangle tabRect = this.tabControl.GetTabRect(1);
                clientRectangle.Y = 0;
                clientRectangle.Height -= clientRectangle.Y;
                int x = 2;
                this.lbSystem.SetBounds(x, clientRectangle.Y + (2 * x), clientRectangle.Width - x, (this.pal.Size.Height - tabRect.Height) + (2 * x));
                this.lbCommon.Bounds = this.lbSystem.Bounds;
                this.pal.Location = new Point(0, clientRectangle.Y);
            }

            private void OnTabControlSelChange(object sender, EventArgs e)
            {
                TabPage selectedTab = this.tabControl.SelectedTab;
                if ((selectedTab != null) && (selectedTab.Controls.Count > 0))
                {
                    selectedTab.Controls[0].Focus();
                }
            }

            protected override bool ProcessDialogKey(Keys keyData)
            {
                if ((((keyData & Keys.Alt) == Keys.None) && ((keyData & Keys.Control) == Keys.None)) && ((keyData & Keys.KeyCode) == Keys.Tab))
                {
                    bool flag = (keyData & Keys.Shift) == Keys.None;
                    int selectedIndex = this.tabControl.SelectedIndex;
                    if (selectedIndex != -1)
                    {
                        int count = this.tabControl.TabPages.Count;
                        if (flag)
                        {
                            selectedIndex = (selectedIndex + 1) % count;
                        }
                        else
                        {
                            selectedIndex = ((selectedIndex + count) - 1) % count;
                        }
                        this.tabControl.SelectedTab = this.tabControl.TabPages[selectedIndex];
                        return true;
                    }
                }
                return base.ProcessDialogKey(keyData);
            }

            public void Start(IWindowsFormsEditorService edSvc, object value)
            {
                this.edSvc = edSvc;
                this.value = value;
                this.AdjustColorUIHeight();
                if (value != null)
                {
                    object[] colorValues = this.ColorValues;
                    TabPage paletteTabPage = this.paletteTabPage;
                    for (int i = 0; i < colorValues.Length; i++)
                    {
                        if (colorValues[i].Equals(value))
                        {
                            this.lbCommon.SelectedItem = value;
                            paletteTabPage = this.commonTabPage;
                            break;
                        }
                    }
                    if (paletteTabPage == this.paletteTabPage)
                    {
                        colorValues = this.SystemColorValues;
                        for (int j = 0; j < colorValues.Length; j++)
                        {
                            if (colorValues[j].Equals(value))
                            {
                                this.lbSystem.SelectedItem = value;
                                paletteTabPage = this.systemTabPage;
                                break;
                            }
                        }
                    }
                    this.tabControl.SelectedTab = paletteTabPage;
                }
            }

            private object[] ColorValues
            {
                get
                {
                    if (this.colorConstants == null)
                    {
                        this.colorConstants = GetConstants(typeof(Color));
                    }
                    return this.colorConstants;
                }
            }

            private Color[] CustomColors
            {
                get
                {
                    if (this.customColors == null)
                    {
                        this.customColors = new Color[0x10];
                        for (int i = 0; i < 0x10; i++)
                        {
                            this.customColors[i] = Color.White;
                        }
                    }
                    return this.customColors;
                }
                set
                {
                    this.customColors = value;
                    this.pal = null;
                }
            }

            public IWindowsFormsEditorService EditorService
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.edSvc;
                }
            }

            private object[] SystemColorValues
            {
                get
                {
                    if (this.systemColorConstants == null)
                    {
                        this.systemColorConstants = GetConstants(typeof(SystemColors));
                    }
                    return this.systemColorConstants;
                }
            }

            public object Value
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.value;
                }
            }

            private class ColorEditorListBox : ListBox
            {
                protected override bool IsInputKey(Keys keyData)
                {
                    Keys keys = keyData;
                    return ((keys == Keys.Enter) || base.IsInputKey(keyData));
                }
            }

            private class ColorEditorTabControl : TabControl
            {
                protected override void OnGotFocus(EventArgs e)
                {
                    TabPage selectedTab = base.SelectedTab;
                    if ((selectedTab != null) && (selectedTab.Controls.Count > 0))
                    {
                        selectedTab.Controls[0].Focus();
                    }
                }
            }
        }

        private class CustomColorDialog : ColorDialog
        {
            private const int COLOR_ADD = 0x2c8;
            private const int COLOR_BLUE = 0x2c4;
            private const int COLOR_GREEN = 0x2c3;
            private const int COLOR_HUE = 0x2bf;
            private const int COLOR_LUM = 0x2c1;
            private const int COLOR_MIX = 0x2cf;
            private const int COLOR_RED = 0x2c2;
            private const int COLOR_SAT = 0x2c0;
            private IntPtr hInstance;

            public CustomColorDialog()
            {
                Stream manifestResourceStream = typeof(ColorEditor).Module.Assembly.GetManifestResourceStream(typeof(ColorEditor), "colordlg.data");
                int count = (int) (manifestResourceStream.Length - manifestResourceStream.Position);
                byte[] buffer = new byte[count];
                manifestResourceStream.Read(buffer, 0, count);
                this.hInstance = Marshal.AllocHGlobal(count);
                Marshal.Copy(buffer, 0, this.hInstance, count);
            }

            protected override void Dispose(bool disposing)
            {
                try
                {
                    if (this.hInstance != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(this.hInstance);
                        this.hInstance = IntPtr.Zero;
                    }
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }

            protected override IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
            {
                switch (msg)
                {
                    case 0x110:
                    {
                        System.Drawing.Design.NativeMethods.SendDlgItemMessage(hwnd, 0x2bf, 0xd3, (IntPtr) 3, IntPtr.Zero);
                        System.Drawing.Design.NativeMethods.SendDlgItemMessage(hwnd, 0x2c0, 0xd3, (IntPtr) 3, IntPtr.Zero);
                        System.Drawing.Design.NativeMethods.SendDlgItemMessage(hwnd, 0x2c1, 0xd3, (IntPtr) 3, IntPtr.Zero);
                        System.Drawing.Design.NativeMethods.SendDlgItemMessage(hwnd, 0x2c2, 0xd3, (IntPtr) 3, IntPtr.Zero);
                        System.Drawing.Design.NativeMethods.SendDlgItemMessage(hwnd, 0x2c3, 0xd3, (IntPtr) 3, IntPtr.Zero);
                        System.Drawing.Design.NativeMethods.SendDlgItemMessage(hwnd, 0x2c4, 0xd3, (IntPtr) 3, IntPtr.Zero);
                        IntPtr dlgItem = System.Drawing.Design.NativeMethods.GetDlgItem(hwnd, 0x2cf);
                        System.Drawing.Design.NativeMethods.EnableWindow(dlgItem, false);
                        System.Drawing.Design.NativeMethods.SetWindowPos(dlgItem, IntPtr.Zero, 0, 0, 0, 0, 0x80);
                        dlgItem = System.Drawing.Design.NativeMethods.GetDlgItem(hwnd, 1);
                        System.Drawing.Design.NativeMethods.EnableWindow(dlgItem, false);
                        System.Drawing.Design.NativeMethods.SetWindowPos(dlgItem, IntPtr.Zero, 0, 0, 0, 0, 0x80);
                        base.Color = Color.Empty;
                        break;
                    }
                    case 0x111:
                        if (System.Drawing.Design.NativeMethods.Util.LOWORD((int) ((long) wParam)) == 0x2c8)
                        {
                            bool[] err = new bool[1];
                            byte red = (byte) System.Drawing.Design.NativeMethods.GetDlgItemInt(hwnd, 0x2c2, err, false);
                            byte green = (byte) System.Drawing.Design.NativeMethods.GetDlgItemInt(hwnd, 0x2c3, err, false);
                            byte blue = (byte) System.Drawing.Design.NativeMethods.GetDlgItemInt(hwnd, 0x2c4, err, false);
                            base.Color = Color.FromArgb(red, green, blue);
                            System.Drawing.Design.NativeMethods.PostMessage(hwnd, 0x111, (IntPtr) System.Drawing.Design.NativeMethods.Util.MAKELONG(1, 0), System.Drawing.Design.NativeMethods.GetDlgItem(hwnd, 1));
                        }
                        break;
                }
                return base.HookProc(hwnd, msg, wParam, lParam);
            }

            protected override IntPtr Instance
            {
                get
                {
                    return this.hInstance;
                }
            }

            protected override int Options
            {
                get
                {
                    return 0x42;
                }
            }
        }

        private class StandardColorComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                Color color = (Color) x;
                Color color2 = (Color) y;
                if (color.A < color2.A)
                {
                    return -1;
                }
                if (color.A > color2.A)
                {
                    return 1;
                }
                if (color.GetHue() < color2.GetHue())
                {
                    return -1;
                }
                if (color.GetHue() > color2.GetHue())
                {
                    return 1;
                }
                if (color.GetSaturation() < color2.GetSaturation())
                {
                    return -1;
                }
                if (color.GetSaturation() > color2.GetSaturation())
                {
                    return 1;
                }
                if (color.GetBrightness() < color2.GetBrightness())
                {
                    return -1;
                }
                if (color.GetBrightness() > color2.GetBrightness())
                {
                    return 1;
                }
                return 0;
            }
        }

        private class SystemColorComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                Color color = (Color) x;
                Color color2 = (Color) y;
                return string.Compare(color.Name, color2.Name, false, CultureInfo.InvariantCulture);
            }
        }
    }
}

