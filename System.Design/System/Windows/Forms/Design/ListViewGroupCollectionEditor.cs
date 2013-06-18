namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Windows.Forms;

    internal class ListViewGroupCollectionEditor : CollectionEditor
    {
        private object editValue;

        public ListViewGroupCollectionEditor(System.Type type) : base(type)
        {
        }

        protected override object CreateInstance(System.Type itemType)
        {
            ListViewGroup group = (ListViewGroup) base.CreateInstance(itemType);
            group.Name = this.CreateListViewGroupName((ListViewGroupCollection) this.editValue);
            return group;
        }

        private string CreateListViewGroupName(ListViewGroupCollection lvgCollection)
        {
            string str = "ListViewGroup";
            INameCreationService service = base.GetService(typeof(INameCreationService)) as INameCreationService;
            IContainer container = base.GetService(typeof(IContainer)) as IContainer;
            if ((service != null) && (container != null))
            {
                str = service.CreateName(container, typeof(ListViewGroup));
            }
            while (char.IsDigit(str[str.Length - 1]))
            {
                str = str.Substring(0, str.Length - 1);
            }
            int num = 1;
            string str2 = str + num.ToString(CultureInfo.CurrentCulture);
            while (lvgCollection[str2] != null)
            {
                num++;
                str2 = str + num.ToString(CultureInfo.CurrentCulture);
            }
            return str2;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            this.editValue = value;
            object obj2 = base.EditValue(context, provider, value);
            this.editValue = null;
            return obj2;
        }
    }
}

