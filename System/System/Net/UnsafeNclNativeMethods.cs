namespace System.Net
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.Net.Cache;
    using System.Net.Sockets;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNclNativeMethods
    {
        private const string ADVAPI32 = "advapi32.dll";
        private const string BCRYPT = "bcrypt.dll";
        private const string CRYPT32 = "crypt32.dll";
        private const string HTTPAPI = "httpapi.dll";
        private const string KERNEL32 = "kernel32.dll";
        private const string RASAPI32 = "rasapi32.dll";
        private const string SCHANNEL = "schannel.dll";
        private const string SECUR32 = "secur32.dll";
        private const string SECURITY = "security.dll";
        private const string WINHTTP = "winhttp.dll";
        private const string WININET = "wininet.dll";
        private const string WS2_32 = "ws2_32.dll";

        [DllImport("kernel32.dll")]
        internal static extern IntPtr CreateSemaphore([In] IntPtr lpSemaphoreAttributes, [In] int lInitialCount, [In] int lMaximumCount, [In] IntPtr lpName);
        [DllImport("kernel32.dll", ExactSpelling=true)]
        internal static extern void DebugBreak();
        [DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
        internal static extern uint GetCurrentThreadId();
        [DllImport("kernel32.dll")]
        internal static extern bool ReleaseSemaphore([In] IntPtr hSemaphore, [In] int lReleaseCount, [In] IntPtr lpPreviousCount);

        internal static class ErrorCodes
        {
            internal const uint ERROR_ALREADY_EXISTS = 0xb7;
            internal const uint ERROR_HANDLE_EOF = 0x26;
            internal const uint ERROR_INVALID_PARAMETER = 0x57;
            internal const uint ERROR_IO_PENDING = 0x3e5;
            internal const uint ERROR_MORE_DATA = 0xea;
            internal const uint ERROR_NOT_FOUND = 0x490;
            internal const uint ERROR_NOT_SUPPORTED = 50;
            internal const uint ERROR_OPERATION_ABORTED = 0x3e3;
            internal const uint ERROR_SUCCESS = 0;
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class HttpApi
        {
            private static bool extendedProtectionSupported;
            private const string HTTPAPI = "httpapi.dll";
            private const int HttpHeaderRequestMaximum = 0x29;
            private const int HttpHeaderResponseMaximum = 30;
            internal static readonly string[] HttpVerbs;
            internal const int MaxTimeout = 6;
            private static bool supported;
            private static HTTPAPI_VERSION version;

            static HttpApi()
            {
                string[] strArray = new string[20];
                strArray[1] = "Unknown";
                strArray[2] = "Invalid";
                strArray[3] = "OPTIONS";
                strArray[4] = "GET";
                strArray[5] = "HEAD";
                strArray[6] = "POST";
                strArray[7] = "PUT";
                strArray[8] = "DELETE";
                strArray[9] = "TRACE";
                strArray[10] = "CONNECT";
                strArray[11] = "TRACK";
                strArray[12] = "MOVE";
                strArray[13] = "COPY";
                strArray[14] = "PROPFIND";
                strArray[15] = "PROPPATCH";
                strArray[0x10] = "MKCOL";
                strArray[0x11] = "LOCK";
                strArray[0x12] = "UNLOCK";
                strArray[0x13] = "SEARCH";
                HttpVerbs = strArray;
                SafeLoadLibrary library = SafeLoadLibrary.LoadLibraryEx("httpapi.dll");
                if (!library.IsInvalid)
                {
                    try
                    {
                        InitHttpApi(2, 0);
                        if (!Supported)
                        {
                            InitHttpApi(1, 0);
                        }
                    }
                    finally
                    {
                        library.Close();
                    }
                }
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            private static unsafe void CopyOutAddress(IntPtr address, ref SocketAddress v4address, ref SocketAddress v6address)
            {
                if (address != IntPtr.Zero)
                {
                    switch (*(((ushort*) address)))
                    {
                        case 2:
                            v6address = null;
                            fixed (byte* numRef = v4address.m_Buffer)
                            {
                                for (int i = 2; i < 0x10; i++)
                                {
                                    numRef[i] = *((byte*) (((void*) address) + i));
                                }
                            }
                            return;

                        case 0x17:
                            byte[] buffer2;
                            v4address = null;
                            if (((buffer2 = v6address.m_Buffer) == null) || (buffer2.Length == 0))
                            {
                                numRef2 = null;
                                goto Label_0086;
                            }
                            fixed (byte* numRef2 = buffer2)
                            {
                                int num3;
                            Label_0086:
                                num3 = 2;
                                while (num3 < 0x1c)
                                {
                                    numRef2[num3] = *((byte*) (((void*) address) + num3));
                                    num3++;
                                }
                            }
                            return;
                    }
                }
                v4address = null;
                v6address = null;
            }

            internal static unsafe uint GetChunks(byte[] memoryBlob, IntPtr originalAddress, ref int dataChunkIndex, ref uint dataChunkOffset, byte[] buffer, int offset, int size)
            {
                uint num = 0;
                fixed (byte* numRef = memoryBlob)
                {
                    HTTP_REQUEST* http_requestPtr = (HTTP_REQUEST*) numRef;
                    long num2 = (long) ((numRef - ((void*) originalAddress)) / 1);
                    if (((http_requestPtr->EntityChunkCount > 0) && (dataChunkIndex < http_requestPtr->EntityChunkCount)) && (dataChunkIndex != -1))
                    {
                        HTTP_DATA_CHUNK* http_data_chunkPtr = (HTTP_DATA_CHUNK*) (((IntPtr) num2) + ((IntPtr) (http_requestPtr->pEntityChunks + dataChunkIndex)));
                        fixed (byte* numRef2 = buffer)
                        {
                            byte* numPtr = numRef2 + offset;
                            while ((dataChunkIndex < http_requestPtr->EntityChunkCount) && (num < size))
                            {
                                if (dataChunkOffset >= http_data_chunkPtr->BufferLength)
                                {
                                    dataChunkOffset = 0;
                                    dataChunkIndex++;
                                    http_data_chunkPtr++;
                                }
                                else
                                {
                                    byte* numPtr2 = (http_data_chunkPtr->pBuffer + dataChunkOffset) + ((IntPtr) num2);
                                    uint num3 = http_data_chunkPtr->BufferLength - dataChunkOffset;
                                    if (num3 > size)
                                    {
                                        num3 = (uint) size;
                                    }
                                    for (uint i = 0; i < num3; i++)
                                    {
                                        numPtr++;
                                        numPtr2++;
                                        numPtr[0] = numPtr2[0];
                                    }
                                    num += num3;
                                    dataChunkOffset += num3;
                                }
                            }
                        }
                    }
                    if (dataChunkIndex == http_requestPtr->EntityChunkCount)
                    {
                        dataChunkIndex = -1;
                    }
                }
                return num;
            }

            internal static unsafe WebHeaderCollection GetHeaders(byte[] memoryBlob, IntPtr originalAddress)
            {
                WebHeaderCollection headers = new WebHeaderCollection(WebHeaderCollectionType.HttpListenerRequest);
                fixed (byte* numRef = memoryBlob)
                {
                    int num2;
                    HTTP_REQUEST* http_requestPtr = (HTTP_REQUEST*) numRef;
                    long num = (long) ((numRef - ((void*) originalAddress)) / 1);
                    if (http_requestPtr->Headers.UnknownHeaderCount != 0)
                    {
                        HTTP_UNKNOWN_HEADER* http_unknown_headerPtr = ((HTTP_UNKNOWN_HEADER*) num) + http_requestPtr->Headers.pUnknownHeaders;
                        for (num2 = 0; num2 < http_requestPtr->Headers.UnknownHeaderCount; num2++)
                        {
                            if ((http_unknown_headerPtr->pName != null) && (http_unknown_headerPtr->NameLength > 0))
                            {
                                string str2;
                                string name = new string(http_unknown_headerPtr->pName + ((sbyte*) num), 0, http_unknown_headerPtr->NameLength);
                                if ((http_unknown_headerPtr->pRawValue != null) && (http_unknown_headerPtr->RawValueLength > 0))
                                {
                                    str2 = new string(http_unknown_headerPtr->pRawValue + ((sbyte*) num), 0, http_unknown_headerPtr->RawValueLength);
                                }
                                else
                                {
                                    str2 = string.Empty;
                                }
                                headers.AddInternal(name, str2);
                            }
                            http_unknown_headerPtr++;
                        }
                    }
                    HTTP_KNOWN_HEADER* http_known_headerPtr = &http_requestPtr->Headers.KnownHeaders;
                    for (num2 = 0; num2 < 0x29; num2++)
                    {
                        if (http_known_headerPtr->pRawValue != null)
                        {
                            string str3 = new string(http_known_headerPtr->pRawValue + ((sbyte*) num), 0, http_known_headerPtr->RawValueLength);
                            headers.AddInternal(HTTP_REQUEST_HEADER_ID.ToString(num2), str3);
                        }
                        http_known_headerPtr++;
                    }
                }
                return headers;
            }

            internal static unsafe string GetKnownHeader(HTTP_REQUEST* request, int headerIndex)
            {
                return GetKnownHeader(request, 0L, headerIndex);
            }

            private static unsafe string GetKnownHeader(HTTP_REQUEST* request, long fixup, int headerIndex)
            {
                string str = null;
                HTTP_KNOWN_HEADER* http_known_headerPtr = &request.Headers.KnownHeaders + headerIndex;
                if (http_known_headerPtr->pRawValue != null)
                {
                    str = new string(http_known_headerPtr->pRawValue + ((sbyte*) fixup), 0, http_known_headerPtr->RawValueLength);
                }
                return str;
            }

            internal static unsafe string GetKnownHeader(byte[] memoryBlob, IntPtr originalAddress, int headerIndex)
            {
                fixed (byte* numRef = memoryBlob)
                {
                    return GetKnownHeader((HTTP_REQUEST*) numRef, (long) ((numRef - ((void*) originalAddress)) / 1), headerIndex);
                }
            }

            internal static unsafe HTTP_VERB GetKnownVerb(byte[] memoryBlob, IntPtr originalAddress)
            {
                HTTP_VERB httpVerbUnknown = HTTP_VERB.HttpVerbUnknown;
                fixed (byte* numRef = memoryBlob)
                {
                    HTTP_REQUEST* http_requestPtr = (HTTP_REQUEST*) numRef;
                    if ((http_requestPtr->Verb > HTTP_VERB.HttpVerbUnparsed) && (http_requestPtr->Verb < HTTP_VERB.HttpVerbMaximum))
                    {
                        httpVerbUnknown = http_requestPtr->Verb;
                    }
                }
                return httpVerbUnknown;
            }

            internal static unsafe IPEndPoint GetLocalEndPoint(byte[] memoryBlob, IntPtr originalAddress)
            {
                byte[] buffer;
                SocketAddress address = new SocketAddress(AddressFamily.InterNetwork, 0x10);
                SocketAddress address2 = new SocketAddress(AddressFamily.InterNetworkV6, 0x1c);
                if (((buffer = memoryBlob) == null) || (buffer.Length == 0))
                {
                    numRef = null;
                    goto Label_002D;
                }
                fixed (byte* numRef = buffer)
                {
                    HTTP_REQUEST* http_requestPtr;
                Label_002D:
                    http_requestPtr = (HTTP_REQUEST*) numRef;
                    IntPtr ptr = (http_requestPtr->Address.pLocalAddress != null) ? (((IntPtr) ((long) ((numRef - ((void*) originalAddress)) / 1))) + http_requestPtr->Address.pLocalAddress) : IntPtr.Zero;
                    CopyOutAddress(ptr, ref address, ref address2);
                }
                IPEndPoint point = null;
                if (address != null)
                {
                    return (IPEndPoint.Any.Create(address) as IPEndPoint);
                }
                if (address2 != null)
                {
                    point = IPEndPoint.IPv6Any.Create(address2) as IPEndPoint;
                }
                return point;
            }

            internal static unsafe IPEndPoint GetRemoteEndPoint(byte[] memoryBlob, IntPtr originalAddress)
            {
                byte[] buffer;
                SocketAddress address = new SocketAddress(AddressFamily.InterNetwork, 0x10);
                SocketAddress address2 = new SocketAddress(AddressFamily.InterNetworkV6, 0x1c);
                if (((buffer = memoryBlob) == null) || (buffer.Length == 0))
                {
                    numRef = null;
                    goto Label_002D;
                }
                fixed (byte* numRef = buffer)
                {
                    HTTP_REQUEST* http_requestPtr;
                Label_002D:
                    http_requestPtr = (HTTP_REQUEST*) numRef;
                    IntPtr ptr = (http_requestPtr->Address.pRemoteAddress != null) ? (((IntPtr) ((long) ((numRef - ((void*) originalAddress)) / 1))) + http_requestPtr->Address.pRemoteAddress) : IntPtr.Zero;
                    CopyOutAddress(ptr, ref address, ref address2);
                }
                IPEndPoint point = null;
                if (address != null)
                {
                    return (IPEndPoint.Any.Create(address) as IPEndPoint);
                }
                if (address2 != null)
                {
                    point = IPEndPoint.IPv6Any.Create(address2) as IPEndPoint;
                }
                return point;
            }

            internal static unsafe string GetVerb(HTTP_REQUEST* request)
            {
                return GetVerb(request, 0L);
            }

            private static unsafe string GetVerb(HTTP_REQUEST* request, long fixup)
            {
                string str = null;
                if ((request.Verb > HTTP_VERB.HttpVerbUnknown) && (request.Verb < HTTP_VERB.HttpVerbMaximum))
                {
                    return HttpVerbs[(int) request.Verb];
                }
                if ((request.Verb == HTTP_VERB.HttpVerbUnknown) && (request.pUnknownVerb != null))
                {
                    str = new string(request.pUnknownVerb + ((sbyte*) fixup), 0, request.UnknownVerbLength);
                }
                return str;
            }

            internal static unsafe string GetVerb(byte[] memoryBlob, IntPtr originalAddress)
            {
                fixed (byte* numRef = memoryBlob)
                {
                    return GetVerb((HTTP_REQUEST*) numRef, (long) ((numRef - ((void*) originalAddress)) / 1));
                }
            }

            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe uint HttpAddUrl(CriticalHandle requestQueueHandle, string pFullyQualifiedUrl, void* pReserved);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern uint HttpAddUrlToUrlGroup(ulong urlGroupId, string pFullyQualifiedUrl, ulong context, uint pReserved);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
            internal static extern uint HttpCloseServerSession(ulong serverSessionId);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
            internal static extern uint HttpCloseUrlGroup(ulong urlGroupId);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe uint HttpCreateServerSession(HTTPAPI_VERSION version, ulong* serverSessionId, uint reserved);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe uint HttpCreateUrlGroup(ulong serverSessionId, ulong* urlGroupId, uint reserved);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe uint HttpInitialize(HTTPAPI_VERSION version, uint flags, void* pReserved);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe uint HttpReceiveClientCertificate(CriticalHandle requestQueueHandle, ulong connectionId, uint flags, byte* pSslClientCertInfo, uint sslClientCertInfoSize, uint* pBytesReceived, NativeOverlapped* pOverlapped);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe uint HttpReceiveClientCertificate(CriticalHandle requestQueueHandle, ulong connectionId, uint flags, HTTP_SSL_CLIENT_CERT_INFO* pSslClientCertInfo, uint sslClientCertInfoSize, uint* pBytesReceived, NativeOverlapped* pOverlapped);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe uint HttpReceiveHttpRequest(CriticalHandle requestQueueHandle, ulong requestId, uint flags, HTTP_REQUEST* pRequestBuffer, uint requestBufferLength, uint* pBytesReturned, NativeOverlapped* pOverlapped);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe uint HttpReceiveRequestEntityBody(CriticalHandle requestQueueHandle, ulong requestId, uint flags, void* pEntityBuffer, uint entityBufferLength, uint* pBytesReturned, NativeOverlapped* pOverlapped);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern uint HttpRemoveUrl(CriticalHandle requestQueueHandle, string pFullyQualifiedUrl);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern uint HttpRemoveUrlFromUrlGroup(ulong urlGroupId, string pFullyQualifiedUrl, uint flags);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe uint HttpSendHttpResponse(CriticalHandle requestQueueHandle, ulong requestId, uint flags, HTTP_RESPONSE* pHttpResponse, void* pCachePolicy, uint* pBytesSent, SafeLocalFree pRequestBuffer, uint requestBufferLength, NativeOverlapped* pOverlapped, void* pLogData);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe uint HttpSendResponseEntityBody(CriticalHandle requestQueueHandle, ulong requestId, uint flags, ushort entityChunkCount, HTTP_DATA_CHUNK* pEntityChunks, uint* pBytesSent, SafeLocalFree pRequestBuffer, uint requestBufferLength, NativeOverlapped* pOverlapped, void* pLogData);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
            internal static extern uint HttpSetUrlGroupProperty(ulong urlGroupId, HTTP_SERVER_PROPERTY serverProperty, IntPtr pPropertyInfo, uint propertyInfoLength);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe uint HttpWaitForDisconnect(CriticalHandle requestQueueHandle, ulong connectionId, NativeOverlapped* pOverlapped);
            private static void InitHttpApi(ushort majorVersion, ushort minorVersion)
            {
                version.HttpApiMajorVersion = majorVersion;
                version.HttpApiMinorVersion = minorVersion;
                uint num = 0;
                extendedProtectionSupported = true;
                if (ComNetOS.IsWin7)
                {
                    num = HttpInitialize(version, 1, null);
                }
                else
                {
                    num = HttpInitialize(version, 5, null);
                    if (num == 0x57)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintWarning(Logging.HttpListener, SR.GetString("net_listener_cbt_not_supported"));
                        }
                        extendedProtectionSupported = false;
                        num = HttpInitialize(version, 1, null);
                    }
                }
                supported = num == 0;
            }

            internal static HTTP_API_VERSION ApiVersion
            {
                get
                {
                    if ((version.HttpApiMajorVersion == 2) && (version.HttpApiMinorVersion == 0))
                    {
                        return HTTP_API_VERSION.Version20;
                    }
                    if ((version.HttpApiMajorVersion == 1) && (version.HttpApiMinorVersion == 0))
                    {
                        return HTTP_API_VERSION.Version10;
                    }
                    return HTTP_API_VERSION.Invalid;
                }
            }

            internal static bool ExtendedProtectionSupported
            {
                get
                {
                    return extendedProtectionSupported;
                }
            }

            internal static bool Supported
            {
                get
                {
                    return supported;
                }
            }

            internal static HTTPAPI_VERSION Version
            {
                get
                {
                    return version;
                }
            }

            internal enum HTTP_API_VERSION
            {
                Invalid,
                Version10,
                Version20
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_BINDING_INFO
            {
                internal UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS Flags;
                internal IntPtr RequestQueueHandle;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_COOKED_URL
            {
                internal ushort FullUrlLength;
                internal ushort HostLength;
                internal ushort AbsPathLength;
                internal ushort QueryStringLength;
                internal unsafe ushort* pFullUrl;
                internal unsafe ushort* pHost;
                internal unsafe ushort* pAbsPath;
                internal unsafe ushort* pQueryString;
            }

            [StructLayout(LayoutKind.Sequential, Size=0x20)]
            internal struct HTTP_DATA_CHUNK
            {
                internal UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK_TYPE DataChunkType;
                internal uint p0;
                internal unsafe byte* pBuffer;
                internal uint BufferLength;
            }

            internal enum HTTP_DATA_CHUNK_TYPE
            {
                HttpDataChunkFromMemory,
                HttpDataChunkFromFileHandle,
                HttpDataChunkFromFragmentCache,
                HttpDataChunkMaximum
            }

            [Flags]
            internal enum HTTP_FLAGS : uint
            {
                HTTP_INITIALIZE_CBT = 4,
                HTTP_INITIALIZE_SERVER = 1,
                HTTP_PROPERTY_FLAG_PRESENT = 1,
                HTTP_RECEIVE_REQUEST_FLAG_COPY_BODY = 1,
                HTTP_RECEIVE_SECURE_CHANNEL_TOKEN = 1,
                HTTP_SEND_REQUEST_FLAG_MORE_DATA = 1,
                HTTP_SEND_RESPONSE_FLAG_DISCONNECT = 1,
                HTTP_SEND_RESPONSE_FLAG_MORE_DATA = 2,
                HTTP_SEND_RESPONSE_FLAG_RAW_HEADER = 4,
                NONE = 0
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_KNOWN_HEADER
            {
                internal ushort RawValueLength;
                internal unsafe sbyte* pRawValue;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_REQUEST
            {
                internal uint Flags;
                internal ulong ConnectionId;
                internal ulong RequestId;
                internal ulong UrlContext;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_VERSION Version;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_VERB Verb;
                internal ushort UnknownVerbLength;
                internal ushort RawUrlLength;
                internal unsafe sbyte* pUnknownVerb;
                internal unsafe sbyte* pRawUrl;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_COOKED_URL CookedUrl;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_TRANSPORT_ADDRESS Address;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_HEADERS Headers;
                internal ulong BytesReceived;
                internal ushort EntityChunkCount;
                internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK* pEntityChunks;
                internal ulong RawConnectionId;
                internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_SSL_INFO* pSslInfo;
                internal ushort RequestInfoCount;
                internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_INFO* pRequestInfo;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_REQUEST_CHANNEL_BIND_STATUS
            {
                internal IntPtr ServiceName;
                internal IntPtr ChannelToken;
                internal uint ChannelTokenSize;
                internal uint Flags;
            }

            internal static class HTTP_REQUEST_HEADER_ID
            {
                private static string[] m_Strings = new string[] { 
                    "Cache-Control", "Connection", "Date", "Keep-Alive", "Pragma", "Trailer", "Transfer-Encoding", "Upgrade", "Via", "Warning", "Allow", "Content-Length", "Content-Type", "Content-Encoding", "Content-Language", "Content-Location", 
                    "Content-MD5", "Content-Range", "Expires", "Last-Modified", "Accept", "Accept-Charset", "Accept-Encoding", "Accept-Language", "Authorization", "Cookie", "Expect", "From", "Host", "If-Match", "If-Modified-Since", "If-None-Match", 
                    "If-Range", "If-Unmodified-Since", "Max-Forwards", "Proxy-Authorization", "Referer", "Range", "Te", "Translate", "User-Agent"
                 };

                internal static string ToString(int position)
                {
                    return m_Strings[position];
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_REQUEST_HEADERS
            {
                internal ushort UnknownHeaderCount;
                internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER* pUnknownHeaders;
                internal ushort TrailerCount;
                internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER* pTrailers;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_02;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_03;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_04;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_05;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_06;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_07;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_08;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_09;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_10;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_11;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_12;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_13;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_14;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_15;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_16;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_17;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_18;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_19;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_20;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_21;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_22;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_23;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_24;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_25;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_26;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_27;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_28;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_29;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_30;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_31;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_32;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_33;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_34;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_35;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_36;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_37;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_38;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_39;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_40;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_41;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_REQUEST_INFO
            {
                internal UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_INFO_TYPE InfoType;
                internal uint InfoLength;
                internal unsafe void* pInfo;
            }

            internal enum HTTP_REQUEST_INFO_TYPE
            {
                HttpRequestInfoTypeAuth
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_RESPONSE
            {
                internal uint Flags;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_VERSION Version;
                internal ushort StatusCode;
                internal ushort ReasonLength;
                internal unsafe sbyte* pReason;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADERS Headers;
                internal ushort EntityChunkCount;
                internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK* pEntityChunks;
                internal ushort ResponseInfoCount;
                internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_INFO* pResponseInfo;
            }

            internal static class HTTP_RESPONSE_HEADER_ID
            {
                private static Hashtable m_Hashtable = new Hashtable(30);
                private static string[] m_Strings = new string[] { 
                    "Cache-Control", "Connection", "Date", "Keep-Alive", "Pragma", "Trailer", "Transfer-Encoding", "Upgrade", "Via", "Warning", "Allow", "Content-Length", "Content-Type", "Content-Encoding", "Content-Language", "Content-Location", 
                    "Content-MD5", "Content-Range", "Expires", "Last-Modified", "Accept-Ranges", "Age", "ETag", "Location", "Proxy-Authenticate", "Retry-After", "Server", "Set-Cookie", "Vary", "WWW-Authenticate"
                 };

                static HTTP_RESPONSE_HEADER_ID()
                {
                    for (int i = 0; i < 30; i++)
                    {
                        m_Hashtable.Add(m_Strings[i], i);
                    }
                }

                internal static int IndexOfKnownHeader(string HeaderName)
                {
                    object obj2 = m_Hashtable[HeaderName];
                    if (obj2 != null)
                    {
                        return (int) obj2;
                    }
                    return -1;
                }

                internal static string ToString(int position)
                {
                    return m_Strings[position];
                }

                internal enum Enum
                {
                    HttpHeaderAcceptRanges = 20,
                    HttpHeaderAge = 0x15,
                    HttpHeaderAllow = 10,
                    HttpHeaderCacheControl = 0,
                    HttpHeaderConnection = 1,
                    HttpHeaderContentEncoding = 13,
                    HttpHeaderContentLanguage = 14,
                    HttpHeaderContentLength = 11,
                    HttpHeaderContentLocation = 15,
                    HttpHeaderContentMd5 = 0x10,
                    HttpHeaderContentRange = 0x11,
                    HttpHeaderContentType = 12,
                    HttpHeaderDate = 2,
                    HttpHeaderEtag = 0x16,
                    HttpHeaderExpires = 0x12,
                    HttpHeaderKeepAlive = 3,
                    HttpHeaderLastModified = 0x13,
                    HttpHeaderLocation = 0x17,
                    HttpHeaderMaximum = 0x29,
                    HttpHeaderPragma = 4,
                    HttpHeaderProxyAuthenticate = 0x18,
                    HttpHeaderResponseMaximum = 30,
                    HttpHeaderRetryAfter = 0x19,
                    HttpHeaderServer = 0x1a,
                    HttpHeaderSetCookie = 0x1b,
                    HttpHeaderTrailer = 5,
                    HttpHeaderTransferEncoding = 6,
                    HttpHeaderUpgrade = 7,
                    HttpHeaderVary = 0x1c,
                    HttpHeaderVia = 8,
                    HttpHeaderWarning = 9,
                    HttpHeaderWwwAuthenticate = 0x1d
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_RESPONSE_HEADERS
            {
                internal ushort UnknownHeaderCount;
                internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER* pUnknownHeaders;
                internal ushort TrailerCount;
                internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER* pTrailers;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_02;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_03;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_04;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_05;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_06;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_07;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_08;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_09;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_10;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_11;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_12;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_13;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_14;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_15;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_16;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_17;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_18;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_19;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_20;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_21;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_22;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_23;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_24;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_25;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_26;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_27;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_28;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_29;
                internal UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER KnownHeaders_30;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_RESPONSE_INFO
            {
                internal UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_INFO_TYPE Type;
                internal uint Length;
                internal unsafe void* pInfo;
            }

            internal enum HTTP_RESPONSE_INFO_TYPE
            {
                HttpResponseInfoTypeMultipleKnownHeaders,
                HttpResponseInfoTypeAuthenticationProperty,
                HttpResponseInfoTypeQosProperty
            }

            internal enum HTTP_SERVER_PROPERTY
            {
                HttpServerAuthenticationProperty,
                HttpServerLoggingProperty,
                HttpServerQosProperty,
                HttpServerTimeoutsProperty,
                HttpServerQueueLengthProperty,
                HttpServerStateProperty,
                HttpServer503VerbosityProperty,
                HttpServerBindingProperty,
                HttpServerExtendedAuthenticationProperty,
                HttpServerListenEndpointProperty,
                HttpServerChannelBindProperty,
                HttpServerProtectionLevelProperty
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_SERVICE_BINDING_BASE
            {
                internal UnsafeNclNativeMethods.HttpApi.HTTP_SERVICE_BINDING_TYPE Type;
            }

            internal enum HTTP_SERVICE_BINDING_TYPE : uint
            {
                HttpServiceBindingTypeA = 2,
                HttpServiceBindingTypeNone = 0,
                HttpServiceBindingTypeW = 1
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_SSL_CLIENT_CERT_INFO
            {
                internal uint CertFlags;
                internal uint CertEncodedSize;
                internal unsafe byte* pCertEncoded;
                internal unsafe void* Token;
                internal byte CertDeniedByMapper;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_SSL_INFO
            {
                internal ushort ServerCertKeySize;
                internal ushort ConnectionKeySize;
                internal uint ServerCertIssuerSize;
                internal uint ServerCertSubjectSize;
                internal unsafe sbyte* pServerCertIssuer;
                internal unsafe sbyte* pServerCertSubject;
                internal unsafe UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* pClientCertInfo;
                internal uint SslClientCertNegotiated;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_TIMEOUT_LIMIT_INFO
            {
                internal UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS Flags;
                internal ushort EntityBody;
                internal ushort DrainEntityBody;
                internal ushort RequestQueue;
                internal ushort IdleConnection;
                internal ushort HeaderWait;
                internal uint MinSendRate;
            }

            internal enum HTTP_TIMEOUT_TYPE
            {
                EntityBody,
                DrainEntityBody,
                RequestQueue,
                IdleConnection,
                HeaderWait,
                MinSendRate
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_TRANSPORT_ADDRESS
            {
                internal unsafe UnsafeNclNativeMethods.HttpApi.SOCKADDR* pRemoteAddress;
                internal unsafe UnsafeNclNativeMethods.HttpApi.SOCKADDR* pLocalAddress;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_UNKNOWN_HEADER
            {
                internal ushort NameLength;
                internal ushort RawValueLength;
                internal unsafe sbyte* pName;
                internal unsafe sbyte* pRawValue;
            }

            internal enum HTTP_VERB
            {
                HttpVerbUnparsed,
                HttpVerbUnknown,
                HttpVerbInvalid,
                HttpVerbOPTIONS,
                HttpVerbGET,
                HttpVerbHEAD,
                HttpVerbPOST,
                HttpVerbPUT,
                HttpVerbDELETE,
                HttpVerbTRACE,
                HttpVerbCONNECT,
                HttpVerbTRACK,
                HttpVerbMOVE,
                HttpVerbCOPY,
                HttpVerbPROPFIND,
                HttpVerbPROPPATCH,
                HttpVerbMKCOL,
                HttpVerbLOCK,
                HttpVerbUNLOCK,
                HttpVerbSEARCH,
                HttpVerbMaximum
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTP_VERSION
            {
                internal ushort MajorVersion;
                internal ushort MinorVersion;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HTTPAPI_VERSION
            {
                internal ushort HttpApiMajorVersion;
                internal ushort HttpApiMinorVersion;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct SOCKADDR
            {
                internal ushort sa_family;
                internal byte sa_data;
                internal byte sa_data_02;
                internal byte sa_data_03;
                internal byte sa_data_04;
                internal byte sa_data_05;
                internal byte sa_data_06;
                internal byte sa_data_07;
                internal byte sa_data_08;
                internal byte sa_data_09;
                internal byte sa_data_10;
                internal byte sa_data_11;
                internal byte sa_data_12;
                internal byte sa_data_13;
                internal byte sa_data_14;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class NativeNTSSPI
        {
            private const string SECURITY = "security.dll";

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("security.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int DecryptMessage([In] ref SSPIHandle contextHandle, [In, Out] SecurityBufferDescriptor inputOutput, [In] uint sequenceNumber, uint* qualityOfProtection);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("security.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int EncryptMessage(ref SSPIHandle contextHandle, [In] uint qualityOfProtection, [In, Out] SecurityBufferDescriptor inputOutput, [In] uint sequenceNumber);
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class NativePKI
        {
            private const string CRYPT32 = "crypt32.dll";

            [DllImport("crypt32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int CertVerifyCertificateChainPolicy([In] IntPtr policy, [In] SafeFreeCertChain chainContext, [In] ref ChainPolicyParameter cpp, [In, Out] ref ChainPolicyStatus ps);
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class NativeSSLWin9xSSPI
        {
            private const string SCHANNEL = "schannel.dll";
            private const string SECUR32 = "secur32.dll";

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("schannel.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int SealMessage(ref SSPIHandle contextHandle, [In] uint qualityOfProtection, [In, Out] SecurityBufferDescriptor inputOutput, [In] uint sequenceNumber);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("schannel.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int UnsealMessage([In] ref SSPIHandle contextHandle, [In, Out] SecurityBufferDescriptor inputOutput, [In] IntPtr qualityOfProtection, [In] uint sequenceNumber);
        }

        internal static class NTStatus
        {
            internal const uint STATUS_OBJECT_NAME_NOT_FOUND = 0xc0000034;
            internal const uint STATUS_SUCCESS = 0;
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class OSSOCK
        {
            private const string mswsock = "mswsock.dll";
            private const string WS2_32 = "ws2_32.dll";

            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError bind([In] SafeCloseSocket socketHandle, [In] byte[] socketAddress, [In] int socketAddressSize);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern IntPtr gethostbyaddr([In] ref int addr, [In] int len, [In] ProtocolFamily type);
            [DllImport("ws2_32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
            internal static extern IntPtr gethostbyname([In] string host);
            [DllImport("ws2_32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
            internal static extern SocketError gethostname([Out] StringBuilder hostName, [In] int bufferLength);
            [DllImport("ws2_32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
            internal static extern SocketError getnameinfo([In] byte[] sa, [In] int salen, [In, Out] StringBuilder host, [In] int hostlen, [In, Out] StringBuilder serv, [In] int servlen, [In] int flags);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError getpeername([In] SafeCloseSocket socketHandle, [Out] byte[] socketAddress, [In, Out] ref int socketAddressSize);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError getsockname([In] SafeCloseSocket socketHandle, [Out] byte[] socketAddress, [In, Out] ref int socketAddressSize);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError getsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, out int optionValue, [In, Out] ref int optionLength);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError getsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [Out] byte[] optionValue, [In, Out] ref int optionLength);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError getsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, out IPMulticastRequest optionValue, [In, Out] ref int optionLength);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError getsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, out IPv6MulticastRequest optionValue, [In, Out] ref int optionLength);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError getsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, out Linger optionValue, [In, Out] ref int optionLength);
            [DllImport("ws2_32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
            internal static extern int inet_addr([In] string cp);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError ioctlsocket([In] SafeCloseSocket socketHandle, [In] int cmd, [In, Out] ref int argp);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError listen([In] SafeCloseSocket socketHandle, [In] int backlog);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern unsafe int recv([In] IntPtr socketHandle, [In] byte* pinnedBuffer, [In] int len, [In] SocketFlags socketFlags);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern unsafe int recvfrom([In] IntPtr socketHandle, [In] byte* pinnedBuffer, [In] int len, [In] SocketFlags socketFlags, [Out] byte[] socketAddress, [In, Out] ref int socketAddressSize);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern int select([In] int ignoredParameter, [In, Out] IntPtr[] readfds, [In, Out] IntPtr[] writefds, [In, Out] IntPtr[] exceptfds, [In] ref TimeValue timeout);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern int select([In] int ignoredParameter, [In, Out] IntPtr[] readfds, [In, Out] IntPtr[] writefds, [In, Out] IntPtr[] exceptfds, [In] IntPtr nullTimeout);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern unsafe int send([In] IntPtr socketHandle, [In] byte* pinnedBuffer, [In] int len, [In] SocketFlags socketFlags);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern unsafe int sendto([In] IntPtr socketHandle, [In] byte* pinnedBuffer, [In] int len, [In] SocketFlags socketFlags, [In] byte[] socketAddress, [In] int socketAddressSize);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref int optionValue, [In] int optionLength);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] byte[] optionValue, [In] int optionLength);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref IntPtr pointer, [In] int optionLength);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref IPMulticastRequest mreq, [In] int optionLength);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref IPv6MulticastRequest mreq, [In] int optionLength);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError setsockopt([In] SafeCloseSocket socketHandle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref Linger linger, [In] int optionLength);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError shutdown([In] SafeCloseSocket socketHandle, [In] int how);
            [DllImport("mswsock.dll", SetLastError=true)]
            internal static extern bool TransmitFile([In] SafeCloseSocket socket, [In] IntPtr fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] IntPtr overlapped, [In] IntPtr buffers, [In] TransmitFileOptions flags);
            [DllImport("mswsock.dll", SetLastError=true)]
            internal static extern bool TransmitFile([In] SafeCloseSocket socket, [In] SafeHandle fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] IntPtr overlapped, [In] IntPtr buffers, [In] TransmitFileOptions flags);
            [DllImport("mswsock.dll", SetLastError=true)]
            internal static extern bool TransmitFile([In] SafeCloseSocket socket, [In] SafeHandle fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] SafeHandle overlapped, [In] TransmitFileBuffers buffers, [In] TransmitFileOptions flags);
            [DllImport("mswsock.dll", EntryPoint="TransmitFile", SetLastError=true)]
            internal static extern bool TransmitFile_Blocking([In] IntPtr socket, [In] SafeHandle fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] SafeHandle overlapped, [In] TransmitFileBuffers buffers, [In] TransmitFileOptions flags);
            [DllImport("mswsock.dll", EntryPoint="TransmitFile", SetLastError=true)]
            internal static extern bool TransmitFile_Blocking2([In] IntPtr socket, [In] IntPtr fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] SafeHandle overlapped, [In] TransmitFileBuffers buffers, [In] TransmitFileOptions flags);
            [DllImport("mswsock.dll", EntryPoint="TransmitFile", SetLastError=true)]
            internal static extern bool TransmitFile2([In] SafeCloseSocket socket, [In] IntPtr fileHandle, [In] int numberOfBytesToWrite, [In] int numberOfBytesPerSend, [In] SafeHandle overlapped, [In] TransmitFileBuffers buffers, [In] TransmitFileOptions flags);
            [DllImport("ws2_32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
            internal static extern SocketError WSAAddressToString([In] byte[] socketAddress, [In] int socketAddressSize, [In] IntPtr lpProtocolInfo, [Out] StringBuilder addressString, [In, Out] ref int addressStringLength);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError WSAConnect([In] IntPtr socketHandle, [In] byte[] socketAddress, [In] int socketAddressSize, [In] IntPtr inBuffer, [In] IntPtr outBuffer, [In] IntPtr sQOS, [In] IntPtr gQOS);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern unsafe int WSADuplicateSocket([In] SafeCloseSocket socketHandle, [In] uint targetProcessID, [In] byte* pinnedBuffer);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError WSAEnumNetworkEvents([In] SafeCloseSocket socketHandle, [In] SafeWaitHandle Event, [In, Out] ref NetworkEvents networkEvents);
            [DllImport("ws2_32.dll", CharSet=CharSet.Auto, SetLastError=true)]
            internal static extern int WSAEnumProtocols([In, MarshalAs(UnmanagedType.LPArray)] int[] lpiProtocols, [In] SafeLocalFree lpProtocolBuffer, [In, Out] ref uint lpdwBufferLength);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError WSAEventSelect([In] SafeCloseSocket socketHandle, [In] IntPtr Event, [In] AsyncEventBits NetworkEvents);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError WSAEventSelect([In] SafeCloseSocket socketHandle, [In] SafeHandle Event, [In] AsyncEventBits NetworkEvents);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern bool WSAGetOverlappedResult([In] SafeCloseSocket socketHandle, [In] SafeHandle overlapped, out uint bytesTransferred, [In] bool wait, out SocketFlags socketFlags);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError WSAIoctl([In] SafeCloseSocket socketHandle, [In] int ioControlCode, [In, Out] ref Guid guid, [In] int guidSize, out IntPtr funcPtr, [In] int funcPtrSize, out int bytesTransferred, [In] IntPtr shouldBeNull, [In] IntPtr shouldBeNull2);
            [DllImport("ws2_32.dll", EntryPoint="WSAIoctl", SetLastError=true)]
            internal static extern SocketError WSAIoctl_Blocking([In] IntPtr socketHandle, [In] int ioControlCode, [In] byte[] inBuffer, [In] int inBufferSize, [Out] byte[] outBuffer, [In] int outBufferSize, out int bytesTransferred, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);
            [DllImport("ws2_32.dll", EntryPoint="WSAIoctl", SetLastError=true)]
            internal static extern SocketError WSAIoctl_Blocking_Internal([In] IntPtr socketHandle, [In] uint ioControlCode, [In] IntPtr inBuffer, [In] int inBufferSize, [Out] IntPtr outBuffer, [In] int outBufferSize, out int bytesTransferred, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError WSARecv([In] SafeCloseSocket socketHandle, [In] IntPtr buffers, [In] int bufferCount, out int bytesTransferred, [In, Out] ref SocketFlags socketFlags, [In] IntPtr overlapped, [In] IntPtr completionRoutine);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError WSARecv([In] SafeCloseSocket socketHandle, [In, Out] ref WSABuffer buffer, [In] int bufferCount, out int bytesTransferred, [In, Out] ref SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError WSARecv([In] SafeCloseSocket socketHandle, [In, Out] WSABuffer[] buffers, [In] int bufferCount, out int bytesTransferred, [In, Out] ref SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);
            [DllImport("ws2_32.dll", EntryPoint="WSARecv", SetLastError=true)]
            internal static extern SocketError WSARecv_Blocking([In] IntPtr socketHandle, [In, Out] WSABuffer[] buffers, [In] int bufferCount, out int bytesTransferred, [In, Out] ref SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError WSARecvFrom([In] SafeCloseSocket socketHandle, [In, Out] ref WSABuffer buffer, [In] int bufferCount, out int bytesTransferred, [In, Out] ref SocketFlags socketFlags, [In] IntPtr socketAddressPointer, [In] IntPtr socketAddressSizePointer, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError WSARecvFrom([In] SafeCloseSocket socketHandle, [In, Out] WSABuffer[] buffers, [In] int bufferCount, out int bytesTransferred, [In, Out] ref SocketFlags socketFlags, [In] IntPtr socketAddressPointer, [In] IntPtr socketAddressSizePointer, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError WSASend([In] SafeCloseSocket socketHandle, [In] IntPtr buffers, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] IntPtr overlapped, [In] IntPtr completionRoutine);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError WSASend([In] SafeCloseSocket socketHandle, [In] ref WSABuffer buffer, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError WSASend([In] SafeCloseSocket socketHandle, [In] WSABuffer[] buffersArray, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);
            [DllImport("ws2_32.dll", EntryPoint="WSASend", SetLastError=true)]
            internal static extern SocketError WSASend_Blocking([In] IntPtr socketHandle, [In] WSABuffer[] buffersArray, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError WSASendTo([In] SafeCloseSocket socketHandle, [In] WSABuffer[] buffersArray, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] IntPtr socketAddress, [In] int socketAddressSize, [In] SafeNativeOverlapped overlapped, [In] IntPtr completionRoutine);
            [DllImport("ws2_32.dll", SetLastError=true)]
            internal static extern SocketError WSASendTo([In] SafeCloseSocket socketHandle, [In] ref WSABuffer buffer, [In] int bufferCount, out int bytesTransferred, [In] SocketFlags socketFlags, [In] IntPtr socketAddress, [In] int socketAddressSize, [In] SafeHandle overlapped, [In] IntPtr completionRoutine);
            [DllImport("ws2_32.dll", CharSet=CharSet.Auto, SetLastError=true)]
            internal static extern SafeCloseSocket.InnerSafeCloseSocket WSASocket([In] AddressFamily addressFamily, [In] SocketType socketType, [In] ProtocolType protocolType, [In] IntPtr protocolInfo, [In] uint group, [In] SocketConstructorFlags flags);
            [DllImport("ws2_32.dll", CharSet=CharSet.Auto, SetLastError=true)]
            internal static extern unsafe SafeCloseSocket.InnerSafeCloseSocket WSASocket([In] AddressFamily addressFamily, [In] SocketType socketType, [In] ProtocolType protocolType, [In] byte* pinnedBuffer, [In] uint group, [In] SocketConstructorFlags flags);
            [DllImport("ws2_32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
            internal static extern SocketError WSAStartup([In] short wVersionRequested, out WSAData lpWSAData);
            [DllImport("ws2_32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
            internal static extern SocketError WSAStringToAddress([In] string addressString, [In] AddressFamily addressFamily, [In] IntPtr lpProtocolInfo, [Out] byte[] socketAddress, [In, Out] ref int socketAddressSize);

            [StructLayout(LayoutKind.Sequential)]
            internal struct ControlData
            {
                internal UIntPtr length;
                internal uint level;
                internal uint type;
                internal uint address;
                internal uint index;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct ControlDataIPv6
            {
                internal UIntPtr length;
                internal uint level;
                internal uint type;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
                internal byte[] address;
                internal uint index;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct SOCKET_ADDRESS
            {
                internal IntPtr lpSockAddr;
                internal int iSockaddrLength;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct SOCKET_ADDRESS_LIST
            {
                internal int iAddressCount;
                internal UnsafeNclNativeMethods.OSSOCK.SOCKET_ADDRESS Addresses;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct TransmitFileBuffersStruct
            {
                internal IntPtr preBuffer;
                internal int preBufferLength;
                internal IntPtr postBuffer;
                internal int postBufferLength;
            }

            [StructLayout(LayoutKind.Explicit)]
            internal struct TransmitPacketsElement
            {
                [FieldOffset(8)]
                internal IntPtr buffer;
                [FieldOffset(0x10)]
                internal IntPtr fileHandle;
                [FieldOffset(8)]
                internal long fileOffset;
                [FieldOffset(0)]
                internal UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags flags;
                [FieldOffset(4)]
                internal uint length;
            }

            [Flags]
            internal enum TransmitPacketsElementFlags : uint
            {
                EndOfPacket = 4,
                File = 2,
                Memory = 1,
                None = 0
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct WSAMsg
            {
                internal IntPtr socketAddress;
                internal uint addressLength;
                internal IntPtr buffers;
                internal uint count;
                internal WSABuffer controlBuffer;
                internal SocketFlags flags;
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
            internal struct WSAPROTOCOL_INFO
            {
                internal uint dwServiceFlags1;
                internal uint dwServiceFlags2;
                internal uint dwServiceFlags3;
                internal uint dwServiceFlags4;
                internal uint dwProviderFlags;
                private Guid ProviderId;
                internal uint dwCatalogEntryId;
                private UnsafeNclNativeMethods.OSSOCK.WSAPROTOCOLCHAIN ProtocolChain;
                internal int iVersion;
                internal AddressFamily iAddressFamily;
                internal int iMaxSockAddr;
                internal int iMinSockAddr;
                internal int iSocketType;
                internal int iProtocol;
                internal int iProtocolMaxOffset;
                internal int iNetworkByteOrder;
                internal int iSecurityScheme;
                internal uint dwMessageSize;
                internal uint dwProviderReserved;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x100)]
                internal string szProtocol;
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
            internal struct WSAPROTOCOLCHAIN
            {
                internal int ChainLen;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst=7)]
                internal uint[] ChainEntries;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        internal class RasHelper
        {
            private const int DNLEN = 15;
            private const uint ERROR_BUFFER_TOO_SMALL = 0x25b;
            private const uint ERROR_DIAL_ALREADY_IN_PROGRESS = 0x2f4;
            private ManualResetEvent m_RasEvent;
            private bool m_Suppressed;
            private const int MAX_PATH = 260;
            private const int PWLEN = 0x100;
            private const int RAS_MaxCallbackNumber = 0x80;
            private const int RAS_MaxDeviceName = 0x80;
            private const int RAS_MaxDeviceType = 0x10;
            private const int RAS_MaxEntryName = 0x100;
            private const int RAS_MaxPhoneNumber = 0x80;
            private const uint RASBASE = 600;
            private const uint RASCN_Connection = 1;
            private const uint RASCN_Disconnection = 2;
            private const int RASCS_DONE = 0x2000;
            private const int RASCS_PAUSED = 0x1000;
            private static bool s_RasSupported;
            private const int UNLEN = 0x100;

            static RasHelper()
            {
                InitRasSupported();
            }

            internal RasHelper()
            {
                if (!s_RasSupported)
                {
                    throw new InvalidOperationException(SR.GetString("net_log_proxy_ras_notsupported_exception"));
                }
                this.m_RasEvent = new ManualResetEvent(false);
                if (RasConnectionNotification((IntPtr) (-1), this.m_RasEvent.SafeWaitHandle, 3) != 0)
                {
                    this.m_Suppressed = true;
                    this.m_RasEvent.Close();
                    this.m_RasEvent = null;
                }
            }

            internal static string GetCurrentConnectoid()
            {
                uint num = (uint) Marshal.SizeOf(typeof(RASCONN));
                if (s_RasSupported)
                {
                    uint lpcConnections = 4;
                    uint num3 = 0;
                    RASCONN[] lprasconn = null;
                    while (true)
                    {
                        uint lpcb = num * lpcConnections;
                        lprasconn = new RASCONN[lpcConnections];
                        lprasconn[0].dwSize = num;
                        num3 = RasEnumConnections(lprasconn, ref lpcb, ref lpcConnections);
                        if (num3 != 0x25b)
                        {
                            break;
                        }
                        lpcConnections = ((lpcb + num) - 1) / num;
                    }
                    if ((lpcConnections != 0) && (num3 == 0))
                    {
                        for (uint i = 0; i < lpcConnections; i++)
                        {
                            RASCONNSTATUS rasconnstatus;
                            rasconnstatus = new RASCONNSTATUS {
                                dwSize = (uint) Marshal.SizeOf(rasconnstatus)
                            };
                            if ((RasGetConnectStatus(lprasconn[i].hrasconn, ref rasconnstatus) == 0) && (rasconnstatus.rasconnstate == RASCONNSTATE.RASCS_Connected))
                            {
                                return lprasconn[i].szEntryName;
                            }
                        }
                    }
                }
                return null;
            }

            private static void InitRasSupported()
            {
                if (ComNetOS.InstallationType == WindowsInstallationType.ServerCore)
                {
                    s_RasSupported = false;
                }
                else
                {
                    s_RasSupported = true;
                }
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.Web, SR.GetString("net_log_proxy_ras_supported", new object[] { s_RasSupported }));
                }
            }

            [DllImport("rasapi32.dll", CharSet=CharSet.Auto)]
            private static extern uint RasConnectionNotification([In] IntPtr hrasconn, [In] SafeWaitHandle hEvent, uint dwFlags);
            [DllImport("rasapi32.dll", CharSet=CharSet.Auto)]
            private static extern uint RasEnumConnections([In, Out] RASCONN[] lprasconn, ref uint lpcb, ref uint lpcConnections);
            [DllImport("rasapi32.dll", CharSet=CharSet.Auto)]
            private static extern uint RasGetConnectStatus([In] IntPtr hrasconn, [In, Out] ref RASCONNSTATUS lprasconnstatus);
            internal void Reset()
            {
                if (!this.m_Suppressed)
                {
                    ManualResetEvent rasEvent = this.m_RasEvent;
                    if (rasEvent == null)
                    {
                        throw new ObjectDisposedException(base.GetType().FullName);
                    }
                    rasEvent.Reset();
                }
            }

            internal bool HasChanged
            {
                get
                {
                    if (this.m_Suppressed)
                    {
                        return false;
                    }
                    ManualResetEvent rasEvent = this.m_RasEvent;
                    if (rasEvent == null)
                    {
                        throw new ObjectDisposedException(base.GetType().FullName);
                    }
                    return rasEvent.WaitOne(0, false);
                }
            }

            internal static bool RasSupported
            {
                get
                {
                    return s_RasSupported;
                }
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=4)]
            private struct RASCONN
            {
                internal uint dwSize;
                internal IntPtr hrasconn;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x101)]
                internal string szEntryName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x11)]
                internal string szDeviceType;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x81)]
                internal string szDeviceName;
            }

            private enum RASCONNSTATE
            {
                RASCS_AllDevicesConnected = 4,
                RASCS_AuthAck = 12,
                RASCS_AuthCallback = 8,
                RASCS_AuthChangePassword = 9,
                RASCS_Authenticate = 5,
                RASCS_Authenticated = 14,
                RASCS_AuthLinkSpeed = 11,
                RASCS_AuthNotify = 6,
                RASCS_AuthProject = 10,
                RASCS_AuthRetry = 7,
                RASCS_CallbackComplete = 20,
                RASCS_CallbackSetByCaller = 0x1002,
                RASCS_ConnectDevice = 2,
                RASCS_Connected = 0x2000,
                RASCS_DeviceConnected = 3,
                RASCS_Disconnected = 0x2001,
                RASCS_Interactive = 0x1000,
                RASCS_InvokeEapUI = 0x1004,
                RASCS_LogonNetwork = 0x15,
                RASCS_OpenPort = 0,
                RASCS_PasswordExpired = 0x1003,
                RASCS_PortOpened = 1,
                RASCS_PrepareForCallback = 15,
                RASCS_Projected = 0x12,
                RASCS_ReAuthenticate = 13,
                RASCS_RetryAuthentication = 0x1001,
                RASCS_StartAuthentication = 0x13,
                RASCS_SubEntryConnected = 0x16,
                RASCS_SubEntryDisconnected = 0x17,
                RASCS_WaitForCallback = 0x11,
                RASCS_WaitForModemReset = 0x10
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
            private struct RASCONNSTATUS
            {
                internal uint dwSize;
                internal UnsafeNclNativeMethods.RasHelper.RASCONNSTATE rasconnstate;
                internal uint dwError;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x11)]
                internal string szDeviceType;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x81)]
                internal string szDeviceName;
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
            private struct RASDIALPARAMS
            {
                internal uint dwSize;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x101)]
                internal string szEntryName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x81)]
                internal string szPhoneNumber;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x81)]
                internal string szCallbackNumber;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x101)]
                internal string szUserName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x101)]
                internal string szPassword;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x10)]
                internal string szDomain;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class RegistryHelper
        {
            internal static readonly IntPtr HKEY_CURRENT_USER = ((IntPtr) (-2147483647));
            internal static readonly IntPtr HKEY_LOCAL_MACHINE = ((IntPtr) (-2147483646));
            internal const uint KEY_READ = 0x20019;
            internal const uint REG_BINARY = 3;
            internal const uint REG_NOTIFY_CHANGE_LAST_SET = 4;

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern uint RegCloseKey(IntPtr key);
            [DllImport("advapi32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern uint RegNotifyChangeKeyValue(System.Net.SafeRegistryHandle key, bool watchSubTree, uint notifyFilter, SafeWaitHandle regEvent, bool async);
            [DllImport("advapi32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern uint RegOpenCurrentUser(uint samDesired, out System.Net.SafeRegistryHandle resultKey);
            [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
            internal static extern uint RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint samDesired, out System.Net.SafeRegistryHandle resultSubKey);
            [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
            internal static extern uint RegOpenKeyEx(System.Net.SafeRegistryHandle key, string subKey, uint ulOptions, uint samDesired, out System.Net.SafeRegistryHandle resultSubKey);
            [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
            internal static extern uint RegQueryValueEx(System.Net.SafeRegistryHandle key, string valueName, IntPtr reserved, out uint type, [Out] byte[] data, [In, Out] ref uint size);
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class SafeNetHandles
        {
            [DllImport("ws2_32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern SafeCloseSocket.InnerSafeCloseSocket accept([In] IntPtr socketHandle, [Out] byte[] socketAddress, [In, Out] ref int socketAddressSize);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("crypt32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern void CertFreeCertificateChain([In] IntPtr pChainContext);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("crypt32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern bool CertFreeCertificateContext([In] IntPtr certContext);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern bool CloseHandle(IntPtr handle);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("ws2_32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern SocketError closesocket([In] IntPtr socketHandle);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern bool FreeLibrary([In] IntPtr hModule);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern IntPtr GlobalFree(IntPtr handle);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
            internal static extern uint HttpCloseRequestQueue(IntPtr pReqQueueHandle);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true, ExactSpelling=true)]
            internal static extern uint HttpCreateHttpHandle(out SafeCloseHandle pReqQueueHandle, uint options);
            [DllImport("httpapi.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern uint HttpCreateRequestQueue(UnsafeNclNativeMethods.HttpApi.HTTPAPI_VERSION version, string pName, Microsoft.Win32.NativeMethods.SECURITY_ATTRIBUTES pSecurityAttributes, uint flags, out HttpRequestQueueV2Handle pReqQueueHandle);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("ws2_32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern SocketError ioctlsocket([In] IntPtr handle, [In] int cmd, [In, Out] ref int argp);
            [DllImport("kernel32.dll", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe SafeLoadLibrary LoadLibraryExA([In] string lpwLibFileName, [In] void* hFile, [In] uint dwFlags);
            [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe SafeLoadLibrary LoadLibraryExW([In] string lpwLibFileName, [In] void* hFile, [In] uint dwFlags);
            [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern SafeLocalFree LocalAlloc(int uFlags, UIntPtr sizetdwBytes);
            [DllImport("kernel32.dll", EntryPoint="LocalAlloc", SetLastError=true)]
            internal static extern SafeLocalFreeChannelBinding LocalAllocChannelBinding(int uFlags, UIntPtr sizetdwBytes);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern IntPtr LocalFree(IntPtr handle);
            [DllImport("security.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int QuerySecurityContextToken(ref SSPIHandle phContext, out SafeCloseHandle handle);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("wininet.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe bool RetrieveUrlCacheEntryFileW([In] char* urlName, [In] byte* entryPtr, [In, Out] ref int entryBufSize, [In] int dwReserved);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("ws2_32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern SocketError setsockopt([In] IntPtr handle, [In] SocketOptionLevel optionLevel, [In] SocketOptionName optionName, [In] ref Linger linger, [In] int optionLength);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("wininet.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe bool UnlockUrlCacheEntryFileW([In] char* urlName, [In] int dwReserved);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("ws2_32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern SocketError WSAEventSelect([In] IntPtr handle, [In] IntPtr Event, [In] AsyncEventBits NetworkEvents);
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class SafeNetHandles_SCHANNEL
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("schannel.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int AcceptSecurityContext(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] SecurityBufferDescriptor inputBuffer, [In] ContextFlags inFlags, [In] Endianness endianness, ref SSPIHandle outContextPtr, [In, Out] SecurityBufferDescriptor outputBuffer, [In, Out] ref ContextFlags attributes, out long timeStamp);
            [DllImport("schannel.dll", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int AcquireCredentialsHandleA([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] ref SecureCredential authData, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("schannel.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int DeleteSecurityContext(ref SSPIHandle handlePtr);
            [DllImport("schannel.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int EnumerateSecurityPackagesA(out int pkgnum, out SafeFreeContextBuffer_SCHANNEL handle);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("schannel.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int FreeContextBuffer([In] IntPtr contextBuffer);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("schannel.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int FreeCredentialsHandle(ref SSPIHandle handlePtr);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("schannel.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int InitializeSecurityContextA(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] byte* targetName, [In] ContextFlags inFlags, [In] int reservedI, [In] Endianness endianness, [In] SecurityBufferDescriptor inputBuffer, [In] int reservedII, ref SSPIHandle outContextPtr, [In, Out] SecurityBufferDescriptor outputBuffer, [In, Out] ref ContextFlags attributes, out long timeStamp);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("schannel.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int QueryContextAttributesA(ref SSPIHandle contextHandle, [In] ContextAttribute attribute, [In] void* buffer);
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class SafeNetHandles_SECUR32
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("secur32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int AcceptSecurityContext(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] SecurityBufferDescriptor inputBuffer, [In] ContextFlags inFlags, [In] Endianness endianness, ref SSPIHandle outContextPtr, [In, Out] SecurityBufferDescriptor outputBuffer, [In, Out] ref ContextFlags attributes, out long timeStamp);
            [DllImport("secur32.dll", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int AcquireCredentialsHandleA([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] ref AuthIdentity authdata, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);
            [DllImport("secur32.dll", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int AcquireCredentialsHandleA([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] IntPtr zero, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("secur32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int DeleteSecurityContext(ref SSPIHandle handlePtr);
            [DllImport("secur32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int EnumerateSecurityPackagesA(out int pkgnum, out SafeFreeContextBuffer_SECUR32 handle);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("secur32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int FreeContextBuffer([In] IntPtr contextBuffer);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("secur32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int FreeCredentialsHandle(ref SSPIHandle handlePtr);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("secur32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int InitializeSecurityContextA(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] byte* targetName, [In] ContextFlags inFlags, [In] int reservedI, [In] Endianness endianness, [In] SecurityBufferDescriptor inputBuffer, [In] int reservedII, ref SSPIHandle outContextPtr, [In, Out] SecurityBufferDescriptor outputBuffer, [In, Out] ref ContextFlags attributes, out long timeStamp);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("secur32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int QueryContextAttributesA(ref SSPIHandle contextHandle, [In] ContextAttribute attribute, [In] void* buffer);
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class SafeNetHandles_SECURITY
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("security.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int AcceptSecurityContext(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] SecurityBufferDescriptor inputBuffer, [In] ContextFlags inFlags, [In] Endianness endianness, ref SSPIHandle outContextPtr, [In, Out] SecurityBufferDescriptor outputBuffer, [In, Out] ref ContextFlags attributes, out long timeStamp);
            [DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] ref AuthIdentity authdata, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);
            [DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] IntPtr zero, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);
            [DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int AcquireCredentialsHandleW([In] string principal, [In] string moduleName, [In] int usage, [In] void* logonID, [In] ref SecureCredential authData, [In] void* keyCallback, [In] void* keyArgument, ref SSPIHandle handlePtr, out long timeStamp);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("security.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int CompleteAuthToken([In] void* inContextPtr, [In, Out] SecurityBufferDescriptor inputBuffers);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("security.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int DeleteSecurityContext(ref SSPIHandle handlePtr);
            [DllImport("security.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int EnumerateSecurityPackagesW(out int pkgnum, out SafeFreeContextBuffer_SECURITY handle);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("security.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int FreeContextBuffer([In] IntPtr contextBuffer);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("security.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern int FreeCredentialsHandle(ref SSPIHandle handlePtr);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("security.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int InitializeSecurityContextW(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] byte* targetName, [In] ContextFlags inFlags, [In] int reservedI, [In] Endianness endianness, [In] SecurityBufferDescriptor inputBuffer, [In] int reservedII, ref SSPIHandle outContextPtr, [In, Out] SecurityBufferDescriptor outputBuffer, [In, Out] ref ContextFlags attributes, out long timeStamp);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("security.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe int QueryContextAttributesW(ref SSPIHandle contextHandle, [In] ContextAttribute attribute, [In] void* buffer);
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class SafeNetHandlesSafeOverlappedFree
        {
            [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern SafeOverlappedFree LocalAlloc(int uFlags, UIntPtr sizetdwBytes);
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class SafeNetHandlesXPOrLater
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("ws2_32.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern void freeaddrinfo([In] IntPtr info);
            [DllImport("ws2_32.dll", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
            internal static extern int getaddrinfo([In] string nodename, [In] string servicename, [In] ref AddressInfo hints, out SafeFreeAddrInfo handle);
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class SecureStringHelper
        {
            internal static unsafe SecureString CreateSecureString(string plainString)
            {
                SecureString str;
                if ((plainString == null) || (plainString.Length == 0))
                {
                    return new SecureString();
                }
                fixed (char* str2 = ((char*) plainString))
                {
                    char* chPtr = str2;
                    str = new SecureString(chPtr, plainString.Length);
                }
                return str;
            }

            internal static string CreateString(SecureString secureString)
            {
                string str;
                IntPtr zero = IntPtr.Zero;
                if ((secureString == null) || (secureString.Length == 0))
                {
                    return string.Empty;
                }
                try
                {
                    zero = Marshal.SecureStringToBSTR(secureString);
                    str = Marshal.PtrToStringBSTR(zero);
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        Marshal.ZeroFreeBSTR(zero);
                    }
                }
                return str;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class UnsafeWinInetCache
        {
            public const int MAX_PATH = 260;

            [DllImport("wininet.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe bool CommitUrlCacheEntryW([In] string urlName, [In] string localFileName, [In] _WinInetCache.FILETIME expireTime, [In] _WinInetCache.FILETIME lastModifiedTime, [In] _WinInetCache.EntryType EntryType, [In] byte* headerInfo, [In] int headerSizeTChars, [In] string fileExtension, [In] string originalUrl);
            [DllImport("wininet.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern bool CreateUrlCacheEntryW([In] string urlName, [In] int expectedFileSize, [In] string fileExtension, [Out] StringBuilder fileName, [In] int dwReserved);
            [DllImport("wininet.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern bool DeleteUrlCacheEntryW([In] string urlName);
            [DllImport("wininet.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe bool GetUrlCacheEntryInfoW([In] string urlName, [In] byte* entryPtr, [In, Out] ref int bufferSz);
            [DllImport("wininet.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern unsafe bool SetUrlCacheEntryInfoW([In] string lpszUrlName, [In] byte* EntryPtr, [In] _WinInetCache.Entry_FC fieldControl);
            [DllImport("wininet.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
            internal static extern bool UnlockUrlCacheEntryFileW([In] string urlName, [In] int dwReserved);
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class WinHttp
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("winhttp.dll", CharSet=CharSet.Unicode, SetLastError=true)]
            internal static extern bool WinHttpCloseHandle(IntPtr httpSession);
            [DllImport("winhttp.dll", SetLastError=true, ExactSpelling=true)]
            internal static extern bool WinHttpDetectAutoProxyConfigUrl(AutoDetectType autoDetectFlags, out SafeGlobalFree autoConfigUrl);
            [DllImport("winhttp.dll", SetLastError=true)]
            internal static extern bool WinHttpGetIEProxyConfigForCurrentUser(ref WINHTTP_CURRENT_USER_IE_PROXY_CONFIG proxyConfig);
            [DllImport("winhttp.dll", CharSet=CharSet.Unicode, SetLastError=true)]
            internal static extern bool WinHttpGetProxyForUrl(SafeInternetHandle session, string url, [In] ref WINHTTP_AUTOPROXY_OPTIONS autoProxyOptions, out WINHTTP_PROXY_INFO proxyInfo);
            [DllImport("winhttp.dll", CharSet=CharSet.Unicode, SetLastError=true)]
            internal static extern SafeInternetHandle WinHttpOpen(string userAgent, AccessType accessType, string proxyName, string proxyBypass, int dwFlags);
            [DllImport("winhttp.dll", CharSet=CharSet.Unicode, SetLastError=true)]
            internal static extern bool WinHttpSetTimeouts(SafeInternetHandle session, int resolveTimeout, int connectTimeout, int sendTimeout, int receiveTimeout);

            internal enum AccessType
            {
                DefaultProxy = 0,
                NamedProxy = 3,
                NoProxy = 1
            }

            [Flags]
            internal enum AutoDetectType
            {
                None,
                Dhcp,
                DnsA
            }

            [Flags]
            internal enum AutoProxyFlags
            {
                AutoDetect = 1,
                AutoProxyConfigUrl = 2,
                RunInProcess = 0x10000,
                RunOutProcessOnly = 0x20000
            }

            internal enum ErrorCodes
            {
                AudodetectionFailed = 0x2f94,
                AuthCertNeeded = 0x2f0c,
                AutoProxyServiceError = 0x2f92,
                BadAutoProxyScript = 0x2f86,
                CannotCallAfterOpen = 0x2f47,
                CannotCallAfterSend = 0x2f46,
                CannotCallBeforeOpen = 0x2f44,
                CannotCallBeforeSend = 0x2f45,
                CannotConnect = 0x2efd,
                ChunkedEncodingHeaderSizeOverflow = 0x2f97,
                ClientCertNoAccessPrivateKey = 0x2f9a,
                ClientCertNoPrivateKey = 0x2f99,
                ConnectionError = 0x2efe,
                HeaderAlreadyExists = 0x2f7b,
                HeaderCountExceeded = 0x2f95,
                HeaderNotFound = 0x2f76,
                HeaderSizeOverflow = 0x2f96,
                IncorrectHandleState = 0x2ef3,
                IncorrectHandleType = 0x2ef2,
                InternalError = 0x2ee4,
                InvalidHeader = 0x2f79,
                InvalidOption = 0x2ee9,
                InvalidQueryRequest = 0x2f7a,
                InvalidServerResponse = 0x2f78,
                InvalidUrl = 0x2ee5,
                LoginFailure = 0x2eef,
                NameNotResolved = 0x2ee7,
                NotInitialized = 0x2f8c,
                OperationCancelled = 0x2ef1,
                OptionNotSettable = 0x2eeb,
                OutOfHandles = 0x2ee1,
                RedirectFailed = 0x2f7c,
                ResendRequest = 0x2f00,
                ResponseDrainOverflow = 0x2f98,
                SecureCertCNInvalid = 0x2f06,
                SecureCertDateInvalid = 0x2f05,
                SecureCertRevFailed = 0x2f19,
                SecureCertRevoked = 0x2f8a,
                SecureCertWrongUsage = 0x2f93,
                SecureChannelError = 0x2f7d,
                SecureFailure = 0x2f8f,
                SecureInvalidCA = 0x2f0d,
                SecureInvalidCert = 0x2f89,
                Shutdown = 0x2eec,
                Success = 0,
                Timeout = 0x2ee2,
                UnableToDownloadScript = 0x2f87,
                UnrecognizedScheme = 0x2ee6
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
            internal struct WINHTTP_AUTOPROXY_OPTIONS
            {
                public UnsafeNclNativeMethods.WinHttp.AutoProxyFlags Flags;
                public UnsafeNclNativeMethods.WinHttp.AutoDetectType AutoDetectFlags;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string AutoConfigUrl;
                private IntPtr lpvReserved;
                private int dwReserved;
                public bool AutoLogonIfChallenged;
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
            internal struct WINHTTP_CURRENT_USER_IE_PROXY_CONFIG
            {
                public bool AutoDetect;
                public IntPtr AutoConfigUrl;
                public IntPtr Proxy;
                public IntPtr ProxyBypass;
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
            internal struct WINHTTP_PROXY_INFO
            {
                public System.Net.UnsafeNclNativeMethods.WinHttp.AccessType AccessType;
                public IntPtr Proxy;
                public IntPtr ProxyBypass;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class WinInet
        {
            [DllImport("wininet.dll", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
            internal static extern bool DetectAutoProxyUrl([Out] StringBuilder autoProxyUrl, [In] int autoProxyUrlLength, [In] int detectFlags);
        }
    }
}

