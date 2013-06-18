namespace System.Web.UI.Design.Util
{
    using System;
    using System.ComponentModel.Design;
    using System.Web.UI.Design;

    internal class UrlPath
    {
        private UrlPath()
        {
        }

        private static bool IsAbsolutePhysicalPath(string path)
        {
            if ((path == null) || (path.Length < 3))
            {
                return false;
            }
            return (path.StartsWith(@"\\", StringComparison.Ordinal) || ((char.IsLetter(path[0]) && (path[1] == ':')) && (path[2] == '\\')));
        }

        internal static string MapPath(IServiceProvider serviceProvider, string path)
        {
            if (path.Length != 0)
            {
                if (IsAbsolutePhysicalPath(path))
                {
                    return path;
                }
                WebFormsRootDesigner designer = null;
                if (serviceProvider != null)
                {
                    IDesignerHost service = (IDesignerHost) serviceProvider.GetService(typeof(IDesignerHost));
                    if ((service != null) && (service.RootComponent != null))
                    {
                        designer = service.GetDesigner(service.RootComponent) as WebFormsRootDesigner;
                        if (designer != null)
                        {
                            string appRelativeUrl = designer.ResolveUrl(path);
                            IWebApplication application = (IWebApplication) serviceProvider.GetService(typeof(IWebApplication));
                            if (application != null)
                            {
                                IProjectItem projectItemFromUrl = application.GetProjectItemFromUrl(appRelativeUrl);
                                if (projectItemFromUrl != null)
                                {
                                    return projectItemFromUrl.PhysicalPath;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}

