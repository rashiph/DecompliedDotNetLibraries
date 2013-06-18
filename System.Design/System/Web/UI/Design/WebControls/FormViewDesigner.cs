namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Globalization;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    public class FormViewDesigner : DataBoundControlDesigner
    {
        private FormViewActionList _actionLists;
        private static DesignerAutoFormatCollection _autoFormats;
        private static string[] _controlTemplateNames = new string[] { "ItemTemplate", "FooterTemplate", "EditItemTemplate", "InsertItemTemplate", "HeaderTemplate", "EmptyDataTemplate", "PagerTemplate" };
        private static bool[] _controlTemplateSupportsDataBinding = new bool[] { true, true, true, true, true, true, true };
        private bool _enableDynamicData;
        private const string boolEditItemTemplateFieldString = "{0}: <asp:CheckBox Checked='<%# {1} %>' runat=\"server\" id=\"{2}CheckBox\" /><br />";
        private const string boolInsertItemTemplateFieldString = "{0}: <asp:CheckBox Checked='<%# {1} %>' runat=\"server\" id=\"{2}CheckBox\" /><br />";
        private const string boolItemTemplateFieldString = "{0}: <asp:CheckBox Checked='<%# {1} %>' runat=\"server\" id=\"{2}CheckBox\" Enabled=\"false\" /><br />";
        private const string dynamicDataInsertItemTemplateFieldString = "{0}: <asp:DynamicControl DataField=\"{0}\" runat=\"server\" id=\"{1}DynamicControl\" Mode=\"Insert\" ValidationGroup=\"Insert\" /><br />";
        private const string dynamicDataInsertTemplateButtonString = "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" ValidationGroup=\"Insert\" />";
        private const string dynamicDataItemTemplateFieldString = "{0}: <asp:DynamicControl DataField=\"{0}\" runat=\"server\" id=\"{2}DynamicControl\" Mode=\"{1}\" /><br />";
        private const string editItemTemplateFieldString = "{0}: <asp:TextBox Text='<%# {1} %>' runat=\"server\" id=\"{2}TextBox\" /><br />";
        private const int IDX_CONTROL_EDITITEM_TEMPLATE = 2;
        private const int IDX_CONTROL_EMPTY_DATA_TEMPLATE = 5;
        private const int IDX_CONTROL_FOOTER_TEMPLATE = 1;
        private const int IDX_CONTROL_HEADER_TEMPLATE = 4;
        private const int IDX_CONTROL_INSERTITEM_TEMPLATE = 3;
        private const int IDX_CONTROL_ITEM_TEMPLATE = 0;
        private const int IDX_CONTROL_PAGER_TEMPLATE = 6;
        private const string insertItemTemplateFieldString = "{0}: <asp:TextBox Text='<%# {1} %>' runat=\"server\" id=\"{2}TextBox\" /><br />";
        private const string itemTemplateFieldString = "{0}: <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label\" /><br />";
        private const string keyEditItemTemplateFieldString = "{0}: <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label1\" /><br />";
        private const string keyItemTemplateFieldString = "{0}: <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label\" /><br />";
        private const string nonBreakingSpace = "&nbsp;";
        private const string templateButtonString = "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" />";

        private void AddTemplatesAndKeys(IDataSourceViewSchema schema)
        {
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            StringBuilder builder3 = new StringBuilder();
            IDesignerHost service = (IDesignerHost) base.Component.Site.GetService(typeof(IDesignerHost));
            if (schema != null)
            {
                IDataSourceFieldSchema[] fields = schema.GetFields();
                if ((fields != null) && (fields.Length > 0))
                {
                    ArrayList list = new ArrayList();
                    foreach (IDataSourceFieldSchema schema2 in fields)
                    {
                        string name = schema2.Name;
                        char[] chArray = new char[name.Length];
                        for (int i = 0; i < name.Length; i++)
                        {
                            char c = name[i];
                            if (char.IsLetterOrDigit(c) || (c == '_'))
                            {
                                chArray[i] = c;
                            }
                            else
                            {
                                chArray[i] = '_';
                            }
                        }
                        string str2 = new string(chArray);
                        string str3 = DesignTimeDataBinding.CreateEvalExpression(name, string.Empty);
                        string str4 = DesignTimeDataBinding.CreateBindExpression(name, string.Empty);
                        if (schema2.PrimaryKey || schema2.Identity)
                        {
                            if (this.EnableDynamicData)
                            {
                                builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:DynamicControl DataField=\"{0}\" runat=\"server\" id=\"{2}DynamicControl\" Mode=\"{1}\" /><br />", new object[] { name, "ReadOnly", str2 }));
                                builder2.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:DynamicControl DataField=\"{0}\" runat=\"server\" id=\"{2}DynamicControl\" Mode=\"{1}\" /><br />", new object[] { name, "ReadOnly", str2 }));
                            }
                            else
                            {
                                builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label1\" /><br />", new object[] { name, str3, str2 }));
                                builder2.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label\" /><br />", new object[] { name, str3, str2 }));
                            }
                            if (!schema2.Identity)
                            {
                                if (this.EnableDynamicData)
                                {
                                    builder3.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:DynamicControl DataField=\"{0}\" runat=\"server\" id=\"{1}DynamicControl\" Mode=\"Insert\" ValidationGroup=\"Insert\" /><br />", new object[] { name, str2 }));
                                }
                                else
                                {
                                    builder3.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:TextBox Text='<%# {1} %>' runat=\"server\" id=\"{2}TextBox\" /><br />", new object[] { name, str4, str2 }));
                                }
                            }
                        }
                        else if (this.EnableDynamicData)
                        {
                            builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:DynamicControl DataField=\"{0}\" runat=\"server\" id=\"{2}DynamicControl\" Mode=\"{1}\" /><br />", new object[] { name, "Edit", str2 }));
                            builder2.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:DynamicControl DataField=\"{0}\" runat=\"server\" id=\"{2}DynamicControl\" Mode=\"{1}\" /><br />", new object[] { name, "ReadOnly", str2 }));
                            builder3.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:DynamicControl DataField=\"{0}\" runat=\"server\" id=\"{1}DynamicControl\" Mode=\"Insert\" ValidationGroup=\"Insert\" /><br />", new object[] { name, str2 }));
                        }
                        else if ((schema2.DataType == typeof(bool)) || (schema2.DataType == typeof(bool?)))
                        {
                            builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:CheckBox Checked='<%# {1} %>' runat=\"server\" id=\"{2}CheckBox\" /><br />", new object[] { name, str4, str2 }));
                            builder2.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:CheckBox Checked='<%# {1} %>' runat=\"server\" id=\"{2}CheckBox\" Enabled=\"false\" /><br />", new object[] { name, str4, str2 }));
                            builder3.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:CheckBox Checked='<%# {1} %>' runat=\"server\" id=\"{2}CheckBox\" /><br />", new object[] { name, str4, str2 }));
                        }
                        else
                        {
                            builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:TextBox Text='<%# {1} %>' runat=\"server\" id=\"{2}TextBox\" /><br />", new object[] { name, str4, str2 }));
                            builder2.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label\" /><br />", new object[] { name, str4, str2 }));
                            builder3.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:TextBox Text='<%# {1} %>' runat=\"server\" id=\"{2}TextBox\" /><br />", new object[] { name, str4, str2 }));
                        }
                        builder.Append(Environment.NewLine);
                        builder2.Append(Environment.NewLine);
                        builder3.Append(Environment.NewLine);
                        if (schema2.PrimaryKey)
                        {
                            list.Add(name);
                        }
                    }
                    bool flag = true;
                    if (base.DesignerView.CanUpdate)
                    {
                        builder2.Append(string.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" />", new object[] { "Edit", string.Empty, bool.FalseString, System.Design.SR.GetString("FormView_Edit") }));
                        flag = false;
                    }
                    if (this.EnableDynamicData)
                    {
                        builder.Append(string.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" ValidationGroup=\"Insert\" />", new object[] { "Update", string.Empty, bool.TrueString, System.Design.SR.GetString("FormView_Update") }));
                    }
                    else
                    {
                        builder.Append(string.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" />", new object[] { "Update", string.Empty, bool.TrueString, System.Design.SR.GetString("FormView_Update") }));
                    }
                    builder.Append("&nbsp;");
                    builder.Append(string.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" />", new object[] { "Cancel", "Update", bool.FalseString, System.Design.SR.GetString("FormView_Cancel") }));
                    if (base.DesignerView.CanDelete)
                    {
                        if (!flag)
                        {
                            builder2.Append("&nbsp;");
                        }
                        builder2.Append(string.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" />", new object[] { "Delete", string.Empty, bool.FalseString, System.Design.SR.GetString("FormView_Delete") }));
                        flag = false;
                    }
                    if (base.DesignerView.CanInsert)
                    {
                        if (!flag)
                        {
                            builder2.Append("&nbsp;");
                        }
                        builder2.Append(string.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" />", new object[] { "New", string.Empty, bool.FalseString, System.Design.SR.GetString("FormView_New") }));
                    }
                    if (this.EnableDynamicData)
                    {
                        builder3.Append(string.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" ValidationGroup=\"Insert\" />", new object[] { "Insert", string.Empty, bool.TrueString, System.Design.SR.GetString("FormView_Insert") }));
                    }
                    else
                    {
                        builder3.Append(string.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" />", new object[] { "Insert", string.Empty, bool.TrueString, System.Design.SR.GetString("FormView_Insert") }));
                    }
                    builder3.Append("&nbsp;");
                    builder3.Append(string.Format(CultureInfo.InvariantCulture, "<asp:LinkButton runat=\"server\" Text=\"{3}\" CommandName=\"{0}\" id=\"{1}{0}Button\" CausesValidation=\"{2}\" />", new object[] { "Cancel", "Insert", bool.FalseString, System.Design.SR.GetString("FormView_Cancel") }));
                    builder.Append(Environment.NewLine);
                    builder2.Append(Environment.NewLine);
                    builder3.Append(Environment.NewLine);
                    try
                    {
                        ((FormView) base.Component).EditItemTemplate = ControlParser.ParseTemplate(service, builder.ToString());
                        ((FormView) base.Component).ItemTemplate = ControlParser.ParseTemplate(service, builder2.ToString());
                        ((FormView) base.Component).InsertItemTemplate = ControlParser.ParseTemplate(service, builder3.ToString());
                    }
                    catch
                    {
                    }
                    int count = list.Count;
                    if (count > 0)
                    {
                        string[] array = new string[count];
                        list.CopyTo(array, 0);
                        ((FormView) base.Component).DataKeyNames = array;
                    }
                }
            }
        }

        private bool CheckDynamicDataAllowed()
        {
            IDesignerHost service = (IDesignerHost) base.Component.Site.GetService(typeof(IDesignerHost));
            WebFormsRootDesigner designer = service.GetDesigner(service.RootComponent) as WebFormsRootDesigner;
            return ((designer != null) && (designer.ReferenceManager.GetType("asp", "DynamicControl") != null));
        }

        private bool EnablePagingCallback(object context)
        {
            bool flag2 = !((FormView) base.Component).AllowPaging;
            if (context is bool)
            {
                flag2 = (bool) context;
            }
            TypeDescriptor.GetProperties(typeof(FormView))["AllowPaging"].SetValue(base.Component, flag2);
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
            FormView viewControl = (FormView) base.ViewControl;
            bool flag = false;
            string[] dataKeyNames = null;
            if (this.CurrentModeTemplateExists)
            {
                bool flag2 = false;
                IDataSourceViewSchema dataSourceSchema = this.GetDataSourceSchema();
                if (dataSourceSchema != null)
                {
                    IDataSourceFieldSchema[] fields = dataSourceSchema.GetFields();
                    if ((fields != null) && (fields.Length > 0))
                    {
                        flag2 = true;
                    }
                }
                try
                {
                    if (!flag2)
                    {
                        dataKeyNames = viewControl.DataKeyNames;
                        viewControl.DataKeyNames = new string[0];
                        flag = true;
                    }
                    TypeDescriptor.Refresh(base.Component);
                    return base.GetDesignTimeHtml();
                }
                finally
                {
                    if (flag)
                    {
                        viewControl.DataKeyNames = dataKeyNames;
                    }
                }
            }
            return this.GetEmptyDesignTimeHtml();
        }

        protected override string GetEmptyDesignTimeHtml()
        {
            return base.CreatePlaceHolderDesignTimeHtml(System.Design.SR.GetString("DataList_NoTemplatesInst"));
        }

        private Style GetTemplateStyle(int templateIndex)
        {
            Style style = new Style();
            style.CopyFrom(((FormView) base.ViewControl).ControlStyle);
            switch (templateIndex)
            {
                case 0:
                    style.CopyFrom(((FormView) base.ViewControl).RowStyle);
                    return style;

                case 1:
                    style.CopyFrom(((FormView) base.ViewControl).FooterStyle);
                    return style;

                case 2:
                    style.CopyFrom(((FormView) base.ViewControl).RowStyle);
                    style.CopyFrom(((FormView) base.ViewControl).EditRowStyle);
                    return style;

                case 3:
                    style.CopyFrom(((FormView) base.ViewControl).RowStyle);
                    style.CopyFrom(((FormView) base.ViewControl).InsertRowStyle);
                    return style;

                case 4:
                    style.CopyFrom(((FormView) base.ViewControl).HeaderStyle);
                    return style;

                case 5:
                    style.CopyFrom(((FormView) base.ViewControl).EmptyDataRowStyle);
                    return style;

                case 6:
                    style.CopyFrom(((FormView) base.ViewControl).PagerStyle);
                    return style;
            }
            return style;
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(FormView));
            base.Initialize(component);
            if (base.View != null)
            {
                base.View.SetFlags(ViewFlags.TemplateEditing, true);
            }
        }

        protected override void OnSchemaRefreshed()
        {
            if (!base.InTemplateMode)
            {
                ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.SchemaRefreshedCallback), null, System.Design.SR.GetString("DataControls_SchemaRefreshedTransaction"));
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            RenderOuterTableHelper.SetupRenderOuterTable(properties, base.Component, true, base.GetType());
        }

        private bool SchemaRefreshedCallback(object context)
        {
            IDataSourceViewSchema dataSourceSchema = this.GetDataSourceSchema();
            if ((base.DataSourceID.Length > 0) && (dataSourceSchema != null))
            {
                if (((((FormView) base.Component).DataKeyNames.Length > 0) || (((FormView) base.Component).ItemTemplate != null)) || (((FormView) base.Component).EditItemTemplate != null))
                {
                    if (DialogResult.Yes == UIServiceHelper.ShowMessage(base.Component.Site, System.Design.SR.GetString("FormView_SchemaRefreshedWarning"), System.Design.SR.GetString("FormView_SchemaRefreshedCaption", new object[] { ((FormView) base.Component).ID }), MessageBoxButtons.YesNo))
                    {
                        ((FormView) base.Component).DataKeyNames = new string[0];
                        this.AddTemplatesAndKeys(dataSourceSchema);
                    }
                }
                else
                {
                    this.AddTemplatesAndKeys(dataSourceSchema);
                }
            }
            else if ((((((FormView) base.Component).DataKeyNames.Length > 0) || (((FormView) base.Component).ItemTemplate != null)) || (((FormView) base.Component).EditItemTemplate != null)) && (DialogResult.Yes == UIServiceHelper.ShowMessage(base.Component.Site, System.Design.SR.GetString("FormView_SchemaRefreshedWarningNoDataSource"), System.Design.SR.GetString("FormView_SchemaRefreshedCaption", new object[] { ((FormView) base.Component).ID }), MessageBoxButtons.YesNo)))
            {
                ((FormView) base.Component).DataKeyNames = new string[0];
                ((FormView) base.Component).ItemTemplate = null;
                ((FormView) base.Component).InsertItemTemplate = null;
                ((FormView) base.Component).EditItemTemplate = null;
            }
            this.UpdateDesignTimeHtml();
            return true;
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                if (this._actionLists == null)
                {
                    this._actionLists = new FormViewActionList(this);
                }
                bool inTemplateMode = base.InTemplateMode;
                DesignerDataSourceView designerView = base.DesignerView;
                this._actionLists.AllowPaging = !inTemplateMode && (designerView != null);
                this._actionLists.AllowDynamicData = this.CheckDynamicDataAllowed();
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
                    _autoFormats = ControlDesigner.CreateAutoFormats(AutoFormatSchemes.FORMVIEW_SCHEME_NAMES, schemeName => new FormViewAutoFormat(schemeName, "<Schemes>\r\n        <xsd:schema id=\"Schemes\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n          <xsd:element name=\"Scheme\">\r\n            <xsd:complexType>\r\n              <xsd:all>\r\n                <xsd:element name=\"SchemeName\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"GridLines\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"CellPadding\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"CellSpacing\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"RowForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"RowBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"RowFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"EditRowForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"EditRowBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"EditRowFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FooterForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FooterBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FooterFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerAlign\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerButtons\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"RenderOuterTable\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n              </xsd:all>\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n          <xsd:element name=\"Schemes\" msdata:IsDataSet=\"true\">\r\n            <xsd:complexType>\r\n              <xsd:choice maxOccurs=\"unbounded\">\r\n                <xsd:element ref=\"Scheme\"/>\r\n              </xsd:choice>\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n        </xsd:schema>\r\n        <Scheme>\r\n          <SchemeName>FVScheme_Empty</SchemeName>\r\n          <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Consistent1</SchemeName>\r\n          <AltRowBackColor>White</AltRowBackColor>\r\n          <GridLines>0</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <ForeColor>#333333</ForeColor>\r\n          <RowForeColor>#333333</RowForeColor>\r\n          <RowBackColor>#FFFBD6</RowBackColor>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#990000</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>White</FooterForeColor>\r\n          <FooterBackColor>#990000</FooterBackColor>\r\n          <FooterFont>1</FooterFont>\r\n          <PagerForeColor>#333333</PagerForeColor>\r\n          <PagerBackColor>#FFCC66</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Consistent2</SchemeName>\r\n            <AltRowBackColor>White</AltRowBackColor>\r\n            <GridLines>0</GridLines>\r\n            <CellPadding>4</CellPadding>\r\n            <ForeColor>#333333</ForeColor>\r\n            <RowBackColor>#EFF3FB</RowBackColor>\r\n            <HeaderForeColor>White</HeaderForeColor>\r\n            <HeaderBackColor>#507CD1</HeaderBackColor>\r\n            <HeaderFont>1</HeaderFont>\r\n            <FooterForeColor>White</FooterForeColor>\r\n            <FooterBackColor>#507CD1</FooterBackColor>\r\n            <FooterFont>1</FooterFont>\r\n            <PagerForeColor>White</PagerForeColor>\r\n            <PagerBackColor>#2461BF</PagerBackColor>\r\n            <PagerAlign>2</PagerAlign>\r\n            <EditRowBackColor>#2461BF</EditRowBackColor>\r\n            <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Consistent3</SchemeName>\r\n            <AltRowBackColor>White</AltRowBackColor>\r\n            <GridLines>0</GridLines>\r\n            <CellPadding>4</CellPadding>\r\n            <ForeColor>#333333</ForeColor>\r\n            <RowBackColor>#E3EAEB</RowBackColor>\r\n            <HeaderForeColor>White</HeaderForeColor>\r\n            <HeaderBackColor>#1C5E55</HeaderBackColor>\r\n            <HeaderFont>1</HeaderFont>\r\n            <FooterForeColor>White</FooterForeColor>\r\n            <FooterBackColor>#1C5E55</FooterBackColor>\r\n            <FooterFont>1</FooterFont>\r\n            <PagerForeColor>White</PagerForeColor>\r\n            <PagerBackColor>#666666</PagerBackColor>\r\n            <PagerAlign>2</PagerAlign>\r\n            <EditRowBackColor>#7C6F57</EditRowBackColor>\r\n            <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>DVScheme_Consistent4</SchemeName>\r\n            <AltRowBackColor>White</AltRowBackColor>\r\n            <AltRowForeColor>#284775</AltRowForeColor>\r\n            <GridLines>0</GridLines>\r\n            <CellPadding>4</CellPadding>\r\n            <ForeColor>#333333</ForeColor>\r\n            <RowForeColor>#333333</RowForeColor>\r\n            <RowBackColor>#F7F6F3</RowBackColor>\r\n            <HeaderForeColor>White</HeaderForeColor>\r\n            <HeaderBackColor>#5D7B9D</HeaderBackColor>\r\n            <HeaderFont>1</HeaderFont>\r\n            <FooterForeColor>White</FooterForeColor>\r\n            <FooterBackColor>#5D7B9D</FooterBackColor>\r\n            <FooterFont>1</FooterFont>\r\n            <PagerForeColor>White</PagerForeColor>\r\n            <PagerBackColor>#284775</PagerBackColor>\r\n            <PagerAlign>2</PagerAlign>\r\n            <EditRowBackColor>#999999</EditRowBackColor>\r\n            <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>FVScheme_Colorful1</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#CC9966</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <RowForeColor>#330099</RowForeColor>\r\n          <RowBackColor>White</RowBackColor>\r\n          <EditRowForeColor>#663399</EditRowForeColor>\r\n          <EditRowBackColor>#FFCC66</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>#FFFFCC</HeaderForeColor>\r\n          <HeaderBackColor>#990000</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#330099</FooterForeColor>\r\n          <FooterBackColor>#FFFFCC</FooterBackColor>\r\n          <PagerForeColor>#330099</PagerForeColor>\r\n          <PagerBackColor>#FFFFCC</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>FVScheme_Colorful2</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#3366CC</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <RowForeColor>#003399</RowForeColor>\r\n          <RowBackColor>White</RowBackColor>\r\n          <EditRowForeColor>#CCFF99</EditRowForeColor>\r\n          <EditRowBackColor>#009999</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>#CCCCFF</HeaderForeColor>\r\n          <HeaderBackColor>#003399</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#003399</FooterForeColor>\r\n          <FooterBackColor>#99CCCC</FooterBackColor>\r\n          <PagerForeColor>#003399</PagerForeColor>\r\n          <PagerBackColor>#99CCCC</PagerBackColor>\r\n          <PagerAlign>1</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>FVScheme_Colorful3</SchemeName>\r\n          <BackColor>#DEBA84</BackColor>\r\n          <BorderColor>#DEBA84</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>2</CellSpacing>\r\n          <RowForeColor>#8C4510</RowForeColor>\r\n          <RowBackColor>#FFF7E7</RowBackColor>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#738A9C</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#A55129</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#8C4510</FooterForeColor>\r\n          <FooterBackColor>#F7DFB5</FooterBackColor>\r\n          <PagerForeColor>#8C4510</PagerForeColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>FVScheme_Colorful4</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#E7E7FF</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>1</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <RowForeColor>#4A3C8C</RowForeColor>\r\n          <RowBackColor>#E7E7FF</RowBackColor>\r\n          <EditRowForeColor>#F7F7F7</EditRowForeColor>\r\n          <EditRowBackColor>#738A9C</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>#F7F7F7</HeaderForeColor>\r\n          <HeaderBackColor>#4A3C8C</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#4A3C8C</FooterForeColor>\r\n          <FooterBackColor>#B5C7DE</FooterBackColor>\r\n          <PagerForeColor>#4A3C8C</PagerForeColor>\r\n          <PagerBackColor>#E7E7FF</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>FVScheme_Colorful5</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>LightGoldenRodYellow</BackColor>\r\n          <BorderColor>Tan</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <GridLines>0</GridLines>\r\n          <CellPadding>2</CellPadding>\r\n          <HeaderBackColor>Tan</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>Tan</FooterBackColor>\r\n          <EditRowBackColor>DarkSlateBlue</EditRowBackColor>\r\n          <EditRowForeColor>GhostWhite</EditRowForeColor>\r\n          <PagerBackColor>PaleGoldenrod</PagerBackColor>\r\n          <PagerForeColor>DarkSlateBlue</PagerForeColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>FVScheme_Professional1</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#999999</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>2</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <RowForeColor>Black</RowForeColor>\r\n          <RowBackColor>#EEEEEE</RowBackColor>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#008A8C</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#000084</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>Black</FooterForeColor>\r\n          <FooterBackColor>#CCCCCC</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#999999</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>FVScheme_Professional2</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#CCCCCC</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <RowForeColor>#000066</RowForeColor>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#669999</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#006699</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#000066</FooterForeColor>\r\n          <FooterBackColor>White</FooterBackColor>\r\n          <PagerForeColor>#000066</PagerForeColor>\r\n          <PagerBackColor>White</PagerBackColor>\r\n          <PagerAlign>1</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>FVScheme_Professional3</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>White</BorderColor>\r\n          <BorderWidth>2px</BorderWidth>\r\n          <BorderStyle>7</BorderStyle>\r\n          <GridLines>0</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>1</CellSpacing>\r\n          <RowForeColor>Black</RowForeColor>\r\n          <RowBackColor>#DEDFDE</RowBackColor>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#9471DE</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>#E7E7FF</HeaderForeColor>\r\n          <HeaderBackColor>#4A3C8C</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>Black</FooterForeColor>\r\n          <FooterBackColor>#C6C3C6</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#C6C3C6</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n          <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>FVScheme_Simple1</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#999999</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>4</BorderStyle>\r\n          <GridLines>2</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#000099</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>Black</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>#CCCCCC</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#999999</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>FVScheme_Simple2</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>#CCCCCC</BackColor>\r\n          <BorderColor>#999999</BorderColor>\r\n          <BorderWidth>3px</BorderWidth>\r\n          <BorderStyle>4</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>2</CellSpacing>\r\n          <RowBackColor>White</RowBackColor>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#000099</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>Black</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>#CCCCCC</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#CCCCCC</PagerBackColor>\r\n          <PagerAlign>1</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>FVScheme_Simple3</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#336666</BorderColor>\r\n          <BorderWidth>3px</BorderWidth>\r\n          <BorderStyle>5</BorderStyle>\r\n          <GridLines>1</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <RowForeColor>#333333</RowForeColor>\r\n          <RowBackColor>White</RowBackColor>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#339966</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#336666</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#333333</FooterForeColor>\r\n          <FooterBackColor>White</FooterBackColor>\r\n          <PagerForeColor>White</PagerForeColor>\r\n          <PagerBackColor>#336666</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>FVScheme_Classic1</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#CCCCCC</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>1</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#CC3333</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#333333</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>Black</FooterForeColor>\r\n          <FooterBackColor>#CCCC99</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>White</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n          <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>FVScheme_Classic2</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#DEDFDE</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>2</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <RowBackColor>#F7F7DE</RowBackColor>\r\n          <EditRowForeColor>White</EditRowForeColor>\r\n          <EditRowBackColor>#CE5D5A</EditRowBackColor>\r\n          <EditRowFont>1</EditRowFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#6B696B</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>#CCCC99</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#F7F7DE</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n          <PagerButtons>1</PagerButtons>\r\n          <RenderOuterTable>True</RenderOuterTable>\r\n        </Scheme>\r\n      </Schemes>"));
                }
                return _autoFormats;
            }
        }

        private bool CurrentModeTemplateExists
        {
            get
            {
                ITemplate itemTemplate = null;
                if (((FormView) base.ViewControl).CurrentMode == FormViewMode.ReadOnly)
                {
                    itemTemplate = ((FormView) base.ViewControl).ItemTemplate;
                }
                if (((FormView) base.ViewControl).CurrentMode == FormViewMode.Insert)
                {
                    itemTemplate = ((FormView) base.ViewControl).InsertItemTemplate;
                }
                if ((((FormView) base.ViewControl).CurrentMode == FormViewMode.Edit) || ((((FormView) base.ViewControl).CurrentMode == FormViewMode.Insert) && (itemTemplate == null)))
                {
                    itemTemplate = ((FormView) base.ViewControl).EditItemTemplate;
                }
                if (itemTemplate == null)
                {
                    return false;
                }
                IDesignerHost service = (IDesignerHost) base.ViewControl.Site.GetService(typeof(IDesignerHost));
                string str = ControlPersister.PersistTemplate(itemTemplate, service);
                return ((str != null) && (str.Length > 0));
            }
        }

        internal bool EnableDynamicData
        {
            get
            {
                return this._enableDynamicData;
            }
            set
            {
                this._enableDynamicData = value;
                if (this.GetDataSourceSchema() != null)
                {
                    this.OnSchemaRefreshed();
                }
            }
        }

        internal bool EnablePaging
        {
            get
            {
                return ((FormView) base.Component).AllowPaging;
            }
            set
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.EnablePagingCallback), value, System.Design.SR.GetString("FormView_EnablePagingTransaction"));
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        public bool RenderOuterTable
        {
            get
            {
                return ((FormView) base.Component).RenderOuterTable;
            }
            set
            {
                RenderOuterTableHelper.SetRenderOuterTable(value, this, true);
            }
        }

        protected override int SampleRowCount
        {
            get
            {
                return 2;
            }
        }

        public override TemplateGroupCollection TemplateGroups
        {
            get
            {
                TemplateGroupCollection templateGroups = base.TemplateGroups;
                for (int i = 0; i < _controlTemplateNames.Length; i++)
                {
                    string name = _controlTemplateNames[i];
                    TemplateGroup group = new TemplateGroup(_controlTemplateNames[i], this.GetTemplateStyle(i));
                    TemplateDefinition templateDefinition = new TemplateDefinition(this, name, base.Component, name) {
                        SupportsDataBinding = _controlTemplateSupportsDataBinding[i]
                    };
                    group.AddTemplateDefinition(templateDefinition);
                    templateGroups.Add(group);
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
    }
}

