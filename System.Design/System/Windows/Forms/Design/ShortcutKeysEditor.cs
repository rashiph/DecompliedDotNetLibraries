namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ShortcutKeysEditor : UITypeEditor
    {
        private ShortcutKeysUI shortcutKeysUI;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc == null)
                {
                    return value;
                }
                if (this.shortcutKeysUI == null)
                {
                    this.shortcutKeysUI = new ShortcutKeysUI(this);
                }
                this.shortcutKeysUI.Start(edSvc, value);
                edSvc.DropDownControl(this.shortcutKeysUI);
                if (this.shortcutKeysUI.Value != null)
                {
                    value = this.shortcutKeysUI.Value;
                }
                this.shortcutKeysUI.End();
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        private class ShortcutKeysUI : UserControl
        {
            private Button btnReset;
            private CheckBox chkAlt;
            private CheckBox chkCtrl;
            private CheckBox chkShift;
            private ComboBox cmbKey;
            private object currentValue;
            private ShortcutKeysEditor editor;
            private IWindowsFormsEditorService edSvc;
            private TypeConverter keysConverter;
            private Label lblKey;
            private Label lblModifiers;
            private object originalValue;
            private TableLayoutPanel tlpInner;
            private TableLayoutPanel tlpOuter;
            private Keys unknownKeyCode;
            private bool updateCurrentValue;
            private static Keys[] validKeys = new Keys[] { 
                Keys.A, Keys.B, Keys.C, Keys.D, Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.Delete, Keys.Down, 
                Keys.E, Keys.End, Keys.F, Keys.F1, Keys.F10, Keys.F11, Keys.F12, Keys.F13, Keys.F14, Keys.F15, Keys.F16, Keys.F17, Keys.F18, Keys.F19, Keys.F2, Keys.F20, 
                Keys.F21, Keys.F22, Keys.F23, Keys.F24, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.G, Keys.H, Keys.I, Keys.Insert, Keys.J, 
                Keys.K, Keys.L, Keys.Left, Keys.M, Keys.N, Keys.NumLock, Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9, 
                Keys.O, Keys.Oem102, Keys.OemClear, Keys.Oem6, Keys.Oemcomma, Keys.OemMinus, Keys.Oem4, Keys.OemPeriod, Keys.Oem5, Keys.Oemplus, Keys.Oem2, Keys.Oem7, Keys.Oem1, Keys.Oem3, Keys.P, Keys.Pause, 
                Keys.Q, Keys.R, Keys.Right, Keys.S, Keys.Space, Keys.T, Keys.Tab, Keys.U, Keys.Up, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z
             };

            public ShortcutKeysUI(ShortcutKeysEditor editor)
            {
                this.editor = editor;
                this.keysConverter = null;
                this.End();
                this.InitializeComponent();
                this.AdjustSize();
            }

            private void AdjustSize()
            {
                ComponentResourceManager manager = new ComponentResourceManager(typeof(ShortcutKeysEditor));
                Size size = (Size) manager.GetObject("btnReset.Size");
                base.Size = new Size((base.Size.Width + this.btnReset.Size.Width) - size.Width, base.Size.Height);
            }

            private void btnReset_Click(object sender, EventArgs e)
            {
                this.chkCtrl.Checked = false;
                this.chkAlt.Checked = false;
                this.chkShift.Checked = false;
                this.cmbKey.SelectedIndex = -1;
            }

            private void chkModifier_CheckedChanged(object sender, EventArgs e)
            {
                this.UpdateCurrentValue();
            }

            private void cmbKey_SelectedIndexChanged(object sender, EventArgs e)
            {
                this.UpdateCurrentValue();
            }

            public void End()
            {
                this.edSvc = null;
                this.originalValue = null;
                this.currentValue = null;
                this.updateCurrentValue = false;
                if (this.unknownKeyCode != Keys.None)
                {
                    this.cmbKey.Items.RemoveAt(0);
                    this.unknownKeyCode = Keys.None;
                }
            }

            private void InitializeComponent()
            {
                ComponentResourceManager manager = new ComponentResourceManager(typeof(ShortcutKeysEditor));
                this.tlpOuter = new TableLayoutPanel();
                this.lblModifiers = new Label();
                this.chkCtrl = new CheckBox();
                this.chkAlt = new CheckBox();
                this.chkShift = new CheckBox();
                this.tlpInner = new TableLayoutPanel();
                this.lblKey = new Label();
                this.cmbKey = new ComboBox();
                this.btnReset = new Button();
                this.tlpOuter.SuspendLayout();
                this.tlpInner.SuspendLayout();
                base.SuspendLayout();
                manager.ApplyResources(this.tlpOuter, "tlpOuter");
                this.tlpOuter.ColumnCount = 3;
                this.tlpOuter.ColumnStyles.Add(new ColumnStyle());
                this.tlpOuter.ColumnStyles.Add(new ColumnStyle());
                this.tlpOuter.ColumnStyles.Add(new ColumnStyle());
                this.tlpOuter.Controls.Add(this.lblModifiers, 0, 0);
                this.tlpOuter.Controls.Add(this.chkCtrl, 0, 1);
                this.tlpOuter.Controls.Add(this.chkShift, 1, 1);
                this.tlpOuter.Controls.Add(this.chkAlt, 2, 1);
                this.tlpOuter.Name = "tlpOuter";
                this.tlpOuter.RowCount = 2;
                this.tlpOuter.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
                this.tlpOuter.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
                manager.ApplyResources(this.lblModifiers, "lblModifiers");
                this.tlpOuter.SetColumnSpan(this.lblModifiers, 3);
                this.lblModifiers.Name = "lblModifiers";
                manager.ApplyResources(this.chkCtrl, "chkCtrl");
                this.chkCtrl.Name = "chkCtrl";
                this.chkCtrl.Margin = new Padding(12, 3, 3, 3);
                this.chkCtrl.CheckedChanged += new EventHandler(this.chkModifier_CheckedChanged);
                manager.ApplyResources(this.chkAlt, "chkAlt");
                this.chkAlt.Name = "chkAlt";
                this.chkAlt.CheckedChanged += new EventHandler(this.chkModifier_CheckedChanged);
                manager.ApplyResources(this.chkShift, "chkShift");
                this.chkShift.Name = "chkShift";
                this.chkShift.CheckedChanged += new EventHandler(this.chkModifier_CheckedChanged);
                manager.ApplyResources(this.tlpInner, "tlpInner");
                this.tlpInner.ColumnCount = 2;
                this.tlpInner.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                this.tlpInner.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                this.tlpInner.Controls.Add(this.lblKey, 0, 0);
                this.tlpInner.Controls.Add(this.cmbKey, 0, 1);
                this.tlpInner.Controls.Add(this.btnReset, 1, 1);
                this.tlpInner.Name = "tlpInner";
                this.tlpInner.RowCount = 2;
                this.tlpInner.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
                this.tlpInner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                manager.ApplyResources(this.lblKey, "lblKey");
                this.tlpInner.SetColumnSpan(this.lblKey, 2);
                this.lblKey.Name = "lblKey";
                manager.ApplyResources(this.cmbKey, "cmbKey");
                this.cmbKey.DropDownStyle = ComboBoxStyle.DropDownList;
                this.cmbKey.Name = "cmbKey";
                this.cmbKey.Margin = new Padding(9, 4, 3, 3);
                this.cmbKey.Padding = this.cmbKey.Margin;
                foreach (Keys keys in validKeys)
                {
                    this.cmbKey.Items.Add(this.KeysConverter.ConvertToString(keys));
                }
                this.cmbKey.SelectedIndexChanged += new EventHandler(this.cmbKey_SelectedIndexChanged);
                manager.ApplyResources(this.btnReset, "btnReset");
                this.btnReset.Name = "btnReset";
                this.btnReset.Click += new EventHandler(this.btnReset_Click);
                manager.ApplyResources(this, "$this");
                base.Controls.AddRange(new Control[] { this.tlpInner, this.tlpOuter });
                base.Name = "ShortcutKeysUI";
                base.Padding = new Padding(4);
                this.tlpOuter.ResumeLayout(false);
                this.tlpOuter.PerformLayout();
                this.tlpInner.ResumeLayout(false);
                this.tlpInner.PerformLayout();
                base.ResumeLayout(false);
                base.PerformLayout();
            }

            private static bool IsValidKey(Keys keyCode)
            {
                foreach (Keys keys in validKeys)
                {
                    if (keys == keyCode)
                    {
                        return true;
                    }
                }
                return false;
            }

            protected override void OnGotFocus(EventArgs e)
            {
                base.OnGotFocus(e);
                this.chkCtrl.Focus();
            }

            protected override bool ProcessDialogKey(Keys keyData)
            {
                Keys keys = keyData & Keys.KeyCode;
                Keys keys2 = keyData & ~Keys.KeyCode;
                Keys keys3 = keys;
                if (keys3 != Keys.Tab)
                {
                    switch (keys3)
                    {
                        case Keys.Left:
                            if (((keys2 & (Keys.Alt | Keys.Control)) != Keys.None) || !this.chkCtrl.Focused)
                            {
                                break;
                            }
                            this.btnReset.Focus();
                            return true;

                        case Keys.Right:
                            if ((keys2 & (Keys.Alt | Keys.Control)) != Keys.None)
                            {
                                break;
                            }
                            if (!this.chkShift.Focused)
                            {
                                if (!this.btnReset.Focused)
                                {
                                    break;
                                }
                                this.chkCtrl.Focus();
                                return true;
                            }
                            this.cmbKey.Focus();
                            return true;

                        case Keys.Escape:
                            if ((!this.cmbKey.Focused || ((keys2 & (Keys.Alt | Keys.Control)) != Keys.None)) || !this.cmbKey.DroppedDown)
                            {
                                this.currentValue = this.originalValue;
                            }
                            break;
                    }
                }
                else if ((keys2 == Keys.Shift) && this.chkCtrl.Focused)
                {
                    this.btnReset.Focus();
                    return true;
                }
                return base.ProcessDialogKey(keyData);
            }

            public void Start(IWindowsFormsEditorService edSvc, object value)
            {
                this.edSvc = edSvc;
                this.originalValue = this.currentValue = value;
                Keys keys = (Keys) value;
                this.chkCtrl.Checked = (keys & Keys.Control) != Keys.None;
                this.chkAlt.Checked = (keys & Keys.Alt) != Keys.None;
                this.chkShift.Checked = (keys & Keys.Shift) != Keys.None;
                Keys keyCode = keys & Keys.KeyCode;
                if (keyCode == Keys.None)
                {
                    this.cmbKey.SelectedIndex = -1;
                }
                else if (IsValidKey(keyCode))
                {
                    this.cmbKey.SelectedItem = this.KeysConverter.ConvertToString(keyCode);
                }
                else
                {
                    this.cmbKey.Items.Insert(0, System.Design.SR.GetString("ShortcutKeys_InvalidKey"));
                    this.cmbKey.SelectedIndex = 0;
                    this.unknownKeyCode = keyCode;
                }
                this.updateCurrentValue = true;
            }

            private void UpdateCurrentValue()
            {
                if (this.updateCurrentValue)
                {
                    int selectedIndex = this.cmbKey.SelectedIndex;
                    Keys none = Keys.None;
                    if (this.chkCtrl.Checked)
                    {
                        none |= Keys.Control;
                    }
                    if (this.chkAlt.Checked)
                    {
                        none |= Keys.Alt;
                    }
                    if (this.chkShift.Checked)
                    {
                        none |= Keys.Shift;
                    }
                    if ((this.unknownKeyCode != Keys.None) && (selectedIndex == 0))
                    {
                        none |= this.unknownKeyCode;
                    }
                    else if (selectedIndex != -1)
                    {
                        none |= validKeys[(this.unknownKeyCode == Keys.None) ? selectedIndex : (selectedIndex - 1)];
                    }
                    this.currentValue = none;
                }
            }

            public IWindowsFormsEditorService EditorService
            {
                get
                {
                    return this.edSvc;
                }
            }

            private TypeConverter KeysConverter
            {
                get
                {
                    if (this.keysConverter == null)
                    {
                        this.keysConverter = TypeDescriptor.GetConverter(typeof(Keys));
                    }
                    return this.keysConverter;
                }
            }

            public object Value
            {
                get
                {
                    if ((((Keys) this.currentValue) & Keys.KeyCode) == Keys.None)
                    {
                        return Keys.None;
                    }
                    return this.currentValue;
                }
            }
        }
    }
}

