namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;

    internal class RelatedCurrencyManager : CurrencyManager
    {
        private string dataField;
        private PropertyDescriptor fieldInfo;
        private static List<BindingManagerBase> IgnoreItemChangedTable = new List<BindingManagerBase>();
        private BindingManagerBase parentManager;

        internal RelatedCurrencyManager(BindingManagerBase parentManager, string dataField) : base(null)
        {
            this.Bind(parentManager, dataField);
        }

        internal void Bind(BindingManagerBase parentManager, string dataField)
        {
            this.UnwireParentManager(this.parentManager);
            this.parentManager = parentManager;
            this.dataField = dataField;
            this.fieldInfo = parentManager.GetItemProperties().Find(dataField, true);
            if ((this.fieldInfo == null) || !typeof(IList).IsAssignableFrom(this.fieldInfo.PropertyType))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("RelatedListManagerChild", new object[] { dataField }));
            }
            base.finalType = this.fieldInfo.PropertyType;
            this.WireParentManager(this.parentManager);
            this.ParentManager_CurrentItemChanged(parentManager, EventArgs.Empty);
        }

        public override PropertyDescriptorCollection GetItemProperties()
        {
            return this.GetItemProperties(null);
        }

        internal override PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            PropertyDescriptor[] descriptorArray;
            if ((listAccessors != null) && (listAccessors.Length > 0))
            {
                descriptorArray = new PropertyDescriptor[listAccessors.Length + 1];
                listAccessors.CopyTo(descriptorArray, 1);
            }
            else
            {
                descriptorArray = new PropertyDescriptor[1];
            }
            descriptorArray[0] = this.fieldInfo;
            return this.parentManager.GetItemProperties(descriptorArray);
        }

        internal override string GetListName()
        {
            string listName = this.GetListName(new ArrayList());
            if (listName.Length > 0)
            {
                return listName;
            }
            return base.GetListName();
        }

        protected internal override string GetListName(ArrayList listAccessors)
        {
            listAccessors.Insert(0, this.fieldInfo);
            return this.parentManager.GetListName(listAccessors);
        }

        private void ParentManager_CurrentItemChanged(object sender, EventArgs e)
        {
            if (!IgnoreItemChangedTable.Contains(this.parentManager))
            {
                int listposition = base.listposition;
                try
                {
                    base.PullData();
                }
                catch (Exception exception)
                {
                    base.OnDataError(exception);
                }
                if (this.parentManager is CurrencyManager)
                {
                    CurrencyManager parentManager = (CurrencyManager) this.parentManager;
                    if (parentManager.Count > 0)
                    {
                        this.SetDataSource(this.fieldInfo.GetValue(parentManager.Current));
                        base.listposition = (this.Count > 0) ? 0 : -1;
                    }
                    else
                    {
                        parentManager.AddNew();
                        try
                        {
                            IgnoreItemChangedTable.Add(parentManager);
                            parentManager.CancelCurrentEdit();
                        }
                        finally
                        {
                            if (IgnoreItemChangedTable.Contains(parentManager))
                            {
                                IgnoreItemChangedTable.Remove(parentManager);
                            }
                        }
                    }
                }
                else
                {
                    this.SetDataSource(this.fieldInfo.GetValue(this.parentManager.Current));
                    base.listposition = (this.Count > 0) ? 0 : -1;
                }
                if (listposition != base.listposition)
                {
                    this.OnPositionChanged(EventArgs.Empty);
                }
                this.OnCurrentChanged(EventArgs.Empty);
                this.OnCurrentItemChanged(EventArgs.Empty);
            }
        }

        private void ParentManager_MetaDataChanged(object sender, EventArgs e)
        {
            base.OnMetaDataChanged(e);
        }

        private void UnwireParentManager(BindingManagerBase bmb)
        {
            if (bmb != null)
            {
                bmb.CurrentItemChanged -= new EventHandler(this.ParentManager_CurrentItemChanged);
                if (bmb is CurrencyManager)
                {
                    (bmb as CurrencyManager).MetaDataChanged -= new EventHandler(this.ParentManager_MetaDataChanged);
                }
            }
        }

        private void WireParentManager(BindingManagerBase bmb)
        {
            if (bmb != null)
            {
                bmb.CurrentItemChanged += new EventHandler(this.ParentManager_CurrentItemChanged);
                if (bmb is CurrencyManager)
                {
                    (bmb as CurrencyManager).MetaDataChanged += new EventHandler(this.ParentManager_MetaDataChanged);
                }
            }
        }
    }
}

