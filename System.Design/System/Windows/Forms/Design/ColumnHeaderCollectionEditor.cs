namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Windows.Forms;

    internal class ColumnHeaderCollectionEditor : CollectionEditor
    {
        public ColumnHeaderCollectionEditor(System.Type type) : base(type)
        {
        }

        internal override void OnItemRemoving(object item)
        {
            ListView instance = base.Context.Instance as ListView;
            if (instance != null)
            {
                ColumnHeader column = item as ColumnHeader;
                if (column != null)
                {
                    IComponentChangeService service = base.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                    PropertyDescriptor member = null;
                    if (service != null)
                    {
                        member = TypeDescriptor.GetProperties(base.Context.Instance)["Columns"];
                        service.OnComponentChanging(base.Context.Instance, member);
                    }
                    instance.Columns.Remove(column);
                    if ((service != null) && (member != null))
                    {
                        service.OnComponentChanged(base.Context.Instance, member, null, null);
                    }
                }
            }
        }

        protected override object SetItems(object editValue, object[] value)
        {
            if (editValue != null)
            {
                ListView.ColumnHeaderCollection headers = editValue as ListView.ColumnHeaderCollection;
                if (editValue != null)
                {
                    headers.Clear();
                    ColumnHeader[] destinationArray = new ColumnHeader[value.Length];
                    Array.Copy(value, 0, destinationArray, 0, value.Length);
                    headers.AddRange(destinationArray);
                }
            }
            return editValue;
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.ComponentModel.ColumnHeaderCollectionEditor";
            }
        }
    }
}

