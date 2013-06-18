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

    public class DetailsViewDesigner : DataBoundControlDesigner
    {
        private DetailsViewActionList _actionLists;
        private static DesignerAutoFormatCollection _autoFormats;
        private static string[] _controlTemplateNames = new string[] { "FooterTemplate", "HeaderTemplate", "EmptyDataTemplate", "PagerTemplate" };
        private static bool[] _controlTemplateSupportsDataBinding = new bool[] { true, true, true, true };
        private bool _currentDeleteState;
        private bool _currentEditState;
        private bool _currentInsertState;
        internal bool _ignoreSchemaRefreshedEvent;
        private static string[] _rowTemplateNames = new string[] { "ItemTemplate", "AlternatingItemTemplate", "EditItemTemplate", "InsertItemTemplate", "HeaderTemplate" };
        private static bool[] _rowTemplateSupportsDataBinding = new bool[] { true, true, true, true, false };
        private const int BASE_INDEX = 0x3e8;
        private const int IDX_CONTROL_EMPTY_DATA_TEMPLATE = 2;
        private const int IDX_CONTROL_FOOTER_TEMPLATE = 0;
        private const int IDX_CONTROL_HEADER_TEMPLATE = 1;
        private const int IDX_CONTROL_PAGER_TEMPLATE = 3;
        private const int IDX_ROW_ALTITEM_TEMPLATE = 1;
        private const int IDX_ROW_EDITITEM_TEMPLATE = 2;
        private const int IDX_ROW_HEADER_TEMPLATE = 4;
        private const int IDX_ROW_INSERTITEM_TEMPLATE = 3;
        private const int IDX_ROW_ITEM_TEMPLATE = 0;

        private void AddKeysAndBoundFields(IDataSourceViewSchema schema)
        {
            DataControlFieldCollection fields = ((DetailsView) base.Component).Fields;
            if (schema != null)
            {
                IDataSourceFieldSchema[] schemaArray = schema.GetFields();
                if ((schemaArray != null) && (schemaArray.Length > 0))
                {
                    ArrayList list = new ArrayList();
                    foreach (IDataSourceFieldSchema schema2 in schemaArray)
                    {
                        if (((DetailsView) base.Component).IsBindableType(schema2.DataType))
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
                            fields.Add(field);
                        }
                    }
                    ((DetailsView) base.Component).AutoGenerateRows = false;
                    int count = list.Count;
                    if (count > 0)
                    {
                        string[] array = new string[count];
                        list.CopyTo(array, 0);
                        ((DetailsView) base.Component).DataKeyNames = array;
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
                ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.AddNewFieldChangeCallback), null, System.Design.SR.GetString("DetailsView_AddNewFieldTransaction"));
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
            base.DataBind(dataBoundControl);
            DetailsView view = (DetailsView) dataBoundControl;
            Table table = view.Controls[0] as Table;
            int autoGeneratedRows = 0;
            int num2 = 1;
            int num3 = 1;
            int num4 = 0;
            if (view.AllowPaging)
            {
                if (view.PagerSettings.Position == PagerPosition.TopAndBottom)
                {
                    num4 = 2;
                }
                else
                {
                    num4 = 1;
                }
            }
            if (view.AutoGenerateRows)
            {
                int num5 = 0;
                if ((view.AutoGenerateInsertButton || view.AutoGenerateDeleteButton) || view.AutoGenerateEditButton)
                {
                    num5 = 1;
                }
                autoGeneratedRows = ((((table.Rows.Count - view.Fields.Count) - num5) - num2) - num3) - num4;
            }
            this.SetRegionAttributes(autoGeneratedRows);
        }

        internal void EditFields()
        {
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                this._ignoreSchemaRefreshedEvent = true;
                ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.EditFieldsChangeCallback), null, System.Design.SR.GetString("DetailsView_EditFieldsTransaction"));
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

        private bool EnableInsertingCallback(object context)
        {
            bool newState = !this._currentInsertState;
            if (context is bool)
            {
                newState = (bool) context;
            }
            this.SaveManipulationSetting(ManipulationMode.Insert, newState);
            return true;
        }

        private bool EnablePagingCallback(object context)
        {
            bool flag2 = !((DetailsView) base.Component).AllowPaging;
            if (context is bool)
            {
                flag2 = (bool) context;
            }
            TypeDescriptor.GetProperties(typeof(DetailsView))["AllowPaging"].SetValue(base.Component, flag2);
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
            DetailsView viewControl = (DetailsView) base.ViewControl;
            if (viewControl.Fields.Count == 0)
            {
                viewControl.AutoGenerateRows = true;
            }
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
                viewControl.DataKeyNames = new string[0];
            }
            TypeDescriptor.Refresh(base.Component);
            return base.GetDesignTimeHtml();
        }

        public override string GetDesignTimeHtml(DesignerRegionCollection regions)
        {
            string designTimeHtml = this.GetDesignTimeHtml();
            DetailsView viewControl = (DetailsView) base.ViewControl;
            int count = viewControl.Rows.Count;
            int selectedFieldIndex = this.SelectedFieldIndex;
            DetailsViewRowCollection rows = viewControl.Rows;
            for (int i = 0; i < viewControl.Fields.Count; i++)
            {
                string name = System.Design.SR.GetString("DetailsView_Field", new object[] { i.ToString(NumberFormatInfo.InvariantInfo) });
                string headerText = viewControl.Fields[i].HeaderText;
                if (headerText.Length == 0)
                {
                    name = name + " - " + headerText;
                }
                if (i < count)
                {
                    DetailsViewRow row = rows[i];
                    for (int j = 0; j < row.Cells.Count; j++)
                    {
                        TableCell cell1 = row.Cells[j];
                        if (j == 0)
                        {
                            DesignerRegion region = new DesignerRegion(this, name, true) {
                                UserData = i
                            };
                            if (i == selectedFieldIndex)
                            {
                                region.Highlight = true;
                            }
                            regions.Add(region);
                        }
                        else
                        {
                            DesignerRegion region2 = new DesignerRegion(this, i.ToString(NumberFormatInfo.InvariantInfo), false) {
                                UserData = -1
                            };
                            if (i == selectedFieldIndex)
                            {
                                region2.Highlight = true;
                            }
                            regions.Add(region2);
                        }
                    }
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
            style.CopyFrom(((DetailsView) base.ViewControl).ControlStyle);
            switch (templateIndex)
            {
                case 0:
                    style.CopyFrom(((DetailsView) base.ViewControl).FooterStyle);
                    return style;

                case 1:
                    style.CopyFrom(((DetailsView) base.ViewControl).HeaderStyle);
                    return style;

                case 2:
                    style.CopyFrom(((DetailsView) base.ViewControl).EmptyDataRowStyle);
                    return style;

                case 3:
                    style.CopyFrom(((DetailsView) base.ViewControl).PagerStyle);
                    return style;

                case 0x3e8:
                    style.CopyFrom(((DetailsView) base.ViewControl).RowStyle);
                    style.CopyFrom(templateField.ItemStyle);
                    return style;

                case 0x3e9:
                    style.CopyFrom(((DetailsView) base.ViewControl).RowStyle);
                    style.CopyFrom(((DetailsView) base.ViewControl).AlternatingRowStyle);
                    style.CopyFrom(templateField.ItemStyle);
                    return style;

                case 0x3ea:
                    style.CopyFrom(((DetailsView) base.ViewControl).RowStyle);
                    style.CopyFrom(((DetailsView) base.ViewControl).EditRowStyle);
                    style.CopyFrom(templateField.ItemStyle);
                    return style;

                case 0x3eb:
                    style.CopyFrom(((DetailsView) base.ViewControl).RowStyle);
                    style.CopyFrom(((DetailsView) base.ViewControl).InsertRowStyle);
                    style.CopyFrom(templateField.ItemStyle);
                    return style;

                case 0x3ec:
                    style.CopyFrom(((DetailsView) base.ViewControl).HeaderStyle);
                    style.CopyFrom(templateField.HeaderStyle);
                    return style;
            }
            return style;
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(DetailsView));
            base.Initialize(component);
            if (base.View != null)
            {
                base.View.SetFlags(ViewFlags.TemplateEditing, true);
            }
        }

        internal void MoveDown()
        {
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.MoveDownCallback), null, System.Design.SR.GetString("DetailsView_MoveDownTransaction"));
                this.UpdateDesignTimeHtml();
            }
            finally
            {
                Cursor.Current = current;
            }
        }

        private bool MoveDownCallback(object context)
        {
            DataControlFieldCollection fields = ((DetailsView) base.Component).Fields;
            int selectedFieldIndex = this.SelectedFieldIndex;
            if ((selectedFieldIndex >= 0) && (fields.Count > (selectedFieldIndex + 1)))
            {
                DataControlField field = fields[selectedFieldIndex];
                fields.RemoveAt(selectedFieldIndex);
                fields.Insert(selectedFieldIndex + 1, field);
                this.SelectedFieldIndex++;
                this.UpdateDesignTimeHtml();
                return true;
            }
            return false;
        }

        internal void MoveUp()
        {
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.MoveUpCallback), null, System.Design.SR.GetString("DetailsView_MoveUpTransaction"));
                this.UpdateDesignTimeHtml();
            }
            finally
            {
                Cursor.Current = current;
            }
        }

        private bool MoveUpCallback(object context)
        {
            DataControlFieldCollection fields = ((DetailsView) base.Component).Fields;
            int selectedFieldIndex = this.SelectedFieldIndex;
            if (selectedFieldIndex > 0)
            {
                DataControlField field = fields[selectedFieldIndex];
                fields.RemoveAt(selectedFieldIndex);
                fields.Insert(selectedFieldIndex - 1, field);
                this.SelectedFieldIndex--;
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

        protected override void OnSchemaRefreshed()
        {
            if (!base.InTemplateMode && !this._ignoreSchemaRefreshedEvent)
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.SchemaRefreshedCallback), null, System.Design.SR.GetString("DataControls_SchemaRefreshedTransaction"));
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
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["Fields"];
                properties["Fields"] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { BrowsableAttribute.No });
            }
        }

        private bool RemoveCallback(object context)
        {
            int selectedFieldIndex = this.SelectedFieldIndex;
            if (selectedFieldIndex < 0)
            {
                return false;
            }
            ((DetailsView) base.Component).Fields.RemoveAt(selectedFieldIndex);
            if (selectedFieldIndex == ((DetailsView) base.Component).Fields.Count)
            {
                this.SelectedFieldIndex--;
                this.UpdateDesignTimeHtml();
            }
            return true;
        }

        internal void RemoveField()
        {
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.RemoveCallback), null, System.Design.SR.GetString("DetailsView_RemoveFieldTransaction"));
                this.UpdateDesignTimeHtml();
            }
            finally
            {
                Cursor.Current = current;
            }
        }

        private void SaveManipulationSetting(ManipulationMode mode, bool newState)
        {
            DataControlFieldCollection fields = ((DetailsView) base.Component).Fields;
            bool flag = false;
            ArrayList list = new ArrayList();
            foreach (DataControlField field in fields)
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

                        case ManipulationMode.Insert:
                            field2.ShowInsertButton = newState;
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
                fields.Remove((DataControlField) obj2);
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

                    case ManipulationMode.Insert:
                        field3.ShowInsertButton = newState;
                        break;
                }
                fields.Add(field3);
            }
            if (!newState)
            {
                PropertyDescriptor descriptor;
                DetailsView component = (DetailsView) base.Component;
                switch (mode)
                {
                    case ManipulationMode.Edit:
                        descriptor = TypeDescriptor.GetProperties(typeof(DetailsView))["AutoGenerateEditButton"];
                        descriptor.SetValue(base.Component, newState);
                        return;

                    case ManipulationMode.Delete:
                        descriptor = TypeDescriptor.GetProperties(typeof(DetailsView))["AutoGenerateDeleteButton"];
                        descriptor.SetValue(base.Component, newState);
                        return;

                    case ManipulationMode.Insert:
                        TypeDescriptor.GetProperties(typeof(DetailsView))["AutoGenerateInsertButton"].SetValue(base.Component, newState);
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
                if ((((DetailsView) base.Component).Fields.Count > 0) || (((DetailsView) base.Component).DataKeyNames.Length > 0))
                {
                    if (DialogResult.Yes == UIServiceHelper.ShowMessage(base.Component.Site, System.Design.SR.GetString("DataBoundControl_SchemaRefreshedWarning", new object[] { System.Design.SR.GetString("DataBoundControl_DetailsView"), System.Design.SR.GetString("DataBoundControl_Row") }), System.Design.SR.GetString("DataBoundControl_SchemaRefreshedCaption", new object[] { ((DetailsView) base.Component).ID }), MessageBoxButtons.YesNo))
                    {
                        ((DetailsView) base.Component).DataKeyNames = new string[0];
                        ((DetailsView) base.Component).Fields.Clear();
                        this.SelectedFieldIndex = -1;
                        this.AddKeysAndBoundFields(dataSourceSchema);
                    }
                }
                else
                {
                    this.AddKeysAndBoundFields(dataSourceSchema);
                }
            }
            else if (((((DetailsView) base.Component).Fields.Count > 0) || (((DetailsView) base.Component).DataKeyNames.Length > 0)) && (DialogResult.Yes == UIServiceHelper.ShowMessage(base.Component.Site, System.Design.SR.GetString("DataBoundControl_SchemaRefreshedWarningNoDataSource", new object[] { System.Design.SR.GetString("DataBoundControl_DetailsView"), System.Design.SR.GetString("DataBoundControl_Row") }), System.Design.SR.GetString("DataBoundControl_SchemaRefreshedCaption", new object[] { ((DetailsView) base.Component).ID }), MessageBoxButtons.YesNo)))
            {
                ((DetailsView) base.Component).DataKeyNames = new string[0];
                ((DetailsView) base.Component).Fields.Clear();
                this.SelectedFieldIndex = -1;
            }
            return true;
        }

        public override void SetEditableDesignerRegionContent(EditableDesignerRegion region, string content)
        {
        }

        private void SetRegionAttributes(int autoGeneratedRows)
        {
            int num = 0;
            DetailsView viewControl = (DetailsView) base.ViewControl;
            Table table = viewControl.Controls[0] as Table;
            if (table != null)
            {
                int num2 = 0;
                if (viewControl.AllowPaging && (viewControl.PagerSettings.Position != PagerPosition.Bottom))
                {
                    num2 = 1;
                }
                int num3 = (autoGeneratedRows + 1) + num2;
                TableRowCollection rows = table.Rows;
                for (int i = num3; (i < (viewControl.Fields.Count + num3)) && (i < rows.Count); i++)
                {
                    TableRow row = rows[i];
                    foreach (TableCell cell in row.Cells)
                    {
                        cell.Attributes[DesignerRegion.DesignerRegionAttributeName] = num.ToString(NumberFormatInfo.InvariantInfo);
                        num++;
                    }
                }
            }
        }

        private void UpdateFieldsCurrentState()
        {
            this._currentInsertState = ((DetailsView) base.Component).AutoGenerateInsertButton;
            this._currentEditState = ((DetailsView) base.Component).AutoGenerateEditButton;
            this._currentDeleteState = ((DetailsView) base.Component).AutoGenerateDeleteButton;
            foreach (DataControlField field in ((DetailsView) base.Component).Fields)
            {
                CommandField field2 = field as CommandField;
                if (field2 != null)
                {
                    if (field2.ShowInsertButton)
                    {
                        this._currentInsertState = true;
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
                    this._actionLists = new DetailsViewActionList(this);
                }
                bool inTemplateMode = base.InTemplateMode;
                int selectedFieldIndex = this.SelectedFieldIndex;
                this.UpdateFieldsCurrentState();
                this._actionLists.AllowRemoveField = ((((DetailsView) base.Component).Fields.Count > 0) && (selectedFieldIndex >= 0)) && !inTemplateMode;
                this._actionLists.AllowMoveUp = ((((DetailsView) base.Component).Fields.Count > 0) && (selectedFieldIndex > 0)) && !inTemplateMode;
                this._actionLists.AllowMoveDown = (((((DetailsView) base.Component).Fields.Count > 0) && (selectedFieldIndex >= 0)) && (((DetailsView) base.Component).Fields.Count > (selectedFieldIndex + 1))) && !inTemplateMode;
                DesignerDataSourceView designerView = base.DesignerView;
                this._actionLists.AllowPaging = !inTemplateMode && (designerView != null);
                this._actionLists.AllowInserting = !inTemplateMode && ((designerView != null) && designerView.CanInsert);
                this._actionLists.AllowEditing = !inTemplateMode && ((designerView != null) && designerView.CanUpdate);
                this._actionLists.AllowDeleting = !inTemplateMode && ((designerView != null) && designerView.CanDelete);
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
                    _autoFormats = ControlDesigner.CreateAutoFormats(AutoFormatSchemes.DETAILSVIEW_SCHEME_NAMES, schemeName => new DetailsViewAutoFormat(schemeName, "<Schemes>\r\n        <xsd:schema id=\"Schemes\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n          <xsd:element name=\"Scheme\">\r\n            <xsd:complexType>\r\n              <xsd:all>\r\n                <xsd:element name=\"SchemeName\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"GridLines\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"CellPadding\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"CellSpacing\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"RowForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"RowBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"RowFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"AltRowForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"AltRowBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"AltRowFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"CommandRowForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"CommandRowBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"CommandRowFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FieldHeaderForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FieldHeaderBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FieldHeaderFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"EditRowForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"EditRowBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"EditRowFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FooterForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FooterBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FooterFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerAlign\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerButtons\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n              </xsd:all>\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n          <xsd:element name=\"Schemes\" msdata:IsDataSet=\"true\">\r\n            <xsd:complexType>\r\n              <xsd:choice maxOccurs=\"unbounded\">\r\n                <xsd:element ref=\"Scheme\"/>\r\n              </xsd:choice>\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n        </xsd:schema>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Empty</SchemeName>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Consistent1</SchemeName>\r\n          <AltRowBackColor>White</AltRowBackColor>\r\n          <GridLines>0</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <ForeColor>#333333</ForeColor>\r\n          <RowForeColor>#333333</RowForeColor>\r\n          <RowBackColor>#FFFBD6</RowBackColor>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#990000</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>White</FooterForeColor>\r\n          <FooterBackColor>#990000</FooterBackColor>\r\n          <FooterFont>1</FooterFont>\r\n          <PagerForeColor>#333333</PagerForeColor>\r\n          <PagerBackColor>#FFCC66</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <CommandRowBackColor>#FFFFC0</CommandRowBackColor>\r\n          <CommandRowFont>1</CommandRowFont>\r\n          <FieldHeaderFont>1</FieldHeaderFont>\r\n          <FieldHeaderBackColor>#FFFF99</FieldHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Consistent2</SchemeName>\r\n            <AltRowBackColor>White</AltRowBackColor>\r\n            <GridLines>0</GridLines>\r\n            <CellPadding>4</CellPadding>\r\n            <ForeColor>#333333</ForeColor>\r\n            <RowBackColor>#EFF3FB</RowBackColor>\r\n            <HeaderForeColor>White</HeaderForeColor>\r\n            <HeaderBackColor>#507CD1</HeaderBackColor>\r\n            <HeaderFont>1</HeaderFont>\r\n            <FooterForeColor>White</FooterForeColor>\r\n            <FooterBackColor>#507CD1</FooterBackColor>\r\n            <FooterFont>1</FooterFont>\r\n            <PagerForeColor>White</PagerForeColor>\r\n            <PagerBackColor>#2461BF</PagerBackColor>\r\n            <PagerAlign>2</PagerAlign>\r\n            <EditRowBackColor>#2461BF</EditRowBackColor>\r\n            <CommandRowBackColor>#D1DDF1</CommandRowBackColor>\r\n            <CommandRowFont>1</CommandRowFont>\r\n            <FieldHeaderFont>1</FieldHeaderFont>\r\n            <FieldHeaderBackColor>#DEE8F5</FieldHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Consistent3</SchemeName>\r\n            <AltRowBackColor>White</AltRowBackColor>\r\n            <GridLines>0</GridLines>\r\n            <CellPadding>4</CellPadding>\r\n            <ForeColor>#333333</ForeColor>\r\n            <RowBackColor>#E3EAEB</RowBackColor>\r\n            <HeaderForeColor>White</HeaderForeColor>\r\n            <HeaderBackColor>#1C5E55</HeaderBackColor>\r\n            <HeaderFont>1</HeaderFont>\r\n            <FooterForeColor>White</FooterForeColor>\r\n            <FooterBackColor>#1C5E55</FooterBackColor>\r\n            <FooterFont>1</FooterFont>\r\n            <PagerForeColor>White</PagerForeColor>\r\n            <PagerBackColor>#666666</PagerBackColor>\r\n            <PagerAlign>2</PagerAlign>\r\n            <EditRowBackColor>#7C6F57</EditRowBackColor>\r\n            <CommandRowBackColor>#C5BBAF</CommandRowBackColor>\r\n            <CommandRowFont>1</CommandRowFont>\r\n            <FieldHeaderFont>1</FieldHeaderFont>\r\n            <FieldHeaderBackColor>#D0D0D0</FieldHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Consistent4</SchemeName>\r\n            <AltRowBackColor>White</AltRowBackColor>\r\n            <AltRowForeColor>#284775</AltRowForeColor>\r\n            <GridLines>0</GridLines>\r\n            <CellPadding>4</CellPadding>\r\n            <ForeColor>#333333</ForeColor>\r\n            <RowForeColor>#333333</RowForeColor>\r\n            <RowBackColor>#F7F6F3</RowBackColor>\r\n            <HeaderForeColor>White</HeaderForeColor>\r\n            <HeaderBackColor>#5D7B9D</HeaderBackColor>\r\n            <HeaderFont>1</HeaderFont>\r\n            <FooterForeColor>White</FooterForeColor>\r\n            <FooterBackColor>#5D7B9D</FooterBackColor>\r\n            <FooterFont>1</FooterFont>\r\n            <PagerForeColor>White</PagerForeColor>\r\n            <PagerBackColor>#284775</PagerBackColor>\r\n            <PagerAlign>2</PagerAlign>\r\n            <EditRowBackColor>#999999</EditRowBackColor>\r\n            <CommandRowBackColor>#E2DED6</CommandRowBackColor>\r\n            <CommandRowFont>1</CommandRowFont>\r\n            <FieldHeaderFont>1</FieldHeaderFont>\r\n            <FieldHeaderBackColor>#E9ECF1</FieldHeaderBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Colorful1</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#CC9966</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <RowForeColor>#330099</RowForeColor>\r\n          <RowBackColor>White</RowBackColor>\r\n          <EditRowForeColor>#663399</EditRowForeColor>\r\n          <EditRowBackColor>#FFCC66</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>#FFFFCC</HeaderForeColor>\r\n          <HeaderBackColor>#990000</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#330099</FooterForeColor>\r\n          <FooterBackColor>#FFFFCC</FooterBackColor>\r\n          <PagerForeColor>#330099</PagerForeColor>\r\n          <PagerBackColor>#FFFFCC</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Colorful2</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#3366CC</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <RowForeColor>#003399</RowForeColor>\r\n          <RowBackColor>White</RowBackColor>\r\n          <EditRowForeColor>#CCFF99</EditRowForeColor>\r\n          <EditRowBackColor>#009999</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>#CCCCFF</HeaderForeColor>\r\n          <HeaderBackColor>#003399</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#003399</FooterForeColor>\r\n          <FooterBackColor>#99CCCC</FooterBackColor>\r\n          <PagerForeColor>#003399</PagerForeColor>\r\n          <PagerBackColor>#99CCCC</PagerBackColor>\r\n          <PagerAlign>1</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Colorful3</SchemeName>\r\n          <BackColor>#DEBA84</BackColor>\r\n          <BorderColor>#DEBA84</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>2</CellSpacing>\r\n          <RowForeColor>#8C4510</RowForeColor>\r\n          <RowBackColor>#FFF7E7</RowBackColor>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#738A9C</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#A55129</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#8C4510</FooterForeColor>\r\n          <FooterBackColor>#F7DFB5</FooterBackColor>\r\n          <PagerForeColor>#8C4510</PagerForeColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Colorful4</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#E7E7FF</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>1</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <RowForeColor>#4A3C8C</RowForeColor>\r\n          <RowBackColor>#E7E7FF</RowBackColor>\r\n          <AltRowBackColor>#F7F7F7</AltRowBackColor>\r\n          <EditRowForeColor>#F7F7F7</EditRowForeColor>\r\n          <EditRowBackColor>#738A9C</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>#F7F7F7</HeaderForeColor>\r\n          <HeaderBackColor>#4A3C8C</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#4A3C8C</FooterForeColor>\r\n          <FooterBackColor>#B5C7DE</FooterBackColor>\r\n          <PagerForeColor>#4A3C8C</PagerForeColor>\r\n          <PagerBackColor>#E7E7FF</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Colorful5</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>LightGoldenRodYellow</BackColor>\r\n          <BorderColor>Tan</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <GridLines>0</GridLines>\r\n          <CellPadding>2</CellPadding>\r\n          <AltRowBackColor>PaleGoldenRod</AltRowBackColor>\r\n          <HeaderBackColor>Tan</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>Tan</FooterBackColor>\r\n          <EditRowBackColor>DarkSlateBlue</EditRowBackColor>\r\n          <EditRowForeColor>GhostWhite</EditRowForeColor>\r\n          <PagerBackColor>PaleGoldenrod</PagerBackColor>\r\n          <PagerForeColor>DarkSlateBlue</PagerForeColor>\r\n          <PagerAlign>2</PagerAlign>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Professional1</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#999999</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>2</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <RowForeColor>Black</RowForeColor>\r\n          <RowBackColor>#EEEEEE</RowBackColor>\r\n          <AltRowBackColor>#DCDCDC</AltRowBackColor>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#008A8C</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#000084</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>Black</FooterForeColor>\r\n          <FooterBackColor>#CCCCCC</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#999999</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Professional2</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#CCCCCC</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <RowForeColor>#000066</RowForeColor>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#669999</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#006699</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#000066</FooterForeColor>\r\n          <FooterBackColor>White</FooterBackColor>\r\n          <PagerForeColor>#000066</PagerForeColor>\r\n          <PagerBackColor>White</PagerBackColor>\r\n          <PagerAlign>1</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Professional3</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>White</BorderColor>\r\n          <BorderWidth>2px</BorderWidth>\r\n          <BorderStyle>7</BorderStyle>\r\n          <GridLines>0</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>1</CellSpacing>\r\n          <RowForeColor>Black</RowForeColor>\r\n          <RowBackColor>#DEDFDE</RowBackColor>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#9471DE</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>#E7E7FF</HeaderForeColor>\r\n          <HeaderBackColor>#4A3C8C</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>Black</FooterForeColor>\r\n          <FooterBackColor>#C6C3C6</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#C6C3C6</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Simple1</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#999999</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>4</BorderStyle>\r\n          <GridLines>2</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <AltRowBackColor>#CCCCCC</AltRowBackColor>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#000099</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>Black</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>#CCCCCC</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#999999</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Simple2</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>#CCCCCC</BackColor>\r\n          <BorderColor>#999999</BorderColor>\r\n          <BorderWidth>3px</BorderWidth>\r\n          <BorderStyle>4</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>2</CellSpacing>\r\n          <RowBackColor>White</RowBackColor>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#000099</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>Black</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>#CCCCCC</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#CCCCCC</PagerBackColor>\r\n          <PagerAlign>1</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Simple3</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#336666</BorderColor>\r\n          <BorderWidth>3px</BorderWidth>\r\n          <BorderStyle>5</BorderStyle>\r\n          <GridLines>1</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <RowForeColor>#333333</RowForeColor>\r\n          <RowBackColor>White</RowBackColor>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#339966</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#336666</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#333333</FooterForeColor>\r\n          <FooterBackColor>White</FooterBackColor>\r\n          <PagerForeColor>White</PagerForeColor>\r\n          <PagerBackColor>#336666</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Classic1</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#CCCCCC</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>1</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#CC3333</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#333333</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>Black</FooterForeColor>\r\n          <FooterBackColor>#CCCC99</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>White</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Classic2</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#DEDFDE</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>2</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <RowBackColor>#F7F7DE</RowBackColor>\r\n          <AltRowBackColor>White</AltRowBackColor>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#CE5D5A</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#6B696B</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>#CCCC99</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#F7F7DE</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n        </Scheme>\r\n      </Schemes>"));
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
                    ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.EnableDeletingCallback), value, System.Design.SR.GetString("DetailsView_EnableDeletingTransaction"));
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
                    ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.EnableEditingCallback), value, System.Design.SR.GetString("DetailsView_EnableEditingTransaction"));
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        internal bool EnableInserting
        {
            get
            {
                return this._currentInsertState;
            }
            set
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.EnableInsertingCallback), value, System.Design.SR.GetString("DetailsView_EnableInsertingTransaction"));
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
                return ((DetailsView) base.Component).AllowPaging;
            }
            set
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.EnablePagingCallback), value, System.Design.SR.GetString("DetailsView_EnablePagingTransaction"));
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
                return 2;
            }
        }

        private int SelectedFieldIndex
        {
            get
            {
                object obj2 = base.DesignerState["SelectedFieldIndex"];
                int count = ((DetailsView) base.Component).Fields.Count;
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
                DataControlFieldCollection fields = ((DetailsView) base.Component).Fields;
                int count = fields.Count;
                if (count > 0)
                {
                    for (int j = 0; j < count; j++)
                    {
                        TemplateField templateField = fields[j] as TemplateField;
                        if (templateField != null)
                        {
                            string headerText = fields[j].HeaderText;
                            string groupName = System.Design.SR.GetString("DetailsView_Field", new object[] { j.ToString(NumberFormatInfo.InvariantInfo) });
                            if ((headerText != null) && (headerText.Length != 0))
                            {
                                groupName = groupName + " - " + headerText;
                            }
                            TemplateGroup group = new TemplateGroup(groupName);
                            for (int k = 0; k < _rowTemplateNames.Length; k++)
                            {
                                string name = _rowTemplateNames[k];
                                TemplateDefinition templateDefinition = new TemplateDefinition(this, name, fields[j], name, this.GetTemplateStyle(k + 0x3e8, templateField)) {
                                    SupportsDataBinding = _rowTemplateSupportsDataBinding[k]
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
                    TemplateGroup group2 = new TemplateGroup(_controlTemplateNames[i], this.GetTemplateStyle(i, null));
                    TemplateDefinition definition2 = new TemplateDefinition(this, str4, base.Component, str4) {
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
            Insert
        }
    }
}

