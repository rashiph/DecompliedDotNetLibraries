namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Web.UI;

    public abstract class WebPartTransformer
    {
        protected WebPartTransformer()
        {
        }

        public virtual Control CreateConfigurationControl()
        {
            return null;
        }

        protected internal virtual void LoadConfigurationState(object savedState)
        {
        }

        protected internal virtual object SaveConfigurationState()
        {
            return null;
        }

        public abstract object Transform(object providerData);
    }
}

