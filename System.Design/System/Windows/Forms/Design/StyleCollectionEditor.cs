namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    internal class StyleCollectionEditor : CollectionEditor
    {
        protected string helptopic;
        private bool isRowCollection;

        public StyleCollectionEditor(System.Type type) : base(type)
        {
            this.isRowCollection = type.IsAssignableFrom(typeof(TableLayoutRowStyleCollection));
        }

        protected override CollectionEditor.CollectionForm CreateCollectionForm()
        {
            return new StyleEditorForm(this, this.isRowCollection);
        }

        protected override string HelpTopic
        {
            get
            {
                return this.helptopic;
            }
        }

        protected class NavigationalTableLayoutPanel : TableLayoutPanel
        {
            protected override bool ProcessDialogKey(Keys keyData)
            {
                bool flag = keyData == Keys.Down;
                bool flag2 = keyData == Keys.Up;
                if (flag || flag2)
                {
                    List<RadioButton> radioButtons = this.RadioButtons;
                    for (int i = 0; i < radioButtons.Count; i++)
                    {
                        RadioButton button = radioButtons[i];
                        if (button.Focused)
                        {
                            int num2;
                            if (flag)
                            {
                                num2 = (i == (this.RadioButtons.Count - 1)) ? 0 : (i + 1);
                            }
                            else
                            {
                                num2 = (i == 0) ? (this.RadioButtons.Count - 1) : (i - 1);
                            }
                            radioButtons[num2].Focus();
                            return true;
                        }
                    }
                }
                return base.ProcessDialogKey(keyData);
            }

            private List<RadioButton> RadioButtons
            {
                get
                {
                    List<RadioButton> list = new List<RadioButton>();
                    foreach (Control control in base.Controls)
                    {
                        RadioButton item = control as RadioButton;
                        if (item != null)
                        {
                            list.Add(item);
                        }
                    }
                    return list;
                }
            }
        }

        protected class StyleEditorForm : CollectionEditor.CollectionForm
        {
            private NumericUpDown absoluteNumericUpDown;
            private RadioButton absoluteRadioButton;
            private Button addButton;
            private TableLayoutPanel addRemoveInsertTableLayoutPanel;
            private RadioButton autoSizedRadioButton;
            private Button cancelButton;
            private PropertyDescriptor colStyleProp;
            private ListView columnsAndRowsListView;
            private ComboBox columnsOrRowsComboBox;
            private IComponentChangeService compSvc;
            private ArrayList deleteList;
            private StyleCollectionEditor editor;
            private bool haveInvoked;
            private LinkLabel helperLinkLabel1;
            private LinkLabel helperLinkLabel2;
            private TableLayoutPanel helperTextTableLayoutPanel;
            private PictureBox infoPictureBox1;
            private PictureBox infoPictureBox2;
            private Button insertButton;
            private bool isDialogDirty;
            private bool isRowCollection;
            private static int MEMBER_INDEX = 0;
            private ColumnHeader membersColumnHeader;
            private Label memberTypeLabel;
            private Button okButton;
            private TableLayoutPanel okCancelTableLayoutPanel;
            private TableLayoutPanel overarchingTableLayoutPanel;
            private Label percentLabel;
            private NumericUpDown percentNumericUpDown;
            private RadioButton percentRadioButton;
            private Label pixelsLabel;
            private Button removeButton;
            private PropertyDescriptor rowStyleProp;
            private TableLayoutPanel showTableLayoutPanel;
            private ColumnHeader sizeTypeColumnHeader;
            private GroupBox sizeTypeGroupBox;
            private StyleCollectionEditor.NavigationalTableLayoutPanel sizeTypeTableLayoutPanel;
            private TableLayoutPanel tlp;
            private TableLayoutPanelDesigner tlpDesigner;
            private static int TYPE_INDEX = 1;
            private static int VALUE_INDEX = 2;
            private ColumnHeader valueColumnHeader;

            internal StyleEditorForm(CollectionEditor editor, bool isRowCollection) : base(editor)
            {
                this.editor = (StyleCollectionEditor) editor;
                this.isRowCollection = isRowCollection;
                this.InitializeComponent();
                this.HookEvents();
                DesignerUtils.ApplyListViewThemeStyles(this.columnsAndRowsListView);
                base.ActiveControl = this.columnsAndRowsListView;
                this.tlp = base.Context.Instance as TableLayoutPanel;
                this.tlp.SuspendLayout();
                this.deleteList = new ArrayList();
                IDesignerHost service = this.tlp.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (service != null)
                {
                    this.tlpDesigner = service.GetDesigner(this.tlp) as TableLayoutPanelDesigner;
                    this.compSvc = service.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                }
                this.rowStyleProp = TypeDescriptor.GetProperties(this.tlp)["RowStyles"];
                this.colStyleProp = TypeDescriptor.GetProperties(this.tlp)["ColumnStyles"];
                this.tlpDesigner.SuspendEnsureAvailableStyles();
            }

            private void AddItem(int index)
            {
                string str = null;
                this.tlpDesigner.InsertRowCol(this.isRowCollection, index);
                if (this.isRowCollection)
                {
                    str = "Row" + this.tlp.RowStyles.Count.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    str = "Column" + this.tlp.RowStyles.Count.ToString(CultureInfo.InvariantCulture);
                }
                if (str != null)
                {
                    this.columnsAndRowsListView.Items.Insert(index, new ListViewItem(new string[] { str, SizeType.Absolute.ToString(), DesignerUtils.MINIMUMSTYLESIZE.ToString(CultureInfo.InvariantCulture) }));
                    this.UpdateListViewMember();
                    this.ClearAndSetSelectionAndFocus(index);
                }
            }

            private void ClearAndSetSelectionAndFocus(int index)
            {
                this.columnsAndRowsListView.BeginUpdate();
                this.columnsAndRowsListView.Focus();
                if (this.columnsAndRowsListView.FocusedItem != null)
                {
                    this.columnsAndRowsListView.FocusedItem.Focused = false;
                }
                this.columnsAndRowsListView.SelectedItems.Clear();
                this.columnsAndRowsListView.Items[index].Selected = true;
                this.columnsAndRowsListView.Items[index].Focused = true;
                this.columnsAndRowsListView.Items[index].EnsureVisible();
                this.columnsAndRowsListView.EndUpdate();
            }

            private string FormatValueString(SizeType type, float value)
            {
                if (type == SizeType.Absolute)
                {
                    return value.ToString(CultureInfo.CurrentCulture);
                }
                if (type == SizeType.Percent)
                {
                    float num = value / 100f;
                    return num.ToString("P", CultureInfo.CurrentCulture);
                }
                return string.Empty;
            }

            private void HookEvents()
            {
                base.HelpButtonClicked += new CancelEventHandler(this.OnHelpButtonClicked);
                this.columnsAndRowsListView.SelectedIndexChanged += new EventHandler(this.OnListViewSelectedIndexChanged);
                this.columnsOrRowsComboBox.SelectionChangeCommitted += new EventHandler(this.OnComboBoxSelectionChangeCommitted);
                this.okButton.Click += new EventHandler(this.OnOkButtonClick);
                this.cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
                this.addButton.Click += new EventHandler(this.OnAddButtonClick);
                this.removeButton.Click += new EventHandler(this.OnRemoveButtonClick);
                this.insertButton.Click += new EventHandler(this.OnInsertButtonClick);
                this.absoluteRadioButton.Enter += new EventHandler(this.OnAbsoluteEnter);
                this.absoluteNumericUpDown.ValueChanged += new EventHandler(this.OnValueChanged);
                this.percentRadioButton.Enter += new EventHandler(this.OnPercentEnter);
                this.percentNumericUpDown.ValueChanged += new EventHandler(this.OnValueChanged);
                this.autoSizedRadioButton.Enter += new EventHandler(this.OnAutoSizeEnter);
                base.Shown += new EventHandler(this.OnShown);
                this.helperLinkLabel1.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnLink1Click);
                this.helperLinkLabel2.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnLink2Click);
            }

            private void InitializeComponent()
            {
                ComponentResourceManager manager = new ComponentResourceManager(typeof(StyleCollectionEditor));
                this.addRemoveInsertTableLayoutPanel = new TableLayoutPanel();
                this.addButton = new Button();
                this.removeButton = new Button();
                this.insertButton = new Button();
                this.okCancelTableLayoutPanel = new TableLayoutPanel();
                this.okButton = new Button();
                this.cancelButton = new Button();
                this.overarchingTableLayoutPanel = new TableLayoutPanel();
                this.showTableLayoutPanel = new TableLayoutPanel();
                this.memberTypeLabel = new Label();
                this.columnsOrRowsComboBox = new ComboBox();
                this.columnsAndRowsListView = new ListView();
                this.membersColumnHeader = new ColumnHeader(manager.GetString("columnsAndRowsListView.Columns"));
                this.sizeTypeColumnHeader = new ColumnHeader(manager.GetString("columnsAndRowsListView.Columns1"));
                this.valueColumnHeader = new ColumnHeader(manager.GetString("columnsAndRowsListView.Columns2"));
                this.helperTextTableLayoutPanel = new TableLayoutPanel();
                this.infoPictureBox2 = new PictureBox();
                this.infoPictureBox1 = new PictureBox();
                this.helperLinkLabel1 = new LinkLabel();
                this.helperLinkLabel2 = new LinkLabel();
                this.sizeTypeGroupBox = new GroupBox();
                this.sizeTypeTableLayoutPanel = new StyleCollectionEditor.NavigationalTableLayoutPanel();
                this.absoluteNumericUpDown = new NumericUpDown();
                this.absoluteRadioButton = new RadioButton();
                this.pixelsLabel = new Label();
                this.percentLabel = new Label();
                this.percentRadioButton = new RadioButton();
                this.autoSizedRadioButton = new RadioButton();
                this.percentNumericUpDown = new NumericUpDown();
                this.addRemoveInsertTableLayoutPanel.SuspendLayout();
                this.okCancelTableLayoutPanel.SuspendLayout();
                this.overarchingTableLayoutPanel.SuspendLayout();
                this.showTableLayoutPanel.SuspendLayout();
                this.helperTextTableLayoutPanel.SuspendLayout();
                ((ISupportInitialize) this.infoPictureBox2).BeginInit();
                ((ISupportInitialize) this.infoPictureBox1).BeginInit();
                this.sizeTypeGroupBox.SuspendLayout();
                this.sizeTypeTableLayoutPanel.SuspendLayout();
                this.absoluteNumericUpDown.BeginInit();
                this.percentNumericUpDown.BeginInit();
                base.SuspendLayout();
                manager.ApplyResources(this.addRemoveInsertTableLayoutPanel, "addRemoveInsertTableLayoutPanel");
                this.addRemoveInsertTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
                this.addRemoveInsertTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
                this.addRemoveInsertTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
                this.addRemoveInsertTableLayoutPanel.Controls.Add(this.addButton, 0, 0);
                this.addRemoveInsertTableLayoutPanel.Controls.Add(this.removeButton, 1, 0);
                this.addRemoveInsertTableLayoutPanel.Controls.Add(this.insertButton, 2, 0);
                this.addRemoveInsertTableLayoutPanel.Margin = new Padding(0, 3, 3, 3);
                this.addRemoveInsertTableLayoutPanel.Name = "addRemoveInsertTableLayoutPanel";
                this.addRemoveInsertTableLayoutPanel.RowStyles.Add(new RowStyle());
                manager.ApplyResources(this.addButton, "addButton");
                this.addButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                this.addButton.Margin = new Padding(0, 0, 4, 0);
                this.addButton.MinimumSize = new Size(0x4b, 0x17);
                this.addButton.Name = "addButton";
                this.addButton.Padding = new Padding(10, 0, 10, 0);
                manager.ApplyResources(this.removeButton, "removeButton");
                this.removeButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                this.removeButton.Margin = new Padding(2, 0, 2, 0);
                this.removeButton.MinimumSize = new Size(0x4b, 0x17);
                this.removeButton.Name = "removeButton";
                this.removeButton.Padding = new Padding(10, 0, 10, 0);
                manager.ApplyResources(this.insertButton, "insertButton");
                this.insertButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                this.insertButton.Margin = new Padding(4, 0, 0, 0);
                this.insertButton.MinimumSize = new Size(0x4b, 0x17);
                this.insertButton.Name = "insertButton";
                this.insertButton.Padding = new Padding(10, 0, 10, 0);
                manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
                this.overarchingTableLayoutPanel.SetColumnSpan(this.okCancelTableLayoutPanel, 2);
                this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                this.okCancelTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
                this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
                this.okCancelTableLayoutPanel.Margin = new Padding(0, 6, 0, 0);
                this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
                this.okCancelTableLayoutPanel.RowStyles.Add(new RowStyle());
                manager.ApplyResources(this.okButton, "okButton");
                this.okButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                this.okButton.Margin = new Padding(0, 0, 3, 0);
                this.okButton.MinimumSize = new Size(0x4b, 0x17);
                this.okButton.Name = "okButton";
                this.okButton.Padding = new Padding(10, 0, 10, 0);
                manager.ApplyResources(this.cancelButton, "cancelButton");
                this.cancelButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                this.cancelButton.DialogResult = DialogResult.Cancel;
                this.cancelButton.Margin = new Padding(3, 0, 0, 0);
                this.cancelButton.MinimumSize = new Size(0x4b, 0x17);
                this.cancelButton.Name = "cancelButton";
                this.cancelButton.Padding = new Padding(10, 0, 10, 0);
                manager.ApplyResources(this.overarchingTableLayoutPanel, "overarchingTableLayoutPanel");
                this.overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                this.overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                this.overarchingTableLayoutPanel.Controls.Add(this.sizeTypeGroupBox, 1, 0);
                this.overarchingTableLayoutPanel.Controls.Add(this.okCancelTableLayoutPanel, 0, 4);
                this.overarchingTableLayoutPanel.Controls.Add(this.showTableLayoutPanel, 0, 0);
                this.overarchingTableLayoutPanel.Controls.Add(this.addRemoveInsertTableLayoutPanel, 0, 3);
                this.overarchingTableLayoutPanel.Controls.Add(this.columnsAndRowsListView, 0, 1);
                this.overarchingTableLayoutPanel.Controls.Add(this.helperTextTableLayoutPanel, 1, 2);
                this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
                this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
                this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
                this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
                this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
                this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
                manager.ApplyResources(this.showTableLayoutPanel, "showTableLayoutPanel");
                this.showTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
                this.showTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                this.showTableLayoutPanel.Controls.Add(this.memberTypeLabel, 0, 0);
                this.showTableLayoutPanel.Controls.Add(this.columnsOrRowsComboBox, 1, 0);
                this.showTableLayoutPanel.Margin = new Padding(0, 0, 3, 3);
                this.showTableLayoutPanel.Name = "showTableLayoutPanel";
                this.showTableLayoutPanel.RowStyles.Add(new RowStyle());
                manager.ApplyResources(this.memberTypeLabel, "memberTypeLabel");
                this.memberTypeLabel.Margin = new Padding(0, 0, 3, 0);
                this.memberTypeLabel.Name = "memberTypeLabel";
                manager.ApplyResources(this.columnsOrRowsComboBox, "columnsOrRowsComboBox");
                this.columnsOrRowsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                this.columnsOrRowsComboBox.FormattingEnabled = true;
                this.columnsOrRowsComboBox.Items.AddRange(new object[] { manager.GetString("columnsOrRowsComboBox.Items"), manager.GetString("columnsOrRowsComboBox.Items1") });
                this.columnsOrRowsComboBox.Margin = new Padding(3, 0, 0, 0);
                this.columnsOrRowsComboBox.Name = "columnsOrRowsComboBox";
                manager.ApplyResources(this.columnsAndRowsListView, "columnsAndRowsListView");
                this.columnsAndRowsListView.Columns.AddRange(new ColumnHeader[] { this.membersColumnHeader, this.sizeTypeColumnHeader, this.valueColumnHeader });
                this.columnsAndRowsListView.FullRowSelect = true;
                this.columnsAndRowsListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
                this.columnsAndRowsListView.HideSelection = false;
                this.columnsAndRowsListView.Margin = new Padding(0, 3, 3, 3);
                this.columnsAndRowsListView.Name = "columnsAndRowsListView";
                this.overarchingTableLayoutPanel.SetRowSpan(this.columnsAndRowsListView, 2);
                this.columnsAndRowsListView.View = View.Details;
                manager.ApplyResources(this.membersColumnHeader, "membersColumnHeader");
                manager.ApplyResources(this.sizeTypeColumnHeader, "sizeTypeColumnHeader");
                manager.ApplyResources(this.valueColumnHeader, "valueColumnHeader");
                manager.ApplyResources(this.helperTextTableLayoutPanel, "helperTextTableLayoutPanel");
                this.helperTextTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
                this.helperTextTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                this.helperTextTableLayoutPanel.Controls.Add(this.infoPictureBox2, 0, 1);
                this.helperTextTableLayoutPanel.Controls.Add(this.infoPictureBox1, 0, 0);
                this.helperTextTableLayoutPanel.Controls.Add(this.helperLinkLabel1, 1, 0);
                this.helperTextTableLayoutPanel.Controls.Add(this.helperLinkLabel2, 1, 1);
                this.helperTextTableLayoutPanel.Margin = new Padding(6, 6, 0, 3);
                this.helperTextTableLayoutPanel.Name = "helperTextTableLayoutPanel";
                this.helperTextTableLayoutPanel.RowStyles.Add(new RowStyle());
                this.helperTextTableLayoutPanel.RowStyles.Add(new RowStyle());
                manager.ApplyResources(this.infoPictureBox2, "infoPictureBox2");
                this.infoPictureBox2.Name = "infoPictureBox2";
                this.infoPictureBox2.TabStop = false;
                manager.ApplyResources(this.infoPictureBox1, "infoPictureBox1");
                this.infoPictureBox1.Name = "infoPictureBox1";
                this.infoPictureBox1.TabStop = false;
                manager.ApplyResources(this.helperLinkLabel1, "helperLinkLabel1");
                this.helperLinkLabel1.Margin = new Padding(3, 0, 0, 3);
                this.helperLinkLabel1.Name = "helperLinkLabel1";
                this.helperLinkLabel1.TabStop = true;
                this.helperLinkLabel1.UseCompatibleTextRendering = true;
                manager.ApplyResources(this.helperLinkLabel2, "helperLinkLabel2");
                this.helperLinkLabel2.Margin = new Padding(3, 3, 0, 0);
                this.helperLinkLabel2.Name = "helperLinkLabel2";
                this.helperLinkLabel2.TabStop = true;
                this.helperLinkLabel2.UseCompatibleTextRendering = true;
                manager.ApplyResources(this.sizeTypeGroupBox, "sizeTypeGroupBox");
                this.sizeTypeGroupBox.Controls.Add(this.sizeTypeTableLayoutPanel);
                this.sizeTypeGroupBox.Margin = new Padding(6, 0, 0, 3);
                this.sizeTypeGroupBox.Name = "sizeTypeGroupBox";
                this.sizeTypeGroupBox.Padding = new Padding(0);
                this.overarchingTableLayoutPanel.SetRowSpan(this.sizeTypeGroupBox, 2);
                this.sizeTypeGroupBox.TabStop = false;
                manager.ApplyResources(this.sizeTypeTableLayoutPanel, "sizeTypeTableLayoutPanel");
                this.sizeTypeTableLayoutPanel.Controls.Add(this.absoluteNumericUpDown, 1, 0);
                this.sizeTypeTableLayoutPanel.Controls.Add(this.absoluteRadioButton, 0, 0);
                this.sizeTypeTableLayoutPanel.Controls.Add(this.pixelsLabel, 2, 0);
                this.sizeTypeTableLayoutPanel.Controls.Add(this.percentLabel, 2, 1);
                this.sizeTypeTableLayoutPanel.Controls.Add(this.percentRadioButton, 0, 1);
                this.sizeTypeTableLayoutPanel.Controls.Add(this.autoSizedRadioButton, 0, 2);
                this.sizeTypeTableLayoutPanel.Controls.Add(this.percentNumericUpDown, 1, 1);
                this.sizeTypeTableLayoutPanel.Margin = new Padding(9);
                this.sizeTypeTableLayoutPanel.Name = "sizeTypeTableLayoutPanel";
                manager.ApplyResources(this.absoluteNumericUpDown, "absoluteNumericUpDown");
                int[] bits = new int[4];
                bits[0] = 0x1869f;
                this.absoluteNumericUpDown.Maximum = new decimal(bits);
                this.absoluteNumericUpDown.Name = "absoluteNumericUpDown";
                manager.ApplyResources(this.absoluteRadioButton, "absoluteRadioButton");
                this.absoluteRadioButton.Margin = new Padding(0, 3, 3, 0);
                this.absoluteRadioButton.Name = "absoluteRadioButton";
                manager.ApplyResources(this.pixelsLabel, "pixelsLabel");
                this.pixelsLabel.Name = "pixelsLabel";
                manager.ApplyResources(this.percentLabel, "percentLabel");
                this.percentLabel.Name = "percentLabel";
                manager.ApplyResources(this.percentRadioButton, "percentRadioButton");
                this.percentRadioButton.Margin = new Padding(0, 3, 3, 0);
                this.percentRadioButton.Name = "percentRadioButton";
                manager.ApplyResources(this.autoSizedRadioButton, "autoSizedRadioButton");
                this.autoSizedRadioButton.Margin = new Padding(0, 3, 3, 0);
                this.autoSizedRadioButton.Name = "autoSizedRadioButton";
                manager.ApplyResources(this.percentNumericUpDown, "percentNumericUpDown");
                this.percentNumericUpDown.DecimalPlaces = 2;
                int[] numArray2 = new int[4];
                numArray2[0] = 0x270f;
                this.percentNumericUpDown.Maximum = new decimal(numArray2);
                this.percentNumericUpDown.Name = "percentNumericUpDown";
                base.AcceptButton = this.okButton;
                manager.ApplyResources(this, "$this");
                base.AutoScaleMode = AutoScaleMode.Font;
                base.CancelButton = this.cancelButton;
                base.Controls.Add(this.overarchingTableLayoutPanel);
                base.HelpButton = true;
                base.MaximizeBox = false;
                base.MinimizeBox = false;
                base.Name = "Form1";
                base.ShowIcon = false;
                base.ShowInTaskbar = false;
                this.addRemoveInsertTableLayoutPanel.ResumeLayout(false);
                this.addRemoveInsertTableLayoutPanel.PerformLayout();
                this.okCancelTableLayoutPanel.ResumeLayout(false);
                this.okCancelTableLayoutPanel.PerformLayout();
                this.overarchingTableLayoutPanel.ResumeLayout(false);
                this.overarchingTableLayoutPanel.PerformLayout();
                this.showTableLayoutPanel.ResumeLayout(false);
                this.showTableLayoutPanel.PerformLayout();
                this.helperTextTableLayoutPanel.ResumeLayout(false);
                this.helperTextTableLayoutPanel.PerformLayout();
                ((ISupportInitialize) this.infoPictureBox2).EndInit();
                ((ISupportInitialize) this.infoPictureBox1).EndInit();
                this.sizeTypeGroupBox.ResumeLayout(false);
                this.sizeTypeTableLayoutPanel.ResumeLayout(false);
                this.sizeTypeTableLayoutPanel.PerformLayout();
                this.absoluteNumericUpDown.EndInit();
                this.percentNumericUpDown.EndInit();
                base.ResumeLayout(false);
            }

            private void InitListView()
            {
                this.columnsAndRowsListView.Items.Clear();
                string str = this.isRowCollection ? "Row" : "Column";
                int num = this.isRowCollection ? this.tlp.RowStyles.Count : this.tlp.ColumnStyles.Count;
                for (int i = 0; i < num; i++)
                {
                    string str2;
                    string str3;
                    if (this.isRowCollection)
                    {
                        RowStyle style = this.tlp.RowStyles[i];
                        str2 = style.SizeType.ToString();
                        str3 = this.FormatValueString(style.SizeType, style.Height);
                    }
                    else
                    {
                        ColumnStyle style2 = this.tlp.ColumnStyles[i];
                        str2 = style2.SizeType.ToString();
                        str3 = this.FormatValueString(style2.SizeType, style2.Width);
                    }
                    string[] items = new string[] { str + ((i + 1)).ToString(CultureInfo.InvariantCulture), str2, str3 };
                    this.columnsAndRowsListView.Items.Add(new ListViewItem(items));
                }
                if (num > 0)
                {
                    this.ClearAndSetSelectionAndFocus(0);
                }
                this.removeButton.Enabled = this.columnsAndRowsListView.Items.Count > 1;
            }

            private void NormalizePercentStyle(bool normalizeRow)
            {
                int num = normalizeRow ? this.tlp.RowStyles.Count : this.tlp.ColumnStyles.Count;
                float num2 = 0f;
                for (int i = 0; i < num; i++)
                {
                    if (normalizeRow)
                    {
                        if (this.tlp.RowStyles[i].SizeType == SizeType.Percent)
                        {
                            num2 += this.tlp.RowStyles[i].Height;
                        }
                    }
                    else if (this.tlp.ColumnStyles[i].SizeType == SizeType.Percent)
                    {
                        num2 += this.tlp.ColumnStyles[i].Width;
                    }
                }
                switch (num2)
                {
                    case 100f:
                    case 0f:
                        return;
                }
                for (int j = 0; j < num; j++)
                {
                    if (normalizeRow)
                    {
                        if (this.tlp.RowStyles[j].SizeType == SizeType.Percent)
                        {
                            this.tlp.RowStyles[j].Height = (this.tlp.RowStyles[j].Height * 100f) / num2;
                        }
                    }
                    else if (this.tlp.ColumnStyles[j].SizeType == SizeType.Percent)
                    {
                        this.tlp.ColumnStyles[j].Width = (this.tlp.ColumnStyles[j].Width * 100f) / num2;
                    }
                }
            }

            private void NormalizePercentStyles()
            {
                this.NormalizePercentStyle(true);
                this.NormalizePercentStyle(false);
            }

            private void OnAbsoluteEnter(object sender, EventArgs e)
            {
                this.isDialogDirty = true;
                this.UpdateTypeAndValue(SizeType.Absolute, (float) this.absoluteNumericUpDown.Value);
                this.absoluteNumericUpDown.Enabled = true;
                this.ResetPercent();
            }

            private void OnAddButtonClick(object sender, EventArgs e)
            {
                this.isDialogDirty = true;
                this.AddItem(this.columnsAndRowsListView.Items.Count);
            }

            private void OnAutoSizeEnter(object sender, EventArgs e)
            {
                this.isDialogDirty = true;
                this.UpdateTypeAndValue(SizeType.AutoSize, 0f);
                this.ResetAbsolute();
                this.ResetPercent();
            }

            private void OnCancelButtonClick(object sender, EventArgs e)
            {
                this.tlpDesigner.ResumeEnsureAvailableStyles(false);
                this.tlp.ResumeLayout();
                base.DialogResult = DialogResult.Cancel;
            }

            private void OnComboBoxSelectionChangeCommitted(object sender, EventArgs e)
            {
                this.isRowCollection = this.columnsOrRowsComboBox.SelectedIndex != 0;
                this.InitListView();
            }

            protected override void OnEditValueChanged()
            {
            }

            private void OnHelpButtonClicked(object sender, CancelEventArgs e)
            {
                e.Cancel = true;
                this.editor.helptopic = "net.ComponentModel.StyleCollectionEditor";
                this.editor.ShowHelp();
            }

            private void OnInsertButtonClick(object sender, EventArgs e)
            {
                this.isDialogDirty = true;
                this.AddItem(this.columnsAndRowsListView.SelectedIndices[0]);
                this.tlpDesigner.FixUpControlsOnInsert(this.isRowCollection, this.columnsAndRowsListView.SelectedIndices[0]);
            }

            private void OnLink1Click(object sender, LinkLabelLinkClickedEventArgs e)
            {
                CancelEventArgs args = new CancelEventArgs();
                this.editor.helptopic = "net.ComponentModel.StyleCollectionEditor.TLP.SpanRowsColumns";
                this.OnHelpButtonClicked(sender, args);
            }

            private void OnLink2Click(object sender, LinkLabelLinkClickedEventArgs e)
            {
                CancelEventArgs args = new CancelEventArgs();
                this.editor.helptopic = "net.ComponentModel.StyleCollectionEditor.TLP.AnchorDock";
                this.OnHelpButtonClicked(sender, args);
            }

            private void OnListSelectionComplete(object sender, EventArgs e)
            {
                this.haveInvoked = false;
                if (this.columnsAndRowsListView.SelectedItems.Count == 0)
                {
                    this.ResetAllRadioButtons();
                    this.sizeTypeGroupBox.Enabled = false;
                    this.insertButton.Enabled = false;
                    this.removeButton.Enabled = false;
                }
            }

            private void OnListViewSelectedIndexChanged(object sender, EventArgs e)
            {
                ListView.SelectedListViewItemCollection selectedItems = this.columnsAndRowsListView.SelectedItems;
                if (selectedItems.Count == 0)
                {
                    if (!this.haveInvoked)
                    {
                        base.BeginInvoke(new EventHandler(this.OnListSelectionComplete));
                        this.haveInvoked = true;
                    }
                }
                else
                {
                    this.sizeTypeGroupBox.Enabled = true;
                    this.insertButton.Enabled = true;
                    if (selectedItems.Count == this.columnsAndRowsListView.Items.Count)
                    {
                        this.removeButton.Enabled = false;
                    }
                    else
                    {
                        this.removeButton.Enabled = this.columnsAndRowsListView.Items.Count > 1;
                    }
                    if (selectedItems.Count == 1)
                    {
                        int index = this.columnsAndRowsListView.Items.IndexOf(selectedItems[0]);
                        if (this.isRowCollection)
                        {
                            this.UpdateGroupBox(this.tlp.RowStyles[index].SizeType, this.tlp.RowStyles[index].Height);
                        }
                        else
                        {
                            this.UpdateGroupBox(this.tlp.ColumnStyles[index].SizeType, this.tlp.ColumnStyles[index].Width);
                        }
                    }
                    else
                    {
                        float num2 = 0f;
                        bool flag = true;
                        int num3 = this.columnsAndRowsListView.Items.IndexOf(selectedItems[0]);
                        SizeType type = this.isRowCollection ? this.tlp.RowStyles[num3].SizeType : this.tlp.ColumnStyles[num3].SizeType;
                        num2 = this.isRowCollection ? this.tlp.RowStyles[num3].Height : this.tlp.ColumnStyles[num3].Width;
                        for (int i = 1; i < selectedItems.Count; i++)
                        {
                            num3 = this.columnsAndRowsListView.Items.IndexOf(selectedItems[i]);
                            if (type != (this.isRowCollection ? this.tlp.RowStyles[num3].SizeType : this.tlp.ColumnStyles[num3].SizeType))
                            {
                                flag = false;
                                break;
                            }
                            if (num2 != (this.isRowCollection ? this.tlp.RowStyles[num3].Height : this.tlp.ColumnStyles[num3].Width))
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            this.ResetAllRadioButtons();
                        }
                        else
                        {
                            this.UpdateGroupBox(type, num2);
                        }
                    }
                }
            }

            private void OnOkButtonClick(object sender, EventArgs e)
            {
                if (this.isDialogDirty)
                {
                    if (this.absoluteRadioButton.Checked)
                    {
                        this.UpdateTypeAndValue(SizeType.Absolute, (float) this.absoluteNumericUpDown.Value);
                    }
                    else if (this.percentRadioButton.Checked)
                    {
                        this.UpdateTypeAndValue(SizeType.Percent, (float) this.percentNumericUpDown.Value);
                    }
                    else if (this.autoSizedRadioButton.Checked)
                    {
                        this.UpdateTypeAndValue(SizeType.AutoSize, 0f);
                    }
                    this.NormalizePercentStyles();
                    if (this.deleteList.Count > 0)
                    {
                        PropertyDescriptor member = TypeDescriptor.GetProperties(this.tlp)["Controls"];
                        if ((this.compSvc != null) && (member != null))
                        {
                            this.compSvc.OnComponentChanging(this.tlp, member);
                        }
                        IDesignerHost service = this.tlp.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if (service != null)
                        {
                            foreach (object obj2 in this.deleteList)
                            {
                                ArrayList list = new ArrayList();
                                DesignerUtils.GetAssociatedComponents((IComponent) obj2, service, list);
                                foreach (IComponent component in list)
                                {
                                    this.compSvc.OnComponentChanging(component, null);
                                }
                                service.DestroyComponent(obj2 as Component);
                            }
                        }
                        if ((this.compSvc != null) && (member != null))
                        {
                            this.compSvc.OnComponentChanged(this.tlp, member, null, null);
                        }
                    }
                    if (this.compSvc != null)
                    {
                        if (this.rowStyleProp != null)
                        {
                            this.compSvc.OnComponentChanged(this.tlp, this.rowStyleProp, null, null);
                        }
                        if (this.colStyleProp != null)
                        {
                            this.compSvc.OnComponentChanged(this.tlp, this.colStyleProp, null, null);
                        }
                    }
                    base.DialogResult = DialogResult.OK;
                }
                else
                {
                    base.DialogResult = DialogResult.Cancel;
                }
                this.tlpDesigner.ResumeEnsureAvailableStyles(true);
                this.tlp.ResumeLayout();
            }

            private void OnPercentEnter(object sender, EventArgs e)
            {
                this.isDialogDirty = true;
                this.UpdateTypeAndValue(SizeType.Percent, (float) this.percentNumericUpDown.Value);
                this.percentNumericUpDown.Enabled = true;
                this.ResetAbsolute();
            }

            private void OnRemoveButtonClick(object sender, EventArgs e)
            {
                if ((this.columnsAndRowsListView.Items.Count != 1) && (this.columnsAndRowsListView.Items.Count != this.columnsAndRowsListView.SelectedIndices.Count))
                {
                    this.isDialogDirty = true;
                    int index = this.columnsAndRowsListView.SelectedIndices[0];
                    for (int i = this.columnsAndRowsListView.SelectedIndices.Count - 1; i >= 0; i--)
                    {
                        int num3 = this.columnsAndRowsListView.SelectedIndices[i];
                        this.tlpDesigner.FixUpControlsOnDelete(this.isRowCollection, num3, this.deleteList);
                        this.tlpDesigner.DeleteRowCol(this.isRowCollection, num3);
                        if (this.isRowCollection)
                        {
                            this.columnsAndRowsListView.Items.RemoveAt(num3);
                        }
                        else
                        {
                            this.columnsAndRowsListView.Items.RemoveAt(num3);
                        }
                    }
                    if (index >= this.columnsAndRowsListView.Items.Count)
                    {
                        index--;
                    }
                    this.UpdateListViewMember();
                    this.ClearAndSetSelectionAndFocus(index);
                }
            }

            private void OnShown(object sender, EventArgs e)
            {
                this.isDialogDirty = false;
            }

            private void OnValueChanged(object sender, EventArgs e)
            {
                if ((this.absoluteNumericUpDown == sender) && this.absoluteRadioButton.Checked)
                {
                    this.isDialogDirty = true;
                    this.UpdateTypeAndValue(SizeType.Absolute, (float) this.absoluteNumericUpDown.Value);
                }
                else if ((this.percentNumericUpDown == sender) && this.percentRadioButton.Checked)
                {
                    this.isDialogDirty = true;
                    this.UpdateTypeAndValue(SizeType.Percent, (float) this.percentNumericUpDown.Value);
                }
            }

            private void ResetAbsolute()
            {
                this.absoluteNumericUpDown.ValueChanged -= new EventHandler(this.OnValueChanged);
                this.absoluteNumericUpDown.Enabled = false;
                this.absoluteNumericUpDown.Value = DesignerUtils.MINIMUMSTYLESIZE;
                this.absoluteNumericUpDown.ValueChanged += new EventHandler(this.OnValueChanged);
            }

            private void ResetAllRadioButtons()
            {
                this.absoluteRadioButton.Checked = false;
                this.ResetAbsolute();
                this.percentRadioButton.Checked = false;
                this.ResetPercent();
                this.autoSizedRadioButton.Checked = false;
            }

            private void ResetPercent()
            {
                this.percentNumericUpDown.ValueChanged -= new EventHandler(this.OnValueChanged);
                this.percentNumericUpDown.Enabled = false;
                this.percentNumericUpDown.Value = DesignerUtils.MINIMUMSTYLEPERCENT;
                this.percentNumericUpDown.ValueChanged += new EventHandler(this.OnValueChanged);
            }

            protected internal override DialogResult ShowEditorDialog(IWindowsFormsEditorService edSvc)
            {
                if (this.compSvc != null)
                {
                    if (this.rowStyleProp != null)
                    {
                        this.compSvc.OnComponentChanging(this.tlp, this.rowStyleProp);
                    }
                    if (this.colStyleProp != null)
                    {
                        this.compSvc.OnComponentChanging(this.tlp, this.colStyleProp);
                    }
                }
                int[] columnWidths = this.tlp.GetColumnWidths();
                int[] rowHeights = this.tlp.GetRowHeights();
                if (this.tlp.ColumnStyles.Count > columnWidths.Length)
                {
                    int num = this.tlp.ColumnStyles.Count - columnWidths.Length;
                    for (int i = 0; i < num; i++)
                    {
                        this.tlp.ColumnStyles.RemoveAt(this.tlp.ColumnStyles.Count - 1);
                    }
                }
                if (this.tlp.RowStyles.Count > rowHeights.Length)
                {
                    int num3 = this.tlp.RowStyles.Count - rowHeights.Length;
                    for (int j = 0; j < num3; j++)
                    {
                        this.tlp.RowStyles.RemoveAt(this.tlp.RowStyles.Count - 1);
                    }
                }
                this.columnsOrRowsComboBox.SelectedIndex = this.isRowCollection ? 1 : 0;
                this.InitListView();
                return base.ShowEditorDialog(edSvc);
            }

            private void UpdateGroupBox(SizeType type, float value)
            {
                switch (type)
                {
                    case SizeType.AutoSize:
                        this.autoSizedRadioButton.Checked = true;
                        this.ResetAbsolute();
                        this.ResetPercent();
                        return;

                    case SizeType.Absolute:
                        this.absoluteRadioButton.Checked = true;
                        this.absoluteNumericUpDown.Enabled = true;
                        try
                        {
                            this.absoluteNumericUpDown.Value = (decimal) value;
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            this.absoluteNumericUpDown.Value = DesignerUtils.MINIMUMSTYLESIZE;
                        }
                        this.ResetPercent();
                        return;

                    case SizeType.Percent:
                        this.percentRadioButton.Checked = true;
                        this.percentNumericUpDown.Enabled = true;
                        try
                        {
                            this.percentNumericUpDown.Value = (decimal) value;
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            this.percentNumericUpDown.Value = DesignerUtils.MINIMUMSTYLEPERCENT;
                        }
                        this.ResetAbsolute();
                        return;
                }
            }

            private void UpdateListViewItem(int index, string member, string type, string value)
            {
                this.columnsAndRowsListView.Items[index].SubItems[MEMBER_INDEX].Text = member;
                this.columnsAndRowsListView.Items[index].SubItems[TYPE_INDEX].Text = type;
                this.columnsAndRowsListView.Items[index].SubItems[VALUE_INDEX].Text = value;
            }

            private void UpdateListViewMember()
            {
                for (int i = 0; i < this.columnsAndRowsListView.Items.Count; i++)
                {
                    this.columnsAndRowsListView.Items[i].SubItems[MEMBER_INDEX].Text = (this.isRowCollection ? "Row" : "Column") + ((i + 1)).ToString(CultureInfo.InvariantCulture);
                }
            }

            private void UpdateTypeAndValue(SizeType type, float value)
            {
                for (int i = 0; i < this.columnsAndRowsListView.SelectedIndices.Count; i++)
                {
                    int index = this.columnsAndRowsListView.SelectedIndices[i];
                    if (this.isRowCollection)
                    {
                        this.tlp.RowStyles[index].SizeType = type;
                        this.tlp.RowStyles[index].Height = value;
                    }
                    else
                    {
                        this.tlp.ColumnStyles[index].SizeType = type;
                        this.tlp.ColumnStyles[index].Width = value;
                    }
                    this.UpdateListViewItem(index, this.columnsAndRowsListView.Items[index].SubItems[MEMBER_INDEX].Text, type.ToString(), this.FormatValueString(type, value));
                }
            }
        }
    }
}

