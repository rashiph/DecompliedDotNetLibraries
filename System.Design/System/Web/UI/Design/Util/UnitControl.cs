namespace System.Web.UI.Design.Util
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class UnitControl : Panel
    {
        private bool allowNonUnit;
        private bool allowPercent = true;
        private const int COMBO_X_SIZE = 40;
        private const int CTL_Y_SIZE = 0x15;
        private int defaultUnit = 1;
        private const int EDIT_X_SIZE = 0x2c;
        private bool initMode = true;
        private bool internalChange;
        private int maxValue = 0xffff;
        private int minValue;
        private const int SEPARATOR_X_SIZE = 4;
        public const int UNIT_CM = 4;
        public const int UNIT_EM = 6;
        public const int UNIT_EX = 7;
        public const int UNIT_IN = 5;
        public const int UNIT_MM = 3;
        public const int UNIT_NONE = 9;
        public const int UNIT_PC = 2;
        public const int UNIT_PERCENT = 8;
        public const int UNIT_PT = 1;
        public const int UNIT_PX = 0;
        private static readonly string[] UNIT_VALUES = new string[] { "px", "pt", "pc", "mm", "cm", "in", "em", "ex", "%", "" };
        private ComboBox unitCombo;
        private bool validateMinMax;
        private bool valueChanged;
        private NumberEdit valueEdit;

        public event EventHandler Changed;

        public UnitControl()
        {
            base.Size = new Size(0x58, 0x15);
            this.InitControl();
            this.InitUI();
            this.initMode = false;
        }

        private string GetValidatedValue()
        {
            string str = null;
            if (this.validateMinMax)
            {
                string text = this.valueEdit.Text;
                if (text.Length == 0)
                {
                    return str;
                }
                try
                {
                    if (!text.Contains(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator))
                    {
                        int num = int.Parse(text, CultureInfo.CurrentCulture);
                        if (num < this.minValue)
                        {
                            return this.minValue.ToString(NumberFormatInfo.CurrentInfo);
                        }
                        if (num > this.maxValue)
                        {
                            str = this.maxValue.ToString(NumberFormatInfo.CurrentInfo);
                        }
                        return str;
                    }
                    float num2 = float.Parse(text, CultureInfo.CurrentCulture);
                    if (num2 < this.minValue)
                    {
                        return this.minValue.ToString(NumberFormatInfo.CurrentInfo);
                    }
                    if (num2 > this.maxValue)
                    {
                        str = this.maxValue.ToString(NumberFormatInfo.CurrentInfo);
                    }
                }
                catch
                {
                    str = this.maxValue.ToString(NumberFormatInfo.CurrentInfo);
                }
            }
            return str;
        }

        private void InitControl()
        {
            int width = base.Width - 0x2c;
            if (width < 0)
            {
                width = 0;
            }
            this.valueEdit = new NumberEdit();
            this.valueEdit.Location = new Point(0, 0);
            this.valueEdit.Size = new Size(width, 0x15);
            this.valueEdit.TabIndex = 0;
            this.valueEdit.MaxLength = 10;
            this.valueEdit.TextChanged += new EventHandler(this.OnValueTextChanged);
            this.valueEdit.LostFocus += new EventHandler(this.OnValueLostFocus);
            this.unitCombo = new ComboBox();
            this.unitCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.unitCombo.Location = new Point(width + 4, 0);
            this.unitCombo.Size = new Size(40, 0x15);
            this.unitCombo.TabIndex = 1;
            this.unitCombo.MaxDropDownItems = 9;
            this.unitCombo.SelectedIndexChanged += new EventHandler(this.OnUnitSelectedIndexChanged);
            base.Controls.Clear();
            base.Controls.AddRange(new Control[] { this.unitCombo, this.valueEdit });
            for (int i = 0; i <= 7; i++)
            {
                this.unitCombo.Items.Add(UNIT_VALUES[i]);
            }
            if (this.allowPercent)
            {
                this.unitCombo.Items.Add(UNIT_VALUES[8]);
            }
            if (this.allowNonUnit)
            {
                this.unitCombo.Items.Add(UNIT_VALUES[9]);
            }
        }

        private void InitUI()
        {
            this.valueEdit.Text = string.Empty;
            this.unitCombo.SelectedIndex = -1;
        }

        private void OnChanged(EventArgs e)
        {
            if (this.onChangedHandler != null)
            {
                this.onChangedHandler(this, e);
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            this.valueEdit.Enabled = base.Enabled;
            this.unitCombo.Enabled = base.Enabled;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            this.valueEdit.Focus();
        }

        private void OnUnitSelectedIndexChanged(object source, EventArgs e)
        {
            if (!this.initMode && !this.internalChange)
            {
                this.OnChanged(EventArgs.Empty);
            }
        }

        private void OnValueLostFocus(object source, EventArgs e)
        {
            if (this.valueChanged)
            {
                string validatedValue = this.GetValidatedValue();
                if (validatedValue != null)
                {
                    this.valueEdit.Text = validatedValue;
                    this.OnValueTextChanged(this.valueEdit, EventArgs.Empty);
                }
                this.valueChanged = false;
                this.OnChanged(EventArgs.Empty);
            }
        }

        private void OnValueTextChanged(object source, EventArgs e)
        {
            if (!this.initMode)
            {
                if (this.valueEdit.Text.Length == 0)
                {
                    this.internalChange = true;
                    this.unitCombo.SelectedIndex = -1;
                    this.internalChange = false;
                }
                else if (this.unitCombo.SelectedIndex == -1)
                {
                    this.internalChange = true;
                    this.unitCombo.SelectedIndex = this.defaultUnit;
                    this.internalChange = false;
                }
                this.valueChanged = true;
                this.OnChanged(null);
            }
        }

        public bool AllowNegativeValues
        {
            get
            {
                return this.valueEdit.AllowNegative;
            }
            set
            {
                this.valueEdit.AllowNegative = value;
            }
        }

        public bool AllowNonUnitValues
        {
            get
            {
                return this.allowNonUnit;
            }
            set
            {
                if (value != this.allowNonUnit)
                {
                    if (value && !this.allowPercent)
                    {
                        throw new Exception();
                    }
                    this.allowNonUnit = value;
                    if (this.allowNonUnit)
                    {
                        this.unitCombo.Items.Add(UNIT_VALUES[9]);
                    }
                    else
                    {
                        this.unitCombo.Items.Remove(UNIT_VALUES[9]);
                    }
                }
            }
        }

        public bool AllowPercentValues
        {
            get
            {
                return this.allowPercent;
            }
            set
            {
                if (value != this.allowPercent)
                {
                    if (!value && this.allowNonUnit)
                    {
                        throw new Exception();
                    }
                    this.allowPercent = value;
                    if (this.allowPercent)
                    {
                        this.unitCombo.Items.Add(UNIT_VALUES[8]);
                    }
                    else
                    {
                        this.unitCombo.Items.Remove(UNIT_VALUES[8]);
                    }
                }
            }
        }

        public int DefaultUnit
        {
            get
            {
                return this.defaultUnit;
            }
            set
            {
                this.defaultUnit = value;
            }
        }

        public int MaxValue
        {
            get
            {
                return this.maxValue;
            }
            set
            {
                this.maxValue = value;
            }
        }

        public int MinValue
        {
            get
            {
                return this.minValue;
            }
            set
            {
                this.minValue = value;
            }
        }

        public string UnitAccessibleDescription
        {
            get
            {
                if (this.unitCombo != null)
                {
                    return this.unitCombo.AccessibleDescription;
                }
                return string.Empty;
            }
            set
            {
                if (this.unitCombo != null)
                {
                    this.unitCombo.AccessibleDescription = value;
                }
            }
        }

        public string UnitAccessibleName
        {
            get
            {
                if (this.unitCombo != null)
                {
                    return this.unitCombo.AccessibleName;
                }
                return string.Empty;
            }
            set
            {
                if (this.unitCombo != null)
                {
                    this.unitCombo.AccessibleName = value;
                }
            }
        }

        public bool ValidateMinMax
        {
            get
            {
                return this.validateMinMax;
            }
            set
            {
                this.validateMinMax = value;
            }
        }

        public string Value
        {
            get
            {
                string validatedValue = this.GetValidatedValue();
                if (validatedValue == null)
                {
                    validatedValue = this.valueEdit.Text;
                }
                else
                {
                    this.valueEdit.Text = validatedValue;
                    this.OnValueTextChanged(this.valueEdit, EventArgs.Empty);
                }
                int selectedIndex = this.unitCombo.SelectedIndex;
                if ((validatedValue.Length != 0) && (selectedIndex != -1))
                {
                    return (validatedValue + UNIT_VALUES[selectedIndex]);
                }
                return null;
            }
            set
            {
                this.initMode = true;
                this.InitUI();
                if (value != null)
                {
                    string str = value.Trim().ToLower(CultureInfo.InvariantCulture);
                    int length = str.Length;
                    int num2 = -1;
                    int num3 = -1;
                    for (int i = 0; i < length; i++)
                    {
                        char ch = str[i];
                        if (((ch < '0') || (ch > '9')) && (!NumberFormatInfo.CurrentInfo.NumberDecimalSeparator.Contains(ch.ToString(CultureInfo.CurrentCulture)) && (!NumberFormatInfo.CurrentInfo.NegativeSign.Contains(ch.ToString(CultureInfo.CurrentCulture)) || !this.valueEdit.AllowNegative)))
                        {
                            break;
                        }
                        num3 = i;
                    }
                    if (num3 != -1)
                    {
                        if ((num3 + 1) < length)
                        {
                            int num5 = this.allowPercent ? 8 : 7;
                            string str2 = str.Substring(num3 + 1);
                            for (int j = 0; j <= num5; j++)
                            {
                                if (UNIT_VALUES[j].Equals(str2))
                                {
                                    num2 = j;
                                    break;
                                }
                            }
                        }
                        else if (this.allowNonUnit)
                        {
                            num2 = 9;
                        }
                        if (num2 != -1)
                        {
                            this.valueEdit.Text = str.Substring(0, num3 + 1);
                            this.unitCombo.SelectedIndex = num2;
                        }
                    }
                }
                this.initMode = false;
            }
        }

        public string ValueAccessibleDescription
        {
            get
            {
                if (this.valueEdit != null)
                {
                    return this.valueEdit.AccessibleDescription;
                }
                return string.Empty;
            }
            set
            {
                if (this.valueEdit != null)
                {
                    this.valueEdit.AccessibleDescription = value;
                }
            }
        }

        public string ValueAccessibleName
        {
            get
            {
                if (this.valueEdit != null)
                {
                    return this.valueEdit.AccessibleName;
                }
                return string.Empty;
            }
            set
            {
                if (this.valueEdit != null)
                {
                    this.valueEdit.AccessibleName = value;
                }
            }
        }
    }
}

