namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Windows.Forms;

    internal class ImageIndexEditor : UITypeEditor
    {
        protected ImageList currentImageList;
        protected WeakReference currentImageListPropRef;
        protected object currentInstance;
        protected UITypeEditor imageEditor = ((UITypeEditor) TypeDescriptor.GetEditor(typeof(Image), typeof(UITypeEditor)));
        protected string imageListPropertyName;
        protected string parentImageListProperty = "Parent";

        protected virtual Image GetImage(ITypeDescriptorContext context, int index, string key, bool useIntIndex)
        {
            Image image = null;
            object instance = context.Instance;
            if (!(instance is object[]))
            {
                if ((index < 0) && (key == null))
                {
                    return image;
                }
                PropertyDescriptor target = null;
                if (this.currentImageListPropRef != null)
                {
                    target = this.currentImageListPropRef.Target as PropertyDescriptor;
                }
                if (((this.currentImageList == null) || (instance != this.currentInstance)) || ((target != null) && (((ImageList) target.GetValue(this.currentInstance)) != this.currentImageList)))
                {
                    this.currentInstance = instance;
                    PropertyDescriptor imageListProperty = System.Windows.Forms.ImageListUtils.GetImageListProperty(context.PropertyDescriptor, ref instance);
                    while ((instance != null) && (imageListProperty == null))
                    {
                        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(instance);
                        foreach (PropertyDescriptor descriptor3 in properties)
                        {
                            if (typeof(ImageList).IsAssignableFrom(descriptor3.PropertyType))
                            {
                                imageListProperty = descriptor3;
                                break;
                            }
                        }
                        if (imageListProperty == null)
                        {
                            PropertyDescriptor descriptor4 = properties[this.ParentImageListProperty];
                            if (descriptor4 != null)
                            {
                                instance = descriptor4.GetValue(instance);
                                continue;
                            }
                            instance = null;
                        }
                    }
                    if (imageListProperty != null)
                    {
                        this.currentImageList = (ImageList) imageListProperty.GetValue(instance);
                        this.currentImageListPropRef = new WeakReference(imageListProperty);
                        this.currentInstance = instance;
                    }
                }
                if (this.currentImageList != null)
                {
                    if (useIntIndex)
                    {
                        if ((this.currentImageList != null) && (index < this.currentImageList.Images.Count))
                        {
                            index = (index > 0) ? index : 0;
                            image = this.currentImageList.Images[index];
                        }
                        return image;
                    }
                    return this.currentImageList.Images[key];
                }
            }
            return null;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return ((this.imageEditor != null) && this.imageEditor.GetPaintValueSupported(context));
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (this.ImageEditor != null)
            {
                Image image = null;
                if (e.Value is int)
                {
                    image = this.GetImage(e.Context, (int) e.Value, null, true);
                }
                else if (e.Value is string)
                {
                    image = this.GetImage(e.Context, -1, (string) e.Value, false);
                }
                if (image != null)
                {
                    this.ImageEditor.PaintValue(new PaintValueEventArgs(e.Context, image, e.Graphics, e.Bounds));
                }
            }
        }

        internal UITypeEditor ImageEditor
        {
            get
            {
                return this.imageEditor;
            }
        }

        internal string ParentImageListProperty
        {
            get
            {
                return this.parentImageListProperty;
            }
        }
    }
}

