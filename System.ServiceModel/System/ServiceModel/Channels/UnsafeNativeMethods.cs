namespace System.ServiceModel.Channels
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.EnterpriseServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Text;
    using System.Threading;
    using System.Transactions;

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        public const string ADVAPI32 = "advapi32.dll";
        public const int ALG_CLASS_DATA_ENCRYPT = 0x6000;
        public const int ALG_CLASS_HASH = 0x8000;
        public const int ALG_SID_AES = 0x11;
        public const int ALG_SID_MD5 = 3;
        public const int ALG_SID_RC4 = 1;
        public const int ALG_SID_SHA_256 = 12;
        public const int ALG_SID_SHA_512 = 14;
        public const int ALG_SID_SHA1 = 4;
        public const int ALG_TYPE_ANY = 0;
        public const int ALG_TYPE_BLOCK = 0x600;
        public const int ALG_TYPE_STREAM = 0x800;
        public const string BCRYPT = "bcrypt.dll";
        public const int CALG_AES = 0x6611;
        public const int CALG_MD5 = 0x8003;
        public const int CALG_RC4 = 0x6801;
        public const int CALG_SHA_256 = 0x800c;
        public const int CALG_SHA_512 = 0x800e;
        public const int CALG_SHA1 = 0x8004;
        public const int DUPLICATE_CLOSE_SOURCE = 1;
        public const int DUPLICATE_SAME_ACCESS = 2;
        public const int ERROR_ACCESS_DENIED = 5;
        public const int ERROR_ALLOTTED_SPACE_EXCEEDED = 0x540;
        public const int ERROR_ALREADY_EXISTS = 0xb7;
        public const int ERROR_BROKEN_PIPE = 0x6d;
        public const int ERROR_FILE_NOT_FOUND = 2;
        public const int ERROR_INVALID_HANDLE = 6;
        public const int ERROR_INVALID_PARAMETER = 0x57;
        public const int ERROR_IO_PENDING = 0x3e5;
        public const int ERROR_MORE_DATA = 0xea;
        public const int ERROR_NETNAME_DELETED = 0x40;
        public const int ERROR_NO_DATA = 0xe8;
        public const int ERROR_NO_SYSTEM_RESOURCES = 0x5aa;
        public const int ERROR_NO_TRACKING_SERVICE = 0x494;
        public const int ERROR_NOT_ENOUGH_MEMORY = 8;
        public const int ERROR_OPERATION_ABORTED = 0x3e3;
        public const int ERROR_OUTOFMEMORY = 14;
        public const int ERROR_PIPE_BUSY = 0xe7;
        public const int ERROR_PIPE_CONNECTED = 0x217;
        public const int ERROR_SERVICE_ALREADY_RUNNING = 0x420;
        public const int ERROR_SERVICE_DISABLED = 0x422;
        public const int ERROR_SHARING_VIOLATION = 0x20;
        public const int ERROR_SUCCESS = 0;
        public const int FILE_CREATE_PIPE_INSTANCE = 4;
        public const int FILE_FLAG_FIRST_PIPE_INSTANCE = 0x80000;
        public const int FILE_FLAG_OVERLAPPED = 0x40000000;
        public const int FILE_MAP_READ = 4;
        public const int FILE_MAP_WRITE = 2;
        public const int FILE_WRITE_ATTRIBUTES = 0x100;
        public const int FILE_WRITE_DATA = 2;
        public const int FILE_WRITE_EA = 0x10;
        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        public const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
        public const int FORMAT_MESSAGE_FROM_STRING = 0x400;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        public const int GENERIC_ALL = 0x10000000;
        public const int GENERIC_READ = -2147483648;
        public const int GENERIC_WRITE = 0x40000000;
        public const string KERNEL32 = "kernel32.dll";
        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_DECOMMIT = 0x4000;
        public const int MQ_ACTION_PEEK_CURRENT = -2147483648;
        public const int MQ_ACTION_PEEK_NEXT = -2147483647;
        public const int MQ_ACTION_RECEIVE = 0;
        public const int MQ_DENY_NONE = 0;
        public const int MQ_DENY_RECEIVE_SHARE = 1;
        public const int MQ_ERROR = -1072824319;
        public const int MQ_ERROR_ACCESS_DENIED = -1072824283;
        public const int MQ_ERROR_BAD_SECURITY_CONTEXT = -1072824267;
        public const int MQ_ERROR_BAD_XML_FORMAT = -1072824174;
        public const int MQ_ERROR_BUFFER_OVERFLOW = -1072824294;
        public const int MQ_ERROR_CANNOT_CREATE_CERT_STORE = -1072824209;
        public const int MQ_ERROR_CANNOT_CREATE_HASH_EX = -1072824191;
        public const int MQ_ERROR_CANNOT_CREATE_ON_GC = -1072824201;
        public const int MQ_ERROR_CANNOT_CREATE_PSC_OBJECTS = -1072824171;
        public const int MQ_ERROR_CANNOT_DELETE_PSC_OBJECTS = -1072824189;
        public const int MQ_ERROR_CANNOT_GET_DN = -1072824194;
        public const int MQ_ERROR_CANNOT_GRANT_ADD_GUID = -1072824206;
        public const int MQ_ERROR_CANNOT_HASH_DATA_EX = -1072824193;
        public const int MQ_ERROR_CANNOT_IMPERSONATE_CLIENT = -1072824284;
        public const int MQ_ERROR_CANNOT_JOIN_DOMAIN = -1072824202;
        public const int MQ_ERROR_CANNOT_LOAD_MQAD = -1072824187;
        public const int MQ_ERROR_CANNOT_LOAD_MQDSSRV = -1072824186;
        public const int MQ_ERROR_CANNOT_LOAD_MSMQOCM = -1072824205;
        public const int MQ_ERROR_CANNOT_OPEN_CERT_STORE = -1072824208;
        public const int MQ_ERROR_CANNOT_SET_CRYPTO_SEC_DESCR = -1072824212;
        public const int MQ_ERROR_CANNOT_SIGN_DATA_EX = -1072824192;
        public const int MQ_ERROR_CANNOT_UPDATE_PSC_OBJECTS = -1072824170;
        public const int MQ_ERROR_CANT_RESOLVE_SITES = -1072824183;
        public const int MQ_ERROR_CERTIFICATE_NOT_PROVIDED = -1072824211;
        public const int MQ_ERROR_COMPUTER_DOES_NOT_SUPPORT_ENCRYPTION = -1072824269;
        public const int MQ_ERROR_CORRUPTED_INTERNAL_CERTIFICATE = -1072824275;
        public const int MQ_ERROR_CORRUPTED_PERSONAL_CERT_STORE = -1072824271;
        public const int MQ_ERROR_CORRUPTED_SECURITY_DATA = -1072824272;
        public const int MQ_ERROR_COULD_NOT_GET_ACCOUNT_INFO = -1072824265;
        public const int MQ_ERROR_COULD_NOT_GET_USER_SID = -1072824266;
        public const int MQ_ERROR_DELETE_CN_IN_USE = -1072824248;
        public const int MQ_ERROR_DEPEND_WKS_LICENSE_OVERFLOW = -1072824217;
        public const int MQ_ERROR_DS_BIND_ROOT_FOREST = -1072824177;
        public const int MQ_ERROR_DS_ERROR = -1072824253;
        public const int MQ_ERROR_DS_IS_FULL = -1072824254;
        public const int MQ_ERROR_DS_LOCAL_USER = -1072824176;
        public const int MQ_ERROR_DTC_CONNECT = -1072824244;
        public const int MQ_ERROR_ENCRYPTION_PROVIDER_NOT_SUPPORTED = -1072824213;
        public const int MQ_ERROR_FAIL_VERIFY_SIGNATURE_EX = -1072824190;
        public const int MQ_ERROR_FORMATNAME_BUFFER_TOO_SMALL = -1072824289;
        public const int MQ_ERROR_GC_NEEDED = -1072824178;
        public const int MQ_ERROR_GUID_NOT_MATCHING = -1072824200;
        public const int MQ_ERROR_ILLEGAL_CONTEXT = -1072824229;
        public const int MQ_ERROR_ILLEGAL_CURSOR_ACTION = -1072824292;
        public const int MQ_ERROR_ILLEGAL_ENTERPRISE_OPERATION = -1072824207;
        public const int MQ_ERROR_ILLEGAL_FORMATNAME = -1072824290;
        public const int MQ_ERROR_ILLEGAL_MQCOLUMNS = -1072824264;
        public const int MQ_ERROR_ILLEGAL_MQPRIVATEPROPS = -1072824197;
        public const int MQ_ERROR_ILLEGAL_MQQMPROPS = -1072824255;
        public const int MQ_ERROR_ILLEGAL_MQQUEUEPROPS = -1072824259;
        public const int MQ_ERROR_ILLEGAL_OPERATION = -1072824220;
        public const int MQ_ERROR_ILLEGAL_PROPERTY_SIZE = -1072824261;
        public const int MQ_ERROR_ILLEGAL_PROPERTY_VALUE = -1072824296;
        public const int MQ_ERROR_ILLEGAL_PROPERTY_VT = -1072824295;
        public const int MQ_ERROR_ILLEGAL_PROPID = -1072824263;
        public const int MQ_ERROR_ILLEGAL_QUEUE_PATHNAME = -1072824300;
        public const int MQ_ERROR_ILLEGAL_RELATION = -1072824262;
        public const int MQ_ERROR_ILLEGAL_RESTRICTION_PROPID = -1072824260;
        public const int MQ_ERROR_ILLEGAL_SECURITY_DESCRIPTOR = -1072824287;
        public const int MQ_ERROR_ILLEGAL_SORT = -1072824304;
        public const int MQ_ERROR_ILLEGAL_SORT_PROPID = -1072824228;
        public const int MQ_ERROR_ILLEGAL_USER = -1072824303;
        public const int MQ_ERROR_INSUFFICIENT_PROPERTIES = -1072824257;
        public const int MQ_ERROR_INSUFFICIENT_RESOURCES = -1072824281;
        public const int MQ_ERROR_INTERNAL_USER_CERT_EXIST = -1072824274;
        public const int MQ_ERROR_INVALID_CERTIFICATE = -1072824276;
        public const int MQ_ERROR_INVALID_HANDLE = -1072824313;
        public const int MQ_ERROR_INVALID_OWNER = -1072824252;
        public const int MQ_ERROR_INVALID_PARAMETER = -1072824314;
        public const int MQ_ERROR_IO_TIMEOUT = -1072824293;
        public const int MQ_ERROR_LABEL_BUFFER_TOO_SMALL = -1072824226;
        public const int MQ_ERROR_LABEL_TOO_LONG = -1072824227;
        public const int MQ_ERROR_MACHINE_EXISTS = -1072824256;
        public const int MQ_ERROR_MACHINE_NOT_FOUND = -1072824307;
        public const int MQ_ERROR_MESSAGE_ALREADY_RECEIVED = -1072824291;
        public const int MQ_ERROR_MESSAGE_LOCKED_UNDER_TRANSACTION = -1072824164;
        public const int MQ_ERROR_MESSAGE_NOT_FOUND = -1072824184;
        public const int MQ_ERROR_MESSAGE_STORAGE_FAILED = -1072824278;
        public const int MQ_ERROR_MISSING_CONNECTOR_TYPE = -1072824235;
        public const int MQ_ERROR_MQIS_READONLY_MODE = -1072824224;
        public const int MQ_ERROR_MQIS_SERVER_EMPTY = -1072824225;
        public const int MQ_ERROR_MULTI_SORT_KEYS = -1072824179;
        public const int MQ_ERROR_NO_DS = -1072824301;
        public const int MQ_ERROR_NO_ENTRY_POINT_MSMQOCM = -1072824204;
        public const int MQ_ERROR_NO_GC_IN_DOMAIN = -1072824196;
        public const int MQ_ERROR_NO_INTERNAL_USER_CERT = -1072824273;
        public const int MQ_ERROR_NO_MQUSER_OU = -1072824188;
        public const int MQ_ERROR_NO_MSMQ_SERVERS_ON_DC = -1072824203;
        public const int MQ_ERROR_NO_MSMQ_SERVERS_ON_GC = -1072824195;
        public const int MQ_ERROR_NO_RESPONSE_FROM_OBJECT_SERVER = -1072824247;
        public const int MQ_ERROR_NOT_A_CORRECT_OBJECT_CLASS = -1072824180;
        public const int MQ_ERROR_NOT_SUPPORTED_BY_DEPENDENT_CLIENTS = -1072824182;
        public const int MQ_ERROR_OBJECT_SERVER_NOT_AVAILABLE = -1072824246;
        public const int MQ_ERROR_OPERATION_CANCELLED = -1072824312;
        public const int MQ_ERROR_OPERATION_NOT_SUPPORTED_BY_REMOTE_COMPUTER = -1072824181;
        public const int MQ_ERROR_PRIVILEGE_NOT_HELD = -1072824282;
        public const int MQ_ERROR_PROPERTIES_CONFLICT = -1072824185;
        public const int MQ_ERROR_PROPERTY = -1072824318;
        public const int MQ_ERROR_PROPERTY_NOTALLOWED = -1072824258;
        public const int MQ_ERROR_PROV_NAME_BUFFER_TOO_SMALL = -1072824221;
        public const int MQ_ERROR_PUBLIC_KEY_DOES_NOT_EXIST = -1072824198;
        public const int MQ_ERROR_PUBLIC_KEY_NOT_FOUND = -1072824199;
        public const int MQ_ERROR_Q_ADS_PROPERTY_NOT_SUPPORTED = -1072824175;
        public const int MQ_ERROR_Q_DNS_PROPERTY_NOT_SUPPORTED = -1072824210;
        public const int MQ_ERROR_QUEUE_DELETED = -1072824230;
        public const int MQ_ERROR_QUEUE_EXISTS = -1072824315;
        public const int MQ_ERROR_QUEUE_NOT_ACTIVE = -1072824316;
        public const int MQ_ERROR_QUEUE_NOT_AVAILABLE = -1072824245;
        public const int MQ_ERROR_QUEUE_NOT_FOUND = -1072824317;
        public const int MQ_ERROR_REMOTE_MACHINE_NOT_AVAILABLE = -1072824215;
        public const int MQ_ERROR_RESULT_BUFFER_TOO_SMALL = -1072824250;
        public const int MQ_ERROR_SECURITY_DESCRIPTOR_TOO_SMALL = -1072824285;
        public const int MQ_ERROR_SENDER_CERT_BUFFER_TOO_SMALL = -1072824277;
        public const int MQ_ERROR_SENDERID_BUFFER_TOO_SMALL = -1072824286;
        public const int MQ_ERROR_SERVICE_NOT_AVAILABLE = -1072824309;
        public const int MQ_ERROR_SHARING_VIOLATION = -1072824311;
        public const int MQ_ERROR_SIGNATURE_BUFFER_TOO_SMALL = -1072824222;
        public const int MQ_ERROR_STALE_HANDLE = -1072824234;
        public const int MQ_ERROR_SYMM_KEY_BUFFER_TOO_SMALL = -1072824223;
        public const int MQ_ERROR_TRANSACTION_ENLIST = -1072824232;
        public const int MQ_ERROR_TRANSACTION_IMPORT = -1072824242;
        public const int MQ_ERROR_TRANSACTION_SEQUENCE = -1072824239;
        public const int MQ_ERROR_TRANSACTION_USAGE = -1072824240;
        public const int MQ_ERROR_UNINITIALIZED_OBJECT = -1072824172;
        public const int MQ_ERROR_UNSUPPORTED_ACCESS_MODE = -1072824251;
        public const int MQ_ERROR_UNSUPPORTED_CLASS = -1072824173;
        public const int MQ_ERROR_UNSUPPORTED_FORMATNAME_OPERATION = -1072824288;
        public const int MQ_ERROR_UNSUPPORTED_OPERATION = -1072824214;
        public const int MQ_ERROR_USER_BUFFER_TOO_SMALL = -1072824280;
        public const int MQ_ERROR_WKS_CANT_SERVE_CLIENT = -1072824218;
        public const int MQ_ERROR_WRITE_NOT_ALLOWED = -1072824219;
        public const int MQ_INFORMATION_DUPLICATE_PROPERTY = 0x400e0005;
        public const int MQ_INFORMATION_FORMATNAME_BUFFER_TOO_SMALL = 0x400e0009;
        public const int MQ_INFORMATION_ILLEGAL_PROPERTY = 0x400e0002;
        public const int MQ_INFORMATION_INTERNAL_USER_CERT_EXIST = 0x400e000a;
        public const int MQ_INFORMATION_OPERATION_PENDING = 0x400e0006;
        public const int MQ_INFORMATION_OWNER_IGNORED = 0x400e000b;
        public const int MQ_INFORMATION_PROPERTY = 0x400e0001;
        public const int MQ_INFORMATION_PROPERTY_IGNORED = 0x400e0003;
        public const int MQ_INFORMATION_UNSUPPORTED_PROPERTY = 0x400e0004;
        public const int MQ_LOOKUP_PEEK_CURRENT = 0x40000010;
        public const int MQ_LOOKUP_RECEIVE_CURRENT = 0x40000020;
        public const int MQ_MAX_MSG_LABEL_LEN = 250;
        public const int MQ_MOVE_ACCESS = 4;
        public const int MQ_MTS_TRANSACTION = 1;
        public const int MQ_NO_TRANSACTION = 0;
        public const int MQ_RECEIVE_ACCESS = 1;
        public const int MQ_SEND_ACCESS = 2;
        public const int MQ_SINGLE_MESSAGE = 3;
        public const int MQ_TRANSACTIONAL = 1;
        public const int MQ_TRANSACTIONAL_NONE = 0;
        public const int MQMSG_ACKNOWLEDGMENT_NEG_ARRIVAL = 4;
        public const int MQMSG_ACKNOWLEDGMENT_NEG_RECEIVE = 8;
        public const int MQMSG_ACKNOWLEDGMENT_NONE = 0;
        public const int MQMSG_ACKNOWLEDGMENT_POS_ARRIVAL = 1;
        public const int MQMSG_ACKNOWLEDGMENT_POS_RECEIVE = 2;
        public const int MQMSG_AUTH_LEVEL_ALWAYS = 1;
        public const int MQMSG_AUTH_LEVEL_NONE = 0;
        public const int MQMSG_CLASS_NORMAL = 0;
        public const int MQMSG_CLASS_REPORT = 1;
        public const int MQMSG_DEADLETTER = 1;
        public const int MQMSG_DELIVERY_EXPRESS = 0;
        public const int MQMSG_DELIVERY_RECOVERABLE = 1;
        public const int MQMSG_JOURNAL = 2;
        public const int MQMSG_JOURNAL_NONE = 0;
        public const int MQMSG_PRIV_LEVEL_BODY_BASE = 1;
        public const int MQMSG_PRIV_LEVEL_BODY_ENHANCED = 3;
        public const int MQMSG_PRIV_LEVEL_NONE = 0;
        public const int MQMSG_SEND_ROUTE_TO_REPORT_QUEUE = 1;
        public const int MQMSG_SENDERID_TYPE_NONE = 0;
        public const int MQMSG_SENDERID_TYPE_SID = 1;
        public const int MQMSG_TRACE_NONE = 0;
        public const string MQRT = "mqrt.dll";
        public const string MS_ENH_RSA_AES_PROV = "Microsoft Enhanced RSA and AES Cryptographic Provider";
        public const int OPEN_EXISTING = 3;
        public const int PAGE_READWRITE = 4;
        public const int PIPE_ACCESS_DUPLEX = 3;
        public const int PIPE_READMODE_BYTE = 0;
        public const int PIPE_READMODE_MESSAGE = 2;
        public const int PIPE_TYPE_BYTE = 0;
        public const int PIPE_TYPE_MESSAGE = 4;
        public const int PIPE_UNLIMITED_INSTANCES = 0xff;
        public const int PROPID_M_ABORT_COUNT = 0x45;
        public const int PROPID_M_ACKNOWLEDGE = 6;
        public const int PROPID_M_ADMIN_QUEUE = 0x11;
        public const int PROPID_M_ADMIN_QUEUE_LEN = 0x12;
        public const int PROPID_M_APPSPECIFIC = 8;
        public const int PROPID_M_ARRIVEDTIME = 0x20;
        public const int PROPID_M_AUTH_LEVEL = 0x18;
        public const int PROPID_M_AUTHENTICATED = 0x19;
        public const int PROPID_M_AUTHENTICATED_EX = 0x35;
        public const int PROPID_M_BASE = 0;
        public const int PROPID_M_BODY = 9;
        public const int PROPID_M_BODY_SIZE = 10;
        public const int PROPID_M_BODY_TYPE = 0x2a;
        public const int PROPID_M_CLASS = 1;
        public const int PROPID_M_COMPOUND_MESSAGE = 0x3f;
        public const int PROPID_M_COMPOUND_MESSAGE_SIZE = 0x40;
        public const int PROPID_M_CONNECTOR_TYPE = 0x26;
        public const int PROPID_M_CORRELATIONID = 3;
        public const int PROPID_M_CORRELATIONID_SIZE = 20;
        public const int PROPID_M_DEADLETTER_QUEUE = 0x43;
        public const int PROPID_M_DEADLETTER_QUEUE_LEN = 0x44;
        public const int PROPID_M_DELIVERY = 5;
        public const int PROPID_M_DEST_FORMAT_NAME = 0x3a;
        public const int PROPID_M_DEST_FORMAT_NAME_LEN = 0x3b;
        public const int PROPID_M_DEST_QUEUE = 0x21;
        public const int PROPID_M_DEST_QUEUE_LEN = 0x22;
        public const int PROPID_M_DEST_SYMM_KEY = 0x2b;
        public const int PROPID_M_DEST_SYMM_KEY_LEN = 0x2c;
        public const int PROPID_M_ENCRYPTION_ALG = 0x1b;
        public const int PROPID_M_EXTENSION = 0x23;
        public const int PROPID_M_EXTENSION_LEN = 0x24;
        public const int PROPID_M_FIRST_IN_GROUP = 0x49;
        public const int PROPID_M_FIRST_IN_XACT = 50;
        public const int PROPID_M_GROUP_ID = 0x47;
        public const int PROPID_M_GROUP_ID_LEN = 0x48;
        public const int PROPID_M_HASH_ALG = 0x1a;
        public const int PROPID_M_JOURNAL = 7;
        public const int PROPID_M_LABEL = 11;
        public const int PROPID_M_LABEL_LEN = 12;
        public const int PROPID_M_LAST_IN_GROUP = 0x4a;
        public const int PROPID_M_LAST_IN_XACT = 0x33;
        public const int PROPID_M_LAST_MOVE_TIME = 0x4b;
        public const int PROPID_M_LOOKUPID = 60;
        public const int PROPID_M_MOVE_COUNT = 70;
        public const int PROPID_M_MSGID = 2;
        public const int PROPID_M_MSGID_SIZE = 20;
        public const int PROPID_M_PRIORITY = 4;
        public const int PROPID_M_PRIV_LEVEL = 0x17;
        public const int PROPID_M_PROV_NAME = 0x30;
        public const int PROPID_M_PROV_NAME_LEN = 0x31;
        public const int PROPID_M_PROV_TYPE = 0x2f;
        public const int PROPID_M_RESP_FORMAT_NAME = 0x36;
        public const int PROPID_M_RESP_FORMAT_NAME_LEN = 0x37;
        public const int PROPID_M_RESP_QUEUE = 15;
        public const int PROPID_M_RESP_QUEUE_LEN = 0x10;
        public const int PROPID_M_SECURITY_CONTEXT = 0x25;
        public const int PROPID_M_SENDER_CERT = 0x1c;
        public const int PROPID_M_SENDER_CERT_LEN = 0x1d;
        public const int PROPID_M_SENDERID = 20;
        public const int PROPID_M_SENDERID_LEN = 0x15;
        public const int PROPID_M_SENDERID_TYPE = 0x16;
        public const int PROPID_M_SENTTIME = 0x1f;
        public const int PROPID_M_SIGNATURE = 0x2d;
        public const int PROPID_M_SIGNATURE_LEN = 0x2e;
        public const int PROPID_M_SOAP_BODY = 0x42;
        public const int PROPID_M_SOAP_ENVELOPE = 0x3d;
        public const int PROPID_M_SOAP_ENVELOPE_LEN = 0x3e;
        public const int PROPID_M_SOAP_HEADER = 0x41;
        public const int PROPID_M_SRC_MACHINE_ID = 30;
        public const int PROPID_M_TIME_TO_BE_RECEIVED = 14;
        public const int PROPID_M_TIME_TO_REACH_QUEUE = 13;
        public const int PROPID_M_TRACE = 0x29;
        public const int PROPID_M_VERSION = 0x13;
        public const int PROPID_M_XACT_STATUS_QUEUE = 0x27;
        public const int PROPID_M_XACT_STATUS_QUEUE_LEN = 40;
        public const int PROPID_M_XACTID = 0x34;
        public const int PROPID_MGMT_QUEUE_BASE = 0;
        public const int PROPID_MGMT_QUEUE_SUBQUEUE_NAMES = 0x1b;
        public const int PROPID_PC_BASE = 0x16a8;
        public const int PROPID_PC_DS_ENABLED = 0x16aa;
        public const int PROPID_PC_VERSION = 0x16a9;
        public const int PROPID_Q_ADS_PATH = 0x7e;
        public const int PROPID_Q_AUTHENTICATE = 0x6f;
        public const int PROPID_Q_BASE = 100;
        public const int PROPID_Q_BASEPRIORITY = 0x6a;
        public const int PROPID_Q_CREATE_TIME = 0x6d;
        public const int PROPID_Q_INSTANCE = 0x65;
        public const int PROPID_Q_JOURNAL = 0x68;
        public const int PROPID_Q_JOURNAL_QUOTA = 0x6b;
        public const int PROPID_Q_LABEL = 0x6c;
        public const int PROPID_Q_MODIFY_TIME = 110;
        public const int PROPID_Q_MULTICAST_ADDRESS = 0x7d;
        public const int PROPID_Q_PATHNAME = 0x67;
        public const int PROPID_Q_PATHNAME_DNS = 0x7c;
        public const int PROPID_Q_PRIV_LEVEL = 0x70;
        public const int PROPID_Q_QUOTA = 0x69;
        public const int PROPID_Q_TRANSACTION = 0x71;
        public const int PROPID_Q_TYPE = 0x66;
        public const int PROV_RSA_AES = 0x18;
        public const int SDDL_REVISION_1 = 1;
        public const string SECUR32 = "secur32.dll";
        public const int SECURITY_IDENTIFICATION = 0x10000;
        public const int SECURITY_QOS_PRESENT = 0x100000;
        public const int STATUS_PENDING = 0x103;
        public const ushort VT_BOOL = 11;
        public const ushort VT_LPWSTR = 0x1f;
        public const ushort VT_NULL = 1;
        public const ushort VT_UI1 = 0x11;
        public const ushort VT_UI2 = 0x12;
        public const ushort VT_UI4 = 0x13;
        public const ushort VT_UI8 = 0x15;
        public const ushort VT_VECTOR = 0x1000;
        public const int WAIT_TIMEOUT = 0x102;
        public const string WS2_32 = "ws2_32.dll";
        public const int WSAACCESS = 0x271d;
        public const int WSAEADDRINUSE = 0x2740;
        public const int WSAEADDRNOTAVAIL = 0x2741;
        public const int WSAECONNABORTED = 0x2745;
        public const int WSAECONNREFUSED = 0x274d;
        public const int WSAECONNRESET = 0x2746;
        public const int WSAEHOSTDOWN = 0x2750;
        public const int WSAEHOSTUNREACH = 0x2751;
        public const int WSAEMFILE = 0x2728;
        public const int WSAEMSGSIZE = 0x2738;
        public const int WSAENETDOWN = 0x2742;
        public const int WSAENETRESET = 0x2744;
        public const int WSAENETUNREACH = 0x2743;
        public const int WSAENOBUFS = 0x2747;
        public const int WSAESHUTDOWN = 0x274a;
        public const int WSAETIMEDOUT = 0x274c;

        [DllImport("bcrypt.dll", SetLastError=true)]
        internal static extern int BCryptGetFipsAlgorithmMode([MarshalAs(UnmanagedType.U1)] out bool pfEnabled);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll")]
        internal static extern int CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int ConnectNamedPipe(PipeHandle handle, NativeOverlapped* lpOverlapped);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern PipeHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr lpSECURITY_ATTRIBUTES, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern System.ServiceModel.Channels.SafeFileMappingHandle CreateFileMapping(IntPtr fileHandle, SECURITY_ATTRIBUTES securityAttributes, int protect, int sizeHigh, int sizeLow, string name);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern PipeHandle CreateNamedPipe(string name, int openMode, int pipeMode, int maxInstances, int outBufSize, int inBufSize, int timeout, SECURITY_ATTRIBUTES securityAttributes);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern SafeWaitHandle CreateWaitableTimer(IntPtr mustBeZero, bool manualReset, string timerName);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern int DisconnectNamedPipe(PipeHandle handle);
        [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, PipeHandle hSourceHandle, SafeCloseHandle hTargetProcessHandle, out IntPtr lpTargetHandle, int dwDesiredAccess, bool bInheritHandle, int dwOptions);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr arguments);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern int FormatMessage(int dwFlags, System.ServiceModel.Channels.SafeLibraryHandle lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr arguments);
        internal static string GetComputerName(ComputerNameFormat nameType)
        {
            int size = 0;
            if (!GetComputerNameEx(nameType, null, ref size))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != 0xea)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                }
            }
            if (size < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("GetComputerNameReturnedInvalidLenth", new object[] { size })));
            }
            StringBuilder lpBuffer = new StringBuilder(size);
            if (!GetComputerNameEx(nameType, lpBuffer, ref size))
            {
                int num3 = Marshal.GetLastWin32Error();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(num3));
            }
            return lpBuffer.ToString();
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        private static extern bool GetComputerNameEx([In] ComputerNameFormat nameType, [In, Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpBuffer, [In, Out] ref int size);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern int GetHandleInformation(MsmqQueueHandle handle, out int flags);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool GetNamedPipeClientProcessId(PipeHandle handle, out int id);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool GetNamedPipeServerProcessId(PipeHandle handle, out int id);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int GetOverlappedResult(IntPtr handle, NativeOverlapped* overlapped, out int bytesTransferred, int wait);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int GetOverlappedResult(PipeHandle handle, NativeOverlapped* overlapped, out int bytesTransferred, int wait);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        internal static extern IntPtr GetProcAddress(System.ServiceModel.Channels.SafeLibraryHandle hModule, [In, MarshalAs(UnmanagedType.LPStr)] string lpProcName);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);
        internal static unsafe bool HasOverlappedIoCompleted(NativeOverlapped* overlapped)
        {
            return (overlapped.InternalLow != ((IntPtr) 0x103));
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern System.ServiceModel.Channels.SafeLibraryHandle LoadLibrary(string libFilename);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern System.ServiceModel.Channels.SafeViewOfFileHandle MapViewOfFile(System.ServiceModel.Channels.SafeFileMappingHandle handle, int dwDesiredAccess, int dwFileOffsetHigh, int dwFileOffsetLow, IntPtr dwNumberOfBytesToMap);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQBeginTransaction(out ITransaction refTransaction);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQCloseQueue(IntPtr handle);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern void MQFreeMemory(IntPtr nativeBuffer);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern unsafe int MQGetOverlappedResult(NativeOverlapped* nativeOverlapped);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQGetPrivateComputerInformation(string computerName, IntPtr properties);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQGetQueueProperties(string formatName, IntPtr properties);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQMarkMessageRejected(MsmqQueueHandle handle, long lookupId);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQMgmtGetInfo(string computerName, string objectName, IntPtr properties);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQMoveMessage(MsmqQueueHandle sourceQueueHandle, MsmqQueueHandle destinationQueueHandle, long lookupId, IntPtr transaction);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQMoveMessage(MsmqQueueHandle sourceQueueHandle, MsmqQueueHandle destinationQueueHandle, long lookupId, IDtcTransaction transaction);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQOpenQueue(string formatName, int access, int shareMode, out MsmqQueueHandle handle);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQPathNameToFormatName(string pathName, StringBuilder formatName, ref int count);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern unsafe int MQReceiveMessage(IntPtr handle, int timeout, int action, IntPtr properties, NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, IntPtr cursorHandle, ITransaction transaction);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern unsafe int MQReceiveMessage(IntPtr handle, int timeout, int action, IntPtr properties, NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, IntPtr cursorHandle, IntPtr transaction);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern unsafe int MQReceiveMessage(IntPtr handle, int timeout, int action, IntPtr properties, NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, IntPtr cursorHandle, IDtcTransaction transaction);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern unsafe int MQReceiveMessage(MsmqQueueHandle handle, int timeout, int action, IntPtr properties, NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, IntPtr cursorHandle, IntPtr transaction);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern unsafe int MQReceiveMessage(MsmqQueueHandle handle, int timeout, int action, IntPtr properties, NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, IntPtr cursorHandle, IDtcTransaction transaction);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern unsafe int MQReceiveMessage(MsmqQueueHandle handle, int timeout, int action, IntPtr properties, NativeOverlapped* nativeOverlapped, MQReceiveCallback receiveCallback, IntPtr cursorHandle, IntPtr transaction);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern unsafe int MQReceiveMessageByLookupId(MsmqQueueHandle handle, long lookupId, int action, IntPtr properties, NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, ITransaction transaction);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern unsafe int MQReceiveMessageByLookupId(MsmqQueueHandle handle, long lookupId, int action, IntPtr properties, NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, IntPtr transaction);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern unsafe int MQReceiveMessageByLookupId(MsmqQueueHandle handle, long lookupId, int action, IntPtr properties, NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, IDtcTransaction transaction);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQReplaceTransaction(ITransaction sourceTransaction, IDtcTransaction destinationTransaction);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQSendMessage(MsmqQueueHandle handle, IntPtr properties, IntPtr transaction);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQSendMessage(MsmqQueueHandle handle, IntPtr properties, IDtcTransaction transaction);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern System.ServiceModel.Channels.SafeFileMappingHandle OpenFileMapping(int access, bool inheritHandle, string name);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern int QueryPerformanceCounter(out long time);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int ReadFile(IntPtr handle, byte* bytes, int numBytesToRead, IntPtr numBytesRead_mustBeZero, NativeOverlapped* overlapped);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern int SetNamedPipeHandleState(PipeHandle handle, ref int mode, IntPtr collectionCount, IntPtr collectionDataTimeout);
        [DllImport("kernel32.dll", ExactSpelling=true)]
        public static extern bool SetWaitableTimer(SafeWaitHandle handle, ref long dueTime, int period, IntPtr mustBeZero, IntPtr mustBeZeroAlso, bool resume);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("secur32.dll")]
        internal static extern uint SspiExcludePackage([In] IntPtr AuthIdentity, [In, MarshalAs(UnmanagedType.LPWStr)] string pszPackageName, out IntPtr ppNewAuthIdentity);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("secur32.dll")]
        internal static extern int SspiFreeAuthIdentity([In] IntPtr ppAuthIdentity);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", ExactSpelling=true)]
        internal static extern int UnmapViewOfFile(IntPtr lpBaseAddress);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
        internal static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
        internal static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);
        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern unsafe int WriteFile(IntPtr handle, byte* bytes, int numBytesToWrite, IntPtr numBytesWritten_mustBeZero, NativeOverlapped* lpOverlapped);
        [DllImport("ws2_32.dll", SetLastError=true)]
        internal static extern unsafe bool WSAGetOverlappedResult(IntPtr socketHandle, NativeOverlapped* overlapped, out int bytesTransferred, bool wait, out uint flags);
        [DllImport("ws2_32.dll", SetLastError=true)]
        internal static extern unsafe int WSARecv(IntPtr handle, WSABuffer* buffers, int bufferCount, out int bytesTransferred, ref int socketFlags, NativeOverlapped* nativeOverlapped, IntPtr completionRoutine);

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MQMSGPROPS
        {
            public int count;
            public IntPtr ids;
            public IntPtr variants;
            public IntPtr status;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct MQPROPVARIANT
        {
            [FieldOffset(8)]
            public CAUI1 byteArrayValue;
            [FieldOffset(8)]
            public byte byteValue;
            [FieldOffset(8)]
            public IntPtr intPtr;
            [FieldOffset(8)]
            public int intValue;
            [FieldOffset(8)]
            public long longValue;
            [FieldOffset(2)]
            public ushort reserved1;
            [FieldOffset(4)]
            public ushort reserved2;
            [FieldOffset(6)]
            public ushort reserved3;
            [FieldOffset(8)]
            public short shortValue;
            [FieldOffset(8)]
            public CALPWSTR stringArraysValue;
            [FieldOffset(0)]
            public ushort vt;

            [StructLayout(LayoutKind.Sequential)]
            public struct CALPWSTR
            {
                public int count;
                public IntPtr stringArrays;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CAUI1
            {
                public int size;
                public IntPtr intPtr;
            }
        }

        public unsafe delegate void MQReceiveCallback(int error, IntPtr handle, int timeout, int action, IntPtr props, NativeOverlapped* nativeOverlapped, IntPtr cursor);

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            internal int nLength = Marshal.SizeOf(typeof(UnsafeNativeMethods.SECURITY_ATTRIBUTES));
            internal IntPtr lpSecurityDescriptor = IntPtr.Zero;
            internal bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WSABuffer
        {
            public int length;
            public IntPtr buffer;
        }
    }
}

