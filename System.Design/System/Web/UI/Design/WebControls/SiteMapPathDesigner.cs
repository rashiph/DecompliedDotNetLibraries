namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class SiteMapPathDesigner : ControlDesigner
    {
        private DesignerAutoFormatCollection _autoFormats;
        private static string[] _controlTemplateNames = new string[] { "NodeTemplate", "CurrentNodeTemplate", "RootNodeTemplate", "PathSeparatorTemplate" };
        private SiteMapPath _navigationPath;
        private SiteMapProvider _siteMapProvider;
        private static Style[] _templateStyleArray;

        public override string GetDesignTimeHtml()
        {
            SiteMapPath viewControl = (SiteMapPath) base.ViewControl;
            try
            {
                viewControl.Provider = this.DesignTimeSiteMapProvider;
                ICompositeControlDesignerAccessor accessor = viewControl;
                accessor.RecreateChildControls();
                return base.GetDesignTimeHtml();
            }
            catch (Exception exception)
            {
                return this.GetErrorDesignTimeHtml(exception);
            }
        }

        protected override string GetErrorDesignTimeHtml(Exception e)
        {
            return base.CreatePlaceHolderDesignTimeHtml(System.Design.SR.GetString("Control_ErrorRendering") + e.Message);
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(SiteMapPath));
            base.Initialize(component);
            this._navigationPath = (SiteMapPath) component;
            if (base.View != null)
            {
                base.View.SetFlags(ViewFlags.TemplateEditing, true);
            }
        }

        public override DesignerAutoFormatCollection AutoFormats
        {
            get
            {
                if (this._autoFormats == null)
                {
                    this._autoFormats = ControlDesigner.CreateAutoFormats(AutoFormatSchemes.SITEMAPPATH_SCHEME_NAMES, schemeName => new SiteMapPathAutoFormat(schemeName, "<Schemes>\r\n        <xsd:schema id=\"Schemes\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n          <xsd:element name=\"Scheme\">\r\n            <xsd:complexType>\r\n              <xsd:all>\r\n                <xsd:element name=\"SchemeName\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FontName\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"FontSize\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PathSeparator\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"NodeStyleFontBold\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"NodeStyleForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"RootNodeStyleFontBold\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"RootNodeStyleForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"CurrentNodeStyleForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PathSeparatorStyleForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n                <xsd:element name=\"PathSeparatorStyleFontBold\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n              </xsd:all>\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n          <xsd:element name=\"Schemes\" msdata:IsDataSet=\"true\">\r\n            <xsd:complexType>\r\n              <xsd:choice maxOccurs=\"unbounded\">\r\n                <xsd:element ref=\"Scheme\"/>\r\n              </xsd:choice>\r\n            </xsd:complexType>\r\n          </xsd:element>\r\n        </xsd:schema>\r\n        <Scheme>\r\n          <SchemeName>SiteMapPathAFmt_Scheme_Default</SchemeName>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>SiteMapPathAFmt_Scheme_Colorful</SchemeName>\r\n          <FontName>Verdana</FontName>\r\n          <FontSize>0.8em</FontSize>\r\n          <PathSeparator> : </PathSeparator>\r\n          <NodeStyleFontBold>True</NodeStyleFontBold>\r\n          <NodeStyleForeColor>#990000</NodeStyleForeColor>\r\n          <RootNodeStyleFontBold>True</RootNodeStyleFontBold>\r\n          <RootNodeStyleForeColor>#FF8000</RootNodeStyleForeColor>\r\n          <CurrentNodeStyleForeColor>#333333</CurrentNodeStyleForeColor>\r\n          <PathSeparatorStyleFontBold>True</PathSeparatorStyleFontBold>\r\n          <PathSeparatorStyleForeColor>#990000</PathSeparatorStyleForeColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>SiteMapPathAFmt_Scheme_Simple</SchemeName>\r\n          <FontName>Verdana</FontName>\r\n          <FontSize>0.8em</FontSize>\r\n          <PathSeparator> : </PathSeparator>\r\n          <NodeStyleFontBold>True</NodeStyleFontBold>\r\n          <NodeStyleForeColor>#666666</NodeStyleForeColor>\r\n          <RootNodeStyleFontBold>True</RootNodeStyleFontBold>\r\n          <RootNodeStyleForeColor>#1C5E55</RootNodeStyleForeColor>\r\n          <CurrentNodeStyleForeColor>#333333</CurrentNodeStyleForeColor>\r\n          <PathSeparatorStyleFontBold>True</PathSeparatorStyleFontBold>\r\n          <PathSeparatorStyleForeColor>#1C5E55</PathSeparatorStyleForeColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>SiteMapPathAFmt_Scheme_Professional</SchemeName>\r\n          <FontName>Verdana</FontName>\r\n          <FontSize>0.8em</FontSize>\r\n          <PathSeparator> : </PathSeparator>\r\n          <NodeStyleFontBold>True</NodeStyleFontBold>\r\n          <NodeStyleForeColor>#7C6F57</NodeStyleForeColor>\r\n          <RootNodeStyleFontBold>True</RootNodeStyleFontBold>\r\n          <RootNodeStyleForeColor>#5D7B9D</RootNodeStyleForeColor>\r\n          <CurrentNodeStyleForeColor>#333333</CurrentNodeStyleForeColor>\r\n          <PathSeparatorStyleFontBold>True</PathSeparatorStyleFontBold>\r\n          <PathSeparatorStyleForeColor>#5D7B9D</PathSeparatorStyleForeColor>\r\n        </Scheme>\r\n        <Scheme>\r\n          <SchemeName>SiteMapPathAFmt_Scheme_Classic</SchemeName>\r\n          <FontName>Verdana</FontName>\r\n          <FontSize>0.8em</FontSize>\r\n          <PathSeparator> : </PathSeparator>\r\n          <NodeStyleFontBold>True</NodeStyleFontBold>\r\n          <NodeStyleForeColor>#284E98</NodeStyleForeColor>\r\n          <RootNodeStyleFontBold>True</RootNodeStyleFontBold>\r\n          <RootNodeStyleForeColor>#507CD1</RootNodeStyleForeColor>\r\n          <CurrentNodeStyleForeColor>#333333</CurrentNodeStyleForeColor>\r\n          <PathSeparatorStyleFontBold>True</PathSeparatorStyleFontBold>\r\n          <PathSeparatorStyleForeColor>#507CD1</PathSeparatorStyleForeColor>\r\n        </Scheme>\r\n      </Schemes>"));
                }
                return this._autoFormats;
            }
        }

        private SiteMapProvider DesignTimeSiteMapProvider
        {
            get
            {
                if (this._siteMapProvider == null)
                {
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    this._siteMapProvider = new System.Web.UI.Design.WebControls.DesignTimeSiteMapProvider(service);
                }
                return this._siteMapProvider;
            }
        }

        public override TemplateGroupCollection TemplateGroups
        {
            get
            {
                TemplateGroupCollection templateGroups = base.TemplateGroups;
                for (int i = 0; i < _controlTemplateNames.Length; i++)
                {
                    string groupName = _controlTemplateNames[i];
                    TemplateGroup group = new TemplateGroup(groupName);
                    group.AddTemplateDefinition(new TemplateDefinition(this, groupName, base.Component, groupName, this.TemplateStyleArray[i]));
                    templateGroups.Add(group);
                }
                return templateGroups;
            }
        }

        private Style[] TemplateStyleArray
        {
            get
            {
                if (_templateStyleArray == null)
                {
                    _templateStyleArray = new Style[] { ((SiteMapPath) base.ViewControl).NodeStyle, ((SiteMapPath) base.ViewControl).CurrentNodeStyle, ((SiteMapPath) base.ViewControl).RootNodeStyle, ((SiteMapPath) base.ViewControl).PathSeparatorStyle };
                }
                return _templateStyleArray;
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

