namespace System.DirectoryServices
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class PropertyValueCollection : CollectionBase
    {
        private bool allowMultipleChange;
        private ArrayList changeList;
        private DirectoryEntry entry;
        private bool needNewBehavior;
        private string propertyName;
        private UpdateType updateType = UpdateType.None;

        internal PropertyValueCollection(DirectoryEntry entry, string propertyName)
        {
            this.entry = entry;
            this.propertyName = propertyName;
            this.PopulateList();
            ArrayList list = new ArrayList();
            this.changeList = ArrayList.Synchronized(list);
            this.allowMultipleChange = entry.allowMultipleChange;
            string path = entry.Path;
            if ((path == null) || (path.Length == 0))
            {
                this.needNewBehavior = true;
            }
            else if (path.StartsWith("LDAP:", StringComparison.Ordinal))
            {
                this.needNewBehavior = true;
            }
        }

        public int Add(object value)
        {
            return base.List.Add(value);
        }

        public void AddRange(object[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
        }

        public void AddRange(PropertyValueCollection value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            int count = value.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(value[i]);
            }
        }

        public bool Contains(object value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(object[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(object value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            base.List.Insert(index, value);
        }

        protected override void OnClearComplete()
        {
            if ((this.needNewBehavior && !this.allowMultipleChange) && ((this.updateType != UpdateType.None) && (this.updateType != UpdateType.Update)))
            {
                throw new InvalidOperationException(Res.GetString("DSPropertyValueSupportOneOperation"));
            }
            this.entry.AdsObject.PutEx(1, this.propertyName, null);
            this.updateType = UpdateType.Update;
            try
            {
                this.entry.CommitIfNotCaching();
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode != -2147016694)
                {
                    throw;
                }
            }
        }

        protected override void OnInsertComplete(int index, object value)
        {
            if (this.needNewBehavior)
            {
                if (!this.allowMultipleChange)
                {
                    if ((this.updateType != UpdateType.None) && (this.updateType != UpdateType.Add))
                    {
                        throw new InvalidOperationException(Res.GetString("DSPropertyValueSupportOneOperation"));
                    }
                    this.changeList.Add(value);
                    object[] array = new object[this.changeList.Count];
                    this.changeList.CopyTo(array, 0);
                    this.entry.AdsObject.PutEx(3, this.propertyName, array);
                    this.updateType = UpdateType.Add;
                }
                else
                {
                    this.entry.AdsObject.PutEx(3, this.propertyName, new object[] { value });
                }
            }
            else
            {
                object[] objArray2 = new object[base.InnerList.Count];
                base.InnerList.CopyTo(objArray2, 0);
                this.entry.AdsObject.PutEx(2, this.propertyName, objArray2);
            }
            this.entry.CommitIfNotCaching();
        }

        protected override void OnRemoveComplete(int index, object value)
        {
            if (this.needNewBehavior)
            {
                if (!this.allowMultipleChange)
                {
                    if ((this.updateType != UpdateType.None) && (this.updateType != UpdateType.Delete))
                    {
                        throw new InvalidOperationException(Res.GetString("DSPropertyValueSupportOneOperation"));
                    }
                    this.changeList.Add(value);
                    object[] array = new object[this.changeList.Count];
                    this.changeList.CopyTo(array, 0);
                    this.entry.AdsObject.PutEx(4, this.propertyName, array);
                    this.updateType = UpdateType.Delete;
                }
                else
                {
                    this.entry.AdsObject.PutEx(4, this.propertyName, new object[] { value });
                }
            }
            else
            {
                object[] objArray2 = new object[base.InnerList.Count];
                base.InnerList.CopyTo(objArray2, 0);
                this.entry.AdsObject.PutEx(2, this.propertyName, objArray2);
            }
            this.entry.CommitIfNotCaching();
        }

        protected override void OnSetComplete(int index, object oldValue, object newValue)
        {
            if (base.Count <= 1)
            {
                this.entry.AdsObject.Put(this.propertyName, newValue);
            }
            else if (this.needNewBehavior)
            {
                this.entry.AdsObject.PutEx(4, this.propertyName, new object[] { oldValue });
                this.entry.AdsObject.PutEx(3, this.propertyName, new object[] { newValue });
            }
            else
            {
                object[] array = new object[base.InnerList.Count];
                base.InnerList.CopyTo(array, 0);
                this.entry.AdsObject.PutEx(2, this.propertyName, array);
            }
            this.entry.CommitIfNotCaching();
        }

        private void PopulateList()
        {
            object obj2;
            int ex = this.entry.AdsObject.GetEx(this.propertyName, out obj2);
            if (ex != 0)
            {
                if ((ex != -2147463155) && (ex != -2147463162))
                {
                    throw COMExceptionHelper.CreateFormattedComException(ex);
                }
            }
            else if (obj2 is ICollection)
            {
                base.InnerList.AddRange((ICollection) obj2);
            }
            else
            {
                base.InnerList.Add(obj2);
            }
        }

        public void Remove(object value)
        {
            if (this.needNewBehavior)
            {
                try
                {
                    base.List.Remove(value);
                }
                catch (ArgumentException)
                {
                    this.OnRemoveComplete(0, value);
                }
            }
            else
            {
                base.List.Remove(value);
            }
        }

        public object this[int index]
        {
            get
            {
                return base.List[index];
            }
            set
            {
                if (this.needNewBehavior && !this.allowMultipleChange)
                {
                    throw new NotSupportedException();
                }
                base.List[index] = value;
            }
        }

        [ComVisible(false)]
        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }
        }

        public object Value
        {
            get
            {
                if (base.Count == 0)
                {
                    return null;
                }
                if (base.Count == 1)
                {
                    return base.List[0];
                }
                object[] array = new object[base.Count];
                base.List.CopyTo(array, 0);
                return array;
            }
            set
            {
                try
                {
                    base.Clear();
                }
                catch (COMException exception)
                {
                    if ((exception.ErrorCode != -2147467259) || (value == null))
                    {
                        throw;
                    }
                }
                if (value != null)
                {
                    this.changeList.Clear();
                    if (value is Array)
                    {
                        if (value is byte[])
                        {
                            this.changeList.Add(value);
                        }
                        else if (value is object[])
                        {
                            this.changeList.AddRange((object[]) value);
                        }
                        else
                        {
                            object[] objArray = new object[((Array) value).Length];
                            ((Array) value).CopyTo(objArray, 0);
                            this.changeList.AddRange(objArray);
                        }
                    }
                    else
                    {
                        this.changeList.Add(value);
                    }
                    object[] array = new object[this.changeList.Count];
                    this.changeList.CopyTo(array, 0);
                    this.entry.AdsObject.PutEx(2, this.propertyName, array);
                    this.entry.CommitIfNotCaching();
                    this.PopulateList();
                }
            }
        }

        internal enum UpdateType
        {
            Add,
            Delete,
            Update,
            None
        }
    }
}

