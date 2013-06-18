namespace System.Web.UI.WebControls
{
    using System;
    using System.Web;
    using System.Web.UI;

    [SupportsEventValidation]
    internal sealed class DataControlButton : Button
    {
        private IPostBackContainer _container;

        internal DataControlButton(IPostBackContainer container)
        {
            this._container = container;
        }

        protected sealed override PostBackOptions GetPostBackOptions()
        {
            if (this._container != null)
            {
                PostBackOptions postBackOptions = this._container.GetPostBackOptions(this);
                if (this.Page != null)
                {
                    postBackOptions.ClientSubmit = true;
                }
                return postBackOptions;
            }
            return base.GetPostBackOptions();
        }

        public override bool CausesValidation
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("CannotSetValidationOnDataControlButtons"));
            }
        }

        public override bool UseSubmitBehavior
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}

