namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;

    public static class ListBindingHelper
    {
        private static Attribute[] browsableAttribute;

        private static object CreateInstanceOfType(System.Type type)
        {
            object obj2 = null;
            Exception innerException = null;
            try
            {
                obj2 = System.Windows.Forms.SecurityUtils.SecureCreateInstance(type);
            }
            catch (TargetInvocationException exception2)
            {
                innerException = exception2;
            }
            catch (MethodAccessException exception3)
            {
                innerException = exception3;
            }
            catch (MissingMethodException exception4)
            {
                innerException = exception4;
            }
            if (innerException != null)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("BindingSourceInstanceError"), innerException);
            }
            return obj2;
        }

        private static object GetFirstItemByEnumerable(IEnumerable enumerable)
        {
            object current = null;
            if (enumerable is IList)
            {
                IList list = enumerable as IList;
                return ((list.Count > 0) ? list[0] : null);
            }
            try
            {
                IEnumerator enumerator = enumerable.GetEnumerator();
                enumerator.Reset();
                if (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                }
                enumerator.Reset();
            }
            catch (NotSupportedException)
            {
                current = null;
            }
            return current;
        }

        public static object GetList(object list)
        {
            if (list is IListSource)
            {
                return (list as IListSource).GetList();
            }
            return list;
        }

        public static object GetList(object dataSource, string dataMember)
        {
            object firstItemByEnumerable;
            dataSource = GetList(dataSource);
            if (((dataSource == null) || (dataSource is System.Type)) || string.IsNullOrEmpty(dataMember))
            {
                return dataSource;
            }
            PropertyDescriptor descriptor = GetListItemProperties(dataSource).Find(dataMember, true);
            if (descriptor == null)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataSourceDataMemberPropNotFound", new object[] { dataMember }));
            }
            if (dataSource is ICurrencyManagerProvider)
            {
                CurrencyManager currencyManager = (dataSource as ICurrencyManagerProvider).CurrencyManager;
                firstItemByEnumerable = (((currencyManager != null) && (currencyManager.Position >= 0)) && (currencyManager.Position <= (currencyManager.Count - 1))) ? currencyManager.Current : null;
            }
            else if (dataSource is IEnumerable)
            {
                firstItemByEnumerable = GetFirstItemByEnumerable(dataSource as IEnumerable);
            }
            else
            {
                firstItemByEnumerable = dataSource;
            }
            if (firstItemByEnumerable != null)
            {
                return descriptor.GetValue(firstItemByEnumerable);
            }
            return null;
        }

        public static PropertyDescriptorCollection GetListItemProperties(object list)
        {
            if (list == null)
            {
                return new PropertyDescriptorCollection(null);
            }
            if (list is System.Type)
            {
                return GetListItemPropertiesByType(list as System.Type);
            }
            object component = GetList(list);
            if (component is ITypedList)
            {
                return (component as ITypedList).GetItemProperties(null);
            }
            if (component is IEnumerable)
            {
                return GetListItemPropertiesByEnumerable(component as IEnumerable);
            }
            return TypeDescriptor.GetProperties(component);
        }

        public static PropertyDescriptorCollection GetListItemProperties(object list, PropertyDescriptor[] listAccessors)
        {
            if ((listAccessors == null) || (listAccessors.Length == 0))
            {
                return GetListItemProperties(list);
            }
            if (list is System.Type)
            {
                return GetListItemPropertiesByType(list as System.Type, listAccessors);
            }
            object target = GetList(list);
            if (target is ITypedList)
            {
                return (target as ITypedList).GetItemProperties(listAccessors);
            }
            if (target is IEnumerable)
            {
                return GetListItemPropertiesByEnumerable(target as IEnumerable, listAccessors);
            }
            return GetListItemPropertiesByInstance(target, listAccessors, 0);
        }

        public static PropertyDescriptorCollection GetListItemProperties(object dataSource, string dataMember, PropertyDescriptor[] listAccessors)
        {
            dataSource = GetList(dataSource);
            if (!string.IsNullOrEmpty(dataMember))
            {
                PropertyDescriptor descriptor = GetListItemProperties(dataSource).Find(dataMember, true);
                if (descriptor == null)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("DataSourceDataMemberPropNotFound", new object[] { dataMember }));
                }
                int num = (listAccessors == null) ? 1 : (listAccessors.Length + 1);
                PropertyDescriptor[] descriptorArray = new PropertyDescriptor[num];
                descriptorArray[0] = descriptor;
                for (int i = 1; i < num; i++)
                {
                    descriptorArray[i] = listAccessors[i - 1];
                }
                listAccessors = descriptorArray;
            }
            return GetListItemProperties(dataSource, listAccessors);
        }

        private static PropertyDescriptorCollection GetListItemPropertiesByEnumerable(IEnumerable enumerable)
        {
            PropertyDescriptorCollection properties = null;
            System.Type c = enumerable.GetType();
            if (typeof(Array).IsAssignableFrom(c))
            {
                properties = TypeDescriptor.GetProperties(c.GetElementType(), BrowsableAttributeList);
            }
            else
            {
                ITypedList list = enumerable as ITypedList;
                if (list != null)
                {
                    properties = list.GetItemProperties(null);
                }
                else
                {
                    PropertyInfo typedIndexer = GetTypedIndexer(c);
                    if ((typedIndexer != null) && !typeof(ICustomTypeDescriptor).IsAssignableFrom(typedIndexer.PropertyType))
                    {
                        properties = TypeDescriptor.GetProperties(typedIndexer.PropertyType, BrowsableAttributeList);
                    }
                }
            }
            if (properties != null)
            {
                return properties;
            }
            object firstItemByEnumerable = GetFirstItemByEnumerable(enumerable);
            if (!(enumerable is string))
            {
                if (firstItemByEnumerable == null)
                {
                    return new PropertyDescriptorCollection(null);
                }
                properties = TypeDescriptor.GetProperties(firstItemByEnumerable, BrowsableAttributeList);
                if ((enumerable is IList) || ((properties != null) && (properties.Count != 0)))
                {
                    return properties;
                }
            }
            return TypeDescriptor.GetProperties(enumerable, BrowsableAttributeList);
        }

        private static PropertyDescriptorCollection GetListItemPropertiesByEnumerable(IEnumerable enumerable, PropertyDescriptor[] listAccessors)
        {
            if ((listAccessors == null) || (listAccessors.Length == 0))
            {
                return GetListItemPropertiesByEnumerable(enumerable);
            }
            ITypedList list = enumerable as ITypedList;
            if (list != null)
            {
                return list.GetItemProperties(listAccessors);
            }
            return GetListItemPropertiesByEnumerable(enumerable, listAccessors, 0);
        }

        private static PropertyDescriptorCollection GetListItemPropertiesByEnumerable(IEnumerable iEnumerable, PropertyDescriptor[] listAccessors, int startIndex)
        {
            object target = null;
            object firstItemByEnumerable = GetFirstItemByEnumerable(iEnumerable);
            if (firstItemByEnumerable != null)
            {
                target = GetList(listAccessors[startIndex].GetValue(firstItemByEnumerable));
            }
            if (target == null)
            {
                return GetListItemPropertiesByType(listAccessors[startIndex].PropertyType, listAccessors, startIndex);
            }
            startIndex++;
            IEnumerable enumerable = target as IEnumerable;
            if (enumerable != null)
            {
                if (startIndex == listAccessors.Length)
                {
                    return GetListItemPropertiesByEnumerable(enumerable);
                }
                return GetListItemPropertiesByEnumerable(enumerable, listAccessors, startIndex);
            }
            return GetListItemPropertiesByInstance(target, listAccessors, startIndex);
        }

        private static PropertyDescriptorCollection GetListItemPropertiesByInstance(object target, PropertyDescriptor[] listAccessors, int startIndex)
        {
            if ((listAccessors != null) && (listAccessors.Length > startIndex))
            {
                object list = listAccessors[startIndex].GetValue(target);
                if (list == null)
                {
                    return GetListItemPropertiesByType(listAccessors[startIndex].PropertyType, listAccessors, startIndex);
                }
                PropertyDescriptor[] descriptorArray = null;
                if (listAccessors.Length > (startIndex + 1))
                {
                    int num = listAccessors.Length - (startIndex + 1);
                    descriptorArray = new PropertyDescriptor[num];
                    for (int i = 0; i < num; i++)
                    {
                        descriptorArray[i] = listAccessors[(startIndex + 1) + i];
                    }
                }
                return GetListItemProperties(list, descriptorArray);
            }
            return TypeDescriptor.GetProperties(target, BrowsableAttributeList);
        }

        private static PropertyDescriptorCollection GetListItemPropertiesByType(System.Type type)
        {
            return TypeDescriptor.GetProperties(GetListItemType(type), BrowsableAttributeList);
        }

        private static PropertyDescriptorCollection GetListItemPropertiesByType(System.Type type, PropertyDescriptor[] listAccessors)
        {
            if ((listAccessors == null) || (listAccessors.Length == 0))
            {
                return GetListItemPropertiesByType(type);
            }
            return GetListItemPropertiesByType(type, listAccessors, 0);
        }

        private static PropertyDescriptorCollection GetListItemPropertiesByType(System.Type type, PropertyDescriptor[] listAccessors, int startIndex)
        {
            System.Type propertyType = listAccessors[startIndex].PropertyType;
            startIndex++;
            if (startIndex >= listAccessors.Length)
            {
                return GetListItemProperties(propertyType);
            }
            return GetListItemPropertiesByType(propertyType, listAccessors, startIndex);
        }

        public static System.Type GetListItemType(object list)
        {
            if (list == null)
            {
                return null;
            }
            if ((list is System.Type) && typeof(IListSource).IsAssignableFrom(list as System.Type))
            {
                list = CreateInstanceOfType(list as System.Type);
            }
            list = GetList(list);
            System.Type c = (list is System.Type) ? (list as System.Type) : list.GetType();
            object obj2 = (list is System.Type) ? null : list;
            if (typeof(Array).IsAssignableFrom(c))
            {
                return c.GetElementType();
            }
            PropertyInfo typedIndexer = GetTypedIndexer(c);
            if (typedIndexer != null)
            {
                return typedIndexer.PropertyType;
            }
            if (obj2 is IEnumerable)
            {
                return GetListItemTypeByEnumerable(obj2 as IEnumerable);
            }
            return c;
        }

        public static System.Type GetListItemType(object dataSource, string dataMember)
        {
            if (dataSource != null)
            {
                if (string.IsNullOrEmpty(dataMember))
                {
                    return GetListItemType(dataSource);
                }
                PropertyDescriptorCollection listItemProperties = GetListItemProperties(dataSource);
                if (listItemProperties == null)
                {
                    return typeof(object);
                }
                PropertyDescriptor descriptor = listItemProperties.Find(dataMember, true);
                if ((descriptor != null) && !(descriptor.PropertyType is ICustomTypeDescriptor))
                {
                    return GetListItemType(descriptor.PropertyType);
                }
            }
            return typeof(object);
        }

        private static System.Type GetListItemTypeByEnumerable(IEnumerable iEnumerable)
        {
            object firstItemByEnumerable = GetFirstItemByEnumerable(iEnumerable);
            if (firstItemByEnumerable == null)
            {
                return typeof(object);
            }
            return firstItemByEnumerable.GetType();
        }

        public static string GetListName(object list, PropertyDescriptor[] listAccessors)
        {
            System.Type propertyType;
            if (list == null)
            {
                return string.Empty;
            }
            ITypedList list2 = list as ITypedList;
            if (list2 != null)
            {
                return list2.GetListName(listAccessors);
            }
            if ((listAccessors == null) || (listAccessors.Length == 0))
            {
                System.Type type2 = list as System.Type;
                if (type2 != null)
                {
                    propertyType = type2;
                }
                else
                {
                    propertyType = list.GetType();
                }
            }
            else
            {
                PropertyDescriptor descriptor = listAccessors[0];
                propertyType = descriptor.PropertyType;
            }
            return GetListNameFromType(propertyType);
        }

        private static string GetListNameFromType(System.Type type)
        {
            if (typeof(Array).IsAssignableFrom(type))
            {
                return type.GetElementType().Name;
            }
            if (typeof(IList).IsAssignableFrom(type))
            {
                PropertyInfo typedIndexer = GetTypedIndexer(type);
                if (typedIndexer != null)
                {
                    return typedIndexer.PropertyType.Name;
                }
                return type.Name;
            }
            return type.Name;
        }

        private static PropertyInfo GetTypedIndexer(System.Type type)
        {
            if ((!typeof(IList).IsAssignableFrom(type) && !typeof(ITypedList).IsAssignableFrom(type)) && !typeof(IListSource).IsAssignableFrom(type))
            {
                return null;
            }
            PropertyInfo info = null;
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < properties.Length; i++)
            {
                if ((properties[i].GetIndexParameters().Length > 0) && (properties[i].PropertyType != typeof(object)))
                {
                    info = properties[i];
                    if (info.Name == "Item")
                    {
                        return info;
                    }
                }
            }
            return info;
        }

        private static Attribute[] BrowsableAttributeList
        {
            get
            {
                if (browsableAttribute == null)
                {
                    browsableAttribute = new Attribute[] { new BrowsableAttribute(true) };
                }
                return browsableAttribute;
            }
        }
    }
}

