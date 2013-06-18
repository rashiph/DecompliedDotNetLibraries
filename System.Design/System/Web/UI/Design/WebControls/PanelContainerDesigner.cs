namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    public class PanelContainerDesigner : ContainerControlDesigner
    {
        private const string PanelNoCaptionDesignTimeHtml = "<div style=\"{0}{2}{3}{4}{6}{10}\" class=\"{11}\" {7}=0></div>";
        private const string PanelWithCaptionDesignTimeHtml = "<div style=\"{0}{2}{3}{4}{6}{10}\" class=\"{11}\">\r\n    <fieldset>\r\n        <legend>{5}</legend>\r\n        <div {7}=0></div>\r\n    </fieldset>\r\n</div>";

        protected override void AddDesignTimeCssAttributes(IDictionary styleAttributes)
        {
            Panel component = (Panel) base.Component;
            switch (component.Direction)
            {
                case ContentDirection.LeftToRight:
                    styleAttributes["direction"] = "ltr";
                    break;

                case ContentDirection.RightToLeft:
                    styleAttributes["direction"] = "rtl";
                    break;
            }
            string backImageUrl = component.BackImageUrl;
            if (backImageUrl.Trim().Length > 0)
            {
                IUrlResolutionService service = (IUrlResolutionService) this.GetService(typeof(IUrlResolutionService));
                if (service != null)
                {
                    backImageUrl = service.ResolveClientUrl(backImageUrl);
                    styleAttributes["background-image"] = "url(" + backImageUrl + ")";
                }
            }
            switch (component.ScrollBars)
            {
                case ScrollBars.Horizontal:
                    styleAttributes["overflow-x"] = "scroll";
                    break;

                case ScrollBars.Vertical:
                    styleAttributes["overflow-y"] = "scroll";
                    break;

                case ScrollBars.Both:
                    styleAttributes["overflow"] = "scroll";
                    break;

                case ScrollBars.Auto:
                    styleAttributes["overflow"] = "auto";
                    break;
            }
            HorizontalAlign horizontalAlign = component.HorizontalAlign;
            if (horizontalAlign != HorizontalAlign.NotSet)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(HorizontalAlign));
                styleAttributes["text-align"] = converter.ConvertToInvariantString(horizontalAlign).ToLowerInvariant();
            }
            if (!component.Wrap)
            {
                styleAttributes["white-space"] = "nowrap";
            }
            base.AddDesignTimeCssAttributes(styleAttributes);
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(Panel));
            base.Initialize(component);
        }

        internal override string DesignTimeHtml
        {
            get
            {
                if (this.FrameCaption.Length > 0)
                {
                    return "<div style=\"{0}{2}{3}{4}{6}{10}\" class=\"{11}\">\r\n    <fieldset>\r\n        <legend>{5}</legend>\r\n        <div {7}=0></div>\r\n    </fieldset>\r\n</div>";
                }
                return "<div style=\"{0}{2}{3}{4}{6}{10}\" class=\"{11}\" {7}=0></div>";
            }
        }

        public override string FrameCaption
        {
            get
            {
                return ((Panel) base.Component).GroupingText;
            }
        }

        public override Style FrameStyle
        {
            get
            {
                if (((Panel) base.Component).GroupingText.Length == 0)
                {
                    return new Style();
                }
                return base.FrameStyle;
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

