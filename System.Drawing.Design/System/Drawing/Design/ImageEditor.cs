namespace System.Drawing.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ImageEditor : UITypeEditor
    {
        internal FileDialog fileDialog;
        internal static System.Type[] imageExtenders = new System.Type[] { typeof(BitmapEditor), typeof(MetafileEditor) };

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

        protected static string CreateFilterEntry(ImageEditor e)
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
                    for (int i = 0; i < this.GetImageExtenders().Length; i++)
                    {
                        ImageEditor o = (ImageEditor) Activator.CreateInstance(this.GetImageExtenders()[i], BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, null);
                        System.Type type = base.GetType();
                        System.Type type2 = o.GetType();
                        if ((!type.Equals(type2) && (o != null)) && type.IsInstanceOfType(o))
                        {
                            str = str + "|" + CreateFilterEntry(o);
                        }
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
            ArrayList list = new ArrayList();
            for (int i = 0; i < this.GetImageExtenders().Length; i++)
            {
                ImageEditor editor = (ImageEditor) Activator.CreateInstance(this.GetImageExtenders()[i], BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, null);
                if (!editor.GetType().Equals(typeof(ImageEditor)))
                {
                    list.AddRange(new ArrayList(editor.GetExtensions()));
                }
            }
            return (string[]) list.ToArray(typeof(string));
        }

        protected virtual string GetFileDialogDescription()
        {
            return System.Drawing.Design.SR.GetString("imageFileDescription");
        }

        protected virtual System.Type[] GetImageExtenders()
        {
            return imageExtenders;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        protected virtual Image LoadFromStream(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int) stream.Length);
            MemoryStream stream2 = new MemoryStream(buffer);
            return Image.FromStream(stream2);
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            Image image = e.Value as Image;
            if (image != null)
            {
                Rectangle bounds = e.Bounds;
                bounds.Width--;
                bounds.Height--;
                e.Graphics.DrawRectangle(SystemPens.WindowFrame, bounds);
                e.Graphics.DrawImage(image, e.Bounds);
            }
        }
    }
}

