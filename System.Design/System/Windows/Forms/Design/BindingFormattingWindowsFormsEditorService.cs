namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal class BindingFormattingWindowsFormsEditorService : Panel, IWindowsFormsEditorService, ITypeDescriptorContext, IServiceProvider
    {
        private System.Windows.Forms.Binding binding;
        private DropDownButton button;
        private ITypeDescriptorContext context;
        private DataSourceUpdateMode defaultDataSourceUpdateMode;
        private DesignBindingPicker designBindingPicker;
        private DropDownHolder dropDownHolder;
        private IComponent ownerComponent;
        private string propertyName = string.Empty;

        public event EventHandler PropertyValueChanged;

        public BindingFormattingWindowsFormsEditorService()
        {
            this.BackColor = SystemColors.Window;
            this.Text = System.Design.SR.GetString("DataGridNoneString");
            base.SetStyle(ControlStyles.UserPaint, true);
            base.SetStyle(ControlStyles.Selectable, true);
            base.SetStyle(ControlStyles.UseTextForAccessibility, true);
            base.AccessibleRole = AccessibleRole.DropList;
            base.TabStop = true;
            this.button = new DropDownButton(this);
            this.button.FlatStyle = FlatStyle.Popup;
            this.button.Image = this.CreateDownArrow();
            this.button.Padding = new Padding(0);
            this.button.BackColor = SystemColors.Control;
            this.button.ForeColor = SystemColors.ControlText;
            this.button.Click += new EventHandler(this.button_Click);
            this.button.Size = new Size(SystemInformation.VerticalScrollBarArrowHeight, this.Font.Height + 2);
            this.button.AccessibleName = System.Design.SR.GetString("BindingFormattingDialogDataSourcePickerDropDownAccName");
            base.Controls.Add(this.button);
        }

        private void button_Click(object sender, EventArgs e)
        {
            this.DropDownPicker();
        }

        private static string ConstructDisplayTextFromBinding(System.Windows.Forms.Binding binding)
        {
            string name;
            if (binding.DataSource == null)
            {
                name = System.Design.SR.GetString("DataGridNoneString");
            }
            else if (binding.DataSource is IComponent)
            {
                IComponent dataSource = binding.DataSource as IComponent;
                if (dataSource.Site != null)
                {
                    name = dataSource.Site.Name;
                }
                else
                {
                    name = "";
                }
            }
            else if (((binding.DataSource is IListSource) || (binding.DataSource is IList)) || (binding.DataSource is Array))
            {
                name = System.Design.SR.GetString("BindingFormattingDialogList");
            }
            else
            {
                string className = TypeDescriptor.GetClassName(binding.DataSource);
                int num = className.LastIndexOf(".");
                if (num != -1)
                {
                    className = className.Substring(num + 1);
                }
                name = string.Format(CultureInfo.CurrentCulture, "({0})", new object[] { className });
            }
            return (name + " - " + binding.BindingMemberInfo.BindingMember);
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new BindingFormattingWindowFormsEditorAccessibleObject(this);
        }

        private Bitmap CreateDownArrow()
        {
            Bitmap bitmap = null;
            try
            {
                Icon icon = new Icon(typeof(BindingFormattingDialog), "BindingFormattingDialog.Arrow.ico");
                bitmap = icon.ToBitmap();
                icon.Dispose();
            }
            catch
            {
                bitmap = new Bitmap(0x10, 0x10);
            }
            return bitmap;
        }

        private void DropDownPicker()
        {
            if (this.designBindingPicker == null)
            {
                this.designBindingPicker = new DesignBindingPicker();
                this.designBindingPicker.Width = base.Width;
            }
            DesignBinding initialSelectedItem = null;
            if (this.binding != null)
            {
                initialSelectedItem = new DesignBinding(this.binding.DataSource, this.binding.BindingMemberInfo.BindingMember);
            }
            DesignBinding binding2 = this.designBindingPicker.Pick(this, this, true, true, false, null, string.Empty, initialSelectedItem);
            if (binding2 != null)
            {
                System.Windows.Forms.Binding binding = this.binding;
                System.Windows.Forms.Binding binding4 = null;
                string formatString = (binding != null) ? binding.FormatString : string.Empty;
                IFormatProvider formatInfo = (binding != null) ? binding.FormatInfo : null;
                object nullValue = (binding != null) ? binding.NullValue : null;
                DataSourceUpdateMode dataSourceUpdateMode = (binding != null) ? binding.DataSourceUpdateMode : this.defaultDataSourceUpdateMode;
                if ((binding2.DataSource != null) && !string.IsNullOrEmpty(binding2.DataMember))
                {
                    binding4 = new System.Windows.Forms.Binding(this.propertyName, binding2.DataSource, binding2.DataMember, true, dataSourceUpdateMode, nullValue, formatString, formatInfo);
                }
                this.Binding = binding4;
                if ((((binding4 == null) || (binding != null)) || ((binding4 != null) && (binding == null))) || (((binding4 != null) && (binding != null)) && ((binding4.DataSource != binding.DataSource) || !binding4.BindingMemberInfo.Equals(binding.BindingMemberInfo))))
                {
                    this.OnPropertyValueChanged(EventArgs.Empty);
                }
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            base.Select();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            base.Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            base.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs p)
        {
            base.OnPaint(p);
            string text = this.Text;
            if (ComboBoxRenderer.IsSupported)
            {
                ComboBoxState disabled;
                SolidBrush controlDark;
                SolidBrush control;
                Rectangle bounds = new Rectangle(base.ClientRectangle.X, base.ClientRectangle.Y, base.ClientRectangle.Width, base.ClientRectangle.Height);
                if (!base.Enabled)
                {
                    controlDark = (SolidBrush) SystemBrushes.ControlDark;
                    control = (SolidBrush) SystemBrushes.Control;
                    disabled = ComboBoxState.Disabled;
                }
                else if (base.ContainsFocus)
                {
                    controlDark = (SolidBrush) SystemBrushes.HighlightText;
                    control = (SolidBrush) SystemBrushes.Highlight;
                    disabled = ComboBoxState.Hot;
                }
                else
                {
                    controlDark = (SolidBrush) SystemBrushes.WindowText;
                    control = (SolidBrush) SystemBrushes.Window;
                    disabled = ComboBoxState.Normal;
                }
                ComboBoxRenderer.DrawTextBox(p.Graphics, bounds, string.Empty, this.Font, disabled);
                Graphics graphics = p.Graphics;
                bounds.Inflate(-2, -2);
                ControlPaint.DrawBorder(graphics, bounds, control.Color, ButtonBorderStyle.None);
                bounds.Inflate(-1, -1);
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    bounds.X += this.button.Width;
                }
                bounds.Width -= this.button.Width;
                graphics.FillRectangle(control, bounds);
                TextFormatFlags verticalCenter = TextFormatFlags.VerticalCenter;
                if (this.RightToLeft == RightToLeft.No)
                {
                    verticalCenter = verticalCenter;
                }
                else
                {
                    verticalCenter |= TextFormatFlags.Right;
                }
                if (base.ContainsFocus)
                {
                    ControlPaint.DrawFocusRectangle(graphics, bounds, Color.Empty, control.Color);
                }
                TextRenderer.DrawText(graphics, text, this.Font, bounds, controlDark.Color, verticalCenter);
            }
            else if (!string.IsNullOrEmpty(text))
            {
                StringFormat format = new StringFormat {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Near
                };
                Rectangle clientRectangle = base.ClientRectangle;
                Rectangle rectangle3 = new Rectangle(clientRectangle.X, clientRectangle.Y, clientRectangle.Width, clientRectangle.Height);
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    rectangle3.X += this.button.Width;
                }
                rectangle3.Width -= this.button.Width;
                TextFormatFlags flags = TextFormatFlags.VerticalCenter;
                if (this.RightToLeft == RightToLeft.No)
                {
                    flags = flags;
                }
                else
                {
                    flags |= TextFormatFlags.Right;
                }
                TextRenderer.DrawText(p.Graphics, text, this.Font, rectangle3, this.ForeColor, flags);
                format.Dispose();
            }
        }

        protected void OnPropertyValueChanged(EventArgs e)
        {
            if (this.propertyValueChanged != null)
            {
                this.propertyValueChanged(this, e);
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (((Control.ModifierKeys & Keys.Alt) == Keys.Alt) && ((keyData & Keys.KeyCode) == Keys.Down))
            {
                this.DropDownPicker();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, this.PreferredHeight, specified);
            int num = base.Height - 2;
            int horizontalScrollBarThumbWidth = SystemInformation.HorizontalScrollBarThumbWidth;
            int num3 = (base.Width - horizontalScrollBarThumbWidth) - 2;
            int num4 = 1;
            if (this.RightToLeft == RightToLeft.No)
            {
                this.button.Bounds = new Rectangle(num4, num3, horizontalScrollBarThumbWidth, num);
            }
            else
            {
                this.button.Bounds = new Rectangle(num4, 2, horizontalScrollBarThumbWidth, num);
            }
        }

        void ITypeDescriptorContext.OnComponentChanged()
        {
            if (this.context != null)
            {
                this.context.OnComponentChanged();
            }
        }

        bool ITypeDescriptorContext.OnComponentChanging()
        {
            if (this.context != null)
            {
                return this.context.OnComponentChanging();
            }
            return true;
        }

        object IServiceProvider.GetService(System.Type type)
        {
            if (type == typeof(IWindowsFormsEditorService))
            {
                return this;
            }
            if (this.context != null)
            {
                return this.context.GetService(type);
            }
            return null;
        }

        void IWindowsFormsEditorService.CloseDropDown()
        {
            this.dropDownHolder.SetComponent(null);
            this.dropDownHolder.Visible = false;
            this.button.Focus();
        }

        void IWindowsFormsEditorService.DropDownControl(Control ctl)
        {
            if (this.dropDownHolder == null)
            {
                this.dropDownHolder = new DropDownHolder(this);
            }
            this.dropDownHolder.SetComponent(ctl);
            this.dropDownHolder.Location = base.PointToScreen(new Point(0, base.Height));
            try
            {
                this.dropDownHolder.Visible = true;
                System.Design.UnsafeNativeMethods.SetWindowLong(new HandleRef(this.dropDownHolder, this.dropDownHolder.Handle), -8, new HandleRef(this, base.Handle));
                this.dropDownHolder.FocusComponent();
                this.dropDownHolder.DoModalLoop();
            }
            finally
            {
                System.Design.UnsafeNativeMethods.SetWindowLong(new HandleRef(this.dropDownHolder, this.dropDownHolder.Handle), -8, new HandleRef(null, IntPtr.Zero));
            }
        }

        DialogResult IWindowsFormsEditorService.ShowDialog(Form form)
        {
            return form.ShowDialog();
        }

        public System.Windows.Forms.Binding Binding
        {
            get
            {
                return this.binding;
            }
            set
            {
                if (this.binding != value)
                {
                    this.binding = value;
                    if (this.binding != null)
                    {
                        this.Text = ConstructDisplayTextFromBinding(this.binding);
                    }
                    else
                    {
                        this.Text = System.Design.SR.GetString("DataGridNoneString");
                    }
                    base.Invalidate();
                }
            }
        }

        public ITypeDescriptorContext Context
        {
            set
            {
                this.context = value;
            }
        }

        public DataSourceUpdateMode DefaultDataSourceUpdateMode
        {
            set
            {
                this.defaultDataSourceUpdateMode = value;
            }
        }

        public IComponent OwnerComponent
        {
            set
            {
                this.ownerComponent = value;
            }
        }

        private int PreferredHeight
        {
            get
            {
                return ((TextRenderer.MeasureText("j^", this.Font, new Size(0x7fff, (int) (base.FontHeight * 1.25))).Height + (SystemInformation.BorderSize.Height * 8)) + base.Padding.Size.Height);
            }
        }

        public string PropertyName
        {
            set
            {
                this.propertyName = value;
            }
        }

        IContainer ITypeDescriptorContext.Container
        {
            get
            {
                if (this.ownerComponent == null)
                {
                    return null;
                }
                ISite site = this.ownerComponent.Site;
                if (site == null)
                {
                    return null;
                }
                return site.Container;
            }
        }

        object ITypeDescriptorContext.Instance
        {
            get
            {
                return this.ownerComponent;
            }
        }

        PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor
        {
            get
            {
                return null;
            }
        }

        private class BindingFormattingWindowFormsEditorAccessibleObject : Control.ControlAccessibleObject
        {
            private BindingFormattingWindowsFormsEditorService owner;

            public BindingFormattingWindowFormsEditorAccessibleObject(BindingFormattingWindowsFormsEditorService owner) : base(owner)
            {
                this.owner = owner;
            }

            public override void DoDefaultAction()
            {
                this.owner.DropDownPicker();
            }

            public override string Name
            {
                get
                {
                    return System.Design.SR.GetString("BindingFormattingDialogBindingPickerAccName");
                }
            }

            public override string Value
            {
                get
                {
                    return this.owner.Text;
                }
            }
        }

        private class DropDownButton : System.Windows.Forms.Button
        {
            private bool mouseIsDown;
            private bool mouseIsOver;
            private BindingFormattingWindowsFormsEditorService owner;
            private const int WM_CANCELMODE = 0x1f;
            private const int WM_CAPTURECHANGED = 0x215;
            private const int WM_KILLFOCUS = 8;

            public DropDownButton(BindingFormattingWindowsFormsEditorService owner)
            {
                this.owner = owner;
                base.TabStop = false;
            }

            protected override void OnEnabledChanged(EventArgs e)
            {
                base.OnEnabledChanged(e);
                if (!base.Enabled)
                {
                    this.mouseIsDown = false;
                    this.mouseIsOver = false;
                }
            }

            protected override void OnKeyDown(KeyEventArgs kevent)
            {
                base.OnKeyDown(kevent);
                if (kevent.KeyData == Keys.Space)
                {
                    this.mouseIsDown = true;
                    base.Invalidate();
                }
            }

            protected override void OnKeyUp(KeyEventArgs kevent)
            {
                base.OnKeyUp(kevent);
                if (this.mouseIsDown)
                {
                    this.mouseIsDown = false;
                    base.Invalidate();
                }
            }

            protected override void OnLostFocus(EventArgs e)
            {
                base.OnLostFocus(e);
                this.mouseIsDown = false;
                base.Invalidate();
            }

            protected override void OnMouseDown(MouseEventArgs mevent)
            {
                base.OnMouseDown(mevent);
                if (mevent.Button == MouseButtons.Left)
                {
                    this.mouseIsDown = true;
                    base.Invalidate();
                }
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                base.OnMouseEnter(e);
                if (!this.mouseIsOver)
                {
                    this.mouseIsOver = true;
                    base.Invalidate();
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                if (this.mouseIsOver || this.mouseIsDown)
                {
                    this.mouseIsOver = false;
                    this.mouseIsDown = false;
                    base.Invalidate();
                }
            }

            protected override void OnMouseMove(MouseEventArgs mevent)
            {
                base.OnMouseMove(mevent);
                if (mevent.Button != MouseButtons.None)
                {
                    if (!base.ClientRectangle.Contains(mevent.X, mevent.Y))
                    {
                        if (this.mouseIsDown)
                        {
                            this.mouseIsDown = false;
                            base.Invalidate();
                        }
                    }
                    else if (!this.mouseIsDown)
                    {
                        this.mouseIsDown = true;
                        base.Invalidate();
                    }
                }
            }

            protected override void OnMouseUp(MouseEventArgs mevent)
            {
                base.OnMouseUp(mevent);
                if (this.mouseIsDown)
                {
                    this.mouseIsDown = false;
                    base.Invalidate();
                }
            }

            protected override void OnPaint(PaintEventArgs pevent)
            {
                base.OnPaint(pevent);
                if (VisualStyleRenderer.IsSupported)
                {
                    ComboBoxState normal = ComboBoxState.Normal;
                    if (!base.Enabled)
                    {
                        normal = ComboBoxState.Disabled;
                    }
                    if (this.mouseIsDown && this.mouseIsOver)
                    {
                        normal = ComboBoxState.Pressed;
                    }
                    else if (this.mouseIsOver)
                    {
                        normal = ComboBoxState.Hot;
                    }
                    ComboBoxRenderer.DrawDropDownButton(pevent.Graphics, pevent.ClipRectangle, normal);
                }
            }

            protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
            {
                height = Math.Min(height, this.owner.Height - 2);
                width = SystemInformation.HorizontalScrollBarThumbWidth;
                y = 1;
                if (base.Parent != null)
                {
                    if (base.Parent.RightToLeft == RightToLeft.No)
                    {
                        x = (base.Parent.Width - width) - 1;
                    }
                    else
                    {
                        x = 1;
                    }
                }
                base.SetBoundsCore(x, y, width, height, specified);
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case 8:
                    case 0x1f:
                    case 0x215:
                        this.mouseIsDown = false;
                        base.Invalidate();
                        base.WndProc(ref m);
                        return;
                }
                base.WndProc(ref m);
            }

            protected override Size DefaultSize
            {
                get
                {
                    return new Size(0x11, 0x13);
                }
            }
        }
    }
}

