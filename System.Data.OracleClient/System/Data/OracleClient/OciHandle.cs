namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal abstract class OciHandle : SafeHandle
    {
        private OCI.HTYPE _handleType;
        private bool _isUnicode;
        private OciHandle _parentHandle;
        private int _refCount;

        protected OciHandle() : base(IntPtr.Zero, true)
        {
        }

        protected OciHandle(OCI.HTYPE handleType) : base(IntPtr.Zero, false)
        {
            this._handleType = handleType;
        }

        protected OciHandle(OciHandle parentHandle, OCI.HTYPE handleType) : this(parentHandle, handleType, OCI.MODE.OCI_DEFAULT, HANDLEFLAG.DEFAULT)
        {
        }

        protected OciHandle(OciHandle parentHandle, OCI.HTYPE handleType, OCI.MODE ocimode, HANDLEFLAG handleflags) : this()
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                int num;
                this._handleType = handleType;
                this._parentHandle = parentHandle;
                this._refCount = 1;
                switch (handleType)
                {
                    case OCI.HTYPE.OCI_DTYPE_TIMESTAMP:
                    case OCI.HTYPE.OCI_DTYPE_TIMESTAMP_TZ:
                    case OCI.HTYPE.OCI_DTYPE_TIMESTAMP_LTZ:
                    case OCI.HTYPE.OCI_DTYPE_INTERVAL_DS:
                    case OCI.HTYPE.OCI_DTYPE_FIRST:
                    case OCI.HTYPE.OCI_DTYPE_ROWID:
                    case OCI.HTYPE.OCI_DTYPE_FILE:
                        num = TracedNativeMethods.OCIDescriptorAlloc(parentHandle.EnvironmentHandle, out this.handle, handleType);
                        if ((num != 0) || (IntPtr.Zero == base.handle))
                        {
                            throw System.Data.Common.ADP.OperationFailed("OCIDescriptorAlloc", num);
                        }
                        break;

                    case OCI.HTYPE.OCI_HTYPE_ENV:
                        if ((handleflags & HANDLEFLAG.NLS) != HANDLEFLAG.NLS)
                        {
                            num = TracedNativeMethods.OCIEnvCreate(out this.handle, ocimode);
                            if ((num != 0) || (IntPtr.Zero == base.handle))
                            {
                                throw System.Data.Common.ADP.OperationFailed("OCIEnvCreate", num);
                            }
                        }
                        else
                        {
                            num = TracedNativeMethods.OCIEnvNlsCreate(out this.handle, ocimode, 0, 0);
                            if ((num != 0) || (IntPtr.Zero == base.handle))
                            {
                                throw System.Data.Common.ADP.OperationFailed("OCIEnvNlsCreate", num);
                            }
                        }
                        break;

                    case OCI.HTYPE.OCI_HTYPE_ERROR:
                    case OCI.HTYPE.OCI_HTYPE_SVCCTX:
                    case OCI.HTYPE.OCI_HTYPE_STMT:
                    case OCI.HTYPE.OCI_HTYPE_SERVER:
                    case OCI.HTYPE.OCI_HTYPE_SESSION:
                        num = TracedNativeMethods.OCIHandleAlloc(parentHandle.EnvironmentHandle, out this.handle, handleType);
                        if ((num != 0) || (IntPtr.Zero == base.handle))
                        {
                            throw System.Data.Common.ADP.OperationFailed("OCIHandleAlloc", num);
                        }
                        break;
                }
                if (parentHandle != null)
                {
                    parentHandle.AddRef();
                    this._isUnicode = parentHandle.IsUnicode;
                }
                else
                {
                    this._isUnicode = (handleflags & HANDLEFLAG.UNICODE) == HANDLEFLAG.UNICODE;
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal int AddRef()
        {
            return Interlocked.Increment(ref this._refCount);
        }

        internal void GetAttribute(OCI.ATTR attribute, out byte value, OciErrorHandle errorHandle)
        {
            uint sizep = 0;
            int rc = TracedNativeMethods.OCIAttrGet(this, out value, out sizep, attribute, errorHandle);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        internal void GetAttribute(OCI.ATTR attribute, out short value, OciErrorHandle errorHandle)
        {
            uint sizep = 0;
            int rc = TracedNativeMethods.OCIAttrGet(this, out value, out sizep, attribute, errorHandle);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        internal void GetAttribute(OCI.ATTR attribute, out int value, OciErrorHandle errorHandle)
        {
            uint sizep = 0;
            int rc = TracedNativeMethods.OCIAttrGet(this, out value, out sizep, attribute, errorHandle);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        internal void GetAttribute(OCI.ATTR attribute, out string value, OciErrorHandle errorHandle, OracleConnection connection)
        {
            IntPtr zero = IntPtr.Zero;
            uint sizep = 0;
            int rc = TracedNativeMethods.OCIAttrGet(this, ref zero, ref sizep, attribute, errorHandle);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
            byte[] destination = new byte[sizep];
            Marshal.Copy(zero, destination, 0, (int) sizep);
            value = connection.GetString(destination);
        }

        internal static string GetAttributeName(OciHandle handle, OCI.ATTR atype)
        {
            if (OCI.HTYPE.OCI_DTYPE_PARAM == handle.HandleType)
            {
                return ((OCI.PATTR) atype).ToString();
            }
            return atype.ToString();
        }

        internal byte[] GetBytes(string value)
        {
            byte[] buffer;
            uint length = (uint) value.Length;
            if (this.IsUnicode)
            {
                buffer = new byte[length * System.Data.Common.ADP.CharSize];
                this.GetBytes(value.ToCharArray(), 0, length, buffer, 0);
                return buffer;
            }
            byte[] bytes = new byte[length * 4];
            uint num2 = this.GetBytes(value.ToCharArray(), 0, length, bytes, 0);
            buffer = new byte[num2];
            Buffer.BlockCopy(bytes, 0, buffer, 0, (int) num2);
            return buffer;
        }

        internal uint GetBytes(char[] chars, int charIndex, uint charCount, byte[] bytes, int byteIndex)
        {
            uint num;
            int num2;
            if (this.IsUnicode)
            {
                num = (uint) (charCount * System.Data.Common.ADP.CharSize);
                Buffer.BlockCopy(chars, charIndex * System.Data.Common.ADP.CharSize, bytes, byteIndex, (int) num);
                return num;
            }
            OciHandle environmentHandle = this.EnvironmentHandle;
            GCHandle handle2 = new GCHandle();
            GCHandle handle = new GCHandle();
            try
            {
                IntPtr zero;
                handle2 = GCHandle.Alloc(chars, GCHandleType.Pinned);
                IntPtr src = new IntPtr(((long) handle2.AddrOfPinnedObject()) + charIndex);
                if (bytes == null)
                {
                    zero = IntPtr.Zero;
                    num = 0;
                }
                else
                {
                    handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                    zero = new IntPtr(((long) handle.AddrOfPinnedObject()) + byteIndex);
                    num = (uint) (bytes.Length - byteIndex);
                }
                num2 = System.Data.Common.UnsafeNativeMethods.OCIUnicodeToCharSet(environmentHandle, zero, num, src, charCount, out num);
            }
            finally
            {
                handle2.Free();
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
            if (num2 != 0)
            {
                throw System.Data.Common.ADP.OperationFailed("OCIUnicodeToCharSet", num2);
            }
            return num;
        }

        internal uint GetChars(byte[] bytes, int byteIndex, uint byteCount, char[] chars, int charIndex)
        {
            uint num;
            int num2;
            if (this.IsUnicode)
            {
                num = (uint) (((ulong) byteCount) / ((long) System.Data.Common.ADP.CharSize));
                Buffer.BlockCopy(bytes, byteIndex, chars, charIndex * System.Data.Common.ADP.CharSize, (int) byteCount);
                return num;
            }
            OciHandle environmentHandle = this.EnvironmentHandle;
            GCHandle handle2 = new GCHandle();
            GCHandle handle = new GCHandle();
            try
            {
                IntPtr zero;
                handle2 = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                IntPtr src = new IntPtr(((long) handle2.AddrOfPinnedObject()) + byteIndex);
                if (chars == null)
                {
                    zero = IntPtr.Zero;
                    num = 0;
                }
                else
                {
                    handle = GCHandle.Alloc(chars, GCHandleType.Pinned);
                    zero = new IntPtr(((long) handle.AddrOfPinnedObject()) + charIndex);
                    num = (uint) (chars.Length - charIndex);
                }
                num2 = System.Data.Common.UnsafeNativeMethods.OCICharSetToUnicode(environmentHandle, zero, num, src, byteCount, out num);
            }
            finally
            {
                handle2.Free();
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
            if (num2 != 0)
            {
                throw System.Data.Common.ADP.OperationFailed("OCICharSetToUnicode", num2);
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static IntPtr HandleValueToTrace(OciHandle handle)
        {
            return handle.DangerousGetHandle();
        }

        internal string PtrToString(NativeBuffer buf)
        {
            if (this.IsUnicode)
            {
                return buf.PtrToStringUni(0);
            }
            return buf.PtrToStringAnsi(0);
        }

        internal string PtrToString(IntPtr buf, int len)
        {
            if (this.IsUnicode)
            {
                return Marshal.PtrToStringUni(buf, len);
            }
            return Marshal.PtrToStringAnsi(buf, len);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal int Release()
        {
            int num2;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                int num;
                num2 = Interlocked.Decrement(ref this._refCount);
                if (num2 != 0)
                {
                    goto Label_0168;
                }
                IntPtr hndlp = Interlocked.CompareExchange(ref this.handle, IntPtr.Zero, base.handle);
                if (!(IntPtr.Zero != hndlp))
                {
                    goto Label_0168;
                }
                OCI.HTYPE handleType = this.HandleType;
                OciHandle parentHandle = this.ParentHandle;
                switch (handleType)
                {
                    case OCI.HTYPE.OCI_HTYPE_ENV:
                        num = TracedNativeMethods.OCIHandleFree(hndlp, handleType);
                        if (num != 0)
                        {
                            throw System.Data.Common.ADP.OperationFailed("OCIHandleFree", num);
                        }
                        goto Label_015E;

                    case OCI.HTYPE.OCI_HTYPE_ERROR:
                    case OCI.HTYPE.OCI_HTYPE_STMT:
                    case OCI.HTYPE.OCI_HTYPE_SESSION:
                        break;

                    case OCI.HTYPE.OCI_HTYPE_SVCCTX:
                    {
                        OciHandle handle2 = parentHandle;
                        if (handle2 != null)
                        {
                            OciHandle handle4 = handle2.ParentHandle;
                            if (handle4 != null)
                            {
                                OciHandle handle3 = handle4.ParentHandle;
                                if (handle3 != null)
                                {
                                    num = TracedNativeMethods.OCISessionEnd(hndlp, handle3.DangerousGetHandle(), handle2.DangerousGetHandle(), OCI.MODE.OCI_DEFAULT);
                                }
                            }
                        }
                        break;
                    }
                    case OCI.HTYPE.OCI_HTYPE_SERVER:
                        TracedNativeMethods.OCIServerDetach(hndlp, parentHandle.DangerousGetHandle(), OCI.MODE.OCI_DEFAULT);
                        break;

                    case OCI.HTYPE.OCI_DTYPE_FIRST:
                    case OCI.HTYPE.OCI_DTYPE_ROWID:
                    case OCI.HTYPE.OCI_DTYPE_FILE:
                    case OCI.HTYPE.OCI_DTYPE_INTERVAL_DS:
                    case OCI.HTYPE.OCI_DTYPE_TIMESTAMP:
                    case OCI.HTYPE.OCI_DTYPE_TIMESTAMP_TZ:
                    case OCI.HTYPE.OCI_DTYPE_TIMESTAMP_LTZ:
                        num = TracedNativeMethods.OCIDescriptorFree(hndlp, handleType);
                        if (num != 0)
                        {
                            throw System.Data.Common.ADP.OperationFailed("OCIDescriptorFree", num);
                        }
                        goto Label_015E;

                    default:
                        goto Label_015E;
                }
                num = TracedNativeMethods.OCIHandleFree(hndlp, handleType);
                if (num != 0)
                {
                    throw System.Data.Common.ADP.OperationFailed("OCIHandleFree", num);
                }
            Label_015E:
                if (parentHandle != null)
                {
                    parentHandle.Release();
                }
            Label_0168:;
            }
            return num2;
        }

        protected override bool ReleaseHandle()
        {
            this.Release();
            return true;
        }

        internal static void SafeDispose(ref OciBindHandle handle)
        {
            if (handle != null)
            {
                handle.Dispose();
            }
            handle = null;
        }

        internal static void SafeDispose(ref OciDateTimeDescriptor handle)
        {
            if (handle != null)
            {
                handle.Dispose();
            }
            handle = null;
        }

        internal static void SafeDispose(ref OciDefineHandle handle)
        {
            if (handle != null)
            {
                handle.Dispose();
            }
            handle = null;
        }

        internal static void SafeDispose(ref OciEnvironmentHandle handle)
        {
            if (handle != null)
            {
                handle.Dispose();
            }
            handle = null;
        }

        internal static void SafeDispose(ref OciErrorHandle handle)
        {
            if (handle != null)
            {
                handle.Dispose();
            }
            handle = null;
        }

        internal static void SafeDispose(ref OciHandle handle)
        {
            if (handle != null)
            {
                handle.Dispose();
            }
            handle = null;
        }

        internal static void SafeDispose(ref OciParameterDescriptor handle)
        {
            if (handle != null)
            {
                handle.Dispose();
            }
            handle = null;
        }

        internal static void SafeDispose(ref OciRowidDescriptor handle)
        {
            if (handle != null)
            {
                handle.Dispose();
            }
            handle = null;
        }

        internal static void SafeDispose(ref OciServerHandle handle)
        {
            if (handle != null)
            {
                handle.Dispose();
            }
            handle = null;
        }

        internal static void SafeDispose(ref OciServiceContextHandle handle)
        {
            if (handle != null)
            {
                handle.Dispose();
            }
            handle = null;
        }

        internal static void SafeDispose(ref OciSessionHandle handle)
        {
            if (handle != null)
            {
                handle.Dispose();
            }
            handle = null;
        }

        internal static void SafeDispose(ref OciStatementHandle handle)
        {
            if (handle != null)
            {
                handle.Dispose();
            }
            handle = null;
        }

        internal void SetAttribute(OCI.ATTR attribute, OciHandle value, OciErrorHandle errorHandle)
        {
            int rc = TracedNativeMethods.OCIAttrSet(this, value, 0, attribute, errorHandle);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        internal void SetAttribute(OCI.ATTR attribute, int value, OciErrorHandle errorHandle)
        {
            int rc = TracedNativeMethods.OCIAttrSet(this, ref value, 0, attribute, errorHandle);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        internal void SetAttribute(OCI.ATTR attribute, string value, OciErrorHandle errorHandle)
        {
            uint length = (uint) value.Length;
            byte[] bytes = new byte[length * 4];
            uint size = this.GetBytes(value.ToCharArray(), 0, length, bytes, 0);
            int rc = TracedNativeMethods.OCIAttrSet(this, bytes, size, attribute, errorHandle);
            if (rc != 0)
            {
                OracleException.Check(errorHandle, rc);
            }
        }

        internal OciHandle EnvironmentHandle
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                if (this.HandleType == OCI.HTYPE.OCI_HTYPE_ENV)
                {
                    return this;
                }
                return this.ParentHandle.EnvironmentHandle;
            }
        }

        internal OCI.HTYPE HandleType
        {
            get
            {
                return this._handleType;
            }
        }

        public override bool IsInvalid
        {
            get
            {
                return (IntPtr.Zero == base.handle);
            }
        }

        internal bool IsUnicode
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this._isUnicode;
            }
        }

        internal OciHandle ParentHandle
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this._parentHandle;
            }
        }

        [Flags]
        protected enum HANDLEFLAG
        {
            DEFAULT,
            UNICODE,
            NLS
        }
    }
}

