namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [Obsolete("Use of this type is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202"), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public sealed class TemplateEditingService : ITemplateEditingService, IDisposable
    {
        private IDesignerHost designerHost;

        public TemplateEditingService(IDesignerHost designerHost)
        {
            if (designerHost == null)
            {
                throw new ArgumentNullException("designerHost");
            }
            this.designerHost = designerHost;
        }

        public ITemplateEditingFrame CreateFrame(TemplatedControlDesigner designer, string frameName, string[] templateNames)
        {
            return this.CreateFrame(designer, frameName, templateNames, null, null);
        }

        public ITemplateEditingFrame CreateFrame(TemplatedControlDesigner designer, string frameName, string[] templateNames, Style controlStyle, Style[] templateStyles)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }
            if ((frameName == null) || (frameName.Length == 0))
            {
                throw new ArgumentNullException("frameName");
            }
            if ((templateNames == null) || (templateNames.Length == 0))
            {
                throw new ArgumentException("templateNames");
            }
            if ((templateStyles != null) && (templateStyles.Length != templateNames.Length))
            {
                throw new ArgumentException("templateStyles");
            }
            frameName = this.CreateFrameName(frameName);
            return new TemplateEditingFrame(designer, frameName, templateNames, controlStyle, templateStyles);
        }

        private string CreateFrameName(string frameName)
        {
            int index = frameName.IndexOf('&');
            if (index < 0)
            {
                return frameName;
            }
            if (index == 0)
            {
                return frameName.Substring(index + 1);
            }
            return (frameName.Substring(0, index) + frameName.Substring(index + 1));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.designerHost = null;
            }
        }

        ~TemplateEditingService()
        {
            this.Dispose(false);
        }

        public string GetContainingTemplateName(Control control)
        {
            string str = string.Empty;
            HtmlControlDesigner designer = (HtmlControlDesigner) this.designerHost.GetDesigner(control);
            if (designer != null)
            {
                System.Design.NativeMethods.IHTMLElement designTimeElement = (System.Design.NativeMethods.IHTMLElement) designer.BehaviorInternal.DesignTimeElement;
                if (designTimeElement == null)
                {
                    return str;
                }
                object[] pvars = new object[1];
                for (System.Design.NativeMethods.IHTMLElement element3 = designTimeElement.GetParentElement(); element3 != null; element3 = element3.GetParentElement())
                {
                    element3.GetAttribute("templatename", 0, pvars);
                    if ((pvars[0] != null) && (pvars[0].GetType() == typeof(string)))
                    {
                        return pvars[0].ToString();
                    }
                }
            }
            return str;
        }

        public bool SupportsNestedTemplateEditing
        {
            get
            {
                return false;
            }
        }
    }
}

