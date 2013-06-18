namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    public class MenuDesigner : HierarchicalDataBoundControlDesigner, IDataBindingSchemaProvider
    {
        private static DesignerAutoFormatCollection _autoFormats;
        private ViewType _currentView;
        private const string _getDesignTimeDynamicHtml = "GetDesignTimeDynamicHtml";
        private const string _getDesignTimeStaticHtml = "GetDesignTimeStaticHtml";
        private const int _maxDesignDepth = 10;
        private System.Web.UI.WebControls.Menu _menu;
        private TemplateGroupCollection _templateGroups;
        private static readonly string[] _templateNames = new string[] { "StaticItemTemplate", "DynamicItemTemplate" };
        private const string emptyDesignTimeHtml = "\r\n                <table cellpadding=4 cellspacing=0 style=\"font-family:Tahoma;font-size:8pt;color:buttontext;background-color:buttonface\">\r\n                  <tr><td><span style=\"font-weight:bold\">Menu</span> - {0}</td></tr>\r\n                  <tr><td>{1}</td></tr>\r\n                </table>\r\n             ";
        private const string errorDesignTimeHtml = "\r\n                <table cellpadding=4 cellspacing=0 style=\"font-family:Tahoma;font-size:8pt;color:buttontext;background-color:buttonface;border: solid 1px;border-top-color:buttonhighlight;border-left-color:buttonhighlight;border-bottom-color:buttonshadow;border-right-color:buttonshadow\">\r\n                  <tr><td><span style=\"font-weight:bold\">Menu</span> - {0}</td></tr>\r\n                  <tr><td>{1}</td></tr>\r\n                </table>\r\n             ";

        private void ConvertToDynamicTemplate()
        {
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ConvertToDynamicTemplateChangeCallback), null, System.Design.SR.GetString("MenuDesigner_ConvertToDynamicTemplate"));
        }

        private bool ConvertToDynamicTemplateChangeCallback(object context)
        {
            string templateText = null;
            string dynamicItemFormatString = this._menu.DynamicItemFormatString;
            if ((dynamicItemFormatString != null) && (dynamicItemFormatString.Length != 0))
            {
                templateText = "<%# Eval(\"Text\", \"" + dynamicItemFormatString + "\") %>";
            }
            else
            {
                templateText = "<%# Eval(\"Text\") %>";
            }
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                this._menu.DynamicItemTemplate = ControlParser.ParseTemplate(service, templateText);
            }
            return true;
        }

        private void ConvertToStaticTemplate()
        {
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ConvertToStaticTemplateChangeCallback), null, System.Design.SR.GetString("MenuDesigner_ConvertToStaticTemplate"));
        }

        private bool ConvertToStaticTemplateChangeCallback(object context)
        {
            string templateText = null;
            string staticItemFormatString = this._menu.StaticItemFormatString;
            if ((staticItemFormatString != null) && (staticItemFormatString.Length != 0))
            {
                templateText = "<%# Eval(\"Text\", \"" + staticItemFormatString + "\") %>";
            }
            else
            {
                templateText = "<%# Eval(\"Text\") %>";
            }
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                this._menu.StaticItemTemplate = ControlParser.ParseTemplate(service, templateText);
            }
            return true;
        }

        protected override void DataBind(BaseDataBoundControl dataBoundControl)
        {
            System.Web.UI.WebControls.Menu menu = (System.Web.UI.WebControls.Menu) dataBoundControl;
            if (((menu.DataSourceID != null) && (menu.DataSourceID.Length > 0)) || ((menu.DataSource != null) || (menu.Items.Count == 0)))
            {
                menu.Items.Clear();
                base.DataBind(menu);
            }
        }

        private void EditBindings()
        {
            IServiceProvider site = this._menu.Site;
            MenuBindingsEditorForm form = new MenuBindingsEditorForm(site, this._menu, this);
            UIServiceHelper.ShowDialog(site, form);
        }

        private void EditMenuItems()
        {
            PropertyDescriptor member = TypeDescriptor.GetProperties(base.Component)["Items"];
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.EditMenuItemsChangeCallback), null, System.Design.SR.GetString("MenuDesigner_EditNodesTransactionDescription"), member);
        }

        private bool EditMenuItemsChangeCallback(object context)
        {
            IServiceProvider site = this._menu.Site;
            MenuItemCollectionEditorDialog form = new MenuItemCollectionEditorDialog(this._menu, this);
            return (UIServiceHelper.ShowDialog(site, form) == DialogResult.OK);
        }

        public override string GetDesignTimeHtml()
        {
            try
            {
                System.Web.UI.WebControls.Menu viewControl = (System.Web.UI.WebControls.Menu) base.ViewControl;
                ListDictionary data = new ListDictionary();
                data.Add("DesignTimeTextWriterType", typeof(DesignTimeHtmlTextWriter));
                ((IControlDesignerAccessor) base.ViewControl).SetDesignModeState(data);
                int maximumDynamicDisplayLevels = viewControl.MaximumDynamicDisplayLevels;
                if (maximumDynamicDisplayLevels > 10)
                {
                    viewControl.MaximumDynamicDisplayLevels = 10;
                }
                this.DataBind((BaseDataBoundControl) base.ViewControl);
                IDictionary designModeState = ((IControlDesignerAccessor) base.ViewControl).GetDesignModeState();
                switch (this._currentView)
                {
                    case ViewType.Static:
                        return (string) designModeState["GetDesignTimeStaticHtml"];

                    case ViewType.Dynamic:
                        return (string) designModeState["GetDesignTimeDynamicHtml"];
                }
                if (maximumDynamicDisplayLevels > 10)
                {
                    viewControl.MaximumDynamicDisplayLevels = maximumDynamicDisplayLevels;
                }
                return base.GetDesignTimeHtml();
            }
            catch (Exception exception)
            {
                return this.GetErrorDesignTimeHtml(exception);
            }
        }

        protected override string GetEmptyDesignTimeHtml()
        {
            string name = this._menu.Site.Name;
            return string.Format(CultureInfo.CurrentUICulture, "\r\n                <table cellpadding=4 cellspacing=0 style=\"font-family:Tahoma;font-size:8pt;color:buttontext;background-color:buttonface\">\r\n                  <tr><td><span style=\"font-weight:bold\">Menu</span> - {0}</td></tr>\r\n                  <tr><td>{1}</td></tr>\r\n                </table>\r\n             ", new object[] { name, System.Design.SR.GetString("MenuDesigner_Empty") });
        }

        protected override string GetErrorDesignTimeHtml(Exception e)
        {
            string name = this._menu.Site.Name;
            return string.Format(CultureInfo.CurrentUICulture, "\r\n                <table cellpadding=4 cellspacing=0 style=\"font-family:Tahoma;font-size:8pt;color:buttontext;background-color:buttonface;border: solid 1px;border-top-color:buttonhighlight;border-left-color:buttonhighlight;border-bottom-color:buttonshadow;border-right-color:buttonshadow\">\r\n                  <tr><td><span style=\"font-weight:bold\">Menu</span> - {0}</td></tr>\r\n                  <tr><td>{1}</td></tr>\r\n                </table>\r\n             ", new object[] { name, System.Design.SR.GetString("MenuDesigner_Error", new object[] { e.Message }) });
        }

        protected override IHierarchicalEnumerable GetSampleDataSource()
        {
            return new MenuSampleData(this._menu, 0, string.Empty);
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(System.Web.UI.WebControls.Menu));
            base.Initialize(component);
            this._menu = (System.Web.UI.WebControls.Menu) component;
            base.SetViewFlags(ViewFlags.TemplateEditing, true);
        }

        internal void InvokeMenuBindingsEditor()
        {
            this.EditBindings();
        }

        internal void InvokeMenuItemCollectionEditor()
        {
            this.EditMenuItems();
        }

        protected void RefreshSchema(bool preferSilent)
        {
        }

        private void ResetDynamicTemplate()
        {
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ResetDynamicTemplateChangeCallback), null, System.Design.SR.GetString("MenuDesigner_ResetDynamicTemplate"));
        }

        private bool ResetDynamicTemplateChangeCallback(object context)
        {
            this._menu.Controls.Clear();
            this._menu.DynamicItemTemplate = null;
            return true;
        }

        private void ResetStaticTemplate()
        {
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ResetStaticTemplateChangeCallback), null, System.Design.SR.GetString("MenuDesigner_ResetStaticTemplate"));
        }

        private bool ResetStaticTemplateChangeCallback(object context)
        {
            this._menu.Controls.Clear();
            this._menu.StaticItemTemplate = null;
            return true;
        }

        void IDataBindingSchemaProvider.RefreshSchema(bool preferSilent)
        {
            this.RefreshSchema(preferSilent);
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                lists.Add(new MenuDesignerActionList(this));
                return lists;
            }
        }

        public override DesignerAutoFormatCollection AutoFormats
        {
            get
            {
                if (_autoFormats == null)
                {
                    _autoFormats = ControlDesigner.CreateAutoFormats(AutoFormatSchemes.MENU_SCHEME_NAMES, schemeName => new MenuAutoFormat(schemeName, "<Schemes>\r\n<xsd:schema id=\"Schemes\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n  <xsd:element name=\"Scheme\">\r\n     <xsd:complexType>\r\n       <xsd:all>\r\n        <xsd:element name=\"SchemeName\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"DynamicHorizontalOffset\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"DynamicHoverStyle-BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"DynamicHoverStyle-Font--ClearDefaults\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"DynamicHoverStyle-ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"DynamicMenuItemStyle-HorizontalPadding\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"DynamicMenuItemStyle-VerticalPadding\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"DynamicMenuStyle-BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"DynamicSelectedStyle-BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"Font-Size\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"Font-Names\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"StaticHoverStyle-BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"StaticHoverStyle-Font--ClearDefaults\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"StaticHoverStyle-ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"StaticMenuItemStyle-HorizontalPadding\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"StaticMenuItemStyle-VerticalPadding\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"StaticSelectedStyle-BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"StaticSubMenuIndent\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n      </xsd:all>\r\n    </xsd:complexType>\r\n  </xsd:element>\r\n  <xsd:element name=\"Schemes\" msdata:IsDataSet=\"true\">\r\n    <xsd:complexType>\r\n      <xsd:choice maxOccurs=\"unbounded\">\r\n        <xsd:element ref=\"Scheme\"/>\r\n      </xsd:choice>\r\n    </xsd:complexType>\r\n  </xsd:element>\r\n</xsd:schema>\r\n<Scheme>\r\n    <SchemeName>MenuScheme_Empty</SchemeName>\r\n    <BackColor></BackColor>\r\n    <BorderColor></BorderColor>\r\n    <BorderWidth></BorderWidth>\r\n    <BorderStyle>notset</BorderStyle>\r\n    <DynamicHorizontalOffset>0</DynamicHorizontalOffset>\r\n    <DynamicHoverStyle-BackColor></DynamicHoverStyle-BackColor>\r\n    <DynamicHoverStyle-Font--ClearDefaults>true</DynamicHoverStyle-Font--ClearDefaults>\r\n    <DynamicHoverStyle-ForeColor></DynamicHoverStyle-ForeColor>\r\n    <DynamicMenuItemStyle-HorizontalPadding></DynamicMenuItemStyle-HorizontalPadding>\r\n    <DynamicMenuItemStyle-VerticalPadding></DynamicMenuItemStyle-VerticalPadding>\r\n    <DynamicMenuStyle-BackColor></DynamicMenuStyle-BackColor>\r\n    <DynamicSelectedStyle-BackColor></DynamicSelectedStyle-BackColor>\r\n    <Font-Size></Font-Size>\r\n    <Font-Names></Font-Names>\r\n    <ForeColor></ForeColor>\r\n    <StaticHoverStyle-BackColor></StaticHoverStyle-BackColor>\r\n    <StaticHoverStyle-Font--ClearDefaults>true</StaticHoverStyle-Font--ClearDefaults>\r\n    <StaticHoverStyle-ForeColor></StaticHoverStyle-ForeColor>\r\n    <StaticMenuItemStyle-HorizontalPadding></StaticMenuItemStyle-HorizontalPadding>\r\n    <StaticMenuItemStyle-VerticalPadding></StaticMenuItemStyle-VerticalPadding>\r\n    <StaticSelectedStyle-BackColor></StaticSelectedStyle-BackColor>\r\n    <StaticSubMenuIndent>16px</StaticSubMenuIndent>\r\n</Scheme>\r\n  <Scheme>\r\n    <SchemeName>MenuScheme_Classic</SchemeName>\r\n    <BackColor>#B5C7DE</BackColor>\r\n    <BorderColor></BorderColor>\r\n    <BorderWidth></BorderWidth>\r\n    <BorderStyle>notset</BorderStyle>\r\n    <DynamicHorizontalOffset>2</DynamicHorizontalOffset>\r\n    <DynamicHoverStyle-BackColor>#284E98</DynamicHoverStyle-BackColor>\r\n    <DynamicHoverStyle-Font--ClearDefaults>false</DynamicHoverStyle-Font--ClearDefaults>\r\n    <DynamicHoverStyle-ForeColor>White</DynamicHoverStyle-ForeColor>\r\n    <DynamicMenuItemStyle-HorizontalPadding>5</DynamicMenuItemStyle-HorizontalPadding>\r\n    <DynamicMenuItemStyle-VerticalPadding>2</DynamicMenuItemStyle-VerticalPadding>\r\n    <DynamicMenuStyle-BackColor>#B5C7DE</DynamicMenuStyle-BackColor>\r\n    <DynamicSelectedStyle-BackColor>#507CD1</DynamicSelectedStyle-BackColor>\r\n    <Font-Names>Verdana</Font-Names>\r\n    <Font-Size>0.8em</Font-Size>\r\n    <ForeColor>#284E98</ForeColor>\r\n    <StaticHoverStyle-BackColor>#284E98</StaticHoverStyle-BackColor>\r\n    <StaticHoverStyle-Font--ClearDefaults>false</StaticHoverStyle-Font--ClearDefaults>\r\n    <StaticHoverStyle-ForeColor>White</StaticHoverStyle-ForeColor>\r\n    <StaticMenuItemStyle-HorizontalPadding>5</StaticMenuItemStyle-HorizontalPadding>\r\n    <StaticMenuItemStyle-VerticalPadding>2</StaticMenuItemStyle-VerticalPadding>\r\n    <StaticSelectedStyle-BackColor>#507CD1</StaticSelectedStyle-BackColor>\r\n    <StaticSubMenuIndent>10px</StaticSubMenuIndent>\r\n  </Scheme>\r\n<Scheme>\r\n    <SchemeName>MenuScheme_Colorful</SchemeName>\r\n    <BackColor>#FFFBD6</BackColor>\r\n    <BorderColor></BorderColor>\r\n    <BorderWidth></BorderWidth>\r\n    <BorderStyle>notset</BorderStyle>\r\n    <DynamicHorizontalOffset>2</DynamicHorizontalOffset>\r\n    <DynamicHoverStyle-BackColor>#990000</DynamicHoverStyle-BackColor>\r\n    <DynamicHoverStyle-Font--ClearDefaults>false</DynamicHoverStyle-Font--ClearDefaults>\r\n    <DynamicHoverStyle-ForeColor>White</DynamicHoverStyle-ForeColor>\r\n    <DynamicMenuItemStyle-HorizontalPadding>5</DynamicMenuItemStyle-HorizontalPadding>\r\n    <DynamicMenuItemStyle-VerticalPadding>2</DynamicMenuItemStyle-VerticalPadding>\r\n    <DynamicMenuStyle-BackColor>#FFFBD6</DynamicMenuStyle-BackColor>\r\n    <DynamicSelectedStyle-BackColor>#FFCC66</DynamicSelectedStyle-BackColor>\r\n    <Font-Names>Verdana</Font-Names>\r\n    <Font-Size>0.8em</Font-Size>\r\n    <ForeColor>#990000</ForeColor>\r\n    <StaticHoverStyle-BackColor>#990000</StaticHoverStyle-BackColor>\r\n    <StaticHoverStyle-Font--ClearDefaults>false</StaticHoverStyle-Font--ClearDefaults>\r\n    <StaticHoverStyle-ForeColor>White</StaticHoverStyle-ForeColor>\r\n    <StaticMenuItemStyle-HorizontalPadding>5</StaticMenuItemStyle-HorizontalPadding>\r\n    <StaticMenuItemStyle-VerticalPadding>2</StaticMenuItemStyle-VerticalPadding>\r\n    <StaticSelectedStyle-BackColor>#FFCC66</StaticSelectedStyle-BackColor>\r\n    <StaticSubMenuIndent>10px</StaticSubMenuIndent>\r\n</Scheme>\r\n<Scheme>\r\n    <SchemeName>MenuScheme_Professional</SchemeName>\r\n    <BackColor>#F7F6F3</BackColor>\r\n    <BorderColor></BorderColor>\r\n    <BorderWidth></BorderWidth>\r\n    <BorderStyle>notset</BorderStyle>\r\n    <DynamicHorizontalOffset>2</DynamicHorizontalOffset>\r\n    <DynamicHoverStyle-BackColor>#7C6F57</DynamicHoverStyle-BackColor>\r\n    <DynamicHoverStyle-Font--ClearDefaults>false</DynamicHoverStyle-Font--ClearDefaults>\r\n    <DynamicHoverStyle-ForeColor>White</DynamicHoverStyle-ForeColor>\r\n    <DynamicMenuItemStyle-HorizontalPadding>5</DynamicMenuItemStyle-HorizontalPadding>\r\n    <DynamicMenuItemStyle-VerticalPadding>2</DynamicMenuItemStyle-VerticalPadding>\r\n    <DynamicMenuStyle-BackColor>#F7F6F3</DynamicMenuStyle-BackColor>\r\n    <DynamicSelectedStyle-BackColor>#5D7B9D</DynamicSelectedStyle-BackColor>\r\n    <Font-Names>Verdana</Font-Names>\r\n    <Font-Size>0.8em</Font-Size>\r\n    <ForeColor>#7C6F57</ForeColor>\r\n    <StaticHoverStyle-BackColor>#7C6F57</StaticHoverStyle-BackColor>\r\n    <StaticHoverStyle-Font--ClearDefaults>false</StaticHoverStyle-Font--ClearDefaults>\r\n    <StaticHoverStyle-ForeColor>White</StaticHoverStyle-ForeColor>\r\n    <StaticMenuItemStyle-HorizontalPadding>5</StaticMenuItemStyle-HorizontalPadding>\r\n    <StaticMenuItemStyle-VerticalPadding>2</StaticMenuItemStyle-VerticalPadding>\r\n    <StaticSelectedStyle-BackColor>#5D7B9D</StaticSelectedStyle-BackColor>\r\n    <StaticSubMenuIndent>10px</StaticSubMenuIndent>\r\n</Scheme>\r\n  <Scheme>\r\n    <SchemeName>MenuScheme_Simple</SchemeName>\r\n    <BackColor>#E3EAEB</BackColor>\r\n    <BorderColor></BorderColor>\r\n    <BorderWidth></BorderWidth>\r\n    <BorderStyle>notset</BorderStyle>\r\n    <DynamicHorizontalOffset>2</DynamicHorizontalOffset>\r\n    <DynamicHoverStyle-BackColor>#666666</DynamicHoverStyle-BackColor>\r\n    <DynamicHoverStyle-Font--ClearDefaults>false</DynamicHoverStyle-Font--ClearDefaults>\r\n    <DynamicHoverStyle-ForeColor>White</DynamicHoverStyle-ForeColor>\r\n    <DynamicMenuItemStyle-HorizontalPadding>5</DynamicMenuItemStyle-HorizontalPadding>\r\n    <DynamicMenuItemStyle-VerticalPadding>2</DynamicMenuItemStyle-VerticalPadding>\r\n    <DynamicMenuStyle-BackColor>#E3EAEB</DynamicMenuStyle-BackColor>\r\n    <DynamicSelectedStyle-BackColor>#1C5E55</DynamicSelectedStyle-BackColor>\r\n    <Font-Names>Verdana</Font-Names>\r\n    <Font-Size>0.8em</Font-Size>\r\n    <ForeColor>#666666</ForeColor>\r\n    <StaticHoverStyle-BackColor>#666666</StaticHoverStyle-BackColor>\r\n    <StaticHoverStyle-Font--ClearDefaults>false</StaticHoverStyle-Font--ClearDefaults>\r\n    <StaticHoverStyle-ForeColor>White</StaticHoverStyle-ForeColor>\r\n    <StaticMenuItemStyle-HorizontalPadding>5</StaticMenuItemStyle-HorizontalPadding>\r\n    <StaticMenuItemStyle-VerticalPadding>2</StaticMenuItemStyle-VerticalPadding>\r\n    <StaticSelectedStyle-BackColor>#1C5E55</StaticSelectedStyle-BackColor>\r\n    <StaticSubMenuIndent>10px</StaticSubMenuIndent>\r\n  </Scheme>\r\n</Schemes>\r\n"));
                }
                return _autoFormats;
            }
        }

        protected bool CanRefreshSchema
        {
            get
            {
                return false;
            }
        }

        private bool DynamicTemplated
        {
            get
            {
                return (this._menu.DynamicItemTemplate != null);
            }
        }

        protected IDataSourceViewSchema Schema
        {
            get
            {
                return new MenuItemSchema();
            }
        }

        private bool StaticTemplated
        {
            get
            {
                return (this._menu.StaticItemTemplate != null);
            }
        }

        bool IDataBindingSchemaProvider.CanRefreshSchema
        {
            get
            {
                return this.CanRefreshSchema;
            }
        }

        IDataSourceViewSchema IDataBindingSchemaProvider.Schema
        {
            get
            {
                return this.Schema;
            }
        }

        public override TemplateGroupCollection TemplateGroups
        {
            get
            {
                TemplateGroupCollection templateGroups = base.TemplateGroups;
                if (this._templateGroups == null)
                {
                    this._templateGroups = new TemplateGroupCollection();
                    TemplateGroup group = new TemplateGroup("Item Templates", ((WebControl) base.ViewControl).ControlStyle);
                    TemplateDefinition templateDefinition = new TemplateDefinition(this, _templateNames[0], this._menu, _templateNames[0], ((System.Web.UI.WebControls.Menu) base.ViewControl).StaticMenuStyle) {
                        SupportsDataBinding = true
                    };
                    group.AddTemplateDefinition(templateDefinition);
                    TemplateDefinition definition2 = new TemplateDefinition(this, _templateNames[1], this._menu, _templateNames[1], ((System.Web.UI.WebControls.Menu) base.ViewControl).DynamicMenuStyle) {
                        SupportsDataBinding = true
                    };
                    group.AddTemplateDefinition(definition2);
                    this._templateGroups.Add(group);
                }
                templateGroups.AddRange(this._templateGroups);
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

        private class MenuDesignerActionList : DesignerActionList
        {
            private MenuDesigner _parent;

            public MenuDesignerActionList(MenuDesigner parent) : base(parent.Component)
            {
                this._parent = parent;
            }

            public void ConvertToDynamicTemplate()
            {
                this._parent.ConvertToDynamicTemplate();
            }

            public void ConvertToStaticTemplate()
            {
                this._parent.ConvertToStaticTemplate();
            }

            public void EditBindings()
            {
                this._parent.EditBindings();
            }

            public void EditMenuItems()
            {
                this._parent.EditMenuItems();
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                string category = System.Design.SR.GetString("MenuDesigner_DataActionGroup");
                items.Add(new DesignerActionPropertyItem("View", System.Design.SR.GetString("WebControls_Views"), category, System.Design.SR.GetString("MenuDesigner_ViewsDescription")));
                if (string.IsNullOrEmpty(this._parent.DataSourceID))
                {
                    items.Add(new DesignerActionMethodItem(this, "EditMenuItems", System.Design.SR.GetString("MenuDesigner_EditMenuItems"), category, System.Design.SR.GetString("MenuDesigner_EditMenuItemsDescription"), true));
                }
                else
                {
                    items.Add(new DesignerActionMethodItem(this, "EditBindings", System.Design.SR.GetString("MenuDesigner_EditBindings"), category, System.Design.SR.GetString("MenuDesigner_EditBindingsDescription"), true));
                }
                if (this._parent.DynamicTemplated)
                {
                    items.Add(new DesignerActionMethodItem(this, "ResetDynamicTemplate", System.Design.SR.GetString("MenuDesigner_ResetDynamicTemplate"), category, System.Design.SR.GetString("MenuDesigner_ResetDynamicTemplateDescription"), true));
                }
                else
                {
                    items.Add(new DesignerActionMethodItem(this, "ConvertToDynamicTemplate", System.Design.SR.GetString("MenuDesigner_ConvertToDynamicTemplate"), category, System.Design.SR.GetString("MenuDesigner_ConvertToDynamicTemplateDescription"), true));
                }
                if (this._parent.StaticTemplated)
                {
                    items.Add(new DesignerActionMethodItem(this, "ResetStaticTemplate", System.Design.SR.GetString("MenuDesigner_ResetStaticTemplate"), category, System.Design.SR.GetString("MenuDesigner_ResetStaticTemplateDescription"), true));
                    return items;
                }
                items.Add(new DesignerActionMethodItem(this, "ConvertToStaticTemplate", System.Design.SR.GetString("MenuDesigner_ConvertToStaticTemplate"), category, System.Design.SR.GetString("MenuDesigner_ConvertToStaticTemplateDescription"), true));
                return items;
            }

            public void ResetDynamicTemplate()
            {
                this._parent.ResetDynamicTemplate();
            }

            public void ResetStaticTemplate()
            {
                this._parent.ResetStaticTemplate();
            }

            public override bool AutoShow
            {
                get
                {
                    return true;
                }
                set
                {
                }
            }

            [TypeConverter(typeof(MenuViewTypeConverter))]
            public string View
            {
                get
                {
                    if (this._parent._currentView == MenuDesigner.ViewType.Static)
                    {
                        return System.Design.SR.GetString("Menu_StaticView");
                    }
                    if (this._parent._currentView == MenuDesigner.ViewType.Dynamic)
                    {
                        return System.Design.SR.GetString("Menu_DynamicView");
                    }
                    return string.Empty;
                }
                set
                {
                    if (string.Compare(value, System.Design.SR.GetString("Menu_StaticView"), StringComparison.Ordinal) == 0)
                    {
                        this._parent._currentView = MenuDesigner.ViewType.Static;
                    }
                    else if (string.Compare(value, System.Design.SR.GetString("Menu_DynamicView"), StringComparison.Ordinal) == 0)
                    {
                        this._parent._currentView = MenuDesigner.ViewType.Dynamic;
                    }
                    TypeDescriptor.Refresh(this._parent.Component);
                    this._parent.UpdateDesignTimeHtml();
                }
            }

            private class MenuViewTypeConverter : TypeConverter
            {
                public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
                {
                    return new TypeConverter.StandardValuesCollection(new string[] { System.Design.SR.GetString("Menu_StaticView"), System.Design.SR.GetString("Menu_DynamicView") });
                }

                public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
                {
                    return true;
                }

                public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
                {
                    return true;
                }
            }
        }

        private class MenuItemSchema : IDataSourceViewSchema
        {
            private static IDataSourceFieldSchema[] _fieldSchema;

            static MenuItemSchema()
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(System.Web.UI.WebControls.MenuItem));
                _fieldSchema = new IDataSourceFieldSchema[] { new TypeFieldSchema(properties["DataPath"]), new TypeFieldSchema(properties["Depth"]), new TypeFieldSchema(properties["Enabled"]), new TypeFieldSchema(properties["ImageUrl"]), new TypeFieldSchema(properties["NavigateUrl"]), new TypeFieldSchema(properties["PopOutImageUrl"]), new TypeFieldSchema(properties["Selectable"]), new TypeFieldSchema(properties["Selected"]), new TypeFieldSchema(properties["SeparatorImageUrl"]), new TypeFieldSchema(properties["Target"]), new TypeFieldSchema(properties["Text"]), new TypeFieldSchema(properties["ToolTip"]), new TypeFieldSchema(properties["Value"]), new TypeFieldSchema(properties["ValuePath"]) };
            }

            IDataSourceViewSchema[] IDataSourceViewSchema.GetChildren()
            {
                return new IDataSourceViewSchema[0];
            }

            IDataSourceFieldSchema[] IDataSourceViewSchema.GetFields()
            {
                return _fieldSchema;
            }

            string IDataSourceViewSchema.Name
            {
                get
                {
                    return "MenuItem";
                }
            }
        }

        private class MenuSampleData : IHierarchicalEnumerable, IEnumerable
        {
            private ArrayList _list = new ArrayList();
            private System.Web.UI.WebControls.Menu _menu;

            public MenuSampleData(System.Web.UI.WebControls.Menu menu, int depth, string path)
            {
                this._menu = menu;
                int num = this._menu.StaticDisplayLevels + this._menu.MaximumDynamicDisplayLevels;
                if ((num < this._menu.StaticDisplayLevels) || (num < this._menu.MaximumDynamicDisplayLevels))
                {
                    num = 0x7fffffff;
                }
                if (depth == 0)
                {
                    this._list.Add(new MenuDesigner.MenuSampleDataNode(this._menu, System.Design.SR.GetString("HierarchicalDataBoundControlDesigner_SampleRoot"), depth, path, false));
                    this._list.Add(new MenuDesigner.MenuSampleDataNode(this._menu, System.Design.SR.GetString("HierarchicalDataBoundControlDesigner_SampleRoot"), depth, path));
                    this._list.Add(new MenuDesigner.MenuSampleDataNode(this._menu, System.Design.SR.GetString("HierarchicalDataBoundControlDesigner_SampleRoot"), depth, path, false));
                    this._list.Add(new MenuDesigner.MenuSampleDataNode(this._menu, System.Design.SR.GetString("HierarchicalDataBoundControlDesigner_SampleRoot"), depth, path, false));
                    this._list.Add(new MenuDesigner.MenuSampleDataNode(this._menu, System.Design.SR.GetString("HierarchicalDataBoundControlDesigner_SampleRoot"), depth, path, false));
                }
                else if ((depth <= this._menu.StaticDisplayLevels) && (depth < 10))
                {
                    this._list.Add(new MenuDesigner.MenuSampleDataNode(this._menu, System.Design.SR.GetString("HierarchicalDataBoundControlDesigner_SampleParent", new object[] { depth }), depth, path));
                }
                else if ((depth < num) && (depth < 10))
                {
                    this._list.Add(new MenuDesigner.MenuSampleDataNode(this._menu, System.Design.SR.GetString("HierarchicalDataBoundControlDesigner_SampleLeaf", new object[] { 1 }), depth, path));
                    this._list.Add(new MenuDesigner.MenuSampleDataNode(this._menu, System.Design.SR.GetString("HierarchicalDataBoundControlDesigner_SampleLeaf", new object[] { 2 }), depth, path));
                    this._list.Add(new MenuDesigner.MenuSampleDataNode(this._menu, System.Design.SR.GetString("HierarchicalDataBoundControlDesigner_SampleLeaf", new object[] { 3 }), depth, path));
                    this._list.Add(new MenuDesigner.MenuSampleDataNode(this._menu, System.Design.SR.GetString("HierarchicalDataBoundControlDesigner_SampleLeaf", new object[] { 4 }), depth, path));
                }
            }

            public IEnumerator GetEnumerator()
            {
                return this._list.GetEnumerator();
            }

            public IHierarchyData GetHierarchyData(object enumeratedItem)
            {
                return (IHierarchyData) enumeratedItem;
            }
        }

        private class MenuSampleDataNode : IHierarchyData
        {
            private int _depth;
            private bool _hasChildren;
            private System.Web.UI.WebControls.Menu _menu;
            private string _path;
            private string _text;

            public MenuSampleDataNode(System.Web.UI.WebControls.Menu menu, string text, int depth, string path) : this(menu, text, depth, path, true)
            {
            }

            public MenuSampleDataNode(System.Web.UI.WebControls.Menu menu, string text, int depth, string path, bool hasChildren)
            {
                this._text = text;
                this._depth = depth;
                this._path = path + '\\' + text;
                this._menu = menu;
                this._hasChildren = hasChildren;
            }

            public IHierarchicalEnumerable GetChildren()
            {
                return new MenuDesigner.MenuSampleData(this._menu, this._depth + 1, this._path);
            }

            public IHierarchyData GetParent()
            {
                return null;
            }

            public override string ToString()
            {
                return this._text;
            }

            public bool HasChildren
            {
                get
                {
                    if (!this._hasChildren)
                    {
                        return false;
                    }
                    int num = this._menu.StaticDisplayLevels + this._menu.MaximumDynamicDisplayLevels;
                    if ((num < this._menu.StaticDisplayLevels) || (num < this._menu.MaximumDynamicDisplayLevels))
                    {
                        num = 0x7fffffff;
                    }
                    return ((this._depth < num) && (this._depth < 10));
                }
            }

            public object Item
            {
                get
                {
                    return this;
                }
            }

            public string Path
            {
                get
                {
                    return this._path;
                }
            }

            public string Type
            {
                get
                {
                    return "SampleData";
                }
            }
        }

        private enum ViewType
        {
            Static,
            Dynamic
        }
    }
}

