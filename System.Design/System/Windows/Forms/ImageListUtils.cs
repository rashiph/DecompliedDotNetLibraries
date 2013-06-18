namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    internal static class ImageListUtils
    {
        public static PropertyDescriptor GetImageListProperty(PropertyDescriptor currentComponent, ref object instance)
        {
            if (instance is object[])
            {
                return null;
            }
            PropertyDescriptor descriptor = null;
            object component = instance;
            RelatedImageListAttribute attribute = currentComponent.Attributes[typeof(RelatedImageListAttribute)] as RelatedImageListAttribute;
            if (attribute != null)
            {
                string[] strArray = attribute.RelatedImageList.Split(new char[] { '.' });
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (component == null)
                    {
                        return descriptor;
                    }
                    PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(component)[strArray[i]];
                    if (descriptor2 == null)
                    {
                        return descriptor;
                    }
                    if (i == (strArray.Length - 1))
                    {
                        if (typeof(ImageList).IsAssignableFrom(descriptor2.PropertyType))
                        {
                            instance = component;
                            return descriptor2;
                        }
                    }
                    else
                    {
                        component = descriptor2.GetValue(component);
                    }
                }
            }
            return descriptor;
        }
    }
}

