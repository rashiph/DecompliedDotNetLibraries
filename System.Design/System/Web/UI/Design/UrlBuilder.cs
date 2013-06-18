namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public sealed class UrlBuilder
    {
        private UrlBuilder()
        {
        }

        public static string BuildUrl(IComponent component, Control owner, string initialUrl, string caption, string filter)
        {
            return BuildUrl(component, owner, initialUrl, caption, filter, UrlBuilderOptions.None);
        }

        public static string BuildUrl(IComponent component, Control owner, string initialUrl, string caption, string filter, UrlBuilderOptions options)
        {
            ISite serviceProvider = component.Site;
            if (serviceProvider == null)
            {
                return null;
            }
            return BuildUrl(serviceProvider, owner, initialUrl, caption, filter, options);
        }

        public static string BuildUrl(IServiceProvider serviceProvider, Control owner, string initialUrl, string caption, string filter, UrlBuilderOptions options)
        {
            string baseUrl = string.Empty;
            string str2 = null;
            IDesignerHost host = (IDesignerHost) serviceProvider.GetService(typeof(IDesignerHost));
            if (host != null)
            {
                WebFormsRootDesigner designer = host.GetDesigner(host.RootComponent) as WebFormsRootDesigner;
                if (designer != null)
                {
                    baseUrl = designer.DocumentUrl;
                }
            }
            if (baseUrl.Length == 0)
            {
                IWebFormsDocumentService service = (IWebFormsDocumentService) serviceProvider.GetService(typeof(IWebFormsDocumentService));
                if (service != null)
                {
                    baseUrl = service.DocumentUrl;
                }
            }
            IWebFormsBuilderUIService service2 = (IWebFormsBuilderUIService) serviceProvider.GetService(typeof(IWebFormsBuilderUIService));
            if (service2 != null)
            {
                str2 = service2.BuildUrl(owner, initialUrl, baseUrl, caption, filter, options);
            }
            return str2;
        }
    }
}

