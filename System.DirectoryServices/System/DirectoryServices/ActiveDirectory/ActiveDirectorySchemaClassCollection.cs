namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.DirectoryServices;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ActiveDirectorySchemaClassCollection : CollectionBase
    {
        private DirectoryEntry classEntry;
        private DirectoryContext context;
        private bool isBound;
        private string propertyName;
        private ActiveDirectorySchemaClass schemaClass;

        internal ActiveDirectorySchemaClassCollection(DirectoryContext context, ActiveDirectorySchemaClass schemaClass, bool isBound, string propertyName, ICollection classes)
        {
            this.schemaClass = schemaClass;
            this.propertyName = propertyName;
            this.isBound = isBound;
            this.context = context;
            foreach (ActiveDirectorySchemaClass class2 in classes)
            {
                base.InnerList.Add(class2);
            }
        }

        internal ActiveDirectorySchemaClassCollection(DirectoryContext context, ActiveDirectorySchemaClass schemaClass, bool isBound, string propertyName, ICollection classNames, bool onlyNames)
        {
            this.schemaClass = schemaClass;
            this.propertyName = propertyName;
            this.isBound = isBound;
            this.context = context;
            foreach (string str in classNames)
            {
                base.InnerList.Add(new ActiveDirectorySchemaClass(context, str, null, null));
            }
        }

        public int Add(ActiveDirectorySchemaClass schemaClass)
        {
            if (schemaClass == null)
            {
                throw new ArgumentNullException("schemaClass");
            }
            if (!schemaClass.isBound)
            {
                throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", new object[] { schemaClass.Name }));
            }
            if (this.Contains(schemaClass))
            {
                throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { schemaClass }), "schemaClass");
            }
            return base.List.Add(schemaClass);
        }

        public void AddRange(ActiveDirectorySchemaClass[] schemaClasses)
        {
            if (schemaClasses == null)
            {
                throw new ArgumentNullException("schemaClasses");
            }
            ActiveDirectorySchemaClass[] classArray = schemaClasses;
            for (int i = 0; i < classArray.Length; i++)
            {
                if (classArray[i] == null)
                {
                    throw new ArgumentException("schemaClasses");
                }
            }
            for (int j = 0; j < schemaClasses.Length; j++)
            {
                this.Add(schemaClasses[j]);
            }
        }

        public void AddRange(ActiveDirectorySchemaClassCollection schemaClasses)
        {
            if (schemaClasses == null)
            {
                throw new ArgumentNullException("schemaClasses");
            }
            using (IEnumerator enumerator = schemaClasses.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (((ActiveDirectorySchemaClass) enumerator.Current) == null)
                    {
                        throw new ArgumentException("schemaClasses");
                    }
                }
            }
            int count = schemaClasses.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(schemaClasses[i]);
            }
        }

        public void AddRange(ReadOnlyActiveDirectorySchemaClassCollection schemaClasses)
        {
            if (schemaClasses == null)
            {
                throw new ArgumentNullException("schemaClasses");
            }
            using (IEnumerator enumerator = schemaClasses.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (((ActiveDirectorySchemaClass) enumerator.Current) == null)
                    {
                        throw new ArgumentException("schemaClasses");
                    }
                }
            }
            int count = schemaClasses.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(schemaClasses[i]);
            }
        }

        public bool Contains(ActiveDirectorySchemaClass schemaClass)
        {
            if (schemaClass == null)
            {
                throw new ArgumentNullException("schemaClass");
            }
            if (!schemaClass.isBound)
            {
                throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", new object[] { schemaClass.Name }));
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySchemaClass class2 = (ActiveDirectorySchemaClass) base.InnerList[i];
                if (Utils.Compare(class2.Name, schemaClass.Name) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(ActiveDirectorySchemaClass[] schemaClasses, int index)
        {
            base.List.CopyTo(schemaClasses, index);
        }

        internal string[] GetMultiValuedProperty()
        {
            string[] strArray = new string[base.InnerList.Count];
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                strArray[i] = ((ActiveDirectorySchemaClass) base.InnerList[i]).Name;
            }
            return strArray;
        }

        public int IndexOf(ActiveDirectorySchemaClass schemaClass)
        {
            if (schemaClass == null)
            {
                throw new ArgumentNullException("schemaClass");
            }
            if (!schemaClass.isBound)
            {
                throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", new object[] { schemaClass.Name }));
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySchemaClass class2 = (ActiveDirectorySchemaClass) base.InnerList[i];
                if (Utils.Compare(class2.Name, schemaClass.Name) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, ActiveDirectorySchemaClass schemaClass)
        {
            if (schemaClass == null)
            {
                throw new ArgumentNullException("schemaClass");
            }
            if (!schemaClass.isBound)
            {
                throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", new object[] { schemaClass.Name }));
            }
            if (this.Contains(schemaClass))
            {
                throw new ArgumentException(Res.GetString("AlreadyExistingInCollection", new object[] { schemaClass }), "schemaClass");
            }
            base.List.Insert(index, schemaClass);
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
                    this.classEntry.Properties[this.propertyName].Add(((ActiveDirectorySchemaClass) value).Name);
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
                string name = ((ActiveDirectorySchemaClass) value).Name;
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
            if (!(value is ActiveDirectorySchemaClass))
            {
                throw new ArgumentException("value");
            }
            if (!((ActiveDirectorySchemaClass) value).isBound)
            {
                throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", new object[] { ((ActiveDirectorySchemaClass) value).Name }));
            }
        }

        public void Remove(ActiveDirectorySchemaClass schemaClass)
        {
            if (schemaClass == null)
            {
                throw new ArgumentNullException("schemaClass");
            }
            if (!schemaClass.isBound)
            {
                throw new InvalidOperationException(Res.GetString("SchemaObjectNotCommitted", new object[] { schemaClass.Name }));
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySchemaClass class2 = (ActiveDirectorySchemaClass) base.InnerList[i];
                if (Utils.Compare(class2.Name, schemaClass.Name) == 0)
                {
                    base.List.Remove(class2);
                    return;
                }
            }
            throw new ArgumentException(Res.GetString("NotFoundInCollection", new object[] { schemaClass }), "schemaClass");
        }

        public ActiveDirectorySchemaClass this[int index]
        {
            get
            {
                return (ActiveDirectorySchemaClass) base.List[index];
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

