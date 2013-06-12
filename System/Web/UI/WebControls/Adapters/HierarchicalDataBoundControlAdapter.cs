namespace System.Web.UI.WebControls.Adapters
{
    using System;
    using System.Web.UI.WebControls;

    public class HierarchicalDataBoundControlAdapter : WebControlAdapter
    {
        protected internal virtual void PerformDataBinding()
        {
            this.Control.PerformDataBinding();
        }

        protected HierarchicalDataBoundControl Control
        {
            get
            {
                return (HierarchicalDataBoundControl) base.Control;
            }
        }
    }
}

