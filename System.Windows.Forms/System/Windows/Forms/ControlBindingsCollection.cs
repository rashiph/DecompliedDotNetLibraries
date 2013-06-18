namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;

    [DefaultEvent("CollectionChanged"), TypeConverter("System.Windows.Forms.Design.ControlBindingsConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Editor("System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public class ControlBindingsCollection : BindingsCollection
    {
        internal IBindableComponent control;
        private DataSourceUpdateMode defaultDataSourceUpdateMode;

        public ControlBindingsCollection(IBindableComponent control)
        {
            this.control = control;
        }

        public void Add(Binding binding)
        {
            base.Add(binding);
        }

        public Binding Add(string propertyName, object dataSource, string dataMember)
        {
            return this.Add(propertyName, dataSource, dataMember, false, this.DefaultDataSourceUpdateMode, null, string.Empty, null);
        }

        public Binding Add(string propertyName, object dataSource, string dataMember, bool formattingEnabled)
        {
            return this.Add(propertyName, dataSource, dataMember, formattingEnabled, this.DefaultDataSourceUpdateMode, null, string.Empty, null);
        }

        public Binding Add(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode updateMode)
        {
            return this.Add(propertyName, dataSource, dataMember, formattingEnabled, updateMode, null, string.Empty, null);
        }

        public Binding Add(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode updateMode, object nullValue)
        {
            return this.Add(propertyName, dataSource, dataMember, formattingEnabled, updateMode, nullValue, string.Empty, null);
        }

        public Binding Add(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode updateMode, object nullValue, string formatString)
        {
            return this.Add(propertyName, dataSource, dataMember, formattingEnabled, updateMode, nullValue, formatString, null);
        }

        public Binding Add(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode updateMode, object nullValue, string formatString, IFormatProvider formatInfo)
        {
            if (dataSource == null)
            {
                throw new ArgumentNullException("dataSource");
            }
            Binding binding = new Binding(propertyName, dataSource, dataMember, formattingEnabled, updateMode, nullValue, formatString, formatInfo);
            this.Add(binding);
            return binding;
        }

        protected override void AddCore(Binding dataBinding)
        {
            if (dataBinding == null)
            {
                throw new ArgumentNullException("dataBinding");
            }
            if (dataBinding.BindableComponent == this.control)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("BindingsCollectionAdd1"));
            }
            if (dataBinding.BindableComponent != null)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("BindingsCollectionAdd2"));
            }
            dataBinding.SetBindableComponent(this.control);
            base.AddCore(dataBinding);
        }

        internal void CheckDuplicates(Binding binding)
        {
            if (binding.PropertyName.Length != 0)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (((binding != base[i]) && (base[i].PropertyName.Length > 0)) && (string.Compare(binding.PropertyName, base[i].PropertyName, false, CultureInfo.InvariantCulture) == 0))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("BindingsCollectionDup"), "binding");
                    }
                }
            }
        }

        public void Clear()
        {
            base.Clear();
        }

        protected override void ClearCore()
        {
            int count = this.Count;
            for (int i = 0; i < count; i++)
            {
                base[i].SetBindableComponent(null);
            }
            base.ClearCore();
        }

        public void Remove(Binding binding)
        {
            base.Remove(binding);
        }

        public void RemoveAt(int index)
        {
            base.RemoveAt(index);
        }

        protected override void RemoveCore(Binding dataBinding)
        {
            if (dataBinding.BindableComponent != this.control)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("BindingsCollectionForeign"));
            }
            dataBinding.SetBindableComponent(null);
            base.RemoveCore(dataBinding);
        }

        public IBindableComponent BindableComponent
        {
            get
            {
                return this.control;
            }
        }

        public System.Windows.Forms.Control Control
        {
            get
            {
                return (this.control as System.Windows.Forms.Control);
            }
        }

        public DataSourceUpdateMode DefaultDataSourceUpdateMode
        {
            get
            {
                return this.defaultDataSourceUpdateMode;
            }
            set
            {
                this.defaultDataSourceUpdateMode = value;
            }
        }

        public Binding this[string propertyName]
        {
            get
            {
                foreach (Binding binding in this)
                {
                    if (string.Equals(binding.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase))
                    {
                        return binding;
                    }
                }
                return null;
            }
        }
    }
}

