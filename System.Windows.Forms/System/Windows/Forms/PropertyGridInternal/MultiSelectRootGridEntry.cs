namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class MultiSelectRootGridEntry : SingleSelectRootGridEntry
    {
        private static PDComparer PropertyComparer = new PDComparer();

        internal MultiSelectRootGridEntry(PropertyGridView view, object obj, IServiceProvider baseProvider, IDesignerHost host, PropertyTab tab, PropertySort sortType) : base(view, obj, baseProvider, host, tab, sortType)
        {
        }

        protected override bool CreateChildren()
        {
            return this.CreateChildren(false);
        }

        protected override bool CreateChildren(bool diffOldChildren)
        {
            try
            {
                object[] objValue = (object[]) base.objValue;
                base.ChildCollection.Clear();
                MultiPropertyDescriptorGridEntry[] entryArray = PropertyMerger.GetMergedProperties(objValue, this, base.PropertySort, this.CurrentTab);
                if (entryArray != null)
                {
                    base.ChildCollection.AddRange(entryArray);
                }
                bool flag = this.Children.Count > 0;
                if (!flag)
                {
                    this.SetFlag(0x80000, true);
                }
                base.CategorizePropEntries();
                return flag;
            }
            catch
            {
                return false;
            }
        }

        internal override bool ForceReadOnly
        {
            get
            {
                if (!base.forceReadOnlyChecked)
                {
                    bool flag = false;
                    foreach (object obj2 in (Array) base.objValue)
                    {
                        ReadOnlyAttribute attribute = (ReadOnlyAttribute) TypeDescriptor.GetAttributes(obj2)[typeof(ReadOnlyAttribute)];
                        if (((attribute != null) && !attribute.IsDefaultAttribute()) || TypeDescriptor.GetAttributes(obj2).Contains(InheritanceAttribute.InheritedReadOnly))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        base.flags |= 0x400;
                    }
                    base.forceReadOnlyChecked = true;
                }
                return base.ForceReadOnly;
            }
        }

        private class PDComparer : IComparer
        {
            public int Compare(object obj1, object obj2)
            {
                PropertyDescriptor descriptor = obj1 as PropertyDescriptor;
                PropertyDescriptor descriptor2 = obj2 as PropertyDescriptor;
                if ((descriptor == null) && (descriptor2 == null))
                {
                    return 0;
                }
                if (descriptor == null)
                {
                    return -1;
                }
                if (descriptor2 == null)
                {
                    return 1;
                }
                int num = string.Compare(descriptor.Name, descriptor2.Name, false, CultureInfo.InvariantCulture);
                if (num == 0)
                {
                    num = string.Compare(descriptor.PropertyType.FullName, descriptor2.PropertyType.FullName, true, CultureInfo.CurrentCulture);
                }
                return num;
            }
        }

        internal static class PropertyMerger
        {
            private static ArrayList GetCommonProperties(object[] objs, bool presort, PropertyTab tab, GridEntry parentEntry)
            {
                PropertyDescriptorCollection[] descriptorsArray = new PropertyDescriptorCollection[objs.Length];
                Attribute[] array = new Attribute[parentEntry.BrowsableAttributes.Count];
                parentEntry.BrowsableAttributes.CopyTo(array, 0);
                for (int i = 0; i < objs.Length; i++)
                {
                    PropertyDescriptorCollection descriptors = tab.GetProperties(parentEntry, objs[i], array);
                    if (presort)
                    {
                        descriptors = descriptors.Sort(MultiSelectRootGridEntry.PropertyComparer);
                    }
                    descriptorsArray[i] = descriptors;
                }
                ArrayList list = new ArrayList();
                PropertyDescriptor[] descriptorArray = new PropertyDescriptor[objs.Length];
                int[] numArray = new int[descriptorsArray.Length];
                for (int j = 0; j < descriptorsArray[0].Count; j++)
                {
                    PropertyDescriptor descriptor = descriptorsArray[0][j];
                    bool flag = descriptor.Attributes[typeof(MergablePropertyAttribute)].IsDefaultAttribute();
                    for (int k = 1; flag && (k < descriptorsArray.Length); k++)
                    {
                        if (numArray[k] >= descriptorsArray[k].Count)
                        {
                            flag = false;
                            break;
                        }
                        PropertyDescriptor descriptor2 = descriptorsArray[k][numArray[k]];
                        if (descriptor.Equals(descriptor2))
                        {
                            numArray[k]++;
                            if (!descriptor2.Attributes[typeof(MergablePropertyAttribute)].IsDefaultAttribute())
                            {
                                flag = false;
                                break;
                            }
                            descriptorArray[k] = descriptor2;
                            continue;
                        }
                        int num4 = numArray[k];
                        descriptor2 = descriptorsArray[k][num4];
                        flag = false;
                        while (MultiSelectRootGridEntry.PropertyComparer.Compare(descriptor2, descriptor) <= 0)
                        {
                            if (descriptor.Equals(descriptor2))
                            {
                                if (!descriptor2.Attributes[typeof(MergablePropertyAttribute)].IsDefaultAttribute())
                                {
                                    flag = false;
                                    num4++;
                                }
                                else
                                {
                                    flag = true;
                                    descriptorArray[k] = descriptor2;
                                    numArray[k] = num4 + 1;
                                }
                                break;
                            }
                            num4++;
                            if (num4 >= descriptorsArray[k].Count)
                            {
                                break;
                            }
                            descriptor2 = descriptorsArray[k][num4];
                        }
                        if (!flag)
                        {
                            numArray[k] = num4;
                            break;
                        }
                    }
                    if (flag)
                    {
                        descriptorArray[0] = descriptor;
                        list.Add(descriptorArray.Clone());
                    }
                }
                return list;
            }

            public static MultiPropertyDescriptorGridEntry[] GetMergedProperties(object[] rgobjs, GridEntry parentEntry, PropertySort sort, PropertyTab tab)
            {
                MultiPropertyDescriptorGridEntry[] entryArray = null;
                try
                {
                    int length = rgobjs.Length;
                    if ((sort & PropertySort.Alphabetical) != PropertySort.NoSort)
                    {
                        ArrayList list = GetCommonProperties(rgobjs, true, tab, parentEntry);
                        MultiPropertyDescriptorGridEntry[] entryArray2 = new MultiPropertyDescriptorGridEntry[list.Count];
                        for (int k = 0; k < entryArray2.Length; k++)
                        {
                            entryArray2[k] = new MultiPropertyDescriptorGridEntry(parentEntry.OwnerGrid, parentEntry, rgobjs, (PropertyDescriptor[]) list[k], false);
                        }
                        return SortParenEntries(entryArray2);
                    }
                    object[] destinationArray = new object[length - 1];
                    Array.Copy(rgobjs, 1, destinationArray, 0, length - 1);
                    ArrayList sortedMergedEntries = GetCommonProperties(destinationArray, true, tab, parentEntry);
                    ArrayList list3 = GetCommonProperties(new object[] { rgobjs[0] }, false, tab, parentEntry);
                    PropertyDescriptor[] baseEntries = new PropertyDescriptor[list3.Count];
                    for (int i = 0; i < list3.Count; i++)
                    {
                        baseEntries[i] = ((PropertyDescriptor[]) list3[i])[0];
                    }
                    sortedMergedEntries = UnsortedMerge(baseEntries, sortedMergedEntries);
                    MultiPropertyDescriptorGridEntry[] entries = new MultiPropertyDescriptorGridEntry[sortedMergedEntries.Count];
                    for (int j = 0; j < entries.Length; j++)
                    {
                        entries[j] = new MultiPropertyDescriptorGridEntry(parentEntry.OwnerGrid, parentEntry, rgobjs, (PropertyDescriptor[]) sortedMergedEntries[j], false);
                    }
                    entryArray = SortParenEntries(entries);
                }
                catch
                {
                }
                return entryArray;
            }

            private static MultiPropertyDescriptorGridEntry[] SortParenEntries(MultiPropertyDescriptorGridEntry[] entries)
            {
                MultiPropertyDescriptorGridEntry[] entryArray = null;
                int num = 0;
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].ParensAroundName)
                    {
                        if (entryArray == null)
                        {
                            entryArray = new MultiPropertyDescriptorGridEntry[entries.Length];
                        }
                        entryArray[num++] = entries[i];
                        entries[i] = null;
                    }
                }
                if (num > 0)
                {
                    for (int j = 0; j < entries.Length; j++)
                    {
                        if (entries[j] != null)
                        {
                            entryArray[num++] = entries[j];
                        }
                    }
                    entries = entryArray;
                }
                return entries;
            }

            private static ArrayList UnsortedMerge(PropertyDescriptor[] baseEntries, ArrayList sortedMergedEntries)
            {
                ArrayList list = new ArrayList();
                PropertyDescriptor[] destinationArray = new PropertyDescriptor[((PropertyDescriptor[]) sortedMergedEntries[0]).Length + 1];
                for (int i = 0; i < baseEntries.Length; i++)
                {
                    PropertyDescriptor descriptor = baseEntries[i];
                    PropertyDescriptor[] sourceArray = null;
                    string strA = descriptor.Name + " " + descriptor.PropertyType.FullName;
                    int count = sortedMergedEntries.Count;
                    int num3 = count / 2;
                    int num4 = 0;
                    while (count > 0)
                    {
                        PropertyDescriptor[] descriptorArray3 = (PropertyDescriptor[]) sortedMergedEntries[num4 + num3];
                        PropertyDescriptor descriptor2 = descriptorArray3[0];
                        string strB = descriptor2.Name + " " + descriptor2.PropertyType.FullName;
                        int num5 = string.Compare(strA, strB, false, CultureInfo.InvariantCulture);
                        if (num5 == 0)
                        {
                            sourceArray = descriptorArray3;
                            break;
                        }
                        if (num5 < 0)
                        {
                            count = num3;
                        }
                        else
                        {
                            int num6 = num3 + 1;
                            num4 += num6;
                            count -= num6;
                        }
                        num3 = count / 2;
                    }
                    if (sourceArray != null)
                    {
                        destinationArray[0] = descriptor;
                        Array.Copy(sourceArray, 0, destinationArray, 1, sourceArray.Length);
                        list.Add(destinationArray.Clone());
                    }
                }
                return list;
            }
        }
    }
}

