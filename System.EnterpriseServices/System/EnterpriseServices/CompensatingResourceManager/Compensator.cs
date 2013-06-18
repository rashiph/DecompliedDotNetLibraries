namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;
    using System.EnterpriseServices;
    using System.Globalization;
    using System.Runtime.InteropServices;

    public class Compensator : ServicedComponent, _ICompensator, _IFormatLogRecords
    {
        private System.EnterpriseServices.CompensatingResourceManager.Clerk _clerk = null;

        public virtual bool AbortRecord(LogRecord rec)
        {
            return false;
        }

        public virtual void BeginAbort(bool fRecovery)
        {
        }

        public virtual void BeginCommit(bool fRecovery)
        {
        }

        public virtual void BeginPrepare()
        {
        }

        public virtual bool CommitRecord(LogRecord rec)
        {
            return false;
        }

        public virtual void EndAbort()
        {
        }

        public virtual void EndCommit()
        {
        }

        public virtual bool EndPrepare()
        {
            return true;
        }

        public virtual bool PrepareRecord(LogRecord rec)
        {
            return false;
        }

        bool _ICompensator._AbortRecord(_LogRecord record)
        {
            LogRecord rec = new LogRecord(record);
            return this.AbortRecord(rec);
        }

        void _ICompensator._BeginAbort(bool fRecovery)
        {
            this.BeginAbort(fRecovery);
        }

        void _ICompensator._BeginCommit(bool fRecovery)
        {
            this.BeginCommit(fRecovery);
        }

        void _ICompensator._BeginPrepare()
        {
            this.BeginPrepare();
        }

        bool _ICompensator._CommitRecord(_LogRecord record)
        {
            LogRecord rec = new LogRecord(record);
            return this.CommitRecord(rec);
        }

        void _ICompensator._EndAbort()
        {
            this.EndAbort();
        }

        void _ICompensator._EndCommit()
        {
            this.EndCommit();
        }

        bool _ICompensator._EndPrepare()
        {
            return this.EndPrepare();
        }

        bool _ICompensator._PrepareRecord(_LogRecord record)
        {
            LogRecord rec = new LogRecord(record);
            return this.PrepareRecord(rec);
        }

        void _ICompensator._SetLogControl(IntPtr logControl)
        {
            this._clerk = new System.EnterpriseServices.CompensatingResourceManager.Clerk(new CrmLogControl(logControl));
        }

        object _IFormatLogRecords.GetColumn(_LogRecord r)
        {
            LogRecord record = new LogRecord(r);
            if (this is IFormatLogRecords)
            {
                return ((IFormatLogRecords) this).Format(record);
            }
            return new string[] { record.Flags.ToString(), record.Sequence.ToString(CultureInfo.CurrentUICulture), record.Record.ToString() };
        }

        int _IFormatLogRecords.GetColumnCount()
        {
            if (this is IFormatLogRecords)
            {
                return ((IFormatLogRecords) this).ColumnCount;
            }
            return 3;
        }

        object _IFormatLogRecords.GetColumnHeaders()
        {
            if (this is IFormatLogRecords)
            {
                return ((IFormatLogRecords) this).ColumnHeaders;
            }
            return new string[] { Resource.FormatString("CRM_HeaderFlags"), Resource.FormatString("CRM_HeaderRecord"), Resource.FormatString("CRM_HeaderString") };
        }

        object _IFormatLogRecords.GetColumnVariants(object logRecord)
        {
            throw new NotSupportedException();
        }

        public System.EnterpriseServices.CompensatingResourceManager.Clerk Clerk
        {
            get
            {
                return this._clerk;
            }
        }
    }
}

