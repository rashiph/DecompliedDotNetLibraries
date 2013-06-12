namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class SingleSelectRootGridEntry : GridEntry, IRootGridEntry
    {
        protected IServiceProvider baseProvider;
        protected AttributeCollection browsableAttributes;
        private IComponentChangeService changeService;
        protected bool forceReadOnlyChecked;
        protected PropertyGridView gridEntryHost;
        protected IDesignerHost host;
        protected object objValue;
        protected string objValueClassName;
        protected GridEntry propDefault;
        protected PropertyTab tab;

        internal SingleSelectRootGridEntry(PropertyGridView view, object value, IServiceProvider baseProvider, IDesignerHost host, PropertyTab tab, PropertySort sortType) : this(view, value, null, baseProvider, host, tab, sortType)
        {
        }

        internal SingleSelectRootGridEntry(PropertyGridView gridEntryHost, object value, GridEntry parent, IServiceProvider baseProvider, IDesignerHost host, PropertyTab tab, PropertySort sortType) : base(gridEntryHost.OwnerGrid, parent)
        {
            this.host = host;
            this.gridEntryHost = gridEntryHost;
            this.baseProvider = baseProvider;
            this.tab = tab;
            this.objValue = value;
            this.objValueClassName = TypeDescriptor.GetClassName(this.objValue);
            this.IsExpandable = true;
            base.PropertySort = sortType;
            this.InternalExpanded = true;
        }

        internal void CategorizePropEntries()
        {
            if (this.Children.Count > 0)
            {
                GridEntry[] dest = new GridEntry[this.Children.Count];
                this.Children.CopyTo(dest, 0);
                if ((base.PropertySort & PropertySort.Categorized) != PropertySort.NoSort)
                {
                    Hashtable hashtable = new Hashtable();
                    for (int i = 0; i < dest.Length; i++)
                    {
                        GridEntry entry = dest[i];
                        if (entry != null)
                        {
                            string propertyCategory = entry.PropertyCategory;
                            ArrayList list = (ArrayList) hashtable[propertyCategory];
                            if (list == null)
                            {
                                list = new ArrayList();
                                hashtable[propertyCategory] = list;
                            }
                            list.Add(entry);
                        }
                    }
                    ArrayList list2 = new ArrayList();
                    IDictionaryEnumerator enumerator = hashtable.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        ArrayList list3 = (ArrayList) enumerator.Value;
                        if (list3 != null)
                        {
                            string key = (string) enumerator.Key;
                            if (list3.Count > 0)
                            {
                                GridEntry[] array = new GridEntry[list3.Count];
                                list3.CopyTo(array, 0);
                                try
                                {
                                    list2.Add(new CategoryGridEntry(base.ownerGrid, this, key, array));
                                    continue;
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    dest = new GridEntry[list2.Count];
                    list2.CopyTo(dest, 0);
                    StringSorter.Sort(dest);
                    base.ChildCollection.Clear();
                    base.ChildCollection.AddRange(dest);
                }
            }
        }

        protected override bool CreateChildren()
        {
            bool flag = base.CreateChildren();
            this.CategorizePropEntries();
            return flag;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.host = null;
                this.baseProvider = null;
                this.tab = null;
                this.gridEntryHost = null;
                this.changeService = null;
            }
            this.objValue = null;
            this.objValueClassName = null;
            this.propDefault = null;
            base.Dispose(disposing);
        }

        public override object GetService(System.Type serviceType)
        {
            object service = null;
            if (this.host != null)
            {
                service = this.host.GetService(serviceType);
            }
            if ((service == null) && (this.baseProvider != null))
            {
                service = this.baseProvider.GetService(serviceType);
            }
            return service;
        }

        public void ResetBrowsableAttributes()
        {
            this.browsableAttributes = new AttributeCollection(new Attribute[] { BrowsableAttribute.Yes });
        }

        public virtual void ShowCategories(bool fCategories)
        {
            if (((base.PropertySort &= PropertySort.Categorized) != PropertySort.NoSort) != fCategories)
            {
                if (fCategories)
                {
                    base.PropertySort |= PropertySort.Categorized;
                }
                else
                {
                    base.PropertySort &= ~PropertySort.Categorized;
                }
                if (this.Expandable && (base.ChildCollection != null))
                {
                    this.CreateChildren();
                }
            }
        }

        internal override bool AlwaysAllowExpand
        {
            get
            {
                return true;
            }
        }

        public override AttributeCollection BrowsableAttributes
        {
            get
            {
                if (this.browsableAttributes == null)
                {
                    this.browsableAttributes = new AttributeCollection(new Attribute[] { BrowsableAttribute.Yes });
                }
                return this.browsableAttributes;
            }
            set
            {
                if (value == null)
                {
                    this.ResetBrowsableAttributes();
                }
                else
                {
                    bool flag = true;
                    if (((this.browsableAttributes != null) && (value != null)) && (this.browsableAttributes.Count == value.Count))
                    {
                        Attribute[] array = new Attribute[this.browsableAttributes.Count];
                        Attribute[] attributeArray2 = new Attribute[value.Count];
                        this.browsableAttributes.CopyTo(array, 0);
                        value.CopyTo(attributeArray2, 0);
                        Array.Sort(array, GridEntry.AttributeTypeSorter);
                        Array.Sort(attributeArray2, GridEntry.AttributeTypeSorter);
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (!array[i].Equals(attributeArray2[i]))
                            {
                                flag = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        flag = false;
                    }
                    this.browsableAttributes = value;
                    if ((!flag && (this.Children != null)) && (this.Children.Count > 0))
                    {
                        this.DisposeChildren();
                    }
                }
            }
        }

        protected override IComponentChangeService ComponentChangeService
        {
            get
            {
                if (this.changeService == null)
                {
                    this.changeService = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                }
                return this.changeService;
            }
        }

        public override PropertyTab CurrentTab
        {
            get
            {
                return this.tab;
            }
            set
            {
                this.tab = value;
            }
        }

        internal override GridEntry DefaultChild
        {
            get
            {
                return this.propDefault;
            }
            set
            {
                this.propDefault = value;
            }
        }

        internal override IDesignerHost DesignerHost
        {
            get
            {
                return this.host;
            }
            set
            {
                this.host = value;
            }
        }

        internal override bool ForceReadOnly
        {
            get
            {
                if (!this.forceReadOnlyChecked)
                {
                    ReadOnlyAttribute attribute = (ReadOnlyAttribute) TypeDescriptor.GetAttributes(this.objValue)[typeof(ReadOnlyAttribute)];
                    if (((attribute != null) && !attribute.IsDefaultAttribute()) || TypeDescriptor.GetAttributes(this.objValue).Contains(InheritanceAttribute.InheritedReadOnly))
                    {
                        base.flags |= 0x400;
                    }
                    this.forceReadOnlyChecked = true;
                }
                return (base.ForceReadOnly || ((this.GridEntryHost != null) && !this.GridEntryHost.Enabled));
            }
        }

        internal override PropertyGridView GridEntryHost
        {
            get
            {
                return this.gridEntryHost;
            }
            set
            {
                this.gridEntryHost = value;
            }
        }

        public override System.Windows.Forms.GridItemType GridItemType
        {
            get
            {
                return System.Windows.Forms.GridItemType.Root;
            }
        }

        public override string HelpKeyword
        {
            get
            {
                HelpKeywordAttribute attribute = (HelpKeywordAttribute) TypeDescriptor.GetAttributes(this.objValue)[typeof(HelpKeywordAttribute)];
                if ((attribute != null) && !attribute.IsDefaultAttribute())
                {
                    return attribute.HelpKeyword;
                }
                return this.objValueClassName;
            }
        }

        public override string PropertyLabel
        {
            get
            {
                if (this.objValue is IComponent)
                {
                    ISite site = ((IComponent) this.objValue).Site;
                    if (site == null)
                    {
                        return this.objValue.GetType().Name;
                    }
                    return site.Name;
                }
                if (this.objValue != null)
                {
                    return this.objValue.ToString();
                }
                return null;
            }
        }

        public override object PropertyValue
        {
            get
            {
                return this.objValue;
            }
            set
            {
                object objValue = this.objValue;
                this.objValue = value;
                this.objValueClassName = TypeDescriptor.GetClassName(this.objValue);
                base.ownerGrid.ReplaceSelectedObject(objValue, value);
            }
        }
    }
}

