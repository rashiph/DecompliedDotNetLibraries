namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    internal class FormatControl : UserControl
    {
        private IContainer components;
        private const int CurrencyIndex = 2;
        private const int CustomIndex = 5;
        private TextBox customStringTextBox = new TextBox();
        private ListBox dateTimeFormatsListBox;
        private static DateTime dateTimeFormatValue = DateTime.Now;
        private const int DateTimeIndex = 3;
        private NumericUpDown decimalPlacesUpDown;
        private bool dirty;
        private Label explanationLabel;
        private GroupBox formatGroupBox;
        private Label formatTypeLabel;
        private ListBox formatTypeListBox;
        private bool loaded;
        private const int NoFormattingIndex = 0;
        private Label nullValueLabel;
        private TextBox nullValueTextBox;
        private const int NumericIndex = 1;
        private GroupBox sampleGroupBox;
        private Label sampleLabel;
        private const int ScientificIndex = 4;
        private Label secondRowLabel;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private Label thirdRowLabel;

        public FormatControl()
        {
            this.InitializeComponent();
        }

        private void customStringTextBox_TextChanged(object sender, EventArgs e)
        {
            CustomFormatType selectedItem = this.formatTypeListBox.SelectedItem as CustomFormatType;
            this.sampleLabel.Text = selectedItem.SampleString;
            this.dirty = true;
        }

        private void dateTimeFormatsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            FormatTypeClass selectedItem = this.formatTypeListBox.SelectedItem as FormatTypeClass;
            this.sampleLabel.Text = selectedItem.SampleString;
            this.dirty = true;
        }

        private void decimalPlacesUpDown_ValueChanged(object sender, EventArgs e)
        {
            FormatTypeClass selectedItem = this.formatTypeListBox.SelectedItem as FormatTypeClass;
            this.sampleLabel.Text = selectedItem.SampleString;
            this.dirty = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void FormatControl_Load(object sender, EventArgs e)
        {
            if (!this.loaded)
            {
                this.nullValueLabel.Text = System.Design.SR.GetString("BindingFormattingDialogNullValue");
                int width = this.nullValueLabel.Width;
                int height = this.nullValueLabel.Height;
                this.secondRowLabel.Text = System.Design.SR.GetString("BindingFormattingDialogDecimalPlaces");
                width = Math.Max(width, this.secondRowLabel.Width);
                height = Math.Max(height, this.secondRowLabel.Height);
                this.secondRowLabel.Text = System.Design.SR.GetString("BindingFormattingDialogCustomFormat");
                width = Math.Max(width, this.secondRowLabel.Width);
                height = Math.Max(height, this.secondRowLabel.Height);
                this.nullValueLabel.MinimumSize = new Size(width, height);
                this.secondRowLabel.MinimumSize = new Size(width, height);
                this.formatTypeListBox.SelectedIndexChanged -= new EventHandler(this.formatTypeListBox_SelectedIndexChanged);
                this.formatTypeListBox.Items.Clear();
                this.formatTypeListBox.Items.Add(new NoFormattingFormatType());
                this.formatTypeListBox.Items.Add(new NumericFormatType(this));
                this.formatTypeListBox.Items.Add(new CurrencyFormatType(this));
                this.formatTypeListBox.Items.Add(new DateTimeFormatType(this));
                this.formatTypeListBox.Items.Add(new ScientificFormatType(this));
                this.formatTypeListBox.Items.Add(new CustomFormatType(this));
                this.formatTypeListBox.SelectedIndex = 0;
                this.formatTypeListBox.SelectedIndexChanged += new EventHandler(this.formatTypeListBox_SelectedIndexChanged);
                this.UpdateCustomStringTextBox();
                this.UpdateTBLHeight();
                this.UpdateFormatTypeListBoxHeight();
                this.UpdateFormatTypeListBoxItems();
                this.UpdateControlVisibility(this.formatTypeListBox.SelectedItem as FormatTypeClass);
                this.sampleLabel.Text = (this.formatTypeListBox.SelectedItem as FormatTypeClass).SampleString;
                this.explanationLabel.Size = new Size(this.formatGroupBox.Width - 10, 30);
                this.explanationLabel.Text = (this.formatTypeListBox.SelectedItem as FormatTypeClass).TopLabelString;
                this.dirty = false;
                this.FormatControlFinishedLoading();
                this.loaded = true;
            }
        }

        private void FormatControlFinishedLoading()
        {
            BindingFormattingDialog dialog = null;
            FormatStringDialog dialog2 = null;
            for (Control control = base.Parent; control != null; control = control.Parent)
            {
                dialog = control as BindingFormattingDialog;
                dialog2 = control as FormatStringDialog;
                if ((dialog != null) || (dialog2 != null))
                {
                    break;
                }
            }
            if (dialog2 != null)
            {
                dialog2.FormatControlFinishedLoading();
            }
        }

        private void formatGroupBox_Enter(object sender, EventArgs e)
        {
        }

        private void formatTypeListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            FormatTypeClass selectedItem = this.formatTypeListBox.SelectedItem as FormatTypeClass;
            this.UpdateControlVisibility(selectedItem);
            this.sampleLabel.Text = selectedItem.SampleString;
            this.explanationLabel.Text = selectedItem.TopLabelString;
            this.dirty = true;
        }

        public static string FormatTypeStringFromFormatString(string formatString)
        {
            if (string.IsNullOrEmpty(formatString))
            {
                return System.Design.SR.GetString("BindingFormattingDialogFormatTypeNoFormatting");
            }
            if (NumericFormatType.ParseStatic(formatString))
            {
                return System.Design.SR.GetString("BindingFormattingDialogFormatTypeNumeric");
            }
            if (CurrencyFormatType.ParseStatic(formatString))
            {
                return System.Design.SR.GetString("BindingFormattingDialogFormatTypeCurrency");
            }
            if (DateTimeFormatType.ParseStatic(formatString))
            {
                return System.Design.SR.GetString("BindingFormattingDialogFormatTypeDateTime");
            }
            if (ScientificFormatType.ParseStatic(formatString))
            {
                return System.Design.SR.GetString("BindingFormattingDialogFormatTypeScientific");
            }
            return System.Design.SR.GetString("BindingFormattingDialogFormatTypeCustom");
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(FormatControl));
            this.formatGroupBox = new GroupBox();
            this.tableLayoutPanel3 = new TableLayoutPanel();
            this.explanationLabel = new Label();
            this.tableLayoutPanel2 = new TableLayoutPanel();
            this.sampleGroupBox = new GroupBox();
            this.sampleLabel = new Label();
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.secondRowLabel = new Label();
            this.nullValueLabel = new Label();
            this.nullValueTextBox = new TextBox();
            this.decimalPlacesUpDown = new NumericUpDown();
            this.thirdRowLabel = new Label();
            this.dateTimeFormatsListBox = new ListBox();
            this.formatTypeLabel = new Label();
            this.formatTypeListBox = new ListBox();
            this.formatGroupBox.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.sampleGroupBox.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.decimalPlacesUpDown.BeginInit();
            base.SuspendLayout();
            manager.ApplyResources(this.formatGroupBox, "formatGroupBox");
            this.formatGroupBox.Controls.Add(this.tableLayoutPanel3);
            this.formatGroupBox.Dock = DockStyle.Fill;
            this.formatGroupBox.Name = "formatGroupBox";
            this.formatGroupBox.TabStop = false;
            this.formatGroupBox.Enter += new EventHandler(this.formatGroupBox_Enter);
            manager.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel3.Controls.Add(this.explanationLabel, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel2, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.formatTypeLabel, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.formatTypeListBox, 0, 2);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            manager.ApplyResources(this.explanationLabel, "explanationLabel");
            this.tableLayoutPanel3.SetColumnSpan(this.explanationLabel, 2);
            this.explanationLabel.Name = "explanationLabel";
            manager.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.sampleGroupBox, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel1, 0, 1);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel3.SetRowSpan(this.tableLayoutPanel2, 2);
            manager.ApplyResources(this.sampleGroupBox, "sampleGroupBox");
            this.sampleGroupBox.Controls.Add(this.sampleLabel);
            this.sampleGroupBox.MinimumSize = new Size(250, 0x26);
            this.sampleGroupBox.Name = "sampleGroupBox";
            this.sampleGroupBox.TabStop = false;
            manager.ApplyResources(this.sampleLabel, "sampleLabel");
            this.sampleLabel.Name = "sampleLabel";
            manager.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.secondRowLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.nullValueLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.nullValueTextBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.decimalPlacesUpDown, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.thirdRowLabel, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.dateTimeFormatsListBox, 0, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            manager.ApplyResources(this.secondRowLabel, "secondRowLabel");
            this.secondRowLabel.MinimumSize = new Size(0x51, 14);
            this.secondRowLabel.Name = "secondRowLabel";
            manager.ApplyResources(this.nullValueLabel, "nullValueLabel");
            this.nullValueLabel.MinimumSize = new Size(0x51, 14);
            this.nullValueLabel.Name = "nullValueLabel";
            manager.ApplyResources(this.nullValueTextBox, "nullValueTextBox");
            this.nullValueTextBox.Name = "nullValueTextBox";
            this.nullValueTextBox.TextChanged += new EventHandler(this.nullValueTextBox_TextChanged);
            manager.ApplyResources(this.decimalPlacesUpDown, "decimalPlacesUpDown");
            int[] bits = new int[4];
            bits[0] = 6;
            this.decimalPlacesUpDown.Maximum = new decimal(bits);
            this.decimalPlacesUpDown.Name = "decimalPlacesUpDown";
            int[] numArray2 = new int[4];
            numArray2[0] = 2;
            this.decimalPlacesUpDown.Value = new decimal(numArray2);
            this.decimalPlacesUpDown.ValueChanged += new EventHandler(this.decimalPlacesUpDown_ValueChanged);
            manager.ApplyResources(this.thirdRowLabel, "thirdRowLabel");
            this.thirdRowLabel.Name = "thirdRowLabel";
            manager.ApplyResources(this.dateTimeFormatsListBox, "dateTimeFormatsListBox");
            this.dateTimeFormatsListBox.FormattingEnabled = true;
            this.dateTimeFormatsListBox.Name = "dateTimeFormatsListBox";
            manager.ApplyResources(this.formatTypeLabel, "formatTypeLabel");
            this.formatTypeLabel.Name = "formatTypeLabel";
            manager.ApplyResources(this.formatTypeListBox, "formatTypeListBox");
            this.formatTypeListBox.FormattingEnabled = true;
            this.formatTypeListBox.Name = "formatTypeListBox";
            this.formatTypeListBox.SelectedIndexChanged += new EventHandler(this.formatTypeListBox_SelectedIndexChanged);
            manager.ApplyResources(this, "$this");
            base.AutoScaleMode = AutoScaleMode.Font;
            base.Controls.Add(this.formatGroupBox);
            this.MinimumSize = new Size(390, 0xed);
            base.Name = "FormatControl";
            base.Load += new EventHandler(this.FormatControl_Load);
            this.formatGroupBox.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.sampleGroupBox.ResumeLayout(false);
            this.sampleGroupBox.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.decimalPlacesUpDown.EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void nullValueTextBox_TextChanged(object sender, EventArgs e)
        {
            this.dirty = true;
        }

        protected override bool ProcessMnemonic(char charCode)
        {
            if (Control.IsMnemonic(charCode, this.formatTypeLabel.Text))
            {
                this.formatTypeListBox.Focus();
                return true;
            }
            if (Control.IsMnemonic(charCode, this.nullValueLabel.Text))
            {
                this.nullValueTextBox.Focus();
                return true;
            }
            switch (this.formatTypeListBox.SelectedIndex)
            {
                case 0:
                    return false;

                case 1:
                case 2:
                case 4:
                    if (!Control.IsMnemonic(charCode, this.secondRowLabel.Text))
                    {
                        return false;
                    }
                    this.decimalPlacesUpDown.Focus();
                    return true;

                case 3:
                    if (!Control.IsMnemonic(charCode, this.secondRowLabel.Text))
                    {
                        return false;
                    }
                    this.dateTimeFormatsListBox.Focus();
                    return true;

                case 5:
                    if (!Control.IsMnemonic(charCode, this.secondRowLabel.Text))
                    {
                        return false;
                    }
                    this.customStringTextBox.Focus();
                    return true;
            }
            return false;
        }

        public void ResetFormattingInfo()
        {
            this.decimalPlacesUpDown.ValueChanged -= new EventHandler(this.decimalPlacesUpDown_ValueChanged);
            this.customStringTextBox.TextChanged -= new EventHandler(this.customStringTextBox_TextChanged);
            this.dateTimeFormatsListBox.SelectedIndexChanged -= new EventHandler(this.dateTimeFormatsListBox_SelectedIndexChanged);
            this.formatTypeListBox.SelectedIndexChanged -= new EventHandler(this.formatTypeListBox_SelectedIndexChanged);
            this.decimalPlacesUpDown.Value = 2M;
            this.nullValueTextBox.Text = string.Empty;
            this.dateTimeFormatsListBox.SelectedIndex = -1;
            this.formatTypeListBox.SelectedIndex = -1;
            this.customStringTextBox.Text = string.Empty;
            this.decimalPlacesUpDown.ValueChanged += new EventHandler(this.decimalPlacesUpDown_ValueChanged);
            this.customStringTextBox.TextChanged += new EventHandler(this.customStringTextBox_TextChanged);
            this.dateTimeFormatsListBox.SelectedIndexChanged += new EventHandler(this.dateTimeFormatsListBox_SelectedIndexChanged);
            this.formatTypeListBox.SelectedIndexChanged += new EventHandler(this.formatTypeListBox_SelectedIndexChanged);
        }

        private void UpdateControlVisibility(FormatTypeClass formatType)
        {
            if (formatType == null)
            {
                this.explanationLabel.Visible = false;
                this.sampleLabel.Visible = false;
                this.nullValueLabel.Visible = false;
                this.secondRowLabel.Visible = false;
                this.nullValueTextBox.Visible = false;
                this.thirdRowLabel.Visible = false;
                this.dateTimeFormatsListBox.Visible = false;
                this.customStringTextBox.Visible = false;
                this.decimalPlacesUpDown.Visible = false;
            }
            else
            {
                this.tableLayoutPanel1.SuspendLayout();
                this.secondRowLabel.Text = "";
                if (formatType.DropDownVisible)
                {
                    this.secondRowLabel.Text = System.Design.SR.GetString("BindingFormattingDialogDecimalPlaces");
                    this.decimalPlacesUpDown.Visible = true;
                }
                else
                {
                    this.decimalPlacesUpDown.Visible = false;
                }
                if (formatType.FormatStringTextBoxVisible)
                {
                    this.secondRowLabel.Text = System.Design.SR.GetString("BindingFormattingDialogCustomFormat");
                    this.thirdRowLabel.Visible = true;
                    this.tableLayoutPanel1.SetColumn(this.thirdRowLabel, 0);
                    this.tableLayoutPanel1.SetColumnSpan(this.thirdRowLabel, 2);
                    this.customStringTextBox.Visible = true;
                    if (this.tableLayoutPanel1.Controls.Contains(this.dateTimeFormatsListBox))
                    {
                        this.tableLayoutPanel1.Controls.Remove(this.dateTimeFormatsListBox);
                    }
                    this.tableLayoutPanel1.Controls.Add(this.customStringTextBox, 1, 1);
                }
                else
                {
                    this.thirdRowLabel.Visible = false;
                    this.customStringTextBox.Visible = false;
                }
                if (formatType.ListBoxVisible)
                {
                    this.secondRowLabel.Text = System.Design.SR.GetString("BindingFormattingDialogType");
                    if (this.tableLayoutPanel1.Controls.Contains(this.customStringTextBox))
                    {
                        this.tableLayoutPanel1.Controls.Remove(this.customStringTextBox);
                    }
                    this.dateTimeFormatsListBox.Visible = true;
                    this.tableLayoutPanel1.Controls.Add(this.dateTimeFormatsListBox, 0, 2);
                    this.tableLayoutPanel1.SetColumn(this.dateTimeFormatsListBox, 0);
                    this.tableLayoutPanel1.SetColumnSpan(this.dateTimeFormatsListBox, 2);
                }
                else
                {
                    this.dateTimeFormatsListBox.Visible = false;
                }
                this.tableLayoutPanel1.ResumeLayout(true);
            }
        }

        private void UpdateCustomStringTextBox()
        {
            this.customStringTextBox = new TextBox();
            this.customStringTextBox.AccessibleDescription = System.Design.SR.GetString("BindingFormattingDialogCustomFormatAccessibleDescription");
            this.customStringTextBox.Margin = new Padding(0, 3, 0, 3);
            this.customStringTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            this.customStringTextBox.TabIndex = 3;
            this.customStringTextBox.TextChanged += new EventHandler(this.customStringTextBox_TextChanged);
        }

        private void UpdateFormatTypeListBoxHeight()
        {
            this.formatTypeListBox.Height = this.tableLayoutPanel1.Bottom - this.formatTypeListBox.Top;
        }

        private void UpdateFormatTypeListBoxItems()
        {
            this.dateTimeFormatsListBox.SelectedIndexChanged -= new EventHandler(this.dateTimeFormatsListBox_SelectedIndexChanged);
            this.dateTimeFormatsListBox.Items.Clear();
            this.dateTimeFormatsListBox.Items.Add(new DateTimeFormatsListBoxItem(dateTimeFormatValue, "d"));
            this.dateTimeFormatsListBox.Items.Add(new DateTimeFormatsListBoxItem(dateTimeFormatValue, "D"));
            this.dateTimeFormatsListBox.Items.Add(new DateTimeFormatsListBoxItem(dateTimeFormatValue, "f"));
            this.dateTimeFormatsListBox.Items.Add(new DateTimeFormatsListBoxItem(dateTimeFormatValue, "F"));
            this.dateTimeFormatsListBox.Items.Add(new DateTimeFormatsListBoxItem(dateTimeFormatValue, "g"));
            this.dateTimeFormatsListBox.Items.Add(new DateTimeFormatsListBoxItem(dateTimeFormatValue, "G"));
            this.dateTimeFormatsListBox.Items.Add(new DateTimeFormatsListBoxItem(dateTimeFormatValue, "t"));
            this.dateTimeFormatsListBox.Items.Add(new DateTimeFormatsListBoxItem(dateTimeFormatValue, "T"));
            this.dateTimeFormatsListBox.Items.Add(new DateTimeFormatsListBoxItem(dateTimeFormatValue, "M"));
            this.dateTimeFormatsListBox.SelectedIndex = 0;
            this.dateTimeFormatsListBox.SelectedIndexChanged += new EventHandler(this.dateTimeFormatsListBox_SelectedIndexChanged);
        }

        private void UpdateTBLHeight()
        {
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel1.Controls.Add(this.customStringTextBox, 1, 1);
            this.customStringTextBox.Visible = false;
            this.thirdRowLabel.MaximumSize = new Size(this.tableLayoutPanel1.Width, 0);
            this.dateTimeFormatsListBox.Visible = false;
            this.tableLayoutPanel1.SetColumn(this.thirdRowLabel, 0);
            this.tableLayoutPanel1.SetColumnSpan(this.thirdRowLabel, 2);
            this.thirdRowLabel.AutoSize = true;
            this.tableLayoutPanel1.ResumeLayout(true);
            this.tableLayoutPanel1.MinimumSize = new Size(this.tableLayoutPanel1.Width, this.tableLayoutPanel1.Height);
        }

        public bool Dirty
        {
            get
            {
                return this.dirty;
            }
            set
            {
                this.dirty = value;
            }
        }

        public string FormatType
        {
            get
            {
                FormatTypeClass selectedItem = this.formatTypeListBox.SelectedItem as FormatTypeClass;
                if (selectedItem != null)
                {
                    return selectedItem.ToString();
                }
                return string.Empty;
            }
            set
            {
                this.formatTypeListBox.SelectedIndex = 0;
                for (int i = 0; i < this.formatTypeListBox.Items.Count; i++)
                {
                    FormatTypeClass class2 = this.formatTypeListBox.Items[i] as FormatTypeClass;
                    if (class2.ToString().Equals(value))
                    {
                        this.formatTypeListBox.SelectedIndex = i;
                    }
                }
            }
        }

        public FormatTypeClass FormatTypeItem
        {
            get
            {
                return (this.formatTypeListBox.SelectedItem as FormatTypeClass);
            }
        }

        public string NullValue
        {
            get
            {
                string str = this.nullValueTextBox.Text.Trim();
                if (str.Length != 0)
                {
                    return str;
                }
                return null;
            }
            set
            {
                this.nullValueTextBox.TextChanged -= new EventHandler(this.nullValueTextBox_TextChanged);
                this.nullValueTextBox.Text = value;
                this.nullValueTextBox.TextChanged += new EventHandler(this.nullValueTextBox_TextChanged);
            }
        }

        public bool NullValueTextBoxEnabled
        {
            set
            {
                this.nullValueTextBox.Enabled = value;
            }
        }

        private class CurrencyFormatType : FormatControl.FormatTypeClass
        {
            private FormatControl owner;

            public CurrencyFormatType(FormatControl owner)
            {
                this.owner = owner;
            }

            public override bool Parse(string formatString)
            {
                return ParseStatic(formatString);
            }

            public static bool ParseStatic(string formatString)
            {
                if (((!formatString.Equals("C0") && !formatString.Equals("C1")) && (!formatString.Equals("C2") && !formatString.Equals("C3"))) && (!formatString.Equals("C4") && !formatString.Equals("C5")))
                {
                    return formatString.Equals("C6");
                }
                return true;
            }

            public override void PushFormatStringIntoFormatType(string formatString)
            {
                if (formatString.Equals("C0"))
                {
                    this.owner.decimalPlacesUpDown.Value = 0M;
                }
                else if (formatString.Equals("C1"))
                {
                    this.owner.decimalPlacesUpDown.Value = 1M;
                }
                else if (formatString.Equals("C2"))
                {
                    this.owner.decimalPlacesUpDown.Value = 2M;
                }
                else if (formatString.Equals("C3"))
                {
                    this.owner.decimalPlacesUpDown.Value = 3M;
                }
                else if (formatString.Equals("C4"))
                {
                    this.owner.decimalPlacesUpDown.Value = 4M;
                }
                else if (formatString.Equals("C5"))
                {
                    this.owner.decimalPlacesUpDown.Value = 5M;
                }
                else if (formatString.Equals("C6"))
                {
                    this.owner.decimalPlacesUpDown.Value = 6M;
                }
            }

            public override string ToString()
            {
                return System.Design.SR.GetString("BindingFormattingDialogFormatTypeCurrency");
            }

            public override bool DropDownVisible
            {
                get
                {
                    return true;
                }
            }

            public override bool FormatLabelVisible
            {
                get
                {
                    return false;
                }
            }

            public override string FormatString
            {
                get
                {
                    switch (((int) this.owner.decimalPlacesUpDown.Value))
                    {
                        case 0:
                            return "C0";

                        case 1:
                            return "C1";

                        case 2:
                            return "C2";

                        case 3:
                            return "C3";

                        case 4:
                            return "C4";

                        case 5:
                            return "C5";

                        case 6:
                            return "C6";
                    }
                    return "";
                }
            }

            public override bool FormatStringTextBoxVisible
            {
                get
                {
                    return false;
                }
            }

            public override bool ListBoxVisible
            {
                get
                {
                    return false;
                }
            }

            public override string SampleString
            {
                get
                {
                    double num = -1234.5678;
                    return num.ToString(this.FormatString, CultureInfo.CurrentCulture);
                }
            }

            public override string TopLabelString
            {
                get
                {
                    return System.Design.SR.GetString("BindingFormattingDialogFormatTypeCurrencyExplanation");
                }
            }
        }

        private class CustomFormatType : FormatControl.FormatTypeClass
        {
            private FormatControl owner;

            public CustomFormatType(FormatControl owner)
            {
                this.owner = owner;
            }

            public override bool Parse(string formatString)
            {
                return ParseStatic(formatString);
            }

            public static bool ParseStatic(string formatString)
            {
                return true;
            }

            public override void PushFormatStringIntoFormatType(string formatString)
            {
                this.owner.customStringTextBox.Text = formatString;
            }

            public override string ToString()
            {
                return System.Design.SR.GetString("BindingFormattingDialogFormatTypeCustom");
            }

            public override bool DropDownVisible
            {
                get
                {
                    return false;
                }
            }

            public override bool FormatLabelVisible
            {
                get
                {
                    return false;
                }
            }

            public override string FormatString
            {
                get
                {
                    return this.owner.customStringTextBox.Text;
                }
            }

            public override bool FormatStringTextBoxVisible
            {
                get
                {
                    return true;
                }
            }

            public override bool ListBoxVisible
            {
                get
                {
                    return false;
                }
            }

            public override string SampleString
            {
                get
                {
                    string formatString = this.FormatString;
                    if (string.IsNullOrEmpty(formatString))
                    {
                        return "";
                    }
                    string str2 = "";
                    if (FormatControl.DateTimeFormatType.ParseStatic(formatString))
                    {
                        str2 = FormatControl.dateTimeFormatValue.ToString(formatString, CultureInfo.CurrentCulture);
                    }
                    if (str2.Equals(""))
                    {
                        try
                        {
                            str2 = -1234.5678.ToString(formatString, CultureInfo.CurrentCulture);
                        }
                        catch (FormatException)
                        {
                            str2 = "";
                        }
                    }
                    if (str2.Equals(""))
                    {
                        try
                        {
                            str2 = -1234.ToString(formatString, CultureInfo.CurrentCulture);
                        }
                        catch (FormatException)
                        {
                            str2 = "";
                        }
                    }
                    if (str2.Equals(""))
                    {
                        try
                        {
                            str2 = FormatControl.dateTimeFormatValue.ToString(formatString, CultureInfo.CurrentCulture);
                        }
                        catch (FormatException)
                        {
                            str2 = "";
                        }
                    }
                    if (str2.Equals(""))
                    {
                        str2 = System.Design.SR.GetString("BindingFormattingDialogFormatTypeCustomInvalidFormat");
                    }
                    return str2;
                }
            }

            public override string TopLabelString
            {
                get
                {
                    return System.Design.SR.GetString("BindingFormattingDialogFormatTypeCustomExplanation");
                }
            }
        }

        private class DateTimeFormatsListBoxItem
        {
            private string formatString;
            private DateTime value;

            public DateTimeFormatsListBoxItem(DateTime value, string formatString)
            {
                this.value = value;
                this.formatString = formatString;
            }

            public override string ToString()
            {
                return this.value.ToString(this.formatString, CultureInfo.CurrentCulture);
            }

            public string FormatString
            {
                get
                {
                    return this.formatString;
                }
            }
        }

        private class DateTimeFormatType : FormatControl.FormatTypeClass
        {
            private FormatControl owner;

            public DateTimeFormatType(FormatControl owner)
            {
                this.owner = owner;
            }

            public override bool Parse(string formatString)
            {
                return ParseStatic(formatString);
            }

            public static bool ParseStatic(string formatString)
            {
                if (((!formatString.Equals("d") && !formatString.Equals("D")) && (!formatString.Equals("f") && !formatString.Equals("F"))) && ((!formatString.Equals("g") && !formatString.Equals("G")) && (!formatString.Equals("t") && !formatString.Equals("T"))))
                {
                    return formatString.Equals("M");
                }
                return true;
            }

            public override void PushFormatStringIntoFormatType(string formatString)
            {
                int num = -1;
                if (formatString.Equals("d"))
                {
                    num = 0;
                }
                else if (formatString.Equals("D"))
                {
                    num = 1;
                }
                else if (formatString.Equals("f"))
                {
                    num = 2;
                }
                else if (formatString.Equals("F"))
                {
                    num = 3;
                }
                else if (formatString.Equals("g"))
                {
                    num = 4;
                }
                else if (formatString.Equals("G"))
                {
                    num = 5;
                }
                else if (formatString.Equals("t"))
                {
                    num = 6;
                }
                else if (formatString.Equals("T"))
                {
                    num = 7;
                }
                else if (formatString.Equals("M"))
                {
                    num = 8;
                }
                this.owner.dateTimeFormatsListBox.SelectedIndex = num;
            }

            public override string ToString()
            {
                return System.Design.SR.GetString("BindingFormattingDialogFormatTypeDateTime");
            }

            public override bool DropDownVisible
            {
                get
                {
                    return false;
                }
            }

            public override bool FormatLabelVisible
            {
                get
                {
                    return false;
                }
            }

            public override string FormatString
            {
                get
                {
                    FormatControl.DateTimeFormatsListBoxItem selectedItem = this.owner.dateTimeFormatsListBox.SelectedItem as FormatControl.DateTimeFormatsListBoxItem;
                    return selectedItem.FormatString;
                }
            }

            public override bool FormatStringTextBoxVisible
            {
                get
                {
                    return false;
                }
            }

            public override bool ListBoxVisible
            {
                get
                {
                    return true;
                }
            }

            public override string SampleString
            {
                get
                {
                    if (this.owner.dateTimeFormatsListBox.SelectedItem == null)
                    {
                        return "";
                    }
                    return FormatControl.dateTimeFormatValue.ToString(this.FormatString, CultureInfo.CurrentCulture);
                }
            }

            public override string TopLabelString
            {
                get
                {
                    return System.Design.SR.GetString("BindingFormattingDialogFormatTypeDateTimeExplanation");
                }
            }
        }

        internal abstract class FormatTypeClass
        {
            protected FormatTypeClass()
            {
            }

            public abstract bool Parse(string formatString);
            public abstract void PushFormatStringIntoFormatType(string formatString);

            public abstract bool DropDownVisible { get; }

            public abstract bool FormatLabelVisible { get; }

            public abstract string FormatString { get; }

            public abstract bool FormatStringTextBoxVisible { get; }

            public abstract bool ListBoxVisible { get; }

            public abstract string SampleString { get; }

            public abstract string TopLabelString { get; }
        }

        private class NoFormattingFormatType : FormatControl.FormatTypeClass
        {
            public override bool Parse(string formatString)
            {
                return false;
            }

            public override void PushFormatStringIntoFormatType(string formatString)
            {
            }

            public override string ToString()
            {
                return System.Design.SR.GetString("BindingFormattingDialogFormatTypeNoFormatting");
            }

            public override bool DropDownVisible
            {
                get
                {
                    return false;
                }
            }

            public override bool FormatLabelVisible
            {
                get
                {
                    return false;
                }
            }

            public override string FormatString
            {
                get
                {
                    return "";
                }
            }

            public override bool FormatStringTextBoxVisible
            {
                get
                {
                    return false;
                }
            }

            public override bool ListBoxVisible
            {
                get
                {
                    return false;
                }
            }

            public override string SampleString
            {
                get
                {
                    return "-1234.5";
                }
            }

            public override string TopLabelString
            {
                get
                {
                    return System.Design.SR.GetString("BindingFormattingDialogFormatTypeNoFormattingExplanation");
                }
            }
        }

        private class NumericFormatType : FormatControl.FormatTypeClass
        {
            private FormatControl owner;

            public NumericFormatType(FormatControl owner)
            {
                this.owner = owner;
            }

            public override bool Parse(string formatString)
            {
                return ParseStatic(formatString);
            }

            public static bool ParseStatic(string formatString)
            {
                if (((!formatString.Equals("N0") && !formatString.Equals("N1")) && (!formatString.Equals("N2") && !formatString.Equals("N3"))) && (!formatString.Equals("N4") && !formatString.Equals("N5")))
                {
                    return formatString.Equals("N6");
                }
                return true;
            }

            public override void PushFormatStringIntoFormatType(string formatString)
            {
                if (formatString.Equals("N0"))
                {
                    this.owner.decimalPlacesUpDown.Value = 0M;
                }
                else if (formatString.Equals("N1"))
                {
                    this.owner.decimalPlacesUpDown.Value = 1M;
                }
                else if (formatString.Equals("N2"))
                {
                    this.owner.decimalPlacesUpDown.Value = 2M;
                }
                else if (formatString.Equals("N3"))
                {
                    this.owner.decimalPlacesUpDown.Value = 3M;
                }
                else if (formatString.Equals("N4"))
                {
                    this.owner.decimalPlacesUpDown.Value = 4M;
                }
                else if (formatString.Equals("N5"))
                {
                    this.owner.decimalPlacesUpDown.Value = 5M;
                }
                else if (formatString.Equals("N6"))
                {
                    this.owner.decimalPlacesUpDown.Value = 6M;
                }
            }

            public override string ToString()
            {
                return System.Design.SR.GetString("BindingFormattingDialogFormatTypeNumeric");
            }

            public override bool DropDownVisible
            {
                get
                {
                    return true;
                }
            }

            public override bool FormatLabelVisible
            {
                get
                {
                    return false;
                }
            }

            public override string FormatString
            {
                get
                {
                    switch (((int) this.owner.decimalPlacesUpDown.Value))
                    {
                        case 0:
                            return "N0";

                        case 1:
                            return "N1";

                        case 2:
                            return "N2";

                        case 3:
                            return "N3";

                        case 4:
                            return "N4";

                        case 5:
                            return "N5";

                        case 6:
                            return "N6";
                    }
                    return "";
                }
            }

            public override bool FormatStringTextBoxVisible
            {
                get
                {
                    return false;
                }
            }

            public override bool ListBoxVisible
            {
                get
                {
                    return false;
                }
            }

            public override string SampleString
            {
                get
                {
                    double num = -1234.5678;
                    return num.ToString(this.FormatString, CultureInfo.CurrentCulture);
                }
            }

            public override string TopLabelString
            {
                get
                {
                    return System.Design.SR.GetString("BindingFormattingDialogFormatTypeNumericExplanation");
                }
            }
        }

        private class ScientificFormatType : FormatControl.FormatTypeClass
        {
            private FormatControl owner;

            public ScientificFormatType(FormatControl owner)
            {
                this.owner = owner;
            }

            public override bool Parse(string formatString)
            {
                return ParseStatic(formatString);
            }

            public static bool ParseStatic(string formatString)
            {
                if (((!formatString.Equals("E0") && !formatString.Equals("E1")) && (!formatString.Equals("E2") && !formatString.Equals("E3"))) && (!formatString.Equals("E4") && !formatString.Equals("E5")))
                {
                    return formatString.Equals("E6");
                }
                return true;
            }

            public override void PushFormatStringIntoFormatType(string formatString)
            {
                if (formatString.Equals("E0"))
                {
                    this.owner.decimalPlacesUpDown.Value = 0M;
                }
                else if (formatString.Equals("E1"))
                {
                    this.owner.decimalPlacesUpDown.Value = 1M;
                }
                else if (formatString.Equals("E2"))
                {
                    this.owner.decimalPlacesUpDown.Value = 2M;
                }
                else if (formatString.Equals("E3"))
                {
                    this.owner.decimalPlacesUpDown.Value = 3M;
                }
                else if (formatString.Equals("E4"))
                {
                    this.owner.decimalPlacesUpDown.Value = 4M;
                }
                else if (formatString.Equals("E5"))
                {
                    this.owner.decimalPlacesUpDown.Value = 5M;
                }
                else if (formatString.Equals("E6"))
                {
                    this.owner.decimalPlacesUpDown.Value = 6M;
                }
            }

            public override string ToString()
            {
                return System.Design.SR.GetString("BindingFormattingDialogFormatTypeScientific");
            }

            public override bool DropDownVisible
            {
                get
                {
                    return true;
                }
            }

            public override bool FormatLabelVisible
            {
                get
                {
                    return false;
                }
            }

            public override string FormatString
            {
                get
                {
                    switch (((int) this.owner.decimalPlacesUpDown.Value))
                    {
                        case 0:
                            return "E0";

                        case 1:
                            return "E1";

                        case 2:
                            return "E2";

                        case 3:
                            return "E3";

                        case 4:
                            return "E4";

                        case 5:
                            return "E5";

                        case 6:
                            return "E6";
                    }
                    return "";
                }
            }

            public override bool FormatStringTextBoxVisible
            {
                get
                {
                    return false;
                }
            }

            public override bool ListBoxVisible
            {
                get
                {
                    return false;
                }
            }

            public override string SampleString
            {
                get
                {
                    double num = -1234.5678;
                    return num.ToString(this.FormatString, CultureInfo.CurrentCulture);
                }
            }

            public override string TopLabelString
            {
                get
                {
                    return System.Design.SR.GetString("BindingFormattingDialogFormatTypeScientificExplanation");
                }
            }
        }
    }
}

