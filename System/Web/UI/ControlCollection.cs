namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime;
    using System.Web;

    public class ControlCollection : ICollection, IEnumerable
    {
        private Control[] _controls;
        private int _defaultCapacity;
        private int _growthFactor;
        private Control _owner;
        private string _readOnlyErrorMsg;
        private int _size;
        private int _version;

        public ControlCollection(Control owner)
        {
            this._defaultCapacity = 5;
            this._growthFactor = 4;
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            this._owner = owner;
        }

        internal ControlCollection(Control owner, int defaultCapacity, int growthFactor)
        {
            this._defaultCapacity = 5;
            this._growthFactor = 4;
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            this._owner = owner;
            this._defaultCapacity = defaultCapacity;
            this._growthFactor = growthFactor;
        }

        public virtual void Add(Control child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }
            if (this._readOnlyErrorMsg != null)
            {
                throw new HttpException(System.Web.SR.GetString(this._readOnlyErrorMsg));
            }
            if (this._controls == null)
            {
                this._controls = new Control[this._defaultCapacity];
            }
            else if (this._size >= this._controls.Length)
            {
                Control[] destinationArray = new Control[this._controls.Length * this._growthFactor];
                Array.Copy(this._controls, destinationArray, this._controls.Length);
                this._controls = destinationArray;
            }
            int index = this._size;
            this._controls[index] = child;
            this._size++;
            this._version++;
            this._owner.AddedControl(child, index);
        }

        public virtual void AddAt(int index, Control child)
        {
            if (index == -1)
            {
                this.Add(child);
            }
            else
            {
                if (child == null)
                {
                    throw new ArgumentNullException("child");
                }
                if ((index < 0) || (index > this._size))
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                if (this._readOnlyErrorMsg != null)
                {
                    throw new HttpException(System.Web.SR.GetString(this._readOnlyErrorMsg));
                }
                if (this._controls == null)
                {
                    this._controls = new Control[this._defaultCapacity];
                }
                else if (this._size >= this._controls.Length)
                {
                    Control[] destinationArray = new Control[this._controls.Length * this._growthFactor];
                    Array.Copy(this._controls, destinationArray, index);
                    destinationArray[index] = child;
                    Array.Copy(this._controls, index, destinationArray, index + 1, this._size - index);
                    this._controls = destinationArray;
                }
                else if (index < this._size)
                {
                    Array.Copy(this._controls, index, this._controls, index + 1, this._size - index);
                }
                this._controls[index] = child;
                this._size++;
                this._version++;
                this._owner.AddedControl(child, index);
            }
        }

        public virtual void Clear()
        {
            if (this._controls != null)
            {
                for (int i = this._size - 1; i >= 0; i--)
                {
                    this.RemoveAt(i);
                }
                if (this._owner is INamingContainer)
                {
                    this._owner.ClearNamingContainer();
                }
            }
        }

        public virtual bool Contains(Control c)
        {
            if ((this._controls != null) && (c != null))
            {
                for (int i = 0; i < this._size; i++)
                {
                    if (object.ReferenceEquals(c, this._controls[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void CopyTo(Array array, int index)
        {
            if (this._controls != null)
            {
                if ((array != null) && (array.Rank != 1))
                {
                    throw new HttpException(System.Web.SR.GetString("InvalidArgumentValue", new object[] { "array" }));
                }
                Array.Copy(this._controls, 0, array, index, this._size);
            }
        }

        public virtual IEnumerator GetEnumerator()
        {
            return new ControlCollectionEnumerator(this);
        }

        public virtual int IndexOf(Control value)
        {
            if (this._controls == null)
            {
                return -1;
            }
            return Array.IndexOf<Control>(this._controls, value, 0, this._size);
        }

        public virtual void Remove(Control value)
        {
            int index = this.IndexOf(value);
            if (index >= 0)
            {
                this.RemoveAt(index);
            }
        }

        public virtual void RemoveAt(int index)
        {
            if (this._readOnlyErrorMsg != null)
            {
                throw new HttpException(System.Web.SR.GetString(this._readOnlyErrorMsg));
            }
            Control control = this[index];
            this._size--;
            if (index < this._size)
            {
                Array.Copy(this._controls, index + 1, this._controls, index, this._size - index);
            }
            this._controls[this._size] = null;
            this._version++;
            this._owner.RemovedControl(control);
        }

        internal string SetCollectionReadOnly(string errorMsg)
        {
            string str = this._readOnlyErrorMsg;
            this._readOnlyErrorMsg = errorMsg;
            return str;
        }

        public virtual int Count
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this._size;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return (this._readOnlyErrorMsg != null);
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public virtual Control this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this._size))
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                return this._controls[index];
            }
        }

        protected Control Owner
        {
            get
            {
                return this._owner;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        private class ControlCollectionEnumerator : IEnumerator
        {
            private Control currentElement;
            private int index;
            private ControlCollection list;
            private int version;

            internal ControlCollectionEnumerator(ControlCollection list)
            {
                this.list = list;
                this.index = -1;
                this.version = list._version;
            }

            public bool MoveNext()
            {
                if (this.index < (this.list.Count - 1))
                {
                    if (this.version != this.list._version)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("ListEnumVersionMismatch"));
                    }
                    this.index++;
                    this.currentElement = this.list[this.index];
                    return true;
                }
                this.index = this.list.Count;
                return false;
            }

            public void Reset()
            {
                if (this.version != this.list._version)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ListEnumVersionMismatch"));
                }
                this.currentElement = null;
                this.index = -1;
            }

            public Control Current
            {
                [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
                get
                {
                    if (this.index == -1)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("ListEnumCurrentOutOfRange"));
                    }
                    if (this.index >= this.list.Count)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("ListEnumCurrentOutOfRange"));
                    }
                    return this.currentElement;
                }
            }

            object IEnumerator.Current
            {
                [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
                get
                {
                    return this.Current;
                }
            }
        }
    }
}

