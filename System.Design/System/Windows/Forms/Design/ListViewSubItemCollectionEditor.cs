namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Windows.Forms;

    internal class ListViewSubItemCollectionEditor : CollectionEditor
    {
        private static int count;
        private ListViewItem.ListViewSubItem firstSubItem;

        public ListViewSubItemCollectionEditor(System.Type type) : base(type)
        {
        }

        protected override object CreateInstance(System.Type type)
        {
            object obj2 = base.CreateInstance(type);
            if (obj2 is ListViewItem.ListViewSubItem)
            {
                ((ListViewItem.ListViewSubItem) obj2).Name = System.Design.SR.GetString("ListViewSubItemBaseName") + count++;
            }
            return obj2;
        }

        protected override string GetDisplayText(object value)
        {
            string str;
            if (value == null)
            {
                return string.Empty;
            }
            PropertyDescriptor defaultProperty = TypeDescriptor.GetDefaultProperty(base.CollectionType);
            if ((defaultProperty != null) && (defaultProperty.PropertyType == typeof(string)))
            {
                str = (string) defaultProperty.GetValue(value);
                if ((str != null) && (str.Length > 0))
                {
                    return str;
                }
            }
            str = TypeDescriptor.GetConverter(value).ConvertToString(value);
            if ((str != null) && (str.Length != 0))
            {
                return str;
            }
            return value.GetType().Name;
        }

        protected override object[] GetItems(object editValue)
        {
            ListViewItem.ListViewSubItemCollection items = (ListViewItem.ListViewSubItemCollection) editValue;
            object[] array = new object[items.Count];
            ((ICollection) items).CopyTo(array, 0);
            if (array.Length > 0)
            {
                this.firstSubItem = items[0];
                object[] destinationArray = new object[array.Length - 1];
                Array.Copy(array, 1, destinationArray, 0, destinationArray.Length);
                array = destinationArray;
            }
            return array;
        }

        protected override object SetItems(object editValue, object[] value)
        {
            IList list = editValue as IList;
            list.Clear();
            list.Add(this.firstSubItem);
            for (int i = 0; i < value.Length; i++)
            {
                list.Add(value[i]);
            }
            return editValue;
        }
    }
}

