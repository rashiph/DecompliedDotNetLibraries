namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Globalization;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    public class GridViewDesigner : DataBoundControlDesigner
    {
        private GridViewActionList _actionLists;
        private static DesignerAutoFormatCollection _autoFormats;
        private static string[] _columnTemplateNames = new string[] { "ItemTemplate", "AlternatingItemTemplate", "EditItemTemplate", "HeaderTemplate", "FooterTemplate" };
        private static bool[] _columnTemplateSupportsDataBinding = new bool[] { true, true, true, false, false };
        private static string[] _controlTemplateNames = new string[] { "EmptyDataTemplate", "PagerTemplate" };
        private static bool[] _controlTemplateSupportsDataBinding = new bool[] { true, true };
        private bool _currentDeleteState;
        private bool _currentEditState;
        private bool _currentSelectState;
        internal bool _ignoreSchemaRefreshedEvent;
        private int _regionCount;
        private const int BASE_INDEX = 0x3e8;
        private const int IDX_COLUMN_ALTITEM_TEMPLATE = 1;
        private const int IDX_COLUMN_EDITITEM_TEMPLATE = 2;
        private const int IDX_COLUMN_FOOTER_TEMPLATE = 4;
        private const int IDX_COLUMN_HEADER_TEMPLATE = 3;
        private const int IDX_COLUMN_ITEM_TEMPLATE = 0;
        private const int IDX_CONTROL_EMPTY_DATA_TEMPLATE = 0;
        private const int IDX_CONTROL_PAGER_TEMPLATE = 1;

        private void AddKeysAndBoundFields(IDataSourceViewSchema schema)
        {
            DataControlFieldCollection columns = ((GridView) base.Component).Columns;
            if (schema != null)
            {
                IDataSourceFieldSchema[] fields = schema.GetFields();
                if ((fields != null) && (fields.Length > 0))
                {
                    ArrayList list = new ArrayList();
                    foreach (IDataSourceFieldSchema schema2 in fields)
                    {
                        if (((GridView) base.Component).IsBindableType(schema2.DataType))
                        {
                            BoundField field;
                            if ((schema2.DataType == typeof(bool)) || (schema2.DataType == typeof(bool?)))
                            {
                                field = new CheckBoxField();
                            }
                            else
                            {
                                field = new BoundField();
                            }
                            string name = schema2.Name;
                            if (schema2.PrimaryKey)
                            {
                                list.Add(name);
                            }
                            field.DataField = name;
                            field.HeaderText = name;
                            field.SortExpression = name;
                            field.ReadOnly = schema2.PrimaryKey || schema2.IsReadOnly;
                            field.InsertVisible = !schema2.Identity;
                            columns.Add(field);
                        }
                    }
                    ((GridView) base.Component).AutoGenerateColumns = false;
                    int count = list.Count;
                    if (count > 0)
                    {
                        string[] array = new string[count];
                        list.CopyTo(array, 0);
                        ((GridView) base.Component).DataKeyNames = array;
                    }
                }
            }
        }

        internal void AddNewField()
        {
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                this._ignoreSchemaRefreshedEvent = true;
                ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.AddNewFieldChangeCallback), null, System.Design.SR.GetString("GridView_AddNewFieldTransaction"));
                this._ignoreSchemaRefreshedEvent = false;
                this.UpdateDesignTimeHtml();
            }
            finally
            {
                Cursor.Current = current;
            }
        }

        private bool AddNewFieldChangeCallback(object context)
        {
            if (base.DataSourceDesigner != null)
            {
                base.DataSourceDesigner.SuppressDataSourceEvents();
            }
            AddDataControlFieldDialog form = new AddDataControlFieldDialog(this);
            DialogResult result = UIServiceHelper.ShowDialog(base.Component.Site, form);
            if (base.DataSourceDesigner != null)
            {
                base.DataSourceDesigner.ResumeDataSourceEvents();
            }
            return (result == DialogResult.OK);
        }

        protected override void DataBind(BaseDataBoundControl dataBoundControl)
        {
            GridView view = (GridView) dataBoundControl;
            view.RowDataBound += new GridViewRowEventHandler(this.OnRowDataBound);
            try
            {
                base.DataBind(dataBoundControl);
            }
            finally
            {
                view.RowDataBound -= new GridViewRowEventHandler(this.OnRowDataBound);
            }
        }

        internal void EditFields()
        {
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                this._ignoreSchemaRefreshedEvent = true;
                ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.EditFieldsChangeCallback), null, System.Design.SR.GetString("GridView_EditFieldsTransaction"));
                this._ignoreSchemaRefreshedEvent = false;
                this.UpdateDesignTimeHtml();
            }
            finally
            {
                Cursor.Current = current;
            }
        }

        private bool EditFieldsChangeCallback(object context)
        {
            if (base.DataSourceDesigner != null)
            {
                base.DataSourceDesigner.SuppressDataSourceEvents();
            }
            DataControlFieldsEditor form = new DataControlFieldsEditor(this);
            DialogResult result = UIServiceHelper.ShowDialog(base.Component.Site, form);
            if (base.DataSourceDesigner != null)
            {
                base.DataSourceDesigner.ResumeDataSourceEvents();
            }
            return (result == DialogResult.OK);
        }

        private bool EnableDeletingCallback(object context)
        {
            bool newState = !this._currentDeleteState;
            if (context is bool)
            {
                newState = (bool) context;
            }
            this.SaveManipulationSetting(ManipulationMode.Delete, newState);
            return true;
        }

        private bool EnableEditingCallback(object context)
        {
            bool newState = !this._currentEditState;
            if (context is bool)
            {
                newState = (bool) context;
            }
            this.SaveManipulationSetting(ManipulationMode.Edit, newState);
            return true;
        }

        private bool EnablePagingCallback(object context)
        {
            bool flag2 = !((GridView) base.Component).AllowPaging;
            if (context is bool)
            {
                flag2 = (bool) context;
            }
            TypeDescriptor.GetProperties(typeof(GridView))["AllowPaging"].SetValue(base.Component, flag2);
            return true;
        }

        private bool EnableSelectionCallback(object context)
        {
            bool newState = !this._currentEditState;
            if (context is bool)
            {
                newState = (bool) context;
            }
            this.SaveManipulationSetting(ManipulationMode.Select, newState);
            return true;
        }

        private bool EnableSortingCallback(object context)
        {
            bool flag2 = !((GridView) base.Component).AllowSorting;
            if (context is bool)
            {
                flag2 = (bool) context;
            }
            TypeDescriptor.GetProperties(typeof(GridView))["AllowSorting"].SetValue(base.Component, flag2);
            return true;
        }

        private IDataSourceViewSchema GetDataSourceSchema()
        {
            DesignerDataSourceView designerView = base.DesignerView;
            if (designerView != null)
            {
                try
                {
                    return designerView.Schema;
                }
                catch (Exception exception)
                {
                    IComponentDesignerDebugService service = (IComponentDesignerDebugService) base.Component.Site.GetService(typeof(IComponentDesignerDebugService));
                    if (service != null)
                    {
                        service.Fail(System.Design.SR.GetString("DataSource_DebugService_FailedCall", new object[] { "DesignerDataSourceView.Schema", exception.Message }));
                    }
                }
            }
            return null;
        }

        public override string GetDesignTimeHtml()
        {
            GridView viewControl = (GridView) base.ViewControl;
            viewControl.EnablePersistedSelection = false;
            IDataSourceDesigner dataSourceDesigner = base.DataSourceDesigner;
            this._regionCount = 0;
            bool flag = false;
            IDataSourceViewSchema dataSourceSchema = this.GetDataSourceSchema();
            if (dataSourceSchema != null)
            {
                IDataSourceFieldSchema[] fields = dataSourceSchema.GetFields();
                if ((fields != null) && (fields.Length > 0))
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                viewControl.DataKeyNames = null;
            }
            if (viewControl.Columns.Count == 0)
            {
                viewControl.AutoGenerateColumns = true;
            }
            TypeDescriptor.Refresh(base.Component);
            return base.GetDesignTimeHtml();
        }

        public override string GetDesignTimeHtml(DesignerRegionCollection regions)
        {
            string designTimeHtml = this.GetDesignTimeHtml();
            GridView viewControl = (GridView) base.ViewControl;
            int count = viewControl.Columns.Count;
            GridViewRow headerRow = viewControl.HeaderRow;
            GridViewRow footerRow = viewControl.FooterRow;
            int selectedFieldIndex = this.SelectedFieldIndex;
            if (headerRow != null)
            {
                for (int j = 0; j < count; j++)
                {
                    string name = System.Design.SR.GetString("GridView_Field", new object[] { j.ToString(NumberFormatInfo.InvariantInfo) });
                    string headerText = viewControl.Columns[j].HeaderText;
                    if (headerText.Length == 0)
                    {
                        name = name + " - " + headerText;
                    }
                    DesignerRegion region = new DesignerRegion(this, name, true) {
                        UserData = j
                    };
                    if (j == selectedFieldIndex)
                    {
                        region.Highlight = true;
                    }
                    regions.Add(region);
                }
            }
            for (int i = 0; i < viewControl.Rows.Count; i++)
            {
                GridViewRow row1 = viewControl.Rows[i];
                for (int k = 0; k < count; k++)
                {
                    DesignerRegion region2 = new DesignerRegion(this, k.ToString(NumberFormatInfo.InvariantInfo), false) {
                        UserData = -1
                    };
                    if (k == selectedFieldIndex)
                    {
                        region2.Highlight = true;
                    }
                    regions.Add(region2);
                }
            }
            if (footerRow != null)
            {
                for (int m = 0; m < count; m++)
                {
                    DesignerRegion region3 = new DesignerRegion(this, m.ToString(NumberFormatInfo.InvariantInfo), false) {
                        UserData = -1
                    };
                    if (m == selectedFieldIndex)
                    {
                        region3.Highlight = true;
                    }
                    regions.Add(region3);
                }
            }
            return designTimeHtml;
        }

        public override string GetEditableDesignerRegionContent(EditableDesignerRegion region)
        {
            return string.Empty;
        }

        private Style GetTemplateStyle(int templateIndex, TemplateField templateField)
        {
            Style style = new Style();
            style.CopyFrom(((GridView) base.ViewControl).ControlStyle);
            switch (templateIndex)
            {
                case 0:
                    style.CopyFrom(((GridView) base.ViewControl).EmptyDataRowStyle);
                    return style;

                case 1:
                    style.CopyFrom(((GridView) base.ViewControl).PagerStyle);
                    return style;

                case 0x3e8:
                    style.CopyFrom(((GridView) base.ViewControl).RowStyle);
                    style.CopyFrom(templateField.ItemStyle);
                    return style;

                case 0x3e9:
                    style.CopyFrom(((GridView) base.ViewControl).RowStyle);
                    style.CopyFrom(((GridView) base.ViewControl).AlternatingRowStyle);
                    style.CopyFrom(templateField.ItemStyle);
                    return style;

                case 0x3ea:
                    style.CopyFrom(((GridView) base.ViewControl).RowStyle);
                    style.CopyFrom(((GridView) base.ViewControl).EditRowStyle);
                    style.CopyFrom(templateField.ItemStyle);
                    return style;

                case 0x3eb:
                    style.CopyFrom(((GridView) base.ViewControl).HeaderStyle);
                    style.CopyFrom(templateField.HeaderStyle);
                    return style;

                case 0x3ec:
                    style.CopyFrom(((GridView) base.ViewControl).FooterStyle);
                    style.CopyFrom(templateField.FooterStyle);
                    return style;
            }
            return style;
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(GridView));
            base.Initialize(component);
            if (base.View != null)
            {
                base.View.SetFlags(ViewFlags.TemplateEditing, true);
            }
        }

        internal void MoveLeft()
        {
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.MoveLeftCallback), null, System.Design.SR.GetString("GridView_MoveLeftTransaction"));
                this.UpdateDesignTimeHtml();
            }
            finally
            {
                Cursor.Current = current;
            }
        }

        private bool MoveLeftCallback(object context)
        {
            DataControlFieldCollection columns = ((GridView) base.Component).Columns;
            int selectedFieldIndex = this.SelectedFieldIndex;
            if (selectedFieldIndex > 0)
            {
                DataControlField field = columns[selectedFieldIndex];
                columns.RemoveAt(selectedFieldIndex);
                columns.Insert(selectedFieldIndex - 1, field);
                this.SelectedFieldIndex--;
                this.UpdateDesignTimeHtml();
                return true;
            }
            return false;
        }

        internal void MoveRight()
        {
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.MoveRightCallback), null, System.Design.SR.GetString("GridView_MoveRightTransaction"));
                this.UpdateDesignTimeHtml();
            }
            finally
            {
                Cursor.Current = current;
            }
        }

        private bool MoveRightCallback(object context)
        {
            DataControlFieldCollection columns = ((GridView) base.Component).Columns;
            int selectedFieldIndex = this.SelectedFieldIndex;
            if ((selectedFieldIndex >= 0) && (columns.Count > (selectedFieldIndex + 1)))
            {
                DataControlField field = columns[selectedFieldIndex];
                columns.RemoveAt(selectedFieldIndex);
                columns.Insert(selectedFieldIndex + 1, field);
                this.SelectedFieldIndex++;
                this.UpdateDesignTimeHtml();
                return true;
            }
            return false;
        }

        protected override void OnClick(DesignerRegionMouseEventArgs e)
        {
            if (e.Region != null)
            {
                this.SelectedFieldIndex = (int) e.Region.UserData;
                this.UpdateDesignTimeHtml();
            }
        }

        private void OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            GridViewRow row = e.Row;
            if (((row.RowType == DataControlRowType.DataRow) || (row.RowType == DataControlRowType.Header)) || (row.RowType == DataControlRowType.Footer))
            {
                int count = ((GridView) sender).Columns.Count;
                int num2 = 0;
                if ((((GridView) sender).AutoGenerateDeleteButton || ((GridView) sender).AutoGenerateEditButton) || ((GridView) sender).AutoGenerateSelectButton)
                {
                    num2 = 1;
                }
                for (int i = 0; i < count; i++)
                {
                    TableCell cell = row.Cells[i + num2];
                    cell.Attributes[DesignerRegion.DesignerRegionAttributeName] = this._regionCount.ToString(NumberFormatInfo.InvariantInfo);
                    this._regionCount++;
                }
            }
        }

        protected override void OnSchemaRefreshed()
        {
            if (!base.InTemplateMode && !this._ignoreSchemaRefreshedEvent)
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.SchemaRefreshedCallback), null, System.Design.SR.GetString("GridView_SchemaRefreshedTransaction"));
                    this.UpdateDesignTimeHtml();
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            if (base.InTemplateMode)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["Columns"];
                properties["Columns"] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { BrowsableAttribute.No });
            }
        }

        internal void RemoveField()
        {
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.RemoveFieldCallback), null, System.Design.SR.GetString("GridView_RemoveFieldTransaction"));
                this.UpdateDesignTimeHtml();
            }
            finally
            {
                Cursor.Current = current;
            }
        }

        private bool RemoveFieldCallback(object context)
        {
            int selectedFieldIndex = this.SelectedFieldIndex;
            if (selectedFieldIndex < 0)
            {
                return false;
            }
            ((GridView) base.Component).Columns.RemoveAt(selectedFieldIndex);
            if (selectedFieldIndex == ((GridView) base.Component).Columns.Count)
            {
                this.SelectedFieldIndex--;
                this.UpdateDesignTimeHtml();
            }
            return true;
        }

        private void SaveManipulationSetting(ManipulationMode mode, bool newState)
        {
            DataControlFieldCollection columns = ((GridView) base.Component).Columns;
            bool flag = false;
            ArrayList list = new ArrayList();
            foreach (DataControlField field in columns)
            {
                CommandField field2 = field as CommandField;
                if (field2 != null)
                {
                    switch (mode)
                    {
                        case ManipulationMode.Edit:
                            field2.ShowEditButton = newState;
                            break;

                        case ManipulationMode.Delete:
                            field2.ShowDeleteButton = newState;
                            break;

                        case ManipulationMode.Select:
                            field2.ShowSelectButton = newState;
                            break;
                    }
                    if (((!newState && !field2.ShowEditButton) && (!field2.ShowDeleteButton && !field2.ShowInsertButton)) && !field2.ShowSelectButton)
                    {
                        list.Add(field2);
                    }
                    flag = true;
                }
            }
            foreach (object obj2 in list)
            {
                columns.Remove((DataControlField) obj2);
            }
            if (!flag && newState)
            {
                CommandField field3 = new CommandField();
                switch (mode)
                {
                    case ManipulationMode.Edit:
                        field3.ShowEditButton = newState;
                        break;

                    case ManipulationMode.Delete:
                        field3.ShowDeleteButton = newState;
                        break;

                    case ManipulationMode.Select:
                        field3.ShowSelectButton = newState;
                        break;
                }
                columns.Insert(0, field3);
            }
            if (!newState)
            {
                PropertyDescriptor descriptor;
                GridView component = (GridView) base.Component;
                switch (mode)
                {
                    case ManipulationMode.Edit:
                        descriptor = TypeDescriptor.GetProperties(typeof(GridView))["AutoGenerateEditButton"];
                        descriptor.SetValue(base.Component, newState);
                        return;

                    case ManipulationMode.Delete:
                        descriptor = TypeDescriptor.GetProperties(typeof(GridView))["AutoGenerateDeleteButton"];
                        descriptor.SetValue(base.Component, newState);
                        return;

                    case ManipulationMode.Select:
                        TypeDescriptor.GetProperties(typeof(GridView))["AutoGenerateSelectButton"].SetValue(base.Component, newState);
                        break;

                    default:
                        return;
                }
            }
        }

        private bool SchemaRefreshedCallback(object context)
        {
            IDataSourceViewSchema dataSourceSchema = this.GetDataSourceSchema();
            if ((base.DataSourceID.Length > 0) && (dataSourceSchema != null))
            {
                if ((((GridView) base.Component).Columns.Count > 0) || (((GridView) base.Component).DataKeyNames.Length > 0))
                {
                    if (DialogResult.Yes == UIServiceHelper.ShowMessage(base.Component.Site, System.Design.SR.GetString("DataBoundControl_SchemaRefreshedWarning", new object[] { System.Design.SR.GetString("DataBoundControl_GridView"), System.Design.SR.GetString("DataBoundControl_Column") }), System.Design.SR.GetString("DataBoundControl_SchemaRefreshedCaption", new object[] { ((GridView) base.Component).ID }), MessageBoxButtons.YesNo))
                    {
                        ((GridView) base.Component).DataKeyNames = new string[0];
                        ((GridView) base.Component).Columns.Clear();
                        this.SelectedFieldIndex = -1;
                        this.AddKeysAndBoundFields(dataSourceSchema);
                    }
                }
                else
                {
                    this.AddKeysAndBoundFields(dataSourceSchema);
                }
            }
            else if (((((GridView) base.Component).Columns.Count > 0) || (((GridView) base.Component).DataKeyNames.Length > 0)) && (DialogResult.Yes == UIServiceHelper.ShowMessage(base.Component.Site, System.Design.SR.GetString("DataBoundControl_SchemaRefreshedWarningNoDataSource", new object[] { System.Design.SR.GetString("DataBoundControl_GridView"), System.Design.SR.GetString("DataBoundControl_Column") }), System.Design.SR.GetString("DataBoundControl_SchemaRefreshedCaption", new object[] { ((GridView) base.Component).ID }), MessageBoxButtons.YesNo)))
            {
                ((GridView) base.Component).DataKeyNames = new string[0];
                ((GridView) base.Component).Columns.Clear();
                this.SelectedFieldIndex = -1;
            }
            return true;
        }

        public override void SetEditableDesignerRegionContent(EditableDesignerRegion region, string content)
        {
        }

        private void UpdateFieldsCurrentState()
        {
            this._currentSelectState = ((GridView) base.Component).AutoGenerateSelectButton;
            this._currentEditState = ((GridView) base.Component).AutoGenerateEditButton;
            this._currentDeleteState = ((GridView) base.Component).AutoGenerateDeleteButton;
            foreach (DataControlField field in ((GridView) base.Component).Columns)
            {
                CommandField field2 = field as CommandField;
                if (field2 != null)
                {
                    if (field2.ShowSelectButton)
                    {
                        this._currentSelectState = true;
                    }
                    if (field2.ShowEditButton)
                    {
                        this._currentEditState = true;
                    }
                    if (field2.ShowDeleteButton)
                    {
                        this._currentDeleteState = true;
                    }
                }
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                if (this._actionLists == null)
                {
                    this._actionLists = new GridViewActionList(this);
                }
                bool inTemplateMode = base.InTemplateMode;
                int selectedFieldIndex = this.SelectedFieldIndex;
                this.UpdateFieldsCurrentState();
                this._actionLists.AllowRemoveField = ((((GridView) base.Component).Columns.Count > 0) && (selectedFieldIndex >= 0)) && !inTemplateMode;
                this._actionLists.AllowMoveLeft = ((((GridView) base.Component).Columns.Count > 0) && (selectedFieldIndex > 0)) && !inTemplateMode;
                this._actionLists.AllowMoveRight = (((((GridView) base.Component).Columns.Count > 0) && (selectedFieldIndex >= 0)) && (((GridView) base.Component).Columns.Count > (selectedFieldIndex + 1))) && !inTemplateMode;
                DesignerDataSourceView designerView = base.DesignerView;
                this._actionLists.AllowPaging = !inTemplateMode && (designerView != null);
                this._actionLists.AllowSorting = !inTemplateMode && ((designerView != null) && designerView.CanSort);
                this._actionLists.AllowEditing = !inTemplateMode && ((designerView != null) && designerView.CanUpdate);
                this._actionLists.AllowDeleting = !inTemplateMode && ((designerView != null) && designerView.CanDelete);
                this._actionLists.AllowSelection = !inTemplateMode && (designerView != null);
                lists.Add(this._actionLists);
                return lists;
            }
        }

        public override DesignerAutoFormatCollection AutoFormats
        {
            get
            {
                if (_autoFormats == null)
                {
                    _autoFormats = ControlDesigner.CreateAutoFormats(AutoFormatSchemes.GRIDVIEW_SCHEME_NAMES, schemeName => new GridViewAutoFormat(schemeName, "<Schemes>\r\n        <xsd:schema id=\"Schemes\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n          <xsd:element name=\"Scheme\">\r\n            <xsd:complexType>\r\n              <xsd:all>\r\n                <xsd:element name=\"SchemeName\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"GridLines\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"CellPadding\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"CellSpacing\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"ItemForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"ItemBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"ItemFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"AltItemForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"AltItemBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"AltItemFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SelItemForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SelItemBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SelItemFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FooterForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FooterBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FooterFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerAlign\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerButtons\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"EditItemForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"EditItemBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"EditItemFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SortedDescendingCellBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SortedDescendingHeaderBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SortedAscendingCellBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SortedAscendingHeaderBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n              </xsd:all>\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n          <xsd:element name=\"Schemes\" msdata:IsDataSet=\"true\">\r\n            <xsd:complexType>\r\n              <xsd:choice maxOccurs=\"unbounded\">\r\n                <xsd:element ref=\"Scheme\"/>\r\n              </xsd:choice>\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n        </xsd:schema>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Empty</SchemeName>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Consistent1</SchemeName>\r\n          <AltItemBackColor>White</AltItemBackColor>\r\n          <GridLines>0</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <ForeColor>#333333</ForeColor>\r\n          <ItemForeColor>#333333</ItemForeColor>\r\n          <ItemBackColor>#FFFBD6</ItemBackColor>\r\n          <SelItemForeColor>Navy</SelItemForeColor>\r\n          <SelItemBackColor>#FFCC66</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#990000</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>White</FooterForeColor>\r\n          <FooterBackColor>#990000</FooterBackColor>\r\n          <FooterFont>1</FooterFont>\r\n          <PagerForeColor>#333333</PagerForeColor>\r\n          <PagerBackColor>#FFCC66</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <SortedDescendingCellBackColor>#FCF6C0</SortedDescendingCellBackColor>\r\n          <SortedDescendingHeaderBackColor>#820000</SortedDescendingHeaderBackColor>\r\n          <SortedAscendingCellBackColor>#FDF5AC</SortedAscendingCellBackColor>\r\n          <SortedAscendingHeaderBackColor>#4D0000</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Consistent2</SchemeName>\r\n            <AltItemBackColor>White</AltItemBackColor>\r\n            <GridLines>0</GridLines>\r\n            <CellPadding>4</CellPadding>\r\n            <ForeColor>#333333</ForeColor>\r\n            <ItemBackColor>#EFF3FB</ItemBackColor>\r\n            <SelItemForeColor>#333333</SelItemForeColor>\r\n            <SelItemBackColor>#D1DDF1</SelItemBackColor>\r\n            <SelItemFont>1</SelItemFont>\r\n            <HeaderForeColor>White</HeaderForeColor>\r\n            <HeaderBackColor>#507CD1</HeaderBackColor>\r\n            <HeaderFont>1</HeaderFont>\r\n            <FooterForeColor>White</FooterForeColor>\r\n            <FooterBackColor>#507CD1</FooterBackColor>\r\n            <FooterFont>1</FooterFont>\r\n            <PagerForeColor>White</PagerForeColor>\r\n            <PagerBackColor>#2461BF</PagerBackColor>\r\n            <PagerAlign>2</PagerAlign>\r\n            <EditItemBackColor>#2461BF</EditItemBackColor>\r\n            <SortedDescendingCellBackColor>#E9EBEF</SortedDescendingCellBackColor>\r\n            <SortedDescendingHeaderBackColor>#4870BE</SortedDescendingHeaderBackColor>\r\n            <SortedAscendingCellBackColor>#F5F7FB</SortedAscendingCellBackColor>\r\n            <SortedAscendingHeaderBackColor>#6D95E1</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Consistent3</SchemeName>\r\n            <AltItemBackColor>White</AltItemBackColor>\r\n            <GridLines>0</GridLines>\r\n            <CellPadding>4</CellPadding>\r\n            <ForeColor>#333333</ForeColor>\r\n            <ItemBackColor>#E3EAEB</ItemBackColor>\r\n            <SelItemForeColor>#333333</SelItemForeColor>\r\n            <SelItemBackColor>#C5BBAF</SelItemBackColor>\r\n            <SelItemFont>1</SelItemFont>\r\n            <HeaderForeColor>White</HeaderForeColor>\r\n            <HeaderBackColor>#1C5E55</HeaderBackColor>\r\n            <HeaderFont>1</HeaderFont>\r\n            <FooterForeColor>White</FooterForeColor>\r\n            <FooterBackColor>#1C5E55</FooterBackColor>\r\n            <FooterFont>1</FooterFont>\r\n            <PagerForeColor>White</PagerForeColor>\r\n            <PagerBackColor>#666666</PagerBackColor>\r\n            <PagerAlign>2</PagerAlign>\r\n            <EditItemBackColor>#7C6F57</EditItemBackColor>\r\n            <SortedDescendingCellBackColor>#D4DFE1</SortedDescendingCellBackColor>\r\n            <SortedDescendingHeaderBackColor>#15524A</SortedDescendingHeaderBackColor>\r\n            <SortedAscendingCellBackColor>#F8FAFA</SortedAscendingCellBackColor>\r\n            <SortedAscendingHeaderBackColor>#246B61</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Consistent4</SchemeName>\r\n            <AltItemBackColor>White</AltItemBackColor>\r\n            <AltItemForeColor>#284775</AltItemForeColor>\r\n            <GridLines>0</GridLines>\r\n            <CellPadding>4</CellPadding>\r\n            <ForeColor>#333333</ForeColor>\r\n            <ItemForeColor>#333333</ItemForeColor>\r\n            <ItemBackColor>#F7F6F3</ItemBackColor>\r\n            <SelItemForeColor>#333333</SelItemForeColor>\r\n            <SelItemBackColor>#E2DED6</SelItemBackColor>\r\n            <SelItemFont>1</SelItemFont>\r\n            <HeaderForeColor>White</HeaderForeColor>\r\n            <HeaderBackColor>#5D7B9D</HeaderBackColor>\r\n            <HeaderFont>1</HeaderFont>\r\n            <FooterForeColor>White</FooterForeColor>\r\n            <FooterBackColor>#5D7B9D</FooterBackColor>\r\n            <FooterFont>1</FooterFont>\r\n            <PagerForeColor>White</PagerForeColor>\r\n            <PagerBackColor>#284775</PagerBackColor>\r\n            <PagerAlign>2</PagerAlign>\r\n            <EditItemBackColor>#999999</EditItemBackColor>\r\n            <SortedDescendingCellBackColor>#FFFDF8</SortedDescendingCellBackColor>\r\n            <SortedDescendingHeaderBackColor>#6F8DAE</SortedDescendingHeaderBackColor>\r\n            <SortedAscendingCellBackColor>#E9E7E2</SortedAscendingCellBackColor>\r\n            <SortedAscendingHeaderBackColor>#506C8C</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Colorful1</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#CC9966</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <ItemForeColor>#330099</ItemForeColor>\r\n          <ItemBackColor>White</ItemBackColor>\r\n          <SelItemForeColor>#663399</SelItemForeColor>\r\n          <SelItemBackColor>#FFCC66</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>#FFFFCC</HeaderForeColor>\r\n          <HeaderBackColor>#990000</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#330099</FooterForeColor>\r\n          <FooterBackColor>#FFFFCC</FooterBackColor>\r\n          <PagerForeColor>#330099</PagerForeColor>\r\n          <PagerBackColor>#FFFFCC</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <SortedDescendingCellBackColor>#F6F0C0</SortedDescendingCellBackColor>\r\n          <SortedDescendingHeaderBackColor>#7E0000</SortedDescendingHeaderBackColor>\r\n          <SortedAscendingCellBackColor>#FEFCEB</SortedAscendingCellBackColor>\r\n          <SortedAscendingHeaderBackColor>#AF0101</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Colorful2</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#3366CC</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <ItemForeColor>#003399</ItemForeColor>\r\n          <ItemBackColor>White</ItemBackColor>\r\n          <SelItemForeColor>#CCFF99</SelItemForeColor>\r\n          <SelItemBackColor>#009999</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>#CCCCFF</HeaderForeColor>\r\n          <HeaderBackColor>#003399</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#003399</FooterForeColor>\r\n          <FooterBackColor>#99CCCC</FooterBackColor>\r\n          <PagerForeColor>#003399</PagerForeColor>\r\n          <PagerBackColor>#99CCCC</PagerBackColor>\r\n          <PagerAlign>1</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <SortedDescendingCellBackColor>#D6DFDF</SortedDescendingCellBackColor>\r\n          <SortedDescendingHeaderBackColor>#002876</SortedDescendingHeaderBackColor>\r\n          <SortedAscendingCellBackColor>#EDF6F6</SortedAscendingCellBackColor>\r\n          <SortedAscendingHeaderBackColor>#0D4AC4</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Colorful3</SchemeName>\r\n          <BackColor>#DEBA84</BackColor>\r\n          <BorderColor>#DEBA84</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>2</CellSpacing>\r\n          <ItemForeColor>#8C4510</ItemForeColor>\r\n          <ItemBackColor>#FFF7E7</ItemBackColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#738A9C</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#A55129</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#8C4510</FooterForeColor>\r\n          <FooterBackColor>#F7DFB5</FooterBackColor>\r\n          <PagerForeColor>#8C4510</PagerForeColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <SortedDescendingCellBackColor>#F1E5CE</SortedDescendingCellBackColor>\r\n          <SortedDescendingHeaderBackColor>#93451F</SortedDescendingHeaderBackColor>\r\n          <SortedAscendingCellBackColor>#FFF1D4</SortedAscendingCellBackColor>\r\n          <SortedAscendingHeaderBackColor>#B95C30</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Colorful4</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#E7E7FF</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>1</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <ItemForeColor>#4A3C8C</ItemForeColor>\r\n          <ItemBackColor>#E7E7FF</ItemBackColor>\r\n          <AltItemBackColor>#F7F7F7</AltItemBackColor>\r\n          <SelItemForeColor>#F7F7F7</SelItemForeColor>\r\n          <SelItemBackColor>#738A9C</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>#F7F7F7</HeaderForeColor>\r\n          <HeaderBackColor>#4A3C8C</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#4A3C8C</FooterForeColor>\r\n          <FooterBackColor>#B5C7DE</FooterBackColor>\r\n          <PagerForeColor>#4A3C8C</PagerForeColor>\r\n          <PagerBackColor>#E7E7FF</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <SortedDescendingCellBackColor>#D8D8F0</SortedDescendingCellBackColor>\r\n          <SortedDescendingHeaderBackColor>#3E3277</SortedDescendingHeaderBackColor>\r\n          <SortedAscendingCellBackColor>#F4F4FD</SortedAscendingCellBackColor>\r\n          <SortedAscendingHeaderBackColor>#5A4C9D</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Colorful5</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>LightGoldenRodYellow</BackColor>\r\n          <BorderColor>Tan</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <GridLines>0</GridLines>\r\n          <CellPadding>2</CellPadding>\r\n          <AltItemBackColor>PaleGoldenRod</AltItemBackColor>\r\n          <HeaderBackColor>Tan</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>Tan</FooterBackColor>\r\n          <SelItemBackColor>DarkSlateBlue</SelItemBackColor>\r\n          <SelItemForeColor>GhostWhite</SelItemForeColor>\r\n          <PagerBackColor>PaleGoldenrod</PagerBackColor>\r\n          <PagerForeColor>DarkSlateBlue</PagerForeColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <SortedDescendingCellBackColor>#E1DB9C</SortedDescendingCellBackColor>\r\n          <SortedDescendingHeaderBackColor>#C2A47B</SortedDescendingHeaderBackColor>\r\n          <SortedAscendingCellBackColor>#FAFAE7</SortedAscendingCellBackColor>\r\n          <SortedAscendingHeaderBackColor>#DAC09E</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Professional1</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#999999</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>2</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <ItemForeColor>Black</ItemForeColor>\r\n          <ItemBackColor>#EEEEEE</ItemBackColor>\r\n          <AltItemBackColor>#DCDCDC</AltItemBackColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#008A8C</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#000084</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>Black</FooterForeColor>\r\n          <FooterBackColor>#CCCCCC</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#999999</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <SortedDescendingCellBackColor>#CAC9C9</SortedDescendingCellBackColor>\r\n          <SortedDescendingHeaderBackColor>#000065</SortedDescendingHeaderBackColor>\r\n          <SortedAscendingCellBackColor>#F1F1F1</SortedAscendingCellBackColor>\r\n          <SortedAscendingHeaderBackColor>#0000A9</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Professional2</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#CCCCCC</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <ItemForeColor>#000066</ItemForeColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#669999</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#006699</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#000066</FooterForeColor>\r\n          <FooterBackColor>White</FooterBackColor>\r\n          <PagerForeColor>#000066</PagerForeColor>\r\n          <PagerBackColor>White</PagerBackColor>\r\n          <PagerAlign>1</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <SortedDescendingCellBackColor>#CAC9C9</SortedDescendingCellBackColor>\r\n          <SortedDescendingHeaderBackColor>#00547E</SortedDescendingHeaderBackColor>\r\n          <SortedAscendingCellBackColor>#F1F1F1</SortedAscendingCellBackColor>\r\n          <SortedAscendingHeaderBackColor>#007DBB</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Professional3</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>White</BorderColor>\r\n          <BorderWidth>2px</BorderWidth>\r\n          <BorderStyle>7</BorderStyle>\r\n          <GridLines>0</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>1</CellSpacing>\r\n          <ItemForeColor>Black</ItemForeColor>\r\n          <ItemBackColor>#DEDFDE</ItemBackColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#9471DE</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>#E7E7FF</HeaderForeColor>\r\n          <HeaderBackColor>#4A3C8C</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>Black</FooterForeColor>\r\n          <FooterBackColor>#C6C3C6</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#C6C3C6</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n          <SortedDescendingCellBackColor>#CAC9C9</SortedDescendingCellBackColor>\r\n          <SortedDescendingHeaderBackColor>#33276A</SortedDescendingHeaderBackColor>\r\n          <SortedAscendingCellBackColor>#F1F1F1</SortedAscendingCellBackColor>\r\n          <SortedAscendingHeaderBackColor>#594B9C</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Simple1</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#999999</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>4</BorderStyle>\r\n          <GridLines>2</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <AltItemBackColor>#CCCCCC</AltItemBackColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#000099</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>Black</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>#CCCCCC</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#999999</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <SortedDescendingCellBackColor>#CAC9C9</SortedDescendingCellBackColor>\r\n          <SortedDescendingHeaderBackColor>#383838</SortedDescendingHeaderBackColor>\r\n          <SortedAscendingCellBackColor>#F1F1F1</SortedAscendingCellBackColor>\r\n          <SortedAscendingHeaderBackColor>#808080</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Simple2</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>#CCCCCC</BackColor>\r\n          <BorderColor>#999999</BorderColor>\r\n          <BorderWidth>3px</BorderWidth>\r\n          <BorderStyle>4</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>2</CellSpacing>\r\n          <ItemBackColor>White</ItemBackColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#000099</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>Black</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>#CCCCCC</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#CCCCCC</PagerBackColor>\r\n          <PagerAlign>1</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <SortedDescendingCellBackColor>#CAC9C9</SortedDescendingCellBackColor>\r\n          <SortedDescendingHeaderBackColor>#383838</SortedDescendingHeaderBackColor>\r\n          <SortedAscendingCellBackColor>#F1F1F1</SortedAscendingCellBackColor>\r\n          <SortedAscendingHeaderBackColor>#808080</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Simple3</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#336666</BorderColor>\r\n          <BorderWidth>3px</BorderWidth>\r\n          <BorderStyle>5</BorderStyle>\r\n          <GridLines>1</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <ItemForeColor>#333333</ItemForeColor>\r\n          <ItemBackColor>White</ItemBackColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#339966</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#336666</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#333333</FooterForeColor>\r\n          <FooterBackColor>White</FooterBackColor>\r\n          <PagerForeColor>White</PagerForeColor>\r\n          <PagerBackColor>#336666</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <SortedDescendingCellBackColor>#E5E5E5</SortedDescendingCellBackColor>\r\n          <SortedDescendingHeaderBackColor>#275353</SortedDescendingHeaderBackColor>\r\n          <SortedAscendingCellBackColor>#F7F7F7</SortedAscendingCellBackColor>\r\n          <SortedAscendingHeaderBackColor>#487575</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Classic1</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#CCCCCC</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>1</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#CC3333</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#333333</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>Black</FooterForeColor>\r\n          <FooterBackColor>#CCCC99</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>White</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n          <SortedDescendingCellBackColor>#E5E5E5</SortedDescendingCellBackColor>\r\n          <SortedDescendingHeaderBackColor>#242121</SortedDescendingHeaderBackColor>\r\n          <SortedAscendingCellBackColor>#F7F7F7</SortedAscendingCellBackColor>\r\n          <SortedAscendingHeaderBackColor>#4B4B4B</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Classic2</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#DEDFDE</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>2</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <ItemBackColor>#F7F7DE</ItemBackColor>\r\n          <AltItemBackColor>White</AltItemBackColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#CE5D5A</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#6B696B</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>#CCCC99</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#F7F7DE</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <SortedDescendingCellBackColor>#EAEAD3</SortedDescendingCellBackColor>\r\n          <SortedDescendingHeaderBackColor>#575357</SortedDescendingHeaderBackColor>\r\n          <SortedAscendingCellBackColor>#FBFBF2</SortedAscendingCellBackColor>\r\n          <SortedAscendingHeaderBackColor>#848384</SortedAscendingHeaderBackColor>\r\n        </Scheme>\r\n      </Schemes>"));
                }
                return _autoFormats;
            }
        }

        internal bool EnableDeleting
        {
            get
            {
                return this._currentDeleteState;
            }
            set
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.EnableDeletingCallback), value, System.Design.SR.GetString("GridView_EnableDeletingTransaction"));
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        internal bool EnableEditing
        {
            get
            {
                return this._currentEditState;
            }
            set
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.EnableEditingCallback), value, System.Design.SR.GetString("GridView_EnableEditingTransaction"));
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        internal bool EnablePaging
        {
            get
            {
                return ((GridView) base.Component).AllowPaging;
            }
            set
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.EnablePagingCallback), value, System.Design.SR.GetString("GridView_EnablePagingTransaction"));
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        internal bool EnableSelection
        {
            get
            {
                return this._currentSelectState;
            }
            set
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.EnableSelectionCallback), value, System.Design.SR.GetString("GridView_EnableSelectionTransaction"));
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        internal bool EnableSorting
        {
            get
            {
                return ((GridView) base.Component).AllowSorting;
            }
            set
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.EnableSortingCallback), value, System.Design.SR.GetString("GridView_EnableSortingTransaction"));
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        protected override int SampleRowCount
        {
            get
            {
                int num = 5;
                GridView component = (GridView) base.Component;
                if (component.AllowPaging && (component.PageSize != 0))
                {
                    num = Math.Min(component.PageSize, 100) + 1;
                }
                return num;
            }
        }

        private int SelectedFieldIndex
        {
            get
            {
                object obj2 = base.DesignerState["SelectedFieldIndex"];
                int count = ((GridView) base.Component).Columns.Count;
                if (((obj2 != null) && (count != 0)) && ((((int) obj2) >= 0) && (((int) obj2) < count)))
                {
                    return (int) obj2;
                }
                return -1;
            }
            set
            {
                base.DesignerState["SelectedFieldIndex"] = value;
            }
        }

        public override TemplateGroupCollection TemplateGroups
        {
            get
            {
                TemplateGroupCollection templateGroups = base.TemplateGroups;
                DataControlFieldCollection columns = ((GridView) base.Component).Columns;
                int count = columns.Count;
                if (count > 0)
                {
                    for (int j = 0; j < count; j++)
                    {
                        TemplateField templateField = columns[j] as TemplateField;
                        if (templateField != null)
                        {
                            string headerText = columns[j].HeaderText;
                            string groupName = System.Design.SR.GetString("GridView_Field", new object[] { j.ToString(NumberFormatInfo.InvariantInfo) });
                            if ((headerText != null) && (headerText.Length != 0))
                            {
                                groupName = groupName + " - " + headerText;
                            }
                            TemplateGroup group = new TemplateGroup(groupName);
                            for (int k = 0; k < _columnTemplateNames.Length; k++)
                            {
                                string name = _columnTemplateNames[k];
                                TemplateDefinition templateDefinition = new TemplateDefinition(this, name, columns[j], name, this.GetTemplateStyle(k + 0x3e8, templateField)) {
                                    SupportsDataBinding = _columnTemplateSupportsDataBinding[k]
                                };
                                group.AddTemplateDefinition(templateDefinition);
                            }
                            templateGroups.Add(group);
                        }
                    }
                }
                for (int i = 0; i < _controlTemplateNames.Length; i++)
                {
                    string str4 = _controlTemplateNames[i];
                    TemplateGroup group2 = new TemplateGroup(_controlTemplateNames[i]);
                    TemplateDefinition definition2 = new TemplateDefinition(this, str4, base.Component, str4, this.GetTemplateStyle(i, null)) {
                        SupportsDataBinding = _controlTemplateSupportsDataBinding[i]
                    };
                    group2.AddTemplateDefinition(definition2);
                    templateGroups.Add(group2);
                }
                return templateGroups;
            }
        }

        protected override bool UsePreviewControl
        {
            get
            {
                return true;
            }
        }

        private enum ManipulationMode
        {
            Edit,
            Delete,
            Select
        }
    }
}

