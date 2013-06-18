namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Windows.Forms;

    internal class LinkAreaEditor : UITypeEditor
    {
        private LinkAreaUI linkAreaUI;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                IHelpService service = (IHelpService) provider.GetService(typeof(IHelpService));
                if (edSvc == null)
                {
                    return value;
                }
                if (this.linkAreaUI == null)
                {
                    this.linkAreaUI = new LinkAreaUI(this, service);
                }
                string sampleText = string.Empty;
                PropertyDescriptor descriptor = null;
                if ((context != null) && (context.Instance != null))
                {
                    descriptor = TypeDescriptor.GetProperties(context.Instance)["Text"];
                    if ((descriptor != null) && (descriptor.PropertyType == typeof(string)))
                    {
                        sampleText = (string) descriptor.GetValue(context.Instance);
                    }
                }
                string str2 = sampleText;
                this.linkAreaUI.SampleText = sampleText;
                this.linkAreaUI.Start(edSvc, value);
                if (edSvc.ShowDialog(this.linkAreaUI) == DialogResult.OK)
                {
                    value = this.linkAreaUI.Value;
                    sampleText = this.linkAreaUI.SampleText;
                    if ((!str2.Equals(sampleText) && (descriptor != null)) && (descriptor.PropertyType == typeof(string)))
                    {
                        descriptor.SetValue(context.Instance, sampleText);
                    }
                }
                this.linkAreaUI.End();
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        internal class LinkAreaUI : Form
        {
            private Button cancelButton = new Button();
            private Label caption = new Label();
            private LinkAreaEditor editor;
            private IWindowsFormsEditorService edSvc;
            private IHelpService helpService;
            private Button okButton = new Button();
            private TableLayoutPanel okCancelTableLayoutPanel;
            private TextBox sampleEdit = new TextBox();
            private object value;

            public LinkAreaUI(LinkAreaEditor editor, IHelpService helpService)
            {
                this.editor = editor;
                this.helpService = helpService;
                this.InitializeComponent();
            }

            public void End()
            {
                this.edSvc = null;
                this.value = null;
            }

            private void InitializeComponent()
            {
                ComponentResourceManager manager = new ComponentResourceManager(typeof(LinkAreaEditor));
                this.caption = new Label();
                this.sampleEdit = new TextBox();
                this.okButton = new Button();
                this.cancelButton = new Button();
                this.okCancelTableLayoutPanel = new TableLayoutPanel();
                this.okCancelTableLayoutPanel.SuspendLayout();
                base.SuspendLayout();
                this.okButton.Click += new EventHandler(this.okButton_click);
                manager.ApplyResources(this.caption, "caption");
                this.caption.Margin = new Padding(3, 1, 3, 0);
                this.caption.Name = "caption";
                manager.ApplyResources(this.sampleEdit, "sampleEdit");
                this.sampleEdit.Margin = new Padding(3, 2, 3, 3);
                this.sampleEdit.Name = "sampleEdit";
                this.sampleEdit.HideSelection = false;
                this.sampleEdit.ScrollBars = ScrollBars.Vertical;
                manager.ApplyResources(this.okButton, "okButton");
                this.okButton.DialogResult = DialogResult.OK;
                this.okButton.Margin = new Padding(0, 0, 2, 0);
                this.okButton.Name = "okButton";
                manager.ApplyResources(this.cancelButton, "cancelButton");
                this.cancelButton.DialogResult = DialogResult.Cancel;
                this.cancelButton.Margin = new Padding(3, 0, 0, 0);
                this.cancelButton.Name = "cancelButton";
                manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
                this.okCancelTableLayoutPanel.ColumnCount = 2;
                this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                this.okCancelTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
                this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
                this.okCancelTableLayoutPanel.Margin = new Padding(3, 1, 3, 3);
                this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
                this.okCancelTableLayoutPanel.RowCount = 1;
                this.okCancelTableLayoutPanel.RowStyles.Add(new RowStyle());
                this.okCancelTableLayoutPanel.RowStyles.Add(new RowStyle());
                manager.ApplyResources(this, "$this");
                base.AutoScaleMode = AutoScaleMode.Font;
                base.CancelButton = this.cancelButton;
                base.Controls.Add(this.okCancelTableLayoutPanel);
                base.Controls.Add(this.sampleEdit);
                base.Controls.Add(this.caption);
                base.HelpButton = true;
                base.MaximizeBox = false;
                base.MinimizeBox = false;
                base.Name = "LinkAreaEditor";
                base.ShowIcon = false;
                base.ShowInTaskbar = false;
                base.HelpButtonClicked += new CancelEventHandler(this.LinkAreaEditor_HelpButtonClicked);
                this.okCancelTableLayoutPanel.ResumeLayout(false);
                this.okCancelTableLayoutPanel.PerformLayout();
                base.ResumeLayout(false);
                base.PerformLayout();
            }

            private void LinkAreaEditor_HelpButtonClicked(object sender, CancelEventArgs e)
            {
                e.Cancel = true;
                this.ShowHelp();
            }

            private void okButton_click(object sender, EventArgs e)
            {
                this.value = new LinkArea(this.sampleEdit.SelectionStart, this.sampleEdit.SelectionLength);
            }

            private void ShowHelp()
            {
                if (this.helpService != null)
                {
                    this.helpService.ShowHelpFromKeyword(this.HelpTopic);
                }
            }

            public void Start(IWindowsFormsEditorService edSvc, object value)
            {
                this.edSvc = edSvc;
                this.value = value;
                this.UpdateSelection();
                base.ActiveControl = this.sampleEdit;
            }

            private void UpdateSelection()
            {
                if (this.value is LinkArea)
                {
                    LinkArea area = (LinkArea) this.value;
                    try
                    {
                        this.sampleEdit.SelectionStart = area.Start;
                        this.sampleEdit.SelectionLength = area.Length;
                    }
                    catch (Exception exception)
                    {
                        if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                        {
                            throw;
                        }
                    }
                }
            }

            private string HelpTopic
            {
                get
                {
                    return "net.ComponentModel.LinkAreaEditor";
                }
            }

            public string SampleText
            {
                get
                {
                    return this.sampleEdit.Text;
                }
                set
                {
                    this.sampleEdit.Text = value;
                    this.UpdateSelection();
                }
            }

            public object Value
            {
                get
                {
                    return this.value;
                }
            }
        }
    }
}

