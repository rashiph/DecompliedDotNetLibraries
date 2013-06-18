namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal class DataGridViewCellStyleBuilder : Form
    {
        private Button cancelButton;
        private DataGridViewCellStyle cellStyle;
        private PropertyGrid cellStyleProperties;
        private IComponent comp;
        private Container components;
        private ITypeDescriptorContext context;
        private IHelpService helpService;
        private Label label1;
        private DataGridView listenerDataGridView;
        private Label normalLabel;
        private Button okButton;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private TableLayoutPanel overarchingTableLayoutPanel;
        private GroupBox previewGroupBox;
        private DataGridView sampleDataGridView;
        private DataGridView sampleDataGridViewSelected;
        private TableLayoutPanel sampleViewGridsTableLayoutPanel;
        private TableLayoutPanel sampleViewTableLayoutPanel;
        private Label selectedLabel;
        private IServiceProvider serviceProvider;

        public DataGridViewCellStyleBuilder(IServiceProvider serviceProvider, IComponent comp)
        {
            this.InitializeComponent();
            this.InitializeGrids();
            this.listenerDataGridView = new DataGridView();
            this.serviceProvider = serviceProvider;
            this.comp = comp;
            if (this.serviceProvider != null)
            {
                this.helpService = (IHelpService) serviceProvider.GetService(typeof(IHelpService));
            }
            this.cellStyleProperties.Site = new DataGridViewComponentPropertyGridSite(serviceProvider, comp);
        }

        private void DataGridViewCellStyleBuilder_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.DataGridViewCellStyleBuilder_HelpRequestHandled();
        }

        private void DataGridViewCellStyleBuilder_HelpRequested(object sender, HelpEventArgs e)
        {
            e.Handled = true;
            this.DataGridViewCellStyleBuilder_HelpRequestHandled();
        }

        private void DataGridViewCellStyleBuilder_HelpRequestHandled()
        {
            IHelpService service = this.context.GetService(typeof(IHelpService)) as IHelpService;
            if (service != null)
            {
                service.ShowHelpFromKeyword("vs.CellStyleDialog");
            }
        }

        private void DataGridViewCellStyleBuilder_Load(object sender, EventArgs e)
        {
            this.sampleDataGridView.ClearSelection();
            this.sampleDataGridView.Rows[0].Height = this.sampleDataGridView.Height;
            this.sampleDataGridView.Columns[0].Width = this.sampleDataGridView.Width;
            this.sampleDataGridViewSelected.Rows[0].Height = this.sampleDataGridViewSelected.Height;
            this.sampleDataGridViewSelected.Columns[0].Width = this.sampleDataGridViewSelected.Width;
            this.sampleDataGridView.Layout += new LayoutEventHandler(this.sampleDataGridView_Layout);
            this.sampleDataGridViewSelected.Layout += new LayoutEventHandler(this.sampleDataGridView_Layout);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(DataGridViewCellStyleBuilder));
            this.cellStyleProperties = new PropertyGrid();
            this.sampleViewTableLayoutPanel = new TableLayoutPanel();
            this.sampleViewGridsTableLayoutPanel = new TableLayoutPanel();
            this.normalLabel = new Label();
            this.sampleDataGridView = new DataGridView();
            this.selectedLabel = new Label();
            this.sampleDataGridViewSelected = new DataGridView();
            this.label1 = new Label();
            this.okButton = new Button();
            this.cancelButton = new Button();
            this.okCancelTableLayoutPanel = new TableLayoutPanel();
            this.previewGroupBox = new GroupBox();
            this.overarchingTableLayoutPanel = new TableLayoutPanel();
            this.sampleViewTableLayoutPanel.SuspendLayout();
            this.sampleViewGridsTableLayoutPanel.SuspendLayout();
            ((ISupportInitialize) this.sampleDataGridView).BeginInit();
            ((ISupportInitialize) this.sampleDataGridViewSelected).BeginInit();
            this.okCancelTableLayoutPanel.SuspendLayout();
            this.previewGroupBox.SuspendLayout();
            this.overarchingTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.cellStyleProperties, "cellStyleProperties");
            this.cellStyleProperties.LineColor = SystemColors.ScrollBar;
            this.cellStyleProperties.Margin = new Padding(0, 0, 0, 3);
            this.cellStyleProperties.Name = "cellStyleProperties";
            this.cellStyleProperties.ToolbarVisible = false;
            manager.ApplyResources(this.sampleViewTableLayoutPanel, "sampleViewTableLayoutPanel");
            this.sampleViewTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 423f));
            this.sampleViewTableLayoutPanel.Controls.Add(this.sampleViewGridsTableLayoutPanel, 0, 1);
            this.sampleViewTableLayoutPanel.Controls.Add(this.label1, 0, 0);
            this.sampleViewTableLayoutPanel.Name = "sampleViewTableLayoutPanel";
            this.sampleViewTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.sampleViewTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.sampleViewGridsTableLayoutPanel, "sampleViewGridsTableLayoutPanel");
            this.sampleViewGridsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10f));
            this.sampleViewGridsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            this.sampleViewGridsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
            this.sampleViewGridsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            this.sampleViewGridsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10f));
            this.sampleViewGridsTableLayoutPanel.Controls.Add(this.normalLabel, 1, 0);
            this.sampleViewGridsTableLayoutPanel.Controls.Add(this.sampleDataGridView, 1, 1);
            this.sampleViewGridsTableLayoutPanel.Controls.Add(this.selectedLabel, 3, 0);
            this.sampleViewGridsTableLayoutPanel.Controls.Add(this.sampleDataGridViewSelected, 3, 1);
            this.sampleViewGridsTableLayoutPanel.Margin = new Padding(0, 3, 0, 0);
            this.sampleViewGridsTableLayoutPanel.Name = "sampleViewGridsTableLayoutPanel";
            this.sampleViewGridsTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.sampleViewGridsTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.normalLabel, "normalLabel");
            this.normalLabel.Margin = new Padding(0);
            this.normalLabel.Name = "normalLabel";
            this.sampleDataGridView.AllowUserToAddRows = false;
            manager.ApplyResources(this.sampleDataGridView, "sampleDataGridView");
            this.sampleDataGridView.ColumnHeadersVisible = false;
            this.sampleDataGridView.Margin = new Padding(0);
            this.sampleDataGridView.Name = "sampleDataGridView";
            this.sampleDataGridView.ReadOnly = true;
            this.sampleDataGridView.RowHeadersVisible = false;
            this.sampleDataGridView.CellStateChanged += new DataGridViewCellStateChangedEventHandler(this.sampleDataGridView_CellStateChanged);
            manager.ApplyResources(this.selectedLabel, "selectedLabel");
            this.selectedLabel.Margin = new Padding(0);
            this.selectedLabel.Name = "selectedLabel";
            this.sampleDataGridViewSelected.AllowUserToAddRows = false;
            manager.ApplyResources(this.sampleDataGridViewSelected, "sampleDataGridViewSelected");
            this.sampleDataGridViewSelected.ColumnHeadersVisible = false;
            this.sampleDataGridViewSelected.Margin = new Padding(0);
            this.sampleDataGridViewSelected.Name = "sampleDataGridViewSelected";
            this.sampleDataGridViewSelected.ReadOnly = true;
            this.sampleDataGridViewSelected.RowHeadersVisible = false;
            manager.ApplyResources(this.label1, "label1");
            this.label1.Margin = new Padding(0, 0, 0, 3);
            this.label1.Name = "label1";
            manager.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = DialogResult.OK;
            this.okButton.Margin = new Padding(0, 0, 3, 0);
            this.okButton.Name = "okButton";
            manager.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Margin = new Padding(3, 0, 0, 0);
            this.cancelButton.Name = "cancelButton";
            manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okCancelTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
            this.okCancelTableLayoutPanel.Margin = new Padding(0, 3, 0, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            this.okCancelTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.previewGroupBox, "previewGroupBox");
            this.previewGroupBox.Controls.Add(this.sampleViewTableLayoutPanel);
            this.previewGroupBox.Margin = new Padding(0, 3, 0, 3);
            this.previewGroupBox.Name = "previewGroupBox";
            this.previewGroupBox.TabStop = false;
            manager.ApplyResources(this.overarchingTableLayoutPanel, "overarchingTableLayoutPanel");
            this.overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.overarchingTableLayoutPanel.Controls.Add(this.cellStyleProperties, 0, 0);
            this.overarchingTableLayoutPanel.Controls.Add(this.okCancelTableLayoutPanel, 0, 2);
            this.overarchingTableLayoutPanel.Controls.Add(this.previewGroupBox, 0, 1);
            this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this, "$this");
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.Controls.Add(this.overarchingTableLayoutPanel);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.HelpButton = true;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "DataGridViewCellStyleBuilder";
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            base.HelpButtonClicked += new CancelEventHandler(this.DataGridViewCellStyleBuilder_HelpButtonClicked);
            base.HelpRequested += new HelpEventHandler(this.DataGridViewCellStyleBuilder_HelpRequested);
            base.Load += new EventHandler(this.DataGridViewCellStyleBuilder_Load);
            this.sampleViewTableLayoutPanel.ResumeLayout(false);
            this.sampleViewTableLayoutPanel.PerformLayout();
            this.sampleViewGridsTableLayoutPanel.ResumeLayout(false);
            this.sampleViewGridsTableLayoutPanel.PerformLayout();
            ((ISupportInitialize) this.sampleDataGridView).EndInit();
            ((ISupportInitialize) this.sampleDataGridViewSelected).EndInit();
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            this.previewGroupBox.ResumeLayout(false);
            this.previewGroupBox.PerformLayout();
            this.overarchingTableLayoutPanel.ResumeLayout(false);
            this.overarchingTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
        }

        private void InitializeGrids()
        {
            this.sampleDataGridViewSelected.Size = new Size(100, this.Font.Height + 9);
            this.sampleDataGridView.Size = new Size(100, this.Font.Height + 9);
            this.sampleDataGridView.AccessibilityObject.Name = System.Design.SR.GetString("CellStyleBuilderNormalPreviewAccName");
            DataGridViewRow dataGridViewRow = new DataGridViewRow();
            dataGridViewRow.Cells.Add(new DialogDataGridViewCell());
            dataGridViewRow.Cells[0].Value = "####";
            dataGridViewRow.Cells[0].AccessibilityObject.Name = System.Design.SR.GetString("CellStyleBuilderSelectedPreviewAccName");
            this.sampleDataGridViewSelected.Columns.Add(new DataGridViewTextBoxColumn());
            this.sampleDataGridViewSelected.Rows.Add(dataGridViewRow);
            this.sampleDataGridViewSelected.Rows[0].Selected = true;
            this.sampleDataGridViewSelected.AccessibilityObject.Name = System.Design.SR.GetString("CellStyleBuilderSelectedPreviewAccName");
            dataGridViewRow = new DataGridViewRow();
            dataGridViewRow.Cells.Add(new DialogDataGridViewCell());
            dataGridViewRow.Cells[0].Value = "####";
            dataGridViewRow.Cells[0].AccessibilityObject.Name = System.Design.SR.GetString("CellStyleBuilderNormalPreviewAccName");
            this.sampleDataGridView.Columns.Add(new DataGridViewTextBoxColumn());
            this.sampleDataGridView.Rows.Add(dataGridViewRow);
        }

        private void ListenerDataGridViewDefaultCellStyleChanged(object sender, EventArgs e)
        {
            DataGridViewCellStyle style = new DataGridViewCellStyle(this.cellStyle);
            this.sampleDataGridView.DefaultCellStyle = style;
            this.sampleDataGridViewSelected.DefaultCellStyle = style;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (((keyData & ~Keys.KeyCode) == Keys.None) && ((keyData & Keys.KeyCode) == Keys.Escape))
            {
                base.Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void sampleDataGridView_CellStateChanged(object sender, DataGridViewCellStateChangedEventArgs e)
        {
            if (((e.StateChanged & DataGridViewElementStates.Selected) != DataGridViewElementStates.None) && ((e.Cell.State & DataGridViewElementStates.Selected) != DataGridViewElementStates.None))
            {
                this.sampleDataGridView.ClearSelection();
            }
        }

        private void sampleDataGridView_Layout(object sender, LayoutEventArgs e)
        {
            DataGridView view = (DataGridView) sender;
            view.Rows[0].Height = view.Height;
            view.Columns[0].Width = view.Width;
        }

        public DataGridViewCellStyle CellStyle
        {
            get
            {
                return this.cellStyle;
            }
            set
            {
                this.cellStyle = new DataGridViewCellStyle(value);
                this.cellStyleProperties.SelectedObject = this.cellStyle;
                this.ListenerDataGridViewDefaultCellStyleChanged(null, EventArgs.Empty);
                this.listenerDataGridView.DefaultCellStyle = this.cellStyle;
                this.listenerDataGridView.DefaultCellStyleChanged += new EventHandler(this.ListenerDataGridViewDefaultCellStyleChanged);
            }
        }

        public ITypeDescriptorContext Context
        {
            set
            {
                this.context = value;
            }
        }

        private class DialogDataGridViewCell : DataGridViewTextBoxCell
        {
            private DialogDataGridViewCellAccessibleObject accObj;

            protected override AccessibleObject CreateAccessibilityInstance()
            {
                if (this.accObj == null)
                {
                    this.accObj = new DialogDataGridViewCellAccessibleObject(this);
                }
                return this.accObj;
            }

            private class DialogDataGridViewCellAccessibleObject : DataGridViewCell.DataGridViewCellAccessibleObject
            {
                private string name;

                public DialogDataGridViewCellAccessibleObject(DataGridViewCell owner) : base(owner)
                {
                    this.name = "";
                }

                public override string Name
                {
                    get
                    {
                        return this.name;
                    }
                    set
                    {
                        this.name = value;
                    }
                }
            }
        }
    }
}

