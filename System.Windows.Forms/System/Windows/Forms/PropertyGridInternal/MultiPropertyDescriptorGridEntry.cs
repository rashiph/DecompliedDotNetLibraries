namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Windows.Forms;

    internal class MultiPropertyDescriptorGridEntry : PropertyDescriptorGridEntry
    {
        private MergePropertyDescriptor mergedPd;
        private object[] objs;

        public MultiPropertyDescriptorGridEntry(PropertyGrid ownerGrid, GridEntry peParent, object[] objectArray, PropertyDescriptor[] propInfo, bool hide) : base(ownerGrid, peParent, hide)
        {
            this.mergedPd = new MergePropertyDescriptor(propInfo);
            this.objs = objectArray;
            base.Initialize(this.mergedPd);
        }

        protected override bool CreateChildren()
        {
            return this.CreateChildren(false);
        }

        protected override bool CreateChildren(bool diffOldChildren)
        {
            try
            {
                if (this.mergedPd.PropertyType.IsValueType || ((this.Flags & 0x200) != 0))
                {
                    return base.CreateChildren(diffOldChildren);
                }
                base.ChildCollection.Clear();
                MultiPropertyDescriptorGridEntry[] entryArray = MultiSelectRootGridEntry.PropertyMerger.GetMergedProperties(this.mergedPd.GetValues(this.objs), this, base.PropertySort, this.CurrentTab);
                if (entryArray != null)
                {
                    base.ChildCollection.AddRange(entryArray);
                }
                bool flag = this.Children.Count > 0;
                if (!flag)
                {
                    this.SetFlag(0x80000, true);
                }
                return flag;
            }
            catch
            {
                return false;
            }
        }

        public override object GetChildValueOwner(GridEntry childEntry)
        {
            if (!this.mergedPd.PropertyType.IsValueType && ((this.Flags & 0x200) == 0))
            {
                return this.mergedPd.GetValues(this.objs);
            }
            return base.GetChildValueOwner(childEntry);
        }

        public override IComponent[] GetComponents()
        {
            IComponent[] destinationArray = new IComponent[this.objs.Length];
            Array.Copy(this.objs, 0, destinationArray, 0, this.objs.Length);
            return destinationArray;
        }

        public override string GetPropertyTextValue(object value)
        {
            bool allEqual = true;
            try
            {
                if (((value == null) && (this.mergedPd.GetValue(this.objs, out allEqual) == null)) && !allEqual)
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }
            return base.GetPropertyTextValue(value);
        }

        internal override bool NotifyChildValue(GridEntry pe, int type)
        {
            bool flag = false;
            IDesignerHost designerHost = this.DesignerHost;
            DesignerTransaction transaction = null;
            if (designerHost != null)
            {
                transaction = designerHost.CreateTransaction();
            }
            try
            {
                flag = base.NotifyChildValue(pe, type);
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            return flag;
        }

        protected override void NotifyParentChange(GridEntry ge)
        {
            while (((ge != null) && (ge is PropertyDescriptorGridEntry)) && ((PropertyDescriptorGridEntry) ge).propertyInfo.Attributes.Contains(NotifyParentPropertyAttribute.Yes))
            {
                object valueOwner = ge.GetValueOwner();
                while (!(ge is PropertyDescriptorGridEntry) || this.OwnersEqual(valueOwner, ge.GetValueOwner()))
                {
                    ge = ge.ParentGridEntry;
                    if (ge == null)
                    {
                        break;
                    }
                }
                if (ge != null)
                {
                    valueOwner = ge.GetValueOwner();
                    IComponentChangeService componentChangeService = this.ComponentChangeService;
                    if (componentChangeService == null)
                    {
                        continue;
                    }
                    Array array = valueOwner as Array;
                    if (array != null)
                    {
                        for (int i = 0; i < array.Length; i++)
                        {
                            PropertyDescriptor propertyInfo = ((PropertyDescriptorGridEntry) ge).propertyInfo;
                            if (propertyInfo is MergePropertyDescriptor)
                            {
                                propertyInfo = ((MergePropertyDescriptor) propertyInfo)[i];
                            }
                            if (propertyInfo != null)
                            {
                                componentChangeService.OnComponentChanging(array.GetValue(i), propertyInfo);
                                componentChangeService.OnComponentChanged(array.GetValue(i), propertyInfo, null, null);
                            }
                        }
                        continue;
                    }
                    componentChangeService.OnComponentChanging(valueOwner, ((PropertyDescriptorGridEntry) ge).propertyInfo);
                    componentChangeService.OnComponentChanged(valueOwner, ((PropertyDescriptorGridEntry) ge).propertyInfo, null, null);
                }
            }
        }

        internal override bool NotifyValueGivenParent(object obj, int type)
        {
            if (obj is ICustomTypeDescriptor)
            {
                obj = ((ICustomTypeDescriptor) obj).GetPropertyOwner(base.propertyInfo);
            }
            switch (type)
            {
                case 1:
                {
                    object[] objArray = (object[]) obj;
                    if ((objArray != null) && (objArray.Length > 0))
                    {
                        IDesignerHost designerHost = this.DesignerHost;
                        DesignerTransaction transaction = null;
                        if (designerHost != null)
                        {
                            transaction = designerHost.CreateTransaction(System.Windows.Forms.SR.GetString("PropertyGridResetValue", new object[] { this.PropertyName }));
                        }
                        try
                        {
                            bool flag = !(objArray[0] is IComponent) || (((IComponent) objArray[0]).Site == null);
                            if (flag && !this.OnComponentChanging())
                            {
                                if (transaction != null)
                                {
                                    transaction.Cancel();
                                    transaction = null;
                                }
                                return false;
                            }
                            this.mergedPd.ResetValue(obj);
                            if (flag)
                            {
                                this.OnComponentChanged();
                            }
                            this.NotifyParentChange(this);
                        }
                        finally
                        {
                            if (transaction != null)
                            {
                                transaction.Commit();
                            }
                        }
                    }
                    return false;
                }
                case 3:
                case 5:
                {
                    MergePropertyDescriptor propertyInfo = base.propertyInfo as MergePropertyDescriptor;
                    if (propertyInfo == null)
                    {
                        return base.NotifyValueGivenParent(obj, type);
                    }
                    object[] objArray1 = (object[]) obj;
                    if (base.eventBindings == null)
                    {
                        base.eventBindings = (IEventBindingService) this.GetService(typeof(IEventBindingService));
                    }
                    if (base.eventBindings != null)
                    {
                        EventDescriptor eventdesc = base.eventBindings.GetEvent(propertyInfo[0]);
                        if (eventdesc != null)
                        {
                            return base.ViewEvent(obj, null, eventdesc, true);
                        }
                    }
                    return false;
                }
            }
            return base.NotifyValueGivenParent(obj, type);
        }

        public override void OnComponentChanged()
        {
            if (this.ComponentChangeService != null)
            {
                int length = this.objs.Length;
                for (int i = 0; i < length; i++)
                {
                    this.ComponentChangeService.OnComponentChanged(this.objs[i], this.mergedPd[i], null, null);
                }
            }
        }

        public override bool OnComponentChanging()
        {
            if (this.ComponentChangeService != null)
            {
                int length = this.objs.Length;
                for (int i = 0; i < length; i++)
                {
                    try
                    {
                        this.ComponentChangeService.OnComponentChanging(this.objs[i], this.mergedPd[i]);
                    }
                    catch (CheckoutException exception)
                    {
                        if (exception != CheckoutException.Canceled)
                        {
                            throw exception;
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        private bool OwnersEqual(object owner1, object owner2)
        {
            if (!(owner1 is Array))
            {
                return (owner1 == owner2);
            }
            Array array = owner1 as Array;
            Array array2 = owner2 as Array;
            if (((array == null) || (array2 == null)) || (array.Length != array2.Length))
            {
                return false;
            }
            for (int i = 0; i < array.Length; i++)
            {
                if (array.GetValue(i) != array2.GetValue(i))
                {
                    return false;
                }
            }
            return true;
        }

        public override IContainer Container
        {
            get
            {
                IContainer container = null;
                foreach (object obj2 in this.objs)
                {
                    IComponent component = obj2 as IComponent;
                    if ((component != null) && (component.Site != null))
                    {
                        if (container == null)
                        {
                            container = component.Site.Container;
                            continue;
                        }
                        if (container == component.Site.Container)
                        {
                            continue;
                        }
                    }
                    return null;
                }
                return container;
            }
        }

        public override bool Expandable
        {
            get
            {
                bool flagSet = this.GetFlagSet(0x20000);
                if (flagSet && (base.ChildCollection.Count > 0))
                {
                    return true;
                }
                if (this.GetFlagSet(0x80000))
                {
                    return false;
                }
                try
                {
                    object[] values = this.mergedPd.GetValues(this.objs);
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i] == null)
                        {
                            return false;
                        }
                    }
                }
                catch
                {
                    flagSet = false;
                }
                return flagSet;
            }
        }

        public override object PropertyValue
        {
            set
            {
                base.PropertyValue = value;
                base.RecreateChildren();
                if (this.Expanded)
                {
                    this.GridEntryHost.Refresh(false);
                }
            }
        }
    }
}

