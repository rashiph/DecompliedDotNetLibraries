namespace System.Web.UI.HtmlControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;

    [ValidationProperty("Value"), DefaultEvent("ServerChange"), SupportsEventValidation]
    public class HtmlInputPassword : HtmlInputText, IPostBackDataHandler
    {
        private static readonly object EventServerChange = new object();

        public HtmlInputPassword() : base("password")
        {
        }

        protected override void RenderAttributes(HtmlTextWriter writer)
        {
            this.ViewState.Remove("value");
            base.RenderAttributes(writer);
        }
    }
}

