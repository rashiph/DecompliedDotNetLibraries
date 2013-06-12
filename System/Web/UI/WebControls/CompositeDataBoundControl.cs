namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Web.UI;

    public abstract class CompositeDataBoundControl : DataBoundControl, INamingContainer
    {
        internal const string ItemCountViewStateKey = "_!ItemCount";

        protected CompositeDataBoundControl()
        {
        }

        protected internal override void CreateChildControls()
        {
            this.Controls.Clear();
            object obj2 = this.ViewState["_!ItemCount"];
            if ((obj2 == null) && base.RequiresDataBinding)
            {
                this.EnsureDataBound();
            }
            if ((obj2 != null) && (((int) obj2) != -1))
            {
                DummyDataSource dataSource = new DummyDataSource((int) obj2);
                this.CreateChildControls(dataSource, false);
                base.ClearChildViewState();
            }
        }

        protected abstract int CreateChildControls(IEnumerable dataSource, bool dataBinding);
        protected internal override void PerformDataBinding(IEnumerable data)
        {
            base.PerformDataBinding(data);
            this.Controls.Clear();
            base.ClearChildViewState();
            this.TrackViewState();
            int num = this.CreateChildControls(data, true);
            base.ChildControlsCreated = true;
            this.ViewState["_!ItemCount"] = num;
        }

        public override ControlCollection Controls
        {
            get
            {
                this.EnsureChildControls();
                return base.Controls;
            }
        }
    }
}

