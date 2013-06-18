namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Interop;

    public sealed class TypeBrowserDialog : Form, ISite, IServiceProvider
    {
        private TabPage advancedTabPage;
        private ImageList artifactImages;
        private TextBox artifactLabel;
        private ListView artifactListView;
        private TreeView artifactTreeView;
        private IntPtr bitmapSortDown;
        private IntPtr bitmapSortUp;
        private Button buttonBrowse;
        private Button buttonCancel;
        private Button buttonOK;
        private ColumnHeader fullyQualifiedName;
        private GenericParameters genericParameters;
        private PropertyGrid genericParametersPropertyGrid;
        private TextBox helpTextHolder;
        private HelpTextWindow helpTextWindow;
        private string lastComboboxValue;
        private System.Workflow.ComponentModel.Compiler.TypeProvider localTypeProvider;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private bool refreshTreeView;
        private bool refreshTypeTextBox;
        private static ResourceManager ResMgr = new ResourceManager("System.Workflow.ComponentModel.Design.ArtifactReference", Assembly.GetExecutingAssembly());
        private System.Type selectedType;
        private string selectedTypeName;
        private IServiceProvider serviceProvider;
        private bool sortListViewAscending;
        private System.Windows.Forms.TabControl tabControl;
        private ITypeFilterProvider typeFilterProvider;
        private ColumnHeader typeName;
        private Label typeNameLabel;
        private TableLayoutPanel typeNameTableLayoutPanel;
        private SplitContainer typeSplitContainer;
        private TabPage typeTabPage;
        private TextBox typeTextBox;

        public TypeBrowserDialog(IServiceProvider serviceProvider, ITypeFilterProvider filterProvider, string selectedTypeName)
        {
            this.genericParameters = new GenericParameters();
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            this.InitializeDialog(serviceProvider, filterProvider, selectedTypeName);
        }

        public TypeBrowserDialog(IServiceProvider serviceProvider, ITypeFilterProvider filterProvider, string selectedTypeName, System.Workflow.ComponentModel.Compiler.TypeProvider typeProvider)
        {
            this.genericParameters = new GenericParameters();
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            this.localTypeProvider = typeProvider;
            this.serviceProvider = serviceProvider;
            Helpers.AddTypeProviderAssembliesFromRegistry(this.localTypeProvider, serviceProvider);
            this.InitializeDialog(serviceProvider, filterProvider, selectedTypeName);
            this.buttonBrowse.Visible = true;
            this.buttonBrowse.Enabled = true;
            this.buttonBrowse.BringToFront();
        }

        private void CustomInitializeComponent()
        {
            base.SuspendLayout();
            this.artifactTreeView.Sorted = true;
            this.bitmapSortUp = (ResMgr.GetObject("IDB_SORTUP") as Bitmap).GetHbitmap();
            this.bitmapSortDown = (ResMgr.GetObject("IDB_SORTDOWN") as Bitmap).GetHbitmap();
            this.artifactImages = new ImageList();
            this.artifactImages.TransparentColor = Color.FromArgb(0, 0xff, 0);
            this.artifactImages.Images.AddStrip((Image) ResMgr.GetObject("IDB_ARTIFACTIMAGES"));
            this.artifactListView.Dock = DockStyle.Fill;
            this.artifactListView.FullRowSelect = true;
            this.artifactListView.HideSelection = false;
            this.artifactListView.MultiSelect = false;
            this.artifactListView.SmallImageList = this.artifactImages;
            this.artifactListView.MouseDown += new MouseEventHandler(this.OnListViewMouseDown);
            this.artifactTreeView.HideSelection = false;
            this.artifactTreeView.ImageList = this.artifactImages;
            this.helpTextHolder.Visible = false;
            this.helpTextWindow = new HelpTextWindow();
            this.helpTextWindow.Parent = this;
            this.helpTextWindow.Location = new Point(this.helpTextHolder.Location.X + 3, this.helpTextHolder.Location.Y + 3);
            this.helpTextWindow.Size = new Size(this.helpTextHolder.Size.Width - 6, this.helpTextHolder.Size.Height - 6);
            if (this.typeFilterProvider != null)
            {
                this.artifactLabel.Text = this.typeFilterProvider.FilterDescription;
            }
            IUIService service = (IUIService) this.serviceProvider.GetService(typeof(IUIService));
            if (service != null)
            {
                this.Font = (Font) service.Styles["DialogFont"];
            }
            this.artifactLabel.Font = new Font(this.Font.Name, this.Font.SizeInPoints, FontStyle.Bold);
            this.genericParametersPropertyGrid.SelectedObject = this.genericParameters;
            this.genericParametersPropertyGrid.Site = this;
            this.genericParametersPropertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(this.GenericParameterChanged);
            base.ResumeLayout(false);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private void GenericParameterChanged(object sender, PropertyValueChangedEventArgs e)
        {
            bool flag = true;
            foreach (ParameterData data in this.genericParameters.Parameters)
            {
                if (data.Type == null)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                this.UpdateTypeTextBox();
            }
        }

        private void GetHelp()
        {
            DesignerHelpers.ShowHelpFromKeyword(this.serviceProvider, typeof(TypeBrowserDialog).FullName + ".UI");
        }

        private string GetSimpleTypeFullName(System.Type type)
        {
            StringBuilder builder = new StringBuilder(type.FullName);
            Stack<System.Type> stack = new Stack<System.Type>();
            stack.Push(type);
            while (stack.Count > 0)
            {
                type = stack.Pop();
                while (type.IsArray)
                {
                    type = type.GetElementType();
                }
                if (type.IsGenericType && !type.IsGenericTypeDefinition)
                {
                    foreach (System.Type type2 in type.GetGenericArguments())
                    {
                        builder.Replace("[" + type2.AssemblyQualifiedName + "]", type2.FullName);
                        stack.Push(type2);
                    }
                }
            }
            return builder.ToString();
        }

        private List<System.Type> GetTargetFrameworkTypes(ITypeProvider currentTypeProvider)
        {
            IExtendedUIService2 service = (IExtendedUIService2) this.serviceProvider.GetService(typeof(IExtendedUIService2));
            List<System.Type> list = new List<System.Type>();
            if (currentTypeProvider != null)
            {
                if (service != null)
                {
                    List<Assembly> list2 = new List<Assembly>(currentTypeProvider.ReferencedAssemblies);
                    foreach (Assembly assembly in list2)
                    {
                        Assembly reflectionAssembly = service.GetReflectionAssembly(assembly.GetName());
                        if (reflectionAssembly != null)
                        {
                            foreach (System.Type type in reflectionAssembly.GetTypes())
                            {
                                if (type.IsPublic)
                                {
                                    list.Add(type);
                                }
                            }
                        }
                    }
                    foreach (System.Type type2 in currentTypeProvider.GetTypes())
                    {
                        if (type2.Assembly == null)
                        {
                            list.Add(type2);
                        }
                    }
                    return list;
                }
                list.AddRange(currentTypeProvider.GetTypes());
            }
            return list;
        }

        private void GetTypeParts(System.Type type, out System.Type baseType, out ParameterData[] parameterDataArray, out int[] arrayRanks)
        {
            baseType = null;
            parameterDataArray = null;
            arrayRanks = null;
            if (type.IsArray)
            {
                ArrayList list = new ArrayList();
                this.GetTypeParts(type.GetElementType(), out baseType, out parameterDataArray, out arrayRanks);
                if (arrayRanks != null)
                {
                    list.AddRange(arrayRanks);
                }
                list.Add(type.GetArrayRank());
                arrayRanks = (int[]) list.ToArray(typeof(int));
            }
            else if (type.IsGenericType)
            {
                System.Type underlyingSystemType = null;
                System.Type type3 = null;
                if (type.ContainsGenericParameters)
                {
                    type3 = null;
                    underlyingSystemType = type.UnderlyingSystemType;
                }
                else
                {
                    type3 = type;
                    underlyingSystemType = type.GetGenericTypeDefinition().UnderlyingSystemType;
                }
                ArrayList list2 = new ArrayList();
                for (int i = 0; i < underlyingSystemType.GetGenericArguments().Length; i++)
                {
                    ParameterData data = new ParameterData {
                        ParameterType = underlyingSystemType.GetGenericArguments()[i]
                    };
                    if (type3 != null)
                    {
                        data.Type = type.GetGenericArguments()[i];
                    }
                    list2.Add(data);
                }
                parameterDataArray = (ParameterData[]) list2.ToArray(typeof(ParameterData));
                baseType = underlyingSystemType;
            }
            else
            {
                baseType = type;
            }
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(TypeBrowserDialog));
            this.buttonCancel = new Button();
            this.helpTextHolder = new TextBox();
            this.buttonOK = new Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.typeTabPage = new TabPage();
            this.typeSplitContainer = new SplitContainer();
            this.artifactTreeView = new TreeView();
            this.artifactListView = new ListView();
            this.typeName = new ColumnHeader();
            this.fullyQualifiedName = new ColumnHeader();
            this.advancedTabPage = new TabPage();
            this.genericParametersPropertyGrid = new PropertyGrid();
            this.buttonBrowse = new Button();
            this.typeTextBox = new TextBox();
            this.typeNameLabel = new Label();
            this.okCancelTableLayoutPanel = new TableLayoutPanel();
            this.typeNameTableLayoutPanel = new TableLayoutPanel();
            this.artifactLabel = new TextBox();
            this.tabControl.SuspendLayout();
            this.typeTabPage.SuspendLayout();
            this.typeSplitContainer.Panel1.SuspendLayout();
            this.typeSplitContainer.Panel2.SuspendLayout();
            this.typeSplitContainer.SuspendLayout();
            this.advancedTabPage.SuspendLayout();
            this.okCancelTableLayoutPanel.SuspendLayout();
            this.typeNameTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            manager.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            manager.ApplyResources(this.helpTextHolder, "helpTextHolder");
            this.helpTextHolder.BorderStyle = BorderStyle.FixedSingle;
            this.helpTextHolder.Name = "helpTextHolder";
            this.helpTextHolder.ReadOnly = true;
            manager.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Click += new EventHandler(this.OkButtonClicked);
            manager.ApplyResources(this.tabControl, "tabControl");
            this.tabControl.Controls.Add(this.typeTabPage);
            this.tabControl.Controls.Add(this.advancedTabPage);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.typeTabPage.BackColor = Color.Transparent;
            this.typeTabPage.Controls.Add(this.typeSplitContainer);
            manager.ApplyResources(this.typeTabPage, "typeTabPage");
            this.typeTabPage.Name = "typeTabPage";
            this.typeSplitContainer.BackColor = Color.Transparent;
            manager.ApplyResources(this.typeSplitContainer, "typeSplitContainer");
            this.typeSplitContainer.Name = "typeSplitContainer";
            this.typeSplitContainer.Panel1.Controls.Add(this.artifactTreeView);
            this.typeSplitContainer.Panel2.Controls.Add(this.artifactListView);
            this.typeSplitContainer.TabStop = false;
            this.artifactTreeView.BackColor = SystemColors.Window;
            manager.ApplyResources(this.artifactTreeView, "artifactTreeView");
            this.artifactTreeView.ItemHeight = 0x10;
            this.artifactTreeView.Name = "artifactTreeView";
            this.artifactTreeView.AfterSelect += new TreeViewEventHandler(this.OnTreeSelectionChange);
            this.artifactTreeView.GotFocus += new EventHandler(this.OnTreeViewGotFocus);
            this.artifactListView.AllowColumnReorder = true;
            this.artifactListView.Columns.AddRange(new ColumnHeader[] { this.typeName, this.fullyQualifiedName });
            manager.ApplyResources(this.artifactListView, "artifactListView");
            this.artifactListView.Name = "artifactListView";
            this.artifactListView.UseCompatibleStateImageBehavior = false;
            this.artifactListView.View = View.Details;
            this.artifactListView.SelectedIndexChanged += new EventHandler(this.OnListViewSelectedIndexChanged);
            this.artifactListView.ColumnClick += new ColumnClickEventHandler(this.OnListViewColumnClick);
            manager.ApplyResources(this.typeName, "typeName");
            manager.ApplyResources(this.fullyQualifiedName, "fullyQualifiedName");
            this.advancedTabPage.BackColor = Color.Transparent;
            this.advancedTabPage.Controls.Add(this.genericParametersPropertyGrid);
            manager.ApplyResources(this.advancedTabPage, "advancedTabPage");
            this.advancedTabPage.Name = "advancedTabPage";
            manager.ApplyResources(this.genericParametersPropertyGrid, "genericParametersPropertyGrid");
            this.genericParametersPropertyGrid.Name = "genericParametersPropertyGrid";
            this.genericParametersPropertyGrid.PropertySort = PropertySort.Categorized;
            this.genericParametersPropertyGrid.ToolbarVisible = false;
            manager.ApplyResources(this.buttonBrowse, "buttonBrowse");
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Click += new EventHandler(this.OnButtonBrowse_Click);
            manager.ApplyResources(this.typeTextBox, "typeTextBox");
            this.typeTextBox.Name = "typeTextBox";
            this.typeTextBox.TextChanged += new EventHandler(this.OnTypeTextBoxTextChanged);
            manager.ApplyResources(this.typeNameLabel, "typeNameLabel");
            this.typeNameLabel.Name = "typeNameLabel";
            manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.Controls.Add(this.buttonOK, 0, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.buttonCancel, 1, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            manager.ApplyResources(this.typeNameTableLayoutPanel, "typeNameTableLayoutPanel");
            this.typeNameTableLayoutPanel.Controls.Add(this.typeNameLabel, 0, 0);
            this.typeNameTableLayoutPanel.Controls.Add(this.typeTextBox, 1, 0);
            this.typeNameTableLayoutPanel.Name = "typeNameTableLayoutPanel";
            this.artifactLabel.BorderStyle = BorderStyle.None;
            this.artifactLabel.CausesValidation = false;
            manager.ApplyResources(this.artifactLabel, "artifactLabel");
            this.artifactLabel.Name = "artifactLabel";
            this.artifactLabel.ReadOnly = true;
            this.artifactLabel.TabStop = false;
            base.AcceptButton = this.buttonOK;
            manager.ApplyResources(this, "$this");
            base.CancelButton = this.buttonCancel;
            base.Controls.Add(this.artifactLabel);
            base.Controls.Add(this.typeNameTableLayoutPanel);
            base.Controls.Add(this.okCancelTableLayoutPanel);
            base.Controls.Add(this.buttonBrowse);
            base.Controls.Add(this.helpTextHolder);
            base.Controls.Add(this.tabControl);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.HelpButton = true;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "TypeBrowserDialog";
            base.ShowInTaskbar = false;
            base.HelpButtonClicked += new CancelEventHandler(this.TypeBrowserDialog_HelpButtonClicked);
            this.tabControl.ResumeLayout(false);
            this.typeTabPage.ResumeLayout(false);
            this.typeSplitContainer.Panel1.ResumeLayout(false);
            this.typeSplitContainer.Panel2.ResumeLayout(false);
            this.typeSplitContainer.ResumeLayout(false);
            this.advancedTabPage.ResumeLayout(false);
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            this.okCancelTableLayoutPanel.PerformLayout();
            this.typeNameTableLayoutPanel.ResumeLayout(false);
            this.typeNameTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        internal void InitializeDialog(IServiceProvider serviceProvider, ITypeFilterProvider filterProvider, string selectedTypeName)
        {
            this.serviceProvider = serviceProvider;
            this.sortListViewAscending = true;
            this.refreshTreeView = false;
            this.refreshTypeTextBox = false;
            this.selectedTypeName = selectedTypeName;
            this.typeFilterProvider = filterProvider;
            IDesignerHost service = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            WorkflowDesignerLoader loader = this.serviceProvider.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
            if (((service == null) || (service.RootComponent == null)) || ((loader == null) || loader.InDebugMode))
            {
                throw new Exception(DR.GetString("Error_WorkflowNotLoaded", new object[0]));
            }
            this.InitializeComponent();
            this.CustomInitializeComponent();
            this.genericParametersPropertyGrid.Site = new DummySite(this.serviceProvider);
        }

        private void ListSelectionChanged(System.Type selectedType)
        {
            if (!this.artifactTreeView.Focused)
            {
                string fullName = selectedType.FullName;
                string name = string.Empty;
                if (selectedType.Assembly != null)
                {
                    name = selectedType.Assembly.GetName().Name;
                }
                TreeNode node = null;
                if (name.Length == 0)
                {
                    node = this.artifactTreeView.Nodes["CurrentProject"];
                }
                else
                {
                    TreeNode node2 = this.artifactTreeView.Nodes["ReferencedAssemblies"];
                    foreach (TreeNode node3 in node2.Nodes)
                    {
                        Assembly tag = node3.Tag as Assembly;
                        if ((tag.FullName == name) || (tag.GetName().Name == name))
                        {
                            node = node3;
                            break;
                        }
                    }
                }
                TreeNode node4 = null;
                if (node != null)
                {
                    string str3 = string.Empty;
                    int length = fullName.LastIndexOf('.');
                    if (length != -1)
                    {
                        str3 = fullName.Substring(0, length);
                    }
                    if (str3.Length > 0)
                    {
                        foreach (TreeNode node5 in node.Nodes)
                        {
                            if (node5.Text == str3)
                            {
                                node4 = node5;
                                break;
                            }
                        }
                    }
                }
                TreeNode node6 = node4;
                if (node6 == null)
                {
                    node6 = node;
                }
                if ((node6 != null) && this.artifactTreeView.CanFocus)
                {
                    this.artifactTreeView.SelectedNode = node6;
                    node6.EnsureVisible();
                }
            }
        }

        private void OkButtonClicked(object sender, EventArgs e)
        {
            try
            {
                System.Type type = this.TypeProvider.GetType(this.typeTextBox.Text);
                if ((type != null) && ((this.typeFilterProvider == null) || this.typeFilterProvider.CanFilterType(type, false)))
                {
                    this.selectedTypeName = type.AssemblyQualifiedName;
                    this.selectedType = type;
                    base.DialogResult = DialogResult.OK;
                }
                else
                {
                    base.DialogResult = DialogResult.None;
                }
            }
            catch (Exception exception)
            {
                DesignerHelpers.ShowError(this.serviceProvider, exception);
            }
        }

        private void OnButtonBrowse_Click(object Sender, EventArgs e)
        {
            EventHandler handler2 = null;
            OpenFileDialog fileDialog = new OpenFileDialog {
                Title = DR.GetString("OpenfileDialogTitle", new object[0]),
                AddExtension = true,
                DefaultExt = "dll",
                CheckFileExists = true,
                CheckPathExists = true,
                DereferenceLinks = true,
                ValidateNames = true,
                Filter = DR.GetString("PackageAssemblyReferenceFilter", new object[0]),
                RestoreDirectory = false
            };
            if (fileDialog.ShowDialog(this) == DialogResult.OK)
            {
                if (handler2 == null)
                {
                    handler2 = delegate (object sender, EventArgs ea) {
                        Exception exception = null;
                        if (this.localTypeProvider.TypeLoadErrors.ContainsKey(fileDialog.FileName))
                        {
                            exception = this.localTypeProvider.TypeLoadErrors[fileDialog.FileName];
                        }
                        if (exception != null)
                        {
                            string format = ((exception is ReflectionTypeLoadException) || ((exception.InnerException != null) && (exception.InnerException is ReflectionTypeLoadException))) ? DR.GetString("TypeBrowser_UnableToLoadOneOrMoreTypes", new object[0]) : DR.GetString("TypeBrowser_ProblemsLoadingAssembly", new object[0]);
                            format = string.Format(CultureInfo.CurrentCulture, format, new object[] { fileDialog.FileName });
                            DesignerHelpers.ShowError(this.serviceProvider, format);
                        }
                    };
                }
                EventHandler handler = handler2;
                try
                {
                    ITypeProviderCreator service = this.serviceProvider.GetService(typeof(ITypeProviderCreator)) as ITypeProviderCreator;
                    if (service != null)
                    {
                        this.localTypeProvider.AddAssembly(service.GetTransientAssembly(AssemblyName.GetAssemblyName(fileDialog.FileName)));
                    }
                    else
                    {
                        this.localTypeProvider.AddAssemblyReference(fileDialog.FileName);
                    }
                    Helpers.UpdateTypeProviderAssembliesRegistry(fileDialog.FileName);
                    this.localTypeProvider.TypeLoadErrorsChanged += handler;
                    this.UpdateTreeView(this.GetTargetFrameworkTypes(this.localTypeProvider).ToArray(), this.typeTextBox.AutoCompleteCustomSource);
                }
                catch (FileNotFoundException exception)
                {
                    DesignerHelpers.ShowError(this.serviceProvider, exception.Message);
                }
                catch (BadImageFormatException)
                {
                    DesignerHelpers.ShowError(this.serviceProvider, SR.GetString("Error_AssemblyBadImage", new object[] { fileDialog.FileName }));
                }
                catch (FileLoadException)
                {
                    DesignerHelpers.ShowError(this.serviceProvider, SR.GetString("Error_AssemblyBadImage", new object[] { fileDialog.FileName }));
                }
                catch (Exception exception2)
                {
                    DesignerHelpers.ShowError(this.serviceProvider, SR.GetString("Error_AddAssemblyRef", new object[] { fileDialog.FileName, exception2.Message }));
                }
                finally
                {
                    this.localTypeProvider.TypeLoadErrorsChanged -= handler;
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (IntPtr.Zero != this.bitmapSortUp)
            {
                System.Workflow.Interop.NativeMethods.DeleteObject(this.bitmapSortUp);
            }
            if (IntPtr.Zero != this.bitmapSortDown)
            {
                System.Workflow.Interop.NativeMethods.DeleteObject(this.bitmapSortDown);
            }
        }

        protected override void OnHelpRequested(HelpEventArgs e)
        {
            e.Handled = true;
            this.GetHelp();
        }

        private void OnListViewColumnClick(object sender, ColumnClickEventArgs e)
        {
            this.sortListViewAscending = !this.sortListViewAscending;
            this.SortListViewItems(e.Column);
        }

        private void OnListViewMouseDown(object sender, MouseEventArgs mouseArgs)
        {
            if (((mouseArgs.Clicks > 1) && (this.artifactListView.SelectedItems.Count > 0)) && ((this.artifactListView.SelectedItems[0].Tag is System.Type) && this.buttonOK.Enabled))
            {
                this.OkButtonClicked(this.buttonOK, EventArgs.Empty);
            }
        }

        private void OnListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (!this.refreshTypeTextBox && (this.artifactListView.SelectedItems.Count > 0))
                {
                    this.typeTextBox.Text = (this.artifactListView.SelectedItems[0].Tag as System.Type).FullName;
                }
                if (this.artifactListView.SelectedItems.Count > 0)
                {
                    this.artifactListView.SelectedItems[0].EnsureVisible();
                }
                if (this.artifactListView.Focused && (this.artifactListView.SelectedItems.Count != 0))
                {
                    System.Type tag = this.artifactListView.SelectedItems[0].Tag as System.Type;
                    if (tag != null)
                    {
                        this.ListSelectionChanged(tag);
                    }
                }
            }
            catch (Exception exception)
            {
                DesignerHelpers.ShowError(this.serviceProvider, exception);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                TreeNode node = null;
                if (this.localTypeProvider == null)
                {
                    node = this.artifactTreeView.Nodes.Add("CurrentProject", SR.GetString("CurrentProject"), 2, 2);
                }
                TreeNode node2 = this.artifactTreeView.Nodes.Add("ReferencedAssemblies", SR.GetString("ReferencedAssemblies"), 2, 2);
                ITypeProvider typeProvider = this.TypeProvider;
                AutoCompleteStringCollection autoCompleteStringCollection = new AutoCompleteStringCollection();
                this.UpdateTreeView(this.GetTargetFrameworkTypes(this.TypeProvider).ToArray(), autoCompleteStringCollection);
                node2.Expand();
                TreeNode treeNode = (node == null) ? node2 : node;
                this.artifactTreeView.SelectedNode = treeNode;
                this.TreeSelectionChanged(treeNode);
                treeNode.EnsureVisible();
                this.typeTextBox.AutoCompleteMode = AutoCompleteMode.Suggest;
                this.typeTextBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                this.typeTextBox.AutoCompleteCustomSource = autoCompleteStringCollection;
                if (this.selectedTypeName != null)
                {
                    System.Type type = typeProvider.GetType(this.selectedTypeName);
                    if (type != null)
                    {
                        this.typeTextBox.Text = this.GetSimpleTypeFullName(type);
                    }
                }
            }
            catch (FileNotFoundException)
            {
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
            this.UpdateControlState();
            this.typeTextBox.Select();
        }

        protected override void OnPaint(PaintEventArgs paintArgs)
        {
            base.OnPaint(paintArgs);
            Rectangle rectangle = new Rectangle(base.ClientRectangle.Left, this.artifactLabel.Bottom + (((this.typeNameTableLayoutPanel.Top + this.typeTextBox.Top) - this.artifactLabel.Bottom) / 2), base.ClientRectangle.Width, 1);
            paintArgs.Graphics.DrawLine(SystemPens.ControlDark, rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Bottom);
            paintArgs.Graphics.DrawLine(SystemPens.ControlLightLight, rectangle.Left, rectangle.Bottom + 1, rectangle.Right, rectangle.Bottom + 1);
            rectangle = new Rectangle(base.ClientRectangle.Left, this.helpTextHolder.Bottom + (((this.okCancelTableLayoutPanel.Top + this.buttonOK.Top) - this.helpTextHolder.Bottom) / 2), base.ClientRectangle.Width, 1);
            paintArgs.Graphics.DrawLine(SystemPens.ControlDark, rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Bottom);
            paintArgs.Graphics.DrawLine(SystemPens.ControlLightLight, rectangle.Left, rectangle.Bottom + 1, rectangle.Right, rectangle.Bottom + 1);
            paintArgs.Graphics.DrawLine(SystemPens.WindowFrame, this.helpTextHolder.Left - 1, this.helpTextHolder.Top - 1, this.helpTextHolder.Left - 1, this.helpTextHolder.Bottom);
            paintArgs.Graphics.DrawLine(SystemPens.WindowFrame, this.helpTextHolder.Left - 1, this.helpTextHolder.Bottom, this.helpTextHolder.Right, this.helpTextHolder.Bottom);
            paintArgs.Graphics.DrawLine(SystemPens.WindowFrame, this.helpTextHolder.Right, this.helpTextHolder.Bottom, this.helpTextHolder.Right, this.helpTextHolder.Top - 1);
            paintArgs.Graphics.DrawLine(SystemPens.WindowFrame, this.helpTextHolder.Right, this.helpTextHolder.Top - 1, this.helpTextHolder.Left - 1, this.helpTextHolder.Top - 1);
        }

        private void OnTreeSelectionChange(object sender, TreeViewEventArgs e)
        {
            this.TreeSelectionChanged(e.Node);
        }

        private void OnTreeViewGotFocus(object sender, EventArgs e)
        {
            if (this.refreshTreeView)
            {
                this.refreshTreeView = false;
                if (this.artifactTreeView.SelectedNode != null)
                {
                    this.TreeSelectionChanged(this.artifactTreeView.SelectedNode);
                }
            }
        }

        private void OnTypeTextBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            this.OnTypeTextBoxTextChanged(sender, e);
        }

        private void OnTypeTextBoxTextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!this.refreshTypeTextBox)
                {
                    this.refreshTypeTextBox = true;
                    System.Type type = this.TypeProvider.GetType(this.typeTextBox.Text);
                    if (type != null)
                    {
                        this.lastComboboxValue = this.typeTextBox.Text;
                        System.Type baseType = null;
                        ParameterData[] parameterDataArray = null;
                        int[] arrayRanks = null;
                        this.GetTypeParts(type, out baseType, out parameterDataArray, out arrayRanks);
                        this.genericParameters.Parameters = (parameterDataArray != null) ? parameterDataArray : new ParameterData[0];
                        this.genericParametersPropertyGrid.Refresh();
                        this.ListSelectionChanged(baseType);
                        this.genericParametersPropertyGrid.Enabled = baseType.IsGenericTypeDefinition;
                        foreach (ListViewItem item in this.artifactListView.Items)
                        {
                            System.Type tag = item.Tag as System.Type;
                            if ((tag != null) && tag.FullName.Equals(baseType.FullName))
                            {
                                if (!item.Selected)
                                {
                                    item.Selected = true;
                                }
                                break;
                            }
                            item.Selected = false;
                        }
                    }
                    else
                    {
                        if (this.artifactListView.SelectedItems.Count != 0)
                        {
                            this.artifactListView.SelectedItems[0].Selected = false;
                        }
                        this.genericParameters.Parameters = new ParameterData[0];
                        this.genericParametersPropertyGrid.Enabled = false;
                    }
                    this.UpdateControlState();
                    this.refreshTypeTextBox = false;
                }
            }
            catch (Exception exception)
            {
                DesignerHelpers.ShowError(this.serviceProvider, exception);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData == Keys.Enter) && (this.lastComboboxValue != this.typeTextBox.Text))
            {
                this.lastComboboxValue = this.typeTextBox.Text;
                this.typeTextBox.Text = string.Empty;
                this.typeTextBox.Text = this.lastComboboxValue;
                this.typeTextBox.SelectionStart = this.typeTextBox.Text.Length;
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SortListViewItems(int columnIndex)
        {
            if ((columnIndex >= 0) && (columnIndex < this.artifactListView.Columns.Count))
            {
                ListItemComparer comparer = new ListItemComparer(columnIndex == 0, this.sortListViewAscending);
                this.artifactListView.ListViewItemSorter = comparer;
                if (this.artifactListView.SelectedItems.Count > 0)
                {
                    this.artifactListView.SelectedItems[0].EnsureVisible();
                }
                IntPtr hWndHeader = System.Workflow.Interop.NativeMethods.ListView_GetHeader(this.artifactListView.Handle);
                System.Workflow.Interop.NativeMethods.HDITEM hdi = new System.Workflow.Interop.NativeMethods.HDITEM {
                    mask = 20
                };
                for (int i = 0; i < this.artifactListView.Columns.Count; i++)
                {
                    if (System.Workflow.Interop.NativeMethods.Header_GetItem(hWndHeader, i, hdi))
                    {
                        hdi.fmt &= -12289;
                        hdi.hbm = IntPtr.Zero;
                        System.Workflow.Interop.NativeMethods.Header_SetItem(hWndHeader, i, hdi);
                    }
                }
                if (System.Workflow.Interop.NativeMethods.Header_GetItem(hWndHeader, columnIndex, hdi))
                {
                    hdi.mask = 20;
                    hdi.fmt |= 0x3000;
                    hdi.hbm = this.sortListViewAscending ? this.bitmapSortUp : this.bitmapSortDown;
                    System.Workflow.Interop.NativeMethods.Header_SetItem(hWndHeader, columnIndex, hdi);
                }
            }
        }

        object IServiceProvider.GetService(System.Type serviceType)
        {
            object typeProvider = null;
            if (serviceType == typeof(ITypeProvider))
            {
                typeProvider = this.TypeProvider;
            }
            return typeProvider;
        }

        private void TreeSelectionChanged(TreeNode treeNode)
        {
            try
            {
                if (!this.artifactListView.Focused)
                {
                    this.artifactListView.Items.Clear();
                    this.artifactListView.ListViewItemSorter = null;
                    string text = null;
                    ArrayList list = new ArrayList();
                    if (treeNode != this.artifactTreeView.Nodes["CurrentProject"])
                    {
                        if (treeNode == this.artifactTreeView.Nodes["ReferencedAssemblies"])
                        {
                            foreach (TreeNode node in treeNode.Nodes)
                            {
                                list.Add(node.Tag);
                            }
                        }
                        else if (treeNode.Tag is Assembly)
                        {
                            list.Add(treeNode.Tag);
                        }
                        else
                        {
                            if (treeNode.Parent.Tag != null)
                            {
                                list.Add(treeNode.Parent.Tag);
                            }
                            text = treeNode.Text;
                        }
                    }
                    ITypeProvider typeProvider = this.TypeProvider;
                    IExtendedUIService2 service = (IExtendedUIService2) this.serviceProvider.GetService(typeof(IExtendedUIService2));
                    foreach (System.Type type in this.GetTargetFrameworkTypes(typeProvider))
                    {
                        try
                        {
                            object[] customAttributes = type.GetCustomAttributes(typeof(ObsoleteAttribute), false);
                            if ((customAttributes != null) && (customAttributes.Length > 0))
                            {
                                continue;
                            }
                        }
                        catch (Exception)
                        {
                        }
                        if ((((text == null) || (type.Namespace == text)) && (((list.Count == 0) && (type.Assembly == null)) || list.Contains(type.Assembly))) && ((this.typeFilterProvider == null) || this.typeFilterProvider.CanFilterType((service != null) ? service.GetRuntimeType(type) : type, false)))
                        {
                            ListViewItem item = new ListViewItem {
                                Text = type.Name
                            };
                            item.SubItems.Add(type.FullName);
                            item.Tag = type;
                            item.ImageIndex = 0;
                            this.artifactListView.Items.Add(item);
                        }
                    }
                    this.SortListViewItems(0);
                    this.artifactListView.SelectedIndices.Clear();
                    if (this.artifactListView.Items.Count > 0)
                    {
                        this.artifactListView.Items[0].Selected = true;
                    }
                }
            }
            catch (Exception exception)
            {
                DesignerHelpers.ShowError(this.serviceProvider, exception);
            }
        }

        private void TypeBrowserDialog_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.GetHelp();
        }

        private void UpdateControlState()
        {
            ITypeProvider typeProvider = this.TypeProvider;
            System.Type type = null;
            type = typeProvider.GetType(this.typeTextBox.Text);
            if (((null != type) && ((this.typeFilterProvider == null) || this.typeFilterProvider.CanFilterType(type, false))) && !type.IsGenericTypeDefinition)
            {
                this.buttonOK.Enabled = true;
                base.AcceptButton = this.buttonOK;
            }
            else
            {
                this.buttonOK.Enabled = false;
                base.AcceptButton = null;
            }
            this.helpTextWindow.UpdateHelpText(type);
        }

        private void UpdateTreeView(System.Type[] types, AutoCompleteStringCollection autoCompleteStringCollection)
        {
            TreeNode node = this.artifactTreeView.Nodes["ReferencedAssemblies"];
            Hashtable hashtable = new Hashtable();
            IExtendedUIService2 service = (IExtendedUIService2) this.serviceProvider.GetService(typeof(IExtendedUIService2));
            foreach (System.Type type in types)
            {
                if (((this.typeFilterProvider != null) && !this.typeFilterProvider.CanFilterType((service != null) ? service.GetRuntimeType(type) : type, false)) || autoCompleteStringCollection.Contains(type.FullName))
                {
                    continue;
                }
                autoCompleteStringCollection.Add(type.FullName);
                TreeNode node2 = null;
                if (type.Assembly != null)
                {
                    node2 = hashtable[type.Assembly] as TreeNode;
                    if (node2 == null)
                    {
                        node2 = new TreeNode(type.Assembly.GetName().Name, 3, 3) {
                            Tag = type.Assembly
                        };
                        node.Nodes.Add(node2);
                        hashtable[type.Assembly] = node2;
                    }
                }
                else
                {
                    node2 = this.artifactTreeView.Nodes["CurrentProject"];
                }
                if ((type.Namespace != null) && (type.Namespace.Length > 0))
                {
                    bool flag = false;
                    string text = type.Namespace;
                    foreach (TreeNode node3 in node2.Nodes)
                    {
                        if (node3.Text == text)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        TreeNode node4 = new TreeNode(text, 0x31, 0x31);
                        node2.Nodes.Add(node4);
                    }
                }
            }
        }

        private void UpdateTypeTextBox()
        {
            string fullName = string.Empty;
            if (this.artifactListView.SelectedItems.Count > 0)
            {
                System.Type tag = this.artifactListView.SelectedItems[0].Tag as System.Type;
                fullName = tag.FullName;
                bool flag = true;
                string str2 = string.Empty;
                if (tag.IsGenericType && (this.genericParameters.Parameters.Length > 0))
                {
                    str2 = "[";
                    int num = 0;
                    foreach (ParameterData data in this.genericParameters.Parameters)
                    {
                        str2 = str2 + "[";
                        num++;
                        System.Type type = data.Type;
                        if (type != null)
                        {
                            str2 = str2 + type.FullName;
                        }
                        else
                        {
                            flag = false;
                            break;
                        }
                        str2 = str2 + "]";
                        if (num < this.genericParameters.Parameters.Length)
                        {
                            str2 = str2 + ",";
                        }
                    }
                    str2 = str2 + "]";
                }
                if (flag)
                {
                    fullName = fullName + str2;
                }
            }
            this.typeTextBox.Text = fullName;
        }

        public System.Type SelectedType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.selectedType;
            }
        }

        IComponent ISite.Component
        {
            get
            {
                return null;
            }
        }

        IContainer ISite.Container
        {
            get
            {
                return null;
            }
        }

        bool ISite.DesignMode
        {
            get
            {
                return true;
            }
        }

        string ISite.Name
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        private ITypeProvider TypeProvider
        {
            get
            {
                ITypeProvider localTypeProvider = this.localTypeProvider;
                if (localTypeProvider == null)
                {
                    localTypeProvider = (ITypeProvider) this.serviceProvider.GetService(typeof(ITypeProvider));
                }
                if (localTypeProvider == null)
                {
                    throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
                }
                return localTypeProvider;
            }
        }

        private class DummySite : ISite, IServiceProvider
        {
            private IServiceProvider serviceProvider;

            internal DummySite(IServiceProvider serviceProvider)
            {
                this.serviceProvider = serviceProvider;
            }

            public object GetService(System.Type type)
            {
                return this.serviceProvider.GetService(type);
            }

            public IComponent Component
            {
                get
                {
                    return null;
                }
            }

            public IContainer Container
            {
                get
                {
                    return null;
                }
            }

            public bool DesignMode
            {
                get
                {
                    return true;
                }
            }

            public string Name
            {
                get
                {
                    return string.Empty;
                }
                set
                {
                }
            }
        }

        [TypeConverter(typeof(TypeBrowserDialog.GenericParametersConverter))]
        private class GenericParameters
        {
            private TypeBrowserDialog.ParameterData[] parameters = new TypeBrowserDialog.ParameterData[0];

            public TypeBrowserDialog.ParameterData[] Parameters
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.parameters;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.parameters = value;
                }
            }
        }

        private class GenericParametersConverter : TypeConverter
        {
            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                PropertyDescriptorCollection descriptors = new PropertyDescriptorCollection(null);
                TypeBrowserDialog.GenericParameters parameters = value as TypeBrowserDialog.GenericParameters;
                foreach (TypeBrowserDialog.ParameterData data in parameters.Parameters)
                {
                    descriptors.Add(new TypeBrowserDialog.ParameterDataPropertyDescriptor(context, TypeDescriptor.CreateProperty(typeof(TypeBrowserDialog.GenericParameters), data.ParameterType.Name, typeof(TypeBrowserDialog.ParameterData), new Attribute[0])));
                }
                return descriptors;
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        private sealed class HelpTextWindow : RichTextBox
        {
            internal HelpTextWindow()
            {
                base.TabStop = false;
                base.BorderStyle = BorderStyle.None;
                base.ReadOnly = true;
                this.BackColor = SystemColors.Control;
                this.Multiline = true;
                base.ScrollBars = RichTextBoxScrollBars.Both;
                this.Cursor = Cursors.Default;
            }

            internal void UpdateHelpText(System.Type selectedType)
            {
                using (Font font = new Font(this.Font.FontFamily, this.Font.SizeInPoints, FontStyle.Bold))
                {
                    base.Clear();
                    if (null != selectedType)
                    {
                        string[] strArray = new string[3];
                        strArray[0] = selectedType.Name;
                        try
                        {
                            strArray[1] = ((selectedType.Namespace != null) && (selectedType.Namespace.Length > 0)) ? selectedType.Namespace : TypeBrowserDialog.ResMgr.GetString("IDS_GLOBALNS");
                        }
                        catch (NullReferenceException)
                        {
                        }
                        strArray[1] = "{" + strArray[1] + "}";
                        strArray[2] = (selectedType.Assembly != null) ? selectedType.Assembly.GetName().FullName : "<Current Project>";
                        Color[] colorArray = new Color[] { Color.DarkRed, Color.Green, Color.Blue };
                        this.Text = TypeBrowserDialog.ResMgr.GetString("IDS_SELECTEDTYPE") + " " + strArray[0] + " " + TypeBrowserDialog.ResMgr.GetString("IDS_MEMBEROF") + " " + strArray[1] + "\r\n" + TypeBrowserDialog.ResMgr.GetString("IDS_CONTAINEDINASM") + " " + strArray[2];
                        int start = 0;
                        for (int i = 0; i < strArray.GetLength(0); i++)
                        {
                            start = base.Find(strArray[i], start, RichTextBoxFinds.MatchCase | RichTextBoxFinds.WholeWord);
                            base.SelectionColor = colorArray[i];
                            base.SelectionFont = font;
                            start += strArray[i].Length;
                        }
                    }
                    else
                    {
                        this.Text = TypeBrowserDialog.ResMgr.GetString("IDS_NOTYPESSELECTED");
                        base.SelectionStart = 0;
                        this.SelectionLength = this.Text.Length;
                        base.SelectionColor = Color.DarkRed;
                        base.SelectionFont = font;
                    }
                }
            }

            protected override void WndProc(ref Message msg)
            {
                if ((msg.Msg < 0x200) || (msg.Msg > 0x20d))
                {
                    base.WndProc(ref msg);
                }
            }
        }

        private sealed class ListItemComparer : IComparer
        {
            private bool ascending;
            private bool compareTypeName;

            internal ListItemComparer(bool compareTypeName, bool ascending)
            {
                this.compareTypeName = compareTypeName;
                this.ascending = ascending;
            }

            public int Compare(object first, object second)
            {
                int num = 0;
                if (this.compareTypeName)
                {
                    num = string.Compare(((ListViewItem) first).Text, ((ListViewItem) second).Text);
                }
                else
                {
                    num = string.Compare(((ListViewItem) first).SubItems[1].Text, ((ListViewItem) second).SubItems[1].Text);
                }
                if (!this.ascending)
                {
                    return (-1 * num);
                }
                return num;
            }
        }

        private class ParamaeterDataConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
            {
                return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
            {
                return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value == null)
                {
                    return new TypeBrowserDialog.ParameterData();
                }
                if (!(value is string))
                {
                    return base.ConvertFrom(context, culture, value);
                }
                TypeBrowserDialog.ParameterData data = new TypeBrowserDialog.ParameterData();
                if (((string) value) != string.Empty)
                {
                    data.Type = (context.GetService(typeof(ITypeProvider)) as ITypeProvider).GetType(value as string, true);
                }
                return data;
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    TypeBrowserDialog.ParameterData data = value as TypeBrowserDialog.ParameterData;
                    if (data.Type != null)
                    {
                        return data.Type.AssemblyQualifiedName;
                    }
                    return string.Empty;
                }
                if (destinationType == null)
                {
                    return string.Empty;
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        [TypeConverter(typeof(TypeBrowserDialog.ParamaeterDataConverter)), Editor(typeof(TypeBrowserEditor), typeof(UITypeEditor))]
        private sealed class ParameterData : ITypeFilterProvider
        {
            private System.Type parameterType;
            private System.Type type;

            public bool CanFilterType(System.Type type, bool throwOnError)
            {
                bool flag = true;
                if (type.IsByRef || !TypeProvider.IsAssignable(this.parameterType.BaseType, type))
                {
                    flag = false;
                }
                if (throwOnError && !flag)
                {
                    throw new Exception(SR.GetString("Error_ArgumentTypeNotMatchParameter"));
                }
                return flag;
            }

            public string FilterDescription
            {
                get
                {
                    return SR.GetString("FilterDescription_GenericArgument", new object[] { this.parameterType.Name });
                }
            }

            public System.Type ParameterType
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.parameterType;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.parameterType = value;
                }
            }

            public System.Type Type
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.type;
                }
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.type = value;
                }
            }
        }

        private class ParameterDataPropertyDescriptor : PropertyDescriptor
        {
            private PropertyDescriptor realPropertyDescriptor;
            private IServiceProvider serviceProvider;

            internal ParameterDataPropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor desc) : base(desc, null)
            {
                this.realPropertyDescriptor = desc;
                this.serviceProvider = serviceProvider;
            }

            public override bool CanResetValue(object component)
            {
                return this.realPropertyDescriptor.CanResetValue(component);
            }

            public override object GetValue(object component)
            {
                TypeBrowserDialog.GenericParameters parameters = component as TypeBrowserDialog.GenericParameters;
                foreach (TypeBrowserDialog.ParameterData data in parameters.Parameters)
                {
                    if (data.ParameterType.Name == this.Name)
                    {
                        return data;
                    }
                }
                return null;
            }

            public override void ResetValue(object component)
            {
                this.realPropertyDescriptor.ResetValue(component);
            }

            public override void SetValue(object component, object value)
            {
                TypeBrowserDialog.GenericParameters parameters = component as TypeBrowserDialog.GenericParameters;
                foreach (TypeBrowserDialog.ParameterData data in parameters.Parameters)
                {
                    if (data.ParameterType.Name == this.Name)
                    {
                        data.Type = ((TypeBrowserDialog.ParameterData) value).Type;
                        return;
                    }
                }
            }

            public override bool ShouldSerializeValue(object component)
            {
                return this.realPropertyDescriptor.ShouldSerializeValue(component);
            }

            public override AttributeCollection Attributes
            {
                get
                {
                    return this.realPropertyDescriptor.Attributes;
                }
            }

            public override string Category
            {
                get
                {
                    return SR.GetString("GenericParameters");
                }
            }

            public override System.Type ComponentType
            {
                get
                {
                    return this.realPropertyDescriptor.ComponentType;
                }
            }

            public override TypeConverter Converter
            {
                get
                {
                    return this.realPropertyDescriptor.Converter;
                }
            }

            public override string Description
            {
                get
                {
                    return this.realPropertyDescriptor.Description;
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public override System.Type PropertyType
            {
                get
                {
                    return this.realPropertyDescriptor.PropertyType;
                }
            }
        }
    }
}

