namespace System.Web.UI.Design.WebControls.ListControls
{
    using System;
    using System.Collections;
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
    internal sealed class FormatPage : BaseDataListPage
    {
        private System.Windows.Forms.CheckBox allowWrappingCheck;
        private ColorComboBox backColorCombo;
        private System.Windows.Forms.Button backColorPickerButton;
        private System.Windows.Forms.CheckBox boldCheck;
        private const int COL_ROW_TYPE_COUNT = 3;
        private System.Windows.Forms.Panel columnPanel;
        private FormatTreeNode currentFormatNode;
        private FormatObject currentFormatObject;
        private bool fontNameChanged;
        private ComboBox fontNameCombo;
        private UnsettableComboBox fontSizeCombo;
        private UnitControl fontSizeUnit;
        private ColorComboBox foreColorCombo;
        private System.Windows.Forms.Button foreColorPickerButton;
        private ArrayList formatNodes;
        private System.Windows.Forms.TreeView formatTree;
        private UnsettableComboBox horzAlignCombo;
        private const int IDX_ENTIRE = 0;
        private const int IDX_FOOTER = 1;
        private const int IDX_FSIZE_CUSTOM = 10;
        private const int IDX_FSIZE_LARGE = 7;
        private const int IDX_FSIZE_LARGER = 2;
        private const int IDX_FSIZE_MEDIUM = 6;
        private const int IDX_FSIZE_SMALL = 5;
        private const int IDX_FSIZE_SMALLER = 1;
        private const int IDX_FSIZE_XLARGE = 8;
        private const int IDX_FSIZE_XSMALL = 4;
        private const int IDX_FSIZE_XXLARGE = 9;
        private const int IDX_FSIZE_XXSMALL = 3;
        private const int IDX_HALIGN_CENTER = 2;
        private const int IDX_HALIGN_JUSTIFY = 4;
        private const int IDX_HALIGN_LEFT = 1;
        private const int IDX_HALIGN_NOTSET = 0;
        private const int IDX_HALIGN_RIGHT = 3;
        private const int IDX_HEADER = 0;
        private const int IDX_ITEM_ALT = 3;
        private const int IDX_ITEM_EDIT = 5;
        private const int IDX_ITEM_NORMAL = 2;
        private const int IDX_ITEM_SELECTED = 4;
        private const int IDX_ITEM_SEPARATOR = 6;
        private const int IDX_PAGER = 1;
        private const int IDX_ROW_ALT = 3;
        private const int IDX_ROW_EDIT = 5;
        private const int IDX_ROW_NORMAL = 2;
        private const int IDX_ROW_SELECTED = 4;
        private const int IDX_VALIGN_BOTTOM = 3;
        private const int IDX_VALIGN_MIDDLE = 2;
        private const int IDX_VALIGN_NOTSET = 0;
        private const int IDX_VALIGN_TOP = 1;
        private System.Windows.Forms.CheckBox italicCheck;
        private const int ITEM_TYPE_COUNT = 7;
        private System.Windows.Forms.CheckBox overlineCheck;
        private bool propChangesPending;
        private const int ROW_TYPE_COUNT = 6;
        private System.Windows.Forms.CheckBox strikeOutCheck;
        private System.Windows.Forms.Panel stylePanel;
        private System.Windows.Forms.CheckBox underlineCheck;
        private UnsettableComboBox vertAlignCombo;
        private System.Windows.Forms.Label vertAlignLabel;
        private UnitControl widthUnit;

        private void InitFontList()
        {
            try
            {
                FontFamily[] families = FontFamily.Families;
                for (int i = 0; i < families.Length; i++)
                {
                    if ((this.fontNameCombo.Items.Count == 0) || (this.fontNameCombo.FindStringExact(families[i].Name) == -1))
                    {
                        this.fontNameCombo.Items.Add(families[i].Name);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void InitForm()
        {
            System.Windows.Forms.Label label = new System.Windows.Forms.Label();
            this.formatTree = new System.Windows.Forms.TreeView();
            this.stylePanel = new System.Windows.Forms.Panel();
            GroupLabel label2 = new GroupLabel();
            System.Windows.Forms.Label label3 = new System.Windows.Forms.Label();
            this.foreColorCombo = new ColorComboBox();
            this.foreColorPickerButton = new System.Windows.Forms.Button();
            System.Windows.Forms.Label label4 = new System.Windows.Forms.Label();
            this.backColorCombo = new ColorComboBox();
            this.backColorPickerButton = new System.Windows.Forms.Button();
            System.Windows.Forms.Label label5 = new System.Windows.Forms.Label();
            this.fontNameCombo = new ComboBox();
            System.Windows.Forms.Label label6 = new System.Windows.Forms.Label();
            this.fontSizeCombo = new UnsettableComboBox();
            this.fontSizeUnit = new UnitControl();
            this.boldCheck = new System.Windows.Forms.CheckBox();
            this.italicCheck = new System.Windows.Forms.CheckBox();
            this.underlineCheck = new System.Windows.Forms.CheckBox();
            this.strikeOutCheck = new System.Windows.Forms.CheckBox();
            this.overlineCheck = new System.Windows.Forms.CheckBox();
            GroupLabel label7 = new GroupLabel();
            System.Windows.Forms.Label label8 = new System.Windows.Forms.Label();
            this.horzAlignCombo = new UnsettableComboBox();
            this.vertAlignLabel = new System.Windows.Forms.Label();
            this.vertAlignCombo = new UnsettableComboBox();
            this.allowWrappingCheck = new System.Windows.Forms.CheckBox();
            GroupLabel label9 = null;
            System.Windows.Forms.Label label10 = null;
            if (base.IsDataGridMode)
            {
                this.columnPanel = new System.Windows.Forms.Panel();
                label9 = new GroupLabel();
                label10 = new System.Windows.Forms.Label();
                this.widthUnit = new UnitControl();
            }
            label.SetBounds(4, 4, 0x6f, 14);
            label.Text = System.Design.SR.GetString("BDLFmt_Objects");
            label.TabStop = false;
            label.TabIndex = 2;
            this.formatTree.SetBounds(4, 20, 0xa2, 350);
            this.formatTree.HideSelection = false;
            this.formatTree.TabIndex = 3;
            this.formatTree.AfterSelect += new TreeViewEventHandler(this.OnSelChangedFormatObject);
            this.stylePanel.SetBounds(0xb1, 4, 230, 370);
            this.stylePanel.TabIndex = 6;
            this.stylePanel.Visible = false;
            label2.SetBounds(0, 2, 0xe0, 14);
            label2.Text = System.Design.SR.GetString("BDLFmt_AppearanceGroup");
            label2.TabStop = false;
            label2.TabIndex = 1;
            label3.SetBounds(8, 0x13, 160, 14);
            label3.Text = System.Design.SR.GetString("BDLFmt_ForeColor");
            label3.TabStop = false;
            label3.TabIndex = 2;
            this.foreColorCombo.SetBounds(8, 0x25, 0x66, 0x16);
            this.foreColorCombo.TabIndex = 3;
            this.foreColorCombo.TextChanged += new EventHandler(this.OnFormatChanged);
            this.foreColorCombo.SelectedIndexChanged += new EventHandler(this.OnFormatChanged);
            this.foreColorPickerButton.SetBounds(0x72, 0x24, 0x18, 0x16);
            this.foreColorPickerButton.TabIndex = 4;
            this.foreColorPickerButton.Text = "...";
            this.foreColorPickerButton.FlatStyle = FlatStyle.System;
            this.foreColorPickerButton.Click += new EventHandler(this.OnClickForeColorPicker);
            this.foreColorPickerButton.AccessibleName = System.Design.SR.GetString("BDLFmt_ChooseColorButton");
            this.foreColorPickerButton.AccessibleDescription = System.Design.SR.GetString("BDLFmt_ChooseForeColorDesc");
            label4.SetBounds(8, 0x3e, 160, 14);
            label4.Text = System.Design.SR.GetString("BDLFmt_BackColor");
            label4.TabStop = false;
            label4.TabIndex = 5;
            this.backColorCombo.SetBounds(8, 0x4e, 0x66, 0x16);
            this.backColorCombo.TabIndex = 6;
            this.backColorCombo.TextChanged += new EventHandler(this.OnFormatChanged);
            this.backColorCombo.SelectedIndexChanged += new EventHandler(this.OnFormatChanged);
            this.backColorPickerButton.SetBounds(0x72, 0x4d, 0x18, 0x16);
            this.backColorPickerButton.TabIndex = 7;
            this.backColorPickerButton.Text = "...";
            this.backColorPickerButton.FlatStyle = FlatStyle.System;
            this.backColorPickerButton.Click += new EventHandler(this.OnClickBackColorPicker);
            this.backColorPickerButton.AccessibleName = System.Design.SR.GetString("BDLFmt_ChooseColorButton");
            this.backColorPickerButton.AccessibleDescription = System.Design.SR.GetString("BDLFmt_ChooseBackColorDesc");
            label5.SetBounds(8, 0x68, 160, 14);
            label5.Text = System.Design.SR.GetString("BDLFmt_FontName");
            label5.TabStop = false;
            label5.TabIndex = 8;
            this.fontNameCombo.SetBounds(8, 120, 200, 0x16);
            this.fontNameCombo.Sorted = true;
            this.fontNameCombo.TabIndex = 9;
            this.fontNameCombo.SelectedIndexChanged += new EventHandler(this.OnFontNameChanged);
            this.fontNameCombo.TextChanged += new EventHandler(this.OnFontNameChanged);
            label6.SetBounds(8, 0x92, 160, 14);
            label6.Text = System.Design.SR.GetString("BDLFmt_FontSize");
            label6.TabStop = false;
            label6.TabIndex = 10;
            this.fontSizeCombo.SetBounds(8, 0xa2, 100, 0x16);
            this.fontSizeCombo.TabIndex = 11;
            this.fontSizeCombo.MaxDropDownItems = 11;
            this.fontSizeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.fontSizeCombo.Items.AddRange(new object[] { System.Design.SR.GetString("BDLFmt_FS_Smaller"), System.Design.SR.GetString("BDLFmt_FS_Larger"), System.Design.SR.GetString("BDLFmt_FS_XXSmall"), System.Design.SR.GetString("BDLFmt_FS_XSmall"), System.Design.SR.GetString("BDLFmt_FS_Small"), System.Design.SR.GetString("BDLFmt_FS_Medium"), System.Design.SR.GetString("BDLFmt_FS_Large"), System.Design.SR.GetString("BDLFmt_FS_XLarge"), System.Design.SR.GetString("BDLFmt_FS_XXLarge"), System.Design.SR.GetString("BDLFmt_FS_Custom") });
            this.fontSizeCombo.SelectedIndexChanged += new EventHandler(this.OnFontSizeChanged);
            this.fontSizeUnit.SetBounds(0x70, 0xa2, 0x60, 0x16);
            this.fontSizeUnit.AllowNegativeValues = false;
            this.fontSizeUnit.TabIndex = 12;
            this.fontSizeUnit.Changed += new EventHandler(this.OnFormatChanged);
            this.fontSizeUnit.ValueAccessibleDescription = System.Design.SR.GetString("BDLFmt_FontSizeValueDesc");
            this.fontSizeUnit.ValueAccessibleName = System.Design.SR.GetString("BDLFmt_FontSizeValueName");
            this.fontSizeUnit.UnitAccessibleDescription = System.Design.SR.GetString("BDLFmt_FontSizeUnitDesc");
            this.fontSizeUnit.UnitAccessibleName = System.Design.SR.GetString("BDLFmt_FontSizeUnitName");
            this.boldCheck.SetBounds(8, 0xba, 0x6a, 20);
            this.boldCheck.Text = System.Design.SR.GetString("BDLFmt_FontBold");
            this.boldCheck.TabIndex = 13;
            this.boldCheck.TextAlign = ContentAlignment.MiddleLeft;
            this.boldCheck.FlatStyle = FlatStyle.System;
            this.boldCheck.CheckedChanged += new EventHandler(this.OnFormatChanged);
            this.italicCheck.SetBounds(8, 0xcc, 0x6a, 20);
            this.italicCheck.Text = System.Design.SR.GetString("BDLFmt_FontItalic");
            this.italicCheck.TabIndex = 14;
            this.italicCheck.TextAlign = ContentAlignment.MiddleLeft;
            this.italicCheck.FlatStyle = FlatStyle.System;
            this.italicCheck.CheckedChanged += new EventHandler(this.OnFormatChanged);
            this.underlineCheck.SetBounds(8, 0xde, 0x6a, 20);
            this.underlineCheck.Text = System.Design.SR.GetString("BDLFmt_FontUnderline");
            this.underlineCheck.TabIndex = 15;
            this.underlineCheck.TextAlign = ContentAlignment.MiddleLeft;
            this.underlineCheck.FlatStyle = FlatStyle.System;
            this.underlineCheck.CheckedChanged += new EventHandler(this.OnFormatChanged);
            this.strikeOutCheck.SetBounds(120, 0xba, 0x6a, 20);
            this.strikeOutCheck.Text = System.Design.SR.GetString("BDLFmt_FontStrikeout");
            this.strikeOutCheck.TabIndex = 0x10;
            this.strikeOutCheck.TextAlign = ContentAlignment.MiddleLeft;
            this.strikeOutCheck.FlatStyle = FlatStyle.System;
            this.strikeOutCheck.CheckedChanged += new EventHandler(this.OnFormatChanged);
            this.overlineCheck.SetBounds(120, 0xcc, 0x6a, 20);
            this.overlineCheck.Text = System.Design.SR.GetString("BDLFmt_FontOverline");
            this.overlineCheck.TabIndex = 0x11;
            this.overlineCheck.TextAlign = ContentAlignment.MiddleLeft;
            this.overlineCheck.FlatStyle = FlatStyle.System;
            this.overlineCheck.CheckedChanged += new EventHandler(this.OnFormatChanged);
            label7.SetBounds(0, 0xf6, 0xe0, 14);
            label7.Text = System.Design.SR.GetString("BDLFmt_AlignmentGroup");
            label7.TabStop = false;
            label7.TabIndex = 0x12;
            label8.SetBounds(8, 0x108, 160, 14);
            label8.Text = System.Design.SR.GetString("BDLFmt_HorzAlign");
            label8.TabStop = false;
            label8.TabIndex = 0x13;
            this.horzAlignCombo.SetBounds(8, 280, 190, 0x16);
            this.horzAlignCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.horzAlignCombo.Items.AddRange(new object[] { System.Design.SR.GetString("BDLFmt_HA_Left"), System.Design.SR.GetString("BDLFmt_HA_Center"), System.Design.SR.GetString("BDLFmt_HA_Right"), System.Design.SR.GetString("BDLFmt_HA_Justify") });
            this.horzAlignCombo.TabIndex = 20;
            this.horzAlignCombo.SelectedIndexChanged += new EventHandler(this.OnFormatChanged);
            this.vertAlignLabel.SetBounds(8, 0x132, 160, 14);
            this.vertAlignLabel.Text = System.Design.SR.GetString("BDLFmt_VertAlign");
            this.vertAlignLabel.TabStop = false;
            this.vertAlignLabel.TabIndex = 0x15;
            this.vertAlignCombo.SetBounds(8, 0x142, 190, 0x16);
            this.vertAlignCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.vertAlignCombo.Items.AddRange(new object[] { System.Design.SR.GetString("BDLFmt_VA_Top"), System.Design.SR.GetString("BDLFmt_VA_Middle"), System.Design.SR.GetString("BDLFmt_VA_Bottom") });
            this.vertAlignCombo.TabIndex = 0x16;
            this.vertAlignCombo.SelectedIndexChanged += new EventHandler(this.OnFormatChanged);
            this.allowWrappingCheck.SetBounds(8, 0x15c, 200, 0x11);
            this.allowWrappingCheck.Text = System.Design.SR.GetString("BDLFmt_AllowWrapping");
            this.allowWrappingCheck.TabIndex = 0x18;
            this.allowWrappingCheck.FlatStyle = FlatStyle.System;
            this.allowWrappingCheck.CheckedChanged += new EventHandler(this.OnFormatChanged);
            if (base.IsDataGridMode)
            {
                this.columnPanel.SetBounds(0xb1, 4, 0x117, 350);
                this.columnPanel.TabIndex = 7;
                this.columnPanel.Visible = false;
                label9.SetBounds(0, 0, 0x117, 14);
                label9.Text = System.Design.SR.GetString("BDLFmt_LayoutGroup");
                label9.TabStop = false;
                label9.TabIndex = 0;
                label10.SetBounds(8, 20, 0x40, 14);
                label10.Text = System.Design.SR.GetString("BDLFmt_Width");
                label10.TabStop = false;
                label10.TabIndex = 1;
                this.widthUnit.SetBounds(80, 0x11, 0x66, 0x16);
                this.widthUnit.AllowNegativeValues = false;
                this.widthUnit.DefaultUnit = 0;
                this.widthUnit.TabIndex = 2;
                this.widthUnit.Changed += new EventHandler(this.OnFormatChanged);
                this.widthUnit.ValueAccessibleName = System.Design.SR.GetString("BDLFmt_WidthValueName");
                this.widthUnit.ValueAccessibleDescription = System.Design.SR.GetString("BDLFmt_WidthValueDesc");
                this.widthUnit.UnitAccessibleName = System.Design.SR.GetString("BDLFmt_WidthUnitName");
                this.widthUnit.UnitAccessibleDescription = System.Design.SR.GetString("BDLFmt_WidthUnitDesc");
            }
            this.Text = System.Design.SR.GetString("BDLFmt_Text");
            base.AccessibleDescription = System.Design.SR.GetString("BDLFmt_Desc");
            base.Size = new Size(0x198, 370);
            base.CommitOnDeactivate = true;
            base.Icon = new Icon(base.GetType(), "FormatPage.ico");
            this.stylePanel.Controls.Clear();
            this.stylePanel.Controls.AddRange(new Control[] { 
                this.allowWrappingCheck, this.vertAlignCombo, this.vertAlignLabel, this.horzAlignCombo, label8, label7, this.overlineCheck, this.strikeOutCheck, this.underlineCheck, this.italicCheck, this.boldCheck, this.fontSizeUnit, this.fontSizeCombo, label6, this.fontNameCombo, label5, 
                this.backColorPickerButton, this.backColorCombo, label4, this.foreColorPickerButton, this.foreColorCombo, label3, label2
             });
            if (base.IsDataGridMode)
            {
                this.columnPanel.Controls.Clear();
                this.columnPanel.Controls.AddRange(new Control[] { this.widthUnit, label10, label9 });
                base.Controls.Clear();
                base.Controls.AddRange(new Control[] { this.columnPanel, this.stylePanel, this.formatTree, label });
            }
            else
            {
                base.Controls.Clear();
                base.Controls.AddRange(new Control[] { this.stylePanel, this.formatTree, label });
            }
        }

        private void InitFormatTree()
        {
            FormatTreeNode node;
            FormatObject obj2;
            if (base.IsDataGridMode)
            {
                System.Web.UI.WebControls.DataGrid baseControl = (System.Web.UI.WebControls.DataGrid) base.GetBaseControl();
                obj2 = new FormatStyle(baseControl.ControlStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_EntireDG"), obj2);
                this.formatTree.Nodes.Add(node);
                this.formatNodes.Add(node);
                obj2 = new FormatStyle(baseControl.HeaderStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_Header"), obj2);
                this.formatTree.Nodes.Add(node);
                this.formatNodes.Add(node);
                obj2 = new FormatStyle(baseControl.FooterStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_Footer"), obj2);
                this.formatTree.Nodes.Add(node);
                this.formatNodes.Add(node);
                obj2 = new FormatStyle(baseControl.PagerStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_Pager"), obj2);
                this.formatTree.Nodes.Add(node);
                this.formatNodes.Add(node);
                FormatTreeNode node2 = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_Items"), null);
                this.formatTree.Nodes.Add(node2);
                obj2 = new FormatStyle(baseControl.ItemStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_NormalItems"), obj2);
                node2.Nodes.Add(node);
                this.formatNodes.Add(node);
                obj2 = new FormatStyle(baseControl.AlternatingItemStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_AltItems"), obj2);
                node2.Nodes.Add(node);
                this.formatNodes.Add(node);
                obj2 = new FormatStyle(baseControl.SelectedItemStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_SelItems"), obj2);
                node2.Nodes.Add(node);
                this.formatNodes.Add(node);
                obj2 = new FormatStyle(baseControl.EditItemStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_EditItems"), obj2);
                node2.Nodes.Add(node);
                this.formatNodes.Add(node);
                DataGridColumnCollection columns = baseControl.Columns;
                int count = columns.Count;
                if (count != 0)
                {
                    FormatTreeNode node3 = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_Columns"), null);
                    this.formatTree.Nodes.Add(node3);
                    for (int i = 0; i < count; i++)
                    {
                        DataGridColumn runtimeColumn = columns[i];
                        string text = "Columns[" + i.ToString(NumberFormatInfo.CurrentInfo) + "]";
                        string headerText = runtimeColumn.HeaderText;
                        if (headerText.Length != 0)
                        {
                            text = text + " - " + headerText;
                        }
                        obj2 = new FormatColumn(runtimeColumn);
                        obj2.LoadFormatInfo();
                        FormatTreeNode node4 = new FormatTreeNode(text, obj2);
                        node3.Nodes.Add(node4);
                        this.formatNodes.Add(node4);
                        obj2 = new FormatStyle(runtimeColumn.HeaderStyle);
                        obj2.LoadFormatInfo();
                        node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_Header"), obj2);
                        node4.Nodes.Add(node);
                        this.formatNodes.Add(node);
                        obj2 = new FormatStyle(runtimeColumn.FooterStyle);
                        obj2.LoadFormatInfo();
                        node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_Footer"), obj2);
                        node4.Nodes.Add(node);
                        this.formatNodes.Add(node);
                        obj2 = new FormatStyle(runtimeColumn.ItemStyle);
                        obj2.LoadFormatInfo();
                        node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_Items"), obj2);
                        node4.Nodes.Add(node);
                        this.formatNodes.Add(node);
                    }
                }
            }
            else
            {
                DataList list = (DataList) base.GetBaseControl();
                obj2 = new FormatStyle(list.ControlStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_EntireDL"), obj2);
                this.formatTree.Nodes.Add(node);
                this.formatNodes.Add(node);
                obj2 = new FormatStyle(list.HeaderStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_Header"), obj2);
                this.formatTree.Nodes.Add(node);
                this.formatNodes.Add(node);
                obj2 = new FormatStyle(list.FooterStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_Footer"), obj2);
                this.formatTree.Nodes.Add(node);
                this.formatNodes.Add(node);
                FormatTreeNode node5 = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_Items"), null);
                this.formatTree.Nodes.Add(node5);
                obj2 = new FormatStyle(list.ItemStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_NormalItems"), obj2);
                node5.Nodes.Add(node);
                this.formatNodes.Add(node);
                obj2 = new FormatStyle(list.AlternatingItemStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_AltItems"), obj2);
                node5.Nodes.Add(node);
                this.formatNodes.Add(node);
                obj2 = new FormatStyle(list.SelectedItemStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_SelItems"), obj2);
                node5.Nodes.Add(node);
                this.formatNodes.Add(node);
                obj2 = new FormatStyle(list.EditItemStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_EditItems"), obj2);
                node5.Nodes.Add(node);
                this.formatNodes.Add(node);
                obj2 = new FormatStyle(list.SeparatorStyle);
                obj2.LoadFormatInfo();
                node = new FormatTreeNode(System.Design.SR.GetString("BDLFmt_Node_Separators"), obj2);
                this.formatTree.Nodes.Add(node);
                this.formatNodes.Add(node);
            }
        }

        private void InitFormatUI()
        {
            this.foreColorCombo.Color = null;
            this.backColorCombo.Color = null;
            this.fontNameCombo.Text = string.Empty;
            this.fontNameCombo.SelectedIndex = -1;
            this.fontSizeCombo.SelectedIndex = -1;
            this.fontSizeUnit.Value = null;
            this.italicCheck.Checked = false;
            this.underlineCheck.Checked = false;
            this.strikeOutCheck.Checked = false;
            this.overlineCheck.Checked = false;
            this.horzAlignCombo.SelectedIndex = -1;
            this.vertAlignCombo.SelectedIndex = -1;
            this.allowWrappingCheck.Checked = false;
            if (base.IsDataGridMode)
            {
                this.widthUnit.Value = null;
                this.columnPanel.Visible = false;
            }
            this.stylePanel.Visible = false;
        }

        private void InitPage()
        {
            this.formatNodes = new ArrayList();
            this.propChangesPending = false;
            this.fontNameChanged = false;
            this.currentFormatNode = null;
            this.currentFormatObject = null;
            this.formatTree.Nodes.Clear();
            this.InitFormatUI();
        }

        protected override void LoadComponent()
        {
            if (base.IsFirstActivate())
            {
                this.InitFontList();
            }
            this.InitPage();
            this.InitFormatTree();
        }

        private void LoadFormatProperties()
        {
            if (this.currentFormatObject != null)
            {
                base.EnterLoadingMode();
                this.InitFormatUI();
                if (this.currentFormatObject is FormatStyle)
                {
                    FormatStyle currentFormatObject = (FormatStyle) this.currentFormatObject;
                    this.foreColorCombo.Color = currentFormatObject.foreColor;
                    this.backColorCombo.Color = currentFormatObject.backColor;
                    int num = -1;
                    if (currentFormatObject.fontName.Length != 0)
                    {
                        num = this.fontNameCombo.FindStringExact(currentFormatObject.fontName);
                    }
                    if (num != -1)
                    {
                        this.fontNameCombo.SelectedIndex = num;
                    }
                    else
                    {
                        this.fontNameCombo.Text = currentFormatObject.fontName;
                    }
                    this.boldCheck.Checked = currentFormatObject.bold;
                    this.italicCheck.Checked = currentFormatObject.italic;
                    this.underlineCheck.Checked = currentFormatObject.underline;
                    this.strikeOutCheck.Checked = currentFormatObject.strikeOut;
                    this.overlineCheck.Checked = currentFormatObject.overline;
                    if (currentFormatObject.fontType != -1)
                    {
                        this.fontSizeCombo.SelectedIndex = currentFormatObject.fontType;
                        if (currentFormatObject.fontType == 10)
                        {
                            this.fontSizeUnit.Value = currentFormatObject.fontSize;
                        }
                    }
                    if (currentFormatObject.horzAlignment == 0)
                    {
                        this.horzAlignCombo.SelectedIndex = -1;
                    }
                    else
                    {
                        this.horzAlignCombo.SelectedIndex = currentFormatObject.horzAlignment;
                    }
                    if (currentFormatObject.vertAlignment == 0)
                    {
                        this.vertAlignCombo.SelectedIndex = -1;
                    }
                    else
                    {
                        this.vertAlignCombo.SelectedIndex = currentFormatObject.vertAlignment;
                    }
                    this.allowWrappingCheck.Checked = currentFormatObject.allowWrapping;
                }
                else
                {
                    FormatColumn column = (FormatColumn) this.currentFormatObject;
                    this.widthUnit.Value = column.width;
                }
                base.ExitLoadingMode();
            }
            this.UpdateEnabledVisibleState();
        }

        private void OnClickBackColorPicker(object source, EventArgs e)
        {
            string color = this.backColorCombo.Color;
            color = ColorBuilder.BuildColor(base.GetBaseControl(), this, color);
            if (color != null)
            {
                this.backColorCombo.Color = color;
                this.OnFormatChanged(this.backColorCombo, EventArgs.Empty);
            }
        }

        private void OnClickForeColorPicker(object source, EventArgs e)
        {
            string color = this.foreColorCombo.Color;
            color = ColorBuilder.BuildColor(base.GetBaseControl(), this, color);
            if (color != null)
            {
                this.foreColorCombo.Color = color;
                this.OnFormatChanged(this.foreColorCombo, EventArgs.Empty);
            }
        }

        private void OnFontNameChanged(object source, EventArgs e)
        {
            if (!base.IsLoading())
            {
                this.fontNameChanged = true;
                this.OnFormatChanged(this.fontNameCombo, EventArgs.Empty);
            }
        }

        private void OnFontSizeChanged(object source, EventArgs e)
        {
            if (!base.IsLoading())
            {
                this.UpdateEnabledVisibleState();
                this.OnFormatChanged(this.fontSizeCombo, EventArgs.Empty);
            }
        }

        private void OnFormatChanged(object source, EventArgs e)
        {
            if (!base.IsLoading() && (this.currentFormatNode != null))
            {
                this.SetDirty();
                this.propChangesPending = true;
                this.currentFormatNode.Dirty = true;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (this.formatTree.Nodes.Count != 0)
            {
                IntPtr handle = this.formatTree.Handle;
                this.formatTree.SelectedNode = this.formatTree.Nodes[0];
            }
        }

        private void OnSelChangedFormatObject(object source, TreeViewEventArgs e)
        {
            if (this.propChangesPending)
            {
                this.SaveFormatProperties();
            }
            this.currentFormatNode = (FormatTreeNode) this.formatTree.SelectedNode;
            if (this.currentFormatNode != null)
            {
                this.currentFormatObject = this.currentFormatNode.FormatObject;
            }
            else
            {
                this.currentFormatObject = null;
            }
            this.LoadFormatProperties();
        }

        protected override void SaveComponent()
        {
            if (this.propChangesPending)
            {
                this.SaveFormatProperties();
            }
            IEnumerator enumerator = this.formatNodes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                FormatTreeNode current = (FormatTreeNode) enumerator.Current;
                if (current.Dirty)
                {
                    current.FormatObject.SaveFormatInfo();
                    current.Dirty = false;
                }
            }
            base.GetBaseDesigner().OnStylesChanged();
        }

        private void SaveFormatProperties()
        {
            if (this.currentFormatObject != null)
            {
                if (this.currentFormatObject is FormatStyle)
                {
                    FormatStyle currentFormatObject = (FormatStyle) this.currentFormatObject;
                    currentFormatObject.foreColor = this.foreColorCombo.Color;
                    currentFormatObject.backColor = this.backColorCombo.Color;
                    if (this.fontNameChanged)
                    {
                        currentFormatObject.fontName = this.fontNameCombo.Text.Trim();
                        currentFormatObject.fontNameChanged = true;
                        this.fontNameChanged = false;
                    }
                    currentFormatObject.bold = this.boldCheck.Checked;
                    currentFormatObject.italic = this.italicCheck.Checked;
                    currentFormatObject.underline = this.underlineCheck.Checked;
                    currentFormatObject.strikeOut = this.strikeOutCheck.Checked;
                    currentFormatObject.overline = this.overlineCheck.Checked;
                    if (this.fontSizeCombo.IsSet())
                    {
                        currentFormatObject.fontType = this.fontSizeCombo.SelectedIndex;
                        if (currentFormatObject.fontType == 10)
                        {
                            currentFormatObject.fontSize = this.fontSizeUnit.Value;
                        }
                    }
                    else
                    {
                        currentFormatObject.fontType = -1;
                    }
                    int selectedIndex = this.horzAlignCombo.SelectedIndex;
                    if (selectedIndex == -1)
                    {
                        selectedIndex = 0;
                    }
                    currentFormatObject.horzAlignment = selectedIndex;
                    selectedIndex = this.vertAlignCombo.SelectedIndex;
                    if (selectedIndex == -1)
                    {
                        selectedIndex = 0;
                    }
                    currentFormatObject.vertAlignment = selectedIndex;
                    currentFormatObject.allowWrapping = this.allowWrappingCheck.Checked;
                }
                else
                {
                    FormatColumn column = (FormatColumn) this.currentFormatObject;
                    column.width = this.widthUnit.Value;
                }
                this.currentFormatNode.Dirty = true;
            }
            this.propChangesPending = false;
        }

        public override void SetComponent(IComponent component)
        {
            base.SetComponent(component);
            this.InitForm();
        }

        private void UpdateEnabledVisibleState()
        {
            if (this.currentFormatObject == null)
            {
                this.stylePanel.Visible = false;
                if (base.IsDataGridMode)
                {
                    this.columnPanel.Visible = false;
                }
            }
            else if (this.currentFormatObject is FormatStyle)
            {
                this.stylePanel.Visible = true;
                if (base.IsDataGridMode)
                {
                    this.columnPanel.Visible = false;
                }
                this.fontSizeUnit.Enabled = this.fontSizeCombo.SelectedIndex == 10;
                if (((FormatStyle) this.currentFormatObject).IsTableItemStyle)
                {
                    this.vertAlignLabel.Visible = true;
                    this.vertAlignCombo.Visible = true;
                    this.allowWrappingCheck.Visible = true;
                }
                else
                {
                    this.vertAlignLabel.Visible = false;
                    this.vertAlignCombo.Visible = false;
                    this.allowWrappingCheck.Visible = false;
                }
            }
            else
            {
                this.stylePanel.Visible = false;
                this.columnPanel.Visible = true;
            }
        }

        protected override string HelpKeyword
        {
            get
            {
                if (base.IsDataGridMode)
                {
                    return "net.Asp.DataGridProperties.Format";
                }
                return "net.Asp.DataListProperties.Format";
            }
        }

        private class FormatColumn : FormatPage.FormatObject
        {
            protected DataGridColumn runtimeColumn;
            public string width;

            public FormatColumn(DataGridColumn runtimeColumn)
            {
                this.runtimeColumn = runtimeColumn;
            }

            public override void LoadFormatInfo()
            {
                TableItemStyle headerStyle = this.runtimeColumn.HeaderStyle;
                if (!headerStyle.Width.IsEmpty)
                {
                    this.width = headerStyle.Width.ToString(NumberFormatInfo.CurrentInfo);
                }
                else
                {
                    this.width = null;
                }
            }

            public override void SaveFormatInfo()
            {
                TableItemStyle headerStyle = this.runtimeColumn.HeaderStyle;
                if (this.width == null)
                {
                    headerStyle.Width = Unit.Empty;
                }
                else
                {
                    try
                    {
                        headerStyle.Width = new Unit(this.width, CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private abstract class FormatObject
        {
            protected FormatObject()
            {
            }

            public abstract void LoadFormatInfo();
            public abstract void SaveFormatInfo();
        }

        private class FormatStyle : FormatPage.FormatObject
        {
            public bool allowWrapping;
            public string backColor;
            public bool bold;
            public string fontName;
            public bool fontNameChanged;
            public string fontSize;
            public int fontType;
            public string foreColor;
            public int horzAlignment;
            public bool italic;
            public bool overline;
            protected Style runtimeStyle;
            public bool strikeOut;
            public bool underline;
            public int vertAlignment;

            public FormatStyle(Style runtimeStyle)
            {
                this.runtimeStyle = runtimeStyle;
            }

            public override void LoadFormatInfo()
            {
                HorizontalAlign horizontalAlign;
                Color backColor = this.runtimeStyle.BackColor;
                this.backColor = ColorTranslator.ToHtml(backColor);
                backColor = this.runtimeStyle.ForeColor;
                this.foreColor = ColorTranslator.ToHtml(backColor);
                FontInfo font = this.runtimeStyle.Font;
                this.fontName = font.Name;
                this.fontNameChanged = false;
                this.bold = font.Bold;
                this.italic = font.Italic;
                this.underline = font.Underline;
                this.strikeOut = font.Strikeout;
                this.overline = font.Overline;
                this.fontType = -1;
                FontUnit size = font.Size;
                if (!size.IsEmpty)
                {
                    this.fontSize = null;
                    switch (size.Type)
                    {
                        case FontSize.AsUnit:
                            this.fontType = 10;
                            this.fontSize = size.ToString(CultureInfo.CurrentCulture);
                            break;

                        case FontSize.Smaller:
                            this.fontType = 1;
                            break;

                        case FontSize.Larger:
                            this.fontType = 2;
                            break;

                        case FontSize.XXSmall:
                            this.fontType = 3;
                            break;

                        case FontSize.XSmall:
                            this.fontType = 4;
                            break;

                        case FontSize.Small:
                            this.fontType = 5;
                            break;

                        case FontSize.Medium:
                            this.fontType = 6;
                            break;

                        case FontSize.Large:
                            this.fontType = 7;
                            break;

                        case FontSize.XLarge:
                            this.fontType = 8;
                            break;

                        case FontSize.XXLarge:
                            this.fontType = 9;
                            break;
                    }
                }
                TableItemStyle runtimeStyle = null;
                if (this.runtimeStyle is TableItemStyle)
                {
                    runtimeStyle = (TableItemStyle) this.runtimeStyle;
                    horizontalAlign = runtimeStyle.HorizontalAlign;
                    this.allowWrapping = runtimeStyle.Wrap;
                }
                else
                {
                    horizontalAlign = ((TableStyle) this.runtimeStyle).HorizontalAlign;
                }
                this.horzAlignment = 0;
                switch (horizontalAlign)
                {
                    case HorizontalAlign.Left:
                        this.horzAlignment = 1;
                        break;

                    case HorizontalAlign.Center:
                        this.horzAlignment = 2;
                        break;

                    case HorizontalAlign.Right:
                        this.horzAlignment = 3;
                        break;

                    case HorizontalAlign.Justify:
                        this.horzAlignment = 4;
                        break;
                }
                if (runtimeStyle != null)
                {
                    VerticalAlign verticalAlign = runtimeStyle.VerticalAlign;
                    this.vertAlignment = 0;
                    switch (verticalAlign)
                    {
                        case VerticalAlign.Top:
                            this.vertAlignment = 1;
                            return;

                        case VerticalAlign.Middle:
                            this.vertAlignment = 2;
                            return;

                        case VerticalAlign.Bottom:
                            this.vertAlignment = 3;
                            break;

                        default:
                            return;
                    }
                }
            }

            public override void SaveFormatInfo()
            {
                try
                {
                    this.runtimeStyle.BackColor = ColorTranslator.FromHtml(this.backColor);
                    this.runtimeStyle.ForeColor = ColorTranslator.FromHtml(this.foreColor);
                }
                catch
                {
                }
                FontInfo font = this.runtimeStyle.Font;
                if (this.fontNameChanged)
                {
                    font.Name = this.fontName;
                    this.fontNameChanged = false;
                }
                font.Bold = this.bold;
                font.Italic = this.italic;
                font.Underline = this.underline;
                font.Strikeout = this.strikeOut;
                font.Overline = this.overline;
                switch (this.fontType)
                {
                    case 1:
                        font.Size = FontUnit.Smaller;
                        break;

                    case 2:
                        font.Size = FontUnit.Larger;
                        break;

                    case 3:
                        font.Size = FontUnit.XXSmall;
                        break;

                    case 4:
                        font.Size = FontUnit.XSmall;
                        break;

                    case 5:
                        font.Size = FontUnit.Small;
                        break;

                    case 6:
                        font.Size = FontUnit.Medium;
                        break;

                    case 7:
                        font.Size = FontUnit.Large;
                        break;

                    case 8:
                        font.Size = FontUnit.XLarge;
                        break;

                    case 9:
                        font.Size = FontUnit.XXLarge;
                        break;

                    case 10:
                        try
                        {
                            font.Size = new FontUnit(this.fontSize, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                        }
                        break;

                    case -1:
                        font.Size = FontUnit.Empty;
                        break;
                }
                TableItemStyle runtimeStyle = null;
                HorizontalAlign notSet = HorizontalAlign.NotSet;
                switch (this.horzAlignment)
                {
                    case 0:
                        notSet = HorizontalAlign.NotSet;
                        break;

                    case 1:
                        notSet = HorizontalAlign.Left;
                        break;

                    case 2:
                        notSet = HorizontalAlign.Center;
                        break;

                    case 3:
                        notSet = HorizontalAlign.Right;
                        break;

                    case 4:
                        notSet = HorizontalAlign.Justify;
                        break;
                }
                if (this.runtimeStyle is TableItemStyle)
                {
                    runtimeStyle = (TableItemStyle) this.runtimeStyle;
                    runtimeStyle.HorizontalAlign = notSet;
                    if (!this.allowWrapping)
                    {
                        runtimeStyle.Wrap = false;
                    }
                }
                else
                {
                    ((TableStyle) this.runtimeStyle).HorizontalAlign = notSet;
                }
                if (runtimeStyle != null)
                {
                    switch (this.vertAlignment)
                    {
                        case 0:
                            runtimeStyle.VerticalAlign = VerticalAlign.NotSet;
                            return;

                        case 1:
                            runtimeStyle.VerticalAlign = VerticalAlign.Top;
                            return;

                        case 2:
                            runtimeStyle.VerticalAlign = VerticalAlign.Middle;
                            return;

                        case 3:
                            runtimeStyle.VerticalAlign = VerticalAlign.Bottom;
                            break;

                        default:
                            return;
                    }
                }
            }

            public bool IsTableItemStyle
            {
                get
                {
                    return (this.runtimeStyle is TableItemStyle);
                }
            }
        }

        private class FormatTreeNode : System.Windows.Forms.TreeNode
        {
            protected bool dirty;
            protected System.Web.UI.Design.WebControls.ListControls.FormatPage.FormatObject formatObject;

            public FormatTreeNode(string text, System.Web.UI.Design.WebControls.ListControls.FormatPage.FormatObject formatObject) : base(text)
            {
                this.formatObject = formatObject;
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

            public System.Web.UI.Design.WebControls.ListControls.FormatPage.FormatObject FormatObject
            {
                get
                {
                    return this.formatObject;
                }
            }
        }
    }
}

