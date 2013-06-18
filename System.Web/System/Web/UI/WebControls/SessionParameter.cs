namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Web;
    using System.Web.UI;

    [DefaultProperty("SessionField")]
    public class SessionParameter : Parameter
    {
        public SessionParameter()
        {
        }

        protected SessionParameter(SessionParameter original) : base(original)
        {
            this.SessionField = original.SessionField;
        }

        public SessionParameter(string name, string sessionField) : base(name)
        {
            this.SessionField = sessionField;
        }

        public SessionParameter(string name, DbType dbType, string sessionField) : base(name, dbType)
        {
            this.SessionField = sessionField;
        }

        public SessionParameter(string name, TypeCode type, string sessionField) : base(name, type)
        {
            this.SessionField = sessionField;
        }

        protected override Parameter Clone()
        {
            return new SessionParameter(this);
        }

        protected internal override object Evaluate(HttpContext context, Control control)
        {
            if ((context != null) && (context.Session != null))
            {
                return context.Session[this.SessionField];
            }
            return null;
        }

        [WebCategory("Parameter"), WebSysDescription("SessionParameter_SessionField"), DefaultValue("")]
        public string SessionField
        {
            get
            {
                object obj2 = base.ViewState["SessionField"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                if (this.SessionField != value)
                {
                    base.ViewState["SessionField"] = value;
                    base.OnParameterChanged();
                }
            }
        }
    }
}

