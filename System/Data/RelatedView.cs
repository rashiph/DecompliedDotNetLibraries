namespace System.Data
{
    using System;

    internal sealed class RelatedView : DataView, IFilter
    {
        private readonly DataKey key;
        private object[] values;

        public RelatedView(DataColumn[] columns, object[] values) : base(columns[0].Table, false)
        {
            if (values == null)
            {
                throw ExceptionBuilder.ArgumentNull("values");
            }
            this.key = new DataKey(columns, true);
            this.values = values;
            base.ResetRowViewCache();
        }

        public override DataRowView AddNew()
        {
            DataRowView view = base.AddNew();
            view.Row.SetKeyValues(this.key, this.values);
            return view;
        }

        private bool CompareArray(object[] value1, object[] value2)
        {
            if (value1.Length != value2.Length)
            {
                return false;
            }
            for (int i = 0; i < value1.Length; i++)
            {
                if (value1[i] != value2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(DataView dv)
        {
            if (!(dv is RelatedView))
            {
                return false;
            }
            if (!base.Equals(dv))
            {
                return false;
            }
            if (!this.CompareArray(this.key.ColumnsReference, ((RelatedView) dv).key.ColumnsReference))
            {
                return this.CompareArray(this.values, ((RelatedView) dv).values);
            }
            return true;
        }

        internal override IFilter GetFilter()
        {
            return this;
        }

        public bool Invoke(DataRow row, DataRowVersion version)
        {
            object[] keyValues = row.GetKeyValues(this.key, version);
            bool flag = true;
            if (keyValues.Length != this.values.Length)
            {
                flag = false;
            }
            else
            {
                for (int i = 0; i < keyValues.Length; i++)
                {
                    if (!keyValues[i].Equals(this.values[i]))
                    {
                        flag = false;
                        break;
                    }
                }
            }
            IFilter filter = base.GetFilter();
            if (filter != null)
            {
                flag &= filter.Invoke(row, version);
            }
            return flag;
        }

        internal override void SetIndex(string newSort, DataViewRowState newRowStates, IFilter newRowFilter)
        {
            base.SetIndex2(newSort, newRowStates, newRowFilter, false);
            base.Reset();
        }
    }
}

