namespace System.Web.UI.WebControls
{
    using System;
    using System.Drawing;
    using System.Web;
    using System.Web.UI;

    [SupportsEventValidation]
    internal class DataControlPagerLinkButton : DataControlLinkButton
    {
        internal DataControlPagerLinkButton(IPostBackContainer container) : base(container)
        {
        }

        protected override void SetForeColor()
        {
            if (!base.ControlStyle.IsSet(4))
            {
                Control parent = this;
                for (int i = 0; i < 6; i++)
                {
                    parent = parent.Parent;
                    Color foreColor = ((WebControl) parent).ForeColor;
                    if (foreColor != Color.Empty)
                    {
                        this.ForeColor = foreColor;
                        return;
                    }
                }
            }
        }

        public override bool CausesValidation
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("CannotSetValidationOnPagerButtons"));
            }
        }
    }
}

