namespace System.Web.UI.Design.WebControls.WebParts
{
    using System;
    using System.Design;
    using System.Web.UI;
    using System.Web.UI.Design.WebControls;
    using System.Web.UI.WebControls.WebParts;

    internal sealed class WebPartZoneAutoFormat : ReflectionBasedAutoFormat
    {
        public WebPartZoneAutoFormat(string schemeName, string schemes) : base(schemeName, schemes)
        {
            base.Style.Width = 250;
        }

        public override Control GetPreviewControl(Control runtimeControl)
        {
            WebPartZone previewControl = (WebPartZone) base.GetPreviewControl(runtimeControl);
            if ((previewControl != null) && (previewControl.WebParts.Count == 0))
            {
                previewControl.ZoneTemplate = new AutoFormatTemplate();
            }
            return previewControl;
        }

        private sealed class AutoFormatTemplate : ITemplate
        {
            public void InstantiateIn(Control container)
            {
                container.Controls.Add(new SampleWebPart());
            }

            private sealed class SampleWebPart : WebPart
            {
                public SampleWebPart()
                {
                    this.Title = System.Design.SR.GetString("WebPartZoneAutoFormat_SampleWebPartTitle");
                    this.ID = "SampleWebPart";
                }

                protected internal override void RenderContents(HtmlTextWriter writer)
                {
                    writer.Write(System.Design.SR.GetString("WebPartZoneAutoFormat_SampleWebPartContents"));
                }
            }
        }
    }
}

