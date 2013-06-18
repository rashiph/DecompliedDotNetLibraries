namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;

    public class ListViewItemConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((destinationType == typeof(InstanceDescriptor)) && (value is ListViewItem))
            {
                ConstructorInfo constructor;
                ListViewItem item = (ListViewItem) value;
                for (int i = 1; i < item.SubItems.Count; i++)
                {
                    if (item.SubItems[i].CustomStyle)
                    {
                        if (string.IsNullOrEmpty(item.ImageKey))
                        {
                            constructor = typeof(ListViewItem).GetConstructor(new System.Type[] { typeof(ListViewItem.ListViewSubItem[]), typeof(int) });
                            if (constructor != null)
                            {
                                ListViewItem.ListViewSubItem[] array = new ListViewItem.ListViewSubItem[item.SubItems.Count];
                                ((ICollection) item.SubItems).CopyTo(array, 0);
                                return new InstanceDescriptor(constructor, new object[] { array, item.ImageIndex }, false);
                            }
                        }
                        else
                        {
                            constructor = typeof(ListViewItem).GetConstructor(new System.Type[] { typeof(ListViewItem.ListViewSubItem[]), typeof(string) });
                            if (constructor != null)
                            {
                                ListViewItem.ListViewSubItem[] itemArray = new ListViewItem.ListViewSubItem[item.SubItems.Count];
                                ((ICollection) item.SubItems).CopyTo(itemArray, 0);
                                return new InstanceDescriptor(constructor, new object[] { itemArray, item.ImageKey }, false);
                            }
                        }
                        break;
                    }
                }
                string[] strArray = new string[item.SubItems.Count];
                for (int j = 0; j < strArray.Length; j++)
                {
                    strArray[j] = item.SubItems[j].Text;
                }
                if (item.SubItems[0].CustomStyle)
                {
                    if (!string.IsNullOrEmpty(item.ImageKey))
                    {
                        constructor = typeof(ListViewItem).GetConstructor(new System.Type[] { typeof(string[]), typeof(string), typeof(Color), typeof(Color), typeof(Font) });
                        if (constructor != null)
                        {
                            return new InstanceDescriptor(constructor, new object[] { strArray, item.ImageKey, item.SubItems[0].CustomForeColor ? item.ForeColor : Color.Empty, item.SubItems[0].CustomBackColor ? item.BackColor : Color.Empty, item.SubItems[0].CustomFont ? item.Font : null }, false);
                        }
                    }
                    else
                    {
                        constructor = typeof(ListViewItem).GetConstructor(new System.Type[] { typeof(string[]), typeof(int), typeof(Color), typeof(Color), typeof(Font) });
                        if (constructor != null)
                        {
                            return new InstanceDescriptor(constructor, new object[] { strArray, item.ImageIndex, item.SubItems[0].CustomForeColor ? item.ForeColor : Color.Empty, item.SubItems[0].CustomBackColor ? item.BackColor : Color.Empty, item.SubItems[0].CustomFont ? item.Font : null }, false);
                        }
                    }
                }
                if (((item.ImageIndex == -1) && string.IsNullOrEmpty(item.ImageKey)) && (item.SubItems.Count <= 1))
                {
                    constructor = typeof(ListViewItem).GetConstructor(new System.Type[] { typeof(string) });
                    if (constructor != null)
                    {
                        return new InstanceDescriptor(constructor, new object[] { item.Text }, false);
                    }
                }
                if (item.SubItems.Count <= 1)
                {
                    if (!string.IsNullOrEmpty(item.ImageKey))
                    {
                        constructor = typeof(ListViewItem).GetConstructor(new System.Type[] { typeof(string), typeof(string) });
                        if (constructor != null)
                        {
                            return new InstanceDescriptor(constructor, new object[] { item.Text, item.ImageKey }, false);
                        }
                    }
                    else
                    {
                        constructor = typeof(ListViewItem).GetConstructor(new System.Type[] { typeof(string), typeof(int) });
                        if (constructor != null)
                        {
                            return new InstanceDescriptor(constructor, new object[] { item.Text, item.ImageIndex }, false);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(item.ImageKey))
                {
                    constructor = typeof(ListViewItem).GetConstructor(new System.Type[] { typeof(string[]), typeof(string) });
                    if (constructor != null)
                    {
                        return new InstanceDescriptor(constructor, new object[] { strArray, item.ImageKey }, false);
                    }
                }
                else
                {
                    constructor = typeof(ListViewItem).GetConstructor(new System.Type[] { typeof(string[]), typeof(int) });
                    if (constructor != null)
                    {
                        return new InstanceDescriptor(constructor, new object[] { strArray, item.ImageIndex }, false);
                    }
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

