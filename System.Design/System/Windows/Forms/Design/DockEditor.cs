namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Windows.Forms;

    public sealed class DockEditor : UITypeEditor
    {
        private DockUI dockUI;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc == null)
                {
                    return value;
                }
                if (this.dockUI == null)
                {
                    this.dockUI = new DockUI(this);
                }
                this.dockUI.Start(edSvc, value);
                edSvc.DropDownControl(this.dockUI);
                value = this.dockUI.Value;
                this.dockUI.End();
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        private class DockUI : Control
        {
            private CheckBox bottom = new DockEditorCheckBox();
            private ContainerPlaceholder container = new ContainerPlaceholder();
            private DockEditor editor;
            private IWindowsFormsEditorService edSvc;
            private CheckBox fill = new DockEditorCheckBox();
            private CheckBox left = new DockEditorCheckBox();
            private CheckBox[] leftRightOrder;
            private CheckBox none = new DockEditorCheckBox();
            private CheckBox right = new DockEditorCheckBox();
            private CheckBox[] tabOrder;
            private CheckBox top = new DockEditorCheckBox();
            private CheckBox[] upDownOrder;
            private object value;

            public DockUI(DockEditor editor)
            {
                this.editor = editor;
                this.upDownOrder = new CheckBox[] { this.top, this.fill, this.bottom, this.none };
                this.leftRightOrder = new CheckBox[] { this.left, this.fill, this.right };
                this.tabOrder = new CheckBox[] { this.top, this.left, this.fill, this.right, this.bottom, this.none };
                this.InitializeComponent();
            }

            public void End()
            {
                this.edSvc = null;
                this.value = null;
            }

            public virtual DockStyle GetDock(CheckBox btn)
            {
                if (this.top == btn)
                {
                    return DockStyle.Top;
                }
                if (this.left == btn)
                {
                    return DockStyle.Left;
                }
                if (this.bottom == btn)
                {
                    return DockStyle.Bottom;
                }
                if (this.right == btn)
                {
                    return DockStyle.Right;
                }
                if (this.fill == btn)
                {
                    return DockStyle.Fill;
                }
                return DockStyle.None;
            }

            private void InitializeComponent()
            {
                base.SetBounds(0, 0, 0x5e, 0x74);
                this.BackColor = SystemColors.Control;
                this.ForeColor = SystemColors.ControlText;
                base.AccessibleName = System.Design.SR.GetString("DockEditorAccName");
                this.none.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
                this.none.Location = new Point(2, 0x5e);
                this.none.Size = new Size(90, 0x18);
                this.none.Text = DockStyle.None.ToString();
                this.none.TabIndex = 0;
                this.none.TabStop = true;
                this.none.Appearance = Appearance.Button;
                this.none.Click += new EventHandler(this.OnClick);
                this.none.KeyDown += new KeyEventHandler(this.OnKeyDown);
                this.none.AccessibleName = System.Design.SR.GetString("DockEditorNoneAccName");
                this.container.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
                this.container.Location = new Point(2, 2);
                this.container.Size = new Size(90, 90);
                this.none.Dock = DockStyle.Bottom;
                this.container.Dock = DockStyle.Fill;
                this.right.Dock = DockStyle.Right;
                this.right.Size = new Size(20, 20);
                this.right.TabIndex = 4;
                this.right.TabStop = true;
                this.right.Text = " ";
                this.right.Appearance = Appearance.Button;
                this.right.Click += new EventHandler(this.OnClick);
                this.right.KeyDown += new KeyEventHandler(this.OnKeyDown);
                this.right.AccessibleName = System.Design.SR.GetString("DockEditorRightAccName");
                this.left.Dock = DockStyle.Left;
                this.left.Size = new Size(20, 20);
                this.left.TabIndex = 2;
                this.left.TabStop = true;
                this.left.Text = " ";
                this.left.Appearance = Appearance.Button;
                this.left.Click += new EventHandler(this.OnClick);
                this.left.KeyDown += new KeyEventHandler(this.OnKeyDown);
                this.left.AccessibleName = System.Design.SR.GetString("DockEditorLeftAccName");
                this.top.Dock = DockStyle.Top;
                this.top.Size = new Size(20, 20);
                this.top.TabIndex = 1;
                this.top.TabStop = true;
                this.top.Text = " ";
                this.top.Appearance = Appearance.Button;
                this.top.Click += new EventHandler(this.OnClick);
                this.top.KeyDown += new KeyEventHandler(this.OnKeyDown);
                this.top.AccessibleName = System.Design.SR.GetString("DockEditorTopAccName");
                this.bottom.Dock = DockStyle.Bottom;
                this.bottom.Size = new Size(20, 20);
                this.bottom.TabIndex = 5;
                this.bottom.TabStop = true;
                this.bottom.Text = " ";
                this.bottom.Appearance = Appearance.Button;
                this.bottom.Click += new EventHandler(this.OnClick);
                this.bottom.KeyDown += new KeyEventHandler(this.OnKeyDown);
                this.bottom.AccessibleName = System.Design.SR.GetString("DockEditorBottomAccName");
                this.fill.Dock = DockStyle.Fill;
                this.fill.Size = new Size(20, 20);
                this.fill.TabIndex = 3;
                this.fill.TabStop = true;
                this.fill.Text = " ";
                this.fill.Appearance = Appearance.Button;
                this.fill.Click += new EventHandler(this.OnClick);
                this.fill.KeyDown += new KeyEventHandler(this.OnKeyDown);
                this.fill.AccessibleName = System.Design.SR.GetString("DockEditorFillAccName");
                base.Controls.Clear();
                base.Controls.AddRange(new Control[] { this.container, this.none });
                this.container.Controls.Clear();
                this.container.Controls.AddRange(new Control[] { this.fill, this.left, this.right, this.top, this.bottom });
            }

            private void OnClick(object sender, EventArgs eventargs)
            {
                DockStyle dock = this.GetDock((CheckBox) sender);
                if (dock >= DockStyle.None)
                {
                    this.value = dock;
                }
                this.Teardown();
            }

            protected override void OnGotFocus(EventArgs e)
            {
                base.OnGotFocus(e);
                for (int i = 0; i < this.tabOrder.Length; i++)
                {
                    if (this.tabOrder[i].Checked)
                    {
                        this.tabOrder[i].Focus();
                        return;
                    }
                }
            }

            private void OnKeyDown(object sender, KeyEventArgs e)
            {
                int num;
                Keys keyCode = e.KeyCode;
                Control control = null;
                switch (keyCode)
                {
                    case Keys.Left:
                    case Keys.Right:
                        num = this.leftRightOrder.Length - 1;
                        for (int i = 0; i <= num; i++)
                        {
                            if (this.leftRightOrder[i] == sender)
                            {
                                if (keyCode == Keys.Left)
                                {
                                    control = this.leftRightOrder[Math.Max(i - 1, 0)];
                                }
                                else
                                {
                                    control = this.leftRightOrder[Math.Min(i + 1, num)];
                                }
                                break;
                            }
                        }
                        break;

                    case Keys.Up:
                    case Keys.Down:
                        if ((sender == this.left) || (sender == this.right))
                        {
                            sender = this.fill;
                        }
                        num = this.upDownOrder.Length - 1;
                        for (int j = 0; j <= num; j++)
                        {
                            if (this.upDownOrder[j] == sender)
                            {
                                if (keyCode == Keys.Up)
                                {
                                    control = this.upDownOrder[Math.Max(j - 1, 0)];
                                }
                                else
                                {
                                    control = this.upDownOrder[Math.Min(j + 1, num)];
                                }
                                break;
                            }
                        }
                        break;

                    case Keys.Enter:
                        base.InvokeOnClick((CheckBox) sender, EventArgs.Empty);
                        return;

                    case Keys.Tab:
                        for (int k = 0; k < this.tabOrder.Length; k++)
                        {
                            if (this.tabOrder[k] == sender)
                            {
                                k += ((e.Modifiers & Keys.Shift) == Keys.None) ? 1 : -1;
                                k = (k < 0) ? (k + this.tabOrder.Length) : (k % this.tabOrder.Length);
                                control = this.tabOrder[k];
                                break;
                            }
                        }
                        break;

                    default:
                        return;
                }
                e.Handled = true;
                if ((control != null) && (control != sender))
                {
                    control.Focus();
                }
            }

            public void Start(IWindowsFormsEditorService edSvc, object value)
            {
                this.edSvc = edSvc;
                this.value = value;
                if (value is DockStyle)
                {
                    DockStyle style = (DockStyle) value;
                    this.none.Checked = false;
                    this.top.Checked = false;
                    this.left.Checked = false;
                    this.right.Checked = false;
                    this.bottom.Checked = false;
                    this.fill.Checked = false;
                    switch (style)
                    {
                        case DockStyle.None:
                            this.none.Checked = true;
                            return;

                        case DockStyle.Top:
                            this.top.Checked = true;
                            return;

                        case DockStyle.Bottom:
                            this.bottom.Checked = true;
                            return;

                        case DockStyle.Left:
                            this.left.Checked = true;
                            return;

                        case DockStyle.Right:
                            this.right.Checked = true;
                            return;

                        case DockStyle.Fill:
                            this.fill.Checked = true;
                            break;

                        default:
                            return;
                    }
                }
            }

            private void Teardown()
            {
                this.edSvc.CloseDropDown();
            }

            public object Value
            {
                get
                {
                    return this.value;
                }
            }

            private class ContainerPlaceholder : Control
            {
                public ContainerPlaceholder()
                {
                    this.BackColor = SystemColors.Control;
                    base.TabStop = false;
                }

                protected override void OnPaint(PaintEventArgs e)
                {
                    Rectangle clientRectangle = base.ClientRectangle;
                    ControlPaint.DrawButton(e.Graphics, clientRectangle, ButtonState.Pushed);
                }
            }

            private class DockEditorCheckBox : CheckBox
            {
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
                    }
                    return base.IsInputKey(keyData);
                }

                protected override bool ShowFocusCues
                {
                    get
                    {
                        return true;
                    }
                }
            }
        }
    }
}

