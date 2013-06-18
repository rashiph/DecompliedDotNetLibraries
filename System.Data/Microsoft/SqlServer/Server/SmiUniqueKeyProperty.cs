namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    internal class SmiUniqueKeyProperty : SmiMetaDataProperty
    {
        private IList<bool> _columns;

        internal SmiUniqueKeyProperty(IList<bool> columnIsKey)
        {
            this._columns = new List<bool>(columnIsKey).AsReadOnly();
        }

        [Conditional("DEBUG")]
        internal void CheckCount(int countToMatch)
        {
        }

        internal override string TraceString()
        {
            string str = "UniqueKey(";
            bool flag = false;
            for (int i = 0; i < this._columns.Count; i++)
            {
                if (flag)
                {
                    str = str + ",";
                }
                else
                {
                    flag = true;
                }
                if (this._columns[i])
                {
                    str = str + i.ToString(CultureInfo.InvariantCulture);
                }
            }
            return (str + ")");
        }

        internal bool this[int ordinal]
        {
            get
            {
                if (this._columns.Count <= ordinal)
                {
                    return false;
                }
                return this._columns[ordinal];
            }
        }
    }
}

