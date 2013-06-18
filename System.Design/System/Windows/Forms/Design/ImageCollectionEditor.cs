namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing.Design;

    internal class ImageCollectionEditor : CollectionEditor
    {
        public ImageCollectionEditor(Type type) : base(type)
        {
        }

        protected override CollectionEditor.CollectionForm CreateCollectionForm()
        {
            CollectionEditor.CollectionForm form = base.CreateCollectionForm();
            form.Text = System.Design.SR.GetString("ImageCollectionEditorFormText");
            return form;
        }

        protected override object CreateInstance(Type type)
        {
            UITypeEditor editor = (UITypeEditor) TypeDescriptor.GetEditor(typeof(ImageListImage), typeof(UITypeEditor));
            return editor.EditValue(base.Context, null);
        }

        protected override string GetDisplayText(object value)
        {
            string str;
            if (value == null)
            {
                return string.Empty;
            }
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(value)["Name"];
            if (descriptor != null)
            {
                str = (string) descriptor.GetValue(value);
                if ((str != null) && (str.Length > 0))
                {
                    return str;
                }
            }
            if (value is ImageListImage)
            {
                value = ((ImageListImage) value).Image;
            }
            str = TypeDescriptor.GetConverter(value).ConvertToString(value);
            if ((str != null) && (str.Length != 0))
            {
                return str;
            }
            return value.GetType().Name;
        }

        protected override IList GetObjectsFromInstance(object instance)
        {
            ArrayList list = instance as ArrayList;
            if (list != null)
            {
                return list;
            }
            return null;
        }
    }
}

