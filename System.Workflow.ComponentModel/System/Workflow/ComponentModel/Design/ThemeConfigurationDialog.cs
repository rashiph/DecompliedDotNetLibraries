namespace System.Workflow.ComponentModel.Design
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    public sealed class ThemeConfigurationDialog : Form
    {
        private WorkflowTheme bufferedTheme;
        private Button button3;
        private Button cancelButton;
        private IContainer components;
        private DesignerPreview designerPreview;
        private TreeView designerTreeView;
        private Panel dummyPreviewPanel;
        private TableLayoutPanel nameLocationTableLayoutPanel;
        private Button okButton;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private Button previewButton;
        private Label previewLabel;
        private bool previewShown;
        private PropertyGrid propertiesGrid;
        private Label selectDesignerLabel;
        private IServiceProvider serviceProvider;
        private Splitter splitter;
        private Panel themeConfigPanel;
        private bool themeDirty;
        private Button themeLocationButton;
        private Label themeLocationLabel;
        private TextBox themeLocationTextBox;
        private Label themeNameLabel;
        private TextBox themeNameTextBox;
        private Panel themePanel;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ThemeConfigurationDialog(IServiceProvider serviceProvider) : this(serviceProvider, null)
        {
        }

        public ThemeConfigurationDialog(IServiceProvider serviceProvider, WorkflowTheme theme)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            this.serviceProvider = serviceProvider;
            if (theme == null)
            {
                this.bufferedTheme = new WorkflowTheme();
                this.themeDirty = true;
            }
            else
            {
                this.bufferedTheme = theme;
                this.themeDirty = false;
            }
            this.bufferedTheme.ReadOnly = false;
            this.InitializeComponent();
            this.themeLocationButton.AutoSize = true;
            this.Font = this.StandardFont;
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.OnOperatingSystemSettingsChanged);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                }
                SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.OnOperatingSystemSettingsChanged);
                if (this.designerPreview != null)
                {
                    this.designerPreview.Dispose();
                    this.designerPreview = null;
                }
                if (this.bufferedTheme != null)
                {
                    ((IDisposable) this.bufferedTheme).Dispose();
                    this.bufferedTheme = null;
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(ThemeConfigurationDialog));
            this.designerTreeView = new TreeView();
            this.themeNameLabel = new Label();
            this.themeLocationLabel = new Label();
            this.themeNameTextBox = new TextBox();
            this.nameLocationTableLayoutPanel = new TableLayoutPanel();
            this.themeLocationButton = new Button();
            this.themeLocationTextBox = new TextBox();
            this.button3 = new Button();
            this.okButton = new Button();
            this.cancelButton = new Button();
            this.themePanel = new Panel();
            this.themeConfigPanel = new Panel();
            this.propertiesGrid = new PropertyGrid();
            this.previewLabel = new Label();
            this.selectDesignerLabel = new Label();
            this.dummyPreviewPanel = new Panel();
            this.previewButton = new Button();
            this.okCancelTableLayoutPanel = new TableLayoutPanel();
            this.nameLocationTableLayoutPanel.SuspendLayout();
            this.themePanel.SuspendLayout();
            this.themeConfigPanel.SuspendLayout();
            this.okCancelTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.designerTreeView, "designerTreeView");
            this.designerTreeView.Name = "designerTreeView";
            manager.ApplyResources(this.themeNameLabel, "themeNameLabel");
            this.themeNameLabel.Margin = new Padding(0, 0, 3, 3);
            this.themeNameLabel.Name = "themeNameLabel";
            manager.ApplyResources(this.themeLocationLabel, "themeLocationLabel");
            this.themeLocationLabel.Margin = new Padding(0, 3, 3, 0);
            this.themeLocationLabel.Name = "themeLocationLabel";
            manager.ApplyResources(this.themeNameTextBox, "themeNameTextBox");
            this.nameLocationTableLayoutPanel.SetColumnSpan(this.themeNameTextBox, 2);
            this.themeNameTextBox.Margin = new Padding(3, 0, 0, 3);
            this.themeNameTextBox.Name = "themeNameTextBox";
            manager.ApplyResources(this.nameLocationTableLayoutPanel, "nameLocationTableLayoutPanel");
            this.nameLocationTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.nameLocationTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.nameLocationTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.nameLocationTableLayoutPanel.Controls.Add(this.themeNameLabel, 0, 0);
            this.nameLocationTableLayoutPanel.Controls.Add(this.themeNameTextBox, 1, 0);
            this.nameLocationTableLayoutPanel.Controls.Add(this.themeLocationButton, 2, 1);
            this.nameLocationTableLayoutPanel.Controls.Add(this.themeLocationLabel, 0, 1);
            this.nameLocationTableLayoutPanel.Controls.Add(this.themeLocationTextBox, 1, 1);
            this.nameLocationTableLayoutPanel.Name = "nameLocationTableLayoutPanel";
            this.nameLocationTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            this.nameLocationTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            manager.ApplyResources(this.themeLocationButton, "themeLocationButton");
            this.themeLocationButton.Margin = new Padding(3, 3, 0, 0);
            this.themeLocationButton.Name = "themeLocationButton";
            manager.ApplyResources(this.themeLocationTextBox, "themeLocationTextBox");
            this.themeLocationTextBox.Margin = new Padding(3, 3, 3, 0);
            this.themeLocationTextBox.Name = "themeLocationTextBox";
            manager.ApplyResources(this.button3, "button3");
            this.button3.Name = "button3";
            manager.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = DialogResult.OK;
            this.okButton.Margin = new Padding(0, 0, 3, 0);
            this.okButton.Name = "okButton";
            manager.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Margin = new Padding(3, 0, 3, 0);
            this.cancelButton.Name = "cancelButton";
            this.themePanel.Controls.Add(this.themeConfigPanel);
            this.themePanel.Controls.Add(this.previewLabel);
            this.themePanel.Controls.Add(this.selectDesignerLabel);
            this.themePanel.Controls.Add(this.dummyPreviewPanel);
            manager.ApplyResources(this.themePanel, "themePanel");
            this.themePanel.Margin = new Padding(4);
            this.themePanel.Name = "themePanel";
            this.themeConfigPanel.Controls.Add(this.designerTreeView);
            this.themeConfigPanel.Controls.Add(this.propertiesGrid);
            manager.ApplyResources(this.themeConfigPanel, "themeConfigPanel");
            this.themeConfigPanel.Name = "themeConfigPanel";
            this.propertiesGrid.CommandsVisibleIfAvailable = true;
            manager.ApplyResources(this.propertiesGrid, "propertiesGrid");
            this.propertiesGrid.Name = "propertiesGrid";
            this.propertiesGrid.ToolbarVisible = false;
            manager.ApplyResources(this.previewLabel, "previewLabel");
            this.previewLabel.Name = "previewLabel";
            manager.ApplyResources(this.selectDesignerLabel, "selectDesignerLabel");
            this.selectDesignerLabel.Name = "selectDesignerLabel";
            manager.ApplyResources(this.dummyPreviewPanel, "dummyPreviewPanel");
            this.dummyPreviewPanel.Name = "dummyPreviewPanel";
            manager.ApplyResources(this.previewButton, "previewButton");
            this.previewButton.Margin = new Padding(3, 0, 0, 0);
            this.previewButton.Name = "previewButton";
            manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            this.okCancelTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.previewButton, 2, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            this.okCancelTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            base.AcceptButton = this.okButton;
            base.CancelButton = this.cancelButton;
            manager.ApplyResources(this, "$this");
            base.Controls.Add(this.nameLocationTableLayoutPanel);
            base.Controls.Add(this.okCancelTableLayoutPanel);
            base.Controls.Add(this.themePanel);
            base.Controls.Add(this.button3);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "ThemeConfigurationDialog";
            base.ShowInTaskbar = false;
            base.HelpButton = true;
            base.SizeGripStyle = SizeGripStyle.Hide;
            this.nameLocationTableLayoutPanel.ResumeLayout(false);
            this.nameLocationTableLayoutPanel.PerformLayout();
            this.themePanel.ResumeLayout(false);
            this.themeConfigPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeControls()
        {
            base.HelpButtonClicked += new CancelEventHandler(this.OnHelpClicked);
            this.themeNameTextBox.Text = this.bufferedTheme.Name;
            this.themeLocationTextBox.Text = this.bufferedTheme.FilePath;
            this.propertiesGrid.PropertySort = PropertySort.Categorized;
            this.designerPreview = new DesignerPreview(this);
            this.dummyPreviewPanel.Parent.Controls.Add(this.designerPreview);
            this.designerPreview.TabStop = false;
            this.designerPreview.Location = this.dummyPreviewPanel.Location;
            this.designerPreview.Size = this.dummyPreviewPanel.Size;
            this.dummyPreviewPanel.Visible = false;
            this.designerPreview.Parent.Controls.Remove(this.dummyPreviewPanel);
            this.designerTreeView.ShowLines = false;
            this.designerTreeView.ShowPlusMinus = false;
            this.designerTreeView.ShowRootLines = false;
            this.designerTreeView.ShowNodeToolTips = true;
            this.designerTreeView.HideSelection = false;
            this.designerTreeView.ItemHeight = Math.Max(this.designerTreeView.ItemHeight, 0x12);
            ThemeConfigHelpers.PopulateActivities(this.serviceProvider, this.designerTreeView);
            this.themeConfigPanel.Controls.Remove(this.designerTreeView);
            this.themeConfigPanel.Controls.Remove(this.propertiesGrid);
            this.designerTreeView.Dock = DockStyle.Left;
            this.splitter = new Splitter();
            this.splitter.Dock = DockStyle.Left;
            this.propertiesGrid.Dock = DockStyle.Fill;
            this.themeConfigPanel.Controls.AddRange(new Control[] { this.propertiesGrid, this.splitter, this.designerTreeView });
            this.themePanel.Paint += new PaintEventHandler(this.OnThemePanelPaint);
            this.previewButton.Click += new EventHandler(this.OnPreviewClicked);
            this.designerTreeView.AfterSelect += new TreeViewEventHandler(this.OnDesignerSelectionChanged);
            this.themeLocationButton.Click += new EventHandler(this.OnThemeLocationClicked);
            this.okButton.Click += new EventHandler(this.OnOk);
            this.propertiesGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(this.OnThemePropertyChanged);
            this.themeNameTextBox.TextChanged += new EventHandler(this.OnThemeChanged);
            this.themeLocationTextBox.TextChanged += new EventHandler(this.OnThemeChanged);
            this.designerTreeView.SelectedNode = (this.designerTreeView.Nodes.Count > 0) ? this.designerTreeView.Nodes[0] : null;
            this.designerTreeView.SelectedNode.EnsureVisible();
            this.ShowPreview = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            this.bufferedTheme.ReadOnly = true;
        }

        private void OnDesignerSelectionChanged(object sender, TreeViewEventArgs eventArgs)
        {
            System.Type activityType = ((eventArgs.Node != null) && typeof(Activity).IsAssignableFrom(eventArgs.Node.Tag as System.Type)) ? (eventArgs.Node.Tag as System.Type) : null;
            IDesigner designer = this.designerPreview.UpdatePreview(activityType);
            object[] objArray = null;
            if (activityType == null)
            {
                if (eventArgs.Node != null)
                {
                    objArray = (eventArgs.Node.Parent == null) ? new object[] { this.bufferedTheme.AmbientTheme } : ((object[]) ThemeConfigHelpers.GetDesignerThemes(this.serviceProvider, this.bufferedTheme, eventArgs.Node));
                }
            }
            else
            {
                objArray = (designer != null) ? new object[] { this.bufferedTheme.GetDesignerTheme(designer as ActivityDesigner) } : null;
            }
            this.propertiesGrid.SelectedObjects = objArray;
        }

        private void OnHelpClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.ShowHelp();
        }

        protected override void OnHelpRequested(HelpEventArgs e)
        {
            this.ShowHelp();
            e.Handled = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                this.InitializeControls();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void OnOk(object sender, EventArgs e)
        {
            string error = string.Empty;
            Control control = null;
            if (!this.ValidateControls(out error, out control))
            {
                base.DialogResult = DialogResult.None;
                DesignerHelpers.ShowError(this.serviceProvider, error);
                if (control != null)
                {
                    TextBox box = control as TextBox;
                    if (box != null)
                    {
                        box.SelectionStart = 0;
                        box.SelectionLength = (box.Text != null) ? box.Text.Length : 0;
                    }
                    control.Focus();
                }
            }
            else if (!this.bufferedTheme.FilePath.Equals(this.themeLocationTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase) && (DialogResult.No == DesignerHelpers.ShowMessage(this.serviceProvider, DR.GetString("UpdateRelativePaths", new object[0]), DR.GetString("WorkflowDesignerTitle", new object[0]), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)))
            {
                base.DialogResult = DialogResult.None;
            }
            else if (this.themeDirty)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ThemeConfigHelpers.EnsureDesignerThemes(this.serviceProvider, this.bufferedTheme, ThemeConfigHelpers.GetAllTreeNodes(this.designerTreeView));
                    this.bufferedTheme.ReadOnly = false;
                    this.bufferedTheme.Name = this.themeNameTextBox.Text.Trim();
                    this.bufferedTheme.Description = DR.GetString("ThemeDescription", new object[0]);
                    this.bufferedTheme.Save(this.themeLocationTextBox.Text.Trim());
                    this.themeDirty = false;
                    this.bufferedTheme.ReadOnly = true;
                }
                catch
                {
                    DesignerHelpers.ShowError(this.serviceProvider, DR.GetString("ThemeFileCreationError", new object[0]));
                    this.themeLocationTextBox.SelectionStart = 0;
                    this.themeLocationTextBox.SelectionLength = (this.themeLocationTextBox.Text != null) ? this.themeLocationTextBox.Text.Length : 0;
                    this.themeLocationTextBox.Focus();
                    base.DialogResult = DialogResult.None;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void OnOperatingSystemSettingsChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if ((e.Category == UserPreferenceCategory.Color) || (e.Category == UserPreferenceCategory.VisualStyle))
            {
                this.Font = this.StandardFont;
            }
        }

        private void OnPreviewClicked(object sender, EventArgs e)
        {
            this.ShowPreview = !this.ShowPreview;
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            this.themeDirty = true;
        }

        private void OnThemeLocationClicked(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog {
                AddExtension = true,
                DefaultExt = "*.wtm",
                Filter = DR.GetString("ThemeFileFilter", new object[0]),
                RestoreDirectory = false
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                this.themeLocationTextBox.Text = dialog.FileName;
            }
        }

        private void OnThemePanelPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(SystemPens.ControlDark, 0, 0, this.themePanel.ClientSize.Width - 1, this.themePanel.ClientSize.Height - 2);
            if (this.previewShown)
            {
                Point point = new Point(this.propertiesGrid.Right + ((this.dummyPreviewPanel.Left - this.propertiesGrid.Right) / 2), this.themePanel.Margin.Top);
                Point point2 = new Point(point.X, this.themePanel.Height - this.themePanel.Margin.Bottom);
                e.Graphics.DrawLine(SystemPens.ControlDark, point, point2);
            }
            Size size = new Size(8, 8);
            using (Pen pen = new Pen(System.Drawing.Color.Black, 1f))
            {
                pen.DashStyle = DashStyle.Dot;
                e.Graphics.DrawLine(pen, (int) (this.designerPreview.Left - size.Width), (int) (this.designerPreview.Top - 1), (int) (this.designerPreview.Right + size.Width), (int) (this.designerPreview.Top - 1));
                e.Graphics.DrawLine(pen, (int) (this.designerPreview.Left - size.Width), (int) (this.designerPreview.Bottom + 1), (int) (this.designerPreview.Right + size.Width), (int) (this.designerPreview.Bottom + 1));
                e.Graphics.DrawLine(pen, (int) (this.designerPreview.Left - 1), (int) (this.designerPreview.Top - size.Height), (int) (this.designerPreview.Left - 1), (int) (this.designerPreview.Bottom + size.Height));
                e.Graphics.DrawLine(pen, (int) (this.designerPreview.Right + 1), (int) (this.designerPreview.Top - size.Height), (int) (this.designerPreview.Right + 1), (int) (this.designerPreview.Bottom + size.Height));
            }
        }

        private void OnThemePropertyChanged(object sender, PropertyValueChangedEventArgs e)
        {
            this.themeDirty = true;
        }

        private void ShowHelp()
        {
            DesignerHelpers.ShowHelpFromKeyword(this.serviceProvider, typeof(ThemeConfigurationDialog).FullName + ".UI");
        }

        private bool ValidateControls(out string error, out Control control)
        {
            error = string.Empty;
            control = null;
            if ((this.themeNameTextBox.Text == null) || (this.themeNameTextBox.Text.Trim().Length == 0))
            {
                error = DR.GetString("ThemeNameNotValid", new object[0]);
                control = this.themeNameTextBox;
                return false;
            }
            if (this.themeLocationTextBox.Text == null)
            {
                error = DR.GetString("ThemePathNotValid", new object[0]);
                control = this.themeNameTextBox;
                return false;
            }
            string path = this.themeLocationTextBox.Text.Trim();
            if (((path.IndexOfAny(Path.GetInvalidPathChars()) >= 0) || !Path.IsPathRooted(path)) || !Path.HasExtension(path))
            {
                error = DR.GetString("ThemePathNotValid", new object[0]);
                control = this.themeLocationTextBox;
                return false;
            }
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            if (((fileNameWithoutExtension == null) || (fileNameWithoutExtension.Trim().Length == 0)) || ((extension == null) || (extension.Trim().Length == 0)))
            {
                error = DR.GetString("ThemePathNotValid", new object[0]);
                control = this.themeLocationTextBox;
                return false;
            }
            if (!extension.Equals("*.wtm".Replace("*", ""), StringComparison.Ordinal))
            {
                error = DR.GetString("ThemeFileNotXml", new object[0]);
                control = this.themeLocationTextBox;
                return false;
            }
            return true;
        }

        public WorkflowTheme ComposedTheme
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.bufferedTheme;
            }
        }

        private bool ShowPreview
        {
            get
            {
                return this.previewShown;
            }
            set
            {
                this.previewShown = value;
                this.previewLabel.Visible = this.previewShown;
                this.designerPreview.Visible = this.previewShown;
                if (this.previewShown)
                {
                    this.themePanel.Width = this.designerPreview.Right + ((this.designerPreview.Left - this.propertiesGrid.Right) / 2);
                    this.previewButton.Text = DR.GetString("Preview", new object[0]) + " <<";
                }
                else
                {
                    this.themePanel.Width = this.themeConfigPanel.Right + this.themeConfigPanel.Left;
                    this.previewButton.Text = DR.GetString("Preview", new object[0]) + " >>";
                }
                base.Width = ((this.themePanel.Right + this.themePanel.Left) + base.Margin.Left) + base.Margin.Right;
                this.themePanel.Invalidate();
            }
        }

        private Font StandardFont
        {
            get
            {
                Font menuFont = SystemInformation.MenuFont;
                if (this.serviceProvider != null)
                {
                    IUIService service = (IUIService) this.serviceProvider.GetService(typeof(IUIService));
                    if (service != null)
                    {
                        menuFont = (Font) service.Styles["DialogFont"];
                    }
                }
                return menuFont;
            }
        }

        internal sealed class DesignerPreview : UserControl
        {
            private ThemeConfigurationDialog parent;
            private PreviewDesignSurface surface;

            internal DesignerPreview(ThemeConfigurationDialog parent)
            {
                this.BackColor = System.Drawing.Color.White;
                this.parent = parent;
            }

            private void AddDummyActivity(CompositeActivity parentActivity, System.Type activityType)
            {
                if (this.surface.GetService(typeof(IDesignerHost)) is IDesignerHost)
                {
                    Activity item = Activator.CreateInstance(activityType) as Activity;
                    if (item != null)
                    {
                        parentActivity.Activities.Add(item);
                        this.EnsureUniqueId(item);
                        WorkflowDesignerLoader.AddActivityToDesigner(this.surface, item);
                    }
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && (this.surface != null))
                {
                    IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if ((service != null) && (service.RootComponent != null))
                    {
                        WorkflowDesignerLoader.RemoveActivityFromDesigner(this.surface, service.RootComponent as Activity);
                    }
                    ReadonlyWorkflow workflow = (base.Controls.Count > 0) ? (base.Controls[0] as ReadonlyWorkflow) : null;
                    base.Controls.Clear();
                    if (workflow != null)
                    {
                        workflow.Dispose();
                        workflow = null;
                    }
                    this.surface.Dispose();
                    this.surface = null;
                }
                base.Dispose(disposing);
            }

            private void EnsureUniqueId(Activity addedActivity)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>();
                Queue<Activity> queue = new Queue<Activity>();
                queue.Enqueue(addedActivity);
                while (queue.Count > 0)
                {
                    Activity activity = queue.Dequeue();
                    string fullName = activity.GetType().FullName;
                    int num = dictionary.ContainsKey(fullName) ? dictionary[fullName] : 1;
                    activity.Name = activity.GetType().Name + num.ToString(CultureInfo.InvariantCulture);
                    num++;
                    if (dictionary.ContainsKey(fullName))
                    {
                        dictionary[fullName] = num;
                    }
                    else
                    {
                        dictionary.Add(fullName, num);
                    }
                    CompositeActivity activity2 = activity as CompositeActivity;
                    if (activity2 != null)
                    {
                        foreach (Activity activity3 in activity2.Activities)
                        {
                            queue.Enqueue(activity3);
                        }
                    }
                }
            }

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);
                base.SuspendLayout();
                this.surface = new PreviewDesignSurface(this.parent.serviceProvider);
                PreviewWorkflowDesignerLoader loader = new PreviewWorkflowDesignerLoader();
                this.surface.BeginLoad(loader);
                IDesignerHost service = this.surface.GetService(typeof(IDesignerHost)) as IDesignerHost;
                Activity activity = service.CreateComponent(System.Type.GetType("System.Workflow.Activities.SequentialWorkflowActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")) as Activity;
                activity.Name = "ThemeSequentialWorkflow";
                WorkflowDesignerLoader.AddActivityToDesigner(this.surface, activity);
                ReadonlyWorkflow workflow = new ReadonlyWorkflow(this.parent, this.surface) {
                    TabStop = false,
                    Dock = DockStyle.Fill
                };
                base.Controls.Add(workflow);
                service.Activate();
                base.ResumeLayout(true);
            }

            internal IDesigner UpdatePreview(System.Type activityType)
            {
                bool flag = false;
                IDesignerHost host = this.surface.GetService(typeof(IDesignerHost)) as IDesignerHost;
                CompositeActivity rootComponent = host.RootComponent as CompositeActivity;
                if ((host == null) || (rootComponent == null))
                {
                    return null;
                }
                IComponent component = null;
                try
                {
                    while (rootComponent.Activities.Count > 0)
                    {
                        Activity item = rootComponent.Activities[0];
                        rootComponent.Activities.Remove(item);
                        WorkflowDesignerLoader.RemoveActivityFromDesigner(this.surface, item);
                    }
                    if ((activityType == null) || activityType.FullName.Equals("System.Workflow.Activities.SequentialWorkflowActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", StringComparison.OrdinalIgnoreCase))
                    {
                        this.AddDummyActivity(rootComponent, System.Type.GetType("System.Workflow.Activities.CodeActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
                        flag = true;
                    }
                    else
                    {
                        IComponent[] componentArray = null;
                        object[] customAttributes = activityType.GetCustomAttributes(typeof(ToolboxItemAttribute), false);
                        ToolboxItemAttribute attribute = ((customAttributes != null) && (customAttributes.GetLength(0) > 0)) ? (customAttributes[0] as ToolboxItemAttribute) : null;
                        if (((attribute != null) && (attribute.ToolboxItemType != null)) && typeof(ActivityToolboxItem).IsAssignableFrom(attribute.ToolboxItemType))
                        {
                            componentArray = (Activator.CreateInstance(attribute.ToolboxItemType, new object[] { activityType }) as ActivityToolboxItem).CreateComponents(host);
                        }
                        if (componentArray == null)
                        {
                            componentArray = new IComponent[] { Activator.CreateInstance(activityType) as IComponent };
                        }
                        Activity activity3 = ((componentArray != null) && (componentArray.Length > 0)) ? (componentArray[0] as Activity) : null;
                        if (activity3 != null)
                        {
                            rootComponent.Activities.Add(activity3);
                            this.EnsureUniqueId(activity3);
                            WorkflowDesignerLoader.AddActivityToDesigner(this.surface, activity3);
                            CompositeActivityDesigner designer = host.GetDesigner(rootComponent) as CompositeActivityDesigner;
                            ActivityDesigner containedDesigner = host.GetDesigner(activity3) as ActivityDesigner;
                            if ((designer != null) && (containedDesigner != null))
                            {
                                designer.EnsureVisibleContainedDesigner(containedDesigner);
                            }
                        }
                    }
                    ISelectionService service = host.GetService(typeof(ISelectionService)) as ISelectionService;
                    if (service != null)
                    {
                        service.SetSelectedComponents(new IComponent[] { rootComponent });
                    }
                    ReadonlyWorkflow workflow = (base.Controls.Count > 0) ? (base.Controls[0] as ReadonlyWorkflow) : null;
                    if (workflow != null)
                    {
                        workflow.PerformLayout();
                    }
                    component = ((rootComponent.Activities.Count > 0) && !flag) ? rootComponent.Activities[0] : rootComponent;
                }
                catch
                {
                }
                if (component == null)
                {
                    return null;
                }
                return host.GetDesigner(component);
            }

            private sealed class PreviewDesignSurface : DesignSurface
            {
                internal PreviewDesignSurface(IServiceProvider parentProvider) : base(new PreviewDesignerServiceProvider(parentProvider))
                {
                    if (!(base.GetService(typeof(ITypeProvider)) is ITypeProvider))
                    {
                        TypeProvider serviceInstance = new TypeProvider(this);
                        serviceInstance.AddAssemblyReference(typeof(string).Assembly.Location);
                        base.ServiceContainer.AddService(typeof(ITypeProvider), serviceInstance, true);
                    }
                }

                protected override IDesigner CreateDesigner(IComponent component, bool rootDesigner)
                {
                    IDesigner designer = base.CreateDesigner(component, rootDesigner);
                    Activity activity = component as Activity;
                    if (((designer == null) && !rootDesigner) && (activity != null))
                    {
                        designer = ActivityDesigner.CreateDesigner(activity.Site, activity);
                    }
                    return designer;
                }

                private sealed class PreviewDesignerServiceProvider : IServiceProvider
                {
                    private IServiceProvider serviceProvider;

                    internal PreviewDesignerServiceProvider(IServiceProvider serviceProvider)
                    {
                        this.serviceProvider = serviceProvider;
                    }

                    object IServiceProvider.GetService(System.Type serviceType)
                    {
                        if (serviceType == typeof(IPropertyValueUIService))
                        {
                            return null;
                        }
                        return this.serviceProvider.GetService(serviceType);
                    }
                }
            }

            private class PreviewWorkflowDesignerLoader : WorkflowDesignerLoader
            {
                public override TextReader GetFileReader(string filePath)
                {
                    return null;
                }

                public override TextWriter GetFileWriter(string filePath)
                {
                    return null;
                }

                public override string FileName
                {
                    get
                    {
                        return string.Empty;
                    }
                }
            }

            private class ReadonlyWorkflow : WorkflowView
            {
                private ThemeConfigurationDialog themeConfigDialog;

                internal ReadonlyWorkflow(ThemeConfigurationDialog themeConfigDialog, IServiceProvider serviceProvider) : base(serviceProvider)
                {
                    this.themeConfigDialog = themeConfigDialog;
                    this.themeConfigDialog.propertiesGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(this.OnThemePropertyChanged);
                    base.EnableFitToScreen = false;
                    base.AddDesignerMessageFilter(new ReadonlyMessageFilter());
                }

                protected override void Dispose(bool disposing)
                {
                    base.Dispose(disposing);
                    if ((this.themeConfigDialog != null) && (this.themeConfigDialog.propertiesGrid != null))
                    {
                        this.themeConfigDialog.propertiesGrid.PropertyValueChanged -= new PropertyValueChangedEventHandler(this.OnThemePropertyChanged);
                    }
                }

                protected override void OnLayout(LayoutEventArgs levent)
                {
                    if (this.themeConfigDialog != null)
                    {
                        using (new BufferedTheme(this.themeConfigDialog.bufferedTheme))
                        {
                            base.OnLayout(levent);
                        }
                        Size extent = base.ActiveLayout.Extent;
                        Size size = base.Size;
                        PointF tf = new PointF(((float) size.Width) / ((float) extent.Width), ((float) size.Height) / ((float) extent.Height));
                        base.Zoom = Convert.ToInt32((float) (Math.Min(tf.X, tf.Y) * 100f));
                    }
                }

                protected override void OnPaint(PaintEventArgs e)
                {
                    if (this.themeConfigDialog == null)
                    {
                        base.OnPaint(e);
                    }
                    else
                    {
                        using (new BufferedTheme(this.themeConfigDialog.bufferedTheme))
                        {
                            base.OnPaint(e);
                        }
                    }
                }

                private void OnThemePropertyChanged(object sender, PropertyValueChangedEventArgs e)
                {
                    if (this.themeConfigDialog != null)
                    {
                        using (new BufferedTheme(this.themeConfigDialog.bufferedTheme))
                        {
                            base.OnThemeChange(WorkflowTheme.CurrentTheme, EventArgs.Empty);
                        }
                    }
                }

                private sealed class BufferedTheme : IDisposable
                {
                    private WorkflowTheme oldTheme;

                    internal BufferedTheme(WorkflowTheme themeToApply)
                    {
                        if ((themeToApply != null) && (WorkflowTheme.CurrentTheme != themeToApply))
                        {
                            WorkflowTheme.EnableChangeNotification = false;
                            this.oldTheme = WorkflowTheme.CurrentTheme;
                            WorkflowTheme.CurrentTheme = themeToApply;
                        }
                    }

                    void IDisposable.Dispose()
                    {
                        if ((this.oldTheme != null) && (WorkflowTheme.CurrentTheme != this.oldTheme))
                        {
                            WorkflowTheme.CurrentTheme.ReadOnly = false;
                            WorkflowTheme.CurrentTheme = this.oldTheme;
                            WorkflowTheme.EnableChangeNotification = true;
                        }
                    }
                }
            }
        }

        private static class ThemeConfigHelpers
        {
            internal static void EnsureDesignerThemes(IServiceProvider serviceProvider, WorkflowTheme workflowTheme, TreeNode[] items)
            {
                foreach (TreeNode node in items)
                {
                    System.Type tag = node.Tag as System.Type;
                    if (tag != null)
                    {
                        System.Type designerBaseType = tag.FullName.Equals("System.Workflow.Activities.SequentialWorkflowActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", StringComparison.OrdinalIgnoreCase) ? typeof(IRootDesigner) : typeof(IDesigner);
                        System.Type designerType = ActivityDesigner.GetDesignerType(serviceProvider, tag, designerBaseType);
                        if (designerType != null)
                        {
                            workflowTheme.GetTheme(designerType);
                        }
                    }
                }
            }

            internal static TreeNode[] GetAllTreeNodes(TreeView treeView)
            {
                List<TreeNode> list = new List<TreeNode>();
                Queue<TreeNodeCollection> queue = new Queue<TreeNodeCollection>();
                queue.Enqueue(treeView.Nodes);
                while (queue.Count > 0)
                {
                    foreach (TreeNode node in queue.Dequeue())
                    {
                        list.Add(node);
                        if (node.Nodes.Count > 0)
                        {
                            queue.Enqueue(node.Nodes);
                        }
                    }
                }
                return list.ToArray();
            }

            internal static TreeNode GetCatagoryNodeForDesigner(System.Type designerType, TreeNode[] treeNodes)
            {
                if (designerType == null)
                {
                    throw new ArgumentNullException("designerType");
                }
                if (treeNodes == null)
                {
                    throw new ArgumentNullException("treeNodes");
                }
                if (treeNodes.Length == 0)
                {
                    throw new ArgumentException(SR.GetString("Error_InvalidArgumentValue"), "treeNodes");
                }
                CategoryAttribute attribute = null;
                CategoryAttribute attribute2 = null;
                for (System.Type type = designerType; (type != typeof(object)) && (attribute == null); type = type.BaseType)
                {
                    object[] customAttributes = type.GetCustomAttributes(typeof(CategoryAttribute), false);
                    if ((customAttributes != null) && (customAttributes.GetLength(0) > 0))
                    {
                        if (attribute2 == null)
                        {
                            attribute2 = customAttributes[0] as CategoryAttribute;
                        }
                        else
                        {
                            attribute = customAttributes[0] as CategoryAttribute;
                        }
                    }
                }
                if (attribute2 == null)
                {
                    return null;
                }
                TreeNode node = null;
                TreeNode node2 = treeNodes[0];
                foreach (TreeNode node3 in treeNodes)
                {
                    if (((attribute != null) && (attribute.Category == node3.Text)) && ((node3.Tag == null) || !typeof(Activity).IsAssignableFrom(node3.Tag.GetType())))
                    {
                        node2 = node3;
                    }
                    if ((attribute2.Category == node3.Text) && ((node3.Tag == null) || !typeof(Activity).IsAssignableFrom(node3.Tag.GetType())))
                    {
                        node = node3;
                        break;
                    }
                }
                if ((node == null) && (node2 != null))
                {
                    node = new TreeNode(attribute2.Category);
                    node2.Nodes.Add(node);
                }
                return node;
            }

            internal static DesignerTheme[] GetDesignerThemes(IServiceProvider serviceProvider, WorkflowTheme workflowTheme, TreeNode selectedNode)
            {
                ArrayList list = new ArrayList();
                Queue<TreeNode> queue = new Queue<TreeNode>();
                queue.Enqueue(selectedNode);
                while (queue.Count > 0)
                {
                    TreeNode node = queue.Dequeue();
                    System.Type tag = node.Tag as System.Type;
                    if (tag != null)
                    {
                        System.Type designerBaseType = tag.FullName.Equals("System.Workflow.Activities.SequentialWorkflowActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", StringComparison.OrdinalIgnoreCase) ? typeof(IRootDesigner) : typeof(IDesigner);
                        System.Type designerType = ActivityDesigner.GetDesignerType(serviceProvider, tag, designerBaseType);
                        if (designerType != null)
                        {
                            DesignerTheme theme = workflowTheme.GetTheme(designerType);
                            if (theme != null)
                            {
                                list.Add(theme);
                            }
                        }
                    }
                    else
                    {
                        foreach (TreeNode node2 in node.Nodes)
                        {
                            queue.Enqueue(node2);
                        }
                        continue;
                    }
                }
                return (DesignerTheme[]) list.ToArray(typeof(DesignerTheme));
            }

            internal static void PopulateActivities(IServiceProvider serviceProvider, TreeView treeView)
            {
                List<System.Type> list = new List<System.Type>();
                foreach (string str in new List<string> { "System.Workflow.Activities.SequentialWorkflowActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "System.Workflow.Activities.StateMachineWorkflowActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "System.Workflow.Activities.IfElseBranchActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", typeof(FaultHandlersActivity).AssemblyQualifiedName, "System.Workflow.Activities.EventHandlersActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", typeof(CompensationHandlerActivity).AssemblyQualifiedName, typeof(CancellationHandlerActivity).AssemblyQualifiedName })
                {
                    System.Type type = System.Type.GetType(str, false);
                    if (type != null)
                    {
                        list.Add(type);
                    }
                }
                IList<System.Type> list3 = new List<System.Type>();
                treeView.BeginUpdate();
                treeView.Nodes.Clear();
                TreeNode node = new TreeNode(DR.GetString("WorkflowDesc", new object[0]));
                treeView.Nodes.Add(node);
                IToolboxService service = serviceProvider.GetService(typeof(IToolboxService)) as IToolboxService;
                ITypeProviderCreator creator = serviceProvider.GetService(typeof(ITypeProviderCreator)) as ITypeProviderCreator;
                if ((service != null) && (creator != null))
                {
                    foreach (ToolboxItem item in service.GetToolboxItems())
                    {
                        bool flag = item is ActivityToolboxItem;
                        if (!flag)
                        {
                            foreach (ToolboxItemFilterAttribute attribute in item.Filter)
                            {
                                if (attribute.FilterString.StartsWith("Microsoft.Workflow.VSDesigner", StringComparison.OrdinalIgnoreCase) || attribute.FilterString.StartsWith("System.Workflow.ComponentModel", StringComparison.OrdinalIgnoreCase))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (flag)
                        {
                            System.Type type2 = null;
                            Assembly transientAssembly = creator.GetTransientAssembly(item.AssemblyName);
                            if (transientAssembly != null)
                            {
                                type2 = transientAssembly.GetType(item.TypeName);
                            }
                            if (type2 != null)
                            {
                                foreach (ConstructorInfo info in type2.GetConstructors())
                                {
                                    if (info.IsPublic && (info.GetParameters().GetLength(0) == 0))
                                    {
                                        list.Add(type2);
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (System.Type type3 in list)
                {
                    System.Type designerBaseType = type3.FullName.Equals("System.Workflow.Activities.SequentialWorkflowActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", StringComparison.OrdinalIgnoreCase) ? typeof(IRootDesigner) : typeof(IDesigner);
                    System.Type type5 = ActivityDesigner.GetDesignerType(serviceProvider, type3, designerBaseType);
                    if ((type5 != null) && !list3.Contains(type5))
                    {
                        object[] customAttributes = type5.GetCustomAttributes(typeof(ActivityDesignerThemeAttribute), true);
                        if ((((customAttributes != null) && (customAttributes.GetLength(0) > 0)) ? (customAttributes[0] as ActivityDesignerThemeAttribute) : null) != null)
                        {
                            Image toolboxImage = ActivityToolboxItem.GetToolboxImage(type3);
                            if (treeView.ImageList == null)
                            {
                                treeView.ImageList = new ImageList();
                                treeView.ImageList.ColorDepth = ColorDepth.Depth32Bit;
                                Image image = DR.GetImage("Activity");
                                treeView.ImageList.Images.Add(image, AmbientTheme.TransparentColor);
                            }
                            TreeNode catagoryNodeForDesigner = GetCatagoryNodeForDesigner(type5, GetAllTreeNodes(treeView));
                            if (catagoryNodeForDesigner != null)
                            {
                                int imageIndex = (toolboxImage != null) ? treeView.ImageList.Images.Add(toolboxImage, AmbientTheme.TransparentColor) : 0;
                                TreeNode node3 = (imageIndex >= 0) ? new TreeNode(ActivityToolboxItem.GetToolboxDisplayName(type3), imageIndex, imageIndex) : new TreeNode(ActivityToolboxItem.GetToolboxDisplayName(type3));
                                node3.Tag = type3;
                                int index = catagoryNodeForDesigner.Nodes.Count - 1;
                                while ((index >= 0) && (catagoryNodeForDesigner.Nodes[index].Tag is System.Type))
                                {
                                    index--;
                                }
                                catagoryNodeForDesigner.Nodes.Insert(index, node3);
                            }
                        }
                    }
                }
                treeView.TreeViewNodeSorter = new ThemeConfigurationDialog.ThemeTreeNodeComparer();
                treeView.Sort();
                treeView.Nodes[0].ExpandAll();
                treeView.EndUpdate();
            }
        }

        internal sealed class ThemeTreeNodeComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                TreeNode node = x as TreeNode;
                TreeNode node2 = y as TreeNode;
                if (node.Nodes.Count > node2.Nodes.Count)
                {
                    return 1;
                }
                return string.Compare(node.Text, node2.Text, StringComparison.CurrentCulture);
            }
        }
    }
}

