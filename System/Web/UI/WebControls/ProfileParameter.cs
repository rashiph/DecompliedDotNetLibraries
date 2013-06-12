namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Web;
    using System.Web.UI;

    [DefaultProperty("PropertyName")]
    public class ProfileParameter : Parameter
    {
        public ProfileParameter()
        {
        }

        protected ProfileParameter(ProfileParameter original) : base(original)
        {
            this.PropertyName = original.PropertyName;
        }

        public ProfileParameter(string name, string propertyName) : base(name)
        {
            this.PropertyName = propertyName;
        }

        public ProfileParameter(string name, DbType dbType, string propertyName) : base(name, dbType)
        {
            this.PropertyName = propertyName;
        }

        public ProfileParameter(string name, TypeCode type, string propertyName) : base(name, type)
        {
            this.PropertyName = propertyName;
        }

        protected override Parameter Clone()
        {
            return new ProfileParameter(this);
        }

        protected internal override object Evaluate(HttpContext context, Control control)
        {
            if ((context != null) && (context.Profile != null))
            {
                return DataBinder.Eval(context.Profile, this.PropertyName);
            }
            return null;
        }

        [WebSysDescription("ProfileParameter_PropertyName"), DefaultValue(""), WebCategory("Parameter")]
        public string PropertyName
        {
            get
            {
                object obj2 = base.ViewState["PropertyName"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                if (this.PropertyName != value)
                {
                    base.ViewState["PropertyName"] = value;
                    base.OnParameterChanged();
                }
            }
        }
    }
}

