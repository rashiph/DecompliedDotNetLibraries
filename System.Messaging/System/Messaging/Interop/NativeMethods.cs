namespace System.Messaging.Interop
{
    using System;
    using System.Messaging;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    internal static class NativeMethods
    {
        public const int ACKNOWLEDGE_FULL_REACH_QUEUE = 5;
        public const int ACKNOWLEDGE_FULL_RECEIVE = 14;
        public const int ACKNOWLEDGE_NEGATIVE_ARRIVAL = 4;
        public const int ACKNOWLEDGE_NEGATIVE_RECEIVE = 8;
        public const int ACKNOWLEDGE_NONE = 0;
        public const int ACKNOWLEDGE_NOTACKNOWLEDGE_REACH_QUEUE = 4;
        public const int ACKNOWLEDGE_NOTACKNOWLEDGE_RECEIVE = 12;
        public const int ACKNOWLEDGE_POSITIVE_ARRIVAL = 1;
        public const int ACKNOWLEDGE_POSITIVE_RECEIVE = 2;
        private const int ALG_CLASS_DATA_ENCRYPT = 0x6000;
        private const int ALG_CLASS_HASH = 0x8000;
        private const int ALG_SID_MAC = 5;
        private const int ALG_SID_MD2 = 1;
        private const int ALG_SID_MD4 = 2;
        private const int ALG_SID_MD5 = 3;
        private const int ALG_SID_RC2 = 2;
        private const int ALG_SID_RC4 = 1;
        private const int ALG_SID_RIPEMD = 6;
        private const int ALG_SID_RIPEMD160 = 7;
        private const int ALG_SID_SHA = 4;
        private const int ALG_SID_SSL3SHAMD5 = 8;
        private const int ALG_TYPE_ANY = 0;
        private const int ALG_TYPE_BLOCK = 0x600;
        private const int ALG_TYPE_STREAM = 0x800;
        public const int CALG_MAC = 0x8005;
        public const int CALG_MD2 = 0x8001;
        public const int CALG_MD4 = 0x8002;
        public const int CALG_MD5 = 0x8003;
        public const int CALG_RC2 = 0x6602;
        public const int CALG_RC4 = 0x6801;
        public const int CALG_SHA = 0x8004;
        public const int DACL_SECURITY_INFORMATION = 4;
        public const int DENY_ACCESS = 3;
        public const int E_ABORT = -2147467260;
        public const int E_FAIL = -2147467259;
        public const int E_HANDLE = -2147024890;
        public const int E_INVALIDARG = -2147024809;
        public const int E_NOINTERFACE = -2147467262;
        public const int E_NOTIMPL = -2147467263;
        public const int E_OUTOFMEMORY = -2147024882;
        public const int E_POINTER = -2147467261;
        public const int E_UNEXPECTED = -2147418113;
        public const int ERROR_SUCCESS = 0;
        public const int GRANT_ACCESS = 1;
        public static Guid IID_IUnknown = new Guid("{00000000-0000-0000-C000-000000000046}");
        public const int LOCK_EXCLUSIVE = 2;
        public const int LOCK_ONLYONCE = 4;
        public const int LOCK_WRITE = 1;
        internal const int LOOKUP_PEEK_MASK = 0x40000010;
        internal const int LOOKUP_RECEIVE_MASK = 0x40000020;
        public const int MACHINE_BASE = 200;
        public const int MACHINE_CONNECTION = 0xcc;
        public const int MACHINE_ENCRYPTION_PK = 0xcd;
        public const int MACHINE_ID = 0xca;
        public const int MACHINE_PATHNAME = 0xcb;
        public const int MACHINE_SITE_ID = 0xc9;
        public const int MANAGEMENT_ACTIVEQUEUES = 1;
        public const int MANAGEMENT_BASE = 0;
        public const int MANAGEMENT_CONNECTED = 4;
        public const int MANAGEMENT_DSSERVER = 3;
        public const int MANAGEMENT_PRIVATEQ = 2;
        public const int MANAGEMENT_TYPE = 5;
        public const int MAX_LABEL_LEN = 0x7c;
        public const int MAX_MESSAGE_ID_SIZE = 20;
        public const int MESSAGE_AUTHENTICATION_LEVEL_ALWAYS = 1;
        public const int MESSAGE_AUTHENTICATION_LEVEL_MSMQ10 = 2;
        public const int MESSAGE_AUTHENTICATION_LEVEL_MSMQ20 = 4;
        public const int MESSAGE_AUTHENTICATION_LEVEL_NONE = 0;
        public const int MESSAGE_CLASS_ACCESS_DENIED = 0x8004;
        public const int MESSAGE_CLASS_BAD_DESTINATION_QUEUE = 0x8000;
        public const int MESSAGE_CLASS_BAD_ENCRYPTION = 0x8007;
        public const int MESSAGE_CLASS_BAD_SIGNATURE = 0x8006;
        public const int MESSAGE_CLASS_COULD_NOT_ENCRYPT = 0x8008;
        public const int MESSAGE_CLASS_HOP_COUNT_EXCEEDED = 0x8005;
        public const int MESSAGE_CLASS_NORMAL = 0;
        public const int MESSAGE_CLASS_NOT_TRANSACTIONAL_MESSAGE = 0x800a;
        public const int MESSAGE_CLASS_NOT_TRANSACTIONAL_QUEUE = 0x8009;
        public const int MESSAGE_CLASS_PURGED = 0x8001;
        public const int MESSAGE_CLASS_QUEUE_DELETED = 0xc000;
        public const int MESSAGE_CLASS_QUEUE_EXCEED_QUOTA = 0x8003;
        public const int MESSAGE_CLASS_QUEUE_PURGED = 0xc001;
        public const int MESSAGE_CLASS_REACH_QUEUE = 2;
        public const int MESSAGE_CLASS_REACH_QUEUE_TIMEOUT = 0x8002;
        public const int MESSAGE_CLASS_RECEIVE = 0x4000;
        public const int MESSAGE_CLASS_RECEIVE_TIMEOUT = 0xc002;
        public const int MESSAGE_CLASS_REPORT = 1;
        public const int MESSAGE_DELIVERY_EXPRESS = 0;
        public const int MESSAGE_DELIVERY_RECOVERABLE = 1;
        public const int MESSAGE_JOURNAL_DEADLETTER = 1;
        public const int MESSAGE_JOURNAL_JOURNAL = 2;
        public const int MESSAGE_JOURNAL_NONE = 0;
        public const int MESSAGE_PRIVACY_LEVEL_BODY = 1;
        public const int MESSAGE_PRIVACY_LEVEL_NONE = 0;
        public const int MESSAGE_PROPID_ACKNOWLEDGE = 6;
        public const int MESSAGE_PROPID_ADMIN_QUEUE = 0x11;
        public const int MESSAGE_PROPID_ADMIN_QUEUE_LEN = 0x12;
        public const int MESSAGE_PROPID_APPSPECIFIC = 8;
        public const int MESSAGE_PROPID_ARRIVEDTIME = 0x20;
        public const int MESSAGE_PROPID_AUTH_LEVEL = 0x18;
        public const int MESSAGE_PROPID_AUTHENTICATED = 0x19;
        public const int MESSAGE_PROPID_BASE = 0;
        public const int MESSAGE_PROPID_BODY = 9;
        public const int MESSAGE_PROPID_BODY_SIZE = 10;
        public const int MESSAGE_PROPID_BODY_TYPE = 0x2a;
        public const int MESSAGE_PROPID_CLASS = 1;
        public const int MESSAGE_PROPID_CONNECTOR_TYPE = 0x26;
        public const int MESSAGE_PROPID_CORRELATIONID = 3;
        public const int MESSAGE_PROPID_DELIVERY = 5;
        public const int MESSAGE_PROPID_DEST_QUEUE = 0x21;
        public const int MESSAGE_PROPID_DEST_QUEUE_LEN = 0x22;
        public const int MESSAGE_PROPID_DEST_SYMM_KEY = 0x2b;
        public const int MESSAGE_PROPID_DEST_SYMM_KEY_LEN = 0x2c;
        public const int MESSAGE_PROPID_ENCRYPTION_ALG = 0x1b;
        public const int MESSAGE_PROPID_EXTENSION = 0x23;
        public const int MESSAGE_PROPID_EXTENSION_LEN = 0x24;
        public const int MESSAGE_PROPID_FIRST_IN_XACT = 50;
        public const int MESSAGE_PROPID_HASH_ALG = 0x1a;
        public const int MESSAGE_PROPID_JOURNAL = 7;
        public const int MESSAGE_PROPID_LABEL = 11;
        public const int MESSAGE_PROPID_LABEL_LEN = 12;
        public const int MESSAGE_PROPID_LAST_IN_XACT = 0x33;
        public const int MESSAGE_PROPID_LOOKUPID = 60;
        public const int MESSAGE_PROPID_MSGID = 2;
        public const int MESSAGE_PROPID_PRIORITY = 4;
        public const int MESSAGE_PROPID_PRIV_LEVEL = 0x17;
        public const int MESSAGE_PROPID_PROV_NAME = 0x30;
        public const int MESSAGE_PROPID_PROV_NAME_LEN = 0x31;
        public const int MESSAGE_PROPID_PROV_TYPE = 0x2f;
        public const int MESSAGE_PROPID_RESP_QUEUE = 15;
        public const int MESSAGE_PROPID_RESP_QUEUE_LEN = 0x10;
        public const int MESSAGE_PROPID_SECURITY_CONTEXT = 0x25;
        public const int MESSAGE_PROPID_SENDER_CERT = 0x1c;
        public const int MESSAGE_PROPID_SENDER_CERT_LEN = 0x1d;
        public const int MESSAGE_PROPID_SENDERID = 20;
        public const int MESSAGE_PROPID_SENDERID_LEN = 0x15;
        public const int MESSAGE_PROPID_SENDERID_TYPE = 0x16;
        public const int MESSAGE_PROPID_SENTTIME = 0x1f;
        public const int MESSAGE_PROPID_SIGNATURE = 0x2d;
        public const int MESSAGE_PROPID_SIGNATURE_LEN = 0x2e;
        public const int MESSAGE_PROPID_SRC_MACHINE_ID = 30;
        public const int MESSAGE_PROPID_TIME_TO_BE_RECEIVED = 14;
        public const int MESSAGE_PROPID_TIME_TO_REACH_QUEUE = 13;
        public const int MESSAGE_PROPID_TRACE = 0x29;
        public const int MESSAGE_PROPID_VERSION = 0x13;
        public const int MESSAGE_PROPID_XACT_STATUS_QUEUE = 0x27;
        public const int MESSAGE_PROPID_XACT_STATUS_QUEUE_LEN = 40;
        public const int MESSAGE_PROPID_XACTID = 0x34;
        public const int MESSAGE_SENDERID_TYPE_NONE = 0;
        public const int MESSAGE_SENDERID_TYPE_SID = 1;
        public const int MESSAGE_TRACE_NONE = 0;
        public const int MESSAGE_TRACE_SEND_ROUTE_TO_REPORT_QUEUE = 1;
        public const int MQ_ERROR_SECURITY_DESCRIPTOR_TOO_SMALL = -1072824285;
        public const int MQ_OK = 0;
        public const int NO_MULTIPLE_TRUSTEE = 0;
        public const int PROV_DSS = 3;
        public const int PROV_FORTEZZA = 4;
        public const int PROV_MS_EXCHANGE = 5;
        public const int PROV_RSA_FULL = 1;
        public const int PROV_RSA_SIG = 2;
        public const int PROV_SSL = 6;
        public const int PROV_STT_ACQ = 8;
        public const int PROV_STT_BRND = 9;
        public const int PROV_STT_ISS = 11;
        public const int PROV_STT_MER = 7;
        public const int PROV_STT_ROOT = 10;
        public const int QUEUE_ACCESS_ADMIN = 0x80;
        public const int QUEUE_ACCESS_PEEK = 0x20;
        public const int QUEUE_ACCESS_RECEIVE = 1;
        public const int QUEUE_ACCESS_SEND = 2;
        public const int QUEUE_ACTION_PEEK_CURRENT = -2147483648;
        public const int QUEUE_ACTION_PEEK_NEXT = -2147483647;
        public const int QUEUE_ACTION_RECEIVE = 0;
        public const int QUEUE_AUTHENTICATE_AUTHENTICATE = 1;
        public const int QUEUE_AUTHENTICATE_NONE = 0;
        public const int QUEUE_JOURNAL_JOURNAL = 1;
        public const int QUEUE_JOURNAL_NONE = 0;
        public const int QUEUE_PRIVACY_LEVEL_BODY = 2;
        public const int QUEUE_PRIVACY_LEVEL_NONE = 0;
        public const int QUEUE_PRIVACY_LEVEL_OPTIONAL = 1;
        public const int QUEUE_PROPID_AUTHENTICATE = 0x6f;
        public const int QUEUE_PROPID_BASE = 100;
        public const int QUEUE_PROPID_BASEPRIORITY = 0x6a;
        public const int QUEUE_PROPID_CREATE_TIME = 0x6d;
        public const int QUEUE_PROPID_INSTANCE = 0x65;
        public const int QUEUE_PROPID_JOURNAL = 0x68;
        public const int QUEUE_PROPID_JOURNAL_QUOTA = 0x6b;
        public const int QUEUE_PROPID_LABEL = 0x6c;
        public const int QUEUE_PROPID_MODIFY_TIME = 110;
        public const int QUEUE_PROPID_MULTICAST_ADDRESS = 0x7d;
        public const int QUEUE_PROPID_PATHNAME = 0x67;
        public const int QUEUE_PROPID_PRIV_LEVEL = 0x70;
        public const int QUEUE_PROPID_QUOTA = 0x69;
        public const int QUEUE_PROPID_TRANSACTION = 0x71;
        public const int QUEUE_PROPID_TYPE = 0x66;
        public const int QUEUE_SHARED_MODE_DENY_NONE = 0;
        public const int QUEUE_SHARED_MODE_DENY_RECEIVE = 1;
        public const int QUEUE_TRANSACTION_MTS = 1;
        public const int QUEUE_TRANSACTION_NONE = 0;
        public const int QUEUE_TRANSACTION_SINGLE = 3;
        public const int QUEUE_TRANSACTION_XA = 2;
        public const int QUEUE_TRANSACTIONAL_NONE = 0;
        public const int QUEUE_TRANSACTIONAL_TRANSACTIONAL = 1;
        public const int REVOKE_ACCESS = 4;
        public const int SECURITY_DESCRIPTOR_REVISION = 1;
        public const int SET_ACCESS = 2;
        public const int STATFLAG_DEFAULT = 0;
        public const int STATFLAG_NONAME = 1;
        public const int STATFLAG_NOOPEN = 2;
        public const int STGC_DANGEROUSLYCOMMITMERELYTODISKCACHE = 4;
        public const int STGC_DEFAULT = 0;
        public const int STGC_ONLYIFCURRENT = 2;
        public const int STGC_OVERWRITE = 1;
        public const int STREAM_SEEK_CUR = 1;
        public const int STREAM_SEEK_END = 2;
        public const int STREAM_SEEK_SET = 0;
        public const int TRUSTEE_IS_ALIAS = 4;
        public const int TRUSTEE_IS_DOMAIN = 3;
        public const int TRUSTEE_IS_GROUP = 2;
        public const int TRUSTEE_IS_NAME = 1;
        public const int TRUSTEE_IS_SID = 0;
        public const int TRUSTEE_IS_USER = 1;
        public const int TRUSTEE_IS_WELL_KNOWN_GROUP = 5;

        [DllImport("mqrt.dll", EntryPoint="MQGetSecurityContextEx", CharSet=CharSet.Unicode)]
        private static extern int IntMQGetSecurityContextEx(IntPtr lpCertBuffer, int dwCertBufferLength, out SecurityContextHandle phSecurityContext);
        public static int MQGetSecurityContextEx(out SecurityContextHandle securityContext)
        {
            int num;
            try
            {
                num = IntMQGetSecurityContextEx(IntPtr.Zero, 0, out securityContext);
            }
            catch (DllNotFoundException)
            {
                throw new InvalidOperationException(Res.GetString("MSMQNotInstalled"));
            }
            return num;
        }

        [return: MarshalAs(UnmanagedType.Interface)]
        [DllImport("ole32.dll", PreserveSig=false)]
        public static extern object OleLoadFromStream(IStream stream, [In] ref Guid iid);
        [DllImport("ole32.dll", PreserveSig=false)]
        public static extern void OleSaveToStream(IPersistStream persistStream, IStream stream);

        [StructLayout(LayoutKind.Sequential)]
        public struct ExplicitAccess
        {
            public int grfAccessPermissions;
            public int grfAccessMode;
            public int grfInheritance;
            public IntPtr pMultipleTrustees;
            public int MultipleTrusteeOperation;
            public int TrusteeForm;
            public int TrusteeType;
            public IntPtr data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SECURITY_DESCRIPTOR
        {
            public byte revision;
            public byte size;
            public short control;
            public IntPtr owner = IntPtr.Zero;
            public IntPtr Group = IntPtr.Zero;
            public IntPtr Sacl = IntPtr.Zero;
            public IntPtr Dacl = IntPtr.Zero;
        }
    }
}

