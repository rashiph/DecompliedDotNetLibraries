namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.Activities.Common;
    using System.Workflow.Activities.Rules;
    using System.Workflow.ComponentModel;

    internal abstract class BasicBrowserDialog : System.Windows.Forms.Form
    {
        private System.Workflow.ComponentModel.Activity activity;
        private Button cancelButton;
        private IContainer components;
        private ToolStripButton deleteToolStripButton;
        private Label descriptionLabel;
        private ToolStripButton editToolStripButton;
        private PictureBox headerPictureBox;
        private ImageList imageList;
        private string name;
        private ColumnHeader nameColumnHeader;
        private ToolStripButton newRuleToolStripButton;
        private Button okButton;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private Panel preiviewPanel;
        private Label previewLabel;
        private Panel previewRichEditBoxPanel;
        private TextBox previewRichTextBox;
        private ToolStripButton renameToolStripButton;
        private ListView rulesListView;
        private Panel rulesPanel;
        private ToolStrip rulesToolStrip;
        private IServiceProvider serviceProvider;
        private ToolStripSeparator toolStripSeparator1;
        private ColumnHeader validColumnHeader;

        protected BasicBrowserDialog(System.Workflow.ComponentModel.Activity activity, string name)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            this.activity = activity;
            this.InitializeComponent();
            this.descriptionLabel.Text = this.DescriptionText;
            this.Text = this.TitleText;
            this.previewLabel.Text = this.PreviewLabelText;
            this.newRuleToolStripButton.Enabled = true;
            this.name = name;
            this.serviceProvider = activity.Site;
            IUIService service = (IUIService) activity.Site.GetService(typeof(IUIService));
            if (service != null)
            {
                this.Font = (Font) service.Styles["DialogFont"];
            }
            base.HelpRequested += new HelpEventHandler(this.OnHelpRequested);
            base.HelpButtonClicked += new CancelEventHandler(this.OnHelpClicked);
            this.rulesListView.Select();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected abstract string GetObjectName(object ruleObject);
        private void InitializeComponent()
        {
            this.components = new Container();
            ComponentResourceManager manager = new ComponentResourceManager(typeof(BasicBrowserDialog));
            this.cancelButton = new Button();
            this.okButton = new Button();
            this.rulesListView = new ListView();
            this.nameColumnHeader = new ColumnHeader();
            this.validColumnHeader = new ColumnHeader();
            this.rulesPanel = new Panel();
            this.rulesToolStrip = new ToolStrip();
            this.imageList = new ImageList(this.components);
            this.newRuleToolStripButton = new ToolStripButton();
            this.editToolStripButton = new ToolStripButton();
            this.renameToolStripButton = new ToolStripButton();
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.deleteToolStripButton = new ToolStripButton();
            this.preiviewPanel = new Panel();
            this.previewRichEditBoxPanel = new Panel();
            this.previewRichTextBox = new TextBox();
            this.previewLabel = new Label();
            this.descriptionLabel = new Label();
            this.headerPictureBox = new PictureBox();
            this.okCancelTableLayoutPanel = new TableLayoutPanel();
            this.rulesPanel.SuspendLayout();
            this.rulesToolStrip.SuspendLayout();
            this.preiviewPanel.SuspendLayout();
            this.previewRichEditBoxPanel.SuspendLayout();
            ((ISupportInitialize) this.headerPictureBox).BeginInit();
            this.okCancelTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Click += new EventHandler(this.OnCancel);
            manager.ApplyResources(this.okButton, "okButton");
            this.okButton.Name = "okButton";
            this.okButton.Click += new EventHandler(this.OnOk);
            this.rulesListView.Columns.AddRange(new ColumnHeader[] { this.nameColumnHeader, this.validColumnHeader });
            manager.ApplyResources(this.rulesListView, "rulesListView");
            this.rulesListView.FullRowSelect = true;
            this.rulesListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.rulesListView.HideSelection = false;
            this.rulesListView.MultiSelect = false;
            this.rulesListView.Name = "rulesListView";
            this.rulesListView.Sorting = SortOrder.Ascending;
            this.rulesListView.UseCompatibleStateImageBehavior = false;
            this.rulesListView.View = View.Details;
            this.rulesListView.DoubleClick += new EventHandler(this.OnDoubleClick);
            this.rulesListView.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(this.OnItemSelectionChanged);
            manager.ApplyResources(this.nameColumnHeader, "nameColumnHeader");
            manager.ApplyResources(this.validColumnHeader, "validColumnHeader");
            manager.ApplyResources(this.rulesPanel, "rulesPanel");
            this.rulesPanel.Controls.Add(this.rulesToolStrip);
            this.rulesPanel.Controls.Add(this.rulesListView);
            this.rulesPanel.Name = "rulesPanel";
            this.rulesToolStrip.BackColor = SystemColors.Control;
            this.rulesToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.rulesToolStrip.ImageList = this.imageList;
            this.rulesToolStrip.Items.AddRange(new ToolStripItem[] { this.newRuleToolStripButton, this.editToolStripButton, this.renameToolStripButton, this.toolStripSeparator1, this.deleteToolStripButton });
            manager.ApplyResources(this.rulesToolStrip, "rulesToolStrip");
            this.rulesToolStrip.Name = "rulesToolStrip";
            this.rulesToolStrip.RenderMode = ToolStripRenderMode.System;
            this.rulesToolStrip.TabStop = true;
            this.imageList.ImageStream = (ImageListStreamer) manager.GetObject("imageList.ImageStream");
            this.imageList.TransparentColor = Color.Transparent;
            this.imageList.Images.SetKeyName(0, "NewRule.bmp");
            this.imageList.Images.SetKeyName(1, "EditRule.bmp");
            this.imageList.Images.SetKeyName(2, "RenameRule.bmp");
            this.imageList.Images.SetKeyName(3, "Delete.bmp");
            manager.ApplyResources(this.newRuleToolStripButton, "newRuleToolStripButton");
            this.newRuleToolStripButton.Name = "newRuleToolStripButton";
            this.newRuleToolStripButton.Click += new EventHandler(this.OnNew);
            manager.ApplyResources(this.editToolStripButton, "editToolStripButton");
            this.editToolStripButton.Name = "editToolStripButton";
            this.editToolStripButton.Click += new EventHandler(this.OnEdit);
            manager.ApplyResources(this.renameToolStripButton, "renameToolStripButton");
            this.renameToolStripButton.Name = "renameToolStripButton";
            this.renameToolStripButton.Click += new EventHandler(this.OnRename);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            manager.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            manager.ApplyResources(this.deleteToolStripButton, "deleteToolStripButton");
            this.deleteToolStripButton.Name = "deleteToolStripButton";
            this.deleteToolStripButton.Click += new EventHandler(this.OnDelete);
            this.preiviewPanel.Controls.Add(this.previewRichEditBoxPanel);
            this.preiviewPanel.Controls.Add(this.previewLabel);
            manager.ApplyResources(this.preiviewPanel, "preiviewPanel");
            this.preiviewPanel.Name = "preiviewPanel";
            manager.ApplyResources(this.previewRichEditBoxPanel, "previewRichEditBoxPanel");
            this.previewRichEditBoxPanel.BackColor = SystemColors.GradientInactiveCaption;
            this.previewRichEditBoxPanel.Controls.Add(this.previewRichTextBox);
            this.previewRichEditBoxPanel.Name = "previewRichEditBoxPanel";
            this.previewRichTextBox.BackColor = SystemColors.Control;
            this.previewRichTextBox.BorderStyle = BorderStyle.None;
            manager.ApplyResources(this.previewRichTextBox, "previewRichTextBox");
            this.previewRichTextBox.Name = "previewRichTextBox";
            this.previewRichTextBox.ReadOnly = true;
            this.previewRichTextBox.TabStop = false;
            manager.ApplyResources(this.previewLabel, "previewLabel");
            this.previewLabel.Name = "previewLabel";
            manager.ApplyResources(this.descriptionLabel, "descriptionLabel");
            this.descriptionLabel.Name = "descriptionLabel";
            manager.ApplyResources(this.headerPictureBox, "headerPictureBox");
            this.headerPictureBox.Name = "headerPictureBox";
            this.headerPictureBox.TabStop = false;
            manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            base.AcceptButton = this.okButton;
            manager.ApplyResources(this, "$this");
            base.CancelButton = this.cancelButton;
            base.Controls.Add(this.okCancelTableLayoutPanel);
            base.Controls.Add(this.headerPictureBox);
            base.Controls.Add(this.descriptionLabel);
            base.Controls.Add(this.preiviewPanel);
            base.Controls.Add(this.rulesPanel);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.HelpButton = true;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "BasicBrowserDialog";
            base.ShowInTaskbar = false;
            this.rulesPanel.ResumeLayout(false);
            this.rulesPanel.PerformLayout();
            this.rulesToolStrip.ResumeLayout(false);
            this.rulesToolStrip.PerformLayout();
            this.preiviewPanel.ResumeLayout(false);
            this.preiviewPanel.PerformLayout();
            this.previewRichEditBoxPanel.ResumeLayout(false);
            this.previewRichEditBoxPanel.PerformLayout();
            ((ISupportInitialize) this.headerPictureBox).EndInit();
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected void InitializeListView(IList list, string selectedName)
        {
            foreach (object obj2 in list)
            {
                ListViewItem listViewItem = this.rulesListView.Items.Add(new ListViewItem());
                this.UpdateListViewItem(obj2, listViewItem);
                if (this.GetObjectName(obj2) == selectedName)
                {
                    listViewItem.Selected = true;
                }
            }
            if (this.rulesListView.SelectedItems.Count == 0)
            {
                this.OnToolbarStatus();
            }
        }

        internal abstract bool IsUniqueName(string ruleName);
        private void OnCancel(object sender, EventArgs e)
        {
            this.name = null;
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void OnComponentChanged()
        {
            ISite serviceProvider = this.activity.Site;
            IComponentChangeService service = (IComponentChangeService) serviceProvider.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.OnComponentChanged(this.activity, null, null, null);
            }
            ConditionHelper.Flush_Rules_DT(serviceProvider, Helpers.GetRootActivity(this.activity));
        }

        private bool OnComponentChanging()
        {
            IComponentChangeService service = (IComponentChangeService) this.activity.Site.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                try
                {
                    service.OnComponentChanging(this.activity, null);
                }
                catch (CheckoutException exception)
                {
                    if (exception != CheckoutException.Canceled)
                    {
                        throw;
                    }
                    return false;
                }
            }
            return true;
        }

        private void OnDelete(object sender, EventArgs e)
        {
            MessageBoxOptions options = 0;
            if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
            {
                options = MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;
            }
            if (MessageBox.Show(this, this.ConfirmDeleteMessageText, this.ConfirmDeleteTitleText, MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, options) == DialogResult.OK)
            {
                using (new WaitCursor())
                {
                    object tag = this.rulesListView.SelectedItems[0].Tag;
                    try
                    {
                        this.OnComponentChanging();
                        int index = this.rulesListView.SelectedIndices[0];
                        object ruleObject = null;
                        this.OnDeleteInternal(tag);
                        this.rulesListView.Items.RemoveAt(index);
                        if (this.rulesListView.Items.Count > 0)
                        {
                            int num2 = Math.Min(index, this.rulesListView.Items.Count - 1);
                            this.rulesListView.Items[num2].Selected = true;
                            ruleObject = this.rulesListView.Items[num2].Tag;
                        }
                        this.UpdatePreview(this.previewRichTextBox, ruleObject);
                        this.OnComponentChanged();
                    }
                    catch (InvalidOperationException exception)
                    {
                        DesignerHelpers.DisplayError(exception.Message, this.Text, this.activity.Site);
                    }
                }
            }
        }

        protected abstract void OnDeleteInternal(object ruleObject);
        private void OnDoubleClick(object sender, EventArgs e)
        {
            if (this.rulesListView.SelectedItems.Count > 0)
            {
                this.OnOk(sender, e);
            }
        }

        private void OnEdit(object sender, EventArgs e)
        {
            try
            {
                this.OnComponentChanging();
                object updatedRuleObject = null;
                object tag = this.rulesListView.SelectedItems[0].Tag;
                if (this.OnEditInternal(tag, out updatedRuleObject))
                {
                    using (new WaitCursor())
                    {
                        this.UpdateListViewItem(updatedRuleObject, this.rulesListView.SelectedItems[0]);
                        this.UpdatePreview(this.previewRichTextBox, updatedRuleObject);
                        this.OnComponentChanged();
                    }
                }
            }
            catch (InvalidOperationException exception)
            {
                DesignerHelpers.DisplayError(exception.Message, this.Text, this.activity.Site);
            }
        }

        protected abstract bool OnEditInternal(object currentRuleObject, out object updatedRuleObject);
        private void OnHelpClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.ShowHelp();
        }

        private void OnHelpRequested(object sender, HelpEventArgs e)
        {
            this.ShowHelp();
        }

        private void OnItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                e.Item.Focused = true;
            }
            this.OnToolbarStatus();
            this.okButton.Enabled = e.IsSelected;
            object ruleObject = null;
            if (e.IsSelected)
            {
                ruleObject = e.Item.Tag;
            }
            this.UpdatePreview(this.previewRichTextBox, ruleObject);
        }

        private void OnNew(object sender, EventArgs e)
        {
            try
            {
                this.OnComponentChanging();
                object ruleObject = this.OnNewInternal();
                if (ruleObject != null)
                {
                    using (new WaitCursor())
                    {
                        ListViewItem listViewItem = this.rulesListView.Items.Add(new ListViewItem());
                        this.UpdateListViewItem(ruleObject, listViewItem);
                        listViewItem.Selected = true;
                        this.OnComponentChanged();
                    }
                }
            }
            catch (InvalidOperationException exception)
            {
                DesignerHelpers.DisplayError(exception.Message, this.Text, this.activity.Site);
            }
        }

        protected abstract object OnNewInternal();
        private void OnOk(object sender, EventArgs e)
        {
            object tag = this.rulesListView.SelectedItems[0].Tag;
            this.name = this.GetObjectName(tag);
            base.DialogResult = DialogResult.OK;
            base.Close();
        }

        private void OnRename(object sender, EventArgs e)
        {
            try
            {
                this.OnComponentChanging();
                object tag = this.rulesListView.SelectedItems[0].Tag;
                string str = this.OnRenameInternal(tag);
                if (str != null)
                {
                    using (new WaitCursor())
                    {
                        ListViewItem item = this.rulesListView.SelectedItems[0];
                        item.Text = str;
                        this.OnComponentChanged();
                    }
                }
            }
            catch (InvalidOperationException exception)
            {
                DesignerHelpers.DisplayError(exception.Message, this.Text, this.activity.Site);
            }
        }

        protected abstract string OnRenameInternal(object ruleObject);
        private void OnToolbarStatus()
        {
            if (this.rulesListView.SelectedItems.Count == 1)
            {
                this.editToolStripButton.Enabled = true;
                this.renameToolStripButton.Enabled = true;
                this.deleteToolStripButton.Enabled = true;
            }
            else
            {
                this.editToolStripButton.Enabled = false;
                this.renameToolStripButton.Enabled = false;
                this.deleteToolStripButton.Enabled = false;
            }
        }

        private void ShowHelp()
        {
            if (this.serviceProvider != null)
            {
                IHelpService service = this.serviceProvider.GetService(typeof(IHelpService)) as IHelpService;
                if (service != null)
                {
                    service.ShowHelpFromKeyword(base.GetType().FullName + ".UI");
                }
                else
                {
                    IUIService service2 = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
                    if (service2 != null)
                    {
                        service2.ShowError(Messages.NoHelp);
                    }
                }
            }
            else
            {
                IUIService service3 = (IUIService) this.GetService(typeof(IUIService));
                if (service3 != null)
                {
                    service3.ShowError(Messages.NoHelp);
                }
            }
        }

        protected abstract void UpdateListViewItem(object ruleObject, ListViewItem listViewItem);
        protected abstract void UpdatePreview(TextBox previewTextBox, object ruleObject);

        protected System.Workflow.ComponentModel.Activity Activity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activity;
            }
        }

        protected abstract string ConfirmDeleteMessageText { get; }

        protected abstract string ConfirmDeleteTitleText { get; }

        protected abstract string DescriptionText { get; }

        internal abstract string DuplicateNameErrorText { get; }

        internal abstract string EmptyNameErrorText { get; }

        internal abstract string NewNameLabelText { get; }

        protected abstract string PreviewLabelText { get; }

        internal abstract string RenameTitleText { get; }

        public string SelectedName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
        }

        protected abstract string TitleText { get; }

        private class WaitCursor : IDisposable
        {
            private Cursor oldCursor;

            public WaitCursor()
            {
                Application.DoEvents();
                this.oldCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
            }

            public void Dispose()
            {
                Cursor.Current = this.oldCursor;
            }
        }
    }
}

