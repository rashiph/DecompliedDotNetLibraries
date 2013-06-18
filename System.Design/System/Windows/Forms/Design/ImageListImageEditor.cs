namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing.Design;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class ImageListImageEditor : ImageEditor
    {
        private OpenFileDialog fileDialog;
        internal static System.Type[] imageExtenders = new System.Type[] { typeof(BitmapEditor) };

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            ArrayList list = new ArrayList();
            if (provider == null)
            {
                return value;
            }
            if (((IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService))) != null)
            {
                if (this.fileDialog == null)
                {
                    this.fileDialog = new OpenFileDialog();
                    this.fileDialog.Multiselect = true;
                    string str = ImageEditor.CreateFilterEntry(this);
                    for (int i = 0; i < this.GetImageExtenders().Length; i++)
                    {
                        ImageEditor o = (ImageEditor) Activator.CreateInstance(this.GetImageExtenders()[i], BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, null);
                        System.Type type = base.GetType();
                        System.Type type2 = o.GetType();
                        if ((!type.Equals(type2) && (o != null)) && type.IsInstanceOfType(o))
                        {
                            str = str + "|" + ImageEditor.CreateFilterEntry(o);
                        }
                    }
                    this.fileDialog.Filter = str;
                }
                IntPtr focus = System.Design.UnsafeNativeMethods.GetFocus();
                try
                {
                    if (this.fileDialog.ShowDialog() != DialogResult.OK)
                    {
                        return list;
                    }
                    foreach (string str2 in this.fileDialog.FileNames)
                    {
                        FileStream stream = new FileStream(str2, FileMode.Open, FileAccess.Read, FileShare.Read);
                        ImageListImage image = this.LoadImageFromStream(stream, str2.EndsWith(".ico"));
                        image.Name = Path.GetFileName(str2);
                        list.Add(image);
                    }
                }
                finally
                {
                    if (focus != IntPtr.Zero)
                    {
                        System.Design.UnsafeNativeMethods.SetFocus(new HandleRef(null, focus));
                    }
                }
            }
            return list;
        }

        protected override string GetFileDialogDescription()
        {
            return System.Design.SR.GetString("imageFileDescription");
        }

        protected override System.Type[] GetImageExtenders()
        {
            return imageExtenders;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        private ImageListImage LoadImageFromStream(Stream stream, bool imageIsIcon)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int) stream.Length);
            MemoryStream stream2 = new MemoryStream(buffer);
            return ImageListImage.ImageListImageFromStream(stream2, imageIsIcon);
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Value is ImageListImage)
            {
                e = new PaintValueEventArgs(e.Context, ((ImageListImage) e.Value).Image, e.Graphics, e.Bounds);
            }
            base.PaintValue(e);
        }
    }
}

