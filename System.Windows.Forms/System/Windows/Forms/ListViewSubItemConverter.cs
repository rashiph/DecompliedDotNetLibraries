namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;

    internal class ListViewSubItemConverter : ExpandableObjectConverter
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
            if ((destinationType == typeof(InstanceDescriptor)) && (value is ListViewItem.ListViewSubItem))
            {
                ConstructorInfo constructor;
                ListViewItem.ListViewSubItem item = (ListViewItem.ListViewSubItem) value;
                if (item.CustomStyle)
                {
                    constructor = typeof(ListViewItem.ListViewSubItem).GetConstructor(new System.Type[] { typeof(ListViewItem), typeof(string), typeof(Color), typeof(Color), typeof(Font) });
                    if (constructor != null)
                    {
                        object[] arguments = new object[5];
                        arguments[1] = item.Text;
                        arguments[2] = item.ForeColor;
                        arguments[3] = item.BackColor;
                        arguments[4] = item.Font;
                        return new InstanceDescriptor(constructor, arguments, true);
                    }
                }
                constructor = typeof(ListViewItem.ListViewSubItem).GetConstructor(new System.Type[] { typeof(ListViewItem), typeof(string) });
                if (constructor != null)
                {
                    object[] objArray2 = new object[2];
                    objArray2[1] = item.Text;
                    return new InstanceDescriptor(constructor, objArray2, true);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

