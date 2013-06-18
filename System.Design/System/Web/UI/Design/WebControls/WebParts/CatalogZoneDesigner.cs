namespace System.Web.UI.Design.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.WebControls;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.WebParts;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class CatalogZoneDesigner : ToolZoneDesigner
    {
        private static DesignerAutoFormatCollection _autoFormats;
        private TemplateGroup _templateGroup;
        private CatalogZone _zone;

        public override string GetDesignTimeHtml()
        {
            return this.GetDesignTimeHtml(null);
        }

        public override string GetDesignTimeHtml(DesignerRegionCollection regions)
        {
            string emptyDesignTimeHtml;
            try
            {
                CatalogZone viewControl = (CatalogZone) base.ViewControl;
                bool flag = base.UseRegions(regions, this._zone.ZoneTemplate, viewControl.ZoneTemplate);
                if ((viewControl.ZoneTemplate == null) && !flag)
                {
                    emptyDesignTimeHtml = this.GetEmptyDesignTimeHtml();
                }
                else
                {
                    ((ICompositeControlDesignerAccessor) viewControl).RecreateChildControls();
                    if (flag)
                    {
                        viewControl.Controls.Clear();
                        CatalogPartEditableDesignerRegion region = new CatalogPartEditableDesignerRegion(viewControl, base.TemplateDefinition);
                        region.Properties[typeof(Control)] = viewControl;
                        region.IsSingleInstanceTemplate = true;
                        region.Description = System.Design.SR.GetString("ContainerControlDesigner_RegionWatermark");
                        regions.Add(region);
                    }
                    emptyDesignTimeHtml = base.GetDesignTimeHtml();
                }
                if (base.ViewInBrowseMode && (viewControl.ID != "AutoFormatPreviewControl"))
                {
                    emptyDesignTimeHtml = base.CreatePlaceHolderDesignTimeHtml();
                }
            }
            catch (Exception exception)
            {
                emptyDesignTimeHtml = this.GetErrorDesignTimeHtml(exception);
            }
            return emptyDesignTimeHtml;
        }

        public override string GetEditableDesignerRegionContent(EditableDesignerRegion region)
        {
            return ControlPersister.PersistTemplate(this._zone.ZoneTemplate, (IDesignerHost) base.Component.Site.GetService(typeof(IDesignerHost)));
        }

        protected override string GetEmptyDesignTimeHtml()
        {
            return base.CreatePlaceHolderDesignTimeHtml(System.Design.SR.GetString("CatalogZoneDesigner_Empty"));
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(CatalogZone));
            base.Initialize(component);
            this._zone = (CatalogZone) component;
        }

        public override void SetEditableDesignerRegionContent(EditableDesignerRegion region, string content)
        {
            this._zone.ZoneTemplate = ControlParser.ParseTemplate((IDesignerHost) base.Component.Site.GetService(typeof(IDesignerHost)), content);
            base.IsDirtyInternal = true;
        }

        public override DesignerAutoFormatCollection AutoFormats
        {
            get
            {
                if (_autoFormats == null)
                {
                    _autoFormats = ControlDesigner.CreateAutoFormats(AutoFormatSchemes.CATALOGZONE_SCHEME_NAMES, schemeName => new CatalogZoneAutoFormat(schemeName, "<Schemes>\r\n<xsd:schema id=\"Schemes\" xmlns=\"\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n  <xsd:element name=\"Scheme\">\r\n     <xsd:complexType>\r\n       <xsd:all>\r\n        <xsd:element name=\"SchemeName\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"BorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"EditUIStyle-Font-Names\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"EditUIStyle-Font-Size\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"EditUIStyle-ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"EmptyZoneTextStyle-Font-Size\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"EmptyZoneTextStyle-ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"Font-Names\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"FooterStyle-BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"FooterStyle-HorizontalAlign\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderStyle-BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderStyle-Font-Bold\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderStyle-Font-Size\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderStyle-Font--ClearDefaults\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderStyle-ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderVerbStyle-Font-Bold\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderVerbStyle-Font-Size\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderVerbStyle-Font-Underline\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderVerbStyle-Font--ClearDefaults\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"HeaderVerbStyle-ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"InstructionTextStyle-Font-Size\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"InstructionTextStyle-ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"LabelStyle-Font-Size\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"LabelStyle-ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"Padding\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"PartChromeStyle-BorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"PartChromeStyle-BorderStyle\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"PartChromeStyle-BorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"PartStyle-BorderColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"PartStyle-BorderWidth\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"PartLinkStyle-Font-Size\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"PartTitleStyle-BackColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"PartTitleStyle-Font-Bold\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"PartTitleStyle-Font-Size\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"PartTitleStyle-Font--ClearDefaults\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"PartTitleStyle-ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"SelectedPartLinkStyle-Font-Size\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"VerbStyle-Font-Names\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"VerbStyle-Font-Size\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n        <xsd:element name=\"VerbStyle-ForeColor\" minOccurs=\"0\" type=\"xsd:string\"/>\r\n      </xsd:all>\r\n    </xsd:complexType>\r\n  </xsd:element>\r\n  <xsd:element name=\"Schemes\" msdata:IsDataSet=\"true\">\r\n    <xsd:complexType>\r\n      <xsd:choice maxOccurs=\"unbounded\">\r\n        <xsd:element ref=\"Scheme\"/>\r\n      </xsd:choice>\r\n    </xsd:complexType>\r\n  </xsd:element>\r\n</xsd:schema>\r\n<Scheme>\r\n  <SchemeName>WebPartScheme_Empty</SchemeName>\r\n  <HeaderStyle-Font-Bold>False</HeaderStyle-Font-Bold>\r\n  <HeaderStyle-Font--ClearDefaults>True</HeaderStyle-Font--ClearDefaults>\r\n  <HeaderVerbStyle-Font-Bold>False</HeaderVerbStyle-Font-Bold>\r\n  <HeaderVerbStyle-Font-Underline>False</HeaderVerbStyle-Font-Underline>\r\n  <HeaderVerbStyle-Font--ClearDefaults>True</HeaderVerbStyle-Font--ClearDefaults>\r\n  <Padding>2</Padding>\r\n  <PartChromeStyle-BorderStyle>NotSet</PartChromeStyle-BorderStyle>\r\n  <PartTitleStyle-Font-Bold>False</PartTitleStyle-Font-Bold>\r\n  <PartTitleStyle-Font--ClearDefaults>True</PartTitleStyle-Font--ClearDefaults>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>WebPartScheme_Professional</SchemeName>\r\n  <BackColor>#F7F6F3</BackColor>\r\n  <BorderColor>#CCCCCC</BorderColor>\r\n  <BorderWidth>1px</BorderWidth>\r\n  <EditUIStyle-Font-Names>Verdana</EditUIStyle-Font-Names>\r\n  <EditUIStyle-Font-Size>0.8em</EditUIStyle-Font-Size>\r\n  <EditUIStyle-ForeColor>#333333</EditUIStyle-ForeColor>\r\n  <EmptyZoneTextStyle-Font-Size>0.8em</EmptyZoneTextStyle-Font-Size>\r\n  <EmptyZoneTextStyle-ForeColor>#333333</EmptyZoneTextStyle-ForeColor>\r\n  <Font-Names>Verdana</Font-Names>\r\n  <FooterStyle-BackColor>#E2DED6</FooterStyle-BackColor>\r\n  <FooterStyle-HorizontalAlign>Right</FooterStyle-HorizontalAlign>\r\n  <HeaderStyle-BackColor>#E2DED6</HeaderStyle-BackColor>\r\n  <HeaderStyle-Font-Bold>True</HeaderStyle-Font-Bold>\r\n  <HeaderStyle-Font-Size>0.8em</HeaderStyle-Font-Size>\r\n  <HeaderStyle-ForeColor>#333333</HeaderStyle-ForeColor>\r\n  <HeaderVerbStyle-Font-Bold>False</HeaderVerbStyle-Font-Bold>\r\n  <HeaderVerbStyle-Font-Size>0.8em</HeaderVerbStyle-Font-Size>\r\n  <HeaderVerbStyle-Font-Underline>False</HeaderVerbStyle-Font-Underline>\r\n  <HeaderVerbStyle-ForeColor>#333333</HeaderVerbStyle-ForeColor>\r\n  <InstructionTextStyle-Font-Size>0.8em</InstructionTextStyle-Font-Size>\r\n  <InstructionTextStyle-ForeColor>#333333</InstructionTextStyle-ForeColor>\r\n  <LabelStyle-Font-Size>0.8em</LabelStyle-Font-Size>\r\n  <LabelStyle-ForeColor>#333333</LabelStyle-ForeColor>\r\n  <Padding>6</Padding>\r\n  <PartChromeStyle-BorderColor>#E2DED6</PartChromeStyle-BorderColor>\r\n  <PartChromeStyle-BorderStyle>Solid</PartChromeStyle-BorderStyle>\r\n  <PartChromeStyle-BorderWidth>1px</PartChromeStyle-BorderWidth>\r\n  <PartLinkStyle-Font-Size>0.8em</PartLinkStyle-Font-Size>\r\n  <PartStyle-BorderColor>#F7F6F3</PartStyle-BorderColor>\r\n  <PartStyle-BorderWidth>5px</PartStyle-BorderWidth>\r\n  <PartTitleStyle-BackColor>#5D7B9D</PartTitleStyle-BackColor>\r\n  <PartTitleStyle-Font-Bold>True</PartTitleStyle-Font-Bold>\r\n  <PartTitleStyle-Font-Size>0.8em</PartTitleStyle-Font-Size>\r\n  <PartTitleStyle-ForeColor>#FFFFFF</PartTitleStyle-ForeColor>\r\n  <SelectedPartLinkStyle-Font-Size>0.8em</SelectedPartLinkStyle-Font-Size>\r\n  <VerbStyle-Font-Names>Verdana</VerbStyle-Font-Names>\r\n  <VerbStyle-Font-Size>0.8em</VerbStyle-Font-Size>\r\n  <VerbStyle-ForeColor>#333333</VerbStyle-ForeColor>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>WebPartScheme_Simple</SchemeName>\r\n  <BackColor>#E3EAEB</BackColor>\r\n  <BorderColor>#CCCCCC</BorderColor>\r\n  <BorderWidth>1px</BorderWidth>\r\n  <EditUIStyle-Font-Names>Verdana</EditUIStyle-Font-Names>\r\n  <EditUIStyle-Font-Size>0.8em</EditUIStyle-Font-Size>\r\n  <EditUIStyle-ForeColor>#333333</EditUIStyle-ForeColor>\r\n  <EmptyZoneTextStyle-Font-Size>0.8em</EmptyZoneTextStyle-Font-Size>\r\n  <EmptyZoneTextStyle-ForeColor>#333333</EmptyZoneTextStyle-ForeColor>\r\n  <Font-Names>Verdana</Font-Names>\r\n  <FooterStyle-BackColor>#C5BBAF</FooterStyle-BackColor>\r\n  <FooterStyle-HorizontalAlign>Right</FooterStyle-HorizontalAlign>\r\n  <HeaderStyle-BackColor>#C5BBAF</HeaderStyle-BackColor>\r\n  <HeaderStyle-Font-Bold>True</HeaderStyle-Font-Bold>\r\n  <HeaderStyle-Font-Size>0.8em</HeaderStyle-Font-Size>\r\n  <HeaderStyle-ForeColor>#333333</HeaderStyle-ForeColor>\r\n  <HeaderVerbStyle-Font-Bold>False</HeaderVerbStyle-Font-Bold>\r\n  <HeaderVerbStyle-Font-Size>0.8em</HeaderVerbStyle-Font-Size>\r\n  <HeaderVerbStyle-Font-Underline>False</HeaderVerbStyle-Font-Underline>\r\n  <HeaderVerbStyle-ForeColor>#333333</HeaderVerbStyle-ForeColor>\r\n  <InstructionTextStyle-Font-Size>0.8em</InstructionTextStyle-Font-Size>\r\n  <InstructionTextStyle-ForeColor>#333333</InstructionTextStyle-ForeColor>\r\n  <LabelStyle-Font-Size>0.8em</LabelStyle-Font-Size>\r\n  <LabelStyle-ForeColor>#333333</LabelStyle-ForeColor>\r\n  <Padding>6</Padding>\r\n  <PartChromeStyle-BorderColor>#C5BBAF</PartChromeStyle-BorderColor>\r\n  <PartChromeStyle-BorderStyle>Solid</PartChromeStyle-BorderStyle>\r\n  <PartChromeStyle-BorderWidth>1px</PartChromeStyle-BorderWidth>\r\n  <PartLinkStyle-Font-Size>0.8em</PartLinkStyle-Font-Size>\r\n  <PartStyle-BorderColor>#E3EAEB</PartStyle-BorderColor>\r\n  <PartStyle-BorderWidth>5px</PartStyle-BorderWidth>\r\n  <PartTitleStyle-BackColor>#1C5E55</PartTitleStyle-BackColor>\r\n  <PartTitleStyle-Font-Bold>True</PartTitleStyle-Font-Bold>\r\n  <PartTitleStyle-Font-Size>0.8em</PartTitleStyle-Font-Size>\r\n  <PartTitleStyle-ForeColor>#FFFFFF</PartTitleStyle-ForeColor>\r\n  <SelectedPartLinkStyle-Font-Size>0.8em</SelectedPartLinkStyle-Font-Size>\r\n  <VerbStyle-Font-Names>Verdana</VerbStyle-Font-Names>\r\n  <VerbStyle-Font-Size>0.8em</VerbStyle-Font-Size>\r\n  <VerbStyle-ForeColor>#333333</VerbStyle-ForeColor>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>WebPartScheme_Classic</SchemeName>\r\n  <BackColor>#EFF3FB</BackColor>\r\n  <BorderColor>#CCCCCC</BorderColor>\r\n  <BorderWidth>1px</BorderWidth>\r\n  <EditUIStyle-Font-Names>Verdana</EditUIStyle-Font-Names>\r\n  <EditUIStyle-Font-Size>0.8em</EditUIStyle-Font-Size>\r\n  <EditUIStyle-ForeColor>#333333</EditUIStyle-ForeColor>\r\n  <EmptyZoneTextStyle-Font-Size>0.8em</EmptyZoneTextStyle-Font-Size>\r\n  <EmptyZoneTextStyle-ForeColor>#333333</EmptyZoneTextStyle-ForeColor>\r\n  <Font-Names>Verdana</Font-Names>\r\n  <FooterStyle-BackColor>#D1DDF1</FooterStyle-BackColor>\r\n  <FooterStyle-HorizontalAlign>Right</FooterStyle-HorizontalAlign>\r\n  <HeaderStyle-BackColor>#D1DDF1</HeaderStyle-BackColor>\r\n  <HeaderStyle-Font-Bold>True</HeaderStyle-Font-Bold>\r\n  <HeaderStyle-Font-Size>0.8em</HeaderStyle-Font-Size>\r\n  <HeaderStyle-ForeColor>#333333</HeaderStyle-ForeColor>\r\n  <HeaderVerbStyle-Font-Bold>False</HeaderVerbStyle-Font-Bold>\r\n  <HeaderVerbStyle-Font-Size>0.8em</HeaderVerbStyle-Font-Size>\r\n  <HeaderVerbStyle-Font-Underline>False</HeaderVerbStyle-Font-Underline>\r\n  <HeaderVerbStyle-ForeColor>#333333</HeaderVerbStyle-ForeColor>\r\n  <InstructionTextStyle-Font-Size>0.8em</InstructionTextStyle-Font-Size>\r\n  <InstructionTextStyle-ForeColor>#333333</InstructionTextStyle-ForeColor>\r\n  <LabelStyle-Font-Size>0.8em</LabelStyle-Font-Size>\r\n  <LabelStyle-ForeColor>#333333</LabelStyle-ForeColor>\r\n  <Padding>6</Padding>\r\n  <PartChromeStyle-BorderColor>#D1DDF1</PartChromeStyle-BorderColor>\r\n  <PartChromeStyle-BorderStyle>Solid</PartChromeStyle-BorderStyle>\r\n  <PartChromeStyle-BorderWidth>1px</PartChromeStyle-BorderWidth>\r\n  <PartLinkStyle-Font-Size>0.8em</PartLinkStyle-Font-Size>\r\n  <PartStyle-BorderColor>#EFF3FB</PartStyle-BorderColor>\r\n  <PartStyle-BorderWidth>5px</PartStyle-BorderWidth>\r\n  <PartTitleStyle-BackColor>#507CD1</PartTitleStyle-BackColor>\r\n  <PartTitleStyle-Font-Bold>True</PartTitleStyle-Font-Bold>\r\n  <PartTitleStyle-Font-Size>0.8em</PartTitleStyle-Font-Size>\r\n  <PartTitleStyle-ForeColor>#FFFFFF</PartTitleStyle-ForeColor>\r\n  <SelectedPartLinkStyle-Font-Size>0.8em</SelectedPartLinkStyle-Font-Size>\r\n  <VerbStyle-Font-Names>Verdana</VerbStyle-Font-Names>\r\n  <VerbStyle-Font-Size>0.8em</VerbStyle-Font-Size>\r\n  <VerbStyle-ForeColor>#333333</VerbStyle-ForeColor>\r\n</Scheme>\r\n<Scheme>\r\n  <SchemeName>WebPartScheme_Colorful</SchemeName>\r\n  <BackColor>#FFFBD6</BackColor>\r\n  <BorderColor>#CCCCCC</BorderColor>\r\n  <BorderWidth>1px</BorderWidth>\r\n  <EditUIStyle-Font-Names>Verdana</EditUIStyle-Font-Names>\r\n  <EditUIStyle-Font-Size>0.8em</EditUIStyle-Font-Size>\r\n  <EditUIStyle-ForeColor>#333333</EditUIStyle-ForeColor>\r\n  <EmptyZoneTextStyle-Font-Size>0.8em</EmptyZoneTextStyle-Font-Size>\r\n  <EmptyZoneTextStyle-ForeColor>#333333</EmptyZoneTextStyle-ForeColor>\r\n  <Font-Names>Verdana</Font-Names>\r\n  <FooterStyle-BackColor>#FFCC66</FooterStyle-BackColor>\r\n  <FooterStyle-HorizontalAlign>Right</FooterStyle-HorizontalAlign>\r\n  <HeaderStyle-BackColor>#FFCC66</HeaderStyle-BackColor>\r\n  <HeaderStyle-Font-Bold>True</HeaderStyle-Font-Bold>\r\n  <HeaderStyle-Font-Size>0.8em</HeaderStyle-Font-Size>\r\n  <HeaderStyle-ForeColor>#333333</HeaderStyle-ForeColor>\r\n  <HeaderVerbStyle-Font-Bold>False</HeaderVerbStyle-Font-Bold>\r\n  <HeaderVerbStyle-Font-Size>0.8em</HeaderVerbStyle-Font-Size>\r\n  <HeaderVerbStyle-Font-Underline>False</HeaderVerbStyle-Font-Underline>\r\n  <HeaderVerbStyle-ForeColor>#333333</HeaderVerbStyle-ForeColor>\r\n  <InstructionTextStyle-Font-Size>0.8em</InstructionTextStyle-Font-Size>\r\n  <InstructionTextStyle-ForeColor>#333333</InstructionTextStyle-ForeColor>\r\n  <LabelStyle-Font-Size>0.8em</LabelStyle-Font-Size>\r\n  <LabelStyle-ForeColor>#333333</LabelStyle-ForeColor>\r\n  <Padding>6</Padding>\r\n  <PartChromeStyle-BorderColor>#FFCC66</PartChromeStyle-BorderColor>\r\n  <PartChromeStyle-BorderStyle>Solid</PartChromeStyle-BorderStyle>\r\n  <PartChromeStyle-BorderWidth>1px</PartChromeStyle-BorderWidth>\r\n  <PartLinkStyle-Font-Size>0.8em</PartLinkStyle-Font-Size>\r\n  <PartStyle-BorderColor>#FFFBD6</PartStyle-BorderColor>\r\n  <PartStyle-BorderWidth>5px</PartStyle-BorderWidth>\r\n  <PartTitleStyle-BackColor>#990000</PartTitleStyle-BackColor>\r\n  <PartTitleStyle-Font-Bold>True</PartTitleStyle-Font-Bold>\r\n  <PartTitleStyle-Font-Size>0.8em</PartTitleStyle-Font-Size>\r\n  <PartTitleStyle-ForeColor>#FFFFFF</PartTitleStyle-ForeColor>\r\n  <SelectedPartLinkStyle-Font-Size>0.8em</SelectedPartLinkStyle-Font-Size>\r\n  <VerbStyle-Font-Names>Verdana</VerbStyle-Font-Names>\r\n  <VerbStyle-Font-Size>0.8em</VerbStyle-Font-Size>\r\n  <VerbStyle-ForeColor>#333333</VerbStyle-ForeColor>\r\n</Scheme>\r\n</Schemes>\r\n"));
                }
                return _autoFormats;
            }
        }

        public override TemplateGroupCollection TemplateGroups
        {
            get
            {
                TemplateGroupCollection templateGroups = base.TemplateGroups;
                if (this._templateGroup == null)
                {
                    this._templateGroup = base.CreateZoneTemplateGroup();
                }
                templateGroups.Add(this._templateGroup);
                return templateGroups;
            }
        }

        private sealed class CatalogPartEditableDesignerRegion : TemplatedEditableDesignerRegion
        {
            private CatalogZone _zone;

            public CatalogPartEditableDesignerRegion(CatalogZone zone, TemplateDefinition templateDefinition) : base(templateDefinition)
            {
                this._zone = zone;
            }

            public override ViewRendering GetChildViewRendering(Control control)
            {
                if (control == null)
                {
                    throw new ArgumentNullException("control");
                }
                DesignerCatalogPartChrome chrome = new DesignerCatalogPartChrome(this._zone);
                return chrome.GetViewRendering(control);
            }
        }
    }
}

