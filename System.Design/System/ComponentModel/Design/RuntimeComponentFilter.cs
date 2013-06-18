namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    internal static class RuntimeComponentFilter
    {
        public static void FilterProperties(IDictionary properties, ICollection makeReadWrite, ICollection makeBrowsable)
        {
            FilterProperties(properties, makeReadWrite, makeBrowsable, null);
        }

        public static void FilterProperties(IDictionary properties, ICollection makeReadWrite, ICollection makeBrowsable, bool[] browsableSettings)
        {
            if (makeReadWrite != null)
            {
                foreach (string str in makeReadWrite)
                {
                    PropertyDescriptor oldPropertyDescriptor = properties[str] as PropertyDescriptor;
                    if (oldPropertyDescriptor != null)
                    {
                        properties[str] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { ReadOnlyAttribute.No });
                    }
                }
            }
            if (makeBrowsable != null)
            {
                int index = -1;
                foreach (string str2 in makeBrowsable)
                {
                    PropertyDescriptor descriptor2 = properties[str2] as PropertyDescriptor;
                    index++;
                    if (descriptor2 != null)
                    {
                        Attribute yes;
                        if ((browsableSettings == null) || browsableSettings[index])
                        {
                            yes = BrowsableAttribute.Yes;
                        }
                        else
                        {
                            yes = BrowsableAttribute.No;
                        }
                        properties[str2] = TypeDescriptor.CreateProperty(descriptor2.ComponentType, descriptor2, new Attribute[] { yes });
                    }
                }
            }
        }
    }
}

