namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI;

    [PersistChildren(false), Designer("System.Web.UI.Design.WebControls.SubstitutionDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ParseChildren(true), DefaultProperty("MethodName")]
    public class Substitution : Control
    {
        protected override ControlCollection CreateControlCollection()
        {
            return new EmptyControlCollection(this);
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        private HttpResponseSubstitutionCallback GetDelegate(Type targetType, string methodName)
        {
            return (HttpResponseSubstitutionCallback) Delegate.CreateDelegate(typeof(HttpResponseSubstitutionCallback), targetType, methodName);
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            for (Control control = this.Parent; control != null; control = control.Parent)
            {
                if (control is BasePartialCachingControl)
                {
                    throw new HttpException(System.Web.SR.GetString("Substitution_CannotBeInCachedControl"));
                }
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            this.RenderMarkup(writer);
        }

        internal void RenderMarkup(HtmlTextWriter writer)
        {
            if (this.MethodName.Length != 0)
            {
                TemplateControl templateControl = base.TemplateControl;
                if (templateControl != null)
                {
                    HttpResponseSubstitutionCallback callback = null;
                    try
                    {
                        callback = this.GetDelegate(templateControl.GetType(), this.MethodName);
                    }
                    catch
                    {
                    }
                    if (callback == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Substitution_BadMethodName", new object[] { this.MethodName }));
                    }
                    this.Page.Response.WriteSubstitution(callback);
                }
            }
        }

        [WebCategory("Behavior"), DefaultValue(""), WebSysDescription("Substitution_MethodNameDescr")]
        public virtual string MethodName
        {
            get
            {
                string str = this.ViewState["MethodName"] as string;
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["MethodName"] = value;
            }
        }
    }
}

