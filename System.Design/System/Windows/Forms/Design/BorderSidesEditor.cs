namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class BorderSidesEditor : UITypeEditor
    {
        private BorderSidesEditorUI borderSidesEditorUI;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc == null)
                {
                    return value;
                }
                if (this.borderSidesEditorUI == null)
                {
                    this.borderSidesEditorUI = new BorderSidesEditorUI(this);
                }
                this.borderSidesEditorUI.Start(edSvc, value);
                edSvc.DropDownControl(this.borderSidesEditorUI);
                if (this.borderSidesEditorUI.Value != null)
                {
                    value = this.borderSidesEditorUI.Value;
                }
                this.borderSidesEditorUI.End();
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        private class BorderSidesEditorUI : UserControl
        {
            private CheckBox allCheckBox;
            private bool allChecked;
            private CheckBox bottomCheckBox;
            private object currentValue;
            private BorderSidesEditor editor;
            private IWindowsFormsEditorService edSvc;
            private CheckBox leftCheckBox;
            private CheckBox noneCheckBox;
            private bool noneChecked;
            private object originalValue;
            private CheckBox rightCheckBox;
            private Label splitterLabel;
            private TableLayoutPanel tableLayoutPanel1;
            private CheckBox topCheckBox;
            private bool updateCurrentValue;

            public BorderSidesEditorUI(BorderSidesEditor editor)
            {
                this.editor = editor;
                this.End();
                this.InitializeComponent();
                base.Size = base.PreferredSize;
            }

            private void allCheckBox_CheckedChanged(object sender, EventArgs e)
            {
                CheckBox box = sender as CheckBox;
                if (box.Checked)
                {
                    this.noneCheckBox.Checked = false;
                    this.topCheckBox.Checked = true;
                    this.bottomCheckBox.Checked = true;
                    this.leftCheckBox.Checked = true;
                    this.rightCheckBox.Checked = true;
                }
                this.UpdateCurrentValue();
            }

            private void allCheckBoxClicked(object sender, EventArgs e)
            {
                if (this.allChecked)
                {
                    this.allCheckBox.Checked = true;
                }
            }

            private void bottomCheckBox_CheckedChanged(object sender, EventArgs e)
            {
                CheckBox box = sender as CheckBox;
                if (box.Checked)
                {
                    this.noneCheckBox.Checked = false;
                }
                else if (this.allCheckBox.Checked)
                {
                    this.allCheckBox.Checked = false;
                }
                this.UpdateCurrentValue();
            }

            public void End()
            {
                this.edSvc = null;
                this.originalValue = null;
                this.currentValue = null;
                this.updateCurrentValue = false;
            }

            private void InitializeComponent()
            {
                ComponentResourceManager manager = new ComponentResourceManager(typeof(BorderSidesEditor));
                this.tableLayoutPanel1 = new TableLayoutPanel();
                this.noneCheckBox = new CheckBox();
                this.allCheckBox = new CheckBox();
                this.topCheckBox = new CheckBox();
                this.bottomCheckBox = new CheckBox();
                this.rightCheckBox = new CheckBox();
                this.leftCheckBox = new CheckBox();
                this.splitterLabel = new Label();
                this.tableLayoutPanel1.SuspendLayout();
                base.SuspendLayout();
                manager.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
                this.tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                this.tableLayoutPanel1.BackColor = SystemColors.Window;
                this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
                this.tableLayoutPanel1.Controls.Add(this.noneCheckBox, 0, 0);
                this.tableLayoutPanel1.Controls.Add(this.allCheckBox, 0, 2);
                this.tableLayoutPanel1.Controls.Add(this.topCheckBox, 0, 3);
                this.tableLayoutPanel1.Controls.Add(this.bottomCheckBox, 0, 4);
                this.tableLayoutPanel1.Controls.Add(this.rightCheckBox, 0, 6);
                this.tableLayoutPanel1.Controls.Add(this.leftCheckBox, 0, 5);
                this.tableLayoutPanel1.Controls.Add(this.splitterLabel, 0, 1);
                this.tableLayoutPanel1.Name = "tableLayoutPanel1";
                this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
                this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
                this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
                this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
                this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
                this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
                this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
                this.tableLayoutPanel1.Margin = new Padding(0);
                manager.ApplyResources(this.noneCheckBox, "noneCheckBox");
                this.noneCheckBox.Name = "noneCheckBox";
                this.noneCheckBox.Margin = new Padding(3, 3, 3, 1);
                manager.ApplyResources(this.allCheckBox, "allCheckBox");
                this.allCheckBox.Name = "allCheckBox";
                this.allCheckBox.Margin = new Padding(3, 3, 3, 1);
                manager.ApplyResources(this.topCheckBox, "topCheckBox");
                this.topCheckBox.Margin = new Padding(20, 1, 3, 1);
                this.topCheckBox.Name = "topCheckBox";
                manager.ApplyResources(this.bottomCheckBox, "bottomCheckBox");
                this.bottomCheckBox.Margin = new Padding(20, 1, 3, 1);
                this.bottomCheckBox.Name = "bottomCheckBox";
                manager.ApplyResources(this.rightCheckBox, "rightCheckBox");
                this.rightCheckBox.Margin = new Padding(20, 1, 3, 1);
                this.rightCheckBox.Name = "rightCheckBox";
                manager.ApplyResources(this.leftCheckBox, "leftCheckBox");
                this.leftCheckBox.Margin = new Padding(20, 1, 3, 1);
                this.leftCheckBox.Name = "leftCheckBox";
                manager.ApplyResources(this.splitterLabel, "splitterLabel");
                this.splitterLabel.BackColor = SystemColors.ControlDark;
                this.splitterLabel.Name = "splitterLabel";
                manager.ApplyResources(this, "$this");
                base.Controls.Add(this.tableLayoutPanel1);
                base.Padding = new Padding(1, 1, 1, 1);
                base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                base.AutoScaleMode = AutoScaleMode.Font;
                base.AutoScaleDimensions = new SizeF(6f, 13f);
                this.tableLayoutPanel1.ResumeLayout(false);
                this.tableLayoutPanel1.PerformLayout();
                base.ResumeLayout(false);
                base.PerformLayout();
                this.rightCheckBox.CheckedChanged += new EventHandler(this.rightCheckBox_CheckedChanged);
                this.leftCheckBox.CheckedChanged += new EventHandler(this.leftCheckBox_CheckedChanged);
                this.bottomCheckBox.CheckedChanged += new EventHandler(this.bottomCheckBox_CheckedChanged);
                this.topCheckBox.CheckedChanged += new EventHandler(this.topCheckBox_CheckedChanged);
                this.noneCheckBox.CheckedChanged += new EventHandler(this.noneCheckBox_CheckedChanged);
                this.allCheckBox.CheckedChanged += new EventHandler(this.allCheckBox_CheckedChanged);
                this.noneCheckBox.Click += new EventHandler(this.noneCheckBoxClicked);
                this.allCheckBox.Click += new EventHandler(this.allCheckBoxClicked);
            }

            private void leftCheckBox_CheckedChanged(object sender, EventArgs e)
            {
                CheckBox box = sender as CheckBox;
                if (box.Checked)
                {
                    this.noneCheckBox.Checked = false;
                }
                else if (this.allCheckBox.Checked)
                {
                    this.allCheckBox.Checked = false;
                }
                this.UpdateCurrentValue();
            }

            private void noneCheckBox_CheckedChanged(object sender, EventArgs e)
            {
                CheckBox box = sender as CheckBox;
                if (box.Checked)
                {
                    this.allCheckBox.Checked = false;
                    this.topCheckBox.Checked = false;
                    this.bottomCheckBox.Checked = false;
                    this.leftCheckBox.Checked = false;
                    this.rightCheckBox.Checked = false;
                }
                this.UpdateCurrentValue();
            }

            private void noneCheckBoxClicked(object sender, EventArgs e)
            {
                if (this.noneChecked)
                {
                    this.noneCheckBox.Checked = true;
                }
            }

            protected override void OnGotFocus(EventArgs e)
            {
                base.OnGotFocus(e);
                this.noneCheckBox.Focus();
            }

            private void ResetCheckBoxState()
            {
                this.allCheckBox.Checked = false;
                this.noneCheckBox.Checked = false;
                this.topCheckBox.Checked = false;
                this.bottomCheckBox.Checked = false;
                this.leftCheckBox.Checked = false;
                this.rightCheckBox.Checked = false;
            }

            private void rightCheckBox_CheckedChanged(object sender, EventArgs e)
            {
                CheckBox box = sender as CheckBox;
                if (box.Checked)
                {
                    this.noneCheckBox.Checked = false;
                }
                else if (this.allCheckBox.Checked)
                {
                    this.allCheckBox.Checked = false;
                }
                this.UpdateCurrentValue();
            }

            private void SetCheckBoxCheckState(ToolStripStatusLabelBorderSides sides)
            {
                this.ResetCheckBoxState();
                if ((sides & ToolStripStatusLabelBorderSides.All) == ToolStripStatusLabelBorderSides.All)
                {
                    this.allCheckBox.Checked = true;
                    this.topCheckBox.Checked = true;
                    this.bottomCheckBox.Checked = true;
                    this.leftCheckBox.Checked = true;
                    this.rightCheckBox.Checked = true;
                    this.allCheckBox.Checked = true;
                }
                else
                {
                    this.noneCheckBox.Checked = 0 == 0;
                    this.topCheckBox.Checked = (sides & ToolStripStatusLabelBorderSides.Top) == ToolStripStatusLabelBorderSides.Top;
                    this.bottomCheckBox.Checked = (sides & ToolStripStatusLabelBorderSides.Bottom) == ToolStripStatusLabelBorderSides.Bottom;
                    this.leftCheckBox.Checked = (sides & ToolStripStatusLabelBorderSides.Left) == ToolStripStatusLabelBorderSides.Left;
                    this.rightCheckBox.Checked = (sides & ToolStripStatusLabelBorderSides.Right) == ToolStripStatusLabelBorderSides.Right;
                }
            }

            public void Start(IWindowsFormsEditorService edSvc, object value)
            {
                this.edSvc = edSvc;
                this.originalValue = this.currentValue = value;
                ToolStripStatusLabelBorderSides sides = (ToolStripStatusLabelBorderSides) value;
                this.SetCheckBoxCheckState(sides);
                this.updateCurrentValue = true;
            }

            private void topCheckBox_CheckedChanged(object sender, EventArgs e)
            {
                CheckBox box = sender as CheckBox;
                if (box.Checked)
                {
                    this.noneCheckBox.Checked = false;
                }
                else if (this.allCheckBox.Checked)
                {
                    this.allCheckBox.Checked = false;
                }
                this.UpdateCurrentValue();
            }

            private void UpdateCurrentValue()
            {
                if (this.updateCurrentValue)
                {
                    ToolStripStatusLabelBorderSides none = ToolStripStatusLabelBorderSides.None;
                    if (this.allCheckBox.Checked)
                    {
                        none |= ToolStripStatusLabelBorderSides.All;
                        this.currentValue = none;
                        this.allChecked = true;
                        this.noneChecked = false;
                    }
                    else
                    {
                        if (this.noneCheckBox.Checked)
                        {
                            none = none;
                        }
                        if (this.topCheckBox.Checked)
                        {
                            none |= ToolStripStatusLabelBorderSides.Top;
                        }
                        if (this.bottomCheckBox.Checked)
                        {
                            none |= ToolStripStatusLabelBorderSides.Bottom;
                        }
                        if (this.leftCheckBox.Checked)
                        {
                            none |= ToolStripStatusLabelBorderSides.Left;
                        }
                        if (this.rightCheckBox.Checked)
                        {
                            none |= ToolStripStatusLabelBorderSides.Right;
                        }
                        if (none == ToolStripStatusLabelBorderSides.None)
                        {
                            this.allChecked = false;
                            this.noneChecked = true;
                            this.noneCheckBox.Checked = true;
                        }
                        if (none == ToolStripStatusLabelBorderSides.All)
                        {
                            this.allChecked = true;
                            this.noneChecked = false;
                            this.allCheckBox.Checked = true;
                        }
                        this.currentValue = none;
                    }
                }
            }

            public IWindowsFormsEditorService EditorService
            {
                get
                {
                    return this.edSvc;
                }
            }

            public object Value
            {
                get
                {
                    return this.currentValue;
                }
            }
        }
    }
}

