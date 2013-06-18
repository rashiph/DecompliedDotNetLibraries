namespace System.Drawing.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class ContentAlignmentEditor : UITypeEditor
    {
        private ContentUI contentUI;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc == null)
                {
                    return value;
                }
                if (this.contentUI == null)
                {
                    this.contentUI = new ContentUI();
                }
                this.contentUI.Start(edSvc, value);
                edSvc.DropDownControl(this.contentUI);
                value = this.contentUI.Value;
                this.contentUI.End();
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        private class ContentUI : Control
        {
            private RadioButton bottomCenter = new RadioButton();
            private RadioButton bottomLeft = new RadioButton();
            private RadioButton bottomRight = new RadioButton();
            private IWindowsFormsEditorService edSvc;
            private RadioButton middleCenter = new RadioButton();
            private RadioButton middleLeft = new RadioButton();
            private RadioButton middleRight = new RadioButton();
            private RadioButton topCenter = new RadioButton();
            private RadioButton topLeft = new RadioButton();
            private RadioButton topRight = new RadioButton();
            private object value;

            public ContentUI()
            {
                this.InitComponent();
            }

            public void End()
            {
                this.edSvc = null;
                this.value = null;
            }

            private void InitComponent()
            {
                base.Size = new Size(0x7d, 0x59);
                this.BackColor = SystemColors.Control;
                this.ForeColor = SystemColors.ControlText;
                base.AccessibleName = System.Drawing.Design.SR.GetString("ContentAlignmentEditorAccName");
                this.topLeft.Size = new Size(0x18, 0x19);
                this.topLeft.TabIndex = 8;
                this.topLeft.Text = "";
                this.topLeft.Appearance = Appearance.Button;
                this.topLeft.Click += new EventHandler(this.OptionClick);
                this.topLeft.AccessibleName = System.Drawing.Design.SR.GetString("ContentAlignmentEditorTopLeftAccName");
                this.topCenter.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this.topCenter.Location = new Point(0x20, 0);
                this.topCenter.Size = new Size(0x3b, 0x19);
                this.topCenter.TabIndex = 0;
                this.topCenter.Text = "";
                this.topCenter.Appearance = Appearance.Button;
                this.topCenter.Click += new EventHandler(this.OptionClick);
                this.topCenter.AccessibleName = System.Drawing.Design.SR.GetString("ContentAlignmentEditorTopCenterAccName");
                this.topRight.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                this.topRight.Location = new Point(0x63, 0);
                this.topRight.Size = new Size(0x18, 0x19);
                this.topRight.TabIndex = 1;
                this.topRight.Text = "";
                this.topRight.Appearance = Appearance.Button;
                this.topRight.Click += new EventHandler(this.OptionClick);
                this.topRight.AccessibleName = System.Drawing.Design.SR.GetString("ContentAlignmentEditorTopRightAccName");
                this.middleLeft.Location = new Point(0, 0x20);
                this.middleLeft.Size = new Size(0x18, 0x19);
                this.middleLeft.TabIndex = 2;
                this.middleLeft.Text = "";
                this.middleLeft.Appearance = Appearance.Button;
                this.middleLeft.Click += new EventHandler(this.OptionClick);
                this.middleLeft.AccessibleName = System.Drawing.Design.SR.GetString("ContentAlignmentEditorMiddleLeftAccName");
                this.middleCenter.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this.middleCenter.Location = new Point(0x20, 0x20);
                this.middleCenter.Size = new Size(0x3b, 0x19);
                this.middleCenter.TabIndex = 3;
                this.middleCenter.Text = "";
                this.middleCenter.Appearance = Appearance.Button;
                this.middleCenter.Click += new EventHandler(this.OptionClick);
                this.middleCenter.AccessibleName = System.Drawing.Design.SR.GetString("ContentAlignmentEditorMiddleCenterAccName");
                this.middleRight.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                this.middleRight.Location = new Point(0x63, 0x20);
                this.middleRight.Size = new Size(0x18, 0x19);
                this.middleRight.TabIndex = 4;
                this.middleRight.Text = "";
                this.middleRight.Appearance = Appearance.Button;
                this.middleRight.Click += new EventHandler(this.OptionClick);
                this.middleRight.AccessibleName = System.Drawing.Design.SR.GetString("ContentAlignmentEditorMiddleRightAccName");
                this.bottomLeft.Location = new Point(0, 0x40);
                this.bottomLeft.Size = new Size(0x18, 0x19);
                this.bottomLeft.TabIndex = 5;
                this.bottomLeft.Text = "";
                this.bottomLeft.Appearance = Appearance.Button;
                this.bottomLeft.Click += new EventHandler(this.OptionClick);
                this.bottomLeft.AccessibleName = System.Drawing.Design.SR.GetString("ContentAlignmentEditorBottomLeftAccName");
                this.bottomCenter.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this.bottomCenter.Location = new Point(0x20, 0x40);
                this.bottomCenter.Size = new Size(0x3b, 0x19);
                this.bottomCenter.TabIndex = 6;
                this.bottomCenter.Text = "";
                this.bottomCenter.Appearance = Appearance.Button;
                this.bottomCenter.Click += new EventHandler(this.OptionClick);
                this.bottomCenter.AccessibleName = System.Drawing.Design.SR.GetString("ContentAlignmentEditorBottomCenterAccName");
                this.bottomRight.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                this.bottomRight.Location = new Point(0x63, 0x40);
                this.bottomRight.Size = new Size(0x18, 0x19);
                this.bottomRight.TabIndex = 7;
                this.bottomRight.Text = "";
                this.bottomRight.Appearance = Appearance.Button;
                this.bottomRight.Click += new EventHandler(this.OptionClick);
                this.bottomRight.AccessibleName = System.Drawing.Design.SR.GetString("ContentAlignmentEditorBottomRightAccName");
                base.Controls.Clear();
                base.Controls.AddRange(new Control[] { this.bottomRight, this.bottomCenter, this.bottomLeft, this.middleRight, this.middleCenter, this.middleLeft, this.topRight, this.topCenter, this.topLeft });
            }

            protected override bool IsInputKey(Keys keyData)
            {
                switch (keyData)
                {
                    case Keys.Left:
                    case Keys.Up:
                    case Keys.Right:
                    case Keys.Down:
                        return false;
                }
                return base.IsInputKey(keyData);
            }

            private void OptionClick(object sender, EventArgs e)
            {
                this.value = this.Align;
                this.edSvc.CloseDropDown();
            }

            protected override bool ProcessDialogKey(Keys keyData)
            {
                RadioButton checkedControl = this.CheckedControl;
                if ((keyData & Keys.KeyCode) == Keys.Left)
                {
                    if (checkedControl == this.bottomRight)
                    {
                        this.CheckedControl = this.bottomCenter;
                    }
                    else if (checkedControl == this.middleRight)
                    {
                        this.CheckedControl = this.middleCenter;
                    }
                    else if (checkedControl == this.topRight)
                    {
                        this.CheckedControl = this.topCenter;
                    }
                    else if (checkedControl == this.bottomCenter)
                    {
                        this.CheckedControl = this.bottomLeft;
                    }
                    else if (checkedControl == this.middleCenter)
                    {
                        this.CheckedControl = this.middleLeft;
                    }
                    else if (checkedControl == this.topCenter)
                    {
                        this.CheckedControl = this.topLeft;
                    }
                    return true;
                }
                if ((keyData & Keys.KeyCode) == Keys.Right)
                {
                    if (checkedControl == this.bottomLeft)
                    {
                        this.CheckedControl = this.bottomCenter;
                    }
                    else if (checkedControl == this.middleLeft)
                    {
                        this.CheckedControl = this.middleCenter;
                    }
                    else if (checkedControl == this.topLeft)
                    {
                        this.CheckedControl = this.topCenter;
                    }
                    else if (checkedControl == this.bottomCenter)
                    {
                        this.CheckedControl = this.bottomRight;
                    }
                    else if (checkedControl == this.middleCenter)
                    {
                        this.CheckedControl = this.middleRight;
                    }
                    else if (checkedControl == this.topCenter)
                    {
                        this.CheckedControl = this.topRight;
                    }
                    return true;
                }
                if ((keyData & Keys.KeyCode) == Keys.Up)
                {
                    if (checkedControl == this.bottomRight)
                    {
                        this.CheckedControl = this.middleRight;
                    }
                    else if (checkedControl == this.middleRight)
                    {
                        this.CheckedControl = this.topRight;
                    }
                    else if (checkedControl == this.bottomCenter)
                    {
                        this.CheckedControl = this.middleCenter;
                    }
                    else if (checkedControl == this.middleCenter)
                    {
                        this.CheckedControl = this.topCenter;
                    }
                    else if (checkedControl == this.bottomLeft)
                    {
                        this.CheckedControl = this.middleLeft;
                    }
                    else if (checkedControl == this.middleLeft)
                    {
                        this.CheckedControl = this.topLeft;
                    }
                    return true;
                }
                if ((keyData & Keys.KeyCode) == Keys.Down)
                {
                    if (checkedControl == this.topRight)
                    {
                        this.CheckedControl = this.middleRight;
                    }
                    else if (checkedControl == this.middleRight)
                    {
                        this.CheckedControl = this.bottomRight;
                    }
                    else if (checkedControl == this.topCenter)
                    {
                        this.CheckedControl = this.middleCenter;
                    }
                    else if (checkedControl == this.middleCenter)
                    {
                        this.CheckedControl = this.bottomCenter;
                    }
                    else if (checkedControl == this.topLeft)
                    {
                        this.CheckedControl = this.middleLeft;
                    }
                    else if (checkedControl == this.middleLeft)
                    {
                        this.CheckedControl = this.bottomLeft;
                    }
                    return true;
                }
                if ((keyData & Keys.KeyCode) == Keys.Space)
                {
                    this.OptionClick(this, EventArgs.Empty);
                    return true;
                }
                if (((keyData & Keys.KeyCode) == Keys.Enter) && ((keyData & (Keys.Alt | Keys.Control)) == Keys.None))
                {
                    this.OptionClick(this, EventArgs.Empty);
                    return true;
                }
                if (((keyData & Keys.KeyCode) == Keys.Escape) && ((keyData & (Keys.Alt | Keys.Control)) == Keys.None))
                {
                    this.edSvc.CloseDropDown();
                    return true;
                }
                if (((keyData & Keys.KeyCode) != Keys.Tab) || ((keyData & (Keys.Alt | Keys.Control)) != Keys.None))
                {
                    return base.ProcessDialogKey(keyData);
                }
                int num = this.CheckedControl.TabIndex + (((keyData & Keys.Shift) == Keys.None) ? 1 : -1);
                if (num < 0)
                {
                    num = base.Controls.Count - 1;
                }
                else if (num >= base.Controls.Count)
                {
                    num = 0;
                }
                for (int i = 0; i < base.Controls.Count; i++)
                {
                    if ((base.Controls[i] is RadioButton) && (base.Controls[i].TabIndex == num))
                    {
                        this.CheckedControl = (RadioButton) base.Controls[i];
                        return true;
                    }
                }
                return true;
            }

            public void Start(IWindowsFormsEditorService edSvc, object value)
            {
                ContentAlignment middleLeft;
                this.edSvc = edSvc;
                this.value = value;
                if (value == null)
                {
                    middleLeft = ContentAlignment.MiddleLeft;
                }
                else
                {
                    middleLeft = (ContentAlignment) value;
                }
                this.Align = middleLeft;
            }

            private ContentAlignment Align
            {
                get
                {
                    if (this.topLeft.Checked)
                    {
                        return ContentAlignment.TopLeft;
                    }
                    if (this.topCenter.Checked)
                    {
                        return ContentAlignment.TopCenter;
                    }
                    if (this.topRight.Checked)
                    {
                        return ContentAlignment.TopRight;
                    }
                    if (this.middleLeft.Checked)
                    {
                        return ContentAlignment.MiddleLeft;
                    }
                    if (this.middleCenter.Checked)
                    {
                        return ContentAlignment.MiddleCenter;
                    }
                    if (this.middleRight.Checked)
                    {
                        return ContentAlignment.MiddleRight;
                    }
                    if (this.bottomLeft.Checked)
                    {
                        return ContentAlignment.BottomLeft;
                    }
                    if (this.bottomCenter.Checked)
                    {
                        return ContentAlignment.BottomCenter;
                    }
                    return ContentAlignment.BottomRight;
                }
                set
                {
                    ContentAlignment alignment = value;
                    if (alignment <= ContentAlignment.MiddleCenter)
                    {
                        switch (alignment)
                        {
                            case ContentAlignment.TopLeft:
                                this.topLeft.Checked = true;
                                return;

                            case ContentAlignment.TopCenter:
                                this.topCenter.Checked = true;
                                return;

                            case (ContentAlignment.TopCenter | ContentAlignment.TopLeft):
                                return;

                            case ContentAlignment.TopRight:
                                this.topRight.Checked = true;
                                return;

                            case ContentAlignment.MiddleLeft:
                                this.middleLeft.Checked = true;
                                return;

                            case ContentAlignment.MiddleCenter:
                                this.middleCenter.Checked = true;
                                return;
                        }
                    }
                    else if (alignment <= ContentAlignment.BottomLeft)
                    {
                        switch (alignment)
                        {
                            case ContentAlignment.MiddleRight:
                                this.middleRight.Checked = true;
                                break;

                            case ContentAlignment.BottomLeft:
                                this.bottomLeft.Checked = true;
                                break;
                        }
                    }
                    else
                    {
                        switch (alignment)
                        {
                            case ContentAlignment.BottomCenter:
                                this.bottomCenter.Checked = true;
                                return;

                            case ContentAlignment.BottomRight:
                                this.bottomRight.Checked = true;
                                break;

                            default:
                                return;
                        }
                    }
                }
            }

            private RadioButton CheckedControl
            {
                get
                {
                    for (int i = 0; i < base.Controls.Count; i++)
                    {
                        if ((base.Controls[i] is RadioButton) && ((RadioButton) base.Controls[i]).Checked)
                        {
                            return (RadioButton) base.Controls[i];
                        }
                    }
                    return this.middleLeft;
                }
                set
                {
                    this.CheckedControl.Checked = false;
                    value.Checked = true;
                    if (value.IsHandleCreated)
                    {
                        System.Drawing.Design.UnsafeNativeMethods.NotifyWinEvent(0x8005, new HandleRef(value, value.Handle), -4, 0);
                    }
                }
            }

            protected override bool ShowFocusCues
            {
                get
                {
                    return true;
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
        }
    }
}

