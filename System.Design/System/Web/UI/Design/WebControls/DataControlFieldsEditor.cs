namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class DataControlFieldsEditor : DesignerForm
    {
        private System.Windows.Forms.Button _addFieldButton;
        private System.Windows.Forms.CheckBox _autoFieldCheck;
        private System.Windows.Forms.Label _availableFieldsLabel;
        private TreeViewWithEnter _availableFieldsTree;
        private System.Windows.Forms.Button _cancelButton;
        private DataControlFieldCollection _clonedFieldCollection;
        private DataBoundControlDesigner _controlDesigner;
        private FieldItem _currentFieldItem;
        private PropertyGrid _currentFieldProps;
        private IDictionary<System.Type, DataControlFieldDesigner> _customFieldDesigners;
        private System.Windows.Forms.Button _deleteFieldButton;
        private bool _fieldMovePending;
        private IDataSourceFieldSchema[] _fieldSchemas;
        private bool _initialActivate;
        private bool _initialIgnoreRefreshSchemaValue;
        private bool _isLoading;
        private System.Windows.Forms.Button _moveFieldDownButton;
        private System.Windows.Forms.Button _moveFieldUpButton;
        private System.Windows.Forms.Button _okButton;
        private bool _propChangesPending;
        private LinkLabel _refreshSchemaLink;
        private BoolDataSourceNode _selectedCheckBoxDataSourceNode;
        private DataSourceNode _selectedDataSourceNode;
        private System.Windows.Forms.Label _selFieldLabel;
        private System.Windows.Forms.Label _selFieldsLabel;
        private ListViewWithEnter _selFieldsList;
        private LinkLabel _templatizeLink;
        private IDataSourceViewSchema _viewSchema;
        private const int CF_DELETE = 3;
        private const int CF_EDIT = 0;
        private const int CF_INSERT = 1;
        private const int CF_SELECT = 2;
        private const int ILI_ALL = 2;
        private const int ILI_BOOLDATASOURCE = 13;
        private const int ILI_BOUND = 1;
        private const int ILI_BUTTON = 4;
        private const int ILI_CHECKBOX = 10;
        private const int ILI_COMMAND = 12;
        private const int ILI_CUSTOM = 3;
        private const int ILI_DATASOURCE = 0;
        private const int ILI_DELETEBUTTON = 7;
        private const int ILI_EDITBUTTON = 6;
        private const int ILI_FIELDDESIGNER = 15;
        private const int ILI_HYPERLINK = 8;
        private const int ILI_IMAGE = 14;
        private const int ILI_INSERTBUTTON = 11;
        private const int ILI_SELECTBUTTON = 5;
        private const int ILI_TEMPLATE = 9;
        private const int MODE_EDIT = 1;
        private const int MODE_INSERT = 2;
        private const int MODE_READONLY = 0;

        public DataControlFieldsEditor(DataBoundControlDesigner controlDesigner) : base(controlDesigner.Component.Site)
        {
            this._controlDesigner = controlDesigner;
            this.InitializeComponent();
            this.InitForm();
            this._initialActivate = true;
            this.IgnoreRefreshSchemaEvents();
        }

        private void EnterLoadingMode()
        {
            this._isLoading = true;
        }

        private void ExitLoadingMode()
        {
            this._isLoading = false;
        }

        private IDataSourceFieldSchema[] GetFieldSchemas()
        {
            if (this._fieldSchemas == null)
            {
                IDataSourceViewSchema viewSchema = this.GetViewSchema();
                if (viewSchema != null)
                {
                    this._fieldSchemas = viewSchema.GetFields();
                }
            }
            return this._fieldSchemas;
        }

        private string GetNewDataSourceName(System.Type controlType, int editMode)
        {
            int startIndex = 1;
            return this.GetNewDataSourceName(controlType, editMode, ref startIndex);
        }

        internal string GetNewDataSourceName(System.Type controlType, DataBoundControlMode mode)
        {
            if (mode == DataBoundControlMode.Edit)
            {
                return this.GetNewDataSourceName(controlType, 1);
            }
            if (mode == DataBoundControlMode.Insert)
            {
                return this.GetNewDataSourceName(controlType, 2);
            }
            return this.GetNewDataSourceName(controlType, 0);
        }

        private string GetNewDataSourceName(System.Type controlType, int editMode, ref int startIndex)
        {
            int num = startIndex;
            DataControlFieldCollection fields = new DataControlFieldCollection();
            int count = this._selFieldsList.Items.Count;
            for (int i = 0; i < count; i++)
            {
                FieldItem item = (FieldItem) this._selFieldsList.Items[i];
                fields.Add(item.RuntimeField);
            }
            if ((fields != null) && (fields.Count > 0))
            {
                bool flag = false;
                while (!flag)
                {
                    for (int j = 0; j < fields.Count; j++)
                    {
                        DataControlField field = fields[j];
                        if (field is TemplateField)
                        {
                            ITemplate itemTemplate = null;
                            switch (editMode)
                            {
                                case 0:
                                    itemTemplate = ((TemplateField) field).ItemTemplate;
                                    break;

                                case 1:
                                    itemTemplate = ((TemplateField) field).EditItemTemplate;
                                    break;

                                case 2:
                                    itemTemplate = ((TemplateField) field).InsertItemTemplate;
                                    break;
                            }
                            if (itemTemplate != null)
                            {
                                IDesignerHost service = (IDesignerHost) this.Control.Site.GetService(typeof(IDesignerHost));
                                if (ControlSerializer.SerializeTemplate(itemTemplate, service).Contains(controlType.Name + num.ToString(NumberFormatInfo.InvariantInfo)))
                                {
                                    num++;
                                    continue;
                                }
                            }
                        }
                        if (j == (fields.Count - 1))
                        {
                            flag = true;
                        }
                    }
                }
            }
            startIndex = num;
            return (controlType.Name + num.ToString(NumberFormatInfo.InvariantInfo));
        }

        private IDataSourceViewSchema GetViewSchema()
        {
            if ((this._viewSchema == null) && (this._controlDesigner != null))
            {
                DesignerDataSourceView designerView = this._controlDesigner.DesignerView;
                if (designerView != null)
                {
                    try
                    {
                        this._viewSchema = designerView.Schema;
                    }
                    catch (Exception exception)
                    {
                        IComponentDesignerDebugService service = (IComponentDesignerDebugService) base.ServiceProvider.GetService(typeof(IComponentDesignerDebugService));
                        if (service != null)
                        {
                            service.Fail(System.Design.SR.GetString("DataSource_DebugService_FailedCall", new object[] { "DesignerDataSourceView.Schema", exception.Message }));
                        }
                    }
                }
            }
            return this._viewSchema;
        }

        private void IgnoreRefreshSchemaEvents()
        {
            this._initialIgnoreRefreshSchemaValue = this.IgnoreRefreshSchema;
            this.IgnoreRefreshSchema = true;
            IDataSourceDesigner dataSourceDesigner = this._controlDesigner.DataSourceDesigner;
            if (dataSourceDesigner != null)
            {
                dataSourceDesigner.SuppressDataSourceEvents();
            }
        }

        private void InitForm()
        {
            System.Drawing.Image image = new Bitmap(base.GetType(), "FieldNodes.bmp");
            ImageList list = new ImageList {
                TransparentColor = Color.Magenta
            };
            list.Images.AddStrip(image);
            this._autoFieldCheck.Text = System.Design.SR.GetString("DCFEditor_AutoGen");
            this._availableFieldsTree.ImageList = list;
            this._addFieldButton.Text = System.Design.SR.GetString("DCFEditor_Add");
            ColumnHeader header = new ColumnHeader {
                Width = this._selFieldsList.Width - 4
            };
            this._selFieldsList.Columns.Add(header);
            this._selFieldsList.SmallImageList = list;
            Bitmap bitmap = new Icon(base.GetType(), "SortUp.ico").ToBitmap();
            bitmap.MakeTransparent();
            this._moveFieldUpButton.Image = bitmap;
            this._moveFieldUpButton.AccessibleDescription = System.Design.SR.GetString("DCFEditor_MoveFieldUpDesc");
            this._moveFieldUpButton.AccessibleName = System.Design.SR.GetString("DCFEditor_MoveFieldUpName");
            Bitmap bitmap2 = new Icon(base.GetType(), "SortDown.ico").ToBitmap();
            bitmap2.MakeTransparent();
            this._moveFieldDownButton.Image = bitmap2;
            this._moveFieldDownButton.AccessibleDescription = System.Design.SR.GetString("DCFEditor_MoveFieldDownDesc");
            this._moveFieldDownButton.AccessibleName = System.Design.SR.GetString("DCFEditor_MoveFieldDownName");
            Bitmap bitmap3 = new Icon(base.GetType(), "Delete.ico").ToBitmap();
            bitmap3.MakeTransparent();
            this._deleteFieldButton.Image = bitmap3;
            this._deleteFieldButton.AccessibleDescription = System.Design.SR.GetString("DCFEditor_DeleteFieldDesc");
            this._deleteFieldButton.AccessibleName = System.Design.SR.GetString("DCFEditor_DeleteFieldName");
            this._templatizeLink.Text = System.Design.SR.GetString("DCFEditor_Templatize");
            this._refreshSchemaLink.Text = System.Design.SR.GetString("DataSourceDesigner_RefreshSchemaNoHotkey");
            this._refreshSchemaLink.Visible = (this._controlDesigner.DataSourceDesigner != null) && this._controlDesigner.DataSourceDesigner.CanRefreshSchema;
            this._okButton.Text = System.Design.SR.GetString("OKCaption");
            this._cancelButton.Text = System.Design.SR.GetString("CancelCaption");
            this._selFieldLabel.Text = System.Design.SR.GetString("DCFEditor_FieldProps");
            this._availableFieldsLabel.Text = System.Design.SR.GetString("DCFEditor_AvailableFields");
            this._selFieldsLabel.Text = System.Design.SR.GetString("DCFEditor_SelectedFields");
            this._currentFieldProps.Site = this._controlDesigner.Component.Site;
            this.Text = System.Design.SR.GetString("DCFEditor_Text");
            base.Icon = new Icon(base.GetType(), "DataControlFieldsEditor.ico");
        }

        private void InitializeComponent()
        {
            this._availableFieldsTree = new TreeViewWithEnter();
            this._selFieldsList = new ListViewWithEnter();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._moveFieldUpButton = new System.Windows.Forms.Button();
            this._moveFieldDownButton = new System.Windows.Forms.Button();
            this._addFieldButton = new System.Windows.Forms.Button();
            this._deleteFieldButton = new System.Windows.Forms.Button();
            this._currentFieldProps = new VsPropertyGrid(base.ServiceProvider);
            this._autoFieldCheck = new System.Windows.Forms.CheckBox();
            this._refreshSchemaLink = new LinkLabel();
            this._templatizeLink = new LinkLabel();
            this._selFieldLabel = new System.Windows.Forms.Label();
            this._availableFieldsLabel = new System.Windows.Forms.Label();
            this._selFieldsLabel = new System.Windows.Forms.Label();
            base.SuspendLayout();
            this._availableFieldsTree.HideSelection = false;
            this._availableFieldsTree.ImageIndex = -1;
            this._availableFieldsTree.Indent = 15;
            this._availableFieldsTree.Location = new Point(12, 0x1c);
            this._availableFieldsTree.Name = "_availableFieldsTree";
            this._availableFieldsTree.SelectedImageIndex = -1;
            this._availableFieldsTree.Size = new Size(0xc4, 0x74);
            this._availableFieldsTree.TabIndex = 1;
            this._availableFieldsTree.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(this.OnAvailableFieldsDoubleClick);
            this._availableFieldsTree.AfterSelect += new TreeViewEventHandler(this.OnSelChangedAvailableFields);
            this._availableFieldsTree.GotFocus += new EventHandler(this.OnAvailableFieldsGotFocus);
            this._availableFieldsTree.KeyPress += new KeyPressEventHandler(this.OnAvailableFieldsKeyPress);
            this._selFieldsList.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._selFieldsList.HeaderStyle = ColumnHeaderStyle.None;
            this._selFieldsList.HideSelection = false;
            this._selFieldsList.LabelWrap = false;
            this._selFieldsList.Location = new Point(12, 0xc5);
            this._selFieldsList.MultiSelect = false;
            this._selFieldsList.Name = "_selFieldsList";
            this._selFieldsList.Size = new Size(0xa4, 0x70);
            this._selFieldsList.TabIndex = 4;
            this._selFieldsList.View = System.Windows.Forms.View.Details;
            this._selFieldsList.KeyDown += new KeyEventHandler(this.OnSelFieldsListKeyDown);
            this._selFieldsList.SelectedIndexChanged += new EventHandler(this.OnSelIndexChangedSelFieldsList);
            this._selFieldsList.ItemActivate += new EventHandler(this.OnClickDeleteField);
            this._selFieldsList.GotFocus += new EventHandler(this.OnSelFieldsListGotFocus);
            this._okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._okButton.DialogResult = DialogResult.OK;
            this._okButton.FlatStyle = FlatStyle.System;
            this._okButton.Location = new Point(340, 350);
            this._okButton.Name = "_okButton";
            this._okButton.TabIndex = 100;
            this._okButton.Click += new EventHandler(this.OnClickOK);
            this._cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._cancelButton.DialogResult = DialogResult.Cancel;
            this._cancelButton.FlatStyle = FlatStyle.System;
            this._cancelButton.Location = new Point(420, 350);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.TabIndex = 0x65;
            this._moveFieldUpButton.Location = new Point(0xba, 0xc5);
            this._moveFieldUpButton.Name = "_moveFieldUpButton";
            this._moveFieldUpButton.Size = new Size(0x1a, 0x17);
            this._moveFieldUpButton.TabIndex = 5;
            this._moveFieldUpButton.Click += new EventHandler(this.OnClickMoveFieldUp);
            this._moveFieldDownButton.Location = new Point(0xba, 0xdd);
            this._moveFieldDownButton.Name = "_moveFieldDownButton";
            this._moveFieldDownButton.Size = new Size(0x1a, 0x17);
            this._moveFieldDownButton.TabIndex = 6;
            this._moveFieldDownButton.Click += new EventHandler(this.OnClickMoveFieldDown);
            this._addFieldButton.FlatStyle = FlatStyle.System;
            this._addFieldButton.Location = new Point(0x7b, 150);
            this._addFieldButton.Name = "_addFieldButton";
            this._addFieldButton.Size = new Size(0x55, 0x17);
            this._addFieldButton.TabIndex = 2;
            this._addFieldButton.Click += new EventHandler(this.OnClickAddField);
            this._deleteFieldButton.Location = new Point(0xba, 0xf5);
            this._deleteFieldButton.Name = "_deleteFieldButton";
            this._deleteFieldButton.Size = new Size(0x1a, 0x17);
            this._deleteFieldButton.TabIndex = 7;
            this._deleteFieldButton.Click += new EventHandler(this.OnClickDeleteField);
            this._currentFieldProps.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._currentFieldProps.CommandsVisibleIfAvailable = true;
            this._currentFieldProps.Enabled = false;
            this._currentFieldProps.LargeButtons = false;
            this._currentFieldProps.LineColor = SystemColors.ScrollBar;
            this._currentFieldProps.Location = new Point(0xf4, 0x1c);
            this._currentFieldProps.Name = "_currentFieldProps";
            this._currentFieldProps.Size = new Size(0xf8, 0x119);
            this._currentFieldProps.TabIndex = 9;
            this._currentFieldProps.ToolbarVisible = true;
            this._currentFieldProps.ViewBackColor = SystemColors.Window;
            this._currentFieldProps.ViewForeColor = SystemColors.WindowText;
            this._currentFieldProps.PropertyValueChanged += new PropertyValueChangedEventHandler(this.OnChangedPropertyValues);
            this._autoFieldCheck.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this._autoFieldCheck.FlatStyle = FlatStyle.System;
            this._autoFieldCheck.Location = new Point(12, 0x139);
            this._autoFieldCheck.Name = "_autoFieldCheck";
            this._autoFieldCheck.Size = new Size(0xac, 0x18);
            this._autoFieldCheck.TabIndex = 10;
            this._autoFieldCheck.CheckedChanged += new EventHandler(this.OnCheckChangedAutoField);
            this._autoFieldCheck.TextAlign = ContentAlignment.TopLeft;
            this._autoFieldCheck.CheckAlign = ContentAlignment.TopLeft;
            this._refreshSchemaLink.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this._refreshSchemaLink.Location = new Point(12, 0x15b);
            this._refreshSchemaLink.Name = "_refreshSchemaLink";
            this._refreshSchemaLink.Size = new Size(0xc4, 0x10);
            this._refreshSchemaLink.TabIndex = 11;
            this._refreshSchemaLink.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnClickRefreshSchema);
            this._templatizeLink.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this._templatizeLink.FlatStyle = FlatStyle.System;
            this._templatizeLink.Location = new Point(0xf4, 0x139);
            this._templatizeLink.Name = "_templatizeLink";
            this._templatizeLink.Size = new Size(0xf8, 0x20);
            this._templatizeLink.TabIndex = 12;
            this._templatizeLink.Visible = false;
            this._templatizeLink.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnClickTemplatize);
            this._selFieldLabel.Location = new Point(0xf4, 12);
            this._selFieldLabel.Name = "_selFieldLabel";
            this._selFieldLabel.Size = new Size(0xf8, 0x10);
            this._selFieldLabel.TabIndex = 8;
            this._availableFieldsLabel.Location = new Point(12, 12);
            this._availableFieldsLabel.Name = "_availableFieldsLabel";
            this._availableFieldsLabel.Size = new Size(0xc4, 0x10);
            this._availableFieldsLabel.TabIndex = 0;
            this._selFieldsLabel.Location = new Point(12, 0xb5);
            this._selFieldsLabel.Name = "_selFieldsLabel";
            this._selFieldsLabel.Size = new Size(0xc4, 0x10);
            this._selFieldsLabel.TabIndex = 3;
            base.AcceptButton = this._okButton;
            base.CancelButton = this._cancelButton;
            base.ClientSize = new Size(0x1fb, 0x181);
            base.Controls.Add(this._selFieldsLabel);
            base.Controls.Add(this._availableFieldsLabel);
            base.Controls.Add(this._selFieldLabel);
            base.Controls.Add(this._templatizeLink);
            base.Controls.Add(this._refreshSchemaLink);
            base.Controls.Add(this._autoFieldCheck);
            base.Controls.Add(this._currentFieldProps);
            base.Controls.Add(this._deleteFieldButton);
            base.Controls.Add(this._addFieldButton);
            base.Controls.Add(this._moveFieldDownButton);
            base.Controls.Add(this._moveFieldUpButton);
            base.Controls.Add(this._cancelButton);
            base.Controls.Add(this._okButton);
            base.Controls.Add(this._selFieldsList);
            base.Controls.Add(this._availableFieldsTree);
            this.MinimumSize = new Size(0x203, 0x1a3);
            base.Name = "Form1";
            base.InitializeForm();
            base.ResumeLayout(false);
        }

        private void InitPage()
        {
            this._autoFieldCheck.Checked = false;
            this._selectedDataSourceNode = null;
            this._selectedCheckBoxDataSourceNode = null;
            this._availableFieldsTree.Nodes.Clear();
            this._selFieldsList.Items.Clear();
            this._currentFieldItem = null;
            this._propChangesPending = false;
        }

        private void LoadAvailableFieldsTree()
        {
            IDataSourceFieldSchema[] fieldSchemas = this.GetFieldSchemas();
            if ((fieldSchemas != null) && (fieldSchemas.Length > 0))
            {
                this._selectedDataSourceNode = new DataSourceNode();
                this._availableFieldsTree.Nodes.Add(this._selectedDataSourceNode);
                this._selectedCheckBoxDataSourceNode = new BoolDataSourceNode();
                this._availableFieldsTree.Nodes.Add(this._selectedCheckBoxDataSourceNode);
            }
            HyperLinkNode node = new HyperLinkNode(this);
            this._availableFieldsTree.Nodes.Add(node);
            ImageNode node2 = new ImageNode(this);
            this._availableFieldsTree.Nodes.Add(node2);
            ButtonNode node3 = new ButtonNode(this);
            this._availableFieldsTree.Nodes.Add(node3);
            CommandNode node4 = new CommandNode(this);
            this._availableFieldsTree.Nodes.Add(node4);
            CommandNode node5 = new CommandNode(this, 0, System.Design.SR.GetString("DCFEditor_Node_Edit"), 6);
            node4.Nodes.Add(node5);
            if (this.Control is GridView)
            {
                CommandNode node6 = new CommandNode(this, 2, System.Design.SR.GetString("DCFEditor_Node_Select"), 5);
                node4.Nodes.Add(node6);
            }
            CommandNode node7 = new CommandNode(this, 3, System.Design.SR.GetString("DCFEditor_Node_Delete"), 7);
            node4.Nodes.Add(node7);
            if (this.Control is DetailsView)
            {
                CommandNode node8 = new CommandNode(this, 1, System.Design.SR.GetString("DCFEditor_Node_Insert"), 11);
                node4.Nodes.Add(node8);
            }
            TemplateNode node9 = new TemplateNode(this);
            this._availableFieldsTree.Nodes.Add(node9);
        }

        private void LoadComponent()
        {
            this.InitPage();
            this.LoadAvailableFieldsTree();
            this.LoadDataSourceFields();
            this.LoadCustomFields();
            this._autoFieldCheck.Checked = this.AutoGenerateFields;
            this.LoadFields();
            this.UpdateEnabledVisibleState();
        }

        private void LoadCustomFields()
        {
            if (this._customFieldDesigners == null)
            {
                this._customFieldDesigners = DataControlFieldHelper.GetCustomFieldDesigners(this, this.Control);
            }
            IDataSourceFieldSchema[] fieldSchemas = this.GetFieldSchemas();
            bool flag = (fieldSchemas != null) && (fieldSchemas.Length > 0);
            foreach (KeyValuePair<System.Type, DataControlFieldDesigner> pair in this._customFieldDesigners)
            {
                DataControlFieldDesigner fieldDesigner = pair.Value;
                if (fieldDesigner.UsesSchema && flag)
                {
                    DataSourceNode node = new DataSourceNode(pair.Key.Name);
                    this._availableFieldsTree.Nodes.Add(node);
                    foreach (IDataSourceFieldSchema schema in fieldSchemas)
                    {
                        node.Nodes.Add(new DataControlFieldDesignerNode(fieldDesigner, schema));
                    }
                    node.Expand();
                }
                else
                {
                    this._availableFieldsTree.Nodes.Add(new DataControlFieldDesignerNode(fieldDesigner));
                }
            }
        }

        private void LoadDataSourceFields()
        {
            this.EnterLoadingMode();
            IDataSourceFieldSchema[] fieldSchemas = this.GetFieldSchemas();
            if ((fieldSchemas != null) && (fieldSchemas.Length > 0))
            {
                DataFieldNode node = new DataFieldNode(this);
                this._availableFieldsTree.Nodes.Insert(0, node);
                foreach (IDataSourceFieldSchema schema in fieldSchemas)
                {
                    BoundNode node2 = new BoundNode(this, schema);
                    this._selectedDataSourceNode.Nodes.Add(node2);
                }
                this._selectedDataSourceNode.Expand();
                foreach (IDataSourceFieldSchema schema2 in fieldSchemas)
                {
                    if ((schema2.DataType == typeof(bool)) || (schema2.DataType == typeof(bool?)))
                    {
                        CheckBoxNode node3 = new CheckBoxNode(this, schema2);
                        this._selectedCheckBoxDataSourceNode.Nodes.Add(node3);
                    }
                }
                this._selectedCheckBoxDataSourceNode.Expand();
                this._availableFieldsTree.SelectedNode = node;
                node.EnsureVisible();
            }
            else
            {
                BoundNode node4 = new BoundNode(this, null);
                this._availableFieldsTree.Nodes.Insert(0, node4);
                node4.EnsureVisible();
                CheckBoxNode node5 = new CheckBoxNode(this, null);
                this._availableFieldsTree.Nodes.Insert(1, node5);
                node5.EnsureVisible();
                this._availableFieldsTree.SelectedNode = node4;
            }
            this.ExitLoadingMode();
        }

        private void LoadFields()
        {
            DataControlFieldCollection fieldCollection = this.FieldCollection;
            if (fieldCollection != null)
            {
                int count = fieldCollection.Count;
                IDataSourceViewSchema viewSchema = this.GetViewSchema();
                for (int i = 0; i < count; i++)
                {
                    DataControlField runtimeField = fieldCollection[i];
                    FieldItem item = null;
                    System.Type key = runtimeField.GetType();
                    if (key == typeof(CheckBoxField))
                    {
                        item = new CheckBoxFieldItem(this, (CheckBoxField) runtimeField);
                    }
                    else if (key == typeof(BoundField))
                    {
                        item = new BoundFieldItem(this, (BoundField) runtimeField);
                    }
                    else if (key == typeof(ButtonField))
                    {
                        item = new ButtonFieldItem(this, (ButtonField) runtimeField);
                    }
                    else if (key == typeof(HyperLinkField))
                    {
                        item = new HyperLinkFieldItem(this, (HyperLinkField) runtimeField);
                    }
                    else if (key == typeof(TemplateField))
                    {
                        item = new TemplateFieldItem(this, (TemplateField) runtimeField);
                    }
                    else if (key == typeof(CommandField))
                    {
                        item = new CommandFieldItem(this, (CommandField) runtimeField);
                    }
                    else if (key == typeof(ImageField))
                    {
                        item = new ImageFieldItem(this, (ImageField) runtimeField);
                    }
                    else if (this._customFieldDesigners.ContainsKey(key))
                    {
                        item = new DataControlFieldDesignerItem(this._customFieldDesigners[key], runtimeField);
                    }
                    else
                    {
                        item = new CustomFieldItem(this, runtimeField);
                    }
                    item.LoadFieldInfo();
                    IDataSourceViewSchemaAccessor accessor = item.RuntimeField;
                    if (accessor != null)
                    {
                        accessor.DataSourceViewSchema = viewSchema;
                    }
                    this._selFieldsList.Items.Add(item);
                }
                if (this._selFieldsList.Items.Count != 0)
                {
                    this._currentFieldItem = (FieldItem) this._selFieldsList.Items[0];
                    this._currentFieldItem.Selected = true;
                }
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (this._initialActivate)
            {
                this.LoadComponent();
                this._initialActivate = false;
            }
        }

        private void OnAvailableFieldsDoubleClick(object source, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.OnClickAddField(source, e);
            }
        }

        private void OnAvailableFieldsGotFocus(object source, EventArgs e)
        {
            this._currentFieldProps.SelectedObject = null;
        }

        private void OnAvailableFieldsKeyPress(object source, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                this.OnClickAddField(source, e);
                e.Handled = true;
            }
        }

        private void OnChangedPropertyValues(object source, PropertyValueChangedEventArgs e)
        {
            if (!this._isLoading && ((e.ChangedItem.Label == "HeaderText") || (e.ChangedItem.PropertyDescriptor.ComponentType == typeof(CommandField))))
            {
                this._propChangesPending = true;
                this.SaveFieldProperties();
                if (this._selFieldsList.SelectedItems.Count == 0)
                {
                    this._currentFieldItem = null;
                }
                else
                {
                    this._currentFieldItem = (FieldItem) this._selFieldsList.SelectedItems[0];
                    CommandFieldItem item = this._currentFieldItem as CommandFieldItem;
                    if (item != null)
                    {
                        item.UpdateImageIndex();
                    }
                }
            }
        }

        private void OnCheckChangedAutoField(object source, EventArgs e)
        {
            if (!this._isLoading)
            {
                this.UpdateEnabledVisibleState();
            }
        }

        private void OnClickAddField(object source, EventArgs e)
        {
            AvailableFieldNode selectedNode = (AvailableFieldNode) this._availableFieldsTree.SelectedNode;
            if (this._addFieldButton.Enabled)
            {
                if (this._propChangesPending)
                {
                    this.SaveFieldProperties();
                }
                if (!selectedNode.CreatesMultipleFields)
                {
                    FieldItem item = selectedNode.CreateField();
                    this._selFieldsList.Items.Add(item);
                    this._currentFieldItem = item;
                    this._currentFieldItem.Selected = true;
                    this._currentFieldItem.EnsureVisible();
                }
                else
                {
                    IDataSourceFieldSchema[] fieldSchemas = this.GetFieldSchemas();
                    FieldItem[] itemArray = selectedNode.CreateFields(this.Control, fieldSchemas);
                    int length = itemArray.Length;
                    for (int i = 0; i < length; i++)
                    {
                        this._selFieldsList.Items.Add(itemArray[i]);
                    }
                    this._currentFieldItem = itemArray[length - 1];
                    this._currentFieldItem.Selected = true;
                    this._currentFieldItem.EnsureVisible();
                }
                IDataSourceViewSchemaAccessor runtimeField = this._currentFieldItem.RuntimeField;
                if (runtimeField != null)
                {
                    runtimeField.DataSourceViewSchema = this.GetViewSchema();
                }
                this._selFieldsList.Focus();
                this._selFieldsList.FocusedItem = this._currentFieldItem;
                this.UpdateEnabledVisibleState();
            }
        }

        private void OnClickDeleteField(object source, EventArgs e)
        {
            int index = this._currentFieldItem.Index;
            int num2 = -1;
            int count = this._selFieldsList.Items.Count;
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
            this._propChangesPending = false;
            this._currentFieldItem.Remove();
            this._currentFieldItem = null;
            if (num2 != -1)
            {
                this._currentFieldItem = (FieldItem) this._selFieldsList.Items[num2];
                this._currentFieldItem.Selected = true;
                this._currentFieldItem.EnsureVisible();
                this._deleteFieldButton.Focus();
            }
            this.UpdateEnabledVisibleState();
        }

        private void OnClickMoveFieldDown(object source, EventArgs e)
        {
            this._fieldMovePending = true;
            int index = this._currentFieldItem.Index;
            ListViewItem item = this._selFieldsList.Items[index];
            this._selFieldsList.Items.RemoveAt(index);
            this._selFieldsList.Items.Insert(index + 1, item);
            this._currentFieldItem = (FieldItem) this._selFieldsList.Items[index + 1];
            this._currentFieldItem.Selected = true;
            this._currentFieldItem.EnsureVisible();
            this.UpdateFieldPositionButtonsState();
            if (this._moveFieldUpButton.Enabled && !this._moveFieldDownButton.Enabled)
            {
                this._moveFieldUpButton.Focus();
            }
            this._fieldMovePending = false;
        }

        private void OnClickMoveFieldUp(object source, EventArgs e)
        {
            this._fieldMovePending = true;
            int index = this._currentFieldItem.Index;
            ListViewItem item = this._selFieldsList.Items[index];
            this._selFieldsList.Items.RemoveAt(index);
            this._selFieldsList.Items.Insert(index - 1, item);
            this._currentFieldItem = (FieldItem) this._selFieldsList.Items[index - 1];
            this._currentFieldItem.Selected = true;
            this._currentFieldItem.EnsureVisible();
            this.UpdateFieldPositionButtonsState();
            this._fieldMovePending = false;
        }

        private void OnClickOK(object source, EventArgs e)
        {
            this.SaveComponent();
            this.PersistClonedFieldsToControl();
        }

        private void OnClickRefreshSchema(object source, LinkLabelLinkClickedEventArgs e)
        {
            this._fieldSchemas = null;
            this._viewSchema = null;
            IDataSourceDesigner dataSourceDesigner = this._controlDesigner.DataSourceDesigner;
            if ((dataSourceDesigner != null) && dataSourceDesigner.CanRefreshSchema)
            {
                dataSourceDesigner.RefreshSchema(false);
            }
            IDataSourceViewSchema viewSchema = this.GetViewSchema();
            foreach (FieldItem item in this._selFieldsList.Items)
            {
                IDataSourceViewSchemaAccessor runtimeField = item.RuntimeField;
                if (runtimeField != null)
                {
                    runtimeField.DataSourceViewSchema = viewSchema;
                }
            }
            this._availableFieldsTree.Nodes.Clear();
            this.LoadAvailableFieldsTree();
            this.LoadDataSourceFields();
        }

        private void OnClickTemplatize(object source, LinkLabelLinkClickedEventArgs e)
        {
            if (this._propChangesPending)
            {
                this.SaveFieldProperties();
            }
            TemplateField templateField = this._currentFieldItem.GetTemplateField(this.Control);
            TemplateFieldItem item = new TemplateFieldItem(this, templateField);
            item.LoadFieldInfo();
            this._selFieldsList.Items[this._currentFieldItem.Index] = item;
            this._currentFieldItem = item;
            this._currentFieldItem.Selected = true;
            this.UpdateEnabledVisibleState();
        }

        protected override void OnClosed(EventArgs e)
        {
            IDataSourceDesigner dataSourceDesigner = this._controlDesigner.DataSourceDesigner;
            if (dataSourceDesigner != null)
            {
                dataSourceDesigner.ResumeDataSourceEvents();
            }
            this.IgnoreRefreshSchema = this._initialIgnoreRefreshSchemaValue;
        }

        private void OnSelChangedAvailableFields(object source, TreeViewEventArgs e)
        {
            this.UpdateEnabledVisibleState();
        }

        private void OnSelFieldsListGotFocus(object source, EventArgs e)
        {
            this.UpdateEnabledVisibleState();
        }

        private void OnSelFieldsListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                if (this._currentFieldItem != null)
                {
                    this.OnClickDeleteField(sender, e);
                }
                e.Handled = true;
            }
        }

        private void OnSelIndexChangedSelFieldsList(object source, EventArgs e)
        {
            if (!this._fieldMovePending)
            {
                if (this._propChangesPending)
                {
                    this.SaveFieldProperties();
                }
                if (this._selFieldsList.SelectedItems.Count == 0)
                {
                    this._currentFieldItem = null;
                }
                else
                {
                    this._currentFieldItem = (FieldItem) this._selFieldsList.SelectedItems[0];
                }
                this.SetFieldPropertyHeader();
                this.UpdateEnabledVisibleState();
            }
        }

        private void PersistClonedFieldsToControl()
        {
            DataControlFieldCollection columns = null;
            if (this.Control is GridView)
            {
                columns = ((GridView) this.Control).Columns;
            }
            else if (this.Control is DetailsView)
            {
                columns = ((DetailsView) this.Control).Fields;
            }
            if (columns != null)
            {
                columns.Clear();
                foreach (DataControlField field in this.FieldCollection)
                {
                    columns.Add(field);
                }
            }
        }

        private void SaveComponent()
        {
            if (this._propChangesPending)
            {
                this.SaveFieldProperties();
            }
            this.AutoGenerateFields = this._autoFieldCheck.Checked;
            DataControlFieldCollection fieldCollection = this.FieldCollection;
            if (fieldCollection != null)
            {
                fieldCollection.Clear();
                int count = this._selFieldsList.Items.Count;
                for (int i = 0; i < count; i++)
                {
                    FieldItem item = (FieldItem) this._selFieldsList.Items[i];
                    fieldCollection.Add(item.RuntimeField);
                }
            }
        }

        private void SaveFieldProperties()
        {
            if (this._currentFieldItem != null)
            {
                this._currentFieldItem.HeaderText = this._currentFieldItem.RuntimeField.HeaderText;
                if (this._currentFieldProps.Visible)
                {
                    this._currentFieldProps.Refresh();
                }
            }
            this._propChangesPending = false;
        }

        private void SetFieldPropertyHeader()
        {
            string str = System.Design.SR.GetString("DCFEditor_FieldProps");
            if (this._currentFieldItem != null)
            {
                this.EnterLoadingMode();
                System.Type type = this._currentFieldItem.GetType();
                if (type == typeof(CheckBoxFieldItem))
                {
                    str = System.Design.SR.GetString("DCFEditor_FieldPropsFormat", new object[] { System.Design.SR.GetString("DCFEditor_Node_CheckBox") });
                }
                else if (type == typeof(BoundFieldItem))
                {
                    str = System.Design.SR.GetString("DCFEditor_FieldPropsFormat", new object[] { System.Design.SR.GetString("DCFEditor_Node_Bound") });
                }
                else if (type == typeof(ButtonFieldItem))
                {
                    str = System.Design.SR.GetString("DCFEditor_FieldPropsFormat", new object[] { System.Design.SR.GetString("DCFEditor_Node_Button") });
                }
                else if (type == typeof(HyperLinkFieldItem))
                {
                    str = System.Design.SR.GetString("DCFEditor_FieldPropsFormat", new object[] { System.Design.SR.GetString("DCFEditor_Node_HyperLink") });
                }
                else if (type == typeof(CommandFieldItem))
                {
                    str = System.Design.SR.GetString("DCFEditor_FieldPropsFormat", new object[] { System.Design.SR.GetString("DCFEditor_Node_Command") });
                }
                else if (type == typeof(TemplateFieldItem))
                {
                    str = System.Design.SR.GetString("DCFEditor_FieldPropsFormat", new object[] { System.Design.SR.GetString("DCFEditor_Node_Template") });
                }
                else if (type == typeof(ImageFieldItem))
                {
                    str = System.Design.SR.GetString("DCFEditor_FieldPropsFormat", new object[] { System.Design.SR.GetString("DCFEditor_Node_Image") });
                }
                this.ExitLoadingMode();
            }
            this._selFieldLabel.Text = str;
        }

        private void UpdateEnabledVisibleState()
        {
            AvailableFieldNode selectedNode = (AvailableFieldNode) this._availableFieldsTree.SelectedNode;
            int count = this._selFieldsList.Items.Count;
            int num2 = this._selFieldsList.SelectedItems.Count;
            FieldItem item = null;
            int index = -1;
            if (num2 != 0)
            {
                item = (FieldItem) this._selFieldsList.SelectedItems[0];
            }
            if (item != null)
            {
                index = item.Index;
            }
            bool flag = index != -1;
            this._addFieldButton.Enabled = (selectedNode != null) && selectedNode.IsFieldCreator;
            this._deleteFieldButton.Enabled = flag;
            this.UpdateFieldPositionButtonsState();
            this._currentFieldProps.Enabled = item != null;
            this._currentFieldProps.SelectedObject = ((item != null) && this._selFieldsList.Focused) ? item.RuntimeField : null;
            System.Type key = (item == null) ? null : item.RuntimeField.GetType();
            this._templatizeLink.Visible = ((count != 0) && (item != null)) && (((((key == typeof(BoundField)) || (key == typeof(CheckBoxField))) || ((key == typeof(ButtonField)) || (key == typeof(HyperLinkField)))) || ((key == typeof(CommandField)) || (key == typeof(ImageField)))) || this._customFieldDesigners.ContainsKey(key));
        }

        private void UpdateFieldPositionButtonsState()
        {
            int index = -1;
            int count = this._selFieldsList.SelectedItems.Count;
            FieldItem item = null;
            if (count > 0)
            {
                item = this._selFieldsList.SelectedItems[0] as FieldItem;
            }
            if (item != null)
            {
                index = item.Index;
            }
            this._moveFieldUpButton.Enabled = index > 0;
            this._moveFieldDownButton.Enabled = (index >= 0) && (index < (this._selFieldsList.Items.Count - 1));
        }

        private bool AutoGenerateFields
        {
            get
            {
                if (this.Control is GridView)
                {
                    return ((GridView) this.Control).AutoGenerateColumns;
                }
                return ((this.Control is DetailsView) && ((DetailsView) this.Control).AutoGenerateRows);
            }
            set
            {
                if (this.Control is GridView)
                {
                    ((GridView) this.Control).AutoGenerateColumns = value;
                }
                else if (this.Control is DetailsView)
                {
                    ((DetailsView) this.Control).AutoGenerateRows = value;
                }
            }
        }

        private DataBoundControl Control
        {
            get
            {
                return (this._controlDesigner.Component as DataBoundControl);
            }
        }

        private DataControlFieldCollection FieldCollection
        {
            get
            {
                if (this._clonedFieldCollection == null)
                {
                    if (this.Control is GridView)
                    {
                        DataControlFieldCollection columns = ((GridView) this.Control).Columns;
                        this._clonedFieldCollection = columns.CloneFields();
                        for (int i = 0; i < columns.Count; i++)
                        {
                            this._controlDesigner.RegisterClone(columns[i], this._clonedFieldCollection[i]);
                        }
                    }
                    else if (this.Control is DetailsView)
                    {
                        DataControlFieldCollection fields = ((DetailsView) this.Control).Fields;
                        this._clonedFieldCollection = fields.CloneFields();
                        for (int j = 0; j < fields.Count; j++)
                        {
                            this._controlDesigner.RegisterClone(fields[j], this._clonedFieldCollection[j]);
                        }
                    }
                }
                return this._clonedFieldCollection;
            }
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.DataControlField.DataControlFieldEditor";
            }
        }

        private bool IgnoreRefreshSchema
        {
            get
            {
                if (this._controlDesigner is GridViewDesigner)
                {
                    return ((GridViewDesigner) this._controlDesigner)._ignoreSchemaRefreshedEvent;
                }
                return ((this._controlDesigner is DetailsViewDesigner) && ((DetailsViewDesigner) this._controlDesigner)._ignoreSchemaRefreshedEvent);
            }
            set
            {
                if (this._controlDesigner is GridViewDesigner)
                {
                    ((GridViewDesigner) this._controlDesigner)._ignoreSchemaRefreshedEvent = value;
                }
                if (this._controlDesigner is DetailsViewDesigner)
                {
                    ((DetailsViewDesigner) this._controlDesigner)._ignoreSchemaRefreshedEvent = value;
                }
            }
        }

        private abstract class AvailableFieldNode : System.Windows.Forms.TreeNode
        {
            public AvailableFieldNode(string text, int icon) : base(text, icon, icon)
            {
            }

            public virtual DataControlFieldsEditor.FieldItem CreateField()
            {
                return null;
            }

            public virtual DataControlFieldsEditor.FieldItem[] CreateFields(DataBoundControl control, IDataSourceFieldSchema[] fieldSchemas)
            {
                return null;
            }

            public virtual bool CreatesMultipleFields
            {
                get
                {
                    return false;
                }
            }

            public virtual bool IsFieldCreator
            {
                get
                {
                    return true;
                }
            }
        }

        private class BoolDataSourceNode : DataControlFieldsEditor.AvailableFieldNode
        {
            public BoolDataSourceNode() : base(System.Design.SR.GetString("DCFEditor_Node_CheckBox"), 13)
            {
            }

            public override bool IsFieldCreator
            {
                get
                {
                    return false;
                }
            }
        }

        private class BoundFieldItem : DataControlFieldsEditor.FieldItem
        {
            public BoundFieldItem(DataControlFieldsEditor fieldsEditor, BoundField runtimeField) : base(fieldsEditor, runtimeField, 1)
            {
            }

            protected override string GetDefaultNodeText()
            {
                string dataField = ((BoundField) base.RuntimeField).DataField;
                if ((dataField != null) && (dataField.Length != 0))
                {
                    return dataField;
                }
                return System.Design.SR.GetString("DCFEditor_Node_Bound");
            }

            private string GetTemplateContent(int editMode, bool readOnly)
            {
                StringBuilder builder = new StringBuilder();
                bool flag = (editMode == 1) && readOnly;
                System.Type controlType = ((editMode == 0) || flag) ? typeof(System.Web.UI.WebControls.Label) : typeof(System.Web.UI.WebControls.TextBox);
                string dataFormatString = ((BoundField) base.RuntimeField).DataFormatString;
                string dataField = ((BoundField) base.RuntimeField).DataField;
                string format = string.Empty;
                if (((editMode != 1) || ((BoundField) base.RuntimeField).ApplyFormatInEditMode) || flag)
                {
                    format = base.PrepareFormatString(dataFormatString);
                }
                string str4 = flag ? DesignTimeDataBinding.CreateEvalExpression(dataField, format) : DesignTimeDataBinding.CreateBindExpression(dataField, format);
                if ((editMode == 2) && !((BoundField) base.RuntimeField).InsertVisible)
                {
                    return string.Empty;
                }
                builder.Append("<asp:");
                builder.Append(controlType.Name);
                builder.Append(" runat=\"server\"");
                if (dataField.Length != 0)
                {
                    builder.Append(" Text='<%# ");
                    builder.Append(str4);
                    builder.Append(" %>'");
                }
                builder.Append(" id=\"");
                builder.Append(base.fieldsEditor.GetNewDataSourceName(controlType, editMode));
                builder.Append("\"></asp:");
                builder.Append(controlType.Name);
                builder.Append(">");
                return builder.ToString();
            }

            public override TemplateField GetTemplateField(DataBoundControl dataBoundControl)
            {
                TemplateField templateField = base.GetTemplateField(dataBoundControl);
                templateField.SortExpression = base.RuntimeField.SortExpression;
                templateField.ItemTemplate = base.GetTemplate(dataBoundControl, this.GetTemplateContent(0, false));
                templateField.ConvertEmptyStringToNull = ((BoundField) base.RuntimeField).ConvertEmptyStringToNull;
                templateField.EditItemTemplate = base.GetTemplate(dataBoundControl, this.GetTemplateContent(1, ((BoundField) base.RuntimeField).ReadOnly));
                if ((dataBoundControl is DetailsView) && ((BoundField) base.RuntimeField).InsertVisible)
                {
                    templateField.InsertItemTemplate = base.GetTemplate(dataBoundControl, this.GetTemplateContent(2, false));
                }
                return templateField;
            }
        }

        private class BoundNode : DataControlFieldsEditor.AvailableFieldNode
        {
            protected IDataSourceFieldSchema _fieldSchema;
            private DataControlFieldsEditor _fieldsEditor;
            private bool _genericBoundField;

            public BoundNode(DataControlFieldsEditor fieldsEditor, IDataSourceFieldSchema fieldSchema) : base((fieldSchema == null) ? string.Empty : fieldSchema.Name, 1)
            {
                this._fieldSchema = fieldSchema;
                this._fieldsEditor = fieldsEditor;
                if (fieldSchema == null)
                {
                    this._genericBoundField = true;
                    base.Text = System.Design.SR.GetString("DCFEditor_Node_Bound");
                }
            }

            public override DataControlFieldsEditor.FieldItem CreateField()
            {
                BoundField runtimeField = new BoundField();
                string name = string.Empty;
                if (this._fieldSchema != null)
                {
                    name = this._fieldSchema.Name;
                }
                if (!this._genericBoundField)
                {
                    runtimeField.HeaderText = name;
                    runtimeField.DataField = name;
                    runtimeField.SortExpression = name;
                }
                if (this._fieldSchema != null)
                {
                    if (this._fieldSchema.PrimaryKey)
                    {
                        runtimeField.ReadOnly = true;
                    }
                    if (this._fieldSchema.Identity)
                    {
                        runtimeField.InsertVisible = false;
                    }
                }
                DataControlFieldsEditor.FieldItem item = new DataControlFieldsEditor.BoundFieldItem(this._fieldsEditor, runtimeField);
                item.LoadFieldInfo();
                return item;
            }
        }

        private class ButtonFieldItem : DataControlFieldsEditor.FieldItem
        {
            public ButtonFieldItem(DataControlFieldsEditor fieldsEditor, ButtonField runtimeField) : base(fieldsEditor, runtimeField, 4)
            {
            }

            protected override string GetDefaultNodeText()
            {
                string text = ((ButtonField) base.runtimeField).Text;
                if ((text != null) && (text.Length != 0))
                {
                    return text;
                }
                return System.Design.SR.GetString("DCFEditor_Node_Button");
            }

            public override TemplateField GetTemplateField(DataBoundControl dataBoundControl)
            {
                TemplateField templateField = base.GetTemplateField(dataBoundControl);
                ButtonField runtimeField = (ButtonField) base.RuntimeField;
                StringBuilder builder = new StringBuilder();
                System.Type controlType = typeof(System.Web.UI.WebControls.Button);
                if (runtimeField.ButtonType == ButtonType.Link)
                {
                    controlType = typeof(LinkButton);
                }
                else if (runtimeField.ButtonType == ButtonType.Image)
                {
                    controlType = typeof(ImageButton);
                }
                builder.Append("<asp:");
                builder.Append(controlType.Name);
                builder.Append(" runat=\"server\"");
                if (runtimeField.DataTextField.Length != 0)
                {
                    builder.Append(" Text='<%# ");
                    builder.Append(DesignTimeDataBinding.CreateEvalExpression(runtimeField.DataTextField, base.PrepareFormatString(runtimeField.DataTextFormatString)));
                    builder.Append(" %>'");
                }
                else
                {
                    builder.Append(" Text=\"");
                    builder.Append(runtimeField.Text);
                    builder.Append("\"");
                }
                builder.Append(" CommandName=\"");
                builder.Append(runtimeField.CommandName);
                builder.Append("\"");
                if ((runtimeField.ButtonType == ButtonType.Image) && (runtimeField.ImageUrl.Length > 0))
                {
                    builder.Append(" ImageUrl=\"");
                    builder.Append(runtimeField.ImageUrl);
                    builder.Append("\"");
                }
                builder.Append(" CausesValidation=\"false\" id=\"");
                builder.Append(base.fieldsEditor.GetNewDataSourceName(controlType, 0));
                builder.Append("\"></asp:");
                builder.Append(controlType.Name);
                builder.Append(">");
                templateField.ItemTemplate = base.GetTemplate(dataBoundControl, builder.ToString());
                return templateField;
            }
        }

        private class ButtonNode : DataControlFieldsEditor.AvailableFieldNode
        {
            private DataControlFieldsEditor _fieldsEditor;
            private string buttonText;
            private string command;

            public ButtonNode(DataControlFieldsEditor fieldsEditor) : this(fieldsEditor, string.Empty, System.Design.SR.GetString("DCFEditor_Button"), System.Design.SR.GetString("DCFEditor_Node_Button"))
            {
            }

            public ButtonNode(DataControlFieldsEditor fieldsEditor, string command, string buttonText, string text) : base(text, 4)
            {
                this._fieldsEditor = fieldsEditor;
                this.command = command;
                this.buttonText = buttonText;
            }

            public override DataControlFieldsEditor.FieldItem CreateField()
            {
                ButtonField runtimeField = new ButtonField {
                    Text = this.buttonText,
                    CommandName = this.command
                };
                DataControlFieldsEditor.FieldItem item = new DataControlFieldsEditor.ButtonFieldItem(this._fieldsEditor, runtimeField);
                item.LoadFieldInfo();
                return item;
            }
        }

        private class CheckBoxFieldItem : DataControlFieldsEditor.FieldItem
        {
            public CheckBoxFieldItem(DataControlFieldsEditor fieldsEditor, CheckBoxField runtimeField) : base(fieldsEditor, runtimeField, 10)
            {
            }

            protected override string GetDefaultNodeText()
            {
                string dataField = ((CheckBoxField) base.RuntimeField).DataField;
                if ((dataField != null) && (dataField.Length != 0))
                {
                    return dataField;
                }
                return System.Design.SR.GetString("DCFEditor_Node_CheckBox");
            }

            private string GetTemplateContent(int editMode)
            {
                StringBuilder builder = new StringBuilder();
                System.Type controlType = typeof(System.Web.UI.WebControls.CheckBox);
                if ((editMode == 2) && !((CheckBoxField) base.RuntimeField).InsertVisible)
                {
                    return string.Empty;
                }
                builder.Append("<asp:");
                builder.Append(controlType.Name);
                builder.Append(" runat=\"server\"");
                string dataField = ((CheckBoxField) base.RuntimeField).DataField;
                if (dataField.Length != 0)
                {
                    builder.Append(" Checked='<%# ");
                    builder.Append(DesignTimeDataBinding.CreateBindExpression(dataField, string.Empty));
                    builder.Append(" %>'");
                    if (editMode == 0)
                    {
                        builder.Append(" Enabled=\"false\"");
                    }
                }
                builder.Append(" id=\"");
                builder.Append(base.fieldsEditor.GetNewDataSourceName(controlType, editMode));
                builder.Append("\"></asp:");
                builder.Append(controlType.Name);
                builder.Append(">");
                return builder.ToString();
            }

            public override TemplateField GetTemplateField(DataBoundControl dataBoundControl)
            {
                TemplateField templateField = base.GetTemplateField(dataBoundControl);
                CheckBoxField runtimeField = (CheckBoxField) base.RuntimeField;
                templateField.SortExpression = runtimeField.SortExpression;
                templateField.ItemTemplate = base.GetTemplate(dataBoundControl, this.GetTemplateContent(0));
                if (!runtimeField.ReadOnly)
                {
                    templateField.EditItemTemplate = base.GetTemplate(dataBoundControl, this.GetTemplateContent(1));
                }
                if ((dataBoundControl is DetailsView) && ((CheckBoxField) base.RuntimeField).InsertVisible)
                {
                    templateField.InsertItemTemplate = base.GetTemplate(dataBoundControl, this.GetTemplateContent(2));
                }
                return templateField;
            }
        }

        private class CheckBoxNode : DataControlFieldsEditor.AvailableFieldNode
        {
            protected IDataSourceFieldSchema _fieldSchema;
            private DataControlFieldsEditor _fieldsEditor;
            private bool _genericCheckBoxField;

            public CheckBoxNode(DataControlFieldsEditor fieldsEditor, IDataSourceFieldSchema fieldSchema) : base((fieldSchema == null) ? string.Empty : fieldSchema.Name, 10)
            {
                this._fieldsEditor = fieldsEditor;
                this._fieldSchema = fieldSchema;
                if (fieldSchema == null)
                {
                    this._genericCheckBoxField = true;
                    base.Text = System.Design.SR.GetString("DCFEditor_Node_CheckBox");
                }
            }

            public override DataControlFieldsEditor.FieldItem CreateField()
            {
                CheckBoxField runtimeField = new CheckBoxField();
                string name = string.Empty;
                if (this._fieldSchema != null)
                {
                    name = this._fieldSchema.Name;
                }
                if (!this._genericCheckBoxField)
                {
                    runtimeField.HeaderText = name;
                    runtimeField.DataField = name;
                    runtimeField.SortExpression = name;
                }
                if (this._fieldSchema != null)
                {
                    if (this._fieldSchema.PrimaryKey)
                    {
                        runtimeField.ReadOnly = true;
                    }
                    if (this._fieldSchema.Identity)
                    {
                        runtimeField.InsertVisible = false;
                    }
                }
                DataControlFieldsEditor.FieldItem item = new DataControlFieldsEditor.CheckBoxFieldItem(this._fieldsEditor, runtimeField);
                item.LoadFieldInfo();
                return item;
            }
        }

        private class CommandFieldItem : DataControlFieldsEditor.FieldItem
        {
            public CommandFieldItem(DataControlFieldsEditor fieldsEditor, CommandField runtimeField) : base(fieldsEditor, runtimeField, 12)
            {
                this.UpdateImageIndex();
            }

            private string BuildButtonString(System.Type controlType, string buttonText, string commandName, string imageUrl, bool causesValidation, int mode, ref int buttonNameStartIndex)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("<asp:");
                builder.Append(controlType.Name);
                builder.Append(" runat=\"server\"");
                builder.Append(" Text=\"");
                builder.Append(buttonText);
                builder.Append("\"");
                builder.Append(" CommandName=\"");
                builder.Append(commandName);
                if ((imageUrl != null) && (imageUrl.Length > 0))
                {
                    builder.Append("\" ImageUrl=\"");
                    builder.Append(imageUrl);
                }
                builder.Append("\" CausesValidation=\"");
                builder.Append(causesValidation.ToString());
                builder.Append("\" id=\"");
                builder.Append(base.fieldsEditor.GetNewDataSourceName(controlType, mode, ref buttonNameStartIndex));
                builder.Append("\"></asp:");
                builder.Append(controlType.Name);
                builder.Append(">");
                return builder.ToString();
            }

            protected override string GetDefaultNodeText()
            {
                CommandField runtimeField = (CommandField) base.RuntimeField;
                if ((runtimeField.ShowEditButton && !runtimeField.ShowDeleteButton) && (!runtimeField.ShowSelectButton && !runtimeField.ShowInsertButton))
                {
                    return System.Design.SR.GetString("DCFEditor_Node_Edit");
                }
                if ((runtimeField.ShowDeleteButton && !runtimeField.ShowEditButton) && (!runtimeField.ShowSelectButton && !runtimeField.ShowInsertButton))
                {
                    return System.Design.SR.GetString("DCFEditor_Node_Delete");
                }
                if ((runtimeField.ShowSelectButton && !runtimeField.ShowDeleteButton) && (!runtimeField.ShowEditButton && !runtimeField.ShowInsertButton))
                {
                    return System.Design.SR.GetString("DCFEditor_Node_Select");
                }
                if ((runtimeField.ShowInsertButton && !runtimeField.ShowDeleteButton) && (!runtimeField.ShowSelectButton && !runtimeField.ShowEditButton))
                {
                    return System.Design.SR.GetString("DCFEditor_Node_Insert");
                }
                return System.Design.SR.GetString("DCFEditor_Node_Command");
            }

            private string GetTemplateContent(int editMode)
            {
                StringBuilder builder = new StringBuilder();
                CommandField runtimeField = (CommandField) base.RuntimeField;
                System.Type controlType = typeof(System.Web.UI.WebControls.Button);
                int buttonNameStartIndex = 1;
                if (runtimeField.ButtonType == ButtonType.Link)
                {
                    controlType = typeof(LinkButton);
                }
                else if (runtimeField.ButtonType == ButtonType.Image)
                {
                    controlType = typeof(ImageButton);
                }
                switch (editMode)
                {
                    case 0:
                    {
                        bool flag = true;
                        if (runtimeField.ShowEditButton)
                        {
                            string imageUrl = (runtimeField.ButtonType == ButtonType.Image) ? runtimeField.EditImageUrl : null;
                            builder.Append(this.BuildButtonString(controlType, runtimeField.EditText, "Edit", imageUrl, false, 0, ref buttonNameStartIndex));
                            buttonNameStartIndex++;
                            flag = false;
                        }
                        if (runtimeField.ShowInsertButton)
                        {
                            if (!flag)
                            {
                                builder.Append("&nbsp;");
                            }
                            string str4 = (runtimeField.ButtonType == ButtonType.Image) ? runtimeField.NewImageUrl : null;
                            builder.Append(this.BuildButtonString(controlType, runtimeField.NewText, "New", str4, false, 0, ref buttonNameStartIndex));
                            buttonNameStartIndex++;
                        }
                        if (runtimeField.ShowSelectButton)
                        {
                            if (!flag)
                            {
                                builder.Append("&nbsp;");
                            }
                            string str5 = (runtimeField.ButtonType == ButtonType.Image) ? runtimeField.SelectImageUrl : null;
                            builder.Append(this.BuildButtonString(controlType, runtimeField.SelectText, "Select", str5, false, 0, ref buttonNameStartIndex));
                            buttonNameStartIndex++;
                        }
                        if (runtimeField.ShowDeleteButton)
                        {
                            if (!flag)
                            {
                                builder.Append("&nbsp;");
                            }
                            string str6 = (runtimeField.ButtonType == ButtonType.Image) ? runtimeField.DeleteImageUrl : null;
                            builder.Append(this.BuildButtonString(controlType, runtimeField.DeleteText, "Delete", str6, false, 0, ref buttonNameStartIndex));
                            buttonNameStartIndex++;
                        }
                        break;
                    }
                    case 1:
                        if (runtimeField.ShowEditButton)
                        {
                            string str = (runtimeField.ButtonType == ButtonType.Image) ? runtimeField.UpdateImageUrl : null;
                            builder.Append(this.BuildButtonString(controlType, runtimeField.UpdateText, "Update", str, true, 1, ref buttonNameStartIndex));
                            buttonNameStartIndex++;
                            if (runtimeField.ShowCancelButton)
                            {
                                builder.Append("&nbsp;");
                                string str2 = (runtimeField.ButtonType == ButtonType.Image) ? runtimeField.CancelImageUrl : null;
                                builder.Append(this.BuildButtonString(controlType, runtimeField.CancelText, "Cancel", str2, false, 1, ref buttonNameStartIndex));
                                buttonNameStartIndex++;
                            }
                        }
                        break;

                    case 2:
                        if (runtimeField.ShowInsertButton)
                        {
                            string str7 = (runtimeField.ButtonType == ButtonType.Image) ? runtimeField.InsertImageUrl : null;
                            builder.Append(this.BuildButtonString(controlType, runtimeField.InsertText, "Insert", str7, true, 2, ref buttonNameStartIndex));
                            buttonNameStartIndex++;
                            if (runtimeField.ShowCancelButton)
                            {
                                builder.Append("&nbsp;");
                                string str8 = (runtimeField.ButtonType == ButtonType.Image) ? runtimeField.CancelImageUrl : null;
                                builder.Append(this.BuildButtonString(controlType, runtimeField.CancelText, "Cancel", str8, false, 2, ref buttonNameStartIndex));
                                buttonNameStartIndex++;
                            }
                        }
                        break;
                }
                return builder.ToString();
            }

            public override TemplateField GetTemplateField(DataBoundControl dataBoundControl)
            {
                TemplateField templateField = base.GetTemplateField(dataBoundControl);
                templateField.ItemTemplate = base.GetTemplate(dataBoundControl, this.GetTemplateContent(0));
                templateField.EditItemTemplate = base.GetTemplate(dataBoundControl, this.GetTemplateContent(1));
                if (dataBoundControl is DetailsView)
                {
                    templateField.InsertItemTemplate = base.GetTemplate(dataBoundControl, this.GetTemplateContent(2));
                }
                return templateField;
            }

            public void UpdateImageIndex()
            {
                CommandField runtimeField = (CommandField) base.RuntimeField;
                if ((runtimeField.ShowEditButton && !runtimeField.ShowDeleteButton) && (!runtimeField.ShowSelectButton && !runtimeField.ShowInsertButton))
                {
                    base.ImageIndex = 6;
                }
                else if ((runtimeField.ShowDeleteButton && !runtimeField.ShowEditButton) && (!runtimeField.ShowSelectButton && !runtimeField.ShowInsertButton))
                {
                    base.ImageIndex = 7;
                }
                else if ((runtimeField.ShowSelectButton && !runtimeField.ShowDeleteButton) && (!runtimeField.ShowEditButton && !runtimeField.ShowInsertButton))
                {
                    base.ImageIndex = 5;
                }
                else if ((runtimeField.ShowInsertButton && !runtimeField.ShowDeleteButton) && (!runtimeField.ShowSelectButton && !runtimeField.ShowEditButton))
                {
                    base.ImageIndex = 11;
                }
                else
                {
                    base.ImageIndex = 12;
                }
            }
        }

        private class CommandNode : DataControlFieldsEditor.AvailableFieldNode
        {
            private DataControlFieldsEditor _fieldsEditor;
            private int commandType;

            public CommandNode(DataControlFieldsEditor fieldsEditor) : this(fieldsEditor, -1, System.Design.SR.GetString("DCFEditor_Node_Command"), 12)
            {
            }

            public CommandNode(DataControlFieldsEditor fieldsEditor, int commandType, string text, int icon) : base(text, icon)
            {
                this.commandType = commandType;
                this._fieldsEditor = fieldsEditor;
            }

            public override DataControlFieldsEditor.FieldItem CreateField()
            {
                CommandField runtimeField = new CommandField();
                switch (this.commandType)
                {
                    case 0:
                        runtimeField.ShowEditButton = true;
                        break;

                    case 1:
                        runtimeField.ShowInsertButton = true;
                        break;

                    case 2:
                        runtimeField.ShowSelectButton = true;
                        break;

                    case 3:
                        runtimeField.ShowDeleteButton = true;
                        break;
                }
                DataControlFieldsEditor.FieldItem item = new DataControlFieldsEditor.CommandFieldItem(this._fieldsEditor, runtimeField);
                item.LoadFieldInfo();
                return item;
            }
        }

        private class CustomFieldItem : DataControlFieldsEditor.FieldItem
        {
            public CustomFieldItem(DataControlFieldsEditor fieldsEditor, DataControlField runtimeField) : base(fieldsEditor, runtimeField, 3)
            {
            }
        }

        private sealed class DataControlFieldDesignerItem : DataControlFieldsEditor.FieldItem
        {
            private DataControlFieldDesigner _fieldDesigner;

            public DataControlFieldDesignerItem(DataControlFieldDesigner fieldDesigner, DataControlField runtimeField) : base(null, runtimeField, 15)
            {
                this._fieldDesigner = fieldDesigner;
                base.Text = this.GetDefaultNodeText();
            }

            protected override string GetDefaultNodeText()
            {
                if (this._fieldDesigner != null)
                {
                    return this._fieldDesigner.GetNodeText(base.RuntimeField);
                }
                return base.GetDefaultNodeText();
            }

            public override TemplateField GetTemplateField(DataBoundControl dataBoundControl)
            {
                if (this._fieldDesigner != null)
                {
                    return this._fieldDesigner.CreateTemplateField(base.RuntimeField, dataBoundControl);
                }
                return base.GetTemplateField(dataBoundControl);
            }
        }

        private sealed class DataControlFieldDesignerNode : DataControlFieldsEditor.AvailableFieldNode
        {
            private DataControlFieldDesigner _fieldDesigner;
            private IDataSourceFieldSchema _fieldSchema;

            public DataControlFieldDesignerNode(DataControlFieldDesigner fieldDesigner) : base(fieldDesigner.DefaultNodeText, 15)
            {
                this._fieldDesigner = fieldDesigner;
            }

            public DataControlFieldDesignerNode(DataControlFieldDesigner fieldDesigner, IDataSourceFieldSchema fieldSchema) : base((fieldSchema == null) ? fieldDesigner.DefaultNodeText : fieldSchema.Name, 15)
            {
                this._fieldSchema = fieldSchema;
                this._fieldDesigner = fieldDesigner;
            }

            public override DataControlFieldsEditor.FieldItem CreateField()
            {
                DataControlField runtimeField = (this._fieldSchema == null) ? this._fieldDesigner.CreateField() : this._fieldDesigner.CreateField(this._fieldSchema);
                DataControlFieldsEditor.FieldItem item = new DataControlFieldsEditor.DataControlFieldDesignerItem(this._fieldDesigner, runtimeField);
                item.LoadFieldInfo();
                return item;
            }
        }

        private class DataFieldNode : DataControlFieldsEditor.AvailableFieldNode
        {
            private DataControlFieldsEditor _fieldsEditor;

            public DataFieldNode(DataControlFieldsEditor fieldsEditor) : base(System.Design.SR.GetString("DCFEditor_Node_AllFields"), 2)
            {
                this._fieldsEditor = fieldsEditor;
            }

            public override DataControlFieldsEditor.FieldItem CreateField()
            {
                throw new NotSupportedException();
            }

            public override DataControlFieldsEditor.FieldItem[] CreateFields(DataBoundControl control, IDataSourceFieldSchema[] fieldSchemas)
            {
                if (fieldSchemas == null)
                {
                    return null;
                }
                ArrayList list = new ArrayList();
                foreach (IDataSourceFieldSchema schema in fieldSchemas)
                {
                    if (((control is GridView) && ((GridView) control).IsBindableType(schema.DataType)) || ((control is DetailsView) && ((DetailsView) control).IsBindableType(schema.DataType)))
                    {
                        BoundField runtimeField = null;
                        DataControlFieldsEditor.FieldItem item = null;
                        string name = schema.Name;
                        if ((schema.DataType == typeof(bool)) || (schema.DataType == typeof(bool?)))
                        {
                            runtimeField = new CheckBoxField {
                                HeaderText = name,
                                DataField = name,
                                SortExpression = name
                            };
                            item = new DataControlFieldsEditor.CheckBoxFieldItem(this._fieldsEditor, (CheckBoxField) runtimeField);
                        }
                        else
                        {
                            runtimeField = new BoundField {
                                HeaderText = name,
                                DataField = name,
                                SortExpression = name
                            };
                            item = new DataControlFieldsEditor.BoundFieldItem(this._fieldsEditor, runtimeField);
                        }
                        if (schema.PrimaryKey)
                        {
                            runtimeField.ReadOnly = true;
                        }
                        if (schema.Identity)
                        {
                            runtimeField.InsertVisible = false;
                        }
                        item.LoadFieldInfo();
                        list.Add(item);
                    }
                }
                return (DataControlFieldsEditor.FieldItem[]) list.ToArray(typeof(DataControlFieldsEditor.FieldItem));
            }

            public override bool CreatesMultipleFields
            {
                get
                {
                    return true;
                }
            }
        }

        private class DataSourceNode : DataControlFieldsEditor.AvailableFieldNode
        {
            public DataSourceNode() : base(System.Design.SR.GetString("DCFEditor_Node_Bound"), 0)
            {
            }

            public DataSourceNode(string nodeText) : base(nodeText, 0)
            {
            }

            public override bool IsFieldCreator
            {
                get
                {
                    return false;
                }
            }
        }

        private abstract class FieldItem : ListViewItem
        {
            protected DataControlFieldsEditor fieldsEditor;
            protected DataControlField runtimeField;

            public FieldItem(DataControlFieldsEditor fieldsEditor, DataControlField runtimeField, int image) : base(string.Empty, image)
            {
                this.fieldsEditor = fieldsEditor;
                this.runtimeField = runtimeField;
                base.Text = this.GetNodeText(null);
            }

            protected virtual string GetDefaultNodeText()
            {
                return this.runtimeField.GetType().Name;
            }

            public virtual string GetNodeText(string headerText)
            {
                if ((headerText != null) && (headerText.Length != 0))
                {
                    return headerText;
                }
                return this.GetDefaultNodeText();
            }

            protected ITemplate GetTemplate(DataBoundControl control, string templateContent)
            {
                return DataControlFieldHelper.GetTemplate(control, templateContent);
            }

            public virtual TemplateField GetTemplateField(DataBoundControl dataBoundControl)
            {
                return DataControlFieldHelper.GetTemplateField(this.runtimeField, dataBoundControl);
            }

            public virtual void LoadFieldInfo()
            {
                this.UpdateDisplayText();
            }

            protected string PrepareFormatString(string formatString)
            {
                return formatString.Replace("'", "&#039;");
            }

            protected void UpdateDisplayText()
            {
                base.Text = this.GetNodeText(this.HeaderText);
            }

            public string HeaderText
            {
                get
                {
                    return this.runtimeField.HeaderText;
                }
                set
                {
                    this.runtimeField.HeaderText = value;
                    this.UpdateDisplayText();
                }
            }

            public DataControlField RuntimeField
            {
                get
                {
                    return this.runtimeField;
                }
            }
        }

        private class HyperLinkFieldItem : DataControlFieldsEditor.FieldItem
        {
            public HyperLinkFieldItem(DataControlFieldsEditor fieldsEditor, HyperLinkField runtimeField) : base(fieldsEditor, runtimeField, 8)
            {
            }

            protected override string GetDefaultNodeText()
            {
                string text = ((HyperLinkField) base.RuntimeField).Text;
                if ((text != null) && (text.Length != 0))
                {
                    return text;
                }
                return System.Design.SR.GetString("DCFEditor_Node_HyperLink");
            }

            public override TemplateField GetTemplateField(DataBoundControl dataBoundControl)
            {
                TemplateField templateField = base.GetTemplateField(dataBoundControl);
                HyperLinkField runtimeField = (HyperLinkField) base.RuntimeField;
                System.Type controlType = typeof(HyperLink);
                StringBuilder builder = new StringBuilder();
                builder.Append("<asp:");
                builder.Append(controlType.Name);
                builder.Append(" runat=\"server\"");
                if (runtimeField.DataTextField.Length != 0)
                {
                    builder.Append(" Text='<%# ");
                    builder.Append(DesignTimeDataBinding.CreateEvalExpression(runtimeField.DataTextField, base.PrepareFormatString(runtimeField.DataTextFormatString)));
                    builder.Append(" %>'");
                }
                else
                {
                    builder.Append(" Text=\"");
                    builder.Append(runtimeField.Text);
                    builder.Append("\"");
                }
                if ((runtimeField.DataNavigateUrlFields.Length != 0) && (runtimeField.DataNavigateUrlFields[0].Length > 0))
                {
                    builder.Append(" NavigateUrl='<%# ");
                    builder.Append(DesignTimeDataBinding.CreateEvalExpression(runtimeField.DataNavigateUrlFields[0], base.PrepareFormatString(runtimeField.DataNavigateUrlFormatString)));
                    builder.Append(" %>'");
                }
                else
                {
                    builder.Append(" NavigateUrl=\"");
                    builder.Append(runtimeField.NavigateUrl);
                    builder.Append("\"");
                }
                if (runtimeField.Target.Length != 0)
                {
                    builder.Append(" Target=\"");
                    builder.Append(runtimeField.Target);
                    builder.Append("\"");
                }
                builder.Append(" id=\"");
                builder.Append(base.fieldsEditor.GetNewDataSourceName(controlType, 0));
                builder.Append("\"></asp:");
                builder.Append(controlType.Name);
                builder.Append(">");
                templateField.ItemTemplate = base.GetTemplate(dataBoundControl, builder.ToString());
                return templateField;
            }
        }

        private class HyperLinkNode : DataControlFieldsEditor.AvailableFieldNode
        {
            private DataControlFieldsEditor _fieldsEditor;
            private string hyperLinkText;

            public HyperLinkNode(DataControlFieldsEditor fieldsEditor) : this(fieldsEditor, System.Design.SR.GetString("DCFEditor_HyperLink"))
            {
            }

            public HyperLinkNode(DataControlFieldsEditor fieldsEditor, string hyperLinkText) : base(System.Design.SR.GetString("DCFEditor_Node_HyperLink"), 8)
            {
                this._fieldsEditor = fieldsEditor;
                this.hyperLinkText = hyperLinkText;
            }

            public override DataControlFieldsEditor.FieldItem CreateField()
            {
                HyperLinkField runtimeField = new HyperLinkField();
                DataControlFieldsEditor.FieldItem item = new DataControlFieldsEditor.HyperLinkFieldItem(this._fieldsEditor, runtimeField) {
                    Text = this.hyperLinkText
                };
                item.LoadFieldInfo();
                return item;
            }
        }

        private class ImageFieldItem : DataControlFieldsEditor.FieldItem
        {
            public ImageFieldItem(DataControlFieldsEditor fieldsEditor, ImageField runtimeField) : base(fieldsEditor, runtimeField, 14)
            {
            }

            protected override string GetDefaultNodeText()
            {
                string dataImageUrlField = ((ImageField) base.RuntimeField).DataImageUrlField;
                if ((dataImageUrlField != null) && (dataImageUrlField.Length != 0))
                {
                    return dataImageUrlField;
                }
                return System.Design.SR.GetString("DCFEditor_Node_Image");
            }

            private string GetTemplateContent(int editMode)
            {
                System.Type type;
                string alternateText;
                StringBuilder builder = new StringBuilder();
                string dataImageUrlField = ((ImageField) base.RuntimeField).DataImageUrlField;
                string dataAlternateTextField = ((ImageField) base.runtimeField).DataAlternateTextField;
                if (dataAlternateTextField.Length > 0)
                {
                    string dataAlternateTextFormatString = ((ImageField) base.runtimeField).DataAlternateTextFormatString;
                    alternateText = "'<%# " + DesignTimeDataBinding.CreateEvalExpression(dataAlternateTextField, base.PrepareFormatString(dataAlternateTextFormatString)) + " %>'";
                }
                else
                {
                    alternateText = ((ImageField) base.runtimeField).AlternateText;
                }
                if (editMode == 0)
                {
                    type = typeof(System.Web.UI.WebControls.Image);
                }
                else
                {
                    type = typeof(System.Web.UI.WebControls.TextBox);
                }
                builder.Append("<asp:");
                builder.Append(type.Name);
                builder.Append(" runat=\"server\"");
                if (dataImageUrlField.Length > 0)
                {
                    if (type == typeof(System.Web.UI.WebControls.Image))
                    {
                        builder.Append(" ImageUrl='<%# ");
                        builder.Append(DesignTimeDataBinding.CreateEvalExpression(dataImageUrlField, base.PrepareFormatString(((ImageField) base.runtimeField).DataImageUrlFormatString)));
                    }
                    else if (type == typeof(System.Web.UI.WebControls.TextBox))
                    {
                        builder.Append(" Text='<%# ");
                        builder.Append(DesignTimeDataBinding.CreateEvalExpression(dataImageUrlField, string.Empty));
                    }
                    builder.Append(" %>' ");
                }
                if (alternateText.Length > 0)
                {
                    if (type == typeof(System.Web.UI.WebControls.TextBox))
                    {
                        builder.Append(" Tooltip=");
                    }
                    else
                    {
                        builder.Append(" AlternateText=");
                    }
                    builder.Append(alternateText);
                }
                builder.Append(" id=\"");
                builder.Append(base.fieldsEditor.GetNewDataSourceName(type, editMode));
                builder.Append("\"></asp:");
                builder.Append(type.Name);
                builder.Append(">");
                return builder.ToString();
            }

            public override TemplateField GetTemplateField(DataBoundControl dataBoundControl)
            {
                TemplateField templateField = base.GetTemplateField(dataBoundControl);
                ImageField runtimeField = (ImageField) base.RuntimeField;
                templateField.SortExpression = runtimeField.SortExpression;
                templateField.ItemTemplate = base.GetTemplate(dataBoundControl, this.GetTemplateContent(0));
                templateField.ConvertEmptyStringToNull = runtimeField.ConvertEmptyStringToNull;
                if (!runtimeField.ReadOnly)
                {
                    templateField.EditItemTemplate = base.GetTemplate(dataBoundControl, this.GetTemplateContent(1));
                    if (dataBoundControl is DetailsView)
                    {
                        templateField.InsertItemTemplate = base.GetTemplate(dataBoundControl, this.GetTemplateContent(2));
                    }
                }
                return templateField;
            }
        }

        private class ImageNode : DataControlFieldsEditor.AvailableFieldNode
        {
            private DataControlFieldsEditor _fieldsEditor;

            public ImageNode(DataControlFieldsEditor fieldsEditor) : base(string.Empty, 14)
            {
                this._fieldsEditor = fieldsEditor;
                base.Text = System.Design.SR.GetString("DCFEditor_Node_Image");
            }

            public override DataControlFieldsEditor.FieldItem CreateField()
            {
                ImageField runtimeField = new ImageField();
                DataControlFieldsEditor.FieldItem item = new DataControlFieldsEditor.ImageFieldItem(this._fieldsEditor, runtimeField);
                item.LoadFieldInfo();
                return item;
            }
        }

        private class ListViewWithEnter : ListView
        {
            protected override bool IsInputKey(Keys keyCode)
            {
                return ((keyCode == Keys.Enter) || base.IsInputKey(keyCode));
            }
        }

        private class TemplateFieldItem : DataControlFieldsEditor.FieldItem
        {
            public TemplateFieldItem(DataControlFieldsEditor fieldsEditor, TemplateField runtimeField) : base(fieldsEditor, runtimeField, 9)
            {
            }

            protected override string GetDefaultNodeText()
            {
                return System.Design.SR.GetString("DCFEditor_Node_Template");
            }
        }

        private class TemplateNode : DataControlFieldsEditor.AvailableFieldNode
        {
            private DataControlFieldsEditor _fieldsEditor;

            public TemplateNode(DataControlFieldsEditor fieldsEditor) : base(System.Design.SR.GetString("DCFEditor_Node_Template"), 9)
            {
                this._fieldsEditor = fieldsEditor;
            }

            public override DataControlFieldsEditor.FieldItem CreateField()
            {
                TemplateField runtimeField = new TemplateField();
                DataControlFieldsEditor.FieldItem item = new DataControlFieldsEditor.TemplateFieldItem(this._fieldsEditor, runtimeField);
                item.LoadFieldInfo();
                return item;
            }
        }

        private class TreeViewWithEnter : System.Windows.Forms.TreeView
        {
            protected override bool IsInputKey(Keys keyCode)
            {
                return ((keyCode == Keys.Enter) || base.IsInputKey(keyCode));
            }
        }
    }
}

