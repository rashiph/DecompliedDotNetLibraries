namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel.Activation.Interop;
    using System.Threading;

    [SecurityCritical(SecurityCriticalScope.Everything)]
    internal class MetabaseReader : IDisposable
    {
        private static IMSAdminBase adminBase;
        [SecurityCritical]
        private SafeHandle bufferHandle;
        private uint currentBufferSize;
        private bool disposed;
        private const uint E_DATA_NOT_FOUND = 0x800cc801;
        private const uint E_INSUFFICIENT_BUFFER = 0x8007007a;
        private const uint E_PATH_NOT_FOUND = 0x80070003;
        internal const string LMPath = "/LM";
        private int mdHandle;
        [SecurityCritical]
        private METADATA_RECORD record;
        private static object syncRoot = new object();

        [SecuritySafeCritical]
        public MetabaseReader()
        {
            uint num;
            this.currentBufferSize = 0x400;
            lock (syncRoot)
            {
                if (adminBase == null)
                {
                    adminBase = (IMSAdminBase) new MSAdminBase();
                }
            }
            uint num2 = adminBase.OpenKey(0, "/LM", 1, 0x7530, out num);
            this.mdHandle = (int) num;
            if (num2 != 0)
            {
                throw FxTrace.Exception.AsError(new COMException(System.ServiceModel.Activation.SR.Hosting_MetabaseAccessError, (int) num2));
            }
            this.bufferHandle = SafeHGlobalHandleCritical.AllocHGlobal(this.currentBufferSize);
        }

        [SecurityCritical]
        private object ConvertData()
        {
            switch (this.record.dwMDDataType)
            {
                case 1:
                    return (uint) Marshal.ReadInt32(this.record.pbMDData);

                case 2:
                case 4:
                    return Marshal.PtrToStringUni(this.record.pbMDData);

                case 5:
                    return this.RecordToStringArray();
            }
            string introduced1 = this.record.dwMDDataType.ToString(NumberFormatInfo.CurrentInfo);
            throw FxTrace.Exception.AsError(new NotSupportedException(System.ServiceModel.Activation.SR.Hosting_MetabaseDataTypeUnsupported(introduced1, this.record.dwMDIdentifier.ToString(NumberFormatInfo.CurrentInfo))));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecuritySafeCritical]
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.record.pbMDData = IntPtr.Zero;
                    this.currentBufferSize = 0;
                    if (this.bufferHandle != null)
                    {
                        this.bufferHandle.Close();
                    }
                }
                int num = Interlocked.Exchange(ref this.mdHandle, 0);
                if (num != 0)
                {
                    adminBase.CloseKey((uint) num);
                }
                this.disposed = true;
            }
        }

        [SecuritySafeCritical]
        private void EnsureRecordBuffer(uint bytes)
        {
            if (bytes > this.currentBufferSize)
            {
                this.bufferHandle.Close();
                this.currentBufferSize = bytes;
                this.bufferHandle = SafeHGlobalHandleCritical.AllocHGlobal(this.currentBufferSize);
                this.record.pbMDData = this.bufferHandle.DangerousGetHandle();
                this.record.dwMDDataLen = this.currentBufferSize;
            }
        }

        ~MetabaseReader()
        {
            this.Dispose(false);
        }

        [SecurityCritical]
        public object GetData(string path, MetabasePropertyType propertyType)
        {
            return this.GetData(path, (uint) propertyType);
        }

        [SecurityCritical]
        private object GetData(string path, uint type)
        {
            uint currentBufferSize = this.currentBufferSize;
            this.record.dwMDAttributes = 1;
            this.record.dwMDUserType = 1;
            this.record.dwMDDataType = 0;
            this.record.dwMDIdentifier = type;
            this.record.pbMDData = this.bufferHandle.DangerousGetHandle();
            this.record.dwMDDataLen = this.currentBufferSize;
            uint num2 = adminBase.GetData((uint) this.mdHandle, path, ref this.record, ref currentBufferSize);
            switch (num2)
            {
                case 0x8007007a:
                    this.EnsureRecordBuffer(currentBufferSize);
                    num2 = adminBase.GetData((uint) this.mdHandle, path, ref this.record, ref currentBufferSize);
                    break;

                case 0x80070003:
                case 0x800cc801:
                    return null;
            }
            if (num2 != 0)
            {
                throw FxTrace.Exception.AsError(new COMException(System.ServiceModel.Activation.SR.Hosting_MetabaseAccessError, (int) num2));
            }
            return this.ConvertData();
        }

        [SecurityCritical]
        private string[] RecordToStringArray()
        {
            List<string> list = new List<string>();
            if (this.record.dwMDDataType == 5)
            {
                if ((this.record.dwMDDataLen & 1) != 0)
                {
                    throw FxTrace.Exception.AsError(new DataMisalignedException(System.ServiceModel.Activation.SR.Hosting_MetabaseDataStringsTerminate(this.record.dwMDIdentifier.ToString(NumberFormatInfo.CurrentInfo))));
                }
                int num = 0;
                int ofs = 0;
                while (this.record.dwMDDataLen > 0)
                {
                    while ((ofs < this.record.dwMDDataLen) && (Marshal.ReadInt16(this.record.pbMDData, ofs) != 0))
                    {
                        ofs += 2;
                    }
                    if ((ofs == this.record.dwMDDataLen) && (Marshal.ReadInt16(this.record.pbMDData, ofs - 2) != 0))
                    {
                        throw FxTrace.Exception.AsError(new DataMisalignedException(System.ServiceModel.Activation.SR.Hosting_MetabaseDataStringsTerminate(this.record.dwMDIdentifier.ToString(NumberFormatInfo.CurrentInfo))));
                    }
                    if (ofs == num)
                    {
                        break;
                    }
                    list.Add(Marshal.PtrToStringUni(new IntPtr(this.record.pbMDData.ToInt64() + num), (ofs - num) / 2));
                    num = ofs += 2;
                }
            }
            return list.ToArray();
        }
    }
}

