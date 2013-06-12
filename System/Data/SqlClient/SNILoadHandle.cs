namespace System.Data.SqlClient
{
    using System;
    using System.Data;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class SNILoadHandle : SafeHandle
    {
        private readonly EncryptionOptions _encryptionOption;
        private readonly uint _sniStatus;
        internal readonly SNINativeMethodWrapper.SqlAsyncCallbackDelegate ReadAsyncCallbackDispatcher;
        internal static readonly SNILoadHandle SingletonInstance = new SNILoadHandle();
        internal readonly SNINativeMethodWrapper.SqlAsyncCallbackDelegate WriteAsyncCallbackDispatcher;

        private SNILoadHandle() : base(IntPtr.Zero, true)
        {
            this.ReadAsyncCallbackDispatcher = new SNINativeMethodWrapper.SqlAsyncCallbackDelegate(SNILoadHandle.ReadDispatcher);
            this.WriteAsyncCallbackDispatcher = new SNINativeMethodWrapper.SqlAsyncCallbackDelegate(SNILoadHandle.WriteDispatcher);
            this._sniStatus = uint.MaxValue;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                this._sniStatus = SNINativeMethodWrapper.SNIInitialize();
                uint qInfo = 0;
                if (this._sniStatus == 0)
                {
                    SNINativeMethodWrapper.SNIQueryInfo(SNINativeMethodWrapper.QTypes.SNI_QUERY_CLIENT_ENCRYPT_POSSIBLE, ref qInfo);
                }
                this._encryptionOption = (qInfo == 0) ? EncryptionOptions.NOT_SUP : EncryptionOptions.OFF;
                base.handle = (IntPtr) 1;
            }
        }

        private static void ReadDispatcher(IntPtr key, IntPtr packet, uint error)
        {
            if (IntPtr.Zero != key)
            {
                GCHandle handle = (GCHandle) key;
                TdsParserStateObject target = (TdsParserStateObject) handle.Target;
                if (target != null)
                {
                    target.ReadAsyncCallback(IntPtr.Zero, packet, error);
                }
            }
        }

        protected override bool ReleaseHandle()
        {
            if (base.handle != IntPtr.Zero)
            {
                if (this._sniStatus == 0)
                {
                    LocalDBAPI.ReleaseDLLHandles();
                    SNINativeMethodWrapper.SNITerminate();
                }
                base.handle = IntPtr.Zero;
            }
            return true;
        }

        private static void WriteDispatcher(IntPtr key, IntPtr packet, uint error)
        {
            if (IntPtr.Zero != key)
            {
                GCHandle handle = (GCHandle) key;
                TdsParserStateObject target = (TdsParserStateObject) handle.Target;
                if (target != null)
                {
                    target.WriteAsyncCallback(IntPtr.Zero, packet, error);
                }
            }
        }

        public override bool IsInvalid
        {
            get
            {
                return (IntPtr.Zero == base.handle);
            }
        }

        public EncryptionOptions Options
        {
            get
            {
                return this._encryptionOption;
            }
        }

        public uint SNIStatus
        {
            get
            {
                return this._sniStatus;
            }
        }
    }
}

