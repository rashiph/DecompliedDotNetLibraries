namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Web.UI.WebControls;

    public sealed class TitleStyle : TableItemStyle
    {
        public TitleStyle()
        {
            this.Wrap = false;
        }

        [DefaultValue(false)]
        public override bool Wrap
        {
            get
            {
                return base.Wrap;
            }
            set
            {
                base.Wrap = value;
            }
        }
    }
}

