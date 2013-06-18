namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Windows.Forms;

    public sealed class AnchorEditor : UITypeEditor
    {
        private AnchorUI anchorUI;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc == null)
                {
                    return value;
                }
                if (this.anchorUI == null)
                {
                    this.anchorUI = new AnchorUI(this);
                }
                this.anchorUI.Start(edSvc, value);
                edSvc.DropDownControl(this.anchorUI);
                value = this.anchorUI.Value;
                this.anchorUI.End();
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        private class AnchorUI : Control
        {
            private SpringControl bottom;
            private ContainerPlaceholder container = new ContainerPlaceholder();
            private ControlPlaceholder control = new ControlPlaceholder();
            private AnchorEditor editor;
            private IWindowsFormsEditorService edSvc;
            private SpringControl left;
            private AnchorStyles oldAnchor;
            private SpringControl right;
            private SpringControl[] tabOrder;
            private SpringControl top;
            private object value;

            public AnchorUI(AnchorEditor editor)
            {
                this.editor = editor;
                this.left = new SpringControl(this);
                this.right = new SpringControl(this);
                this.top = new SpringControl(this);
                this.bottom = new SpringControl(this);
                this.tabOrder = new SpringControl[] { this.left, this.top, this.right, this.bottom };
                this.InitializeComponent();
            }

            public void End()
            {
                this.edSvc = null;
                this.value = null;
            }

            public virtual AnchorStyles GetSelectedAnchor()
            {
                AnchorStyles none = AnchorStyles.None;
                if (this.left.GetSolid())
                {
                    none |= AnchorStyles.Left;
                }
                if (this.top.GetSolid())
                {
                    none |= AnchorStyles.Top;
                }
                if (this.bottom.GetSolid())
                {
                    none |= AnchorStyles.Bottom;
                }
                if (this.right.GetSolid())
                {
                    none |= AnchorStyles.Right;
                }
                return none;
            }

            internal virtual void InitializeComponent()
            {
                int width = SystemInformation.Border3DSize.Width;
                int height = SystemInformation.Border3DSize.Height;
                base.SetBounds(0, 0, 90, 90);
                base.AccessibleName = System.Design.SR.GetString("AnchorEditorAccName");
                this.container.Location = new Point(0, 0);
                this.container.Size = new Size(90, 90);
                this.container.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
                this.control.Location = new Point(30, 30);
                this.control.Size = new Size(30, 30);
                this.control.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
                this.right.Location = new Point(60, 40);
                this.right.Size = new Size(30 - width, 10);
                this.right.TabIndex = 2;
                this.right.TabStop = true;
                this.right.Anchor = AnchorStyles.Right;
                this.right.AccessibleName = System.Design.SR.GetString("AnchorEditorRightAccName");
                this.left.Location = new Point(width, 40);
                this.left.Size = new Size(30 - width, 10);
                this.left.TabIndex = 0;
                this.left.TabStop = true;
                this.left.Anchor = AnchorStyles.Left;
                this.left.AccessibleName = System.Design.SR.GetString("AnchorEditorLeftAccName");
                this.top.Location = new Point(40, height);
                this.top.Size = new Size(10, 30 - height);
                this.top.TabIndex = 1;
                this.top.TabStop = true;
                this.top.Anchor = AnchorStyles.Top;
                this.top.AccessibleName = System.Design.SR.GetString("AnchorEditorTopAccName");
                this.bottom.Location = new Point(40, 60);
                this.bottom.Size = new Size(10, 30 - height);
                this.bottom.TabIndex = 3;
                this.bottom.TabStop = true;
                this.bottom.Anchor = AnchorStyles.Bottom;
                this.bottom.AccessibleName = System.Design.SR.GetString("AnchorEditorBottomAccName");
                base.Controls.Clear();
                base.Controls.AddRange(new Control[] { this.container });
                this.container.Controls.Clear();
                this.container.Controls.AddRange(new Control[] { this.control, this.top, this.left, this.bottom, this.right });
            }

            protected override void OnGotFocus(EventArgs e)
            {
                base.OnGotFocus(e);
                this.top.Focus();
            }

            private void SetValue()
            {
                this.value = this.GetSelectedAnchor();
            }

            public void Start(IWindowsFormsEditorService edSvc, object value)
            {
                this.edSvc = edSvc;
                this.value = value;
                if (value is AnchorStyles)
                {
                    this.left.SetSolid((((AnchorStyles) value) & AnchorStyles.Left) == AnchorStyles.Left);
                    this.top.SetSolid((((AnchorStyles) value) & AnchorStyles.Top) == AnchorStyles.Top);
                    this.bottom.SetSolid((((AnchorStyles) value) & AnchorStyles.Bottom) == AnchorStyles.Bottom);
                    this.right.SetSolid((((AnchorStyles) value) & AnchorStyles.Right) == AnchorStyles.Right);
                    this.oldAnchor = (AnchorStyles) value;
                }
                else
                {
                    this.oldAnchor = AnchorStyles.Left | AnchorStyles.Top;
                }
            }

            private void Teardown(bool saveAnchor)
            {
                if (!saveAnchor)
                {
                    this.value = this.oldAnchor;
                }
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
                    this.BackColor = SystemColors.Window;
                    this.ForeColor = SystemColors.WindowText;
                    base.TabStop = false;
                }

                protected override void OnPaint(PaintEventArgs e)
                {
                    Rectangle clientRectangle = base.ClientRectangle;
                    ControlPaint.DrawBorder3D(e.Graphics, clientRectangle, Border3DStyle.Sunken);
                }
            }

            private class ControlPlaceholder : Control
            {
                public ControlPlaceholder()
                {
                    this.BackColor = SystemColors.Control;
                    base.TabStop = false;
                    base.SetStyle(ControlStyles.Selectable, false);
                }

                protected override void OnPaint(PaintEventArgs e)
                {
                    Rectangle clientRectangle = base.ClientRectangle;
                    ControlPaint.DrawButton(e.Graphics, clientRectangle, ButtonState.Normal);
                }
            }

            private class SpringControl : Control
            {
                internal bool focused;
                private AnchorEditor.AnchorUI picker;
                internal bool solid;

                public SpringControl(AnchorEditor.AnchorUI picker)
                {
                    if (picker == null)
                    {
                        throw new ArgumentException();
                    }
                    this.picker = picker;
                    base.TabStop = true;
                }

                protected override AccessibleObject CreateAccessibilityInstance()
                {
                    return new SpringControlAccessibleObject(this);
                }

                public virtual bool GetSolid()
                {
                    return this.solid;
                }

                protected override void OnGotFocus(EventArgs e)
                {
                    if (!this.focused)
                    {
                        this.focused = true;
                        base.Invalidate();
                    }
                    base.OnGotFocus(e);
                }

                protected override void OnLostFocus(EventArgs e)
                {
                    if (this.focused)
                    {
                        this.focused = false;
                        base.Invalidate();
                    }
                    base.OnLostFocus(e);
                }

                protected override void OnMouseDown(MouseEventArgs e)
                {
                    this.SetSolid(!this.solid);
                    base.Focus();
                }

                protected override void OnPaint(PaintEventArgs e)
                {
                    Rectangle clientRectangle = base.ClientRectangle;
                    if (this.solid)
                    {
                        e.Graphics.FillRectangle(SystemBrushes.ControlDark, clientRectangle);
                        e.Graphics.DrawRectangle(SystemPens.WindowFrame, clientRectangle.X, clientRectangle.Y, clientRectangle.Width - 1, clientRectangle.Height - 1);
                    }
                    else
                    {
                        ControlPaint.DrawFocusRectangle(e.Graphics, clientRectangle);
                    }
                    if (this.focused)
                    {
                        clientRectangle.Inflate(-2, -2);
                        ControlPaint.DrawFocusRectangle(e.Graphics, clientRectangle);
                    }
                }

                protected override bool ProcessDialogChar(char charCode)
                {
                    if (charCode == ' ')
                    {
                        this.SetSolid(!this.solid);
                        return true;
                    }
                    return base.ProcessDialogChar(charCode);
                }

                protected override bool ProcessDialogKey(Keys keyData)
                {
                    if (((keyData & Keys.KeyCode) == Keys.Enter) && ((keyData & (Keys.Alt | Keys.Control)) == Keys.None))
                    {
                        this.picker.Teardown(true);
                        return true;
                    }
                    if (((keyData & Keys.KeyCode) == Keys.Escape) && ((keyData & (Keys.Alt | Keys.Control)) == Keys.None))
                    {
                        this.picker.Teardown(false);
                        return true;
                    }
                    if (((keyData & Keys.KeyCode) != Keys.Tab) || ((keyData & (Keys.Alt | Keys.Control)) != Keys.None))
                    {
                        return base.ProcessDialogKey(keyData);
                    }
                    for (int i = 0; i < this.picker.tabOrder.Length; i++)
                    {
                        if (this.picker.tabOrder[i] == this)
                        {
                            i += ((keyData & Keys.Shift) == Keys.None) ? 1 : -1;
                            i = (i < 0) ? (i + this.picker.tabOrder.Length) : (i % this.picker.tabOrder.Length);
                            this.picker.tabOrder[i].Focus();
                            break;
                        }
                    }
                    return true;
                }

                public virtual void SetSolid(bool value)
                {
                    if (this.solid != value)
                    {
                        this.solid = value;
                        this.picker.SetValue();
                        base.Invalidate();
                    }
                }

                private class SpringControlAccessibleObject : Control.ControlAccessibleObject
                {
                    public SpringControlAccessibleObject(AnchorEditor.AnchorUI.SpringControl owner) : base(owner)
                    {
                    }

                    public override AccessibleStates State
                    {
                        get
                        {
                            AccessibleStates state = base.State;
                            if (((AnchorEditor.AnchorUI.SpringControl) base.Owner).GetSolid())
                            {
                                state |= AccessibleStates.Selected;
                            }
                            return state;
                        }
                    }
                }
            }
        }
    }
}

