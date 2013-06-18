namespace System.Data
{
    using System;
    using System.ComponentModel;

    internal sealed class DataViewManagerListItemTypeDescriptor : ICustomTypeDescriptor
    {
        private DataViewManager dataViewManager;
        private PropertyDescriptorCollection propsCollection;

        internal DataViewManagerListItemTypeDescriptor(DataViewManager dataViewManager)
        {
            this.dataViewManager = dataViewManager;
        }

        internal DataView GetDataView(DataTable table)
        {
            DataView view = new DataView(table);
            view.SetDataViewManager(this.dataViewManager);
            return view;
        }

        internal void Reset()
        {
            this.propsCollection = null;
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return new AttributeCollection(null);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return null;
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return null;
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return null;
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return null;
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return null;
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return new EventDescriptorCollection(null);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return new EventDescriptorCollection(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor) this).GetProperties(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            if (this.propsCollection == null)
            {
                PropertyDescriptor[] properties = null;
                DataSet dataSet = this.dataViewManager.DataSet;
                if (dataSet != null)
                {
                    int count = dataSet.Tables.Count;
                    properties = new PropertyDescriptor[count];
                    for (int i = 0; i < count; i++)
                    {
                        properties[i] = new DataTablePropertyDescriptor(dataSet.Tables[i]);
                    }
                }
                this.propsCollection = new PropertyDescriptorCollection(properties);
            }
            return this.propsCollection;
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
    }
}

