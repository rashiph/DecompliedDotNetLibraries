namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web.UI;

    public sealed class DataKeyArray : ICollection, IEnumerable, IStateManager
    {
        private bool _isTracking;
        private ArrayList _keys;

        public DataKeyArray(ArrayList keys)
        {
            this._keys = keys;
        }

        public void CopyTo(DataKey[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this._keys.GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            IEnumerator enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                array.SetValue(enumerator.Current, index++);
            }
        }

        void IStateManager.LoadViewState(object state)
        {
            if (state != null)
            {
                object[] objArray = (object[]) state;
                for (int i = 0; i < objArray.Length; i++)
                {
                    if (objArray[i] != null)
                    {
                        ((IStateManager) this._keys[i]).LoadViewState(objArray[i]);
                    }
                }
            }
        }

        object IStateManager.SaveViewState()
        {
            int count = this._keys.Count;
            object[] objArray = new object[count];
            bool flag = false;
            for (int i = 0; i < count; i++)
            {
                objArray[i] = ((IStateManager) this._keys[i]).SaveViewState();
                if (objArray[i] != null)
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                return null;
            }
            return objArray;
        }

        void IStateManager.TrackViewState()
        {
            this._isTracking = true;
            int count = this._keys.Count;
            for (int i = 0; i < count; i++)
            {
                ((IStateManager) this._keys[i]).TrackViewState();
            }
        }

        public int Count
        {
            get
            {
                return this._keys.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public DataKey this[int index]
        {
            get
            {
                return (this._keys[index] as DataKey);
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this._isTracking;
            }
        }
    }
}

