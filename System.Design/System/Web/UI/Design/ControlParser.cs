namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Security.Permissions;
    using System.Web.UI;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public sealed class ControlParser
    {
        private ControlParser()
        {
        }

        public static Control ParseControl(IDesignerHost designerHost, string controlText)
        {
            if (designerHost == null)
            {
                throw new ArgumentNullException("designerHost");
            }
            if ((controlText == null) || (controlText.Length == 0))
            {
                throw new ArgumentNullException("controlText");
            }
            return ControlSerializer.DeserializeControl(controlText, designerHost);
        }

        internal static Control ParseControl(IDesignerHost designerHost, string controlText, bool applyTheme)
        {
            if (designerHost == null)
            {
                throw new ArgumentNullException("designerHost");
            }
            if ((controlText == null) || (controlText.Length == 0))
            {
                throw new ArgumentNullException("controlText");
            }
            return ControlSerializer.DeserializeControlInternal(controlText, designerHost, applyTheme);
        }

        public static Control ParseControl(IDesignerHost designerHost, string controlText, string directives)
        {
            if (designerHost == null)
            {
                throw new ArgumentNullException("designerHost");
            }
            if ((controlText == null) || (controlText.Length == 0))
            {
                throw new ArgumentNullException("controlText");
            }
            if ((directives != null) && (directives.Length != 0))
            {
                controlText = directives + controlText;
            }
            return ControlSerializer.DeserializeControl(controlText, designerHost);
        }

        public static Control[] ParseControls(IDesignerHost designerHost, string controlText)
        {
            if (designerHost == null)
            {
                throw new ArgumentNullException("designerHost");
            }
            if ((controlText == null) || (controlText.Length == 0))
            {
                throw new ArgumentNullException("controlText");
            }
            return ControlSerializer.DeserializeControls(controlText, designerHost);
        }

        public static ITemplate ParseTemplate(IDesignerHost designerHost, string templateText)
        {
            if (designerHost == null)
            {
                throw new ArgumentNullException("designerHost");
            }
            return ControlSerializer.DeserializeTemplate(templateText, designerHost);
        }

        public static ITemplate ParseTemplate(IDesignerHost designerHost, string templateText, string directives)
        {
            if (designerHost == null)
            {
                throw new ArgumentNullException("designerHost");
            }
            return ControlSerializer.DeserializeTemplate(templateText, designerHost);
        }
    }
}

