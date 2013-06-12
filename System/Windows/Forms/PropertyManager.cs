namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    public class PropertyManager : BindingManagerBase
    {
        private bool bound;
        private object dataSource;
        private PropertyDescriptor propInfo;
        private string propName;

        public PropertyManager()
        {
        }

        internal PropertyManager(object dataSource) : base(dataSource)
        {
        }

        internal PropertyManager(object dataSource, string propName)
        {
            this.propName = propName;
            this.SetDataSource(dataSource);
        }

        public override void AddNew()
        {
            throw new NotSupportedException(System.Windows.Forms.SR.GetString("DataBindingAddNewNotSupportedOnPropertyManager"));
        }

        public override void CancelCurrentEdit()
        {
            IEditableObject current = this.Current as IEditableObject;
            if (current != null)
            {
                current.CancelEdit();
            }
            base.PushData();
        }

        public override void EndCurrentEdit()
        {
            bool flag;
            base.PullData(out flag);
            if (flag)
            {
                IEditableObject current = this.Current as IEditableObject;
                if (current != null)
                {
                    current.EndEdit();
                }
            }
        }

        internal override PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            if (this.dataSource == null)
            {
                return new PropertyDescriptorCollection(null);
            }
            return TypeDescriptor.GetProperties(this.dataSource);
        }

        internal override string GetListName()
        {
            return (TypeDescriptor.GetClassName(this.dataSource) + "." + this.propName);
        }

        protected internal override string GetListName(ArrayList listAccessors)
        {
            return "";
        }

        protected internal override void OnCurrentChanged(EventArgs ea)
        {
            base.PushData();
            if (base.onCurrentChangedHandler != null)
            {
                base.onCurrentChangedHandler(this, ea);
            }
            if (base.onCurrentItemChangedHandler != null)
            {
                base.onCurrentItemChangedHandler(this, ea);
            }
        }

        protected internal override void OnCurrentItemChanged(EventArgs ea)
        {
            base.PushData();
            if (base.onCurrentItemChangedHandler != null)
            {
                base.onCurrentItemChangedHandler(this, ea);
            }
        }

        private void PropertyChanged(object sender, EventArgs ea)
        {
            this.EndCurrentEdit();
            this.OnCurrentChanged(EventArgs.Empty);
        }

        public override void RemoveAt(int index)
        {
            throw new NotSupportedException(System.Windows.Forms.SR.GetString("DataBindingRemoveAtNotSupportedOnPropertyManager"));
        }

        public override void ResumeBinding()
        {
            this.OnCurrentChanged(new EventArgs());
            if (!this.bound)
            {
                try
                {
                    this.bound = true;
                    this.UpdateIsBinding();
                }
                catch
                {
                    this.bound = false;
                    this.UpdateIsBinding();
                    throw;
                }
            }
        }

        internal override void SetDataSource(object dataSource)
        {
            if ((this.dataSource != null) && !string.IsNullOrEmpty(this.propName))
            {
                this.propInfo.RemoveValueChanged(this.dataSource, new EventHandler(this.PropertyChanged));
                this.propInfo = null;
            }
            this.dataSource = dataSource;
            if ((this.dataSource != null) && !string.IsNullOrEmpty(this.propName))
            {
                this.propInfo = TypeDescriptor.GetProperties(dataSource).Find(this.propName, true);
                if (this.propInfo == null)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("PropertyManagerPropDoesNotExist", new object[] { this.propName, dataSource.ToString() }));
                }
                this.propInfo.AddValueChanged(dataSource, new EventHandler(this.PropertyChanged));
            }
        }

        public override void SuspendBinding()
        {
            this.EndCurrentEdit();
            if (this.bound)
            {
                try
                {
                    this.bound = false;
                    this.UpdateIsBinding();
                }
                catch
                {
                    this.bound = true;
                    this.UpdateIsBinding();
                    throw;
                }
            }
        }

        protected override void UpdateIsBinding()
        {
            for (int i = 0; i < base.Bindings.Count; i++)
            {
                base.Bindings[i].UpdateIsBinding();
            }
        }

        internal override System.Type BindType
        {
            get
            {
                return this.dataSource.GetType();
            }
        }

        public override int Count
        {
            get
            {
                return 1;
            }
        }

        public override object Current
        {
            get
            {
                return this.dataSource;
            }
        }

        internal override object DataSource
        {
            get
            {
                return this.dataSource;
            }
        }

        internal override bool IsBinding
        {
            get
            {
                return (this.dataSource != null);
            }
        }

        public override int Position
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }
    }
}

