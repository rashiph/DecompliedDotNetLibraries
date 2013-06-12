namespace System.Data.OleDb
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;

    public sealed class OleDbEnumerator
    {
        public DataTable GetElements()
        {
            OleDbConnection.ExecutePermission.Demand();
            DataTable table = new DataTable("MSDAENUM") {
                Locale = CultureInfo.InvariantCulture
            };
            OleDbDataAdapter.FillDataTable(GetRootEnumerator(), new DataTable[] { table });
            return table;
        }

        public static OleDbDataReader GetEnumerator(Type type)
        {
            OleDbConnection.ExecutePermission.Demand();
            return GetEnumeratorFromType(type);
        }

        internal static OleDbDataReader GetEnumeratorFromType(Type type)
        {
            return GetEnumeratorReader(Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.Instance, null, null, CultureInfo.InvariantCulture, null));
        }

        private static OleDbDataReader GetEnumeratorReader(object value)
        {
            System.Data.Common.NativeMethods.ISourcesRowset rowset = null;
            try
            {
                rowset = (System.Data.Common.NativeMethods.ISourcesRowset) value;
            }
            catch (InvalidCastException)
            {
                throw ODB.ISourcesRowsetNotSupported();
            }
            if (rowset == null)
            {
                throw ODB.ISourcesRowsetNotSupported();
            }
            value = null;
            int cPropertySets = 0;
            IntPtr ptrZero = ADP.PtrZero;
            Bid.Trace("<oledb.ISourcesRowset.GetSourcesRowset|API|OLEDB> IID_IRowset\n");
            OleDbHResult result = rowset.GetSourcesRowset(ADP.PtrZero, ODB.IID_IRowset, cPropertySets, ptrZero, out value);
            Bid.Trace("<oledb.ISourcesRowset.GetSourcesRowset|API|OLEDB|RET> %08X{HRESULT}\n", result);
            Exception exception = OleDbConnection.ProcessResults(result, null, null);
            if (exception != null)
            {
                throw exception;
            }
            OleDbDataReader reader = new OleDbDataReader(null, null, 0, CommandBehavior.Default);
            reader.InitializeIRowset(value, ChapterHandle.DB_NULL_HCHAPTER, ADP.RecordsUnaffected);
            reader.BuildMetaInfo();
            reader.HasRowsRead();
            return reader;
        }

        public static OleDbDataReader GetRootEnumerator()
        {
            OleDbDataReader enumeratorFromType;
            IntPtr ptr;
            OleDbConnection.ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<oledb.OleDbEnumerator.GetRootEnumerator|API>\n");
            try
            {
                enumeratorFromType = GetEnumeratorFromType(Type.GetTypeFromProgID("MSDAENUM", true));
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return enumeratorFromType;
        }
    }
}

