namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    internal class BindToObject
    {
        private System.Windows.Forms.BindingManagerBase bindingManager;
        private System.Windows.Forms.BindingMemberInfo dataMember;
        private object dataSource;
        private bool dataSourceInitialized;
        private string errorText = string.Empty;
        private PropertyDescriptor fieldInfo;
        private Binding owner;
        private bool waitingOnDataSource;

        internal BindToObject(Binding owner, object dataSource, string dataMember)
        {
            this.owner = owner;
            this.dataSource = dataSource;
            this.dataMember = new System.Windows.Forms.BindingMemberInfo(dataMember);
            this.CheckBinding();
        }

        internal void CheckBinding()
        {
            if (((this.owner == null) || (this.owner.BindableComponent == null)) || !this.owner.ControlAtDesignTime())
            {
                if (((this.owner.BindingManagerBase != null) && (this.fieldInfo != null)) && (this.owner.BindingManagerBase.IsBinding && !(this.owner.BindingManagerBase is CurrencyManager)))
                {
                    this.fieldInfo.RemoveValueChanged(this.owner.BindingManagerBase.Current, new EventHandler(this.PropValueChanged));
                }
                if ((((this.owner != null) && (this.owner.BindingManagerBase != null)) && ((this.owner.BindableComponent != null) && this.owner.ComponentCreated)) && this.IsDataSourceInitialized)
                {
                    string bindingField = this.dataMember.BindingField;
                    this.fieldInfo = this.owner.BindingManagerBase.GetItemProperties().Find(bindingField, true);
                    if (((this.owner.BindingManagerBase.DataSource != null) && (this.fieldInfo == null)) && (bindingField.Length > 0))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("ListBindingBindField", new object[] { bindingField }), "dataMember");
                    }
                    if (((this.fieldInfo != null) && this.owner.BindingManagerBase.IsBinding) && !(this.owner.BindingManagerBase is CurrencyManager))
                    {
                        this.fieldInfo.AddValueChanged(this.owner.BindingManagerBase.Current, new EventHandler(this.PropValueChanged));
                    }
                }
                else
                {
                    this.fieldInfo = null;
                }
            }
        }

        private void DataSource_Initialized(object sender, EventArgs e)
        {
            ISupportInitializeNotification dataSource = this.dataSource as ISupportInitializeNotification;
            if (dataSource != null)
            {
                dataSource.Initialized -= new EventHandler(this.DataSource_Initialized);
            }
            this.waitingOnDataSource = false;
            this.dataSourceInitialized = true;
            this.CheckBinding();
        }

        private string GetErrorText(object value)
        {
            IDataErrorInfo info = value as IDataErrorInfo;
            string error = string.Empty;
            if (info != null)
            {
                if (this.fieldInfo == null)
                {
                    error = info.Error;
                }
                else
                {
                    error = info[this.fieldInfo.Name];
                }
            }
            return (error ?? string.Empty);
        }

        internal object GetValue()
        {
            object current = this.bindingManager.Current;
            this.errorText = this.GetErrorText(current);
            if (this.fieldInfo != null)
            {
                current = this.fieldInfo.GetValue(current);
            }
            return current;
        }

        private void PropValueChanged(object sender, EventArgs e)
        {
            if (this.bindingManager != null)
            {
                this.bindingManager.OnCurrentChanged(EventArgs.Empty);
            }
        }

        internal void SetBindingManagerBase(System.Windows.Forms.BindingManagerBase lManager)
        {
            if (this.bindingManager != lManager)
            {
                if (((this.bindingManager != null) && (this.fieldInfo != null)) && (this.bindingManager.IsBinding && !(this.bindingManager is CurrencyManager)))
                {
                    this.fieldInfo.RemoveValueChanged(this.bindingManager.Current, new EventHandler(this.PropValueChanged));
                    this.fieldInfo = null;
                }
                this.bindingManager = lManager;
                this.CheckBinding();
            }
        }

        internal void SetValue(object value)
        {
            object component = null;
            if (this.fieldInfo != null)
            {
                component = this.bindingManager.Current;
                if (component is IEditableObject)
                {
                    ((IEditableObject) component).BeginEdit();
                }
                if (!this.fieldInfo.IsReadOnly)
                {
                    this.fieldInfo.SetValue(component, value);
                }
            }
            else
            {
                CurrencyManager bindingManager = this.bindingManager as CurrencyManager;
                if (bindingManager != null)
                {
                    bindingManager[bindingManager.Position] = value;
                    component = value;
                }
            }
            this.errorText = this.GetErrorText(component);
        }

        internal System.Windows.Forms.BindingManagerBase BindingManagerBase
        {
            get
            {
                return this.bindingManager;
            }
        }

        internal System.Windows.Forms.BindingMemberInfo BindingMemberInfo
        {
            get
            {
                return this.dataMember;
            }
        }

        internal System.Type BindToType
        {
            get
            {
                if (this.dataMember.BindingField.Length == 0)
                {
                    System.Type bindType = this.bindingManager.BindType;
                    if (typeof(Array).IsAssignableFrom(bindType))
                    {
                        bindType = bindType.GetElementType();
                    }
                    return bindType;
                }
                if (this.fieldInfo != null)
                {
                    return this.fieldInfo.PropertyType;
                }
                return null;
            }
        }

        internal string DataErrorText
        {
            get
            {
                return this.errorText;
            }
        }

        internal object DataSource
        {
            get
            {
                return this.dataSource;
            }
        }

        internal PropertyDescriptor FieldInfo
        {
            get
            {
                return this.fieldInfo;
            }
        }

        private bool IsDataSourceInitialized
        {
            get
            {
                if (this.dataSourceInitialized)
                {
                    return true;
                }
                ISupportInitializeNotification dataSource = this.dataSource as ISupportInitializeNotification;
                if ((dataSource == null) || dataSource.IsInitialized)
                {
                    this.dataSourceInitialized = true;
                    return true;
                }
                if (!this.waitingOnDataSource)
                {
                    dataSource.Initialized += new EventHandler(this.DataSource_Initialized);
                    this.waitingOnDataSource = true;
                }
                return false;
            }
        }
    }
}

