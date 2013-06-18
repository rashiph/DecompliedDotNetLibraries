namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Printing;
    using System.Windows.Forms;

    public sealed class WorkflowPageSetupDialog : Form
    {
        private RadioButton adjustToRadioButton;
        private NumericUpDown adjustToScaleInput;
        private Button cancelButton;
        private GroupBox centerGroup;
        private CheckBox CenterHorizontallyCheckBox;
        private TableLayoutPanel centerTableLayoutPanel;
        private CheckBox CenterVerticallyCheckBox;
        private Container components;
        private Label customFooterLabel;
        private TextBox customFooterText;
        private Label customHeaderLabel;
        private TextBox customHeaderText;
        private NumericUpDown fitToPagesTallInput;
        private NumericUpDown fitToPagesWideInput;
        private RadioButton fitToRadioButton;
        private Label fitToTallLabel;
        private Label fitToWideLabel;
        private ComboBox footerAlignmentComboBox;
        private Label footerAlignmentLabel;
        private bool footerCustom;
        private GroupBox footerGroup;
        private NumericUpDown footerMarginInput;
        private Label footerMarginLabel;
        private Label footerMarginUnitsLabel;
        private TableLayoutPanel footerTableLayoutPanel;
        private ComboBox footerTextComboBox;
        private Label footerTextLabel;
        private ComboBox headerAlignmentComboBox;
        private Label headerAlignmentLabel;
        private bool headerCustom;
        private string headerFooterCustom;
        private string headerFooterNone;
        private TabPage headerFooterTab;
        private string[] headerFooterTemplates;
        private GroupBox headerGroup;
        private NumericUpDown headerMarginInput;
        private Label headerMarginLabel;
        private Label headerMarginUnitsLabel;
        private TableLayoutPanel headerTableLayoutPanel;
        private ComboBox headerTextComboBox;
        private Label headerTextLabel;
        private PictureBox landscapePicture;
        private RadioButton landscapeRadioButton;
        private NumericUpDown marginsBottomInput;
        private Label marginsBottomLabel;
        private GroupBox marginsGroup;
        private NumericUpDown marginsLeftInput;
        private Label marginsLeftLabel;
        private NumericUpDown marginsRightInput;
        private Label marginsRightLabel;
        private TableLayoutPanel marginsTableLayoutPanel;
        private NumericUpDown marginsTopInput;
        private Label marginsTopLabel;
        private Button OKButton;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private GroupBox orientationGroup;
        private TableLayoutPanel orientationTableLayoutPanel;
        private TabPage pageSettingsTab;
        private GroupBox paperSettingsGroup;
        private ComboBox paperSizeComboBox;
        private Label paperSizeLabel;
        private ComboBox paperSourceComboBox;
        private Label paperSourceLabel;
        private TableLayoutPanel paperTableLayoutPanel;
        private PictureBox portraitPicture;
        private RadioButton portraitRadioButton;
        private WorkflowPrintDocument printDocument;
        private Button printerButton;
        private GroupBox scalingGroup;
        private Label scalingOfSizeLabel;
        private TableLayoutPanel scalingTableLayoutPanel;
        private IServiceProvider serviceProvider;
        private System.Windows.Forms.TabControl tabs;

        public WorkflowPageSetupDialog(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            this.serviceProvider = serviceProvider;
            WorkflowView service = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(WorkflowView).FullName }));
            }
            if (!(service.PrintDocument is WorkflowPrintDocument))
            {
                throw new InvalidOperationException(DR.GetString("WorkflowPrintDocumentNotFound", new object[] { typeof(WorkflowPrintDocument).Name }));
            }
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                this.InitializeComponent();
                this.printDocument = service.PrintDocument as WorkflowPrintDocument;
                this.adjustToScaleInput.Value = this.printDocument.PageSetupData.ScaleFactor;
                this.fitToPagesWideInput.Value = this.printDocument.PageSetupData.PagesWide;
                this.fitToPagesTallInput.Value = this.printDocument.PageSetupData.PagesTall;
                if (this.printDocument.PageSetupData.AdjustToScaleFactor)
                {
                    this.adjustToRadioButton.Checked = true;
                }
                else
                {
                    this.fitToRadioButton.Checked = true;
                }
                if (this.printDocument.PageSetupData.Landscape)
                {
                    this.landscapeRadioButton.Checked = true;
                }
                else
                {
                    this.portraitRadioButton.Checked = true;
                }
                this.SetMarginsToUI(this.printDocument.PageSetupData.Margins);
                this.CenterHorizontallyCheckBox.Checked = this.printDocument.PageSetupData.CenterHorizontally;
                this.CenterVerticallyCheckBox.Checked = this.printDocument.PageSetupData.CenterVertically;
                this.InitializePaperInformation();
                this.headerFooterNone = DR.GetString("HeaderFooterStringNone", new object[0]);
                this.headerFooterCustom = DR.GetString("HeaderFooterStringCustom", new object[0]);
                this.headerFooterTemplates = new string[] { DR.GetString("HeaderFooterFormat1", new object[0]), DR.GetString("HeaderFooterFormat2", new object[0]), DR.GetString("HeaderFooterFormat3", new object[0]), DR.GetString("HeaderFooterFormat4", new object[0]), DR.GetString("HeaderFooterFormat5", new object[0]), DR.GetString("HeaderFooterFormat6", new object[0]), DR.GetString("HeaderFooterFormat7", new object[0]), DR.GetString("HeaderFooterFormat8", new object[0]), DR.GetString("HeaderFooterFormat9", new object[0]) };
                this.headerTextComboBox.Items.Add(this.headerFooterNone);
                this.headerTextComboBox.Items.AddRange(this.headerFooterTemplates);
                this.headerTextComboBox.Items.Add(this.headerFooterCustom);
                this.headerTextComboBox.SelectedIndex = 0;
                string headerTemplate = this.printDocument.PageSetupData.HeaderTemplate;
                this.headerCustom = this.printDocument.PageSetupData.HeaderCustom;
                if (headerTemplate.Length == 0)
                {
                    this.headerTextComboBox.SelectedIndex = 0;
                }
                else
                {
                    int index = this.headerTextComboBox.Items.IndexOf(headerTemplate);
                    if ((-1 == index) || this.headerCustom)
                    {
                        this.headerTextComboBox.SelectedIndex = this.headerTextComboBox.Items.IndexOf(this.headerFooterCustom);
                        this.customHeaderText.Text = headerTemplate;
                    }
                    else
                    {
                        this.headerTextComboBox.SelectedIndex = index;
                    }
                }
                this.headerAlignmentComboBox.Items.AddRange(new object[] { HorizontalAlignment.Left, HorizontalAlignment.Center, HorizontalAlignment.Right });
                if (this.headerAlignmentComboBox.Items.IndexOf(this.printDocument.PageSetupData.HeaderAlignment) != -1)
                {
                    this.headerAlignmentComboBox.SelectedItem = this.printDocument.PageSetupData.HeaderAlignment;
                }
                else
                {
                    this.headerAlignmentComboBox.SelectedItem = HorizontalAlignment.Center;
                }
                this.headerMarginInput.Value = this.PrinterUnitToUIUnit(this.printDocument.PageSetupData.HeaderMargin);
                this.footerTextComboBox.Items.Add(this.headerFooterNone);
                this.footerTextComboBox.SelectedIndex = 0;
                this.footerTextComboBox.Items.AddRange(this.headerFooterTemplates);
                this.footerTextComboBox.Items.Add(this.headerFooterCustom);
                string footerTemplate = this.printDocument.PageSetupData.FooterTemplate;
                this.footerCustom = this.printDocument.PageSetupData.FooterCustom;
                if (footerTemplate.Length == 0)
                {
                    this.footerTextComboBox.SelectedIndex = 0;
                }
                else
                {
                    int num2 = this.footerTextComboBox.Items.IndexOf(footerTemplate);
                    if ((-1 == num2) || this.footerCustom)
                    {
                        this.footerTextComboBox.SelectedIndex = this.footerTextComboBox.Items.IndexOf(this.headerFooterCustom);
                        this.customFooterText.Text = footerTemplate;
                    }
                    else
                    {
                        this.footerTextComboBox.SelectedIndex = num2;
                    }
                }
                this.footerAlignmentComboBox.Items.AddRange(new object[] { HorizontalAlignment.Left, HorizontalAlignment.Center, HorizontalAlignment.Right });
                if (this.footerAlignmentComboBox.Items.IndexOf(this.printDocument.PageSetupData.FooterAlignment) != -1)
                {
                    this.footerAlignmentComboBox.SelectedItem = this.printDocument.PageSetupData.FooterAlignment;
                }
                else
                {
                    this.footerAlignmentComboBox.SelectedItem = HorizontalAlignment.Center;
                }
                this.footerMarginInput.Value = this.PrinterUnitToUIUnit(this.printDocument.PageSetupData.FooterMargin);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void adjustToInput_ValueChanged(object sender, EventArgs e)
        {
            this.adjustToRadioButton.Checked = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void fitToInputs_ValueChanged(object sender, EventArgs e)
        {
            this.fitToRadioButton.Checked = true;
        }

        private void footerMarginInput_Validating(object sender, CancelEventArgs e)
        {
        }

        private void footerTextComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.footerCustom = this.footerTextComboBox.Text.Equals(this.headerFooterCustom);
            this.customFooterText.Enabled = this.footerCustom;
            if (!this.footerCustom)
            {
                this.customFooterText.Text = this.footerTextComboBox.Text;
            }
        }

        private void GetHelp()
        {
            DesignerHelpers.ShowHelpFromKeyword(this.serviceProvider, typeof(WorkflowPageSetupDialog).FullName + ".UI");
        }

        private Margins GetMarginsFromUI()
        {
            return new Margins(this.UIUnitToPrinterUnit(this.marginsLeftInput.Value), this.UIUnitToPrinterUnit(this.marginsRightInput.Value), this.UIUnitToPrinterUnit(this.marginsTopInput.Value), this.UIUnitToPrinterUnit(this.marginsBottomInput.Value));
        }

        private void headerMarginInput_Validating(object sender, CancelEventArgs e)
        {
        }

        private void headerTextComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.headerCustom = this.headerTextComboBox.Text.Equals(this.headerFooterCustom);
            this.customHeaderText.Enabled = this.headerCustom;
            if (!this.headerCustom)
            {
                this.customHeaderText.Text = this.headerTextComboBox.Text;
            }
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(WorkflowPageSetupDialog));
            this.tabs = new System.Windows.Forms.TabControl();
            this.pageSettingsTab = new TabPage();
            this.centerGroup = new GroupBox();
            this.centerTableLayoutPanel = new TableLayoutPanel();
            this.CenterVerticallyCheckBox = new CheckBox();
            this.CenterHorizontallyCheckBox = new CheckBox();
            this.marginsGroup = new GroupBox();
            this.marginsTableLayoutPanel = new TableLayoutPanel();
            this.marginsRightInput = new NumericUpDown();
            this.marginsBottomInput = new NumericUpDown();
            this.marginsTopLabel = new Label();
            this.marginsLeftLabel = new Label();
            this.marginsRightLabel = new Label();
            this.marginsBottomLabel = new Label();
            this.marginsTopInput = new NumericUpDown();
            this.marginsLeftInput = new NumericUpDown();
            this.scalingGroup = new GroupBox();
            this.scalingTableLayoutPanel = new TableLayoutPanel();
            this.fitToTallLabel = new Label();
            this.scalingOfSizeLabel = new Label();
            this.fitToWideLabel = new Label();
            this.adjustToRadioButton = new RadioButton();
            this.fitToPagesTallInput = new NumericUpDown();
            this.fitToPagesWideInput = new NumericUpDown();
            this.adjustToScaleInput = new NumericUpDown();
            this.fitToRadioButton = new RadioButton();
            this.orientationGroup = new GroupBox();
            this.orientationTableLayoutPanel = new TableLayoutPanel();
            this.landscapeRadioButton = new RadioButton();
            this.landscapePicture = new PictureBox();
            this.portraitRadioButton = new RadioButton();
            this.portraitPicture = new PictureBox();
            this.paperSettingsGroup = new GroupBox();
            this.paperTableLayoutPanel = new TableLayoutPanel();
            this.paperSourceComboBox = new ComboBox();
            this.paperSizeComboBox = new ComboBox();
            this.paperSizeLabel = new Label();
            this.paperSourceLabel = new Label();
            this.headerFooterTab = new TabPage();
            this.footerGroup = new GroupBox();
            this.footerTableLayoutPanel = new TableLayoutPanel();
            this.footerTextLabel = new Label();
            this.footerAlignmentLabel = new Label();
            this.footerMarginUnitsLabel = new Label();
            this.footerMarginLabel = new Label();
            this.footerMarginInput = new NumericUpDown();
            this.footerTextComboBox = new ComboBox();
            this.footerAlignmentComboBox = new ComboBox();
            this.customFooterText = new TextBox();
            this.customFooterLabel = new Label();
            this.headerGroup = new GroupBox();
            this.headerTableLayoutPanel = new TableLayoutPanel();
            this.headerTextLabel = new Label();
            this.headerAlignmentLabel = new Label();
            this.headerMarginUnitsLabel = new Label();
            this.headerMarginLabel = new Label();
            this.headerMarginInput = new NumericUpDown();
            this.headerTextComboBox = new ComboBox();
            this.headerAlignmentComboBox = new ComboBox();
            this.customHeaderText = new TextBox();
            this.customHeaderLabel = new Label();
            this.OKButton = new Button();
            this.cancelButton = new Button();
            this.printerButton = new Button();
            this.okCancelTableLayoutPanel = new TableLayoutPanel();
            this.tabs.SuspendLayout();
            this.pageSettingsTab.SuspendLayout();
            this.centerGroup.SuspendLayout();
            this.centerTableLayoutPanel.SuspendLayout();
            this.marginsGroup.SuspendLayout();
            this.marginsTableLayoutPanel.SuspendLayout();
            this.marginsRightInput.BeginInit();
            this.marginsBottomInput.BeginInit();
            this.marginsTopInput.BeginInit();
            this.marginsLeftInput.BeginInit();
            this.scalingGroup.SuspendLayout();
            this.scalingTableLayoutPanel.SuspendLayout();
            this.fitToPagesTallInput.BeginInit();
            this.fitToPagesWideInput.BeginInit();
            this.adjustToScaleInput.BeginInit();
            this.orientationGroup.SuspendLayout();
            this.orientationTableLayoutPanel.SuspendLayout();
            ((ISupportInitialize) this.landscapePicture).BeginInit();
            ((ISupportInitialize) this.portraitPicture).BeginInit();
            this.paperSettingsGroup.SuspendLayout();
            this.paperTableLayoutPanel.SuspendLayout();
            this.headerFooterTab.SuspendLayout();
            this.footerGroup.SuspendLayout();
            this.footerTableLayoutPanel.SuspendLayout();
            this.footerMarginInput.BeginInit();
            this.headerGroup.SuspendLayout();
            this.headerTableLayoutPanel.SuspendLayout();
            this.headerMarginInput.BeginInit();
            this.okCancelTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.tabs, "tabs");
            this.tabs.Controls.Add(this.pageSettingsTab);
            this.tabs.Controls.Add(this.headerFooterTab);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.pageSettingsTab.Controls.Add(this.centerGroup);
            this.pageSettingsTab.Controls.Add(this.marginsGroup);
            this.pageSettingsTab.Controls.Add(this.scalingGroup);
            this.pageSettingsTab.Controls.Add(this.orientationGroup);
            this.pageSettingsTab.Controls.Add(this.paperSettingsGroup);
            manager.ApplyResources(this.pageSettingsTab, "pageSettingsTab");
            this.pageSettingsTab.Name = "pageSettingsTab";
            manager.ApplyResources(this.centerGroup, "centerGroup");
            this.centerGroup.Controls.Add(this.centerTableLayoutPanel);
            this.centerGroup.Name = "centerGroup";
            this.centerGroup.TabStop = false;
            manager.ApplyResources(this.centerTableLayoutPanel, "centerTableLayoutPanel");
            this.centerTableLayoutPanel.Controls.Add(this.CenterVerticallyCheckBox, 1, 0);
            this.centerTableLayoutPanel.Controls.Add(this.CenterHorizontallyCheckBox, 0, 0);
            this.centerTableLayoutPanel.Name = "centerTableLayoutPanel";
            manager.ApplyResources(this.CenterVerticallyCheckBox, "CenterVerticallyCheckBox");
            this.CenterVerticallyCheckBox.Name = "CenterVerticallyCheckBox";
            manager.ApplyResources(this.CenterHorizontallyCheckBox, "CenterHorizontallyCheckBox");
            this.CenterHorizontallyCheckBox.Name = "CenterHorizontallyCheckBox";
            manager.ApplyResources(this.marginsGroup, "marginsGroup");
            this.marginsGroup.Controls.Add(this.marginsTableLayoutPanel);
            this.marginsGroup.Name = "marginsGroup";
            this.marginsGroup.TabStop = false;
            manager.ApplyResources(this.marginsTableLayoutPanel, "marginsTableLayoutPanel");
            this.marginsTableLayoutPanel.Controls.Add(this.marginsRightInput, 3, 1);
            this.marginsTableLayoutPanel.Controls.Add(this.marginsBottomInput, 3, 0);
            this.marginsTableLayoutPanel.Controls.Add(this.marginsTopLabel, 0, 0);
            this.marginsTableLayoutPanel.Controls.Add(this.marginsLeftLabel, 0, 1);
            this.marginsTableLayoutPanel.Controls.Add(this.marginsRightLabel, 2, 1);
            this.marginsTableLayoutPanel.Controls.Add(this.marginsBottomLabel, 2, 0);
            this.marginsTableLayoutPanel.Controls.Add(this.marginsTopInput, 1, 0);
            this.marginsTableLayoutPanel.Controls.Add(this.marginsLeftInput, 1, 1);
            this.marginsTableLayoutPanel.Name = "marginsTableLayoutPanel";
            manager.ApplyResources(this.marginsRightInput, "marginsRightInput");
            this.marginsRightInput.DecimalPlaces = 2;
            int[] bits = new int[4];
            bits[0] = 1;
            bits[3] = 0x20000;
            this.marginsRightInput.Increment = new decimal(bits);
            this.marginsRightInput.Name = "marginsRightInput";
            int[] numArray2 = new int[4];
            numArray2[0] = 100;
            numArray2[3] = 0x20000;
            this.marginsRightInput.Value = new decimal(numArray2);
            this.marginsRightInput.Validating += new CancelEventHandler(this.Margins_Validating);
            manager.ApplyResources(this.marginsBottomInput, "marginsBottomInput");
            this.marginsBottomInput.DecimalPlaces = 2;
            int[] numArray3 = new int[4];
            numArray3[0] = 1;
            numArray3[3] = 0x20000;
            this.marginsBottomInput.Increment = new decimal(numArray3);
            this.marginsBottomInput.Name = "marginsBottomInput";
            int[] numArray4 = new int[4];
            numArray4[0] = 100;
            numArray4[3] = 0x20000;
            this.marginsBottomInput.Value = new decimal(numArray4);
            this.marginsBottomInput.Validating += new CancelEventHandler(this.Margins_Validating);
            manager.ApplyResources(this.marginsTopLabel, "marginsTopLabel");
            this.marginsTopLabel.Name = "marginsTopLabel";
            manager.ApplyResources(this.marginsLeftLabel, "marginsLeftLabel");
            this.marginsLeftLabel.Name = "marginsLeftLabel";
            manager.ApplyResources(this.marginsRightLabel, "marginsRightLabel");
            this.marginsRightLabel.Name = "marginsRightLabel";
            manager.ApplyResources(this.marginsBottomLabel, "marginsBottomLabel");
            this.marginsBottomLabel.Name = "marginsBottomLabel";
            manager.ApplyResources(this.marginsTopInput, "marginsTopInput");
            this.marginsTopInput.DecimalPlaces = 2;
            int[] numArray5 = new int[4];
            numArray5[0] = 1;
            numArray5[3] = 0x20000;
            this.marginsTopInput.Increment = new decimal(numArray5);
            this.marginsTopInput.Name = "marginsTopInput";
            int[] numArray6 = new int[4];
            numArray6[0] = 100;
            numArray6[3] = 0x20000;
            this.marginsTopInput.Value = new decimal(numArray6);
            this.marginsTopInput.Validating += new CancelEventHandler(this.Margins_Validating);
            manager.ApplyResources(this.marginsLeftInput, "marginsLeftInput");
            this.marginsLeftInput.DecimalPlaces = 2;
            int[] numArray7 = new int[4];
            numArray7[0] = 1;
            numArray7[3] = 0x20000;
            this.marginsLeftInput.Increment = new decimal(numArray7);
            this.marginsLeftInput.Name = "marginsLeftInput";
            int[] numArray8 = new int[4];
            numArray8[0] = 100;
            numArray8[3] = 0x20000;
            this.marginsLeftInput.Value = new decimal(numArray8);
            this.marginsLeftInput.Validating += new CancelEventHandler(this.Margins_Validating);
            manager.ApplyResources(this.scalingGroup, "scalingGroup");
            this.scalingGroup.Controls.Add(this.scalingTableLayoutPanel);
            this.scalingGroup.Name = "scalingGroup";
            this.scalingGroup.TabStop = false;
            manager.ApplyResources(this.scalingTableLayoutPanel, "scalingTableLayoutPanel");
            this.scalingTableLayoutPanel.Controls.Add(this.fitToTallLabel, 2, 2);
            this.scalingTableLayoutPanel.Controls.Add(this.scalingOfSizeLabel, 2, 0);
            this.scalingTableLayoutPanel.Controls.Add(this.fitToWideLabel, 2, 1);
            this.scalingTableLayoutPanel.Controls.Add(this.adjustToRadioButton, 0, 0);
            this.scalingTableLayoutPanel.Controls.Add(this.fitToPagesTallInput, 1, 2);
            this.scalingTableLayoutPanel.Controls.Add(this.fitToPagesWideInput, 1, 1);
            this.scalingTableLayoutPanel.Controls.Add(this.adjustToScaleInput, 1, 0);
            this.scalingTableLayoutPanel.Controls.Add(this.fitToRadioButton, 0, 1);
            this.scalingTableLayoutPanel.Name = "scalingTableLayoutPanel";
            manager.ApplyResources(this.fitToTallLabel, "fitToTallLabel");
            this.fitToTallLabel.Name = "fitToTallLabel";
            manager.ApplyResources(this.scalingOfSizeLabel, "scalingOfSizeLabel");
            this.scalingOfSizeLabel.Name = "scalingOfSizeLabel";
            manager.ApplyResources(this.fitToWideLabel, "fitToWideLabel");
            this.fitToWideLabel.Name = "fitToWideLabel";
            manager.ApplyResources(this.adjustToRadioButton, "adjustToRadioButton");
            this.adjustToRadioButton.Name = "adjustToRadioButton";
            manager.ApplyResources(this.fitToPagesTallInput, "fitToPagesTallInput");
            int[] numArray9 = new int[4];
            numArray9[0] = 20;
            this.fitToPagesTallInput.Maximum = new decimal(numArray9);
            int[] numArray10 = new int[4];
            numArray10[0] = 1;
            this.fitToPagesTallInput.Minimum = new decimal(numArray10);
            this.fitToPagesTallInput.Name = "fitToPagesTallInput";
            int[] numArray11 = new int[4];
            numArray11[0] = 1;
            this.fitToPagesTallInput.Value = new decimal(numArray11);
            this.fitToPagesTallInput.ValueChanged += new EventHandler(this.fitToInputs_ValueChanged);
            manager.ApplyResources(this.fitToPagesWideInput, "fitToPagesWideInput");
            int[] numArray12 = new int[4];
            numArray12[0] = 20;
            this.fitToPagesWideInput.Maximum = new decimal(numArray12);
            int[] numArray13 = new int[4];
            numArray13[0] = 1;
            this.fitToPagesWideInput.Minimum = new decimal(numArray13);
            this.fitToPagesWideInput.Name = "fitToPagesWideInput";
            int[] numArray14 = new int[4];
            numArray14[0] = 1;
            this.fitToPagesWideInput.Value = new decimal(numArray14);
            this.fitToPagesWideInput.ValueChanged += new EventHandler(this.fitToInputs_ValueChanged);
            manager.ApplyResources(this.adjustToScaleInput, "adjustToScaleInput");
            int[] numArray15 = new int[4];
            numArray15[0] = 400;
            this.adjustToScaleInput.Maximum = new decimal(numArray15);
            int[] numArray16 = new int[4];
            numArray16[0] = 10;
            this.adjustToScaleInput.Minimum = new decimal(numArray16);
            this.adjustToScaleInput.Name = "adjustToScaleInput";
            int[] numArray17 = new int[4];
            numArray17[0] = 100;
            this.adjustToScaleInput.Value = new decimal(numArray17);
            this.adjustToScaleInput.ValueChanged += new EventHandler(this.adjustToInput_ValueChanged);
            manager.ApplyResources(this.fitToRadioButton, "fitToRadioButton");
            this.fitToRadioButton.Name = "fitToRadioButton";
            manager.ApplyResources(this.orientationGroup, "orientationGroup");
            this.orientationGroup.Controls.Add(this.orientationTableLayoutPanel);
            this.orientationGroup.Name = "orientationGroup";
            this.orientationGroup.TabStop = false;
            manager.ApplyResources(this.orientationTableLayoutPanel, "orientationTableLayoutPanel");
            this.orientationTableLayoutPanel.Controls.Add(this.landscapeRadioButton, 3, 0);
            this.orientationTableLayoutPanel.Controls.Add(this.landscapePicture, 2, 0);
            this.orientationTableLayoutPanel.Controls.Add(this.portraitRadioButton, 1, 0);
            this.orientationTableLayoutPanel.Controls.Add(this.portraitPicture, 0, 0);
            this.orientationTableLayoutPanel.Name = "orientationTableLayoutPanel";
            manager.ApplyResources(this.landscapeRadioButton, "landscapeRadioButton");
            this.landscapeRadioButton.Name = "landscapeRadioButton";
            this.landscapeRadioButton.CheckedChanged += new EventHandler(this.landscapeRadioButton_CheckedChanged);
            manager.ApplyResources(this.landscapePicture, "landscapePicture");
            this.landscapePicture.Name = "landscapePicture";
            this.landscapePicture.TabStop = false;
            manager.ApplyResources(this.portraitRadioButton, "portraitRadioButton");
            this.portraitRadioButton.Name = "portraitRadioButton";
            this.portraitRadioButton.CheckedChanged += new EventHandler(this.portraitRadioButton_CheckedChanged);
            manager.ApplyResources(this.portraitPicture, "portraitPicture");
            this.portraitPicture.Name = "portraitPicture";
            this.portraitPicture.TabStop = false;
            manager.ApplyResources(this.paperSettingsGroup, "paperSettingsGroup");
            this.paperSettingsGroup.Controls.Add(this.paperTableLayoutPanel);
            this.paperSettingsGroup.Name = "paperSettingsGroup";
            this.paperSettingsGroup.TabStop = false;
            manager.ApplyResources(this.paperTableLayoutPanel, "paperTableLayoutPanel");
            this.paperTableLayoutPanel.Controls.Add(this.paperSourceComboBox, 1, 1);
            this.paperTableLayoutPanel.Controls.Add(this.paperSizeComboBox, 1, 0);
            this.paperTableLayoutPanel.Controls.Add(this.paperSizeLabel, 0, 0);
            this.paperTableLayoutPanel.Controls.Add(this.paperSourceLabel, 0, 1);
            this.paperTableLayoutPanel.Name = "paperTableLayoutPanel";
            manager.ApplyResources(this.paperSourceComboBox, "paperSourceComboBox");
            this.paperSourceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.paperSourceComboBox.FormattingEnabled = true;
            this.paperSourceComboBox.Name = "paperSourceComboBox";
            manager.ApplyResources(this.paperSizeComboBox, "paperSizeComboBox");
            this.paperSizeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.paperSizeComboBox.FormattingEnabled = true;
            this.paperSizeComboBox.Name = "paperSizeComboBox";
            this.paperSizeComboBox.SelectedIndexChanged += new EventHandler(this.paperSizeComboBox_SelectedIndexChanged);
            manager.ApplyResources(this.paperSizeLabel, "paperSizeLabel");
            this.paperSizeLabel.Name = "paperSizeLabel";
            manager.ApplyResources(this.paperSourceLabel, "paperSourceLabel");
            this.paperSourceLabel.Name = "paperSourceLabel";
            this.headerFooterTab.Controls.Add(this.footerGroup);
            this.headerFooterTab.Controls.Add(this.headerGroup);
            manager.ApplyResources(this.headerFooterTab, "headerFooterTab");
            this.headerFooterTab.Name = "headerFooterTab";
            manager.ApplyResources(this.footerGroup, "footerGroup");
            this.footerGroup.Controls.Add(this.footerTableLayoutPanel);
            this.footerGroup.Controls.Add(this.customFooterText);
            this.footerGroup.Controls.Add(this.customFooterLabel);
            this.footerGroup.Name = "footerGroup";
            this.footerGroup.TabStop = false;
            manager.ApplyResources(this.footerTableLayoutPanel, "footerTableLayoutPanel");
            this.footerTableLayoutPanel.Controls.Add(this.footerTextLabel, 0, 0);
            this.footerTableLayoutPanel.Controls.Add(this.footerAlignmentLabel, 0, 1);
            this.footerTableLayoutPanel.Controls.Add(this.footerMarginUnitsLabel, 2, 2);
            this.footerTableLayoutPanel.Controls.Add(this.footerMarginLabel, 0, 2);
            this.footerTableLayoutPanel.Controls.Add(this.footerMarginInput, 1, 2);
            this.footerTableLayoutPanel.Controls.Add(this.footerTextComboBox, 1, 0);
            this.footerTableLayoutPanel.Controls.Add(this.footerAlignmentComboBox, 1, 1);
            this.footerTableLayoutPanel.Name = "footerTableLayoutPanel";
            manager.ApplyResources(this.footerTextLabel, "footerTextLabel");
            this.footerTextLabel.Name = "footerTextLabel";
            manager.ApplyResources(this.footerAlignmentLabel, "footerAlignmentLabel");
            this.footerAlignmentLabel.Name = "footerAlignmentLabel";
            manager.ApplyResources(this.footerMarginUnitsLabel, "footerMarginUnitsLabel");
            this.footerMarginUnitsLabel.Name = "footerMarginUnitsLabel";
            manager.ApplyResources(this.footerMarginLabel, "footerMarginLabel");
            this.footerMarginLabel.Name = "footerMarginLabel";
            manager.ApplyResources(this.footerMarginInput, "footerMarginInput");
            this.footerMarginInput.DecimalPlaces = 2;
            int[] numArray18 = new int[4];
            numArray18[0] = 1;
            numArray18[3] = 0x20000;
            this.footerMarginInput.Increment = new decimal(numArray18);
            this.footerMarginInput.Name = "footerMarginInput";
            int[] numArray19 = new int[4];
            numArray19[0] = 1;
            this.footerMarginInput.Value = new decimal(numArray19);
            this.footerMarginInput.Validating += new CancelEventHandler(this.footerMarginInput_Validating);
            manager.ApplyResources(this.footerTextComboBox, "footerTextComboBox");
            this.footerTableLayoutPanel.SetColumnSpan(this.footerTextComboBox, 2);
            this.footerTextComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.footerTextComboBox.FormattingEnabled = true;
            this.footerTextComboBox.Name = "footerTextComboBox";
            this.footerTextComboBox.SelectedIndexChanged += new EventHandler(this.footerTextComboBox_SelectedIndexChanged);
            manager.ApplyResources(this.footerAlignmentComboBox, "footerAlignmentComboBox");
            this.footerTableLayoutPanel.SetColumnSpan(this.footerAlignmentComboBox, 2);
            this.footerAlignmentComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.footerAlignmentComboBox.FormattingEnabled = true;
            this.footerAlignmentComboBox.Name = "footerAlignmentComboBox";
            manager.ApplyResources(this.customFooterText, "customFooterText");
            this.customFooterText.Name = "customFooterText";
            manager.ApplyResources(this.customFooterLabel, "customFooterLabel");
            this.customFooterLabel.Name = "customFooterLabel";
            manager.ApplyResources(this.headerGroup, "headerGroup");
            this.headerGroup.Controls.Add(this.headerTableLayoutPanel);
            this.headerGroup.Controls.Add(this.customHeaderText);
            this.headerGroup.Controls.Add(this.customHeaderLabel);
            this.headerGroup.Name = "headerGroup";
            this.headerGroup.TabStop = false;
            manager.ApplyResources(this.headerTableLayoutPanel, "headerTableLayoutPanel");
            this.headerTableLayoutPanel.Controls.Add(this.headerTextLabel, 0, 0);
            this.headerTableLayoutPanel.Controls.Add(this.headerAlignmentLabel, 0, 1);
            this.headerTableLayoutPanel.Controls.Add(this.headerMarginUnitsLabel, 2, 2);
            this.headerTableLayoutPanel.Controls.Add(this.headerMarginLabel, 0, 2);
            this.headerTableLayoutPanel.Controls.Add(this.headerMarginInput, 1, 2);
            this.headerTableLayoutPanel.Controls.Add(this.headerTextComboBox, 1, 0);
            this.headerTableLayoutPanel.Controls.Add(this.headerAlignmentComboBox, 1, 1);
            this.headerTableLayoutPanel.Name = "headerTableLayoutPanel";
            manager.ApplyResources(this.headerTextLabel, "headerTextLabel");
            this.headerTextLabel.Name = "headerTextLabel";
            manager.ApplyResources(this.headerAlignmentLabel, "headerAlignmentLabel");
            this.headerAlignmentLabel.Cursor = Cursors.Arrow;
            this.headerAlignmentLabel.Name = "headerAlignmentLabel";
            manager.ApplyResources(this.headerMarginUnitsLabel, "headerMarginUnitsLabel");
            this.headerMarginUnitsLabel.Name = "headerMarginUnitsLabel";
            manager.ApplyResources(this.headerMarginLabel, "headerMarginLabel");
            this.headerMarginLabel.Name = "headerMarginLabel";
            manager.ApplyResources(this.headerMarginInput, "headerMarginInput");
            this.headerMarginInput.DecimalPlaces = 2;
            int[] numArray20 = new int[4];
            numArray20[0] = 1;
            numArray20[3] = 0x20000;
            this.headerMarginInput.Increment = new decimal(numArray20);
            this.headerMarginInput.Name = "headerMarginInput";
            int[] numArray21 = new int[4];
            numArray21[0] = 1;
            this.headerMarginInput.Value = new decimal(numArray21);
            this.headerMarginInput.Validating += new CancelEventHandler(this.headerMarginInput_Validating);
            manager.ApplyResources(this.headerTextComboBox, "headerTextComboBox");
            this.headerTableLayoutPanel.SetColumnSpan(this.headerTextComboBox, 2);
            this.headerTextComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.headerTextComboBox.FormattingEnabled = true;
            this.headerTextComboBox.Name = "headerTextComboBox";
            this.headerTextComboBox.SelectedIndexChanged += new EventHandler(this.headerTextComboBox_SelectedIndexChanged);
            manager.ApplyResources(this.headerAlignmentComboBox, "headerAlignmentComboBox");
            this.headerTableLayoutPanel.SetColumnSpan(this.headerAlignmentComboBox, 2);
            this.headerAlignmentComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.headerAlignmentComboBox.FormattingEnabled = true;
            this.headerAlignmentComboBox.Name = "headerAlignmentComboBox";
            manager.ApplyResources(this.customHeaderText, "customHeaderText");
            this.customHeaderText.Name = "customHeaderText";
            manager.ApplyResources(this.customHeaderLabel, "customHeaderLabel");
            this.customHeaderLabel.Name = "customHeaderLabel";
            manager.ApplyResources(this.OKButton, "OKButton");
            this.OKButton.DialogResult = DialogResult.OK;
            this.OKButton.Name = "OKButton";
            this.OKButton.Click += new EventHandler(this.OKButton_Click);
            manager.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            manager.ApplyResources(this.printerButton, "printerButton");
            this.printerButton.Name = "printerButton";
            this.printerButton.Click += new EventHandler(this.printerButton_Click);
            manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.Controls.Add(this.OKButton, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.printerButton, 2, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            base.AcceptButton = this.OKButton;
            manager.ApplyResources(this, "$this");
            base.CancelButton = this.cancelButton;
            base.Controls.Add(this.okCancelTableLayoutPanel);
            base.Controls.Add(this.tabs);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.HelpButton = true;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "WorkflowPageSetupDialog";
            base.ShowInTaskbar = false;
            base.HelpButtonClicked += new CancelEventHandler(this.WorkflowPageSetupDialog_HelpButtonClicked);
            this.tabs.ResumeLayout(false);
            this.pageSettingsTab.ResumeLayout(false);
            this.centerGroup.ResumeLayout(false);
            this.centerTableLayoutPanel.ResumeLayout(false);
            this.centerTableLayoutPanel.PerformLayout();
            this.marginsGroup.ResumeLayout(false);
            this.marginsTableLayoutPanel.ResumeLayout(false);
            this.marginsTableLayoutPanel.PerformLayout();
            this.marginsRightInput.EndInit();
            this.marginsBottomInput.EndInit();
            this.marginsTopInput.EndInit();
            this.marginsLeftInput.EndInit();
            this.scalingGroup.ResumeLayout(false);
            this.scalingTableLayoutPanel.ResumeLayout(false);
            this.scalingTableLayoutPanel.PerformLayout();
            this.fitToPagesTallInput.EndInit();
            this.fitToPagesWideInput.EndInit();
            this.adjustToScaleInput.EndInit();
            this.orientationGroup.ResumeLayout(false);
            this.orientationTableLayoutPanel.ResumeLayout(false);
            this.orientationTableLayoutPanel.PerformLayout();
            ((ISupportInitialize) this.landscapePicture).EndInit();
            ((ISupportInitialize) this.portraitPicture).EndInit();
            this.paperSettingsGroup.ResumeLayout(false);
            this.paperTableLayoutPanel.ResumeLayout(false);
            this.paperTableLayoutPanel.PerformLayout();
            this.headerFooterTab.ResumeLayout(false);
            this.footerGroup.ResumeLayout(false);
            this.footerGroup.PerformLayout();
            this.footerTableLayoutPanel.ResumeLayout(false);
            this.footerTableLayoutPanel.PerformLayout();
            this.footerMarginInput.EndInit();
            this.headerGroup.ResumeLayout(false);
            this.headerGroup.PerformLayout();
            this.headerTableLayoutPanel.ResumeLayout(false);
            this.headerTableLayoutPanel.PerformLayout();
            this.headerMarginInput.EndInit();
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializePaperInformation()
        {
            PrinterSettings.PaperSizeCollection paperSizes = this.printDocument.PrinterSettings.PaperSizes;
            PrinterSettings.PaperSourceCollection paperSources = this.printDocument.PrinterSettings.PaperSources;
            this.paperSizeComboBox.Items.Clear();
            this.paperSizeComboBox.DisplayMember = "PaperName";
            foreach (PaperSize size in paperSizes)
            {
                if ((size.PaperName != null) && (size.PaperName.Length > 0))
                {
                    this.paperSizeComboBox.Items.Add(size);
                    if (((this.paperSizeComboBox.SelectedItem == null) && (this.printDocument.DefaultPageSettings.PaperSize.Kind == size.Kind)) && ((this.printDocument.DefaultPageSettings.PaperSize.Width == size.Width) && (this.printDocument.DefaultPageSettings.PaperSize.Height == size.Height)))
                    {
                        this.paperSizeComboBox.SelectedItem = size;
                        this.printDocument.DefaultPageSettings.PaperSize = size;
                    }
                }
            }
            if (this.paperSizeComboBox.SelectedItem == null)
            {
                PaperKind kind = this.printDocument.DefaultPageSettings.PaperSize.Kind;
                this.printDocument.DefaultPageSettings = new PageSettings(this.printDocument.PrinterSettings);
                foreach (PaperSize size2 in this.paperSizeComboBox.Items)
                {
                    if (((this.paperSizeComboBox.SelectedItem == null) && (kind == size2.Kind)) && ((this.printDocument.DefaultPageSettings.PaperSize.Width == size2.Width) && (this.printDocument.DefaultPageSettings.PaperSize.Height == size2.Height)))
                    {
                        this.paperSizeComboBox.SelectedItem = size2;
                        this.printDocument.DefaultPageSettings.PaperSize = size2;
                    }
                }
                if ((this.paperSizeComboBox.SelectedItem == null) && (this.paperSizeComboBox.Items.Count > 0))
                {
                    this.paperSizeComboBox.SelectedItem = this.paperSizeComboBox.Items[0] as PaperSize;
                    this.printDocument.DefaultPageSettings.PaperSize = this.paperSizeComboBox.SelectedItem as PaperSize;
                }
            }
            this.paperSourceComboBox.Items.Clear();
            this.paperSourceComboBox.DisplayMember = "SourceName";
            foreach (PaperSource source in paperSources)
            {
                this.paperSourceComboBox.Items.Add(source);
                if (((this.paperSourceComboBox.SelectedItem == null) && (this.printDocument.DefaultPageSettings.PaperSource.Kind == source.Kind)) && (this.printDocument.DefaultPageSettings.PaperSource.SourceName == source.SourceName))
                {
                    this.paperSourceComboBox.SelectedItem = source;
                }
            }
            if ((this.paperSourceComboBox.SelectedItem == null) && (this.paperSourceComboBox.Items.Count > 0))
            {
                this.paperSourceComboBox.SelectedItem = this.paperSourceComboBox.Items[0] as PaperSource;
                this.printDocument.DefaultPageSettings.PaperSource = this.paperSourceComboBox.SelectedItem as PaperSource;
            }
        }

        private void landscapeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.UpdateHeaderFooterMarginLimit();
        }

        private void Margins_Validating(object sender, CancelEventArgs e)
        {
            Size size;
            Margins marginsFromUI = this.GetMarginsFromUI();
            PaperSize selectedItem = this.paperSizeComboBox.SelectedItem as PaperSize;
            if (selectedItem != null)
            {
                size = new Size(selectedItem.Width, selectedItem.Height);
            }
            else
            {
                size = this.printDocument.DefaultPageSettings.Bounds.Size;
            }
            int num = marginsFromUI.Left + marginsFromUI.Right;
            int num2 = marginsFromUI.Top + marginsFromUI.Bottom;
            if ((num >= size.Width) || (num2 >= size.Height))
            {
                string message = DR.GetString("EnteredMarginsAreNotValidErrorMessage", new object[0]);
                DesignerHelpers.ShowError(this.serviceProvider, message);
                e.Cancel = true;
            }
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            Margins marginsFromUI = this.GetMarginsFromUI();
            this.printDocument.PageSetupData.AdjustToScaleFactor = this.adjustToRadioButton.Checked;
            this.printDocument.PageSetupData.ScaleFactor = (int) this.adjustToScaleInput.Value;
            this.printDocument.PageSetupData.PagesWide = (int) this.fitToPagesWideInput.Value;
            this.printDocument.PageSetupData.PagesTall = (int) this.fitToPagesTallInput.Value;
            this.printDocument.PageSetupData.Landscape = this.landscapeRadioButton.Checked;
            this.printDocument.PageSetupData.Margins = marginsFromUI;
            this.printDocument.PageSetupData.CenterHorizontally = this.CenterHorizontallyCheckBox.Checked;
            this.printDocument.PageSetupData.CenterVertically = this.CenterVerticallyCheckBox.Checked;
            if (this.headerTextComboBox.SelectedIndex == 0)
            {
                this.printDocument.PageSetupData.HeaderTemplate = string.Empty;
            }
            else if (!this.headerTextComboBox.Text.Equals(this.headerFooterCustom))
            {
                this.printDocument.PageSetupData.HeaderTemplate = this.headerTextComboBox.Text;
            }
            else
            {
                this.printDocument.PageSetupData.HeaderTemplate = this.customHeaderText.Text;
            }
            this.printDocument.PageSetupData.HeaderCustom = this.headerCustom;
            this.printDocument.PageSetupData.HeaderAlignment = (HorizontalAlignment) this.headerAlignmentComboBox.SelectedItem;
            this.printDocument.PageSetupData.HeaderMargin = this.UIUnitToPrinterUnit(this.headerMarginInput.Value);
            if (this.footerTextComboBox.SelectedIndex == 0)
            {
                this.printDocument.PageSetupData.FooterTemplate = string.Empty;
            }
            else if (!this.footerTextComboBox.Text.Equals(this.headerFooterCustom))
            {
                this.printDocument.PageSetupData.FooterTemplate = this.footerTextComboBox.Text;
            }
            else
            {
                this.printDocument.PageSetupData.FooterTemplate = this.customFooterText.Text;
            }
            this.printDocument.PageSetupData.FooterCustom = this.footerCustom;
            this.printDocument.PageSetupData.FooterAlignment = (HorizontalAlignment) this.footerAlignmentComboBox.SelectedItem;
            this.printDocument.PageSetupData.FooterMargin = this.UIUnitToPrinterUnit(this.footerMarginInput.Value);
            if (PrinterSettings.InstalledPrinters.Count > 0)
            {
                if (this.paperSizeComboBox.SelectedItem != null)
                {
                    this.printDocument.DefaultPageSettings.PaperSize = (PaperSize) this.paperSizeComboBox.SelectedItem;
                }
                if (this.paperSourceComboBox.SelectedItem != null)
                {
                    this.printDocument.DefaultPageSettings.PaperSource = (PaperSource) this.paperSourceComboBox.SelectedItem;
                }
                this.printDocument.DefaultPageSettings.Landscape = this.printDocument.PageSetupData.Landscape;
                this.printDocument.DefaultPageSettings.Margins = marginsFromUI;
                this.printDocument.PrinterSettings.DefaultPageSettings.PaperSize = this.printDocument.DefaultPageSettings.PaperSize;
                this.printDocument.PrinterSettings.DefaultPageSettings.PaperSource = this.printDocument.DefaultPageSettings.PaperSource;
                this.printDocument.PrinterSettings.DefaultPageSettings.Landscape = this.printDocument.PageSetupData.Landscape;
                this.printDocument.PrinterSettings.DefaultPageSettings.Margins = marginsFromUI;
            }
            this.printDocument.PageSetupData.StorePropertiesToRegistry();
            base.DialogResult = DialogResult.OK;
        }

        protected override void OnHelpRequested(HelpEventArgs hlpevent)
        {
            hlpevent.Handled = true;
            this.GetHelp();
        }

        private void paperSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateHeaderFooterMarginLimit();
        }

        private void portraitRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.UpdateHeaderFooterMarginLimit();
        }

        private void printerButton_Click(object sender, EventArgs e)
        {
            PrintDialog dialog = new PrintDialog {
                AllowPrintToFile = false,
                Document = this.printDocument
            };
            try
            {
                if (DialogResult.OK == dialog.ShowDialog())
                {
                    this.printDocument.PrinterSettings = dialog.PrinterSettings;
                    this.printDocument.DefaultPageSettings = dialog.Document.DefaultPageSettings;
                    if (this.printDocument.DefaultPageSettings.Landscape)
                    {
                        this.landscapeRadioButton.Checked = true;
                    }
                    else
                    {
                        this.portraitRadioButton.Checked = true;
                    }
                    this.InitializePaperInformation();
                    this.printDocument.Print();
                }
            }
            catch (Exception exception)
            {
                string message = DR.GetString("SelectedPrinterIsInvalidErrorMessage", new object[0]) + "\n" + exception.Message;
                DesignerHelpers.ShowError(this.serviceProvider, message);
            }
        }

        private decimal PrinterUnitToUIUnit(int printerValue)
        {
            return Convert.ToDecimal((double) (((double) printerValue) / 100.0));
        }

        private void SetMarginsToUI(Margins margins)
        {
            this.marginsLeftInput.Value = this.PrinterUnitToUIUnit(margins.Left);
            this.marginsRightInput.Value = this.PrinterUnitToUIUnit(margins.Right);
            this.marginsTopInput.Value = this.PrinterUnitToUIUnit(margins.Top);
            this.marginsBottomInput.Value = this.PrinterUnitToUIUnit(margins.Bottom);
        }

        private int UIUnitToPrinterUnit(decimal uiValue)
        {
            return Convert.ToInt32((double) (((double) uiValue) * 100.0));
        }

        private void UpdateHeaderFooterMarginLimit()
        {
            PaperSize selectedItem = this.paperSizeComboBox.SelectedItem as PaperSize;
            if (selectedItem != null)
            {
                this.footerMarginInput.Maximum = this.headerMarginInput.Maximum = this.PrinterUnitToUIUnit(this.landscapeRadioButton.Checked ? selectedItem.Width : selectedItem.Height);
            }
        }

        private void WorkflowPageSetupDialog_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.GetHelp();
        }
    }
}

