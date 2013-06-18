namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public sealed class ColorBuilder
    {
        private ColorBuilder()
        {
        }

        public static string BuildColor(IComponent component, Control owner, string initialColor)
        {
            string str = null;
            ISite site = component.Site;
            if (site == null)
            {
                return null;
            }
            if (site != null)
            {
                IWebFormsBuilderUIService service = (IWebFormsBuilderUIService) site.GetService(typeof(IWebFormsBuilderUIService));
                if (service != null)
                {
                    str = service.BuildColor(owner, initialColor);
                }
            }
            return str;
        }
    }
}

