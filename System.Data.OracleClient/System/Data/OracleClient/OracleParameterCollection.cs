namespace System.Data.OracleClient
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;

    [Editor("Microsoft.VSDesigner.Data.Design.DBParametersEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ListBindable(false)]
    public sealed class OracleParameterCollection : DbParameterCollection
    {
        private List<OracleParameter> _items;
        private static Type ItemType = typeof(OracleParameter);

        public OracleParameter Add(OracleParameter value)
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
            this.InnerList.Add((OracleParameter) value);
            return (this.Count - 1);
        }

        public OracleParameter Add(string parameterName, OracleType dataType)
        {
            OracleParameter parameter = new OracleParameter(parameterName, dataType);
            return this.Add(parameter);
        }

        [Obsolete("Add(String parameterName, Object value) has been deprecated.  Use AddWithValue(String parameterName, Object value).  http://go.microsoft.com/fwlink/?linkid=14202", false), EditorBrowsable(EditorBrowsableState.Never)]
        public OracleParameter Add(string parameterName, object value)
        {
            OracleParameter parameter = new OracleParameter(parameterName, value);
            return this.Add(parameter);
        }

        public OracleParameter Add(string parameterName, OracleType dataType, int size)
        {
            OracleParameter parameter = new OracleParameter(parameterName, dataType, size);
            return this.Add(parameter);
        }

        public OracleParameter Add(string parameterName, OracleType dataType, int size, string srcColumn)
        {
            OracleParameter parameter = new OracleParameter(parameterName, dataType, size, srcColumn);
            return this.Add(parameter);
        }

        public void AddRange(OracleParameter[] values)
        {
            this.AddRange(values);
        }

        public override void AddRange(Array values)
        {
            this.OnChange();
            if (values == null)
            {
                throw System.Data.Common.ADP.ArgumentNull("values");
            }
            foreach (object obj2 in values)
            {
                this.ValidateType(obj2);
            }
            foreach (OracleParameter parameter in values)
            {
                this.Validate(-1, parameter);
                this.InnerList.Add(parameter);
            }
        }

        public OracleParameter AddWithValue(string parameterName, object value)
        {
            OracleParameter parameter = new OracleParameter(parameterName, value);
            return this.Add(parameter);
        }

        private int CheckName(string parameterName)
        {
            int index = this.IndexOf(parameterName);
            if (index < 0)
            {
                throw System.Data.Common.ADP.ParametersSourceIndex(parameterName, this, ItemType);
            }
            return index;
        }

        public override void Clear()
        {
            this.OnChange();
            List<OracleParameter> innerList = this.InnerList;
            if (innerList != null)
            {
                foreach (OracleParameter parameter in innerList)
                {
                    parameter.ResetParent();
                }
                innerList.Clear();
            }
        }

        public bool Contains(OracleParameter value)
        {
            return (-1 != this.IndexOf(value));
        }

        public override bool Contains(object value)
        {
            return (-1 != this.IndexOf(value));
        }

        public override bool Contains(string parameterName)
        {
            return (-1 != this.IndexOf(parameterName));
        }

        public void CopyTo(OracleParameter[] array, int index)
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
                throw System.Data.Common.ADP.ParametersSourceIndex(parameterName, this, ItemType);
            }
            return this.InnerList[index];
        }

        public int IndexOf(OracleParameter value)
        {
            return this.IndexOf(value);
        }

        public override int IndexOf(object value)
        {
            if (value != null)
            {
                this.ValidateType(value);
                List<OracleParameter> innerList = this.InnerList;
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
                foreach (OracleParameter parameter2 in items)
                {
                    if (System.Data.Common.ADP.SrcCompare(parameterName, parameter2.ParameterName) == 0)
                    {
                        return num;
                    }
                    num++;
                }
                num = 0;
                foreach (OracleParameter parameter in items)
                {
                    if (System.Data.Common.ADP.DstCompare(parameterName, parameter.ParameterName) == 0)
                    {
                        return num;
                    }
                    num++;
                }
            }
            return -1;
        }

        public void Insert(int index, OracleParameter value)
        {
            this.Insert(index, value);
        }

        public override void Insert(int index, object value)
        {
            this.OnChange();
            this.ValidateType(value);
            this.Validate(-1, (OracleParameter) value);
            this.InnerList.Insert(index, (OracleParameter) value);
        }

        private void OnChange()
        {
        }

        private void RangeCheck(int index)
        {
            if ((index < 0) || (this.Count <= index))
            {
                throw System.Data.Common.ADP.ParametersMappingIndex(index, this);
            }
        }

        public void Remove(OracleParameter value)
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
            else if (this != ((OracleParameter) value).CompareExchangeParent(null, this))
            {
                throw System.Data.Common.ADP.CollectionRemoveInvalidObject(ItemType, this);
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
            List<OracleParameter> innerList = this.InnerList;
            OracleParameter parameter = innerList[index];
            innerList.RemoveAt(index);
            parameter.ResetParent();
        }

        private void Replace(int index, object newValue)
        {
            List<OracleParameter> innerList = this.InnerList;
            this.ValidateType(newValue);
            this.Validate(index, newValue);
            OracleParameter parameter = innerList[index];
            innerList[index] = (OracleParameter) newValue;
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
                throw System.Data.Common.ADP.ParametersSourceIndex(parameterName, this, ItemType);
            }
            this.Replace(index, value);
        }

        private void Validate(int index, object value)
        {
            if (value == null)
            {
                throw System.Data.Common.ADP.ParameterNull("value", this, ItemType);
            }
            object obj2 = ((OracleParameter) value).CompareExchangeParent(this, null);
            if (obj2 != null)
            {
                if (this != obj2)
                {
                    throw System.Data.Common.ADP.ParametersIsNotParent(ItemType, this);
                }
                if (index != this.IndexOf(value))
                {
                    throw System.Data.Common.ADP.ParametersIsParent(ItemType, this);
                }
            }
            if (((OracleParameter) value).ParameterName.Length == 0)
            {
                string str;
                index = 1;
                do
                {
                    str = "Parameter" + index.ToString(CultureInfo.CurrentCulture);
                    index++;
                }
                while (-1 != this.IndexOf(str));
                ((OracleParameter) value).ParameterName = str;
            }
        }

        private void ValidateType(object value)
        {
            if (value == null)
            {
                throw System.Data.Common.ADP.ParameterNull("value", this, ItemType);
            }
            if (!ItemType.IsInstanceOfType(value))
            {
                throw System.Data.Common.ADP.InvalidParameterType(this, ItemType, value);
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

        private List<OracleParameter> InnerList
        {
            get
            {
                List<OracleParameter> list = this._items;
                if (list == null)
                {
                    list = new List<OracleParameter>();
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

        public OracleParameter this[int index]
        {
            get
            {
                return (OracleParameter) this.GetParameter(index);
            }
            set
            {
                this.SetParameter(index, value);
            }
        }

        public OracleParameter this[string parameterName]
        {
            get
            {
                int index = this.IndexOf(parameterName);
                return (OracleParameter) this.GetParameter(index);
            }
            set
            {
                int index = this.IndexOf(parameterName);
                this.SetParameter(index, value);
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

