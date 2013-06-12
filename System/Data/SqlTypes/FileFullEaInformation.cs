namespace System.Data.SqlTypes
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class FileFullEaInformation : SafeHandleZeroOrMinusOneIsInvalid
    {
        private string EA_NAME_STRING;
        private int m_cbBuffer;

        public FileFullEaInformation(byte[] transactionContext) : base(true)
        {
            this.EA_NAME_STRING = "Filestream_Transaction_Tag";
            this.m_cbBuffer = 0;
            this.InitializeEaBuffer(transactionContext);
        }

        private void InitializeEaBuffer(byte[] transactionContext)
        {
            System.Data.SqlTypes.UnsafeNativeMethods.FILE_FULL_EA_INFORMATION file_full_ea_information;
            if (transactionContext.Length >= 0xffff)
            {
                throw ADP.ArgumentOutOfRange("transactionContext");
            }
            file_full_ea_information.nextEntryOffset = 0;
            file_full_ea_information.flags = 0;
            file_full_ea_information.EaName = 0;
            file_full_ea_information.EaNameLength = (byte) this.EA_NAME_STRING.Length;
            file_full_ea_information.EaValueLength = (ushort) transactionContext.Length;
            int introduced10 = Marshal.SizeOf(file_full_ea_information);
            this.m_cbBuffer = (introduced10 + file_full_ea_information.EaNameLength) + file_full_ea_information.EaValueLength;
            IntPtr zero = IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                zero = Marshal.AllocHGlobal(this.m_cbBuffer);
                if (zero != IntPtr.Zero)
                {
                    base.SetHandle(zero);
                }
            }
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                IntPtr handle = base.DangerousGetHandle();
                Marshal.StructureToPtr(file_full_ea_information, handle, false);
                byte[] bytes = new ASCIIEncoding().GetBytes(this.EA_NAME_STRING);
                int ofs = Marshal.OffsetOf(typeof(System.Data.SqlTypes.UnsafeNativeMethods.FILE_FULL_EA_INFORMATION), "EaName").ToInt32();
                int index = 0;
                while ((ofs < this.m_cbBuffer) && (index < file_full_ea_information.EaNameLength))
                {
                    Marshal.WriteByte(handle, ofs, bytes[index]);
                    index++;
                    ofs++;
                }
                Marshal.WriteByte(handle, ofs, 0);
                ofs++;
                int num2 = 0;
                while ((ofs < this.m_cbBuffer) && (num2 < file_full_ea_information.EaValueLength))
                {
                    Marshal.WriteByte(handle, ofs, transactionContext[num2]);
                    num2++;
                    ofs++;
                }
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
        }

        protected override bool ReleaseHandle()
        {
            this.m_cbBuffer = 0;
            if (base.handle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(base.handle);
                base.handle = IntPtr.Zero;
            }
            return true;
        }

        public int Length
        {
            get
            {
                return this.m_cbBuffer;
            }
        }
    }
}

