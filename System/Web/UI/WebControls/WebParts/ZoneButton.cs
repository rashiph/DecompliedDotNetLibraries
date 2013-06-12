namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [SupportsEventValidation]
    internal sealed class ZoneButton : Button
    {
        private string _eventArgument;
        private WebZone _owner;

        public ZoneButton(WebZone owner, string eventArgument)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            this._owner = owner;
            this._eventArgument = eventArgument;
        }

        protected override PostBackOptions GetPostBackOptions()
        {
            if (!string.IsNullOrEmpty(this._eventArgument) && (this._owner.Page != null))
            {
                return new PostBackOptions(this._owner, this._eventArgument) { ClientSubmit = true };
            }
            return base.GetPostBackOptions();
        }

        [DefaultValue(false)]
        public override bool UseSubmitBehavior
        {
            get
            {
                return false;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }
    }
}

