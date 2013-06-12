namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web;

    public abstract class StateManagedCollection : IList, ICollection, IEnumerable, IStateManager
    {
        private ArrayList _collectionItems = new ArrayList();
        private bool _hadItems;
        private bool _saveAll;
        private bool _tracking;

        protected StateManagedCollection()
        {
        }

        public void Clear()
        {
            this.OnClear();
            this._collectionItems.Clear();
            this.OnClearComplete();
            if (this._tracking)
            {
                this._saveAll = true;
            }
        }

        public void CopyTo(Array array, int index)
        {
            this._collectionItems.CopyTo(array, index);
        }

        protected virtual object CreateKnownType(int index)
        {
            throw new InvalidOperationException(System.Web.SR.GetString("StateManagedCollection_NoKnownTypes"));
        }

        public IEnumerator GetEnumerator()
        {
            return this._collectionItems.GetEnumerator();
        }

        private int GetKnownTypeCount()
        {
            Type[] knownTypes = this.GetKnownTypes();
            if (knownTypes == null)
            {
                return 0;
            }
            return knownTypes.Length;
        }

        protected virtual Type[] GetKnownTypes()
        {
            return null;
        }

        private void InsertInternal(int index, object o)
        {
            int num;
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }
            if (((IStateManager) this).IsTrackingViewState)
            {
                ((IStateManager) o).TrackViewState();
                this.SetDirtyObject(o);
            }
            this.OnInsert(index, o);
            if (index == -1)
            {
                num = this._collectionItems.Add(o);
            }
            else
            {
                num = index;
                this._collectionItems.Insert(index, o);
            }
            try
            {
                this.OnInsertComplete(index, o);
            }
            catch
            {
                this._collectionItems.RemoveAt(num);
                throw;
            }
        }

        private void LoadAllItemsFromViewState(object savedState)
        {
            Pair pair = (Pair) savedState;
            if (pair.Second is Pair)
            {
                Pair second = (Pair) pair.Second;
                object[] first = (object[]) pair.First;
                int[] numArray = (int[]) second.First;
                ArrayList list = (ArrayList) second.Second;
                this.Clear();
                for (int i = 0; i < first.Length; i++)
                {
                    object obj2;
                    if (numArray == null)
                    {
                        obj2 = this.CreateKnownType(0);
                    }
                    else
                    {
                        int index = numArray[i];
                        if (index < this.GetKnownTypeCount())
                        {
                            obj2 = this.CreateKnownType(index);
                        }
                        else
                        {
                            string typeName = (string) list[index - this.GetKnownTypeCount()];
                            obj2 = Activator.CreateInstance(Type.GetType(typeName));
                        }
                    }
                    ((IStateManager) obj2).TrackViewState();
                    ((IStateManager) obj2).LoadViewState(first[i]);
                    ((IList) this).Add(obj2);
                }
            }
            else
            {
                object[] objArray2 = (object[]) pair.First;
                int[] numArray2 = (int[]) pair.Second;
                this.Clear();
                for (int j = 0; j < objArray2.Length; j++)
                {
                    int num4 = 0;
                    if (numArray2 != null)
                    {
                        num4 = numArray2[j];
                    }
                    object obj3 = this.CreateKnownType(num4);
                    ((IStateManager) obj3).TrackViewState();
                    ((IStateManager) obj3).LoadViewState(objArray2[j]);
                    ((IList) this).Add(obj3);
                }
            }
        }

        private void LoadChangedItemsFromViewState(object savedState)
        {
            Triplet triplet = (Triplet) savedState;
            if (triplet.Third is Pair)
            {
                Pair third = (Pair) triplet.Third;
                ArrayList first = (ArrayList) triplet.First;
                ArrayList second = (ArrayList) triplet.Second;
                ArrayList list3 = (ArrayList) third.First;
                ArrayList list4 = (ArrayList) third.Second;
                for (int i = 0; i < first.Count; i++)
                {
                    int num2 = (int) first[i];
                    if (num2 < this.Count)
                    {
                        ((IStateManager) ((IList) this)[num2]).LoadViewState(second[i]);
                    }
                    else
                    {
                        object obj2;
                        if (list3 == null)
                        {
                            obj2 = this.CreateKnownType(0);
                        }
                        else
                        {
                            int index = (int) list3[i];
                            if (index < this.GetKnownTypeCount())
                            {
                                obj2 = this.CreateKnownType(index);
                            }
                            else
                            {
                                string typeName = (string) list4[index - this.GetKnownTypeCount()];
                                obj2 = Activator.CreateInstance(Type.GetType(typeName));
                            }
                        }
                        ((IStateManager) obj2).TrackViewState();
                        ((IStateManager) obj2).LoadViewState(second[i]);
                        ((IList) this).Add(obj2);
                    }
                }
            }
            else
            {
                ArrayList list5 = (ArrayList) triplet.First;
                ArrayList list6 = (ArrayList) triplet.Second;
                ArrayList list7 = (ArrayList) triplet.Third;
                for (int j = 0; j < list5.Count; j++)
                {
                    int num5 = (int) list5[j];
                    if (num5 < this.Count)
                    {
                        ((IStateManager) ((IList) this)[num5]).LoadViewState(list6[j]);
                    }
                    else
                    {
                        int num6 = 0;
                        if (list7 != null)
                        {
                            num6 = (int) list7[j];
                        }
                        object obj3 = this.CreateKnownType(num6);
                        ((IStateManager) obj3).TrackViewState();
                        ((IStateManager) obj3).LoadViewState(list6[j]);
                        ((IList) this).Add(obj3);
                    }
                }
            }
        }

        protected virtual void OnClear()
        {
        }

        protected virtual void OnClearComplete()
        {
        }

        protected virtual void OnInsert(int index, object value)
        {
        }

        protected virtual void OnInsertComplete(int index, object value)
        {
        }

        protected virtual void OnRemove(int index, object value)
        {
        }

        protected virtual void OnRemoveComplete(int index, object value)
        {
        }

        protected virtual void OnValidate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
        }

        private object SaveAllItemsToViewState()
        {
            bool flag = false;
            int count = this._collectionItems.Count;
            int[] x = new int[count];
            object[] objArray = new object[count];
            ArrayList y = null;
            IDictionary dictionary = null;
            int knownTypeCount = this.GetKnownTypeCount();
            for (int i = 0; i < count; i++)
            {
                object o = this._collectionItems[i];
                this.SetDirtyObject(o);
                objArray[i] = ((IStateManager) o).SaveViewState();
                if (objArray[i] != null)
                {
                    flag = true;
                }
                Type type = o.GetType();
                int index = -1;
                if (knownTypeCount != 0)
                {
                    index = this.GetKnownTypes().IndexOf(type);
                }
                if (index != -1)
                {
                    x[i] = index;
                }
                else
                {
                    if (y == null)
                    {
                        y = new ArrayList();
                        dictionary = new HybridDictionary();
                    }
                    object obj3 = dictionary[type];
                    if (obj3 == null)
                    {
                        y.Add(type.AssemblyQualifiedName);
                        obj3 = (y.Count + knownTypeCount) - 1;
                        dictionary[type] = obj3;
                    }
                    x[i] = (int) obj3;
                }
            }
            if (!this._hadItems && !flag)
            {
                return null;
            }
            if (y != null)
            {
                return new Pair(objArray, new Pair(x, y));
            }
            if (knownTypeCount == 1)
            {
                x = null;
            }
            return new Pair(objArray, x);
        }

        private object SaveChangedItemsToViewState()
        {
            bool flag = false;
            int count = this._collectionItems.Count;
            ArrayList x = new ArrayList();
            ArrayList y = new ArrayList();
            ArrayList list3 = new ArrayList();
            ArrayList list4 = null;
            IDictionary dictionary = null;
            int knownTypeCount = this.GetKnownTypeCount();
            for (int i = 0; i < count; i++)
            {
                object obj2 = this._collectionItems[i];
                object obj3 = ((IStateManager) obj2).SaveViewState();
                if (obj3 != null)
                {
                    flag = true;
                    x.Add(i);
                    y.Add(obj3);
                    Type type = obj2.GetType();
                    int index = -1;
                    if (knownTypeCount != 0)
                    {
                        index = this.GetKnownTypes().IndexOf(type);
                    }
                    if (index != -1)
                    {
                        list3.Add(index);
                    }
                    else
                    {
                        if (list4 == null)
                        {
                            list4 = new ArrayList();
                            dictionary = new HybridDictionary();
                        }
                        object obj4 = dictionary[type];
                        if (obj4 == null)
                        {
                            list4.Add(type.AssemblyQualifiedName);
                            obj4 = (list4.Count + knownTypeCount) - 1;
                            dictionary[type] = obj4;
                        }
                        list3.Add(obj4);
                    }
                }
            }
            if (!this._hadItems && !flag)
            {
                return null;
            }
            if (list4 != null)
            {
                return new Triplet(x, y, new Pair(list3, list4));
            }
            if (knownTypeCount == 1)
            {
                list3 = null;
            }
            return new Triplet(x, y, list3);
        }

        public void SetDirty()
        {
            this._saveAll = true;
        }

        protected abstract void SetDirtyObject(object o);
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        int IList.Add(object value)
        {
            this.OnValidate(value);
            this.InsertInternal(-1, value);
            return (this._collectionItems.Count - 1);
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            if (value == null)
            {
                return false;
            }
            this.OnValidate(value);
            return this._collectionItems.Contains(value);
        }

        int IList.IndexOf(object value)
        {
            if (value == null)
            {
                return -1;
            }
            this.OnValidate(value);
            return this._collectionItems.IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((index < 0) || (index > this.Count))
            {
                throw new ArgumentOutOfRangeException("index", System.Web.SR.GetString("StateManagedCollection_InvalidIndex"));
            }
            this.OnValidate(value);
            this.InsertInternal(index, value);
            if (this._tracking)
            {
                this._saveAll = true;
            }
        }

        void IList.Remove(object value)
        {
            if (value != null)
            {
                this.OnValidate(value);
                ((IList) this).RemoveAt(((IList) this).IndexOf(value));
            }
        }

        void IList.RemoveAt(int index)
        {
            object obj2 = this._collectionItems[index];
            this.OnRemove(index, obj2);
            this._collectionItems.RemoveAt(index);
            try
            {
                this.OnRemoveComplete(index, obj2);
            }
            catch
            {
                this._collectionItems.Insert(index, obj2);
                throw;
            }
            if (this._tracking)
            {
                this._saveAll = true;
            }
        }

        void IStateManager.LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                if (savedState is Triplet)
                {
                    this.LoadChangedItemsFromViewState(savedState);
                }
                else
                {
                    this.LoadAllItemsFromViewState(savedState);
                }
            }
        }

        object IStateManager.SaveViewState()
        {
            if (this._saveAll)
            {
                return this.SaveAllItemsToViewState();
            }
            return this.SaveChangedItemsToViewState();
        }

        void IStateManager.TrackViewState()
        {
            if (!((IStateManager) this).IsTrackingViewState)
            {
                this._hadItems = this.Count > 0;
                this._tracking = true;
                foreach (IStateManager manager in this._collectionItems)
                {
                    manager.TrackViewState();
                }
            }
        }

        public int Count
        {
            get
            {
                return this._collectionItems.Count;
            }
        }

        int ICollection.Count
        {
            get
            {
                return this.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return null;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return this._collectionItems.IsReadOnly;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this._collectionItems[index];
            }
            set
            {
                if ((index < 0) || (index >= this.Count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Web.SR.GetString("StateManagedCollection_InvalidIndex"));
                }
                ((IList) this).RemoveAt(index);
                ((IList) this).Insert(index, value);
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this._tracking;
            }
        }
    }
}

