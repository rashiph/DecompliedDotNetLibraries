namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;

    internal sealed class DataSourceHelper
    {
        private DataSourceHelper()
        {
        }

        internal static IEnumerable GetResolvedDataSource(object dataSource, string dataMember)
        {
            if (dataSource != null)
            {
                IListSource source = dataSource as IListSource;
                if (source != null)
                {
                    IList list = source.GetList();
                    if (!source.ContainsListCollection)
                    {
                        return list;
                    }
                    if ((list != null) && (list is ITypedList))
                    {
                        PropertyDescriptorCollection itemProperties = ((ITypedList) list).GetItemProperties(new PropertyDescriptor[0]);
                        if ((itemProperties == null) || (itemProperties.Count == 0))
                        {
                            throw new HttpException(System.Web.SR.GetString("ListSource_Without_DataMembers"));
                        }
                        PropertyDescriptor descriptor = null;
                        if (string.IsNullOrEmpty(dataMember))
                        {
                            descriptor = itemProperties[0];
                        }
                        else
                        {
                            descriptor = itemProperties.Find(dataMember, true);
                        }
                        if (descriptor != null)
                        {
                            object component = list[0];
                            object obj3 = descriptor.GetValue(component);
                            if ((obj3 != null) && (obj3 is IEnumerable))
                            {
                                return (IEnumerable) obj3;
                            }
                        }
                        throw new HttpException(System.Web.SR.GetString("ListSource_Missing_DataMember", new object[] { dataMember }));
                    }
                }
                if (dataSource is IEnumerable)
                {
                    return (IEnumerable) dataSource;
                }
            }
            return null;
        }
    }
}

