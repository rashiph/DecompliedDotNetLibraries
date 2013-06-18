namespace System.Drawing.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class IconEditor : UITypeEditor
    {
        internal FileDialog fileDialog;
        internal static System.Type[] imageExtenders = new System.Type[0];

        protected static string CreateExtensionsString(string[] extensions, string sep)
        {
            if ((extensions == null) || (extensions.Length == 0))
            {
                return null;
            }
            string str = null;
            for (int i = 0; i < (extensions.Length - 1); i++)
            {
                str = str + "*." + extensions[i] + sep;
            }
            return (str + "*." + extensions[extensions.Length - 1]);
        }

        protected static string CreateFilterEntry(IconEditor e)
        {
            string fileDialogDescription = e.GetFileDialogDescription();
            string str2 = CreateExtensionsString(e.GetExtensions(), ",");
            string str3 = CreateExtensionsString(e.GetExtensions(), ";");
            return (fileDialogDescription + "(" + str2 + ")|" + str3);
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if ((provider != null) && (((IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService))) != null))
            {
                if (this.fileDialog == null)
                {
                    this.fileDialog = new OpenFileDialog();
                    string str = CreateFilterEntry(this);
                    for (int i = 0; i < imageExtenders.Length; i++)
                    {
                    }
                    this.fileDialog.Filter = str;
                }
                IntPtr focus = System.Drawing.Design.UnsafeNativeMethods.GetFocus();
                try
                {
                    if (this.fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        FileStream stream = new FileStream(this.fileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        value = this.LoadFromStream(stream);
                    }
                }
                finally
                {
                    if (focus != IntPtr.Zero)
                    {
                        System.Drawing.Design.UnsafeNativeMethods.SetFocus(new HandleRef(null, focus));
                    }
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        protected virtual string[] GetExtensions()
        {
            return new string[] { "ico" };
        }

        protected virtual string GetFileDialogDescription()
        {
            return System.Drawing.Design.SR.GetString("iconFileDescription");
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        protected virtual Icon LoadFromStream(Stream stream)
        {
            return new Icon(stream);
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            Icon icon = e.Value as Icon;
            if (icon != null)
            {
                Size size = icon.Size;
                Rectangle bounds = e.Bounds;
                if (icon.Width < bounds.Width)
                {
                    bounds.X = (bounds.Width - icon.Width) / 2;
                    bounds.Width = icon.Width;
                }
                if (icon.Height < bounds.Height)
                {
                    bounds.X = (bounds.Height - icon.Height) / 2;
                    bounds.Height = icon.Height;
                }
                e.Graphics.DrawIcon(icon, bounds);
            }
        }
    }
}

