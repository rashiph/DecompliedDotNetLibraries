namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Security.Permissions;
    using System.Web.UI;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public sealed class ControlPersister
    {
        private ControlPersister()
        {
        }

        public static string PersistControl(Control control)
        {
            return ControlSerializer.SerializeControl(control);
        }

        public static void PersistControl(TextWriter sw, Control control)
        {
            ControlSerializer.SerializeControl(control, sw);
        }

        public static string PersistControl(Control control, IDesignerHost host)
        {
            return ControlSerializer.SerializeControl(control, host);
        }

        public static void PersistControl(TextWriter sw, Control control, IDesignerHost host)
        {
            ControlSerializer.SerializeControl(control, host, sw);
        }

        public static string PersistInnerProperties(object component, IDesignerHost host)
        {
            return ControlSerializer.SerializeInnerProperties(component, host);
        }

        public static void PersistInnerProperties(TextWriter sw, object component, IDesignerHost host)
        {
            ControlSerializer.SerializeInnerProperties(component, host, sw);
        }

        public static string PersistTemplate(ITemplate template, IDesignerHost host)
        {
            return ControlSerializer.SerializeTemplate(template, host);
        }

        public static void PersistTemplate(TextWriter writer, ITemplate template, IDesignerHost host)
        {
            ControlSerializer.SerializeTemplate(template, writer, host);
        }
    }
}

