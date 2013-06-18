namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.DirectoryServices;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ActiveDirectorySchemaPropertyCollection : CollectionBase
    {
        private DirectoryEntry classEntry;
        private DirectoryContext context;
        private bool isBound;
        private string propertyName;
        private ActiveDirectorySchemaClass schemaClass;

        internal ActiveDirectorySchemaPropertyCollection(DirectoryContext context, ActiveDirectorySchemaClass schemaClass, bool isBound, string propertyName, ICollection properties)
        {
            this.schemaClass = schemaClass;
            this.propertyName = propertyName;
            this.isBound = isBound;
            this.context = context;
            foreach (ActiveDirectorySchemaProperty property in properties)
            {
                base.InnerList.Add(property);
            }
        }

        internal ActiveDirectorySchemaPropertyCollection(DirectoryContext context, ActiveDirectorySchemaClass schemaClass, bool isBound, string propertyName, ICollection propertyNames, bool onlyNames)
        {
            this.schemaClass = schemaClass;
            this.propertyName = propertyName;
            this.isBound = isBound;
            this.context = context;
            foreach (string str in propertyNames)
            {
                base.InnerList.Add(new ActiveDirectorySchemaProperty(context, str, null, null));
            }
        }

        public int Add(ActiveDirectorySchemaProperty schemaProperty)
        {
            if (schemaProperty == null)
            {
                throw new ArgumentNullException("schemaProperty");
            }
            if (!schemaProperty.isBound)
            {
                throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", new object[] { schemaProperty.Name }));
            }
            if (this.Contains(schemaProperty))
            {
                throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { schemaProperty }), "schemaProperty");
            }
            return base.List.Add(schemaProperty);
        }

        public void AddRange(ActiveDirectorySchemaProperty[] properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }
            ActiveDirectorySchemaProperty[] propertyArray = properties;
            for (int i = 0; i < propertyArray.Length; i++)
            {
                if (propertyArray[i] == null)
                {
                    throw new ArgumentException("properties");
                }
            }
            for (int j = 0; j < properties.Length; j++)
            {
                this.Add(properties[j]);
            }
        }

        public void AddRange(ActiveDirectorySchemaPropertyCollection properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }
            using (IEnumerator enumerator = properties.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (((ActiveDirectorySchemaProperty) enumerator.Current) == null)
                    {
                        throw new ArgumentException("properties");
                    }
                }
            }
            int count = properties.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(properties[i]);
            }
        }

        public void AddRange(ReadOnlyActiveDirectorySchemaPropertyCollection properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }
            using (IEnumerator enumerator = properties.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (((ActiveDirectorySchemaProperty) enumerator.Current) == null)
                    {
                        throw new ArgumentException("properties");
                    }
                }
            }
            int count = properties.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(properties[i]);
            }
        }

        public bool Contains(ActiveDirectorySchemaProperty schemaProperty)
        {
            if (schemaProperty == null)
            {
                throw new ArgumentNullException("schemaProperty");
            }
            if (!schemaProperty.isBound)
            {
                throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", new object[] { schemaProperty.Name }));
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySchemaProperty property = (ActiveDirectorySchemaProperty) base.InnerList[i];
                if (Utils.Compare(property.Name, schemaProperty.Name) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool Contains(string propertyName)
        {
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySchemaProperty property = (ActiveDirectorySchemaProperty) base.InnerList[i];
                if (Utils.Compare(property.Name, propertyName) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(ActiveDirectorySchemaProperty[] properties, int index)
        {
            base.List.CopyTo(properties, index);
        }

        internal string[] GetMultiValuedProperty()
        {
            string[] strArray = new string[base.InnerList.Count];
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                strArray[i] = ((ActiveDirectorySchemaProperty) base.InnerList[i]).Name;
            }
            return strArray;
        }

        public int IndexOf(ActiveDirectorySchemaProperty schemaProperty)
        {
            if (schemaProperty == null)
            {
                throw new ArgumentNullException("schemaProperty");
            }
            if (!schemaProperty.isBound)
            {
                throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", new object[] { schemaProperty.Name }));
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySchemaProperty property = (ActiveDirectorySchemaProperty) base.InnerList[i];
                if (Utils.Compare(property.Name, schemaProperty.Name) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, ActiveDirectorySchemaProperty schemaProperty)
        {
            if (schemaProperty == null)
            {
                throw new ArgumentNullException("schemaProperty");
            }
            if (!schemaProperty.isBound)
            {
                throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", new object[] { schemaProperty.Name }));
            }
            if (this.Contains(schemaProperty))
            {
                throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { schemaProperty }), "schemaProperty");
            }
            base.List.Insert(index, schemaProperty);
        }

        protected override void OnClearComplete()
        {
            if (this.isBound)
            {
                if (this.classEntry == null)
                {
                    this.classEntry = this.schemaClass.GetSchemaClassDirectoryEntry();
                }
                try
                {
                    if (this.classEntry.Properties.Contains(this.propertyName))
                    {
                        this.classEntry.Properties[this.propertyName].Clear();
                    }
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        protected override void OnInsertComplete(int index, object value)
        {
            if (this.isBound)
            {
                if (this.classEntry == null)
                {
                    this.classEntry = this.schemaClass.GetSchemaClassDirectoryEntry();
                }
                try
                {
                    this.classEntry.Properties[this.propertyName].Add(((ActiveDirectorySchemaProperty) value).Name);
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        protected override void OnRemoveComplete(int index, object value)
        {
            if (this.isBound)
            {
                if (this.classEntry == null)
                {
                    this.classEntry = this.schemaClass.GetSchemaClassDirectoryEntry();
                }
                string name = ((ActiveDirectorySchemaProperty) value).Name;
                try
                {
                    if (!this.classEntry.Properties[this.propertyName].Contains(name))
                    {
                        throw new ActiveDirectoryOperationException(Res.GetString("ValueCannotBeModified"));
                    }
                    this.classEntry.Properties[this.propertyName].Remove(name);
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        protected override void OnSetComplete(int index, object oldValue, object newValue)
        {
            if (this.isBound)
            {
                this.OnRemoveComplete(index, oldValue);
                this.OnInsertComplete(index, newValue);
            }
        }

        protected override void OnValidate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!(value is ActiveDirectorySchemaProperty))
            {
                throw new ArgumentException("value");
            }
            if (!((ActiveDirectorySchemaProperty) value).isBound)
            {
                throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", new object[] { ((ActiveDirectorySchemaProperty) value).Name }));
            }
        }

        public void Remove(ActiveDirectorySchemaProperty schemaProperty)
        {
            if (schemaProperty == null)
            {
                throw new ArgumentNullException("schemaProperty");
            }
            if (!schemaProperty.isBound)
            {
                throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", new object[] { schemaProperty.Name }));
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySchemaProperty property = (ActiveDirectorySchemaProperty) base.InnerList[i];
                if (Utils.Compare(property.Name, schemaProperty.Name) == 0)
                {
                    base.List.Remove(property);
                    return;
                }
            }
            throw new ArgumentException(Res.GetString("NotFoundInCollection", new object[] { schemaProperty }), "schemaProperty");
        }

        public ActiveDirectorySchemaProperty this[int index]
        {
            get
            {
                return (ActiveDirectorySchemaProperty) base.List[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!value.isBound)
                {
                    throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", new object[] { value.Name }));
                }
                if (this.Contains(value))
                {
                    throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { value }), "value");
                }
                base.List[index] = value;
            }
        }
    }
}

