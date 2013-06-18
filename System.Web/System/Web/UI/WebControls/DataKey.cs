namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;

    public class DataKey : IStateManager, IEquatable<DataKey>
    {
        private bool _isTracking;
        private string[] _keyNames;
        private IOrderedDictionary _keyTable;

        public DataKey(IOrderedDictionary keyTable)
        {
            this._keyTable = keyTable;
        }

        public DataKey(IOrderedDictionary keyTable, string[] keyNames) : this(keyTable)
        {
            this._keyNames = keyNames;
        }

        public bool Equals(DataKey other)
        {
            if (other == null)
            {
                return false;
            }
            string[] array = this._keyNames;
            string[] strArray2 = other._keyNames;
            if ((array == null) && (this._keyTable != null))
            {
                array = new string[this._keyTable.Count];
                this._keyTable.Keys.CopyTo(array, 0);
            }
            if ((strArray2 == null) && (this._keyTable != null))
            {
                strArray2 = new string[other._keyTable.Count];
                other._keyTable.Keys.CopyTo(strArray2, 0);
            }
            if (!DataBoundControlHelper.CompareStringArrays(array, strArray2))
            {
                return false;
            }
            if ((array != null) && (strArray2 != null))
            {
                foreach (string str in array)
                {
                    if (!object.Equals(this[str], other[str]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        protected virtual void LoadViewState(object state)
        {
            if (state != null)
            {
                if (this._keyNames == null)
                {
                    if (state != null)
                    {
                        ArrayList list = state as ArrayList;
                        if (list == null)
                        {
                            throw new HttpException(System.Web.SR.GetString("ViewState_InvalidViewState"));
                        }
                        OrderedDictionaryStateHelper.LoadViewState(this._keyTable, list);
                    }
                }
                else
                {
                    object[] objArray = (object[]) state;
                    if (objArray[0] != null)
                    {
                        for (int i = 0; (i < objArray.Length) && (i < this._keyNames.Length); i++)
                        {
                            this._keyTable.Add(this._keyNames[i], objArray[i]);
                        }
                    }
                }
            }
        }

        protected virtual object SaveViewState()
        {
            int count = this._keyTable.Count;
            if (count <= 0)
            {
                return null;
            }
            if (this._keyNames != null)
            {
                object obj2 = new object[count];
                for (int i = 0; i < count; i++)
                {
                    ((object[]) obj2)[i] = this._keyTable[i];
                }
                return obj2;
            }
            return OrderedDictionaryStateHelper.SaveViewState(this._keyTable);
        }

        void IStateManager.LoadViewState(object state)
        {
            this.LoadViewState(state);
        }

        object IStateManager.SaveViewState()
        {
            return this.SaveViewState();
        }

        void IStateManager.TrackViewState()
        {
            this.TrackViewState();
        }

        protected virtual void TrackViewState()
        {
            this._isTracking = true;
        }

        protected virtual bool IsTrackingViewState
        {
            get
            {
                return this._isTracking;
            }
        }

        public virtual object this[int index]
        {
            get
            {
                if (this._keyTable != null)
                {
                    return this._keyTable[index];
                }
                return null;
            }
        }

        public virtual object this[string name]
        {
            get
            {
                if (this._keyTable != null)
                {
                    return this._keyTable[name];
                }
                return null;
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this.IsTrackingViewState;
            }
        }

        public virtual object Value
        {
            get
            {
                if ((this._keyTable != null) && (this._keyTable.Count > 0))
                {
                    return this._keyTable[0];
                }
                return null;
            }
        }

        public virtual IOrderedDictionary Values
        {
            get
            {
                if (this._keyTable == null)
                {
                    return null;
                }
                if (this._keyTable is OrderedDictionary)
                {
                    return ((OrderedDictionary) this._keyTable).AsReadOnly();
                }
                if (this._keyTable is ICloneable)
                {
                    return (IOrderedDictionary) ((ICloneable) this._keyTable).Clone();
                }
                OrderedDictionary dictionary = new OrderedDictionary();
                foreach (DictionaryEntry entry in this._keyTable)
                {
                    dictionary.Add(entry.Key, entry.Value);
                }
                return dictionary.AsReadOnly();
            }
        }
    }
}

