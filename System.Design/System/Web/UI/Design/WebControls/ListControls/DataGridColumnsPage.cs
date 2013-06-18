namespace System.Web.UI.Design.WebControls.ListControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.Design.WebControls;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class DataGridColumnsPage : BaseDataListPage
    {
        private System.Windows.Forms.Button addColumnButton;
        private System.Windows.Forms.CheckBox autoColumnCheck;
        private System.Windows.Forms.TreeView availableColumnsTree;
        private BoundColumnEditor boundColumnEditor;
        private ButtonColumnEditor buttonColumnEditor;
        private System.Windows.Forms.TextBox columnFooterTextEdit;
        private System.Windows.Forms.TextBox columnHeaderImageEdit;
        private System.Windows.Forms.Button columnHeaderImagePickerButton;
        private System.Windows.Forms.TextBox columnHeaderTextEdit;
        private GroupLabel columnPropsGroup;
        private ComboBox columnSortExprCombo;
        private System.Windows.Forms.CheckBox columnVisibleCheck;
        private ColumnItemEditor currentColumnEditor;
        private ColumnItem currentColumnItem;
        private BaseDataListPage.DataSourceItem currentDataSource;
        private System.Windows.Forms.Button deleteColumnButton;
        private EditCommandColumnEditor editCommandColumnEditor;
        private bool headerTextChanged;
        private HyperLinkColumnEditor hyperLinkColumnEditor;
        private const int ILI_ALL = 2;
        private const int ILI_BOUND = 1;
        private const int ILI_BUTTON = 4;
        private const int ILI_CUSTOM = 3;
        private const int ILI_DATASOURCE = 0;
        private const int ILI_DELETEBUTTON = 7;
        private const int ILI_EDITBUTTON = 6;
        private const int ILI_HYPERLINK = 8;
        private const int ILI_SELECTBUTTON = 5;
        private const int ILI_TEMPLATE = 9;
        private System.Windows.Forms.Button moveColumnDownButton;
        private System.Windows.Forms.Button moveColumnUpButton;
        private bool propChangesPending;
        private ListView selColumnsList;
        private DataSourceNode selectedDataSourceNode;
        private LinkLabel templatizeLink;

        private void InitForm()
        {
            this.autoColumnCheck = new System.Windows.Forms.CheckBox();
            GroupLabel label = new GroupLabel();
            System.Windows.Forms.Label label2 = new System.Windows.Forms.Label();
            this.availableColumnsTree = new System.Windows.Forms.TreeView();
            this.addColumnButton = new System.Windows.Forms.Button();
            System.Windows.Forms.Label label3 = new System.Windows.Forms.Label();
            this.selColumnsList = new ListView();
            this.moveColumnUpButton = new System.Windows.Forms.Button();
            this.moveColumnDownButton = new System.Windows.Forms.Button();
            this.deleteColumnButton = new System.Windows.Forms.Button();
            this.columnPropsGroup = new GroupLabel();
            System.Windows.Forms.Label label4 = new System.Windows.Forms.Label();
            this.columnHeaderTextEdit = new System.Windows.Forms.TextBox();
            System.Windows.Forms.Label label5 = new System.Windows.Forms.Label();
            this.columnHeaderImageEdit = new System.Windows.Forms.TextBox();
            this.columnHeaderImagePickerButton = new System.Windows.Forms.Button();
            System.Windows.Forms.Label label6 = new System.Windows.Forms.Label();
            this.columnFooterTextEdit = new System.Windows.Forms.TextBox();
            System.Windows.Forms.Label label7 = new System.Windows.Forms.Label();
            this.columnSortExprCombo = new ComboBox();
            this.columnVisibleCheck = new System.Windows.Forms.CheckBox();
            this.boundColumnEditor = new BoundColumnEditor();
            this.buttonColumnEditor = new ButtonColumnEditor();
            this.hyperLinkColumnEditor = new HyperLinkColumnEditor();
            this.editCommandColumnEditor = new EditCommandColumnEditor();
            this.templatizeLink = new LinkLabel();
            System.Drawing.Image image = new Bitmap(base.GetType(), "ColumnNodes.bmp");
            ImageList list = new ImageList {
                TransparentColor = Color.Magenta
            };
            list.Images.AddStrip(image);
            this.autoColumnCheck.SetBounds(4, 4, 400, 0x10);
            this.autoColumnCheck.Text = System.Design.SR.GetString("DGCol_AutoGen");
            this.autoColumnCheck.TabIndex = 0;
            this.autoColumnCheck.TextAlign = ContentAlignment.MiddleLeft;
            this.autoColumnCheck.FlatStyle = FlatStyle.System;
            this.autoColumnCheck.CheckedChanged += new EventHandler(this.OnCheckChangedAutoColumn);
            this.autoColumnCheck.Name = "AutoColumnCheckBox";
            label.SetBounds(4, 0x18, 0x1af, 14);
            label.Text = System.Design.SR.GetString("DGCol_ColListGroup");
            label.TabStop = false;
            label.TabIndex = 1;
            label.Name = "ColumnListGroup";
            label2.SetBounds(12, 40, 0xb8, 0x10);
            label2.Text = System.Design.SR.GetString("DGCol_AvailableCols");
            label2.TabStop = false;
            label2.TabIndex = 2;
            label2.Name = "AvailableColumnsLabel";
            this.availableColumnsTree.SetBounds(12, 0x3a, 170, 0x58);
            this.availableColumnsTree.ImageList = list;
            this.availableColumnsTree.Indent = 5;
            this.availableColumnsTree.HideSelection = false;
            this.availableColumnsTree.TabIndex = 3;
            this.availableColumnsTree.AfterSelect += new TreeViewEventHandler(this.OnSelChangedAvailableColumns);
            this.availableColumnsTree.Name = "AvailableColumnsTree";
            this.addColumnButton.SetBounds(0xbb, 0x52, 0x1f, 0x18);
            this.addColumnButton.Text = ">";
            this.addColumnButton.TabIndex = 4;
            this.addColumnButton.FlatStyle = FlatStyle.System;
            this.addColumnButton.Click += new EventHandler(this.OnClickAddColumn);
            this.addColumnButton.Name = "AddColumnButton";
            this.addColumnButton.AccessibleName = System.Design.SR.GetString("DGCol_AddColButtonDesc");
            label3.SetBounds(0xe2, 40, 200, 14);
            label3.Text = System.Design.SR.GetString("DGCol_SelectedCols");
            label3.TabStop = false;
            label3.TabIndex = 5;
            label3.Name = "SelectedColumnsLabel";
            ColumnHeader header = new ColumnHeader {
                Width = 0xb0
            };
            this.selColumnsList.SetBounds(0xde, 0x3a, 180, 0x58);
            this.selColumnsList.Columns.Add(header);
            this.selColumnsList.SmallImageList = list;
            this.selColumnsList.View = System.Windows.Forms.View.Details;
            this.selColumnsList.HeaderStyle = ColumnHeaderStyle.None;
            this.selColumnsList.LabelWrap = false;
            this.selColumnsList.HideSelection = false;
            this.selColumnsList.MultiSelect = false;
            this.selColumnsList.TabIndex = 6;
            this.selColumnsList.SelectedIndexChanged += new EventHandler(this.OnSelIndexChangedSelColumnsList);
            this.selColumnsList.KeyDown += new KeyEventHandler(this.OnSelColumnsListKeyDown);
            this.selColumnsList.Name = "SelectedColumnsList";
            this.moveColumnUpButton.SetBounds(0x196, 0x3a, 0x1c, 0x1b);
            this.moveColumnUpButton.TabIndex = 7;
            Bitmap bitmap = new Icon(base.GetType(), "SortUp.ico").ToBitmap();
            bitmap.MakeTransparent();
            this.moveColumnUpButton.Image = bitmap;
            this.moveColumnUpButton.Click += new EventHandler(this.OnClickMoveColumnUp);
            this.moveColumnUpButton.Name = "MoveColumnUpButton";
            this.moveColumnUpButton.AccessibleName = System.Design.SR.GetString("DGCol_MoveColumnUpButtonDesc");
            this.moveColumnDownButton.SetBounds(0x196, 0x58, 0x1c, 0x1b);
            this.moveColumnDownButton.TabIndex = 8;
            Bitmap bitmap2 = new Icon(base.GetType(), "SortDown.ico").ToBitmap();
            bitmap2.MakeTransparent();
            this.moveColumnDownButton.Image = bitmap2;
            this.moveColumnDownButton.Click += new EventHandler(this.OnClickMoveColumnDown);
            this.moveColumnDownButton.Name = "MoveColumnDownButton";
            this.moveColumnDownButton.AccessibleName = System.Design.SR.GetString("DGCol_MoveColumnDownButtonDesc");
            this.deleteColumnButton.SetBounds(0x196, 0x76, 0x1c, 0x1b);
            this.deleteColumnButton.TabIndex = 9;
            Bitmap bitmap3 = new Icon(base.GetType(), "Delete.ico").ToBitmap();
            bitmap3.MakeTransparent();
            this.deleteColumnButton.Image = bitmap3;
            this.deleteColumnButton.Click += new EventHandler(this.OnClickDeleteColumn);
            this.deleteColumnButton.Name = "DeleteColumnButton";
            this.deleteColumnButton.AccessibleName = System.Design.SR.GetString("DGCol_DeleteColumnButtonDesc");
            this.columnPropsGroup.SetBounds(8, 150, 0x1af, 14);
            this.columnPropsGroup.Text = System.Design.SR.GetString("DGCol_ColumnPropsGroup1");
            this.columnPropsGroup.TabStop = false;
            this.columnPropsGroup.TabIndex = 10;
            label4.SetBounds(20, 0xa6, 180, 14);
            label4.Text = System.Design.SR.GetString("DGCol_HeaderText");
            label4.TabStop = false;
            label4.TabIndex = 11;
            label4.Name = "ColumnHeaderTextLabel";
            this.columnHeaderTextEdit.SetBounds(20, 0xb6, 0xb6, 0x18);
            this.columnHeaderTextEdit.TabIndex = 12;
            this.columnHeaderTextEdit.TextChanged += new EventHandler(this.OnTextChangedColHeaderText);
            this.columnHeaderTextEdit.LostFocus += new EventHandler(this.OnLostFocusColHeaderText);
            this.columnHeaderTextEdit.Name = "ColumnHeaderTextEdit";
            label5.SetBounds(20, 0xd0, 180, 14);
            label5.Text = System.Design.SR.GetString("DGCol_HeaderImage");
            label5.TabStop = false;
            label5.TabIndex = 13;
            label5.Name = "ColumnHeaderImageLabel";
            this.columnHeaderImageEdit.SetBounds(20, 0xe0, 0x9c, 0x18);
            this.columnHeaderImageEdit.TabIndex = 14;
            this.columnHeaderImageEdit.TextChanged += new EventHandler(this.OnChangedColumnProperties);
            this.columnHeaderImageEdit.Name = "ColumnHeaderImageEdit";
            this.columnHeaderImagePickerButton.SetBounds(180, 0xdf, 0x18, 0x17);
            this.columnHeaderImagePickerButton.Text = "...";
            this.columnHeaderImagePickerButton.TabIndex = 15;
            this.columnHeaderImagePickerButton.FlatStyle = FlatStyle.System;
            this.columnHeaderImagePickerButton.Click += new EventHandler(this.OnClickColHeaderImagePicker);
            this.columnHeaderImagePickerButton.Name = "ColumnHeaderImagePickerButton";
            this.columnHeaderImagePickerButton.AccessibleName = System.Design.SR.GetString("DGCol_HeaderImagePickerDesc");
            label6.SetBounds(220, 0xa6, 180, 14);
            label6.Text = System.Design.SR.GetString("DGCol_FooterText");
            label6.TabStop = false;
            label6.TabIndex = 0x10;
            label6.Name = "ColumnFooterTextLabel";
            this.columnFooterTextEdit.SetBounds(220, 0xb6, 0xb6, 0x18);
            this.columnFooterTextEdit.TabIndex = 0x11;
            this.columnFooterTextEdit.TextChanged += new EventHandler(this.OnChangedColumnProperties);
            this.columnFooterTextEdit.Name = "ColumnFooterTextEdit";
            label7.SetBounds(220, 0xd0, 0x90, 0x10);
            label7.Text = System.Design.SR.GetString("DGCol_SortExpr");
            label7.TabStop = false;
            label7.TabIndex = 0x12;
            label7.Name = "ColumnSortExprLabel";
            this.columnSortExprCombo.SetBounds(220, 0xe0, 140, 0x15);
            this.columnSortExprCombo.TabIndex = 0x13;
            this.columnSortExprCombo.TextChanged += new EventHandler(this.OnChangedColumnProperties);
            this.columnSortExprCombo.SelectedIndexChanged += new EventHandler(this.OnChangedColumnProperties);
            this.columnSortExprCombo.Name = "ColumnSortExprCombo";
            this.columnVisibleCheck.SetBounds(0x170, 0xde, 100, 40);
            this.columnVisibleCheck.Text = System.Design.SR.GetString("DGCol_Visible");
            this.columnVisibleCheck.TabIndex = 20;
            this.columnVisibleCheck.FlatStyle = FlatStyle.System;
            this.columnVisibleCheck.CheckAlign = ContentAlignment.TopLeft;
            this.columnVisibleCheck.TextAlign = ContentAlignment.TopLeft;
            this.columnVisibleCheck.CheckedChanged += new EventHandler(this.OnChangedColumnProperties);
            this.columnVisibleCheck.Name = "ColumnVisibleCheckBox";
            this.boundColumnEditor.SetBounds(20, 250, 0x1a0, 0xa4);
            this.boundColumnEditor.TabIndex = 0x15;
            this.boundColumnEditor.Visible = false;
            this.boundColumnEditor.Changed += new EventHandler(this.OnChangedColumnProperties);
            this.buttonColumnEditor.SetBounds(20, 250, 0x1a0, 0xa4);
            this.buttonColumnEditor.TabIndex = 0x16;
            this.buttonColumnEditor.Visible = false;
            this.buttonColumnEditor.Changed += new EventHandler(this.OnChangedColumnProperties);
            this.hyperLinkColumnEditor.SetBounds(20, 250, 0x1a0, 0xa4);
            this.hyperLinkColumnEditor.TabIndex = 0x17;
            this.hyperLinkColumnEditor.Visible = false;
            this.hyperLinkColumnEditor.Changed += new EventHandler(this.OnChangedColumnProperties);
            this.editCommandColumnEditor.SetBounds(20, 250, 0x1a0, 0xa4);
            this.editCommandColumnEditor.TabIndex = 0x18;
            this.editCommandColumnEditor.Visible = false;
            this.editCommandColumnEditor.Changed += new EventHandler(this.OnChangedColumnProperties);
            this.templatizeLink.SetBounds(0x12, 0x19e, 400, 0x10);
            this.templatizeLink.TabIndex = 0x19;
            this.templatizeLink.Text = System.Design.SR.GetString("DGCol_Templatize");
            this.templatizeLink.Visible = false;
            this.templatizeLink.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnClickTemplatize);
            this.templatizeLink.Name = "TemplatizeLink";
            this.Text = System.Design.SR.GetString("DGCol_Text");
            base.AccessibleDescription = System.Design.SR.GetString("DGCol_Desc");
            base.Size = new Size(0x1d0, 0x1b0);
            base.CommitOnDeactivate = true;
            base.Icon = new Icon(base.GetType(), "DataGridColumnsPage.ico");
            base.Controls.Clear();
            base.Controls.AddRange(new System.Windows.Forms.Control[] { 
                this.templatizeLink, this.editCommandColumnEditor, this.hyperLinkColumnEditor, this.buttonColumnEditor, this.boundColumnEditor, this.columnVisibleCheck, this.columnSortExprCombo, label7, this.columnFooterTextEdit, label6, this.columnHeaderImagePickerButton, this.columnHeaderImageEdit, label5, this.columnHeaderTextEdit, label4, this.columnPropsGroup, 
                this.deleteColumnButton, this.moveColumnDownButton, this.moveColumnUpButton, this.selColumnsList, label3, this.addColumnButton, this.availableColumnsTree, label2, label, this.autoColumnCheck
             });
        }

        private void InitPage()
        {
            this.currentDataSource = null;
            this.autoColumnCheck.Checked = false;
            this.selectedDataSourceNode = null;
            this.availableColumnsTree.Nodes.Clear();
            this.selColumnsList.Items.Clear();
            this.currentColumnItem = null;
            this.columnSortExprCombo.Items.Clear();
            this.currentColumnEditor = null;
            this.boundColumnEditor.ClearDataFields();
            this.buttonColumnEditor.ClearDataFields();
            this.hyperLinkColumnEditor.ClearDataFields();
            this.editCommandColumnEditor.ClearDataFields();
            this.propChangesPending = false;
            this.headerTextChanged = false;
        }

        private void LoadAvailableColumnsTree()
        {
            if (this.currentDataSource != null)
            {
                this.selectedDataSourceNode = new DataSourceNode();
                this.availableColumnsTree.Nodes.Add(this.selectedDataSourceNode);
            }
            ButtonNode node = new ButtonNode();
            this.availableColumnsTree.Nodes.Add(node);
            ButtonNode node2 = new ButtonNode("Select", System.Design.SR.GetString("DGCol_SelectButton"), System.Design.SR.GetString("DGCol_Node_Select"));
            node.Nodes.Add(node2);
            EditCommandNode node3 = new EditCommandNode();
            node.Nodes.Add(node3);
            ButtonNode node4 = new ButtonNode("Delete", System.Design.SR.GetString("DGCol_DeleteButton"), System.Design.SR.GetString("DGCol_Node_Delete"));
            node.Nodes.Add(node4);
            HyperLinkNode node5 = new HyperLinkNode();
            this.availableColumnsTree.Nodes.Add(node5);
            TemplateNode node6 = new TemplateNode();
            this.availableColumnsTree.Nodes.Add(node6);
        }

        private void LoadColumnProperties()
        {
            string str = System.Design.SR.GetString("DGCol_ColumnPropsGroup1");
            if (this.currentColumnItem != null)
            {
                base.EnterLoadingMode();
                this.columnHeaderTextEdit.Text = this.currentColumnItem.HeaderText;
                this.columnHeaderImageEdit.Text = this.currentColumnItem.HeaderImageUrl;
                this.columnFooterTextEdit.Text = this.currentColumnItem.FooterText;
                this.columnSortExprCombo.Text = this.currentColumnItem.SortExpression;
                this.columnVisibleCheck.Checked = this.currentColumnItem.Visible;
                this.currentColumnEditor = null;
                if (this.currentColumnItem is BoundColumnItem)
                {
                    this.currentColumnEditor = this.boundColumnEditor;
                    str = System.Design.SR.GetString("DGCol_ColumnPropsGroup2", new object[] { "BoundColumn" });
                }
                else if (this.currentColumnItem is ButtonColumnItem)
                {
                    this.currentColumnEditor = this.buttonColumnEditor;
                    str = System.Design.SR.GetString("DGCol_ColumnPropsGroup2", new object[] { "ButtonColumn" });
                }
                else if (this.currentColumnItem is HyperLinkColumnItem)
                {
                    this.currentColumnEditor = this.hyperLinkColumnEditor;
                    str = System.Design.SR.GetString("DGCol_ColumnPropsGroup2", new object[] { "HyperLinkColumn" });
                }
                else if (this.currentColumnItem is EditCommandColumnItem)
                {
                    this.currentColumnEditor = this.editCommandColumnEditor;
                    str = System.Design.SR.GetString("DGCol_ColumnPropsGroup2", new object[] { "EditCommandColumn" });
                }
                else if (this.currentColumnItem is TemplateColumnItem)
                {
                    str = System.Design.SR.GetString("DGCol_ColumnPropsGroup2", new object[] { "TemplateColumn" });
                }
                if (this.currentColumnEditor != null)
                {
                    this.currentColumnEditor.LoadColumn(this.currentColumnItem);
                }
                base.ExitLoadingMode();
            }
            this.columnPropsGroup.Text = str;
        }

        private void LoadColumns()
        {
            System.Web.UI.WebControls.DataGrid baseControl = (System.Web.UI.WebControls.DataGrid) base.GetBaseControl();
            DataGridColumnCollection columns = baseControl.Columns;
            if (columns != null)
            {
                int count = columns.Count;
                for (int i = 0; i < count; i++)
                {
                    DataGridColumn runtimeColumn = columns[i];
                    ColumnItem item = null;
                    if (runtimeColumn is BoundColumn)
                    {
                        item = new BoundColumnItem((BoundColumn) runtimeColumn);
                    }
                    else if (runtimeColumn is ButtonColumn)
                    {
                        item = new ButtonColumnItem((ButtonColumn) runtimeColumn);
                    }
                    else if (runtimeColumn is HyperLinkColumn)
                    {
                        item = new HyperLinkColumnItem((HyperLinkColumn) runtimeColumn);
                    }
                    else if (runtimeColumn is TemplateColumn)
                    {
                        item = new TemplateColumnItem((TemplateColumn) runtimeColumn);
                    }
                    else if (runtimeColumn is EditCommandColumn)
                    {
                        item = new EditCommandColumnItem((EditCommandColumn) runtimeColumn);
                    }
                    else
                    {
                        item = new CustomColumnItem(runtimeColumn);
                    }
                    item.LoadColumnInfo();
                    this.selColumnsList.Items.Add(item);
                }
                if (this.selColumnsList.Items.Count != 0)
                {
                    this.currentColumnItem = (ColumnItem) this.selColumnsList.Items[0];
                    this.currentColumnItem.Selected = true;
                }
            }
        }

        protected override void LoadComponent()
        {
            this.InitPage();
            System.Web.UI.WebControls.DataGrid baseControl = (System.Web.UI.WebControls.DataGrid) base.GetBaseControl();
            this.LoadDataSourceItem();
            this.LoadAvailableColumnsTree();
            this.LoadDataSourceFields();
            this.autoColumnCheck.Checked = baseControl.AutoGenerateColumns;
            this.LoadColumns();
            this.UpdateEnabledVisibleState();
        }

        private void LoadDataSourceFields()
        {
            base.EnterLoadingMode();
            if (this.currentDataSource != null)
            {
                PropertyDescriptorCollection fields = this.currentDataSource.Fields;
                if ((fields != null) && (fields.Count > 0))
                {
                    DataFieldNode node = new DataFieldNode();
                    this.selectedDataSourceNode.Nodes.Add(node);
                    IEnumerator enumerator = fields.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        PropertyDescriptor current = (PropertyDescriptor) enumerator.Current;
                        if (BaseDataList.IsBindableType(current.PropertyType))
                        {
                            string name = current.Name;
                            DataFieldNode node2 = new DataFieldNode(name);
                            this.selectedDataSourceNode.Nodes.Add(node2);
                            this.boundColumnEditor.AddDataField(name);
                            this.buttonColumnEditor.AddDataField(name);
                            this.hyperLinkColumnEditor.AddDataField(name);
                            this.editCommandColumnEditor.AddDataField(name);
                            this.columnSortExprCombo.Items.Add(name);
                        }
                    }
                    this.availableColumnsTree.SelectedNode = node;
                    node.EnsureVisible();
                }
            }
            else
            {
                DataFieldNode node3 = new DataFieldNode(null);
                this.availableColumnsTree.Nodes.Insert(0, node3);
                this.availableColumnsTree.SelectedNode = node3;
                node3.EnsureVisible();
            }
            base.ExitLoadingMode();
        }

        private void LoadDataSourceItem()
        {
            System.Web.UI.WebControls.DataGrid baseControl = (System.Web.UI.WebControls.DataGrid) base.GetBaseControl();
            DataGridDesigner baseDesigner = (DataGridDesigner) base.GetBaseDesigner();
            string dataSource = baseDesigner.DataSource;
            if (dataSource != null)
            {
                IContainer service = (IContainer) baseControl.Site.GetService(typeof(IContainer));
                if (service != null)
                {
                    IComponent component = service.Components[dataSource];
                    if (component != null)
                    {
                        if (component is IListSource)
                        {
                            BaseDataListPage.ListSourceDataSourceItem item = new BaseDataListPage.ListSourceDataSourceItem(dataSource, (IListSource) component) {
                                CurrentDataMember = baseDesigner.DataMember
                            };
                            this.currentDataSource = item;
                        }
                        else if (component is IEnumerable)
                        {
                            this.currentDataSource = new BaseDataListPage.DataSourceItem(dataSource, (IEnumerable) component);
                        }
                    }
                }
            }
        }

        private void OnChangedColumnProperties(object source, EventArgs e)
        {
            if (!base.IsLoading())
            {
                this.propChangesPending = true;
                this.SetDirty();
            }
        }

        private void OnCheckChangedAutoColumn(object source, EventArgs e)
        {
            if (!base.IsLoading())
            {
                this.SetDirty();
                this.UpdateEnabledVisibleState();
            }
        }

        private void OnClickAddColumn(object source, EventArgs e)
        {
            AvailableColumnNode selectedNode = (AvailableColumnNode) this.availableColumnsTree.SelectedNode;
            if (this.propChangesPending)
            {
                this.SaveColumnProperties();
            }
            if (!selectedNode.CreatesMultipleColumns)
            {
                ColumnItem item = selectedNode.CreateColumn();
                this.selColumnsList.Items.Add(item);
                this.currentColumnItem = item;
                this.currentColumnItem.Selected = true;
                this.currentColumnItem.EnsureVisible();
            }
            else
            {
                ColumnItem[] itemArray = selectedNode.CreateColumns(this.currentDataSource.Fields);
                int length = itemArray.Length;
                for (int i = 0; i < length; i++)
                {
                    this.selColumnsList.Items.Add(itemArray[i]);
                }
                this.currentColumnItem = itemArray[length - 1];
                this.currentColumnItem.Selected = true;
                this.currentColumnItem.EnsureVisible();
            }
            this.selColumnsList.Focus();
            this.SetDirty();
            this.UpdateEnabledVisibleState();
        }

        private void OnClickColHeaderImagePicker(object source, EventArgs e)
        {
            string initialUrl = this.columnHeaderImageEdit.Text.Trim();
            string caption = System.Design.SR.GetString("DGCol_URLPCaption");
            string filter = System.Design.SR.GetString("DGCol_URLPFilter");
            initialUrl = UrlBuilder.BuildUrl(base.GetBaseControl(), this, initialUrl, caption, filter);
            if (initialUrl != null)
            {
                this.columnHeaderImageEdit.Text = initialUrl;
                this.OnChangedColumnProperties(this.columnHeaderImageEdit, EventArgs.Empty);
            }
        }

        private void OnClickDeleteColumn(object source, EventArgs e)
        {
            int index = this.currentColumnItem.Index;
            int num2 = -1;
            int count = this.selColumnsList.Items.Count;
            if (count > 1)
            {
                if (index == (count - 1))
                {
                    num2 = index - 1;
                }
                else
                {
                    num2 = index;
                }
            }
            this.propChangesPending = false;
            this.currentColumnItem.Remove();
            this.currentColumnItem = null;
            if (num2 != -1)
            {
                this.currentColumnItem = (ColumnItem) this.selColumnsList.Items[num2];
                this.currentColumnItem.Selected = true;
                this.currentColumnItem.EnsureVisible();
            }
            this.SetDirty();
            this.UpdateEnabledVisibleState();
        }

        private void OnClickMoveColumnDown(object source, EventArgs e)
        {
            if (this.propChangesPending)
            {
                this.SaveColumnProperties();
            }
            int index = this.currentColumnItem.Index;
            ListViewItem item = this.selColumnsList.Items[index];
            this.selColumnsList.Items.RemoveAt(index);
            this.selColumnsList.Items.Insert(index + 1, item);
            this.currentColumnItem = (ColumnItem) this.selColumnsList.Items[index + 1];
            this.currentColumnItem.Selected = true;
            this.currentColumnItem.EnsureVisible();
            this.SetDirty();
            this.UpdateEnabledVisibleState();
        }

        private void OnClickMoveColumnUp(object source, EventArgs e)
        {
            if (this.propChangesPending)
            {
                this.SaveColumnProperties();
            }
            int index = this.currentColumnItem.Index;
            ListViewItem item = this.selColumnsList.Items[index];
            this.selColumnsList.Items.RemoveAt(index);
            this.selColumnsList.Items.Insert(index - 1, item);
            this.currentColumnItem = (ColumnItem) this.selColumnsList.Items[index - 1];
            this.currentColumnItem.Selected = true;
            this.currentColumnItem.EnsureVisible();
            this.SetDirty();
            this.UpdateEnabledVisibleState();
        }

        private void OnClickTemplatize(object source, LinkLabelLinkClickedEventArgs e)
        {
            if (this.currentColumnItem != null)
            {
                if (this.propChangesPending)
                {
                    this.SaveColumnProperties();
                }
                this.currentColumnItem.SaveColumnInfo();
                TemplateColumnItem item = new TemplateColumnItem(this.currentColumnItem.GetTemplateColumn((System.Web.UI.WebControls.DataGrid) base.GetBaseControl()));
                item.LoadColumnInfo();
                this.selColumnsList.Items[this.currentColumnItem.Index] = item;
                this.currentColumnItem = item;
                this.currentColumnItem.Selected = true;
                this.SetDirty();
                this.UpdateEnabledVisibleState();
            }
        }

        private void OnLostFocusColHeaderText(object source, EventArgs e)
        {
            if (this.headerTextChanged)
            {
                this.headerTextChanged = false;
                if (this.currentColumnItem != null)
                {
                    this.currentColumnItem.HeaderText = this.columnHeaderTextEdit.Text;
                }
            }
        }

        private void OnSelChangedAvailableColumns(object source, TreeViewEventArgs e)
        {
            this.UpdateEnabledVisibleState();
        }

        private void OnSelColumnsListKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyData == Keys.Delete) && (this.currentColumnItem != null))
            {
                this.OnClickDeleteColumn(sender, e);
            }
        }

        private void OnSelIndexChangedSelColumnsList(object source, EventArgs e)
        {
            if (this.propChangesPending)
            {
                this.SaveColumnProperties();
            }
            if (this.selColumnsList.SelectedItems.Count == 0)
            {
                this.currentColumnItem = null;
            }
            else
            {
                this.currentColumnItem = (ColumnItem) this.selColumnsList.SelectedItems[0];
            }
            this.LoadColumnProperties();
            this.UpdateEnabledVisibleState();
        }

        private void OnTextChangedColHeaderText(object source, EventArgs e)
        {
            if (!base.IsLoading())
            {
                this.headerTextChanged = true;
                this.propChangesPending = true;
                this.SetDirty();
            }
        }

        private void SaveColumnProperties()
        {
            if (this.currentColumnItem != null)
            {
                this.currentColumnItem.HeaderText = this.columnHeaderTextEdit.Text;
                this.currentColumnItem.HeaderImageUrl = this.columnHeaderImageEdit.Text.Trim();
                this.currentColumnItem.FooterText = this.columnFooterTextEdit.Text;
                this.currentColumnItem.SortExpression = this.columnSortExprCombo.Text.Trim();
                this.currentColumnItem.Visible = this.columnVisibleCheck.Checked;
                if (this.currentColumnEditor != null)
                {
                    this.currentColumnEditor.SaveColumn();
                }
            }
            this.propChangesPending = false;
        }

        protected override void SaveComponent()
        {
            if (this.propChangesPending)
            {
                this.SaveColumnProperties();
            }
            System.Web.UI.WebControls.DataGrid baseControl = (System.Web.UI.WebControls.DataGrid) base.GetBaseControl();
            DataGridDesigner baseDesigner = (DataGridDesigner) base.GetBaseDesigner();
            baseControl.AutoGenerateColumns = this.autoColumnCheck.Checked;
            DataGridColumnCollection columns = baseControl.Columns;
            columns.Clear();
            int count = this.selColumnsList.Items.Count;
            for (int i = 0; i < count; i++)
            {
                ColumnItem item = (ColumnItem) this.selColumnsList.Items[i];
                item.SaveColumnInfo();
                columns.Add(item.RuntimeColumn);
            }
            baseDesigner.OnColumnsChanged();
        }

        public override void SetComponent(IComponent component)
        {
            base.SetComponent(component);
            this.InitForm();
        }

        private void UpdateEnabledVisibleState()
        {
            AvailableColumnNode selectedNode = (AvailableColumnNode) this.availableColumnsTree.SelectedNode;
            int count = this.selColumnsList.Items.Count;
            int num2 = this.selColumnsList.SelectedItems.Count;
            ColumnItem item = null;
            int index = -1;
            if (num2 != 0)
            {
                item = (ColumnItem) this.selColumnsList.SelectedItems[0];
            }
            if (item != null)
            {
                index = item.Index;
            }
            bool flag = index != -1;
            this.addColumnButton.Enabled = (selectedNode != null) && selectedNode.IsColumnCreator;
            this.moveColumnUpButton.Enabled = index > 0;
            this.moveColumnDownButton.Enabled = (index >= 0) && (index < (count - 1));
            this.deleteColumnButton.Enabled = flag;
            this.columnHeaderTextEdit.Enabled = flag;
            this.columnHeaderImageEdit.Enabled = flag;
            this.columnHeaderImagePickerButton.Enabled = flag;
            this.columnFooterTextEdit.Enabled = flag;
            this.columnSortExprCombo.Enabled = flag;
            this.columnVisibleCheck.Enabled = flag;
            this.boundColumnEditor.Visible = (this.currentColumnEditor == this.boundColumnEditor) && flag;
            this.buttonColumnEditor.Visible = (this.currentColumnEditor == this.buttonColumnEditor) && flag;
            this.hyperLinkColumnEditor.Visible = (this.currentColumnEditor == this.hyperLinkColumnEditor) && flag;
            this.editCommandColumnEditor.Visible = (this.currentColumnEditor == this.editCommandColumnEditor) && flag;
            this.templatizeLink.Visible = (count != 0) && (((this.boundColumnEditor.Visible || this.buttonColumnEditor.Visible) || this.hyperLinkColumnEditor.Visible) || this.editCommandColumnEditor.Visible);
        }

        protected override string HelpKeyword
        {
            get
            {
                return "net.Asp.DataGridProperties.Columns";
            }
        }

        private abstract class AvailableColumnNode : System.Windows.Forms.TreeNode
        {
            public AvailableColumnNode(string text, int icon) : base(text, icon, icon)
            {
            }

            public virtual DataGridColumnsPage.ColumnItem CreateColumn()
            {
                return null;
            }

            public virtual DataGridColumnsPage.ColumnItem[] CreateColumns(PropertyDescriptorCollection fields)
            {
                return null;
            }

            public virtual bool CreatesMultipleColumns
            {
                get
                {
                    return false;
                }
            }

            public virtual bool IsColumnCreator
            {
                get
                {
                    return true;
                }
            }
        }

        private class BoundColumnEditor : DataGridColumnsPage.ColumnItemEditor
        {
            private System.Windows.Forms.TextBox dataFieldEdit;
            private System.Windows.Forms.TextBox dataFormatStringEdit;
            private System.Windows.Forms.CheckBox readOnlyCheck;

            protected override void InitPanel()
            {
                System.Windows.Forms.Label label = new System.Windows.Forms.Label();
                this.dataFieldEdit = new System.Windows.Forms.TextBox();
                System.Windows.Forms.Label label2 = new System.Windows.Forms.Label();
                this.dataFormatStringEdit = new System.Windows.Forms.TextBox();
                this.readOnlyCheck = new System.Windows.Forms.CheckBox();
                label.SetBounds(0, 0, 160, 14);
                label.Text = System.Design.SR.GetString("DGCol_DFC_DataField");
                label.TabStop = false;
                label.TabIndex = 1;
                label.Name = "BoundColumnDataFieldLabel";
                this.dataFieldEdit.SetBounds(0, 0x10, 0xb6, 20);
                this.dataFieldEdit.TabIndex = 2;
                this.dataFieldEdit.ReadOnly = true;
                this.dataFieldEdit.TextChanged += new EventHandler(this.OnColumnChanged);
                this.dataFieldEdit.Name = "BoundColumnDataFieldEdit";
                label2.SetBounds(0, 40, 0xb6, 14);
                label2.Text = System.Design.SR.GetString("DGCol_DFC_DataFormat");
                label2.TabStop = false;
                label2.TabIndex = 3;
                label2.Name = "BoundColumnDataFormatStringLabel";
                this.dataFormatStringEdit.SetBounds(0, 0x38, 0xb6, 20);
                this.dataFormatStringEdit.TabIndex = 4;
                this.dataFormatStringEdit.TextChanged += new EventHandler(this.OnColumnChanged);
                this.dataFormatStringEdit.Name = "BoundColumnDataFormatStringEdit";
                this.readOnlyCheck.SetBounds(0, 80, 160, 0x10);
                this.readOnlyCheck.Text = System.Design.SR.GetString("DGCol_DFC_ReadOnly");
                this.readOnlyCheck.TabIndex = 5;
                this.readOnlyCheck.TextAlign = ContentAlignment.MiddleLeft;
                this.readOnlyCheck.FlatStyle = FlatStyle.System;
                this.readOnlyCheck.CheckedChanged += new EventHandler(this.OnColumnChanged);
                this.readOnlyCheck.Name = "BoundColumnReadOnlyCheck";
                base.Controls.Clear();
                base.Controls.AddRange(new System.Windows.Forms.Control[] { this.readOnlyCheck, this.dataFormatStringEdit, label2, this.dataFieldEdit, label });
            }

            public override void LoadColumn(DataGridColumnsPage.ColumnItem columnItem)
            {
                base.LoadColumn(columnItem);
                DataGridColumnsPage.BoundColumnItem item = (DataGridColumnsPage.BoundColumnItem) columnItem;
                this.dataFieldEdit.Text = item.DataField;
                this.dataFormatStringEdit.Text = item.DataFormatString;
                this.readOnlyCheck.Checked = item.ReadOnly;
                this.dataFieldEdit.ReadOnly = base.dataFieldsAvailable;
            }

            private void OnColumnChanged(object source, EventArgs e)
            {
                this.OnChanged(EventArgs.Empty);
            }

            public override void SaveColumn()
            {
                base.SaveColumn();
                DataGridColumnsPage.BoundColumnItem columnItem = (DataGridColumnsPage.BoundColumnItem) base.columnItem;
                columnItem.DataFormatString = this.dataFormatStringEdit.Text;
                columnItem.ReadOnly = this.readOnlyCheck.Checked;
                if (!base.dataFieldsAvailable)
                {
                    columnItem.DataField = this.dataFieldEdit.Text.Trim();
                }
            }
        }

        private class BoundColumnItem : DataGridColumnsPage.ColumnItem
        {
            protected string dataField;
            protected string dataFormatString;
            protected bool readOnly;

            public BoundColumnItem(BoundColumn runtimeColumn) : base(runtimeColumn, 1)
            {
            }

            protected override string GetDefaultHeaderText()
            {
                if ((this.dataField != null) && (this.dataField.Length != 0))
                {
                    return this.dataField;
                }
                return System.Design.SR.GetString("DGCol_Node_Bound");
            }

            public override TemplateColumn GetTemplateColumn(System.Web.UI.WebControls.DataGrid dataGrid)
            {
                TemplateColumn templateColumn = base.GetTemplateColumn(dataGrid);
                templateColumn.ItemTemplate = base.GetTemplate(dataGrid, this.GetTemplateContent(false));
                if (!this.readOnly)
                {
                    templateColumn.EditItemTemplate = base.GetTemplate(dataGrid, this.GetTemplateContent(true));
                }
                return templateColumn;
            }

            private string GetTemplateContent(bool editMode)
            {
                StringBuilder builder = new StringBuilder();
                string str = editMode ? "TextBox" : "Label";
                builder.Append("<asp:");
                builder.Append(str);
                builder.Append(" runat=\"server\"");
                string dataField = ((BoundColumn) base.RuntimeColumn).DataField;
                if (dataField.Length != 0)
                {
                    builder.Append(" Text='<%# DataBinder.Eval(Container, \"DataItem.");
                    builder.Append(dataField);
                    builder.Append("\"");
                    if (this.dataFormatString.Length != 0)
                    {
                        builder.Append(", \"");
                        builder.Append(this.dataFormatString);
                        builder.Append("\"");
                    }
                    builder.Append(") %>'");
                }
                builder.Append("></asp:");
                builder.Append(str);
                builder.Append(">");
                return builder.ToString();
            }

            public override void LoadColumnInfo()
            {
                base.LoadColumnInfo();
                BoundColumn runtimeColumn = (BoundColumn) base.RuntimeColumn;
                this.dataField = runtimeColumn.DataField;
                this.dataFormatString = runtimeColumn.DataFormatString;
                this.readOnly = runtimeColumn.ReadOnly;
                base.UpdateDisplayText();
            }

            public override void SaveColumnInfo()
            {
                base.SaveColumnInfo();
                BoundColumn runtimeColumn = (BoundColumn) base.RuntimeColumn;
                runtimeColumn.DataField = this.dataField;
                runtimeColumn.DataFormatString = this.dataFormatString;
                runtimeColumn.ReadOnly = this.readOnly;
            }

            public string DataField
            {
                get
                {
                    return this.dataField;
                }
                set
                {
                    this.dataField = value;
                    base.UpdateDisplayText();
                }
            }

            public string DataFormatString
            {
                get
                {
                    return this.dataFormatString;
                }
                set
                {
                    this.dataFormatString = value;
                }
            }

            public bool ReadOnly
            {
                get
                {
                    return this.readOnly;
                }
                set
                {
                    this.readOnly = value;
                }
            }
        }

        private class ButtonColumnEditor : DataGridColumnsPage.ColumnItemEditor
        {
            private ComboBox buttonTypeCombo;
            private System.Windows.Forms.TextBox commandEdit;
            private UnsettableComboBox dataTextFieldCombo;
            private System.Windows.Forms.TextBox dataTextFieldEdit;
            private System.Windows.Forms.TextBox dataTextFormatStringEdit;
            private const int IDX_TYPE_LINKBUTTON = 0;
            private const int IDX_TYPE_PUSHBUTTON = 1;
            private System.Windows.Forms.TextBox textEdit;

            public override void AddDataField(string fieldName)
            {
                this.dataTextFieldCombo.AddItem(fieldName);
                base.AddDataField(fieldName);
            }

            public override void ClearDataFields()
            {
                this.dataTextFieldCombo.Items.Clear();
                this.dataTextFieldCombo.EnsureNotSetItem();
                base.ClearDataFields();
            }

            protected override void InitPanel()
            {
                System.Windows.Forms.Label label = new System.Windows.Forms.Label();
                this.textEdit = new System.Windows.Forms.TextBox();
                System.Windows.Forms.Label label2 = new System.Windows.Forms.Label();
                this.dataTextFieldCombo = new UnsettableComboBox();
                this.dataTextFieldEdit = new System.Windows.Forms.TextBox();
                System.Windows.Forms.Label label3 = new System.Windows.Forms.Label();
                this.dataTextFormatStringEdit = new System.Windows.Forms.TextBox();
                System.Windows.Forms.Label label4 = new System.Windows.Forms.Label();
                this.commandEdit = new System.Windows.Forms.TextBox();
                System.Windows.Forms.Label label5 = new System.Windows.Forms.Label();
                this.buttonTypeCombo = new ComboBox();
                label.SetBounds(0, 0, 160, 14);
                label.Text = System.Design.SR.GetString("DGCol_BC_Text");
                label.TabStop = false;
                label.TabIndex = 1;
                label.Name = "ButtonColumnTextLabel";
                this.textEdit.SetBounds(0, 0x10, 0xb6, 0x18);
                this.textEdit.TabIndex = 2;
                this.textEdit.TextChanged += new EventHandler(this.OnColumnChanged);
                this.textEdit.Name = "ButtonColumnTextEdit";
                label2.SetBounds(0, 40, 160, 14);
                label2.Text = System.Design.SR.GetString("DGCol_BC_DataTextField");
                label2.TabStop = false;
                label2.TabIndex = 3;
                label2.Name = "ButtonColumnDataTextFieldLabel";
                this.dataTextFieldCombo.SetBounds(0, 0x38, 0xb6, 0x15);
                this.dataTextFieldCombo.TabIndex = 4;
                this.dataTextFieldCombo.DropDownStyle = ComboBoxStyle.DropDownList;
                this.dataTextFieldCombo.SelectedIndexChanged += new EventHandler(this.OnColumnChanged);
                this.dataTextFieldCombo.Name = "ButtonColumnDataTextFieldCombo";
                this.dataTextFieldEdit.SetBounds(0, 0x38, 0xb6, 14);
                this.dataTextFieldEdit.TabIndex = 4;
                this.dataTextFieldEdit.TextChanged += new EventHandler(this.OnColumnChanged);
                this.dataTextFieldEdit.Name = "ButtonColumnDataTextFieldEdit";
                label3.SetBounds(0, 0x52, 0xb6, 14);
                label3.Text = System.Design.SR.GetString("DGCol_BC_DataTextFormat");
                label3.TabIndex = 5;
                label3.TabStop = false;
                label3.Name = "ButtonColumnDataTextFormatStringLabel";
                this.dataTextFormatStringEdit.SetBounds(0, 0x62, 0xb6, 14);
                this.dataTextFormatStringEdit.TabIndex = 6;
                this.dataTextFormatStringEdit.TextChanged += new EventHandler(this.OnColumnChanged);
                this.dataTextFormatStringEdit.Name = "ButtonColumDataTextFormatStringEdit";
                label4.SetBounds(200, 0, 160, 14);
                label4.Text = System.Design.SR.GetString("DGCol_BC_Command");
                label4.TabStop = false;
                label4.TabIndex = 8;
                label4.Name = "ButtonColumnCommandLabel";
                this.commandEdit.SetBounds(200, 0x10, 0xb6, 0x18);
                this.commandEdit.TabIndex = 9;
                this.commandEdit.TextChanged += new EventHandler(this.OnColumnChanged);
                this.commandEdit.Name = "ButtonColumnCommandEdit";
                label5.SetBounds(200, 40, 160, 14);
                label5.Text = System.Design.SR.GetString("DGCol_BC_ButtonType");
                label5.TabStop = false;
                label5.TabIndex = 10;
                label5.Name = "ButtonColumnButtonTypeLabel";
                this.buttonTypeCombo.SetBounds(200, 0x38, 0xb6, 0x15);
                this.buttonTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
                this.buttonTypeCombo.Items.AddRange(new object[] { System.Design.SR.GetString("DGCol_BC_BT_Link"), System.Design.SR.GetString("DGCol_BC_BT_Push") });
                this.buttonTypeCombo.TabIndex = 11;
                this.buttonTypeCombo.SelectedIndexChanged += new EventHandler(this.OnColumnChanged);
                this.buttonTypeCombo.Name = "ButtonColumnButtonTypeCombo";
                base.Controls.Clear();
                base.Controls.AddRange(new System.Windows.Forms.Control[] { this.buttonTypeCombo, label5, this.commandEdit, label4, this.dataTextFormatStringEdit, label3, this.dataTextFieldEdit, this.dataTextFieldCombo, label2, this.textEdit, label });
            }

            public override void LoadColumn(DataGridColumnsPage.ColumnItem columnItem)
            {
                base.LoadColumn(columnItem);
                DataGridColumnsPage.ButtonColumnItem item = (DataGridColumnsPage.ButtonColumnItem) base.columnItem;
                this.commandEdit.Text = item.Command;
                this.textEdit.Text = item.ButtonText;
                if (base.dataFieldsAvailable)
                {
                    if (item.ButtonDataTextField != null)
                    {
                        int num = this.dataTextFieldCombo.FindStringExact(item.ButtonDataTextField);
                        this.dataTextFieldCombo.SelectedIndex = num;
                    }
                    this.dataTextFieldCombo.Visible = true;
                    this.dataTextFieldEdit.Visible = false;
                }
                else
                {
                    this.dataTextFieldEdit.Text = item.ButtonDataTextField;
                    this.dataTextFieldEdit.Visible = true;
                    this.dataTextFieldCombo.Visible = false;
                }
                this.dataTextFormatStringEdit.Text = item.ButtonDataTextFormatString;
                switch (item.ButtonType)
                {
                    case ButtonColumnType.LinkButton:
                        this.buttonTypeCombo.SelectedIndex = 0;
                        break;

                    case ButtonColumnType.PushButton:
                        this.buttonTypeCombo.SelectedIndex = 1;
                        break;
                }
                this.UpdateEnabledState();
            }

            private void OnColumnChanged(object source, EventArgs e)
            {
                this.OnChanged(EventArgs.Empty);
                if ((source == this.dataTextFieldCombo) || (source == this.dataTextFieldEdit))
                {
                    this.UpdateEnabledState();
                }
            }

            public override void SaveColumn()
            {
                base.SaveColumn();
                DataGridColumnsPage.ButtonColumnItem columnItem = (DataGridColumnsPage.ButtonColumnItem) base.columnItem;
                columnItem.Command = this.commandEdit.Text.Trim();
                columnItem.ButtonText = this.textEdit.Text;
                if (base.dataFieldsAvailable)
                {
                    if (this.dataTextFieldCombo.IsSet())
                    {
                        columnItem.ButtonDataTextField = this.dataTextFieldCombo.Text;
                    }
                    else
                    {
                        columnItem.ButtonDataTextField = string.Empty;
                    }
                }
                else
                {
                    columnItem.ButtonDataTextField = this.dataTextFieldEdit.Text.Trim();
                }
                columnItem.ButtonDataTextFormatString = this.dataTextFormatStringEdit.Text;
                switch (this.buttonTypeCombo.SelectedIndex)
                {
                    case 0:
                        columnItem.ButtonType = ButtonColumnType.LinkButton;
                        return;

                    case 1:
                        columnItem.ButtonType = ButtonColumnType.PushButton;
                        return;
                }
            }

            private void UpdateEnabledState()
            {
                if (base.dataFieldsAvailable)
                {
                    this.dataTextFormatStringEdit.Enabled = this.dataTextFieldCombo.IsSet();
                }
                else
                {
                    this.dataTextFormatStringEdit.Enabled = this.dataTextFieldEdit.Text.Trim().Length != 0;
                }
            }
        }

        private class ButtonColumnItem : DataGridColumnsPage.ColumnItem
        {
            protected string buttonDataTextField;
            protected string buttonDataTextFormatString;
            protected string buttonText;
            protected ButtonColumnType buttonType;
            protected string command;

            public ButtonColumnItem(ButtonColumn runtimeColumn) : base(runtimeColumn, 4)
            {
            }

            protected override string GetDefaultHeaderText()
            {
                if ((this.buttonText != null) && (this.buttonText.Length != 0))
                {
                    return this.buttonText;
                }
                return System.Design.SR.GetString("DGCol_Node_Button");
            }

            public override TemplateColumn GetTemplateColumn(System.Web.UI.WebControls.DataGrid dataGrid)
            {
                TemplateColumn templateColumn = base.GetTemplateColumn(dataGrid);
                StringBuilder builder = new StringBuilder();
                string str = (this.buttonType == ButtonColumnType.LinkButton) ? "LinkButton" : "Button";
                builder.Append("<asp:");
                builder.Append(str);
                builder.Append(" runat=\"server\"");
                if (this.buttonDataTextField.Length != 0)
                {
                    builder.Append(" Text='<%# DataBinder.Eval(Container, \"DataItem.");
                    builder.Append(this.buttonDataTextField);
                    builder.Append("\"");
                    if (this.buttonDataTextFormatString.Length != 0)
                    {
                        builder.Append(", \"");
                        builder.Append(this.buttonDataTextFormatString);
                        builder.Append("\"");
                    }
                    builder.Append(") %>'");
                }
                else
                {
                    builder.Append(" Text=\"");
                    builder.Append(this.buttonText);
                    builder.Append("\"");
                }
                builder.Append(" CommandName=\"");
                builder.Append(this.command);
                builder.Append("\"");
                builder.Append(" CausesValidation=\"false\"></asp:");
                builder.Append(str);
                builder.Append(">");
                templateColumn.ItemTemplate = base.GetTemplate(dataGrid, builder.ToString());
                return templateColumn;
            }

            public override void LoadColumnInfo()
            {
                base.LoadColumnInfo();
                ButtonColumn runtimeColumn = (ButtonColumn) base.RuntimeColumn;
                this.command = runtimeColumn.CommandName;
                this.buttonText = runtimeColumn.Text;
                this.buttonDataTextField = runtimeColumn.DataTextField;
                this.buttonDataTextFormatString = runtimeColumn.DataTextFormatString;
                this.buttonType = runtimeColumn.ButtonType;
                base.UpdateDisplayText();
            }

            public override void SaveColumnInfo()
            {
                base.SaveColumnInfo();
                ButtonColumn runtimeColumn = (ButtonColumn) base.RuntimeColumn;
                runtimeColumn.CommandName = this.command;
                runtimeColumn.Text = this.buttonText;
                runtimeColumn.DataTextField = this.buttonDataTextField;
                runtimeColumn.DataTextFormatString = this.buttonDataTextFormatString;
                runtimeColumn.ButtonType = this.buttonType;
            }

            public string ButtonDataTextField
            {
                get
                {
                    return this.buttonDataTextField;
                }
                set
                {
                    this.buttonDataTextField = value;
                }
            }

            public string ButtonDataTextFormatString
            {
                get
                {
                    return this.buttonDataTextFormatString;
                }
                set
                {
                    this.buttonDataTextFormatString = value;
                }
            }

            public string ButtonText
            {
                get
                {
                    return this.buttonText;
                }
                set
                {
                    this.buttonText = value;
                    base.UpdateDisplayText();
                }
            }

            public ButtonColumnType ButtonType
            {
                get
                {
                    return this.buttonType;
                }
                set
                {
                    this.buttonType = value;
                }
            }

            public string Command
            {
                get
                {
                    return this.command;
                }
                set
                {
                    this.command = value;
                }
            }
        }

        private class ButtonNode : DataGridColumnsPage.AvailableColumnNode
        {
            private string buttonText;
            private string command;

            public ButtonNode() : this(string.Empty, System.Design.SR.GetString("DGCol_Button"), System.Design.SR.GetString("DGCol_Node_Button"))
            {
            }

            public ButtonNode(string command, string buttonText, string text) : base(text, 4)
            {
                this.command = command;
                this.buttonText = buttonText;
            }

            public override DataGridColumnsPage.ColumnItem CreateColumn()
            {
                ButtonColumn runtimeColumn = new ButtonColumn {
                    Text = this.buttonText,
                    CommandName = this.command
                };
                DataGridColumnsPage.ColumnItem item = new DataGridColumnsPage.ButtonColumnItem(runtimeColumn);
                item.LoadColumnInfo();
                return item;
            }
        }

        private abstract class ColumnItem : ListViewItem
        {
            protected string footerText;
            protected string headerImageUrl;
            protected string headerText;
            protected DataGridColumn runtimeColumn;
            protected string sortExpression;
            protected bool visible;

            public ColumnItem(DataGridColumn runtimeColumn, int image) : base(string.Empty, image)
            {
                this.runtimeColumn = runtimeColumn;
                this.headerText = this.GetDefaultHeaderText();
                base.Text = this.GetNodeText(null);
            }

            protected virtual string GetDefaultHeaderText()
            {
                return System.Design.SR.GetString("DGCol_Node");
            }

            public virtual string GetNodeText(string headerText)
            {
                if ((headerText != null) && (headerText.Length != 0))
                {
                    return headerText;
                }
                return this.GetDefaultHeaderText();
            }

            protected ITemplate GetTemplate(System.Web.UI.WebControls.DataGrid dataGrid, string templateContent)
            {
                try
                {
                    IDesignerHost service = (IDesignerHost) dataGrid.Site.GetService(typeof(IDesignerHost));
                    return ControlParser.ParseTemplate(service, templateContent, null);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            public virtual TemplateColumn GetTemplateColumn(System.Web.UI.WebControls.DataGrid dataGrid)
            {
                return new TemplateColumn { HeaderText = this.headerText, HeaderImageUrl = this.headerImageUrl };
            }

            public virtual void LoadColumnInfo()
            {
                this.headerText = this.runtimeColumn.HeaderText;
                this.headerImageUrl = this.runtimeColumn.HeaderImageUrl;
                this.footerText = this.runtimeColumn.FooterText;
                this.visible = this.runtimeColumn.Visible;
                this.sortExpression = this.runtimeColumn.SortExpression;
                this.UpdateDisplayText();
            }

            public virtual void SaveColumnInfo()
            {
                this.runtimeColumn.HeaderText = this.headerText;
                this.runtimeColumn.HeaderImageUrl = this.headerImageUrl;
                this.runtimeColumn.FooterText = this.footerText;
                this.runtimeColumn.Visible = this.visible;
                this.runtimeColumn.SortExpression = this.sortExpression;
            }

            protected void UpdateDisplayText()
            {
                base.Text = this.GetNodeText(this.headerText);
            }

            public virtual DataGridColumnsPage.ColumnItemEditor ColumnEditor
            {
                get
                {
                    return null;
                }
            }

            public string FooterText
            {
                get
                {
                    return this.footerText;
                }
                set
                {
                    this.footerText = value;
                }
            }

            public string HeaderImageUrl
            {
                get
                {
                    return this.headerImageUrl;
                }
                set
                {
                    this.headerImageUrl = value;
                }
            }

            public string HeaderText
            {
                get
                {
                    return this.headerText;
                }
                set
                {
                    this.headerText = value;
                    this.UpdateDisplayText();
                }
            }

            public DataGridColumn RuntimeColumn
            {
                get
                {
                    return this.runtimeColumn;
                }
            }

            public string SortExpression
            {
                get
                {
                    return this.sortExpression;
                }
                set
                {
                    this.sortExpression = value;
                }
            }

            public bool Visible
            {
                get
                {
                    return this.visible;
                }
                set
                {
                    this.visible = value;
                }
            }
        }

        private abstract class ColumnItemEditor : System.Windows.Forms.Panel
        {
            protected DataGridColumnsPage.ColumnItem columnItem;
            protected bool dataFieldsAvailable;

            public event EventHandler Changed;

            public ColumnItemEditor()
            {
                this.InitPanel();
            }

            public virtual void AddDataField(string fieldName)
            {
                this.dataFieldsAvailable = true;
            }

            public virtual void ClearDataFields()
            {
                this.dataFieldsAvailable = false;
            }

            protected virtual void InitPanel()
            {
            }

            public virtual void LoadColumn(DataGridColumnsPage.ColumnItem columnItem)
            {
                this.columnItem = columnItem;
            }

            protected virtual void OnChanged(EventArgs e)
            {
                if (this.onChangedHandler != null)
                {
                    this.onChangedHandler(this, e);
                }
            }

            public virtual void SaveColumn()
            {
            }
        }

        private class CustomColumnItem : DataGridColumnsPage.ColumnItem
        {
            public CustomColumnItem(DataGridColumn runtimeColumn) : base(runtimeColumn, 3)
            {
            }
        }

        private class DataFieldNode : DataGridColumnsPage.AvailableColumnNode
        {
            private bool allFields;
            protected string fieldName;
            private bool genericBoundColumn;

            public DataFieldNode() : base(System.Design.SR.GetString("DGCol_Node_AllFields"), 2)
            {
                this.fieldName = null;
                this.allFields = true;
            }

            public DataFieldNode(string fieldName) : base(fieldName, 1)
            {
                this.fieldName = fieldName;
                if (fieldName == null)
                {
                    this.genericBoundColumn = true;
                    base.Text = System.Design.SR.GetString("DGCol_Node_Bound");
                }
            }

            public override DataGridColumnsPage.ColumnItem CreateColumn()
            {
                BoundColumn runtimeColumn = new BoundColumn();
                if (!this.genericBoundColumn)
                {
                    runtimeColumn.HeaderText = this.fieldName;
                    runtimeColumn.DataField = this.fieldName;
                    runtimeColumn.SortExpression = this.fieldName;
                }
                DataGridColumnsPage.ColumnItem item = new DataGridColumnsPage.BoundColumnItem(runtimeColumn);
                item.LoadColumnInfo();
                return item;
            }

            public override DataGridColumnsPage.ColumnItem[] CreateColumns(PropertyDescriptorCollection fields)
            {
                ArrayList list = new ArrayList();
                IEnumerator enumerator = fields.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    PropertyDescriptor current = (PropertyDescriptor) enumerator.Current;
                    if (BaseDataList.IsBindableType(current.PropertyType))
                    {
                        BoundColumn runtimeColumn = new BoundColumn {
                            HeaderText = current.Name,
                            DataField = current.Name
                        };
                        DataGridColumnsPage.ColumnItem item = new DataGridColumnsPage.BoundColumnItem(runtimeColumn);
                        item.LoadColumnInfo();
                        list.Add(item);
                    }
                }
                return (DataGridColumnsPage.ColumnItem[]) list.ToArray(typeof(DataGridColumnsPage.ColumnItem));
            }

            public override bool CreatesMultipleColumns
            {
                get
                {
                    return this.allFields;
                }
            }
        }

        private class DataSourceNode : DataGridColumnsPage.AvailableColumnNode
        {
            public DataSourceNode() : base(System.Design.SR.GetString("DGCol_Node_DataFields"), 0)
            {
            }

            public override bool IsColumnCreator
            {
                get
                {
                    return false;
                }
            }
        }

        private class EditCommandColumnEditor : DataGridColumnsPage.ColumnItemEditor
        {
            private ComboBox buttonTypeCombo;
            private System.Windows.Forms.TextBox cancelTextEdit;
            private System.Windows.Forms.TextBox editTextEdit;
            private const int IDX_TYPE_LINKBUTTON = 0;
            private const int IDX_TYPE_PUSHBUTTON = 1;
            private System.Windows.Forms.TextBox updateTextEdit;

            protected override void InitPanel()
            {
                System.Windows.Forms.Label label = new System.Windows.Forms.Label();
                this.editTextEdit = new System.Windows.Forms.TextBox();
                System.Windows.Forms.Label label2 = new System.Windows.Forms.Label();
                this.updateTextEdit = new System.Windows.Forms.TextBox();
                System.Windows.Forms.Label label3 = new System.Windows.Forms.Label();
                this.cancelTextEdit = new System.Windows.Forms.TextBox();
                System.Windows.Forms.Label label4 = new System.Windows.Forms.Label();
                this.buttonTypeCombo = new ComboBox();
                label.SetBounds(0, 0, 160, 14);
                label.Text = System.Design.SR.GetString("DGCol_EC_Edit");
                label.TabStop = false;
                label.TabIndex = 1;
                label.Name = "EditColumnEditTextLabel";
                this.editTextEdit.SetBounds(0, 0x10, 0xb6, 0x18);
                this.editTextEdit.TabIndex = 2;
                this.editTextEdit.TextChanged += new EventHandler(this.OnColumnChanged);
                this.editTextEdit.Name = "EditColumnEditTextEdit";
                label2.SetBounds(0, 40, 160, 14);
                label2.Text = System.Design.SR.GetString("DGCol_EC_Update");
                label2.TabStop = false;
                label2.TabIndex = 3;
                label2.Name = "EditColumnUpdateTextLabel";
                this.updateTextEdit.SetBounds(0, 0x38, 0xb6, 0x18);
                this.updateTextEdit.TabIndex = 4;
                this.updateTextEdit.TextChanged += new EventHandler(this.OnColumnChanged);
                this.updateTextEdit.Name = "EditColumnUpdateTextEdit";
                label3.SetBounds(200, 0, 160, 14);
                label3.Text = System.Design.SR.GetString("DGCol_EC_Cancel");
                label3.TabStop = false;
                label3.TabIndex = 5;
                label3.Name = "EditColumnCancelTextLabel";
                this.cancelTextEdit.SetBounds(200, 0x10, 0xb6, 0x18);
                this.cancelTextEdit.TabIndex = 6;
                this.cancelTextEdit.TextChanged += new EventHandler(this.OnColumnChanged);
                this.cancelTextEdit.Name = "EditColumnCancelTextEdit";
                label4.SetBounds(200, 40, 160, 14);
                label4.Text = System.Design.SR.GetString("DGCol_EC_ButtonType");
                label4.TabStop = false;
                label4.TabIndex = 7;
                label4.Name = "EditColumnButtonTypeLabel";
                this.buttonTypeCombo.SetBounds(200, 0x38, 0xb6, 0x15);
                this.buttonTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
                this.buttonTypeCombo.Items.AddRange(new object[] { System.Design.SR.GetString("DGCol_EC_BT_Link"), System.Design.SR.GetString("DGCol_EC_BT_Push") });
                this.buttonTypeCombo.TabIndex = 8;
                this.buttonTypeCombo.SelectedIndexChanged += new EventHandler(this.OnColumnChanged);
                this.buttonTypeCombo.Name = "EditColumnButtonTypeCombo";
                base.Controls.Clear();
                base.Controls.AddRange(new System.Windows.Forms.Control[] { this.buttonTypeCombo, label4, this.cancelTextEdit, label3, this.updateTextEdit, label2, this.editTextEdit, label });
            }

            public override void LoadColumn(DataGridColumnsPage.ColumnItem columnItem)
            {
                base.LoadColumn(columnItem);
                DataGridColumnsPage.EditCommandColumnItem item = (DataGridColumnsPage.EditCommandColumnItem) base.columnItem;
                this.editTextEdit.Text = item.EditText;
                this.updateTextEdit.Text = item.UpdateText;
                this.cancelTextEdit.Text = item.CancelText;
                switch (item.ButtonType)
                {
                    case ButtonColumnType.LinkButton:
                        this.buttonTypeCombo.SelectedIndex = 0;
                        return;

                    case ButtonColumnType.PushButton:
                        this.buttonTypeCombo.SelectedIndex = 1;
                        return;
                }
            }

            private void OnColumnChanged(object source, EventArgs e)
            {
                this.OnChanged(EventArgs.Empty);
            }

            public override void SaveColumn()
            {
                base.SaveColumn();
                DataGridColumnsPage.EditCommandColumnItem columnItem = (DataGridColumnsPage.EditCommandColumnItem) base.columnItem;
                columnItem.EditText = this.editTextEdit.Text;
                columnItem.UpdateText = this.updateTextEdit.Text;
                columnItem.CancelText = this.cancelTextEdit.Text;
                switch (this.buttonTypeCombo.SelectedIndex)
                {
                    case 0:
                        columnItem.ButtonType = ButtonColumnType.LinkButton;
                        return;

                    case 1:
                        columnItem.ButtonType = ButtonColumnType.PushButton;
                        return;
                }
            }
        }

        private class EditCommandColumnItem : DataGridColumnsPage.ColumnItem
        {
            private ButtonColumnType buttonType;
            private string cancelText;
            private string editText;
            private string updateText;

            public EditCommandColumnItem(EditCommandColumn runtimeColumn) : base(runtimeColumn, 4)
            {
            }

            protected override string GetDefaultHeaderText()
            {
                return System.Design.SR.GetString("DGCol_Node_Edit");
            }

            public override TemplateColumn GetTemplateColumn(System.Web.UI.WebControls.DataGrid dataGrid)
            {
                TemplateColumn templateColumn = base.GetTemplateColumn(dataGrid);
                templateColumn.ItemTemplate = base.GetTemplate(dataGrid, this.GetTemplateContent(false));
                templateColumn.EditItemTemplate = base.GetTemplate(dataGrid, this.GetTemplateContent(true));
                return templateColumn;
            }

            private string GetTemplateContent(bool editMode)
            {
                StringBuilder builder = new StringBuilder();
                string str = (this.buttonType == ButtonColumnType.LinkButton) ? "LinkButton" : "Button";
                builder.Append("<asp:");
                builder.Append(str);
                builder.Append(" runat=\"server\"");
                builder.Append(" Text=\"");
                if (!editMode)
                {
                    builder.Append(this.editText);
                }
                else
                {
                    builder.Append(this.updateText);
                }
                builder.Append("\"");
                builder.Append(" CommandName=\"");
                if (!editMode)
                {
                    builder.Append("Edit\"");
                    builder.Append(" CausesValidation=\"false\"");
                }
                else
                {
                    builder.Append("Update\"");
                }
                builder.Append("></asp:");
                builder.Append(str);
                builder.Append(">");
                if (editMode)
                {
                    builder.Append("&nbsp;");
                    builder.Append("<asp:");
                    builder.Append(str);
                    builder.Append(" runat=\"server\"");
                    builder.Append(" Text=\"");
                    builder.Append(this.cancelText);
                    builder.Append("\"");
                    builder.Append(" CommandName=\"");
                    builder.Append("Cancel\"");
                    builder.Append(" CausesValidation=\"false\"></asp:");
                    builder.Append(str);
                    builder.Append(">");
                }
                return builder.ToString();
            }

            public override void LoadColumnInfo()
            {
                base.LoadColumnInfo();
                EditCommandColumn runtimeColumn = (EditCommandColumn) base.RuntimeColumn;
                this.editText = runtimeColumn.EditText;
                this.updateText = runtimeColumn.UpdateText;
                this.cancelText = runtimeColumn.CancelText;
                this.buttonType = runtimeColumn.ButtonType;
            }

            public override void SaveColumnInfo()
            {
                base.SaveColumnInfo();
                EditCommandColumn runtimeColumn = (EditCommandColumn) base.RuntimeColumn;
                runtimeColumn.EditText = this.editText;
                runtimeColumn.UpdateText = this.updateText;
                runtimeColumn.CancelText = this.cancelText;
                runtimeColumn.ButtonType = this.buttonType;
            }

            public ButtonColumnType ButtonType
            {
                get
                {
                    return this.buttonType;
                }
                set
                {
                    this.buttonType = value;
                }
            }

            public string CancelText
            {
                get
                {
                    return this.cancelText;
                }
                set
                {
                    this.cancelText = value;
                }
            }

            public string EditText
            {
                get
                {
                    return this.editText;
                }
                set
                {
                    this.editText = value;
                }
            }

            public string UpdateText
            {
                get
                {
                    return this.updateText;
                }
                set
                {
                    this.updateText = value;
                }
            }
        }

        private class EditCommandNode : DataGridColumnsPage.AvailableColumnNode
        {
            public EditCommandNode() : base(System.Design.SR.GetString("DGCol_Node_Edit"), 4)
            {
            }

            public override DataGridColumnsPage.ColumnItem CreateColumn()
            {
                EditCommandColumn runtimeColumn = new EditCommandColumn {
                    EditText = System.Design.SR.GetString("DGCol_EditButton"),
                    UpdateText = System.Design.SR.GetString("DGCol_UpdateButton"),
                    CancelText = System.Design.SR.GetString("DGCol_CancelButton")
                };
                DataGridColumnsPage.ColumnItem item = new DataGridColumnsPage.EditCommandColumnItem(runtimeColumn);
                item.LoadColumnInfo();
                return item;
            }
        }

        private class HyperLinkColumnEditor : DataGridColumnsPage.ColumnItemEditor
        {
            private UnsettableComboBox dataTextFieldCombo;
            private System.Windows.Forms.TextBox dataTextFieldEdit;
            private System.Windows.Forms.TextBox dataTextFormatStringEdit;
            private UnsettableComboBox dataUrlFieldCombo;
            private System.Windows.Forms.TextBox dataUrlFieldEdit;
            private System.Windows.Forms.TextBox dataUrlFormatStringEdit;
            private ComboBox targetCombo;
            private System.Windows.Forms.TextBox textEdit;
            private System.Windows.Forms.TextBox urlEdit;

            public override void AddDataField(string fieldName)
            {
                this.dataTextFieldCombo.AddItem(fieldName);
                this.dataUrlFieldCombo.AddItem(fieldName);
                base.AddDataField(fieldName);
            }

            public override void ClearDataFields()
            {
                this.dataTextFieldCombo.Items.Clear();
                this.dataUrlFieldCombo.Items.Clear();
                this.dataTextFieldCombo.EnsureNotSetItem();
                this.dataUrlFieldCombo.EnsureNotSetItem();
                base.ClearDataFields();
            }

            protected override void InitPanel()
            {
                System.Windows.Forms.Label label = new System.Windows.Forms.Label();
                this.textEdit = new System.Windows.Forms.TextBox();
                System.Windows.Forms.Label label2 = new System.Windows.Forms.Label();
                this.dataTextFieldCombo = new UnsettableComboBox();
                this.dataTextFieldEdit = new System.Windows.Forms.TextBox();
                System.Windows.Forms.Label label3 = new System.Windows.Forms.Label();
                this.dataTextFormatStringEdit = new System.Windows.Forms.TextBox();
                System.Windows.Forms.Label label4 = new System.Windows.Forms.Label();
                this.targetCombo = new ComboBox();
                System.Windows.Forms.Label label5 = new System.Windows.Forms.Label();
                this.urlEdit = new System.Windows.Forms.TextBox();
                System.Windows.Forms.Label label6 = new System.Windows.Forms.Label();
                this.dataUrlFieldCombo = new UnsettableComboBox();
                this.dataUrlFieldEdit = new System.Windows.Forms.TextBox();
                System.Windows.Forms.Label label7 = new System.Windows.Forms.Label();
                this.dataUrlFormatStringEdit = new System.Windows.Forms.TextBox();
                label.SetBounds(0, 0, 160, 14);
                label.Text = System.Design.SR.GetString("DGCol_HC_Text");
                label.TabStop = false;
                label.TabIndex = 1;
                label.Name = "HyperlinkColumnTextLabel";
                this.textEdit.SetBounds(0, 0x10, 0xb6, 0x18);
                this.textEdit.TabIndex = 2;
                this.textEdit.TextChanged += new EventHandler(this.OnColumnChanged);
                this.textEdit.Name = "HyperlinkColumnTextEdit";
                label2.SetBounds(0, 40, 160, 14);
                label2.Text = System.Design.SR.GetString("DGCol_HC_DataTextField");
                label2.TabStop = false;
                label2.TabIndex = 3;
                label2.Name = "HyperlinkColumnDataTextFieldLabel";
                this.dataTextFieldCombo.SetBounds(0, 0x38, 0xb6, 0x15);
                this.dataTextFieldCombo.DropDownStyle = ComboBoxStyle.DropDownList;
                this.dataTextFieldCombo.TabIndex = 4;
                this.dataTextFieldCombo.SelectedIndexChanged += new EventHandler(this.OnColumnChanged);
                this.dataTextFieldCombo.Name = "HyperlinkColumnDataTextFieldCombo";
                this.dataTextFieldEdit.SetBounds(0, 0x38, 0xb6, 14);
                this.dataTextFieldEdit.TabIndex = 4;
                this.dataTextFieldEdit.TextChanged += new EventHandler(this.OnColumnChanged);
                this.dataTextFieldEdit.Name = "HyperlinkColumnDataTextFieldEdit";
                label3.SetBounds(0, 0x52, 160, 14);
                label3.Text = System.Design.SR.GetString("DGCol_HC_DataTextFormat");
                label3.TabStop = false;
                label3.TabIndex = 5;
                label3.Name = "HyperlinkColumnDataTextFormatStringLabel";
                this.dataTextFormatStringEdit.SetBounds(0, 0x62, 0xb6, 0x15);
                this.dataTextFormatStringEdit.TabIndex = 6;
                this.dataTextFormatStringEdit.TextChanged += new EventHandler(this.OnColumnChanged);
                this.dataTextFormatStringEdit.Name = "HyperlinkColumnDataTextFormatStringEdit";
                label4.SetBounds(0, 0x7b, 160, 14);
                label4.Text = System.Design.SR.GetString("DGCol_HC_Target");
                label4.TabStop = false;
                label4.TabIndex = 7;
                label4.Name = "HyperlinkColumnTargetLabel";
                this.targetCombo.SetBounds(0, 0x8b, 0xb6, 0x15);
                this.targetCombo.TabIndex = 8;
                this.targetCombo.Items.AddRange(new object[] { "_blank", "_parent", "_search", "_self", "_top" });
                this.targetCombo.SelectedIndexChanged += new EventHandler(this.OnColumnChanged);
                this.targetCombo.TextChanged += new EventHandler(this.OnColumnChanged);
                this.targetCombo.Name = "HyperlinkColumnTargetCombo";
                label5.SetBounds(200, 0, 160, 14);
                label5.Text = System.Design.SR.GetString("DGCol_HC_URL");
                label5.TabStop = false;
                label5.TabIndex = 10;
                label5.Name = "HyperlinkColumnUrlLabel";
                this.urlEdit.SetBounds(200, 0x10, 0xb6, 0x18);
                this.urlEdit.TabIndex = 11;
                this.urlEdit.TextChanged += new EventHandler(this.OnColumnChanged);
                this.urlEdit.Name = "HyperlinkColumnUrlEdit";
                label6.SetBounds(200, 40, 160, 14);
                label6.Text = System.Design.SR.GetString("DGCol_HC_DataURLField");
                label6.TabStop = false;
                label6.TabIndex = 12;
                label6.Name = "HyperlinkColumnDataUrlFieldLabel";
                this.dataUrlFieldCombo.SetBounds(200, 0x38, 0xb6, 0x15);
                this.dataUrlFieldCombo.DropDownStyle = ComboBoxStyle.DropDownList;
                this.dataUrlFieldCombo.TabIndex = 13;
                this.dataUrlFieldCombo.SelectedIndexChanged += new EventHandler(this.OnColumnChanged);
                this.dataUrlFieldCombo.Name = "HyperlinkColumnDataUrlFieldCombo";
                this.dataUrlFieldEdit.SetBounds(200, 0x38, 0xb6, 14);
                this.dataUrlFieldEdit.TabIndex = 13;
                this.dataUrlFieldEdit.TextChanged += new EventHandler(this.OnColumnChanged);
                this.dataUrlFieldEdit.Name = "HyperlinkColumnDataUrlFieldEdit";
                label7.SetBounds(200, 0x52, 160, 14);
                label7.Text = System.Design.SR.GetString("DGCol_HC_DataURLFormat");
                label7.TabStop = false;
                label7.TabIndex = 14;
                label7.Name = "HyperlinkColumnDataUrlFormatStringLabel";
                this.dataUrlFormatStringEdit.SetBounds(200, 0x62, 0xb6, 0x15);
                this.dataUrlFormatStringEdit.TabIndex = 15;
                this.dataUrlFormatStringEdit.TextChanged += new EventHandler(this.OnColumnChanged);
                this.dataUrlFormatStringEdit.Name = "HyperlinkColumnDataUrlFormatStringEdit";
                base.Controls.Clear();
                base.Controls.AddRange(new System.Windows.Forms.Control[] { this.dataUrlFormatStringEdit, label7, this.dataUrlFieldEdit, this.dataUrlFieldCombo, label6, this.urlEdit, label5, this.targetCombo, label4, this.dataTextFormatStringEdit, label3, this.dataTextFieldEdit, this.dataTextFieldCombo, label2, this.textEdit, label });
            }

            public override void LoadColumn(DataGridColumnsPage.ColumnItem columnItem)
            {
                base.LoadColumn(columnItem);
                DataGridColumnsPage.HyperLinkColumnItem item = (DataGridColumnsPage.HyperLinkColumnItem) base.columnItem;
                this.textEdit.Text = item.AnchorText;
                if (base.dataFieldsAvailable)
                {
                    if (item.AnchorDataTextField != null)
                    {
                        int num = this.dataTextFieldCombo.FindStringExact(item.AnchorDataTextField);
                        this.dataTextFieldCombo.SelectedIndex = num;
                    }
                    this.dataTextFieldCombo.Visible = true;
                    this.dataTextFieldEdit.Visible = false;
                }
                else
                {
                    this.dataTextFieldEdit.Text = item.AnchorDataTextField;
                    this.dataTextFieldEdit.Visible = true;
                    this.dataTextFieldCombo.Visible = false;
                }
                this.dataTextFormatStringEdit.Text = item.AnchorDataTextFormatString;
                this.urlEdit.Text = item.Url;
                if (base.dataFieldsAvailable)
                {
                    if (item.DataUrlField != null)
                    {
                        int num2 = this.dataTextFieldCombo.FindStringExact(item.DataUrlField);
                        this.dataUrlFieldCombo.SelectedIndex = num2;
                    }
                    this.dataUrlFieldCombo.Visible = true;
                    this.dataUrlFieldEdit.Visible = false;
                }
                else
                {
                    this.dataUrlFieldEdit.Text = item.DataUrlField;
                    this.dataUrlFieldEdit.Visible = true;
                    this.dataUrlFieldCombo.Visible = false;
                }
                this.dataUrlFormatStringEdit.Text = item.DataUrlFormatString;
                this.targetCombo.Text = item.Target;
                this.UpdateEnabledState();
            }

            protected void OnColumnChanged(object source, EventArgs e)
            {
                this.OnChanged(EventArgs.Empty);
                if (((source == this.dataTextFieldCombo) || (source == this.dataUrlFieldCombo)) || ((source == this.dataTextFieldEdit) || (source == this.dataUrlFieldEdit)))
                {
                    this.UpdateEnabledState();
                }
            }

            public override void SaveColumn()
            {
                base.SaveColumn();
                DataGridColumnsPage.HyperLinkColumnItem columnItem = (DataGridColumnsPage.HyperLinkColumnItem) base.columnItem;
                columnItem.AnchorText = this.textEdit.Text;
                if (base.dataFieldsAvailable)
                {
                    if (this.dataTextFieldCombo.IsSet())
                    {
                        columnItem.AnchorDataTextField = this.dataTextFieldCombo.Text;
                    }
                    else
                    {
                        columnItem.AnchorDataTextField = string.Empty;
                    }
                }
                else
                {
                    columnItem.AnchorDataTextField = this.dataTextFieldEdit.Text.Trim();
                }
                columnItem.AnchorDataTextFormatString = this.dataTextFormatStringEdit.Text;
                columnItem.Url = this.urlEdit.Text.Trim();
                if (base.dataFieldsAvailable)
                {
                    if (this.dataUrlFieldCombo.IsSet())
                    {
                        columnItem.DataUrlField = this.dataUrlFieldCombo.Text;
                    }
                    else
                    {
                        columnItem.DataUrlField = string.Empty;
                    }
                }
                else
                {
                    columnItem.DataUrlField = this.dataUrlFieldEdit.Text.Trim();
                }
                columnItem.DataUrlFormatString = this.dataUrlFormatStringEdit.Text;
                columnItem.Target = this.targetCombo.Text.Trim();
            }

            private void UpdateEnabledState()
            {
                if (base.dataFieldsAvailable)
                {
                    this.dataTextFormatStringEdit.Enabled = this.dataTextFieldCombo.IsSet();
                    this.dataUrlFormatStringEdit.Enabled = this.dataUrlFieldCombo.IsSet();
                }
                else
                {
                    this.dataTextFormatStringEdit.Enabled = this.dataTextFieldEdit.Text.Trim().Length != 0;
                    this.dataUrlFormatStringEdit.Enabled = this.dataUrlFieldEdit.Text.Trim().Length != 0;
                }
            }
        }

        private class HyperLinkColumnItem : DataGridColumnsPage.ColumnItem
        {
            protected string anchorDataTextField;
            protected string anchorDataTextFormatString;
            protected string anchorText;
            protected string dataUrlField;
            protected string dataUrlFormatString;
            protected string target;
            protected string url;

            public HyperLinkColumnItem(HyperLinkColumn runtimeColumn) : base(runtimeColumn, 8)
            {
            }

            protected override string GetDefaultHeaderText()
            {
                if ((this.anchorText != null) && (this.anchorText.Length != 0))
                {
                    return this.anchorText;
                }
                return System.Design.SR.GetString("DGCol_Node_HyperLink");
            }

            public override TemplateColumn GetTemplateColumn(System.Web.UI.WebControls.DataGrid dataGrid)
            {
                TemplateColumn templateColumn = base.GetTemplateColumn(dataGrid);
                StringBuilder builder = new StringBuilder();
                builder.Append("<asp:HyperLink");
                builder.Append(" runat=\"server\"");
                if (this.anchorDataTextField.Length != 0)
                {
                    builder.Append(" Text='<%# DataBinder.Eval(Container, \"DataItem.");
                    builder.Append(this.anchorDataTextField);
                    builder.Append("\"");
                    if (this.anchorDataTextFormatString.Length != 0)
                    {
                        builder.Append(", \"");
                        builder.Append(this.anchorDataTextFormatString);
                        builder.Append("\"");
                    }
                    builder.Append(") %>'");
                }
                else
                {
                    builder.Append(" Text=\"");
                    builder.Append(this.anchorText);
                    builder.Append("\"");
                }
                if (this.dataUrlField.Length != 0)
                {
                    builder.Append(" NavigateUrl='<%# DataBinder.Eval(Container, \"DataItem.");
                    builder.Append(this.dataUrlField);
                    builder.Append("\"");
                    if (this.dataUrlFormatString.Length != 0)
                    {
                        builder.Append(", \"");
                        builder.Append(this.dataUrlFormatString);
                        builder.Append("\"");
                    }
                    builder.Append(") %>'");
                }
                else
                {
                    builder.Append(" NavigateUrl=\"");
                    builder.Append(this.url);
                    builder.Append("\"");
                }
                if (this.target.Length != 0)
                {
                    builder.Append(" Target=\"");
                    builder.Append(this.target);
                    builder.Append("\"");
                }
                builder.Append("></asp:HyperLink>");
                templateColumn.ItemTemplate = base.GetTemplate(dataGrid, builder.ToString());
                return templateColumn;
            }

            public override void LoadColumnInfo()
            {
                base.LoadColumnInfo();
                HyperLinkColumn runtimeColumn = (HyperLinkColumn) base.RuntimeColumn;
                this.anchorText = runtimeColumn.Text;
                this.anchorDataTextField = runtimeColumn.DataTextField;
                this.anchorDataTextFormatString = runtimeColumn.DataTextFormatString;
                this.url = runtimeColumn.NavigateUrl;
                this.dataUrlField = runtimeColumn.DataNavigateUrlField;
                this.dataUrlFormatString = runtimeColumn.DataNavigateUrlFormatString;
                this.target = runtimeColumn.Target;
                base.UpdateDisplayText();
            }

            public override void SaveColumnInfo()
            {
                base.SaveColumnInfo();
                HyperLinkColumn runtimeColumn = (HyperLinkColumn) base.RuntimeColumn;
                runtimeColumn.Text = this.anchorText;
                runtimeColumn.DataTextField = this.anchorDataTextField;
                runtimeColumn.DataTextFormatString = this.anchorDataTextFormatString;
                runtimeColumn.NavigateUrl = this.url;
                runtimeColumn.DataNavigateUrlField = this.dataUrlField;
                runtimeColumn.DataNavigateUrlFormatString = this.dataUrlFormatString;
                runtimeColumn.Target = this.target;
            }

            public string AnchorDataTextField
            {
                get
                {
                    return this.anchorDataTextField;
                }
                set
                {
                    this.anchorDataTextField = value;
                }
            }

            public string AnchorDataTextFormatString
            {
                get
                {
                    return this.anchorDataTextFormatString;
                }
                set
                {
                    this.anchorDataTextFormatString = value;
                }
            }

            public string AnchorText
            {
                get
                {
                    return this.anchorText;
                }
                set
                {
                    this.anchorText = value;
                    base.UpdateDisplayText();
                }
            }

            public string DataUrlField
            {
                get
                {
                    return this.dataUrlField;
                }
                set
                {
                    this.dataUrlField = value;
                }
            }

            public string DataUrlFormatString
            {
                get
                {
                    return this.dataUrlFormatString;
                }
                set
                {
                    this.dataUrlFormatString = value;
                }
            }

            public string Target
            {
                get
                {
                    return this.target;
                }
                set
                {
                    this.target = value;
                }
            }

            public string Url
            {
                get
                {
                    return this.url;
                }
                set
                {
                    this.url = value;
                }
            }
        }

        private class HyperLinkNode : DataGridColumnsPage.AvailableColumnNode
        {
            private string hyperLinkText;

            public HyperLinkNode() : this(System.Design.SR.GetString("DGCol_HyperLink"))
            {
            }

            public HyperLinkNode(string hyperLinkText) : base(System.Design.SR.GetString("DGCol_Node_HyperLink"), 8)
            {
                this.hyperLinkText = hyperLinkText;
            }

            public override DataGridColumnsPage.ColumnItem CreateColumn()
            {
                HyperLinkColumn runtimeColumn = new HyperLinkColumn();
                DataGridColumnsPage.ColumnItem item = new DataGridColumnsPage.HyperLinkColumnItem(runtimeColumn) {
                    Text = this.hyperLinkText
                };
                item.LoadColumnInfo();
                return item;
            }
        }

        private class TemplateColumnItem : DataGridColumnsPage.ColumnItem
        {
            public TemplateColumnItem(TemplateColumn runtimeColumn) : base(runtimeColumn, 9)
            {
            }

            protected override string GetDefaultHeaderText()
            {
                return System.Design.SR.GetString("DGCol_Node_Template");
            }
        }

        private class TemplateNode : DataGridColumnsPage.AvailableColumnNode
        {
            public TemplateNode() : base(System.Design.SR.GetString("DGCol_Node_Template"), 9)
            {
            }

            public override DataGridColumnsPage.ColumnItem CreateColumn()
            {
                TemplateColumn runtimeColumn = new TemplateColumn();
                DataGridColumnsPage.ColumnItem item = new DataGridColumnsPage.TemplateColumnItem(runtimeColumn);
                item.LoadColumnInfo();
                return item;
            }
        }
    }
}

