namespace System.Web.UI.WebControls
{
    using System;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;

    public class ViewCollection : ControlCollection
    {
        public ViewCollection(Control owner) : base(owner)
        {
        }

        public override void Add(Control v)
        {
            if (!(v is System.Web.UI.WebControls.View))
            {
                throw new ArgumentException(System.Web.SR.GetString("ViewCollection_must_contain_view"));
            }
            base.Add(v);
        }

        public override void AddAt(int index, Control v)
        {
            if (!(v is System.Web.UI.WebControls.View))
            {
                throw new ArgumentException(System.Web.SR.GetString("ViewCollection_must_contain_view"));
            }
            base.AddAt(index, v);
        }

        public System.Web.UI.WebControls.View this[int i]
        {
            get
            {
                return (System.Web.UI.WebControls.View) base[i];
            }
        }
    }
}

