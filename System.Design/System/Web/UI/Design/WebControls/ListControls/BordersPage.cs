namespace System.Web.UI.Design.WebControls.ListControls
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class BordersPage : BaseDataListPage
    {
        private ColorComboBox borderColorCombo;
        private System.Windows.Forms.Button borderColorPickerButton;
        private UnitControl borderWidthUnit;
        private NumberEdit cellPaddingEdit;
        private NumberEdit cellSpacingEdit;
        private ComboBox gridLinesCombo;
        private const int IDX_GRID_BOTH = 2;
        private const int IDX_GRID_HORIZONTAL = 0;
        private const int IDX_GRID_NEITHER = 3;
        private const int IDX_GRID_VERTICAL = 1;

        private void InitForm()
        {
            GroupLabel label = new GroupLabel();
            System.Windows.Forms.Label label2 = new System.Windows.Forms.Label();
            this.cellPaddingEdit = new NumberEdit();
            System.Windows.Forms.Label label3 = new System.Windows.Forms.Label();
            this.cellSpacingEdit = new NumberEdit();
            GroupLabel label4 = new GroupLabel();
            System.Windows.Forms.Label label5 = new System.Windows.Forms.Label();
            this.gridLinesCombo = new ComboBox();
            System.Windows.Forms.Label label6 = new System.Windows.Forms.Label();
            this.borderColorCombo = new ColorComboBox();
            this.borderColorPickerButton = new System.Windows.Forms.Button();
            System.Windows.Forms.Label label7 = new System.Windows.Forms.Label();
            this.borderWidthUnit = new UnitControl();
            label.SetBounds(4, 4, 300, 0x10);
            label.Text = System.Design.SR.GetString("BDLBor_CellMarginsGroup");
            label.TabStop = false;
            label.TabIndex = 0;
            label2.Text = System.Design.SR.GetString("BDLBor_CellPadding");
            label2.SetBounds(12, 0x18, 120, 14);
            label2.TabStop = false;
            label2.TabIndex = 1;
            this.cellPaddingEdit.SetBounds(12, 40, 70, 20);
            this.cellPaddingEdit.AllowDecimal = false;
            this.cellPaddingEdit.AllowNegative = false;
            this.cellPaddingEdit.TabIndex = 2;
            this.cellPaddingEdit.TextChanged += new EventHandler(this.OnBordersChanged);
            label3.Text = System.Design.SR.GetString("BDLBor_CellSpacing");
            label3.SetBounds(160, 0x18, 120, 14);
            label3.TabStop = false;
            label3.TabIndex = 3;
            this.cellSpacingEdit.SetBounds(160, 40, 70, 20);
            this.cellSpacingEdit.AllowDecimal = false;
            this.cellSpacingEdit.AllowNegative = false;
            this.cellSpacingEdit.TabIndex = 4;
            this.cellSpacingEdit.TextChanged += new EventHandler(this.OnBordersChanged);
            label4.SetBounds(4, 70, 300, 0x10);
            label4.Text = System.Design.SR.GetString("BDLBor_BorderLinesGroup");
            label4.TabStop = false;
            label4.TabIndex = 5;
            label5.Text = System.Design.SR.GetString("BDLBor_GridLines");
            label5.SetBounds(12, 90, 150, 14);
            label5.TabStop = false;
            label5.TabIndex = 6;
            this.gridLinesCombo.SetBounds(12, 0x6a, 140, 0x15);
            this.gridLinesCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.gridLinesCombo.Items.Clear();
            this.gridLinesCombo.Items.AddRange(new object[] { System.Design.SR.GetString("BDLBor_GL_Horz"), System.Design.SR.GetString("BDLBor_GL_Vert"), System.Design.SR.GetString("BDLBor_GL_Both"), System.Design.SR.GetString("BDLBor_GL_None") });
            this.gridLinesCombo.TabIndex = 7;
            this.gridLinesCombo.SelectedIndexChanged += new EventHandler(this.OnBordersChanged);
            label6.Text = System.Design.SR.GetString("BDLBor_BorderColor");
            label6.SetBounds(12, 0x86, 150, 14);
            label6.TabStop = false;
            label6.TabIndex = 8;
            this.borderColorCombo.SetBounds(12, 150, 140, 0x15);
            this.borderColorCombo.TabIndex = 9;
            this.borderColorCombo.TextChanged += new EventHandler(this.OnBordersChanged);
            this.borderColorCombo.SelectedIndexChanged += new EventHandler(this.OnBordersChanged);
            this.borderColorPickerButton.SetBounds(0x9c, 0x95, 0x18, 0x16);
            this.borderColorPickerButton.Text = "...";
            this.borderColorPickerButton.TabIndex = 10;
            this.borderColorPickerButton.FlatStyle = FlatStyle.System;
            this.borderColorPickerButton.Click += new EventHandler(this.OnClickColorPicker);
            this.borderColorPickerButton.AccessibleName = System.Design.SR.GetString("BDLBor_ChooseColorButton");
            this.borderColorPickerButton.AccessibleDescription = System.Design.SR.GetString("BDLBor_ChooseColorDesc");
            label7.Text = System.Design.SR.GetString("BDLBor_BorderWidth");
            label7.SetBounds(12, 0xb2, 150, 14);
            label7.TabStop = false;
            label7.TabIndex = 11;
            this.borderWidthUnit.SetBounds(12, 0xc2, 0x66, 0x16);
            this.borderWidthUnit.AllowNegativeValues = false;
            this.borderWidthUnit.AllowPercentValues = false;
            this.borderWidthUnit.DefaultUnit = 0;
            this.borderWidthUnit.TabIndex = 12;
            this.borderWidthUnit.Changed += new EventHandler(this.OnBordersChanged);
            this.borderWidthUnit.ValueAccessibleDescription = System.Design.SR.GetString("BDLBor_BorderWidthValueDesc");
            this.borderWidthUnit.ValueAccessibleName = System.Design.SR.GetString("BDLBor_BorderWidthValueName");
            this.borderWidthUnit.UnitAccessibleDescription = System.Design.SR.GetString("BDLBor_BorderWidthUnitDesc");
            this.borderWidthUnit.UnitAccessibleName = System.Design.SR.GetString("BDLBor_BorderWidthUnitName");
            this.Text = System.Design.SR.GetString("BDLBor_Text");
            base.AccessibleDescription = System.Design.SR.GetString("BDLBor_Desc");
            base.Size = new Size(0x134, 0x9c);
            base.CommitOnDeactivate = true;
            base.Icon = new Icon(base.GetType(), "BordersPage.ico");
            base.Controls.Clear();
            base.Controls.AddRange(new Control[] { this.borderWidthUnit, label7, this.borderColorPickerButton, this.borderColorCombo, label6, this.gridLinesCombo, label5, label4, this.cellSpacingEdit, label3, this.cellPaddingEdit, label2, label });
        }

        private void InitPage()
        {
            this.cellPaddingEdit.Clear();
            this.cellSpacingEdit.Clear();
            this.gridLinesCombo.SelectedIndex = -1;
            this.borderColorCombo.Color = null;
            this.borderWidthUnit.Value = null;
        }

        protected override void LoadComponent()
        {
            this.InitPage();
            BaseDataList baseControl = base.GetBaseControl();
            int cellPadding = baseControl.CellPadding;
            if (cellPadding != -1)
            {
                this.cellPaddingEdit.Text = cellPadding.ToString(NumberFormatInfo.CurrentInfo);
            }
            int cellSpacing = baseControl.CellSpacing;
            if (cellSpacing != -1)
            {
                this.cellSpacingEdit.Text = cellSpacing.ToString(NumberFormatInfo.CurrentInfo);
            }
            switch (baseControl.GridLines)
            {
                case GridLines.None:
                    this.gridLinesCombo.SelectedIndex = 3;
                    break;

                case GridLines.Horizontal:
                    this.gridLinesCombo.SelectedIndex = 0;
                    break;

                case GridLines.Vertical:
                    this.gridLinesCombo.SelectedIndex = 1;
                    break;

                case GridLines.Both:
                    this.gridLinesCombo.SelectedIndex = 2;
                    break;
            }
            this.borderColorCombo.Color = ColorTranslator.ToHtml(baseControl.BorderColor);
            this.borderWidthUnit.Value = baseControl.BorderWidth.ToString(CultureInfo.CurrentCulture);
        }

        private void OnBordersChanged(object source, EventArgs e)
        {
            if (!base.IsLoading())
            {
                this.SetDirty();
            }
        }

        private void OnClickColorPicker(object source, EventArgs e)
        {
            string color = this.borderColorCombo.Color;
            color = ColorBuilder.BuildColor(base.GetBaseControl(), this, color);
            if (color != null)
            {
                this.borderColorCombo.Color = color;
                this.OnBordersChanged(this.borderColorCombo, EventArgs.Empty);
            }
        }

        protected override void SaveComponent()
        {
            BaseDataList baseControl = base.GetBaseControl();
            try
            {
                string s = this.cellPaddingEdit.Text.Trim();
                if (s.Length != 0)
                {
                    baseControl.CellPadding = int.Parse(s, CultureInfo.CurrentCulture);
                }
                else
                {
                    baseControl.CellPadding = -1;
                }
            }
            catch
            {
                if (baseControl.CellPadding != -1)
                {
                    this.cellPaddingEdit.Text = baseControl.CellPadding.ToString(NumberFormatInfo.CurrentInfo);
                }
                else
                {
                    this.cellPaddingEdit.Clear();
                }
            }
            try
            {
                string str2 = this.cellSpacingEdit.Text.Trim();
                if (str2.Length != 0)
                {
                    baseControl.CellSpacing = int.Parse(str2, CultureInfo.CurrentCulture);
                }
                else
                {
                    baseControl.CellSpacing = -1;
                }
            }
            catch
            {
                if (baseControl.CellSpacing != -1)
                {
                    this.cellSpacingEdit.Text = baseControl.CellSpacing.ToString(NumberFormatInfo.CurrentInfo);
                }
                else
                {
                    this.cellSpacingEdit.Clear();
                }
            }
            switch (this.gridLinesCombo.SelectedIndex)
            {
                case 0:
                    baseControl.GridLines = GridLines.Horizontal;
                    break;

                case 1:
                    baseControl.GridLines = GridLines.Vertical;
                    break;

                case 2:
                    baseControl.GridLines = GridLines.Both;
                    break;

                case 3:
                    baseControl.GridLines = GridLines.None;
                    break;
            }
            try
            {
                string color = this.borderColorCombo.Color;
                baseControl.BorderColor = ColorTranslator.FromHtml(color);
            }
            catch
            {
                this.borderColorCombo.Color = ColorTranslator.ToHtml(baseControl.BorderColor);
            }
            try
            {
                string str4 = this.borderWidthUnit.Value;
                Unit empty = Unit.Empty;
                if (str4 != null)
                {
                    empty = Unit.Parse(str4, CultureInfo.CurrentCulture);
                }
                baseControl.BorderWidth = empty;
            }
            catch
            {
                this.borderWidthUnit.Value = baseControl.BorderWidth.ToString(CultureInfo.CurrentCulture);
            }
        }

        public override void SetComponent(IComponent component)
        {
            base.SetComponent(component);
            this.InitForm();
        }

        protected override string HelpKeyword
        {
            get
            {
                if (base.IsDataGridMode)
                {
                    return "net.Asp.DataGridProperties.Borders";
                }
                return "net.Asp.DataListProperties.Borders";
            }
        }
    }
}

