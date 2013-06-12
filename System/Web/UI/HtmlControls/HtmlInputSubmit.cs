namespace System.Web.UI.HtmlControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;

    [SupportsEventValidation, DefaultEvent("ServerClick")]
    public class HtmlInputSubmit : HtmlInputButton, IPostBackEventHandler
    {
        public HtmlInputSubmit() : base("submit")
        {
        }

        public HtmlInputSubmit(string type) : base(type)
        {
        }

        internal override void RenderAttributesInternal(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                Util.WriteOnClickAttribute(writer, this, true, false, this.CausesValidation && (this.Page.GetValidators(this.ValidationGroup).Count > 0), this.ValidationGroup);
            }
        }
    }
}

