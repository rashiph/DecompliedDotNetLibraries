namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class SmiOrderProperty : SmiMetaDataProperty
    {
        private IList<SmiColumnOrder> _columns;

        internal SmiOrderProperty(IList<SmiColumnOrder> columnOrders)
        {
            this._columns = new List<SmiColumnOrder>(columnOrders).AsReadOnly();
        }

        [Conditional("DEBUG")]
        internal void CheckCount(int countToMatch)
        {
        }

        internal override string TraceString()
        {
            string str = "SortOrder(";
            bool flag = false;
            foreach (SmiColumnOrder order in this._columns)
            {
                if (flag)
                {
                    str = str + ",";
                }
                else
                {
                    flag = true;
                }
                if (SortOrder.Unspecified != order.Order)
                {
                    str = str + order.TraceString();
                }
            }
            return (str + ")");
        }

        internal SmiColumnOrder this[int ordinal]
        {
            get
            {
                if (this._columns.Count <= ordinal)
                {
                    return new SmiColumnOrder { Order = SortOrder.Unspecified, SortOrdinal = -1 };
                }
                return this._columns[ordinal];
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SmiColumnOrder
        {
            internal int SortOrdinal;
            internal SortOrder Order;
            internal string TraceString()
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} {1}", new object[] { this.SortOrdinal, this.Order });
            }
        }
    }
}

