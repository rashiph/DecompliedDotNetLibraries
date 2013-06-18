namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    internal static class FilteredDataSetHelper
    {
        public static DataView CreateFilteredDataView(DataTable table, string sortExpression, string filterExpression, IDictionary filterParameters)
        {
            DataView view = new DataView(table);
            if (!string.IsNullOrEmpty(sortExpression))
            {
                view.Sort = sortExpression;
            }
            if (!string.IsNullOrEmpty(filterExpression))
            {
                bool flag = false;
                object[] args = new object[filterParameters.Count];
                int index = 0;
                foreach (DictionaryEntry entry in filterParameters)
                {
                    if (entry.Value == null)
                    {
                        flag = true;
                        break;
                    }
                    args[index] = entry.Value;
                    index++;
                }
                filterExpression = string.Format(CultureInfo.InvariantCulture, filterExpression, args);
                if (!flag)
                {
                    view.RowFilter = filterExpression;
                }
            }
            return view;
        }

        public static DataTable GetDataTable(Control owner, object dataObject)
        {
            DataSet set = dataObject as DataSet;
            if (set == null)
            {
                return (dataObject as DataTable);
            }
            if (set.Tables.Count == 0)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("FilteredDataSetHelper_DataSetHasNoTables", new object[] { owner.ID }));
            }
            return set.Tables[0];
        }
    }
}

