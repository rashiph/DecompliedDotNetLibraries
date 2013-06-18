namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;
    using System.EnterpriseServices.Thunk;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class CrmMonitorLogRecords
    {
        private unsafe ICrmMonitorLogRecords* _pMon;

        public unsafe CrmMonitorLogRecords(IntPtr mon)
        {
            ICrmMonitorLogRecords* recordsPtr;
            IUnknown* unknownPtr = (IUnknown*) mon.ToInt32();
            if (unknownPtr == null)
            {
                throw new NullReferenceException();
            }
            int modopt(IsLong) errorCode = **(*((int*) unknownPtr))(unknownPtr, &IID_ICrmMonitorLogRecords, &recordsPtr);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            this._pMon = recordsPtr;
        }

        public unsafe void Dispose()
        {
            ICrmMonitorLogRecords* recordsPtr = this._pMon;
            if (recordsPtr != null)
            {
                **(((int*) recordsPtr))[8](recordsPtr);
                this._pMon = null;
            }
        }

        public unsafe int GetCount()
        {
            int modopt(IsLong) num2;
            ICrmMonitorLogRecords* recordsPtr = this._pMon;
            int modopt(IsLong) errorCode = **(((int*) recordsPtr))[12](recordsPtr, &num2);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            return num2;
        }

        public unsafe _LogRecord GetLogRecord(int index)
        {
            tagCrmLogRecordRead read;
            ICrmMonitorLogRecords* recordsPtr = this._pMon;
            int modopt(IsLong) errorCode = **(((int*) recordsPtr))[0x18](recordsPtr, index, &read);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            _LogRecord record = new _LogRecord {
                dwCrmFlags = *((int*) &read),
                dwSequenceNumber = *((int*) (&read + 4))
            };
            record.blobUserData.cbSize = *((int*) (&read + 8));
            IntPtr modopt(IsConst) ptr = new IntPtr(*((void**) (&read + 12)));
            record.blobUserData.pBlobData = ptr;
            return record;
        }

        public unsafe int GetTransactionState()
        {
            int num2;
            ICrmMonitorLogRecords* recordsPtr = this._pMon;
            int modopt(IsLong) errorCode = **(((int*) recordsPtr))[0x10](recordsPtr, &num2);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            return num2;
        }
    }
}

