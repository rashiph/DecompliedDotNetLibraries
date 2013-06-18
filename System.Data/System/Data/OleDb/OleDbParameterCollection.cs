namespace System.Data.OleDb
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;

    [Editor("Microsoft.VSDesigner.Data.Design.DBParametersEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ListBindable(false)]
    public sealed class OleDbParameterCollection : DbParameterCollection
    {
        private int _changeID;
        private List<OleDbParameter> _items;
        private static Type ItemType = typeof(OleDbParameter);

        internal OleDbParameterCollection()
        {
        }

        public OleDbParameter Add(OleDbParameter value)
        {
            this.Add(value);
            return value;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int Add(object value)
        {
            this.OnChange();
            this.ValidateType(value);
            this.Validate(-1, value);
            this.InnerList.Add((OleDbParameter) value);
            return (this.Count - 1);
        }

        public OleDbParameter Add(string parameterName, OleDbType oleDbType)
        {
            return this.Add(new OleDbParameter(parameterName, oleDbType));
        }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Add(String parameterName, Object value) has been deprecated.  Use AddWithValue(String parameterName, Object value).  http://go.microsoft.com/fwlink/?linkid=14202", false)]
        public OleDbParameter Add(string parameterName, object value)
        {
            return this.Add(new OleDbParameter(parameterName, value));
        }

        public OleDbParameter Add(string parameterName, OleDbType oleDbType, int size)
        {
            return this.Add(new OleDbParameter(parameterName, oleDbType, size));
        }

        public OleDbParameter Add(string parameterName, OleDbType oleDbType, int size, string sourceColumn)
        {
            return this.Add(new OleDbParameter(parameterName, oleDbType, size, sourceColumn));
        }

        public void AddRange(OleDbParameter[] values)
        {
            this.AddRange(values);
        }

        public override void AddRange(Array values)
        {
            this.OnChange();
            if (values == null)
            {
                throw ADP.ArgumentNull("values");
            }
            foreach (object obj2 in values)
            {
                this.ValidateType(obj2);
            }
            foreach (OleDbParameter parameter in values)
            {
                this.Validate(-1, parameter);
                this.InnerList.Add(parameter);
            }
        }

        public OleDbParameter AddWithValue(string parameterName, object value)
        {
            return this.Add(new OleDbParameter(parameterName, value));
        }

        private int CheckName(string parameterName)
        {
            int index = this.IndexOf(parameterName);
            if (index < 0)
            {
                throw ADP.ParametersSourceIndex(parameterName, this, ItemType);
            }
            return index;
        }

        public override void Clear()
        {
            this.OnChange();
            List<OleDbParameter> innerList = this.InnerList;
            if (innerList != null)
            {
                foreach (OleDbParameter parameter in innerList)
                {
                    parameter.ResetParent();
                }
                innerList.Clear();
            }
        }

        public bool Contains(OleDbParameter value)
        {
            return (-1 != this.IndexOf(value));
        }

        public override bool Contains(object value)
        {
            return (-1 != this.IndexOf(value));
        }

        public override bool Contains(string value)
        {
            return (-1 != this.IndexOf(value));
        }

        public void CopyTo(OleDbParameter[] array, int index)
        {
            this.CopyTo(array, index);
        }

        public override void CopyTo(Array array, int index)
        {
            this.InnerList.CopyTo(array, index);
        }

        public override IEnumerator GetEnumerator()
        {
            return this.InnerList.GetEnumerator();
        }

        protected override DbParameter GetParameter(int index)
        {
            this.RangeCheck(index);
            return this.InnerList[index];
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            int index = this.IndexOf(parameterName);
            if (index < 0)
            {
                throw ADP.ParametersSourceIndex(parameterName, this, ItemType);
            }
            return this.InnerList[index];
        }

        public int IndexOf(OleDbParameter value)
        {
            return this.IndexOf(value);
        }

        public override int IndexOf(object value)
        {
            if (value != null)
            {
                this.ValidateType(value);
                List<OleDbParameter> innerList = this.InnerList;
                if (innerList != null)
                {
                    int count = innerList.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (value == innerList[i])
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        public override int IndexOf(string parameterName)
        {
            return IndexOf(this.InnerList, parameterName);
        }

        private static int IndexOf(IEnumerable items, string parameterName)
        {
            if (items != null)
            {
                int num = 0;
                foreach (OleDbParameter parameter2 in items)
                {
                    if (ADP.SrcCompare(parameterName, parameter2.ParameterName) == 0)
                    {
                        return num;
                    }
                    num++;
                }
                num = 0;
                foreach (OleDbParameter parameter in items)
                {
                    if (ADP.DstCompare(parameterName, parameter.ParameterName) == 0)
                    {
                        return num;
                    }
                    num++;
                }
            }
            return -1;
        }

        public void Insert(int index, OleDbParameter value)
        {
            this.Insert(index, value);
        }

        public override void Insert(int index, object value)
        {
            this.OnChange();
            this.ValidateType(value);
            this.Validate(-1, (OleDbParameter) value);
            this.InnerList.Insert(index, (OleDbParameter) value);
        }

        private void OnChange()
        {
            this._changeID++;
        }

        private void RangeCheck(int index)
        {
            if ((index < 0) || (this.Count <= index))
            {
                throw ADP.ParametersMappingIndex(index, this);
            }
        }

        public void Remove(OleDbParameter value)
        {
            this.Remove(value);
        }

        public override void Remove(object value)
        {
            this.OnChange();
            this.ValidateType(value);
            int index = this.IndexOf(value);
            if (-1 != index)
            {
                this.RemoveIndex(index);
            }
            else if (this != ((OleDbParameter) value).CompareExchangeParent(null, this))
            {
                throw ADP.CollectionRemoveInvalidObject(ItemType, this);
            }
        }

        public override void RemoveAt(int index)
        {
            this.OnChange();
            this.RangeCheck(index);
            this.RemoveIndex(index);
        }

        public override void RemoveAt(string parameterName)
        {
            this.OnChange();
            int index = this.CheckName(parameterName);
            this.RemoveIndex(index);
        }

        private void RemoveIndex(int index)
        {
            List<OleDbParameter> innerList = this.InnerList;
            OleDbParameter parameter = innerList[index];
            innerList.RemoveAt(index);
            parameter.ResetParent();
        }

        private void Replace(int index, object newValue)
        {
            List<OleDbParameter> innerList = this.InnerList;
            this.ValidateType(newValue);
            this.Validate(index, newValue);
            OleDbParameter parameter = innerList[index];
            innerList[index] = (OleDbParameter) newValue;
            parameter.ResetParent();
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            this.OnChange();
            this.RangeCheck(index);
            this.Replace(index, value);
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            this.OnChange();
            int index = this.IndexOf(parameterName);
            if (index < 0)
            {
                throw ADP.ParametersSourceIndex(parameterName, this, ItemType);
            }
            this.Replace(index, value);
        }

        private void Validate(int index, object value)
        {
            if (value == null)
            {
                throw ADP.ParameterNull("value", this, ItemType);
            }
            object obj2 = ((OleDbParameter) value).CompareExchangeParent(this, null);
            if (obj2 != null)
            {
                if (this != obj2)
                {
                    throw ADP.ParametersIsNotParent(ItemType, this);
                }
                if (index != this.IndexOf(value))
                {
                    throw ADP.ParametersIsParent(ItemType, this);
                }
            }
            if (((OleDbParameter) value).ParameterName.Length == 0)
            {
                string str;
                index = 1;
                do
                {
                    str = "Parameter" + index.ToString(CultureInfo.CurrentCulture);
                    index++;
                }
                while (-1 != this.IndexOf(str));
                ((OleDbParameter) value).ParameterName = str;
            }
        }

        private void ValidateType(object value)
        {
            if (value == null)
            {
                throw ADP.ParameterNull("value", this, ItemType);
            }
            if (!ItemType.IsInstanceOfType(value))
            {
                throw ADP.InvalidParameterType(this, ItemType, value);
            }
        }

        internal int ChangeID
        {
            get
            {
                return this._changeID;
            }
        }

        public override int Count
        {
            get
            {
                if (this._items == null)
                {
                    return 0;
                }
                return this._items.Count;
            }
        }

        private List<OleDbParameter> InnerList
        {
            get
            {
                List<OleDbParameter> list = this._items;
                if (list == null)
                {
                    list = new List<OleDbParameter>();
                    this._items = list;
                }
                return list;
            }
        }

        public override bool IsFixedSize
        {
            get
            {
                return ((IList) this.InnerList).IsFixedSize;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return ((IList) this.InnerList).IsReadOnly;
            }
        }

        public override bool IsSynchronized
        {
            get
            {
                return ((ICollection) this.InnerList).IsSynchronized;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OleDbParameter this[int index]
        {
            get
            {
                return (OleDbParameter) this.GetParameter(index);
            }
            set
            {
                this.SetParameter(index, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public OleDbParameter this[string parameterName]
        {
            get
            {
                return (OleDbParameter) this.GetParameter(parameterName);
            }
            set
            {
                this.SetParameter(parameterName, value);
            }
        }

        public override object SyncRoot
        {
            get
            {
                return ((ICollection) this.InnerList).SyncRoot;
            }
        }
    }
}

