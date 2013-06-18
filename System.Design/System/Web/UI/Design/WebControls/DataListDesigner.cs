namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    [SupportsPreviewControl(true), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class DataListDesigner : BaseDataListDesigner
    {
        private static DesignerAutoFormatCollection _autoFormats;
        private const string breakString = "<br />";
        internal static TraceSwitch DataListDesignerSwitch = new TraceSwitch("DATALISTDESIGNER", "Enable DataList designer general purpose traces.");
        private static string[] HeaderFooterTemplateNames = new string[] { "HeaderTemplate", "FooterTemplate" };
        private const int HeaderFooterTemplates = 1;
        private const int IDX_ALTITEM_TEMPLATE = 1;
        private const int IDX_EDITITEM_TEMPLATE = 3;
        private const int IDX_FOOTER_TEMPLATE = 1;
        private const int IDX_HEADER_TEMPLATE = 0;
        private const int IDX_ITEM_TEMPLATE = 0;
        private const int IDX_SELITEM_TEMPLATE = 2;
        private const int IDX_SEPARATOR_TEMPLATE = 0;
        private static string[] ItemTemplateNames = new string[] { "ItemTemplate", "AlternatingItemTemplate", "SelectedItemTemplate", "EditItemTemplate" };
        private const int ItemTemplates = 0;
        private const int SeparatorTemplate = 2;
        private static string[] SeparatorTemplateNames = new string[] { "SeparatorTemplate" };
        private const string templateFieldString = "{0}: <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label\"/><br />";
        private TemplateEditingVerb[] templateVerbs;
        private bool templateVerbsDirty = true;

        private void CreateDefaultTemplate()
        {
            string text = string.Empty;
            StringBuilder builder = new StringBuilder();
            DataList component = (DataList) base.Component;
            IDataSourceViewSchema dataSourceSchema = this.GetDataSourceSchema();
            IDataSourceFieldSchema[] fields = null;
            if (dataSourceSchema != null)
            {
                fields = dataSourceSchema.GetFields();
            }
            if ((fields != null) && (fields.Length > 0))
            {
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
                    string str3 = new string(chArray);
                    builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}: <asp:Label Text='<%# {1} %>' runat=\"server\" id=\"{2}Label\"/><br />", new object[] { name, DesignTimeDataBinding.CreateEvalExpression(name, string.Empty), str3 }));
                    builder.Append(Environment.NewLine);
                    if (schema2.PrimaryKey && (component.DataKeyField.Length == 0))
                    {
                        component.DataKeyField = name;
                    }
                }
                builder.Append("<br />");
                builder.Append(Environment.NewLine);
                text = builder.ToString();
            }
            if ((text != null) && (text.Length > 0))
            {
                try
                {
                    component.ItemTemplate = base.GetTemplateFromText(text, component.ItemTemplate);
                }
                catch
                {
                }
            }
        }

        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        protected override ITemplateEditingFrame CreateTemplateEditingFrame(TemplateEditingVerb verb)
        {
            ITemplateEditingService service = (ITemplateEditingService) this.GetService(typeof(ITemplateEditingService));
            DataList viewControl = (DataList) base.ViewControl;
            string[] templateNames = null;
            Style[] templateStyles = null;
            switch (verb.Index)
            {
                case 0:
                    templateNames = ItemTemplateNames;
                    templateStyles = new Style[] { viewControl.ItemStyle, viewControl.AlternatingItemStyle, viewControl.SelectedItemStyle, viewControl.EditItemStyle };
                    break;

                case 1:
                    templateNames = HeaderFooterTemplateNames;
                    templateStyles = new Style[] { viewControl.HeaderStyle, viewControl.FooterStyle };
                    break;

                case 2:
                    templateNames = SeparatorTemplateNames;
                    templateStyles = new Style[] { viewControl.SeparatorStyle };
                    break;
            }
            return service.CreateFrame(this, verb.Text, templateNames, viewControl.ControlStyle, templateStyles);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DisposeTemplateVerbs();
            }
            base.Dispose(disposing);
        }

        private void DisposeTemplateVerbs()
        {
            if (this.templateVerbs != null)
            {
                for (int i = 0; i < this.templateVerbs.Length; i++)
                {
                    this.templateVerbs[i].Dispose();
                }
                this.templateVerbs = null;
                this.templateVerbsDirty = true;
            }
        }

        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        protected override TemplateEditingVerb[] GetCachedTemplateEditingVerbs()
        {
            if (this.templateVerbsDirty)
            {
                this.DisposeTemplateVerbs();
                this.templateVerbs = new TemplateEditingVerb[] { new TemplateEditingVerb(System.Design.SR.GetString("DataList_ItemTemplates"), 0, this), new TemplateEditingVerb(System.Design.SR.GetString("DataList_HeaderFooterTemplates"), 1, this), new TemplateEditingVerb(System.Design.SR.GetString("DataList_SeparatorTemplate"), 2, this) };
                this.templateVerbsDirty = false;
            }
            return this.templateVerbs;
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
            bool templatesExist = this.TemplatesExist;
            if (templatesExist)
            {
                IEnumerable designTimeDataSource;
                DataList viewControl = (DataList) base.ViewControl;
                bool dummyDataSource = false;
                DesignerDataSourceView designerView = base.DesignerView;
                if (designerView == null)
                {
                    designTimeDataSource = base.GetDesignTimeDataSource(5, out dummyDataSource);
                }
                else
                {
                    try
                    {
                        designTimeDataSource = designerView.GetDesignTimeData(5, out dummyDataSource);
                    }
                    catch (Exception exception)
                    {
                        if (base.Component.Site != null)
                        {
                            IComponentDesignerDebugService service = (IComponentDesignerDebugService) base.Component.Site.GetService(typeof(IComponentDesignerDebugService));
                            if (service != null)
                            {
                                service.Fail(System.Design.SR.GetString("DataSource_DebugService_FailedCall", new object[] { "DesignerDataSourceView.GetDesignTimeData", exception.Message }));
                            }
                        }
                        designTimeDataSource = null;
                    }
                }
                bool flag3 = false;
                string dataKeyField = null;
                bool flag4 = false;
                string dataSourceID = null;
                try
                {
                    viewControl.DataSource = designTimeDataSource;
                    dataKeyField = viewControl.DataKeyField;
                    if (dataKeyField.Length != 0)
                    {
                        flag3 = true;
                        viewControl.DataKeyField = string.Empty;
                    }
                    dataSourceID = viewControl.DataSourceID;
                    viewControl.DataSourceID = string.Empty;
                    flag4 = true;
                    viewControl.DataBind();
                    return base.GetDesignTimeHtml();
                }
                catch (Exception exception2)
                {
                    return this.GetErrorDesignTimeHtml(exception2);
                }
                finally
                {
                    viewControl.DataSource = null;
                    if (flag3)
                    {
                        viewControl.DataKeyField = dataKeyField;
                    }
                    if (flag4)
                    {
                        viewControl.DataSourceID = dataSourceID;
                    }
                }
            }
            return this.GetEmptyDesignTimeHtml();
        }

        protected override string GetEmptyDesignTimeHtml()
        {
            string str;
            if (base.CanEnterTemplateMode)
            {
                str = System.Design.SR.GetString("DataList_NoTemplatesInst");
            }
            else
            {
                str = System.Design.SR.GetString("DataList_NoTemplatesInst2");
            }
            return base.CreatePlaceHolderDesignTimeHtml(str);
        }

        protected override string GetErrorDesignTimeHtml(Exception e)
        {
            return base.CreatePlaceHolderDesignTimeHtml(System.Design.SR.GetString("Control_ErrorRendering"));
        }

        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        public override string GetTemplateContainerDataItemProperty(string templateName)
        {
            return "DataItem";
        }

        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        public override string GetTemplateContent(ITemplateEditingFrame editingFrame, string templateName, out bool allowEditing)
        {
            allowEditing = true;
            DataList component = (DataList) base.Component;
            ITemplate itemTemplate = null;
            string textFromTemplate = string.Empty;
            switch (editingFrame.Verb.Index)
            {
                case 0:
                    if (!templateName.Equals(ItemTemplateNames[0]))
                    {
                        if (templateName.Equals(ItemTemplateNames[1]))
                        {
                            itemTemplate = component.AlternatingItemTemplate;
                        }
                        else if (templateName.Equals(ItemTemplateNames[2]))
                        {
                            itemTemplate = component.SelectedItemTemplate;
                        }
                        else if (templateName.Equals(ItemTemplateNames[3]))
                        {
                            itemTemplate = component.EditItemTemplate;
                        }
                        break;
                    }
                    itemTemplate = component.ItemTemplate;
                    break;

                case 1:
                    if (!templateName.Equals(HeaderFooterTemplateNames[0]))
                    {
                        if (templateName.Equals(HeaderFooterTemplateNames[1]))
                        {
                            itemTemplate = component.FooterTemplate;
                        }
                        break;
                    }
                    itemTemplate = component.HeaderTemplate;
                    break;

                case 2:
                    itemTemplate = component.SeparatorTemplate;
                    break;
            }
            if (itemTemplate != null)
            {
                textFromTemplate = base.GetTextFromTemplate(itemTemplate);
            }
            return textFromTemplate;
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(DataList));
            base.Initialize(component);
        }

        protected override void OnSchemaRefreshed()
        {
            if (!base.InTemplateModeInternal)
            {
                ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.RefreshSchemaCallback), null, System.Design.SR.GetString("DataList_RefreshSchemaTransaction"));
            }
        }

        protected override void OnTemplateEditingVerbsChanged()
        {
            this.templateVerbsDirty = true;
        }

        private bool RefreshSchemaCallback(object context)
        {
            DataList component = (DataList) base.Component;
            bool flag = (((component.ItemTemplate == null) && (component.EditItemTemplate == null)) && (component.AlternatingItemTemplate == null)) && (component.SelectedItemTemplate == null);
            IDataSourceViewSchema dataSourceSchema = this.GetDataSourceSchema();
            if ((base.DataSourceID.Length > 0) && (dataSourceSchema != null))
            {
                if (flag || (!flag && (DialogResult.Yes == UIServiceHelper.ShowMessage(base.Component.Site, System.Design.SR.GetString("DataList_RegenerateTemplates"), System.Design.SR.GetString("DataList_ClearTemplatesCaption"), MessageBoxButtons.YesNo))))
                {
                    component.ItemTemplate = null;
                    component.EditItemTemplate = null;
                    component.AlternatingItemTemplate = null;
                    component.SelectedItemTemplate = null;
                    component.DataKeyField = string.Empty;
                    this.CreateDefaultTemplate();
                    this.UpdateDesignTimeHtml();
                }
            }
            else if (flag || (!flag && (DialogResult.Yes == UIServiceHelper.ShowMessage(base.Component.Site, System.Design.SR.GetString("DataList_ClearTemplates"), System.Design.SR.GetString("DataList_ClearTemplatesCaption"), MessageBoxButtons.YesNo))))
            {
                component.ItemTemplate = null;
                component.EditItemTemplate = null;
                component.AlternatingItemTemplate = null;
                component.SelectedItemTemplate = null;
                component.DataKeyField = string.Empty;
                this.UpdateDesignTimeHtml();
            }
            return true;
        }

        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        public override void SetTemplateContent(ITemplateEditingFrame editingFrame, string templateName, string templateContent)
        {
            ITemplate templateFromText = null;
            DataList component = (DataList) base.Component;
            if ((templateContent != null) && (templateContent.Length != 0))
            {
                ITemplate currentTemplate = null;
                switch (editingFrame.Verb.Index)
                {
                    case 0:
                        if (!templateName.Equals(ItemTemplateNames[0]))
                        {
                            if (templateName.Equals(ItemTemplateNames[1]))
                            {
                                currentTemplate = component.AlternatingItemTemplate;
                            }
                            else if (templateName.Equals(ItemTemplateNames[2]))
                            {
                                currentTemplate = component.SelectedItemTemplate;
                            }
                            else if (templateName.Equals(ItemTemplateNames[3]))
                            {
                                currentTemplate = component.EditItemTemplate;
                            }
                            break;
                        }
                        currentTemplate = component.ItemTemplate;
                        break;

                    case 1:
                        if (!templateName.Equals(HeaderFooterTemplateNames[0]))
                        {
                            if (templateName.Equals(HeaderFooterTemplateNames[1]))
                            {
                                currentTemplate = component.FooterTemplate;
                            }
                            break;
                        }
                        currentTemplate = component.HeaderTemplate;
                        break;

                    case 2:
                        currentTemplate = component.SeparatorTemplate;
                        break;
                }
                templateFromText = base.GetTemplateFromText(templateContent, currentTemplate);
            }
            switch (editingFrame.Verb.Index)
            {
                case 0:
                    if (!templateName.Equals(ItemTemplateNames[0]))
                    {
                        if (templateName.Equals(ItemTemplateNames[1]))
                        {
                            component.AlternatingItemTemplate = templateFromText;
                            return;
                        }
                        if (templateName.Equals(ItemTemplateNames[2]))
                        {
                            component.SelectedItemTemplate = templateFromText;
                            return;
                        }
                        if (!templateName.Equals(ItemTemplateNames[3]))
                        {
                            break;
                        }
                        component.EditItemTemplate = templateFromText;
                        return;
                    }
                    component.ItemTemplate = templateFromText;
                    return;

                case 1:
                    if (!templateName.Equals(HeaderFooterTemplateNames[0]))
                    {
                        if (!templateName.Equals(HeaderFooterTemplateNames[1]))
                        {
                            break;
                        }
                        component.FooterTemplate = templateFromText;
                        return;
                    }
                    component.HeaderTemplate = templateFromText;
                    return;

                case 2:
                    component.SeparatorTemplate = templateFromText;
                    break;

                default:
                    return;
            }
        }

        public override bool AllowResize
        {
            get
            {
                if (!this.TemplatesExist)
                {
                    return base.InTemplateModeInternal;
                }
                return true;
            }
        }

        public override DesignerAutoFormatCollection AutoFormats
        {
            get
            {
                if (_autoFormats == null)
                {
                    _autoFormats = ControlDesigner.CreateAutoFormats(AutoFormatSchemes.BDL_SCHEME_NAMES, schemeName => new DataListAutoFormat(schemeName, "<Schemes>\r\n        <xsd:schema id=\"Schemes\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n          <xsd:element name=\"Scheme\">\r\n            <xsd:complexType>\r\n              <xsd:all>\r\n                <xsd:element name=\"SchemeName\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"BorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"GridLines\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"CellPadding\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"CellSpacing\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"ItemForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"ItemBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"ItemFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"AltItemForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"AltItemBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"AltItemFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SelItemForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SelItemBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"SelItemFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"HeaderFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FooterForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FooterBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FooterFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerAlign\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PagerMode\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"EditItemForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"EditItemBackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"EditItemFont\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n              </xsd:all>\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n          <xsd:element name=\"Schemes\" msdata:IsDataSet=\"true\">\r\n            <xsd:complexType>\r\n              <xsd:choice maxOccurs=\"unbounded\">\r\n                <xsd:element ref=\"Scheme\"/>\r\n              </xsd:choice>\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n        </xsd:schema>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Empty</SchemeName>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Consistent1</SchemeName>\r\n          <AltItemBackColor>White</AltItemBackColor>\r\n          <GridLines>0</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <ForeColor>#333333</ForeColor>\r\n          <ItemForeColor>#333333</ItemForeColor>\r\n          <ItemBackColor>#FFFBD6</ItemBackColor>\r\n          <SelItemForeColor>Navy</SelItemForeColor>\r\n          <SelItemBackColor>#FFCC66</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#990000</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>White</FooterForeColor>\r\n          <FooterBackColor>#990000</FooterBackColor>\r\n          <FooterFont>1</FooterFont>\r\n          <PagerForeColor>#333333</PagerForeColor>\r\n          <PagerBackColor>#FFCC66</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Consistent2</SchemeName>\r\n            <AltItemBackColor>White</AltItemBackColor>\r\n            <GridLines>0</GridLines>\r\n            <CellPadding>4</CellPadding>\r\n            <ForeColor>#333333</ForeColor>\r\n            <ItemBackColor>#EFF3FB</ItemBackColor>\r\n            <SelItemForeColor>#333333</SelItemForeColor>\r\n            <SelItemBackColor>#D1DDF1</SelItemBackColor>\r\n            <SelItemFont>1</SelItemFont>\r\n            <HeaderForeColor>White</HeaderForeColor>\r\n            <HeaderBackColor>#507CD1</HeaderBackColor>\r\n            <HeaderFont>1</HeaderFont>\r\n            <FooterForeColor>White</FooterForeColor>\r\n            <FooterBackColor>#507CD1</FooterBackColor>\r\n            <FooterFont>1</FooterFont>\r\n            <PagerForeColor>White</PagerForeColor>\r\n            <PagerBackColor>#2461BF</PagerBackColor>\r\n            <PagerAlign>2</PagerAlign>\r\n            <EditItemBackColor>#2461BF</EditItemBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Consistent3</SchemeName>\r\n            <AltItemBackColor>White</AltItemBackColor>\r\n            <GridLines>0</GridLines>\r\n            <CellPadding>4</CellPadding>\r\n            <ForeColor>#333333</ForeColor>\r\n            <ItemBackColor>#E3EAEB</ItemBackColor>\r\n            <SelItemForeColor>#333333</SelItemForeColor>\r\n            <SelItemBackColor>#C5BBAF</SelItemBackColor>\r\n            <SelItemFont>1</SelItemFont>\r\n            <HeaderForeColor>White</HeaderForeColor>\r\n            <HeaderBackColor>#1C5E55</HeaderBackColor>\r\n            <HeaderFont>1</HeaderFont>\r\n            <FooterForeColor>White</FooterForeColor>\r\n            <FooterBackColor>#1C5E55</FooterBackColor>\r\n            <FooterFont>1</FooterFont>\r\n            <PagerForeColor>White</PagerForeColor>\r\n            <PagerBackColor>#666666</PagerBackColor>\r\n            <PagerAlign>2</PagerAlign>\r\n            <EditItemBackColor>#7C6F57</EditItemBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Consistent4</SchemeName>\r\n            <AltItemBackColor>White</AltItemBackColor>\r\n            <AltItemForeColor>#284775</AltItemForeColor>\r\n            <GridLines>0</GridLines>\r\n            <CellPadding>4</CellPadding>\r\n            <ForeColor>#333333</ForeColor>\r\n            <ItemForeColor>#333333</ItemForeColor>\r\n            <ItemBackColor>#F7F6F3</ItemBackColor>\r\n            <SelItemForeColor>#333333</SelItemForeColor>\r\n            <SelItemBackColor>#E2DED6</SelItemBackColor>\r\n            <SelItemFont>1</SelItemFont>\r\n            <HeaderForeColor>White</HeaderForeColor>\r\n            <HeaderBackColor>#5D7B9D</HeaderBackColor>\r\n            <HeaderFont>1</HeaderFont>\r\n            <FooterForeColor>White</FooterForeColor>\r\n            <FooterBackColor>#5D7B9D</FooterBackColor>\r\n            <FooterFont>1</FooterFont>\r\n            <PagerForeColor>White</PagerForeColor>\r\n            <PagerBackColor>#284775</PagerBackColor>\r\n            <PagerAlign>2</PagerAlign>\r\n            <EditItemBackColor>#999999</EditItemBackColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Colorful1</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#CC9966</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <ItemForeColor>#330099</ItemForeColor>\r\n          <ItemBackColor>White</ItemBackColor>\r\n          <SelItemForeColor>#663399</SelItemForeColor>\r\n          <SelItemBackColor>#FFCC66</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>#FFFFCC</HeaderForeColor>\r\n          <HeaderBackColor>#990000</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#330099</FooterForeColor>\r\n          <FooterBackColor>#FFFFCC</FooterBackColor>\r\n          <PagerForeColor>#330099</PagerForeColor>\r\n          <PagerBackColor>#FFFFCC</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Colorful2</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#3366CC</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <ItemForeColor>#003399</ItemForeColor>\r\n          <ItemBackColor>White</ItemBackColor>\r\n          <SelItemForeColor>#CCFF99</SelItemForeColor>\r\n          <SelItemBackColor>#009999</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>#CCCCFF</HeaderForeColor>\r\n          <HeaderBackColor>#003399</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#003399</FooterForeColor>\r\n          <FooterBackColor>#99CCCC</FooterBackColor>\r\n          <PagerForeColor>#003399</PagerForeColor>\r\n          <PagerBackColor>#99CCCC</PagerBackColor>\r\n          <PagerAlign>1</PagerAlign>\r\n          <PagerMode>1</PagerMode>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Colorful3</SchemeName>\r\n          <BackColor>#DEBA84</BackColor>\r\n          <BorderColor>#DEBA84</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>2</CellSpacing>\r\n          <ItemForeColor>#8C4510</ItemForeColor>\r\n          <ItemBackColor>#FFF7E7</ItemBackColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#738A9C</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#A55129</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#8C4510</FooterForeColor>\r\n          <FooterBackColor>#F7DFB5</FooterBackColor>\r\n          <PagerForeColor>#8C4510</PagerForeColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <PagerMode>1</PagerMode>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Colorful4</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#E7E7FF</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>1</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <ItemForeColor>#4A3C8C</ItemForeColor>\r\n          <ItemBackColor>#E7E7FF</ItemBackColor>\r\n          <AltItemBackColor>#F7F7F7</AltItemBackColor>\r\n          <SelItemForeColor>#F7F7F7</SelItemForeColor>\r\n          <SelItemBackColor>#738A9C</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>#F7F7F7</HeaderForeColor>\r\n          <HeaderBackColor>#4A3C8C</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#4A3C8C</FooterForeColor>\r\n          <FooterBackColor>#B5C7DE</FooterBackColor>\r\n          <PagerForeColor>#4A3C8C</PagerForeColor>\r\n          <PagerBackColor>#E7E7FF</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n          <PagerMode>1</PagerMode>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Colorful5</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>LightGoldenRodYellow</BackColor>\r\n          <BorderColor>Tan</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <GridLines>0</GridLines>\r\n          <CellPadding>2</CellPadding>\r\n          <AltItemBackColor>PaleGoldenRod</AltItemBackColor>\r\n          <HeaderBackColor>Tan</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>Tan</FooterBackColor>\r\n          <SelItemBackColor>DarkSlateBlue</SelItemBackColor>\r\n          <SelItemForeColor>GhostWhite</SelItemForeColor>\r\n          <PagerBackColor>PaleGoldenrod</PagerBackColor>\r\n          <PagerForeColor>DarkSlateBlue</PagerForeColor>\r\n          <PagerAlign>2</PagerAlign>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Professional1</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#999999</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>2</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <ItemForeColor>Black</ItemForeColor>\r\n          <ItemBackColor>#EEEEEE</ItemBackColor>\r\n          <AltItemBackColor>#DCDCDC</AltItemBackColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#008A8C</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#000084</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>Black</FooterForeColor>\r\n          <FooterBackColor>#CCCCCC</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#999999</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <PagerMode>1</PagerMode>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Professional2</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#CCCCCC</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <ItemForeColor>#000066</ItemForeColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#669999</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#006699</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#000066</FooterForeColor>\r\n          <FooterBackColor>White</FooterBackColor>\r\n          <PagerForeColor>#000066</PagerForeColor>\r\n          <PagerBackColor>White</PagerBackColor>\r\n          <PagerAlign>1</PagerAlign>\r\n          <PagerMode>1</PagerMode>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Professional3</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>White</BorderColor>\r\n          <BorderWidth>2px</BorderWidth>\r\n          <BorderStyle>7</BorderStyle>\r\n          <GridLines>0</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>1</CellSpacing>\r\n          <ItemForeColor>Black</ItemForeColor>\r\n          <ItemBackColor>#DEDFDE</ItemBackColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#9471DE</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>#E7E7FF</HeaderForeColor>\r\n          <HeaderBackColor>#4A3C8C</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>Black</FooterForeColor>\r\n          <FooterBackColor>#C6C3C6</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#C6C3C6</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Simple1</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#999999</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>4</BorderStyle>\r\n          <GridLines>2</GridLines>\r\n          <CellPadding>3</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <AltItemBackColor>#CCCCCC</AltItemBackColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#000099</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>Black</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>#CCCCCC</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#999999</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Simple2</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>#CCCCCC</BackColor>\r\n          <BorderColor>#999999</BorderColor>\r\n          <BorderWidth>3px</BorderWidth>\r\n          <BorderStyle>4</BorderStyle>\r\n          <GridLines>3</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>2</CellSpacing>\r\n          <ItemBackColor>White</ItemBackColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#000099</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>Black</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>#CCCCCC</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#CCCCCC</PagerBackColor>\r\n          <PagerAlign>1</PagerAlign>\r\n          <PagerMode>1</PagerMode>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Simple3</SchemeName>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#336666</BorderColor>\r\n          <BorderWidth>3px</BorderWidth>\r\n          <BorderStyle>5</BorderStyle>\r\n          <GridLines>1</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <ItemForeColor>#333333</ItemForeColor>\r\n          <ItemBackColor>White</ItemBackColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#339966</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#336666</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>#333333</FooterForeColor>\r\n          <FooterBackColor>White</FooterBackColor>\r\n          <PagerForeColor>White</PagerForeColor>\r\n          <PagerBackColor>#336666</PagerBackColor>\r\n          <PagerAlign>2</PagerAlign>\r\n          <PagerMode>1</PagerMode>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Classic1</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#CCCCCC</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>1</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#CC3333</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#333333</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterForeColor>Black</FooterForeColor>\r\n          <FooterBackColor>#CCCC99</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>White</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>BDLScheme_Classic2</SchemeName>\r\n          <ForeColor>Black</ForeColor>\r\n          <BackColor>White</BackColor>\r\n          <BorderColor>#DEDFDE</BorderColor>\r\n          <BorderWidth>1px</BorderWidth>\r\n          <BorderStyle>1</BorderStyle>\r\n          <GridLines>2</GridLines>\r\n          <CellPadding>4</CellPadding>\r\n          <CellSpacing>0</CellSpacing>\r\n          <ItemBackColor>#F7F7DE</ItemBackColor>\r\n          <AltItemBackColor>White</AltItemBackColor>\r\n          <SelItemForeColor>White</SelItemForeColor>\r\n          <SelItemBackColor>#CE5D5A</SelItemBackColor>\r\n          <SelItemFont>1</SelItemFont>\r\n          <HeaderForeColor>White</HeaderForeColor>\r\n          <HeaderBackColor>#6B696B</HeaderBackColor>\r\n          <HeaderFont>1</HeaderFont>\r\n          <FooterBackColor>#CCCC99</FooterBackColor>\r\n          <PagerForeColor>Black</PagerForeColor>\r\n          <PagerBackColor>#F7F7DE</PagerBackColor>\r\n          <PagerAlign>3</PagerAlign>\r\n          <PagerMode>1</PagerMode>\r\n        </Scheme>\r\n      </Schemes>"));
                }
                return _autoFormats;
            }
        }

        protected bool TemplatesExist
        {
            get
            {
                DataList viewControl = (DataList) base.ViewControl;
                ITemplate itemTemplate = viewControl.ItemTemplate;
                string textFromTemplate = null;
                if (itemTemplate == null)
                {
                    return false;
                }
                textFromTemplate = base.GetTextFromTemplate(itemTemplate);
                return ((textFromTemplate != null) && (textFromTemplate.Length > 0));
            }
        }
    }
}

