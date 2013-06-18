namespace System.Web.UI.Design.WebControls.WebParts
{
    using System;
    using System.Design;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.Design.WebControls;
    using System.Web.UI.WebControls.WebParts;

    internal sealed class CatalogZoneAutoFormat : ReflectionBasedAutoFormat
    {
        internal const string PreviewControlID = "AutoFormatPreviewControl";

        public CatalogZoneAutoFormat(string schemeName, string schemes) : base(schemeName, schemes)
        {
            base.Style.Width = 300;
        }

        public override Control GetPreviewControl(Control runtimeControl)
        {
            CatalogZone previewControl = (CatalogZone) base.GetPreviewControl(runtimeControl);
            if ((previewControl != null) && (previewControl.CatalogParts.Count == 0))
            {
                previewControl.ZoneTemplate = new AutoFormatTemplate();
            }
            previewControl.ID = "AutoFormatPreviewControl";
            return previewControl;
        }

        private sealed class AutoFormatTemplate : ITemplate
        {
            public void InstantiateIn(Control container)
            {
                DeclarativeCatalogPart child = new DeclarativeCatalogPart {
                    WebPartsTemplate = new SampleCatalogPartTemplate(),
                    ID = "SampleCatalogPart"
                };
                container.Controls.Add(child);
            }

            private sealed class SampleCatalogPartTemplate : ITemplate
            {
                public void InstantiateIn(Control container)
                {
                    SampleWebPart child = new SampleWebPart {
                        ID = "SampleWebPart1",
                        Title = string.Format(CultureInfo.CurrentCulture, System.Design.SR.GetString("CatalogZone_SampleWebPartTitle"), new object[] { "1" })
                    };
                    container.Controls.Add(child);
                    child = new SampleWebPart {
                        ID = "SampleWebPart2",
                        Title = string.Format(CultureInfo.CurrentCulture, System.Design.SR.GetString("CatalogZone_SampleWebPartTitle"), new object[] { "2" })
                    };
                    container.Controls.Add(child);
                    child = new SampleWebPart {
                        ID = "SampleWebPart3",
                        Title = string.Format(CultureInfo.CurrentCulture, System.Design.SR.GetString("CatalogZone_SampleWebPartTitle"), new object[] { "3" })
                    };
                    container.Controls.Add(child);
                }

                private sealed class SampleWebPart : WebPart
                {
                }
            }
        }
    }
}

