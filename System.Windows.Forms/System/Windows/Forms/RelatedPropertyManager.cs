namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    internal class RelatedPropertyManager : PropertyManager
    {
        private string dataField;
        private PropertyDescriptor fieldInfo;
        private BindingManagerBase parentManager;

        internal RelatedPropertyManager(BindingManagerBase parentManager, string dataField) : base(GetCurrentOrNull(parentManager), dataField)
        {
            this.Bind(parentManager, dataField);
        }

        private void Bind(BindingManagerBase parentManager, string dataField)
        {
            this.parentManager = parentManager;
            this.dataField = dataField;
            this.fieldInfo = parentManager.GetItemProperties().Find(dataField, true);
            if (this.fieldInfo == null)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("RelatedListManagerChild", new object[] { dataField }));
            }
            parentManager.CurrentItemChanged += new EventHandler(this.ParentManager_CurrentItemChanged);
            this.Refresh();
        }

        private static object GetCurrentOrNull(BindingManagerBase parentManager)
        {
            if ((parentManager.Position < 0) || (parentManager.Position >= parentManager.Count))
            {
                return null;
            }
            return parentManager.Current;
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
            this.Refresh();
        }

        private void Refresh()
        {
            this.EndCurrentEdit();
            this.SetDataSource(GetCurrentOrNull(this.parentManager));
            this.OnCurrentChanged(EventArgs.Empty);
        }

        internal override System.Type BindType
        {
            get
            {
                return this.fieldInfo.PropertyType;
            }
        }

        public override object Current
        {
            get
            {
                if (this.DataSource == null)
                {
                    return null;
                }
                return this.fieldInfo.GetValue(this.DataSource);
            }
        }
    }
}

