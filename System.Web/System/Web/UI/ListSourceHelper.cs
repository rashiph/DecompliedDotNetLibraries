namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    public static class ListSourceHelper
    {
        public static bool ContainsListCollection(IDataSource dataSource)
        {
            ICollection viewNames = dataSource.GetViewNames();
            return ((viewNames != null) && (viewNames.Count > 0));
        }

        public static IList GetList(IDataSource dataSource)
        {
            ICollection viewNames = dataSource.GetViewNames();
            if ((viewNames != null) && (viewNames.Count > 0))
            {
                return new ListSourceList(dataSource);
            }
            return null;
        }

        internal sealed class ListSourceList : CollectionBase, ITypedList
        {
            private IDataSource _dataSource;

            public ListSourceList(IDataSource dataSource)
            {
                this._dataSource = dataSource;
                ((IList) this).Add(new ListSourceHelper.ListSourceRow(this._dataSource));
            }

            PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
            {
                if (this._dataSource != null)
                {
                    ICollection viewNames = this._dataSource.GetViewNames();
                    if ((viewNames != null) && (viewNames.Count > 0))
                    {
                        string[] array = new string[viewNames.Count];
                        viewNames.CopyTo(array, 0);
                        PropertyDescriptor[] properties = new PropertyDescriptor[viewNames.Count];
                        for (int i = 0; i < array.Length; i++)
                        {
                            properties[i] = new ListSourceHelper.ListSourcePropertyDescriptor(array[i]);
                        }
                        return new PropertyDescriptorCollection(properties);
                    }
                }
                return new PropertyDescriptorCollection(null);
            }

            string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
            {
                return string.Empty;
            }
        }

        internal class ListSourcePropertyDescriptor : PropertyDescriptor
        {
            private string _name;

            public ListSourcePropertyDescriptor(string name) : base(name, null)
            {
                this._name = name;
            }

            public override bool CanResetValue(object value)
            {
                return false;
            }

            public override object GetValue(object source)
            {
                if (source is ListSourceHelper.ListSourceRow)
                {
                    ListSourceHelper.ListSourceRow row = (ListSourceHelper.ListSourceRow) source;
                    return row.DataSource.GetView(this._name).ExecuteSelect(DataSourceSelectArguments.Empty);
                }
                return null;
            }

            public override void ResetValue(object component)
            {
                throw new NotSupportedException();
            }

            public override void SetValue(object component, object value)
            {
                throw new NotSupportedException();
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get
                {
                    return typeof(ListSourceHelper.ListSourceRow);
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    return typeof(IEnumerable);
                }
            }
        }

        internal class ListSourceRow
        {
            private IDataSource _dataSource;

            public ListSourceRow(IDataSource dataSource)
            {
                this._dataSource = dataSource;
            }

            public IDataSource DataSource
            {
                get
                {
                    return this._dataSource;
                }
            }
        }
    }
}

