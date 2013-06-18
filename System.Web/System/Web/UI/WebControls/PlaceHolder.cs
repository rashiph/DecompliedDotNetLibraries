namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;

    [ControlBuilder(typeof(PlaceHolderControlBuilder))]
    public class PlaceHolder : Control
    {
        [Browsable(true)]
        public override bool EnableTheming
        {
            get
            {
                return base.EnableTheming;
            }
            set
            {
                base.EnableTheming = value;
            }
        }
    }
}

