namespace System.Data.OleDb
{
    using System;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Serializable]
    public sealed class OleDbError
    {
        private readonly string message;
        private readonly int nativeError;
        private readonly string source;
        private readonly string sqlState;

        internal OleDbError(UnsafeNativeMethods.IErrorRecords errorRecords, int index)
        {
            OleDbHResult description;
            UnsafeNativeMethods.ISQLErrorInfo info2;
            int lCID = CultureInfo.CurrentCulture.LCID;
            Bid.Trace("<oledb.IErrorRecords.GetErrorInfo|API|OS>\n");
            UnsafeNativeMethods.IErrorInfo errorInfo = errorRecords.GetErrorInfo(index, lCID);
            if (errorInfo != null)
            {
                Bid.Trace("<oledb.IErrorInfo.GetDescription|API|OS>\n");
                description = errorInfo.GetDescription(out this.message);
                Bid.Trace("<oledb.IErrorInfo.GetDescription|API|OS|RET> Message='%ls'\n", this.message);
                if (OleDbHResult.DB_E_NOLOCALE == description)
                {
                    Bid.Trace("<oledb.ReleaseComObject|API|OS> ErrorInfo\n");
                    Marshal.ReleaseComObject(errorInfo);
                    Bid.Trace("<oledb.Kernel32.GetUserDefaultLCID|API|OS>\n");
                    lCID = SafeNativeMethods.GetUserDefaultLCID();
                    Bid.Trace("<oledb.IErrorRecords.GetErrorInfo|API|OS> LCID=%d\n", lCID);
                    errorInfo = errorRecords.GetErrorInfo(index, lCID);
                    if (errorInfo != null)
                    {
                        Bid.Trace("<oledb.IErrorInfo.GetDescription|API|OS>\n");
                        description = errorInfo.GetDescription(out this.message);
                        Bid.Trace("<oledb.IErrorInfo.GetDescription|API|OS|RET> Message='%ls'\n", this.message);
                    }
                }
                if ((description < OleDbHResult.S_OK) && ADP.IsEmpty(this.message))
                {
                    this.message = ODB.FailedGetDescription(description);
                }
                if (errorInfo != null)
                {
                    Bid.Trace("<oledb.IErrorInfo.GetSource|API|OS>\n");
                    description = errorInfo.GetSource(out this.source);
                    Bid.Trace("<oledb.IErrorInfo.GetSource|API|OS|RET> Source='%ls'\n", this.source);
                    if (OleDbHResult.DB_E_NOLOCALE == description)
                    {
                        Marshal.ReleaseComObject(errorInfo);
                        Bid.Trace("<oledb.Kernel32.GetUserDefaultLCID|API|OS>\n");
                        lCID = SafeNativeMethods.GetUserDefaultLCID();
                        Bid.Trace("<oledb.IErrorRecords.GetErrorInfo|API|OS> LCID=%d\n", lCID);
                        errorInfo = errorRecords.GetErrorInfo(index, lCID);
                        if (errorInfo != null)
                        {
                            Bid.Trace("<oledb.IErrorInfo.GetSource|API|OS>\n");
                            description = errorInfo.GetSource(out this.source);
                            Bid.Trace("<oledb.IErrorInfo.GetSource|API|OS|RET> Source='%ls'\n", this.source);
                        }
                    }
                    if ((description < OleDbHResult.S_OK) && ADP.IsEmpty(this.source))
                    {
                        this.source = ODB.FailedGetSource(description);
                    }
                    Bid.Trace("<oledb.Marshal.ReleaseComObject|API|OS> ErrorInfo\n");
                    Marshal.ReleaseComObject(errorInfo);
                }
            }
            Bid.Trace("<oledb.IErrorRecords.GetCustomErrorObject|API|OS> IID_ISQLErrorInfo\n");
            description = errorRecords.GetCustomErrorObject(index, ref ODB.IID_ISQLErrorInfo, out info2);
            if (info2 != null)
            {
                Bid.Trace("<oledb.ISQLErrorInfo.GetSQLInfo|API|OS>\n");
                this.nativeError = info2.GetSQLInfo(out this.sqlState);
                Bid.Trace("<oledb.ReleaseComObject|API|OS> SQLErrorInfo\n");
                Marshal.ReleaseComObject(info2);
            }
        }

        public override string ToString()
        {
            return this.Message;
        }

        public string Message
        {
            get
            {
                string message = this.message;
                if (message == null)
                {
                    return ADP.StrEmpty;
                }
                return message;
            }
        }

        public int NativeError
        {
            get
            {
                return this.nativeError;
            }
        }

        public string Source
        {
            get
            {
                string source = this.source;
                if (source == null)
                {
                    return ADP.StrEmpty;
                }
                return source;
            }
        }

        public string SQLState
        {
            get
            {
                string sqlState = this.sqlState;
                if (sqlState == null)
                {
                    return ADP.StrEmpty;
                }
                return sqlState;
            }
        }
    }
}

