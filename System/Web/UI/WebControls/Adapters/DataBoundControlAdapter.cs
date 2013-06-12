namespace System.Web.UI.WebControls.Adapters
{
    using System;
    using System.Collections;
    using System.Web.UI.WebControls;

    public class DataBoundControlAdapter : WebControlAdapter
    {
        protected internal virtual void PerformDataBinding(IEnumerable data)
        {
            this.Control.PerformDataBinding(data);
        }

        protected DataBoundControl Control
        {
            get
            {
                return (DataBoundControl) base.Control;
            }
        }
    }
}

