namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Web;
    using System.Web.Routing;
    using System.Web.UI;

    [DefaultProperty("RouteKey")]
    public class RouteParameter : Parameter
    {
        public RouteParameter()
        {
        }

        protected RouteParameter(RouteParameter original) : base(original)
        {
            this.RouteKey = original.RouteKey;
        }

        public RouteParameter(string name, string routeKey) : base(name)
        {
            this.RouteKey = routeKey;
        }

        public RouteParameter(string name, DbType dbType, string routeKey) : base(name, dbType)
        {
            this.RouteKey = routeKey;
        }

        public RouteParameter(string name, TypeCode type, string routeKey) : base(name, type)
        {
            this.RouteKey = routeKey;
        }

        protected override Parameter Clone()
        {
            return new RouteParameter(this);
        }

        protected internal override object Evaluate(HttpContext context, Control control)
        {
            if (((context == null) || (context.Request == null)) || (control == null))
            {
                return null;
            }
            RouteData routeData = control.Page.RouteData;
            if (routeData == null)
            {
                return null;
            }
            return routeData.Values[this.RouteKey];
        }

        [WebSysDescription("RouteParameter_RouteKey"), WebCategory("Parameter"), DefaultValue("")]
        public string RouteKey
        {
            get
            {
                object obj2 = base.ViewState["RouteKey"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                if (this.RouteKey != value)
                {
                    base.ViewState["RouteKey"] = value;
                    base.OnParameterChanged();
                }
            }
        }
    }
}

