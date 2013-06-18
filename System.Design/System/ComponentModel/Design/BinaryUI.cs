namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class BinaryUI : Form
    {
        private Button buttonOK;
        private Button buttonSave;
        private ByteViewer byteViewer;
        private BinaryEditor editor;
        private GroupBox groupBoxMode;
        private TableLayoutPanel okSaveTableLayoutPanel;
        private TableLayoutPanel overarchingTableLayoutPanel;
        private RadioButton radioAnsi;
        private RadioButton radioAuto;
        private TableLayoutPanel radioButtonsTableLayoutPanel;
        private RadioButton radioHex;
        private RadioButton radioUnicode;
        private object value;

        public BinaryUI(BinaryEditor editor)
        {
            this.editor = editor;
            this.InitializeComponent();
        }

        private void ButtonOK_click(object source, EventArgs e)
        {
            object obj2 = this.value;
            this.editor.ConvertToValue(this.byteViewer.GetBytes(), ref obj2);
            this.value = obj2;
        }

        private void ButtonSave_click(object source, EventArgs e)
        {
            try
            {
                SaveFileDialog dialog = new SaveFileDialog {
                    FileName = System.Design.SR.GetString("BinaryEditorFileName"),
                    Title = System.Design.SR.GetString("BinaryEditorSaveFile"),
                    Filter = System.Design.SR.GetString("BinaryEditorAllFiles") + " (*.*)|*.*"
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.byteViewer.SaveToFile(dialog.FileName);
                }
            }
            catch (IOException exception)
            {
                System.Windows.Forms.Design.RTLAwareMessageBox.Show(null, System.Design.SR.GetString("BinaryEditorFileError", new object[] { exception.Message }), System.Design.SR.GetString("BinaryEditorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, 0);
            }
        }

        private void Form_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.editor.ShowHelp();
        }

        private void Form_HelpRequested(object sender, HelpEventArgs e)
        {
            this.editor.ShowHelp();
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(BinaryEditor));
            this.byteViewer = new ByteViewer();
            this.buttonOK = new Button();
            this.buttonSave = new Button();
            this.groupBoxMode = new GroupBox();
            this.radioButtonsTableLayoutPanel = new TableLayoutPanel();
            this.radioUnicode = new RadioButton();
            this.radioAuto = new RadioButton();
            this.radioAnsi = new RadioButton();
            this.radioHex = new RadioButton();
            this.okSaveTableLayoutPanel = new TableLayoutPanel();
            this.overarchingTableLayoutPanel = new TableLayoutPanel();
            this.byteViewer.SuspendLayout();
            this.groupBoxMode.SuspendLayout();
            this.radioButtonsTableLayoutPanel.SuspendLayout();
            this.okSaveTableLayoutPanel.SuspendLayout();
            this.overarchingTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.byteViewer, "byteViewer");
            this.byteViewer.SetDisplayMode(DisplayMode.Auto);
            this.byteViewer.Name = "byteViewer";
            this.byteViewer.Margin = Padding.Empty;
            this.byteViewer.Dock = DockStyle.Fill;
            manager.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Margin = new Padding(0, 0, 3, 0);
            this.buttonOK.MinimumSize = new Size(0x4b, 0x17);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Padding = new Padding(10, 0, 10, 0);
            this.buttonOK.Click += new EventHandler(this.ButtonOK_click);
            manager.ApplyResources(this.buttonSave, "buttonSave");
            this.buttonSave.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.buttonSave.Margin = new Padding(3, 0, 0, 0);
            this.buttonSave.MinimumSize = new Size(0x4b, 0x17);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Padding = new Padding(10, 0, 10, 0);
            this.buttonSave.Click += new EventHandler(this.ButtonSave_click);
            manager.ApplyResources(this.groupBoxMode, "groupBoxMode");
            this.groupBoxMode.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.groupBoxMode.Controls.Add(this.radioButtonsTableLayoutPanel);
            this.groupBoxMode.Margin = new Padding(0, 3, 0, 3);
            this.groupBoxMode.Name = "groupBoxMode";
            this.groupBoxMode.Padding = new Padding(0);
            this.groupBoxMode.TabStop = false;
            manager.ApplyResources(this.radioButtonsTableLayoutPanel, "radioButtonsTableLayoutPanel");
            this.radioButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            this.radioButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            this.radioButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            this.radioButtonsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            this.radioButtonsTableLayoutPanel.Controls.Add(this.radioUnicode, 3, 0);
            this.radioButtonsTableLayoutPanel.Controls.Add(this.radioAuto, 0, 0);
            this.radioButtonsTableLayoutPanel.Controls.Add(this.radioAnsi, 2, 0);
            this.radioButtonsTableLayoutPanel.Controls.Add(this.radioHex, 1, 0);
            this.radioButtonsTableLayoutPanel.Margin = new Padding(9);
            this.radioButtonsTableLayoutPanel.Name = "radioButtonsTableLayoutPanel";
            this.radioButtonsTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.radioUnicode, "radioUnicode");
            this.radioUnicode.Margin = new Padding(3, 0, 0, 0);
            this.radioUnicode.Name = "radioUnicode";
            this.radioUnicode.CheckedChanged += new EventHandler(this.RadioUnicode_checkedChanged);
            manager.ApplyResources(this.radioAuto, "radioAuto");
            this.radioAuto.Checked = true;
            this.radioAuto.Margin = new Padding(0, 0, 3, 0);
            this.radioAuto.Name = "radioAuto";
            this.radioAuto.CheckedChanged += new EventHandler(this.RadioAuto_checkedChanged);
            manager.ApplyResources(this.radioAnsi, "radioAnsi");
            this.radioAnsi.Margin = new Padding(3, 0, 3, 0);
            this.radioAnsi.Name = "radioAnsi";
            this.radioAnsi.CheckedChanged += new EventHandler(this.RadioAnsi_checkedChanged);
            manager.ApplyResources(this.radioHex, "radioHex");
            this.radioHex.Margin = new Padding(3, 0, 3, 0);
            this.radioHex.Name = "radioHex";
            this.radioHex.CheckedChanged += new EventHandler(this.RadioHex_checkedChanged);
            manager.ApplyResources(this.okSaveTableLayoutPanel, "okSaveTableLayoutPanel");
            this.okSaveTableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.okSaveTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okSaveTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okSaveTableLayoutPanel.Controls.Add(this.buttonOK, 0, 0);
            this.okSaveTableLayoutPanel.Controls.Add(this.buttonSave, 1, 0);
            this.okSaveTableLayoutPanel.Margin = new Padding(0, 9, 0, 0);
            this.okSaveTableLayoutPanel.Name = "okSaveTableLayoutPanel";
            this.okSaveTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            manager.ApplyResources(this.overarchingTableLayoutPanel, "overarchingTableLayoutPanel");
            this.overarchingTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.overarchingTableLayoutPanel.Controls.Add(this.byteViewer, 0, 0);
            this.overarchingTableLayoutPanel.Controls.Add(this.groupBoxMode, 0, 1);
            this.overarchingTableLayoutPanel.Controls.Add(this.okSaveTableLayoutPanel, 0, 2);
            this.overarchingTableLayoutPanel.Name = "overarchingTableLayoutPanel";
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.overarchingTableLayoutPanel.RowStyles.Add(new RowStyle());
            base.AcceptButton = this.buttonOK;
            manager.ApplyResources(this, "$this");
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.buttonOK;
            base.Controls.Add(this.overarchingTableLayoutPanel);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.HelpButton = true;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "BinaryUI";
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            base.HelpRequested += new HelpEventHandler(this.Form_HelpRequested);
            base.HelpButtonClicked += new CancelEventHandler(this.Form_HelpButtonClicked);
            this.byteViewer.ResumeLayout(false);
            this.byteViewer.PerformLayout();
            this.groupBoxMode.ResumeLayout(false);
            this.groupBoxMode.PerformLayout();
            this.radioButtonsTableLayoutPanel.ResumeLayout(false);
            this.radioButtonsTableLayoutPanel.PerformLayout();
            this.okSaveTableLayoutPanel.ResumeLayout(false);
            this.okSaveTableLayoutPanel.PerformLayout();
            this.overarchingTableLayoutPanel.ResumeLayout(false);
            this.overarchingTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
        }

        private void RadioAnsi_checkedChanged(object source, EventArgs e)
        {
            if (this.radioAnsi.Checked)
            {
                this.byteViewer.SetDisplayMode(DisplayMode.Ansi);
            }
        }

        private void RadioAuto_checkedChanged(object source, EventArgs e)
        {
            if (this.radioAuto.Checked)
            {
                this.byteViewer.SetDisplayMode(DisplayMode.Auto);
            }
        }

        private void RadioHex_checkedChanged(object source, EventArgs e)
        {
            if (this.radioHex.Checked)
            {
                this.byteViewer.SetDisplayMode(DisplayMode.Hexdump);
            }
        }

        private void RadioUnicode_checkedChanged(object source, EventArgs e)
        {
            if (this.radioUnicode.Checked)
            {
                this.byteViewer.SetDisplayMode(DisplayMode.Unicode);
            }
        }

        public object Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
                byte[] bytes = null;
                if (value != null)
                {
                    bytes = this.editor.ConvertToBytes(value);
                }
                if (bytes != null)
                {
                    this.byteViewer.SetBytes(bytes);
                    this.byteViewer.Enabled = true;
                }
                else
                {
                    this.byteViewer.SetBytes(new byte[0]);
                    this.byteViewer.Enabled = false;
                }
            }
        }
    }
}

