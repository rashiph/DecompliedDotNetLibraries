namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class UrlEditor : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if ((provider != null) && (((IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService))) != null))
            {
                string initialUrl = (string) value;
                string caption = this.Caption;
                string filter = this.Filter;
                initialUrl = UrlBuilder.BuildUrl(provider, null, initialUrl, caption, filter, this.Options);
                if (initialUrl != null)
                {
                    value = initialUrl;
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        protected virtual string Caption
        {
            get
            {
                return System.Design.SR.GetString("UrlPicker_DefaultCaption");
            }
        }

        protected virtual string Filter
        {
            get
            {
                return System.Design.SR.GetString("UrlPicker_DefaultFilter");
            }
        }

        protected virtual UrlBuilderOptions Options
        {
            get
            {
                return UrlBuilderOptions.None;
            }
        }
    }
}

