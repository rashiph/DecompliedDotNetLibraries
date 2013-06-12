namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Web;
    using System.Web.UI;

    [DefaultProperty("FormField")]
    public class FormParameter : Parameter
    {
        public FormParameter()
        {
        }

        protected FormParameter(FormParameter original) : base(original)
        {
            this.FormField = original.FormField;
        }

        public FormParameter(string name, string formField) : base(name)
        {
            this.FormField = formField;
        }

        public FormParameter(string name, DbType dbType, string formField) : base(name, dbType)
        {
            this.FormField = formField;
        }

        public FormParameter(string name, TypeCode type, string formField) : base(name, type)
        {
            this.FormField = formField;
        }

        protected override Parameter Clone()
        {
            return new FormParameter(this);
        }

        protected internal override object Evaluate(HttpContext context, Control control)
        {
            if ((context != null) && (context.Request != null))
            {
                return context.Request.Form[this.FormField];
            }
            return null;
        }

        [DefaultValue(""), WebCategory("Parameter"), WebSysDescription("FormParameter_FormField")]
        public string FormField
        {
            get
            {
                object obj2 = base.ViewState["FormField"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                if (this.FormField != value)
                {
                    base.ViewState["FormField"] = value;
                    base.OnParameterChanged();
                }
            }
        }
    }
}

