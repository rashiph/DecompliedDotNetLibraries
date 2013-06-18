namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public sealed class OracleException : DbException
    {
        private int _code;

        private OracleException(SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
            this._code = (int) si.GetValue("code", typeof(int));
            base.HResult = -2146232008;
        }

        private OracleException(string message, int code) : base(message)
        {
            this._code = code;
            base.HResult = -2146232008;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static void Check(OciErrorHandle errorHandle, int rc)
        {
            switch (((OCI.RETURNCODE) rc))
            {
                case OCI.RETURNCODE.OCI_INVALID_HANDLE:
                    throw System.Data.Common.ADP.InvalidOperation(Res.GetString("ADP_InternalError", new object[] { rc }));

                case OCI.RETURNCODE.OCI_ERROR:
                case OCI.RETURNCODE.OCI_NO_DATA:
                    throw System.Data.Common.ADP.OracleError(errorHandle, rc);
            }
            if (((rc < 0) || (rc == 0x63)) || (rc == 1))
            {
                throw System.Data.Common.ADP.Simple(Res.GetString("ADP_UnexpectedReturnCode", new object[] { rc.ToString(CultureInfo.CurrentCulture) }));
            }
        }

        internal static void Check(int rc, OracleInternalConnection internalConnection)
        {
            if (rc != 0)
            {
                throw System.Data.Common.ADP.OracleError(rc, internalConnection);
            }
        }

        private static bool ConnectionIsBroken(int code)
        {
            if ((0x30d4 <= code) && (code <= 0x319b))
            {
                return true;
            }
            switch (code)
            {
                case 0x1c:
                case 0x1b4:
                case 0x3f4:
                case 0x12:
                case 0x13:
                case 0x18:
                case 0x409:
                case 0x40a:
                case 0x433:
                case 0xc29:
                case 0xc2a:
                case 0x958:
                case 0x95f:
                    return true;
            }
            return false;
        }

        internal static OracleException CreateException(OciErrorHandle errorHandle, int rc)
        {
            using (NativeBuffer buffer = new NativeBuffer_Exception(0x3e8))
            {
                int num;
                string str;
                if (errorHandle != null)
                {
                    int recordno = 1;
                    int num2 = TracedNativeMethods.OCIErrorGet(errorHandle, recordno, out num, buffer);
                    if (num2 == 0)
                    {
                        str = errorHandle.PtrToString(buffer);
                        if (((num != 0) && str.StartsWith("ORA-00000", StringComparison.Ordinal)) && (TracedNativeMethods.oermsg((short) num, buffer) == 0))
                        {
                            str = errorHandle.PtrToString(buffer);
                        }
                    }
                    else
                    {
                        str = Res.GetString("ADP_NoMessageAvailable", new object[] { rc, num2 });
                        num = 0;
                    }
                    if (ConnectionIsBroken(num))
                    {
                        errorHandle.ConnectionIsBroken = true;
                    }
                }
                else
                {
                    str = Res.GetString("ADP_NoMessageAvailable", new object[] { rc, -1 });
                    num = 0;
                }
                return new OracleException(str, num);
            }
        }

        internal static OracleException CreateException(int rc, OracleInternalConnection internalConnection)
        {
            using (NativeBuffer buffer = new NativeBuffer_Exception(0x3e8))
            {
                string str;
                int length = buffer.Length;
                int dwErr = 0;
                int num2 = TracedNativeMethods.OraMTSOCIErrGet(ref dwErr, buffer, ref length);
                if (1 == num2)
                {
                    str = buffer.PtrToStringAnsi(0, length);
                }
                else
                {
                    str = Res.GetString("ADP_NoMessageAvailable", new object[] { rc, num2 });
                    dwErr = 0;
                }
                if (ConnectionIsBroken(dwErr))
                {
                    internalConnection.DoomThisConnection();
                }
                return new OracleException(str, dwErr);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo si, StreamingContext context)
        {
            if (si == null)
            {
                throw new ArgumentNullException("si");
            }
            si.AddValue("code", this._code, typeof(int));
            base.GetObjectData(si, context);
        }

        public int Code
        {
            get
            {
                return this._code;
            }
        }
    }
}

